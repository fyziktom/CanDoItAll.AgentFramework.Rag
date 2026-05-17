using CanDoItAll.AgentFramework.Rag.Driver.Models;
using CanDoItAll.AgentFramework.Rag.Qdrant.Mapping;
using Qdrant.Client.Grpc;

namespace CanDoItAll.AgentFramework.Rag.Tests.Qdrant;

public sealed class QdrantRagMapperTests
{
    [Fact]
    public void ToPointId_ReturnsStableIdsForStringKnowledgeIds()
    {
        var first = QdrantRagMapper.ToPointId("knowledge:invoice-approval");
        var second = QdrantRagMapper.ToPointId("knowledge:invoice-approval");
        var different = QdrantRagMapper.ToPointId("knowledge:payment-terms");

        Assert.Equal(first, second);
        Assert.NotEqual(first, different);
    }

    [Fact]
    public void ToPointStruct_PreservesKnowledgeTextIdAndMetadata()
    {
        var entry = new RagKnowledgeEntry
        {
            Id = "invoice-approval",
            Text = "Invoices over 5000 require manager approval.",
            Metadata = new Dictionary<string, object?>
            {
                ["source"] = "policy",
                ["priority"] = 2,
                ["active"] = true
            },
            Tags = ["finance", "approval"]
        };

        var point = QdrantRagMapper.ToPointStruct(entry, new[] { 0.1f, 0.2f, 0.3f });

        Assert.Equal((Value)"invoice-approval", point.Payload[QdrantRagMapper.KnowledgeIdPayloadKey]);
        Assert.Equal((Value)"Invoices over 5000 require manager approval.", point.Payload[QdrantRagMapper.KnowledgeTextPayloadKey]);
        Assert.Equal((Value)"policy", point.Payload["source"]);
        Assert.Equal((Value)2L, point.Payload["priority"]);
        Assert.Equal((Value)true, point.Payload["active"]);
        Assert.True(point.Payload.ContainsKey(QdrantRagMapper.KnowledgeTagsPayloadKey));
    }

    [Fact]
    public void ToVectorParams_MapsGenericDistance()
    {
        var options = new RagCollectionOptions
        {
            CollectionName = "knowledge",
            VectorSize = 12,
            Distance = RagDistanceMetric.Cosine
        };

        var vectorParams = QdrantRagMapper.ToVectorParams(options);

        Assert.Equal(12ul, vectorParams.Size);
        Assert.Equal(Distance.Cosine, vectorParams.Distance);
    }

    [Fact]
    public void ToSearchResult_RecreatesKnowledgeEntryFromPayload()
    {
        var scoredPoint = new ScoredPoint
        {
            Id = QdrantRagMapper.ToPointId("invoice-approval"),
            Score = 0.98f,
            Payload =
            {
                [QdrantRagMapper.KnowledgeIdPayloadKey] = "invoice-approval",
                [QdrantRagMapper.KnowledgeTextPayloadKey] = "Invoices over 5000 require manager approval.",
                [QdrantRagMapper.KnowledgeTagsPayloadKey] = new[] { "finance", "approval" },
                ["source"] = "policy",
                ["priority"] = 2
            }
        };

        var result = QdrantRagMapper.ToSearchResult(scoredPoint);

        Assert.Equal("invoice-approval", result.Knowledge.Id);
        Assert.Equal("Invoices over 5000 require manager approval.", result.Knowledge.Text);
        Assert.Equal("policy", result.Knowledge.Metadata["source"]);
        Assert.Equal(2L, result.Knowledge.Metadata["priority"]);
        Assert.Equal(["approval", "finance"], result.Knowledge.Tags);
        Assert.Equal(0.98f, result.Score, precision: 3);
    }

    [Fact]
    public void ToFilter_MapsStringEquality()
    {
        var filter = QdrantRagMapper.ToFilter(RagFilterCondition.Equal("sourceKind", "policy"));

        var condition = Assert.Single(filter.Must);
        Assert.Equal("sourceKind", condition.Field.Key);
        Assert.Equal("policy", condition.Field.Match.Keyword);
    }

    [Fact]
    public void ToFilter_MapsStringMembership()
    {
        var filter = QdrantRagMapper.ToFilter(
            RagFilterCondition.In("projectId", "project-a", "project-b"));

        var condition = Assert.Single(filter.Must);
        Assert.Equal("projectId", condition.Field.Key);
        Assert.Equal(["project-a", "project-b"], condition.Field.Match.Keywords.Strings);
    }

    [Fact]
    public void ToFilter_MapsNumericRange()
    {
        var filter = QdrantRagMapper.ToFilter(
            RagFilterCondition.Within("projectionVersion", RagFilterRange.Closed(2, 5)));

        var condition = Assert.Single(filter.Must);
        Assert.Equal("projectionVersion", condition.Field.Key);
        Assert.True(condition.Field.Range.HasGte);
        Assert.True(condition.Field.Range.HasLte);
        Assert.Equal(2, condition.Field.Range.Gte);
        Assert.Equal(5, condition.Field.Range.Lte);
    }

    [Fact]
    public void ToFilter_MapsExistenceAsNotEmpty()
    {
        var filter = QdrantRagMapper.ToFilter(RagFilterCondition.Exists("embeddingProfile"));

        var condition = Assert.Single(filter.MustNot);
        Assert.Equal("embeddingProfile", condition.IsEmpty.Key);
    }

    [Fact]
    public void ToFilter_MapsBooleanComposition()
    {
        var filter = QdrantRagMapper.ToFilter(
            RagFilterGroup.Any(
                RagFilterCondition.Equal("sourceKind", "workflow"),
                RagFilterCondition.Equal("sourceKind", "process")));

        Assert.Empty(filter.Must);
        Assert.Equal(2, filter.Should.Count);
        Assert.All(filter.Should, condition => Assert.NotNull(condition.Filter));
    }

    [Fact]
    public void ToFilter_MapsProjectionLifecycleCleanupMetadata()
    {
        var filter = QdrantRagMapper.ToFilter(
            RagFilterGroup.All(
                RagFilterCondition.Equal("sourceId", "source-1"),
                RagFilterCondition.Equal("embeddingProfile", "local-hashing:dimension=384"),
                RagFilterCondition.LessThan("projectionVersion", 7)));

        Assert.Equal(3, filter.Must.Count);
        Assert.All(filter.Must, condition => Assert.NotNull(condition.Filter));

        var versionCondition = filter.Must[2].Filter.Must.Single();
        Assert.Equal("projectionVersion", versionCondition.Field.Key);
        Assert.True(versionCondition.Field.Range.HasLt);
        Assert.Equal(7, versionCondition.Field.Range.Lt);
    }

    [Fact]
    public void ToPayloadSchemaType_MapsGenericIndexKinds()
    {
        Assert.Equal(PayloadSchemaType.Keyword, QdrantRagMapper.ToPayloadSchemaType(RagPayloadIndexKind.Keyword));
        Assert.Equal(PayloadSchemaType.Integer, QdrantRagMapper.ToPayloadSchemaType(RagPayloadIndexKind.Integer));
        Assert.Equal(PayloadSchemaType.Bool, QdrantRagMapper.ToPayloadSchemaType(RagPayloadIndexKind.Boolean));
        Assert.Equal(PayloadSchemaType.Datetime, QdrantRagMapper.ToPayloadSchemaType(RagPayloadIndexKind.DateTime));
    }
}
