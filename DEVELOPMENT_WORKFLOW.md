# Entwicklungsablauf

## Ziel

Die teure Test-, Build-, Paketierungs- und Installationskette wird pro Feature
einmal vom Menschen ausgelöst. Der Agent entwickelt und bereitet den gezielten
Livetest vor; er führt keine Vollkette und keine Mod-Installation selbst aus.

## Ablauf pro Feature

Bauentwicklung folgt missionsplan.md (A–E). Bericht/Diagnose erweitert keine
Baufreigabe. Vor dem Gate aktive Änderungen von zurückgestellten Entwürfen trennen;
ungeprüfte Erweiterungen nicht beiläufig mitdeployen. Für MCP-only-Auswertungen
kann die Spiel-Bridge unverändert bleiben; trotzdem menschliches Test-Gate und
gezielter MCP-Livetest. Erfolgsnachweis nennt fachliche Aussage und deren Grenze,
nicht nur Werkzeugaufruf oder erfolgreiche Paketierung.

1. **Agent: Umsetzung und Vorprüfung.** Klar abgegrenzte Änderung implementieren,
   Diff prüfen, betroffene Projekte kompilieren und relevante Tests gezielt
   ausführen. Bei Mod-Code gegen die lokalen Spielreferenzen kompilieren.
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
- Gezielte Projekt-Builds und betroffene automatische Tests gehören zur
  Agent-Vorprüfung. Kein automatischer Start der vollständigen `verify.ps1`-,
  Paketierungs- oder Deploy-Kette; keine Installation oder Prozessbeendigung
  durch die Vorprüfung.
- Kein Commit/Push ohne bestätigten erfolgreichen Skriptlauf und erfolgreichen
  Abschluss-Livetest.

## Vorprüfung vor der Übergabe

- Tatsächlich kompilieren: Syntax-/Diffprüfung erkennt keine Typ-, Referenz- oder
  Nullability-Fehler. Vorhandenen Restore nutzen; fehlende Voraussetzungen benennen.
- Bei neuen MCP-Werkzeugen Namen, Aktionsschalter, Read-only-Annotationen und
  Vertragsprüfungen gemeinsam aktualisieren und die betroffenen Testvarianten ausführen.
- Nach einem gemeldeten Fehler den betroffenen Build/Test gezielt reproduzieren,
  korrigieren und erfolgreich prüfen, bevor der Mensch die Vollkette erneut startet.
  Keine blinden Wiederholungen; bei neuem Fehler Diagnose statt Retry.
- Nur tatsächlich ausgeführte Prüfungen als bestanden nennen. Compiler, Tests,
  Installation und fachlicher Livetest sind getrennte Nachweise. Bei unverändertem
  Code erfolgreiche gezielte Prüfungen nicht grundlos wiederholen.

## Git-Sicherung

GitHub-Ziel: https://github.com/chr0mat0mcodex/timberborn-mcp

Vor jedem zulässigen Commit die Dateiliste und den Diff auf unbeabsichtigte
Änderungen, Geheimnisse sowie private oder generierte Daten prüfen. Nur ausgewählte
Projektdateien stagen. Commit-Identität pro Befehl setzen; keine globale Identität
konfigurieren. Vor dem Push `origin` prüfen, keine fremden Commits überschreiben und
keinen Force-Push verwenden. Erfolgreichen Push gegen lokalen HEAD abgleichen.
