# Timberborn MCP

Lokaler C#-MCP-Server für Timberborn, standardmäßig ausschließlich lesend. Umsetzung nach `missionsplan.md`.
Aktueller Nutzerauftrag: den generischen Bauzugang fertigstellen und anschließend
im Entwicklungsspielstand Grundversorgung praktisch aufbauen. Fähigkeitsanalyse
und getrennte Nachweise für Auftrag, Fertigstellung und Wirkung bleiben Grundlage.
MCP läuft über stdio; der Spielzugriff wird hinter einem eigenen Backend gekapselt.

Repository: [chr0mat0mcodex/timberborn-mcp](https://github.com/chr0mat0mcodex/timberborn-mcp).
Für die Mitarbeit gelten [Agentenanweisungen](AGENTS.md) und [Entwicklungsablauf](DEVELOPMENT_WORKFLOW.md).
Aktuelle Ergebnisse dokumentiert das [Projektjournal](docs/project-journal.md).
Nächste geplante Ausbaustufe: [Phase 2 — Wasser, Nahrung, Holz, Wege und Wohnraum](docs/phase-2-plan.md).

## Entwicklungsstand

Neu: 0.17.0 mit [Forschung und regul�rer Geb�udefreischaltung](docs/research.md).
19 native Leser; unlock_building mit eigenem Opt-in. 378 Tests bestanden, 0.17.0 installiert; Live-Abnahme offen.

0.16.0 mit [Ingame-MCP-Log und optionaler Aktionsbegründung](docs/activity-log.md).
Fenster mit 128 Aufrufen, Status, Parameterzusammenfassung und reasoning; 18 native Leser.
Installiert/live bestätigt: Fenster sichtbar, Abfragen und Pumpenaktionen mit Begründungen protokolliert.

0.15.0 ergänzt [Lager-/Farmoptionen und Gebäudepause](docs/building-settings.md).
17 native Leser; fünf neue Schreibwerkzeuge mit eigener Freigabe. Anbauflächen
werden über die bestehenden Flächenwerkzeuge gesteuert. Automatisch geprüft;
Live bestätigt: Pause, Lagerwahl/-modi, Farmpriorität/Pflanzenwahl und Flächenmarkierungen.
Enthält die live bestätigte Layoutkorrektur aus 0.14.1.
Karottenkette live nachgewiesen: vier Pflanzen angebaut, 12 Karotten eingelagert,
vier neue Pflanzen nach der Ernte beobachtet. Ein Zyklus, keine Gesamtversorgungszusage.
Wassertank regulär gebaut und mit 30/30 Wasser gefüllt; positiver Gesamtnachschub
im ersten Messintervall, langfristige Wasserbilanz weiter offen.

0.14.0 implementiert [generisches Bauen](docs/generic-building.md): vollständiger
Gebäudekatalog, Vorprüfung, Spielvalidierung und reguläre Einzelaufträge nach Vorlagen-ID.
Mehrere Aufträge je Sitzung mit festen Aktions-IDs; Sonderlayouts ausdrücklich begrenzt.
0.14.0 ist live bestätigt: vier verschiedene Gebäude regulär beauftragt und separat
nachgelesen; Material-/Baufortschritt beobachtet. 0.14.1 korrigiert den zu engen
Layoutfilter für einzelne Wege und kleine Lager; mit 0.15.0 installiert und live bestätigt.

Endziel: Der Agent spielt Timberborn über MCP und baut Wasser-, Nahrungs-, Holzversorgung,
Wege und Wohnraum auf. Agent Bridge 0.2.0 ist über drei MCP-Werkzeuge live geprüft;
Bevölkerung, Beispielbestände und Wohnraum wurden vom Nutzer bestätigt, Wiederverbindung
nach Neuladen erfolgreich. Die Erweiterung 0.3.0 ergänzt Gebäude-/Wegegeometrie,
Pilotkatalog und [rein lesende Bauplatzvorprüfung](docs/spatial-precheck.md); räumlicher MCP-Pilot bestanden.
0.4.0 ergänzt einen [geschützten Spielvalidator-Prototyp](docs/native-validation.md),
dessen erster Livekontrolltest einen belegten Standort fälschlich akzeptierte.
0.4.1 ergänzt die direkte BlockObject-Prüfung und ist installiert/live geprüft:
belegter Lodge-Standort abgewiesen, freier Path-Standort akzeptiert, Zustandswachen
unverändert. Keine Baufreigabe aus 0.4.0 ableiten.
0.5.0 implementiert einen separat geschützten [einzelnen Wegauftrag](docs/path-placement.md),
installiert und begrenzt live geprüft: genau ein regulärer Weg gebaut und über seine
Entity-ID separat als fertig nachgelesen. Kein allgemeiner Haus-/Versorgungsbau.
0.6.0 ergänzt [Baustellen- und Distriktbeobachtung](docs/building-observations.md),
an fertigen Gebäuden und einem nutzerplatzierten Farmhaus live gelesen. Baustellenstatus
und Baudistrikt bestätigt; die gemeldete Restmaterialmenge ist noch nicht verlässlich
interpretiert. Paths haben keine Distriktkomponente.
0.6.1 ersetzt diese missverständliche Angabe durch getrennte Vorlagen-Baukosten und
Baustellenbestände, ohne Restbedarfsberechnung. Am Farmhaus live bestätigt:
25 Holz Baukosten; anschließend Lieferung 2 -> 4 Holz, Materialfortschritt 8 -> 16 %
und Bauzeitfortschritt 0 -> rund 11 % beobachtet. Farmhaus noch nicht fertig.
0.7.0 erweitert den Baupilot um einen separat freizuschaltenden
[Lodge-Auftrag](docs/lodge-placement.md). Installiert und begrenzt live bestätigt: genau eine Lodge regulär beauftragt und
über dieselbe Entity-ID als aktive Baustelle mit bekanntem Baudistrikt nachgelesen.
Fertigstellung und zusätzliche Betten noch offen.
0.8.0 ergänzt [Zeitbeobachtung und Pause/1×](docs/simulation-control.md).
Live bestätigt: Pause hält Spielzeit an, MCP bleibt erreichbar, Weiterlauf auf 1× funktioniert.
0.9.0 ergänzt [Gebäudepause und Arbeitsplatzdiagnose](docs/building-operations.md)
in inspect_building sowie die [kolonieweite Arbeitskräfteliste](docs/workforce-roster.md)
inspect_workforce. Live bestätigt: 10 Arbeiter, 3 beschäftigt, 7 unbeschäftigt;
Arbeitsplatzzuordnungen stimmen mit Gebäudebesetzung überein.
0.10.0 bereitet [reguläre Sollbesetzung](docs/workplace-staffing.md) vor;
gebaut/getestet und in 0.11.0 live bestätigt: Soll- und Istbesetzung 2 → 3 → 2. Spätere Integration:
[Agentenfrage als Texteingabe-Popup](docs/player-question-popup.md).
0.11.0 unterstützt Pause und alle drei regulären Geschwindigkeiten (0/1/3/7).
Alle Stufen live bestätigt; Simulation abschließend 1×.
[Installation und Abnahme](docs/native-bridge-install.md),
[offizielle Quellen und good references](docs/references/README.md).
Für die eigene Bridge: `scripts/start-native.ps1 -ConfigPath '<private Konfiguration>'`;
Build und nativen Lesetest mit `scripts/verify.ps1 -NativeConfig '<private Konfiguration>'`
ausführen. Beide Abläufe sind in der Installationsanleitung beschrieben.
362 reguläre Tests bestanden; drei separate Live-Tests im Standardlauf übersprungen.
Nativer lesender MCP-Livetest zuletzt mit 0.6.1 erfolgreich.
Auch mit ausschließlich eigener Bridge laut Nutzer-Mod-Auswahl: alle sechs nativen
Lesewerkzeuge erneut live erfolgreich (0.3.0). Keine Fremdmod für diesen Zugriff erforderlich.

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
| TIMBERBORN_ENABLE_PLACEMENT | Nur `1` bietet place_path an; enablePlacement in der Mod ebenfalls erforderlich, ein Versuch je Sitzung |
| TIMBERBORN_BASE_URL | http://localhost:8080/; nur HTTP-Loopback, kein Pfad/Query/Login |
| TIMBERBORN_AUTHORIZATION | Optionaler Authorization-Wert; nicht als Argument oder Git-Datei speichern |
| TIMBERBORN_FAKE_SCENARIO | healthy; alternativ partial oder offline; nur für Simulation relevant |
| TIMBERBORN_ENABLE_WRITES | Nur `1` aktiviert zusätzlich set_building_paused; standardmäßig aus |

Beim bisherigen Backend bleiben fünf Tools offline auflistbar: `timberborn_status`, `inspect_colony`,
`inspect_population`, `find_buildings`, `inspect_building`. Parameter und Ergebnisfelder
stehen in missionsplan.md Abschnitt 3. Simulierte Daten sind immer markiert.

Das native Backend bietet sieben eigene lesende Werkzeuge: `timberborn_status`,
`inspect_colony`, `inspect_map_region`, `find_buildings`, `inspect_build_catalog`
`precheck_build_site` und `inspect_building(id, session)` (ab Mod 0.6.0).
Die Vorprüfung ist keine vollständige Bauvalidierung.
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
Objektpositionen und einen begrenzten Gelände-/Wasserausschnitt. Ein einzelner Wegauftrag
ist als Pilot implementiert; allgemeiner Gebäudebau, Save/Load und Automationsgraphen fehlen.

Kein Client darf aus `readOnlyHint` alleine Sicherheit ableiten: Der Adapter selbst begrenzt die Routen.
Fremdmod-Routen können auch bei GET Änderungen ausführen; deshalb gibt es kein generisches HTTP-Tool.

0.12.0 ergänzt [Prioritäten, Baustellen und Flächen](docs/priorities-construction-areas.md):
13 native Lesewerkzeuge; Arbeitsplatz-/Baupriorität und Flächenmarkierungen separat
freischaltbar. Implementierung vorbereitet, Live-Abnahme offen. Zapfflächen ausdrücklich
nicht als eigenständige Markierungsart unterstützt.

0.13.0 ergänzt [Kiefernschutz und getrennte Entfernung](docs/removal-and-pine-protection.md).
tapping entfernt Fällmarkierungen; Gebäude, Pflanzen auf Pflanzmarkierung, übrige
Vegetation und Schutt besitzen getrennte Werkzeuge. Hindernisse vor Bauvorhaben sind
bereits prüfbar; eine Objektabfrage ergänzt jetzt die konkreten Ziel-IDs.
14 native Lesewerkzeuge live bestätigt. Arbeitsplatz- und Baupriorität erfolgreich geändert und wiederhergestellt; Flächenmarkierungen einschließlich Kiefernschutz ebenfalls live geprüft; Abriss-/Wiederaufbaupiloten bestanden. 0.13.1 korrigiert die Antwort bei sofort abgeschlossener Vegetationsentfernung; Patch installiert und live bestätigt: wartende sowie sofort erledigte Entfernung korrekt gemeldet.

0.13.2 ergänzt [Lebenszustände natürlicher Ressourcen](docs/vegetation-state.md) und
korrigiert den Zapfkandidatenfilter: tote, sterbende, junge oder unbekannte Kiefern
werden ausgeschlossen. Installiert und rein lesend live geprüft: lebende/tote Kiefern unterschieden, 19 geeignete Zapfkandidaten bestätigt.
