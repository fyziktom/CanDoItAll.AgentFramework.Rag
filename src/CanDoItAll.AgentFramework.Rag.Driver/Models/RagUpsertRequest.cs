namespace CanDoItAll.AgentFramework.Rag.Driver.Models;

public sealed record RagUpsertRequest
{
    public string? CollectionName { get; init; }

    public required IReadOnlyList<RagKnowledgeEntry> Entries { get; init; }

    public void Validate(int? expectedVectorSize = null)
    {
        ArgumentNullException.ThrowIfNull(Entries);

        if (Entries.Count == 0)
        {
            throw new ArgumentException("At least one knowledge entry is required.", nameof(Entries));
        }

        foreach (var entry in Entries)
        {
            entry.Validate(expectedVectorSize);
        }
    }
}
