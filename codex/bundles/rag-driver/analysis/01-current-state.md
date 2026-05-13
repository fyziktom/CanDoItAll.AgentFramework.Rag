# Current State

## Repository State

- `C:\repositories\CanDoItAll.AgentFramework.Rag` now contains a standalone `.slnx`, driver, Qdrant, sample console, Blazor sandbox, and test projects.
- The current Blazor sandbox has inline collection and record forms on a single page; the follow-up request requires dialogs, tabs, tag editing, and multi-collection similarity search.
- The current generic driver models do not expose tag capability metadata or tag fields.
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
- Keep the sample console project for operational API usage and use the Blazor sandbox as a UI demonstration surface.
- Use tests to validate the provider-neutral behavior without requiring a live vector database.
- Use browser validation for the sandbox because the follow-up request is UI-heavy and BaseLib component behavior must be checked in the browser.
