# Bauprojektausführung

## Zweck

Der MCP-Server soll Bauvorhaben kontrolliert planen, prüfen, schrittweise ausführen
und ihren tatsächlichen Zustand zurückmelden. Ein Auftrag ist erst belastbar, wenn
die erzeugten Objekte und ihre Zugänglichkeit strukturiert nachgelesen wurden.

## Vertrag

- Planung, Vorschau und Ausführung sind an eine frische Session gebunden.
- Jede Ausführung besitzt eine neue Aktions-ID und einen abfragbaren Projektstatus.
- Vor jedem Schritt werden aktuelle Geometrie, Freischaltung und bekannte Zugänge
  erneut geprüft.
- Teilstände werden ausgewiesen; es gibt keinen automatischen Retry oder Rollback.
- Auftragsannahme, Fertigstellung, Materiallieferung und Betrieb sind getrennte
  Aussagen.
- Der Entwicklungspilot beschränkt `SmallWarehouse.Folktails` auf höchstens vier
  neue ebene Wege; jeder Weg wird einzeln vor der nächsten Platzierung bestätigt.

## Ausbau

Der aktuelle Pfad ist absichtlich begrenzt. Er wird nach einem erfolgreichen
Live-Nachweis datengetrieben auf weitere Vorlagen, Anschlusswege und Projektgrößen
ausgeweitet. Fehlende Nachweise führen zu einer begründeten Ablehnung, nicht zu
einer stillen Umgehung.
