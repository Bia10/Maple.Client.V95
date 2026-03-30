namespace Maple.Client.V95.Runtime;

/// <summary>
/// One resolved v95 singleton entry.
/// </summary>
public readonly record struct ResolvedSingleton(string Name, uint PointerTableAddress, uint InstanceAddress);
