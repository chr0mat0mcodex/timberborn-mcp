# Bretterketten-Vorprüfung — begrenzt live abgenommen

N10-Pilot abgeschlossen: Negativkontrolle null Zyklen; nach kleinem Lager-/Kraftaufbau
drei native Produktionszyklen in fünf Spielstunden. Ausgang 13→1 Bretter, neues
Lager 0→14, altes Lager 20→20. Ausgabeentlastung und Lagerzufluss beobachtet;
keine einzelne Quelle-Ziel-Lieferung instrumentiert. Kraftrad bei pausiertem
Windrad mit 58 Leistung gegenüber 50 Bedarf beobachtet, keine Dauerleistungsgarantie.
[Nachweis und verbleibende Liefergrenze](plank-production-transport-pilot.md).

`inspect_plank_chain(session, districtId)` ist ein rein lesendes Werkzeug für
Holz → Sägewerk → Bretterlager. districtId vorher aus nativer Bezirkszuordnung
lesen; keine Zuständigkeit aus räumlicher Nähe ableiten.

## Ablauf und Ergebnis

1. Vor einem Produktionslauf die Vorprüfung für den betroffenen Bezirk lesen.
2. `supply` zeigt automatische Auswahl, Abdeckung, Holz-/Bretterbestände und
   Betrieb der ausgewählten Holzlieferanten/Sägewerke. Mengenbedarf ist hier 0:
   Versorgungsprüfung, keine Baukostenrechnung oder Mindestbestandssperre.
3. `recipes` enthält native Bretterrezepte mit Zutaten, Ausbeute und Zeit.
   `sawmills` enthält zusätzliche aktuelle Inventar-, Kraft- und Engpassbelege
   der ausgewählten Sägewerke, einschließlich geeigneter Quellen/Produktziele.
4. `findings` nach Priorität bearbeiten: Überflutung zuerst, danach belegte
   Betriebs-/Zugangsblockaden, offene Versorgungs-/Lagerfragen und Messung.
   Jeder Befund enthält stage, target, code, severity und einen konkreten nextStep.
5. Nach der Entscheidung ausdrücklich kurz simulieren, mit Sägewerk-IDs und
   Produktionsmessung; native Zyklen, Zutaten und Ausgabe gegen Einzelabfragen
   prüfen. Der Reader startet selbst weder Lauf noch Ausbau.

Geprüft werden Pause, Personal, Arbeitszeit, Zutatenbereitschaft, Kraftversorgung,
Produktplatz, geeignete öffentliche Quellen bzw. Lager und deren Erreichbarkeit.
Volle Ausgabe trotz erreichbarem Produktziel führt zuerst zu Prüfung von
Abtransport, Personal und Lagerpriorität. Ein inaktiver Kraftverbraucher beweist
keinen defekten Anschluss. Globale Holzknappheit ist ein Hinweis, kein Startverbot.
Nicht gewähltes Bretterrezept wird eigens benannt; keine automatische Umstellung.

## Grenzen

Automatische Auswahl wie bei inspect_operating_readiness_auto: maximal 128
Gebäude, 16 Produzentenkandidaten, sieben ausgewählte Produzenten. Zusätzlich
ein Definitionsabruf und höchstens ein diagnostischer Betriebsbatch für bis
zu sieben Sägewerke. Native Lager-/Wegsuche behält ihre eigenen Grenzen und
complete-/unknown-Belege. Alle Aufrufe sequenziell, ohne Warten/Retry, gleiche
Sitzung und Bridgeversion; Zeitmarken zeigen die nicht atomare Beobachtung.

Die vollständigen Auswahl- und Engpassbelege bleiben im Ergebnis erhalten.
Fehlende Treffer bei begrenzter Suche bedeuten unbekannt, keine kolonieweite
Abwesenheit. Globale Vorräte beweisen keine lokale Lieferung, erreichbare
Lager mit Platz keinen tatsächlichen Abtransport. Holzfäller als Kandidaten
beweisen weder erreichbare Fällmarkierungen noch dauerhaft nachwachsendes Holz.
Keine transitive Ausbauplanung, keine automatische Handlung, keine Materialsperre,
keine Budgetverlängerung. productionProven bleibt false bis zu separaten Messungen.

## Automatische Einbindung vor Starts

Menschliches Gate und begrenzte Liveabnahme der Integration bestanden.
Der separate Reader und die automatische Einbindung haben eigene Nachweise unten.

Der bestehende MCP-Startbeobachter ergänzt `operatingPreflight[].plankChain`,
wenn die ermittelten Bedarfe `Plank` enthalten. Auch Nullbedarf aus einer
Produktionsprüfung zählt: Es gibt keinen Mindestbestand als Auslöser.

- Bau: jeder Strukturstart (`start_large_project`, `run_large_project`,
  `run_large_project_repair`), auch interne Starts von Bauabläufen. Kosten aller
  tatsächlichen Schritte einschließlich Zugangshilfen bestimmen den Bretterbezug.
- Simulation: `run_simulation_for` ab sechs Spielstunden und jeder
  `run_simulation_until`. Güterziele, aktuelle Rezepte der Bezugsgebäude oder
  die bestehende allgemeine Versorgungsauswahl bestimmen den Bezug.
- Ohne Bretterbezug kein Zusatzscan. Kurze for-Läufe, Status und Cancel bleiben
  ohne automatische Vorprüfung. Fehlende Kostendefinitionen bleiben im
  Betriebsbericht unbekannt; daraus wird kein erfundener Bretterbedarf abgeleitet.

`plankChain.state=observed` enthält unter `report` denselben Diagnosevertrag wie
der separate Reader, insbesondere priorisierte `findings` mit `nextStep` und
Zeitmarken. Compact und Full behalten die Zusatzdiagnose. Sie wird vor dem
Starttransport erhoben und gemeinsam mit dessen Antwort geliefert. Wer vor dem
Auftrag selbst entscheiden möchte, verwendet weiterhin den separaten Reader.

Unklare Bezirke liefern `district_unknown` und einen konkreten Klärungsschritt;
keine automatische Bezirkswahl nach Nähe. Transport-/Datenfehler liefern
`unavailable`, Zeitablauf `budget_exhausted`, jeweils mit `nextSteps`.
Die bereits erhobene Betriebsdiagnose bleibt dabei erhalten. Beide Diagnosen
teilen dasselbe Fünf-Sekunden-Budget; kein neues Budget pro Zusatzscan und kein
Retry. Die Bretterkette nutzt ihre bestehende begrenzte Auswahl und kann diese
erneut lesen; Beobachtungen sind nicht atomar. Session-/Versionskonflikte und
Abbruch durch den Aufrufer bleiben Fehler und verhindern den Starttransport.

Materialmangel und Betriebsblockaden sind Hinweise: keine pauschale Startsperre,
kein automatischer Ausbau oder Eingriff in Rezepte/Personal/Lager. Native
Startprüfungen bleiben maßgeblich; Laufparameter, Bauplan und Zeitbudgets werden
unverändert übertragen. Ein Diagnosefehler rechtfertigt keinen zweiten Start.

## Liveabnahme der automatischen Einbindung 2026-10-18

Menschliches Gate: 2025 Unit- und 13 Integrationstests bestanden, drei optionale
Integration-Livefälle übersprungen; Release-Build ohne Fehler.

Simulationspilot: sechs Spielstunden, Tempo 7, maximal 30 Echtzeitsekunden,
Ziel 100 Bretter bei 33 verfügbar. Start trotz 33 Prozent Deckung und blockierter
Produzenten. Automatische Kette state=observed im kompakten Startbericht;
Auswahl, sämtliche findings und beide Sägewerksberichte identisch zum separaten
Reader. Vollständige Betriebs-/Inventar-/Kraft-/Engpassbelege beider Werke stimmen
mit jeweils eigener Einzelabfrage überein. Native Beobachtungen 01:03:23.501 bis
01:03:24.503 UTC liegen vor dem Startbeleg 01:03:24.579 UTC.
Abschluss production_time_limit nach exakt sechs Spielstunden, 17,72 Sekunden,
overshootHours=0, Ziel weiter unerfüllt, Pause bestätigt. Kein automatischer Retry.

Baupilot: sechs Plattformen mit je sechs Brettern, insgesamt 36 bei 33 verfügbar
(91,7 Prozent Deckung). Erste Fläche wegen Pflanzenbelegung verworfen; Vorschauen
vollständig zurückgenommen. Freie Fläche separat erhoben, gemeinsame native
Vorschau aller sechs Teile gültig, pilotEligible/restored. Entwicklungspilot,
keine allgemeine Bauarbeiter- oder Liefergarantie aus der Vorschau.
Einmaliger run_large_project mit Tempo 7, einer Spielstunde und 30 Sekunden
Höchstbudget. Automatische Kette observed im Compact-Startbericht; Auswahl,
findings und Sägewerksberichte erneut identisch zu frischem separatem Reader und
Einzelabfragen. Beobachtungen 01:05:27.366 bis 01:05:28.342 UTC vor Startbeleg
01:05:28.477 UTC. Tatsächliche Kosten im Startbericht: 36 Bretter, kein Restbedarf.

Alle sechs Baustellen bestätigt, keine Ablehnung wegen Fehlmenge. Stopp mit
game_time_limit nach exakt einer Spielstunde, 2,95 Sekunden, overshootHours=0,
Pause bestätigt. Frische Diagnose: sechs unfertige Baustellen mit Materialwarten,
kein beobachteter Zugangsverlust. Einzelabfrage einer Baustelle bestätigt sechs
Bretter Kosten, leeren Baustellenvorrat und null Bau-/Materialfortschritt.
Die Baustellen bleiben bestehen, Projektledger blocked/unreleased. Keine
Fertigstellung behauptet und keine zusätzliche Laufzeit zum Fertigbauen gestartet.

Folgelesung nach Diagnose: Zielzeit, Zeitbudgets und Endzustand unverändert.
Frische Simulation: Tempo 0, kein aktiver Lauf, kein ausstehender Tempoauftrag.
Keine automatische Änderung an Personal, Rezept, Lager oder Produktionsanlagen.
Konkrete Maßnahmen bleiben Pause/Personal prüfen, Kraftversorgung bei 0/50
herstellen sowie Bretterlager auf Auswahl, Annahmemodus und freien Platz prüfen.

Grenzen: Fehlendes Material relativ zu Bestandsziel und Baukosten live belegt;
kein erfolgreicher Produktionszyklus oder vollständiger Bauabschluss. Gemeinsame
Diagnosebeobachtungen in beiden Fällen etwa eine Sekunde, kein Live-Timeout.
Timeout, Transportfehler, unklarer Bezirk, Sessionkonflikt, until und Reparaturstart
bleiben synthetische Gate-Nachweise. Der Baupilot belegt die Struktur-Startvorprüfung
mit kurzem festem Beobachtungsfenster, keinen langen Belastungstest.

## Abnahme 2026-10-18

Menschliches Korrekturgate 2026-10-18 bestanden: 2013 Unit- und 13
Integrationstests, drei optionale Livefälle übersprungen, Release-Build ohne Fehler.
Die zuvor fehlenden Katalogerwartungen sind korrigiert; strikte Vergleiche bleiben.
Nach Codex-Neustart ist inspect_plank_chain auch im Client verfügbar.

Liveabgleich: vier Holzfäller und zwei Sägewerke im gewählten Bezirk vollständig
ausgewählt. Betrieb aller sechs Produzenten stimmt mit Einzelabfragen überein;
Inventare, Kraft und Lager-/Wegbelege beider Sägewerke separat bestätigt.
Rezept: ein Holz zu einem Brett in 1,25 Stunden; verfügbare globale Vorräte
260 Holz und 33 Bretter. Ein Sägewerk pausiert, ohne Personal: inaktiver Verbraucher
ist kein Anschlussfehler. Das andere ist besetzt und in Arbeitszeit, hat aber
13 Bretter im vollen Inventar und Kraftversorgung 0 bei Bedarf 50.
Fünf erreichbare Holzquellen dort; beim pausierten Werk bleibt Erreichbarkeit
unbekannt. Kein geeignetes Bretterziel in der vollständigen nativen Suche gefunden.
Nächste Schritte: Pause/Personal prüfen, Kraftversorgung herstellen und vorhandene
Lager auf Bretterauswahl, Annahme und freien Platz prüfen; Ausbau nur separat entscheiden.

Separater Messlauf beider Werke: exakt eine Spielstunde, 2,97 Echtzeitsekunden,
unverändertes Höchstbudget 30 Sekunden, overshootHours=0. Je null Zyklen,
eine Stunde multiple_blockers; frische Diagnose bestätigt dieselben Ursachen.
Lauf trotz Betriebsblockaden gestartet, automatisch beendet und Pause bestätigt;
frische Tempoabfrage zeigt 0, keinen aktiven Lauf und keinen ausstehenden Auftrag.
Keine Einstellungen geändert, kein Ausbau und keine automatische Budgetverlängerung.

Grenzen: kein erfolgreicher Produktionszyklus und kein Materialmangel live geprüft.
Holzknappheit ohne pauschale Startsperre ist durch synthetische Gate-Tests abgedeckt.
Vorprüfung bleibt lesend und productionProven=false; globale Bestände und
erreichbare Quellen sind keine Liefer- oder Dauerproduktionsgarantie.
