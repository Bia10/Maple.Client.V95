namespace Maple.Client.V95;

public static partial class ClientStructs
{
    public static partial class Fields
    {
        /// <summary>Pre-built <see cref="StructField"/> list for <c>CUIWorldSelect</c>.</summary>
        public static readonly IReadOnlyList<StructField> CUIWorldSelect =
        [
            new(nameof(CUIWorldSelect), nameof(Offsets.CUIWorldSelect.Login), Offsets.CUIWorldSelect.Login),
            new(nameof(CUIWorldSelect), nameof(Offsets.CUIWorldSelect.World), Offsets.CUIWorldSelect.World),
            new(nameof(CUIWorldSelect), nameof(Offsets.CUIWorldSelect.WorldIdx), Offsets.CUIWorldSelect.WorldIdx),
        ];
    }
}
