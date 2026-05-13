# Sample Console Tests And Docs

## Status

- `Completed`

## Objective

- Add a sample console app, root documentation, and final tests that show the intended RAG driver usage from configuration through knowledge upsert and search.

## Success Criteria

- Sample project compiles and shows factory-based Qdrant usage.
- README explains project layout, Qdrant setup, embedding provider extension points, and sample run commands.
- Full solution build and tests pass.

## Covered Inputs

- `N001`: Standard standalone repo structure must be documented.
- `N007`: Add a sample console application showing how to work with the driver.
- `N006`: Document how external embedding providers can be supplied.

## Prerequisites

- `03-qdrant-driver-implementation` status is `Completed`.

## Exact Source References

- `C:\repositories\CanDoItAll.AgentFramework.Rag\README.md`
- `C:\repositories\CanDoItAll.AgentFramework.SemanticCompletion\README.md`
- `C:\repositories\qdrant-dotnet\README.md`

## Deliverables

- Console sample `Program.cs`.
- README usage and extension-point documentation.
- Final unit test pass and build proof.
- Execution report and bundle closure updates.

## Dependency Impact

- This is the final closure phase; weak proof here leaves the user without confidence that the driver is usable.

## Validation Depth

- `End-to-end regression and closure`

## Implementation Steps

1. Implement sample console flow using DI, factory, local deterministic embeddings, and Qdrant options.
2. Document local Qdrant configuration and sample commands.
3. Run full tests and solution build.
4. Update execution report, raw note closure, and bundle final status.
5. Run completed-stage bundle validation.

## Scope Exceptions

- The sample may default to localhost Qdrant and should not run automatically during tests.

## Do Not Do

- Do not hard-code secrets or OpenAI/Ollama credentials.
- Do not require Docker startup in tests.
- Do not add a browser UI.

## Acceptance Checklist

- Sample compiles.
- README documents factory and embedding provider extension points.
- Full tests pass.
- Bundle execution report closes all raw notes.

## Proof Required

- `dotnet build CanDoItAll.AgentFramework.Rag.slnx`
- `dotnet test tests/CanDoItAll.AgentFramework.Rag.Tests/CanDoItAll.AgentFramework.Rag.Tests.csproj`
- `python codex/bundles/rag-driver/scripts/validate_bundle.py codex/bundles/rag-driver --profile initiative --stage completed`

## Browser Validation Logging

- `N/A - no browser-visible or host-visible UI`

## Progression Gate

- Bundle can close only when code, tests, documentation, execution report, and completed-stage validator agree.

## Suggested Agent Prompt

```text
Implement this subbundle only. Add the console sample and documentation, run final build/tests, update bundle closure proof, and stop if any raw note cannot be marked solved or explicitly scoped.
```
