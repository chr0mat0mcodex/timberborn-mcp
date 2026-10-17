param(
    [string]$ConfigPath = (Join-Path ([Environment]::GetFolderPath('MyDocuments')) 'Timberborn/Mods/TimberbornAgentBridge/bridge.local.json'),
    [switch]$DisableValidation
)
$ErrorActionPreference = 'Stop'
& "$PSScriptRoot/start-native.ps1" -ConfigPath $ConfigPath -DisableValidation:$DisableValidation
exit $LASTEXITCODE
