using System.Buffers.Binary;
using Maple.Memory;
using Maple.Native;
using Maple.Process;

namespace Maple.Client.V95.Test;

public sealed class FieldAccessorTests
{
    [Test]
    public async Task FieldAccessor_ReadsRegisteredPrimitiveFields()
    {
        using var processMemory = new FakeRemoteProcessMemory();
        using var memoryAccessor = new MemoryAccessor(processMemory);
        var fieldAccessor = new FieldAccessor(memoryAccessor);
        const uint cwvsContext = 0x1000_0000u;
        const uint cQuestMan = 0x2000_0000u;

        processMemory.WriteInt32(cwvsContext + ClientStructs.Offsets.CWvsContext.WorldId, 17);
        processMemory.WriteInt32(cwvsContext + ClientStructs.Offsets.CWvsContext.ChannelId, 4);
        processMemory.WriteInt32(cQuestMan + ClientStructs.Offsets.CQuestMan.WorldId, 9);

        await Assert
            .That(
                fieldAccessor.ReadInt32(
                    cwvsContext,
                    nameof(ClientStructs.Fields.CWvsContext),
                    nameof(ClientStructs.Offsets.CWvsContext.WorldId)
                )
            )
            .IsEqualTo(17);
        await Assert
            .That(
                fieldAccessor.ReadInt32(
                    cwvsContext,
                    nameof(ClientStructs.Fields.CWvsContext),
                    nameof(ClientStructs.Offsets.CWvsContext.ChannelId)
                )
            )
            .IsEqualTo(4);
        await Assert
            .That(
                fieldAccessor.ReadInt32(
                    cQuestMan,
                    nameof(ClientStructs.Fields.CQuestMan),
                    nameof(ClientStructs.Offsets.CQuestMan.WorldId)
                )
            )
            .IsEqualTo(9);
    }

    [Test]
    public async Task FieldAccessor_ReadLoginStep_ReadsByteEnum()
    {
        using var processMemory = new FakeRemoteProcessMemory();
        using var memoryAccessor = new MemoryAccessor(processMemory);
        var fieldAccessor = new FieldAccessor(memoryAccessor);
        const uint cLogin = 0x3000_0000u;

        processMemory.WriteByte(cLogin + ClientStructs.Offsets.CLogin.LoginStep, (byte)LoginStep.SelectCharacter);

        await Assert.That(fieldAccessor.ReadLoginStep(cLogin)).IsEqualTo(LoginStep.SelectCharacter);
    }

    [Test]
    public async Task FieldAccessor_ReadBool32_ReadsWin32StyleBoolean()
    {
        using var processMemory = new FakeRemoteProcessMemory();
        using var memoryAccessor = new MemoryAccessor(processMemory);
        var fieldAccessor = new FieldAccessor(memoryAccessor);
        const uint cLogin = 0x4000_0000u;

        processMemory.WriteInt32(cLogin + ClientStructs.Offsets.CLogin.StepChanging, 1);

        await Assert
            .That(
                fieldAccessor.ReadBool32(
                    cLogin,
                    nameof(ClientStructs.Fields.CLogin),
                    nameof(ClientStructs.Offsets.CLogin.StepChanging)
                )
            )
            .IsTrue();
    }

    [Test]
    public async Task FieldAccessor_GetOffset_WhenUnknownField_Throws()
    {
        using var processMemory = new FakeRemoteProcessMemory();
        using var memoryAccessor = new MemoryAccessor(processMemory);
        var fieldAccessor = new FieldAccessor(memoryAccessor);

        await Assert
            .That(() => fieldAccessor.GetOffset(nameof(ClientStructs.Fields.CLogin), "MissingField"))
            .Throws<KeyNotFoundException>();
    }

    [Test]
    public async Task FieldAccessor_TypedHelpers_ReadLoginFlowFields()
    {
        using var processMemory = new FakeRemoteProcessMemory();
        using var memoryAccessor = new MemoryAccessor(processMemory);
        var fieldAccessor = new FieldAccessor(memoryAccessor);
        const uint cwvsContext = 0x5000_0000u;
        const uint cLogin = 0x6000_0000u;
        const uint cUiWorldSelect = 0x7000_0000u;
        const uint cUiChannelSelect = 0x7100_0000u;

        processMemory.WriteInt32(cwvsContext + ClientStructs.Offsets.CWvsContext.WorldId, 1);
        processMemory.WriteInt32(cwvsContext + ClientStructs.Offsets.CWvsContext.ChannelId, 12);
        processMemory.WriteInt32(cwvsContext + ClientStructs.Offsets.CWvsContext.CharacterId, 3456);
        processMemory.WriteInt32(cLogin + ClientStructs.Offsets.CLogin.StepChanging, 1);
        processMemory.WriteInt32(cLogin + ClientStructs.Offsets.CLogin.RequestSent, 0);
        processMemory.WriteUInt32(cLogin + ClientStructs.Offsets.CLogin.WorldItem, 0x1234_0000u);
        processMemory.WriteInt32(cUiWorldSelect + ClientStructs.Offsets.CUIWorldSelect.WorldIdx, 3);
        processMemory.WriteInt32(cUiChannelSelect + ClientStructs.Offsets.CUIChannelSelect.Select, 2);

        await Assert.That(fieldAccessor.ReadWorldId(cwvsContext)).IsEqualTo(1);
        await Assert.That(fieldAccessor.ReadChannelId(cwvsContext)).IsEqualTo(12);
        await Assert.That(fieldAccessor.ReadCharacterId(cwvsContext)).IsEqualTo(3456);
        await Assert.That(fieldAccessor.ReadStepChanging(cLogin)).IsTrue();
        await Assert.That(fieldAccessor.ReadRequestSent(cLogin)).IsFalse();
        await Assert.That(fieldAccessor.ReadLoginWorldItem(cLogin)).IsEqualTo(0x1234_0000u);
        await Assert.That(fieldAccessor.ReadWorldIndex(cUiWorldSelect)).IsEqualTo(3);
        await Assert.That(fieldAccessor.ReadChannelSelection(cUiChannelSelect)).IsEqualTo(2);
    }

    [Test]
    public async Task FieldAccessor_TypedWriteHelpers_UpdateLoginFlowFields()
    {
        using var processMemory = new FakeRemoteProcessMemory();
        using var memoryAccessor = new MemoryAccessor(processMemory);
        var fieldAccessor = new FieldAccessor(memoryAccessor);
        const uint cLogin = 0x6200_0000u;
        const uint cUiWorldSelect = 0x7200_0000u;
        const uint cUiChannelSelect = 0x7300_0000u;

        fieldAccessor.WriteLoginWorldItem(cLogin, 0x1111_0000u);
        fieldAccessor.WriteWorldIndex(cUiWorldSelect, 9);
        fieldAccessor.WriteWorldSelectWorldItem(cUiWorldSelect, 0x2222_0000u);
        fieldAccessor.WriteChannelSelection(cUiChannelSelect, 4);
        fieldAccessor.WriteChannelSelectWorldItem(cUiChannelSelect, 0x3333_0000u);

        await Assert.That(fieldAccessor.ReadLoginWorldItem(cLogin)).IsEqualTo(0x1111_0000u);
        await Assert.That(fieldAccessor.ReadWorldIndex(cUiWorldSelect)).IsEqualTo(9);
        await Assert.That(fieldAccessor.ReadWorldSelectWorldItem(cUiWorldSelect)).IsEqualTo(0x2222_0000u);
        await Assert.That(fieldAccessor.ReadChannelSelection(cUiChannelSelect)).IsEqualTo(4);
        await Assert.That(fieldAccessor.ReadChannelSelectWorldItem(cUiChannelSelect)).IsEqualTo(0x3333_0000u);
    }

    [Test]
    public async Task SingletonResolver_ResolvesKnownSingleton_ByAddressAndName()
    {
        using var processMemory = new FakeRemoteProcessMemory();
        using var memoryAccessor = new MemoryAccessor(processMemory);
        var resolver = new SingletonResolver(memoryAccessor);

        processMemory.WriteUInt32(ClientStructs.Addresses.CWvsContextSingletonPtr, 0x1111_0000u);

        bool byAddress = resolver.TryResolve(
            ClientStructs.Addresses.CWvsContextSingletonPtr,
            out ResolvedSingleton addressSingleton
        );
        bool byName = resolver.TryResolve("CWvsContext", out ResolvedSingleton nameSingleton);

        await Assert.That(byAddress).IsTrue();
        await Assert.That(byName).IsTrue();
        await Assert.That(addressSingleton.Name).IsEqualTo("CWvsContext");
        await Assert
            .That(addressSingleton.PointerTableAddress)
            .IsEqualTo(ClientStructs.Addresses.CWvsContextSingletonPtr);
        await Assert.That(addressSingleton.InstanceAddress).IsEqualTo(0x1111_0000u);
        await Assert.That(nameSingleton).IsEqualTo(addressSingleton);
        await Assert.That(resolver.TryResolveContext(out uint contextAddress)).IsTrue();
        await Assert.That(contextAddress).IsEqualTo(0x1111_0000u);
    }

    [Test]
    public async Task SingletonResolver_TypedHelpers_ResolveKnownUiSingletonsWithoutNameLookup()
    {
        using var processMemory = new FakeRemoteProcessMemory();
        using var memoryAccessor = new MemoryAccessor(processMemory);
        var resolver = new SingletonResolver(memoryAccessor);

        processMemory.WriteUInt32(ClientStructs.Addresses.CUIWorldSelectSingletonPtr, 0x2222_0000u);
        processMemory.WriteUInt32(ClientStructs.Addresses.CUIChannelSelectSingletonPtr, 0x3333_0000u);

        bool hasWorldSelect = resolver.TryResolveWorldSelect(out uint worldSelectAddress);
        bool hasChannelSelect = resolver.TryResolveChannelSelect(out uint channelSelectAddress);

        await Assert.That(hasWorldSelect).IsTrue();
        await Assert.That(worldSelectAddress).IsEqualTo(0x2222_0000u);
        await Assert.That(hasChannelSelect).IsTrue();
        await Assert.That(channelSelectAddress).IsEqualTo(0x3333_0000u);
        await Assert.That(resolver.HasActiveWorldSelect()).IsTrue();
        await Assert.That(resolver.HasActiveChannelSelect()).IsTrue();
        await Assert.That(resolver.ResolveWorldSelect()).IsEqualTo(0x2222_0000u);
        await Assert.That(resolver.ResolveChannelSelect()).IsEqualTo(0x3333_0000u);
    }

    [Test]
    public async Task SingletonResolver_ResolveUnknownSingletonName_Throws()
    {
        using var processMemory = new FakeRemoteProcessMemory();
        using var memoryAccessor = new MemoryAccessor(processMemory);
        var resolver = new SingletonResolver(memoryAccessor);

        await Assert.That(() => resolver.Resolve("MissingSingleton")).Throws<KeyNotFoundException>();
    }

    [Test]
    public async Task FieldAccessor_ReadWorldItem_DecodesStructuredEntry()
    {
        using var processMemory = new FakeRemoteProcessMemory();
        using var memoryAccessor = new MemoryAccessor(processMemory);
        var fieldAccessor = new FieldAccessor(memoryAccessor);
        const uint worldItemsBase = 0x7200_0000u;
        uint worldItemAddress = FieldAccessor.GetWorldItemAddress(worldItemsBase, 2);

        processMemory.WriteInt32(worldItemAddress + ClientStructs.Offsets.WorldItem.Id, 7);
        processMemory.WriteUInt32(worldItemAddress + ClientStructs.Offsets.WorldItem.NamePtr, 0x2222_0000u);
        processMemory.WriteZXStringPayload(0x2222_0000u, "Scania");
        processMemory.WriteUInt32(worldItemAddress + ClientStructs.Offsets.WorldItem.ChannelItemsPtr, 0x3333_0000u);

        ResolvedWorldItem worldItem = fieldAccessor.ReadWorldItem(worldItemAddress);

        await Assert.That(worldItem.Address).IsEqualTo(worldItemAddress);
        await Assert.That(worldItem.Id).IsEqualTo(7);
        await Assert.That(worldItem.NamePointer).IsEqualTo(0x2222_0000u);
        await Assert.That(worldItem.Name).IsEqualTo("Scania");
        await Assert.That(worldItem.ChannelItemsPointer).IsEqualTo(0x3333_0000u);
    }

    [Test]
    public async Task FieldAccessor_ReadRawWorldItem_ReadsStructuredEntryWithoutDecoding()
    {
        using var processMemory = new FakeRemoteProcessMemory();
        using var memoryAccessor = new MemoryAccessor(processMemory);
        var fieldAccessor = new FieldAccessor(memoryAccessor);
        const uint worldItemsBase = 0x7220_0000u;
        uint worldItemAddress = FieldAccessor.GetWorldItemAddress(worldItemsBase, 1);

        processMemory.WriteInt32(worldItemAddress + ClientStructs.Offsets.WorldItem.Id, 11);
        processMemory.WriteUInt32(worldItemAddress + ClientStructs.Offsets.WorldItem.NamePtr, 0x2223_0000u);
        processMemory.WriteUInt32(worldItemAddress + ClientStructs.Offsets.WorldItem.ChannelItemsPtr, 0x3334_0000u);

        RawWorldItem worldItem = fieldAccessor.ReadRawWorldItem(worldItemAddress);

        await Assert.That(worldItem.Address).IsEqualTo(worldItemAddress);
        await Assert.That(worldItem.Id).IsEqualTo(11);
        await Assert.That(worldItem.NamePointer).IsEqualTo(0x2223_0000u);
        await Assert.That(worldItem.ChannelItemsPointer).IsEqualTo(0x3334_0000u);
    }

    [Test]
    public async Task FieldAccessor_ReadChannelItem_DecodesStructuredEntry()
    {
        using var processMemory = new FakeRemoteProcessMemory();
        using var memoryAccessor = new MemoryAccessor(processMemory);
        var fieldAccessor = new FieldAccessor(memoryAccessor);
        const uint channelItemsBase = 0x7300_0000u;
        uint channelItemAddress = FieldAccessor.GetChannelItemAddress(channelItemsBase, 4);

        processMemory.WriteUInt32(channelItemAddress + ClientStructs.Offsets.ChannelItem.NamePtr, 0x4444_0000u);
        processMemory.WriteZXStringPayload(0x4444_0000u, "Channel 14");
        processMemory.WriteInt32(channelItemAddress + ClientStructs.Offsets.ChannelItem.WorldId, 9);
        processMemory.WriteInt32(channelItemAddress + ClientStructs.Offsets.ChannelItem.ChannelId, 14);
        processMemory.WriteInt32(channelItemAddress + ClientStructs.Offsets.ChannelItem.AdultFlag, 1);

        ResolvedChannelItem channelItem = fieldAccessor.ReadChannelItem(channelItemAddress);

        await Assert.That(channelItem.Address).IsEqualTo(channelItemAddress);
        await Assert.That(channelItem.NamePointer).IsEqualTo(0x4444_0000u);
        await Assert.That(channelItem.Name).IsEqualTo("Channel 14");
        await Assert.That(channelItem.WorldId).IsEqualTo(9);
        await Assert.That(channelItem.ChannelId).IsEqualTo(14);
        await Assert.That(channelItem.AdultFlag).IsTrue();
    }

    [Test]
    public async Task FieldAccessor_ReadRawChannelItem_ReadsStructuredEntryWithoutDecoding()
    {
        using var processMemory = new FakeRemoteProcessMemory();
        using var memoryAccessor = new MemoryAccessor(processMemory);
        var fieldAccessor = new FieldAccessor(memoryAccessor);
        const uint channelItemsBase = 0x7320_0000u;
        uint channelItemAddress = FieldAccessor.GetChannelItemAddress(channelItemsBase, 3);

        processMemory.WriteUInt32(channelItemAddress + ClientStructs.Offsets.ChannelItem.NamePtr, 0x4445_0000u);
        processMemory.WriteInt32(channelItemAddress + ClientStructs.Offsets.ChannelItem.WorldId, 12);
        processMemory.WriteInt32(channelItemAddress + ClientStructs.Offsets.ChannelItem.ChannelId, 4);
        processMemory.WriteInt32(channelItemAddress + ClientStructs.Offsets.ChannelItem.AdultFlag, 0);

        RawChannelItem channelItem = fieldAccessor.ReadRawChannelItem(channelItemAddress);

        await Assert.That(channelItem.Address).IsEqualTo(channelItemAddress);
        await Assert.That(channelItem.NamePointer).IsEqualTo(0x4445_0000u);
        await Assert.That(channelItem.WorldId).IsEqualTo(12);
        await Assert.That(channelItem.ChannelId).IsEqualTo(4);
        await Assert.That(channelItem.AdultFlag).IsFalse();
    }

    [Test]
    public async Task LoginSelectionResolver_ResolvesRawWorldAndChannelSelectionsWithoutDecoding()
    {
        using var processMemory = new FakeRemoteProcessMemory();
        using var memoryAccessor = new MemoryAccessor(processMemory);
        var resolver = new LoginSelectionResolver(memoryAccessor);
        const uint cUiWorldSelect = 0x7610_0000u;
        const uint cUiChannelSelect = 0x7710_0000u;
        const uint worldItemAddress = 0x7811_0000u;
        const uint channelItemsBase = 0x7812_0000u;
        uint channelItemAddress = FieldAccessor.GetChannelItemAddress(channelItemsBase, 1);

        processMemory.WriteUInt32(ClientStructs.Addresses.CUIWorldSelectSingletonPtr, cUiWorldSelect);
        processMemory.WriteUInt32(ClientStructs.Addresses.CUIChannelSelectSingletonPtr, cUiChannelSelect);

        processMemory.WriteInt32(cUiWorldSelect + ClientStructs.Offsets.CUIWorldSelect.WorldIdx, 3);
        processMemory.WriteUInt32(cUiWorldSelect + ClientStructs.Offsets.CUIWorldSelect.World, worldItemAddress);

        processMemory.WriteInt32(cUiChannelSelect + ClientStructs.Offsets.CUIChannelSelect.Select, 1);
        processMemory.WriteUInt32(
            cUiChannelSelect + ClientStructs.Offsets.CUIChannelSelect.WorldItem,
            worldItemAddress
        );

        processMemory.WriteInt32(worldItemAddress + ClientStructs.Offsets.WorldItem.Id, 22);
        processMemory.WriteUInt32(worldItemAddress + ClientStructs.Offsets.WorldItem.NamePtr, 0x7821_0000u);
        processMemory.WriteUInt32(worldItemAddress + ClientStructs.Offsets.WorldItem.ChannelItemsPtr, channelItemsBase);

        processMemory.WriteUInt32(channelItemAddress + ClientStructs.Offsets.ChannelItem.NamePtr, 0x7831_0000u);
        processMemory.WriteInt32(channelItemAddress + ClientStructs.Offsets.ChannelItem.WorldId, 22);
        processMemory.WriteInt32(channelItemAddress + ClientStructs.Offsets.ChannelItem.ChannelId, 2);
        processMemory.WriteInt32(channelItemAddress + ClientStructs.Offsets.ChannelItem.AdultFlag, 1);

        RawWorldSelection worldSelection = resolver.ResolveRawWorldSelection();
        RawChannelSelection channelSelection = resolver.ResolveRawChannelSelection();

        await Assert.That(worldSelection.UiAddress).IsEqualTo(cUiWorldSelect);
        await Assert.That(worldSelection.WorldIndex).IsEqualTo(3);
        await Assert.That(worldSelection.WorldItem.Address).IsEqualTo(worldItemAddress);
        await Assert.That(worldSelection.WorldItem.Id).IsEqualTo(22);
        await Assert.That(worldSelection.WorldItem.NamePointer).IsEqualTo(0x7821_0000u);
        await Assert.That(channelSelection.UiAddress).IsEqualTo(cUiChannelSelect);
        await Assert.That(channelSelection.ChannelIndex).IsEqualTo(1);
        await Assert.That(channelSelection.WorldItem.ChannelItemsPointer).IsEqualTo(channelItemsBase);
        await Assert.That(channelSelection.ChannelItem.Address).IsEqualTo(channelItemAddress);
        await Assert.That(channelSelection.ChannelItem.NamePointer).IsEqualTo(0x7831_0000u);
        await Assert.That(channelSelection.ChannelItem.ChannelId).IsEqualTo(2);
        await Assert.That(channelSelection.ChannelItem.AdultFlag).IsTrue();
    }

    [Test]
    public async Task LoginSelectionResolver_ResolvesWorldAndChannelSelections()
    {
        using var processMemory = new FakeRemoteProcessMemory();
        using var memoryAccessor = new MemoryAccessor(processMemory);
        var resolver = new LoginSelectionResolver(memoryAccessor);
        const uint cUiWorldSelect = 0x7600_0000u;
        const uint cUiChannelSelect = 0x7700_0000u;
        const uint worldItemAddress = 0x7800_0000u;
        const uint channelItemsBase = 0x7810_0000u;
        uint channelItemAddress = FieldAccessor.GetChannelItemAddress(channelItemsBase, 2);

        processMemory.WriteUInt32(ClientStructs.Addresses.CUIWorldSelectSingletonPtr, cUiWorldSelect);
        processMemory.WriteUInt32(ClientStructs.Addresses.CUIChannelSelectSingletonPtr, cUiChannelSelect);

        processMemory.WriteInt32(cUiWorldSelect + ClientStructs.Offsets.CUIWorldSelect.WorldIdx, 5);
        processMemory.WriteUInt32(cUiWorldSelect + ClientStructs.Offsets.CUIWorldSelect.World, worldItemAddress);

        processMemory.WriteInt32(cUiChannelSelect + ClientStructs.Offsets.CUIChannelSelect.Select, 2);
        processMemory.WriteUInt32(
            cUiChannelSelect + ClientStructs.Offsets.CUIChannelSelect.WorldItem,
            worldItemAddress
        );

        processMemory.WriteInt32(worldItemAddress + ClientStructs.Offsets.WorldItem.Id, 17);
        processMemory.WriteUInt32(worldItemAddress + ClientStructs.Offsets.WorldItem.NamePtr, 0x7820_0000u);
        processMemory.WriteZXStringPayload(0x7820_0000u, "Luna");
        processMemory.WriteUInt32(worldItemAddress + ClientStructs.Offsets.WorldItem.ChannelItemsPtr, channelItemsBase);

        processMemory.WriteUInt32(channelItemAddress + ClientStructs.Offsets.ChannelItem.NamePtr, 0x7830_0000u);
        processMemory.WriteZXStringPayload(0x7830_0000u, "Channel 3");
        processMemory.WriteInt32(channelItemAddress + ClientStructs.Offsets.ChannelItem.WorldId, 17);
        processMemory.WriteInt32(channelItemAddress + ClientStructs.Offsets.ChannelItem.ChannelId, 3);
        processMemory.WriteInt32(channelItemAddress + ClientStructs.Offsets.ChannelItem.AdultFlag, 0);

        ResolvedWorldSelection worldSelection = resolver.ResolveWorldSelection();
        ResolvedChannelSelection channelSelection = resolver.ResolveChannelSelection();

        await Assert.That(worldSelection.UiAddress).IsEqualTo(cUiWorldSelect);
        await Assert.That(worldSelection.WorldIndex).IsEqualTo(5);
        await Assert.That(worldSelection.WorldItem.Name).IsEqualTo("Luna");
        await Assert.That(channelSelection.UiAddress).IsEqualTo(cUiChannelSelect);
        await Assert.That(channelSelection.ChannelIndex).IsEqualTo(2);
        await Assert.That(channelSelection.WorldItem.Id).IsEqualTo(17);
        await Assert.That(channelSelection.ChannelItem.Name).IsEqualTo("Channel 3");
        await Assert.That(channelSelection.ChannelItem.ChannelId).IsEqualTo(3);
    }

    [Test]
    public async Task LoginSelectionResolver_SelectWorldAndChannel_UpdatesUiState()
    {
        using var processMemory = new FakeRemoteProcessMemory();
        using var memoryAccessor = new MemoryAccessor(processMemory);
        var resolver = new LoginSelectionResolver(memoryAccessor);
        const uint cUiWorldSelect = 0x7A00_0000u;
        const uint cUiChannelSelect = 0x7B00_0000u;
        const uint cLogin = 0x7C00_0000u;
        const uint worldItemAddress = 0x7D00_0000u;
        const uint channelItemsBase = 0x7D10_0000u;
        uint channelItemAddress = FieldAccessor.GetChannelItemAddress(channelItemsBase, 1);

        processMemory.WriteUInt32(ClientStructs.Addresses.CUIWorldSelectSingletonPtr, cUiWorldSelect);
        processMemory.WriteUInt32(ClientStructs.Addresses.CUIChannelSelectSingletonPtr, cUiChannelSelect);
        processMemory.WriteUInt32(cUiWorldSelect + ClientStructs.Offsets.CUIWorldSelect.Login, cLogin);
        processMemory.WriteUInt32(cUiChannelSelect + ClientStructs.Offsets.CUIChannelSelect.Login, cLogin);

        processMemory.WriteInt32(worldItemAddress + ClientStructs.Offsets.WorldItem.Id, 8);
        processMemory.WriteUInt32(worldItemAddress + ClientStructs.Offsets.WorldItem.NamePtr, 0x7D20_0000u);
        processMemory.WriteZXStringPayload(0x7D20_0000u, "Windia");
        processMemory.WriteUInt32(worldItemAddress + ClientStructs.Offsets.WorldItem.ChannelItemsPtr, channelItemsBase);

        processMemory.WriteUInt32(channelItemAddress + ClientStructs.Offsets.ChannelItem.NamePtr, 0x7D30_0000u);
        processMemory.WriteZXStringPayload(0x7D30_0000u, "Channel 2");
        processMemory.WriteInt32(channelItemAddress + ClientStructs.Offsets.ChannelItem.WorldId, 8);
        processMemory.WriteInt32(channelItemAddress + ClientStructs.Offsets.ChannelItem.ChannelId, 2);
        processMemory.WriteInt32(channelItemAddress + ClientStructs.Offsets.ChannelItem.AdultFlag, 1);

        ResolvedWorldSelection worldSelection = resolver.SelectWorld(worldItemAddress, 6);
        ResolvedChannelSelection channelSelection = resolver.SelectChannel(worldItemAddress, 1);

        await Assert.That(worldSelection.WorldIndex).IsEqualTo(6);
        await Assert.That(worldSelection.WorldItem.Name).IsEqualTo("Windia");
        await Assert.That(channelSelection.ChannelIndex).IsEqualTo(1);
        await Assert.That(channelSelection.ChannelItem.Name).IsEqualTo("Channel 2");
        await Assert.That(channelSelection.ChannelItem.AdultFlag).IsTrue();
        await Assert
            .That(processMemory.ReadUInt32Value(cLogin + ClientStructs.Offsets.CLogin.WorldItem))
            .IsEqualTo(worldItemAddress);
    }

    [Test]
    public async Task FieldAccessor_ReadsSelectedWorldPointers_FromUiStructures()
    {
        using var processMemory = new FakeRemoteProcessMemory();
        using var memoryAccessor = new MemoryAccessor(processMemory);
        var fieldAccessor = new FieldAccessor(memoryAccessor);
        const uint cUiWorldSelect = 0x7400_0000u;
        const uint cUiChannelSelect = 0x7500_0000u;

        processMemory.WriteUInt32(cUiWorldSelect + ClientStructs.Offsets.CUIWorldSelect.World, 0x5555_0000u);
        processMemory.WriteUInt32(cUiChannelSelect + ClientStructs.Offsets.CUIChannelSelect.WorldItem, 0x6666_0000u);

        await Assert.That(fieldAccessor.ReadWorldSelectWorldItem(cUiWorldSelect)).IsEqualTo(0x5555_0000u);
        await Assert.That(fieldAccessor.ReadChannelSelectWorldItem(cUiChannelSelect)).IsEqualTo(0x6666_0000u);
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

        public uint ReadUInt32Value(uint address)
        {
            if (!_values.TryGetValue(address, out byte[]? bytes) || bytes.Length < TypeSizes.UInt32)
                throw new KeyNotFoundException($"Address 0x{address:X8} is not present.");

            return BinaryPrimitives.ReadUInt32LittleEndian(bytes);
        }

        public void WriteZXStringPayload(uint payloadAddress, string value)
        {
            byte[] payload = System.Text.Encoding.Latin1.GetBytes(value);
            byte[] bytes = new byte[12 + payload.Length + 1];
            BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(0, 4), 1);
            BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(4, 4), value.Length);
            BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(8, 4), payload.Length);
            payload.CopyTo(bytes.AsSpan(12));
            _values[payloadAddress - 12u] = bytes;
        }
    }
}
