# Phase Plan

## Phase Sequence

1. Create the standalone solution, root config, project files, and base references.
2. Add provider-neutral contracts, typed models, local embeddings, and the factory foundation.
3. Add the Qdrant driver implementation using the `Qdrant.Client` NuGet package.
4. Add sample console usage, tests, README documentation, and final closure proof.
5. Add tag capability contracts and Qdrant tag payload support.
6. Refactor the Blazor sandbox into dialog-driven tabbed collection and record management with TagEditor support.
7. Add generic multi-collection similarity search and browser proof.

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
section Follow-up
05 Driver tag capabilities :done, after s04, s05, 1d
06 Sandbox dialogs tabs and tags :done, after s05, s06, 1d
07 Sandbox generic similarity search :done, after s06, s07, 1d
```

- `02` depends on `01` project paths and target frameworks.
- `03` depends on `02` generic contracts staying provider-neutral.
- `04` depends on `02` and `03` because it proves the factory, sample, and tests together.
- `05` depends on `02` and `03` because tag support must stay generic while Qdrant maps tags as payload.
- `06` depends on `05` because the sandbox must reflect whether tags are supported.
- `07` depends on `06` because the search tab reuses the tabbed shell, TagEditor chip behavior, and selected collection state.

## Critical Subbundles

- `01-standalone-solution-and-project-layout`: critical foundation for all restore, build, and test proof.
- `02-generic-rag-and-embedding-contracts`: critical architecture foundation because all vector database implementations depend on these contracts.
- `03-qdrant-driver-implementation`: critical implementation foundation because it proves Qdrant can be one implementation behind the generic driver.
- `05-driver-tag-capabilities`: critical API foundation because unsupported vector DBs must reject record tags predictably.
- `06-sandbox-dialog-tabs-and-tags`: critical UI foundation because the record and search flows depend on the selected-collection layout and dialog state.

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
- Gate before `05`: confirm Qdrant mapping tests pass and generic driver contracts are not Qdrant-specific.
- Gate after `05`: tag capability tests pass and Qdrant payload mapping preserves tags.
- Gate before `06`: confirm BaseLib Dialog, Tabs, TagEditor, Badge, DataGrid, and CheckBox APIs from local component source.
- Gate after `06`: browser proof covers dialogs, tabs, badges, record left rail, and collection/record TagEditor editing.
- Gate before `07`: confirm tabbed sandbox state is stable and collection summaries include tags for search selection.
- Gate after `07`: browser proof covers collection-picker double-click, checkbox multi-select, removable selected collections, and multi-collection similarity results.
