# Offene Arbeiten

Aktueller Fähigkeitsstand: [PROJECT_STATE.md](PROJECT_STATE.md).
Hier stehen offene Aufgaben; datierte historische Kästchen sind kein aktueller Backlog.

## Nächster sinnvoller Ausbau

1. **Vollständige Güterübersicht:** alle verfügbaren Güter statt nur Water/Berries/Log;
   Bestände, Kapazitäten und sinnvolle Abgrenzung zwischen Lagern, Produktion, Baustellen und Transport.
2. **Alerts und betroffene Ziele:** aktuelle Probleme samt Gebäuden/Bibern/Orten;
   [öffentliche API bereits untersucht](docs/alerts-plan.md), noch nicht implementiert.
3. **Produktionshindernisse und Bedürfnisse:** Hunger/Durst, fehlender Eingang,
   Material, Personal oder andere konkrete Betriebsblockaden zuverlässig unterscheiden.
4. **Erreichbarkeit und Reichweiten:** durchgängige Wegverbindungen, Distrikt-/Arbeitsreichweite.
   `pathAtEntrance` und `entranceOccupants` sind einzelne Beobachtungen, keine Routenprüfung.
5. **Versorgungsentwicklung:** datierte Bestandsverläufe und belastbare Produktions-/Verbrauchsdaten;
   einen positiven Einzelzeitraum nicht als dauerhaft sichere Versorgung ausgeben.

Diese Reihenfolge ist eine Planung, keine Behauptung bereits vorhandener Werkzeuge.
Neue Funktionen werden entsprechend dem Projektauftrag vor ihrer Umsetzung konkretisiert.

## Technische Restpunkte

- Fachliche Fehlercodes auf weitere Personal-, Prioritäts-, Flächen- und Entfernungsfälle ausweiten.
- Expliziten nativen Einstieg beibehalten; historischen Default bei direktem Serverstart
  separat entscheiden, falls Legacy-Kompatibilität geändert werden soll.
- Sonderlayouts, gespiegelte Platzierung, Treppen und spezielle Wege gezielt bewerten;
  generischer Bau unterstützt nicht pauschal jede Vorlage.
- Große Kolonien, mehrere Distrikte, weitere Fraktionen und Spielupdates separat testen.
- Manuelle Log-Funktionen wie Filter/Leeren und Szenenwechsel nicht allein aus dem
  bestätigten Fenster-/Scrolltest als umfassend live abgenommen darstellen.

## Noch offene Spielabnahme

- Vollständigen Wohnraum nachweisen.
- Nachhaltige Wasser-/Nahrungs-/Holzversorgung über einen begrenzten, aussagekräftigen Zeitraum nachweisen.
- Güterfluss, Fertigstellung und Wirkung bei weiteren Gebäuden getrennt prüfen.

## Später

- [Frage-Popup mit Texteingabe im Spiel](docs/player-question-popup.md).
- Energieindustrie, Bots, Distriktmigration, komplexe Automationsgraphen, Terraforming und Wasserbau-Großprojekte.
- Automatisches Speichern/Laden; bislang nicht als MCP-Funktion implementiert.
