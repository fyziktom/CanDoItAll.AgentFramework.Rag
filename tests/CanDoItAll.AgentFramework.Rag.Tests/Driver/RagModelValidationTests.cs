using CanDoItAll.AgentFramework.Rag.Driver.Models;

namespace CanDoItAll.AgentFramework.Rag.Tests.Driver;

public sealed class RagModelValidationTests
{
    [Fact]
    public void KnowledgeEntryValidate_AllowsMetadataAndExpectedVectorSize()
    {
        var entry = new RagKnowledgeEntry
        {
            Id = "invoice-approval",
            Text = "Approvals require manager sign-off.",
            Metadata = new Dictionary<string, object?>
            {
                ["source"] = "policy",
                ["priority"] = 1
            },
            Vector = new[] { 0.1f, 0.2f, 0.3f }
        };

        entry.Validate(expectedVectorSize: 3);

        Assert.Equal("policy", entry.Metadata["source"]);
    }

    [Fact]
    public void KnowledgeEntryValidate_RejectsWrongVectorSize()
    {
        var entry = new RagKnowledgeEntry
        {
            Id = "invoice-approval",
            Text = "Approvals require manager sign-off.",
            Vector = new[] { 0.1f, 0.2f }
        };

        Assert.Throws<ArgumentException>(() => entry.Validate(expectedVectorSize: 3));
    }

    [Fact]
    public void SearchRequestValidate_RejectsNonPositiveLimit()
    {
        var request = new RagSearchRequest
        {
            QueryText = "approval rules",
            Limit = 0
        };

        Assert.Throws<ArgumentOutOfRangeException>(() => request.Validate());
    }

    [Fact]
    public void VectorValidation_RejectsNonFiniteValues()
    {
        Assert.Throws<ArgumentException>(() =>
            RagVectorValidation.EnsureVectorSize(new[] { 1.0f, float.NaN }, 2, "vector"));
    }
}
