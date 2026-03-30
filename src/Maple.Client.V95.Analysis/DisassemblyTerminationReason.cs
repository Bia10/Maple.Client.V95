namespace Maple.Client.V95.Analysis;

/// <summary>
/// Explains why one linear function disassembly stopped.
/// </summary>
public enum DisassemblyTerminationReason
{
    EndOfModuleImage = 0,
    InvalidInstruction,
    Return,
    TerminalBranch,
    InstructionLimit,
}
