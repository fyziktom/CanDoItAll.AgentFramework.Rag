# Publishing

This repository produces two public NuGet packages:

- `CanDoItAll.AgentFramework.Rag.Driver`
- `CanDoItAll.AgentFramework.Rag.Qdrant`

The console sample, Blazor sandbox, and test project are release-proof hosts,
not package outputs.

## Prerequisites

- The .NET SDK pinned by `global.json`
- PowerShell 7
- Access to nuget.org for restore

Restore through the repository-owned source configuration:

```powershell
dotnet restore CanDoItAll.AgentFramework.Rag.slnx --configfile NuGet.config
```

## Preview a package build

The repository packaging entry point supports `-WhatIf` and makes no file or
process changes during a preview:

```powershell
./tools/deployment/nugets/Build-NuGets.ps1 `
  -Version 0.2.0 `
  -WhatIf
```

## Build release candidates

Always pass the intended release version explicitly:

```powershell
./tools/deployment/nugets/Build-NuGets.ps1 `
  -Configuration Release `
  -Version 0.2.0
```

By default, the tool creates a fresh
`artifacts/packages/<version>_<timestamp>` directory. It rejects a destination
that already contains `.nupkg` or `.snupkg` files so stale artifacts cannot
satisfy the current validation run. Versions must use normalized SemVer
core/prerelease syntax without build metadata.

The version override is forwarded to restore, build, test, and pack. The
entry point selects only the Driver and Qdrant projects, creates symbol
packages, and then validates every package archive.

The validation gate proves:

- the exact expected package and symbol-package set;
- matching package and Driver dependency versions;
- repository, website, SPDX MIT license, README, and icon metadata;
- byte-identical project README and approved icon;
- portable symbols and XML documentation.

The build entry point deliberately does not publish. Publishing is a separate,
authorized operation so local validation and CI cannot push packages
accidentally.

## Publish an approved candidate

Publish the CI artifact associated with the exact reviewed commit or release
tag. CI starts from a clean checkout and records that commit in package
metadata; a local dirty-worktree package can legitimately name `HEAD` while
containing different source and must not be published.

If an explicitly approved local fallback is ever required, first prove that
`git status --porcelain` produces no output, record `git rev-parse HEAD`,
confirm the intended release tag points at that commit, and rebuild into a
fresh isolated directory. Re-run `Test-NuGetPackages.ps1` against the
candidate before pushing.

After provenance and package contents have been reviewed and a nuget.org API
key has been provided through the release environment, publish each validated
`.nupkg` from the isolated candidate directory:

```powershell
dotnet nuget push `
  artifacts/packages/0.2.0_20260729-153045123/CanDoItAll.AgentFramework.Rag.Driver.0.2.0.nupkg `
  --source https://api.nuget.org/v3/index.json `
  --api-key $env:NUGET_API_KEY `
  --skip-duplicate

dotnet nuget push `
  artifacts/packages/0.2.0_20260729-153045123/CanDoItAll.AgentFramework.Rag.Qdrant.0.2.0.nupkg `
  --source https://api.nuget.org/v3/index.json `
  --api-key $env:NUGET_API_KEY `
  --skip-duplicate
```

Do not put API keys in project files, scripts, command history, or CI logs.
Confirm the package pages and dependency metadata after nuget.org finishes
indexing the release.
