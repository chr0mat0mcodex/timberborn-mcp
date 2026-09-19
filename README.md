# Timberborn MCP

Lokaler C#-MCP-Server für lesenden Zugriff auf Timberborn. Umsetzung nach `missionsplan.md`.
MCP läuft über stdio; der Spielzugriff wird hinter einem eigenen Backend gekapselt.

## Entwicklungsstand

Read-only-POC mit More HTTP API und explizitem Fake-Backend implementiert und live getestet.
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

## Spiel vorbereiten

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
Die Client-Konfiguration muss separat vom Nutzer eingerichtet werden; dieses Projekt verändert sie nicht.
Diagnose geht nach stderr, stdout enthält ausschließlich Protokollnachrichten.

| Prozessvariable | Default / Bedeutung |
|---|---|
| TIMBERBORN_BACKEND | more-http-api; alternativ ausdrücklich fake |
| TIMBERBORN_BASE_URL | http://localhost:8080/; nur HTTP-Loopback, kein Pfad/Query/Login |
| TIMBERBORN_AUTHORIZATION | Optionaler Authorization-Wert; nicht als Argument oder Git-Datei speichern |
| TIMBERBORN_FAKE_SCENARIO | healthy; alternativ partial oder offline; nur für Simulation relevant |

Alle fünf Tools bleiben offline auflistbar: `timberborn_status`, `inspect_colony`,
`inspect_population`, `find_buildings`, `inspect_building`. Parameter und Ergebnisfelder
stehen in missionsplan.md Abschnitt 3. Simulierte Daten sind immer markiert.

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
Spielsteuerung oder Reparatur. Ressourcenbestände, Save/Load, Bauen und Automationsgraphen sind nicht enthalten.

Kein Client darf aus `readOnlyHint` alleine Sicherheit ableiten: Der Adapter selbst begrenzt die Routen.
Fremdmod-Routen können auch bei GET Änderungen ausführen; deshalb gibt es kein generisches HTTP-Tool.
