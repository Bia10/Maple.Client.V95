namespace Maple.Client.V95.Runtime;

/// <summary>
/// One undecoded GMS v95 <c>WorldItem</c> entry.
/// </summary>
public readonly record struct RawWorldItem(uint Address, int Id, uint NamePointer, uint ChannelItemsPointer);
