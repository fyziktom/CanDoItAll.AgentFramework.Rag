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
| `R011` | Move collection and record add/edit forms into BaseLib dialogs. | Browser proof opens collection and record dialogs, edits values, saves, and sees updated rows. |
| `R012` | Split the sandbox into BaseLib tabs for collections, records, and similarity search. | Browser proof shows three tabs and each tab exposes only its own management surface. |
| `R013` | Rework records management as a collection list plus right-side record workspace. | Selecting a collection in the left rail changes the right-side record list and actions. |
| `R014` | Replace the large summary stat cards with compact badges. | Top of the page displays compact badge status for collections, records, selected collection, and last action. |
| `R015` | Add generic tag capability support for records and reject tags when a driver does not support them. | Driver tests prove tag rejection for unsupported providers and Qdrant mapping preserves record tags. |
| `R016` | Add collection and record tag editing with BaseLib `TagEditor`. | Browser proof adds/removes collection and record tags through TagEditor controls in dialogs. |
| `R017` | Add generic similarity search across selected collections with a dialog-based collection picker. | Browser proof selects collections by double click and checkbox confirmation, removes selected collections through TagEditor chips, and sees cross-collection search results. |
| `R018` | Repair the bundle with new subbundles for the follow-up work before implementation. | Prepared-stage bundle validator passes with subbundles 05 through 07 present. |
