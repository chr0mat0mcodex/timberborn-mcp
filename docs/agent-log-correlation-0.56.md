# Projekt-/Laufkorrelation im Agentenlog (0.56)

2026-10-16: Nach menschlicher Gate-Freigabe durch `live bereit` Bridge 0.56.0
und aktuelle Session bestätigt. MCP-Liveproben bestanden; der Mensch bestätigt
Laufkennung und beide Herkunftslabels im Ingame-Fenster als lesbar. Begrenzt abgenommen.

Gate-Rücklauf: Vorbereitung zunächst wegen laufendem Spiel korrekt abgebrochen.
Danach WinError 5 im Vorschaltprozess-Test beim erneuten Statusschreiben innerhalb
von `close()`. Schließpfad stoppt nun ohne den unmittelbar vor dem Löschen unnötigen
Registry-Austausch. Fehler beim Backend-Stopp lassen die Registry bestehen;
ein noch laufender Watcher verhindert das Löschen. Zwei Regressionen ergänzt.
Ursache der Windows-Zugriffsverweigerung nicht bewiesen; keine allgemeine
Dateisperrenkorrektur behauptet. Anschließend menschliche Bereitmeldung und
erfolgreicher Backendstart mit installierter Bridge 0.56.0.

## MCP-Livebelege

- Ein begrenzter Lauf über genau eine Spielstunde endete nach rund 2,925 realen
  Sekunden mit `completed`, `target_reached` und bestätigter Pause, ohne Überschuss.
- Start und Status desselben Laufs trugen jeweils `run` mit Herkunft request und
  result. Nach Lauf-ID gefilterte Einzelseiten lieferten Sequenz 4, dann 3,
  ohne Duplikat; Logabfragen erzeugten keine eigenen Laufbezüge.
- Lesende Abfrage einer synthetischen nicht vorhandenen Projektkennung:
  `state_conflict`; Logeintrag rejected mit ausschließlich project/request.
- Kombination dieser unabhängigen Projekt- und Laufkennung: leere Seite.
- Cursor mit geänderter Laufkennung: `invalid_argument`, keine Ausführung.
- Fenster angehängt und geöffnet; Mensch bestätigt Laufkennung und beide
  Herkunftslabels als lesbar.
- Reparaturen und zusammengesetzte Berichte bleiben synthetische Gate-Nachweise.

## Vertrag

Jeder neue Logeintrag enthält `correlations`, höchstens acht Bezüge mit:

- `kind`: `project`, `parent_project` oder `run`.
- `id`: kanonische, nicht leere UUID.
- `source`: `request` (angefragt) oder `result` (im Ergebnis gemeldet).

Ein angefragter Bezug bleibt auch bei Ablehnung erhalten. Er beweist weder
Existenz noch Ausführung. Ein Ergebnisbezug belegt nur, dass der Fachbericht die
Kennung geliefert hat; er beweist keine Fertigstellung oder Spielwirkung.
Die MCP-Aufrufkennung `items[].id` bleibt davon getrennt.

Beim Start werden gültige eindeutige Anfragekennungen aus bekannten Feldern
erfasst. Beim Abschluss kommen Ergebnisbezüge hinzu; Startsequenz, Begründung
und Anfragebezüge bleiben erhalten. Doppelte identische Bezüge werden vereinigt,
unterschiedliche Herkunft bleibt sichtbar. Fehlende Abschlussübertragung führt
zu keinem Replay und erfindet keine Ergebnisbezüge. Nach Löschen/Verdrängung
kehren alte Einträge durch Abschlussmeldungen nicht zurück.

Die Transportmetadaten liegen begrenzt im authentisierten Telemetrie-Header,
nicht in frei auswertbarem Begründungstext oder einer rekursiven Suche durch
Antworten. Keine neuen Spielzugriffe für die Zuordnung und keine Speicherung
außerhalb des vorhandenen Sitzungspuffers. Neue Antworten melden
`correlationVersion=1`. Alte Bridges ohne Metadaten bleiben beim bisherigen
Abruf möglich; neue Filter benötigen die aktualisierte Bridge.

## Unterstützte Zuordnung

| Werkzeugfamilie | Anfrage | Ergebnis |
| --- | --- | --- |
| Große Bauprojekte ohne `inspect_power_network` | `actionId` als Projekt; Reparaturen zusätzlich `parentActionId` | `actionId`, `parentActionId`, `runId`, `simulation.runId`, soweit vorhanden |
| Ebenes Bauprojekt: execute/inspect; advance/inspect completion; develop | `actionId` | Direkte Projekt-/Laufkennung; beim Develop auch `start.actionId` und `completion.actionId/runId` |
| Simulationslauf: Start, Status, Abbruch | `runId` | `runId` |
| Gebäudekonfiguration | Kein aus `jobId` abgeleiteter Lauf | ausdrücklich gemeldete `runId` |
| Räumgruppe | Keine aus `jobId` abgeleiteten Bezüge | `clearanceRun.runId`, `construction.actionId`, `construction.simulation.runId` |

Vorab reservierte `runId/actionId` am Wurzelobjekt einer Räumgruppe sind bewusst
keine Ergebnisbezüge. Logfilter, Gebäude-IDs, beliebige `actionId` anderer
Werkzeuge und unbekannte Werkzeuge erzeugen keine Zuordnung. Keine rückwirkende
Anreicherung alter Einträge, kein vollständiger interner Aufrufbaum. Legacy-
Bauchargen, Pflanzenjobs und weitere Auftragsarten sind nicht automatisch erfasst.

## Filter und Ingame-Ansicht

`inspect_agent_log` erhält optionale UUID-Filter `projectId` und `runId`.
`projectId` trifft Projekt- und Elternprojektbezüge. Alle gesetzten Filter
(Werkzeug, Status, Projekt, Lauf) gelten mit UND und vor der Seitengröße.
Cursor sind auch an diese Filter gebunden. Bestehende Cursor ohne ID-Filter
behalten ihr Format; ID-Filter erweitern den Cursor, maximal 260 Zeichen.
Korrelationsfilter ändern die [0.55-Lückenbedeutung](agent-log-pagination-0.55.md)
nicht: Verdrängungszahlen gelten vor allen Filtern, verlorene Treffer unbekannt.
Auch Bezüge sind beim Lesen aktuell, kein eingefrorener Snapshot. Später ergänzte
Ergebnisbezüge hinter einer bereits gelesenen Grenze erfordern eine frische Abfrage.

Das bestehende Fenster **MCP-Aufrufe** ist bereits das Ingame-Agentenlog.
Über **MCP-Log** unten rechts öffnen. Unter dem Status stehen nun vollständige
Kennungen mit „Projekt“, „Elternprojekt“ oder „Lauf“ und der Herkunft „angefragt“
bzw. „im Ergebnis gemeldet“. Diese Anzeige verwendet denselben Puffer wie das MCP.
Die vorhandenen UI-Filter und das Leeren bleiben bestehen; neue ID-Filter sind
MCP-Parameter, kein neues UI-Suchfeld.

## Vorbereitete Tests und Abnahme

Synthetische Tests: bekannte/irrelevante Felder, doppelte oder ungültige IDs,
Elternprojekt, verschachtelte Ergebnisbezüge, keine reservierten Räumgruppen-IDs,
Zusammenführung ohne Sequenzwechsel, UND-Filter, filtergebundener Cursor,
fehlende/fehlerhafte Metadaten, Löschung, fehlender Abschluss, Headertransport,
abgewiesener gesperrter Lauf ohne Spielauftrag und lesbare UI-Herkunftslabels.
Bestehende Paginierungstests bleiben aktiv. Versionsregressionen behalten die
Fähigkeits- und Pflanzenjournalprüfungen für 0.56 bei; 99.0.0 bleibt unbekannt.

Nach menschlichem Gate und `live bereit`:

1. Backend starten, Bridge 0.56 und aktuelle Session bestätigen.
2. Einen kurzen begrenzten Simulationslauf starten, Status lesen und den Log
   nach dieser `runId` filtern. Start und Status müssen denselben Lauf nennen;
   request/result müssen nachvollziehbar bleiben. Abschluss/Pause bestätigen.
3. Ein vorhandenes Projekt lesend prüfen; falls keines verfügbar ist, eine
   synthetische nicht vorhandene Projekt-ID abfragen. Deren Ablehnung muss als
   angefragter Projektbezug auffindbar sein, ohne Ergebnisbezug zu erfinden.
4. Kombination dieser Projekt-ID mit der unabhängigen Lauf-ID ergibt null Treffer.
   Cursor mit geändertem ID-Filter wird abgewiesen; keinen Liveüberlauf erzeugen.
5. Mensch prüft im Fenster MCP-Aufrufe dieselben Kennungen und Herkunftslabels.

Erfolg: korrekte Zuordnung/Filter ohne zusätzliche Spielaufträge, sichtbare
Ingame-Zuordnung. Beim ersten falschen Kontrollfall stoppen und diagnostizieren.
Reparaturen und zusammengesetzte Berichte sind im Gate synthetisch geprüft;
dieser kleine Pilot beansprucht keinen neuen Livebeleg für jede Werkzeugfamilie.
Keine automatische Wiederholung der Vollkette. Commit/Push erst nach Gate und
bestandener gezielter Liveabnahme gemäß [Workflow](../DEVELOPMENT_WORKFLOW.md).
