namespace CanDoItAll.AgentFramework.Rag.Sandbox.Components.Shared;

internal static class SandboxTestIds
{
    public static string ForCollection(string prefix, string collectionName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(prefix);
        ArgumentException.ThrowIfNullOrWhiteSpace(collectionName);

        var normalizedName = new string(collectionName
            .Trim()
            .ToLowerInvariant()
            .Select(character => char.IsLetterOrDigit(character) ? character : '-')
            .ToArray())
            .Trim('-');

        return normalizedName.Length == 0
            ? prefix
            : $"{prefix}-{normalizedName}";
    }
}
