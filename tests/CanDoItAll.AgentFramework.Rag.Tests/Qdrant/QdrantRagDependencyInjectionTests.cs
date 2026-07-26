using CanDoItAll.AgentFramework.Rag.Driver.Abstractions;
using CanDoItAll.AgentFramework.Rag.Driver.DependencyInjection;
using CanDoItAll.AgentFramework.Rag.Driver.Embeddings;
using CanDoItAll.AgentFramework.Rag.Driver.Models;
using CanDoItAll.AgentFramework.Rag.Qdrant;
using CanDoItAll.AgentFramework.Rag.Qdrant.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace CanDoItAll.AgentFramework.Rag.Tests.Qdrant;

public sealed class QdrantRagDependencyInjectionTests
{
    [Fact]
    public void AddQdrantRagDriver_RegistersFactoryAndDefaultDriver()
    {
        var services = new ServiceCollection();
        services.AddLocalHashingRagEmbeddingGenerator(options => options.Dimension = 8);
        services.AddQdrantRagDriver(
            configureFactory: options =>
            {
                options.DefaultCollection = new RagCollectionOptions
                {
                    CollectionName = "test-knowledge",
                    VectorSize = 8
                };
            });

        using var provider = services.BuildServiceProvider();

        var factory = provider.GetRequiredService<IRagDriverFactory>();
        var driver = factory.Create();
        var defaultDriver = provider.GetRequiredService<IRagDriver>();
        var embeddingGenerator = provider.GetRequiredService<IRagEmbeddingGenerator>();

        Assert.Equal(QdrantRagDriver.ProviderIdentifier, driver.ProviderName);
        Assert.Equal("test-knowledge", driver.DefaultCollection.CollectionName);
        Assert.Equal(QdrantRagDriver.ProviderIdentifier, defaultDriver.ProviderName);
        Assert.IsType<LocalHashingRagEmbeddingGenerator>(embeddingGenerator);
    }

    [Fact]
    public void AddQdrantRagDriver_WithoutEmbeddingRegistration_FailsClearly()
    {
        var services = new ServiceCollection();
        services.AddQdrantRagDriver();

        using var provider = services.BuildServiceProvider();

        var exception = Assert.Throws<InvalidOperationException>(
            () => provider.GetRequiredService<IRagDriverFactory>());

        Assert.Contains(
            typeof(IRagEmbeddingGenerator).FullName!,
            exception.Message,
            StringComparison.Ordinal);
    }
}
