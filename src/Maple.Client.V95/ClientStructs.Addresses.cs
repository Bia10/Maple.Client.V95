namespace Maple.Client.V95;

public static partial class ClientStructs
{
    /// <summary>Static address constants for the GMS v95 client.</summary>
    public static class Addresses
    {
        /// <summary>Pointer-table address for the <c>CUniqueModelessSingleton</c> singleton.</summary>
        public const uint CUniqueModelessSingletonPtr = 0x00C64060;

        /// <summary>Pointer-table address for the <c>CClientSocket</c> singleton.</summary>
        public const uint CClientSocketSingletonPtr = 0x00C64064;

        /// <summary>Pointer-table address for the <c>CWvsContext</c> singleton.</summary>
        public const uint CWvsContextSingletonPtr = 0x00C64068;

        /// <summary>Pointer-table address for the <c>CWvsApp</c> singleton.</summary>
        public const uint CWvsAppSingletonPtr = 0x00C64314;

        /// <summary>Pointer-table address for the <c>CUITitle</c> singleton.</summary>
        public const uint CUITitleSingletonPtr = 0x00C6B198;

        /// <summary>Pointer-table address for the <c>CLicenseDlg</c> singleton.</summary>
        public const uint CLicenseDlgSingletonPtr = 0x00C6B19C;

        /// <summary>Pointer-table address for the <c>CConnectionNoticeDlg</c> singleton.</summary>
        public const uint CConnectionNoticeDlgSingletonPtr = 0x00C6B1A0;

        /// <summary>Pointer-table address for the <c>CUIWorldSelect</c> singleton.</summary>
        public const uint CUIWorldSelectSingletonPtr = 0x00C6B1A4;

        /// <summary>Pointer-table address for the <c>CUIChannelSelect</c> singleton.</summary>
        public const uint CUIChannelSelectSingletonPtr = 0x00C6B1A8;

        /// <summary>Pointer-table address for the <c>CUIRecommendWorld</c> singleton.</summary>
        public const uint CUIRecommendWorldSingletonPtr = 0x00C6B1AC;

        /// <summary>Pointer-table address for the <c>CUICharSelect</c> singleton.</summary>
        public const uint CUICharSelectSingletonPtr = 0x00C6B1B0;

        /// <summary>Pointer-table address for the <c>CUICharDetail</c> singleton.</summary>
        public const uint CUICharDetailSingletonPtr = 0x00C6B1B4;

        /// <summary>Pointer-table address for the <c>CUIAvatar</c> singleton.</summary>
        public const uint CUIAvatarSingletonPtr = 0x00C6B1B8;

        /// <summary>Pointer-table address for the VAC variant of <c>CUICharDetail</c>.</summary>
        public const uint CUICharDetailVacSingletonPtr = 0x00C6B1BC;

        /// <summary>Pointer-table address for the VAC variant of <c>CUIAvatar</c>.</summary>
        public const uint CUIAvatarVacSingletonPtr = 0x00C6B1C0;

        /// <summary>Pointer-table address for the <c>CUINewCharRaceSelect</c> singleton.</summary>
        public const uint CUINewCharRaceSelectSingletonPtr = 0x00C6B1C4;

        /// <summary>Pointer-table address for the Cygnus variant of <c>CUINewCharNameSelect</c>.</summary>
        public const uint CUINewCharNameSelectCygnusSingletonPtr = 0x00C6B1C8;

        /// <summary>Pointer-table address for the normal variant of <c>CUINewCharNameSelect</c>.</summary>
        public const uint CUINewCharNameSelectNormalSingletonPtr = 0x00C6B1CC;

        /// <summary>Pointer-table address for the Aran variant of <c>CUINewCharNameSelect</c>.</summary>
        public const uint CUINewCharNameSelectAranSingletonPtr = 0x00C6B1D0;

        /// <summary>Pointer-table address for the Evan variant of <c>CUINewCharNameSelect</c>.</summary>
        public const uint CUINewCharNameSelectEvanSingletonPtr = 0x00C6B1D4;

        /// <summary>Pointer-table address for the <c>CUINewCharJobSelect</c> singleton.</summary>
        public const uint CUINewCharJobSelectSingletonPtr = 0x00C6B1D8;

        /// <summary>Pointer-table address for the <c>CUINewCharAvatarSelect</c> singleton.</summary>
        public const uint CUINewCharAvatarSelectSingletonPtr = 0x00C6B1DC;

        /// <summary>Pointer-table address for the <c>CLoginGradeWnd</c> singleton.</summary>
        public const uint CLoginGradeWndSingletonPtr = 0x00C6B1E0;

        /// <summary>Pointer-table address for the <c>CUIGetUserInfo</c> singleton.</summary>
        public const uint CUIGetUserInfoSingletonPtr = 0x00C6B1E4;

        /// <summary>Pointer-table address for the <c>CNmcoClientObject</c> singleton.</summary>
        public const uint CNmcoClientObjectSingletonPtr = 0x00C6B1E8;

        /// <summary>Pointer-table address for the <c>CQuestMan</c> singleton.</summary>
        public const uint CQuestManSingletonPtr = 0x00C6AB68;

        /// <summary>Pointer-table address of the <c>CClientSocket</c> send-packet function pointer.</summary>
        public const uint CClientSocketSendPacketPtr = 0x00C687AC;

        /// <summary>Address of the <c>SendLoginPacket</c> function.</summary>
        public const uint SendLoginPacketFunc = 0x005DBEF0;

        /// <summary>Address of the <c>SetWorldInfo</c> function.</summary>
        public const uint SetWorldInfoFunc = 0x009E02A0;
    }
}
