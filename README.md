# Timberborn MCP

Eigene Timberborn-Mod und MCP-Server für strukturierte Zustandsabfragen,
kontrollierte Bau-/Betriebsaktionen und begrenzte Simulation.

**Aktueller Stand:** Bridge 0.74.0 mit nachfolgenden MCP-Erweiterungen.
Letzter fachlicher Abschluss ist die automatische Bretterketten-Vorprüfung,
begrenzt live abgenommen; keine automatische Ausbauplanung oder Materialsperre.
[Vertrag](docs/plank-chain.md#automatische-einbindung-vor-starts).

Aktuelle Gatezahlen, Featurestand und Nachweisgrenzen stehen ausschließlich im
[Projektstand](PROJECT_STATE.md). Einstieg über [START_HERE.md](START_HERE.md);
offene Aufgaben und abgeschlossene Dokumentationspflege A01/A02 im [Backlog](BACKLOG.md).

## Frühere Abnahmen

Die folgenden Versionsabschnitte beschreiben damalige Ergebnisse; sie ersetzen nicht den aktuellen Projektstand.

**0.74.0 abgeschlossen:** Materialwarten, echter Bestandszugangsverlust und Fertigbau korrekt unterschieden; Weg wiederhergestellt und Journale freigegeben. [Livebelege](docs/releases/0.74.0.md).

**0.73.0 abgeschlossen:** Gate und Livetest für Parameterdiagnose sowie frischen Fertigbau trotz altem Zeitlimit bestanden. [Livebelege](docs/releases/0.73.0.md).

**0.72.0 abgeschlossen:** Gate, Laufzeitdiagnosen und vollständiger Plattform-Umwegpilot bestanden: 15/15 Teile fertig und zugänglich, Bestandswege erhalten, Journal freigegeben. [Livebelege](docs/releases/0.72.0.md).

**0.71.0: Gate und Umwegplatzierung bestanden, Fertigbau offen.** Oberer Treppenanschluss und seitliche Negativkontrolle bestätigt; automatischer Plattformumweg nativ gültig, 17/17 Teile platziert, keine verlorene Bestandsverbindung. [Belege und Learnings](docs/releases/0.71.0.md).

**0.70.0: Höhenkernpilot bestanden, Umweg offen:** native Vorschau registrierter Treppen-Zielwege, getrennte Anschlussbelege. Gate und bekannter Höhenbau bis Fertigzugang bestanden; erweiterter Umweg nativ abgelehnt. [Nachweisplan](docs/releases/0.70.0.md).

**0.69.0: Gate bestanden, Höhenpilot fehlgeschlagen:** native Weggraph-Prüfung für Bestandstreppen und automatisch geöffnetes MCP-Log. Beide Graphabfragen bleiben negativ, manuelle Gegenprobe gültig. Logfenster sichtbar und angehängt. [Nachweisplan](docs/releases/0.69.0.md).

**0.68.0: Gate bestanden, Höhenpilot fehlgeschlagen:** Zielgerichtete Zugangssuche und sichtbare
Bestands-/Treppenanschlüsse; 104 Höhenproben ohne bestätigten Übergang trotz gültiger manueller Gegenprobe.
[Änderungen und Nachweisplan](docs/releases/0.68.0.md).

**0.67.0: Gate bestanden, Höhen-Livepilot fehlgeschlagen.** Automatische
Suche erschöpft ihr Budget trotz gültiger manueller Verbindung. [Änderungen und Nachweisplan](docs/releases/0.67.0.md).

**0.66.0 Kernpilot begrenzt bestanden:** Uferpumpen-Terraincheck und konkrete
Simulationsstart-Ablehnungen. [Umfang und Pilot](docs/releases/0.66.0.md).

**0.65.0 Kernpilot begrenzt bestanden:** Rückstau-/Faulwasserwarnungen und native
Überflutungs-/Ansaugdiagnose für Pumpen, auch bei Gebäudepause.
[Livebelege und verbleibende Grenzen](docs/releases/0.65.0.md).

**0.64.0 begrenzt live abgenommen:** Höhenbau bis Fertigzugang,
stabile Pause und Recovery nach Rückbau mit Folgeauftrag ohne Reload.
[Nachweise und Grenzen](docs/releases/0.64.0.md).

**0.63.0 abgeschlossen:** gesamte Struktur zuerst platzieren, danach Probleme
und Fertigstellung verfolgen; kompakte Vorwarnung vor möglichen Faulwasserdurchbrüchen.
Menschliches Gate und begrenzte Liveabnahme bestanden. Teilablehnung mitten im
Platzierungsdurchgang nur synthetisch geprüft; diese Grenze ist ausdrücklich akzeptiert.
[Releaseumfang](docs/releases/0.63.0.md), [B15-Belege](docs/construction-structures-0.63.md).


Zusätzlich `analysis.terraforming`: bedingte 3×3-Reservoir-Optionen, zwei Ebenen Absenkung, Nutzen und Blockaden kompakt, Zellbelege full. Gate und begrenzter Livepilot bestanden.

Strategischer Scan: `detail="compact"` als Standard für Bedrohung und Gegenmaßnahmen; `detail="full"` für komplette Scanbelege. Gate und Livevergleich bestanden.

Gefahrenbewertung einschließlich Restwasser bei ruhenden Quellen live bestätigt: Zielscan liefert `analysis.hazards` mit priorisiertem Restwasser, Schutzquerschnitten und Dach-/Umwegprüfung.

## 0.62.0 — Karten-, Quellen- und Flussüberblick begrenzt live abgenommen

Zielunabhängiger Flussüberblick, native Quellenstärke und
Kontamination, Wetterzustände/Aktivierungsregeln sowie Vergleich zweier Scans.
Geometrische Verbindungen und beobachtete Flüssigkeit bleiben von Prognosen getrennt.

Fortsetzbarer Gesamtscan bei laufendem Spiel: Höhlen/Überhänge, Spezialobjekte,
mögliche Zuflussrinnen zur Schutzzone und konkrete Sperrquerschnitte.
Geometrische Planungshilfe, keine Flutprognose oder automatische Baufreigabe.
Menschliches Gate und Livepilot einschließlich Dürrewechsel bestanden.
Faulwasserflutprognose und Wasserbauwerkswirkung bleiben offen.
[Umfang und Nachweise](docs/strategic-map-0.62.md).

## 0.54.0 - Pflanzen- und Flaechenjournal begrenzt live abgenommen

Menschliches Gate und Abschluss-Livetest am 12. Oktober 2026 bestanden.
Pflanz-/Faellmarkierungen, reale Pflanzenwechsel und Baum-/Stumpfunterscheidung
mit ehrlicher Herkunft; Compact/Full, Filter und Cursor live bestaetigt.
Menschliche Eingriffe bleiben unknown. Neue Sitzung startet ohne Altbestands-
ereignisse; alte Session wird abgewiesen. Produktionsregression bestanden.
Grenzen: abgetastete Zustandswechsel, keine lueckenlose Ereignis-/Erntehistorie.
[Vertrag und Livebelege](docs/spatial-journal-0.54.md).


## 0.53.0 - Sammelpflanzung begrenzt live abgenommen

Menschliches Gate bestanden; nativer Pilot am 12. Oktober 2026 bestaetigt.
Bis acht Rechtecke je 8x8, gemeinsame native Vorpruefung mit Arbeitsreichweite,
Feuchte und Belegungsart. Zwei Rechtecke mit acht Zellen live geprueft:
zwei Markierungen gesetzt, vier passende belassen, fremdes Feld und Weg ausgeschlossen.
Falsche Pflanzenart/Hoehe und veraltete Vorschau abgewiesen; Zustand frisch gelesen.
Compact/Full gleiche Summen; Testmarkierungen wiederhergestellt, Endpause bestaetigt.
Nachweis gilt fuer Markierungen, nicht Wachstum; 512-Zellen-Grenze automatisiert
getestet, nicht als grosser Liveauftrag. Kleine 0.52-Nacharbeiten im Testgate geprueft.
Nacharbeiten zu Ablehnungsstatus und Compact ebenfalls live abgenommen.
Fruehere sporadische Gatefehler bleiben ohne bewiesene Ursache.
Kein allgemeiner Aquakultur- oder anderer Fraktionsnachweis.
[Vertrag und Livepilot](docs/planting-batch-0.53.md).

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

## 0.50.0 — effizientere Gruppenplanung begrenzt live abgenommen

Gemeinsame Flächendaten innerhalb eines Gruppenaufrufs, frische Pause-/Sessiongrenzen
und unveränderte native Gesamtprüfung. Frühfilter anhand nativer Belegungsverträglichkeit (BlockOccupations.Intersects);
unbekannte Profile/fehlende Geometrie bleiben nativ zu prüfen.
`readMetrics` nennt tatsächliche Reads, Cachetreffer und native Vorschauen.
Antworten nennen Belegungsarten: geplantes Gebäude mit Index/Vorlage, Bestandsgebäude,
Weg, Vegetation, Rohstoffstapel oder unbekanntes Objekt. Regionsbelegung ist Kontext,
kein erfundener Ablehnungsgrund. Compact bündelt Filterbefunde und beschränkt
Bestandsbeispiele mit total/truncated; `detail=full` enthält alle Belege.
Acht Gebäude/32 Teile bleiben. Nach menschlichem Gate: gemischte Dreiergruppe
mit vier gemeinsamen Wegen live fertig und zugänglich, Endpause bestätigt.
28 Reads eingespart, drei statt sechs native Vorschauen, Compact 71 % kürzer.
Testobjekte entfernt; Bestandswege und Rohstoffstapel erhalten.
[Vertrag und Pilot](docs/efficient-group-planning.md).

## 0.49.0 — gemeinsame regionale Gebäudegruppe begrenzt live abgenommen

`plan_region_buildings` erweitert: exklusiv zu `template` eine Liste `templates`
mit 1–8 Gebäuden, Wiederholungen erlaubt. Suche einmal je unterschiedlicher Vorlage,
gemeinsame native Vorschau nach jeder Ergänzung; Zugangshilfen und Wege gemeinsam
berechnen. Maximal vier Kandidaten je Gebäude / 32 native Vorschauen insgesamt.
`complete/partial/none` bezeichnet Planung, nicht Bauabschluss. Unveränderte
`steps` und `planKey` an `run_large_project` übergeben; fehlende Gebäude stehen
in `unplaced`. Compact standardmäßig, `detail=full` mit allen Vorschau-Belegen.
Greedy in Eingabereihenfolge, keine optimale oder vollständige Standortsuche.
32 Gesamtteile, 32×32 und acht Höhen bleiben. Nach menschlicher Bereitmeldung
Bridge 0.49.0 bestätigt: drei Bänke plus vier gemeinsame Wegstücke in einem
Startauftrag fertig und zugänglich, 3,03125 Spielstunden / 8,85 reale Sekunden.
Teilplan- und belegte Einzelflächenprobe bestanden; Testbauten/Wege entfernt,
Bestandswege und drei zurückgewonnene Rohstoffstapel erhalten, Endpause bestätigt.
[Vertrag und Pilot](docs/group-building-plans.md).

Eine eigene Timberborn-Mod und ein lokaler C#-MCP-Server ermöglichen strukturierte
Beobachtung und kontrollierte Eingriffe über reguläre Spielservices.

## Status

Aktueller dokumentierter Stand: [PROJECT_STATE.md](PROJECT_STATE.md). Die folgenden
Versionsabschnitte sind historische Teilabnahmen, keine parallel aktuellen Stände.
Offene Fähigkeiten stehen im [Backlog](BACKLOG.md).

**0.48.0 begrenzt live abgenommen:** [Regionale native Baupläne](docs/regional-building-plans.md).

**0.47.0 begrenzt live abgenommen:** [Günstige Statusabfragen und Warten](docs/lightweight-status-wait.md).

**0.46.0 begrenzt live abgenommen:** [Kompakte Bauprojektberichte](docs/compact-project-reports.md).

**0.45.0 begrenzt live abgenommen:** [Aktionsergebnisse und kompakte Laufberichte](docs/action-results-and-run-reports.md).

**0.44.0 begrenzt live abgenommen:** [Durchsatz und beobachtete Stillstandszeiten](docs/production-throughput.md).

**0.43.0 begrenzt live abgenommen:**
[gemeinsame Engpassdiagnose](docs/production-bottlenecks.md).

**0.42.0 begrenzt live abgenommen:** [gemeinsame Gebäudeeinrichtung](docs/building-configuration-batch.md).

**0.41.0 begrenzt live abgenommen:** [Produktionsnachweis für bis acht Gebäude](docs/building-production-proof.md).

**0.40.0 begrenzt live abgenommen:** [Änderungsjournal](docs/recent-actions-journal.md).

**0.39.0 begrenzt live abgenommen:** [Gebäudefilter und Sammeldiagnose](docs/building-query-batch.md).

**0.37.0 abgeschlossen:** [Releaseumfang und Grenzen](docs/releases/0.37.0.md).
**0.38.0 begrenzt live abgenommen:** [Rezeptwahl und Betriebsdiagnose](docs/recipe-operation-settings.md).

Nach menschlichem Gate begrenzt live abgenommen:
[automatische Zugangs- und Bauhilfenplanung](docs/automatic-access-planning.md).

Neu nach menschlichem Gate begrenzt live abgenommen:
[Projektabschluss ohne Ledger-Dauersperre, Blockadediagnose und Bauzugangsreparatur](docs/project-recovery.md).

Neu begrenzt live abgenommen: [autonome Bauchargen](docs/autonomous-construction-batches.md).
Ein Gesamtplan, alle unabhängigen Aufträge gemeinsam, danach Simulation bis fertig
oder blockiert; Folgeaufträge ohne Agentenaufruf pro Bauteil. Zwei Piloten mit 14
fertigen Teilen sowie Wegblockade und Zeitlimit geprüft.

0.37.0: begrenzte Livepiloten für [größere Bauvorhaben](docs/large-projects-0.37.0.md)
einschließlich realem Stützenbau bestanden. Implementierte Grenzen: 32 Bauteile
und acht belegte Höhenebenen; dazu Kraftanschlussdiagnose. Keine pauschale
Abnahme aller Bauteile oder Kraftdrehungen.

Historischer Screenshot-Release: Bridge 0.36.0. Explizite Screenshots der
Spielansicht inklusive Kameraposition/Höhe, Richtung und Bildweite sind begrenzt
live geprüft; Bilddaten werden als MCP-Bildblock mit Größenlimit ausgegeben.
[Abnahme und Grenzen](docs/screenshot-0.36.0.md).
Maßgeblich sind [Projektstand](PROJECT_STATE.md),
[Testdoku](docs/playtest-2026-10-04.md), [Werkzeugübersicht](docs/tools.md)
und [Agenten-Spielanleitung](docs/agent-playing-guide.md).

Codex während des menschlichen Build-Gates verbunden halten:
[Vorschaltprozess einmalig einrichten](docs/codex-mcp-supervisor.md)
(Gate und Stop/Start-Liveabnahme bestanden).

**Phase E abgeschlossen** mit 0.35.1 im begrenzten, live geprüften Umfang.
[Abschlussnachweise](missionsplan.md#abschluss-phase-e),
[offene Folgearbeiten](BACKLOG.md#offene-folgearbeiten-nach-phase-e).

0.35.1: budgetgerechte Routensuche live bestanden. Fünf neue Wege werden bereits
bei der Kandidatensuche verworfen, vier akzeptiert; passende spätere Vorschläge
bleiben erhalten. Bau um ein Hindernis, Bau-/Fertigzugang und Bestandskontrolle
geprüft. [Nachweis](docs/road-budget-planner.md).

0.35.0 macht den ebenen Bauprojektpiloten generisch über den aktiven Spielkatalog,
volle Geometrie und anschließbaren Eingang. Bank sowie großes 3x3-Freiluftlager
mit Anschlusswegen, Bau-/Fertigzugang, vollständiger Grundrisskontrolle,
gefährdeter Bestandsverbindung, Höhenkontrolle und Replay nach menschlichem
Gate live geprüft. Derzeit ein öffentlich gemeldeter Wegzugang;
kein vollständiger Bauphasen-Vorabnachweis oder pauschaler Katalognachweis.
[Umfang und Nachweis](docs/generic-building-project.md).

Agent Bridge 0.34.0 ergänzt das ebene Folktails-Wohnhausprojekt. Gedrehter
Vier-Zellen-Grundriss, zwei neue Anschlusswege, tatsächlicher Bau-/Fertigzugang,
Grundriss-Ablehnung, Bestandsanschlüsse und Replay live geprüft.
[Nachweis und Grenzen](docs/lodge-project-pilot.md).

Agent Bridge 0.33.1 ermöglicht bis zu vier Vertikalprojekte nacheinander pro
Sitzung. Zwei Projekte ohne Neuladen fertiggebaut, Startsperren und Replay
live geprüft; Bau-/Fertigzugang und ausgewählte Bestandsanschlüsse erhalten.
Kein paralleler Bau oder allgemeiner Vorschau-Schutz.
[Nachweis](docs/vertical-sequential-pilot.md).

`inspect_building_capabilities` weist den versions-/fraktionsgebundenen
Projektumfang, technische Grenzen und historische Live-Nachweise strukturiert aus.
Lesend live geprüft, keine allgemeine Baufreigabe.
[Nachweis](docs/building-capabilities.md).

Agent Bridge 0.33.0 erweitert den ebenen Entwicklungspilot um das mittlere
Folktails-Lager. Gedrehter Sechs-Felder-Grundriss, erreichbare Baustelle, fertiger
freier Eingang und ausgewählte Bestandswege live geprüft; zusätzlich ein fertiger
Lagerablauf mit drei tatsächlich neuen Bodenwegen. Alle vier ebenen Drehungen
dieser Vorlage sind in begrenzten Livefällen belegt. Kein allgemeiner
Vorlagen-/Wegschutz. [Nachweis und Grenzen](docs/medium-warehouse-pilot.md).

Agent Bridge 0.32.1 schließt B im konservativen Minimalumfang: Bauanfragen bei
unabhängigen offenen Baustellen werden abgelehnt; eigene Folgeplatzierungen
benötigen fertige Vorgänger. Ablehnung ohne neue Objekte und erlaubter eigener
Ablauf live geprüft. Kein allgemeiner Builder-Vorabnachweis.
[Nachweis und Grenzen](docs/construction-isolation.md).

Erster E-Schritt auf 0.32.1: fehlende Drehungen des kleinen ebenen Lagers live
geprüft, mit erreichbarer Baustelle, freiem fertigem Eingang und erhaltenem
Bestandsweg. Weitere Vorlagen und Projektgrößen bleiben offen.
[Nachweis und Grenzen](docs/building-rotation-pilot.md).

Agent Bridge 0.32.0 ergänzt einen live belegten vertikalen Lagerablauf: neue
Treppe, drei Plattformen, zwei obere Wege und kleines Lager. Bauarbeiterzugang
und fertiger Lageranschluss wurden getrennt geprüft; ausgewählte Bestands-
anschlüsse blieben erhalten. Begrenzter Entwicklungspilot, kein vollständiger
Bauphasen-Vorabnachweis. [Nachweis und Grenzen](docs/vertical-warehouse-pilot.md).

Agent Bridge 0.29.3 ergänzt einen live belegten Plattformpilot: eine Treppe,
zwei Plattformen und zwei obere Wege. Der Ablauf wartet auf fertige Träger und
setzt erst bei Pause weiter. Alle fünf Objekte wurden fertig zurückgelesen;
die vertikale Distriktanbindung beider oberer Wege ist mit der direkten
Wegzellenabfrage ebenfalls live belegt.

Agent Bridge 0.30.0 ergänzt `inspect_selection` für die aktuelle UI-Objektauswahl.
Zwei Gebäude, der Auswahlwechsel und keine Auswahl sind live verifiziert.
Damit lässt sich „das markierte Objekt“ strukturiert zuordnen; Auswahl allein
erteilt keinen Änderungsauftrag. Details und Grenzen im [Projektstand](PROJECT_STATE.md).

## Umfang

Aktueller Schwerpunkt: Gebäude bauen, ohne bestehende Wege/Zugänge zu verlieren,
und den neuen Zugang während Bau und nach Fertigstellung nachweisen. Beides ist
teilweise, noch nicht allgemein gelöst. Der historische [Etappenplan](missionsplan.md) ist im begrenzten Umfang
abgeschlossen; nächste Erweiterungen stehen im [Backlog](BACKLOG.md).

- Zustands-, Güter-, Personal-, Bau-, Forschungs- und Flächenabfragen
- Produktions-, Zugangs- und Versorgungsdiagnosen
- Kontrollierte Aktionen für Bau, Betrieb, Forschung, Entfernung und Simulation
- Sessionbindung, Aktions-IDs, Rücklesungen und fachliche Fehlercodes

MCP nutzt stdio. Die eigene Mod kommuniziert ausschließlich über authentifiziertes
Loopback-HTTP und die Hauptthread-Queue. Es gibt keine Pflichtabhängigkeit zu
Fremdmods und keine Screenshot- oder Eingabesteuerung.

## Einstieg

```powershell
# Durch den menschlichen Entwickler ausführen; beendet Timberborn hart ohne Speichern:
pwsh -NoProfile -File ./scripts/prepare-human-live-test.ps1 -TimberbornManagedDir '<Spielverzeichnis>/Timberborn_Data/Managed'
```

Weitere Informationen: [Einstieg](START_HERE.md), [Projektstand](PROJECT_STATE.md),
[Installation](docs/native-bridge-install.md), [Werkzeuge](docs/tools.md) und
[Entwicklungsablauf](DEVELOPMENT_WORKFLOW.md).
