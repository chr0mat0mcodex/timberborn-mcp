# Simulation lesen und steuern — Pilot 0.8.0

Status: implementiert und installiert; Live-Abnahme ausstehend. Eigene Mod,
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
Retry und einen synthetischen MCP-Hin-/Rückweg. Die reale Geschwindigkeitssemantik
und Erreichbarkeit während Pause bleiben bis zum Live-Test ausdrücklich offen.
