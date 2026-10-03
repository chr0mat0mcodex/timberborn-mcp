# Technische Übergabe

## Stand

Agent Bridge 0.32.1 ist zuletzt nach menschlichem Skript-Gate installiert und live geprüft.
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

1. Etappe E aus ../missionsplan.md nur in explizit unterstützten Eigenschaften
   erweitern; nächste kleine Erweiterung eine ebene Vorlage mit mehrzelliger
   Grundfläche, keine freie Höhenplanung
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
