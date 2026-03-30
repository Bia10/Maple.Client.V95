namespace Maple.Client.V95;

public static partial class ClientStructs
{
    /// <summary>
    /// Structured registry over the known GMS v95 client field offsets.
    /// </summary>
    public static readonly StructFieldRegistry Registry = StructFieldRegistry.Create(
        Fields.CWvsContext,
        Fields.CLogin,
        Fields.CUIChannelSelect,
        Fields.CUIWorldSelect,
        Fields.CQuestMan,
        Fields.WorldItem,
        Fields.ChannelItem,
        Fields.CWvsApp
    );

    public static partial class Fields;
}
