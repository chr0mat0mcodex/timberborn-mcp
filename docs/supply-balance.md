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
Die Korrektur ist inzwischen unter 0.21.0 live bestätigt: 11 lebende, zwei ausgeschlossene tote Biber; beide toten Einzelziele ohne aktuelle Bedürfniswerte.


## 2026-09-20 — zusätzlicher Wassertank als begrenzter Wirkungstest

Unter 0.21.0 zuerst erneut lesend geprüft: zwei besetzte, aktive Pumpen mit
hasIngredients=true, hasFuel=true, outputSpace=false. Einziger Tank Water/accept,
30/30 gefüllt. Daraus einen einzelnen Lagerkapazitätstest abgeleitet.

Produktionsgraph nennt 15 Holz für SmallTank. Freier Bauplatz neben vorhandenem
Tank, fertiger Weg am Eingang, 20 Holz global; reguläre Spielvalidierung bestanden.
Ein Bauauftrag erteilt, separat unfertige Baustelle bestätigt. Nach erstem
15-Sekunden-Abschnitt bei 7× fertig; auf Water gestellt und rückgelesen.
Nach zweitem Abschnitt neuer Tank 30/30, bestehender Tank ebenfalls gefüllt.
Gebäudezugang frei, Distriktdistanz 4; Sofort-Wegverbindung von beiden Pumpen bestätigt.

| Wasserfeld | Vorher | Nachher |
| --- | ---: | ---: |
| Gesamtbestand | 138 | 138 |
| Lagerbestand | 30 | 60 |
| Globale Ausgangsbestände | 108 | 78 |
| Gesamtkapazität | 80 | 110 |

Die 30 Einheiten sind als Umlagerung belegt. Beide Pumpen melden am Ende weiterhin
outputSpace=false. Kein Nachweis höherer Nettoproduktion oder nachhaltiger Versorgung;
nicht automatisch weitere Tanks bauen. Vorhandene Pufferbestände übersteigen weiterhin
die ausgewiesene Kapazität, deren genaue Zusammensetzung bleibt zu untersuchen.
Eine reine Differenz der Gesamtbestände ist keine Produktions-/Verbrauchsmessung.

Tag 18 etwa 07:21 bis 18:20, rund 0,46 Spieltage; 11 lebende Biber an den
Kontrollpunkten, keine Hunger-/Durstwarnflags. Beeren 221→214, Holz 20→16 trotz
Baukosten 15 (gleichzeitige Holzgewinnung nicht aus Nettobestand allein quantifizieren).
Tank bleibt als nutzbare Testkorrektur bestehen, Simulation am Ende bestätigt pausiert.
Keine neue Modversion, keine Screenshots, keine weiteren Gebäude-/Personaländerungen.


## Inventardiagnose 0.21.1 — 2026-09-20

Korrektur der bisherigen Interpretation: BufferedOutputStock ist ein globaler
Ausgangsbestandszähler, kein nachgewiesener Pumpenbestand. Die eigene Bridge
übernimmt das öffentliche ResourceCount-Feld unverändert. Die offizielle
DistrictCenter.Folktails-Definition enthält SimpleOutputInventorySpec mit Capacity=20
und IgnorableCapacity=true. Damit ist eine Überschreitung nomineller Kapazität
grundsätzlich möglich; die konkrete Verteilung in dieser Kolonie ist noch nicht belegt.

inspect_building_operation erhält inventories für fertiggestellte Gebäude:
aktivierte Inventory-Komponenten mit Komponentenname, vom Spiel gemeldeter Gesamtkapazität,
TotalStock, Input-/Output- und öffentlichen Zugriffsflags, IsFull/IsFullyReserved/
IsUnblocked sowie je Gut Bestand, unreservierter Bestand, reservierte Kapazität und
unreservierte Kapazität. Rohwerte; keine erfundene Ursache oder Lieferzusage.

Quellen sind öffentliche Inventory-Methoden und BaseComponent.GetComponentsAllocating.
Maximal acht Inventare mit jeweils 64 Gütern, kein stilles Abschneiden. Inventories=null
bedeutet bei älteren Bridges oder unfertigen Gebäuden unbekannt; [] bedeutet keine
aktivierten Inventarkomponenten am fertigen Ziel. Keine Träger-/Baustelleninventarliste,
keine globale Summengleichheit zugesichert. Freie Kapazitäten je Gut nicht addieren;
Kapazität kann vom Spiel ignoriert werden, dieses Flag ist hier nicht als Laufzeitwert verfügbar.

0.21.1 ist installiert. Gezielter Live-Abgleich von Distriktzentrum, beiden Pumpen und Tanks
bestanden. Spiel in diesem Entwicklungsschritt nicht verändert.


## 2026-09-20 — Inventardiagnose 0.21.1 live bestätigt

Alle 30 MCP-Leser bestanden; gezielter rein lesender Abgleich an fünf Gebäuden.
Simulation blieb pausiert (Tag 18, etwa 18:20), Bevölkerung elf Erwachsene.

| Gebäude | Wasserbestand | Gemeldete Inventarkapazität |
| --- | ---: | ---: |
| Kleiner Tank 1 | 30 | 30 |
| Kleiner Tank 2 | 30 | 30 |
| Wasserpumpe 1 | 15 | 15 |
| Wasserpumpe 2 | 15 | 15 |
| Distriktzentrum | 48 | 2147483647 |

Summe 138, exakt gleich dem globalen Wasserbestand. BufferedOutputStock=78 umfasst
hier 30 Pumpenwasser plus 48 Wasser im Distriktzentrum; StockpiledStock=60 die Tanks.
Keine Wasser-Kapazitätsreservierungen, jeweils gesamter Wasserbestand unreserviert.
Die Pumpen sind tatsächlich voll (outputSpace=false), nicht fehlerhaft überfüllt.

Beim Distriktzentrum meldet Inventory.Capacity int.MaxValue, die Definition dagegen
Capacity=20 und IgnorableCapacity=true. Die globale TotalCapacity=110 ist daher
nicht durch Addition der fünf rohen Inventory.Capacity-Werte zu rekonstruieren.
UnreservedCapacity(Water)=0 bei gleichzeitig Full=false im Distriktzentrum zeigt
ebenfalls: güterspezifische Grenzen und aggregierte Flags nicht gleichsetzen.
FullyReserved=true bei vollen Tanks/Pumpen beweist keine aktive Lieferreservierung.

Die Bestandszuordnung ist geklärt. Keine nachhaltige Produktionsbilanz oder
vollständige Umwelt-/Lieferdiagnose abgeleitet. Kein weiterer Tankbau erforderlich
für diesen Nachweis; nächste Versorgungsprüfung muss normale Entnahme und
Wiederauffüllung bzw. getrennte Produktions-/Verbrauchshistorie betrachten.
