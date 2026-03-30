using System.Runtime.Versioning;
using Maple.Memory;
using Maple.Process;

namespace Maple.Client.V95.Analysis.Test;

[SupportedOSPlatform("windows")]
public sealed class RuntimeFunctionAnalyzerTests
{
    [Test]
    public async Task RuntimeFunctionAnalyzer_Disassemble_FromSnapshot_PreservesInstructionTextAndTargets()
    {
        byte[] image = BuildModuleImage();
        ProcessModuleInfo module = new("MapleStory.exe", @"C:\MapleStory.exe", 0x0040_0000u, image.Length);
        using var processMemory = new FakeRemoteProcessMemory((uint)module.BaseAddress, image);
        using var accessor = new MemoryAccessor(processMemory);

        ProcessPeImageSnapshot snapshot = ProcessPeImageSnapshot.Capture(module, accessor);
        var analyzer = new RuntimeFunctionAnalyzer(snapshot);

        FunctionDisassembly disassembly = analyzer.Disassemble(0x0040_1000u, maxInstructionCount: 16);

        await Assert.That(disassembly.Module.BaseAddress).IsEqualTo(module.BaseAddress);
        await Assert.That(disassembly.StartAddress).IsEqualTo(0x0040_1000u);
        await Assert.That(disassembly.TerminationReason).IsEqualTo(DisassemblyTerminationReason.Return);
        await Assert.That(disassembly.Instructions.Count).IsEqualTo(5);
        await Assert.That(disassembly.Instructions[0].Mnemonic).IsEqualTo("push");
        await Assert.That(disassembly.Instructions[2].Mnemonic).IsEqualTo("call");
        await Assert.That(disassembly.Instructions[2].DirectTargetAddress).IsEqualTo(0x0040_100Cu);
        await Assert.That(disassembly.Instructions[3].DirectTargetAddress).IsEqualTo(0x0040_100Cu);
        await Assert.That(disassembly.Instructions[4].Mnemonic).IsEqualTo("ret");
        await Assert.That(disassembly.DirectTargetAddresses.Count).IsEqualTo(1);
        await Assert.That(disassembly.DirectTargetAddresses[0]).IsEqualTo(0x0040_100Cu);
    }

    [Test]
    public async Task RuntimeFunctionAnalyzer_TryDisassemble_WhenAddressFallsOutsideModule_ReturnsFalse()
    {
        byte[] image = BuildModuleImage();
        ProcessModuleInfo module = new("MapleStory.exe", @"C:\MapleStory.exe", 0x0040_0000u, image.Length);
        var analyzer = new RuntimeFunctionAnalyzer(image, module);

        bool decoded = analyzer.TryDisassemble(0x0040_3000u, 16, out FunctionDisassembly disassembly);

        await Assert.That(decoded).IsFalse();
        await Assert.That(disassembly).IsNull();
    }

    [Test]
    public async Task RuntimeFunctionAnalyzer_Traverse_FollowsReachableDirectBranchPaths()
    {
        byte[] image = BuildBranchingModuleImage();
        ProcessModuleInfo module = new("MapleStory.exe", @"C:\MapleStory.exe", 0x0040_0000u, image.Length);
        var analyzer = new RuntimeFunctionAnalyzer(image, module);

        FunctionTraversal traversal = analyzer.Traverse(0x0040_1000u, maxInstructionCount: 16, maxPathCount: 8);

        await Assert.That(traversal.Status).IsEqualTo(FunctionTraversalStatus.Complete);
        await Assert.That(traversal.PathEntryAddresses.Count).IsEqualTo(3);
        await Assert.That(traversal.PathEntryAddresses[0]).IsEqualTo(0x0040_1000u);
        await Assert.That(traversal.PathEntryAddresses[1]).IsEqualTo(0x0040_1009u);
        await Assert.That(traversal.PathEntryAddresses[2]).IsEqualTo(0x0040_1010u);
        await Assert.That(traversal.Instructions.Count).IsEqualTo(6);
        await Assert.That(traversal.Instructions[0].Address).IsEqualTo(0x0040_1000u);
        await Assert.That(traversal.Instructions[5].Address).IsEqualTo(0x0040_1010u);
        await Assert.That(traversal.DirectBranchTargetAddresses.Count).IsEqualTo(2);
        await Assert.That(traversal.DirectBranchTargetAddresses[0]).IsEqualTo(0x0040_1009u);
        await Assert.That(traversal.DirectBranchTargetAddresses[1]).IsEqualTo(0x0040_1010u);
        await Assert.That(traversal.DirectCallTargetAddresses.Count).IsEqualTo(0);
    }

    [Test]
    public async Task RuntimeFunctionAnalyzer_Traverse_WhenPathBudgetIsExceeded_ReturnsPathLimit()
    {
        byte[] image = BuildBranchingModuleImage();
        ProcessModuleInfo module = new("MapleStory.exe", @"C:\MapleStory.exe", 0x0040_0000u, image.Length);
        var analyzer = new RuntimeFunctionAnalyzer(image, module);

        FunctionTraversal traversal = analyzer.Traverse(0x0040_1000u, maxInstructionCount: 16, maxPathCount: 2);

        await Assert.That(traversal.Status).IsEqualTo(FunctionTraversalStatus.PathLimit);
        await Assert.That(traversal.Truncated).IsTrue();
        await Assert.That(traversal.PathEntryAddresses.Count).IsEqualTo(2);
        await Assert.That(traversal.Instructions.Count).IsEqualTo(5);
        await Assert.That(traversal.Instructions[^1].Address).IsEqualTo(0x0040_100Eu);
    }

    private static byte[] BuildModuleImage()
    {
        byte[] image = GC.AllocateUninitializedArray<byte>(0x1100);
        image[0] = (byte)'M';
        image[1] = (byte)'Z';

        ReadOnlySpan<byte> functionBytes =
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
        functionBytes.CopyTo(image.AsSpan(0x1000));
        return image;
    }

    private static byte[] BuildBranchingModuleImage()
    {
        byte[] image = GC.AllocateUninitializedArray<byte>(0x1200);
        image[0] = (byte)'M';
        image[1] = (byte)'Z';

        ReadOnlySpan<byte> functionBytes =
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
        functionBytes.CopyTo(image.AsSpan(0x1000));
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
