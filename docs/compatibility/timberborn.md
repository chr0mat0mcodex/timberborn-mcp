# Kompatibilität und Nachweise

## 0.37.0 — begrenzte Live-Nachweise (2026-10-10)

Nach menschlichen Gates und Bereitmeldungen: reale Wellen-/Lagerfolge und
fünfteiliger Höhenbau mit Treppe, zwei Plattformen, Weg und erhöhtem Lager fertig.
Aufgeschobene Stützenprüfung nur in der Planung, vollständige native Prüfung
vor jedem realen Auftrag. Träger-Negativkontrolle, Bestandsblockade, identischer
Start und Stopp vor Ausführung geprüft. Bau-/Fertigzugang sowie reale Kraftnetzdaten
separat beobachtet. [Belege und Grenzen](../large-projects-0.37.0.md).
Kein Livebau mit 32 Teilen/acht Ebenen und keine Abnahme sämtlicher Sondergebäude,
Fraktionen oder Kraftdrehungen. Der Release-Nachweis bleibt auf diese Fälle begrenzt.

## Screenshot-Zusatznachweis 0.36.0 (2026-10-10)

Entwickler-Gate und begrenzte Liveabnahme bestanden: zwei Spielbildauflösungen,
Kamerametadaten einschließlich Bildweite, falsche Session ohne Bild. Keine
zusätzliche Fraktions-/Plattformabnahme. [Details](../screenshot-0.36.0.md).

## Aktueller Zusatznachweis 0.35.5 (2026-10-10)

Menschliches Gate mit anschließender Bereitmeldung; native Bridge 0.35.5 live
bestätigt. Baumstumpf-/Ertragsunterscheidung, Sammelmarkierung/Replay, früher
Holzstopp und kompakte/terminale Chargenberichte bestanden. Gültige Flagge
unabhängig finished_accessible bestätigt. 77 gezielte Unit-Tests und sechs
Integrationsvarianten bestanden; MCP-/Testprojekte und Mod ohne Compilerwarnung.
Keine erneute allgemeine Katalog-, Fraktions- oder Spielversionsabnahme.
[Nachweis](../forestry-efficiency-0.35.5.md).

## Frühere Basis

Installiert und begrenzt live bestanden: 0.35.1 mit budgetgerechter ebener
Routensuche, generischem Gebäudeprojekt und dynamischem MCP-Katalogprofil.
[Nachweis](../road-budget-planner.md).

Vorheriger Schritt live bestanden: Bridge 0.33.1 mit sequenziellen
Vertikalprojekten. Versionszuordnungen im nativen Backend erweitert, übrige
Spiel-/Protokollbasis unverändert. [Nachweis](../vertical-sequential-pilot.md).
Die folgende Tabelle beschreibt den zuletzt verifizierten Stand.

| Bereich | Stand |
| --- | --- |
| Timberborn | 1.1.2.4 / Folktails |
| Bridge | Agent Bridge 0.35.1; Wegbudget-Grenzkontrolle, generischer ebener Bauablauf samt Bau-/Fertigzugang, Bestandskontrolle und Replay begrenzt live belegt; allgemeiner Vorschau-Bauphasenschutz nicht belegt |
| Transport | stdio-MCP und authentifiziertes Loopback-HTTP |
| Abhängigkeiten | keine Fremdmod-Pflichtbasis |
| Automatische Prüfung | Letzter übermittelter Zahlenstand 0.29.3: 667 erfolgreich, 3 übersprungen; Bereitmeldung für 0.35.1 ohne neue Zählung |

## Nachweisniveau

Die Bridge, die Lesewerkzeuge, strukturierte Aktionen und ausgewählte Interaktionen
sind jeweils für konkrete technische Fälle geprüft. Das ist keine Zusage für jede
Spielversion, Fraktion, Vorlage, Kartenform oder Modkombination.

Für neue Funktionen gilt: Build, synthetische Tests und ein begrenzter Livefall mit
strukturierter Rücklesung. Bauprojekte brauchen zusätzlich Vorschau, Sessionbindung,
Aktions-ID und Nachprüfung der tatsächlich erzeugten Objekte.

Historische Spielweltdaten und Testprotokolle werden nicht mehr geführt.
