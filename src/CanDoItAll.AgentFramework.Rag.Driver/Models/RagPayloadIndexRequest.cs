namespace CanDoItAll.AgentFramework.Rag.Driver.Models;

public sealed record RagPayloadIndexRequest
{
    public string? CollectionName { get; init; }

    public required string FieldName { get; init; }

    public required RagPayloadIndexKind IndexKind { get; init; }

    public void Validate()
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(FieldName);

        if (IndexKind == RagPayloadIndexKind.Unknown || !Enum.IsDefined(IndexKind))
        {
            throw new ArgumentOutOfRangeException(nameof(IndexKind), IndexKind, "Unsupported payload index kind.");
        }
    }
}

