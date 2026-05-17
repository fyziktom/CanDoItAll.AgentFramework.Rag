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
    public void SearchRequestValidate_RejectsInvalidFilter()
    {
        var request = new RagSearchRequest
        {
            QueryText = "approval rules",
            Filter = RagFilterGroup.All()
        };

        Assert.Throws<ArgumentException>(() => request.Validate());
    }

    [Fact]
    public void FilterValidation_AllowsBooleanCompositionAndRange()
    {
        var filter = RagFilterGroup.All(
            RagFilterCondition.Equal("sourceKind", "policy"),
            RagFilterCondition.In("projectId", "project-a", "project-b"),
            RagFilterCondition.Within("projectionVersion", RagFilterRange.Closed(2, 5)),
            RagFilterCondition.Exists("embeddingProfile"));

        filter.Validate();
    }

    [Fact]
    public void FilterValidation_RejectsMixedMembershipKinds()
    {
        var filter = RagFilterCondition.In("projectId", "project-a", 42);

        Assert.Throws<ArgumentException>(() => filter.Validate());
    }

    [Fact]
    public void FilterValidation_RejectsInvalidRangeKind()
    {
        var filter = RagFilterCondition.GreaterThan("sourceKind", "policy");

        Assert.Throws<ArgumentException>(() => filter.Validate());
    }

    [Fact]
    public void PayloadIndexRequestValidate_RejectsUnsupportedIndexKind()
    {
        var request = new RagPayloadIndexRequest
        {
            FieldName = "projectId",
            IndexKind = RagPayloadIndexKind.Unknown
        };

        Assert.Throws<ArgumentOutOfRangeException>(() => request.Validate());
    }

    [Fact]
    public async Task RagDriverBase_RejectsFiltersWhenProviderDoesNotSupportThem()
    {
        var driver = new UnsupportedTagDriver();
        var request = new RagSearchRequest
        {
            QueryText = "approval rules",
            Filter = RagFilterCondition.Equal("sourceKind", "policy")
        };

        var exception = await Assert.ThrowsAsync<NotSupportedException>(async () =>
            await driver.SearchAsync(request));

        Assert.Contains("does not support search filters", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task RagDriverBase_RejectsPayloadIndexesWhenProviderDoesNotSupportThem()
    {
        var driver = new UnsupportedTagDriver();
        var request = new RagPayloadIndexRequest
        {
            FieldName = "projectId",
            IndexKind = RagPayloadIndexKind.Keyword
        };

        var exception = await Assert.ThrowsAsync<NotSupportedException>(async () =>
            await driver.EnsurePayloadIndexAsync(request));

        Assert.Contains("does not support payload indexes", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task RagDriverBase_RejectsDeleteByFilterWhenProviderDoesNotSupportIt()
    {
        var driver = new UnsupportedTagDriver();
        var request = new RagDeleteByFilterRequest
        {
            Filter = RagFilterCondition.Equal("sourceId", "source-1")
        };

        var exception = await Assert.ThrowsAsync<NotSupportedException>(async () =>
            await driver.DeleteByFilterAsync(request));

        Assert.Contains("does not support delete-by-filter", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void DeleteByFilterRequestValidate_RejectsInvalidFilter()
    {
        var request = new RagDeleteByFilterRequest
        {
            Filter = RagFilterGroup.Any()
        };

        Assert.Throws<ArgumentException>(() => request.Validate());
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
            return SearchCoreAsync(request, cancellationToken);
        }

        private async ValueTask<IReadOnlyList<RagSearchResult>> SearchCoreAsync(
            RagSearchRequest request,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var collection = ResolveCollection(request.CollectionName);
            _ = await ResolveQueryVectorAsync(request, collection, cancellationToken);

            return Array.Empty<RagSearchResult>();
        }
    }
}
