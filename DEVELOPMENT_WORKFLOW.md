# Entwicklungsablauf

## Ziel

Die teure Test-, Build-, Paketierungs- und Installationskette wird pro Feature
einmal vom Menschen ausgelöst. Der Agent entwickelt und bereitet den gezielten
Livetest vor; er führt keine Vollkette und keine Mod-Installation selbst aus.

## Ablauf pro Feature

1. **Agent: Umsetzung.** Klar abgegrenzte Änderung implementieren und nur
   kostengünstige Quellcode-/Diff-Prüfungen durchführen.
2. **Agent: Testübergabe.** Erwarteten fachlichen Nachweis, bekannte Grenzen und
   den Aufruf des menschlichen Skripts nennen. Noch nicht committen.
3. **Mensch: Test und Deploy.** Bei beendetem Spiel
   `scripts/prepare-human-live-test.ps1 -TimberbornManagedDir <Managed-Verzeichnis>`
   ausführen. Das Skript führt die automatischen Tests aus, baut und paketiert die
   Bridge, sichert die bisherige Installation, erhält `bridge.local.json`,
   installiert die neue Mod und verifiziert die Paketdateien.
4. **Mensch: Spiel bereitstellen.** Timberborn starten, einen beliebigen
   passenden Spielstand laden und dem Agenten `live bereit` melden.
5. **Agent: Abschluss-Livetest.** Genau den angekündigten MCP-Nachweis ausführen,
   Ergebnis und relevante Grenze berichten. Bei Fehlschlag gezielt diagnostizieren;
   keine automatische Vollketten-Wiederholung.
6. **Agent: Abschluss.** Nur wenn Skript und feature-spezifischer Livetest
   erfolgreich waren: Dokumentation knapp aktualisieren, committen und zu `origin`
   pushen.

## Skriptoptionen

`-ModsRoot <Pfad>` setzt bei Bedarf einen abweichenden Timberborn-Mod-Ordner.
`-Port <Port>` erzeugt für eine Erstinstallation eine passende lokale
Bridge-Konfiguration. Bestehende `bridge.local.json` wird unverändert übernommen
und niemals ausgegeben oder versioniert.

## Grenzen

- Das Skript wird vom Menschen lokal gestartet, weil es die Mod-Installation
  verändert. Es bricht ab, wenn ein Timberborn-Prozess läuft.
- Der Agent darf kleine statische Prüfungen zur Implementierung ausführen, jedoch
  keine vollständige `verify.ps1`-, Build-, Paketierungs- oder Deploy-Runde,
  sofern der Mensch dies nicht ausdrücklich für eine Diagnose anfordert.
- Kein Commit/Push ohne bestätigten erfolgreichen Skriptlauf und erfolgreichen
  Abschluss-Livetest.

## Git-Sicherung

GitHub-Ziel: https://github.com/chr0mat0mcodex/timberborn-mcp

Vor jedem zulässigen Commit die Dateiliste und den Diff auf unbeabsichtigte
Änderungen, Geheimnisse sowie private oder generierte Daten prüfen. Nur ausgewählte
Projektdateien stagen. Commit-Identität pro Befehl setzen; keine globale Identität
konfigurieren. Vor dem Push `origin` prüfen, keine fremden Commits überschreiben und
keinen Force-Push verwenden. Erfolgreichen Push gegen lokalen HEAD abgleichen.
