namespace CanDoItAll.AgentFramework.Rag.Driver.Models;

public sealed record RagDeleteByFilterRequest
{
    public string? CollectionName { get; init; }

    public required RagFilter Filter { get; init; }

    public void Validate()
    {
        ArgumentNullException.ThrowIfNull(Filter);
        Filter.Validate();
    }
}

