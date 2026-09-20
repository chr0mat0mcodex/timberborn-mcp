# Simulation lesen und steuern — 0.11.0

- [x] Pause und alle drei regulären Spielstufen implementiert: 0, 1, 3, 7.
- [x] Ziel und expectedSpeed auf diese vier Werte begrenzt; Session-/Istwertschutz erhalten.
- [x] 200 automatisierte Tests bestanden, Mod 0.11.0 gebaut und installiert.
- [x] Alle vier Zustände 0/1/3/7 in 0.11.0 live bestätigt; Schlusszustand 1×.

`set_simulation_speed(speed, expectedSpeed, session)` nutzt die tatsächlichen
Geschwindigkeitswerte: Pause=0, erste Stufe=1, zweite Stufe=3, dritte Stufe=7.
Bestehende Freigaben enableSpeedControl/TIMBERBORN_ENABLE_SPEED_CONTROL bleiben.
Keine Debuggeschwindigkeiten, Zeitsprünge oder Entsperrung von Spielsperren.
Die Wirkung ist verzögert: immer inspect_simulation separat nachlesen.

Quelle der Standardwerte: vom Hersteller mitgeliefertes Modding/UI.zip,
Views/Game/SpeedControlPanel.uxml in Timberborn 1.1.2.4. Darin heißen die vier
Buttons Speed0, Speed1, Speed3, Speed7; zugehörige Klassen speed-button--0 bis --3.
Nur als Daten gelesen, keine UI-Automation, keine Spielassets ins Repository kopiert.

Live-Abnahme: aktuellen Zustand lesen, jede Stufe höchstens einmal gezielt setzen,
Geschwindigkeit und Zeitfortschritt nachlesen, zum Schluss gewünschte 1× herstellen.
Keine feste Echtzeit-Beschleunigungsquote verlangen: Tickleistung kann begrenzen.
Kein automatisches Retry nach Fehler oder abweichendem Zustand.

## Historischer Pause-/Normalgeschwindigkeitspilot 0.8.0


Status: implementiert, installiert und Live-Abnahme bestanden. Eigene Mod,
öffentliche Spiel-API, keine Fremdmod und keine UI-Automation.

## Werkzeuge

- `inspect_simulation()` liest `SpeedManager.CurrentSpeed`, `IDayNightCycle.DayNumber`,
  `DayProgress` und `HoursPassedToday`. Geschwindigkeit ist ein Spielzustandswert,
  keine gemessene Tickrate. Menüs/Spielsperren können tatsächlichen Fortschritt verhindern.
- `set_simulation_speed(speed, expectedSpeed, session)` akzeptiert zunächst 0
  (Pause) und 1 (Normalgeschwindigkeit), ebenso für expectedSpeed. Bridge vergleicht
  auf dem Spielthread Session und Istgeschwindigkeit vor `SpeedManager.ChangeSpeed`.
  Bei abweichendem Zustand keine Aktion. Andere Geschwindigkeiten bleiben lesbar,
  werden aber in diesem Pilot nicht überschrieben. Höhere Stufen benötigen zunächst
  belegte Standardwerte und einen eigenen Live-Abgleich.

Eigenes Opt-in `enableSpeedControl: true` in der privaten Mod-Konfiguration und
`TIMBERBORN_ENABLE_SPEED_CONTROL=1` im MCP-Prozess. Alle Standardstart-/Prüfskripte
lassen Aktionen aus. HTTP: GET `/agent-api/v1/simulation`, bodyloses POST
`/agent-api/v1/simulation-speed?speed=0&expectedSpeed=1&session=<uuid>`.
Authentifizierung, Loopback, Origin-Verbot und Mainthread-Queue bleiben bestehen.

Die Quittung enthält requestedSpeed, previousSpeed, matchedImmediately und eine
unmittelbare Beobachtung. Auch bei false oder Fehler kein automatischer Wiederholungsversuch;
anschließend inspect_simulation lesen. ChangeSpeed kann erst in einem späteren
Spielupdate sichtbar werden. Keine Aufrufe von ChangeSpeedScale, ChangeAndLockSpeed,
UnlockSpeed, Zeitsprüngen oder privaten Interna. Keine Änderung der Bausitzungssperre.

## API-Beleg und Grenzen

Öffentliche Metadaten der lokalen Timberborn-1.1.2.4-Assemblies:
`Timberborn.TimeSystem.SpeedManager` mit CurrentSpeed und ChangeSpeed(float),
öffentliches IDayNightCycle für die vier Zeitwerte. TimeSpeedButton.TimeSpeed ist
öffentlich, die Standardstufen konnten daraus nicht zuverlässig abgeleitet werden.
Deshalb zunächst explizite Pause/Normalgeschwindigkeit statt erratener höherer Werte.
Die etablierte Game-Kontext-DI und öffentliche Update-Queue werden weiterverwendet.

## Begrenzte Live-Abnahme nach Installation

1. Version 0.8.0, Session und laufende Normalgeschwindigkeit lesen.
2. Einmal 1 -> 0 mit erwarteter Geschwindigkeit 1; separat Zustand nachlesen.
3. Zwei Zeitbeobachtungen mit kurzem Abstand: Tagesfortschritt muss beim Pausieren
   stillstehen. MCP muss auch in Pause antworten (UpdateSingleton statt Tick).
4. Einmal 0 -> 1 mit erwartetem Wert 0; separat nachlesen und Zeitfortschritt prüfen.
5. Schlusszustand 1 entspricht der aktuellen Nutzerpräferenz. Bei Fehler nur lesen,
   nicht blind erneut steuern oder eine fremde Spielsperre entsperren.

Tests prüfen Gates, Parametergrenzen, Session-/Quittungskorrelation, Fehler ohne
Retry und einen synthetischen MCP-Hin-/Rückweg.

## Live-Ergebnis 2026-09-20

Pause/1× erfolgreich nachgelesen. Zwei Zeitbeobachtungen mit 2,5 Sekunden Abstand
waren während Pause identisch; danach bei 1× fortschreitend. MCP blieb in Pause
erreichbar. Schlusszustand 1×. Ausgangszustand nach Laden war 0, daher einmaliger
Start vor Hin-/Rücktest: insgesamt drei Befehle, zehn fachliche MCP-Aufrufe.
Alle unmittelbaren Quittungen enthielten noch den alten Geschwindigkeitswert
(matchedImmediately=false); die Änderung wird im folgenden Spielupdate wirksam.
Kein automatisches Retry. Höhere Stufen nicht getestet.

## Vollständige Live-Abnahme 0.11.0 — 2026-09-20

0/1/3/7/1 separat nachgelesen. Pause: keine Zeitänderung. Laufende Stufen zeigen
positive DayProgress-Deltas (rund 0.002604 / 0.007813 / 0.018229 bei kurzem
Messintervall). Wieder 1× bestätigt. Tatsächliche Tickleistung bleibt lastabhängig.
