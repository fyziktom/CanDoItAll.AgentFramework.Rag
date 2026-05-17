namespace CanDoItAll.AgentFramework.Rag.Driver.Models;

public sealed record RagFilterRange
{
    public RagFilterValue? GreaterThan { get; init; }

    public RagFilterValue? GreaterThanOrEqual { get; init; }

    public RagFilterValue? LessThan { get; init; }

    public RagFilterValue? LessThanOrEqual { get; init; }

    public static RagFilterRange Closed(RagFilterValue lowerBound, RagFilterValue upperBound)
    {
        return new RagFilterRange
        {
            GreaterThanOrEqual = lowerBound,
            LessThanOrEqual = upperBound
        };
    }

    public void Validate()
    {
        var values = Values().ToArray();
        if (values.Length == 0)
        {
            throw new ArgumentException("At least one range bound is required.");
        }

        foreach (var value in values)
        {
            value.Validate();
        }

        var firstKind = values[0].Kind;
        if (firstKind == RagFilterValueKind.Boolean || firstKind == RagFilterValueKind.String)
        {
            throw new ArgumentException("Range filters support numeric and date-time values only.");
        }

        if (values.Any(value => !IsCompatibleRangeKind(firstKind, value.Kind)))
        {
            throw new ArgumentException("Range filters cannot mix numeric and date-time values.");
        }
    }

    public IReadOnlyList<RagFilterValue> Values()
    {
        return new[]
            {
                GreaterThan,
                GreaterThanOrEqual,
                LessThan,
                LessThanOrEqual
            }
            .OfType<RagFilterValue>()
            .ToArray();
    }

    private static bool IsCompatibleRangeKind(
        RagFilterValueKind firstKind,
        RagFilterValueKind candidateKind)
    {
        if (firstKind == RagFilterValueKind.DateTime || candidateKind == RagFilterValueKind.DateTime)
        {
            return firstKind == RagFilterValueKind.DateTime && candidateKind == RagFilterValueKind.DateTime;
        }

        return candidateKind is RagFilterValueKind.Integer or RagFilterValueKind.Double;
    }
}

