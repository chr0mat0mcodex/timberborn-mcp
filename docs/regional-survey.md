# Regionale Flächensuche — begrenzt live abgenommen

## Erweiterung live abgenommen (2026-10-10)

Menschlicher Release-Gate-Lauf einschließlich Tests/Installation bestanden.
Sägewerksfeld (31,4), 8×8, Start z=3, maxHeights=2/maxWindows=1: Ebene z=4
automatisch gefunden, Rampe lokal als o, beide Ebenen ohne unknown und ohne
Legacy-Verfeinerungsreads. 33 native Reads und vier Planungen statt Einzelabfragen;
vier obere Lagerkandidaten. Lager bei (32,8,4) mit nativer Vorschau gültig,
Eingang angeschlossen, keine neuen Wege.

Hinter Hauptsiedlung (30,24), 16×16, z=4, maxHeights=1/maxWindows=2:
28 native Reads, acht Planungen, zwei nicht überlappende 8×8-Fenster.
Raster zeigt teilweise/vollständig/ungeprüft (p/c/u) getrennt; elf andere beobachtete
Höhen ausdrücklich nicht auf Hindernisse untersucht. Lager bei (35,28,4)
gültig und angeschlossen. Negativkontrolle Lodge (33,27,4) korrekt ungültig,
obwohl Vorschau-Eingang angeschlossen. Alle Vorschauen wiederhergestellt,
keine Sitzungssperre, keine Bau-/Räumaufträge. Simulation durchgehend pausiert.

Dies belegt Flächensuche, Höhenfolge, begrenzte Suchabdeckung und Spielvorschau,
nicht tatsächlichen Bauarbeiterzugang oder fertigen Gebäudezugang. Legacy-Fallback
und Maximalbudgets sind synthetisch geprüft, nicht zusätzlich live skaliert.
Die nachfolgenden Gate-Abschnitte halten den vorherigen Entwicklungsstand fest.

## Regionalsuche erweitert — Entwickler-Test-Gate offen (2026-10-10)

Auftrag: Rampenblindfleck vollständig beheben, Höhen und ungeprüfte Bereiche
berücksichtigen. Quellcode vorbereitet; keine Agent-Builds oder Testausführung.

- Native Mod liefert overlapCells aus öffentlichem PositionedBlocks.GetOccupiedCoordinates,
  auf den Abfragequader begrenzt (maximal 8×8×4 Zellen), ohne Namensheuristik oder
  Reflection. Backend prüft Nichtleere, eindeutige Zellen und Abfragegrenzen.
- RegionSurvey vereinigt die Teilmasken je Objekt. Unknown betrifft bei vorhandener
  Geometrie nur echte Objektzellen: o kennzeichnet nachgewiesene Belegung durch
  andere Objekte (Rampe/Ruine), nicht fehlende Geometrie. Vegetation/Schutt ebenfalls mit Überlappungszellen
  statt allein dem Ursprung. Die Rampe wird nicht als frei erklärt. Kein Extra-Read
  zur Verfeinerung bei vollständiger neuer Geometrie.
- Kompatibilität für ältere Bridge ohne Masken: native Überlappungsabfragen rekursiv
  verkleinern, maximal 64 Zusatzreads je Höhe. Nur eine vollständige Abfrage ohne
  unbekannte Überlappung entlastet Zellen. Rest bleibt unknown, Budgetflag sichtbar.
- scanOtherHeights=true als Standard für Bausuche: neben angefragtem z weitere
  beobachtete Gelände-/Weghöhen untersuchen, Wege bevorzugt. maxHeights=4 inklusive
  Ausgangshöhe, einstellbar 1–8. additionalLevels enthält eigene Raster, Feuchte,
  Kandidaten und Suchabdeckung. Keine neue vertikale Wegplanung.
- maxWindows=3 je Höhe, einstellbar 1–9. Fenster bevorzugen noch nicht erfasste
  Fläche statt immer dieselben hoch bewerteten Überlappungen; vier Drehungen je
  Fenster. Höchstens maxHeights×maxWindows×4 Planungen, im Maximum 288. Standard
  höchstens 48, auf einer flachen Ebene weiterhin höchstens zwölf. Keine globale
  Vollsuche behauptet; Kandidatenliste bleibt auf vier je Höhe begrenzt.
- coverage.heights: Geländeanzahl, beobachtete Wegzellen und ObstaclesInspected.
  Nicht untersuchte Höhen bleiben explizit false. planningRows: u=ungeprüft,
  p=teilweise (z.B. Optionslimit), c=alle vier Drehungen mit vollständiger
  Ursprungssuche in abdeckenden Fenstern. Das ist keine Platzierungsvalidierung
  und kein Nachweis aller denkbaren Anschlussrouten über Fenstergrenzen hinweg.
- planBuildings=false und workBuildingId bleiben auf der angefragten Ebene;
  keine unnötigen zusätzlichen Höhensuchen bei Landwirtschaftsdiagnose.
- Bestehende Versionsnummer 0.36.0, additive optionale Bridge-Felder; Paketstand
  statt Versionsnummer allein beachten. Neues Modpaket am menschlichen Gate nötig.

Regressionen vorbereitet: lokale Rampenüberlappung über Kachelgrenzen, gültige/
fehlerhafte native Masken, Legacy-Verfeinerung und Budgetende, veränderte Objekte,
zusätzliche Höhen mit Wegpriorität und ausgelassenen Ebenen, Sitzungswechsel auf
oberer Ebene, vollständige versus abgeschnittene Suche und breitere Fensterabdeckung.

Live-Gate nach Skript und Bereitmeldung: Sägewerksfeld bei z=4 darf wegen Slope
nicht mehr fast vollständig unbekannt werden und muss Lager-/Lodge-Kandidaten
liefern; belegte Rampenzellen bleiben ausgeschlossen. Dasselbe Gebiet ab z=3
aufrufen: z=4 mit Wegen muss in additionalLevels auftauchen. Hinter Hauptsiedlung
freie Nachbarn auf z=4 erkennen; Limits/ausgelassene Ebenen sichtbar prüfen.
Je ein gültiger Kandidat pro Gebiet frisch validieren, den bekannten abgelehnten
Lodge-Kandidaten als Negativkontrolle verwenden. Keine pauschale Baufreigabe.
Bei falschem Kontrollfall stoppen. Commit/Push erst nach Gate und Livetest.


## Systematische Gegenprüfung der blinden Flecken (2026-10-10)

Bridge 0.36.0, strukturierte Liveabfragen; keine Bauten oder Räumungen.
Gebiete über Rasterkoordinaten und Geländeebenen definiert, nicht Kamerarichtung.
Vollständiger Gebäude-/Wegebestand gelesen; Sägewerk bei (26,7,3), Hauptdistrikt
bei (29,30,3). Höhenstichproben in fünf 8×8-Feldern, anschließend flächige
16×16-Raster und gezielte Anschlussfelder. Kein vollständiger Kartenatlas.

| Gebiet und abgedeckte Ebene | Beobachtung | Gezielter Baunachweis |
| --- | --- | --- |
| Hinter Hauptsiedlung: x=30–45, y=24–39, z=4 | 20 freie Rasterzellen, zehn Vegetationszellen, sechs Wege; 207 Zellen andere Höhe. Schmale nutzbare Terrasse. Wege (34,28,4) und (34,33,4) nativ distriktverbunden. | Kleines Lager (35,28,4), Drehung 1: Spielvorschau gültig, Eingang angeschlossen, kein neuer Weg. |
| Höherer Hang im selben Rechteck, z=8 | 192 unbekannte Zellen, sieben Vegetationszellen, 57 andere Höhe. Keine freie Fläche zuverlässig ausgewiesen. | Nicht als ungeeignet klassifiziert. Hindernisse/weitere Höhen und Zugang bleiben offen. |
| Neben Sägewerk: x=27–42, y=0–15, z=4 | 50 freie, 70 Vegetations-, 125 unbekannte Zellen, zwei Wege, acht andere Höhe, eine Gebäudezelle. Wege (32,7,4) und (33,7,4) nativ distriktverbunden. Ausgewiesene freie Flächen trocken. | Kleines Lager (32,8,4), Drehung 0 und Lodge (31,8,4), Drehung 0: Spielvorschau gültig, Eingang angeschlossen, jeweils kein neuer Weg. Alternative Belegungen, nicht gleichzeitig zugesichert. |

Rasterzahlen sind Klassifikationen auf genau einer Höhe, keine Anzahl sicherer
Bauplätze. Zusätzliche Umgebungssichtung beim Sägewerk: x=19–34, y=0–15, z=3.
Ein weiterer z=3-Aufruf wurde nach zwischenzeitlichem Tempo-Wechsel abgewiesen;
keine vollständige zusätzliche z=3-Abdeckung behauptet. Die Ursache des Wechsels
auf Tempo 7 wurde nicht ermittelt. Für weitere Prüfungen explizit pausiert und
nachgelesen; Endzustand Tag 424, 21,375 Uhr, Tempo 0.

Gegenprobe größerer Grundriss: vier Drehungen einer Lodge in jedem Anschlussfeld
(x=32–37/y=24–31/z=4 und x=31–38/y=4–11/z=4). Hinter der Siedlung meldete der
Planer Kandidaten, aber Lodge (33,27,4), Drehung 3 scheiterte an der nativen
Platzierungsprüfung. Ursache durch aktuelle Diagnose nicht genauer belegt.
Beim Sägewerk bestand der oben genannte Lodge-Kandidat. Vier gemeinsame
Vorschauprüfungen insgesamt: drei gültig, eine abgelehnt; jeweils keine bleibende
Änderung beobachtet und Vorschauzustand wiederhergestellt. Kandidatenlisten teils
am Optionslimit gekürzt; keine vollständige Aufzählung aller Bauplätze.
Alle Vorschauen behalten die bekannte Bauphasennachweislücke: kein tatsächlicher
Bauarbeiter-, Liefer-, Fertigbau- oder Betriebsnachweis, keine pauschale Baufreigabe.

### Konkrete Ursache im MCP

Im Sägewerksfeld x=31–38/y=4–11/z=4 lieferte survey_region 62 unbekannte Zellen,
zwei Wege, keine Suchfenster und planningCalls=0. Direkte Planung im gleichen
Feld fand dagegen kleine Lagerplätze und den später gültigen Lodge-Kandidaten.
Die vollständige Hindernisabfrage enthielt eine natürliche Slope bei (31,7,3),
Kategorie other, die in die obere Ebene hineinragt. RegionSurvey.Observe setzt
bei einem unbekannten Hindernisgrundriss vorsorglich die gesamte betroffene
8×8-Kachel auf uncertain. Dadurch verschwinden auch tatsächlich prüfbare
Nachbarflächen aus der automatischen Fensterauswahl. Das ist eine belegte
Informations-/Suchlücke, keine falsche Behauptung der Spielvalidierung.
Die 192 unbekannten Hangzellen sind nicht einzeln auf dieselbe Ursache untersucht.

### Vorgehen gegen erneute blinde Flecken

1. Vor Ausbau zunächst alle vorhandenen Wege und Geländehöhen am Siedlungsrand
   inventarisieren; Suchgebiete mit x/y-Grenzen und jeder relevanten z-Ebene führen.
   Ein z=3-Ergebnis bewertet keine Fläche auf z=4.
2. Pro Gebiet Abdeckung und Erkenntnisstufe getrennt dokumentieren: ungeprüft,
   Gelände/Hindernisse beobachtet, Anschluss bestätigt, Kandidat gefunden,
   Vorschau gültig/abgelehnt. Unbekannt und am Limit abgeschnitten separat halten.
3. Vor Abriss bestehender Gebäude zuerst ungenutzte, bereits distriktverbundene
   Randflächen prüfen. Ein kleiner und ein repräsentativer größerer Grundriss
   reichen als Pilot; keine Vollkatalogsuche ohne erkennbaren Nutzen.
4. Bei unbekannten Kacheln Ursache und tatsächliche planningCalls prüfen.
   Konkrete Hindernisse lesen; begrenzte direkte Planung um bestätigte Wege
   verwenden, statt die ganze Region aus der Auswahl zu streichen. Gültige
   Vorschau bleibt Voraussetzung für jede positive Platzierungsaussage.
5. Suchfenster nicht ausschließlich nach den drei besten freien Ausschnitten
   auswählen: bisher ungeprüfte Bereiche/Höhen und Anschlussnähe berücksichtigen.
   Nach vier repräsentativen Vorschauen wie hier Befund dokumentieren; weitere
   Suche nur bei offenem, konkretem Bauziel. Ergebnisse nach Spieländerung frisch prüfen.

MCP-Folgearbeiten im Backlog: Höhenübersicht, Abdeckungsregister, lokalisierte
unknown-Gründe und nachvollziehbare Fensterauswahl. In diesem Auftrag dokumentiert,
nicht implementiert. Befund: beide Bereiche wurden bisher zu wenig berücksichtigt;
beim Sägewerk verstärkt eine konkrete Aggregationsschwäche den Planungsfehler.


## Live bestanden: regionale Arbeitsreichweite (2026-10-05)

Menschliches Gate bestätigt, fünf installierte Dateien stimmen mit Paket
0.35.4-20261005-174150-098e2061 überein. Farm: 490 native Reichweitenzellen,
36 interne Reads, planningCalls=0. Fünf freie feuchte Rechtecke mit 13 Zellen;
alle empfohlenen Zellen zugleich frei, feucht und in Reichweite. Unabhängige
32-Zellen-Reichweitenseite stimmt vollständig mit dem Raster überein.
Wohnhauskontrolle: supported=false/source=unavailable, vollständig unbekanntes
Raster und keine Reichweitenempfehlungen; ebenfalls keine Planung (21 Reads).
Keine Spielmutation im Abnahmetest. Feature im beschriebenen Umfang bestanden.
Die folgenden offenen Gate-Einträge sind historische Vorbereitung.


## Vor Test-Gate: regionale Arbeitsreichweite (2026-10-05)

Anbaupilot auf Bridge 0.35.4 bestanden: drei freie, feuchte Zellen mit separat
belegter Farmreichweite als Karotten markiert. Nach rund 23 Spielstunden alle
DREI tatsächlich lebend/wachsend, ohne Wasserstress. Zeitlauf nach exakt 24 Stunden
abgeschlossen, Überschwingen 0, Endpause bestätigt. Danach 35 Biber (26/9),
36 Betten, Wasser 77, Beeren 70, frei verfügbares Holz 7. Keine kritischen
Bedürfnisflags; Vorlagenabdeckung weiterhin 24/157, Gesamtziel offen.

Nächste Effizienzverbesserung MCP-seitig vorbereitet, Bridge bleibt 0.35.4:
survey_region optional workBuildingId und planBuildings=false. Vollständige
native Reichweite (höchstens 2048 Zellen/64 Seiten), Quellenangabe und separates
Raster; freie feuchte Rechtecke daraus neu berechnet, nicht nur vorhandene Top-6
gefiltert. Fehlende Reichweite unbekannt, wechselnde/unvollständige Daten abweisen.
Ohne workBuildingId bleibt bisheriges Verhalten; planBuildings=false spart bis
zu zwölf Planaufrufe. template/districtId bleiben aus Kompatibilität Pflicht.
Keine automatische Markierung und keine Aussage über Personal oder passenden Rohstoff.

Regressionen ergänzt: mehrseitige Schnittmenge, trockene/gesperrte Zellen,
unbekannte Reichweite, wechselnde Zähler, Duplikate, Budget, falsche Sitzung und
Standardverhalten. Drei Feature-Dateien per Roslyn syntaktisch geprüft, kein Build
oder Testlauf. Änderungen uncommitted bis menschliches Gate und Liveabnahme.
Abnahmepilot: Farm plus 16×16-Region, planBuildings=false; planningCalls=0,
Reichweitenzellen gegen gezielte inspect_work_range-Seite prüfen und Empfehlungen
gegen Feuchte/Hindernisse. Nicht unterstütztes Wohnhaus muss unbekannte Reichweite
und keine Empfehlungen liefern. Bei falscher Kontrollprobe stoppen.


## Live bestanden: Bodenfeuchte 0.35.4 (2026-10-05)

Menschliches Gate abgeschlossen; alle fünf installierten Dateien stimmen mit
Paket 0.35.4-20261005-172618-6954fbfd überein. Native MCP-Verbindung erreichbar.
Vier Forstzellen feucht, 16 obere Vergleichszellen trocken; vier Zellen über
Boden liefern null. Drei regionale Abfragen stimmen mit den direkten Proben
überein. Trockenes freies 2×2-Rechteck ausgeschlossen, feuchtes freies 2×1-Rechteck
gefunden und separat bestätigt. Rechteckzellen sind gleichzeitig hindernisfrei
und feucht; bestehende Pflanzflächen bleiben ausgeschlossen.
Regional 12 bzw. 17 native Reads, keine zusätzlichen Reads für Bodenfeuchte.
Keine Spielmutation oder Simulation; Spiel pausiert. Momentane Bodenfeuchte
belegt keine Arbeitsreichweite, pflanzenspezifische Eignung oder Dauerbewässerung.
Versionslisten-/Syntaxkorrektur eingeschlossen; frühere offene Gate-Einträge
unten sind historisch. Feature im beschriebenen Umfang abgenommen.


## Bodenfeuchte: 0.35.4 vorbereitet, noch nicht live abgenommen

Erster Live-Einstieg nach menschlichem Gate am 2026-10-05 blockiert: Mod startet,
aber Client-Versionslisten akzeptieren 0.35.4 noch nicht. Listen korrigiert und
manifestgebundener Client-Regressionstest ergänzt. Neuer Skriptlauf nötig;
Feuchte-/Höhenvergleich noch nicht durchgeführt.

inspect_map_region liefert soilIsMoist: true/false auf Bodenfeldern, null für
andere Höhen bzw. alte Bridge. Quelle: öffentliches
Timberborn.SoilMoistureSystem.ISoilMoistureService.SoilIsMoist(Vector3Int).
Signatur gegen lokale Spielbibliothek per bestehendem MetadataLoadContext-Prüfer
bestätigt; keine privaten Member, Methodenkörper oder neue Abhängigkeit nötig.
Dienstauflösung und Koordinatensemantik bleiben Gegenstand des Livetests.

survey_region ergänzt moisture.rows mit m=feucht, d=trocken, ?=unbekannt,
-=kein Boden auf dieser Höhe sowie moisture.moistEmptyGroundPatches (maximal
sechs disjunkte Rechtecke bis 4×4). Nur feuchte Zellen mit Hinderniszeichen .
gehen ein. Gebäude, Felder, unbekannte Geometrie, Wasser und Verschmutzung bleiben
ausgeschlossen. Das Hinderniszeichen . bedeutet nicht überflutet, nicht trockene
Erde. Beide Karten haben identische Zeilen-/Spaltenreihenfolge. Keine zusätzlichen
nativen Reads; Baukandidatensuche unverändert. Feuchte ist eine Momentaufnahme,
keine Zusage pflanzenspezifischer Eignung, Bewirtschaftung oder zukünftiger Versorgung.

Gate: menschliches prepare-human-live-test.ps1, danach Spiel laden/live bereit.
Pilot: feuchte Bodenprobe bei lebenden Pflanzen und trockene Vergleichsprobe
finden; Höhen-Negativkontrolle muss null liefern. Regionale Werte gegen dieselben
direkt abgefragten Zellen vergleichen; feuchte Rechtecke dürfen keine gesperrte
Zelle enthalten. Bei falscher Kontrollprobe stoppen und Koordinatensemantik
diagnostizieren. Build, neue Tests und Livetest noch nicht ausgeführt.


## Fortsetzung 2026-10-05 — unknown-Reparatur live bestanden

Nutzer hat die Arbeit mit live bereit fortgesetzt. Neues Paket
0.35.3-20261005-165123-33f2985e: alle fünf installierten Paketdateien stimmen.
Anfangs Transport closed; nach erneuter Bereitmeldung reguläre MCP-Verbindung
wieder erreichbar. Kein zusätzlicher Server oder Spielsteuerungs-Fallback.

Gleicher 16×16-Nordpilot liefert jetzt drei Bodenrechtecke und vier Baukandidaten,
33 interne Leseaufrufe inklusive zwölf Planungen in etwa 2,65 Sekunden. Die
überlappende Teilfläche mit natürlicher Slope bleibt ohne freie Bodenrechtecke.
Eine unbeteiligte 4×1-Fläche ist separat als hindernisfrei, trocken und auf
richtiger Höhe geprüft. Reparatur im angekündigten Umfang live bestanden;
Mehrteilflächen-Überlappung synthetisch abgedeckt, nicht zusätzlich live erzeugt.
Nächster Schritt: nachweislich erreichbare Produktionsflächen vergrößern.



## Vor menschlichem Gate — unbekannte Objekte regional begrenzen (2026-10-05)

Fortsetzung des Gesamtziels: Nordscan 16×16 liefert wegen unbekannter Objekte
keine Bodenrechtecke. Eine natürliche Slope auf niedrigerer Ebene wurde in einer
überlappenden Teilfläche direkt nachgewiesen. Fehler im Klassifikator: jedes
other-Objekt sperrte alle ansonsten freien Zellen der gesamten Region.

Korrektur: unbekannte Grundrisse sperren nur die nativen 8×8-Abfrageflächen,
die ihre Überlappung tatsächlich melden. Überlappt ein Objekt zwei Teilflächen,
bleiben beide gesperrt. Bekannte Gebäude, Eingänge und Pflanzmarkierungen bleiben
geschützt. Keine Annahme über den unbekannten Grundriss, keine neue Baufreigabe.
Zwei synthetische Regressionsfälle ergänzt; Quellcode/Diff geprüft. Kein Build,
keine Installation und kein Commit/Push dieser Reparatur.

Nach menschlichem Gate denselben Nordausschnitt prüfen: unbekannte Objekte dürfen
unbeteiligte Teilflächen nicht mehr sperren. Betroffene Überlappungsflächen müssen
unbekannt bleiben; freie Rechtecke stichprobenweise nativ prüfen. Bei falschem
Kontrollfall stoppen. Aktueller Ausbau unverändert: 32 Biber, 36 Betten, 24/157;
Holz frei 1. Keine neuen Spielaktionen in dieser Diagnose. Aufforstung/Nahrung
bleiben der nächste Schritt nach Reparatur. Die untersuchte Nordreihe enthält
auch einen versetzten Wohnhausgrundriss und wurde deshalb nicht bepflanzt.



## Praxistest: regionaler Kandidat bis zum fertigen Wohnhaus

2026-10-04: Ersten regional gefundenen Lodge-Kandidaten im Entwicklungspilot
frisch geplant und gebaut. Tatsächlicher Baustellenzugang buildersReachable=true
vor Zeitlauf, anschließend finished_accessible: Gebäude fertig, Eingang frei
und erreichbar. Exakt 24 Spielstunden, Überschwingen 0, Endpause bestätigt.
Bevölkerung danach 32 (25 Erwachsene, 7 Kinder), 36 Betten, vier frei;
keine kritischen Bedürfnisflags. Vorlagenabdeckung unverändert 24/157.

Versorgungsgrenze: Wasser 83, Beeren 63, frei verfügbares Holz 1. Karottenbestand
während des Laufs 0; nach Abschluss 30 lebende Karottenpflanzen, davon zwei reif,
keine mit Wasserstress. Forststichprobe: 13 lebende Kiefern, noch nicht ausgewachsen,
kein Wasserstress. Besetzte westliche Holzfällerflagge meldet laufenden Job;
das allein beweist keinen ausreichenden Holzoutput. Nächster Ausbau muss zuerst
Holz- und Nahrungsproduktion vergrößern, nicht weitere Baumaterialien verplanen.

Bedienungsfehler im Test: advance_building_project akzeptiert waitSeconds nur
0–20, Bauchargen dagegen 0–45. Versuch mit 30 vor Ausführung abgelehnt; lesend
awaiting_simulation und run=null bestätigt, dann gezielt auf 20 korrigiert.
Backlog: Parametergrenzen in kurzen Werkzeugbeschreibungen nennen und reine
Parameterfehler von möglicherweise unbestätigten Aktionen unterscheiden.



## Live bestanden — regionale Flächensuche (2026-10-04)

Menschliches Gate abgeschlossen, fünf installierte Paketdateien mit Paket
0.35.3-20261004-234748-393364c9 abgeglichen. 16×16-Pilot: 32 native Leseaufrufe,
darunter zwölf Planungen, in etwa 2,6 Sekunden. Vier Baukandidaten und vier
Bodenrechtecke in einer kompakten Antwort. Richtwert etwa 30 Aufrufe knapp
überschritten; feste Obergrenze 82 eingehalten. Kein zusätzlicher Scan nötig.

Alle 30 Feldmarkierungen als belegt, sechs unabhängig abgefragte tiefere
Wasserzellen als andere Bodenhöhe ausgeschlossen. Stichprobe von 17 vorhandenen
Gebäuden/Wegen: belegte Grundrisszellen und Eingänge nicht frei. Größtes freies
Bodenrechteck unabhängig ohne überlappende Objekte. Erster Kandidat nativ
validiert: Geometrie gültig, Eingang angeschlossen, keine verlorenen Verbindungen
in den geprüften Bestandsproben, Vorschau wiederhergestellt. Baustellenzugang
und tatsächlicher Fertigzugang bleiben unknown; keine allgemeine Baufreigabe.

Negativkontrolle bei bestätigtem Tempo 1 liefert state_conflict ohne Bericht.
Kontrolllauf danach abgebrochen, Pause separat bestätigt. Feature im begrenzten
Umfang abgenommen; keine Behauptung vollständiger Regionssuche, Bewirtschaftung
oder neuer Gebäude durch diese lesende Abfrage. Nächster Schritt: Kandidaten für
den weiteren Gesamtausbau verwenden. Letzter Ausbaucheckpoint: 31 Biber, 24/157.



`survey_region` ist eine rein lesende MCP-Zusammenfassung auf vorhandenen nativen
Schnittstellen. Keine Änderung an Bridge, Baugrenzen, Spielzeit oder Spielstand.
Eingabe: session, districtId, template, x/y/z, width/height (1–16).
Vorher und nachher muss Tempo 0 beobachtet werden; keine automatische Pause.

Ein Aufruf bündelt Gelände, vollständige Gebäudegrundrisse, vorhandene Eingänge,
Vegetation und auch noch leere Feld-/Forstmarkierungen. Rasterzeilen laufen von
kleinem nach großem y; Spalten von kleinem nach großem x. Legende im Ergebnis.
Bis zu sechs nicht überlappende Bodenrechtecke, jeweils höchstens 4×4, dienen als
Anbauhinweise. Bewässerung, Bodenfruchtbarkeit, Pflanzenvalidierung und
Arbeiterreichweite sind damit ausdrücklich nicht nachgewiesen.

Bis zu drei überlappende Suchfenster von höchstens 8×8 mit beobachtetem Weg und
möglichst viel trockenem freien Boden werden priorisiert. Für jedes Fenster
prüft die vorhandene native Gebäudeplanung vier Drehungen. Bis zu vier nach
neuem Wegbedarf sortierte Kandidaten enthalten Ursprungsposition, Fenster,
Drehung und Planbezug. Das sind keine ausführbaren Bauaufträge: gemeinsame
Validierung, Bauzugang und Schutz bestehender Zugänge bleiben unverändert nötig.
Leere Ergebnisliste beweist nicht, dass die ganze Region unbebaubar ist.

Grenzen: eine Bodenhöhe, keine automatische Räumplanung oder Höhenanschlüsse;
Vegetationsursprünge sind keine vollständigen Pflanzen-Grundrisse. Ein Objekt,
dessen Ursprung außerhalb des abgefragten Teilbereichs liegt, macht dessen freie
Zellen konservativ unbekannt. Gebäude mit abgeschnittenem Grundriss verhindern
den gesamten Bericht, auch bei Ursprung außerhalb der Region. Bestehende Wege
sind noch kein Distriktanschlussnachweis. Seiten sind trotz Pausenkontrollen nicht
atomar; Sitzungs-/Versionswechsel, unstabile Gesamtzahlen und Duplikate brechen ab.

Feste Grenzen: 512 Gebäude, je 512 Feld-/Forstmarkierungen, 128 Objekte je
8×8-Teilfläche; maximal 82 native Leseaufrufe einschließlich höchstens zwölf
Planungen. Keine Wiederholungen, keine Mutation. NativeReads und PlanningCalls
machen den tatsächlichen Aufwand sichtbar. Unvollständige Daten liefern keinen
Teilbericht mit vermeintlich freien Flächen.

## Prüfung und Übergabe

Gezielte synthetische Tests ergänzt: Wasser/Höhen, versetzter Gebäudegrundriss,
Bestandseingang, unbepflanzte Markierung, begrenzte überlappende Suche, kompakte
Antwort, fehlende/abgeschnittene Geometrie, Daten-/Sitzungswechsel und laufendes
Spiel. Katalogintegration ergänzt. Nur Quellcode/Diff geprüft, nicht ausgeführt.

Nach menschlichem Skriptlauf und `live bereit`: ein 16×16-Ausschnitt mit
bekannten Feldern, Wasser/Höhen und Gebäuden. Raster anhand weniger gezielter
Rückfragen abgleichen; keine geschützte Fläche als freies Bodenrechteck.
Ein gelieferter Baukandidat wird mit der bestehenden nativen Bauprüfung geprüft;
kein Kandidat wird als Gesamtablehnung interpretiert. Negativkontrolle bei
laufender Simulation muss vor dem Flächenscan ablehnen. Ziel: eine gebündelte
Entscheidung statt der bisherigen einzelnen Gelände-/Hindernis-/Markierungs-
und Drehungssuchen; im aktuellen kleinen Spielstand höchstens etwa 30 native
Leseaufrufe für den Hauptpilot. Bei falscher Klassifikation stoppen und reparieren.
