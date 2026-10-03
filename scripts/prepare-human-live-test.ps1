param(
    [Parameter(Mandatory)]
    [string]$TimberbornManagedDir,
    [string]$ModsRoot = (Join-Path ([Environment]::GetFolderPath('MyDocuments')) 'Timberborn\Mods'),
    [int]$Port = 8081
)

$ErrorActionPreference = 'Stop'

function Get-NormalizedPath([string]$Path, [string]$Label) {
    if (-not (Test-Path -LiteralPath $Path -PathType Container)) { throw "$Label existiert nicht: $Path" }
    (Resolve-Path -LiteralPath $Path).Path.TrimEnd([System.IO.Path]::DirectorySeparatorChar, [System.IO.Path]::AltDirectorySeparatorChar)
}

function Assert-ChildPath([string]$Child, [string]$Parent, [string]$Label) {
    if (-not $Child.StartsWith($Parent + [System.IO.Path]::DirectorySeparatorChar, [System.StringComparison]::OrdinalIgnoreCase)) {
        throw "$Label liegt nicht innerhalb von $Parent."
    }
}

function Assert-SameFile([string]$Expected, [string]$Actual) {
    if ((Get-FileHash -LiteralPath $Expected -Algorithm SHA256).Hash -ne (Get-FileHash -LiteralPath $Actual -Algorithm SHA256).Hash) {
        throw "Paketprüfung fehlgeschlagen: $(Split-Path -Leaf $Expected)"
    }
}

function Stop-LocalMcpServers([string]$WorkspaceRoot) {
    $serverDll = [System.IO.Path]::GetFullPath((Join-Path $WorkspaceRoot 'src\Timberborn.McpServer\bin\Release\net10.0\Timberborn.McpServer.dll'))
    try {
        $processes = @(Get-CimInstance Win32_Process -Filter "Name = 'dotnet.exe'" -ErrorAction Stop)
    } catch {
        throw "Laufende MCP-Prozesse konnten nicht sicher ermittelt werden: $($_.Exception.Message)"
    }
    $matches = @($processes | Where-Object {
        $commandLine = [string]$_.CommandLine
        $commandLine.IndexOf($serverDll, [System.StringComparison]::OrdinalIgnoreCase) -ge 0 -or
        $commandLine -match '(?i)(^|[\\/\s])Timberborn\.McpServer\.dll(?:\s|$)'
    })
    if ($matches.Count -eq 0) { return }
    $ids = @($matches | ForEach-Object ProcessId)
    Write-Output "Vorbereitung: $($ids.Count) lokaler MCP-Prozess(e) beenden ..."
    foreach ($id in $ids) { Stop-Process -Id $id -ErrorAction Stop }
    foreach ($id in $ids) {
        try { Wait-Process -Id $id -Timeout 5 -ErrorAction Stop }
        catch { if ($_.Exception.Message -notmatch 'Cannot find a process') { throw } }
    }
    $remaining = @(Get-Process -Id $ids -ErrorAction SilentlyContinue)
    if ($remaining.Count -gt 0) { throw 'Lokaler MCP-Prozess konnte nicht vollständig beendet werden.' }
}

. "$PSScriptRoot/environment.ps1"
Push-Location $taskRoot
try {
    if (@(Get-Process -Name 'Timberborn*' -ErrorAction SilentlyContinue).Count -gt 0) {
        throw 'Timberborn läuft noch. Spiel vollständig beenden und dieses Skript erneut starten.'
    }
    Stop-LocalMcpServers $taskRoot
    $managedDir = Get-NormalizedPath $TimberbornManagedDir 'Timberborn Managed-Verzeichnis'
    $modsDir = Get-NormalizedPath $ModsRoot 'Mods-Verzeichnis'
    $targetDir = Join-Path $modsDir 'TimberbornAgentBridge'
    Assert-ChildPath $targetDir $modsDir 'Zielordner'

    Write-Output '1/4: Automatische Tests und Build ausführen ...'
    & "$PSScriptRoot/verify.ps1"
    if ($LASTEXITCODE -ne 0) { throw 'Automatische Tests fehlgeschlagen.' }
    Write-Output '2/4: Native Bridge paketieren ...'
    & "$PSScriptRoot/build-native-bridge.ps1" -TimberbornManagedDir $managedDir -Port $Port
    if ($LASTEXITCODE -ne 0) { throw 'Bridge-Paketierung fehlgeschlagen.' }

    $packageRoot = Get-ChildItem -LiteralPath (Join-Path $taskRoot '.local/packages') -Directory -Filter 'agent-bridge-*' | Sort-Object LastWriteTimeUtc -Descending | Select-Object -First 1
    if ($null -eq $packageRoot) { throw 'Kein gerade erzeugtes Bridge-Paket gefunden.' }
    $sourceDir = Join-Path $packageRoot.FullName 'TimberbornAgentBridge'
    $packageFiles = @('Timberborn.AgentBridge.dll', 'Timberborn.Bridge.Core.dll', 'manifest.json', 'LICENSE', 'INSTALL.md')
    foreach ($file in $packageFiles) {
        if (-not (Test-Path -LiteralPath (Join-Path $sourceDir $file) -PathType Leaf)) { throw "Paketdatei fehlt: $file" }
    }

    $previousConfig = $null
    if (Test-Path -LiteralPath $targetDir -PathType Container) {
        $configPath = Join-Path $targetDir 'bridge.local.json'
        if (Test-Path -LiteralPath $configPath -PathType Leaf) {
            $previousConfig = Get-Content -LiteralPath $configPath -Raw
            if ([string]::IsNullOrWhiteSpace([string](($previousConfig | ConvertFrom-Json).token))) {
                throw 'Die vorhandene private Bridge-Konfiguration enthält keinen Token und wird nicht überschrieben.'
            }
        }
    }

    $stagingDir = Join-Path $modsDir ('TimberbornAgentBridge.staging-' + [guid]::NewGuid().ToString('N'))
    $backupRoot = Join-Path $taskRoot ('.local/backups/human-live-test-' + (Get-Date -Format 'yyyyMMdd-HHmmss'))
    $previousInstall = Join-Path $backupRoot 'TimberbornAgentBridge'
    $installationStarted = $false
    try {
        Write-Output '3/4: Mod sicher installieren ...'
        New-Item -ItemType Directory -Path $stagingDir | Out-Null
        Copy-Item -LiteralPath $sourceDir -Destination $stagingDir -Recurse
        $stagedModDir = Join-Path $stagingDir 'TimberbornAgentBridge'
        if ($null -ne $previousConfig) { Set-Content -LiteralPath (Join-Path $stagedModDir 'bridge.local.json') -Value $previousConfig -Encoding utf8NoBOM }
        New-Item -ItemType Directory -Path $backupRoot | Out-Null
        if (Test-Path -LiteralPath $targetDir -PathType Container) { Move-Item -LiteralPath $targetDir -Destination $previousInstall }
        $installationStarted = $true
        Move-Item -LiteralPath $stagedModDir -Destination $targetDir
        Write-Output '4/4: Installierte Paketdateien prüfen ...'
        foreach ($file in $packageFiles) { Assert-SameFile (Join-Path $sourceDir $file) (Join-Path $targetDir $file) }
    } catch {
        if ($installationStarted -and (Test-Path -LiteralPath $previousInstall -PathType Container)) {
            if (Test-Path -LiteralPath $targetDir -PathType Container) { Remove-Item -LiteralPath $targetDir -Recurse -Force }
            Move-Item -LiteralPath $previousInstall -Destination $targetDir
        }
        throw
    } finally {
        if (Test-Path -LiteralPath $stagingDir -PathType Container) { Remove-Item -LiteralPath $stagingDir -Recurse -Force }
    }
    Write-Output ''
    Write-Output 'Bereit für den menschlichen Test: Tests bestanden, Mod installiert und Paketdateien verifiziert.'
    Write-Output 'Nächste Schritte: Timberborn starten, einen Spielstand laden und danach dem Agenten "live bereit" schreiben.'
    Write-Output 'Eine vorherige Mod-Installation wurde lokal gesichert; private Bridge-Konfiguration wurde beibehalten.'
} finally { Pop-Location }
