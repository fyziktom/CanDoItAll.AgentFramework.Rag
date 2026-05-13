namespace CanDoItAll.AgentFramework.Rag.Driver.Embeddings;

public sealed record RagEmbeddingRequest(string Text, int? Dimensions = null)
{
    public void Validate()
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(Text);

        if (Dimensions <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(Dimensions), "Dimensions must be greater than zero.");
        }
    }
}
