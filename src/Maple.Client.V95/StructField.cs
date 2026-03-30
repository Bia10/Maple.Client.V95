namespace Maple.Client.V95;

/// <summary>
/// Describes one named field within a version-specific client struct.
/// </summary>
public readonly record struct StructField(string StructName, string FieldName, int Offset)
{
    /// <summary>
    /// Fully qualified registry key in the form <c>StructName.FieldName</c>.
    /// </summary>
    public string Key => $"{StructName}.{FieldName}";
}
