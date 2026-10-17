# Presentation only: native diagnostics stay visible, no retries or swallowed errors.
function Format-TimberbornDuration([TimeSpan]$Elapsed) {
    '{0:00}:{1:00}:{2:00}' -f [int][Math]::Floor($Elapsed.TotalHours), $Elapsed.Minutes, $Elapsed.Seconds
}

function New-TimberbornGateDisplay {
    param([string[]]$Names, [string]$TimingPath)
    Write-Host ''
    Write-Host '  +----------------------------------------------------------+' -ForegroundColor DarkCyan
    Write-Host '  |  TIMBERBORN   /   MCP LIVE TEST GATE                       |' -ForegroundColor Cyan
    Write-Host '  +----------------------------------------------------------+' -ForegroundColor DarkCyan
    Write-Host ('  Start {0:HH:mm:ss}   |   {1} Phasen   |   Stopp beim ersten Fehler' -f (Get-Date), $Names.Count) -ForegroundColor DarkGray
    Write-Host '  Spiel wird hart beendet, ohne Speichern.' -ForegroundColor Yellow
    $references = @{}
    if ($TimingPath -and (Test-Path -LiteralPath $TimingPath)) {
        try {
            $saved = Get-Content -LiteralPath $TimingPath -Raw | ConvertFrom-Json -AsHashtable
            if ($saved.schemaVersion -eq 1 -and $saved.references -is [System.Collections.IDictionary]) {
                foreach ($name in $Names) {
                    $value = $saved.references[$name]
                    if ($value -is [ValueType] -and $value -isnot [bool] -and [double]::IsFinite([double]$value) -and $value -gt 0 -and $value -le 86400) {
                        $references[$name] = [double]$value
                    }
                }
            }
        } catch { Write-Host '  Laufzeitreferenz nicht lesbar; dieser Lauf misst neu.' -ForegroundColor DarkYellow }
    }
    $phases = @($Names | ForEach-Object { [pscustomobject]@{ Name = $_; State = 'OFFEN'; Watch = [Diagnostics.Stopwatch]::new(); Reference = $references[$_] } })
    $display = [hashtable]::Synchronized(@{ Watch = [Diagnostics.Stopwatch]::StartNew(); Phases = $phases; Current = -1
        TimingPath = $TimingPath; References = $references; ProgressWorker = $null })
    Write-Host '  Balken = Zeitschaetzung aus letztem erfolgreichen Phasenlauf; 100% erst bei Erfolg.' -ForegroundColor DarkGray
    $display
}

function Start-TimberbornGateProgress($Display) {
    # Only display work in this runspace. Build/install commands stay synchronous
    # in the caller, preserving their streams, exit codes and fail-fast behavior.
    if ($Host.Name -ne 'ConsoleHost' -or [Console]::IsOutputRedirected -or $ProgressPreference -eq 'SilentlyContinue') { return }
    $runspace = $null; $worker = $null; $stop = $null
    try {
        $stop = [Threading.ManualResetEventSlim]::new($false)
        $runspace = [runspacefactory]::CreateRunspace($Host)
        $runspace.Open()
        $worker = [powershell]::Create()
        $worker.Runspace = $runspace
        [void]$worker.AddScript({
            param($display, $stop)
            try {
                while (-not $stop.Wait(500)) {
                    $index = $display.Current
                    if ($index -lt 0) { continue }
                    $phase = $display.Phases[$index]
                    if ($phase.State -ne 'LAEUFT') { continue }
                    $seconds = $phase.Watch.Elapsed.TotalSeconds
                    $elapsed = '{0:00}:{1:00}:{2:00}' -f [int][Math]::Floor($seconds / 3600), [int][Math]::Floor(($seconds % 3600) / 60), [int][Math]::Floor($seconds % 60)
                    $percent = -1
                    $status = "$elapsed | erster Lauf: Referenz wird gemessen"
                    if ($null -ne $phase.Reference) {
                        $percent = [int][Math]::Min(95, [Math]::Floor(100 * $seconds / $phase.Reference))
                        $note = if ($seconds -gt $phase.Reference) { 'Referenz ueberschritten; arbeitet weiter' } else { 'geschaetzt' }
                        $status = '{0} | Referenz {1:n1} s | {2}' -f $elapsed, $phase.Reference, $note
                    }
                    Write-Progress -Id 650 -Activity ('Phase {0}/{1}: {2}' -f ($index + 1), $display.Phases.Count, $phase.Name) -Status $status -PercentComplete $percent
                }
            } finally { Write-Progress -Id 650 -Activity 'Testgate' -Completed }
        }).AddArgument($Display).AddArgument($stop)
        $handle = $worker.BeginInvoke()
        $Display.ProgressWorker = @{ Worker = $worker; Runspace = $runspace; Stop = $stop; Handle = $handle }
    } catch {
        if ($null -ne $worker) { $worker.Dispose() }
        if ($null -ne $runspace) { $runspace.Dispose() }
        if ($null -ne $stop) { $stop.Dispose() }
        Write-Host '  Livebalken nicht verfuegbar; Laufzeiten und Abschlussbalken bleiben aktiv.' -ForegroundColor DarkYellow
    }
}

function Stop-TimberbornGateProgress($Display) {
    $progress = $Display.ProgressWorker
    if ($null -eq $progress) { return }
    try {
        $progress.Stop.Set()
        if (-not $progress.Handle.AsyncWaitHandle.WaitOne(1500)) { $progress.Worker.Stop() }
        [void]$progress.Worker.EndInvoke($progress.Handle)
    } catch { Write-Host '  Livebalken beendet; Gate-Ergebnis bleibt unveraendert.' -ForegroundColor DarkYellow }
    finally {
        $progress.Worker.Dispose(); $progress.Runspace.Dispose(); $progress.Stop.Dispose()
        $Display.ProgressWorker = $null
    }
}

function Save-TimberbornGateTimings($Display) {
    if (-not $Display.TimingPath) { return }
    $temporary = $null
    try {
        # Failed/unfinished phases never replace a successful duration reference.
        foreach ($phase in $Display.Phases) {
            if ($phase.State -eq 'OK') { $Display.References[$phase.Name] = [Math]::Max(0.01, $phase.Watch.Elapsed.TotalSeconds) }
        }
        $rows = @($Display.Phases | ForEach-Object { @{ name = $_.Name; state = $_.State; seconds = [Math]::Round($_.Watch.Elapsed.TotalSeconds, 3) } })
        $payload = @{ schemaVersion = 1; references = $Display.References; lastRun = $rows }
        $directory = Split-Path -Parent $Display.TimingPath
        [void][IO.Directory]::CreateDirectory($directory)
        $temporary = $Display.TimingPath + '.' + [guid]::NewGuid().ToString('N') + '.tmp'
        $payload | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath $temporary -Encoding utf8NoBOM
        [IO.File]::Move($temporary, $Display.TimingPath, $true)
        Write-Host '  Laufzeiten gespeichert. Erfolgreiche Phasen dienen beim naechsten Lauf als Referenz.' -ForegroundColor DarkGray
    } catch { Write-Host '  Laufzeiten konnten nicht gespeichert werden; Gate-Ergebnis bleibt unveraendert.' -ForegroundColor DarkYellow }
    finally { if ($temporary -and [IO.File]::Exists($temporary)) { try { [IO.File]::Delete($temporary) } catch {} } }
}

function Start-TimberbornGatePhase($Display, [int]$Index) {
    $Display.Current = $Index
    $phase = $Display.Phases[$Index]
    $phase.State = 'LAEUFT'
    $phase.Watch.Start()
    if ($Index -eq 0) { Start-TimberbornGateProgress $Display }
    $done = @($Display.Phases | Where-Object State -EQ 'OK').Count
    $bar = ('#' * $done) + ('.' * ($Display.Phases.Count - $done))
    Write-Host ''
    Write-Host ('  [{0}] {1}/{2} fertig   |   Gesamt {3}' -f $bar, $done, $Display.Phases.Count, (Format-TimberbornDuration $Display.Watch.Elapsed)) -ForegroundColor DarkCyan
    Write-Host ('  >> {0:00}/{1:00}  {2}' -f ($Index + 1), $Display.Phases.Count, $phase.Name) -ForegroundColor Cyan
    if ($null -ne $phase.Reference) { Write-Host ('  Referenz: {0:n1} s (letzter erfolgreicher Phasenlauf)' -f $phase.Reference) -ForegroundColor DarkGray }
}

function Complete-TimberbornGatePhase($Display) {
    $phase = $Display.Phases[$Display.Current]
    $phase.Watch.Stop()
    $phase.State = 'OK'
    Write-TimberbornStatus 'FERTIG' ('[####################] 100%  {0}  |  Dauer {1}  |  Gesamt {2}' -f $phase.Name, (Format-TimberbornDuration $phase.Watch.Elapsed), (Format-TimberbornDuration $Display.Watch.Elapsed)) Green
}

function Write-TimberbornGateSummary($Display, [switch]$Failed) {
    Stop-TimberbornGateProgress $Display
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
        $filled = if ($phase.State -eq 'OK') { 20 } elseif ($phase.State -eq 'OFFEN' -or $null -eq $phase.Reference) { 0 } else { [int][Math]::Min(19, [Math]::Floor(20 * $phase.Watch.Elapsed.TotalSeconds / $phase.Reference)) }
        $bar = ('#' * $filled) + ('.' * (20 - $filled))
        $reference = if ($null -eq $phase.Reference) { 'neu' } else { '{0:n1} s' -f $phase.Reference }
        Write-Host ('  {0,-8} [{1}]  {2}  {3}  | Ref {4}' -f $phase.State, $bar, $duration, $phase.Name, $reference) -ForegroundColor $rowColor
    }
    Write-Host '  ------------------------------------------------------------' -ForegroundColor DarkCyan
    Save-TimberbornGateTimings $Display
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
