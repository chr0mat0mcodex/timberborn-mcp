# Betriebsdiagnose pro Gebäude — 0.9.0

Status: implementiert, installiert und begrenzt live bestätigt. Die bestehende Abfrage
`inspect_building(id, session)` erhält `details.operations`; kein neues Werkzeug,
keine neue Schreibaktion und keine Fremdmod-Abhängigkeit.

## Beobachtungen

- pauseComponentPresent und pause: Paused sowie IsPausable() der öffentlichen
  PausableBuilding-Komponente. Fehlende Komponente ergibt pause=null.
- workplaceComponentPresent: Komponente vorhanden, auch wenn das Objekt unfertig ist.
- workplace nur bei fertigen Objekten mit Workplace: DesiredWorkers,
  NumberOfAssignedWorkers, MaxWorkers, Understaffed, Overstaffed und
  AnyWorkerHasJobRunning(). Sonst null, nicht künstlich null Arbeiter.

Sollbesetzung, zugewiesene Arbeiter und laufender Arbeitsauftrag unterscheiden.
Ein zugewiesener Arbeiter kann gerade nicht arbeiten; ein laufender Auftrag ist
noch kein fertiges Gut. Material-, Lager-, Fällgebiets- und andere Produktionsblocker
sind noch nicht erfasst. Spiel-Flags werden gelesen, nicht aus Zahlen nachgebildet.
Vorübergehende Überbesetzung wird erhalten und nicht als fehlerhafte Antwort verworfen.

Öffentliche Signaturen gegen lokale Timberborn-1.1.2.4-Bibliotheken geprüft.
Community-WorkplaceSettings dient nur als good reference: seine SetDesiredWorkers-
Methode ist in der aktuellen öffentlichen Workplace-Signatur nicht vorhanden.
Die reguläre Sollbesetzung ist inzwischen über IncreaseDesiredWorkers/DecreaseDesiredWorkers implementiert und live geprüft; siehe [Personalsteuerung](workplace-staffing.md).
Keine private Reflection, Kopie fremden Codes oder zusätzliche Mod-DLL.

## Kompatibilität und Abnahme

Alte Bridge-Versionen liefern operations=null (unbekannt). Ab 0.9.0 wird der Vertrag
verlangt. Komponentenverfügbarkeit und Zahlen werden im MCP-Backend geprüft.
Arbeitsplatzzustand unfertiger Objekte wird bewusst nicht als betriebsfähiger Arbeitsplatz
ausgewiesen. Bestehende Bau-/Distriktabfrage bleibt unverändert.

Nach Installation begrenzter Lesepilot: Holzfällerflagge und Distriktzentrum anhand
frischer Entity-IDs abfragen, außerdem Lodge bzw. Baustelle und Path als Gegenproben.
Soll-/Ist-/Maximalbesetzung, Gebäudepause und Nullwerte auf plausiblen Komponentenbezug
prüfen. Keine Personaleinstellung, Pause oder Flächenmarkierung für diesen Lesetest.
Erst danach gezielte native Personal-/Betriebssteuerung als nächsten Schritt bewerten.

Live: Distriktzentrum 2/2/4, Holzfällerflagge 1/1/1 (Soll/Ist/Max), beide aktiv.
Lodge unfertig ohne Workplace, Path ohne Workplace und canPause=false trotz
vorhandener PausableBuilding-Komponente. JobRunning kann während Spielpause true
bleiben; kein Beleg für aktuell fortschreitende Produktion.
