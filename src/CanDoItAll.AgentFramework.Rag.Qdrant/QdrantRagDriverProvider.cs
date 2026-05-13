using CanDoItAll.AgentFramework.Rag.Driver.Abstractions;
using CanDoItAll.AgentFramework.Rag.Driver.Embeddings;
using CanDoItAll.AgentFramework.Rag.Driver.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Qdrant.Client;

namespace CanDoItAll.AgentFramework.Rag.Qdrant;

public sealed class QdrantRagDriverProvider : IRagDriverProvider
{
    public string ProviderName => RagDriverProviderNames.Qdrant;

    public IRagDriver Create(
        RagDriverFactoryOptions options,
        IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(serviceProvider);
        options.Validate();

        return new QdrantRagDriver(
            serviceProvider.GetRequiredService<QdrantClient>(),
            serviceProvider.GetRequiredService<IRagEmbeddingGenerator>(),
            options.DefaultCollection,
            serviceProvider.GetRequiredService<IOptions<QdrantRagOptions>>().Value);
    }
}
