namespace Maple.Client.V95.Analysis;

/// <summary>
/// One decoded x86 instruction from a runtime function snapshot.
/// </summary>
public readonly record struct FunctionInstruction(
    uint Address,
    int Size,
    string BytesHex,
    string Mnemonic,
    string Text,
    FunctionFlowControl FlowControl,
    uint? DirectTargetAddress
);
