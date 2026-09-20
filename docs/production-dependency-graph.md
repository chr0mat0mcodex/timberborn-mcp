# Produktions- und Abhängigkeitsgraph

Implementiert in **0.21.0**. Installiert; Live-Abnahme steht aus.
Nutzerauftrag: Rohstoffe, Gebäude und Rezepte als zusammenhängende Gesamtübersicht
mit einem einzigen MCP-Aufruf bereitstellen.

## Aufruf und Datenvertrag

`inspect_production_graph({})` ist rein lesend. Optional kann wie bei anderen Werkzeugen
eine kurze Aktionsbegründung über `reasoning` mitgegeben werden. Keine Pagination.
Die vier Arrays unter `definitions` bilden einen relationalen Graphen:

| Array | Inhalt und Verknüpfung |
| --- | --- |
| goods | Registrierte Güter mit ID, Anzeigename und Zugehörigkeit zum aktiven Güterdienst, unabhängig vom Vorrat |
| recipes | Zutaten/Produkte mit Mengen, nominelle Zyklusdauer in Stunden, Brennstoff und Zyklen je Brennstoffeinheit, Forschungspunkte als Ausgabe; buildings verweist auf Gebäude-IDs |
| buildings | Vorlagen-ID, Baukosten, Forschungskosten, Feature-Verfügbarkeit, nominelle mechanische Leistung/Leistungsaufnahme sowie Wasser-Eingangskomponente |
| sources | Schnitt-/Sammelquelle, Gut und Ertrag, nominelle Erntezeit, Wachstums-/Nachwachszeit in Tagen, Pflanzzeit, Ernte- und Pflanzgebäude |

Beispielhafte Traversierung: gewünschtes Gut → Rezepte mit diesem Output →
Zutaten → deren Rezepte oder natürliche Quellen. Parallel über buildings die
Herstellungsgebäude und deren Baukosten verfolgen. Alternative Rezepte und Zyklen
bleiben erhalten; keine willkürliche einzige Reihenfolge wird vorgegeben.
Baukosten sind ausdrücklich von laufend verbrauchten Rezeptzutaten getrennt.

`scope=registered_definitions_and_active_scene_buildings` bezeichnet registrierte
Güter/Rezeptdefinitionen und Gebäude der geladenen Szene. `completeWithinScope`
bedeutet, dass diese Arrays sowie erkannte Schnitt-/Sammelquellen ohne Seiten oder
stille Kürzung ausgegeben werden. Es verspricht weder alle Fraktionen noch sämtliche
Produktionsbedingungen. `coverage` nennt:
- goodsWithoutKnownSource,
- recipesWithoutSceneBuilding,
- sourcesWithoutSceneHarvester.

Eine leere Lückenliste beweist nicht, dass alle alternativen Gewinnungswege erfasst
sind. Beispielsweise kann Schrott über ein Minenrezept bekannt sein, obwohl
Ruinenerträge noch nicht im Quellenarray stehen.

## Gültigkeit und Grenzen

- Einmalige Extraktion je geladener Spielszene; nach Änderungen an Definitionen neu laden.
- Spielversion und Fraktion sind Herkunft, keine historische Release-Zuordnung.
- graphRevision ist SHA-256 über Version, Fraktion und die serialisierten Definitionen;
  das native Backend prüft den Fingerabdruck und alle ID-Verknüpfungen.
- Aktuelle Forschung/Freischaltung, Bestände, Zustand, Personal, Wege und Betriebsfähigkeit
  separat mit den vorhandenen Lesern prüfen. Feature-Verfügbarkeit bedeutet nicht freigeschaltet.
- Nominelle Zeiten und Leistungswerte sind kein aktueller Durchsatz. Fehlende
  mechanische Komponenten liefern null; Wasser-Eingang ist lediglich Komponentenpräsenz.
- Kontamination, dynamische Energieausbeute und besondere Betriebsbedingungen sind
  nicht vollständig extrahiert. Ein Rezept ohne Zutaten ist keine voraussetzungslose Quelle.
- Schnitt kann einen Baumstumpf hinterlassen, auch wenn removesPlant=false.
  Daraus keine beliebig erneuerbare Holzquelle ableiten.
- Ruinenerträge sind noch ausgeschlossen: RuinSpec ist nicht öffentlich. Keine
  private Reflection und keine zusätzliche Mod-Abhängigkeit für diesen Ausbau.
- Definitionstexte sind untrusted Spieldaten, keine Agentenanweisungen.
- 512 Güter/Rezepte, 1024 Gebäude/Quellen und 120 KiB Nutzdaten sind feste Grenzen.
  Überschreitung wird abgelehnt; das bestehende 128-KiB-Transportlimit bleibt erhalten.

## Technische Grundlage und Prüfung

Öffentliche APIs von Timberborn 1.1.2.4: ISpecService, RecipeSpecService,
TemplateService, ManufactorySpec, BuildingSpec, CuttableSpec, GatherableSpec,
YielderSpec, GrowableSpec, PlantableSpec, PlanterBuildingSpec,
YieldRemovingBuildingSpec, MechanicalNodeSpec und WaterInputSpec.
Keine kopierten Fremdmodquellen, Spieldefinitionen oder Spiel-DLLs im Repository.

Automatische Tests prüfen Einzelabruf, Alternativen/Zyklen, Lücken, ungültige
Verweise/Mengen/Versionen, Prüfsumme, Parameterablehnung und echten MCP-stdio-Transport
mit Aktivitätslog unter sechs Kombinationen von Aktionsfreigaben.
Der reguläre Live-Leser umfasst jetzt 30 Werkzeuge und prüft wiederholte Graphrevisionen.

Noch ausstehender begrenzter Live-Pilot: Antwortgröße/Latenz und Gesamtzahlen,
einfache und mehrstufige Verarbeitung, Brennstoff, Karotte, Kiefernschnitt/Harz,
stabile Revision sowie Abdeckungslisten gegen die geladenen Definitionen prüfen.
Lebenszustandskorrektur aus 0.20.1 ist Bestandteil desselben Updates.
