using System.Runtime.Versioning;
using Maple.Memory;
using Maple.Process;

namespace Maple.Client.V95.Analysis.Test;

[SupportedOSPlatform("windows")]
public sealed class RuntimeFunctionDumperTests
{
    [Test]
    public async Task RuntimeFunctionDumper_Dump_FromSnapshot_ProducesStablePerTargetResults()
    {
        byte[] image = BuildMultiTargetModuleImage();
        ProcessModuleInfo module = new("MapleStory.exe", @"C:\MapleStory.exe", 0x0040_0000u, image.Length);
        using var processMemory = new FakeRemoteProcessMemory((uint)module.BaseAddress, image);
        using var accessor = new MemoryAccessor(processMemory);

        ProcessPeImageSnapshot snapshot = ProcessPeImageSnapshot.Capture(module, accessor);
        var dumper = new RuntimeFunctionDumper(snapshot);
        RuntimeFunctionDumpBatch batch = dumper.Dump(
            [
                new RuntimeFunctionTarget("simple", 0x0040_1000u),
                new RuntimeFunctionTarget("branch", 0x0040_1100u),
                new RuntimeFunctionTarget("missing", 0x0040_3000u),
            ],
            new RuntimeFunctionDumpOptions
            {
                Kind = RuntimeFunctionDumpKind.Traversal,
                MaxInstructionCount = 32,
                MaxPathCount = 8,
            }
        );

        await Assert.That(batch.Module.BaseAddress).IsEqualTo(module.BaseAddress);
        await Assert.That(batch.Kind).IsEqualTo(RuntimeFunctionDumpKind.Traversal);
        await Assert.That(batch.Entries.Count).IsEqualTo(3);
        await Assert.That(batch.Entries[0].Target.Name).IsEqualTo("simple");
        await Assert.That(batch.Entries[0].Succeeded).IsTrue();
        await Assert.That(batch.Entries[0].Traversal).IsNotNull();
        await Assert.That(batch.Entries[1].Target.Name).IsEqualTo("branch");
        await Assert.That(batch.Entries[1].Traversal!.DirectBranchTargetAddresses.Count).IsEqualTo(2);
        await Assert.That(batch.Entries[2].Status).IsEqualTo(RuntimeFunctionDumpStatus.AddressOutsideModule);
        await Assert.That(batch.Entries[2].Traversal).IsNull();
    }

    [Test]
    public async Task RuntimeFunctionDumper_CaptureAndDump_UsesLiveCapturePathOverMemoryAccessor()
    {
        byte[] image = BuildMultiTargetModuleImage();
        ProcessModuleInfo module = new("MapleStory.exe", @"C:\MapleStory.exe", 0x0040_0000u, image.Length);
        using var processMemory = new FakeRemoteProcessMemory((uint)module.BaseAddress, image);
        using var accessor = new MemoryAccessor(processMemory);

        RuntimeFunctionDumpBatch batch = RuntimeFunctionDumper.CaptureAndDump(
            module,
            accessor,
            [new RuntimeFunctionTarget("simple", 0x0040_1000u)],
            new RuntimeFunctionDumpOptions { Kind = RuntimeFunctionDumpKind.Disassembly, MaxInstructionCount = 16 }
        );

        await Assert.That(batch.Kind).IsEqualTo(RuntimeFunctionDumpKind.Disassembly);
        await Assert.That(batch.Entries.Count).IsEqualTo(1);
        await Assert.That(batch.Entries[0].Succeeded).IsTrue();
        await Assert.That(batch.Entries[0].Disassembly).IsNotNull();
        await Assert.That(batch.Entries[0].Disassembly!.Instructions.Count).IsEqualTo(5);
    }

    [Test]
    public async Task RuntimeFunctionDumper_DumpMainModule_FollowsMemoryWriterCaptureContract()
    {
        using var currentProcess = System.Diagnostics.Process.GetCurrentProcess();
        using var writer = MemoryWriter.Open(currentProcess.Id, WindowsProcessAccess.Default);
        ProcessModuleInfo mainModule = writer.ProcessHandle.GetMainModule();

        if (mainModule.BaseAddress > uint.MaxValue)
        {
            await Assert
                .That(() => RuntimeFunctionDumper.DumpMainModule(writer.ProcessHandle, writer.MemoryAccessor, []))
                .Throws<InvalidOperationException>();
            return;
        }

        RuntimeFunctionDumpBatch batch = RuntimeFunctionDumper.DumpMainModule(
            writer.ProcessHandle,
            writer.MemoryAccessor,
            []
        );

        await Assert.That(batch.Module.BaseAddress).IsEqualTo(mainModule.BaseAddress);
        await Assert.That(batch.Entries.Count).IsEqualTo(0);
    }

    private static byte[] BuildMultiTargetModuleImage()
    {
        byte[] image = GC.AllocateUninitializedArray<byte>(0x1300);
        image[0] = (byte)'M';
        image[1] = (byte)'Z';

        ReadOnlySpan<byte> simpleFunction =
        [
            0x55,
            0x8B,
            0xEC,
            0xE8,
            0x04,
            0x00,
            0x00,
            0x00,
            0x75,
            0x02,
            0xC3,
            0x90,
            0xC3,
        ];
        simpleFunction.CopyTo(image.AsSpan(0x1000));

        ReadOnlySpan<byte> branchingFunction =
        [
            0x75,
            0x07,
            0xB8,
            0x01,
            0x00,
            0x00,
            0x00,
            0xEB,
            0x07,
            0xB8,
            0x02,
            0x00,
            0x00,
            0x00,
            0xC3,
            0x90,
            0xC3,
        ];
        branchingFunction.CopyTo(image.AsSpan(0x1100));

        return image;
    }

    private sealed class FakeRemoteProcessMemory(uint baseAddress, byte[] image) : IRemoteProcessMemory
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

        public void Dispose() { }
    }
}
