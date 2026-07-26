using CanDoItAll.AgentFramework.Rag.Driver.Embeddings;
using CanDoItAll.AgentFramework.Rag.Driver.Models;

namespace CanDoItAll.AgentFramework.Rag.Sandbox.Services;

public sealed class RagSandboxStore
{
    private readonly IRagEmbeddingGenerator embeddingGenerator;
    private readonly RagSandboxSimilarityCalculator similarityCalculator;
    private readonly TimeProvider timeProvider;
    private readonly List<RagSandboxCollectionState> collections;
    private readonly SemaphoreSlim vectorGate = new(1, 1);

    public RagSandboxStore(
        IRagEmbeddingGenerator embeddingGenerator,
        RagSandboxSimilarityCalculator similarityCalculator,
        TimeProvider timeProvider)
    {
        this.embeddingGenerator = embeddingGenerator
            ?? throw new ArgumentNullException(nameof(embeddingGenerator));
        this.similarityCalculator = similarityCalculator
            ?? throw new ArgumentNullException(nameof(similarityCalculator));
        this.timeProvider = timeProvider
            ?? throw new ArgumentNullException(nameof(timeProvider));
        collections = RagSandboxSeedData.Create(timeProvider.GetUtcNow());
    }

    public RagDriverCapabilities Capabilities { get; } = RagDriverCapabilities.WithTags;

    public bool SupportsRecordTags => Capabilities.SupportsTags;

    public IReadOnlyList<RagSandboxCollectionSummary> SearchCollections(string? query)
    {
        var normalizedQuery = Normalize(query);
        return collections
            .Where(collection => string.IsNullOrWhiteSpace(normalizedQuery)
                || Normalize(collection.Name).Contains(normalizedQuery, StringComparison.Ordinal)
                || Normalize(collection.Description).Contains(normalizedQuery, StringComparison.Ordinal)
                || collection.Tags.Any(tag => Normalize(tag).Contains(normalizedQuery, StringComparison.Ordinal)))
            .OrderBy(collection => collection.Name, StringComparer.OrdinalIgnoreCase)
            .Select(RagSandboxProjection.ToSummary)
            .ToArray();
    }

    public RagSandboxCollectionSummary? GetCollection(string collectionName)
    {
        return collections
            .Where(collection => string.Equals(collection.Name, collectionName, StringComparison.OrdinalIgnoreCase))
            .Select(RagSandboxProjection.ToSummary)
            .FirstOrDefault();
    }

    public ValueTask<RagSandboxCollectionSummary> SaveCollectionAsync(
        RagSandboxCollectionEditModel edit,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(edit);
        cancellationToken.ThrowIfCancellationRequested();
        edit.Validate();

        var normalizedName = edit.Name.Trim();
        var originalName = string.IsNullOrWhiteSpace(edit.OriginalName)
            ? normalizedName
            : edit.OriginalName.Trim();

        var existing = collections.FirstOrDefault(collection =>
            string.Equals(collection.Name, originalName, StringComparison.OrdinalIgnoreCase));

        if (!string.Equals(originalName, normalizedName, StringComparison.OrdinalIgnoreCase) &&
            collections.Any(collection => string.Equals(collection.Name, normalizedName, StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException($"Collection '{normalizedName}' already exists.");
        }

        if (existing is null)
        {
            existing = new RagSandboxCollectionState
            {
                Name = normalizedName,
                Description = edit.Description.Trim(),
                Tags = edit.Tags.ToList(),
                Options = new RagCollectionOptions
                {
                    CollectionName = normalizedName,
                    VectorSize = edit.VectorSize,
                    Distance = edit.Distance
                },
                UpdatedAt = timeProvider.GetUtcNow()
            };
            collections.Add(existing);
        }
        else
        {
            var vectorSizeChanged = existing.Options.VectorSize != edit.VectorSize;
            existing.Name = normalizedName;
            existing.Description = edit.Description.Trim();
            existing.Tags = edit.Tags.ToList();
            existing.Options = existing.Options with
            {
                CollectionName = normalizedName,
                VectorSize = edit.VectorSize,
                Distance = edit.Distance
            };

            if (vectorSizeChanged)
            {
                InvalidateVectors(existing);
            }

            existing.UpdatedAt = timeProvider.GetUtcNow();
        }

        return ValueTask.FromResult(RagSandboxProjection.ToSummary(existing));
    }

    public bool DeleteCollection(string collectionName)
    {
        var collection = FindCollection(collectionName);
        return collection is not null && collections.Remove(collection);
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
            .Select(record => RagSandboxProjection.ToRecordSummary(record, score: null))
            .ToArray();
    }

    public RagSandboxRecordSummary? GetRecord(string collectionName, string recordId)
    {
        var collection = FindCollection(collectionName);
        var record = collection?.Records.FirstOrDefault(candidate =>
            string.Equals(candidate.Id, recordId, StringComparison.OrdinalIgnoreCase));

        return record is null
            ? null
            : RagSandboxProjection.ToRecordSummary(record, score: null);
    }

    public ValueTask<RagSandboxRecordSummary> SaveRecordAsync(
        string collectionName,
        RagSandboxRecordEditModel edit,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(edit);
        cancellationToken.ThrowIfCancellationRequested();
        edit.Validate();

        var collection = FindCollection(collectionName)
            ?? throw new InvalidOperationException($"Collection '{collectionName}' was not found.");

        var normalizedId = edit.Id.Trim();
        var originalId = string.IsNullOrWhiteSpace(edit.OriginalId)
            ? normalizedId
            : edit.OriginalId.Trim();
        var existing = collection.Records.FirstOrDefault(record =>
            string.Equals(record.Id, originalId, StringComparison.OrdinalIgnoreCase));

        if (edit.Tags.Count > 0 && !SupportsRecordTags)
        {
            throw new NotSupportedException("The selected RAG driver does not support record tags.");
        }

        if (!string.Equals(originalId, normalizedId, StringComparison.OrdinalIgnoreCase) &&
            collection.Records.Any(record => string.Equals(record.Id, normalizedId, StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException($"Record '{normalizedId}' already exists in '{collectionName}'.");
        }

        if (existing is null)
        {
            existing = new RagSandboxRecordState
            {
                Id = normalizedId,
                UpdatedAt = timeProvider.GetUtcNow()
            };
            collection.Records.Add(existing);
        }

        existing.Id = normalizedId;
        existing.Text = edit.Text.Trim();
        existing.Metadata = edit.Metadata.Trim();
        existing.Tags = edit.Tags.ToList();
        existing.UpdatedAt = timeProvider.GetUtcNow();
        existing.Vector = null;
        collection.UpdatedAt = timeProvider.GetUtcNow();

        return ValueTask.FromResult(RagSandboxProjection.ToRecordSummary(existing, score: null));
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
        collection.UpdatedAt = timeProvider.GetUtcNow();
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

        await EnsureVectorsAsync(collection, cancellationToken).ConfigureAwait(false);
        var queryVector = await GenerateVectorAsync(collection, query, cancellationToken).ConfigureAwait(false);

        return collection.Records
            .Select(record => RagSandboxProjection.ToRecordSummary(
                record,
                similarityCalculator.Calculate(queryVector, record.Vector!)))
            .OrderByDescending(record => record.Score ?? 0)
            .ThenBy(record => record.Id, StringComparer.OrdinalIgnoreCase)
            .Take(Math.Clamp(limit, 1, 50))
            .ToArray();
    }

    public async ValueTask<IReadOnlyList<RagSandboxSearchHit>> SearchAcrossCollectionsAsync(
        IReadOnlyList<string> collectionNames,
        string? query,
        int limit,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(collectionNames);

        if (string.IsNullOrWhiteSpace(query) || collectionNames.Count == 0)
        {
            return [];
        }

        var selectedCollections = collectionNames
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Select(FindCollection)
            .OfType<RagSandboxCollectionState>()
            .ToArray();

        var queryVectors = new Dictionary<int, float[]>();
        var hits = new List<RagSandboxSearchHit>();

        foreach (var collection in selectedCollections)
        {
            await EnsureVectorsAsync(collection, cancellationToken).ConfigureAwait(false);

            if (!queryVectors.TryGetValue(collection.Options.VectorSize, out var queryVector))
            {
                queryVector = await GenerateVectorAsync(collection, query, cancellationToken).ConfigureAwait(false);
                queryVectors.Add(collection.Options.VectorSize, queryVector);
            }

            hits.AddRange(collection.Records.Select(record =>
                RagSandboxProjection.ToSearchHit(
                    collection,
                    record,
                    similarityCalculator.Calculate(queryVector, record.Vector!))));
        }

        return hits
            .OrderByDescending(hit => hit.Score)
            .ThenBy(hit => hit.CollectionName, StringComparer.OrdinalIgnoreCase)
            .ThenBy(hit => hit.RecordId, StringComparer.OrdinalIgnoreCase)
            .Take(Math.Clamp(limit, 1, 100))
            .ToArray();
    }

    private RagSandboxCollectionState? FindCollection(string collectionName)
    {
        return collections.FirstOrDefault(collection =>
            string.Equals(collection.Name, collectionName, StringComparison.OrdinalIgnoreCase));
    }

    private async ValueTask EnsureVectorsAsync(
        RagSandboxCollectionState collection,
        CancellationToken cancellationToken)
    {
        if (collection.Records.All(record => HasExpectedVector(record, collection.Options.VectorSize)))
        {
            return;
        }

        await vectorGate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            foreach (var record in collection.Records.Where(
                         record => !HasExpectedVector(record, collection.Options.VectorSize)))
            {
                record.Vector = await GenerateVectorAsync(collection, record.Text, cancellationToken)
                    .ConfigureAwait(false);
            }
        }
        finally
        {
            vectorGate.Release();
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

    private static bool HasExpectedVector(RagSandboxRecordState record, int expectedSize)
        => record.Vector is { Length: > 0 } vector && vector.Length == expectedSize;

    private static void InvalidateVectors(RagSandboxCollectionState collection)
    {
        foreach (var record in collection.Records)
        {
            record.Vector = null;
        }
    }

    private static string Normalize(string? value)
        => (value ?? string.Empty).Trim().ToLowerInvariant();
}
