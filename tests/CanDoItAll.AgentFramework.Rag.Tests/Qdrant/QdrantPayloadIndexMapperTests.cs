using CanDoItAll.AgentFramework.Rag.Driver.Models;
using CanDoItAll.AgentFramework.Rag.Qdrant.Mapping;
using Qdrant.Client.Grpc;

namespace CanDoItAll.AgentFramework.Rag.Tests.Qdrant;

public sealed class QdrantPayloadIndexMapperTests
{
    [Theory]
    [InlineData(RagPayloadIndexKind.Keyword, PayloadSchemaType.Keyword)]
    [InlineData(RagPayloadIndexKind.Integer, PayloadSchemaType.Integer)]
    [InlineData(RagPayloadIndexKind.Float, PayloadSchemaType.Float)]
    [InlineData(RagPayloadIndexKind.Boolean, PayloadSchemaType.Bool)]
    [InlineData(RagPayloadIndexKind.DateTime, PayloadSchemaType.Datetime)]
    [InlineData(RagPayloadIndexKind.Text, PayloadSchemaType.Text)]
    [InlineData(RagPayloadIndexKind.Uuid, PayloadSchemaType.Uuid)]
    public void ToPayloadSchemaType_MapsGenericIndexKinds(
        RagPayloadIndexKind source,
        PayloadSchemaType expected)
    {
        Assert.Equal(expected, QdrantPayloadIndexMapper.ToPayloadSchemaType(source));
    }

    [Fact]
    public void ToPayloadIndexParams_MapsProviderSpecificParameters()
    {
        Assert.IsType<KeywordIndexParams>(
            QdrantPayloadIndexMapper.ToPayloadIndexParams(RagPayloadIndexKind.Keyword).KeywordIndexParams);
        Assert.IsType<IntegerIndexParams>(
            QdrantPayloadIndexMapper.ToPayloadIndexParams(RagPayloadIndexKind.Integer).IntegerIndexParams);
        Assert.IsType<FloatIndexParams>(
            QdrantPayloadIndexMapper.ToPayloadIndexParams(RagPayloadIndexKind.Float).FloatIndexParams);
        Assert.IsType<BoolIndexParams>(
            QdrantPayloadIndexMapper.ToPayloadIndexParams(RagPayloadIndexKind.Boolean).BoolIndexParams);
        Assert.IsType<DatetimeIndexParams>(
            QdrantPayloadIndexMapper.ToPayloadIndexParams(RagPayloadIndexKind.DateTime).DatetimeIndexParams);
        Assert.IsType<TextIndexParams>(
            QdrantPayloadIndexMapper.ToPayloadIndexParams(RagPayloadIndexKind.Text).TextIndexParams);
        Assert.IsType<UuidIndexParams>(
            QdrantPayloadIndexMapper.ToPayloadIndexParams(RagPayloadIndexKind.Uuid).UuidIndexParams);
    }
}
