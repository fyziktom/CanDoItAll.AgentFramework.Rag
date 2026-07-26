# Repository Agent Instructions

## Shared standards

Follow the reviewed standards in a resolved `CanDoItAll.SharedInfo` clone. This
repository owns its local implementation and documented exceptions.

Use `$apply-candoitall-shared-standards` when available. It resolves SharedInfo
from an explicit `CANDOITALL_SHAREDINFO_ROOT` or nearby sibling locations.

## Repository scope

- Keep provider-neutral contracts, models, factories, and embedding
  abstractions in `CanDoItAll.AgentFramework.Rag.Driver`.
- Keep provider SDKs and mappings in their implementation projects. Driver must
  not reference Qdrant, a UI project, or another provider implementation.
- Keep the sample and sandbox as non-packable composition and release-proof
  hosts.
- Consume released cross-repository dependencies from NuGet; do not add sibling
  source paths to shipping project files.

## Commands

- Build: `dotnet build CanDoItAll.AgentFramework.Rag.slnx --configuration Release`
- Test: `dotnet test CanDoItAll.AgentFramework.Rag.slnx --configuration Release`
- Validate: `./tools/validation/Test-Repository.ps1`
- Package: `./tools/deployment/nugets/Build-NuGets.ps1 -Version <version>`

## Safety

- Keep sibling repositories read-only unless the user explicitly requests a
  multi-repository change.
- Do not commit generated output, local settings, credentials, runtime state, or
  browser artifacts.
- Preserve repository-specific changes unrelated to the active task.
- Publishing is a separate authorized action; the build script only creates
  local package artifacts.
