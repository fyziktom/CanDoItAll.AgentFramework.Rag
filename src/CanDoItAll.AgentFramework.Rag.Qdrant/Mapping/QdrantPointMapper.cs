using CanDoItAll.AgentFramework.Rag.Driver.Models;
using Qdrant.Client.Grpc;

namespace CanDoItAll.AgentFramework.Rag.Qdrant.Mapping;

internal static class QdrantPointMapper
{
    public const string KnowledgeIdPayloadKey = "_candoitall_knowledge_id";
    public const string KnowledgeTextPayloadKey = "_candoitall_knowledge_text";
    public const string KnowledgeTagsPayloadKey = "_candoitall_knowledge_tags";

    public static PointStruct ToPointStruct(RagKnowledgeEntry entry, float[] vector)
    {
        ArgumentNullException.ThrowIfNull(entry);
        ArgumentNullException.ThrowIfNull(vector);

        entry.Validate(vector.Length);
        var point = new PointStruct
        {
            Id = QdrantPointIdMapper.ToPointId(entry.Id),
            Vectors = vector
        };

        foreach (var (key, value) in entry.Metadata)
        {
            if (value is null || IsReservedPayloadKey(key))
            {
                continue;
            }

            point.Payload[key] = QdrantPayloadValueMapper.ToPayloadValue(value);
        }

        point.Payload[KnowledgeIdPayloadKey] = entry.Id;
        point.Payload[KnowledgeTextPayloadKey] = entry.Text;
        if (entry.Tags.Count > 0)
        {
            point.Payload[KnowledgeTagsPayloadKey] = QdrantPayloadValueMapper.ToPayloadValue(entry.Tags);
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

            metadata[key] = QdrantPayloadValueMapper.FromPayloadValue(value);
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
