namespace CanDoItAll.AgentFramework.Rag.Driver.Models;

public sealed record RagDriverCapabilities
{
    public static RagDriverCapabilities None { get; } = new();

    public static RagDriverCapabilities WithTags { get; } = new()
    {
        SupportsTags = true
    };

    public bool SupportsTags { get; init; }
}
