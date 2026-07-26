using System.Globalization;
using Qdrant.Client.Grpc;

namespace CanDoItAll.AgentFramework.Rag.Qdrant.Mapping;

internal static class QdrantPayloadValueMapper
{
    public static Value ToPayloadValue(object value)
    {
        ArgumentNullException.ThrowIfNull(value);

        return value switch
        {
            string stringValue => stringValue,
            bool boolValue => boolValue,
            byte byteValue => (long)byteValue,
            short shortValue => (long)shortValue,
            int intValue => (long)intValue,
            long longValue => longValue,
            float floatValue => (double)floatValue,
            double doubleValue => doubleValue,
            decimal decimalValue => (double)decimalValue,
            DateTime dateTime => dateTime.ToString("O", CultureInfo.InvariantCulture),
            DateTimeOffset dateTimeOffset => dateTimeOffset.ToString("O", CultureInfo.InvariantCulture),
            string[] strings => strings,
            IEnumerable<string> strings => strings.ToArray(),
            _ => value.ToString() ?? string.Empty
        };
    }

    public static object? FromPayloadValue(Value value)
    {
        ArgumentNullException.ThrowIfNull(value);

        return value.KindCase switch
        {
            Value.KindOneofCase.StringValue => value.StringValue,
            Value.KindOneofCase.BoolValue => value.BoolValue,
            Value.KindOneofCase.IntegerValue => value.IntegerValue,
            Value.KindOneofCase.DoubleValue => value.DoubleValue,
            Value.KindOneofCase.ListValue => value.ListValue.Values.Select(FromPayloadValue).ToArray(),
            Value.KindOneofCase.None => null,
            _ => value.ToString()
        };
    }
}
