using CanDoItAll.AgentFramework.Rag.Driver.Abstractions;
using CanDoItAll.AgentFramework.Rag.Driver.Embeddings;
using CanDoItAll.AgentFramework.Rag.Driver.Models;
using Microsoft.Extensions.Options;
using Qdrant.Client;

namespace CanDoItAll.AgentFramework.Rag.Qdrant;

internal sealed class QdrantRagDriverProvider : IRagDriverProvider
{
    private readonly QdrantClient _client;
    private readonly IRagEmbeddingGenerator _embeddingGenerator;
    private readonly QdrantRagOptions _options;

    public QdrantRagDriverProvider(
        QdrantClient client,
        IRagEmbeddingGenerator embeddingGenerator,
        IOptions<QdrantRagOptions> options)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _embeddingGenerator = embeddingGenerator ?? throw new ArgumentNullException(nameof(embeddingGenerator));
        ArgumentNullException.ThrowIfNull(options);

        _options = options.Value;
        _options.Validate();
    }

    public string ProviderName => QdrantRagDriver.ProviderIdentifier;

    public IRagDriver Create(RagDriverFactoryOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        options.Validate();

        return new QdrantRagDriver(
            _client,
            _embeddingGenerator,
            options.DefaultCollection,
            _options);
    }
}
