# Target Solution

## Solution Layout

- `CanDoItAll.AgentFramework.Rag.slnx`
- `src/CanDoItAll.AgentFramework.Rag.Driver/CanDoItAll.AgentFramework.Rag.Driver.csproj`
- `src/CanDoItAll.AgentFramework.Rag.Qdrant/CanDoItAll.AgentFramework.Rag.Qdrant.csproj`
- `src/CanDoItAll.AgentFramework.Rag.Sample/CanDoItAll.AgentFramework.Rag.Sample.csproj`
- `tests/CanDoItAll.AgentFramework.Rag.Tests/CanDoItAll.AgentFramework.Rag.Tests.csproj`
- `Directory.Build.props`
- `global.json`

## Driver Project Responsibilities

- Public generic RAG contracts: `IRagDriver`, `IRagDriverFactory`, `IRagEmbeddingGenerator`.
- Public models for knowledge entry, metadata, search request/result, collection/vector settings, options, provider kind, and validation errors.
- Deterministic local embedding provider for test/sample use.
- Dependency injection registration for generic services and factory.

## Qdrant Project Responsibilities

- Qdrant-specific options and validation.
- Qdrant implementation of `IRagDriver`.
- Translation between generic models and `Qdrant.Client.Grpc` point/vector/payload types.
- DI registration for Qdrant without leaking Qdrant types into core driver contracts.

## Sample Responsibilities

- Configure local deterministic embeddings and the Qdrant driver through the factory.
- Ensure a collection, upsert a few knowledge entries, and search by text.
- Keep the sample console-only and suitable for local Docker Qdrant usage.

## Test Responsibilities

- Validate vector dimensions and deterministic embeddings.
- Validate factory selection and unsupported provider behavior.
- Validate Qdrant mapping logic through testable helpers without requiring live Qdrant.
- Validate sample-facing configuration can be built from DI.

## Non-Goals

- No main CanDoItAll solution integration in this bundle.
- No mandatory OpenAI, Ollama, or SemanticCompletion project reference in the default path.
- No browser UI or Blazor sandbox.
