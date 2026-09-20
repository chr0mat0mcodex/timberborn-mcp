# Mission: Timberborn über MCP spielen

## Ziel

Ein KI-Agent beobachtet und steuert Timberborn über eine eigene lokale Mod und MCP.
Er verwendet reguläre Spielregeln und baut eine kleine funktionierende Versorgung
mit Wasser, Nahrung, Holz, Wegen und Wohnraum auf.

Aktueller Schwerpunkt ist die Fähigkeit des Agenten: Welche Daten, Eingriffe und
Rückabfragen benötigt er? Praktische Spieltests belegen diese Fähigkeiten; ein
unkontrollierter Kolonieaufbau ersetzt keine gezielte Abnahme.

## Stand

**0.20.1 korrigiert die Bedürfniszählung bei Todesfällen; gebaut, Installation/Live-Abnahme offen.
0.20.0 bleibt installiert. Unter 0.19.2 sind
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
| Vollständige Grundversorgung | Offen: Wohnraum, nachhaltige Bilanz und allgemeine Problembehandlung |

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
- [ ] Lebenszustandskorrektur 0.20.1 installieren und lebende/tote Biber getrennt live prüfen.
- [x] Gebäudezugang und Sofort-Wegverbindung einschließlich Unterbrechung/Wiederherstellung gezielt prüfen.
- [x] Konkrete Farm-/Holzfällerreichweiten über BuildingTerrainRange live abnehmen (485/611 Zellen; erste/letzte Seite).
- [x] Native Produktions-/Verbrauchshistorie über einen Tageswechsel prüfen.
- [x] Native Bedürfnisflags und Betriebsbelege als begrenzte Leser implementieren und regulär testen.
- [x] 0.20.0 live abnehmen: Bedürfnisübersicht/Einzelbiber und fertiges Produktionsgebäude.
- [x] Unfertigen Bauauftrag diagnostizieren, separat bestätigen und wieder entfernen.
- [ ] Produktionshindernisse und Bedürfnisse vollständig unterscheiden (insbesondere Energie, Wasser, Rohstoff- und Lieferwege).
- [x] Versorgung über rund drei Spieltage bewerten und fehlenden Wohnraum gezielt nachweisen.
- [ ] Negative Wasser-/Nahrungs-/Holzbilanz beheben und nachhaltige Versorgung mit Wohnraum abnehmen.
- [ ] Anschließend einen begrenzten zusammenhängenden Agenten-Spielablauf abnehmen.

Diese Reihenfolge beschreibt den nächsten Ausbau, nicht bereits freigegebene Codeänderungen.
Detailaufgaben und spätere Ideen stehen im [Backlog](BACKLOG.md).

## Nachweise und Historie

Das [Projektjournal](docs/project-journal.md) enthält datierte Ergebnisse.
Die [ursprüngliche Mission einschließlich aller Meilensteine bis 0.17.2](docs/history/missionsplan-2026-09-20.md)
bleibt als Historie erhalten. Dortige alte Verbote, Installationsstände und offene Kästchen
beschreiben ihren damaligen Zeitpunkt und sind keine aktuelle Arbeitsanweisung.
