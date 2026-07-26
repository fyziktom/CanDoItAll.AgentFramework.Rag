using CanDoItAll.AgentFramework.Rag.Driver.Embeddings;
using CanDoItAll.AgentFramework.Rag.Driver.Models;
using CanDoItAll.AgentFramework.Rag.Sandbox.Services;

namespace CanDoItAll.AgentFramework.Rag.Tests.Sandbox;

public sealed class RagSandboxStoreTests
{
    private static readonly DateTimeOffset FixedTimestamp =
        new(2026, 7, 26, 12, 30, 0, TimeSpan.Zero);

    [Fact]
    public void Constructor_SeedsStateWithoutGeneratingEmbeddings()
    {
        var generator = new RecordingEmbeddingGenerator();
        var store = CreateStore(generator);

        Assert.Equal(2, store.SearchCollections(null).Count);
        Assert.Empty(generator.Requests);
    }

    [Fact]
    public async Task SearchRecordsAsync_FirstSearchBuildsRecordVectorsLazily()
    {
        var generator = new RecordingEmbeddingGenerator();
        var store = CreateStore(generator);

        var firstResults = await store.SearchRecordsAsync(
            "finance-policies",
            "invoice",
            limit: 25,
            TestContext.Current.CancellationToken);
        var requestsAfterFirstSearch = generator.Requests.Count;
        var secondResults = await store.SearchRecordsAsync(
            "finance-policies",
            "invoice",
            limit: 25,
            TestContext.Current.CancellationToken);

        Assert.Equal(2, firstResults.Count);
        Assert.Equal(2, secondResults.Count);
        Assert.Equal(3, requestsAfterFirstSearch);
        Assert.Equal(4, generator.Requests.Count);
        Assert.Equal(2, generator.Requests.Count(request => request.Text != "invoice"));
        Assert.All(generator.Requests, request => Assert.Equal(64, request.Dimensions));
    }

    [Fact]
    public async Task SaveRecordAsync_DefersEmbeddingAndUsesInjectedClock()
    {
        var generator = new RecordingEmbeddingGenerator();
        var store = CreateStore(generator);

        var saved = await store.SaveRecordAsync(
            "support-runbooks",
            new RagSandboxRecordEditModel
            {
                Id = "incident-escalation",
                Text = "Escalate unresolved critical incidents after fifteen minutes.",
                Metadata = "owner=support",
                Tags = ["incident", "escalation"]
            },
            TestContext.Current.CancellationToken);

        Assert.Equal(FixedTimestamp, saved.UpdatedAt);
        Assert.Empty(generator.Requests);

        var results = await store.SearchRecordsAsync(
            "support-runbooks",
            "critical incident",
            limit: 25,
            TestContext.Current.CancellationToken);

        Assert.Equal(2, results.Count);
        Assert.Equal(3, generator.Requests.Count);
    }

    [Fact]
    public async Task SaveCollectionAsync_VectorSizeChangeInvalidatesCachedVectors()
    {
        var generator = new RecordingEmbeddingGenerator();
        var store = CreateStore(generator);
        await store.SearchRecordsAsync(
            "finance-policies",
            "invoice",
            limit: 25,
            TestContext.Current.CancellationToken);

        await store.SaveCollectionAsync(new RagSandboxCollectionEditModel
        {
            OriginalName = "finance-policies",
            Name = "finance-policies",
            Description = "Finance policies.",
            Tags = ["finance"],
            VectorSize = 32,
            Distance = RagDistanceMetric.Cosine
        }, TestContext.Current.CancellationToken);

        Assert.Equal(3, generator.Requests.Count);

        await store.SearchRecordsAsync(
            "finance-policies",
            "invoice",
            limit: 25,
            TestContext.Current.CancellationToken);

        Assert.Equal(6, generator.Requests.Count);
        Assert.Equal([64, 64, 64, 32, 32, 32], generator.Requests.Select(request => request.Dimensions));
    }

    [Fact]
    public async Task SaveRecordAsync_MissingCollection_ThrowsInvalidOperationException()
    {
        var store = CreateStore(new RecordingEmbeddingGenerator());

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await store.SaveRecordAsync(
                "missing",
                new RagSandboxRecordEditModel
                {
                    Id = "record",
                    Text = "text"
                },
                TestContext.Current.CancellationToken));

        Assert.Contains("missing", exception.Message, StringComparison.Ordinal);
    }

    private static RagSandboxStore CreateStore(RecordingEmbeddingGenerator generator)
    {
        return new RagSandboxStore(
            generator,
            new RagSandboxSimilarityCalculator(),
            new FixedTimeProvider(FixedTimestamp));
    }

    private sealed class RecordingEmbeddingGenerator : IRagEmbeddingGenerator
    {
        public List<RagEmbeddingRequest> Requests { get; } = [];

        public ValueTask<RagEmbedding> GenerateAsync(
            RagEmbeddingRequest request,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Requests.Add(request);

            var vector = new float[request.Dimensions ?? 8];
            vector[0] = 1;
            return ValueTask.FromResult(new RagEmbedding(request.Text, vector, "test"));
        }
    }

    private sealed class FixedTimeProvider(DateTimeOffset timestamp) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => timestamp;
    }
}
