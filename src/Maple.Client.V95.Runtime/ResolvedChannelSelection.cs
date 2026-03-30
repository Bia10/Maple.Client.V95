namespace Maple.Client.V95.Runtime;

/// <summary>
/// The current channel selection resolved from the v95 login UI state.
/// </summary>
public readonly record struct ResolvedChannelSelection(
    uint UiAddress,
    int ChannelIndex,
    ResolvedWorldItem WorldItem,
    ResolvedChannelItem ChannelItem
);
