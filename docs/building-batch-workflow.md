# Bauchargen: begrenzter Live-Pilot bestanden

## Bauchargen-Frühabschluss live bestanden (2026-10-10)

Menschliches Entwickler-Gate durch anschließende Bereitmeldung bestätigt;
Bridge 0.36.0 erreichbar. Zwei kleine Lager in einer Charge ohne neue Wege:
Frühpause nach 4 und 3 statt jeweils 24 Spielstunden. Derselbe advance-Aufruf
schloss beide Bauten sequenziell ab; frische Einzelabfragen bestätigten
finished_accessible, freien Eingang, erhaltene Karotten-/accept-Konfiguration
und Pause. Laufkennungen und ursprüngliche Zeitobergrenzen blieben unverändert.
Identischer start wiederholte nur den terminalen Checkpoint, ohne Doppelbauten.

Kontrollfälle: unfertige erste Bank löste keinen Folgeauftrag aus; nach ihrer
Fertigstellung bei 6,90625 Stunden stoppte die nächste Materialprüfung im selben
Aufruf mit materials_missing (nur ein statt zwei benötigten Brettern vorhanden).
Keine zweite Bank beauftragt. Journalwechsel im Livepilot ohne Zugriffsfehler.
Die ursprüngliche externe Ursache des File.Move-Fehlers bleibt unbekannt.

Zwei Holzfällerplätze dienten als austauschbare Testfläche; Testbank wieder
entfernt, zwei Lager bleiben. Endstand Tag 424, 9,875 Uhr, Tempo 0. Keine neuen
Vorlagentypen: kumulative Abdeckung weiterhin 33/157. Keine künstlich erzeugte
verlorene Pausenbestätigung im Livetest; dafür vorbereitete Regressionen im Gate.
Kein Hintergrunddispatcher, Räumzeitfenster weiterhin unverändert.

## Umsetzung und frühere Gate-Diagnose (2026-10-10)

Erster Gateversuch: Build bestanden, ein Unit-Test scheitert beim Ersetzen des
Chargenjournals (UnauthorizedAccessException bei File.Move mit Overwrite).
Korrektur: File.Replace für bestehende Checkpoints, File.Move nur bei Erstanlage.
Kein Delete-then-write und keine Wiederholungen. Atomaren Austausch mit offenem
Snapshot-Leser zusätzlich als Regression ergänzt; anschließend menschliches Gate
und oben dokumentierter Livetest bestanden.

MCP-only-Änderung für Bridge 0.36.0: `advance_building_batch` erkennt fertig
beobachtete Bauobjekte und lässt den zugehörigen Simulationslauf früh pausieren.
Erst bei bestätigter Pause, erneut geprüften Objekten, Gebäudezugang und ggf.
Lagerkonfiguration folgt `finished_accessible` und der nächste Bauauftrag.
`constructionHours` bleibt unveränderte Obergrenze. Ein bewusst beendeter Lauf
kann dabei `cancelled/cancel_requested`, das Bauprojekt trotzdem korrekt
`finished_accessible` melden. Unfertige oder unterbrochene Projekte zählen nicht.

`waitSeconds` begrenzt nur internes Warten; sofort mögliche Übergänge laufen
auch bei 0 im selben Aufruf weiter (maximal 64). Es gibt keinen Hintergrund-
Dispatcher: nach Rückgabe ist erneut `advance` nötig; ohne aktiven Aufruf gilt
weiter die native Zeitobergrenze. Räumzeitfenster bleiben vorerst unverändert.
Einzelprojekt-advance nutzt dieselbe Frühpause; reine inspect-Aufrufe mutieren nie.
Verlorene Pausenbestätigung mit demselben Lauf lesend klären, kein neuer Zeitlauf.

Quell-/Diffprüfung durch Agent; Testausführung über menschliches Gate bestätigt.
Build, Tests und Installation ausschließlich über Entwickler-Gate.
Live-Abnahme: zwei kleine Gebäude mit großzügigem Zeitbudget; erstes muss weit
vor Zeitlimit pausieren und nach Zugangsprüfung zum zweiten wechseln. Kontrollfall:
noch unfertiger Bau/fehlender Zugang darf keinen Folgeauftrag auslösen. Endpause
und unveränderte Budget-/Laufkennung prüfen; bei Abweichung stoppen.

## Live bestanden: Mehrflächen-Charge (2026-10-05)

Menschliches Gate abgeschlossen; fünf installierte Dateien stimmen mit Paket
0.35.4-20261005-181012-9be85db6 überein. Native Vorprüfung: erste 1×1-Wegfläche
ohne Kandidat, zweite 2×2-Fläche mit zwei Kandidaten. Charge wechselte automatisch
auf regionIndex=1, dokumentierte die erste Fläche und baute genau einen Holzfäller
ohne zusätzliche Wege. Frische unabhängige Abschlussabfrage: finished_accessible,
Eingang frei/erreichbar, 24 Stunden exakt, Überschwingen 0, Pause bestätigt.
Identischer start-Aufruf liefert denselben unveränderten Abschluss/Zeitstempel.
Negativkontrolle mit zwei belegten Wegflächen: no_candidate_in_authorized_regions,
finishedCount=0, keine Auswahl, kein Bau-/Räumlauf. Spiel blieb pausiert.

Abnahme gilt für begrenzten automatischen Flächenwechsel und Abschluss/Replay.
Mehrflächen-Räumung, gemeinsame Budgets und verlorene Bestätigungen synthetisch
abgedeckt, in diesem Live-Pilot nicht zusätzlich erzeugt. Keine freie globale
Standortoptimierung. Vorbereitung und frühere offene Gate-Einträge unten historisch.


## Vor menschlichem Gate: mehrere Teilflächen (2026-10-05)

Nach ausdrücklichem Go implementiert: Basisfläche plus optional additionalRegions
mit bis zu drei weiteren Rechtecken (x/y/z/width/height, jeweils maximal 8×8).
Feste Listenreihenfolge, kein Rücksprung und keine freie Kartensuche. Pro Fläche
bisherige Planung und optionale Räumung; nur nach fehlendem Kandidaten bzw. fehlender
räumbarer Vegetation weiter. Offene/unbestätigte Eingriffe, Materialmangel,
Zugangsfehler oder ausgeschöpftes Räumbudget stoppen die gesamte Charge.

Ein gemeinsames maxClearTargets (maximal 64), keine Multiplikation je Fläche.
Worst-Case-Zeitbudget: clearanceHours × Flächenzahl + constructionHours × Gebäude
höchstens 672 Stunden. Individuelle Laufgrenzen unverändert. RegionIndex und
PreviousRegions bleiben gespeichert; Räumlauf-Kennungen je Fläche getrennt,
Baukennungen weiterhin je Gebäude. Bei Flächenwechsel keine erneute Einreichung
bereits gestarteter Aufträge. Fertignachweise enthalten die Teilfläche.

Alte Einflächenparameter serialisieren unverändert (additionalRegions bei null
weggelassen); alte Fingerprints und Räumkennungen bleiben gültig. Bestehende
terminale Chargen werden nicht nachträglich erweitert: neue Liste braucht neue ID.
Fünf Feature-Dateien syntaktisch geprüft; Regressionen ergänzt, aber noch nicht
ausgeführt. Keine Modänderung, Bridge bleibt 0.35.4. Kein Commit vor Gate/Livetest.

Abnahme nach menschlichem Skript: erste kleine Fläche ohne Kandidat, zweite
mit vorher nativ geprüftem Bauplatz. Eine Charge muss selbst wechseln und ein
Gebäude bis finished_accessible bauen; inspect/erneuter Start mit gleicher ID
darf keinen weiteren Auftrag erzeugen. Negativkontrolle: nur ungeeignete Flächen
stoppen begrenzt. Bei falscher Kontrollprobe oder unbestätigtem Eingriff stoppen.


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
