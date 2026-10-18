# A05-Pilot: lesbare Produktionszielprüfung

Stand 2026-10-18: menschliches Gate und gezielter Livetest bestanden.
Bridge unverändert 0.74.0. Kein Agent-Build oder Testlauf.

## Umfang und Entscheidung

Nur `ProductionRunContract.Validate` im nativen Backend wird aufgeteilt:
Ziele prüfen, Ergebnis prüfen, Ziele mit dem Startauftrag abgleichen, Diagnosebelege
prüfen. Die vier privaten Methoden bleiben in derselben Datei. Kein gemeinsames
Validierungsframework, keine neue Abhängigkeit, keine Versions- oder Werkzeugmigration.
Die Gebäudeproduktionsprüfung und der Dispatcher bleiben unverändert.

Reihenfolge, Kurzschlussverhalten, Nullbehandlung, Grenzwerte und Exceptiontexte
bleiben erhalten. Unbekannte Beobachtung ist kein erreichtes Bestandsziel;
Zeitablauf und Bestandszielerfüllung bleiben getrennt. Startquittungen müssen die
angeforderten Ziele bestätigen; Status/Cancel tragen keine solchen Anfrageziele.

Quellvergleich bestätigt die sieben Ablehnungsbedingungen nach reiner Bezeichner-
und Whitespace-Normalisierung. Der Diagnose-Präsenzzweig ist als frühe Rückkehr
formuliert; Berechnung des Zielerfolgs und Diagnosezuordnung separat abgeglichen.
Compiler- und Testnachweis anschließend durch das menschliche Gate erbracht.

## Bestandenes menschliches Gate

23 neue Charakterisierungsfälle in `ProductionRunContractTests`: sechs zulässige
Beobachtungsvarianten, 13 Ablehnungen mit exaktem Fehlertext, drei ungültige
Diagnosezeitpunkte und Start-/Statusunterscheidung. Mehrfachfehler sichern die
Reihenfolge ab; Nullziele/-diagnosen dürfen keine NullReferenceException erzeugen.
Bestehende `ProductionRunTests` sichern unter anderem kombinierte Ziele,
begrenzte Diagnosebelege und strukturierte MCP-Ziele bis zum Transport ab.

Gate: 13 Paket-, 28 Supervisor-, 2197 Unit- und 13 Integrationstests bestanden;
drei optionale Livefälle übersprungen. Alle 23 neuen Charakterisierungsfälle
bestanden. Release-Builds ohne Warnungen/Fehler, fünf installierte Paketdateien
per SHA-256 bestätigt.

## Gezielter Livebeleg am 2026-10-18

- Verbundener Supervisor gestartet (`running=true`, `gate=false`), native Bridge
  0.74.0 und aktuelle Session bestätigt. Ausgangstempo 0, kein aktiver Lauf.
- Frischer Bestand 33 Bretter; Rezeptgraph bestätigt Bretterherstellung aus Holz
  im Sägewerk. Einmaliger for-Lauf mit Ziel 100 Bretter, Zeitobergrenze 0,25
  Spielstunden, Tempo 7 und maximal 30 Echtzeitsekunden gestartet.
- Terminal `completed/production_time_limit`, exakt 0,25 Spielstunden ohne
  Überschuss; `targetReached=true`, aber `productionTargetsReached=false`,
  eingefrorener Zielbestand 33/100. Zeitablauf wird nicht als Zielerfüllung gewertet.
- Leichter Status ohne Diagnose akzeptiert. Einmalige Rücklesung mit `diagnose=true`
  lieferte passende Bretterdiagnose, zwei Produzenten und gültigen Diagnosezeitpunkt;
  `diagnosticRequests=1`, `diagnosisState=returned`. Beobachtete Befunde: Kraftdefizit,
  fehlende Besetzung, fehlende Ausgabekapazität und pausierter Produzent.
- Laufbericht mit bestätigter Pause; frische Simulationsabfrage: Tempo 0,
  kein aktiver Lauf und kein ausstehender Tempoauftrag. Keine Startwiederholung.

Grenze: Live belegt sind gültige Ziel-/Zeitstoppquittungen und Diagnosebelege
für diesen begrenzten Fall. Zielerfolg, Forschungsziele, ungültige Quittungen und
Fehlerprioritäten sind synthetisch abgesichert; keine allgemeine Produktionsabnahme.

## Nutzen und Aufwand

Die kleine Aufteilung ist sinnvoll: Prüfabsicht und feste Reihenfolge stehen im
Einstieg, die vier fachlichen Abschnitte sind getrennt lesbar. Quellvergleich,
Gate und begrenzter Livepilot zeigen keine Vertragsänderung. Eine Produktionsdatei
wurde geändert; vier private Methoden und mehr Textzeilen sind der überschaubare
Preis. 23 Charakterisierungsfälle sichern zuvor schwächer belegte Null-/Fehler-
und Prioritätsfälle. Kein zusätzliches Framework oder Abhängigkeit, kein
Laufzeitgewinn beabsichtigt oder gemessen.
Gesamt-A05 bleibt offen. Weitere Fachprüfungen auf ausdrücklichen Nutzerwunsch
vorerst zurückgestellt; keine automatische Umformatierung. A03/A04-Migrationen
bleiben ebenfalls zurückgestellt.
Agenten-Spielanleitung geprüft: öffentliche Parameter, Ergebnisbedeutung und
Bedienabläufe unverändert, daher keine fachliche Anpassung. Keine projektbezogenen
Spielskills vorhanden.
