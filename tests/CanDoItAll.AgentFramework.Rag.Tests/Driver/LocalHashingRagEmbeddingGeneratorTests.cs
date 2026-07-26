using CanDoItAll.AgentFramework.Rag.Driver.Embeddings;

namespace CanDoItAll.AgentFramework.Rag.Tests.Driver;

public sealed class LocalHashingRagEmbeddingGeneratorTests
{
    [Fact]
    public async Task GenerateAsync_ReturnsConfiguredDimension()
    {
        var generator = new LocalHashingRagEmbeddingGenerator(new LocalHashingRagEmbeddingOptions
        {
            Dimension = 12
        });

        var embedding = await generator.GenerateAsync(
            new RagEmbeddingRequest("Store invoice approval knowledge."),
            TestContext.Current.CancellationToken);

        Assert.Equal(12, embedding.Dimension);
        Assert.Equal(LocalHashingRagEmbeddingGenerator.ProviderName, embedding.ProviderName);
        Assert.All(embedding.Vector, value => Assert.True(float.IsFinite(value)));
    }

    [Fact]
    public async Task GenerateAsync_IsDeterministicForSameTextAndDimensions()
    {
        var generator = new LocalHashingRagEmbeddingGenerator(new LocalHashingRagEmbeddingOptions
        {
            Dimension = 16
        });

        var first = await generator.GenerateAsync(
            new RagEmbeddingRequest("Qdrant stores knowledge vectors."),
            TestContext.Current.CancellationToken);
        var second = await generator.GenerateAsync(
            new RagEmbeddingRequest("Qdrant stores knowledge vectors."),
            TestContext.Current.CancellationToken);

        Assert.Equal(first.Vector, second.Vector);
    }

    [Fact]
    public async Task GenerateAsync_RequestDimensionOverridesDefaultDimension()
    {
        var generator = new LocalHashingRagEmbeddingGenerator(new LocalHashingRagEmbeddingOptions
        {
            Dimension = 16
        });

        var embedding = await generator.GenerateAsync(
            new RagEmbeddingRequest("short text", Dimensions: 8),
            TestContext.Current.CancellationToken);

        Assert.Equal(8, embedding.Dimension);
    }
}
