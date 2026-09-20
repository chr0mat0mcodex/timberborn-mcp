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

## Einzelkorrektur und vorzeitig beendeter Vergleich

Am Tag 17 ein vorhandenes, leeres mittleres Lager (200 Plätze, Modus accept) nach
positiver Sofort-Wegprüfung vom Sammler und Distriktzentrum auf Berries gesetzt.
Auswahl separat bestätigt, „No good selected“ verschwand. Keine anderen Einstellungen
verändert. Zusatzbefunde: einziger Wassertank 30/30, Pumpen outputSpace=false;
Karottenfeld vier markierte Zellen. Ursachen und Wirkung getrennt betrachten.

Gleicher Messablauf von Tag 17 02:02 bis Tag 18 07:21, rund 1,22 Tage, fünf Messpunkte.
Beeren 208→221, Karotten 0→1 (zwischenzeitlich 12), Wasser 149→138, Holz 20→20.
Neuer Tagesdatensatz für Beeren: Produktion 19, Verbrauch 3, Kapazität 220 statt 20.
Karotten: Produktion 12, Verbrauch 10; ihre Ernte senkt zugleich den Beerenverbrauch.
Die Beerenproduktion ist gegenüber zuvor 2/6/4 pro Tagesdatensatz gestiegen. Das stützt
den Lagerengpass als Ansatzpunkt, isoliert aber keine langfristige Wirkung von Reifezyklen.
Die Auswahl Berries bleibt als nutzbare Korrektur bestehen.

Bevölkerung am letzten Messpunkt 13→11: automatische Stoppschwelle griff, Simulation
pausiert. Aktiver Entity-Status „Died of old age.“ mit zwei BeaverAdult-Zielen,
showAlert=false. Damit ist der Biber-Todesstatus samt Zielen belegt, keine Hunger-/Durst-
UI-Warnung. Keine Hunger-/Durstflags an den Messpunkten. Kein vollständiger Dreitages-
Vergleich und keine Nachhaltigkeitszusage; veränderte Bevölkerung beeinflusst Folgebilanzen.

Dabei Produktfehler entdeckt: 0.20.0 zählt weiter registrierte tote Biber in der
Bedürfnisübersicht mit (13 statt 11 lebend). 0.20.1 liest öffentlich Mortal.Dead,
schließt tote/unbekannte Lebenszustände aus der Bilanz aus und kennzeichnet sie separat.
Installation und erneute Live-Abnahme der Korrektur stehen aus.
