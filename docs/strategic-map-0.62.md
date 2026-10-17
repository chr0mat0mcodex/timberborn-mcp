# Strategischer Kartenscan 0.62 — begrenzt live abgenommen

## Zweck und Einstieg

Das übergeordnete Ziel ist eine kartenweite Übersicht über Flüssigkeitsnetze:
wo Flüssigkeit fließen kann, wo aktuell Wasser/Faulwasser oder Mischungen liegen
und welche Änderungen aus belegtem Quellenverhalten und Wetter zu erwarten sind.
Flutschutz ist eine Anwendung. Der erweiterte 0.62-Quellstand liefert zusätzlich
einen allgemeinen geometrischen Flussnetzbericht und native Quellen-/Wetterdaten;
Gate und begrenzter Livepilot einschließlich Dürrewechsel sind bestanden. Kontamination ist eine
lokale Beobachtung; sie darf nicht auf die gesamte Route übertragen werden.

### Flussüberblick ohne Siedlungsziel

`scan_strategic_map(session, overviewOnly=true, detail="full")` benötigt keine Zielkoordinaten.
Mit derselben Kennung und `overviewOnly=true` fortsetzen. Der fertige Bericht
enthält unter `rivers` räumliche Abschnitte, Verbindungen und Quellen. `sectionOffset`
blättert 32 Abschnitte, `linkOffset` 64 Verbindungen und `sourceOffset` 16 Quellen;
Gesamtzahlen und `more*` zeigen die verbleibende Abdeckung. Blättern fertiger Scans
liest keine Terrainkacheln erneut. Abschnittsnummern gelten nur innerhalb des Scans.

Abschnitte verbinden benachbarte nasse Höhenintervalle mit überlappenden
Wassersäulen, unterteilt an 8×8-Grenzen. Sie nennen räumliche Grenzen, Beispielzelle,
überdeckte Lage, lokale Pegel- und Kontaminationsbereiche. `water`/`badwater` nutzen
eine numerische Toleranz von 0,00001; `mixed_or_variable` bedeutet Mischung oder
unterschiedliche lokale Werte, keine gleichmäßig durchmischte Flüssigkeit.
Trockene native Auslässe bleiben als `dry` erhalten. Fehlende Quellenabdeckung
wird ausdrücklich gemeldet, einschließlich Quellen ohne verwendbaren Gebäudeanker.

Eine geometrische Mehrquellensuche verbindet die Abschnitte über freie Intervalle,
auch über trockene Schwellen. Verbindungen nennen die gefundene Geländeschwelle,
einen Treffpunkt der Suchgebiete und ob die lokalen maximalen Pegel darüber liegen.
Der Treffpunkt ist nicht notwendigerweise die höchste Schwelle. Die Kanten sind
ein Netz von Nachbargebieten, keine Liste aller möglichen Routen oder global
optimaler Sperrstellen. `direction` bleibt ausdrücklich unbekannt: keine native
Strömungsmessung, keine Wasserbauwerksimulation, keine Ankunftszeit. Kartenrand-
schwellen sind geometrische Hinweise und kein Nachweis eines tatsächlich offenen
Abflusses. Grenzen: 524.288 Knoten, 4.096 Abschnitte, 32.768 Kanten; Überschreitung
liefert einen benannten Analysestopp, keine abgeschnittene Erfolgsmeldung.

### Native Quellen und Wetteränderungen

`WaterSource` liefert Auslasskoordinaten (maximal 64 mit Kürzungsflag), eingestellte
und aktuelle native Stärke, Kontamination und aktive Stärkemodifikatoren (maximal
32 mit Kürzungsflag). Stärke ist kein gemessener Volumenstrom. Die native
`HazardousWeatherWaterSourceSpec.ActiveInHazardousWeather` wird, soweit vorhanden,
als Aktivierungsregel für das ausgewählte gefährliche Wetter ausgewiesen.
Andere Modifikatoren, Regler oder Übergänge können die tatsächliche Stärke
zusätzlich verändern. Ein fehlender Regelwert bedeutet unbekannt, nicht konstant.

Wetterberichte enthalten Zyklus, Tag/Teilzyklustag, aktuelle gefährliche Phase,
ausgewählten Gefahrentyp und native Phasendauern/Starttag. Die Rohwerte sind noch
kein live verifizierter Restzeit-Countdown. Erster und letzter Wetterstand zeigen
einen Wechsel während des nichtatomaren Scans. Ein späterer vollständiger Scan
kann mit dem letzten zeitlich vorausgehenden vollständigen Scan derselben Sitzung
im begrenzten Cache verglichen werden: Stärke-/Kontaminationsdifferenz je stabiler
Quellen-ID und Wetterwechsel getrennt. Das belegt eine Änderung, keine alleinige
Wetterursache. Künftige Kontamination bleibt ohne öffentlich belegte Regel unbekannt;
insbesondere wird kein Wasser-/Faulwasserwechsel aus einem Vorlagennamen erfunden.

`scan_strategic_map` untersucht die ganze Karte für eine explizite Schutzregion.
`x/y/z/width/height` beschreiben die bewohnte oder geplante Zone, nicht den
Scanbereich. `z` ist eine freie Rasterebene dieser Zone, üblicherweise die
Aufstandshöhe. Der Scan ist lesend und verlangt keine Pause oder Schreibfreigabe.

Erster Aufruf: aktuelle `session`, Zielregion und zunächst `tilesPerCall=1`.
Die Antwort nennt `scanId`, bearbeitete Kacheln und `coverage.nextTile`.
Mit derselben Kennung, Session und unveränderten Ziel-/Szenarioparametern fortsetzen.
Nach dem kleinen Pilot sind bis acht Kacheln je Aufruf möglich, Standard vier.
Eine 256×256-Karte hat 64 Kacheln. Erst bei vollständiger Terrainabdeckung entsteht
eine Analyse. Das Ergebnis einer fertigen Kennung ist historisch; ein neuer
Aufruf ohne `scanId` beginnt eine frische Beobachtung. Keine Simulationsteuerung.

Optional `scenarioWaterLevel`: ausdrücklich angenommene absolute Wasserhöhe in
Rasterkoordinaten, keine Zusatzwassertiefe und keine Zukunftsprognose. Ohne Szenario
werden vorhandene Wasserproben benutzt; trockene Quellen-/Randkandidaten haben
keinen erfundenen Pegel. `objectOffset` blättert beobachtete Spezialobjekte zu 32.

## Daten und Grenzen

- Eigener GET-Endpunkt `strategic-map`, sessiongebunden, auf dem Spielthread.
  Pro Kachel höchstens 32×32 Spalten und alle bis zu 64 Höhen; Karten bis 512×512.
  Freie vertikale Intervalle bleiben getrennt: Höhlen und Überhänge werden nicht
  auf eine einzige Oberflächenhöhe reduziert. Terrainservice und Wasserkarten-API
  sind dieselben öffentlichen Dienste wie beim bestehenden lokalen Kartenleser.
- Wasserstand und Kontamination werden am unteren Ende jedes freien Intervalls
  gelesen. Die Semantik von Tiefe und gedecktem Wasser ist im Livepilot zu prüfen.
  Aus Tiefe plus Intervallboden abgeleitete Pegel sind vor dieser Prüfung nicht
  als allgemein hydrologisch validiert zu behandeln.
- Nichtvegetative Blockobjekte werden mit Vorlage, Kategorie und Ankerposition
  beobachtet. Pflanzen werden nicht gegen die Objektgrenze gezählt. Unbekannte
  Vorlagen bleiben sichtbar. Quellnamen sind ausdrücklich Vorlagenhinweise,
  keine native Klassifikation oder Aussage über Förderrate bzw. Aktivierung.
  Gebäude und Wege dienen nicht als modellierte Wassersperren.
- Begrenzung je Kachel: 100.000 Registry-Einträge und 512 Objektbelege, maximal
  4 MiB Transportantwort. `objectsComplete=false` macht fehlende Objektabdeckung
  sichtbar; Terrainabdeckung ist davon getrennt. Keine stille Vollständigkeitsbehauptung.
- Jede Kachel ist eine eigene Beobachtung. Zeitspanne und Alter stehen im Bericht;
  `atomic=false`, keine globale Änderungsrevision. Neue Terrainänderungen und
  wandernde Wasserfronten können zwischen Kacheln liegen. Kritische Kandidaten
  daher vor der Planung lokal frisch beobachten. Ein Sessionwechsel wird abgewiesen.
- MCP hält höchstens zwei Scans je Client, 15 Minuten Leerlauf. Kein persistentes
  Kartenarchiv und keine Speicherung persönlicher Spielstände. Ungültige oder
  abgebrochene Seiten erhöhen die bestätigte Abdeckung nicht. Nach Verlust der
  Kennung ist ein neuer rein lesender Scan zulässig.

## Gefahrenpfade und Schutzkandidaten

Ein Graph verbindet benachbarte freie Intervalle nur bei vertikalem Überlapp.
Eine Minimax-Suche von der Zielzone ermittelt die niedrigste maximal zu
überwindende Geländehöhe pro Verbindung. Getrennte Höhlen bleiben getrennt,
alternative niedrigere Rinnen können einen direkten hohen Rücken umgehen.
Maximal 524.288 freie Intervalle werden analysiert; Überschreitung liefert einen
expliziten Analysestopp ohne Empfehlungen. Vollständiger Scan ist keine Garantie
für erfolgreiche Analyse und erst recht kein Sicherheitsnachweis.

In `detail="full"` werden bis zu acht repräsentative Korridore zu beobachtetem Wasser, Quellvorlagen oder
dem Kartenrand werden ausgegeben. `candidateCount` nennt die Zahl der geometrischen
Kandidaten vor der Auswahl; die Liste ist kein vollständiges Gefahreninventar.
Pfade enthalten Wendepunkte, maximal 64 plus Endpunkt; Abschneiden wird gemeldet.
Ein fehlender Pfad ist keine Aussage, dass Wasser oder Biber dort nicht hinkommen.
Strömungsrichtung, Durchsatz, Rückstau, Druck in Höhlen und bestehende
Dämme/Schleusen werden nicht simuliert.

Jeder Korridor nennt einen Geländesattel, benachbarte höhere Geländeintervalle und
einen quer verlaufenden möglichen Sperrabschnitt (bis 16 Zellen je Seite).
Die angenommene Krone liegt eine Rasterstufe über dem Maximum aus Geländeschwelle
und aufgerundetem Szenario-/Probepegel; eine niedrigere Pegelreferenz wird in
`crestBasis` ausdrücklich als solche gekennzeichnet.
Ohne Pegel gilt eine Stufe über der Geländeschwelle. Das ist eine Planungsannahme,
kein geprüfter Freibord. Positionen und Höhe je Zelle sind konkret, aber kein
ausführbarer Bauplan. `crestWithinMapHeight=false` meldet eine zu hohe angenommene
Krone ausdrücklich. `bothTerrainBanksFound` beschreibt ausschließlich diesen
Querschnitt; parallele Rinnen und Höhlen können ihn umgehen.

Vor einer Schutzmaßnahme: Querschnitt und Alternativzuflüsse frisch prüfen,
geeigneten Abfluss erhalten, Rückstau bewerten, verfügbare Vorlagen/Forschung,
Material, Bauarbeiterzugang und Bauphasen prüfen. Danach vorhandene native
Bauplanung einsetzen. Wasser vor/nach Fertigstellung messen. Höhere Standorte
im Ergebnis sind geometrische Kandidaten, keine belegten Evakuierungswege.

## Nachweise und menschliches Gate

### Erweiterter Livepilot am 16. Oktober 2026

Menschliches Gate erfolgreich; nach Katalogneustart `overviewOnly` ohne Zielzone
bei Tempo 7 nutzbar. Vollständiger Scan: 65.536 Spalten, 444 Abschnitte, 776
geometrische Verbindungen, zehn native Quellen; Quellenabdeckung vollständig
gemeldet. Alle Abschnitts-/Kantenseiten gelesen: keine doppelten IDs/Kanten,
korrekte Endflags, leere Quellenseite nach Ende. Fertige Seiten führen nur die
Sessionkontrolle aus, keine neuen Terrainreads. Die zehn Quellen sind in dieser
Momentaufnahme sieben reine Wasser- und drei reine Faulwasserquellen; Abschnitte
enthalten dagegen auch örtliche Mischungen. Sauberer Auslass, verseuchte Höhle
und trockene Schwelle gegen native Zellen bestätigt. Wetter zunächst gemäßigt,
ausgewählte Gefahr Dürre; spezielles Aktivierungsregel-Feld bei diesen Quellen
unbekannt, vorhandene Dürre-Stärkemodifikatoren gemeldet.

Separater Zielzonenscan in 2,48 Sekunden Beobachtungszeit: keine Quellenhinweise
im falschen Höhlenstockwerk mehr; alle Kronen über dem jeweiligen Sattel,
niedrigere Referenzpegel mit eigenem `crestBasis` gekennzeichnet. Alte Session
aus vorherigem Spielstart mit `stale_session` abgewiesen. Keine Bauaufträge.
Wetterwechselvergleich bestanden: gemäßigt zu Dürre an Zyklustag 9. Alle zehn
Quellen gingen auf Stärke null, die Dürremodifikatoren von 1 auf 0; die ausgegebenen
Stärkedifferenzen entsprachen den früheren Werten mit negativem Vorzeichen.
Quellkontamination blieb unverändert. Der neue Scan meldete 404 Abschnitte/681
Kanten und den tatsächlichen Wetterwechsel; Beobachtungszeit 3,48 Sekunden.
Lokale Restflüssigkeit und Quellenzustand bleiben getrennt. Nicht belegt:
Faulwasserflutwechsel, spezielle Aktivierungsregeln (bei allen Quellen null),
zukünftige Kontamination, exakte Ankunftszeiten und allgemeiner Wettercountdown.

Das bestandene menschliche Gate umfasst die ergänzten Tests für Rinne mit Geländesattel, niedrigeren Umweg, Tunnel
unter Dach, getrennte Höhen, Teilabdeckung, trockene Quelle, fortsetzbare Seiten,
ungültige Seiten, Sessionwechsel, Schema/GET-only und MCP-Aufruf bei Tempo 7.
Keine Agent-Builds oder Testläufe. Menschliches Gate laut Entwickler erfolgreich;
Bridge 0.62.0 anschließend über den verbundenen Vorschaltprozess live bestätigt.
Nach Codex-Neustart war das Werkzeug verfügbar. Live bei Tempo 7: 64/64 Kacheln,
65.536/65.536 Spalten, Terrain und Objekte vollständig gemeldet; 73.390 freie
Intervalle und 7.559 Höhlenspalten. Beobachtungszeitraum 20,7 Sekunden einschließlich
Pilotpausen; Achterchargen dauerten im Pilot jeweils deutlich unter einer Sekunde.
Quellboden 2 plus Wassertiefe 4 ergab Pegel 6; ein dreizelliger Tunnel mit Boden 5,
Dach ab 6 und Wassertiefe 1 wurde durch native Detaildaten bestätigt.
Dabei entdeckt: Quellenhinweise wurden fälschlich auf andere Höhenintervalle
derselben Spalte übertragen; bei niedrigem Pegel lag die Krone unter dem Sattel.
Beides korrigiert, Regressionen im menschlichen Gate bestanden und oben live nachgeprüft.
Kronenhöhe bleibt eine Annahme: Tunneldecken können niedriger liegen. Die genannten
Bauhöhen sind daher keine unter einem Dach tatsächlich platzierbaren Blöcke.
Keine Bauvorschau oder Schutzwirkung abgenommen; alte Session nach Spielneustart abgewiesen.

Wiederholbarer Pilot und Grenzen für weitere Karten:

0. Erweiterung: zunächst zwei Kacheln mit `overviewOnly=true`, native Quellen- und
   Wetterwerte auf Vollständigkeit/plausible Rohwerte prüfen. Danach Gesamtnetz,
   Seitenwechsel und eine trockene sowie eine überdeckte Verbindung prüfen.
   Einen tatsächlich beobachteten Wetterwechsel mit zwei vollständigen Scans
   derselben Sitzung vergleichen; Quellenänderungen gegen neue native Werte
   prüfen. Ohne Wetterwechsel keine wetterabhängige Liveabnahme behaupten.
1. 0.62 und Session bestätigen. Eine, dann eine zweite Kachel bei laufender
   Simulation lesen: kein Tempoeingriff, exakte Abdeckung, begrenzte Laufzeit.
   Spalten an offenem Wasser und unter einem Überhang gegen lokale native
   Zellabfragen vergleichen. Bei Semantikfehler, Zeitlimit oder unbrauchbarer
   Laufzeit stoppen und diagnostizieren, nicht die ganze Karte durchlaufen.
2. Bei bestandenem Pilot mit derselben Kennung fortsetzen. Kartenmaße, vollständige
   Spaltenzahl, Objektlücken und Beobachtungszeitraum prüfen. Einen Spezialgegenstand
   außerhalb der Siedlung gegen native Detaildaten prüfen.
3. Einen möglichen Zufluss zur Zielzone, einen höherliegenden Umweg und einen
   Tunnel-/Höhengegenfall mit Detaildaten abgleichen. Querschnitt eines
   Schutzkandidaten lokal prüfen, dann höchstens eine native Bauvorschau; keine
   automatische Bauausführung aus dem Scan.
4. Alte Kennung nach Sessionwechsel ablehnen. Einzelfälle, Laufzeit und Grenzen
   protokollieren. Erst dann Featureabschluss/Commit; kein pauschaler Flutschutzbeleg.

## Gefahrenbewertung im Zielscan — begrenzt live abgenommen

`analysis.hazards` zuerst auswerten, danach die Korridorbeispiele. Die Bewertung
zählt alle erreichbaren nassen Hohlräume und diejenigen mit beobachtetem Pegel
mehr als 0,01 über Zielhöhe **und** minimaler Geländeschwelle. Kontaminiertes
Restwasser bleibt auch bei stillstehenden Quellen eine priorisierte Gefahr.
Die Zähler sind Hohlräume, keine unabhängigen Flutereignisse. Bis zu acht
repräsentative Gefahren erscheinen unter `priorities`; weitere bleiben im Zähler.
`target_water_observed` bedeutet bereits beobachtetes Wasser über der Zielhöhe.
`no_current_head_exceedance_not_safe` ist keine Entwarnung für künftige Ereignisse.

Pro Gefahr: Ursprung, Pegel, Höhenüberschuss, Kontamination, Zielzutritt sowie
Schutzquerschnitt und angenommene Krone. `closureAssessment` prüft Dachfreiheit,
Kartenhöhe, vollständige Ufer und geometrische Umwege bei hypothetischer Sperre.
`terrain_route_cut_at_observed_level` ist nur ein geometrischer Schnitt, keine
Empfehlung zur Sperre. Ab 0.65 zuerst `backwater` und einen
unabhängig geprüften Abfluss bewerten. Dieser Nachweis betrifft einen Ursprung
bei beobachtetem Pegel; weitere Zuflüsse, Bauwerke, Rückstau, Bauphasen, Material,
Forschung, Arbeitszugang und Dauer bleiben zu prüfen. Bei Dachkonflikt, offenem
Umweg oder unvollständigem Querschnitt einen anderen Sperrpunkt untersuchen.
Keinen Abfluss aufgraben, bevor dessen Auslass und Rückwirkungen belegt sind.
Ein niedriger Szenariopegel verkleinert den Schutzvorschlag für beobachtetes Wasser nicht.
`overviewOnly=true` liefert weiterhin keine zielbezogene Gefahrenbewertung.
Explosionszeit, Ereigniswirkung, zukünftige Pegel und Ankunftszeiten bleiben unbekannt.

Abnahme dieses MCP-only-Nachtrags: menschliches Gate; danach vollständiger Zielscan
mit gespeichertem Faulwasser und ruhender Quelle. Gefahrenkopf muss trotz Dürre
erscheinen, Pegel/Schwelle/Zielhöhe lokal bestätigen. Dachkonflikt oder Nebenweg
muss den einfachen Sperrvorschlag abweisen. Keine behauptete Schutzwirkung ohne
separaten Bau- und Wirkungsnachweis. Synthetische Regressionen decken trockene
Quellen vor Restwasser, niedrigen Szenariopegel, Höhlendecke, Umweg, Zielhöhe,
bereits nasse Zielzone und unvollständige Karte ab. Menschliches Gate anschließend als live bereit übergeben.

Livezwischenstand: vollständiger Zielscan bei Tempo 7, 65.536 Spalten über 9,44
Sekunden. 9.053 erreichbare nasse Hohlräume, 2.615 potenzielle kontaminierte
Zuflüsse; acht priorisierte Beispiele und Handlungsbedarf in nextStep.
Priorisierter Pegel 11,33 bei (95,109,5), lokale Folgemessung 11,32 bei nahezu
reiner Kontamination. Dachkonflikt bei (131–132,95,5) bestätigt: z=5 frei,
z=6–8 fest. Hypothetische Krone 13 ist dort kein platzierbarer Sperrplan.
Andere Ausgaben: geometrischer Schnitt und unvollständiger Querschnitt.
Kein praktischer Bau-/Flutschutzbeleg. Gemäßigtes Wetter, Quellen aktiv;
Dürre-/Restwasser-Livefall anschließend bestanden (Abschlussbeleg unten). MCP-Lesungen müssen einzeln
erfolgen: parallele Detailabfragen werden als busy vor Ausführung abgewiesen.

## Kompakte Scanantwort — live geprüft

`scan_strategic_map` liefert standardmäßig `detail="compact"`: Bedrohung,
betroffenes Ziel, beobachteter Pegel, minimale Zuflussschwelle, Kontamination,
Schutzpunkt/Kronenhöhe, konkrete Blockade und nächste Maßnahme. Die vollständigen
Gefahrenzähler und bis zu acht priorisierte Beispiele bleiben erhalten.
`hazards.commonMeasures` und `commonUnresolved` gelten für alle Prioritäten;
individuelle `measures`/`unresolved` ergänzen diese gemeinsamen Angaben.
Abdeckung, Beobachtungsalter und fachliche Grenzen bleiben sichtbar.

Kacheln, Korridorpfade, Querschnittszellen, Spezialobjekte und die allgemeine
Fluss-/Quelleninventur stehen in `detail="full"`. Mit identischer Zielzone und
derselben `scanId` nachlesen: kein erneuter Kartenscan; die bestehende frische
Sessionprüfung bleibt. `overviewOnly=true` liefert kompakt nur den Flussbericht-
Überblick mit Zählern/Wetter/Seitengrenzen, keine zielbezogene Gefahrenbewertung.
Für Inventurseiten full plus entsprechende Offsets verwenden. Fehlende Details
sind keine leeren Beobachtungen. Fehler bleiben vollständig erhalten.

Abnahme: compact/full desselben abgeschlossenen Scans vergleichen; gleiche Gefahren, Maßnahmen und Blockaden, deutlich weniger JSON-Zeichen. Full muss alle Detailfelder wiedergeben, ohne weitere Terrainlesung. Menschliches Gate und Livevergleich bestanden; keine Agent-Testläufe.

## Positive Terraforming-Optionen — begrenzt live geprüft

Der vollständige Zielscan liefert zusätzlich `analysis.terraforming`: bis zu drei
räumlich getrennte 3×3-Reservoirkandidaten mit zwei Ebenen Absenkung. Suchgebiet:
32 Felder um die Zielzone; feste Geometrie, keine erschöpfende Landschaftsplanung.
Kompakt bleiben Nutzen, Ursprung, Fläche, Aushubtiefe, Zielbodenhöhe, geometrisches
Zusatzvolumen, Zuflussbeleg, Blockaden und nächste Maßnahme sichtbar. Nur die
neun `excavationCells` je Kandidat stehen ausschließlich in `detail="full"`.

18 Rasterwürfel Zusatzvolumen sind weder 18 Wassergüter noch eine Reichweite in
Tagen. Ebene offene Oberfläche erforderlich; ein fester Block bleibt unter dem
neuen Boden. Höhlen im Aushub/Boden, seitlich offene Beckenwände, Zielzone samt
Rand und beobachtete Objektanker samt Rand schließen Standorte aus. Objektanker
ersetzen keine vollständige Belegungs-/Fundamentprüfung. Unvollständige Objekt-
abdeckung blockiert den Übergang zur Planung. Örtliches Faulwasser und fehlendes
Wasser über Randhöhe werden als Blockaden ausgewiesen. Sauberes örtliches Wasser
beweist keine dauerhaft saubere Quelle oder künftige Nachfüllung.

Vor Ausführung native Werkzeug-/Forschungs-, Belegungs-, Zugangs- und Bauphasen-
prüfung sowie kontrollierte Nachfüllung, Abfluss, Rückstau, Verdunstung und Pumpen-
nutzung klären. Überregionale Faulwasserpfade aus hazards weiterhin berücksichtigen.
Keine automatische Grabung; keine erfundenen Kosten oder Bewässerungsreichweiten.
`overviewOnly=true` bleibt ohne zielbezogene Terraforming-Optionen.

Abnahme: menschliches Gate, danach ein vollständiger Zielscan. Einen Kandidaten anhand Boden, Seitenwand und Wasserzustand lokal prüfen; compact/full müssen dieselbe Geometrie und Blockaden liefern. Negativfälle für Höhle, seitliche Öffnung, Bebauung, Faulwasser, fehlenden Zufluss und unvollständige Abdeckung sind synthetisch vorbereitet. Keine Agent-Testläufe.

### Livebeleg Compact/Full und Reservoir (16. Oktober 2026)

Menschliches Gate bestanden. Gesamtscan: 64 Kacheln, 65.536 Spalten, 2,87 Sekunden
Beobachtungszeit. Compact 10.434, Full 87.261 JSON-Zeichen: 88,0 Prozent kleiner.
Identische Scan-ID/Abdeckung; alle acht Gefahren einschließlich Pegel/Schwelle,
Zielzutritt, Blockade und gemeinsamer plus individueller Maßnahmen abgeglichen.
Alle drei Reservoir-Optionen stimmen außer den bewusst nur in Full ausgegebenen
neun Aushubzellen überein. Full erzeugte keine Terrainlesung, eine Sessionlesung.

Reservoir (117,145,3), 3×3, Zielboden 1: lokale 5×5×4-Abfrage bestätigt alle
25 Spalten bei z=0,1,2 fest, z=3 frei/trocken. Fester Restboden und seitliche
Wände stimmen mit dem Kandidaten überein. 18 Rasterwürfel sind rechnerisches
Zusatzvolumen; fehlender Zufluss wird als Blockade erhalten. Keine Grabung oder
Wasserspeicherung durchgeführt. Tempo 7 blieb bestehen. Zu diesem Zwischenstand war das Wetter gemäßigt;
Dürre-Abschlussbeleg folgt unten. Keine pauschale Abnahme von Hydraulik,
Fundamenten, Arbeitszugang oder Speicherleistung.

### Abschlussprüfung 0.62.0

Nach bestandenem menschlichem Gate Dürrephase an Zyklustag 9 beobachtet:
zehn von zehn nativen Quellen auf Stärke 0, Quellenabdeckung vollständig.
Vollscan über 65.536 Spalten in 2,67 Sekunden: 7.458 erreichbare nasse Hohlräume,
1.242 potenzielle kontaminierte Zuflüsse. Gefahrenzustand und Schutzpriorität
bleiben trotz ruhender Quellen erhalten. Beispiel (168,122,4): Pegel etwa 10,78,
Kontamination nahezu 1, minimale Geländeschwelle 5. Dachkonflikt am vorgeschlagenen
Sperrquerschnitt bleibt ausdrücklich blockiert; keine Bauwirkung behauptet.

Zusätzliche Kontrollen: geänderte Zielzone bei bestehender Scan-ID liefert
strategic_scan_parameters_changed; anschließende gültige Abfrage liest den Cache
unverändert. overviewOnly kompakt enthält Zähler/Wetter statt Inventurlisten;
Full derselben Kennung liefert 32 Abschnitte, 64 Links und zehn Quellen, ohne
Terrain-Neulesung. Damit sind die vorgesehenen lesenden Nachweise abgeschlossen.
Faktischer Aushub, Speicherung, Nachfüllung und hydraulischer Schutz bleiben
außerhalb dieses Releaseumfangs. Recheck ohne weiteren Codeänderungsbedarf.

Spezialobjektkontrolle beim Abschluss: neun UnstableCore-Anker vorhanden,
darunter (103,116,12) und (168,130,12). Positionen sind lesbar, aber nicht mit
Countdown, Sprengradius, zukünftig geöffneter Geländesperre oder freigesetztem
Reservoir verknüpft. Restwasserwarnung ist keine Vorhersage dieser Explosionen.
