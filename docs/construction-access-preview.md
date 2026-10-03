# Etappe B: Baustellenzugang unter Vorschau

## Stand

Agent Bridge 0.31.0: menschliches Skript-Gate durch Bereitmeldung bestätigt,
installierte Version live geprüft. Positivkontrolle bestanden, negative
Baustellenkontrolle nicht nachgewiesen; Etappe B nicht abgeschlossen. Keine allgemeine Baufreigabe;
`constructionCovered` bleibt false.

## Öffentliche API und begrenzter Versuch

Die lokal geprüften öffentlichen Signaturen bieten tatsächliche Erreichbarkeit
über `ReachableConstructionSite.IsReachableByBuilders()`. In den geprüften APIs
wurde kein entsprechender direkter Builder-Vorschauaufruf gefunden.
`INavigationRangeService` bietet jedoch `GetRoadSpillNodesInRange(Vector3)` und
`GetRoadSpillPreviewNodesInRange(Vector3)`. `ConstructionSiteAccessible` liefert
die gültigen Zugänge einer bestehenden Baustelle.

Der Versuch vergleicht, ob die Baustellenzugänge in der Reichweite der einzigen
fertigen Distriktzentrale liegen: tatsächliche Navigation, leere Vorschau,
vollständige Bauvorschau und beide Zustände nach deren Entfernung. Die tatsächliche
Builder-Abfrage kontrolliert Ausgangs- und Endzustand. Die Annahme, dass Reichweite
und Builderzugang im Testfall zusammenpassen, benötigt einen Livetest.

Der offizielle [Architekturüberblick](https://github.com/mechanistry/timberborn-modding/wiki/Timberborn-architecture)
erklärt Komponenten, Lebenszyklus und Service-Injektion, aber keinen
Builder-Vorschauvertrag. Keine private Reflection, kein Ersatzgraph und keine
direkte Änderung des echten Navmesh.

## Befund und Grenzen

`roadProtection.constructionAccessPreview` enthält Status, Grund, Baselinevergleich,
Wiederherstellung, Verlustzahl und pro Baustelle Zugänge und Vergleichswerte.
`observed` bedeutet nur: Baseline passt zur Builderkontrolle und die geprüften
Werte sind nach der Vorschau wiederhergestellt. Es bedeutet nicht safe. Ohne
Baustellen, bei Sonderkomponenten, überschrittenen Grenzen, abweichender Baseline
oder nicht auswertbarer API bleibt es unknown.

Beobachteter Reichweitenverlust blockiert den Gesamtprüfbericht und die
Entwicklungspiloten. Gemessene fehlgeschlagene Wiederherstellung sperrt die Sitzung.
Keine Verluste ist kein vollständiger Nachweis. Distriktverlustzähler und affected
behalten ihre Bedeutung; Baustellenbefunde stehen separat in items.

Genau ein fertiger Distrikt, höchstens 32 Baustellen, 64 Zugänge pro Baustelle und
32768 Reichweitenknoten. Erweiterte Sonder-Erreichbarkeit wird ausgeschlossen.
Bestehende Baustellen unter fertiger Vorschau sind kein simulierter neuer Bauzustand.
Bei Joint-Projekten wird nur die vollständige Vorschau geprüft, nicht jedes
Wegpräfix. Neue Baustellen, Lieferwege und vollständiger Bauphasenschutz bleiben offen.

## Test-Gate

1. Mensch beendet Timberborn und führt das Vorbereitungsskript aus.
2. Nach `live bereit`: Version und Pause prüfen; falls nötig mit dem bestehenden
   kleinen Lagerpilot eine echte Baustelle erzeugen.
3. Baseline gegen tatsächliches buildersReachable prüfen. Freie Kontrollvorschau
   und Vorschau auf möglichem Zugang/Engpass vergleichen; zuerst zwei bis drei Fälle.
4. Bei Baselineabweichung, nicht auswertbarer API oder erfolgloser Kontrolle stoppen
   und Ursache klären. Keine Sperre lockern, keine breite Suchschleife oder
   stillschweigende Ersatzarchitektur. Bei Erfolg genaue Abdeckung dokumentieren.

Erwarteter Nachweis: tatsächlicher Baustellenzugang und Vorschauvergleich sind
getrennt; freie und zugangsgefährdende Kontrollen liefern nachvollziehbare Befunde,
Vorschauen vollständig zurückgenommen. Etappe C folgt nach Auswertung dieses Versuchs.

## Livebefund 2026-10-03 und Stopp

Sauber geladenes Testsystem ohne Baustellen: gemeinsame Lager-/Wegvorschau meldet
korrekt unknown/no_existing_construction_sites. Bestehender Entwicklungspilot legt
einen Anschlussweg und eine echte kleine Lagerbaustelle an; Auftrag und realer
Builderzugang separat bestätigt. Das bedeutet nicht fertiggestelltes Lager.

Vier Einzelvorschauen mit dieser Baustelle: geometrisch ungültiger benachbarter
Weg, gültiger freier Weg, Lodge-Sperrvorschau am Hauptweg und abschließend eine
breite ungültige Zentralesperrvorschau im Bereich der acht Baustellenzugänge.
Baselinevergleich und Rücknahme jeweils bestanden; direkter Builderzugang davor
und danach true. Freie Vorschau ohne Verluste. Beide Gebäudesperrvorschauen melden
elf verlorene Distriktverbindungen, aber die Reichweitendiagnose bleibt während
der Vorschau true/lostSites=0. Das kann ohne weiter geprüfte Zugangsgeometrie und
Semantik nicht als bestandene negative Baustellenkontrolle gewertet werden.

Versuch hier gestoppt, keine Suchserie und kein Bauphasenschutz behauptet.
Vorschauen entfernt; Simulation weiterhin pausiert und Spielzeit unverändert.
Die angelegte Baustelle und ihr Anschlussweg bleiben als Testfixture vorhanden.
Keine neue Baufreigabe, kein Abschluss von B und kein Start von C. Nächste
Entscheidung: begrenzte Klärung der tatsächlichen Vorschau-Zugangs-/Reichweitensemantik
oder alternativer öffentlich gestützter Prüfweg. Weitere Experimente benötigen
nach diesem erfolglosen Negativpilot ein neues ausdrückliches go.

## Freigegebene Detaildiagnose 0.31.1

Nach gezielter Ursachenprüfung vom Nutzer ausdrücklich freigegeben. Menschliches
Skript-Gate durch Bereitmeldung bestätigt; installierte 0.31.1 und Detailausgabe
live geprüft. Unter `details` werden
für genau eine Baustelle mit höchstens acht Zugängen fünf Zustände ausgegeben:
before, previewBefore, during, after, previewAfter. Mehr Baustellen oder Zugänge
liefern eine explizit unbekannte Detaildiagnose, keine still abgeschnittene Auswahl.
Die bisherige begrenzte aggregierte Reichweitendiagnose bleibt separat bestehen.

Pro Zelle: rangeContains, connected, roadConnected, existingOccupied,
candidateCoversCell, candidateOccupied und candidateOccupation. Verbindung über
öffentliche AreConnectedInstant/Preview und AreConnectedRoadInstant/Preview, mit
der Distriktzentralenkoordinate als ausgewiesenem Ursprung. Keine Entfernung,
Reichweitenbegrenzung oder tatsächliche Builderfahrt aus diesen Boolwerten ableiten.
Die realen Baustellenzugangspositionen bleiben bewusst unverändert gespeichert.

existingOccupied prüft reale Objektbelegung, nicht Terrain oder vollständige
Begehbarkeit. Kandidatenfelder stammen aus den tatsächlichen Vorlagenblöcken der
aktiven Vorschau; bei Joint-Projekten einschließlich Anschlusswegen. Belegung ist
kein Blockadenurteil: ein Weg kann eine belegte und zugleich begehbare Zelle sein.
Die Zustände außerhalb during enthalten keinen Kandidaten. Vorher-/Nachher-
Vergleich umfasst auch Verbindungen und reale Objektbelegung. Gemessene fehlende
Detailwiederherstellung sperrt über den bisherigen Wiederherstellungsbefund die
Sitzung. Baselineabweichung und nicht auswertbare Details bleiben sichtbar unknown.

Gate: menschlicher Skriptlauf, anschließend ein Lagerbaustellenfall mit freier
Vorschau und der bisherigen breiten Sperrkontrolle. Erwartet werden acht getrennte
nachvollziehbare Befunde, korrekte Kandidatenbelegung, gleiche Baseline und
Wiederherstellung. Erfolg darf auch eine klare Erklärung der voneinander
abweichenden Reichweiten-/Verbindungsbefunde sein; kein erfundener Zugangsverlust.
Bei fehlenden Daten oder unklarer negativer Kontrolle nach diesen zwei Fällen
stoppen und Ursache auswerten. Keine Baufreigabe, weiterhin kein Abschluss von B.

## Detail-Livebefund 2026-10-03

Sauberer Ausgangsstand ohne Baustellen. Der vorhandene kleine Lagerpilot legt
genau einen Anschlussweg und eine echte Lagerbaustelle an; Auftrag und direkter
Builderzugang separat bestätigt. Danach genau zwei Kontrollvorschauen.

Freier Weg: gültig; genau eine Kandidatenzelle mit Belegungsart Path, andere
Zugänge ohne Kandidatenbelegung. Acht detaillierte Zugänge: sechs im Reichweitenfeld,
zwei erhöhte Zugänge außerhalb. Breite ungültige Zentralesperrvorschau: alle acht
Zugänge durch Kandidatenblöcke belegt, obere zwei mit All, untere sechs mit
Bottom/Top/Corners/Path/Middle. Trotzdem unveränderte Reichweitenmitgliedschaft;
elf Distriktverbindungen verloren, kein aggregierter Baustellenverlust behauptet.

Alle acht Verbindungswerte sind bereits vor jeder Vorschau false und bleiben
false, während der reale Builderzugang true ist. Der Vergleich vom gewählten
Zentralenursprung ist damit keine geeignete positive Zugangskontrolle. Ursache
(Start-/Zielkoordinaten oder Abfragesemantik) noch nicht nachgewiesen. False darf
nicht als tatsächliche Unerreichbarkeit der Baustelle interpretiert werden.
details.status=observed bezeichnet nur vergleichbare Werte und Rücknahme, nicht
eine erfolgreiche Verbindung oder bestätigten Baustellenschutz.

Beide Detail-/Aggregat-Baselines und Rücknahmevergleiche bestanden; keine
Sitzungssperre. Reale Erreichbarkeit unabhängig erneut true, Pause und Spielzeit
unverändert. Testfixture bleibt vorhanden. Detailausgabe und Grenzen damit
praktisch nachgewiesen; Etappe B insgesamt weiterhin offen. Keine weitere
Suchserie. Nächster fachlicher Schritt: native Start-/Zielgültigkeit und geeigneten
positiven Verbindungsbezug prüfen, bevor diese Abfrage Zugangsschutz tragen darf.
