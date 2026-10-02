# Übergabe zum Chatabschluss — 2026-10-02

## Haltepunkt und nächste Nutzerhilfe

0.25.0 ist installiert. Vor dem Austausch wurde Timberborn beendet vorgefunden;
Vorversion vollständig gesichert, fünf Paketdateien und alle erhaltenen Zusatzdateien
verglichen. 648 reguläre Tests und warnungsfreier Mod-Build sind dokumentiert.
**Noch kein Neustart-/Ladenachweis und kein Live-Bau mit 0.25.0.** Der Nutzer beendet
hier den Chat; die Übergabe startet weder das Spiel noch einen Test oder Zeitplan.

Bei Fortsetzung zuerst prüfen, ob die eigene Bridge 0.25.0 mit geladenem Spielstand
„MCP“ antwortet. Falls nicht, benötigt der nächste Agent nur Start/Laden durch den
Nutzer. Der begrenzte Pilot ist bereits autorisiert, eine erneute Grundsatzfreigabe
für genau diesen Umfang fehlt nicht. Spiel möglichst pausiert laden; aktuelles Tempo
und Session über MCP lesen. Nicht aus installiertem Manifest auf laufende Version schließen.

## Was ausdrücklich entschieden wurde

- Eigenständige Mod und nativer MCP-Pfad. Community-Mods und offizielle Beispiele
  dienen als Referenzen, nicht als zwingende Laufzeitbasis. Keine neuen Abhängigkeiten
  ohne Abwägung und Rückfrage; [Referenzinventar](references/README.md).
- Spielzustand strukturiert abfragen, Aktionen programmatisch über MCP ausführen.
  Keine Screenshot-Auswertung, Maus-/Tastatursteuerung, Save-Manipulation oder Cheats.
- Im freigegebenen Entwicklungsumfang selbstständig arbeiten. Der Nutzer wünscht
  größere zusammenhängende Entwicklungsschritte statt vieler kleiner Installationen.
  Speichern, Beenden und Neustarten des Spiels übernimmt weiterhin der Nutzer.
- Der Entwicklungsspielstand ist für gezielte Bau-/Abrissversuche freigegeben;
  die aktuell vereinbarte **engere Pilotgrenze** bleibt maßgeblich. Frühere konkrete
  Freigaben für 25 Beerenbüsche oder andere erledigte Tests sind keine offenen Aufträge.
- Regelmäßige geprüfte Checkpoints auf `chr0mat0mcodex/timberborn-mcp` sind beauftragt.
  Kein Hintergrunddienst. Freigegebener Commit-Autor: `Codex <codex@openai.com>`,
  nur pro Commit verwenden, keine globale Git-Konfiguration ändern.
- Ingame-Log mit kurzer Aktionsbegründung und Maus-Scrollen wurde vom Nutzer bestätigt;
  Bedienung belassen. `reasoning` ist eine kurze Erklärung für den Spieler, keine
  Speicherung interner Gedankengänge. [Vertrag](activity-log.md).

Diese Zusammenfassung dokumentiert bestehende Entscheidungen; sie ersetzt oder
erweitert [AGENTS.md](../AGENTS.md) nicht.

## Konkreter nächster Nachweis: Lager mit Anschlussweg

Der normale Baupfad verweigert weiterhin `roadProtection=unknown`. Allein für
`execute_building_project_pilot` wurde die noch unbelegte Bauphasen-Vorhersage
ausdrücklich akzeptiert: **ein SmallWarehouse.Folktails mit höchstens zwei neuen
ebenen Path-Zellen**. Bekannte Wegverluste und ungültige Geometrie bleiben verboten.
Kein automatischer Abriss, keine Ressourcen oder Sofortfertigstellung.

1. Laufende Bridge-Version, aktuelle Session, Pause, reale Ziele und eine unabhängige
   vorhandene Wegverbindung lesen. Objektbestand für den Abgleich erfassen.
2. `plan_building_project` mit aktuellen Parametern lesen; einen passenden kleinen
   Kandidaten wählen. Suchbereich höchstens 8×8, eine Drehung, vier Optionen.
   Aus der frischen Antwort `optionIndex` und `planKey` übernehmen.
3. Genau einmal `execute_building_project_pilot` mit denselben Suchparametern,
   frischer `session`, neuer `actionId` und `mode=development_pilot` aufrufen.
   Der Aufruf macht selbst eine frische gemeinsame Vorschau. Eine zusätzliche
   Diagnosevorschau ist bei bereits eindeutigem Kandidaten nicht zwingend nötig.
4. `inspect_building_project(session, actionId)` begrenzt abfragen. Die Mod führt
   je Update höchstens einen Platzierungsschritt aus und wartet je Objekt höchstens
   fünf Echtzeitsekunden auf Bestätigung. Währenddessen pausiert lassen.
5. Reale Entities, Lagerbaustelle, Eingang, Bauarbeiterzugang und Bestandsverbindung
   unabhängig nachlesen. `completed` bestätigt Auftrag und Zugang, **nicht** Fertigstellung,
   Lieferung, Lagerkonfiguration oder nachhaltigen Betrieb.
6. Identische Anfrage darf nur denselben Status liefern; geänderte Parameter müssen
   abweisen. Nach `stopped`/`unconfirmed` nur Zustand abgleichen, kein neuer Bauversuch.
   Auch nach Neustart ist ein zurückgesetztes Sitzungsbudget keine erneute Pilotfreigabe.

Ein akzeptierter Auftrag pro geladener Sitzung. Teilstände bleiben stehen; IDs sind
in der Quittung. Statusdaten sind sitzungslokal und nach Neuladen verloren. Bei
Transportabbruch Status/Entities lesen, nicht aus fehlender Antwort auf fehlende Wirkung schließen.
Abnahme begrenzen, beispielsweise insgesamt höchstens 24 MCP-Aufrufe und zehn
Statusabfragen; keine lange autonome Spielschleife anschließen.
[Vollständiger Vertrag](building-project-execution-proposal.md).

## Letzte Live-Evidenz — historisch, neu abfragen

Unter 0.24.1 wurden zwei gemeinsame Vorschauen und eine falsche Plan-Kennung geprüft:
Lager plus zwei Wege gültig, Eingang verbunden, keine gemessenen Verluste,
Vorschau vollständig zurückgenommen; falsche Kennung als `state_conflict` abgelehnt.
172 Gebäude-/Wegobjekte und pausierte Spielzeit unverändert. Letzte Zeitbeobachtung:
Tag 74, etwa 0,28125 Stunden. Keine aktualisierte Wirtschaftsbilanz für diesen Zeitpunkt.

Früher geeigneter Suchbereich: x=42, y=18, z=6, width=8, height=8, rotation=0.
Damals Lagerursprung (48,23,6), Eingang (48,22,6), Anschluss (46,22,6), neue Wege
(47,22,6) und (48,22,6). **Nur Suchhinweis**, kein wiederverwendbarer Bauplan.
Distrikt-ID, Belegung und Plan-Kennung müssen frisch gelesen werden.
Eine nutzbare unabhängige Kontrollverbindung war Distriktzentrum → oberer Erfinder.

Neun Prüfstellen einer vorhandenen unfertigen mittleren Lagerbaustelle hatten keine
Distriktweg-Mitgliedschaft, obwohl `IsReachableByBuilders()` wahr war. Deshalb niemals
Distriktweg, Gelände-/Bauarbeiterzugang und hypothetische Bauphase gleichsetzen.
Öffentliche Vorschau-Reichweiten und `ConstructionSiteAccessible` sind mögliche weitere
Forschungsansätze, noch keine belegte vollständige Vorabprüfung.

## Wo die Umsetzung liegt

| Aufgabe | Quelle |
| --- | --- |
| MCP-Werkzeuge und Schemata | [BuildingTools](../src/Timberborn.McpServer/BuildingTools.cs) |
| Antwort-/Planbindung | [Native-Client](../src/Timberborn.Backend.Native/BuildingProjectExecutionClient.cs) |
| Eingaben, enger Ausnahmefilter, Ablauf | [Request](../src/Timberborn.Bridge.Core/BuildingProjectExecutionRequest.cs), [Policy](../src/Timberborn.Bridge.Core/BuildingProjectPilotPolicy.cs), [Controller](../src/Timberborn.Bridge.Core/BuildingProjectController.cs) |
| Reguläre Spielplatzierung und Nachprüfung | [BuildingProjectExecution](../mod/Timberborn.AgentBridge/BuildingProjectExecution.cs) |
| Gemeinsame Vorschau / bestehende Wege | [SiteValidation](../mod/Timberborn.AgentBridge/SiteValidation.cs), [RoadProtection](../mod/Timberborn.AgentBridge/RoadProtection.cs) |
| Tests | [Pilotprüfungen](../tests/Timberborn.Tests/BuildingProjectExecutionTests.cs), [realer MCP-Transport mit synthetischem Spielport](../tests/Timberborn.IntegrationTests/NativeBridgeTests.cs) |

## Lokale Arbeitsmittel und Fallstricke

- Installationsort relativ zum Dokumente-Ordner: `Timberborn/Mods/TimberbornAgentBridge`.
  `bridge.local.json` dort ist privat; nicht ausgeben, ersetzen oder committen.
- Geprüftes Paket: `.local/packages/agent-bridge-0.25.0-20261002-220536-96f02f02/TimberbornAgentBridge`.
  Sicherung: `.local/backups/agent-bridge-before-0.25.0-20261002-220857-5d8095f2/TimberbornAgentBridge`.
  Jeweils vom Repository aus; nur lokale Artefakte, nicht Bestandteil des Git-Repos.
- `.local/road-protection-live/` enthält einen bisherigen MCP-stdio-Pilothelfer.
  Er erwartet noch **0.24.1** und erlaubt die beiden neuen Werkzeuge noch nicht.
  Nicht unverändert starten: Versionsprüfung und enge Werkzeugliste gezielt anpassen
  oder einen gleichartig begrenzten MCP-Client verwenden. Keine generischen HTTP-Aufrufe.
  Bisher prüft er `outcome`; für Bauprojekte muss er auch `state=stopped/unconfirmed`
  beachten und dann nur noch abgleichen. Pro Test einen frischen lokalen Ausgabeordner
  verwenden; vorhandene Startmarker und Rohbelege nicht überschreiben.
- `scripts/start-native.ps1` startet absichtlich nur Leser und setzt sämtliche
  Aktionsschalter zurück. Für den freigegebenen Pilot braucht der eigene Serverprozess
  `TIMBERBORN_ENABLE_BUILDING_PLACEMENT=1` und die Mod ihr vorhandenes Bau-Opt-in.
  Die private Konfiguration beim Wiedereinstieg nicht pauschal ändern.
- Vor direktem `dotnet`-Aufruf `scripts/environment.ps1` laden. Build-/Testablauf:
  [DEVELOPMENT_WORKFLOW](../DEVELOPMENT_WORKFLOW.md). Keine globalen Tools nötig.
- Laufende eigene MCP-Prozesse können Build-DLLs sperren und vom Client neu gestartet
  werden. Prozess und genaue Projekt-Kommandozeile identifizieren, nur betroffenen
  eigenen Server für den Build neu starten; nicht pauschal `dotnet` oder das Spiel beenden.
- Private lokale Rohbelege und Hilfsprogramme unter `.local/` belassen. Öffentliche
  Übergabe enthält keine Tokens, Spiel-IDs, Benutzernamen aus dem Spiel oder Rohlogs.

Bereits geklärte Spielsemantik: `inspect_colony` enthält weiterhin nur drei Beispielgüter;
für die vollständige Übersicht `inspect_goods` verwenden. Globale Vorräte und
Ausgabepuffer nicht einem einzelnen Gebäude zuschreiben; Gebäudeinventare separat lesen.
Güterhistorien werden zum Tageswechsel fortgeschrieben. Navigationsreichweite beweist
noch keine Ernte. Tote/gestresste Vegetation und verstorbene Biber getrennt behandeln.
Eine Zapfzone bedeutet Entfernen von Fällmarkierungen, keine eigene gespeicherte Zone;
Details und Grenzen stehen in [Kiefernschutz](removal-and-pine-protection.md).
Der bestätigte Personal-Sonderfall `desiredWorkers=0` am Erfinder ist kein verlässlicher
Stilllegungsweg; reguläre Gebäudepause nutzen, keine unbestätigte Änderung wiederholen.

## Weitere offene Arbeit bleibt erhalten

100 lebende Biber und alle regulären Gebäude sind **nicht erreicht**. Letzter separat
dokumentierter Wirtschaftsnachweis ist Tag 68 mit 40 Bibern, 40 Betten und 26/157
Gebäudetypen; nicht als heutigen Bestand ausgeben. Weitere offene Punkte: vollständiger
Wegschutz, Sonderbauten/weitere Distrikte, Hunger-/Durst-UI-Alerts, vollständige
Produktionsblockaden und nachhaltige Gesamtversorgung. Später: Frage-Popup im Spiel.
Produktionsgraph, Zeitläufe und Ingame-Log sind bereits implementiert, keine offenen
Neuentwicklungen. [Backlog](../BACKLOG.md), [Kolonieziele](colony-goals.md),
[Popup-Recherche](player-question-popup.md).
