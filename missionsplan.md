# Mission: Timberborn über MCP spielen

## Ziel

Ein KI-Agent beobachtet und steuert Timberborn über eine eigene lokale Mod und MCP.
Er verwendet reguläre Spielregeln und baut eine kleine funktionierende Versorgung
mit Wasser, Nahrung, Holz, Wegen und Wohnraum auf.

Aktueller Schwerpunkt ist die Fähigkeit des Agenten: Welche Daten, Eingriffe und
Rückabfragen benötigt er? Praktische Spieltests belegen diese Fähigkeiten; ein
unkontrollierter Kolonieaufbau ersetzt keine gezielte Abnahme.

## Stand

**0.19.1 ist als Korrektur gebaut und installiert; erneute Live-Abnahme steht aus.
Der vorherige Live-Pilot von 0.19.0 fand Weg-/Reichweitenlücken.**
[Projektstand](PROJECT_STATE.md), [Werkzeugkatalog](docs/tools.md), [offene Arbeiten](BACKLOG.md).
Keine Fremdmod-Pflichtbasis; die frühere More-HTTP-API-Phase ist abgeschlossen.

| Fähigkeit | Status und verbleibende Grenze |
| --- | --- |
| Eigener MCP-/Mod-Zugang | Live bestätigt; authentifizierter lokaler Transport und Spielthread-Queue |
| Bevölkerung und Betten | Live bestätigt |
| Vollständige Güter, aktive Status und Ziele | 40 Güter und eine Lagerwarnung mit Ziel live bestätigt; Mehrfachziele und Verschwinden bestätigt; Biberwarnungen offen |
| Karte, Gebäude und Baustellen | Live genutzt; neue Weg-/Zugangs-/Reichweitenleser in 0.19.0 gebaut, Live-Pilot offen |
| Produktion/Verbrauch über Zeit | Native Güterhistorie mit Produktion/Verbrauch live lesbar; Zeitsemantik separat prüfen |
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
- [ ] Biberwarnungen im Live-Test ergänzen.
- [ ] Neue Gebäudezugangs-, Weg- und Arbeitsreichweitenleser live abnehmen.
- [ ] Produktionshindernisse und Bedürfnisse vollständig unterscheiden.
- [ ] Versorgung über Zeit bewerten und fehlenden Wohnraum gezielt nachweisen.
- [ ] Anschließend einen begrenzten zusammenhängenden Agenten-Spielablauf abnehmen.

Diese Reihenfolge beschreibt den nächsten Ausbau, nicht bereits freigegebene Codeänderungen.
Detailaufgaben und spätere Ideen stehen im [Backlog](BACKLOG.md).

## Nachweise und Historie

Das [Projektjournal](docs/project-journal.md) enthält datierte Ergebnisse.
Die [ursprüngliche Mission einschließlich aller Meilensteine bis 0.17.2](docs/history/missionsplan-2026-09-20.md)
bleibt als Historie erhalten. Dortige alte Verbote, Installationsstände und offene Kästchen
beschreiben ihren damaligen Zeitpunkt und sind keine aktuelle Arbeitsanweisung.
