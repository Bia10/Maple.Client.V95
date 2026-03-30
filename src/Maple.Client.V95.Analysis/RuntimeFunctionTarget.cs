namespace Maple.Client.V95.Analysis;

/// <summary>
/// Names one runtime function target to analyze.
/// </summary>
public readonly record struct RuntimeFunctionTarget(string Name, uint Address);
