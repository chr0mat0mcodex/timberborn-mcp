---
name: timberborn-mcp-server
description: Timberborn über die vorhandene Hermes-MCP-Verbindung steuern; Testmodus mit allen Aktionsgruppen, Baugrenzen und Fehlerdiagnose. Keine manuellen Serverstarts für Spielaufgaben.
---

# Timberborn in Hermes

## Stand und Geltung

Fachlich aktualisiert am 16. Oktober 2026: Projektstand 0.58 begrenzt live
abgenommen: Gebäudepause, Ein-Stunden-Lauf und Pflanzbeleg, lesbare Ergebniszeilen bestätigt.
Diese lokale Hermes-Skillvorlage bleibt ein zurückgestellter Integrationsentwurf.
Die Dokumentationspflege bestätigt weder Hermes-Kompatibilität noch Installation.
Installierte Version, geladene Werkzeuge und echte Rückgaben sind maßgeblich.

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

Abgenommener Projektstand: Bridge 0.58.0 im jeweiligen Fachumfang; MCP-only-Erweiterungen zusätzlich am
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
- `regionIndex`/`previousRegions` zeigen Teilflächenwechsel. Materialmangel,
  unbestätigte Eingriffe oder ausgeschöpfte Räumzeit stoppen die ganze Charge.
  Gleiche batchId ist Status-Replay; geänderte Parameter sind nicht zulässig.
- `finished_accessible` ist Bau-/Zugangsnachweis, kein Betriebsnachweis.
  `checkpointOnly` bedeutet gespeicherte Beobachtung. Mehrere alternative
  Baukandidaten können überlappen und sind keine gemeinsame Chargenkapazität.

## Aufruflog und Ergebnisbelege (0.55–0.58)

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
