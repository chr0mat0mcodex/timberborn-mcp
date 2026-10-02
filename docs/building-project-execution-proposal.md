# Bauprojektausführung in 0.25.0

## Einordnung

**Testsystem; schnelle und effiziente Entwicklung zuerst.** Die frühere Beschränkung
auf einen separat genehmigten Einzelversuch ist als Arbeitsanweisung überholt.
Normale Ingame-Tests sind freigegeben. Der folgende Vertrag beschreibt den tatsächlich
implementierten Stand, nicht die Grenze künftiger Entwicklung oder Testbefugnisse.
[Maßgebliche Direktiven](../AGENTS.md).

0.25.0 ist gebaut, automatisch geprüft und installiert. Reale Ausführung noch nicht
live bestätigt. Die gemeinsame Vorschau für ein kleines Lager mit zwei Wegen wurde
unter 0.24.1 erfolgreich geprüft; sie ist kein Nachweis der gesamten Bauphase.

## Technischer Vertrag der installierten Version

- `execute_building_project_pilot`: Plan-Suchparameter, optionIndex, planKey,
  session, neue actionId und `mode=development_pilot`.
- Voraussetzungen im Code: Bau-Opt-in in Mod und Server, pausiertes Spiel,
  SmallWarehouse.Folktails und höchstens zwei neue ebene Path-Zellen.
- Frische gemeinsame Vorschau prüft Geometrie, Wegpräfixe, Eingang und Rücknahme
  temporärer Vorschauobjekte. Ausschließlich
  `construction_and_road_node_coverage_unproven` darf als Nachweislücke offenbleiben.
- Ein akzeptierter Auftrag je geladener Sitzung. Identische ID und Parameter
  liefern denselben Auftrag; andere Parameter oder eine weitere ID: state_conflict.
- Reguläre Platzierung auf dem Hauptthread über mehrere Updates; Rückprüfung
  frühestens im nächsten Update, höchstens fünf Echtzeitsekunden je Objekt.
- Vor jedem Schritt frische Spielvalidierung und Kontrolle der gemessenen vorhandenen
  Verbindungen sowie der bereits bestätigten eigenen Wege.
- Wege müssen fertig und im realen Distriktwegnetz verbunden sein. Beim Lager werden
  Entity, Vorlage, Position/Drehung, Baustelle, Eingangsverbindung und
  `IsReachableByBuilders()` geprüft.
- `inspect_building_project(session, actionId)` liefert Schritt-IDs und Status:
  running, completed, stopped oder unconfirmed. completed bedeutet regulärer
  Auftrag und Zugang bestätigt, nicht Fertigstellung, Lieferung oder Betrieb.
- Keine automatische Rücknahme durch diese Implementierung. Teilstände bleiben
  stehen; Ledger nur in der Sitzung. Ein anderer Test darf vorhandene Objekte
  verwenden oder entfernen. Vor einem Retry unklare Wirkung auflösen.

Die normalen Bauaufrufe bleiben in 0.25.0 technisch bei unknown gesperrt.
Diese Einschränkungen wurden durch die neue Arbeitsdirektive nicht aus der DLL
entfernt; sinnvolle Erweiterungen und Korrekturen sind nächste Entwicklungsarbeit.

## Nächster praktischer Nachweis

Einen passenden aktuellen Bauplan lesen, einmal ausführen, Status und tatsächliche
Wirkung prüfen. Aussagekräftige Positiv-/Negativfälle und Duplikatsicherheit genügen
als erste Abnahme. Kein alter Save-Name, fester Gebäudebestand, konkreter Vorrat oder
früherer Bauplatz nötig. Bei Fehlern Ursache bearbeiten statt blind wiederholen;
nach einem Ergebnis ohne zusätzliche Erhaltungsfreigabe sinnvoll weiterarbeiten.

Wegkonflikte und Vorschau-Rücknahme werden geprüft, weil sie Produktfunktionen sind.
Es geht nicht darum, die Testkolonie zu bewahren. Reale Testbauten und Abrisse sind
zulässig; Ausgangszustand wiederherzustellen ist keine allgemeine Testpflicht.

## Offene technische Frage

Öffentliche Signaturen: ConstructionSiteAccessible bietet Accessible und
OnPreviewSelect/OnPreviewUnselect; INavigationRangeService bietet Terrain- und
RoadSpill-Reichweiten mit Vorschauvarianten. IDistrictService bietet
IsOnInstantDistrictRoadSpill, aber keine entsprechende öffentliche Preview-Methode.
Diese APIs sind mögliche Ansätze, kein Nachweis vollständiger Bauphasenabdeckung.
Gezielte reale Versuche sind vor langwieriger spekulativer Nachbildung zu bevorzugen.
Private Zustandsmanipulation ist bislang nicht Teil der Umsetzung.
