namespace CanDoItAll.AgentFramework.Rag.Driver.Models;

public sealed record RagDriverCapabilities
{
    public static RagDriverCapabilities None { get; } = new();

    public static RagDriverCapabilities WithTags { get; } = new()
    {
        SupportsTags = true
    };

    public static RagDriverCapabilities WithTagsAndProjectionControls { get; } = new()
    {
        SupportsTags = true,
        SupportsFilters = true,
        SupportsPayloadIndexes = true,
        SupportsDeleteByFilter = true
    };

    public bool SupportsTags { get; init; }

    public bool SupportsFilters { get; init; }

    public bool SupportsPayloadIndexes { get; init; }

    public bool SupportsDeleteByFilter { get; init; }

    public bool SupportsNamedVectors { get; init; }
}
