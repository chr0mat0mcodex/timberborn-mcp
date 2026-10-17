param(
    [Parameter(Mandatory)][string]$ConfigPath,
    [switch]$ReadOnly,
    [switch]$DisableValidation
)
$ErrorActionPreference = 'Stop'
# Invoke in a dedicated process: stdout is reserved for MCP.
$config = (Resolve-Path -LiteralPath $ConfigPath).Path
if (-not (Test-Path -LiteralPath $config -PathType Leaf)) { throw 'Private Bridge-Konfiguration fehlt.' }
$server = Join-Path (Split-Path $PSScriptRoot -Parent) 'src/Timberborn.McpServer/bin/Release/net10.0/Timberborn.McpServer.dll'
if (-not (Test-Path -LiteralPath $server -PathType Leaf)) { throw 'MCP-Build fehlt. Menschliches Test-Gate ausführen.' }
$env:TIMBERBORN_BACKEND = 'native'
$env:TIMBERBORN_NATIVE_CONFIG = $config
$enabled = if ($ReadOnly) { '0' } else { '1' }
foreach ($flag in @('WRITES','VALIDATION','PLACEMENT','LODGE_PLACEMENT','SPEED_CONTROL','STAFFING','PRIORITIES','AREAS','BUILDING_SETTINGS','BUILDING_PLACEMENT','REMOVAL','RESEARCH')) {
    [Environment]::SetEnvironmentVariable('TIMBERBORN_ENABLE_' + $flag, $enabled, 'Process')
}
if ($DisableValidation) { $env:TIMBERBORN_ENABLE_VALIDATION = '0' }
& dotnet $server
exit $LASTEXITCODE
