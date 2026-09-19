# Timberborn MCP

Lokaler C#-MCP-Server für Timberborn, standardmäßig ausschließlich lesend. Umsetzung nach `missionsplan.md`.
Aktueller Vorrang: analysieren und nachweisen, welche Daten und kontrollierten Aktionen
ein spielender Agent braucht. Tatsächlicher Spielbetrieb ist derzeit sekundär.
MCP läuft über stdio; der Spielzugriff wird hinter einem eigenen Backend gekapselt.

Repository: [chr0mat0mcodex/timberborn-mcp](https://github.com/chr0mat0mcodex/timberborn-mcp).
Für die Mitarbeit gelten [Agentenanweisungen](AGENTS.md) und [Entwicklungsablauf](DEVELOPMENT_WORKFLOW.md).
Aktuelle Ergebnisse dokumentiert das [Projektjournal](docs/project-journal.md).
Nächste geplante Ausbaustufe: [Phase 2 — Wasser, Nahrung, Holz, Wege und Wohnraum](docs/phase-2-plan.md).

## Entwicklungsstand

Endziel: Der Agent spielt Timberborn über MCP und baut Wasser-, Nahrungs-, Holzversorgung,
Wege und Wohnraum auf. Agent Bridge 0.2.0 ist über drei MCP-Werkzeuge live geprüft;
Bevölkerung, Beispielbestände und Wohnraum wurden vom Nutzer bestätigt, Wiederverbindung
nach Neuladen erfolgreich. Die Erweiterung 0.3.0 ergänzt Gebäude-/Wegegeometrie,
Pilotkatalog und [rein lesende Bauplatzvorprüfung](docs/spatial-precheck.md); räumlicher MCP-Pilot bestanden.
0.4.0 ergänzt einen [geschützten Spielvalidator-Prototyp](docs/native-validation.md),
lokal gebaut und gepackt, nicht installiert oder live abgenommen.
[Installation und Abnahme](docs/native-bridge-install.md),
[offizielle Quellen und good references](docs/references/README.md).
109 reguläre Tests bestanden; drei separate Live-Tests im Standardlauf übersprungen.
Nativer lesender MCP-Livetest zusätzlich erfolgreich.

Read-only-POC mit More HTTP API und explizitem Fake-Backend implementiert und live getestet.
Optionaler einzelner Schreib-POC ebenfalls implementiert und live geprüft: Holzfällerflagge pausieren
und ursprünglichen Pausenstatus wiederherstellen. 74 reguläre Tests bestanden; Live-Tests separat opt-in.
SDK: .NET 10.0.303. Abhängigkeiten sind zentral gepinnt; Lockfiles gehören ins Repository.
Keine Unity-/Mod-Binärdateien, Saves oder Laufzeitcaches einchecken.

## Projektlokaler Build

In einer PowerShell im Repository:

```powershell
. ./scripts/environment.ps1
dotnet restore TimberbornMcp.slnx --locked-mode --configfile NuGet.Config
dotnet build TimberbornMcp.slnx -c Release --no-restore
dotnet test TimberbornMcp.slnx -c Release --no-build --no-restore
```

`environment.ps1` setzt nur Variablen des aufrufenden Prozesses. Für die Entwicklung eine eigene
PowerShell verwenden; deren Schließen verwirft diese Variablen. Cache- und temporäre Dateien landen
unter `.local/`, Builddateien unter `bin/obj`.

Alternativ `pwsh -NoProfile -File ./scripts/verify.ps1`. Der erste Restore benötigt Netzwerkzugriff,
die späteren Builds und Tests verwenden die gepinnten Pakete. Kein globales Tool wird installiert.

## Spiel vorbereiten — bisheriger More-HTTP-API-POC

Der Nutzer installiert und aktiviert More HTTP API, Moddable Timberborn, Mod Settings, Harmony
und TimberUi. In More HTTP API Auto-start aktivieren, Port einstellen und Testkolonie laden.
Die getestete Kombination steht in docs/compatibility/timberborn.md.
Ping: `http://localhost:8080/MoreHttpApi/ping` muss HTTP 204 liefern.
Für diese Installation ist localhost erforderlich; 127.0.0.1 wurde abgelehnt.

## Server starten

Nach dem Build im Repository:

```powershell
dotnet ./src/Timberborn.McpServer/bin/Release/net10.0/Timberborn.McpServer.dll
```

Der Server wartet auf MCP-Nachrichten an stdin; er ist keine interaktive Shell.
Für einen MCP-Client ist der Befehl `dotnet` und das Argument der **absolute Pfad** zur DLL.
Die Client-Konfiguration erfolgt separat auf ausdrücklichen Auftrag. Am 2026-09-19 wurde der lokale
Codex-Client unter dem Servernamen `timberborn` eingerichtet (stdio, absoluter dotnet-/DLL-Pfad,
Backend `more-http-api`, Basisadresse `http://localhost:8080/`). Die maschinenlokale Konfiguration
gehört nicht ins Repository. Prüfen mit `codex mcp get timberborn --json`.
Diagnose geht nach stderr, stdout enthält ausschließlich Protokollnachrichten.

| Prozessvariable | Default / Bedeutung |
|---|---|
| TIMBERBORN_BACKEND | more-http-api; alternativ ausdrücklich native oder fake |
| TIMBERBORN_NATIVE_CONFIG | Bei native: absoluter Pfad zur privaten bridge.local.json, kein Token im Client-Befehl |
| TIMBERBORN_ENABLE_VALIDATION | Nur `1` bietet im nativen Backend zusätzlich validate_build_site an; Mod-Opt-in ebenfalls erforderlich |
| TIMBERBORN_BASE_URL | http://localhost:8080/; nur HTTP-Loopback, kein Pfad/Query/Login |
| TIMBERBORN_AUTHORIZATION | Optionaler Authorization-Wert; nicht als Argument oder Git-Datei speichern |
| TIMBERBORN_FAKE_SCENARIO | healthy; alternativ partial oder offline; nur für Simulation relevant |
| TIMBERBORN_ENABLE_WRITES | Nur `1` aktiviert zusätzlich set_building_paused; standardmäßig aus |

Beim bisherigen Backend bleiben fünf Tools offline auflistbar: `timberborn_status`, `inspect_colony`,
`inspect_population`, `find_buildings`, `inspect_building`. Parameter und Ergebnisfelder
stehen in missionsplan.md Abschnitt 3. Simulierte Daten sind immer markiert.

Das native Backend bietet sechs eigene lesende Werkzeuge: `timberborn_status`,
`inspect_colony`, `inspect_map_region`, `find_buildings`, `inspect_build_catalog`
und `precheck_build_site`. Letzteres ist keine vollständige Bauvalidierung.
Kein automatischer Backendwechsel;
`TIMBERBORN_ENABLE_WRITES` aktiviert dort keine Schreibfunktionen.

## Optionaler Schreib-POC

`set_building_paused(id, paused, expectedPaused)` verändert genau ein Gebäude. Es prüft frische
Gebäudezugehörigkeit, Pausierbarkeit und Ausgangszustand und liest den Zustand nach dem Request erneut.
Ohne `TIMBERBORN_ENABLE_WRITES=1` wird das Werkzeug weder angeboten noch ausgeführt.
Die bestehende lokale Codex-Konfiguration bleibt standardmäßig lesend.

Ergebnisse: `unchanged` (kein Request nötig), `applied` (Zielzustand nachgelesen),
`rejected` (abgewiesen) oder `unconfirmed` (Änderung möglicherweise ausgeführt).
Bei `unconfirmed` nur lesend klären; kein automatisches Retry oder Zurücksetzen.
Die Vorbedingung ist keine atomare Sperre gegenüber dem Spiel oder anderen Clients.
Das Rücksetzen des Pausenstatus stellt entgangene Produktion nicht wieder her.

Der separate Live-Schreibtest benötigt `TIMBERBORN_LIVE_WRITE_TEST=1` und die ausdrückliche Freigabe
für Hin-/Rückweg an genau einer `LumberjackFlag.Folktails`. Er bricht bei null oder mehreren Treffern ab.
`scripts/verify.ps1` deaktiviert diesen Test ausdrücklich, auch mit `-Live`; `-Live` bleibt rein lesend.

## Tests und Grenzen

```powershell
# Gewöhnliche Tests ohne Spielzugriff:
pwsh -NoProfile -File ./scripts/verify.ps1
# Nur mit vom Nutzer vorbereiteter Testkolonie und aktivierter API:
pwsh -NoProfile -File ./scripts/verify.ps1 -Live
```

Standardtests verwenden synthetische Daten und lokale HTTP-Stubs. Der Live-Test ist explizit opt-in.
Ein leeres Suchergebnis ist Erfolg; unbekannte Werte sind null; Teilfehler bleiben erkennbar.
Seiten sind neue Beobachtungen und kein eingefrorener Spielzustand.
Die API kann im Menü, beim Laden oder nach Mod-Updates ausfallen; es gibt keine automatische
Spielsteuerung oder Reparatur. Die native Mod ergänzt drei Beispielbestände, Betten/Personal,
Objektpositionen und einen begrenzten Gelände-/Wasserausschnitt. Save/Load, Bauen und
Automationsgraphen sind weiterhin nicht implementiert.

Kein Client darf aus `readOnlyHint` alleine Sicherheit ableiten: Der Adapter selbst begrenzt die Routen.
Fremdmod-Routen können auch bei GET Änderungen ausführen; deshalb gibt es kein generisches HTTP-Tool.
