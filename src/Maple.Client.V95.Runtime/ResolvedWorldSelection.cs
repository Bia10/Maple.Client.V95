namespace Maple.Client.V95.Runtime;

/// <summary>
/// The current world selection resolved from the v95 login UI state.
/// </summary>
public readonly record struct ResolvedWorldSelection(uint UiAddress, int WorldIndex, ResolvedWorldItem WorldItem);
