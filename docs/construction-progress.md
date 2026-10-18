# Fortschrittsdiagnose — begrenzt live abgenommen


## Fortschrittspilot 2026-10-18 — begrenzt live bestanden

Menschliches Korrekturgate: 2002 Unit- und 13 Integrationstests bestanden,
drei optionale Integration-Livetests übersprungen; Release-Build ohne Fehler.
Kleine Lodge plus Weg nativ geprüft (pilotEligible/restored), einmal beauftragt.
Erste Diagnose bei 08:28 liefert baseline_recorded, erneute Abfrage bei Pause
no_game_time_elapsed. Der fertige Weg mit construction=null wird korrekt gelesen.
Einzelabfrage bestätigt Material- und Baufortschritt 0, Baustellenvorrat leer.

Erstes festes Fenster: eine Spielstunde bis 09:28, 2,99 Echtzeitsekunden,
30 Sekunden Höchstbudget. Ziel exakt erreicht, overshootHours=0, Pause bestätigt.
Diagnose waiting_for_materials: beide Fortschrittsdifferenzen 0, Vorrat unverändert.
Einzelabfrage der Lodge stimmt überein; Zentrum und Bauhütte jeweils vier
Beschäftigte, Arbeitszeit und laufender Job aktiv. Compact/Full erhalten dieselben
Messpunkte. Weitere Diagnose verlängert das Fenster nicht; kein aktiver Lauf.

Separat ausdrücklich gestartetes zweites Fenster: drei Spielstunden bis 12:28,
8,86 Echtzeitsekunden bei weiter 30 Sekunden Höchstbudget. Diagnose
finished_since_baseline; Einzelabfrage bestätigt fertige Lodge und construction=null.
Keine erfundenen numerischen Endwerte nach Fertigstellung. Vergleich bleibt
auch in Compact erhalten, obwohl fertige Teile aus der Aufmerksamkeitsliste entfallen.
Erneute Diagnose und Laufabfrage: Ziel und Grenzen unverändert, overshootHours=0,
terminal und Pause bestätigt. Projekt freigegeben, beide Teile fertig/zugänglich,
keine beobachteten Bestandszugangsverluste; Spiel pausiert, kein Lauf/Tempoauftrag.

Grenzen: Lieferung und Teilbaufortschritt lagen zwischen den Messpunkten.
Positive numerische Material-/Baudeltas und Stillstand bei materialbereiter
Baustelle bleiben synthetische Gate-Nachweise. Keine Dauerarbeits-, Liefermengen-
oder allgemeine Produktionsgarantie. Budgetbeleg gilt für diese getrennten
Simulationsfenster, nicht als zusätzlicher autonomer Projektlauf-Nachweis.

Historischer Fehler behoben: JSON-null wurde im nativen Adapter als Objekt
indiziert. Explizite JObject-/JArray-Prüfungen sind nun im Gate und im
Lodge-/Weg-Livepilot bestätigt.


`diagnose_large_project` und `inspect_large_project(diagnose=true)` ergänzen
`progressComparison`: Vorher/Nachher-Werte je Teil, vergangene Spielstunden,
Differenzen von Material- und Baufortschritt sowie Baustellenvorrat je Gut.
Die Vergleichswerte bleiben in Compact und Full erhalten, auch für fertige Teile.
Die bestehende aktuelle Blockadediagnose und der historische Laufstatus bleiben getrennt.

## Ablauf

1. Nach bestätigtem Bauauftrag bei Pause eine frische Diagnose lesen. Der erste
   Aufruf erfasst den Ausgangswert (`baseline_recorded`); normale Statusabfragen
   ohne `diagnose=true` erfassen keinen Ausgangswert.
2. Ein ausdrücklich begrenztes Simulationsfenster wählen oder einen bereits
   laufenden begrenzten Projektlauf beobachten. Keine automatische Verlängerung.
3. Erneut frisch diagnostizieren und `progressComparison.parts` lesen. Verglichen
   wird mit der vorherigen frischen Diagnose bei anderer Spielzeit. Wiederholte
   Abfragen zur selben Spielzeit behalten das letzte Vergleichsfenster.
4. Nächste Schritte nach aktuellem Zugang, Pause, Stützen und Fortschrittsbelegen
   wählen. Bei fehlendem Ausgangswert zuerst einen neuen erfassen, nie alte
   Projektaufträge erneut senden.

## Befunde und nächste Schritte

| Befund | Bedeutung / nächster Schritt |
| --- | --- |
| `no_game_time_elapsed` | Kein Zeitfenster, kein Stillstandsnachweis. |
| `construction_progress` | Bauwert gestiegen; innerhalb des bestehenden Budgets beobachten. |
| `material_progress` | Materialfortschritt oder Baustellenvorrat gestiegen; Lieferung im bestehenden Budget beobachten. |
| `finished_since_baseline` | Fertigstellung beobachtet; Zugang und Abschluss separat prüfen. |
| `currently_outside_working_hours` | Jetzt gemeinsamer Feierabend; nächste Arbeitsphase bewusst wählen. |
| `waiting_for_materials` | Keine positive Materialänderung beobachtet; Produzenten, Lager und Lieferweg prüfen. |
| `no_progress_builders_available_at_endpoints` | Unveränderter Bauwert bei verfügbaren Bauarbeitern an beiden Messpunkten: Besetzung, Aufträge, Zugang, Anmarsch und konkurrierende Baustellen prüfen. |
| `current_blocker` | Aktuelle Pause, fehlender Bauzugang oder unfertige Stütze gezielt prüfen. |
| `no_progress_observed` | Arbeitszeit/Besetzung fehlen oder wechselten; keine eindeutige Ursache. |
| `observation_unknown`, `not_comparable`, `progress_regressed` | Identität, Messwerte oder Rückgang klären; kein erfundener Stillstand. |

`siteStockDelta` ist eine Nettodifferenz, keine Bruttoliefermenge: Material kann
zwischen Messungen geliefert und verbraucht worden sein. Fortschrittswerte sind
native Messwerte, keine geschätzte Restzeit. Positive Fortschritte können mit
einer aktuellen Blockade zusammen auftreten; beide Belege beachten.
Arbeitsbereitschaft an zwei Endpunkten beweist weder durchgehende Arbeit noch
Zuweisung zu genau dieser Baustelle. Feierabend am Ende erklärt nicht das gesamte
Intervall. Konkurrierende Baustellen sind ein Prüfauftrag, keine bewiesene Ursache.

Maximal zwei Messungen je Teil im vorhandenen begrenzten Projektbestand.
Die Werte leben in der Spiel-Bridge: MCP-Neuverbindung erhält sie, Spielneuladen
verliert sie zusammen mit dem Projektledger. Interne Schutz-/Reparaturdiagnosen
und Abschlussabfragen verschieben das Fenster nicht. Keine neuen Schreibwerkzeuge,
keine automatische Simulation, keine Änderung an Zeitbudgets oder Material-Startregeln.

## Abnahmeumfang

Gate und begrenzter Livepilot bestanden, konkrete Messwerte und Grenzen oben.
Keine Builds/Tests durch den Agenten. Anleitung, Werkzeugvertrag, Projektstand
und Backlog aktualisiert; keine zusätzlichen Spielskills oder Hermes-Anbindung.
