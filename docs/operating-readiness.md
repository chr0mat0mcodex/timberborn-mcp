# Gemeinsame Betriebsdiagnose (begrenzt live abgenommen)

Automatische Auswahl und Startintegration werden separat ergänzt: [aktueller Entwurf](automatic-operating-readiness.md), Gate/Live noch offen. Die folgenden Nachweise gelten für die manuelle Auswahl.

## Abschluss 2026-10-18 — Gate und gezielter Livevergleich bestanden

Menschliches Gate: 28 Supervisor-, 1952 Unit- und 13 Integrationstests bestanden;
drei optionale Integration-Livetests übersprungen. Bridge weiterhin 0.74.0.
Nachabnahme um 00:53 Uhr Europe/Berlin: kein `access_unknown` bei allen vier
Gebäuden mit `unconnectedBlockerCount=0`. Vier einzelne Betriebsabfragen und
vier Zugangsabfragen bestätigen sämtliche verglichenen Felder des Sammelberichts.
Globale Personalreserve und Materialbestände separat identisch bestätigt:
fünf Arbeitslose, 33 Bretter, 40 Zahnräder, 260 Holz; Bauzentrum trocken, 4/4.
Testbedarf je 100 Bretter/Zahnräder ergibt 33/40 Prozent, ohne Startsperre.
Holz ohne ausgewählten Ernter bleibt ausdrücklich außerhalb der Auswahl unbekannt.
40 Güter vollständig; neun native Beobachtungen über rund 0,54 Sekunden.
Abschluss weiter Tempo 0, kein aktiver Lauf/ausstehender Tempoauftrag.

Abgenommen sind Auswahlaggregation, Quellenübereinstimmung und korrigierte
Warnungsableitung. Keine Spielmutation und kein Produktions-/Durchsatznachweis.
Überflutungspriorität, fehlende Komponenten, Sessionwechsel und Scanlimit sind
synthetische Gate-Nachweise, keine zusätzlich erzeugten Livefälle.
Frühere offene Gate-/Livevermerke unten dokumentieren die Entwicklungshistorie.


`inspect_operating_readiness` bündelt Bauzentrum, Überflutung, Personal,
Gebäudezugang und ausgewählte Materialproduktion vor längeren Bau- oder
Simulationsläufen. Rein lesend; keine automatische Aktion und keine Startsperre.
Die bestehende Bridge 0.74.0 genügt, ergänzt wird die MCP-Schicht.

## Aufruf

Aktuelle Session und Gebäude-IDs lesen. `constructionCenterId` bezeichnet das
für den Bau vorgesehene Zentrum; die Rolle ist vom Aufrufer gewählt, keine
native Bestätigung seiner Bauzuständigkeit. `producerIds` enthält bis sieben
weitere eindeutige IDs, auch Erntegebäude. Auswahl mit `find_buildings` und
`inspect_production_graph` vorbereiten. Das Zentrum darf nicht doppelt vorkommen.
`materials` enthält 1–8 eindeutige `{good, amount}`-Einträge; vollständigen
Bedarf einschließlich Wege/Treppen/Hilfsbauten übergeben. Nullbedarf (`amount=0`)
prüft Versorgung für Simulation ohne erfundene Deckungsquote. `reasoning` ist
wie bei den anderen Werkzeugen erforderlich.

Der Bericht enthält lokale Soll-/Ist-Besetzung, globale Personalreserve,
native Überflutungswerte (auch bei Pause), Zugangsbelege, globale Bestände,
Deckungsquoten und priorisierte nächste Schritte. Aktive Rezepte werden über
den Definitionsgraphen ihren Ausgaben zugeordnet; Erntegebäude bleiben
Kandidaten ohne Nachweis markierter, erreichbarer Ressourcen.

Überflutung hat Vorrang vor Personalmaßnahmen. Fehlendes Personal, Pause,
Zutaten/Brennstoff, Kraft und Produktplatz bekommen getrennte Hinweise.
Teilbesetzung und fehlender laufender Job beweisen keinen Produktionsstillstand.
Materialmangel bleibt Entscheidungsinformation: Bau darf auf Lieferung warten.

## Grenzen

- Auswahl statt Kolonievollscan: kein passender Produzent in der Auswahl bedeutet
  unbekannte Versorgung, niemals bewiesene Abwesenheit in der Kolonie.
- Maximal vier Güterseiten à 32, insgesamt höchstens 15 native Leseaufrufe.
  `goodsComplete`/`goodsNextOffset` benennen die Abdeckung. Nicht gelesener Bestand
  bleibt null. Fehlende IDs bleiben `not_found`; keine Zugangsabfrage dafür.
- Gleiche Session und Bridgeversion, aber mehrere Beobachtungszeitpunkte.
  Zeitmarken stehen in `observations`; `atomic` und `productionProven` sind false.
  Sessionwechsel, veränderte Güterzahl oder doppelte Seiten werden abgewiesen;
  kein Retry. Gleichbleibende Anzahl garantiert keine atomare Sicht.
- Globale Arbeitslose sind keine Bezirksreserve, globaler Bestand keine
  Lieferzusage oder Reservierung. Gebäudezugang beweist keinen Baustellenzugang.
  Bauzuständigkeit, Projektgeometrie und tatsächlicher Durchsatz bleiben getrennt.
- Defekte/abgewiesene native Antworten brechen die Diagnose ab, statt eine
  scheinbar gesunde Teilantwort zu erzeugen.

## Abnahme

Synthetische Tests vorbereitet: Überflutung vor Personal, informative Unterdeckung,
fehlende Produzenten, Scanlimit/Unknown, Session-/Versions-/Katalogwechsel,
Abbruch, Nullbedarf und strikte Eingaben/Katalog. Im ersten menschlichen Gate
bestanden alle 1948 Unit-Tests und der Build. Der Integrationslauf scheiterte an
der nicht ergänzten Reader-Katalogliste; diese und die optionale Live-Katalogliste
sind korrigiert. Das zweite Gate bestand; der folgende Livevergleich erforderte eine gezielte Nacharbeit.

Nach menschlichem Gate und `live bereit`: bestehendes Zentrum und einen passenden
Produzenten wählen, einen gemeinsamen Bericht lesen und mit Einzelabfragen
Überflutung, Besetzung, Rezept und Bestand vergleichen. Unterdeckung muss als
Information erscheinen; fehlende Kandidaten dürfen keine globale Abwesenheit
behaupten. Bei widersprüchlichem Kontrollfall stoppen, keine längere Simulation
starten. Überflutungs-Priorität ist nur dann live belegt, wenn ein passender
Zustand vorliegt; sonst bleibt sie synthetischer Nachweis.

## Livevergleich 2026-10-18 — Korrektur vorbereitet, erneutes Gate offen

Gate zuvor bestätigt: 28 Supervisor-, 1948 Unit- und 13 Integrationstests,
drei optionale Tests übersprungen. Nach Client-Neustart Reader verfügbar.
Rein lesender Pilot bei Tempo 0: Bauzentrum und drei Produzenten. Sämtliche
Betriebsfelder (Überflutung, Pause, Personal, Rezept, Kraft, Assessment) und
Zugangsbelege stimmten mit vier einzelnen Betriebs-/Zugangspaaren überein;
globale Personalreserve und drei Bestände ebenfalls einzeln bestätigt.
40 Güter vollständig in zwei Seiten; neun Beobachtungen in rund 0,50 Sekunden.

Bauzentrum trocken, 4/4 besetzt; fünf globale Arbeitslose. Ein Sägewerk pausiert
und unbesetzt; das zweite sowie die Zahnradfabrik mit fehlender Kraft und ohne
Produktplatz. 33 Bretter, 40 Zahnräder, 260 Holz. Testbedarf je 100 Bretter und
Zahnräder (kein Bauauftrag) korrekt als 33/40 Prozent und informative Fehlmenge
67/60 ausgewiesen. Holz ohne ausgewählten Ernter bleibt Versorgung außerhalb
der Auswahl unbekannt, Nullbedarf hat keine Deckungsquote. Kein Produktionsbeweis.

Fehler im abgeleiteten nächsten Schritt: `access_unknown` trotz ausdrücklich
fehlender optionaler Anschlussblocker (`unconnectedBlockerCount=0`). Die Quelle
unterscheidet nun diesen belegten Sonderfall von unbekannter Anzahl bzw. fehlenden
Eingangsbelegen. Vier Regressionen vorbereitet, noch nicht ausgeführt.
Keine Simulation oder Spielmutation; frische Abschlussabfrage weiter Tempo 0,
kein aktiver Lauf/ausstehender Tempoauftrag. Keine Überflutung im Pilot:
Überflutungs-Priorität bleibt synthetisch geprüft. Featureabschluss und Commit
stehen bis erneutem Gate und gezieltem Livevergleich der Warnung aus.
