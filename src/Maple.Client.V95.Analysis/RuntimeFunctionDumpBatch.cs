using Maple.Process;

namespace Maple.Client.V95.Analysis;

/// <summary>
/// Stable shared-library result for one runtime dump batch.
/// </summary>
public sealed class RuntimeFunctionDumpBatch
{
    private readonly RuntimeFunctionDump[] _entries;

    /// <summary>
    /// Creates one runtime dump batch.
    /// </summary>
    public RuntimeFunctionDumpBatch(
        ProcessModuleInfo module,
        RuntimeFunctionDumpKind kind,
        RuntimeFunctionDump[] entries
    )
    {
        Module = module;
        Kind = kind;
        _entries = entries ?? throw new ArgumentNullException(nameof(entries));
    }

    /// <summary>
    /// Gets the module snapshot that supplied the analyzed bytes.
    /// </summary>
    public ProcessModuleInfo Module { get; }

    /// <summary>
    /// Gets the per-target analysis mode used by this batch.
    /// </summary>
    public RuntimeFunctionDumpKind Kind { get; }

    /// <summary>
    /// Gets the stable per-target dump results.
    /// </summary>
    public IReadOnlyList<RuntimeFunctionDump> Entries => _entries;
}
