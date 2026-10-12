# 0.40.0 — Änderungsjournal

0.54-Erweiterung begrenzt live abgenommen: [Pflanzen/Flaechen,
Gruppierung und neue Nachweisgrenzen](spatial-journal-0.54.md). Die folgenden
Angaben zur ersten Fassung und ihrer Abnahme sind historisch.

## Begrenzt live abgenommen 2026-10-11

Nach menschlichem Gate Paket `agent-bridge-0.40.0-20261011-214613-482c1f73`
erreichbar; Codex-Katalog nach Client-Neustart aktualisiert. Begrenzter Abschluss-
pilot nach menschlichem Gate bestanden.

- Baseline: 424 verfolgte Gebäude/Wege, null Einträge statt erfundener Altaufträge,
  trackingComplete=true und observationFailures=0.
- Vier Deichbaustellen und später deren Entfernung sowie drei Treppenbaustellen
  und zwei Entfernungen erfasst. Neue Objekte per state_difference, Entfernungen
  per event, Herkunft unknown. Menschlicher Eingriff anschließend bestätigt.
- Papierfabrik per MCP pausiert und wieder aktiviert, beide Werte separat
  rückgelesen. Exakt zwei zugehörige Journaländerungen, Quelle mcp.
- Gefilterte Limit-1-Folge, Fortsetzung und leere Seite korrekt; nextCursor
  überspringt ausgeschlossene Einträge. stale_session und cursor_ahead bestätigt.
- Menschliche Gegenprobe bestätigt: weitere Löschungen/Neubauten mit getrennten
  Objekt-IDs, Sägewerkpause false -> true, Tempo 0 -> 7 -> 0 und zwei Plattformen
  finished false -> true. 30 fortlaufende Einträge, keine Erfassungsfehler oder
  Pufferverluste. Herkunft korrekt unknown; Fertigstellungen nicht als Auftrag
  oder bewiesene menschliche Aktion ausgegeben.
- Sessionwechsel bestanden: neue Sitzungskennung, 431 Bestandsobjekte und leeres
  Journal mit Cursor 0; alte Sitzung mit Cursor 30 -> stale_session.
- Keine Ressourcen durch den Agenten gelöscht; menschlich gesetzte Bauten und
  Sägewerkpause bleiben bestehen. Kein künstlicher Überlauf durch Spielmutationen.

Grenzen: Pause, Tempo, Hinzufügen/Entfernen und Fertigstellung live belegt.
Rezept-, Lager-, Sollpersonal- und Abrissmarkierungsänderungen nicht einzeln live
abgenommen; Überlauf durch synthetische Gate-Tests abgedeckt. Hinzufügen wurde im
Pilot durch Zustandsvergleich erkannt; keine Behauptung bestätigter Lieferung des
EntityInitializedEvent. Entfernung und Tempo wurden als event beobachtet.

## Umfang und Herkunft

`inspect_recent_actions(session, limit=32, after?, kind=all, source=all)` liest
Änderungen seit Beginn der geladenen Spielsitzung. Erfassung läuft in der Mod auch
ohne Agentenabfragen; MCP-Neustart löscht das Journal nicht, Neuladen der Spielsitzung
schon. 512 Einträge im RAM, keine Datei/Save-Persistenz, keine Namen oder Texteingaben.

Erfasst werden Gebäude und Wege (einschließlich PathSpec/Treppen): Objekt erschienen,
entfernt, Fertigzustand, Gebäudepause, Rezept, Lagergut/-modus, Sollpersonal und
vorhandene Abrissmarkierung. Global: Simulationstempo. Ein neues Objekt kann bereits
fertig sein; Entfernen belegt Verschwinden, nicht den vorausgegangenen Abrissauftrag.
Fertigstellung beweist weder Zugang noch laufenden Betrieb. Rohstoffbestände,
Pflanzen, Flächenmarkierungen und Forschung gehören nicht zur ersten Fassung.

Öffentliche EntityInitializedEvent/EntityDeletedEvent/CurrentSpeedChangedEvent
werden über EventBus erfasst. Für Einstellungen und Fertigstellung gibt es eine
zusätzliche Zustandsbeobachtung ungefähr einmal pro realer Sekunde, auch in Pause.
Vor/nach relevanten direkten MCP-Eingriffen sowie beim Journalabruf wird frisch
abgeglichen. Anfangsbestand ist Baseline, keine Serie erfundener neuer Aufträge.
Zwischenänderungen zwischen Abgleichen können fehlen. `event` oder
`state_difference` beschreibt die Belegart, keine Vollständigkeitsgarantie.

Herkunft `mcp` nur innerhalb synchroner MCP-Ausführung einschließlich eigener
Controller-Schritte. Verzögerte Wirkungen und andere Änderungen bleiben `unknown`.
Öffentliche Ereignisse belegen keinen menschlichen Urheber; deswegen werden weder
`human` noch `simulation` geraten. Auftragsannahme und beobachtete Wirkung nicht
verwechseln. Das bestehende MCP-Aufruflog bleibt separat.

## Cursor und Lücken

- Ohne after: letzte maximal 64 passende Einträge (Standard 32), chronologisch.
- Mit after: passende Einträge nach dieser Sequenz, älteste zuerst; hasMore zeigt
  weitere passende Einträge. nextCursor für die Fortsetzung verwenden.
- Auch eine leere Filterantwort bewegt nextCursor bis zum letzten beobachteten
  Ereignis. Für geänderte Filter einen eigenen Cursor verwenden.
- Cursor gilt nur für die aktuelle Session; falsche Session -> stale_session,
  Cursor oberhalb latest -> cursor_ahead. Kein automatischer Reset oder Replay.
- oldestAvailable/dropped zeigen Pufferüberlauf; gap meldet, wenn der angefragte
  Cursor vor dem erhaltenen Verlauf liegt. Ohne Cursor bedeutet gap=false keine
  vollständige Vergangenheit: Startzeit und dropped beachten.
- startedAtUtc, lastScanAtUtc, observationFailures und trackingComplete offenlegen.
  Letzteres bedeutet nur, dass der letzte Scan innerhalb des Limits lag, nicht
  vollständige Aktionserfassung. Bei Fehlern dürfen Ereignisse fehlen.

Maximal 4.096 beobachtete Gebäude/Wege und 20.000 geprüfte Registry-Einträge pro
Scan. Überschreitung zählt kumulativ als observationFailures und verwirft den unvollständigen Scan; insbesondere werden fehlende
Objekte dann nicht als entfernt protokolliert. Ereignisse bleiben begrenzt.
Antwortlimit 256 KiB. Lesender Endpunkt, keine zusätzliche Aktionsfreigabe.

## Agentenablauf

Bei „Überprüf mal meine letzten Aktionen“ Sitzung prüfen, Journal seit bekanntem
Cursor lesen oder begrenzte letzte Einträge zeigen. Ergebnis als beobachtete
Änderungen formulieren, unbekannte Herkunft offen benennen. Konflikte mit laufenden
Agentenplänen anhand frischer Objektbeobachtung prüfen; nichts automatisch zurücknehmen.
Bei Lücke neu beobachten statt aus fehlenden Einträgen unveränderten Zustand abzuleiten.

## API-Entscheidung

Bestehender Singleton-/EventBus-Pfad wie SimulationControl; öffentliche Signaturen
an den lokalen unveränderten Spielassemblies mit vorhandenem Metadatenleser geprüft.
Keine Spielmethoden ausgeführt, keine neue Abhängigkeit, Harmony oder private Reflection.
Öffentliche Metadaten beweisen noch nicht die Laufzeit-Lieferung jedes Ereignisses;
Zustandsabgleich ergänzt sie. Kein UI-Klick-Hook als unbelegter Menschennachweis.

## Vorbereitete Prüfung und menschliches Gate

Synthetisch: Tail/Cursor, Filter, leere Seiten, Überlauf, frische Puffer,
Future-Cursor, ungültige/doppelte Parameter, falsche Herkunft, Transportkorrelation,
Read-only-Katalog und echter stdio-Durchlauf. Bestehende Versions-Negativfixtures
bleiben synthetisch unbekannt, 0.40 wird positiv ergänzt. Nicht vom Agenten ausgeführt.

Nach menschlichem Skriptlauf und live bereit begrenzter Pilot:

1. Baseline ohne erfundene Altaufträge, frische Session und Cursor lesen.
2. Entwickler setzt einen erreichbaren kleinen Bauauftrag, ändert Rezept oder
   Gebäudepause und Tempo. Einträge mit konkreten Werten, Objekt/Position und
   ehrlicher Herkunft wiederfinden. Kurze Änderungen können im Polling fehlen.
3. MCP-Pausewechsel als Herkunftskontrolle, danach regulär zurückstellen; neuer
   Bau wird fertig, später regulär abgerissen. Rohstoffe nicht löschen.
4. Limit, Filter, fortlaufenden Cursor, leere Folge, falsche Session und
   Future-Cursor prüfen. Überlauf synthetisch; keine 513 Spielmutationen dafür.
5. Ein menschlicher Spielstand-Neuladevorgang bestätigt Sessionwechsel und leeren
   Verlauf. Höchstens wenige Kontrollen, bei fehlerhafter Kernprobe stoppen.

Erfolg: richtige Vorher-/Nachherwerte ohne Baseline-Spam, keine erfundene Herkunft,
keine doppelte Zustandsänderung durch Ereignis plus Scan, korrekte Cursor/Lücken.
Falls Client den neuen Katalog nicht übernimmt, Codex neu verbinden; kein neues
Buildgate allein für den Katalog. Commit/Push erst nach Gate und bestandener Liveabnahme.
