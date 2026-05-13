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
}
