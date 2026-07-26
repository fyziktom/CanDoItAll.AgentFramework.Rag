using CanDoItAll.AgentFramework.Rag.Driver.Models;
using Qdrant.Client.Grpc;

namespace CanDoItAll.AgentFramework.Rag.Qdrant.Mapping;

internal static class QdrantCollectionMapper
{
    public static VectorParams ToVectorParams(RagCollectionOptions collection)
    {
        ArgumentNullException.ThrowIfNull(collection);
        collection.Validate();

        return new VectorParams
        {
            Size = (ulong)collection.VectorSize,
            Distance = ToQdrantDistance(collection.Distance)
        };
    }

    public static Distance ToQdrantDistance(RagDistanceMetric distance)
    {
        return distance switch
        {
            RagDistanceMetric.Cosine => Distance.Cosine,
            RagDistanceMetric.Dot => Distance.Dot,
            RagDistanceMetric.Euclidean => Distance.Euclid,
            RagDistanceMetric.Manhattan => Distance.Manhattan,
            _ => throw new ArgumentOutOfRangeException(nameof(distance), distance, "Unsupported distance metric.")
        };
    }
}
