# Simulation nach Spielzeit laufen lassen — 0.22.0

Priorität: **WICHTIG**. Implementiert, automatisch geprüft und **im begrenzten Live-Pilot bestätigt**.
Ergänzt [Pause und Geschwindigkeit](simulation-control.md).

## MCP-Vertrag

| Aufruf | Pflichtparameter |
| --- | --- |
| `run_simulation_for` | `duration`, `unit`, `speed`, `expectedSpeed`, `maxRealSeconds`, `runId`, `session` |
| `run_simulation_until` | `dayNumber`, `hour`, `speed`, `expectedSpeed`, `maxRealSeconds`, `runId`, `session` |
| `inspect_simulation_run` | `runId`, `session` |
| `cancel_simulation_run` | `runId`, `session` |

Alle unterstützen die optionale kurze Aktionsbegründung `reasoning` im Ingame-MCP-Log.
Das Log bestätigt den einzelnen MCP-Aufruf; der Laufstatus belegt den Laufabschluss.

- `unit`: `hours`, `days`, `weeks`. Tag = 24 Spielstunden, Woche = sieben Tage.
  Dauer positiv, Bruchteile erlaubt, maximal 28 Spieltage.
- Absolutes Ziel: `dayNumber` aus `inspect_simulation`, **kein Zyklustag** oder
  umgerechneter UI-Tag. `hour`: 0 einschließlich bis 24 ausschließlich.
  Vergangene Ziele und mehr als 28 Tage Vorlauf werden abgelehnt. Ein bereits
  erreichtes Ziel startet keinen Lauf, fordert Pause an und bestätigt sie separat.
- `speed`: 1, 3, 7; `expectedSpeed`: zuletzt beobachtete 0/1/3/7.
- `maxRealSeconds`: Pflichtwert 30..86400; 7200 ist ein möglicher Wert, keine
  Zusage, dass jede Spieldauer innerhalb dieser Echtzeit erreicht wird.
- `runId`: neue UUID vom Aufrufer. Nach unklarer Startantwort ausschließlich
  mit dieser ID nachlesen, niemals automatisch erneut starten. Eine aktive Aufgabe,
  maximal 128 aufbewahrte Aufträge pro Session; IDs nicht wiederverwendbar.
- Start/Abbruch nutzen das bestehende Speed-Control-Opt-in in MCP und Mod.
  Status ist lesend. Alle vier Aufrufe verlangen die aktuelle Session.

## Ablauf und Ergebnis

Die Mod prüft die tatsächliche Spieluhr in jedem Hauptthread-Update. MCP-Polling
ist für den Lauf nicht nötig. Regulärer `SpeedManager`, keine Zeitsprünge oder
Entsperrung von Spielsperren.

`starting` → `running` → `pausing` → `completed`. Erfolg erst nach erreichtem
Ziel UND beobachteter Geschwindigkeit 0. Auch `cancelled` braucht eine bestätigte
Pause. `failed` und `interrupted` sind keine Erfolgsmeldungen.

Status: Start-, Ziel- und beobachtete Zeit in absoluten Spielstunden
(`dayNumber * 24 + hoursPassedToday`), vergangene Spiel-/Echtzeit, `overshootHours`,
angeforderte/beobachtete Geschwindigkeit, `targetReached`, `pauseConfirmed`,
`terminal`, Zustand und Grund. Terminale Belege bleiben eingefroren. Aktuellen
Spielzustand mit `inspect_simulation` lesen. Überschreitung wird nicht zurückgedreht.

## Grenzen und Abbruch

- Beobachtete manuelle Geschwindigkeitsänderung unterbricht, ohne automatische
  Wiederaufnahme. Ein gültiger `set_simulation_speed` beendet den Auftrag auch bei
  gleichem Wert. Gleichwertige UI-Klicks ohne beobachtbare Änderung sind nicht erkennbar.
- 120 Sekunden ohne Spielzeitfortschritt, rückläufige Spielzeit oder das Echtzeitlimit
  fordern Pause an. Start und Pause haben je fünf Sekunden Bestätigungsfrist.
  Unbestätigte Pause: `failed` / `pause_unconfirmed`, keine Pausenzusage.
- Wiederholter Abbruch verlängert keine Frist und sendet keinen zweiten Befehl.
  Befindet sich der Lauf bereits in der Abschlusspause, bleibt deren Abschlussgrund erhalten.
- **MCP-/Client-Disconnect lässt den begrenzten Auftrag in der Mod weiterlaufen.**
  Abbrechen über `cancel_simulation_run`. Netzwerkabbruch ist kein Spielabbruch.
- Monotone Echtzeitfristen werden erst im nächsten Spiel-Update ausgewertet.
  Eine eingefrorene Anwendung kann die Mod nicht von außen pausieren.
- Beim Szenenende wird Pause angefordert, ohne Bestätigung zu behaupten.
  Aufträge werden nicht gespeichert oder in neue Sessions übernommen.
- Fachliche Fehler: `run_not_found`, `run_id_used`, `invalid_time_target`,
  `stale_session`, `state_conflict`. Schreibaktionen nie automatisch wiederholen.

## Nachweise und Live-Abnahme

Automatisch: zwei Stunden bei allen drei Stufen, zwei Tage/zwei Wochen, absolutes
Ziel über Tagesgrenze, verzögerte Pause/Überschreitung, vergangenes Ziel, Doppel-/
Parallelauftrag, Eingriff, Abbruch, Stillstand, Start-/Pausensperre, Timeout,
Szenenende sowie Parameter- und Antwortprüfung. Transportprüfung über echten
MCP-stdio-Prozess und authentifizierte Bridge mit synthetischem Spielzustand.

Abnahmeplan (Punkte 1/2 am 2026-10-02 bestanden; Eingriff per explizitem MCP-Pausenbefehl):

1. Kleiner Pilot: zwei Stunden auf 1×, 3× und 7×; Uhr und Pause unabhängig nachlesen.
   Laufabschluss ohne dauerndes Polling nachweisen.
2. Absoluter Tageswechsel, gezielter Abbruch und manueller Eingriff.
3. Nur bei bestandenem Pilot längere Läufe. Zwei Wochen nicht allein für einen
   Test real durchsimulieren, wenn der zusätzliche Nutzen gering ist.

Erfolg: Ziel erreicht, Pause bestätigt, nachvollziehbare Überschreitung, keine
unbeabsichtigte Wiederaufnahme. Bei ausbleibender Pause oder Sessionabweichung
stoppen und Ursache klären. Bauplatzsuche und Wegschutz bleiben Folgefeatures.

## Live-Pilot 2026-10-02

Geladene Entwicklungskolonie, Bridge 0.22.0; ausschließlich MCP-Aufrufe.
Drei relative Läufe ohne Zwischen-Polling bis zur ersten Ergebnisabfrage:

| Spielstufe | Angeforderte Spielzeit | Beobachtet | Überschreitung | Endgeschwindigkeit |
| --- | --- | --- | --- | --- |
| 1× | 2 Stunden | 2 Stunden | 0 | 0 |
| 3× | 2 Stunden | 2 Stunden | 0 | 0 |
| 7× | 2 Stunden | 2 Stunden | 0 | 0 |

Laufstatus und unabhängige Spieluhr-Rückabfrage bestätigen die Pause. Der erste
Lauf überschritt eine Tagesgrenze. Gemessene Laufzeiten ungefähr 37,9 / 13,7 / 6,0
Echtzeitsekunden; keine Zusage konstanter Tickleistung oder dauerhaft exakter Zieltreffer.
Ein angeforderter Zwei-Tage-Lauf wurde gezielt nach 0,0625 Spielstunden abgebrochen:
`cancelled`, Pause unabhängig bestätigt. Keine langen Zwei-Wochen-Livetests.

Absolutes Ziel am Folgetag 00:15 Uhr: 20,0625 Spielstunden vergangen,
`completed`, Ziel exakt erreicht und Pause unabhängig bestätigt. Ein weiterer
Zwei-Tage-Auftrag wurde durch `set_simulation_speed(0)` unterbrochen:
`interrupted` / `explicit_speed_change`; aktuelle Pause separat bestätigt.
Insgesamt 26 fachliche MCP-Aufrufe in zwei begrenzten Testabschnitten.
Schlusszustand: pausiert. Keine Bau-/Wirtschaftsaktionen.

Noch nicht separat live geprüft: UI-Eingriff, Client-Disconnect, eingefrorene
Anwendung, reale Langläufe über zwei Wochen. Diese Grenzen nicht mit dem bestandenen
begrenzten Pilot gleichsetzen; synthetische Tests belegen die übrigen Zustandsübergänge.
