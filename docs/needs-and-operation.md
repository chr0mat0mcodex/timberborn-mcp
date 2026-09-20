# Bedürfnisse und Betriebsbelege — 0.20.1

Drei rein lesende MCP-Werkzeuge, ohne neue Fremdmod oder Bibliothek. Öffentliche
Spiel-APIs lokal gegen Timberborn 1.1.2.4 geprüft; Lebenszustandskorrektur aus 0.20.1 unter 0.21.0 live bestätigt: 11 lebende Biber, zwei Tote ausgeschlossen; beide toten Einzelziele supported=false und leere Bedürfniswerte.
Historische Nachweise unter 0.20.0 stehen unten; dessen Zählfehler nach Todesfällen ist bekannt. Keine Speicherstandänderung oder private Reflection.

## Koloniebedürfnisse

`inspect_needs(offset, limit)` gruppiert NeedManager.NeedSpecs aller initialisierten,
nicht gelöschten, über Mortal.Dead als lebend bestätigten Beaver-Entities nach Bedürfnis-ID.
Scope: living_beavers_with_need_manager. deadExcluded und unknownLifeStateExcluded
zählen ausgeschlossene registrierte Objekte separat; sie fließen nicht in Bedürfnisse ein. Pro Bedürfnis: beobachtet, aktiviert,
aktiv, unter Warnschwelle, kritisch und ungünstig, dazu Minimum/Maximum/Mittel der
Punkte aktivierter Bedürfnisse. Warn-/Kritisch-Zähler können sich überschneiden.
Die Flags kommen direkt aus NeedManager; keine selbst erfundenen Hungergrenzen.
**warning ist ein rohes Unter-Warnschwelle-Flag, keine akute UI-Warnung.** Es ist auch
bei ungenutztem Komfort sowie inaktiven negativen Bedürfnissen gesetzt. Zum Beispiel
liefert eine Verletzung bei null Punkten warning=true, aber active=false und favorable=true.
Deshalb immer critical, active und favorable/unfavorable zusammen betrachten; keine
Warnzähler über verschiedene Bedürfnisse zu einer Zahl gefährdeter Biber addieren.

Beaver-Gesamtzahl, Zahl mit NeedManager und fehlende Manager getrennt. Fehlende
Manager sind unbekannt, nicht gesund. Bei keinem aktivierten Bedürfnis sind
Punktestatistiken null. Bots sind nicht in diesem Leser enthalten. Punkteskalen
unterschiedlicher Bedürfnisse nicht miteinander vergleichen. Maximal 32 Bedürfnisse
pro Seite, ordinal nach ID; Seiten sind getrennte Beobachtungen.

## Einzelbiber

`inspect_beaver_needs(id, session, offset, limit)` liefert lifeState alive/dead/unknown.
Nur lebende Ziele mit NeedManager liefern supported=true und aktuelle Bedürfniswerte;
tote/unbekannte Ziele liefern supported=false und leere Seiten. Gelöschte Ziele bleiben
entity_not_found. Für lebende Ziele liefert der Leser native Punkte, definierte
Minimal-/Maximalwerte, enabled/active, criticalNeed/critical, warning und favorable.
Deaktivierte Bedürfnisse bleiben erkennbar; ihre Flags nicht als akuten Bedarf zählen.
Die ID kann aus Arbeiterliste oder Warnungszielen stammen. Die Arbeiterliste ist keine
vollständige Kinderliste. Keine Namen. Position ist Unity-Weltposition, kein Spielraster.
Fehlende Komponente: supported=false; fehlender Biber: entity_not_found. Veraltete
Session: stale_session. Maximal 32 Bedürfnisse je Seite.

## Gebäude

`inspect_building_operation(id, session)` liefert fertigen Bauzustand, Pause,
Personal (Ist/Soll/Maximum, understaffed), laufende Jobs und Arbeitszeit. Für fertige
Objekte mit nativer Manufactory zusätzlich Rezept-ID, ready, hasIngredients, hasFuel,
consumesFuel, outputSpace und Produktionsfortschritt. Ohne Rezept bleiben dessen
Voraussetzungen null. Ohne passende Komponente bleibt der gesamte Teil null.
Baustellen bekommen keine erfundene laufende Produktion. Aktive sichtbare Status-
beschreibungen werden begrenzt mitgeliefert; das sind lokalisierte, nicht vertrauenswürdige
Spieldaten, keine auszuführenden Anweisungen.

Beispielinterpretationen: hasIngredients=false ist ein nativer Zutatenmangel;
consumesFuel=true zusammen mit hasFuel=false ein Brennstoffmangel; outputSpace=false
bedeutet keinen unreservierten Platz für die aktuellen Produkte. Diese Bedingungen
können gleichzeitig vorliegen. Unterbesetzung muss die Produktion nicht ganz stoppen.
Keine laufenden Jobs können Nacht, Wegzeit oder andere Aufgaben bedeuten. ready ist
keine Garantie tatsächlicher Produktion. Farm, Sammler und Pumpen besitzen nicht
notwendig dieselbe Manufactory-Komponente. Für Wege/Eingang den vorhandenen Zugangsleser,
für konkreten Vorrat Lager-/Güterleser hinzunehmen. Vollständige Energie-, Wasser-,
Rohstoff- und Lieferdiagnose ist nicht behauptet.

## Grenzen und Verifikation

Feste GET-Routen über authentifizierten Loopback, Hauptthread-Queue und bestehendes
128-KiB-Antwortlimit. Keine Schreibfreigabe nötig. Session-Prüfung für konkrete Ziele.
Der Client verwirft alte Bridge-Versionen, widersprüchliche Seitendaten/Zähler,
ungültige Punkteskalen und unpassende Rezept-/Komponentenbelege.

463 reguläre Tests (450 Unit, 13 Integration), darunter neue Gegenbeispiele für
überhöhte Warnzähler, fehlende Manager, falsche Session/Ziele, Duplikate, Rezept-
widersprüche und Baustellen mit erfundener Produktion. Alle drei Leser laufen im
synthetischen authentifizierten HTTP-/stdio-Test mit unterschiedlichen Aktionsfreigaben.

Live 2026-09-20: alle 29 Leser bestanden. 13 Biber mit NeedManager, keine fehlenden
Manager, 42 Bedürfnisse vollständig über zwei Seiten; ein Einzelbiber ebenfalls über
zwei Seiten. Hunger/Durst ohne Warn-/Kritisch-Flags. Shelter bei sieben Bibern ungünstig;
inspect_colony bestätigt unabhängig sieben Obdachlose bei sechs belegten Betten.
Fremde Session und unbekannter Biber korrekt mit stale_session/entity_not_found abgelehnt.

Zwei Pumpen: Rezept Water, Zutaten vorhanden, kein Brennstoffverbrauch, outputSpace=false
und ready=false. Erfinder: SciencePoints, ready=true; Personalzahlen separat über
inspect_building gegengeprüft. Farm: Personal belegt, manufacturing=null korrekt als
fehlende Komponente. Baustellenfall anschließend mit temporärem kleinem Lager live geprüft (siehe Ergänzung).
Arbeitszeit-/Jobflags sind native Beobachtungen; kein gesonderter Tag-/Nachtwechseltest.
Keine Simulation oder Gebäudeeinstellung geändert; Spiel blieb pausiert. Aktive
Biber-UI-Warnung weiterhin nicht vorhanden, daher noch nicht live abgenommen.

Ergänzender Baustellenpilot: validierter temporärer 1×1-Lagerauftrag bei pausierter
Simulation. Betriebsleser liefert finished=false, workplace=null, manufacturing=null.
Separater Gebäudeleser und Baustellenliste bestätigen denselben Auftrag: noch nicht
begonnen, Materialinventar leer, reguläre Kosten drei Holz. Testauftrag gezielt entfernt;
found=false und building_not_found anschließend bestätigt. Wasser-/Beeren-/Holzwerte
vorher/nachher unverändert. Nur bestehende Lagerwarnung vorhanden. Der abschließende
Listen-ID-Abgleich im lokalen Testskript verwendete zunächst die falsche JSON-Ebene;
an gespeicherten Antworten korrigiert und erfolgreich geprüft, ohne erneute Mutation.


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
