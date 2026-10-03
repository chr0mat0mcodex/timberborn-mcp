# Mission: Timberborn über MCP steuerbar machen

## Ziel

Neuester E-Abschluss: 0.35.1 budgetgerechte ebene Routensuche live bestanden:
isolierter Fünf-/Vier-Wege-Vergleich samt Weitersuchen und reproduzierbaren
Schlüsseln, realer Anschluss um ein Hindernis und erhaltene Bestandszugänge im
Baupilot. Aktionslimits und allgemeine Nachweislücken unverändert.
[Nachweis](docs/road-budget-planner.md).

Aktueller E-Auftrag: ebene Gebäudeprojekte direkt generisch über volle tatsächliche
Geometrie und anschließbaren Wegzugang, keine weiteren Einzelgebäude-Freischaltungen.
0.35.0 nach menschlichem Test-/Installationsgate live bestanden: Bank mit zwei
neuen Wegen und großes 3x3-Freiluftlager mit einem neuen Weg. Vollständiger
Grundriss einschließlich äußerem Eckfeld, Bau-/Fertigzugang, gefährdete
Bestandsverbindung, Höhenkontrolle und Replay geprüft.
Öffentliche Spiel-API liefert nur einen Eingang; Mehrfachzugänge im
Routenbaustein vorbereitet, spielseitig nicht behauptet. Feste Vertikalprojekte
und bestehende Schutzprüfungen bleiben unverändert.
[Umsetzung/Nachweis](docs/generic-building-project.md).

Abgeschlossener E-Schritt: 0.34.0 erweitert den ebenen Pilot um ein
Folktails-Wohnhaus mit vier Grundrisszellen und versetztem Eingang.
Ein vollständiger gedrehter Bauablauf mit zwei neuen Wegen und Grundriss-
Negativkontrolle nach menschlichem Gate live bestanden. Bau-/Fertigzugang und
ausgewählte Bestandsanschlüsse erhalten. [Nachweis](docs/lodge-project-pilot.md).

Abgeschlossener E-Schritt: sequenzielle Vertikalprojekte und historisches MCP-Profil
in 0.33.1 live bestanden. Zwei Projekte ohne Neuladen fertiggebaut;
laufende/unbestätigte Aufträge und offene Baustellen bleiben gesperrt.
[Nachweis](docs/vertical-sequential-pilot.md).

Aktueller Nutzerauftrag: zuerst B im freigegebenen Minimalumfang, dann E.
0.32.1 nach menschlichem Gate live bestanden: konservative Ablehnung bei
unabhängigen offenen Baustellen und eigener Ablauf mit fertigen Vorgängern.
B-Abschluss bedeutet hier belegte Ausschlussregel, keinen allgemeinen Builder-
Vorschauvertrag; bestehende unknown-Sperren nicht lockern.

B-Minimalumfang geschlossen. Erster E-Schritt live bestanden: bislang fehlende
ebene Lagerdrehungen 0/2, Baustellen-/Fertigzugang und Bestandsanschluss geprüft.
0.33.0 ergänzt das mittlere ebene Lager mit gedrehtem Sechs-Felder-Grundriss,
Bau-/Fertigzugang und erhaltenen ausgewählten Bestandsanschlüssen, live bestanden.
Ergänzend auf unveränderter 0.33.0 mittleres Lager Rotation 1 mit drei neuen
fertigen Bodenwegen, Bau-/Fertigzugang und erhaltenen Vergleichsanschlüssen bestanden.
Drehungen 0/2 ebenfalls fertig und erreichbar bei erhaltenen Bestandsanschlüssen.
Damit begrenzter E-Ausbau um eine zusätzliche Vorlage mit allen vier ebenen
Drehungen belegt, keine allgemeine Freigabe für beliebige Vorlagen/Projektgrößen.
Projektumfang inzwischen strukturiert im MCP ausweisbar und lesend live geprüft.
Zusätzlich vertikales Lager mit Treppendrehung 0 / Lagerdrehung 2 auf unveränderter
0.33.0 live bestanden: sieben fertige Objekte, Bau-/Fertigzugang, erhaltene
Bestandsanschlüsse und Replay. Auch Treppendrehung 2 / Lagerdrehung 0 mit denselben
getrennten Nachweisen auf unveränderter 0.33.0 live bestanden. Auch Treppendrehung
1 / Lagerdrehung 3 bestanden; alle vier festen Treppendrehungen begrenzt belegt.
Keine freie
Höhenplanung oder allgemeine Schutzfreigabe.
[Mehrzellen-Nachweis](docs/medium-warehouse-pilot.md).
[Nachweis](docs/building-rotation-pilot.md).

Etappe D / 0.32.0 nach menschlichem Skript-Gate live bestanden: kleines Lager
über neue Treppe und Plattformstrecke, Bau- und Fertigzugang separat geprüft.
Nächste Etappe E bleibt auf explizite Eigenschaften begrenzt; B-Schutzabnahme
bleibt offen. Siehe [Pilot](docs/vertical-warehouse-pilot.md).

Ein Agent soll Timberborn über eine eigene Mod und einen lokalen MCP-Server
strukturiert beobachten, entscheiden und über reguläre Spielservices steuern können.
Das Spiel dient als austauschbare Entwicklungsumgebung.

## Erreicht

- Native Bridge ohne Fremdmod-Pflichtbasis.
- Strukturierte Zustands- und Diagnoseabfragen.
- Kontrollierte Aktionen für Bau, Betrieb, Forschung, Flächen, Entfernung und Zeit.
- Sitzungsbindung, Aktions-IDs, technische Freigaben, Fehlercodes und Rücklesungen.

## Nächste Etappen

Maßgeblich seit 2026-10-03, vom Nutzer freigegeben. Zwei Bauziele:
Bestehende Wege und Zugänge nicht verschlechtern; neue Gebäude während Bau und
nach Fertigstellung erreichbar machen. Diagnose, Ablehnung und erfolgreiche
Ausführung sind gleichwertige Fortschritte, wenn sie fachlich korrekt sind.

| Etappe | Arbeit | Abschlussnachweis |
| --- | --- | --- |
| A | Gemeinsamer Bauprüfbericht aus bestehenden Spielbelegen | Geometrie, Bestandsverbindungen, Baustellenzugänge, geplanter/neuer Zugang und Wiederherstellung getrennt; unknown bleibt sichtbar; keine neue Baufreigabe |
| B | Öffentliche API für Bauphasen-/Bauarbeiterprüfung klären und begrenzt testen | Freie und blockierte Kontrollen korrekt; Abdeckung und Grenzen belegt; bei fehlender API keine Sperre stillschweigend lockern |
| C | Kleinen Lagerbau vollständig durchführen | Vorschau, nutzbare Anschlusswege, erreichbare Baustelle, fertiges Gebäude mit geprüftem Zugang; Bestandsverbindungen erhalten |
| D | Höhen in denselben Gebäudeablauf integrieren | Neues Gebäude mit Treppe/Plattformanschluss, Bau- und Fertigzugang sowie Bestandswege geprüft |
| E | Weitere Vorlagen, Drehungen, Projektgrößen | Erweiterung nur auf explizit unterstützte und geprüfte Eigenschaften |

Versorgung und Betrieb erst als separate Folgestufe ausbauen. Keine freie 3-D-
Planung und kein weiterer Weglängenpilot als Ersatz für A–C. Ein neuer Umweg muss
real nutzbar sein, bevor ein Folgeschritt auf dessen Schutzwirkung angewiesen ist.

Aktuelle Priorisierung durch Nutzer am 2026-10-03: C vor weiterer B-Diagnose.
B bleibt offen; C verwendet den vorhandenen Entwicklungspiloten mit sichtbarer
Vorabnachweislücke, keine allgemeine sichere Baufreigabe. Erster vollständiger
Lagerablauf mit bestehendem Weganschluss bestanden; neu gebauter Anschluss noch
separat nachzuweisen. Bestandsverbindung blieb erhalten, ihre Distanz änderte sich.
[Konkreter Nachweis](docs/building-completion-pilot.md).

Ergänzung 0.31.4: C mit zwei neuen fertigen Anschlusswegen, drei fertigen Lagern
und rückgelesenem Zugang bestanden. B-Untersuchung beendet mit echter positiver
und negativer Builderkontrolle sowie dokumentierter Vorschaugrenze; **kein**
bestandener Vorschau-Schutztest. Diese Sicherheitslücke bleibt technische Arbeit,
nicht Freigabe durch Etappenabschluss. Keine weitere Kandidaten-/Abrisssuchserie.

Teststrategie: kleiner repräsentativer Pilot, insgesamt etwa zehn fachliche Fälle
über die Etappen verteilt. Kontrollen: freier/ungültiger Platz, verbundener/getrennter
Eingang, Engpass/Umweg, erreichbare/gefährdete Baustelle, Höhen und Zustandsänderung.
Bei fehlerhaftem Kontrollfall zuerst Ursache klären, nicht weitere Positivfälle anhäufen.
Jede Implementierung endet am menschlichen Test-Gate aus DEVELOPMENT_WORKFLOW.md.

Keine historische Kolonie, konkrete Baucharge oder Bestandshaltung ist Teil dieser
Mission. Aktueller Stand: [PROJECT_STATE.md](PROJECT_STATE.md).
