# Architekturentscheidungen

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
