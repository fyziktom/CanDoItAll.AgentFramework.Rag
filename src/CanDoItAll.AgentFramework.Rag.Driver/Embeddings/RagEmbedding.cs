namespace CanDoItAll.AgentFramework.Rag.Driver.Embeddings;

public sealed record RagEmbedding
{
    public RagEmbedding(
        string sourceText,
        float[] vector,
        string providerName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceText);
        ArgumentNullException.ThrowIfNull(vector);
        ArgumentException.ThrowIfNullOrWhiteSpace(providerName);

        if (vector.Length == 0)
        {
            throw new ArgumentException("Embedding vector must not be empty.", nameof(vector));
        }

        SourceText = sourceText;
        Vector = vector;
        ProviderName = providerName;
    }

    public string SourceText { get; }

    public float[] Vector { get; }

    public string ProviderName { get; }

    public int Dimension => Vector.Length;
}
