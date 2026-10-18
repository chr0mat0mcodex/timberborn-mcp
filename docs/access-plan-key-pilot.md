# A12/N04: Planschlüssel nach Zugangsergänzung

Stand 2026-10-18: begrenzter Livevergleich auf dem abgenommenen N03-Stand
(Bridge/API 0.74.0, Codecommit `0afc19b`) bestanden. Keine Code-, Test-,
Konfigurations- oder Paketänderung; vorhandenes menschliches Gate gilt weiter.

## Fragestellung und Umfang

Historisch wurde ein automatisch ergänzter Plan beim Start mit
`project_start_plan_changed` abgelehnt, eine frische explizite Gesamtvorschau
danach angenommen. Der damalige konkrete Auslöser ist nicht nachgewiesen.
N04 vergleicht genau einen kleinen Lagerplan samt einem ergänzten Bodenweg.
Erfolg: identische Eingaben liefern denselben Schlüssel und werden angenommen;
eine gezielte Eingabeänderung mit altem Schlüssel wird vor Auftrag abgewiesen.
Bei unerklärtem Unterschied stoppen und diagnostizieren, kein Blindretry.

Die aktuelle Implementierung in `LargeProjectService.Plan` bindet Session,
Distrikt, kodierte Schritte, Elternprojekt und erfassten Ausgangszustand an den
Schlüssel. Laufzeit-Diagnosezähler und Vorschauidentitäten sind ausdrücklich
ausgenommen. `StartBatch` erzeugt eine frische native Gesamtvorschau und ruft
`LargeProjectStartPolicy.RequirePlan` vor der Reservierung auf. Das erklärt
die heutige Prüflogik, beweist aber keine historische Fehlerursache.

## Livebelege

- Zwei Karotten regulär geräumt, zugehörige Pflanzmarkierungen entfernt.
  Kleines Lager bei (30,31,3), Cw90; Eingang (29,31,3), Bestandsweg (28,31,3).
  Kosten einschließlich kostenfreiem Weg: drei Stämme, 260 global verfügbar
  (rund 8667 Prozent Deckung). Betriebsdiagnose vor Start durch MCP ausgeführt.
- `plan_large_project_access` ergänzt genau einen Weg und die Abhängigkeit
  des Lagers von diesem Weg. Beide Platzierungen nativ gültig und verbunden,
  Lagerarbeitszelle `road_reachable`, `restored=true`, `lostConnections=0`.
  Das ist noch kein tatsächlicher Bauarbeiterbeleg.
- Explizite Vorschau mit exakt den zurückgegebenen zwei Schritten: identischer
  `planKey`, `pilotEligible=true`, Vorschau wiederhergestellt.
- Negativkontrolle: nur Wegrotation von 0 auf 1 geändert. Auch dieser Plan
  ist nativ gültig, hat aber einen anderen Schlüssel. Start mit absichtlich
  altem Schlüssel meldet `project_start_plan_changed`, `not_executed`,
  `retryable=false`. Beide Zellen weiterhin leer; Journalzähler unverändert.
  Konkrete Ursache dieser Ablehnung ist die geänderte kodierte Rotation,
  auch wenn sie bei einem Bodenweg keinen sichtbaren Unterschied bewirkt.
- Erneute Vorschau der Originalschritte liefert weiterhin den ursprünglichen
  Schlüssel. Einmaliger Start mit dem Schlüssel aus der **automatischen**
  Vorschau angenommen; beide Aufträge bestätigt, kein Ersatzschlüssel benutzt.
- Reale Lagerbaustelle: `buildersReachable=true`, tatsächliche Zugangszellen
  beobachtet, Eingang frei und zugänglich, Distriktdistanz 4. Weg fertig.
  Vierstundenbudget endet mit `game_time_limit` und bestätigter Pause;
  Lager noch im Materialwarten, Bauarbeitsplätze außerhalb der Arbeitszeit.
  Kein beobachteter Zugangsverlust. Separates Zwölfstundenfenster für den
  Abschluss bis in die nächste Arbeitsschicht; kein Replay der Bauaufträge.
- Danach 2/2 Teile frisch fertig und zugänglich, keine fremden Baustellen oder
  verlorenen Bestandsverbindungen. Journal regulär freigegeben (`released=true`),
  Spiel pausiert. Historischer Projektstatus bleibt `blocked/game_time_limit`;
  `currentConstruction=finished_accessible` belegt den späteren Fertigbau.

## Bewertung und Grenzen

Der historische Fehler ist in diesem begrenzten Fall **nicht reproduziert**.
N04 klärt die aktuelle Übergabe positiv und die konkrete Änderung negativ;
das ist keine rückwirkende Ursachenfeststellung oder Abnahme aller Routen.
Kein belegter Bedarf für Codeänderung, Refactoring oder Lockerung der Prüfung.
Nutzen: echter Start mit dem automatisch erzeugten Schlüssel statt eines
kaschierenden Neuplans; Aufwand klein, überwiegend Aufbau und Livevergleich.
Weitere Varianten, Fehlerfälle und A12-Pakete bleiben zurückgestellt.

Bei künftigem `project_start_plan_changed` die zurückgegebenen Gesamt-Schritte
(einschließlich Reihenfolge, Drehung und Abhängigkeiten), Session/Distrikt und
Ausgangszustand vergleichen. Eine frische Vorschau dient der Diagnose; ein
neuer Schlüssel allein erklärt den alten Fehler nicht. Keine automatische
Wiederholung des Starts. Produktion oder Lagerbetrieb sind hier nicht belegt.
