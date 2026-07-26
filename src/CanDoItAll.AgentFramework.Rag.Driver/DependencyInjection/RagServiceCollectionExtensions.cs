using CanDoItAll.AgentFramework.Rag.Driver.Abstractions;
using CanDoItAll.AgentFramework.Rag.Driver.Embeddings;
using CanDoItAll.AgentFramework.Rag.Driver.Factories;
using CanDoItAll.AgentFramework.Rag.Driver.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CanDoItAll.AgentFramework.Rag.Driver.DependencyInjection;

public static class RagServiceCollectionExtensions
{
    public static IServiceCollection AddRagDriverCore(
        this IServiceCollection services,
        Action<RagDriverFactoryOptions>? configureFactory = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddOptions<RagDriverFactoryOptions>();

        if (configureFactory is not null)
        {
            services.Configure(configureFactory);
        }

        services.TryAddSingleton<IRagDriverFactory, RagDriverFactory>();
        return services;
    }

    public static IServiceCollection AddLocalHashingRagEmbeddingGenerator(
        this IServiceCollection services,
        Action<LocalHashingRagEmbeddingOptions>? configureEmbedding = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddOptions<LocalHashingRagEmbeddingOptions>();

        if (configureEmbedding is not null)
        {
            services.Configure(configureEmbedding);
        }

        services.TryAddSingleton<IRagEmbeddingGenerator>(serviceProvider =>
            new LocalHashingRagEmbeddingGenerator(
                serviceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<LocalHashingRagEmbeddingOptions>>()));
        return services;
    }
}
