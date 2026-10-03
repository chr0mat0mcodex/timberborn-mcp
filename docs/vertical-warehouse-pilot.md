# Vertikales Lagerprojekt — Etappe D

0.32.0: nach menschlichem Test-Gate am 2026-10-03 live bestanden.
Vorhandene Werkzeugnamen bleiben unverändert: `execute_vertical_stair_pilot` und
`inspect_vertical_stair_pilot`; neuer Modus `stair_platform_warehouse_pilot`,
`upperPathCount=2`. Alle MCP-Anfragen benötigen weiterhin `reasoning`.

Feste Reihenfolge: Treppe → Plattform → oberer Weg → Plattform → oberer Weg →
Plattform → SmallWarehouse.Folktails. Plattformen stehen auf Treppenhöhe,
Wege und Lager eine Ebene darüber. Lager drei Rasterzellen in Treppenrichtung,
Drehung `(Treppendrehung + 2) % 4`; Eingang zeigt auf den zweiten oberen Weg.
Keine freie Strecken- oder mehrgeschossige Höhenplanung.

Jeder Folgeschritt wartet auf fertige Vorgänger und pausiertes Spiel. Simulation
erfolgt separat und begrenzt; maximal 300 reale Sekunden je Wartephase. Vor dem
Lager müssen beide oberen Wege tatsächlich am gewählten Distrikt hängen.
Lagerbestätigung verlangt den realen Distrikteingang und, solange unfertig,
tatsächlichen Bauarbeiterzugang. Sie bestätigt nicht die spätere Nutzung.

Vor dem ersten Auftrag werden Zielgrundrisse auf reale Hindernisse geprüft und
eine vollständige bestehende Distriktvergleichsbaseline verlangt. Während des
Auftrags stoppen verlorene ursprüngliche Verbindungen den Ablauf. Jede
Platzierung erhält zusätzlich frische reguläre Spielvalidierung samt Vorschau.
Dies ist nachträgliche Überwachung, keine garantierte Vermeidung jeder Blockade:
Bauphasen-Vorabnachweis bleibt unbewiesen, `constructionCovered=false`.
Teilstände bleiben erhalten; keine Wiederholung oder Rücknahme unbestätigter Aktionen.

## Erwarteter Live-Nachweis

- Sieben reale Objekte in korrekter Reihenfolge; Warten setzt keine Folgeschritte vorzeitig.
- Lagerbaustelle tatsächlich builder-erreichbar und Eingang distriktverbunden.
- Nach begrenzter Simulation Lager fertig, Eingang frei, beide oberen Wege verbunden.
- Ausgewählte bestehende Anschlüsse vor/nach vergleichen; keine Vollschutzabnahme behaupten.
- Gleiche actionId liefert bisherigen Beleg ohne neue Objekte.

Synthetische Tests prüfen vier Drehungen, Schrittfolge, Trägerhöhe, Lagerrotation,
Versionsbindung, Wartephasen/Guard-Abbruch und Wiederholungsschutz. Ausführung
durch das menschliche Vorbereitungsskript, nicht durch den Agenten.

## Ergebnis des begrenzten Livefalls

Eine Drehung live geprüft: Treppe und drei Plattformen fertig, zwei obere Wege
fertig und distriktverbunden. Wartephasen vor fertigen Trägern beobachtet; Lager
erst nach drittem fertigem Träger und nutzbaren oberen Wegen gesetzt.
Lagerbaustelle: `buildersReachable=true`, Eingang frei, Distriktdistanz 22.
Danach separat Lager `finished=true`, Eingang weder blockiert noch unzugänglich,
Distriktdistanz weiterhin 22. Bestandsweg verbunden geblieben, entferntes
Vergleichslager vor/nach mit freiem Zugang und Distanz 11. Kein Guard-Abbruch.
Identische Aktions-ID lieferte alten completed-Beleg; genau sieben Objekte
im Projektbereich rückgelesen. Alle begrenzten Simulationsläufe endeten pausiert
ohne Spielzeitüberschreitung. Zwei tote Testflächen-Bäume regulär entfernt und
Plattform für 100 Forschungspunkte regulär freigeschaltet; kein Bestandsbau abgerissen.

Nicht live belegt im ursprünglichen Fall: übrige Drehungen, Guard-Negativfall, vollständiger Schutz
aller Baustellen und unverbundener Wegnetze, Lieferung/Betrieb. Keine Aussage
über allgemeine sichere Baufreigabe; B bleibt offen.

## Ergänzung E: Treppendrehung 0 auf unveränderter 0.33.0

Nach dem bereits bestandenen menschlichen Gate zusätzlich live geprüft:
Treppendrehung 0, daraus Lagerdrehung 2. Sieben Objekte einzeln mit
`details.finished=true` und korrekter Position rückgelesen. Wartefolge vor
unfertigen Vorgängern beobachtet; beide oberen Wege vor dem Lagerauftrag fertig
und am angegebenen Distrikt. Keine verlorene Vergleichsverbindung im Projektguard.

Lagerbaustelle `buildersReachable=true`, Eingang frei, Distriktdistanz 34.
Anschließend tatsächliches fertiges Lager, zugewiesener Distrikt und freier
Eingang mit Distanz weiterhin 34. Unterer Bestandsweg und beide oberen Wege
verbunden; entferntes Vergleichslager weiterhin frei mit Distanz 25.
Identische actionId liefert denselben completed-Beleg: exakt sieben Projektobjekte
vor/nach dem Replay. Fünf Bauzeitläufe à acht Spielstunden endeten jeweils
pausiert und ohne Überschreitung; kein neuer Code oder Modbuild erforderlich.

Fixture begrenzt hergestellt: zwei tote Birken entfernt; die zunächst markierte
junge Eiche nach ungeeigneter Höhenkontrolle wieder unmarkiert. Am Ersatzgrundriss
ein Wasserrad und eine Getriebewerkstatt samt drei rückgelesenen Schuttstapeln
regulär entfernt, keine Wege oder Vergleichslager. Diese Abrisse sind
Testvorbereitung, kein Beleg für Bestandsschutz durch den anschließenden Bau.

Damit Treppendrehungen 0 und 3 begrenzt belegt; 1/2 und allgemeiner
Bauphasen-Vorabnachweis offen. B-Minimalabschluss ist die konservative
Baustellensperre, nicht eine allgemeine Vorschau-Schutzgarantie.
Das derzeit deployte Fähigkeitsprofil katalogisiert weiterhin nur Drehung 3;
eine Erweiterung der strukturierten Nachweise braucht einen neuen MCP-Test-Gate.
