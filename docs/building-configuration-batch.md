# 0.42 — Gebäude gesammelt einrichten

Status: nach Korrektur und menschlichem Gate begrenzt live abgenommen am 2026-10-11.

## Aufruf

`configure_buildings(session, jobId, items, proofHours?, proofSpeed?, proofMaxRealSeconds?)`
richtet ein bis acht unterschiedliche fertige Manufakturen gemeinsam ein.
Jedes Element enthält `id`, `recipe`, `expectedRecipe`, `desiredWorkers`,
`expectedDesiredWorkers`, `paused`, `expectedPaused`. Alle Felder sind ausdrücklich
anzugeben. Unterstützte Sollbesetzung: 1 bis Gebäudemaximum; zum Deaktivieren
paused=true verwenden. Null wird vor jedem Setter als staffing_out_of_range
abgewiesen (auch bei sonst zulässigen anderen Zielen). Unveränderte Werte sind erlaubte No-ops. Rezepte stammen aus den nativen
ProductionRecipes des Gebäudes, ohne Fraktions- oder Rezept-Whitelist.

Voraussetzungen: aktuelle Sitzung, Simulation pausiert, kein aktiver Simulationslauf
oder reservierte Baucharge; Manufactory, Workplace und PausableBuilding vorhanden.
Schreibzugriff verlangt gemeinsam Gebäudeeinstellungen, Personal- und Tempofreigabe,
auch ohne optionalen Produktionslauf. Die Einzelwerkzeuge bleiben unverändert.

Alle Gebäude werden vor der ersten Mutation auf Existenz, Fertigstellung, Rezept,
Personalgrenze, Pausierbarkeit und erwartete Ausgangswerte geprüft. Scheitert eine
Vorprüfung, wird kein Gebäude verändert. Anschließend je Gebäude Rezept, Sollpersonal
und Pause/Resume setzen, nötige Setter auslassen und unmittelbar zurücklesen.
Vor jedem Feld erneut die zuletzt bestätigte Konfiguration vergleichen.
Ein Registry-Durchlauf findet die Ziele; weitere Prüfungen lesen deren Komponenten.

## Ergebnis, Teilerfolg und Wiederanschluss

- `outcome=applied`: alle gewünschten Einstellungen rückgelesen. Kein Beleg für
  tatsächliche Besetzung, Erreichbarkeit oder Produktion.
- `rejected`: gemeinsame Vorprüfung gescheitert, keine Setter gestartet.
- `partial`: Ausführung nach Vorprüfung gestoppt; auch null Änderungen möglich,
  etwa bei einem Konflikt unmittelbar vor dem ersten Setter.
- Je Gebäude `outcome`, konkreter `code`, `observed`, `appliedFields` und
  gegebenenfalls `attemptedField`. `unconfirmed` bedeutet möglicher Teileffekt des
  genannten Setters, nicht automatisch Wirkungslosigkeit. `not_attempted` bleibt
  unangetastet. Die letzte Beobachtung kann bei Lesefehler fehlen.

Keine automatische Rücknahme oder Wiederholung. Pro Auftrag neue UUID als jobId,
maximal 128 reservierte Aufträge pro Sitzung. Bereits verwendete ID wird abgewiesen.
`inspect_building_configuration(session, jobId)` liest den eingefrorenen Bericht;
Sessionwechsel beendet dessen Lebensdauer. Bei unterbrochenem Aufruf zuerst lesen.
`configuration_not_found` liefert keinen allgemeinen Beleg gegen eine noch laufende
Anfrage; keine automatische Wiederholung bei unklarer Ausführung.

## Optionaler Produktionslauf

Die drei proof-Parameter nur gemeinsam: 0,000001–672 Spielstunden, Tempo 1/3/7,
30–86400 Echtzeitsekunden. Alle Zielgebäude müssen aktiviert und mit positivem
Sollpersonal konfiguriert werden. Der bestehende Lauf aus 0.41 startet genau einmal
nach bestätigter Einrichtung. Keine eigene Simulationssteuerung oder zweite Schleife.

`proofState`: not_requested, not_started, started oder unconfirmed.
`runId` entspricht jobId, sobald der Start versucht wurde. Bei unconfirmed nennt
`proofCode` eine bekannte Ablehnung oder proof_start_unconfirmed; Einstellungen
bleiben bestehen. `inspect_simulation_run` klärt Laufzustand und Produktionsnachweis.
started bedeutet nur, dass ein Lauf angelegt wurde, nicht dass seine Ziele erreicht
sind. Nach Abschluss gelten weiterhin echte Zyklusereignisse und bestätigte Pause.
Der Konfigurationsbericht wird durch den Lauf nicht nachträglich verändert.

## Gate und Pilot

Tests ergänzt: Achtfachauftrag, vollständig gemeinsame Vorprüfung, unbekanntes
Rezept, veraltete Erwartungen, Personalgrenze, Konflikt nach Vorprüfung,
Setter-Teilfehler, kein Folgelauf bei Teilfehler, unbestätigter Laufstart, Replay,
Historie, Parsergrenzen, Transport, unabhängige Aktionsschalter und Katalog.
Nicht vom Agenten ausgeführt; Compiler-/Testnachweis im menschlichen Gate.

Nach erfolgreichem Gate:

1. Zwei geeignete Gebäude gemeinsam konfigurieren und optionalen Produktionslauf
   starten; alle Einstellungen und native Produktionsabschlüsse samt Pause bestätigen.
2. Ungültiges Rezept in einem von zwei Einträgen: kompletter Auftrag ohne Änderung
   abgewiesen; beide Konfigurationen separat kontrollieren.
3. Zwischen Ausgangslesung und Sammelauftrag Einstellung gezielt ändern: erwarteter
   Konflikt, keine Überschreibung und keine Änderungen am anderen Gebäude.
4. Auftragshistorie nach späteren Änderungen unverändert lesen; getestete Einstellungen
   bei Bedarf mit einem neuen ausdrücklich geplanten Auftrag bereinigen.

Bei fehlerhafter Gegenprobe stoppen. Achtfachgrenze zunächst automatisch, kein
Kolonieausbau für den Test. Keine Rohstofflöschung. Commit/Push erst nach bestandenem
menschlichem Gate und Feature-Livepilot. Neue Werkzeuge können einen einmaligen
Codex-Neustart nach Backendstart benötigen; das ist kein erneuter Mod-Build.

## Livebefund und Korrektur vom 2026-10-11

Erster Dreierauftrag stoppte am ersten Erfinder: DecreaseDesiredWorkers änderte
Soll 1 bei Ziel 0 nicht. Ergebnis korrekt partial/setting_unconfirmed, Folgegebäude
not_attempted. Separate Sammelrücklesung bestätigte alle drei Gebäude unverändert,
keine Simulation gestartet. Weitere Pilotfälle gemäß Stoppkriterium zurückgestellt.

Fehler: gemeinsame Vorprüfung kannte nur MaxWorkers. Öffentliche Workplace- und
WorkplaceSpec-Metadaten bieten keine MinWorkers-/CanDecrease-Abfrage. Deshalb
unterstützt die Sammelkonfiguration ausdrücklich nur positive Sollbesetzung;
Deaktivierung über den vorhandenen Pausewert. Native Vorprüfung und Setterguard
sowie MCP-Schema angepasst, Regression für vollständig mutationsfreie Ablehnung
von Ziel 0 ergänzt. Keine private Reflection, keine neue Abhängigkeit. Das beweist
keine allgemeine Mindestbesetzung für fremde modifizierte Workplace-Komponenten.

Neuer Pilot nach Gate: Null-Gegenprobe auf Vorprüfungsebene, danach zwei Erfinder
bei Soll 1 pausieren und gemeinsam aktivieren mit Produktionsnachweis. Rezeptwechsel
separat am Grill, da Grill/Bäckerei aktuell volle Ausgänge haben. Anschließend
ungültiges Rezept und veraltete Erwartung kontrollieren. Aktuelle Einstellungen
sind unverändert; keine Testbauten oder Rohstofflöschungen.

## Abschluss-Livepilot nach Korrektur

- Drei Ziele gemeinsam eingerichtet: zwei Erfinder pausiert; Grill von
  GrilledSpadderdock auf GrilledPotato gewechselt und pausiert. Alle Sollwerte 1,
  unverändertes Personal ohne Setter. Rezept-/Pauseänderungen rückgelesen.
- Beide Erfinder in einem Folgeauftrag aktiviert, proofState=started. Bestehender
  0.41-Lauf belegte je einen SciencePoints-Zyklus; building_production_proven,
  bestätigte Pause nach 1,40625 Spielstunden / 4,086 Echtzeitsekunden. Zwischenzeitlich
  fehlende Besetzung blieb als seenBlockers sichtbar.
- Zweierauftrag mit gültiger Pauseänderung und ungültigem Bread-Rezept beim zweiten
  Erfinder vollständig rejected/unavailable_recipe, kein Setter ausgeführt.
- Zweierauftrag mit gültiger Grilländerung und veralteter Pausenerwartung aus der
  Vorbereitung vollständig rejected/state_conflict. Sammelrücklesung bestätigte
  alle betroffenen Einstellungen unverändert.
- Grill mit neuem Auftrag auf ursprüngliches Rezept/aktiv zurückgestellt und
  separat geprüft. Alte Dreierquittung bewahrt weiterhin die damaligen Werte.
  Endtempo separat 0 bestätigt, keine Testbauten oder Rohstofflöschungen.

Abnahmegrenzen: drei Ziele gemeinsam eingerichtet, zwei gemeinsam produziert.
Achtfachgrenze und erfolgreiche Personaländerungen im neuen Sammelpfad automatisch,
nicht live belegt; lokale Manufakturen haben MaxWorkers=1. Null-Grenze per Regression
im menschlichen Gate, nicht erneut per ungültigem MCP-Parameter gesendet. Keine
IronTeeth-Liveabnahme. Katalog wurde nach einmaligem Codex-Neustart sichtbar.
