# Phase Plan

## Phase Sequence

1. Create the standalone solution, root config, project files, and base references.
2. Add provider-neutral contracts, typed models, local embeddings, and the factory foundation.
3. Add the Qdrant driver implementation using the `Qdrant.Client` NuGet package.
4. Add sample console usage, tests, README documentation, and final closure proof.

## Subbundle Dependency Map

```mermaid
gantt
title RAG Driver Bundle Dependency Map
dateFormat  YYYY-MM-DD
section Foundations
01 Standalone solution and project layout :done, s01, 2026-05-13, 1d
02 Generic RAG and embedding contracts :after s01, s02, 1d
section Implementations
03 Qdrant driver implementation :after s02, s03, 1d
section Closure
04 Sample console tests and docs :after s03, s04, 1d
```

- `02` depends on `01` project paths and target frameworks.
- `03` depends on `02` generic contracts staying provider-neutral.
- `04` depends on `02` and `03` because it proves the factory, sample, and tests together.

## Critical Subbundles

- `01-standalone-solution-and-project-layout`: critical foundation for all restore, build, and test proof.
- `02-generic-rag-and-embedding-contracts`: critical architecture foundation because all vector database implementations depend on these contracts.
- `03-qdrant-driver-implementation`: critical implementation foundation because it proves Qdrant can be one implementation behind the generic driver.

## Phase Gates

- Gate after preparation: run `python codex/bundles/rag-driver/scripts/validate_bundle.py codex/bundles/rag-driver --profile initiative --stage prepared`.
- Gate before `01`: confirm current repo still lacks solution/projects and source references exist.
- Gate after `01`: solution restore/build reaches project discovery or compile stage without missing project paths.
- Gate before `02`: confirm `01` completed and public API files are in the Driver project only.
- Gate after `02`: unit tests for embeddings, model validation, and factory contracts pass.
- Gate before `03`: confirm Qdrant package reference restores and no generic contract requires Qdrant types.
- Gate after `03`: Qdrant project builds and mapping behavior is tested without live service dependency.
- Gate before `04`: confirm sample project references the generic and Qdrant projects through normal project references.
- Gate after `04`: full solution build and tests pass; README and execution report contain closure proof.
