# Wiederaufnehmbare Bauabläufe

Status: nach erneutem menschlichem Gate und Reparatur-Livetest abgenommen.
Bridge bleibt 0.35.3, ergänzt jedoch den optionalen
Beleg `unconnectedBlockerCount`. Platzierung und native Zugangsprüfungen bleiben.

Abschluss: Lager mit zwei neuen Wegen, Anfangskonfiguration Carrot/obtain,
Bauarbeiterzugang und alle drei Objekte fertig bestätigt. Komponentenanzahl 0,
freier/zugänglicher Eingang und aktuelle Pause führen korrekt zu
finished_accessible, gegen drei separate Fachabfragen geprüft. Simulation exakt
24 Stunden ohne Überschwingen. Geändertes Budget abgelehnt; Wiederaufnahme nach
Abschluss behält Lauf-ID, Ziel und verstrichene Zeit, keine weitere Simulation.
Fünf installierte Paketdateien identisch mit menschlich erzeugtem Reparaturpaket.

Vier Ablaufaufrufe bis Abschluss, ohne Gelände-/Räumvorbereitung und zusätzliche
Abnahmekontrollen. Abschlussantwort 2185 JSON-Zeichen, drei separate Gebäude-/
Zugangs-/Konfigurationsantworten zusammen 2887 Zeichen; das Aggregat enthält
zusätzlich Auftrags-/Lauf-/Pausenbelege. Unterschiedliche Feldumfänge, daher kein
pauschaler Kompressionsfaktor. Ein MCP-Aufruf ersetzt mehrere Fachabfragen;
intern bleiben mehrere Bridge-Lesezugriffe. Tatsächlicher Prozessabbruch wurde
im Livefall nicht provoziert; Wiederaufnahme derselben IDs ist live, Transport-
unsicherheit synthetisch geprüft. Kein vollständiger Mehrgebäude-Ausbau bewiesen.

## Erster Livetest und Reparatur

Belegter Standort korrekt ohne Auftrag abgelehnt, konkrete Objektkollision
gemeldet. Einfeld-Suche auf freiem Ursprung liefert ebenfalls keinen Kandidaten,
weil der Wegeingang außerhalb liegt; Ursprungsdiagnose allein erklärt das nicht.
Nach Erweiterung um den Eingang: Lager bestellt, Konfiguration direkt an der
Baustelle bestätigt, Bauzugang true. Gesamtaufruf zunächst awaiting_order,
advance startet einmalige 24 Stunden. Wiederaufnahme erhält Lauf-ID und Ziel;
48-Stunden-Änderung wird mit state_conflict abgelehnt. Lauf endet exakt,
Pause bestätigt, Lager fertig, Eingang frei und zugänglich, Carrot/obtain erhalten.

Fehler: Aggregat meldete trotzdem budget_exhausted. Die optionale Schnittstelle
IUnconnectedBuildingBlocker fehlt an diesem Lager; daher ist ihr Status null.
Die ursprüngliche Abschlussprüfung verlangte pauschal false. Reparatur ergänzt
die tatsächlich beobachtete Anzahl solcher Komponenten. Nur explizite Anzahl 0
macht den optionalen Test nicht anwendbar; null ohne Anzahl bleibt unbewiesen.
Vorhandene Blocker müssen false melden. Freier und zugänglicher Eingang bleiben
eigene Pflichtbelege. Bei fertigem Bau ohne Zugangsnachweis künftig access_unproven
statt irreführendem budget_exhausted. Regressionen für absent/unknown/false/true
und widersprüchliche Transportdaten vorbereitet. Keine Live-Abnahme behauptet.

## Ablauf

- `develop_building_project`: bisherige Parameter von `start_building_project`
  plus `durationHours` (1–168), `speed` (1/3/7), `maxRealSeconds` (30–3600)
  und `waitSeconds` (0–20). Beide Phasen werden vor dem ersten Aufruf validiert.
  Ersten Kandidaten nativ starten, dann Bauzeit und Abschluss begleiten.
  Anfangskonfiguration bleibt direkt an der Baustelle.
- `advance_building_project`: bestehende `session`/`actionId` und dieselben vier
  Zeitparameter. Liest zuerst den Auftrag und seinen Simulationslauf. Startet
  höchstens ein Zeitfenster je Projekt, bei bestätigter Abwesenheit des Laufs,
  fertiggestellten Vorgängerwegen, bewiesenem Bauarbeiterzugang und Pause.
- `inspect_building_completion`: nur `session`/`actionId`, rein lesend. Bündelt
  Auftrag, alle Bauobjekte, Zugang, gegebenenfalls Lagerkonfiguration, Zeitlauf
  und aktuelle Spielgeschwindigkeit. Details über bisherige Werkzeuge möglich.

Aus Sitzung und Bau-ID wird eine separate Simulations-ID deterministisch
abgeleitet. Der vorhandene native Laufverlauf überlebt einen MCP-Neustart;
eine neue Spielsession verwirft ihn. Es gibt keine neue lokale Zustandsdatei,
keinen Hintergrundprozess und keine zusätzliche Abhängigkeit. Native Grenzen
(unter anderem 128 Simulations-IDs je Session) bleiben unverändert.

`develop` nur einmal senden. Danach mit den beiden Folgewerkzeugen weiterlesen.
Ein Transportfehler ist kein fehlender Auftrag. Nur explizites `run_not_found`
erlaubt den ersten Zeitstart; konkurrierende Starts derselben ID lehnt der
native Controller ab. Andere Laufparameter ergeben `budget_conflict`.
Ein bestehender oder beendeter Lauf wird niemals neu gestartet oder verlängert.
Auch bei abgebrochenem MCP-Aufruf läuft ein gestarteter Modlauf bis zu seiner
eigenen Grenze weiter. Abbruch ist über das vorhandene `cancel_simulation_run`
mit der ausgegebenen Run-ID möglich.

`running` und `awaiting_order` sind Zwischenstände. `finished_accessible`
verlangt alle Projektobjekte fertig, freien und zugänglichen Gebäudeeingang,
erhaltene angeforderte Lagerkonfiguration sowie eine frisch beobachtete Pause.
Das ist kein Produktions-, Liefer-, Personal- oder Zufriedenheitsnachweis.
`budget_exhausted`, `simulation_stopped`, fehlende Entities, Unknown-Zugang
und geänderte Konfiguration verlangen Diagnose statt automatischer Wiederholung.
Beobachtungen sind nicht atomar. Interne Statusbeobachtung ist auf höchstens
zehn Polls beschränkt; einzelne Bridge-Laufzeiten kommen zum Wartefenster hinzu.

Erfolglose Suche liefert zusätzlich eine räumliche Vorprüfung am Suchursprung:
Gründe, ausdrücklich blockierte Zellen und Eingangshindernisse. Dies erklärt
nicht die gesamte Suchregion und autorisiert keine automatische Räumung.
Keine Mehrgebäude-Warteschlange, automatische Forschung oder Ressourcenplanung.

## Gezielte Abnahme

Nach menschlichem Skriptlauf und `live bereit`:

1. Belegter Suchursprung: `not_started`, keine Mutation, konkrete Diagnose.
2. Kleines bezahlbares Lager samt Anfangskonfiguration über `develop` starten.
   `waitSeconds=0` erlaubt einen beobachtbaren Zwischenstand. Bestehende
   Baustellen-/Zugangsprüfungen müssen weiterhin greifen.
3. Über `advance`/`inspect` denselben Lauf wiederfinden. Abschluss gegen eine
   separate Gebäude-/Zugangs-/Konfigurationsabfrage vergleichen. Lauf-ID und
   ursprüngliches Ziel dürfen sich bei Wiederholung nicht ändern.
4. Geändertes Zeitbudget ablehnen; keine weitere Simulation oder Doppelbau.

Erfolg: kompletter Bauablauf mit erhaltenen Nachweisen und deutlich weniger
MCP-Einzelaufrufen; Aufrufe und Antwortumfang messen. Stopp beim ersten falschen
Kontrollfall oder fehlenden Abschlussbeleg, keine Skalierung auf viele Gebäude.
Transportabbruch, erschöpftes Budget, Unknown-/fehlende Zugänge, fehlende Objekte,
Sitzungswechsel und geänderte Konfiguration sind zusätzlich als synthetische
Tests vorbereitet. Tests werden erst im menschlichen Gate ausgeführt.
