# A12/N07: Laufregistrierung und Navigations-Baseline

Stand 2026-10-18: begrenzter Diagnosepilot abgeschlossen; historische
`unconfirmed`/`run_not_found`-Kette und Navigations-Baselinekonflikt nicht
reproduziert. Keine Code-, Test-, Konfigurations- oder Paketänderung.
Unveränderter abgenommener N06-Gatestand (Bridge/API 0.74.0): 2304 Unit-,
19 Integrations-, 13 Paket- und 35 Supervisor-Prüfungen bestanden, drei
optionale Livefälle übersprungen. Laufzeitkennungen und aktuelle Session bestätigt.

## Historischer Befund und heutige Einordnung

Der ältere Start meldete generisch state_conflict/unconfirmed, danach fehlte
die Lauf-ID. Später wurde ein Tempounterschied beobachtet; er beweist nicht die
Ursache des früheren Falls. Der Baselinekonflikt verschwand nach Simulation
und Entfernung einer fremden Baustelle; welche Änderung wirkte, wurde nicht
isoliert. [Herkunft](releases/0.66.0.md), [ältere Grenzen](releases/0.64.0.md).

Heute unterscheidet SimulationStartPolicy konkrete Ablehnungen vor Registrierung
und Tempobefehl. SimulationRunController registriert einen gültigen Lauf vor
der Tempoanforderung. RoadProtection.CaptureCore vergleicht Instant- und
Preview-Verbindungen derselben Zugangsstichprobe; ein Unterschied sperrt die
Planung. Diese Quellbefunde sind keine rückwirkende Ursachenfeststellung.
Vorhandene synthetische Tests sichern frühe Ablehnung, Transportfehler ohne
Retry und die Weitergabe konkreter Baselinegründe ab.

## Kleiner Livepilot

Erfolg/Stopp vorab: ein absichtlicher Tempokonflikt, ein gültiger Einzellauf
und dieselbe native Vorschau vor/nach dem Lauf. Bei unbestätigter Wirkung nur
dieselbe ID lesen, bei unerklärtem Baselinekonflikt stoppen und Belege sichern.
Keine Fehlerserie oder künstliche Transportunterbrechung.

- Ausgang: Pause, activeRunId=null, pendingSpeed=null; Session durchgehend gleich.
- Negativkontrolle: expectedSpeed=3 bei realem Tempo 0. Start einmal abgewiesen
  mit run_start_speed_changed/not_executed, retryable=false. Einmalige Inspektion
  dieser ID liefert run_not_found/not_applicable. Frische Zustandsabfrage zeigt
  weiterhin Pause und keinen aktiven Lauf. Hier belegt die **vorherige konkrete
  Startablehnung** die Nichtausführung, nicht run_not_found allein.
- Positivkontrolle nach frischer Zustandsprüfung mit neuer ID: eine Spielstunde,
  Tempo 1, maximal 60 Echtzeitsekunden. Start bestätigt; inspect_simulation nennt
  genau diese activeRunId, inspect_simulation_run liest sie laufend. Start und
  Rückabfrage besitzen passende request/result-Laufkorrelation im Agentenlog.
- Dieselbe ID terminal: completed/target_reached, genau eine Spielstunde,
  kein Überschwingen, Pause bestätigt. Anschließend Tempo 0, activeRunId=null,
  pendingSpeed=null. Kein Start wiederholt, kein Sessionwechsel.
- Vorher/nachher explizite Einweg-Vorschau bei (33,30,4): native_valid,
  previewEntranceConnected=true, pilotEligible=true, restored=true. Jeweils
  740 geprüfte Verbindungen, 378 vorher verbunden, 304 Wegproben und keine
  verlorenen Verbindungen. Projektjournal jeweils leer; kein Bauauftrag.
  Keine vorhandenen Baustellen: deren separate Baselinefelder bleiben null.
  roadProtection.status=unknown wegen allgemeiner Abdeckungsgrenzen ist kein
  beobachteter Baselinekonflikt und keine reguläre Baufreigabe.

## Ergebnis, Nutzen und Aufwand

Die aktuelle Kette aus früher Ablehnung, Registrierung, Rückabfrage und Pause
funktioniert in diesem kleinen Fall nachvollziehbar. Kein unconfirmed, kein
Transportverlust und kein unerklärtes run_not_found; kein Baselinekonflikt.
Keine Behauptung, die historischen Ursachen seien behoben oder vollständig geklärt.
Antwortverlust, Reload während Lauf, konkurrierende Starts und vorhandene fremde
Baustellen wurden nicht live provoziert. Keine neue Injektion implementiert.

Nutzen: aktuelle Fehlerklassen sauber voneinander getrennt und Diagnoseweg
praktisch bestätigt. Aufwand klein: eine Negativkontrolle, ein kurzer Lauf,
zwei Vorschauen. Kein belegter Bedarf für Refactoring oder neue Diagnoseschichten.
Die Untersuchung endet hier; weitere Wiederholungen ohne neuen Befund hätten
geringen Erkenntniswert. N07 bleibt ein konkreter Anlass bei Wiederauftreten.

## Abzuarbeiten beim nächsten echten Vorfall

1. Nicht erneut starten. Aktuelle Session, fragliche Lauf-ID, Startantwort samt
   error.execution und frisches inspect_simulation mit activeRunId/pendingSpeed
   sichern. Wechsel der Session ausdrücklich berücksichtigen.
2. Genau dieselbe ID lesen und korrelierte Start-/Lesebelege prüfen. Nach
   unconfirmed beweist run_not_found weder ausgebliebene Wirkung noch einen
   sicheren Wiederholungsauftrag. Nicht auf eine neue ID ausweichen.
3. Bei Navigationskonflikt vor Simulation, Abriss oder weiteren Vorschauen den
   konkreten Fehlercode, Session und vorhandene Baustellen festhalten. Instant-
   und Preview-Navigation derselben Ziele getrennt untersuchen; nur einen
   relevanten Faktor ändern, falls das zur Ursachenklärung erforderlich ist.
4. Fehlende Belege als unbekannt belassen. Rohlogs/Spiel-IDs bleiben lokal;
   ins Projekt nur Ursache oder ausdrücklich nicht reproduzierter Umfang.
