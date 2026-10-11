# Gebäudebezogener Produktionsnachweis — 0.41.0

Status: menschliches Test-Gate und begrenzter Livepilot am 2026-10-11 bestanden.
Erweitert den bestehenden `run_simulation_for`, keinen zweiten Laufcontroller.

## Vertrag

- Optional `buildingIds`: ein bis acht unterschiedliche IDs aus der aktuellen
  Sitzung. Exklusiv zu `goodTargets`, `stopAtScience` und `stopAtAvailableLogs`.
- Nur fertige Gebäude mit nativer `Manufactory` und gewähltem Rezept. Auswahl
  vor Beschleunigung prüfen; unbekannte Gebäude oder fehlendes Rezept ablehnen.
  Keine Liste fest codierter Folktails-Rezepte: das native aktuelle Rezept zählt.
- Je Gebäude ein nach Start beobachtetes öffentliches
  `Manufactory.ProductionFinished`-Ereignis. `RecipeChanged` invalidiert direkt.
  Beide öffentlichen Ereignisse haben den Handler-Typ `System.EventHandler`
  (lokale Assembly-Metadaten geprüft). Kein privater Zugriff erforderlich.
- `completedCycles` ist ein auf 1 begrenzter Erstnachweis, kein Durchsatzzähler.
  Ein vor Start begonnener Zyklus darf nach Start fertig werden. Der Nachweis
  behauptet weder eine vollständig im Zeitfenster gestartete Produktion noch
  Nettoertrag, Lieferleistung oder eine durchgehend stabile Versorgung.
- Alle ausgewählten Gebäude müssen nachgewiesen sein:
  `buildingProductionReached=true`, `reason=building_production_proven`.
  Erst terminaler Abschluss mit bestätigter Pause ist ein fertiger Lauf.
- `duration` bleibt Spielzeitobergrenze, `maxRealSeconds` Echtzeitobergrenze.
  `building_production_time_limit`/`targetReached` bedeutet nur Zeitablauf;
  vorhandene Teilnachweise bleiben erhalten. Fehlender Gesamtnachweis ist kein
  Beleg, dass ein Gebäude grundsätzlich nicht produzieren kann.

## Diagnose und Eingriffe

`buildingProduction` liefert je ID Vorlage, Startrezept, Erstnachweiszeit,
Beobachtungszeit sowie aktuelle `blockers` und historische `seenBlockers`.
Beobachtbare Gründe: Pause, fehlende Besetzung, außerhalb der Arbeitszeit,
Zutaten/Brennstoff/Ausgabekapazität und inaktive/unversorgte/unbekannte Kraft.
Diese Signale sind Beobachtungen, keine vollständige Ursachenanalyse.
Ab Abschluss bzw. begonnener Pause wird der Bericht eingefroren.

Rezeptwechsel, protokollierte Pause-/Lager-/Sollpersonaländerungen an Zielgebäuden,
Objektverlust, unbekannte Beobachtung oder Journallücke stoppen mit Fehler und
angeforderter Pause. Änderungen werden nicht zurückgesetzt. Manuelle Tempoänderung
unterbricht wie bisher und wird nicht überschrieben. Sessionwechsel beendet die
Beobachtung; keine Wiederaufnahme alter Läufe.

Das Journal wird mit Cursor innerhalb der Mod gelesen (bis 512 Einträge), ohne
zusätzliche MCP-Aufrufe. Ereignisse hängen nur an den höchstens acht ausgewählten
Manufakturen. Normale Diagnose liest deren Komponenten, keine erneute globale
Gebäudesuche. Vor Gesamterfolg erfolgt ein Journalabgleich. Für Zustandsunterschiede
gilt weiterhin die Journalgrenze: kurze Änderung mit Rücknahme zwischen zwei
Abgleichen kann unsichtbar bleiben; das ist keine lückenlose Eingriffserkennung.

## Agentenablauf

1. Mit Gebäudesuche/Betriebssammelabfrage bis acht fertige Manufakturen auswählen;
   Rezept, Personal und Kraft vorher einstellen.
2. Einen `run_simulation_for` mit aktueller Session, neuer Run-ID, `buildingIds`,
   begrenzter Dauer, erwarteter Geschwindigkeit und Echtzeitgrenze starten.
3. Mod selbst laufen lassen. Keine Aufrufe je Gebäude, keine enge Statuspollschleife.
   Nach unbestätigtem Start nur dieselbe Run-ID lesen; nicht erneut starten.
4. Mit `inspect_simulation_run` Ergebnis und bestätigte Pause prüfen. Teilnachweise,
   Zeitlimit und Blockaden getrennt berichten. Änderungen erfordern einen neuen
   begrenzten Lauf, keinen nachträglich umgedeuteten alten Nachweis.

## Gate und kleiner Livepilot (bestanden)

Automatische Tests für Auswahlgrenze/Mischverbote, Achtfachnachweis, Teilnachweis,
Beobachtungsverlust, Eingriffe, Zeitgrenze mit Spielsperre, eingefrorene Ergebnisse,
Transportvertrag und Journalfenster im menschlichen Gate enthalten.

Abgenommener Pilot nach menschlichem Skriptlauf:

1. Zwei erreichbare, eingerichtete Manufakturen mit kurzen Rezepten gemeinsam
   nachweisen; Erstereignisse, terminalen Erfolg und Tempo 0 prüfen.
2. Einen blockierten Betrieb kurz laufen lassen: passender beobachteter Grund,
   Zeitende ohne erfundenen Erfolg und historische Diagnose nach erneutem Lesen.
3. Konfiguration eines laufenden Ziels ändern: Fehlerabschluss und Pause; Eingriff
   bleibt bestehen. Unbestätigte Starts nicht wiederholen.

Bei fehlerhaftem Kontrollfall abbrechen und gezielt diagnostizieren. Keine
stundenlange Koloniepflege für diesen Pilot. Achtfachgrenze zunächst automatisch,
Fraktionsbreite und Sonderproduzenten erst bei eigenem tatsächlichem Nachweis.
Commit/Push erst nach erfolgreichem menschlichem Gate und diesem Livepilot.

## Live-Ergebnis 2026-10-11

- Grill und Bäckerei gemeinsam: je ein natives Abschlussereignis für
  GrilledSpadderdock und Bread; `building_production_proven`, Gesamtnachweis wahr,
  terminal und Pause bestätigt. 0,46875 Spielstunden / 1,363 Sekunden Echtzeit.
- Bereits pausiertes Sägewerk: nach genau einer Spielstunde
  `building_production_time_limit`, null Zyklen, Gesamtnachweis falsch.
  Pause, fehlendes Personal, fehlende Zutaten und inaktiver Verbraucher ausgewiesen.
- Papierfabrik während des Laufs pausiert: `building_configuration_changed`,
  Fehlerabschluss mit bestätigter Pause nach 0,125 Spielstunden / 2,689 Sekunden.
  Historie bewahrt die vorherige fehlende Kraft neben den neuen Pausesignalen.
- Ersten Positivbericht nach beiden Folgeläufen erneut gelesen: Ergebnis,
  Zeitstempel und Gebäudebelege unverändert.
- Papierfabrik wieder aktiviert und separat rückgelesen; Endtempo unabhängig 0
  bestätigt. Keine Testbauten, Wege oder Rohstofflöschungen erforderlich.

Live wurden zwei Gebäude gemeinsam nachgewiesen. Achtfachgrenze im automatischen
Gate, nicht als Achtfach-Liveproduktion belegt. IronTeeth, Objektverlust,
Rezeptwechselereignis und Journallücke nicht zusätzlich live nachgestellt.
Der aktualisierte Werkzeugparameter wurde nach einmaligem Codex-Neustart sichtbar;
kein erneuter Build nötig. Feature-Checkpoint kann jetzt veröffentlicht werden.
