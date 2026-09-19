# Kontrollierter Spielvalidator-Pilot — 0.4.0

Status nach Nutzerklärung: gebaut, 109 reguläre Tests bestanden, lokal gepackt;
nicht installiert oder live geprüft. Fähigkeitsanalyse hat Vorrang vor tatsächlichem Spielen.
Der folgende Ablauf ist ein vorbereiteter Nachweisplan, kein ausgeführter Spieltest.

Endziel bleibt reguläres Spielen per MCP. Nach erfolgreichem rein lesendem Raum-Pilot
folgt die Prüfung eines einzelnen Bauplatzes mit den Validatoren der laufenden Spielversion.
Dies ist weiterhin **kein Bauauftrag**. Keine Screenshots oder Eingabesimulation.

## Öffentlicher Ablauf

TemplateNameMapper -> PlaceableBlockObjectSpec -> PreviewFactory.Create -> eigene
Preview.Reposition -> BlockObjectValidationService.AreValid -> Hide/RemoveFromPreviewServices.
Höchstens zwei eigene verborgene Vorschauen werden pro Spielszene wiederverwendet;
der vom Spiel bereitgestellte Vorschau-Root gehört zur Szene. Keine EntityService-
Instanziierung, keine Place-Aufrufe, keine Freischaltung oder Fertigbau-Abkürzung.
PreviewFactory erhält TemplateInstantiator/RootObjectProvider; dessen öffentlicher
Konstruktor wurde anhand der lokalen DLL-Metadaten geprüft. Referenzmuster für Caching
und Verbergen: SharedPreviewRepository und BlueprintPreviewRepository im gepinnten
[Quellenkatalog](references/sources.json). Kein fremder Code oder Mod wird eingebunden.

## Schutzmechanismen

- Mod-Konfiguration `enableValidation` und MCP-Prozess `TIMBERBORN_ENABLE_VALIDATION`
  müssen beide ausdrücklich aktiv sein. Ohne MCP-Opt-in wird das Werkzeug nicht gelistet;
  ohne Mod-Opt-in wird die Route nicht ausgeführt.
- Nur POST ohne Body an `/agent-api/v1/site-validation`; keine GET-Ausführung,
  keine generische Methodenwahl. Loopback und vorhandene Bearer-Prüfung bleiben bestehen.
- Eingabe enthält die frische `meta.sessionId` einer vorherigen Beobachtung.
  Die Mod vergleicht sie direkt auf dem Spielhauptthread, bevor die Vorschau berührt wird.
- Nur Lodge.Folktails/Path, passende Fraktion, aktive Vorlage, reguläre Freischaltung,
  begrenzte Koordinaten/Rotation und höchstens 64 innerhalb der Karte liegende Zellen.
- Höchstens acht Validierungsversuche je Spielsitzung. Vor/nach Prüfung werden registrierte
  Entity-IDs und globale AllStock-Werte sämtlicher bekannter Güter verglichen.
- Preview muss tatsächlich IsPreview und nicht AddedToService sein. Bereinigung im finally.
  Ausnahme oder beobachtete Zustandsänderung sperrt weitere Versuche für diese Sitzung.
- Keine automatischen Wiederholungen bei Fehler/Timeout. MCP-Hinweise sind konservativ:
  nicht read-only, nicht idempotent und potentiell destruktiv, obwohl keine Platzierung erfolgt.

Bei Zustandsabweichung: `valid=null`, `noPersistentChangeObserved=false`, `sessionLocked=true`.
Keine automatische Rücknahme, kein erneuter Probeversuch; erst Ursache und Zustand klären.
Das Prüfpaar überwacht nicht jede beliebige interne Variable, Statistik oder jedes Ereignis.
Der separate Live-Pilot muss bestätigen, dass der Vorschaupfad in dieser Version geeignet ist.

`gameValidated=true` bedeutet: Die registrierten BlockObject-Validatoren wurden aufgerufen.
`valid=true` reserviert keinen Platz, keine Materialien und keine Arbeiter; es garantiert
keine Distriktanbindung, Lieferung, Bauzeit oder Fertigstellung. Fraktion/Freischaltung und
später die tatsächliche Platzierung müssen im Aktionsablauf frisch geprüft werden.

## Nächster Live-Schritt

Nach Neustart Version/Sitzung lesen. Kleinen bekannten Bereich strukturiert abfragen und
einen Standort mit vollständig freier Grundfläche wählen, nicht nur mit freier Ankerzelle.
Genau ein belegter und ein plausibel freier Standort über den Validator, maximal zwei
Versuche im ersten Pilot. Bei Ausnahme/Inkonsistenz sofort abbrechen. Kein Bauauftrag.
Ein vorheriger Gebäudepausenstatus wird nicht verändert.

Wenn beide Ergebnisse und Zustandswachen nachvollziehbar sind: regulären Platzierungspfad
über BlockObjectPlacerService/IBlockObjectPlacer entwickeln. EntitySetup.Builder.SetId
ist öffentlich verfügbar und kann eine gezielte Ergebniszuordnung ermöglichen; dies muss
am echten Platzierer nachgewiesen werden. Keine Platzierung allein aus Metadatensignaturen.
Für das beobachtete Wohnhaus braucht die Kolonie 12 Holz, aktuell waren 0 verfügbar.
Auftragserteilung und anschließende Materialversorgung/Fertigstellung getrennt behandeln.
