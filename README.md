# CanDoItAll.AgentFramework.Rag

Standalone CanDoItAll RAG driver repository.

This solution keeps vector database drivers outside the main CanDoItAll solution while exposing generic contracts that the main app can consume later.

## Projects

- `src/CanDoItAll.AgentFramework.Rag.Driver` - provider-neutral RAG contracts, knowledge/search models, factory, and embedding abstractions.
- `src/CanDoItAll.AgentFramework.Rag.Qdrant` - Qdrant implementation using the `Qdrant.Client` NuGet package.
- `src/CanDoItAll.AgentFramework.Rag.Sample` - console sample that configures the factory, stores knowledge, and searches it.
- `tests/CanDoItAll.AgentFramework.Rag.Tests` - xUnit tests for contracts, embeddings, factory behavior, and Qdrant mapping.

## Core Shape

The generic driver surface is centered on:

- `IRagDriver` for ensuring collections, upserting knowledge, deleting knowledge, and searching.
- `IRagDriverFactory` for selecting a configured vector database provider.
- `IRagEmbeddingGenerator` for converting text into vectors.
- `RagKnowledgeEntry`, `RagSearchRequest`, `RagSearchResult`, and `RagCollectionOptions` for typed data flow.

The Driver project does not reference `Qdrant.Client`. Qdrant-specific types stay in `CanDoItAll.AgentFramework.Rag.Qdrant`.

## Embeddings

`LocalHashingRagEmbeddingGenerator` is included for deterministic local samples and tests. Production callers can register their own `IRagEmbeddingGenerator` for SemanticCompletion, OpenAI, Ollama, or existing CanDoItAll provider settings.

Example:

```csharp
services.AddSingleton<IRagEmbeddingGenerator>(
    new DelegatingRagEmbeddingGenerator(async (request, cancellationToken) =>
    {
        var vector = await myEmbeddingProvider.CreateEmbeddingAsync(request.Text, cancellationToken);
        return new RagEmbedding(request.Text, vector, "my-provider");
    }));
```

## Qdrant

The Qdrant project references:

```xml
<PackageReference Include="Qdrant.Client" Version="1.15.0" />
```

Register the Qdrant driver:

```csharp
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

## Sample

Run a dry configuration pass:

```powershell
dotnet run --project src/CanDoItAll.AgentFramework.Rag.Sample -- --dry-run
```

Run against local Qdrant gRPC:

```powershell
# Qdrant.Client uses gRPC. Make sure your container publishes 6334, for example:
# docker run -p 6333:6333 -p 6334:6334 qdrant/qdrant

$env:QDRANT_HOST = "localhost"
$env:QDRANT_GRPC_PORT = "6334"
$env:RAG_COLLECTION = "candoitall-knowledge-sample"
$env:RAG_VECTOR_SIZE = "64"
dotnet run --project src/CanDoItAll.AgentFramework.Rag.Sample
```

The sample uses local deterministic embeddings, ensures the collection, upserts two knowledge entries, and searches by query text.

## Validation

```powershell
dotnet restore CanDoItAll.AgentFramework.Rag.slnx
dotnet build CanDoItAll.AgentFramework.Rag.slnx
dotnet test tests/CanDoItAll.AgentFramework.Rag.Tests/CanDoItAll.AgentFramework.Rag.Tests.csproj
```
