# B06.1 — öffentliche Save-API und Objektidentität nach Reload

Stand 2026-10-18. **Begrenzte Vorprüfung abgeschlossen; eigener Save-Beleg noch
nicht implementiert oder abgenommen.** Öffentliche Anbindung vorhanden und zwei
Objekt-IDs nach einem regulären Save/Reload live erhalten. Der vollständige
B06-Persistenz-/Abgleichnachweis bleibt offen. [Entwurf](project-reload-reconciliation-design.md).

## Methode und Umfang

Unveränderter abgenommener Stand Bridge 0.74.0, Quellcheckpoint `1279561`.
Vorhandenes Metadaten-Lesewerkzeug in Release-Fassung verwendet; keine Kompilierung,
keine Tests, kein DLL-Methodenaufruf und keine private Runtime-Reflection.
`MetadataLoadContext` liest Typ-/Methodensignaturen, nicht deren Laufzeitwirkung.
Lesender Zugriff auf die installierten Bibliotheken mit Dateifreigabe erfolgreich;
die im Entwurf vermerkte Zugriffslücke ist damit geklärt.

Genau zwei bereits vorhandene Teile des fertigen N10-Aufbaus ausgewählt: ein
SmallPile.Folktails und ein Path. Kein Neubau, kein Abriss, keine Save-Manipulation.
Der Mensch speicherte den pausierten Stand regulär und lud genau diesen Save neu.
Vorher/nachher ausschließlich bestehende MCP-Leser verwendet. Keine Spiel- oder
Sessionkennungen, lokalen Benutzerpfade, Saves oder Rohlogs im Repository abgelegt.

## Öffentliche Signaturen — direkt aus installierten Metadaten

| Zweck | Bestätigte öffentliche API |
| --- | --- |
| Moddaten speichern | `ISaveableSingleton.Save(ISingletonSaver)` |
| Eigenen Datenbereich wählen | `ISingletonSaver.GetSingleton(SingletonKey)` → `IObjectSaver`; `SingletonKey(string)` |
| Datenbereich optional laden | `ISingletonLoader.TryGetSingleton(SingletonKey, out IObjectLoader)`; außerdem `GetSingleton` |
| Typisierte Felder | `PropertyKey<T>(string)`, `ListKey<T>(string)`; `IObjectSaver.Set`, `IObjectLoader.Get/Has`; unter anderem int, string, Guid, Vector3Int und entsprechende Listen |
| Lebenszyklus | `ILoadableSingleton.Load()`, `IPostLoadableSingleton.PostLoad()` |
| Gemeinsame Weltstruktur | `SerializedWorld.Entities()` und `Singletons()`; `SerializedWorldFactory.Create()` sowie `Create(IEnumerable<ISaveableSingleton>)` |
| Gespeicherte Objektidentität | `SerializedEntity(Guid, string)`, `Id`, `TemplateName`; `InstantiatedSerializedEntity` verbindet Entity und SerializedEntity |
| Laufende Objektidentität | `EntityComponent.EntityId`, `Initialized`, `Deleted`; `EntityRegistry.GetEntity(Guid)` und `Entities` |

Wichtig für den Entwurf: In dieser installierten API heißt die optionale
Singleton-Abfrage `TryGetSingleton`; kein ungeprüftes historisches `HasSingleton`
verwenden. Für den kleinen Beleg wäre ein namespaced SingletonKey mit Formatnummer
und begrenztem Payload über öffentliche Property-Schlüssel möglich. Das ist ein
Implementierungsvorschlag, kein ausgeführter Speicher-/Ladeversuch.

Die öffentliche `GameSaver`-API besitzt ebenfalls reguläre Save-Methoden, wurde
aber nicht aufgerufen. Die vorhandene MCP-Schnittstelle bietet sie nicht an;
für B06 ist kein neues Fern-Save-/Load-Werkzeug nötig.

`SerializedWorld` umfasst sowohl Entities als auch Singletons: ein gemeinsamer
Speicherweg ist strukturell vorgesehen. **Signaturen beweisen weder atomare
Snapshot-Erstellung noch Save-Erfolg, Lade-Reihenfolge oder vollständige Registry-
Bereitschaft.** Insbesondere reicht die Existenz von PostLoad allein nicht als
Beleg, dass dort bereits alle benötigten Entitäten initialisiert sind.

Referenzvergleich: Das [Singleton-Beispiel des Modautors datvm](https://datvm.github.io/TimberbornMods/BuffDebuff/stepbystep.html)
verwendet ISingletonLoader und SingletonKey für gespeicherte Moddaten. Es dient
nur als Musterbeleg, nicht als Versions- oder Kohärenznachweis; seine zusätzliche
Buff-Bibliothek wird nicht übernommen. Die offiziellen
[Coding-Grundlagen](https://github.com/mechanistry/timberborn-modding/wiki/Coding-basics)
verweisen auf die lokalen Spiel-DLLs. Für B06 ist kein Fremdmod nötig; die vorhandene
Projektdatei referenziert Timberborn-DLLs bereits ohne deren Auslieferung.

### Identifikation der untersuchten Bibliotheken

SHA-256, nur zur Reproduzierbarkeit dieser Metadatenprüfung, keine pauschale
Kompatibilitätsfreigabe:

| Bibliothek | SHA-256 |
| --- | --- |
| Timberborn.WorldPersistence.dll | `031243eb971a7d1868c6747a0b5c4b1dcffbb826c0c49fc18e083c673d54cc26` |
| Timberborn.Persistence.dll | `147c234d96fd27c8ea92c337d46242a75109c6b171bb3ffabfb118beefd7059c` |
| Timberborn.WorldSerialization.dll | `da88c5355ff2347db3794f700cee8d34b000709dfe6fb0b74093f060f916f74f` |
| Timberborn.SingletonSystem.dll | `82554dd17c8cd68ed6972dd7ad99e60c5750591b7c815c7ecc7dc8f9ffff4ed1` |

## Livebelege — ein Save/Reload, zwei Teile

| Kontrolle | Tatsächliches Ergebnis |
| --- | --- |
| Vor Reload | Beide exakten IDs gefunden, erwartete Vorlage/Position und finished=true; Spiel Tempo 0, kein aktiver Lauf/Tempobefehl. |
| Nach menschlich gemeldetem Reload | Neue Bridge-Session; Bridgeversion, MCP-Build-ID und Katalog-ID unverändert. |
| Beide Objekte mit neuer Session lesen | Beide IDs erhalten. Gesamte fachliche `inspect_building.data`-Antwort je Objekt identisch zur Baseline, einschließlich Vorlage, Position und Fertigstatus; beim Lager auch Distriktzuordnung. |
| Alte Session für lesende Objektabfrage | `stale_session`, keine automatische Wiederholung. |
| Alte Projekt-actionId mit neuer Session lesen | `project_not_found`; reale Objekte vorhanden, altes Ledger nicht vorhanden. Genau die B06-Lücke bestätigt. |
| Gleiche Lagerabfrage wiederholen | Identische fachliche Antwort; kein Bau-/Startaufruf gesendet. |
| Abschluss | Tempo 0, kein aktiver Lauf, kein ausstehender Tempobefehl; Spielzeit wie vor Reload. |

Das belegt ID-Erhalt für diese beiden realen Objekte in diesem einen normalen
Reload. Keine allgemeine Garantie für alle Objekttypen, Save-Konvertierungen,
Fremdmods, neue Instanzen am gleichen Ort oder kopierte/ältere Saves.
Der vorhandene Gebäudeleser liefert keine Orientierung; deren Erhalt ist in
B06.1 nicht separat live bewiesen. Die gespeicherten Planrotationen ersetzen
keine frische Beobachtung nach Reload. Fehlende/uninitialisierte/falsch zugeordnete
Objekte wurden hier nicht injiziert oder erzeugt; sie gehören zur späteren Matrix.
Keine Replay-Sperre des noch nicht implementierten B06-Werkzeugs behauptet.

## Bewertung und verbleibender Stoppunkt

Nutzen hoch: der öffentliche Weg ist konkret, und vorhandene Weltobjekte bleiben
im kleinen Livefall identifizierbar, obwohl das Auftragsledger verloren geht.
Aufwand klein: drei gezielte Metadatenabfragen plus Bibliothekskennungen, zwei
Ausgangsobjekte, ein menschlicher Reload und gezielte Rück-/Negativkontrollen.
Keine weitere Stichprobenserie nötig, kein größerer Kolonieaufbau.

B06.2 bleibt ausdrücklich **nicht freigegeben**. Falls beauftragt, zuerst nur den
Save-Roundtrip eines minimalen eigenen Belegs einschließlich optional fehlendem
Datenbereich, eindeutiger Entity-Zuordnung und Ladebereitschaft implementieren.
Dabei Welt-/Beleg-Kohärenz im vorgesehenen pausierten Capture-/Save-Ablauf und
Format-/Größenfehler klären. Erst nach diesem Nachweis Capture-/Abgleichwerkzeuge
und die alte-ID-Sperre vollständig ausbauen. Ein Serializer-/Ladeproblem stoppt
diesen Schritt; kein stiller Wechsel auf externe Dateien oder private APIs.
Compiler-/Testnachweise und Installation ausschließlich im menschlichen Gate.

Der Roundtrip eigener Moddaten, Verhalten bei beschädigten/älteren Belegen und
beim Entfernen der Mod bleiben ungetestet. Deshalb kein vollständiges „Save-sicher“
und keine automatische B06-Implementierungsfreigabe. Agentenanleitung geprüft:
keine neue nutzbare Fähigkeit, keine fachliche Änderung nötig; keine Spielskills.
