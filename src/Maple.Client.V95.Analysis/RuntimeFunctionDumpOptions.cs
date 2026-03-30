namespace Maple.Client.V95.Analysis;

/// <summary>
/// Configures shared runtime function dumping behavior.
/// </summary>
public sealed class RuntimeFunctionDumpOptions
{
    /// <summary>
    /// Gets or sets the per-target analysis mode.
    /// </summary>
    public RuntimeFunctionDumpKind Kind { get; init; } = RuntimeFunctionDumpKind.Traversal;

    /// <summary>
    /// Gets or sets the maximum number of unique instructions to decode per target.
    /// </summary>
    public int MaxInstructionCount { get; init; } = 256;

    /// <summary>
    /// Gets or sets the maximum number of path entry points to traverse per target.
    /// </summary>
    public int MaxPathCount { get; init; } = 32;

    /// <summary>
    /// Gets or sets the maximum number of attempts used when capturing a live module snapshot.
    /// </summary>
    public int SnapshotMaxAttempts { get; init; } = 3;
}
