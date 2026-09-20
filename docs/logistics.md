# Erreichbarkeit, Arbeitsreichweite und Versorgungsverlauf

Codeversion und Installation: 0.19.2. Die Sofort-Wegsuche ist unter 0.19.1 live bestätigt.
Der konkrete Terrain-Reichweitenzugriff ist unter 0.19.2 für Farm/Holzfäller live bestätigt.
Vier zusätzliche Leser, keine Fremdmod oder Bibliothek.

## Gebäudezugang

`inspect_building_access(id, session)` fragt BlockableEntranceBuilding nach blockiertem
und unzugänglichem Eingang, IUnconnectedBuildingBlocker nach Anschlussblockade und
bei unfertigen Objekten ReachableConstructionSite nach Baumeister-Erreichbarkeit.
DistrictBuildingDistance.TryGetDistanceToDistrict liefert die native Distriktdistanz.
Fehlende Komponenten/Werte erscheinen als null, nicht als freie oder blockierte Route.
Accessible-Anzahl und gültige Accessible-Anzahl werden gesondert ausgegeben.

`inspect_road_connection(id, toId, session)` nutzt ab 0.19.1 Accessible.FindInstantRoadPath zwischen zwei
fertigen Blockobjekten mit jeweils genau einer gültigen Accessible-Komponente. Der
Spielservice prüft die eigentliche Wegverbindung, einschließlich seiner Weggeometrien;
kein selbstgebauter Nachbarschaftsgraph. Ergebnis ist gerichtet von id nach toId.
Bei fehlendem/mehrdeutigem Zugang: supported=false und connected/distance=null.
Bei negativer regulärer Wegsuche: supported=true, connected=false, distance=null.
Bei Erfolg: connected=true und native Distanz. Distanz ist keine Reisezeit. Es werden
weder Terrain-Abkürzungen noch Vorschauwege zu einer Straßenverbindung umgedeutet.

Die Daten können nach Bauänderungen bis zum nächsten Navigationstick verzögert sein.
Anschluss beweist nicht Personal, Materiallieferung, aktive Arbeit oder Produktion.
Fehlendes Objekt: building_not_found. Veraltete Session: stale_session. Keine Auswahl,
Kameraänderung oder reguläre Spielaktion; nur öffentliche Beobachtungs-/Suchmethoden.

## Arbeitsreichweite

`inspect_work_range(id, session, offset, limit)` liest ab 0.19.2 bevorzugt die öffentliche
Komponente `BuildingTerrainRange.GetRange()`. Deren natives ReadOnlyHashSet wird mit
seinem öffentlichen Enumerator gelesen. Nur wenn diese Komponente fehlt, dienen
vorhandene IBuildingWithRange-Provider als Alternative. `source` benennt eindeutig
`building_terrain_range`, `range_providers` oder `unavailable`. Keine Kreisnäherung.

Das fertige Gebäude liefert eindeutige Rasterzellen in Z/Y/X-Sortierung. Bei fehlendem
Zugang oder unfertigem Objekt bedeutet supported=false unbekannt, nicht Reichweite null.
Für die konkrete Terrainquelle bezeichnet rangeNames=[terrain_navigation] den technischen
Vertrag, keinen lokalisierten Spielnamen. Die alternative Quelle vereinigt ihre Provider.

32 Zellen pro Seite; Offset 0..65535. Enumeration auf 65536 Providerzellen begrenzt;
Überschreitung wird abgelehnt, nicht als vollständige Reichweite ausgegeben. Reichweite
ist keine feste Farmflächenzuordnung und keine Garantie für passende Gewächse, Personal
oder laufende Arbeit. Seiten sind frische Beobachtungen, kein atomarer Snapshot.

## Güterhistorie und Bilanz

`inspect_good_history(good, offset, limit)` liest GlobalGoodSamplingRegistry und die
bereits vom Spiel gespeicherten GoodSamples. Pro Eintrag: index, Cycle, Day, Stock,
Capacity, Production, Consumption. Reihenfolge bleibt diejenige des Spiels. Keine
neuen Save-Felder, keine dauerhafte eigene Zeitreihendatenbank und kein Hintergrundpoller.

netProduction ist die Differenz der beiden nativen Produktions-/Verbrauchszähler.
pageProduction und pageConsumption summieren nur die gelesene Seite. stockChange
ist getrennt die Differenz des letzten und ersten Bestandes der Seite; bei weniger
als zwei Einträgen null. StockChange darf nicht als Produktion interpretiert werden.
Unbekanntes Gut: available=false, leere Historie. Bekanntes Gut ohne Stichproben:
available=true, leere Historie; nicht als Aktivität null auslegen.

Abtastintervall und Vollständigkeit der aktuellen Stichprobe sind durch die öffentlichen
Signaturen allein nicht belegt. Daher keine erdachten Pro-Stunde-Raten, keine Behauptung
vollständiger Kalendertage und keine Prognose sicherer Dauer-Versorgung. Eine vom Spiel
belegte historische Bilanz ist verfügbar; Dauer, Tagesgrenzen und Fortschreibung werden
im Live-Pilot gezielt überprüft. 32 Stichproben je Seite, Zeit-/Session-Metadaten wie bisher.

## Belege und historische Korrekturschleife

Öffentliche lokale Metadaten und vorhandene Architektur geprüft. Bindito-Singletons und
Entity-Komponenten entsprechen dem bestehenden offiziellen Integrationsweg. Keine private
Reflection, Patches oder übernommenen Fremdmodquellen. Mod-Build erfolgreich. Tests prüfen
Unknown/False-Unterscheidung, falsche Ziele/Session, Seitengrenzen, Reichweitenordnung und
getrennte native Flüsse/Vorratsdifferenzen; stdio-/HTTP-Pfad ergänzt.

Live 0.19.0: alle 26 Leser ohne Protokollfehler; Distriktzentrum → Erfinder/Farm/Holzfäller
mit Distanzen 10/9/17. Historien Water/Berries/Log mit je 13 Samples gelesen.
Negativtest: entferntes einzelnes Wegstück wurde von FindRoadPath im pausierten Spiel
weiter als verbunden gemeldet, obwohl die Distriktdistanz fehlte. Wegstück regulär
wiederhergestellt. Farm und Holzfäller meldeten beim bisherigen Interface-Lookup
supported=false. Das ist ein korrektes Unknown-Ergebnis, aber keine erfüllte Reichweitenfunktion.

0.19.1 verwendet deshalb die öffentliche Sofort-Wegsuche sowie AllComponents.OfType
für Interface-Implementierungen statt registrierter Lookup-Schlüssel. Der Native-Client
lehnt Weg-/Reichweitenantworten aus 0.19.0 ausdrücklich ab. Die damalige Folgeprüfung ist unten dokumentiert.

Der damalige Prüfplan nach Installation: verbundenes Gebäudepaar und eine kontrolliert unterbrochene Verbindung;
Eingangsblockade/fehlender Zugang separat. Arbeitsreichweiten mindestens Farm/Holzfäller
gegen vorhandene Karten-/Flächenkoordinaten prüfen. Water/Berries/Log-Historien über
mindestens zwei Spielzeitpunkte lesen und native Produktions-/Verbrauchswerte mit
Bestandsänderung getrennt vergleichen. Spieländerungen kontrolliert rückabfragen;
keine Zusage allgemeiner Fraktions-, Treppen- oder Großkolonieabdeckung aus Einzeltests.


Historien-Zeitpilot unter 0.19.0: zwischen Tag 13 um 06:30 und etwa 12:02 blieb das
letzte Sample unverändert. Nach Fortschritt über Mitternacht bis Tag 14 um etwa 00:49
kam ein neues Water-Sample hinzu: Produktion 2, Verbrauch 13, Nettobilanz -11; der
historische Bestand sank passend von 186 auf 175. Dieser Tageswechsel ist live belegt;
Extrapolation zu dauerhafter Versorgung oder jeder Sampling-Sonderlage bleibt unzulässig.
Spiel anschließend wieder pausiert. Biberwarnung auch in diesem Zeitraum nicht vorhanden.


Erneuter Live-Pilot unter 0.19.1: Sofort-Wegsuche bestätigt connected=true vor der
Unterbrechung, false nach Entfernen eines einzelnen Wegstücks und wieder true nach
regulärem Wiederaufbau, alles bei pausierter Simulation. Wegstück wiederhergestellt.
Farm und Holzfäller liefern auch über AllComponents keine IBuildingWithRange-Provider.
Dieser Ansatz war daher unzureichend. Öffentliche Metadaten belegen den konkreten
BuildingTerrainRange.GetRange-Zugriff; 0.19.2 nutzt ihn direkt. Alte Reichweitenantworten
werden vom neuen Client verworfen. Der folgende Pilot bestätigt die Korrektur.

## Live-Abnahme 0.19.2 — 2026-09-20

Alle 26 Leser im opt-in MCP-/HTTP-Livetest bestanden. Der gezielte Terrain-Pilot
liefert supported=true und source=building_terrain_range: Holzfäller 611, Farm 485
Rasterzellen. Jeweils erste und letzte Seite geprüft; die letzte enthält genau einen
Eintrag und hasMore=false. Mittlere Seiten wurden nicht einzeln vollständig abgerufen.
Beide Gebäude sind per Sofort-Wegsuche mit dem Distriktzentrum verbunden (17/9).
Dieser Pilot war ausschließlich lesend.

Die Zellen belegen native Navigationsreichweite, keine Erntefähigkeit, konkrete
Arbeitszuordnung oder Produktionsleistung. Weitere Gebäudetypen und Änderungen der
Reichweite bleiben separate Testfälle. Biberwarnungen waren bislang nicht aktiv und
sind daher nicht live abgenommen.

Mehrtagspilot unter 0.20.0: drei neue Tagesdatensätze und elf Messpunkte über rund
3,05 Spieltage; negative Wasser-/Beeren-/Holzbilanz ohne beobachtete Hunger-/Durstflags.
[Messwerte, Zeitfenster und Grenzen](supply-balance.md).
