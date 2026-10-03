# Kompatibilität und Nachweise

## Aktuelle Basis

Installiert und live bestanden: 0.34.0 mit ebenem Wohnhausprojekt und
vorlagenspezifischem MCP-Nachweisprofil. [Nachweis](../lodge-project-pilot.md).

Vorheriger Schritt live bestanden: Bridge 0.33.1 mit sequenziellen
Vertikalprojekten. Versionszuordnungen im nativen Backend erweitert, übrige
Spiel-/Protokollbasis unverändert. [Nachweis](../vertical-sequential-pilot.md).
Die folgende Tabelle beschreibt den zuletzt verifizierten Stand.

| Bereich | Stand |
| --- | --- |
| Timberborn | 1.1.2.4 / Folktails |
| Bridge | Agent Bridge 0.34.0; gedrehtes ebenes Wohnhaus samt Bau-/Fertigzugang, Bestandskontrolle und Replay live belegt; allgemeiner Vorschau-Bauphasenschutz nicht belegt |
| Transport | stdio-MCP und authentifiziertes Loopback-HTTP |
| Abhängigkeiten | keine Fremdmod-Pflichtbasis |
| Automatische Prüfung | Letzter übermittelter Zahlenstand 0.29.3: 667 erfolgreich, 3 übersprungen; Bereitmeldung für 0.34.0 ohne neue Zählung |

## Nachweisniveau

Die Bridge, die Lesewerkzeuge, strukturierte Aktionen und ausgewählte Interaktionen
sind jeweils für konkrete technische Fälle geprüft. Das ist keine Zusage für jede
Spielversion, Fraktion, Vorlage, Kartenform oder Modkombination.

Für neue Funktionen gilt: Build, synthetische Tests und ein begrenzter Livefall mit
strukturierter Rücklesung. Bauprojekte brauchen zusätzlich Vorschau, Sessionbindung,
Aktions-ID und Nachprüfung der tatsächlich erzeugten Objekte.

Historische Spielweltdaten und Testprotokolle werden nicht mehr geführt.
