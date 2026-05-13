# QA Prompt

Review the completed RAG driver bundle against the raw request and normalized requirements.

Check that the solution structure mirrors the SemanticCompletion standalone repository, that the core Driver project has no Qdrant package/type dependency, that Qdrant is implemented through `Qdrant.Client` from NuGet, and that factory/options selection can return the configured provider. Confirm embeddings are provider-driven, with a local deterministic default for tests and samples and clear extension points for SemanticCompletion, OpenAI, Ollama, and main CanDoItAll providers.

Run or inspect the recorded build and test proof. Verify `reviews/01-execution-report.md` has populated subbundle gate rows, no browser proof is required, and every raw note is marked `Solved`, `Partially solved`, or `Not solved` with proof. Fail the closure gate if any required proof is missing or any generic API leaks Qdrant types.
