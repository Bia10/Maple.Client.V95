namespace Maple.Client.V95.Runtime;

/// <summary>
/// One undecoded GMS v95 <c>ChannelItem</c> entry.
/// </summary>
public readonly record struct RawChannelItem(
    uint Address,
    uint NamePointer,
    int WorldId,
    int ChannelId,
    bool AdultFlag
);
