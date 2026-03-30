using System.Buffers.Binary;
using Maple.Memory;
using Maple.Native;
using Maple.Process;

namespace Maple.Client.V95.Test;

public sealed class LoginStateMonitorTests
{
    [Test]
    public async Task LoginStateMonitor_WaitForStepAsync_ReturnsWhenTargetStepAppearsOnNextPoll()
    {
        using var processMemory = new FakeRemoteProcessMemory();
        using var memoryAccessor = new MemoryAccessor(processMemory);
        var resolver = new LoginStateResolver(memoryAccessor);
        var sink = new RecordingLoginMonitorSink();
        const uint cLogin = 0xA100_0000u;
        const uint cUiWorldSelect = 0xA200_0000u;
        int delayCalls = 0;
        var monitor = new LoginStateMonitor(
            resolver,
            sink,
            (_, _) =>
            {
                delayCalls++;
                if (delayCalls == 1)
                {
                    processMemory.WriteByte(
                        cLogin + ClientStructs.Offsets.CLogin.LoginStep,
                        (byte)LoginStep.SelectWorld
                    );
                }

                return Task.CompletedTask;
            }
        );

        processMemory.WriteUInt32(ClientStructs.Addresses.CUIWorldSelectSingletonPtr, cUiWorldSelect);
        processMemory.WriteUInt32(cUiWorldSelect + ClientStructs.Offsets.CUIWorldSelect.Login, cLogin);
        processMemory.WriteUInt32(cUiWorldSelect + ClientStructs.Offsets.CUIWorldSelect.World, 0);
        processMemory.WriteByte(cLogin + ClientStructs.Offsets.CLogin.LoginStep, (byte)LoginStep.Title);
        processMemory.WriteInt32(cLogin + ClientStructs.Offsets.CLogin.StepChanging, 0);
        processMemory.WriteInt32(cLogin + ClientStructs.Offsets.CLogin.RequestSent, 0);
        processMemory.WriteUInt32(cLogin + ClientStructs.Offsets.CLogin.WorldItem, 0);

        RawLoginState state = await monitor.WaitForStepAsync(
            LoginStep.SelectWorld,
            TimeSpan.FromMilliseconds(25),
            TimeSpan.Zero
        );

        await Assert.That(state.Step).IsEqualTo(LoginStep.SelectWorld);
        await Assert.That(delayCalls).IsEqualTo(1);
        await Assert.That(sink.Events.Count).IsEqualTo(4);
        await Assert.That(sink.Events[0].Kind).IsEqualTo(LoginMonitorEventKind.Started);
        await Assert.That(sink.Events[1].Kind).IsEqualTo(LoginMonitorEventKind.Polling);
        await Assert.That(sink.Events[1].Context.StateResolved).IsTrue();
        await Assert.That(sink.Events[1].Context.RawState!.Value.Step).IsEqualTo(LoginStep.Title);
        await Assert.That(sink.Events[3].Kind).IsEqualTo(LoginMonitorEventKind.Matched);
        await Assert.That(sink.Events[3].Context.AttemptCount).IsEqualTo(2);
        await Assert.That(sink.Events[3].Context.RawState!.Value.Step).IsEqualTo(LoginStep.SelectWorld);
    }

    [Test]
    public async Task LoginStateMonitor_WaitForStableStepAsync_WhenStepNeverStabilizes_ThrowsTimeoutException()
    {
        using var processMemory = new FakeRemoteProcessMemory();
        using var memoryAccessor = new MemoryAccessor(processMemory);
        var resolver = new LoginStateResolver(memoryAccessor);
        var sink = new RecordingLoginMonitorSink();
        var monitor = new LoginStateMonitor(resolver, sink, static (_, _) => Task.CompletedTask);
        const uint cLogin = 0xA300_0000u;
        const uint cUiWorldSelect = 0xA400_0000u;

        processMemory.WriteUInt32(ClientStructs.Addresses.CUIWorldSelectSingletonPtr, cUiWorldSelect);
        processMemory.WriteUInt32(cUiWorldSelect + ClientStructs.Offsets.CUIWorldSelect.Login, cLogin);
        processMemory.WriteUInt32(cUiWorldSelect + ClientStructs.Offsets.CUIWorldSelect.World, 0);
        processMemory.WriteByte(cLogin + ClientStructs.Offsets.CLogin.LoginStep, (byte)LoginStep.SelectWorld);
        processMemory.WriteInt32(cLogin + ClientStructs.Offsets.CLogin.StepChanging, 1);
        processMemory.WriteInt32(cLogin + ClientStructs.Offsets.CLogin.RequestSent, 0);
        processMemory.WriteUInt32(cLogin + ClientStructs.Offsets.CLogin.WorldItem, 0);

        await Assert
            .That(() =>
                monitor.WaitForStableStepAsync(LoginStep.SelectWorld, TimeSpan.FromMilliseconds(2), TimeSpan.Zero)
            )
            .Throws<TimeoutException>();

        await Assert.That(sink.Events.Count).IsGreaterThanOrEqualTo(3);
        await Assert.That(sink.Events[^1].Kind).IsEqualTo(LoginMonitorEventKind.TimedOut);
        await Assert.That(sink.Events[^1].Context.StateResolved).IsTrue();
        await Assert.That(sink.Events[^1].Context.RawState!.Value.StepChanging).IsTrue();
    }

    [Test]
    public async Task LoginStateMonitor_PublicSinkConstructor_EmitsStartedPollingAndMatchedEvents()
    {
        using var processMemory = new FakeRemoteProcessMemory();
        using var memoryAccessor = new MemoryAccessor(processMemory);
        var resolver = new LoginStateResolver(memoryAccessor);
        var sink = new RecordingLoginMonitorSink();
        var monitor = new LoginStateMonitor(resolver, sink);
        const uint cLogin = 0xA500_0000u;
        const uint cUiWorldSelect = 0xA600_0000u;

        processMemory.WriteUInt32(ClientStructs.Addresses.CUIWorldSelectSingletonPtr, cUiWorldSelect);
        processMemory.WriteUInt32(cUiWorldSelect + ClientStructs.Offsets.CUIWorldSelect.Login, cLogin);
        processMemory.WriteUInt32(cUiWorldSelect + ClientStructs.Offsets.CUIWorldSelect.World, 0);
        processMemory.WriteByte(cLogin + ClientStructs.Offsets.CLogin.LoginStep, (byte)LoginStep.SelectWorld);
        processMemory.WriteInt32(cLogin + ClientStructs.Offsets.CLogin.StepChanging, 0);
        processMemory.WriteInt32(cLogin + ClientStructs.Offsets.CLogin.RequestSent, 0);
        processMemory.WriteUInt32(cLogin + ClientStructs.Offsets.CLogin.WorldItem, 0);

        RawLoginState state = await monitor.WaitForStepAsync(
            LoginStep.SelectWorld,
            TimeSpan.FromMilliseconds(25),
            TimeSpan.Zero
        );

        await Assert.That(state.Step).IsEqualTo(LoginStep.SelectWorld);
        await Assert.That(sink.Events.Count).IsEqualTo(3);
        await Assert.That(sink.Events[0].Kind).IsEqualTo(LoginMonitorEventKind.Started);
        await Assert.That(sink.Events[1].Kind).IsEqualTo(LoginMonitorEventKind.Polling);
        await Assert.That(sink.Events[2].Kind).IsEqualTo(LoginMonitorEventKind.Matched);
    }

    private sealed class RecordingLoginMonitorSink : ILoginMonitorSink
    {
        public List<LoginMonitorEvent> Events { get; } = [];

        public ValueTask OnEventAsync(LoginMonitorEvent monitorEvent, CancellationToken cancellationToken)
        {
            Events.Add(monitorEvent);
            return ValueTask.CompletedTask;
        }
    }

    private sealed class FakeRemoteProcessMemory : IRemoteProcessMemory
    {
        private readonly Dictionary<uint, byte[]> _values = [];

        public uint Allocate(int size) => throw new NotSupportedException();

        public bool Read(uint address, Span<byte> destination)
        {
            foreach ((uint baseAddress, byte[] bytes) in _values)
            {
                uint endAddress = baseAddress + (uint)bytes.Length;
                uint requestedEnd = address + (uint)destination.Length;
                if (address < baseAddress || requestedEnd > endAddress)
                    continue;

                bytes.AsSpan((int)(address - baseAddress), destination.Length).CopyTo(destination);
                return true;
            }

            return false;
        }

        public bool Write(uint address, ReadOnlySpan<byte> data)
        {
            _values[address] = data.ToArray();
            return true;
        }

        public void Free(uint address) => _values.Remove(address);

        public void Dispose() { }

        public void WriteByte(uint address, byte value) => _values[address] = [value];

        public void WriteInt32(uint address, int value)
        {
            byte[] bytes = new byte[sizeof(int)];
            BinaryPrimitives.WriteInt32LittleEndian(bytes, value);
            _values[address] = bytes;
        }

        public void WriteUInt32(uint address, uint value)
        {
            byte[] bytes = new byte[TypeSizes.UInt32];
            BinaryPrimitives.WriteUInt32LittleEndian(bytes, value);
            _values[address] = bytes;
        }
    }
}
