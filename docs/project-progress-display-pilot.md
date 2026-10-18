# A12/N05: Fortschritt im Ingame-MCP-Log

Stand 2026-10-18: Fehler live reproduziert, enge Korrektur nach menschlichem
Gate und Abschluss-Livetest begrenzt abgenommen. Bridge/API bleibt 0.74.0;
geändert wird ausschließlich die MCP-Ergebnisdarstellung samt Regressionstests.

## Befund und Ursache

Am vorhandenen N04-Zweiteileprojekt: vollständiger Bericht enthält
`partStates=[construction,finished]`, kompakter Bericht
`summary.finishedAccessRecorded=1`, jeweils zwei geplante Teile. Das terminale
Ledger ist absichtlich historisch: nach seinem Zeitlimit wurde das Lager in
einem separaten Simulationsfenster fertig; frische Diagnose bestätigte 2/2.
Diese Differenz ist korrekt und darf nicht durch Umschreiben des Ledgers
oder eine zusätzliche versteckte Diagnose beseitigt werden.

Fehlerhaft ist die Ergebniszeile für `inspect_large_project`: sowohl bei
`detail=full` als auch `compact` meldete sie **„Bauprüfung: 0 von 2“**.
Der Text wurde aus dem strukturierten Agentenlog gelesen und auf der aktuellen
Spielaufnahme tatsächlich gesehen. Keine Screenshots oder Rohlogs im Repository.

`BuildingActivityOutcome.Large` wählte die Diagnoseprojektion allein anhand
eines `parts`-Arrays. Native Chargenquittungen besitzen ebenfalls `parts`,
aber mit `state` statt `observed`. Dadurch zählte die Diagnoseprojektion null
fertige Beobachtungen und behauptete zusätzlich eine frische Bauprüfung.
`InvokeLogged` erzeugt die Zeile vor `ResponsePresentation.Present`; deshalb
tritt der Fehler auch bei einer anschließend korrekt verdichteten Antwort auf.
Dieser Mechanismus erklärt den reproduzierten Fehler und passt zum historischen
0/15-Befund; dessen damalige Rohantwort wird damit nicht nachträglich rekonstruiert.

## Begrenzte Korrektur

Direkte Großprojektberichte wählen die vorhandene Projektion anhand des
Werkzeugnamens. Inspektion/Start/Stopp bleiben Ledger; Diagnose/Close bleiben
frische Recovery-Beobachtungen. Gleiche vorhandene Zählung wie im Fachbericht,
keine zweite Zählimplementierung, keine zusätzlichen Spielabfragen oder Aufträge.
Verschachtelte Räumgruppen behalten ihre bisherige Darstellung und sind kein
neuer Abnahmeumfang. Stopgrundtexte und andere Auftragsfamilien bleiben B10-Restarbeit.

Acht neue synthetische Testfälle: reale Chargenform mit `parts` und
`partStates`, 14/15 laufend oder angehalten sowie 15/15 abgeschlossen;
Start, Inspektion, Stopp und Reparaturstart; frische Diagnose/Close 2/2 trotz
älterem aufgezeichnetem Teilstand. Full/Compact-Gleichheit, Belegbezeichnung
und unveränderte Eingabedaten geprüft; alle acht neuen Fälle im Gate bestanden.

## Verwendeter Abschlussplan nach menschlichem Gate

1. Backend starten, Session und neue MCP-Buildkennung bestätigen.
2. Kleinen Zweiteileplan mit kostenfreiem Weg und Lager verwenden. Native
   Gesamtvorschau, Kosten und Bauzugang wie üblich prüfen; ein Auftrag.
3. Während des Laufs `inspect_large_project` full/compact lesen und die jeweilige
   Logzeile mit `partStates` beziehungsweise `summary` vergleichen. Aufnahme
   der sichtbaren Ergebniszeile: gespeicherter Stand, korrekte Teilzahl.
4. Nach Budgetende oder Fertigstellung denselben Vergleich terminal wiederholen.
   `diagnose_large_project` getrennt lesen: „Bauprüfung“ zählt frische
   `observed=finished`/`finishedAccessible`, nicht historische Ledgerwerte.
5. Bei einer Abweichung stoppen und diagnostizieren; keine Fallserie. Auftrag,
   Fertigbau, Zugang und Logbeleg getrennt festhalten. Nach geklärtem Fertigbau
   Journal freigeben, Pause bestätigen. Erst danach Commit/Push.

## Gate und Liveabnahme

- Menschliches Gate: 2292 Unit-, 19 Integrations-, 13 Paket- und 35 Supervisor-
  Prüfungen bestanden. Drei optionale Livefälle übersprungen; beide Release-
  Builds ohne Warnungen/Fehler, fünf installierte Dateien per SHA-256 bestätigt.
  Geladene MCP-Build- und Katalogkennung entsprechen dem Gate.
- Neuer Zweiteileplan: kleines Lager Cw90 und automatisch ergänzter Bodenweg.
  Zwei Karotten regulär geräumt. Native gemeinsame Vorschau gültig, restauriert,
  keine verlorenen Bestandsverbindungen; vorgeschlagene Arbeitszelle erreichbar.
  Drei Stämme Gesamtkosten, 260 verfügbar (rund 8667 Prozent Deckung); reguläre
  Betriebsdiagnose vor Auftrag. Ein Start, vier Spielstunden/60 Sekunden Budget.
- Laufender Vollbericht zunächst 0/2 und zwei unbestätigte Aufträge: Logzeile
  identisch, korrekt als gespeicherter Projektstand bezeichnet. Kurz danach
  kompakter Bericht 1/2, zwei bestätigte Platzierungen; zugehörige Ergebniszeile
  ebenfalls 1/2. Unterschiedliche Abfragezeitpunkte, kein Full/Compact-Widerspruch.
  Die laufende 1/2-Zeile im Spielbild tatsächlich gesehen.
- Nach 1,78125 Spielstunden beide Teile fertig, Projekt completed und Pause
  bestätigt. Terminal full/compact und beide Ergebniszeilen identisch 2/2 als
  gespeicherter Projektstand. Auch terminale 2/2-Zeile im Spielbild gesehen.
- Frische Diagnose full/compact: 2/2 fertig und zugänglich, Logzeilen ausdrücklich
  „Bauprüfung: 2 von 2“. Keine fremden Baustellen oder verlorenen Verbindungen.
  Fertiges Lager separat: Eingang frei/zugänglich, native Distriktdistanz 4.
  Eine separate Baustellenzugangsabfrage kam erst nach Fertigbau; daraus wird
  kein nachträglicher buildersReachable-Beleg behauptet.
- Journal regulär freigegeben, Spieltempo 0, kein aktiver Lauf oder Tempoauftrag.
  Keine Aufträge wiederholt und keine weiteren Auftragsfamilien erweitert.

## Nutzen, Aufwand und verbleibende Grenze

Nutzen hoch bei kleinem Aufwand: eine falsche Null und eine falsche Nachweisart
werden aus derselben Ursache beseitigt. Kein Anlass für ein größeres Refactoring.
Eine kleine Änderung in einer Darstellungsklasse, acht Regressionstestfälle
und ein Zweiteile-Livepilot genügen. Keine neue Abhängigkeit, keine Mod- oder
Vertragsänderung und keine zusätzliche Laufzeitabfrage. Ein größerer Umbau
ist nicht begründet. Unterschiedliche historische/frische Zahlen sind durch
die synthetischen Tests abgesichert; nach diesem Live-Gate waren beide Endstände
2/2. Zeitlimit-/Stopgrundtexte und verschachtelte Räumgruppen bleiben außerhalb
dieser Abnahme. Die strukturierten Fachberichte bleiben maßgeblich.
