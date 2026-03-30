using System.Runtime.CompilerServices;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Running;
using Maple.Client.V95.ComparisonBenchmarks;

// Usage (via task runner): dotnet Build.cs comparison-bench
// Direct:                  dotnet run -c Release -- run
if (args is not ["run"])
{
    Console.Error.WriteLine("Usage: dotnet run -c Release -- run");
    return 1;
}

var config = ManualConfig
    .CreateMinimumViable()
    .WithOptions(ConfigOptions.DisableOptimizationsValidator)
    .AddColumnProvider(DefaultColumnProviders.Instance)
    .AddExporter(MarkdownExporter.GitHub)
    .AddLogger(ConsoleLogger.Default);

var summary = BenchmarkRunner.Run<TestBench>(config);

var machineName = string.Concat(Environment.MachineName.Split(Path.GetInvalidFileNameChars()));
var outputDir = Path.Combine(RepoRoot(), "benchmarks", machineName);
Directory.CreateDirectory(outputDir);

File.WriteAllText(Path.Combine(outputDir, "Versions.txt"), $".NET {Environment.Version}");

var mdSource = Directory.GetFiles(summary.ResultsDirectoryPath, "*TestBench-report-github.md").FirstOrDefault();
if (mdSource is not null)
    File.Copy(mdSource, Path.Combine(outputDir, "TestBench.md"), overwrite: true);
else
    Console.Error.WriteLine("WARNING: No benchmark Markdown found in " + summary.ResultsDirectoryPath);

return 0;

static string RepoRoot([CallerFilePath] string path = "") =>
    Path.GetFullPath(Path.Combine(Path.GetDirectoryName(path)!, "..", ".."));
