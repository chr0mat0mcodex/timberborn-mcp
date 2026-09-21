# Mission: Timberborn über MCP spielen

## Aktuelle Spielziele seit 2026-09-21

1. **100 gleichzeitig lebende Biber erreichen** (Erwachsene und Kinder, keine Bots).
2. **Jedes reguläre Gebäude der aktuellen Fraktion mindestens einmal fertigstellen.**

Der Nutzer hat tatsächlichen Kolonieausbau ausdrücklich zum Auftrag gemacht.
Der bisherige Schwerpunkt einzelner Funktionstests wird damit um einen fortlaufenden,
kontrollierten Spielablauf erweitert. Reguläre Kosten, Forschung und Bauzeiten gelten.
Aktueller Ausgangspunkt: zehn Biber, zwölf Betten, 15 von 157 regulären Bauvorlagen
fertig vorhanden. Fünf Entwicklerwerkzeuge zählen nicht; andere Fraktionen separat.
15 reguläre Vorlagen haben noch technische MCP-Baugrenzen und bleiben Teil des Ziels.

Aktueller Nachweis Tag 68: 40 lebende Biber, 40 Betten, 26/157 Gebäudetypen.
63 Karottenfelder, zwei Farmen und drei Pumpen; begrenzter Erntepilot bestanden.

[Abnahme, Etappen und vollständige Gebäudecheckliste](docs/colony-goals.md).
## Stand

**0.21.1 ergänzt konkrete Gebäudeinventare; installiert und mit allen 30 Lesern sowie konkreten Inventaren live geprüft.**

**0.21.0 liefert einen Produktionsgraphen und korrigiert die Bedürfniszählung bei Todesfällen; installiert, alle 30 Leser und begrenzter Graph-/Lebenszustandspilot live bestanden.
Fünf Paketdateien geprüft, Konfiguration erhalten. Unter 0.19.2 sind
Sofort-Wegsuche, historische Bilanz und Farm-/Holzfällerreichweiten sind live bestätigt.**
[Projektstand](PROJECT_STATE.md), [Werkzeugkatalog](docs/tools.md), [offene Arbeiten](BACKLOG.md).
Keine Fremdmod-Pflichtbasis; die frühere More-HTTP-API-Phase ist abgeschlossen.

| Fähigkeit | Status und verbleibende Grenze |
| --- | --- |
| Eigener MCP-/Mod-Zugang | Live bestätigt; authentifizierter lokaler Transport und Spielthread-Queue |
| Bevölkerung und Betten | Live bestätigt |
| Bedürfnisse und Betriebsbelege | Drei Leser in 0.20.0 live bestätigt; rohe Schwellenflags sind keine UI-Warnung, keine vollständige Blockadendiagnose |
| Vollständige Güter, aktive Status und Ziele | 40 Güter und eine Lagerwarnung mit Ziel live bestätigt; Mehrfachziele und Verschwinden bestätigt; Biberwarnungen offen |
| Karte, Gebäude und Baustellen | Live genutzt; Sofort-Wegsuche inklusive Unterbrechung live bestätigt; Farm-/Holzfällerreichweiten live bestätigt |
| Produktion/Verbrauch über Zeit | Native Güterhistorie mit Produktion/Verbrauch und Fortschreibung über einen Tageswechsel live bestätigt |
| Generische Bauaufträge | Mehrere Vorlagen live; Geometrien/Sonderformen begrenzt |
| Lager, Farm, Gebäudepause | Live bestätigt |
| Personal, Prioritäten, Flächen, Entfernung | Gezielte Live-Piloten bestanden |
| Simulation | Pause, 1×, 3×, 7× live bestätigt |
| Forschung | Erzeugung, Kostenabzug, Freischaltung und kostenfreie Wiederholung live bestätigt |
| Ingame-MCP-Log | Fenster, Scrollen, Calls und Begründungen live bestätigt |
| Fachliche Ablehnungen | Drei Fehlerfälle ohne Zustandsänderung live; noch nicht alle Bereiche spezifisch |
| Vollständige Grundversorgung | Wohnraum erweitert und belegt; nachhaltige Bilanz und allgemeine Problembehandlung offen |

## Abnahmeschleife

1. Frischen Zustand, Session und konkrete Ziele lesen.
2. Fehlende Voraussetzungen und Kosten feststellen.
3. Geometrische Vorprüfung und reguläre Spielvalidierung unterscheiden.
4. Eine begrenzte Aktion ausführen.
5. Ergebnis separat lesen: Auftrag, Fertigstellung, Anschluss, Besetzung und Wirkung sind unterschiedliche Belege.
6. Nur bei belegtem Nutzen fortsetzen; unbestätigte Aktionen nicht automatisch wiederholen.

## Nächste Meilensteine

- [x] Güterübersicht und aktive Status mit konkretem Ziel im begrenzten Live-Pilot abnehmen.
- [x] Mehrfachziele und tatsächliches Verschwinden einer Statusgruppe live prüfen.
- [x] Biber-Todesstatus mit zwei konkreten Entity-Zielen live lesen (kein UI-Alert: showAlert=false).
- [ ] Aktive Hunger-/Durst-UI-Warnungen im Live-Test ergänzen.
- [x] 0.21.0 mit Produktionsgraph und Lebenszustandskorrektur installieren; Sicherung/Hashes/Konfiguration geprüft.
- [x] Produktionsgraph und lebende/tote Biber getrennt live prüfen (11 lebend, zwei tot).
- [x] Gebäudeinventare und globale Wasserbestände abgleichen: 60 Tank + 30 Pumpen + 48 Distriktzentrum = 138.
- [x] Gebäudezugang und Sofort-Wegverbindung einschließlich Unterbrechung/Wiederherstellung gezielt prüfen.
- [x] Konkrete Farm-/Holzfällerreichweiten über BuildingTerrainRange live abnehmen (485/611 Zellen; erste/letzte Seite).
- [x] Native Produktions-/Verbrauchshistorie über einen Tageswechsel prüfen.
- [x] Native Bedürfnisflags und Betriebsbelege als begrenzte Leser implementieren und regulär testen.
- [x] 0.20.0 live abnehmen: Bedürfnisübersicht/Einzelbiber und fertiges Produktionsgebäude.
- [x] Unfertigen Bauauftrag diagnostizieren, separat bestätigen und wieder entfernen.
- [ ] Produktionshindernisse und Bedürfnisse vollständig unterscheiden (insbesondere Energie, Wasser, Rohstoff- und Lieferwege).
- [x] Versorgung über rund drei Spieltage bewerten und fehlenden Wohnraum gezielt nachweisen.
- [x] Einzelnen zusätzlichen Wassertank regulär bauen, konfigurieren und tatsächliche Umlagerung bestätigen; allein keine Nachhaltigkeitsabnahme.
- [x] Normale Wasserentnahme, freien Ausgang und Wiederauffüllung beider Pumpen beobachten; getrennt von Tagesbilanz bewerten.
- [x] Zusätzliche Lodge und Holzlager regulär bauen; zwölf Betten ohne Obdachlose und reale Holzeinlagerung bestätigen.
- [x] 16 zusätzliche Karottenzellen im Farmbereich markieren und tatsächliches Pflanzen bestätigen; Ernte separat offen.
- [x] Förster einschließlich vorgelagerter Brettproduktion regulär bauen; zwölf Eichen tatsächlich pflanzen und Reichweite/Besetzung bestätigen.
- [x] Eichenfläche auf 79 Pflanzplätze erweitern; 19 lebende Eichen nachgelesen.
- [x] Alle 79 Eichen und zwölf zusätzliche Birken tatsächlich gepflanzt und lebend bestätigt.
- [x] Wachstum, reguläre Holzernte und Nachpflanzung an repräsentativen Eichen-/Birkenstandorten bestätigt; 19 neue lebende Eicheninstanzen an zuvor belegten Pflanzplätzen. Keine vollständige Ertragsbilanz aller Bäume.
- [ ] Negative Wasser-/Nahrungs-/Holzbilanz beheben und nachhaltige Versorgung mit Wohnraum abnehmen.
- [ ] Anschließend einen begrenzten zusammenhängenden Agenten-Spielablauf abnehmen.

Diese Reihenfolge beschreibt den nächsten Ausbau, nicht bereits freigegebene Codeänderungen.
Detailaufgaben und spätere Ideen stehen im [Backlog](BACKLOG.md).

## Produktionsgraph — Umsetzung 0.21.0

- [x] Produktions-/Abhängigkeitsgraphen des definierten Umfangs einmal extrahieren und mit einem
  MCP-Aufruf bereitstellen: Produkte, Vorprodukte, Rezepte, Gebäude und Voraussetzungen.
  [Umfang und öffentliche API-Grundlage](docs/production-dependency-graph.md).
- [x] Live-Abnahme: Antwortgröße, stabile Revision, Verarbeitungsketten, Brennstoff sowie Ernte-/Sammelquellen geprüft. Ruinenerträge und besondere Betriebsbedingungen bleiben explizite Lücken.

## Zukünftige Steuerfunktionen

- [ ] Simulation für eine vorgegebene Ingame-Dauer (Stunden, Tage, Wochen) oder bis
  zu einem konkreten Ingame-Zeitpunkt laufen lassen. Zielkontrolle, automatisches
  Pausieren sowie Status/Abbruch sollen als MCP-Funktion bereitstehen, statt vom
  Agenten aus Echtzeit-Wartebefehlen zusammengesetzt zu werden.
  [Featureplan und Abnahmekriterien](docs/simulation-runs-plan.md).

## Nachweise und Historie

Das [Projektjournal](docs/project-journal.md) enthält datierte Ergebnisse.
Die [ursprüngliche Mission einschließlich aller Meilensteine bis 0.17.2](docs/history/missionsplan-2026-09-20.md)
bleibt als Historie erhalten. Dortige alte Verbote, Installationsstände und offene Kästchen
beschreiben ihren damaligen Zeitpunkt und sind keine aktuelle Arbeitsanweisung.
