# Eigene Agent Bridge installieren und MCP starten

Letzter dokumentierter Abschluss: **0.50.0**, begrenzt live abgenommen.
[Projektstand](../PROJECT_STATE.md) und [Gruppenplanung](efficient-group-planning.md).
Lokale Änderungen sind nicht allein durch dieselbe Versionsnummer abgenommen.
Die Bridge wird gegen Timberborn 1.1.2.4 gebaut. Private Konfiguration bleibt bei
Updates erhalten. Zeit-, Diagnose- und Bauwerkzeuge sind im Werkzeugkatalog beschrieben.
Benötigt wird ausschließlich unsere eigene Spielmod; `RequiredMods` ist leer.
MCP-Server und Mod sind zwei getrennte Prozesse/Komponenten. Kein More-HTTP-API-Setup nötig.

## Voraussetzungen und Paket

- Timberborn mit aktivierbaren lokalen Mods; getestet: 1.1.2.4 / Folktails.
- .NET SDK laut [global.json](../global.json) für den Projektbuild.
- Für den Mod-Build: `Timberborn_Data/Managed` der eigenen Spielinstallation.

Im Repository:

```powershell
# Menschliches Entwickler-Gate bei beendetem Spiel:
pwsh -NoProfile -File ./scripts/prepare-human-live-test.ps1 `
  -TimberbornManagedDir '<Spielverzeichnis>/Timberborn_Data/Managed'
```

Ein neuer Ordner unter `.local/packages/agent-bridge-<Version>-<Zeit>-<ID>/` enthält:

| Datei | Zweck |
| --- | --- |
| Timberborn.AgentBridge.dll | Spielmod |
| Timberborn.Bridge.Core.dll | Transport-/Validierungsbausteine |
| manifest.json | Mod-ID, Version und Metadaten |
| LICENSE | Eigene Quellcode-Lizenz |
| INSTALL.md | Installationshinweise |

Das Code-ZIP enthält nur diese fünf Dateien. Daneben erzeugt der Paketbau im lokalen
Modordner eine **private `bridge.local.json`** mit neuem Token und ausgeschalteten Aktionen.
Diese Datei nicht veröffentlichen oder durch eine fremde Beispielkonfiguration ersetzen.
Spiel-/Unity-DLLs und fremde Mod-DLLs gehören nicht in das Paket.

## Installation und Updates

Das Entwickler-Gate übernimmt genau das Ergebnis seines eigenen Paketbuilds
(Build-ID und konkreter Pfad), nicht das zuletzt geänderte Paketverzeichnis.
Manifestversion, Mod-ID, vollständiges Quellmanifest und die fünf Datei-Hashes
werden vor Installation geprüft. Staging und installierte Dateien müssen denselben
Hashes entsprechen. Fehlendes/inkonsistentes Ergebnis beendet das Gate ohne
Auswahl eines Ersatzpakets. Der lokale Gate-Log nennt Build-ID, Version und Pfad;
die private Konfiguration wird nicht als Paketergebnis ausgegeben.

1. Timberborn für den Dateiaustausch beenden; Speichern ist im Testsystem nicht erforderlich.
2. Vorhandenen eigenen Modordner vollständig sichern.
3. Den Paketordner `TimberbornAgentBridge` unter dem lokalen Timberborn-Modverzeichnis
   installieren (Windows-Standard: `<Dokumente>/Timberborn/Mods/`).
4. Bei Erstinstallation die privat generierte Konfiguration mitnehmen. Bei Updates
   bestehende `bridge.local.json`, Token, Port und gesetzte Freigaben erhalten; nur
   die fünf Paketdateien ersetzen. Mod-ID und Datei-Hashes vergleichen.
5. Spiel starten, eigene Mod aktivieren und den gewünschten Testspielstand laden.

Die Mod startet im geladenen Spielkontext. Im Menü oder beim Laden kann der Dienst
vorübergehend fehlen. Sie fügt keine eigenen Save-Daten hinzu; normale Spielaktionen
wie Bau oder Forschung verändern selbstverständlich den Spielstand.

## Lesender MCP-Einstieg

```powershell
pwsh -NoProfile -File ./scripts/start-native.ps1 `
  -ConfigPath '<absoluter Pfad>/TimberbornAgentBridge/bridge.local.json'
```

Für einen MCP-Client denselben Befehl mit **absolutem Skript- und Konfigurationspfad**
als stdio-Server eintragen. stdout ist ausschließlich MCP, Diagnose geht nach stderr.
Der Starter setzt explizit `TIMBERBORN_BACKEND=native` und aktiviert im Testmodus
alle vorhandenen Aktionsgruppen; `-ReadOnly` deaktiviert sie.
`-DisableValidation` deaktiviert nur das separate Validierungswerkzeug, nicht
die nativen Prüfungen bei Bauaufträgen. Reguläre Spielvalidierung, Kollisionserkennung
und Aktions-IDs bleiben dabei unverändert aktiv.

Für eigene Client-Einträge mit Aktionen stattdessen direkt die gebaute
`src/Timberborn.McpServer/bin/Release/net10.0/Timberborn.McpServer.dll` über `dotnet` starten
und die folgenden Prozessvariablen ausdrücklich setzen:

```text
TIMBERBORN_BACKEND=native
TIMBERBORN_NATIVE_CONFIG=<absoluter Pfad zur privaten bridge.local.json>
```

Der Server lädt `config/server.example.json` nicht automatisch; diese Datei beschreibt
nur Beispiel-Prozessvariablen. Seit dem abgenommenen A11-Piloten: Direkter DLL-Start
**ohne** oder mit leerer Backendvariable bricht mit Exitcode 2 und Konfigurationsmeldung
auf stderr ab. `native`, `more-http-api` (Legacy) oder `fake` ausdrücklich wählen.
Kein automatischer Legacy-Fallback. [Nachweise](backend-selection-pilot.md).
Keine Tokens als Argument oder in öffentliche Client-Beispiele schreiben.

## Aktionsfreigaben

Für jede Gruppe müssen Mod-Konfiguration (`true`) **und** MCP-Prozess (`1`) zustimmen.
Neue Paketkonfigurationen und der Starter aktivieren im Testmodus alle Gruppen.

| Mod-Konfiguration | MCP-Prozessvariable | Werkzeuge |
| --- | --- | --- |
| enableBuildingPlacement | TIMBERBORN_ENABLE_BUILDING_PLACEMENT | validate_building, place_building |
| enableBuildingSettings | TIMBERBORN_ENABLE_BUILDING_SETTINGS | set_building_paused, set_storage_good, set_storage_mode, set_farm_priority, set_farm_crop |
| enableSpeedControl | TIMBERBORN_ENABLE_SPEED_CONTROL | set_simulation_speed, run_simulation_for, run_simulation_until, cancel_simulation_run |
| enableStaffing | TIMBERBORN_ENABLE_STAFFING | set_workplace_staffing |
| enablePriorities | TIMBERBORN_ENABLE_PRIORITIES | set_building_priority |
| enableAreas | TIMBERBORN_ENABLE_AREAS | set_area |
| enableRemoval | TIMBERBORN_ENABLE_REMOVAL | demolish_building, remove_planted, remove_vegetation, remove_debris |
| enableResearch | TIMBERBORN_ENABLE_RESEARCH | unlock_building |
| enableValidation | TIMBERBORN_ENABLE_VALIDATION | validate_build_site (früher Pilot) |
| enablePlacement | TIMBERBORN_ENABLE_PLACEMENT | place_path (früher Pilot) |
| enableLodgePlacement | TIMBERBORN_ENABLE_LODGE_PLACEMENT | place_lodge (früher Pilot) |

`TIMBERBORN_ENABLE_WRITES` gehört zum Legacy-Adapter und aktiviert keine nativen Aktionen.
Die generischen Bauwerkzeuge ersetzen für neue Vorhaben die engen Einzelpiloten.
MCP tools/list enthält konkrete Parameter, Grenzen und erforderliche Erwartungswerte.

## Verbindung, Log und Prüfung

Der interne Modtransport verwendet `http://localhost:<port>/agent-api/v1/` (Paketstandard
8081) mit Bearer-Authentifizierung. Der Dienst ist auf Loopback beschränkt. Browser ohne
Token erhalten 401; es gibt keinen öffentlichen Ping. Das ist kein Fehler im MCP-Setup.
Der Zugriff erfolgt über den MCP-Server und dessen private Konfigurationsdatei.

Im Spiel rechts unten **MCP-Log** öffnen. reasoning ist eine verpflichtende kurze Absicht
für den Spieler; das Fenster und die Aufzeichnung benötigen keinen neuen Aktionsschalter.
[Agentenlog im Werkzeugkatalog](tools.md), [Fehlercodes](bridge-errors.md).

```powershell
pwsh -NoProfile -File ./scripts/verify.ps1 `
  -NativeConfig '<installierte Mod>/bridge.local.json'
```

Prüft normale Tests plus den nativen Lesetest. Keine automatischen Schreibtests.
Nach Updates Version, neue Session und relevante Funktionen gezielt abnehmen; bei
unklarem Aktionsergebnis nur nachlesen. [Entwicklungsablauf](../DEVELOPMENT_WORKFLOW.md).

## Portable Teststarter

`run-server.ps1` verwendet standardmäßig die private Mod-Konfiguration unter
dem Dokumente-Verzeichnis des aktuellen Benutzers; `-ConfigPath` überschreibt
den Pfad. Die vorhandenen `.cmd`-/`.bat`-/Bash- und PowerShell-Varianten sind
schmale Weiterleitungen. Keine Benutzerpfade oder Tokens im Repository.
`start-writes.ps1` verlangt einen expliziten Konfigurationspfad.
Diese direkten Starter ersetzen nicht die bestehende Codex-Supervisor-Verbindung.
