<#
.SYNOPSIS
Runs the repository's structural and optional build validation gates.
#>
[CmdletBinding()]
param(
    [switch]$SkipBuild
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$repositoryRoot = [System.IO.Path]::GetFullPath(
    (Join-Path $PSScriptRoot '..\..')
)
$solutionPath = Join-Path $repositoryRoot 'CanDoItAll.AgentFramework.Rag.slnx'
$nugetConfigPath = Join-Path $repositoryRoot 'NuGet.config'
$failures = [System.Collections.Generic.List[string]]::new()

$requiredFiles = @(
    '.editorconfig',
    '.gitattributes',
    '.gitignore',
    'AGENTS.md',
    'CONTRIBUTING.md',
    'SECURITY.md',
    'LICENSE',
    'README.md',
    'NuGet.config',
    'Directory.Build.props',
    'Directory.Build.targets',
    'global.json',
    '.github\workflows\ci.yml',
    'docs\architecture.md',
    'docs\architecture-review.md',
    'docs\publishing.md',
    'docs\ui-validation.md',
    'docs\package-icon.png',
    'src\CanDoItAll.AgentFramework.Rag.Driver\README.md',
    'src\CanDoItAll.AgentFramework.Rag.Qdrant\README.md',
    'tools\deployment\nugets\Build-NuGets.ps1',
    'tools\validation\Test-NuGetPackages.ps1'
)
foreach ($relativePath in $requiredFiles) {
    $path = Join-Path $repositoryRoot $relativePath
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        $failures.Add("Required file is missing: $relativePath")
    }
}

$projectExpectations = @(
    [pscustomobject]@{
        Path = 'src\CanDoItAll.AgentFramework.Rag.Driver\CanDoItAll.AgentFramework.Rag.Driver.csproj'
        IsPackable = 'true'
        PackageId = 'CanDoItAll.AgentFramework.Rag.Driver'
    }
    [pscustomobject]@{
        Path = 'src\CanDoItAll.AgentFramework.Rag.Qdrant\CanDoItAll.AgentFramework.Rag.Qdrant.csproj'
        IsPackable = 'true'
        PackageId = 'CanDoItAll.AgentFramework.Rag.Qdrant'
    }
    [pscustomobject]@{
        Path = 'src\CanDoItAll.AgentFramework.Rag.Sample\CanDoItAll.AgentFramework.Rag.Sample.csproj'
        IsPackable = 'false'
        PackageId = $null
    }
    [pscustomobject]@{
        Path = 'src\CanDoItAll.AgentFramework.Rag.Sandbox\CanDoItAll.AgentFramework.Rag.Sandbox.csproj'
        IsPackable = 'false'
        PackageId = $null
    }
    [pscustomobject]@{
        Path = 'tests\CanDoItAll.AgentFramework.Rag.Tests\CanDoItAll.AgentFramework.Rag.Tests.csproj'
        IsPackable = 'false'
        PackageId = $null
    }
)
$repositoryBoundary = $repositoryRoot.TrimEnd(
    [System.IO.Path]::DirectorySeparatorChar,
    [System.IO.Path]::AltDirectorySeparatorChar
) + [System.IO.Path]::DirectorySeparatorChar

foreach ($expectation in $projectExpectations) {
    $projectPath = Join-Path $repositoryRoot $expectation.Path
    [xml]$project = Get-Content -Raw -LiteralPath $projectPath
    $isPackable = @($project.Project.PropertyGroup.IsPackable) |
        Where-Object { $null -ne $_ } |
        Select-Object -First 1
    if ([string]$isPackable -cne $expectation.IsPackable) {
        $failures.Add(
            "$($expectation.Path) IsPackable is '$isPackable', expected '$($expectation.IsPackable)'."
        )
    }

    if ($null -ne $expectation.PackageId) {
        $packageId = @($project.Project.PropertyGroup.PackageId) |
            Where-Object { $null -ne $_ } |
            Select-Object -First 1
        $description = @($project.Project.PropertyGroup.Description) |
            Where-Object { $null -ne $_ } |
            Select-Object -First 1
        $packageReadme = @($project.Project.PropertyGroup.PackageReadmeFile) |
            Where-Object { $null -ne $_ } |
            Select-Object -First 1

        if ([string]$packageId -cne $expectation.PackageId) {
            $failures.Add("$($expectation.Path) has incorrect PackageId '$packageId'.")
        }
        if ([string]::IsNullOrWhiteSpace([string]$description)) {
            $failures.Add("$($expectation.Path) must define a package Description.")
        }
        if ([string]$packageReadme -cne 'README.md') {
            $failures.Add("$($expectation.Path) must package README.md.")
        }
    }

    foreach ($reference in @($project.SelectNodes('/Project/ItemGroup/ProjectReference'))) {
        $include = [string]$reference.Include
        $resolvedReference = [System.IO.Path]::GetFullPath(
            (Join-Path (Split-Path $projectPath -Parent) $include)
        )
        if (-not $resolvedReference.StartsWith(
            $repositoryBoundary,
            [StringComparison]::OrdinalIgnoreCase
        )) {
            $failures.Add("$($expectation.Path) references sibling source '$include'.")
        }
    }
}

$sandboxProjectPath = Join-Path $repositoryRoot (
    'src\CanDoItAll.AgentFramework.Rag.Sandbox\CanDoItAll.AgentFramework.Rag.Sandbox.csproj'
)
[xml]$sandboxProject = Get-Content -Raw -LiteralPath $sandboxProjectPath
$baseLibReference = @($sandboxProject.SelectNodes('/Project/ItemGroup/PackageReference')) |
    Where-Object { $_.Include -ceq 'CanDoItAll.Components.BaseLib' } |
    Select-Object -First 1
if ($null -eq $baseLibReference -or [string]$baseLibReference.Version -cne '0.1.15') {
    $failures.Add('Sandbox must consume CanDoItAll.Components.BaseLib 0.1.15 from NuGet.')
}

$driverProjectText = Get-Content -Raw -LiteralPath (
    Join-Path $repositoryRoot 'src\CanDoItAll.AgentFramework.Rag.Driver\CanDoItAll.AgentFramework.Rag.Driver.csproj'
)
if ($driverProjectText.IndexOf('Qdrant.Client', [StringComparison]::Ordinal) -ge 0) {
    $failures.Add('Driver must not reference Qdrant.Client.')
}

$serviceLocatorFiles = @(
    'src\CanDoItAll.AgentFramework.Rag.Driver\Abstractions\IRagDriverProvider.cs',
    'src\CanDoItAll.AgentFramework.Rag.Driver\Factories\RagDriverFactory.cs'
)
foreach ($relativePath in $serviceLocatorFiles) {
    $content = Get-Content -Raw -LiteralPath (Join-Path $repositoryRoot $relativePath)
    if ($content.IndexOf('IServiceProvider', [StringComparison]::Ordinal) -ge 0) {
        $failures.Add("$relativePath contains service location.")
    }
}

$mainLayoutPath = Join-Path $repositoryRoot (
    'src\CanDoItAll.AgentFramework.Rag.Sandbox\Components\Layout\MainLayout.razor'
)
if (-not (Test-Path -LiteralPath $mainLayoutPath -PathType Leaf)) {
    $failures.Add('Sandbox MainLayout.razor is missing.')
}
else {
    $mainLayout = Get-Content -Raw -LiteralPath $mainLayoutPath
    foreach ($requiredComponent in @('<ThemeHost', '<Layout', '<SideMenu', '<Body')) {
        if ($mainLayout.IndexOf($requiredComponent, [StringComparison]::Ordinal) -lt 0) {
            $failures.Add("Sandbox MainLayout does not use $requiredComponent.")
        }
    }
}

$sandboxPageFiles = Get-ChildItem -LiteralPath (
    Join-Path $repositoryRoot 'src\CanDoItAll.AgentFramework.Rag.Sandbox\Components\Pages'
) -Filter '*.razor' -File
foreach ($pageFile in $sandboxPageFiles) {
    $content = Get-Content -Raw -LiteralPath $pageFile.FullName
    if ($content -match '<Tabs(?:\s|>)') {
        $failures.Add("$($pageFile.Name) still uses Tabs as the sandbox primary menu.")
    }
}

if ($failures.Count -gt 0) {
    throw "Repository structure validation failed:`n - $($failures -join "`n - ")"
}

if (-not $SkipBuild) {
    & dotnet restore $solutionPath --configfile $nugetConfigPath
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet restore failed with exit code $LASTEXITCODE."
    }

    & dotnet build $solutionPath --configuration Release --no-restore
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet build failed with exit code $LASTEXITCODE."
    }

    & dotnet test $solutionPath --configuration Release --no-build --no-restore
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet test failed with exit code $LASTEXITCODE."
    }
}

[pscustomobject]@{
    Repository = Split-Path $repositoryRoot -Leaf
    StructuralChecks = 'Passed'
    BuildChecks = if ($SkipBuild) { 'Skipped' } else { 'Passed' }
    Status = 'Passed'
}
