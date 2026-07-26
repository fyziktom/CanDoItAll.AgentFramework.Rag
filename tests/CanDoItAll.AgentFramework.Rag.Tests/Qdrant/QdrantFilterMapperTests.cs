using CanDoItAll.AgentFramework.Rag.Driver.Models;
using CanDoItAll.AgentFramework.Rag.Qdrant.Mapping;

namespace CanDoItAll.AgentFramework.Rag.Tests.Qdrant;

public sealed class QdrantFilterMapperTests
{
    [Fact]
    public void ToFilter_MapsStringEquality()
    {
        var filter = QdrantFilterMapper.ToFilter(RagFilterCondition.Equal("sourceKind", "policy"));

        var condition = Assert.Single(filter.Must);
        Assert.Equal("sourceKind", condition.Field.Key);
        Assert.Equal("policy", condition.Field.Match.Keyword);
    }

    [Fact]
    public void ToFilter_MapsStringMembership()
    {
        var filter = QdrantFilterMapper.ToFilter(
            RagFilterCondition.In("projectId", "project-a", "project-b"));

        var condition = Assert.Single(filter.Must);
        Assert.Equal("projectId", condition.Field.Key);
        Assert.Equal(["project-a", "project-b"], condition.Field.Match.Keywords.Strings);
    }

    [Fact]
    public void ToFilter_MapsNumericRange()
    {
        var filter = QdrantFilterMapper.ToFilter(
            RagFilterCondition.Within("projectionVersion", RagFilterRange.Closed(2, 5)));

        var condition = Assert.Single(filter.Must);
        Assert.Equal("projectionVersion", condition.Field.Key);
        Assert.True(condition.Field.Range.HasGte);
        Assert.True(condition.Field.Range.HasLte);
        Assert.Equal(2, condition.Field.Range.Gte);
        Assert.Equal(5, condition.Field.Range.Lte);
    }

    [Fact]
    public void ToFilter_MapsExistenceAsNotEmpty()
    {
        var filter = QdrantFilterMapper.ToFilter(RagFilterCondition.Exists("embeddingProfile"));

        var condition = Assert.Single(filter.MustNot);
        Assert.Equal("embeddingProfile", condition.IsEmpty.Key);
    }

    [Fact]
    public void ToFilter_MapsBooleanComposition()
    {
        var filter = QdrantFilterMapper.ToFilter(
            RagFilterGroup.Any(
                RagFilterCondition.Equal("sourceKind", "workflow"),
                RagFilterCondition.Equal("sourceKind", "process")));

        Assert.Empty(filter.Must);
        Assert.Equal(2, filter.Should.Count);
        Assert.All(filter.Should, condition => Assert.NotNull(condition.Filter));
    }

    [Fact]
    public void ToFilter_MapsProjectionLifecycleCleanupMetadata()
    {
        var filter = QdrantFilterMapper.ToFilter(
            RagFilterGroup.All(
                RagFilterCondition.Equal("sourceId", "source-1"),
                RagFilterCondition.Equal("embeddingProfile", "local-hashing:dimension=384"),
                RagFilterCondition.LessThan("projectionVersion", 7)));

        Assert.Equal(3, filter.Must.Count);
        Assert.All(filter.Must, condition => Assert.NotNull(condition.Filter));

        var versionCondition = filter.Must[2].Filter.Must.Single();
        Assert.Equal("projectionVersion", versionCondition.Field.Key);
        Assert.True(versionCondition.Field.Range.HasLt);
        Assert.Equal(7, versionCondition.Field.Range.Lt);
    }

    [Fact]
    public void ToFilter_UnsupportedMembershipValueKind_Throws()
    {
        var condition = RagFilterCondition.In("active", true, false);

        var exception = Assert.Throws<ArgumentException>(
            () => QdrantFilterMapper.ToFilter(condition));

        Assert.Contains("string and integer", exception.Message, StringComparison.OrdinalIgnoreCase);
    }
}
