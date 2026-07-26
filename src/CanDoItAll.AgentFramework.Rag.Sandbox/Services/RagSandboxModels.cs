using CanDoItAll.AgentFramework.Rag.Driver.Models;

namespace CanDoItAll.AgentFramework.Rag.Sandbox.Services;

public sealed record RagSandboxCollectionSummary(
    string Name,
    string Description,
    IReadOnlyList<string> Tags,
    int VectorSize,
    RagDistanceMetric Distance,
    int RecordCount,
    DateTimeOffset UpdatedAt);

public sealed record RagSandboxRecordSummary(
    string Id,
    string Text,
    string Metadata,
    IReadOnlyList<string> Tags,
    double? Score,
    DateTimeOffset UpdatedAt);

public sealed record RagSandboxSearchHit(
    string CollectionName,
    string RecordId,
    string Text,
    string Metadata,
    IReadOnlyList<string> Tags,
    double Score,
    DateTimeOffset UpdatedAt);

public sealed class RagSandboxCollectionEditModel
{
    public string? OriginalName { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public IReadOnlyList<string> Tags { get; set; } = Array.Empty<string>();

    public int VectorSize { get; set; } = 64;

    public RagDistanceMetric Distance { get; set; } = RagDistanceMetric.Cosine;

    public void Validate()
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(Name);
        ArgumentNullException.ThrowIfNull(Tags);

        if (VectorSize <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(VectorSize),
                "Vector size must be greater than zero.");
        }
    }
}

public sealed class RagSandboxRecordEditModel
{
    public string? OriginalId { get; set; }

    public string Id { get; set; } = string.Empty;

    public string Text { get; set; } = string.Empty;

    public string Metadata { get; set; } = string.Empty;

    public IReadOnlyList<string> Tags { get; set; } = Array.Empty<string>();

    public void Validate()
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(Id);
        ArgumentException.ThrowIfNullOrWhiteSpace(Text);
        ArgumentNullException.ThrowIfNull(Tags);
    }
}
