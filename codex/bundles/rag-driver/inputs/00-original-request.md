# Original Request

Use `candoitall-bundle-workflow` to solve this:

We need to have drivers for RAG.

I created this repo, because we will do those drivers aside of the main candoitall slnx.

Create solution with projects. Keep our standard structure of candoitall repos (check for example `C:\repositories\CanDoItAll.AgentFramework.SemanticCompletion`).

We need wrapped generic driver for RAG, because there are different vector dbs we could use.

We will start with qdrant. I started it in docker (`3dc22cfd8a87a8ae6bf6196adf2b87b27f3231ebfe4b592bccf225a04b65429f`) and I cloned their .NET driver so you can have references.

`C:\repositories\qdrant-dotnet`

Use it as nuget package. Code is just for references. I also installed skills like `qdrant-clients-sdk` where you can find detailed instructions related to qdrant.

We need some generic interface and driver wrap for qdrant will be just one of the implementations. Add also factory where you can specify options and will give you proper instance of the driver.

Our main purpose of using vector db is storing of knowledges. It will need embeddings conversion.

For creating embeddings you will have to add proper interface and implementations can be for example use of `C:\repositories\CanDoItAll.AgentFramework.SemanticCompletion` or OpenAI API or local Ollama. This should be possible to feed from above too, because for example in main candoitall we already have LLM providers so user will do the settings and we will just add providers for embeddings.

Add also some sample console application that shows how to work with our driver.

## Follow-up Request

After the initial driver and Blazor sandbox were created, add these improvements:

1. Use dialogs for add/edit forms for collections and records.
2. Use tabs to split collection management, record management, and generic similarity search.
3. In the records tab, show a thinner left collection list and a right-side record management workspace for the selected collection.
4. Replace the large top stat cards with compact badges.
5. Add tag support for records when the vector database supports tags; reject tags through the driver when it does not.
6. Add collection and record tag editing through the BaseLib `TagEditor` component.
7. Add a third tab for RAG-style similarity search over a selectable set of collections. The add-collections dialog must support double-click single add plus checkbox multi-select and confirm. Selected collections should render as removable colored badges, using `TagEditor` behavior.
8. Repair the bundle by adding these steps as new subbundles before implementation.
