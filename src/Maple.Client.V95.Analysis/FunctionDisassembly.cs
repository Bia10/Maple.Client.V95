using Maple.Process;

namespace Maple.Client.V95.Analysis;

/// <summary>
/// Snapshot-backed linear disassembly of one function body.
/// </summary>
/// <remarks>
/// Creates a function disassembly result.
/// </remarks>
public sealed class FunctionDisassembly(
    ProcessModuleInfo module,
    uint startAddress,
    FunctionInstruction[] instructions,
    uint[] directTargetAddresses,
    DisassemblyTerminationReason terminationReason
)
{
    private readonly FunctionInstruction[] _instructions =
        instructions ?? throw new ArgumentNullException(nameof(instructions));
    private readonly uint[] _directTargetAddresses =
        directTargetAddresses ?? throw new ArgumentNullException(nameof(directTargetAddresses));

    /// <summary>
    /// Gets the module snapshot that supplied the decoded bytes.
    /// </summary>
    public ProcessModuleInfo Module { get; } = module;

    /// <summary>
    /// Gets the function entry address requested by the caller.
    /// </summary>
    public uint StartAddress { get; } = startAddress;

    /// <summary>
    /// Gets the decoded instructions in linear order.
    /// </summary>
    public IReadOnlyList<FunctionInstruction> Instructions => _instructions;

    /// <summary>
    /// Gets the unique direct control-transfer targets discovered while decoding.
    /// </summary>
    public IReadOnlyList<uint> DirectTargetAddresses => _directTargetAddresses;

    /// <summary>
    /// Gets why the linear decode stopped.
    /// </summary>
    public DisassemblyTerminationReason TerminationReason { get; } = terminationReason;

    /// <summary>
    /// Gets whether disassembly stopped on a terminal instruction rather than a guard condition.
    /// </summary>
    public bool ReachedTerminalInstruction =>
        TerminationReason is DisassemblyTerminationReason.Return or DisassemblyTerminationReason.TerminalBranch;
}
