# Günstige Statusabfragen und begrenztes Warten — 0.47.0

Status: menschliches Gate nach Waldprotokoll-Korrektur bestanden und vom Nutzer
ausdrücklich bestätigt; gezielter Livepilot mit Bridge 0.47.0 bestanden (2026-10-12).

## Backend-Cancel und historische erste Liveabnahme

Während dieser beiden Statusabfragen mit `waitSeconds > 0` lässt das MCP-Backend
genau einen zusätzlichen `cancel_simulation_run` zu. Alle anderen konkurrierenden
Aufrufe bleiben abgewiesen; laufende Schreib-/Fortsetzungsaufträge erhalten keine
allgemeine Parallelfreigabe. Parameter, Session, Aktionsschalter und native
Cancel-Prüfungen gelten weiter. Transportverlust bleibt unbestätigt, kein Retry.
Die Statusabfrage läuft weiter und liest den geänderten/terminalen Zustand nach.
Cancel-Antwort allein ersetzt nicht den Nachweis `pauseConfirmed` und Tempo 0.
Gate bestanden; Livepilot am 2026-10-18 nicht bestanden: Cancel antwortete im
verbundenen Codex-Aufrufweg erst nach Ende der 30-Sekunden-Statusabfrage
(31,13 Sekunden ab Cancel-Anforderung). Anschließende Pause bestätigt.
Dieser erste Pilot ist historisch. Der folgende Abschluss lokalisiert die
Serialisierung vor dem Supervisor. Bridge bleibt 0.74.0.

## Codex-Transportgrenze — begrenzt live abgenommen am 2026-10-18

Nach erneuter Gate-Abnahme und vollständigem Codex-Neustart begrenzt der
Supervisor gültige waitSeconds-Werte dieser beiden Leser auf höchstens 1.
Ein bestehender Aufruf mit 30 bleibt gültig, wartet intern aber höchstens eine
Sekunde nach der ersten Lesung. watch.waitedSeconds und reason berichten den
tatsächlichen Ablauf; der Werkzeugtext erklärt die Kürzung. Netzwerk-/Client-
und Diagnosezeit sind zusätzliche Latenz. Direkte Backendclients behalten 0–30.
Keine automatische Folgeabfrage, kein Wiederholen einer Mutation. Die neue
Grenze verhindert lange eigene Wartefenster auch bei serieller Clientzustellung;
sie macht einen extern seriellen Client nicht parallel.

Gate: 28 Supervisor-, 1936 Unit- und 13 Integrationstests bestanden, drei
optionale Livetests übersprungen. Nach Codex-Neustart: 30 angefordert, 1,002 s
intern gewartet; Statusantwort nach 1,175 s. Parallel angeforderter Cancel
antwortete nach 2,986 s, anschließend cancelled und Pause 0 bestätigt.
Cancel kam erst 1,894 s nach der Statusantwort am Supervisor an; von dort bis
zur Antwort nur 116 ms. Serialisierung damit vor dem Supervisor lokalisiert;
keine echte Parallelzustellung oder feste Gesamtlatenz behaupten. Der native
Parallelpfad sowie Projektstatusbegrenzung bleiben hier Gate-Nachweise.

## Öffentlicher Backendvertrag

Bestehende inspect_simulation_run und inspect_large_project erhalten:

| Parameter | Standard | Bedeutung |
|---|---|---|
| waitSeconds | 0 | Ganzzahl 0–30, begrenztes Warten nach der ersten Statuslesung |
| after | weggelassen | Letztes watch.changeToken desselben Projekts/Laufs |
| diagnose | false | Nach dem Warten genau eine zusätzliche Diagnoseabfrage |
| detail | compact | Bestehender Ausgabeumfang; full aktiviert keine Diagnose |

Session und runId/actionId bleiben Pflicht. Schreibwerkzeuge akzeptieren diese
Warteparameter nicht. Kein neuer Werkzeugname und kein neues Baugate.

Standard-Simulationsstatus liest nur die vorhandenen Laufdaten. Die Mod lässt
sowohl die aggregierte Güterdiagnose als auch den Gebäudebetriebs-/Inventar-/
Wegescan aus. Historische Produktionsbelege, Zielbestände, Blockaden und Messwerte
bleiben vorhanden. Kein Opt-in aus detail=full ableiten. Bestehende native interne
Aufrufe ohne diagnose behalten ihr bisheriges Verhalten; der öffentliche
MCP-Statuspfad übergibt ausdrücklich diagnose=false und verlangt Bridge 0.47.
Kein stiller Rückfall auf eine teure alte Schnittstelle.

Projektstatus war bereits eine Ledgerlesung. diagnose=true ruft nach dem Warten
einmal diagnose_large_project auf und legt den vollständigen Ergebnisumschlag mit
eigenem Beobachtungszeitpunkt unter watch.diagnosis ab. Compact projiziert auch
diesen Diagnosebericht nach 0.46. Beim Simulationslauf liefert die letzte Lesung
mit diagnose=true frische Diagnose im bisherigen data-Vertrag. unavailable bleibt
sichtbar. Diagnosezustand returned bedeutet Antwort erhalten, keine vollständige
Abdeckung oder bewiesene Produktion.

## Warten und Änderungskennung

Der MCP-Server liest zuerst einmal. Danach höchstens eine leichte Folgelesung pro
Sekunde, bei unverändertem Zustand bis zur Wartefrist. Kein Warten auf dem
Spielthread, keine neue Spielautomation, kein Fortschalten manueller Baupläne,
kein Tempoeingriff und keine Verlängerung von Spiel-/Laufbudgets. Die bestehenden
HTTP-Abfragen bleiben jeweils über ihre Transportbudgets begrenzt. Erste Lesung
und optional abschließende Diagnose kommen zur Wartefrist hinzu; 30 Sekunden ist
keine Garantie für die gesamte End-to-End-Latenz.

watch enthält:

- changeToken: SHA-256 über Werkzeug, Session, Lauf-ID und relevante Zustandsfelder.
- changed: Vergleich zu after, sonst zur ersten Lesung; bei einfacher Momentaufnahme
  ohne after null. Kein Ereigniszähler und keine vollständige Änderungshistorie.
- reason: snapshot, changed, terminal, blocked oder wait_limit.
- waitedSeconds: tatsächliche Zeit nach der ersten Lesung bis zum Ende des Wartens.
- statusReads: Anzahl versuchter leichter Lesungen; eine bei Ablauf abgebrochene
  Lesung kann enthalten sein. diagnosticRequests: 0 oder 1, keine Anzahl einzelner
  interner Wegabfragen. Activity-Logging zählt nicht als Status-/Diagnoseabfrage.
- diagnosisState: not_requested oder returned; fachliche Unknowns/Unavailable
  stehen weiterhin im Diagnoseergebnis. Ohne Diagnose nichts als gesund auslegen.

Relevante Änderungen: Zustand/Grund, Tempo/Pause, Zielerreichung, beobachtete
Zielbestände, Teilzustände, neue Produktionszyklen und bekannte Blockaden.
Laufzeit, Fortschrittsraten, beobachtete Zeitanteile und reine Zeitstempel allein
wecken nicht. Für aktuelle Fortschrittswerte waitSeconds=0 lesen. Der Token wird
vor Compact-Projektion gebildet und ist von detail/Diagnoseumfang unabhängig.
Sitzungswechsel weiterhin anhand der angeforderten Session prüfen; alter Cursor
bei anderer gültiger Session ergibt einen anderen Token, niemals Projektwiederaufnahme.

Terminale Zustände und bereits bekannte Blockaden kehren sofort zurück; sie werden
nicht bis zur nächsten Änderung verborgen. Ohne neue Beobachtung liefert wait_limit
den letzten validierten Bericht mit dessen ursprünglichem meta.observedAtUtc.
Es gibt keine künstlich frische Zeitangabe. Verbindungs-/Sitzungs-/Validierungsfehler
werden nicht intern wiederholt. Benutzerabbruch bricht den Warteaufruf ab und
stoppt keine Simulation. watch.reason beschreibt das Ende der Wartephase; eine
nachfolgende gewünschte Diagnose kann einen nochmals neueren Zustand zeigen.

Bekannte Blockaden stammen aus Ledger oder vorhandenen Produktionsnachweisen.
Ohne Diagnose werden keine neuen Lager-/Transportursachen gesucht. Warten kann
mehr leichte Reads als ein einzelner Sofortaufruf erzeugen; Einsparung betrifft
Agentenrunden und den bislang bei jedem Simulations-Inspect ausgelösten Scan.
Keine pauschale Last-/Latenzgarantie.

## Regression und Livepilot

Vorbereitet: 30-Sekunden-Grenze mit künstlicher Uhr, keine Weckung durch bloßen
Zeitfortschritt, frühe Fertigstellung/Blockade, geänderte Teilzustände, Cursor mit
Session/ID-Bindung, Fehler ohne Retry, Abbruch, ungültige/duplizierte Parameter,
GET-only und null/eine Diagnoseanfrage, alte Bridge ohne stillen Diagnosefallback,
optionale Katalogfelder und echte stdio-Integration mit terminalem Sofortrücklauf.
0.47 ist explizit bekannt, negative Versionsfixture bleibt 99.0.0.
Keine Agent-Builds/Testläufe; Nachweis entsteht im menschlichen Gate.

Nach Gate: kurzer laufender Simulationslauf mit buildingIds. Standardlesung auch
mit full ohne currentBuildingOperations/productionDiagnostics; diagnose=true
liefert einmal dieselben fachlichen Diagnosebelege. Begrenztes Warten bis Änderung
oder Ende; terminaler Wiederaufruf mit gleichem Token sofort, changed=false.
Bekannte Blockade kehrt sofort zurück. Kurze unveränderte Warteprobe mit kleinem
Budget; Zeitstempel bleibt die tatsächliche Beobachtung. Projekt-Ledger an kleiner
Charge einschließlich optionaler frischer Diagnose prüfen, Hilfswege anschließend
zurückbauen, keine Rohstofflöschung. Bei falscher Pause, Mutation oder Diagnose im
Standardpfad stoppen. Kosten als leichte Reads und Diagnoseanfragen dokumentieren,
keine native Lastmessung aus bloßer Antwortgröße ableiten.

## Begrenzter Live-Nachweis 2026-10-12

- Zeitlauf: 2,000 s / zwei Reads und 30,007 s / 30 Reads jeweils wait_limit,
  changed=false und null Diagnosen trotz fortschreitender Spielzeit. Rückgabe
  behielt die Zeit der letzten Beobachtung, nicht die spätere Rückgabezeit.
- Folgende Warteprobe: nach 11,274 s / zwölf Reads completed, changed=true,
  genau drei Spielstunden und bestätigte Pause. Terminaler Wiederaufruf mit
  gleichem Token: ein Read, changed=false, sofortige Rückkehr.
- Zwei Produktionsgebäude: bekannte Blockaden mit einem Read sofort zurück;
  Full enthielt keine frische Betriebs-/Güterdiagnose. diagnose=true lieferte
  einen leichten Read plus genau eine frische Betriebsdiagnose beider Gebäude.
- Zwei temporäre Wege: Abschluss nach 1,024 s / zwei Reads ohne Diagnose.
  Terminales Full mit gleichem Token: changed=false, eine zusätzliche Diagnose
  mit eigenem Beobachtungszeitpunkt; zwei fertige Teile, keine verlorenen Zugänge.
  Projekt freigegeben, beide Testwege entfernt, beide Bestandswege bestätigt.
  Keine Rohstoffe gelöscht; Simulation abschließend pausiert.

Ein erster Vorschauaufruf nutzte fälschlich Path.Folktails. Lesende Kontrolle
bestätigte die native Vorlage Path und freie Teststellen; korrigierte Vorschau
bestand. Kein Bauauftrag aus der fehlerhaften Vorschau wiederholt. Die allgemeine
Fehlermeldung stufte diesen Vorschaufehler als unconfirmed ein; Präzisierung der
Aktionsklassifikation bleibt ein Verbesserungspunkt außerhalb dieser Statusänderung.

Nachweis gilt für diese Fälle, nicht als Lastbenchmark. Fehler-/Abbruch- und
Versionsgegenproben gehören zu den automatisierten Gate-Tests. Die Waldprotokolle
nutzen nun dieselbe Replace-Strategie wie Bauchargen; der ursprüngliche externe
Auslöser der Dateizugriffsverweigerung bleibt ungeklärt.
