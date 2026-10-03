# Technische Übergabe

## Stand

Agent Bridge 0.32.0 ist zuletzt nach menschlichem Skript-Gate installiert und live geprüft.
Der aktuelle Arbeitsbranch ist `codex/road-protection-pilot`. Ein begrenzter
Bauprojekt-Livefall D hat eine neue Treppe, drei Plattformen, zwei obere Wege und
ein kleines Lager vollständig gebaut. Wartephasen, tatsächlicher Baustellenzugang,
fertiger Lagerzugang, ausgewählte Bestandsanschlüsse und Idempotenz sind getrennt
live belegt. A und begrenztes C bestanden; B-Untersuchung abgeschlossen, aber
B-Schutzabnahme weiter offen. Details in vertical-warehouse-pilot.md.

## Nächster Ablauf

1. Etappe E aus ../missionsplan.md nur in explizit unterstützten Eigenschaften
   erweitern; keine freie Höhenplanung oder Vollschutzbehauptung. B-Lücke bleibt.
2. Kandidat, Vorschau, Ausführung und Rücklesung weiterhin an dieselbe Session
   und eine neue Aktions-ID binden.
3. Bei Fehlern Ursache beheben; keine unbestätigte Aktion blind wiederholen.

## Wichtige Grenzen

- Die eigene Bridge ist die Laufzeitbasis; Legacy-Adapter sind keine Voraussetzung.
- Aktionen benötigen passende Freigaben in Mod und MCP-Prozess.
- Private Konfiguration bleibt lokal und wird nicht ausgegeben oder eingecheckt.
- Konkrete Spielstände und historische Baupläne sind nicht Teil der Übergabe.
- Allgemeiner Wegschutz ist nicht bewiesen; fertiger Lagerzugang nur in begrenzten
  C/D-Fällen, nicht für beliebige Vorlagen und Geometrien.
  Vier-Wege-Entwurf zurückgestellt und lokal separat erhalten, siehe BACKLOG.md.
