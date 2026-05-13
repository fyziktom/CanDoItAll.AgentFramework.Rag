using CanDoItAll.AgentFramework.Rag.Driver.Abstractions;
using CanDoItAll.AgentFramework.Rag.Driver.Models;
using CanDoItAll.AgentFramework.Rag.Qdrant.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

var collectionName = ReadValue("RAG_COLLECTION", "candoitall-knowledge-sample");
var vectorSize = ReadInt32("RAG_VECTOR_SIZE", 64);
var qdrantHost = ReadValue("QDRANT_HOST", "localhost");
var qdrantPort = ReadInt32("QDRANT_GRPC_PORT", 6334);
var qdrantApiKey = Environment.GetEnvironmentVariable("QDRANT_API_KEY");
var dryRun = args.Any(argument => string.Equals(argument, "--dry-run", StringComparison.OrdinalIgnoreCase));

var services = new ServiceCollection();
services.AddQdrantRagDriver(
    configureQdrant: options =>
    {
        options.Host = qdrantHost;
        options.Port = qdrantPort;
        options.ApiKey = string.IsNullOrWhiteSpace(qdrantApiKey) ? null : qdrantApiKey;
    },
    configureFactory: options =>
    {
        options.DefaultCollection = new RagCollectionOptions
        {
            CollectionName = collectionName,
            VectorSize = vectorSize,
            Distance = RagDistanceMetric.Cosine
        };
    },
    configureEmbedding: options => options.Dimension = vectorSize);

using var provider = services.BuildServiceProvider();
var driver = provider.GetRequiredService<IRagDriverFactory>().Create();

Console.WriteLine($"Provider: {driver.ProviderName}");
Console.WriteLine($"Collection: {driver.DefaultCollection.CollectionName}");
Console.WriteLine($"Qdrant gRPC endpoint: {qdrantHost}:{qdrantPort}");

if (dryRun)
{
    Console.WriteLine("Dry run completed. Remove --dry-run to call Qdrant.");
    return 0;
}

try
{
    await driver.EnsureCollectionAsync();

    await driver.UpsertAsync(new RagUpsertRequest
    {
        Entries = new[]
        {
            new RagKnowledgeEntry
            {
                Id = "knowledge:invoice-approval",
                Text = "Invoices over 5000 require manager approval before payment.",
                Metadata = new Dictionary<string, object?>
                {
                    ["source"] = "finance-policy",
                    ["category"] = "approval"
                }
            },
            new RagKnowledgeEntry
            {
                Id = "knowledge:payment-terms",
                Text = "Standard vendor payment terms are net 30 unless the contract overrides them.",
                Metadata = new Dictionary<string, object?>
                {
                    ["source"] = "vendor-policy",
                    ["category"] = "payment"
                }
            }
        }
    });

    var results = await driver.SearchAsync(new RagSearchRequest
    {
        QueryText = "Which invoices need approval?",
        Limit = 3
    });

    foreach (var result in results)
    {
        Console.WriteLine($"{result.Score:0.000} {result.Knowledge.Id}: {result.Knowledge.Text}");
    }

    return 0;
}
catch (Exception exception)
{
    Console.Error.WriteLine("Sample failed while calling Qdrant.");
    Console.Error.WriteLine(exception.Message);
    Console.Error.WriteLine("Check that Qdrant is running and exposes gRPC on the configured port.");
    return 1;
}

static string ReadValue(string name, string fallback)
{
    var value = Environment.GetEnvironmentVariable(name);
    return string.IsNullOrWhiteSpace(value) ? fallback : value;
}

static int ReadInt32(string name, int fallback)
{
    var value = Environment.GetEnvironmentVariable(name);
    return int.TryParse(value, out var parsed) && parsed > 0 ? parsed : fallback;
}
