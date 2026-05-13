using CanDoItAll.AgentFramework.Rag.Driver.Models;

namespace CanDoItAll.AgentFramework.Rag.Driver.Abstractions;

public interface IRagDriver
{
    string ProviderName { get; }

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

    ValueTask<IReadOnlyList<RagSearchResult>> SearchAsync(
        RagSearchRequest request,
        CancellationToken cancellationToken = default);
}
