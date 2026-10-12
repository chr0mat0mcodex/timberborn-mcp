# Backlog

Stand: 2026-10-12, nach begrenzter Liveabnahme von **0.50.0** (`fdbc42a`).
Diese Liste trennt noch offene Fähigkeiten von abgeschlossenen Teilumfängen.
Vorschläge sind keine Implementierungsfreigabe. Test-Gate unverändert:
[Entwicklungsablauf](DEVELOPMENT_WORKFLOW.md).
Alle bisherigen Beobachtungen bleiben im [historischen Backlog](BACKLOG_HISTORY.md).

## Offene Entwicklung

| ID | Verbesserung | Vorhandene Grundlage und noch offener Umfang |
| --- | --- | --- |
| B01 | Bessere Standortwahl | 0.50 teilt Flächendaten und filtert native Belegungskollisionen. Offen: begrenzte Rückwärtssuche, alternative Reihenfolgen und Vergleich ganzer Gruppenpläne statt ausschließlich Greedy-Auswahl. |
| B02 | Reservierungen und Gesamtplanung | Freizuhaltende Wege, Höhenzugänge, Kraftkorridore und spätere Gebäudegrundflächen gemeinsam führen. Dauerhafte Reservierungen und Suchabdeckung fehlen; keine globale Siedlungsoptimierung behaupten. |
| B03 | Kraft- und Höhenrouten planen | Ports, Netze, explizite Höhenpläne und Zugangshilfen existieren. Offen: allgemeine automatische Routenauswahl, Alternativstandorte und kombinierte Versorgungslayouts mit getrenntem Geometrie-/Netz-/Betriebsnachweis. |
| B04 | Flächenumwidmung in Gruppenpläne integrieren | Vegetationsbatches und Räumwartephasen existieren. Offen: Anbaufläche → Räumung → gemeinsame Gebäudegruppe als zusammenhängender Ablauf; Arbeitszeiten berücksichtigen, Rohstoffstapel erhalten. |
| B05 | Flächenvorprüfung und Sammelpflanzung | Konkrete blockierte Zellen/Belegungsarten vor Flächenaktionen, begrenzte Mehrrechteck-Aufträge sowie Feuchte mit tatsächlicher Arbeitsreichweite verbinden. 0.50-Belegungskontext ersetzt keine native Pflanzprüfung. |
| B06 | Projekte über Sitzungswechsel wiederaufnehmen | Wiederanschluss innerhalb derselben Spielsession funktioniert. Offen: gespeicherte Absichten nach Neuladen mit realen Objekten abgleichen; alte Aktionen nicht blind wiederholen. |
| B07 | Produktionsketten planen | Rezeptwahl, gemeinsame Einrichtung, Engpässe und Produktionsläufe existieren. Offen: Zutatenquellen, Lager/Transport, Personal und Kraft bereits vor Bau als gemeinsamen Ausbauplan berücksichtigen. |
| B08 | Weitere Produktions- und Wirkungsnachweise | Produktionsnachweis bisher für Manufactory. Weitere Produzenten und reale Effektzellen/aktive Ziele etwa des Bienenstocks separat erschließen; Reichweite ist kein Wirkungsnachweis. |
| B09 | Änderungsjournal erweitern | 0.40 erfasst Gebäude/Wege, Konfiguration und Tempo. Offen: Pflanzen, Flächen, Forschung und belegbare Herkunft; beobachtete Änderung nicht automatisch dem Menschen zuschreiben. |
| B10 | Agentenlog verbessern | Aktionsgewissheit und kompakte Fachberichte existieren. `inspect_agent_log` hat weiterhin maximal 32 von 128 Einträgen. Offen: Cursor, Filter, sichtbare Lücken, Projekt-/Laufkorrelation und aussagekräftige Effekte; kein Ersatz für das Spieländerungsjournal. |
| B11 | Agenten-UI im Spiel | Screenshot mit Kamerametadaten und „schau mal“ existieren. Offen: „Zeigen“-Button nimmt die Ansicht beim Klick auf, begrenzte Aufbewahrung und Abhol-/Fehlerstatus. Aktive Benachrichtigung ist ein separat zu definierender Umfang. |
| B12 | Weitere Effizienz und Diagnosequalität | Noch große Werkzeugantworten/-beschreibungen gezielt messen, veralteten Clientkatalog erkennbar machen, unnötige interne Reads reduzieren. Compact/Full und günstiges Warten sind bereits vorhanden. |
| B13 | Optionale Koloniebeobachtung | Geburten/Todesfälle im Intervall, Familienwohnungen, tatsächliche Wohlbefindensursachen und historisches Bauregister. Kein fortlaufender Auftrag, alle Gebäude oder eine bestimmte Bevölkerung zu erreichen. |

Empfehlung für das nächste abgegrenzte Paket: **B01 plus begrenzte Korridore aus B02**.
Konkrete Suchbudgets, Reservierungsdauer und Pilot vor Umsetzung festlegen.

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
