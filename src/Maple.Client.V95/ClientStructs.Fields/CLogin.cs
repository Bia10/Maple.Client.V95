namespace Maple.Client.V95;

public static partial class ClientStructs
{
    public static partial class Fields
    {
        /// <summary>Pre-built <see cref="StructField"/> list for <c>CLogin</c>.</summary>
        public static readonly IReadOnlyList<StructField> CLogin =
        [
            new(nameof(CLogin), nameof(Offsets.CLogin.ConnectionDlg), Offsets.CLogin.ConnectionDlg),
            new(nameof(CLogin), nameof(Offsets.CLogin.CountCharacters), Offsets.CLogin.CountCharacters),
            new(nameof(CLogin), nameof(Offsets.CLogin.LoginStep), Offsets.CLogin.LoginStep),
            new(nameof(CLogin), nameof(Offsets.CLogin.StepChanging), Offsets.CLogin.StepChanging),
            new(nameof(CLogin), nameof(Offsets.CLogin.RequestSent), Offsets.CLogin.RequestSent),
            new(nameof(CLogin), nameof(Offsets.CLogin.LoginOpt), Offsets.CLogin.LoginOpt),
            new(nameof(CLogin), nameof(Offsets.CLogin.SlotCount), Offsets.CLogin.SlotCount),
            new(nameof(CLogin), nameof(Offsets.CLogin.WorldItem), Offsets.CLogin.WorldItem),
            new(nameof(CLogin), nameof(Offsets.CLogin.CharSelected), Offsets.CLogin.CharSelected),
            new(nameof(CLogin), nameof(Offsets.CLogin.LatestConnectedWorldId), Offsets.CLogin.LatestConnectedWorldId),
            new(nameof(CLogin), nameof(Offsets.CLogin.CurSelectedRace), Offsets.CLogin.CurSelectedRace),
            new(nameof(CLogin), nameof(Offsets.CLogin.CurSelectedSubJob), Offsets.CLogin.CurSelectedSubJob),
        ];
    }
}
