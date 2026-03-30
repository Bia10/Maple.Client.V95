namespace Maple.Client.V95;

public static partial class ClientStructs
{
    public static partial class Fields
    {
        /// <summary>Pre-built <see cref="StructField"/> list for <c>CWvsApp</c>.</summary>
        public static readonly IReadOnlyList<StructField> CWvsApp =
        [
            new(nameof(CWvsApp), nameof(Offsets.CWvsApp.HWnd), Offsets.CWvsApp.HWnd),
            new(nameof(CWvsApp), nameof(Offsets.CWvsApp.MainThreadId), Offsets.CWvsApp.MainThreadId),
            new(nameof(CWvsApp), nameof(Offsets.CWvsApp.AutoConnect), Offsets.CWvsApp.AutoConnect),
            new(nameof(CWvsApp), nameof(Offsets.CWvsApp.TargetVersion), Offsets.CWvsApp.TargetVersion),
            new(nameof(CWvsApp), nameof(Offsets.CWvsApp.EnabledDx9), Offsets.CWvsApp.EnabledDx9),
            new(nameof(CWvsApp), nameof(Offsets.CWvsApp.WindowActive), Offsets.CWvsApp.WindowActive),
        ];
    }
}
