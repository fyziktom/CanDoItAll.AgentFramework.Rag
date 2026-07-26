using CanDoItAll.AgentFramework.Rag.Driver.Abstractions;
using CanDoItAll.AgentFramework.Rag.Driver.Models;
using Microsoft.Extensions.Options;

namespace CanDoItAll.AgentFramework.Rag.Driver.Factories;

public sealed class RagDriverFactory : IRagDriverFactory
{
    private readonly RagDriverFactoryOptions _defaultOptions;
    private readonly IReadOnlyDictionary<string, IRagDriverProvider> _providers;

    public RagDriverFactory(
        IOptions<RagDriverFactoryOptions> defaultOptions,
        IEnumerable<IRagDriverProvider> providers)
    {
        ArgumentNullException.ThrowIfNull(defaultOptions);
        ArgumentNullException.ThrowIfNull(providers);

        _defaultOptions = defaultOptions.Value;
        _defaultOptions.Validate();

        var providerCatalog = new Dictionary<string, IRagDriverProvider>(StringComparer.OrdinalIgnoreCase);
        foreach (var provider in providers)
        {
            ArgumentNullException.ThrowIfNull(provider);
            ArgumentException.ThrowIfNullOrWhiteSpace(provider.ProviderName);

            if (!providerCatalog.TryAdd(provider.ProviderName, provider))
            {
                throw new InvalidOperationException(
                    $"More than one RAG driver provider is registered for '{provider.ProviderName}'.");
            }
        }

        _providers = providerCatalog;
    }

    public IRagDriver Create(RagDriverFactoryOptions? options = null)
    {
        var effectiveOptions = options ?? _defaultOptions;
        effectiveOptions.Validate();

        if (!_providers.TryGetValue(effectiveOptions.ProviderName, out var provider))
        {
            throw new InvalidOperationException(
                $"No RAG driver provider is registered for '{effectiveOptions.ProviderName}'.");
        }

        return provider.Create(effectiveOptions);
    }
}
