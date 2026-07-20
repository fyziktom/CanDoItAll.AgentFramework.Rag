using Qdrant.Client.Grpc;

namespace CanDoItAll.AgentFramework.Rag.Qdrant;

internal static class QdrantRagWriteReceiptPolicy
{
    public static void EnsureAccepted(
        UpdateResult result,
        string operation,
        bool waitedForCompletion)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentException.ThrowIfNullOrWhiteSpace(operation);
        var accepted = result.Status == UpdateStatus.Completed ||
            (!waitedForCompletion && result.Status == UpdateStatus.Acknowledged);
        if (!accepted)
        {
            throw new InvalidOperationException(
                $"Qdrant did not accept '{operation}'; status was '{result.Status}'.");
        }
    }
}
