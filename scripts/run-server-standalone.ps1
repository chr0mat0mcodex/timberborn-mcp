param([string]$ConfigPath)
$ErrorActionPreference = 'Stop'
$options = @{}
if ($ConfigPath) { $options.ConfigPath = $ConfigPath }
& "$PSScriptRoot/run-server.ps1" @options
exit $LASTEXITCODE
