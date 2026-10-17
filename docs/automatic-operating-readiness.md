# Automatische Betriebsdiagnose — begrenzt live abgenommen

`inspect_operating_readiness_auto` wählt Bauzentrum und Materialproduzenten
automatisch. Die manuelle Variante bleibt für gezielte Gegenproben erhalten.
Nur lesend: `session` erforderlich; optional `districtId`, bis acht `buildingIds`
als Bezirks-/Produktbezug und bis acht `materials` mit `good`/`amount`.

## Auswahl und Grenzen

Native Bezirkskennungen bestimmen die Zuordnung, niemals geometrische Nähe.
Widersprüchliche Zuweisungen oder mehrere Bezirke ohne Vorgabe bleiben unklar.
Zuerst das native Bezirkszentrum, sonst eine fertig beobachtete BuildersHut im
belegten Bezirk. Ohne Bezirk werden Produzenten nur als globale Auswahl gezeigt;
keine behauptete Bauzuständigkeit. Unfertige Gebäude werden nicht als betriebliche
Produzenten gewählt. Potenzielle Vorlagen kommen aus Rezeptausgaben und natürlichen
Erntequellen; der eigentliche Betriebsbericht prüft das aktuell gewählte Rezept.

Maximal vier Gebäudeseiten (128 Treffer), acht Zentrumskandidaten, 16
Produzentenkandidaten, davon maximal sieben im Bericht. `searchComplete`,
`nextOffset`, `candidateLimitReached` und unbekannte IDs nennen Auswahlgrenzen.
Keine vollständige Lieferketten- oder Lieferwegprüfung. Ein automatischer Bericht
prüft höchstens acht Güter; weitere bleiben in `demands` und `omittedGoods` sichtbar.

Ohne Materialvorgabe: aktuelle Produkte der Bezugsgebäude, sonst Log/Plank/Gear
mit Nullbedarf. Daraus keine erfundene Baukosten-Deckungsquote ableiten.

## Integration vor Starts

Der MCP-Client setzt einen Beobachter unmittelbar vor den nativen Starttransport.
Damit werden auch interne Strukturstarts von Bauchargen/Räumgruppen und
Simulationsstarts von Fertigbauabläufen erfasst. NativeClients ohne MCP-Beobachter
bleiben unverändert. Kein neuer nativer Endpunkt und keine Bridge-Versionsänderung.

- Jeder `start_large_project`, `run_large_project` und `run_large_project_repair`:
  Bezirk aus dem Auftrag, Kosten aller tatsächlichen Schritte einschließlich
  Wege/Treppen/Stützen aus dem Definitionsgraphen. Fehlende Kostendefinitionen
  separat, keine Nullkostenbehauptung.
- `run_simulation_for` ab sechs Spielstunden und jeder `run_simulation_until`:
  Bezugsgebäude bzw. Güterziele verwenden. Bestandsziele sind ausdrücklich keine
  Baukosten. Kurze for-Läufe, Status und Cancel lösen keine Vorprüfung aus.
- Ergebnis unter dem zusätzlichen optionalen Wurzelfeld `operatingPreflight`,
  auch in Compact und bei nachfolgender nativer Ablehnung. Je MCP-Aufruf getrennt;
  alte Reports sind kein aktueller Startbeleg. Vorprüfung erfolgt vor dem POST,
  Bericht und Startbeleg kommen gemeinsam zurück. Für eine bewusste Entscheidung
  vor dem Auftrag den lesenden Auto-Reader separat nutzen.

Materialmangel, blockierte Produktion oder unklare Auswahl sind Hinweise, keine
automatische Startsperre. Native Geometrie-/Zugangs-/Session-/Aktionsprüfungen
bleiben bestehen. Diagnose-Transport-/Datenfehler liefern `unavailable`, ein
Fünf-Sekunden-Budget `budget_exhausted`; keine automatischen Wiederholungen.
Expliziter Aufrufabbruch oder belegter Session-/Katalogkonflikt bricht vor dem
Start ab. Vorprüfung beweist weder Durchsatz noch tatsächliche Lieferung.
Manuelles direktes Setzen der Spielgeschwindigkeit ist kein begrenzter Lauf und
hat keine automatische Vorprüfung. Rohlogs/Spiel-IDs werden nicht versioniert.

## Nachtrag 2026-10-18 — wartende Lodge fertiggestellt

Bei Spielzeit 20:28: Lodge unpausiert, 12/12 Holz an der Baustelle,
buildersReachable=true, einzige Baustelle, Priorität Normal. DistrictCenter
und BuildersHut jeweils 4/4 besetzt, trocken und unpausiert, aber
workingHours=false und anyJobRunning=false: Feierabend als aktuelle Blockade.
Keine dauerhafte Zugangs-/Materialblockade beobachtet.

Gezielte neue Diagnoseentscheidung: einmal bis Folgetag 10 Uhr, Tempo 7,
maximal 60 Echtzeitsekunden. Um etwa 08:23 bereits fertig; Bauzentrum wieder
workingHours=true/anyJobRunning=true. Ende nach 13,53125 Spielstunden und
39,90 Echtzeitsekunden mit bestätigter Pause. Frische Projektdiagnose: beide
Teile finished_accessible, keine verlorenen Bestandszugänge. Projekt freigegeben.
Der historische game_time_limit-Beleg blieb korrekt getrennt vom Fertigbau.
Die Verteilung von Transport-/Anmarschzeit vor dem vorherigen Feierabend ist
nicht beobachtet; Besetzung allein belegt weder Arbeitszeit noch Bauarbeit.
Keine Codeänderung erforderlich.

## Simulationspilot 2026-10-18

Menschliches Gate bestanden: 1964 Unit- und 13 Integrationstests; drei optionale
Live-Integrationstests übersprungen. Builds ohne Warnungen/Fehler.
Backend über bestehenden Supervisor gestartet, Bridge 0.74.0 bestätigt.

Einmaliger Lauf: sechs Spielstunden, Tempo 7, maximal 30 Echtzeitsekunden,
Bestandsziel 100 Bretter bei 33 verfügbaren Brettern. Automatische Auswahl fand
das native DistrictCenter und beide fertigen LumberMills im selben Bezirk;
Suche vollständig, keine ausgelassenen Kandidaten oder unbekannten Bezirke.
Vorprüfung meldete 33 Prozent Deckung, 67 fehlende Bretter und blockierte
Produzenten. Trotzdem Start bestätigt und Fortschritt bei Tempo 7 beobachtet.
Diagnosebeobachtungen 23:23:26.472–23:23:27.060 UTC, Startbeleg 23:23:27.137 UTC:
alle Diagnosewerte liegen vor dem Startbeleg.

Einzelabfragen bestätigten Vorlagen und native Bezirkszuordnung. Betriebsdaten
und Zugangsdaten aller drei Gebäude waren mit dem Vorprüfungsbericht identisch;
Personalabfrage bestätigte 42 Beschäftigte und fünf Arbeitslose. Alle drei
Gebäude trocken, Zentrum 4/4 besetzt; eine LumberMill pausiert und unbesetzt,
die andere mit vollem Ausgang und ohne Kraft. Keine falsche Zugangswarnung.

Abschluss nach genau sechs Spielstunden und 17,72 Echtzeitsekunden:
completed/production_time_limit, weiterhin 33 Bretter, Materialziel nicht erfüllt.
Pause bestätigt, Tempo 0, kein aktiver Lauf oder ausstehender Tempoauftrag.
Das belegt Start trotz Unterdeckung, keine Produktion oder Materiallieferung.

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

## Synthetischer Abnahmeumfang

Im menschlichen Gate geprüft: native Bezirke/Mehrdeutigkeit, Kosten aller Schritte und
fehlende Vorlagen, Rezept-/Erntezuordnung, Suchlimit, automatischer Bericht,
Diagnose vor POST trotz Materialunterdeckung/Diagnoseausfall, kurzer Lauf ohne
Vorprüfung, Sessionkonflikt ohne POST sowie Struktur-Startvarianten.
Agent hat keine Builds/Tests ausgeführt.

## Anleitung

Der konkrete Entscheidungs- und Wiederaufnahmeablauf steht in der
[Agentenanleitung](agent-playing-guide.md#automatische-vorprüfung-vor-längeren-läufen).
`coveragePercent` ist das rohe Bestands-/Bedarfsverhältnis, nicht auf 100 Prozent
begrenzt. Für eine gedeckelte Gesamt-Baukostendeckung jedes Gut separat begrenzen;
Überschüsse dürfen fehlende andere Güter nicht ausgleichen.
