using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using CanDoItAll.AgentFramework.Rag.Driver.Models;
using Qdrant.Client.Grpc;

namespace CanDoItAll.AgentFramework.Rag.Qdrant.Mapping;

public static class QdrantRagMapper
{
    public const string KnowledgeIdPayloadKey = "_candoitall_knowledge_id";
    public const string KnowledgeTextPayloadKey = "_candoitall_knowledge_text";
    public const string KnowledgeTagsPayloadKey = "_candoitall_knowledge_tags";

    public static VectorParams ToVectorParams(RagCollectionOptions collection)
    {
        ArgumentNullException.ThrowIfNull(collection);
        collection.Validate();

        return new VectorParams
        {
            Size = (ulong)collection.VectorSize,
            Distance = ToQdrantDistance(collection.Distance)
        };
    }

    public static Distance ToQdrantDistance(RagDistanceMetric distance)
    {
        return distance switch
        {
            RagDistanceMetric.Cosine => Distance.Cosine,
            RagDistanceMetric.Dot => Distance.Dot,
            RagDistanceMetric.Euclidean => Distance.Euclid,
            RagDistanceMetric.Manhattan => Distance.Manhattan,
            _ => throw new ArgumentOutOfRangeException(nameof(distance), distance, "Unsupported distance metric.")
        };
    }

    public static PointStruct ToPointStruct(RagKnowledgeEntry entry, float[] vector)
    {
        ArgumentNullException.ThrowIfNull(entry);
        ArgumentNullException.ThrowIfNull(vector);

        entry.Validate(vector.Length);
        var point = new PointStruct
        {
            Id = ToPointId(entry.Id),
            Vectors = vector
        };

        foreach (var (key, value) in entry.Metadata)
        {
            if (value is null || IsReservedPayloadKey(key))
            {
                continue;
            }

            point.Payload[key] = ToPayloadValue(value);
        }

        point.Payload[KnowledgeIdPayloadKey] = entry.Id;
        point.Payload[KnowledgeTextPayloadKey] = entry.Text;
        if (entry.Tags.Count > 0)
        {
            point.Payload[KnowledgeTagsPayloadKey] = ToPayloadValue(entry.Tags);
        }

        return point;
    }

    public static RagSearchResult ToSearchResult(ScoredPoint point)
    {
        ArgumentNullException.ThrowIfNull(point);

        var metadata = new Dictionary<string, object?>(StringComparer.Ordinal);
        foreach (var (key, value) in point.Payload)
        {
            if (IsReservedPayloadKey(key))
            {
                continue;
            }

            metadata[key] = FromPayloadValue(value);
        }

        var id = TryReadStringPayload(point.Payload, KnowledgeIdPayloadKey) ?? point.Id.ToString();
        var text = TryReadStringPayload(point.Payload, KnowledgeTextPayloadKey) ?? string.Empty;
        var tags = TryReadStringListPayload(point.Payload, KnowledgeTagsPayloadKey);

        return new RagSearchResult
        {
            Knowledge = new RagKnowledgeEntry
            {
                Id = id,
                Text = text,
                Metadata = metadata,
                Tags = tags
            },
            Score = point.Score
        };
    }

    public static PointId ToPointId(string knowledgeId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(knowledgeId);

        if (Guid.TryParse(knowledgeId, out var guid))
        {
            return guid;
        }

        if (ulong.TryParse(knowledgeId, NumberStyles.None, CultureInfo.InvariantCulture, out var numericId))
        {
            return numericId;
        }

        return CreateDeterministicGuid(knowledgeId);
    }

    public static Guid CreateDeterministicGuid(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes($"candoitall-rag:{value}"));
        Span<byte> guidBytes = stackalloc byte[16];
        bytes.AsSpan(0, 16).CopyTo(guidBytes);
        return new Guid(guidBytes);
    }

    public static Value ToPayloadValue(object value)
    {
        ArgumentNullException.ThrowIfNull(value);

        return value switch
        {
            string stringValue => stringValue,
            bool boolValue => boolValue,
            byte byteValue => (long)byteValue,
            short shortValue => (long)shortValue,
            int intValue => (long)intValue,
            long longValue => longValue,
            float floatValue => (double)floatValue,
            double doubleValue => doubleValue,
            decimal decimalValue => (double)decimalValue,
            DateTime dateTime => dateTime.ToString("O", CultureInfo.InvariantCulture),
            DateTimeOffset dateTimeOffset => dateTimeOffset.ToString("O", CultureInfo.InvariantCulture),
            string[] strings => strings,
            IEnumerable<string> strings => strings.ToArray(),
            _ => value.ToString() ?? string.Empty
        };
    }

    public static object? FromPayloadValue(Value value)
    {
        ArgumentNullException.ThrowIfNull(value);

        return value.KindCase switch
        {
            Value.KindOneofCase.StringValue => value.StringValue,
            Value.KindOneofCase.BoolValue => value.BoolValue,
            Value.KindOneofCase.IntegerValue => value.IntegerValue,
            Value.KindOneofCase.DoubleValue => value.DoubleValue,
            Value.KindOneofCase.ListValue => value.ListValue.Values.Select(FromPayloadValue).ToArray(),
            Value.KindOneofCase.None => null,
            _ => value.ToString()
        };
    }

    private static string? TryReadStringPayload(
        IDictionary<string, Value> payload,
        string key)
    {
        if (!payload.TryGetValue(key, out var value))
        {
            return null;
        }

        return value.StringValue;
    }

    private static IReadOnlyList<string> TryReadStringListPayload(
        IDictionary<string, Value> payload,
        string key)
    {
        if (!payload.TryGetValue(key, out var value))
        {
            return Array.Empty<string>();
        }

        if (value.KindCase == Value.KindOneofCase.ListValue)
        {
            return value.ListValue.Values
                .Where(item => item.KindCase == Value.KindOneofCase.StringValue)
                .Select(item => item.StringValue)
                .Where(item => !string.IsNullOrWhiteSpace(item))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(item => item, StringComparer.Ordinal)
                .ToArray();
        }

        if (value.KindCase == Value.KindOneofCase.StringValue &&
            !string.IsNullOrWhiteSpace(value.StringValue))
        {
            return [value.StringValue];
        }

        return Array.Empty<string>();
    }

    private static bool IsReservedPayloadKey(string key)
    {
        return string.Equals(key, KnowledgeIdPayloadKey, StringComparison.Ordinal) ||
            string.Equals(key, KnowledgeTextPayloadKey, StringComparison.Ordinal) ||
            string.Equals(key, KnowledgeTagsPayloadKey, StringComparison.Ordinal);
    }
}
