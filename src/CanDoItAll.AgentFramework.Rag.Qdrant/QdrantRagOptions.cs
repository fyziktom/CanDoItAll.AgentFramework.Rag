namespace CanDoItAll.AgentFramework.Rag.Qdrant;

public sealed class QdrantRagOptions
{
    public string Host { get; set; } = "localhost";

    public int Port { get; set; } = 6334;

    public bool Https { get; set; }

    public string? ApiKey { get; set; }

    public TimeSpan GrpcTimeout { get; set; } = TimeSpan.FromSeconds(30);

    public bool CreateCollectionIfMissing { get; set; } = true;

    public bool WaitForWrites { get; set; } = true;

    public void Validate()
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(Host);

        if (Port <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(Port), "Qdrant port must be greater than zero.");
        }

        if (GrpcTimeout < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(GrpcTimeout), "gRPC timeout cannot be negative.");
        }
    }
}
