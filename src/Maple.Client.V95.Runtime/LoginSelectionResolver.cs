using Maple.Memory;

namespace Maple.Client.V95.Runtime;

/// <summary>
/// Resolves the active world and channel selections from the v95 login UI singletons.
/// </summary>
public sealed class LoginSelectionResolver
{
    private readonly FieldAccessor _fieldAccessor;
    private readonly SingletonResolver _singletonResolver;

    /// <summary>
    /// Creates a login selection resolver over <paramref name="memoryAccessor"/>.
    /// </summary>
    public LoginSelectionResolver(MemoryAccessor memoryAccessor, StructFieldRegistry? registry = null)
    {
        ArgumentNullException.ThrowIfNull(memoryAccessor);
        _fieldAccessor = new FieldAccessor(memoryAccessor, registry);
        _singletonResolver = new SingletonResolver(memoryAccessor);
    }

    /// <summary>
    /// Creates a login selection resolver over existing field and singleton helpers.
    /// </summary>
    public LoginSelectionResolver(FieldAccessor fieldAccessor, SingletonResolver singletonResolver)
    {
        _fieldAccessor = fieldAccessor ?? throw new ArgumentNullException(nameof(fieldAccessor));
        _singletonResolver = singletonResolver ?? throw new ArgumentNullException(nameof(singletonResolver));
    }

    /// <summary>
    /// Resolves the current world-selection UI state without string decoding.
    /// </summary>
    public RawWorldSelection ResolveRawWorldSelection()
    {
        uint worldSelectAddress = _singletonResolver.ResolveWorldSelect();
        return ResolveRawWorldSelection(worldSelectAddress);
    }

    internal RawWorldSelection ResolveRawWorldSelection(uint worldSelectAddress)
    {
        int worldIndex = _fieldAccessor.ReadWorldIndex(worldSelectAddress);
        uint worldItemAddress = _fieldAccessor.ReadWorldSelectWorldItem(worldSelectAddress);
        RawWorldItem worldItem = _fieldAccessor.ReadRawWorldItem(worldItemAddress);
        return new(worldSelectAddress, worldIndex, worldItem);
    }

    internal bool TryResolveRawWorldSelection(uint worldSelectAddress, out RawWorldSelection selection)
    {
        try
        {
            selection = ResolveRawWorldSelection(worldSelectAddress);
            return true;
        }
        catch (InvalidOperationException)
        {
            selection = default;
            return false;
        }
    }

    /// <summary>
    /// Resolves the current world-selection UI state.
    /// </summary>
    public ResolvedWorldSelection ResolveWorldSelection() =>
        _fieldAccessor.DecodeWorldSelection(ResolveRawWorldSelection());

    /// <summary>
    /// Resolves the current channel-selection UI state without string decoding.
    /// </summary>
    public RawChannelSelection ResolveRawChannelSelection()
    {
        uint channelSelectAddress = _singletonResolver.ResolveChannelSelect();
        return ResolveRawChannelSelection(channelSelectAddress);
    }

    internal RawChannelSelection ResolveRawChannelSelection(uint channelSelectAddress)
    {
        int channelIndex = _fieldAccessor.ReadChannelSelection(channelSelectAddress);
        uint worldItemAddress = _fieldAccessor.ReadChannelSelectWorldItem(channelSelectAddress);
        RawWorldItem worldItem = _fieldAccessor.ReadRawWorldItem(worldItemAddress);
        uint channelItemAddress = FieldAccessor.GetChannelItemAddress(worldItem.ChannelItemsPointer, channelIndex);
        RawChannelItem channelItem = _fieldAccessor.ReadRawChannelItem(channelItemAddress);
        return new(channelSelectAddress, channelIndex, worldItem, channelItem);
    }

    internal bool TryResolveRawChannelSelection(uint channelSelectAddress, out RawChannelSelection selection)
    {
        try
        {
            selection = ResolveRawChannelSelection(channelSelectAddress);
            return true;
        }
        catch (InvalidOperationException)
        {
            selection = default;
            return false;
        }
    }

    /// <summary>
    /// Resolves the current channel-selection UI state.
    /// </summary>
    public ResolvedChannelSelection ResolveChannelSelection() =>
        _fieldAccessor.DecodeChannelSelection(ResolveRawChannelSelection());

    /// <summary>
    /// Updates the current world-selection UI state and returns the raw selection after the write.
    /// </summary>
    public RawWorldSelection SelectWorldRaw(uint worldItemAddress, int worldIndex)
    {
        uint worldSelectAddress = _singletonResolver.ResolveWorldSelect();
        WriteWorldSelection(worldSelectAddress, worldItemAddress, worldIndex);

        return ResolveRawWorldSelection(worldSelectAddress);
    }

    internal void WriteWorldSelection(uint worldItemAddress, int worldIndex)
    {
        uint worldSelectAddress = _singletonResolver.ResolveWorldSelect();
        WriteWorldSelection(worldSelectAddress, worldItemAddress, worldIndex);
    }

    /// <summary>
    /// Updates the current world-selection UI state and returns the resolved selection after the write.
    /// </summary>
    public ResolvedWorldSelection SelectWorld(uint worldItemAddress, int worldIndex) =>
        _fieldAccessor.DecodeWorldSelection(SelectWorldRaw(worldItemAddress, worldIndex));

    /// <summary>
    /// Updates the current channel-selection UI state and returns the raw selection after the write.
    /// </summary>
    public RawChannelSelection SelectChannelRaw(uint worldItemAddress, int channelIndex)
    {
        uint channelSelectAddress = _singletonResolver.ResolveChannelSelect();
        WriteChannelSelection(channelSelectAddress, worldItemAddress, channelIndex);

        return ResolveRawChannelSelection(channelSelectAddress);
    }

    internal void WriteChannelSelection(uint worldItemAddress, int channelIndex)
    {
        uint channelSelectAddress = _singletonResolver.ResolveChannelSelect();
        WriteChannelSelection(channelSelectAddress, worldItemAddress, channelIndex);
    }

    /// <summary>
    /// Updates the current channel-selection UI state and returns the resolved selection after the write.
    /// </summary>
    public ResolvedChannelSelection SelectChannel(uint worldItemAddress, int channelIndex) =>
        _fieldAccessor.DecodeChannelSelection(SelectChannelRaw(worldItemAddress, channelIndex));

    internal bool TryResolveRawWorldItem(uint worldItemAddress, out RawWorldItem worldItem)
    {
        if (worldItemAddress == 0)
        {
            worldItem = default;
            return false;
        }

        try
        {
            worldItem = _fieldAccessor.ReadRawWorldItem(worldItemAddress);
            return true;
        }
        catch (InvalidOperationException)
        {
            worldItem = default;
            return false;
        }
    }

    internal bool TryResolveRawChannelItem(uint worldItemAddress, int channelIndex, out RawChannelItem channelItem)
    {
        if (worldItemAddress == 0)
        {
            channelItem = default;
            return false;
        }

        try
        {
            RawWorldItem worldItem = _fieldAccessor.ReadRawWorldItem(worldItemAddress);
            uint channelItemAddress = FieldAccessor.GetChannelItemAddress(worldItem.ChannelItemsPointer, channelIndex);
            channelItem = _fieldAccessor.ReadRawChannelItem(channelItemAddress);
            return true;
        }
        catch (InvalidOperationException)
        {
            channelItem = default;
            return false;
        }
    }

    private void WriteWorldSelection(uint worldSelectAddress, uint worldItemAddress, int worldIndex)
    {
        _fieldAccessor.WriteWorldIndex(worldSelectAddress, worldIndex);
        _fieldAccessor.WriteWorldSelectWorldItem(worldSelectAddress, worldItemAddress);

        uint cLoginAddress = _fieldAccessor.ReadWorldSelectLogin(worldSelectAddress);
        if (cLoginAddress != 0)
            _fieldAccessor.WriteLoginWorldItem(cLoginAddress, worldItemAddress);
    }

    private void WriteChannelSelection(uint channelSelectAddress, uint worldItemAddress, int channelIndex)
    {
        _fieldAccessor.WriteChannelSelection(channelSelectAddress, channelIndex);
        _fieldAccessor.WriteChannelSelectWorldItem(channelSelectAddress, worldItemAddress);

        uint cLoginAddress = _fieldAccessor.ReadChannelSelectLogin(channelSelectAddress);
        if (cLoginAddress != 0)
            _fieldAccessor.WriteLoginWorldItem(cLoginAddress, worldItemAddress);
    }
}
