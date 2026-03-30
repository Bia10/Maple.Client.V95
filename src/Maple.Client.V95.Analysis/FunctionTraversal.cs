using Maple.Process;

namespace Maple.Client.V95.Analysis;

/// <summary>
/// Snapshot-backed traversal of the reachable direct-branch paths from one function entry.
/// </summary>
/// <remarks>
/// Creates a function traversal result.
/// </remarks>
public sealed class FunctionTraversal(
    ProcessModuleInfo module,
    uint startAddress,
    uint[] pathEntryAddresses,
    FunctionInstruction[] instructions,
    uint[] directBranchTargetAddresses,
    uint[] directCallTargetAddresses,
    FunctionTraversalStatus status
)
{
    private readonly uint[] _pathEntryAddresses =
        pathEntryAddresses ?? throw new ArgumentNullException(nameof(pathEntryAddresses));
    private readonly FunctionInstruction[] _instructions =
        instructions ?? throw new ArgumentNullException(nameof(instructions));
    private readonly uint[] _directBranchTargetAddresses =
        directBranchTargetAddresses ?? throw new ArgumentNullException(nameof(directBranchTargetAddresses));
    private readonly uint[] _directCallTargetAddresses =
        directCallTargetAddresses ?? throw new ArgumentNullException(nameof(directCallTargetAddresses));

    /// <summary>
    /// Gets the module snapshot that supplied the decoded bytes.
    /// </summary>
    public ProcessModuleInfo Module { get; } = module;

    /// <summary>
    /// Gets the function entry address requested by the caller.
    /// </summary>
    public uint StartAddress { get; } = startAddress;

    /// <summary>
    /// Gets the unique path entry addresses that were actually traversed.
    /// </summary>
    public IReadOnlyList<uint> PathEntryAddresses => _pathEntryAddresses;

    /// <summary>
    /// Gets the unique reachable instructions decoded across all traversed paths.
    /// </summary>
    public IReadOnlyList<FunctionInstruction> Instructions => _instructions;

    /// <summary>
    /// Gets the unique direct branch targets discovered while traversing the function.
    /// </summary>
    public IReadOnlyList<uint> DirectBranchTargetAddresses => _directBranchTargetAddresses;

    /// <summary>
    /// Gets the unique direct call targets discovered while traversing the function.
    /// </summary>
    public IReadOnlyList<uint> DirectCallTargetAddresses => _directCallTargetAddresses;

    /// <summary>
    /// Gets how traversal completed.
    /// </summary>
    public FunctionTraversalStatus Status { get; } = status;

    /// <summary>
    /// Gets whether traversal stopped because one configured traversal budget was exhausted.
    /// </summary>
    public bool Truncated => Status is FunctionTraversalStatus.InstructionLimit or FunctionTraversalStatus.PathLimit;
}
