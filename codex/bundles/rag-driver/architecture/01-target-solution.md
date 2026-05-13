# Target Solution

## Solution Layout

- `CanDoItAll.AgentFramework.Rag.slnx`
- `src/CanDoItAll.AgentFramework.Rag.Driver/CanDoItAll.AgentFramework.Rag.Driver.csproj`
- `src/CanDoItAll.AgentFramework.Rag.Qdrant/CanDoItAll.AgentFramework.Rag.Qdrant.csproj`
- `src/CanDoItAll.AgentFramework.Rag.Sample/CanDoItAll.AgentFramework.Rag.Sample.csproj`
- `src/CanDoItAll.AgentFramework.Rag.Sandbox/CanDoItAll.AgentFramework.Rag.Sandbox.csproj`
- `tests/CanDoItAll.AgentFramework.Rag.Tests/CanDoItAll.AgentFramework.Rag.Tests.csproj`
- `Directory.Build.props`
- `global.json`

## Driver Project Responsibilities

- Public generic RAG contracts: `IRagDriver`, `IRagDriverFactory`, `IRagEmbeddingGenerator`.
- Public models for knowledge entry, metadata, search request/result, collection/vector settings, options, provider kind, and validation errors.
- Public driver capability metadata, including whether the concrete vector database supports record tags.
- Deterministic local embedding provider for test/sample use.
- Dependency injection registration for generic services and factory.

## Qdrant Project Responsibilities

- Qdrant-specific options and validation.
- Qdrant implementation of `IRagDriver`.
- Translation between generic models and `Qdrant.Client.Grpc` point/vector/payload types.
- DI registration for Qdrant without leaking Qdrant types into core driver contracts.
- Tag payload mapping through Qdrant payload arrays while keeping reserved payload keys out of user metadata.

## Sample Responsibilities

- Configure local deterministic embeddings and the Qdrant driver through the factory.
- Ensure a collection, upsert a few knowledge entries, and search by text.
- Keep the sample console-only and suitable for local Docker Qdrant usage.

## Sandbox Responsibilities

- Use CanDoItAll BaseLib components for the Blazor SSR sample UI.
- Provide compact badge status for collection count, record count, selected collection, and last action.
- Split collection management, record management, and similarity search through BaseLib `Tabs`.
- Use BaseLib `Dialog` for collection and record add/edit forms.
- Use BaseLib `TagEditor` for collection tags, record tags, and removable selected collections in similarity search.
- Present record management as a left collection rail and a right record workspace.
- Keep data session-scoped and deterministic through the local embedding generator so the sandbox runs without Qdrant or external embedding services.

## Test Responsibilities

- Validate vector dimensions and deterministic embeddings.
- Validate factory selection and unsupported provider behavior.
- Validate Qdrant mapping logic through testable helpers without requiring live Qdrant.
- Validate driver tag capability behavior and Qdrant tag payload round trips.
- Validate sample-facing configuration can be built from DI.

## Non-Goals

- No main CanDoItAll solution integration in this bundle.
- No mandatory OpenAI, Ollama, or SemanticCompletion project reference in the default path.
- No persistent storage in the Blazor sandbox.
- No live Qdrant requirement for browser proof.
