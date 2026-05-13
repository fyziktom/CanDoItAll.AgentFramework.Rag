# Requirement Traceability

| Requirement | Raw notes | Owning subbundle | Bundle files | Planned proof |
| --- | --- | --- | --- | --- |
| `R001` | `N001` | `01-standalone-solution-and-project-layout` | `architecture/01-target-solution.md`, `plan/01-phase-plan.md` | Solution/project files exist; build command recorded. |
| `R002` | `N002` | `02-generic-rag-and-embedding-contracts` | `requirements/01-normalized-requirements.md`, `architecture/01-target-solution.md` | Contract tests compile against provider-neutral APIs. |
| `R003` | `N002` | `02-generic-rag-and-embedding-contracts` | `requirements/01-normalized-requirements.md` | Model validation tests. |
| `R004` | `N003` | `03-qdrant-driver-implementation` | `analysis/01-current-state.md`, `architecture/01-target-solution.md` | Qdrant project uses `Qdrant.Client` package and builds. |
| `R005` | `N004` | `02-generic-rag-and-embedding-contracts` | `architecture/01-target-solution.md` | Factory selection tests. |
| `R006` | `N005` | `02-generic-rag-and-embedding-contracts`, `03-qdrant-driver-implementation` | `requirements/01-normalized-requirements.md` | Upsert/search paths call embedding generator in tests or sample. |
| `R007` | `N006` | `02-generic-rag-and-embedding-contracts` | `architecture/01-target-solution.md` | Public `IRagEmbeddingGenerator` has no vendor dependency. |
| `R008` | `N006` | `02-generic-rag-and-embedding-contracts` | `requirements/01-normalized-requirements.md` | Local deterministic embedding tests pass. |
| `R009` | `N007` | `04-sample-console-tests-and-docs` | `architecture/01-target-solution.md` | Sample project builds; README shows run command. |
| `R010` | `N001` through `N007` | `04-sample-console-tests-and-docs` | `README.md`, `reviews/01-execution-report.md` | Documentation reviewed and final validator passes. |
