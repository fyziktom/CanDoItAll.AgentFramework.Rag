using CanDoItAll.AgentFramework.Rag.Driver.Abstractions;
using CanDoItAll.AgentFramework.Rag.Driver.DependencyInjection;
using CanDoItAll.AgentFramework.Rag.Driver.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Qdrant.Client;

namespace CanDoItAll.AgentFramework.Rag.Qdrant.DependencyInjection;

public static class QdrantRagServiceCollectionExtensions
{
    public static IServiceCollection AddQdrantRagDriver(
        this IServiceCollection services,
        Action<QdrantRagOptions>? configureQdrant = null,
        Action<RagDriverFactoryOptions>? configureFactory = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddRagDriverCore(
            options =>
            {
                options.ProviderName = QdrantRagDriver.ProviderIdentifier;
                configureFactory?.Invoke(options);
            });

        if (configureQdrant is not null)
        {
            services.Configure(configureQdrant);
        }

        services.TryAddSingleton(serviceProvider =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<QdrantRagOptions>>().Value;
            options.Validate();
            return new QdrantClient(
                options.Host,
                options.Port,
                options.Https,
                options.ApiKey,
                options.GrpcTimeout);
        });

        services.TryAddEnumerable(ServiceDescriptor.Singleton<IRagDriverProvider, QdrantRagDriverProvider>());
        services.TryAddSingleton<IRagDriver>(serviceProvider =>
            serviceProvider.GetRequiredService<IRagDriverFactory>().Create());

        return services;
    }
}
