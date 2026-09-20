# Phase 2A: Ergebnis des begrenzten Schnittstellenpiloten

> Historischer Pilot-/Nachweisbericht. Alte Versionsstände und Grenzen gelten für den damaligen Test. Aktueller Einstieg: [native Installation](native-bridge-install.md), [generischer Bau](generic-building.md) und [Projektstand](../PROJECT_STATE.md).

Stand: 2026-09-19. Nutzer hat Paket 2A freigegeben. Pilot abgeschlossen mit dokumentierten Lücken;
die technischen Voraussetzungen für selbstständiges Bauen sind noch nicht erfüllt.
Keine neue Mod, keine Konfigurationsänderung, kein Bauauftrag und keine Schreibroute ausgeführt.

## Ergebnis

More HTTP API liefert live einen brauchbaren statischen Wohnbaukatalog und Teile der Bevölkerungs-/
Arbeitsplatzdaten. Globale Bestände und Bettendaten haben belegte Mod-interne Zugangswege,
aber keinen nachgewiesenen HTTP-Zugang. Raumabbild und reguläre Bauprüfung bleiben harte Lücken.
Ein unverändertes Weiterbauen nur am externen MCP-Adapter würde diese Informationen nicht erzeugen.

## Tatsächlich ausgeführter Live-Pilot

Sechs erfolgreiche GET-Abfragen an dokumentierte Leserouten, ohne Rohantworten zu speichern:

1. buildings: Feldinventar, Gebäudetypen und Einstellungstypen.
2. characters: aggregierte Zuordnungen, keine Namen oder IDs in diesem Bericht.
3. blueprints: verfügbare Spec-Typnamen, keine Exportfunktion.
4. blueprints/specs mit Timberborn.DwellingSystem.DwellingSpec: acht Wohnbauvorlagen.
5. buildings erneut: genau die Holzfällerflagge und ihre Arbeitsplatz-Einstellung.
6. derselbe Wohnbau-Spec-Typ erneut: gezielte Auswertung von Lodge.Folktails statt unübersichtlicher Gesamtausgabe.

Keine geratenen Ressourcen-/Kartenendpunkte, kein Blueprint-Export, keine Dateirouten, kein Distrikt-Probeaufruf.
Es handelt sich um direkte HTTP-Quellenprüfung; diese neuen Datenfelder sind noch nicht im MCP-Adapter implementiert.

Momentaufnahme: 21 Einträge in buildings, Typen DistrictCenter.Folktails, LumberjackFlag.Folktails und Path.
Damit enthält die Liste auch Wege, nicht nur betretbare Gebäude. Neun Erwachsene, vier Kinder;
13 Charaktere ohne Dwelling-Zuordnung, zwei Erwachsene mit Workplace-Zuordnung.
Das sind Zuordnungsbefunde, kein Nachweis von 13 offiziell gezählten Obdachlosen oder zwei aktuell arbeitenden Bibern.
Die Holzfällerflagge bleibt pausiert. Sämtliche Zahlen können sich bei weiterlaufendem Spiel ändern.

## Fähigkeitsmatrix

| Benötigter Nachweis | Befund | Status / verbleibende Lücke |
|---|---|---|
| Wasser-, Holz- und Nahrungsbestand | GoodStatsProvider benutzt GetGlobalResourceCount(goodId), liefert AvailableStock und InputOutputCapacity | Quellcode belegt; kein Live-Bestand über HTTP verfügbar/nachgewiesen. Nahrungsklassifikation und nutzbare lokale Vorräte separat klären |
| Betten und Obdachlose | GlobalPopulationStatsProvider liefert OccupiedBeds, FreeBeds, Homeless und deren Summe | Quellcode belegt; live nur fehlende Dwelling-Zuordnung ermittelt, keine Kapazitätsmessung |
| Arbeitskräfte | characters liefert Zuordnungen; Workplace-Setting live als JSON-String mit Value=1 | teilweise live; Value ist DesiredWorkers, nicht Ist-Besetzung oder Arbeitsfähigkeit. Employable/Unemployable nur Mod-intern belegt |
| Kartenregion | Entity hat live EntityId/State; Building hat keine Position. MapStatsProvider liefert nur Dimensionen/Name | keine belegte Kartenregion. Terrain, Wasser, Objektposition und Wegekonnektivität fehlen |
| Baukatalog | acht Wohnvorlagen, darunter vier Folktails-Typen; Kosten, Kapazität, Blockgeometrie und Eingang live vorhanden | positiver Teilnachweis; aktive Fraktion, Freischaltung und aktuelle Bebaubarkeit nicht durch Katalog bewiesen |
| Bauprüfung | in geprüften More-HTTP-API-Handlern keine entsprechende Route; Areas ist keine Bauprüfung | nicht verfügbar nachgewiesen; kein Dry-run auf einem realen Bauplatz möglich |

## Konkreter Wohnbau-Datensatz

Live aus dem statischen Katalog, nicht aus einem gebauten Haus:

| Feld | Lodge.Folktails |
|---|---|
| Materialkosten | 12 Log |
| ScienceCost | 0; kein allgemeiner Nachweis des Freischaltungszustands |
| Kapazität MaxBeavers | 3 |
| BlockObjectSpec.Size | X=2, Y=2, Z=1 |
| Eingang relativ zum Objekt | X=1, Y=-1, Z=0; HasEntrance=true |
| PlaceFinished | false |

Die Wohnvorlagenliste enthält beide Fraktionen. Ein Filter auf Folktails ist für unsere beobachtete
Kolonie plausibel, aber ersetzt künftig nicht den expliziten Fraktionsnachweis.
Koordinatenkonvention, Rotation und Eingangstransformation sind vor Platzierung zu prüfen.
Eine rechnerische Auswahl mehrerer Häuser wäre jetzt verfrüht: verfügbare Ressourcen, vorhandene
nutzbare Betten und Bauplätze fehlen als verlässliche Eingaben.

## Quellennachweise und wichtige Unterschiede

Herstellerrepository datvm/TimberbornMods, Commit 1c057bda82ec955d1712937da916cddf0f382f24.
Unverändert gegenüber Phase-2-Planung. Lokale Kombination zuletzt geprüft: Spiel 1.1.2.4,
More HTTP API 11.0.0, Moddable Timberborn 11.1.2. Quellcodegleichstand ist kein Binärgleichheitsnachweis.

- [GoodStatsProvider](https://github.com/datvm/TimberbornMods/blob/1c057bda82ec955d1712937da916cddf0f382f24/ModdableTimberborn/GameStats/Implementations/GoodStatsProvider.cs): globale verfügbare Bestände und Input-/Output-Kapazität, keine vollständige distriktbezogene Logistik.
- [GlobalPopulationStatsProvider](https://github.com/datvm/TimberbornMods/blob/1c057bda82ec955d1712937da916cddf0f382f24/ModdableTimberborn/GameStats/Implementations/GlobalPopulationStatsProvider.cs): Bettendaten und Arbeitskräfte mit eigenen Bedeutungen, nicht aus Alterszahlen erraten.
- [GameStatService](https://github.com/datvm/TimberbornMods/blob/1c057bda82ec955d1712937da916cddf0f382f24/ModdableTimberborn/GameStats/GameStatService.cs): HasStat/TryGetStat als Mod-interner Einstieg. Eine HTTP-Route wird dadurch nicht automatisch registriert.
- [MapStatsProvider](https://github.com/datvm/TimberbornMods/blob/1c057bda82ec955d1712937da916cddf0f382f24/ModdableTimberborn/GameStats/Implementations/MapStatsProvider.cs): Kartengröße ist keine Terrain-/Wasserkarte.
- [WorkplaceSettings](https://github.com/datvm/TimberbornMods/blob/1c057bda82ec955d1712937da916cddf0f382f24/ModdableTimberborn/BuildingSettings/BuiltInSettings/WorkplaceSettings.cs): GetModel liefert DesiredWorkers; ApplyModel ist eine interne Einstellungsmethode, keine belegte HTTP-Schreibroute.
- [Areas-Dokumentation](https://github.com/datvm/TimberbornMods/blob/1c057bda82ec955d1712937da916cddf0f382f24/ModdableTimberborn/Docs/Areas.MD): Charakterbereichsabfrage unterstützt; BlockObject-Tracking ausdrücklich unvollständig. Kein Terrain- oder Platzierungsvalidator.
- [BlueprintHandler](https://github.com/datvm/TimberbornMods/blob/1c057bda82ec955d1712937da916cddf0f382f24/MoreHttpApi/Handlers/BlueprintHandler.cs): tatsächlich verwendete Katalogroute. Export ist eine separate dateischreibende Funktion und wurde nicht verwendet.

## Architekturblocker und begrenzter nächster Vorschlag

**Überarbeitet nach Nutzer-Go:** Die [direkte Herstellerprüfung](architecture/native-game-api.md)
weist öffentliche Spielservices nach. Der folgende ursprüngliche Vorschlag einer Pflichtbasis auf
ModdableTimberborn ist damit überholt. Die Live-Befunde dieses Berichts bleiben gültig.

Der Pilot wird an diesem Punkt nicht zur Suche durch immer mehr Mods oder dekompilierte Spielassemblies ausgeweitet.
Der Nutzen weiterer externer HTTP-Adapterarbeit allein ist für das Bauziel gering: die fehlenden Daten
und Spieloperationen müssen zuerst auf der Spielseite erschlossen werden.

Empfehlung zur Freigabe: kleiner zusätzlicher Brückenmod mit zwei getrennten Nachweisen,
anstatt alle Phase-2-Werkzeuge vorab zu bauen.

1. **Lesendes Statistikmodul:** explizit erlaubte GoodAmount/GoodCapacity-, Betten- und Arbeitskräftestatistiken
   über GameStatService bereitstellen. Nur eine kleine versionierte Übersicht, null/unsupported bei fehlenden
   Stat-IDs, keine frei wählbaren internen Typen oder Methoden. Dazu eine Adaptererweiterung und UI-Abgleich.
   Aufwand relativ gering gegenüber Raum-/Bauzugriff, aber zusätzliche Mod-Build-/Installationskette nötig.
2. **Separater Machbarkeitsnachweis für Raum und Bauprüfung:** auf offizieller Modding-Dokumentation und
   dokumentierten Herstellerbeispielen zunächst genau eine Objektposition, einen kleinen Terrain-/Wasserausschnitt
   und eine normale Bauprüfung für Lodge.Folktails untersuchen. Keine Platzierung. Aufwand/Risiko deutlich höher;
   keine belastbare Zeitzusage, solange Spielservices und ihre Verträge unbekannt sind.
3. **Entscheidung vor weiteren Ausbauarbeiten:** erst wenn beide Nachweise tragen, 2B vervollständigen
   und einen konkreten 2C-Bauablauf spezifizieren. Bei fehlendem dokumentiertem Bauweg erneut stoppen
   und Aufwand/Alternativen vorlegen, bevor private Interna oder breites Reverse Engineering eingesetzt werden.

Mögliche Abhängigkeiten: vorhandenes ModdableTimberborn und dessen Voraussetzungen; vorhandener
More-HTTP-API-Router als Integrationskandidat statt eines zweiten Servers. Das genaue Hosting-, Auth- und
Hauptthread-Konzept muss vor Umsetzung anhand der dokumentierten Registrierung geprüft werden.
Keine Fremdmod ersetzen oder ändern. Nutzer installiert den zusätzlichen Mod selbst nach geprüftem Paket.
Keine globale SDK-/Unity-Installation ohne gesonderten Auftrag.

Erfolg des Statistikmoduls: drei repräsentative Güter und alle Bettensummen konsistent mit der UI;
Arbeitsfähigkeits- und Besetzungsbegriffe sauber getrennt; keine ungeschützte Schreibroute.
Erfolg des Raum-/Baupiloten: Position und Ausschnitt visuell plausibel, gültiger und ungültiger Standort
erklärbar geprüft, keine neue Entity oder Ressourcenänderung durch die Prüfung.
Wenn das nicht nachweisbar ist, bleibt der Agent vorerst Beobachter/Berater; das Bauziel bleibt offen.

## Abschluss

Pilotabdeckung: alle vorgesehenen Fähigkeitsgruppen eingeordnet; positive Live-Nachweise für
Katalog und Teilbeobachtungen, Quellennachweise für Statistikzugänge, explizite Lücken für Karte/Bauprüfung.
Keine Behauptung, Phase 2A habe alle Spielfähigkeiten erfolgreich erschlossen.
Nächste Entscheidung erforderlich: den oben begrenzten Brückenmod-/Machbarkeitsschritt freigeben.
