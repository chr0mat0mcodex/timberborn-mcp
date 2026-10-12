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

1. Der Agent implementiert eine klar abgegrenzte Fähigkeit, ergänzt die passenden
   Tests und führt gezielte Quellcode-/Diff-Prüfungen aus. Er startet keine Builds,
   Testläufe, Paketierung oder Installation. Es gibt genau einen Release-Build
   über das Entwickler-Test-Gate; keine Debug-/Parallelfassung als Ausweichweg.
   Compiler- und Testnachweise entstehen im Gate, nicht durch Syntaxprüfung.
   Details stehen in DEVELOPMENT_WORKFLOW.md.
2. Vor der Übergabe stoppt der Agent den eindeutig diesem Projekt zugeordneten
   Codex-Timberborn-MCP über `timberborn_server_control(action=stop)` oder
   `scripts/stop-codex-mcp.ps1` und bestätigt den Prozesszustand. Der persistente
   Vorschaltprozess bleibt verbunden; nur sein Backend wird beendet und gesperrt.
   Danach keine MCP-Aufrufe bis `live bereit`, damit kein vorzeitiger Neustart erfolgt.
   Dies ist ausdrücklich erlaubtes Lifecycle-Handling, kein Agent-Build/Testlauf.
   Am Test-Gate übergibt der Agent an den menschlichen Entwickler. Dieser führt
   `scripts/prepare-human-live-test.ps1` aus; das Skript testet, baut, paketiert,
   installiert die Mod mit Sicherung und prüft die installierten Paketdateien.
3. Nach erfolgreichem Skriptlauf startet und lädt der Mensch Timberborn und meldet
   dem Agenten `live bereit`. Dann startet der Agent mit
   `timberborn_server_control(action=start)` das Backend durch den verbundenen
   Vorschaltprozess und bestätigt `timberborn_status` samt aktueller Session.
   Ein isolierter stdio-Prozess ist kein Wiederanschluss. Einmalige Codex-Einrichtung
   und Ausfallgrenzen stehen in `docs/codex-mcp-supervisor.md`.
   Erst dann führt der Agent den gezielten MCP-Livetest
   für die jeweilige Fähigkeit durch.
4. Der Agent nennt vor der Übergabe den erwarteten Nachweis und nach dem Livetest
   das konkrete Ergebnis. Bei Fehlern wird diagnostiziert; keine automatische
   Wiederholung der vollständigen Kette.
5. Git-Commit und Push erfolgen ausschließlich nach erfolgreichem Skriptlauf und
   bestandenem feature-spezifischem Livetest. Unfertige oder nur statisch geprüfte
   Änderungen bleiben uncommitted. Ausnahme: reine Dokumentations-/Regelpflege
   ohne Änderungen an Code, Tests, Build, Konfiguration oder Laufzeitverhalten darf
   nach Inhalts-, Diff- und Linkprüfung separat committed und gepusht werden.
   Dafür kein Build, MCP-Stopp oder erneuter Livetest; Freigabepflichten für
   dauerhafte Regeländerungen bleiben bestehen.

## Einstieg

- Standardmäßig auf Deutsch, knapp und technisch nachvollziehbar arbeiten.
- Vor Änderungen Git-Status prüfen und die für den Arbeitsschritt relevanten Abschnitte von README.md, PROJECT_STATE.md, DEVELOPMENT_WORKFLOW.md und missionsplan.md lesen. Offene Arbeiten stehen in BACKLOG.md; keine vollständige Historie ohne konkreten Bedarf laden.
- Architekturentscheidungen stehen in docs/architecture/decisions.md, getestete Versionen in docs/compatibility/timberborn.md.
- Historische Planungsabschnitte sind keine aktuelle Zustandsbeschreibung. Aussagen gegen Dateien und Git prüfen.

## Dauerhafte Entwicklungsprinzipien

- Aktueller Release- und Abnahmestand: [PROJECT_STATE.md](PROJECT_STATE.md).
  Offene Aufgaben und zurückgestellte Experimente: [BACKLOG.md](BACKLOG.md).
  [missionsplan.md](missionsplan.md) dokumentiert den historischen Abschluss
  von A–E und dessen Nachweisgrenzen, keine erneut abzuarbeitende Reihenfolge.
- Für jedes Baufeature Geometrie, Distriktanschluss, Bauarbeiterzugang und
  fertigen Gebäudezugang getrennt bewerten. Bestehende Wege und Zugänge sowie
  Auswirkungen während der Bauphasen berücksichtigen. Mehr Werkzeuge oder
  größere Limits ersetzen diese Nachweise nicht.
- unknown ist weder sicher noch unerreichbar. Pflichtprüfungen mit unbekanntem
  Ergebnis blockieren reguläre Ausführung; ausdrücklich begrenzte Entwicklungspiloten
  sind kein allgemeiner Sicherheitsnachweis. pilotEligible ist keine allgemeine Freigabe.
- Auftrag, Fertigstellung, Zugang, Lieferung, Besetzung und Betrieb getrennt
  belegen. Kraftgeometrie, tatsächliche Netzverbindung und Produktion sind
  ebenfalls unterschiedliche Nachweise.
- Aktuelle Werkzeuggrenzen, Ausführungsmodi und Abnahmeumfänge aus Schema,
  Fähigkeitsbericht und Fachvertrag lesen. Historische Einzelschrittregeln nicht
  auf autonome Chargen übertragen; keine pauschale Freigabe aus Versionsnummern.
- Zurückgestellte Entwürfe oder lokale Experimente nicht beiläufig ausliefern
  oder als abgenommen darstellen. Vor Wiederaufnahme Nutzen und aktuellen Stand prüfen.

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
- Feature-Code erst nach erfolgreichem menschlichem Skriptlauf und bestandenem Abschluss-Livetest gezielt committen und zu origin pushen. Reine Dokumentations-/Regelpflege darf nach der oben genannten Prüfung separat gesichert werden; Ablauf und Grenzen stehen in DEVELOPMENT_WORKFLOW.md.
- Diese Regel gilt während der Projektarbeit. Sie richtet keinen Hintergrunddienst oder Zeitplan ein.
