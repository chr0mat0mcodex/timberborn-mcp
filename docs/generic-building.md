# Generisches Bauen — 0.14.0 / Patch 0.14.1

Ziel: Grundversorgung im Entwicklungsspielstand regulär aufbauen, ohne für jeden
Gebäudetyp ein eigenes MCP-Werkzeug zu implementieren. Keine neue Fremdmod-Abhängigkeit.
0.14.0 live geprüft: kompletter Katalog (162 Einträge, davon 119 unterstützt), vier
reguläre Aufträge für Farmhaus, Lodge, mittleres Lager und Pumpe mit getrennten
Entity-Rücklesungen; belegter Standort in Validator und Platzierer abgelehnt.
Patch 0.14.1 korrigiert den dabei erkannten zu engen Layoutfilter; noch nicht live geprüft.

| Werkzeug | Zweck |
|---|---|
| inspect_build_options(offset, limit) | Gesamter seitenweiser Gebäudekatalog der aktiven Spiel-Template-Sammlungen; maximal 32 Einträge je Seite |
| precheck_building(template, x, y, z, rotation) | Rein lesende Hindernis-, Gelände-, Eingangs- und Kostenprüfung |
| validate_building(..., session) | Spielvorschau und beide Spielvalidatoren, kein Bauauftrag |
| place_building(..., session, actionId) | Frische Spielvalidierung und ein regulärer Auftrag; actionId wird Entity-ID |

Vorlagen sind Daten aus dem Katalog. Keine fest verdrahtete Liste von Pumpe/Farm/Lager.
Katalogeinträge melden available, unlocked, supported, unsupportedReasons, Größe,
Eingang, Kosten, Toolgruppe, Layout und PlaceFinished. supported ist eine Aussage
über den implementierten Platzierungsweg, kein Live-Nachweis oder Produktionsversprechen.
Die alten Werkzeuge inspect_build_catalog, precheck_build_site, validate_build_site,
place_path und place_lodge bleiben als unveränderte Pilotverträge erhalten.

## Grenzen und Auftragsverfolgung

- Einzelplatzierung, ungespiegelt; rotation 0/1/2/3 = Cw0/90/180/270.
- x/y/z bezeichnet den BlockObject-Ursprung, nicht einen angenommenen UI-Mausanker.
- Ab 0.14.1 Layout Single, Rectangle, SideLine oder TwoSegmentLine,
  Square-Werkzeugform, höchstens 64 Blockzellen. Immer genau ein festes
  Blueprint-Objekt, kein Ziehbereich oder automatisch erzeugter Linienzug.
- Keine Entwicklerwerkzeuge, seitliche Geländebefestigung oder Sonderlayouts
  (weiterhin Line und Half). Solche Einträge bleiben mit Ablehnungsgrund sichtbar.
  Keine variable Spanne, Drehung per UI oder freie Größenwahl implementiert.
- Aktuelle Feature-Toggles und Freischaltung werden vor dem Auftrag geprüft.
  Kein Freischalten, keine Materialerzeugung, keine erzwungene Fertigstellung.
  Das Spiel entscheidet anhand PlaceFinished, welche Objekte sofort fertig sind.
- Keine Ersetzung vorhandener, schneidender Blockobjekte, auch nicht overridable.
  Stapelbare Unterstützung bleibt Aufgabe der Spielvalidatoren.
- Jede neue actionId nur einmal pro geladener Sitzung; auch Ablehnung oder Fehler
  verbrauchen die ID. Keine automatische Wiederholung und kein automatischer Rollback.
  Bei unklarer Antwort inspect_building(id=actionId, session) und find_buildings lesen.
- Maximal 256 generische Auftragsversuche und 256 generische Vorschauprüfungen
  einschließlich Platzierungen pro Sitzung; maximal 64 versteckte Vorschauvorlagen.
  IDs werden nicht verdrängt. Neustart der MCP-Verbindung setzt diese Grenzen nicht zurück.
- applied bestätigt Entity-ID, Vorlage, Ursprung und Rotation. finished bleibt
  getrennt. Baustellenfortschritt, Distrikt, Materialzufuhr und Wirkung separat lesen.
- Grundstücksprüfung garantiert weder Wasserzugang/Ertrag noch Wegeanbindung,
  Versorgungssicherheit oder spätere Konfiguration eines Lagerhauses.

## Freigaben und öffentliche APIs

Mod: `enableBuildingPlacement: true` in privater bridge.local.json.
MCP: `TIMBERBORN_ENABLE_BUILDING_PLACEMENT=1`. Beide schalten nur den neuen
Validator/Platzierer frei; Standard bleibt aus. Zwei neue Leser sind standardmäßig
sichtbar (insgesamt 16). Legacy-Freigaben schalten den generischen Bau nicht frei.

Die Implementierung erweitert den vorhandenen, live erprobten Vorschau-/Platzierungsweg:
TemplateService.GetAll<TemplateSpec> aus TemplateCollectionService, BuildingSpec,
PlaceableBlockObjectSpec, BlockObjectSpec, BuildingUnlockingService.Unlocked,
PreviewFactory, BlockObject.IsValid, BlockObjectValidationService.IsValid und
BlockObjectPlacerService.GetMatchingPlacer(...).Place(...). Öffentliche Signaturen
der unveränderten lokalen Timberborn-1.1.2.4-Bibliotheken geprüft. Keine private
Reflection, kein Patch und kein Kopieren fremder Implementierungen.

Referenzbasis: [offizielle Quellen und bestehende good references](references/README.md),
[Vorschauvalidierung](native-validation.md) und [Lodge-Pilot](lodge-placement.md).

## Begrenzte Live-Abnahme

Der erste Pilot zeigte: Path verwendet TwoSegmentLine, kleine Lager/Tanks und
Holzfällerflaggen SideLine. Der frühere Filter verwechselte diese Werkzeuglayouts
mit einer grundsätzlichen Unmöglichkeit, das einzelne feste Blueprint zu platzieren.
0.14.1 lässt diese beiden Layouts zu; der Platzierer verwendet weiterhin exakt eine
Geometrie mit unveränderten Feature-, Freischaltungs-, Hindernis- und Spielprüfungen.
Path war über den alten Einzelweg-Piloten bereits live platziert worden. Die neuen
Layoutklassen müssen zusätzlich über den generischen Weg praktisch abgenommen werden.

Nach Modwechsel zuerst Version, Sitzung, vollständigen Katalog und Koloniezustand lesen.
Pumpe, Farmhaus, Lager und Wohnraum als erste vier Gebäudefamilien untersuchen.
Pro Typ einen geeigneten Standort vorprüfen und regulär beauftragen; nicht blind
alle Templates platzieren. Mindestens zwei unterschiedliche neue Aufträge in derselben
Sitzung nachlesen. Blockierten Standort kontrolliert ablehnen lassen. Anschließend
Baufortschritt und Nutzen beobachten. Bei unconfirmed/Fehler nur lesend klären;
keine automatische Wiederholung. An Sonderlayout oder fehlender Betriebskonfiguration
gezielt die nächste Lücke festhalten, nicht „vollständig spielbar“ behaupten.

0.15.0: Beide zusätzlichen Layoutklassen live bestätigt. Zwei Path-Einzelobjekte
regulär platziert/fertig nachgelesen; SmallWarehouse regulär beauftragt und mit drei
Holz fertiggestellt. Bestehende Farm ebenfalls fertig, neue Pumpe nun mit Baudistrikt.
