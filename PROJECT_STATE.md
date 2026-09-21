# Projektstand

Stand: 2026-09-21. Codeversion: **0.21.1**, Gebäudeinventardiagnose gebaut, regulär getestet, installiert und mit allen 30 Lesern live geprüft. Zuvor 0.21.0 mit 30 Lesern live bestätigt.
Installiert ist **0.21.1**, fünf Datei-Hashes geprüft und Konfiguration erhalten. Zuvor unter **0.20.0**: 29 Leser live geprüft, danach Fehler bei weiter registrierten
verstorbenen Bibern gefunden (Bedürfniszählung 13 statt 11 lebender Biber).
Sofort-Wegsuche einschließlich Unterbrechung/Wiederherstellung unter **0.19.1** live bestätigt. Farm-/Holzfällerreichweiten unter 0.19.2 über den konkreten Terrainzugriff bestätigt.
Die Version im [Manifest](mod/Timberborn.AgentBridge/manifest.json) ist die Codeversion;
ältere datierte Einträge im Journal dokumentieren frühere Zustände.

## Aktueller Spielauftrag

Seit 2026-09-21: 100 lebende Biber und jede reguläre Gebäudevorlage mindestens einmal
fertigstellen. Ausgangspunkt zehn Biber, 15/157 Folktails-Vorlagen bestätigt.
15 reguläre Vorlagen haben noch MCP-Baugrenzen. [Fortschritt und Checkliste](docs/colony-goals.md).

## Produkt und Architektur

- Ziel: KI-Agent spielt Timberborn über MCP mit regulären Kosten, Bauzeit, Personal und Forschung.
- Aktiver Weg: `Timberborn.McpServer` → `Timberborn.Backend.Native` → eigene `Timberborn.AgentBridge`.
- Mod-ID: `chr0mat0mcodex.TimberbornAgentBridge`; keine erforderlichen Fremdmods.
- MCP: stdio; Modtransport: authentifiziertes Loopback-HTTP auf `localhost`, Port aus privater Konfiguration.
- Spielzugriffe auf dem Hauptthread, feste Routen, begrenzte Antworten und Aktionen.
- Keine eigenen Save-Daten. Reguläre Spieländerungen werden mit dem Spielstand gespeichert.
- `MoreHttpApi`, Fake sowie die frühen Contracts/Application-Schichten bleiben für Legacy/Regression erhalten.
  Der native Werkzeugpfad nutzt eigene Native-Verträge. Keine komplette Backend-Parität behaupten.
- Direkter Serverstart ohne `TIMBERBORN_BACKEND` fällt im aktuellen Code noch auf `more-http-api`
  zurück. Der dokumentierte Einstieg setzt deshalb ausdrücklich `native`; kein stiller Backendwechsel.

## Produktionsgraph 0.21.0

30 Leser und 19 Aktions-/Validierungswerkzeuge. Neuer Gesamtaufruf für registrierte Güter,
Rezepte, Gebäude/Baukosten, nominelle Energie und Schnitt-/Sammelquellen.
Einmal je Szene, geprüfte Verweise und SHA-256-Revision. Ruinenerträge und spezielle
Betriebsbedingungen sind explizite Lücken; [Vertrag](docs/production-dependency-graph.md).
Live bestätigt: 60 Güter (40 aktiv), 55 Rezepte, 162 Gebäude, 17 Quellen; 58.345 Byte MCP-Nutzdaten, stabile Revision. Zehn repräsentative Definitionen unabhängig mit offiziellen Blueprints verglichen.

## Letzter Spielstand und Wirkungstest

Tag 68, abends: 40 lebende Biber (38 Erwachsene, zwei Kinder), 40 Betten und keine
Obdachlosen. 26/157 reguläre Gebäudetypen fertig, zuletzt Bank, Strauch und 1x1-Dach.
Zweites Farmhaus und dritte Wasserpumpe fertig, mit drei bzw. einem
Arbeiter in Betrieb bestätigt. Neue Farm High, neue Pumpe VeryHigh; Karottenpräferenz
unabhängig zurückgelesen. Weitere 25 Beerenbüsche nach konkreter Freigabe geräumt.

Karottenfläche von 34 auf 63 Markierungen erweitert: 13 freie Zellen und 16 im
südlichen Versorgungsblock. Alle 16 südlichen Pflanzen lebend ohne Wasserstress;
später 16 neue Pflanzeninstanzen nach der ersten Ernte bestätigt. Erntepilot nach
acht begrenzten Intervallen regulär bei 65 Karotten beendet. Letzte zwei gespeicherte
Tagesproben: 117 produziert, 84 verbraucht. Noch keine 100-Biber-Versorgungsabnahme.

Zwei zusätzliche kleine Lager fertig, auf Carrot/Gear eingestellt und zurückgelesen
(je 30 Plätze). Zahnradbestand über den früheren Ausgabepuffer von zehn gestiegen.
Nach kleiner Baucharge: 141 Wasser, 220 Beeren, 33 Karotten, 232 Holz,
56 Bretter und 25 Zahnräder. Wasserlager 120 Plätze plus drei Pumpenpuffer.

Forst: 79 Eichen zuvor lebend bestätigt, 33 Birken-Pflanzplätze. Eichen-Ernte und
Nachpflanzung nun an 19 früher belegten Standorten durch neue lebende Bauminstanzen
bestätigt; entsprechender Birkennachweis besteht bereits. Dauerhafte Ertragsbilanz
offen. Großes Holzlager auf Holz, zwei Sägewerke und zwei Erfinder aktiv.

Begrenzte Simulationsabschnitte kontrollieren Bedürfnisse, Vorräte und Besetzung.
Keine abschließende Nachhaltigkeits- oder 100-Biber-Abnahme.
[Spielziele und Checkliste](docs/colony-goals.md), [Versorgungsbelege](docs/supply-balance.md).

## Verifikation

| Ebene | Beleg |
| --- | --- |
| Automatisch | 487 reguläre Tests: 474 Unit, 13 Integration; drei opt-in Live-Tests im Standardlauf übersprungen |
| Mod-Build | Gegen Timberborn 1.1.2.4, ohne Warnungen/Fehler |
| Installation | Fünf Paketdateien per SHA-256 geprüft, private Konfiguration erhalten |
| Live 0.21.1 | Alle 30 Leser; fünf Gebäudeinventare erklären exakt 138 Wasser: Tanks 60, Pumpen 30, Distriktzentrum 48; keine Kapazitätsreservierung für Wasser |
| Live 0.21.0 | Alle 30 Leser; Produktionsgraph samt Ketten/Quellen/Größe/Revision; 11 lebende Biber und zwei Tote korrekt getrennt, beide toten Einzelziele ohne aktuelle Bedürfnisse |
| Live 0.20.0 | Alle 29 Leser; 42 Bedürfnisse über zwei Seiten, Einzelbiber und Betriebsbelege; Wohnraumbefund unabhängig bestätigt |
| Live 0.19.2 | Alle 26 Leser bestanden; Holzfäller 611 und Farm 485 Terrainzellen, erste und letzte Ergebnisseite geprüft |
| Live 0.18.0 | Alle 22 Leser bestanden; 40 Güter, drei Beispielvorräte deckungsgleich, eine Lagerwarnung mit Ziel und unabhängiger Bestätigung; unbekannte ID und stale_session korrekt |
| Live 0.17.2 | template_locked, stale_session, state_conflict; unveränderter Zustand, rejected im Log |
| Live Forschung | Regulär produzierte Punkte, bezahlter Unlock 35 → 5, Wiederholung ohne zweiten Abzug, gültige Bauvalidierung danach |
| Live Eingang | Echte Path-Vorlage von Lodge-Belegung unterschieden; Distrikt und Personal separat bestätigt |
| Weitere Piloten | Generischer Bau, Lager/Farmoptionen, Prioritäten, Flächen, Entfernung, Zeitsteuerung und MCP-Log |

Die Live-Nachweise beziehen sich auf eine kleine Entwicklungskolonie mit Folktails.
Nicht jede Gebäudekombination oder Spielversion ist getestet. Nicht jeder reguläre
Testlauf startet das Spiel oder prüft alle nativen Leser live.

## Umfang und Grenzen

[29 Leser + 19 freizugebende Werkzeuge](docs/tools.md). Die Steuerungsbasis ist vorhanden.
Neu in 0.20.0: Bedürfnisübersicht, Details je Biber und Gebäudebetriebsbelege;
[Diagnosevertrag und Live-Nachweis](docs/needs-and-operation.md). 13 Biber/42 Bedürfnisse,
Einzelbiber, Session-/Zielablehnungen, Erfinder/Farm/Pumpen live geprüft. warning ist
ein rohes Schwellenflag, keine akute UI-Warnung. Baustellenfall mit temporärem Lagerauftrag und bestätigter Entfernung ebenfalls live geprüft.
Weiterhin implementiert: vollständige registrierte Güter mit ResourceCount-Feldern sowie
sichtbare aktive Entity-Status und betroffene Ziele; [Vertrag](docs/economy-observations.md).
Live bestätigt: Lagerwarnung, Mehrfachziele, Verschwinden und Wiederherstellung; Biberwarnungen offen.
0.19.0 ergänzt Gebäudezugang, echte Straßenverbindungen, Arbeitsreichweiten und native
Güterhistorien mit Produktions-/Verbrauchswerten. [Vertrag und Pilot](docs/logistics.md).
Alle 26 Leser unter 0.19.2 erneut live bestanden. Funktionale Folgeprüfungen:
Sofort-Wegsuche unter 0.19.1 mit true → false → true bei Unterbrechung/Wiederaufbau bestätigt;
Güterhistorie bleibt tagsüber unverändert und erhält zum Tageswechsel neue Produktions-/Verbrauchsdaten.
Farm/Holzfäller besitzen keine allgemeinen Range-Provider; 0.19.2 ergänzt BuildingTerrainRange.GetRange.
Live bestätigt: Holzfäller 611, Farm 485 Zellen; Quelle building_terrain_range sowie erste und letzte Seite geprüft.
Das belegt Navigationsreichweite, nicht Erntefähigkeit oder Produktionsleistung. Weitere Grenzen: vollständige
UI-Meldungsabdeckung, Biberwarnungen, Bedürfnisse/Produktionshindernisse und nachhaltige Versorgung.
Mehrtagspilot: rund 3,05 Spieltage, elf Messpunkte ohne Hunger/Durst, aber negative
Wasser-/Beeren-/Holzbilanz. [Auswertung](docs/supply-balance.md). Spiel danach pausiert.
Eine Lagerkorrektur (Beerenauswahl im bestehenden mittleren Lager) zeigt erhöhte
Produktion; Vergleich nach zwei gemeldeten Alterstodesfällen vorzeitig gestoppt.
Todesstatus mit zwei BeaverAdult-Zielen gelesen; aktive Hunger-/Durst-UI-Warnung weiter offen.
Die Grundversorgungsabnahme ist nicht vollständig: einzelne Produktionsketten belegt,
Wohnraum bestätigt, nachhaltige Gesamtversorgung noch offen. [Backlog](BACKLOG.md).

## Dateien und Veröffentlichung

- `src/`: MCP-Server, Native-Client, Bridge.Core, Legacy-/Fake-Code.
- `mod/Timberborn.AgentBridge/`: eigene Spielmod und Manifest.
- `tests/`: synthetische Tests und getrennte opt-in Live-Tests.
- `scripts/`: reproduzierbarer Build, Lesestart, Prüfungen und Referenzabruf.
- `.local/`: ignorierte lokale Pakete, Sicherungen und Testartefakte; keine Veröffentlichung.
- GitHub: [chr0mat0mcodex/timberborn-mcp](https://github.com/chr0mat0mcodex/timberborn-mcp), Branch `master`.
- Keine GitHub-Releases vorhanden beim Abgleich am 2026-09-20; lokale Modpakete sind keine veröffentlichten Releases.
- Private Konfiguration, Tokens, Saves und Spielbibliotheken gehören nicht ins Repository.
