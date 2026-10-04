# Presentation only: native diagnostics stay visible, no retries or swallowed errors.
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
