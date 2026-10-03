# Agentenanweisungen

## Kernanweisungen — maßgeblich seit 2026-10-02

1. **Testsystem.** Timberborn und der verwendete Spielstand sind eine austauschbare
   Entwicklungsumgebung. Kolonie, Gebäude, Ressourcen, Bevölkerung und Fortschritt
   haben keinen Erhaltungswert. Kein Spielstandschutz, keine Pflicht zum Speichern
   oder Wiederherstellen des Ausgangszustands und keine Rückfragen nur wegen
   möglicher Ingame-Verluste. Für die MCP-Entwicklung nötige Testbauten, Abrisse,
   Flächenänderungen und Simulationsläufe sind grundsätzlich freigegeben.
2. **Schnelle und effiziente Entwicklung.** Zügig zu funktionierenden, überprüften
   MCP-Fähigkeiten kommen. Zusammenhängende Änderungen bündeln, den kleinsten
   aussagekräftigen Test ausführen und bei klarem Ergebnis weiterarbeiten. Keine
   unnötigen Bestätigungsrunden, Wiederholungsprüfungen, Installationsschleifen oder
   aufwendige Pflege einer Testkolonie. Rückfragen nur bei echter Unklarheit,
   erforderlicher Nutzerhilfe oder Auswirkungen außerhalb des Testsystems.

Diese Anweisung ersetzt ältere spielstandsbezogene Einschränkungen, einzelne
Ingame-Freigaben und fortlaufende Kolonieziele in Planungs- und Übergabedokumenten.
Aktuelle Codegrenzen sind technische Tatsachen, keine dauerhaften Freigabegrenzen.
Ihre sinnvolle Weiterentwicklung gehört zum Entwicklungsauftrag. Datenschutz,
Schutz anderer Projekte/Systemdateien und nachvollziehbare Git-Checkpoints bleiben.

## Entwicklungszyklus: Agent → Mensch → Live-Test

1. Der Agent implementiert eine klar abgegrenzte Fähigkeit und führt nur gezielte,
   günstige Quellcode-/Diff-Prüfungen aus. Er startet weder die vollständige
   Test-/Build-/Deploy-Kette noch installiert er die Mod selbst.
2. Am Test-Gate übergibt der Agent an den Menschen. Der Mensch führt
   `scripts/prepare-human-live-test.ps1` aus; das Skript testet, baut, paketiert,
   installiert die Mod mit Sicherung und prüft die installierten Paketdateien.
3. Nach erfolgreichem Skriptlauf startet und lädt der Mensch Timberborn und meldet
   dem Agenten `live bereit`. Erst dann führt der Agent den gezielten MCP-Livetest
   für die jeweilige Fähigkeit durch.
4. Der Agent nennt vor der Übergabe den erwarteten Nachweis und nach dem Livetest
   das konkrete Ergebnis. Bei Fehlern wird diagnostiziert; keine automatische
   Wiederholung der vollständigen Kette.
5. Git-Commit und Push erfolgen ausschließlich nach erfolgreichem Skriptlauf und
   bestandenem feature-spezifischem Livetest. Unfertige oder nur statisch geprüfte
   Änderungen bleiben uncommitted.

## Einstieg

- Standardmäßig auf Deutsch, knapp und technisch nachvollziehbar arbeiten.
- Vor Änderungen Git-Status prüfen und die für den Arbeitsschritt relevanten Abschnitte von README.md, PROJECT_STATE.md, DEVELOPMENT_WORKFLOW.md und missionsplan.md lesen. Offene Arbeiten stehen in BACKLOG.md; keine vollständige Historie ohne konkreten Bedarf laden.
- Architekturentscheidungen stehen in docs/architecture/decisions.md, getestete Versionen in docs/compatibility/timberborn.md.
- Historische Planungsabschnitte sind keine aktuelle Zustandsbeschreibung. Aussagen gegen Dateien und Git prüfen.

## Aktueller Missionsschwerpunkt

- Maßgeblicher Etappenplan seit 2026-10-03: [missionsplan.md](missionsplan.md).
  Zuerst bestehende Wege/Gebäude-/Baustellenzugänge erhalten und Zugang neuer
  Gebäude während Bau und nach Fertigstellung nachweisen; erst danach Umfang
  (Vorlagen, Weglängen, freie Höhenplanung) erweitern.
- Reihenfolge A–E einhalten: gemeinsamer Bauprüfbericht, Baustellenlücke,
  vollständiger kleiner Gebäudeablauf, vertikaler Gebäudeanschluss, Breitenausbau.
  Mehr Werkzeuge oder längere Wege ersetzen keinen Nachweis der beiden Bauziele.
- Geometrie, Distriktanschluss, Bauarbeiterzugang und fertiger Gebäudezugang
  getrennt bewerten. unknown ist weder sicher noch unerreichbar. Pflichtprüfungen
  mit unbekanntem Ergebnis blockieren reguläre Ausführung; Entwicklungspiloten
  mit ausdrücklich ausgewiesenen Lücken sind kein allgemeiner Sicherheitsnachweis.
- `completed` eines Pilots nicht als fertiges, erreichbar geprüftes Gebäude
  darstellen. Lieferung, Besetzung und Betrieb sind separate Aussagen.
- Ungeprüfter Vier-Wege-Ausbau ist zurückgestellt und lokal separat gesichert.
  Nicht beiläufig in Etappe A deployen oder seine Version als verifiziert nennen.

- Ziel ist eine funktionierende MCP-Steuerung für Timberborn: Zustandsdaten, Entscheidungsgrundlagen und programmierte Eingriffe implementieren und praktisch prüfen. Analyse nur so weit betreiben, wie sie die nächste sinnvolle Umsetzung oder Fehlerklärung unterstützt.
- Tatsächliches Spielen ist derzeit nachrangig. Prototypen und Live-Tests dienen gezielt dem Nachweis einer konkreten Fähigkeit oder der Klärung einer Lücke; keinen autonomen Kolonieaufbau als Standard-Fortsetzung betreiben.
- Ergebnisse knapp festhalten: Funktion, geprüfter Nachweis, relevante Grenze und nächster Schritt. Vorprüfung, Spielvalidierung, Auftrag und Wirkung nicht verwechseln; keine Dokumentation ohne praktischen Nutzen erzeugen.
- Zustände strukturiert aus dem Spiel abfragen und Interaktionen kontrolliert programmieren. Keine Screenshot-Auswertung oder simulierten Maus-/Tastatureingriffe als Spielsteuerung.
- Im freigegebenen Umfang selbstständig weiterarbeiten, bis tatsächliche Nutzerhilfe nötig ist. Abhängigkeiten möglichst vermeiden; sinnvolle Abhängigkeiten mit Nutzen/Aufwand/Risiko gegenüber Eigenbau abwägen und vor Aufnahme fragen.

## Eingriffsgrenzen

- Aktuelle Architektur: eigene Agent Bridge und natives MCP-Backend. More HTTP API ist ein vorhandener Legacy-Adapter, keine Laufzeitabhängigkeit der eigenen Mod. Reguläre Spielservices und strukturierte MCP-Aufrufe bleiben der Funktionsnachweis; Save-Manipulation oder generische HTTP-Werkzeuge sind kein Ersatz für eine funktionierende Schnittstelle.
- Installation/Updates der eigenen Mod erfolgen ausschließlich durch den Menschen über `scripts/prepare-human-live-test.ps1`. Das Skript prüft vor dem Austausch, sichert die vorhandene Mod, erhält die private Konfiguration und verifiziert das Paket. Fremdmods, zusätzliche Abhängigkeiten und darüber hinausgehende System-/Clientänderungen vorher abstimmen.
- Routineimplementierung und technische Tests innerhalb des MCP-Projektziels selbstständig durchführen. Grundlegende Architekturwechsel oder unklare, aufwendige Erweiterungen vorher konkret abklären; nicht jede neue Funktion oder jeder Test benötigt eine eigene Freigaberunde.
- Änderungen an dauerhaften Agentenregeln zuerst beschreiben und freigeben lassen; bereits ausdrücklich beauftragte Regeländerungen nicht erneut bestätigen lassen.
- Bestehende fremde Änderungen erhalten. Keine destruktiven Git-Befehle oder Force-Pushes.

## Qualität und Datenschutz

- Dokumentierte Schnittstellen und bestehende Schichten verwenden. Fake-Daten immer als Simulation kennzeichnen; unbekannte Werte nicht erfinden.
- stdout bleibt ausschließlich MCP-Protokoll; Diagnose nach stderr.
- Den Übergabezyklus in DEVELOPMENT_WORKFLOW.md einhalten. Das lokale Spiel ist das freigegebene Testsystem; kein bestimmter Save-Name oder Koloniezustand erforderlich. Technische Aktionsschalter, Session-Bindung und Rückabfragen dienen korrekten Tests, nicht dem Erhalt der Kolonie. Unbestätigte Aktionen vor einer Wiederholung diagnostizieren statt blind erneut senden.
- Keine Zugangsdaten, Auth-Dateien, Rohlogs, Chats, persönlichen Erinnerungen, Screenshots, Anhänge, SQLite-Zustände, Spielstände, Spiel-/Mod-Binärdateien oder Laufzeitcaches committen.
- Synthetische Testdaten verwenden. Keine persönlichen Namen, lokalen Benutzerpfade oder Spiel-IDs in Fixtures aufnehmen.
- Relevante Ergebnisse und Fallstricke knapp im Projektjournal bzw. passenden Fachdokument festhalten, ohne Rohdaten abzulegen.
- Bauprüfungen mit wenigen gezielten Positiv-/Negativfällen abnehmen, inklusive
  Umweg, gefährdetem Bestandszugang und Höhenunterschied. Erfolg und Stoppkriterium
  vor dem Pilot nennen; bei fehlerhaftem Kontrollfall nicht auf weitere Fälle skalieren.

## GitHub-Sicherung

- Der Nutzer hat Veröffentlichung und regelmäßige Pushes dieses Projekts nach chr0mat0mcodex/timberborn-mcp ausdrücklich beauftragt.
- Erst nach erfolgreichem menschlichem Skriptlauf und bestandenem Abschluss-Livetest eines Features gezielt committen und zum eingerichteten origin pushen; Ablauf und Grenzen stehen in DEVELOPMENT_WORKFLOW.md.
- Diese Regel gilt während der Projektarbeit. Sie richtet keinen Hintergrunddienst oder Zeitplan ein.
