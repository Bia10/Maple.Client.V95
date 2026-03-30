# Maple.Client.V95

![.NET](https://img.shields.io/badge/net10.0-5C2D91?logo=.NET&labelColor=gray)
![C#](https://img.shields.io/badge/C%23-14.0-239120?labelColor=gray)
[![Build Status](https://github.com/Bia10/Maple.Client.V95/actions/workflows/dotnet.yml/badge.svg?branch=main)](https://github.com/Bia10/Maple.Client.V95/actions/workflows/dotnet.yml)
[![codecov](https://codecov.io/gh/Bia10/Maple.Client.V95/branch/main/graph/badge.svg)](https://codecov.io/gh/Bia10/Maple.Client.V95)
[![Nuget](https://img.shields.io/nuget/v/Maple.Client.V95?color=purple)](https://www.nuget.org/packages/Maple.Client.V95/)
[![License](https://img.shields.io/github/license/Bia10/Maple.Client.V95)](https://github.com/Bia10/Maple.Client.V95/blob/main/LICENSE)

MapleStory GMS v95 client structures, runtime memory access, and function disassembly helpers for Maple memory tooling.

⭐ Please star this project if you like it. ⭐

[Example](#example) | [Example Catalogue](#example-catalogue) | [Public API](docs/PublicApi.md)

## Example

```csharp
// Demonstrate core API — client struct registry and address constants.
var registry = ClientStructs.Registry;
_ = registry.StructNames;

var addr = ClientStructs.Addresses.CWvsContextSingletonPtr;
_ = addr;
```

For more examples see [Example Catalogue](#example-catalogue).

## Packages

| Package | Description |
| --- | --- |
| `Maple.Client.V95` | Client struct definitions, addresses, field offsets, and version-specific schemas |
| `Maple.Client.V95.Analysis` | Snapshot-backed x86 runtime function disassembly helpers |
| `Maple.Client.V95.Runtime` | Attached-process runtime helpers for memory access, singleton resolution, and login-flow interaction |

## Benchmarks

Benchmarks.

### Detailed Benchmarks

#### Comparison Benchmarks

##### TestBench Benchmark Results

######WIN-4JIQ1EAG9C3 - TestBench Benchmark Results (.NET 10.0.5)

| Method             | Count | Mean     | Error     | StdDev    | Ratio |
|------------------- |------ |---------:|----------:|----------:|------:|
| MapleClientV95____ | 25000 | 5.586 ns | 0.0488 ns | 0.0457 ns |  1.00 |


## Example Catalogue

The following examples are available in [ReadMeTest.cs](src/Maple.Client.V95.DocTest/ReadMeTest.cs).

### Example - Empty

```csharp
// Demonstrate core API — client struct registry and address constants.
var registry = ClientStructs.Registry;
_ = registry.StructNames;

var addr = ClientStructs.Addresses.CWvsContextSingletonPtr;
_ = addr;
```

## Public API Reference

See [docs/PublicApi.md](docs/PublicApi.md) for the complete auto-generated public API reference.

> **Note**: `docs/PublicApi.md` is auto-updated by the `ReadMeTest_PublicApi` test on every `dotnet test` run. Do not edit it manually.
