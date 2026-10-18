# Package handoff only; no build, installation or process control.
function Get-TimberbornPackageFiles {
    @('Timberborn.AgentBridge.dll', 'Timberborn.Bridge.Core.dll', 'manifest.json', 'LICENSE', 'INSTALL.md')
}

function New-TimberbornPackageResult {
    param([Parameter(Mandatory)][string]$PackageRoot, [Parameter(Mandatory)][guid]$BuildId)
    if ($BuildId -eq [guid]::Empty) { throw 'Leere Build-ID.' }
    $resolvedRoot = (Resolve-Path -LiteralPath $PackageRoot -ErrorAction Stop).Path
    $sourceDir = Join-Path $resolvedRoot 'TimberbornAgentBridge'
    $manifest = Get-Content -LiteralPath (Join-Path $sourceDir 'manifest.json') -Raw -ErrorAction Stop | ConvertFrom-Json -ErrorAction Stop
    if ($manifest.Version -notmatch '^\d+\.\d+\.\d+$' -or [string]::IsNullOrWhiteSpace($manifest.Id)) {
        throw 'Ungültiges Paketmanifest.'
    }
    $hashes = @{}
    foreach ($file in Get-TimberbornPackageFiles) {
        $hashes[$file] = (Get-FileHash -LiteralPath (Join-Path $sourceDir $file) -Algorithm SHA256 -ErrorAction Stop).Hash
    }
    [pscustomobject]@{
        SchemaVersion = 1
        BuildId = $BuildId.ToString('D')
        PackageRoot = $resolvedRoot
        Version = [string]$manifest.Version
        ModId = [string]$manifest.Id
        Hashes = $hashes
    }
}

function Assert-TimberbornPackageFiles {
    param([Parameter(Mandatory)][string]$SourceDir, [Parameter(Mandatory)]$Hashes)
    $files = @(Get-TimberbornPackageFiles)
    if ($Hashes -isnot [Collections.IDictionary] -or $Hashes.Count -ne $files.Count) {
        throw 'Unvollständige Paketprüfsummen.'
    }
    foreach ($file in $files) {
        $path = Join-Path $SourceDir $file
        if ($Hashes[$file] -isnot [string] -or $Hashes[$file] -notmatch '^[0-9A-Fa-f]{64}$' -or
            -not (Test-Path -LiteralPath $path -PathType Leaf)) {
            throw "Paketdatei oder Prüfsumme fehlt: $file"
        }
        if ((Get-FileHash -LiteralPath $path -Algorithm SHA256 -ErrorAction Stop).Hash -ne $Hashes[$file]) {
            throw "Paketprüfung fehlgeschlagen: $file"
        }
    }
}

function Resolve-TimberbornPackageResult {
    param(
        [AllowNull()]$Result,
        [Parameter(Mandatory)][guid]$BuildId,
        [Parameter(Mandatory)][string]$PackagesDirectory,
        [Parameter(Mandatory)][string]$ExpectedManifestPath
    )
    if ($null -eq $Result -or $Result -is [array] -or $Result.SchemaVersion -ne 1 -or
        $BuildId -eq [guid]::Empty -or $Result.BuildId -cne $BuildId.ToString('D')) {
        throw 'Fehlendes oder fremdes Paketergebnis für diesen Build.'
    }
    if ($Result.PackageRoot -isnot [string] -or -not [IO.Path]::IsPathFullyQualified($Result.PackageRoot) -or
        -not (Test-Path -LiteralPath $Result.PackageRoot -PathType Container)) {
        throw 'Ungültiger Paketpfad.'
    }
    $packageRoot = (Resolve-Path -LiteralPath $Result.PackageRoot -ErrorAction Stop).Path
    $packagesRoot = (Resolve-Path -LiteralPath $PackagesDirectory -ErrorAction Stop).Path.TrimEnd([IO.Path]::DirectorySeparatorChar)
    if (-not [string]::Equals((Split-Path $packageRoot -Parent), $packagesRoot, [StringComparison]::OrdinalIgnoreCase)) {
        throw 'Paketergebnis liegt nicht direkt im Paketverzeichnis.'
    }
    $sourceDir = Join-Path $packageRoot 'TimberbornAgentBridge'
    Assert-TimberbornPackageFiles -SourceDir $sourceDir -Hashes $Result.Hashes
    $expected = Get-Content -LiteralPath $ExpectedManifestPath -Raw -ErrorAction Stop | ConvertFrom-Json -ErrorAction Stop
    $manifest = Get-Content -LiteralPath (Join-Path $sourceDir 'manifest.json') -Raw -ErrorAction Stop | ConvertFrom-Json -ErrorAction Stop
    if ($expected.Version -notmatch '^\d+\.\d+\.\d+$' -or [string]::IsNullOrWhiteSpace($expected.Id) -or
        $Result.Version -cne $expected.Version -or $Result.ModId -cne $expected.Id -or
        $manifest.Version -cne $expected.Version -or $manifest.Id -cne $expected.Id -or
        $Result.Hashes['manifest.json'] -ne (Get-FileHash -LiteralPath $ExpectedManifestPath -Algorithm SHA256 -ErrorAction Stop).Hash) {
        throw 'Paketmanifest passt nicht zum aktuellen Buildauftrag.'
    }
    # Never enumerate other packages or infer provenance from timestamps.
    return $sourceDir
}
