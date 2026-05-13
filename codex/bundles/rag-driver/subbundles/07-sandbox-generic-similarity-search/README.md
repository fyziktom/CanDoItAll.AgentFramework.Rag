# Sandbox Generic Similarity Search

## Status

- `Completed`

## Objective

- Add a third sandbox tab for RAG-style similarity search across a user-selected set of collections.

## Success Criteria

- Similarity search has its own BaseLib tab.
- Users can open a collection-picker dialog from the search tab.
- Double-clicking a collection adds it and closes the dialog.
- Checkbox multi-select plus confirm adds multiple collections.
- Selected collections render as removable colored chips through `TagEditor`.
- Search results show collection, record, tags, score, and text across selected collections.
- Browser proof covers the full picker and search workflow.

## Covered Inputs

- `N009`: Add a third tab for similarity search.
- `N014`: Add/remove collections included in search through a dialog and removable colored selected-collection badges.
- `R017`: Generic cross-collection similarity search.

## Prerequisites

- `06-sandbox-dialog-tabs-and-tags` status is `Completed`.
- Collection summaries include enough data for picker display and selected collection tags.

## Exact Source References

- `C:\repositories\CanDoItAll.AgentFramework.Rag\src\CanDoItAll.AgentFramework.Rag.Sandbox\Components\Pages\Home.razor`
- `C:\repositories\CanDoItAll.AgentFramework.Rag\src\CanDoItAll.AgentFramework.Rag.Sandbox\Services\RagSandboxStore.cs`
- `C:\repositories\CanDoItAll\src\CanDoItAll.Components.BaseLib\Components\Forms\TagEditor.razor`
- `C:\repositories\CanDoItAll\src\CanDoItAll.Components.BaseLib\Components\Modals\Dialog.razor`
- `C:\repositories\CanDoItAll\src\CanDoItAll.Components.BaseLib\Components\Forms\CheckBox.razor`
- `C:\repositories\CanDoItAll\src\CanDoItAll.Components.BaseLib\Components\DataVisualization\DataGrid.razor`

## Deliverables

- Search tab UI.
- Collection-picker dialog with double-click and checkbox multi-select behaviors.
- Selected-collection chip list using TagEditor.
- Store method for cross-collection similarity search.
- Browser proof artifacts for picker, removal, and search results.

## Dependency Impact

- This is the final UI closure phase for the follow-up request; weak proof here leaves the main requested RAG-style search workflow unverified.

## Validation Depth

- `UI, browser-proof, and end-to-end sandbox closure`

## Implementation Steps

1. Add cross-collection similarity search to the sandbox store.
2. Add a similarity-search tab with query, limit, selected collection chips, and result grid.
3. Add a collection-picker dialog with search/filter, checkboxes, and double-click behavior.
4. Wire TagEditor removal to update selected collections.
5. Run build/test proof.
6. Run browser proof for double-click add, checkbox multi-add, chip removal, and search results.
7. Update execution report and final bundle closure.

## Scope Exceptions

- Search remains in-memory and deterministic; it does not call live Qdrant.

## Do Not Do

- Do not introduce persistence or background indexing.
- Do not add custom chip controls instead of using TagEditor for selected collections.
- Do not require external services for search proof.

## Acceptance Checklist

- Search tab exists and is navigable.
- Add-collections dialog supports double-click single add.
- Add-collections dialog supports checkbox multi-select and confirm.
- Selected collection chips can be removed.
- Similarity results span all selected collections.

## Proof Required

- `dotnet build CanDoItAll.AgentFramework.Rag.slnx`
- `dotnet test tests\CanDoItAll.AgentFramework.Rag.Tests\CanDoItAll.AgentFramework.Rag.Tests.csproj`
- Browser route `http://localhost:5046` at large desktop viewport.
- Browser proof for collection picker and search results.
- Completed-stage bundle validator.

## Browser Validation Logging

- Route: `http://localhost:5046`
- Viewports: large desktop plus a narrow-width pass.
- Required actions: open search tab, open picker, double-click one collection, remove selected chip, reopen picker, checkbox-select multiple collections, confirm, run search, inspect result rows.
- Screenshots: search tab, picker dialog open with checkboxes, selected collection chips, populated search results.
- Review questions: picker dialog is layered and readable, selected collection chips wrap without overlap, results retain collection context, and actions remain reachable on narrow width.

## Progression Gate

- The bundle can close only when search-tab browser proof, build/test proof, raw-note closure, and completed-stage validation all agree.

## Suggested Agent Prompt

```text
Implement this subbundle only. Add the similarity search tab, collection-picker dialog, TagEditor selected collections, and cross-collection search proof. Close the bundle only after browser and validator evidence is recorded.
```
