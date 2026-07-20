using CanDoItAll.AgentFramework.Rag.Qdrant;
using Qdrant.Client.Grpc;

namespace CanDoItAll.AgentFramework.Rag.Tests.Qdrant;

public sealed class QdrantRagWriteReceiptPolicyTests
{
    [Theory]
    [InlineData(UpdateStatus.Completed, true)]
    [InlineData(UpdateStatus.Completed, false)]
    [InlineData(UpdateStatus.Acknowledged, false)]
    public void EnsureAccepted_AcceptsValidReceipt(
        UpdateStatus status,
        bool waitedForCompletion)
    {
        QdrantRagWriteReceiptPolicy.EnsureAccepted(
            new UpdateResult { Status = status },
            "test operation",
            waitedForCompletion);
    }

    [Theory]
    [InlineData(UpdateStatus.Acknowledged, true)]
    [InlineData(UpdateStatus.UnknownUpdateStatus, true)]
    [InlineData(UpdateStatus.UnknownUpdateStatus, false)]
    public void EnsureAccepted_RejectsUnexpectedReceipt(
        UpdateStatus status,
        bool waitedForCompletion)
    {
        Assert.Throws<InvalidOperationException>(() =>
            QdrantRagWriteReceiptPolicy.EnsureAccepted(
                new UpdateResult { Status = status },
                "test operation",
                waitedForCompletion));
    }
}
