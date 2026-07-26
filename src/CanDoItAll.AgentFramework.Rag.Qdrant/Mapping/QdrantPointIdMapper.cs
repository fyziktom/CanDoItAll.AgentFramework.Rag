using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Qdrant.Client.Grpc;

namespace CanDoItAll.AgentFramework.Rag.Qdrant.Mapping;

internal static class QdrantPointIdMapper
{
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
}
