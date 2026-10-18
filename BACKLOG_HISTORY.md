# Backlog-Archiv

Zeitgebundene Befunde und frühere Arbeitsstände; keine aktuelle Aufgabenliste.
Spätere Abnahmen überholen ältere Offen-/Gate-Aussagen. Maßgeblich sind
[Backlog](BACKLOG.md) und [Projektstand](PROJECT_STATE.md).

## Archivstand 2026-10-18 vor A01 und A02

Unveränderter Backlog-Inhalt vor der Bereinigung, einschließlich des Audits.
Aktive Restumfänge wurden in die aktuelle Aufgabenliste übernommen; abgeschlossene
Verläufe und ihre damaligen Widersprüche bleiben hier als Herkunft erhalten.
Die Auditpunkte A01/A02 sind inzwischen abgeschlossen, siehe aktuelles Backlog.

# Backlog

## Projektprüfung 2026-10-18 — abarbeitbare Befunde

Prüfbasis: sauberer Arbeitsbaum auf `f52750f`, Branch
`codex/road-protection-pilot`, lokal ohne Abstand zum vorhandenen Remote-Trackingstand.
Bridge/Manifest 0.74.0; letzter dokumentierter Featureabschluss: automatische
Bretterketten-Vorprüfung, 2025 Unit- und 13 Integrationstests, drei optionale
Livefälle übersprungen. Diese Zahlen stammen aus PROJECT_STATE.md, nicht aus
einem neuen Testlauf. Der aktuelle Spiel-/Installationszustand wurde nicht abgefragt.

**Kurzurteil:** Die Schichten Mod → Bridge.Core → Native-Client → MCP sind sinnvoll.
Gezieltes Refactoring lohnt sich bei Versionsprofilen, Werkzeugdispatch und
lesbaren Prüfketten. Ein Komplettumbau, neuer Planer oder neue Abhängigkeiten sind
durch diese Prüfung nicht begründet. Die folgenden Einträge sind Arbeitsvorschläge;
dieser Audit implementiert keine Codeänderungen und erteilt keine Liveabnahme.

**Prüfumfang und Grenzen:** Inventar aller 336 versionierten C#-Dateien
(34.284 Textzeilen, einschließlich Tests), Projekt-/Paketkonfiguration,
Start-/Gate-/Supervisor-Skripte und Dokumentationsstruktur. Vertiefte Quellprüfung
der zentralen Transport-, Dispatch-, Versions-, Journal-, Vorprüfungs- und
Installationspfade sowie zugehöriger Testquellen. Fachliche Planungs-/Spieladapter
wurden über Struktur, Verträge und dokumentierte Nachweise eingeordnet; keine
vollständige Zeilenprüfung jeder Geometrie- oder Produktionsroutine. 112 versionierte
Markdown-Dateien auf lokale Dateilinkziele geprüft: keine fehlenden Dateien;
Anker und externe Links dabei nicht geprüft. Keine Builds, Tests, Installation,
Spielmutation oder externe Paket-/Schwachstellenprüfung ausgeführt. Damit ist dies
eine breite statische Bestandsaufnahme, kein Fehlerfreiheits- oder Sicherheitszertifikat.
Die Dateiinventur fand keine versionierten DLLs, Screenshots oder Datenbankdateien;
das ersetzt keinen vollständigen Secret-Scan.

Prioritäten: **P1** vor weiterer Arbeit mit irreführendem Projektstand beheben;
**P2** gezielt vor der nächsten betroffenen Erweiterung bearbeiten;
**P3** nach Bedarf. Aufwand S/M/L ist relativ, keine Zeitschätzung.
Jeder Punkt bleibt offen, bis sein eigener Abschlussnachweis dokumentiert ist.
Für Codeänderungen gilt unverändert das menschliche Gate mit gezieltem Livetest.

### A01 — P1 / S: widersprüchliche aktuelle Einstiege bereinigen

- [ ] **Befund, bestätigt:** [START_HERE.md](START_HERE.md) nennt unter
  „Aktueller Auftrag“ noch den fehlgeschlagenen 0.67-Pilot und unter „Technischer
  Stand“ Bridge 0.64.0. README und Einstieg nennen 1936 Tests als „Letzte Abnahme“,
  [Kompatibilität](docs/compatibility/timberborn.md) aktuell 1931,
  [Übergabe](docs/session-handoff.md) den Abschluss 0.50.0. Das widerspricht
  [PROJECT_STATE.md](PROJECT_STATE.md) und dem aktuellen Code/Manifest.
- **Arbeit:** aktuelle Köpfe korrigieren oder auf einen maßgeblichen Projektstand
  verlinken; historische Belege ausdrücklich datieren. Auch den „aktuellen
  Releaseabschluss 0.53.0“ im historischen missionsplan eindeutig historisieren.
- **Abnahme:** Einstieg, README, Übergabe und Kompatibilitätskopf nennen keinen
  überholten Auftrag als aktuell; Bridge-Version und MCP-Featurestand bleiben
  getrennt. Inhalts-/Diff-/Linkprüfung genügt, keine Code- oder Regeländerung nötig.

### A02 — P2 / M: aktive Aufgaben aus der Chronik herauslösen

- [ ] **Befund, bestätigt:** Dieses Backlog hatte vor dem Audit rund 840 Zeilen.
  Aktive Folgearbeiten, abgeschlossene Gates, damalige Fehlschläge und B-/F-Ideen
  stehen trotz historischer Einordnung in derselben Datei. Ein Archiv existiert
  bereits, wird für viele neuere Abschlussverläufe aber nicht genutzt.
- **Arbeit:** abgeschlossene Verlaufsblöcke nach [BACKLOG_HISTORY.md](BACKLOG_HISTORY.md)
  verschieben; aktive B-/F-Restumfänge, A-IDs, Zurückstellungen und Belegverweise
  behalten. Vorhandene Ankerverweise erhalten oder alle eingehenden Links anpassen.
  Keine erledigten Zwischenfehler durch bloßes Verschieben wieder eröffnen.
- **Abnahme:** jede aktive Aufgabe hat Status, nächsten Schritt und Abschlusskriterium;
  alte Gate-Aufträge erscheinen ausschließlich als Historie. Keine verlorenen
  Belege/IDs. Nach A01, gern als gemeinsames reines Dokumentationspaket.

### A03 — P2 / M: Versions- und Fähigkeitsprofile zentralisieren

- [ ] **Befund, bestätigt:** `0.74.0` steht in 41 Dateien unter src/mod/tests.
  Besonders [NativeClient.cs](src/Timberborn.Backend.Native/NativeClient.cs)
  wiederholt lange Versionslisten für Envelope, Straßen-/Baunachweise und
  Laufzeitfelder; [ValidationBudgetContract.cs](src/Timberborn.Backend.Native/ValidationBudgetContract.cs)
  pflegt eine weitere Liste. Ein Versionsschritt kann dadurch einzelne
  Pflichtprüfungen vergessen. Die Fundzahl umfasst auch berechtigte Fixtures.
- **Arbeit:** explizite bekannte Versionsprofile mit benannten Vertragsfähigkeiten
  einführen; erst Envelope und eine Vertragsfamilie migrieren, dann die übrigen.
  Historische Unterschiede und unbekannte Versionen strikt erhalten; keine
  pauschale Freigabe durch `version >= ...` und keine erfundenen Livefähigkeiten.
- **Abnahme:** Matrix mit historischen Grenzversionen, 0.74.0 und synthetischem
  unbekanntem Stand; fehlende Pflichtfelder werden weiterhin abgewiesen.
  Nach Gate ein aktueller Reader-/Start-/Ablehnungspilot. Aufwand/Nutzen nach
  der ersten migrierten Familie prüfen, bevor weitere Familien folgen.

### A04 — P2 / M: Werkzeugmetadaten und Freigaben gemeinsam modellieren

- [ ] **Befund, bestätigt:** [NativeTools.cs](src/Timberborn.McpServer/NativeTools.cs)
  verbindet Katalog, Dispatch, elf Positions-Bools, Fehlerübersetzung und Logging.
  [BridgeHttpServer.MethodAllowed](src/Timberborn.Bridge.Core/BridgeHttpServer.cs)
  pflegt die HTTP-Freigaben separat als lange bedingte Ausdruckskette.
  Im aktiven Räumgruppenpfad wird für die Mutationsprüfung sogar der vollständige
  `Catalog(true,...)` einschließlich Schemagenerierung erneut aufgebaut.
- **Arbeit:** zunächst benannte Optionsstruktur und feste Klassifikation
  lesend/mutierend/Status/Cancel einführen; Katalogbau aus dem Aufrufpfad entfernen.
  Danach familienweise Registrierung konsolidieren. Die native HTTP-Freigabe
  bleibt eine eigenständige Schutzschicht, ihre Übereinstimmung wird geprüft.
- **Abnahme:** Katalog, Handler und Opt-ins stimmen für ein-/ausgeschaltete
  Aktionsgruppen überein; deaktivierte Mutation erreicht kein HTTP. Status/Cancel
  während belegter Ausführung und Räumgruppenbesitz bleiben korrekt. Vorher/Nachher
  gleiche Toolnamen, Schemas und Fehlerverträge. A03 kann unabhängig beginnen.

### A05 — P2 / S–M: dicht geschriebene Fachprüfungen lesbar machen

- [ ] **Befund, bestätigt:** 271 C#-Textzeilen sind länger als 300 Zeichen
  (inklusive Beschreibungen und Tests, daher keine reine Komplexitätsmetrik).
  NativeTools umfasst rund 34.000 Zeichen in nur 299 Zeilen; auch native
  Vertragsprüfungen und Mod-Dispatch bündeln zahlreiche Bedingungen pro Zeile.
- **Arbeit:** bei A03/A04 betroffene Methoden normal formatieren; Session-,
  Identitäts-, Geometrie- und Ergebnisprüfungen in benannte kleine Prüfungen
  zerlegen. Kein repoweiter Formatierungsdiff und keine neue Abstraktionsschicht
  allein wegen Dateigröße. Weitere Planer erst bei konkreter Änderung anfassen.
- **Abnahme:** vorhandene Positiv-/Negativverträge bleiben gleich; Diff trennt
  mechanische Umformatierung und fachliche Änderung. Charakterisierung dort
  ergänzen, wo eine verschobene Bedingung sonst ungeschützt wäre.

### A06 — P2 / S: exakt das gerade gebaute Paket installieren

- [ ] **Befund, bestätigt; Fehlzuordnung nicht praktisch reproduziert:**
  [prepare-human-live-test.ps1](scripts/prepare-human-live-test.ps1) sucht nach
  dem Build unter `.local/packages` alle `agent-bridge-*` und nimmt den neuesten
  `LastWriteTimeUtc`. [build-native-bridge.ps1](scripts/build-native-bridge.ps1)
  kennt den exakten eindeutigen Ausgabepfad, gibt ihn aber nicht strukturiert zurück.
  Ein anderes Paket mit neuerem Zeitstempel kann so ausgewählt werden. Die spätere
  Hashprüfung vergleicht nur gegen dieses bereits ausgewählte Paket.
- **Arbeit:** konkreten Paketpfad als Rückgabewert/Artefaktmanifest übergeben;
  Manifestversion, erwartete Dateien und Buildzuordnung vor Installation prüfen.
  Keine Auswahl durch globale Verzeichnissortierung.
- **Abnahme:** isolierter Skriptfall mit einem fremden, neuer datierten Paket
  wählt trotzdem ausschließlich die aktuelle Ausgabe; Fehler vor Austausch bei
  fehlendem/inkonsistentem Ergebnis. Anschließend menschliches Installationsgate
  und Status-/Versionsabgleich. Sicherung und Konfigurationserhalt beibehalten.

### A07 — P2 / M: ausgelieferten MCP-Stand eindeutig erkennbar machen

- [ ] **Befund, bestätigt:** mehrere jüngste MCP-Fähigkeiten nutzen dieselbe
  Bridge 0.74.0; der Supervisor meldet konstant Version `1`. Eine Bridge-Version
  beweist daher weder aktuellen MCP-Build noch aktuellen Client-Werkzeugkatalog.
  Das ist auch im bestehenden B12 als Erkennungsproblem vermerkt.
- **Arbeit:** datensparsame Build-/Katalogkennung im Status/Handshake vorsehen,
  getrennt von Bridge-API und Liveabnahme. Keine lokalen Pfade oder private
  Metadaten. Gate-Ausgabe und installierten/betriebenen Stand damit zuordnen.
- **Abnahme:** zwei MCP-Builds bei gleicher Bridge sind unterscheidbar;
  veralteter Clientkatalog wird als mögliche Abweichung erkennbar. Kein
  automatischer Neustart und keine Behauptung einer Liveabnahme aus der Kennung.
  Mit A06 abstimmen; A03 darf nicht von einer neuen API abhängig gemacht werden.

### A08 — P2 / M: direkte Mod-Adapter als eigene Nachweisgrenze behandeln

- [ ] **Befund, bestätigt:** [TimberbornMcp.slnx](TimberbornMcp.slnx) und die
  Testprojekte referenzieren die spielabhängige AgentBridge nicht; sie wird im
  Gate separat gegen Spiel-DLLs gebaut. Die hohe Unit-Testzahl prüft damit nicht
  unmittelbar die tatsächlichen Spielservice-Aufrufe in den Mod-Adaptern.
  Das ist eine Architekturgrenze, kein Beleg für fehlerhafte Adapter.
- **Arbeit:** für den nächsten geänderten Adapter die reine Entscheidung in
  Bridge.Core halten/verschieben und einen gezielten Adapter-Livefall definieren.
  Vorrang: Wiederherstellung temporärer Vorschau, Sessionwechsel und tatsächliche
  Bau-/Fertigzugänge. Bestehende Tests nutzen; keine Spiel-DLLs in Fixtures ablegen.
- **Abnahme:** dokumentierte Zuordnung „synthetisch geprüfte Policy / gegen Spiel
  kompiliert / real beobachteter Adapterfall“ für die betroffene Familie. Keine
  pauschale Behauptung, alle Modpfade seien durch 2025 Unit-Tests abgenommen.

### A09 — P3 / M: Supervisor-Eingang begrenzen und Fehler isolieren

- [ ] **Befund, bestätigt; Lastwirkung ungeprüft:**
  [codex-mcp-supervisor.py](scripts/codex-mcp-supervisor.py), `main()`, liest
  beliebig lange Zeilen, ruft `json.loads` ohne lokale Fehlerbehandlung auf und
  startet für jede Nachricht mit `id` einen neuen Thread. Fehlerhaftes JSON
  verlässt die Schleife über `finally`; viele Requests können viele Threads
  erzeugen. Dies betrifft den lokalen stdio-Client, keinen öffentlichen Endpunkt.
- **Arbeit:** sinnvolle Frame-/Inflightgrenzen und kontrollierte Parsefehler
  ergänzen; Lifecycle/Cancel darf auch bei belegtem Kontingent erreichbar bleiben.
  Keine Warteschlange mit später wiederholten Mutationen einführen.
- **Abnahme:** synthetisch fehlerhafte Zeile, übergroßer Frame und kleine begrenzte
  Requestserie; kontrollierte Antwort, kein unerwarteter Backendneustart,
  Stop bleibt nutzbar. Anschließend normaler Supervisor-Lifecycle-Livefall.

### A10 — P2 / S–M: Journallimits eindeutig diagnostizieren

- [ ] **Befund, bestätigt:**
  [LargeProjectLedger.Start](src/Timberborn.Bridge.Core/LargeProjectLedger.cs)
  weist ab 128 Einträgen mit demselben `state_conflict` wie bei einem blockierenden
  Projekt ab. Freigabe entfernt die Historie nicht. Räumgruppen haben dagegen
  einen expliziten `cleared_group_journal_limit`. Eine lange Session kann damit
  trotz abgeschlossener Projekte dauerhaft neue Großprojekte ablehnen.
- **Arbeit:** Limit und verbleibende Kapazität diagnostizierbar machen, eigenen
  Ablehnungsgrund durch die Schichten tragen. Aufbewahrung/Archivierung nur als
  gesonderten Entwurf prüfen; alte Aktions-IDs dürfen durch Bereinigung nicht
  wieder mutieren. Bestehenden B06-Reloadbedarf nicht mit blindem Neustart lösen.
- **Abnahme:** Grenze 127/128/129 synthetisch, abgeschlossene/freigegebene
  Einträge sowie identischer und geänderter Wiederaufruf. Aktionsschutz bleibt
  erhalten; keine 128-Projekte-Liveserie nur zum Erreichen des Limits.

### A11 — P3 / S: Legacy-Standard beim Direktstart klären

- [ ] **Befund, bestätigt:** [Program.cs](src/Timberborn.McpServer/Program.cs)
  wählt ohne `TIMBERBORN_BACKEND` weiterhin `more-http-api`, obwohl die native
  Bridge die aktuelle Architektur ist. Der reguläre Supervisor setzt `native`
  korrekt; deshalb kein Fehler im belegten Standard-Supervisorpfad.
- **Arbeit:** Direktstart explizit als Legacy kennzeichnen oder bei fehlender
  Backendwahl mit verständlicher Konfigurationsmeldung abbrechen. Auswahl erst
  nach Kompatibilitätsentscheidung ändern; Legacy/Fake nicht beiläufig löschen.
- **Abnahme:** dokumentierter Direktstart, Supervisorstart und explizite
  Legacy-/Fake-Auswahl liefern den beabsichtigten Backendmodus; kein stiller
  Fallback auf die Fremdmod nach einem nativen Verbindungsfehler.

### A12 — P2 / M–L: bekannte Funktions- und Livegrenzen gezielt schließen

- [ ] **Bestehende Restarbeit, keine neu nachgewiesenen Regressionen:** B06
  (Reload), B03 (allgemeine Kraft-/Höhenrouten), B07 (Produktionsausbauplanung),
  F01/F02 (vollständiger Bauphasenschutz/Umweg vor Bestandsänderung) und F04/F05
  (Geometrien/Fraktionen) bleiben offen. Die aktuelle Bretterdiagnose ersetzt
  keinen erfolgreichen Produktions- oder Liefernachweis.
- **Nächste kleine Pakete:** (a) konkrete Kartengrenzen in Parameterfehlern;
  (b) unterer Zielweg an gedrehter Bestandstreppe; (c) deterministischer Nachweis
  unbekannter Objektidentität; (d) ein positiver Bretterproduktions-/Abtransportfall.
  Jeweils vorhandenen Fachvertrag und aktuelle Implementierung zuerst abgleichen.
  B06/B03/B07 erst als abgegrenzten Entwurf bewerten, nicht als Sammelrefactoring.
- **Abnahme:** pro Paket ein Positiv- und nötiger Negativkontrollfall mit
  vorher benanntem Erfolg/Stoppkriterium. Geometrie, Bauarbeiterzugang,
  Fertigzugang, Netz und Betrieb getrennt belegen. Bei fehlerhafter Kontrolle
  stoppen; keine ungezielten Zeitrennen, Save-Manipulation oder Koloniepflege.
  Bereits akzeptierte synthetische Grenzen und abgeschlossene Cancel-Versuche
  nicht ohne konkreten Änderungsgrund erneut aufrollen.

**Empfohlene Reihenfolge:** A01/A02 als Dokumentationspaket; A06 als kleines
Gate-Zuverlässigkeitspaket; A03 als erster Refactoringpilot; anschließend A04
mit A05. A07/A08/A10 bei passenden Featurearbeiten; A09/A11 nach Bedarf.
A12 wird nach gewünschter nächster Fähigkeit einzeln ausgewählt. Die laufende
Architektur bleibt währenddessen nutzbar; kein großer Umbau vor neuen Features.

## Aktuelle Arbeit

Automatische Bretterketten-Vorprüfung begrenzt live abgeschlossen.
Simulation und Bau trotz Materialfehlmenge gestartet, Diagnose abgeglichen,
feste Zeitbudgets und Pause bestätigt. Kein weiteres Feature begonnen.
[Nachweise und Grenzen](docs/plank-chain.md#liveabnahme-der-automatischen-einbindung-2026-10-18).

## Aktuell offene Folgearbeiten

- Produktionsketten vor Bau gemeinsam planen: Zutaten, Lager/Transport, Personal
  und Kraft (B07): Bretterketten-Reader und automatische Startvorprüfung live abgenommen; weiterhin
  kein vollständiger oder automatischer Ausbauplan.
- Recovery: unbekannte Objektidentität deterministisch live nachweisen. Keine
  ungezielten Zeitrennen oder Save-Manipulation; Testmechanismus noch zu entscheiden.
- Parameterfehler bei Kartenabfragen um konkrete Kartengrenzen ergänzen.
- Weitere Zugangsgeometrien, insbesondere unterer Zielweg an gedrehter
  Bestandstreppe, gezielt live nachweisen.
- Projektwiederaufnahme nach Spielneuladen (B06) und allgemeine Kraft-/Höhenrouten
  (B03) bleiben eigenständige größere Arbeitspakete.

## Zuletzt abgeschlossen

- Automatische Bretterketten-Vorprüfung: Start bei 33/100 Brettern im Simulations-
  und 33/36 Brettern im Baupilot. Befunde stimmen mit Einzelabfragen überein;
  sechs bzw. eine Spielstunde, unveränderte 30-Sekunden-Grenze, Pause bestätigt.
  Sechs Testbaustellen bleiben unfertig; kein automatischer Produktionsausbau.

- Bretterketten-Vorprüfung: sechs Produzenten gegen Einzelabfragen abgeglichen;
  Pause/Personal, Kraftmangel und volle Ausgabe korrekt benannt. Begrenzte Messung
  endet nach einer Spielstunde mit Pause, ohne Ausbau oder Budgetverlängerung.
  Holzknappheit synthetisch geprüft; erfolgreiche Produktion hier nicht nachgewiesen.

- Fortschrittsdiagnose: Ausgangswerte, Materialwarten und Fertigstellung gegen
  Einzelabfragen live bestätigt. Zwei feste Zeitfenster stoppen ohne Verlängerung;
  Compact/Full erhalten die Messpunkte. Positive numerische Teildeltas weiterhin
  Gate-Nachweise. [Belege und Grenzen](docs/construction-progress.md).

- Bezirksweite Arbeitszeitdiagnose vom Nutzer abgenommen: Zentrum und Bauhütten
  gemeinsam; genaue unterschiedliche Schichten synthetisch geprüft. Keine weitere
  Live-Schichtprüfung offen. [Belege](docs/operating-readiness.md).
- Automatische Betriebsdiagnose vor längeren Läufen, Material ohne pauschale
  Startsperre; Gate und begrenzte Livepiloten bestanden.
- Notstopp/Statuswarten: eigenes Warten begrenzt, verbleibende Clientserialisierung
  eingegrenzt; kein offener Auftrag für weitere Transportversuche.

## Historische Nachweise und Entwicklungskatalog

Die folgenden Abschnitte dokumentieren damalige Zwischenstände. Formulierungen
wie „Gate offen“, „fehlgeschlagen“ oder „nächster Schritt“ gelten für den genannten
Zeitpunkt und werden durch die aktuelle Übersicht oben ersetzt. Der spätere
Entwicklungskatalog enthält zusätzliche Ideen, keine bereits beauftragten Arbeiten.


Bezirksweite Bauarbeitszeit **begrenzt live abgenommen**: Zentrum und
Bauhütten gemeinsam prüfen. Aktiver Job oder besetzte unpausierte Bauhütte in
Arbeitszeit verhindert gemeinsamen Feierabend; unbekannte Belege bleiben null.
Einzelner Feierabend am Zentrum ist keine Baupause im ganzen Bezirk.
[Vertrag, Grenzen und Pilot](docs/operating-readiness.md).



Arbeitszeitdiagnose erweitert, **Gate und Livepilot bestanden**: Feierabend am beobachteten
Arbeitsplatz wird konkret benannt. Frische Baudiagnose nutzt den Schichtstatus
des zuständigen Bezirkszentrums; unbekannte Werte und andere Blockaden bleiben
getrennt. Kein automatisches Verlängern von Laufbudgets.
[Vertrag und Pilot](docs/operating-readiness.md).


## Abschluss 2026-10-18 — automatische Betriebsdiagnose begrenzt live abgenommen

Gate: 1964 Unit- und 13 Integrationstests bestanden, drei optionale Live-Tests
übersprungen. Separater Auto-Reader nach Client-Neustart verfügbar: Auswahl von
DistrictCenter und zwei LumberMills ohne IDs; Diagnose bis auf Zeitstempel
identisch zur manuellen Gegenprobe. Simulation trotz 33 Prozent Deckung und
blockierter Produktion gestartet, nach sechs Stunden automatisch pausiert.

Baupilot: eine Lodge mit einem neuen Zugangsweg, gemeinsam nativ geplant,
pilotEligible und Vorschau wiederhergestellt. Vorprüfung im Compact-Startbeleg:
full_planned_costs_including_helpers, exakt 12 Holz laut Definitionsgraph
(Lodge 12, Weg 0), vier Holzfäller im angegebenen Bezirk automatisch gewählt.
Letzte Vorbeobachtung 23:27:14.579 UTC, Startbeleg 23:27:14.703 UTC.
Beide Aufträge anschließend bestätigt; Weg fertig und zugänglich, Lodge mit
vollständigem Materialbestand. Nach sechs Stunden/17,73 Echtzeitsekunden Pause,
Projekt game_time_limit, Lodge weiterhin awaiting_builders_or_work.
Keine beobachteten Bestandszugangsverluste. Kein aktiver Lauf/Tempoauftrag.

Abgenommen: automatische Auswahl, gemeinsame Diagnose, Kostenübernahme und
Vorprüfung vor Simulations-/Strukturstart. Materialunterdeckung live am
Simulationsziel belegt; der Baupilot hatte ausreichendes Holz. Kostenpflichtige
Zugangshilfen sind synthetisch geprüft, dieser Liveweg war kostenlos.
Keine Fertigstellungs-, Produktions- oder Liefergarantie aus Bereitschaft.
Die Lodge bleibt als Baustelle im pausierten Testspiel, das Projekt unreleased;
vor weiteren Baupiloten frisch diagnostizieren. Kein automatisches Zeitbudgetplus.
README, Einstieg, Backlog, Werkzeugvertrag und Agentenanleitung aktualisiert.
Keine Änderungen an globalen Skills oder Wiederaufnahme des Hermes-Experiments.

[Vertrag und Grenzen](docs/automatic-operating-readiness.md).

Aktueller fachlicher Stand: **0.74.0 plus Cancel-/Transport-Nacharbeit**,
Gate und begrenzter Livepilot bestanden. Grenzen siehe Abschlussbeleg unten.
Ältere Versionsabschnitte sind historische Nachweise.

## Cancel-Transport — Gate und begrenzter Livepilot bestanden (2026-10-18)

28 Supervisor-, 1936 Unit- und 13 Integrationstests bestanden; drei optionale
Integration-Livetests übersprungen. Codex vollständig neu gestartet, neuer
Supervisor-Werkzeugtext und Bridge 0.74.0 bestätigt. Kein Agent-Build/Testlauf.

Anforderung waitSeconds=30: tatsächlich 1,002 Sekunden internes Warten,
1,175 Sekunden bis Antwort. Cancel 200 ms nach Beginn bei noch offenem
Status-Promise angefordert: Antwort nach 2,986 Sekunden (zuvor 31,13 Sekunden).
Zunächst pausing/cancel_requested, anschließend frisch cancelled, terminal=true,
pauseConfirmed=true und observedSpeed=0. Kein aktiver Lauf/ausstehender Tempoauftrag.

Zeitmarken (Europe/Berlin, 18. Oktober): Status empfangen 00:22:19.364,
weitergeleitet 19.394, Backendantwort 20.484, Clientantwort 20.499.
Cancel erst empfangen 00:22:22.393, weitergeleitet 22.409, Backendantwort 22.493,
Clientantwort 22.509. Damit liegt die beobachtete Serialisierung vor dem
Supervisor: rund 1,894 Sekunden zwischen Statusantwort und Cancel-Eingang;
im Supervisor/Backend nur 116 ms vom Cancel-Eingang bis zur Antwort.
Die genaue interne Client-/Tooltransport-Komponente ist nicht weiter aufgelöst.

Abgenommen ist die Begrenzung des eigenen Statuswartens und der anschließende
bestätigte Stopp im bestehenden Codex-Pfad, keine echte parallele Zustellung
oder feste End-to-End-Latenzgarantie. Der parallele Backend-Cancel-Pfad ist
synthetisch geprüft; er wurde durch diesen seriellen Livepfad nicht ausgeübt.
Die Projektstatusvariante der Begrenzung ist Gate-, kein separater Live-Nachweis.
Andere lokale Dokumentationsänderungen, portable Teststarter und historischer
Entwicklungsbericht auf Nutzerauftrag einbezogen; keine Rohlogs oder privaten
Konfigurationen. Anleitung/Toolvertrag aktualisiert, Hermes-Skill bleibt entfernt.
Die folgenden Pilot-/Vorbereitungsabschnitte sind historische Nachweise.

## Transport-Nacharbeit 2026-10-18 — neues Gate und Frontend-Neustart erforderlich

Lokales MCP-Log des isolierten Piloten: Statushandler begann 22:01:49.387 UTC
und endete 22:02:19.434 UTC; Cancelhandler begann erst 22:02:22.152 UTC und
endete 22:02:22.227 UTC. Die zusätzliche Verzögerung liegt vor InvokeLogged,
nicht an der korrigierten NativeTools-Sperre. Der bisherige Supervisor-Quellcode
verarbeitet Requests bereits in getrennten Threads. Ohne Ankunftszeitmarken ist
die genaue Grenze Client/Tooltransport → Supervisor → SDK noch nicht bewiesen.

Vorbereitet: Der Codex-Supervisor begrenzt gültige Statuswarteparameter auf eine
Sekunde. Der Backendvertrag für direkte Clients bleibt unverändert; keine
allgemeine Parallelfreigabe oder Garantie externer Zustelllatenz. Der Tooltext
erklärt die Begrenzung. Ungültige Werte bleiben unverändert zur Validierung;
keine automatischen Folgeabfragen, kein Retry. Zeitmarken für client_received,
backend_forward, backend_response und client_response: maximal 128 Einträge
pro Supervisorinstanz, nur Zeit/Ereignis/Methoden-/Werkzeugname. Keine Argumente,
Antwortinhalte, Sitzungen oder Tokens. Lokal ignoriert, Diagnosefehler blockieren
keine Aktionen. Neue Supervisor-Regressionen vorbereitet, noch nicht ausgeführt.

Auf Nutzerauftrag übrige lokale Dokuänderungen und historische Entwicklungsnotiz
einbezogen. Neun Startervarianten auf gemeinsame portable Startlogik umgestellt;
keine fest eingebauten Benutzerpfade. start-native aktiviert Testaktionen,
-ReadOnly deaktiviert sie, -DisableValidation nur die separate Validierung.
Native Auftragsprüfungen bleiben aktiv. Bestehende private Konfiguration bleibt.

Nächste Abnahme: menschliches Gate, Codex vollständig beenden/neu starten (neuer
Supervisorcode!), Spiel laden, live bereit. Ein 30-Sekunden-Statuswunsch muss nach
höchstens einer Sekunde internen Wartens zurückkehren; danach expliziter Cancel
mit frischer Pausebestätigung. Vor-/Nachlauf-Transportlatenz separat messen.
Zeitmarken zeigen, ob Cancel erst nach client_response ankommt (vor Supervisor)
oder früher empfangen/weitergeleitet, aber verspätet verarbeitet wird.
Kein Commit/Push vor bestandenem Gate und End-to-End-Livepilot.

## Cancel-Livepilot 2026-10-18 — Gate bestanden, End-to-End-Abnahme fehlgeschlagen

Menschliches Gate bestätigt; Gate-Log: 1936 Unit-Tests und 13 Integrationstests
bestanden, drei optionale Integration-Livetests übersprungen. Backend über den
verbundenen Supervisor gestartet; Bridge 0.74.0 und frische Session bestätigt.

Erster Kontrollfall: 30-Sekunden-Statuswarten kehrt nach 30,35 Sekunden zurück;
der währenddessen angeforderte gewöhnliche Schreibaufruf wird erst danach
ausgeführt statt wegen Konkurrenz abgewiesen. Dieser Tempoaufruf unterbricht
den Testlauf, daher kein gültiger paralleler Cancel-Beleg. Explizit pausiert.

Isolierter Folgefall ohne Kontrollmutation: Status mit waitSeconds=30 gestartet,
Cancel nach zwei Sekunden bei noch offenem Status-Promise angefordert. Status
antwortet nach 30,32 Sekunden mit wait_limit; Cancel erst nach 31,13 Sekunden
ab seiner Anforderung, zunächst pausing/cancel_requested. Anschließend frisch
cancelled, terminal=true, pauseConfirmed=true, observedSpeed=0 bestätigt.
Endzustand: Tempo 0, kein aktiver Lauf, kein ausstehender Tempoauftrag.

Damit keine End-to-End-Freigabe: die Aufrufe erreichen den wirksamen Dispatch
im verbundenen Aufrufweg offenbar weiterhin nacheinander. Welche Schicht
(Client/Tooltransport, Supervisor-Zustellung oder MCP-SDK) serialisiert, ist
noch nicht belegt. Die Backend-Unit-Regressionen ersetzen diesen Nachweis nicht.
Keine weiteren Livefälle, kein Commit/Push. Nächster Schritt: Ankunftszeiten
an Supervisor und Backend vergleichen; danach gezielte Korrektur oder eine
ehrliche Begrenzung langer Statuswartefenster. Keine zusätzlichen stdio-Prozesse
als vermeintlicher Nachweis der bestehenden Codex-Verbindung starten.

## Journal-Korrekturen — Gate und gezielter Livetest bestanden

Menschliches Gate: 23 Supervisor-, 1931 Unit- und 13 Integrationstests bestanden;
drei optionale Integration-Livetests übersprungen. Bridge weiterhin 0.74.0.
Einzelkiefer markiert und geschützt, jeweils completed und genau ein bestätigtes
Ziel. Alte Markierungs-ID nach Schutz erneut gelesen: Kiefer blieb unmarkiert.
Mit neuer Action wieder markiert; alte Schutz-ID nach Backend-Neustart und
vollständiger Client-Neuverbindung erneut gelesen: Markierung blieb bestehen.
Damit keine erneute Spielmutation in beiden Gegenrichtungen beobachtet.
Abschluss durch neue Schutz-Action, readbackConfirmed=true. Endtempo 0, kein
aktiver Lauf oder Tempoauftrag; Spielzeit unverändert.
Supervisor zunächst noch alter persistenter Prozess. Nach menschlicher
Neuverbindung lagen die Elternprozess-Startzeiten nach der Skriptänderung.
Stop/Status bestätigten running=false/gate=true; Start running=true/gate=false,
Spielsession erhalten. Windows-Austauschfehler synthetisch im Gate geprüft,
nicht künstlich live erzeugt. Keine Live-Unterbrechung eines laufenden
Schreibauftrags; deren Nichtwiederholung bleibt Gate-Nachweis.
Learnings und Grenzen in docs/releases/0.74.0.md; Anleitung, Tool-Doku und
zurückgestellter Skillentwurf aktualisiert. Korrekturen fachlich abgeschlossen; Git-Sicherung: Journal-Korrekturen `c09750f`, ursprünglicher 0.74-Abschluss `680fd00`.

## 0.74.0 — Gate und gezielter Livetest bestanden

Menschliches Gate: 21 Supervisor-, 1929 Unit- und 13 Integrationstests bestanden, drei optionale
Integration-Livetests übersprungen. Bridge 0.74.0 in neuer Session bestätigt.
Materialwarten trotz repairEligible=true empfiehlt check_material_supply.
Gezielt entfernter Bestandsweg erzeugt zwei belegte Anschlussverluste und
review_access_repair_plan; Einzelbefund Materialwarten bleibt erhalten.
Weg regulär wiederhergestellt, Plattform 1/1 fertig und zugänglich; Empfehlung
review_close_finished_project trotz historischem game_time_limit. Compact/full
und explizite Watch-Diagnose bestätigt. Beide Journale freigegeben, null
Bestandsverluste, Tempo 0, kein aktiver Lauf oder ausstehender Tempoauftrag.
[Livebelege und Grenzen](docs/releases/0.74.0.md). Anleitung, Tool-Doku und
zurückgestellter Hermes-Skillentwurf mit Livebelegen aktualisiert; kein Skill
installiert. Keine Agent-Builds/Tests. Version 0.74.0 fachlich abgeschlossen; Git-Sicherung durch den Abschlusscommit.

Ergänzender Livetest: isolierte Plattformbaustelle nativ buildersReachable=false,
construction_unreachable, keine unfertigen Stützen und lostConnections=0.
Compact/full und Watch: observed_access_problem, accessProblemPartIndices=[0],
repairEligible=true, review_access_repair_plan. Testbau regulär entfernt und
Journal freigegeben; keine Baustellen, Endtempo 0. Unbekannte Identität bleibt
live offen: reguläre Werkzeuge ändern keine Objektidentität; automatische
Bestätigung verhindert einen deterministischen unbestätigten Abrissfall.
Bekannter Abriss wurde korrekt als previously_observed_object_removed erkannt.

## 0.73.0 — Gate und gezielter Livetest bestanden

Menschliches Gate: 21 Supervisor-, 1908 Unit- und 13 Integrationstests bestanden;
drei optionale Integration-Livetests übersprungen. Bridge 0.73.0 live bestätigt.
limit=100 und depth=5 melden den konkreten Bereich mit not_applicable;
korrigierte Abfragen funktionieren. Einzelplattform nach einer Spielstunde
blocked/game_time_limit und frisch 0/1 fertig; nach separaten zwölf Stunden
frisch 1/1 fertig und zugänglich bei unveränderter Historie. Compact/full,
explizite watch-Diagnose und rein historische Journalansicht bestätigt.
Keine beobachteten Bestandsverluste; Journal regulär freigegeben, Tempo 0,
kein aktiver Lauf. [Livebelege und Grenzen](docs/releases/0.73.0.md).
Anleitung, Tool-Doku und zurückgestellter Hermes-Skillentwurf aktualisiert;
Workflow unverändert. Keine Agent-Builds/Tests. Version 0.73.0 fachlich abgeschlossen; Git-Sicherung durch den Abschlusscommit.

## 0.72.0 — Gate und vollständiger Plattform-Umwegpilot bestanden

Abschluss der seit 0.67 entwickelten Zugangsverbesserungen: automatische
Umwegsuche im sicheren Stand mit 191 Zuständen; 15/15 Teile (zehn Plattformen,
drei Wege, zwei Lager) fertig und zugänglich. Beide Lagereingänge und native
Verbindung zum Distriktzentrum bestätigt; untere Bestandswege trotz Überbau
weiter verbunden. Bestandsdiagnosen durchgehend lostConnections=0.
Journal regulär freigegeben, keine Baustellen, Endtempo 0, kein aktiver Lauf.
Zweites vorhandenes Sägewerk für die Brettlieferung aktiviert und besetzt.
Baupause/Stützwarten, beobachtete Mindestbesetzung und Menü-Sperre/Entsperre
bereits live bestanden. Menschliches Gate bestanden, keine Agent-Builds/Tests.
[Umfang, historische Vorläufe und Abschlussbelege](docs/releases/0.72.0.md).
Anleitung, Tool-Doku und zurückgestellter Hermes-Skillentwurf aktualisiert;
Workflow geprüft. Skill bleibt zurückgestellt. Abnahme für den dokumentierten
Umfang abgeschlossen; Code und Nachweise zur Git-Sicherung freigegeben.

Grenzen: neuer unterer Zielweg einer gedrehten Bestandstreppe noch kein eigener
Livefall; keine allgemeine Such-/Bauphasengarantie. Zielgebäude kann vor seinem
Zugang fertig sein. Weganschluss ist kein Nachweis trockenen Betriebs.

## In 0.74 gezielt live bestanden — repairEligible und Reparaturempfehlung trennen

Ausgangspunkt 0.73: Im Livepilot war bei
materials_not_ready und lostConnections=0 bereits repairEligible=true samt
review_access_repair_plan sichtbar. Das beschreibt eine mögliche Operation,
nicht einen nachgewiesenen Reparaturbedarf.

- Technische Reparaturmöglichkeit beibehalten, tatsächlichen Reparaturbedarf
  anhand frischer Zugangsbelege separat ausweisen; unknown nicht als fehlenden
  Zugang behandeln.
- Bei reinem Material-/Stützwarten oder Personalproblem den passenden nächsten
  Schritt nennen. Kein automatischer Zugangsbau allein wegen repairEligible.
- Live bestanden: Materialwarten, belegter Bestandsverlust, Wiederherstellung
  und vollständiger Fertigbau sowie isolierte unerreichbare Baustelle. Unbekannter
  Zugang/Identität bleiben Live-Nachweisgrenzen; Gate-Regression vorhanden. Historischen
  Laufabbruch, aktuellen Bauzustand und Journalfreigabe weiter getrennt halten.
- Learning aus 0.73: Fähigkeitsflag ist keine Handlungsempfehlung. Belege und
  konkrete Gründe müssen den nächsten Schritt bestimmen.

## Offener Recovery-Nachweis nach 0.74

Unbekannte Identität benötigt einen deterministischen, ausdrücklich als injiziert
gekennzeichneten Entwicklungsfall oder einen echten passenden Laufzeitfehler.
Keine wiederholten Zeitrennen zwischen Platzierung und Abriss: Legacy-Projekte
beobachten automatisch auch bei Pause. Kein Save-/Ledger-Patching als Livebeleg.
Bei möglichem Testmechanismus native Sperren sowie repairEligible=false und
closeEligible=false mitprüfen; Präsentationsfixtures allein belegen diese nicht.
Begrenzte Fehler-Injektion ist ein Vorschlag, noch nicht implementiert/freigegeben.

Zusatzbefund: inspect_map_region außerhalb der Karte lieferte nur generisches
invalid_argument. Konkrete Kartengrenzen fehlen in dieser Fehlerausgabe; separat
präzisieren, nicht mit unbekannter Objektidentität verwechseln.

## Weitere Verbesserungen

- Betriebs-/Überflutungszustand des Bauzentrums und der Lieferkette vor längeren
  Bauzeitfenstern bündeln; connected allein reicht nicht.
  Erledigt im begrenzten Umfang: `inspect_operating_readiness` für ausgewählte
  Gebäude; Gate und Einzelabgleich live bestanden. Keine Kolonievollabdeckung. [Vertrag](docs/operating-readiness.md).
- Unteren neuen Zielweg an gedrehter Bestandstreppe separat live prüfen.
- Parameterdiagnose und Trennung Zeitlimit/Fertigbau sind mit 0.73 erledigt;
  keine erneute Umsetzung dieser Punkte.

## 0.71.0 — Gate, Anschlussprüfung und Umwegplatzierung bestanden; Fertigbau offen

Bridge 0.71 und neue Session bestätigt. Oberer Treppenanschluss sowie seitliche
Negativkontrolle bestanden. Zwei Anschlusswege fertig, realer Bauzugang belegt.
Automatische Umwegvorschau um ein mitgeplantes Lager: 819 Suchzustände,
17 Teile, native Eingänge und Arbeitszugänge bestätigt; 3477 Bestandsprüfungen
ohne Verlust. Alle 17 Objekte einmal platziert und bestätigt. Geprüfte reale
Baustellenzugänge erreichbar, Bestandsdiagnose weiter ohne Verlust.
17 Baustellen bleiben, Endpause bestätigt; Fertigbau und unterer neuer
Treppenzielweg noch nicht abgenommen. Kein Commit/Push.
[Belege, Grenzen und Learnings](docs/releases/0.71.0.md).
Nächste Diagnosekorrektur: construction_paused trotz paused=false bei unfertigen
Trägern; Inaktivität, manuelle Pause und Stützabhängigkeit trennen. Nullbesetzung
am Holzfäller unbestätigt; minimale Besetzung und Zeitsperrstatus sichtbar machen.
Anleitung, Tool-Doku und zurückgestellter Hermes-Skillentwurf aktualisiert.
Cockpit-Gate erfolgreich; Darstellung nicht gesondert visuell abgenommen.
Backend verbunden, Startsperre frei. Baustellen nicht erneut beauftragen.

## 0.70.0 — Bestandstreppen-Höhenfall bestanden, erweiterter Umweg offen

Instant-Netz und Vorschau mit Zielweg getrennt: gedrehter oberer Austritt
von Folktails-Bestandstreppen nur als Kandidat; native Zielweg-/Stützenvorschau
bestätigt Distriktanschluss und Wiederherstellung. Keine ausgelesene direkte
Treppenkante behaupten. Maximal acht Proben, bestehende Suchlimits unverändert.
Diagnose Version 3 zeigt Instant-Kante und Vorschauergebnis separat.
[API-Befund, Learning, Grenzen und Pilot](docs/releases/0.70.0.md).
Regressionen und Versionsprofile ergänzt; Quell-/Diffprüfung, keine Agent-
Builds/Tests/Installation. Gate und bekannter Höhenfertigbau bestanden; erweiterter Umweg nicht abgenommen, kein vollständiger Abschluss/Commit/Push.
Anleitung, Toolbeschreibung und lokaler Hermes-Skillentwurf aktualisiert;
Entwurf bleibt zurückgestellt. Workflow geprüft, unverändert.

## 0.69.0 — Gate bestanden, Höhenpilot erneut fehlgeschlagen

Bestandstreppen werden mit AreConnectedRoadInstant statt AreConnectedInstant
geprüft. Diagnose Version 2 kennzeichnet den Weggraph und zeigt die allgemeine
Abfrage getrennt; kein Ersatz nativer Freigabe durch Geometrie. Regressionen
und Versionsprofile ergänzt. Höhenpilot: beide Graphabfragen am Treppenaustritt false; 2048 Zustände ausgeschöpft. Manuelle Zwei-Wege-Gegenprobe gültig, Bestandsverbindungen erhalten.
MCP-Log öffnet sich nach dem Laden automatisch; bleibt manuell schließbar.
[Änderungen, Learning und Abnahmeplan](docs/releases/0.69.0.md).
Quell-/Diffprüfung; keine Agent-Builds, Tests oder Installation. Kein Commit/Push.
Spielanleitung, Toolbeschreibung und lokaler Hermes-Skillentwurf gepflegt;
Skill bleibt zurückgestellt. Entwicklungsworkflow geprüft, unverändert.

## 0.68.0 — Gate bestanden, Höhenanschluss weiterhin fehlerhaft

Zielpriorisierung und begrenzte Diagnose der Bestands-/Treppenanschlüsse aus
0.67-Learning umgesetzt. Budgets unverändert. Zuerst bekannter Höhenfall,
bei Erfolg Umweg und tatsächlicher Bau mit Bestandskontrolle. Bei erneutem
Fehlschlag Diagnose auswerten und stoppen. [Abnahmeplan](docs/releases/0.68.0.md).

## 0.67.0 — Gate bestanden, Suchkorrektur noch nicht live abgenommen

P1-Eigenkollisionen und Nutzung vorhandener Treppen aus dem 0.66-Folgepilot
implementiert: Geometrie je Suchpfad, alternative Belegungen je Knoten und
nativ bestätigte Höhenanschlüsse mit regulärem Zielweg. Regressionen ergänzt.
Gate bestanden. Gleicher Höhenfall scheitert bei 2048 Suchzuständen, obwohl
manuelle Zwei-Wege-Gegenprobe gültig bleibt. Zielpriorisierung der geometrischen
Suchzustände und Diagnose bestätigter/abgelehnter Treppenaustritte fehlen.
Kein Bauauftrag, Bestandszugänge erhalten. Erst diesen Kontrollfall lösen,
dann Bauphasen-/Fertigzugangspilot; Limits nicht pauschal erhöhen.
Die folgenden 0.66-Befunde bleiben Herkunft der Änderung.
[Gateplan](docs/releases/0.67.0.md).

## 0.66.0 — Kernpilot bestanden; verbleibende Nachweise

Uferpumpen-Terraincheck berücksichtigt `Block.IsOccupied`; Laufstart-Ablehnungen
haben konkrete Codes und gezielt `not_executed`. Uferpumpe bis Fertigzugang,
Tempoabweichung, konkurrierender Start und gültiger Folgelauf live bestanden.
**P1 Höhen-/Umwegsuche:** Folgepilot findet einen selbstkollidierenden Plan
(Treppe und spätere Plattform auf derselben Zelle), obwohl eine native gültige
Gegenprobe mit zwei Wegen über die bestehende Treppe vorliegt. Suchzustand
berücksichtigt bisher nur Zielzelle/Kosten, nicht Geometrie des bisherigen
Routenpfads. Kantenpakete gegen Vorgängerkanten prüfen und zulässige Alternativen
bei unterschiedlicher Belegung erhalten. Vorhandene Treppenanschlüsse separat
prüfen: Quellen stammen bisher ausschließlich aus `MainPathCoordinates`.
Keine erfundene Verbindung; gültige Anschlüsse nativ bestätigen.
Regression: erhöhtes Ziel hinter Hindernis, Eigenkollision verworfen,
gültiger Umweg gefunden, Bestandsanschlüsse vor/während/nach Bau erhalten.
Native Schlussvorschau hat den Fehler korrekt abgefangen; nicht lockern.
Details und Nachweisgrenzen im Releasebericht.

Navigations-Baselinekonflikt bei
Wiederauftreten isoliert diagnostizieren. [Belege](docs/releases/0.66.0.md).
Die nachfolgenden 0.65-Befunde bleiben Herkunft und Live-Abnahmekriterien;
Cancel-Priorisierung ist nicht Teil dieser Version.

## 0.65.0 — begrenzt live abgenommen; Folgearbeiten

Rückstauszenario unter Sperrkrone, Abflussunsicherheit und kontaminiertes Zielwasser
sowie native Flooded-/Ansaugdiagnose implementiert; Bridge nach `live bereit`
verbunden. Rückstaupflicht und trockene Ansaugung live bestätigt; selbst gebaute
Kontrollpumpe hat einen nativen Produktionszyklus geliefert. Überflutungsfall
auch bei Gebäudepause positiv bestätigt; höhere Ersatzpumpe produziert.
[Nachweisplan und Grenzen](docs/releases/0.65.0.md). Kein vollständiges Hydraulikmodell.
Reserve-/Gefahrenstopps, Abflusskapazität und automatische sichere Umleitung bleiben
offen. Learnings: Kartenrandkontakt ist kein Auslassbeleg; Pause kann UI-Flutstatus
verdecken; natives Flooded vor Umverteilung von Versorgungsarbeitern prüfen.

Neue konkrete Befunde aus dem 0.65-Pilot:
- **P1 Notstopp während Statuswarten — Korrektur vorbereitet, Gate/Live offen:** Die Sperre liegt in NativeTools, nicht im parallel weiterleitenden Vorschaltprozess. Der bisherige Backendpfad serialisiert
  MCP-Aufrufe. Ein laufendes `inspect_simulation_run(waitSeconds=30)` blockiert
  auch Cancel mit `not_executed`. Nach Ende des Leseaufrufs funktionierte Cancel
  samt Pause. Eng begrenzter Cancel-Pfad während reinem Statuswarten vorbereitet;
  bis dahin Spiel-MCP seriell und bei Nutzersteuerung kurze Wartefenster nutzen.
- **P1 Zugangssuche/Geometrie:** `plan_large_project_access` verwirft eine
  Uferpumpe wegen Terrainüberschneidung. Dieselbe Position besteht native
  Einzel- und Gesamtvorschau und ist anschließend regulär fertiggebaut und
  produktiv. `SpatialObservations` hat denselben pauschalen Terraincheck.
  Native Belegungsregeln leerer/optionaler Pumpenzellen prüfen; keine pauschale
  Lockerung für echte Hindernisse. Eine Ebene höher erzeugt die Suche unnötige
  Plattformen. Kontrollfall für nächsten Fix: gültige Uferpumpe plus echte
  Terrainkollision, Objektkollision und nachgewiesener Bau-/Betriebszugang.
- **P1 Lauf-Fehlerweitergabe:** Starts meldeten generisch
  `state_conflict`/`unconfirmed`, danach `run_not_found`. Ein späterer Fall zeigte
  frisch Tempo 7 statt erwartetem Tempo 0. Batchbesitz ist als Ursache des
  früheren Falls nicht bewiesen; terminale Chargen geben ihren Besitzer frei.
  Frühe Ablehnung von unbestätigtem Transport trennen und konkreten Tempo-,
  Sperr- oder Besitzerkonflikt nennen. Nach frischer Diagnose eine neue Aktion
  planen, unbestätigte Starts nicht blind wiederholen.
- **Baukosten und Abschluss:** Höhenzugang benötigte zwölf Plattformen und
  drei Treppen. Materialbedarf samt Produktionszeit vor dem Auftrag anzeigen.
  Nach abgelaufenem Bauzeitfenster frische `diagnose_large_project` verwenden;
  eingefrorene Chargenzähler zeigen spätere Fertigstellung nicht.

## Fehler aus dem Überlebenslauf — Stand nach 0.64.0

Die drei P0-Korrekturen sind in [0.64.0](docs/releases/0.64.0.md) begrenzt live
abgenommen: Höhenbau bis Fertigzugang, stabile Pause und Journalfreigabe nach
Rückbau mit Folgeauftrag ohne Reload. Menschliches Gate laut Nutzer bestanden.
Der konkrete Ursprung des damaligen run_not_found bleibt offen; Antwortverlust
und Rennen innerhalb desselben Updates sind synthetische Gatefälle.

Kolonieverlust vom Nutzer gemeldet; Schutzmauer nicht fertiggestellt.
Keine hydraulische Schutz- oder autonome Überlebensabnahme. Priorisierte Folgearbeit:

- **Offener Diagnoseumfang:** bei erneutem unconfirmed/run_not_found Transport,
  Session und Laufregistrierung getrennt belegen. Kein Wiederholen aus bloßer
  Abwesenheit einer Kennung. Neue Felder activeRunId/pendingSpeed dafür nutzen.
- **Weitere Livegrenzen:** Game-Lock-Abbruch, Antwortverlust, erst nach terminalem
  Stop bestätigte Identität und separat ausgelöster lostConnections-Guard in
  dieser Version nicht live provoziert; vorhandene synthetische Regressionen.
- **P1 Wasserbau und Versorgung:** sichere Abflussroute, Rückstau und Umgehung
  getrennt von gültiger Geometrie bewerten. Pumpendiagnose um native Flooded-
  und Ansaugblockaden ergänzen; Reserve-/Gefahrenstopps für Zeitläufe. Kein
  Schutzversprechen aus Geländescan oder fertiggestellter Pumpe ableiten.
- **P1 vollständiger Zugang:** Räumung, offener Wasserübergang, Treppen, Kosten
  und Bauzeit gemeinsam planen. Plattformen kosteten hier sechs Bretter je Teil;
  Material aus aktuellem Katalog statt Erinnerungswerten. Markierte Bäume ohne
  erreichbaren Bauweg räumen sich nicht durch zusätzliche Simulationszeit.

Details und weitere Abnahmekriterien: [Pilotjournal](docs/survival-run-2026-10-17.txt).
Die verbleibenden Wasserbau-/Zugangsplanerarbeiten sind nicht implementiert oder
abgenommen. Learnings aus 0.64: gemeinsames Testfenster ohne parallele Nutzerbauten,
Materialbedarf aus Katalog, Quittung und frische Diagnose getrennt lesen.

## Bauplaner und Fehlerweitergabe — begrenzt live abgeschlossen

Priorität P0 aus dem Überlebenspilot vom 17. Oktober. Menschliches Gate und
15-Teile-Baupilot bestanden; [Nachweise und Grenzen](docs/planner-repair-2026-10-17.md).
Weitere Überlebens-Learnings stehen in `docs/survival-run-2026-10-17.txt`;
Wasserdiagnose, Ereignisstopps und Transportqueue sind nicht Teil dieses Fixes.

### Neue Learnings aus dem Baupilot

- Ingame-Fortschrittstext korrigieren: `inspect_large_project` meldete 0/15,
  strukturierter Bericht dagegen 14/15 fertig. Abnahme: laufende/terminale
  Chargenberichte korrekt zählen und Ledgerbeobachtung von frischer Diagnose trennen.
- Eingabeschema und Fehlerdetails präzisieren: unbekannte Vorlage bzw. nicht-UUID
  als actionId liefern nur `invalid_argument`. Feld und zulässiges Format nennen;
  UUID-/Enum-/Zahlengrenzen müssen auch in der Client-Werkzeugdarstellung ankommen.
- Suchbudget bleibt Qualitätsgrenze: acht Vorschauen fanden zwei Gebäude,
  16 fanden alle drei mit zwölf Umweg-Wegen. Vollständigkeit und Wegkosten
  getrennt berichten; kein Nachweis einer optimalen Route.

## B15: Gesamtstruktur — 0.63 mit dokumentierter Grenze abgeschlossen

**Ursprünglich P0, Nutzerauftrag vom 16. Oktober 2026.** Abschluss am 17. Oktober
mit ausdrücklicher Nutzerfreigabe der unten dokumentierten Nachweisgrenze.
Menschliches Gate bestanden; kein offener Implementierungsblocker dieses Umfangs.
Ebener Gesamtauftrag, Zeitlimit, Reparaturabschluss und Schema-2-Gebäudecharge live
belegt; manueller Höhenstart setzt auch oberen Weg auf unfertiger Plattform sofort.
Separater Vertikalpilot nach erneutem Gate mit fünf gemeinsamen Aufträgen bestanden;
ebener Dreiteilepilot fertig, konkrete Identitätsablehnung und Objektverlust ohne
Replay geprüft. Teilablehnung mitten im Durchgang nur synthetisch im Gate belegt;
begrenzter Versionsabschluss ausdrücklich vom Nutzer freigegeben. Zusätzlicher
nativer Livefehler und vollständiger Höhenfertigbau bleiben weitere Nachweise.

Gewünschter Ablauf: **Plan machen → einmal Go → komplette Struktur als
Bauaufträge platzieren → Baufortschritt verfolgen → erfolgreich beenden, wenn
alles gebaut ist.** Der Agent beschreibt das gesamte Bauvorhaben; MCP übernimmt
die gemeinsame Platzierung und meldet konkrete Probleme zurück. Die Biber bauen
die platzierten Baustellen anschließend nach und nach. MCP wartet nicht nach
jedem einzelnen Element auf dessen Fertigstellung, bevor es das nächste setzt.

- **Ein Gesamtplan:** Gebäude, Wege, Treppen, Stützen, Kraftverbindungen und
  Wasserbau einer Struktur gemeinsam beschreiben und prüfen. Geometrie,
  Distriktanschluss, Bauarbeiterzugang, fertiger Gebäudezugang und Bauphasen
  getrennt bewerten, einschließlich der Abhängigkeiten innerhalb des Plans.
- **Ein Go für die ganze Platzierungsphase:** Alle vom Spiel zulässigen Teile
  unmittelbar als Bauaufträge einreichen. Interne API-Aufrufe dürfen technisch
  sequenziell sein; zwischen ihnen keine Baufortschrittswartezeiten, keine
  Agentenrunde und keine erneute Einzelbestätigung. Klarstellung des Nutzers:
  erst sämtliche Aufträge einreichen, DANACH prüfen; auch keine Material-,
  Zugangs- oder Initialisierungsprüfungen zwischen den Platzierungen. Noch fehlendes Baumaterial
  ist kein Grund, weitere vom Spiel erlaubte Baustellen zurückzuhalten.
- **Echte Spielgrenzen konkret zurückmelden:** Verlangt das Spiel für ein Teil
  tatsächlich eine fertiggestellte Stütze, Räumung oder andere Vorleistung,
  dies als belegte Ausnahme mit betroffenem Teil und Abhängigkeit ausweisen.
  Keine pauschale Serialisierung des gesamten Vorhabens daraus ableiten.
  Keine vorsorglichen Fertigstellungsbarrieren aus dependsOn ableiten. Erst alle
  Teile versuchen; tatsächliche native Ablehnungen danach konkret ausweisen.
  Unbestätigte Aufträge niemals automatisch wiederholen.
- **Ein Platzierungsbericht:** Pro Teil geplant / platziert / abgewiesen /
  zurückgestellt / unbestätigt mit Objektzuordnung, Koordinaten, konkretem
  nativen Fehler und erforderlicher Korrektur. Teilerfolg offen ausweisen;
  keine vorgetäuschte atomare Alles-oder-nichts-Transaktion und kein blindes
  Wiederholen unbestätigter Aufträge.
- **Ein Lauf bis zum tatsächlichen Bauabschluss:** Gesamte Charge unter einer
  Auftrags-ID beobachten. Fortschritt und Blockaden wie Materialmangel,
  fehlende Bauarbeiter oder verlorener Zugang gesammelt melden. Erfolgsende
  erst, wenn alle beauftragten Teile nachweislich fertig sind. Platzierung
  allein ist kein Abschluss. Abbruch, Zeitlimit und dauerhaft blockierte Teile
  sind gesonderte End-/Fehlerzustände mit offenem Rest, niemals Erfolg.
  Zugang, Kraftanschluss und Betrieb zusätzlich getrennt ausweisen.

**Abnahme:** Eine mehrteilige Struktur wird nach einem Go vollständig als
Baustellen sichtbar, bevor das erste Teil fertig ist; der Agent muss keine
Elemente einzeln weiterreichen. Ein Kontrollfall mit ungültigem Teil liefert
einen präzisen Teilfehler und den tatsächlich platzierten Rest. Ein Fall mit
Materialmangel bleibt nach kompletter Platzierung in Bau und endet erst nach
Nachschub/Fertigstellung erfolgreich. Native Vorleistungsabhängigkeit und
unbestätigte Platzierung jeweils separat prüfen.

Bestehende Gruppen-, Großprojekt- und Bauchargenwege auf diesen Ablauf umbauen,
statt lediglich einen weiteren Wrapper um die bisherige Fertigstellungssequenz
zu setzen. Verknüpft B02 (Gesamtplanung), B03 (Kraft/Höhen), B07 (Versorgung)
und B14 (zusammenhängende Schutzstrukturen).

## 0.54.0 - Pflanzen- und Flaechenjournal begrenzt live abgenommen

Menschliches Gate und Abschluss-Livetest am 12. Oktober 2026 bestanden.
Pflanz-/Faellmarkierungen, reale Pflanzenwechsel und Baum-/Stumpfunterscheidung
mit ehrlicher Herkunft; Compact/Full, Filter und Cursor live bestaetigt.
Menschliche Eingriffe bleiben unknown. Neue Sitzung startet ohne Altbestands-
ereignisse; alte Session wird abgewiesen. Produktionsregression bestanden.
Grenzen: abgetastete Zustandswechsel, keine lueckenlose Ereignis-/Erntehistorie.
[Vertrag und Livebelege](docs/spatial-journal-0.54.md).


## 0.53.0 - Sammelpflanzung begrenzt live abgenommen

Menschliches Gate bestanden; nativer Pilot am 12. Oktober 2026 bestaetigt.
Bis acht Rechtecke je 8x8, gemeinsame native Vorpruefung mit Arbeitsreichweite,
Feuchte und Belegungsart. Zwei Rechtecke mit acht Zellen live geprueft:
zwei Markierungen gesetzt, vier passende belassen, fremdes Feld und Weg ausgeschlossen.
Falsche Pflanzenart/Hoehe und veraltete Vorschau abgewiesen; Zustand frisch gelesen.
Compact/Full gleiche Summen; Testmarkierungen wiederhergestellt, Endpause bestaetigt.
Nachweis gilt fuer Markierungen, nicht Wachstum; 512-Zellen-Grenze automatisiert
getestet, nicht als grosser Liveauftrag. Kleine 0.52-Nacharbeiten im Testgate geprueft.
Nacharbeiten zu Ablehnungsstatus und Compact ebenfalls live abgenommen.
Fruehere sporadische Gatefehler bleiben ohne bewiesene Ursache.
Kein allgemeiner Aquakultur- oder anderer Fraktionsnachweis.
[Vertrag und Livepilot](docs/planting-batch-0.53.md).

## 0.52.0 begrenzt live abgenommen — Flächenräumung und Gruppenbau

Zusammenhängender Hintergrundablauf: Pflanzmarkierungen entfernen, bis 64
Vegetationsziele räumen, auf echte Fertigstellung warten, frisch gemeinsam planen
und bis acht Gebäude samt Wegen autonom bauen. Eine Fläche bis 8×8 auf einer Höhe;
Rohstoffstapel ignorieren und niemals löschen. Zeitlimits, keine automatische
Wiederholung nach Unsicherheit/Neustart. Compact/Full und lokale Statusabfrage.
Menschliches Gate bestanden; 0.52.0 am 12. Oktober 2026 begrenzt live abgenommen.
Räumbudget-Stopp, autonome Zweiergruppe, Pflanzmarkierungen und Bau auf
einer Rohstoffstapel-Zelle bestätigt; Testbauten entfernt, Endpause belegt.
[Vertrag und Abnahmepilot](docs/cleared-building-groups-0.52.md).

## 0.51.0 — Gruppen-Rückwärtssuche begrenzt live abgenommen

`plan_region_buildings` erweitert Gruppen um begrenzte Rückwärtssuche und
`reservedAreas` (bis acht Ausschlussvolumen). `searchBudget` 1–32 native Vorschauen,
Standard 32; höchstens 256 Suchzustände einschließlich Kandidatenprüfungen.
`maxCandidates` 1–4 gilt pro Erweiterungszustand. Bewertung: mehr Wunschgebäude,
danach weniger neue Wege, danach weniger Zugangshilfen. Bestätigter bester
Teilplan bleibt bei Budgetende erhalten; keine globale Optimalitätsbehauptung.
Native Prüfung hält auch neue Wege/Treppen/Stützen aus den Volumen heraus.
Bestehende Wege bleiben nutzbar; Reservierungen gelten nur für den zurückgegebenen
Plan. Keine dauerhaften Siedlungsreservierungen. Compact/Full erhalten denselben
Plan und Suchstatus. Grenzen acht Gebäude/32 Teile/32×32/acht Höhen unverändert.
Nach menschlichem Gate live bestätigt: Rücksprung senkt einen Viererplan von
13 auf zehn Hilfsteile. Ziel-/Wegeausschluss und Höhenkontrolle bestanden.
Zweiergruppe samt Treppe/zwei Wegen autonom fertig: 3,65625 Spielstunden,
10,72 reale Sekunden; Zugänge und null Bestandsverluste bestätigt. Compact
61.686 → 11.944 JSON-Zeichen (80,6 % kleiner), gleicher Plan/Schlüssel/Suchstatus.
Fünf Testobjekte entfernt, keine Rohstoffe gelöscht, Endpause bestätigt.
[Vertrag und Abnahmepilot](docs/group-search-0.51.md).

Stand: 2026-10-16, nach begrenzter Liveabnahme von **0.62.0**.
Diese Liste trennt noch offene Fähigkeiten von abgeschlossenen Teilumfängen.
Vorschläge sind keine Implementierungsfreigabe. Test-Gate unverändert:
[Entwicklungsablauf](DEVELOPMENT_WORKFLOW.md).
Alle bisherigen Beobachtungen bleiben im [historischen Backlog](BACKLOG_HISTORY.md).

## Offene Entwicklung

| ID | Verbesserung | Vorhandene Grundlage und noch offener Umfang |
| --- | --- | --- |
| B15 — **0.63 abgeschlossen** | **Gesamtstruktur gemeinsam platzieren, dann bis Fertigstellung verfolgen** | Ein Plan, ein Go, gemeinsame Platzierungsphase und Bauabschluss. Menschliches Gate und begrenzte Liveabnahme bestanden; Teilablehnung mitten im Durchgang nur synthetisch geprüft, ausdrücklich akzeptiert. Umfang und Grenzen oben. |
| B01 | Bessere Standortwahl | 0.51: begrenzte Rückwärtssuche, Vergleich ganzer Pläne und Auslassen blockierender Wünsche abgenommen. Offen: alternative Zielreihenfolgen und breitere Suche; keine globale Optimalität. |
| B02 | Reservierungen und Gesamtplanung | 0.51: bis acht Ausschlussvolumen für einen Gruppenplan vorhanden. Offen: dauerhafte Reservierungen, planübergreifende Korridore und Suchabdeckung; keine globale Siedlungsoptimierung. |
| B03 | Kraft- und Höhenrouten planen | Ports, Netze, explizite Höhenpläne und Zugangshilfen existieren. Offen: allgemeine automatische Routenauswahl, Alternativstandorte und kombinierte Versorgungslayouts mit getrenntem Geometrie-/Netz-/Betriebsnachweis. |
| B04 | Flächenumwidmung in Gruppenpläne integrieren | 0.52 begrenzt live abgenommen: eine Fläche bis 8×8 → Räumung → gemeinsame Gruppe. Rohstoffstapel ignoriert, keine Löschung. Größere/mehrere Flächen bleiben offen. |
| B05 | Flächenvorprüfung und Sammelpflanzung | 0.53 begrenzt abgenommen: Zweiflaechenpilot, Ausschluesse und veraltete Vorschau bestanden. Wachstum/Aquakultur nicht nachgewiesen. |
| B06 | Projekte über Sitzungswechsel wiederaufnehmen | Wiederanschluss innerhalb derselben Spielsession funktioniert. Offen: gespeicherte Absichten nach Neuladen mit realen Objekten abgleichen; alte Aktionen nicht blind wiederholen. |
| B07 | Produktionsketten planen | Rezeptwahl, gemeinsame Einrichtung, Engpässe und Produktionsläufe existieren. Offen: Zutatenquellen, Lager/Transport, Personal und Kraft bereits vor Bau als gemeinsamen Ausbauplan berücksichtigen. |
| B08 | Weitere Produktions- und Wirkungsnachweise | Produktionsnachweis bisher für Manufactory. Weitere Produzenten und reale Effektzellen/aktive Ziele etwa des Bienenstocks separat erschließen; Reichweite ist kein Wirkungsnachweis. |
| B09 | Weitere Journaldaten | Pflanzen/Stuempfe und Flaechenmarkierungen mit 0.54 begrenzt live abgenommen. Forschung weiterhin offen; keine automatische Zuschreibung zum Menschen. |
| B10 | Agentenlog verbessern | 0.55: Cursor, Werkzeug-/Statusfilter und Lücken begrenzt live abgenommen. [Vertrag](docs/agent-log-pagination-0.55.md). 0.56: explizite Projekt-/Laufkorrelation mit ID-Filtern begrenzt live abgenommen; Ingame-Anzeige vom Menschen bestätigt. [Umfang](docs/agent-log-correlation-0.56.md). 0.57: Sitzungs-Dateilogs mit Save-Namen, neue Dateien beim Neuladen, vereinfachte Anzeige und automatischer Spielstart begrenzt live abgenommen. [Vertrag](docs/file-diagnostics-0.57.md). 0.58: verständliche Ergebniszeilen für Gebäudeeinstellungen, Simulation und Pflanzchargen begrenzt live abgenommen; Ingame-Lesbarkeit vom Menschen bestätigt. [Umfang](docs/agent-log-outcomes-0.58.md). 0.59: Ergebniszeilen für Bauprojekte und Bauchargen begrenzt live abgenommen (Einzelprojekt, Ein-Gebäude-Charge, Vier-Teile-Großprojekt und gezielter Großprojektstopp am Westufer); Ingame-Lesbarkeit bestätigt. Zusatzproben: Räumgruppe, Spielzeitbudgetende und separater Bauchargenstopp bestanden; Reparaturstart nach gültiger Vorschau mit state_conflict fehlgeschlagen. Neuer 0.59-Kontrollfall erfolgreich, ursprüngliche Ursache weiter offen. 0.60: falscher Planschlüssel korrekt ohne neuen Auftrag abgewiesen, gültiger Reparaturabschluss/Pause/Elternfreigabe live geprüft; zwölf Ergebniszeilen in beiden Dateilogs identisch. Ingame-Lesbarkeit vom Menschen bestätigt; [Befund und Livepilot](docs/repair-start-0.60.md). Zeitlimit-/Stopgrund im Kurztext präzisieren. [Testprotokoll](docs/agent-log-live-tests-0.59.md). [Umfang](docs/agent-log-building-outcomes-0.59.md). Weitere Auftragsfamilien bleiben offen; kein Ersatz für das Spieländerungsjournal. |
| B11 | Agenten-UI im Spiel | Screenshot mit Kamerametadaten und „schau mal“ existieren. Offen: „Zeigen“-Button nimmt die Ansicht beim Klick auf, begrenzte Aufbewahrung und Abhol-/Fehlerstatus. Aktive Benachrichtigung ist ein separat zu definierender Umfang. |
| B12 | Weitere Effizienz und Diagnosequalität | Noch große Werkzeugantworten/-beschreibungen gezielt messen, veralteten Clientkatalog erkennbar machen, unnötige interne Reads reduzieren. 0.54-Pilot: kleine Journalgruppen adaptiv darstellen; sechs Ereignisse mit vier Gruppen sparen nur 5 von 1555 page-Zeichen. Compact/Full und günstiges Warten sind bereits vorhanden. |
| B13 | Optionale Koloniebeobachtung | Geburten/Todesfälle im Intervall, Altersverteilung, bevorstehendes Erwachsenwerden und altersbedingte Personalengpässe, Familienwohnungen, tatsächliche Wohlbefindensursachen und historisches Bauregister. Prognosen mit Zeitraum, Annahmen und Unsicherheit; freie Betten beweisen keinen gesicherten Nachwuchs. Kein fortlaufender Auftrag, alle Gebäude oder eine bestimmte Bevölkerung zu erreichen. |
| B14 | Strategische Kartenübersicht und Spezialobjektsuche | Nutzerauftrag vom 2026-10-16: kartenweite, strukturierte Übersicht über Topologie, Höhenlagen, Höhlen, Wasserreservoirs und Spezialobjekte. 0.62 liefert kartenweiten Terrain-/Quellen-/Flussüberblick, priorisierte Restwassergefahren und bedingte Reservoir-Optionen; Ereignis-/Durchbruchprognosen bleiben offen. Umfang und Abnahme unten. |

### Spielbetrieb 0.61 — neue konkrete Lücken (2026-10-16)

- **Pumpenquelle fehlt in Betriebsdiagnose:** Beide Ansaugstellen hatten
  waterDepth=0, während inspect_building_operation keine Quellenblockade meldete.
  Ansaugzelle, Wasserstand, Verunreinigung und quellenseitige Betriebsbedingung
  strukturiert ausweisen; fehlende Warnung nicht als Förderfähigkeit behandeln.

- **Planschlüssel nach Zugangsergänzung:** `plan_large_project_access` lieferte
  einen gültigen Plan; direkter Start bei unveränderter Pause meldete
  `project_start_plan_changed`. Keine Baustelle entstanden. Frische explizite
  Gesamtvorschau mit denselben Schritten startete erfolgreich. Cache-/Vorschau-
  Identitäten als Ursache untersuchen, nicht ohne Beleg behaupten.
- **Gefahrenanzeige:** Unstable-Core-Alert enthält `{0} days` statt Countdown;
  Wirkungszone und gefährdete Wasser-/Wegeverbindungen fehlen. Ergänzt B14.
  Nutzerhinweis beim Versionsabschluss: Die Bombe/der instabile Kern auf dem Berg
  bleibt eine konkrete offene Gefahr. Objektposition, nativer Countdown, erwartete
  Geländeöffnung und Freisetzung hochliegenden Wassers zusammenführen. Der 0.62-
  Gefahrenkopf erkennt Restwasser, aber prognostiziert diese Explosion nicht.
  Umsetzung nach Nutzer-Go: native Radiuszellen, hypothetischer Vor/Nach-Geländevergleich
  und kompakte Kern–Faulwasser–Ziel–Schutzketten implementiert; Gate bestanden,
  kompakte Vorwarnung bei neun Kernen live belegt. Schutzquerschnitt beim Bergkern
  unvollständig, Vor-/Nach-Schwelle unverändert. Warnung samt soweit möglichen
  Eindämmungsvorschlägen gemäß Nutzerpräzisierung am 17. Oktober abgenommen;
  vollständiger Schutzbau und Ereignisvergleich sind Folgearbeiten, keine Abnahmeblocker.
  Countdown, endgültige Wirkungszone und Einsturz bleiben ungeprüft.
  Historischer Recheck vor der Erweiterung: vollständige Terrainabdeckung und alle 501 Spezialobjekte
  gelesen; neun `UnstableCore` werden nur als `special_or_unknown` geführt. Compact
  warnt vor hochliegendem kontaminiertem Restwasser, verknüpft die Kerne aber nicht
  mit einem Durchbruch. **Konkreter B14-Folgeauftrag:** native Kernzustände und
  belegbare Wirkungszellen ermitteln; Szenario vor/nach Geländeöffnung vergleichen,
  betroffenen Speicher, Kontamination, Zuflussrinne und Zielgewässer verbinden.
  Compact muss Kernposition, Auslöser/Frist (sonst ausdrücklich unbekannt), bedrohtes
  Gewässer und vorab prüfbare Schutz-/Umleitungsstellen zeigen. Ohne belegten
  Explosionsradius keine erfundene Öffnung. Abnahme: noch geschlossene Sperre,
  Faulwasser dahinter, Teich darunter; Warnung vor Auslösung, danach Routenvergleich.
  Bildorientierung ist ohne Kamerabezug keine sichere Objektzuordnung.
- **Kollisionsdiagnose:** precheck_building nennt belegte Zellen, aber keine
  verursachenden Objekt-IDs/-typen; erschwert gezielte Räumung und Standortwahl.
- **Kraftplanung:** Generator-/Verbraucher-Verbindung als explizites Planungsziel;
  `rotationMatches=false` bedeutet im belegten Laufradfall keine fehlende reale
  Verbindung. Portausrichtung, Netzverbindung und Produktion verständlich trennen.
- **Fortlaufende Simulation:** Bauchargen pausieren zwingend am Abschluss.
  Optionale ausdrücklich beauftragte Abschlussgeschwindigkeit prüfen, damit
  Spielbetrieb auf Tempo 7 keinen zusätzlichen Resume-Aufruf braucht.
- **Gemeinsamer Versorgungsstart (B07):** Wasserpumpe, Beerensammler und
  Holzfällerfahne einschließlich Wegen, Fällmarkierung, erreichbaren Quellen und
  Personal in einem Plan vorbereiten. Danach Wohnraum und Erfinder, anschließend
  Sägewerk mit Kraftversorgung. Fertige Gebäude allein reichen als Abschluss
  nicht: Förderung, Sammlung und Holzeintrag getrennt nachweisen. Vorhandene
  Gruppenplanung wiederverwenden; fehlende gemeinsame Quellen-/Personalprüfung
  ergänzen, keinen neuen Planer allein für diese Reihenfolge schaffen.

### B14 — Strategische Sicht statt isolierter Kartenausschnitte

Positive Terraforming-Optionen implementiert, Gate und begrenzter Livepilot bestanden: begrenzte 3×3-Reservoirsuche mit zwei Ebenen Absenkung, Höhlen-/Seitenwand-/Objektprüfung, geometrischem Volumen und expliziten Zufluss-/Kontaminationsblockaden. Freie Beckenformen, echte Vorratsreichweite, Nachfüll-/Abflussplanung und Bewässerungswirkung bleiben offen. B15 ist mit 0.63 im oben dokumentierten Umfang abgeschlossen.

Compact/Full-Nachtrag live geprüft: Bedrohung und Gegenmaßnahmen im Standard, vollständige Inventur/Geometrie nur in full. Gemeinsame Maßnahmen und offene Prüfpunkte ohne Wiederholung.

Nachtrag live abgenommen einschließlich Dürrefall: Gefahrenkopf über alle erreichbaren
nassen Hohlräume; Restwasser vor Quellhinweisen, Zielhöhe berücksichtigt;
priorisierte Schutzstellen mit Dachprüfung und geometrischem Nebenweg-Kontrollfall.
Weiter offen: Unstable-Core-Countdown/Wirkungszone, Durchbruch- und zeitliche
Pegel-/Geländeänderungsanalyse, Wasserbauwerkszustände und hydraulische Wirkung.


Ziel präzisiert: allgemeine Mapawareness und Flüssigkeitsübersicht, nicht nur
Flutschutz einer Zielzone. Kartenweit Quellen, Flussbetten, Tunnel, Becken,
Verzweigungen, Überläufe und Kartenabflüsse samt Verbindungen darstellen.
0.62 nach menschlichem Gate begrenzt live abgenommen: zielunabhängiger Flussbericht mit blätterbaren
Abschnitten/Verbindungen, nativen Quellen-/Wetterdaten und Vergleich zweier Scans.
Offen: tatsächliche Strömung/Wasserbauwerke, zukünftige Kontamination ohne native
Regel und verifizierter Wettercountdown. Dürrebeginn mit zehn auf null fallenden
Quellenstärken und korrekt gemeldeten Differenzen live belegt; Faulwasserflutwechsel
und spezielle Aktivierungsregeln weiterhin offen.
Der nächste fachliche Ausbau muss drei getrennte Ebenen liefern:
- **Mögliche Wege:** Geometrische Verbindungen und Schwellen, auch trockene Rinnen;
  Richtung nicht allein aus Nachbarschaft ableiten. Wasserbauwerke und deren
  tatsächliche Einstellungen berücksichtigen oder als unbekannte Barriere melden.
- **Aktueller Zustand:** Beobachtete Pegel, Wasser/Faulwasser und Mischungen mit
  gemessener Kontamination je Abschnitt; native Quellentypen und Aktivität.
  Quellentyp ist nicht automatisch der Flüssigkeitstyp im gesamten verbundenen Netz.
- **Erwartete Änderung:** Wetter-/Ereignisphase, verbleibende Zeit, belegtes
  Quellenverhalten und mögliche Ausbreitungswege zusammenführen. Bedingte Szenarien
  von Prognosen trennen; ohne Durchfluss-/Mischungsnachweis keine Ankunftszeit oder
  künftige Reinheit behaupten. Unbekannte Wetterdaten ausdrücklich ausweisen.
Abnahme: zusammenhängendes Flussnetz auch ohne Siedlungsziel, trockener Überlauf,
getrennte Höhlenstockwerke, Zusammenfluss von Wasser und Faulwasser sowie
Ereigniswechsel mit nachvollziehbarer Änderung von Quelle und Flüssigkeitszustand.

0.62 ist als erster zusammenhängender Umfang **begrenzt live abgenommen**:
Livepilot bestätigt Gesamtabdeckung bei Tempo 7 und einen verseuchten Tunnel.
Quellenhöhen-/Kronenfehler korrigiert und live nachgeprüft. Für den nächsten Ausbau:
einzelne freie Höhenintervalle gezielt abrufbar machen, vollständige Routen blättern,
Querschnitte gegen Tunneldecken prüfen und nicht platzierbare Bauhöhen explizit
kennzeichnen. Die Priorisierung gespeicherten Wassers vor trockenen Quellhinweisen ist inzwischen abgenommen.
Aktueller Umfang: fortsetzbarer Gesamtscan mit freien Höhenintervallen, Spezialobjekten, geometrischen
Zuflusskorridoren und Sperrquerschnitten. [Vertrag/Pilot](docs/strategic-map-0.62.md).
Offen bleiben insbesondere native Wasserbauwerks-/Strömungsmodellierung,
vollständige Gefahrenabdeckung statt repräsentativer Korridore, Wetterprognose,
globale Änderungsrevision, native Quellklassifikation und reale Schutzwirkung.

- Kartenweite Bestandsaufnahme mit Kartengrenzen, Höhenbereichen und expliziter Suchabdeckung; paginiert bzw. fortsetzbar, kompakte Übersicht mit gezielten Detailabfragen. Untersuchte, veraltete und unbekannte Bereiche unterscheiden. Im laufenden Spiel nutzbar, ohne lange Planungspause.
- Oberflächen und unterirdische Hohlräume getrennt darstellen: Höhlen, Überhänge, offene Schächte, Eingänge, Rampen, Engstellen und mögliche Übergänge zwischen Höhen. Zusammenhängende Regionen und Verbindungen als Topologie anbieten; geometrische Verbindung, Biberweg, Distriktanschluss und Bauarbeiterzugang getrennt belegen.
- Spezialobjekte nach nativer Kategorie/Vorlage und Position finden, etwa Wasser-/Schlechtwasserquellen, Ruinen und andere vorhandene Kartenobjekte. Unbekannte Typen sichtbar lassen; keine Namen oder Fähigkeiten erfinden. Eine lokale Abrisszielabfrage mit Kategorie `other` ersetzt diesen Index nicht.
- Wasserflächen und Reservoirs zusammenhängend erfassen: offene oder überdeckte Lage, Oberfläche/Boden, Tiefe, Verschmutzung, Ufer und mögliche Pumpenstandorte. Aktueller Wasserbestand ist keine Garantie für Dürredauer oder erreichbaren Trinkwasserbetrieb.
- Skalierung an Vegetation und großen Karten prüfen. Livebefund vom 2026-10-16: `navigation_object_limit` in der Einzelvorschau, während Projektplaner nur `state_conflict` meldeten. Globale Objektgrenzen dürfen weder unbemerkt die Abdeckung abschneiden noch den konkreten Ablehnungsgrund verbergen.
- Kleiner Abnahmepilot: offenes Wasserloch gegenüber überdecktem Wasser, zwei getrennte Höhen mit/ohne Übergang und ein Spezialobjekt außerhalb des Siedlungsumfelds. Übersicht gegen native Detaildaten prüfen; Abdeckung, Aufwand und Grenzen ausweisen, bevor auf die gesamte Karte skaliert wird. Keine Implementierungs- oder Architekturfreigabe durch diesen Backlogeintrag.

B01 plus begrenzte Korridore aus B02 sind mit 0.51 im beauftragten Umfang erledigt.
B04 ist als 0.52 im begrenzten Umfang live abgenommen. Mehrere oder größere Räumflächen bleiben offen.

## Offene Folgearbeiten nach Phase E

Die ursprünglichen IDs bleiben als Verweisziele erhalten; ihre alten Grenzen
sind nicht pauschal der aktuelle Funktionsumfang.

| ID | Aktueller Restumfang |
| --- | --- |
| F01 | Vollständiger Bauphasenschutz: Bestandsbaustellen, Zugänge während aller Änderungen und freie Rückwege betroffener Biber. Zugangsergänzung und begrenzte Bau-/Fertigkontrollen sind bereits abgenommen. |
| F02 | Ersatzumweg erst fertigstellen und seine Nutzbarkeit bestätigen, anschließend abhängige Bestandsänderung. Der Anschluss um ein Hindernis allein beweist diesen Ablauf nicht. |
| F03 | Teilweise erledigt: explizite 32-Teil-/acht-Höhen-Pläne, autonome Chargen und Zugangshilfen vorhanden. Allgemeine räumliche Standort-/Routensuche bleibt B01–B03. |
| F04 | Besondere Standort-, Geometrie- und Distriktfälle gezielt prüfen. Mehrere Türen nur bei belegter Spiel-API; Bauzugangszellen sind keine zusätzlichen Eingänge. |
| F05 | Repräsentative weitere Fraktionen, Karten und Spielversionen. Iron-Teeth-Rezepte sind generisch vorgesehen, aber nicht durch den Folktails-Pilot mitabgenommen. |

## Zurückgestellte Experimente

Der Hermes-Integrationstest wurde am 2026-10-17 auf Nutzerwunsch beendet.
Proxy, Integrationsanleitung und Skillvorlage wurden aus dem Projekt entfernt;
keine weitere Umsetzung, Installation oder Versionspflege für Hermes vorgesehen.
Historische Release-Erwähnungen bleiben als Nachweise früherer Arbeiten erhalten.
Andere lokale Startskript-/Budgetexperimente sind kein Auftrag für den nächsten
Release und bleiben außerhalb dieses Rückbaus.

## Zurückgestellte Nachweise

- Große Gebäude: bis 512 native Blockeinträge und DomedGarden begrenzt live belegt.
  Offen bleiben Großgebäude ohne Eingang, native Hex-Platzierung mit isolierter
  Außenzellenkontrolle und Mine auf ungeeignetem Standort. Vorbereitung ist keine
  Liveabnahme. Kein erneuter stundenlanger Forschungsaufbau ohne Umfangsentscheidung.
  [Nachweise](docs/large-building-geometry.md).
- Allgemeiner Kraftgeometrie-/Drehungsnachweis: widersprüchliche Vorschauindikatoren
  gegenüber realer Netzverbindung gezielt prüfen, nicht aus einzelnen erfolgreichen
  Kraftpiloten auf alle Kombinationen schließen.
- Historischer Vier-Wege-Stash: nur bei konkretem Bedarf gegen heutige Umsetzung
  prüfen; kein aktiver Entwicklungsauftrag und kein ungeprüftes Wiedereinspielen.

## Erledigt im jeweils dokumentierten Umfang

| Bereich | Abschluss / Beleg |
| --- | --- |
| Gruppenpläne, gemeinsame Wege, native Kandidaten | 0.48–0.50; [effiziente Gruppenplanung](docs/efficient-group-planning.md). Die drei Folgepunkte aus 0.49 sind damit im vereinbarten Umfang erledigt. |
| Kompakte Ergebnisse, Ausführungsgewissheit, günstiges Warten | 0.45–0.47; [Aktionsberichte](docs/action-results-and-run-reports.md), [Projektberichte](docs/compact-project-reports.md), [Status/Warten](docs/lightweight-status-wait.md). |
| Rezepte, Gebäudesuche, gemeinsame Konfiguration, Produktion und Engpässe | 0.38–0.44; [Projektstand](PROJECT_STATE.md). Keine automatische Produktionskettenplanung. |
| Spieländerungsjournal | 0.40, Pflanzen/Flaechen mit [0.54](docs/spatial-journal-0.54.md); [Vertrag](docs/recent-actions-journal.md). Erweiterungen siehe B09. |
| Autonome Baucharge, Abschluss, Diagnose und Zugangsergänzung | [Chargen](docs/autonomous-construction-batches.md), [Reparatur](docs/project-recovery.md), [Zugangsplanung](docs/automatic-access-planning.md). Kein vollständiger Navigationsschutz. |
| Waldreichweite, Stümpfe, artenbezogener Fällschutz und weniger Reads | [Waldnachweis](docs/forestry-range-protection.md). Fällschutz ist keine dauerhafte Schutzregel gegen spätere Eingriffe. |
| Screenshot und persistenter Codex-Vorschaltprozess | [Screenshot](docs/screenshot-0.36.0.md), [Lifecycle](docs/codex-mcp-supervisor.md). UI-Button bleibt B11. |

## Dokumentationsklärung

[Prüfbericht 2026-10-12](docs/documentation-audit-2026-10-12.md): korrigierte
Altstände und die anschließend getroffenen Nutzerentscheidungen. Die damalige
Hermes-Rückstellung ist durch die Entfernung am 2026-10-17 überholt. Lokale
Budgetänderungen nicht aus dem Versionsnamen automatisch als abgenommen behandeln.

## Folgebeobachtungen aus 0.51

- Suchbudget wirkt: 32 Vorschauen statt Greedy-Frühabschluss; Viererplan spart drei
  Hilfsteile. Das feste Tiefensuchbudget kann frühere Wünsche lange festhalten.
  Breitere Zielreihenfolge nur bei gemessenem Nutzen, keine automatische Budgeterhöhung.
- Die native Bauzugangsplanung kann für kleine Gruppen weite Umwege erzeugen.
  Gesamtplan-Kosten sichtbar lassen; Rückwärtssuche garantiert keine kürzeste Route.
- Ein zu schmales Suchfenster ergab keine Kandidaten, die größere Kontrollfläche
  funktionierte. Suchabdeckung nicht mit Reservierungsablehnung verwechseln.
- Client zeigte nach Neustart noch die 0.50-Werkzeugbeschreibung, Backend nahm
  0.51-Parameter korrekt an. Katalog-Veraltungsdiagnose bleibt B12.

## Folgebeobachtungen aus 0.52

- Compact ist im Zweierpilot 92,2 % kleiner (67.790 auf 5.268 JSON-Zeichen),
  enthält aber noch viele leere Produktionsfelder im Räumlauf. In 0.53 gekuerzt; automatischer Nachweis im bestandenen Gate.
- Ungültige jobId wurde korrekt vor Transport abgewiesen; die allgemeine
  Fehlermeldung verschweigt jedoch die konkrete UUID-Anforderung. Feste Hinweise in 0.53 implementiert und im Gate geprueft.
- B12 bleibt: neue Werkzeuge wurden erst nach Codex-Neustart verfügbar,
  obwohl der Vorschaltprozess bereits den aktuellen Katalog meldete.

## Nacharbeiten aus dem 0.53-Livepilot

Am 12. Oktober 2026 abgeschlossen: Bestaetigte native Vorschau-Ablehnung meldet
not_executed; Compact reduziert passende Markierungen auf Summen. Menschliches
Gate und gezielter Livetest bestanden; Full bleibt vollstaendig.
Weiter offen: MCP-Aufrufe sind seriell, parallele Leser werden sofort abgewiesen.
Die feste Argumentdiagnose nennt den laufenden Aufruf; keine Warteschlange.

### Pumpengeometrie im automatischen Zugangsplaner (Live 0.61)

- Reservoirpumpe wird von plan_large_project_access mit target_intersects_existing_or_planned_object abgewiesen, ohne Objekt im lokalen Baufeld. Dieselbe Pumpe ist in plan_large_project nativ gueltig; mit expliziter Treppe und Wegen auch verbunden und pilotEligible. Vermutung: unterirdische Any-Zellen werden pauschal als Hindernis behandelt. Mit nativen Supportregeln abgleichen und konkrete Zelle/Objekt/Regel ausgeben.
- Unbekannte Wegvorlage Path.Folktails ergibt im reinen Planaufruf invalid_argument mit pauschalem execution=unconfirmed ohne konkrete Ursache. Katalogvorlage ist Path. Vor Transport validieren oder native Argumentdiagnose erhalten; lesende Planung und moegliche Vorschauwirkungen von Bauauftrag unterscheiden.

### Hochwasserdiagnose und Ausweichplanung (Live 0.61)

- **Präventiver Gefährdungsscan (B14):** Wohn-/Versorgungszone als Zielgebiet wählen; zuführende Rinnen, Durchbrüche, Überlaufkanten und Quellen auch außerhalb des Siedlungsumfelds erfassen. Gefahrenpfad, Höhenprofil, Suchabdeckung und Unsicherheit ausgeben. Priorisierte Eingriffspunkte mit Bauzeit, Kosten, Zugang und möglichen Ausweichfließwegen vorschlagen; geometrisch möglicher Zufluss ist noch keine hydrologische Prognose.
- **Terraforming/Wasserbau im Early- und Midgame:** Verfügbare native Dämme, Deiche, Wassersteuerung und Geländeänderungen samt Freischaltungen systematisch erschließen. Schließen oder Umleiten gefährlicher Rinnen, Entwässerung, Rückhalt und Höhenzugänge als zusammenhängende Vorhaben planen; Bauphasen und Rückstau berücksichtigen. Reguläre Fähigkeiten von Entwicklungswerkzeugen trennen. Abnahmepilot: Zufluss zur Siedlung mit kontrollierter Ableitung sperren, Vorher-/Nachher-Pegel und neue Überläufe prüfen; keine Schutzgarantie aus bloßer Fertigstellung.
- inspect_building_operation meldet Flooded als Text, assessment.blockers aber nur fehlendes Personal/Arbeitszeit; ueberflutetes Inventar unblocked=false. Native Ueberflutung als expliziten Betriebsblocker abbilden, damit keine falsche Personalreparatur folgt.
- Globale Vorraete auch bei ueberfluteten Lagern positiv: tatsaechlich erreichbare Trinkwasser-/Nahrungsreserve getrennt ausweisen.
- Reserven nach Distrikt, Lager-/Produktionsausgabe und tatsächlichem Zugriff aufschlüsseln; unzugängliche, überflutete oder nicht trinkbare Bestände nicht als verfügbare Notreserve zählen. Verbrauchsreichweite aus beobachtetem Verbrauch mit Zeitfenster und Unsicherheit ableiten.
- Strategische Karte B14 um Flusszufluss/Abfluss, Wasseroberflaechenhoehe, Hoehenprofil, Kontaminationsfront und erreichbare hoehere Ausweichflaechen erweitern. Lokale Baugueltigkeit allein erkennt keine bevorstehende Ueberflutung.
- Hochwasser-Livebeleg: Paused kann Flooded im sichtbaren Statuslisting verdecken. Direkten nativen Flutzustand unabhaengig von sichtbaren Statusprioritaeten abfragen; fehlender Fluttext beweist keine Betriebsfaehigkeit. Wasserstandtrend an festen Messzellen statt wechselnden Regionen fuer Zu-/Abflussbewertung verwenden.
- Wetter-/Gefahrenphase, verbleibende Dauer und bekannte Vorwarnzeit strukturiert liefern; unbekannte Dauer explizit kennzeichnen. Dürre, Schlechtwasser und andere native Ereignisse unterscheiden. Keine exakte Flutprognose aus einer einzelnen Tiefenmessung behaupten.
- Evakuierung und Ausweichdistrikt als fehlende Fähigkeit prüfen: höher gelegenen erreichbaren Standort, sichere Route, Trinkwasser/Nahrung, Unterkünfte und Baupersonal gemeinsam bewerten; native Migration mit tatsächlichem Ankunftsnachweis ermöglichen. Vor Überflutung des Bauzentrums vorbereiten; ein Bauauftrag ohne verfügbare Bauarbeiter ist keine Rettung.
- Notentwässerung/Quellensteuerung: vorhandene native Bau-, Abriss- und Wassersteuerungsmöglichkeiten mit Voraussetzungen und Wirkungsnachweis erschließen. Im Livefall fehlten MCP-Eingriffe zum Öffnen eines Abflusses bzw. Abstellen der Flutquelle. Regulär erreichbare Maßnahmen von gesonderten Entwicklungs-/Geländeeingriffen unterscheiden; keine vorhandene Fähigkeit oder rechtzeitige Wirkung unterstellen.


# Historischer Backlog bis 0.50

Archiviert am 2026-10-12. Die folgenden Einträge sind zeitgebundene Beobachtungen,
keine aktuelle Aufgabenliste. Spätere Abnahmen überholen ältere Offen-/Gate-Aussagen.
Maßgeblich: [aktueller Backlog](BACKLOG.md) und [Projektstand](PROJECT_STATE.md).
Relative Fachlinks bleiben erhalten; frühere Kolonieziele sind kein Entwicklungsauftrag.

# Backlog

## 0.50.0 — effizientere Gruppenplanung begrenzt live abgenommen

Gemeinsame Flächendaten innerhalb eines Gruppenaufrufs, frische Pause-/Sessiongrenzen
und unveränderte native Gesamtprüfung. Frühfilter anhand nativer Belegungsverträglichkeit (BlockOccupations.Intersects);
unbekannte Profile/fehlende Geometrie bleiben nativ zu prüfen.
`readMetrics` nennt tatsächliche Reads, Cachetreffer und native Vorschauen.
Antworten nennen Belegungsarten: geplantes Gebäude mit Index/Vorlage, Bestandsgebäude,
Weg, Vegetation, Rohstoffstapel oder unbekanntes Objekt. Regionsbelegung ist Kontext,
kein erfundener Ablehnungsgrund. Compact bündelt Filterbefunde und beschränkt
Bestandsbeispiele mit total/truncated; `detail=full` enthält alle Belege.
Acht Gebäude/32 Teile bleiben. Nach menschlichem Gate: gemischte Dreiergruppe
mit vier gemeinsamen Wegen live fertig und zugänglich, Endpause bestätigt.
28 Reads eingespart, drei statt sechs native Vorschauen, Compact 71 % kürzer.
Testobjekte entfernt; Bestandswege und Rohstoffstapel erhalten.
[Vertrag und Pilot](docs/efficient-group-planning.md).

## 0.49.0 — gemeinsame regionale Gebäudegruppe begrenzt live abgenommen

`plan_region_buildings` erweitert: exklusiv zu `template` eine Liste `templates`
mit 1–8 Gebäuden, Wiederholungen erlaubt. Suche einmal je unterschiedlicher Vorlage,
gemeinsame native Vorschau nach jeder Ergänzung; Zugangshilfen und Wege gemeinsam
berechnen. Maximal vier Kandidaten je Gebäude / 32 native Vorschauen insgesamt.
`complete/partial/none` bezeichnet Planung, nicht Bauabschluss. Unveränderte
`steps` und `planKey` an `run_large_project` übergeben; fehlende Gebäude stehen
in `unplaced`. Compact standardmäßig, `detail=full` mit allen Vorschau-Belegen.
Greedy in Eingabereihenfolge, keine optimale oder vollständige Standortsuche.
32 Gesamtteile, 32×32 und acht Höhen bleiben. Nach menschlicher Bereitmeldung
Bridge 0.49.0 bestätigt: drei Bänke plus vier gemeinsame Wegstücke in einem
Startauftrag fertig und zugänglich, 3,03125 Spielstunden / 8,85 reale Sekunden.
Teilplan- und belegte Einzelflächenprobe bestanden; Testbauten/Wege entfernt,
Bestandswege und drei zurückgewonnene Rohstoffstapel erhalten, Endpause bestätigt.
[Vertrag und Pilot](docs/group-building-plans.md).

### Folgepunkte aus 0.49

- Kollidierende Drehungen bereits gewählter Ziele vor der nativen Vorschau filtern;
  im Pilot gingen dafür mehrere der vier Versuche je Gebäude verloren.
- Gemeinsame Karten-/Objektbeobachtung für unterschiedliche Vorlagen; bisher nur
  identische Vorlagen ohne weitere Regionalsuche wiederverwendet.
- Compact weiter verdichten: wiederholte Nachweisgrenzen bündeln, konkrete
  unknowns, Blockaden und Teilplanstatus erhalten.


## 0.48.0 — regionale native Baupläne begrenzt live abgenommen

Menschliches Gate ausdrücklich bestanden; Bridge 0.48.0 live bestätigt.
Gedrehte Tank-Geländekante in Vorprüfung, Suche und nativer Vorschau abgelehnt.
Regionale Positivprobe: vier Kandidaten, zwei nativ geprüft/bestanden, zwei
ungeprüft; unveränderter Bankplan nach 4,78125 Spielstunden fertig und zugänglich.
Zusatzwegprobe: zurückgegebene Abhängigkeit Weg → Bank unverändert ausgeführt,
zwei Teile nach 1,84375 Spielstunden fertig. Beide Projekte frisch diagnostiziert,
keine verlorenen Anschlüsse, freigegeben. Zwei Testbänke und Testweg entfernt,
Bestandswege erhalten, keine Rohstoffe gelöscht; Endpause bestätigt.
Doku und Agentenanleitung aktualisiert. [Vertrag und Grenzen](docs/regional-building-plans.md).
Keine allgemeine 3D-Standortsuche oder vollständige Bauphasenschutz-Abnahme.

## 0.47.0 — günstige Statuslesung und begrenztes Warten begrenzt live abgenommen

inspect_simulation_run und inspect_large_project: waitSeconds 0–30, optional after,
diagnose=false als Standard. Einfache Statuslesungen ohne teuren Diagnosescan;
Diagnose auf Wunsch einmal nach dem Warten. Sessiongebundene Änderungskennung,
Beobachtungszeit und Abfragezahlen, kein Schreibauftrag/Tempo-/Budgeteingriff.
Menschliches Gate nach Waldprotokoll-Korrektur ausdrücklich bestätigt. Live 0.47:
30,007 Sekunden / 30 leichte Reads ohne Diagnose; Abschluss nach weiteren
11,274 Sekunden mit Pause. Bekannte Blockade sofort; explizite Diagnose genau
einmal. Zweiteil-Projekt nach 1,024 Sekunden abgeschlossen, Full-Diagnose mit
eigener Beobachtungszeit. Testwege entfernt, Bestandswege erhalten, Endpause.
[Vertrag, Grenzen und Nachweis](docs/lightweight-status-wait.md).

## 0.46.0 — kompakte Bauprojektberichte begrenzt live abgenommen

Nach Größenkorrektur menschliches Gate und Zweiteil-Livepilot bestanden.
Start trennt zwei Versuche von noch null bestätigten Platzierungen; Abschluss
und frische Diagnose bestätigen zwei fertige zugängliche Teile sowie Pause.
Compact/Full: Ledger 1.384/1.856 UTF-8-JSON-Bytes (25,4 % kleiner), fertige
Diagnose 1.065/1.278 Bytes (16,7 % kleiner); gemeinsame Werte unverändert.
Unterirdischer Weg bleibt native_invalid, pilotEligible=false, restored=true.
Testwege entfernt, Fläche leer und Bestandsanschluss bestätigt; keine Rohstoffe
gelöscht. Doku/Agentenanleitung angepasst, Ausführungslogik und Limits unverändert.
[Vertrag und begrenzte Abnahme](docs/compact-project-reports.md).

## 0.45.0 — Aktionsergebnisse und Laufberichte begrenzt live abgenommen

Menschliches Gate nach korrigierter Integrationserwartung bestanden; Bridge 0.45.0
live bestätigt. Zwei ungültige Gebäudeauswahlen melden not_executed und legen
keinen Lauf an; lesendes run_not_found meldet not_applicable.
Drei Spielstunden mit zwei Gebäuden: automatische Pause, identische historische
Messwerte und Diagnosebefunde in Compact/Full; Text und StructuredContent gleich.
Compact 3.436 statt 8.206 UTF-8-JSON-Bytes (58,1 % weniger in diesem Pilot).
Erfinder zwei native Zyklen, Papierfabrik null mit mehreren Blockaden.
Vollständige Inventarbelege nur in Full; keine zusätzliche interne Lesekostenersparnis.
Transportfehler synthetisch im Gate, keine absichtlichen Live-Verbindungsabbrüche.
[Vertrag und Abnahme](docs/action-results-and-run-reports.md).

## 0.44.0 — Durchsatz und Zeitanteile begrenzt live abgenommen

Nach Korrektur der Fortschrittsbasis bei unveränderter Spielzeit menschliches
Gate und Zweierpilot bestanden. Zwei Messfenster zu je drei Spielstunden:
Erfinder vier/fünf native Zyklen; Papierfabrik nach freiem Lager null → zwei
Zyklen, Lagerstillstand 3 → 0,65625 Stunden. Automatische Pause, unveränderlicher
Altbericht und mutationsfreie Auswahlfehler bestätigt. Ursprüngliche
Lagereinstellungen wiederhergestellt; 20 transportierte Papier bleiben erhalten.
Achtfachumfang automatisch geprüft, zwei Gebäude live. Zeitanteile sind
beobachtete Schätzungen; Rezeptmengen keine Liefermengen.
[Vertrag und Abnahmegrenzen](docs/production-throughput.md).

## Abgenommen: 0.43.0 Engpassdiagnose

Auch das abschließende menschliche Gate der Beschreibungskorrektur und die erneute Zweierdiagnose sind bestanden. Der aktualisierte Werkzeugkatalog zeigt korrekte Umlaute. 0.43.0 ist im dokumentierten Umfang abgenommen.

Menschliches Gate und begrenzter Abschluss-Livepilot bestanden. Grill,
Papierfabrik und Erfinder liefern verwertbare Diagnose; Quellenbestand und Wege
separat bestätigt. Freies Papierlager erkannt, Leermodus korrekt ausgeschlossen;
Kastanienmangel mit lokal/global null korrekt diagnostiziert. Gemeinsamer Lauf:
Erfinder ein Zyklus, Papierfabrik kein Zyklus wegen vollem Ausgang, Zeitlimit und
automatische Pause bestätigt. Testeinstellungen wiederhergestellt, keine Abrisse
oder Rohstofflöschung. Achtfach- und Teilabdeckungsgrenzen automatisch geprüft,
nicht als achtfacher Live- oder Lasttest abgenommen.
[Vertrag und Nachweisgrenzen](docs/production-bottlenecks.md).

## Abgenommen: 0.42.0 gemeinsame Einrichtung und Betriebsaufnahme

Bis acht Rezept-/Personal-/Pauseziele, gemeinsame Vorprüfung, konkrete Teilfehler,
optional bestehender Produktionslauf. Nach Untergrenzenkorrektur Gate und begrenzter
Livepilot bestanden: drei Konfigurationen, zwei Produktionsnachweise, zwei
mutationsfreie Konfliktkontrollen und stabile Historie. Keine automatische
Versorgung oder Produktionskettenplanung. [Nachweis](docs/building-configuration-batch.md).

## Abgenommen: 0.41.0 Gebäudebezogener Produktionsnachweis

Beauftragter Umfang implementiert: bis acht Manufakturen im bestehenden Lauf,
Erstzyklusnachweis, eingefrorene Blockadenhistorie und Unterbrechung bei erkannten
Konfigurationsänderungen. Menschliches Gate und Drei-Fall-Livepilot bestanden.
[Details](docs/building-production-proof.md). Durchsatzmessung und Produzenten ohne
Manufactory sind nicht Teil dieses Nachweises.

## Abgenommen: 0.40.0 Änderungsjournal

Beauftragter erster Umfang implementiert: Gebäude/Wege, Fertigstellung,
Pause/Rezept/Lager/Sollpersonal/Abrissmarkierung und Tempo; Cursor, Filter,
Ringpuffer und sichtbare Erfassungslücken. Gate und begrenzter Livepilot bestanden.
Pflanzen/Flächen/Forschung und nachweisbare Mensch-/Simulationsherkunft bleiben
Folgeumfang. [Vertrag und Pilot](docs/recent-actions-journal.md).

## Abgenommen: 0.39.0 Gebäudesuche und Sammeldiagnose

Beauftragt und implementiert: native Vorlagen-/Status-/Ankerbereichsfilter,
Wege optional, Manufactory-Auswahl; bis 32 Betriebsbeobachtungen mit einem
fachlichen Request und kompakter Antwort. Inaktive Kraftverbraucher separat.
Gate und begrenzter Livevergleich bestanden (15 IDs, drei Einzelkontrollen). [Vertrag und Pilot](docs/building-query-batch.md).

## Abgenommen: 0.38.0 Rezeptwahl und gebündelte Betriebsdiagnose

Nach Abschluss von 0.37.0 beauftragt. Rezeptoptionen samt Zutaten/Produkten,
Auswahl mit Zustandskonfliktprüfung und Rücklesung; gebündelte Diagnose für
Rezept, Personal, Pause, Zutaten und Kraft. Menschliches Gate und begrenzter
Livetest bestanden; Bäckerei-Output 180 → 190 Brot, Simulation pausiert. [Vertrag und Pilot](docs/recipe-operation-settings.md).

## Neue Beobachtungen aus 0.38.0

- Nutzeranforderung: Rezeptwahl generell fraktionsunabhängig halten. Bestehender
  Pfad verwendet bereits native ProductionRecipes pro Gebäude, ohne Folktails-
  Whitelist. Bei nächster geeigneter Iron-Teeth-Sitzung denselben Wechsel- und
  Konfliktpilot mit dort angebotenen Rezepten nachweisen; keine eigene Rezeptliste.

- Veralteten Client-Werkzeugkatalog nach Backendwechsel sichtbar melden.
- Gebäudesuche nach Vorlage filtern, statt Wege seitenweise mitzulesen.
- Kraftstatus bei inaktiven Verbrauchern einordnen: unversorgt ist kein Beweis
  eines defekten Anschlusses.

Nur vorgemerkt, noch nicht umgesetzt.

## Erledigt: interne Wald-Leseaufrufe reduziert

Menschliches Gate und Vergleichspilot bestanden: Übersicht 54→32 (−40,7 %),
Zweibaum-Schutz 113→40 (−64,6 %). Vollständigkeit, Mengenlimit, Gegenkontrolle und
Replay unverändert geprüft. Testmarkierungen entfernt, Simulation pausiert.
[Vertrag und Nachweis](docs/forestry-range-protection.md).

## Live abgenommen: vollständige Waldreichweite und artenbezogener Fällschutz

Menschliches Gate und begrenzter Pilot 2026-10-11 bestanden: gesamte Reichweite,
native Baumidentität statt Försterkategorie, Stümpfe separat, zwei Kiefernmarkierungen
entfernt bei unveränderten 71 markierten lebenden Eichen. Mengenlimit und Replay
geprüft; Testmarkierungen vollständig aufgeräumt. Keine dauerhafte Schutzregel.
Effizienzfolgearbeit: 54 interne Reads pro Vollübersicht und 113 beim Schutz von
zwei Bäumen. Native Reichweiten-/Pflanzenabfrage und gezielte Schlusskontrolle
könnten interne Seitenaufrufe sparen; vor Optimierung konkreten Umfang wählen.
[Vertrag, Grenzen und Nachweis](docs/forestry-range-protection.md).

## Großbau-Nachweis gesichert; Sondertests zurückgestellt

Bis 512 native Blockeinträge durch Katalog, Baupfade und Zugangsplanung;
32 Teile/32×32/acht Höhen unverändert. Menschliches Gate und DomedGarden mit elf
automatisch ergänzten Zugangsteilen am 2026-10-11 bestanden. Reale Bauarbeiter-
reichweite und fertiger Eingang separat belegt. Feste Single/Hex-Geometrie ist
vorbereitet und lesend geprüft, native Platzierung nicht abgenommen.
Nach gemessenem Forschungsaufwand von etwa 12–13 realen Stunden ausdrücklich
zurückgestellt: Großgebäude ohne Eingang, native Hex-Platzierung mit isolierter
Außenzellen-Negativkontrolle sowie Mine auf ungeeignetem Standort.
Kein automatischer Kolonieausbau dafür. Testbau und elf Zugangshelfer stehen noch;
bei späterem Rückbau Rohstoffe regulär einsammeln, nicht löschen.
[Umfang und begrenzter Nachweis](docs/large-building-geometry.md).

Livebeobachtung: `find_buildings`-Beschreibung nannte noch 64 Zellen, obwohl der
neue Transport 512 unterstützt. Korrektur auf 512 mit dem Wald-Gate bereitgestellt.
Testvorbereitung sollte Forschung, Materialkapazität, freie Arbeitskräfte und
große freie Grundfläche vor der Pilotwahl gemeinsam bewerten.

## Abgenommen: Produktionslauf bis Zielbestand

Nach ausdrücklicher Umfangsauswahl: bis acht Güter und optional Forschung,
gleichzeitiger freier Zielbestand, automatische Pause, feste Zeitbudgets und
belegte Produktionsdiagnose. Menschliches Gate und gezielter Livetest am
2026-10-11 bestanden: Mehrfachziel, automatische Pause, bereits erfülltes Ziel,
unbekanntes Gut und Zutatenmangel mit Zeitlimit. [Nachweis](docs/production-target-runs.md).
Keine automatische Rezeptwahl, Produktionskettenplanung oder vollständige Wind-/
Lieferursachendiagnose. Große/spezielle Harzgebäude bleiben nachgelagert.

Verbesserung aus dem Pilot: unbekannte Zielgüter werden korrekt ohne Laufanlage
abgewiesen, die Antwort nennt aber nur `state_conflict`. Künftig das unbekannte
Gut bzw. die fehlende Beobachtung konkret ausweisen; erspart Diagnoseaufrufe.

## Beobachtung aus dem Bauzugangspilot: Materialwartezeit und Diagnosepräzision

Die neue Zugangsroute funktioniert auch mit Höhenwechsel. Der Testbau wartet
danach auf laufende Plankenveredelung. Vor künftigen Abnahmepiloten die benötigten
Materialien separat prüfen; Zugangserfolg und Materialwartezeit getrennt berichten.
Weitere Verbesserung: `final_work_access_blocked` derzeit auch dann, wenn schon
die erste Vorschau keinen Weg hatte. Ursprüngliche Ursache erhalten und spätere
Blockade nur bei einem tatsächlichen Wechsel ausweisen. Noch nicht umgesetzt.

## Begrenzt abgenommen: Bauarbeitszugang vor Auftrag und automatische Ergänzung

Implementiert: freie Fundament-Nachbarzellen mit konkreter nativer
Blockgeometrie, Distriktwegprüfung in Vorschau und vor Auftrag, spätere Blockaden,
automatische Wege/Treppen/Stützen sowie gezielte Fehlerdiagnose. Suchrand zwölf
Zellen bei unveränderten Budgets. Nach menschlichem Gate: negative Gegenprobe,
automatische Höhenroute, realer Bauarbeiterzugang, Fertigstellung und belegter
Bestandsweg geprüft. [Nachweise und Grenzen](docs/automatic-access-planning.md).
Ergänzend bestanden: Windrad mit Arbeitsweg unter dem Rotor, Mast-auf-Weg-Gegenprobe
und kleines Lager unter dem fertigen Rotor einschließlich Bau- und Eingangszugang.
Spätere Überbauung einer zunächst erreichbaren Arbeitszelle bleibt separat ungeprüft.
Offen bleiben native hypothetische Baustellenreichweite, automatische Routensuche
für bereits bestehende Reparaturprojekte und besondere Bauformen. Produktionsziele
und weitere Gebäudeunterstützung folgen separat; kein autonomer Kolonieausbau.

## Begrenzt abgenommen 2026-10-11: Gemeinsame Zugangs- und Bauhilfenplanung

Begrenzt live abgenommen: ebener Zweiteiler, Höhen-Sechsteiler und Bestandsweg-Gegenprobe.
`plan_large_project_access` ergänzt Wege, Treppen, Stützen und Abhängigkeiten,
prüft den Gesamtplan nativ und weist mögliche Rückbauhilfen aus. Keine allgemeine
Vorhersage nativer Baustellenreichweite, globale Standortsuche oder automatische
Räumung. [Umfang, Budgets und Nachweise](docs/automatic-access-planning.md).

## Erledigt im begrenzten Pilot 2026-10-11: Projektabschluss, Diagnose, Bauzugangsreparatur

Menschliches Gate und gezielter Livetest bestanden: fünfteilige Reparatur samt
Originalbau fertig, Folgeprojekt ohne Neuladen, zwei negative Kontrollfälle.
`diagnose_large_project` trennt frische Identität, Bauzugang, Materialbereitschaft,
Bestandsverluste und fremde Baustellen; letzte Guard-/Vorprüfungsfehler separat.
`close_large_project` gibt geklärte terminale Ledger frei, ohne ursprüngliche
Aufträge wiederaufzunehmen. `plan_large_project_repair`/`run_large_project_repair`
ergänzen begrenzte Bauhilfen und warten auch auf beauftragte Originalteile.
Keine globale Baustellenfreigabe; Restplan und Rückbau der Hilfen bleiben explizit.
[Vertrag, Grenzen und Nachweise](docs/project-recovery.md).
Damit ist der beauftragte Umfang der nachstehenden historischen P1-Einträge
begrenzt abgenommen. Allgemeine Wegplanung und Rezeptwahl bleiben separat offen.

## P0 — Dringend: vollständige Baucharge beauftragen, dann bis fertig simulieren

Explizites Umsetzungs-Go vom 2026-10-11: **Begrenzt live abgenommen.** Zwei
autonome Chargen mit zusammen 14 fertigen Teilen, Wegblockade und Zeitlimit
geprüft. Neuer `run_large_project`-Dispatcher in der Mod;
[Vertrag und Abnahmeplan](docs/autonomous-construction-batches.md).
Der bisherige Ablauf mit einem Auftrag, Simulationslauf,
Rücklesung und erneutem Agentenaufruf pro Bauteil ist zu interaktionsintensiv.

Gewünschtes Zielverhalten:

- Einen vollständigen Bauplan einschließlich Wege, Kraftanschlüsse, Stützen
  und temporärer Bauzugänge gemeinsam vorbereiten und prüfen.
- **Alle Bauaufträge gesammelt im Spiel erteilen.** Danach die Simulation
  einmal starten und bis zur tatsächlichen Fertigstellung aller Aufträge laufen
  lassen. Kein Agentenwechsel und kein weiterer advance-Aufruf pro Teil.
- Abhängigkeiten, Bauzugang und bei zunächst nicht platzierbaren Teilen die
  automatische spätere Auftragserteilung innerhalb derselben serverseitigen
  Charge lösen. Solche Ausnahmen ausdrücklich im Ergebnis ausweisen; nicht
  still zum bisherigen manuellen Einzelschrittverfahren zurückfallen.
- Der Server beobachtet Fortschritt und meldet nur fertige Gesamtcharge oder
  konkrete Blockade mit betroffenem Teil und Ursache. Bounded Lauf, Abbruch,
  Wiederanschluss und menschliche Eingriffe bleiben nachvollziehbar; keine
  Wiederholung bereits erteilter Aufträge.
- Abschluss kompakt: beauftragt/fertig/offen, Zugangsbelege und relevante
  Blockaden. Produktion und Kraftleistung bei Bedarf separat nachweisen.

Abnahmekriterium: gemischter Bauplan mit Treppe, Höhenstützen, Windturbine und
weiterem Gebäude; ein Chargenstart, keine Agenteninteraktion pro Bauteil,
Fertigmeldung für die gesamte Charge. Blockierter Kontrollfall meldet sich
gezielt. Anzahl externer MCP-Aufrufe gegen den aktuellen Ablauf vergleichen.
Priorität vor weiterer Skalierung des bisherigen Einzelauftrag-Verfahrens.

Ausführung erfüllt im begrenzten Pilot; offene Folgeverbesserungen: Wiederanschluss
mitten im aktiven Lauf gezielt live belegen, Material-/Zugangsblockaden genauer
diagnostizieren, terminale Testledger kontrolliert abschließen können und
Standortsuche/Räumung an Gesamtpläne anbinden. Keine automatische Wiederaufnahme
gestoppter oder unbestätigter Aufträge. Details/Nachweis im verlinkten Bericht.

## Produktionsketten: Livebefunde 2026-10-11

- **P1: Bauzugang reparieren und Bauhilfen einplanen.** Windturbine auf zwei
  fertigen senkrechten Wellen ließ sich nativ platzieren, war als Baustelle
  aber `buildersReachable=false`; keine Materiallieferung nach zwei Spieltagen.
  Temporäre Treppe/Plattform bereits vor dem Hochbau planen, danach gezielt
  entfernen können. Laufende Projekte benötigen einen begrenzten Reparaturplan
  mit bekannten Baustellen als Bestand und frischer Zugangsprüfung. Aktuell
  sperrt ConstructionIsolation zusätzliche Aufträge; Stop nach erfolgter
  Platzierung gibt das Großprojekt-Ledger nicht frei. Kein blindes Warten,
  kein Abbruch als vermeintlicher Reparaturweg. Menschliche Bautreppe bei
  (43,23,6), Drehung 0, ermöglichte die Fertigstellung. Turbine danach über
  beide Wellen real verbunden; nach Entfernung beider Papier-Laufräder 123
  Erzeugung beobachtet. Folgender Windumbau plant die Bautreppe vor den Wellen.
  Bestätigter Folgefall am unteren Sägewerk: Treppe (26,9,3), Drehung 0,
  vorab gebaut; hohe Turbinenbaustelle danach buildersReachable=true mit dieser
  tatsächlichen Zugangszelle. Turbine fertig und real verbunden. Keine
  allgemeine Erreichbarkeitsfreigabe für beliebige Türme daraus ableiten.

- **P1: Pflanzenabfrage ohne Kategorienfalle.** `kind=vegetation` schließt
  aktuell passend markierte Nutzpflanzen aus (`kind=planted`). Dadurch lieferte
  ein bewachsenes Weizengebiet null Treffer. Eine gemeinsame Pflanzenabfrage
  mit optionalen Filtern für Markierung, Art, Reife und Ertrag anbieten; leere
  Teilkategorie nicht als leere Fläche interpretieren. Kontrollread `all`
  bestätigte zehn lebende Weizenpflanzen mit rund 60 Prozent Wachstum.
- **P1: Sicheren Vorprüfungsfehler von unbestätigter Mutation unterscheiden.**
  Wechsel bestehender Karottenmarkierungen auf Weizen wurde vor Ausführung
  abgelehnt; Rücklesung zeigte unveränderte Karotten. Native `CanPlant`-Prüfung
  läuft vor der Mutation, aber die Antwort meldet pauschal mögliche
  Unbestätigtheit. Konkreten Fehler samt betroffenen Zellen und nachweislich
  nicht erfolgtem Schreibbeginn transportieren; echte Teiländerungen getrennt.
- **P1: Betriebsaufnahme als zusammenhängenden Nachweis unterstützen.**
  Spieler wählte WheatFlour/Bread manuell. Bäckerei anschließend besetzt und
  mit fünf Stämmen versorgt, aber noch ohne Mehl. Rezept, Personal, Brennstoff,
  Zutaten, Netzverbindung, momentane Kraft und tatsächlichen Produktausgang
  getrennt und kompakt zusammenführen. Rezept-Schreibfunktion bleibt offen.
- **P2: Bauwartezeit an Fertigstellung koppeln.** Achtteilige Kraftleitung
  benötigt weiterhin einzelne Simulationsläufe und Rücklesungen. Ein Schacht
  blieb nachts bei 97,5 Prozent stehen. Begrenzten Lauf mit Fertigstellungsziel
  und anschließendem sicheren Folgeschritt anbieten; kein blindes Zeitwachstum.
- **Kraftbeleg:** Drei VerticalPowerShaft auf derselben Rasterzelle, Z6/7/8,
  verbinden die Höhen über echte Top-/Bottom-Ports. Acht Leitungsteile fertig;
  Mühle und Windturbine danach im selben Zehn-Mitglieder-Netz. Flaute ergab
  trotzdem 0 Erzeugung bei 60 Bedarf: Verbindung ist kein Leistungsnachweis.

## Größere Baugruppe: Livebefunde 2026-10-11

- **P1: Bauplan um Betriebsvoraussetzungen ergänzen.** Kräuterkundler fertig,
  Rezept Antidote vorhanden, aber Dandelion fehlt; Mühle fertig ohne Rezept und
  Kraft. Kosten des Gebäudes reichen als Planungshilfe nicht. Rezeptwahl,
  Zutatenquellen, Lagerung und Kraftanschluss als getrennte beobachtete Lücken
  zusammenfassen. Vorhandene Rezept-Schreibfunktion-Lücke erneut bestätigt.
- **P2: Volumen, Fundament und tatsächliche Kraftanschlüsse unterscheiden.**
  Windturbinenports liegen am zentralen Sockel innerhalb der 3×3-Ausdehnung.
  Randnachbarschaft zu einer Mühle beweist deshalb keine Kraftverbindung.
  Vorschau sollte nutzbare Portzellen und konkrete native Ablehnungsgründe
  zeigen. Tiefer Standort abgelehnt, höherer ebener Standort gültig; Ursache
  des ersten Fehlers nicht eindeutig beobachtet, kein pauschaler Geländefehler.
- **Effizienzbeleg:** Fünf Sofortwege mit anschließender Mühle in einer begrenzten
  Agentenfolge abgearbeitet, weiterhin jeder Teil separat gelesen/validiert und
  nur einmal beauftragt. Kein neuer Servermodus, kein Retry; native Bündelung
  bleibt offen. Zustands-/Rückleseaufwand lässt sich so bereits besser bündeln.

## Bienenstock und Zusatzkraft: Livebefunde 2026-10-11

- **P2: Wirkungsbereiche strukturiert abfragen.** Bienenstock fertig, aber
  aktuelle Tools belegen weder tatsächliche Effektzellen noch betroffene Pflanzen
  oder aktive Wachstumsboni. Native Reichweite, wirksame Ziele und Grenzen
  getrennt liefern; Bau allein nicht als landwirtschaftlichen Nutzen ausgeben.
  Kontrollfälle: Pflanze innerhalb/außerhalb, leere Fläche und mehrere Effekte.
- **Kraftvorschau-Befund bestätigt:** Auch Laufrad Drehung 3 östlich der
  Papierfabrik meldet rotationMatches=false, ist fertig real verbunden und erhöht
  das Netz von 53/80 auf 106/80. Unpassende Drehung 1 wurde vor Auftrag anhand
  der Portposition und des fehlenden Eingangsanschlusses verworfen.

## Papierfabrik-Spieltest 2026-10-11

- **P1: Sofort fertige Wege innerhalb eines begrenzten Projektaufrufs abarbeiten.**
  Elfteilprojekt aus neun Wegen, Fabrik und Laufrad benötigt zwölf Advance-Aufrufe
  plus separate Rücklesungen. Jeden Weg nativ prüfen, genau einmal beauftragen,
  tatsächliche Fertigstellung lesen und bei eindeutigem Ergebnis direkt zum
  nächsten Teil übergehen. Bei Baustelle, Ablehnung oder ungeklärter Wirkung
  stoppen; harte Teile-/Zeitgrenze und kompakter Fortschrittsbericht. Ergänzt
  den bestehenden Wunsch nach wirkungsabhängiger Projektfortsetzung.
- **P1: Kraftvorschau fachlich verständlich machen.** Papierfabrik Drehung 0 und
  Laufrad Drehung 2: geometrischer Link vorhanden, rotationMatches=false;
  nach Fertigstellung genau dieser Anschluss real verbunden und Papier produziert.
  Bedeutung des Felds prüfen, Richtungsverträglichkeit getrennt von absoluter
  Drehung ausweisen. Kontrollfall: gegenüberliegende kompatible Ports versus
  gleichgerichtete/unpassende Ports. Keine Netzgarantie aus reiner Vorschau.
- **P2: Kraftbedarf und tatsächlich zugeteilte Leistung klar trennen.** Tagsüber
  Netz 53 Erzeugung/80 Bedarf, gleichzeitig actualInput=80 und Produktion.
  Semantik dieses Werts prüfen; nicht als volle Versorgung ausgeben. Nachts
  bei workingHours=false korrekt 0/0. Betriebszeit und Versorgung gemeinsam
  diagnostizieren, ohne eine Nachtpause als Defekt zu melden.

## Höhenwohnblock: Livebefunde 2026-10-11

- **P1: Gemischte Träger verständlich prüfen.** Dachterrasse auf vorhandenem
  Wohnhaus und geplanten Doppelplattformen war in gemeinsamer Vorschau
  native_invalid, bei fertigen Plattformen anschließend native_valid, Zugang
  verbunden. Bestand und geplante Träger pro Fundamentzelle aufschlüsseln;
  native_invalid nicht ohne weitere Evidenz als reines Trägerproblem umdeuten.
  Kontrollfall: dieselbe Geometrie mit unfertigen/fertigen Trägern und echte
  Kollision. Ausführungsprüfung weiterhin erst auf real fertigen Trägern.
- **P1: Höhenprojekte wirkungsabhängig vorantreiben.** Sechs Bauteile erfordern
  weiterhin separate Advance-, Zugang-, Simulations-, Fertig- und Pauseaufrufe.
  Begrenzten Ablauf analog zu Bauchargen anbieten: nach realer Fertigstellung
  früh pausieren, nächsten Teil frisch validieren, genau einmal beauftragen.
  Abbruch, Nutzeränderung und unbestätigte Wirkung sichtbar halten; keine Replays.
- **P2: Zugangskosten im 3D-Plan vergleichen.** Doppelwohnhaus am lokalen
  Treppenkern hat native Distriktdistanz 15; Terrassenbaustelle über oberen
  Spielerweg 72. Beide erreichbar, aber klarer Umweg. Kürzere Anschlüsse vor
  Ausführung vorschlagen; Distanz ist keine gemessene Gehzeit.

## Höhenbau nach gestopptem Projekt (2026-10-10)

Enge Korrektur vorbereitet, Gate offen: ausdrücklicher Stopp ohne jeden
Platzierungsversuch blockiert neue Projekte nicht mehr; alte ID bleibt terminal.
Teilbau-Abgleich und Abschluss nach tatsächlichen Aufträgen bleiben offen.

- **P1: Bestätigten Projektstopp abschließen, statt Sitzung dauerhaft sperren.**
  Ein vor dem ersten Bauauftrag gestoppter Abnahmepilot blockiert auch spätere,
  unabhängige Höhenprojekte. Der aktuelle Hochhausauftrag benötigt deshalb einen
  manuellen Spielstand-Neuladevorgang. Neues Projekt nach explizitem Abschluss
  eines nachweislich gestoppten Projekts ermöglichen; alte Aufträge niemals
  wiederholen. Bereits gebaute Teile und unbestätigte Wirkungen frisch abgleichen.
  Abnahme: Stopp vor erstem Auftrag und nach Teilbau; danach unabhängiges Projekt
  möglich, alte ID bleibt terminal; ungeklärte Wirkung blockiert weiterhin.

## Umzug auf Z4 — laufender Spieltest (2026-10-10)

Nutzerauftrag: unten Landwirtschaft/Wald, Wohnen und übrige Betriebe höher;
zusammenhängendes Viertel und kurze Wege. Verbesserungen nur dokumentieren.

- **P1: Räumen nach Wirkung beenden.** Wohncharge wartet derzeit ein festes
  48-Stunden-Fenster ab. Stattdessen eigene Räumziele auf tatsächliche Entfernung
  prüfen, früh stoppen und Bauprüfung fortsetzen. Restziele/Blockade nennen;
  Spielzeitlimit bleibt Obergrenze. Keine fremden Räumaufträge übernehmen.
- **P1: Fremdbaustelle verständlich melden.** Eine unabhängige Treppe mit einem
  fehlenden Brett blockierte trotz gültigem Wohnkandidaten die Projektausführung.
  Konstruktion-Isolation vor teurer Suche prüfen; konkrete Baustelle und Grund
  statt generischem state_conflict ausgeben. Räumlich unabhängige Aufträge erst
  nach belegter Zugangs-/Ressourcenverträglichkeit parallel zulassen.
- **P1: Straßen und Viertel gemeinsam planen.** Hauptachse, Eingänge, kurze
  Abzweige, Höhenanschlüsse und spätere Kraftkorridore explizit reservieren.
  Kandidaten nach zusätzlicher Weglänge, Laufwegen und Flächennutzung bewerten;
  nicht nur den ersten einzeln gültigen Standort bauen. Straßen erst entfernen,
  wenn keine verbleibenden Gebäude-/Baustellenzugänge davon abhängen.
  Konkreter Negativfall: Ein erreichbares Haus am Straßenende belegte selbst
  den Fortsetzungskorridor und sperrte die bereits geräumte Nordfläche ab.
  Bestehenden Zugang zu schützen genügt nicht; zukünftige reservierte Achse
  muss Kandidaten ausschließen. Haus musste für die Erweiterung wieder weichen.
- **P1: Änderungen seit Cursor.** Spieler bestätigte eigene Eingriffe; sechs
  neue Weg-/Treppenobjekte und entfernte Minihütte wurden erst durch sieben
  Bestandsseiten erkannt. Gebaute/entfernte Objekte, Aufträge, Einstellungen und
  Tempowechsel mit Sequenz, bekannter Quelle und sichtbaren Beobachtungslücken
  liefern. Chat-Auslöser: „Überprüf mal meine letzten Aktionen“. Unbekannte
  Urheber nicht als Spieleraktionen ausgeben; vorhandenen Aktionsjournalpunkt
  damit konkretisieren, kein zusätzlicher isolierter Logdienst.
- **P2: Aktueller Simulationszustand neben Abschlussbeleg.** Abgeschlossener
  Lauf meldete bestätigte Pause, Spiel war später wieder bei Tempo 7. Frischen
  Istzustand getrennt ausgeben; Baucharge bei ausschließlich fehlender Pause
  ohne Mutation fortsetzbar halten statt terminal neu anlegen zu müssen.
- **P1: Eingabefehler nicht als unbestätigte Spielaktion tarnen.** Lager-Modus
  `Accept` statt `accept` wurde vor Auftragsspeicherung abgewiesen, aber als
  möglicherweise unbestätigte Aktion gemeldet; reine Statuslesung einer fehlenden
  Charge ebenso. Feld, erlaubte Werte, `mutationAttempted=false` und echten
  `batch_not_found` ausgeben. Modus als Enum im Schema veröffentlichen.
- **P2: Lagerumzug bündeln.** Quell-/Ziellager gemeinsam prüfen, kompatibles Gut
  und Kapazität nennen, Quelle auf Leeren/Ziel auf Beschaffen konfigurieren und
  tatsächlichen Bestandsabbau beobachten. Abriss erst nach erklärtem Restbestand;
  keine erfundene automatische Übertragung. Inventarantwort standardmäßig nur
  belegte Güter plus ausgewähltes Gut liefern, nicht Dutzende Nullbestände.
- **P1: Vollständigen Fußabdruck gegen Gelände prüfen.** Regionalsuche fand eine
  Bäckerei mit verbundenem Eingang, deren gedrehter 2×3-Fußabdruck teilweise
  über die Geländekante ragte. Gemeinsame Vorschau wies sie korrekt ab; anderer
  Kandidat auf ebener Fläche war gültig. Planer muss alle tragenden Grundzellen
  filtern, nicht nur den Ursprung. Geometrische Ablehnung verständlich pro Zelle
  nennen und von Anschluss-/Fremdbaustellenkonflikten unterscheiden.

Zwischenstand: erstes neues Wohnhaus auf Z4 als finished_accessible bestätigt;
weitere Sechsercharge läuft. Kein abgeschlossener Umzug, keine Codeänderung.

## MCP-Log und Aufrufeffizienz — konkrete Livebefunde (2026-10-10)

Auf Nutzerauftrag während des 0.37-Stützenpiloten geprüft: aktuelle 32 Logeinträge
und Implementierung von `InvokeLogged`/`ActivityTools`. Nur verdichtete Befunde,
keine Rohlogs oder Spiel-IDs gespeichert. Vorschläge, noch nicht implementiert:

- **P1: Auftragszustände eindeutig trennen.** Projekt `waiting` mit Teilstatus
  `unconfirmed` nach erfolgreicher Platzierung wurde von der Aktionsprüfung als
  Wiederholungsrisiko blockiert. Normalen Zwischenzustand `awaiting_observation`
  nennen; terminale unbestätigte Wirkung separat halten. Ergänzend rein lesenden
  `refresh_large_project` anbieten: tatsächliche Objekte/Zugänge bestätigen, ohne
  jemals den nächsten Auftrag auszulösen. Abnahme: unterbrochene Antwort lässt
  sich ohne Schreibaufruf klären; kein doppelter Auftrag.
- **P1: Fachliches Ergebnis statt nur `ok`.** Advance, Projektleser und
  Fällmarkierung lieferten leere Summaries. Log getrennt um Transportergebnis,
  fachlichen Zustand/Grund, Projekt-/Laufkorrelation und knappen Effekt ergänzen:
  etwa „Treppe im Bau; 0/5 fertig; kein neuer Auftrag“ bzw. „2/2 markiert,
  Ernte ausstehend“. Fehlercode und ungültiges Feld nennen, keine Rohantworten.
  Abnahme: Bauauftrag, Warten, Ablehnung, Fertigstellung und bloße Lesung sind
  ohne zusätzliche Detailabfrage unterscheidbar.
- **P1: Wartegrund und gezielter Baustopp.** Ein Tagesfenster ließ die Treppe
  bei Materialfortschritt 0,8 stehen; erst getrennte Gebäude-/Güterabfragen
  zeigten vier Bretter am Bau und null frei verfügbare Stämme. Projektdiagnose
  mit Kosten, Baustellenbestand, verfügbaren Gütern und beobachtetem Zugang
  bündeln; keine erfundene Restliefermenge. Begrenzten Simulationslauf bei echter
  Fertigstellung der ausgewählten Teile stoppen lassen, mit hartem Zeitlimit
  und bestätigter Pause. Kein automatisches Setzen des Nachfolgers.
- **P2: Zielgerichtete Suchfilter.** Für drei Holzfäller und zwei Verbraucher
  mussten sieben Gebäudeseiten mit insgesamt 197 Objekten gelesen werden.
  `find_buildings` um Vorlagen-/Rollen- und Regionsfilter erweitern; Güter gezielt
  über IDs abfragen. Abnahme: diese Diagnose benötigt höchstens zwei Leseaufrufe,
  ohne Wegeinventar und unbeteiligte Güter zu übertragen.
- **P2: Grenzen im Werkzeugschema.** Mein Aufruf `inspect_goods(limit=64)` war
  ungültig; `limit=32` funktionierte. Numerische Min-/Maximalwerte im Inputschema
  und feldbezogene Fehlermeldungen ausgeben. Agent muss Fehlerdaten vor Zugriff
  auf `data.items` prüfen; keine zusätzliche Spielabfrage wegen Parserfehlern.
- **P2: Log gezielt und vollständig abrufen.** Aktuell maximal die neuesten 32
  aus 128 Einträgen, darunter die eigene laufende Logabfrage. Cursor/Sequenz,
  `hasMore`, sichtbare Ringpufferlücke und Filter nach Projekt/Lauf/Aktionen
  ergänzen; eigene Loglesung standardmäßig ausblenden. Ein fehlender Eintrag
  beweist bei Best-Effort-Telemetrie keine Nichtausführung. Vor dem Server
  blockierte Aufrufe können nur mit separater Client-/Supervisor-Telemetrie
  sichtbar werden; Spiel-Log darf dazu keine erfundenen Aussagen machen.
- **P2: Tempowechsel nachvollziehbar machen.** Im selben Pilot unterbrach ein
  Lauf wegen `speed_changed` (7 → 3); spätere Leser meldeten erst 0, dann 7.
  Urheber unbekannt, keine menschliche Aktion daraus behaupten. Lauf-/Tempoevents
  mit alter/neuer Stufe, Spielzeit und bekannter Quelle protokollieren; unbekannte
  Quelle ausdrücklich markieren. Aktuelles Tempo und eingefrorenen Laufabschluss
  gemeinsam ausgeben, damit ein alter Abschlusswert nicht als Istzustand gilt.

Empfohlene Reihenfolge: eindeutiger Rückbestätigungsleser und Ergebnislog,
danach Bau-/Materialdiagnose mit Fertigstellungsstopp, dann Suchfilter und Cursor.
Keine Ausweitung der laufenden 0.37-Abnahme; Umsetzung als abgegrenzte Folgearbeit.

## Siedlung als Gesamtanlage planen (Spielerfeedback 2026-10-10)

Die aktuelle Ansicht zeigt dicht aneinander gesetzte Testbauten, schmale
Verbindungen zu weiteren Einzelbauten und kaum erschlossene erhöhte Flächen.
Einzelne gültige Bauplätze ergeben keinen brauchbaren Gesamtplan für „jedes
Gebäude einmal“. Vor weiterem Katalogausbau zusammenhängende Bereiche für
Wohnen/Freizeit, Produktion und große Sondergebäude planen; benötigte Grundflächen,
Hauptwege, Höhenzugänge und Kraftleitungen vorab reservieren. Reservierungen und
regionale Suchabdeckung persistent im Plan führen; spätere Bauten dürfen die
vorgesehenen Korridore nicht versehentlich verbrauchen. Eigenständige Folgefähigkeit,
kein stiller Umfangszuwachs der laufenden 0.37-Abnahme.

## 0.37.0 größere Bauvorhaben — Stützenpilot bestanden (2026-10-10)

Kraftvorschau, reale Kraftverbindung und zweiteilige Baufolge bestanden.
Nach erneutem Gate jetzt auch fünfteilige Stützenfolge fertig: bedingte Planung,
strikte native Ausführungsprüfung, Bau-/Fertigzugänge und negative Trägerkontrolle.
Bestandsblockade und Stopp vor Bauauftrag geprüft. Fachlicher Abschluss dokumentiert;
keine pauschale Vorlagen-/Kraftdrehungsabnahme.

Beauftragter erster Umfang: 32 Teile / acht belegte Höhen, explizite Abhängigkeiten,
Fertigstellung vor Folgeauftrag, entrancelose Stützen/Kraftteile sowie öffentliche
Kraftports und Netzleistung. [Abnahmeplan](docs/large-projects-0.37.0.md).
F03 damit erweitert, noch nicht abgenommen und nicht vollständig erledigt:
automatische 3D-Routensuche bleibt offen. F04: neuer Distrikt-Lebenszyklus,
Gelände-Seitenanbauten und Mehrfacheingänge weiter offen. F01 bleibt ausdrücklich
offen; Vorschau ersetzt keinen vollständigen Baustellenzugangsnachweis.

Abnahme 2026-10-10: Vorschaltprozess einschließlich Spielkontakt nach Backendwechsel
und Rampen-/Höhensuche in beiden Pilotgebieten bestanden. Die untenstehenden
Gate-Vermerke sind historisch; Nachweise in PROJECT_STATE.md und den Fachdokumenten.
Offen bleiben dauerhaftes Suchregister und Client-Neustart bei erstmaliger
Werkzeugkatalogübernahme bzw. gegebenenfalls neu hinzugefügten Werkzeugen.

## Persistente Codex-Verbindung — Umsetzung am Gate (2026-10-10)

Vorschaltprozess, stop/start/status und Skriptsperre vorbereitet; Python-Tests im
menschlichen Gate. Offen: einmalige Clientumstellung, Gate und Stop/Start-Nachweis
ohne Clientneustart. [Prüfplan](docs/codex-mcp-supervisor.md).

## Blinde Flecken der Flächensuche — Umsetzung am Gate (2026-10-10)

Native Geometriemasken, automatische Folgeebenen und Suchabdeckung vorbereitet,
noch nicht live abgenommen. [Umfang/Prüfplan](docs/regional-survey.md).
Ein dauerhaftes sitzungsübergreifendes Suchregister bleibt Folgearbeit; aktuelle
Abdeckung wird pro Aufruf samt ungesuchten Ebenen geliefert.

Live belegt: obere Wege hinter Hauptsiedlung und neben Sägewerk sind bereits
an den Distrikt angeschlossen. Drei Bauvorschauen gültig, ein größerer Kandidat
abgelehnt. [Prüfumfang und Vorgehen](docs/regional-survey.md).

- RegionSurvey: ein Slope/other-Objekt macht derzeit seine ganze 8×8-Kachel
  unbekannt; im Sägewerksfeld unterdrückt das sämtliche Planungsaufrufe trotz
  direkt nachweisbarer gültiger Bauplätze. Öffentliche Hindernisgeometrie nutzen,
  soweit vorhanden; sonst Unsicherheit ausdrücklich lokalisieren/begründen und
  begrenzte direkte Kandidatenprüfung zulassen. Unbekannt nie einfach freigeben.
- Höhenübersicht und obere distriktverbundene Wege in die Gebietsauswahl aufnehmen;
  Höhenwechsel dürfen nicht lediglich als h aus der weiteren Planung verschwinden.
- Abdeckungsregister pro Rechteck/Höhe/Vorlage/Drehung: ungeprüft, beobachtet,
  Kandidat, validiert, abgelehnt, unknown; Zeit/Sitzung und Abbruchgrund/Optionslimit.
  Räum- und Bauänderungen invalidieren betroffene Ergebnisse.
- Kompakte Diagnose: whyUnknown, unterdrückte Suchfenster, planningCalls,
  ungesuchte Höhen und Abdeckung. Drei bevorzugte Fenster sind keine Vollsuche.
- Abnahme für spätere Umsetzung: bekannte Slope neben freiem angeschlossenem
  Bauplatz darf die Nachbarprüfung nicht verhindern; Hinderniszelle bleibt korrekt
  gesperrt/unbekannt. Höhe 3 versus 4 und größeres abgelehntes Gebäude als Kontrollen.
  Nach Folgeauftrag jetzt implementiert; Entwickler-Gate und Liveabnahme stehen aus. AGENTS.md unverändert.


## Aktionsjournal des Spielers — geplante MCP-Abfrage (2026-10-10)

Der menschliche Spieler greift auch ohne vorherige Ankündigung ein. Der Agent
soll die letzten X Aktionen im Spiel abfragen und seine Planung daran anpassen
können. Teil des Vorhabens Agenten-UI im Spiel; strukturierter MCP-Abruf auch
unabhängig von einer neuen Oberfläche. Noch nicht implementiert.

- Geplanter lesender Call, Arbeitsname `inspect_recent_actions`: aktuelle
  Sitzung, begrenztes `limit` für die letzten X Einträge, optional Cursor für
  Änderungen seit der letzten Abfrage sowie Filter nach Herkunft/Aktionsart.
- Erfassen: neue Bauaufträge, Abriss-/Räumaufträge, Flächenmarkierungen,
  Gebäude- und Lagereinstellungen, Pause/Tempo, Forschung und weitere relevante
  Spielereingriffe. Unterstützte Ereignisarten und Erfassungslücken offenlegen.
- Kompakte Einträge mit Sequenznummer, Spiel-/Aufnahmezeit, Aktion, betroffenem
  Objekt/Vorlage/Position, altem und neuem Wert soweit beobachtet sowie Ergebnis.
  Auftrag, beobachtete Statusänderung und spätere Wirkung getrennt ausweisen.
- Herkunft nur bei belegbarer Zuordnung als Mensch, MCP oder Simulation
  kennzeichnen; sonst unknown. Ein Zustandsvergleich allein beweist keinen
  menschlichen Eingriff. Geeignete öffentliche Spielereignisse zuerst prüfen.
- Begrenzter sitzungsgebundener Puffer; Cursor, ältester verfügbarer Eintrag,
  Überlauf, Erfassungsbeginn und Sitzungswechsel explizit melden. Keine
  rückwirkende Vollständigkeit vor Erfassungsbeginn behaupten; keine Rohjournale
  oder personenbezogenen Inhalte ins Repository schreiben.
- Chat-Aufruf **„Überprüf mal meine letzten Aktionen“**: nach Implementierung
  Journal abrufen, Änderungen knapp erklären und Konflikte mit laufenden
  Agentenaufträgen prüfen. Keine automatische Rücknahme menschlicher Änderungen.
  Optional vor Fortsetzung längerer Bauabläufe Änderungen seit Cursor abfragen.
- Liveabnahme: Mensch erteilt Bauauftrag, ändert Einstellung und Tempo ohne
  Vorankündigung; Agent findet die Einträge und Werte wieder. MCP-Kontrollaktion,
  automatische Fertigstellung, Limit/Cursor, Pufferüberlauf und Sitzungswechsel
  getrennt prüfen. Entwickler-Test-Gate bleibt maßgeblich.

## Gebäudespieltest 0.36.0 — laufende Effizienzbeobachtungen (2026-10-10)

- Kompakte Typenabdeckung aus aktuellem Bestand: Fertig-/Baustellenanzahl je
  Vorlage, Abgleich gegen regulären Katalog. Der Pilot benötigte sechs
  Objektseiten mit vollständiger Geometrie für lediglich 30 fertige Typen.
- Katalog nach fehlenden Typen, Materialkosten, Eingangshöhe und unterstütztem
  Baupfad filtern. Vollständiger aktueller Katalog: 157 reguläre Vorlagen,
  davon 15 im Baupfad nicht unterstützt; Entwicklerwerkzeuge separat ausschließen.
- Regionalsuche stärker nach tatsächlichem Anschluss und nutzbarer gleicher
  Höhe bewerten; 33 native Reads/zwölf Planungen im südlichen Testgebiet ohne
  Kandidaten. Räumbare Anbauflächen gesondert ausweisen.
- Präzise Eingabefehler statt pauschalem invalid_argument; Leser unterscheiden
  sich hinsichtlich erforderlicher/unerlaubter session. Schema bleibt maßgeblich.
- Vor Räumaufträgen verbleibende Pflanzmarkierungen und spätere Anschlusswege
  gemeinsam prüfen. Erste Charge räumte zwölf Objekte ohne danach nutzbaren
  Bauplatz; vier gezielt entfernte Karottenmarkierungen ermöglichten den Anschluss.
- Bei Fertigstellung aller Bauobjekte früh pausieren und Zugang prüfen:
  Bauhütte samt Wegen bei 28,625 von 48 Stunden fertig, Charge wartete weiter.
  Auf Nutzerauftrag MCP-only umgesetzt; Entwickler-Gate und Liveabnahme
  bestanden (zwei Lager nach 4/3 statt jeweils 24 Stunden), siehe [Bauchargen](docs/building-batch-workflow.md). Kein Frühstopp des
  Räumlaufs oder Hintergrunddispatcher in diesem Umfang.
- Konkrete fehlgeschlagene Spielvalidatoren und Stützbedingungen liefern.
  Dachterrasse auf freiem Boden mit Weg und Material: Planer findet Kandidaten,
  Bau lehnt mit state_conflict ab; Einzelvalidierung nur placement_invalid_in_preview.
  Stackable-Auflage vor teurer Räumung prüfen, nicht aus supported ableiten.

Evidenz und weitere Ergebnisse: [Spieltest 0.36.0](docs/playtest-0.36.0.md).
Übrige Vorschläge nur dokumentiert; Frühabschluss separat implementiert und abgenommen.

## Baumstumpf-Korrektur live bestätigt (2026-10-10)

Benutzer weist im Livepilot auf Baumreste hin. Growable.IsGrown und Lebenszustand
belegen keinen noch vorhandenen Fällertrag. Quellcodekorrektur nutzt öffentliche
Cuttable.Yielder-Daten: abgeerntete Reste separat zählen; nur aktiven positiven
Log-Ertrag empfehlen. Fehlende Ertragsdaten bleiben unbekannt und ausgeschlossen.
Keine heuristische Unterscheidung über Namen, Modelle oder Screenshot notwendig.
Livekontrolle vollständig über 45 Objekte: 34 abgeerntete Reste und acht junge
Bäume ohne Holzertrag ausgeschlossen, drei fällbare Kiefern mit je zwei Holz
erkannt. Stumpfauftrag abgelehnt; zwei echte Bäume markiert, Replay identisch,
anschließende Diagnose bestätigt beide als already_marked. Detailnachweis in
docs/forestry-efficiency-0.35.5.md. Übrige Versionsabnahme bleibt separat offen.


## 0.35.5 abgeschlossen (2026-10-10)

Holzdiagnose mit Ertragsprüfung, Sammelmarkierung/Replay, früher Holzstopp,
kompakte Chargenantworten und terminale native Bauablehnung live bestanden.
[Abschlussnachweis](docs/forestry-efficiency-0.35.5.md).

Folgearbeiten: native räumliche Filter statt 31 internen Reads je Forstdiagnose;
Forestry-Eingabefehler als Ablehnung vor Auftrag ausweisen statt pauschal
„möglicherweise unbestätigt“; Verbrauchsraten und allgemeine ereignisbasierte
Bauabschlüsse. Die Entwurfsabschnitte vom 5. Oktober sind teilweise umgesetzt.



## Screenshot 0.36.0 live abgenommen (2026-10-10)

Lesende Screenshot-Funktion, damit der spielende Agent bei Bedarf das aktuelle
Spielbild sehen kann. Expliziter Abruf statt Dauerstream; Bild als MCP-Bildinhalt
mit Sitzungskennung, Aufnahmezeit und Bildgröße zurückgeben. Nur Timberborn-
Spielansicht, keine Aufnahme anderer Desktopfenster; begrenzte Auflösung und
Antwortgröße. Aufnahme bei fehlender Kamera oder ungeladenem Spiel klar ablehnen.
Keine Screenshots in Git und kein automatisches Mitschreiben einer Bildhistorie.
Strukturierte Daten bleiben Grundlage der Aktionen; Bild dient ergänzender
Orientierung. Entwickler-Gate und begrenzter Livetest bestanden: Spielbild,
Auflösungsbegrenzung, Kameraposition/Höhe, Richtung, Bildwinkel und Bildweite,
sowie Ablehnung fremder Session. [Abnahme](docs/screenshot-0.36.0.md).
Minimierte Ansicht/fehlende Kamera nicht zusätzlich live provoziert.


## Agenten-UI im Spiel — geplantes Vorhaben (2026-10-10)

Eine Mod-Oberfläche für die Zusammenarbeit zwischen menschlichem Spieler und
Agenten vorsehen. Umfang und Interaktionskonzept vor Umsetzung konkretisieren;
dieser Eintrag ist Planung, keine bereits implementierte Funktion.

- **„Zeigen“-Button:** Der Spieler stellt Kamera und UI ein und hält per Knopf
  diese Spielansicht für den Agenten fest. Vorhandene Screenshot-Funktion samt
  Kamerametadaten nutzen. Aufnahmezeitpunkt ist der Klick; späteres Abholen darf
  nicht stillschweigend eine inzwischen veränderte Ansicht aufnehmen.
- Übergabe und sichtbaren Status festlegen: Aufnahme bereit, vom Agenten abgeholt
  oder fehlgeschlagen. Ein Klick allein startet noch keinen Agentenlauf; den
  Abruf beim nächsten Agentenkontakt ausdrücklich vom aktiven Benachrichtigen
  unterscheiden. Begrenzte sitzungsgebundene Aufbewahrung, keine Bildhistorie.
- Abnahme: Ansicht A zeigen, Kamera auf B bewegen, beim Agenten weiterhin A
  mit zugehörigem Zeitpunkt und Kameradaten erhalten; Sitzungswechsel und
  fehlgeschlagene Aufnahme eindeutig behandeln. Entwickler-Test-Gate bleibt.

Bis dahin funktioniert „schau mal“ im Chat mit einer Aufnahme zum Zeitpunkt des
Werkzeugaufrufs. Der Ingame-Button gehört zum größeren UI-Vorhaben, nicht zu einer
noch offenen Pflichtfunktion des abgenommenen Screenshot-Releases 0.36.0.

## Effizienter Holzausbau – Vorschlag, keine Umsetzung (2026-10-05)

Evidenz: Radbau nach einem Spieltag wegen Holzlieferung unvollständig. Für die
Entscheidung waren getrennte Güter-, Inventar-, Vegetations-, Markierungs- und
Arbeitsreichweitenabfragen nötig; Fällmarkierungen wurden über acht Seiten gelesen.
Erst der Abgleich auf der unteren Ebene lieferte geeignete Kiefern. Am Ende
27 Holz insgesamt, aber nur zwei frei verfügbar; neun im Saunainventar.

Priorisierte Erweiterungen bestehender Werkzeuge:

1. **Regionale Holzdiagnose (höchster Nutzen):** vorhandene Gebietssuche mit
   nativer Arbeitsreichweite, Wachstum, Lebenszustand und aktuellen Fällmarkierungen
   serverseitig verknüpfen. Für einen Holzfäller und ein begrenztes Gebiet nur
   geeignete Kandidaten sowie Ausschlusszähler liefern; bekannte Höhen mitführen.
   Unbekannte Reichweite gesondert ausweisen, keine Freigabe daraus ableiten.
   Bestehende regionale Suche erweitern statt ein paralleles Planungssystem bauen.
2. **Verfügbarkeit und Verbrauch gemeinsam erklären:** freie Holzbestände,
   Baustellenbedarf und relevante Gebäudeinventare getrennt anzeigen. Bestand,
   Transportreservierung und gemessener Zu-/Abfluss nicht vermischen. Raten nur
   aus zwei zeitlich bezeichneten Beobachtungen, andernfalls ausdrücklich unbekannt.
   So wird sichtbar, ob Ernte, Transport oder Verbrauch den Ausbau begrenzt.
3. **Begrenzte Sammelmarkierung:** geprüfte Kandidaten in einem Auftrag markieren,
   mit Sitzung, erwarteter Markierung und aktuellem Reichweitenbeleg. Ergebnis
   zählt angenommen/abgelehnt/unbestätigt; keine automatische Wiederholung nach
   unklarer Teiländerung. Das bestätigt Fällaufträge, nicht gefällte Bäume.
4. **Bedingtes Zeitfenster:** optional nach beobachteter Holzlieferung oder einem
   definierten frei verfügbaren Vorrat pausieren, mit harter Zeitgrenze. Nicht
   nur auf feste Spieltage warten. Abschluss nennt Stoppgrund, Veränderungen und
   bestätigte Pause; keine Aussage über langfristige Nachhaltigkeit.
5. **Kompakte Standardantworten:** Zustand, Kernergebnis, Ausschlussgründe,
   Vollständigkeit und notwendige nächste Aktion. Einzelobjekte, alte Schritte
   und wiederholte Einschränkungen nur auf Detailabruf; entscheidende Unsicherheit
   und Teilfehler bleiben immer sichtbar. Gebiet direkt filtern statt globale
   Markierungsseiten beim Client zusammenzuführen.

Vorgeschlagener Abnahmepilot für eine spätere Umsetzung: ein Holzfäller, ein
begrenztes Gebiet mit mindestens einem geeigneten und einem ungeeigneten Baum
(unreif, schon markiert oder außerhalb Reichweite). Diagnose gegen vorhandene
Detailwerkzeuge abgleichen; bei falschem Einschluss stoppen. Ziel für den normalen
Ablauf: Diagnose, Sammelauftrag, begrenzter Lauf und Abschluss in höchstens vier
Client-Aufrufen, ohne zusätzliche manuelle globale Seitenabfragen. Das ist ein
Entwurfsziel, kein gemessener Gewinn. Vorher/nachher Aufrufzahl, Antwortumfang und
Zeit bis zur Entscheidung messen; einmalige Kontrollabfragen separat zählen.


## Kraftanschluss für Produktionsausbau (2026-10-05)

MCP liefert Betriebswarnung fehlender Kraft, aber keine strukturierten
Kraftanschlüsse, Netze, Leistung und Verbindungsgeometrie. Radposition muss aus
einem funktionierenden Vergleich abgeleitet und über tatsächliche Produktion
geprüft werden. Ergänzen: Anschlusspositionen/-richtungen, Netzzugehörigkeit,
Erzeugung/Verbrauch und Vorschläge für geprüfte Kraftverbindungen.

Live-Nachweis inzwischen bestanden: Rad neben Zahnradwerkstatt, Kraftwarnung
verschwunden, fünf Zahnräder in Sauna verbaut und vier weitere im Ausgang.
Das ist ein konkreter Produktionsnachweis, keine allgemeine Anschlussprüfung.
Zusätzlich Materialengpässe mit passenden Arbeitsflächen verknüpfen: Radbau
wartete auf Holz; globaler Bestand, Gebäudeinventare, Fällmarkierungen und native
Holzfällerreichweite mussten getrennt gelesen werden. Vorschlag: kompakter
Engpassbericht mit tatsächlich erreichbaren reifen, noch unmarkierten Bäumen.


## Weitere Beobachtungen beim Farmbau (2026-10-05)

- Räumzeitfenster sollten Arbeitszeiten berücksichtigen: acht Pflanzen nach acht
  Abend-/Nachtstunden nur halb entfernt, erst im folgenden Arbeitsfenster vollständig.
  Acht Einzelmarkierungen plus separate Flächenlöschung zeigen Bedarf für gebündelte
  Umwidmung von Anbaufläche zu Baufläche mit verifiziertem Räumabschluss.

- Sehr kleine Suchfenster liefern keinen Kandidaten trotz geometrisch freiem
  Standort und nahem Weg. Derselbe Standort im 8×8-Fenster als Option mit einem
  neuen Weg gefunden. Suchfenstergrenze/Anschlussroutengrenze als Ursache ausweisen;
  Diagnose requires_game_validation ohne Grund ist für die Entscheidung zu schwach.
- Bündelablauf wählt first_candidate; zur bewussten Erhaltung eines freien
  Durchgangs war manuelle Auswahl von Option 1 und Einzelprojektaufruf nötig.
  Freizuhaltende Korridore oder ausgeschlossene Grundrisse in Chargen unterstützen.
- Farmreichweite plus Bodenfeuchte funktionierte für 38 neue Karottenmarkierungen;
  fünf separate Schreibaufrufe nötig. Begrenzte Mehrrechteck-Pflanzaufträge würden
  den Ablauf verkürzen, mit identischen Vorbedingungen und getrennten Nachweisen.


## Bauprüfung: gestrandete Biber trotz fertigem Gebäudezugang

Südliche Wohncharge baute zwei Häuser mit finished_accessible. Anschließend drei
Biber südlich der Häuser gestrandet; bei freien Betten obdachlos/erschöpft.
Beide Häuser selbst fertig, unpausiert, Distrikt zugeordnet. Engstelle außerhalb
des bestehenden Wegnetzes durch Hausgrundrisse geschlossen. Zweites Haus gezielt
entfernt; nach acht Spielstunden keine Stranded-/Exhausted-Warnung mehr. Gebäude-/Wegzugangsprüfung deckt aktuelle
Biberpositionen und freie Geländeverbindungen nicht ab. Vor weiteren Flächenchargen
freie Rückwege bzw. betroffene bewegliche Einheiten prüfen und diese Nachweislücke
im Ergebnis deutlich ausweisen. Kein behaupteter allgemeiner Navigationsschutz.

## Spielabschnitt 2026-10-05: Chargenfehler und unnötiges Warten

- Definitive native Ablehnung vor Auftragsregistrierung bleibt als pending_build /
  batch_interrupted hängen. Live: zweites Wohnhaus; inspect-Auftrag state_conflict,
  Objekt fehlt, keine Baustellen, gezielte Vorschau buildingValid=false. Ursache
  beibehalten und als abgelehnt abschließen; keinen unbestätigten Auftrag vortäuschen.
- Erstkandidat kann native Bauvalidierung verfehlen. Nach eindeutig mutationsfreier
  Ablehnung andere Kandidaten prüfen, mit festem Versuchslimit und erhaltener Diagnose.
- Fertiges Haus wartet weiter bis Ende des fixen Zeitfensters. Optionale frühzeitige
  Beendigung mit bestätigter Pause würde lange Chargen deutlich beschleunigen.
- Wartende Antworten wiederholen Suchdetails und den Abschluss des Vorgängerhauses.
  Aktuellen Baufortschritt, Restzeit und empfohlenen nächsten Abfragezeitpunkt anzeigen.
- Wohnbelegung je Haus fehlt: freie Betten plus obdachlose Biber sind ohne
  Bewohner-/Kapazitätsdaten pro Gebäude schwer zu erklären; Distrikt und Zugang
  allein belegen keine tatsächliche Wohnnutzung.
- Katalog-supported ist für Gebäude ohne Eingang oder mit erhöhtem Eingang zu grob:
  Hecke/Laterne ohne Eingang, Doppelwohnhaus mit Eingang auf z+1. Direkt passende
  Ausführungsfähigkeit samt Grund ausweisen, bevor Forschung/Standortsuche erfolgt.


## Beauftragt: automatische Teilflächenwechsel in Bauchargen

Go nach zwei räumlich ausgeschöpften Chargen. Basis plus drei zusätzliche
8×8-Flächen implementiert, globale Räum-/Zeitgrenzen und gespeicherte Wechsel.
Menschliches Gate und begrenzter Flächenwechsel-Livetest bestanden. Details:
docs/building-batch-workflow.md. Globale gemeinsame Standortoptimierung bleibt offen.


## Chargenplanung: Alternativen überlappen (Live 2026-10-05)

Drei-Häuser-Charge baute das erste Haus fertig/erreichbar und stoppte dann korrekt
mit no_candidate_in_authorized_region: regionale Alternativstandorte waren
nicht gleichzeitig nutzbar. Verbesserung: vor Chargenstart Anzahl gemeinsam
nutzbarer Standorte bzw. Konflikte ausweisen, keine Kapazität aus Kandidatenzahl
ableiten. Fortsetzung mit begrenzter bestehender Räumcharge, keine neue Funktion.

## Produktionsflächen: Feuchte mit Arbeitsreichweite verbinden

Optionales Arbeitsgebäude und abschaltbare Bauplanung in survey_region vorbereitet.
Pilot mit drei tatsächlich wachsenden Karotten bestätigt Nutzen des Abgleichs.
Menschliches Gate und kombinierter Livetest bestanden; Bridge unverändert 0.35.4.
Grenze: bis 64 interne Reichweitenseiten; spätere native Regionsfilterung kann
auch diese internen Reads reduzieren. Keine Feld-/Forstautonomie behauptet.


## Versionswechsel: vergessene Clientfreigabe verhindern

0.35.4 zunächst von den eigenen Versionslisten abgelehnt. Korrektur aller
betroffenen Listen und manifestgebundener Clienttest ergänzt; Gate und Livetest bestanden.
Weitere Verbesserung: verteilte Versionslisten konsolidieren und inkompatible
Bridge-Version auch im Aktivitätslogging als verständlichen Fehler ausgeben;
aktuell generischer MCP-Aufruffehler. Kein größerer Umbau in dieser Reparatur.


## Direkter Nutzerauftrag: grüne Pflanzflächen erkennen

0.35.4 vorbereitet: Bodenfeuchte für leere Felder in inspect_map_region und
survey_region; öffentliche SoilIsMoist-Abfrage statt Ableitung aus Pflanzenstress.
Separate kompakte Feuchtigkeitskarte und freie feuchte Rechtecke, keine zusätzlichen
Spielabfragen. Menschliches Gate und feucht/trocken/Höhen-Livekontrolle am 2026-10-05 bestanden.
Pflanzenspezifische Eignung und Arbeitsreichweite bleiben separate Aussagen.


## Live bestanden: regionale unknown-Ausbreitung

Ein other-Objekt sperrte bisher freie Zellen der gesamten Suchregion.
Auf tatsächlich überlappende Teilflächen begrenzt, zwei Regressionsfälle ergänzt.
Menschliches Gate und Live-Gegenprobe am 2026-10-05 bestanden: betroffene
Teilfläche bleibt unbekannt, freie Rechtecke anderer Teilflächen wieder sichtbar.


## Bedienungsdiagnose aus regionalem Baupilot

- waitSeconds-Grenzen direkt in Kurzbeschreibung nennen: Einzelprojekt 0–20,
  Charge 0–45. Metadatenschema allein verhindert Parameterverwechslungen nicht.
- Für develop/advance/inspect_building_completion implementiert, Gate und begrenzter Livetest bestanden: vor Eingriff erkannte Parameterfehler konkret benennen; pauschaler Hinweis
  auf möglicherweise unbestätigte Aktion verursacht unnötige Diagnoseabfragen.
- Regionaler Baukandidat inzwischen bis finished_accessible praktisch bestätigt;
  nächste wirtschaftliche Grenze sind Holz- und Nahrungsproduktion.


## Priorität: regionale Flächensuche — Vorschlag nach Ausbaupilot

Rund 40 MCP-Aufrufe für zwei Wohnhäuser, eine Transportstation und sieben
Feldzellen. Regionale Suche soll Gelände, Bestandszugänge und geeignete Bau- und
Anbauflächen gebündelt ausweisen, statt blindes Räumen oder einzelne 8x8-Suchen
zu skalieren. Kandidaten bleiben Vorschläge; native Bau-/Zugangsprüfung bleibt
verbindlich. Keine Gebäudefertigstellung, Pflanzung oder Ernte aus Planung ableiten.
Nach ausdrücklichem Go implementiert und nach menschlichem Gate begrenzt live
abgenommen: 32 native Leseaufrufe, vier Kandidaten, Bestands-/Feldkontrollen
korrekt; laufendes Spiel abgelehnt. Keine allgemeine Bau-/Anbaufreigabe.
Umfang und Grenzen: [regionale Flächensuche](docs/regional-survey.md).
Chargenantworten weiter kürzen: unveränderte Suchdetails/alte completion-Werte
nur bei Bedarf; wartender Status braucht nächsten sinnvollen Abfragezeitpunkt.


## Live bestanden — temporäre Spielsperren in Zeitläufen (2026-10-04)

Menschliches Test-/Installationsgate abgeschlossen; alle fünf installierten
Paketdateien stimmen mit Paket 0.35.3-20261004-231320-8e75fd0c überein.
Gezielter Vier-Tage-Lauf: beim vom Nutzer ausgelösten Speichern dreimal
running/game_speed_locked mit Tempo 0 beobachtet; danach derselbe Lauf wieder
running/advancing mit Tempo 7. Ziel und Echtzeitbudget unverändert.
Abschluss nach exakt 96 Spielstunden und 271,166 Echtzeitsekunden: completed,
target_reached, pauseConfirmed, Überschwingen 0. Aktuelles Tempo separat 0.

Negativkontrolle: ausdrückliche MCP-Pause unterbricht einen zweiten aktiven Lauf
mit explicit_speed_change; aktuelles Tempo danach separat 0 und Spielzeit stabil.
Der eingefrorene Unterbrechungsbeleg enthält noch Tempo 7 vor Befehlswirkung;
deshalb die aktuelle Pause immer gesondert lesen.

Öffentliches SpeedLockChangedEvent über EventBus angebunden; keine Entsperrung
oder Wiederbeschleunigung durch die Bridge. Sperre beim Speichern jetzt direkt
nachgewiesen; eine rückwirkende eindeutige Ursache aller früheren Abbrüche ist
nicht beweisbar. Budgetablauf/Abbruch während Sperre synthetisch geprüft, im
Livetest nicht zusätzlich provoziert. UI-Geschwindigkeitswechsel separat nicht
live getestet. Gesamtziel bleibt offen: zuletzt 27 Biber, 23/157 Vorlagen.

## Live bestanden — Bauchargen (2026-10-04)

Menschliches Gate mit anschließendem „live bereit“; fünf installierte Paketdateien
gegen das neue Paket geprüft. Zwei Gebäude automatisch nacheinander fertig und
zugänglich: kleines Lager mit Carrot/obtain, danach Mini-Wohnhaus. Zehn tote Bäume
automatisch geräumt, sechs lebende erhalten. Räumwartefenster tatsächlich gestartet;
bei der unabhängigen Rückfrage waren die Ziele bereits entfernt. Drei native
Zeitfenster je exakt 24 Stunden, kein Überschwingen, Pause jeweils bestätigt.
Fünf ausführende Chargenaufrufe bis Abschluss; Replay/Budgetkontrolle zusätzlich.
Identischer Start und abgeschlossene Fortsetzung unverändert, Budgetänderung
abgelehnt. Negative bebaute 1×1-Fläche stoppt vor Bau/Räumung und Folgegebäude.
Unabhängige Zugangs-/Lagerabfragen stimmen mit Abschluss überein. 21 Betten,
17 belegt; kein Nachweis für 100 zufriedene Biber oder neue Vorlagenabdeckung.

Grenzen: kein erzwungener Prozessabbruch im Livetest; Materialmangel und erschöpftes
Räumbudget synthetisch abgedeckt, hier nicht zusätzlich live provoziert.
Baustellenkonfiguration in diesem Pilot erst nach Fertigstellung zurückgelesen.
Verbesserungen: vollständiges Räumbudget läuft trotz früher geräumter Ziele aus;
Budgetkonflikt meldet zu allgemein invalid_argument; gespeicherter completion-Wert
kann während Räumung noch zum vorherigen Gebäude gehören. Keine falsche Aktualität
behaupten: checkpointOnly und Zeitstempel beachten. Breitere Vorlagen-/Höhenplanung
und kleinere gezielte Räumflächen bleiben offen.


## Vor Test-Gate — Standortwahl und komplette Bauchargen

Erneutes Nutzer-Go: 1–8 Gebäude, begrenzte Fläche, Rotationensuche, einmalige
Vegetationsräumung mit festem Zeitbudget und sequenzielle Fertigstellung als
MCP-Ablauf implementiert. Persistente Wiederaufnahme und kompakte Checkpoints;
kein neuer Hintergrundprozess. Synthetische Prüfungen ergänzt, Ausführung und
Livetest stehen aus. Räumung umfasst bei Bedarf alle freigegebenen passenden
Ziele der Region; minimale Räumfläche und Höhenplanung bleiben offen.
[Pilot und Grenzen](docs/building-batch-workflow.md).

## Live bestanden — vollständiger einzelner Bauablauf

Reparatur nach menschlichem Gate bestanden: Lager plus zwei Wege fertig,
Zugang und Konfiguration bestätigt, Zeitbudget exakt und Replay ohne neue Zeit.
Explizite Komponentenabwesenheit wird korrekt von unknown unterschieden.

Erneutes Nutzer-Go nach Aufwandsalarm erhalten. Gesamtaufruf für Planung,
Baustart, begrenzte Simulation und gebündelte Abschlussprüfung implementiert;
Wiederaufnahme über vorhandene native IDs, keine automatische Budgetverlängerung.
Suchursprungdiagnose bei fehlendem Kandidaten. Menschliches Gate und gezielter
Livetest bestanden. Mehrgebäude-Warteschlange, automatische
Räumplanung und vollständige Ursachenaufteilung der Suchregion bleiben offen.
[Abnahmeplan](docs/building-completion-workflow.md).

## Live bestanden — gebündelte Bau- und Räumaufträge

Nach ausdrücklichem Nutzer-Go: zwei MCP-Werkzeuge über bestehenden nativen
Services, ohne neue Abhängigkeit. Räumstapel bis 16
Vegetationsziele mit Einzelbelegen/Stopp beim ersten Fehler; Baustart bündelt
Planung und native Pilot-Ausführung des ersten Kandidaten. Bestehende Rechte,
Zugangsprüfungen und initiale Lagerkonfiguration bleiben wirksam.
Sammelräumung und korrigierter Konfliktstopp live bestanden; explizites
state_conflict und unverändertes Folgeziel bestätigt. Gebündelter Baustart
live bis zum fertigen erreichbaren Lager samt erhaltener Konfiguration geprüft.
Drei Ziele in einem Aufruf und Baustart ohne manuelles Kopieren von
planKey/optionIndex. Mehrere Gebäude
in einer Bauwarteschlange, automatische Simulation und Sammelräumung anderer
Objektarten bleiben Folgearbeit; keine allgemeine autonome Ausbaupipeline behauptet.

## Weitere Live-Lücken beim Kolonieausbau

- Räumaufträge in Bauablauf integrieren: gleicher Vegetationsbatch kann sofortige
  Entfernung und offene Arbeiteraufträge mischen. Live blieben vier tote Kiefern
  zunächst markiert; nach einem begrenzten Spieltag waren alle entfernt. Bau
  darf erst nach tatsächlicher Räumung starten. Status und Zeitbudget dieser
  Vorbereitung mitführen, keine Wiederholung der ursprünglichen Markierung.
- Generische erhöhte Gebäudeeingänge: DoubleLodge/TripleLodge sind im Katalog
  als geometrisch unterstützt ausgewiesen, ihr Eingang liegt aber eine Ebene
  über dem Ursprung. Ebener Projektablauf reicht nicht; vorhandene feste
  Vertikalpiloten sind noch kein allgemeiner Anschluss solcher Wohngebäude.
- Suchbereich vorab gegen gedrehte Grundfläche und Wegeingang erklären:
  Forsthausursprung geometrisch frei, aber Grundfläche teilweise außerhalb der
  angegebenen Region. Erst eine zusätzliche Spalte ermöglicht den Kandidaten.
  Ursprungsdiagnose meldet requires_game_validation und erklärt diesen Planer-
  Ausschluss nicht. Konkrete benötigte Suchgrenzen bzw. Ablehnungszähler ausgeben.
- Flächenvorprüfung mit konkreten blockierten Zellen und Gründen: `set_area`
  meldet bei besetzter Pflanzfläche nur `invalid_argument` und einen pauschalen
  Hinweis auf möglicherweise unbestätigte Wirkung. Live: Kiefern vor der Farm,
  keine Markierungen verändert. Validierungsfehler vor Mutation von tatsächlich
  unbestätigten Änderungen unterscheiden; `precheck_area` vorsehen.
- Bauplanung ohne Kandidaten: Ablehnungsgründe aggregieren (Gelände, Objekte,
  Anschluss, Wegbudget), damit kein Suchraster durchprobiert werden muss.
  Erneut live: zwei 8x8-Suchen für Lagerfeuer mit je 64 Ablehnungen ohne
  Ursachenaufteilung. Gezielte Gelände-/Objektabfrage zeigte neun tote Kiefern
  auf ebener 3x3-Fläche; nach Räumung genau ein Kandidat ohne neue Wege.
- Begrenzte Vegetations-Sammelaufträge mit Einzelbelegen und Konfliktprüfung:
  neun tote Bäume erforderten neun serielle MCP-Aufrufe. `operation=mark`
  lieferte hier unmittelbar `removed=true, marked=null`; Antwort und Beschreibung
  sollten direkte Entfernung klar von einem noch offenen Arbeiterauftrag trennen.
- Räumliche Filter für Fäll-/Pflanzmarkierungen und Reichweiten: zwanzig lokale
  Fällfelder erfordern derzeit den Abgleich von 190 kolonieweiten Markierungen.
- Fortschrittsbericht: lebende Bevölkerung, beobachtete Grundbedürfnisse und
  echte Wohlbefindenspunkte sowie gebaute/fehlende Katalogvorlagen gemeinsam.
  Vorhandener Fertigbestand ist kein vollständiges historisches Bauregister.
- Kompakte Bevölkerungsdiagnose: Altersgruppen, Geburten/Todesfälle im Intervall,
  belegte Familienwohnungen und tatsächliche Wohlbefindensboni. Einzelne
  Bestandsänderungen (11 → 10 → 11) erklären weder Ursache noch Wachstumsrate;
  keine Todesursache oder allgemeine Zufriedenheit aus Bedürfnisflags ableiten.
- Energieplanung: tatsächliche Anschlusspunkte, Netzzugehörigkeit, Erzeugung und
  Bedarf strukturiert anbieten. Sägewerk/Laufrad liefern live Bretter, aber die
  Verbindung ist bisher erst über Produktion und verschwundene Statusmeldung
  nachgewiesen; Vorabplanung kennt nur Gebäudegeometrie und Wegeingänge.

## Kurzfristig priorisiert — kompakte MCP-Antworten (P1)

Folgeschritt live bestanden: `inspect_colony_overview` ersetzt mehrere
MCP-Abfragen durch eine begrenzte lesende Zusammenführung, ohne weniger interne
Bridge-Abfragen oder Atomizität zu behaupten. Bauprüfung entfernt weitere
Zell-/Quellenangaben, erhält separate Unknowns und Nachweisgrenzen.
Nutzen im pausierten Spiel belegt: ein statt drei MCP-Aufrufen, 60 % weniger
Antwortzeichen bei gleichen relevanten Daten und allen 42 Bedürfnissen.
Bauprüfung 16 % kürzer, vollständige Projektion fachlich gleich.
Allgemeine Ausbauabläufe und weitere Werkzeuggruppen bleiben offen.

Nutzerauftrag vom 2026-10-04: zeitnah umsetzen, direkt nach dem laufenden
0.35.3-Test-Gate. Standardantworten sollen nur das für die konkrete Aktion oder
Entscheidung Nötige enthalten. Im Playtest wiederholen sich lange `limitations`,
verschachtelte Prüfberichte, Katalog-/Fähigkeitsbeschreibungen und vollständige
Objektzustände auch bei einfachen Status- und Einstellungsabfragen.

- Kompakte Standardansicht: Ergebnis, relevante Werte/Änderungen, notwendige IDs,
  konkrete Fehler oder Blockierungsgründe. Unbekannt/unbestätigt und wesentliche
  Nachweisgrenzen müssen eindeutig bleiben.
- Statische Erläuterungen einmal über Fähigkeiten/Dokumentation bereitstellen;
  ausführliche Geometrie, Prüfbelege und vollständige Objektdetails gezielt abrufbar
  machen. Keine wiederholten vollständigen Zustände bei kleinen Änderungen.
- MCP-Text und `structuredContent` auf unnötige Doppelübertragung prüfen;
  Protokoll- und Client-Kompatibilität erhalten. Lange Werkzeugbeschreibungen
  ebenfalls auf Wiederholungen und veraltete Aussagen prüfen.
- Kleiner Pilot mit Status, Lagereinstellung und Bauprüfung: Antwortgröße vorher/
  nachher messen, deutlich reduzieren und dieselben Entscheidungen ermöglichen.
  Fehler-, Konflikt- und Unknown-Fälle sowie abrufbare Details mitprüfen;
  erst danach auf weitere Werkzeuge ausweiten.

Status: erster Pilot nach menschlichem Gate live abgenommen:
Status, Lagerlesen/-änderungen und generische Bauprüfung mit optionalem
`detail=full`. MCP-Text bleibt aus Kompatibilitätsgründen inhaltlich gleich zur
strukturierten Antwort; beide tragen dieselbe gekürzte Projektion. Gebündelte
Ausbauabläufe und weitere Werkzeuggruppen folgen nach gemessenem Pilotnutzen.
Lagerlesen live 53 % kürzer; Bauprüfung nur 7 % kürzer. Dort bleibt der Großteil
in verschachtelten Diagnosebelegen: nächster Schritt ist eine kompakte fachliche
Zusammenfassung mit vollständigen Belegen auf Abruf, keine pauschale Feldlöschung.
Kein Aufschub hinter den vollständigen Gebäudekatalogausbau.

## Offene Folgearbeiten nach Phase E

Phase E ist mit 0.35.1 im vereinbarten begrenzten Umfang abgeschlossen.
Die folgenden Punkte sind eigenständige Folgearbeiten, keine offenen
Abschlussbedingungen von E und keine pauschale Baufreigabe.

| ID | Offener Punkt | Erforderlicher Nachweis / Grenze |
| --- | --- | --- |
| F01 | Vollständiger Weg- und Bauphasenschutz | Bestehende Baustellenzugänge unter geplanten Änderungen und Zugang neuer Baustellen vorab belastbar prüfen; positive und negative Kontrollen. Bis dahin unabhängige offene Baustellen ausschließen und `unknown` nicht als sicher behandeln. |
| F02 | Umweg als Schutz für eine spätere Bestandsänderung | Neuen Umweg zuerst real fertigstellen und seine Nutzbarkeit prüfen, dann die davon abhängige Änderung einschließlich betroffener Bestandszugänge testen. Der bestandene Anschluss um einen Baum ersetzt diesen Nachweis nicht. |
| F03 | Freie Höhenplanung und größere Bauprojekte | Bedarfsgerecht über feste Treppen-/Plattformfolgen, 8x8-Suche und vier neue Bodenwege hinausgehen; Abhängigkeiten, Bauphasen und Bestandszugänge für den erweiterten Umfang prüfen. Keine automatische Erhöhung der aktuellen Limits. |
| F04 | Besondere Gebäude- und Eingangsfälle | Direkt-fertig-Distriktzentrum mit eigenem Distrikt-Lebenszyklus gezielt live prüfen. Mehrere echte Gebäude-Eingänge erst bei belegter Spiel-API unterstützen; Bauzugangszellen sind keine Türen. |
| F05 | Breitere Kompatibilitäts- und Katalogabdeckung | Weitere relevante Geometrien, Fraktionen, Karten und Spielversionen mit repräsentativen Fällen prüfen. Generische Unterstützung ist keine pauschale Live-Abnahme aller Kataloggebäude; keine vollständige Vorlagen-/Drehungsserie ohne konkreten Bedarf. |

Weitere bereits erfasste Projektaufgaben stehen unter „Technische Lücken“;
Versorgung und Betrieb bleiben eine separate Folgestufe. Priorisierung des
nächsten Arbeitspakets erfolgt eigenständig, nicht als Fortsetzung einer offenen
Phase E. Abschlussumfang und Nachweise: [Missionsplan](missionsplan.md#abschluss-phase-e).

## Abgeschlossene Schritte und historische Priorisierung

Abgeschlossen: 0.35.1, Bauablauf und isolierte Überbudgetkontrolle live bestanden.
Vier Vorschläge nicht mit überlangen Anschlussprojekten füllen; vorhandene Wege
bevorzugt verwenden. [Abnahmekriterien](docs/road-budget-planner.md).
Historische Bank-/LargePile-Nachweise im geladenen MCP-Profil live bestätigt.
Fünf-Wege-Kandidat verworfen, spätere 4/3/2/1-Wege-Kandidaten gefunden;
keine weitere Grenzfallserie nötig. Sechs Wege für den reinen Planungstest
entfernt; Teststreifen bleibt offen, Simulation pausiert.

Generischer ebener Projektpilot 0.35.0 nach menschlichem Gate live bestanden:
Bank mit zwei neuen Wegen und großes 3x3-Freiluftlager mit einem Weg, beide
Drehung 1, mit realem Bau-/Fertigzugang, erhaltenen Bestandsanschlüssen und Replay.
Einzelne Kollision am äußeren Eckfeld des großen Grundrisses nativ abgelehnt.
Höhen-/Blockadekontrolle bestanden. Gefährliche Vorschau
erkannte 24 verlorene Verbindungen und wurde restauriert. Keine Vorlage mehr
einzeln freischalten. [Nachweis](docs/generic-building-project.md).

Offen: Direkt-fertig-Distriktzentrum mit eigenem Distrikt-Lebenszyklus nicht
live geprüft; Mehrfacheingänge spielseitig nicht öffentlich beobachtet. Keine
pauschale Katalogabnahme. Bank-/LargePile-Historie ist seit 0.35.1 im Profil.
Weitere Arbeit nach Missionsplan auf konkrete Lücken bei Weg-/Bauzugangsschutz
ausrichten, einschließlich Umwegfall; keine Drehungs-/Vorlagenserie ohne Bedarf.

### Ausgangspunkt vor 0.35.0

E-Schritt 0.34.0 live abgeschlossen, Lodge.Folktails im ebenen Bauprojekt.
Gedrehter Wohnhausablauf mit zwei neuen Wegen, Grundriss-Negativkontrolle,
realem Bau-/Fertigzugang, Bestandsanschlüssen und Replay bestanden.
Keine neue Vier-Drehungs-Serie und keine Lockerung der Schutzgrenzen.
[Nachweis](docs/lodge-project-pilot.md).
Bei nächstem ohnehin nötigen MCP-Gate Wohnhaus-Drehung 3 in TemplateEvidence
aufnehmen; aktuelles ausgeliefertes Profil beschreibt den Historienstand vor
dieser Abnahme. Keine Neuinstallation allein für diese Metadaten.

Aktiver Schritt abgeschlossen: 0.33.1 mit vier gemeinsamen
sequenziellen Vertikalprojektplätzen und aktualisiertem MCP-Nachweisprofil.
Nach menschlichem Gate zwei Projekte in derselben Sitzung fertiggebaut,
Baustellensperre und Replay bestanden. Nächsten E-Ausbau an einer konkreten
Fähigkeitslücke ausrichten; keine weiteren identischen Drehungstests nötig.
[Nachweis](docs/vertical-sequential-pilot.md).

### Ausgangspunkt vor 0.33.1

Vertikales Lagerprojekt zusätzlich in Treppendrehung 0 / Lagerdrehung 2 live
bestanden auf unveränderter 0.33.0. Sieben fertige Objekte, Bau-/Fertigzugang,
Bestandsanschlüsse und Replay geprüft. Drehung 2 / Lagerdrehung 0 ebenfalls
mit sieben fertigen Objekten, Bau-/Fertigzugang, Bestandsanschlüssen und Replay
live bestanden. Drehung 1 / Lagerdrehung 3 ebenfalls bestanden; damit alle vier
Treppendrehungen des festen Lagerprojekts begrenzt belegt. Derzeitiges
MCP-Profil enthält weiter nur historischen Nachweis 3. Profilaktualisierung
als MCP-only-Änderung mit eigenem Gate bündeln, keine Modneuinstallation nur
für weitere Tests unveränderten Codes. Pro Sitzung nur ein Vertikalprojekt;
weiterer Start braucht eine frische Sitzung. Nächster sinnvoller Code-Schritt:
begrenzte sequenzielle Vertikalprojekte bei erhaltenem Replay-/Sitzungsschutz und
frischen Zugangsprüfungen, dazu aktualisiertes Nachweisprofil. Kein paralleler
Bau oder Lockerung der Baustellensperre. [Nachweis](docs/vertical-warehouse-pilot.md).

Strukturierte Projektumfangsabfrage nach menschlichem Gate live bestanden:
fünf Modi, Grenzen und historische Nachweise; keine Modversionsänderung oder
allgemeine Baufreigabe. [Nachweis](docs/building-capabilities.md).

Nutzerpriorität: B minimal vor E. 0.32.1 nach menschlichem Gate live bestanden:
neues Projekt/Einzelplatzierung bei unabhängiger Baustelle ohne neue Objekte
abgelehnt; nach deren Fertigstellung eigener Plattformablauf vollständig gebaut.
B im freigegebenen Ausschlussumfang geschlossen, allgemeiner Vorschau-Schutz
weiter ungelöst. Erster E-Schritt auf unveränderter 0.32.1 live bestanden:
Rotation 0/2 des kleinen ebenen Lagers, Baustellen- und Fertigzugang sowie
gemeinsamer Bestandsweg geprüft. [Nachweis](docs/building-rotation-pilot.md).
0.33.0 nach menschlichem Gate live bestanden: mittleres ebenes Folktails-Lager
mit Rotation 3 und sechs Grundrisszellen, Bau-/Fertigzugang und Bestandsanschlüsse.
[Nachweis](docs/medium-warehouse-pilot.md). Ergänzung auf unveränderter 0.33.0:
Rotation 1 mit drei tatsächlich neuen Bodenwegen, fertigem Lager, Baustellen-/
Fertigzugang und erhaltenen Vergleichsanschlüssen live bestanden. Drehungen 0/2
ebenfalls fertig und erreichbar mit erhaltenen Bestandsanschlüssen; alle vier
ebenen Drehungen dieser Vorlage begrenzt live belegt. Nächster Vorschlag:
Projektvorlagen, Modi, Grenzen und Nachweisniveau strukturiert im MCP ausweisen;
kein freier Vorlagen-/Höhenausbau.

Etappe D / 0.32.0 nach menschlichem Gate live bestanden: festes Lagerprojekt über
neue Treppe, drei Plattformen und zwei obere Wege, alle sieben Objekte fertig.
Tatsächlicher Baustellen-/fertiger Lagerzugang, beide oberen Wege, ausgewählte
Bestandsanschlüsse und gleiche Aktions-ID ohne neue Objekte geprüft.
Nächster Umfang E nur begrenzt: weitere explizite Eigenschaften statt freier
Höhenplanung. B-Schutzabnahme bleibt offen; D ist keine allgemeine Baufreigabe.

Kombinierter B/C-Pilot nach Skript-Gate in 0.31.4 live abgeschlossen: drei
sequenzielle Aufträge und separat lesbare tatsächliche Baustellenzugangszellen.
C mit zwei neuen Wegen und fertigen Lagern bestanden. B-Untersuchung erstmals
mit echter Builder-Positiv-/Negativbaseline; Vorschau-Schutzabnahme weiter offen.
Keine weitere Suchserie: aktuelle Reichweitendiagnose nicht zur Baufreigabe verwenden.

1. Etappe A bestanden: gemeinsamer Bericht, freie/gesperrte Vorschau und
   Vorschau-Eingang separat von tatsächlichen Zugängen live geprüft.
2. Etappe B: Untersuchung abgeschlossen mit technischer Grenze; Schutzabnahme offen.
   Baustellen-Erreichbarkeit unter geplanten Änderungen prüfen.
   0.31.0 live: Baseline/freie Kontrolle bestanden; negative Baustellenkontrolle
   nicht belegt. Begrenzte Detaildiagnose erneut freigegeben und in 0.31.1
   live geprüft. Acht Einzelbefunde und Rücknahme passen; Verbindung vom
   Zentralenursprung bereits in der erreichbaren Baseline überall false. Nächster
   Schritt: Start-/Zielgültigkeit und positiven Verbindungsbezug klären.
   0.31.2 installiert: Zentraleingang und alle acht Ziele auf tatsächlichem
   NavMesh, Start auch auf Bezirksweg. Dennoch alle Verbindungen false bei
   realem Builderzugang true. Positive Kontrolle gescheitert, Pilot gestoppt.
   Nach erneutem go öffentliche Signaturen geprüft: AreConnected vermutlich
   direkte Kante, echte Accessible-Pfadsuche an fertigem Lager positiv.
   Nächster Vorschlag: Nachbar-/Fernkontrolle und tatsächlichen Road-Spill mit
   Builderzugang vergleichen, danach erst gezielte Preview-Engpasskontrolle.
   Keine explizite öffentliche Preview-Pfadsuche gefunden, keine neue Suchserie
   0.31.2 wurde nicht separat committed.
   0.31.3 nach Skript-Gate live belegt: drei Nachbarkanten/echte Straßenpfade,
   vier erreichbare Spill-Ziele, Fern-Kantenwerte weiterhin false. Einzige
   Sperrvorschau ungültig, sieben Bezirksverluste, kein Baustellen-Reichweitenverlust.
   Diagnose bestanden, negative Baustellenkontrolle offen. Nächster Schritt:
   gültige Blockadekontrolle; Ist-Daten ausdrücklich kein Preview-Nachweis.
   Eine gültige Einzelpunkt-Lagervorschau belegt nur einen von vier Zugängen,
   ohne Verlust; kein Negativfall. Weitere Suche in dieser Fixture gestoppt.
   Nach erneutem go isolierten Einzelzugang mit echter Negativbaseline prüfen.
   Erhöhte Fixture tatsächlich getestet: Treppe entfernt, oberer Zugang getrennt,
   vier niedrigere Geländezugänge bleiben erreichbar. Kein Builder-Negativfall.
   Pilot gestoppt; nach erneutem go Isolation gegen sämtliche native Accesses
   und höchstes Nachbargelände herstellen, nicht nur gegen Treppenanschluss.
   Einzelner weiterer Wegabriss trennt Bezirkswegast nachweislich, nicht die vier
   Builder-Spill-Zugänge. Pilot gestoppt. Vor erneuter Fortsetzung konkreten Ansatz
   wählen: separate minimale Fixture oder Herkunftsdiagnose des Spill-Zugangs.
   Keine vollständige Bauzustandssimulation; allgemeine Baufreigabe bleibt gesperrt.
3. Aktuell Etappe C: kleiner Lagerpilot mit bestehendem Weganschluss bis zur
   tatsächlichen Fertigstellung live bestanden. Baustellenzugang und fertiger
   Straßenanschluss positiv; Nachbarlager weiterhin erreichbar, Distanz 10→11.
   Ergänzung 0.31.4: zwei neue fertige Anschlusswege, drei fertige Lager mit freien
   Eingängen/positiven Straßenverbindungen; Bestandskontrolle diesmal Distanz 11→11.
   C im begrenzten Lagerumfang bestanden, kein pauschaler Bestandsschutz.
   [Nachweis und Grenzen](docs/building-completion-pilot.md).
4. Etappe D: Gebäude mit vertikalem Anschluss; danach E: Breitenausbau.
   Abnahmekriterien: [Etappenplan](missionsplan.md).

## Technische Lücken

- Vollständiger Wegschutz für Bauvorhaben.
- Diagnose der tatsächlichen Ursachen von Produktionsstillstand.
- Belastbarkeit auf unterschiedlichen Fraktionen, Karten und Spielversionen.
- Persistenz oder Wiederaufnahme von Bauprojekten nach Sessionwechseln.

## Zurückgestellt

- Vier-Wege-Entwurf (vorgesehene 0.30.1): implementiert, nicht gebaut/live geprüft.
  Lokal wiederherstellbar als benannter Git-Stash
  `deferred-four-path-pilot-before-build-safety-stages`. Nicht Teil von Etappe A;
  erst nach C/D Nutzen prüfen und Konflikte vor Wiederaufnahme gezielt abgleichen.
- Größere Bauprojekte, freie 3-D-Suche und weitere Versorgungsdiagnosen erst nach
  belastbarem Wegschutz und Gebäudezugang.

Historische Baupläne, Kolonieziele und Spielstände sind bewusst kein Backlog mehr.

Aktuelle UI-Objektauswahl in 0.30.0 erledigt: Gebäude, Auswahlwechsel und keine
Auswahl live geprüft; Grenzen und Nachweis stehen in PROJECT_STATE.md.

## Baufortsetzung und Waldzugang: Livebefunde 2026-10-11

- P1: Gesamtstatus waiting mit Teilstatus unconfirmed eindeutig von terminalem
  Gesamtstatus unconfirmed unterscheiden. Aktuelle pauschale Limitations-Zeile
  führte zur Blockierung regulärer Fertigbestätigung durch Aktionsprüfung.
  Lesende reconcile-Funktion erwägen; abgeschlossene Einzelteile nie neu platzieren.
- P1: Waldstandort muss vom Eingang aus erreichbare Ertragsbäume nachweisen.
  Eingang auf Brücke kann zugänglich sein, während angrenzender Wald außerhalb
  des nativen Arbeitsbereichs bleibt. Landseitigen Wegausgang gezielt prüfen.
- P2: Veraltete Navigationsdistanz nach Fertigstellung sichtbar machen: Terrasse
  zunächst weiter 72, nach einer Spielstunde 24. Freshness/ausstehendes Update nennen.
- P2: Planablehnung präzisieren. Gebäudepause zwischen Plan und Start veränderte
  Bestandsanschlusszahl und PlanKey; state_conflict allein erklärt das nicht.
  Änderungsgrund liefern, ohne Prüfungen oder frische Validierung auszulassen.
- P1: Holzdiagnose trennt Rest-Ertrag von Lebenszustand. Tote Birken meldeten
  cutIsYielding=true und Log-Ertrag 1, wurden aber als not_observed_alive verworfen.
  Erntefähigkeit durch regulären Spielservice separat prüfen, tote Vollbäume nicht
  mit abgeernteten Stümpfen vermischen; keine pauschale Freigabe allein nach Ertrag.
- P2: Pflanznachweise bündeln. inspect_areas liefert Markierungen, Pflanzenzustand
  bleibt null; tatsächlicher Nachweis von 54 Eichen erforderte zusätzliche
  Objektseiten. Regionalen Bericht mit markiert/gepflanzt/lebend/reif/Ertrag anbieten.
- P2: Kompakte, direkt sichtbare Fortsetzungsbelege ausgeben: Gesamtstatus,
  currentIndex, currentEntityId, found, finished, nächster Auftrag. In intern
  gebündelten Aufrufen erhobene Fertigbelege wurden von der Aktionsprüfung teils
  übersehen oder mit der nächsten pending-ID verwechselt. Keine Prüfungen lockern.
- Erntenachweis zu toten Vollbäumen: Alle sechs zusätzlich regulär markierten Birken wurden
  anschließend mit cutYieldRemoved=true und Ertrag 0 gelesen. Der bisherige
  Ausschluss not_observed_alive verwirft damit praktisch erntbare Vollbäume.

## Warnungsbereinigung: Bestandszugang und Betrieb (2026-10-11)

- P1: Regression nach Treppenersatz eines bestehenden Weges. Native gemeinsame
  Vorschau meldete keinen verlorenen Anschluss, später waren Haus und Wassertank
  mit Eingang auf derselben Treppenzelle unverbunden. Abkürzung entfernt, flacher
  Weg wiederhergestellt: beide Eingänge frei, Distanz 15. Terrasse bleibt verbunden,
  wieder Distanz 72. Fertigabnahme muss betroffene Bestandszugänge nach echten
  Navigationsticks erneut prüfen, nicht nur Vorschau und neue Gebäude.
- P1: Rezeptauswahl fehlt als Schreibfunktion. inspect_building_operation erkennt
  hasRecipe=false, aber Grill/Bäckerei können per MCP keine Rezeptwahl erhalten.
  Reguläre auswählbare Rezepte lesen, erwartetes altes Rezept prüfen, neues setzen
  und Wirkung rücklesen; benötigte Zutaten/Brennstoff getrennt anzeigen.

- P1: guard_changed bei Großprojekten aufschlüsseln. Nach fertig beobachtetem
  Pegel stoppte Zweierprojekt vor Kraftwelle, ohne offene Baustellen/HUD-Alerts.
  Teilprüfung, betroffene Objekt-/Zugangskoordinaten und Baseline-/Istwerte liefern;
  lesenden Diagnoseaufruf auch für terminale Projekte ermöglichen. Historischer
  Wegvergleich erlaubt keine eindeutige Ursachen- oder Verursacherzuordnung.
  Kontrollierten Neuplanungsabschluss nach bestätigten bestehenden Aufträgen
  entwickeln, damit ein diagnostizierter Stopp keinen Spielstand-Neuladevorgang
  erzwingt. Keine automatische Wiederholung unbestätigter Aufträge.

- P1: Regionalsuche/gedrehter Grundriss erneut inkonsistent: MediumTank-Kandidat
  (42,13,6), Drehung 2, ohne neue Wege vorgeschlagen. Native 3D-Prüfung lehnt ab;
  tatsächlicher Grundriss enthält (41/42,12,6) über niedrigerem Gelände. Herkunft
  der Kandidatenkoordinate und Drehtransformation vereinheitlichen, volle Auflage
  prüfen. Ungeprüfte Suchtreffer nicht als belastbare Bauplätze darstellen.

- P2: Lange Wachstumsläufe effizient begleiten: optionales natives Ereignisziel
  für erste erntereife markierte Bäume in einem Gebiet und Abschlussbenachrichtigung
  statt kurzer MCP-Statusabfragen. Feste vom Nutzer gewünschte Laufdauer beibehalten,
  wenn kein Ereignisziel beauftragt ist. Aktueller Bestandsstopp beweist weder
  Baumreife noch Ernteursache; Zeitprognosen nur mit ausgewiesenen Annahmen zu
  Wachstum/Wasserstress und ohne garantierten Ertrag.

## Spieltest 2026-10-11: Rohstoffeinzugsbereich und Kraftzugang

- Rohstoffsuche an Gebäude-Reichweite binden statt an ein einzelnes 8x8-Fenster;
  lebende, nachwachsende Bäume und abgeerntete Reste getrennt liefern. Harzsammler:
  erste Suche fand vier, vollständige Reichweite 18 lebende Kiefern. Alle geschützt.
- Fällschutz nach Baumart über vollständige Reichweite gesammelt anbieten, mit
  Vorschau der betroffenen Zellen und Rücklesung; Eichen unangetastet lassen.
- Automatische Zugangsplanung meldet bei gemischtem Werkstatt-/Kraftplan nur
  no_access_candidate nach erstem Gebäude; fehlgeschlagenes Teil und Arbeitszelle
  konkret nennen. Bestehende Wege auch jenseits des Sechs-Zellen-Randes einbeziehen.
- Regionalsuche sollte Kartenrand diagnostizieren statt generischem invalid_argument.

- Produktionsläufe auf Zielbestand beliebiger Güter erweitern: aktuell nur Log
  als vorzeitiges Bestandsziel. TreatedPlank plus Forschungspunkte müssen durch
  Zeitläufe und separate Abfragen beobachtet werden. Zielmenge, maximale Spielzeit,
  Arbeits-/Liefer-/Windblockaden sowie bestätigte Pause gebündelt zurückgeben.
- Kraftvorschau: rotationMatches=false an einer Welle zur Holzwerkstatt war
  kein tatsächlicher Anschlussfehler; fertig befanden sich beide im selben Netz.
  Geometriehinweis präzisieren und gegenüber realem Netzbeleg klar kennzeichnen.
- Wetterstationspilot: gemeinsame Bauvorschau gültig, Baustelle dennoch nicht
  erreichbar. Eintrittslose Ziele brauchen vor Auftrag konkrete Bauarbeitszellen
  und deren Verbindung; ein freier Grundriss genügt nicht. Fehlende Baumöglichkeiten
  einer Reparatur früh gegen einen einfacheren Standortwechsel abwägen.
- Ungültige UUID nicht als möglicherweise ausgeführte Aktion melden: lokale
  Argumentfehler klar vor dem Transport abweisen und als nicht ausgeführt kennzeichnen.

## Nächste MCP-Etappen nach Harzpilot — Planung, noch keine Umsetzung

Nutzerentscheidung 2026-10-11: nach Produktionsnachweis und ersten Folgegebäuden
stoppen; weiterer Spielausbau pausiert. Reihenfolge:

1. **Bauzugang vor Auftrag und automatische Reparaturplanung.** Für Gebäude ohne
   Eingang konkrete Baustellenzugänge einschließlich Terrain, Kraftleitungen und
   Höhenübergängen bewerten. Unvollständige Vorschau als unbekannt ausweisen;
   kein positives Ergebnis aus bloßer Platzierbarkeit. Reparaturvorschläge als
   Gesamtplan erzeugen; bei teurem Umweg nahe Alternativstandorte vergleichen.
   Gate-Nachweis: direkt erreichbare Station, geometrisch gültige aber abgesperrte
   Station und eine durch Zusatzwege tatsächlich erschlossene Station.
2. **Produktionslauf bis Zielbestand.** Mehrere frei verfügbare Güter und optional
   Forschung als Ziele; keine Addition von Produktions-/Transportbeständen.
   Begrenzte Spiel-/Echtzeit, automatische Pause und knappe Blockadediagnose für
   Zutaten, Personal, Kapazität, Lieferung und Wind. Nachweis: TreatedPlank-Ziel
   erreicht, fehlende Zutat benannt, Pause am Ziel; kein Agentenpoll pro Bauteil.
3. **Große und besondere Harzgebäude.** Die sechs nicht unterstützten Typen zuerst
   nach Ursache trennen: DirtExcavator, DomedGarden, EarthRecultivator,
   FountainOfJoy, HallOfAbundance und Mine. Exakte native belegte Zellen statt
   pauschalem 64-Zellen-Ausschluss prüfen; Sonderform und besondere Standortbindung
   separat behandeln. Bestehende 32-Teil-/acht-Höhen-Grenzen bleiben, bis gezielt
   anders beauftragt. Pilot erst ein großer regulärer Typ, dann ein Sonderfall.

Flankierend: kompakte Rohstoffübersicht über die vollständige Arbeitsreichweite,
artenbezogener Fällschutz und bessere Kraftport-/Argumentfehlerdiagnosen aus
obigen Befunden. Vor Implementierung konkreten Umfang auswählen; weiterhin ein
Release-Build und menschliches Testgate. Keine neue Modfassung in dieser Etappe.
