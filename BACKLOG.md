# Backlog

## MCP-Log und Aufrufeffizienz — konkrete Livebefunde (2026-10-10)

Auf Nutzerauftrag während des 0.37-Stützenpiloten geprüft: aktuelle 32 Logeinträge
und Implementierung von `InvokeLogged`/`ActivityTools`. Nur verdichtete Befunde,
keine Rohlogs oder Spiel-IDs gespeichert. Vorschläge, noch nicht implementiert:

- **P1: Auftragszustände eindeutig trennen.** Projekt `waiting` mit Teilstatus
  `unconfirmed` nach erfolgreicher Platzierung wurde von der Aktionsprüfung als
  Wiederholungsrisiko blockiert. Normalen Zwischenzustand `awaiting_observation`
  nennen; terminale unbestätigte Wirkung separat halten. Ergänzend rein lesenden
  `refresh_large_project` anbieten: tatsächliche Objekte/Zugänge bestätigen, ohne
  jemals den nächsten Auftrag auszulösen. Abnahme: unterbrochene Antwort lässt
  sich ohne Schreibaufruf klären; kein doppelter Auftrag.
- **P1: Fachliches Ergebnis statt nur `ok`.** Advance, Projektleser und
  Fällmarkierung lieferten leere Summaries. Log getrennt um Transportergebnis,
  fachlichen Zustand/Grund, Projekt-/Laufkorrelation und knappen Effekt ergänzen:
  etwa „Treppe im Bau; 0/5 fertig; kein neuer Auftrag“ bzw. „2/2 markiert,
  Ernte ausstehend“. Fehlercode und ungültiges Feld nennen, keine Rohantworten.
  Abnahme: Bauauftrag, Warten, Ablehnung, Fertigstellung und bloße Lesung sind
  ohne zusätzliche Detailabfrage unterscheidbar.
- **P1: Wartegrund und gezielter Baustopp.** Ein Tagesfenster ließ die Treppe
  bei Materialfortschritt 0,8 stehen; erst getrennte Gebäude-/Güterabfragen
  zeigten vier Bretter am Bau und null frei verfügbare Stämme. Projektdiagnose
  mit Kosten, Baustellenbestand, verfügbaren Gütern und beobachtetem Zugang
  bündeln; keine erfundene Restliefermenge. Begrenzten Simulationslauf bei echter
  Fertigstellung der ausgewählten Teile stoppen lassen, mit hartem Zeitlimit
  und bestätigter Pause. Kein automatisches Setzen des Nachfolgers.
- **P2: Zielgerichtete Suchfilter.** Für drei Holzfäller und zwei Verbraucher
  mussten sieben Gebäudeseiten mit insgesamt 197 Objekten gelesen werden.
  `find_buildings` um Vorlagen-/Rollen- und Regionsfilter erweitern; Güter gezielt
  über IDs abfragen. Abnahme: diese Diagnose benötigt höchstens zwei Leseaufrufe,
  ohne Wegeinventar und unbeteiligte Güter zu übertragen.
- **P2: Grenzen im Werkzeugschema.** Mein Aufruf `inspect_goods(limit=64)` war
  ungültig; `limit=32` funktionierte. Numerische Min-/Maximalwerte im Inputschema
  und feldbezogene Fehlermeldungen ausgeben. Agent muss Fehlerdaten vor Zugriff
  auf `data.items` prüfen; keine zusätzliche Spielabfrage wegen Parserfehlern.
- **P2: Log gezielt und vollständig abrufen.** Aktuell maximal die neuesten 32
  aus 128 Einträgen, darunter die eigene laufende Logabfrage. Cursor/Sequenz,
  `hasMore`, sichtbare Ringpufferlücke und Filter nach Projekt/Lauf/Aktionen
  ergänzen; eigene Loglesung standardmäßig ausblenden. Ein fehlender Eintrag
  beweist bei Best-Effort-Telemetrie keine Nichtausführung. Vor dem Server
  blockierte Aufrufe können nur mit separater Client-/Supervisor-Telemetrie
  sichtbar werden; Spiel-Log darf dazu keine erfundenen Aussagen machen.
- **P2: Tempowechsel nachvollziehbar machen.** Im selben Pilot unterbrach ein
  Lauf wegen `speed_changed` (7 → 3); spätere Leser meldeten erst 0, dann 7.
  Urheber unbekannt, keine menschliche Aktion daraus behaupten. Lauf-/Tempoevents
  mit alter/neuer Stufe, Spielzeit und bekannter Quelle protokollieren; unbekannte
  Quelle ausdrücklich markieren. Aktuelles Tempo und eingefrorenen Laufabschluss
  gemeinsam ausgeben, damit ein alter Abschlusswert nicht als Istzustand gilt.

Empfohlene Reihenfolge: eindeutiger Rückbestätigungsleser und Ergebnislog,
danach Bau-/Materialdiagnose mit Fertigstellungsstopp, dann Suchfilter und Cursor.
Keine Ausweitung der laufenden 0.37-Abnahme; Umsetzung als abgegrenzte Folgearbeit.

## Siedlung als Gesamtanlage planen (Spielerfeedback 2026-10-10)

Die aktuelle Ansicht zeigt dicht aneinander gesetzte Testbauten, schmale
Verbindungen zu weiteren Einzelbauten und kaum erschlossene erhöhte Flächen.
Einzelne gültige Bauplätze ergeben keinen brauchbaren Gesamtplan für „jedes
Gebäude einmal“. Vor weiterem Katalogausbau zusammenhängende Bereiche für
Wohnen/Freizeit, Produktion und große Sondergebäude planen; benötigte Grundflächen,
Hauptwege, Höhenzugänge und Kraftleitungen vorab reservieren. Reservierungen und
regionale Suchabdeckung persistent im Plan führen; spätere Bauten dürfen die
vorgesehenen Korridore nicht versehentlich verbrauchen. Eigenständige Folgefähigkeit,
kein stiller Umfangszuwachs der laufenden 0.37-Abnahme.

## 0.37.0 größere Bauvorhaben — Stützenpilot bestanden (2026-10-10)

Kraftvorschau, reale Kraftverbindung und zweiteilige Baufolge bestanden.
Nach erneutem Gate jetzt auch fünfteilige Stützenfolge fertig: bedingte Planung,
strikte native Ausführungsprüfung, Bau-/Fertigzugänge und negative Trägerkontrolle.
Bestandsblockade und Stopp vor Bauauftrag geprüft. Fachlicher Abschluss dokumentiert;
keine pauschale Vorlagen-/Kraftdrehungsabnahme.

Beauftragter erster Umfang: 32 Teile / acht belegte Höhen, explizite Abhängigkeiten,
Fertigstellung vor Folgeauftrag, entrancelose Stützen/Kraftteile sowie öffentliche
Kraftports und Netzleistung. [Abnahmeplan](docs/large-projects-0.37.0.md).
F03 damit erweitert, noch nicht abgenommen und nicht vollständig erledigt:
automatische 3D-Routensuche bleibt offen. F04: neuer Distrikt-Lebenszyklus,
Gelände-Seitenanbauten und Mehrfacheingänge weiter offen. F01 bleibt ausdrücklich
offen; Vorschau ersetzt keinen vollständigen Baustellenzugangsnachweis.

Abnahme 2026-10-10: Vorschaltprozess einschließlich Spielkontakt nach Backendwechsel
und Rampen-/Höhensuche in beiden Pilotgebieten bestanden. Die untenstehenden
Gate-Vermerke sind historisch; Nachweise in PROJECT_STATE.md und den Fachdokumenten.
Offen bleiben dauerhaftes Suchregister und Client-Neustart bei erstmaliger
Werkzeugkatalogübernahme bzw. gegebenenfalls neu hinzugefügten Werkzeugen.

## Persistente Codex-Verbindung — Umsetzung am Gate (2026-10-10)

Vorschaltprozess, stop/start/status und Skriptsperre vorbereitet; Python-Tests im
menschlichen Gate. Offen: einmalige Clientumstellung, Gate und Stop/Start-Nachweis
ohne Clientneustart. [Prüfplan](docs/codex-mcp-supervisor.md).

## Blinde Flecken der Flächensuche — Umsetzung am Gate (2026-10-10)

Native Geometriemasken, automatische Folgeebenen und Suchabdeckung vorbereitet,
noch nicht live abgenommen. [Umfang/Prüfplan](docs/regional-survey.md).
Ein dauerhaftes sitzungsübergreifendes Suchregister bleibt Folgearbeit; aktuelle
Abdeckung wird pro Aufruf samt ungesuchten Ebenen geliefert.

Live belegt: obere Wege hinter Hauptsiedlung und neben Sägewerk sind bereits
an den Distrikt angeschlossen. Drei Bauvorschauen gültig, ein größerer Kandidat
abgelehnt. [Prüfumfang und Vorgehen](docs/regional-survey.md).

- RegionSurvey: ein Slope/other-Objekt macht derzeit seine ganze 8×8-Kachel
  unbekannt; im Sägewerksfeld unterdrückt das sämtliche Planungsaufrufe trotz
  direkt nachweisbarer gültiger Bauplätze. Öffentliche Hindernisgeometrie nutzen,
  soweit vorhanden; sonst Unsicherheit ausdrücklich lokalisieren/begründen und
  begrenzte direkte Kandidatenprüfung zulassen. Unbekannt nie einfach freigeben.
- Höhenübersicht und obere distriktverbundene Wege in die Gebietsauswahl aufnehmen;
  Höhenwechsel dürfen nicht lediglich als h aus der weiteren Planung verschwinden.
- Abdeckungsregister pro Rechteck/Höhe/Vorlage/Drehung: ungeprüft, beobachtet,
  Kandidat, validiert, abgelehnt, unknown; Zeit/Sitzung und Abbruchgrund/Optionslimit.
  Räum- und Bauänderungen invalidieren betroffene Ergebnisse.
- Kompakte Diagnose: whyUnknown, unterdrückte Suchfenster, planningCalls,
  ungesuchte Höhen und Abdeckung. Drei bevorzugte Fenster sind keine Vollsuche.
- Abnahme für spätere Umsetzung: bekannte Slope neben freiem angeschlossenem
  Bauplatz darf die Nachbarprüfung nicht verhindern; Hinderniszelle bleibt korrekt
  gesperrt/unbekannt. Höhe 3 versus 4 und größeres abgelehntes Gebäude als Kontrollen.
  Nach Folgeauftrag jetzt implementiert; Entwickler-Gate und Liveabnahme stehen aus. AGENTS.md unverändert.


## Aktionsjournal des Spielers — geplante MCP-Abfrage (2026-10-10)

Der menschliche Spieler greift auch ohne vorherige Ankündigung ein. Der Agent
soll die letzten X Aktionen im Spiel abfragen und seine Planung daran anpassen
können. Teil des Vorhabens Agenten-UI im Spiel; strukturierter MCP-Abruf auch
unabhängig von einer neuen Oberfläche. Noch nicht implementiert.

- Geplanter lesender Call, Arbeitsname `inspect_recent_actions`: aktuelle
  Sitzung, begrenztes `limit` für die letzten X Einträge, optional Cursor für
  Änderungen seit der letzten Abfrage sowie Filter nach Herkunft/Aktionsart.
- Erfassen: neue Bauaufträge, Abriss-/Räumaufträge, Flächenmarkierungen,
  Gebäude- und Lagereinstellungen, Pause/Tempo, Forschung und weitere relevante
  Spielereingriffe. Unterstützte Ereignisarten und Erfassungslücken offenlegen.
- Kompakte Einträge mit Sequenznummer, Spiel-/Aufnahmezeit, Aktion, betroffenem
  Objekt/Vorlage/Position, altem und neuem Wert soweit beobachtet sowie Ergebnis.
  Auftrag, beobachtete Statusänderung und spätere Wirkung getrennt ausweisen.
- Herkunft nur bei belegbarer Zuordnung als Mensch, MCP oder Simulation
  kennzeichnen; sonst unknown. Ein Zustandsvergleich allein beweist keinen
  menschlichen Eingriff. Geeignete öffentliche Spielereignisse zuerst prüfen.
- Begrenzter sitzungsgebundener Puffer; Cursor, ältester verfügbarer Eintrag,
  Überlauf, Erfassungsbeginn und Sitzungswechsel explizit melden. Keine
  rückwirkende Vollständigkeit vor Erfassungsbeginn behaupten; keine Rohjournale
  oder personenbezogenen Inhalte ins Repository schreiben.
- Chat-Aufruf **„Überprüf mal meine letzten Aktionen“**: nach Implementierung
  Journal abrufen, Änderungen knapp erklären und Konflikte mit laufenden
  Agentenaufträgen prüfen. Keine automatische Rücknahme menschlicher Änderungen.
  Optional vor Fortsetzung längerer Bauabläufe Änderungen seit Cursor abfragen.
- Liveabnahme: Mensch erteilt Bauauftrag, ändert Einstellung und Tempo ohne
  Vorankündigung; Agent findet die Einträge und Werte wieder. MCP-Kontrollaktion,
  automatische Fertigstellung, Limit/Cursor, Pufferüberlauf und Sitzungswechsel
  getrennt prüfen. Entwickler-Test-Gate bleibt maßgeblich.

## Gebäudespieltest 0.36.0 — laufende Effizienzbeobachtungen (2026-10-10)

- Kompakte Typenabdeckung aus aktuellem Bestand: Fertig-/Baustellenanzahl je
  Vorlage, Abgleich gegen regulären Katalog. Der Pilot benötigte sechs
  Objektseiten mit vollständiger Geometrie für lediglich 30 fertige Typen.
- Katalog nach fehlenden Typen, Materialkosten, Eingangshöhe und unterstütztem
  Baupfad filtern. Vollständiger aktueller Katalog: 157 reguläre Vorlagen,
  davon 15 im Baupfad nicht unterstützt; Entwicklerwerkzeuge separat ausschließen.
- Regionalsuche stärker nach tatsächlichem Anschluss und nutzbarer gleicher
  Höhe bewerten; 33 native Reads/zwölf Planungen im südlichen Testgebiet ohne
  Kandidaten. Räumbare Anbauflächen gesondert ausweisen.
- Präzise Eingabefehler statt pauschalem invalid_argument; Leser unterscheiden
  sich hinsichtlich erforderlicher/unerlaubter session. Schema bleibt maßgeblich.
- Vor Räumaufträgen verbleibende Pflanzmarkierungen und spätere Anschlusswege
  gemeinsam prüfen. Erste Charge räumte zwölf Objekte ohne danach nutzbaren
  Bauplatz; vier gezielt entfernte Karottenmarkierungen ermöglichten den Anschluss.
- Bei Fertigstellung aller Bauobjekte früh pausieren und Zugang prüfen:
  Bauhütte samt Wegen bei 28,625 von 48 Stunden fertig, Charge wartete weiter.
  Auf Nutzerauftrag MCP-only umgesetzt; Entwickler-Gate und Liveabnahme
  bestanden (zwei Lager nach 4/3 statt jeweils 24 Stunden), siehe [Bauchargen](docs/building-batch-workflow.md). Kein Frühstopp des
  Räumlaufs oder Hintergrunddispatcher in diesem Umfang.
- Konkrete fehlgeschlagene Spielvalidatoren und Stützbedingungen liefern.
  Dachterrasse auf freiem Boden mit Weg und Material: Planer findet Kandidaten,
  Bau lehnt mit state_conflict ab; Einzelvalidierung nur placement_invalid_in_preview.
  Stackable-Auflage vor teurer Räumung prüfen, nicht aus supported ableiten.

Evidenz und weitere Ergebnisse: [Spieltest 0.36.0](docs/playtest-0.36.0.md).
Übrige Vorschläge nur dokumentiert; Frühabschluss separat implementiert und abgenommen.

## Baumstumpf-Korrektur live bestätigt (2026-10-10)

Benutzer weist im Livepilot auf Baumreste hin. Growable.IsGrown und Lebenszustand
belegen keinen noch vorhandenen Fällertrag. Quellcodekorrektur nutzt öffentliche
Cuttable.Yielder-Daten: abgeerntete Reste separat zählen; nur aktiven positiven
Log-Ertrag empfehlen. Fehlende Ertragsdaten bleiben unbekannt und ausgeschlossen.
Keine heuristische Unterscheidung über Namen, Modelle oder Screenshot notwendig.
Livekontrolle vollständig über 45 Objekte: 34 abgeerntete Reste und acht junge
Bäume ohne Holzertrag ausgeschlossen, drei fällbare Kiefern mit je zwei Holz
erkannt. Stumpfauftrag abgelehnt; zwei echte Bäume markiert, Replay identisch,
anschließende Diagnose bestätigt beide als already_marked. Detailnachweis in
docs/forestry-efficiency-0.35.5.md. Übrige Versionsabnahme bleibt separat offen.


## 0.35.5 abgeschlossen (2026-10-10)

Holzdiagnose mit Ertragsprüfung, Sammelmarkierung/Replay, früher Holzstopp,
kompakte Chargenantworten und terminale native Bauablehnung live bestanden.
[Abschlussnachweis](docs/forestry-efficiency-0.35.5.md).

Folgearbeiten: native räumliche Filter statt 31 internen Reads je Forstdiagnose;
Forestry-Eingabefehler als Ablehnung vor Auftrag ausweisen statt pauschal
„möglicherweise unbestätigt“; Verbrauchsraten und allgemeine ereignisbasierte
Bauabschlüsse. Die Entwurfsabschnitte vom 5. Oktober sind teilweise umgesetzt.



## Screenshot 0.36.0 live abgenommen (2026-10-10)

Lesende Screenshot-Funktion, damit der spielende Agent bei Bedarf das aktuelle
Spielbild sehen kann. Expliziter Abruf statt Dauerstream; Bild als MCP-Bildinhalt
mit Sitzungskennung, Aufnahmezeit und Bildgröße zurückgeben. Nur Timberborn-
Spielansicht, keine Aufnahme anderer Desktopfenster; begrenzte Auflösung und
Antwortgröße. Aufnahme bei fehlender Kamera oder ungeladenem Spiel klar ablehnen.
Keine Screenshots in Git und kein automatisches Mitschreiben einer Bildhistorie.
Strukturierte Daten bleiben Grundlage der Aktionen; Bild dient ergänzender
Orientierung. Entwickler-Gate und begrenzter Livetest bestanden: Spielbild,
Auflösungsbegrenzung, Kameraposition/Höhe, Richtung, Bildwinkel und Bildweite,
sowie Ablehnung fremder Session. [Abnahme](docs/screenshot-0.36.0.md).
Minimierte Ansicht/fehlende Kamera nicht zusätzlich live provoziert.


## Agenten-UI im Spiel — geplantes Vorhaben (2026-10-10)

Eine Mod-Oberfläche für die Zusammenarbeit zwischen menschlichem Spieler und
Agenten vorsehen. Umfang und Interaktionskonzept vor Umsetzung konkretisieren;
dieser Eintrag ist Planung, keine bereits implementierte Funktion.

- **„Zeigen“-Button:** Der Spieler stellt Kamera und UI ein und hält per Knopf
  diese Spielansicht für den Agenten fest. Vorhandene Screenshot-Funktion samt
  Kamerametadaten nutzen. Aufnahmezeitpunkt ist der Klick; späteres Abholen darf
  nicht stillschweigend eine inzwischen veränderte Ansicht aufnehmen.
- Übergabe und sichtbaren Status festlegen: Aufnahme bereit, vom Agenten abgeholt
  oder fehlgeschlagen. Ein Klick allein startet noch keinen Agentenlauf; den
  Abruf beim nächsten Agentenkontakt ausdrücklich vom aktiven Benachrichtigen
  unterscheiden. Begrenzte sitzungsgebundene Aufbewahrung, keine Bildhistorie.
- Abnahme: Ansicht A zeigen, Kamera auf B bewegen, beim Agenten weiterhin A
  mit zugehörigem Zeitpunkt und Kameradaten erhalten; Sitzungswechsel und
  fehlgeschlagene Aufnahme eindeutig behandeln. Entwickler-Test-Gate bleibt.

Bis dahin funktioniert „schau mal“ im Chat mit einer Aufnahme zum Zeitpunkt des
Werkzeugaufrufs. Der Ingame-Button gehört zum größeren UI-Vorhaben, nicht zu einer
noch offenen Pflichtfunktion des abgenommenen Screenshot-Releases 0.36.0.

## Effizienter Holzausbau – Vorschlag, keine Umsetzung (2026-10-05)

Evidenz: Radbau nach einem Spieltag wegen Holzlieferung unvollständig. Für die
Entscheidung waren getrennte Güter-, Inventar-, Vegetations-, Markierungs- und
Arbeitsreichweitenabfragen nötig; Fällmarkierungen wurden über acht Seiten gelesen.
Erst der Abgleich auf der unteren Ebene lieferte geeignete Kiefern. Am Ende
27 Holz insgesamt, aber nur zwei frei verfügbar; neun im Saunainventar.

Priorisierte Erweiterungen bestehender Werkzeuge:

1. **Regionale Holzdiagnose (höchster Nutzen):** vorhandene Gebietssuche mit
   nativer Arbeitsreichweite, Wachstum, Lebenszustand und aktuellen Fällmarkierungen
   serverseitig verknüpfen. Für einen Holzfäller und ein begrenztes Gebiet nur
   geeignete Kandidaten sowie Ausschlusszähler liefern; bekannte Höhen mitführen.
   Unbekannte Reichweite gesondert ausweisen, keine Freigabe daraus ableiten.
   Bestehende regionale Suche erweitern statt ein paralleles Planungssystem bauen.
2. **Verfügbarkeit und Verbrauch gemeinsam erklären:** freie Holzbestände,
   Baustellenbedarf und relevante Gebäudeinventare getrennt anzeigen. Bestand,
   Transportreservierung und gemessener Zu-/Abfluss nicht vermischen. Raten nur
   aus zwei zeitlich bezeichneten Beobachtungen, andernfalls ausdrücklich unbekannt.
   So wird sichtbar, ob Ernte, Transport oder Verbrauch den Ausbau begrenzt.
3. **Begrenzte Sammelmarkierung:** geprüfte Kandidaten in einem Auftrag markieren,
   mit Sitzung, erwarteter Markierung und aktuellem Reichweitenbeleg. Ergebnis
   zählt angenommen/abgelehnt/unbestätigt; keine automatische Wiederholung nach
   unklarer Teiländerung. Das bestätigt Fällaufträge, nicht gefällte Bäume.
4. **Bedingtes Zeitfenster:** optional nach beobachteter Holzlieferung oder einem
   definierten frei verfügbaren Vorrat pausieren, mit harter Zeitgrenze. Nicht
   nur auf feste Spieltage warten. Abschluss nennt Stoppgrund, Veränderungen und
   bestätigte Pause; keine Aussage über langfristige Nachhaltigkeit.
5. **Kompakte Standardantworten:** Zustand, Kernergebnis, Ausschlussgründe,
   Vollständigkeit und notwendige nächste Aktion. Einzelobjekte, alte Schritte
   und wiederholte Einschränkungen nur auf Detailabruf; entscheidende Unsicherheit
   und Teilfehler bleiben immer sichtbar. Gebiet direkt filtern statt globale
   Markierungsseiten beim Client zusammenzuführen.

Vorgeschlagener Abnahmepilot für eine spätere Umsetzung: ein Holzfäller, ein
begrenztes Gebiet mit mindestens einem geeigneten und einem ungeeigneten Baum
(unreif, schon markiert oder außerhalb Reichweite). Diagnose gegen vorhandene
Detailwerkzeuge abgleichen; bei falschem Einschluss stoppen. Ziel für den normalen
Ablauf: Diagnose, Sammelauftrag, begrenzter Lauf und Abschluss in höchstens vier
Client-Aufrufen, ohne zusätzliche manuelle globale Seitenabfragen. Das ist ein
Entwurfsziel, kein gemessener Gewinn. Vorher/nachher Aufrufzahl, Antwortumfang und
Zeit bis zur Entscheidung messen; einmalige Kontrollabfragen separat zählen.


## Kraftanschluss für Produktionsausbau (2026-10-05)

MCP liefert Betriebswarnung fehlender Kraft, aber keine strukturierten
Kraftanschlüsse, Netze, Leistung und Verbindungsgeometrie. Radposition muss aus
einem funktionierenden Vergleich abgeleitet und über tatsächliche Produktion
geprüft werden. Ergänzen: Anschlusspositionen/-richtungen, Netzzugehörigkeit,
Erzeugung/Verbrauch und Vorschläge für geprüfte Kraftverbindungen.

Live-Nachweis inzwischen bestanden: Rad neben Zahnradwerkstatt, Kraftwarnung
verschwunden, fünf Zahnräder in Sauna verbaut und vier weitere im Ausgang.
Das ist ein konkreter Produktionsnachweis, keine allgemeine Anschlussprüfung.
Zusätzlich Materialengpässe mit passenden Arbeitsflächen verknüpfen: Radbau
wartete auf Holz; globaler Bestand, Gebäudeinventare, Fällmarkierungen und native
Holzfällerreichweite mussten getrennt gelesen werden. Vorschlag: kompakter
Engpassbericht mit tatsächlich erreichbaren reifen, noch unmarkierten Bäumen.


## Weitere Beobachtungen beim Farmbau (2026-10-05)

- Räumzeitfenster sollten Arbeitszeiten berücksichtigen: acht Pflanzen nach acht
  Abend-/Nachtstunden nur halb entfernt, erst im folgenden Arbeitsfenster vollständig.
  Acht Einzelmarkierungen plus separate Flächenlöschung zeigen Bedarf für gebündelte
  Umwidmung von Anbaufläche zu Baufläche mit verifiziertem Räumabschluss.

- Sehr kleine Suchfenster liefern keinen Kandidaten trotz geometrisch freiem
  Standort und nahem Weg. Derselbe Standort im 8×8-Fenster als Option mit einem
  neuen Weg gefunden. Suchfenstergrenze/Anschlussroutengrenze als Ursache ausweisen;
  Diagnose requires_game_validation ohne Grund ist für die Entscheidung zu schwach.
- Bündelablauf wählt first_candidate; zur bewussten Erhaltung eines freien
  Durchgangs war manuelle Auswahl von Option 1 und Einzelprojektaufruf nötig.
  Freizuhaltende Korridore oder ausgeschlossene Grundrisse in Chargen unterstützen.
- Farmreichweite plus Bodenfeuchte funktionierte für 38 neue Karottenmarkierungen;
  fünf separate Schreibaufrufe nötig. Begrenzte Mehrrechteck-Pflanzaufträge würden
  den Ablauf verkürzen, mit identischen Vorbedingungen und getrennten Nachweisen.


## Bauprüfung: gestrandete Biber trotz fertigem Gebäudezugang

Südliche Wohncharge baute zwei Häuser mit finished_accessible. Anschließend drei
Biber südlich der Häuser gestrandet; bei freien Betten obdachlos/erschöpft.
Beide Häuser selbst fertig, unpausiert, Distrikt zugeordnet. Engstelle außerhalb
des bestehenden Wegnetzes durch Hausgrundrisse geschlossen. Zweites Haus gezielt
entfernt; nach acht Spielstunden keine Stranded-/Exhausted-Warnung mehr. Gebäude-/Wegzugangsprüfung deckt aktuelle
Biberpositionen und freie Geländeverbindungen nicht ab. Vor weiteren Flächenchargen
freie Rückwege bzw. betroffene bewegliche Einheiten prüfen und diese Nachweislücke
im Ergebnis deutlich ausweisen. Kein behaupteter allgemeiner Navigationsschutz.

## Spielabschnitt 2026-10-05: Chargenfehler und unnötiges Warten

- Definitive native Ablehnung vor Auftragsregistrierung bleibt als pending_build /
  batch_interrupted hängen. Live: zweites Wohnhaus; inspect-Auftrag state_conflict,
  Objekt fehlt, keine Baustellen, gezielte Vorschau buildingValid=false. Ursache
  beibehalten und als abgelehnt abschließen; keinen unbestätigten Auftrag vortäuschen.
- Erstkandidat kann native Bauvalidierung verfehlen. Nach eindeutig mutationsfreier
  Ablehnung andere Kandidaten prüfen, mit festem Versuchslimit und erhaltener Diagnose.
- Fertiges Haus wartet weiter bis Ende des fixen Zeitfensters. Optionale frühzeitige
  Beendigung mit bestätigter Pause würde lange Chargen deutlich beschleunigen.
- Wartende Antworten wiederholen Suchdetails und den Abschluss des Vorgängerhauses.
  Aktuellen Baufortschritt, Restzeit und empfohlenen nächsten Abfragezeitpunkt anzeigen.
- Wohnbelegung je Haus fehlt: freie Betten plus obdachlose Biber sind ohne
  Bewohner-/Kapazitätsdaten pro Gebäude schwer zu erklären; Distrikt und Zugang
  allein belegen keine tatsächliche Wohnnutzung.
- Katalog-supported ist für Gebäude ohne Eingang oder mit erhöhtem Eingang zu grob:
  Hecke/Laterne ohne Eingang, Doppelwohnhaus mit Eingang auf z+1. Direkt passende
  Ausführungsfähigkeit samt Grund ausweisen, bevor Forschung/Standortsuche erfolgt.


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
