using System.Buffers;
using System.Buffers.Binary;
using System.Text;
using Maple.Memory;
using Maple.Native;

namespace Maple.Client.V95.Runtime;

/// <summary>
/// MapleStory-specific string decoding helpers over a raw memory accessor.
/// </summary>
public static class MemoryAccessorStringExtensions
{
    private const int StackStringBytes = 256;

    /// <summary>
    /// Attempts to read a narrow <c>ZXString&lt;char&gt;</c> payload from one in-struct <c>_m_pStr</c> pointer.
    /// </summary>
    /// <remarks>
    /// The supplied <paramref name="payloadAddress"/> is the payload pointer itself, not the allocation base.
    /// The 12-byte <c>_ZXStringData</c> header is read from <c>payloadAddress - 12</c>.
    /// </remarks>
    public static bool TryReadZXString(this MemoryAccessor memoryAccessor, uint payloadAddress, out string? value)
    {
        ArgumentNullException.ThrowIfNull(memoryAccessor);
        return TryReadZXStringPayload(
            memoryAccessor,
            payloadAddress,
            Encoding.Latin1,
            requireEvenByteLength: false,
            out value
        );
    }

    /// <summary>
    /// Reads a narrow <c>ZXString&lt;char&gt;</c> payload from one in-struct <c>_m_pStr</c> pointer.
    /// </summary>
    public static string ReadZXString(this MemoryAccessor memoryAccessor, uint payloadAddress)
    {
        ArgumentNullException.ThrowIfNull(memoryAccessor);

        if (!memoryAccessor.TryReadZXString(payloadAddress, out string? value))
            throw CreateZXStringReadException(payloadAddress, "narrow");

        return value!;
    }

    /// <summary>
    /// Attempts to read a wide <c>ZXString&lt;unsigned short&gt;</c> payload from one in-struct <c>_m_pStr</c> pointer.
    /// </summary>
    /// <remarks>
    /// The supplied <paramref name="payloadAddress"/> is the payload pointer itself, not the allocation base.
    /// The 12-byte <c>_ZXStringData</c> header is read from <c>payloadAddress - 12</c>.
    /// </remarks>
    public static bool TryReadZXStringWide(this MemoryAccessor memoryAccessor, uint payloadAddress, out string? value)
    {
        ArgumentNullException.ThrowIfNull(memoryAccessor);
        return TryReadZXStringPayload(
            memoryAccessor,
            payloadAddress,
            Encoding.Unicode,
            requireEvenByteLength: true,
            out value
        );
    }

    /// <summary>
    /// Reads a wide <c>ZXString&lt;unsigned short&gt;</c> payload from one in-struct <c>_m_pStr</c> pointer.
    /// </summary>
    public static string ReadZXStringWide(this MemoryAccessor memoryAccessor, uint payloadAddress)
    {
        ArgumentNullException.ThrowIfNull(memoryAccessor);

        if (!memoryAccessor.TryReadZXStringWide(payloadAddress, out string? value))
            throw CreateZXStringReadException(payloadAddress, "wide");

        return value!;
    }

    private static bool TryReadZXStringPayload(
        MemoryAccessor memoryAccessor,
        uint payloadAddress,
        Encoding encoding,
        bool requireEvenByteLength,
        out string? value
    )
    {
        if (payloadAddress < ZXStringDataLayout.HeaderBytes)
        {
            value = null;
            return false;
        }

        uint headerAddress = payloadAddress - ZXStringDataLayout.HeaderBytes;
        Span<byte> header = stackalloc byte[ZXStringDataLayout.HeaderBytes];
        if (!memoryAccessor.Read(headerAddress, header))
        {
            value = null;
            return false;
        }

        int byteLength = BinaryPrimitives.ReadInt32LittleEndian(header[ZXStringDataLayout.ByteLengthOffset..]);
        if (byteLength < 0 || (requireEvenByteLength && (byteLength & 1) != 0))
        {
            value = null;
            return false;
        }

        if (byteLength == 0)
        {
            value = string.Empty;
            return true;
        }

        if (byteLength <= StackStringBytes)
        {
            Span<byte> stackBuffer = stackalloc byte[StackStringBytes];
            Span<byte> payload = stackBuffer[..byteLength];
            if (!memoryAccessor.Read(payloadAddress, payload))
            {
                value = null;
                return false;
            }

            value = encoding.GetString(payload);
            return true;
        }

        byte[] rented = ArrayPool<byte>.Shared.Rent(byteLength);
        try
        {
            Span<byte> payload = rented.AsSpan(0, byteLength);
            if (!memoryAccessor.Read(payloadAddress, payload))
            {
                value = null;
                return false;
            }

            value = encoding.GetString(payload);
            return true;
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(rented, clearArray: true);
        }
    }

    private static InvalidOperationException CreateZXStringReadException(uint payloadAddress, string kind) =>
        new($"Failed to read {kind} ZXString payload from 0x{payloadAddress:X8}.");
}
