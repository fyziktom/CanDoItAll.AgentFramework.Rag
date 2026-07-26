using CanDoItAll.AgentFramework.Rag.Driver.Models;

namespace CanDoItAll.AgentFramework.Rag.Driver.Abstractions;

public interface IRagDriverProvider
{
    string ProviderName { get; }

    IRagDriver Create(RagDriverFactoryOptions options);
}
