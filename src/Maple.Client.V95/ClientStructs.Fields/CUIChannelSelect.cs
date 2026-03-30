namespace Maple.Client.V95;

public static partial class ClientStructs
{
    public static partial class Fields
    {
        /// <summary>Pre-built <see cref="StructField"/> list for <c>CUIChannelSelect</c>.</summary>
        public static readonly IReadOnlyList<StructField> CUIChannelSelect =
        [
            new(nameof(CUIChannelSelect), nameof(Offsets.CUIChannelSelect.Login), Offsets.CUIChannelSelect.Login),
            new(
                nameof(CUIChannelSelect),
                nameof(Offsets.CUIChannelSelect.UserPopulation),
                Offsets.CUIChannelSelect.UserPopulation
            ),
            new(nameof(CUIChannelSelect), nameof(Offsets.CUIChannelSelect.Select), Offsets.CUIChannelSelect.Select),
            new(
                nameof(CUIChannelSelect),
                nameof(Offsets.CUIChannelSelect.WorldItem),
                Offsets.CUIChannelSelect.WorldItem
            ),
            new(
                nameof(CUIChannelSelect),
                nameof(Offsets.CUIChannelSelect.ConnectionDlg),
                Offsets.CUIChannelSelect.ConnectionDlg
            ),
        ];
    }
}
