using CanDoItAll.AgentFramework.Rag.Driver.Models;
using Qdrant.Client.Grpc;

namespace CanDoItAll.AgentFramework.Rag.Qdrant.Mapping;

internal static class QdrantPayloadIndexMapper
{
    public static PayloadSchemaType ToPayloadSchemaType(RagPayloadIndexKind kind)
    {
        return kind switch
        {
            RagPayloadIndexKind.Keyword => PayloadSchemaType.Keyword,
            RagPayloadIndexKind.Integer => PayloadSchemaType.Integer,
            RagPayloadIndexKind.Float => PayloadSchemaType.Float,
            RagPayloadIndexKind.Boolean => PayloadSchemaType.Bool,
            RagPayloadIndexKind.DateTime => PayloadSchemaType.Datetime,
            RagPayloadIndexKind.Text => PayloadSchemaType.Text,
            RagPayloadIndexKind.Uuid => PayloadSchemaType.Uuid,
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "Unsupported payload index kind.")
        };
    }

    public static PayloadIndexParams ToPayloadIndexParams(RagPayloadIndexKind kind)
    {
        return kind switch
        {
            RagPayloadIndexKind.Keyword => new PayloadIndexParams { KeywordIndexParams = new KeywordIndexParams() },
            RagPayloadIndexKind.Integer => new PayloadIndexParams { IntegerIndexParams = new IntegerIndexParams() },
            RagPayloadIndexKind.Float => new PayloadIndexParams { FloatIndexParams = new FloatIndexParams() },
            RagPayloadIndexKind.Boolean => new PayloadIndexParams { BoolIndexParams = new BoolIndexParams() },
            RagPayloadIndexKind.DateTime => new PayloadIndexParams { DatetimeIndexParams = new DatetimeIndexParams() },
            RagPayloadIndexKind.Text => new PayloadIndexParams { TextIndexParams = new TextIndexParams() },
            RagPayloadIndexKind.Uuid => new PayloadIndexParams { UuidIndexParams = new UuidIndexParams() },
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "Unsupported payload index kind.")
        };
    }
}
