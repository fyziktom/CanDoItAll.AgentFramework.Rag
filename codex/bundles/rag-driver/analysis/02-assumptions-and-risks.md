# Assumptions And Risks

## Assumptions

- The standalone repo should mirror SemanticCompletion naming with `CanDoItAll.AgentFramework.Rag.Driver`, `CanDoItAll.AgentFramework.Rag.Qdrant`, `CanDoItAll.AgentFramework.Rag.Sample`, and `CanDoItAll.AgentFramework.Rag.Tests`.
- The first shipped embedding implementation can be deterministic and local for sample/tests while cloud and Ollama providers remain option-driven extension points.
- The RAG driver should manage knowledge entries as typed records with text, metadata, embedding, and search result score.
- The factory can use Microsoft dependency injection plus strongly typed options so the main CanDoItAll app can feed its existing provider settings later.
- Tags are a vector-database capability, not an unconditional generic guarantee; drivers that cannot represent tags should reject tagged records before write attempts.
- Collection tags in the sandbox are sample metadata used to organize and select collections; record tags are part of the driver knowledge-entry model when the selected provider supports tags.
- The sandbox should continue to use an in-memory store for UI proof and should not introduce persistence during this follow-up.

## Critical Path Risks

- `02-generic-rag-and-embedding-contracts` is the critical foundation; if its contracts are too Qdrant-specific, every later implementation becomes coupled to Qdrant.
- `03-qdrant-driver-implementation` depends on the Qdrant NuGet API surface; wrong assumptions about generated gRPC types would break builds and sample usage.
- `04-sample-console-tests-and-docs` depends on the earlier factory and driver contracts; if those are unstable, sample code becomes misleading.
- `05-driver-tag-capabilities` reopens the generic model surface; weak capability validation would make unsupported provider behavior ambiguous.
- `06-sandbox-dialog-tabs-and-tags` depends on BaseLib component APIs; wrong assumptions about Dialog, Tabs, or TagEditor bindings would fail browser proof.
- `07-sandbox-generic-similarity-search` depends on the tabbed state model and cross-collection search service behavior.

## Validation Risks

- Live Qdrant integration may be blocked by local Docker state, port exposure, or container health. Unit proof must not depend on the container.
- OpenAI and Ollama embeddings should not be exercised by default because credentials and local services may not be configured.
- SemanticCompletion adapter proof may be limited to contract compatibility unless the repo is referenced explicitly in a later integration bundle.
- Browser proof must include open dialogs and collection-picker overlay states because clipping, layering, and TagEditor chip behavior are the main UI risks.

## Reopen Triggers

- Reopen `02-generic-rag-and-embedding-contracts` if Qdrant implementation requires provider-specific concepts in generic contracts.
- Reopen `03-qdrant-driver-implementation` if `Qdrant.Client` package restore or compile shows API mismatches with the local reference repo.
- Reopen `04-sample-console-tests-and-docs` if tests pass but the sample cannot demonstrate create, upsert, and search flow clearly.
- Reopen the bundle if live Qdrant proof is required later and cannot be represented by the existing driver surface.
- Reopen `05-driver-tag-capabilities` if UI tag editing can create tags that a configured driver would silently drop.
- Reopen `06-sandbox-dialog-tabs-and-tags` if browser proof shows dialog, tab, or left-rail behavior does not match the follow-up request.
