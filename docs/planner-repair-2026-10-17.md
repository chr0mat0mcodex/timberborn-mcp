# Bauplaner und Fehlerweitergabe — begrenzt live abgenommen

Status: Korrektur auf Basis 0.63.0 nach menschlichem Gate begrenzt live abgenommen.
Beim menschlichen Gate bestanden Vorschaltprozessprüfung und MCP/Test-Kompilierung;
ein Unit-Test zur Chargenanzeige schlug fehl. Installation wurde nicht erreicht.
Die generische Vorschaufehlermeldung wurde vor dem strukturierten Chargenfehler
ausgewertet. Reihenfolge korrigiert; Regression prüft nun Code, Phase und Recovery
auch nach Journal-Neuladen über `inspect` und `advance`. Erneutes Gate laut
Entwickler bestanden; anschließend neue native Session bestätigt.
Learning: Spezifische Fehlerberichte vor generischen Meldungen auswerten und
ihre Anzeige auch nach Wiederaufnahme prüfen. Die bisherige 0.63-Abnahme ersetzt
diesen Nachweis nicht.

## Fehler und Korrektur

Der Überlebenspilot löste beim gemeinsamen Lagerplan eine native
`ArgumentException` in der Path-Vorschau aus. Danach blieb der Planer gesperrt.
Die Zugangssuche konnte die Ausnahme als Geometriefehler umdeuten; die Charge
verbarg den Folgefehler hinter `batch_interrupted` und blieb in `planning`.

- Gecachte Vorschauen werden vor dem Versetzen aus Vorschau-Diensten entfernt.
- Ungültige/kollidierende Teile werden nicht in die Vorschau-Navigation eingefügt.
  Gleiche Hauptwegzellen gelten auch bei nicht kollidierenden Blockschichten als
  Konflikt. Begrenzte Stützvorschauen bleiben nur ohne Kollision möglich.
- Negative Geometrie bleibt `native_invalid`, zusätzlich `placementReason` und
  `previewRegistered`. Kein neuer Ausführungsmodus, keine Umgehung der Prüfung.
- Nach dem Aufräumen werden reale und Vorschau-Wegbaseline verglichen.
  Native Laufzeitfehler erhalten `project_preview_failed`; die Sitzung bleibt
  gesperrt. Keine automatische Entsperrung bei unbewiesener Wiederherstellung.
- Fehlende Projekte melden `project_not_found` statt generischem `state_conflict`.
- Bauchargen behalten `failure.code`, `stage`, `execution` und `recoveryAction`
  auch im gespeicherten Compact-/Full-Bericht und Ingame-Ergebnis. Fehlgeschlagene
  Planung stoppt dauerhaft; `advance` startet sie nicht erneut.
- Eine verlorene Bauantwort behält `pending_build` und dieselbe Aktionskennung.
  Erst deren lesender Abgleich darf den Fehler löschen; kein erneutes `run`.

`failure.execution=no_build_order` bezieht sich nur auf die gescheiterte
Planungsphase. Es behauptet weder erfolgreiche Vorschau-Bereinigung noch, dass
ältere Räum-/Bauschritte nie ausgeführt wurden. Der Fehlerumschlag bleibt in
diesem Fall konservativ `unconfirmed`. Nur explizite Vorauftragsablehnungen
werden `not_executed`. Keine beliebigen Exceptiontexte werden weitergereicht.

Die Schutzänderung adressiert den im Quelltext erkennbaren Registrierungsfehler.
Die genaue native Fehlerstelle war im alten Log nicht enthalten; ein Nachweis,
dass der konkrete Save damit wieder baut, entsteht erst im untenstehenden Pilot.
Neue lokale Diagnose nennt Exceptiontyp und Methode, keine rohe Fehlermeldung.
Öffentliche bestehende Preview-/Path-/RoadProtection-APIs und die lokal gesicherte
Preview-Referenz bleiben Grundlage; keine neue Abhängigkeit oder private Reflection.

## Vorbereitete Regressionstests

- `ProjectPreviewPolicyTests`: belegte Wegzelle trotz positivem Validator bzw.
  Stützannahme blockiert; ungültig/frei/deferred getrennt; Vorschaufehler nicht als
  sichere Vorauftragsablehnung klassifiziert.
- `StructureBatchTests`: nativer Fehler während Planung bzw. Start bleibt über
  Journal-Neuladen, `inspect` und `advance` erhalten, ohne erneut zu planen/starten;
  verlorene Antwort wird nur lesend abgeglichen; private Diagnose wird unterdrückt.
- `LargeProjectPlanningTests`: konkrete Vorschaufehler über HTTP/MCP/Ingame-Text,
  fehlendes Projekt ausschließlich per GET, bestehende Vorauftragsfälle weiterhin.

Diese Tests sind durch das menschliche Gate geprüft. Reale native Vorschau-Lebensdauer
und intakte Wegverbindungen werden durch Unit-Fixtures allein nicht bewiesen.

## Menschliches Gate

Timberborn beenden und im Projektverzeichnis ausführen:

```powershell
.\scripts\prepare-human-live-test.ps1 -TimberbornManagedDir '<Timberborn_Data\Managed>'
```

Den Platzhalter durch das übliche lokale Managed-Verzeichnis ersetzen. Das Skript
testet, baut Release, paketiert, installiert und prüft. Danach Spielstand laden
und `live bereit` melden. Keine Builds/Tests/Installation durch den Agenten.

## Gezielter Livepilot nach dem Gate

1. Backend über bestehenden Vorschaltprozess starten, Version/Session/Pause lesen.
   Baustellen und aktuelle Weg-/Bestandszugänge frisch erfassen.
2. Negativkontrolle: explizite Vorschau eines zusätzlichen Path auf einem bereits
   fertigen Weg. Erwartet: `native_invalid`, keine Registrierung/Baustelle, intakte
   reale und Vorschau-Baseline, keine neue Planersperre.
3. Direkt anschließend freier Mehrfachplan mit zwei kleinen Tanks, Karottenlager
   und gegebenenfalls gemeinsam nötigen Wegen. Mehrere Vorschauen mit geänderten
   Positionen prüfen die Wiederverwendung gecachter Vorschauen. Ursprüngliche
   Region (112,148,3), 8×8, nur verwenden, wenn im geladenen Save noch passend.
4. Ein gemeinsamer Start: alle Teile als Baustellen bestätigt, danach begrenzter
   Lauf bis Fertigbau und Zugang; Lagergut/-modus separat rücklesen. Keine
   Fertigstellungsbarriere zwischen den einzelnen Platzierungen.
5. Kontrolliert bei laufendem Tempo einen Plan ablehnen lassen; konkrete Ursache
   prüfen, anschließend pausieren. Fehlende synthetische Projektkennung muss
   `project_not_found` liefern. Kein künstlicher nativer Crash im Spiel nötig;
   Runtimefehler-/Quittungsverlustpfade bleiben synthetische Gate-Nachweise.

Stopp beim ersten kaputten Kontrollfall, unbestätigten Auftrag, verlorenen
Bestandszugang oder erneuten Vorschaufehler. Kein Retry mit neuer Kennung und
keine automatische Wiederholung der Gate-Kette. Erst nach bestandenem Gate und
diesem Baupilot committen/pushen. Danach den Überlebenslauf wieder aufnehmen.

## Liveergebnis vom 17. Oktober

- Belegter Path: `native_invalid`, `geometry_collision`, `previewRegistered=false`,
  `restored=true`, `sessionLocked=false`, keine verlorenen Anschlüsse.
- Wiederverwendete Vorschauen mit unterschiedlichen Positionen: acht native
  Vorschauen fanden zwei Tanks, anschließend 16 eine vollständige Dreiergruppe.
  Kein Vorschaufehler. Zwölf Umweg-Wege statt behaupteter optimaler Lösung.
- Ein gemeinsamer Start: 15/15 Aufträge bestätigt, null Ablehnungen. Nach zwölf
  Spielstunden automatische Pause, 14 Teile fertig; ein Tank noch in Arbeit.
  Frische Diagnose: Zugänge intakt, Material vorhanden, kein Planerfehler.
  Weitere 16 Stunden reine Simulation über Nacht/Arbeitsbeginn, kein Neubauauftrag.
  Danach 15/15 frisch fertig und zugänglich, null verlorene Bestandsanschlüsse,
  null Fremdbaustellen, Projekt freigegeben. Endzeit Tag 51, 14:15, Tempo 0.
- Zwei Tanks auf Water, Lager auf Carrot, jeweils accept separat rückgelesen.
  Keine Produktions- oder Befüllungsgarantie aus diesem Konfigurationsnachweis.
- Laufende Baucharge liefert `project_plan_busy`; laufende Simulation ohne aktive
  Baucharge `project_plan_requires_pause`, jeweils `not_executed`.
  Synthetische unbekannte Projektkennung liefert `project_not_found` und passenden
  Ingame-Text. Keine generische `state_conflict`-Umdeutung in diesen Kontrollen.
- Grenzen: native Ausnahme/Planersperre, Chargenpersistenz über inspect/advance
  und Quittungsverlust nur synthetisch im Gate. Kein absichtlich ausgelöster
  Live-Crash, kein neuer Nachweis fertiger Höhenbauten oder aller Bauphasen.
- Nebenbefund: Ingame-Fortschrittsanzeige zeigt 0/15 bei strukturiertem 14/15.
  Fehlerhafte Darstellung separat im Backlog; strukturierte Diagnose maßgeblich.
- Zwei fehlerhafte Pilotanfragen vor Dispatch korrigiert: SmallWaterTank statt
  katalogbestätigtem SmallTank und Textkennung statt UUID. `invalid_argument`
  nennt das betroffene Feld nicht; Schema-/Fehlerverbesserung im Backlog.

Anleitung und lokaler Hermes-Skillentwurf wurden um Fehlervertrag und Livegrenzen
ergänzt; der Skill bleibt zurückgestellt. Vorbestehende Hermes-/
Startskriptänderungen und andere Arbeitskopieänderungen gehören nicht zu diesem Fix.
