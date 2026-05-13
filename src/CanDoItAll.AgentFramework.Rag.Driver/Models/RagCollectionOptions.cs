namespace CanDoItAll.AgentFramework.Rag.Driver.Models;

public sealed record RagCollectionOptions
{
    public string CollectionName { get; init; } = "candoitall-knowledge";

    public int VectorSize { get; init; } = 384;

    public RagDistanceMetric Distance { get; init; } = RagDistanceMetric.Cosine;

    public void Validate()
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(CollectionName);

        if (VectorSize <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(VectorSize), "Vector size must be greater than zero.");
        }
    }
}
