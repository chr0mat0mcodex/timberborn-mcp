# Wegschutz-Diagnose — experimentell in 0.23.x

**Kein fertiger Wegschutz. Nicht als reguläre Bauversion verwenden.**
0.23.0 ist seit 2026-10-02 für den freigegebenen Diagnose-Pilot installiert; 0.22.0 vollständig gesichert.
In 0.23.x sind sämtliche MCP-Bauaufträge vorläufig gesperrt, einschließlich Path/Lodge-
Pilotpfaden. Der Grund ist die noch nicht belegte Abdeckung von Baustellen und reinen
Wegknoten. Sonstige Werkzeuge behalten ihren bisherigen Umfang.

## Öffentliche API und Entscheidung

Lokale öffentliche Signaturen aus Timberborn 1.1.2.4:
`Preview.AddToPreviewServices/RemoveFromPreviewServices`, `DistrictCenter.IsOnInstantDistrictRoad`
und `IsOnPreviewDistrictRoad`, `Accessible.Accesses`. Die offiziellen Blueprints zeigen
für Path und Lodge unterschiedliche Navigationsregeln. Die vorhandenen offiziellen
und Community-Referenzen belegen keinen vollständigen hypothetischen Baustellenvergleich.
Es werden keine privaten Member reflektiert, keine Fremdmods ergänzt und keine echten
Navigationskanten oder Gebäude für eine Probe verändert.

Die eingebaute Diagnose vergleicht für vorhandene fertige Gebäudezugänge:

1. Aktuelle Distriktzugehörigkeit mit Vorschau-Zugehörigkeit ohne eigene Vorschau.
   Unterschiede bedeuten unklare Ausgangslage, nicht sicher.
2. Distriktzugehörigkeit bei registrierter eigener Bauvorschau.
3. Wiederherstellung nach Entfernen der Vorschau. Bei Abweichung wird die
   Validierung für die Sitzung gesperrt.

Die native Navigation berücksichtigt ihre eigenen Umwege und Höhen. Wir rekonstruieren
kein vereinfachtes XY-Netz. Das belegt jedoch weder die Vollständigkeit aller Wegknoten
noch das Verhalten während der Bauphase. Deshalb ist `constructionCovered=false`
und eine `safe`-Freigabe in dieser Version absichtlich nicht erreichbar.

## Ergebnisvertrag

`validate_building`, `validate_build_site`, `place_building`, `place_path` und
`place_lodge` liefern zusätzlich `roadProtection` (ab Bridge 0.23.0 verpflichtend):

- `status`: `blocked` bei verlorener nativer Vorschau-Verbindung, `unknown` bei
  unvollständigem Nachweis. `safe` bleibt für eine spätere belegte Implementierung.
- `reasons`: maschinenlesbare Ursache; beispielsweise
  `existing_district_access_lost`, `navigation_preview_baseline_mismatch`,
  `navigation_not_restored`, `construction_and_road_node_coverage_unproven`.
- `checkedConnections`, `lostConnections`, `restored`, `constructionCovered`.
- `connectedBefore`: ab 0.23.1 verpflichtend, tatsächlich verbundene Ausgangspaare.
- `affected`: höchstens 32 betroffene Objekt-IDs, Eingangskoordinaten und
  Distriktzentrum-Koordinaten; `affectedTruncated` kennzeichnet weitere Treffer.
- `candidateCells`: höchstens 64 geprüfte Bauzellen. **Keine Behauptung, dass jede
  dieser Zellen individuell die Ursache der Trennung ist.**

Geometrisches `valid=true` bleibt getrennt von Wegsicherheit. Der Platzierer prüft
beides vor dem regulären Bauaufruf und verweigert unbekannte Sicherheit. Die
MCP-Schicht weist eine behauptete erfolgreiche Platzierung ohne sicheren Beleg zurück.
Ein abgelehnter Auftrag verbraucht wie bisher seine Aktions-ID; nicht blind wiederholen.
Früh abgelehnte Platzierungsaufträge erhalten einen ungeprüften Status. Eine explizite
Validierung diagnostiziert ab 0.23.1 auch geometrisch ungültige Kontrollvorschauen.

Limits: 4096 registrierte Blockobjekte, 16 fertige Distriktzentren, 4096 Zugänge,
16384 Zugehörigkeitsvergleiche. Überschreitung/fehlende Daten ergeben `unknown`.
Vorhandene entity-/Bestandskontrolle und Sessionbindung bleiben erhalten.

## Live-Pilot 0.23.0 und Korrektur 0.23.1

Am 2026-10-02 drei Vorschauen mit insgesamt 27 MCP-Aufrufen geprüft. Freier Wegplatz
und Lodge auf einer zweiten Höhe: geometrisch gültig, jeweils 430 Zugang-/Distriktpaare
verglichen, keine verlorene Verbindung, Vorschau-Navigation wiederhergestellt.
430 bezeichnet Vergleiche, nicht nachgewiesen verbundene Zugänge. Unabhängige
Wegabfrage unverändert; alle 171 Gebäude/Wegobjekte erhalten, Simulation pausiert.
Der beabsichtigte Sperrfall war geometrisch ungültig und wurde deshalb vor der
Navigationsdiagnose ausgesondert. **Keine negative Sperrwirkung live nachgewiesen.**

0.23.1 diagnostiziert deshalb auch geometrisch ungültige Vorschauen, ohne deren
Bauzulässigkeit zu ändern. `connectedBefore` zählt tatsächlich verbundene
Ausgangspaare; eine vollständig unverbundene Basis ergibt `unknown` mit
`navigation_no_connected_baseline`. Der Native-Vertrag verlangt das Feld ab 0.23.1;
0.23.0 bleibt ohne dieses Feld lesbar. Alle Bauaufträge bleiben gesperrt.
Installation und Live-Nachweis dieser Korrektur stehen aus.

## Nächster Nachweis: begrenzte Kontrollvorschau

Nach Installation ausschließlich Validierung, keine realen Bau-/Abrissaktionen:

1. Ein bekannt freier Platz: geometrisches Ergebnis und `unknown` wegen fehlender
   Vollabdeckung nachvollziehbar; Weltbestand unverändert.
2. Ein bewusst geometrisch ungültiger Kontrollplatz auf einem bestehenden Zugang: native
   Vorschau muss eine verlorene Verbindung melden und das betroffene Objekt nennen.
3. Alternative Verbindung beziehungsweise zweite Höhe: Unterschied nachvollziehbar;
   Ausgangsnavigation nach jeder Vorschau unabhängig unverändert zurücklesen.

Wenn schon der bekannte Engpass keine Änderung zeigt oder die Vorschau verzögert
wirkt, den Pilot beenden und den Vorschau-Lebenszyklus korrigieren. Keine weiteren
Dutzende Blindversuche. Erst danach Baustellen-/Wegknotenabdeckung implementieren
und sichere Bauaufträge freigeben. Keine Entfernung der Sperre nur aufgrund eines
positiven Einzelplatztests. Bauplatzsuche bleibt ein separates Folgefeature.
