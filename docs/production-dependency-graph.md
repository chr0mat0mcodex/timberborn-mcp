# Zukunftsfeature: vollständiger Produktions- und Abhängigkeitsgraph

Status: geplant, nicht implementiert. Nutzerauftrag vom 2026-09-20: den vollständigen
Abhängigkeitsbestand einmal extrahieren und mit einem einzelnen MCP-Aufruf als
Gesamtübersicht abrufen können. Der Agent soll alle Produkte, ihre Vorprodukte,
Herstellungsorte und notwendigen Schritte für eine Produktionslinie erkennen.
Keine Festlegung auf eine Release-Nummer der eigenen Mod.

## Gewünschter Umfang

- Güter und Rohstoffe mit stabilen IDs und lesbaren Namen, auch ohne vorhandenen Vorrat.
- Rezepte mit Eingangs-/Ausgangsmengen, Brennstoff, Zyklusdauer und Nebenprodukten.
- Gebäude, die ein Rezept herstellen können; benötigte Baukosten und Freischaltungen
  als eigene Voraussetzungen, getrennt von laufend verbrauchten Zutaten.
- Quellen ohne normales Herstellungsrezept: Sammeln, Holzfällen, Anbau/Ernte,
  Wassergewinnung sowie weitere tatsächlich verfügbare Gewinnungswege.
- Abgeleitete Abhängigkeiten und mögliche Herstellungsreihenfolgen vom Rohstoff bis
  zum gewünschten Endprodukt. Alternativen und Zyklen explizit erhalten; keine
  erfundene einzige Reihenfolge. Auch Forschungspunkte als eigene Ausgabeart beachten.
- Weitere Betriebsbedingungen, soweit belegt: Personal/Arbeitertyp, Energie, Wasser,
  Fläche, Wachstum/Reife, Fraktion und Spiel-Features. Fehlende Daten als unbekannt
  kennzeichnen, nicht durch Standardwerte oder bloße Gebäudenamen ersetzen.

Klargestellt durch den Nutzer: Gemeint sind **Rohstoffe, Gebäude und Rezepte**.
Arbeiterrollen und historische Erstverfügbarkeit in Spielreleases sind kein eigener
Featureauftrag. Spielversion/Fraktion dienen nur als Herkunft und Geltungsbereich der
extrahierten Definitionen; notwendige Betriebsbedingungen ergänzen die Voraussetzungen.

## Vorgeschlagener MCP-Vertrag

Arbeitsname: `inspect_production_graph()` — rein lesend, keine Spieländerung.
Ein Aufruf liefert den vollständigen kompakten Graphen des ausgewiesenen Datenumfangs,
mit `schemaVersion`, Spiel-/Modversion, Fraktion/Features, `graphRevision`, Herkunft,
Knoten, Kanten und explizitem Vollständigkeitsstatus samt Lücken. Dies ist zunächst ein
Vertragsentwurf, kein bereits vorhandenes Werkzeug.

Knotentypen etwa Gut, Rezept, Gebäude, natürliche Quelle und Freischaltung; Kanten
tragen Rollen wie verbraucht, produziert, hergestellt_in, Baukosten oder benötigt.
Damit kann der Agent zusammenhängende Produktionslinien selbst auswerten. Große
lokalisierte Beschreibungen nicht an jedem Knoten wiederholen. Ein „vollständig“-Flag
nur innerhalb des klar benannten Geltungsbereichs vergeben. Wenn nur die aktive
Fraktion zugänglich ist, keine Vollständigkeit für andere Fraktionen behaupten.

Statische Spieldefinitionen einmal je passender Datenrevision extrahieren und
wiederverwenden; nach Versions-, Fraktions- oder Definitionswechsel neu aufbauen.
Aktuelle Freischaltung und Betriebsfähigkeit getrennt behandeln, da sie sich während
der Sitzung ändern können. Eine Rezeptdefinition garantiert weder freies Personal
noch erreichbare Rohstoffe oder tatsächlich laufende Produktion.

Ziel ist ausdrücklich eine Gesamtantwort mit einem MCP-Aufruf, kein Zwang zum
Durchblättern für den Agenten. Zuerst Größe/Latenz des vollständigen kompakten Graphen
im Pilot gegen das bestehende 128-KiB-Transportlimit prüfen. Keine stille Kürzung und
kein unbegrenztes Hochsetzen des Limits. Falls nötig intern begrenzt übertragen und
serverseitig zur Gesamtantwort zusammensetzen; bei zu großer MCP-Antwort den
Zielkonflikt vor Umsetzung klären. Teilgraphabfragen wären eine spätere Ergänzung.

## Kurzprüfung öffentlicher APIs

Lokale öffentliche Metadaten von Timberborn 1.1.2.4 am 2026-09-20 geprüft:

- RecipeSpecService.GetRecipes()/GetRecipe(id): registrierte Rezeptdefinitionen.
- RecipeSpec: Id, Ingredients, Products, CycleDurationInHours, Fuel,
  CyclesFuelLasts, FuelCapacity und ProducedSciencePoints.
- ManufactorySpec.ProductionRecipeIds: Zuordnung Gebäudevorlage zu Rezepten.
- WorkplaceSpec: MaxWorkers, DefaultWorkers, DefaultWorkerType,
  DisallowOtherWorkerTypes und WorkerTypeUnlockCosts.
- Bestehende eigene Güter-, Baukatalog- und Forschungsleser liefern bereits weitere
  Anschlussstellen. Ihre Verknüpfung und Vollständigkeit sind noch zu prüfen.

Das belegt die technische Grundlage für Rezeptketten, noch keinen vollständigen
extrahierten Graphen. Natürliche Quellen, Ernte, Energie/Wasser, Sonderproduktion,
Fraktionsabdeckung und alternative Rezepte benötigen eine eigene Abdeckungsprüfung.
Kein Übernehmen fremder Modquellen oder Spielbibliotheken ins Repository; abgeleitete
Daten zunächst lokal erzeugen, keine neue Abhängigkeit ohne gesonderte Abwägung.

## Spätere Abnahme

Zuerst kleiner repräsentativer Pilot: einfache Verarbeitungskette, mehrstufige Kette,
Rezept mit Brennstoff/Nebenprodukt und mindestens eine Ernte-/Sammelquelle. Erfolg:
Mengen, Gebäude und Voraussetzungen stimmen mit öffentlichen Definitionen überein;
fehlende Quellen werden erkennbar als Lücke behandelt. Danach alle registrierten
Güter/Rezept-IDs auf Abdeckung, verwaiste Kanten, Alternativen und Zyklen prüfen.
Einzelaufruf, Antwortgröße, reproduzierbare Revision und Aktualisierung bei
Definitionswechsel testen. Kein separater Implementierungsstart durch diesen Plan.
