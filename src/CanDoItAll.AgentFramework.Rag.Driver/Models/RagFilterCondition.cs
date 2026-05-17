namespace CanDoItAll.AgentFramework.Rag.Driver.Models;

public sealed record RagFilterCondition : RagFilter
{
    public required string FieldName { get; init; }

    public required RagFilterOperator Operator { get; init; }

    public RagFilterValue? Value { get; init; }

    public IReadOnlyList<RagFilterValue> Values { get; init; } = [];

    public RagFilterRange? Range { get; init; }

    public static RagFilterCondition Equal(string fieldName, RagFilterValue value)
    {
        return new RagFilterCondition
        {
            FieldName = fieldName,
            Operator = RagFilterOperator.Equal,
            Value = value
        };
    }

    public static RagFilterCondition In(string fieldName, params RagFilterValue[] values)
    {
        return new RagFilterCondition
        {
            FieldName = fieldName,
            Operator = RagFilterOperator.In,
            Values = values
        };
    }

    public static RagFilterCondition Within(string fieldName, RagFilterRange range)
    {
        return new RagFilterCondition
        {
            FieldName = fieldName,
            Operator = RagFilterOperator.Range,
            Range = range
        };
    }

    public static RagFilterCondition GreaterThan(string fieldName, RagFilterValue value)
    {
        return Within(fieldName, new RagFilterRange { GreaterThan = value });
    }

    public static RagFilterCondition GreaterThanOrEqual(string fieldName, RagFilterValue value)
    {
        return Within(fieldName, new RagFilterRange { GreaterThanOrEqual = value });
    }

    public static RagFilterCondition LessThan(string fieldName, RagFilterValue value)
    {
        return Within(fieldName, new RagFilterRange { LessThan = value });
    }

    public static RagFilterCondition LessThanOrEqual(string fieldName, RagFilterValue value)
    {
        return Within(fieldName, new RagFilterRange { LessThanOrEqual = value });
    }

    public static RagFilterCondition Exists(string fieldName)
    {
        return new RagFilterCondition
        {
            FieldName = fieldName,
            Operator = RagFilterOperator.Exists
        };
    }

    public static RagFilterCondition Missing(string fieldName)
    {
        return new RagFilterCondition
        {
            FieldName = fieldName,
            Operator = RagFilterOperator.Missing
        };
    }

    internal override void ValidateCore()
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(FieldName);

        switch (Operator)
        {
            case RagFilterOperator.Equal:
                ArgumentNullException.ThrowIfNull(Value);
                EnsureNoValuesOrRange();
                Value.Validate();
                break;
            case RagFilterOperator.In:
                ArgumentNullException.ThrowIfNull(Values);
                if (Values.Count == 0)
                {
                    throw new ArgumentException("At least one filter value is required.", nameof(Values));
                }

                foreach (var value in Values)
                {
                    value.Validate();
                }

                EnsureMembershipValueKinds();
                EnsureNoValueOrRange();
                break;
            case RagFilterOperator.Range:
                ArgumentNullException.ThrowIfNull(Range);
                Range.Validate();
                EnsureNoValueOrValues();
                break;
            case RagFilterOperator.Exists:
            case RagFilterOperator.Missing:
                EnsureNoValueValuesOrRange();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(Operator), Operator, "Unsupported filter operator.");
        }
    }

    private void EnsureMembershipValueKinds()
    {
        var firstKind = Values[0].Kind;
        if (firstKind is not (RagFilterValueKind.String or RagFilterValueKind.Integer))
        {
            throw new ArgumentException(
                "Membership filters support string and integer values only.",
                nameof(Values));
        }

        if (Values.Any(value => value.Kind != firstKind))
        {
            throw new ArgumentException(
                "Membership filters cannot mix value kinds.",
                nameof(Values));
        }
    }

    private void EnsureNoValuesOrRange()
    {
        if (Values.Count > 0 || Range is not null)
        {
            throw new ArgumentException("Equal filters accept Value only.");
        }
    }

    private void EnsureNoValueOrRange()
    {
        if (Value is not null || Range is not null)
        {
            throw new ArgumentException("Membership filters accept Values only.");
        }
    }

    private void EnsureNoValueOrValues()
    {
        if (Value is not null || Values.Count > 0)
        {
            throw new ArgumentException("Range filters accept Range only.");
        }
    }

    private void EnsureNoValueValuesOrRange()
    {
        if (Value is not null || Values.Count > 0 || Range is not null)
        {
            throw new ArgumentException("Existence filters do not accept values.");
        }
    }
}
