param(
    [Parameter(Mandatory)]
    [string]$TimberbornManagedDir,
    [string]$ModsRoot = (Join-Path ([Environment]::GetFolderPath('MyDocuments')) 'Timberborn\Mods'),
    [int]$Port = 8081
)

$ErrorActionPreference = 'Stop'
. "$PSScriptRoot/cli-output.ps1"
$gateDisplay = New-TimberbornGateDisplay @(
    'Spiel und MCP stoppen / Umgebung prüfen',
    'Tests und Release-Build',
    'Native Bridge und Paket',
    'Mod sichern und installieren',
    'Installierte Dateien prüfen',
    'Timberborn starten'
) -TimingPath (Join-Path (Split-Path $PSScriptRoot -Parent) '.local/gate-timings.json')

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

function Stop-TimberbornGame {
    $processes = @(Get-Process -ErrorAction Stop | Where-Object ProcessName -EQ 'Timberborn')
    if ($processes.Count -eq 0) {
        Write-TimberbornStatus 'OK' 'Timberborn ist bereits beendet.'
        return
    }
    Write-TimberbornStatus 'INFO' "Timberborn.exe hart beenden, ohne Speichern ($($processes.Count) Prozess(e))."
    foreach ($process in $processes) {
        try {
            if (-not $process.HasExited) { Stop-Process -InputObject $process -Force -ErrorAction Stop }
        } catch {
            if (-not $process.HasExited) { throw }
        }
        if (-not $process.WaitForExit(5000)) { throw 'Timberborn wurde nicht innerhalb von 5 Sekunden beendet. Kein Build gestartet.' }
    }
    if (@(Get-Process -ErrorAction Stop | Where-Object ProcessName -EQ 'Timberborn').Count -gt 0) {
        throw 'Timberborn läuft weiterhin oder wurde erneut gestartet. Kein Build gestartet.'
    }
    Write-TimberbornStatus 'OK' 'Timberborn ohne Speichern beendet.'
}

function Stop-LocalMcpServers([string]$WorkspaceRoot) {
    & python -B "$PSScriptRoot/codex-mcp-supervisor.py" --control stop
    if ($LASTEXITCODE -ne 0) { throw 'MCP-Stopp nicht bestätigt. Kein Build gestartet.' }
    $serverDll = [System.IO.Path]::GetFullPath((Join-Path $WorkspaceRoot 'src\Timberborn.McpServer\bin\Release\net10.0\Timberborn.McpServer.dll'))
    try {
        $processes = @(Get-CimInstance Win32_Process -Filter "Name = 'dotnet.exe'" -ErrorAction Stop)
    } catch {
        throw "Laufende MCP-Prozesse konnten nicht sicher ermittelt werden: $($_.Exception.Message)"
    }
    $matches = @($processes | Where-Object {
        $commandLine = ([string]$_.CommandLine).Replace('/', '\')
        $commandLine -match ('(?i)(?:^|[\s"])' + [regex]::Escape($serverDll) + '(?:[\s"]|$)')
    })
    if ($matches.Count -eq 0) { return }
    $ids = @($matches | ForEach-Object ProcessId)
    Write-TimberbornStatus 'INFO' "$($ids.Count) lokalen MCP-Prozess(e) beenden"
    foreach ($id in $ids) { Stop-Process -Id $id -ErrorAction Stop }
    foreach ($id in $ids) {
        try { Wait-Process -Id $id -Timeout 5 -ErrorAction Stop }
        catch { if ($_.Exception.Message -notmatch 'Cannot find a process') { throw } }
    }
    $remaining = @(Get-Process -Id $ids -ErrorAction SilentlyContinue)
    if ($remaining.Count -gt 0) { throw 'Lokaler MCP-Prozess konnte nicht vollständig beendet werden.' }
}

$gateLogDirectory = Join-Path (Split-Path $PSScriptRoot -Parent) '.local/gate-logs'
[void][IO.Directory]::CreateDirectory($gateLogDirectory)
$gateDisplay.LogPath = Join-Path $gateLogDirectory ('gate-' + (Get-Date -Format 'yyyyMMdd-HHmmss') + '-' + [guid]::NewGuid().ToString('N') + '.log')
$gateFailed = $false
$gateLocationPushed = $false
try {
    # Dot-source preserves phase variables. Every command stream goes to the local
    # log; only the display renderer writes directly to the console.
    . {
    $ProgressPreference = 'SilentlyContinue'
    . "$PSScriptRoot/environment.ps1"
    Push-Location $taskRoot
    $gateLocationPushed = $true
    Start-TimberbornGatePhase $gateDisplay 0
    Stop-TimberbornGame
    Stop-LocalMcpServers $taskRoot
    $managedDir = Get-NormalizedPath $TimberbornManagedDir 'Timberborn Managed-Verzeichnis'
    $gameDir = Split-Path (Split-Path $managedDir -Parent) -Parent
    $gameExe = Join-Path $gameDir 'Timberborn.exe'
    if (-not (Test-Path -LiteralPath $gameExe -PathType Leaf)) { throw "Timberborn.exe fehlt: $gameExe" }
    $modsDir = Get-NormalizedPath $ModsRoot 'Mods-Verzeichnis'
    $targetDir = Join-Path $modsDir 'TimberbornAgentBridge'
    Assert-ChildPath $targetDir $modsDir 'Zielordner'
    Complete-TimberbornGatePhase $gateDisplay

    Start-TimberbornGatePhase $gateDisplay 1
    & "$PSScriptRoot/verify.ps1"
    Complete-TimberbornGatePhase $gateDisplay
    Start-TimberbornGatePhase $gateDisplay 2
    & "$PSScriptRoot/build-native-bridge.ps1" -TimberbornManagedDir $managedDir -Port $Port

    $packageRoot = Get-ChildItem -LiteralPath (Join-Path $taskRoot '.local/packages') -Directory -Filter 'agent-bridge-*' | Sort-Object LastWriteTimeUtc -Descending | Select-Object -First 1
    if ($null -eq $packageRoot) { throw 'Kein gerade erzeugtes Bridge-Paket gefunden.' }
    $sourceDir = Join-Path $packageRoot.FullName 'TimberbornAgentBridge'
    $packageFiles = @('Timberborn.AgentBridge.dll', 'Timberborn.Bridge.Core.dll', 'manifest.json', 'LICENSE', 'INSTALL.md')
    foreach ($file in $packageFiles) {
        if (-not (Test-Path -LiteralPath (Join-Path $sourceDir $file) -PathType Leaf)) { throw "Paketdatei fehlt: $file" }
    }
    Complete-TimberbornGatePhase $gateDisplay
    Start-TimberbornGatePhase $gateDisplay 3

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
        New-Item -ItemType Directory -Path $stagingDir | Out-Null
        Copy-Item -LiteralPath $sourceDir -Destination $stagingDir -Recurse
        $stagedModDir = Join-Path $stagingDir 'TimberbornAgentBridge'
        if ($null -ne $previousConfig) { Set-Content -LiteralPath (Join-Path $stagedModDir 'bridge.local.json') -Value $previousConfig -Encoding utf8NoBOM }
        New-Item -ItemType Directory -Path $backupRoot | Out-Null
        if (Test-Path -LiteralPath $targetDir -PathType Container) { Move-Item -LiteralPath $targetDir -Destination $previousInstall }
        $installationStarted = $true
        Move-Item -LiteralPath $stagedModDir -Destination $targetDir
        Complete-TimberbornGatePhase $gateDisplay
        Start-TimberbornGatePhase $gateDisplay 4
        $verifiedFiles = 0
        foreach ($file in $packageFiles) {
            Assert-SameFile (Join-Path $sourceDir $file) (Join-Path $targetDir $file)
            $verifiedFiles++
            Write-TimberbornStatus 'SHA256' ("{0}/{1}  {2}" -f $verifiedFiles, $packageFiles.Count, $file) Green
        }
    } catch {
        if ($installationStarted -and (Test-Path -LiteralPath $previousInstall -PathType Container)) {
            if (Test-Path -LiteralPath $targetDir -PathType Container) { Remove-Item -LiteralPath $targetDir -Recurse -Force }
            Move-Item -LiteralPath $previousInstall -Destination $targetDir
            Write-TimberbornStatus 'ROLLBACK' 'Vorherige Mod-Installation wiederhergestellt.' Yellow
        }
        throw
    } finally {
        if (Test-Path -LiteralPath $stagingDir -PathType Container) { Remove-Item -LiteralPath $stagingDir -Recurse -Force }
    }
    Complete-TimberbornGatePhase $gateDisplay
    if (Test-Path -LiteralPath $previousInstall) { Write-Host "  Sicherung: $previousInstall" -ForegroundColor DarkGray }
    Start-TimberbornGatePhase $gateDisplay 5
    Start-Process -FilePath $gameExe -WorkingDirectory $gameDir -ErrorAction Stop
    Write-TimberbornStatus 'OK' 'Timberborn-Start angefordert.'
    Complete-TimberbornGatePhase $gateDisplay
    } *> ($gateDisplay.LogPath)
    Write-TimberbornGateSummary $gateDisplay
} catch {
    $gateFailed = $true
    $gateDisplay.Failure = $_.Exception.Message
    try { ($_ | Out-String) | Add-Content -LiteralPath $gateDisplay.LogPath -Encoding utf8NoBOM } catch {}
    Write-TimberbornGateSummary $gateDisplay -Failed
} finally {
    Stop-TimberbornGateProgress $gateDisplay
    if ($gateLocationPushed) { Pop-Location }
}
if ($gateFailed) { exit 1 }
