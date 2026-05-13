# Structured Input

## Raw Notes

| ID | Raw request | Normalized requirement |
| --- | --- | --- |
| `N001` | Create solution with projects and standard CanDoItAll repo structure. | `R001` |
| `N002` | Build generic RAG driver interfaces because different vector databases may be used. | `R002`, `R003` |
| `N003` | Start with Qdrant and use its .NET driver as a NuGet package. | `R004` |
| `N004` | Add a factory where options select the proper driver instance. | `R005` |
| `N005` | Store knowledges in a vector DB and support embeddings conversion. | `R006`, `R007` |
| `N006` | Embedding implementations must be pluggable from SemanticCompletion, OpenAI, Ollama, or main CanDoItAll providers. | `R007`, `R008` |
| `N007` | Add a sample console application showing driver usage. | `R009` |

## Scope Boundaries

- Build the standalone RAG repository only.
- Do not integrate into the main CanDoItAll solution yet.
- Do not require OpenAI, Ollama, or a downloaded local model for tests.
- Make Qdrant the first implementation, not the only abstraction.
- Preserve room for future vector databases and future embedding providers.
