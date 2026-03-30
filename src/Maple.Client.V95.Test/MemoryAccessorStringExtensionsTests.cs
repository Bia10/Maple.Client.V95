using System.Buffers.Binary;
using Maple.Memory;
using Maple.Process;

namespace Maple.Client.V95.Test;

public sealed class MemoryAccessorStringExtensionsTests
{
    [Test]
    public async Task ReadZXString_ReadsLatin1PayloadPointer()
    {
        using var processMemory = new FakeRemoteProcessMemory();
        using var accessor = new MemoryAccessor(processMemory);
        const uint payloadAddress = 0x6000u;

        processMemory.WriteZXStringPayload(payloadAddress, "Scania");

        await Assert.That(accessor.ReadZXString(payloadAddress)).IsEqualTo("Scania");
    }

    [Test]
    public async Task ReadZXStringWide_ReadsUtf16PayloadPointer()
    {
        using var processMemory = new FakeRemoteProcessMemory();
        using var accessor = new MemoryAccessor(processMemory);
        const uint payloadAddress = 0x7000u;

        processMemory.WriteZXStringWidePayload(payloadAddress, "Bera");

        await Assert.That(accessor.ReadZXStringWide(payloadAddress)).IsEqualTo("Bera");
    }

    [Test]
    public async Task TryReadZXString_WhenHeaderMissing_ReturnsFalse()
    {
        using var processMemory = new FakeRemoteProcessMemory();
        using var accessor = new MemoryAccessor(processMemory);

        bool found = accessor.TryReadZXString(0x8000u, out string? value);

        await Assert.That(found).IsFalse();
        await Assert.That(value).IsNull();
    }

    private sealed class FakeRemoteProcessMemory : IRemoteProcessMemory
    {
        private readonly Dictionary<uint, byte[]> _values = [];

        public uint Allocate(int size) => throw new NotSupportedException();

        public bool Read(uint address, Span<byte> destination)
        {
            foreach ((uint baseAddress, byte[] bytes) in _values)
            {
                uint endAddress = baseAddress + (uint)bytes.Length;
                uint requestedEnd = address + (uint)destination.Length;
                if (address < baseAddress || requestedEnd > endAddress)
                    continue;

                bytes.AsSpan((int)(address - baseAddress), destination.Length).CopyTo(destination);
                return true;
            }

            return false;
        }

        public bool Write(uint address, ReadOnlySpan<byte> data)
        {
            _values[address] = data.ToArray();
            return true;
        }

        public void Free(uint address) => _values.Remove(address);

        public void Dispose() { }

        public void WriteZXStringPayload(uint payloadAddress, string value)
        {
            byte[] payload = System.Text.Encoding.Latin1.GetBytes(value);
            byte[] bytes = new byte[12 + payload.Length + 1];
            BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(0, 4), 1);
            BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(4, 4), value.Length);
            BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(8, 4), payload.Length);
            payload.CopyTo(bytes.AsSpan(12));
            _values[payloadAddress - 12u] = bytes;
        }

        public void WriteZXStringWidePayload(uint payloadAddress, string value)
        {
            byte[] payload = System.Text.Encoding.Unicode.GetBytes(value);
            byte[] bytes = new byte[12 + payload.Length + 2];
            BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(0, 4), 1);
            BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(4, 4), value.Length);
            BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(8, 4), payload.Length);
            payload.CopyTo(bytes.AsSpan(12));
            _values[payloadAddress - 12u] = bytes;
        }
    }
}
