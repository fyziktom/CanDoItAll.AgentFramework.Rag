using CanDoItAll.AgentFramework.Rag.Driver.Models;
using CanDoItAll.AgentFramework.Rag.Qdrant.Mapping;
using Qdrant.Client.Grpc;

namespace CanDoItAll.AgentFramework.Rag.Tests.Qdrant;

public sealed class QdrantPointMapperTests
{
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

        var point = QdrantPointMapper.ToPointStruct(entry, [0.1f, 0.2f, 0.3f]);

        Assert.Equal((Value)"invoice-approval", point.Payload[QdrantPointMapper.KnowledgeIdPayloadKey]);
        Assert.Equal(
            (Value)"Invoices over 5000 require manager approval.",
            point.Payload[QdrantPointMapper.KnowledgeTextPayloadKey]);
        Assert.Equal((Value)"policy", point.Payload["source"]);
        Assert.Equal((Value)2L, point.Payload["priority"]);
        Assert.Equal((Value)true, point.Payload["active"]);
        Assert.True(point.Payload.ContainsKey(QdrantPointMapper.KnowledgeTagsPayloadKey));
    }

    [Fact]
    public void ToSearchResult_RecreatesKnowledgeEntryFromPayload()
    {
        var scoredPoint = new ScoredPoint
        {
            Id = QdrantPointIdMapper.ToPointId("invoice-approval"),
            Score = 0.98f,
            Payload =
            {
                [QdrantPointMapper.KnowledgeIdPayloadKey] = "invoice-approval",
                [QdrantPointMapper.KnowledgeTextPayloadKey] = "Invoices over 5000 require manager approval.",
                [QdrantPointMapper.KnowledgeTagsPayloadKey] = new[] { "finance", "approval" },
                ["source"] = "policy",
                ["priority"] = 2
            }
        };

        var result = QdrantPointMapper.ToSearchResult(scoredPoint);

        Assert.Equal("invoice-approval", result.Knowledge.Id);
        Assert.Equal("Invoices over 5000 require manager approval.", result.Knowledge.Text);
        Assert.Equal("policy", result.Knowledge.Metadata["source"]);
        Assert.Equal(2L, result.Knowledge.Metadata["priority"]);
        Assert.Equal(["approval", "finance"], result.Knowledge.Tags);
        Assert.Equal(0.98f, result.Score, precision: 3);
    }
}
