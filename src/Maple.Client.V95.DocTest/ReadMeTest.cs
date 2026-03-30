#pragma warning disable CA2007 // ConfigureAwait
#pragma warning disable CA1822 // Mark as static

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using Maple.Client.V95.Analysis;
using Maple.Client.V95.Runtime;
using Maple.Memory;
using Maple.Process;
using PublicApiGenerator;

namespace Maple.Client.V95.DocTest;

[NotInParallel]
[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
public partial class ReadMeTest
{
    static readonly string s_testSourceFilePath = SourceFile();

    // Navigate from src/Maple.Client.V95.DocTest/ up to repo root (2 levels: DocTest → src → root)
    static readonly string s_rootDirectory =
        Path.GetFullPath(Path.Combine(Path.GetDirectoryName(s_testSourceFilePath)!, "..", ".."))
        + Path.DirectorySeparatorChar;

    static readonly string s_readmeFilePath = s_rootDirectory + "README.md";

    // ─────────────────────────────────────────────────────────────
    // SECTION 1: README example methods — bodies are snipped into README.md.
    // ─────────────────────────────────────────────────────────────

    [Test]
    public void ReadMeTest_()
    {
        // Look up a known struct field by name — returns the byte offset within the struct.
        bool found = ClientStructs.Registry.TryGetField("CWvsContext", "WorldId", out StructField worldId);
        Debug.Assert(found);
        Debug.Assert(worldId.Offset == ClientStructs.Offsets.CWvsContext.WorldId);

        // Walk all known struct names in the registry.
        foreach (string structName in ClientStructs.Registry.StructNames)
            Debug.Assert(structName.Length > 0);

        // Look up a static address constant.
        uint loginPtr = ClientStructs.Addresses.CWvsContextSingletonPtr;
        Debug.Assert(loginPtr != 0);
    }

    [Test]
    public void ReadMeTest_StructRegistryAndOffsets()
    {
        // Iterate all fields registered for one struct.
        IReadOnlyDictionary<string, StructField> loginFields = ClientStructs.Registry.GetFields("CLogin");
        foreach ((string name, StructField field) in loginFields)
            Console.WriteLine($"CLogin.{name} @ +0x{field.Offset:X}");

        // Look up a single field — offset matches the typed constant.
        ClientStructs.Registry.TryGetField("CLogin", "LoginStep", out StructField step);
        Debug.Assert(step.Offset == ClientStructs.Offsets.CLogin.LoginStep);

        // The singleton map enumerates all pointer-table addresses with their names.
        foreach ((uint addr, string name) in ClientStructs.Singletons)
            Console.WriteLine($"0x{addr:X8} → {name}");
    }

    [Test]
    [SupportedOSPlatform("windows")]
    public void ReadMeTest_RuntimeSingletonResolution()
    {
        // Skip when not attached to a live MapleStory process.
        if (!ProcessHandle.TryAttach("MapleStory", out ProcessHandle? handle))
            return;
        using ProcessHandle process = handle!;
        using WindowsProcessMemory memory = WindowsProcessMemory.Open(process);
        using var accessor = new MemoryAccessor(memory);
        var singletons = new SingletonResolver(accessor);

        // Resolve CWvsContext by name — returns false when the client is not running.
        if (!singletons.TryResolve("CWvsContext", out ResolvedSingleton ctx))
            return;

        Console.WriteLine($"{ctx.Name}: instance @ 0x{ctx.InstanceAddress:X8}");

        // Resolve the full login-flow state when the client is in the login screen.
        var resolver = new LoginStateResolver(accessor);
        if (!resolver.TryResolve(out ResolvedLoginState state))
            return;

        Console.WriteLine($"LoginStep={state.Step}  World={state.ContextWorldId}  Channel={state.ContextChannelId}");
    }

    [Test]
    [SupportedOSPlatform("windows")]
    public void ReadMeTest_AnalysisDisassembly()
    {
        // Skip when not attached to a live MapleStory process.
        if (!ProcessHandle.TryAttach("MapleStory", out ProcessHandle? handle))
            return;
        using ProcessHandle process = handle!;
        using WindowsProcessMemory memory = WindowsProcessMemory.Open(process);
        using var accessor = new MemoryAccessor(memory);

        // Capture the main module image into managed memory — one stable snapshot for all analysis.
        ProcessPeImageSnapshot snapshot = ProcessPeImageSnapshot.CaptureMainModule(process, accessor);

        // Disassemble one known function from the snapshot.
        var analyzer = new RuntimeFunctionAnalyzer(snapshot);
        FunctionDisassembly disassembly = analyzer.Disassemble(ClientStructs.Addresses.SendLoginPacketFunc);

        foreach (FunctionInstruction instr in disassembly.Instructions)
            Console.WriteLine(instr);
    }

    // ─────────────────────────────────────────────────────────────
    // SECTION 2: README sync tests — run only on latest TFM
    // ─────────────────────────────────────────────────────────────

#if NET10_0
    [Test]
#endif
    public void ReadMeTest_UpdateExampleCodeInMarkdown()
    {
        if (!File.Exists(s_readmeFilePath))
        {
            return;
        }

        var readmeLines = File.ReadAllLines(s_readmeFilePath);
        var testSourceLines = File.ReadAllLines(s_testSourceFilePath);

        var testBlocksToUpdate = new (string StartLineContains, string ReadmeLineBeforeCodeBlock)[]
        {
            (nameof(ReadMeTest_) + "()", "## Example"),
            (nameof(ReadMeTest_StructRegistryAndOffsets) + "()", "### Example - Struct registry and offsets"),
            (nameof(ReadMeTest_RuntimeSingletonResolution) + "()", "### Example - Runtime singleton resolution"),
            (nameof(ReadMeTest_AnalysisDisassembly) + "()", "### Example - Analysis disassembly"),
        };

        readmeLines = UpdateReadme(
            testSourceLines,
            readmeLines,
            testBlocksToUpdate,
            sourceStartLineOffset: 2,
            "    }",
            sourceEndLineOffset: 0,
            sourceWhitespaceToRemove: 8
        );

        var newReadme = string.Join(Environment.NewLine, readmeLines) + Environment.NewLine;
        File.WriteAllText(s_readmeFilePath, newReadme, System.Text.Encoding.UTF8);
    }

#if NET10_0
    [Test]
#endif
    public void ReadMeTest_PublicApi()
    {
        var publicApiFilePath = Path.Combine(s_rootDirectory, "docs", "PublicApi.md");
        if (!File.Exists(publicApiFilePath))
        {
            return;
        }

        var publicApi = typeof(ClientStructs).Assembly.GeneratePublicApi();
        var apiLines = File.ReadAllLines(publicApiFilePath);
        apiLines = ReplaceReadmeLines(apiLines, [publicApi], "## Public API Reference", "```csharp", 1, "```", 0);
        var newContent = string.Join(Environment.NewLine, apiLines) + Environment.NewLine;
        File.WriteAllText(publicApiFilePath, newContent, System.Text.Encoding.UTF8);
    }

    // ─────────────────────────────────────────────────────────────
    // INFRASTRUCTURE — do not modify
    // ─────────────────────────────────────────────────────────────

    static string[] UpdateReadme(
        string[] sourceLines,
        string[] readmeLines,
        (string StartLineContains, string ReadmeLineBefore)[] blocksToUpdate,
        int sourceStartLineOffset,
        string sourceEndLineStartsWith,
        int sourceEndLineOffset,
        int sourceWhitespaceToRemove,
        string readmeStartLineStartsWith = "```csharp",
        int readmeStartLineOffset = 1,
        string readmeEndLineStartsWith = "```",
        int readmeEndLineOffset = 0
    )
    {
        foreach (var (startLineContains, readmeLineBeforeBlock) in blocksToUpdate)
        {
            var sourceExampleLines = SnipLines(
                sourceLines,
                startLineContains,
                sourceStartLineOffset,
                sourceEndLineStartsWith,
                sourceEndLineOffset,
                sourceWhitespaceToRemove
            );
            readmeLines = ReplaceReadmeLines(
                readmeLines,
                sourceExampleLines,
                readmeLineBeforeBlock,
                readmeStartLineStartsWith,
                readmeStartLineOffset,
                readmeEndLineStartsWith,
                readmeEndLineOffset
            );
        }

        return readmeLines;
    }

    static string[] ReplaceReadmeLines(
        string[] readmeLines,
        string[] newLines,
        string readmeLineBeforeBlock,
        string readmeStartLineStartsWith,
        int readmeStartLineOffset,
        string readmeEndLineStartsWith,
        int readmeEndLineOffset
    )
    {
        var beforeIndex = Array.FindIndex(
            readmeLines,
            l => l.StartsWith(readmeLineBeforeBlock, StringComparison.Ordinal)
        );
        if (beforeIndex < 0)
        {
            throw new ArgumentException($"README line '{readmeLineBeforeBlock}' not found.");
        }

        var replaceStart =
            Array.FindIndex(
                readmeLines,
                beforeIndex,
                l => l.StartsWith(readmeStartLineStartsWith, StringComparison.Ordinal)
            ) + readmeStartLineOffset;
        Debug.Assert(replaceStart >= 0);
        var replaceEnd =
            Array.FindIndex(
                readmeLines,
                replaceStart,
                l => l.StartsWith(readmeEndLineStartsWith, StringComparison.Ordinal)
            ) + readmeEndLineOffset;

        return [.. readmeLines[..replaceStart].AsEnumerable(), .. newLines, .. readmeLines[replaceEnd..]];
    }

    static string[] SnipLines(
        string[] sourceLines,
        string startLineContains,
        int startLineOffset,
        string endLineStartsWith,
        int endLineOffset,
        int whitespaceToRemove = 8
    )
    {
        var start =
            Array.FindIndex(sourceLines, l => l.Contains(startLineContains, StringComparison.Ordinal))
            + startLineOffset;
        var end =
            Array.FindIndex(sourceLines, start, l => l.StartsWith(endLineStartsWith, StringComparison.Ordinal))
            + endLineOffset;
        return
        [
            .. sourceLines[start..end]
                .Select(l => l.Length > whitespaceToRemove ? l[whitespaceToRemove..] : l.TrimStart()),
        ];
    }

    static string SourceFile([CallerFilePath] string sourceFilePath = "") => sourceFilePath;
}
