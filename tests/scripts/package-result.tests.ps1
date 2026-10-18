# Synthetic filesystem tests, invoked by the human gate. No game/build/install calls.
$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot '../../scripts/package-result.ps1')
$testParent = [IO.Path]::GetFullPath([IO.Path]::GetTempPath()).TrimEnd([IO.Path]::DirectorySeparatorChar)
$testRoot = Join-Path $testParent ('timberborn-package-test-' + [guid]::NewGuid().ToString('N'))
$script:packageChecks = 0

function Assert-Equal($Expected, $Actual) {
    if ($Expected -cne $Actual) { throw "Erwartet: $Expected; erhalten: $Actual" }
}
function Assert-Rejected([scriptblock]$Action, [string]$Message) {
    $rejected = $false
    try { & $Action | Out-Null }
    catch {
        $rejected = $true
        if ($_.Exception.Message -notlike $Message) { throw }
    }
    if (-not $rejected) { throw "Erwartete Ablehnung fehlt: $Message" }
}
function Invoke-Case([string]$Name, [scriptblock]$Action) {
    try { & $Action; $script:packageChecks++ }
    catch { throw "Pakettest '$Name' fehlgeschlagen: $($_.Exception.Message)" }
}
function New-Fixture([string]$Name, [string]$Version = '0.74.0', [string]$ModId = 'synthetic.agent.bridge') {
    $package = Join-Path $packages $Name
    $source = Join-Path $package 'TimberbornAgentBridge'
    [void][IO.Directory]::CreateDirectory($source)
    foreach ($file in Get-TimberbornPackageFiles) {
        Set-Content -LiteralPath (Join-Path $source $file) -Value "synthetic fixture $file" -Encoding utf8NoBOM
    }
    @{ Version = $Version; Id = $ModId } | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $source 'manifest.json') -Encoding utf8NoBOM
    return $package
}

try {
    $packages = Join-Path $testRoot 'packages'
    [void][IO.Directory]::CreateDirectory($packages)
    $current = New-Fixture 'agent-bridge-current'
    $sourceDir = Join-Path $current 'TimberbornAgentBridge'
    $expectedManifest = Join-Path $testRoot 'expected-manifest.json'
    Copy-Item -LiteralPath (Join-Path $sourceDir 'manifest.json') -Destination $expectedManifest
    $buildId = [guid]::NewGuid()
    $receipt = New-TimberbornPackageResult -PackageRoot $current -BuildId $buildId
    $resolve = @{ BuildId = $buildId; PackagesDirectory = $packages; ExpectedManifestPath = $expectedManifest }

    Invoke-Case 'Aktueller Pfad trotz neuerem fremdem Paket' {
        $foreign = New-Fixture 'agent-bridge-newer'
        (Get-Item -LiteralPath $foreign).LastWriteTimeUtc = [datetime]::UtcNow.AddDays(1)
        (Get-Item -LiteralPath $current).LastWriteTimeUtc = [datetime]::UtcNow.AddDays(-1)
        Assert-Equal $sourceDir (Resolve-TimberbornPackageResult -Result $receipt @resolve)
    }
    Invoke-Case 'Fehlendes Ergebnis hat keinen Verzeichnisfallback' {
        Assert-Rejected { Resolve-TimberbornPackageResult -Result $null @resolve } 'Fehlendes oder fremdes Paketergebnis*'
    }
    Invoke-Case 'Ergebnis bleibt trotz umgeleiteter Diagnose separat' {
        $handedOff = $null
        $producer = { param([ref]$PackageResult)
            $PackageResult.Value = $null
            Write-Output 'synthetic compiler output'
            Write-Host 'synthetic package status'
            $PackageResult.Value = New-TimberbornPackageResult -PackageRoot $current -BuildId $buildId
        }
        & $producer -PackageResult ([ref]$handedOff) *> (Join-Path $testRoot 'synthetic-output.log')
        Assert-Equal $sourceDir (Resolve-TimberbornPackageResult -Result $handedOff @resolve)
    }
    Invoke-Case 'Mehrdeutiges Ergebnis abgewiesen' {
        Assert-Rejected { Resolve-TimberbornPackageResult -Result @($receipt, $receipt) @resolve } 'Fehlendes oder fremdes Paketergebnis*'
    }
    Invoke-Case 'Fremde Build-ID trotz gleicher Version abgewiesen' {
        $other = New-TimberbornPackageResult -PackageRoot $current -BuildId ([guid]::NewGuid())
        Assert-Rejected { Resolve-TimberbornPackageResult -Result $other @resolve } 'Fehlendes oder fremdes Paketergebnis*'
    }
    Invoke-Case 'Falsche Manifestversion abgewiesen' {
        $other = New-TimberbornPackageResult -PackageRoot (New-Fixture 'agent-bridge-wrong-version' '99.0.0') -BuildId $buildId
        Assert-Rejected { Resolve-TimberbornPackageResult -Result $other @resolve } 'Paketmanifest passt nicht*'
    }
    Invoke-Case 'Falsche Mod-ID abgewiesen' {
        $other = New-TimberbornPackageResult -PackageRoot (New-Fixture 'agent-bridge-wrong-id' '0.74.0' 'synthetic.other') -BuildId $buildId
        Assert-Rejected { Resolve-TimberbornPackageResult -Result $other @resolve } 'Paketmanifest passt nicht*'
    }
    Invoke-Case 'Manifeständerung bei gleicher Version abgewiesen' {
        $otherPath = New-Fixture 'agent-bridge-changed-manifest'
        Add-Content -LiteralPath (Join-Path $otherPath 'TimberbornAgentBridge/manifest.json') -Value ' '
        $other = New-TimberbornPackageResult -PackageRoot $otherPath -BuildId $buildId
        Assert-Rejected { Resolve-TimberbornPackageResult -Result $other @resolve } 'Paketmanifest passt nicht*'
    }
    Invoke-Case 'Fehlende Pflichtdatei abgewiesen' {
        $path = Join-Path $sourceDir 'INSTALL.md'
        $contents = [IO.File]::ReadAllBytes($path)
        Remove-Item -LiteralPath $path
        try { Assert-Rejected { Resolve-TimberbornPackageResult -Result $receipt @resolve } 'Paketdatei oder Prüfsumme fehlt*' }
        finally { [IO.File]::WriteAllBytes($path, $contents) }
    }
    Invoke-Case 'Nach Ergebnisübergabe veränderte Datei abgewiesen' {
        $path = Join-Path $sourceDir 'Timberborn.AgentBridge.dll'
        $contents = [IO.File]::ReadAllBytes($path)
        Add-Content -LiteralPath $path -Value 'changed synthetic fixture'
        try { Assert-Rejected { Resolve-TimberbornPackageResult -Result $receipt @resolve } 'Paketprüfung fehlgeschlagen*' }
        finally { [IO.File]::WriteAllBytes($path, $contents) }
    }
    Invoke-Case 'Paket außerhalb des erwarteten Verzeichnisses abgewiesen' {
        $outside = Join-Path $testRoot 'outside'
        Copy-Item -LiteralPath $current -Destination $outside -Recurse
        $other = New-TimberbornPackageResult -PackageRoot $outside -BuildId $buildId
        Assert-Rejected { Resolve-TimberbornPackageResult -Result $other @resolve } 'Paketergebnis liegt nicht*'
    }
    Invoke-Case 'Unvollständige Prüfsummen abgewiesen' {
        $other = New-TimberbornPackageResult -PackageRoot $current -BuildId $buildId
        $other.Hashes.Remove('LICENSE')
        Assert-Rejected { Resolve-TimberbornPackageResult -Result $other @resolve } 'Unvollständige Paketprüfsummen*'
    }
    Invoke-Case 'Staging mit privater Konfiguration, dann beschädigte Kopie' {
        $stage = Join-Path $testRoot 'stage'
        Copy-Item -LiteralPath $sourceDir -Destination $stage -Recurse
        $private = Join-Path $stage 'bridge.local.json'
        Set-Content -LiteralPath $private -Value '{"synthetic":true}' -Encoding utf8NoBOM
        $before = [IO.File]::ReadAllText($private)
        Assert-TimberbornPackageFiles -SourceDir $stage -Hashes $receipt.Hashes
        Assert-Equal $before ([IO.File]::ReadAllText($private))
        Add-Content -LiteralPath (Join-Path $stage 'LICENSE') -Value 'changed synthetic fixture'
        Assert-Rejected { Assert-TimberbornPackageFiles -SourceDir $stage -Hashes $receipt.Hashes } 'Paketprüfung fehlgeschlagen*'
    }
    Write-Host ("[TESTS] {0} Paketergebnis-Prüfungen bestanden" -f $script:packageChecks)
} finally {
    if (Test-Path -LiteralPath $testRoot) {
        $resolved = (Resolve-Path -LiteralPath $testRoot).Path
        if ((Split-Path $resolved -Parent) -ne $testParent -or (Split-Path $resolved -Leaf) -notmatch '^timberborn-package-test-[0-9a-f]{32}$') {
            throw 'Unerwarteter Testpfad; keine Bereinigung ausgeführt.'
        }
        Remove-Item -LiteralPath $resolved -Recurse -Force
    }
}
