namespace CanDoItAll.AgentFramework.Rag.Driver.Models;

public sealed record RagDeleteRequest
{
    public string? CollectionName { get; init; }

    public required IReadOnlyList<string> KnowledgeIds { get; init; }

    public void Validate()
    {
        ArgumentNullException.ThrowIfNull(KnowledgeIds);

        if (KnowledgeIds.Count == 0)
        {
            throw new ArgumentException("At least one knowledge id is required.", nameof(KnowledgeIds));
        }

        foreach (var id in KnowledgeIds)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(id);
        }
    }
}
