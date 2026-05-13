namespace CanDoItAll.AgentFramework.Rag.Driver.Models;

public static class RagVectorValidation
{
    public static void EnsureVectorSize(
        IReadOnlyList<float> vector,
        int? expectedVectorSize,
        string parameterName)
    {
        ArgumentNullException.ThrowIfNull(vector);

        if (vector.Count == 0)
        {
            throw new ArgumentException("Vector must not be empty.", parameterName);
        }

        if (expectedVectorSize is not null && vector.Count != expectedVectorSize.Value)
        {
            throw new ArgumentException(
                $"Vector has {vector.Count} dimensions, but {expectedVectorSize.Value} dimensions were expected.",
                parameterName);
        }

        for (var index = 0; index < vector.Count; index++)
        {
            if (!float.IsFinite(vector[index]))
            {
                throw new ArgumentException("Vector must contain only finite values.", parameterName);
            }
        }
    }
}
