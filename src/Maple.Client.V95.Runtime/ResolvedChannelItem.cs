namespace Maple.Client.V95.Runtime;

/// <summary>
/// One decoded GMS v95 <c>ChannelItem</c> entry.
/// </summary>
public readonly record struct ResolvedChannelItem(
    uint Address,
    uint NamePointer,
    string? Name,
    int WorldId,
    int ChannelId,
    bool AdultFlag
);
