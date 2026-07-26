namespace CanDoItAll.AgentFramework.Rag.Sandbox.Services;

internal static class RagSandboxProjection
{
    public static RagSandboxCollectionSummary ToSummary(RagSandboxCollectionState collection)
    {
        return new RagSandboxCollectionSummary(
            collection.Name,
            collection.Description,
            collection.Tags.ToArray(),
            collection.Options.VectorSize,
            collection.Options.Distance,
            collection.Records.Count,
            collection.UpdatedAt);
    }

    public static RagSandboxRecordSummary ToRecordSummary(
        RagSandboxRecordState record,
        double? score)
    {
        return new RagSandboxRecordSummary(
            record.Id,
            record.Text,
            record.Metadata,
            record.Tags.ToArray(),
            score,
            record.UpdatedAt);
    }

    public static RagSandboxSearchHit ToSearchHit(
        RagSandboxCollectionState collection,
        RagSandboxRecordState record,
        double score)
    {
        return new RagSandboxSearchHit(
            collection.Name,
            record.Id,
            record.Text,
            record.Metadata,
            record.Tags.ToArray(),
            score,
            record.UpdatedAt);
    }
}
