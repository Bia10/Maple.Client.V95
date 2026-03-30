using System.Buffers.Binary;
using Maple.Memory;
using Maple.Native;
using Maple.Process;

namespace Maple.Client.V95.Test;

public sealed class LoginStateResolverTests
{
    [Test]
    public async Task LoginStateResolver_Resolve_ComposesLoginStateAndSelections()
    {
        using var processMemory = new FakeRemoteProcessMemory();
        using var memoryAccessor = new MemoryAccessor(processMemory);
        var resolver = new LoginStateResolver(memoryAccessor);
        const uint cwvsContext = 0x8100_0000u;
        const uint cLogin = 0x8200_0000u;
        const uint cUiWorldSelect = 0x8300_0000u;
        const uint cUiChannelSelect = 0x8400_0000u;
        const uint worldItemAddress = 0x8500_0000u;
        const uint channelItemsBase = 0x8510_0000u;
        uint channelItemAddress = FieldAccessor.GetChannelItemAddress(channelItemsBase, 2);

        processMemory.WriteUInt32(ClientStructs.Addresses.CWvsContextSingletonPtr, cwvsContext);
        processMemory.WriteUInt32(ClientStructs.Addresses.CUIWorldSelectSingletonPtr, cUiWorldSelect);
        processMemory.WriteUInt32(ClientStructs.Addresses.CUIChannelSelectSingletonPtr, cUiChannelSelect);

        processMemory.WriteUInt32(cUiWorldSelect + ClientStructs.Offsets.CUIWorldSelect.Login, cLogin);
        processMemory.WriteUInt32(cUiChannelSelect + ClientStructs.Offsets.CUIChannelSelect.Login, cLogin);
        processMemory.WriteInt32(cUiWorldSelect + ClientStructs.Offsets.CUIWorldSelect.WorldIdx, 4);
        processMemory.WriteUInt32(cUiWorldSelect + ClientStructs.Offsets.CUIWorldSelect.World, worldItemAddress);
        processMemory.WriteInt32(cUiChannelSelect + ClientStructs.Offsets.CUIChannelSelect.Select, 2);
        processMemory.WriteUInt32(
            cUiChannelSelect + ClientStructs.Offsets.CUIChannelSelect.WorldItem,
            worldItemAddress
        );

        processMemory.WriteByte(cLogin + ClientStructs.Offsets.CLogin.LoginStep, (byte)LoginStep.SelectCharacter);
        processMemory.WriteInt32(cLogin + ClientStructs.Offsets.CLogin.StepChanging, 0);
        processMemory.WriteInt32(cLogin + ClientStructs.Offsets.CLogin.RequestSent, 1);
        processMemory.WriteUInt32(cLogin + ClientStructs.Offsets.CLogin.WorldItem, worldItemAddress);

        processMemory.WriteInt32(cwvsContext + ClientStructs.Offsets.CWvsContext.WorldId, 17);
        processMemory.WriteInt32(cwvsContext + ClientStructs.Offsets.CWvsContext.ChannelId, 3);
        processMemory.WriteInt32(cwvsContext + ClientStructs.Offsets.CWvsContext.CharacterId, 123456);

        processMemory.WriteInt32(worldItemAddress + ClientStructs.Offsets.WorldItem.Id, 17);
        processMemory.WriteUInt32(worldItemAddress + ClientStructs.Offsets.WorldItem.NamePtr, 0x8520_0000u);
        processMemory.WriteZXStringPayload(0x8520_0000u, "Luna");
        processMemory.WriteUInt32(worldItemAddress + ClientStructs.Offsets.WorldItem.ChannelItemsPtr, channelItemsBase);

        processMemory.WriteUInt32(channelItemAddress + ClientStructs.Offsets.ChannelItem.NamePtr, 0x8530_0000u);
        processMemory.WriteZXStringPayload(0x8530_0000u, "Channel 3");
        processMemory.WriteInt32(channelItemAddress + ClientStructs.Offsets.ChannelItem.WorldId, 17);
        processMemory.WriteInt32(channelItemAddress + ClientStructs.Offsets.ChannelItem.ChannelId, 3);
        processMemory.WriteInt32(channelItemAddress + ClientStructs.Offsets.ChannelItem.AdultFlag, 0);

        ResolvedLoginState state = resolver.Resolve();

        await Assert.That(state.LoginAddress).IsEqualTo(cLogin);
        await Assert.That(state.Step).IsEqualTo(LoginStep.SelectCharacter);
        await Assert.That(state.StepChanging).IsFalse();
        await Assert.That(state.RequestSent).IsTrue();
        await Assert.That(state.SelectedWorldItemAddress).IsEqualTo(worldItemAddress);
        await Assert.That(state.ContextWorldId).IsEqualTo(17);
        await Assert.That(state.ContextChannelId).IsEqualTo(3);
        await Assert.That(state.ContextCharacterId).IsEqualTo(123456);
        await Assert.That(state.HasWorldSelection).IsTrue();
        await Assert.That(state.HasChannelSelection).IsTrue();
        await Assert.That(state.WorldSelection).IsNotNull();
        await Assert.That(state.ChannelSelection).IsNotNull();
        ResolvedWorldSelection worldSelection = state.WorldSelection ?? throw new InvalidOperationException();
        ResolvedChannelSelection channelSelection = state.ChannelSelection ?? throw new InvalidOperationException();
        await Assert.That(worldSelection.WorldIndex).IsEqualTo(4);
        await Assert.That(worldSelection.WorldItem.Name).IsEqualTo("Luna");
        await Assert.That(channelSelection.ChannelIndex).IsEqualTo(2);
        await Assert.That(channelSelection.ChannelItem.Name).IsEqualTo("Channel 3");
    }

    [Test]
    public async Task LoginStateResolver_ResolveRaw_ComposesLoginStateAndSelectionsWithoutDecoding()
    {
        using var processMemory = new FakeRemoteProcessMemory();
        using var memoryAccessor = new MemoryAccessor(processMemory);
        var resolver = new LoginStateResolver(memoryAccessor);
        const uint cwvsContext = 0x8110_0000u;
        const uint cLogin = 0x8210_0000u;
        const uint cUiWorldSelect = 0x8310_0000u;
        const uint cUiChannelSelect = 0x8410_0000u;
        const uint worldItemAddress = 0x8511_0000u;
        const uint channelItemsBase = 0x8512_0000u;
        uint channelItemAddress = FieldAccessor.GetChannelItemAddress(channelItemsBase, 0);

        processMemory.WriteUInt32(ClientStructs.Addresses.CWvsContextSingletonPtr, cwvsContext);
        processMemory.WriteUInt32(ClientStructs.Addresses.CUIWorldSelectSingletonPtr, cUiWorldSelect);
        processMemory.WriteUInt32(ClientStructs.Addresses.CUIChannelSelectSingletonPtr, cUiChannelSelect);

        processMemory.WriteUInt32(cUiWorldSelect + ClientStructs.Offsets.CUIWorldSelect.Login, cLogin);
        processMemory.WriteUInt32(cUiChannelSelect + ClientStructs.Offsets.CUIChannelSelect.Login, cLogin);
        processMemory.WriteInt32(cUiWorldSelect + ClientStructs.Offsets.CUIWorldSelect.WorldIdx, 1);
        processMemory.WriteUInt32(cUiWorldSelect + ClientStructs.Offsets.CUIWorldSelect.World, worldItemAddress);
        processMemory.WriteInt32(cUiChannelSelect + ClientStructs.Offsets.CUIChannelSelect.Select, 0);
        processMemory.WriteUInt32(
            cUiChannelSelect + ClientStructs.Offsets.CUIChannelSelect.WorldItem,
            worldItemAddress
        );

        processMemory.WriteByte(cLogin + ClientStructs.Offsets.CLogin.LoginStep, (byte)LoginStep.SelectWorld);
        processMemory.WriteInt32(cLogin + ClientStructs.Offsets.CLogin.StepChanging, 0);
        processMemory.WriteInt32(cLogin + ClientStructs.Offsets.CLogin.RequestSent, 0);
        processMemory.WriteUInt32(cLogin + ClientStructs.Offsets.CLogin.WorldItem, worldItemAddress);

        processMemory.WriteInt32(cwvsContext + ClientStructs.Offsets.CWvsContext.WorldId, 19);
        processMemory.WriteInt32(cwvsContext + ClientStructs.Offsets.CWvsContext.ChannelId, 1);
        processMemory.WriteInt32(cwvsContext + ClientStructs.Offsets.CWvsContext.CharacterId, 7654321);

        processMemory.WriteInt32(worldItemAddress + ClientStructs.Offsets.WorldItem.Id, 19);
        processMemory.WriteUInt32(worldItemAddress + ClientStructs.Offsets.WorldItem.NamePtr, 0x8521_0000u);
        processMemory.WriteUInt32(worldItemAddress + ClientStructs.Offsets.WorldItem.ChannelItemsPtr, channelItemsBase);

        processMemory.WriteUInt32(channelItemAddress + ClientStructs.Offsets.ChannelItem.NamePtr, 0x8531_0000u);
        processMemory.WriteInt32(channelItemAddress + ClientStructs.Offsets.ChannelItem.WorldId, 19);
        processMemory.WriteInt32(channelItemAddress + ClientStructs.Offsets.ChannelItem.ChannelId, 1);
        processMemory.WriteInt32(channelItemAddress + ClientStructs.Offsets.ChannelItem.AdultFlag, 0);

        RawLoginState state = resolver.ResolveRaw();

        await Assert.That(state.LoginAddress).IsEqualTo(cLogin);
        await Assert.That(state.Step).IsEqualTo(LoginStep.SelectWorld);
        await Assert.That(state.StepChanging).IsFalse();
        await Assert.That(state.RequestSent).IsFalse();
        await Assert.That(state.SelectedWorldItemAddress).IsEqualTo(worldItemAddress);
        await Assert.That(state.ContextWorldId).IsEqualTo(19);
        await Assert.That(state.ContextChannelId).IsEqualTo(1);
        await Assert.That(state.ContextCharacterId).IsEqualTo(7654321);
        await Assert.That(state.HasWorldSelection).IsTrue();
        await Assert.That(state.HasChannelSelection).IsTrue();
        await Assert.That(state.WorldSelection).IsNotNull();
        await Assert.That(state.ChannelSelection).IsNotNull();
        RawWorldSelection worldSelection = state.WorldSelection ?? throw new InvalidOperationException();
        RawChannelSelection channelSelection = state.ChannelSelection ?? throw new InvalidOperationException();
        await Assert.That(worldSelection.WorldIndex).IsEqualTo(1);
        await Assert.That(worldSelection.WorldItem.NamePointer).IsEqualTo(0x8521_0000u);
        await Assert.That(channelSelection.ChannelIndex).IsEqualTo(0);
        await Assert.That(channelSelection.ChannelItem.NamePointer).IsEqualTo(0x8531_0000u);
        await Assert.That(channelSelection.ChannelItem.ChannelId).IsEqualTo(1);
    }

    [Test]
    public async Task LoginStateResolver_TryResolve_WhenLoginAddressCannotBeDiscovered_ReturnsFalse()
    {
        using var processMemory = new FakeRemoteProcessMemory();
        using var memoryAccessor = new MemoryAccessor(processMemory);
        var resolver = new LoginStateResolver(memoryAccessor);

        bool resolved = resolver.TryResolve(out ResolvedLoginState state);

        await Assert.That(resolved).IsFalse();
        await Assert.That(state.IsResolved).IsFalse();
        await Assert.That(state.LoginAddress).IsEqualTo(0u);
    }

    [Test]
    public async Task LoginStateResolver_Resolve_LeavesOptionalSnapshotsNull_WhenUiPointersAreMissing()
    {
        using var processMemory = new FakeRemoteProcessMemory();
        using var memoryAccessor = new MemoryAccessor(processMemory);
        var resolver = new LoginStateResolver(memoryAccessor);
        const uint cLogin = 0x8600_0000u;
        const uint cUiWorldSelect = 0x8700_0000u;

        processMemory.WriteUInt32(ClientStructs.Addresses.CUIWorldSelectSingletonPtr, cUiWorldSelect);
        processMemory.WriteUInt32(cUiWorldSelect + ClientStructs.Offsets.CUIWorldSelect.Login, cLogin);

        processMemory.WriteByte(cLogin + ClientStructs.Offsets.CLogin.LoginStep, (byte)LoginStep.Title);
        processMemory.WriteInt32(cLogin + ClientStructs.Offsets.CLogin.StepChanging, 0);
        processMemory.WriteInt32(cLogin + ClientStructs.Offsets.CLogin.RequestSent, 0);
        processMemory.WriteUInt32(cLogin + ClientStructs.Offsets.CLogin.WorldItem, 0);
        processMemory.WriteInt32(cUiWorldSelect + ClientStructs.Offsets.CUIWorldSelect.WorldIdx, 0);
        processMemory.WriteUInt32(cUiWorldSelect + ClientStructs.Offsets.CUIWorldSelect.World, 0);

        ResolvedLoginState state = resolver.Resolve();

        await Assert.That(state.LoginAddress).IsEqualTo(cLogin);
        await Assert.That(state.Step).IsEqualTo(LoginStep.Title);
        await Assert.That(state.SelectedWorldItemAddress).IsEqualTo(0u);
        await Assert.That(state.ContextWorldId).IsNull();
        await Assert.That(state.ContextChannelId).IsNull();
        await Assert.That(state.ContextCharacterId).IsNull();
        await Assert.That(state.HasWorldSelection).IsFalse();
        await Assert.That(state.HasChannelSelection).IsFalse();
        await Assert.That(state.WorldSelection).IsNull();
        await Assert.That(state.ChannelSelection).IsNull();
    }

    [Test]
    public async Task LoginStateResolver_ResolveRaw_LeavesOptionalContextNull_WhenContextReadsFail()
    {
        using var processMemory = new FakeRemoteProcessMemory();
        using var memoryAccessor = new MemoryAccessor(processMemory);
        var resolver = new LoginStateResolver(memoryAccessor);
        const uint cwvsContext = 0x8800_0000u;
        const uint cLogin = 0x8810_0000u;
        const uint cUiWorldSelect = 0x8820_0000u;

        processMemory.WriteUInt32(ClientStructs.Addresses.CWvsContextSingletonPtr, cwvsContext);
        processMemory.WriteUInt32(ClientStructs.Addresses.CUIWorldSelectSingletonPtr, cUiWorldSelect);
        processMemory.WriteUInt32(cUiWorldSelect + ClientStructs.Offsets.CUIWorldSelect.Login, cLogin);
        processMemory.WriteUInt32(cUiWorldSelect + ClientStructs.Offsets.CUIWorldSelect.World, 0);
        processMemory.WriteByte(cLogin + ClientStructs.Offsets.CLogin.LoginStep, (byte)LoginStep.Title);
        processMemory.WriteInt32(cLogin + ClientStructs.Offsets.CLogin.StepChanging, 0);
        processMemory.WriteInt32(cLogin + ClientStructs.Offsets.CLogin.RequestSent, 0);
        processMemory.WriteUInt32(cLogin + ClientStructs.Offsets.CLogin.WorldItem, 0);

        RawLoginState state = resolver.ResolveRaw();

        await Assert.That(state.LoginAddress).IsEqualTo(cLogin);
        await Assert.That(state.ContextWorldId).IsNull();
        await Assert.That(state.ContextChannelId).IsNull();
        await Assert.That(state.ContextCharacterId).IsNull();
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
