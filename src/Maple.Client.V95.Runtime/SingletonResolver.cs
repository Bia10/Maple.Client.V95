using Maple.Memory;

namespace Maple.Client.V95.Runtime;

/// <summary>
/// Resolves known GMS v95 singleton pointer-table entries through a memory accessor.
/// </summary>
public sealed class SingletonResolver
{
    private readonly MemoryAccessor _memoryAccessor;
    private readonly IReadOnlyDictionary<uint, string> _singletonsByAddress;
    private readonly Dictionary<string, uint> _singletonsByName;

    /// <summary>
    /// Creates a singleton resolver over <paramref name="memoryAccessor"/>.
    /// </summary>
    public SingletonResolver(
        MemoryAccessor memoryAccessor,
        IReadOnlyDictionary<uint, string>? singletonsByAddress = null
    )
    {
        _memoryAccessor = memoryAccessor ?? throw new ArgumentNullException(nameof(memoryAccessor));
        _singletonsByAddress = singletonsByAddress ?? ClientStructs.Singletons;
        _singletonsByName = _singletonsByAddress.ToDictionary(
            static kv => kv.Value,
            static kv => kv.Key,
            StringComparer.Ordinal
        );
    }

    /// <summary>
    /// Attempts to resolve a singleton by pointer-table address.
    /// </summary>
    public bool TryResolve(uint pointerTableAddress, out ResolvedSingleton singleton)
    {
        if (!_singletonsByAddress.TryGetValue(pointerTableAddress, out string? name))
        {
            singleton = default;
            return false;
        }

        if (!_memoryAccessor.TryReadPointer(pointerTableAddress, out uint instanceAddress))
        {
            singleton = default;
            return false;
        }

        singleton = new ResolvedSingleton(name, pointerTableAddress, instanceAddress);
        return true;
    }

    /// <summary>
    /// Attempts to resolve the active <c>CWvsContext</c> instance address.
    /// </summary>
    public bool TryResolveContext(out uint instanceAddress) =>
        _memoryAccessor.TryReadPointer(ClientStructs.Addresses.CWvsContextSingletonPtr, out instanceAddress);

    /// <summary>
    /// Attempts to resolve the active <c>CUIWorldSelect</c> instance address.
    /// </summary>
    public bool TryResolveWorldSelect(out uint instanceAddress) =>
        _memoryAccessor.TryReadPointer(ClientStructs.Addresses.CUIWorldSelectSingletonPtr, out instanceAddress);

    /// <summary>
    /// Attempts to resolve the active <c>CUIChannelSelect</c> instance address.
    /// </summary>
    public bool TryResolveChannelSelect(out uint instanceAddress) =>
        _memoryAccessor.TryReadPointer(ClientStructs.Addresses.CUIChannelSelectSingletonPtr, out instanceAddress);

    /// <summary>
    /// Resolves the active <c>CWvsContext</c> instance address or throws if the pointer entry is unreadable.
    /// </summary>
    public uint ResolveContext() =>
        ResolveInstanceAddress(ClientStructs.Addresses.CWvsContextSingletonPtr, "CWvsContext");

    /// <summary>
    /// Resolves the active <c>CUIWorldSelect</c> instance address or throws if the pointer entry is unreadable.
    /// </summary>
    public uint ResolveWorldSelect() =>
        ResolveInstanceAddress(ClientStructs.Addresses.CUIWorldSelectSingletonPtr, "CUIWorldSelect");

    /// <summary>
    /// Resolves the active <c>CUIChannelSelect</c> instance address or throws if the pointer entry is unreadable.
    /// </summary>
    public uint ResolveChannelSelect() =>
        ResolveInstanceAddress(ClientStructs.Addresses.CUIChannelSelectSingletonPtr, "CUIChannelSelect");

    /// <summary>
    /// Gets whether the <c>CUIWorldSelect</c> singleton currently points at one live instance.
    /// </summary>
    public bool HasActiveWorldSelect() => TryResolveWorldSelect(out uint instanceAddress) && instanceAddress != 0;

    /// <summary>
    /// Gets whether the <c>CUIChannelSelect</c> singleton currently points at one live instance.
    /// </summary>
    public bool HasActiveChannelSelect() => TryResolveChannelSelect(out uint instanceAddress) && instanceAddress != 0;

    /// <summary>
    /// Resolves a singleton by pointer-table address or throws if it is unknown or unreadable.
    /// </summary>
    public ResolvedSingleton Resolve(uint pointerTableAddress)
    {
        if (TryResolve(pointerTableAddress, out ResolvedSingleton singleton))
            return singleton;

        throw new KeyNotFoundException($"Singleton pointer entry 0x{pointerTableAddress:X8} is unknown or unreadable.");
    }

    /// <summary>
    /// Attempts to resolve a singleton by symbolic name.
    /// </summary>
    public bool TryResolve(string singletonName, out ResolvedSingleton singleton)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(singletonName);

        if (!_singletonsByName.TryGetValue(singletonName, out uint pointerTableAddress))
        {
            singleton = default;
            return false;
        }

        return TryResolve(pointerTableAddress, out singleton);
    }

    /// <summary>
    /// Resolves a singleton by symbolic name or throws if it is unknown or unreadable.
    /// </summary>
    public ResolvedSingleton Resolve(string singletonName)
    {
        if (TryResolve(singletonName, out ResolvedSingleton singleton))
            return singleton;

        throw new KeyNotFoundException($"Singleton '{singletonName}' is unknown or unreadable.");
    }

    /// <summary>
    /// Attempts to resolve the pointer-table address for a known singleton name.
    /// </summary>
    public bool TryGetPointerTableAddress(string singletonName, out uint pointerTableAddress)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(singletonName);

        return _singletonsByName.TryGetValue(singletonName, out pointerTableAddress);
    }

    private uint ResolveInstanceAddress(uint pointerTableAddress, string singletonName)
    {
        if (_memoryAccessor.TryReadPointer(pointerTableAddress, out uint instanceAddress))
            return instanceAddress;

        throw new KeyNotFoundException($"Singleton '{singletonName}' is unknown or unreadable.");
    }
}
