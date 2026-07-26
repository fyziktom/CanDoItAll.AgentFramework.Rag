# Architecture review

Review date: 2026-07-26
Gate: passed

This review applies the repository's C# architecture-governance gate to the
publishing candidate. The final full-solution CodeAnalytics snapshot is
`snap-20260726224758-3abbda11`.

## Gate result

| Concern | Result | Evidence |
|---|---|---|
| Dependency direction | Pass | Driver has no project dependency and no Qdrant SDK type; Qdrant depends only on Driver and `Qdrant.Client`; the snapshot reports no dependency cycle |
| Construction | Pass | `RagDriverFactory` and `IRagDriverProvider` do not accept or retain `IServiceProvider`; providers use constructor injection |
| Provider extensibility | Pass | The factory selects registered providers by identifier and has no Qdrant-specific branch or default |
| Qdrant isolation | Pass | Six internal mapper owners replace the former broad mapper facade and each has direct tests |
| Testability | Pass | Factory tests use fake providers without a container; sandbox embeddings, similarity calculation, and time are injected |
| Sandbox responsibilities | Pass | Models, projections, seed data, state, and similarity calculation are separate from CRUD orchestration |
| UI composition | Pass | Three routed pages use the NuGet BaseLib `SideMenu`, layout, data, dialog, list-detail, badge, and selection primitives |
| Package boundary | Pass | Only Driver and Qdrant are packable; package READMEs live beside their project files |

## Analyzer interpretation

The full snapshot contains 13 informational `COMPLEXITY-002` observations,
zero warning/error findings, zero open questions, and zero dependency cycles.
The observations identify member-rich contracts, driver adapters, Razor page
event surfaces, and the sandbox store. They do not identify a layering or
dependency violation.

The sandbox store remains the session-level CRUD coordinator, but vector math,
seed creation, state, projections, and edit models have separate owners. Razor
pages keep their framework-bound event handlers in `.razor.cs` files rather
than adding presentation service layers with no independent lifecycle.

The analyzer also emits two non-blocking name-disambiguation diagnostics
because the console sample and ASP.NET Core sandbox both use C# top-level
statements and therefore generate an implicit type named `Program`. These are
analysis-tool display-name warnings, not build diagnostics or a project
dependency ambiguity.

## Deliberate constraints

- Provider packages must not resolve dependencies from a service container.
- Local deterministic embeddings remain explicit opt-in sample/test behavior.
- Provider SDK mapping types remain internal.
- Razor markup/code-behind pairs are the only accepted partial-class boundary.
- New sandbox visual styling should first be expressed through BaseLib
  components and theme tokens.
- A new project boundary requires an independently versioned lifecycle or an
  SDK/isolation concern; file count alone is not sufficient.

## Release decision

No architectural blocker remains for packaging. Future work can reconsider
member-rich public contracts only when a concrete consumer needs a smaller
capability interface; splitting them before that would add coordination
surface without changing the current provider lifecycle.
