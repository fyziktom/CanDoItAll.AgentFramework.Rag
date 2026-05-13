namespace CanDoItAll.AgentFramework.Rag.Driver.Models;

public sealed record RagKnowledgeEntry
{
    public required string Id { get; init; }

    public required string Text { get; init; }

    public IReadOnlyDictionary<string, object?> Metadata { get; init; } = new Dictionary<string, object?>();

    public IReadOnlyList<string> Tags { get; init; } = Array.Empty<string>();

    public float[]? Vector { get; init; }

    public void Validate(int? expectedVectorSize = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(Id);
        ArgumentException.ThrowIfNullOrWhiteSpace(Text);
        ArgumentNullException.ThrowIfNull(Metadata);
        ArgumentNullException.ThrowIfNull(Tags);

        if (Tags.Any(string.IsNullOrWhiteSpace))
        {
            throw new ArgumentException("Tags cannot contain empty values.", nameof(Tags));
        }

        if (Tags.Distinct(StringComparer.OrdinalIgnoreCase).Count() != Tags.Count)
        {
            throw new ArgumentException("Tags cannot contain duplicate values.", nameof(Tags));
        }

        if (Vector is not null)
        {
            RagVectorValidation.EnsureVectorSize(Vector, expectedVectorSize, nameof(Vector));
        }
    }
}
