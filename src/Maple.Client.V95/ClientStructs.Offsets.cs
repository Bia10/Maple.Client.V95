namespace Maple.Client.V95;

public static partial class ClientStructs
{
    /// <summary>Struct field byte offsets for the GMS v95 client.</summary>
    public static class Offsets
    {
        /// <summary>Field offsets for <c>CWvsContext</c>.</summary>
        public static class CWvsContext
        {
            /// <summary>Offset of <c>m_dwAccountID</c>.</summary>
            public const int AccountId = 0x2030;

            /// <summary>Offset of <c>m_nGender</c>.</summary>
            public const int Gender = 0x2034;

            /// <summary>Offset of <c>m_nGradeCode</c>.</summary>
            public const int GradeCode = 0x2038;

            /// <summary>Offset of <c>m_nSubGradeCode</c>.</summary>
            public const int SubGradeCode = 0x2044;

            /// <summary>Offset of <c>m_sEmailAccount</c>.</summary>
            public const int EmailAccount = 0x2050;

            /// <summary>Offset of <c>m_sNexonClubID</c>.</summary>
            public const int NexonClubId = 0x2054;

            /// <summary>Offset of <c>m_nCountryID</c>.</summary>
            public const int CountryId = 0x2058;

            /// <summary>Offset of the purchase experience flag.</summary>
            public const int PurchaseExp = 0x2059;

            /// <summary>Offset of <c>m_nWorldID</c>.</summary>
            public const int WorldId = 0x205C;

            /// <summary>Offset of <c>m_nChannelID</c>.</summary>
            public const int ChannelId = 0x2060;

            /// <summary>Offset of <c>m_nCharacterCount</c>.</summary>
            public const int CharacterCount = 0x20A0;

            /// <summary>Offset of <c>m_nSlotCount</c>.</summary>
            public const int SlotCount = 0x20A4;

            /// <summary>Offset of <c>m_ClientKey</c>.</summary>
            public const int ClientKey = 0x20A8;

            /// <summary>Offset of <c>m_dwCharacterID</c>.</summary>
            public const int CharacterId = 0x20B4;

            /// <summary>Offset of the guild data pointer.</summary>
            public const int Guild = 0x37C8;

            /// <summary>Offset of <c>m_bDirectionMode</c>.</summary>
            public const int DirectionMode = 0x3850;

            /// <summary>Offset of <c>m_bStandAloneMode</c>.</summary>
            public const int StandAloneMode = 0x3854;

            /// <summary>Offset of the show-UI flag.</summary>
            public const int ShowUi = 0x3F28;

            /// <summary>Offset of <c>m_sChannelName</c>.</summary>
            public const int ChannelName = 0x3F74;

            /// <summary>Offset of the adult-channel flag.</summary>
            public const int AdultChannel = 0x3F78;

            /// <summary>Offset of <c>m_nScreenWidth</c>.</summary>
            public const int ScreenWidth = 0x41B8;

            /// <summary>Offset of <c>m_nScreenHeight</c>.</summary>
            public const int ScreenHeight = 0x41BC;
        }

        /// <summary>Field offsets for <c>CLogin</c>.</summary>
        public static class CLogin
        {
            /// <summary>Offset of the connection dialog pointer.</summary>
            public const int ConnectionDlg = 0x148;

            /// <summary>Offset of <c>m_nCountCharacters</c>.</summary>
            public const int CountCharacters = 0x160;

            /// <summary>Offset of <c>m_nLoginStep</c>.</summary>
            public const int LoginStep = 0x1A4;

            /// <summary>Offset of <c>m_bStepChanging</c>.</summary>
            public const int StepChanging = 0x1A8;

            /// <summary>Offset of <c>m_bRequestSent</c>.</summary>
            public const int RequestSent = 0x1AC;

            /// <summary>Offset of the login options field.</summary>
            public const int LoginOpt = 0x1B0;

            /// <summary>Offset of <c>m_nSlotCount</c>.</summary>
            public const int SlotCount = 0x1B8;

            /// <summary>Offset of <c>m_pWorldItem</c>.</summary>
            public const int WorldItem = 0x1CC;

            /// <summary>Offset of the selected character index.</summary>
            public const int CharSelected = 0x1D0;

            /// <summary>Offset of <c>m_nLatestConnectedWorldID</c>.</summary>
            public const int LatestConnectedWorldId = 0x234;

            /// <summary>Offset of the currently selected race index.</summary>
            public const int CurSelectedRace = 0x240;

            /// <summary>Offset of the currently selected sub-job index.</summary>
            public const int CurSelectedSubJob = 0x244;
        }

        /// <summary>Field offsets for <c>CUIChannelSelect</c>.</summary>
        public static class CUIChannelSelect
        {
            /// <summary>Offset of the login pointer.</summary>
            public const int Login = 0xF4;

            /// <summary>Offset of the user-population display value.</summary>
            public const int UserPopulation = 0xF8;

            /// <summary>Offset of the selected channel index.</summary>
            public const int Select = 0xFC;

            /// <summary>Offset of the world-item pointer.</summary>
            public const int WorldItem = 0x100;

            /// <summary>Offset of the connection dialog pointer.</summary>
            public const int ConnectionDlg = 0x128;
        }

        /// <summary>Field offsets for <c>CUIWorldSelect</c>.</summary>
        public static class CUIWorldSelect
        {
            /// <summary>Offset of the login pointer.</summary>
            public const int Login = 0x94;

            /// <summary>Offset of the world pointer.</summary>
            public const int World = 0x98;

            /// <summary>Offset of <c>m_nWorldIdx</c>.</summary>
            public const int WorldIdx = 0x1D4;
        }

        /// <summary>Field offsets for <c>CQuestMan</c>.</summary>
        public static class CQuestMan
        {
            /// <summary>Offset of <c>m_nWorldID</c>.</summary>
            public const int WorldId = 0x64;
        }

        /// <summary>Field offsets for the <c>WorldItem</c> struct within <c>CLogin</c>.</summary>
        public static class WorldItem
        {
            /// <summary>Byte stride between consecutive world-item entries in the array.</summary>
            public const int Stride = 0x20;

            /// <summary>Offset of the world item ID field.</summary>
            public const int Id = 0x00;

            /// <summary>Offset of the world name string pointer.</summary>
            public const int NamePtr = 0x04;

            /// <summary>Offset of the channel-items array pointer.</summary>
            public const int ChannelItemsPtr = 0x1C;
        }

        /// <summary>Field offsets for the <c>ChannelItem</c> struct within <c>CUIChannelSelect</c>.</summary>
        public static class ChannelItem
        {
            /// <summary>Byte stride between consecutive channel-item entries in the array.</summary>
            public const int Stride = 0x14;

            /// <summary>Offset of the channel name string pointer.</summary>
            public const int NamePtr = 0x00;

            /// <summary>Offset of the world ID field.</summary>
            public const int WorldId = 0x08;

            /// <summary>Offset of the channel ID field.</summary>
            public const int ChannelId = 0x0C;

            /// <summary>Offset of the adult-channel flag.</summary>
            public const int AdultFlag = 0x10;
        }

        /// <summary>Field offsets for <c>CWvsApp</c>.</summary>
        public static class CWvsApp
        {
            /// <summary>Offset of the main window handle.</summary>
            public const int HWnd = 0x04;

            /// <summary>Offset of the main thread ID.</summary>
            public const int MainThreadId = 0x0C;

            /// <summary>Offset of the auto-connect flag.</summary>
            public const int AutoConnect = 0x3C;

            /// <summary>Offset of the target client version field.</summary>
            public const int TargetVersion = 0x54;

            /// <summary>Offset of the DirectX 9 enabled flag.</summary>
            public const int EnabledDx9 = 0x78;

            /// <summary>Offset of the window-active flag.</summary>
            public const int WindowActive = 0x88;
        }
    }
}
