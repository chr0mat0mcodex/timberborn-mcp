# Console presentation only. Gate commands remain synchronous; all their streams
# are captured by prepare-human-live-test.ps1. Console.Write bypasses that capture.
function Format-TimberbornDuration([TimeSpan]$Elapsed) {
    '{0:00}:{1:00}:{2:00}' -f [int][Math]::Floor($Elapsed.TotalHours), $Elapsed.Minutes, $Elapsed.Seconds
}

function New-TimberbornGateDisplay {
    param([string[]]$Names, [string]$TimingPath)
    $references = @{}
    $note = 'Zeitbalken lernen aus erfolgreichen Phasen'
    if ($TimingPath -and (Test-Path -LiteralPath $TimingPath)) {
        try {
            $saved = Get-Content -LiteralPath $TimingPath -Raw | ConvertFrom-Json -AsHashtable
            if ($saved.schemaVersion -eq 1 -and $saved.references -is [Collections.IDictionary]) {
                foreach ($name in $Names) {
                    $value = $saved.references[$name]
                    if ($value -is [ValueType] -and $value -isnot [bool] -and [double]::IsFinite([double]$value) -and $value -gt 0 -and $value -le 86400) {
                        $references[$name] = [double]$value
                    }
                }
            }
        } catch { $note = 'Referenz unlesbar; Laufzeiten werden neu gemessen' }
    }
    $interactive = $Host.Name -eq 'ConsoleHost' -and -not [Console]::IsOutputRedirected -and $Host.UI.SupportsVirtualTerminal
    $phases = @($Names | ForEach-Object { [pscustomobject]@{ Name = $_; State = 'OFFEN'; Watch = [Diagnostics.Stopwatch]::new(); Reference = $references[$_] } })
    [hashtable]::Synchronized(@{ Watch = [Diagnostics.Stopwatch]::StartNew(); Phases = $phases; Current = -1
        TimingPath = $TimingPath; References = $references; ProgressWorker = $null; Interactive = $interactive
        FrameLines = 0; RenderLock = [object]::new(); Note = $note; Outcome = 'BEREIT'; LogPath = ''; Failure = '' })
}

function Write-TimberbornCockpit($Display) {
    [Threading.Monitor]::Enter($Display.RenderLock)
    try {
        $width = 88
        if ($Display.Interactive) { $width = [Math]::Max(20, [Console]::WindowWidth - 2) }
        $done = @($Display.Phases | Where-Object State -EQ 'OK').Count
        # ANSI support does not imply that the output encoding supports box glyphs.
        $solid = '#'
        $empty = '-'
        $referenceSeconds = 0.0
        $referencesComplete = $true
        foreach ($phase in $Display.Phases) {
            if ($null -ne $phase.Reference) { $referenceSeconds += $phase.Reference }
            else { $referencesComplete = $false }
        }
        $totalReference = if ($referencesComplete) { Format-TimberbornDuration ([TimeSpan]::FromSeconds($referenceSeconds)) } else { '--:--:--' }
        $phaseLines = [Collections.Generic.List[string]]::new()
        $totalProgress = 0.0
        for ($i = 0; $i -lt $Display.Phases.Count; $i++) {
            $phase = $Display.Phases[$i]
            $seconds = $phase.Watch.Elapsed.TotalSeconds
            $filled = 0; $percent = ' -- '; $marker = ' '; $barSize = 16
            if ($phase.State -eq 'OK') { $filled = $barSize; $percent = '100%'; $marker = '+' }
            elseif ($phase.State -eq 'LAEUFT' -or $phase.State -eq 'FEHLER') {
                $marker = if ($phase.State -eq 'FEHLER') { '!' } else { '>' }
                if ($null -ne $phase.Reference) {
                    $value = [int][Math]::Min(95,[Math]::Floor(100 * $seconds / $phase.Reference))
                    $filled = [int][Math]::Floor($barSize * $value / 100)
                    $percent = '{0,3}%' -f $value
                } else { $percent = ' .. ' }
            }
            $bar = ($solid * $filled) + ($empty * ($barSize - $filled))
            if ($phase.State -eq 'OK') { $totalProgress += 1 }
            elseif ($null -ne $phase.Reference -and $phase.State -in @('LAEUFT','FEHLER')) {
                $totalProgress += [Math]::Min(0.95, $seconds / $phase.Reference)
            }
            if ($phase.State -eq 'LAEUFT' -and $null -eq $phase.Reference) {
                $pos = [int][Math]::Floor($seconds * 2) % ($barSize - 2)
                $bar = ($empty * $pos) + ($solid * 3) + ($empty * ($barSize - $pos - 3))
            }
            $name = $phase.Name
            if ($i -eq 0) { $name = 'Spiel / MCP / Umgebung' }
            $nameWidth = [Math]::Max(8, [Math]::Min(28, $width - 64))
            if ($name.Length -gt $nameWidth) { $name = $name.Substring(0,$nameWidth - 1) + '~' }
            $elapsed = if ($phase.State -eq 'OFFEN') { '--:--:--' } else { Format-TimberbornDuration $phase.Watch.Elapsed }
            $last = if ($null -eq $phase.Reference) { '--:--:--' } else { Format-TimberbornDuration ([TimeSpan]::FromSeconds($phase.Reference)) }
            $phaseLines.Add(('  {0} {1:00} {2} [{3}] {4} {5}  Ref {6}' -f $marker, ($i + 1), $name.PadRight($nameWidth), $bar, $percent, $elapsed, $last))
        }
        $totalPercent = if ($Display.Phases.Count -gt 0) { [int][Math]::Floor(100 * $totalProgress / $Display.Phases.Count) } else { 0 }
        $totalBarWidth = [Math]::Max(1, $width - 6)
        $totalFilled = [int][Math]::Floor($totalBarWidth * $totalPercent / 100)
        $totalBar = '  [' + ($solid * $totalFilled) + ($empty * ($totalBarWidth - $totalFilled)) + ']'
        $lines = [Collections.Generic.List[string]]::new()
        $lines.Add('  TIMBERBORN / MCP    ::    TESTGATE')
        $lines.Add('  ' + ('=' * [Math]::Max(1,$width - 4)))
        $lines.Add(('  GESAMT {0,3}%  {1}/{2}  Ref {3}  Lauf {4}  {5}' -f $totalPercent, $done, $Display.Phases.Count, $totalReference, (Format-TimberbornDuration $Display.Watch.Elapsed), $Display.Outcome))
        $lines.Add($totalBar)
        $lines.Add($totalBar)
        $lines.Add('')
        $lines.AddRange($phaseLines)
        $lines.Add('')
        $lines.Add('  ' + ('-' * [Math]::Max(1,$width - 4)))
        $lines.Add('  Balken: Zeitschaetzung. Voll erst nach Erfolg.')
        $lines.Add('  Ref: letzte erfolgreiche Phasenzeit; Gesamt = Summe.')
        $active = if ($Display.Current -ge 0) { $Display.Phases[$Display.Current] } else { $null }
        $note = $Display.Note
        if ($null -ne $active -and $active.State -eq 'LAEUFT' -and $null -ne $active.Reference -and $active.Watch.Elapsed.TotalSeconds -gt $active.Reference) {
            $note = 'Referenzzeit ueberschritten - Phase arbeitet weiter'
        }
        $lines.Add('  ' + $note)
        $lines.Add('  Spielstopp ohne Save | Abbruch beim ersten Fehler')
        $esc = [char]27
        $frame = [Text.StringBuilder]::new()
        if ($Display.Interactive -and $Display.FrameLines -gt 0) { [void]$frame.Append("$esc[$($Display.FrameLines)F") }
        for ($i = 0; $i -lt $lines.Count; $i++) {
            $line = $lines[$i]
            if ($line.Length -gt $width) { $line = $line.Substring(0,$width) }
            if ($Display.Interactive) {
                $color = if ($line.StartsWith('  ! ')) { '31' } elseif ($line.StartsWith('  + ') -or $line.StartsWith('  GESAMT ') -or $line.StartsWith('  [')) { '32' } elseif ($line.StartsWith('  > ')) { '96' } else { '36' }
                [void]$frame.Append("$esc[2K$esc[${color}m$line$esc[0m")
            } else { [void]$frame.Append($line) }
            [void]$frame.Append([Environment]::NewLine)
        }
        [Console]::Write($frame.ToString())
        $Display.FrameLines = $lines.Count
    } catch { $Display.Interactive = $false }
    finally { [Threading.Monitor]::Exit($Display.RenderLock) }
}

function Start-TimberbornGateProgress($Display) {
    if (-not $Display.Interactive -or $null -ne $Display.ProgressWorker) { return }
    $runspace = $null; $worker = $null; $stop = $null
    try {
        $stop = [Threading.ManualResetEventSlim]::new($false)
        $runspace = [runspacefactory]::CreateRunspace(); $runspace.Open()
        $worker = [powershell]::Create(); $worker.Runspace = $runspace
        $render = 'function Format-TimberbornDuration {' + ${function:Format-TimberbornDuration}.ToString() + "}`n" +
            'function Write-TimberbornCockpit {' + ${function:Write-TimberbornCockpit}.ToString() + "}`n"
        [void]$worker.AddScript('param($display, $stop)' + "`n" + $render + 'while (-not $stop.Wait(250)) { if ($display.Interactive) { Write-TimberbornCockpit $display } }').AddArgument($Display).AddArgument($stop)
        $handle = $worker.BeginInvoke()
        $Display.ProgressWorker = @{ Worker = $worker; Runspace = $runspace; Stop = $stop; Handle = $handle }
    } catch {
        if ($null -ne $worker) { $worker.Dispose() }
        if ($null -ne $runspace) { $runspace.Dispose() }
        if ($null -ne $stop) { $stop.Dispose() }
        $Display.Note = 'Liveanzeige nicht verfuegbar; Abschluss bleibt sichtbar'
    }
}

function Stop-TimberbornGateProgress($Display) {
    $progress = $Display.ProgressWorker
    if ($null -eq $progress) { return }
    try {
        $progress.Stop.Set()
        if (-not $progress.Handle.AsyncWaitHandle.WaitOne(1500)) { $progress.Worker.Stop() }
        [void]$progress.Worker.EndInvoke($progress.Handle)
    } catch { $Display.Note = 'Anzeige beendet; Gate-Ergebnis unveraendert' }
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
        $Display.Note = 'Referenzzeiten gespeichert'
    } catch { $Display.Note = 'Referenzzeiten nicht gespeichert' }
    finally { if ($temporary -and [IO.File]::Exists($temporary)) { try { [IO.File]::Delete($temporary) } catch {} } }
}

function Start-TimberbornGatePhase($Display, [int]$Index) {
    $Display.Current = $Index
    $phase = $Display.Phases[$Index]
    $phase.State = 'LAEUFT'; $phase.Watch.Start(); $Display.Outcome = 'LAEUFT'
    if ($Display.Interactive) { Write-TimberbornCockpit $Display }
    if ($Index -eq 0) { Start-TimberbornGateProgress $Display }
}

function Complete-TimberbornGatePhase($Display) {
    $phase = $Display.Phases[$Display.Current]
    $phase.Watch.Stop(); $phase.State = 'OK'
    if ($Display.Interactive) { Write-TimberbornCockpit $Display }
}

function Write-TimberbornGateSummary($Display, [switch]$Failed) {
    Stop-TimberbornGateProgress $Display
    $Display.Watch.Stop()
    if ($Failed -and $Display.Current -ge 0) {
        $phase = $Display.Phases[$Display.Current]
        if ($phase.State -eq 'LAEUFT') { $phase.Watch.Stop(); $phase.State = 'FEHLER' }
    }
    $Display.Outcome = if ($Failed) { 'ABGEBROCHEN' } else { 'GATE FERTIG' }
    Save-TimberbornGateTimings $Display
    Write-TimberbornCockpit $Display
    if ($Failed) { [Console]::WriteLine('  Ursache: ' + ($Display.Failure -replace '[\r\n\x1b]', ' ')) }
    else { [Console]::WriteLine('  Spielstand laden und "live bereit" schreiben.') }
    [Console]::WriteLine('  Details: ' + $Display.LogPath)
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
