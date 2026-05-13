# Driver Tag Capabilities

## Status

- `Completed`

## Objective

- Add provider-neutral tag capability metadata and record tag support so vector database drivers either preserve tags or reject tagged writes clearly.

## Success Criteria

- `IRagDriver` exposes capabilities including tag support.
- `RagKnowledgeEntry` can carry tags.
- Generic validation rejects record tags when the active driver does not support them.
- Qdrant maps record tags to reserved payload and recreates them in search results.
- Unit tests prove unsupported-provider rejection and Qdrant tag payload round trips.

## Covered Inputs

- `N012`: Support tags in records only when the vector database supports tags.
- `N013`: Record tags must be available for downstream UI editing.
- `R015`: Driver tag capability support and rejection behavior.

## Prerequisites

- `04-sample-console-tests-and-docs` status is `Completed`.
- Current Qdrant mapping tests pass before changing reserved payload behavior.

## Exact Source References

- `C:\repositories\CanDoItAll.AgentFramework.Rag\src\CanDoItAll.AgentFramework.Rag.Driver\Abstractions\IRagDriver.cs`
- `C:\repositories\CanDoItAll.AgentFramework.Rag\src\CanDoItAll.AgentFramework.Rag.Driver\Abstractions\RagDriverBase.cs`
- `C:\repositories\CanDoItAll.AgentFramework.Rag\src\CanDoItAll.AgentFramework.Rag.Driver\Models\RagKnowledgeEntry.cs`
- `C:\repositories\CanDoItAll.AgentFramework.Rag\src\CanDoItAll.AgentFramework.Rag.Qdrant\Mapping\QdrantRagMapper.cs`
- `C:\repositories\CanDoItAll.AgentFramework.Rag\tests\CanDoItAll.AgentFramework.Rag.Tests\Qdrant\QdrantRagMapperTests.cs`

## Deliverables

- Generic `RagDriverCapabilities` model.
- Capability property on `IRagDriver`.
- Tag validation helper in the base driver path.
- Qdrant capability value with `SupportsTags = true`.
- Qdrant reserved tag payload mapping and search result reconstruction.
- Focused model/base-driver and Qdrant mapper tests.

## Dependency Impact

- `06-sandbox-dialog-tabs-and-tags` depends on this phase to know whether record TagEditor input should be enabled and whether tags will be accepted.
- Future non-Qdrant drivers depend on predictable rejection rather than silent tag loss.

## Validation Depth

- `Critical API foundation`

## Implementation Steps

1. Add `RagDriverCapabilities` to the Driver models.
2. Expose `Capabilities` through `IRagDriver` and `RagDriverBase`.
3. Add tags to `RagKnowledgeEntry` and validate null/blank/duplicate behavior.
4. Add base-driver validation that rejects tagged entries when `SupportsTags` is false.
5. Set Qdrant driver capabilities to support tags.
6. Map tags to and from Qdrant payload with a reserved key.
7. Add tests for unsupported-provider rejection and Qdrant tag round trip.

## Scope Exceptions

- Collection tags are sandbox sample metadata in this follow-up; this phase owns record tags in the generic driver surface.

## Do Not Do

- Do not introduce Qdrant types into the Driver project.
- Do not add OpenAI, Ollama, or SemanticCompletion embedding implementations.
- Do not require a live Qdrant container for tests.

## Acceptance Checklist

- `IRagDriver.Capabilities.SupportsTags` exists.
- Tagged entries pass through Qdrant mapping.
- Tagged entries are rejected by an unsupported test driver.
- Existing knowledge-entry metadata behavior remains compatible.

## Proof Required

- `dotnet test tests\CanDoItAll.AgentFramework.Rag.Tests\CanDoItAll.AgentFramework.Rag.Tests.csproj`
- `dotnet build CanDoItAll.AgentFramework.Rag.slnx`

## Browser Validation Logging

- `N/A - no browser-visible UI in this subbundle`

## Progression Gate

- `06-sandbox-dialog-tabs-and-tags` may start only after tag support tests pass and the driver API exposes clear tag capability metadata.

## Suggested Agent Prompt

```text
Implement this subbundle only. Add generic tag capability support, Qdrant tag mapping, and tests. Keep the Driver project provider-neutral and stop if tag support requires Qdrant-specific generic APIs.
```
