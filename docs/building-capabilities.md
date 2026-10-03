# Strukturierter Projektumfang — Live-Nachweis

## Aktualisierung in 0.33.1, live bestanden

0.33.1-Profil mit vier gemeinsam gezählten Vertikalprojekt-IDs live gelesen;
0.33.0 bleibt bei einer. Historische Live-Drehungen des siebenstufigen Lagerpilots
werden für beide Profile auf 0..3 korrigiert. Andere Vertikalmodi behalten ihre
eigenen bisherigen Nachweisgrenzen. Allgemeine Schutzfreigaben bleiben false.
Sequenzbetrieb und Log-Kompatibilität ebenfalls live bestanden; der folgende
Abschnitt beschreibt die vorherige bestandene Abnahme.
[Neue Abnahme](vertical-sequential-pilot.md).

## Bisherige Abnahme

Stand: 2026-10-03. MCP-only-Erweiterung nach korrigiertem menschlichem Skript-Gate
live abgenommen. Agent Bridge bleibt unverändert 0.33.0; keine neue Bauaktion.

`inspect_building_capabilities(session, reasoning)` liest den Katalogkopf der
aktiven Bridge und liefert ein versioniertes Serverprofil. Ein GET für Fraktion,
Bridge-Version und Sitzung; keine Vorschau, Platzierung oder Simulationssteuerung.
Bestehende Best-Effort-MCP-Aktivitätstelemetrie bleibt wie bei allen Werkzeugen.

## Aussagen getrennt halten

- `profileState=known` nur für die ausdrücklich zugeordnete Bridge 0.33.0 und
  Folktails. Andere bekannte Backendversionen/Fraktionen liefern unknown und keine
  Modi; nicht verstandene Backendantworten weiterhin backend_incompatible.
- `modes` nennt fünf Projektmodi, zugelassene Objektvorlagen, technische Drehungen,
  obere Weganzahl, Schritt-/Bodenweg-/Suchregions-/Ledger-/Wartegrenzen.
  Drehungen beziehen sich auf das jeweilige Anfragefeld: beim Vertikalpilot auf
  die Treppe, nicht auf die daraus abgeleitete Lagerdrehung.
- `liveRotations` und `evidenceReference` sind dokumentierte begrenzte
  Projektnachweise, keine aktuelle Bauplatzbeobachtung. Nicht katalogisierte
  Abdeckung einzelner Treppenmodi wird ausdrücklich so bezeichnet.
- `serverBuildingPlacementEnabled` ist nur der MCP-Prozessschalter;
  `bridgePlacementGate=not_observed` vermeidet eine erfundene Bridge-Freigabe.
- `siteValidationPerformed=false`, `regularExecutionAllowed=false` und
  `constructionPreflightProven=false`: Diese Abfrage erteilt keine Baufreigabe.
- Aktuelle Verfügbarkeit, Freischaltung, Größe, Eingang und Kosten weiterhin
  über `inspect_build_options` lesen. ObjectTemplates sind zugelassene Vorlagen,
  keine vollständige Schrittfolge und keine Garantie aktueller Bauverfügbarkeit.

Das Profil erweitert weder Ausführung noch Gebäudekatalog. Die vertikale
Lagerfolge bleibt auf das kleine Lager beschränkt. Nachweislevel nicht zwischen
Vorlagen, Modi oder Geometrien übertragen; keine allgemeine Sicherheitszusage.

## Bestandene Abnahme nach menschlichem Skript-Gate

1. Werkzeug mit frischer Sitzung lesen: Version 0.33.0, Folktails, known,
   fünf Modi. Ebener Pilot: kleines/mittleres Lager, maximal vier Bodenwege,
   maximal 8×8, vier Projekt-IDs. Vertikales Lager: nur kleines Lager,
   sieben Schritte, zwei obere Wege, eine Projekt-ID.
2. Historische Live-Drehungen separat: ebene Lager 0..3, Plattform-/vertikaler
   Lagerpilot nur begrenzt katalogisierte Drehung 3. Keine current-site-safe-Freigabe.
3. Falsche Sitzung liefert state_conflict, unbekannte Argumente werden abgewiesen.
4. Vor/nach der Abfrage Objektzahl unverändert bei 988; Simulation weiterhin
   pausiert, Tag 84 und 17,125 Tagesstunden unverändert. Keine Bauobjekte oder
   Geschwindigkeitsänderung. Native, nicht simulierte Daten und gleiche Sitzung.

Synthetische Tests vorbereitet für Profilgrenzen, unbekannte Version/Fraktion,
abgeschalteten Server-Schalter, Schutz gegen Sicherheitsbehauptungen, strikte
Parameter, Pflichtbegründung im Schema und GET-/Sitzungsbindung. Menschlicher Lauf:
Build erfolgreich, 778 Tests bestanden, drei Live-Tests übersprungen; sechs
Gate-Varianten scheiterten an der nicht ergänzten erwarteten Werkzeugliste.
Diese Liste und die separate Opt-in-Live-Liste sind korrigiert (dort zusätzlich
das bereits vorhandene `inspect_simulation_run` ergänzt). Im ersten Lauf wurde
Paketierung/Installation nicht erreicht. Nach Korrektur meldete der Mensch das
Spiel erneut live bereit; anschließende feature-spezifische Abnahme bestanden.

Kein neuer Bau-/Sicherheitsnachweis: Diese Abnahme betrifft die lesende Auskunft
und ihre Grenzen. Allgemeiner Weg-/Builder-Vorabnachweis bleibt offen.

Nachfolgende Livefälle des unveränderten Vertikalcodes haben inzwischen alle vier
Treppendrehungen des festen kleinen Lagerprojekts begrenzt belegt, siehe
vertical-warehouse-pilot.md. Das deployte MCP-Profil bleibt bewusst der getestete
Code-Snapshot mit liveRotations=[3]; Aktualisierung erst mit neuem MCP-Gate.
