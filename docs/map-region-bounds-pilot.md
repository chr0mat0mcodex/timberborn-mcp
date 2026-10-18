# A12/N01-Pilot: konkrete Kartengrenzen

Stand 2026-10-18: menschliches Gate und Liveabnahme bestanden; N01 abgeschlossen.
Nur `inspect_map_region`, keine anderen Karten-/Bauwerkzeuge.

## Umfang und Vertrag

Die native Kartenlesung prüft Ursprung und gesamte Ausdehnung gegen das aktuelle
`terrain.Size`, bevor sie Zellen liest. Bei Überschreitung wird die erste betroffene
Achse in Reihenfolge x, y, z gemeldet, zusammen mit dem gültigen inklusiven Bereich
`0..size-1`. Die bisherigen Terrain-Contains-Prüfungen bleiben zusätzlich erhalten.
Schema-/Parserlimits wie depth 1..4 gelten vorher weiter und sind keine Aussage
über die tatsächliche Kartengröße. Keine Koordinatenkorrektur oder Abschneidung.

Eine eigene typisierte Argumentexception transportiert ausschließlich die erlaubte
Achse und positive ganzzahlige Größe durch beide Main-Thread-Queue-Pfade.
HTTP 400 liefert dafür `error=map_region_out_of_bounds`, `axis` und `size`.
Der Client akzeptiert diese Form nur für die Kartenroute; falsche Typen, Achsen,
Zusatzfelder und andere Routen bleiben inkompatible Antworten. Freie Fehlermeldungen
oder private Daten werden nicht übernommen. MCP bleibt bei `invalid_argument`,
`execution=not_applicable`, `retryable=false` und ergänzt den konkreten Bereich.
Keine zusätzliche Abfrage, Mutation oder automatische Wiederholung.

Bridge/API bleibt 0.74.0; Mod und MCP werden gemeinsam durch das Gate ausgeliefert.
Alte Bridges liefern weiterhin den allgemeinen Parameterfehler. Alte Clients
können die neue detaillierte HTTP-Fehlerform nicht interpretieren; die Abnahme
gilt nur für das gemeinsam installierte Paket, nicht für gemischte Stände.
Kein neuer Werkzeugname oder Katalogparameter, kein Codex-Neustart erforderlich.

## Nachweise und Stoppkriterium

17 bestandene Testfälle: gültiger Ursprung/Rand und passende Ausdehnung;
Ursprungs- und Ausdehnungsüberschreitung auf allen drei Achsen; beide Queue-Pfade;
HTTP-Fehlerformat bis zum MCP-Ergebnis; fünf ungültige Antworten/Routenkontrollen.
Bestehende allgemeine Fehlerbereinigung und Parserlimits bleiben bestehen.
Keine Tests oder Builds durch den Agenten ausgeführt.

Nach erfolgreichem menschlichen Gate und `live bereit`:

1. Verbundenes Backend starten; Status, aktuelle Sitzung und Gatekennungen prüfen.
2. `inspect_colony` liest die tatsächliche Kartengröße. Gültige Eckzelle
   `(sizeX-1,sizeY-1,sizeZ-1)` mit width/height/depth jeweils 1 lesen.
3. Je eine kleine Abfrage mit x=sizeX, y=sizeY bzw. z=sizeZ bei ansonsten gültigen
   Koordinaten: genau die betroffene Achse und `0..size-1`, `invalid_argument`,
   `not_applicable`, kein Retry. Nur innerhalb der veröffentlichten Parserlimits.
4. Eine Ausdehnung von zwei Zellen ab x=sizeX-1 muss dieselbe x-Grenze nennen.
   Anschließend gültige Randzelle erneut lesbar. Keine Bau- oder Simulationsaktion.

Bei falscher Grenze, unerwartetem Fehler oder fehlgeschlagener Randlesung stoppen
und gezielt diagnostizieren. Keine weitere A12-Familie in diesem Pilot.

## Abnahme

Menschliches Gate: 13 Paket-, 35 Supervisor-, 2256 Unit- und 19 Integrationstests
bestanden; drei optionale Livefälle übersprungen. Release-Builds ohne Warnungen
oder Fehler; fünf installierte Dateien per SHA-256 bestätigt. Alle 17 neuen Fälle
bestanden. Verbundenes Backend gestartet; Status erreichbar, native und nicht
simuliert. Build- und Katalogkennung stimmen mit dem Gate überein.

Livekarte 50×50×23: Eckzelle (49,49,22) vor und nach den Negativkontrollen
erfolgreich gelesen, dieselbe Sitzung. x=50 und y=50 melden jeweils 0..49;
z=23 meldet 0..22. x=49 mit width=2 meldet ebenfalls x und 0..49.
Alle vier Ablehnungen: invalid_argument, not_applicable und retryable=false.
Fehlerantworten enthalten weiterhin keine Session-/Beobachtungskennung; die
erfolgreichen Lesungen davor und danach bestätigen die Sitzung separat.
Kein Bau- oder Simulationsauftrag. Keine allgemeine Geometrie-/Zugangsabnahme.

## Bewertung

Hoher praktischer Diagnosewert bei kleinem Fachumfang. Aufwand entsteht vor allem
durch den kontrollierten Fehlertransport über Mod, Queue, HTTP und MCP; eine bloße
Textänderung im MCP könnte die tatsächlichen Kartengrenzen nicht kennen.
Eine zusätzliche Snapshot-Abfrage wurde vermieden: Grenzen stammen aus derselben
nativen Kartenprüfung. Nutzen/Aufwand nach Gate und Livebeleg positiv: kleine
fachliche Änderung, durch 17 gezielte Fälle und vier reale Ablehnungen abgesichert.
N01 ist im dokumentierten Kartenlesepfad abgeschlossen. Gesamt-A12 bleibt teilweise
offen; weitere Pakete sind auf Nutzerwunsch zurückgestellt.
