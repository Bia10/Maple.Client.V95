#!/usr/bin/env dotnet
// Task runner for the repository.
// Usage: dotnet Build.cs <command> [args]
// Requires: .NET 10 SDK (file-based apps are a .NET 10 feature).
// Invoke from any directory — [CallerFilePath] locates the repo root at compile time.

#:property PublishAot=false

using System.Diagnostics;
using System.Runtime.CompilerServices;

var repoRoot = RepoRoot();
var cmd = args.Length > 0 ? args[0] : "help";

switch (cmd)
{
    case "bench":
        Run(
            "dotnet",
            "run --project src/Maple.Client.V95.Benchmarks/Maple.Client.V95.Benchmarks.csproj"
                + " -c Release -- --filter \"*\" --join",
            repoRoot
        );
        break;

    case "comparison-bench":
        Run(
            "dotnet",
            "run --project src/Maple.Client.V95.ComparisonBenchmarks/Maple.Client.V95.ComparisonBenchmarks.csproj"
                + " -c Release -- run",
            repoRoot
        );
        break;

    case "disasm":
        Run(
            "dotnet",
            "run --project src/Maple.Client.V95.Benchmarks/Maple.Client.V95.Benchmarks.csproj"
                + " -c Release -- --filter \"*ClientStructs*\" --disasm",
            repoRoot
        );
        break;

    case "pack":
        Run("dotnet", "pack -c Release", repoRoot);
        break;

    case "publish-tester":
        var rid = args.Length > 1 ? args[1] : System.Runtime.InteropServices.RuntimeInformation.RuntimeIdentifier;
        Run(
            "dotnet",
            $"publish src/Maple.Client.V95.Tester/Maple.Client.V95.Tester.csproj -c Release -r {rid} --self-contained",
            repoRoot
        );
        break;

    case "prettier":
        Run("npx", "prettier --write \"**/*.{yml,yaml,json,md}\" --ignore-path .gitignore", repoRoot);
        break;

    case "format":
        var verify = args.Length > 1 && args[1] == "check";
        Run("dotnet", "tool restore", repoRoot);
        Run("dotnet", verify ? "csharpier check ." : "csharpier format .", repoRoot);
        Run("dotnet", verify ? "format style --verify-no-changes" : "format style", repoRoot);
        Run("dotnet", verify ? "format analyzers --verify-no-changes" : "format analyzers", repoRoot);
        break;

    case "rename":
        if (args.Length < 3)
        {
            Console.Error.WriteLine("Usage: dotnet Build.cs rename <OldName> <NewName>");
            return 1;
        }
        RenameAll(repoRoot, args[1], args[2]);
        break;

    default:
        Console.WriteLine(
            """
            Usage: dotnet Build.cs <command>

            Commands:
              bench                     Run BenchmarkDotNet benchmarks
              comparison-bench          Run comparison benchmarks (writes to benchmarks/)
              disasm                    Inspect JIT disassembly via BDN disassembler
              pack                      Pack NuGet package locally
              publish-tester [RID]      Publish tester app (defaults to current platform RID)
              format [check]            Format C# (CSharpier) + code style (dotnet format); 'check' verifies only
              prettier                  Format YAML/Markdown/JSON via Prettier (requires Node)
              rename <OldName> <New>    Rename template throughout repository
            """
        );
        break;
}

return 0;

// ─── Helpers ─────────────────────────────────────────────────────────────────

static void Run(string exe, string arguments, string workingDir)
{
    var psi = new ProcessStartInfo(exe, arguments) { WorkingDirectory = workingDir, UseShellExecute = false };
    using var p = Process.Start(psi) ?? throw new Exception($"Failed to start '{exe}'");
    p.WaitForExit();
    if (p.ExitCode != 0)
        throw new Exception($"'{exe}' exited with code {p.ExitCode}");
}

static void RenameAll(string repoRoot, string oldName, string newName)
{
    var ignore = new[]
    {
        Path.DirectorySeparatorChar + ".git" + Path.DirectorySeparatorChar,
        Path.DirectorySeparatorChar + "artifacts" + Path.DirectorySeparatorChar,
    };

    var files = Directory
        .EnumerateFiles(repoRoot, "*", SearchOption.AllDirectories)
        .Where(f => !ignore.Any(seg => f.Contains(seg)))
        .ToList();

    var textExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        ".cs",
        ".csproj",
        ".props",
        ".targets",
        ".json",
        ".xml",
        ".yml",
        ".yaml",
        ".md",
        ".txt",
        ".slnx",
        ".sln",
        ".editorconfig",
        ".gitignore",
        ".gitattributes",
        ".config",
    };

    foreach (var file in files)
    {
        if (!textExtensions.Contains(Path.GetExtension(file)))
        {
            continue;
        }

        string content;
        try
        {
            content = File.ReadAllText(file);
        }
        catch
        {
            continue;
        }

        if (content.Contains(oldName))
            File.WriteAllText(file, content.Replace(oldName, newName));

        var name = Path.GetFileName(file);
        if (name.Contains(oldName))
        {
            var newPath = Path.Combine(Path.GetDirectoryName(file)!, name.Replace(oldName, newName));
            File.Move(file, newPath);
        }
    }

    var dirs = Directory
        .EnumerateDirectories(repoRoot, "*", SearchOption.AllDirectories)
        .Where(d => Path.GetFileName(d).Contains(oldName) && !ignore.Any(seg => d.Contains(seg)))
        .OrderByDescending(d => d.Length)
        .ToList();

    foreach (var dir in dirs)
    {
        var parent = Path.GetDirectoryName(dir)!;
        Directory.Move(dir, Path.Combine(parent, Path.GetFileName(dir).Replace(oldName, newName)));
    }

    Console.WriteLine($"Renamed '{oldName}' → '{newName}' throughout repository.");
}

// Returns the directory containing Build.cs (repo root) regardless of cwd.
static string RepoRoot([CallerFilePath] string path = "") => Path.GetDirectoryName(path)!;
