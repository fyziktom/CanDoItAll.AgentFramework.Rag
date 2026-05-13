# Qdrant Driver Implementation

## Status

- `Completed`

## Objective

- Implement Qdrant as the first concrete vector database provider behind the generic RAG driver contracts using the `Qdrant.Client` NuGet package.

## Success Criteria

- Qdrant project uses a package reference to `Qdrant.Client`.
- Qdrant driver translates generic knowledge entries, metadata, vector settings, and searches into Qdrant client calls.
- Mapping behavior is testable without a live Qdrant service.

## Covered Inputs

- `N003`: Start with Qdrant and use its .NET driver as a NuGet package.
- `N005`: Store knowledges in a vector DB.

## Prerequisites

- `02-generic-rag-and-embedding-contracts` status is `Completed`.

## Exact Source References

- `C:\repositories\qdrant-dotnet\README.md`
- `C:\repositories\qdrant-dotnet\src\Qdrant.Client\IQdrantClient.cs`
- `C:\repositories\CanDoItAll.AgentFramework.Rag\codex\skills\qdrant-clients-sdk\SKILL.md`

## Deliverables

- `QdrantRagDriver` implementation.
- Qdrant options and DI registration.
- Internal mapping helpers for distance, payload, point IDs, and vector conversion.
- Tests covering mapping and factory registration without requiring live Qdrant.

## Dependency Impact

- The sample app and future production integration depend on Qdrant being selectable through the generic factory.
- If Qdrant translation is wrong, storing and searching knowledge entries will fail even if generic contracts compile.

## Validation Depth

- `Critical implementation foundation`

## Implementation Steps

1. Add `Qdrant.Client` package reference to the Qdrant project.
2. Implement options and DI registration for Qdrant.
3. Implement collection ensure/create, upsert, delete, and search operations.
4. Add testable mapping helpers for Qdrant types.
5. Add tests proving options and mapping behavior without live service dependency.

## Scope Exceptions

- Live Docker integration proof is optional and should be recorded separately if the local container is reachable.

## Do Not Do

- Do not reference the cloned Qdrant source project.
- Do not expose Qdrant generated types through generic Driver project APIs.
- Do not require live Qdrant in normal unit tests.

## Acceptance Checklist

- Qdrant project restores `Qdrant.Client` from NuGet.
- Driver compiles against Qdrant client public APIs.
- Generic factory can select Qdrant provider.
- Tests validate Qdrant payload/vector mapping.

## Proof Required

- `dotnet test tests/CanDoItAll.AgentFramework.Rag.Tests/CanDoItAll.AgentFramework.Rag.Tests.csproj --no-restore`
- `dotnet build CanDoItAll.AgentFramework.Rag.slnx --no-restore`

## Browser Validation Logging

- `N/A - no browser-visible or host-visible UI`

## Progression Gate

- Sample/docs phase may start only after Qdrant compiles through NuGet and no generic API exposes Qdrant types.

## Suggested Agent Prompt

```text
Implement this subbundle only. Use Qdrant.Client as a NuGet package, keep Qdrant-specific logic in the Qdrant project, and test mapping/factory behavior without making live Qdrant mandatory.
```
