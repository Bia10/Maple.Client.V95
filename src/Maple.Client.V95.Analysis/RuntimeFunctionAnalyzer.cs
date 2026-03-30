using System.Runtime.Versioning;
using System.Text;
using Iced.Intel;
using Maple.Native;
using Maple.Process;

namespace Maple.Client.V95.Analysis;

/// <summary>
/// Disassembles x86 functions from one captured Maple module snapshot.
/// </summary>
/// <remarks>
/// This is the first extraction slice from Rue's runtime dumping surface. It intentionally stays
/// narrow: accept a captured module image, linearly decode one function, preserve instruction text,
/// and report direct control-transfer targets without reintroducing Rue-local Win32 wrappers.
/// </remarks>
public sealed class RuntimeFunctionAnalyzer
{
    private readonly ReadOnlyMemory<byte> _image;
    private readonly NativeImageView _view;

    /// <summary>
    /// Creates a runtime function analyzer over one module image.
    /// </summary>
    public RuntimeFunctionAnalyzer(ReadOnlyMemory<byte> image, ProcessModuleInfo module)
    {
        if (module.ImageSize <= 0)
            throw new ArgumentOutOfRangeException(nameof(module), "Module image size must be positive.");

        if (module.BaseAddress > uint.MaxValue)
        {
            throw new InvalidOperationException(
                $"Module base 0x{module.BaseAddress:X} does not fit in the x86 Maple address space required for runtime disassembly."
            );
        }

        if (image.Length != module.ImageSize)
        {
            throw new ArgumentException(
                $"Module image length {image.Length} does not match module size {module.ImageSize}.",
                nameof(image)
            );
        }

        _image = image;
        Module = module;
        _view = new NativeImageView(image, checked((uint)module.BaseAddress));
    }

    /// <summary>
    /// Creates a runtime function analyzer over one captured process snapshot.
    /// </summary>
    [SupportedOSPlatform("windows")]
    public RuntimeFunctionAnalyzer(ProcessPeImageSnapshot snapshot)
        : this((snapshot ?? throw new ArgumentNullException(nameof(snapshot))).Image, snapshot.Module) { }

    /// <summary>
    /// Gets the backing module metadata.
    /// </summary>
    public ProcessModuleInfo Module { get; }

    /// <summary>
    /// Attempts to linearly disassemble one function starting at <paramref name="functionAddress"/>.
    /// </summary>
    public bool TryDisassemble(uint functionAddress, int maxInstructionCount, out FunctionDisassembly disassembly)
    {
        ValidateInstructionLimit(maxInstructionCount);

        if (!Module.Contains(functionAddress))
        {
            disassembly = default!;
            return false;
        }

        disassembly = DisassembleCore(functionAddress, maxInstructionCount);
        return true;
    }

    /// <summary>
    /// Linearly disassembles one function starting at <paramref name="functionAddress"/>.
    /// </summary>
    public FunctionDisassembly Disassemble(uint functionAddress, int maxInstructionCount = 128)
    {
        ValidateInstructionLimit(maxInstructionCount);

        if (!Module.Contains(functionAddress))
        {
            throw new ArgumentOutOfRangeException(
                nameof(functionAddress),
                $"Function address 0x{functionAddress:X8} is outside module {Module.ModuleName} at 0x{Module.BaseAddress:X}."
            );
        }

        return DisassembleCore(functionAddress, maxInstructionCount);
    }

    /// <summary>
    /// Attempts to traverse the reachable direct-branch paths from <paramref name="functionAddress"/>.
    /// </summary>
    public bool TryTraverse(
        uint functionAddress,
        int maxInstructionCount,
        int maxPathCount,
        out FunctionTraversal traversal
    )
    {
        ValidateInstructionLimit(maxInstructionCount);
        ValidatePathLimit(maxPathCount);

        if (!Module.Contains(functionAddress))
        {
            traversal = default!;
            return false;
        }

        traversal = TraverseCore(functionAddress, maxInstructionCount, maxPathCount);
        return true;
    }

    /// <summary>
    /// Traverses the reachable direct-branch paths from <paramref name="functionAddress"/>.
    /// </summary>
    public FunctionTraversal Traverse(uint functionAddress, int maxInstructionCount = 256, int maxPathCount = 32)
    {
        ValidateInstructionLimit(maxInstructionCount);
        ValidatePathLimit(maxPathCount);

        if (!Module.Contains(functionAddress))
        {
            throw new ArgumentOutOfRangeException(
                nameof(functionAddress),
                $"Function address 0x{functionAddress:X8} is outside module {Module.ModuleName} at 0x{Module.BaseAddress:X}."
            );
        }

        return TraverseCore(functionAddress, maxInstructionCount, maxPathCount);
    }

    private FunctionDisassembly DisassembleCore(uint functionAddress, int maxInstructionCount)
    {
        int startOffset = _view.FileOffset(functionAddress);
        ReadOnlyMemory<byte> functionBytes = _image[startOffset..];
        var decoder = Iced.Intel.Decoder.Create(32, new MemoryCodeReader(functionBytes), ip: functionAddress);
        var formatter = new NasmFormatter();
        var formattedInstruction = new StringBuilder();
        var formatterOutput = new StringBuilderFormatterOutput(formattedInstruction);
        List<FunctionInstruction> instructions = [];
        List<uint> directTargetAddresses = [];
        HashSet<uint> seenTargets = [];
        DisassemblyTerminationReason terminationReason = DisassemblyTerminationReason.EndOfModuleImage;
        bool terminated = false;

        for (int decodedCount = 0; decodedCount < maxInstructionCount; decodedCount++)
        {
            decoder.Decode(out Instruction instruction);
            if (decoder.LastError == DecoderError.NoMoreBytes)
            {
                terminationReason = DisassemblyTerminationReason.EndOfModuleImage;
                terminated = true;
                break;
            }

            if (decoder.LastError == DecoderError.InvalidInstruction || instruction.Code == Code.INVALID)
            {
                terminationReason = DisassemblyTerminationReason.InvalidInstruction;
                terminated = true;
                break;
            }

            uint address = checked((uint)instruction.IP);
            int instructionOffset = _view.FileOffset(address);
            if (instructionOffset + instruction.Length > _image.Length)
            {
                terminationReason = DisassemblyTerminationReason.EndOfModuleImage;
                terminated = true;
                break;
            }

            formattedInstruction.Clear();
            formatter.Format(in instruction, formatterOutput);

            uint? directTargetAddress = TryGetDirectTargetAddress(in instruction, out uint targetAddress)
                ? targetAddress
                : null;
            if (directTargetAddress is uint directTarget && seenTargets.Add(directTarget))
                directTargetAddresses.Add(directTarget);

            instructions.Add(
                new FunctionInstruction(
                    address,
                    instruction.Length,
                    Convert.ToHexString(_image.Span.Slice(instructionOffset, instruction.Length)),
                    instruction.Mnemonic.ToString().ToLowerInvariant(),
                    formattedInstruction.ToString(),
                    MapFlowControl(in instruction),
                    directTargetAddress
                )
            );

            switch (instruction.FlowControl)
            {
                case FlowControl.Return:
                    terminationReason = DisassemblyTerminationReason.Return;
                    terminated = true;
                    break;

                case FlowControl.UnconditionalBranch:
                case FlowControl.IndirectBranch:
                case FlowControl.Interrupt:
                case FlowControl.Exception:
                    terminationReason = DisassemblyTerminationReason.TerminalBranch;
                    terminated = true;
                    break;
            }

            if (terminated)
                break;
        }

        if (!terminated)
            terminationReason = DisassemblyTerminationReason.InstructionLimit;

        return new FunctionDisassembly(
            Module,
            functionAddress,
            [.. instructions],
            [.. directTargetAddresses],
            terminationReason
        );
    }

    private FunctionTraversal TraverseCore(uint functionAddress, int maxInstructionCount, int maxPathCount)
    {
        Queue<uint> pendingPathStarts = [];
        HashSet<uint> queuedPathStarts = [];
        List<uint> pathEntryAddresses = [];
        List<FunctionInstruction> instructions = [];
        HashSet<uint> seenInstructionAddresses = [];
        List<uint> directBranchTargetAddresses = [];
        HashSet<uint> seenBranchTargets = [];
        List<uint> directCallTargetAddresses = [];
        HashSet<uint> seenCallTargets = [];
        FunctionTraversalStatus status = FunctionTraversalStatus.Complete;
        int decodedInstructionCount = 0;
        var formatter = new NasmFormatter();
        var formattedInstruction = new StringBuilder();
        var formatterOutput = new StringBuilderFormatterOutput(formattedInstruction);

        EnqueuePathStart(functionAddress);

        while (pendingPathStarts.Count > 0)
        {
            if (pathEntryAddresses.Count >= maxPathCount)
            {
                status = FunctionTraversalStatus.PathLimit;
                break;
            }

            uint pathStart = pendingPathStarts.Dequeue();
            if (!Module.Contains(pathStart) || seenInstructionAddresses.Contains(pathStart))
                continue;

            pathEntryAddresses.Add(pathStart);

            int startOffset = _view.FileOffset(pathStart);
            var decoder = Iced.Intel.Decoder.Create(32, new MemoryCodeReader(_image[startOffset..]), ip: pathStart);

            while (true)
            {
                decoder.Decode(out Instruction instruction);
                if (decoder.LastError == DecoderError.NoMoreBytes)
                    break;

                if (decoder.LastError == DecoderError.InvalidInstruction || instruction.Code == Code.INVALID)
                {
                    status = FunctionTraversalStatus.InvalidInstruction;
                    goto CompleteTraversal;
                }

                uint address = checked((uint)instruction.IP);
                if (!seenInstructionAddresses.Add(address))
                    break;

                decodedInstructionCount++;
                if (decodedInstructionCount > maxInstructionCount)
                {
                    status = FunctionTraversalStatus.InstructionLimit;
                    goto CompleteTraversal;
                }

                int instructionOffset = _view.FileOffset(address);
                if (instructionOffset + instruction.Length > _image.Length)
                    break;

                formattedInstruction.Clear();
                formatter.Format(in instruction, formatterOutput);

                uint? directTargetAddress = TryGetDirectTargetAddress(in instruction, out uint targetAddress)
                    ? targetAddress
                    : null;
                if (directTargetAddress is uint directTarget)
                {
                    switch (instruction.FlowControl)
                    {
                        case FlowControl.Call:
                            if (seenCallTargets.Add(directTarget))
                                directCallTargetAddresses.Add(directTarget);
                            break;

                        case FlowControl.ConditionalBranch:
                        case FlowControl.UnconditionalBranch:
                            if (seenBranchTargets.Add(directTarget))
                                directBranchTargetAddresses.Add(directTarget);

                            EnqueuePathStart(directTarget);
                            break;
                    }
                }

                instructions.Add(
                    new FunctionInstruction(
                        address,
                        instruction.Length,
                        Convert.ToHexString(_image.Span.Slice(instructionOffset, instruction.Length)),
                        instruction.Mnemonic.ToString().ToLowerInvariant(),
                        formattedInstruction.ToString(),
                        MapFlowControl(in instruction),
                        directTargetAddress
                    )
                );

                switch (instruction.FlowControl)
                {
                    case FlowControl.Return:
                    case FlowControl.UnconditionalBranch:
                    case FlowControl.IndirectBranch:
                    case FlowControl.Interrupt:
                    case FlowControl.Exception:
                        goto NextPath;
                }

                uint nextAddress = checked(address + (uint)instruction.Length);
                if (!Module.Contains(nextAddress) || seenInstructionAddresses.Contains(nextAddress))
                    goto NextPath;
            }

            NextPath:
            continue;
        }

        CompleteTraversal:
        instructions.Sort(static (left, right) => left.Address.CompareTo(right.Address));
        return new FunctionTraversal(
            Module,
            functionAddress,
            [.. pathEntryAddresses],
            [.. instructions],
            [.. directBranchTargetAddresses],
            [.. directCallTargetAddresses],
            status
        );

        void EnqueuePathStart(uint pathStart)
        {
            if (Module.Contains(pathStart) && queuedPathStarts.Add(pathStart))
                pendingPathStarts.Enqueue(pathStart);
        }
    }

    private static FunctionFlowControl MapFlowControl(in Instruction instruction) =>
        instruction.FlowControl switch
        {
            FlowControl.Call => FunctionFlowControl.Call,
            FlowControl.ConditionalBranch => FunctionFlowControl.ConditionalBranch,
            FlowControl.UnconditionalBranch => FunctionFlowControl.UnconditionalBranch,
            FlowControl.IndirectCall => FunctionFlowControl.IndirectCall,
            FlowControl.IndirectBranch => FunctionFlowControl.IndirectBranch,
            FlowControl.Return => FunctionFlowControl.Return,
            FlowControl.Interrupt => FunctionFlowControl.Interrupt,
            FlowControl.Exception => FunctionFlowControl.Exception,
            _ => FunctionFlowControl.Next,
        };

    private static bool TryGetDirectTargetAddress(in Instruction instruction, out uint directTargetAddress)
    {
        switch (instruction.Op0Kind)
        {
            case OpKind.NearBranch16:
            case OpKind.NearBranch32:
            case OpKind.NearBranch64:
            {
                ulong target = instruction.NearBranchTarget;
                if (target <= uint.MaxValue)
                {
                    directTargetAddress = (uint)target;
                    return true;
                }

                break;
            }
        }

        directTargetAddress = 0;
        return false;
    }

    private static void ValidateInstructionLimit(int maxInstructionCount)
    {
        if (maxInstructionCount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxInstructionCount), "Instruction limit must be positive.");
        }
    }

    private static void ValidatePathLimit(int maxPathCount)
    {
        if (maxPathCount <= 0)
            throw new ArgumentOutOfRangeException(nameof(maxPathCount), "Path limit must be positive.");
    }

    private sealed class MemoryCodeReader(ReadOnlyMemory<byte> code) : CodeReader
    {
        private readonly ReadOnlyMemory<byte> _code = code;
        private int _index;

        public override int ReadByte() => _index < _code.Length ? _code.Span[_index++] : -1;
    }

    private sealed class StringBuilderFormatterOutput(StringBuilder builder) : FormatterOutput
    {
        private readonly StringBuilder _builder = builder;

        public override void Write(string text, FormatterTextKind kind) => _builder.Append(text);
    }
}
