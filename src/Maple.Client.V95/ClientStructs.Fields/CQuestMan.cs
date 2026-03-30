namespace Maple.Client.V95;

public static partial class ClientStructs
{
    public static partial class Fields
    {
        /// <summary>Pre-built <see cref="StructField"/> list for <c>CQuestMan</c>.</summary>
        public static readonly IReadOnlyList<StructField> CQuestMan =
        [
            new(nameof(CQuestMan), nameof(Offsets.CQuestMan.WorldId), Offsets.CQuestMan.WorldId),
        ];
    }
}
