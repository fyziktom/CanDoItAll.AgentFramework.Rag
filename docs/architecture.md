# Architecture

## Scope and boundaries

`CanDoItAll.AgentFramework.Rag` owns provider-neutral retrieval contracts, an
extensible RAG-driver selection seam, the Qdrant adapter, and local composition
hosts that prove the packages. It does not own embedding-provider SDKs, the
Qdrant service, or CanDoItAll's shared UI components.

The dependency direction is:

```text
Sample / Sandbox / Tests
          |
          v
Qdrant provider implementation ---> Qdrant.Client
          |
          v
Driver contracts, models, factory, and embedding abstraction
```

The Driver project must remain free of Qdrant SDK types. The Qdrant project may
reference Driver and `Qdrant.Client`. Composition hosts may reference both
implementation and contract projects.

The console sample and Blazor sandbox remain under `src` as deliberate
repository validation hosts: they are part of the release gate and exercise
the source projects directly. They are explicitly non-packable.

## Architecture decision: provider construction

Status: accepted for publishing preparation.

### Observed force

Runtime input chooses one of an extensible set of RAG providers. Before the
publishing refactor, the factory retained `IServiceProvider`, and
`IRagDriverProvider.Create` received it to resolve provider dependencies. That
made the provider contract a service locator and leaked composition mechanics
into provider behavior.

### Selected pattern

Use a narrow factory plus provider implementations constructed by dependency
injection:

- `RagDriverFactory` validates options and selects an `IRagDriverProvider`.
- Each provider receives its concrete dependencies through its constructor.
- `IRagDriverProvider.Create` receives only provider-neutral factory options.
- Provider registration remains in the implementation package's composition
  extension.

Adding another provider requires a new implementation and registration, not a
branch in `RagDriverFactory`.

### Rejected alternatives

- A builder is unnecessary because driver construction has one validated
  options object and no ordered assembly process.
- A new abstractions project would add a package and reference boundary without
  isolating a new lifecycle or SDK; the existing Driver project is already the
  SDK-free contract boundary.
- Passing `IServiceProvider`, delegates that wrap it, or calling
  `BuildServiceProvider` would preserve service location and is rejected.

### Test seam

Factory tests instantiate `RagDriverFactory` with fake providers and no service
container. Provider composition tests prove the Qdrant provider is constructible
through the public registration path. Negative tests cover an unknown provider
and duplicate provider identifiers.

## Responsibility inventory

| Former owner | Responsibility after the refactor | Verification |
|---|---|---|
| `RagDriverFactory` | Validates provider-neutral options and selects an already-constructed provider | Direct factory tests require no `IServiceProvider` |
| `IRagDriverProvider` | Describes a provider and constructs its driver from provider-neutral options | Contract and fake-provider tests contain no service-container seam |
| `QdrantRagDriverProvider` | Receives its concrete dependencies through its constructor and creates Qdrant drivers | Public DI registration smoke tests |
| `QdrantRagMapper` | Removed; collection, point, payload-value, filter, payload-index, and point-id mapping each have an internal top-level owner | Direct tests target all six mapper owners; a public-API test guards their visibility |
| `RagSandboxStore` | Orchestrates session CRUD and vector caching; models, state, seed data, projections, and similarity calculation are independent top-level owners | Similarity and caching tests use injected seams |
| `Home.razor` | Removed; Collections, Records, and Similarity Search are routed pages under one `MainLayout` | Route, history, menu, dialog, and workflow browser proof |
| `app.css` | Contains only the document baseline, tokenized Blazor error styling, and one narrow responsive integration rule | Application surfaces use BaseLib layout and content primitives without inline styles |

## UI composition decision

The Blazor sandbox uses one `ThemeHost` and one full-height, overflow-locked
`Layout`. `SideMenu` sits directly beside `Body`. `Body` defines the
application overflow boundary, while the active BaseLib `PageScaffold` owns
page-content scrolling when the viewport is constrained. The document and
child content surfaces do not introduce competing scroll regions.

The stable menu id is `rag-sandbox-primary`. Menu selections navigate to the
routed work areas, and route changes, including browser history, synchronize
the selected item through `SideMenuService`.

BaseLib 0.1.15 changes the menu into a full-width top shell below 768 pixels.
One application integration rule changes the containing `Layout` from a row to
a column at the same breakpoint so `Body` retains the full viewport width. It
does not restyle the menu or its contents.

The first viewport contains one compact `PageHeader` and the active primary
surface. Collection and record editing remains in controlled BaseLib dialogs.
Supporting counts use badges rather than metric cards. The record page uses a
list-detail composition; similarity controls and results use a two-column grid.

## Publishing boundary

Only these projects produce packages:

- `CanDoItAll.AgentFramework.Rag.Driver`
- `CanDoItAll.AgentFramework.Rag.Qdrant`

Each package owns a README beside its project file. Repository-wide build
targets embed the repository license and approved package icon. The sample,
sandbox, and test projects are non-packable. Cross-repository UI dependencies
come from nuget.org; shipping project files never reference sibling source
paths.

## Architecture acceptance criteria

- No production factory or provider contract stores or accepts
  `IServiceProvider`.
- Driver has no reference to `Qdrant.Client` or a provider implementation.
- No project-reference cycle exists.
- Partial classes are limited to the framework-bound Razor
  `.razor`/`.razor.cs` pairing; no general partial or nested architecture
  boundary is introduced.
- Qdrant mapping responsibilities have cohesive top-level owners and direct
  tests.
- Similarity math is testable without constructing `RagSandboxStore`.
- The sandbox uses BaseLib `SideMenu` instead of the former top-level `Tabs`
  menu and has one active scroll surface.
- BaseLib is consumed from nuget.org.
- Package archives contain the exact requested version, package README,
  repository license, corporate icon, source metadata, symbols, and XML
  documentation.
