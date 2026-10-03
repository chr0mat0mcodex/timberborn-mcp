# B-Minimalabschluss: konservative Baustellensperre

0.32.1 nach menschlichem Skript-Gate am 2026-10-03 live bestanden.
Vom Nutzer freigegebener Ansatz: unbewiesenen hypothetischen Builder-Schutz nicht
erraten, sondern neue Bauaktionen bei unabhängigen offenen Baustellen ablehnen.

## Umfang

- Ebenen-Lagerpilot, sämtliche Treppen-/Plattformmodi und Einzelplatzierungen
  prüfen vor Vorschau/Ausführung den realen Blockobjektbestand.
- Jede unabhängige Baustelle blockiert, auch pausiert oder bereits unerreichbar.
  Entfernung und vermutete Unbetroffenheit ergeben keine Ausnahme.
- Unvollständige Inventur (>4096 reale Blockobjekte), nicht initialisierte Objekte
  oder unklare Fertig-/Unfertigzustände blockieren ebenfalls.
- Eigene bereits beauftragte Schritte dürfen beim Warten/Bestätigen unfertig sein.
  Vor der nächsten Platzierung darf jedoch keine offene Baustelle mehr bestehen.
  Ebenen-Wege werden bereits erst fertig und verbunden bestätigt; alle vertikalen
  Modi warten nun auf fertige Vorgänger und Pause (300 reale Sekunden je Phase).
- Neue unabhängige Baustelle während eines laufenden Projekts stoppt Folgeschritte.
  Teilstände bleiben erhalten; kein Retry/Rollback. Alte Aktions-ID liefert weiterhin
  nur gespeicherten Beleg, sie autorisiert keine weitere Platzierung.

Dies schließt B nur als technisch geprüfte Ausschlussregel für unabhängige
Baustellen, nicht als allgemeines Bauen in einer Kolonie mit Baustellen.
`constructionCovered=false`, `constructionPreflightProven=false` und unbekannte
Vorschauwerte bleiben bestehen. Reguläre Baufreigabe wird nicht gelockert.
Ist-Nachprüfung garantiert keine sichere hypothetische Bauphase und ersetzt sie nicht.
Andere Eingriffe (Abriss, Gelände, Simulation) sind nicht durch diese Bausperre
geschützt; neue Baustellen aus Nutzer-/anderen Modaktionen lassen sich nicht verhindern.
Nach completed erfolgt keine dauerhafte Überwachung der fertigen Kolonie.

## Kleines Test-Gate

1. Bestehende echte unabhängige Baustelle: neue Projektanfrage ablehnen,
   keine neue Entity, keine Vorschauwirkung; bestehender Zugang unverändert.
2. Ohne unabhängige Baustellen: begrenzten eigenen Ablauf durchführen und
   Fertigstatus vor Folgeschritt sowie tatsächlichen Zugang getrennt rücklesen.

Keine Suchserie, keine API-Neuerfindung. Eine falsche Kontrolle stoppt die Abnahme.
Synthetische Tests: bekannte/unklare Zustände, vollständige Inventur, eigene Wartephase,
nachträgliche unabhängige Baustelle, keine Folgemutation, Versionsbindung neuer Wartezustände.
Neue Version erst nach menschlichem Gate und feature-spezifischem Livetest committen.

## Live-Abnahme

Eine kleine Lagerbaustelle regulär angelegt: unfertig, builder-erreichbar,
Distriktdistanz 19. Neuer ebener Lagerpilot mit separatem freien Kandidaten,
neuer Plattformpilot und direkte Path-Platzierung jeweils state_conflict.
Objektzahl vor/nach identisch (986), ursprünglicher Builderzugang und Zugangszellen
unverändert. Keine Entity aus den abgelehnten Anfragen.

Nach tatsächlicher Fertigstellung der Kontrollbaustelle Plattformpilot angenommen.
Eigene Treppenbaustelle builder-erreichbar und korrekte Wartephase; weitere
Plattform-/Wegschritte erst nach fertigen Vorgängern. Alle fünf Objekte separat
fertig rückgelesen, beide oberen Wege distriktverbunden, Kontrolllager weiterhin
mit freiem Eingang und Distanz 19. Begrenzte Simulationen pausiert ohne Überschreitung.

B damit für die konservative Ausschlussregel geschlossen. Pausierte/unerreichbare
fremde Baustellen, spätere Fremdbaustelle während eines Projekts, Inventurgrenzen
und neue Wartebelege der Nicht-Plattformmodi synthetisch, nicht zusätzlich live geprüft.
Allgemeiner Builder-Vorschau-Schutz bleibt unbewiesen; unknown nicht gelockert.
