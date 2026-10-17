# Kompatibilität und Nachweise

0.73.0: Menschliches Gate und gezielter Livetest für Parameterdiagnose und frischen Fertigbaustand bestanden; Einzelplattform nach historischem Zeitlimit, keine allgemeine Betriebs-/Zugangsgarantie.

**0.72.0 abgeschlossen:** Gate, Laufzeitdiagnosen und vollständiger Plattform-Umwegpilot bestanden: 15/15 Teile fertig und zugänglich, Bestandswege erhalten, Journal freigegeben. [Livebelege](../releases/0.72.0.md).

**0.71.0: Gate und Umwegplatzierung bestanden, Fertigbau offen.** Oberer Treppenanschluss und seitliche Negativkontrolle bestätigt; automatischer Plattformumweg nativ gültig, 17/17 Teile platziert, keine verlorene Bestandsverbindung. [Belege und Learnings](../releases/0.71.0.md).

**0.70.0: Höhenkernpilot bestanden, Umweg offen:** native Vorschau registrierter Treppen-Zielwege, getrennte Anschlussbelege. Gate und bekannter Höhenbau bis Fertigzugang bestanden; erweiterter Umweg nativ abgelehnt. [Nachweisplan](../releases/0.70.0.md).

**0.69.0: Gate bestanden, Höhenpilot fehlgeschlagen:** native Weggraph-Prüfung für Bestandstreppen und automatisch geöffnetes MCP-Log. Beide Graphabfragen bleiben negativ, manuelle Gegenprobe gültig. Logfenster sichtbar und angehängt. [Nachweisplan](../releases/0.69.0.md).

0.68.0: Gate bestanden, Höhenpilot fehlgeschlagen, Diagnose live sichtbar:
[Suchpriorisierung und Anschlussdiagnose](../releases/0.68.0.md).

## Einordnung zum aktuellen Stand

0.67.0: Gate bestanden, Höhenpilot an Suchbudget gescheitert; keine Liveabnahme:
[Eigenkollisionen und vorhandene Treppen](../releases/0.67.0.md).

0.66.0: menschliches Gate und begrenzter Kernpilot bestanden; Uferpumpe bis
Fertigzugang sowie Tempoabweichung/konkurrierender Start mit gültigem Folgelauf:
[Uferpumpen und Startablehnungen](../releases/0.66.0.md).

0.65.0: Nach menschlicher Bereitmeldung begrenzter Kernpilot bestanden:
Wasserwarnungen, native Überflutung auch bei Pause und produktive Pumpenkontrolle.
Blockiertes Ansaugrohr und höherer Umweg bleiben synthetische Kontrollfälle.
[API, Regressionen und Livebelege](../releases/0.65.0.md).

0.64.0: menschliches Gate und begrenzter Livepilot am 17. Oktober 2026 bestanden.
Höhenbau bis Fertigzugang, Pause und Recovery mit Folgeauftrag ohne Reload.
[Konkrete Belege und Grenzen](../releases/0.64.0.md).

Vorheriger Bridge-Stand: 0.63.0. Menschliches Gate und begrenzte Liveabnahme
am 17. Oktober 2026 bestanden; [Nachweisgrenzen](../releases/0.63.0.md). Feature-spezifische Nachweise
stehen im [Projektstand](../../PROJECT_STATE.md) und den verlinkten Fachverträgen.
Die historischen Nachweise unten sind keine pauschale Abnahme neuer Fraktionen,
Karten oder Spielversionen.

## Versionsabschluss 0.37.0

Am 2026-10-11 abgeschlossen: [Releaseumfang und Nachweisgrenzen](../releases/0.37.0.md).
Die folgenden Einzelpiloten sind begrenzte Nachweise, keine pauschale Abnahme
aller Gebäudetypen oder Sonderstandorte.

## 0.37.0-Ergänzung autonome Charge — begrenzt live abgenommen

`run_large_project`: nach menschlichem Gate zwei vollständige Chargen mit fünf
und neun Teilen, automatische Folgeaufträge und Abschlusspause, negative Weg-
und Zeitlimitkontrolle bestanden. [Livetest](../autonomous-construction-batches.md).
Die Versionsnummer allein beweist diese Fähigkeit nicht. Frühere manuelle
Stützen-/Kraftpiloten gelten weiterhin nur für den damaligen Einzelschrittmodus.

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
