using CanDoItAll.AgentFramework.Rag.Qdrant.Mapping;
using Qdrant.Client.Grpc;

namespace CanDoItAll.AgentFramework.Rag.Tests.Qdrant;

public sealed class QdrantPayloadValueMapperTests
{
    [Fact]
    public void ToPayloadValue_MapsPrimitiveAndCollectionValues()
    {
        Assert.Equal((Value)"policy", QdrantPayloadValueMapper.ToPayloadValue("policy"));
        Assert.Equal((Value)7L, QdrantPayloadValueMapper.ToPayloadValue(7));
        Assert.Equal((Value)true, QdrantPayloadValueMapper.ToPayloadValue(true));

        var list = QdrantPayloadValueMapper.ToPayloadValue(new[] { "finance", "approval" });
        Assert.Equal(
            ["finance", "approval"],
            list.ListValue.Values.Select(value => value.StringValue));
    }

    [Fact]
    public void FromPayloadValue_RecreatesPrimitiveAndCollectionValues()
    {
        Assert.Equal("policy", QdrantPayloadValueMapper.FromPayloadValue((Value)"policy"));
        Assert.Equal(7L, QdrantPayloadValueMapper.FromPayloadValue((Value)7L));
        Assert.Equal(true, QdrantPayloadValueMapper.FromPayloadValue((Value)true));

        var list = QdrantPayloadValueMapper.FromPayloadValue((Value)new[] { "finance", "approval" });
        Assert.Equal(["finance", "approval"], Assert.IsType<object?[]>(list));
    }
}
