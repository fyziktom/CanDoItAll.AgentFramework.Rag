using CanDoItAll.AgentFramework.Rag.Driver.Abstractions;
using CanDoItAll.AgentFramework.Rag.Driver.Models;
using Microsoft.Extensions.Options;

namespace CanDoItAll.AgentFramework.Rag.Driver.Factories;

public sealed class RagDriverFactory : IRagDriverFactory
{
    private readonly RagDriverFactoryOptions _defaultOptions;
    private readonly IReadOnlyList<IRagDriverProvider> _providers;
    private readonly IServiceProvider _serviceProvider;

    public RagDriverFactory(
        IOptions<RagDriverFactoryOptions> defaultOptions,
        IEnumerable<IRagDriverProvider> providers,
        IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(defaultOptions);
        ArgumentNullException.ThrowIfNull(providers);
        ArgumentNullException.ThrowIfNull(serviceProvider);

        _defaultOptions = defaultOptions.Value;
        _defaultOptions.Validate();
        _providers = providers.ToArray();
        _serviceProvider = serviceProvider;
    }

    public IRagDriver Create(RagDriverFactoryOptions? options = null)
    {
        var effectiveOptions = options ?? _defaultOptions;
        effectiveOptions.Validate();

        var provider = _providers.FirstOrDefault(candidate =>
            string.Equals(candidate.ProviderName, effectiveOptions.ProviderName, StringComparison.OrdinalIgnoreCase));

        if (provider is null)
        {
            throw new InvalidOperationException(
                $"No RAG driver provider is registered for '{effectiveOptions.ProviderName}'.");
        }

        return provider.Create(effectiveOptions, _serviceProvider);
    }
}
