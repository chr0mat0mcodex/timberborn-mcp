# Einstieg und Übergabe

Stand der Chat-Übergabe: 2026-10-02. Der Nutzer beendet den bisherigen Chat;
diese Dateien ersetzen die Abhängigkeit von dessen Verlauf. Keine Hintergrundarbeit
oder neuen Spielaktionen beim Erstellen dieser Übergabe gestartet.

## Sofort relevanter Stand

- **0.25.0 ist gebaut, paketiert und installiert.** 648 reguläre Tests bestehen
  (635 Unit, 13 Integration); drei opt-in Live-Tests waren nicht Teil dieses Laufs.
- Fünf installierte Paketdateien erneut abgeglichen; Vorversion 0.24.1 gesichert,
  private Einstellungen und Zusatzdateien beim Update erhalten.
- **0.25.0 ist noch nicht live abgenommen.** Zuletzt war der Nutzer zum Starten,
  Laden von „MCP“ und Pausiertlassen aufgefordert. Ein anschließendes Laden oder
  ein realer Bauauftrag mit 0.25.0 ist bisher nicht bestätigt.
- Arbeitsbranch: `codex/road-protection-pilot`. Implementierungscheckpoint
  `8349b80`, Installationscheckpoint `e92713c`, beide gepusht. Der zuletzt lokal
  geprüfte `origin/master` enthält 0.22.0; nicht mit dem Entwicklungsstand verwechseln.
- Nächste Aufgabe nach Wiederaufnahme: **ein begrenzter realer Baupilot**, kleines
  Lager plus höchstens zwei neue ebene Wege. Diese konkrete Ausnahme wurde bereits
  ausdrücklich freigegeben. Kein allgemeiner Kolonieausbau und keine allgemeine
  Aufhebung des noch unvollständigen Wegschutzes.

## Lesereihenfolge

1. [AGENTS.md](AGENTS.md): bestehende Projektregeln, Eingriffsgrenzen und GitHub-Auftrag.
2. [Übergabe](docs/session-handoff.md): offene Nutzerhilfe, Live-Ablauf,
   lokale Artefakte, technische Stolperstellen und fortgeltende Entscheidungen.
3. [Projektstand](PROJECT_STATE.md), [Mission](missionsplan.md), [Backlog](BACKLOG.md).
4. [Bauprojekt-Vertrag](docs/building-project-execution-proposal.md) und
   [Entwicklungsablauf](DEVELOPMENT_WORKFLOW.md) für die konkrete Fortsetzung.

Git-Zustand, installierte Version und frische Spielsession beim Wiedereinstieg
prüfen. Historische Session-IDs, Entity-IDs und Plan-Kennungen sind keine aktuellen
Eingaben. Vollständige Chronologie und frühere Nachweise:
[Projektjournal](docs/project-journal.md), [Kompatibilität](docs/compatibility/timberborn.md).

## Dauerziele und aktueller Schwerpunkt

Endziel bleibt ein Agent, der Timberborn regulär über MCP spielen kann. Grundversorgung
umfasst Wasser, Nahrung, Holz, Wege **und Wohnraum**. Die beauftragten Spielziele
100 lebende Biber und alle regulären Gebäude bleiben offen. Unmittelbarer Schwerpunkt
ist seit der Rückkehr zur MCP-Entwicklung die zuverlässige Steuerungsbasis.
[Früherer Koloniefortschritt](docs/colony-goals.md) ist keine frische Bestandsaufnahme.
