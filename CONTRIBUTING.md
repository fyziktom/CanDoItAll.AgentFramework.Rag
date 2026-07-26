# Contributing

This repository accepts code contributions only from partners who have been
explicitly approved by the maintainer. Unsolicited pull requests are not
accepted.

To discuss becoming an approved partner, contact the maintainer on LinkedIn
using the handle `fyziktom`. Wait for approval before preparing or opening a
pull request.

## Development setup

1. Install the .NET SDK pinned by `global.json`.
2. Restore packages from the repository-owned `NuGet.config`.
3. Run commands from the repository root.

## Validation

```powershell
dotnet restore CanDoItAll.AgentFramework.Rag.slnx --configfile NuGet.config
dotnet build CanDoItAll.AgentFramework.Rag.slnx --configuration Release --no-restore
dotnet test CanDoItAll.AgentFramework.Rag.slnx --configuration Release --no-build --no-restore
./tools/validation/Test-Repository.ps1 -SkipBuild
```

For package changes, also run:

```powershell
./tools/deployment/nugets/Build-NuGets.ps1 -Version 0.0.0-local
```

## Architecture rules

- Preserve the dependency direction documented in
  [docs/architecture.md](docs/architecture.md).
- Keep provider SDKs out of the provider-neutral Driver package.
- Use constructor injection and narrow factories; do not introduce service
  location into core behavior.
- Add direct unit tests and a negative case for extracted responsibilities.
- Keep generated output and local state out of Git.
- Update documentation when public behavior or package contracts change.

## Pull requests

- Open a pull request only after partner approval.
- Keep changes focused.
- Add or update tests for behavior changes.
- Describe public API and migration effects.
- Include the exact validation commands and results.
