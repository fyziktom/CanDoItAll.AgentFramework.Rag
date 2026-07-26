using CanDoItAll.AgentFramework.Rag.Driver.Models;
using CanDoItAll.AgentFramework.Rag.Sandbox.Services;

namespace CanDoItAll.AgentFramework.Rag.Sandbox.Components.Pages;

public partial class Collections
{
    private static readonly RagDistanceMetric[] DistanceOptions = Enum.GetValues<RagDistanceMetric>();

    private string? collectionSearch;
    private bool isCollectionDialogOpen;
    private bool isCollectionSaving;
    private string? collectionOriginalName;
    private string collectionName = string.Empty;
    private string collectionDescription = string.Empty;
    private string collectionVectorSize = "64";
    private RagDistanceMetric collectionDistance = RagDistanceMetric.Cosine;
    private IReadOnlyList<string> collectionTags = Array.Empty<string>();
    private string? collectionDialogError;
    private string? errorMessage;
    private string lastAction = "Ready";

    private IReadOnlyList<RagSandboxCollectionSummary> AllCollections => Store.SearchCollections(null);
    private IReadOnlyList<RagSandboxCollectionSummary> FilteredCollections => Store.SearchCollections(collectionSearch);
    private IReadOnlyList<string> CollectionTagSuggestions => AllCollections
        .SelectMany(collection => collection.Tags)
        .Distinct(StringComparer.OrdinalIgnoreCase)
        .OrderBy(tag => tag, StringComparer.Ordinal)
        .ToArray();
    private int TotalRecordCount => AllCollections.Sum(collection => collection.RecordCount);
    private string LastActionTone => errorMessage is null && collectionDialogError is null
        ? "success"
        : "danger";
    private string CollectionDialogTitle => string.IsNullOrWhiteSpace(collectionOriginalName)
        ? "Add collection"
        : "Edit collection";
    private string CollectionSaveText => string.IsNullOrWhiteSpace(collectionOriginalName)
        ? "Add collection"
        : "Update collection";

    private void OpenRecords(string collectionName)
    {
        Navigation.NavigateTo($"/records?collection={Uri.EscapeDataString(collectionName)}");
    }

    private void OpenNewCollectionDialog()
    {
        collectionOriginalName = null;
        collectionName = string.Empty;
        collectionDescription = string.Empty;
        collectionVectorSize = "64";
        collectionDistance = RagDistanceMetric.Cosine;
        collectionTags = Array.Empty<string>();
        collectionDialogError = null;
        errorMessage = null;
        isCollectionDialogOpen = true;
    }

    private void OpenEditCollectionDialog(RagSandboxCollectionSummary collection)
    {
        collectionOriginalName = collection.Name;
        collectionName = collection.Name;
        collectionDescription = collection.Description;
        collectionVectorSize = collection.VectorSize.ToString();
        collectionDistance = collection.Distance;
        collectionTags = collection.Tags.ToArray();
        collectionDialogError = null;
        errorMessage = null;
        isCollectionDialogOpen = true;
    }

    private async Task SaveCollectionAsync()
    {
        if (isCollectionSaving)
        {
            return;
        }

        errorMessage = null;
        collectionDialogError = null;
        isCollectionSaving = true;
        try
        {
            var isNew = string.IsNullOrWhiteSpace(collectionOriginalName);
            var summary = await Store.SaveCollectionAsync(new RagSandboxCollectionEditModel
            {
                OriginalName = collectionOriginalName,
                Name = collectionName,
                Description = collectionDescription,
                Tags = collectionTags,
                VectorSize = ParseVectorSize(),
                Distance = collectionDistance
            });

            lastAction = isNew
                ? $"Added {summary.Name}"
                : $"Updated {summary.Name}";
            isCollectionDialogOpen = false;
        }
        catch (Exception exception)
        {
            collectionDialogError = exception.Message;
            lastAction = "Failed";
        }
        finally
        {
            isCollectionSaving = false;
        }
    }

    private void DeleteCollection(string collectionName)
    {
        errorMessage = null;
        if (Store.DeleteCollection(collectionName))
        {
            lastAction = $"Deleted {collectionName}";
        }
    }

    private Task HandleCollectionTagsChangedAsync(IReadOnlyList<string> value)
    {
        collectionTags = value;
        return Task.CompletedTask;
    }

    private Task CloseCollectionDialogAsync()
    {
        if (isCollectionSaving)
        {
            return Task.CompletedTask;
        }

        isCollectionDialogOpen = false;
        collectionDialogError = null;
        return Task.CompletedTask;
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

    private int ParseVectorSize()
    {
        if (int.TryParse(collectionVectorSize, out var vectorSize) && vectorSize > 0)
        {
            return vectorSize;
        }

        throw new InvalidOperationException("Vector size must be a positive whole number.");
    }
}
