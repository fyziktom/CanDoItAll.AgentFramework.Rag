# Normalized Requirements

| ID | Requirement | Acceptance signal |
| --- | --- | --- |
| `R001` | Create a standalone CanDoItAll-style `.slnx` with `src`, `tests`, root config, and project naming aligned with the SemanticCompletion repo. | Solution file lists driver, Qdrant, sample, and test projects; build restores and compiles. |
| `R002` | Define provider-neutral RAG driver contracts for storing and searching knowledge entries. | Contracts expose create/ensure collection, upsert knowledge, delete, and search operations without Qdrant-specific types. |
| `R003` | Model knowledge entries, search requests, search results, metadata, vector settings, and distance metrics as typed public APIs. | Tests can construct records and validate vector dimensionality and metadata round-trip behavior. |
| `R004` | Implement Qdrant as the first vector database driver using `Qdrant.Client` from NuGet, not a source reference to `C:\repositories\qdrant-dotnet`. | Qdrant project has a NuGet package reference and compiles against public Qdrant APIs. |
| `R005` | Add a factory that takes options and returns the configured RAG driver implementation. | DI/factory tests resolve the Qdrant driver for Qdrant options and reject unsupported providers clearly. |
| `R006` | Include embedding conversion in the storage/search pipeline. | Driver upsert/search paths request embeddings when only text is provided and validate returned vector dimensions. |
| `R007` | Define a pluggable embedding provider interface that can be implemented by SemanticCompletion, OpenAI, Ollama, or main CanDoItAll providers. | Public embedding contracts are independent from vector DB contracts and accept provider-specific implementations through DI. |
| `R008` | Provide at least one local deterministic embedding implementation for samples/tests without external services. | Tests and sample can run without OpenAI, Ollama, model files, or network calls. |
| `R009` | Add a sample console application that demonstrates configuring the factory, creating/ensuring storage, adding knowledge, and searching it. | Sample project compiles and README documents how to run it against local Qdrant. |
| `R010` | Document setup, provider boundaries, and Qdrant configuration. | Root README describes project layout, packages, sample usage, and extension points. |
