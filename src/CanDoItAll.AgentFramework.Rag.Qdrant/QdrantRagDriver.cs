using CanDoItAll.AgentFramework.Rag.Driver.Abstractions;
using CanDoItAll.AgentFramework.Rag.Driver.Embeddings;
using CanDoItAll.AgentFramework.Rag.Driver.Models;
using CanDoItAll.AgentFramework.Rag.Qdrant.Mapping;
using Grpc.Core;
using Qdrant.Client;
using Qdrant.Client.Grpc;

namespace CanDoItAll.AgentFramework.Rag.Qdrant;

public sealed class QdrantRagDriver : RagDriverBase
{
    private readonly QdrantClient _client;
    private readonly QdrantRagOptions _options;

    public QdrantRagDriver(
        QdrantClient client,
        IRagEmbeddingGenerator embeddingGenerator,
        RagCollectionOptions defaultCollection,
        QdrantRagOptions? options = null)
        : base(
            RagDriverProviderNames.Qdrant,
            defaultCollection,
            embeddingGenerator,
            RagDriverCapabilities.WithTagsAndProjectionControls)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _options = options ?? new QdrantRagOptions();
        _options.Validate();
    }

    public override async ValueTask EnsureCollectionAsync(
        RagCollectionOptions? collection = null,
        CancellationToken cancellationToken = default)
    {
        var effectiveCollection = collection ?? DefaultCollection;
        effectiveCollection.Validate();

        if (!await _client.CollectionExistsAsync(effectiveCollection.CollectionName, cancellationToken)
                .ConfigureAwait(false))
        {
            if (!_options.CreateCollectionIfMissing)
            {
                throw new InvalidOperationException(
                    $"Qdrant collection '{effectiveCollection.CollectionName}' does not exist and automatic creation is disabled.");
            }

            try
            {
                await _client.CreateCollectionAsync(
                        effectiveCollection.CollectionName,
                        QdrantRagMapper.ToVectorParams(effectiveCollection),
                        cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (RpcException exception) when (exception.StatusCode == StatusCode.AlreadyExists)
            {
            }
        }

        var info = await _client.GetCollectionInfoAsync(
                effectiveCollection.CollectionName,
                cancellationToken)
            .ConfigureAwait(false);
        var actual = info.Config?.Params?.VectorsConfig?.Params;
        var expected = QdrantRagMapper.ToVectorParams(effectiveCollection);
        if (actual is null || actual.Size != expected.Size || actual.Distance != expected.Distance)
        {
            throw new InvalidOperationException(
                $"Qdrant collection '{effectiveCollection.CollectionName}' does not match the configured vector size and distance.");
        }
    }

    public override async ValueTask UpsertAsync(
        RagUpsertRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var collection = ResolveCollection(request.CollectionName);
        request.Validate(collection.VectorSize);

        var points = new List<PointStruct>(request.Entries.Count);
        foreach (var entry in request.Entries)
        {
            var vector = await ResolveEntryVectorAsync(entry, collection, cancellationToken).ConfigureAwait(false);
            points.Add(QdrantRagMapper.ToPointStruct(entry, vector));
        }

        var result = await _client.UpsertAsync(
                collection.CollectionName,
                points,
                wait: _options.WaitForWrites,
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        QdrantRagWriteReceiptPolicy.EnsureAccepted(
            result,
            "upsert knowledge entries",
            _options.WaitForWrites);
    }

    public override async ValueTask DeleteAsync(
        RagDeleteRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var collection = ResolveCollection(request.CollectionName);
        request.Validate();

        var ids = request.KnowledgeIds.Select(QdrantRagMapper.ToPointId).ToArray();
        var result = await _client.DeleteAsync(
                collection.CollectionName,
                ids,
                wait: _options.WaitForWrites,
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        QdrantRagWriteReceiptPolicy.EnsureAccepted(
            result,
            "delete knowledge entries",
            _options.WaitForWrites);
    }

    public override async ValueTask DeleteByFilterAsync(
        RagDeleteByFilterRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var collection = ResolveCollection(request.CollectionName);
        request.Validate();

        var result = await _client.DeleteAsync(
                collection.CollectionName,
                QdrantRagMapper.ToFilter(request.Filter),
                wait: _options.WaitForWrites,
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        QdrantRagWriteReceiptPolicy.EnsureAccepted(
            result,
            "delete knowledge entries by filter",
            _options.WaitForWrites);
    }

    public override async ValueTask<RagPayloadIndexResult> EnsurePayloadIndexAsync(
        RagPayloadIndexRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var collection = ResolveCollection(request.CollectionName);
        request.Validate();

        var expectedSchema = QdrantRagMapper.ToPayloadSchemaType(request.IndexKind);
        var info = await _client.GetCollectionInfoAsync(collection.CollectionName, cancellationToken)
            .ConfigureAwait(false);
        if (info.PayloadSchema.TryGetValue(request.FieldName, out var existingSchema))
        {
            if (existingSchema.DataType != expectedSchema)
            {
                throw new InvalidOperationException(
                    $"Qdrant payload field '{request.FieldName}' is indexed with an incompatible type.");
            }

            return EnsuredIndex(collection.CollectionName, request);
        }

        var result = await _client.CreatePayloadIndexAsync(
                collection.CollectionName,
                request.FieldName,
                expectedSchema,
                QdrantRagMapper.ToPayloadIndexParams(request.IndexKind),
                wait: _options.WaitForWrites,
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        QdrantRagWriteReceiptPolicy.EnsureAccepted(
            result,
            $"create payload index '{request.FieldName}'",
            _options.WaitForWrites);

        return EnsuredIndex(collection.CollectionName, request);
    }

    private static RagPayloadIndexResult EnsuredIndex(
        string collectionName,
        RagPayloadIndexRequest request)
    {
        return new RagPayloadIndexResult
        {
            CollectionName = collectionName,
            FieldName = request.FieldName,
            IndexKind = request.IndexKind,
            Status = RagPayloadIndexStatus.Ensured
        };
    }

    public override async ValueTask<IReadOnlyList<RagSearchResult>> SearchAsync(
        RagSearchRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var collection = ResolveCollection(request.CollectionName);
        var vector = await ResolveQueryVectorAsync(request, collection, cancellationToken).ConfigureAwait(false);

        var points = await _client.SearchAsync(
                collection.CollectionName,
                vector,
                filter: request.Filter is null ? null : QdrantRagMapper.ToFilter(request.Filter),
                limit: (ulong)request.Limit,
                payloadSelector: true,
                scoreThreshold: request.MinScore is null ? null : (float)request.MinScore.Value,
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        return points.Select(QdrantRagMapper.ToSearchResult).ToArray();
    }

}
