using System.Runtime.Versioning;
using Maple.Memory;
using Maple.Native;
using Maple.Process;

namespace Maple.Client.V95.Analysis.Test;

[SupportedOSPlatform("windows")]
public sealed class ProcessPeImageSnapshotTests
{
    [Test]
    public async Task CaptureMainModule_CurrentProcess_ReadsMzHeader()
    {
        using var currentProcess = System.Diagnostics.Process.GetCurrentProcess();
        using var writer = MemoryWriter.Open(currentProcess.Id, WindowsProcessAccess.Default);
        ProcessModuleInfo mainModule = writer.ProcessHandle.GetMainModule();

        if (mainModule.BaseAddress > uint.MaxValue)
        {
            await Assert
                .That(() => ProcessPeImageSnapshot.CaptureMainModule(writer.ProcessHandle, writer.MemoryAccessor))
                .Throws<InvalidOperationException>();
            return;
        }

        ProcessPeImageSnapshot snapshot = ProcessPeImageSnapshot.CaptureMainModule(
            writer.ProcessHandle,
            writer.MemoryAccessor
        );

        await Assert.That(snapshot.Module.ModuleName).IsEqualTo(currentProcess.MainModule!.ModuleName);
        await Assert.That(snapshot.Module.FileName).IsEqualTo(currentProcess.MainModule!.FileName);
        await Assert.That(snapshot.Module.ImageSize).IsEqualTo(currentProcess.MainModule!.ModuleMemorySize);
        await Assert.That(snapshot.Image.Length).IsEqualTo(currentProcess.MainModule!.ModuleMemorySize);
        await Assert.That(snapshot.Image.Span[0]).IsEqualTo((byte)'M');
        await Assert.That(snapshot.Image.Span[1]).IsEqualTo((byte)'Z');
    }

    [Test]
    public async Task Capture_WhenModuleUsesX86Base_CreatesNativeImageView()
    {
        byte[] image = [(byte)'M', (byte)'Z', 0x90, 0x00];
        ProcessModuleInfo module = new("MapleStory.exe", @"C:\MapleStory.exe", 0x0040_0000u, image.Length);
        using var processMemory = new FakeRemoteProcessMemory((uint)module.BaseAddress, image);
        using var accessor = new MemoryAccessor(processMemory);

        ProcessPeImageSnapshot snapshot = ProcessPeImageSnapshot.Capture(module, accessor);
        bool created = snapshot.TryCreateView(out NativeImageView? view);

        await Assert.That(created).IsTrue();
        await Assert.That(view).IsNotNull();
        await Assert.That(view!.FileOffset(0x0040_0000u)).IsEqualTo(0);
        await Assert.That(snapshot.CreateView().FileOffset(0x0040_0003u)).IsEqualTo(3);
    }

    private sealed class FakeRemoteProcessMemory(uint baseAddress, byte[] image)
        : IRemoteProcessMemory,
            IRemoteProcessMemoryInspector
    {
        public uint Allocate(int size) => throw new NotSupportedException();

        public bool Read(uint address, Span<byte> destination)
        {
            uint endAddress = checked(baseAddress + (uint)image.Length);
            uint requestedEnd = checked(address + (uint)destination.Length);
            if (address < baseAddress || requestedEnd > endAddress)
                return false;

            image.AsSpan((int)(address - baseAddress), destination.Length).CopyTo(destination);
            return true;
        }

        public bool Write(uint address, ReadOnlySpan<byte> data) => false;

        public void Free(uint address) { }

        public bool TryQuery(nuint address, out ProcessMemoryRegion region)
        {
            region = new ProcessMemoryRegion(
                baseAddress,
                baseAddress,
                WindowsMemoryProtection.ReadOnly,
                (nuint)image.Length,
                WindowsMemoryState.Commit,
                WindowsMemoryProtection.ReadOnly,
                WindowsMemoryType.Image
            );
            return region.Contains(address);
        }

        public void Dispose() { }
    }
}
