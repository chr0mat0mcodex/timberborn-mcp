# Eigene Spielschnittstelle: Machbarkeit und Zielarchitektur

Stand 2026-09-19. Nutzer hat Umplanung und Prüfung direkter Spielservices freigegeben.
Ergebnis: eigene kleine Spielmod bevorzugen; ModdableTimberborn ist keine notwendige Pflichtbasis.
Externer MCP-Server, Verträge, Prüfungen und allgemeine Bibliotheken bleiben erhalten.
More HTTP API bleibt zunächst Vergleichsadapter; Community-Mods dienen als Referenz.
Keine Fremdmods jetzt entfernen. Kein automatischer Backendwechsel innerhalb einer Aktion.

Aktualisierung 2026-09-20: Punkte 1/2 nach Nutzer-Go implementiert, eigener Mod-Build erfolgreich,
89 reguläre Tests bestanden. Installation und Live-Abnahme noch offen. Keine Fremdmod-Pflichtbasis
im nativen Backend. [Installationsanleitung](../native-bridge-install.md),
[gesicherte good references und extrahierte Erkenntnisse](../references/README.md).
Die folgenden Recherchebefunde beschreiben den Ausgangspunkt vor dieser Implementierung.

## Belegstufen und Grenzen

Offizielle Dokumentation und Beispiele belegen Registrierung und Lebenszyklus. Lokale DLL-Metadaten
belegen öffentliche Typen und Signaturen der vorhandenen Installation (bisheriger Spielversionsnachweis 1.1.2.4).
Das ist noch kein Nachweis erfolgreicher Dienstauflösung, Laufzeitsemantik oder nebenwirkungsfreier Bauprüfung.
Öffentlich zugänglich bedeutet nicht dauerhaft versionsstabil oder vollständig dokumentiert.

Zur Prüfung wurde ein temporärer MetadataLoadContext-Prüfer unter dem ignorierten .local-Verzeichnis
gebaut. Keine Spielmethoden aufgerufen, keine Methodenkörper dekompiliert, keine DLLs verändert.
Keine Spielassemblies, Maschinenpfade oder Rohdaten werden veröffentlicht. Produktionscode unverändert.

## Offizieller Einstieg

- [Coding basics](https://github.com/mechanistry/timberborn-modding/wiki/Coding-basics): eigenes C#-Projekt
  mit Spiel-DLL-Referenzen möglich; Unity nicht zwingend, IModStarter als Startpunkt.
- [Timberborn architecture](https://github.com/mechanistry/timberborn-modding/wiki/Timberborn-architecture):
  Configurator mit Context("Game"), Konstruktorinjektion, ILoadableSingleton/IUpdatableSingleton;
  BlockObjectValidationService/IBlockObjectValidator werden ausdrücklich gezeigt.
- Geprüfter offizieller Beispielstand: `976487702864412bbcd2dbd6abaf1058465e1ea4`:
  [HelloWorldConfigurator](https://github.com/mechanistry/timberborn-modding/blob/976487702864412bbcd2dbd6abaf1058465e1ea4/Assets/Mods/HelloWorld/Scripts/HelloWorldConfigurator.cs),
  [HelloWorldInitializer](https://github.com/mechanistry/timberborn-modding/blob/976487702864412bbcd2dbd6abaf1058465e1ea4/Assets/Mods/HelloWorld/Scripts/HelloWorldInitializer.cs).
- Dokumentation kann Experimental voraussetzen; tatsächliche Referenzversionen vor dem Build prüfen.

Der offizielle Importer enthält einen DllPublicizer für erweiterte Sichtbarkeit importierter DLLs.
Dieser wurde nicht ausgeführt. Für die unten öffentlich nachgewiesenen Zugänge ist das nicht grundsätzlich
nötig; keine Publicizer-/Harmony-Abhängigkeit vorsorglich aufnehmen.

## Lokal nachgewiesene öffentliche Zugänge

| Bereich | Signaturen / Eigenschaften | Aussage und Grenze |
|---|---|---|
| Güter | ResourceCountingService.GetGlobalResourceCount(string), GetDistrictResourceCounter(DistrictCenter) | direkter Zugriff vorhanden; Semantik im UI prüfen |
| Bestandsdetails | ResourceCount.AvailableStock, AllStock, StockpiledStock, BufferedOutputStock, BufferedInput, StockUnderProcessing, CarriedToStockpilesStock, CarriedToProcessors, InputOutputCapacity, TotalCapacity | mehr Detail als bisheriger Adapter; nicht ungeprüft addieren |
| Wohnraum | PopulationService.GlobalPopulationData.BedData; BedData.OccupiedBeds/FreeBeds/Homeless | Bettendaten direkt vorhanden |
| Arbeitskräfte | PopulationData.BeaverWorkforceData/BotWorkforceData, BeaverWorkplaceData/BotWorkplaceData; WorkforceData.Employable/Unemployable/Total | Einsatzfähigkeit und Beschäftigung getrennt auswerten |
| Entitäten | EntityRegistry.Entities, GetEntity(Guid) | Einstieg zu vorhandenen Objekten; Komponentenfilter im Prototyp prüfen |
| Position/Baustatus | BlockObject.Coordinates, Orientation, FlipMode, PositionedBlocks, PositionedEntrance, Placement, IsFinished, IsUnfinished, IsPreview | öffentliche räumliche Daten vorhanden, noch nicht im Spiel ausgelesen |
| Gelände | ITerrainService.Size, GetTerrainHeight(Vector3Int), GetAllHeightsInCell(Vector2Int), Contains, OnGround | öffentliche Schnittstelle; 3D-Schichten beachten |
| Geländespalten | IThreadSafeColumnTerrainMap.GetColumnCount/GetColumnFloor/GetColumnCeiling, TerrainColumns | Index-/Koordinatenkonvention noch prüfen |
| Wasser | IThreadSafeWaterMap.WaterDepth(Vector3Int), ColumnContamination(Vector3Int), WaterHeightOrFloor, CellIsUnderwater, IsWaterOnAnyHeight | öffentliche lesende räumliche Daten |
| Bauvalidierung | BlockObjectValidationService.IsValid(BlockObject), AreValid(IReadOnlyList<BaseComponent>, out string); IBlockObjectValidator.IsValid(BlockObject, out string) | vorhandenes Objekt als Eingabe; noch kein fertiger Dry-run-Vertrag |
| Vorschau | PreviewFactory.Create(PlaceableBlockObjectSpec), PreviewPlacer.GetBuildableCoordinates(IEnumerable<Placement>), WarningText | Kandidaten für Prüfung; Lebenszyklus/Nebenwirkungen offen |
| Platzierung | BlockObjectPlacerService.GetMatchingPlacer(BlockObjectSpec); IBlockObjectPlacer.Place(EntitySetup.Builder, Placement) | regulärer Platzierungspfad als Kandidat; Kosten/Forschung/Ergebnis-ID nicht allein durch Signatur belegt |

TerrainService und WaterService selbst sind intern, die genannten Interfaces öffentlich.
Interfaces bevorzugen, statt Implementierungen mit Reflection zu öffnen. Mutationseinträge werden
nicht automatisch freigegeben. Keine MarkAsFinished-/Terraforming-Aufrufe als Abkürzung beim Bauen.

## Zielaufbau und Umsetzungsvorschlag

MCP-Client -> vorhandener MCP-Server -> NativeGameBackend -> versionierte lokale Schnittstelle
unserer Mod -> begrenzte Hauptthread-Aufträge -> öffentliche Spielservices.

Die neue Mod soll ohne More HTTP API, ModdableTimberborn, TimberUi oder Harmony als Pflichtabhängigkeit
auskommen, sofern der Prototyp das bestätigt. Neue Abhängigkeiten nur bei konkret belegtem Nutzen.
Transport, Port, Authentifizierung und Hauptthread-Queue vor einem installierbaren Paket festlegen.
Keine Spielservices aus HTTP-Arbeitsthreads aufrufen; beim Entladen Aufträge verwerfen und Sitzungskennung
erneuern. ThreadSafe-Interfaces garantieren keinen atomaren Gesamtsnapshot über mehrere Services.

Eigenes spielkompatibles Zielframework prüfen, nicht net10.0 des MCP-Servers ungeprüft übernehmen.
Spiel-DLLs nur lokal referenzieren, nicht in Git/Paket kopieren. Installation/Aktivierung übernimmt der Nutzer.

Nächste vorgeschlagene Pakete:

1. **Minimaler Build-Nachweis:** eigene Mod mit offizieller Game-Kontext-Registrierung und Spiel-DLL-Referenzen;
   Framework/Abhängigkeiten prüfen, keine globale SDK-/Unity-Installation ohne Auftrag.
2. **Lesender Diagnoseprototyp:** drei Güter, Betten/Arbeitskräfte, eine Objektposition und kleiner Terrain-/
   Wasserausschnitt; UI-Abgleich. Höchster unmittelbarer Nutzen bei überschaubarem Eingriffsrisiko.
3. **Getrennter Validierungspilot:** genau ein gültiger/ungültiger Wohnbaustandort; Entity-Anzahl und Materialbestand
   vor/nach prüfen, Vorschauen sauber entfernen. Keine Platzierung, bevor der Prüfablauf verstanden ist.
4. **Erst anschließend Bauauftrag:** normale Freischaltung/Kosten, Duplikatschutz und Ergebnisabfrage nachweisen.
   Die Void-Rückgabe von Place liefert noch keine sichere Gebäude-ID; kein Direkt-Spawn als Ersatz.

Güter/Wohnraum/Positionen haben geringere Unsicherheit, 3D-Karte mittlere und Bauvorschau/Platzierung die höchste.
Der Erkenntnisgewinn rechtfertigt die eigene Schnittstelle, aber noch keine Zusage vollständiger Spielsteuerung.
Nächste Freigabe betrifft Punkt 1/2; in diesem Schritt wurden nur Recherche und Planung ausgeführt.
