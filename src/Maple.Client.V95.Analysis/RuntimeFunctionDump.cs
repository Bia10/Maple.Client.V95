namespace Maple.Client.V95.Analysis;

/// <summary>
/// Holds the dump result for one named runtime function target.
/// </summary>
public sealed class RuntimeFunctionDump
{
    /// <summary>
    /// Creates one runtime function dump result.
    /// </summary>
    public RuntimeFunctionDump(
        RuntimeFunctionTarget target,
        RuntimeFunctionDumpStatus status,
        FunctionDisassembly? disassembly,
        FunctionTraversal? traversal
    )
    {
        Target = target;
        Status = status;
        Disassembly = disassembly;
        Traversal = traversal;
    }

    /// <summary>
    /// Gets the named target requested by the caller.
    /// </summary>
    public RuntimeFunctionTarget Target { get; }

    /// <summary>
    /// Gets whether the target could be dumped.
    /// </summary>
    public RuntimeFunctionDumpStatus Status { get; }

    /// <summary>
    /// Gets the linear disassembly result when the dump used disassembly mode.
    /// </summary>
    public FunctionDisassembly? Disassembly { get; }

    /// <summary>
    /// Gets the reachable-path traversal result when the dump used traversal mode.
    /// </summary>
    public FunctionTraversal? Traversal { get; }

    /// <summary>
    /// Gets whether the target was dumped successfully.
    /// </summary>
    public bool Succeeded => Status == RuntimeFunctionDumpStatus.Success;
}
