# Versorgungsbilanz: begrenzter Live-Pilot

Stand 2026-09-20, Bridge 0.20.0, kleine Folktails-Entwicklungskolonie.
Der Agent kann Bedarf und Bestandsentwicklung über mehrere Spieltage beobachten.
Eine ausgeglichene Grundversorgung wurde in diesem Pilot **nicht** nachgewiesen.

## Methode und Abbruchgrenzen

Von Tag 14, 00:49 bis Tag 17, 02:02: rund 3,05 Spieltage. Zehn Abschnitte von je
20 Echtzeitsekunden bei 7×, zwischen jedem Abschnitt Pause und strukturierte Abfragen.
Elf Messpunkte mit Güterbeständen, Bevölkerung und nativen Hunger-/Durstflags.
Abbruch bei Bevölkerungsverlust, Hunger-/Durstflag oder verfügbarem Wasser unter
max(30, zweimal Biberzahl); außerdem feste Zeit-/Aufrufgrenzen. Keine Gebäude-,
Personal-, Lager- oder Flächenänderungen während dieses Tests. Simulation am Ende pausiert.

## Ergebnis

| Gut | Bestand Start | Bestand Ende | Änderung | Native Produktion* | Nativer Verbrauch* |
| --- | ---: | ---: | ---: | ---: | ---: |
| Wasser | 175 | 149 | -26 | 10 | 39 |
| Beeren | 232 | 208 | -24 | 12 | 39 |
| Karotten | 0 | 0 | 0 | 0 | 0 |
| Holz | 32 | 20 | -12 | 0 | 12 |

*Summen der drei neu hinzugekommenen gespeicherten Tagesdatensätze (Tag 15–17).
Diese Zeitfenster sind nicht exakt identisch mit den beiden Momentaufnahmen: Der
letzte gespeicherte Wasserbestand war 146, die spätere Endabfrage bereits 149.
Deshalb native Produktion/Verbrauch nicht aus den Start-/Endbeständen errechnen.

An allen elf Messpunkten 13 Biber, keine Hunger-/Durst-Warnschwellen oder kritischen
Hunger-/Durstzustände. Das belegt die Messpunkte, keine lückenlose Überwachung jeder
Simulationssekunde. Einzige sichtbare Statusgruppe am Ende: Lagergut nicht ausgewählt;
betroffenes Ziel als Gebäude bestätigt. Kein aktiver Biber-UI-Warnungsnachweis.

## Fachliche Bewertung

- Wasser und Beeren: negative native Bilanz in allen drei neuen Tagesdatensätzen.
  Reserven tragen die Versorgung; ausreichender aktueller Vorrat ist kein
  Nachhaltigkeitsnachweis. Die genaue Ursache ist separat zu untersuchen.
- Holz: keine Produktion, zwölf Einheiten nativer Verbrauch. Verbrauch ist belegt,
  dessen konkreter Empfänger wird durch die globale Historie nicht identifiziert.
- Karotten: keine Ernte im Messfenster. Ein früherer Tagesdatensatz enthält zwölf
  produzierte und zwölf verbrauchte Karotten. Ein begrenztes Fenster ohne Ernte
  beweist weder defekte Farm noch ausreichende langfristige Nahrungsproduktion.
- Wohnraum: vorher separat sieben Obdachlose bei sechs Betten nachgewiesen; in diesem
  Pilot keine zusätzlichen Wohnungen gebaut. Grundversorgungsabnahme bleibt offen.

## Nächster gezielter Nachweis

Produktionsengpässe mit den vorhandenen Betriebs-, Lager-, Personal-, Flächen- und
Reichweitenlesern eingrenzen. Frühere Pumpenbeobachtung outputSpace=false zusammen
mit der negativen Bilanz ist ein Ansatzpunkt, noch keine bewiesene Ursache. Globaler
Bestand, lokale Puffer und erreichbare Lagerkapazität getrennt betrachten. Danach
höchstens eine begründete Korrektur und dieselbe Bilanz erneut vergleichen. Keine
lineare Prognose dauerhafter Versorgung aus diesen drei Tagen.
