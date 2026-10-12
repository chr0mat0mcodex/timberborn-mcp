# Einstieg

## Maßgebliche Arbeitsweise

Das Spiel ist ein austauschbares Testsystem. Maßgeblich sind [AGENTS.md](AGENTS.md)
und der unveränderte [Entwicklungsablauf](DEVELOPMENT_WORKFLOW.md): Agent entwickelt
und prüft Quelltext/Diff; der Mensch testet, baut und installiert; danach gezielter
MCP-Livetest. Keine historische Kolonie oder konkreter Spielstand erforderlich.

## Technischer Stand

Letzter dokumentierter Abschluss: **0.50.0**, begrenzt live abgenommen und als
`fdbc42a` gepusht. Gruppenplanung bis acht Gebäude, gemeinsame Wege/Flächendaten,
native Belegungsfilter und Compact/Full. Grenzen: 32 Teile, 32×32, acht belegte
Höhen; keine globale Standortoptimierung oder pauschale Katalogabnahme.
[Nachweis](docs/efficient-group-planning.md).

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
