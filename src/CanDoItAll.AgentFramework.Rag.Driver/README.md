# CanDoItAll Agent Framework RAG Driver

`CanDoItAll.AgentFramework.Rag.Driver` is the provider-neutral entry package
for retrieval-augmented generation. It contains driver contracts, validated
collection and request models, provider selection, and embedding abstractions.
It does not reference a vector-database SDK.

## Install

```powershell
dotnet add package CanDoItAll.AgentFramework.Rag.Driver
```

Most applications should install a provider package, such as
`CanDoItAll.AgentFramework.Rag.Qdrant`, which brings this contract package
transitively and supplies its composition extension.

## Extension model

Implement `IRagDriverProvider` in a provider-specific package and register the
implementation with dependency injection. Provider dependencies belong in the
provider constructor; the provider-neutral factory never exposes a service
container.

Applications must register an `IRagEmbeddingGenerator` appropriate for their
production model provider. The deterministic local hashing implementation is
intended for samples and tests and must be selected explicitly.

```csharp
using CanDoItAll.AgentFramework.Rag.Driver.DependencyInjection;

services.AddLocalHashingRagEmbeddingGenerator(
    options => options.Dimension = 384);
```

See the [source repository](https://github.com/fyziktom/CanDoItAll.AgentFramework.Rag)
for the full architecture, sample, sandbox, and API examples. The CanDoItAll
project website is [aicandoitall.com](https://aicandoitall.com).

## License

This package uses the MIT-Derived License with CanDoItAll Website Link
Requirement embedded in the package as `LICENSE`.
