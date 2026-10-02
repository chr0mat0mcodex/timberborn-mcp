# Offene Arbeiten

Aktueller Fähigkeitsstand: [PROJECT_STATE.md](PROJECT_STATE.md).
Hier stehen offene Aufgaben; datierte historische Kästchen sind kein aktueller Backlog.

## Aktueller Spielauftrag

100 lebende Biber erreichen und alle regulären Gebäudetypen mindestens einmal fertigstellen.
[Abnahme, Fortschritt und vollständige Checkliste](docs/colony-goals.md).
Aktuelle Schwerpunkte: Holznachschub, zusätzlicher Wohnraum, Forschung und Versorgung.
15 reguläre Gebäude sind noch durch den MCP-Baupfad begrenzt; Größen, Sonderlayouts,
Geländeanschluss und Sitzungsbudgets vor der breiten Bauabnahme erweitern und prüfen.

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
   Pumpen weiter ohne Ausgangsplatz. Globale Ausgangsbestände nicht als Pumpenbestand interpretieren. 0.21.1 live bestätigt: Tanks 60, Pumpen 30, Distriktzentrum 48 ergeben 138 Wasser. Kein fehlerhaft überfüllter Pumpenpuffer. Entnahme und Wiederauffüllung beider Pumpen inzwischen über zwölf Messpunkte bestätigt; Gesamtwasser im Fenster 138→130. Wohnraum inzwischen bestätigt; Nachhaltigkeit von Wasser/Nahrung/Holz offen.

Diese Reihenfolge ist eine Planung, keine Behauptung bereits vorhandener Werkzeuge.
Neue Funktionen werden entsprechend dem Projektauftrag vor ihrer Umsetzung konkretisiert.

## Technische Restpunkte

- Sollbesetzung null prüfen: Inventor nimmt desiredWorkers=0 im Live-Test nicht an (observed=1, outcome=unconfirmed). Kein Retry; für Stilllegung reguläre Gebäudepause verwenden.

- Fachliche Fehlercodes auf weitere Personal-, Prioritäts-, Flächen- und Entfernungsfälle ausweiten.
- Expliziten nativen Einstieg beibehalten; historischen Default bei direktem Serverstart
  separat entscheiden, falls Legacy-Kompatibilität geändert werden soll.
- Sonderlayouts, gespiegelte Platzierung, Treppen und spezielle Wege gezielt bewerten;
  generischer Bau unterstützt nicht pauschal jede Vorlage.
- Große Kolonien, mehrere Distrikte, weitere Fraktionen und Spielupdates separat testen.
- Manuelle Log-Funktionen wie Filter/Leeren und Szenenwechsel nicht allein aus dem
  bestätigten Fenster-/Scrolltest als umfassend live abgenommen darstellen.

## Noch offene Spielabnahme

- Wohnraum bestätigt: 40 Betten, zuletzt 40 lebende Biber und keine Obdachlosen; Doppel-Lodge über Dachweg erreichbar. Kapazität bei Bevölkerungswachstum weiter beobachten.
- Nachhaltige Wasser-/Nahrungs-/Holzversorgung über einen begrenzten, aussagekräftigen Zeitraum nachweisen. Förster und 79 lebende Eichen bestätigt. Birken-Ernte/Nachpflanzung an konkreten Stellen belegt; inzwischen 33 Birken-Pflanzplätze. Eichen-Ernte/Nachpflanzung an 19 früher belegten Standorten bestätigt; dauerhafte Gesamtbilanz offen. Zwei Sägewerke und zwei Erfinder aktiv; erste Zahnräder produziert. 63 Karottenfelder, zwei aktive Farmen und drei aktive Pumpen; erste südliche Ernte/Nachpflanzung bestätigt. Nächste Etappe: Wohnraum und Versorgung für 60 Biber, Energie und größerer Wasservorrat.
- Güterfluss, Fertigstellung und Wirkung bei weiteren Gebäuden getrennt prüfen.

## Wichtige Future-Features

- **WICHTIG — Ingame-Zeitläufe:** Simulation für X Spielstunden/-tage/-wochen oder bis zu einem
  konkreten Spielzeitpunkt laufen lassen und automatisch pausieren; Zeitmessung,
  Auftragsstatus und Abbruch übernimmt der MCP-/Mod-Pfad.
  [Vertrag und Abnahme](docs/simulation-runs-plan.md): in 0.22.0 implementiert und automatisch
  geprüft. **Live-Pilot bestanden:** 1/3/7, absolute Tagesgrenze, Abbruch und expliziter Eingriff; Zielzeit und Pause unabhängig bestätigt.

- **WICHTIG — Bauplatzsuche:** Gebäudevorlage und Suchbereich angeben; konkrete,
  geprüfte Plätze mit Drehung, Eingang, Anschluss und etwaigen Vorarbeiten erhalten.
- **WICHTIG — Schutz vor Wegversperrung (in Arbeit):** Bauaufträge standardmäßig verweigern,
  wenn sie die letzte nutzbare Verbindung oder Gebäudezugänge versperren; konkrete
  Konflikte und betroffene Ziele melden. Auch direkte Platzierungen absichern.
  [Gemeinsamer Featureplan und Abnahme](docs/building-site-search-plan.md).
  0.23.0: drei Vorschauen geprüft, negative Sperrwirkung unter 0.23.1 im Kontrolltest bestätigt.
  0.23.1 ergänzt Kontrollvorschauen und verbundene Ausgangszugänge; installiert, begrenzter Kontrolltest bestanden.
  [Diagnose-Prototyp](docs/road-protection.md), alle Bauaufträge bis zum vollständigen Nachweis gesperrt.
  0.23.2: Wegpunkte und Sperrwirkung live bestätigt; neun Baustellenpunkte erfasst,
  aber trotz buildersReachable=true keine Distriktweg-Verbindung. Bauarbeiter-/
  Geländenavigation unter Vorschau sowie hypothetische Bauphase bleiben offen.

- **WICHTIG — Gemeinsamer Bauplan (zur Umsetzung freigegeben):** Gebäude und Anschlussweg
  zusammen planen und prüfen; zunächst ebene Wege. Erst Wege regulär setzen und
  Nutzbarkeit bestätigen, danach Gebäudeauftrag. Grenzen, erneute Validierung,
  Aktions-IDs und Teilergebnisse vorsehen; kein automatischer Abriss-Rollback.
  Anschlussprüfung ersetzt weder Schutz bestehender Zugänge noch Bauarbeiterprüfung.
  [Beschlossener Ablauf und Abnahme](docs/building-site-search-plan.md).
  0.24.0: rein lesende Kandidatensuche implementiert (max. 8 × 8, vier Optionen),
  alle Vorschläge ausdrücklich nicht ausführbar. Begrenzter Live-Pilot bestanden;
  gemeinsame Vorschau in 0.24.1 live bestätigt; begrenzte Ausführung in 0.25.0 implementiert, Live-Abnahme offen.

## Später

- [Produktionsgraph 0.21.0](docs/production-dependency-graph.md) ist implementiert. Begrenzter Live-Pilot bestanden. Noch offen: weitere Fraktionen, Ruinenerträge und spezielle Betriebsbedingungen; keine neue Fremdmod-Abhängigkeit.

- [Frage-Popup mit Texteingabe im Spiel](docs/player-question-popup.md).
- Energieindustrie, Bots, Distriktmigration, komplexe Automationsgraphen, Terraforming und Wasserbau-Großprojekte.
- Automatisches Speichern/Laden; bislang nicht als MCP-Funktion implementiert.

0.24.1: validate_building_project ergänzt gemeinsame Vorschau von höchstens acht
Wegfeldern und einem Gebäude, mit frischer Plan-Kennung und Nachkontrolle. Gebaut,
Live-Pilot bestanden; normaler Baupfad weiterhin gesperrt.

0.24.1 Live-Pilot bestanden: Lager plus zwei Wege gemeinsam gültig, Vorschau-Eingang
verbunden, keine Verbindungsverluste, Wiederherstellung und Cache-Wiederverwendung
bestätigt. Falsche Plan-Kennung abgelehnt. Vor Ausführung bleiben Bauphase und
Bauarbeiter-Erreichbarkeit offen; keine Freigabe allein aus diesem Vorschautest.

## Nächste Abnahme: 0.25.0

- [x] Explizit freigegebenen Entwicklungspilot für ein kleines Lager und ≤2 Wege
  implementieren: Auftrags-ID, Status, Wiederholschutz, Rückprüfung je Schritt.
- [x] 0.25.0 bei beendetem Spiel installieren, Sicherung und Dateivergleich prüfen.
- [ ] Begrenzten realen Baupilot nach Neustart abnehmen.
- [ ] Vollständigen Bauphasen-/Wegschutz vor allgemeiner Freigabe nachweisen.
  Die Ausnahme ist keine generelle Lockerung. [Vertrag](docs/building-project-execution-proposal.md).
