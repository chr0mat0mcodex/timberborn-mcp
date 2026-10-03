# Technische Übergabe

## Stand

Agent Bridge 0.28.1 ist installiert und über den nativen MCP-Pfad erreichbar.
Der aktuelle Arbeitsbranch ist `codex/road-protection-pilot`. Ein begrenzter
Bauprojekt-Livefall hat Vorschau, drei Wegaufträge, Lagerauftrag und Zugang über
denselben Projektstatus sowie Objekt-Rücklesung bestätigt. Ein Kombi-Treppenpilot
(eine Treppe plus ein oberer Weg) ist zusätzlich mit regulärer Vorschau,
bestätigten Schritten und beiden Objekt-Rücklesungen live belegt.

## Nächster Ablauf

1. Die nächste Bauvorlage oder einen längeren Anschluss gezielt erweitern.
2. Kandidat, Vorschau, Ausführung und Rücklesung weiterhin an dieselbe Session
   und eine neue Aktions-ID binden.
3. Bei Fehlern Ursache beheben; keine unbestätigte Aktion blind wiederholen.

## Wichtige Grenzen

- Die eigene Bridge ist die Laufzeitbasis; Legacy-Adapter sind keine Voraussetzung.
- Aktionen benötigen passende Freigaben in Mod und MCP-Prozess.
- Private Konfiguration bleibt lokal und wird nicht ausgegeben oder eingecheckt.
- Konkrete Spielstände und historische Baupläne sind nicht Teil der Übergabe.
