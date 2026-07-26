using CanDoItAll.AgentFramework.Rag.Qdrant.Mapping;

namespace CanDoItAll.AgentFramework.Rag.Tests.Qdrant;

public sealed class QdrantPointIdMapperTests
{
    [Fact]
    public void ToPointId_ReturnsStableIdsForStringKnowledgeIds()
    {
        var first = QdrantPointIdMapper.ToPointId("knowledge:invoice-approval");
        var second = QdrantPointIdMapper.ToPointId("knowledge:invoice-approval");
        var different = QdrantPointIdMapper.ToPointId("knowledge:payment-terms");

        Assert.Equal(first, second);
        Assert.NotEqual(first, different);
    }

    [Fact]
    public void ToPointId_PreservesGuidAndNumericIdentifiers()
    {
        var guid = Guid.Parse("f26b6f82-7ecf-4f70-88f6-f9464bb04ce9");

        Assert.Equal(guid.ToString(), QdrantPointIdMapper.ToPointId(guid.ToString()).Uuid);
        Assert.Equal(42ul, QdrantPointIdMapper.ToPointId("42").Num);
    }
}
