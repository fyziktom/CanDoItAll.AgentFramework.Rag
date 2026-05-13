# CanDoItAll RAG Driver

This bundle coordinates the standalone CanDoItAll RAG driver repository.

## Profile

- `initiative`

## Mission

Build a standalone `.slnx` with generic retrieval-augmented-generation driver contracts, pluggable embedding providers, a Qdrant vector database implementation, a factory for selecting configured drivers, tests, a sample console app, and a BaseLib Blazor SSR sandbox for collection, record, tag, and similarity-search workflows.

## Outcome Contract

- Requested outcome: a reusable CanDoItAll RAG driver library with Qdrant as the first vector database implementation plus an operator-facing Blazor SSR sandbox.
- Hard constraints: keep this outside the main CanDoItAll solution, mirror the SemanticCompletion repo structure, use `Qdrant.Client` as a NuGet package, and keep embedding generation provider-driven so the main application can inject its own providers later.
- Evidence required before closure: standalone solution build, unit tests, Qdrant driver tests that do not require a live service by default, sample console compile, documentation showing configured usage, and browser proof for the Blazor sandbox workflows.
- Known blockers or explicit scope exceptions: live Qdrant integration proof is optional because it depends on the local Docker container state; the required proof is a compile-safe Qdrant adapter plus unit-level behavior around contracts, factory, embedding flow, and vector validation.

## Bundle Layout

- `inputs/` raw request, artifacts, and structured input
- `analysis/` current state, assumptions, and risks
- `requirements/` normalized, testable requirements
- `architecture/` target solution and important boundaries
- `plan/` execution order and dependencies
- `traceability/` requirement-to-bundle mapping
- `shared-prompts/` reusable implementation and QA prompts
- `subbundles/` numbered execution-ready workstreams
- `reviews/` bundle self-review and execution report
- `scripts/validate_bundle.py` bundle validation helper copied from the CanDoItAll bundle preparation skill

## Recommended Execution Order

1. `subbundles/01-standalone-solution-and-project-layout`
2. `subbundles/02-generic-rag-and-embedding-contracts`
3. `subbundles/03-qdrant-driver-implementation`
4. `subbundles/04-sample-console-tests-and-docs`
5. `subbundles/05-driver-tag-capabilities`
6. `subbundles/06-sandbox-dialog-tabs-and-tags`
7. `subbundles/07-sandbox-generic-similarity-search`

## Dependency And Validation Map

- Keep the mermaid dependency map, critical-subbundle notes, and phase gates current in `plan/01-phase-plan.md`.
- If the bundle is resumed after compaction or by a different agent, use this README, the current subbundle README, and `reviews/01-execution-report.md` as the durable state.

## Validation Summary

- Bundle preparation status: `Prepared`
- Bundle readiness gate: `Passed - structure and dependency plan validated before implementation`
- Execution status: `Completed`
- Subbundle gate review: `Completed - all subbundle gates passed including follow-up subbundles 05 through 07`
- Final closure gate: `Passed - completed-stage validator passed after follow-up implementation`
- Browser validation analytics: `Passed - Blazor sandbox workflows verified at desktop and narrow viewports`
