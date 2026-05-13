namespace CanDoItAll.AgentFramework.Rag.Driver.Models;

public sealed record RagSearchRequest
{
    public string? CollectionName { get; init; }

    public required string QueryText { get; init; }

    public float[]? Vector { get; init; }

    public int Limit { get; init; } = 5;

    public double? MinScore { get; init; }

    public void Validate(int? expectedVectorSize = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(QueryText);

        if (Limit <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(Limit), "Limit must be greater than zero.");
        }

        if (Vector is not null)
        {
            RagVectorValidation.EnsureVectorSize(Vector, expectedVectorSize, nameof(Vector));
        }
    }
}
