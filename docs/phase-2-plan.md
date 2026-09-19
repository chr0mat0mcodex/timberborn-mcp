# Phase 2: Grundversorgung aufbauen und betreiben

**Aktuelle Priorität (Nutzerklärung 2026-09-20):** analysieren, welche Beobachtungen,
Entscheidungsgrundlagen und kontrollierten Eingriffe das Spielen ermöglichen. Der praktische
Kolonieaufbau ist derzeit sekundär und dient als Referenzszenario, nicht als nächster Automatismus.
Die folgenden Abnahmeziele beschreiben das spätere Spielziel; aktuelle Arbeit bewertet
Fähigkeiten, Belegstufen, Lücken und gezielte nächste Nachweise.

Stand: 2026-09-19. Status: Paket 2A freigegeben und als begrenzter Pilot abgeschlossen;
[Ergebnisse und Architekturblocker](phase-2a-results.md).
Aktualisierung 2026-09-20: eigene native Mod nach Nutzer-Go umgesetzt und Basiszugriff live geprüft.
Autonomes Weiterarbeiten freigegeben, bis echte Nutzerhilfe nötig ist. Ausschließlich strukturierte
Spielabfragen und programmierte Interaktionen, keine Screenshots. 0.3.0 ergänzt den
[räumlichen Vorprüfungspiloten](spatial-precheck.md); vollständige Bauvalidierung bleibt offen.
Der Nutzer hat Wasser, Nahrung, Holz und Wege als erstes Spielziel gewählt und Wohnraum ergänzt.
Phase 1 hat Beobachtung und einen einzelnen Spieleingriff nachgewiesen. Phase 2 soll einen
kleinen, überprüfbaren Spielablauf ermöglichen: beobachten, Bedarf erkennen, bauen, betreiben, Wirkung prüfen.

## 1. Ziel und Grenzen

Abnahmeziel: In einer vorbereiteten Testkolonie innerhalb eines festgelegten Startgebiets eine
funktionsfähige Grundversorgung mit Wasser, Nahrung, Holz, Wegen und Wohnraum herstellen.
Zunächst ein Distrikt und die vorhandene Fraktion; keine allgemeine Unterstützung aller Fraktionen behaupten.
Ausgangspunkt, vorhandene Vorräte, verfügbarer Bauplatz und Abnahmezeitraum werden vor dem Live-Lauf erhoben.

Normale Spielregeln gelten: Materialkosten, Bauzeit, Arbeiter, Forschung, Reichweiten und Zugang.
Keine Ressourcen erzeugen, Gebäude sofort fertigstellen oder Forschung kostenlos freischalten.
Gebäude platzieren bedeutet einen regulären Bauauftrag, nicht direkt ein fertiges Objekt erzeugen.

Nicht im ersten Umfang: Bots, Energieindustrie, Distriktmigration, komplexe Automationsgraphen,
Terraforming, Abriss fertiger Gebäude, Wasserbau-Großprojekte sowie automatisches Save/Load.
Vorhandene Gefahren müssen trotzdem erkannt werden. Ein ungeeigneter Startzustand führt zu einer
klaren Grenze oder einem neuen Plan, nicht zu ungeprüfter Erweiterung der Eingriffe.
Die zuletzt absichtlich pausierte Holzfällerflagge wird in dieser Planungsphase nicht verändert.

## 2. Was der Agent verstehen muss

Ein Bestandswert reicht nicht: Der Agent muss erkennen können, wo ein Engpass entsteht und
ob ein Bauauftrag ihn tatsächlich beheben kann. Rohdaten, daraus berechnete Werte und Schätzungen bleiben getrennt.

| Bereich | Erforderliche Beobachtung | Entscheidung, die dadurch möglich wird |
|---|---|---|
| Sitzung und Zeit | geladene Kolonie, Fraktion, Spielversion, Tageszeit, Wetter, sichtbare Warnungen, Geschwindigkeit/Spielpause, verfügbare Funktionen | Ist die Beobachtung noch gültig, wie dringend ist eine Aktion? |
| Vorräte | je Gut Menge, nutzbare Lagerkapazität und Standort/Distrikt; verfügbare versus reservierte/in Transport befindliche Mengen, soweit belegbar | Reichen Wasser, essbare Nahrung und Holz; fehlen Lager oder Baumaterial? |
| Entwicklung | mindestens zwei datierte Beobachtungen, Zu-/Abnahme und Messintervall; Produktions-/Verbrauchsdaten, falls verfügbar | Verbessert sich die Versorgung oder wird nur ein Restbestand aufgebraucht? |
| Bevölkerung | Erwachsene/Kinder, einsatzfähige Arbeitskräfte, Arbeitslose, besetzte/freie Stellen, Hunger/Durst und weitere kritische Zustände | Gibt es Personal für Bau, Versorgung und Transport? |
| Wohnraum | fertige nutzbare Betten, belegte/freie Betten, Obdachlose; Wohnbaustellen separat | Wie viele zusätzliche Wohnplätze fehlen tatsächlich? |
| Gebäude | ID, Typ, Position, Orientierung, Zugang, Baustatus, Pause, Arbeiter, Produktionszustand und Blockierungsgründe | Warum produziert eine vorhandene Einrichtung nicht? |
| Karte | begrenzter Kartenausschnitt: Koordinaten/Höhen, belegte Flächen, Wege, Wasser, relevante Vegetation/Ressourcen und bebaubare Flächen | Wo passen Brunnen/Pumpe, Sammelstelle, Holzfäller, Lager und Wohngebäude hin? |
| Erreichbarkeit | Gebäudezugänge, durchgängige Wegverbindung, Distrikt-/Arbeitsreichweite und Hindernisse | Ist eine geometrisch mögliche Platzierung auch praktisch nutzbar? |
| Bauwissen | erlaubte Gebäudetypen der Fraktion, Kosten, Grundfläche/Höhe, Eingang, Rotation, Freischaltung, Betriebsanforderungen | Welcher konkrete Gebäudetyp löst den Bedarf mit verfügbaren Mitteln? |
| Aufträge und Probleme | Baustellen, Materialbedarf, Baufortschritt, Priorität und bestätigte Fehlermeldungen | Muss der Agent warten, Personal umlenken, Material beschaffen oder einen Plan korrigieren? |

Wichtige Regeln für die Auswertung:

- Nahrung nur über tatsächlich essbare Güter aggregieren; Rohzutaten nicht als sofort verfügbare Mahlzeiten zählen.
- Wasser im Tank und Wasser auf der Karte sind unterschiedliche Größen.
- Ein Gebäude im Bau liefert noch keine Betten oder Produktion. Pausiert, unbesetzt und unerreichbar sind unterschiedliche Ursachen.
- Global vorhandene Güter sind nicht automatisch am benötigten Ort verfügbar. Fehlende Reservierungs-/Distriktdaten offen kennzeichnen.
- Aus Bestandsänderungen entsteht zunächst nur ein Nettotrend, keine sichere Produktions- oder Verbrauchsrate.
  Baustellenkosten und Transport können denselben Trend erklären. Reichweiten in Tagen nur mit belegter Basis und Unsicherheit ausgeben.
- Wohnraumbedarf richtet sich zunächst nach der tatsächlich vorhandenen Bevölkerung; erwartetes Wachstum nur als ausgewiesene Prognose.
- Keine versteckten Wetterinformationen nutzen. Fehlende Daten sind unbekannt, nicht null Bestand oder ungehindert bebaubar.

## 3. Welche Eingriffe der Agent braucht

| Funktion | Phase-2-Rolle | Grenze und Ergebnisprüfung |
|---|---|---|
| Gebäude pausieren/aktivieren | vorhanden; Betriebe gezielt betreiben und Arbeitskräfte freisetzen | bekannter Ausgangszustand, beobachteter Zielzustand |
| Spiel pausieren/fortsetzen, begrenzte Geschwindigkeit | Zeit zum Planen und kontrolliertes Beobachtungsfenster | nur bestätigte Geschwindigkeiten; Gebäudepause nicht mit Spielpause verwechseln |
| Bauvorhaben prüfen | zwingend vor jedem neuen Bau | Kosten, Platz, Kollision, Rotation, Zugang und Freischaltung prüfen; keine Mutation |
| Gebäude-Bauauftrag erteilen | Wassergewinnung, Sammlung/Nahrung, Holzgewinnung, Lager und Wohnraum | reguläre Baustelle; ID/Auftrag zurückgeben und Existenz nachlesen |
| Wege bauen | Gebäude und Baustellen anbinden | kleine geprüfte Segmente; bestehende Wege erhalten; Höhenübergänge ggf. separat behandeln |
| Baumfällgebiet markieren/entfernen | Holzgewinnung tatsächlich ermöglichen | nur begrenzte bestätigte Fläche; Baumfällmarkierung ist kein sofortiges Löschen von Bäumen |
| Arbeitsplätze/Betriebspriorität einstellen | Versorgung vor unwichtigen Aufgaben besetzen | bevorzugt reguläre Sollbesetzung/Priorität, keine direkte Biber-Teleportation oder erzwungene Einzelzuweisung |
| Lagergut und Lagermodus einstellen | Lager nutzbar machen | nur passende Güter/Modi; Kapazität und Auswirkungen auf Erreichbarkeit prüfen |
| Baupriorität setzen und eigene offene Bauaufträge abbrechen | Material und Baupersonal auf Engpass konzentrieren; Fehlplanung beheben | nur bekannte eigene Baustellen; Rückerstattung nicht voraussetzen; kein pauschaler Abriss |
| Pflanz-/Anbauflächen und erforderliche Rezeptwahl | notwendig, sobald die gewählte Nahrung-/Holzstrategie diese benötigt | getrenntes Zusatzpaket, wenn reine Sammlung nur Übergangslösung ist |

Für den ersten Aufbau wird der kleinste geeignete Satz fraktionsspezifischer Gebäude gewählt.
Konkrete Gebäudenamen, Rezepte, Kosten und Geschwindigkeitswerte werden aus dem geprüften Katalog übernommen.
Sammlung vorhandener Nahrung und Bäume darf als erster Pilot dienen, ist aber kein Nachweis nachhaltiger Versorgung.
Für dauerhaften Betrieb braucht es Ernte-/Nachpflanzung bzw. einen begründeten nachwachsenden Versorgungsweg.

## 4. Vorschlag für die Werkzeuge

Dies sind geplante Verträge, keine bereits verfügbaren MCP-Werkzeuge. Bestehende Lesewerkzeuge werden erweitert,
statt dieselben Daten in mehreren widersprüchlichen Antworten vorzuhalten.

| Werkzeug oder Werkzeuggruppe | Zweck |
|---|---|
| inspect_colony erweitern | kompakte Lage: Versorgung, Wohnraum, Arbeitskräfte, kritische Probleme, Zeit und Datenqualität |
| inspect_resources | Güterbestände und Kapazitäten mit Geltungsbereich; optional belegte zeitliche Entwicklung |
| inspect_workforce / inspect_housing | Ursachen und Details hinter Personal- bzw. Bettendefizit |
| inspect_map_region | begrenzter räumlicher Ausschnitt mit Höhen, Objekten, Wasser, Wegen und Zugangsdaten |
| get_build_catalog | gefilterte erlaubte Bauoptionen mit Kosten, Geometrie und Betriebsanforderungen |
| find_buildings / inspect_building erweitern | Position, Zugang, Baustatus und Betriebsdiagnose |
| inspect_construction | eigene und bestehende Baustellen, Fortschritt und fehlende Voraussetzungen |
| validate_construction | konkreten Gebäude- oder Wegeplan ohne Änderung prüfen und Probleme erklären |
| place_building / build_path | begrenzten validierten Bauauftrag erteilen; Auftrag/Entity anschließend eindeutig identifizieren |
| set_tree_cutting_area | klar begrenztes Fällgebiet setzen oder entfernen |
| set_workplace_staffing / set_workplace_priority | explizite Sollbesetzung bzw. Priorität ändern, soweit API belegt |
| set_storage_policy / set_construction_priority / cancel_construction | typisierte einzelne Betriebs-/Baustellenentscheidungen |
| set_game_speed | expliziten unterstützten Spielzustand setzen |
| observe_progress | begrenzt beobachten und dann neu entscheiden; kein endloses Polling |

Kein universelles „set_property“, beliebiger HTTP-Aufruf oder Konsolenbefehl als Ersatz für fehlende Spielaktionen.
Die strategische Entscheidung bleibt beim Agenten. Der Server validiert die konkreten Parameter und Spielvoraussetzungen.

## 5. Belegter API-Stand und Lücken

Recherchebasis: Herstellerrepository datvm/TimberbornMods, Commit
`1c057bda82ec955d1712937da916cddf0f382f24`, geprüft am 2026-09-19. „Belegt“ bedeutet Quellcode/Dokumentation,
sofern nicht ausdrücklich als live getestet bezeichnet. Keine neuen Spielendpunkte wurden während dieser Planung aufgerufen.

| Fähigkeit | Stand | Konsequenz |
|---|---|---|
| Zeit, sichtbares Wetter, Bevölkerungs-/Gebäudelisten, Gebäudepause | bestehender Adapter und Live-Nachweise | wiederverwenden |
| Geschwindigkeit setzen | LiveDataHandler enthält set-game-speed; nicht live geprüft | kleiner Kandidat; zulässige Werte und Sperrverhalten zuerst belegen |
| Detailbedürfnisse und getragene Güter eines Bibers | CharacterHandler liefert Details; nicht im aktuellen MCP-Vertrag | gezielte Erweiterung; keine Vollabfrage jedes Bibers pro Entscheidung |
| Baukatalog | BlueprintHandler liest Specs/Blueprints | auf geeignete Bauvorlagen begrenzen; Blueprint-Lesen ist kein Platzieren |
| Globale Güter, Kapazitäten, Betten, Arbeitskräfte und Kartengröße | ModdableTimberborn GameStats dokumentiert entsprechende Stat-IDs | nutzbarer Mod-interner Ansatz; noch kein bestätigter HTTP-Zugriff unseres Servers |
| Gebäudeposition, Terrain, Wasserflächen und Wegegraph | bestehendes Entity-Modell enthält nur ID/Zustand; Gebäudemodell kein vollständiges Raumabbild | räumliche Datenquelle noch belegen |
| Distriktabfrage | geprüfter DistrictHandler nicht implementiert | Route nicht als funktionierende Quelle einplanen |
| Bauprüfung/-platzierung, Wege, Fällflächen, Personal-/Lagersteuerung | in geprüften More-HTTP-API-Handlern nicht belegt | gezielte Schnittstellenrecherche bzw. schmale Erweiterung erforderlich |
| Automationsgraph | separater AutomationHandler liest Graphzustand | kein belegter allgemeiner Bau-/Aktionszugriff; für Grundversorgung kein Pflichtmod |

Quellen:

- [LiveDataHandler](https://github.com/datvm/TimberbornMods/blob/1c057bda82ec955d1712937da916cddf0f382f24/MoreHttpApi/Handlers/LiveDataHandler.cs)
- [CharacterHandler](https://github.com/datvm/TimberbornMods/blob/1c057bda82ec955d1712937da916cddf0f382f24/MoreHttpApi/Handlers/CharacterHandler.cs)
- [BlueprintHandler](https://github.com/datvm/TimberbornMods/blob/1c057bda82ec955d1712937da916cddf0f382f24/MoreHttpApi/Handlers/BlueprintHandler.cs)
- [EntityModels](https://github.com/datvm/TimberbornMods/blob/1c057bda82ec955d1712937da916cddf0f382f24/MoreHttpApi.Shared/EntityModels.cs), [BuildingModels](https://github.com/datvm/TimberbornMods/blob/1c057bda82ec955d1712937da916cddf0f382f24/MoreHttpApi.Shared/BuildingModels.cs)
- [DistrictHandler](https://github.com/datvm/TimberbornMods/blob/1c057bda82ec955d1712937da916cddf0f382f24/MoreHttpApi/Handlers/DistrictHandler.cs)
- [GameStats](https://github.com/datvm/TimberbornMods/blob/1c057bda82ec955d1712937da916cddf0f382f24/ModdableTimberborn/Docs/GameStats.MD)
- [ModdableTimberborn-Einstieg](https://github.com/datvm/TimberbornMods/blob/1c057bda82ec955d1712937da916cddf0f382f24/ModdableTimberborn/Docs/README.MD)
- [AutomationHandler](https://github.com/datvm/TimberbornMods/blob/1c057bda82ec955d1712937da916cddf0f382f24/MoreHttpApiAutomations/Handlers/AutomationHandler.cs)

Die Vanilla-HTTP-API ist keine bestätigte Lösung für diese Lücken. Der versuchte Abruf ihrer offiziellen
Dokumentationsseite war nicht erfolgreich; deshalb keine erfundenen Endpunkte oder Fähigkeiten einplanen.

## 6. Architekturentscheidung vorbereiten

Aktueller Beschluss nach Nutzer-Go: [eigene Spielschnittstelle](architecture/native-game-api.md)
als Zielarchitektur auf direkt geprüften öffentlichen Spielservices. Fremdmods zunächst Referenz und
vorhandener Vergleichsadapter, keine Pflichtbasis. Die folgenden Optionen beschreiben den früheren Stand.

Mit dem Skill „Göttliche Inspiration“ kritisch geprüft: Mehr einzelne Schalter ergeben noch keinen spielenden Agenten.
Der Engpass liegt bei verlässlicher Versorgungslage, Raumverständnis und regulären Bauaufträgen.

Empfehlung: MCP-Verträge unabhängig von Fremdmod-Typen halten, vorhandenen Adapter weiterverwenden und
zuerst je einen kleinen Nachweis für Bestände/Wohnraum, räumliche Daten und reguläre Bauprüfung erbringen.
Falls dokumentierte vorhandene Schnittstellen fehlen, eine schmale zusätzliche Spielmod als mögliche Brücke
planen, bevorzugt über dokumentierte ModdableTimberborn-/Spiel-Schnittstellen. Keine bestehenden Mods patchen.
Interne/reflektierte APIs erst nach belegtem Fehlen dokumentierter Wege bewerten und isolieren.
Ein solcher Brückenmod ist mit diesem Dokument noch nicht zur Implementierung oder Installation freigegeben.

Konservative Alternative bei fehlendem Bauzugriff: Agent analysiert und gibt Bauvorschläge, Nutzer baut.
Das wäre ein ausdrücklich reduzierter Zwischenstand, keine bestandene Abnahme „Agent baut Grundversorgung“.
Bildschirmsteuerung ist ebenfalls kein stiller Ersatz für eine fehlende MCP-Fähigkeit.

## 7. Verlässlichkeit des Spielablaufs

- Jede Beobachtung erhält Quelle, Geltungsbereich und Spiel-/Abrufzeit. Kein atomarer Snapshot behauptet,
  wenn Daten aus mehreren Requests stammen. Sitzung/Spielstandwechsel müssen Pläne invalidieren;
  solange dafür kein verlässlicher Marker existiert, keine längerlebigen Aktionspläne automatisch fortsetzen.
- Karten zunächst in kleinen Ausschnitten, z. B. 16 x 16 Bodenfeldern mit begrenzten Höhenschichten.
  Koordinatensystem, Einheiten, Eingang und Rotation vor dem ersten Bau am Spiel abgleichen.
- Neue Platzierungen unmittelbar vor Ausführung erneut prüfen. Eine frühere Prüfung reserviert keine Fläche oder Güter.
- Bauanfrage angenommen, Baustelle angelegt, Gebäude fertig und Gebäude produktiv sind vier verschiedene Ergebnisse.
- Neue Bauaufträge benötigen eine Strategie gegen Duplikate. Bevorzugt backendseitige Request-ID plus Ergebnisabfrage;
  ohne diese Unterstützung bei verlorener Antwort nur Bestand/Position klären, niemals blind erneut platzieren.
- Kleines freigegebenes Baugebiet, Gebäudetypen, Materialbudget und Aktionslimit pro Lauf festlegen.
  Kleine zusammenhängende Vorhaben können gemeinsam freigegeben werden; keine Nachfrage pro Wegfeld nötig.
- Bei unklarer Mutation, Koloniewechsel, fehlenden kritischen Daten oder überschrittenem Budget stoppen.
  Unterbrechung/Fehler darf keine neue Baustelle oder unerwartete Wiederaufnahme der Simulation auslösen.
- Begrenzte Beobachtungsfenster mit Abbruchmöglichkeit. Lokale Laufdaten nicht nach Git exportieren;
  für das Projekt reichen synthetische Tests und zusammengefasste Abnahmeergebnisse.

## 8. Umsetzung in überschaubaren Paketen

| Paket | Inhalt | Abnahme vor dem nächsten Paket |
|---|---|---|
| 2A: Daten- und Schnittstellenpilot | je ein Nachweis für Wasser-/Holz-/Nahrungsbestand, Betten/Obdachlose, Arbeitskräfte, Kartenregion, Baukatalog und Bauprüfung | Quelle, Schema, Versionsgrenze und fehlende Fähigkeiten belegt; noch keine Bauaktion |
| 2B: Lagebild | Ressourcen-, Wohnraum-, Personal- und Gebäudediagnose, kompakte Regionsdarstellung | stichprobenartiger UI-Abgleich; unbekannte Werte korrekt; bestehende 74 Tests weiterhin erfolgreich |
| 2C: Ein regulärer Bauablauf | genau ein geeignetes Wohngebäude und nötige kurze Wegverbindung auf freigegebener Fläche planen und errichten | richtige Position/Kosten, erreichbare fertige Wohnung, nutzbare Betten; keine Duplikate bei Fehlersimulation |
| 2D: Versorgungsketten | Holzgewinnung inklusive Fällgebiet, Wassergewinnung/-lager, Nahrung und passendes Lager; Personal-/Bauprioritäten | nicht nur Gebäude vorhanden: Versorgung und Lagerung nachweisbar in Betrieb |
| 2E: Begrenzter Agentenlauf | beobachten, priorisieren, kleine Aktionen ausführen und Wirkung prüfen; Wohnraumbedarf mitführen | vorab vereinbarte Zeit-/Ressourcen-/Aktionsgrenzen eingehalten, Engpässe nachvollziehbar behandelt |

2A ist ein kleiner repräsentativer Pilot, keine wochenlange API-Vollinventur: höchstens diese sieben
Fähigkeitsgruppen prüfen. Beim ersten klaren Architekturblocker Zwischenbilanz ziehen.
Keine Ausweitung auf weitere Mods oder Reverse Engineering ohne konkrete Nutzen-/Aufwandsentscheidung.
2C setzt einen geeigneten Testzustand voraus; fehlt Baumaterial, wird nach Freigabe zuerst die nötige
Holzversorgung aus 2D aufgebaut, statt Material zu erzeugen.

## 9. Erfolgskriterien und nächster Auftrag

Vor dem Live-Lauf Ausgangsbevölkerung, erlaubtes Gebiet, Materialbudget, Vorratsziel und Beobachtungsdauer
festlegen. Vorschlag für die erste zeitlich begrenzte Betriebsabnahme: zwei vollständige Spieltage;
genug für einen Pilot, ausdrücklich kein Beleg für Dürre-/Badtidefestigkeit oder unbegrenzte Nachhaltigkeit.

Erfolg bedeutet:

1. Trinkwasser und essbare Nahrung sind verfügbar, zugänglich und werden nachweisbar ergänzt;
   vorhandene Vorräte allein gelten nicht als laufende Versorgung.
2. Holz wird tatsächlich gewonnen; der Agent berücksichtigt den Verbrauch durch seine Baustellen.
3. Alle erforderlichen Gebäude haben eine bestätigte funktionsfähige Verbindung, keine hängenden Pflichtbaustellen.
4. Für die aktuelle Bevölkerung sind genügend nutzbare Betten vorhanden und der belegte Obdachlosenzähler ist null.
5. Versorgungseinrichtungen sind besetzt und ihre Lager geeignet. Kritische Fehlzustände werden sichtbar erkannt.
6. Nutzer bestätigt die zentralen Werte im Spiel; Fehler-/Abbruchtests belegen keine Doppelbauten oder blinden Wiederholungen.

Nächster empfohlener Auftrag nach Freigabe: Paket 2A ausführen und daraus einen konkreten
Adapter-/Brückenmod-Vorschlag mit Aufwand, Abhängigkeiten und verbleibenden Lücken ableiten.
Dieser Plan autorisiert keine neue Implementierung, Installation, Konfigurationsänderung oder Spielaktion.
