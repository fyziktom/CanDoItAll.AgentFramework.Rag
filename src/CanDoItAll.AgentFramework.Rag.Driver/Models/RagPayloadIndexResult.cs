namespace CanDoItAll.AgentFramework.Rag.Driver.Models;

public sealed record RagPayloadIndexResult
{
    public string? CollectionName { get; init; }

    public required string FieldName { get; init; }

    public required RagPayloadIndexKind IndexKind { get; init; }

    public required RagPayloadIndexStatus Status { get; init; }
}

