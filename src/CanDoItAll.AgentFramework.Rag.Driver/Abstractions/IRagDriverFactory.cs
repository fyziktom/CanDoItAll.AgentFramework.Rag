using CanDoItAll.AgentFramework.Rag.Driver.Models;

namespace CanDoItAll.AgentFramework.Rag.Driver.Abstractions;

public interface IRagDriverFactory
{
    IRagDriver Create(RagDriverFactoryOptions? options = null);
}
