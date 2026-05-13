namespace CanDoItAll.AgentFramework.Rag.Driver.Models;

public sealed class RagDriverFactoryOptions
{
    public string ProviderName { get; set; } = RagDriverProviderNames.Qdrant;

    public RagCollectionOptions DefaultCollection { get; set; } = new();

    public void Validate()
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(ProviderName);
        ArgumentNullException.ThrowIfNull(DefaultCollection);
        DefaultCollection.Validate();
    }
}
