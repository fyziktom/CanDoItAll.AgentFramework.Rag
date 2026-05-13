# Sandbox Dialog Tabs And Tags

## Status

- `Completed`

## Objective

- Refactor the Blazor SSR sandbox into a BaseLib tabbed workbench with dialog-based collection and record editing, compact badges, a collection rail for records, and TagEditor support.

## Success Criteria

- Collection management and records management are split into separate BaseLib tabs.
- Collection add/edit and record add/edit happen in BaseLib dialogs.
- The page header uses compact badges instead of large summary cards.
- Records tab includes a thinner left collection list and a right record management workspace.
- Collection and record dialogs expose BaseLib `TagEditor` fields.
- Browser proof covers open dialogs, tag edits, tab navigation, and selected collection behavior.

## Covered Inputs

- `N008`: Use dialogs for collection and record add/edit forms.
- `N009`: Split collection and record management into tabs.
- `N010`: Add a left collection list in record management.
- `N011`: Replace top stat cards with badges.
- `N013`: Use BaseLib `TagEditor` for collection and record tags.
- `R011`, `R012`, `R013`, `R014`, `R016`.

## Prerequisites

- `05-driver-tag-capabilities` status is `Completed`.
- Local BaseLib `Dialog`, `Tabs`, `TagEditor`, `Badge`, `StatusBadge`, `DataGrid`, and layout component APIs have been inspected.

## Exact Source References

- `C:\repositories\CanDoItAll.AgentFramework.Rag\src\CanDoItAll.AgentFramework.Rag.Sandbox\Components\Pages\Home.razor`
- `C:\repositories\CanDoItAll.AgentFramework.Rag\src\CanDoItAll.AgentFramework.Rag.Sandbox\Services\RagSandboxStore.cs`
- `C:\repositories\CanDoItAll.AgentFramework.Rag\src\CanDoItAll.AgentFramework.Rag.Sandbox\wwwroot\app.css`
- `C:\repositories\CanDoItAll\src\CanDoItAll.Components.BaseLib\Components\Modals\Dialog.razor`
- `C:\repositories\CanDoItAll\src\CanDoItAll.Components.BaseLib\Components\Navigation\Tabs.razor`
- `C:\repositories\CanDoItAll\src\CanDoItAll.Components.BaseLib\Components\Forms\TagEditor.razor`

## Deliverables

- Tabbed sandbox layout.
- Compact status badge strip.
- Collection and record dialogs with BaseLib form controls.
- Records tab with left collection rail and right record grid/actions.
- Store model updates for collection and record tags.
- Browser proof artifacts for the updated workflows.

## Dependency Impact

- `07-sandbox-generic-similarity-search` depends on this phase because it reuses selected collection state, collection tags, tab shell, and TagEditor chip behavior.

## Validation Depth

- `Critical UI foundation`

## Implementation Steps

1. Replace top `SummaryTiles` with compact badges.
2. Wrap collection, record, and future search surfaces in BaseLib `Tabs`.
3. Move collection add/edit form into a `Dialog`.
4. Move record add/edit form into a `Dialog`.
5. Add `TagEditor` fields to collection and record dialogs.
6. Refactor records tab into a left collection rail and a right record workspace.
7. Update the in-memory store summaries and edit models to carry tags.
8. Build and run browser proof for the updated tab and dialog workflows.

## Scope Exceptions

- This phase does not own the third similarity-search tab behavior beyond reserving the tab shell if needed.

## Do Not Do

- Do not add persistent storage.
- Do not require Qdrant or external embedding services to run the sandbox.
- Do not implement custom modal or tabs markup when BaseLib components can express the flow.

## Acceptance Checklist

- Collection dialog can add and update a collection with tags.
- Record dialog can add and update a record with tags.
- Tabs separate collection and record workflows.
- Records tab collection rail changes the right record list.
- Badge strip replaces the prior large stat cards.

## Proof Required

- `dotnet build CanDoItAll.AgentFramework.Rag.slnx`
- Browser route `http://localhost:5046` at large desktop viewport.
- Browser proof opening collection and record dialogs.
- Screenshot review for tabs, compact badges, left rail, and dialog layering.
- Narrow viewport follow-up if the layout affects mobile widths.

## Browser Validation Logging

- Route: `http://localhost:5046`
- Viewports: large desktop plus a narrow-width pass.
- Required actions: switch tabs, open collection dialog, edit tags, save, open record dialog, edit tags, save, select another collection in the left rail.
- Screenshots: main tabbed layout, collection dialog open, record dialog open, records tab left rail.
- Review questions: dialogs are readable, tabs do not overflow, TagEditor chips fit, badge text does not overlap, and left rail remains thinner than the record workspace on desktop.

## Progression Gate

- `07-sandbox-generic-similarity-search` may start only after browser proof confirms tab, dialog, badge, TagEditor, and left-rail behavior.

## Suggested Agent Prompt

```text
Implement this subbundle only. Refactor the sandbox UI to BaseLib dialogs, tabs, compact badges, records left rail, and TagEditor-backed collection/record tags. Capture browser proof before moving to similarity search.
```
