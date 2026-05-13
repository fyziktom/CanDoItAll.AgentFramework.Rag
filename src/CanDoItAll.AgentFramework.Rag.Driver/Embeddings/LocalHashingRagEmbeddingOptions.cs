namespace CanDoItAll.AgentFramework.Rag.Driver.Embeddings;

public sealed class LocalHashingRagEmbeddingOptions
{
    public int Dimension { get; set; } = 384;

    public int MinimumCharacterNGramLength { get; set; } = 3;

    public int MaximumCharacterNGramLength { get; set; } = 5;

    public float TokenWeight { get; set; } = 1.0f;

    public float CharacterNGramWeight { get; set; } = 0.25f;

    public void Validate()
    {
        if (Dimension <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(Dimension), "Embedding dimension must be greater than zero.");
        }

        if (MinimumCharacterNGramLength <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(MinimumCharacterNGramLength),
                "Minimum character n-gram length must be greater than zero.");
        }

        if (MaximumCharacterNGramLength < MinimumCharacterNGramLength)
        {
            throw new ArgumentOutOfRangeException(
                nameof(MaximumCharacterNGramLength),
                "Maximum character n-gram length must be greater than or equal to the minimum.");
        }
    }
}
