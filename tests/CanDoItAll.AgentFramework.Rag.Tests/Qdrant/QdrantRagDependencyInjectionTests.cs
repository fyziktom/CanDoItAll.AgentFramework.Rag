using CanDoItAll.AgentFramework.Rag.Driver.Abstractions;
using CanDoItAll.AgentFramework.Rag.Driver.Models;
using CanDoItAll.AgentFramework.Rag.Qdrant.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace CanDoItAll.AgentFramework.Rag.Tests.Qdrant;

public sealed class QdrantRagDependencyInjectionTests
{
    [Fact]
    public void AddQdrantRagDriver_RegistersFactoryAndDefaultDriver()
    {
        var services = new ServiceCollection();
        services.AddQdrantRagDriver(
            configureFactory: options =>
            {
                options.DefaultCollection = new RagCollectionOptions
                {
                    CollectionName = "test-knowledge",
                    VectorSize = 8
                };
            },
            configureEmbedding: options => options.Dimension = 8);

        using var provider = services.BuildServiceProvider();

        var factory = provider.GetRequiredService<IRagDriverFactory>();
        var driver = factory.Create();
        var defaultDriver = provider.GetRequiredService<IRagDriver>();

        Assert.Equal(RagDriverProviderNames.Qdrant, driver.ProviderName);
        Assert.Equal("test-knowledge", driver.DefaultCollection.CollectionName);
        Assert.Equal(RagDriverProviderNames.Qdrant, defaultDriver.ProviderName);
    }
}
