namespace Maple.Client.V95;

public static partial class ClientStructs
{
    public static partial class Fields
    {
        /// <summary>Pre-built <see cref="StructField"/> list for the <c>WorldItem</c> struct.</summary>
        public static readonly IReadOnlyList<StructField> WorldItem =
        [
            new(nameof(WorldItem), nameof(Offsets.WorldItem.Id), Offsets.WorldItem.Id),
            new(nameof(WorldItem), nameof(Offsets.WorldItem.NamePtr), Offsets.WorldItem.NamePtr),
            new(nameof(WorldItem), nameof(Offsets.WorldItem.ChannelItemsPtr), Offsets.WorldItem.ChannelItemsPtr),
        ];
    }
}
