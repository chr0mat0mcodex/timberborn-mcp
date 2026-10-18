# B06 — Reload-Abgleich einer Baucharge: Entwurf

Stand 2026-10-18. **Auf Nutzerwunsch vorerst zurückgestellt.**
Nur Entwurf, nicht implementiert und nicht live abgenommen. Wiederaufnahme nur
auf neuen ausdrücklichen Auftrag; B06.1-Nachweise bleiben erhalten.
B06.1 inzwischen gezielt geprüft: [öffentliche API und zwei erhaltene Objekt-IDs](project-reload-api-evidence.md).
Eigener Beleg-Roundtrip, Ladebereitschaft und Kohärenz bleiben offene Pilotnachweise.

Auftrag: eine Projektfamilie, minimale Persistenz, rein lesender Abgleich nach
Spielneuladen. Keine automatische Wiederaufnahme und kein neuer Bauauftrag.

## Entscheidung und kleinstes Pilotpaket

Empfehlung: genau einen ausdrücklich ausgewählten, terminalen Originalauftrag
von `run_large_project` mit höchstens vier Teilen als unveränderlichen Projektbeleg
im regulären Spiel-Save mitführen. Ein Weg und ein kleines Stapellager reichen
für den Positivfall. Aktive Chargen, Reparaturketten, manueller Altmodus,
Räumgruppen, Produktions- und Zeitläufe sind ausgeschlossen. Bestehende allgemeine
Baugrenzen bleiben unverändert; die Vier-Teile-Grenze gilt nur für diesen Belegpilot.

Der Pilot beantwortet nach Reload: „Welche Teile dieser gespeicherten Absicht
sind heute eindeutig vorhanden, fehlen oder bleiben unbekannt?“ Er stellt weder
Controller noch Journal wieder her. Ein terminaler Auftrag darf unfertige oder
unbestätigte Teile enthalten; gerade diese Unterscheidung soll erhalten bleiben.

Kurzprüfung der Annahmen: Fortfahren mit dem Entwurf. Die gespeicherte Session
ist keine Save-Identität; ein letzter externer Bericht ist kein Beleg für einen
älteren Save. Unbekannte Identität muss unbekannt bleiben. Eine neue globale
Persistenzarchitektur oder Fortsetzungssteuerung wäre für diesen Pilot unnötig.

## Verifizierter Ausgangspunkt

Quellstand beim Entwurf: `bbd2cfe`; keine Laufzeitänderung für B06.

- [BridgeMod](../mod/Timberborn.AgentBridge/BridgeMod.cs): neue zufällige Session-ID
  pro Dienstinstanz; Spielzugriffe über Hauptthread, aktuelle Session wird geprüft.
  `LoadedSave` wird bisher für lokale Lognamen verwendet. Namen sind kein stabiler Schlüssel.
- [LargeProjectBatches](../mod/Timberborn.AgentBridge/LargeProjectBatches.cs):
  Chargen und aktive Ausführung im Arbeitsspeicher. Gleiche actionId mit gleichem
  Fingerprint liefert in derselben Sitzung die Quittung; abweichende Daten werden
  abgelehnt. Fingerprint enthält Session, Plan und Laufparameter.
- [LargeProjectService](../mod/Timberborn.AgentBridge/LargeProjectService.cs):
  Auftragsdaten und Wegbaseline im Speicher; deterministische Teil-IDs werden
  beim Start erzeugt. Das ersetzt keinen Nachweis ihrer Stabilität über Save/Load.
- [LargeProjectRecovery](../mod/Timberborn.AgentBridge/LargeProjectRecovery.cs):
  exakte ID, Initialisierung, Vorlage, Position und Orientierung werden geprüft.
  Diagnose kann Identitätsbestätigungen ins aktuelle Ledger zurückschreiben.
  Dieser ganze Diagnosepfad ist deshalb kein unverändert wiederverwendbarer,
  zustandsfreier Reload-Leser. Einzelne Beobachtungs-/Policy-Bausteine sind nutzbar.
- [Recovery-Vertrag](project-recovery.md): heutiger Abschluss und Reparatur bleiben
  sitzungsgebunden; kein aktueller Reload-Nachweis. Alte Zugangsbaselines nicht rekonstruieren.

Offizielle [Coding-Grundlagen](https://github.com/mechanistry/timberborn-modding/wiki/Coding-basics)
verweisen für C#-Mods auf die Spiel-DLLs. Die gelesene offizielle
[Architekturseite](https://github.com/mechanistry/timberborn-modding/wiki/Timberborn-architecture)
und die lokal gesicherten Referenzen liefern hier keinen geprüften Save-Vertrag.
Die anfängliche Lesesperre des Managed-Pfads ist durch B06.1 geklärt:
öffentliche Save-/Load-Signaturen und zwei IDs nach Reload sind nun belegt.
Snapshot-Kohärenz des eigenen Belegs und genauer Ladezeitpunkt bleiben offen.
Keine private Reflection, Save-Datei-
Manipulation oder fremde Persistenzbibliothek als stiller Ersatz.

## Persistenzort und Alternative

Bevorzugt: eigener versionierter Datenbereich der Mod im normalen Spiel-Save,
geschrieben/gelesen ausschließlich über eine belegte öffentliche Spiel-Schnittstelle.
Die gemeinsame Speicherung von Welt und Beleg ist eine zu prüfende Voraussetzung.
Kein Schreiben in `.timber` durch MCP, keine zusätzlichen Save-Aufrufe im Abgleich.
Der Mensch speichert/lädt im späteren Test regulär. Das ist Teststimulus, keine
neue allgemeine Pflicht zum Erhalt eines Spielstands.

Konservative Alternative: lokaler externer JSON-Beleg. Er wäre einfacher zu
inspizieren, kann aber ohne verlässliche Snapshot-Bindung bei Save-Rollback oder
Kopien einen falschen Verlauf unterstellen. Deshalb im Pilot nicht bevorzugt.
Wenn die öffentliche Save-Anbindung nicht belastbar ist, endet das vorgeschaltete
Prüfpaket mit dem Befund; kein automatischer Wechsel auf Dateinamen, Zeitstempel,
Koordinatenähnlichkeit oder externen „letzten Stand“.

Keine neue Datenbank, kein Cloudspeicher, kein globales Projektarchiv. Im Pilot
höchstens ein Beleg pro geladenem Save-Zweig, höchstens 32 KiB kodierte Nutzdaten.
Kein stilles Überschreiben oder Verdrängen bei voller Kapazität. Ein vorhandener
Beleg bleibt beim nächsten normalen Save erhalten; kein Lösch-/Ersetzungswerkzeug
im Pilot. Alte Saves ohne Beleg bleiben ausdrücklich ohne Reload-Nachweis.

## Minimaler gespeicherter Vertrag — vorgeschlagene Felder

| Feld | Inhalt und Zweck |
| --- | --- |
| schemaVersion, family | Format 1 und ausschließlich `large_project_batch`; unbekannte Version nicht interpretieren. |
| checkpointId | Neue opaque UUID bei ausdrücklicher Belegerstellung, nicht bei jedem Save/Lesen. |
| sourceSession, actionId | Historische Herkunft, niemals gültige Autorisierung der neuen Session. |
| bridgeVersion, catalogFingerprint | Herkunft/Kompatibilitätsprüfung; Hash ist kein Vertrauens- oder Livebeweis. |
| districtIdentity | Bezirks-ID, Vorlage, Position und Orientierung zur getrennten Identitätsprüfung. |
| terminalState, released | Historischer Zustand bei Belegerstellung; nicht heutiger Projektabschluss. |
| parts (1–4) | index, template, x/y/z, rotation, dependsOn, erwartete entityId, recordedState. |
| intentDigest | Hash über kanonische Familien-/Bezirks-/Teilabsicht einschließlich Indizes; vom alten planKey getrennt. |

`recordedState` bewahrt die vorhandenen Unterschiede pending/rejected/unconfirmed/
construction/awaiting_access/finished. Nicht aus einer vorberechneten entityId auf
eine erfolgte Platzierung schließen. Abhängigkeiten bleiben Kontext, kein Arbeitsauftrag.
Keine vollständigen Requests, reasoning-Texte, Benutzernamen, Save-Namen/-Pfade,
Tokens, Rohlogs, Bildschirmbilder, Inventarhistorien oder Spielkopien speichern.
Keine alten Timer, Simulationseigentümer, Pausenbestätigungen, planKeys, Wegproben,
Fehlerinjektionen oder Controller-Objekte serialisieren.

Der gespeicherte Beleg stammt aus dem tatsächlich geladenen Save. Das ist die
Snapshot-Bindung; sourceSession ist lediglich Herkunft. Save-Kopien dürfen denselben
Beleg enthalten, werden aber nur gegen ihre jeweils geladene Welt gelesen.
Keine globale Erkennung von Save-Abstammung oder späteren Zweigen behaupten.
Ein Reload eines älteren Saves ohne Beleg liefert `checkpoint_absent`; Ereignisse
nach diesem Save sind nicht rekonstruierbar. Absturz vor erfolgreichem Speichern
hat dieselbe Grenze. Kein „genau einmal über alle Save-Zweige“-Versprechen.

## Lebenszyklus und vorgeschlagene Oberfläche

Alle Namen in diesem Abschnitt sind Entwurf, keine aufrufbaren Werkzeuge.

1. `checkpoint_large_project(session, actionId, reasoning)` wählt den einen
   bestehenden Originalauftrag ausdrücklich aus. Aktuelle Session, bestätigte
   Pause, terminaler Auftrag, kein aktiver Batch/Simulationslauf, keine Reparatur-
   Eltern-/Kindbeziehung, keine aktive Identitätsinjektion, maximal vier Teile.
   Snapshot nur aus realen Auftragsdaten, ohne vorangehende mutierende Diagnose.
   Der neue Aufruf ist als Metadatenmutation gekennzeichnet und verwendet die
   vorhandene technische Baufreigabe auf MCP und Bridge; er baut/speichert nicht.
   Rückgabe `captured_in_memory`, niemals voreilig `persisted`.
2. Derselbe Aufruf bei unverändertem Inhalt gibt denselben Beleg zurück.
   Andere Absicht oder geänderter Beleginhalt: `checkpoint_conflict`; belegter Slot
   mit anderem Auftrag: `checkpoint_capacity`. Keine automatische Aktualisierung.
3. Ein normaler Spiel-Save speichert den Beleg neben der Welt. Fehler dürfen keinen
   Persistenzerfolg melden. Kohärentes Save/Load-Verhalten ist Voraussetzung des Piloten.
4. Beim Laden: Beleg validieren und in einen getrennten unveränderlichen Lesespeicher
   aufnehmen. Niemals in `jobs`, `batches`, `activeBatch`, Simulation oder Reparaturledger
   einsetzen. Keine Start-, Advance-, Cancel-, Pause- oder Platzierungsaktion auslösen.
5. `inspect_project_reload(session, reasoning)` liest den einzigen geladenen Beleg
   und alle bis zu vier Objekte in einem Spielthread-Aufruf. Keine externe Absicht
   als Eingabe. Aktuelle Session ist Pflicht. Vor vollständigem Szenen-/Entity-Laden
   `world_not_ready`; bei laufender Simulation `pause_required` ohne selbst zu pausieren.
   Nach Reload ist eine früher gespeicherte Pause kein heutiger Pausennachweis.
6. Ausgabe mit aktueller Session und Beobachtungszeit, checkpointId, Herkunft,
   `executionRestored=false`, `replayAllowed=false`, alter Auftragslage und separatem
   `currentAssessment`. Ein gleicher Wiederaufruf darf bei unveränderter Welt nur
   Metadaten wie Beobachtungszeit ändern; kein Journalfortschritt oder Schreibzugriff.

Fehlerhafter Beleg: `checkpoint_invalid` oder `checkpoint_schema_unsupported`,
keine Teilinterpretation. Größe, Anzahl, UUIDs, eindeutige IDs/Indizes, DAG und
Wertebereiche validieren. Katalogabweichung: `compatibility_unknown`, keine
abschließende geometrische/Zugangsbestätigung. Laden des Spiels nicht wegen eines
ungültigen optionalen Belegs verhindern; Daten nicht still durch leeren Erfolg ersetzen.
Ein ungültiger belegter Slot darf nicht automatisch überschrieben werden.

## Identität, Zugang und Ergebnis

| Beobachtung | Aussage |
| --- | --- |
| Exakte ID vorhanden, initialisiert, keine Vorschau; Vorlage/Position/Orientierung stimmen | `identity=matched`; Bauzustand und Zugang separat aktuell lesen. |
| ID bei vollständig bereiter Registry nicht vorhanden | `identity=missing`; historische Bestätigung separat anzeigen. Kein automatischer Abriss- oder Nie-gebaut-Befund. |
| Gleiche ID, andere Vorlage/Position/Orientierung | `identity=mismatch`; nicht als verschobenes eigenes Teil übernehmen. |
| Gleiches Gebäude am gleichen Ort, aber andere ID | Das erwartete Objekt bleibt missing; kein Ersatzmatch nach Nähe oder Geometrie. |
| Initialisierung, Leserfehler, unvollständige Registry oder unbekannte Kompatibilität | `identity=unknown` mit Grund; nicht missing und nicht sicher. |

Bei matched: `construction=unfinished/finished/unknown`; Bauarbeiterzugang und
fertiger Gebäudezugang als getrennte Zustände mit eigenen Unknown-Gründen lesen.
Fehlender/abweichender Bezirk darf eine passende Objektidentität nicht auslöschen,
aber die zugehörige Zugangsaussage bleibt unbekannt. `recorded=pending/rejected`
mit heute passender ID ist `unexpected_presence`, kein rückwirkender Auftragsbeweis.
`unconfirmed` plus matched zeigt heute vorhandenes passendes Objekt, lässt den
historischen Quittungsstatus unverändert. Unconfirmed plus missing bleibt ungeklärt.

Aggregat priorisiert unknown/mismatch/unexpected_presence als `needs_review`,
sonst fehlende Teile als `parts_missing`, unfertige als `construction_present`,
fehlenden Zugang als `access_not_confirmed`; nur alle erwarteten, bestätigbar
zugeordneten Teile fertig und zugänglich ergeben `all_parts_finished_access_observed`.
Keines dieser Ergebnisse setzt den alten Auftrag auf completed oder released.
Die frühere Wegbaseline ist `not_restored`; kein Vergleich früher/heute und kein
behaupteter Bestandsschutz. Kraft, Lieferung, Besetzung und Produktion ausgeschlossen.

## Wiederaufruf und spätere Aktionen

Der reine Leser kann beliebig ohne Ausführung wiederholt werden. Zusätzlich muss
ein geladener gültiger Beleg seine actionId in dieser Welt gegen Wiederverwendung
sperren: `run/start/advance/repair` auf diese alte Identität (auch als parentActionId)
werden vor Dispatch als `reload_checkpoint_read_only` abgelehnt; nicht aus dem Beleg
wiederherstellen. Bestehende Live-Journale derselben Session behalten ihre bisherige
Semantik. Nach Reload auch kein `close` oder Injektionsaufruf auf den Beleg möglich.

Das ist eine kleine erforderliche Schreibpfadsperre, keine Fortsetzungsfunktion.
Sie gilt nur für die im geladenen gültigen Beleg enthaltene Identität. Bei beschädigtem
Beleg ist diese Schutzabdeckung unbekannt und wird so gemeldet; keine globale
Deduplizierung behaupten. Alte Sessionparameter werden weiterhin abgelehnt.
Neue Aktionen benötigen neue IDs und frische bestehende Planung/Prüfungen; niemals
fehlende Teile automatisch nachbauen oder andere Baustellen zu eigenen erklären.

## Geplante Nachweismatrix — noch nicht ausgeführt

| Fall | Erwarteter Nachweis |
| --- | --- |
| Terminaler Zweiteiler, normal speichern/laden, neue Session | Beleg wieder da; beide IDs und Geometrie matched; aktueller Bau-/Zugangsstatus getrennt; kein Controller gestartet. |
| Ein Teil unfertig oder Zugang inzwischen blockiert | Identität bleibt matched; korrekter aktueller Bau-/Zugangsstatus, keine falsche Fertigmeldung. |
| Bestätigtes Teil fehlt / gleiche Geometrie mit neuer ID | missing statt Ersatzmatch; historische Bestätigung erhalten, keine automatische Wiederholung. |
| Unconfirmed fehlt / pending ist vorhanden | Wirkung ungeklärt bzw. unexpected_presence; kein rückwirkender Quittungserfolg. |
| Initialisierung oder Lesefehler | unknown/world_not_ready; kein missing; späterer expliziter Leser darf neue Beobachtung liefern. |
| Andere Welt, älterer Save ohne Beleg, Save-Kopie | absent ohne globale Suche; Kopie nur mit ihrem enthaltenen Beleg und ihren Objekten vergleichen. |
| Falsche Session, ungültiges/zu großes/zu neues Format, Katalogwechsel | Explizite Ablehnung oder compatibility_unknown; keine Mutation, keine erfundenen Werte. |
| Identischer Leser und alter Start zweimal nach Reload | Leser ohne Nebenwirkung; alter Start vor Dispatch gesperrt; null neue Objekte, null Tempobefehle. |
| Aktiver/zu großer Auftrag, zweiter Beleg, geänderte Absicht | Capture abgelehnt, vorhandener Beleg unverändert; kein stiller Austausch. |
| MCP-Neustart ohne Spielreload | Bestehendes Modledger bleibt maßgeblich; kein künstliches Reload und keine doppelte Registrierung. |

Synthetische Policy-/Contractfälle im späteren menschlichen Gate; Fixtures ohne reale
Spiel-IDs. Live nur ein Zweiteiler und ein gezielter Negativfall mit entferntem oder
ersetztem Teil; zweite Save-Version dafür regulär erzeugen. Vorher beleglosen Save
als Kontrolle auswählen. Wiederholungsaufrufe und ausbleibende Objekt-/Tempoänderung
explizit beobachten. Fehlgeschlagene Kontrollzuordnung beendet den Pilot sofort.
Keine breite Save-Sammlung, keine weiteren Familien oder Langzeit-Stresstests.

## Umsetzbare Reihenfolge nach gesonderter Freigabe

- **B06.1 — API-/Identitätsvorprüfung (begrenzt abgeschlossen):** öffentliche
  Singleton-Save-Anbindung per Metadaten bestätigt; zwei vorhandene Teile behalten
  ihre IDs nach regulärem Save/Load. Alte Session abgelehnt, altes Ledger fehlt.
  [Belege und Grenzen](project-reload-api-evidence.md). Eigener Beleg-Roundtrip
  und Kohärenz werden dadurch noch nicht als abgenommen dargestellt.
- **B06.2 — enger Pilot (nicht freigegeben):** zuerst minimalen eigenen Save-Roundtrip und Ladebereitschaft beweisen; danach
  vier Teile/ein Beleg, Capture und
  getrennten Leser samt alter-ID-Sperre implementieren; minimale Policy-/Contracttests,
  Anleitung und Versions-/Fähigkeitsvertrag ergänzen. Keine generische Persistenzmigration.
- **B06.3 — Gate und Liveabnahme (ausstehend):** menschliches Gate, obige kleine
  Save/Load-Matrix, Nutzen/Aufwand und verbleibende Grenzen dokumentieren. Featurecode
  erst danach committen/pushen. Bei Erfolg weiterer Umfang nur gesondert entscheiden.

Nutzen hoch für verständliche Zustandsdiagnose nach Reload. Entwurfsaufwand klein;
Umsetzung vorläufig mittel (Serializer-Anbindung, klar getrennte Beobachtung,
Schreibpfadsperre und Reload-Abnahme), mit verbleibender Unsicherheit bis zum eigenen Save-Roundtrip. Kein allgemeines
Recovery-Framework. Erfolg des Entwurfs: reviewbarer Daten-/Lebenszyklusvertrag und
prüfbarer nächster Schritt; Erfolg der Laufzeit bleibt ausdrücklich offen.
