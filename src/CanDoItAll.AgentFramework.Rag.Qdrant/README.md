# CanDoItAll Agent Framework RAG for Qdrant

`CanDoItAll.AgentFramework.Rag.Qdrant` implements the provider-neutral
CanDoItAll RAG contracts with the official `Qdrant.Client` SDK. Qdrant SDK
types remain inside this package.

## Install

```powershell
dotnet add package CanDoItAll.AgentFramework.Rag.Qdrant
```

## Register

```csharp
using CanDoItAll.AgentFramework.Rag.Driver.DependencyInjection;
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

Register an `IRagEmbeddingGenerator` whose output dimension matches the
collection. The repository sample shows how to opt into deterministic local
hashing for a self-contained demonstration; production applications should
provide their model-backed embedding implementation.

Resolve `IRagDriver` for the configured default or `IRagDriverFactory` when
the caller needs explicit factory options.

See the [source repository](https://github.com/fyziktom/CanDoItAll.AgentFramework.Rag)
for filtering, payload-index, tag, sample, and sandbox examples. The CanDoItAll
project website is [aicandoitall.com](https://aicandoitall.com).

## License

This package uses the repository's MIT License.
