using CanDoItAll.AgentFramework.Rag.Driver.Models;

namespace CanDoItAll.AgentFramework.Rag.Sandbox.Services;

internal static class RagSandboxSeedData
{
    public static List<RagSandboxCollectionState> Create(DateTimeOffset timestamp)
    {
        return
        [
            new RagSandboxCollectionState
            {
                Name = "finance-policies",
                Description = "Operational policies used when answering invoice and vendor questions.",
                Tags = ["finance", "policy"],
                Options = new RagCollectionOptions
                {
                    CollectionName = "finance-policies",
                    VectorSize = 64,
                    Distance = RagDistanceMetric.Cosine
                },
                UpdatedAt = timestamp,
                Records =
                {
                    new RagSandboxRecordState
                    {
                        Id = "invoice-approval",
                        Text = "Invoices over 5000 require manager approval before payment.",
                        Metadata = "source=finance-policy; owner=accounts-payable",
                        Tags = ["approval", "invoice"],
                        UpdatedAt = timestamp
                    },
                    new RagSandboxRecordState
                    {
                        Id = "vendor-terms",
                        Text = "Standard vendor payment terms are net 30 unless the contract overrides them.",
                        Metadata = "source=vendor-policy; owner=procurement",
                        Tags = ["vendor", "payment"],
                        UpdatedAt = timestamp
                    }
                }
            },
            new RagSandboxCollectionState
            {
                Name = "support-runbooks",
                Description = "Support knowledge for triage, escalation, and service recovery.",
                Tags = ["support", "runbook"],
                Options = new RagCollectionOptions
                {
                    CollectionName = "support-runbooks",
                    VectorSize = 64,
                    Distance = RagDistanceMetric.Cosine
                },
                UpdatedAt = timestamp,
                Records =
                {
                    new RagSandboxRecordState
                    {
                        Id = "qdrant-grpc-port",
                        Text = "Qdrant .NET client connects through gRPC and expects port 6334 to be reachable.",
                        Metadata = "source=rag-sandbox; service=qdrant",
                        Tags = ["qdrant", "connectivity"],
                        UpdatedAt = timestamp
                    }
                }
            }
        ];
    }
}
