using CanDoItAll.AgentFramework.Rag.Driver.Models;

namespace CanDoItAll.AgentFramework.Rag.Driver.Abstractions;

public interface IRagDriver
{
    string ProviderName { get; }

    RagDriverCapabilities Capabilities { get; }

    RagCollectionOptions DefaultCollection { get; }

    ValueTask EnsureCollectionAsync(
        RagCollectionOptions? collection = null,
        CancellationToken cancellationToken = default);

    ValueTask UpsertAsync(
        RagUpsertRequest request,
        CancellationToken cancellationToken = default);

    ValueTask DeleteAsync(
        RagDeleteRequest request,
        CancellationToken cancellationToken = default);

    ValueTask DeleteByFilterAsync(
        RagDeleteByFilterRequest request,
        CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException("This RAG driver does not support delete-by-filter operations.");
    }

    ValueTask<RagPayloadIndexResult> EnsurePayloadIndexAsync(
        RagPayloadIndexRequest request,
        CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException("This RAG driver does not support payload indexes.");
    }

    ValueTask<IReadOnlyList<RagSearchResult>> SearchAsync(
        RagSearchRequest request,
        CancellationToken cancellationToken = default);
}
