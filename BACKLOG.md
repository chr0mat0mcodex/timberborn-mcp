# Backlog

## 0.52.0 begrenzt live abgenommen — Flächenräumung und Gruppenbau

Zusammenhängender Hintergrundablauf: Pflanzmarkierungen entfernen, bis 64
Vegetationsziele räumen, auf echte Fertigstellung warten, frisch gemeinsam planen
und bis acht Gebäude samt Wegen autonom bauen. Eine Fläche bis 8×8 auf einer Höhe;
Rohstoffstapel ignorieren und niemals löschen. Zeitlimits, keine automatische
Wiederholung nach Unsicherheit/Neustart. Compact/Full und lokale Statusabfrage.
Menschliches Gate bestanden; 0.52.0 am 12. Oktober 2026 begrenzt live abgenommen.
Räumbudget-Stopp, autonome Zweiergruppe, Pflanzmarkierungen und Bau auf
einer Rohstoffstapel-Zelle bestätigt; Testbauten entfernt, Endpause belegt.
[Vertrag und Abnahmepilot](docs/cleared-building-groups-0.52.md).

## 0.51.0 — Gruppen-Rückwärtssuche begrenzt live abgenommen

`plan_region_buildings` erweitert Gruppen um begrenzte Rückwärtssuche und
`reservedAreas` (bis acht Ausschlussvolumen). `searchBudget` 1–32 native Vorschauen,
Standard 32; höchstens 256 Suchzustände einschließlich Kandidatenprüfungen.
`maxCandidates` 1–4 gilt pro Erweiterungszustand. Bewertung: mehr Wunschgebäude,
danach weniger neue Wege, danach weniger Zugangshilfen. Bestätigter bester
Teilplan bleibt bei Budgetende erhalten; keine globale Optimalitätsbehauptung.
Native Prüfung hält auch neue Wege/Treppen/Stützen aus den Volumen heraus.
Bestehende Wege bleiben nutzbar; Reservierungen gelten nur für den zurückgegebenen
Plan. Keine dauerhaften Siedlungsreservierungen. Compact/Full erhalten denselben
Plan und Suchstatus. Grenzen acht Gebäude/32 Teile/32×32/acht Höhen unverändert.
Nach menschlichem Gate live bestätigt: Rücksprung senkt einen Viererplan von
13 auf zehn Hilfsteile. Ziel-/Wegeausschluss und Höhenkontrolle bestanden.
Zweiergruppe samt Treppe/zwei Wegen autonom fertig: 3,65625 Spielstunden,
10,72 reale Sekunden; Zugänge und null Bestandsverluste bestätigt. Compact
61.686 → 11.944 JSON-Zeichen (80,6 % kleiner), gleicher Plan/Schlüssel/Suchstatus.
Fünf Testobjekte entfernt, keine Rohstoffe gelöscht, Endpause bestätigt.
[Vertrag und Abnahmepilot](docs/group-search-0.51.md).

Stand: 2026-10-12, nach begrenzter Liveabnahme von **0.52.0**.
Diese Liste trennt noch offene Fähigkeiten von abgeschlossenen Teilumfängen.
Vorschläge sind keine Implementierungsfreigabe. Test-Gate unverändert:
[Entwicklungsablauf](DEVELOPMENT_WORKFLOW.md).
Alle bisherigen Beobachtungen bleiben im [historischen Backlog](BACKLOG_HISTORY.md).

## Offene Entwicklung

| ID | Verbesserung | Vorhandene Grundlage und noch offener Umfang |
| --- | --- | --- |
| B01 | Bessere Standortwahl | 0.51: begrenzte Rückwärtssuche, Vergleich ganzer Pläne und Auslassen blockierender Wünsche abgenommen. Offen: alternative Zielreihenfolgen und breitere Suche; keine globale Optimalität. |
| B02 | Reservierungen und Gesamtplanung | 0.51: bis acht Ausschlussvolumen für einen Gruppenplan vorhanden. Offen: dauerhafte Reservierungen, planübergreifende Korridore und Suchabdeckung; keine globale Siedlungsoptimierung. |
| B03 | Kraft- und Höhenrouten planen | Ports, Netze, explizite Höhenpläne und Zugangshilfen existieren. Offen: allgemeine automatische Routenauswahl, Alternativstandorte und kombinierte Versorgungslayouts mit getrenntem Geometrie-/Netz-/Betriebsnachweis. |
| B04 | Flächenumwidmung in Gruppenpläne integrieren | 0.52 begrenzt live abgenommen: eine Fläche bis 8×8 → Räumung → gemeinsame Gruppe. Rohstoffstapel ignoriert, keine Löschung. Größere/mehrere Flächen bleiben offen. |
| B05 | Flächenvorprüfung und Sammelpflanzung | Konkrete blockierte Zellen/Belegungsarten vor Flächenaktionen, begrenzte Mehrrechteck-Aufträge sowie Feuchte mit tatsächlicher Arbeitsreichweite verbinden. 0.50-Belegungskontext ersetzt keine native Pflanzprüfung. |
| B06 | Projekte über Sitzungswechsel wiederaufnehmen | Wiederanschluss innerhalb derselben Spielsession funktioniert. Offen: gespeicherte Absichten nach Neuladen mit realen Objekten abgleichen; alte Aktionen nicht blind wiederholen. |
| B07 | Produktionsketten planen | Rezeptwahl, gemeinsame Einrichtung, Engpässe und Produktionsläufe existieren. Offen: Zutatenquellen, Lager/Transport, Personal und Kraft bereits vor Bau als gemeinsamen Ausbauplan berücksichtigen. |
| B08 | Weitere Produktions- und Wirkungsnachweise | Produktionsnachweis bisher für Manufactory. Weitere Produzenten und reale Effektzellen/aktive Ziele etwa des Bienenstocks separat erschließen; Reichweite ist kein Wirkungsnachweis. |
| B09 | Änderungsjournal erweitern | 0.40 erfasst Gebäude/Wege, Konfiguration und Tempo. Offen: Pflanzen, Flächen, Forschung und belegbare Herkunft; beobachtete Änderung nicht automatisch dem Menschen zuschreiben. |
| B10 | Agentenlog verbessern | Aktionsgewissheit und kompakte Fachberichte existieren. `inspect_agent_log` hat weiterhin maximal 32 von 128 Einträgen. Offen: Cursor, Filter, sichtbare Lücken, Projekt-/Laufkorrelation und aussagekräftige Effekte; kein Ersatz für das Spieländerungsjournal. |
| B11 | Agenten-UI im Spiel | Screenshot mit Kamerametadaten und „schau mal“ existieren. Offen: „Zeigen“-Button nimmt die Ansicht beim Klick auf, begrenzte Aufbewahrung und Abhol-/Fehlerstatus. Aktive Benachrichtigung ist ein separat zu definierender Umfang. |
| B12 | Weitere Effizienz und Diagnosequalität | Noch große Werkzeugantworten/-beschreibungen gezielt messen, veralteten Clientkatalog erkennbar machen, unnötige interne Reads reduzieren. Compact/Full und günstiges Warten sind bereits vorhanden. |
| B13 | Optionale Koloniebeobachtung | Geburten/Todesfälle im Intervall, Familienwohnungen, tatsächliche Wohlbefindensursachen und historisches Bauregister. Kein fortlaufender Auftrag, alle Gebäude oder eine bestimmte Bevölkerung zu erreichen. |

B01 plus begrenzte Korridore aus B02 sind mit 0.51 im beauftragten Umfang erledigt.
B04 ist als 0.52 im begrenzten Umfang live abgenommen. Mehrere oder größere Räumflächen bleiben offen.

## Offene Folgearbeiten nach Phase E

Die ursprünglichen IDs bleiben als Verweisziele erhalten; ihre alten Grenzen
sind nicht pauschal der aktuelle Funktionsumfang.

| ID | Aktueller Restumfang |
| --- | --- |
| F01 | Vollständiger Bauphasenschutz: Bestandsbaustellen, Zugänge während aller Änderungen und freie Rückwege betroffener Biber. Zugangsergänzung und begrenzte Bau-/Fertigkontrollen sind bereits abgenommen. |
| F02 | Ersatzumweg erst fertigstellen und seine Nutzbarkeit bestätigen, anschließend abhängige Bestandsänderung. Der Anschluss um ein Hindernis allein beweist diesen Ablauf nicht. |
| F03 | Teilweise erledigt: explizite 32-Teil-/acht-Höhen-Pläne, autonome Chargen und Zugangshilfen vorhanden. Allgemeine räumliche Standort-/Routensuche bleibt B01–B03. |
| F04 | Besondere Standort-, Geometrie- und Distriktfälle gezielt prüfen. Mehrere Türen nur bei belegter Spiel-API; Bauzugangszellen sind keine zusätzlichen Eingänge. |
| F05 | Repräsentative weitere Fraktionen, Karten und Spielversionen. Iron-Teeth-Rezepte sind generisch vorgesehen, aber nicht durch den Folktails-Pilot mitabgenommen. |

## Zurückgestellte Experimente

Hermes-/andere Agenten-Anbindungen sowie die damit verbundenen lokalen Startskript-
und Budgetexperimente sind auf Nutzerentscheidung vom 2026-10-12 **ferne Zukunft**.
Erst bei gezielten Tests anderer Agenten mit dem MCP wieder aufnehmen. Kein Auftrag
für den nächsten Release, keine Abnahme und keine Aufnahme experimenteller Dateien
in den Dokumentationsabschluss. Lokale Änderungen bleiben erhalten.

## Zurückgestellte Nachweise

- Große Gebäude: bis 512 native Blockeinträge und DomedGarden begrenzt live belegt.
  Offen bleiben Großgebäude ohne Eingang, native Hex-Platzierung mit isolierter
  Außenzellenkontrolle und Mine auf ungeeignetem Standort. Vorbereitung ist keine
  Liveabnahme. Kein erneuter stundenlanger Forschungsaufbau ohne Umfangsentscheidung.
  [Nachweise](docs/large-building-geometry.md).
- Allgemeiner Kraftgeometrie-/Drehungsnachweis: widersprüchliche Vorschauindikatoren
  gegenüber realer Netzverbindung gezielt prüfen, nicht aus einzelnen erfolgreichen
  Kraftpiloten auf alle Kombinationen schließen.
- Historischer Vier-Wege-Stash: nur bei konkretem Bedarf gegen heutige Umsetzung
  prüfen; kein aktiver Entwicklungsauftrag und kein ungeprüftes Wiedereinspielen.

## Erledigt im jeweils dokumentierten Umfang

| Bereich | Abschluss / Beleg |
| --- | --- |
| Gruppenpläne, gemeinsame Wege, native Kandidaten | 0.48–0.50; [effiziente Gruppenplanung](docs/efficient-group-planning.md). Die drei Folgepunkte aus 0.49 sind damit im vereinbarten Umfang erledigt. |
| Kompakte Ergebnisse, Ausführungsgewissheit, günstiges Warten | 0.45–0.47; [Aktionsberichte](docs/action-results-and-run-reports.md), [Projektberichte](docs/compact-project-reports.md), [Status/Warten](docs/lightweight-status-wait.md). |
| Rezepte, Gebäudesuche, gemeinsame Konfiguration, Produktion und Engpässe | 0.38–0.44; [Projektstand](PROJECT_STATE.md). Keine automatische Produktionskettenplanung. |
| Spieländerungsjournal | 0.40; [Vertrag](docs/recent-actions-journal.md). Erweiterungen siehe B09. |
| Autonome Baucharge, Abschluss, Diagnose und Zugangsergänzung | [Chargen](docs/autonomous-construction-batches.md), [Reparatur](docs/project-recovery.md), [Zugangsplanung](docs/automatic-access-planning.md). Kein vollständiger Navigationsschutz. |
| Waldreichweite, Stümpfe, artenbezogener Fällschutz und weniger Reads | [Waldnachweis](docs/forestry-range-protection.md). Fällschutz ist keine dauerhafte Schutzregel gegen spätere Eingriffe. |
| Screenshot und persistenter Codex-Vorschaltprozess | [Screenshot](docs/screenshot-0.36.0.md), [Lifecycle](docs/codex-mcp-supervisor.md). UI-Button bleibt B11. |

## Dokumentationsklärung

[Prüfbericht 2026-10-12](docs/documentation-audit-2026-10-12.md): korrigierte
Altstände und die anschließend getroffenen Nutzerentscheidungen. Lokale Hermes-/Budgetänderungen
nicht aus dem Versionsnamen automatisch als abgenommen behandeln.

## Folgebeobachtungen aus 0.51

- Suchbudget wirkt: 32 Vorschauen statt Greedy-Frühabschluss; Viererplan spart drei
  Hilfsteile. Das feste Tiefensuchbudget kann frühere Wünsche lange festhalten.
  Breitere Zielreihenfolge nur bei gemessenem Nutzen, keine automatische Budgeterhöhung.
- Die native Bauzugangsplanung kann für kleine Gruppen weite Umwege erzeugen.
  Gesamtplan-Kosten sichtbar lassen; Rückwärtssuche garantiert keine kürzeste Route.
- Ein zu schmales Suchfenster ergab keine Kandidaten, die größere Kontrollfläche
  funktionierte. Suchabdeckung nicht mit Reservierungsablehnung verwechseln.
- Client zeigte nach Neustart noch die 0.50-Werkzeugbeschreibung, Backend nahm
  0.51-Parameter korrekt an. Katalog-Veraltungsdiagnose bleibt B12.

## Folgebeobachtungen aus 0.52

- Compact ist im Zweierpilot 92,2 % kleiner (67.790 auf 5.268 JSON-Zeichen),
  enthält aber noch viele leere Produktionsfelder im Räumlauf. Weiter kürzen.
- Ungültige jobId wurde korrekt vor Transport abgewiesen; die allgemeine
  Fehlermeldung verschweigt jedoch die konkrete UUID-Anforderung. Diagnose verbessern.
- B12 bleibt: neue Werkzeuge wurden erst nach Codex-Neustart verfügbar,
  obwohl der Vorschaltprozess bereits den aktuellen Katalog meldete.
