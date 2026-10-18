# A12/N08: optionale Abschlussgeschwindigkeit — begrenzter Pilot

Stand 2026-10-18: menschliches Gate und drei begrenzte Livefälle bestanden.
Standardpause bleibt. Nur reine Zeitläufe sind für die Abschlussoption abgenommen.

## Befund und Empfehlung

SimulationRunController beendet erfolgreiche Läufe erst nach bestätigter Pause.
SimulationRunClient verlangt bei completed ausdrücklich PauseConfirmed.
LargeProjectBatch und BuildingCompletionTools stützen ihren Abschluss darauf.
SimulationControl verarbeitet Tempoaufträge verzögert; bei pausing entfernt es
ausstehende positive Befehle. Eine Änderung allein am letzten Tempowert würde
Vertrag, Besitzerfreigabe und Abbruchbehandlung widersprüchlich machen.

Implementiert ist ein ausdrücklich begrenzter Pilot für **reine Zeitläufe über
run_simulation_for**. Neuer optionaler Parameter `completionSpeed`: 0 (Standard)
oder 1 (normales Weiterspielen). Andere Werte, until, Güter-/Forschungsziele,
Produktionsnachweise/-messung und intern gestartete Bau-/Reparaturläufe bleiben
außerhalb. Unzulässige Kombinationen vor Registrierung ablehnen.

Bei completionSpeed=1 endet nach dem Zeitfenster nur der überwachte Lauf;
das Spiel läuft anschließend ohne dessen Zeitbudget und Überwachung weiter.
Das muss Werkzeugbeschreibung und Ergebniszeile ausdrücklich sagen. Keine
automatische Übertragung dieser Wahl auf spätere Läufe oder globale Einstellung.

## Abschlussregeln

| Situation | Verhalten im Pilot |
| --- | --- |
| Parameter fehlt oder 0 | Bestehenden Ablauf einschließlich Pause unverändert erhalten. |
| Reines Zeitende, Option 1 | Zuerst wie bisher pausieren und Pause beobachten; danach einmal Tempo 1 anfordern und tatsächlich beobachten. Erst dann terminal und Besitzer freigeben. |
| Während dieser Abschlussphase | Lauf behält Besitzer; neuer Start bleibt gesperrt. Zusätzlicher Zustand `completing`, noch kein terminaler Erfolg. |
| Cancel vor terminalem Abschluss | Abschlussauftrag für Tempo 1 verwerfen, ausstehendes positives Tempo löschen, Pause anfordern und bestätigen; cancelled bleibt pausiert. |
| Fehler, Echtzeitlimit, fehlende Beobachtung | Keine positive Abschlussgeschwindigkeit. Bestehende Fehler-/Pausenbehandlung; keine Erfolgsbehauptung bei unbestätigter Pause. |
| Spielsperre vor bestätigter Übergabe | Option 1 für diesen Lauf verwerfen; nach Entsperrung nur Pause bestätigen. Nie verzögert automatisch weiterlaufen. |
| Expliziter MCP-Tempobefehl / beobachtete manuelle Abweichung | Lauf unterbrechen, ausstehende eigene Abschlussbefehle entfernen; MCP-Befehle unterbrechen auch bei gleichem Wert. Die öffentliche Beobachtung kann gleichwertige UI-Klicks ohne Zustandsänderung nicht als eigene Absicht erkennen. |
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
- Abschlussbelege: `completionSpeed` (angefordert), `effectiveCompletionSpeed`
  (0/1 erst nach bestätigtem Erfolg/Cancel, sonst null), `completionSpeedConfirmed`,
  `completionPauseObserved` (historische Zwischenpause) und
  `completionFallbackReason` (verworfener Wunsch). Option 1
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

## Nachweisstand und Stoppkriterium

Parser, Controller, verzögerte Tempoqueue, native Modelle/Validierung, MCP-Schema,
Ergebniszeilen und bestehende Bauverbraucher wurden zusammen geprüft.
Regressionen in `CompletionSpeedTests` im menschlichen Gate bestanden: Standardpause, Option 1, unzulässige
Kombinationen, Cancel in jeder Abschlussphase, Besitzerkonflikt, manuelle Änderung,
Lock/Unload, fehlgeschlagener Tempobefehl und Antwortverlust ohne Replay.

Nach menschlichem Gate maximal drei Livefälle: Standard-Zeitlauf endet pausiert;
expliziter Zeitlauf endet mit beobachtetem Tempo 1 und freiem Besitzer, danach
gezielt pausieren; abgebrochener Lauf mit Option 1 endet trotzdem pausiert.
Bei falschem Besitzer, verspätetem Weiterlaufen oder unklarer Bestätigung stoppen.
Lock-/Unload-/Transportfehler ohne natürliche Beobachtung nach bestandenem Gate
ausschließlich als synthetisch geprüft ausweisen. Keine Ausweitung auf Bau- oder Produktionsfamilien.

## Nutzen, Aufwand und Alternative

Nutzen: Agent kann ein festes Beobachtungsfenster beenden, ohne das weitere Spiel
dauerhaft anzuhalten. Aufwand **mittel** trotz kleinem sichtbarem Parameter:
Abschlusszustand, Besitz und verzögerte Tempobefehle müssen zusammen stimmen.
Keine neue Abhängigkeit, kein grundlegender Architekturwechsel erforderlich.

Konservative Alternative bleibt: normal pausiert abschließen, danach einen
ausdrücklichen separaten Tempobefehl senden. Das nutzt vorhandene Verträge,
benötigt aber einen zusätzlichen Agentenschritt und funktioniert nicht als
selbstständiger Abschluss bei fehlendem Folgeaufruf. Kein allgemeiner Umbau
aller Läufe empfohlen. Bewertung nach Liveabnahme: Nutzen bestätigt für bewusst
unüberwachte Fortsetzung nach einem begrenzten Beobachtungsfenster. Aufwand mittel
wegen Controllerzustand, verzögerter Queue, Antwortvertrag und Regressionen;
49 zusätzliche Unit-Testfälle gegenüber N06. Keine neue Abhängigkeit.
Pilot beibehalten; kein Anlass, Bau- oder Produktionsläufe jetzt zu erweitern.


## Technische Abgrenzung

API-Kennung bleibt 0.74.0; Gate und Liveabgleich müssen das zusammengehörige neue
Mod-/MCP-Paket über Build-/Katalogkennung bestätigen. Alte Mod-Parser lehnen den
unbekannten Queryparameter vor Laufregistrierung ab; ein still ignoriertes
completionSpeed=1 wird zusätzlich durch den neuen nativen Antwortvertrag abgewiesen.
Ein Client ohne Parameter erhält weiterhin den bisherigen Pausenablauf.

`completing` hält den Controllerbesitzer. Die Queue prüft direkt vor Übergabe
Pause, Uhr und Budget nochmals; ein Lockevent verwirft den Wunsch auch zwischen
Updates. Nach einem fehlgeschlagenen Tempo-1-Versuch folgt höchstens die reguläre
Fehlerpause, kein positives Retry. Die fünf Sekunden Bestätigungszeit beginnen
mit der Anforderung und verlängern das ursprüngliche Budget nicht; vor Anforderung
müssen mindestens fünf Sekunden verbleiben. Die bestehende Fehlerpause kann danach
noch fünf Sekunden benötigen oder auf die Freigabe einer Spielsperre warten.

Ein terminaler Bericht ist eingefroren. Nach Erfolg mit Tempo 1 immer den frischen
Simulationszustand separat lesen; Cancel derselben terminalen ID pausiert nicht.
Nach dem Livefall deshalb explizit `set_simulation_speed` auf 0 verwenden.


## Abnahme 2026-10-18

Menschliches Gate: 2353 Unit-, 19 Integrations-, 13 Paket- und 35 Supervisor-Prüfungen
bestanden; drei optionale Liveprüfungen übersprungen. Beide Release-Builds ohne
Warnungen/Fehler; fünf installierte Paketdateien per SHA-256 bestätigt.
Geladene MCP-Build-/Katalogkennung stimmten mit dem Gate überein, Bridge 0.74.0.
Der neue Parameter wurde über die bestehende MCP-Verbindung tatsächlich übertragen
und im registrierten Lauf mit completionSpeed=1 zurückgemeldet.

| Livefall | Beobachtung | Ergebnis |
| --- | --- | --- |
| Standard ohne Option | 0,25 Spielstunden bei Tempo 3; completed/target_reached, beobachtetes Tempo 0, pauseConfirmed=true, effectiveCompletionSpeed=0; kein Überschwingen. Frisch: Tempo 0, kein Besitzer/Tempoauftrag. | Bestanden |
| Explizit completionSpeed=1 | 0,25 Spielstunden bei Tempo 3; completed, completionPauseObserved=true, completionSpeedConfirmed=true, effectiveCompletionSpeed=1, pauseConfirmed=false, kein Fallback. Frische Abfrage: Tempo 1, Spielzeit über das Laufende hinaus fortgeschritten, Besitzer/Tempoauftrag frei. Danach ausdrücklich pausiert und Tempo 0 bestätigt. | Bestanden |
| Cancel mit Option 1 | Fünf-Stunden-Zeitfenster bei Tempo 1 vorzeitig abgebrochen; cancelled/cancel_requested nach 0,15625 Spielstunden, Zeitziel nicht erreicht. effectiveCompletionSpeed=0, Pause bestätigt, completionFallbackReason=cancel_requested. Frisch: Tempo 0, kein Besitzer/Tempoauftrag. | Bestanden |

Der kurze completing-Zustand wurde nicht als eigener Live-Snapshot abgefangen;
der terminale Zwischenpause-/Tempo-Beleg und die getrennte Zustandsabfrage sind
der Livebeleg. Besitzerbindung während completing, Cancel innerhalb dieser Phase,
Lock-/Unload-/Budget-/Transportfehler und UI-Rennen bleiben synthetische Nachweise.
Kein natürlicher Lock beobachtet: speedLocked/pauseSettled blieben ausdrücklich
unbekannt, die tatsächliche Geschwindigkeit 0 wurde unabhängig gelesen.
Keine Ausweitung auf Bau-, Produktions- oder until-Läufe.
