using CanDoItAll.AgentFramework.Rag.Sandbox.Services;
using CanDoItAll.Components.BaseLib;

namespace CanDoItAll.AgentFramework.Rag.Sandbox.Components.Pages;

public partial class SimilaritySearch
{
    private readonly HashSet<string> pickerSelections = new(StringComparer.OrdinalIgnoreCase);

    private bool isCollectionPickerOpen;
    private bool isSimilaritySearching;
    private string? pickerSearch;
    private IReadOnlyList<string> similarityCollections = Array.Empty<string>();
    private string similarityQuery = "approval requirements for invoices";
    private string similarityLimit = "10";
    private IReadOnlyList<RagSandboxSearchHit> similarityResults = [];
    private string? errorMessage;
    private string lastAction = "Ready";

    private IReadOnlyList<RagSandboxCollectionSummary> AllCollections => Store.SearchCollections(null);
    private IReadOnlyList<RagSandboxCollectionSummary> PickerCollections => Store.SearchCollections(pickerSearch);
    private IReadOnlyList<string> AllCollectionNames => AllCollections
        .Select(collection => collection.Name)
        .ToArray();
    private string LastActionTone => errorMessage is null ? "success" : "danger";

    protected override void OnInitialized()
    {
        var firstCollection = AllCollections.FirstOrDefault();
        if (firstCollection is not null)
        {
            similarityCollections = [firstCollection.Name];
        }
    }

    private void OpenCollectionPickerDialog()
    {
        pickerSearch = null;
        pickerSelections.Clear();
        errorMessage = null;
        isCollectionPickerOpen = true;
    }

    private Task AddCollectionFromPickerAndCloseAsync(string collectionName)
    {
        AddSimilarityCollections([collectionName]);
        isCollectionPickerOpen = false;
        lastAction = $"Added {collectionName}";
        return Task.CompletedTask;
    }

    private Task AddPickerSelectionsAsync()
    {
        var selectionCount = pickerSelections.Count;
        AddSimilarityCollections(pickerSelections);
        pickerSelections.Clear();
        isCollectionPickerOpen = false;
        lastAction = $"Added {selectionCount} collections";
        return Task.CompletedTask;
    }

    private Task CloseCollectionPickerDialogAsync()
    {
        isCollectionPickerOpen = false;
        return Task.CompletedTask;
    }

    private void TogglePickerSelection(string collectionName)
    {
        if (!pickerSelections.Add(collectionName))
        {
            pickerSelections.Remove(collectionName);
        }
    }

    private Task HandleSimilarityCollectionsChangedAsync(IReadOnlyList<string> values)
    {
        var validNames = AllCollectionNames.ToHashSet(StringComparer.OrdinalIgnoreCase);
        similarityCollections = values
            .Where(validNames.Contains)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();

        similarityResults = similarityResults
            .Where(hit => similarityCollections.Contains(
                hit.CollectionName,
                StringComparer.OrdinalIgnoreCase))
            .ToArray();

        return Task.CompletedTask;
    }

    private void AddSimilarityCollections(IEnumerable<string> names)
    {
        var validNames = AllCollectionNames.ToHashSet(StringComparer.OrdinalIgnoreCase);
        similarityCollections = similarityCollections
            .Concat(names)
            .Where(validNames.Contains)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();
    }

    private void ClearSimilarityCollections()
    {
        similarityCollections = Array.Empty<string>();
        similarityResults = [];
        lastAction = "Search scope cleared";
    }

    private async Task SearchSimilaritiesAsync()
    {
        if (isSimilaritySearching)
        {
            return;
        }

        isSimilaritySearching = true;
        try
        {
            await RunActionAsync(async () =>
            {
                similarityResults = await Store.SearchAcrossCollectionsAsync(
                    similarityCollections,
                    similarityQuery,
                    ParseLimit());

                lastAction = $"Found {similarityResults.Count} results";
            });
        }
        finally
        {
            isSimilaritySearching = false;
        }
    }

    private async Task RunActionAsync(Func<Task> action)
    {
        errorMessage = null;
        try
        {
            await action();
        }
        catch (Exception exception)
        {
            errorMessage = exception.Message;
            lastAction = "Failed";
        }
    }

    private int ParseLimit()
    {
        if (int.TryParse(similarityLimit, out var limit) && limit > 0)
        {
            return Math.Clamp(limit, 1, 100);
        }

        throw new InvalidOperationException("Limit must be a positive whole number.");
    }

    private static BadgeTone ResolveScoreTone(double score)
        => score >= 0.5
            ? BadgeTone.Success
            : BadgeTone.Warn;
}
