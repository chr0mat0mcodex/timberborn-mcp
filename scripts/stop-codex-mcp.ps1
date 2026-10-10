# Lifecycle helper only: no build, install or detached restart.
$ErrorActionPreference = 'Stop'
# Stop through the persistent frontend first. This also inhibits backend startup
# when Codex is opened while the human gate is running.
& python -B "$PSScriptRoot/codex-mcp-supervisor.py" --control stop
if ($LASTEXITCODE -ne 0) { throw 'MCP-Vorschaltprozess hat den Stopp nicht bestätigt.' }
$workspaceRoot = [IO.Path]::GetFullPath((Split-Path $PSScriptRoot -Parent))
$releaseDir = Join-Path $workspaceRoot 'src/Timberborn.McpServer/bin/Release/net10.0'
$serverDll = [IO.Path]::GetFullPath((Join-Path $releaseDir 'Timberborn.McpServer.dll'))
$serverExe = [IO.Path]::GetFullPath((Join-Path $releaseDir 'Timberborn.McpServer.exe'))
$inventory = @(Get-CimInstance Win32_Process -ErrorAction Stop)
$byId = @{}
foreach ($item in $inventory) { $byId[[int]$item.ProcessId] = $item }
$matches = @($inventory | Where-Object {
    $item = $_
    $normalizedCommand = ([string]$item.CommandLine).Replace('/', '\')
    $pattern = '(?i)(?:^|[\s"])' + [regex]::Escape($serverDll) + '(?:[\s"]|$)'
    $localServer = ($item.Name -eq 'dotnet.exe' -and $normalizedCommand -match $pattern) -or
        ($item.Name -eq 'Timberborn.McpServer.exe' -and [string]$item.ExecutablePath -eq $serverExe)
    if (-not $localServer) { return $false }
    $ancestor = $item
    $codexOwned = $false
    $seen = @{}
    for ($depth = 0; $depth -lt 12; $depth++) {
        $parentId = [int]$ancestor.ParentProcessId
        if ($seen.ContainsKey($parentId) -or -not $byId.ContainsKey($parentId)) { break }
        $seen[$parentId] = $true
        $ancestor = $byId[$parentId]
        if ($ancestor.Name -ieq 'Codex.exe') { $codexOwned = $true; break }
    }
    if (-not $codexOwned) { throw 'Projekt-MCP gefunden, aber Codex-Elternprozess nicht nachgewiesen; nicht beendet.' }
    $true
})
if ($matches.Count -eq 0) {
    Write-Host '[OK] Kein laufender Codex-Timberborn-MCP dieses Release-Pfads gefunden.'
    return
}
foreach ($item in $matches) {
    $process = Get-Process -Id $item.ProcessId -ErrorAction SilentlyContinue
    if ($null -eq $process) { continue }
    Stop-Process -InputObject $process -ErrorAction Stop
    if (-not $process.WaitForExit(5000)) { throw 'Codex-Timberborn-MCP wurde nicht rechtzeitig beendet.' }
    Write-Host ('[OK] Codex-Timberborn-MCP beendet (PID {0}).' -f $item.ProcessId)
}
