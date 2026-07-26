namespace CanDoItAll.AgentFramework.Rag.Driver.Models;

public sealed class RagDriverFactoryOptions
{
    public string ProviderName { get; set; } = string.Empty;

    public RagCollectionOptions DefaultCollection { get; set; } = new();

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(ProviderName))
        {
            throw new InvalidOperationException("A RAG driver provider name must be configured.");
        }

        ArgumentNullException.ThrowIfNull(DefaultCollection);
        DefaultCollection.Validate();
    }
}
