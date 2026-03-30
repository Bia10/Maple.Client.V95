using System.Collections.ObjectModel;

namespace Maple.Client.V95;

/// <summary>
/// Read-only lookup surface for version-specific client struct fields.
/// </summary>
public sealed class StructFieldRegistry
{
    private readonly IReadOnlyDictionary<string, IReadOnlyDictionary<string, StructField>> _fieldsByStruct;

    /// <summary>
    /// Creates a registry from grouped field definitions.
    /// </summary>
    public StructFieldRegistry(IEnumerable<IEnumerable<StructField>> structFields)
    {
        ArgumentNullException.ThrowIfNull(structFields);

        var fieldsByStruct = new Dictionary<string, IReadOnlyDictionary<string, StructField>>(StringComparer.Ordinal);

        foreach (IEnumerable<StructField> group in structFields)
        {
            ArgumentNullException.ThrowIfNull(group);

            Dictionary<string, StructField>? fieldMap = null;
            string? structName = null;

            foreach (StructField field in group)
            {
                structName ??= field.StructName;

                if (!StringComparer.Ordinal.Equals(structName, field.StructName))
                {
                    throw new ArgumentException(
                        $"Struct field group contains mixed struct names '{structName}' and '{field.StructName}'.",
                        nameof(structFields)
                    );
                }

                fieldMap ??= new Dictionary<string, StructField>(StringComparer.Ordinal);

                if (!fieldMap.TryAdd(field.FieldName, field))
                {
                    throw new ArgumentException(
                        $"Duplicate field '{field.Key}' was added to the registry.",
                        nameof(structFields)
                    );
                }
            }

            if (structName is null || fieldMap is null)
                continue;

            if (!fieldsByStruct.TryAdd(structName, new ReadOnlyDictionary<string, StructField>(fieldMap)))
            {
                throw new ArgumentException(
                    $"Duplicate struct '{structName}' was added to the registry.",
                    nameof(structFields)
                );
            }
        }

        _fieldsByStruct = new ReadOnlyDictionary<string, IReadOnlyDictionary<string, StructField>>(fieldsByStruct);
    }

    /// <summary>
    /// Known struct names in this registry.
    /// </summary>
    public IEnumerable<string> StructNames => _fieldsByStruct.Keys;

    /// <summary>
    /// Returns the fields registered for <paramref name="structName"/>.
    /// </summary>
    public IReadOnlyDictionary<string, StructField> GetFields(string structName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(structName);

        if (!_fieldsByStruct.TryGetValue(structName, out IReadOnlyDictionary<string, StructField>? fields))
            throw new KeyNotFoundException($"Struct '{structName}' is not registered.");

        return fields;
    }

    /// <summary>
    /// Attempts to look up one field by struct and field name.
    /// </summary>
    public bool TryGetField(string structName, string fieldName, out StructField field)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(structName);
        ArgumentException.ThrowIfNullOrWhiteSpace(fieldName);

        if (
            _fieldsByStruct.TryGetValue(structName, out IReadOnlyDictionary<string, StructField>? fields)
            && fields.TryGetValue(fieldName, out field)
        )
            return true;

        field = default;
        return false;
    }

    /// <summary>
    /// Creates a registry from the supplied grouped field definitions.
    /// </summary>
    public static StructFieldRegistry Create(params IEnumerable<StructField>[] structFields) => new(structFields);
}
