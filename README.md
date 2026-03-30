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
// Example code will be auto-populated from ReadMeTest.cs by tests
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

###### Results will be populated here after running `dotnet Build.cs comparison-bench` then `dotnet test`

## Example Catalogue

The following examples are available in [ReadMeTest.cs](src/Maple.Client.V95.DocTest/ReadMeTest.cs).

### Example - Empty

```csharp
// Example code will be auto-populated from ReadMeTest.cs by tests
```

## Public API Reference

See [docs/PublicApi.md](docs/PublicApi.md) for the complete auto-generated public API reference.

> **Note**: `docs/PublicApi.md` is auto-updated by the `ReadMeTest_PublicApi` test on every `dotnet test` run. Do not edit it manually.
