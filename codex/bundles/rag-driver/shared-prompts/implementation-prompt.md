# Implementation Prompt

Implement the current RAG driver subbundle only.

Before editing, read the root bundle README, `plan/01-phase-plan.md`, the selected subbundle README, `requirements/01-normalized-requirements.md`, and `traceability/01-requirement-traceability.md`. Confirm prerequisites and exact source references still match the repo. If implementation evidence shows the bundle is wrong, repair the bundle and rerun prepared-stage validation before continuing.

Keep the generic Driver project provider-neutral. Put Qdrant-specific references only in the Qdrant project. Use `Qdrant.Client` as a NuGet package. Keep embedding providers injectable and avoid mandatory cloud, Ollama, or model-file dependencies in tests and sample defaults.

After each subbundle, run the listed proof commands, update `reviews/01-execution-report.md`, mark the subbundle status, and record whether downstream work may continue.
