# Lager, Farmen und Gebäudepause (0.15.0)

Implementiert und automatisiert geprüft; Live-Abnahme der neuen Einstellungen nach
Neustart offen. Keine neue Fremdmod-Abhängigkeit. Öffentliche Spielmethoden, keine
UI-Steuerung oder Bestandsmanipulation.

## Werkzeuge

Alle neuen Aufrufe benötigen eine aktuelle `session` und eine Gebäude-`id`.
`inspect_building_settings` liest Pause, Fertigstellung und vorhandene Lager-/Farmoptionen.
Nicht vorhandene Komponenten und noch unfertige Gebäude werden unterschieden.

| Werkzeug | Änderung | Erwarteter bisheriger Wert |
|---|---|---|
| `set_building_paused` | `paused`: true/false | `expectedPaused` |
| `set_storage_good` | `good`: ID aus allowedGoods; leer hebt Auswahl auf | `expectedGood` |
| `set_storage_mode` | `mode`: accept/empty/obtain/supply | `expectedMode` |
| `set_farm_priority` | `priority`: planting/harvesting | `expectedPriority` |
| `set_farm_crop` | `resource`: freigeschaltete Vorlage aus allowedPlants | `expectedResource` |

Lagermodi entsprechen Annehmen, Leeren, Beschaffen und Liefern. Lager- und
Farmeinstellungen erfordern fertige Gebäude. Feste Lagergüter sind nicht umstellbar.
Alte Bestände bleiben erhalten; eine Auswahl- oder Modusänderung transportiert selbst
keine Güter. Pause verwendet PausableBuilding.Pause/Resume und prüft IsPausable.
Fortsetzen garantiert weder Besetzung noch laufende Produktion.

FarmHouse.PrioritizePlanting/UnprioritizePlanting steuert Pflanzen/Ernten zuerst.
PlantablePrioritizer.PrioritizePlantable wählt die bevorzugte Pflanze. Ein Rücksetzen
auf keine Präferenz ist mangels belegter öffentlicher Rücksetzsemantik noch nicht
angeboten (`canClearCropPriority=false`). Keine Jobs werden erzwungen.

## Anbauflächen

Bereits vorhandene Werkzeuge bleiben zuständig: `inspect_area_types` liefert
verfügbare Pflanzen, `inspect_areas(kind="crops", offset=0, limit=32)` die Markierungen.
`set_area` verwendet `kind="crops"`, `operation="mark"` oder `"remove"`, `resource`,
`expectedResource`, `session`, `x/y/z`, `width/height` (je 1–4).
Markieren benötigt eine Pflanzenvorlage; Entfernen verwendet resource="".
expectedResource ist der gelesene vorherige Zustand ("unmarked" oder Pflanzenvorlage).
Ein Wechsel der Markierung entfernt keine bereits gewachsene Vegetation.
Flächen sind globale Markierungen, keine feste Zuordnung zu einem bestimmten Farmhaus.
Erreichbarkeit, Bodenbedingungen, Arbeiter und Ernteertrag müssen separat geprüft werden.

## Freigaben und Prüfungen

Neue Schreibfreigaben: Mod `enableBuildingSettings=true`, MCP-Prozess
`TIMBERBORN_ENABLE_BUILDING_SETTINGS=1`. Standardmäßig aus. Flächen behalten ihre
separate Freigabe `enableAreas` / `TIMBERBORN_ENABLE_AREAS`.

Session und erwarteter Wert verhindern Änderungen auf veralteter Grundlage.
Gleicher Zielwert ist ein No-op. Jede Änderung erfolgt einmal, wird direkt nachgelesen
und als applied/unconfirmed quittiert; anschließend separat lesen. Bei Fehler oder
unconfirmed keine automatische Wiederholung oder Rücknahme.

350 reguläre Tests bestanden (337 Unit, 13 Integration), drei Live-Tests im Standardlauf
übersprungen. HTTP-/MCP-Integration nutzt ausdrücklich synthetische Daten; sie beweist
keine Spielwirkung. Mod gegen Timberborn 1.1.2.4 gebaut, null Warnungen/Fehler.
Live-Abnahme: geeignete fertige Gebäude lesen, Pause hin/zurück, Lagergut/Modes samt
Rücklesung, Farmpriorität und Pflanzenwahl, danach kleine Anbaufläche markieren/entfernen.
