using CanDoItAll.AgentFramework.Rag.Driver.DependencyInjection;
using CanDoItAll.AgentFramework.Rag.Sandbox.Services;
using Microsoft.Extensions.DependencyInjection;

namespace CanDoItAll.AgentFramework.Rag.Tests.Sandbox;

public sealed class RagSandboxCompositionTests
{
    [Fact]
    public void SandboxServices_ResolveStoreWithExplicitEmbeddingRegistration()
    {
        var services = new ServiceCollection();
        services.AddLocalHashingRagEmbeddingGenerator(options => options.Dimension = 384);
        services.AddSingleton<TimeProvider>(TimeProvider.System);
        services.AddSingleton<RagSandboxSimilarityCalculator>();
        services.AddScoped<RagSandboxStore>();

        using var serviceProvider = services.BuildServiceProvider();
        using var scope = serviceProvider.CreateScope();

        var store = scope.ServiceProvider.GetRequiredService<RagSandboxStore>();

        Assert.NotNull(store);
    }
}
