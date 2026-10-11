# Regionale Baupläne — 0.48.0

Status: menschliches Build-/Test-Gate ausdrücklich bestanden; gezielter
Dreifall-Livepilot mit 0.48.0 am 2026-10-12 bestanden. Umfang und Grenzen unten.

## Vertrag

plan_region_buildings verwendet dieselben flachen Regionsparameter wie
survey_region: session, districtId, template, x/y/z, width/height bis 16.
maxCandidates begrenzt die anschließenden nativen Prüfungen auf 1–4, Standard 3.
maxHeights/maxWindows begrenzen weiterhin die Suche; planBuildings muss true
sein und workBuildingId darf nicht gesetzt sein. Nur pausiert, Bauschalter nötig.
Das Werkzeug erzeugt temporäre native Vorschauobjekte, keine Bauaufträge.
Deshalb bleibt survey_region ein separates rein lesendes Werkzeug.

Die Vorprüfung verwendet nun wie die Großbauprüfung BlockObjectSpec.GetBlocks
mit derselben Placement-/Drehungsdefinition. Exakte native Blockzellen statt
manueller Bounding-Box-Drehung; erforderlicher Boden sowie GroundOrStackable und
Stackable werden an Fundamentzellen geprüft. Boden und fertige tatsächlich
stapelbare Blöcke zählen als beobachtete Auflage gemäß Regel. Native gemeinsame
Prüfung bleibt maßgeblich, insbesondere für spezielle Standortregeln.

BuildingPlan liefert rejectionReasons als Zähler und maximal vier rejectedSites
mit Ankerposition, Drehung und Gründen je Fenster/Drehung. Beispiele:
required_ground_missing, required_support_missing, required_stackable_missing,
object_intersection, terrain_intersection, footprint_or_entrance_outside_search,
no_ground_route_within_road_budget. Mehrere Gründe können zu einem Standort
gehören; Zähler sind keine disjunkten Flächengrößen. Ältere Antworten ohne diese
Felder bedeuten unbekannte Diagnoseabdeckung.

## Auswahl und Ergebnisse

Die besten Suchtreffer jeder untersuchten Höhe werden nach geschätzten neuen
Wegen, Höhenabstand und Koordinaten sortiert. Gleiche Position/Drehung nur einmal.
Für höchstens maxCandidates ruft die Komposition die bestehende automatische
Zugangsergänzung samt gemeinsamer nativer Vorschau auf. Kein zweiter eigener
Routingalgorithmus. Bestehende Limits: 32 Teile, 32×32, acht belegte Höhen,
512 Blockzellen pro Objekt, begrenzte Zugangssuche.

- native_preview_passed: steps, planKey, cleanupCandidateIndices und native
  Teilberichte. Ein ausgewählter Plan kann unverändert an run_large_project
  übergeben werden; session/districtId stehen im Bericht. Neue actionId und
  eigenes Laufbudget sind nötig. Start prüft den Plan erneut.
- rejected: Grund, Phase, gegebenenfalls betroffenes Teil und native Befunde.
  Kein ausführbarer planKey. Fehlende Auflage aus der ersten Suche bleibt in
  survey.coverage.plans sichtbar, selbst wenn gar kein Kandidat entsteht.
- unchecked: Kandidat wurde wegen des globalen Prüfbudgets nicht nativ geprüft.
  Kein steps/planKey, keine positive Aussage über Bau- oder Fertigzugang.

Die Varianten sind Alternativen und wurden nicht zusammen geprüft. Mehrere
alternative Pläne niemals ungeprüft als gemeinsame Charge starten. planKey der
alten geometrischen Suche ist nicht der zurückgegebene Großbau-planKey.

survey enthält Such-/Höhenabdeckung einschließlich ungeprüfter Höhen und
PlanningRows. nativeValidationCalls zählt Vorschauaufrufe; survey.NativeReads
zählt Suchlesungen, boundaryReads die zwei zusätzlichen Tempo-/Versionsprüfungen.
Das sind keine gemessenen internen Wegabfragen oder Lastwerte. Jede geprüfte
Variante hat ihren Beobachtungszeitpunkt; Gesamtbericht ist nicht atomar.
Verbindungs-/Sitzungs-/Validierungsfehler brechen ab, ohne anderen Standort als
vermeintlich harmlose Wiederholung zu versuchen. 0.48-Bridge erforderlich.

## Grenzen

Kandidatengenerierung bleibt auf kleine Suchfenster mit beobachtetem ebenem
Weganschluss und maximal vier neuen Bodenwegen beschränkt. Die Zugangsergänzung
kann danach Stützen/Treppen ergänzen, ersetzt aber keine allgemeine 3D-Standortsuche.
Vorlagen ohne Eingang, große Grundrisse außerhalb eines 8×8-Suchfensters und
völlig neue Terrassen werden nicht durch diese Erweiterung erschlossen.
Kein Kandidat bedeutet nicht unbebaubar. Kein Roden, keine gemeinsame
Siedlungsoptimierung, keine Garantie optimaler Wege. Native Vorschau belegt
keinen tatsächlich laufenden Bauarbeiter und keine Produktion. Bekannte
Bauphasenlücken bleiben ausdrücklich bestehen.

## Gate und Pilot

Vorbereitete Regressionen: globales Kandidatenbudget, Deduplizierung, Ablehnung
gegen ungeprüft, unveränderte Koordinaten/Drehung/Abhängigkeiten und Hilfsteile,
Fehler ohne Retry, Sessionwechsel, Abbruch, Bauschalter vor jedem Zugriff,
lesende Regionalsuche unverändert, Rejektionszähler/-positionen validiert,
Katalog/Annotationen in der stdio-Integration sowie Versionsprofile.

Nach menschlichem Gate drei gezielte Fälle:
1. Ebener kleiner Bauplatz mit Bestandsweg: passed und nutzbarer steps/planKey;
   genau einen kleinen Plan starten, fertigen Zugang beobachten, Testbauten
   und nur ihre Hilfswege danach entfernen, keine Rückgewinnungsrohstoffe löschen.
2. Gedrehter Mehrzeller an Geländekante: fehlende Auflage konkret ablehnen;
   dieselbe Position/Drehung in nativer Vorschau als Gegenprobe. Kein positiver
   Status allein aus freiem Ankerfeld.
3. Standort mit nötigem Zusatzweg: Wegschritte/Abhängigkeiten im Rückgabewert,
   gemeinsame native Vorschau und begrenzter tatsächlicher Bauzugangsnachweis.

Bei falscher positiver Kontrolle oder nicht wiederhergestellter Vorschau stoppen
und diagnostizieren. Keine großflächige Kandidatenserie zur Abnahme.

## Zwischenstand Livepilot 2026-10-12

Menschliches Gate ausdrücklich als vollständig bestanden bestätigt; installierte
Bridge 0.48.0 und Pause live gelesen. Gedrehter MediumTank an der historischen
Geländekante: Vorprüfung blocked mit required_support_missing und zusätzlicher
Objektkollision. Regionale Gegenprobe mit 25 Ankern: 25 abgelehnt, 20 mit fehlender
Auflage, 15 mit Kollision (überlappende Gründe), vier konkrete Beispielpositionen.
Native Gesamtvorschau desselben Standorts: native_invalid, pilotEligible=false,
restored=true, kein verlorener Anschluss. Kein Bauauftrag erteilt.

Hauptwerkzeug noch nicht live abgenommen: plan_region_buildings fehlt im
verfügbaren Werkzeugkatalog der laufenden Codex-Sitzung nach Backendwechsel.
Kein Ersatz durch separate stdio-Prozesse oder direkte HTTP-Steuerung. Nach
Aktualisierung der Clientverbindung ebene Positivprobe und Zusatzwegplan fortsetzen;
bis dahin kein Commit/Push oder vollständiger Abnahmeanspruch.

## Abschluss Livepilot 2026-10-12

Nach Codex-Neustart war das neue Werkzeug verfügbar; dieselbe Spielsitzung.
Positivsuche 6×6/eine Höhe/ein Fenster: vier Kandidaten, zwei Prüfungen bestanden,
zwei ausdrücklich unchecked. Suchlesungen 34, Planungsaufrufe vier,
Grenzprüfungen zwei; keine pauschale interne Lastmessung. Andere Geländehöhen
waren sichtbar als ungeprüft ausgewiesen. Unveränderter erster Bankplan wurde
fertig, Zugang beobachtet, automatische Pause nach 4,78125 Spielstunden.
Frische Diagnose: keine verlorenen Bestandsanschlüsse.

Ein erster schmaler Zusatzweg-Suchstreifen lag bereits an der Geländekante;
korrekte Ablehnung des äußeren Ankers. Gezielte 4×4-Geländelesung fand den ebenen
Nachbarstreifen. Dort zwei positive Alternativen, darunter Bank mit zusätzlichem
Weg und dependsOn=[1]. Diesen Plan unverändert gestartet: Weg fertig, danach
Bank im Bau mit beobachtetem Zugang, anschließend beide fertig. Automatische
Pause nach 1,84375 Spielstunden; frische Diagnose ohne verlorene Anschlüsse.

Beide Projekte freigegeben. Zwei Testbänke und ausschließlich ihr Zusatzweg
entfernt; Rücklesung bestätigt die zwei Bestandswege und keine Testgebäude.
Keine Rückgewinnungsrohstoffe gelöscht. Endzustand pausiert.

Verbesserungspunkt: regionale Antworten wiederholen ausführliche Schutzberichte
pro Alternative; kompakte Zusammenfassung mit vollständigen Details auf Wunsch
wäre sinnvoll. Nicht Teil dieser Abnahme. Der Client benötigt bei neuen
Werkzeugnamen unter Umständen einmaliges Neuladen seines MCP-Katalogs.
