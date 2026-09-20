# Offene Arbeiten

Aktueller Fähigkeitsstand: [PROJECT_STATE.md](PROJECT_STATE.md).
Hier stehen offene Aufgaben; datierte historische Kästchen sind kein aktueller Backlog.

## Nächster sinnvoller Ausbau

1. **Güterauswertung erweitern:** 40 registrierte Güter samt ResourceCount-Feldern in
   0.18.0 live bestätigt. Vollständige Baustellenbilanz und Versorgung über Zeit bleiben offen.
2. **Statusabdeckung erweitern:** eine Lagerwarnung samt Ziel und leerer Güterwahl live
   bestätigt. Mehrfachziele und Verschwinden einer aktiven Meldung ebenfalls bestanden; Biberwarnungen offen.
   [Vertrag und Nachweise](docs/economy-observations.md). Vollständige UI-Meldungsabdeckung,
   Benachrichtigungshistorie und dynamische Aggregatwerte weiterhin offen.
3. **Produktionshindernisse und Bedürfnisse:** Hunger/Durst, fehlender Eingang,
   Material, Personal oder andere konkrete Betriebsblockaden zuverlässig unterscheiden.
4. **Erreichbarkeit und Reichweiten live abnehmen:** 0.19.0 nutzt native Eingangsprüfungen,
   Accessible.FindRoadPath und IBuildingWithRange. Installiert; Live-Pilot noch offen.
   Mehrdeutige Zugänge bleiben unsupported; keine allgemeine Geometrieabdeckung behauptet.
5. **Versorgungsentwicklung live abnehmen:** 0.19.0 liest gespeicherte GoodSamples mit
   getrennten Produktions-/Verbrauchszählern und Bestandsänderungen. Installiert; Live-Pilot offen;
   Abtastdauer und Periodengrenzen noch nachweisen. [Vertrag](docs/logistics.md).

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
