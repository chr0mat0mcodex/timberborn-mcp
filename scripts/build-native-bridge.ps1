param([Parameter(Mandatory)][string]$TimberbornManagedDir, [int]$Port = 8081)
$ErrorActionPreference = 'Stop'
if ($Port -lt 1024 -or $Port -gt 65535) { throw 'Ungültiger Port.' }
. "$PSScriptRoot/environment.ps1"
. "$PSScriptRoot/cli-output.ps1"
$managed = (Resolve-Path -LiteralPath $TimberbornManagedDir).Path
Push-Location $taskRoot
try {
    $project = 'mod/Timberborn.AgentBridge/Timberborn.AgentBridge.csproj'
    Invoke-TimberbornStep 'Mod-Abhängigkeiten prüfen' {
        & dotnet restore $project --locked-mode --configfile NuGet.Config "-p:TimberbornManagedDir=$managed" --verbosity quiet --nologo
        if ($LASTEXITCODE -ne 0) { throw 'Mod-Restore fehlgeschlagen.' }
    }
    Invoke-TimberbornStep 'Native Bridge kompilieren' {
        & dotnet build $project -c Release --no-restore "-p:TimberbornManagedDir=$managed" --verbosity quiet --nologo
        if ($LASTEXITCODE -ne 0) { throw 'Mod-Build fehlgeschlagen.' }
    }
    $version = (Get-Content -LiteralPath 'mod/Timberborn.AgentBridge/manifest.json' -Raw | ConvertFrom-Json).Version
    if ($version -notmatch '^\d+\.\d+\.\d+$') { throw 'Ungültige Paketversion.' }
    $packageRoot = Join-Path $taskRoot ('.local/packages/agent-bridge-' + $version + '-' + (Get-Date -Format 'yyyyMMdd-HHmmss') + '-' + [guid]::NewGuid().ToString('N').Substring(0, 8))
    $installDir = Join-Path $packageRoot 'TimberbornAgentBridge'
    New-Item -ItemType Directory -Path $installDir | Out-Null
    $buildDir = Join-Path $taskRoot 'mod/Timberborn.AgentBridge/bin/Release/netstandard2.1'
    foreach ($name in @('Timberborn.AgentBridge.dll', 'Timberborn.Bridge.Core.dll')) {
        Copy-Item -LiteralPath (Join-Path $buildDir $name) -Destination $installDir
    }
    Copy-Item -LiteralPath 'mod/Timberborn.AgentBridge/manifest.json' -Destination $installDir
    Copy-Item -LiteralPath 'LICENSE' -Destination $installDir
    Copy-Item -LiteralPath 'docs/native-bridge-install.md' -Destination (Join-Path $installDir 'INSTALL.md')
    # The distributable archive excludes credentials and all game/vendor DLLs.
    Compress-Archive -LiteralPath $installDir -DestinationPath (Join-Path $packageRoot 'TimberbornAgentBridge-code-only.zip')
    $privateConfig = Join-Path $installDir 'bridge.local.json'
    $token = [Convert]::ToHexString([System.Security.Cryptography.RandomNumberGenerator]::GetBytes(32))
    @{ port = $Port; token = $token; enableValidation = $false; enablePlacement = $false; enableLodgePlacement = $false; enableSpeedControl = $false; enableStaffing = $false; enablePriorities = $false; enableAreas = $false; enableRemoval = $false; enableBuildingPlacement = $false; enableBuildingSettings = $false; enableResearch = $false } | ConvertTo-Json | Set-Content -LiteralPath $privateConfig -Encoding utf8NoBOM
    Write-TimberbornStatus 'PAKET' "Version $version erstellt; Installation folgt separat." Green
    Write-Host "  Paket: $packageRoot" -ForegroundColor DarkGray
} finally { Pop-Location }
