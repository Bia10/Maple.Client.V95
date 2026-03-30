namespace Maple.Client.V95.Analysis;

/// <summary>
/// Selects how runtime function dumping should analyze each target.
/// </summary>
public enum RuntimeFunctionDumpKind
{
    Disassembly = 0,
    Traversal,
}
