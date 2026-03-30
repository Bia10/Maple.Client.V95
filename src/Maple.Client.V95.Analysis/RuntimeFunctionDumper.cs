using System.Runtime.Versioning;
using Maple.Memory;

namespace Maple.Client.V95.Analysis;

/// <summary>
/// Shared batch runtime-dumping surface above snapshot capture and per-function analysis.
/// </summary>
/// <remarks>
/// Creates a batch runtime function dumper over an existing analyzer.
/// </remarks>
public sealed class RuntimeFunctionDumper(RuntimeFunctionAnalyzer analyzer)
{
    private readonly RuntimeFunctionAnalyzer _analyzer = analyzer ?? throw new ArgumentNullException(nameof(analyzer));

    /// <summary>
    /// Creates a batch runtime function dumper over one captured process snapshot.
    /// </summary>
    [SupportedOSPlatform("windows")]
    public RuntimeFunctionDumper(ProcessPeImageSnapshot snapshot)
        : this(new RuntimeFunctionAnalyzer(snapshot)) { }

    /// <summary>
    /// Creates a batch runtime function dumper over one module image.
    /// </summary>
    public RuntimeFunctionDumper(ReadOnlyMemory<byte> image, Process.ProcessModuleInfo module)
        : this(new RuntimeFunctionAnalyzer(image, module)) { }

    /// <summary>
    /// Captures one live module snapshot and dumps the requested targets from it.
    /// </summary>
    [SupportedOSPlatform("windows")]
    public static RuntimeFunctionDumpBatch CaptureAndDump(
        Process.ProcessModuleInfo module,
        MemoryAccessor memoryAccessor,
        IEnumerable<RuntimeFunctionTarget> targets,
        RuntimeFunctionDumpOptions? options = null
    )
    {
        ArgumentNullException.ThrowIfNull(memoryAccessor);

        RuntimeFunctionDumpOptions effectiveOptions = options ?? new RuntimeFunctionDumpOptions();
        ProcessPeImageSnapshot snapshot = ProcessPeImageSnapshot.Capture(
            module,
            memoryAccessor,
            effectiveOptions.SnapshotMaxAttempts
        );
        return new RuntimeFunctionDumper(snapshot).Dump(targets, effectiveOptions);
    }

    /// <summary>
    /// Captures the attached main module through <paramref name="processHandle"/> and <paramref name="memoryAccessor"/> and dumps the requested targets from it.
    /// </summary>
    [SupportedOSPlatform("windows")]
    public static RuntimeFunctionDumpBatch DumpMainModule(
        Process.ProcessHandle processHandle,
        MemoryAccessor memoryAccessor,
        IEnumerable<RuntimeFunctionTarget> targets,
        RuntimeFunctionDumpOptions? options = null
    )
    {
        ArgumentNullException.ThrowIfNull(processHandle);
        ArgumentNullException.ThrowIfNull(memoryAccessor);

        RuntimeFunctionDumpOptions effectiveOptions = options ?? new RuntimeFunctionDumpOptions();
        ProcessPeImageSnapshot snapshot = ProcessPeImageSnapshot.CaptureMainModule(
            processHandle,
            memoryAccessor,
            effectiveOptions.SnapshotMaxAttempts
        );
        return new RuntimeFunctionDumper(snapshot).Dump(targets, effectiveOptions);
    }

    /// <summary>
    /// Dumps the requested function targets through the configured per-target analysis mode.
    /// </summary>
    public RuntimeFunctionDumpBatch Dump(
        IEnumerable<RuntimeFunctionTarget> targets,
        RuntimeFunctionDumpOptions? options = null
    )
    {
        ArgumentNullException.ThrowIfNull(targets);

        RuntimeFunctionDumpOptions effectiveOptions = options ?? new RuntimeFunctionDumpOptions();
        RuntimeFunctionTarget[] targetArray = [.. targets];
        RuntimeFunctionDump[] entries = new RuntimeFunctionDump[targetArray.Length];

        for (int index = 0; index < targetArray.Length; index++)
            entries[index] = DumpTarget(targetArray[index], effectiveOptions);

        return new RuntimeFunctionDumpBatch(_analyzer.Module, effectiveOptions.Kind, entries);
    }

    private RuntimeFunctionDump DumpTarget(RuntimeFunctionTarget target, RuntimeFunctionDumpOptions options) =>
        options.Kind switch
        {
            RuntimeFunctionDumpKind.Disassembly => DumpDisassembly(target, options),
            RuntimeFunctionDumpKind.Traversal => DumpTraversal(target, options),
            _ => throw new ArgumentOutOfRangeException(nameof(options), $"Unknown dump kind {options.Kind}."),
        };

    private RuntimeFunctionDump DumpDisassembly(RuntimeFunctionTarget target, RuntimeFunctionDumpOptions options)
    {
        if (_analyzer.TryDisassemble(target.Address, options.MaxInstructionCount, out FunctionDisassembly disassembly))
            return new RuntimeFunctionDump(target, RuntimeFunctionDumpStatus.Success, disassembly, null);

        return new RuntimeFunctionDump(target, RuntimeFunctionDumpStatus.AddressOutsideModule, null, null);
    }

    private RuntimeFunctionDump DumpTraversal(RuntimeFunctionTarget target, RuntimeFunctionDumpOptions options)
    {
        if (
            _analyzer.TryTraverse(
                target.Address,
                options.MaxInstructionCount,
                options.MaxPathCount,
                out FunctionTraversal traversal
            )
        )
        {
            return new RuntimeFunctionDump(target, RuntimeFunctionDumpStatus.Success, null, traversal);
        }

        return new RuntimeFunctionDump(target, RuntimeFunctionDumpStatus.AddressOutsideModule, null, null);
    }
}
