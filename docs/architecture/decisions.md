# Architekturentscheidungen

> Chronologische Entscheidungen. Der aktive Pfad ist die [eigene native Bridge](native-game-api.md); frühe More-HTTP-API-Entscheidungen dokumentieren den abgelösten POC.

## 2026-09-19 — POC-Implementierung

- MCP läuft extern auf .NET 10 über das offizielle C# SDK und stdio.
- Contracts/Application enthalten keine Unity- oder Mod-Abhängigkeiten.
- More HTTP API ist der erste reale Adapter; Fake ist nur explizit wählbar.
- Ausschließlich feste Leserouten, keine generische URL-/Methodenweiterleitung.
- Hostname localhost wird beibehalten; die tatsächliche Verbindung bleibt auf Loopback-IP-Adressen begrenzt.
- Ausgabe ist limitiert und enthält keine ungefilterten Mod-Einstellungen oder Dateiverzeichnisse.
- Keine Voraussage des Wetters, bevor das Backend die Prognose sichtbar markiert.
- Kleine zusammengehörige Records stehen in Models.cs; Application-Operationen in ObservationService.cs.
  Der Dateiplan wurde deshalb zusammengefasst, die Projektgrenzen bleiben unverändert.
- Backend-internes JSON wird mit geprüften JsonElement-Lesehilfen abgebildet. Es werden keine
  externen DTO-Assemblies referenziert. Diese Wahl spart eine zweite umfangreiche Fremdmodellhierarchie.
- SDK-Handler werden direkt registriert; explizite Eingabeschemata und zentrale Validierung weisen
  unbekannte Parameter ab. Ausgabeschemata werden aus den eigenen Ergebnistypen exportiert.
- Ein GUID-Typ im eigenen Vertrag reicht im POC; separate EntityId-Wrapper sind noch nicht nötig.
- Konfiguration ausschließlich über Prozessvariablen. server.example.json beschreibt diese;
  es ist kein automatisch geladener Dateikonfigurationsanbieter.
- Laufzeitlimits aus dem Missionsplan sind im POC fest. HTTP-Limits sind im Adapter konstruierbar;
  es gibt noch keine benutzerseitige Erweiterung der Limits ohne Änderung und erneuten Test.
- Einzelgebäude zuerst gegen Gebäudeliste prüfen, da die fremde Einzelroute den Typ nicht garantiert.
  Nicht gelistete IDs ergeben entity_not_found. Eine separate Nicht-Gebäude-Diagnose ist ohne
  verlässliche Entity-Typabfrage nicht belegt und wird daher nicht behauptet.
- Epoch-/Koloniewechsel sind über diese API nicht sicher identifizierbar. Beobachtungen werden
  pro Aufruf frisch erhoben, keine Entity-Daten über Aufrufe gecacht. Capabilities sind nur kurze
  Hinweise (30 Sekunden) und kein Beweis für die nächste Abfrage.
- Benutzeroberflächenvergleich bleibt Teil der Live-Abnahme; automatische Tests ändern keine Spielwerte.

## 2026-10-02 — Vorschau-Navigation vor eigener Graphrekonstruktion

Für Wegschutz zunächst öffentliche Preview-/Distrikt-Services verwenden. Keine
XY-Nachbarschaftsannahmen, realen Probeabrisse oder privaten Navigationsgraphen.
Ein Vorschauvergleich ersetzt keinen Nachweis der Baustellen-/Wegknotenabdeckung.
Der experimentelle Stand 0.23.0 hält den Baupfad deshalb geschlossen; Live-Pilot
muss die Semantik vor einer Erweiterung belegen. Aktueller Umfang und Grenzen:
[Wegschutz-Diagnose](../road-protection.md).

## 2026-10-02 — Gebäude und Anschlussweg gemeinsam planen

Nutzerentscheidung: nächster Ausbau verbindet Bauplatzsuche und Anschlussweg in
einem gemeinsamen Bauvorhaben. Die Mod prüft Spielzustand und setzt reguläre
Aktionen; MCP bietet Planwahl und Ausführung. Erste Stufe ausschließlich ebene
Wege. Anschluss vor Gebäude setzen und unabhängig bestätigen; bestehenden
Wegschutz und Bauarbeiterprüfung nicht dadurch ersetzen. Mehrere Aktionen sind
nicht atomar: Teilerfolge melden, stoppen, keine automatische Abriss-Rücknahme.
[Vertrag und Abnahme](../building-site-search-plan.md). Noch nicht implementiert.
