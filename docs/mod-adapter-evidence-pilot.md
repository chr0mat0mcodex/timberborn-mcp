# A08-Pilot: direkte Mod-Adapter getrennt nachweisen

Stand 2026-10-18: menschliches Gate und begrenzter Livepilot bestanden.
Nach ungeeigneter Bank-Negativkontrolle gezielt mit Wohnhausvorschau abgeschlossen.
Keine neue Werkzeugfamilie oder Freigabe.

## Umfang und Entscheidung

Pilot ist ausschließlich die Abschlussbewertung von
[`RoadProtection.Finish`](../mod/Timberborn.AgentBridge/RoadProtection.cs).
[`RoadProtectionPolicy.EvaluatePreview`](../src/Timberborn.Bridge.Core/RoadProtectionPolicy.cs)
entscheidet aus Navigation wiederhergestellt, nullable Baustellen-Wiederherstellung
und Verlustzahl über `restored`, `status` und den bisherigen Fehlergrund.
Die bestehende `Decision` bleibt die Statusentscheidung; kein zweites Regelwerk.

Die Mod behält native Instant-/Preview-Abfragen, Baseline, Probezuordnung,
Verlustermittlung und Berichtserzeugung. Vorschauaufbau und Cleanup der Aufrufer
bleiben unverändert. Die frühe Rückgabe bei unvollständiger Baseline oder fehlender
Vorschau erfolgt weiterhin vor der ausgelagerten Entscheidung. Reihenfolge und
Kurzschluss der beiden nativen Wiederherstellungsabfragen bleiben erhalten.

Semantik unverändert: fehlende Wiederherstellung hat Vorrang vor Verlust;
Verlust bei Wiederherstellung bedeutet `blocked`; ohne Verlust bleibt `unknown`,
da `constructionCovered=false`. Nullable Baustellen-Wiederherstellung wird wie
bisher nur bei explizitem false als Fehler gewertet. Null beweist keine
Baustellenabdeckung und erzeugt niemals `safe`.

Konservative Alternative wäre nur eine Nachweisliste ohne Codeänderung gewesen.
Der kleine Schnitt macht zusätzlich die bisher im Adapter liegende Kombination
und Fehlerpriorität ohne Spiel-DLLs prüfbar. Keine generische Adapterabstraktion,
keine neuen Abhängigkeiten oder Spiel-APIs. Bridge/API bleibt 0.74.0.

## Nachweismatrix

| Ebene | Prüft konkret | Prüft ausdrücklich nicht | Status |
| --- | --- | --- | --- |
| Core-Unitfälle | 12 Kombinationen aus Navigation ja/nein, Baustelle true/false/null und Verlust 0/2; ein negativer Zähler | native Reads, Cleanup, richtige Probezuordnung | 13 Fälle bestanden |
| Bestehende Vertrags-/Integrationstests | Wire-/Backend-Verträge im bisherigen Umfang | Ausführung des direkten Mod-Adapters | bestanden |
| Separater Mod-Release-Build im Gate | Kompilierung gegen lokale Spiel-DLLs und Core-Verknüpfung | Laufzeitwirkung der Dienste | bestanden |
| Paketprüfung im Gate | installierte Dateien entsprechen dem gewählten Paket | fachliches Verhalten | bestanden |
| Nativer MCP-Livepilot | echte Vorschau, Verlustdiagnose, beobachtete Wiederherstellung und anschließende Nutzbarkeit | alle Karten/Adapter, Fehler-Injektion, Bau-/Fertigzugang | Positivfall, Verlustfall und erneute Kontrolle bestanden |

Die [Solution](../TimberbornMcp.slnx) und
[Unit-Testreferenzen](../tests/Timberborn.Tests/Timberborn.Tests.csproj) enthalten
die spielabhängige AgentBridge nicht. Eine grüne Testsummenanzeige ist daher
kein direkter Adaptertest. Der Mod-Build ist separat im menschlichen Gate.

## Gezielte Liveabnahme nach dem Gate

1. Verbundenes Backend starten, native Bridge, aktuelle Session und MCP-Kennung
   lesen; mit dem Gatebericht abgleichen. Spiel für vergleichbare Zustände pausiert
   halten. Geeignete freie Fläche und bestehenden verbundenen Zugang strukturiert
   bestimmen; keine historischen Koordinaten blind übernehmen.
2. `validate_building(detail=full)` auf einer geeigneten freien Stelle:
   `gameValidated=true`, `valid=true`, vollständige Baseline mit verbundenen
   Proben, `lostConnections=0`, `restored=true`, `constructionCovered=false`,
   `status=unknown`, Grund `construction_and_road_node_coverage_unproven`.
   `noPersistentChangeObserved=true`, `sessionLocked=false`.
3. Eine gezielt über einem belegten Bestandszugang liegende Gebäudevorschau als
   Negativkontrolle: echte Verluste samt betroffenen Verbindungen erwarten,
   `blocked`/`existing_district_access_lost`, trotzdem `restored=true` und keine
   persistente Änderung. Geometrische Ablehnung allein reicht als Verlustbeleg
   nicht. Kein Bauauftrag erforderlich.
4. Freie Kontrollvorschau aus Schritt 2 erneut lesen: gleiche fachliche Werte,
   keine Sperre, kein zurückgebliebener Verlust. Versuchszähler darf sich ändern.

Erfolg: alle drei Vorschauen erfüllen ihre Erwartungen in derselben Session.
Abbruch bei fehlender Baseline, unerwarteter Kontrollantwort, fehlender
Wiederherstellung oder Session-Sperre; dann gezielt diagnostizieren, nicht mit
weiteren Fällen skalieren. Keine künstliche native Ausnahme erzeugen: Fehler-
und Nullkombinationen sind zunächst nur synthetisch geprüft. Der Pilot schafft
keine neue Baufreigabe; Umwege, Höhen, Sessionwechsel, Bauarbeiter- und tatsächlicher
Fertigzugang erhalten dadurch keinen zusätzlichen Nachweis.

## Nutzen und Aufwand

Bewertung: kleiner, sinnvoller Schnitt mit einer Core-Funktion, einem Adapteraufruf
und 13 synthetischen Fällen. Entscheidungspriorität ist isoliert prüfbar; die
Nachweismatrix verhindert, dass die grüne Testsumme als nativer Adapterbeleg gilt.
Zusatzaufwand entsteht bei Auswahl und Diagnose einer echten Verlustkontrolle.
Kein Laufzeitgewinn behauptet. Nutzen/Aufwand positiv; begrenzte Liveabnahme
bestanden. Zusätzlicher Diagnoseaufwand: ein erfolgloser Bank-Kontrollfall und
eine gezielte Wohnhauskontrolle, keine Produktcodeänderung oder Gate-Wiederholung.

## Tatsächlicher Gate- und Livebeleg

- Menschliches Gate: 13 Paket-, 28 Supervisor-, 2221 Unit- und 14 Integrationstests
  bestanden; drei optionale Livefälle übersprungen. Beide Release-Builds ohne
  Warnungen/Fehler, fünf installierte Paketdateien per SHA-256 bestätigt.
- Verbundener Supervisor gestartet; Bridge 0.74.0 und frische Session bestätigt.
  Build-/Katalogkennung exakt gleich Gate. Spiel pausiert, kein aktiver Lauf.
- Positivkontrolle mit freier Bankvorschau: geometrisch gültig, 740 geprüfte
  Zugang-/Distriktpaare, 378 vorher verbunden, 304 Wegproben, keine Verluste.
  `restored=true`, `constructionCovered=false`, `unknown` mit erwartetem Grund.
  Keine persistente Änderung beobachtet, keine Session-Sperre.
- Negativkontrolle mit Bankvorschau über einem frisch bestätigten Wohnhauszugang:
  geometrisch ungültig, aber **0 verlorene Verbindungen**, somit `unknown` statt
  erwartetem `blocked`. Wiederherstellung bestätigt; keine persistente Änderung
  oder Session-Sperre. Fallserie hier gemäß Abbruchkriterium beendet; dritte
  Kontrollvorschau nicht ausgeführt.
- Lesende Diagnose bestätigt fertigen Path am Kontrollpunkt und unverändert
  freien Wohnhauszugang mit Distriktdistanz 16 vor/nach der Vorschau. Die native
  Verlustermittlung liefert bereits 0; keine falsche Statuszuordnung durch die
  extrahierte Policy belegt. Die einzelne Bank ist an diesem Punkt als
  Verlustkontrolle ungeeignet; geometrische Überlappung allein beweist keine
  Graphunterbrechung. Die interne Ursache wurde nicht weiter isoliert.
- Keine offenen Baustellen im beobachteten Sample: Baustellen-Wiederherstellung
  blieb null. Damit kein Livebeleg für deren true/false-Zweige.

### Gezielter Abschluss nach erneutem Go

- Derselbe Gate-Build, dieselbe Session und pausierter Zustand bestätigt.
  Keine Produktcodeänderung seit dem erfolgreichen Gate.
- Wohnhausvorschau (`Lodge.Folktails`, vier Zellen) am selben Wegknoten statt
  einzelliger Bank, einschließlich angrenzender Zellen: `valid=false`,
  **5 verlorene Verbindungen** bei 740 geprüften Paaren und 378 vorher verbundenen.
  Zwei Wegpunkte und drei Gebäudezugänge vollständig benannt; `blocked` mit
  `existing_district_access_lost`, `restored=true`, `constructionCovered=false`.
- Ursprüngliche freie Bankvorschau erneut: `valid=true`, 740 Paare, 378 vorher
  verbunden, keine Verluste, `unknown` mit unverändertem Abdeckungsgrund.
  Wiederherstellung bestätigt, keine persistente Änderung beobachtet und keine
  Sitzungssperre. Tatsächlicher Wohnhauszugang anschließend frei und mit
  unveränderter Distriktdistanz 16 zurückgelesen.
- Damit positiver Fall, echter nativer Verlustfall und anschließende Kontrolle
  bestanden. Kein Bauauftrag und keine Simulation erforderlich.

Die Kontrollauswahl war die offene Nachweislücke. Der Vergleich belegt die
unterschiedliche native Wirkung beider Vorschauen am gewählten Ort. Er isoliert
nicht den Einfluss von Vorlagentyp gegenüber Grundrissgröße und liefert keine
allgemeine Aussage über Banken oder beliebige Wegnetze. Baustellen-true/false,
native Ausnahmen und Sessionwechsel bleiben außerhalb dieses Livepiloten.

Gesamt-A08 bleibt offen. Weitere Adapter, Sessionwechsel und Fertigzugang bleiben
zurückgestellt; aus diesem Pilot folgt keine automatische Migration.
