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
| `N008` | Use dialogs for collection and record add/edit forms. | `R011` |
| `N009` | Split collection management, record management, and similarity search into tabs. | `R012`, `R016` |
| `N010` | Records management needs a thinner left collection list and right-side record workspace. | `R013` |
| `N011` | Replace large stat cards with compact badges. | `R014` |
| `N012` | Support tags in records when the vector database supports tags and reject tags otherwise. | `R015` |
| `N013` | Add collection and record tag editing with BaseLib `TagEditor`. | `R015`, `R016` |
| `N014` | Add generic RAG-style similarity search across a user-selected set of collections with a collection-picker dialog and removable colored selected-collection badges. | `R017` |
| `N015` | Add the follow-up work as new bundle subbundles before implementation. | `R018` |

## Scope Boundaries

- Build the standalone RAG repository only.
- Do not integrate into the main CanDoItAll solution yet.
- Do not require OpenAI, Ollama, or a downloaded local model for tests.
- Make Qdrant the first implementation, not the only abstraction.
- Preserve room for future vector databases and future embedding providers.
- Keep the Blazor sandbox session-scoped and sample-focused; it does not need persistence or live Qdrant integration.
- Use BaseLib components for dialogs, tabs, tags, grids, buttons, and badges before adding custom CSS.
