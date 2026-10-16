# Backlog

## Bauplaner und Fehlerweitergabe — begrenzt live abgeschlossen

Priorität P0 aus dem Überlebenspilot vom 17. Oktober. Menschliches Gate und
15-Teile-Baupilot bestanden; [Nachweise und Grenzen](docs/planner-repair-2026-10-17.md).
Weitere Überlebens-Learnings stehen in `docs/survival-run-2026-10-17.txt`;
Wasserdiagnose, Ereignisstopps und Transportqueue sind nicht Teil dieses Fixes.

### Neue Learnings aus dem Baupilot

- Ingame-Fortschrittstext korrigieren: `inspect_large_project` meldete 0/15,
  strukturierter Bericht dagegen 14/15 fertig. Abnahme: laufende/terminale
  Chargenberichte korrekt zählen und Ledgerbeobachtung von frischer Diagnose trennen.
- Eingabeschema und Fehlerdetails präzisieren: unbekannte Vorlage bzw. nicht-UUID
  als actionId liefern nur `invalid_argument`. Feld und zulässiges Format nennen;
  UUID-/Enum-/Zahlengrenzen müssen auch in der Client-Werkzeugdarstellung ankommen.
- Suchbudget bleibt Qualitätsgrenze: acht Vorschauen fanden zwei Gebäude,
  16 fanden alle drei mit zwölf Umweg-Wegen. Vollständigkeit und Wegkosten
  getrennt berichten; kein Nachweis einer optimalen Route.

## B15: Gesamtstruktur — 0.63 mit dokumentierter Grenze abgeschlossen

**Ursprünglich P0, Nutzerauftrag vom 16. Oktober 2026.** Abschluss am 17. Oktober
mit ausdrücklicher Nutzerfreigabe der unten dokumentierten Nachweisgrenze.
Menschliches Gate bestanden; kein offener Implementierungsblocker dieses Umfangs.
Ebener Gesamtauftrag, Zeitlimit, Reparaturabschluss und Schema-2-Gebäudecharge live
belegt; manueller Höhenstart setzt auch oberen Weg auf unfertiger Plattform sofort.
Separater Vertikalpilot nach erneutem Gate mit fünf gemeinsamen Aufträgen bestanden;
ebener Dreiteilepilot fertig, konkrete Identitätsablehnung und Objektverlust ohne
Replay geprüft. Teilablehnung mitten im Durchgang nur synthetisch im Gate belegt;
begrenzter Versionsabschluss ausdrücklich vom Nutzer freigegeben. Zusätzlicher
nativer Livefehler und vollständiger Höhenfertigbau bleiben weitere Nachweise.

Gewünschter Ablauf: **Plan machen → einmal Go → komplette Struktur als
Bauaufträge platzieren → Baufortschritt verfolgen → erfolgreich beenden, wenn
alles gebaut ist.** Der Agent beschreibt das gesamte Bauvorhaben; MCP übernimmt
die gemeinsame Platzierung und meldet konkrete Probleme zurück. Die Biber bauen
die platzierten Baustellen anschließend nach und nach. MCP wartet nicht nach
jedem einzelnen Element auf dessen Fertigstellung, bevor es das nächste setzt.

- **Ein Gesamtplan:** Gebäude, Wege, Treppen, Stützen, Kraftverbindungen und
  Wasserbau einer Struktur gemeinsam beschreiben und prüfen. Geometrie,
  Distriktanschluss, Bauarbeiterzugang, fertiger Gebäudezugang und Bauphasen
  getrennt bewerten, einschließlich der Abhängigkeiten innerhalb des Plans.
- **Ein Go für die ganze Platzierungsphase:** Alle vom Spiel zulässigen Teile
  unmittelbar als Bauaufträge einreichen. Interne API-Aufrufe dürfen technisch
  sequenziell sein; zwischen ihnen keine Baufortschrittswartezeiten, keine
  Agentenrunde und keine erneute Einzelbestätigung. Klarstellung des Nutzers:
  erst sämtliche Aufträge einreichen, DANACH prüfen; auch keine Material-,
  Zugangs- oder Initialisierungsprüfungen zwischen den Platzierungen. Noch fehlendes Baumaterial
  ist kein Grund, weitere vom Spiel erlaubte Baustellen zurückzuhalten.
- **Echte Spielgrenzen konkret zurückmelden:** Verlangt das Spiel für ein Teil
  tatsächlich eine fertiggestellte Stütze, Räumung oder andere Vorleistung,
  dies als belegte Ausnahme mit betroffenem Teil und Abhängigkeit ausweisen.
  Keine pauschale Serialisierung des gesamten Vorhabens daraus ableiten.
  Keine vorsorglichen Fertigstellungsbarrieren aus dependsOn ableiten. Erst alle
  Teile versuchen; tatsächliche native Ablehnungen danach konkret ausweisen.
  Unbestätigte Aufträge niemals automatisch wiederholen.
- **Ein Platzierungsbericht:** Pro Teil geplant / platziert / abgewiesen /
  zurückgestellt / unbestätigt mit Objektzuordnung, Koordinaten, konkretem
  nativen Fehler und erforderlicher Korrektur. Teilerfolg offen ausweisen;
  keine vorgetäuschte atomare Alles-oder-nichts-Transaktion und kein blindes
  Wiederholen unbestätigter Aufträge.
- **Ein Lauf bis zum tatsächlichen Bauabschluss:** Gesamte Charge unter einer
  Auftrags-ID beobachten. Fortschritt und Blockaden wie Materialmangel,
  fehlende Bauarbeiter oder verlorener Zugang gesammelt melden. Erfolgsende
  erst, wenn alle beauftragten Teile nachweislich fertig sind. Platzierung
  allein ist kein Abschluss. Abbruch, Zeitlimit und dauerhaft blockierte Teile
  sind gesonderte End-/Fehlerzustände mit offenem Rest, niemals Erfolg.
  Zugang, Kraftanschluss und Betrieb zusätzlich getrennt ausweisen.

**Abnahme:** Eine mehrteilige Struktur wird nach einem Go vollständig als
Baustellen sichtbar, bevor das erste Teil fertig ist; der Agent muss keine
Elemente einzeln weiterreichen. Ein Kontrollfall mit ungültigem Teil liefert
einen präzisen Teilfehler und den tatsächlich platzierten Rest. Ein Fall mit
Materialmangel bleibt nach kompletter Platzierung in Bau und endet erst nach
Nachschub/Fertigstellung erfolgreich. Native Vorleistungsabhängigkeit und
unbestätigte Platzierung jeweils separat prüfen.

Bestehende Gruppen-, Großprojekt- und Bauchargenwege auf diesen Ablauf umbauen,
statt lediglich einen weiteren Wrapper um die bisherige Fertigstellungssequenz
zu setzen. Verknüpft B02 (Gesamtplanung), B03 (Kraft/Höhen), B07 (Versorgung)
und B14 (zusammenhängende Schutzstrukturen).

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

Stand: 2026-10-16, nach begrenzter Liveabnahme von **0.62.0**.
Diese Liste trennt noch offene Fähigkeiten von abgeschlossenen Teilumfängen.
Vorschläge sind keine Implementierungsfreigabe. Test-Gate unverändert:
[Entwicklungsablauf](DEVELOPMENT_WORKFLOW.md).
Alle bisherigen Beobachtungen bleiben im [historischen Backlog](BACKLOG_HISTORY.md).

## Offene Entwicklung

| ID | Verbesserung | Vorhandene Grundlage und noch offener Umfang |
| --- | --- | --- |
| B15 — **0.63 abgeschlossen** | **Gesamtstruktur gemeinsam platzieren, dann bis Fertigstellung verfolgen** | Ein Plan, ein Go, gemeinsame Platzierungsphase und Bauabschluss. Menschliches Gate und begrenzte Liveabnahme bestanden; Teilablehnung mitten im Durchgang nur synthetisch geprüft, ausdrücklich akzeptiert. Umfang und Grenzen oben. |
| B01 | Bessere Standortwahl | 0.51: begrenzte Rückwärtssuche, Vergleich ganzer Pläne und Auslassen blockierender Wünsche abgenommen. Offen: alternative Zielreihenfolgen und breitere Suche; keine globale Optimalität. |
| B02 | Reservierungen und Gesamtplanung | 0.51: bis acht Ausschlussvolumen für einen Gruppenplan vorhanden. Offen: dauerhafte Reservierungen, planübergreifende Korridore und Suchabdeckung; keine globale Siedlungsoptimierung. |
| B03 | Kraft- und Höhenrouten planen | Ports, Netze, explizite Höhenpläne und Zugangshilfen existieren. Offen: allgemeine automatische Routenauswahl, Alternativstandorte und kombinierte Versorgungslayouts mit getrenntem Geometrie-/Netz-/Betriebsnachweis. |
| B04 | Flächenumwidmung in Gruppenpläne integrieren | 0.52 begrenzt live abgenommen: eine Fläche bis 8×8 → Räumung → gemeinsame Gruppe. Rohstoffstapel ignoriert, keine Löschung. Größere/mehrere Flächen bleiben offen. |
| B05 | Flächenvorprüfung und Sammelpflanzung | 0.53 begrenzt abgenommen: Zweiflaechenpilot, Ausschluesse und veraltete Vorschau bestanden. Wachstum/Aquakultur nicht nachgewiesen. |
| B06 | Projekte über Sitzungswechsel wiederaufnehmen | Wiederanschluss innerhalb derselben Spielsession funktioniert. Offen: gespeicherte Absichten nach Neuladen mit realen Objekten abgleichen; alte Aktionen nicht blind wiederholen. |
| B07 | Produktionsketten planen | Rezeptwahl, gemeinsame Einrichtung, Engpässe und Produktionsläufe existieren. Offen: Zutatenquellen, Lager/Transport, Personal und Kraft bereits vor Bau als gemeinsamen Ausbauplan berücksichtigen. |
| B08 | Weitere Produktions- und Wirkungsnachweise | Produktionsnachweis bisher für Manufactory. Weitere Produzenten und reale Effektzellen/aktive Ziele etwa des Bienenstocks separat erschließen; Reichweite ist kein Wirkungsnachweis. |
| B09 | Weitere Journaldaten | Pflanzen/Stuempfe und Flaechenmarkierungen mit 0.54 begrenzt live abgenommen. Forschung weiterhin offen; keine automatische Zuschreibung zum Menschen. |
| B10 | Agentenlog verbessern | 0.55: Cursor, Werkzeug-/Statusfilter und Lücken begrenzt live abgenommen. [Vertrag](docs/agent-log-pagination-0.55.md). 0.56: explizite Projekt-/Laufkorrelation mit ID-Filtern begrenzt live abgenommen; Ingame-Anzeige vom Menschen bestätigt. [Umfang](docs/agent-log-correlation-0.56.md). 0.57: Sitzungs-Dateilogs mit Save-Namen, neue Dateien beim Neuladen, vereinfachte Anzeige und automatischer Spielstart begrenzt live abgenommen. [Vertrag](docs/file-diagnostics-0.57.md). 0.58: verständliche Ergebniszeilen für Gebäudeeinstellungen, Simulation und Pflanzchargen begrenzt live abgenommen; Ingame-Lesbarkeit vom Menschen bestätigt. [Umfang](docs/agent-log-outcomes-0.58.md). 0.59: Ergebniszeilen für Bauprojekte und Bauchargen begrenzt live abgenommen (Einzelprojekt, Ein-Gebäude-Charge, Vier-Teile-Großprojekt und gezielter Großprojektstopp am Westufer); Ingame-Lesbarkeit bestätigt. Zusatzproben: Räumgruppe, Spielzeitbudgetende und separater Bauchargenstopp bestanden; Reparaturstart nach gültiger Vorschau mit state_conflict fehlgeschlagen. Neuer 0.59-Kontrollfall erfolgreich, ursprüngliche Ursache weiter offen. 0.60: falscher Planschlüssel korrekt ohne neuen Auftrag abgewiesen, gültiger Reparaturabschluss/Pause/Elternfreigabe live geprüft; zwölf Ergebniszeilen in beiden Dateilogs identisch. Ingame-Lesbarkeit vom Menschen bestätigt; [Befund und Livepilot](docs/repair-start-0.60.md). Zeitlimit-/Stopgrund im Kurztext präzisieren. [Testprotokoll](docs/agent-log-live-tests-0.59.md). [Umfang](docs/agent-log-building-outcomes-0.59.md). Weitere Auftragsfamilien bleiben offen; kein Ersatz für das Spieländerungsjournal. |
| B11 | Agenten-UI im Spiel | Screenshot mit Kamerametadaten und „schau mal“ existieren. Offen: „Zeigen“-Button nimmt die Ansicht beim Klick auf, begrenzte Aufbewahrung und Abhol-/Fehlerstatus. Aktive Benachrichtigung ist ein separat zu definierender Umfang. |
| B12 | Weitere Effizienz und Diagnosequalität | Noch große Werkzeugantworten/-beschreibungen gezielt messen, veralteten Clientkatalog erkennbar machen, unnötige interne Reads reduzieren. 0.54-Pilot: kleine Journalgruppen adaptiv darstellen; sechs Ereignisse mit vier Gruppen sparen nur 5 von 1555 page-Zeichen. Compact/Full und günstiges Warten sind bereits vorhanden. |
| B13 | Optionale Koloniebeobachtung | Geburten/Todesfälle im Intervall, Altersverteilung, bevorstehendes Erwachsenwerden und altersbedingte Personalengpässe, Familienwohnungen, tatsächliche Wohlbefindensursachen und historisches Bauregister. Prognosen mit Zeitraum, Annahmen und Unsicherheit; freie Betten beweisen keinen gesicherten Nachwuchs. Kein fortlaufender Auftrag, alle Gebäude oder eine bestimmte Bevölkerung zu erreichen. |
| B14 | Strategische Kartenübersicht und Spezialobjektsuche | Nutzerauftrag vom 2026-10-16: kartenweite, strukturierte Übersicht über Topologie, Höhenlagen, Höhlen, Wasserreservoirs und Spezialobjekte. 0.62 liefert kartenweiten Terrain-/Quellen-/Flussüberblick, priorisierte Restwassergefahren und bedingte Reservoir-Optionen; Ereignis-/Durchbruchprognosen bleiben offen. Umfang und Abnahme unten. |

### Spielbetrieb 0.61 — neue konkrete Lücken (2026-10-16)

- **Pumpenquelle fehlt in Betriebsdiagnose:** Beide Ansaugstellen hatten
  waterDepth=0, während inspect_building_operation keine Quellenblockade meldete.
  Ansaugzelle, Wasserstand, Verunreinigung und quellenseitige Betriebsbedingung
  strukturiert ausweisen; fehlende Warnung nicht als Förderfähigkeit behandeln.

- **Planschlüssel nach Zugangsergänzung:** `plan_large_project_access` lieferte
  einen gültigen Plan; direkter Start bei unveränderter Pause meldete
  `project_start_plan_changed`. Keine Baustelle entstanden. Frische explizite
  Gesamtvorschau mit denselben Schritten startete erfolgreich. Cache-/Vorschau-
  Identitäten als Ursache untersuchen, nicht ohne Beleg behaupten.
- **Gefahrenanzeige:** Unstable-Core-Alert enthält `{0} days` statt Countdown;
  Wirkungszone und gefährdete Wasser-/Wegeverbindungen fehlen. Ergänzt B14.
  Nutzerhinweis beim Versionsabschluss: Die Bombe/der instabile Kern auf dem Berg
  bleibt eine konkrete offene Gefahr. Objektposition, nativer Countdown, erwartete
  Geländeöffnung und Freisetzung hochliegenden Wassers zusammenführen. Der 0.62-
  Gefahrenkopf erkennt Restwasser, aber prognostiziert diese Explosion nicht.
  Umsetzung nach Nutzer-Go: native Radiuszellen, hypothetischer Vor/Nach-Geländevergleich
  und kompakte Kern–Faulwasser–Ziel–Schutzketten implementiert; Gate bestanden,
  kompakte Vorwarnung bei neun Kernen live belegt. Schutzquerschnitt beim Bergkern
  unvollständig, Vor-/Nach-Schwelle unverändert. Warnung samt soweit möglichen
  Eindämmungsvorschlägen gemäß Nutzerpräzisierung am 17. Oktober abgenommen;
  vollständiger Schutzbau und Ereignisvergleich sind Folgearbeiten, keine Abnahmeblocker.
  Countdown, endgültige Wirkungszone und Einsturz bleiben ungeprüft.
  Historischer Recheck vor der Erweiterung: vollständige Terrainabdeckung und alle 501 Spezialobjekte
  gelesen; neun `UnstableCore` werden nur als `special_or_unknown` geführt. Compact
  warnt vor hochliegendem kontaminiertem Restwasser, verknüpft die Kerne aber nicht
  mit einem Durchbruch. **Konkreter B14-Folgeauftrag:** native Kernzustände und
  belegbare Wirkungszellen ermitteln; Szenario vor/nach Geländeöffnung vergleichen,
  betroffenen Speicher, Kontamination, Zuflussrinne und Zielgewässer verbinden.
  Compact muss Kernposition, Auslöser/Frist (sonst ausdrücklich unbekannt), bedrohtes
  Gewässer und vorab prüfbare Schutz-/Umleitungsstellen zeigen. Ohne belegten
  Explosionsradius keine erfundene Öffnung. Abnahme: noch geschlossene Sperre,
  Faulwasser dahinter, Teich darunter; Warnung vor Auslösung, danach Routenvergleich.
  Bildorientierung ist ohne Kamerabezug keine sichere Objektzuordnung.
- **Kollisionsdiagnose:** precheck_building nennt belegte Zellen, aber keine
  verursachenden Objekt-IDs/-typen; erschwert gezielte Räumung und Standortwahl.
- **Kraftplanung:** Generator-/Verbraucher-Verbindung als explizites Planungsziel;
  `rotationMatches=false` bedeutet im belegten Laufradfall keine fehlende reale
  Verbindung. Portausrichtung, Netzverbindung und Produktion verständlich trennen.
- **Fortlaufende Simulation:** Bauchargen pausieren zwingend am Abschluss.
  Optionale ausdrücklich beauftragte Abschlussgeschwindigkeit prüfen, damit
  Spielbetrieb auf Tempo 7 keinen zusätzlichen Resume-Aufruf braucht.
- **Gemeinsamer Versorgungsstart (B07):** Wasserpumpe, Beerensammler und
  Holzfällerfahne einschließlich Wegen, Fällmarkierung, erreichbaren Quellen und
  Personal in einem Plan vorbereiten. Danach Wohnraum und Erfinder, anschließend
  Sägewerk mit Kraftversorgung. Fertige Gebäude allein reichen als Abschluss
  nicht: Förderung, Sammlung und Holzeintrag getrennt nachweisen. Vorhandene
  Gruppenplanung wiederverwenden; fehlende gemeinsame Quellen-/Personalprüfung
  ergänzen, keinen neuen Planer allein für diese Reihenfolge schaffen.

### B14 — Strategische Sicht statt isolierter Kartenausschnitte

Positive Terraforming-Optionen implementiert, Gate und begrenzter Livepilot bestanden: begrenzte 3×3-Reservoirsuche mit zwei Ebenen Absenkung, Höhlen-/Seitenwand-/Objektprüfung, geometrischem Volumen und expliziten Zufluss-/Kontaminationsblockaden. Freie Beckenformen, echte Vorratsreichweite, Nachfüll-/Abflussplanung und Bewässerungswirkung bleiben offen. B15 ist mit 0.63 im oben dokumentierten Umfang abgeschlossen.

Compact/Full-Nachtrag live geprüft: Bedrohung und Gegenmaßnahmen im Standard, vollständige Inventur/Geometrie nur in full. Gemeinsame Maßnahmen und offene Prüfpunkte ohne Wiederholung.

Nachtrag live abgenommen einschließlich Dürrefall: Gefahrenkopf über alle erreichbaren
nassen Hohlräume; Restwasser vor Quellhinweisen, Zielhöhe berücksichtigt;
priorisierte Schutzstellen mit Dachprüfung und geometrischem Nebenweg-Kontrollfall.
Weiter offen: Unstable-Core-Countdown/Wirkungszone, Durchbruch- und zeitliche
Pegel-/Geländeänderungsanalyse, Wasserbauwerkszustände und hydraulische Wirkung.


Ziel präzisiert: allgemeine Mapawareness und Flüssigkeitsübersicht, nicht nur
Flutschutz einer Zielzone. Kartenweit Quellen, Flussbetten, Tunnel, Becken,
Verzweigungen, Überläufe und Kartenabflüsse samt Verbindungen darstellen.
0.62 nach menschlichem Gate begrenzt live abgenommen: zielunabhängiger Flussbericht mit blätterbaren
Abschnitten/Verbindungen, nativen Quellen-/Wetterdaten und Vergleich zweier Scans.
Offen: tatsächliche Strömung/Wasserbauwerke, zukünftige Kontamination ohne native
Regel und verifizierter Wettercountdown. Dürrebeginn mit zehn auf null fallenden
Quellenstärken und korrekt gemeldeten Differenzen live belegt; Faulwasserflutwechsel
und spezielle Aktivierungsregeln weiterhin offen.
Der nächste fachliche Ausbau muss drei getrennte Ebenen liefern:
- **Mögliche Wege:** Geometrische Verbindungen und Schwellen, auch trockene Rinnen;
  Richtung nicht allein aus Nachbarschaft ableiten. Wasserbauwerke und deren
  tatsächliche Einstellungen berücksichtigen oder als unbekannte Barriere melden.
- **Aktueller Zustand:** Beobachtete Pegel, Wasser/Faulwasser und Mischungen mit
  gemessener Kontamination je Abschnitt; native Quellentypen und Aktivität.
  Quellentyp ist nicht automatisch der Flüssigkeitstyp im gesamten verbundenen Netz.
- **Erwartete Änderung:** Wetter-/Ereignisphase, verbleibende Zeit, belegtes
  Quellenverhalten und mögliche Ausbreitungswege zusammenführen. Bedingte Szenarien
  von Prognosen trennen; ohne Durchfluss-/Mischungsnachweis keine Ankunftszeit oder
  künftige Reinheit behaupten. Unbekannte Wetterdaten ausdrücklich ausweisen.
Abnahme: zusammenhängendes Flussnetz auch ohne Siedlungsziel, trockener Überlauf,
getrennte Höhlenstockwerke, Zusammenfluss von Wasser und Faulwasser sowie
Ereigniswechsel mit nachvollziehbarer Änderung von Quelle und Flüssigkeitszustand.

0.62 ist als erster zusammenhängender Umfang **begrenzt live abgenommen**:
Livepilot bestätigt Gesamtabdeckung bei Tempo 7 und einen verseuchten Tunnel.
Quellenhöhen-/Kronenfehler korrigiert und live nachgeprüft. Für den nächsten Ausbau:
einzelne freie Höhenintervalle gezielt abrufbar machen, vollständige Routen blättern,
Querschnitte gegen Tunneldecken prüfen und nicht platzierbare Bauhöhen explizit
kennzeichnen. Die Priorisierung gespeicherten Wassers vor trockenen Quellhinweisen ist inzwischen abgenommen.
Aktueller Umfang: fortsetzbarer Gesamtscan mit freien Höhenintervallen, Spezialobjekten, geometrischen
Zuflusskorridoren und Sperrquerschnitten. [Vertrag/Pilot](docs/strategic-map-0.62.md).
Offen bleiben insbesondere native Wasserbauwerks-/Strömungsmodellierung,
vollständige Gefahrenabdeckung statt repräsentativer Korridore, Wetterprognose,
globale Änderungsrevision, native Quellklassifikation und reale Schutzwirkung.

- Kartenweite Bestandsaufnahme mit Kartengrenzen, Höhenbereichen und expliziter Suchabdeckung; paginiert bzw. fortsetzbar, kompakte Übersicht mit gezielten Detailabfragen. Untersuchte, veraltete und unbekannte Bereiche unterscheiden. Im laufenden Spiel nutzbar, ohne lange Planungspause.
- Oberflächen und unterirdische Hohlräume getrennt darstellen: Höhlen, Überhänge, offene Schächte, Eingänge, Rampen, Engstellen und mögliche Übergänge zwischen Höhen. Zusammenhängende Regionen und Verbindungen als Topologie anbieten; geometrische Verbindung, Biberweg, Distriktanschluss und Bauarbeiterzugang getrennt belegen.
- Spezialobjekte nach nativer Kategorie/Vorlage und Position finden, etwa Wasser-/Schlechtwasserquellen, Ruinen und andere vorhandene Kartenobjekte. Unbekannte Typen sichtbar lassen; keine Namen oder Fähigkeiten erfinden. Eine lokale Abrisszielabfrage mit Kategorie `other` ersetzt diesen Index nicht.
- Wasserflächen und Reservoirs zusammenhängend erfassen: offene oder überdeckte Lage, Oberfläche/Boden, Tiefe, Verschmutzung, Ufer und mögliche Pumpenstandorte. Aktueller Wasserbestand ist keine Garantie für Dürredauer oder erreichbaren Trinkwasserbetrieb.
- Skalierung an Vegetation und großen Karten prüfen. Livebefund vom 2026-10-16: `navigation_object_limit` in der Einzelvorschau, während Projektplaner nur `state_conflict` meldeten. Globale Objektgrenzen dürfen weder unbemerkt die Abdeckung abschneiden noch den konkreten Ablehnungsgrund verbergen.
- Kleiner Abnahmepilot: offenes Wasserloch gegenüber überdecktem Wasser, zwei getrennte Höhen mit/ohne Übergang und ein Spezialobjekt außerhalb des Siedlungsumfelds. Übersicht gegen native Detaildaten prüfen; Abdeckung, Aufwand und Grenzen ausweisen, bevor auf die gesamte Karte skaliert wird. Keine Implementierungs- oder Architekturfreigabe durch diesen Backlogeintrag.

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
| Spieländerungsjournal | 0.40, Pflanzen/Flaechen mit [0.54](docs/spatial-journal-0.54.md); [Vertrag](docs/recent-actions-journal.md). Erweiterungen siehe B09. |
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
  enthält aber noch viele leere Produktionsfelder im Räumlauf. In 0.53 gekuerzt; automatischer Nachweis im bestandenen Gate.
- Ungültige jobId wurde korrekt vor Transport abgewiesen; die allgemeine
  Fehlermeldung verschweigt jedoch die konkrete UUID-Anforderung. Feste Hinweise in 0.53 implementiert und im Gate geprueft.
- B12 bleibt: neue Werkzeuge wurden erst nach Codex-Neustart verfügbar,
  obwohl der Vorschaltprozess bereits den aktuellen Katalog meldete.

## Nacharbeiten aus dem 0.53-Livepilot

Am 12. Oktober 2026 abgeschlossen: Bestaetigte native Vorschau-Ablehnung meldet
not_executed; Compact reduziert passende Markierungen auf Summen. Menschliches
Gate und gezielter Livetest bestanden; Full bleibt vollstaendig.
Weiter offen: MCP-Aufrufe sind seriell, parallele Leser werden sofort abgewiesen.
Die feste Argumentdiagnose nennt den laufenden Aufruf; keine Warteschlange.

### Pumpengeometrie im automatischen Zugangsplaner (Live 0.61)

- Reservoirpumpe wird von plan_large_project_access mit target_intersects_existing_or_planned_object abgewiesen, ohne Objekt im lokalen Baufeld. Dieselbe Pumpe ist in plan_large_project nativ gueltig; mit expliziter Treppe und Wegen auch verbunden und pilotEligible. Vermutung: unterirdische Any-Zellen werden pauschal als Hindernis behandelt. Mit nativen Supportregeln abgleichen und konkrete Zelle/Objekt/Regel ausgeben.
- Unbekannte Wegvorlage Path.Folktails ergibt im reinen Planaufruf invalid_argument mit pauschalem execution=unconfirmed ohne konkrete Ursache. Katalogvorlage ist Path. Vor Transport validieren oder native Argumentdiagnose erhalten; lesende Planung und moegliche Vorschauwirkungen von Bauauftrag unterscheiden.

### Hochwasserdiagnose und Ausweichplanung (Live 0.61)

- **Präventiver Gefährdungsscan (B14):** Wohn-/Versorgungszone als Zielgebiet wählen; zuführende Rinnen, Durchbrüche, Überlaufkanten und Quellen auch außerhalb des Siedlungsumfelds erfassen. Gefahrenpfad, Höhenprofil, Suchabdeckung und Unsicherheit ausgeben. Priorisierte Eingriffspunkte mit Bauzeit, Kosten, Zugang und möglichen Ausweichfließwegen vorschlagen; geometrisch möglicher Zufluss ist noch keine hydrologische Prognose.
- **Terraforming/Wasserbau im Early- und Midgame:** Verfügbare native Dämme, Deiche, Wassersteuerung und Geländeänderungen samt Freischaltungen systematisch erschließen. Schließen oder Umleiten gefährlicher Rinnen, Entwässerung, Rückhalt und Höhenzugänge als zusammenhängende Vorhaben planen; Bauphasen und Rückstau berücksichtigen. Reguläre Fähigkeiten von Entwicklungswerkzeugen trennen. Abnahmepilot: Zufluss zur Siedlung mit kontrollierter Ableitung sperren, Vorher-/Nachher-Pegel und neue Überläufe prüfen; keine Schutzgarantie aus bloßer Fertigstellung.
- inspect_building_operation meldet Flooded als Text, assessment.blockers aber nur fehlendes Personal/Arbeitszeit; ueberflutetes Inventar unblocked=false. Native Ueberflutung als expliziten Betriebsblocker abbilden, damit keine falsche Personalreparatur folgt.
- Globale Vorraete auch bei ueberfluteten Lagern positiv: tatsaechlich erreichbare Trinkwasser-/Nahrungsreserve getrennt ausweisen.
- Reserven nach Distrikt, Lager-/Produktionsausgabe und tatsächlichem Zugriff aufschlüsseln; unzugängliche, überflutete oder nicht trinkbare Bestände nicht als verfügbare Notreserve zählen. Verbrauchsreichweite aus beobachtetem Verbrauch mit Zeitfenster und Unsicherheit ableiten.
- Strategische Karte B14 um Flusszufluss/Abfluss, Wasseroberflaechenhoehe, Hoehenprofil, Kontaminationsfront und erreichbare hoehere Ausweichflaechen erweitern. Lokale Baugueltigkeit allein erkennt keine bevorstehende Ueberflutung.
- Hochwasser-Livebeleg: Paused kann Flooded im sichtbaren Statuslisting verdecken. Direkten nativen Flutzustand unabhaengig von sichtbaren Statusprioritaeten abfragen; fehlender Fluttext beweist keine Betriebsfaehigkeit. Wasserstandtrend an festen Messzellen statt wechselnden Regionen fuer Zu-/Abflussbewertung verwenden.
- Wetter-/Gefahrenphase, verbleibende Dauer und bekannte Vorwarnzeit strukturiert liefern; unbekannte Dauer explizit kennzeichnen. Dürre, Schlechtwasser und andere native Ereignisse unterscheiden. Keine exakte Flutprognose aus einer einzelnen Tiefenmessung behaupten.
- Evakuierung und Ausweichdistrikt als fehlende Fähigkeit prüfen: höher gelegenen erreichbaren Standort, sichere Route, Trinkwasser/Nahrung, Unterkünfte und Baupersonal gemeinsam bewerten; native Migration mit tatsächlichem Ankunftsnachweis ermöglichen. Vor Überflutung des Bauzentrums vorbereiten; ein Bauauftrag ohne verfügbare Bauarbeiter ist keine Rettung.
- Notentwässerung/Quellensteuerung: vorhandene native Bau-, Abriss- und Wassersteuerungsmöglichkeiten mit Voraussetzungen und Wirkungsnachweis erschließen. Im Livefall fehlten MCP-Eingriffe zum Öffnen eines Abflusses bzw. Abstellen der Flutquelle. Regulär erreichbare Maßnahmen von gesonderten Entwicklungs-/Geländeeingriffen unterscheiden; keine vorhandene Fähigkeit oder rechtzeitige Wirkung unterstellen.
