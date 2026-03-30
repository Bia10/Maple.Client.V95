#pragma warning disable CA2007 // ConfigureAwait
#pragma warning disable CA1822 // Mark as static

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
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
    // SECTION 1: Write your README example code here.
    // These methods are directly snipped into README.md by the update test.
    // ─────────────────────────────────────────────────────────────

    [Test]
    public void ReadMeTest_()
    {
        // Demonstrate core API — client struct registry and address constants.
        var registry = ClientStructs.Registry;
        _ = registry.StructNames;

        var addr = ClientStructs.Addresses.CWvsContextSingletonPtr;
        _ = addr;
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
            (nameof(ReadMeTest_) + "()", "### Example - Empty"),
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
    public void ReadMeTest_UpdateBenchmarksInMarkdown()
    {
        if (!File.Exists(s_readmeFilePath))
        {
            return;
        }

        var readmeFilePath = s_readmeFilePath;
        var benchmarkFileNameToConfig = new Dictionary<
            string,
            (string Description, string ReadmeBefore, string ReadmeEnd, string SectionPrefix)
        >()
        {
            {
                "TestBench.md",
                ("TestBench Benchmark Results", "##### TestBench Benchmark Results", "## Example Catalogue", "######")
            },
        };

        var benchmarksDirectory = Path.Combine(s_rootDirectory, "benchmarks");
        if (!Directory.Exists(benchmarksDirectory))
        {
            return;
        }

        var processorDirectories = Directory.EnumerateDirectories(benchmarksDirectory).ToArray();
        var readmeLines = File.ReadAllLines(readmeFilePath);

        foreach (var (fileName, config) in benchmarkFileNameToConfig)
        {
            var all = "";
            foreach (var processorDirectory in processorDirectories)
            {
                var contentsFilePath = Path.Combine(processorDirectory, fileName);
                if (!File.Exists(contentsFilePath))
                {
                    continue;
                }

                var versionsFilePath = Path.Combine(processorDirectory, "Versions.txt");
                var versions = File.ReadAllText(versionsFilePath);
                var contents = File.ReadAllText(contentsFilePath);
                var processor = processorDirectory
                    .Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                    .Last();
                var section = $"{config.SectionPrefix}{processor} - {config.Description} ({versions})";
                var benchmarkTable = contents.Substring(contents.IndexOf('|'));
                all += $"{section}{Environment.NewLine}{Environment.NewLine}{benchmarkTable}{Environment.NewLine}";
            }

            if (all.Length == 0)
            {
                continue;
            }

            readmeLines = ReplaceReadmeLines(
                readmeLines,
                [all],
                config.ReadmeBefore,
                config.SectionPrefix,
                0,
                config.ReadmeEnd,
                0
            );
        }

        var newReadme = string.Join(Environment.NewLine, readmeLines) + Environment.NewLine;
        File.WriteAllText(readmeFilePath, newReadme, System.Text.Encoding.UTF8);
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

        return readmeLines[..replaceStart].AsEnumerable().Concat(newLines).Concat(readmeLines[replaceEnd..]).ToArray();
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
        return sourceLines[start..end]
            .Select(l => l.Length > whitespaceToRemove ? l.Remove(0, whitespaceToRemove) : l.TrimStart())
            .ToArray();
    }

    static string SourceFile([CallerFilePath] string sourceFilePath = "") => sourceFilePath;
}
