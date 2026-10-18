# A12/N08: optionale Abschlussgeschwindigkeit — Entwurf

Stand 2026-10-18: Klärung abgeschlossen, Umsetzung und Abnahme offen.
Keine Code-/Schema-/Konfigurationsänderung. Heutige Pflichtpause bleibt wirksam.

## Befund und Empfehlung

SimulationRunController beendet erfolgreiche Läufe erst nach bestätigter Pause.
SimulationRunClient verlangt bei completed ausdrücklich PauseConfirmed.
LargeProjectBatch und BuildingCompletionTools stützen ihren Abschluss darauf.
SimulationControl verarbeitet Tempoaufträge verzögert; bei pausing entfernt es
ausstehende positive Befehle. Eine Änderung allein am letzten Tempowert würde
Vertrag, Besitzerfreigabe und Abbruchbehandlung widersprüchlich machen.

Empfohlen ist ein ausdrücklich begrenzter Pilot für **reine Zeitläufe über
run_simulation_for**. Neuer optionaler Parameter `completionSpeed`: 0 (Standard)
oder 1 (normales Weiterspielen). Andere Werte, until, Güter-/Forschungsziele,
Produktionsnachweise/-messung und intern gestartete Bau-/Reparaturläufe bleiben
außerhalb. Unzulässige Kombinationen vor Registrierung ablehnen.

Bei completionSpeed=1 endet nach dem Zeitfenster nur der überwachte Lauf;
das Spiel läuft anschließend ohne dessen Zeitbudget und Überwachung weiter.
Das muss Werkzeugbeschreibung und Ergebniszeile ausdrücklich sagen. Keine
automatische Übertragung dieser Wahl auf spätere Läufe oder globale Einstellung.

## Vorgesehene Abschlussregeln

| Situation | Verhalten im Pilot |
| --- | --- |
| Parameter fehlt oder 0 | Bestehenden Ablauf einschließlich Pause unverändert erhalten. |
| Reines Zeitende, Option 1 | Zuerst wie bisher pausieren und Pause beobachten; danach einmal Tempo 1 anfordern und tatsächlich beobachten. Erst dann terminal und Besitzer freigeben. |
| Während dieser Abschlussphase | Lauf behält Besitzer; neuer Start bleibt gesperrt. Zusätzlicher Zustand `completing`, noch kein terminaler Erfolg. |
| Cancel vor terminalem Abschluss | Abschlussauftrag für Tempo 1 verwerfen, ausstehendes positives Tempo löschen, Pause anfordern und bestätigen; cancelled bleibt pausiert. |
| Fehler, Echtzeitlimit, fehlende Beobachtung | Keine positive Abschlussgeschwindigkeit. Bestehende Fehler-/Pausenbehandlung; keine Erfolgsbehauptung bei unbestätigter Pause. |
| Spielsperre vor bestätigter Übergabe | Option 1 für diesen Lauf verwerfen; nach Entsperrung nur Pause bestätigen. Nie verzögert automatisch weiterlaufen. |
| Explizite manuelle/MCP-Tempoänderung | Lauf unterbrechen, ausstehende eigene Abschlussbefehle entfernen; Benutzerentscheidung nicht überschreiben, auch nicht bei gleichem Tempowert. |
| Spielneuladen/Sessionende | Keine Wiederaufnahme; Abschlussauftrag verwerfen. Bestehende Unload-Pause ohne erfundene Bestätigung. |
| Antwortverlust | Keine neue Aktion/ID und kein Replay. Gleichen Lauf plus aktuelle Session/Simulation lesen; Zustand kann bereits terminal sein. |
| Cancel einer bereits terminalen ID | Historischen Lauf nur lesen, keine nachträgliche Tempoänderung. Zum Pausieren frischen regulären Tempobefehl verwenden. |

Positive Tempoanforderung höchstens einmal und nur bei unverändertem Besitzer,
Session und beobachteter Pause. Zeitliche Grenze für die Bestätigung: höchstens
fünf Echtzeitsekunden, ohne Verlängerung des Laufbudgets. Reicht das Restbudget
nicht, Option nicht aktivieren; Fehler/Pause statt verdeckter Budgetverlängerung.
Bei fehlender Tempobestätigung nur noch Pause versuchen, sofern der Lauf weiterhin
Besitzer ist und keine explizite Benutzeränderung vorliegt; kein Retry von Tempo 1.

## Antwortvertrag und Kompatibilität

- `pauseConfirmed` bleibt ausschließlich die tatsächlich beobachtete Geschwindigkeit
  0, niemals ein historisches oder künstlich auf true gesetztes Abschlussflag.
- Ergänzende Abschlussbelege: angeforderte Abschlussgeschwindigkeit, tatsächlich
  angewandte Wahl, Bestätigung und Grund für einen verworfenen Wunsch. Option 1
  benötigt terminal beobachtetes Tempo 1; ein späteres Lesen des eingefrorenen
  Berichts beweist nicht das aktuelle Spieltempo.
- Native Validierung muss completed ohne Pause ausschließlich für diesen explizit
  belegten Pilotfall akzeptieren. Bau-/Produktionsverbraucher behalten ihre
  Pausenbedingung und erhalten die Option nicht.
- Ein altes Paket darf die Option niemals still ignorieren. Nicht unterstützte
  Fähigkeit vor Start ablehnen; neues Mod-/MCP-Paket gemeinsam ausliefern und
  Katalog-/Laufzeitbelege im menschlichen Gate prüfen. Bestehende Clients ohne
  Parameter erhalten weiterhin den alten Pausenvertrag.
- Ergebniszeile: Zeitfenster beendet, Tempo 1 bestätigt, weitere Simulation
  unüberwacht. Kein pauschales „Spiel pausiert“ oder neuer Produktionsnachweis.

## Geplanter Nachweis und Stoppkriterium

Vor Umsetzung müssen Parser, Controller, verzögerte Tempoqueue, native Modelle/
Validierung, MCP-Schema, Ergebniszeilen und bestehende Bauverbraucher gemeinsam
betrachtet werden. Synthetische Tests: Standardpause, Option 1, unzulässige
Kombinationen, Cancel in jeder Abschlussphase, Besitzerkonflikt, manuelle Änderung,
Lock/Unload, fehlgeschlagener Tempobefehl und Antwortverlust ohne Replay.

Nach menschlichem Gate maximal drei Livefälle: Standard-Zeitlauf endet pausiert;
expliziter Zeitlauf endet mit beobachtetem Tempo 1 und freiem Besitzer, danach
gezielt pausieren; abgebrochener Lauf mit Option 1 endet trotzdem pausiert.
Bei falschem Besitzer, verspätetem Weiterlaufen oder unklarer Bestätigung stoppen.
Lock-/Unload-/Transportfehler ohne natürliche Beobachtung als synthetisch geprüft
ausweisen. Keine Ausweitung auf Bau- oder Produktionsfamilien.

## Nutzen, Aufwand und Alternative

Nutzen: Agent kann ein festes Beobachtungsfenster beenden, ohne das weitere Spiel
dauerhaft anzuhalten. Aufwand **mittel** trotz kleinem sichtbarem Parameter:
Abschlusszustand, Besitz und verzögerte Tempobefehle müssen zusammen stimmen.
Keine neue Abhängigkeit, kein grundlegender Architekturwechsel erforderlich.

Konservative Alternative bleibt: normal pausiert abschließen, danach einen
ausdrücklichen separaten Tempobefehl senden. Das nutzt vorhandene Verträge,
benötigt aber einen zusätzlichen Agentenschritt und funktioniert nicht als
selbstständiger Abschluss bei fehlendem Folgeaufruf. Kein allgemeiner Umbau
aller Läufe empfohlen. Nächster Schritt ist die gesonderte Freigabe genau
dieses Entwurfs; noch keine Implementierungs- oder Liveabnahme.
