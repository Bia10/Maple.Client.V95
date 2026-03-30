namespace Maple.Client.V95;

public static partial class ClientStructs
{
    public static partial class Fields
    {
        /// <summary>Pre-built <see cref="StructField"/> list for the <c>ChannelItem</c> struct.</summary>
        public static readonly IReadOnlyList<StructField> ChannelItem =
        [
            new(nameof(ChannelItem), nameof(Offsets.ChannelItem.NamePtr), Offsets.ChannelItem.NamePtr),
            new(nameof(ChannelItem), nameof(Offsets.ChannelItem.WorldId), Offsets.ChannelItem.WorldId),
            new(nameof(ChannelItem), nameof(Offsets.ChannelItem.ChannelId), Offsets.ChannelItem.ChannelId),
            new(nameof(ChannelItem), nameof(Offsets.ChannelItem.AdultFlag), Offsets.ChannelItem.AdultFlag),
        ];
    }
}
