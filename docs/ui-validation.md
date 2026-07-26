# Sandbox UI validation

Validation date: 2026-07-26
Target: `CanDoItAll.AgentFramework.Rag.Sandbox`

## Composition checks

- One `ThemeHost`, one viewport-locked `Layout`, and one BaseLib `SideMenu`
  compose the application shell.
- One `DialogHost`, one `Tooltip`, and one `Notification` serve all routed
  pages.
- `/` and `/collections` select Collections; `/records` selects Records;
  `/similarity-search` selects Similarity search.
- Direct navigation, menu selection, reload, and browser back navigation keep
  the selected menu item synchronized.
- Returning from a qualified Records URL to `/records` restores the default
  collection and matching visible workspace.
- Error and unknown routes use a neutral menu state rather than marking a
  workspace as current.
- The desktop collapsed-menu preference survives reload.
- Async save and search actions expose BaseLib busy/loading state, and
  collection-scoped test selectors are unique.
- No former top-level `Tabs` or inline `style` attributes remain. The only
  sandbox shell class is the stable responsive integration hook described
  below.

## Viewport checks

| Viewport | Result |
|---|---|
| 1440 x 1000 | Expanded and collapsed desktop menu, all routed pages, dialogs, and primary workflows fit |
| 1440 x 650 | Collection and record dialogs fit without document overflow |
| 1440 x 500 | The BaseLib page scaffold is the only active content scroll surface |
| 766 x 900 | SideMenu becomes the full-width mobile top shell and Body retains full width with no horizontal overflow |

The only structural application rule is the 767-pixel layout-direction bridge
on `.rag-sandbox-shell` in `wwwroot/app.css`. It follows BaseLib 0.1.15's
mobile SideMenu breakpoint and changes only the containing shell from row to
column.

## Workflow checks

- Created a record, verified its count and grid row, then deleted it and
  confirmed the seeded record count was restored.
- Ran similarity search against one collection and received two results.
- Added a second collection through the BaseLib multi-picker and received
  three results.
- Opened and canceled collection and record dialogs at constrained desktop
  height.
- Triggered invalid collection and record saves and verified each failure
  remained visible and reachable inside its still-open dialog.
- A fresh browser tab reported no console errors after the final application
  restart.

The validation mutation was removed, so the running sandbox contains only its
seeded data.
