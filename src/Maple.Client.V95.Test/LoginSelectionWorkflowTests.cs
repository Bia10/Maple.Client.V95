using System.Buffers.Binary;
using Maple.Memory;
using Maple.Native;
using Maple.Process;

namespace Maple.Client.V95.Test;

public sealed class LoginSelectionWorkflowTests
{
    [Test]
    public async Task LoginSelectionWorkflow_SelectWorldAndChannel_ReturnsUpdatedLoginSnapshot()
    {
        using var processMemory = new FakeRemoteProcessMemory();
        using var memoryAccessor = new MemoryAccessor(processMemory);
        var workflow = new LoginSelectionWorkflow(memoryAccessor);
        const uint cLogin = 0x9100_0000u;
        const uint cUiWorldSelect = 0x9200_0000u;
        const uint cUiChannelSelect = 0x9300_0000u;
        const uint worldItemAddress = 0x9400_0000u;
        const uint channelItemsBase = 0x9410_0000u;
        uint channelItemAddress = FieldAccessor.GetChannelItemAddress(channelItemsBase, 1);

        processMemory.WriteUInt32(ClientStructs.Addresses.CUIWorldSelectSingletonPtr, cUiWorldSelect);
        processMemory.WriteUInt32(ClientStructs.Addresses.CUIChannelSelectSingletonPtr, cUiChannelSelect);

        processMemory.WriteUInt32(cUiWorldSelect + ClientStructs.Offsets.CUIWorldSelect.Login, cLogin);
        processMemory.WriteUInt32(cUiChannelSelect + ClientStructs.Offsets.CUIChannelSelect.Login, cLogin);
        processMemory.WriteUInt32(cUiWorldSelect + ClientStructs.Offsets.CUIWorldSelect.World, 0);
        processMemory.WriteUInt32(cUiChannelSelect + ClientStructs.Offsets.CUIChannelSelect.WorldItem, 0);

        processMemory.WriteByte(cLogin + ClientStructs.Offsets.CLogin.LoginStep, (byte)LoginStep.SelectWorld);
        processMemory.WriteInt32(cLogin + ClientStructs.Offsets.CLogin.StepChanging, 0);
        processMemory.WriteInt32(cLogin + ClientStructs.Offsets.CLogin.RequestSent, 0);
        processMemory.WriteUInt32(cLogin + ClientStructs.Offsets.CLogin.WorldItem, 0);

        processMemory.WriteInt32(worldItemAddress + ClientStructs.Offsets.WorldItem.Id, 8);
        processMemory.WriteUInt32(worldItemAddress + ClientStructs.Offsets.WorldItem.NamePtr, 0x9420_0000u);
        processMemory.WriteZXStringPayload(0x9420_0000u, "Windia");
        processMemory.WriteUInt32(worldItemAddress + ClientStructs.Offsets.WorldItem.ChannelItemsPtr, channelItemsBase);

        processMemory.WriteUInt32(channelItemAddress + ClientStructs.Offsets.ChannelItem.NamePtr, 0x9430_0000u);
        processMemory.WriteZXStringPayload(0x9430_0000u, "Channel 2");
        processMemory.WriteInt32(channelItemAddress + ClientStructs.Offsets.ChannelItem.WorldId, 8);
        processMemory.WriteInt32(channelItemAddress + ClientStructs.Offsets.ChannelItem.ChannelId, 2);
        processMemory.WriteInt32(channelItemAddress + ClientStructs.Offsets.ChannelItem.AdultFlag, 1);

        ResolvedLoginState state = workflow.SelectWorldAndChannel(worldItemAddress, 6, 1);

        await Assert.That(state.Step).IsEqualTo(LoginStep.SelectWorld);
        await Assert.That(state.StepChanging).IsFalse();
        await Assert.That(state.RequestSent).IsFalse();
        await Assert.That(state.SelectedWorldItemAddress).IsEqualTo(worldItemAddress);
        await Assert.That(state.WorldSelection).IsNotNull();
        await Assert.That(state.ChannelSelection).IsNotNull();

        ResolvedWorldSelection worldSelection = state.WorldSelection ?? throw new InvalidOperationException();
        ResolvedChannelSelection channelSelection = state.ChannelSelection ?? throw new InvalidOperationException();

        await Assert.That(worldSelection.WorldIndex).IsEqualTo(6);
        await Assert.That(worldSelection.WorldItem.Name).IsEqualTo("Windia");
        await Assert.That(channelSelection.ChannelIndex).IsEqualTo(1);
        await Assert.That(channelSelection.ChannelItem.Name).IsEqualTo("Channel 2");
        await Assert.That(channelSelection.ChannelItem.AdultFlag).IsTrue();
    }

    [Test]
    public async Task LoginSelectionWorkflow_TrySelectWorldAndChannelRaw_ReturnsUpdatedRawLoginSnapshot()
    {
        using var processMemory = new FakeRemoteProcessMemory();
        using var memoryAccessor = new MemoryAccessor(processMemory);
        var workflow = new LoginSelectionWorkflow(memoryAccessor);
        const uint cLogin = 0x9110_0000u;
        const uint cUiWorldSelect = 0x9210_0000u;
        const uint cUiChannelSelect = 0x9310_0000u;
        const uint worldItemAddress = 0x9411_0000u;
        const uint channelItemsBase = 0x9412_0000u;
        uint channelItemAddress = FieldAccessor.GetChannelItemAddress(channelItemsBase, 2);

        processMemory.WriteUInt32(ClientStructs.Addresses.CUIWorldSelectSingletonPtr, cUiWorldSelect);
        processMemory.WriteUInt32(ClientStructs.Addresses.CUIChannelSelectSingletonPtr, cUiChannelSelect);

        processMemory.WriteUInt32(cUiWorldSelect + ClientStructs.Offsets.CUIWorldSelect.Login, cLogin);
        processMemory.WriteUInt32(cUiChannelSelect + ClientStructs.Offsets.CUIChannelSelect.Login, cLogin);
        processMemory.WriteUInt32(cUiWorldSelect + ClientStructs.Offsets.CUIWorldSelect.World, 0);
        processMemory.WriteUInt32(cUiChannelSelect + ClientStructs.Offsets.CUIChannelSelect.WorldItem, 0);

        processMemory.WriteByte(cLogin + ClientStructs.Offsets.CLogin.LoginStep, (byte)LoginStep.SelectWorld);
        processMemory.WriteInt32(cLogin + ClientStructs.Offsets.CLogin.StepChanging, 0);
        processMemory.WriteInt32(cLogin + ClientStructs.Offsets.CLogin.RequestSent, 0);
        processMemory.WriteUInt32(cLogin + ClientStructs.Offsets.CLogin.WorldItem, 0);

        processMemory.WriteInt32(worldItemAddress + ClientStructs.Offsets.WorldItem.Id, 13);
        processMemory.WriteUInt32(worldItemAddress + ClientStructs.Offsets.WorldItem.NamePtr, 0x9421_0000u);
        processMemory.WriteUInt32(worldItemAddress + ClientStructs.Offsets.WorldItem.ChannelItemsPtr, channelItemsBase);

        processMemory.WriteUInt32(channelItemAddress + ClientStructs.Offsets.ChannelItem.NamePtr, 0x9431_0000u);
        processMemory.WriteInt32(channelItemAddress + ClientStructs.Offsets.ChannelItem.WorldId, 13);
        processMemory.WriteInt32(channelItemAddress + ClientStructs.Offsets.ChannelItem.ChannelId, 3);
        processMemory.WriteInt32(channelItemAddress + ClientStructs.Offsets.ChannelItem.AdultFlag, 1);

        bool selected = workflow.TrySelectWorldAndChannelRaw(
            worldItemAddress,
            4,
            2,
            out RawLoginState state,
            out LoginSelectionFailure failure
        );

        await Assert.That(selected).IsTrue();
        await Assert.That(failure).IsEqualTo(LoginSelectionFailure.None);
        await Assert.That(state.Step).IsEqualTo(LoginStep.SelectWorld);
        await Assert.That(state.SelectedWorldItemAddress).IsEqualTo(worldItemAddress);
        await Assert.That(state.WorldSelection).IsNotNull();
        await Assert.That(state.ChannelSelection).IsNotNull();

        RawWorldSelection worldSelection = state.WorldSelection ?? throw new InvalidOperationException();
        RawChannelSelection channelSelection = state.ChannelSelection ?? throw new InvalidOperationException();

        await Assert.That(worldSelection.WorldIndex).IsEqualTo(4);
        await Assert.That(worldSelection.WorldItem.NamePointer).IsEqualTo(0x9421_0000u);
        await Assert.That(channelSelection.ChannelIndex).IsEqualTo(2);
        await Assert.That(channelSelection.ChannelItem.NamePointer).IsEqualTo(0x9431_0000u);
        await Assert.That(channelSelection.ChannelItem.ChannelId).IsEqualTo(3);
        await Assert.That(channelSelection.ChannelItem.AdultFlag).IsTrue();
    }

    [Test]
    public async Task LoginSelectionWorkflow_SelectWorld_WhenRequestWasAlreadySent_Throws()
    {
        using var processMemory = new FakeRemoteProcessMemory();
        using var memoryAccessor = new MemoryAccessor(processMemory);
        var workflow = new LoginSelectionWorkflow(memoryAccessor);
        const uint cLogin = 0x9500_0000u;
        const uint cUiWorldSelect = 0x9600_0000u;

        processMemory.WriteUInt32(ClientStructs.Addresses.CUIWorldSelectSingletonPtr, cUiWorldSelect);
        processMemory.WriteUInt32(cUiWorldSelect + ClientStructs.Offsets.CUIWorldSelect.Login, cLogin);
        processMemory.WriteUInt32(cUiWorldSelect + ClientStructs.Offsets.CUIWorldSelect.World, 0);
        processMemory.WriteByte(cLogin + ClientStructs.Offsets.CLogin.LoginStep, (byte)LoginStep.SelectWorld);
        processMemory.WriteInt32(cLogin + ClientStructs.Offsets.CLogin.StepChanging, 0);
        processMemory.WriteInt32(cLogin + ClientStructs.Offsets.CLogin.RequestSent, 1);
        processMemory.WriteUInt32(cLogin + ClientStructs.Offsets.CLogin.WorldItem, 0);

        await Assert.That(() => workflow.SelectWorld(0x9700_0000u, 3)).Throws<InvalidOperationException>();
    }

    [Test]
    public async Task LoginSelectionWorkflow_TrySelectWorld_WhenRequestWasAlreadySent_ReturnsFailureWithoutThrowing()
    {
        using var processMemory = new FakeRemoteProcessMemory();
        using var memoryAccessor = new MemoryAccessor(processMemory);
        var workflow = new LoginSelectionWorkflow(memoryAccessor);
        const uint cLogin = 0x9750_0000u;
        const uint cUiWorldSelect = 0x9760_0000u;

        processMemory.WriteUInt32(ClientStructs.Addresses.CUIWorldSelectSingletonPtr, cUiWorldSelect);
        processMemory.WriteUInt32(cUiWorldSelect + ClientStructs.Offsets.CUIWorldSelect.Login, cLogin);
        processMemory.WriteUInt32(cUiWorldSelect + ClientStructs.Offsets.CUIWorldSelect.World, 0);
        processMemory.WriteByte(cLogin + ClientStructs.Offsets.CLogin.LoginStep, (byte)LoginStep.SelectWorld);
        processMemory.WriteInt32(cLogin + ClientStructs.Offsets.CLogin.StepChanging, 0);
        processMemory.WriteInt32(cLogin + ClientStructs.Offsets.CLogin.RequestSent, 1);
        processMemory.WriteUInt32(cLogin + ClientStructs.Offsets.CLogin.WorldItem, 0);

        bool selected = workflow.TrySelectWorld(
            0x9770_0000u,
            3,
            out ResolvedLoginState state,
            out LoginSelectionFailure failure
        );

        await Assert.That(selected).IsFalse();
        await Assert.That(failure).IsEqualTo(LoginSelectionFailure.RequestSent);
        await Assert.That(state.IsResolved).IsTrue();
    }

    [Test]
    public async Task LoginSelectionWorkflow_SelectChannel_WhenSelectedWorldIsMissing_Throws()
    {
        using var processMemory = new FakeRemoteProcessMemory();
        using var memoryAccessor = new MemoryAccessor(processMemory);
        var workflow = new LoginSelectionWorkflow(memoryAccessor);
        const uint cLogin = 0x9800_0000u;
        const uint cUiWorldSelect = 0x9900_0000u;
        const uint cUiChannelSelect = 0x9A00_0000u;

        processMemory.WriteUInt32(ClientStructs.Addresses.CUIWorldSelectSingletonPtr, cUiWorldSelect);
        processMemory.WriteUInt32(ClientStructs.Addresses.CUIChannelSelectSingletonPtr, cUiChannelSelect);
        processMemory.WriteUInt32(cUiWorldSelect + ClientStructs.Offsets.CUIWorldSelect.Login, cLogin);
        processMemory.WriteUInt32(cUiChannelSelect + ClientStructs.Offsets.CUIChannelSelect.Login, cLogin);
        processMemory.WriteUInt32(cUiWorldSelect + ClientStructs.Offsets.CUIWorldSelect.World, 0);
        processMemory.WriteUInt32(cUiChannelSelect + ClientStructs.Offsets.CUIChannelSelect.WorldItem, 0);
        processMemory.WriteByte(cLogin + ClientStructs.Offsets.CLogin.LoginStep, (byte)LoginStep.SelectWorld);
        processMemory.WriteInt32(cLogin + ClientStructs.Offsets.CLogin.StepChanging, 0);
        processMemory.WriteInt32(cLogin + ClientStructs.Offsets.CLogin.RequestSent, 0);
        processMemory.WriteUInt32(cLogin + ClientStructs.Offsets.CLogin.WorldItem, 0);

        await Assert.That(() => workflow.SelectChannel(1)).Throws<InvalidOperationException>();
    }

    [Test]
    public async Task LoginSelectionWorkflow_TrySelectChannel_WhenSelectedWorldIsMissing_ReturnsFailureWithoutThrowing()
    {
        using var processMemory = new FakeRemoteProcessMemory();
        using var memoryAccessor = new MemoryAccessor(processMemory);
        var workflow = new LoginSelectionWorkflow(memoryAccessor);
        const uint cLogin = 0x9B00_0000u;
        const uint cUiWorldSelect = 0x9C00_0000u;
        const uint cUiChannelSelect = 0x9D00_0000u;

        processMemory.WriteUInt32(ClientStructs.Addresses.CUIWorldSelectSingletonPtr, cUiWorldSelect);
        processMemory.WriteUInt32(ClientStructs.Addresses.CUIChannelSelectSingletonPtr, cUiChannelSelect);
        processMemory.WriteUInt32(cUiWorldSelect + ClientStructs.Offsets.CUIWorldSelect.Login, cLogin);
        processMemory.WriteUInt32(cUiChannelSelect + ClientStructs.Offsets.CUIChannelSelect.Login, cLogin);
        processMemory.WriteUInt32(cUiWorldSelect + ClientStructs.Offsets.CUIWorldSelect.World, 0);
        processMemory.WriteUInt32(cUiChannelSelect + ClientStructs.Offsets.CUIChannelSelect.WorldItem, 0);
        processMemory.WriteByte(cLogin + ClientStructs.Offsets.CLogin.LoginStep, (byte)LoginStep.SelectWorld);
        processMemory.WriteInt32(cLogin + ClientStructs.Offsets.CLogin.StepChanging, 0);
        processMemory.WriteInt32(cLogin + ClientStructs.Offsets.CLogin.RequestSent, 0);
        processMemory.WriteUInt32(cLogin + ClientStructs.Offsets.CLogin.WorldItem, 0);

        bool selected = workflow.TrySelectChannel(1, out ResolvedLoginState state, out LoginSelectionFailure failure);

        await Assert.That(selected).IsFalse();
        await Assert.That(failure).IsEqualTo(LoginSelectionFailure.MissingSelectedWorld);
        await Assert.That(state.IsResolved).IsTrue();
    }

    [Test]
    public async Task LoginSelectionWorkflow_TrySelectChannel_WhenTargetChannelIsUnreadable_ReturnsFailureWithoutMutation()
    {
        using var processMemory = new FakeRemoteProcessMemory();
        using var memoryAccessor = new MemoryAccessor(processMemory);
        var workflow = new LoginSelectionWorkflow(memoryAccessor);
        const uint cLogin = 0x9E00_0000u;
        const uint cUiWorldSelect = 0x9E10_0000u;
        const uint cUiChannelSelect = 0x9E20_0000u;
        const uint selectedWorldItemAddress = 0x9E30_0000u;
        const uint channelItemsBase = 0x9E40_0000u;

        processMemory.WriteUInt32(ClientStructs.Addresses.CUIWorldSelectSingletonPtr, cUiWorldSelect);
        processMemory.WriteUInt32(ClientStructs.Addresses.CUIChannelSelectSingletonPtr, cUiChannelSelect);
        processMemory.WriteUInt32(cUiWorldSelect + ClientStructs.Offsets.CUIWorldSelect.Login, cLogin);
        processMemory.WriteUInt32(cUiChannelSelect + ClientStructs.Offsets.CUIChannelSelect.Login, cLogin);
        processMemory.WriteInt32(cUiWorldSelect + ClientStructs.Offsets.CUIWorldSelect.WorldIdx, 2);
        processMemory.WriteUInt32(
            cUiWorldSelect + ClientStructs.Offsets.CUIWorldSelect.World,
            selectedWorldItemAddress
        );
        processMemory.WriteInt32(cUiChannelSelect + ClientStructs.Offsets.CUIChannelSelect.Select, 0);
        processMemory.WriteUInt32(
            cUiChannelSelect + ClientStructs.Offsets.CUIChannelSelect.WorldItem,
            selectedWorldItemAddress
        );
        processMemory.WriteByte(cLogin + ClientStructs.Offsets.CLogin.LoginStep, (byte)LoginStep.SelectWorld);
        processMemory.WriteInt32(cLogin + ClientStructs.Offsets.CLogin.StepChanging, 0);
        processMemory.WriteInt32(cLogin + ClientStructs.Offsets.CLogin.RequestSent, 0);
        processMemory.WriteUInt32(cLogin + ClientStructs.Offsets.CLogin.WorldItem, selectedWorldItemAddress);

        processMemory.WriteInt32(selectedWorldItemAddress + ClientStructs.Offsets.WorldItem.Id, 21);
        processMemory.WriteUInt32(selectedWorldItemAddress + ClientStructs.Offsets.WorldItem.NamePtr, 0x9E50_0000u);
        processMemory.WriteUInt32(
            selectedWorldItemAddress + ClientStructs.Offsets.WorldItem.ChannelItemsPtr,
            channelItemsBase
        );

        bool selected = workflow.TrySelectChannel(3, out ResolvedLoginState state, out LoginSelectionFailure failure);

        await Assert.That(selected).IsFalse();
        await Assert.That(failure).IsEqualTo(LoginSelectionFailure.SelectionTargetUnavailable);
        await Assert.That(state.IsResolved).IsTrue();
        await Assert.That(state.SelectedWorldItemAddress).IsEqualTo(selectedWorldItemAddress);
        await Assert
            .That(processMemory.ReadInt32(cUiChannelSelect + ClientStructs.Offsets.CUIChannelSelect.Select))
            .IsEqualTo(0);
        await Assert
            .That(processMemory.ReadUInt32(cUiChannelSelect + ClientStructs.Offsets.CUIChannelSelect.WorldItem))
            .IsEqualTo(selectedWorldItemAddress);
        await Assert
            .That(processMemory.ReadUInt32(cLogin + ClientStructs.Offsets.CLogin.WorldItem))
            .IsEqualTo(selectedWorldItemAddress);
    }

    [Test]
    public async Task LoginSelectionWorkflow_TrySelectWorldAndChannelRaw_WhenTargetChannelIsUnreadable_DoesNotPartiallyMutate()
    {
        using var processMemory = new FakeRemoteProcessMemory();
        using var memoryAccessor = new MemoryAccessor(processMemory);
        var workflow = new LoginSelectionWorkflow(memoryAccessor);
        const uint cLogin = 0x9F00_0000u;
        const uint cUiWorldSelect = 0x9F10_0000u;
        const uint cUiChannelSelect = 0x9F20_0000u;
        const uint originalWorldItemAddress = 0x9F30_0000u;
        const uint requestedWorldItemAddress = 0x9F40_0000u;
        const uint originalChannelItemsBase = 0x9F50_0000u;
        const uint requestedChannelItemsBase = 0x9F60_0000u;
        uint originalChannelItemAddress = FieldAccessor.GetChannelItemAddress(originalChannelItemsBase, 0);

        processMemory.WriteUInt32(ClientStructs.Addresses.CUIWorldSelectSingletonPtr, cUiWorldSelect);
        processMemory.WriteUInt32(ClientStructs.Addresses.CUIChannelSelectSingletonPtr, cUiChannelSelect);
        processMemory.WriteUInt32(cUiWorldSelect + ClientStructs.Offsets.CUIWorldSelect.Login, cLogin);
        processMemory.WriteUInt32(cUiChannelSelect + ClientStructs.Offsets.CUIChannelSelect.Login, cLogin);
        processMemory.WriteInt32(cUiWorldSelect + ClientStructs.Offsets.CUIWorldSelect.WorldIdx, 1);
        processMemory.WriteUInt32(
            cUiWorldSelect + ClientStructs.Offsets.CUIWorldSelect.World,
            originalWorldItemAddress
        );
        processMemory.WriteInt32(cUiChannelSelect + ClientStructs.Offsets.CUIChannelSelect.Select, 0);
        processMemory.WriteUInt32(
            cUiChannelSelect + ClientStructs.Offsets.CUIChannelSelect.WorldItem,
            originalWorldItemAddress
        );
        processMemory.WriteByte(cLogin + ClientStructs.Offsets.CLogin.LoginStep, (byte)LoginStep.SelectWorld);
        processMemory.WriteInt32(cLogin + ClientStructs.Offsets.CLogin.StepChanging, 0);
        processMemory.WriteInt32(cLogin + ClientStructs.Offsets.CLogin.RequestSent, 0);
        processMemory.WriteUInt32(cLogin + ClientStructs.Offsets.CLogin.WorldItem, originalWorldItemAddress);

        processMemory.WriteInt32(originalWorldItemAddress + ClientStructs.Offsets.WorldItem.Id, 30);
        processMemory.WriteUInt32(originalWorldItemAddress + ClientStructs.Offsets.WorldItem.NamePtr, 0x9F70_0000u);
        processMemory.WriteUInt32(
            originalWorldItemAddress + ClientStructs.Offsets.WorldItem.ChannelItemsPtr,
            originalChannelItemsBase
        );
        processMemory.WriteUInt32(originalChannelItemAddress + ClientStructs.Offsets.ChannelItem.NamePtr, 0x9F71_0000u);
        processMemory.WriteInt32(originalChannelItemAddress + ClientStructs.Offsets.ChannelItem.WorldId, 30);
        processMemory.WriteInt32(originalChannelItemAddress + ClientStructs.Offsets.ChannelItem.ChannelId, 1);
        processMemory.WriteInt32(originalChannelItemAddress + ClientStructs.Offsets.ChannelItem.AdultFlag, 0);

        processMemory.WriteInt32(requestedWorldItemAddress + ClientStructs.Offsets.WorldItem.Id, 31);
        processMemory.WriteUInt32(requestedWorldItemAddress + ClientStructs.Offsets.WorldItem.NamePtr, 0x9F72_0000u);
        processMemory.WriteUInt32(
            requestedWorldItemAddress + ClientStructs.Offsets.WorldItem.ChannelItemsPtr,
            requestedChannelItemsBase
        );

        bool selected = workflow.TrySelectWorldAndChannelRaw(
            requestedWorldItemAddress,
            4,
            2,
            out RawLoginState state,
            out LoginSelectionFailure failure
        );

        await Assert.That(selected).IsFalse();
        await Assert.That(failure).IsEqualTo(LoginSelectionFailure.SelectionTargetUnavailable);
        await Assert.That(state.IsResolved).IsTrue();
        await Assert.That(state.SelectedWorldItemAddress).IsEqualTo(originalWorldItemAddress);
        await Assert
            .That(processMemory.ReadInt32(cUiWorldSelect + ClientStructs.Offsets.CUIWorldSelect.WorldIdx))
            .IsEqualTo(1);
        await Assert
            .That(processMemory.ReadUInt32(cUiWorldSelect + ClientStructs.Offsets.CUIWorldSelect.World))
            .IsEqualTo(originalWorldItemAddress);
        await Assert
            .That(processMemory.ReadInt32(cUiChannelSelect + ClientStructs.Offsets.CUIChannelSelect.Select))
            .IsEqualTo(0);
        await Assert
            .That(processMemory.ReadUInt32(cUiChannelSelect + ClientStructs.Offsets.CUIChannelSelect.WorldItem))
            .IsEqualTo(originalWorldItemAddress);
        await Assert
            .That(processMemory.ReadUInt32(cLogin + ClientStructs.Offsets.CLogin.WorldItem))
            .IsEqualTo(originalWorldItemAddress);
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

        public int ReadInt32(uint address)
        {
            Span<byte> bytes = stackalloc byte[sizeof(int)];
            if (!Read(address, bytes))
                throw new InvalidOperationException();

            return BinaryPrimitives.ReadInt32LittleEndian(bytes);
        }

        public uint ReadUInt32(uint address)
        {
            Span<byte> bytes = stackalloc byte[TypeSizes.UInt32];
            if (!Read(address, bytes))
                throw new InvalidOperationException();

            return BinaryPrimitives.ReadUInt32LittleEndian(bytes);
        }

        public void WriteZXStringPayload(uint payloadAddress, string value)
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(payloadAddress, 12u);

            byte[] payload = System.Text.Encoding.Latin1.GetBytes(value);
            byte[] bytes = new byte[12 + payload.Length + 1];
            BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(0, 4), 1);
            BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(4, 4), value.Length);
            BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(8, 4), payload.Length);
            payload.CopyTo(bytes.AsSpan(12));
            _values[payloadAddress - 12] = bytes;
        }
    }
}
