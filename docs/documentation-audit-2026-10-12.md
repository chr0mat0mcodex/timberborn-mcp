# Dokumentationsprüfung 2026-10-12

## Umfang und Ergebnis

Quell-/Dokumentationsabgleich nach Release 0.50.0 (`fdbc42a`): Einstieg, README,
Projektstand, Backlog, Workflow, Missionsplan, Agentenanleitung, Werkzeuge,
Installations-/Übergabe-/Kompatibilitätsdoku und ausgewählte Implementierungen.
Keine Builds, Testläufe, Installation oder Spielaktionen. Historische Fachbelege
wurden nicht nachträglich zu neuen Live-Nachweisen umgedeutet.

## Eindeutig korrigiert

- BACKLOG.md: aktive Aufgaben von historischen Beobachtungen getrennt; Original
  vollständig in BACKLOG_HISTORY.md erhalten. Erledigte Rezept-, Journal-, Wald-,
  Produktions-, Chargen- und 0.49-Folgeaufgaben nicht erneut als offen geführt.
- START_HERE.md: behaupteter aktueller Stand 0.35.1 durch dokumentierten 0.50-Abschluss
  ersetzt. Keine Aussage über eine jetzt neu beobachtete laufende Installation.
- README, Mission, Übergabe und Projektstand: historische Zwischenstände als solche
  eingeordnet. „B/Gate offen“ in alten Abschnitten hebt spätere Abnahmen nicht auf.
- Installationsdoku 0.26.0 und Agentenanleitung 0.36.0 berichtigt. Einstieg verweist
  auf das menschliche Gesamtskript statt auf getrennte Buildbefehle.
- Agentenanleitung: 64-Zellen-Angabe auf 512 native Blockeinträge korrigiert;
  8×8/vier neue Bodenwege ausdrücklich dem ebenen Einzelprojektmodus zugeordnet.
  Gruppenmodus bleibt davon getrennt bei acht Gebäuden/32 Teilen/32×32/acht Höhen.
- Kompatibilitätsindex um aktuelle Einordnung ergänzt; neue Teilabnahmen sind keine
  neue allgemeine Fraktions-/Spielversionsabnahme.

## Geklärt durch Nutzerentscheidung vom 2026-10-12

1. **Hermes-/Startskript-/Budgetexperimente:** Spielerei, zurückgestellt für spätere
   Tests anderer Agenten mit dem MCP. Kein nächster Releaseumfang. Lokale Dateien
   bleiben erhalten; experimentelle Implementierungen und deren unbestätigte
   Dokumentationsänderungen werden nicht in diesen Commit aufgenommen.
2. **Veralteter Missionsschwerpunkt in AGENTS.md:** ausdrücklich zur Korrektur
   freigegeben. Durch dauerhafte Nachweisprinzipien und Verweise auf PROJECT_STATE
   und BACKLOG ersetzt; A–E ist historische Grundlage, keine neue Arbeitsreihenfolge.
3. **Reine Dokumentationspflege:** Commit/Push nach Inhalts-, Diff- und Linkprüfung
   ausdrücklich erlaubt. Ausnahme in AGENTS.md und DEVELOPMENT_WORKFLOW.md
   festgehalten. Für Code, Tests, Build, Konfiguration und Laufzeitverhalten bleibt
   das menschliche Test-Gate unverändert.

Keine dieser drei Klärungen ist mehr offen.

## Nicht als Widerspruch behandeln

- „known“ im Fähigkeitsprofil ist kein allgemeiner Live-Nachweis.
- 0.50-Gruppenplanung und alte Einzelprojektmodi haben unterschiedliche Limits.
- Server-Wiederanschluss in derselben Session ist vorhanden; Wiederaufnahme nach
  Neuladen des Spielstands bleibt ein eigener offener Umfang.
- Spieländerungsjournal (0.40) und Agentenaufruflog sind verschiedene Funktionen;
  Journal-Cursor erledigen die Cursor-Lücke von inspect_agent_log nicht.
- Große/Hex-Geometrie kann vorbereitet sein, obwohl Sonder-Livetests zurückgestellt sind.

## Prüfung der Bereinigung

- Historischer Backlog vollständig gegen HEAD verglichen: Inhalt unverändert erhalten.
- Relative Dateilinks der 14 bearbeiteten/neu angelegten Dokumente geprüft;
  fehlenden alten activity-log.md-Verweis durch den Werkzeugkatalog ersetzt.
- UTF-8-Lesbarkeit und git diff --check geprüft; keine Whitespacefehler.
- AGENTS.md nach ausdrücklichem Go aktualisiert. Quellcode und Skripte bleiben
  durch diese Bereinigung unverändert; vorhandene lokale Experimente erhalten.
  Kein Build/Testlauf. Dokumentations-Commit/Push ausdrücklich beauftragt.
