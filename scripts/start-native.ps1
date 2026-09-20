param([Parameter(Mandatory)][string]$ConfigPath)
$ErrorActionPreference = 'Stop'
# Invoke in a dedicated process: stdout is reserved for MCP.
$config = (Resolve-Path -LiteralPath $ConfigPath).Path
if (-not (Test-Path -LiteralPath $config -PathType Leaf)) { throw 'Private Bridge-Konfiguration fehlt.' }
$server = Join-Path (Split-Path $PSScriptRoot -Parent) 'src/Timberborn.McpServer/bin/Release/net10.0/Timberborn.McpServer.dll'
if (-not (Test-Path -LiteralPath $server -PathType Leaf)) { throw 'MCP-Build fehlt. Zuerst scripts/verify.ps1 ausführen.' }
$env:TIMBERBORN_BACKEND = 'native'
$env:TIMBERBORN_NATIVE_CONFIG = $config
$env:TIMBERBORN_ENABLE_WRITES = '0'
$env:TIMBERBORN_ENABLE_VALIDATION = '0'
$env:TIMBERBORN_ENABLE_PLACEMENT = '0'
$env:TIMBERBORN_ENABLE_LODGE_PLACEMENT = '0'
$env:TIMBERBORN_ENABLE_SPEED_CONTROL = '0'
$env:TIMBERBORN_ENABLE_STAFFING = '0'
& dotnet $server
exit $LASTEXITCODE
