# Einstieg

## Maßgebliche Arbeitsweise

Das Spiel ist ein austauschbares Testsystem. Entwickle und prüfe MCP-Fähigkeiten
zügig mit strukturierten Aufrufen; keine konkrete Kolonie, kein Save und kein
historischer Bauplan ist Voraussetzung.

## Technischer Stand

- Vertikaler Lagerpilot auf unveränderter 0.33.0 zusätzlich mit Treppendrehung 0
  live bestanden: alle sieben Objekte fertig, Baustellen-/Fertigzugang und
  Bestandsanschlüsse erhalten. Treppendrehung 2 ebenfalls mit sieben fertigen
  Objekten, Bau-/Fertigzugang und Bestandsanschlüssen live bestanden.
  Treppendrehung 1 ebenfalls mit sieben fertigen Objekten, Bau-/Fertigzugang,
  Bestandsanschlüssen und Replay bestanden. Alle vier Drehungen begrenzt dokumentiert.
  MCP-Nachweiskatalog unverändert mit 3. [Nachweis](docs/vertical-warehouse-pilot.md).

- MCP-only-Abfrage `inspect_building_capabilities` nach menschlichem Gate live
  bestanden: fünf Modi, strukturierter Umfang und getrennte historische Nachweise.
  Sitzungs-/Parameter-Negativkontrollen bestanden, Spielzustand unverändert.
  Keine neue Baufreigabe; Bridge unverändert 0.33.0.
  [Abnahme](docs/building-capabilities.md).

- 0.33.0 nach menschlichem Gate live bestanden: mittleres ebenes Lager mit sechs
  gedrehten Grundrisszellen, Bau-/Fertigzugang und erhaltenen Bestandsanschlüssen.
  Negativkontrollen und Aktions-ID-Replay bestanden. Ergänzend Rotation 1 mit
  drei neuen fertigen Bodenwegen und fertigem Lager live bestanden; ursprüngliche
  Anschlüsse erhalten. Drehungen 0/2 ebenfalls fertig und erreichbar; alle vier
  ebenen Drehungen dieser Vorlage begrenzt live belegt. Nächster Vorschlag:
  unterstützten Projektumfang im MCP strukturiert ausweisen.
  [Nachweis](docs/medium-warehouse-pilot.md).

- 0.32.1 konservative Baustellensperre B nach menschlichem Gate live bestanden.
  B im Ausschlussumfang geschlossen; allgemeiner Vorschau-Schutz weiter offen.
  Neue Projekte nur ohne unabhängige Baustellen; eigene Vorgänger vor weiteren
  Platzierungen fertig. E-Rotation 0/2 des kleinen ebenen Lagers live bestanden:
  Baustellen-/Fertigzugang frei und gemeinsamer Bestandsweg verbunden.
  Mehrzellige Vorlagenerweiterung inzwischen in 0.33.0 begrenzt bestanden.
  [Nachweis und Grenzen](docs/building-rotation-pilot.md).

- 0.32.0 nach menschlichem Gate für Etappe D live bestanden:
  festes Lagerprojekt über neue Treppe, drei Plattformen und zwei obere Wege.
  Alle sieben Objekte fertig, Bauarbeiter-/fertiger Lagerzugang und beide oberen
  Wege geprüft; ausgewählte Bestandsanschlüsse erhalten, keine Doppelplatzierung.
  Nachweise und Grenzen: [Vertikaler Lagerpilot](docs/vertical-warehouse-pilot.md).

- 0.31.4 nach menschlichem Test-Gate live geprüft: drei sequenzielle Lagerpiloten,
  tatsächliche Baustellenzugangszellen, neue Wege und fertige Lager. C im begrenzten
  Umfang bestanden. B-Untersuchung mit echter Builder-Negativbaseline abgeschlossen;
  Vorschau-Schutzabnahme bleibt offen. Details im Projektstand.

- Agent Bridge 0.33.0 installiert; begrenzte Nachbar-/Pfad-/Spill-Diagnose zuvor live geprüft;
  vorherige Auswahlabfrage, Plattformpilot und obere Anschlüsse bleiben vorhanden.
- Beide ursprünglichen Bauziele sind nur teilweise erreicht. Allgemeine sichere
  Baufreigabe fehlt; Baustellenabdeckung ist weiterhin offen.
- Etappe A bestanden; B nicht durch weitere Bestandsabrisse verfolgen. Echte
  Builder-Trennung ist belegt, gleichwertige Vorschau-Schutzwirkung nicht.
  Details und Grenzen in docs/construction-access-preview.md, keine längeren Wege.
  Zurückgestellter Vier-Wege-Entwurf ist separat lokal gesichert, siehe BACKLOG.md.
- Aktueller Arbeitsbranch: `codex/road-protection-pilot`.
- Nächster Schritt: eine Fähigkeit umsetzen und am menschlichen Test-Gate anhalten:
  `scripts/prepare-human-live-test.ps1` übergeben, auf `live bereit` warten und
  erst nach bestandenem Abschluss-Livetest committen.

## Orientierung

[Projektstand](PROJECT_STATE.md) · [Mission](missionsplan.md) · [Backlog](BACKLOG.md) ·
[Build, Tests und Installation](DEVELOPMENT_WORKFLOW.md) ·
[technische Übergabe](docs/session-handoff.md).

Historische Teststände und konkrete Spielweltdaten wurden bewusst entfernt.
