using CanDoItAll.AgentFramework.Rag.Driver.Embeddings;
using CanDoItAll.AgentFramework.Rag.Driver.Models;
using CanDoItAll.AgentFramework.Rag.Qdrant;

namespace CanDoItAll.AgentFramework.Rag.Tests.Qdrant;

public sealed class QdrantRagDriverLeaseTests
{
    [Fact]
    public void Create_OwnsConfiguredQdrantDriverWithoutNetworkAccess()
    {
        var collection = new RagCollectionOptions
        {
            CollectionName = "memory",
            VectorSize = 3,
            Distance = RagDistanceMetric.Cosine
        };
        var embeddings = new LocalHashingRagEmbeddingGenerator(new LocalHashingRagEmbeddingOptions
        {
            Dimension = 3
        });

        using var lease = QdrantRagDriverLease.Create(
            new QdrantRagOptions
            {
                Host = "localhost",
                Port = 6334
            },
            collection,
            embeddings);

        Assert.Equal(RagDriverProviderNames.Qdrant, lease.Driver.ProviderName);
        Assert.Equal(collection, lease.Driver.DefaultCollection);
        Assert.True(lease.Driver.Capabilities.SupportsFilters);
        Assert.True(lease.Driver.Capabilities.SupportsPayloadIndexes);
    }
}
