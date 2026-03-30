namespace Maple.Client.V95.Runtime;

/// <summary>
/// One decoded GMS v95 <c>WorldItem</c> entry.
/// </summary>
public readonly record struct ResolvedWorldItem(
    uint Address,
    int Id,
    uint NamePointer,
    string? Name,
    uint ChannelItemsPointer
);
