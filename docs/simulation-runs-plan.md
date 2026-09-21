# Zukunftsfeature: Simulation nach Spielzeit laufen lassen

Status: **geplant, nicht implementiert**. Nutzerwunsch vom 2026-09-21.
Ergänzt die vorhandene [Geschwindigkeitssteuerung](simulation-control.md).

## Zweck und gewünschte Aufrufe

Der Agent soll eine Laufdauer oder einen Zielzeitpunkt angeben können, ohne
Echtzeit-Sleeps, Geschwindigkeitsumrechnung und Pausenschleifen selbst zu bauen.

- Relativ: „Lass die Simulation zwei Ingame-Stunden / zwei Tage / zwei Wochen laufen.“
- Absolut: „Lass die Simulation bis Spieltag 80, 06:00 Uhr laufen.“
- Vorgeschlagene MCP-Namen: `run_simulation_for(duration, unit, speed, session)`
  und `run_simulation_until(dayNumber, hour, speed, session)`. Namen und endgültige
  Schemas werden vor der Umsetzung festgelegt; dies sind keine vorhandenen Tools.
- Einheiten explizit: Ingame-Stunden, Ingame-Tage und Wochen als sieben Ingame-Tage.
  Stunden/Tagesgrenzen anhand der Spieluhr definieren und prüfen. Absolute Ziele
  verwenden den fortlaufenden Spieltag, nicht einen mehrdeutigen Tag innerhalb
  eines Zyklus. Vergangene Ziele ablehnen, bereits erreichte Ziele ohne Lauf melden.

## Gewünschtes Verhalten

- Reguläre Simulation auf einer unterstützten Spielstufe 1/3/7 starten und beim
  Erreichen des Ziels automatisch pausieren. Keine Zeitsprünge oder simulierten Erträge.
- Tatsächliche Spieluhr beobachten; Echtzeit und gewählte Geschwindigkeit sind kein
  Ersatz für verstrichene Ingame-Zeit. Auch Tages-/Zykluswechsel korrekt behandeln.
- Zielprüfung möglichst in der Mod am Spielupdate verankern, damit die Präzision
  nicht von Agenten-Polling oder langsamen MCP-Antworten abhängt. Genauigkeit und
  unvermeidbare Überschreitung durch einen Simulationsschritt explizit ausweisen.
- Lauf-ID und Status mit Startzeit, Zielzeit, tatsächlich erreichter Zeit, vergangener
  Spielzeit, beobachteter Endgeschwindigkeit und Abschluss-/Abbruchgrund liefern.
  „Ziel erreicht“ erst nach bestätigter Zielzeit und bestätigter Pause melden.
- Lange Läufe als abfragbaren und abbrechbaren Auftrag unterstützen; MCP-Aufrufe
  müssen nicht über Tage/Wochen Ingame-Zeit offen bleiben. Wiederholte Statusabfragen
  dürfen keinen zweiten Lauf auslösen; konkurrierende Läufe eindeutig behandeln.
- Manuelle Pause/Geschwindigkeitsänderung, Spielsperre, fehlenden Zeitfortschritt,
  Szenen-/Sessionwechsel und Server-/Clientabbruch ausdrücklich behandeln. Ein
  manueller Eingriff darf nicht durch automatisches Wiederanlaufen übergangen werden.
  Verhalten bei Verbindungsabbruch und eine begrenzte Echtzeit-Abbruchfrist vor
  Umsetzung festlegen; keine unbeaufsichtigte Endlosschleife.
- Bestehendes Opt-in, Sessionprüfung und Ingame-Aktionslog mit kurzer Aktionsbegründung
  weiterverwenden. Optionale fachliche Stoppbedingungen wie Hunger oder Vorratsgrenzen
  sind eine spätere Erweiterung; der Grundauftrag ist verlässliche Spielzeitsteuerung.

## Geplante Abnahme

- Zwei Ingame-Stunden auf jeder regulären Geschwindigkeit; tatsächlichen Zeitfortschritt
  und Pause prüfen, auch bei schwankender Tickleistung.
- Zwei Tage und zwei Wochen sowie absolutes Ziel über Tages-/Zyklusgrenzen prüfen.
- Bereits erreichtes/vergangenes Ziel, ungültige Dauer, paralleler Auftrag, manueller
  Eingriff, Abbruch, Zeitstillstand und Sessionwechsel gezielt prüfen.
- Ergebnis inklusive Zeitüberschreitung und Endgeschwindigkeit unabhängig zurücklesen;
  keine Erfolgsmeldung allein aufgrund abgelaufener Echtzeit.

Öffentliche Spielzeit- und Geschwindigkeitszugriffe sind bereits für die bestehende
Steuerung belegt. Dauerauftrag, Update-Lebenszyklus und Abbruchverhalten sind damit
noch nicht implementiert oder live nachgewiesen.
