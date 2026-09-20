# Eigene Timberborn Agent Bridge

Aktuell vorbereitet: **0.16.0**, [Ingame-MCP-Log](activity-log.md).
18 native Leser plus getrennt freizugebende Aktionen. Das Log und das optionale
reasoning-Feld benötigen keine neue Konfigurationsfreigabe. Bestehende Freigaben,
Port und Schlüssel bleiben erhalten. Vor dem Update Spielstand speichern und Spiel
beenden, Modordner sichern, nur die fünf Paketdateien ersetzen.
Installiert/live geprüft bleibt 0.15.0 bis zum Update. Fensterdarstellung noch nicht live bestätigt.
Die folgenden Versionsabschnitte dokumentieren frühere Erweiterungen.

Neu vorbereitet: 0.11.0 ergänzt alle regulären Geschwindigkeiten 0/1/3/7.
Die Sollbesetzungssteuerung aus 0.10.0 ist enthalten; bestehende Opt-ins bleiben.

Neu vorbereitet: 0.10.0 mit [regulärer Sollbesetzung](workplace-staffing.md).
Eigene Freigaben enableStaffing / TIMBERBORN_ENABLE_STAFFING; Paketstandard aus.
Installiert/live bestätigt bleibt 0.9.0 bis zum Update.

Neu vorbereitet: 0.9.0 mit [Gebäude-Betriebsdiagnose](building-operations.md).
Zusätzlich [kolonieweite Arbeitskräfteliste](workforce-roster.md) mit inspect_workforce.
Keine zusätzlichen Aktionsfreigaben nötig.
Installiert/live geprüft bleibt 0.8.0 bis zum Update.

Neu vorbereitet: 0.8.0 mit [Pause/Normalgeschwindigkeit](simulation-control.md).
Mod-Opt-in enableSpeedControl und MCP-Opt-in TIMBERBORN_ENABLE_SPEED_CONTROL; beide
im Standard aus. Installiert/live geprüft bleibt 0.7.0 bis zum nächsten Update.

0.6.1 ersetzt die unklare Restmaterialangabe durch gesamte Vorlagenkosten und
tatsächlichen Baustellenbestand. Beide Werte getrennt lesen, keinen Restbedarf ableiten.
0.6.1 ist installiert/live bestätigt. Neu gebaut: 0.7.0 mit separat geschütztem Lodge-Auftrag; noch nicht installiert/live geprüft. Siehe [Lodge-Pilot](lodge-placement.md).

0.6.0 ergänzt die rein lesende [Baustellen-/Distriktabfrage](building-observations.md)
`inspect_building(id, session)`.

0.5.0 ergänzt den separat freizuschaltenden [einzelnen Wegauftrag](path-placement.md).
Paketstandard: `enablePlacement: false`; MCP benötigt zusätzlich
`TIMBERBORN_ENABLE_PLACEMENT=1`. Ein Versuch je Sitzung, kein automatisches Retry.
Die folgenden Vorschauangaben beschreiben weiterhin die separat schaltbare Validierung.

Das Endziel ist ein Agent, der Timberborn über MCP spielt: Wasser, Nahrung, Holz,
Wege und Wohnraum aufbauen und die Versorgung anhand frischer Beobachtungen steuern.
Standardmäßig sind sieben Leserouten aktiv. 0.4.0 ergänzt eine separat geschützte
temporäre Vorschauvalidierung; sie platziert keine Gebäude. Der Wegpilot aus 0.5.0
ist separat freizuschalten; weitere Bau- und Steuerungsaktionen sind noch offen.
Keine Screenshot-Auswertung und keine simulierten Maus-/Tastatureingriffe zur Spielsteuerung.

## Lokales Paket bauen

PowerShell im Repository, mit dem Managed-Verzeichnis der eigenen Spielinstallation:

```powershell
./scripts/build-native-bridge.ps1 -TimberbornManagedDir '<Spielverzeichnis>/Timberborn_Data/Managed'
```

Benötigt .NET SDK 10; die Mod selbst zielt auf netstandard2.1. Die aktuellen
Spiel-DLLs sind nur Compile-Referenzen. Im Paket liegen ausschließlich unsere zwei
DLLs, Manifest, Lizenz und Anleitung. Keine Fremdmod-Abhängigkeit, keine kopierten
Spiel-DLLs, kein Publicizer und keine private Reflection.

Jeder Build erzeugt einen neuen Ordner unter `.local/packages/`. Der darin liegende
Installationsordner `TimberbornAgentBridge` enthält zusätzlich eine frisch erzeugte
private `bridge.local.json` (Port 8081 und Zufallsschlüssel). Diesen Schlüssel nicht
teilen oder einchecken. Das danebenliegende `code-only.zip` enthält keinen Schlüssel
und ist deshalb allein noch keine einsatzfertige Installation.

## Manuell im Spiel installieren

1. Timberborn beenden. Den gesamten erzeugten Ordner `TimberbornAgentBridge` nach
   `Dokumente/Timberborn/Mods/TimberbornAgentBridge` kopieren. Die Dateien müssen
   direkt darin liegen, ohne zusätzlichen gleichnamigen Unterordner.
2. Spiel starten, die Mod **Timberborn MCP Agent Bridge (Prototype)**
   aktivieren und gegebenenfalls neu starten. Spielstand **MCP** laden.
3. Die Mod benötigt keine anderen Mods. Bestehende Mods müssen für den ersten
   Test nicht entfernt werden. Port 8081 trennt sie vom früheren HTTP-POC auf 8080.

Grundlage: [offizielle Mod-Verzeichnisstruktur](https://github.com/mechanistry/timberborn-modding/wiki/Mod-directory-structure)
und [offizieller Mod-Lebenszyklus](https://github.com/mechanistry/timberborn-modding/wiki/Timberborn-architecture).
Geprüfte Compile-Referenzen: Timberborn 1.1.2.4; Laden und tatsächliche Werte
müssen einmal im Spiel geprüft werden. Ein erfolgreicher Build ersetzt diesen Test nicht.

## MCP-Verbindung und Abnahme

Einfacher Startweg für einen lokalen stdio-MCP-Client (nach dem Build):

```powershell
pwsh -NoProfile -File ./scripts/start-native.ps1 -ConfigPath '<installierte Mod>/bridge.local.json'
```

Im Client absolute Pfade verwenden. Dieser Starter wählt ausdrücklich die eigene Bridge,
deaktiviert Schreib-/Vorschauaktionen und gibt den Schlüssel nicht aus. Er registriert
keinen Client, installiert nichts und wartet auf MCP-Nachrichten über stdin.

Wiederholbare Abnahme bei laufendem Spiel und geladener Testkolonie:

```powershell
pwsh -NoProfile -File ./scripts/verify.ps1 -NativeConfig '<installierte Mod>/bridge.local.json'
```

Führt zuerst Build und reguläre Tests aus, danach genau den nativen Lesetest mit sieben
Werkzeugen. Kein Legacy-API-Zugriff, kein automatischer Wiederholungsversuch bei Fehlern.
Ohne `-NativeConfig` bleibt die Prüfung offline; `-Live` bezeichnet weiterhin den Legacy-Test.

Den vorhandenen stdio-MCP-Server ausdrücklich mit folgenden Prozessvariablen starten:

```text
TIMBERBORN_BACKEND=native
TIMBERBORN_NATIVE_CONFIG=<absoluter Pfad zur installierten bridge.local.json>
```

Die Client-Konfiguration bleibt bis zur gesonderten Einrichtung unverändert.
Es gibt keinen automatischen Wechsel zwischen eigener Mod und Fremdmods.
Die sieben Werkzeuge sind `timberborn_status`, `inspect_colony`,
`inspect_map_region(x,y,z,width,height,depth)`, `find_buildings(offset,limit)`,
`inspect_build_catalog`, `precheck_build_site(template,x,y,z,rotation)` und
`inspect_building(id, session)` (letzteres ab 0.6.0). Auch mit gesetztem Schreib-Opt-in
bietet dieses Backend ohne separates Placement-Opt-in keine Schreibwerkzeuge an. Ein Browseraufruf ohne Schlüssel
bekommt absichtlich HTTP 401; es gibt keinen öffentlichen Ping.

Optional ab Mod 0.4.0: `validate_build_site(template,x,y,z,rotation,session)`.
Benötigt zusätzlich `enableValidation: true` in der privaten Mod-Konfiguration
und `TIMBERBORN_ENABLE_VALIDATION=1` im MCP-Prozess. In neuen Paketen standardmäßig aus.
Verwendet ausschließlich die festgelegte POST-Route, frische Sitzungs-ID und maximal
acht Versuche je Spielsitzung. Erzeugt eigene verborgene Vorschauen; keine Platzierung.
Nicht als rein lesend oder automatisch wiederholbar behandeln. Details und Live-Grenzen:
[Geschützte Spielvalidierung](native-validation.md).

Live-Abnahme nach Installation: Verbindung, drei Beispielgüter (Water/Berries/Log),
Bevölkerung, belegte/freie Betten und Obdachlose strukturiert abfragen. Die Basiswerte
wurden für 0.2.0 bereits durch den Nutzer bestätigt. Danach Objektbelegung,
Eingang und höchstens 8×8×4 Zellen großen Kartenausschnitt über die Spielservices
gegeneinander prüfen. [Kontrollierter räumlicher Pilot](spatial-precheck.md).
Zurück zum Menü und Spielstand erneut laden: Sitzung muss wechseln und die
Verbindung wieder funktionieren. Während des Ladens darf ein Aufruf scheitern.
Der erste Test verändert keine Gebäude, Pausenstände oder Spielstände.

Grenzen: globale Werte; Beeren sind nicht die gesamte Nahrung. Die Objektstichprobe
enthält höchstens 16 Blockobjekte, auch natürliche Objekte; Namen sind keine stabilen
Vorlagen-IDs. Die separate Gebäude-/Wegeabfrage liefert dagegen TemplateSpec-IDs
und belegte Zellen. Rohkoordinaten und Wasser-/Geländesemantik sind noch live abzugleichen.
Die neue Bauplatzvorprüfung ersetzt keinen vollständigen Spielvalidator und bestätigt
keine Distriktanbindung oder Reichweite. Ressourcen-/Betten-
Zähler können vom Spiel verzögert aktualisiert werden. Die Abfrage läuft auf dem
Spielhauptthread, bildet aber keinen atomar eingefrorenen Simulationszustand ab.

Transport: ausschließlich Loopback, eigener Bearer-Schlüssel, feste GET-Leserouten,
keine Browser-Origin-Aufrufe, keine Weiterleitungen, begrenzte Antwortgröße und
Zeitlimits. Spielzugriffe werden auf dem Hauptthread abgearbeitet; beim Entladen
werden wartende Anfragen verworfen. Keine eigene Save-Erweiterung.
POST-Validierung und POST-Wegplatzierung sind getrennt freizuschalten. Ein gebauter
Weg ist regulärer Spielzustand und kann vom Spiel gespeichert werden.

## Erweiterung 0.12.0

[Prioritäten, Baustellen und Flächen](priorities-construction-areas.md) ergänzen vier
Lesewerkzeuge (insgesamt 13) und zwei getrennt gesperrte Schreibwerkzeuge.
In bridge.local.json enablePriorities bzw. enableAreas aktivieren und im gezielten
MCP-Testprozess TIMBERBORN_ENABLE_PRIORITIES=1 bzw. TIMBERBORN_ENABLE_AREAS=1 setzen.
Standardstart und Standardtests bleiben ohne Schreibfreigaben. Mod nur bei beendetem
Spiel ersetzen; private Konfiguration und bestehende Marketplace-Metadaten erhalten.

## Erweiterung 0.13.0

[Kiefernschutz und Entfernung](removal-and-pine-protection.md): tapping entfernt jetzt
Fällmarkierungen. Ein zusätzliches Lesewerkzeug (insgesamt 14) und vier getrennte
Entfernungswerkzeuge. enableRemoval und TIMBERBORN_ENABLE_REMOVAL=1 sind unabhängig
von Flächen-/Prioritätsfreigaben nötig. Bestehende Konfiguration beim Update erhalten;
Abriss zunächst deaktiviert lassen, bis ein passendes Testziel feststeht.

## Patch 0.13.1

Korrigiert die Ergebnisbewertung für reguläre Vegetations-Mark-Aufrufe, die ein Objekt
sofort entfernen. Keine neue Konfiguration; bestehende Opt-ins erhalten. Installation
wie gewohnt bei beendetem Spiel mit Sicherung. Live-Nachweis der korrigierten Antwort
erfolgt nach Neustart; 0.13.0 hatte die tatsächliche Entfernung bereits ausgeführt.

## Patch 0.13.2

Ergänzt Lebenszustand, Sterbezustand, Wasserstress und Wachstum natürlicher Ressourcen.
Die Zapfkandidaten-Abfrage schließt tote, sterbende, junge und unbekannte Kiefern aus.
[Details und Grenzen](vegetation-state.md). Mod bei beendetem Spiel gesichert ersetzen;
private Konfiguration unverändert lassen. Nach Neustart zuerst rein lesende Abnahme.

## Forschung ab 0.17.0

`enableResearch` in der privaten Mod-Konfiguration und `TIMBERBORN_ENABLE_RESEARCH=1`
im MCP-Prozess geben `unlock_building` frei. Beide Freigaben sind erforderlich und
standardmäßig deaktiviert. `inspect_research` benötigt keine Schreibfreigabe.
Bestehenden Token und andere Einstellungen bei Updates erhalten.
