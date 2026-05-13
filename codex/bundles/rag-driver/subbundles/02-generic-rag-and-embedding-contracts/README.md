# Generic RAG And Embedding Contracts

## Status

- `Completed`

## Objective

- Add provider-neutral RAG contracts, typed knowledge/search models, embedding abstractions, a deterministic local embedding provider, options, and the generic driver factory foundation.

## Success Criteria

- Public driver contracts do not expose Qdrant types.
- Embedding generation is injectable and reusable by future SemanticCompletion, OpenAI, Ollama, or main CanDoItAll providers.
- Tests prove model validation, deterministic embeddings, and factory behavior.

## Covered Inputs

- `N002`: Build generic RAG driver interfaces for multiple vector databases.
- `N004`: Add a factory where options select the proper driver instance.
- `N005`: Store knowledges and perform embedding conversion.
- `N006`: Allow embedding implementations to come from SemanticCompletion, OpenAI, Ollama, or main CanDoItAll providers.

## Prerequisites

- `01-standalone-solution-and-project-layout` status is `Completed`.

## Exact Source References

- `C:\repositories\CanDoItAll.AgentFramework.Rag`
- `C:\repositories\CanDoItAll.AgentFramework.SemanticCompletion\src\CanDoItAll.AgentFramework.SemanticCompletion.Driver\Embeddings\IAgentTextEmbeddingGenerator.cs`
- `C:\repositories\CanDoItAll.AgentFramework.SemanticCompletion\src\CanDoItAll.AgentFramework.SemanticCompletion.Driver\Embeddings\LocalHashingAgentTextEmbeddingGenerator.cs`

## Deliverables

- `IRagDriver`, `IRagDriverFactory`, and `IRagEmbeddingGenerator`.
- Typed models for knowledge entries, search requests/results, metadata, vectors, distance metrics, and provider options.
- Deterministic local embedding implementation for samples/tests.
- DI registrations for generic services and factory.
- Unit tests covering provider-neutral behavior.

## Dependency Impact

- Qdrant and every future vector database implementation depends on these contracts remaining generic.
- Sample usage and main CanDoItAll integration depend on the factory and embedding provider boundaries.

## Validation Depth

- `Critical foundation`

## Implementation Steps

1. Add public contracts and models in the Driver project.
2. Add option types for vector provider selection and embedding dimensions.
3. Add local deterministic embedding implementation with dimension validation.
4. Add factory interfaces and DI registration helpers.
5. Add focused unit tests for model validation, embeddings, and factory behavior.

## Scope Exceptions

- OpenAI, Ollama, and SemanticCompletion concrete providers are extension points only in this bundle.

## Do Not Do

- Do not reference `Qdrant.Client` from the Driver project.
- Do not make cloud API calls.
- Do not copy SemanticCompletion implementation internals unless needed for a deterministic local provider.

## Acceptance Checklist

- Generic contracts compile without Qdrant package references.
- Local embedding provider returns stable vectors with the configured size.
- Factory can resolve registered providers and fail clearly for unsupported providers.
- Tests cover dimensionality mismatch behavior.

## Proof Required

- `dotnet test tests/CanDoItAll.AgentFramework.Rag.Tests/CanDoItAll.AgentFramework.Rag.Tests.csproj --no-restore`
- `dotnet build CanDoItAll.AgentFramework.Rag.slnx --no-restore`

## Browser Validation Logging

- `N/A - no browser-visible or host-visible UI`

## Progression Gate

- Qdrant implementation may start only if generic contracts are provider-neutral and covered by tests.

## Suggested Agent Prompt

```text
Implement this subbundle only. Add the generic RAG and embedding APIs, local deterministic embeddings, options, and factory foundation without leaking Qdrant types into the Driver project. Capture tests and build proof before moving to Qdrant.
```
