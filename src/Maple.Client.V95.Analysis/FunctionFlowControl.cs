namespace Maple.Client.V95.Analysis;

/// <summary>
/// Reduced x86 control-flow classification for one decoded instruction.
/// </summary>
public enum FunctionFlowControl
{
    Next = 0,
    Call,
    ConditionalBranch,
    UnconditionalBranch,
    IndirectCall,
    IndirectBranch,
    Return,
    Interrupt,
    Exception,
}
