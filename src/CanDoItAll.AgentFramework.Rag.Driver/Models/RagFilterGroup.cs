namespace CanDoItAll.AgentFramework.Rag.Driver.Models;

public sealed record RagFilterGroup : RagFilter
{
    public required RagFilterGroupOperator Operator { get; init; }

    public required IReadOnlyList<RagFilter> Filters { get; init; }

    public static RagFilterGroup All(params RagFilter[] filters)
    {
        return new RagFilterGroup
        {
            Operator = RagFilterGroupOperator.All,
            Filters = filters
        };
    }

    public static RagFilterGroup Any(params RagFilter[] filters)
    {
        return new RagFilterGroup
        {
            Operator = RagFilterGroupOperator.Any,
            Filters = filters
        };
    }

    public static RagFilterGroup Not(RagFilter filter)
    {
        return new RagFilterGroup
        {
            Operator = RagFilterGroupOperator.Not,
            Filters = [filter]
        };
    }

    internal override void ValidateCore()
    {
        ArgumentNullException.ThrowIfNull(Filters);

        if (!Enum.IsDefined(Operator))
        {
            throw new ArgumentOutOfRangeException(nameof(Operator), Operator, "Unsupported filter group operator.");
        }

        if (Filters.Count == 0)
        {
            throw new ArgumentException("At least one child filter is required.", nameof(Filters));
        }

        if (Operator == RagFilterGroupOperator.Not && Filters.Count != 1)
        {
            throw new ArgumentException("A NOT filter group must contain exactly one child filter.", nameof(Filters));
        }

        foreach (var filter in Filters)
        {
            ArgumentNullException.ThrowIfNull(filter);
            filter.Validate();
        }
    }
}

