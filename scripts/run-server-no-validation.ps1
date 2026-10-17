param([string]$ConfigPath)
$ErrorActionPreference = 'Stop'
$options = @{}
if ($ConfigPath) { $options.ConfigPath = $ConfigPath }
& "$PSScriptRoot/run-server.ps1" @options -DisableValidation
exit $LASTEXITCODE
