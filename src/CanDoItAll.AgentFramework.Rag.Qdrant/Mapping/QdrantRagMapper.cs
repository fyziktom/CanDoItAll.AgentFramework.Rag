using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using CanDoItAll.AgentFramework.Rag.Driver.Models;
using Qdrant.Client.Grpc;
using QdrantRange = Qdrant.Client.Grpc.Range;

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

    public static Filter ToFilter(RagFilter filter)
    {
        ArgumentNullException.ThrowIfNull(filter);
        filter.Validate();

        return filter switch
        {
            RagFilterCondition condition => ToFilter(condition),
            RagFilterGroup group => ToFilter(group),
            _ => throw new ArgumentOutOfRangeException(nameof(filter), filter, "Unsupported RAG filter.")
        };
    }

    public static PayloadSchemaType ToPayloadSchemaType(RagPayloadIndexKind kind)
    {
        return kind switch
        {
            RagPayloadIndexKind.Keyword => PayloadSchemaType.Keyword,
            RagPayloadIndexKind.Integer => PayloadSchemaType.Integer,
            RagPayloadIndexKind.Float => PayloadSchemaType.Float,
            RagPayloadIndexKind.Boolean => PayloadSchemaType.Bool,
            RagPayloadIndexKind.DateTime => PayloadSchemaType.Datetime,
            RagPayloadIndexKind.Text => PayloadSchemaType.Text,
            RagPayloadIndexKind.Uuid => PayloadSchemaType.Uuid,
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "Unsupported payload index kind.")
        };
    }

    public static PayloadIndexParams ToPayloadIndexParams(RagPayloadIndexKind kind)
    {
        return kind switch
        {
            RagPayloadIndexKind.Keyword => new PayloadIndexParams { KeywordIndexParams = new KeywordIndexParams() },
            RagPayloadIndexKind.Integer => new PayloadIndexParams { IntegerIndexParams = new IntegerIndexParams() },
            RagPayloadIndexKind.Float => new PayloadIndexParams { FloatIndexParams = new FloatIndexParams() },
            RagPayloadIndexKind.Boolean => new PayloadIndexParams { BoolIndexParams = new BoolIndexParams() },
            RagPayloadIndexKind.DateTime => new PayloadIndexParams { DatetimeIndexParams = new DatetimeIndexParams() },
            RagPayloadIndexKind.Text => new PayloadIndexParams { TextIndexParams = new TextIndexParams() },
            RagPayloadIndexKind.Uuid => new PayloadIndexParams { UuidIndexParams = new UuidIndexParams() },
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "Unsupported payload index kind.")
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

    private static Filter ToFilter(RagFilterCondition condition)
    {
        var filter = new Filter();
        switch (condition.Operator)
        {
            case RagFilterOperator.Equal:
                filter.Must.Add(ToEqualCondition(condition));
                break;
            case RagFilterOperator.In:
                filter.Must.Add(ToMembershipCondition(condition));
                break;
            case RagFilterOperator.Range:
                filter.Must.Add(ToRangeCondition(condition));
                break;
            case RagFilterOperator.Exists:
                filter.MustNot.Add(Conditions.IsEmpty(condition.FieldName));
                break;
            case RagFilterOperator.Missing:
                filter.Must.Add(Conditions.IsEmpty(condition.FieldName));
                break;
            default:
                throw new ArgumentOutOfRangeException(
                    nameof(condition),
                    condition.Operator,
                    "Unsupported filter operator.");
        }

        return filter;
    }

    private static Filter ToFilter(RagFilterGroup group)
    {
        var filter = new Filter();
        foreach (var child in group.Filters)
        {
            var childFilter = ToFilter(child);
            var condition = Conditions.Filter(childFilter);
            switch (group.Operator)
            {
                case RagFilterGroupOperator.All:
                    filter.Must.Add(condition);
                    break;
                case RagFilterGroupOperator.Any:
                    filter.Should.Add(condition);
                    break;
                case RagFilterGroupOperator.Not:
                    filter.MustNot.Add(condition);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(group),
                        group.Operator,
                        "Unsupported filter group operator.");
            }
        }

        return filter;
    }

    private static Condition ToEqualCondition(RagFilterCondition condition)
    {
        var value = condition.Value ?? throw new ArgumentException("Equal filters require a value.", nameof(condition));
        return value.Kind switch
        {
            RagFilterValueKind.String => Conditions.MatchKeyword(condition.FieldName, value.StringValue!),
            RagFilterValueKind.Integer => Conditions.Match(condition.FieldName, value.IntegerValue!.Value),
            RagFilterValueKind.Boolean => Conditions.Match(condition.FieldName, value.BooleanValue!.Value),
            RagFilterValueKind.Double => Conditions.Range(
                condition.FieldName,
                new QdrantRange
                {
                    Gte = value.DoubleValue!.Value,
                    Lte = value.DoubleValue.Value
                }),
            RagFilterValueKind.DateTime => Conditions.DatetimeRange(
                condition.FieldName,
                null,
                null,
                value.DateTimeValue!.Value.UtcDateTime,
                value.DateTimeValue.Value.UtcDateTime),
            _ => throw new ArgumentOutOfRangeException(nameof(condition), value.Kind, "Unsupported filter value kind.")
        };
    }

    private static Condition ToMembershipCondition(RagFilterCondition condition)
    {
        return condition.Values[0].Kind switch
        {
            RagFilterValueKind.String => Conditions.Match(
                condition.FieldName,
                condition.Values.Select(value => value.StringValue!).ToArray()),
            RagFilterValueKind.Integer => Conditions.Match(
                condition.FieldName,
                condition.Values.Select(value => value.IntegerValue!.Value).ToArray()),
            _ => throw new NotSupportedException("Qdrant membership filters support string and integer values only.")
        };
    }

    private static Condition ToRangeCondition(RagFilterCondition condition)
    {
        var range = condition.Range ?? throw new ArgumentException("Range filters require a range.", nameof(condition));
        var values = range.Values();
        if (values[0].Kind == RagFilterValueKind.DateTime)
        {
            return Conditions.DatetimeRange(
                condition.FieldName,
                ToUtcDateTime(range.LessThan),
                ToUtcDateTime(range.GreaterThan),
                ToUtcDateTime(range.GreaterThanOrEqual),
                ToUtcDateTime(range.LessThanOrEqual));
        }

        return Conditions.Range(condition.FieldName, ToQdrantRange(range));
    }

    private static QdrantRange ToQdrantRange(RagFilterRange range)
    {
        var qdrantRange = new QdrantRange();
        if (range.GreaterThan is not null)
        {
            qdrantRange.Gt = ToDouble(range.GreaterThan);
        }

        if (range.GreaterThanOrEqual is not null)
        {
            qdrantRange.Gte = ToDouble(range.GreaterThanOrEqual);
        }

        if (range.LessThan is not null)
        {
            qdrantRange.Lt = ToDouble(range.LessThan);
        }

        if (range.LessThanOrEqual is not null)
        {
            qdrantRange.Lte = ToDouble(range.LessThanOrEqual);
        }

        return qdrantRange;
    }

    private static double ToDouble(RagFilterValue value)
    {
        return value.Kind switch
        {
            RagFilterValueKind.Integer => value.IntegerValue!.Value,
            RagFilterValueKind.Double => value.DoubleValue!.Value,
            _ => throw new ArgumentException("Numeric range bounds must be integer or double values.", nameof(value))
        };
    }

    private static DateTime? ToUtcDateTime(RagFilterValue? value)
    {
        return value?.Kind switch
        {
            null => null,
            RagFilterValueKind.DateTime => value.DateTimeValue!.Value.UtcDateTime,
            _ => throw new ArgumentException("Date-time range bounds must be date-time values.", nameof(value))
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
