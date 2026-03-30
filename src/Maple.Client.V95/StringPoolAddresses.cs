namespace Maple.Client.V95;

/// <summary>
/// Static <c>.data</c> section addresses for a StringPool binary.
/// Each member corresponds to a static field in the native <c>StringPool</c> class.
/// </summary>
/// <remarks>
/// These addresses refer to plain C arrays and scalars in the client image, not heap-backed
/// native array objects. They are version-specific layout data and therefore live in the
/// v95 schema package rather than in the decoder package.
/// </remarks>
public readonly record struct StringPoolAddresses
{
    /// <summary>PE image base address (typically <c>0x400000</c> for GMS x86 clients).</summary>
    public required uint ImageBase { get; init; }

    /// <summary><c>StringPool::ms_aString</c> pointer table base address.</summary>
    public required uint MsAString { get; init; }

    /// <summary><c>StringPool::ms_aKey</c> static XOR master key base address.</summary>
    public required uint MsAKey { get; init; }

    /// <summary><c>StringPool::ms_nKeySize</c> address.</summary>
    public required uint MsNKeySize { get; init; }

    /// <summary><c>StringPool::ms_nSize</c> address.</summary>
    public required uint MsNSize { get; init; }
}
