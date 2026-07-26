using CanDoItAll.AgentFramework.Rag.Driver.Models;
using CanDoItAll.AgentFramework.Rag.Qdrant.Mapping;
using Qdrant.Client.Grpc;

namespace CanDoItAll.AgentFramework.Rag.Tests.Qdrant;

public sealed class QdrantCollectionMapperTests
{
    [Fact]
    public void ToVectorParams_MapsGenericCollectionConfiguration()
    {
        var options = new RagCollectionOptions
        {
            CollectionName = "knowledge",
            VectorSize = 12,
            Distance = RagDistanceMetric.Cosine
        };

        var vectorParams = QdrantCollectionMapper.ToVectorParams(options);

        Assert.Equal(12ul, vectorParams.Size);
        Assert.Equal(Distance.Cosine, vectorParams.Distance);
    }

    [Theory]
    [InlineData(RagDistanceMetric.Cosine, Distance.Cosine)]
    [InlineData(RagDistanceMetric.Dot, Distance.Dot)]
    [InlineData(RagDistanceMetric.Euclidean, Distance.Euclid)]
    [InlineData(RagDistanceMetric.Manhattan, Distance.Manhattan)]
    public void ToQdrantDistance_MapsSupportedMetrics(
        RagDistanceMetric source,
        Distance expected)
    {
        Assert.Equal(expected, QdrantCollectionMapper.ToQdrantDistance(source));
    }

    [Fact]
    public void ToQdrantDistance_UnsupportedMetric_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => QdrantCollectionMapper.ToQdrantDistance((RagDistanceMetric)(-1)));
    }
}
