# Größerer Entwicklungsschritt: Bauvorhaben Ende zu Ende

Status: am 2026-10-02 ausdrücklich mit „go“ freigegeben. In 0.25.0 implementiert;
Installation und Live-Abnahme stehen aus. Normaler Baupfad bleibt gesperrt, wenn
Pflichtnachweise fehlen.

## Befund

0.24.1 bestätigt eine gemeinsame Vorschau für kleines Lager und zwei Wege:
Geometrie gültig, Eingang im Vorschau-Distriktnetz verbunden, keine gemessenen
Verbindungsverluste und Wiederherstellung bestätigt. Nicht belegt sind die
hypothetische Bauphase und Bauarbeiter-Erreichbarkeit vor realer Platzierung.

Öffentliche lokale Signaturen erneut geprüft: ConstructionSiteAccessible bietet
Accessible und OnPreviewSelect/OnPreviewUnselect; INavigationRangeService bietet
Terrain- und RoadSpill-Reichweiten mit Vorschauvarianten. IDistrictService bietet
IsOnInstantDistrictRoadSpill, aber keine entsprechende öffentliche Preview-Methode.
Diese Zugriffe sind eine mögliche Grundlage für weitere Diagnose, kein fertiger
Nachweis der Bauphase. Private Zustandsänderungen oder erzwungene Fertigstellung
werden nicht vorgeschlagen. Öffentlich zugängliche Modding- und Referenzunterlagen
lieferten in dieser gezielten Recherche keinen vollständigen Ersatznachweis.

## Vorgeschlagenes Paket

1. Gemeinsame Planausführung mit Session, Plan-Kennung und eindeutiger Auftrags-ID.
2. Auftragsstatus mit einzelnen Wegschritten, Gebäude-ID, bestätigtem Teilstand,
   Ablehnungsgrund und möglichem unbestätigtem Ergebnis. Wiederholte ID führt
   keine zweite Aktion aus; nach Sessionwechsel Zustand neu abgleichen.
3. Plan vor Beginn frisch prüfen. Bekannte Geometrie-/Wegkonflikte und Verluste
   in Wegpräfixen oder Gesamtvorschau verhindern sämtliche Aktionen.
4. Bestehende Wege wiederverwenden, neue Wege vom Anschluss zum Eingang regulär
   setzen. Jede Platzierung und tatsächliche Wegverbindung zurücklesen; bei
   Abweichung stoppen. Navigation darf erst nach bestätigter Aktualisierung
   als nutzbar gelten, kein blindes Weiterbauen im selben Tick.
5. Genau einen regulären Gebäudeauftrag setzen. Danach Entity, Position,
   Baustellenstatus und native Bauarbeiter-Erreichbarkeit prüfen. Keine
   Sofortfertigstellung, Materialgeschenke oder automatische Abriss-Rücknahme.
6. Getrennte Diagnose für tatsächliche Bauarbeiter-Erreichbarkeit sowie
   öffentliche Vorschau-Reichweiten ergänzen. Unbelegte Vorhersagen klar markieren.
7. Tests für veraltete Pläne, doppelte IDs, Fehler nach einzelnen Wegschritten,
   Sitzungswechsel, Navigationsverzögerung und nur teilweise erfolgreiche Aufträge.
   Erst dann ein gemeinsames Installationspaket und ein begrenzter Live-Baupilot.

## Freigegebene Entscheidung: begrenzter Entwicklungspilot

**Empfohlen für die Entwicklung:** separater, explizit aktivierter Pilotmodus für
zunächst ein kleines Lager mit höchstens zwei neuen ebenen Wegfeldern im vorhandenen
Entwicklungsspielstand. Vorschau, Planbindung und bekannte Wegschutzprüfungen bleiben
Pflicht. Nur der noch unbekannte Bauphasen-/Bauarbeiter-Vorabnachweis darf in diesem
klar benannten Pilot offen sein; tatsächliche Erreichbarkeit danach prüfen.
Nie als safe oder vollständig geschützt ausgeben. Ein neu unzugängliches Gebäude
kann als regulärer Bauauftrag stehenbleiben; bereits gesetzte Wege bleiben ebenfalls.
Kein automatischer Wiederholungsversuch und keine Ausweitung auf weitere Vorlagen.
Der Nutzer hat diesen konkreten Modus freigegeben. Die Ausnahme gilt ausschließlich
für diesen Pilot; normale Bauaufrufe behalten die Unknown-Sperre.

**Konservative Alternative:** ohne realen Baupilot ausschließlich die hypothetische
Bauphase weiter erforschen. Ausführung bleibt gesperrt; Dauer und Erfolg dieses
Vorabnachweises sind unbekannt. Keine weitere Mini-Releasefolge ohne neues Ergebnis.

## Implementierter Vertrag 0.25.0

- `execute_building_project_pilot`: Suchparameter des Plans, optionIndex, planKey,
  session, neue actionId und ausdrücklich `mode=development_pilot`.
- Voraussetzung: bestehendes Bau-Opt-in in Mod und Server, pausiertes Spiel,
  SmallWarehouse.Folktails und höchstens zwei neue ebene Path-Zellen.
- Frische gemeinsame Vorschau prüft Geometrie, Wegpräfixe und Eingang. Ausschließlich
  `construction_and_road_node_coverage_unproven` darf offenbleiben; bekannte Verluste,
  andere Unknown-Gründe oder nicht wiederhergestellte Navigation werden abgelehnt.
- Nach Annahme genau ein Auftrag je geladener Sitzung. Identische ID und Parameter
  liefern denselben Auftrag; abweichende Parameter oder weitere ID: state_conflict.
- Reguläre Platzierung auf dem Spielhauptthread. Jede Bestätigung frühestens im
  nächsten Update, maximal fünf Echtzeitsekunden Wartezeit je Objekt. Keine
  Veränderung der Spielgeschwindigkeit. Vor jedem Schritt erneute Spielvalidierung,
  Kontrolle bestehender Verbindungen und bereits bestätigter eigener Wege.
- Wege müssen fertig und im realen Distriktwegnetz verbunden sein. Beim Lager
  müssen tatsächliche Entity, Vorlage, Position/Drehung, Baustellenzustand,
  Eingangsverbindung und `IsReachableByBuilders()` bestätigt sein.
- `inspect_building_project(session, actionId)` liest Status und alle Schritt-IDs.
  Zustände: running, completed, stopped, unconfirmed. Schrittzustände: pending,
  confirmed, unconfirmed. completed heißt Auftrag und aktueller Zugang bestätigt,
  nicht fertig gebaut, geliefert oder vollständig gegen Wegprobleme abgesichert.
- Bei Fehlern bleiben gesetzte Objekte stehen. Keine Wiederholung und kein Abriss.
  Ledger nur in der Sitzung; nach Neustart anhand der Entity-IDs abgleichen.
- Vorabdiagnose der hypothetischen Bauarbeiter-Reichweite bleibt offen; keine
  private Reflexion oder erzwungene Bauphase ergänzt. Reale Nachprüfung ist Pflicht.

## Begrenzte Live-Abnahme nach Installation

Ein frisch gelesener Plan für das kleine Lager, maximal zwei Wege, pausiertes Spiel.
Vorher Objektzahl und eine unabhängige Bestandsverbindung lesen. Einmal ausführen,
Status begrenzt abfragen, Entity-/Baustellen- und Anschlussdaten unabhängig lesen.
Identische Anfrage darf nur den Status zurückgeben; geänderte Kennung muss abweisen.
Bei stopped/unconfirmed ausschließlich Zustand abgleichen, kein zweiter Bauversuch.
Keine automatische Ausweitung auf weitere Vorlagen oder weitere Projekte.
