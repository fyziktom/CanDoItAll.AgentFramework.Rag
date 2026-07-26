using CanDoItAll.AgentFramework.Rag.Qdrant;

namespace CanDoItAll.AgentFramework.Rag.Tests.Qdrant;

public sealed class QdrantPublicApiTests
{
    [Fact]
    public void Assembly_DoesNotExportMappingOrProviderImplementationTypes()
    {
        var exportedTypes = typeof(QdrantRagDriver).Assembly.GetExportedTypes();

        Assert.DoesNotContain(
            exportedTypes,
            type => string.Equals(
                type.Namespace,
                "CanDoItAll.AgentFramework.Rag.Qdrant.Mapping",
                StringComparison.Ordinal));
        Assert.DoesNotContain(
            exportedTypes,
            type => string.Equals(type.Name, "QdrantRagDriverProvider", StringComparison.Ordinal));
    }
}
