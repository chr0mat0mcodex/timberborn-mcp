# Einstieg

## Maßgebliche Arbeitsweise

1. **Testsystem:** Der Spielstand ist austauschbar. Kein Schutz oder Erhalt von
   Kolonie, Vorräten, Gebäuden oder Fortschritt; Speichern ist nicht erforderlich.
2. **Schnelle, effiziente Entwicklung:** zusammenhängende Arbeit bündeln, gezielt
   testen und selbstständig fortsetzen. Keine Freigaberunden für normale Ingame-Tests.

Diese Nutzerentscheidung vom 2026-10-02 ersetzt die früheren engen Testfreigaben
und Kolonie-Erhaltungsziele. [Verbindliche Projektanweisungen](AGENTS.md).

## Technischer Stand

- Agent Bridge **0.25.0** gebaut, paketiert und installiert; fünf Dateien geprüft,
  vorherige Mod gesichert, private Konfiguration erhalten.
- 648 reguläre Tests bestanden (635 Unit, 13 Integration). Reale Bauausführung
  der neuen Version noch nicht live bestätigt.
- Arbeitsbranch `codex/road-protection-pilot`; `origin/master` zuletzt auf 0.22.0.
- Nächster Schritt: laufende Version und Session lesen, Bauprojektausführung testen,
  Ergebnis oder Fehler abgleichen und den nächsten sinnvollen Entwicklungsschritt angehen.
  Jeder geeignete geladene Testspielstand genügt; kein alter Koloniezustand nötig.
- Die installierte Implementierung kann bislang ein kleines Lager mit bis zu zwei
  neuen ebenen Wegen je Sitzung. Das ist eine Codegrenze, keine Nutzer-Freigabegrenze.

## Orientierung

[Projektstand](PROJECT_STATE.md) · [Mission](missionsplan.md) · [Backlog](BACKLOG.md) ·
[Build/Test/Installation](DEVELOPMENT_WORKFLOW.md) · [technische Übergabe](docs/session-handoff.md).

Git-Zustand und tatsächlich geladene Bridge prüfen. Versionshistorie und frühere
Belege stehen im [Journal](docs/project-journal.md) und in der
[Kompatibilitätsdoku](docs/compatibility/timberborn.md). Alte Ingame-Freigaben oder
Spielziele daraus nicht als aktuelle Anweisungen übernehmen.
