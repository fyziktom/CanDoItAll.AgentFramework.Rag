using CanDoItAll.Components.BaseLib;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;

namespace CanDoItAll.AgentFramework.Rag.Sandbox.Components.Layout;

public partial class MainLayout
{
    private const string MenuId = "rag-sandbox-primary";
    private const string NeutralMenuId = "rag-sandbox-neutral";
    private const string CollectionsItemId = "collections";
    private const string RecordsItemId = "records";
    private const string SimilaritySearchItemId = "similarity-search";

    private static readonly IReadOnlyList<ISideMenuItem> NavigationItems =
    [
        new SideMenuItemDefinition
        {
            Id = CollectionsItemId,
            Text = "Collections",
            Icon = "database",
            Description = "Manage collection settings and tags.",
            Payload = "/collections",
            OverflowBehavior = SideMenuOverflowBehavior.PreferVisible
        },
        new SideMenuItemDefinition
        {
            Id = RecordsItemId,
            Text = "Records",
            Icon = "description",
            Description = "Manage knowledge records in a collection.",
            Payload = "/records",
            OverflowBehavior = SideMenuOverflowBehavior.PreferVisible
        },
        new SideMenuItemDefinition
        {
            Id = SimilaritySearchItemId,
            Text = "Similarity search",
            Icon = "manage_search",
            Description = "Search across selected collections.",
            Payload = "/similarity-search",
            OverflowBehavior = SideMenuOverflowBehavior.PreferVisible
        }
    ];

    private string ActiveMenuId => ResolveItemId(
        Navigation.ToBaseRelativePath(Navigation.Uri)) is null
            ? NeutralMenuId
            : MenuId;

    protected override void OnInitialized()
    {
        Navigation.LocationChanged += HandleLocationChanged;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await SynchronizeMenuSelectionAsync();
        }
    }

    private async void HandleLocationChanged(object? sender, LocationChangedEventArgs args)
    {
        try
        {
            await InvokeAsync(SynchronizeMenuSelectionAsync);
        }
        catch (Exception exception)
        {
            await DispatchExceptionAsync(exception);
        }
    }

    private Task HandleMenuSelectionAsync(SideMenuSelection selection)
    {
        if (selection.Item.Payload is not string route
            || IsCurrentRoute(route))
        {
            return Task.CompletedTask;
        }

        Navigation.NavigateTo(route);
        return Task.CompletedTask;
    }

    private Task SynchronizeMenuSelectionAsync()
    {
        var itemId = ResolveItemId(Navigation.ToBaseRelativePath(Navigation.Uri));
        return itemId is null
            ? Task.CompletedTask
            : SideMenus.SelectAsync(MenuId, itemId);
    }

    private bool IsCurrentRoute(string route)
    {
        var currentPath = NormalizePath(Navigation.ToBaseRelativePath(Navigation.Uri));
        var targetPath = NormalizePath(route);
        return string.Equals(currentPath, targetPath, StringComparison.OrdinalIgnoreCase)
            || targetPath == "collections" && currentPath.Length == 0;
    }

    private static string? ResolveItemId(string relativeUri)
    {
        var path = NormalizePath(relativeUri);
        return path switch
        {
            "" or "collections" => CollectionsItemId,
            "records" => RecordsItemId,
            "similarity-search" => SimilaritySearchItemId,
            _ => null
        };
    }

    private static string NormalizePath(string value)
    {
        var queryIndex = value.IndexOfAny(['?', '#']);
        var path = queryIndex >= 0 ? value[..queryIndex] : value;
        return path.Trim('/');
    }

    public ValueTask DisposeAsync()
    {
        Navigation.LocationChanged -= HandleLocationChanged;
        return ValueTask.CompletedTask;
    }
}
