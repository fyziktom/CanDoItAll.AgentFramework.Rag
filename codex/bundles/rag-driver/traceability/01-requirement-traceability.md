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
| `R011` | `N008` | `06-sandbox-dialog-tabs-and-tags` | `requirements/01-normalized-requirements.md`, `architecture/01-target-solution.md` | Browser proof for collection and record dialogs. |
| `R012` | `N009` | `06-sandbox-dialog-tabs-and-tags` | `plan/01-phase-plan.md` | Browser proof for three-tab sandbox navigation. |
| `R013` | `N010` | `06-sandbox-dialog-tabs-and-tags` | `architecture/01-target-solution.md` | Browser proof that left collection selection changes right records. |
| `R014` | `N011` | `06-sandbox-dialog-tabs-and-tags` | `architecture/01-target-solution.md` | Screenshot review confirms badges replaced summary cards. |
| `R015` | `N012`, `N013` | `05-driver-tag-capabilities` | `requirements/01-normalized-requirements.md` | Unit tests for tag rejection and Qdrant tag payload mapping. |
| `R016` | `N013` | `06-sandbox-dialog-tabs-and-tags` | `architecture/01-target-solution.md` | Browser proof for TagEditor in collection and record dialogs. |
| `R017` | `N014` | `07-sandbox-generic-similarity-search` | `architecture/01-target-solution.md` | Browser proof for picker dialog, selected collection chips, and cross-collection results. |
| `R018` | `N015` | `05-driver-tag-capabilities`, `06-sandbox-dialog-tabs-and-tags`, `07-sandbox-generic-similarity-search` | `plan/01-phase-plan.md`, `reviews/01-execution-report.md` | Prepared-stage bundle validator passes before implementation resumes. |
