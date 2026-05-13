# Current State

## Repository State

- `C:\repositories\CanDoItAll.AgentFramework.Rag` contains only `.gitignore`, `README.md`, and copied Qdrant skill files under `codex/skills`.
- No solution, `src`, `tests`, project files, or package references exist yet.
- The local Qdrant Docker container ID is provided by the user, but the bundle does not require live integration proof unless the container is reachable during execution.

## Reference Repo Shape

- `C:\repositories\CanDoItAll.AgentFramework.SemanticCompletion` uses a root `.slnx`, `global.json`, `Directory.Build.props`, `src`, `tests`, `*.Driver`, `*.Sandbox`, and `*.Tests` naming.
- The reference solution targets `net10.0` and SDK `10.0.200`.
- The reference test project uses xUnit, `Microsoft.NET.Test.Sdk`, `coverlet.collector`, and direct project references.
- SemanticCompletion already has an embedding abstraction: `IAgentTextEmbeddingGenerator` plus local hashing and ONNX implementations. The RAG repository should not copy those implementations by default; it should make adapter injection straightforward.

## Qdrant Reference State

- `C:\repositories\qdrant-dotnet` exposes `Qdrant.Client` and `Qdrant.Client.Grpc` APIs.
- Reference usage includes `QdrantClient`, `CreateCollectionAsync`, `UpsertAsync`, `SearchAsync`, `QueryAsync`, `PointStruct`, `VectorParams`, `Distance`, and payload values.
- The installed `qdrant-clients-sdk` skill identifies `.NET` support through the `Qdrant.Client` NuGet package.

## Initial Design Direction

- Use a core driver project for provider-neutral contracts and factory abstractions.
- Use a Qdrant project for the first concrete vector database driver.
- Use a sample console project rather than a browser sandbox because the requested sample is operational API usage.
- Use tests to validate the provider-neutral behavior without requiring a live vector database.
