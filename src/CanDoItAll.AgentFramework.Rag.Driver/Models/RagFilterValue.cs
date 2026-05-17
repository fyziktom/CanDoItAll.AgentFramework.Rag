namespace CanDoItAll.AgentFramework.Rag.Driver.Models;

public sealed record RagFilterValue
{
    public required RagFilterValueKind Kind { get; init; }

    public string? StringValue { get; init; }

    public long? IntegerValue { get; init; }

    public double? DoubleValue { get; init; }

    public bool? BooleanValue { get; init; }

    public DateTimeOffset? DateTimeValue { get; init; }

    public static RagFilterValue FromString(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        return new RagFilterValue
        {
            Kind = RagFilterValueKind.String,
            StringValue = value
        };
    }

    public static RagFilterValue FromInteger(long value)
    {
        return new RagFilterValue
        {
            Kind = RagFilterValueKind.Integer,
            IntegerValue = value
        };
    }

    public static RagFilterValue FromDouble(double value)
    {
        return new RagFilterValue
        {
            Kind = RagFilterValueKind.Double,
            DoubleValue = value
        };
    }

    public static RagFilterValue FromBoolean(bool value)
    {
        return new RagFilterValue
        {
            Kind = RagFilterValueKind.Boolean,
            BooleanValue = value
        };
    }

    public static RagFilterValue FromDateTime(DateTimeOffset value)
    {
        return new RagFilterValue
        {
            Kind = RagFilterValueKind.DateTime,
            DateTimeValue = value
        };
    }

    public static implicit operator RagFilterValue(string value)
    {
        return FromString(value);
    }

    public static implicit operator RagFilterValue(int value)
    {
        return FromInteger(value);
    }

    public static implicit operator RagFilterValue(long value)
    {
        return FromInteger(value);
    }

    public static implicit operator RagFilterValue(double value)
    {
        return FromDouble(value);
    }

    public static implicit operator RagFilterValue(bool value)
    {
        return FromBoolean(value);
    }

    public static implicit operator RagFilterValue(DateTimeOffset value)
    {
        return FromDateTime(value);
    }

    public void Validate()
    {
        switch (Kind)
        {
            case RagFilterValueKind.String:
                ArgumentNullException.ThrowIfNull(StringValue);
                EnsureOnly(nameof(StringValue));
                break;
            case RagFilterValueKind.Integer:
                ArgumentNullException.ThrowIfNull(IntegerValue);
                EnsureOnly(nameof(IntegerValue));
                break;
            case RagFilterValueKind.Double:
                ArgumentNullException.ThrowIfNull(DoubleValue);
                if (!double.IsFinite(DoubleValue.Value))
                {
                    throw new ArgumentException("Filter double values must be finite.", nameof(DoubleValue));
                }

                EnsureOnly(nameof(DoubleValue));
                break;
            case RagFilterValueKind.Boolean:
                ArgumentNullException.ThrowIfNull(BooleanValue);
                EnsureOnly(nameof(BooleanValue));
                break;
            case RagFilterValueKind.DateTime:
                ArgumentNullException.ThrowIfNull(DateTimeValue);
                EnsureOnly(nameof(DateTimeValue));
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(Kind), Kind, "Unsupported filter value kind.");
        }
    }

    private void EnsureOnly(string allowedPropertyName)
    {
        var populatedCount =
            (StringValue is null ? 0 : 1) +
            (IntegerValue is null ? 0 : 1) +
            (DoubleValue is null ? 0 : 1) +
            (BooleanValue is null ? 0 : 1) +
            (DateTimeValue is null ? 0 : 1);

        if (populatedCount != 1)
        {
            throw new ArgumentException($"Filter value kind '{Kind}' must populate only {allowedPropertyName}.");
        }
    }
}

