param([switch]$Live, [switch]$InitialRestore)
$ErrorActionPreference = 'Stop'
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
    try {
        $env:TIMBERBORN_LIVE_TEST = if ($Live) { '1' } else { '0' }
        $env:TIMBERBORN_LIVE_WRITE_TEST = '0'
        $env:TIMBERBORN_ENABLE_WRITES = '0'
        $env:TIMBERBORN_NATIVE_LIVE_TEST = '0'
        & dotnet test TimberbornMcp.slnx -c Release --no-build --no-restore
        if ($LASTEXITCODE -ne 0) { throw 'Tests fehlgeschlagen.' }
    } finally {
        $env:TIMBERBORN_LIVE_TEST = $oldLive
        $env:TIMBERBORN_LIVE_WRITE_TEST = $oldWriteTest
        $env:TIMBERBORN_ENABLE_WRITES = $oldWrites
        $env:TIMBERBORN_NATIVE_LIVE_TEST = $oldNativeLive
    }
} finally { Pop-Location }
