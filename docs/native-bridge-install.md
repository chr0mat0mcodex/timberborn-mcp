# Eigene Timberborn Agent Bridge — 0.2.0

Das Endziel ist ein Agent, der Timberborn über MCP spielt: Wasser, Nahrung, Holz,
Wege und Wohnraum aufbauen und die Versorgung anhand frischer Beobachtungen steuern.
Dieser erste unabhängige Mod-Baustein ist ausschließlich lesend. Bau- und
Steuerungsaktionen folgen nach dem Live-Abgleich der Spielkoordinaten und Zustände.

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
2. Spiel starten, die Mod **Timberborn MCP Agent Bridge (Read-only Prototype)**
   aktivieren und gegebenenfalls neu starten. Spielstand **MCP** laden.
3. Die Mod benötigt keine anderen Mods. Bestehende Mods müssen für den ersten
   Test nicht entfernt werden. Port 8081 trennt sie vom früheren HTTP-POC auf 8080.

Grundlage: [offizielle Mod-Verzeichnisstruktur](https://github.com/mechanistry/timberborn-modding/wiki/Mod-directory-structure)
und [offizieller Mod-Lebenszyklus](https://github.com/mechanistry/timberborn-modding/wiki/Timberborn-architecture).
Geprüfte Compile-Referenzen: Timberborn 1.1.2.4; Laden und tatsächliche Werte
müssen einmal im Spiel geprüft werden. Ein erfolgreicher Build ersetzt diesen Test nicht.

## MCP-Verbindung und Abnahme

Den vorhandenen stdio-MCP-Server ausdrücklich mit folgenden Prozessvariablen starten:

```text
TIMBERBORN_BACKEND=native
TIMBERBORN_NATIVE_CONFIG=<absoluter Pfad zur installierten bridge.local.json>
```

Die Client-Konfiguration bleibt bis zur gesonderten Einrichtung unverändert.
Es gibt keinen automatischen Wechsel zwischen eigener Mod und Fremdmods.
Die drei Werkzeuge sind `timberborn_status`, `inspect_colony` und
`inspect_map_region(x,y,z,width,height,depth)`. Auch mit gesetztem Schreib-Opt-in
bietet dieses Backend keine Schreibwerkzeuge an. Ein Browseraufruf ohne Schlüssel
bekommt absichtlich HTTP 401; es gibt keinen öffentlichen Ping.

Live-Abnahme nach Installation: Verbindung, drei Beispielgüter (Water/Berries/Log),
Bevölkerung, belegte/freie Betten und Obdachlose mit der UI vergleichen. Danach
Objektkoordinate und einen höchstens 8×8×4 Zellen großen Kartenausschnitt prüfen.
Zurück zum Menü und Spielstand erneut laden: Sitzung muss wechseln und die
Verbindung wieder funktionieren. Während des Ladens darf ein Aufruf scheitern.
Der erste Test verändert keine Gebäude, Pausenstände oder Spielstände.

Grenzen: globale Werte; Beeren sind nicht die gesamte Nahrung. Die Objektstichprobe
enthält höchstens 16 Blockobjekte, auch natürliche Objekte; Namen sind keine stabilen
Vorlagen-IDs. Rohkoordinaten und Wasser-/Geländesemantik sind noch live abzugleichen.
Keine Aussage über Bebaubarkeit, Wegeanbindung oder Reichweite. Ressourcen-/Betten-
Zähler können vom Spiel verzögert aktualisiert werden. Die Abfrage läuft auf dem
Spielhauptthread, bildet aber keinen atomar eingefrorenen Simulationszustand ab.

Transport: ausschließlich Loopback, eigener Bearer-Schlüssel, feste GET-Routen,
keine Browser-Origin-Aufrufe, keine Weiterleitungen, begrenzte Antwortgröße und
Zeitlimits. Spielzugriffe werden auf dem Hauptthread abgearbeitet; beim Entladen
werden wartende Anfragen verworfen. Keine Save-Erweiterung und keine Schreibroute.
