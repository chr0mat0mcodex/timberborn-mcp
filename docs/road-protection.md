# Wegschutz-Diagnose — experimentell in 0.23.x

**Arbeitsweise:** Testsystem, schnelle und effiziente Entwicklung. Ältere Einzel-
freigaben und Spielstandschutzvorgaben sind überholt; Versionsgrenzen unten
beschreiben den Code. [Aktuelle Direktiven](../AGENTS.md).

**Kein fertiger Wegschutz. Nicht als reguläre Bauversion verwenden.**
Aktuell installiert: 0.25.0; gemeinsame Vorschau unter 0.24.1 begrenzt live bestätigt.
0.25.0 ergänzt ausschließlich den ausdrücklich freigegebenen kleinen
[Entwicklungs-Baupilot](building-project-execution-proposal.md); installiert, Live-Abnahme offen.
Die hier beschriebene normale Unknown-Sperre bleibt unverändert.
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
- Ab 0.23.2: `roadProbeCount` und `constructionProbeCount` zählen Prüfpunkte vor
  dem Vergleich mit Distrikten. `affected.kind` unterscheidet `road_cell`,
  `construction_access` und `building_access`. Das historische Feld `entrance`
  enthält bei `road_cell` die Wegkoordinate, sonst die Gebäude-Eingangskoordinate.
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
Installation und begrenzter Kontrolltest dieser Korrektur bestätigt.

## Kontrolltest 0.23.1: bestanden (2026-10-02)

Zwei Vorschauen, 13 MCP-Aufrufe: Der geometrisch ungültige Kontrollplatz meldete
`blocked`, 57 verbundene Ausgangspaare von 430 Vergleichen und zwei verlorene
Zugänge samt betroffenen Objekten. Der freie Wegplatz meldete keinen Verlust und
weiterhin `unknown` wegen unvollständiger Abdeckung. Beide Vorschauen wurden
wiederhergestellt; unabhängige Wegabfrage unverändert, 171 Gebäude/Wegobjekte
erhalten und Spielzeit unverändert pausiert. Keine Bau-/Abrissaufträge.

Der ursprüngliche begrenzte Prüfplan:

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

Nächster Entwicklungsschritt: Baustellen- und reine Wegknotenabdeckung prüfen und ergänzen. Die bestätigte Vorschau-Sperrwirkung allein erlaubt noch keine sichere Baufreigabe.

## 0.23.2 — Wegpunkte und bestehende Baustellen (Live-Nachweis offen)

Fertige Objekte mit öffentlichem `PathSpec` liefern ihren Hauptwegpunkt über
`BlockObject.TransformCoordinates(MainPathCoordinates)` und
`NavigationCoordinateSystem.GridToWorld`. Diese Punkte werden zusätzlich zu
Gebäudezugängen gegen die native aktuelle und Vorschau-Distriktzugehörigkeit geprüft.
Unfertige vorhandene Objekte werden mit ihren gültigen `Accessible`-Zugängen
einbezogen; ein Baustellenzugang wird nicht als fertiger Umweg angenommen.
Die Grenzen von 4096 Prüfpunkten und 16384 Vergleichen gelten für alle Arten zusammen.

Das beweist weder Bauarbeiter-Erreichbarkeit über Gelände noch die komplette
Navigation eines Wegobjekts mit mehreren Ebenen. Distriktlose Wegnetze und der
hypothetische Bauzustand des neu geplanten Objekts sind weiterhin nicht abgedeckt.
Öffentliche Metadaten bieten `IBlockObjectNavMesh`, aber keine belegte Funktion
zum Umschalten einer isolierten Vorschau in eine Baustellenvorschau. Interne
BlockObjectNavMesh-/Preview-Typen werden nicht per privater Reflection verwendet.
`constructionCovered=false` und die Platzierungssperre bleiben erhalten.

Nächster Pilot nach Installation: höchstens drei reine Vorschauen (bekannter
Sperrplatz, freier Wegplatz, vorhandener Baustellenzugang sofern beobachtbar).
Prüfpunktzahlen und betroffene Arten prüfen; Navigation, Weltbestand und Pause
zurücklesen. Fehlt eine erwartete Punktart oder Wiederherstellung, stoppen und
die Koordinaten-/Zugangsabbildung klären. Kein umfassender Wegschutz-Abnahmehaken.

### Live-Pilot 0.23.2 (2026-10-02)

119 Wegprüfpunkte, 549 Vergleiche und 170 verbundene Ausgangspaare bestätigt.
Die bekannte Sperrvorschau meldet drei verlorene Wegzellen und zwei Gebäudezugänge;
freie Gegenprobe ohne Verlust. Beide Vorschauen zurückgenommen, unabhängige
Wegverbindung und 171 Gebäude/Wegobjekte unverändert, Spiel pausiert. 14 MCP-Aufrufe.
Keine offene Baustelle vorhanden; Baustellenzugänge bleiben ohne positiven
Live-Nachweis. Nächster Test benötigt einen regulären unfertigen Bauauftrag an
einem bestehenden Weg. Kein vollständiger Wegschutz und keine Baufreigabe.

### Ergänzter Baustellentest

Eine vom Nutzer angelegte Lagerbaustelle liefert neun construction-Prüfpunkte.
558 Vergleiche, weiterhin 170 verbundene Ausgangspaare: Die zusätzlichen neun
Punkte sind nicht als Distriktweg verbunden. Gleichzeitig meldet die native
Zugangsabfrage buildersReachable=true, vor und nach der rückstandslos entfernten
freien Vorschau. 172 Gebäude/Wegobjekte und pausierte Spielzeit unverändert.
Erfassung bestätigt; kein Beleg für Schutz des Baustellenzugangs. Benötigt wird
separat Bauarbeiter-/Geländenavigation unter Vorschau. Keine weiteren gleichartigen
Blindtests und keine Gleichsetzung von Distriktweg- und Bauarbeiter-Erreichbarkeit.
