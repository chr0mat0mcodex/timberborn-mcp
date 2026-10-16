# B15 / 0.63: gesamte Struktur zuerst platzieren

Status: 0.63.0 am 17. Oktober 2026 mit ausdrücklicher Nutzerfreigabe und den
unten dokumentierten Nachweisgrenzen abgeschlossen; menschliches Gate bestanden.
Ebener Doppelauftrag vor Fertigbau, ehrlicher Zeitlimitabschluss, Reparatur samt
Originalbauten und neue Schema-2-Charge mit zwei Lagern plus Weg belegt. Manueller
Start mit vier Teilen einschließlich Weg auf unfertiger Plattform ebenfalls belegt;
unabhängiger Stapel fertig, Höhenbauteile materialblockiert, danach zurückgebaut.
Diagnose-Nachbesserung durch erneutes menschliches Gate geprüft. Separater
Vertikalpilot: fünf Aufträge gemeinsam bestätigt, einschließlich beider oberer Wege;
Materialblockade korrekt ohne Fertigmeldung, anschließend Testteile zurückgebaut.
Ebener Pilot mit zwei Wegen und Lager vollständig fertig/zugänglich bestätigt.
Geänderte Parameter unter belegter ID konkret vor Auftrag abgewiesen. Gezielter
Objektverlust im manuellen Großprojekt erkannt, kein Replay, Rest fertig und Ledger
freigegeben. Keine offenen Testbaustellen; Tempo 7 bestätigt.
Zusätzliche Einstiegspfade: Develop bis Fertigbau und zurückgelesenem `Log/accept`
sowie Räum-/Gruppenworkflow auf freier Fläche bis Fertigbau, Pause und automatischer
Ledgerfreigabe bestanden. Keine neue Vegetationsräumungsabnahme.
Ausdrücklich akzeptierte Nachweisgrenze: Teilablehnung innerhalb des synchronen Durchgangs nur
synthetisch im Gate geprüft, kein entsprechender nativer Livefehler provoziert.
Fertige Höhennavigation nicht neu nachgewiesen; keine pauschale Abnahme aller Vorlagen.

## Vertrag

Ein Plan → ein Go → alle Aufträge → Rückbeobachtung → Bauabschluss.
Die Platzierungsphase läuft synchron auf dem Game-Thread. Interne API-Aufrufe
sind seriell, aber ohne Framewechsel, Vorschau, Material-/Zugangsprüfung oder
Warten auf Bestätigung/Fertigstellung zwischen Teilen. `dependsOn` bestimmt
die Einreichreihenfolge, nicht das Warten auf fertige Stützen.
Planung und Session-/Identitätsprüfung vor dem gesamten Auftrag bleiben bestehen.

Jeder Platzierungsversuch wird vor dem nativen Aufruf registriert. Bekannte
Ablehnungen erhalten ihren Code; Exceptions ihren Typ und unbestätigten Status.
Die übrigen Teile werden dennoch versucht. Keine atomare Rückabwicklung, keine
automatische Wiederholung. Ein nativer Aufruf beweist noch keine vorhandene
Baustelle. Das wird erst nach dem vollständigen Durchgang zurückgelesen.

## Bauwege

| Weg | Platzierung und Überwachung |
| --- | --- |
| `run_large_project`, Reparatur, Räum-/Baugruppe | Gesamter Plan, danach native begrenzte Simulation bis Fertigbau oder Fehler/Zeitlimit. |
| `start_large_project` | Gesamter Plan sofort; automatische Beobachtung, kein eigener Tempoeingriff. `advance` ist kein Einzelteil-Dispatcher mehr. |
| Ebene Gebäudeprojekte / Develop | Alle Wege und Zielgebäude gemeinsam. `ordered` bestätigt Aufträge, `completed` Fertigbau. Develop/Completion verwendet das bestehende Zeitbudget. |
| Vertikale Treppen-/Plattform-/Lagerpiloten | Alle Teile gemeinsam, anschließend Beobachtung. Kein eigenes Zeitbudget im einfachen Start. |
| Neue `start_building_batch`-Journale (Schema 2) | Gemeinsame regionale Planung aller Ziele samt Zugang, ein nativer Strukturauftrag. Alternative Regionen müssen die ganze Gruppe aufnehmen. |
| Alte Chargenjournale (Schema 1) | Kompatible Fortsetzung des alten Ablaufs; keine automatische Umdeutung bereits begonnener Eingriffe. |
| Einzelplatzierungen | Einzelaktionen bleiben Einzelaktionen. Mehrere Gebäude, Wege, Kraft-/Wasserbauteile werden als expliziter Gesamtplan eingereicht. |

Schema-2-Chargen verwenden ein gemeinsames `constructionHours`-Budget.
Fehlende globale Materialien sind keine zusätzliche MCP-Platzierungssperre.
Räumung bleibt eine vorgeschaltete Arbeitsphase. Die Mod überwacht den Bau auch
bei MCP-Trennung; `advance_building_batch` synchronisiert danach das lokale
Journal und schließt gegebenenfalls die angeforderte Lagerkonfiguration ab.
Ein verlorener Startbeleg wird ausschließlich über dieselbe Struktur-ID gelesen.

## Belege und Grenzen

`pending` = noch nicht versucht; `unconfirmed` = Versuch ohne bestätigte Wirkung;
`rejected` = konkrete Ablehnung; `construction` beziehungsweise `confirmed` =
beobachteter Auftrag; `finished` = fertig und anwendbarer Zugang beobachtet.
`completed` verlangt alle Teile fertig. Der autonome Lauf verlangt zusätzlich
bestätigte Pause. Abgewiesene Teile verhindern einen erfolgreichen Gesamtabschluss.
Material-/Arbeits-/Zugangsprobleme nach der Platzierung prüfen; Diagnose ergänzt
aktuelle Daten, während der leichte Status gespeicherte Beobachtungen liefert.
Kraftnetz, Besetzung, Versorgung und Produktion bleiben eigene Belege.

Gemeinsame Platzierung eines Wegs über unfertiger Plattform ist über den manuellen
Großprojektstart live nachgewiesen; kein Nachweis fertiger Höhennavigation.
Ein vorheriger `deferred_native_validation`-Befund allein ist kein Beweis,
dass das Spiel die gemeinsame Platzierung annimmt. Der gemeinsame Versuch muss
als konkrete native Wirkung oder Ablehnung zurückkommen. Keine erfundene
Unterstützung durch das bloße Entfernen einer bisherigen Wartebarriere.

## Menschliches Gate und begrenzter Livepilot

Keine Builds/Tests/Installation durch den Agenten. Nach dem menschlichen Gate
und `live bereit` Backend starten, Version 0.63 und aktuelle Session bestätigen.

1. Mehrteiliger ebener Plan: nach einem Go alle IDs/Objekte vorhanden, bevor ein
   ressourcenpflichtiges Ziel fertig ist; keine weiteren Dispatch-Aufrufe.
2. Materialarmer Plan: vollständige Auftragsmenge, laufender Bau statt vorschnellem
   Erfolg; anschließend echte Fertigstellung oder ehrlicher Zeitlimitabschluss.
3. Vertikaler Plan mit Stütze/oberem Weg und unabhängigen Teilen: gemeinsamer
   Versuch; native Annahme/Ablehnung einzeln belegen. Bei fehlerhaftem Kontrollfall
   nicht auf weitere Geometrien skalieren.
4. Teilfehler/Verbindungsunsicherheit: übrige Aufträge versucht, genaue Teil-ID und
   Fehler sichtbar, kein Replay. Danach Diagnose derselben ID.
5. Neue Gebäudecharge: gemeinsamer Plan und eine Struktur-ID; alter Journalpfad
   wird nicht für neue reale Aufträge gewählt.

Abschlussentscheidung: Punkt 4 ist live durch Objektverlust ohne Replay und
konkrete Vorauftragsablehnung belegt. Native Teilablehnung mitten im Durchgang
bleibt ein synthetischer Gate-Nachweis; Nutzer hat diese Grenze ausdrücklich
für 0.63 akzeptiert. Keine Gleichsetzung mit einem nativen Livefehler.
