using System.Linq;
using BenchmarkDotNet.Attributes;

namespace Maple.Client.V95.Benchmarks;

public class ClientStructsBench
{
    [Benchmark]
    public int RegistryStructCount() => ClientStructs.Registry.StructNames.Count();
}
