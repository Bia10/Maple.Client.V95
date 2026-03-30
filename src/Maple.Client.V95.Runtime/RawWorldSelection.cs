namespace Maple.Client.V95.Runtime;

/// <summary>
/// The current world selection resolved from the v95 login UI state without string decoding.
/// </summary>
public readonly record struct RawWorldSelection(uint UiAddress, int WorldIndex, RawWorldItem WorldItem);
