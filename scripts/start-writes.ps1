param([Parameter(Mandatory)][string]$ConfigPath)
$ErrorActionPreference = 'Stop'
& "$PSScriptRoot/start-native.ps1" -ConfigPath $ConfigPath
exit $LASTEXITCODE
