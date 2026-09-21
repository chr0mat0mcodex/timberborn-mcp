# Spielziele: 100 Biber und vollständiger Gebäudekatalog

Beauftragt am 2026-09-21. Reguläres Spiel über die eigene MCP-Mod; keine Cheats,
Save-Manipulation oder Screenshot-Steuerung. Dieser ausdrückliche Spielauftrag
erweitert den bisherigen Schwerpunkt der begrenzten Funktionstests.

## Abnahme

1. Mindestens 100 gleichzeitig lebende Biber, Erwachsene und Kinder zusammen;
   Bots und registrierte Verstorbene zählen nicht. Anfang: acht Erwachsene, zwei Kinder.
2. Jede reguläre Gebäudevorlage der aktuellen Folktails-Szene mindestens einmal
   fertiggestellt. Vorhandene fertige Gebäude zählen zum Bestand; Bauaufträge nicht.
   Historisch bestätigte Fertigstellungen bleiben auch nach späterem Abriss erhalten.
   Aktuell 23 von 157 regulären Vorlagen bestätigt; fünf Entwicklerwerkzeuge ausgeschlossen.
   Andere Fraktionen sind im aktuellen Katalog nicht enthalten und bleiben separat offen.

## Fortschritt am Tag 61

- Erste Etappe erreicht; zuletzt 33 lebende Biber (25 Erwachsene und acht Kinder).
- 40 Betten; vier weitere Lodges und der Transportposten in einer überwachten Baucharge fertig. Plateau und Dachzugang über Treppen verbunden.
- 23/157 Gebäudetypen fertig bestätigt; zusätzlich HaulingPost und GearWorkshop. Das erste Zahnrad ist tatsächlich produziert.
- 79 Eichen lebend bestätigt. Jetzt 33 Birken-Pflanzplätze; 24 vor der jüngsten Erweiterung tatsächlich bepflanzt, davon 23 lebend und ein Erntestumpf. Birken-Ernte und neue Bauminstanzen an früher belegten Pflanzstellen bestätigt.
- Zuletzt nach zusätzlichem Antrieb: 138 Wasser, 220 Beeren, sieben Karotten, 241 Holz, 35 Bretter und vier Zahnräder. Wasserlager auf 120 Plätze erweitert; beide neuen Tanks separat mit je 30 Wasser bestätigt. 180 zusätzliche Holzlagerplätze aktiv; 34 Karottenfelder markiert und Farm-Sollbesetzung drei.
- Pumpenpriorität VeryHigh, Nahrung und Holz High, Forschung Low; Arbeitsplatzbesetzung separat überwachen.

## Vorgehen

- Zuerst Holz, Wasser, Nahrung und Nachwuchs absichern; Wohnraum schrittweise erweitern.
- Danach Forschung, Bretter, Zahnräder, Energie und weitere Produktionsketten aufbauen.
- Bevölkerung mit Versorgungsreserven in Etappen 20 / 40 / 60 / 100 ausbauen.
- Fehlende Gebäudetypen systematisch aus dem Katalog ergänzen. Große Bauwerke,
  Sonderlayouts und Geländeanschlüsse benötigen teils noch Werkzeugerweiterungen.
- In begrenzten Simulationsabschnitten prüfen: lebende Bevölkerung, Bedürfnisse,
  Vorräte, Tagesbilanzen, Personal und konkrete Fertigstellungen. Bei akuter Unterversorgung
  pausieren und die Ursache bearbeiten; normale Alterstodesfälle einzeln einordnen.
- Keine Nachhaltigkeit aus Bauaufträgen oder Momentaufnahmen ableiten.

## Gebündelter Lauf

Nach dem Aufwandscheck beide Ziele bestätigt. Lokaler MCP-Batchhelfer verwendet
begrenzte Bauchargen und überwachte Simulationsintervalle statt einzelner
Agentenentscheidungen pro Abruf. Nach jedem Intervall Pause und Kontrolle von
Vorräten, Hunger/Durst, kritischem Personal, Wohnraum und Fertigstellungen.
Ablehnungen stoppen die Charge; angenommene Aufträge werden vor Fortsetzung
mit dem Spielbestand abgeglichen und nicht wiederholt. Kein Hintergrunddienst.

Nächste Etappe: Betrieb des zusätzlichen Antriebs prüfen, Pumpenstandort klären,
Nahrungsproduktion und Energieleistung für 40 und später 100 Biber erweitern.
Zwei Sägewerke und zwei Erfinder stehen; dauerhafte Versorgung ist noch nicht
nachgewiesen. Die 15 technischen Baugrenzen bleiben offen.

## Offene technische Baugrenzen

15 reguläre Vorlagen sind derzeit vom MCP-Baupfad ausgeschlossen. Sie bleiben
Teil des Ziels. Größenbegrenzung, Sonderlayout/-form und Geländeanschluss getrennt
prüfen; keine bloße Freischaltung aller Vorlagen ohne Spielvalidierung.

## Gebäudecheckliste

Stand: erste vollständige Katalog- und Bestandsabfrage am 2026-09-21.
Die Tabelle enthält Vorlagenbezeichner, keine Spiel-IDs oder lokalen Rohdaten.

| Vorlage | Bereich | Fertig bestätigt | MCP-Baugrenze |
| --- | --- | --- | --- |
| Agora.Folktails | Wellbeing | offen | geometry_exceeds_64_cells |
| Airlock.Folktails | Paths | offen | — |
| AquaticFarmhouse.Folktails | Food | offen | — |
| AquiferDrill.Folktails | Water | offen | — |
| BadwaterDome.Folktails | Water | offen | — |
| BadwaterPump.Folktails | Water | offen | — |
| BadwaterRig.Folktails | Water | offen | — |
| Bakery.Folktails | Food | offen | — |
| BeaverStatue.Folktails | Decoration | offen | — |
| Beehive.Folktails | Food | offen | — |
| Bench.Folktails | Decoration | offen | — |
| BotAssembler.Folktails | Science | offen | — |
| BotPartFactory.Folktails | Science | offen | — |
| BrazierOfBonding.Folktails | Monuments | offen | — |
| BuildersHut.Folktails | DistrictManagement | offen | — |
| BulletinPole.Folktails | Decoration | offen | — |
| Campfire.Folktails | Wellbeing | ja | — |
| Carousel.Folktails | Wellbeing | offen | geometry_exceeds_64_cells |
| Centrifuge.Folktails | Water | offen | — |
| Chronometer.Folktails | Automation | offen | — |
| Clutch.Folktails | Power | offen | — |
| CompactMechanicalPump.Folktails | Water | offen | — |
| ContaminationBarrier.Folktails | Landscaping | offen | special_layout |
| ContaminationSensor.Folktails | Automation | offen | — |
| ContemplationSpot.Folktails | Wellbeing | offen | — |
| Dam.Folktails | Landscaping | offen | special_layout |
| DanceHall.Folktails | Wellbeing | offen | — |
| DepthSensor.Folktails | Automation | offen | — |
| Detailer.Folktails | Wellbeing | offen | — |
| Detonator.Folktails | Automation | offen | — |
| DirtExcavator.Folktails | Landscaping | offen | geometry_exceeds_64_cells |
| Discharge.Folktails | Water | offen | — |
| DistrictCenter.Folktails | DistrictManagement | ja | — |
| DistrictCrossing.Folktails | DistrictManagement | offen | special_layout |
| DomedGarden.Folktails | Wellbeing | offen | geometry_exceeds_64_cells |
| DoubleDynamite.Folktails | Landscaping | offen | — |
| DoubleFloodgate.Folktails | Landscaping | offen | special_layout |
| DoubleLodge.Folktails | Housing | ja | — |
| DoublePlatform.Folktails | Paths | offen | — |
| Dynamite.Folktails | Landscaping | offen | — |
| EarthRecultivator.Folktails | Monuments | offen | special_tool_shape, geometry_exceeds_64_cells |
| EfficientFarmHouse.Folktails | Food | ja | — |
| ExplosivesFactory.Folktails | Landscaping | offen | — |
| FarmerMonument.Folktails | Monuments | offen | — |
| FillValve.Folktails | Landscaping | offen | — |
| FireworkLauncher.Folktails | Automation | offen | — |
| Floodgate.Folktails | Landscaping | offen | special_layout |
| FlowSensor.Folktails | Automation | offen | — |
| Forester.Folktails | Wood | ja | — |
| FountainOfJoy.Folktails | Monuments | offen | geometry_exceeds_64_cells |
| Gate.Folktails | Paths | offen | — |
| GathererFlag.Folktails | Food | ja | — |
| GearWorkshop.Folktails | Wood | ja | — |
| GeothermalEngine.Folktails | Power | offen | — |
| GravityBattery.Folktails | Power | offen | — |
| Grill.Folktails | Food | offen | — |
| Gristmill.Folktails | Food | offen | — |
| HallOfAbundance.Folktails | Monuments | offen | geometry_exceeds_64_cells |
| Hammock.Folktails | Decoration | offen | — |
| HaulingPost.Folktails | DistrictManagement | ja | — |
| Hedge.Folktails | Decoration | offen | — |
| Herbalist.Folktails | Wellbeing | offen | — |
| HttpAdapter.Folktails | Automation | offen | — |
| HttpLever.Folktails | Automation | offen | — |
| ImpermeableFloor.Folktails | Landscaping | offen | — |
| ImpermeablePowerShaft.Folktails | Power | offen | — |
| Indicator.Folktails | Automation | offen | — |
| Inventor.Folktails | Science | ja | — |
| Lantern.Folktails | Decoration | offen | — |
| LargePile.Folktails | Storage | ja | — |
| LargeTank.Folktails | Storage | offen | — |
| LargeWarehouse.Folktails | Storage | offen | — |
| LargeWaterPump.Folktails | Water | offen | — |
| LargeWindTurbine.Folktails | Power | offen | — |
| Levee.Folktails | Landscaping | offen | — |
| Lever.Folktails | Automation | offen | — |
| Lido.Folktails | Wellbeing | offen | — |
| Lodge.Folktails | Housing | ja | — |
| LumberjackFlag.Folktails | Wood | ja | — |
| LumberMill.Folktails | Wood | ja | — |
| MechanicalPump.Folktails | Water | offen | — |
| MedicalBed.Folktails | Wellbeing | offen | — |
| MediumTank.Folktails | Storage | offen | — |
| MediumWarehouse.Folktails | Storage | ja | — |
| Memory.Folktails | Automation | offen | — |
| MetalPlatform3x3.Folktails | Paths | offen | — |
| MetalPlatform5x5.Folktails | Paths | offen | — |
| Mine.Folktails | Metal | offen | geometry_exceeds_64_cells |
| MiniLodge.Folktails | Housing | ja | — |
| MudPit.Folktails | Wellbeing | offen | — |
| Observatory.Folktails | Science | offen | — |
| Overhang2x1.Folktails | Paths | offen | — |
| Overhang3x1.Folktails | Paths | offen | — |
| Overhang4x1.Folktails | Paths | offen | — |
| Overhang5x1.Folktails | Paths | offen | — |
| Overhang6x1.Folktails | Paths | offen | — |
| PaperMill.Folktails | Wood | offen | — |
| Path | Paths | ja | — |
| Platform.Folktails | Paths | offen | — |
| PoleBanner.Folktails | Decoration | offen | — |
| PopulationCounter.Folktails | Automation | offen | — |
| PowerMeter.Folktails | Automation | offen | — |
| PowerShaft.Folktails | Power | offen | — |
| PowerWheel.Folktails | Power | ja | — |
| PrintingPress.Folktails | Wood | offen | — |
| Refinery.Folktails | Science | offen | — |
| Relay.Folktails | Automation | offen | — |
| ResourceCounter.Folktails | Automation | offen | — |
| Roof1x1.Folktails | Decoration | offen | — |
| Roof1x2.Folktails | Decoration | offen | — |
| Roof2x2.Folktails | Decoration | offen | — |
| Roof2x3.Folktails | Decoration | offen | — |
| Roof3x2.Folktails | Decoration | offen | — |
| RooftopTerrace.Folktails | Wellbeing | offen | — |
| Sauna.Folktails | Wellbeing | offen | — |
| Scarecrow.Folktails | Decoration | offen | — |
| ScavengerFlag.Folktails | Metal | offen | — |
| ScienceCounter.Folktails | Automation | offen | — |
| Shower.Folktails | Wellbeing | offen | — |
| Shrub.Folktails | Decoration | offen | — |
| SmallPile.Folktails | Storage | ja | — |
| SmallTank.Folktails | Storage | ja | — |
| SmallWarehouse.Folktails | Storage | ja | — |
| Smelter.Folktails | Metal | offen | — |
| Speaker.Folktails | Automation | offen | — |
| SpiralStairs.Folktails | Paths | offen | — |
| SquareBanner.Folktails | Decoration | offen | — |
| Stairs.Folktails | Paths | ja | — |
| StreamGauge.Folktails | Decoration | offen | — |
| SuspensionBridge1x1.Folktails | Paths | offen | — |
| SuspensionBridge2x1.Folktails | Paths | offen | — |
| SuspensionBridge3x1.Folktails | Paths | offen | — |
| SuspensionBridge4x1.Folktails | Paths | offen | — |
| SuspensionBridge5x1.Folktails | Paths | offen | — |
| SuspensionBridge6x1.Folktails | Paths | offen | — |
| TappersShack.Folktails | Wood | offen | — |
| TeethGrindstone.Folktails | Wellbeing | ja | — |
| TerrainBlock.Folktails | Landscaping | offen | terrain_side_attachment |
| ThrottlingValve.Folktails | Landscaping | offen | — |
| Timer.Folktails | Automation | offen | — |
| TripleDynamite.Folktails | Landscaping | offen | — |
| TripleFloodgate.Folktails | Landscaping | offen | special_layout |
| TripleLodge.Folktails | Housing | offen | — |
| TriplePlatform.Folktails | Paths | offen | — |
| Tunnel.Folktails | Landscaping | offen | — |
| UndergroundPile.Folktails | Storage | offen | — |
| VerticalPowerShaft.Folktails | Power | offen | — |
| WaterPump.Folktails | Water | ja | — |
| WaterWheel.Folktails | Power | offen | — |
| WeatherStation.Folktails | Automation | offen | — |
| Weathervane.Folktails | Decoration | offen | — |
| WindTurbine.Folktails | Power | offen | — |
| WoodFence.Folktails | Decoration | offen | — |
| WoodWorkshop.Folktails | Wood | offen | — |
| ZiplineBeam.Folktails | Paths | offen | — |
| ZiplinePylon.Folktails | Paths | offen | — |
| ZiplineStation.Folktails | Paths | offen | — |
