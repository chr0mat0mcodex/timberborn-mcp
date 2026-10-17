# Presentation only: native diagnostics stay visible, no retries or swallowed errors.
function Format-TimberbornDuration([TimeSpan]$Elapsed) {
    '{0:00}:{1:00}:{2:00}' -f [int][Math]::Floor($Elapsed.TotalHours), $Elapsed.Minutes, $Elapsed.Seconds
}

function New-TimberbornGateDisplay {
    param([string[]]$Names)
    Write-Host ''
    Write-Host '  +----------------------------------------------------------+' -ForegroundColor DarkCyan
    Write-Host '  |  TIMBERBORN   /   MCP LIVE TEST GATE                       |' -ForegroundColor Cyan
    Write-Host '  +----------------------------------------------------------+' -ForegroundColor DarkCyan
    Write-Host ('  Start {0:HH:mm:ss}   |   {1} Phasen   |   Stopp beim ersten Fehler' -f (Get-Date), $Names.Count) -ForegroundColor DarkGray
    Write-Host '  Spiel wird hart beendet, ohne Speichern.' -ForegroundColor Yellow
    $phases = @($Names | ForEach-Object { [pscustomobject]@{ Name = $_; State = 'OFFEN'; Watch = [Diagnostics.Stopwatch]::new() } })
    [pscustomobject]@{ Watch = [Diagnostics.Stopwatch]::StartNew(); Phases = $phases; Current = -1 }
}

function Start-TimberbornGatePhase($Display, [int]$Index) {
    $Display.Current = $Index
    $phase = $Display.Phases[$Index]
    $phase.State = 'LAEUFT'
    $phase.Watch.Start()
    $done = @($Display.Phases | Where-Object State -EQ 'OK').Count
    $bar = ('#' * $done) + ('.' * ($Display.Phases.Count - $done))
    Write-Host ''
    Write-Host ('  [{0}] {1}/{2} fertig   |   Gesamt {3}' -f $bar, $done, $Display.Phases.Count, (Format-TimberbornDuration $Display.Watch.Elapsed)) -ForegroundColor DarkCyan
    Write-Host ('  >> {0:00}/{1:00}  {2}' -f ($Index + 1), $Display.Phases.Count, $phase.Name) -ForegroundColor Cyan
}

function Complete-TimberbornGatePhase($Display) {
    $phase = $Display.Phases[$Display.Current]
    $phase.Watch.Stop()
    $phase.State = 'OK'
    Write-TimberbornStatus 'FERTIG' ('{0}  |  Dauer {1}  |  Gesamt {2}' -f $phase.Name, (Format-TimberbornDuration $phase.Watch.Elapsed), (Format-TimberbornDuration $Display.Watch.Elapsed)) Green
}

function Write-TimberbornGateSummary($Display, [switch]$Failed) {
    $Display.Watch.Stop()
    if ($Failed -and $Display.Current -ge 0) {
        $phase = $Display.Phases[$Display.Current]
        if ($phase.State -eq 'LAEUFT') { $phase.Watch.Stop(); $phase.State = 'FEHLER' }
    }
    $done = @($Display.Phases | Where-Object State -EQ 'OK').Count
    $color = if ($Failed) { 'Red' } else { 'Green' }
    $label = if ($Failed) { 'ABGEBROCHEN' } else { 'GATE FERTIG' }
    Write-Host ''
    Write-Host '  ------------------------------------------------------------' -ForegroundColor DarkCyan
    Write-Host ('  {0}   |   {1}/{2} fertig   |   Laufzeit {3}' -f $label, $done, $Display.Phases.Count, (Format-TimberbornDuration $Display.Watch.Elapsed)) -ForegroundColor $color
    foreach ($phase in $Display.Phases) {
        $rowColor = switch ($phase.State) { 'OK' { 'Green' }; 'FEHLER' { 'Red' }; default { 'DarkGray' } }
        $duration = if ($phase.State -eq 'OFFEN') { '--:--:--' } else { Format-TimberbornDuration $phase.Watch.Elapsed }
        Write-Host ('  {0,-8}  {1}  {2}' -f $phase.State, $duration, $phase.Name) -ForegroundColor $rowColor
    }
    Write-Host '  ------------------------------------------------------------' -ForegroundColor DarkCyan
}

function Write-TimberbornStatus {
    param([string]$Label, [string]$Message, [ConsoleColor]$Color = 'Cyan')
    Write-Host ("  [{0}] {1}" -f $Label, $Message) -ForegroundColor $Color
}

function Invoke-TimberbornStep {
    param([string]$Name, [scriptblock]$Action)
    $stepWatch = [Diagnostics.Stopwatch]::StartNew()
    Write-TimberbornStatus 'START' $Name
    try {
        & $Action
        Write-TimberbornStatus 'OK' ("{0} ({1:n1} s)" -f $Name, $stepWatch.Elapsed.TotalSeconds) Green
    } catch {
        Write-TimberbornStatus 'FEHLER' ("{0} ({1:n1} s) — {2}" -f $Name, $stepWatch.Elapsed.TotalSeconds, $_.Exception.Message) Red
        throw
    }
}
