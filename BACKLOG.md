# Offene Arbeiten

Aktueller Fähigkeitsstand: [PROJECT_STATE.md](PROJECT_STATE.md).
Hier stehen offene Aufgaben; datierte historische Kästchen sind kein aktueller Backlog.

## Nächster sinnvoller Ausbau

1. **Güterauswertung erweitern:** 40 registrierte Güter samt ResourceCount-Feldern in
   0.18.0 live bestätigt. Vollständige Baustellenbilanz und nachhaltige Versorgung bleiben offen; Tageshistorien sind bestätigt.
2. **Statusabdeckung erweitern:** eine Lagerwarnung samt Ziel und leerer Güterwahl live
   bestätigt. Mehrfachziele und Verschwinden einer aktiven Meldung ebenfalls bestanden; Biber-Todesstatus mit zwei Entity-Zielen bestätigt; aktive Hunger-/Durst-UI-Warnungen offen.
   [Vertrag und Nachweise](docs/economy-observations.md). Vollständige UI-Meldungsabdeckung,
   Benachrichtigungshistorie und dynamische Aggregatwerte weiterhin offen.
3. **0.21.0 live bestätigt:** alle 30 Leser, Graphgröße/Revision und repräsentative Quellen/Ketten bestanden; Lebenszustandskorrektur mit 11 lebenden und zwei toten Bibern bestätigt. Zuvor registrierte tote Biber
   wurden unter 0.20.0 als Bedürfnisempfänger mitgezählt. Öffentliche Mortal.Dead-Abfrage
   trennt nun lebend/tot/unbekannt; keine aktuellen Bedürfniswerte für tote/unbekannte Ziele.
   **Bedürfnis-/Betriebsdiagnose vervollständigen:** 0.20.0 mit 29 Lesern und gezielten
   Bedürfnis-/Betriebspiloten live bestätigt. Rohe Warnschwellenflags nicht als UI-Alarm werten.
   Baustellenfall einschließlich Aufräumen live bestätigt; aktive Hunger-/Durstfälle fehlen noch.
   [Vertrag und begrenzter Pilot](docs/needs-and-operation.md). Vollständige Energie-, Wasser-,
   Rohstoff- und Lieferdiagnose weiterhin offen; keine Ursachen aus bloßem Stillstand ableiten.
4. **Erreichbarkeit breiter prüfen:** Sofort-Wegsuche besteht den Unterbrechungstest.
   0.19.2 liefert für Farm/Holzfäller 485/611 Terrainzellen; erste und letzte Seite live geprüft.
   Weitere Gebäudetypen, Reichweitenänderungen und besondere Weglayouts separat abnehmen.
5. **Versorgungsdefizite gezielt erklären und korrigieren:** rund drei Spieltage beobachtet,
   elf Messpunkte ohne Hunger/Durst, aber negative Wasser-/Beeren-/Holzbilanz.
   [Nachweis und Grenzen](docs/supply-balance.md). Lokale Produktions-/Lagerursachen
   eingrenzen. Mittleres Lager auf Beeren gesetzt; Tagesproduktion steigt auf 19. Vergleich
   wegen zweier Alterstodesfälle nach rund 1,22 Tagen abgebrochen, keine Dreitagesabnahme.
   Zusätzlicher kleiner Wassertank unter 0.21.0 gebaut und gefüllt; Lager 30→60,
   globale Ausgangsbestände 108→78, Gesamtwasser 138 unverändert. Umlagerung bestätigt,
   Pumpen weiter ohne Ausgangsplatz. Globale Ausgangsbestände nicht als Pumpenbestand interpretieren. 0.21.1 ergänzt Gebäudeinventare; ist installiert und an Distriktzentrum, Pumpen und Tanks live abzugleichen. Nachhaltigkeit offen.

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

- [Produktionsgraph 0.21.0](docs/production-dependency-graph.md) ist implementiert. Begrenzter Live-Pilot bestanden. Noch offen: weitere Fraktionen, Ruinenerträge und spezielle Betriebsbedingungen; keine neue Fremdmod-Abhängigkeit.

- [Frage-Popup mit Texteingabe im Spiel](docs/player-question-popup.md).
- Energieindustrie, Bots, Distriktmigration, komplexe Automationsgraphen, Terraforming und Wasserbau-Großprojekte.
- Automatisches Speichern/Laden; bislang nicht als MCP-Funktion implementiert.
