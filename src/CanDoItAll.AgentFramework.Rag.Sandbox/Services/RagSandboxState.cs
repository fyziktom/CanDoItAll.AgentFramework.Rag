using CanDoItAll.AgentFramework.Rag.Driver.Models;

namespace CanDoItAll.AgentFramework.Rag.Sandbox.Services;

internal sealed class RagSandboxCollectionState
{
    public required string Name { get; set; }

    public string Description { get; set; } = string.Empty;

    public List<string> Tags { get; set; } = [];

    public required RagCollectionOptions Options { get; set; }

    public List<RagSandboxRecordState> Records { get; init; } = [];

    public required DateTimeOffset UpdatedAt { get; set; }
}

internal sealed class RagSandboxRecordState
{
    public required string Id { get; set; }

    public string Text { get; set; } = string.Empty;

    public string Metadata { get; set; } = string.Empty;

    public List<string> Tags { get; set; } = [];

    public float[]? Vector { get; set; }

    public required DateTimeOffset UpdatedAt { get; set; }
}
