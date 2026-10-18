# Bretterketten-Vorprüfung — begrenzt live abgenommen

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
