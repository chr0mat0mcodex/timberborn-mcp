# Technische Übergabe

## Stand

Agent Bridge 0.25.0 ist installiert und über den nativen MCP-Pfad erreichbar.
Der aktuelle Arbeitsbranch ist `codex/road-protection-pilot`. Die regulären Tests
und der Mod-Build sind dokumentiert; der aktuelle Bauprojektpfad braucht noch einen
erfolgreichen Live-Nachweis.

## Nächster Ablauf

1. Bridge-Version, Session und Simulationszustand lesen.
2. Einen begrenzten, gültigen Bauprojektkandidaten suchen.
3. Vorschau und Ausführung über dieselbe Session und neue Aktions-ID durchführen.
4. Projektstatus sowie die erzeugten Objekte und Zugänge strukturiert nachlesen.
5. Bei Fehlern Ursache beheben; keine unbestätigte Aktion blind wiederholen.

## Wichtige Grenzen

- Die eigene Bridge ist die Laufzeitbasis; Legacy-Adapter sind keine Voraussetzung.
- Aktionen benötigen passende Freigaben in Mod und MCP-Prozess.
- Private Konfiguration bleibt lokal und wird nicht ausgegeben oder eingecheckt.
- Konkrete Spielstände und historische Baupläne sind nicht Teil der Übergabe.
