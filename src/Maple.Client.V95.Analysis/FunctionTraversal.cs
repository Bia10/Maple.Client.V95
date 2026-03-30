using Maple.Process;

namespace Maple.Client.V95.Analysis;

/// <summary>
/// Snapshot-backed traversal of the reachable direct-branch paths from one function entry.
/// </summary>
public sealed class FunctionTraversal
{
    private readonly uint[] _pathEntryAddresses;
    private readonly FunctionInstruction[] _instructions;
    private readonly uint[] _directBranchTargetAddresses;
    private readonly uint[] _directCallTargetAddresses;

    /// <summary>
    /// Creates a function traversal result.
    /// </summary>
    public FunctionTraversal(
        ProcessModuleInfo module,
        uint startAddress,
        uint[] pathEntryAddresses,
        FunctionInstruction[] instructions,
        uint[] directBranchTargetAddresses,
        uint[] directCallTargetAddresses,
        FunctionTraversalStatus status
    )
    {
        Module = module;
        StartAddress = startAddress;
        _pathEntryAddresses = pathEntryAddresses ?? throw new ArgumentNullException(nameof(pathEntryAddresses));
        _instructions = instructions ?? throw new ArgumentNullException(nameof(instructions));
        _directBranchTargetAddresses =
            directBranchTargetAddresses ?? throw new ArgumentNullException(nameof(directBranchTargetAddresses));
        _directCallTargetAddresses =
            directCallTargetAddresses ?? throw new ArgumentNullException(nameof(directCallTargetAddresses));
        Status = status;
    }

    /// <summary>
    /// Gets the module snapshot that supplied the decoded bytes.
    /// </summary>
    public ProcessModuleInfo Module { get; }

    /// <summary>
    /// Gets the function entry address requested by the caller.
    /// </summary>
    public uint StartAddress { get; }

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
    public FunctionTraversalStatus Status { get; }

    /// <summary>
    /// Gets whether traversal stopped because one configured traversal budget was exhausted.
    /// </summary>
    public bool Truncated => Status is FunctionTraversalStatus.InstructionLimit or FunctionTraversalStatus.PathLimit;
}
