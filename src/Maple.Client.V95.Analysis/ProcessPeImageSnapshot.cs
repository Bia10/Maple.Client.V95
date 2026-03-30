using System.Runtime.Versioning;
using Maple.Memory;
using Maple.Native;
using Maple.Process;

namespace Maple.Client.V95.Analysis;

/// <summary>
/// Managed snapshot of one attached process module image for analysis.
/// </summary>
/// <remarks>
/// This helper captures one live module image into managed memory so disassembly and image analysis
/// can work against a stable byte snapshot instead of issuing many remote reads. When the captured
/// image base fits in the x86 Maple address space, callers can also obtain a <see cref="NativeImageView"/>.
/// </remarks>
[SupportedOSPlatform("windows")]
public sealed class ProcessPeImageSnapshot
{
    private readonly ReadOnlyMemory<byte> _image;

    private ProcessPeImageSnapshot(ProcessModuleInfo module, ReadOnlyMemory<byte> image)
    {
        Module = module;
        _image = image;
    }

    /// <summary>
    /// Gets the captured module metadata.
    /// </summary>
    public ProcessModuleInfo Module { get; }

    /// <summary>
    /// Gets the captured PE image bytes.
    /// </summary>
    public ReadOnlyMemory<byte> Image => _image;

    /// <summary>
    /// Gets the module base address in the target process.
    /// </summary>
    public nuint ImageBase => Module.BaseAddress;

    /// <summary>
    /// Captures the attached process' main module through <paramref name="processHandle"/> and <paramref name="memoryAccessor"/>.
    /// </summary>
    public static ProcessPeImageSnapshot CaptureMainModule(
        ProcessHandle processHandle,
        MemoryAccessor memoryAccessor,
        int maxAttempts = 3
    )
    {
        ArgumentNullException.ThrowIfNull(processHandle);
        ArgumentNullException.ThrowIfNull(memoryAccessor);
        return Capture(processHandle.GetMainModule(), memoryAccessor, maxAttempts);
    }

    /// <summary>
    /// Captures the module described by <paramref name="module"/> through <paramref name="memoryAccessor"/>.
    /// </summary>
    public static ProcessPeImageSnapshot Capture(
        ProcessModuleInfo module,
        MemoryAccessor memoryAccessor,
        int maxAttempts = 3
    )
    {
        ArgumentNullException.ThrowIfNull(memoryAccessor);

        if (module.ImageSize <= 0)
            throw new ArgumentOutOfRangeException(nameof(module), "Module image size must be positive.");

        if (module.BaseAddress > uint.MaxValue)
        {
            throw new InvalidOperationException(
                $"Module base 0x{module.BaseAddress:X} does not fit in the x86 address space exposed by {nameof(MemoryAccessor)}."
            );
        }

        byte[] image = GC.AllocateUninitializedArray<byte>(module.ImageSize);
        memoryAccessor.ReadStable(checked((uint)module.BaseAddress), image, maxAttempts);
        return new ProcessPeImageSnapshot(module, image);
    }

    /// <summary>
    /// Attempts to create an x86 <see cref="NativeImageView"/> over this snapshot.
    /// </summary>
    public bool TryCreateView(out NativeImageView? view)
    {
        if (Module.BaseAddress > uint.MaxValue)
        {
            view = null;
            return false;
        }

        view = new NativeImageView(_image, (uint)Module.BaseAddress);
        return true;
    }

    /// <summary>
    /// Creates an x86 <see cref="NativeImageView"/> over this snapshot.
    /// </summary>
    public NativeImageView CreateView()
    {
        if (TryCreateView(out NativeImageView? view))
            return view!;

        throw new InvalidOperationException(
            $"Module base 0x{Module.BaseAddress:X} does not fit in the x86 Maple address space required by {nameof(NativeImageView)}."
        );
    }
}
