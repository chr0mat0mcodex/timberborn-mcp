# Größerer Entwicklungsschritt: Bauvorhaben Ende zu Ende

Status: konkreter Architekturvorschlag vom 2026-10-02, noch nicht zur abweichenden
Ausführung freigegeben. Normaler Modus bleibt gesperrt, wenn Pflichtnachweise fehlen.

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

## Entscheidung erforderlich: Umgang mit fehlendem Vorabnachweis

**Empfohlen für die Entwicklung:** separater, explizit aktivierter Pilotmodus für
zunächst ein kleines Lager mit höchstens zwei neuen ebenen Wegfeldern im vorhandenen
Entwicklungsspielstand. Vorschau, Planbindung und bekannte Wegschutzprüfungen bleiben
Pflicht. Nur der noch unbekannte Bauphasen-/Bauarbeiter-Vorabnachweis darf in diesem
klar benannten Pilot offen sein; tatsächliche Erreichbarkeit danach prüfen.
Nie als safe oder vollständig geschützt ausgeben. Ein neu unzugängliches Gebäude
kann als regulärer Bauauftrag stehenbleiben; bereits gesetzte Wege bleiben ebenfalls.
Kein automatischer Wiederholungsversuch und keine Ausweitung auf weitere Vorlagen.
Dieser Modus darf erst nach ausdrücklicher Nutzerentscheidung implementiert und
aktiviert werden; bestehende Entwicklungsfreigaben nicht als stillen Verzicht auf
die zuletzt vereinbarte Unknown-Sperre behandeln.

**Konservative Alternative:** ohne realen Baupilot ausschließlich die hypothetische
Bauphase weiter erforschen. Ausführung bleibt gesperrt; Dauer und Erfolg dieses
Vorabnachweises sind unbekannt. Keine weitere Mini-Releasefolge ohne neues Ergebnis.
