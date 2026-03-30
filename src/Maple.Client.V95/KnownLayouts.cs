namespace Maple.Client.V95;

/// <summary>
/// Pre-built client layouts for known MapleStory versions.
/// </summary>
public static class KnownLayouts
{
    /// <summary>
    /// StringPool static addresses for GMS v95 (<c>MapleStory.exe</c>, image base <c>0x400000</c>).
    /// </summary>
    public static readonly StringPoolAddresses GmsV95 = new()
    {
        ImageBase = 0x400000u,
        MsAString = 0xC5A878u,
        MsAKey = 0xB98830u,
        MsNKeySize = 0xB98840u,
        MsNSize = 0xB98844u,
    };
}
