namespace CanDoItAll.AgentFramework.Rag.Driver.Embeddings;

public sealed class DelegatingRagEmbeddingGenerator : IRagEmbeddingGenerator
{
    private readonly Func<RagEmbeddingRequest, CancellationToken, ValueTask<RagEmbedding>> _generate;

    public DelegatingRagEmbeddingGenerator(
        Func<RagEmbeddingRequest, CancellationToken, ValueTask<RagEmbedding>> generate)
    {
        _generate = generate ?? throw new ArgumentNullException(nameof(generate));
    }

    public ValueTask<RagEmbedding> GenerateAsync(
        RagEmbeddingRequest request,
        CancellationToken cancellationToken = default)
    {
        return _generate(request, cancellationToken);
    }
}
