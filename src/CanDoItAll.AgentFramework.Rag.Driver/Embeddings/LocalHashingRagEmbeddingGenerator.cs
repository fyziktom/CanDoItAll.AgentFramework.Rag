using System.Text;
using Microsoft.Extensions.Options;

namespace CanDoItAll.AgentFramework.Rag.Driver.Embeddings;

public sealed class LocalHashingRagEmbeddingGenerator : IRagEmbeddingGenerator
{
    public const string ProviderName = "local-hashing";

    private readonly LocalHashingRagEmbeddingOptions _options;

    public LocalHashingRagEmbeddingGenerator(IOptions<LocalHashingRagEmbeddingOptions> options)
        : this(options.Value)
    {
    }

    public LocalHashingRagEmbeddingGenerator(LocalHashingRagEmbeddingOptions? options = null)
    {
        _options = options ?? new LocalHashingRagEmbeddingOptions();
        _options.Validate();
    }

    public ValueTask<RagEmbedding> GenerateAsync(
        RagEmbeddingRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();
        request.Validate();

        var dimension = request.Dimensions ?? _options.Dimension;
        if (dimension <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(request), "Embedding dimensions must be greater than zero.");
        }

        var vector = new float[dimension];
        var tokenCount = 0;

        foreach (var token in Tokenize(request.Text))
        {
            tokenCount++;
            AddFeature(vector, $"tok:{token}", _options.TokenWeight);

            for (var length = _options.MinimumCharacterNGramLength; length <= _options.MaximumCharacterNGramLength; length++)
            {
                if (length <= 0 || token.Length < length)
                {
                    continue;
                }

                for (var index = 0; index <= token.Length - length; index++)
                {
                    AddFeature(vector, $"ng:{token.Substring(index, length)}", _options.CharacterNGramWeight);
                }
            }
        }

        if (tokenCount == 0)
        {
            AddFeature(vector, "empty", 1.0f);
        }

        NormalizeInPlace(vector);
        return ValueTask.FromResult(new RagEmbedding(request.Text, vector, ProviderName));
    }

    private static IEnumerable<string> Tokenize(string text)
    {
        var builder = new StringBuilder();

        foreach (var character in text.ToLowerInvariant())
        {
            if (char.IsLetterOrDigit(character))
            {
                builder.Append(character);
                continue;
            }

            if (builder.Length > 0)
            {
                yield return builder.ToString();
                builder.Clear();
            }
        }

        if (builder.Length > 0)
        {
            yield return builder.ToString();
        }
    }

    private static void AddFeature(float[] vector, string feature, float weight)
    {
        var hash = StableHash(feature);
        var index = (int)(hash % (uint)vector.Length);
        var sign = (hash & 0x80000000) == 0 ? 1.0f : -1.0f;
        vector[index] += sign * weight;
    }

    private static uint StableHash(string value)
    {
        const uint offsetBasis = 2166136261;
        const uint prime = 16777619;

        var hash = offsetBasis;
        foreach (var character in value)
        {
            hash ^= character;
            hash *= prime;
        }

        return hash;
    }

    private static void NormalizeInPlace(float[] vector)
    {
        var sumSquares = 0.0d;
        foreach (var value in vector)
        {
            sumSquares += value * value;
        }

        if (sumSquares <= 0)
        {
            return;
        }

        var length = Math.Sqrt(sumSquares);
        for (var index = 0; index < vector.Length; index++)
        {
            vector[index] = (float)(vector[index] / length);
        }
    }
}
