# Technische Übergabe

Stand: 2026-10-02. **Testsystem und schnelle, effiziente Entwicklung zuerst.**
Die aktuelle Nutzerentscheidung ersetzt ältere spielstandsbezogene Schutzgrenzen
und konkrete Einzelgenehmigungen. Kein bestimmter Save oder Koloniefortschritt ist
wichtig. [AGENTS.md](../AGENTS.md) enthält die maßgeblichen Direktiven.

## Technischer Haltepunkt

0.25.0 ist gebaut und installiert, fünf Paketdateien verglichen, vorherige Mod
vollständig gesichert und private Konfiguration erhalten. 648 reguläre Tests
bestanden. **Reale Bauausführung dieser Version noch nicht live bestätigt.**
Ob inzwischen ein Testspiel geladen ist, beim Wiedereinstieg strukturiert prüfen.
Ein beliebiger geeigneter Testspielstand genügt. Start/Neustart nur koordinieren,
wenn technisch erforderlich; Speichern ist keine Voraussetzung.

Arbeitsbranch `codex/road-protection-pilot`; `origin/master` zuletzt 0.22.0.
Implementierung `8349b80`, Installation `e92713c`; weitere Checkpoints aus Git lesen.
Eigene Bridge und natives MCP-Backend, keine Fremdmod-Pflichtbasis.

## Nächster sinnvoller Test

1. Tatsächlich laufende Bridge-Version, Session und Tempo lesen.
2. Frischen Bauplan für einen geeigneten Platz abfragen. Die **installierte 0.25.0**
   unterstützt im Ausführungswerkzeug bislang SmallWarehouse.Folktails und höchstens
   zwei neue ebene Wege; höchstens 8×8 Suchbereich, vier Optionen, eine Drehung.
3. Mit frischer Plan-Kennung, Session, actionId und `mode=development_pilot` ausführen.
   Das Spiel muss für diese Implementierung pausiert sein. Status abfragen und reale
   Entities, Wegverbindung und Bauarbeiterzugang kontrollieren.
4. Ergebnis bewerten: `completed` heißt Auftrag/Zugang bestätigt, nicht fertig gebaut
   oder produktiv. Bei Teilfehlern tatsächlichen Zustand feststellen, Ursache beheben
   und sinnvoll erneut testen. Keine blinden Wiederholungen unklarer Mutationen.
5. Aussagekräftiges Ergebnis dokumentieren, dann weiterentwickeln. Keine Pflicht,
   Testbauten abzureißen, sie zu erhalten oder den Ausgangszustand wiederherzustellen.

Ein Auftrag je Session, fünf Sekunden Bestätigungsfrist je Objekt, normale
Unknown-Sperre und Vorlage/Wegzahl sind **Codegrenzen**, keine dauerhaften Freigaben.
Die produktseitige Duplikatsicherheit bleibt zu prüfen. Weitere Testfälle,
Testbauten und Abrisse benötigen keine Erhaltungsfreigabe. Unnütze oder sehr teure
Testschleifen vermeiden. [0.25.0-Vertrag](building-project-execution-proposal.md).

## Für die nächste Umsetzung relevante Erkenntnisse

- Gemeinsame Vorschau von Lager plus zwei Wegen unter 0.24.1 live bestanden;
  falscher Plan-Hash korrekt abgewiesen. Reale Ausführung folgt erst in 0.25.0.
- Distriktweg-Mitgliedschaft und Bauarbeiterzugang sind verschieden: bei einer
  echten Baustelle fehlte erstere, obwohl `IsReachableByBuilders()` wahr war.
  Keine Gleichsetzung mit hypothetischer Bauphase oder Lieferfähigkeit.
- Öffentliche Vorschau-Reichweiten und `ConstructionSiteAccessible` sind mögliche
  Ansätze, aber noch kein belegter vollständiger Vorabnachweis. Reale Tests sind
  erlaubt und sollen vor aufwendiger spekulativer Diagnose bevorzugt werden.
- `inspect_colony` enthält nur drei Beispielgüter; vollständige Güter über
  `inspect_goods`. Globale Bestände nicht einzelnen Gebäuden zuschreiben;
  Gebäudeinventare separat lesen. Güterhistorien werden zum Tageswechsel fortgeschrieben.
- Native Arbeitsreichweite belegt noch keine Ernte. Tote/gestresste Vegetation und
  verstorbene Biber korrekt unterscheiden. Zapfbereich bedeutet entfernte Fällmarkierung.
- `desiredWorkers=0` am Erfinder wurde nicht übernommen; reguläre Gebäudepause nutzen.
- Ingame-Log und Scrollen sind bestätigt. `reasoning` ist eine kurze Begründung für
  den Spieler, keine interne Gedankenkette. Kein UI-Umbau ohne funktionalen Anlass.

## Implementierung und lokale Mittel

| Aufgabe | Quelle |
| --- | --- |
| MCP-Schema | [BuildingTools](../src/Timberborn.McpServer/BuildingTools.cs) |
| Antwortbindung | [Native-Client](../src/Timberborn.Backend.Native/BuildingProjectExecutionClient.cs) |
| Ablauf / Ausnahmefilter | [Controller](../src/Timberborn.Bridge.Core/BuildingProjectController.cs), [Policy](../src/Timberborn.Bridge.Core/BuildingProjectPilotPolicy.cs) |
| Spielplatzierung | [BuildingProjectExecution](../mod/Timberborn.AgentBridge/BuildingProjectExecution.cs) |
| Vorschau / Wegdiagnose | [SiteValidation](../mod/Timberborn.AgentBridge/SiteValidation.cs), [RoadProtection](../mod/Timberborn.AgentBridge/RoadProtection.cs) |
| Tests | [Pilotprüfungen](../tests/Timberborn.Tests/BuildingProjectExecutionTests.cs), [MCP-Integration](../tests/Timberborn.IntegrationTests/NativeBridgeTests.cs) |

- Installation relativ zu Dokumente: `Timberborn/Mods/TimberbornAgentBridge`.
  `bridge.local.json` dort privat erhalten und nicht ausgeben/committen.
- Paket: `.local/packages/agent-bridge-0.25.0-20261002-220536-96f02f02/TimberbornAgentBridge`.
  Sicherung: `.local/backups/agent-bridge-before-0.25.0-20261002-220857-5d8095f2/TimberbornAgentBridge`.
- `.local/road-protection-live/` enthält den bisherigen MCP-stdio-Helfer. Erwartet
  noch 0.24.1 und kennt die neuen Werkzeuge nicht. Versions-/Werkzeugprüfung und
  `state=stopped/unconfirmed` ergänzen oder einen passenden begrenzten MCP-Client nutzen.
- `scripts/start-native.ps1` ist ein Lesestarter. Für Schreibtests im eigenen
  Serverprozess passende Aktionsschalter setzen, für diesen Baupfad
  `TIMBERBORN_ENABLE_BUILDING_PLACEMENT=1`; Mod braucht ihr entsprechendes Opt-in.
- Vor direkten dotnet-Aufrufen `scripts/environment.ps1` laden. Eigene laufende
  MCP-Prozesse können Build-DLLs sperren; konkreten Prozess identifizieren und
  betroffenen Server neu starten, nicht pauschal sämtliche dotnet-Prozesse beenden.

Weitere Aufgaben: [Backlog](../BACKLOG.md). Historische Spielstände, genaue Vorräte,
Koordinaten oder alte Ingame-Freigaben sind für die Fortsetzung nicht erforderlich.
