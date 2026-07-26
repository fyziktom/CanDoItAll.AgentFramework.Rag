<#
.SYNOPSIS
Builds and validates the repository's publishable NuGet packages.

.DESCRIPTION
Restores, builds, tests, and packs only the Driver and Qdrant projects. The
script creates local artifacts and never publishes them.

.PARAMETER Configuration
Build configuration. The default is Release.

.PARAMETER OutputDirectory
Absolute or repository-relative package destination. When omitted, a
versioned run directory is created below artifacts/packages.

.PARAMETER NoRestore
Skips restore when the caller guarantees it has completed.

.PARAMETER Version
Overrides the package version without editing committed project files. The
override is forwarded to restore, build, test, and pack.

.EXAMPLE
./tools/deployment/nugets/Build-NuGets.ps1 -Version 0.2.0

.EXAMPLE
./tools/deployment/nugets/Build-NuGets.ps1 -Version 0.2.0-preview.1 -WhatIf
#>
[CmdletBinding(SupportsShouldProcess, ConfirmImpact = 'Medium')]
param(
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Release',

    [string]$OutputDirectory,

    [switch]$NoRestore,

    [string]$Version = ''
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$repositoryRoot = [System.IO.Path]::GetFullPath(
    (Join-Path $PSScriptRoot '..\..\..')
)
$solutionPath = Join-Path $repositoryRoot 'CanDoItAll.AgentFramework.Rag.slnx'
$nugetConfigPath = Join-Path $repositoryRoot 'NuGet.config'
$directoryBuildPropsPath = Join-Path $repositoryRoot 'Directory.Build.props'
$packageValidatorPath = Join-Path $repositoryRoot 'tools\validation\Test-NuGetPackages.ps1'
$packableProjects = @(
    Join-Path $repositoryRoot 'src\CanDoItAll.AgentFramework.Rag.Driver\CanDoItAll.AgentFramework.Rag.Driver.csproj'
    Join-Path $repositoryRoot 'src\CanDoItAll.AgentFramework.Rag.Qdrant\CanDoItAll.AgentFramework.Rag.Qdrant.csproj'
)

foreach ($requiredPath in @(
    $solutionPath,
    $nugetConfigPath,
    $directoryBuildPropsPath,
    $packageValidatorPath
) + $packableProjects) {
    if (-not (Test-Path -LiteralPath $requiredPath -PathType Leaf)) {
        throw "Required packaging input was not found: '$requiredPath'."
    }
}

$effectiveVersion = $Version.Trim()
if ([string]::IsNullOrWhiteSpace($effectiveVersion)) {
    [xml]$directoryBuildProps = Get-Content -Raw -LiteralPath $directoryBuildPropsPath
    $versionNode = $directoryBuildProps.SelectSingleNode('/Project/PropertyGroup/Version')
    if ($null -eq $versionNode -or [string]::IsNullOrWhiteSpace($versionNode.InnerText)) {
        throw "Directory.Build.props must define the committed package Version."
    }

    $effectiveVersion = $versionNode.InnerText.Trim()
}

$semanticPackageVersionPattern = (
    '^(?:0|[1-9]\d*)\.(?:0|[1-9]\d*)\.(?:0|[1-9]\d*)' +
    '(?:-(?:0|[1-9]\d*|[0-9A-Za-z-]*[A-Za-z-][0-9A-Za-z-]*)' +
    '(?:\.(?:0|[1-9]\d*|[0-9A-Za-z-]*[A-Za-z-][0-9A-Za-z-]*))*)?$'
)
if ($effectiveVersion -notmatch $semanticPackageVersionPattern) {
    throw (
        "Version '$effectiveVersion' is not a supported semantic package version. " +
        'Use SemVer 2 core and prerelease identifiers without build metadata or leading zeroes.'
    )
}

$versionArguments = if ([string]::IsNullOrWhiteSpace($Version)) {
    @()
}
else {
    @("-p:Version=$effectiveVersion")
}

$outputWasSpecified = -not [string]::IsNullOrWhiteSpace($OutputDirectory)
if ($outputWasSpecified) {
    $resolvedOutputDirectory = if ([System.IO.Path]::IsPathRooted($OutputDirectory)) {
        [System.IO.Path]::GetFullPath($OutputDirectory)
    }
    else {
        [System.IO.Path]::GetFullPath((Join-Path $repositoryRoot $OutputDirectory))
    }
}
else {
    $runTimestamp = Get-Date -Format 'yyyyMMdd-HHmmssfff'
    $resolvedOutputDirectory = Join-Path $repositoryRoot (
        "artifacts\packages\${effectiveVersion}_$runTimestamp"
    )
}

$normalizedRepositoryRoot = $repositoryRoot.TrimEnd(
    [System.IO.Path]::DirectorySeparatorChar,
    [System.IO.Path]::AltDirectorySeparatorChar
)
$normalizedOutputDirectory = $resolvedOutputDirectory.TrimEnd(
    [System.IO.Path]::DirectorySeparatorChar,
    [System.IO.Path]::AltDirectorySeparatorChar
)
if ($normalizedOutputDirectory -eq $normalizedRepositoryRoot) {
    throw 'The package output directory cannot be the repository root.'
}
if (Test-Path -LiteralPath $resolvedOutputDirectory -PathType Leaf) {
    throw "The package output path is a file: '$resolvedOutputDirectory'."
}
if (Test-Path -LiteralPath $resolvedOutputDirectory -PathType Container) {
    $existingPackageArtifacts = @(
        Get-ChildItem -LiteralPath $resolvedOutputDirectory -File |
            Where-Object {
                $_.Extension -ieq '.nupkg' -or $_.Extension -ieq '.snupkg'
            }
    )
    if ($existingPackageArtifacts.Count -gt 0) {
        throw (
            "Package output directory '$resolvedOutputDirectory' already contains package artifacts. " +
            'Use a fresh output directory so stale files cannot satisfy release validation.'
        )
    }
}

$operationParts = [System.Collections.Generic.List[string]]::new()
if (-not $NoRestore) {
    $operationParts.Add('restore')
}
$operationParts.Add('build and test')
$operationParts.Add("pack and validate $($packableProjects.Count) packages at version '$effectiveVersion'")
$operation = $operationParts -join ', '

if (-not $PSCmdlet.ShouldProcess($resolvedOutputDirectory, $operation)) {
    [pscustomobject]@{
        Repository = Split-Path $repositoryRoot -Leaf
        Solution = Split-Path $solutionPath -Leaf
        Configuration = $Configuration
        PackageVersion = $effectiveVersion
        OutputDirectory = $resolvedOutputDirectory
        ProjectCount = $packableProjects.Count
        Status = 'Preview'
    }
    return
}

New-Item -ItemType Directory -Path $resolvedOutputDirectory -Force | Out-Null

if (-not $NoRestore) {
    $restoreArguments = @(
        'restore',
        $solutionPath,
        '--configfile',
        $nugetConfigPath
    ) + $versionArguments
    & dotnet @restoreArguments
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet restore failed with exit code $LASTEXITCODE."
    }
}

$buildArguments = @(
    'build',
    $solutionPath,
    '--configuration',
    $Configuration,
    '--no-restore',
    '-p:ContinuousIntegrationBuild=true'
) + $versionArguments
& dotnet @buildArguments
if ($LASTEXITCODE -ne 0) {
    throw "dotnet build failed with exit code $LASTEXITCODE."
}

$testArguments = @(
    'test',
    $solutionPath,
    '--configuration',
    $Configuration,
    '--no-build',
    '--no-restore',
    '-p:ContinuousIntegrationBuild=true'
) + $versionArguments
& dotnet @testArguments
if ($LASTEXITCODE -ne 0) {
    throw "dotnet test failed with exit code $LASTEXITCODE."
}

foreach ($projectPath in $packableProjects) {
    $packArguments = @(
        'pack',
        $projectPath,
        '--configuration',
        $Configuration,
        '--no-build',
        '--no-restore',
        '--output',
        $resolvedOutputDirectory,
        '-p:ContinuousIntegrationBuild=true'
    ) + $versionArguments
    & dotnet @packArguments
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet pack failed for '$(Split-Path $projectPath -Leaf)' with exit code $LASTEXITCODE."
    }
}

$validation = & $packageValidatorPath `
    -PackageDirectory $resolvedOutputDirectory `
    -ExpectedVersion $effectiveVersion

[pscustomobject]@{
    Repository = Split-Path $repositoryRoot -Leaf
    Solution = Split-Path $solutionPath -Leaf
    Configuration = $Configuration
    PackageVersion = $effectiveVersion
    OutputDirectory = $resolvedOutputDirectory
    Packages = @($validation.Packages)
    SymbolPackages = @($validation.SymbolPackages)
    Status = 'Succeeded'
}
