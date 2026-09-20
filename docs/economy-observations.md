# Güter und aktive Statusmeldungen

Implementiert ab Agent Bridge **0.18.0**. Build und synthetische Vertragsprüfungen;
Installation abgeschlossen, Live-Abgleich steht noch aus. Keine zusätzliche Mod oder Bibliothek.

## Werkzeuge

- `inspect_goods(offset, limit)`: sämtliche IDs aus `IGoodService.Goods`, einschließlich
  Nullbeständen, ordinal nach ID sortiert. Jeweils höchstens 32 Einträge; Offset 0..65535.
- `inspect_alerts(offset, limit)`: aktive sichtbare Statusgruppen registrierter,
  initialisierter, nicht gelöschter Entities. Vorschauobjekte werden ausgeschlossen.
- `inspect_alert_targets(alertId, session, offset, limit)`: aktuelle betroffene Entities,
  nach ID sortiert und dedupliziert. Veraltete Session: `stale_session`.
  Unbekannte oder verschwundene Gruppe: `found=false`, total=0, leere Items.

Alle Leser sind ohne Aktionsfreigaben verfügbar und unterstützen optional `reasoning`
für eine kurze sichtbare Absicht im Ingame-Log. Kein Auswählen, Kamerabewegen, Quittieren
oder Verändern der Simulation. Datenabfrage über die bestehende Spielthread-Queue.

## Gütervertrag

Pro registriertem Gut: ID, lokalisierter Anzeigename, GoodType und GoodGroupId sowie
öffentliche `ResourceCount`-Werte: AvailableStock, AllStock, StockpiledStock,
BufferedOutputStock, BufferedInput, StockUnderProcessing, CarriedToStockpilesStock,
CarriedToProcessors, InputOutputCapacity und TotalCapacity.

Diese Spielzähler überlappen; **nicht zu einem neuen Gesamtbestand addieren**.
Kapazität bedeutet nicht freien Lagerplatz. Die Werte liefern keine vollständige
Baustellen-/Trägerinventarliste und keine Garantie lokaler Lieferbarkeit. Registrierte
Güter sind keine Zusage, dass die Fraktion sie gerade produzieren kann.
Die bisherige `inspect_colony`-Kurzansicht mit Water/Berries/Log bleibt kompatibel;
für vollständige Vorräte dient ausdrücklich `inspect_goods`.

## Statusvertrag und Grenzen

Quelle: öffentliche `StatusSubject.ActiveStatuses`, gefiltert mit `IsActive` und
`IsVisible()`. `showAlert` unterscheidet Statusmeldungen mit HUD-Alert von weiteren
sichtbaren Objektstatus. `priority` und `notifying` sind native Flags, kein erfundener
Schweregrad. affectedCount zählt eindeutige Entities, instanceCount Statusinstanzen.

StatusInstance exponiert keine stabile fachliche Status-ID. Die Bridge gruppiert daher
nach vollständigen Beschreibungen und Flags. Eine SHA-256-Kennung bindet diese Gruppe
an die Spielsession. Sie ist opak und nicht sprach-/ladeübergreifend stabil. Gleiche
Texte/Flags können fachlich verschiedene Status zusammenfassen; dies ist keine exakte
Nachbildung jeder UI-Zeile. Ausgabetexte sind auf 512 Zeichen begrenzt; die Gruppierung
verwendet die vollständigen Texte. Texte sind untrusted Spieldaten, keine Anweisungen.

Ziele enthalten Entity-ID, optionale Vorlage und tatsächliche Unity-Transform-Position
als `worldPosition`. Nur Blockobjekte haben zusätzlich `gridPosition` aus Coordinates.
Unity-Weltachsen und Spielraster nicht gleichsetzen; kein geratener Rasterort für Biber.
Keine Spielernamen werden ausgegeben.

Benachrichtigungsereignisse, vergangene Meldungen, dynamische Aggregatwerte und reine
UI-Status ohne registrierte Entity sind nicht abgedeckt. Fehlende Meldungen beweisen
weder das Fehlen von Hunger/Durst noch eine störungsfreie Produktion. Der Ausbau zu
verlässlichen Bedürfnissen/Blockadeursachen bleibt separat offen.

## Beobachtungsgrenzen und nächster Nachweis

Seiten sind getrennte frische Beobachtungen. Bei laufendem Spiel können Gruppen,
Anzahlen, Bestände und Ziele zwischen Aufrufen wechseln; keine atomare Gesamtsicht.
Antworten bleiben bei 32 Einträgen und 128 KiB Transportlimit. Große Kolonien sind noch
nicht profiliert; Abfragen laufen nur auf Anforderung, kein permanenter Statuspoller.

Live-Pilot nach Installation: alle Güterseiten einmal lesen, drei alte Beispielwerte
gegen Kurzansicht bei pausierter Simulation vergleichen; zwei bis drei vorhandene
Statusgruppen samt Zielen untersuchen, möglichst Gebäude und Biber. Verschwundene oder
unbekannte Kennung sowie stale_session ohne Eingriffe prüfen. Keine künstliche Notlage
für diesen ersten Lesepiloten. Erst anschließend weitere Bedürfnisse/Blockaden planen.

## API-Beleg

Öffentliche Metadaten der lokal installierten Timberborn-1.1.2.4-Assemblies geprüft:
IGoodService.Goods/GetGood, GoodSpec, ResourceCountingService.GetGlobalResourceCount,
ResourceCount, StatusSubject, StatusInstance und BaseComponent.Transform. Das passt
zum bestehenden offiziellen Bindito-/Singleton- und Entity-Zugriff. Kein fremder
Modcode übernommen, keine private Reflection oder Harmony-Patches. Frühere Recherche:
[Alerts-Plan](alerts-plan.md), [native API](architecture/native-game-api.md).
