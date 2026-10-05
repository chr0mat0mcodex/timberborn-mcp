# Backlog

## Beauftragt: automatische Teilflächenwechsel in Bauchargen

Go nach zwei räumlich ausgeschöpften Chargen. Basis plus drei zusätzliche
8×8-Flächen implementiert, globale Räum-/Zeitgrenzen und gespeicherte Wechsel.
Menschliches Gate und begrenzter Flächenwechsel-Livetest bestanden. Details:
docs/building-batch-workflow.md. Globale gemeinsame Standortoptimierung bleibt offen.


## Chargenplanung: Alternativen überlappen (Live 2026-10-05)

Drei-Häuser-Charge baute das erste Haus fertig/erreichbar und stoppte dann korrekt
mit no_candidate_in_authorized_region: regionale Alternativstandorte waren
nicht gleichzeitig nutzbar. Verbesserung: vor Chargenstart Anzahl gemeinsam
nutzbarer Standorte bzw. Konflikte ausweisen, keine Kapazität aus Kandidatenzahl
ableiten. Fortsetzung mit begrenzter bestehender Räumcharge, keine neue Funktion.

## Produktionsflächen: Feuchte mit Arbeitsreichweite verbinden

Optionales Arbeitsgebäude und abschaltbare Bauplanung in survey_region vorbereitet.
Pilot mit drei tatsächlich wachsenden Karotten bestätigt Nutzen des Abgleichs.
Menschliches Gate und kombinierter Livetest bestanden; Bridge unverändert 0.35.4.
Grenze: bis 64 interne Reichweitenseiten; spätere native Regionsfilterung kann
auch diese internen Reads reduzieren. Keine Feld-/Forstautonomie behauptet.


## Versionswechsel: vergessene Clientfreigabe verhindern

0.35.4 zunächst von den eigenen Versionslisten abgelehnt. Korrektur aller
betroffenen Listen und manifestgebundener Clienttest ergänzt; Gate und Livetest bestanden.
Weitere Verbesserung: verteilte Versionslisten konsolidieren und inkompatible
Bridge-Version auch im Aktivitätslogging als verständlichen Fehler ausgeben;
aktuell generischer MCP-Aufruffehler. Kein größerer Umbau in dieser Reparatur.


## Direkter Nutzerauftrag: grüne Pflanzflächen erkennen

0.35.4 vorbereitet: Bodenfeuchte für leere Felder in inspect_map_region und
survey_region; öffentliche SoilIsMoist-Abfrage statt Ableitung aus Pflanzenstress.
Separate kompakte Feuchtigkeitskarte und freie feuchte Rechtecke, keine zusätzlichen
Spielabfragen. Menschliches Gate und feucht/trocken/Höhen-Livekontrolle am 2026-10-05 bestanden.
Pflanzenspezifische Eignung und Arbeitsreichweite bleiben separate Aussagen.


## Live bestanden: regionale unknown-Ausbreitung

Ein other-Objekt sperrte bisher freie Zellen der gesamten Suchregion.
Auf tatsächlich überlappende Teilflächen begrenzt, zwei Regressionsfälle ergänzt.
Menschliches Gate und Live-Gegenprobe am 2026-10-05 bestanden: betroffene
Teilfläche bleibt unbekannt, freie Rechtecke anderer Teilflächen wieder sichtbar.


## Bedienungsdiagnose aus regionalem Baupilot

- waitSeconds-Grenzen direkt in Kurzbeschreibung nennen: Einzelprojekt 0–20,
  Charge 0–45. Metadatenschema allein verhindert Parameterverwechslungen nicht.
- Für develop/advance/inspect_building_completion implementiert, Gate und begrenzter Livetest bestanden: vor Eingriff erkannte Parameterfehler konkret benennen; pauschaler Hinweis
  auf möglicherweise unbestätigte Aktion verursacht unnötige Diagnoseabfragen.
- Regionaler Baukandidat inzwischen bis finished_accessible praktisch bestätigt;
  nächste wirtschaftliche Grenze sind Holz- und Nahrungsproduktion.


## Priorität: regionale Flächensuche — Vorschlag nach Ausbaupilot

Rund 40 MCP-Aufrufe für zwei Wohnhäuser, eine Transportstation und sieben
Feldzellen. Regionale Suche soll Gelände, Bestandszugänge und geeignete Bau- und
Anbauflächen gebündelt ausweisen, statt blindes Räumen oder einzelne 8x8-Suchen
zu skalieren. Kandidaten bleiben Vorschläge; native Bau-/Zugangsprüfung bleibt
verbindlich. Keine Gebäudefertigstellung, Pflanzung oder Ernte aus Planung ableiten.
Nach ausdrücklichem Go implementiert und nach menschlichem Gate begrenzt live
abgenommen: 32 native Leseaufrufe, vier Kandidaten, Bestands-/Feldkontrollen
korrekt; laufendes Spiel abgelehnt. Keine allgemeine Bau-/Anbaufreigabe.
Umfang und Grenzen: [regionale Flächensuche](docs/regional-survey.md).
Chargenantworten weiter kürzen: unveränderte Suchdetails/alte completion-Werte
nur bei Bedarf; wartender Status braucht nächsten sinnvollen Abfragezeitpunkt.


## Live bestanden — temporäre Spielsperren in Zeitläufen (2026-10-04)

Menschliches Test-/Installationsgate abgeschlossen; alle fünf installierten
Paketdateien stimmen mit Paket 0.35.3-20261004-231320-8e75fd0c überein.
Gezielter Vier-Tage-Lauf: beim vom Nutzer ausgelösten Speichern dreimal
running/game_speed_locked mit Tempo 0 beobachtet; danach derselbe Lauf wieder
running/advancing mit Tempo 7. Ziel und Echtzeitbudget unverändert.
Abschluss nach exakt 96 Spielstunden und 271,166 Echtzeitsekunden: completed,
target_reached, pauseConfirmed, Überschwingen 0. Aktuelles Tempo separat 0.

Negativkontrolle: ausdrückliche MCP-Pause unterbricht einen zweiten aktiven Lauf
mit explicit_speed_change; aktuelles Tempo danach separat 0 und Spielzeit stabil.
Der eingefrorene Unterbrechungsbeleg enthält noch Tempo 7 vor Befehlswirkung;
deshalb die aktuelle Pause immer gesondert lesen.

Öffentliches SpeedLockChangedEvent über EventBus angebunden; keine Entsperrung
oder Wiederbeschleunigung durch die Bridge. Sperre beim Speichern jetzt direkt
nachgewiesen; eine rückwirkende eindeutige Ursache aller früheren Abbrüche ist
nicht beweisbar. Budgetablauf/Abbruch während Sperre synthetisch geprüft, im
Livetest nicht zusätzlich provoziert. UI-Geschwindigkeitswechsel separat nicht
live getestet. Gesamtziel bleibt offen: zuletzt 27 Biber, 23/157 Vorlagen.

## Live bestanden — Bauchargen (2026-10-04)

Menschliches Gate mit anschließendem „live bereit“; fünf installierte Paketdateien
gegen das neue Paket geprüft. Zwei Gebäude automatisch nacheinander fertig und
zugänglich: kleines Lager mit Carrot/obtain, danach Mini-Wohnhaus. Zehn tote Bäume
automatisch geräumt, sechs lebende erhalten. Räumwartefenster tatsächlich gestartet;
bei der unabhängigen Rückfrage waren die Ziele bereits entfernt. Drei native
Zeitfenster je exakt 24 Stunden, kein Überschwingen, Pause jeweils bestätigt.
Fünf ausführende Chargenaufrufe bis Abschluss; Replay/Budgetkontrolle zusätzlich.
Identischer Start und abgeschlossene Fortsetzung unverändert, Budgetänderung
abgelehnt. Negative bebaute 1×1-Fläche stoppt vor Bau/Räumung und Folgegebäude.
Unabhängige Zugangs-/Lagerabfragen stimmen mit Abschluss überein. 21 Betten,
17 belegt; kein Nachweis für 100 zufriedene Biber oder neue Vorlagenabdeckung.

Grenzen: kein erzwungener Prozessabbruch im Livetest; Materialmangel und erschöpftes
Räumbudget synthetisch abgedeckt, hier nicht zusätzlich live provoziert.
Baustellenkonfiguration in diesem Pilot erst nach Fertigstellung zurückgelesen.
Verbesserungen: vollständiges Räumbudget läuft trotz früher geräumter Ziele aus;
Budgetkonflikt meldet zu allgemein invalid_argument; gespeicherter completion-Wert
kann während Räumung noch zum vorherigen Gebäude gehören. Keine falsche Aktualität
behaupten: checkpointOnly und Zeitstempel beachten. Breitere Vorlagen-/Höhenplanung
und kleinere gezielte Räumflächen bleiben offen.


## Vor Test-Gate — Standortwahl und komplette Bauchargen

Erneutes Nutzer-Go: 1–8 Gebäude, begrenzte Fläche, Rotationensuche, einmalige
Vegetationsräumung mit festem Zeitbudget und sequenzielle Fertigstellung als
MCP-Ablauf implementiert. Persistente Wiederaufnahme und kompakte Checkpoints;
kein neuer Hintergrundprozess. Synthetische Prüfungen ergänzt, Ausführung und
Livetest stehen aus. Räumung umfasst bei Bedarf alle freigegebenen passenden
Ziele der Region; minimale Räumfläche und Höhenplanung bleiben offen.
[Pilot und Grenzen](docs/building-batch-workflow.md).

## Live bestanden — vollständiger einzelner Bauablauf

Reparatur nach menschlichem Gate bestanden: Lager plus zwei Wege fertig,
Zugang und Konfiguration bestätigt, Zeitbudget exakt und Replay ohne neue Zeit.
Explizite Komponentenabwesenheit wird korrekt von unknown unterschieden.

Erneutes Nutzer-Go nach Aufwandsalarm erhalten. Gesamtaufruf für Planung,
Baustart, begrenzte Simulation und gebündelte Abschlussprüfung implementiert;
Wiederaufnahme über vorhandene native IDs, keine automatische Budgetverlängerung.
Suchursprungdiagnose bei fehlendem Kandidaten. Menschliches Gate und gezielter
Livetest bestanden. Mehrgebäude-Warteschlange, automatische
Räumplanung und vollständige Ursachenaufteilung der Suchregion bleiben offen.
[Abnahmeplan](docs/building-completion-workflow.md).

## Live bestanden — gebündelte Bau- und Räumaufträge

Nach ausdrücklichem Nutzer-Go: zwei MCP-Werkzeuge über bestehenden nativen
Services, ohne neue Abhängigkeit. Räumstapel bis 16
Vegetationsziele mit Einzelbelegen/Stopp beim ersten Fehler; Baustart bündelt
Planung und native Pilot-Ausführung des ersten Kandidaten. Bestehende Rechte,
Zugangsprüfungen und initiale Lagerkonfiguration bleiben wirksam.
Sammelräumung und korrigierter Konfliktstopp live bestanden; explizites
state_conflict und unverändertes Folgeziel bestätigt. Gebündelter Baustart
live bis zum fertigen erreichbaren Lager samt erhaltener Konfiguration geprüft.
Drei Ziele in einem Aufruf und Baustart ohne manuelles Kopieren von
planKey/optionIndex. Mehrere Gebäude
in einer Bauwarteschlange, automatische Simulation und Sammelräumung anderer
Objektarten bleiben Folgearbeit; keine allgemeine autonome Ausbaupipeline behauptet.

## Weitere Live-Lücken beim Kolonieausbau

- Räumaufträge in Bauablauf integrieren: gleicher Vegetationsbatch kann sofortige
  Entfernung und offene Arbeiteraufträge mischen. Live blieben vier tote Kiefern
  zunächst markiert; nach einem begrenzten Spieltag waren alle entfernt. Bau
  darf erst nach tatsächlicher Räumung starten. Status und Zeitbudget dieser
  Vorbereitung mitführen, keine Wiederholung der ursprünglichen Markierung.
- Generische erhöhte Gebäudeeingänge: DoubleLodge/TripleLodge sind im Katalog
  als geometrisch unterstützt ausgewiesen, ihr Eingang liegt aber eine Ebene
  über dem Ursprung. Ebener Projektablauf reicht nicht; vorhandene feste
  Vertikalpiloten sind noch kein allgemeiner Anschluss solcher Wohngebäude.
- Suchbereich vorab gegen gedrehte Grundfläche und Wegeingang erklären:
  Forsthausursprung geometrisch frei, aber Grundfläche teilweise außerhalb der
  angegebenen Region. Erst eine zusätzliche Spalte ermöglicht den Kandidaten.
  Ursprungsdiagnose meldet requires_game_validation und erklärt diesen Planer-
  Ausschluss nicht. Konkrete benötigte Suchgrenzen bzw. Ablehnungszähler ausgeben.
- Flächenvorprüfung mit konkreten blockierten Zellen und Gründen: `set_area`
  meldet bei besetzter Pflanzfläche nur `invalid_argument` und einen pauschalen
  Hinweis auf möglicherweise unbestätigte Wirkung. Live: Kiefern vor der Farm,
  keine Markierungen verändert. Validierungsfehler vor Mutation von tatsächlich
  unbestätigten Änderungen unterscheiden; `precheck_area` vorsehen.
- Bauplanung ohne Kandidaten: Ablehnungsgründe aggregieren (Gelände, Objekte,
  Anschluss, Wegbudget), damit kein Suchraster durchprobiert werden muss.
  Erneut live: zwei 8x8-Suchen für Lagerfeuer mit je 64 Ablehnungen ohne
  Ursachenaufteilung. Gezielte Gelände-/Objektabfrage zeigte neun tote Kiefern
  auf ebener 3x3-Fläche; nach Räumung genau ein Kandidat ohne neue Wege.
- Begrenzte Vegetations-Sammelaufträge mit Einzelbelegen und Konfliktprüfung:
  neun tote Bäume erforderten neun serielle MCP-Aufrufe. `operation=mark`
  lieferte hier unmittelbar `removed=true, marked=null`; Antwort und Beschreibung
  sollten direkte Entfernung klar von einem noch offenen Arbeiterauftrag trennen.
- Räumliche Filter für Fäll-/Pflanzmarkierungen und Reichweiten: zwanzig lokale
  Fällfelder erfordern derzeit den Abgleich von 190 kolonieweiten Markierungen.
- Fortschrittsbericht: lebende Bevölkerung, beobachtete Grundbedürfnisse und
  echte Wohlbefindenspunkte sowie gebaute/fehlende Katalogvorlagen gemeinsam.
  Vorhandener Fertigbestand ist kein vollständiges historisches Bauregister.
- Kompakte Bevölkerungsdiagnose: Altersgruppen, Geburten/Todesfälle im Intervall,
  belegte Familienwohnungen und tatsächliche Wohlbefindensboni. Einzelne
  Bestandsänderungen (11 → 10 → 11) erklären weder Ursache noch Wachstumsrate;
  keine Todesursache oder allgemeine Zufriedenheit aus Bedürfnisflags ableiten.
- Energieplanung: tatsächliche Anschlusspunkte, Netzzugehörigkeit, Erzeugung und
  Bedarf strukturiert anbieten. Sägewerk/Laufrad liefern live Bretter, aber die
  Verbindung ist bisher erst über Produktion und verschwundene Statusmeldung
  nachgewiesen; Vorabplanung kennt nur Gebäudegeometrie und Wegeingänge.

## Kurzfristig priorisiert — kompakte MCP-Antworten (P1)

Folgeschritt live bestanden: `inspect_colony_overview` ersetzt mehrere
MCP-Abfragen durch eine begrenzte lesende Zusammenführung, ohne weniger interne
Bridge-Abfragen oder Atomizität zu behaupten. Bauprüfung entfernt weitere
Zell-/Quellenangaben, erhält separate Unknowns und Nachweisgrenzen.
Nutzen im pausierten Spiel belegt: ein statt drei MCP-Aufrufen, 60 % weniger
Antwortzeichen bei gleichen relevanten Daten und allen 42 Bedürfnissen.
Bauprüfung 16 % kürzer, vollständige Projektion fachlich gleich.
Allgemeine Ausbauabläufe und weitere Werkzeuggruppen bleiben offen.

Nutzerauftrag vom 2026-10-04: zeitnah umsetzen, direkt nach dem laufenden
0.35.3-Test-Gate. Standardantworten sollen nur das für die konkrete Aktion oder
Entscheidung Nötige enthalten. Im Playtest wiederholen sich lange `limitations`,
verschachtelte Prüfberichte, Katalog-/Fähigkeitsbeschreibungen und vollständige
Objektzustände auch bei einfachen Status- und Einstellungsabfragen.

- Kompakte Standardansicht: Ergebnis, relevante Werte/Änderungen, notwendige IDs,
  konkrete Fehler oder Blockierungsgründe. Unbekannt/unbestätigt und wesentliche
  Nachweisgrenzen müssen eindeutig bleiben.
- Statische Erläuterungen einmal über Fähigkeiten/Dokumentation bereitstellen;
  ausführliche Geometrie, Prüfbelege und vollständige Objektdetails gezielt abrufbar
  machen. Keine wiederholten vollständigen Zustände bei kleinen Änderungen.
- MCP-Text und `structuredContent` auf unnötige Doppelübertragung prüfen;
  Protokoll- und Client-Kompatibilität erhalten. Lange Werkzeugbeschreibungen
  ebenfalls auf Wiederholungen und veraltete Aussagen prüfen.
- Kleiner Pilot mit Status, Lagereinstellung und Bauprüfung: Antwortgröße vorher/
  nachher messen, deutlich reduzieren und dieselben Entscheidungen ermöglichen.
  Fehler-, Konflikt- und Unknown-Fälle sowie abrufbare Details mitprüfen;
  erst danach auf weitere Werkzeuge ausweiten.

Status: erster Pilot nach menschlichem Gate live abgenommen:
Status, Lagerlesen/-änderungen und generische Bauprüfung mit optionalem
`detail=full`. MCP-Text bleibt aus Kompatibilitätsgründen inhaltlich gleich zur
strukturierten Antwort; beide tragen dieselbe gekürzte Projektion. Gebündelte
Ausbauabläufe und weitere Werkzeuggruppen folgen nach gemessenem Pilotnutzen.
Lagerlesen live 53 % kürzer; Bauprüfung nur 7 % kürzer. Dort bleibt der Großteil
in verschachtelten Diagnosebelegen: nächster Schritt ist eine kompakte fachliche
Zusammenfassung mit vollständigen Belegen auf Abruf, keine pauschale Feldlöschung.
Kein Aufschub hinter den vollständigen Gebäudekatalogausbau.

## Offene Folgearbeiten nach Phase E

Phase E ist mit 0.35.1 im vereinbarten begrenzten Umfang abgeschlossen.
Die folgenden Punkte sind eigenständige Folgearbeiten, keine offenen
Abschlussbedingungen von E und keine pauschale Baufreigabe.

| ID | Offener Punkt | Erforderlicher Nachweis / Grenze |
| --- | --- | --- |
| F01 | Vollständiger Weg- und Bauphasenschutz | Bestehende Baustellenzugänge unter geplanten Änderungen und Zugang neuer Baustellen vorab belastbar prüfen; positive und negative Kontrollen. Bis dahin unabhängige offene Baustellen ausschließen und `unknown` nicht als sicher behandeln. |
| F02 | Umweg als Schutz für eine spätere Bestandsänderung | Neuen Umweg zuerst real fertigstellen und seine Nutzbarkeit prüfen, dann die davon abhängige Änderung einschließlich betroffener Bestandszugänge testen. Der bestandene Anschluss um einen Baum ersetzt diesen Nachweis nicht. |
| F03 | Freie Höhenplanung und größere Bauprojekte | Bedarfsgerecht über feste Treppen-/Plattformfolgen, 8x8-Suche und vier neue Bodenwege hinausgehen; Abhängigkeiten, Bauphasen und Bestandszugänge für den erweiterten Umfang prüfen. Keine automatische Erhöhung der aktuellen Limits. |
| F04 | Besondere Gebäude- und Eingangsfälle | Direkt-fertig-Distriktzentrum mit eigenem Distrikt-Lebenszyklus gezielt live prüfen. Mehrere echte Gebäude-Eingänge erst bei belegter Spiel-API unterstützen; Bauzugangszellen sind keine Türen. |
| F05 | Breitere Kompatibilitäts- und Katalogabdeckung | Weitere relevante Geometrien, Fraktionen, Karten und Spielversionen mit repräsentativen Fällen prüfen. Generische Unterstützung ist keine pauschale Live-Abnahme aller Kataloggebäude; keine vollständige Vorlagen-/Drehungsserie ohne konkreten Bedarf. |

Weitere bereits erfasste Projektaufgaben stehen unter „Technische Lücken“;
Versorgung und Betrieb bleiben eine separate Folgestufe. Priorisierung des
nächsten Arbeitspakets erfolgt eigenständig, nicht als Fortsetzung einer offenen
Phase E. Abschlussumfang und Nachweise: [Missionsplan](missionsplan.md#abschluss-phase-e).

## Abgeschlossene Schritte und historische Priorisierung

Abgeschlossen: 0.35.1, Bauablauf und isolierte Überbudgetkontrolle live bestanden.
Vier Vorschläge nicht mit überlangen Anschlussprojekten füllen; vorhandene Wege
bevorzugt verwenden. [Abnahmekriterien](docs/road-budget-planner.md).
Historische Bank-/LargePile-Nachweise im geladenen MCP-Profil live bestätigt.
Fünf-Wege-Kandidat verworfen, spätere 4/3/2/1-Wege-Kandidaten gefunden;
keine weitere Grenzfallserie nötig. Sechs Wege für den reinen Planungstest
entfernt; Teststreifen bleibt offen, Simulation pausiert.

Generischer ebener Projektpilot 0.35.0 nach menschlichem Gate live bestanden:
Bank mit zwei neuen Wegen und großes 3x3-Freiluftlager mit einem Weg, beide
Drehung 1, mit realem Bau-/Fertigzugang, erhaltenen Bestandsanschlüssen und Replay.
Einzelne Kollision am äußeren Eckfeld des großen Grundrisses nativ abgelehnt.
Höhen-/Blockadekontrolle bestanden. Gefährliche Vorschau
erkannte 24 verlorene Verbindungen und wurde restauriert. Keine Vorlage mehr
einzeln freischalten. [Nachweis](docs/generic-building-project.md).

Offen: Direkt-fertig-Distriktzentrum mit eigenem Distrikt-Lebenszyklus nicht
live geprüft; Mehrfacheingänge spielseitig nicht öffentlich beobachtet. Keine
pauschale Katalogabnahme. Bank-/LargePile-Historie ist seit 0.35.1 im Profil.
Weitere Arbeit nach Missionsplan auf konkrete Lücken bei Weg-/Bauzugangsschutz
ausrichten, einschließlich Umwegfall; keine Drehungs-/Vorlagenserie ohne Bedarf.

### Ausgangspunkt vor 0.35.0

E-Schritt 0.34.0 live abgeschlossen, Lodge.Folktails im ebenen Bauprojekt.
Gedrehter Wohnhausablauf mit zwei neuen Wegen, Grundriss-Negativkontrolle,
realem Bau-/Fertigzugang, Bestandsanschlüssen und Replay bestanden.
Keine neue Vier-Drehungs-Serie und keine Lockerung der Schutzgrenzen.
[Nachweis](docs/lodge-project-pilot.md).
Bei nächstem ohnehin nötigen MCP-Gate Wohnhaus-Drehung 3 in TemplateEvidence
aufnehmen; aktuelles ausgeliefertes Profil beschreibt den Historienstand vor
dieser Abnahme. Keine Neuinstallation allein für diese Metadaten.

Aktiver Schritt abgeschlossen: 0.33.1 mit vier gemeinsamen
sequenziellen Vertikalprojektplätzen und aktualisiertem MCP-Nachweisprofil.
Nach menschlichem Gate zwei Projekte in derselben Sitzung fertiggebaut,
Baustellensperre und Replay bestanden. Nächsten E-Ausbau an einer konkreten
Fähigkeitslücke ausrichten; keine weiteren identischen Drehungstests nötig.
[Nachweis](docs/vertical-sequential-pilot.md).

### Ausgangspunkt vor 0.33.1

Vertikales Lagerprojekt zusätzlich in Treppendrehung 0 / Lagerdrehung 2 live
bestanden auf unveränderter 0.33.0. Sieben fertige Objekte, Bau-/Fertigzugang,
Bestandsanschlüsse und Replay geprüft. Drehung 2 / Lagerdrehung 0 ebenfalls
mit sieben fertigen Objekten, Bau-/Fertigzugang, Bestandsanschlüssen und Replay
live bestanden. Drehung 1 / Lagerdrehung 3 ebenfalls bestanden; damit alle vier
Treppendrehungen des festen Lagerprojekts begrenzt belegt. Derzeitiges
MCP-Profil enthält weiter nur historischen Nachweis 3. Profilaktualisierung
als MCP-only-Änderung mit eigenem Gate bündeln, keine Modneuinstallation nur
für weitere Tests unveränderten Codes. Pro Sitzung nur ein Vertikalprojekt;
weiterer Start braucht eine frische Sitzung. Nächster sinnvoller Code-Schritt:
begrenzte sequenzielle Vertikalprojekte bei erhaltenem Replay-/Sitzungsschutz und
frischen Zugangsprüfungen, dazu aktualisiertes Nachweisprofil. Kein paralleler
Bau oder Lockerung der Baustellensperre. [Nachweis](docs/vertical-warehouse-pilot.md).

Strukturierte Projektumfangsabfrage nach menschlichem Gate live bestanden:
fünf Modi, Grenzen und historische Nachweise; keine Modversionsänderung oder
allgemeine Baufreigabe. [Nachweis](docs/building-capabilities.md).

Nutzerpriorität: B minimal vor E. 0.32.1 nach menschlichem Gate live bestanden:
neues Projekt/Einzelplatzierung bei unabhängiger Baustelle ohne neue Objekte
abgelehnt; nach deren Fertigstellung eigener Plattformablauf vollständig gebaut.
B im freigegebenen Ausschlussumfang geschlossen, allgemeiner Vorschau-Schutz
weiter ungelöst. Erster E-Schritt auf unveränderter 0.32.1 live bestanden:
Rotation 0/2 des kleinen ebenen Lagers, Baustellen- und Fertigzugang sowie
gemeinsamer Bestandsweg geprüft. [Nachweis](docs/building-rotation-pilot.md).
0.33.0 nach menschlichem Gate live bestanden: mittleres ebenes Folktails-Lager
mit Rotation 3 und sechs Grundrisszellen, Bau-/Fertigzugang und Bestandsanschlüsse.
[Nachweis](docs/medium-warehouse-pilot.md). Ergänzung auf unveränderter 0.33.0:
Rotation 1 mit drei tatsächlich neuen Bodenwegen, fertigem Lager, Baustellen-/
Fertigzugang und erhaltenen Vergleichsanschlüssen live bestanden. Drehungen 0/2
ebenfalls fertig und erreichbar mit erhaltenen Bestandsanschlüssen; alle vier
ebenen Drehungen dieser Vorlage begrenzt live belegt. Nächster Vorschlag:
Projektvorlagen, Modi, Grenzen und Nachweisniveau strukturiert im MCP ausweisen;
kein freier Vorlagen-/Höhenausbau.

Etappe D / 0.32.0 nach menschlichem Gate live bestanden: festes Lagerprojekt über
neue Treppe, drei Plattformen und zwei obere Wege, alle sieben Objekte fertig.
Tatsächlicher Baustellen-/fertiger Lagerzugang, beide oberen Wege, ausgewählte
Bestandsanschlüsse und gleiche Aktions-ID ohne neue Objekte geprüft.
Nächster Umfang E nur begrenzt: weitere explizite Eigenschaften statt freier
Höhenplanung. B-Schutzabnahme bleibt offen; D ist keine allgemeine Baufreigabe.

Kombinierter B/C-Pilot nach Skript-Gate in 0.31.4 live abgeschlossen: drei
sequenzielle Aufträge und separat lesbare tatsächliche Baustellenzugangszellen.
C mit zwei neuen Wegen und fertigen Lagern bestanden. B-Untersuchung erstmals
mit echter Builder-Positiv-/Negativbaseline; Vorschau-Schutzabnahme weiter offen.
Keine weitere Suchserie: aktuelle Reichweitendiagnose nicht zur Baufreigabe verwenden.

1. Etappe A bestanden: gemeinsamer Bericht, freie/gesperrte Vorschau und
   Vorschau-Eingang separat von tatsächlichen Zugängen live geprüft.
2. Etappe B: Untersuchung abgeschlossen mit technischer Grenze; Schutzabnahme offen.
   Baustellen-Erreichbarkeit unter geplanten Änderungen prüfen.
   0.31.0 live: Baseline/freie Kontrolle bestanden; negative Baustellenkontrolle
   nicht belegt. Begrenzte Detaildiagnose erneut freigegeben und in 0.31.1
   live geprüft. Acht Einzelbefunde und Rücknahme passen; Verbindung vom
   Zentralenursprung bereits in der erreichbaren Baseline überall false. Nächster
   Schritt: Start-/Zielgültigkeit und positiven Verbindungsbezug klären.
   0.31.2 installiert: Zentraleingang und alle acht Ziele auf tatsächlichem
   NavMesh, Start auch auf Bezirksweg. Dennoch alle Verbindungen false bei
   realem Builderzugang true. Positive Kontrolle gescheitert, Pilot gestoppt.
   Nach erneutem go öffentliche Signaturen geprüft: AreConnected vermutlich
   direkte Kante, echte Accessible-Pfadsuche an fertigem Lager positiv.
   Nächster Vorschlag: Nachbar-/Fernkontrolle und tatsächlichen Road-Spill mit
   Builderzugang vergleichen, danach erst gezielte Preview-Engpasskontrolle.
   Keine explizite öffentliche Preview-Pfadsuche gefunden, keine neue Suchserie
   0.31.2 wurde nicht separat committed.
   0.31.3 nach Skript-Gate live belegt: drei Nachbarkanten/echte Straßenpfade,
   vier erreichbare Spill-Ziele, Fern-Kantenwerte weiterhin false. Einzige
   Sperrvorschau ungültig, sieben Bezirksverluste, kein Baustellen-Reichweitenverlust.
   Diagnose bestanden, negative Baustellenkontrolle offen. Nächster Schritt:
   gültige Blockadekontrolle; Ist-Daten ausdrücklich kein Preview-Nachweis.
   Eine gültige Einzelpunkt-Lagervorschau belegt nur einen von vier Zugängen,
   ohne Verlust; kein Negativfall. Weitere Suche in dieser Fixture gestoppt.
   Nach erneutem go isolierten Einzelzugang mit echter Negativbaseline prüfen.
   Erhöhte Fixture tatsächlich getestet: Treppe entfernt, oberer Zugang getrennt,
   vier niedrigere Geländezugänge bleiben erreichbar. Kein Builder-Negativfall.
   Pilot gestoppt; nach erneutem go Isolation gegen sämtliche native Accesses
   und höchstes Nachbargelände herstellen, nicht nur gegen Treppenanschluss.
   Einzelner weiterer Wegabriss trennt Bezirkswegast nachweislich, nicht die vier
   Builder-Spill-Zugänge. Pilot gestoppt. Vor erneuter Fortsetzung konkreten Ansatz
   wählen: separate minimale Fixture oder Herkunftsdiagnose des Spill-Zugangs.
   Keine vollständige Bauzustandssimulation; allgemeine Baufreigabe bleibt gesperrt.
3. Aktuell Etappe C: kleiner Lagerpilot mit bestehendem Weganschluss bis zur
   tatsächlichen Fertigstellung live bestanden. Baustellenzugang und fertiger
   Straßenanschluss positiv; Nachbarlager weiterhin erreichbar, Distanz 10→11.
   Ergänzung 0.31.4: zwei neue fertige Anschlusswege, drei fertige Lager mit freien
   Eingängen/positiven Straßenverbindungen; Bestandskontrolle diesmal Distanz 11→11.
   C im begrenzten Lagerumfang bestanden, kein pauschaler Bestandsschutz.
   [Nachweis und Grenzen](docs/building-completion-pilot.md).
4. Etappe D: Gebäude mit vertikalem Anschluss; danach E: Breitenausbau.
   Abnahmekriterien: [Etappenplan](missionsplan.md).

## Technische Lücken

- Vollständiger Wegschutz für Bauvorhaben.
- Diagnose der tatsächlichen Ursachen von Produktionsstillstand.
- Belastbarkeit auf unterschiedlichen Fraktionen, Karten und Spielversionen.
- Persistenz oder Wiederaufnahme von Bauprojekten nach Sessionwechseln.

## Zurückgestellt

- Vier-Wege-Entwurf (vorgesehene 0.30.1): implementiert, nicht gebaut/live geprüft.
  Lokal wiederherstellbar als benannter Git-Stash
  `deferred-four-path-pilot-before-build-safety-stages`. Nicht Teil von Etappe A;
  erst nach C/D Nutzen prüfen und Konflikte vor Wiederaufnahme gezielt abgleichen.
- Größere Bauprojekte, freie 3-D-Suche und weitere Versorgungsdiagnosen erst nach
  belastbarem Wegschutz und Gebäudezugang.

Historische Baupläne, Kolonieziele und Spielstände sind bewusst kein Backlog mehr.

Aktuelle UI-Objektauswahl in 0.30.0 erledigt: Gebäude, Auswahlwechsel und keine
Auswahl live geprüft; Grenzen und Nachweis stehen in PROJECT_STATE.md.
