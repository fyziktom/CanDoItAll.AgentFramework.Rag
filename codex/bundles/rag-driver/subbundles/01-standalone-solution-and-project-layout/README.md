# Standalone Solution And Project Layout

## Status

- `Completed`

## Objective

- Create the standalone CanDoItAll-style solution, root config, source projects, test project, and project references that all later RAG work depends on.

## Success Criteria

- The root solution and project files exist in the planned paths.
- The structure mirrors the SemanticCompletion standalone repo shape.
- Initial restore/build can discover all projects.

## Covered Inputs

- `N001`: Create solution with projects and standard CanDoItAll repo structure.

## Prerequisites

- `none`

## Exact Source References

- `C:\repositories\CanDoItAll.AgentFramework.Rag`
- `C:\repositories\CanDoItAll.AgentFramework.SemanticCompletion\CanDoItAll.AgentFramework.SemanticCompletion.slnx`
- `C:\repositories\CanDoItAll.AgentFramework.SemanticCompletion\Directory.Build.props`
- `C:\repositories\CanDoItAll.AgentFramework.SemanticCompletion\global.json`

## Deliverables

- Root `CanDoItAll.AgentFramework.Rag.slnx`.
- Root `Directory.Build.props` and `global.json`.
- Driver, Qdrant, sample console, and test project directories and `.csproj` files.
- Correct project references among Driver, Qdrant, Sample, and Tests.

## Dependency Impact

- All later code, package restore, build, sample, and test proof depends on these paths and target frameworks being correct.

## Validation Depth

- `Critical foundation`

## Implementation Steps

1. Create root config matching the SemanticCompletion repo.
2. Create the Driver, Qdrant, Sample, and Tests projects under `src` and `tests`.
3. Add project references and package references needed by the scaffold.
4. Create the `.slnx` with `/src/` and `/tests/` folders.
5. Run restore/build far enough to prove project discovery.

## Scope Exceptions

- No feature behavior is required in this subbundle beyond compile-safe skeletons.

## Do Not Do

- Do not add the projects to the main CanDoItAll solution.
- Do not add Qdrant adapter behavior before generic contracts exist.
- Do not require a live Qdrant service for this phase.

## Acceptance Checklist

- Root config exists.
- All project files exist.
- `.slnx` references all four projects.
- Project references point to existing local projects.

## Proof Required

- `dotnet restore CanDoItAll.AgentFramework.Rag.slnx`
- `dotnet build CanDoItAll.AgentFramework.Rag.slnx --no-restore`

## Browser Validation Logging

- `N/A - no browser-visible or host-visible UI`

## Progression Gate

- Downstream subbundles may start only after project paths and references are stable enough for restore/build.

## Suggested Agent Prompt

```text
Implement this subbundle only. Create the standalone solution and project scaffold using the SemanticCompletion repo as the naming and layout precedent. Capture restore/build proof and update the execution report before moving to generic contracts.
```
