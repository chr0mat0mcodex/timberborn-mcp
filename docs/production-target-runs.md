# Produktionslauf bis Zielbestand

## Erweiterung 0.41: tatsächliche Produktion je Gebäude

Optionaler, exklusiver Gebäudemodus im selben Lauf: bis acht `buildingIds`,
Nachweis über native Zyklusabschlüsse statt globale Bestände. Der folgende
0.37-Bestandsnachweis ist kein Livebeleg für diese neue Funktion.
[Vertrag und bestandener 0.41-Pilot](building-production-proof.md).

Stand 2026-10-11: Nach menschlichem Gate gezielt live abgenommen.
Gemeinsamer 0.37.0-Release-Build, keine zweite Modfassung.

## Live-Nachweis vom 2026-10-11

- Kombiniertes Ziel: TreatedPlank 32 → 33, mindestens 40 Gear und 1083 Forschung.
  Nach 10,40625 Spielstunden / 30,20 Echtzeitsekunden automatisch abgeschlossen:
  33 TreatedPlank, 47 Gear, 1120 Forschung, `production_targets_reached`,
  bestätigte Pause. Eine separate Simulationsabfrage bestätigte Tempo 0.
- Bereits erfülltes Forschungsziel 1000: Abschluss ohne Spielzeitfortschritt
  und ohne Beschleunigung.
- Unbekanntes Gut: `state_conflict`; anschließende Statusabfrage `run_not_found`,
  Spielzeit und Tempo unverändert. Kein erneuter Startversuch.
- Grill mit fehlendem Spadderdock: `ingredients_not_ready` und getrennt
  `ingredient_global_shortage`, `missingIngredients=[Spadderdock]`.
  Nach genau einer Spielstunde `production_time_limit`, Bestand weiterhin 0,
  `productionTargetsReached=false`, bestätigte Pause, kein Zeitüberschuss.
- Erneutes Lesen des ersten Abschlusses nach dem Folgelauf: Zielbestände und
  Abschlusszeit unverändert; Diagnosezeitpunkt frisch um eine Spielstunde erhöht.

Der Pilot prüfte zwei Güter plus Forschung. Achtfachgrenze, Beobachtungsverlust,
Abbruch und Sonderfälle sind Gegenstand der automatischen Gate-Tests, nicht
zusätzlich als Livefälle nachgestellt. Keine Testbauten oder Rohstofflöschungen.
Endzustand: Tag 611, Stunde 12,09375, pausiert, alle gestarteten Testläufe terminal.
Der aktualisierte Werkzeugkatalog wurde erst nach Codex-Neustart im Chat sichtbar;
kein erneuter Mod-Build war dafür nötig.

## Aufruf und Bedeutung

`run_simulation_for` erhält optional `goodTargets` (ein bis acht eindeutige
Güter mit `good` und ganzzahligem `amount`, 1–1.000.000) und `stopAtScience`
(1–1.000.000). Ein Forschungsziel ohne Güter ist zulässig. Die Mod verwendet
`AvailableStock` und `SciencePoints`; Produktionspuffer, Lager, Transport und
Reservierungen werden nicht addiert. Alle angegebenen Ziele müssen in derselben
Beobachtung erreicht sein. Bereits erreichte Ziele lösen keine Beschleunigung aus.

Beispiel für die neuen optionalen Argumente, zusätzlich zu den bisherigen
Sitzungs-, Lauf-, Geschwindigkeits- und Zeitangaben:

```json
{
  "goodTargets": [
    { "good": "TreatedPlank", "amount": 8 },
    { "good": "Gear", "amount": 10 }
  ],
  "stopAtScience": 100
}
```

Das vorhandene `stopAtAvailableLogs` bleibt kompatibel, darf aber nicht mit den
neuen Zielen gemischt werden. `run_simulation_until` bleibt ein reiner Zeitlauf.
Unbekannte Güter oder fehlende Bestandsbeobachtung verhindern den Start ohne
Tempoänderung; ein Beobachtungsverlust im laufenden Auftrag fordert eine Pause an.
Keine automatische Wiederholung. Unbestätigte Starts nur über dieselbe Lauf-ID lesen.

`productionTargetsReached=true` mit `reason=production_targets_reached` meldet
das gemeinsame Bestandsziel; terminaler Erfolg setzt die beobachtete Pause voraus.
`production_time_limit` bedeutet nur Ablauf des Spielzeitbudgets, nicht erfolgreiche
Produktion. `targetReached` bleibt der bisherige Zeitindikator. Echtzeitlimit,
Spielsperre, Abbruch, manuelle Tempoänderung und Sessionwechsel behalten ihre
bisherigen Regeln. Maximal 28 Spieltage und 86.400 Echtzeitsekunden bleiben Grenzen.
Ein Zielbestand ist weder Ertragsnachweis noch Materialreservierung.

## Diagnose und Abfragekosten

Die Mod prüft während des Laufs nur die höchstens acht Güter und Forschung.
Produzentendiagnose wird bei expliziten Start-/Status-/Abbruchantworten gelesen,
nicht in jedem Tick. `productionDiagnostics` berichtet je Ziel höchstens 32
fertige Manufakturen mit aktuell passendem Rezept. `complete` kennzeichnet nur
die Begrenzung dieser Stichprobe, keine vollständige Abdeckung aller Produktionsarten.
Fehlende Diagnose bleibt null und beendet nicht einen sonst beobachtbaren Lauf.

Belegte Hinweise: Pause, keine Arbeiter, Unterbesetzung, außerhalb der Arbeitszeit,
fehlende Rezeptzutaten/Brennstoff, fehlender Ausgaberaum, Produktionsbereitschaft
und aktuelles Leistungsdefizit des verbundenen Netzes. `missingIngredients`
benennt Rezeptzutaten, deren global verfügbarer Bestand unter der Rezeptmenge
liegt. Das beweist keinen leeren lokalen Eingang; `ingredients_not_ready` ist
die separate native Beobachtung. Unterschiedliche Produzenten können gleichzeitig
bereite und blockierte Hinweise erzeugen. Keine Aussage, dass alle Produzenten
blockiert seien. Keine stillschweigende Erhöhung des Zeitbudgets wegen einer Diagnose.

Lieferung unterwegs und Wind als Ursache sind nicht vollständig beobachtet;
ein Leistungsdefizit wird deshalb nicht als Windstille ausgegeben. Nicht-Manufakturen,
fehlende Rezeptwahl und spezielle Forschungsquellen sind nicht vollständig erfasst.
`no_current_manufactory_source` bedeutet keine passende beobachtete Manufaktur,
nicht die Unmöglichkeit einer anderen Bezugsquelle.

Terminale Zielbestände bleiben als Abschlussbeleg eingefroren. Die Diagnose ist
dagegen frisch; `diagnosticsObservedGameHours` nennt ihren Beobachtungszeitpunkt.
Lesen verändert weder Zielbestände noch Aufträge oder Geschwindigkeit.

## Testgate

Vorbereitete Tests: gleichzeitige Mehrfachziele statt zeitversetzter Spitzen,
Forschung separat, bereits erfüllte Ziele, unbekannte Beobachtung, Spiel-/Echtzeit-
limit, Spielsperre, Abbruch, manuelles Tempo, kein Wiederverwenden von IDs,
Duplikate/Limits, Rückwärtskompatibilität, MCP-Schema und Transportvertrag.
Keine Agent-Builds oder Testausführung; Nachweis entsteht im Entwickler-Gate.

Abnahmeplan (oben mit konkreten Ergebnissen abgeschlossen):

1. Frische Bestände und Produzenten lesen; TreatedPlank knapp über Bestand sowie
   ein vorhandenes zweites Gut und optional Forschung als gemeinsames Ziel.
   Ein Start, Zielbestände und automatische Pause separat bestätigen.
2. Bereits erfülltes Forschungsziel: keine Beschleunigung. Unbekanntes Gut:
   Ablehnung ohne Tempoänderung oder gestarteten Lauf.
3. Ein vorhandener Produzent mit fehlender Zutat: den nativen Zutatenbefund und
   globale Fehlmengen getrennt prüfen; kurzer unerreichbarer Zielbestand endet
   am Zeitlimit ohne behaupteten Zielerfolg. Keine Rohstoffe für den Test löschen.

Bei falschem Kontrollfall stoppen und diagnostizieren. Erst nach Gate und
bestandenem gezieltem Livetest committen und pushen.
