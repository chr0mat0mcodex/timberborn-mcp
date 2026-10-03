# Technische Übergabe

## Stand

Agent Bridge 0.33.0 ist zuletzt nach menschlichem Skript-Gate installiert und live geprüft.
Der aktuelle Arbeitsbranch ist `codex/road-protection-pilot`. Ein begrenzter
Bauprojekt-Livefall D hat eine neue Treppe, drei Plattformen, zwei obere Wege und
ein kleines Lager vollständig gebaut. Wartephasen, tatsächlicher Baustellenzugang,
fertiger Lagerzugang, ausgewählte Bestandsanschlüsse und Idempotenz sind getrennt
live belegt. A und begrenztes C bestanden; B-Untersuchung abgeschlossen, aber
B jetzt im freigegebenen konservativen Ausschlussumfang geschlossen, allgemeiner
Vorschau-Schutz weiter offen. Details in construction-isolation.md und vertical-warehouse-pilot.md.
Erster E-Schritt auf unveränderter 0.32.1 bestanden: fehlende ebene Lagerdrehungen
0/2 mit tatsächlichem Baustellen-/Fertigzugang und erhaltenem gemeinsamem Weg.
Details und Grenzen in building-rotation-pilot.md; E nicht insgesamt abgeschlossen.

## Nächster Ablauf

Zusätzliche vertikale Treppendrehung 0 auf unveränderter 0.33.0 live bestanden:
sieben fertige Objekte, Lagerbaustelle erreichbar, fertiger Eingang frei mit
Distanz 34; unterer Bestandsweg und obere Wege verbunden, Vergleichslager bei
Distanz 25 erhalten. Replay ohne weitere Objekte. Spiel nach Zeitläufen pausiert.
Ein Vertikalprojekt pro Sitzung verbraucht; weitere Drehung braucht neue Sitzung.
Drehungen 1/2 offen. Strukturierter MCP-Nachweiskatalog unverändert mit nur 3;
späteres Profilupdate getrennt gaten. Details in vertical-warehouse-pilot.md.

MCP-only-Abfrage `inspect_building_capabilities` nach korrigiertem menschlichem
Gate live bestanden. Fünf erwartete Modi, Grenzen und Nachweise korrekt;
state_conflict bei falscher Sitzung, invalid_argument bei Zusatzparameter.
Objektzahl und pausierte Simulation unverändert. Bridge bleibt 0.33.0.
Details in building-capabilities.md. Allgemeiner Weg-/Builder-Vorabnachweis offen;
neue Umfänge erst als abgegrenzte Fähigkeit implementieren und live abnehmen.

0.33.0-Abschluss-Livetest bestanden: mittleres ebenes Lager mit sechs gedrehten
Grundrisszellen, tatsächlicher Baustellen-/Fertigzugang, erhaltene Bestandswege,
negative Grundrisskontrollen und Replay ohne neue Objekte. Keine neue
Vertikalfreigabe. Details in medium-warehouse-pilot.md.
Ergänzend auf unveränderter Installation: Rotation 1 mit drei neuen fertigen,
distriktverbundenen Bodenwegen, erreichbarer Baustelle und fertigem Lager bestanden.
Bestandsweg und Vergleichszugänge erhalten, Vier-Objekt-Replay ohne neue Objekte.
Auch Drehungen 0/2 sequenziell fertig gebaut, sechs korrekte Grundrissfelder,
Baustellen-/Fertigzugang und ursprüngliche Anschlüsse geprüft. Alle vier ebenen
Drehungen des mittleren Lagers begrenzt live belegt.

1. Etappe E aus ../missionsplan.md nur in explizit unterstützten Eigenschaften
   erweitern; Projektumfang ist inzwischen strukturiert ausweisbar,
   keine freie Höhenplanung
   oder Vollschutzbehauptung. Allgemeine B-Vorschaulücke bleibt.
2. Kandidat, Vorschau, Ausführung und Rücklesung weiterhin an dieselbe Session
   und eine neue Aktions-ID binden.
3. Bei Fehlern Ursache beheben; keine unbestätigte Aktion blind wiederholen.

## Wichtige Grenzen

- Die eigene Bridge ist die Laufzeitbasis; Legacy-Adapter sind keine Voraussetzung.
- Aktionen benötigen passende Freigaben in Mod und MCP-Prozess.
- Private Konfiguration bleibt lokal und wird nicht ausgegeben oder eingecheckt.
- Konkrete Spielstände und historische Baupläne sind nicht Teil der Übergabe.
- Allgemeiner Wegschutz ist nicht bewiesen; fertiger Lagerzugang nur in begrenzten
  C/D/E-Fällen, nicht für beliebige Vorlagen und Geometrien.
  Vier-Wege-Entwurf zurückgestellt und lokal separat erhalten, siehe BACKLOG.md.
