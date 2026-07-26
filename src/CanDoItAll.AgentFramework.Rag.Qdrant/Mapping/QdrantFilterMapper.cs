using CanDoItAll.AgentFramework.Rag.Driver.Models;
using Qdrant.Client.Grpc;
using QdrantRange = Qdrant.Client.Grpc.Range;

namespace CanDoItAll.AgentFramework.Rag.Qdrant.Mapping;

internal static class QdrantFilterMapper
{
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
}
