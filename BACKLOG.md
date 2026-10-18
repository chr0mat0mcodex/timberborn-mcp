# Backlog

## Aktuelle Arbeit

**A01/A02 am 2026-10-18 abgeschlossen:** Projektköpfe bereinigt und aktive
Aufgaben von der Chronik getrennt. Reine Dokumentationspflege; kein neues
Codefeature begonnen und kein Test-Gate offen. Letzter fachlicher Abschluss:
automatische Bretterketten-Vorprüfung, siehe [Projektstand](PROJECT_STATE.md).

Als nächstes empfohlen: A06 (eindeutige Paketauswahl), danach A03 als kleiner
Refactoringpilot, anschließend A04/A05. Das ist eine Empfehlung, kein bereits
gestarteter Implementierungsauftrag. A07/A08/A10 bei passenden Featurearbeiten,
A09/A11 nach Bedarf; A12 nach gewünschter Fähigkeit einzeln auswählen.

## Projektprüfung 2026-10-18 — abarbeitbare Befunde

Prüfbasis und vollständiger Audit: [Archivstand](BACKLOG_HISTORY.md#archivstand-2026-10-18-vor-a01-und-a02).
Ergebnis: gezieltes Refactoring von Versionsprofilen, Werkzeugdispatch und
Prüfketten sinnvoll; kein belegter Bedarf für einen Komplettumbau.
Statische Prüfung, keine neue Build-/Liveabnahme. Alle folgenden A03–A12 sind
**offen**. P2 = vor der nächsten betroffenen Erweiterung, P3 = nach Bedarf;
S/M/L sind relative Aufwandsschätzungen. Codeänderungen benötigen weiterhin
das [menschliche Gate und den gezielten Livetest](DEVELOPMENT_WORKFLOW.md).

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

## Aktuell offene Folgearbeiten

Die A12-Pakete bündeln konkrete nächste Nachweise. Die folgenden eigenständigen
Altbefunde bleiben sichtbar, bis sie reproduziert oder mit einem passenden
Abschlussbeleg verworfen wurden. Herkunft: [archivierter Verlauf](BACKLOG_HISTORY.md#archivstand-2026-10-18-vor-a01-und-a02).

| ID / Status | Nächster Schritt | Abschlusskriterium |
| --- | --- | --- |
| N01 — offen, A12 | Kartenfehler um reale Kartengrenzen ergänzen. | Abfrage außerhalb der Karte nennt Achse/Grenze; gültige Randzelle funktioniert. |
| N02 — offen, A12/F04 | Unteren Zielweg an gedrehter Bestandstreppe gezielt prüfen. | Native Geometrie sowie Bau- und Fertigzugang positiv, seitliche Fehlverbindung negativ belegt. |
| N03 — offen, A12 | Deterministischen Fall unbekannter Objektidentität entwerfen; Fehler-Injektion ist noch nicht freigegeben. | Native Sperren, repairEligible=false und closeEligible=false belegt; keine Zeitrennen oder Save-/Ledger-Manipulation als Nachweis. |
| N04 — zu reproduzieren | `project_start_plan_changed` nach Zugangsergänzung mit frischer expliziter Gesamtvorschau vergleichen. | Identische Eingaben nachvollziehbar angenommen oder konkrete Änderung benannt; kein Blindretry. |
| N05 — zu reproduzieren, B10 | Ingame-Text 0/15 gegenüber strukturiertem 14/15 aus dem Planerpilot prüfen. | Laufende/terminale Anzeige zählt korrekt und unterscheidet Ledger von frischer Diagnose; 0.73-Fertigbaudiagnose allein schließt den UI-Befund nicht. |
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
| B10 — teiloffen | N05 prüfen, Zeitlimit-/Stopgründe in Kurztexten präzisieren und weitere Auftragsfamilien einzeln erschließen. | Ergebniszeile stimmt mit strukturiertem Beleg überein und ist im Spiel lesbar. |
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
| F04 — teiloffen | Besondere Standort-/Distrikt-/Geometriefälle, zunächst N02; mehrere Türen nur bei belegter API. | Je ein gültiger und ungültiger Fall mit getrennten Zugangsbelegen. |
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

Ältere Abschlüsse und Entwicklungskataloge: [Backlog-Archiv](BACKLOG_HISTORY.md).
Fachliche Abnahmen und Grenzen: [Projektstand](PROJECT_STATE.md).
