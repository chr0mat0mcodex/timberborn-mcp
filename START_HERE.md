# Einstieg

## Maßgebliche Arbeitsweise

Das Spiel ist ein austauschbares Testsystem. Entwickle und prüfe MCP-Fähigkeiten
zügig mit strukturierten Aufrufen; keine konkrete Kolonie, kein Save und kein
historischer Bauplan ist Voraussetzung.

## Technischer Stand

- Agent Bridge 0.31.3 installiert und begrenzte Nachbar-/Pfad-/Spill-Diagnose live geprüft;
  vorherige Auswahlabfrage, Plattformpilot und obere Anschlüsse bleiben vorhanden.
- Beide ursprünglichen Bauziele sind nur teilweise erreicht. Allgemeine sichere
  Baufreigabe fehlt; Baustellenabdeckung ist weiterhin offen.
- Etappe A bestanden; in B fachlich gültige Baustellen-Blockadekontrolle finden.
  Details und Grenzen in docs/construction-access-preview.md, keine längeren Wege.
  Zurückgestellter Vier-Wege-Entwurf ist separat lokal gesichert, siehe BACKLOG.md.
- Aktueller Arbeitsbranch: `codex/road-protection-pilot`.
- Nächster Schritt: eine Fähigkeit umsetzen und am menschlichen Test-Gate anhalten:
  `scripts/prepare-human-live-test.ps1` übergeben, auf `live bereit` warten und
  erst nach bestandenem Abschluss-Livetest committen.

## Orientierung

[Projektstand](PROJECT_STATE.md) · [Mission](missionsplan.md) · [Backlog](BACKLOG.md) ·
[Build, Tests und Installation](DEVELOPMENT_WORKFLOW.md) ·
[technische Übergabe](docs/session-handoff.md).

Historische Teststände und konkrete Spielweltdaten wurden bewusst entfernt.
