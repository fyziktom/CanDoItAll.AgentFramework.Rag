using CanDoItAll.AgentFramework.Rag.Driver.Embeddings;
using CanDoItAll.AgentFramework.Rag.Driver.Models;

namespace CanDoItAll.AgentFramework.Rag.Sandbox.Services;

public sealed class RagSandboxStore
{
    private readonly LocalHashingRagEmbeddingGenerator embeddingGenerator;
    private readonly List<RagSandboxCollectionState> collections = [];

    public RagSandboxStore()
    {
        embeddingGenerator = new LocalHashingRagEmbeddingGenerator(new LocalHashingRagEmbeddingOptions
        {
            Dimension = 384
        });

        Seed();
    }

    public IReadOnlyList<RagSandboxCollectionSummary> SearchCollections(string? query)
    {
        var normalizedQuery = Normalize(query);
        return collections
            .Where(collection => string.IsNullOrWhiteSpace(normalizedQuery)
                || Normalize(collection.Name).Contains(normalizedQuery, StringComparison.Ordinal)
                || Normalize(collection.Description).Contains(normalizedQuery, StringComparison.Ordinal))
            .OrderBy(collection => collection.Name, StringComparer.OrdinalIgnoreCase)
            .Select(ToSummary)
            .ToArray();
    }

    public RagSandboxCollectionSummary? GetCollection(string collectionName)
    {
        return collections
            .Where(collection => string.Equals(collection.Name, collectionName, StringComparison.OrdinalIgnoreCase))
            .Select(ToSummary)
            .FirstOrDefault();
    }

    public async ValueTask<RagSandboxCollectionSummary> SaveCollectionAsync(
        RagSandboxCollectionEditModel edit,
        CancellationToken cancellationToken = default)
    {
        edit.Validate();
        var existing = collections.FirstOrDefault(collection =>
            string.Equals(collection.Name, edit.Name, StringComparison.OrdinalIgnoreCase));

        if (existing is null)
        {
            existing = new RagSandboxCollectionState
            {
                Name = edit.Name.Trim(),
                Description = edit.Description.Trim(),
                Options = new RagCollectionOptions
                {
                    CollectionName = edit.Name.Trim(),
                    VectorSize = edit.VectorSize,
                    Distance = edit.Distance
                }
            };
            collections.Add(existing);
        }
        else
        {
            existing.Description = edit.Description.Trim();
            existing.Options = existing.Options with
            {
                VectorSize = edit.VectorSize,
                Distance = edit.Distance
            };
        }

        existing.UpdatedAt = DateTimeOffset.Now;
        await RebuildVectorsAsync(existing, cancellationToken).ConfigureAwait(false);
        return ToSummary(existing);
    }

    public bool DeleteCollection(string collectionName)
    {
        var collection = FindCollection(collectionName);
        if (collection is null)
        {
            return false;
        }

        collections.Remove(collection);
        return true;
    }

    public IReadOnlyList<RagSandboxRecordSummary> GetRecords(string collectionName)
    {
        var collection = FindCollection(collectionName);
        if (collection is null)
        {
            return [];
        }

        return collection.Records
            .OrderBy(record => record.Id, StringComparer.OrdinalIgnoreCase)
            .Select(record => ToRecordSummary(record, score: null))
            .ToArray();
    }

    public RagSandboxRecordSummary? GetRecord(string collectionName, string recordId)
    {
        var collection = FindCollection(collectionName);
        var record = collection?.Records.FirstOrDefault(candidate =>
            string.Equals(candidate.Id, recordId, StringComparison.OrdinalIgnoreCase));

        return record is null ? null : ToRecordSummary(record, score: null);
    }

    public async ValueTask<RagSandboxRecordSummary> SaveRecordAsync(
        string collectionName,
        RagSandboxRecordEditModel edit,
        CancellationToken cancellationToken = default)
    {
        edit.Validate();
        var collection = FindCollection(collectionName)
            ?? throw new InvalidOperationException($"Collection '{collectionName}' was not found.");

        var existing = collection.Records.FirstOrDefault(record =>
            string.Equals(record.Id, edit.Id, StringComparison.OrdinalIgnoreCase));

        if (existing is null)
        {
            existing = new RagSandboxRecordState
            {
                Id = edit.Id.Trim()
            };
            collection.Records.Add(existing);
        }

        existing.Text = edit.Text.Trim();
        existing.Metadata = edit.Metadata.Trim();
        existing.UpdatedAt = DateTimeOffset.Now;
        existing.Vector = await GenerateVectorAsync(collection, existing.Text, cancellationToken).ConfigureAwait(false);
        collection.UpdatedAt = DateTimeOffset.Now;

        return ToRecordSummary(existing, score: null);
    }

    public bool DeleteRecord(string collectionName, string recordId)
    {
        var collection = FindCollection(collectionName);
        var record = collection?.Records.FirstOrDefault(candidate =>
            string.Equals(candidate.Id, recordId, StringComparison.OrdinalIgnoreCase));

        if (collection is null || record is null)
        {
            return false;
        }

        collection.Records.Remove(record);
        collection.UpdatedAt = DateTimeOffset.Now;
        return true;
    }

    public async ValueTask<IReadOnlyList<RagSandboxRecordSummary>> SearchRecordsAsync(
        string collectionName,
        string? query,
        int limit,
        CancellationToken cancellationToken = default)
    {
        var collection = FindCollection(collectionName);
        if (collection is null)
        {
            return [];
        }

        if (string.IsNullOrWhiteSpace(query))
        {
            return GetRecords(collectionName);
        }

        var vector = await GenerateVectorAsync(collection, query, cancellationToken).ConfigureAwait(false);
        return collection.Records
            .Select(record => ToRecordSummary(record, CosineSimilarity(vector, record.Vector)))
            .OrderByDescending(record => record.Score ?? 0)
            .ThenBy(record => record.Id, StringComparer.OrdinalIgnoreCase)
            .Take(Math.Clamp(limit, 1, 50))
            .ToArray();
    }

    private RagSandboxCollectionState? FindCollection(string collectionName)
    {
        return collections.FirstOrDefault(collection =>
            string.Equals(collection.Name, collectionName, StringComparison.OrdinalIgnoreCase));
    }

    private async ValueTask RebuildVectorsAsync(
        RagSandboxCollectionState collection,
        CancellationToken cancellationToken)
    {
        foreach (var record in collection.Records)
        {
            record.Vector = await GenerateVectorAsync(collection, record.Text, cancellationToken).ConfigureAwait(false);
        }
    }

    private async ValueTask<float[]> GenerateVectorAsync(
        RagSandboxCollectionState collection,
        string text,
        CancellationToken cancellationToken)
    {
        var embedding = await embeddingGenerator
            .GenerateAsync(new RagEmbeddingRequest(text, collection.Options.VectorSize), cancellationToken)
            .ConfigureAwait(false);

        return embedding.Vector;
    }

    private void Seed()
    {
        collections.Add(new RagSandboxCollectionState
        {
            Name = "finance-policies",
            Description = "Operational policies used when answering invoice and vendor questions.",
            Options = new RagCollectionOptions
            {
                CollectionName = "finance-policies",
                VectorSize = 64,
                Distance = RagDistanceMetric.Cosine
            },
            Records =
            {
                new RagSandboxRecordState
                {
                    Id = "invoice-approval",
                    Text = "Invoices over 5000 require manager approval before payment.",
                    Metadata = "source=finance-policy; owner=accounts-payable"
                },
                new RagSandboxRecordState
                {
                    Id = "vendor-terms",
                    Text = "Standard vendor payment terms are net 30 unless the contract overrides them.",
                    Metadata = "source=vendor-policy; owner=procurement"
                }
            }
        });

        collections.Add(new RagSandboxCollectionState
        {
            Name = "support-runbooks",
            Description = "Support knowledge for triage, escalation, and service recovery.",
            Options = new RagCollectionOptions
            {
                CollectionName = "support-runbooks",
                VectorSize = 64,
                Distance = RagDistanceMetric.Cosine
            },
            Records =
            {
                new RagSandboxRecordState
                {
                    Id = "qdrant-grpc-port",
                    Text = "Qdrant .NET client connects through gRPC and expects port 6334 to be reachable.",
                    Metadata = "source=rag-sandbox; service=qdrant"
                }
            }
        });

        foreach (var collection in collections)
        {
            RebuildVectorsAsync(collection, CancellationToken.None).AsTask().GetAwaiter().GetResult();
        }
    }

    private static RagSandboxCollectionSummary ToSummary(RagSandboxCollectionState collection)
    {
        return new RagSandboxCollectionSummary(
            collection.Name,
            collection.Description,
            collection.Options.VectorSize,
            collection.Options.Distance,
            collection.Records.Count,
            collection.UpdatedAt);
    }

    private static RagSandboxRecordSummary ToRecordSummary(
        RagSandboxRecordState record,
        double? score)
    {
        return new RagSandboxRecordSummary(
            record.Id,
            record.Text,
            record.Metadata,
            score,
            record.UpdatedAt);
    }

    private static double CosineSimilarity(IReadOnlyList<float> left, IReadOnlyList<float> right)
    {
        var length = Math.Min(left.Count, right.Count);
        if (length == 0)
        {
            return 0;
        }

        var dot = 0.0d;
        var leftMagnitude = 0.0d;
        var rightMagnitude = 0.0d;

        for (var index = 0; index < length; index++)
        {
            dot += left[index] * right[index];
            leftMagnitude += left[index] * left[index];
            rightMagnitude += right[index] * right[index];
        }

        if (leftMagnitude <= 0 || rightMagnitude <= 0)
        {
            return 0;
        }

        return dot / (Math.Sqrt(leftMagnitude) * Math.Sqrt(rightMagnitude));
    }

    private static string Normalize(string? value)
        => (value ?? string.Empty).Trim().ToLowerInvariant();

    private sealed class RagSandboxCollectionState
    {
        public required string Name { get; set; }

        public string Description { get; set; } = string.Empty;

        public required RagCollectionOptions Options { get; set; }

        public List<RagSandboxRecordState> Records { get; init; } = [];

        public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.Now;
    }

    private sealed class RagSandboxRecordState
    {
        public required string Id { get; set; }

        public string Text { get; set; } = string.Empty;

        public string Metadata { get; set; } = string.Empty;

        public float[] Vector { get; set; } = [];

        public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.Now;
    }
}

public sealed record RagSandboxCollectionSummary(
    string Name,
    string Description,
    int VectorSize,
    RagDistanceMetric Distance,
    int RecordCount,
    DateTimeOffset UpdatedAt);

public sealed record RagSandboxRecordSummary(
    string Id,
    string Text,
    string Metadata,
    double? Score,
    DateTimeOffset UpdatedAt);

public sealed class RagSandboxCollectionEditModel
{
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public int VectorSize { get; set; } = 64;

    public RagDistanceMetric Distance { get; set; } = RagDistanceMetric.Cosine;

    public void Validate()
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(Name);

        if (VectorSize <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(VectorSize), "Vector size must be greater than zero.");
        }
    }
}

public sealed class RagSandboxRecordEditModel
{
    public string Id { get; set; } = string.Empty;

    public string Text { get; set; } = string.Empty;

    public string Metadata { get; set; } = string.Empty;

    public void Validate()
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(Id);
        ArgumentException.ThrowIfNullOrWhiteSpace(Text);
    }
}
