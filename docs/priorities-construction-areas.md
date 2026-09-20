# Prioritäten, Baustellen und Flächen (0.12.0)

Stand: implementiert; Live-Abnahme nach Installation offen. Keine neue Fremdmod-Abhängigkeit.

## Werkzeuge

| Werkzeug | Zweck |
| --- | --- |
| inspect_building_priority | Priorität für Entity-ID, Session und kind workplace/construction lesen |
| set_building_priority | Priorität mit expectedPriority vergleichen und regulär setzen |
| inspect_construction | Alle offenen Baustellen seitenweise mit ID, Position, Baukosten, Bestand, Fortschritt, Distrikt und Baupriorität |
| inspect_area_types | Unterstützte Arten und verfügbare Pflanzvorlagen samt Werkzeug-Sperrstatus |
| inspect_areas | Markierte Zellen einer Art seitenweise lesen |
| set_area | Markierungen anlegen oder entfernen |

Prioritäten: VeryLow, Low, Normal, High, VeryHigh. Arbeitsplatzpriorität gilt für fertige
Arbeitsplätze, Baupriorität für offene Baustellen. Priorität garantiert keine sofortige
Arbeiterzuweisung oder Materiallieferung. Die bisherige Abnahme 2 → 3 → 2 betraf
Sollbesetzung, nicht Priorisierung.

Flächenarten: tree_cutting (Baumfällen), crops (Pflanzaufträge der Landwirtschaft),
tree_planting (Förster). tapping liefert supported=false und total=null: TappersShack
sammelt laut offizieller Blueprint Ressourcen der Gruppe Tappable, etwa PineResin.
Eine eigenständige Zapfmarkierung ist nicht belegt. Pflanzaufträge sind keine separaten
Erntezonen; entfernte Markierungen entfernen keine Pflanzen.

Leselisten: offset und limit, maximal 32 Einträge pro Seite; bis hasMore=false lesen.
Seiten sind frische Beobachtungen, kein eingefrorener Gesamtsnapshot. Flächen werden als
Zellen mit Position und resource gemeldet, nicht als erfundene benannte Rechtecke.

set_area benötigt kind, operation (mark/remove), resource, expectedResource, session,
x/y/z und width/height (je 1–4). Alle höchstens 16 Zellen müssen dem erwarteten bisherigen
Wert entsprechen. Leerfläche: unmarked; Baumfällmarkierung: marked; sonst Vorlagenname.
resource ist bei Baumfällen und Entfernen leer, sonst eine freigeschaltete Pflanzvorlage.
Karte, bisheriger Zustand, Flächenart, Werkzeug-Sperre und Pflanzvalidierung werden vor
Änderungen geprüft. Bei Fehlern können Teiländerungen bleiben: kein automatischer Retry
oder Rollback; Zustand separat nachlesen.

## API und Schutz

Öffentliche WorkplacePriority/BuilderPrioritizable.SetPriority; TreeCuttingArea
AddCoordinates/RemoveCoordinates; PlantingService SetPlantingCoordinates/
UnsetPlantingCoordinates; PlantingAreaValidator.CanPlant. Vorlagen aus TemplateService
und TemplateNameMapper. ToolUnlockingService.IsLocked prüft ein reguläres PlantingTool;
dieses wird nur konstruiert, nicht aktiviert. Kein Unlock, Dev-Spawning oder UI-Eingriff.
Dieser Konstruktor-/DI-Pfad ist kompiliert, sein Laufzeitverhalten noch live zu prüfen.

Schreibzugriff doppelt gesperrt: private Mod-Konfiguration enablePriorities/enableAreas
und MCP-Prozess TIMBERBORN_ENABLE_PRIORITIES=1/TIMBERBORN_ENABLE_AREAS=1. Defaults aus.
Feste POST-Routen, Sessionbindung, erwarteter Istwert und korrelierte Antworten;
keine freien HTTP-, Reflexions- oder Spielkonsolenwerkzeuge.

## Live-Abnahme

1. Nach Neustart Version/Sitzung, Flächenkatalog, Baustellen und Arbeitskräfte lesen.
2. Einen vorhandenen Arbeitsplatz priorisieren, separat nachlesen, ursprüngliche
   Priorität wiederherstellen und erneut lesen. Bei unsicherem Ergebnis stoppen.
3. Dasselbe mit genau einer offenen Baustelle.
4. Eine geeignete unmarkierte Zelle markieren, separat lesen, Markierung entfernen
   und erneut lesen. Bestehende Nutzerflächen erhalten; keine großen Flächenversuche.
5. Pflanzmarkierung samt regulärer Verfügbarkeit prüfen. Unterstützte Schnittstelle
   von tatsächlicher Pflanzung, Ernte und Erreichbarkeit getrennt bewerten.

## Fortschreibung 0.13.0

Die oben für 0.12.0 beschriebene tapping-Sperre ist ersetzt: auf Nutzerwunsch als
Kiefernschutz durch Entfernen regulärer Fällmarkierungen implementiert. Kein eigener
Zapfzonen-Datentyp im Spiel. [Aktueller Vertrag](removal-and-pine-protection.md).

## Live-Nachweis in 0.13.0

Alle 14 Leserouten geprüft. Arbeitsplatz Normal -> High -> Normal, Baupriorität
Low -> High -> Low; jede Änderung separat bestätigt, beide Originalwerte erhalten.
Zwei offene Baustellen. Flächenkatalog und Abfragen erfolgreich; Flächenänderungen
noch nicht ausgeführt. Priorität ist weiterhin kein Nachweis tatsächlicher Umverteilung.
