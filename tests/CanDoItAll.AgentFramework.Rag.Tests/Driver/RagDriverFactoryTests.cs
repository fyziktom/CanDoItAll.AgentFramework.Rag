using CanDoItAll.AgentFramework.Rag.Driver.Abstractions;
using CanDoItAll.AgentFramework.Rag.Driver.Embeddings;
using CanDoItAll.AgentFramework.Rag.Driver.Factories;
using CanDoItAll.AgentFramework.Rag.Driver.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace CanDoItAll.AgentFramework.Rag.Tests.Driver;

public sealed class RagDriverFactoryTests
{
    [Fact]
    public void Create_ReturnsProviderSelectedByOptions()
    {
        var serviceProvider = new ServiceCollection().BuildServiceProvider();
        var options = Options.Create(new RagDriverFactoryOptions
        {
            ProviderName = "fake",
            DefaultCollection = new RagCollectionOptions
            {
                CollectionName = "knowledge",
                VectorSize = 4
            }
        });

        var expectedDriver = new FakeRagDriver(options.Value.DefaultCollection);
        var factory = new RagDriverFactory(
            options,
            new[] { new FakeRagDriverProvider(expectedDriver) },
            serviceProvider);

        var driver = factory.Create();

        Assert.Same(expectedDriver, driver);
    }

    [Fact]
    public void Create_ThrowsWhenProviderIsNotRegistered()
    {
        var serviceProvider = new ServiceCollection().BuildServiceProvider();
        var factory = new RagDriverFactory(
            Options.Create(new RagDriverFactoryOptions { ProviderName = "missing" }),
            Array.Empty<IRagDriverProvider>(),
            serviceProvider);

        var exception = Assert.Throws<InvalidOperationException>(() => factory.Create());
        Assert.Contains("missing", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    private sealed class FakeRagDriverProvider : IRagDriverProvider
    {
        private readonly IRagDriver _driver;

        public FakeRagDriverProvider(IRagDriver driver)
        {
            _driver = driver;
        }

        public string ProviderName => "fake";

        public IRagDriver Create(RagDriverFactoryOptions options, IServiceProvider serviceProvider)
        {
            return _driver;
        }
    }

    private sealed class FakeRagDriver : RagDriverBase
    {
        public FakeRagDriver(RagCollectionOptions defaultCollection)
            : base("fake", defaultCollection, new LocalHashingRagEmbeddingGenerator())
        {
        }

        public override ValueTask EnsureCollectionAsync(
            RagCollectionOptions? collection = null,
            CancellationToken cancellationToken = default)
        {
            return ValueTask.CompletedTask;
        }

        public override ValueTask UpsertAsync(
            RagUpsertRequest request,
            CancellationToken cancellationToken = default)
        {
            return ValueTask.CompletedTask;
        }

        public override ValueTask DeleteAsync(
            RagDeleteRequest request,
            CancellationToken cancellationToken = default)
        {
            return ValueTask.CompletedTask;
        }

        public override ValueTask<IReadOnlyList<RagSearchResult>> SearchAsync(
            RagSearchRequest request,
            CancellationToken cancellationToken = default)
        {
            return ValueTask.FromResult<IReadOnlyList<RagSearchResult>>(Array.Empty<RagSearchResult>());
        }
    }
}
