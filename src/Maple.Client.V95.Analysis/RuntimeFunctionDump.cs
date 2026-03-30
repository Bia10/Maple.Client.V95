namespace Maple.Client.V95.Analysis;

/// <summary>
/// Holds the dump result for one named runtime function target.
/// </summary>
/// <remarks>
/// Creates one runtime function dump result.
/// </remarks>
public sealed class RuntimeFunctionDump(
    RuntimeFunctionTarget target,
    RuntimeFunctionDumpStatus status,
    FunctionDisassembly? disassembly,
    FunctionTraversal? traversal
)
{
    /// <summary>
    /// Gets the named target requested by the caller.
    /// </summary>
    public RuntimeFunctionTarget Target { get; } = target;

    /// <summary>
    /// Gets whether the target could be dumped.
    /// </summary>
    public RuntimeFunctionDumpStatus Status { get; } = status;

    /// <summary>
    /// Gets the linear disassembly result when the dump used disassembly mode.
    /// </summary>
    public FunctionDisassembly? Disassembly { get; } = disassembly;

    /// <summary>
    /// Gets the reachable-path traversal result when the dump used traversal mode.
    /// </summary>
    public FunctionTraversal? Traversal { get; } = traversal;

    /// <summary>
    /// Gets whether the target was dumped successfully.
    /// </summary>
    public bool Succeeded => Status == RuntimeFunctionDumpStatus.Success;
}
