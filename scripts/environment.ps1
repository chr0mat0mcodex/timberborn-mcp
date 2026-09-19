$taskRoot = Split-Path $PSScriptRoot -Parent
$env:DOTNET_CLI_HOME = Join-Path $taskRoot '.local/dotnet'
$env:NUGET_PACKAGES = Join-Path $taskRoot '.local/packages'
$env:NUGET_HTTP_CACHE_PATH = Join-Path $taskRoot '.local/nuget-http'
$env:NUGET_PLUGINS_CACHE_PATH = Join-Path $taskRoot '.local/nuget-plugins'
$env:DOTNET_CLI_TELEMETRY_OPTOUT = '1'
$env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE = '1'
$env:DOTNET_NOLOGO = '1'
$env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = 'false'
$env:APPDATA = Join-Path $taskRoot '.local/appdata'
$env:LOCALAPPDATA = Join-Path $taskRoot '.local/localappdata'
$env:TEMP = Join-Path $taskRoot '.local/temp'
$env:TMP = $env:TEMP
New-Item -ItemType Directory -Force $env:APPDATA, $env:LOCALAPPDATA, $env:TEMP | Out-Null
