# Backlog

## Aktuelle Arbeit

**A12/N05 begrenzt abgenommen; N01/N02/N03/N04 ebenfalls abgeschlossen.**
Gate und Livevergleich bestanden: laufende/terminale Zählung korrekt,
gespeicherter Projektstand von frischer Bauprüfung getrennt, im Spiel sichtbar.
[Ursache, Nachweise und Bewertung](docs/project-progress-display-pilot.md).
Journal freigegeben, Spiel pausiert; weitere Pakete bleiben zurückgestellt.
A03/A04/A05/A07/A08/A09/A10-Piloten abgenommen; weitere Erweiterungen zurückgestellt.
A01/A02/A06/A11 abgeschlossen. Weitere A12-Pakete zurückgestellt. [Projektstand](PROJECT_STATE.md).

## Projektprüfung 2026-10-18 — abarbeitbare Befunde

Prüfbasis und vollständiger Audit: [Archivstand](BACKLOG_HISTORY.md#archivstand-2026-10-18-vor-a01-und-a02).
Ergebnis: gezieltes Refactoring von Versionsprofilen, Werkzeugdispatch und
Prüfketten sinnvoll; kein belegter Bedarf für einen Komplettumbau.
Der ursprüngliche Audit war statisch. A06 ist inzwischen **abgeschlossen**;
A03 ist **teilweise abgeschlossen**, A04 **teilweise abgeschlossen**; A05 ist **teilweise abgeschlossen**, A07 ist **teilweise abgeschlossen**, A08 ist **teilweise abgeschlossen**, A09 ist **teilweise abgeschlossen**, A10 ist **teilweise abgeschlossen**, A11 ist **abgeschlossen**, A12 ist **teilweise abgeschlossen (N01/N02/N03/N04/N05)**.
P2 = vor der nächsten betroffenen Erweiterung, P3 = nach Bedarf;
S/M/L sind relative Aufwandsschätzungen. Codeänderungen benötigen weiterhin
das [menschliche Gate und den gezielten Livetest](DEVELOPMENT_WORKFLOW.md).

### A03 — P2 / M: Versions- und Fähigkeitsprofile zentralisieren

- [x] **Pilot abgenommen (2026-10-18):** `BridgeVersionProfiles` enthält exakt
  die bisherigen 102 Envelope-Versionen: 58 mit Legacy-, 44 mit erweitertem
  Validierungsbudget. Nur Envelope und `ValidationBudgetContract` migriert.
  Unbekannte Versionen bleiben im Envelope gesperrt; der eigenständig aufgerufene
  Budgethelfer behält seinen bisherigen Legacy-Fallback. 121 neue Matrix-/Negativfälle
  im menschlichen Gate bestanden; gezielter Reader-/Budget-/Lauf-/Ablehnungspilot
  live bestanden. [Belege und Bewertung](docs/compatibility/timberborn.md#a03-pilot--zentrale-wire-vertragsprofile).
  Kleine Zentralisierung sinnvoll; historische Testmatrix als wesentlicher Zusatzaufwand.
  Gesamt-A03 bleibt offen; weitere Familien auf Nutzerwunsch vorerst zurückgestellt.
- [ ] **Restarbeit; Befund zum Auditzeitpunkt:** `0.74.0` stand in 41 Dateien unter src/mod/tests.
  Besonders [NativeClient.cs](src/Timberborn.Backend.Native/NativeClient.cs)
  wiederholte lange Versionslisten für Envelope, Straßen-/Baunachweise und
  Laufzeitfelder; [ValidationBudgetContract.cs](src/Timberborn.Backend.Native/ValidationBudgetContract.cs)
  pflegte eine weitere Liste. Envelope/Budget sind jetzt zentralisiert,
  Straßen-/Baunachweise und Laufzeitfelder bleiben Restarbeit. Ein Versionsschritt kann dadurch einzelne
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

- [x] **Pilot abgenommen (2026-10-18):** elf benannte MCP-Optionen; feste
  Metadaten nur für die vier Simulationslauf-Werkzeuge. Katalogindex einmalig
  vor Dispatch, HTTP-Freigabe unabhängig und unverändert. 28 neue Fälle im Gate
  bestanden; beide Startwege, Status, Cancel, Endpause und Sessionkontrolle live
  bestanden. [Belege und Bewertung](docs/tool-metadata-pilot.md). Nutzen/Aufwand
  positiv bewertet; Gesamt-A04 offen, weitere Familien vorerst zurückgestellt.
- [ ] **Restarbeit, vorerst zurückgestellt:** weitere Werkzeugfamilien haben
  weiterhin getrennte Registrierung und Klassifikation. `NativeTools` bündelt
  Katalog, Dispatch, Fehlerübersetzung und Logging; die elf Positions-Bools
  bestehen nur noch als Kompatibilitätsadapter. Der im Audit bemängelte
  Katalog-Neuaufbau im Aufrufpfad ist durch den Pilot erledigt.
- **Arbeit nach erneutem Auftrag:** weitere Familien einzeln konsolidieren und
  verbleibende Bool-Aufrufer bei Bedarf umstellen. Die native HTTP-Freigabe in
  [BridgeHttpServer.MethodAllowed](src/Timberborn.Bridge.Core/BridgeHttpServer.cs)
  bleibt eine eigenständige Schutzschicht; ihre Übereinstimmung weiter prüfen.
- **Abnahme:** Katalog, Handler und Opt-ins stimmen für ein-/ausgeschaltete
  Aktionsgruppen überein; deaktivierte Mutation erreicht kein HTTP. Status/Cancel
  während belegter Ausführung und Räumgruppenbesitz bleiben korrekt. Vorher/Nachher
  gleiche Toolnamen, Schemas und Fehlerverträge. Weitere A03-Migration bleibt zurückgestellt.

### A05 — P2 / S–M: dicht geschriebene Fachprüfungen lesbar machen

- [x] **Pilot abgenommen (2026-10-18):** nur `ProductionRunContract.Validate`:
  vier private Prüfabschnitte, unveränderte Bedingungen/Reihenfolge/Fehlertexte.
  23 Charakterisierungsfälle und Gate bestanden; Zeitstopp mit unerreichtem
  Güterziel, Diagnosebelege und Pause live bestätigt.
  [Belege und Bewertung](docs/production-contract-readability-pilot.md).
  Nutzen/Aufwand positiv; Gesamt-A05 offen, weitere Fachprüfungen zurückgestellt.
- [ ] **Befund zum Auditzeitpunkt:** 271 C#-Textzeilen waren länger als 300 Zeichen
  (inklusive Beschreibungen und Tests, daher keine reine Komplexitätsmetrik).
  NativeTools umfasste rund 34.000 Zeichen in nur 299 Zeilen; auch native
  Vertragsprüfungen und Mod-Dispatch bündeln zahlreiche Bedingungen pro Zeile.
- **Arbeit:** bei A03/A04 betroffene Methoden normal formatieren; Session-,
  Identitäts-, Geometrie- und Ergebnisprüfungen in benannte kleine Prüfungen
  zerlegen. Kein repoweiter Formatierungsdiff und keine neue Abstraktionsschicht
  allein wegen Dateigröße. Weitere Planer erst bei konkreter Änderung anfassen.
- **Abnahme:** vorhandene Positiv-/Negativverträge bleiben gleich; Diff trennt
  mechanische Umformatierung und fachliche Änderung. Charakterisierung dort
  ergänzen, wo eine verschobene Bedingung sonst ungeschützt wäre.

### A07 — P2 / M: ausgelieferten MCP-Stand eindeutig erkennbar machen

- [x] **Pilot abgenommen (2026-10-18):** `data.mcp` benennt geladenes
  Modulprofil und wirksamen Backend-Katalog; Offline-Gatebericht als Vergleich.
  11 Unit-Fälle und ein Integrationstest bestanden. Beide Kennungen live exakt
  gleich Gate und stabil in compact/full sowie nach lesender Abfrage.
  [Belege/Bewertung](docs/mcp-runtime-identity-pilot.md). Nutzen/Aufwand positiv.
  Clientcache bleibt `not_observed`, Supervisor unverändert; Gesamt-A07 offen,
  weitere Erweiterungen vorerst zurückgestellt.
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

- [x] **Pilot abgenommen (2026-10-18):** ausschließlich `RoadProtection.Finish`:
  reine Abschlussentscheidung in `RoadProtectionPolicy.EvaluatePreview`, 13 neue
  synthetische Fälle bestanden. Native Reads/Cleanup bleiben im Adapter.
  Gate und Livepilot bestanden: freie Vorschau, fünf verlorene Verbindungen
  bei Wohnhausvorschau, anschließend freie Kontrolle wieder verlustfrei.
  Bank-Negativkontrolle war ungeeignet; gezielt diagnostiziert, kein Codefehler
  belegt. [Nachweise und Bewertung](docs/mod-adapter-evidence-pilot.md).
  Nutzen/Aufwand positiv. Weitere Adapter sowie Sessionwechsel/Fertigzugang
  bleiben Restarbeit und auf Nutzerwunsch vorerst zurückgestellt.
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

- [x] **Eingangspilot abgenommen (2026-10-18):** 1 MiB je Clientzeile, lokale
  Parse-/Formfehler, vollständiges Verwerfen übergroßer Frames. Sieben neue Tests
  bestanden; Gate und Stop/Status/Start nach vollständigem Codex-Neustart bestanden.
  Gleiche Spielsitzung nach Wiederstart; Nutzen/Aufwand positiv.
  [Umfang und Grenzen](docs/supervisor-input-pilot.md).
  Inflight-/Threadgrenzen, reservierte Lifecycle-/Cancel-Kapazität und Timeouts
  sind ausdrücklich nicht Teil dieses Piloten.

- [ ] **Ursprünglicher Befund; Lastwirkung ungeprüft:**
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

- [x] **Pilot abgenommen (2026-10-18):** manuelles Großprojektjournal mit
  `large_project_journal_limit` und `manualJournal` (128/count/remaining).
  Replay vor Kapazitätsprüfung, Freigabe ohne Historienlöschung. 18 neue Fälle
  im Gate bestanden; Full-/Compact-Plan live mit stabiler Kapazität und planKey.
  Nutzen/Aufwand positiv. [Umfang](docs/manual-project-journal-pilot.md).
  Andere Journale und Aufbewahrungsänderungen nicht Teil des Piloten.

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

- [x] **Pilot abgenommen:** fehlende/leere Backendwahl bricht vor
  Hoststart mit Exitcode 2 und stderr-Meldung ab. Explizite Modi bleiben erhalten.
  Fünf Prozessregressionen bestanden. Frühe stderr-Meldung nach erstem Gate auf
  ASCII korrigiert; anschließendes Gate und nativer Supervisor-Livefall bestanden.
  Kleiner Eingriff mit klarem Nutzen; keine weiteren Erweiterungen beauftragt.
  [Entscheidung und Nachweise](docs/backend-selection-pilot.md).

- [x] **Ursprünglicher Befund behoben:** [Program.cs](src/Timberborn.McpServer/Program.cs)
  wählte ohne `TIMBERBORN_BACKEND` automatisch `more-http-api`, obwohl die native
  Bridge die aktuelle Architektur ist. Der reguläre Supervisor setzt `native`
  korrekt; deshalb kein Fehler im belegten Standard-Supervisorpfad.
- **Arbeit:** Direktstart explizit als Legacy kennzeichnen oder bei fehlender
  Backendwahl mit verständlicher Konfigurationsmeldung abbrechen. Auswahl erst
  nach Kompatibilitätsentscheidung ändern; Legacy/Fake nicht beiläufig löschen.
- **Abnahme:** dokumentierter Direktstart, Supervisorstart und explizite
  Legacy-/Fake-Auswahl liefern den beabsichtigten Backendmodus; kein stiller
  Fallback auf die Fremdmod nach einem nativen Verbindungsfehler.

### A12 — P2 / M–L: bekannte Funktions- und Livegrenzen gezielt schließen

- [x] **N01-Pilot abgenommen:** native Kartengrenzen für `inspect_map_region`
  mit Achse und gültigem Zellbereich; 17 Testfälle sowie Gate bestanden.
  Randzelle und vier gezielte Überschreitungen live bestätigt.
  [Vertrag, Nachweise und Bewertung](docs/map-region-bounds-pilot.md).
  Andere Karten-/Bauwerkzeuge bleiben außerhalb des N01-Umfangs.

- [x] **N02-Pilot live abgenommen:** automatische untere Zielwegplanung an
  Cw270-Bestandstreppe, gültige seitliche Geometrie ohne Verbindung, reale
  Baustelle erreichbar und anschließend 2/2 Teile fertig/zugänglich.
  Keine Codeänderung; bestehender N01-Gatestand verwendet. Journal freigegeben.
  [Belege und Bewertung](docs/lower-stair-landing-pilot.md).

- [x] **N03-Injektionspilot abgenommen:** ein gebundener identity_mismatch-Fall;
  Diagnose sperrt beide Freigaben, Close und Reparaturplanung tatsächlich
  abgelehnt. Objekt und Journal exakt unverändert; nach Deaktivierung echte
  Freigaben wiederhergestellt und regulär abgeschlossen. Gate und Live bestanden.
  [Belege und Bewertung](docs/unknown-object-identity-pilot.md).
  Weitere Fehlerfälle und A12-Pakete bleiben offen und zurückgestellt.

- [x] **N04-Livepilot begrenzt abgeschlossen:** identische automatische/explizite
  Gesamtpläne liefern denselben Schlüssel; Originalstart angenommen. Geänderte
  Wegrotation mit altem Schlüssel vor Auftrag abgelehnt, Journalzähler unverändert.
  Historischer Fehler nicht reproduziert, Ursache nicht rückwirkend belegt.
  2/2 Teile fertig/zugänglich und Journal freigegeben; keine Codeänderung.
  [Belege und Bewertung](docs/access-plan-key-pilot.md).

- [x] **N05-Pilot abgenommen:** fehlerhafte Nullzählung aus Chargen-parts
  korrigiert. Acht Regressionstestfälle und Gate bestanden; laufende/terminale
  Ergebniszeilen mit strukturiertem Stand und Spielbild abgeglichen. Frische
  Diagnose bleibt getrennt. [Nachweise](docs/project-progress-display-pilot.md).

- [ ] **Bestehende Restarbeit, keine neu nachgewiesenen Regressionen:** B06
  (Reload), B03 (allgemeine Kraft-/Höhenrouten), B07 (Produktionsausbauplanung),
  F01/F02 (vollständiger Bauphasenschutz/Umweg vor Bestandsänderung) und F04/F05
  (Geometrien/Fraktionen) bleiben offen. Die aktuelle Bretterdiagnose ersetzt
  keinen erfolgreichen Produktions- oder Liefernachweis.
- **Kleine Pakete:** (a) konkrete Kartengrenzen bei inspect_map_region abgeschlossen;
  (b) unterer Zielweg an Cw270-Bestandstreppe abgeschlossen;
  (c) deterministischer injizierter Identitäts-Sperrnachweis abgeschlossen;
  (d) ein positiver Bretterproduktions-/Abtransportfall.
  Jeweils vorhandenen Fachvertrag und aktuelle Implementierung zuerst abgleichen.
  B06/B03/B07 erst als abgegrenzten Entwurf bewerten, nicht als Sammelrefactoring.
- **Abnahme:** pro Paket ein Positiv- und nötiger Negativkontrollfall mit
  vorher benanntem Erfolg/Stoppkriterium. Geometrie, Bauarbeiterzugang,
  Fertigzugang, Netz und Betrieb getrennt belegen. Bei fehlerhafter Kontrolle
  stoppen; keine ungezielten Zeitrennen, Save-Manipulation oder Koloniepflege.
  Bereits akzeptierte synthetische Grenzen und abgeschlossene Cancel-Versuche
  nicht ohne konkreten Änderungsgrund erneut aufrollen.

## Aktuell offene Folgearbeiten

Die A12-Pakete bündeln konkrete nächste Nachweise. Die folgenden eigenständigen
Altbefunde bleiben sichtbar, bis sie reproduziert oder mit einem passenden
Abschlussbeleg verworfen wurden. Herkunft: [archivierter Verlauf](BACKLOG_HISTORY.md#archivstand-2026-10-18-vor-a01-und-a02).

| ID / Status | Nächster Schritt | Abschlusskriterium |
| --- | --- | --- |
| N01 — abgeschlossen, A12 | Kein weiterer Auftrag; auf inspect_map_region begrenzt. | Gate und Live bestanden: gültige Randzelle, x/y/z-Überschreitungen und zu breite Randabfrage korrekt. |
| N02 — abgeschlossen, A12/F04 | Begrenzter Cw270-Folktails-Fall; keine weiteren Varianten beauftragt. | Automatische untere Zielwegplanung, native Geometrie, Bau- und Fertigzugang positiv; seitliche Verbindung bei gültiger Geometrie negativ. |
| N03 — begrenzt abgeschlossen, A12 | [Einzelinjektionspilot abgenommen](docs/unknown-object-identity-pilot.md); weitere Fehlerfälle zurückgestellt. | Injektion klar markiert, repairEligible=false und closeEligible=false, Close/Repair tatsächlich abgelehnt; Objekt und Journal unverändert, nach Deaktivierung echte Freigaben wiederhergestellt. |
| N04 — begrenzt abgeschlossen, A12 | [Livevergleich bestanden](docs/access-plan-key-pilot.md); historischer Fehler nicht reproduziert, keine behauptete Ursachenklärung. | Gleicher Plan/Schlüssel angenommen; geänderte Rotation mit altem Schlüssel vor Auftrag abgelehnt. 2/2 Teile fertig/zugänglich, Journal freigegeben. |
| N05 — begrenzt abgeschlossen, A12/B10 | [Anzeigekorrektur abgenommen](docs/project-progress-display-pilot.md); weitere Auftragsfamilien unverändert. | Laufende 0/2 bzw. 1/2 und terminale 2/2 korrekt, Spielbild bestätigt; gespeicherte und frische Belege getrennt. |
| N06 — offen | Kollisionsdiagnose von precheck_building um verursachende Objekt-ID/-typ ergänzen. | Terrain-, Objekt- und unbekannte Belegung getrennt und gegen native Zellen belegt. |
| N07 — nur bei Wiederauftreten | `unconfirmed`/`run_not_found` bzw. Navigations-Baselinekonflikt anhand Session, Transport und Registrierung isolieren. | Ursache belegt oder Fall ausdrücklich unreproduziert; Antwortverlust nicht als Nichtausführung behandeln. |
| N08 — Vorschlag | Optionale ausdrücklich gewünschte Abschlussgeschwindigkeit statt Pflichtpause entwerfen. | Besitzer-/Abbruch-/Fehlerfälle geklärt; bestehende Pause bleibt Standard bis zur gesonderten Freigabe und Abnahme. |
| N09 — Vorschlag, B12 | Nutzen paralleler Leser gegenüber sofortiger Ablehnung messen. | Begrenzter Pilot zeigt Nutzen ohne Mutationsqueue, Replay oder blockierten Cancel; sonst zurückstellen. |

## Offene Entwicklung

Bestehende B-IDs bleiben erhalten. **Teiloffen** bedeutet: vorhandener Teilumfang
ist begrenzt abgenommen, nur der genannte Rest ist offen. Vorschläge sind keine
Implementierungsfreigabe; größere Entwürfe zuerst konkret abgrenzen.

| ID / Status | Restumfang und nächster Schritt | Abschlusskriterium des nächsten Pakets |
| --- | --- | --- |
| B01 — teiloffen | Alternative Zielreihenfolgen und breitere Standortsuche gegen den bestehenden 0.51-Planer vergleichen. | Kleiner Vergleich belegt bessere Vollständigkeit/Kosten im festen Budget; keine behauptete globale Optimalität. |
| B02 — teiloffen | Dauerhafte Reservierungen und planübergreifende Korridore entwerfen; bisher bis acht Ausschlussvolumen je Gruppenplan. | Zwei kollidierende Pläne und Freigabe/Sessionwechsel eindeutig behandelt. |
| B03 — teiloffen | Allgemeine Kraft-/Höhenrouten und alternative Versorgungslayouts begrenzen, vorhandene Ports/Netze nutzen. | Geometrie, reale Netzverbindung und Produktion separat an Positiv-/Negativroute belegt. |
| B04 — teiloffen | Mehrere/größere Räumflächen auf Basis der abgenommenen 8×8-Einzelfläche planen. | Mehrflächenpilot ohne Verlust unbestätigter Aufträge; Rohstoffstapel erhalten. |
| B05 — teiloffen | Wachstum/Aquakultur getrennt vom abgenommenen Sammelpflanzen nachweisen. | Nativer Pflanzen-/Wachstumszustand im definierten Zeitfenster belegt, Markierung allein zählt nicht. |
| B06 — offen | Projektabsichten nach Spielneuladen mit realen Objekten abgleichen; Persistenzumfang entwerfen. | Neue Session, vorhandene/fehlende/unbekannte Objekte und identischer Wiederaufruf ohne Replay geprüft. |
| B07 — teiloffen | Aus Bretterdiagnose einen begrenzten Ausbauplan für Zutaten, Lager/Transport, Personal und Kraft ableiten; gemeinsame Versorgung mit Pumpe/Sammler/Holzfäller als weiteres Teilpaket. | Vor Bau Kosten inklusive Hilfen; anschließend Förderung/Sammlung/Produktion und Lieferung getrennt belegt. Keine Materialsperre. |
| B08 — teiloffen | Weitere Produzenten und reale Wirkungszellen etwa des Bienenstocks wählen. | Produktionszyklus bzw. tatsächliches Effektziel belegt; Reichweite allein genügt nicht. |
| B09 — teiloffen | Forschungsereignisse ins Änderungsjournal aufnehmen. | Forschungswechsel, Wiederlesen und Sessionwechsel geprüft; unbekannte Verursacher bleiben unbekannt. |
| B10 — teiloffen | N05 abgeschlossen; Zeitlimit-/Stopgründe in Kurztexten präzisieren und weitere Auftragsfamilien einzeln erschließen. | Ergebniszeile stimmt mit strukturiertem Beleg überein und ist im Spiel lesbar. |
| B11 — Vorschlag | „Zeigen“-Button mit Aufnahme beim Klick, begrenzter Aufbewahrung und Abhol-/Fehlerstatus entwerfen. | Aufnahmezeitpunkt und Fehlerfall belegt; aktive Benachrichtigung bleibt eigener Umfang. |
| B12 — teiloffen | Große Antworten/Beschreibungen und redundante Reads messen; Katalogerkennung über A07, Parallelität über N09. | Messbarer Nutzen bei unverändertem Compact/Full-Belegumfang und sichtbarer Abdeckung. |
| B13 — Vorschlag | Eine optionale Bevölkerungs-/Wohlbefindensbeobachtung auswählen: Geburten/Todesfälle, Alter, Personalprognose, Familienwohnungen oder Bauregister. | Beobachtetes Intervall, Datenherkunft und Prognoseunsicherheit getrennt; kein autonomes Kolonieziel. |
| B14 — teiloffen | Wasserbau-, Ereignis- und Gefahrenrestumfang gemäß Abschnitt unten einzeln angehen. | Geometrie, beobachteter Zustand und erwartete Wirkung getrennt nachgewiesen. |
| B15 — abgeschlossen | Gesamtstruktur mit gemeinsamer Platzierung ist im 0.63-Umfang abgenommen. | Kein neuer Auftrag; [Vertrag](docs/construction-structures-0.63.md), weitere Höhen-/Schutzgrenzen unter B03/F01–F05. |

### B14 — Strategische Sicht statt isolierter Kartenausschnitte

Abgenommen sind der fortsetzbare Gesamtscan, Quellen-/Flussübersicht, geometrische
Reservoiroptionen und begrenzte Explosions-/Restwasserwarnungen. [Vertrag und Belege](docs/strategic-map-0.62.md).
Die damals fehlende native Pumpen-Ansaug-/Flooded-Diagnose ist durch 0.65,
der falsche Uferpumpen-Terrainblocker durch 0.66 begrenzt erledigt; nicht neu eröffnen.

| Teil / Status | Nächster Schritt | Abschlusskriterium |
| --- | --- | --- |
| Wasserwege — offen | Wasserbauwerkszustände, tatsächliche Strömung, Quellenklassifikation und vollständige Gefahrenabdeckung ergänzen. | Trockenüberlauf, getrennte Höhlenstockwerke und Wasser-/Faulwasserzusammenfluss gegen native Daten; Abdeckung sichtbar. |
| Ereignisse — offen | Countdown, endgültige Kernwirkungszone/Einsturz und zeitliche Pegel-/Geländeänderung prüfen; Wetter-/Quellaktivierung mit Unsicherheit. | Vor-/Nach-Ereignisvergleich; keine Ankunftszeit oder Reinheitsprognose ohne Fluss-/Mischungsbeleg. |
| Schutz/Wasserbau — offen | Kontrollierte Ableitung, Dämme/Deiche, Entwässerung/Quellensteuerung und freie Beckenformen als begrenzte reguläre Vorhaben planen. | Vorher-/Nachher-Pegel, neue Überläufe, Rückstau, Zufluss und hydraulische Schutz-/Bewässerungswirkung separat belegt. |
| Reserven/Gefahrenstopps — offen | Nach Distrikt zugängliche Trinkwasser-/Nahrungsreserve statt globaler Vorräte bestimmen; Verbrauchsfenster und Stopkriterium entwerfen. | Überflutete/unzugängliche/nicht trinkbare Bestände getrennt; Reichweite mit Messfenster und Unsicherheit. |
| Evakuierung — Vorschlag | Höheren Ausweichstandort, sichere Route, Versorgung, Unterkunft und Baupersonal gemeinsam bewerten; native Migration erschließen. | Tatsächliche Ankunft und Versorgung belegt; Bauauftrag allein ist keine Rettung. |
| Skalierung — offen | Große Karten/Vegetation, vollständige Routen und globale Änderungsrevision gezielt bewerten. | Keine stille Abschneidung; konkrete Limits und veraltete Bereiche bleiben erkennbar. |

## Offene Folgearbeiten nach Phase E

| ID / Status | Restumfang und nächster Schritt | Abschlusskriterium |
| --- | --- | --- |
| F01 — teiloffen | Vollständigen Bauphasenschutz einschließlich Bestandsbaustellen und freier Rückwege entwerfen. | Bestands-, Bauarbeiter- und Fertigzugang während jeder relevanten Änderung getrennt belegt. |
| F02 — offen | Ersatzumweg fertigstellen, erst danach abhängige Bestandsänderung ausführen. | Tatsächlich nutzbarer Umweg vor Eingriff und erhaltene Verbindung danach. |
| F03 — teiloffen | Allgemeine räumliche Suche über B01–B03; explizite 32-Teil-/acht-Höhen-Pläne und autonome Chargen bestehen. | Abschluss des konkret gewählten B-Pakets, keine Doppelimplementierung. |
| F04 — teiloffen | N02 begrenzt abgeschlossen; weitere Standort-/Distrikt-/Geometriefälle erst nach Auswahl, mehrere Türen nur bei belegter API. | Je ein gültiger und ungültiger Fall mit getrennten Zugangsbelegen. |
| F05 — offen | Eine weitere Fraktion/Karte/Spielversion für repräsentativen Pilot wählen. | Version/Fraktion und konkreter Funktionsumfang belegt; Folktails-Abnahme nicht übertragen. |

## Zurückgestellte Experimente

Hermes wurde am 2026-10-17 auf Nutzerwunsch beendet; Proxy, Anleitung und
Skillvorlage sind entfernt. Keine Wiederaufnahme oder Versionspflege vorgesehen.
Lokale Startskript-/Budgetexperimente sind kein Auftrag für den nächsten Release.
Historischer Vier-Wege-Stash bleibt zurückgestellt; nur bei konkretem Bedarf
gegen aktuellen Code prüfen, nicht ungeprüft wiedereinspielen.

## Zurückgestellte Nachweise

- Große Gebäude ohne Eingang, isolierte Hex-Außenzellenkontrolle und Mine auf
  ungeeignetem Standort: erst einen kleinen Fall auswählen, dann native
  Ablehnung/Geometrie und Zugang belegen. Kein Forschungsaufbau ohne
  Umfangsentscheidung. [Bisherige Belege](docs/large-building-geometry.md).
- Allgemeine Kraftgeometrie/Drehungen: Vorschauindikatoren gegen reale
  Netzverbindung prüfen; Abschluss erst mit getrennten Geometrie-/Netzbelegen.
- Akzeptierte synthetische Grenzen (unter anderem Datei-/Antwortfehler und
  unterbrochene Schreibaufträge) bleiben Grenzen; kein neuer Liveauftrag allein
  durch diese Bereinigung. [0.74-Nachweise](docs/releases/0.74.0.md).

## Abgeschlossen

### A01 — P1 / S: widersprüchliche aktuelle Einstiege bereinigen

- [x] Am 2026-10-18: README, START_HERE, Übergabe, Kompatibilität und missionsplan
  bereinigt. PROJECT_STATE trennt aktuellen Abschluss von historischer Chronik;
  aktuelle Testzahlen werden nur dort gepflegt. Inhalts-/Diff-/Linkprüfung erfolgt.

### A02 — P2 / M: aktive Aufgaben aus der Chronik herauslösen

- [x] Am 2026-10-18: vollständigen vorherigen Backlog als Archivstand erhalten;
  A03–A12, B-/F-Restumfänge, konkrete Altbefunde und Zurückstellungen hier geführt.
  Aufgaben haben Status, nächsten Schritt und Abschlusskriterium; historische
  Gate-Aufträge stehen im Archiv. Keine Code-, Regel- oder Abnahmeänderung.

### A06 — P2 / S: exakt das gerade gebaute Paket installieren

- [x] Am 2026-10-18: direkter Ergebnisparameter mit Build-ID, Paketpfad und
  SHA-256-Werten ersetzt die Auswahl nach Änderungsdatum. Manifestabgleich und
  Stagingprüfung vor Austausch; installierte Dateien gegen dieselben Hashes.
- **Nachweis:** alle 13 synthetischen Paketfälle im menschlichen Gate bestanden,
  einschließlich neuerem Fremdpaket, fehlendem Ergebnis und beschädigten Dateien.
  Gate verwendet den vom Build gemeldeten Pfad und bestätigt fünf Datei-Hashes.
  Anschließend Supervisor-Wiederanschluss und native Bridge 0.74.0 samt aktueller
  Session lesend bestätigt. Keine Spielmutation; Fehlerfälle bleiben synthetische
  Nachweise. [Ablauf](DEVELOPMENT_WORKFLOW.md#skriptoptionen).

Ältere Abschlüsse und Entwicklungskataloge: [Backlog-Archiv](BACKLOG_HISTORY.md).
Fachliche Abnahmen und Grenzen: [Projektstand](PROJECT_STATE.md).
