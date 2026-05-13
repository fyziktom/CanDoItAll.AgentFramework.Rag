using CanDoItAll.AgentFramework.Rag.Driver.Abstractions;
using CanDoItAll.AgentFramework.Rag.Driver.Embeddings;
using CanDoItAll.AgentFramework.Rag.Driver.Models;

namespace CanDoItAll.AgentFramework.Rag.Tests.Driver;

public sealed class RagModelValidationTests
{
    [Fact]
    public void KnowledgeEntryValidate_AllowsMetadataAndExpectedVectorSize()
    {
        var entry = new RagKnowledgeEntry
        {
            Id = "invoice-approval",
            Text = "Approvals require manager sign-off.",
            Metadata = new Dictionary<string, object?>
            {
                ["source"] = "policy",
                ["priority"] = 1
            },
            Vector = new[] { 0.1f, 0.2f, 0.3f }
        };

        entry.Validate(expectedVectorSize: 3);

        Assert.Equal("policy", entry.Metadata["source"]);
    }

    [Fact]
    public void KnowledgeEntryValidate_RejectsWrongVectorSize()
    {
        var entry = new RagKnowledgeEntry
        {
            Id = "invoice-approval",
            Text = "Approvals require manager sign-off.",
            Vector = new[] { 0.1f, 0.2f }
        };

        Assert.Throws<ArgumentException>(() => entry.Validate(expectedVectorSize: 3));
    }

    [Fact]
    public void KnowledgeEntryValidate_RejectsDuplicateTags()
    {
        var entry = new RagKnowledgeEntry
        {
            Id = "invoice-approval",
            Text = "Approvals require manager sign-off.",
            Tags = ["finance", "Finance"]
        };

        Assert.Throws<ArgumentException>(() => entry.Validate());
    }

    [Fact]
    public async Task RagDriverBase_RejectsTagsWhenProviderDoesNotSupportThem()
    {
        var driver = new UnsupportedTagDriver();
        var request = new RagUpsertRequest
        {
            Entries =
            [
                new RagKnowledgeEntry
                {
                    Id = "invoice-approval",
                    Text = "Approvals require manager sign-off.",
                    Tags = ["finance"]
                }
            ]
        };

        var exception = await Assert.ThrowsAsync<NotSupportedException>(async () =>
            await driver.UpsertAsync(request));

        Assert.Contains("does not support knowledge entry tags", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void SearchRequestValidate_RejectsNonPositiveLimit()
    {
        var request = new RagSearchRequest
        {
            QueryText = "approval rules",
            Limit = 0
        };

        Assert.Throws<ArgumentOutOfRangeException>(() => request.Validate());
    }

    [Fact]
    public void VectorValidation_RejectsNonFiniteValues()
    {
        Assert.Throws<ArgumentException>(() =>
            RagVectorValidation.EnsureVectorSize(new[] { 1.0f, float.NaN }, 2, "vector"));
    }

    private sealed class UnsupportedTagDriver : RagDriverBase
    {
        public UnsupportedTagDriver()
            : base(
                "unsupported-tags",
                new RagCollectionOptions { CollectionName = "test", VectorSize = 3 },
                new LocalHashingRagEmbeddingGenerator(new LocalHashingRagEmbeddingOptions { Dimension = 3 }),
                RagDriverCapabilities.None)
        {
        }

        public override ValueTask EnsureCollectionAsync(
            RagCollectionOptions? collection = null,
            CancellationToken cancellationToken = default)
        {
            return ValueTask.CompletedTask;
        }

        public override async ValueTask UpsertAsync(
            RagUpsertRequest request,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(request);
            var collection = ResolveCollection(request.CollectionName);
            request.Validate(collection.VectorSize);

            foreach (var entry in request.Entries)
            {
                _ = await ResolveEntryVectorAsync(entry, collection, cancellationToken);
            }
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
