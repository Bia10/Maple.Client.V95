namespace Maple.Client.V95.Analysis;

/// <summary>
/// Explains how reachable-path traversal of one function completed.
/// </summary>
public enum FunctionTraversalStatus
{
    Complete = 0,
    InvalidInstruction,
    InstructionLimit,
    PathLimit,
}
