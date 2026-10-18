# Einstieg

## Technischer Stand

Native Agent Bridge **0.74.0**. MCP-Erweiterungen derselben Bridge-Version
werden durch Commit und feature-spezifische Abnahme unterschieden.
Maßgeblich für aktuellen Featurestand, Gatezahlen und Nachweise:
[PROJECT_STATE.md](PROJECT_STATE.md).

Letzter fachlicher Abschluss: automatische Bretterketten-Vorprüfung vor
relevanten Bau-/Simulationsstarts, begrenzt live abgenommen.
[Vertrag und Grenzen](docs/plank-chain.md#automatische-einbindung-vor-starts).
Die Bridge-Version allein beweist weder aktuellen MCP-Build noch Liveabnahme.
Laufzeitstand und Session werden beim regulären Wiederanschluss frisch gelesen.

## Aktueller Auftrag

**B06-Entwurf fertig; Umsetzung ausdrücklich noch offen.**
[Eine Baucharge nach Reload rein lesend abgleichen](docs/project-reload-reconciliation-design.md).
Nächster gesondert zu beauftragender Schritt: B06.1, öffentliche Save-Anbindung
und Objektidentität belegen. Kein Code geändert, kein Gate oder Livetest gestartet.

**A12/N10-Pilot abgeschlossen; einzelne Quelle-Ziel-Lieferzuordnung bleibt offen.**
[Nachweise](docs/plank-production-transport-pilot.md): kleiner Kraft-/Lageraufbau fertig,
drei native Bretterzyklen, Ausgabeentlastung und Lagerzufluss beobachtet.
Negativkontrolle null Zyklen. Spiel pausiert, kein Code geändert, kein Gate offen.
[Weitere Reihenfolge](BACKLOG.md#empfohlene-nächste-schritte--planung-2026-10-18).

## Maßgebliche Arbeitsweise

Das Spiel ist ein austauschbares Testsystem. Maßgeblich sind [AGENTS.md](AGENTS.md)
und der [Entwicklungsablauf](DEVELOPMENT_WORKFLOW.md): Agent prüft Quelltext/Diff;
Mensch testet, baut und installiert; danach gezielter MCP-Livetest.
Reine Dokumentationspflege nutzt die dort beschriebene Ausnahme.
Git-Status vor Änderungen prüfen; Arbeitsbranch zuletzt `codex/road-protection-pilot`.

## Orientierung

- [Projektstand und datierte Abnahmen](PROJECT_STATE.md)
- [Offene Arbeit](BACKLOG.md) / [historischer Backlog](BACKLOG_HISTORY.md)
- [Werkzeuge](docs/tools.md) / [Agentenanleitung](docs/agent-playing-guide.md)
- [Installation](docs/native-bridge-install.md) / [Codex-Lifecycle](docs/codex-mcp-supervisor.md)
- [Historischer Etappenplan A–E](missionsplan.md): begrenzt abgeschlossen.

Hermes ist seit 2026-10-17 entfernt; historische Erwähnungen sind keine
Wiederaufnahmefreigabe. Historische Releasebelege stehen im Projektstand und Archiv.
