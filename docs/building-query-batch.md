# 0.39.0 — gefilterte Suche und gebündelte Betriebsbeobachtung

Begrenzt live abgenommen am 2026-10-11 nach menschlichem Gate und Client-Neustart.
Paket: `agent-bridge-0.39.0-20261011-212534-cae95f12`.

## Live-Nachweis

- 15 Manufactory-Gebäude vollständig mit einem gefilterten Suchaufruf gefunden;
  15 Betriebsbeobachtungen in einem nativen Sammelrequest (statt 15 Einzelrequests).
  Drei Einzelkontrollen an Papierfabrik, Wasserpumpe und Bäckerei stimmen in allen
  Summary-Feldern überein. Compact und Full liefern identische Zusammenfassungen.
- Serialisierte strukturierte Antworten: compact 11.115, full 37.783 Zeichen
  (70,6 % weniger). Das ist der Vergleich der beiden Batch-Ausgabeformate,
  kein gemessener Vergleich zu 15 vollständigen Einzelantworten oder Netzwerkbytes.
- Exakte Vorlage, finished, productionOnly, Höhenbereich und Pagination kombiniert:
  zwei fertige Erfinder auf z6, offset=1 liefert den zweiten; Bereich [5,6)
  schließt beide aus. Baustellenfilter schließt die fertigen Erfinder aus.
- Insgesamt 126 Objekte ohne Wege gegenüber 424 mit PathSpec. Native Vorlage
  `Path` liefert 276 Treffer mit includePaths=true und null Treffer ohne.
  `Slope` ebenfalls als isPath beobachtet. Die zunächst angenommene Vorlage
  Path.Folktails existiert hier nicht; native IDs verwenden.
- Full liefert Geometrie, Compact geometry=null. Unbekannte ID neben vorhandener
  bleibt not_found; doppelte IDs -> invalid_argument, falsche Session -> stale_session.
- Papierfabrik kurz pausiert: active=false, consumer_inactive, kein
  consumer_not_powered. Vorher active=true und powered=false mit entsprechendem
  Blocker. Gebäude anschließend aktiviert und separat rückgelesen.
- Simulation während des Vergleichs pausiert an Tag 640, Stunde 19,96875.
  Keine Bauten, Abrisse oder Rohstofflöschungen. Pausieren löst regulär Personal;
  erneute Besetzung ist kein Bestandteil dieses Beobachtungspiloten.

Grenzen: live 15 IDs, nicht die Maximallast 32; keine Iron-Teeth-Liveabnahme.
Keine positive Baustellenprobe mangels passender Baustelle; Filterausschluss geprüft.
Der Supervisor bot den neuen Katalog bereits an, Codex übernahm ihn erst nach
Client-Neustart. Dafür war kein erneuter Build/Installationslauf nötig.

## Vertrag

`find_buildings(offset, limit, ...)` filtert nativ vor Pagination:

- `template`: exakte native ID, fraktionsunabhängig.
- `finished`: optional fertig/unfertig.
- `productionOnly`: optional nur vorhandene Manufactory-Komponenten, kein
  Nachweis laufender Produktion und keine vollständige Liste aller Produzenten.
- `includePaths`: standardmäßig false; PathSpec-Objekte einschließlich Treppen
  werden ausgeschlossen. Für Wegeübersichten ausdrücklich true setzen.
- `x/y/z/width/height/depth`: alle sechs gemeinsam oder keiner. Halboffener
  Rasterbereich über den Objektanker, kein Schnitt mit Gebäudevolumen.
  Koordinaten 0–4095, Ausdehnung 1–4096; Bereichsende höchstens 4096.
- `detail`: compact (Standard) oder full. Nur full ermittelt `geometry` mit
  Orientierung, Eingang und bis 512 belegten Zellen. null bedeutet weggelassen,
  nicht fehlender Eingang oder leeres Volumen.

Maximal 32 Treffer, GUID-sortiert. total/hasMore zählen gefilterte Treffer.
Seiten sind getrennte Beobachtungen. Das alte interne `objects`-Endpoint bleibt
für vorhandene Planer erhalten; der öffentliche Suchaufruf nutzt `building-search`.
Das native Such-Antwortformat ändert sich bewusst: Geometrie liegt unter
`geometry`, Wege sind standardmäßig ausgeschlossen. Benötigt Bridge 0.39.0.

`inspect_building_operations(ids, session, detail=compact)` liest 1–32 eindeutige
IDs in einem nativen Request und einem Spielthread-Durchlauf. Jede angeforderte
ID bleibt in Eingabereihenfolge enthalten: observed mit Summary oder not_found.
Fehlende Objekte sind keine leeren oder gesunden Betriebsbeobachtungen. Falsche
Session, doppelte IDs und falsche Parameter werden abgewiesen.

Summary enthält Vorlage, Fertigzustand, Pause, Personal, aktuelles Rezept und
Zutaten-/Brennstoffbereitschaft, Produktplatz, Kraft und assessment. Full ergänzt
`details` mit dem vollständigen Einzelbeobachtungsformat einschließlich Inventaren
und Statustexten. Es gibt keine 32 internen Einzel-HTTP-Aufrufe, keine Rezeptlisten
und keine zusätzliche Geometrieabfrage. Der Transport liest die Betriebsbelege
einmal und validiert sie vor Kompaktierung; internes Antwortlimit 2 MiB.
`nativeReads=1` zählt die fachliche Spielabfrage, nicht das bestehende MCP-Aktivitätslog.

Kraftdiagnose verwendet jetzt `MechanicalNode.Active`. Inaktive Verbraucher
liefern consumer_inactive als Warnung, keinen consumer_not_powered-Blocker.
Fehlendes Active bei älteren Bridges bleibt consumer_activity_unknown. Erst ein
aktiver Verbraucher mit gültigem Netz und powered=false belegt aktuelle fehlende
Versorgung; dies identifiziert noch keine Anschluss-/Windursache.
Einzel- und Sammelabfrage verwenden denselben Beobachter und Validator.

## Abnahmepilot nach menschlichem Gate

1. Produktionsgebäude mit einem gefilterten Suchaufruf finden; bei mehr als 32
   Treffern maximal eine weitere Seite, sonst Pilotumfang nicht ausweiten.
2. Bis 32 IDs mit einem Sammelaufruf lesen; höchstens drei Einzelkontrollen
   (Rezept, Personal, Zutaten, Kraft und assessment) vergleichen, Spiel pausiert.
3. Exakte Vorlage, Ankerbereich mit Höhenabgrenzung, unfertig/fertig und
   includePaths gegen gezielte Gegenproben prüfen. Full/compact vergleichen.
4. Eine unbekannte ID neben einer vorhandenen: Teilfehler bleibt sichtbar.
   Doppelte ID und falsche Session ablehnen. Inaktiven Verbraucher ohne erfundene
   Versorgungsblockade belegen; aktive Gegenprobe nur wenn vorhanden.
5. Aufrufzahl und Antwortgröße messen: N Einzel-Beobachtungen → ein nativer
   Batchrequest für dieselben N Ziele. Trefferabdeckung getrennt berichten.

Erfolg: stimmige Filter/Pagination, Einzel-/Sammelbelege identisch, sichtbare
Teilabdeckung, weniger Requests und kleinere Standardantwort. Bei fehlerhaftem
Kontrollfall stoppen und diagnostizieren. Keine autonome Koloniepflege oder
Testbauten erforderlich. Commit/Push erst nach Gate und bestandenem Livetest.
