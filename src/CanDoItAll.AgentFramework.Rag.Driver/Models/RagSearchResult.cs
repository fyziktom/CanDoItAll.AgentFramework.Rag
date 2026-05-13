namespace CanDoItAll.AgentFramework.Rag.Driver.Models;

public sealed record RagSearchResult
{
    public required RagKnowledgeEntry Knowledge { get; init; }

    public required double Score { get; init; }
}
