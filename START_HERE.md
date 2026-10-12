# Einstieg

## 0.53-Nacharbeiten abgeschlossen

Menschliches Gate und gezielter Livetest am 12. Oktober 2026 bestanden.
Native Ablehnung planting_preview_changed meldet not_executed; anschliessende
frische Vorpruefung bestaetigt unveraenderte Markierungen. Transportunsicherheit
und andere Konflikte bleiben unconfirmed (automatische Gegenproben im Gate).
Compact laesst rein passende Zellbeispiele weg: 1165 statt 2929 JSON-Zeichen,
60,2 Prozent kleiner bei gleichem Schluessel und gleichen Summen. Zusatzblockaden
bleiben sichtbar; Full vollstaendig. Testmarkierung wiederhergestellt, alle acht
Zellen frisch nachgelesen, Endpause bestaetigt. Keine Pflanzen oder Stapel entfernt.



## Maßgebliche Arbeitsweise

Das Spiel ist ein austauschbares Testsystem. Maßgeblich sind [AGENTS.md](AGENTS.md)
und der unveränderte [Entwicklungsablauf](DEVELOPMENT_WORKFLOW.md): Agent entwickelt
und prüft Quelltext/Diff; der Mensch testet, baut und installiert; danach gezielter
MCP-Livetest. Keine historische Kolonie oder konkreter Spielstand erforderlich.

## Aktueller Auftrag

0.53 Sammelpflanzung begrenzt live abgenommen; menschliches Gate bestanden.
Zwei Rechtecke, Ausschluesse und veraltete Vorschau getestet; Testmarkierungen
wiederhergestellt, Spiel pausiert. [Nachweise und Grenzen](docs/planting-batch-0.53.md).
Naechste Erweiterung anhand BACKLOG.md auswaehlen, keinen Kolonieaufbau fortsetzen.

## Technischer Stand

Letzter dokumentierter Abschluss: **0.53.0**, begrenzt live abgenommen.
Gruppenplanung bis acht Gebäude mit gemeinsamer Flächenbeobachtung,
Rückwärtssuche und bis acht Ausschlussvolumen für den konkreten Plan.
Grenzen: 32 Teile, 32×32, acht belegte Höhen; höchstens 32 native Vorschauen und
256 Suchzustände. Keine globale Optimalität oder dauerhaften Reservierungen.
[Nachweis](docs/group-search-0.51.md).

Aktuellen Laufzeitstand nach Wiederanschluss aus `timberborn_status` und Session
lesen. Die Versionsnummer allein beweist nicht den Abnahmestatus lokaler Änderungen.
Arbeitsbranch: `codex/road-protection-pilot`; Git-Status vor Änderungen prüfen.
Lokale Hermes-/Startskript-/Budgetänderungen sind separat vom Releaseabschluss
zurückgestellt (Tests anderer Agenten in ferner Zukunft); [Entscheidung](docs/documentation-audit-2026-10-12.md).

Phase A–E ist historisch im begrenzten Umfang abgeschlossen; die verbliebenen
Nachweislücken und Folgefähigkeiten stehen im [aktuellen Backlog](BACKLOG.md).
Nächstes Paket konkret auswählen, keine alten Kolonieziele automatisch fortsetzen.

## Orientierung

- [Projektstand und datierte Abnahmen](PROJECT_STATE.md)
- [Offene Arbeit](BACKLOG.md) / [historischer Backlog](BACKLOG_HISTORY.md)
- [Werkzeuge](docs/tools.md) / [Agentenanleitung](docs/agent-playing-guide.md)
- [Installation](docs/native-bridge-install.md) / [Codex-Lifecycle](docs/codex-mcp-supervisor.md)
- [Entwicklungsablauf](DEVELOPMENT_WORKFLOW.md) / [historischer Etappenplan](missionsplan.md)
