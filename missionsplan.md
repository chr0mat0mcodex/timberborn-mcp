# Missionsplan: Timberborn MCP

Stand: 2026-09-19. Status: Read-only-POC implementiert und technisch live geprüft; UI-Werte vom Nutzer bestätigt.

## Auftrag und Freigabegrenzen

Ziel des ersten POC: Ein lokaler MCP-Agent beantwortet Fragen zu Zeit,
Wetter, Bevölkerung und Gebäuden einer geladenen Timberborn-Kolonie.

- Schritt 1 (Quellenprüfung) ist abgeschlossen und hier dokumentiert.
- Schritt 2 (Architekturplanung) wurde ausdrücklich freigegeben und ist unten ausgearbeitet.
- Schritt 3 (Werkzeugspezifikation) wurde ausdrücklich freigegeben und ist unten ausgearbeitet.
- Schritt 4 (Umsetzung vorbereiten) wurde ausdrücklich freigegeben und ist unten ausgearbeitet.
- Schritt 5 wurde mit „5 go“ freigegeben: Umsetzung einschließlich projektlokalem Restore, Build, Tests und Checkpoints.
- Mods installiert und aktiviert der Nutzer selbst im Spiel.
- Keine System-, Spiel- oder MCP-Client-Konfiguration ändern; nichts installieren.
- Ausnahme auf ausdrücklichen Folgeauftrag am 2026-09-19: lokalen Timberborn-MCP-Eintrag im Codex-Client einrichten. Registrierung und separater stdio-Livetest erfolgreich; keine Spielkonfiguration geändert.
- Implementierung im Projektordner erlaubt; keine Spielaktionen oder Save-Manipulation.
- Nachträglich am 2026-09-19 freigegeben: Veröffentlichung auf GitHub unter chr0mat0mcodex einschließlich projektspezifischer Agentenanweisungen und regelmäßiger Pushes geprüfter Checkpoints. Ablauf: DEVELOPMENT_WORKFLOW.md.
- Die nachfolgenden Planungsabschnitte dokumentieren den damaligen Stand; aktuelle Ergebnisse stehen in Abschnitt 5.

Grundlage ist das vom Nutzer bereitgestellte `projekt_timberborn_MCP.md`.
Die Freigabe zur Umsetzung erweitert die vorherige Planungsphase; System- und Spielkonfiguration bleiben ausgenommen.

## Missionsablauf

| Schritt | Ergebnis | Status |
|---|---|---|
| 1. Schnittstelle prüfen | Mod-Liste, belegter API-Umfang und Unsicherheiten | Abgeschlossen; Quellenprüfung, kein Live-Test |
| 2. Architektur festlegen | Komponenten, Abhängigkeiten und Datenfluss | Als Plan ausgearbeitet |
| 3. Werkzeuge definieren | Konkrete Ein-/Ausgaben, Limits und Fehlerverhalten | Als Plan ausgearbeitet |
| 4. Umsetzung vorbereiten | Konkrete Dateien, Paketversionen und Testfälle | Als Plan ausgearbeitet; Live-API noch nicht erreichbar |
| 5. Implementierungsfreigabe | Nutzer bestätigt konkreten Umfang | Erteilt |
| 6. POC umsetzen und testen | Fake-/HTTP-Tests, danach Spieltest | Implementiert; Live-Test und UI-Vergleich erfolgreich |

## 1. Quellenprüfung

### 1.1 Ausgangslage aus der vorherigen lokalen Bestandsaufnahme

- Timberborn: `1.1.2.4-52e959e-sw` laut lokaler Versionsdatei.
- SDKs: .NET 9.0.102 und 10.0.303 vorhanden.
- Repository: initialisiert, noch keine Commits oder Remotes.
- TimberUi 11.0.0 lokal gefunden; Aktivierung nicht bestätigt.
- More HTTP API und dessen weitere Abhängigkeiten in den geprüften Mod-Verzeichnissen nicht gefunden.
- Zum Prüfzeitpunkt lief Timberborn nicht; kein erfolgreicher API-Aufruf.
- Unity Hub/Editor nicht gefunden. Für einen externen HTTP-Client ist kein eigener Unity-Build vorgesehen.

Diese Angaben sind eine Bestandsaufnahme, kein Nachweis einer funktionierenden Mod-Kombination.

### 1.2 Mod-Liste und Abhängigkeiten

| Komponente | Aufgabe | Quellenlage |
|---|---|---|
| [More HTTP API](https://steamcommunity.com/sharedfiles/filedetails/?id=3729176372) | Spielzugriff über HTTP | Manifest nennt ModdableTimberborn und eMka.ModSettings |
| [Moddable Timberborn](https://steamcommunity.com/workshop/filedetails/?id=3578009707) | Gemeinsame Spiel-API für Mods | Manifest nennt Harmony und TimberUi >=11.0.0 |
| [Mod Settings](https://steamcommunity.com/sharedfiles/filedetails/?id=3283831040) | Einstellungen der Mod | Direkte Abhängigkeit von More HTTP API |
| [Harmony](https://steamcommunity.com/sharedfiles/filedetails/?id=3284904751) | Patching-Bibliothek | Indirekte Abhängigkeit über Moddable Timberborn |
| [TimberUi](https://steamcommunity.com/workshop/filedetails/?id=3430626620) | Gemeinsame UI-Bibliothek | Indirekte Abhängigkeit; lokal bereits gefunden |

Belege: [More-HTTP-API-Manifest](https://raw.githubusercontent.com/datvm/TimberbornMods/master/MoreHttpApi/manifest.json),
[Moddable-Timberborn-Manifest](https://raw.githubusercontent.com/datvm/TimberbornMods/master/ModdableTimberborn/manifest.json).

Versionsunsicherheit:

- Abrufbarer, mehrere Monate alter More-HTTP-API-Quellstand: 10.0.0, Mindestspielversion 1.0.0.
- Projektkonzept: 11.0.0, Mindestspielversion 1.1.1; in dieser Recherche nicht unabhängig bestätigt.
- Jüngerer Moddable-Timberborn-Manifeststand: 11.1.2, Mindestspielversion 1.1.1.
- Für Harmony und Mod Settings wurde keine konkrete Paketversion festgelegt.
- Die Quellen bilden keinen gemeinsam gepinnten Release-Stand ab. Die aktuell im Spiel angebotenen
  Pakete und später deren lokale Manifeste sind vor einem Kompatibilitätsurteil zu prüfen.
- Workshop-Tags, Mindestversionen und Quellcode ersetzen keinen Test mit Timberborn 1.1.2.4.

Das ist eine Planungsliste. Installation und Aktivierung übernimmt der Nutzer.
More HTTP API Automations ist für den ersten POC nicht erforderlich.

### 1.3 Belegte Leserouten

Vorgesehene Basis: lokal konfigurierbare HTTP-Adresse mit Präfix `/MoreHttpApi/`.
`localhost:8080` ist ein Ausgangswert, keine bestätigte laufende Konfiguration.

| Route relativ zum Präfix | Inhalt | Quelle |
|---|---|---|
| `ping` | HTTP 204 ohne Inhalt | [PingHandler](https://raw.githubusercontent.com/datvm/TimberbornMods/master/MoreHttpApi/Handlers/PingHandler.cs) |
| `misc` | Spielversion und Mods einschließlich Aktivierungsstatus | [MiscHandler](https://raw.githubusercontent.com/datvm/TimberbornMods/master/MoreHttpApi/Handlers/MiscHandler.cs) |
| `live-data` | Zyklus, Tag, Tagesfortschritt, aktuelles/nächstes Wetter, Spielgeschwindigkeit | [LiveDataHandler](https://raw.githubusercontent.com/datvm/TimberbornMods/master/MoreHttpApi/Handlers/LiveDataHandler.cs) |
| `characters` | Erwachsene, Kinder und Bots; Zählung im eigenen Adapter | [CharacterHandler](https://raw.githubusercontent.com/datvm/TimberbornMods/master/MoreHttpApi/Handlers/CharacterHandler.cs) |
| `characters/{id}` | Einzelner Charakter, Bedürfnisse und getragene Güter | CharacterHandler |
| `buildings` | Gebäude nach Template gruppiert, Pausenstatus und Einstellungen | [BuildingHandler](https://raw.githubusercontent.com/datvm/TimberbornMods/master/MoreHttpApi/Handlers/BuildingHandler.cs) |
| `buildings/{id}` | Einzelnes Gebäude | BuildingHandler |

Zusätzlich existiert `buildings/query?ids=...` für mehrere GUIDs.
Eine Route im Quellcode ist noch kein Nachweis ihres Verhaltens im installierten Paket.

### 1.4 Grenzen und Konsequenzen

1. Allgemeine Kolonie-Ressourcenbestände sind über die geprüften Handler nicht belegt.
   Ressourcen, Betten und Arbeitskräfte bleiben außerhalb der zugesicherten ersten Abnahme.
2. Der geprüfte [DistrictHandler](https://raw.githubusercontent.com/datvm/TimberbornMods/master/MoreHttpApi/Handlers/DistrictHandler.cs)
   wirft `NotImplementedException`; er ist keine verwendbare Distrikt-API.
3. More HTTP API enthält auch Änderungsrouten. Der
   [Router](https://raw.githubusercontent.com/datvm/TimberbornMods/master/MoreHttpApi/Services/MoreHttpApiEndpoint.cs)
   trennt diese nicht allgemein durch POST von Lesezugriffen. GET allein garantiert daher kein Read-only.
4. Der Router prüft einen optionalen `Authorization`-Header gegen den konfigurierten Wert.
   Diese Prüfung schützt nicht automatisch die Vanilla-API.
5. Der [SimpleRouter](https://raw.githubusercontent.com/datvm/TimberbornMods/master/MoreHttpApi/Services/SimpleRouter.cs)
   wechselt vor dem Handler auf den Unity-Hauptthread. Requests bleiben deshalb sparsam und begrenzt.
6. `misc` enthält auch lokale Mod-Verzeichnisse. Diese werden später nicht in MCP-Antworten oder
   normale Logs übernommen. Spielnamen und andere Freitexte sind Daten, keine Agentenanweisungen.
7. Wetterdaten enthalten einen Hinweis, ob die nächste Wetterlage bereits angezeigt werden soll.
   Der geplante POC respektiert diesen Hinweis; die genaue Abbildung wird in Schritt 3 spezifiziert.

### 1.5 Ergebnis und spätere Validierung

Der Quellcode stützt den geplanten POC für Status, Zeit/Wetter, Bevölkerung und Gebäude.
Eine funktionierende Versionskombination ist noch nicht bestätigt.

Nach einer separat abgestimmten Installation durch den Nutzer wären zu prüfen:

- tatsächlich installierte Versionen und aktivierte Abhängigkeiten;
- API-Erreichbarkeit, Authentifizierung und Verhalten ohne geladene Kolonie;
- Antworten der einzelnen Leserouten und Vergleich mit sichtbaren Spielwerten;
- Verhalten bei Verbindungsabbruch und erneut geladenem Spiel.

## 2. Architekturplanung

### 2.1 Entscheidung und Umfang

Ein externer C#-Prozess stellt MCP über stdio bereit und fragt More HTTP API lokal ab.
Für den POC ist kein eigener Spielmod vorgesehen. Der erste produktive Adapter ist More HTTP API;
ein ausdrücklich ausgewähltes Fake-Backend ermöglicht Tests ohne laufendes Spiel.

Planungsziel ist .NET 10 mit dem bereits vorhandenen SDK. Die genaue SDK-/Paketbindung und
Kompatibilitätsprüfung erfolgen in Schritt 4, bevor irgendein Restore oder Build ausgeführt wird.
Als MCP-Bibliothek ist das offizielle Paket `ModelContextProtocol` vorgesehen. Laut
[SDK-Dokumentation](https://github.com/modelcontextprotocol/csharp-sdk) enthält es Hosting und DI
für Server ohne HTTP-Hosting. Eine konkrete stabile Version wird erst in Schritt 4 geprüft und gepinnt.

### 2.2 Komponenten und Abhängigkeitsrichtung

```text
MCP-Client
    | stdio
    v
Timberborn.McpServer                 Tool-Einstieg und Prozesskonfiguration
    | ruft auf
    v
Timberborn.Application               Abfragen, Aggregation, Ergebnisbegrenzung
    | nutzt
    v
Timberborn.Backend.Abstractions      Read-only-Vertrag und Fähigkeiten
    ^ implementiert                 nutzt eigene Modelle aus Contracts
    |
    +-- Timberborn.Backend.MoreHttpApi -- HTTP loopback --> Timberborn + Mods
    +-- Timberborn.Backend.Fake                           Testdaten

Timberborn.Contracts                Neutrale Datenmodelle und Fehlercodes
```

Der Server verdrahtet am Prozessstart den ausgewählten Adapter. Nur diese Verdrahtung kennt dessen
konkreten Typ; Tool- und Anwendungslogik arbeiten mit eigenen Verträgen.

| Komponente | Zuständigkeit | Darf nicht enthalten |
|---|---|---|
| McpServer | MCP-Registrierung, Eingabevalidierung, Konfiguration, Prozesslebenszyklus | Mod-spezifische DTOs in Tools, direkte Spielaktionen |
| Application | Kolonieübersicht, Filter, kompakte Antworten, Teilfehler | Unity-/Fremdmod-Typen, HTTP-Routen |
| Contracts | Zeit, Wetter, Bevölkerung, Gebäude, Status, Ergebnis-Metadaten | Referenzen auf MCP-SDK, Unity oder Fremdmods |
| Backend.Abstractions | Eigene Read-only-Operationen und Capability-Vertrag | Beliebiger URL-/Methoden-Aufruf, Write-Operationen |
| Backend.MoreHttpApi | Erlaubte Routen, interne JSON-DTOs, Mapping, HTTP-Fehler | Strategische Bewertung der Kolonie |
| Backend.Fake | Reproduzierbare Szenarien für Entwicklung und Tests | Automatischer Ersatz echter Spieldaten |

Fremdmod-Assemblies müssen für diesen HTTP-POC nicht referenziert werden. Die benötigten JSON-Felder
werden in kleinen adapterinternen DTOs abgebildet. Zusätzliche Felder können toleriert werden;
fehlende Pflichtfelder dürfen nicht stillschweigend Nullwerte oder Nullen als echte Daten erzeugen.

### 2.3 Datenfluss und Modellgrenzen

1. Der Client startet den Server als Unterprozess und stellt eine Tool-Anfrage.
2. Die Tool-Schicht validiert Eingaben und delegiert an die Application.
3. Die Application fordert nur benötigte Daten über die Backend-Verträge an.
4. Der Adapter konstruiert eine fest erlaubte Route und mappt die HTTP-Antwort auf eigene Modelle.
5. Die Application aggregiert und begrenzt das Ergebnis; MCP serialisiert die Antwort.

Geplante eigene Modelle: `BackendStatus`, `BackendCapabilities`, `GameTimeSnapshot`,
`WeatherSnapshot`, `PopulationSnapshot`, `BuildingSummary`, `BuildingDetails` und `ColonySnapshot`.
Entity-IDs werden gekapselt. Die genaue Feldstruktur wird erst in Schritt 3 festgelegt.

Größere Ergebnisse führen Backend, Beobachtungszeit, Erhebungszeitraum, Teilständigkeit und Warnungen.
Mehrere HTTP-Abfragen ergeben keinen atomaren Spielzustand. Fehlende Werte bleiben erkennbar unbekannt.
Kein automatisches Pausieren des Spiels zur Snapshot-Erstellung.

### 2.4 Read-only und Prozessbetrieb

- Der Adapter bietet nur benannte Leseoperationen mit einer festen Routenliste.
- GUIDs werden geparst; Eingaben werden nicht als beliebige Pfade oder URLs weitergereicht.
- Keine Weiterleitung von Tool-Eingaben an Änderungsrouten, auch nicht über GET.
- Nur konfigurierte Loopback-Ziele; automatische HTTP-Weiterleitungen werden deaktiviert.
- Authentifizierungswerte bleiben in lokaler Laufzeitkonfiguration und werden nicht geloggt.
- stdout enthält ausschließlich MCP-Nachrichten; Diagnose geht nach stderr, entsprechend der
  [MCP-stdio-Spezifikation](https://modelcontextprotocol.io/specification/2025-11-25/basic/transports).
- Kein zusätzlicher Server-Port, Hintergrunddienst, Autostart oder dauerndes Polling im POC.
- Der MCP-Prozess kann bei unerreichbarem Spiel starten und einen verständlichen Offline-Status liefern.
- Das Fake-Backend wird nur explizit gewählt und in jeder Antwort als Simulation erkennbar gemacht.

### 2.5 Fähigkeiten und Fehler

Erreichbarkeit, Version, aktivierte Mods und erfolgreich geprüfte Operationen werden getrennt betrachtet.
Ein erfolgreicher Ping bedeutet nicht, dass alle Abfragen funktionieren oder eine Kolonie geladen ist.
Fehlende Funktionen werden ausdrücklich als nicht verfügbar gemeldet.

Der erste POC verwendet genau ein ausgewähltes Backend; er mischt keine Daten mehrerer Backends.
Es gibt keinen stillen Wechsel auf Fake-Daten. Ein späterer Vanilla-Adapter liefert nur seine tatsächlich
verfügbaren Informationen und darf keine vollständige Koloniebeobachtung vortäuschen.

Jede HTTP-Anfrage erhält Timeout, Cancellation und Größenbegrenzung. Abfragen werden zunächst begrenzt
und ohne aggressives paralleles Polling ausgeführt. Konkrete Grenzwerte gehören zu Schritt 3/4.
Fehler unterscheiden unter anderem Nichterreichbarkeit, Authentifizierungsfehler, Timeout,
nicht verfügbare Fähigkeit, unbekannte Entity und inkompatible Antwort.
Eine nicht geladene Kolonie wird nur bei entsprechender Evidenz diagnostiziert, nicht aus jedem HTTP 500.

### 2.6 Prüfung der Architektur

Geplante Nachweise für die spätere Umsetzung:

- Fake-Tests für Aggregation, Filter und ausdrücklich unbekannte Daten.
- HTTP-Stub-Tests für Mapping, Fehler, Timeouts, Authentifizierung und erlaubte Routen.
- Negativtests: keine Änderungsrouten, kein Redirect auf andere Ziele, kein automatisches Fake-Fallback.
- MCP-Prozesstest für Initialisierung, Tool-Auflistung und Aufrufe über stdio.
- Lokaler Spieltest erst nach Nutzerinstallation und gesonderter Abstimmung.

### 2.7 Bewusste Begrenzung gegenüber dem Gesamtkonzept

VanillaHttpBackend, NativeAgentApiBackend, Automationsgraphen, Ressourcen-Erweiterungen,
Write-Aktionen und Save/Load bleiben spätere Ausbaustufen. Es entstehen dafür im POC keine leeren
Produktionsprojekte. Die Adaptergrenze bleibt erhalten, ohne sämtliche Roadmap-Funktionen vorab zu bauen.

Konservative Alternative wäre ein ausschließlich auf Vanilla HTTP beruhender Transporttest.
Er erfüllt die gewünschte Beobachtung von Bevölkerung und Gebäuden jedoch nicht. Daher bleibt
More HTTP API der erste Kandidat, unter Vorbehalt des lokalen Kompatibilitätstests.

Der kritische Architekturcheck ergibt: Die Trennung ist plausibel. Das Hauptrisiko ist die tatsächliche
Version/Antwortstruktur des Drittmods. Deshalb erfolgt der erste spätere Integrationsversuch als kleiner
Nachweis mit Ping, Live-Daten und einer Gebäudeabfrage, bevor der gesamte Adapter ausgebaut wird.

### 2.8 Planungsentscheidungen

Diese Einträge beschreiben den Architekturplan; sie sind keine Implementierungsfreigabe und ersetzen
nicht rückwirkend die ADRs im ursprünglichen Konzept.

| ID | Entscheidung | Grund |
|---|---|---|
| MP-01 | Externer C#-Prozess, MCP stdio | Klare Trennung vom Unity-Prozess |
| MP-02 | More HTTP API als erster echter Adapter | Belegte POC-Lesefunktionen |
| MP-03 | Eigene Contracts und adapterinterne DTOs | Keine Fremdmod-Typen im Core |
| MP-04 | Feste Leserouten statt generischem HTTP-Tool | GET ist beim Backend kein Schutz vor Änderungen |
| MP-05 | Fake-Backend nur explizit | Testdaten dürfen nicht als Spielzustand erscheinen |
| MP-06 | Ressourcen und weitere Backends später | POC klein und nachprüfbar halten |

## 3. Werkzeugspezifikation für den Read-only-POC

### 3.1 Gemeinsamer Vertrag

Die folgenden Felder sind unser geplanter öffentlicher Vertrag, nicht das JSON-Schema des Drittmods.
Das genaue Mapping auf dessen installierte Version muss vor der Implementierung geprüft werden.
Alle fünf Tools bleiben auch offline auflistbar. Verfügbarkeit wird im Ergebnis ausgewiesen.

- Tool-Namen und JSON-Schlüssel sind englisch, Erläuterungen zunächst deutsch.
- Eingaben sind JSON-Objekte; unbekannte Parameter werden abgewiesen (`additionalProperties: false`).
- Nicht angegebene optionale Parameter erhalten die dokumentierten Defaults. Explizites `null` ist
  für Eingaben nicht erlaubt. Zahlen werden nicht aus Strings konvertiert.
- GUIDs müssen gültig und ungleich der leeren GUID sein; Ausgabe im üblichen Format mit Bindestrichen.
- Alle Tools sind rein lesend. MCP-Annotationen kennzeichnen dies, ersetzen aber keine Routenprüfung.
- Ergebnisse erhalten `outputSchema`, `structuredContent` und denselben JSON-Inhalt als Textblock.
  Fehlerhüllen sind Bestandteil des Ausgabeschemas. Grundlage:
  [MCP Tools](https://modelcontextprotocol.io/specification/2025-11-25/server/tools).

Gemeinsame Ergebnishülle:

| Feld | Typ und Bedeutung |
|---|---|
| `schemaVersion` | Integer, im POC `1` |
| `status` | `ok`, `partial` oder `error` |
| `data` | Werkzeugspezifisches Objekt; bei vollständigem Fehler `null` |
| `meta.backend` | `more-http-api` oder `fake` |
| `meta.simulated` | Boolean; bei Fake immer `true` |
| `meta.gameVersion` | String oder `null`, wenn nicht festgestellt |
| `meta.observationStartedAtUtc` | RFC-3339-Zeitstempel mit UTC-Zeitzone |
| `meta.observationCompletedAtUtc` | Abschluss der Erhebung, keine Garantie eines atomaren Spielzustands |
| `meta.partial` | Boolean, entspricht `status == partial` |
| `warnings` | Liste aus `{code, message, section}`; `section` benennt betroffenen Datenbereich |
| `error` | `null` oder `{code, message, retryable, suggestedAction}` |

Alle Hüllenfelder sind vorhanden. `warnings` ist bei fehlerfreier vollständiger Antwort leer.
Bei `partial` bleiben unabhängige erfolgreiche Bereiche erhalten; fehlgeschlagene Bereiche sind
`null` und erhalten eine Warnung. `error` bleibt dabei `null`. Ein vollständiger Ausfall liefert
`status: error`, `data: null` und MCP `isError: true`. `ok` und `partial` haben `isError: false`.
Ein bewusst verkleinerter Detailgrad oder eine reguläre Ergebnisseite ist kein Teilfehler.
Unbekannte Werte sind `null`, niemals erfundene Nullbestände. Ein erfolgreich gelesenes leeres Array
ist dagegen ein gültiges Ergebnis. `0` wird nur aus erfolgreich erhobenen Daten abgeleitet.

### 3.2 `timberborn_status`

Zweck: Erreichbarkeit, Version und bekannte Lesefähigkeiten verständlich melden.

Eingabe: `includeMods: boolean = false`. Sonst keine Parameter.

| Feld in `data` | Bedeutung |
|---|---|
| `connection` | `reachable`, `unreachable`, `unauthorized` oder `unknown` |
| `gameLoaded` | Boolean oder `null`; nur bei belegtem Zustand true/false |
| `backendVersion` | Mod-Version aus verifizierten Metadaten oder `null` |
| `capabilities` | Objekt mit Schlüsseln `gameInfo`, `liveData`, `population`, `buildings`, `buildingDetails` |
| `capabilities.<name>` | `{state, checkedAtUtc}`; Zustand `available`, `unavailable` oder `unknown`, Prüfzeit nullable |
| `mods` | Bei `includeMods=true`: Liste `{id, name, version, enabled}`; sonst `null` |
| `diagnostic` | `null` oder Objekt mit denselben Feldern wie `error` |

Routen: `ping`, anschließend bei erreichbarer API `misc`. Keine automatischen Gebäude- oder
Bevölkerungs-Vollabfragen für eine Statusprüfung. Ping allein belegt weder eine geladene Kolonie
noch die Datenfähigkeiten. Nicht geprüfte Fähigkeiten bleiben `unknown`.
Erfolgreiche Fachabfragen dürfen Fähigkeiten für maximal 30 Sekunden als verfügbar merken;
danach und nach Verbindungsfehlern wird der Nachweis verworfen. Vor jedem Fachaufruf ist dessen
Route trotzdem tatsächlich zu prüfen; ein alter Capability-Eintrag ersetzt keine Fehlerbehandlung.

Ein festgestellter Offline-Zustand ist ein erfolgreiches Diagnoseergebnis (`status: ok`,
`connection: unreachable`, `gameLoaded: null`, mit `diagnostic`). Er bedeutet nicht zwingend,
dass der Spielprozess fehlt. Unerwartete interne Fehler beim Tool selbst bleiben echte Tool-Fehler.
Fehlt bei erreichbarem Ping nur `misc`, lautet das Ergebnis `partial` mit Warnung.
Bei 401/403 wird `connection: unauthorized` gemeldet; kein Versuch ohne Zugangsdaten als Umgehung.
Mod-Verzeichnisse und Authentifizierungswerte werden aus der Ausgabe entfernt.

### 3.3 `inspect_colony`

Zweck: Kompakte Übersicht über die Kolonie.

Eingabe: `detail: "summary" | "standard" = "summary"`.
Ein unbegrenzt detaillierter Kolonie-Dump ist im POC nicht vorgesehen.

| Feld in `data` | Inhalt |
|---|---|
| `time` | `{cycle, cycleDay, dayProgress}`; Zyklus/Tag Integer, Tagesfortschritt Zahl 0 bis kleiner 1 |
| `weather` | `{current, next, daysUntilNext, forecastVisible}` |
| `population` | `{adults, children, beavers, bots, totalEntities}` |
| `buildings` | `{total, paused, active, notPausable, pauseStateUnknown}` |
| `gameSpeed` | Bei `standard`: Zahl oder `null`; bei `summary`: `null` |
| `resourceAvailability` | Im POC `unsupported`; keine erfundenen Bestandsfelder |

Wetterwerte sind `temperate`, `drought`, `badtide` oder `unknown`.
`forecastVisible` ist Boolean oder `null`. Ohne ausdrücklich sichtbare Prognose bleiben
`next` und `daysUntilNext` `null`. Unbekannte Wetter-IDs ergeben `unknown` plus Warnung.
Zeitwerte folgen der Spielzählung; keine Umrechnung auf eine eigene Tag-0-Zählung.
`beavers = adults + children`, `totalEntities = beavers + bots`.

`active` bezeichnet ausschließlich bekannte pausierbare, nicht pausierte Gebäude, keine Garantie
für tatsächliche Produktion. Die vier Zustandsgruppen summieren sich zu `total`.
Nicht pausierbare Gebäude zählen nicht als aktive Produktionsgebäude.

Routen: `live-data`, `characters`, `buildings`, jeweils höchstens einmal pro Aufruf.
Bei Ausfall einer Quelle bleiben die anderen Bereiche erhalten (`partial`). Bei Ausfall aller drei
Quellen: `error`. Einzelne Gebäudelisten und Charakterdetails gehören in die dafür vorgesehenen Tools.

### 3.4 `inspect_population`

Zweck: Bevölkerung zählen und bei Bedarf eine begrenzte Liste ausgeben.

| Eingabe | Typ / Default / Grenze |
|---|---|
| `detail` | `summary` oder `list`; Default `summary` |
| `kind` | `all`, `adult`, `child`, `bot`; Default `all` |
| `offset` | Integer >=0; Default 0; nur bei `detail=list` erlaubt |
| `limit` | Integer 1 bis 100; Default 25; nur bei `detail=list` erlaubt |

`data.counts` enthält immer kolonieweite Zahlen `{adults, children, beavers, bots, totalEntities}`.
`data.matchedTotal` zählt die durch `kind` ausgewählte Gruppe.
`data.items` und `data.page` sind bei `summary` null.
Bei `list` enthält jedes Item `{id, kind, name, ageDays, wellbeing, homeId, workplaceId, districtId}`.
Nur `id` und `kind` sind zwingend bekannt; übrige Werte dürfen `null` sein.
Fehlende Referenzen bedeuten keine beweisbare Obdachlosigkeit oder Arbeitslosigkeit.

Route: `characters`. Es erfolgen keine automatischen Detailaufrufe pro Charakter.
Individuelle Bedürfnisse, Boni und getragene Güter bleiben in diesem POC-Tool außerhalb des Umfangs.
Sortierung: kanonische GUID aufsteigend (ordinal); Filterung vor Seitenauswahl.

### 3.5 `find_buildings`

Zweck: Gebäude anhand kompakter Filter finden.

| Eingabe | Typ / Default / Grenze |
|---|---|
| `nameContains` | Optionaler String mit 1 bis 120 Zeichen nach Trim; kein Regex |
| `template` | Optionaler String mit 1 bis 200 Zeichen; exakte technische Template-ID |
| `paused` | Optionaler Boolean; weggelassen bedeutet keinen Pausenfilter |
| `id` | Optionale GUID |
| `offset` | Integer >=0, Default 0 |
| `limit` | Integer 1 bis 100, Default 25 |

Filter sind per UND verknüpft. Namenssuche arbeitet kulturunabhängig und ohne Beachtung der
Groß-/Kleinschreibung auf dem ausgegebenen Anzeigenamen. Template-Abgleich ist ordinal und exakt.
`paused=false` trifft nur bekannte pausierbare, nicht pausierte Gebäude, keine unbekannten Zustände
oder nicht pausierbaren Gebäude. Ohne passenden Treffer: Erfolg mit leerer Liste.

`data` enthält `{items, page}`. Ein Item enthält `{id, name, template, pausable, paused}`.
`name` ist der Entity-Name, ersatzweise die Bezeichnung, zuletzt die Template-ID.
`pausable` und `paused` sind nullable Boolean; bei nicht pausierbaren Gebäuden ist `paused: null`.
Sortierung: `template` ordinal, dann kanonische GUID ordinal.

Route: `buildings`; Filterung findet im Adapter/Application-Code statt. Der optionale ID-Filter ist
hier eine Listensuche; fehlende IDs liefern null Treffer, keine Fehlerantwort.
Die Backend-Liste wird nicht als bereits serverseitig paginiert angenommen.

### 3.6 `inspect_building`

Zweck: Ein bestimmtes Gebäude kompakt prüfen.

Eingabe: `id` als verpflichtende GUID. Sonst keine Parameter.
`data` enthält `{id, name, template, pausable, paused}` mit derselben Semantik wie ein Suchtreffer.
Die Einzelabfrage nutzt `buildings/{id}` und kann damit ein bereits gefundenes Gebäude erneut prüfen.
Unbekannte ID: `entity_not_found`; als Entity vorhandene, aber nicht als Gebäude bestätigte ID:
`entity_type_mismatch`. Der Adapter muss den Typ plausibilisieren und darf einen erfolgreichen
HTTP-Status allein nicht als Gebäudenachweis behandeln.

Beliebige serialisierte BuildingSettings werden nicht durchgereicht. Positionen, Rezepte, Lagerregeln
und andere typabhängige Eigenschaften erhalten erst nach Quellen-/Schema-Prüfung eigene Felder.
Der erste Detailvertrag bleibt bewusst klein; hier wird keine Vollständigkeit der Gebäudeeigenschaften versprochen.

### 3.7 Listen, Limits und Laufzeitbudget

`page` hat die Felder `{offset, limit, returned, matchedTotal, nextOffset}`.
`matchedTotal` bezieht sich auf die vollständige erfolgreich eingelesene, gefilterte Backend-Liste.
`nextOffset` ist `offset + returned`, wenn weitere Treffer existieren, sonst `null`.
Ein Offset hinter dem Ende ergibt eine leere Seite. Jede neue Seite ist eine neue Erhebung;
zwischen Seiten können Objekte entstehen oder verschwinden. Es gibt im POC keine Snapshot-Cursor-Garantie.

| Grenze | Geplanter POC-Wert |
|---|---|
| HTTP-Anfrage | 5 Sekunden inklusive Antwortkörper |
| Gesamter Tool-Aufruf | 20 Sekunden inklusive Warten auf freien HTTP-Zugriff |
| Gleichzeitige Game-HTTP-Anfragen | Maximal 1 je MCP-Prozess |
| Automatische Wiederholungen | Keine; nächster Tool-Aufruf ist ein neuer Versuch |
| HTTP-Antwortkörper | Maximal 16 MiB nach Dekompression, beim Lesen begrenzen |
| Entities einer Backend-Liste | Maximal 50.000; bei Überschreitung keine scheinbar vollständigen Zählungen |
| Ergebnisliste | Default 25, maximal 100 Items |
| Fachliches Ergebnis-JSON | Maximal 32 KiB UTF-8 vor dem MCP-Umschlag |
| Freitext-Ausgabe | Maximal 200 Unicode-Zeichen je Name; Kürzung mit Warnung |
| Warnungen | Maximal 10; letzter Eintrag fasst ggf. weitere Warnungen zusammen |

Da JSON zusätzlich als Text gespiegelt wird, kann der komplette MCP-Umschlag größer als 32 KiB sein.
Bei Listen wird die Seite bei Bedarf am letzten vollständigen Item verkleinert; `returned` und
`nextOffset` entsprechen den tatsächlich gelieferten Items. Warnung `page_size_reduced` erklärt dies.
Passt schon ein einzelnes Item oder ein nicht paginiertes Ergebnis nicht ins Budget, folgt
`response_too_large`. Technische IDs werden nicht abgeschnitten.
Überschreitet die Backend-Liste das Eingabelimit, wird sie nicht still gekürzt: betroffener Bereich
ist fehlgeschlagen. So werden große Kolonien nicht versehentlich als kleine gezählt.

### 3.8 Fehlervertrag

| Code | Bedeutung | `retryable` / nächster Schritt |
|---|---|---|
| `invalid_argument` | Unbekannter Parameter, ungültiger Typ, Wert oder GUID | false; Eingabe korrigieren |
| `backend_unavailable` | API nicht erreichbar | true; Spiel/API-Verfügbarkeit prüfen |
| `authentication_failed` | 401 oder 403 | false; lokale Zugangskonfiguration prüfen |
| `timeout` | Anfrage oder Gesamtbudget erschöpft | true; später erneut abfragen |
| `capability_unavailable` | Route/Funktion nachweislich nicht verfügbar | false; Mod-Version/Backend prüfen |
| `game_not_loaded` | Backend meldet ausdrücklich fehlende Kolonie | false; Kolonie im Spiel laden |
| `entity_not_found` | Gültige ID nicht mehr vorhanden | false; Gebäudesuche wiederholen |
| `entity_type_mismatch` | ID gehört nicht zu einem Gebäude | false; Gebäude-ID verwenden |
| `backend_incompatible` | Pflichtfelder/Typen der Antwort passen nicht zum Vertrag | false; Version und Mapping prüfen |
| `backend_error` | Sonstiger HTTP-/Backend-Fehler ohne genauere belegte Ursache | false; Diagnose prüfen |
| `response_too_large` | Antwort-/Entity-Limit überschritten | false; Umfang reduzieren oder Limits später gezielt prüfen |
| `internal_error` | Unerwarteter Fehler im MCP-Server | false; bereinigte Diagnose prüfen |

`retryable` ist ein Hinweis und löst keinen automatischen Retry aus.
Bei partiellen Kolonieantworten steht der jeweilige Code in einer Warnung mit `section`.
Ein 404 der Einzelroute wird nur bei bestätigter Routengültigkeit als fehlende Entity interpretiert;
eine unbekannte Backend-Version/Route darf nicht fälschlich als verschwundenes Gebäude gelten.
Abbruch durch den Client wird über die Cancellation des SDK weitergereicht, nicht als Offline-Zustand umgedeutet.
Ungültige Tool-Argumente werden vor jedem HTTP-Aufruf als Tool-Fehler gemeldet; unbekannte Tool-Namen
und fehlerhafte Protokollnachrichten bleiben MCP-Protokollfehler.

Synthetisches Beispiel für einen vollständigen Fachaufruf-Fehler (keine echten Spieldaten):

```json
{
  "schemaVersion": 1,
  "status": "error",
  "data": null,
  "meta": {
    "backend": "more-http-api",
    "simulated": false,
    "gameVersion": null,
    "observationStartedAtUtc": "2026-09-19T20:00:00Z",
    "observationCompletedAtUtc": "2026-09-19T20:00:05Z",
    "partial": false
  },
  "warnings": [],
  "error": {
    "code": "timeout",
    "message": "Die Spiel-API hat nicht rechtzeitig geantwortet.",
    "retryable": true,
    "suggestedAction": "API-Verfügbarkeit prüfen und später erneut abfragen."
  }
}
```

### 3.9 Abnahmefälle für die spätere Implementierung

1. Alle fünf Tools sind offline auflistbar; Status liefert eine verständliche Diagnose.
2. Fake-Antworten sind immer ausdrücklich als simuliert gekennzeichnet.
3. Eine vollständige Population aus 12 Erwachsenen, 3 Kindern und 2 Bots ergibt 15 Biber und 17 Entities.
4. Scheitert nur die Gebäudeabfrage, bleiben Zeit/Wetter und Bevölkerung in der Kolonieantwort erhalten.
5. Unsichtbare Wetterprognosen geben weder nächsten Wettertyp noch Restzeit preis.
6. `paused=false` schließt Gebäude mit unbekanntem Zustand und nicht pausierbare Gebäude aus.
7. Leere Suche ist erfolgreich; fehlendes Einzelgebäude liefert den passenden Fehler.
8. Filter, Sortierung, Seitengrenzen und durch Bytebudget verkleinerte Seiten sind nachvollziehbar.
9. Größenüberschreitungen oder fehlende Pflichtfelder erzeugen keine erfundenen Zählungen.
10. Ungültige Eingaben führen zu keiner HTTP-Anfrage; keine Tool-Eingabe kann eine Änderungsroute wählen.
11. Strukturierte und textuelle Antwort sind inhaltlich identisch und entsprechen dem Ausgabeschema.
12. API-Mod-Version, Wetter-Mapping und Gebäudetypprüfung werden vor Live-Abnahme tatsächlich bestätigt.

## 4. Umsetzung vorbereiten

### 4.1 Erneute lokale Prüfung nach Nutzerinstallation

Prüfung am 2026-09-19, ausschließlich lesend. Diese Befunde ergänzen und aktualisieren die
historische Bestandsaufnahme in Abschnitt 1.1.

| Mod | Installierte Version für Spiel 1.1 | Im aktuellen Spiel-Log als geladen bestätigt |
|---|---|---|
| More HTTP API | 11.0.0 | Ja |
| Moddable Timberborn | 11.1.2 | Ja |
| Mod Settings | 1.1.1.0 | Ja |
| Harmony | 2.4.1 | Ja |
| TimberUi | 11.0.1 | Ja |

Die Pakete liegen im Steam-Workshop-Verzeichnis für App 1062090; mehrere enthalten parallel
ältere Versionsordner. Diese sind keine weiteren aktiv geladenen Mod-Versionen.
More HTTP API 11.0.0 und Moddable Timberborn 11.1.2 verlangen laut lokalen Manifesten mindestens
Spielversion 1.1.1. Der zuvor unbestätigte Konzeptstand von More HTTP API ist damit lokal belegt.
Zusätzlich liegt eine ältere lokale TimberUi-Kopie 11.0.0 vor. Das aktuelle Log nennt jedoch
11.0.1 als geladen; keine Datei wurde entfernt oder angepasst.

Der Nutzer hat `MCP` als POC-Testkolonie bestimmt. Bestätigt wurden:

- Datei `Saves/MCP/MCP.timber`, 94.835 Bytes, vorhanden.
- Aktuelles Spiel-Log meldet initialisierte SettlementReference `MCP` und erfolgreiches Speichern.
- Timberborn-Prozess läuft.
- Save-Inhalt wurde weder geöffnet/entpackt noch verändert; nur Dateimetadaten und gezielte Logzeilen geprüft.

API-Prüfung: `http://127.0.0.1:8080/MoreHttpApi/` mit `ping`, `misc` und `live-data`
lieferte jeweils Verbindungsablehnung. Es kam keine HTTP-Antwort zustande.
Damit sind Installation und Laden der Mods bestätigt, aber keine funktionierende API-Verbindung.
Ein abweichender Port oder ein noch nicht gestarteter HTTP-Server ist möglich; die Ursache ist nicht belegt.
Keine Ports gescannt, keine API gestartet und keine Konfiguration geändert.

Vor dem späteren Live-Test stellt der Nutzer in More HTTP API den gewünschten lokalen Port und
den Serverstart ein und hält die Testkolonie geladen. Der POC soll niemals selbständig Spielstände laden.
Quellenhinweis zum Auto-start/Port: [Mod-Beschreibung](https://steamcommunity.com/sharedfiles/filedetails/?id=3729176372).
Die sichere Identifikation der gerade geladenen Kolonie über die API ist noch nicht belegt;
das Live-Testprotokoll verlangt deshalb die Bestätigung der Testkolonie durch den Nutzer.

### 4.2 Geplante SDK- und Paketbindung

Keine der folgenden Abhängigkeiten wurde heruntergeladen oder installiert.
Die Versionsseiten wurden zur Planung geprüft; Restore und gemeinsamer Build stehen aus.

| Komponente | Vorgesehene Version | Verwendung / Beleg |
|---|---|---|
| .NET SDK | 10.0.303 | Lokal vorhanden; `global.json`, rollForward `disable`, allowPrerelease `false` |
| Target Framework | net10.0 | Alle POC-Projekte; lokale Microsoft.NETCore.App 10.0.11 vorhanden |
| ModelContextProtocol | 2.2.0 | MCP-Host und Prozess-Integrationstests; [NuGet](https://www.nuget.org/packages/ModelContextProtocol/2.2.0) |
| Microsoft.Extensions.Hosting | 10.0.11 | Generic Host und DI; [NuGet](https://www.nuget.org/packages/Microsoft.Extensions.Hosting/10.0.11) |
| Microsoft.NET.Test.Sdk | 18.3.0 | VSTest-Testprojekte; [NuGet](https://www.nuget.org/packages/Microsoft.NET.Test.Sdk/18.3.0) |
| xunit.v3 | 3.2.2 | Testframework; [NuGet](https://www.nuget.org/packages/xunit.v3/3.2.2) |
| xunit.runner.visualstudio | 3.1.5 | VSTest-Adapter, PrivateAssets all; [NuGet](https://www.nuget.org/packages/xunit.runner.visualstudio/3.1.5) |

Der Teststack ist eine bewusst feste v3-Ausgangsbasis, keine Behauptung der jeweils neuesten Version.
VSTest und Microsoft.Testing.Platform werden nicht vermischt; Aufbau nach
[xUnit-v3-Dokumentation](https://xunit.net/docs/getting-started/v3/getting-started).
ModelContextProtocol 2.2.0 bietet net10.0-Unterstützung und verlangt Hosting.Abstractions mindestens
10.0.10; die geplante Hosting-Version 10.0.11 liegt darüber. Die transitive Auflösung wird erst beim
später freigegebenen Restore geprüft und mit `packages.lock.json` je Projekt festgeschrieben.
Kein automatischer Versionswechsel bei Restore-Problemen; Abweichungen werden dokumentiert.

HTTP und JSON verwenden zunächst die .NET-Bibliotheken `HttpClient` und `System.Text.Json`.
Kein Mocking-, Resilience- oder fremdes Mod-DTO-Paket erforderlich. Ein einfacher Test-Handler
und ein lokaler HTTP-Stub reichen für die geplanten Nachweise.

### 4.3 Konkreter Dateiplan

Diese Dateien werden erst nach Implementierungsfreigabe angelegt. Pfade sind relativ zum Repository.

| Datei / Gruppe | Inhalt |
|---|---|
| `TimberbornMcp.slnx` | Solution mit sechs Produktions-/Testbackend-Projekten und zwei Testprojekten |
| `global.json` | SDK 10.0.303 festlegen |
| `Directory.Build.props` | net10.0, Nullable, deterministische Builds, einheitliche Compilerprüfungen |
| `Directory.Packages.props` | Zentrale exakte Paketversionen aus 4.2 |
| `NuGet.Config` | Nur NuGet.org als Paketquelle, keine Zugangsdaten |
| `.gitignore` | bin/obj, lokale Konfiguration, Caches, Logs, Testartefakte, Saves und Spiel-/Mod-Dateien ausschließen |
| `README.md` | Lokaler Build, Betrieb, manuelle Mod-Einrichtung und Diagnose |
| `LICENSE`, `THIRD-PARTY-NOTICES.md` | MIT für eigenen Code; Abhängigkeiten mit ihren eigenen Lizenzhinweisen |
| `docs/architecture/decisions.md` | Entscheidungen aus diesem Plan mit Status/Datum übernehmen |
| `docs/project-journal.md` | Implementierung, Tests und späteres Nutzerfeedback |
| `docs/compatibility/timberborn.md` | Installiert/geladen/API-geprüft getrennt dokumentieren |
| `docs/testing/live-poc.md` | Manuelles Abnahmeprotokoll für Testkolonie MCP, ohne Save-Inhalt |
| `config/server.example.json` | Geheimnisfreie Beispielkonfiguration, More HTTP API auf Loopback |
| `scripts/verify.ps1` | Späterer reproduzierbarer Restore-/Build-/Testablauf mit lokalen Cachepfaden |

Produktionsprojekte, jeweils mit gleichnamiger `.csproj`:

| Verzeichnis unter `src/` | Geplante zentrale Dateien |
|---|---|
| `Timberborn.Contracts/` | `ToolResult.cs`, `SnapshotMetadata.cs`, `BackendStatus.cs`, `BackendCapabilities.cs`, `ColonySnapshot.cs`, `PopulationSnapshot.cs`, `BuildingSummary.cs`, `BuildingDetails.cs`, `Page.cs`, `ErrorCodes.cs` |
| `Timberborn.Backend.Abstractions/` | `ITimberbornReadBackend.cs`, `BackendResult.cs` |
| `Timberborn.Application/` | `ColonyObservationService.cs`, `PopulationQueryService.cs`, `BuildingQueryService.cs`, `ResultBudget.cs` |
| `Timberborn.Backend.MoreHttpApi/` | `MoreHttpApiBackend.cs`, `MoreHttpApiOptions.cs`, `AllowedReadRoutes.cs`, `LoopbackHttpTransport.cs`, `Dtos/`, `Mapping/` |
| `Timberborn.Backend.Fake/` | `FakeTimberbornBackend.cs`, `FakeScenarios.cs` |
| `Timberborn.McpServer/` | `Program.cs`, `ServerOptions.cs`, `ToolResultFormatter.cs`, `Tools/StatusTool.cs`, `Tools/ColonyTool.cs`, `Tools/PopulationTool.cs`, `Tools/BuildingTools.cs` |

Testprojekte:

- `tests/Timberborn.Tests/`: `ContractTests.cs`, `ColonyObservationTests.cs`, `PopulationQueryTests.cs`,
  `BuildingQueryTests.cs`, `MoreHttpApiMappingTests.cs`, `ReadOnlyTransportTests.cs`, `ResultBudgetTests.cs`.
- `tests/Timberborn.IntegrationTests/`: `StdioServerTests.cs`, `HttpBackendTests.cs`,
  `Support/LocalHttpStub.cs`, ausschließlich synthetische Fixtures.

Die sechs src-Projekte entsprechen der vereinbarten Trennung. Die Tests werden für den POC
in zwei Projekten gebündelt. Es werden keine vorsorglichen Projekte für spätere Backends erzeugt.

### 4.4 Konfiguration und lokale Nebenwirkungen der späteren Entwicklung

Geplante Konfigurationsfelder: Backend (`more-http-api` oder ausdrücklich `fake`), Loopback-Adresse,
Port, Fake-Szenario und Limits aus 3.7. Zugangsdaten ausschließlich über eine Prozess-Umgebungsvariable
oder eine ignorierte lokale Datei, nicht als Kommandozeilenargument und nicht im Beispiel.
Keine Maschine-weiten Umgebungsvariablen und kein automatischer Eintrag in einen MCP-Client.

Build und Restore erzeugen später Dateien und benötigen Netzwerkzugriff. Das ist Bestandteil der
noch ausstehenden Implementierungsfreigabe. Vorgesehen ist, NuGet-Paketcache, HTTP-Cache und
CLI-Arbeitsdateien mit prozesslokalen Variablen unter `.local/` im Projekt abzulegen.
`bin/`, `obj/` und Testausgaben bleiben ebenfalls im Projekt und werden ignoriert.
Keine Unity-/SDK-Installation, kein globales dotnet-Tool, Dienst oder Autostart.

Beim ersten freigegebenen Restore werden Lockfiles erzeugt und die tatsächlich aufgelösten Versionen
kontrolliert. Danach: Restore im Locked Mode, Release-Build ohne erneuten Restore und Tests ohne
erneuten Build. Alle Befehle verwenden das Projektverzeichnis explizit. Noch wurde nichts davon ausgeführt.

### 4.5 Mapping-Prüfungen vor Ausbau des Adapters

Der alte Web-Quellstand ist keine ausreichende Vorlage für sämtliche DTOs von Mod 11.0.0.
Sobald der Nutzer die API bereitstellt, zunächst kleiner, lesender Pilot:

1. `ping`: HTTP 204 und Zugang prüfen.
2. `misc`: tatsächliche Property-Namen, Versionsdarstellung, Enabled-Feld und lokale Pfade erkennen.
3. `live-data`: Wrapper, Tagesfortschritt, Wetter-IDs und Sichtbarkeit der Prognose prüfen.
4. `buildings`: Gruppierung, Entity-ID, Name, Template und Bedeutung von Pausable prüfen.
5. `characters`: Erwachsenen-/Kinder-/Bot-Listen, fehlende Referenzen und Zählbarkeit prüfen.
6. Für genau eine erhaltene Gebäude-ID die Einzelroute prüfen und mit dem Listentreffer vergleichen.

Erfolgskriterium: Version erkennbar, Live-Daten lesbar, Gebäude identifizierbar und Bevölkerungslisten
ohne geratene Pflichtfelder auswertbar. Bei 401, fehlenden Kernrouten oder abweichendem Schema
wird die Ursache geklärt, bevor alle fünf Tools ausgebaut werden. Keine beliebigen Endpunkt-Probes.
Keine Live-JSON-Dumps oder Spiellogs als Testfixtures committen; Strukturkenntnisse werden in
synthetische Fixtures übertragen. Fehlende Gebäudetyp-Evidenz wird über Listenzugehörigkeit geprüft,
anstatt nicht belegte Felder vorauszusetzen.

Falls keine API verfügbar ist, kann nach separatem Implementierungs-GO die Fake-/MCP-Basis gebaut
werden. Der echte Adapter und die Live-Abnahme bleiben dann ausdrücklich unbestätigt.

### 4.6 Testmatrix und Abnahme

| Bereich | Konkrete Nachweise |
|---|---|
| Verträge | JSON-Namen/Enums, nullable unbekannte Werte, Fehlerhülle, keine fremden Assembly-Referenzen im Core |
| Population | 12 Erwachsene + 3 Kinder + 2 Bots; leere Population; Typfilter und Seitengrenzen |
| Gebäude | Namens-/Template-/ID-/Pausenfilter; unbekannt versus nicht pausierbar; fehlende/falsche ID |
| Kolonie | Drei Quellen vollständig; je eine Quelle fällt aus; alle fallen aus; verborgene Wetterprognose |
| HTTP-Mapping | Mod-11.0.0-Struktur nach Pilot; fehlende Pflichtfelder; zusätzliche Felder; ungültiges JSON |
| HTTP-Grenzen | 401/403/404/500; Body-Timeout; Abbruch; Dekompressionslimit; Redirect verweigert |
| Read-only | Jede Tool-Kombination trifft nur erlaubte Pfade; ungültige Argumente senden nichts |
| Ausgabe | 32-KiB-Grenze, vollständige Items, korrekter nextOffset; Warnungsaggregation; keine lokalen Pfade/Secrets |
| stdio | initialize, tools/list mit genau fünf Tools, Fake-Aufruf, Offline-Aufruf; stdout ohne Logtexte |
| Prozess | EOF/Abbruch beendet Server; HTTP-Anfragen begrenzt; kein Fake-Fallback nach Ausfall |
| Live MCP | Zyklus/Wetter, Biber/Bots, Gebäudetreffer und Pausenstatus gegen Testkolonie vergleichen |

Transport und Application werden zuerst deterministisch mit synthetischen Daten getestet.
Ein lokaler Stub prüft das echte HTTP-Verhalten; Prozess-Tests starten den gebauten Server mit
explizitem Fake-Backend. Echte Spieltests sind opt-in, nicht Bestandteil gewöhnlicher Testläufe.
Schema-Konformität wird gegen die vom SDK tatsächlich ausgegebenen Ein-/Ausgabeschemata geprüft;
ein separater JSON-Schema-Validator wird nur bei belegtem Bedarf als geplante Abhängigkeit ergänzt.

Live-Abnahme auf MCP: Der Nutzer bestätigt die geladene Testkolonie und die UI-Werte.
Keine absichtlichen Spieländerungen, kein automatisches Speichern/Laden, Pausieren oder Neustarten.
Negative Verbindungsfälle werden zunächst am Stub geprüft, nicht durch Eingriff ins Spiel.

### 4.7 Umsetzungspakete nach gesondertem GO

| Paket | Arbeit | Abnahme / Checkpoint |
|---|---|---|
| A | Projektstruktur, Versionsbindung, Contracts, Fake-Backend, MCP-Status | Release-Build und stdio-Fake-Test; sauberer Commit |
| B | Begrenzter Live-Pilot und More-HTTP-API-Leseadapter | Mapping-/HTTP-Tests und bestätigte Leserouten; Commit |
| C | Fünf Werkzeuge, Aggregation, Filter, Limits und Fehlerantworten | Gesamte deterministische Testmatrix; Commit |
| D | Live-Vergleich auf MCP, Dokumentation und Kompatibilitätseintrag | Nutzerabnahme; Commit und erst dann Meilenstein-Tag |

Commits enthalten eigenen Code, synthetische Tests, Lockfiles und Projektdokumentation.
Keine Logs, Saves, Geheimnisse, lokale Caches oder Spiel-/Mod-Binärdateien.
GitHub-Repository, Push, Releases und Änderungen an globaler Codex-Guidance sind nicht Teil dieses Plans.

## 5. Umsetzung und Abnahme

Der Nutzer hat die Implementierung freigegeben. Alle fünf Read-only-Werkzeuge sind implementiert.
Der Live-Test über das offizielle MCP-C#-Client-SDK bestätigt erfolgreiche stdio-Aufrufe gegen das Spiel.
Die Nutzerkontrolle bestätigt Zyklus 1 / Tag 1, 9 Erwachsene, 4 Kinder, 0 Bots und 1 Gebäude.
Es wurden keine Spielaktionen ausgeführt und keine MCP-Client-Konfiguration verändert.

Die zuvor fehlende API-Verbindung ist gelöst: `http://localhost:8080/` funktioniert;
`127.0.0.1` wird in dieser Installation abgelehnt. Die Host-URL bleibt deshalb localhost,
während die Socket-Verbindung auf Loopback beschränkt ist.

Umsetzungsdetails/Abweichungen vom Dateiplan stehen in `docs/architecture/decisions.md`:
zusammengefasste Model-/Service-Dateien, interne JSON-Lesehilfen statt fremder DTOs, Prozessvariablen
statt automatischem Dateikonfigurationsanbieter, feste öffentliche Limits und konservative ID-Diagnose.
Abnahmeprotokoll: `docs/testing/live-poc.md`. Versionsnachweise: `docs/compatibility/timberborn.md`.

Weitere Schritte wie Client-Einrichtung, zusätzliche Datenfelder oder Spielaktionen werden separat abgestimmt.

## 6. Geplanter POC für einen Spieleingriff

Stand 2026-09-19: Nutzer hat die Quellenprüfung und Planung freigegeben. Dieser Abschnitt ist
der konkrete Vorschlag für die nächste Implementierungsfreigabe; noch kein Schreibcode und kein Spieleingriff.
Client-Einrichtung und GitHub-Sicherung wurden inzwischen separat erledigt (siehe Projektjournal).

### 6.1 Ziel und Quellenbefund

Genau ein Gebäude kontrolliert pausieren und anschließend auf seinen vorherigen Pausenstatus zurücksetzen.
Kein Bauen, Abreißen, Ressourcenändern, Save/Load, globales Spieltempo oder Automationsgraph.

Herstellerquellen am 2026-09-19 direkt über GitHub geprüft:

- [BuildingHandler](https://github.com/datvm/TimberbornMods/blob/1c057bda82ec955d1712937da916cddf0f382f24/MoreHttpApi/Handlers/BuildingHandler.cs)
- [Manifest](https://github.com/datvm/TimberbornMods/blob/1c057bda82ec955d1712937da916cddf0f382f24/MoreHttpApi/manifest.json)
- [Router](https://github.com/datvm/TimberbornMods/blob/master/MoreHttpApi/Services/MoreHttpApiEndpoint.cs)

Der aktuelle Upstream nennt More HTTP API 11.0.0, passend zur zuletzt live geprüften Mod-Version.
Route: `/MoreHttpApi/buildings/{id}/toggle-pause?paused=true|false`.
Trotz des Namens setzt der Handler einen gewünschten Zustand über Pause/Resume; er invertiert ihn nicht.
Er prüft GUID und Existenz, bietet aber keine atomare Prüfung des erwarteten Ausgangszustands.
Der Router erzwingt keine Trennung der Änderungen durch POST. Die Route wird deshalb ausdrücklich
als Schreiboperation behandelt, auch wenn der Adapter sie per GET aufruft.
Der [HTTP-Response-Helper](https://github.com/datvm/TimberbornMods/blob/1c057bda82ec955d1712937da916cddf0f382f24/MoreHttpApi/Helpers/HttpHelper.cs)
bestätigt HTTP 204 mit leerem Inhalt für den erfolgreichen Schreibhandler.
Ein Versionsgleichstand ist kein Nachweis für das Verhalten der installierten Binärdatei; das bestätigt erst der freigegebene Live-Pilot.

### 6.2 Werkzeugvertrag

Vorschlag: `set_building_paused(id, paused, expectedPaused)`.

| Parameter | Vertrag |
|---|---|
| id | Genau eine Gebäude-GUID aus einer frischen Gebäudesuche |
| paused | Gewünschter boolescher Zielzustand |
| expectedPaused | Erwarteter boolescher Ausgangszustand als Schutz vor veralteter Beobachtung |

Keine Standardwerte, keine Mehrfachauswahl, keine freien URLs und keine impliziten Wiederholungen.
Vor dem Schreiben Gebäudezugehörigkeit, Pausierbarkeit und aktuellen Zustand frisch lesen.
Unbekannte Werte, fehlendes Gebäude oder Abweichung von expectedPaused verhindern den Schreibrequest.
Wenn Ausgangs- und Zielzustand bereits gleich sind: bestätigter No-op ohne Schreibrequest.

Nach genau einem Schreibrequest den Zustand erneut lesen. Ergebnis enthält Gebäude-ID,
Ausgangszustand, Zielzustand, beobachteten Folgezustand (ggf. unbekannt), Zeitpunkte und Simulation-Kennzeichen.
Ergebnisausgänge: `unchanged`, `applied`, `rejected`, `unconfirmed`.
`applied` bedeutet beobachteter Zielzustand nach dem Request; keine Zusicherung über dauerhaft unveränderten Zustand.
HTTP-Erfolg allein ist kein Erfolg des Werkzeugs. Unklare Transportfehler, Timeout oder fehlgeschlagene
Nachprüfung nach möglichem Versand liefern `unconfirmed`, nicht die Behauptung, nichts sei geändert worden.
Bei diesem Ausgang keine automatische erneute Änderung oder Rücksetzung, sondern lesende Klärung.

### 6.3 Technische Grenzen

- Neues enges Schreib-Backend-Interface neben ITimberbornReadBackend und eigener Action-Service.
- Transport erhält eine typisierte Operation für genau diese Route; die bestehende Leseroutenliste bleibt unverändert.
- Prozessschalter `TIMBERBORN_ENABLE_WRITES=1`, standardmäßig aus. Ohne Freigabeschalter bleibt es bei fünf Lesewerkzeugen;
  der Backend-Schreibpfad verweigert Zugriffe zusätzlich. Kein Fake-Fallback bei Live-Fehlern.
- Schreibfähiges MCP-Werkzeug explizit als nicht lesend kennzeichnen; Annotationen ersetzen keine Laufzeitprüfung.
- Gesamte Vorprüfung/Schreibrequest/Nachprüfung innerhalb dieses Serverprozesses serialisieren.
  Andere Clients und Spielbedienung sind dadurch nicht gesperrt. expectedPaused ist keine atomare Compare-and-set-Garantie.
- Bestehende Loopback-, Timeout-, Größen-, Auth- und Redirect-Grenzen gelten weiter.
- Kein automatisches Wiederherstellen beim Prozessstart, kein Hintergrundauftrag und keine persistierten Spiel-IDs.

Konservative Alternative: vorerst ausschließlich lesen und den Pausenknopf manuell bedienen.
Der begrenzte Schreib-POC ist vertretbar, wenn ein geeignetes Testgebäude verfügbar ist.
Das Zurücksetzen des Pausenflags macht zwischenzeitlich entgangene Produktion oder andere Simulationseffekte nicht rückgängig.

### 6.4 Prüfungen und Abnahme

1. Fake-/Stub-Tests: deaktivierte Schreibfähigkeit, ungültige Parameter, unbekanntes/nicht pausierbares Gebäude,
   geänderter Ausgangszustand und No-op erzeugen keinen Schreibrequest.
2. Positivfälle für beide Zielzustände: genau eine erlaubte Mutation, danach belegte Zustandskontrolle.
3. Fehlerfälle: Auth/404/Serverfehler, Timeout vor/nach möglicher Verarbeitung, Abbruch und fehlgeschlagene Nachprüfung.
   Unklarer Ausgang führt weder zu Retry noch zu einer zweiten Mutation. Parallele Aufträge werden serialisiert.
4. MCP-stdio-Test: fünf Tools im Default; sechstes Tool nur mit Opt-in; gültiges Schema und eindeutige Änderungskennzeichnung.
5. Bestehende Tests bleiben erfolgreich. Gewöhnliche Testläufe mutieren niemals das echte Spiel.
6. Erst anschließend Live-Pilot auf der Testkolonie MCP: Nutzer wählt/bestätigt genau ein unkritisches,
   pausierbares Gebäude und erlaubt den Hin-/Rückweg. Frisch lesen, Gegenstatus setzen, kontrollieren,
   ursprünglichen Pausenstatus mit neuer Vorbedingung wieder setzen und kontrollieren; UI-Abgleich durch Nutzer.
   Kein geeignetes Gebäude vorhanden: stoppen; Nutzer bereitet eines selbst vor.
7. Bei erstem unbestätigtem Ergebnis stoppen und Zustand klären. Nicht mehrere Gebäude ausprobieren.
   Erfolg: beide Übergänge belegt, ursprüngliches Flag wiederhergestellt, keine weitere Schreibroute benutzt.

### 6.5 Umsetzungspakete und Freigabe

| Paket | Umfang | Ergebnis |
|---|---|---|
| E | Vertragsmodelle, Action-Service, Fake und deterministische Tests | Schreiblogik ohne Spielzugriff geprüft |
| F | Begrenzter HTTP-Schreibpfad, MCP-Registrierung hinter Opt-in, stdio-/Fehlertests | Implementierung bereit, normale Konfiguration weiter lesend |
| G | Explizit freigegebener einzelner Live-Pilot einschließlich Rückweg | Abnahme und dokumentierter Checkpoint |

Nächste Entscheidung: E/F implementieren. G und das Aktivieren der Schreibfähigkeit im lokalen Codex-Client
folgen nach bestandenen Tests und konkreter Auswahl des Testgebäudes. Ein zusätzlicher Mod ist für diesen Plan nicht vorgesehen.

### 6.6 Implementierung E/F und Live-Vorbereitung

Der Nutzer hat E/F ausdrücklich freigegeben und die einzige Holzfällerflagge als Testgebäude ausgewählt;
eine Namensvergabe ist im Spiel nicht möglich. Auswahl daher über exakt `LumberjackFlag.Folktails`,
genau einen Treffer und anschließend dessen GUID; keine Auswahl anhand des ersten beliebigen Treffers.
Lesende Vorprüfung bestätigt einen Treffer, pausierbar und nicht pausiert.
Das anschließende Go erlaubt den Live-Versuch einschließlich Rückweg.

E/F implementiert: getrenntes Schreibinterface, Action-Service mit prozessweiter Serialisierung,
typisierte einzelne HTTP-Schreibroute, geschütztes MCP-Werkzeug und zustandsbehafteter Fake.
Bestehende Leseroutenliste unverändert. HTTP 401/403 beim Schreiben sind explizite Ablehnungen;
andere unerwartete Schreibantworten einschließlich 404 konservativ unconfirmed. Fehler beim Nachlesen
sind nach quittiertem Schreiben immer unconfirmed. Keine automatischen Wiederholungen.
MCP-Annotationen sind konservativ: nicht lesend, potentiell destruktiv und keine Retry-Zusage.

Locked Restore, Release-Build mit 0 Warnungen/Fehlern und 74 reguläre Tests bestanden
(67 Anwendung/Adapter, 7 Integration); beide Live-Tests im normalen Lauf übersprungen.
Die Schreibfähigkeit wird für den Pilotprozess explizit aktiviert, nicht dauerhaft im Codex-Client.

### 6.7 Technischer Live-Pilot G

Nach erneutem Nutzer-Go separat über echtes MCP-stdio ausgeführt: genau eine Holzfällerflagge
gefunden, aktiven Ausgangszustand gelesen, pausiert und den Pausenstatus bestätigt, anschließend
wieder aktiviert und den aktiven Status bestätigt. Zusätzliche Abschlussabfrage bestätigt den
ursprünglichen Zustand. LivePauseTests: 1 bestanden, 0 Fehler, keine Wiederholung erforderlich.
Keine Spiel-IDs oder Rohantworten gespeichert. Der abschließende UI-Abgleich wurde beim Nutzer angefragt.
Schreibfähigkeit weiterhin nur im Pilotprozess aktiviert; die dauerhafte Codex-Konfiguration bleibt lesend.

### 6.8 Nutzerabnahme des sichtbaren Tests

Der Nutzer bestätigte zunächst den wieder aktiven Zustand und beauftragte danach ausdrücklich
erneutes Pausieren ohne sofortigen Rückweg. Die einzige Holzfällerflagge wurde über MCP eindeutig
ausgewählt, pausiert (applied) und separat mit Paused=true nachgelesen.
Anschließend bestätigte der Nutzer sichtbar im Spiel: Test erfolgreich.
Damit ist der begrenzte Schreib-POC technisch und durch UI-Abgleich abgenommen.
Letzter bestätigter Zustand: Holzfällerflagge absichtlich pausiert; kein automatisches Wiederaktivieren.
