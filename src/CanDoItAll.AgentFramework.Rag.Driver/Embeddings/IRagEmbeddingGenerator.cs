namespace CanDoItAll.AgentFramework.Rag.Driver.Embeddings;

public interface IRagEmbeddingGenerator
{
    ValueTask<RagEmbedding> GenerateAsync(
        RagEmbeddingRequest request,
        CancellationToken cancellationToken = default);
}
