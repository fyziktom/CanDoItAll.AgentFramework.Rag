# CanDoItAll Agent Framework RAG

[![CI](https://github.com/fyziktom/CanDoItAll.AgentFramework.Rag/actions/workflows/ci.yml/badge.svg?branch=main&event=push)](https://github.com/fyziktom/CanDoItAll.AgentFramework.Rag/actions/workflows/ci.yml)
[![.NET 10](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/download/dotnet/10.0)
[![License](https://img.shields.io/badge/license-MIT--derived%20with%20website%20link-blue.svg)](LICENSE)

Provider-neutral retrieval-augmented generation contracts and a Qdrant
implementation for CanDoItAll applications.

## Ownership

This repository owns:

- RAG driver, embedding, collection, knowledge, filter, and search contracts;
- provider selection and dependency-injection composition;
- the Qdrant implementation and its SDK mappings;
- a console sample and interactive Blazor validation sandbox;
- package construction and package-content validation.

It does not own an embedding-provider SDK, a hosted Qdrant service, or the
shared CanDoItAll component libraries. Applications supply a production
`IRagEmbeddingGenerator`; the sandbox consumes released BaseLib packages from
nuget.org.

## Packages and projects

| Project | Purpose | Published |
|---|---|---|
| `CanDoItAll.AgentFramework.Rag.Driver` | Provider-neutral contracts, models, factory, and embedding abstractions | NuGet package |
| `CanDoItAll.AgentFramework.Rag.Qdrant` | Qdrant provider and isolated `Qdrant.Client` mappings | NuGet package |
| `CanDoItAll.AgentFramework.Rag.Sample` | Console composition and Qdrant smoke host | No |
| `CanDoItAll.AgentFramework.Rag.Sandbox` | BaseLib-backed interactive RAG workbench | No |
| `CanDoItAll.AgentFramework.Rag.Tests` | Unit, mapping, composition, and sandbox-service tests | No |

## Requirements

- The .NET SDK pinned by [`global.json`](global.json)
- nuget.org access for package restore
- Qdrant gRPC when running the non-dry console sample

Restore with the repository-owned source configuration:

```powershell
dotnet restore CanDoItAll.AgentFramework.Rag.slnx --configfile NuGet.config
```

## Install

Applications normally install the provider package, which brings the Driver
contracts transitively:

```powershell
dotnet add package CanDoItAll.AgentFramework.Rag.Qdrant
```

Provider authors can install only the neutral contract package:

```powershell
dotnet add package CanDoItAll.AgentFramework.Rag.Driver
```

## Compose the Qdrant driver

Register the application's embedding implementation before the Qdrant
provider. The local hashing generator is deterministic and explicitly opt-in;
it is suitable for samples and tests, not as an implicit production default.

```csharp
using CanDoItAll.AgentFramework.Rag.Driver.DependencyInjection;
using CanDoItAll.AgentFramework.Rag.Driver.Embeddings;
using CanDoItAll.AgentFramework.Rag.Driver.Models;
using CanDoItAll.AgentFramework.Rag.Qdrant.DependencyInjection;

services.AddLocalHashingRagEmbeddingGenerator(
    options => options.Dimension = 384);

services.AddQdrantRagDriver(
    configureQdrant: options =>
    {
        options.Host = "localhost";
        options.Port = 6334;
    },
    configureFactory: options =>
    {
        options.DefaultCollection = new RagCollectionOptions
        {
            CollectionName = "candoitall-knowledge",
            VectorSize = 384,
            Distance = RagDistanceMetric.Cosine
        };
    });
```

Production applications instead register their model-backed implementation:

```csharp
services.AddSingleton<IRagEmbeddingGenerator, ApplicationEmbeddingGenerator>();
services.AddQdrantRagDriver(/* configuration */);
```

Resolve `IRagDriver` for the configured provider or `IRagDriverFactory` when a
caller needs explicit factory options.

## Architecture

The dependency direction is intentionally one-way:

```text
Sample / Sandbox / Tests
          |
          v
Qdrant provider implementation ---> Qdrant.Client
          |
          v
Provider-neutral Driver
```

The factory selects an `IRagDriverProvider`; dependency injection constructs
each provider with its concrete dependencies. The provider contract never
receives an `IServiceProvider`. Qdrant mapping owners are internal to the
provider package, and the Driver project contains no Qdrant SDK reference or
provider-specific default.

See [`docs/architecture.md`](docs/architecture.md) for the decisions,
boundaries, responsibility inventory, and release acceptance criteria. The
completed governance result is in
[`docs/architecture-review.md`](docs/architecture-review.md).

## Console sample

Validate composition without calling Qdrant:

```powershell
dotnet run --project src/CanDoItAll.AgentFramework.Rag.Sample -- --dry-run
```

Run the full sample against Qdrant gRPC:

```powershell
$env:QDRANT_HOST = "localhost"
$env:QDRANT_GRPC_PORT = "6334"
$env:RAG_COLLECTION = "candoitall-knowledge-sample"
$env:RAG_VECTOR_SIZE = "64"
dotnet run --project src/CanDoItAll.AgentFramework.Rag.Sample
```

## Blazor sandbox

Run the interactive sandbox:

```powershell
dotnet run --project src/CanDoItAll.AgentFramework.Rag.Sandbox `
  --urls http://localhost:5046
```

The BaseLib `SideMenu` exposes three routed workspaces:

- `/collections` for collection CRUD and filtering;
- `/records` for collection-scoped record CRUD and filtering;
- `/similarity-search` for multi-collection vector search.

The sandbox uses session-scoped in-memory state and explicitly registered
local deterministic embeddings. It does not require Qdrant, an external model,
or model files. Browser and responsive evidence is recorded in
[`docs/ui-validation.md`](docs/ui-validation.md).

## Validate

Run the repository gate:

```powershell
./tools/validation/Test-Repository.ps1
```

Or run the individual .NET steps:

```powershell
dotnet restore CanDoItAll.AgentFramework.Rag.slnx --configfile NuGet.config
dotnet build CanDoItAll.AgentFramework.Rag.slnx --configuration Release --no-restore
dotnet test CanDoItAll.AgentFramework.Rag.slnx --configuration Release --no-build --no-restore
```

## Build packages

Preview the exact package operation:

```powershell
./tools/deployment/nugets/Build-NuGets.ps1 `
  -Version 0.2.0 `
  -WhatIf
```

Build, test, pack, and validate the two intended packages:

```powershell
./tools/deployment/nugets/Build-NuGets.ps1 `
  -Version 0.2.0 `
  -OutputDirectory artifacts/packages/0.2.0
```

The script never publishes. See [`docs/publishing.md`](docs/publishing.md) for
the artifact contract and separately authorized nuget.org publishing step.

## License and contributions

This repository uses the
[MIT-Derived License with CanDoItAll Website Link Requirement](LICENSE).
Redistributions of the software or a substantial portion of it in source or
binary form must include at least one link to
[aicandoitall.com](https://aicandoitall.com). One such link satisfies the
added condition for a distribution containing multiple covered CanDoItAll
libraries.

Code contributions are limited to partners approved by the maintainer. See
[`CONTRIBUTING.md`](CONTRIBUTING.md) and contact the `fyziktom` account on
LinkedIn before opening a pull request. Report security issues according to
[`SECURITY.md`](SECURITY.md); repository-specific agent instructions are in
[`AGENTS.md`](AGENTS.md).
