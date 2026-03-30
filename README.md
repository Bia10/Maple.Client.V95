# Maple.Client.V95

![.NET](https://img.shields.io/badge/net10.0-5C2D91?logo=.NET&labelColor=gray)
![C#](https://img.shields.io/badge/C%23-14.0-239120?labelColor=gray)
![Windows](https://img.shields.io/badge/Windows-0078D6?logo=windows&labelColor=gray)
[![Build Status](https://github.com/Bia10/Maple.Client.V95/actions/workflows/dotnet.yml/badge.svg?branch=main)](https://github.com/Bia10/Maple.Client.V95/actions/workflows/dotnet.yml)
[![codecov](https://codecov.io/gh/Bia10/Maple.Client.V95/branch/main/graph/badge.svg)](https://codecov.io/gh/Bia10/Maple.Client.V95)
[![Nuget](https://img.shields.io/nuget/v/Maple.Client.V95?color=purple)](https://www.nuget.org/packages/Maple.Client.V95/)
[![License](https://img.shields.io/github/license/Bia10/Maple.Client.V95)](https://github.com/Bia10/Maple.Client.V95/blob/main/LICENSE)

GMS v95 client struct definitions, singleton addresses, field offsets, attached-process runtime access, and x86 function disassembly for Maple memory tooling.

⭐ Please star this project if you like it. ⭐

[Example](#example) | [Example Catalogue](#example-catalogue) | [Public API](docs/PublicApi.md)

## Packages

| Package | NuGet | OS | Description |
| ------- | ----- | -- | ----------- |
| **Maple.Client.V95** | [![Nuget](https://img.shields.io/nuget/v/Maple.Client.V95?color=purple)](https://www.nuget.org/packages/Maple.Client.V95/) | cross-platform | Struct definitions, singleton addresses, and field offset registry for GMS v95 |
| **Maple.Client.V95.Runtime** | [![Nuget](https://img.shields.io/nuget/v/Maple.Client.V95.Runtime?color=purple)](https://www.nuget.org/packages/Maple.Client.V95.Runtime/) | Windows | Attached-process singleton resolution, field reads, and login-flow state snapshots |
| **Maple.Client.V95.Analysis** | [![Nuget](https://img.shields.io/nuget/v/Maple.Client.V95.Analysis?color=purple)](https://www.nuget.org/packages/Maple.Client.V95.Analysis/) | Windows | Snapshot-backed x86 function disassembly and control-flow analysis |

All packages are trimmable and AOT/NativeAOT compatible.

## Example

```csharp
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
```

For more examples see [Example Catalogue](#example-catalogue).

## Example Catalogue

The following examples are available in [ReadMeTest.cs](src/Maple.Client.V95.DocTest/ReadMeTest.cs).

### Example - Struct registry and offsets

```csharp
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
```

### Example - Runtime singleton resolution

```csharp
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
```

### Example - Analysis disassembly

```csharp
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
```

## Public API Reference

See [docs/PublicApi.md](docs/PublicApi.md) for the complete auto-generated public API reference.

> **Note**: `docs/PublicApi.md` is auto-updated by the `ReadMeTest_PublicApi` test on every `dotnet test` run. Do not edit it manually.
