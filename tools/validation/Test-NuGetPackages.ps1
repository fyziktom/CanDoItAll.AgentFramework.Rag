<#
.SYNOPSIS
Validates the content and metadata of locally built RAG packages.
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [string]$PackageDirectory,

    [Parameter(Mandatory)]
    [string]$ExpectedVersion
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$repositoryRoot = [System.IO.Path]::GetFullPath(
    (Join-Path $PSScriptRoot '..\..')
)
$resolvedPackageDirectory = if ([System.IO.Path]::IsPathRooted($PackageDirectory)) {
    [System.IO.Path]::GetFullPath($PackageDirectory)
}
else {
    [System.IO.Path]::GetFullPath((Join-Path $repositoryRoot $PackageDirectory))
}

if (-not (Test-Path -LiteralPath $resolvedPackageDirectory -PathType Container)) {
    throw "Package directory was not found: '$resolvedPackageDirectory'."
}

$expectedPackages = @(
    [pscustomobject]@{
        Id = 'CanDoItAll.AgentFramework.Rag.Driver'
        ProjectPath = Join-Path $repositoryRoot 'src\CanDoItAll.AgentFramework.Rag.Driver\CanDoItAll.AgentFramework.Rag.Driver.csproj'
        ReadmePath = Join-Path $repositoryRoot 'src\CanDoItAll.AgentFramework.Rag.Driver\README.md'
        ExpectedDependencyId = $null
        RequiredTags = @('CanDoItAll', 'RAG', 'retrieval')
    }
    [pscustomobject]@{
        Id = 'CanDoItAll.AgentFramework.Rag.Qdrant'
        ProjectPath = Join-Path $repositoryRoot 'src\CanDoItAll.AgentFramework.Rag.Qdrant\CanDoItAll.AgentFramework.Rag.Qdrant.csproj'
        ReadmePath = Join-Path $repositoryRoot 'src\CanDoItAll.AgentFramework.Rag.Qdrant\README.md'
        ExpectedDependencyId = 'CanDoItAll.AgentFramework.Rag.Driver'
        RequiredTags = @('CanDoItAll', 'RAG', 'Qdrant')
    }
)
$licensePath = Join-Path $repositoryRoot 'LICENSE'
$iconPath = Join-Path $repositoryRoot 'docs\package-icon.png'
$failures = [System.Collections.Generic.List[string]]::new()
$packagePaths = [System.Collections.Generic.List[string]]::new()
$symbolPackagePaths = [System.Collections.Generic.List[string]]::new()

Add-Type -AssemblyName System.IO.Compression.FileSystem

function Get-EntryBytes {
    param(
        [Parameter(Mandatory)]
        [System.IO.Compression.ZipArchive]$Archive,

        [Parameter(Mandatory)]
        [string]$EntryName
    )

    $entry = $Archive.Entries |
        Where-Object { $_.FullName -ceq $EntryName } |
        Select-Object -First 1
    if ($null -eq $entry) {
        return $null
    }

    $entryStream = $entry.Open()
    $memoryStream = [System.IO.MemoryStream]::new()
    try {
        $entryStream.CopyTo($memoryStream)
        return ,$memoryStream.ToArray()
    }
    finally {
        $memoryStream.Dispose()
        $entryStream.Dispose()
    }
}

function Get-BytesHash {
    param([byte[]]$Bytes)

    $sha256 = [System.Security.Cryptography.SHA256]::Create()
    try {
        $hash = $sha256.ComputeHash($Bytes)
        return (($hash | ForEach-Object { $_.ToString('X2') }) -join '')
    }
    finally {
        $sha256.Dispose()
    }
}

function Get-NodeInnerText {
    param([System.Xml.XmlNode]$Node)

    if ($null -eq $Node) {
        return $null
    }

    return $Node.InnerText
}

function Test-ExpectedDependencyVersion {
    param(
        [string]$ActualVersion,
        [string]$ExpectedMinimumVersion
    )

    return $ActualVersion -ceq $ExpectedMinimumVersion -or
        $ActualVersion -ceq "[$ExpectedMinimumVersion, )" -or
        $ActualVersion -ceq "[$ExpectedMinimumVersion,)"
}

$expectedLicenseHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $licensePath).Hash
$expectedIconHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $iconPath).Hash
$approvedIconHash = '02B338424A63193ECE3E25BC7E15A1E8F382E3E64C6DF80D24279C0C0FDA130E'
if ($expectedIconHash -cne $approvedIconHash) {
    $failures.Add(
        "docs/package-icon.png does not match the approved SharedInfo corporate icon. " +
        "Expected SHA-256 '$approvedIconHash', found '$expectedIconHash'."
    )
}
$actualRegularPackages = @(
    Get-ChildItem -LiteralPath $resolvedPackageDirectory -Filter '*.nupkg' -File |
        Where-Object { -not $_.Name.EndsWith('.snupkg', [StringComparison]::OrdinalIgnoreCase) }
)
$actualSymbolPackages = @(
    Get-ChildItem -LiteralPath $resolvedPackageDirectory -Filter '*.snupkg' -File
)
if ($actualRegularPackages.Count -ne $expectedPackages.Count) {
    $failures.Add(
        "Expected $($expectedPackages.Count) .nupkg files, found $($actualRegularPackages.Count)."
    )
}
if ($actualSymbolPackages.Count -ne $expectedPackages.Count) {
    $failures.Add(
        "Expected $($expectedPackages.Count) .snupkg files, found $($actualSymbolPackages.Count)."
    )
}

foreach ($expectedPackage in $expectedPackages) {
    [xml]$packageProject = Get-Content -Raw -LiteralPath $expectedPackage.ProjectPath
    $expectedDependencies = [System.Collections.Generic.List[object]]::new()
    foreach ($packageReference in @($packageProject.SelectNodes('/Project/ItemGroup/PackageReference'))) {
        $dependencyId = [string]$packageReference.Include
        $dependencyVersion = [string]$packageReference.Version
        if ([string]::IsNullOrWhiteSpace($dependencyId) -or
            [string]::IsNullOrWhiteSpace($dependencyVersion)) {
            $failures.Add(
                "$($expectedPackage.Id) has a PackageReference without a literal Include and Version."
            )
            continue
        }

        $expectedDependencies.Add([pscustomobject]@{
            Id = $dependencyId
            Version = $dependencyVersion
        })
    }
    if ($null -ne $expectedPackage.ExpectedDependencyId) {
        $expectedDependencies.Add([pscustomobject]@{
            Id = $expectedPackage.ExpectedDependencyId
            Version = $ExpectedVersion
        })
    }

    $packagePath = Join-Path $resolvedPackageDirectory (
        "$($expectedPackage.Id).$ExpectedVersion.nupkg"
    )
    $symbolPackagePath = Join-Path $resolvedPackageDirectory (
        "$($expectedPackage.Id).$ExpectedVersion.snupkg"
    )

    if (-not (Test-Path -LiteralPath $packagePath -PathType Leaf)) {
        $failures.Add("Missing package '$packagePath'.")
        continue
    }
    $packagePaths.Add($packagePath)

    if (-not (Test-Path -LiteralPath $symbolPackagePath -PathType Leaf)) {
        $failures.Add("Missing symbol package '$symbolPackagePath'.")
    }
    else {
        $symbolPackagePaths.Add($symbolPackagePath)
        $symbolArchive = [System.IO.Compression.ZipFile]::OpenRead($symbolPackagePath)
        try {
            $pdbEntry = $symbolArchive.Entries |
                Where-Object {
                    $_.FullName.EndsWith(
                        "/$($expectedPackage.Id).pdb",
                        [StringComparison]::OrdinalIgnoreCase
                    )
                } |
                Select-Object -First 1
            if ($null -eq $pdbEntry) {
                $failures.Add("$($expectedPackage.Id) symbol package does not contain its PDB.")
            }
            else {
                $pdbBytes = Get-EntryBytes -Archive $symbolArchive -EntryName $pdbEntry.FullName
                if ($pdbBytes.Length -lt 4 -or
                    [System.Text.Encoding]::ASCII.GetString($pdbBytes, 0, 4) -cne 'BSJB') {
                    $failures.Add("$($expectedPackage.Id) symbol package does not contain a portable PDB.")
                }
            }
        }
        finally {
            $symbolArchive.Dispose()
        }
    }

    $archive = [System.IO.Compression.ZipFile]::OpenRead($packagePath)
    try {
        $licenseBytes = Get-EntryBytes -Archive $archive -EntryName 'LICENSE'
        if ($null -eq $licenseBytes) {
            $failures.Add("$($expectedPackage.Id) does not contain LICENSE.")
        }
        elseif ((Get-BytesHash -Bytes $licenseBytes) -cne $expectedLicenseHash) {
            $failures.Add("$($expectedPackage.Id) contains a LICENSE that differs from the repository file.")
        }

        $iconBytes = Get-EntryBytes -Archive $archive -EntryName 'package-icon.png'
        if ($null -eq $iconBytes) {
            $failures.Add("$($expectedPackage.Id) does not contain package-icon.png.")
        }
        elseif ((Get-BytesHash -Bytes $iconBytes) -cne $expectedIconHash) {
            $failures.Add("$($expectedPackage.Id) contains an unapproved package icon.")
        }

        $readmeBytes = Get-EntryBytes -Archive $archive -EntryName 'README.md'
        if ($null -eq $readmeBytes) {
            $failures.Add("$($expectedPackage.Id) does not contain README.md.")
        }
        else {
            $expectedReadmeHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $expectedPackage.ReadmePath).Hash
            if ((Get-BytesHash -Bytes $readmeBytes) -cne $expectedReadmeHash) {
                $failures.Add("$($expectedPackage.Id) contains a README that differs from its project README.")
            }
        }

        $xmlDocumentationName = "lib/net10.0/$($expectedPackage.Id).xml"
        if ($null -eq (Get-EntryBytes -Archive $archive -EntryName $xmlDocumentationName)) {
            $failures.Add("$($expectedPackage.Id) does not contain XML documentation.")
        }
        $assemblyName = "lib/net10.0/$($expectedPackage.Id).dll"
        if ($null -eq (Get-EntryBytes -Archive $archive -EntryName $assemblyName)) {
            $failures.Add("$($expectedPackage.Id) does not contain its net10.0 assembly.")
        }

        $nuspecEntry = $archive.Entries |
            Where-Object { $_.FullName.EndsWith('.nuspec', [StringComparison]::OrdinalIgnoreCase) } |
            Select-Object -First 1
        if ($null -eq $nuspecEntry) {
            $failures.Add("$($expectedPackage.Id) does not contain a nuspec.")
            continue
        }

        $nuspecStream = $nuspecEntry.Open()
        $reader = [System.IO.StreamReader]::new($nuspecStream)
        try {
            [xml]$nuspec = $reader.ReadToEnd()
        }
        finally {
            $reader.Dispose()
            $nuspecStream.Dispose()
        }

        $namespaceManager = [System.Xml.XmlNamespaceManager]::new($nuspec.NameTable)
        $namespaceManager.AddNamespace('n', $nuspec.DocumentElement.NamespaceURI)
        $metadataPath = '/n:package/n:metadata'

        $id = Get-NodeInnerText (
            $nuspec.SelectSingleNode("$metadataPath/n:id", $namespaceManager)
        )
        $version = Get-NodeInnerText (
            $nuspec.SelectSingleNode("$metadataPath/n:version", $namespaceManager)
        )
        $projectUrl = Get-NodeInnerText (
            $nuspec.SelectSingleNode("$metadataPath/n:projectUrl", $namespaceManager)
        )
        $authors = Get-NodeInnerText (
            $nuspec.SelectSingleNode("$metadataPath/n:authors", $namespaceManager)
        )
        $description = Get-NodeInnerText (
            $nuspec.SelectSingleNode("$metadataPath/n:description", $namespaceManager)
        )
        $tags = Get-NodeInnerText (
            $nuspec.SelectSingleNode("$metadataPath/n:tags", $namespaceManager)
        )
        $licenseNode = $nuspec.SelectSingleNode("$metadataPath/n:license", $namespaceManager)
        $icon = Get-NodeInnerText (
            $nuspec.SelectSingleNode("$metadataPath/n:icon", $namespaceManager)
        )
        $readme = Get-NodeInnerText (
            $nuspec.SelectSingleNode("$metadataPath/n:readme", $namespaceManager)
        )
        $repositoryNode = $nuspec.SelectSingleNode("$metadataPath/n:repository", $namespaceManager)

        if ($id -cne $expectedPackage.Id) {
            $failures.Add("$($expectedPackage.Id) nuspec id is '$id'.")
        }
        if ($version -cne $ExpectedVersion) {
            $failures.Add("$($expectedPackage.Id) nuspec version is '$version', expected '$ExpectedVersion'.")
        }
        if ($projectUrl -cne 'https://aicandoitall.com/') {
            $failures.Add("$($expectedPackage.Id) has incorrect projectUrl '$projectUrl'.")
        }
        if ($authors -cne 'fyziktom') {
            $failures.Add("$($expectedPackage.Id) has incorrect authors '$authors'.")
        }
        if ([string]::IsNullOrWhiteSpace($description)) {
            $failures.Add("$($expectedPackage.Id) has no package description.")
        }
        foreach ($requiredTag in $expectedPackage.RequiredTags) {
            if ($tags -notmatch "(?i)(?:^|[\s;])$([regex]::Escape($requiredTag))(?:[\s;]|$)") {
                $failures.Add("$($expectedPackage.Id) tags do not contain '$requiredTag'.")
            }
        }
        if ($null -eq $licenseNode -or
            $licenseNode.GetAttribute('type') -cne 'file' -or
            $licenseNode.InnerText -cne 'LICENSE') {
            $failures.Add("$($expectedPackage.Id) must use <license type=`"file`">LICENSE</license>.")
        }
        if ($icon -cne 'package-icon.png') {
            $failures.Add("$($expectedPackage.Id) nuspec icon is '$icon'.")
        }
        if ($readme -cne 'README.md') {
            $failures.Add("$($expectedPackage.Id) nuspec readme is '$readme'.")
        }
        if ($null -eq $repositoryNode -or
            $repositoryNode.GetAttribute('type') -cne 'git' -or
            $repositoryNode.GetAttribute('url') -cne 'https://github.com/fyziktom/CanDoItAll.AgentFramework.Rag.git') {
            $failures.Add("$($expectedPackage.Id) has incorrect repository metadata.")
        }
        elseif ([string]::IsNullOrWhiteSpace($repositoryNode.GetAttribute('commit'))) {
            $failures.Add("$($expectedPackage.Id) repository metadata has no source commit.")
        }

        $dependencyNodes = @(
            $nuspec.SelectNodes("$metadataPath/n:dependencies//n:dependency", $namespaceManager)
        )
        $duplicateDependencies = @(
            $dependencyNodes |
                Group-Object { $_.GetAttribute('id').ToUpperInvariant() } |
                Where-Object { $_.Count -gt 1 }
        )
        foreach ($duplicateDependency in $duplicateDependencies) {
            $failures.Add(
                "$($expectedPackage.Id) contains duplicate dependency '$($duplicateDependency.Name)'."
            )
        }

        foreach ($expectedDependency in $expectedDependencies) {
            $matchingDependencies = @(
                $dependencyNodes |
                    Where-Object {
                        [string]::Equals(
                            $_.GetAttribute('id'),
                            $expectedDependency.Id,
                            [StringComparison]::OrdinalIgnoreCase
                        )
                    }
            )
            if ($matchingDependencies.Count -eq 0) {
                $failures.Add(
                    "$($expectedPackage.Id) does not depend on $($expectedDependency.Id)."
                )
            }
            elseif (-not (Test-ExpectedDependencyVersion `
                    -ActualVersion $matchingDependencies[0].GetAttribute('version') `
                    -ExpectedMinimumVersion $expectedDependency.Version)) {
                $failures.Add(
                    "$($expectedPackage.Id) dependency '$($expectedDependency.Id)' has version " +
                    "'$($matchingDependencies[0].GetAttribute('version'))', expected " +
                    "'$($expectedDependency.Version)' as its exact minimum."
                )
            }
        }

        $expectedDependencyIds = @(
            $expectedDependencies | ForEach-Object { $_.Id }
        )
        foreach ($dependencyNode in $dependencyNodes) {
            $actualDependencyId = $dependencyNode.GetAttribute('id')
            if (-not ($expectedDependencyIds -icontains $actualDependencyId)) {
                $failures.Add(
                    "$($expectedPackage.Id) contains unexpected dependency '$actualDependencyId'."
                )
            }
        }
    }
    finally {
        $archive.Dispose()
    }
}

if ($failures.Count -gt 0) {
    throw "NuGet package validation failed:`n - $($failures -join "`n - ")"
}

[pscustomobject]@{
    PackageDirectory = $resolvedPackageDirectory
    PackageVersion = $ExpectedVersion
    Packages = $packagePaths.ToArray()
    SymbolPackages = $symbolPackagePaths.ToArray()
    Status = 'Passed'
}
