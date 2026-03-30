# Public API Reference

## Public API Reference

```csharp
[assembly: System.CLSCompliant(false)]
[assembly: System.Reflection.AssemblyMetadata("IsAotCompatible", "True")]
[assembly: System.Reflection.AssemblyMetadata("IsTrimmable", "True")]
[assembly: System.Reflection.AssemblyMetadata("RepositoryUrl", "https://github.com/Bia10/Maple.Client.V95/")]
[assembly: System.Resources.NeutralResourcesLanguage("en")]
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("Maple.Client.V95.Benchmarks")]
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("Maple.Client.V95.ComparisonBenchmarks")]
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("Maple.Client.V95.DocTest")]
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("Maple.Client.V95.Test")]
[assembly: System.Runtime.Versioning.TargetFramework(".NETCoreApp,Version=v10.0", FrameworkDisplayName=".NET 10.0")]
namespace Maple.Client.V95
{
    public static class ClientStructs
    {
        public static readonly Maple.Client.V95.StructFieldRegistry Registry;
        public static readonly System.Collections.Generic.IReadOnlyDictionary<uint, string> Singletons;
        public static class Addresses
        {
            public const uint CClientSocketSendPacketPtr = 13010860u;
            public const uint CClientSocketSingletonPtr = 12992612u;
            public const uint CConnectionNoticeDlgSingletonPtr = 13021600u;
            public const uint CLicenseDlgSingletonPtr = 13021596u;
            public const uint CLoginGradeWndSingletonPtr = 13021664u;
            public const uint CNmcoClientObjectSingletonPtr = 13021672u;
            public const uint CQuestManSingletonPtr = 13020008u;
            public const uint CUIAvatarSingletonPtr = 13021624u;
            public const uint CUIAvatarVacSingletonPtr = 13021632u;
            public const uint CUIChannelSelectSingletonPtr = 13021608u;
            public const uint CUICharDetailSingletonPtr = 13021620u;
            public const uint CUICharDetailVacSingletonPtr = 13021628u;
            public const uint CUICharSelectSingletonPtr = 13021616u;
            public const uint CUIGetUserInfoSingletonPtr = 13021668u;
            public const uint CUINewCharAvatarSelectSingletonPtr = 13021660u;
            public const uint CUINewCharJobSelectSingletonPtr = 13021656u;
            public const uint CUINewCharNameSelectAranSingletonPtr = 13021648u;
            public const uint CUINewCharNameSelectCygnusSingletonPtr = 13021640u;
            public const uint CUINewCharNameSelectEvanSingletonPtr = 13021652u;
            public const uint CUINewCharNameSelectNormalSingletonPtr = 13021644u;
            public const uint CUINewCharRaceSelectSingletonPtr = 13021636u;
            public const uint CUIRecommendWorldSingletonPtr = 13021612u;
            public const uint CUITitleSingletonPtr = 13021592u;
            public const uint CUIWorldSelectSingletonPtr = 13021604u;
            public const uint CUniqueModelessSingletonPtr = 12992608u;
            public const uint CWvsAppSingletonPtr = 12993300u;
            public const uint CWvsContextSingletonPtr = 12992616u;
            public const uint SendLoginPacketFunc = 6143728u;
            public const uint SetWorldInfoFunc = 10355360u;
        }
        public static class Fields
        {
            public static readonly System.Collections.Generic.IReadOnlyList<Maple.Client.V95.StructField> CLogin;
            public static readonly System.Collections.Generic.IReadOnlyList<Maple.Client.V95.StructField> CQuestMan;
            public static readonly System.Collections.Generic.IReadOnlyList<Maple.Client.V95.StructField> CUIChannelSelect;
            public static readonly System.Collections.Generic.IReadOnlyList<Maple.Client.V95.StructField> CUIWorldSelect;
            public static readonly System.Collections.Generic.IReadOnlyList<Maple.Client.V95.StructField> CWvsApp;
            public static readonly System.Collections.Generic.IReadOnlyList<Maple.Client.V95.StructField> CWvsContext;
            public static readonly System.Collections.Generic.IReadOnlyList<Maple.Client.V95.StructField> ChannelItem;
            public static readonly System.Collections.Generic.IReadOnlyList<Maple.Client.V95.StructField> WorldItem;
        }
        public static class Offsets
        {
            public static class CLogin
            {
                public const int CharSelected = 464;
                public const int ConnectionDlg = 328;
                public const int CountCharacters = 352;
                public const int CurSelectedRace = 576;
                public const int CurSelectedSubJob = 580;
                public const int LatestConnectedWorldId = 564;
                public const int LoginOpt = 432;
                public const int LoginStep = 420;
                public const int RequestSent = 428;
                public const int SlotCount = 440;
                public const int StepChanging = 424;
                public const int WorldItem = 460;
            }
            public static class CQuestMan
            {
                public const int WorldId = 100;
            }
            public static class CUIChannelSelect
            {
                public const int ConnectionDlg = 296;
                public const int Login = 244;
                public const int Select = 252;
                public const int UserPopulation = 248;
                public const int WorldItem = 256;
            }
            public static class CUIWorldSelect
            {
                public const int Login = 148;
                public const int World = 152;
                public const int WorldIdx = 468;
            }
            public static class CWvsApp
            {
                public const int AutoConnect = 60;
                public const int EnabledDx9 = 120;
                public const int HWnd = 4;
                public const int MainThreadId = 12;
                public const int TargetVersion = 84;
                public const int WindowActive = 136;
            }
            public static class CWvsContext
            {
                public const int AccountId = 8240;
                public const int AdultChannel = 16248;
                public const int ChannelId = 8288;
                public const int ChannelName = 16244;
                public const int CharacterCount = 8352;
                public const int CharacterId = 8372;
                public const int ClientKey = 8360;
                public const int CountryId = 8280;
                public const int DirectionMode = 14416;
                public const int EmailAccount = 8272;
                public const int Gender = 8244;
                public const int GradeCode = 8248;
                public const int Guild = 14280;
                public const int NexonClubId = 8276;
                public const int PurchaseExp = 8281;
                public const int ScreenHeight = 16828;
                public const int ScreenWidth = 16824;
                public const int ShowUi = 16168;
                public const int SlotCount = 8356;
                public const int StandAloneMode = 14420;
                public const int SubGradeCode = 8260;
                public const int WorldId = 8284;
            }
            public static class ChannelItem
            {
                public const int AdultFlag = 16;
                public const int ChannelId = 12;
                public const int NamePtr = 0;
                public const int Stride = 20;
                public const int WorldId = 8;
            }
            public static class WorldItem
            {
                public const int ChannelItemsPtr = 28;
                public const int Id = 0;
                public const int NamePtr = 4;
                public const int Stride = 32;
            }
        }
    }
    public static class KnownLayouts
    {
        public static readonly Maple.Client.V95.StringPoolAddresses GmsV95;
    }
    public enum LoginStep : byte
    {
        Title = 0,
        SelectWorld = 1,
        SelectCharacter = 2,
        NewCharacter = 3,
        NewCharacterName = 4,
        Vac = 5,
    }
    public readonly struct StringPoolAddresses : System.IEquatable<Maple.Client.V95.StringPoolAddresses>
    {
        public required uint ImageBase { get; init; }
        public required uint MsAKey { get; init; }
        public required uint MsAString { get; init; }
        public required uint MsNKeySize { get; init; }
        public required uint MsNSize { get; init; }
    }
    public readonly struct StructField : System.IEquatable<Maple.Client.V95.StructField>
    {
        public StructField(string StructName, string FieldName, int Offset) { }
        public string FieldName { get; init; }
        public string Key { get; }
        public int Offset { get; init; }
        public string StructName { get; init; }
    }
    public sealed class StructFieldRegistry
    {
        public StructFieldRegistry(System.Collections.Generic.IEnumerable<System.Collections.Generic.IEnumerable<Maple.Client.V95.StructField>> structFields) { }
        public System.Collections.Generic.IEnumerable<string> StructNames { get; }
        public System.Collections.Generic.IReadOnlyDictionary<string, Maple.Client.V95.StructField> GetFields(string structName) { }
        public bool TryGetField(string structName, string fieldName, out Maple.Client.V95.StructField field) { }
        public static Maple.Client.V95.StructFieldRegistry Create(params System.Collections.Generic.IEnumerable<Maple.Client.V95.StructField>[] structFields) { }
    }
}
```
