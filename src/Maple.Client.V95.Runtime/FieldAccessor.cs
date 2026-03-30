using Maple.Memory;

namespace Maple.Client.V95.Runtime;

/// <summary>
/// Registry-backed primitive field reader for GMS v95 client structs.
/// </summary>
/// <remarks>
/// This type intentionally stops at primitive field access: offset lookup, pointer reads, integer reads,
/// and enum decoding for known byte-sized client states. Higher-level orchestration such as singleton
/// discovery and workflow-specific automation should stay outside this layer until the rest of the
/// `Maple.Memory` extraction is in place.
/// </remarks>
/// <remarks>
/// Creates a field accessor over <paramref name="memoryAccessor"/>.
/// </remarks>
public sealed class FieldAccessor(MemoryAccessor memoryAccessor, StructFieldRegistry? registry = null)
{
    private readonly MemoryAccessor _memoryAccessor =
        memoryAccessor ?? throw new ArgumentNullException(nameof(memoryAccessor));
    private readonly StructFieldRegistry _registry = registry ?? ClientStructs.Registry;

    /// <summary>
    /// Resolves one field offset from the registry.
    /// </summary>
    public int GetOffset(string structName, string fieldName) => GetField(structName, fieldName).Offset;

    /// <summary>
    /// Attempts to resolve one field offset from the registry.
    /// </summary>
    public bool TryGetOffset(string structName, string fieldName, out int offset)
    {
        if (_registry.TryGetField(structName, fieldName, out StructField field))
        {
            offset = field.Offset;
            return true;
        }

        offset = 0;
        return false;
    }

    /// <summary>
    /// Reads a raw byte field.
    /// </summary>
    public byte ReadByte(uint structAddress, string structName, string fieldName) =>
        _memoryAccessor.ReadByte(ResolveAddress(structAddress, structName, fieldName));

    /// <summary>
    /// Reads a 32-bit signed integer field.
    /// </summary>
    public int ReadInt32(uint structAddress, string structName, string fieldName) =>
        _memoryAccessor.ReadInt32(ResolveAddress(structAddress, structName, fieldName));

    /// <summary>
    /// Reads a 32-bit unsigned integer field.
    /// </summary>
    public uint ReadUInt32(uint structAddress, string structName, string fieldName) =>
        _memoryAccessor.ReadUInt32(ResolveAddress(structAddress, structName, fieldName));

    /// <summary>
    /// Reads a 32-bit x86 pointer field.
    /// </summary>
    public uint ReadPointer(uint structAddress, string structName, string fieldName) =>
        _memoryAccessor.ReadPointer(ResolveAddress(structAddress, structName, fieldName));

    /// <summary>
    /// Reads a Win32-style boolean field stored as a 32-bit integer.
    /// </summary>
    public bool ReadBool32(uint structAddress, string structName, string fieldName) =>
        _memoryAccessor.ReadBool32(ResolveAddress(structAddress, structName, fieldName));

    /// <summary>
    /// Writes a raw byte field.
    /// </summary>
    public void WriteByte(uint structAddress, string structName, string fieldName, byte value) =>
        _memoryAccessor.WriteByte(ResolveAddress(structAddress, structName, fieldName), value);

    /// <summary>
    /// Writes a 32-bit signed integer field.
    /// </summary>
    public void WriteInt32(uint structAddress, string structName, string fieldName, int value) =>
        _memoryAccessor.WriteInt32(ResolveAddress(structAddress, structName, fieldName), value);

    /// <summary>
    /// Writes a 32-bit unsigned integer field.
    /// </summary>
    public void WriteUInt32(uint structAddress, string structName, string fieldName, uint value) =>
        _memoryAccessor.WriteUInt32(ResolveAddress(structAddress, structName, fieldName), value);

    /// <summary>
    /// Writes a 32-bit x86 pointer field.
    /// </summary>
    public void WritePointer(uint structAddress, string structName, string fieldName, uint value) =>
        _memoryAccessor.WritePointer(ResolveAddress(structAddress, structName, fieldName), value);

    /// <summary>
    /// Writes a Win32-style boolean field stored as a 32-bit integer.
    /// </summary>
    public void WriteBool32(uint structAddress, string structName, string fieldName, bool value) =>
        _memoryAccessor.WriteBool32(ResolveAddress(structAddress, structName, fieldName), value);

    /// <summary>
    /// Reads <c>CLogin::m_nLoginStep</c> as the versioned enum.
    /// </summary>
    /// <remarks>
    /// The raw byte is cast directly to <see cref="LoginStep"/>. If the client reports an
    /// out-of-range value the cast succeeds and callers should treat undefined enum values
    /// defensively.
    /// </remarks>
    public LoginStep ReadLoginStep(uint cLoginAddress) =>
        (LoginStep)ReadByte(
            cLoginAddress,
            nameof(ClientStructs.Fields.CLogin),
            nameof(ClientStructs.Offsets.CLogin.LoginStep)
        );

    /// <summary>
    /// Reads <c>CWvsContext::m_nWorldID</c>.
    /// </summary>
    public int ReadWorldId(uint cwvsContextAddress) =>
        ReadInt32(
            cwvsContextAddress,
            nameof(ClientStructs.Fields.CWvsContext),
            nameof(ClientStructs.Offsets.CWvsContext.WorldId)
        );

    /// <summary>
    /// Reads <c>CWvsContext::m_nChannelID</c>.
    /// </summary>
    public int ReadChannelId(uint cwvsContextAddress) =>
        ReadInt32(
            cwvsContextAddress,
            nameof(ClientStructs.Fields.CWvsContext),
            nameof(ClientStructs.Offsets.CWvsContext.ChannelId)
        );

    /// <summary>
    /// Reads <c>CWvsContext::m_dwCharacterID</c>.
    /// </summary>
    public int ReadCharacterId(uint cwvsContextAddress) =>
        ReadInt32(
            cwvsContextAddress,
            nameof(ClientStructs.Fields.CWvsContext),
            nameof(ClientStructs.Offsets.CWvsContext.CharacterId)
        );

    /// <summary>
    /// Reads <c>CLogin::m_bStepChanging</c> as a Win32-style boolean.
    /// </summary>
    public bool ReadStepChanging(uint cLoginAddress) =>
        ReadBool32(
            cLoginAddress,
            nameof(ClientStructs.Fields.CLogin),
            nameof(ClientStructs.Offsets.CLogin.StepChanging)
        );

    /// <summary>
    /// Reads <c>CLogin::m_bRequestSent</c> as a Win32-style boolean.
    /// </summary>
    public bool ReadRequestSent(uint cLoginAddress) =>
        ReadBool32(
            cLoginAddress,
            nameof(ClientStructs.Fields.CLogin),
            nameof(ClientStructs.Offsets.CLogin.RequestSent)
        );

    /// <summary>
    /// Reads <c>CLogin::m_pWorldItem</c>.
    /// </summary>
    public uint ReadLoginWorldItem(uint cLoginAddress) =>
        ReadPointer(cLoginAddress, nameof(ClientStructs.Fields.CLogin), nameof(ClientStructs.Offsets.CLogin.WorldItem));

    /// <summary>
    /// Writes <c>CLogin::m_pWorldItem</c>.
    /// </summary>
    public void WriteLoginWorldItem(uint cLoginAddress, uint worldItemAddress) =>
        WritePointer(
            cLoginAddress,
            nameof(ClientStructs.Fields.CLogin),
            nameof(ClientStructs.Offsets.CLogin.WorldItem),
            worldItemAddress
        );

    /// <summary>
    /// Reads <c>CUIWorldSelect::m_nWorldIdx</c>.
    /// </summary>
    public int ReadWorldIndex(uint cUiWorldSelectAddress) =>
        ReadInt32(
            cUiWorldSelectAddress,
            nameof(ClientStructs.Fields.CUIWorldSelect),
            nameof(ClientStructs.Offsets.CUIWorldSelect.WorldIdx)
        );

    /// <summary>
    /// Reads <c>CUIWorldSelect::m_pLogin</c>.
    /// </summary>
    public uint ReadWorldSelectLogin(uint cUiWorldSelectAddress) =>
        ReadPointer(
            cUiWorldSelectAddress,
            nameof(ClientStructs.Fields.CUIWorldSelect),
            nameof(ClientStructs.Offsets.CUIWorldSelect.Login)
        );

    /// <summary>
    /// Writes <c>CUIWorldSelect::m_nWorldIdx</c>.
    /// </summary>
    public void WriteWorldIndex(uint cUiWorldSelectAddress, int worldIndex) =>
        WriteInt32(
            cUiWorldSelectAddress,
            nameof(ClientStructs.Fields.CUIWorldSelect),
            nameof(ClientStructs.Offsets.CUIWorldSelect.WorldIdx),
            worldIndex
        );

    /// <summary>
    /// Reads <c>CUIChannelSelect::m_nSelect</c>.
    /// </summary>
    public int ReadChannelSelection(uint cUiChannelSelectAddress) =>
        ReadInt32(
            cUiChannelSelectAddress,
            nameof(ClientStructs.Fields.CUIChannelSelect),
            nameof(ClientStructs.Offsets.CUIChannelSelect.Select)
        );

    /// <summary>
    /// Reads <c>CUIChannelSelect::m_pLogin</c>.
    /// </summary>
    public uint ReadChannelSelectLogin(uint cUiChannelSelectAddress) =>
        ReadPointer(
            cUiChannelSelectAddress,
            nameof(ClientStructs.Fields.CUIChannelSelect),
            nameof(ClientStructs.Offsets.CUIChannelSelect.Login)
        );

    /// <summary>
    /// Writes <c>CUIChannelSelect::m_nSelect</c>.
    /// </summary>
    public void WriteChannelSelection(uint cUiChannelSelectAddress, int channelSelection) =>
        WriteInt32(
            cUiChannelSelectAddress,
            nameof(ClientStructs.Fields.CUIChannelSelect),
            nameof(ClientStructs.Offsets.CUIChannelSelect.Select),
            channelSelection
        );

    /// <summary>
    /// Reads <c>CUIChannelSelect::m_pWorldItem</c>.
    /// </summary>
    public uint ReadChannelSelectWorldItem(uint cUiChannelSelectAddress) =>
        ReadPointer(
            cUiChannelSelectAddress,
            nameof(ClientStructs.Fields.CUIChannelSelect),
            nameof(ClientStructs.Offsets.CUIChannelSelect.WorldItem)
        );

    /// <summary>
    /// Writes <c>CUIChannelSelect::m_pWorldItem</c>.
    /// </summary>
    public void WriteChannelSelectWorldItem(uint cUiChannelSelectAddress, uint worldItemAddress) =>
        WritePointer(
            cUiChannelSelectAddress,
            nameof(ClientStructs.Fields.CUIChannelSelect),
            nameof(ClientStructs.Offsets.CUIChannelSelect.WorldItem),
            worldItemAddress
        );

    /// <summary>
    /// Reads <c>CUIWorldSelect::m_pWorld</c>.
    /// </summary>
    public uint ReadWorldSelectWorldItem(uint cUiWorldSelectAddress) =>
        ReadPointer(
            cUiWorldSelectAddress,
            nameof(ClientStructs.Fields.CUIWorldSelect),
            nameof(ClientStructs.Offsets.CUIWorldSelect.World)
        );

    /// <summary>
    /// Writes <c>CUIWorldSelect::m_pWorld</c>.
    /// </summary>
    public void WriteWorldSelectWorldItem(uint cUiWorldSelectAddress, uint worldItemAddress) =>
        WritePointer(
            cUiWorldSelectAddress,
            nameof(ClientStructs.Fields.CUIWorldSelect),
            nameof(ClientStructs.Offsets.CUIWorldSelect.World),
            worldItemAddress
        );

    /// <summary>
    /// Computes the base address of one <c>WorldItem</c> entry inside a contiguous world-item array.
    /// </summary>
    public static uint GetWorldItemAddress(uint worldItemsBaseAddress, int index)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(index);
        return checked(worldItemsBaseAddress + (uint)(index * ClientStructs.Offsets.WorldItem.Stride));
    }

    /// <summary>
    /// Computes the base address of one <c>ChannelItem</c> entry inside a contiguous channel-item array.
    /// </summary>
    public static uint GetChannelItemAddress(uint channelItemsBaseAddress, int index)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(index);
        return checked(channelItemsBaseAddress + (uint)(index * ClientStructs.Offsets.ChannelItem.Stride));
    }

    /// <summary>
    /// Reads <c>WorldItem::nWorldID</c>.
    /// </summary>
    public int ReadWorldItemId(uint worldItemAddress) =>
        ReadInt32(worldItemAddress, nameof(ClientStructs.Fields.WorldItem), nameof(ClientStructs.Offsets.WorldItem.Id));

    /// <summary>
    /// Reads the world-item name payload pointer stored by the inline <c>ZXString&lt;char&gt;</c> field.
    /// </summary>
    public uint ReadWorldItemNamePointer(uint worldItemAddress) =>
        ReadPointer(
            worldItemAddress,
            nameof(ClientStructs.Fields.WorldItem),
            nameof(ClientStructs.Offsets.WorldItem.NamePtr)
        );

    /// <summary>
    /// Reads the decoded world-item name.
    /// </summary>
    public string ReadWorldItemName(uint worldItemAddress) =>
        _memoryAccessor.ReadZXString(ReadWorldItemNamePointer(worldItemAddress));

    /// <summary>
    /// Attempts to read the decoded world-item name.
    /// </summary>
    public bool TryReadWorldItemName(uint worldItemAddress, out string? name) =>
        _memoryAccessor.TryReadZXString(ReadWorldItemNamePointer(worldItemAddress), out name);

    /// <summary>
    /// Reads the pointer to the world-item channel array.
    /// </summary>
    public uint ReadWorldItemChannelItemsPointer(uint worldItemAddress) =>
        ReadPointer(
            worldItemAddress,
            nameof(ClientStructs.Fields.WorldItem),
            nameof(ClientStructs.Offsets.WorldItem.ChannelItemsPtr)
        );

    /// <summary>
    /// Reads one undecoded <c>WorldItem</c> entry.
    /// </summary>
    public RawWorldItem ReadRawWorldItem(uint worldItemAddress) =>
        new(
            worldItemAddress,
            ReadWorldItemId(worldItemAddress),
            ReadWorldItemNamePointer(worldItemAddress),
            ReadWorldItemChannelItemsPointer(worldItemAddress)
        );

    /// <summary>
    /// Reads one decoded <c>WorldItem</c> entry.
    /// </summary>
    public ResolvedWorldItem ReadWorldItem(uint worldItemAddress) =>
        DecodeWorldItem(ReadRawWorldItem(worldItemAddress));

    /// <summary>
    /// Decodes one previously read <c>WorldItem</c> entry.
    /// </summary>
    public ResolvedWorldItem DecodeWorldItem(RawWorldItem worldItem) =>
        new(
            worldItem.Address,
            worldItem.Id,
            worldItem.NamePointer,
            _memoryAccessor.TryReadZXString(worldItem.NamePointer, out string? name) ? name : null,
            worldItem.ChannelItemsPointer
        );

    /// <summary>
    /// Reads the channel-item name payload pointer stored by the inline <c>ZXString&lt;char&gt;</c> field.
    /// </summary>
    public uint ReadChannelItemNamePointer(uint channelItemAddress) =>
        ReadPointer(
            channelItemAddress,
            nameof(ClientStructs.Fields.ChannelItem),
            nameof(ClientStructs.Offsets.ChannelItem.NamePtr)
        );

    /// <summary>
    /// Reads the decoded channel-item name.
    /// </summary>
    public string ReadChannelItemName(uint channelItemAddress) =>
        _memoryAccessor.ReadZXString(ReadChannelItemNamePointer(channelItemAddress));

    /// <summary>
    /// Attempts to read the decoded channel-item name.
    /// </summary>
    public bool TryReadChannelItemName(uint channelItemAddress, out string? name) =>
        _memoryAccessor.TryReadZXString(ReadChannelItemNamePointer(channelItemAddress), out name);

    /// <summary>
    /// Reads <c>ChannelItem::nWorldID</c>.
    /// </summary>
    public int ReadChannelItemWorldId(uint channelItemAddress) =>
        ReadInt32(
            channelItemAddress,
            nameof(ClientStructs.Fields.ChannelItem),
            nameof(ClientStructs.Offsets.ChannelItem.WorldId)
        );

    /// <summary>
    /// Reads <c>ChannelItem::nChannelID</c>.
    /// </summary>
    public int ReadChannelItemId(uint channelItemAddress) =>
        ReadInt32(
            channelItemAddress,
            nameof(ClientStructs.Fields.ChannelItem),
            nameof(ClientStructs.Offsets.ChannelItem.ChannelId)
        );

    /// <summary>
    /// Reads the channel-item adult flag.
    /// </summary>
    public bool ReadChannelItemAdultFlag(uint channelItemAddress) =>
        ReadBool32(
            channelItemAddress,
            nameof(ClientStructs.Fields.ChannelItem),
            nameof(ClientStructs.Offsets.ChannelItem.AdultFlag)
        );

    /// <summary>
    /// Reads one undecoded <c>ChannelItem</c> entry.
    /// </summary>
    public RawChannelItem ReadRawChannelItem(uint channelItemAddress) =>
        new(
            channelItemAddress,
            ReadChannelItemNamePointer(channelItemAddress),
            ReadChannelItemWorldId(channelItemAddress),
            ReadChannelItemId(channelItemAddress),
            ReadChannelItemAdultFlag(channelItemAddress)
        );

    /// <summary>
    /// Reads one decoded <c>ChannelItem</c> entry.
    /// </summary>
    public ResolvedChannelItem ReadChannelItem(uint channelItemAddress) =>
        DecodeChannelItem(ReadRawChannelItem(channelItemAddress));

    /// <summary>
    /// Decodes one previously read <c>ChannelItem</c> entry.
    /// </summary>
    public ResolvedChannelItem DecodeChannelItem(RawChannelItem channelItem) =>
        new(
            channelItem.Address,
            channelItem.NamePointer,
            _memoryAccessor.TryReadZXString(channelItem.NamePointer, out string? name) ? name : null,
            channelItem.WorldId,
            channelItem.ChannelId,
            channelItem.AdultFlag
        );

    /// <summary>
    /// Decodes one previously read world selection.
    /// </summary>
    public ResolvedWorldSelection DecodeWorldSelection(RawWorldSelection worldSelection) =>
        new(worldSelection.UiAddress, worldSelection.WorldIndex, DecodeWorldItem(worldSelection.WorldItem));

    /// <summary>
    /// Decodes one previously read channel selection.
    /// </summary>
    public ResolvedChannelSelection DecodeChannelSelection(RawChannelSelection channelSelection) =>
        new(
            channelSelection.UiAddress,
            channelSelection.ChannelIndex,
            DecodeWorldItem(channelSelection.WorldItem),
            DecodeChannelItem(channelSelection.ChannelItem)
        );

    /// <summary>
    /// Decodes one previously read login-state snapshot.
    /// </summary>
    public ResolvedLoginState DecodeLoginState(RawLoginState state) =>
        new(
            state.LoginAddress,
            state.Step,
            state.StepChanging,
            state.RequestSent,
            state.SelectedWorldItemAddress,
            state.ContextWorldId,
            state.ContextChannelId,
            state.ContextCharacterId,
            state.WorldSelection is RawWorldSelection worldSelection ? DecodeWorldSelection(worldSelection) : null,
            state.ChannelSelection is RawChannelSelection channelSelection
                ? DecodeChannelSelection(channelSelection)
                : null
        );

    private StructField GetField(string structName, string fieldName)
    {
        if (_registry.TryGetField(structName, fieldName, out StructField field))
            return field;

        throw new KeyNotFoundException($"Field '{structName}.{fieldName}' is not registered.");
    }

    private uint ResolveAddress(uint structAddress, string structName, string fieldName)
    {
        StructField field = GetField(structName, fieldName);
        return checked(structAddress + (uint)field.Offset);
    }
}
