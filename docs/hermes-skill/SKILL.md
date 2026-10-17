---
name: timberborn-mcp-server
description: Timberborn über die vorhandene Hermes-MCP-Verbindung steuern; Testmodus mit allen Aktionsgruppen, Baugrenzen und Fehlerdiagnose. Keine manuellen Serverstarts für Spielaufgaben.
---

# Timberborn in Hermes

## 0.66 — Uferzugang und Startfehler, Kernpilot begrenzt bestanden

Terraincheck nutzt native Zellbelegung; leere Vorlagenzellen sind kein
Terrainhindernis. Echte Kollisionen und die gemeinsame Bauvorschau weiter prüfen.
Bei `run_simulation_for`/`run_simulation_until` sind explizite `run_start_*`-Codes
für Tempoabweichung, Pending, Spielsperre, Bauchargenbesitz, aktiven Lauf,
Journalgrenze und unbekannten Zielbestand vor dem Start abgewiesen:
`not_executed`, kein neuer Lauf. Frisch Tempo/`pendingSpeed`/`activeRunId` lesen,
dann neu entscheiden; kein automatisches Retry. Generische Konflikte und
Transportverluste bleiben `unconfirmed`; `run_not_found` ist keine Entwarnung.
Code und Pilotumfang: `docs/releases/0.66.0.md`. Uferpumpe bis Fertigzugang und zwei Startablehnungen live belegt; übrige Codes nur im Gate geprüft.

## Bauentscheidung trotz fehlender Materialien

Nicht auf volle Materialvorräte warten. Kosten des gesamten Plans einschließlich
Zugangshilfen je Gut summieren, verfügbare Bestände lesen und vorhandene Erzeuger
samt aktuellem Zustand prüfen. Je Gut Deckung = 100 × min(Bestand, Bedarf) / Bedarf;
gesamt = 100 × Summe der je Gut gedeckten Mengen / Summe des Bedarfs. Kostenfrei
bedeutet 100 Prozent; unbekannte Bestände nicht als null oder vollständig werten.
Rezeptdefinitionen sind keine vorhandenen Erzeuger; vorhanden/betriebsbereit/
produktiv unterscheiden, auch Holzgewinnung prüfen. Fehlende oder blockierte
Produktion melden, keine Mindestquote oder automatische Material-Bausperre.
Der Agent entscheidet und beauftragt nach positiver Entscheidung sofort regulär.
Lieferungen kommen während des Baus; begrenzte Zeitfenster und frische Diagnose
verwenden, nach Zeitlimit keine erneute Platzierung. Geometrie und Zugang bleiben
Pflichtprüfungen. Vollständiger Ablauf: `docs/agent-playing-guide.md`, Abschnitt
„Materialdeckung und Bauentscheidung“. Keine neue API oder Installation nötig.

## Blickauftrag: sofort Spielbild ansehen

Im Spielkontext bedeuten auch allein „schauen“, „SCHAUEN!!!!“, „schau mal“,
„guck mal“, „Screenshot jetzt“, „ich schau genau drauf“ und „da wo ich hinschaue“:
aktuelle Session ermitteln, `capture_screenshot` einmal aufrufen und den
Bildblock tatsächlich ansehen. Großschreibung und offensichtliche Tippfehler
ändern den Auftrag nicht. Keine Markierung oder zusätzliche Bestätigung vor
dem Bildabruf verlangen. Bild und Kamerametadaten zur Orientierung nutzen;
Baupositionen und Eingriffe weiterhin strukturiert prüfen. Erst bei verbleibender
Mehrdeutigkeit nachfragen. „Schau in den Code“ löst keinen Screenshot aus.
Kamera unverändert lassen, keine Bilddatei/-historie speichern. Bei
`screenshot_unavailable` Ursache klären statt automatisch erneut aufzunehmen.
Details: `docs/agent-playing-guide.md`, Abschnitt „Optionales Spielbild“.

## 0.65 — Wasserwarnungen und Versorgung, begrenzt live abgenommen

`water.flooded` meldet native Überflutung unabhängig von Pause und sichtbarem
Statustext. `intakeUnderwater`, `intakeContamination` und `intakeBlocked` prüfen;
null bleibt unbekannt/nicht anwendbar. Überflutung vor zusätzlichem Personal
beheben; bei Ausfall trockene Ersatzversorgung und zugängliche Vorräte prüfen.
Kontaminierter Ansaugpunkt ist kein Trinkbarkeits- oder Produktionsnachweis.
Zielscan: `target_badwater_observed` hat Versorgungsvorrang. `backwater` bleibt
auch kompakt sichtbar; `closureRecommended=false`. Wieder erreichbarer Weg unter
Sperrkrone ist Rückstaurisiko, Kartenrandkontakt kein sicherer Auslass. Auch ohne
Umweg sind Kapazität und Oberliegerüberflutung nicht bewiesen. Keine feste Sperre
als sichere Notlösung aus gültiger Geometrie ableiten. Details: `docs/releases/0.65.0.md`.
Dieser lokale Skillentwurf bleibt zurückgestellt und wird nicht installiert.

## Bauplaner-Korrektur — begrenzt live geprüft

`project_preview_failed` bedeutet Vorschaufehler und unbekannte
Wiederherstellung; `project_plan_faulted` gesperrter Planer. Spiel neu laden,
Session/Zustand prüfen, keine automatische Entsperrung oder Auftragswiederholung.
`project_not_found` bezeichnet eine fehlende native Projektkennung.
Chargen bewahren `failure.code`, `stage`, `execution`, `recoveryAction` auch kompakt.
Gestoppte Planung wird durch `advance` nicht wiederholt. `no_build_order` gilt
nur für diese Planungsphase, nicht für ältere Eingriffe oder bestätigte Bereinigung.
`pending_build`/`ledger_required` ausschließlich unter derselben Kennung abgleichen.
Menschliches Gate und 15-Teile-Pilot bestanden: Tank/Tank/Lager plus Umweg fertig
und zugänglich, Bestandsanschlüsse erhalten. Native Ausnahme/Chargenpersistenz
nur synthetisch geprüft. Ingame-Fortschrittszähler fehlerhaft; strukturierte
summary und frische Diagnose nutzen. Der lokale Skill bleibt zurückgestellt.

## Gesamtstruktur 0.63 — mit dokumentierter Grenze abgeschlossen

Korrektur 0.64 (begrenzt live abgenommen): `awaiting_access` ist
ein vorhandenes fertiges Teil ohne aktuellen Zugang, keine verlorene Identität.
Unterbau/Fertigzugang im selben begrenzten Projekt weiter beobachten; kein Replay.
Diagnose bewahrt positive Identitätsbelege nach Stop; belegter späterer Rückbau
kann Close ermöglichen. Nie bestätigte fehlende Objekte bleiben gesperrt.
`inspect_simulation` ergänzt `activeRunId` und `pendingSpeed`: bei verlorener
Antwort dieselbe Kennung und den aktuellen Tempozustand lesen. Historische
Pausebestätigung ist kein frischer Beleg; ausstehendes Tempo und verzögerte
Update-Wirkung beachten. Höhenbau 3/3, Stopp/Rückbau/Close mit Folgeauftrag ohne
Reload und frische Pause live bestätigt. Antwortverlust und Update-Rennen bleiben
synthetische Fälle. Fachvertrag/Belege: `docs/releases/0.64.0.md` im Projekt.
Dieser lokale Skillentwurf bleibt zurückgestellt, keine Installation/Freigabe.

Ebener Gesamtauftrag, Reparatur, Schema-2-Charge und manueller Höhen-Gesamtstart
mit Weg auf unfertiger Plattform live belegt. Separater Vertikalpilot bestätigt
fünf gemeinsame Aufträge; ebener Dreiteilepilot vollständig fertig. Objektverlust
ohne Replay und konkrete Vorauftragsablehnung live geprüft. Ablehnung mitten im
Durchgang mit ausdrücklicher Nutzerfreigabe nur synthetisch im Gate belegt, fertige Höhennavigation nicht neu geprüft:
nur explizite Ablehnung bedeutet `not_executed`; generischer Konflikt oder
Transportverlust bleibt `unconfirmed` und wird ausschließlich lesend geklärt.

Ein Plan, ein Go: `run_large_project` reicht alle Bauteile in einem Durchgang ein,
erst danach werden Probleme und Fertigstellung geprüft. `dependsOn` ordnet nur
die Einreichung; keine Fertigstellungsbarriere. Ebene/vertikale Piloten und neue
Gebäudechargen verwenden ebenfalls gemeinsame Platzierung. `ordered` ist kein
Fertigbau; `completed` verlangt alle Teile fertig. Native Fehler, Koordinaten,
IDs und unbestätigte Wirkung behalten; unbekannte Platzierungen nie wiederholen.
Autonomes Tempo und Zeitbudget über `run_large_project`, Status über
`inspect_large_project`, frische Blockaden mit `diagnose=true`. Einzelstarts ohne
Simulationsbudget starten nicht eigenmächtig die Zeit. Alte Schema-1-Chargen
bleiben kompatibel. Details: [Bauvertrag](../construction-structures-0.63.md).
Dieser lokale Skillentwurf wird durch die Pflege nicht installiert/freigegeben.

## Positive Terraforming-Optionen — begrenzt live geprüft

Der vollständige Zielscan liefert zusätzlich `analysis.terraforming`: bis zu drei
räumlich getrennte 3×3-Reservoirkandidaten mit zwei Ebenen Absenkung. Suchgebiet:
32 Felder um die Zielzone; feste Geometrie, keine erschöpfende Landschaftsplanung.
Kompakt bleiben Nutzen, Ursprung, Fläche, Aushubtiefe, Zielbodenhöhe, geometrisches
Zusatzvolumen, Zuflussbeleg, Blockaden und nächste Maßnahme sichtbar. Nur die
neun `excavationCells` je Kandidat stehen ausschließlich in `detail="full"`.

18 Rasterwürfel Zusatzvolumen sind weder 18 Wassergüter noch eine Reichweite in
Tagen. Ebene offene Oberfläche erforderlich; ein fester Block bleibt unter dem
neuen Boden. Höhlen im Aushub/Boden, seitlich offene Beckenwände, Zielzone samt
Rand und beobachtete Objektanker samt Rand schließen Standorte aus. Objektanker
ersetzen keine vollständige Belegungs-/Fundamentprüfung. Unvollständige Objekt-
abdeckung blockiert den Übergang zur Planung. Örtliches Faulwasser und fehlendes
Wasser über Randhöhe werden als Blockaden ausgewiesen. Sauberes örtliches Wasser
beweist keine dauerhaft saubere Quelle oder künftige Nachfüllung.

Vor Ausführung native Werkzeug-/Forschungs-, Belegungs-, Zugangs- und Bauphasen-
prüfung sowie kontrollierte Nachfüllung, Abfluss, Rückstau, Verdunstung und Pumpen-
nutzung klären. Überregionale Faulwasserpfade aus hazards weiterhin berücksichtigen.
Keine automatische Grabung; keine erfundenen Kosten oder Bewässerungsreichweiten.
`overviewOnly=true` bleibt ohne zielbezogene Terraforming-Optionen.

## Kompakte Scanantwort — live geprüft

`scan_strategic_map` liefert standardmäßig `detail="compact"`: Bedrohung,
betroffenes Ziel, beobachteter Pegel, minimale Zuflussschwelle, Kontamination,
Schutzpunkt/Kronenhöhe, konkrete Blockade und nächste Maßnahme. Die vollständigen
Gefahrenzähler und bis zu acht priorisierte Beispiele bleiben erhalten.
`hazards.commonMeasures` und `commonUnresolved` gelten für alle Prioritäten;
individuelle `measures`/`unresolved` ergänzen diese gemeinsamen Angaben.
Abdeckung, Beobachtungsalter und fachliche Grenzen bleiben sichtbar.

Kacheln, Korridorpfade, Querschnittszellen, Spezialobjekte und die allgemeine
Fluss-/Quelleninventur stehen in `detail="full"`. Mit identischer Zielzone und
derselben `scanId` nachlesen: kein erneuter Kartenscan; die bestehende frische
Sessionprüfung bleibt. `overviewOnly=true` liefert kompakt nur den Flussbericht-
Überblick mit Zählern/Wetter/Seitengrenzen, keine zielbezogene Gefahrenbewertung.
Für Inventurseiten full plus entsprechende Offsets verwenden. Fehlende Details
sind keine leeren Beobachtungen. Fehler bleiben vollständig erhalten.

## Gefahrenbewertung im Zielscan — begrenzt live abgenommen

`analysis.hazards` zuerst auswerten, danach die Korridorbeispiele. Die Bewertung
zählt alle erreichbaren nassen Hohlräume und diejenigen mit beobachtetem Pegel
mehr als 0,01 über Zielhöhe **und** minimaler Geländeschwelle. Kontaminiertes
Restwasser bleibt auch bei stillstehenden Quellen eine priorisierte Gefahr.
Die Zähler sind Hohlräume, keine unabhängigen Flutereignisse. Bis zu acht
repräsentative Gefahren erscheinen unter `priorities`; weitere bleiben im Zähler.
`target_water_observed` bedeutet bereits beobachtetes Wasser über der Zielhöhe.
`no_current_head_exceedance_not_safe` ist keine Entwarnung für künftige Ereignisse.

Pro Gefahr: Ursprung, Pegel, Höhenüberschuss, Kontamination, Zielzutritt sowie
Schutzquerschnitt und angenommene Krone. `closureAssessment` prüft Dachfreiheit,
Kartenhöhe, vollständige Ufer und geometrische Umwege bei hypothetischer Sperre.
`terrain_route_cut_at_observed_level` ist nur ein geometrischer Schnitt, keine
Empfehlung zur Sperre. Ab 0.65 zuerst `backwater` und einen
unabhängig geprüften Abfluss bewerten. Dieser Nachweis betrifft einen Ursprung
bei beobachtetem Pegel; weitere Zuflüsse, Bauwerke, Rückstau, Bauphasen, Material,
Forschung, Arbeitszugang und Dauer bleiben zu prüfen. Bei Dachkonflikt, offenem
Umweg oder unvollständigem Querschnitt einen anderen Sperrpunkt untersuchen.
Keinen Abfluss aufgraben, bevor dessen Auslass und Rückwirkungen belegt sind.
Ein niedriger Szenariopegel verkleinert den Schutzvorschlag für beobachtetes Wasser nicht.
`overviewOnly=true` liefert weiterhin keine zielbezogene Gefahrenbewertung.
Explosionszeit, tatsächliche Ereigniswirkung, zukünftige Pegel und Ankunftszeiten bleiben unbekannt.

### Instabile Kerne und Faulwasserdurchbruch — 0.63, Warnung im Nutzerumfang abgenommen

Live 17. Oktober: neun Kerne bewertet; Kern (168,130,12) mit Faulwasserherkunft,
Zielzutritt und Schutzposition schon in Compact sichtbar. Der Schutzquerschnitt
war unvollständig; Vor-/Nach-Schwelle unverändert. Damit Warnung bestätigt,
aber weder neu entstandener Durchbruch noch wirksame Schutzanlage nachgewiesen.
Nutzerabnahme: Frühe Warnung mit nachvollziehbarem Gefahrenweg und soweit möglich
Eindämmungsvorschlägen genügt. Ein vollständig validierter Bauplan ist keine
Voraussetzung für diese Warnung; fehlende Nachweise weiter ausdrücklich nennen.

`analysis.explosions` bleibt auch in Compact sichtbar. Zuerst Kernposition,
`state`, `currentlyBlocked`, `timing`, `chains` und `unresolved` lesen.
`badwater_breach_to_target_potential` verbindet Kern, beobachteten Speicher,
Kontamination, Geländeschwelle vor/nach möglicher Öffnung, Zielzutritt und
Schutzkandidat. Das Zielgebiet ist die angegebene Schutz-/Teichzone, kein
automatisch identifiziertes Gewässer. Kernposition allein bestimmt keine Gefahr.

Die Bridge liest öffentlichen nativen Radius und Radiuszellen, filtert gegen
festes Terrain und verändert nichts im Spiel. Die Analyse öffnet diese Zellen
nur in einer Kopie. Das ist `native_radius_terrain_scenario`: tatsächliche
Zerstörung, Einstürze, Bauwerkswirkung und Wasserverteilung sind nicht vollständig
modelliert. Countdown und Ankunftszeit bleiben unbekannt. Ein aktuell blockierter
Kern ist keine dauerhafte Entwarnung. Unvollständige Daten/Budgets ergeben
`explosion_risk_unassessed`, niemals eine erfundene Kette oder Sicherheit.

Vor Auslösung Schutzquerschnitt und Krone außerhalb des möglichen Sprengbereichs
prüfen; `protection_inside_possible_blast_relocate` verwirft den Standort.
Auch bei `terrain_route_cut_at_observed_level` zuerst Abfluss und Rückstau prüfen; keine Sperrempfehlung allein aus diesem Wert.
Material, Bauzugang, Fertigstellung vor Auslösung, alternative Rinnen und Rückstau
prüfen. Zielzutritt/Wasserentnahme absichern; Umleitung erst mit belegtem sicheren
Auslass. Compact nennt Kern, Wasserherkunft, Zielzutritt, Schutzposition/-höhe und
Maßnahmen; `detail=full` ergänzt Route, Radius-Terrainzellen und Querschnitt.



## Strategischer Scan 0.62 — begrenzt live abgenommen

Allgemeiner Flussüberblick: `overviewOnly=true, detail="full"`, keine Zielkoordinaten erforderlich.
Nach Gesamtabdeckung `rivers` lesen und Abschnitte/Verbindungen/Quellen mit
`sectionOffset`/`linkOffset`/`sourceOffset` blättern (32/64/16 je Seite). Native
Quellenstärke, Kontamination, Auslässe, Modifikatoren und Wetterregeln getrennt von
örtlicher Flüssigkeit beurteilen. Neue vollständige Scans vor/nach Wetterwechsel
vergleichen; keine zukünftige Kontamination oder Fließrichtung erfinden.
Gate und Livepilot einschließlich Dürrebeginn bestanden: zehn Quellen gingen auf
Stärke null; lokale Restflüssigkeit bleibt getrennt. Faulwasserflutwechsel und
spezielle Aktivierungsregeln sind noch nicht live belegt.

Wenn im aktuellen Katalog vorhanden: `scan_strategic_map` mit aktueller Session
und Wohn-/Versorgungszone als x/y/z/width/height beginnen. Ganze Karte wird gelesen;
mit scanId und identischer Zielzone fortsetzen, zunächst eine Kachel als Pilot.
Laufende Simulation wird nicht pausiert. Erst vollständige Terrainabdeckung liefert
geometrische Zuflusskorridore und konkrete Sperrquerschnitte. Alter, Abdeckung und
Analysestatus beachten; Spezialobjekte über objectOffset blättern.
Keine Flutprognose oder Baufreigabe: bestehende Wasserbauten nicht modelliert,
Quellnamen nur Hinweise, acht Korridore nur Beispiele. Szenariopegel ist angenommene
absolute Rasterhöhe. Vor Maßnahmen lokal frisch prüfen, Umwege und Rückstau
bewerten und native Bauplanung verwenden. Diese Pflege installiert den Skill nicht.
Erster Livepilot: Gesamtabdeckung und verseuchter Tunnel bestätigt; Korrekturen
an Quellenhöhen und Kronenberechnung live nachgeprüft. Tunneldecken lokal prüfen:
angenommene Krone und Bauhöhe sind keine unter einem Dach platzierbare Blockfolge.

## Stand und Geltung

Fachlich aktualisiert am 16. Oktober 2026: Projektstand 0.62; aktuelle Nachweise
und Grenzen im strategischen Abschnitt oben. Frühere Reparaturabnahme 0.60:
Ergebniszeilen lesbar bestätigt. Vorherige Nachweise für Gebäudepause, Simulation,
Pflanzbeleg, Bau-Einzelprojekt und Baucharge bleiben im dokumentierten Umfang gültig.
Diese lokale Hermes-Skillvorlage bleibt ein zurückgestellter Integrationsentwurf.
Die Dokumentationspflege bestätigt weder Hermes-Kompatibilität noch Installation.
Installierte Version, geladene Werkzeuge und echte Rückgaben sind maßgeblich.

## Reparaturstart ab 0.60 (begrenzt live abgenommen)

Erweiterung 0.61, begrenzt live abgenommen: Die Navigationsprüfung
zählt nicht mehr alle Kartenobjekte gegen 4.096, sondern begrenzt die tatsächlichen
Zugangsmesspunkte auf 4.096 und Distriktvergleiche auf 16.384. Alle Baustellen,
Distrikte, Wege und Zugangsobjekte bleiben Beobachtungsziele.
Direkte Großprojekt-Vorabweisungen reichen `project_plan_*` samt lesbarem Grund
durch. Nur mit `execution=not_executed` ist kein neuer Bauauftrag durch diesen
Aufruf bestätigt; bestehende Aufträge bleiben bestehen. Gemeldete Ursache klären,
kein automatisches Retry. Bei unconfirmed nur lesen. Installierten Stand prüfen;
dies ist keine Freigabe oder Installation des Hermes-Entwurfs.
Live bestätigt: große Karte, Höhenfehler, gefährdeter Bestand, gültiger Umweg
und konkrete Pausenablehnung ohne Bauauftrag. Mehrfachzugänge und beide
Objektgrenzen korrigiert. Initialisierung und unbekannter Zustand bleiben
getrennt von fremden Baustellen. Kein zusätzlicher Bauarbeiter-/Liefernachweis.

Bei konkretem Startfehler und `execution=not_executed` hat dieser direkte
Großprojekt-/Reparaturstart keinen neuen Bauauftrag erteilt. Vorhandene Aufträge
bleiben bestehen. `project_start_plan_changed`: frisch lesen und neu planen;
`repair_parent_ineligible`: Elternprojekt diagnostizieren. Andere gemeldete
Vorbedingungen zuerst klären; keine automatische Wiederholung. Bei `unconfirmed`
weiterhin nur lesend klären. Die Ergebniszeile erklärt den Grund auch ingame.

Ein zusätzlicher 0.59-Kontrollfall mit stabilen Reparaturvorschauen, erfolgreichem
Start, Helfer-/Elternabschluss und bestätigter Pause gelang. Der frühere sporadische
`state_conflict` bleibt ursächlich ungeklärt; keine allgemeine Reparaturfreigabe
daraus ableiten. Unter 0.60 falscher Planschlüssel ohne Bauauftrag abgewiesen;
frisch geplanter gültiger Lauf bis Helfer-/Elternabschluss und Freigabe geprüft,
Pause bestätigt. Zwölf Ergebniszeilen in beiden Dateilogs identisch; Ingame-Lesbarkeit vom Menschen bestätigt. [Befund und Livebelege](../repair-start-0.60.md).

## Verbindung verwenden

- Für Timberborn-Aufgaben die von Hermes bereitgestellten MCP-Werkzeuge des
  Eintrags `timberborn-native` verwenden. Zuerst `timberborn_status` aufrufen.
- Hermes verwaltet Initialisierung, Werkzeugliste und den stdio-Serverprozess.
  Der Agent startet für Spielaufgaben weder Start-/Buildskripte noch zusätzliche
  PowerShell-, Proxy- oder dotnet-Prozesse. Kein Prozesswechsel für Schreibzugriff.
- Ein separat im Terminal gestarteter stdio-Prozess kann nicht nachträglich von
  Hermes übernommen werden. Die vorhandene Hermes-Verbindung ist der Zugangsweg;
  Loopback-HTTP der Spiel-Bridge ist kein HTTP/SSE-MCP-Endpunkt.
- Fehlen die MCP-Werkzeuge, die fehlende Verbindung konkret melden. Nur bei einem
  ausdrücklichen Einrichtungs-/Reparaturauftrag Konfiguration oder Prozesse ändern.
  Ein Verbindungsabbruch allein erteilt keinen Auftrag zum Build oder Neustart.
- Technischer Startpfad, vom Hermes-Client verwaltet:
  `hermes-mcp-proxy.py → start-native.ps1 → Timberborn.McpServer.dll`.
  stdout ist MCP-Protokoll, Diagnose gehört nach stderr. Private
  `bridge.local.json` nur als Pfad verwenden; Inhalt und Token nie ausgeben.

## Testmodus und tatsächliche Grenzen

Maßgeblich für verfügbare Aktionsgruppen ist die von Hermes geladene aktuelle `tools/list`-Antwort.
Das lokale Spiel ist ein austauschbares Testsystem. Für auftragsbezogene
Testbauten, Abrisse und Simulationsläufe ist keine zusätzliche Freigabe allein
wegen möglicher Ingame-Verluste nötig.

Budgets und Aktionsgruppen aus den installierten Fähigkeiten und aktuellen
Werkzeugschemata lesen. Die früher hier beschriebenen Millionenbudgets gehören
zu lokalen Experimenten, nicht zu einer eigenständigen Live-Abnahme. Nicht von
hohen Zahlen auf beliebig parallele Projekte schließen. Geometrie-, Weg-,
Baustellen-, Replay- und Zugangsprüfungen gelten weiterhin. Verfügbare Werkzeuge
belegen keine Forschung, Fertigstellung oder Produktion; ein MCP-Neustart setzt
spielseitige Budgets nicht zurück.
## Spielaufgaben ausführen

1. Über `timberborn_status` die aktuelle Sitzung lesen, dann benötigte Zustands-
   und Fähigkeitsabfragen ausführen. Jeder Aufruf braucht `reasoning` mit 1–600
   Zeichen; Argumentnamen und Pflichtfelder aus dem aktuellen Werkzeugschema nehmen.
2. Für Kataloggebäude `inspect_build_options` verwenden. Den Projektumfang über
   `inspect_building_capabilities` prüfen. `supported` ist kein Live-Nachweis.
3. Bauprüfungen und Aufträge gemäß den konkreten Werkzeugbeschreibungen ausführen.
   Neue generische Platzierungen brauchen eine neue UUID als `actionId`; nach
   unklarem Ergebnis zunächst das Objekt mit dieser ID zurücklesen.
4. `applied` bestätigt Platzierung, nicht Fertigstellung. Baustellen, Zugang,
   Fertigstellung und Betrieb getrennt mit den vorhandenen Werkzeugen prüfen.

## Flächen und Bauchargen effizient verwenden

Abgenommener Projektstand: Bridge 0.59.0 im jeweiligen Fachumfang; MCP-only-Erweiterungen zusätzlich am
aktuell geladenen Werkzeugschema erkennen, nicht allein an der Bridge-Version.

- `inspect_colony_overview` bündelt Bevölkerung, Betten und Bedürfnisse; nur drei
  Beispielgüter. Andere Güter über `inspect_goods`, Produktion separat prüfen.
- `survey_region` bis 16×16, pausiert: Hindernisse und Bodenfeuchte. Für Anbau
  `workBuildingId` und `planBuildings=false`; template/districtId bleiben Pflicht.
  `workRange.moistEmptyGroundPatches` sind freie feuchte Rechtecke in der nativen
  Reichweite. Fehlende Daten bleiben unbekannt. Maximal 2048 Reichweitenzellen;
  weder passende Pflanzenart noch Personal oder Ertrag daraus ableiten.
- `soilIsMoist` ist momentane Bodenfeuchte; null ist unbekannt/nicht anwendbar.
  Markierung ist keine Pflanzung. Über `inspect_removal_targets(kind=planted)`
  tatsächliches Leben, Wachstum und Wasserstress kontrollieren.
- Ein Gebäude: `develop_building_project`, danach denselben Auftrag mit
  `advance_building_project` fortsetzen (waitSeconds 0–20).
- Charge: 1–8 Gebäude, Basisfläche plus bis drei `additionalRegions`, jede bis
  8×8. Bei Platzmangel vorwärts wechseln, keine freie Kartensuche. Räumung einmal
  je Fläche, maximal 64 Ziele insgesamt; gesamte Spielzeitbudgets höchstens 672 h.
  `advance_building_batch` hat waitSeconds 0–45. Ohne Aufrufe läuft nur ein bereits
  gestartetes begrenztes Zeitfenster weiter, kein Hintergrunddispatcher.
- `regionIndex`/`previousRegions` zeigen Teilflächenwechsel. Unbestätigte
  Eingriffe oder ausgeschöpfte Räumzeit stoppen die ganze Charge. Materialmangel
  ist keine Startsperre; Lieferungen nach Auftrag innerhalb der Zeitfenster verfolgen.
  Gleiche batchId ist Status-Replay; geänderte Parameter sind nicht zulässig.
- `finished_accessible` ist Bau-/Zugangsnachweis, kein Betriebsnachweis.
  `checkpointOnly` bedeutet gespeicherte Beobachtung. Mehrere alternative
  Baukandidaten können überlappen und sind keine gemeinsame Chargenkapazität.

## Aufruflog und Ergebnisbelege (0.55–0.59)

- `inspect_agent_log`: höchstens 128 Aufrufe im aktuellen Sitzungspuffer,
  `limit` 1–32. Mit `nextCursor` und unveränderten `tool`/`state`/`projectId`/`runId`-
  Filtern weiterblättern; alle Filter gelten mit UND. Nach Löschen/Neuladen frisch
  beginnen. Status und Bezüge können sich nach dem Lesen ändern.
- `gapDetected`/`unavailableBeforeCursor` zeigen verdrängte ältere Aufrufe vor
  Filtern. Null bedeutet keinen garantierten vollständigen Verlauf; Telemetrie
  kann fehlen. Der Logleser kann selbst als laufender Eintrag enthalten sein.
- Ab 0.56: Projekt-/Elternprojekt-/Laufbezüge unterscheiden `request` und `result`.
  Kennungen sind Zuordnung, kein Existenz-/Wirkungsnachweis. Keine Bezüge aus
  beliebigen `jobId`/Gebäude-IDs erfinden. `projectId` trifft auch Elternprojekte.
- Seit 0.57 zeigt „MCP-Aufrufe“ keine Projekt-/Laufkennungszeilen mehr. Technische
  Bezüge bleiben im MCP-Ergebnis und in lokalen JSON-Zeilenlogs erhalten.
  `%LOCALAPPDATA%\TimberbornMcp\Logs\bridge` und `...\mcp`: neue Datei pro geladener
  Sitzung mit Kolonie-/Spielstandname und UTC-Zeitstempel; MCP ab erstem Kontakt,
  zusätzlich Prozessdatei. Keine Rotation/Größenbegrenzung/automatische Löschung.
  Zur Diagnose gezielt Datei und Zeitraum lesen, nicht alle Logs laden.
  `failedWrites` und Sequenz beachten; lokale Logs nicht veröffentlichen/committen.
- Ab 0.58 begrenzt live abgenommen: optionales `outcomeSummary` nach Aufrufabschluss,
  ingame „Ergebnis:“. Kein Eingabeparameter; leer heißt kein Wirkungsnachweis.
  Gebäudeeinstellung: geändert/bereits passend/unbestätigt; freigegeben ist nicht
  nachgewiesener Betrieb. Simulation: Laufstatus, beobachtete Spielstunden und
  bestätigte Pause getrennt; Zeitlimit/Messende ist kein Produktionsziel.
  Pflanzcharge: Vorschau versus gesetzte/vorhandene/ausgeschlossene/unbestätigte/
  nicht versuchte Markierungen; kein Wachstums- oder Ertragsnachweis.
  `inspect_planting_batch` liefert einen historischen Beleg. Startzeilen werden
  beim späteren Ende eines Laufs nicht automatisch umgeschrieben; dessen Status
  mit derselben Kennung lesen. Keine zusätzlichen Schreibaufträge für Logtexte.
- 0.59 begrenzt live abgenommen: Einzelprojekt, Ein-Gebäude-Charge und
  Vier-Teile-Großprojekt am Westufer bis 4/4 fertig/zugänglich. Gezielter
  Großprojektstopp: 2/4 fertig, eine Baustelle, ein nicht erteilter Auftrag;
  nach Fertigbau der vorhandenen Baustelle Freigabe mit 3/4, letzter Auftrag
  aufgegeben. Alle geprüften Ergebniszeilen ingame lesbar bestätigt.
  Zusätzlich Räumgruppe (sechs Stümpfe, danach Lager/Weg), Spielzeitbudgetende und
  stop_building_batch live geprüft. Reparaturvorschau zulässig, Start jedoch
  state_conflict/unconfirmed: in dieser Probe kein erfolgreicher Reparaturabschluss.
  Späterer unabhängiger Kontrollfall erfolgreich, siehe Abschnitt 0.60. Vor Retry
  lesend klären und Ablehnungsursache diagnostizieren, nicht mit neuer ID umgehen.
  Andere Teilfehler und lange Räumläufe nicht pauschal abgenommen. Ergebniszeilen für
  Einzelprojekte, Bauchargen, Großprojekte und Räum-/Baugruppen. Bis 0.62 ist Auftrags-`completed` kein Fertigbau; ab 0.63 heißen angenommene Aufträge `ordered`, `completed` verlangt Fertigbau.
  Bauabschluss `finished_accessible` belegt Fertigbau/Zugang/Pause, keinen Betrieb.
  Chargen zählen Gebäude; Großprojekte Bauteile einschließlich Wege/Hilfsbauten.
  Gespeicherte Stände sind keine frische Zugangsprüfung; Diagnose/Close prüfen
  frisch, Räum-/Baugruppen liefern weiter gespeicherte Belege. Projektfreigabe
  kann offene Teile aufgeben. stop_large_project fordert Pause an; erst
  pauseConfirmed=true und Tempo 0 bestätigen sie. stop_building_batch lässt vorhandenes Laufbudget
  bestehen; live Tempo 7 bis zum ursprünglichen Budgetende bestätigt. Historischer
  Chargenbeleg kann bei 0/2 bleiben, obwohl frische Einzelabfrage 1/1 fertig meldet.
  Zeitlimit-/Stopursache im Fachbericht lesen, Kurztext noch allgemein.
  Zusatzprotokoll: `docs/agent-log-live-tests-0.59.md` im Projekt.
  Angefragter Gruppenstopp ist noch kein Abschluss. Vorschau erteilt
  keine Aufträge. Fehlende Angaben nicht als null fertige Teile interpretieren.
  Dieselbe Kennung weiter lesend prüfen, keine unbestätigten Aufträge wiederholen.
  Fachumfang: `docs/agent-log-building-outcomes-0.59.md` im Projekt.
- Für tatsächliche Spieländerungen das Spieländerungsjournal und Fachabfragen
  nutzen. Aufrufstatus, Absicht und bestätigte Wirkung sind verschiedene Aussagen.
  Keine Wiederholung bei `unconfirmed`; strukturierten Fachbericht lesend klären.

## Fehler richtig behandeln

### Parameter vor jedem Aufruf prüfen

Das aktuelle Eingabeschema des konkreten Werkzeugs ist verbindlich: alle
Pflichtfelder senden, keine zusätzlichen Felder hinzufügen. `session` und
`actionId` sind keine universellen MCP-Argumente. IDs aus aktuellen Rückgaben
übernehmen; keine Beispiel-UUIDs als echte Sitzungs-, Gebäude- oder Distrikt-IDs
verwenden. Eine neue `actionId` nur für einen tatsächlich neuen Bauauftrag erzeugen.

Korrekte Argumente für die vorhandenen nativen Werkzeuge:

| Werkzeug | Argumente |
| --- | --- |
| `timberborn_status` | `{"reasoning":"Aktuelle Spielsitzung lesen"}` |
| `inspect_colony` | `{"reasoning":"Koloniezustand abfragen"}` |
| `find_buildings` | `{"offset":0,"limit":32,"reasoning":"Gebäude und Distriktzentren suchen"}` |
| `inspect_build_options` | `{"offset":0,"limit":32,"reasoning":"Verfügbare Bauvorlagen lesen"}` |

Insbesondere bei `inspect_colony` weder `session` noch `actionId` senden.
Für Distriktinformationen die dafür passenden Gebäude-/Wegwerkzeuge mit echten
IDs und ihrem jeweiligen Schema verwenden; zusätzliche Argumente erweitern
den Funktionsumfang von `inspect_colony` nicht.

Bei `missing required argument(s)` oder `invalid_arguments` zuerst Werkzeugname,
Pflichtfelder, zusätzliche Felder, Typen und Wertebereiche prüfen. Keine Folge
anderer Werkzeuge mit geratenen Parametern versuchen. Ein vom Client ausdrücklich
nicht ausgeführter Aufruf darf nach Korrektur gesendet werden. Bei einer
Schreibaktion mit unklarem Ergebnis dagegen zuerst ihre tatsächliche Wirkung lesen.

### Hermes-Sperre nach drei Fehlern

Die hier untersuchte Hermes-Version zählt auch beantwortete MCP-Werkzeugfehler
wie `invalid_arguments` als Serverfehler. Nach drei solchen Fehlern kann sie
`unreachable after 3 consecutive failures` melden, obwohl der MCP-Prozess läuft
und zuvor geantwortet hat. Die Meldung allein beweist keinen Transportausfall.

Während `Auto-retry available in ~Ns` keine weiteren Werkzeuge dieses Servers
aufrufen, auch kein anderes Timberborn-Werkzeug als Umgehung. Die vorherigen
Fehler auswerten und die angegebenen Argumente korrigieren. Nach Ablauf der
genannten Wartezeit genau einmal `timberborn_status` mit ausschließlich
`reasoning` aufrufen. Bei Erfolg die aktuelle Session übernehmen und mit korrekten
Argumenten fortfahren. Bei erneutem Fehler den konkreten Befund melden und
gezielt diagnostizieren; keine Retry-Schleife, keine Startskripte, keinen Build
und keinen Prozessneustart aus dieser Meldung ableiten.

Diese Skillregel korrigiert das Agentenverhalten; sie verändert nicht Hermes'
Fehlerzähler oder dessen technische Verbindungssperre.

### Spiel- und Aktionsfehler

- „Fehler – Zustand prüfen“ ist ein Sammelstatus im MCP-Aktivitätslog. Den
  konkreten Werkzeugnamen, `error.code` und die Rückgabe auswerten.
- Fachliche Ablehnungen, verbrauchte IDs, Sitzungsgrenzen, `sessionLocked` oder
  unbestätigte Aktionen nicht durch neue Prozesse, neue IDs oder Wiederholungen
  umgehen. Ein MCP-Neustart setzt keine spielseitigen Sitzungsbudgets zurück.
- `backend_unavailable` kann ein nicht laufendes Spiel, eine fehlende Mod oder
  einen noch nicht geladenen Spielstand bedeuten; es beweist keinen defekten MCP-
  Prozess. Ladezustand/Verbindung gezielt klären, keine automatischen Neustarts.
- Bei `unconfirmed` oder Abbruch erst lesend klären, ob die Aktion wirksam wurde.
  Einen erfolgreichen vorherigen Aufruf nicht wegen eines späteren Lesefehlers
  als fehlgeschlagen darstellen.

## Aktualisierungen

Quellcodebeschreibungen werden erst nach einem Build im Server sichtbar;
spielseitige Grenzänderungen brauchen zusätzlich die aktualisierte geladene Mod.
Der Mensch führt `scripts/prepare-human-live-test.ps1` aus und meldet nach dem
Laden `live bereit`. Das Skript übernimmt die Build-/Installationskette und startet
seit 0.57 nach erfolgreicher Paketprüfung das Spiel automatisch. Der Spielagent
führt dieses Entwicklerskript nicht selbst aus.
Nach einem Update die Hermes-MCP-Verbindung bzw. Hermes-Sitzung einmal regulär
erneuern, damit neue Werkzeugbeschreibungen geladen werden. Dies ist ein
Update-Schritt, kein Bestandteil jedes Spielauftrags.
