# Erreichbarkeit, Arbeitsreichweite und Versorgungsverlauf

Codeversion 0.19.0; gegen öffentliche Timberborn-1.1.2.4-APIs gebaut. Installation und
Live-Abnahme stehen aus. Vier zusätzliche Leser, keine neue Fremdmod oder Bibliothek.

## Gebäudezugang

`inspect_building_access(id, session)` fragt BlockableEntranceBuilding nach blockiertem
und unzugänglichem Eingang, IUnconnectedBuildingBlocker nach Anschlussblockade und
bei unfertigen Objekten ReachableConstructionSite nach Baumeister-Erreichbarkeit.
DistrictBuildingDistance.TryGetDistanceToDistrict liefert die native Distriktdistanz.
Fehlende Komponenten/Werte erscheinen als null, nicht als freie oder blockierte Route.
Accessible-Anzahl und gültige Accessible-Anzahl werden gesondert ausgegeben.

`inspect_road_connection(id, toId, session)` nutzt Accessible.FindRoadPath zwischen zwei
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

`inspect_work_range(id, session, offset, limit)` liest GetBlocksInRange sämtlicher
öffentlicher IBuildingWithRange-Provider des fertigen Gebäudes. Ergebnis ist eine
entdoppelte Vereinigungsmenge von Spielrasterzellen, sortiert Z/Y/X, mit rangeNames.
Keine geometrische Kreisnäherung und kein angenommener Radius. Ohne Provider oder bei
unfertigem Objekt: supported=false; das bedeutet unbekannt, nicht Reichweite null.

32 Zellen pro Seite; Offset 0..65535. Enumeration auf 65536 Providerzellen begrenzt;
Überschreitung wird abgelehnt, nicht als vollständige Reichweite ausgegeben. Mehrere
Provider können unterschiedliche Zwecke haben; die Vereinigungsmenge ist keine feste
Farmflächenzuordnung und keine Garantie, dass ein bestimmtes Gewächs bearbeitet wird.
Alle Seiten sind frische Beobachtungen, kein atomarer Snapshot.

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

## Belege und nächster Pilot

Öffentliche lokale Metadaten und vorhandene Architektur geprüft. Bindito-Singletons und
Entity-Komponenten entsprechen dem bestehenden offiziellen Integrationsweg. Keine private
Reflection, Patches oder übernommenen Fremdmodquellen. Mod-Build erfolgreich. Tests prüfen
Unknown/False-Unterscheidung, falsche Ziele/Session, Seitengrenzen, Reichweitenordnung und
getrennte native Flüsse/Vorratsdifferenzen; stdio-/HTTP-Pfad ergänzt.

Nach Installation: verbundenes Gebäudepaar und eine kontrolliert unterbrochene Verbindung;
Eingangsblockade/fehlender Zugang separat. Arbeitsreichweiten mindestens Farm/Holzfäller
gegen vorhandene Karten-/Flächenkoordinaten prüfen. Water/Berries/Log-Historien über
mindestens zwei Spielzeitpunkte lesen und native Produktions-/Verbrauchswerte mit
Bestandsänderung getrennt vergleichen. Spieländerungen kontrolliert rückabfragen;
keine Zusage allgemeiner Fraktions-, Treppen- oder Großkolonieabdeckung aus Einzeltests.
