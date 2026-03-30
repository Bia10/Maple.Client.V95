namespace Maple.Client.V95.Runtime;

/// <summary>
/// The current channel selection resolved from the v95 login UI state without string decoding.
/// </summary>
public readonly record struct RawChannelSelection(
    uint UiAddress,
    int ChannelIndex,
    RawWorldItem WorldItem,
    RawChannelItem ChannelItem
);
