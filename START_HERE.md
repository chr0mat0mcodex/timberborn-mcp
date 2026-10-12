# Einstieg

## Maßgebliche Arbeitsweise

Das Spiel ist ein austauschbares Testsystem. Maßgeblich sind [AGENTS.md](AGENTS.md)
und der unveränderte [Entwicklungsablauf](DEVELOPMENT_WORKFLOW.md): Agent entwickelt
und prüft Quelltext/Diff; der Mensch testet, baut und installiert; danach gezielter
MCP-Livetest. Keine historische Kolonie oder konkreter Spielstand erforderlich.

## Aktueller Auftrag

0.52 Flächenräumung und gemeinsamer Gruppenbau ist begrenzt live abgenommen.
Nächste Erweiterung nach dem bereinigten Backlog auswählen. [Vertrag](docs/cleared-building-groups-0.52.md).

## Technischer Stand

Letzter dokumentierter Abschluss: **0.52.0**, begrenzt live abgenommen.
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
