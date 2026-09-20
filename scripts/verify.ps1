param([switch]$Live, [switch]$InitialRestore, [string]$NativeConfig)
$ErrorActionPreference = 'Stop'
if ($Live -and $NativeConfig) { throw 'Legacy- und nativen Livetest getrennt ausführen.' }
if ($NativeConfig) { $NativeConfig = (Resolve-Path -LiteralPath $NativeConfig).Path }
. "$PSScriptRoot/environment.ps1"
Push-Location $taskRoot
try {
    $restoreArgs = @('restore', 'TimberbornMcp.slnx', '--configfile', 'NuGet.Config')
    if (-not $InitialRestore) { $restoreArgs += '--locked-mode' }
    & dotnet @restoreArgs
    if ($LASTEXITCODE -ne 0) { throw 'Restore fehlgeschlagen.' }
    & dotnet build TimberbornMcp.slnx -c Release --no-restore
    if ($LASTEXITCODE -ne 0) { throw 'Build fehlgeschlagen.' }
    $oldLive = $env:TIMBERBORN_LIVE_TEST
    $oldWriteTest = $env:TIMBERBORN_LIVE_WRITE_TEST
    $oldWrites = $env:TIMBERBORN_ENABLE_WRITES
    $oldNativeLive = $env:TIMBERBORN_NATIVE_LIVE_TEST
    $oldValidation = $env:TIMBERBORN_ENABLE_VALIDATION
    $oldPlacement = $env:TIMBERBORN_ENABLE_PLACEMENT
    $oldLodgePlacement = $env:TIMBERBORN_ENABLE_LODGE_PLACEMENT
    $oldSpeedControl = $env:TIMBERBORN_ENABLE_SPEED_CONTROL
    $oldStaffing = $env:TIMBERBORN_ENABLE_STAFFING
    $oldPriorities = $env:TIMBERBORN_ENABLE_PRIORITIES
    $oldAreas = $env:TIMBERBORN_ENABLE_AREAS
    $oldBuildingSettings = $env:TIMBERBORN_ENABLE_BUILDING_SETTINGS
    $oldBuildingPlacement = $env:TIMBERBORN_ENABLE_BUILDING_PLACEMENT
    $oldRemoval = $env:TIMBERBORN_ENABLE_REMOVAL
    $oldNativeConfig = $env:TIMBERBORN_NATIVE_CONFIG
    try {
        $env:TIMBERBORN_LIVE_TEST = if ($Live) { '1' } else { '0' }
        $env:TIMBERBORN_LIVE_WRITE_TEST = '0'
        $env:TIMBERBORN_ENABLE_WRITES = '0'
        $env:TIMBERBORN_NATIVE_LIVE_TEST = '0'
        $env:TIMBERBORN_ENABLE_VALIDATION = '0'
        $env:TIMBERBORN_ENABLE_PLACEMENT = '0'
        $env:TIMBERBORN_ENABLE_LODGE_PLACEMENT = '0'
        $env:TIMBERBORN_ENABLE_SPEED_CONTROL = '0'
        $env:TIMBERBORN_ENABLE_STAFFING = '0'
        $env:TIMBERBORN_ENABLE_PRIORITIES = '0'
        $env:TIMBERBORN_ENABLE_AREAS = '0'
        $env:TIMBERBORN_ENABLE_BUILDING_SETTINGS = '0'
        $env:TIMBERBORN_ENABLE_BUILDING_PLACEMENT = '0'
        $env:TIMBERBORN_ENABLE_REMOVAL = '0'
        & dotnet test TimberbornMcp.slnx -c Release --no-build --no-restore
        if ($LASTEXITCODE -ne 0) { throw 'Tests fehlgeschlagen.' }
        if ($NativeConfig) {
            $env:TIMBERBORN_NATIVE_CONFIG = $NativeConfig
            $env:TIMBERBORN_NATIVE_LIVE_TEST = '1'
            & dotnet test tests/Timberborn.IntegrationTests/Timberborn.IntegrationTests.csproj -c Release --no-build --no-restore --filter FullyQualifiedName~LiveNativeTests
            if ($LASTEXITCODE -ne 0) { throw 'Nativer Lesetest fehlgeschlagen. Kein automatischer Wiederholungsversuch.' }
        }
    } finally {
        $env:TIMBERBORN_LIVE_TEST = $oldLive
        $env:TIMBERBORN_LIVE_WRITE_TEST = $oldWriteTest
        $env:TIMBERBORN_ENABLE_WRITES = $oldWrites
        $env:TIMBERBORN_NATIVE_LIVE_TEST = $oldNativeLive
        $env:TIMBERBORN_ENABLE_VALIDATION = $oldValidation
        $env:TIMBERBORN_ENABLE_PLACEMENT = $oldPlacement
        $env:TIMBERBORN_ENABLE_LODGE_PLACEMENT = $oldLodgePlacement
        $env:TIMBERBORN_ENABLE_SPEED_CONTROL = $oldSpeedControl
        $env:TIMBERBORN_ENABLE_STAFFING = $oldStaffing
        $env:TIMBERBORN_ENABLE_PRIORITIES = $oldPriorities
        $env:TIMBERBORN_ENABLE_AREAS = $oldAreas
        $env:TIMBERBORN_ENABLE_BUILDING_SETTINGS = $oldBuildingSettings
        $env:TIMBERBORN_ENABLE_BUILDING_PLACEMENT = $oldBuildingPlacement
        $env:TIMBERBORN_ENABLE_REMOVAL = $oldRemoval
        $env:TIMBERBORN_NATIVE_CONFIG = $oldNativeConfig
    }
} finally { Pop-Location }
