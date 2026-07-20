using CanDoItAll.AgentFramework.Rag.Driver.Abstractions;
using CanDoItAll.AgentFramework.Rag.Driver.Embeddings;
using CanDoItAll.AgentFramework.Rag.Driver.Models;
using Qdrant.Client;

namespace CanDoItAll.AgentFramework.Rag.Qdrant;

public sealed class QdrantRagDriverLease : IDisposable
{
    private QdrantClient? client;

    private QdrantRagDriverLease(QdrantClient client, IRagDriver driver)
    {
        this.client = client;
        Driver = driver;
    }

    public IRagDriver Driver { get; }

    public static QdrantRagDriverLease Create(
        QdrantRagOptions options,
        RagCollectionOptions defaultCollection,
        IRagEmbeddingGenerator embeddingGenerator)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(defaultCollection);
        ArgumentNullException.ThrowIfNull(embeddingGenerator);
        options.Validate();
        defaultCollection.Validate();
        var client = new QdrantClient(
            options.Host,
            options.Port,
            options.Https,
            options.ApiKey,
            options.GrpcTimeout);
        try
        {
            var driver = new QdrantRagDriver(
                client,
                embeddingGenerator,
                defaultCollection,
                options);
            return new QdrantRagDriverLease(client, driver);
        }
        catch
        {
            client.Dispose();
            throw;
        }
    }

    public void Dispose()
    {
        Interlocked.Exchange(ref client, null)?.Dispose();
    }
}
