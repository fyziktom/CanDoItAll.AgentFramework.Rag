namespace CanDoItAll.AgentFramework.Rag.Driver.Models;

public sealed record RagKnowledgeEntry
{
    public required string Id { get; init; }

    public required string Text { get; init; }

    public IReadOnlyDictionary<string, object?> Metadata { get; init; } = new Dictionary<string, object?>();

    public float[]? Vector { get; init; }

    public void Validate(int? expectedVectorSize = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(Id);
        ArgumentException.ThrowIfNullOrWhiteSpace(Text);
        ArgumentNullException.ThrowIfNull(Metadata);

        if (Vector is not null)
        {
            RagVectorValidation.EnsureVectorSize(Vector, expectedVectorSize, nameof(Vector));
        }
    }
}
