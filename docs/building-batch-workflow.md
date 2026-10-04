# Bauchargen: begrenzter Live-Pilot bestanden

Nach menschlichem Gate begrenzt live abgenommen (siehe PROJECT_STATE.md). MCP-Steuerung über
vorhandene native Bau-, Räum- und Simulationsdienste; keine neue Mod-Schnittstelle.

## Auftrag und Grenzen

`start_building_batch` erhält `batch` und `waitSeconds` (0–45). Die Charge enthält
session, batchId, districtId; x/y/z/width/height; rotations; items mit template
und optional initialStorageGood/initialStorageMode; clearing, maxClearTargets,
clearanceHours, constructionHours, speed und maxRealSeconds.

- 1–8 Gebäude, Fläche höchstens 8×8, erlaubte Rotationen 0–3. Die Fläche muss
  vollständige Gebäudegrundrisse und Weganschlüsse erlauben. Ebene Projekte mit
  vorhandenen nativen Zugangsprüfungen; keine automatische Höhenplanung.
- Erst ohne Räumung suchen, Kandidaten nach Anzahl neuer Wege priorisieren.
  Ohne Kandidaten darf einmal die gesamte passende natürliche Vegetation im
  freigegebenen Bereich geräumt werden: none / dead_vegetation / all_vegetation.
  Höchstens 64 Ziele; keine minimale Räumfläche behauptet. Gebäude, gepflanzte
  Kulturen und Trümmer werden nicht automatisch entfernt.
- Räumung und jedes Gebäude erhalten jeweils 1–168 Spielstunden, zusammen
  höchstens 672. Pro Lauf 30–3600 reale Sekunden, Geschwindigkeit 1/3/7.
  Keine automatische Verlängerung, Forschung oder Materialproduktion.
- Erst nach `finished_accessible` folgt das nächste Gebäude. Das umfasst alle
  Bauobjekte, Eingang, optionale Lagerkonfiguration und aktuelle Pause, aber
  keinen Produktions- oder Zufriedenheitsbeleg.

`advance_building_batch` erhält session/batchId/waitSeconds und führt höchstens
64 Zustandsübergänge pro Aufruf aus. Interne native Aufrufe bleiben notwendig;
eingespart werden Modellentscheidungen und manuelle Zwischenaufträge.
Die Wartezeit begrenzt Statuswarten, nicht die Laufzeit einzelner Bridge-Aufrufe.
Zwischen MCP-Aufrufen gibt es keinen Hintergrunddispatcher. Nur ein bereits
gestartetes natives Zeitfenster läuft bis zu seiner automatischen Pause weiter.

`inspect_building_batch` liest den gespeicherten Status mit Zeitstempel und
checkpointOnly=true. Das ist keine frische Spielabfrage. `stop_building_batch`
stoppt weitere Aufträge dauerhaft; laufende Zeitfenster behalten ihr Budget.

## Wiederaufnahme

Privates Journal unter `.local/build-batches` neben dem laufenden MCP-Programm,
aus Git ausgeschlossen. Atomarer Dateiaustausch, exklusiver Prozesslock und
vor jedem Eingriff gespeicherter Zustand. Unbestätigte Eingriffe werden nach
Neustart ausschließlich lesend geklärt, niemals blind erneut gesendet.
Gleiche Startparameter/batchId liefern nur den gespeicherten Status; Änderungen
werden abgelehnt. Fortsetzung explizit über advance. Eine aktive Charge pro
Session. Sessionwechsel oder fehlendes Journal erlauben keine Wiederaufnahme
alter Eingriffe. Journal deshalb bei einer MCP-Verlagerung erhalten.

## Gezielte Abnahme nach menschlichem Skriptlauf

1. Zwei kleine unterstützte Gebäude mit ausreichend Material in einer begrenzten
   Region. Mindestens ein Räumziel benötigt einen Arbeiterauftrag. Automatische
   Standortwahl, Räumwartezeit, sequenzielle Fertigstellung und Abschlusszugang
   separat im Spiel nachweisen; Lagerkonfiguration bereits an Baustelle prüfen.
2. Identischen Start wiederholen und MCP-Wiederaufnahme prüfen: unveränderte
   Auftrags-/Lauf-IDs, keine Doppelbauten oder zusätzliche Zeitfenster. Geändertes
   Budget wird abgelehnt. Abgeschlossene Charge bleibt unverändert.
3. Ein negativer Kontrollfall: fehlendes Material oder zu kleines Räumbudget
   stoppt die Charge, Folgegebäude wird nicht beauftragt.

Erfolg: zwei nacheinander fertige, erreichbar nachgewiesene Gebäude ohne einzelne
Modellentscheidungen zu Standort, Räumzielen oder Baustart; höchstens zehn
Chargenaufrufe als Effizienzpilot, unabhängige Kontrollabfragen separat zählen.
Stopp: Kontrollfall falsch, unbestätigter Eingriff, fehlender Zugang oder Budget
erschöpft. Dann diagnostizieren, nicht weitere Fälle anhängen. Erst nach Gate
und bestandenem Livetest committen/pushen.
