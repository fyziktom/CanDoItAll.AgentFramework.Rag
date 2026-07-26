using CanDoItAll.AgentFramework.Rag.Sandbox.Services;
using CanDoItAll.Components.BaseLib;
using Microsoft.AspNetCore.Components;

namespace CanDoItAll.AgentFramework.Rag.Sandbox.Components.Pages;

public partial class Records
{
    private string? selectedCollectionName;
    private string? recordSearch;
    private bool isRecordDialogOpen;
    private bool isRecordSaving;
    private bool isRecordSearching;
    private string? recordOriginalId;
    private string recordId = string.Empty;
    private string recordText = string.Empty;
    private string recordMetadata = string.Empty;
    private IReadOnlyList<string> recordTags = Array.Empty<string>();
    private IReadOnlyList<RagSandboxRecordSummary> recordResults = [];
    private string? recordDialogError;
    private string? errorMessage;
    private string lastAction = "Ready";

    [SupplyParameterFromQuery(Name = "collection")]
    public string? CollectionQuery { get; set; }

    private IReadOnlyList<RagSandboxCollectionSummary> AllCollections => Store.SearchCollections(null);
    private int TotalRecordCount => AllCollections.Sum(collection => collection.RecordCount);
    private bool HasSelectedCollection => !string.IsNullOrWhiteSpace(selectedCollectionName);
    private string SelectedCollectionLabel => selectedCollectionName ?? "No collection selected";
    private string LastActionTone => errorMessage is null && recordDialogError is null
        ? "success"
        : "danger";
    private string RecordsPanelTitle => HasSelectedCollection
        ? $"Records in {selectedCollectionName}"
        : "Records";
    private string RecordsPanelDescription => HasSelectedCollection
        ? "Search, add, update, and delete records for the selected collection."
        : "Select a collection to view records.";
    private string RecordDialogTitle => string.IsNullOrWhiteSpace(recordOriginalId)
        ? "Add record"
        : "Edit record";
    private string RecordDialogSubtitle => HasSelectedCollection
        ? $"Collection: {selectedCollectionName}"
        : "Select a collection before saving.";
    private string RecordSaveText => string.IsNullOrWhiteSpace(recordOriginalId)
        ? "Add record"
        : "Update record";
    private IReadOnlyList<string> RecordTagSuggestions => HasSelectedCollection
        ? Store.GetRecords(selectedCollectionName!)
            .SelectMany(record => record.Tags)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(tag => tag, StringComparer.Ordinal)
            .ToArray()
        : Array.Empty<string>();

    protected override void OnParametersSet()
    {
        var requestedCollection = string.IsNullOrWhiteSpace(CollectionQuery)
            ? null
            : AllCollections.FirstOrDefault(collection =>
                string.Equals(collection.Name, CollectionQuery, StringComparison.OrdinalIgnoreCase));
        var nextCollection = requestedCollection ?? AllCollections.FirstOrDefault();

        if (nextCollection is null)
        {
            selectedCollectionName = null;
            recordResults = [];
            return;
        }

        if (!string.Equals(nextCollection.Name, selectedCollectionName, StringComparison.OrdinalIgnoreCase))
        {
            SelectCollection(nextCollection);
        }
    }

    private bool IsSelectedCollection(string collectionName)
        => string.Equals(collectionName, selectedCollectionName, StringComparison.OrdinalIgnoreCase);

    private void NavigateToCollection(string collectionName)
    {
        Navigation.NavigateTo($"/records?collection={Uri.EscapeDataString(collectionName)}");
    }

    private void SelectCollection(RagSandboxCollectionSummary collection)
    {
        selectedCollectionName = collection.Name;
        recordSearch = null;
        ShowAllRecords();
    }

    private void OpenNewRecordDialog()
    {
        if (!HasSelectedCollection)
        {
            return;
        }

        recordOriginalId = null;
        recordId = string.Empty;
        recordText = string.Empty;
        recordMetadata = string.Empty;
        recordTags = Array.Empty<string>();
        recordDialogError = null;
        errorMessage = null;
        isRecordDialogOpen = true;
    }

    private void OpenEditRecordDialog(RagSandboxRecordSummary record)
    {
        recordOriginalId = record.Id;
        recordId = record.Id;
        recordText = record.Text;
        recordMetadata = record.Metadata;
        recordTags = record.Tags.ToArray();
        recordDialogError = null;
        errorMessage = null;
        isRecordDialogOpen = true;
    }

    private async Task SaveRecordAsync()
    {
        if (selectedCollectionName is null || isRecordSaving)
        {
            return;
        }

        errorMessage = null;
        recordDialogError = null;
        isRecordSaving = true;
        try
        {
            var isNew = string.IsNullOrWhiteSpace(recordOriginalId);
            var record = await Store.SaveRecordAsync(selectedCollectionName, new RagSandboxRecordEditModel
            {
                OriginalId = recordOriginalId,
                Id = recordId,
                Text = recordText,
                Metadata = recordMetadata,
                Tags = recordTags
            });

            await RefreshRecordResultsAsync();
            lastAction = isNew
                ? $"Added {record.Id}"
                : $"Updated {record.Id}";
            isRecordDialogOpen = false;
        }
        catch (Exception exception)
        {
            recordDialogError = exception.Message;
            lastAction = "Failed";
        }
        finally
        {
            isRecordSaving = false;
        }
    }

    private void DeleteRecord(string recordId)
    {
        if (selectedCollectionName is null)
        {
            return;
        }

        errorMessage = null;
        if (Store.DeleteRecord(selectedCollectionName, recordId))
        {
            ShowAllRecords();
            lastAction = $"Deleted {recordId}";
        }
    }

    private Task HandleRecordTagsChangedAsync(IReadOnlyList<string> value)
    {
        recordTags = value;
        return Task.CompletedTask;
    }

    private Task CloseRecordDialogAsync()
    {
        if (isRecordSaving)
        {
            return Task.CompletedTask;
        }

        isRecordDialogOpen = false;
        recordDialogError = null;
        return Task.CompletedTask;
    }

    private async Task SearchRecordsAsync()
    {
        if (isRecordSearching)
        {
            return;
        }

        isRecordSearching = true;
        try
        {
            await RunActionAsync(async () =>
            {
                await RefreshRecordResultsAsync();
                lastAction = $"Found {recordResults.Count} records";
            });
        }
        finally
        {
            isRecordSearching = false;
        }
    }

    private async Task RefreshRecordResultsAsync()
    {
        if (selectedCollectionName is null)
        {
            recordResults = [];
            return;
        }

        recordResults = await Store.SearchRecordsAsync(selectedCollectionName, recordSearch, 25);
    }

    private void ShowAllRecords()
    {
        recordResults = selectedCollectionName is null
            ? []
            : Store.GetRecords(selectedCollectionName);
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

    private static string FormatScore(double? score)
        => score is null ? "Stored" : score.Value.ToString("0.000");

    private static BadgeTone ResolveScoreTone(double? score)
        => score is null || score >= 0.5
            ? BadgeTone.Success
            : BadgeTone.Warn;
}
