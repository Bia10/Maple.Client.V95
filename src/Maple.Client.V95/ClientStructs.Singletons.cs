using System.Collections.ObjectModel;

namespace Maple.Client.V95;

public static partial class ClientStructs
{
    /// <summary>
    /// Known v95 singleton pointer-table entries for diagnostics and reporting.
    /// </summary>
    public static readonly IReadOnlyDictionary<uint, string> Singletons = new ReadOnlyDictionary<uint, string>(
        new Dictionary<uint, string>
        {
            [Addresses.CUniqueModelessSingletonPtr] = "CUniqueModeless",
            [Addresses.CClientSocketSingletonPtr] = "CClientSocket",
            [Addresses.CWvsContextSingletonPtr] = "CWvsContext",
            [Addresses.CWvsAppSingletonPtr] = "CWvsApp",
            [Addresses.CUITitleSingletonPtr] = "CUITitle",
            [Addresses.CLicenseDlgSingletonPtr] = "CLicenseDlg",
            [Addresses.CConnectionNoticeDlgSingletonPtr] = "CConnectionNoticeDlg",
            [Addresses.CUIWorldSelectSingletonPtr] = "CUIWorldSelect",
            [Addresses.CUIChannelSelectSingletonPtr] = "CUIChannelSelect",
            [Addresses.CUIRecommendWorldSingletonPtr] = "CUIRecommendWorld",
            [Addresses.CUICharSelectSingletonPtr] = "CUICharSelect",
            [Addresses.CUICharDetailSingletonPtr] = "CUICharDetail",
            [Addresses.CUIAvatarSingletonPtr] = "CUIAvatar",
            [Addresses.CUICharDetailVacSingletonPtr] = "CUICharDetailVac",
            [Addresses.CUIAvatarVacSingletonPtr] = "CUIAvatarVac",
            [Addresses.CUINewCharRaceSelectSingletonPtr] = "CUINewCharRaceSelect",
            [Addresses.CUINewCharNameSelectCygnusSingletonPtr] = "CUINewCharNameSelectCygnus",
            [Addresses.CUINewCharNameSelectNormalSingletonPtr] = "CUINewCharNameSelectNormal",
            [Addresses.CUINewCharNameSelectAranSingletonPtr] = "CUINewCharNameSelectAran",
            [Addresses.CUINewCharNameSelectEvanSingletonPtr] = "CUINewCharNameSelectEvan",
            [Addresses.CUINewCharJobSelectSingletonPtr] = "CUINewCharJobSelect",
            [Addresses.CUINewCharAvatarSelectSingletonPtr] = "CUINewCharAvatarSelect",
            [Addresses.CLoginGradeWndSingletonPtr] = "CLoginGradeWnd",
            [Addresses.CUIGetUserInfoSingletonPtr] = "CUIGetUserInfo",
            [Addresses.CNmcoClientObjectSingletonPtr] = "CNmcoClientObject",
            [Addresses.CQuestManSingletonPtr] = "CQuestMan",
        }
    );
}
