# A12/N02: unterer Zielweg an gedrehter Bestandstreppe

Stand 2026-10-18: gezielter Livepilot auf dem abgenommenen 0.74.0-Stand nach N01
bestanden. Keine Code-, Test-, Konfigurations- oder Paketänderung; kein neues Gate nötig.

## Umfang

Eine fertige gerade Folktails-Treppe mit Cw270-Drehung erhält einen neuen unteren
Zielweg. Native Geometrie, Distriktverbindung, tatsächlicher Bauarbeiterzugang
und Fertigzugang werden getrennt geprüft. Ein kleines Lager hinter dem Zielweg
dient dem tatsächlichen Bauzugangsnachweis. Ein seitlicher Weg muss bei freier,
gültiger Geometrie ohne Anschluss bleiben. Kollision allein ist keine negative
Anschlusskontrolle. Keine Abnahme aller Drehungen, Fraktionen oder Treppentypen.

Bestehende Grundlage: [0.71](releases/0.71.0.md),
`AccessRouteSearch.StraightStairLowerCandidate` und
`LargeProjectAccessPlanning` mit nativer Zielwegvorschau. Die Regressionen decken
die vier geometrischen Drehungen synthetisch ab; N02 ergänzt den fehlenden Livefall.
Das zuvor erfolgreiche N01-Gate gilt für den unveränderten Laufzeitstand:
2256 Unit-, 19 Integrations-, 13 Paket- und 35 Supervisor-Tests bestanden,
drei optionale Integrationstest-Livefälle übersprungen. Kennungen live bestätigt.

## Testaufbau und verworfene Kontrollen

- Dichte Cw180-Umgebung: bestehender unterer Weg und mehrere Nachbarwege würden
  den Anschlussnachweis über alternative Wege verfälschen; nicht verwendet.
- Cw90-Treppe: nach Freistellen zweier unterer Wegzellen nicht mehr am Distrikt.
  Damit kein von oben gespeister Testaufbau. Beide kostenfreien Wege regulär
  wieder gesetzt, 2/2 frisch fertig und zugänglich, Treppe wieder verbunden;
  Journal freigegeben. Dieser Fall ist kein N02-Erfolgsnachweis.
- Gewählter Aufbau: vorhandene Cw270-Treppe bei (18,2,2), zunächst isoliert.
  Oberer Anschluss über acht neue kostenfreie Wege bei y=2/z=3 bis zum Bestand.
  Sechs Eichen regulär geräumt und Pflanzmarkierungen entfernt. Nach ergänztem
  Arbeitsweg und begrenzten Zeitfenstern Korridor frei; alle acht Wege fertig
  und zugänglich, Hilfsjournale freigegeben. Bestandstreppe nun verbunden.
- Unterer Zielweg (17,2,2), Seitenkontrolle (18,1,2), kleines Lager (16,2,2).
  Auch dort zunächst Eichen. Seitliche erste Vorschau wegen geometry_collision
  abgelehnt und restauriert; ausdrücklich kein Anschlussnegativbeleg.
  Drei konkrete Bäume zur regulären Entfernung markiert, Pflanzmarkierungen
  entfernt. Kein erzwungenes Löschen und keine Wiederholung der Räumaufträge.
  Nach Arbeits-/Ruhefenster tatsächlich geräumt; Lauf danach gezielt abgebrochen,
  Endpause bestätigt. Keine Save-Manipulation oder Sonderlöschung.

## Anschluss- und Bauzugangsbelege

- Seitliche Kontrolle nach Räumung: native_valid, previewRegistered=true,
  previewEntranceConnected=false, pilotEligible=false, restored=true. Keine
  verlorene Bestandsverbindung; Seitenweg nicht beauftragt.
- Automatische Zugangssuche für das Lager (16,2,2), Rotation Cw270: genau ein
  generierter Weg (17,2,2). Gewählte Quelle ist die Bestandstreppe (18,2,2),
  drei Suchzustände insgesamt und eine Zielwegvorschau. Beide Bauteile nativ
  gültig und verbunden, Lagerarbeitszelle road_reachable, restored=true,
  lostConnections=0. Keine behauptete direkte Instant-Kante aus der Vorschau.
- Vor Auftrag: drei Stämme Gesamtkosten, Hilfswege kostenfrei; 260 global
  verfügbar (rund 8667 Prozent Deckung). Besetzter unpausierter Holzfäller mit
  laufendem Auftrag beobachtet; das ist kein eigenständiger Produktionsbeweis.
- Unveränderten Zwei-Teil-Plan einmal beauftragt. Reale Lagerbaustelle:
  buildersReachable=true, constructionAccess=observed, Eingang frei und zugänglich.
  Neuer unterer Weg bereits fertig und native Distriktzugehörigkeit true.
  Frische Projektdiagnose: beide Objekte vorhanden, zunächst Materialwarten,
  kein Zugangsproblem und keine verlorenen Bestandsverbindungen.
- Nach vier Spielstunden: 2/2 Teile frisch fertig und zugänglich, keine fremden
  Baustellen oder verlorenen Bestandsanschlüsse. Fertiges Lager separat geprüft:
  entranceBlocked=false, entranceInaccessible=false, gültiger Zugang vorhanden,
  native Distriktdistanz 39. Journal regulär freigegeben, Spiel pausiert.
  Kein Nachweis eines Lagerbetriebs oder exakter Bruttoliefermengen.

## Erfolg und Stopp

Vor dem Bau: freie Prüfzellen, verbundene Bestandstreppe, native gültige untere
Verbindung und seitliche Nichtverbindung bei bestätigter Wiederherstellung.
Kosten einschließlich Hilfswegen vor Auftrag prüfen. Tatsächliche Baustelle
und Fertiggebäude separat lesen; Bestandsverluste separat ausweisen.
Bei widersprüchlicher Kontrolle gezielt diagnostizieren, keine Fallserie starten.
Material-/Arbeitszeitwarten ist kein Beleg eines Wegfehlers.

## Bewertung und Grenze

N02 ist für diesen Cw270-Folktails-Fall abgeschlossen. Die vorhandene Umsetzung
funktioniert; ein Refactoring oder eine Lockerung der Zugangsprüfung ist daraus
nicht begründet. Nutzen: Der bisher offene untere Zielweg ist jetzt bis zur realen
Baustelle und zum Fertigzugang belegt, mit isolierter seitlicher Negativkontrolle.
Aufwand: kein neuer Code, aber merkliche Standort- und Räumvorbereitung (neun
Eichen, acht obere Hilfswege, anschließend Weg plus ein Lager). Nachweis bleibt
gezielt; weitere Drehungen, Fraktionen, Reload und allgemeine Bauphasensicherheit
sind nicht abgenommen. Keine weiteren A12-Pakete bearbeitet.

Learning: Vor einer Anschlusskontrolle vollständige Objektüberlappungen lesen;
Gebäudesuche und Terrainfreiheit allein schließen Vegetation nicht aus. Eine
bereits verbundene Treppe kann von unten gespeist sein und eignet sich dann ohne
vorbereiteten oberen Anschluss nicht als Nachweis eines neuen unteren Zielwegs.
