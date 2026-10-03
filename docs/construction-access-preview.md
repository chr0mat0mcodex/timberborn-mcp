# Etappe B: Baustellenzugang unter Vorschau

## Stand

Agent Bridge 0.31.0: menschliches Skript-Gate durch Bereitmeldung bestätigt,
installierte Version live geprüft. Positivkontrolle bestanden, negative
Baustellenkontrolle nicht nachgewiesen; Etappe B nicht abgeschlossen. Keine allgemeine Baufreigabe;
`constructionCovered` bleibt false.

## Öffentliche API und begrenzter Versuch

Die lokal geprüften öffentlichen Signaturen bieten tatsächliche Erreichbarkeit
über `ReachableConstructionSite.IsReachableByBuilders()`. In den geprüften APIs
wurde kein entsprechender direkter Builder-Vorschauaufruf gefunden.
`INavigationRangeService` bietet jedoch `GetRoadSpillNodesInRange(Vector3)` und
`GetRoadSpillPreviewNodesInRange(Vector3)`. `ConstructionSiteAccessible` liefert
die gültigen Zugänge einer bestehenden Baustelle.

Der Versuch vergleicht, ob die Baustellenzugänge in der Reichweite der einzigen
fertigen Distriktzentrale liegen: tatsächliche Navigation, leere Vorschau,
vollständige Bauvorschau und beide Zustände nach deren Entfernung. Die tatsächliche
Builder-Abfrage kontrolliert Ausgangs- und Endzustand. Die Annahme, dass Reichweite
und Builderzugang im Testfall zusammenpassen, benötigt einen Livetest.

Der offizielle [Architekturüberblick](https://github.com/mechanistry/timberborn-modding/wiki/Timberborn-architecture)
erklärt Komponenten, Lebenszyklus und Service-Injektion, aber keinen
Builder-Vorschauvertrag. Keine private Reflection, kein Ersatzgraph und keine
direkte Änderung des echten Navmesh.

## Befund und Grenzen

`roadProtection.constructionAccessPreview` enthält Status, Grund, Baselinevergleich,
Wiederherstellung, Verlustzahl und pro Baustelle Zugänge und Vergleichswerte.
`observed` bedeutet nur: Baseline passt zur Builderkontrolle und die geprüften
Werte sind nach der Vorschau wiederhergestellt. Es bedeutet nicht safe. Ohne
Baustellen, bei Sonderkomponenten, überschrittenen Grenzen, abweichender Baseline
oder nicht auswertbarer API bleibt es unknown.

Beobachteter Reichweitenverlust blockiert den Gesamtprüfbericht und die
Entwicklungspiloten. Gemessene fehlgeschlagene Wiederherstellung sperrt die Sitzung.
Keine Verluste ist kein vollständiger Nachweis. Distriktverlustzähler und affected
behalten ihre Bedeutung; Baustellenbefunde stehen separat in items.

Genau ein fertiger Distrikt, höchstens 32 Baustellen, 64 Zugänge pro Baustelle und
32768 Reichweitenknoten. Erweiterte Sonder-Erreichbarkeit wird ausgeschlossen.
Bestehende Baustellen unter fertiger Vorschau sind kein simulierter neuer Bauzustand.
Bei Joint-Projekten wird nur die vollständige Vorschau geprüft, nicht jedes
Wegpräfix. Neue Baustellen, Lieferwege und vollständiger Bauphasenschutz bleiben offen.

## Test-Gate

1. Mensch beendet Timberborn und führt das Vorbereitungsskript aus.
2. Nach `live bereit`: Version und Pause prüfen; falls nötig mit dem bestehenden
   kleinen Lagerpilot eine echte Baustelle erzeugen.
3. Baseline gegen tatsächliches buildersReachable prüfen. Freie Kontrollvorschau
   und Vorschau auf möglichem Zugang/Engpass vergleichen; zuerst zwei bis drei Fälle.
4. Bei Baselineabweichung, nicht auswertbarer API oder erfolgloser Kontrolle stoppen
   und Ursache klären. Keine Sperre lockern, keine breite Suchschleife oder
   stillschweigende Ersatzarchitektur. Bei Erfolg genaue Abdeckung dokumentieren.

Erwarteter Nachweis: tatsächlicher Baustellenzugang und Vorschauvergleich sind
getrennt; freie und zugangsgefährdende Kontrollen liefern nachvollziehbare Befunde,
Vorschauen vollständig zurückgenommen. Etappe C folgt nach Auswertung dieses Versuchs.

## Livebefund 2026-10-03 und Stopp

Sauber geladenes Testsystem ohne Baustellen: gemeinsame Lager-/Wegvorschau meldet
korrekt unknown/no_existing_construction_sites. Bestehender Entwicklungspilot legt
einen Anschlussweg und eine echte kleine Lagerbaustelle an; Auftrag und realer
Builderzugang separat bestätigt. Das bedeutet nicht fertiggestelltes Lager.

Vier Einzelvorschauen mit dieser Baustelle: geometrisch ungültiger benachbarter
Weg, gültiger freier Weg, Lodge-Sperrvorschau am Hauptweg und abschließend eine
breite ungültige Zentralesperrvorschau im Bereich der acht Baustellenzugänge.
Baselinevergleich und Rücknahme jeweils bestanden; direkter Builderzugang davor
und danach true. Freie Vorschau ohne Verluste. Beide Gebäudesperrvorschauen melden
elf verlorene Distriktverbindungen, aber die Reichweitendiagnose bleibt während
der Vorschau true/lostSites=0. Das kann ohne weiter geprüfte Zugangsgeometrie und
Semantik nicht als bestandene negative Baustellenkontrolle gewertet werden.

Versuch hier gestoppt, keine Suchserie und kein Bauphasenschutz behauptet.
Vorschauen entfernt; Simulation weiterhin pausiert und Spielzeit unverändert.
Die angelegte Baustelle und ihr Anschlussweg bleiben als Testfixture vorhanden.
Keine neue Baufreigabe, kein Abschluss von B und kein Start von C. Nächste
Entscheidung: begrenzte Klärung der tatsächlichen Vorschau-Zugangs-/Reichweitensemantik
oder alternativer öffentlich gestützter Prüfweg. Weitere Experimente benötigen
nach diesem erfolglosen Negativpilot ein neues ausdrückliches go.

## Freigegebene Detaildiagnose 0.31.1

Nach gezielter Ursachenprüfung vom Nutzer ausdrücklich freigegeben. Menschliches
Skript-Gate durch Bereitmeldung bestätigt; installierte 0.31.1 und Detailausgabe
live geprüft. Unter `details` werden
für genau eine Baustelle mit höchstens acht Zugängen fünf Zustände ausgegeben:
before, previewBefore, during, after, previewAfter. Mehr Baustellen oder Zugänge
liefern eine explizit unbekannte Detaildiagnose, keine still abgeschnittene Auswahl.
Die bisherige begrenzte aggregierte Reichweitendiagnose bleibt separat bestehen.

Pro Zelle: rangeContains, connected, roadConnected, existingOccupied,
candidateCoversCell, candidateOccupied und candidateOccupation. Verbindung über
öffentliche AreConnectedInstant/Preview und AreConnectedRoadInstant/Preview, mit
der Distriktzentralenkoordinate als ausgewiesenem Ursprung. Keine Entfernung,
Reichweitenbegrenzung oder tatsächliche Builderfahrt aus diesen Boolwerten ableiten.
Die realen Baustellenzugangspositionen bleiben bewusst unverändert gespeichert.

existingOccupied prüft reale Objektbelegung, nicht Terrain oder vollständige
Begehbarkeit. Kandidatenfelder stammen aus den tatsächlichen Vorlagenblöcken der
aktiven Vorschau; bei Joint-Projekten einschließlich Anschlusswegen. Belegung ist
kein Blockadenurteil: ein Weg kann eine belegte und zugleich begehbare Zelle sein.
Die Zustände außerhalb during enthalten keinen Kandidaten. Vorher-/Nachher-
Vergleich umfasst auch Verbindungen und reale Objektbelegung. Gemessene fehlende
Detailwiederherstellung sperrt über den bisherigen Wiederherstellungsbefund die
Sitzung. Baselineabweichung und nicht auswertbare Details bleiben sichtbar unknown.

Gate: menschlicher Skriptlauf, anschließend ein Lagerbaustellenfall mit freier
Vorschau und der bisherigen breiten Sperrkontrolle. Erwartet werden acht getrennte
nachvollziehbare Befunde, korrekte Kandidatenbelegung, gleiche Baseline und
Wiederherstellung. Erfolg darf auch eine klare Erklärung der voneinander
abweichenden Reichweiten-/Verbindungsbefunde sein; kein erfundener Zugangsverlust.
Bei fehlenden Daten oder unklarer negativer Kontrolle nach diesen zwei Fällen
stoppen und Ursache auswerten. Keine Baufreigabe, weiterhin kein Abschluss von B.

## Detail-Livebefund 2026-10-03

Sauberer Ausgangsstand ohne Baustellen. Der vorhandene kleine Lagerpilot legt
genau einen Anschlussweg und eine echte Lagerbaustelle an; Auftrag und direkter
Builderzugang separat bestätigt. Danach genau zwei Kontrollvorschauen.

Freier Weg: gültig; genau eine Kandidatenzelle mit Belegungsart Path, andere
Zugänge ohne Kandidatenbelegung. Acht detaillierte Zugänge: sechs im Reichweitenfeld,
zwei erhöhte Zugänge außerhalb. Breite ungültige Zentralesperrvorschau: alle acht
Zugänge durch Kandidatenblöcke belegt, obere zwei mit All, untere sechs mit
Bottom/Top/Corners/Path/Middle. Trotzdem unveränderte Reichweitenmitgliedschaft;
elf Distriktverbindungen verloren, kein aggregierter Baustellenverlust behauptet.

Alle acht Verbindungswerte sind bereits vor jeder Vorschau false und bleiben
false, während der reale Builderzugang true ist. Der Vergleich vom gewählten
Zentralenursprung ist damit keine geeignete positive Zugangskontrolle. Ursache
(Start-/Zielkoordinaten oder Abfragesemantik) noch nicht nachgewiesen. False darf
nicht als tatsächliche Unerreichbarkeit der Baustelle interpretiert werden.
details.status=observed bezeichnet nur vergleichbare Werte und Rücknahme, nicht
eine erfolgreiche Verbindung oder bestätigten Baustellenschutz.

Beide Detail-/Aggregat-Baselines und Rücknahmevergleiche bestanden; keine
Sitzungssperre. Reale Erreichbarkeit unabhängig erneut true, Pause und Spielzeit
unverändert. Testfixture bleibt vorhanden. Detailausgabe und Grenzen damit
praktisch nachgewiesen; Etappe B insgesamt weiterhin offen. Keine weitere
Suchserie. Nächster fachlicher Schritt: native Start-/Zielgültigkeit und geeigneten
positiven Verbindungsbezug prüfen, bevor diese Abfrage Zugangsschutz tragen darf.

## Endpunktdiagnose 0.31.2 — positive Kontrolle gescheitert

Die Verbindungsabfrage verwendet jetzt den tatsächlichen Eingang der
Bezirkszentrale (`originSource=district_entrance`). Die Reichweitenabfrage bleibt
am Bezirkszentrum (`rangeOrigin`); beide Koordinaten sind getrennt sichtbar.
`originOnDistrictRoad` prüft den Start zusätzlich am tatsächlichen Bezirksweg.
Alle fünf Zugangszustände enthalten `originOnActualNavMesh` und
`targetOnActualNavMesh`. Diese Werte werden auch bei Baseline und Rücknahme
verglichen. Die öffentliche Schnittstelle bietet hier nur tatsächliche
Knotengültigkeit, keine entsprechende Vorschauabfrage; die Felder behaupten
deshalb keine Gültigkeit auf dem Vorschau-NavMesh.

Fehlender Eingang ergibt `detail_origin_unavailable`; ein Start ohne tatsächlichen
NavMesh-/Bezirkswegbezug ergibt `unknown` mit `detail_origin_invalid`. Rohe
Verbindungswerte bleiben Diagnosewerte und sind kein Builder-Zugangsnachweis.
Insbesondere wurde die Ursache der bisherigen false-Werte noch nicht bewiesen.

Nach dem menschlichen Skript-Gate zuerst an einer tatsächlich erreichbaren
Baustelle eine positive Verbindungskontrolle prüfen. Nur bei brauchbarem Start
und positivem Verbindungsbezug höchstens eine freie und eine Sperrvorschau
vergleichen, einschließlich Rücknahme und unabhängiger Builder-Abfrage.
Bei ungültigem Start oder fehlender positiver Kontrolle stoppen und den Befund
auswerten; keine weitere Kandidatensuchserie. Etappe B bleibt offen und
`constructionCovered=false` unverändert.

0.31.2 ist nach menschlicher Bereitmeldung strukturiert erreichbar. Im geladenen
Stand fehlt eine Baustelle. Ein begrenzter Lagerplan liefert einen Kandidaten,
den die Spielvorschau als ungültig ablehnt; kein registrierter Pilotauftrag.
Ein kleiner Alternativbereich liefert keine Option. Einrichtung gestoppt,
keine Endpunktwerte geprüft. Für den angekündigten Nachweis wird eine erreichbare
kleine Lagerbaustelle benötigt; Commit und Etappenabschluss bleiben aus.

Der Nutzer stellte anschließend eine kleine Lagerbaustelle bereit und markierte
sie. Auswahlauflösung erfolgreich, Baustelle unfertig mit realem Builderzugang
true. Eine einzelne freie Path-Vorschau liefert alle acht Zugangszustände:
Start auf tatsächlichem NavMesh und Bezirksweg, alle acht Ziele auf tatsächlichem
NavMesh, vier Ziele im Reichweitenfeld. Dennoch connected und roadConnected
für alle acht Ziele in sämtlichen fünf Zuständen false. Baseline und Rücknahme
bestanden, echter Builderzugang danach weiterhin true, Pause/Spielzeit unverändert.

Damit erklärt ein ungültiger Start/Zielknoten die beobachteten false-Werte in
diesem Kontrollfall nicht; die genaue Ursache bleibt offen. `observed` bestätigt
vergleichbare Rohdaten, nicht eine erfolgreiche Zugangskontrolle. Positive
Kontrolle gescheitert, keine anschließende Sperrvorschau und kein Commit.
Weitere Klärung der öffentlichen Abfragesemantik bzw. ein alternativer Prüfweg
benötigt nach dem gestoppten Pilot ein neues ausdrückliches go. Etappe B bleibt
offen; keine Baufreigabe.

## Gezielte Semantikprüfung nach erneutem go

Öffentliche Metadaten zeigen getrennte Schichten: `INavMeshService.AreConnected…`
nimmt Rasterknoten; Road-/Terrain-Graphen führen gleichnamige Methoden neben
ConnectNodes, DisconnectNodes und GetNeighbors. Das spricht für direkte
Kantenbeziehungen, ist ohne Methodenkörper-/Laufzeitvergleich aber nur eine
begründete Hypothese, kein abschließender Semantiknachweis.

`INavigationService` bietet ausdrücklich FindInstantRoadPath,
DestinationIsReachable und FindRoadSpillOrTerrainPathUnlimitedRange. Der
bereits vorhandene MCP-Leser mit Accessible.FindInstantRoadPath liefert live
für Zentrale zu einem fertigen kleinen Lager supported=true, connected=true.
Die tatsächliche Pfadsuchschicht hat damit eine positive Kontrolle; dies ist
ein anderer Zielpunkt und kein Ersatz für die Baustellen-Vorschaukontrolle.

`IDistrictService` bietet IsOnInstantDistrictRoadSpill für Accessible und
Weltposition, jedoch keine entsprechende öffentliche Preview-Spill-Methode.
Die bekannte Reichweitenschnittstelle besitzt tatsächliche und Preview-Spill-
Listen. Die öffentliche Navigationsschnittstelle enthält keine explizite
Preview-Pfadsuche. Tatsächliche Pfadsuche während einer Vorschau darf deshalb
nicht als Vorschau-Erreichbarkeit ausgegeben werden.

Empfohlener nächster begrenzter Schritt: direkte Nachbarkante gegen entfernten
Punkt getrennt kontrollieren und tatsächliche Road-Spill-/Pfadsuchbefunde gegen
den realen Builderzugang vergleichen. Erst nach brauchbarer Baseline eine
gezielte Engpasskontrolle der Preview-Reichweite, keine Bauplatzsuchserie.
Keine eigene BFS, privaten Graphzugriffe oder neue Abhängigkeit erforderlich.
Die bisherige Verbindungsdiagnose bleibt Rohbefund, keine Baufreigabe.

## 0.31.3 — begrenzte Kontrolldiagnose live belegt

Menschliches Skript-Gate durch Bereitmeldung bestätigt; installierte Version
und begrenzte Kontrolldiagnose live geprüft.
Die fünf Einzelzustände ergänzen:

- `originNeighbourRoadEdges`: Zahl positiver AreConnectedRoad-Abfragen vom
  Zentraleingang zu zwölf festen Nachbarpositionen (vier horizontale Kardinal-
  richtungen, jeweils Höhe -1/0/+1). Tatsächlicher Graph in before/after,
  Vorschaugraph in previewBefore/during/previewAfter; Kartenrand ausgeschlossen.
  Keine vollständige Nachbarinventur, keine eigene Pfadsuche, keine Routenbehauptung.
- `actualRoadPathFound`: öffentliche FindInstantRoadPath zwischen Rasterzellmitten.
- `actualDestinationReachable`: öffentliche DestinationIsReachable zwischen
  denselben Punkten; nicht gleichbedeutend mit Bauarbeiterzugang.
- `actualDistrictRoadSpill`: öffentliche IsOnInstantDistrictRoadSpill für den
  Zielpunkt. Die API fragt beliebige Bezirke ab; Pilot weiterhin nur ein Bezirk.

Die drei actual-Felder lesen ausschließlich tatsächliche Navigation, auch in
`during`. Sie beweisen keine Vorschauwirkung. Alle neuen Felder gehen in
Baseline-/Rücknahmevergleiche ein; .3-Backend verlangt vollständige, begrenzte
Kontrolldaten und deren ausdrückliche Ist-Zustandskennzeichnung. Synthetische
Tests decken fehlende Werte, Grenzen, gemeinsame Startpunktdaten, Rücknahme und
fehlende Baufreigabe trotz positiver Ist-Pfadsuche ab; menschliches Skript-Gate
bestätigt, neue Testanzahl nicht übermittelt.

Abnahme: eine reale erreichbare Lagerbaustelle, eine freie Kontrollvorschau.
Erwartet wird mindestens eine positive Nachbarkontrolle und ein brauchbarer
positiver Ist-Pfad-/Spillbezug bei weiter negativen Fern-Kantenwerten; Spill-
Gesamtbefund separat gegen echten Builderzugang prüfen. Bei fehlender positiver
Kontrolle stoppen. Nur bei passender Baseline höchstens eine gezielte Engpass-
Vorschau samt Rücknahme prüfen. Kein Etappenabschluss allein aus Diagnosewerten,
kein `constructionCovered=true` und kein Commit vor bestandenem Abschluss-Livetest.

### Ergebnis

Eine markierte reale Lagerbaustelle, genau zwei Vorschauen:

- Freie Path-Vorschau gültig. Startkontrolle drei positive Nachbarkanten in allen
  fünf Zuständen. Drei der acht Ziele mit tatsächlichem Straßenpfad, vier mit
  tatsächlicher Erreichbarkeit und Spill; genau diese vier im Reichweitenfeld.
  Alle acht Fern-Kantenwerte false. Echter Builderzugang vor/nachher true.
- Überlappende Bezirkszentralen-Vorschau bewusst ungültig. Alle vier erreichbaren
  Zielpunkte vom Kandidaten belegt, sieben verlorene Bezirksverbindungen,
  trotzdem alle vier weiter im Preview-Reichweitenfeld, lostSites=0. Tatsächliche
  Abfragen erwartungsgemäß unverändert. Dies ist kein gültiger Nachweis einer
  tatsächlichen Baustellenblockade. Kandidatenbelegung allein genügt nicht,
  um native Zugangssperre oder korrektes Preview-Verhalten zu behaupten.

Beide Detail-/Aggregat-Baselines und Rücknahmen bestanden, keine Sitzungssperre;
echter Builderzugang unabhängig danach true, Pause/Spielzeit unverändert.
Nachbar-/Pfad-Kontrollen belegen im geprüften Fall den Unterschied zwischen
roher Kantenabfrage und echter Route. Kein vollständiger Semantikbeweis für
sämtliche NavMesh-Methoden. Diagnosescope abgeschlossen und sicherbar, Etappe B
weiter offen. Keine weitere Suchserie. Nächster fachlicher Schritt: gültige
Blockadekontrolle mit belegter tatsächlicher Negativbaseline; keine weiteren
überlappenden Zentralen als Ersatz für den Baustellen-Schutznachweis.
