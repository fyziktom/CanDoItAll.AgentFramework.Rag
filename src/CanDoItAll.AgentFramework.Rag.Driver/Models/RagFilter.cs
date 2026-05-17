namespace CanDoItAll.AgentFramework.Rag.Driver.Models;

public abstract record RagFilter
{
    public void Validate()
    {
        ValidateCore();
    }

    internal abstract void ValidateCore();
}

