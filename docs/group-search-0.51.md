# Gruppenplanung mit Rückwärtssuche und Ausschlussflächen — 0.51

Status: Nach menschlichem Gate am 2026-10-12 mit Bridge 0.51.0 begrenzt live
abgenommen. Die folgenden Vertragsgrenzen bleiben bestehen; keine pauschale
Standort-/Katalog- oder vollständige Bauphasenschutz-Abnahme.

## Vertrag

Bestehendes `plan_region_buildings`, nur Gruppenmodus `templates`:

- Bis acht Wunschgebäude, einschließlich Wiederholungen.
- `searchBudget`: 1–32 native Gesamtvorschauen, Standard 32.
- `maxCandidates`: 1–4 unvorabgefilterte Kandidaten je Erweiterungszustand,
  Standard 3. Anders als 0.49/0.50 können frühere Entscheidungen zurückgenommen
  werden. Kandidaten stammen weiter aus der begrenzten Regionalsuche.
- Harte Grenze von 256 besuchten Suchzuständen/Kandidatenprüfungen. Kein neues
  Parallelverfahren und kein unbegrenzter Hintergrundlauf.
- `reservedAreas`: höchstens acht Objekte mit Pflichtfeldern `x,y,z,width,height,depth`.
  Ganzzahlige Rasterkoordinaten 0–4095; width/height 1–32, depth 1–8; gesamtes
  Volumen innerhalb 0–4095. Obergrenzen exklusiv. Beispiel für einen Streifen:
  `{"x":10,"y":10,"z":3,"width":1,"height":8,"depth":2}`.
  Werte aus der tatsächlichen Sitzung wählen, Beispiel ist kein Live-Bauort.
- `searchBudget` und `reservedAreas` sind im Einzelmodus `template` unzulässig.

Ausschlussflächen sperren sämtliche neuen nativen Blockeinträge der Zielgebäude
und Zugangshilfen, unabhängig von kompatiblen Belegungsschichten. Schon vorhandene
Wege darin dürfen weiter als Zugang dienen. Es wird keine Fläche im Spiel markiert,
keine Vegetation entfernt und keine dauerhafte Reservierung gespeichert. Die
Reservierung gehört zur konkreten Planung; spätere andere Aufträge kennen sie nicht.

## Suche und Bewertung

Deterministische Tiefensuche in der Reihenfolge der Wünsche. Kandidaten nach
Wegaufwand/Höhennähe/Koordinaten sortieren. Bei einem schlechten Ergebnis frühere
Standorte wechseln; durch einen Auslassungszweig können spätere Wünsche anstelle
eines blockierenden frühen Gebäudes berücksichtigt werden. Keine freie Permutation
aller Ausführungsreihenfolgen und keine globale oder vollständige Standortsuche.

Vergleich lexikografisch: mehr platzierte Wunschgebäude; dann weniger neue Path-
Teile im gesamten Plan; dann weniger zusätzliche Zugangsteile einschließlich Wege.
Bei Gleichstand bleibt der zuerst bestätigte Plan. Native Prüfung berechnet immer
die gemeinsame Zielgruppe samt Zugangshilfen neu. Identische Zielgruppen werden
innerhalb dieses pausierten Aufrufs wiederverwendet, nicht über Aufrufe hinweg.

Nur erfolgreiche native Vorschauen mit pilotEligible/restored und vollständigem
Plan werden Kandidaten für das Ergebnis. Transport-, Sitzungs- und Vertragsfehler
brechen ab, ohne Wiederholung. Budgetende liefert den besten bis dahin bestätigten
Plan. Eine nicht geprüfte Alternative ist nicht als unbaubar zu melden.

## Native Durchsetzung

`LargeProjectRequest` erlaubt das optionale kodierte `reservedAreas` nur für
`plan-access`. Mod und MCP teilen die Grenzen. Der native Zugangsplaner prüft volle
Geometrie der Zielgebäude, Treppen, Plattformen und Wege gegen alle Volumen.
Die Antwort bestätigt kanonischen `reservedAreasKey` und `reservedAreasClear`;
bei erfolgreicher Planung muss die Bestätigung vollständig sein. Fehlende oder
abweichende Bestätigung ist ein Vertragsfehler, keine Freigabe.

Ausführung bleibt unverändert: zurückgegebene Schritte und nativen planKey an
`run_large_project` übergeben; native Startprüfung bleibt wirksam. Ausführung
ändert oder ergänzt die bestätigte Geometrie nicht. Bestandsschutz, tatsächlicher
Bauzugang und fertiger Zugang bleiben getrennte Nachweise.

## Antworten

- `complete/partial/none` beschreibt Planung, nicht Bauabschluss.
- `selected`: ursprünglicher Wunschindex, Vorlage und Index im zurückgegebenen Plan.
- `unplaced`: nicht ausgewählte Wünsche, beobachtete Ablehnungsgründe und Prüfzahlen.
- `score`: Anzahl Wünsche, neue Wege und gesamte Hilfsteile.
- `search`: Strategie, Budget/Verbrauch, Cachetreffer, Rücksprünge und Stoppgrund.
  `globalOptimalityProven` bleibt false, auch wenn alle Wünsche passen.
- Compact fasst Versuche nach Wunsch/Zustand/Grund zusammen; Blockierer und native
  Belegungsschichten sowie betroffene Ausschlussvolumen bleiben sichtbar.
- `detail=full` enthält einzelne Versuche mit Vorschau-Belegen. Nur der ausgewählte
  finale Plan ist zur Ausführung bestimmt. Frühere Belege sind keine Bauaufträge.
- Shared-Read-Metriken und Regionsbelegung aus 0.50 bleiben erhalten.

## Gate und Pilot

Im menschlichen Gate abgesichert: Greedy-Vergleich, Rücksprung auf früheres Gebäude, besserer Teilplan
durch Auslassen, Bewertung nach Wegen/Hilfen, harte Suchlimits, acht Wünsche,
volle Grundfläche und Höhenunterschied bei Reservierung, unbekannte Geometrie,
fehlende native Bestätigung, Transport-/Session-/Abbruchfälle und Compact/Full.
Versionsfreigaben um 0.51 ergänzt; unbekannte Version bleibt synthetisch 99.0.0.

Abnahmepilot (durchgeführt, Ergebnis unten):

1. Status/Session/0.51 bestätigen. Kleine Gruppe mit engem gemeinsamem Baufeld:
   im Full-Beleg verworfene frühe Wahl und bessere bestätigte Gruppe nachweisen.
   Wenn der gewählte Livefall keinen Rücksprung benötigt, nicht als Rücksprungbeleg
   ausgeben; genau einen passenden Kontrollfall suchen.
2. Ausschlussfläche auf einem Zielgrundriss: Alternative oder none; kein Auftrag
   innerhalb des Volumens. Höhenkontrolle darf eine andere Ebene verwenden.
3. Ausschluss auf einer erforderlichen neuen Zugangsroute: native Planung führt
   außen herum oder lehnt ab. Neue Hilfsgeometrie gegen den Ausschluss kontrollieren.
4. Gewinnergruppe unverändert in einem Auftrag bauen lassen; Fertigstellung,
   tatsächliche Zugänge und Bestandsanschlüsse prüfen. Compact/Full fachlich gleich.
5. Nur Testobjekte und entbehrliche Testwege räumen; Rohstoffstapel erhalten,
   Endpause bestätigen. Bei falscher Reservierungs-/Zugangskontrolle stoppen.

Keine automatische Erweiterung auf großen Kolonieumbau. Bei unverhältnismäßigem
Pilotaufwand neue Entscheidung gemäß Projektregel, keine endlose Flächensuche.

## Zurückgestellte Experimente

Vorhandene Starter-/Budget-/NativeTools-Experimente vor dem Gate lokal bytegenau
unter `.local/deferred-agent-experiments-051-*` gesichert und diese vier Dateien
auf den veröffentlichten Stand zurückgeführt. Andere Hermes-Dateien bleiben lokal;
keine dieser Implementierungen ist Teil von 0.51. Nicht mitcommitten.

## Abschluss-Livepilot 2026-10-12

- Rücksprung real beobachtet: Vierergruppe zuerst mit 13 Hilfsteilen bestätigt,
  später früheres Gebäude umplatziert und Gewinner mit zehn Hilfsteilen
  (neun Wege/eine Treppe) gefunden. 32 native Vorschauen, 91 Suchzustände,
  stopReason=native_budget; keine globale Optimalität.
- Zwei Ausschlusszellen auf bisherigem Zielstandort und neuem Hauptzugang
  führten zu anderem Gebäudeort und alternativer Treppenroute. Kanonischer
  Reservierungsbeleg und reservedAreasClear=true vorhanden.
- Höhenkontrolle: ausgeschlossene obere Ebene verhindert unteren Bankplan nicht.
  Vollständig gesperrte Zielebene: none, null Schritte/Schlüssel, 16 gefilterte
  Kandidaten, null native Vorschauen. Enger erster Suchausschnitt lieferte keine
  Kandidaten und wurde nicht als Höhenbeleg gewertet.
- Zweiergruppe mit denselben Ausschlüssen: Bank, Holzfäller, Treppe und zwei Wege
  unverändert in einem Auftrag gebaut. Fünf Teile nach 3,65625 Spielstunden /
  10,7168 realen Sekunden fertig; automatische Pause. Frischer Abschluss meldet
  closeEligible/released=true und lostConnections=0. Vollständige native Geometrie
  aller fünf fertigen Teile überschneidet keine Ausschlusszelle.
- Compact/Full bei identischer Planung: 61.686/11.944 JSON-Zeichen, 80,6 % weniger;
  Schritte, planKey und Suchstatus identisch. Vollbelege nicht als Mehrfachauftrag
  ausgeführt. Budgetende bei acht Vorschauen sichtbar.
- Alle fünf Testobjekte einschließlich Treppe/Wege entfernt. Vollständige regionale
  Nachlesung: keine Test-IDs, Bestandswege und Rampe vorhanden, zwei zurückgewonnene
  Rohstoffstapel vorhanden. Kein Abrissaufruf auf Rohstoffstapel. Endpause bestätigt.

Achtfachumfang, synthetischer Greedy-Teilplan→Vollplan, Transport-/Vertragsfehler
und Suchzustandslimit stammen aus Gate-Tests; live wurden Zwei-/Drei-/Viererpläne
geprüft. Kein Achtfachbau und keine allgemeine Positionsoptimierung behauptet.
