using BenchmarkDotNet.Attributes;

namespace Maple.Client.V95.Benchmarks;

public class ClientStructsBench
{
    [Benchmark]
    public int RegistryFieldCount() => ClientStructs.Registry.Fields.Count;
}
