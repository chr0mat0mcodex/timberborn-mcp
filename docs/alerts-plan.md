# Zukunftsfeature: Spielmeldungen mit betroffenen Zielen

## Kurzprüfung 2026-09-20

**Über öffentliche APIs grundsätzlich machbar.** Geprüft wurden die öffentlichen
Metadaten der lokal installierten Timberborn-1.1.2.4-Bibliotheken. Keine Implementierung,
keine Änderung am Spiel, kein Live-Abgleich. Voraussichtlich keine zusätzliche Mod nötig.

Für die Statuswarnungen im Alert-Panel gibt es:

- `StatusSubject.ActiveStatuses` und `StatusInstance.StatusSubject`: aktive Zustände
  und Bezug zum betroffenen Objekt. StatusSubject erbt von BaseComponent; damit kann
  der bestehende Komponenten-/Entity-Zugriff als Ausgangspunkt für IDs und Ort dienen.
- `StatusInstance.StatusDescription`, `AlertDescription`, `IsActive`, `ShowAlert`,
  `IsVisible()`, `IsPriorityStatus` und `IsNotifying`: Texte und Anzeigefilter.
- `IStatusAggregator.GetVisibleStatuses(string)` und
  `StatusAggregator.GetVisibleStatusesCount(string)`: sichtbare Gruppen und Anzahl.
- `DynamicStatusAggregator.TryGetStatusData(string, out StatusData)`: zusätzliche
  dynamische Angaben Count, Value und StatusWarningType.
- `AlertStatusSubjectSelector.SelectNextSubject(string)`: die öffentliche Auswahl
  eines nächsten betroffenen Objekts ist belegt. Für die reine Abfrage nicht aufrufen;
  der Agent soll die Zielliste direkt lesen, ohne Kamera oder Auswahl zu verändern.

Die konkrete Bedeutung der String-Gruppenschlüssel, Verzögerungen/Sichtbarkeit und
Zuordnung zu jeder UI-Zeile sind durch Signaturen allein noch nicht vollständig belegt.
StatusWarningType enthält None/Short/Infinite und darf nicht ungeprüft als
Schweregrad wie „harmlos/kritisch“ ausgegeben werden.

Daneben existieren getrennte Meldungssysteme:

- `Notification`: Description, Subject (Guid), Cycle und CycleDay;
  `NotificationEventArgs.Notification` transportiert eine Meldung.
  Ein Ort lässt sich nur bei noch auflösbarem Subject bestimmen. Die geprüfte
  NotificationBus-Schnittstelle bietet Post, aber keinen öffentlichen Historienabruf.
- `QuickNotificationEventArgs`: Text und IsWarning, kein Zielbezug in der geprüften
  Schnittstelle. Für diese Meldungen keine Koordinaten erfinden.

Damit ist noch nicht garantiert, dass **jede** Meldung unten links aus demselben
System stammt oder identische Details liefert. Historische Ereignisse und aktuelle
Probleme getrennt behandeln. Referenzbasis: [offizielle Quellen](references/README.md)
und öffentliche Spielassemblies AlertPanelSystem, StatusSystem, NotificationSystem,
QuickNotificationSystem. Keine privaten UI-Felder oder Reflection erforderlich, solange
die genannten Daten für den späteren Pilot ausreichen.

## Vorgeschlagene spätere MCP-Funktionen

1. `inspect_alerts(offset, limit)` liefert aktuelle Meldungsgruppen: sitzungsgebundene
   Alert-ID, Quellsystem, Text, beobachtete Anzahl, native Statusflags und vorhandene
   dynamische Werte. Beobachtungszeit und Abdeckungsgrenzen ausdrücklich melden.
2. `inspect_alert_targets(alertId, session, offset, limit)` liefert **alle** betroffenen
   Ziele seitenweise: Entity-ID, Objektart/Vorlage, konkreten Status und aktuelle
   Position. Gebäude über BlockObject-Koordinaten, bewegliche Figuren über einen noch
   zu prüfenden öffentlichen Positionszugang. Welt- und Rasterkoordinaten unterscheiden.
3. Nicht mehr vorhandene oder nicht lokalisierbare Ziele ausdrücklich als
   unresolved/removed/locationUnavailable kennzeichnen. Kein Gebäudezentrum als
   vermeintlichen Aufenthaltsort eines Bibers verwenden. Auflösung erfolgt frisch;
   zwischen Liste und Detailabruf kann eine Meldung verschwinden.

Ein globaler stabiler Statuscode ist in den geprüften Signaturen nicht belegt.
Lokalisierte Texte deshalb nicht als dauerhafte technische IDs verwenden. Falls
Ereignismeldungen nur per Event beobachtbar sind: kleiner begrenzter Sitzungspuffer,
klare Angabe „seit Bridge-Start“, keine behauptete vollständige Vergangenheit und
keine Save-/Log-Auswertung als stiller Ersatz.

## Spätere Abnahme

- Zwei bis drei repräsentative Warnungen, darunter mehrere betroffene Ziele sowie
  ein Gebäude und nach Möglichkeit ein Biber. Liste, Anzahl, Detail und Position
  mit den Spielangaben abgleichen; Nutzerbestätigung genügt, keine Screenshots.
- Aufgelöste Warnung, verschwundenes Ziel, mehrere Ziele und Neuladen prüfen.
- Abfrage darf keine Kamera, Auswahl, Pause oder Warnungsquittierung verändern.
- Nach kleinem Pilot entscheiden, ob zusätzliche Alert-Fragmente separat nötig sind.

Status: **Zukunftsplanung / öffentliche Signaturen geprüft**. Der laufende nächste
Schritt bleibt die Live-Abnahme des generischen Bauzugangs 0.14.0.
