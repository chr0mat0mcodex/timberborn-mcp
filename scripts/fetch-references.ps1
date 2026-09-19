$ErrorActionPreference = 'Stop'
$referenceRoot = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$catalog = Get-Content -LiteralPath (Join-Path $referenceRoot 'docs/references/sources.json') -Raw | ConvertFrom-Json
$inventory = @()
foreach ($source in $catalog.sources) {
    if ($source.repository -notin @('mechanistry/timberborn-modding', 'datvm/TimberbornMods') -or
        $source.revision -notmatch '^[a-f0-9]{40}$' -or $source.id -notmatch '^[a-z0-9-]+$') { throw 'Ungültige Referenzquelle.' }
    $cacheDir = Join-Path $referenceRoot ('.local/references/' + $source.id + '/' + $source.revision)
    foreach ($relative in $source.files) {
        if ($relative -match '(^/|\\|(^|/)\.\.(/|$)|:)') { throw 'Ungültiger Referenzpfad.' }
        $target = Join-Path $cacheDir $relative
        $url = 'https://raw.githubusercontent.com/' + $source.repository + '/' + $source.revision + '/' + $relative
        if (-not (Test-Path -LiteralPath $target)) {
            New-Item -ItemType Directory -Force -Path (Split-Path $target) | Out-Null
            Invoke-WebRequest -Uri $url -OutFile $target
        }
        $inventory += [pscustomobject]@{ source = $source.id; revision = $source.revision; file = $relative;
            bytes = (Get-Item -LiteralPath $target).Length; sha256 = (Get-FileHash -LiteralPath $target -Algorithm SHA256).Hash; url = $url }
    }
}
$inventoryPath = Join-Path $referenceRoot '.local/references/inventory.json'
$inventory | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath $inventoryPath -Encoding utf8NoBOM
Write-Output ('Referenzdateien lokal verfügbar: ' + $inventory.Count + '; Bytes: ' + ($inventory | Measure-Object bytes -Sum).Sum)
Write-Output "Inventar mit Quell-URLs und SHA256: $inventoryPath"
Write-Output 'Nur Referenztexte heruntergeladen. Keine Skripte ausgeführt, keine Mods installiert.'
