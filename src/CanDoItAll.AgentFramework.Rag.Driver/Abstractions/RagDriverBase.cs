using CanDoItAll.AgentFramework.Rag.Driver.Embeddings;
using CanDoItAll.AgentFramework.Rag.Driver.Models;

namespace CanDoItAll.AgentFramework.Rag.Driver.Abstractions;

public abstract class RagDriverBase : IRagDriver
{
    private readonly IRagEmbeddingGenerator _embeddingGenerator;

    protected RagDriverBase(
        string providerName,
        RagCollectionOptions defaultCollection,
        IRagEmbeddingGenerator embeddingGenerator,
        RagDriverCapabilities? capabilities = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(providerName);
        ArgumentNullException.ThrowIfNull(defaultCollection);
        ArgumentNullException.ThrowIfNull(embeddingGenerator);

        defaultCollection.Validate();
        ProviderName = providerName;
        DefaultCollection = defaultCollection;
        Capabilities = capabilities ?? RagDriverCapabilities.None;
        _embeddingGenerator = embeddingGenerator;
    }

    public string ProviderName { get; }

    public RagDriverCapabilities Capabilities { get; }

    public RagCollectionOptions DefaultCollection { get; }

    public abstract ValueTask EnsureCollectionAsync(
        RagCollectionOptions? collection = null,
        CancellationToken cancellationToken = default);

    public abstract ValueTask UpsertAsync(
        RagUpsertRequest request,
        CancellationToken cancellationToken = default);

    public abstract ValueTask DeleteAsync(
        RagDeleteRequest request,
        CancellationToken cancellationToken = default);

    public abstract ValueTask<IReadOnlyList<RagSearchResult>> SearchAsync(
        RagSearchRequest request,
        CancellationToken cancellationToken = default);

    protected RagCollectionOptions ResolveCollection(string? collectionName)
    {
        if (string.IsNullOrWhiteSpace(collectionName))
        {
            return DefaultCollection;
        }

        var collection = DefaultCollection with { CollectionName = collectionName };
        collection.Validate();
        return collection;
    }

    protected async ValueTask<float[]> ResolveEntryVectorAsync(
        RagKnowledgeEntry entry,
        RagCollectionOptions collection,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(entry);
        ArgumentNullException.ThrowIfNull(collection);

        entry.Validate(collection.VectorSize);
        EnsureTagsSupported(entry);
        if (entry.Vector is { Length: > 0 } vector)
        {
            return vector;
        }

        var embedding = await _embeddingGenerator
            .GenerateAsync(new RagEmbeddingRequest(entry.Text, collection.VectorSize), cancellationToken)
            .ConfigureAwait(false);

        RagVectorValidation.EnsureVectorSize(embedding.Vector, collection.VectorSize, nameof(embedding));
        return embedding.Vector;
    }

    protected void EnsureTagsSupported(RagKnowledgeEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);

        if (entry.Tags.Count == 0 || Capabilities.SupportsTags)
        {
            return;
        }

        throw new NotSupportedException(
            $"RAG provider '{ProviderName}' does not support knowledge entry tags.");
    }

    protected async ValueTask<float[]> ResolveQueryVectorAsync(
        RagSearchRequest request,
        RagCollectionOptions collection,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(collection);

        request.Validate(collection.VectorSize);
        if (request.Vector is { Length: > 0 } vector)
        {
            return vector;
        }

        var embedding = await _embeddingGenerator
            .GenerateAsync(new RagEmbeddingRequest(request.QueryText, collection.VectorSize), cancellationToken)
            .ConfigureAwait(false);

        RagVectorValidation.EnsureVectorSize(embedding.Vector, collection.VectorSize, nameof(embedding));
        return embedding.Vector;
    }
}
