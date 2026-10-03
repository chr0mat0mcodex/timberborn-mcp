# Mission: Timberborn über MCP steuerbar machen

## Ziel

Aktueller Nutzerauftrag: zuerst B im freigegebenen Minimalumfang, dann E.
0.32.1 nach menschlichem Gate live bestanden: konservative Ablehnung bei
unabhängigen offenen Baustellen und eigener Ablauf mit fertigen Vorgängern.
B-Abschluss bedeutet hier belegte Ausschlussregel, keinen allgemeinen Builder-
Vorschauvertrag; bestehende unknown-Sperren nicht lockern.

B-Minimalumfang geschlossen. Erster E-Schritt live bestanden: bislang fehlende
ebene Lagerdrehungen 0/2, Baustellen-/Fertigzugang und Bestandsanschluss geprüft.
Nächster kleiner E-Schritt: eine zusätzliche ebene Vorlage mit mehrzelliger
Grundfläche, nicht freie Höhenplanung oder breiter Vorlagenbau.
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
