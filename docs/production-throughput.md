# Durchsatz und beobachtete Stillstandszeiten — 0.44.0

Status: menschliches Gate der Korrektur und begrenzter Abschluss-Livepilot
bestanden. Erster Fehlerpilot unten bleibt historische Diagnose.

## Verwendung

Bestehendes `run_simulation_for` mit `buildingIds` (1–8 eindeutige IDs) und
`measureProduction=true` starten. Dauer, Einheit, Tempo, expectedSpeed und
Echtzeitlimit bleiben unverändert. Kein neues Tool und kein eigener Laufdienst.
Der Messmodus läuft bis zur Zeitgrenze, auch wenn alle Gebäude bereits einen
Zyklus abgeschlossen haben. Abschlussgrund `production_measurement_complete`
bedeutet beendetes Zeitfenster, nicht erreichte Produktionsmenge. Automatische
Pause muss weiterhin bestätigt sein. Ohne Option bleibt der Erstzyklusmodus.
Güter-/Forschungs-/Holzziele sind mit Gebäudeauswahl weiterhin ausgeschlossen.

`buildingProduction[].measurement` enthält:

- `completedCycles`: alle beobachteten nativen ProductionFinished-Ereignisse;
  das ältere äußere completedCycles bleibt der kompatible Erstnachweis 0/1.
- `measuredHours`, `cyclesPerHour`: Messdauer in Spielstunden und native
  Abschlüsse je Messstunde; bei null Dauer ist die Rate null, nicht null Erfolg.
- `products`/`sciencePerCycle`: natives Rezept beim Start; `recipeDerivedProducts`
  und `recipeDerivedScience` samt Raten berechnen Ereignisse × Rezeptausstoß.
  Keine Lagerdifferenz, keine Transportmenge, keine ausgelieferte Nettomenge.
- `times`: exklusive beobachtete Spielzeitanteile; Summe = measuredHours.
  producing, paused, staffing, outside_working_hours, ingredients (inklusive
  Brennstoff), storage, power, multiple_blockers und unknown.
- `largestObservedStall`: größter beobachteter Warteanteil, bei Gleichstand
  deterministisch; null ohne beobachtete Wartezeit. Mit aktueller 0.43-Diagnose
  gemeinsam lesen, historische Ursache nicht aus dem aktuellen Zustand erfinden.

## Aussagegrenzen

Native Zyklusereignisse sind der Produktionsbeleg. Produktionsfortschritt alleine
erzeugt weder einen Zyklus noch eine Produktmenge. Bei einem bereits angefangenen
Zyklus zählt dessen Abschluss im Messfenster; für kurze Messungen kann das die
Rate verzerren. Rezeptausstoß ist eine Ableitung, keine unabhängige Messung
eventueller Sonderboni, Forschungskonten oder ausgelieferter Waren.

Zeitanteile werden aus aufeinanderfolgenden Spielthread-Beobachtungen geschätzt:
Fortschrittszuwachs oder Zyklusabschluss weist das Intervall producing zu. Ohne
Fortschritt wird eine an beiden Endpunkten gleiche Blockadenkategorie verwendet.
Mehrere Kategorien ergeben multiple_blockers, wechselnde Kategorien unknown.
Eine Lücke über 0,25 Spielstunden wird vollständig unknown. Bereitschaft ohne
Fortschritt ist unknown; fehlender Arbeiter, Arbeitszeit und Gebäudepause sind
getrennte Kategorien. Keine erfundene Transport-, Wind- oder exakte Stillstandsursache.
Endpunktmessungen können Änderungen innerhalb eines Intervalls nicht erkennen.

Nur verstrichene Spielzeit zählt. Ein Echtzeitwarten bei gleicher Spielzeit addiert
nichts. Bei Abbruch, Tempostörung, Spielsperre oder Beobachtungsfehler kann die
zuletzt akzeptierte Messung kürzer als die gesamte Laufzeit sein: measuredHours
und observedGameHours ausdrücklich beachten. Terminale Messungen werden nicht
nachträglich erweitert; die separat datierte aktuelle Engpassdiagnose bleibt frisch.

Rezept-/Pause-/Lager-/Sollpersonaländerung, Objektverlust und Journallücke behalten
ihre bisherigen Fehler-/Stoppregeln. Für einen Vorher-/Nachhervergleich daher
zwischen zwei Läufen ändern, nicht während einer Messung. Start mit neuer Run-ID;
unbestätigte Starts nur lesen, nicht wiederholen.

## Kosten und Grenzen

Bis acht native Manufakturen, 64 Rezeptproduktzeilen je Gebäude, 28 Spieltage und
die bestehenden Echtzeitgrenzen. Pro Gebäude feste neun Zeitaggregate und ein
Zykluszähler; keine Tickhistorie, kein Inventar-/Wege-Gesamtscan pro Tick.
Erster gemeinsamer Zyklusnachweis und Zeitgrenze gleichen das Journal gezielt ab.
Die teurere 0.43-Endpunktdiagnose bleibt an explizite inspect-Aufrufe gebunden.
Rezepte werden nativ gelesen; kein Fraktionskatalog im Messpfad.

## Vorbereitete Prüfungen und Gate

Automatische Fälle: Mehrfachereignisse, Rezeptmengen/Raten, reine Fortschritts-
beobachtung ohne erfundene Produktion, alle Zeitkategorien, überlappende Blockaden,
wechselnde Zustände, lange Lücken, null Spielzeit, Achtfachlauf über Erstzyklus
hinaus, Nullproduktion bis Zeitgrenze, eingefrorene und kopierte Ergebnisse,
Konfigurations-/Journalfehler, Abbruch, Tempostörung, Echtzeitlimit, Spielsperre,
ungültige Auswahl/Optionen, manipulierte/regressive Berichte und MCP-Transport.
Bekannte Versionsprofile enthalten 0.44; Negativprofil bleibt 99.0.0.
Diese Tests werden ausschließlich im menschlichen Gate ausgeführt.

Livetest nach erfolgreichem Gate:

1. Ein laufender Produzent und ein belegbar blockierter Produzent, gemeinsame
   Messung etwa 3–6 Spielstunden. Mehr als ein nativer Zyklus, passender
   Stillstandsanteil, Zeitlimit und bestätigte Pause sind die Erfolgskriterien.
2. Eine konkrete Blockade zwischen den Läufen beheben und erneut begrenzt messen.
   Änderung der Belege/Zyklusrate vergleichen, keine Durchsatzgarantie aus
   unterschiedlichen Wetter-/Arbeitszeitbedingungen ableiten.
3. Alten Messbericht erneut lesen: unverändert trotz späterer Einstellungen und
   neuer Diagnose. Neunte ID/fehlende Gebäudeauswahl mutationsfrei ablehnen.
4. Bei widersprüchlichen Ereignis-, Mengen- oder Zeitbelegen Pilot stoppen und
   Ursache korrigieren. Kein Ausweiten auf weitere Gebäude oder lange Simulation.

Kein Achtfach-Live- oder Lasttest aus synthetischen Tests behaupten. Teständerungen
anschließend rücklesen; keine Bauten oder Rohstoffe für den Messpilot löschen.

## Erster Livepilot und Korrektur (2026-10-11)

0.44 gestartet, Messoption im aktualisierten Werkzeugkatalog bestätigt. Erfinder
und Papierfabrik gemeinsam exakt drei Spielstunden gemessen. Vier native
Erfinderzyklen, entsprechend vier rezeptabgeleitete Forschungspunkte; Papierfabrik
null Zyklen und drei Stunden storage. Zeitfensterabschluss mit bestätigter Pause.

Fehlerkontrolle: Erfinder nur 0,40625 Stunden producing, 2,59375 unknown trotz
fortgesetzter Produktion. Der Tracker überschrieb Fortschritts-/Zyklusbaselines
auch bei Renderbeobachtungen mit unveränderter Spielzeit. Dadurch ging Fortschritt
für das nächste Zeitintervall verloren. Nun bleibt die Intervallbasis bis zum
Fortschreiten der Spielzeit bestehen; neue Ereignisse werden dennoch sofort
gezählt und rückläufige Zähler abgewiesen. Gezielter Regressionstest vorbereitet.
Zeitanteile dieses ersten Erfinderberichts sind nicht abgenommen.

Pilot nicht ausgeweitet. Keine Einstellungen, Gebäude oder Rohstoffe verändert;
Simulation pausiert. Nach erneutem menschlichem Gate denselben Zweiervergleich
wiederholen, dann Blockadenbehebung und unveränderte Historie nachweisen.

## Abschluss-Livepilot — bestanden (2026-10-11)

Korrigierte Bridge 0.44.0 nach menschlichem Gate gestartet. Jeweils Erfinder und
Papierfabrik gemeinsam drei Spielstunden mit measureProduction=true beobachtet:

| Messung | Erfinderzyklen | Papierzyklen | Papier-Lagerstillstand | Papier-Betriebszeit |
|---|---:|---:|---:|---:|
| Ausgangslager blockiert | 4 | 0 | 3 h | 0 h |
| Zusätzliches Lager nimmt Papier an | 5 | 2 | 0,65625 h | 2 h |

Erfinder-Betriebszeit 2,625 beziehungsweise 2,65625 h; 0,375 beziehungsweise
0,34375 h bleiben unknown. Papier im zweiten Lauf ebenfalls 0,34375 h unknown.
Keine exakte Ursache aus diesen ungeklärten Intervallen behauptet. Alle
Zeitanteile summieren sich auf je drei Stunden. Zwei Papierzyklen × zwei Papier
ergeben vier rezeptabgeleitete Papier und 1,3333 Papier je Messstunde. Separat
beobachtete 20 Papier im Lager stammen aus Transport, nicht aus diesem Zykluswert.

Beide Läufe enden am Zeitfenster mit bestätigter Pause, nicht am Erstzyklus.
Erster buildingProduction-Bericht nach Einstellungen und zweitem Lauf strukturell
identisch. Neunte ID und Messmodus ohne Gebäudeauswahl: invalid_argument;
anschließende Inspektionen bestätigen run_not_found, also keine Laufanlage.
Die pauschale Fehlermeldung nennt trotzdem mögliche Unbestätigtheit: bekannte
Diagnosepräzisionslücke, keine beobachtete Mutation.

Lagerauswahl Carrot/empty wiederhergestellt und separat rückgelesen. Die 20 Papier
bleiben als unerwünschter Bestand für regulären Abtransport erhalten. Keine
Rohstoffe, Gebäude oder Wege gelöscht; Simulation pausiert.

Liveumfang zwei Gebäude und zwei Zeitfenster. Achtfachumfang, weitere Kategorien,
Unterbrechungen und Grenzwerte durch vorbereitete Tests im menschlichen Gate;
kein Achtfach-/Last-/Iron-Teeth-Livenachweis. Version im beschriebenen Umfang
abgenommen; keine langfristige Durchsatzgarantie aus zwei kurzen Fenstern.
