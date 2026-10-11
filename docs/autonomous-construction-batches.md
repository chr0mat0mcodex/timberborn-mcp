# Autonome Bauchargen — P0 vom 2026-10-11

Folgeerweiterung nach menschlichem Gate begrenzt live abgenommen:
[expliziter Projektabschluss, Blockadediagnose und Bauzugangsreparatur](project-recovery.md).
Die nachstehenden Livebefunde beschreiben die bereits getestete Fassung.

Status: **Menschliches Gate und begrenzte Feature-Livepiloten bestanden (2026-10-11).**
Ergänzung im gemeinsamen 0.37.0-Build, keine Parallelfassung. Frühere manuelle
Stützenpiloten sind kein Nachweis dieser neuen Ausführung.

## Live-Ergebnis 2026-10-11

Nach menschlicher Bereitmeldung und Übernahme des neuen Werkzeugkatalogs:

- **Fünfteiler:** Treppe und zwei Plattformen mit einem Run gemeinsam beauftragt;
  oberer Weg und erhöhtes kleines Lager anschließend automatisch. Alle fünf
  Teile nach 34,19 realen Sekunden / 11,625 Spielstunden fertig; Pause bestätigt.
  Lagerzugang separat: nicht blockiert, nicht unzugänglich, Distriktdistanz 35.
- **Neunteiler:** sieben abhängige Wegzellen, danach Windturbine und kleines Lager
  gemeinsam. Alle neun Teile nach 27,90 Sekunden / 9,46875 Spielstunden fertig;
  Pause bestätigt. Turbine separat finished; Lagerzugang frei, Distriktdistanz 90.
  Keine Aussage zu angeschlossener Kraftversorgung oder Produktion.
- **Wegblockade:** Lager auf belegtem Hauptweg plus gültige Plattform. Vorschau
  native_invalid, elf gefährdete Verbindungen; Run mit diesem frischen ungültigen
  Plan vor jedem Auftrag als state_conflict abgewiesen. Anschließende Planung
  ohne unabhängige Baustellen weiterhin möglich.
- **Laufende Zeitlimitkontrolle:** Turbine mit abhängigem Lager und nur einer
  Spielstunde Budget. Ende nach 2,89 Sekunden: blocked/game_time_limit,
  Turbine construction, Lager pending, bestätigte Pause. Lager separat found=false.
  Keine falsche Fertigmeldung oder automatische Erweiterung des Budgets.

Für die beiden positiven Pläne zusammen: **zwei ausführende Run-Aufrufe für
14 Teile, null Advance- und null separate Simulationsaufrufe**. Dazu zwei gültige
Planaufrufe, drei verworfene Standortentwürfe und lesende Vorbereitung/Kontrollen.
Der alte Ablauf hätte allein 14 Advance-Aufträge, abschließende Bestätigungen
und getrennte Simulationsläufe benötigt. Die Standortsuche bleibt ein eigener
Aufwand; die Beschleunigung betrifft die Ausführung vorbereiteter Pläne.

Endzustand nach beauftragtem Aufräumen: alle 15 Testobjekte entfernt, darunter
acht Wegzellen, zwei Plattformen, eine Treppe, zwei Lager und beide Testturbinen.
Auch sechs entstandene Schuttstapel entfernt. Alle Testflächen separat ohne
Testobjekte oder Schutt nachgelesen; ursprüngliche Nachbargebäude und Wege erhalten.
Spiel pausiert. Das terminale Testledger blockiert weiterhin weitere
Großprojekte dieser Sitzung entsprechend der bestehenden Diagnosegrenze.

Grenzen: 32 Teile/acht belegte Ebenen nicht live ausgeschöpft. MCP-Abbruch mitten
in aktiver Charge und Autosave-Sperre in diesem Pilot nicht provoziert; dafür
keinen Live-Nachweis behaupten. Die Übernahme eines neuen Toolnamens brauchte
einmaligen Codex-Neustart; Backend- und Spielneustart allein genügten nicht.

## Agentenablauf

1. Katalog, Freischaltung, Gelände und Zugang lesen. Einen expliziten Plan mit
   allen Gebäuden, Wegen, Treppen, Stützen und temporären Bauzugängen erstellen.
   Grenzen unverändert: 32 Teile, 32×32 Grundfläche, acht belegte Höhenebenen.
2. Pausiert `plan_large_project` aufrufen. `pilotEligible` bleibt ein begrenzter
   Entwicklungspilot; `regularExecutionAllowed=false`. Bekannte negative Prüfungen
   blockieren. Geplante Stützen sind noch kein realer Untergrund.
3. Einmal `run_large_project` mit identischem `session`, `districtId`, `steps`,
   `planKey`, neuer UUID `actionId`, `mode=development_pilot` sowie `speed` (1/3/7),
   `maxGameHours` (1–672) und `maxRealSeconds` (30–86400) aufrufen. MCP-`reasoning`
   bleibt erforderlich. Bau- und Tempofreigabe müssen auf MCP und Bridge aktiv sein.
4. Die Mod beauftragt alle unabhängigen, aktuell geprüften Teile gesammelt im
   pausierten Spiel. Erst danach startet sie einen begrenzten Simulationslauf.
   Explizite `dependsOn` warten auf tatsächliche Fertigstellung ihrer Vorgänger;
   die Mod prüft und beauftragt solche Teile selbst, sobald sie bereit sind.
   Keine künstliche Kette zwischen unabhängigen Gebäuden erzeugen. Notwendige
   Stütz- und Zugangsabhängigkeiten dagegen vollständig im Plan angeben.
5. Optional `inspect_large_project` lesen. Kein `advance_large_project`, kein
   zusätzlicher Simulationslauf und kein Agentenaufruf pro Teil. Die Mod arbeitet
   auch ohne weitere Abfrage und bei unterbrochener MCP-Verbindung weiter.
6. Erst `completed` bedeutet alle Teile tatsächlich fertig, anwendbare Eingänge
   und Wege frisch beobachtet **und** Simulationspause bestätigt. Kraftleistung,
   Lieferungen, Personal und Produktion weiterhin separat prüfen.

Abhängige Aufträge werden zwischen Simulationsticks auf dem Spielthread geprüft
und gesetzt; alle temporären Vorschauen werden vor der Platzierung entfernt.
Die Fortschrittsprüfung erfolgt höchstens viermal pro realer Sekunde. Fertigstellung
löst den Folgeschritt aus; feste Spiel-/Echtzeitbudgets sind nur Abbruchgrenzen.
Keine Wartezeit pro Bauteil und keine Nachbildung des alten Agentenloops im MCP.

## Quittung und Fehler

`executionMode=batch` unterscheidet die Quittung vom unveränderten manuellen Ledger.
`partStates` enthält `pending`, `unconfirmed`, `construction`, `finished`.
`ordered` zählt erfasste Auftragsversuche einschließlich unbestätigter Wirkungen;
`finished` zählt frisch fertig beobachtete Teile. `deferredParts` nennt noch nicht
beauftragte Teile mit offenen Abhängigkeiten. `blockedPart` ist ein nullbasierter
Index oder null bei einer chargenweiten Ursache. `simulation` enthält feste
Zeitziele, verstrichene Zeit, Tempo, Laufgrund und Pausenbestätigung.

| Zustand | Aussage |
| --- | --- |
| `running` | Aufträge werden beauftragt/begleitet; kein Vollabschluss |
| `pausing` | Keine weiteren Aufträge; Stopp der Simulation noch zu bestätigen |
| `completed` | Alle Teile fertig und anwendbare Zugänge beobachtet; Pause bestätigt |
| `blocked` | Z.B. native Prüfung/Bestandszugang fehlgeschlagen oder Zeitbudget erschöpft |
| `unconfirmed` | Platzierungswirkung bzw. Bauzugang nicht sicher bestätigt; kein Replay |
| `stopped` | Nutzer/Agent hat die Charge ausdrücklich gestoppt; Objekte bleiben |
| `interrupted` | Tempoeingriff oder nicht bestätigte Pause; kein automatisches Übersteuern |

`stop_large_project` verhindert Folgeaufträge und fordert für die Charge eine
Pause an. Autosave-/Spielsperren werden nicht aufgehoben; eine ausstehende Pause
wird nach Freigabe bestätigt. Das reale Gesamtbudget wächst dabei nicht.
Ein zweiter Simulationslauf ist während des Chargenbesitzes abgelehnt; explizite
Tempowahl oder ein externes Cancel darf den Lauf unterbrechen. Die erste eigene
Charge gibt den Simulationsbesitz erst terminal frei.

Fehlender Bauzugang bzw. ausbleibende Auftragsbestätigung hat fünf Sekunden
Bestätigungsfrist (Initialisierung während einer Spielsperre ausgenommen).
Eigene noch nicht initialisierte Aufträge dürfen diese Frist durchlaufen;
vor einer weiteren Auftragswelle müssen alle vorherigen Wirkungen bestätigt
sein. Das verlangt keine Fertigstellung unabhängiger Baustellen.
Ein konkreter Fehler stoppt weitere Platzierungen; keine Umdeutung zu Erfolg,
kein automatischer Abriss und keine automatische Neuplanung. Materialmangel kann
bis zum festen Zeitbudget warten; er wird hier noch nicht als eigene Ursache
diagnostiziert. Fehlgeschlagene Kontrollfälle nicht durch neue IDs umgehen.

Eine Platzierungsabsicht wird vor dem Spielservice-Aufruf gespeichert. Derselbe
`actionId` mit identischem Plan und Budget liefert nur den Stand; ein veränderter
Plan oder ein größeres Budget wird abgelehnt. Nach Transportfehler zuerst lesen.
Ledger und Dispatcher leben in der Mod, überstehen einen MCP-Neustart, aber kein
Neuladen der Spielsitzung. Höchstens 128 manuelle/autonome Großprojekte zusammen.
Gestoppte Projekte ohne einen Auftragsversuch dürfen Platz für ein neues Projekt
freigeben; sonst bleiben die bisherigen Diagnosegrenzen bestehen.

## Menschliches Gate und anschließender Pilot

Im menschlichen Gate enthalten: Tests für 32 gemeinsame Aufträge vor Tempoänderung,
unabhängige Teile plus späteren Stützfolger, Leseabfragen ohne Aktion, Stop/Replay,
teilweise fehlgeschlagene Platzierung, Bau-/Tempogates, HTTP-Budgettransport ohne
Retry, falsche Abschlussquittungen, Zeitlimits, manuellen Tempoeingriff und
Spielsperre. Die bisherigen manuellen Ledger-Tests bleiben erhalten.

Nach erfolgreichem menschlichem Skriptlauf und `live bereit`:

- Gemischter kleiner Plan mit zwei unabhängigen Bauteilen, Treppe, Stütze,
  Windturbine und Gebäude. Begehbaren temporären Bauzugang ausdrücklich vorsehen.
  Ein `run_large_project`; keine Advance-Aufrufe. Erste Quittung muss mehrere
  Auftragsversuche und die zurückgestellten Indizes zeigen. Nachfolger muss ohne
  weiteren Schreibaufruf entstehen; Ende `completed` mit bestätigter Pause.
- Danach ein gezielter blockierter Kontrollfall (Zugang/Geometrie), getrennt von
  einem gültigen Folgebauteil. Erwartung: konkrete Ursache/Teil, keine späteren
  Aufträge, kein falsches `completed`. Bei fehlerhaftem Kontrollfall abbrechen.
- Während eines noch aktiven positiven Pilots den MCP-Wiederanschluss prüfen:
  gleiche Session/Action-ID, keine zusätzlichen Objekte und keine Budgetverlängerung.
- Externe Schreibaufrufe zählen: neu Plan + ein Run statt Start + Advance je
  Bauteil + separate Simulationsläufe. Lesende Kontrollaufrufe separat zählen.

Keine Skalierung auf den ganzen Katalog vor diesem Nachweis. Compiler/Testresultate
und praktische Zugangs-/Platzierungsverträglichkeit entstehen erst im Gate/Pilot.

## Kompatibilität

`start_large_project`/`advance_large_project` bleiben der manuelle Altmodus.
Die älteren `start_building_batch`/`advance_building_batch` suchen/räumen einzelne
Standorte und bleiben ebenfalls unverändert; sie sind **nicht** der neue
Hintergrunddispatcher. Für vorbereitete Gesamtpläne nach Abnahme diesen neuen
Run-Modus verwenden. Automatische Flächensuche/Räumung und Ergänzen eines schon
laufenden Plans sind nicht Teil dieses P0-Schritts.
