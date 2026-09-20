# Agentenanweisungen

## Einstieg

- Standardmäßig auf Deutsch, knapp und technisch nachvollziehbar arbeiten.
- Vor Änderungen Git-Status, README.md, PROJECT_STATE.md, DEVELOPMENT_WORKFLOW.md und die relevanten Abschnitte von missionsplan.md lesen. Offene Arbeiten stehen in BACKLOG.md.
- Aktuelle Ergebnisse stehen in docs/project-journal.md, Architekturentscheidungen in docs/architecture/decisions.md, getestete Versionen in docs/compatibility/timberborn.md.
- Historische Planungsabschnitte sind keine aktuelle Zustandsbeschreibung. Aussagen gegen Dateien und Git prüfen.

## Aktueller Missionsschwerpunkt

- Vorrang hat die Analyse, was ein Agent benötigt, um Timberborn über MCP spielen zu können: Zustandsdaten, Entscheidungsgrundlagen, kontrollierte Eingriffe und deren Voraussetzungen.
- Tatsächliches Spielen ist derzeit nachrangig. Prototypen und Live-Tests dienen gezielt dem Nachweis einer konkreten Fähigkeit oder der Klärung einer Lücke; keinen autonomen Kolonieaufbau als Standard-Fortsetzung betreiben.
- Für jede benötigte Fähigkeit festhalten: Zweck, erforderliche Daten/Aktionen, API-Zugang, Belegstufe (Idee, öffentliche Signatur, gebaut/getestet, live bestätigt), Grenzen und nächster sinnvoller Nachweis. Vorprüfung, Spielvalidierung, Auftrag und Wirkung getrennt bewerten.
- Zustände strukturiert aus dem Spiel abfragen und Interaktionen kontrolliert programmieren. Keine Screenshot-Auswertung oder simulierten Maus-/Tastatureingriffe als Spielsteuerung.
- Im freigegebenen Umfang selbstständig weiterarbeiten, bis tatsächliche Nutzerhilfe nötig ist. Abhängigkeiten möglichst vermeiden; sinnvolle Abhängigkeiten mit Nutzen/Aufwand/Risiko gegenüber Eigenbau abwägen und vor Aufnahme fragen.

## Eingriffsgrenzen

- Aktuelle Architektur: eigene Agent Bridge und natives MCP-Backend. More HTTP API ist ein vorhandener Legacy-Adapter, keine Laufzeitabhängigkeit der eigenen Mod. Bereits freigegebene Entwicklung und gezielte Spieltests dürfen im bestehenden Umfang fortgesetzt werden; neue Bereiche benötigen Freigabe. Keine Save-Manipulation oder generischen HTTP-Werkzeuge.
- Installation/Updates der eigenen Mod und bestehende lokale Freigaben sind im beauftragten Entwicklungsablauf autorisiert. Vor Dateiaustausch Spielende prüfen, Mod sichern, Paketdateien verifizieren und private Konfiguration erhalten. Speichern, Beenden und Neustarten übernimmt der Nutzer; Spiel nicht ungefragt beenden. Fremdmods, zusätzliche Abhängigkeiten und darüber hinausgehende System-/Clientänderungen vorher abstimmen.
- Größere Architekturänderungen und neue Funktionen zunächst konkret vorschlagen und auf Freigabe warten.
- Änderungen an dauerhaften Agentenregeln zuerst beschreiben und freigeben lassen; bereits ausdrücklich beauftragte Regeländerungen nicht erneut bestätigen lassen.
- Bestehende fremde Änderungen erhalten. Keine destruktiven Git-Befehle oder Force-Pushes.

## Qualität und Datenschutz

- Dokumentierte Schnittstellen und bestehende Schichten verwenden. Fake-Daten immer als Simulation kennzeichnen; unbekannte Werte nicht erfinden.
- stdout bleibt ausschließlich MCP-Protokoll; Diagnose nach stderr.
- Codeänderungen gemäß DEVELOPMENT_WORKFLOW.md prüfen. Live-Tests nur mit ausdrücklich vorbereiteter Testkolonie und Opt-in.
- Keine Zugangsdaten, Auth-Dateien, Rohlogs, Chats, persönlichen Erinnerungen, Screenshots, Anhänge, SQLite-Zustände, Spielstände, Spiel-/Mod-Binärdateien oder Laufzeitcaches committen.
- Synthetische Testdaten verwenden. Keine persönlichen Namen, lokalen Benutzerpfade oder Spiel-IDs in Fixtures aufnehmen.
- Relevante Ergebnisse und Fallstricke knapp im Projektjournal bzw. passenden Fachdokument festhalten, ohne Rohdaten abzulegen.

## GitHub-Sicherung

- Der Nutzer hat Veröffentlichung und regelmäßige Pushes dieses Projekts nach chr0mat0mcodex/timberborn-mcp ausdrücklich beauftragt.
- Nach sinnvoll abgeschlossenen, geprüften Arbeitsständen gezielt committen und zum eingerichteten origin pushen; Ablauf und Grenzen stehen in DEVELOPMENT_WORKFLOW.md.
- Diese Regel gilt während der Projektarbeit. Sie richtet keinen Hintergrunddienst oder Zeitplan ein.
