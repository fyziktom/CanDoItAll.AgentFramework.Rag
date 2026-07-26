using CanDoItAll.AgentFramework.Rag.Sandbox.Services;

namespace CanDoItAll.AgentFramework.Rag.Tests.Sandbox;

public sealed class RagSandboxSimilarityCalculatorTests
{
    private readonly RagSandboxSimilarityCalculator calculator = new();

    [Fact]
    public void Calculate_AlignedVectors_ReturnsOne()
    {
        var score = calculator.Calculate([1, 2, 3], [2, 4, 6]);

        Assert.Equal(1, score, precision: 12);
    }

    [Fact]
    public void Calculate_OrthogonalVectors_ReturnsZero()
    {
        var score = calculator.Calculate([1, 0], [0, 1]);

        Assert.Equal(0, score);
    }

    [Fact]
    public void Calculate_ZeroVector_ReturnsZero()
    {
        var score = calculator.Calculate([0, 0], [1, 1]);

        Assert.Equal(0, score);
    }

    [Fact]
    public void Calculate_NullVector_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => calculator.Calculate(null!, [1]));
    }
}
