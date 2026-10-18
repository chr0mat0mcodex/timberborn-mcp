# Projektstand

## Aktueller Stand — 2026-10-18

**A12/N03 im begrenzten Injektionspiloten abgenommen.**
2284 Unit-, 19 Integrations-, 13 Paket- und 35 Supervisor-Tests bestanden;
drei optionale Livefälle übersprungen, beide Builds ohne Warnungen/Fehler,
fünf installierte Paketdateien per SHA-256 bestätigt. Nach Codex-Neustart
neues Werkzeug verfügbar; Session und MCP-Kennungen entsprechen dem Gate.
Live: beide Ausgangsfreigaben true; unter Einzelinjektion identity_mismatch,
closeEligible=false und repairEligible=false. Tatsächlicher Close abgelehnt,
Reparaturplanung mit repair_parent_ineligible/not_executed abgelehnt.
Vollständige Objekt- und Journalantworten während Injektion exakt unverändert.
Nach disable wieder echte Identität und beide Freigaben; regulärer Abschluss
released=true. Keine Bestandsverluste, Injektion inaktiv, Spiel pausiert.
Nutzen: deterministischer nativer Recovery-Sperrnachweis; mittlerer Aufwand
über Mod, Vertrag, MCP und Tests. Kein Nachweis eines natürlich entstandenen
Identitätsfehlers; keine weiteren Fehlerfälle erweitert.
[Vertrag, Belege und Bewertung](docs/unknown-object-identity-pilot.md).
Bridge/API weiterhin 0.74.0. Anleitung und Werkzeugübersicht aktualisiert;
keine projektbezogenen Spielskills vorhanden. Kein Gate offen.

### Vorheriger Abschluss A12/N02

**A12/N02 live abgenommen — unterer Zielweg an Cw270-Bestandstreppe.**
Auf unverändertem N01-Gatestand: automatische Zugangssuche ergänzt genau einen
unteren Weg für ein kleines Lager. Native Geometrie/Anschlüsse und Vorschau-
Wiederherstellung bestätigt; seitlicher Weg bei gültiger Geometrie unverbunden.
Reale Baustelle buildersReachable=true; nach vier Spielstunden 2/2 Teile fertig
und zugänglich, keine verlorenen Bestandsverbindungen, Journal freigegeben.
Spiel pausiert; Testaufbau bleibt bestehen. Keine Codeänderung und kein neues Gate.
Nutzen: fehlende Livegrenze gezielt geschlossen; Aufwand überwiegend isolierter
Aufbau und Räumung. Kein Anlass für Refactoring aus diesem Fall.
[Nachweise und Grenzen](docs/lower-stair-landing-pilot.md).
Spielanleitung und Werkzeugübersicht mit Abnahme aktualisiert; keine
projektbezogenen Spielskills vorhanden. Weitere A12-Pakete bleiben offen.

### Vorheriger Abschluss A12/N01

**A12/N01-Pilot abgenommen — N01 abgeschlossen, Gesamt-A12 teilweise offen.**
`inspect_map_region` meldet bei nativer Kartenüberschreitung die erste betroffene
Achse und den tatsächlichen gültigen Zellbereich. Kontrollierter Fehlertransport,
keine Zusatzabfrage oder Mutation; andere Karten-/Bauwerkzeuge unverändert.
Gate: 13 Paket-, 35 Supervisor-, 2256 Unit- und 19 Integrationstests bestanden;
drei optionale Livefälle übersprungen. Alle 17 neuen Fälle grün; Release-Builds
ohne Warnungen/Fehler, fünf installierte Dateien per SHA-256 bestätigt.
Livekarte 50×50×23: Eckzelle vor/nach vier Ablehnungen lesbar; x/y melden 0..49,
z meldet 0..22; auch überschreitende Breite korrekt erkannt. MCP-Kennungen wie Gate.
Nutzen/Aufwand positiv: konkrete Parameterkorrektur ohne zusätzliche Abfrage,
kleiner Fachumfang mit kontrolliertem Fehlertransport. Kein Gate offen.
[Umfang, Kompatibilitätsgrenze und Nachweise](docs/map-region-bounds-pilot.md).
Bridge/API weiterhin 0.74.0; gemeinsam ausgeliefertes Mod-/MCP-Paket erforderlich.
Spielanleitung bei Parameterdiagnose aktualisiert; keine projektbezogenen
Spielskills vorhanden. Weitere A12-Pakete bleiben zurückgestellt.

### Vorheriger Abschluss A11

**A11-Pilot abgenommen — A11 abgeschlossen.**
Roher DLL-Direktstart verlangt explizite Backendwahl; fehlend/leer führt vor
Hoststart zu Exitcode 2 und verständlicher stderr-Meldung, stdout bleibt leer.
Explizite Native-/Legacy-/Fake-Auswahl und Offline-Kennungsbericht bleiben erhalten.
Nach ASCII-Korrektur der frühen stderr-Meldung: 13 Paket-, 35 Supervisor-,
2239 Unit- und 19 Integrationstests bestanden, drei optionale Livefälle übersprungen.
Alle fünf neuen Prozessfälle grün; Release-Builds ohne Warnungen/Fehler und fünf
installierte Dateien per SHA-256 bestätigt. Live: Supervisorstart, nativer Status
und lesende Simulation mit derselben Sitzung; MCP-Kennungen entsprechen dem Gate.
Nutzen/Aufwand positiv: eindeutige Backendwahl durch kleinen lokalen Eingriff.
Bisher implizite Legacy-Direktstarts müssen die Backendvariable ausdrücklich setzen.
[Kompatibilitätsentscheidung und Nachweise](docs/backend-selection-pilot.md).
Bridge/API 0.74.0. Kein Gate offen. Native-/Legacy-Anleitung und
Spielanleitung aktualisiert; keine projektbezogenen Spielskills vorhanden.
Weitere Journale und sonstige Erweiterungen bleiben zurückgestellt.

### Vorheriger Abschluss A10

**A10-Pilot abgenommen — Gate und lesender Livebeleg bestanden.**
Manuelles Großprojektjournal: eigener Limitcode und additive Kapazitätsdiagnose
in Plan-/Projektbelegen. Historie, Replay-Schutz und Limit 128 bleiben erhalten.
Gate: 13 Paket-, 35 Supervisor-, 2239 Unit- und 14 Integrationstests bestanden;
drei optionale Livefälle übersprungen. Alle 18 neuen Fälle bestanden; Release-
Builds ohne Warnungen/Fehler, fünf installierte Dateien per SHA-256 bestätigt.
Live: MCP-Kennung gleich Gate, Full-/Compact-Plan mit identischer manueller
Kapazität (128/0/128, Freigabe schafft keinen Platz), stabiler planKey und Zähler.
Vorschau restauriert; kein Bauauftrag. Planzugang blieb getrennt davon gesperrt.
Grenzfälle/Freigabe/Replay und Limitfehlertransport synthetisch geprüft.
Nutzen/Aufwand positiv: klare Diagnose bei kleinem, auf ein Journal begrenztem
Eingriff. [Nachweise und Bewertung](docs/manual-project-journal-pilot.md).
Bridge/API 0.74.0. Zum A10-Abschluss kein Gate offen. Anleitung mit Abnahme aktualisiert;
keine projektbezogenen Spielskills vorhanden. Andere Journale und Aufbewahrungs-
änderungen zurückgestellt; Gesamt-A10 teilweise offen.

### Vorheriger Abschluss A09

**A09-Eingangspilot abgenommen — Gate und Lifecycle-Livefall bestanden.**
Supervisor begrenzt Clientzeilen auf 1 MiB und isoliert Parse-/Formfehler vor
Dispatch; übergroße Frames werden vollständig verworfen, ohne Replay/Neustart.
Gate: 13 Paket-, 35 Supervisor-, 2221 Unit- und 14 Integrationstests bestanden;
drei optionale Livefälle übersprungen. Sieben neue Supervisor-Testmethoden grün,
Release-Builds ohne Warnungen/Fehler, fünf installierte Dateien per SHA-256 bestätigt.
Nach vom Nutzer bestätigtem Codex-Neustart: Backendstart und nativer Status,
Stop mit running=false/gate=true, Status ohne Neustart, expliziter Wiederstart
und nativer Status mit gleicher Session und MCP-Kennung bestanden. Kein Spielauftrag.
Nutzen/Aufwand positiv: Fehlerisolierung und begrenzte rohe Eingabe bei kleinem
Codeumfang. Inflight-/Threadgrenzen und Eingangstimeouts bleiben offen.
[Nachweise, Bewertung und Grenzen](docs/supervisor-input-pilot.md).
Bridge/API unverändert 0.74.0. Zum A09-Abschluss kein Gate offen. Supervisor-Doku und Spielanleitung
mit tatsächlicher Abnahme aktualisiert; keine projektbezogenen Spielskills vorhanden.
Gesamt-A09 teilweise offen; weitere Erweiterungen auf Nutzerwunsch zurückgestellt.

### Vorheriger Abschluss A08

**A08-Pilot abgenommen — menschliches Gate und gezielter Livepilot bestanden.**
Abschlussentscheidung von `RoadProtection.Finish` nach Bridge.Core gezogen;
native Reads/Cleanup und Freigabegrenzen unverändert. 13 neue synthetische Fälle
bestanden. Gate: 13 Paket-, 28 Supervisor-, 2221 Unit- und 14 Integrationstests;
drei optionale Livefälle übersprungen. Release-Builds ohne Warnungen/Fehler,
fünf installierte Dateien per SHA-256 bestätigt. MCP-Kennung live gleich Gate.
Positive Bankvorschau bestanden: 740 Verbindungspaare, kein Verlust, restauriert.
Einzellige Bank als Negativkontrolle ungeeignet; nach gezielter Diagnose größere
Wohnhausvorschau am selben Wegknoten mit fünf echten Verlusten (zwei Wegpunkte,
drei Gebäudezugänge), korrekt blocked und restauriert. Ursprüngliche freie
Vorschau danach wieder gültig und verlustfrei; tatsächlicher Zugang unverändert.
Keine persistente Änderung beobachtet, keine Session-Sperre, kein Bauauftrag.
Nutzen/Aufwand positiv: isolierte Policytests und explizite Adapter-Nachweisgrenze;
kleiner Codeumfang, zusätzlicher Aufwand für geeignete native Negativkontrolle.
[Nachweismatrix, Grenzen und Bewertung](docs/mod-adapter-evidence-pilot.md).
Bridge/API 0.74.0. Zum A08-Abschluss kein Gate offen. Anleitung mit tatsächlichem Nachweisstand
aktualisiert; keine projektbezogenen Spielskills vorhanden. Gesamt-A08 offen;
weitere Adapter, Sessionwechsel und Fertigzugang bleiben zurückgestellt.

### Vorheriger Abschluss A07

Bridge/Manifest **0.74.0**; letzter neuer Funktionsumfang in `f52750f`:
automatische Bretterketten-Vorprüfung, begrenzt live abgenommen.
**A07-Pilot abgenommen — menschliches Gate und lesender Liveabgleich bestanden.**
Natives Statusfeld `data.mcp`: geladene MCP-Modulkennung und wirksamer
Backend-Kataloghash, getrennt von Bridge-Version und Liveabnahme. Clientcache
explizit `not_observed`; Supervisor-Handshake und Spielmod unverändert.
Gate: 13 Paket-, 28 Supervisor-, 2208 Unit- und 14 Integrationstests bestanden;
drei optionale Livefälle übersprungen. Release-Builds ohne Warnungen/Fehler,
fünf installierte Paketdateien per SHA-256 bestätigt. Elf neue Unit-Fälle und
Offline-Integrationstest bestanden; ungültige Testfixture aus erstem Gate korrigiert.
Live nach Supervisor-Wiederanschluss: beide Kennungen stimmen exakt mit Gate,
compact und full überein und bleiben nach lesender Simulationsabfrage stabil.
Native Bridge 0.74.0, aktuelle Session, Spiel pausiert; keine Spielmutation.
[Belege, Grenzen und Nutzen/Aufwand](docs/mcp-runtime-identity-pilot.md).
Bewertung positiv: MCP-only-Stand dem Gate zuordenbar bei überschaubarem Zusatz-
aufwand. Clientcache, Supervisor und vollständige Installation nicht attestiert.
Gesamt-A07 offen; weitere Erweiterungen auf Nutzerwunsch vorerst zurückgestellt.
Zum A07-Abschluss kein Gate offen. Spielanleitung mit Abgleich und tatsächlichen Nachweisgrenzen
aktualisiert; keine projektbezogenen Spielskills vorhanden.
Weitere A03/A04/A05-Migrationen bleiben zurückgestellt.

### Vorheriger Abschluss A05

**A05-Pilot abgenommen — menschliches Gate und gezielter Livetest bestanden.**
Nur `ProductionRunContract.Validate` in vier benannte private Prüfabschnitte
zerlegt. Bedingungen, Reihenfolge und Fehlertexte erhalten; Quellvergleich erfolgt.
Gate: 13 Paket-, 28 Supervisor-, 2197 Unit- und 13 Integrationstests bestanden;
drei optionale Livefälle übersprungen. Alle 23 neuen Charakterisierungsfälle
bestanden. Release-Builds ohne Warnungen/Fehler, fünf Paketdateien per SHA-256 bestätigt.
Live: Ziel 100 Bretter bei frisch gelesenen 33; nach 0,25 Spielstunden korrekt
`production_time_limit`, Ziel weiter unerreicht. Leichter Status und einmalige
Diagnoserücklesung akzeptiert; terminale und frische Endpause bestätigt.
[Belege, Grenzen und Nutzen/Aufwand](docs/production-contract-readability-pilot.md).
Bewertung positiv: Fachabschnitte und Fehlerreihenfolge leichter prüfbar;
mehr Zeilen und vier private Methoden als überschaubarer Aufwand. Kein Laufzeitgewinn
behauptet. Gesamt-A05 offen; weitere Fachprüfungen auf Nutzerwunsch zurückgestellt.
Keine weitere A03/A04-Migration. Zum A05-Abschluss kein Gate offen;
aktueller A07-Auftrag siehe oben.
Agenten-Spielanleitung geprüft: keine fachliche Änderung, öffentliche Verträge
unverändert. Keine projektbezogenen Spielskills vorhanden.

### Vorheriger Abschluss A04

**A04-Pilot abgenommen — menschliches Gate und gezielter Livetest bestanden.**
Benannte MCP-Optionen und gemeinsame Metadaten ausschließlich für die vier
Simulationslauf-Werkzeuge. HTTP-Freigabe unverändert; übrige Familien bleiben
bestehen. Einmaliger Katalogindex ersetzt Schemaaufbau im Dispatch-/Fehlerpfad.
Gate: 13 Paket-, 28 Supervisor-, 2174 Unit- und 13 Integrationstests bestanden;
drei optionale Livefälle übersprungen, Release-Builds ohne Warnungen/Fehler,
fünf installierte Paketdateien per SHA-256 bestätigt. 28 neue Testfälle bestanden.
Live nach verbundenem Supervisor-Start: native Bridge 0.74.0 und Session bestätigt;
for-Start, laufender Status, einmaliger Cancel bis terminaler Pause bestanden.
Until-Lauf erreichte sein Ziel nach 0,25 Spielstunden ohne Überschuss; frische
Endpause bestätigt. Falsche Session mit `stale_session` abgewiesen.
[Belege, Grenzen und Nutzen/Aufwand](docs/tool-metadata-pilot.md).
Bewertung positiv: weniger getrennte Metadatenpflege und kein Katalog-Neuaufbau
im Aufrufpfad; überschaubarer Modell-/Testaufwand, einmaliger Index als Zusatzkosten.
Kein Laufzeitgewinn gemessen. Gesamt-A04 bleibt offen; weitere Werkzeugfamilien
auf Nutzerwunsch vorerst zurückgestellt. Zum A04-Abschluss kein Gate offen;
aktueller A05-Auftrag siehe oben.
Spielanleitung geprüft: keine fachliche Änderung, da öffentliche Verträge und
Bedienabläufe gleich bleiben. Keine projektbezogenen Spielskills vorhanden.

### Vorheriger abgenommener Refactoringpilot

**A03-Pilot abgenommen — menschliches Gate und gezielter Livetest bestanden.**
Zentrale explizite Wire-Vertragsprofile für Envelope und Validierungsbudget;
102 bekannte Versionen unverändert, davon 58 Legacy-/44 erweiterte Budgetprofile.
Keine neue Bridge-Version oder Abhängigkeit; weitere Vertragsfamilien unverändert.
Gate: 13 Paket-, 28 Supervisor-, 2146 Unit- und 13 Integrationstests bestanden,
drei optionale Livefälle übersprungen; Release-Builds ohne Fehler und fünf
installierte Paketdateien per SHA-256 bestätigt. Die 121 neuen Matrix-/Negativfälle
sichern historische Versionen, Budgetgrenzen und Envelope-Pflichtfelder ab.
Live nach Supervisor-Wiederanschluss: native Bridge 0.74.0 erreichbar;
Karten-/Simulationsreader erfolgreich, Vorschau mit `attemptsRemaining=999999`,
Restaurierung bestätigt und keine reguläre Baufreigabe aus dem Budget abgeleitet.
Kurzlauf über 0,25 Spielstunden terminal erfolgreich, frische Pause bestätigt;
falsche Session mit `stale_session` abgewiesen. Kein Bauauftrag.
[Belege, Grenzen und Nutzen/Aufwand](docs/compatibility/timberborn.md#a03-pilot--zentrale-wire-vertragsprofile).
Bewertung: kleine, sinnvolle Zentralisierung zweier bisher getrennter Listen;
Testmatrix ist der wesentliche Zusatzaufwand. Gesamt-A03 bleibt offen,
weitere Vertragsfamilien auf Nutzerwunsch vorerst zurückgestellt. Beim A03-Abschluss
kein Gate offen; aktueller A04-Auftrag siehe oben.
Agenten-Spielanleitung geprüft: keine Änderung, weil Werkzeugvertrag und
Spielverhalten unverändert sind; keine projektbezogenen Spielskills vorhanden.

### Letzter abgenommener Infrastrukturabschluss

**A06 abgeschlossen — Gate und Status-Liveabgleich am 2026-10-18 bestanden.**
Paketbuild liefert Build-ID, konkreten Pfad und fünf SHA-256-Werte separat von
Diagnoseausgaben. Gate prüft Manifest/Zuordnung vor Installation und die
Staging-/Installationsdateien gegen dieselben Prüfsummen; keine Zeitstempelsuche.
Menschliches Gate: 13 Paket-, 28 Supervisor-, 2025 Unit- und 13 Integrationstests
bestanden; drei optionale Integration-Livefälle übersprungen, Release-Builds
ohne Fehler. Paketmeldung und Gate-Auswahl nennen denselben konkreten Pfad;
alle fünf installierten Paketdateien sind per SHA-256 bestätigt.
Die synthetischen Paketfälle belegen unter anderem ein fremdes neueres Paket,
fehlende/fremde Ergebnisse, Manifestabweichungen und beschädigte Stagingdateien.

Nach menschlichem `live bereit` Backend über den verbundenen Supervisor gestartet:
`running=true`, `gate=false`. Lesendes timberborn_status bestätigt `reachable`,
Bridge 0.74.0, aktuelle native Session und keine Simulation der Daten.
Keine Spielmutation und kein erneuter Agent-Build/Test/Deploy erforderlich.
Grenze: Fehlerfälle synthetisch geprüft; realer Paket-/Installationspfad und
Wiederanschluss bestätigt, keine zusätzliche Spiel-Funktionsabnahme behauptet.
Zum A06-Abschluss war kein Gate offen; der aktuelle A03-Pilot steht oben.

Dokumentationspaket A01/A02 abgeschlossen:
aktuelle Einstiege vereinheitlicht, offene Aufgaben im [Backlog](BACKLOG.md),
vorherigen Backlog vollständig ins [Archiv](BACKLOG_HISTORY.md) übernommen.
Nur Inhalts-/Diff-/Linkprüfung; keine neue Build-/Liveabnahme.
Für A06 wurden Installationsanleitung und Entwicklungsablauf aktualisiert.
Agenten-Spielanleitung geprüft: keine fachliche Änderung, da Spielwerkzeuge,
Parameter und Spielverhalten unverändert bleiben. Keine projektbezogenen
Spielskills vorhanden; globale Skills unverändert.

## Letzter fachlicher Abschluss — automatische Bretterketten-Vorprüfung

Automatische Bretterketten-Vorprüfung **begrenzt live abgenommen**:
Vor Bauaufträgen mit Bretterkosten und langen Simulationsläufen mit Bretterbezug
ergänzt `operatingPreflight[].plankChain` Engpässe und konkrete nächste Schritte.
Gemeinsames Diagnosebudget unverändert fünf Sekunden; keine Materialsperre,
kein Ausbau und keine Verlängerung des Laufbudgets.
[Vertrag und Livebelege](docs/plank-chain.md#automatische-einbindung-vor-starts).

Umsetzung ausschließlich im MCP-Startbeobachter, bestehende Bridge 0.74.0.
Menschliches Gate: 2025 Unit- und 13 Integrationstests bestanden, drei optionale
Livefälle übersprungen; Release-Build ohne Fehler. Simulation mit 33/100 Brettern
und Baupilot mit 33/36 Brettern trotz Fehlmenge gestartet. Beide automatischen
Ketten samt Befunden identisch zu separaten Readern und Sägewerks-Einzelabfragen.
Simulation endet nach sechs Spielstunden (17,72 Sekunden), Bau nach einer
Spielstunde (2,95 Sekunden); jeweils unverändert 30 Sekunden Höchstbudget,
kein Überschwingen, Pause bestätigt. Sechs Plattformbaustellen bestätigt,
noch unfertig; keine Produktionsanlage erweitert. Folgelesungen starten nichts.
Grenzen: kein Fertigbau-/Produktionsnachweis, until/Reparatur und Fehlerfälle
in dieser Integration nur synthetisch geprüft. [Details](docs/plank-chain.md#liveabnahme-der-automatischen-einbindung-2026-10-18).

Anleitung, Werkzeugvertrag und Fachverträge aktualisiert; keine projektbezogenen
Spielskills vorhanden.

## Historische Abnahmen und Zwischenstände

Die folgenden Belege sind chronologisch gewachsene Historie. „Aktuell“, „offen“,
„nächster Schritt“ und damalige Gate-Aufträge gelten ausschließlich für den
jeweiligen Eintrag; sie ersetzen weder den aktuellen Kopf noch die offene
Aufgabenliste im Backlog. Ältere Testzahlen sind keine aktuellen Gesamtzahlen.

### Bretterketten-Reader — Abschluss vor automatischer Einbindung

Bretterketten-Vorprüfung **begrenzt live abgenommen**: `inspect_plank_chain`
bündelt Holzversorgung, Sägewerksbetrieb, Kraft und Lager-/Transportbelege im
gewählten Bezirk. Nur Diagnose und konkrete nächste Schritte; kein automatischer
Ausbau oder Material-Startverbot. [Ablauf und Grenzen](docs/plank-chain.md).

MCP-Erweiterung mit bestehenden nativen Lesern; keine neue Mod-API.
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

Anleitung und Werkzeugdoku
aktualisiert; keine projektbezogenen Spielskills vorhanden, Hermes bleibt entfernt.


## Fortschrittspilot 2026-10-18 — begrenzt live bestanden

Menschliches Korrekturgate: 2002 Unit- und 13 Integrationstests bestanden,
drei optionale Integration-Livetests übersprungen; Release-Build ohne Fehler.
Kleine Lodge plus Weg nativ geprüft (pilotEligible/restored), einmal beauftragt.
Erste Diagnose bei 08:28 liefert baseline_recorded, erneute Abfrage bei Pause
no_game_time_elapsed. Der fertige Weg mit construction=null wird korrekt gelesen.
Einzelabfrage bestätigt Material- und Baufortschritt 0, Baustellenvorrat leer.

Erstes festes Fenster: eine Spielstunde bis 09:28, 2,99 Echtzeitsekunden,
30 Sekunden Höchstbudget. Ziel exakt erreicht, overshootHours=0, Pause bestätigt.
Diagnose waiting_for_materials: beide Fortschrittsdifferenzen 0, Vorrat unverändert.
Einzelabfrage der Lodge stimmt überein; Zentrum und Bauhütte jeweils vier
Beschäftigte, Arbeitszeit und laufender Job aktiv. Compact/Full erhalten dieselben
Messpunkte. Weitere Diagnose verlängert das Fenster nicht; kein aktiver Lauf.

Separat ausdrücklich gestartetes zweites Fenster: drei Spielstunden bis 12:28,
8,86 Echtzeitsekunden bei weiter 30 Sekunden Höchstbudget. Diagnose
finished_since_baseline; Einzelabfrage bestätigt fertige Lodge und construction=null.
Keine erfundenen numerischen Endwerte nach Fertigstellung. Vergleich bleibt
auch in Compact erhalten, obwohl fertige Teile aus der Aufmerksamkeitsliste entfallen.
Erneute Diagnose und Laufabfrage: Ziel und Grenzen unverändert, overshootHours=0,
terminal und Pause bestätigt. Projekt freigegeben, beide Teile fertig/zugänglich,
keine beobachteten Bestandszugangsverluste; Spiel pausiert, kein Lauf/Tempoauftrag.

Grenzen: Lieferung und Teilbaufortschritt lagen zwischen den Messpunkten.
Positive numerische Material-/Baudeltas und Stillstand bei materialbereiter
Baustelle bleiben synthetische Gate-Nachweise. Keine Dauerarbeits-, Liefermengen-
oder allgemeine Produktionsgarantie. Budgetbeleg gilt für diese getrennten
Simulationsfenster, nicht als zusätzlicher autonomer Projektlauf-Nachweis.

Historischer Fehler behoben: JSON-null wurde im nativen Adapter als Objekt
indiziert. Explizite JObject-/JArray-Prüfungen sind nun im Gate und im
Lodge-/Weg-Livepilot bestätigt.


Fortschrittsdiagnose **begrenzt live abgenommen**: frische Baudiagnose vor
und nach einem begrenzten Zeitfenster vergleichen. Lieferung, Baufortschritt
und unveränderten Fortschritt anhand belegter Messwerte trennen. Keine automatische
Budgetverlängerung oder Material-Startsperre. [Ablauf und Grenzen](docs/construction-progress.md).

Implementierung in Core und nativer Baudiagnose; bestehende Werkzeugaufrufe
bleiben unverändert. Tests im menschlichen Gate bestanden. Anleitung und Toolvertrag
aktualisiert; keine projektbezogenen Spielskills vorhanden, Hermes bleibt entfernt.
Backlog trennt aktuelle Arbeit von historischen Gate-/Live-Zwischenständen.

Bezirksweite Bauarbeitszeit **begrenzt live abgenommen**: Zentrum und
Bauhütten gemeinsam prüfen. Aktiver Job oder besetzte unpausierte Bauhütte in
Arbeitszeit verhindert gemeinsamen Feierabend; unbekannte Belege bleiben null.
Einzelner Feierabend am Zentrum ist keine Baupause im ganzen Bezirk.
[Vertrag, Grenzen und Pilot](docs/operating-readiness.md).


## Liveabnahme bezirksweite Bauarbeitszeit 2026-10-18

Menschliches Gate bestanden: 28 Supervisor-, 1984 Unit- und 13 Integrationstests;
drei optionale Integration-Livetests übersprungen. Bridge 0.74.0.
Im pausierten Folktails-Spiel um 14:28 stimmen Bezirkszuordnung, Besetzung,
Pause, Arbeitszeit und laufende Jobs von Zentrum und Bauhütte mit den
Einzelabfragen überein: jeweils vier Beschäftigte, Arbeitszeit und Jobs aktiv.
Kontrolle: Zentrum vorübergehend pausiert (0 Beschäftigte, kein Job), Bauhütte
weiter unpausiert mit vier Beschäftigten und laufendem Job. builderWorkforce
bleibt complete=true/workingHours=true, keine gemeinsame Feierabenddiagnose.
Zentrum anschließend nachweislich wieder eingeschaltet.

Abendgegenprobe nach sechs Spielstunden (17,73 Sekunden), um 20:28 pausiert:
beide Arbeitsplätze besetzt und unpausiert, workingHours=false und kein Job.
Einzelabfragen und vollständiger Bezirksbefund stimmen überein. Lodge mit
12/12 Holz, aktiv und erreichbar, Baufortschritt null: korrekt
builders_outside_working_hours und wait_for_construction_working_hours.
Compact und Full erhalten dieselben Einzelbelege. Betriebsdiagnose bestätigt
den Einzelwert des Zentrums und erklärt ausdrücklich, dass dieser allein
keine bezirksweite Baupause beweist.

Separat begrenzte Morgengegenprobe bis 06:00 (9,53125 Spielstunden, 28,08
Sekunden): beide Arbeitsplätze wieder aktiv, beide Testteile fertig und
erreichbar. Zielpause bestätigt, gestopptes Projekt danach freigegeben.
Der erste Freigabeversuch am Abend blieb wegen der unfertigen Lodge gesperrt.

Grenze: Unterschiedliche Schichtzeiten (Zentrum Feierabend, Bauhütte in Schicht)
wurden synthetisch im Gate geprüft, nicht live hergestellt. Der Livekontrollfall
verwendet eine Gebäudepause; diese ist kein Feierabend. Laufende Jobs belegen
keine Arbeit an genau dieser Baustelle. Keine automatische Budgetverlängerung.
Anleitung, Werkzeugdoku, Fachvertrag und Einstiegsdokumente aktualisiert;
kein vorhandener projektbezogener Spielskill zu ändern, Hermes bleibt entfernt.

## Liveabnahme Arbeitszeitdiagnose 2026-10-18

Menschliches Gate: 28 Supervisor-, 1975 Unit- und 13 Integrationstests bestanden;
drei optionale Integration-Livetests übersprungen. Builds ohne Warnungen/Fehler.
Vormittags Betriebsdiagnose gegen Einzelabfragen von Bezirkszentrum und zwei
Sägewerken: workingHours=true, keine Feierabend-Hinweise, Werte identisch.

Ein Lodge-Auftrag mit Zugangsweg, native Vorschau pilotEligible und wiederhergestellt,
sechs Stunden festes Bauzeitfenster ab 14:28. Frische Anfangsdiagnose:
materials_not_ready und constructionCenterWorkingHours=true. Nach Lieferung
12/12 Holz, aktive unpausierte erreichbare Baustelle, Baufortschritt noch null.
Bei Feierabend construction_center_outside_working_hours mit Quell-ID und
district_center_workplace_component; Compact und Full erhalten den Beleg und
wait_for_construction_working_hours samt verständlicher Erklärung.
Betriebsdiagnose für Zentrum und beide Sägewerke: outside_working_hours mit
Feierabend-Text; alle drei Einzelabfragen stimmen überein. Pause um 20:28
nach exakt sechs Stunden bestätigt. Historischer game_time_limit bleibt getrennt.

Expliziter Kontrolllauf bis Folgetag 06:00 (9,53125 Spielstunden, 28,19 Sekunden):
Pause bestätigt, Zentrum/Produzenten wieder workingHours=true, keine
Feierabend-Hinweise. Baudiagnose Full zeigt ebenfalls true und inzwischen
beide Teile finished_accessible, keine beobachteten Bestandszugangsverluste.
Testprojekt freigegeben. Keine Schicht-/Personaländerung, keine Startsperre
oder automatische Budgetverlängerung. Keine aktiven Aufträge nach Abschluss.

Grenzen: Andere Bauhütten-Schichten nicht aus Bezirkszentrum ableiten;
unbekannte Arbeitszeiten und widersprüchliche Belege synthetisch im Gate geprüft.
Der genaue Zeitpunkt der Fertigstellung zwischen Abend und 06:00 wurde nicht
beobachtet. Der anfängliche true-Beleg und die spätere false-Einzelabfrage lagen
während laufender Simulation zu verschiedenen Zeiten; die pausierten
Abend-/Morgengegenproben bestätigen jeweils konsistente aktuelle Werte.


## Vorheriger Stand: Arbeitszeit nur des Bezirkszentrums — begrenzt live abgenommen

Die Betriebsdiagnose benennt outside_working_hours ausdrücklich als Feierabend,
working_hours_unknown als fehlende Beobachtung. Besetzung ist kein Arbeitsbeleg.
Die frische Baudiagnose (diagnose_large_project bzw. inspect_large_project mit
diagnose=true) liest die native WorkplaceWorkingHours-Komponente des zum Auftrag
gehörenden Bezirkszentrums einmal pro Bericht. constructionCenterId,
constructionCenterWorkingHours und workingHoursSource bleiben in Compact/Full.
Bei erreichbarer, unpausierter, aktiver und materialbereiter Baustelle mit
beobachtetem false lautet reason construction_center_outside_working_hours;
nextStep ist wait_for_construction_working_hours mit konkreter Erklärung.
Pause, inaktive Baustelle/Stützen, Materialmangel und fehlender Zugang behalten
Vorrang. Fehlende Komponente bleibt null, ältere Belege ohne Feld bleiben gültig.

Quelle ist ausschließlich das Bezirkszentrum: keine Behauptung gleicher
Arbeitszeiten anderer Bauhütten, keiner exklusiven Ursache oder garantierten
Fertigstellung am Morgen. Unbekannte Arbeitszeit niemals aus Uhrzeit erraten.
Keine Schichtänderung, Personaländerung, Startsperre oder automatische
Budgetverlängerung. Historische Ledgergründe bleiben historisch; für die
Arbeitszeitursache frische Diagnose anfordern.

Anwendung: Vor längerem Lauf Arbeitsplatz-Schichtstatus berücksichtigen. Bei
Feierabend und sonst geklärten Voraussetzungen bewusst ein begrenztes Fenster
bis zur nächsten Arbeitsphase wählen und Baufortschritt/Pause nachlesen.
Bei fehlendem Fortschritt während Arbeitszeit Personal, Anmarsch, weitere
Bauarbeitsplätze und Zugänge gesondert prüfen, nicht erneut Bauaufträge senden.

Im Gate geprüft: Ursache mit Prioritäten, null/true/false, widersprüchlicher
Beleg/Quelle, Compact-Erhalt sowie Betriebsdiagnose für Zentrum und Produzent.
Kein Agent-Build/Testlauf. Livepilot bestanden: frische erreichbare materialbereite
Baustelle am Feierabend und Morgen gegen Einzelabfragen abgeglichen;
keine zusätzliche Dauer-/Startsperre. Belege und Grenzen siehe Liveabnahme.


## Nachtrag 2026-10-18 — wartende Lodge fertiggestellt

Bei Spielzeit 20:28: Lodge unpausiert, 12/12 Holz an der Baustelle,
buildersReachable=true, einzige Baustelle, Priorität Normal. DistrictCenter
und BuildersHut jeweils 4/4 besetzt, trocken und unpausiert, aber
workingHours=false und anyJobRunning=false: Feierabend als aktuelle Blockade.
Keine dauerhafte Zugangs-/Materialblockade beobachtet.

Gezielte neue Diagnoseentscheidung: einmal bis Folgetag 10 Uhr, Tempo 7,
maximal 60 Echtzeitsekunden. Um etwa 08:23 bereits fertig; Bauzentrum wieder
workingHours=true/anyJobRunning=true. Ende nach 13,53125 Spielstunden und
39,90 Echtzeitsekunden mit bestätigter Pause. Frische Projektdiagnose: beide
Teile finished_accessible, keine verlorenen Bestandszugänge. Projekt freigegeben.
Der historische game_time_limit-Beleg blieb korrekt getrennt vom Fertigbau.
Die Verteilung von Transport-/Anmarschzeit vor dem vorherigen Feierabend ist
nicht beobachtet; Besetzung allein belegt weder Arbeitszeit noch Bauarbeit.
Keine Codeänderung erforderlich.

## Abschluss 2026-10-18 — automatische Betriebsdiagnose begrenzt live abgenommen

Gate: 1964 Unit- und 13 Integrationstests bestanden, drei optionale Live-Tests
übersprungen. Separater Auto-Reader nach Client-Neustart verfügbar: Auswahl von
DistrictCenter und zwei LumberMills ohne IDs; Diagnose bis auf Zeitstempel
identisch zur manuellen Gegenprobe. Simulation trotz 33 Prozent Deckung und
blockierter Produktion gestartet, nach sechs Stunden automatisch pausiert.

Baupilot: eine Lodge mit einem neuen Zugangsweg, gemeinsam nativ geplant,
pilotEligible und Vorschau wiederhergestellt. Vorprüfung im Compact-Startbeleg:
full_planned_costs_including_helpers, exakt 12 Holz laut Definitionsgraph
(Lodge 12, Weg 0), vier Holzfäller im angegebenen Bezirk automatisch gewählt.
Letzte Vorbeobachtung 23:27:14.579 UTC, Startbeleg 23:27:14.703 UTC.
Beide Aufträge anschließend bestätigt; Weg fertig und zugänglich, Lodge mit
vollständigem Materialbestand. Nach sechs Stunden/17,73 Echtzeitsekunden Pause,
Projekt game_time_limit, Lodge weiterhin awaiting_builders_or_work.
Keine beobachteten Bestandszugangsverluste. Kein aktiver Lauf/Tempoauftrag.

Abgenommen: automatische Auswahl, gemeinsame Diagnose, Kostenübernahme und
Vorprüfung vor Simulations-/Strukturstart. Materialunterdeckung live am
Simulationsziel belegt; der Baupilot hatte ausreichendes Holz. Kostenpflichtige
Zugangshilfen sind synthetisch geprüft, dieser Liveweg war kostenlos.
Keine Fertigstellungs-, Produktions- oder Liefergarantie aus Bereitschaft.
Die Lodge bleibt als Baustelle im pausierten Testspiel, das Projekt unreleased;
vor weiteren Baupiloten frisch diagnostizieren. Kein automatisches Zeitbudgetplus.
README, Einstieg, Backlog, Werkzeugvertrag und Agentenanleitung aktualisiert.
Keine Änderungen an globalen Skills oder Wiederaufnahme des Hermes-Experiments.

## Livepilot 2026-10-18 — Simulation bestanden, Restabnahme offen

Menschliches Gate bestanden: 1964 Unit- und 13 Integrationstests; drei optionale
Live-Integrationstests übersprungen. Builds ohne Warnungen/Fehler.
Backend über bestehenden Supervisor gestartet, Bridge 0.74.0 bestätigt.

Einmaliger Lauf: sechs Spielstunden, Tempo 7, maximal 30 Echtzeitsekunden,
Bestandsziel 100 Bretter bei 33 verfügbaren Brettern. Automatische Auswahl fand
das native DistrictCenter und beide fertigen LumberMills im selben Bezirk;
Suche vollständig, keine ausgelassenen Kandidaten oder unbekannten Bezirke.
Vorprüfung meldete 33 Prozent Deckung, 67 fehlende Bretter und blockierte
Produzenten. Trotzdem Start bestätigt und Fortschritt bei Tempo 7 beobachtet.
Diagnosebeobachtungen 23:23:26.472–23:23:27.060 UTC, Startbeleg 23:23:27.137 UTC:
alle Diagnosewerte liegen vor dem Startbeleg.

Einzelabfragen bestätigten Vorlagen und native Bezirkszuordnung. Betriebsdaten
und Zugangsdaten aller drei Gebäude waren mit dem Vorprüfungsbericht identisch;
Personalabfrage bestätigte 42 Beschäftigte und fünf Arbeitslose. Alle drei
Gebäude trocken, Zentrum 4/4 besetzt; eine LumberMill pausiert und unbesetzt,
die andere mit vollem Ausgang und ohne Kraft. Keine falsche Zugangswarnung.

Abschluss nach genau sechs Spielstunden und 17,72 Echtzeitsekunden:
completed/production_time_limit, weiterhin 33 Bretter, Materialziel nicht erfüllt.
Pause bestätigt, Tempo 0, kein aktiver Lauf oder ausstehender Tempoauftrag.
Das belegt Start trotz Unterdeckung, keine Produktion oder Materiallieferung.

Offen: separater inspect_operating_readiness_auto-Aufruf (im Werkzeugkatalog
dieser Codex-Sitzung noch nicht verfügbar; Client-Neustart erforderlich) und
kleiner Strukturpilot mit vollständigen Bau-/Hilfskosten. Deshalb noch kein
abschließender Feature-Commit/Push. Kein weiterer Build erforderlich, solange
kein Code geändert wird. Nach Client-Neustart aktuelle Sitzung neu lesen.

## Automatische Betriebsdiagnose — begrenzt live abgenommen (2026-10-18)

Erster Gate-Lauf für die automatische Variante: 28 Supervisor-Tests bestanden;
Build durch xUnit2031 im neuen Test abgebrochen. Assert.Single verwendet nun
den Prädikat-Overload statt vorgeschaltetem Where. Kein Laufzeitcode geändert;
Folge-Gate: Build erfolgreich; Unit-Suite beim ersten Fehler gestoppt, weil
eine ältere Start-Testattrappe den neuen Diagnose-GET als POST behandelte.
Die drei betroffenen Projekt-/Simulationsstart-Testattrappen berücksichtigen nun
genau einen vorgeschalteten Diagnoseabruf; genau ein Start-POST und unveränderte
Ablehnungszuordnung bleiben geprüft. Planung, Kurzlauf und Abbruch erwarten
weiterhin keine Vorprüfung.
Nächstes Gate: Build ohne Warnungen/Fehler; 1336 Unit-Tests bestanden, dann
Abbruch im neuen Starttest. Dessen synthetischer 0.74.0-Startbeleg ließ die
Pflichtlisten buildingIds/buildingProduction aus. Beide sind jetzt explizit leer;
die native Vertragsprüfung bleibt unverändert. Die gemeinsame Antwort deckt
Materialmangel, Diagnoseausfall und Kurzlauf ab. Fehler zeigen künftig den
MCP-Fehlerinhalt; auch der erste lange Lauf im Isolationstest muss erfolgreich sein.
Erneuter Gate-Nachweis und Livepilot stehen aus.

Auf Nutzerauftrag Anleitung vertieft: konkrete Vorabentscheidung, Auswahlgrenzen,
Startbericht versus Startbeleg, Diagnoseausfall ohne Auftragswiederholung sowie
rohe/gedeckelte Deckungsquote erklärt. START_HERE und Werkzeugübersicht verlinkt.

Automatischer Reader und Startintegration: native Bezirkszuordnung, begrenzte
Produzentensuche, volle Strukturkosten einschließlich Zugangshilfen. Beratender
Report vor Strukturstarts und Simulationen ab sechs Stunden bzw. absolutem
Ziel; keine Material-Startsperre. Status/Cancel unverändert ohne Vorprüfung.
Tests vorbereitet, kein Agent-Build/Testlauf. README, Toolvertrag und
Agentenanleitung aktualisiert; kein wiederbelebter Hermes-Skill.
[Vertrag, Grenzen und Pilot](docs/automatic-operating-readiness.md).


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


Aktueller fachlicher Abschluss: **0.74.0 plus Cancel-/Transport-Nacharbeit**.
Gate und begrenzter Livepilot bestanden; Git-Checkpoint schließt die Nacharbeit ab.
Ältere Versionsabschnitte sind historische Nachweise.



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

## Gemeinsame Betriebsdiagnose — begrenzt live abgenommen (2026-10-18)

Erster menschlicher Gate-Lauf: Build ohne Warnungen/Fehler, 28 Supervisor-
und 1948 Unit-Tests bestanden. Integration abgebrochen: neue Reader-Bezeichnung
fehlte in der expliziten Katalog-Erwartung. Erwartung in NativeBridgeTests und
optionalem LiveNativeTests ergänzt; keine Änderung am Laufzeitverhalten.
Erneutes Gate und Feature-Livepilot ausstehend. Backend bleibt gatesperrt.

Neuer MCP-Reader `inspect_operating_readiness`: Bauzentrum und bis sieben
ausgewählte Produzenten, Überflutung, Personal, Zugang, begrenzte Güterseiten
und Rezept-/Erntezuordnung. Priorisierte nächste Schritte, keine Material-
Startsperre. Auswahlabdeckung und globale/lokale Grenzen bleiben ausdrücklich.
Keine Bridgeänderung. Tests vorbereitet; kein Agent-Build/Testlauf.
README, Werkzeugvertrag und Agentenanleitung ergänzt; ältere Releasebelege
bleiben historisch. [Vertrag und Livepilot](docs/operating-readiness.md).

## Cancel-Transport — Gate und begrenzter Livepilot bestanden (2026-10-18)

28 Supervisor-, 1936 Unit- und 13 Integrationstests bestanden; drei optionale
Integration-Livetests übersprungen. Codex vollständig neu gestartet, neuer
Supervisor-Werkzeugtext und Bridge 0.74.0 bestätigt. Kein Agent-Build/Testlauf.

Anforderung waitSeconds=30: tatsächlich 1,002 Sekunden internes Warten,
1,175 Sekunden bis Antwort. Cancel 200 ms nach Beginn bei noch offenem
Status-Promise angefordert: Antwort nach 2,986 Sekunden (zuvor 31,13 Sekunden).
Zunächst pausing/cancel_requested, anschließend frisch cancelled, terminal=true,
pauseConfirmed=true und observedSpeed=0. Kein aktiver Lauf/ausstehender Tempoauftrag.

Zeitmarken (Europe/Berlin, 18. Oktober): Status empfangen 00:22:19.364,
weitergeleitet 19.394, Backendantwort 20.484, Clientantwort 20.499.
Cancel erst empfangen 00:22:22.393, weitergeleitet 22.409, Backendantwort 22.493,
Clientantwort 22.509. Damit liegt die beobachtete Serialisierung vor dem
Supervisor: rund 1,894 Sekunden zwischen Statusantwort und Cancel-Eingang;
im Supervisor/Backend nur 116 ms vom Cancel-Eingang bis zur Antwort.
Die genaue interne Client-/Tooltransport-Komponente ist nicht weiter aufgelöst.

Abgenommen ist die Begrenzung des eigenen Statuswartens und der anschließende
bestätigte Stopp im bestehenden Codex-Pfad, keine echte parallele Zustellung
oder feste End-to-End-Latenzgarantie. Der parallele Backend-Cancel-Pfad ist
synthetisch geprüft; er wurde durch diesen seriellen Livepfad nicht ausgeübt.
Die Projektstatusvariante der Begrenzung ist Gate-, kein separater Live-Nachweis.
Andere lokale Dokumentationsänderungen, portable Teststarter und historischer
Entwicklungsbericht auf Nutzerauftrag einbezogen; keine Rohlogs oder privaten
Konfigurationen. Anleitung/Toolvertrag aktualisiert, Hermes-Skill bleibt entfernt.
Die folgenden Pilot-/Vorbereitungsabschnitte sind historische Nachweise.

## Transport-Nacharbeit 2026-10-18 — neues Gate und Frontend-Neustart erforderlich

Lokales MCP-Log des isolierten Piloten: Statushandler begann 22:01:49.387 UTC
und endete 22:02:19.434 UTC; Cancelhandler begann erst 22:02:22.152 UTC und
endete 22:02:22.227 UTC. Die zusätzliche Verzögerung liegt vor InvokeLogged,
nicht an der korrigierten NativeTools-Sperre. Der bisherige Supervisor-Quellcode
verarbeitet Requests bereits in getrennten Threads. Ohne Ankunftszeitmarken ist
die genaue Grenze Client/Tooltransport → Supervisor → SDK noch nicht bewiesen.

Vorbereitet: Der Codex-Supervisor begrenzt gültige Statuswarteparameter auf eine
Sekunde. Der Backendvertrag für direkte Clients bleibt unverändert; keine
allgemeine Parallelfreigabe oder Garantie externer Zustelllatenz. Der Tooltext
erklärt die Begrenzung. Ungültige Werte bleiben unverändert zur Validierung;
keine automatischen Folgeabfragen, kein Retry. Zeitmarken für client_received,
backend_forward, backend_response und client_response: maximal 128 Einträge
pro Supervisorinstanz, nur Zeit/Ereignis/Methoden-/Werkzeugname. Keine Argumente,
Antwortinhalte, Sitzungen oder Tokens. Lokal ignoriert, Diagnosefehler blockieren
keine Aktionen. Neue Supervisor-Regressionen vorbereitet, noch nicht ausgeführt.

Auf Nutzerauftrag übrige lokale Dokuänderungen und historische Entwicklungsnotiz
einbezogen. Neun Startervarianten auf gemeinsame portable Startlogik umgestellt;
keine fest eingebauten Benutzerpfade. start-native aktiviert Testaktionen,
-ReadOnly deaktiviert sie, -DisableValidation nur die separate Validierung.
Native Auftragsprüfungen bleiben aktiv. Bestehende private Konfiguration bleibt.

Nächste Abnahme: menschliches Gate, Codex vollständig beenden/neu starten (neuer
Supervisorcode!), Spiel laden, live bereit. Ein 30-Sekunden-Statuswunsch muss nach
höchstens einer Sekunde internen Wartens zurückkehren; danach expliziter Cancel
mit frischer Pausebestätigung. Vor-/Nachlauf-Transportlatenz separat messen.
Zeitmarken zeigen, ob Cancel erst nach client_response ankommt (vor Supervisor)
oder früher empfangen/weitergeleitet, aber verspätet verarbeitet wird.
Kein Commit/Push vor bestandenem Gate und End-to-End-Livepilot.

## Cancel-Livepilot 2026-10-18 — Gate bestanden, End-to-End-Abnahme fehlgeschlagen

Menschliches Gate bestätigt; Gate-Log: 1936 Unit-Tests und 13 Integrationstests
bestanden, drei optionale Integration-Livetests übersprungen. Backend über den
verbundenen Supervisor gestartet; Bridge 0.74.0 und frische Session bestätigt.

Erster Kontrollfall: 30-Sekunden-Statuswarten kehrt nach 30,35 Sekunden zurück;
der währenddessen angeforderte gewöhnliche Schreibaufruf wird erst danach
ausgeführt statt wegen Konkurrenz abgewiesen. Dieser Tempoaufruf unterbricht
den Testlauf, daher kein gültiger paralleler Cancel-Beleg. Explizit pausiert.

Isolierter Folgefall ohne Kontrollmutation: Status mit waitSeconds=30 gestartet,
Cancel nach zwei Sekunden bei noch offenem Status-Promise angefordert. Status
antwortet nach 30,32 Sekunden mit wait_limit; Cancel erst nach 31,13 Sekunden
ab seiner Anforderung, zunächst pausing/cancel_requested. Anschließend frisch
cancelled, terminal=true, pauseConfirmed=true, observedSpeed=0 bestätigt.
Endzustand: Tempo 0, kein aktiver Lauf, kein ausstehender Tempoauftrag.

Damit keine End-to-End-Freigabe: die Aufrufe erreichen den wirksamen Dispatch
im verbundenen Aufrufweg offenbar weiterhin nacheinander. Welche Schicht
(Client/Tooltransport, Supervisor-Zustellung oder MCP-SDK) serialisiert, ist
noch nicht belegt. Die Backend-Unit-Regressionen ersetzen diesen Nachweis nicht.
Keine weiteren Livefälle, kein Commit/Push. Nächster Schritt: Ankunftszeiten
an Supervisor und Backend vergleichen; danach gezielte Korrektur oder eine
ehrliche Begrenzung langer Statuswartefenster. Keine zusätzlichen stdio-Prozesse
als vermeintlicher Nachweis der bestehenden Codex-Verbindung starten.

## Cancel während Statuswarten — Implementierungsstand vor dem Livepilot

Die Sperre lag im MCP-Backend (`NativeTools`), nicht im bereits parallel
weiterleitenden Supervisor. Während `inspect_simulation_run` oder
`inspect_large_project` mit `waitSeconds > 0` darf genau ein explizites
`cancel_simulation_run` parallel passieren. Weitere Leser, Starts und Mutationen
bleiben gesperrt; auch nach Ende der Statusabfrage hält ein noch laufender Cancel
die Sperre. Kein Retry und keine Umgehung von Parametern, Session, Aktionsschalter
oder nativen Prüfungen. Ein Cancel-Transportfehler bleibt `unconfirmed`.
Bridge bleibt 0.74.0; MCP-only-Korrektur, noch kein neuer Abnahmenachweis.

Vorbereitet: Regressionen für erfolgreiche Pause während wartender Statusabfrage,
konkurrierende Mutationen/Cancel-Aufrufe, Sperrfreigabe bei Fehler/Abbruch und
deaktivierte Aktionsfreigabe. Nur Quell-/Diffprüfung, keine Agent-Testläufe.
Menschliches Gate, anschließend Livepilot: begrenzten Lauf starten, 30-Sekunden-
Statusabfrage parallel zu Cancel; Cancel muss vor Ablauf des Wartefensters
beantwortet werden und der Lauf anschließend Pause bestätigen. Kontrollfall:
anderer Schreibaufruf während Statuswarten bleibt abgewiesen. Bei Fehler stoppen.

## Hermes-Testanbindung entfernt — 2026-10-17

Auf Nutzerwunsch den nicht mehr benötigten Hermes-Test beendet: separaten
Kompatibilitätsproxy, Integrationsanleitung und lokale Skillvorlage entfernt.
Aktive Einstiegsverweise bereinigt; historische Release-Erwähnungen beschreiben
damalige Pflege und sind kein aktueller Integrations- oder Pflegeauftrag.
MCP-/Mod-Code und regulärer Codex-Verbindungsweg unverändert. Datei- und
Referenzprüfung durchgeführt; kein Build, Testlauf oder Deploy für diesen Rückbau.

## Journal-Korrekturen — Gate und gezielter Livetest bestanden

Menschliches Gate: 23 Supervisor-, 1931 Unit- und 13 Integrationstests bestanden;
drei optionale Integration-Livetests übersprungen. Bridge weiterhin 0.74.0.
Einzelkiefer markiert und geschützt, jeweils completed und genau ein bestätigtes
Ziel. Alte Markierungs-ID nach Schutz erneut gelesen: Kiefer blieb unmarkiert.
Mit neuer Action wieder markiert; alte Schutz-ID nach Backend-Neustart und
vollständiger Client-Neuverbindung erneut gelesen: Markierung blieb bestehen.
Damit keine erneute Spielmutation in beiden Gegenrichtungen beobachtet.
Abschluss durch neue Schutz-Action, readbackConfirmed=true. Endtempo 0, kein
aktiver Lauf oder Tempoauftrag; Spielzeit unverändert.
Supervisor zunächst noch alter persistenter Prozess. Nach menschlicher
Neuverbindung lagen die Elternprozess-Startzeiten nach der Skriptänderung.
Stop/Status bestätigten running=false/gate=true; Start running=true/gate=false,
Spielsession erhalten. Windows-Austauschfehler synthetisch im Gate geprüft,
nicht künstlich live erzeugt. Keine Live-Unterbrechung eines laufenden
Schreibauftrags; deren Nichtwiederholung bleibt Gate-Nachweis.
Learnings und Grenzen in docs/releases/0.74.0.md; Anleitung, Tool-Doku und
zurückgestellter Skillentwurf aktualisiert. Korrekturen fachlich abgeschlossen; Git-Sicherung: Journal-Korrekturen `c09750f`, ursprünglicher 0.74-Abschluss `680fd00`.

## 0.74.0 — Gate und gezielter Livetest bestanden

Menschliches Gate: 21 Supervisor-, 1929 Unit- und 13 Integrationstests bestanden, drei optionale
Integration-Livetests übersprungen. Bridge 0.74.0 in neuer Session bestätigt.
Materialwarten trotz repairEligible=true empfiehlt check_material_supply.
Gezielt entfernter Bestandsweg erzeugt zwei belegte Anschlussverluste und
review_access_repair_plan; Einzelbefund Materialwarten bleibt erhalten.
Weg regulär wiederhergestellt, Plattform 1/1 fertig und zugänglich; Empfehlung
review_close_finished_project trotz historischem game_time_limit. Compact/full
und explizite Watch-Diagnose bestätigt. Beide Journale freigegeben, null
Bestandsverluste, Tempo 0, kein aktiver Lauf oder ausstehender Tempoauftrag.
[Livebelege und Grenzen](docs/releases/0.74.0.md). Anleitung, Tool-Doku und
zurückgestellter Hermes-Skillentwurf mit Livebelegen aktualisiert; kein Skill
installiert. Keine Agent-Builds/Tests. Version 0.74.0 fachlich abgeschlossen; Git-Sicherung durch den Abschlusscommit.

Ergänzender Livetest: isolierte Plattformbaustelle nativ buildersReachable=false,
construction_unreachable, keine unfertigen Stützen und lostConnections=0.
Compact/full und Watch: observed_access_problem, accessProblemPartIndices=[0],
repairEligible=true, review_access_repair_plan. Testbau regulär entfernt und
Journal freigegeben; keine Baustellen, Endtempo 0. Unbekannte Identität bleibt
live offen: reguläre Werkzeuge ändern keine Objektidentität; automatische
Bestätigung verhindert einen deterministischen unbestätigten Abrissfall.
Bekannter Abriss wurde korrekt als previously_observed_object_removed erkannt.

## 0.73.0 — Gate und gezielter Livetest bestanden

Menschliches Gate: 21 Supervisor-, 1908 Unit- und 13 Integrationstests bestanden;
drei optionale Integration-Livetests übersprungen. Bridge 0.73.0 live bestätigt.
limit=100 und depth=5 melden den konkreten Bereich mit not_applicable;
korrigierte Abfragen funktionieren. Einzelplattform nach einer Spielstunde
blocked/game_time_limit und frisch 0/1 fertig; nach separaten zwölf Stunden
frisch 1/1 fertig und zugänglich bei unveränderter Historie. Compact/full,
explizite watch-Diagnose und rein historische Journalansicht bestätigt.
Keine beobachteten Bestandsverluste; Journal regulär freigegeben, Tempo 0,
kein aktiver Lauf. [Livebelege und Grenzen](docs/releases/0.73.0.md).
Anleitung, Tool-Doku und zurückgestellter Hermes-Skillentwurf aktualisiert;
Workflow unverändert. Keine Agent-Builds/Tests. Version 0.73.0 fachlich abgeschlossen; Git-Sicherung durch den Abschlusscommit.

## 0.72.0 — Gate und vollständiger Plattform-Umwegpilot bestanden

Abschluss der seit 0.67 entwickelten Zugangsverbesserungen: automatische
Umwegsuche im sicheren Stand mit 191 Zuständen; 15/15 Teile (zehn Plattformen,
drei Wege, zwei Lager) fertig und zugänglich. Beide Lagereingänge und native
Verbindung zum Distriktzentrum bestätigt; untere Bestandswege trotz Überbau
weiter verbunden. Bestandsdiagnosen durchgehend lostConnections=0.
Journal regulär freigegeben, keine Baustellen, Endtempo 0, kein aktiver Lauf.
Zweites vorhandenes Sägewerk für die Brettlieferung aktiviert und besetzt.
Baupause/Stützwarten, beobachtete Mindestbesetzung und Menü-Sperre/Entsperre
bereits live bestanden. Menschliches Gate bestanden, keine Agent-Builds/Tests.
[Umfang, historische Vorläufe und Abschlussbelege](docs/releases/0.72.0.md).
Anleitung, Tool-Doku und zurückgestellter Hermes-Skillentwurf aktualisiert;
Workflow geprüft. Skill bleibt zurückgestellt. Abnahme für den dokumentierten
Umfang abgeschlossen; Code und Nachweise zur Git-Sicherung freigegeben.

Grenzen: neuer unterer Zielweg einer gedrehten Bestandstreppe noch kein eigener
Livefall; keine allgemeine Such-/Bauphasengarantie. Zielgebäude kann vor seinem
Zugang fertig sein. Weganschluss ist kein Nachweis trockenen Betriebs.

## 0.71.0 — Gate, Anschlussprüfung und Umwegplatzierung bestanden; Fertigbau offen

Bridge 0.71 und neue Session bestätigt. Oberer Treppenanschluss sowie seitliche
Negativkontrolle bestanden. Zwei Anschlusswege fertig, realer Bauzugang belegt.
Automatische Umwegvorschau um ein mitgeplantes Lager: 819 Suchzustände,
17 Teile, native Eingänge und Arbeitszugänge bestätigt; 3477 Bestandsprüfungen
ohne Verlust. Alle 17 Objekte einmal platziert und bestätigt. Geprüfte reale
Baustellenzugänge erreichbar, Bestandsdiagnose weiter ohne Verlust.
17 Baustellen bleiben, Endpause bestätigt; Fertigbau und unterer neuer
Treppenzielweg noch nicht abgenommen. Kein Commit/Push.
[Belege, Grenzen und Learnings](docs/releases/0.71.0.md).
Nächste Diagnosekorrektur: construction_paused trotz paused=false bei unfertigen
Trägern; Inaktivität, manuelle Pause und Stützabhängigkeit trennen. Nullbesetzung
am Holzfäller unbestätigt; minimale Besetzung und Zeitsperrstatus sichtbar machen.
Anleitung, Tool-Doku und zurückgestellter Hermes-Skillentwurf aktualisiert.
Cockpit-Gate erfolgreich; Darstellung nicht gesondert visuell abgenommen.
Backend verbunden, Startsperre frei. Baustellen nicht erneut beauftragen.

## 0.70.0 — Bestandstreppen-Höhenfall bestanden, erweiterter Umweg offen

Gate-Nacharbeit: 1841 Unit-/13 Integrationstests bestanden (3 Livefälle
übersprungen), Native Bridge mit CS1503 abgebrochen. Falschen Parametertyp
bei `Rotation` korrigiert. Cockpitbalken auf ASCII umgestellt, Grün erhalten.
Anschließendes menschliches Gate bestanden. Höhenfall bis Fertigzugang bestanden; erweiterte Umwegvorschau abgelehnt.

Instant-Netz und Vorschau mit Zielweg getrennt: gedrehter oberer Austritt
von Folktails-Bestandstreppen nur als Kandidat; native Zielweg-/Stützenvorschau
bestätigt Distriktanschluss und Wiederherstellung. Keine ausgelesene direkte
Treppenkante behaupten. Maximal acht Proben, bestehende Suchlimits unverändert.
Diagnose Version 3 zeigt Instant-Kante und Vorschauergebnis separat.
[API-Befund, Learning, Grenzen und Pilot](docs/releases/0.70.0.md).
Regressionen und Versionsprofile ergänzt; Quell-/Diffprüfung, keine Agent-
Builds/Tests/Installation. Gate und bekannter Höhenfertigbau bestanden; erweiterter Umweg nicht abgenommen, kein vollständiger Abschluss/Commit/Push.
Anleitung, Toolbeschreibung und lokaler Hermes-Skillentwurf aktualisiert;
Entwurf bleibt zurückgestellt. Workflow geprüft, unverändert.
Live 0.70: Backend verbunden, neue Session bestätigt. Höhenbau 3/3 fertig und zugänglich, Ledger freigegeben, keine Baustellen, Endpause.
Umweg mit neuen Plattformen nativ abgelehnt, keine Platzierung. Seitliche Quellkante der Bestandstreppe und Stützenvorschau gezielt klären.

## 0.69.0 — Gate bestanden, Höhenpilot erneut fehlgeschlagen

Bestandstreppen werden mit AreConnectedRoadInstant statt AreConnectedInstant
geprüft. Diagnose Version 2 kennzeichnet den Weggraph und zeigt die allgemeine
Abfrage getrennt; kein Ersatz nativer Freigabe durch Geometrie. Regressionen
und Versionsprofile ergänzt. Höhenpilot: beide Graphabfragen am Treppenaustritt false; 2048 Zustände ausgeschöpft. Manuelle Zwei-Wege-Gegenprobe gültig, Bestandsverbindungen erhalten.
MCP-Log öffnet sich nach dem Laden automatisch; bleibt manuell schließbar.
[Änderungen, Learning und Abnahmeplan](docs/releases/0.69.0.md).
Quell-/Diffprüfung; keine Agent-Builds, Tests oder Installation. Kein Commit/Push.
Spielanleitung, Toolbeschreibung und lokaler Hermes-Skillentwurf gepflegt;
Skill bleibt zurückgestellt. Entwicklungsworkflow geprüft, unverändert.
Nach live bereit: 0.69 mit neuer Session verbunden; Gate bestanden. Keine neuen Baustellen, Endpause bestätigt.
Weitere Höhenpiloten gestoppt; Zielwegregistrierung und native Treppen-Endpunkte gezielt klären.

Nachtrag nach dem 0.68-Livepilot: MCP-Log wird beim Laden des Spielstands
über den vorhandenen Sichtbarkeitswechsel automatisch geöffnet. Manuelles
Schließen bleibt bis zum nächsten Laden wirksam. Quell-/Diffprüfung erfolgt;
Gate und Livekontrolle (Laden, Schließen, Neuladen) stehen aus.
Spielanleitung angepasst; Hermes-Skill geprüft, keine geänderte Agentenbedienung.

## 0.68.0 — Gate bestanden, Höhenpilot fehlgeschlagen

A*-Priorisierung mit konservativer Zielkostenschätzung und zielnahem Gleichstand;
Geometrievarianten, native Schlussvorschau und Budgets bleiben erhalten.
`searchDiagnostics` zeigt Bestandsquellen, geprüfte Höhenanschlüsse, native
Bestätigung, Geometrie-/Stützenablehnung und Suchaufwand auch in kompakter Ausgabe.
Begrenzte Stichproben und unbekannte Verbindungen werden ausdrücklich markiert.
[Änderungen, Learning und Abnahmeplan](docs/releases/0.68.0.md).

Neue Regressionen ergänzt, Quell-/Diffprüfung; keine Agent-Builds oder Testläufe.
Menschliches Gate bestanden; Höhenpilot erneut am Suchbudget gescheitert. Kein Commit/Push.
Spielanleitung, Tool-Doku und lokaler Hermes-Skillentwurf aktualisiert;
Entwurf bleibt zurückgestellt. Workflow geprüft, keine fachliche Änderung nötig.
Nach `live bereit`: Backend gestartet, Bridge 0.68.0 und neue Session bestätigt.
Livebefund: 104 Höhenproben ohne bestätigten Übergang; manuelle Zwei-Wege-Gegenprobe gültig. Endpause und null Baustellen bestätigt.
Cockpit ergänzt: unterer Gesamtbalken mit anteiligem Phasenfortschritt;
rechts der Schrittzählung und je Phase sichtbare letzte erfolgreiche
Referenzdauer (Gesamt als Summe). Ohne Referenz `--:--:--`.
Gate erfolgreich; visuelle Cockpitdarstellung nicht separat beobachtet.

## 0.67.0 — Gate bestanden, Höhen-Livepilot fehlgeschlagen

Bridge 0.67.0 mit neuer Session bestätigt. Gleicher erhöhter Lagerfall endet
mit `search_budget_exhausted` nach 2048 Zuständen, ohne Hilfsteile oder Auftrag.
Manuelle Zwei-Wege-Gegenprobe weiterhin nativ gültig und angeschlossen,
Bestandsverbindungen erhalten. Keine Baustellen; Endpause bestätigt.
Keine Releaseabnahme, kein Commit/Push des Codes. Suchpriorisierung und
native Treppenaustritte gezielt diagnostizieren; keine blinde Limiterhöhung.

Auf Nutzerauftrag zusätzlich: Testgate-Cockpit mit sechs festen Balkenzeilen,
Gesamtfortschritt und Laufzeiten; Befehlsdetails ausschließlich in lokalen
Logs. Fehlerursache und Logpfad sichtbar, Exitcode 1 und Fail-fast erhalten.
Gespeicherte Gatehistorie bestätigt sechs erfolgreiche Phasen und Referenzzeiten;
visuelle Konsolendarstellung nicht separat vom Agenten beobachtet.

Eigenkollisionen der Zugangssuche: bisherige Routengeometrie geht in neue
Kanten und Stützenprüfung ein; verschiedene Belegungen am selben Zielknoten
bleiben getrennte, begrenzte Suchzustände. Vorhandene Höhenanschlüsse werden
über native Verbindungen ab fertigen Distriktwegen genutzt, mit regulärem
Zielweg statt erfundener kostenloser Quelle. Schlussvorschau unverändert.
[Änderungen, Regressionen und Livepilot](docs/releases/0.67.0.md).

Quell-/Diffprüfung; keine Agent-Builds, Testläufe oder Installation.
Regressionen im menschlichen Gate, feature-spezifischer Liveabschluss fehlt;
Änderungen bleiben uncommitted.
Spielanleitung, Werkzeugbeschreibung und lokaler Hermes-Skillentwurf fachlich
aktualisiert; Entwurf bleibt zurückgestellt. Workflow geprüft, kein neuer
Ablauf nötig. Vorbestehende Starter-/Hermes-Änderungen bleiben erhalten.
Nach `live bereit` Backend wieder verbunden; Spiel am Ende pausiert.

## 0.66.0 — Kernpilot begrenzt bestanden am 17. Oktober 2026

Mensch meldet Gate bestanden; Bridge 0.66.0 mit neuer Session live bestätigt.
Uferpumpen-Vorprüfung und automatische Zugangssuche stimmen am ursprünglichen
Standort überein. Regulärer Bauauftrag im Entwicklungspiloten, tatsächlicher
Bauarbeiterzugang und Fertigzugang nachgewiesen; keine verlorenen Bestandsanschlüsse.
Echte Terrain- und Objektkollisionen bleiben blockiert. Tempoabweichung und
konkurrierender Start liefern konkrete Codes mit `not_executed`; gültiger
Zwölfstundenlauf erreicht das Ziel und pausiert. Details und Nachweisgrenzen:
[Releasebericht](docs/releases/0.66.0.md).

Gate-Nacharbeiten (Supervisor-Quittung, deterministischer Regressionstest,
xUnit-Assertions) sind durch die aktuelle menschliche Erfolgsmeldung überholt.
Gate-CLI speichert erfolgreiche Phasendauern lokal als Balkenreferenz;
die visuelle Konsolendarstellung wurde vom Agenten nicht separat beobachtet.
Keine Agent-Builds/Tests/Installation. Anleitung, Tool-Doku und lokaler
Hermes-Skillentwurf mit tatsächlichen Belegen aktualisiert; Skill bleibt
zurückgestellt. Vorbestehende Starter-/Hermes-Experimente bleiben separat.

Folgepilot Umweg/Höhe: am 17. Oktober 2026 an einem erhöhten Lagerziel
gescheitert. Automatische Suche erzeugt zehn Hilfsteile mit Treppe/Plattform
auf derselben Zelle; native Gesamtvorschau lehnt korrekt ab. Manuelle
Gegenprobe mit zwei Wegen ab bestehender Treppe ist für dasselbe Ziel gültig,
Eingang und Arbeitszelle verbunden, keine verlorenen Bestandsanschlüsse.
Kein Bauauftrag, keine neue Baustelle, unveränderte Endpause. Suchpfad muss
die Geometrie seiner Vorgängerkanten berücksichtigen; vorhandene Treppenanschlüsse
gezielt prüfen. [Befund und Regressionsziel](docs/releases/0.66.0.md#folgepilot-umweg-und-höhenunterschied).
Der ursprüngliche Uferpumpen-Kernpilot bleibt bestanden; automatische
Umweg-/Höhenzugangssuche ist durch diesen Folgepilot nicht abgenommen.

Weitere Grenzen:
Pending-/Lock-/Kapazitätsfälle nur Gate-Regressionen. Früher Navigations-
Baselinekonflikt nach Simulationsfortschritt und Entfernen einer fremden,
unbegonnenen Testtreppe verschwunden; Ursache nicht isoliert nachgewiesen.
Spiel am Ende pausiert, Bestandspumpe wieder eingeschaltet.

## Agentenregel: Materialdeckung statt Vorratswartepflicht

Auf Nutzerauftrag gilt: Kosten des gesamten Bauplans, Deckung je Gut und insgesamt
sowie vorhandene Erzeuger prüfen; keine Mindestdeckung als Startsperre. Der Agent
entscheidet, beauftragt und verfolgt Lieferungen während der Bauphase.
AGENTS, Spielanleitung, lokaler Skillentwurf und Chargenanleitung aktualisiert;
veraltete Material-Stopptexte ersetzt. Bestehender Struktur-Baupfad unterstützt
Aufträge ohne vollständigen Vorrat bereits. Reine Anleitungskorrektur, keine
neue API, kein Versionssprung und kein neuer Build-/Livetest behauptet.

## 0.65.0 — begrenzt live abgenommen am 17. Oktober 2026

Wasserbauwarnungen verlangen Abfluss-/Rückstauprüfung auch bei geometrischem
Schnitt; Gegenprobe unter Sperrkrone, kontaminiertes Zielwasser und Versorgungsvorrang.
Pumpendiagnose liest native Überflutung, Ansaugwasser/-kontamination und Rohrblockade
auch bei Pause. [Umfang, Regressionen und Pilot](docs/releases/0.65.0.md).
Nach menschlicher Meldung `live bereit` Bridge 0.65.0 verbunden. Trockene
Ansaugung vor Personal/Pause diagnostiziert; vollständiger Kartenscan bestätigt
Rückstaupflicht und identische Compact-/Full-Befunde. Kontrollpumpe eigenständig
gebaut, zugänglich und mit einem nativen Produktionszyklus nachgewiesen.
Nasse Ansaugung mit geringer Kontamination und positives kontaminiertes Testziel
ebenfalls beobachtet. Überflutungs-Positivfall inzwischen bestätigt, auch bei
Gebäudepause und ausschließlich sichtbarem Pausenstatus. Höhere Ersatzpumpe
nicht überflutet; native Produktionszyklen nach Aktivierung nachgewiesen.
Gate-CLI: sechs Phasen mit Abschlusszähler, Phasen-/Gesamtlaufzeiten und
Ergebnisliste; harter Spielstopp ohne Speichern vor den Tests. Laufzeitprüfung
der Anzeige wurde nicht separat beobachtet.
Spielanleitung, Tool-Doku, strategischer Fachvertrag und lokaler Hermes-Skillentwurf
aktualisiert; Skill bleibt zurückgestellt. Fremde Starteränderungen erhalten.
Backend nach Freigabe gestartet. Wetterpilot für Nutzerauftrag unterbrochen:
dreiteilige Deichsperre an oberer Klippenrinne samt Höhenzugang beauftragt.
Frische Diagnose bestätigt alle 30 Teile fertig und zugänglich, keine verlorenen
Bestandsanschlüsse. Drei Deichblöcke schließen die Rinne; sicherer Abfluss und
Schutzwirkung bei künftigem Hochwasser bleiben ungeprüft. Ersatzpumpe lieferte
22 native Zyklen in zwölf Spielstunden. Endpause bestätigt, Wasserbestand 87,
acht Erwachsene und keine Kinder. Releaseabschluss mit Quell-/Diffprüfung;
Livegrenzen und Folgefehler im Releasebericht und Backlog dokumentiert.

## 0.64.0 — begrenzt live abgenommen am 17. Oktober 2026

Bauphasen getrennt von Identitätsverlust (`awaiting_access`), positive
Diagnosebelege für spätere Recovery erhalten, Tempoübergabe an Update-Grenze
mit nachgelagerter Pausenbestätigung. Simulationsübersicht ergänzt aktive
Laufkennung und ausstehendes Tempo. Menschliches Gate laut Nutzer bestanden.
Live: Höhenübergang von `awaiting_access` bis 3/3 fertig, Stopp/Rückbau/Close und
Folgeauftrag ohne Reload, Start/Cancel sowie explizite Tempo-Unterbrechung bestanden.
Frische Endpause 0, keine aktive Laufkennung/Tempoabsicht. Bestandsanschlüsse erhalten.
[Konkrete Belege und synthetisch verbliebene Grenzfälle](docs/releases/0.64.0.md).
Anleitung, Tool-Doku, Simulationserklärung und lokaler Hermes-Skillentwurf
aktualisiert; Skill weiterhin zurückgestellt. Vorbestehende Änderungen erhalten.
Backend nach `live bereit` regulär gestartet; Bridge 0.64.0 bestätigt.
Keine Builds/Tests/Installation durch den Agenten. Historischer `run_not_found`-
Ursprung bleibt ungeklärt; kein neuer Hydraulik- oder Überlebensnachweis.

## Abschluss des Überlebenslaufs — 17. Oktober 2026

Nutzer meldet vollständigen Kolonieverlust und beendet den Lauf. Rettungsziel
nicht erreicht; Klippenmauer nicht gebaut. Letzte native Beobachtung: Tag 70,
12:15, Pause bestätigt, zwei von drei offenen Zugangsplattformen fertig.
Danach Bridge nicht erreichbar; Todesursache und Endpopulation nicht mehr nativ
verifiziert. Kein weiterer Spielauftrag. Backend gestoppt: running=false, gate=true.

0.63.0 einschließlich Bauplaner-Korrektur bleibt im dokumentierten begrenzten
Umfang abgenommen (Codecheckpoint `77a258a`). Kein neuer Build, Versionssprung
oder zusätzlicher Schutz-/Überlebensnachweis. Die neuen Fehler sind offen im
[Backlog](BACKLOG.md); Verlauf und Learnings im
[Pilotjournal](docs/survival-run-2026-10-17.txt).
Releaseabschluss ergänzt; Spielanleitung und lokaler Skillentwurf fachlich
geprüft: kein neues Werkzeug/Parameter, keine zusätzliche Fähigkeitsfreigabe.
Vorbestehende Hermes-/Starteränderungen bleiben außerhalb dieses Abschlusses.

## Bauplaner-Korrektur — 17. Oktober, begrenzt live abgenommen

Überlebenspilot deckte Path-Vorschaufehler, Planersperre und verschluckte
Chargenursache auf. Vorschau-Registrierung/Kollision und Fehlerweitergabe
überarbeitet. Nach Korrektur der Meldungspriorität menschliches Gate bestanden.
Live: belegter Path sauber abgelehnt, wiederholte gemeinsame Vorschauen intakt;
zwei Tanks, Lager und zwölf Umweg-Wege gemeinsam beauftragt, alle 15 Teile fertig
und zugänglich. Keine verlorenen Bestandsanschlüsse. Wasser/Carrot und accept
rückgelesen. Pause bei Tag 51, 14:15; Projekt freigegeben.
Konkrete Fehler busy/requires_pause/not_found live bestätigt. Native Ausnahme,
Chargenpersistenz und Quittungsverlust synthetisch im Gate, kein Live-Crash.
[Änderungen, menschliches Gate und begrenzter Baupilot](docs/planner-repair-2026-10-17.md).
Spielanleitung und lokaler Hermes-Skillentwurf um den Fehlervertrag ergänzt;
Entwurf weiterhin zurückgestellt. Laufzeitbasis bleibt 0.63.0; Anleitung/Skill
um tatsächliche Livebelege und Nachweisgrenzen aktualisiert.
Nebenbefund: Ingame-Fortschrittstext meldet 0/15 trotz strukturiertem 14/15;
separates Darstellungsproblem im Backlog, kein fehlender Bauauftrag.

## Versionsabschluss 0.63.0 — 17. Oktober 2026

Menschliches Gate und begrenzte B15-Liveprüfungen bestanden. Nutzer hat den
Abschluss mit dokumentierter Grenze ausdrücklich freigegeben: native Teilablehnung
mitten im synchronen Platzierungsdurchgang nur synthetisch im Gate geprüft,
nicht live provoziert. Keine neue Abnahme fertiger Höhennavigation.
Explosionsvorwarnung mit möglichen Eindämmungsvorschlägen ebenfalls abgenommen.
[Releaseumfang](docs/releases/0.63.0.md), [B15-Belege](docs/construction-structures-0.63.md).
Anleitung, Werkzeugdokumentation, Kompatibilität und lokaler Hermes-Skillentwurf
aktualisiert; Skill bleibt zurückgestellt. Keine Codeänderung nach bestandenem Gate.

## Darstellungshinweis geklärt — Nutzerentwarnung

Nach den B15-Liveprüfungen meldet der Nutzer leere Ebenen und eine auffällige
Spielansicht. Versionsabschluss war vorübergehend zurückgestellt. Simulation
bei Tag 48, Stunde 13,9375 pausiert; keine weiteren Testeingriffe.
Erste lesende Prüfung: 24 Gebäude ohne Wege vorhanden, darunter Wohnhäuser,
Pumpen, Erfinder und Lager. Testregion 8×8 über drei Höhen besitzt weiterhin
durchgehendes Terrain bis Höhe 3. Gelesener Player.log-Ausschnitt enthält keine
Exception. Das ist kein vollständiger Integritätsnachweis und schließt eine
Mod-/Darstellungsstörung nicht aus. Keine direkte Save-Dateibearbeitung erfolgt;
Ingame-Testplatzierungen und gezielter Rückbau sind oben im B15-Nachweis dokumentiert.
Nutzer anschließend: „stop, passt alles, weiter im test“. Untersuchung damit
beendet; kein bestätigter Schaden. B15-Abnahme fortgesetzt.

## Erweiterung 0.63: Explosionsketten — Warnung im Nutzerumfang abgenommen

Menschliches Gate bestanden, Bridge 0.63.0 in neuer Session bestätigt (17. Oktober).
Frischer Standard-Compact-Scan: 64/64 Kacheln, 65536 Spalten, Objektinventar komplett,
neun Kerne bewertet. Kern (168,130,12) schon vor beobachteter Auslösung mit
`badwater_breach_to_target_potential`, Radius 2 und 14 hypothetisch geöffneten
Terrainzellen sichtbar. Faulwasser (168,126,11), Kontamination ungefähr 1,
Pegel 11.084, Zielzutritt (124,148,2). Compact enthält Schutzposition (177,120,11),
Krone 13 sowie Maßnahmen für Sperrprüfung außerhalb des Sprengbereichs,
Zielzutritt/Wasserentnahme und sichere Umleitung. Full desselben Scans ergänzt
Route/Querschnitt; Route ist ausdrücklich gekürzt. Simulation lief auf Tempo 7;
keine Explosion, Geländeänderung oder Schutzbauten durch diesen Test ausgelöst.

**Abgenommen am 17. Oktober gemäß Nutzerpräzisierung:** frühe Vorwarnung mit
nachvollziehbarem Gefahrenweg und soweit möglich Eindämmungsvorschlägen genügt.
Ein vollständig validierter Schutzbau ist keine Abnahmevoraussetzung.
Vorwarnung und Compact/Full-Ausgabe live belegt; wirksamer Schutzbau nicht behauptet. Schutzquerschnitt `cross_section_incomplete`; keine Bauvalidierung.
Beim genannten Kern bleibt die Schwelle vor/nach Szenario 11, daher nur
`existing_connection_may_worsen`, kein belegter neu entstandener Durchbruch.
Kern (157,186,0) zeigt dagegen eine Szenarioschwelle 6 auf 2. Countdown,
tatsächliche Ereigniswirkung, Einstürze, Bauwerkswirkung, Wasserverteilung und
Zuordnung des Zielgebiets zum konkreten Teich bleiben begrenzt/unbewiesen.
Vollständige Schutzbauplanung und Ereignisvergleich bleiben mögliche Folgearbeiten,
keine Blocker dieser Warnungsabnahme. B15-Abschluss mit Grenze oben dokumentiert.
Anleitung und lokaler, zurückgestellter Hermes-Skillentwurf mit Livegrenze ergänzt.

## 0.63 / B15 — Gate bestanden, begrenzte Livebelege vom 17. Oktober

Menschliches Gate bestanden; native Bridge 0.63.0 und aktuelle Session bestätigt.
Zwei Lager mit einem Go als reale Baustellen beobachtet, beide vor Fertigstellung.
Bei pausiertem Distriktzentrum blieb Lieferung aus; Lauf stoppte korrekt am
12-Stunden-Limit mit bestätigter Pause. Nach Aktivierung des Zentrums schloss ein
Reparaturauftrag einschließlich Originalbauten erfolgreich ab. Frische Diagnose
und Einzelabfragen bestätigen beide Lager fertig/zugänglich; Originalledger freigegeben.
Neue Schema-2-Charge: zwei Lager und ein automatisch ergänzter Weg als ein
Strukturauftrag; alle drei fertig/zugänglich, bestätigte Pause, lokales Journal
mit `whole_structure_finished` synchronisiert. Kein Einzelteil-Dispatch.

Zusatzpilot: nach Plattformfreischaltung über `start_large_project` vier Teile
mit einem Start beauftragt und frisch bestätigt: Treppe, Plattform, oberer Weg
auf unfertiger Plattform und unabhängiger Stapel. Nach sechs Spielstunden war
der Stapel fertig/zugänglich; Höhenbauteile blieben korrekt materialblockiert,
kein falscher Gesamtabschluss. Drei unfertige Testteile kontrolliert entfernt,
Diagnose bestätigte Rückbau und fertigen Stapel, Ledger freigegeben.

Nach Diagnose-Nachbesserung menschliches Gate erneut bestanden. Separater
`execute_vertical_stair_pilot`: fünf Teile sofort eingereicht, danach alle fünf
`confirmed`, Gesamtstatus `ordered`. Beide oberen Wege liegen bereits auf
unfertigen Plattformen. Kein Fertigbaunachweis der Höhenstruktur: 16 Bretter fehlen,
alle fünf Testteile anschließend zurückgebaut. Späterer Pilotstatus korrekt
`stopped/guard_changed`, kein falscher Bauabschluss. Frühere Startablehnung in der
neuen Sitzung nicht reproduziert; damalige Ursache bleibt unbewiesen.
Negativkontrolle geänderter Plan unter belegter ID: `project_start_ledger_conflict`,
`not_executed`, keine zusätzlichen Baustellen. Konkrete Fehlerweitergabe live belegt.

Ebener Pilot: zwei Wege und Lager mit einem Start, zunächst `ordered`, nach vier
Spielstunden alle drei `finished`, Gesamtstatus `completed`; Lager separat fertig
und distriktverbunden bestätigt. Fehlerpilot über manuellen Großprojektstart:
zwei Lager gemeinsam bestätigt, eines gezielt entfernt. Meldung `unconfirmed`,
Teilgrund `missing`, Diagnose `previously_observed_object_removed`; kein Replay.
Restlager fertig, Ledger nach frischem Abgleich freigegeben. Null offene Baustellen,
Tempo 7 separat bestätigt.

Akzeptierte Nachweisgrenze: native Ablehnung mitten im Platzierungsdurchgang
ist synthetisch im bestandenen Gate geprüft, nicht live provoziert. Kein behaupteter
Livebeleg dafür oder für fertige Höhennavigation. Nutzer hat den begrenzten
Versionsabschluss ausdrücklich freigegeben. Kein Code nach Gate geändert.

Fortsetzung nach Nutzerentwarnung: `develop_building_project` live bestanden,
Lager nach 2,3125 Spielstunden fertig/zugänglich, Pause bestätigt; separate
Projektabfrage `completed` und Lagerkonfiguration `Log/accept` zurückgelesen.
Räum-/Gruppenworkflow auf bereits freier Fläche bestanden: ein Lager über native
Gesamtcharge fertig/zugänglich, Pause und automatische Ledgerfreigabe bestätigt
(`group_finished_access_checked_and_released`). Keine neue Abnahme von
Vegetationsräumung. Ein Start bei inzwischen laufendem
Tempo wurde vor jedem Schreibauftrag abgewiesen; Pause frisch bestätigt und
neue Vorschau vor dem tatsächlich angelegten Job gelesen.

Die damalige fehlende Kern-Verknüpfung wurde durch die oben abgenommene
Explosionswarnung ersetzt; historische Grenzen sind kein aktueller Blocker.
Anleitung und lokaler Hermes-Skillentwurf bleiben auf 0.63-Semantik mit offenen
Livegrenzen; Skill bleibt zurückgestellt. [Vertrag und Pilot](docs/construction-structures-0.63.md).

## Versionsabschluss 0.62.0 — 16. Oktober 2026

Menschliches Test-/Build-/Installationsgate bestanden; Abschluss-Liveprüfungen
für Gefahrenbewertung, Compact/Full und Reservoir-Optionen bestanden. Bridge
bleibt 0.62.0; MCP-Nachträge sind durch den Abschlusscommit eindeutig zugeordnet.
[Releaseumfang](docs/releases/0.62.0.md), [Vertrag und Belege](docs/strategic-map-0.62.md).

- Dürre aktiv, alle zehn Quellen Stärke null; 7.458 erreichbare nasse Hohlräume,
  darunter 1.242 potenzielle kontaminierte Zuflüsse. Warnung und konkrete nächste
  Handlung bleiben erhalten. Höchster Restwasserpegel etwa 10,78, Schwelle 5.
- Compact/Full derselben Scan-ID: 10.434 / 87.261 JSON-Zeichen, 88,0 Prozent weniger.
  Gefahren, Maßnahmen, Blockaden und Reservoir-Optionen gleich; keine Terrain-
  Neulesung bei Full, eine frische Sessionprüfung. Auch overviewOnly geprüft.
- Reservoir (117,145,3), 3×3, zwei Ebenen Absenkung: Boden und Seiten lokal
  bestätigt, 18 Rasterwürfel Zusatzvolumen. Fehlender Zufluss ausdrücklich blockiert.
- Dachkonflikt nativ bestätigt; ungültige Parameteränderung konkret abgewiesen,
  gültiger Cache danach unverändert lesbar. Tempo 7 unverändert.

Quellen, Schema/Dispatch, Präsentation, Analyse und Regressionstests erneut
geprüft. Keine Codeänderung nach dem erfolgreichen Gate nötig; keine Agent-
Builds oder Testläufe. README, Einstieg, Kompatibilitätsübersicht, Fachvertrag,
Werkzeugreferenz, Spielanleitung und lokale Hermes-Spielskillvorlage abgeglichen;
keine Installation/Freigabe der zurückgestellten Hermes-Integration.

Grenzen: geometrische Planung, keine hydraulische Flut-/Ankunftsprognose,
kein automatischer Bau/Aushub, kein Speicherbetriebsnachweis. Wasserbauwerke,
Ereigniscountdown, Durchbruchprognose, künftige Kontamination und vollständige
Bauzugangs-/Kostenprüfung bleiben offen. Nächste Priorität: **B15/P0**, komplette
Struktur gemeinsam als Baustellen platzieren, anschließend Fertigstellung verfolgen.
Fremde lokale Änderungen und Rohlogs bleiben außerhalb des Abschlusses.

## 0.62.0 — Karten-, Quellen- und Flussüberblick begrenzt live abgenommen

Menschliches Gate bestanden; Abschluss-Livetest am 16. Oktober 2026 bei Tempo 7.
`scan_strategic_map` erfasst Terrain/Höhlen und Spezialobjekte kartenweit. Mit
`overviewOnly=true` ohne Siedlungsziel: blätterbare Flussabschnitte, geometrische
Verbindungen über nasse/trockene Schwellen, native Quellen und Wetterzustände.
Mit Zielzone zusätzlich repräsentative Korridore und Schutzkandidaten.

Belegt: 64 Kacheln und 65.536 Spalten; zunächst 444 Abschnitte, 776 Verbindungen,
zehn native Quellen. Sämtliche Abschnitts-/Kantenseiten ohne Doppelungen gelesen;
Quellenabdeckung vollständig gemeldet. Sauberer Auslass, verseuchte Höhle und
höhere trockene Schwelle gegen native Zellwerte bestätigt. Quellenhöhen und
Kronenkorrektur live nachgeprüft; alte Session korrekt abgewiesen.

Wetterwechsel von gemäßigt zu Dürre: alle zehn Quellen von ihrer jeweiligen
Stärke auf null, native Dürremodifikatoren von 1 auf 0. Vergleich zum vorherigen
vollständigen Scan meldet Wetterwechsel und passende Quellendifferenzen; lokale
Restflüssigkeit bleibt getrennt ausgewiesen. Folgescan 404 Abschnitte/681 Kanten.
Beobachtungszeiten der zwei zusammenhängenden Vollscans: 2,48 bzw. 3,48 Sekunden.

Grenzen: geometrische Verbindungen, keine gemessene Richtung/Flussphysik;
Wasserbauwerke nicht modelliert. Kein Nachweis von Faulwasserflutwechseln,
zukünftiger Kontamination oder allgemeiner Schutzwirkung. Spezielle native
Aktivierungsregeln waren bei den zehn Quellen nicht vorhanden und bleiben
unbekannt; keine erfundene Prognose. Wetter-Rohwerte sind kein allgemein
abgenommener Countdown. Höhlenkronen sind keine ausführbaren Baupläne.

[Vertrag und Livebelege](docs/strategic-map-0.62.md). Spielanleitung,
Werkzeugreferenz und lokale Hermes-Spielskillvorlage mit Nachweisen/Grenzen
aktualisiert; Hermes-Integration bleibt zurückgestellt. Öffentliche API-Signaturen
geprüft, keine privaten Zugriffe/neuen Abhängigkeiten. B12 bleibt: nach neuen
Werkzeugen/Parametern musste Codex seinen Katalog durch Neustart aktualisieren.
Fremde Arbeitskopieänderungen bleiben außerhalb des Feature-Checkpoints.

## Spielversuch 16. Oktober 2026 — beendet, Learnings dokumentiert

Nutzer beendet den Überlebenslauf nach großflächiger verseuchter Überflutung.
Kein Überlebenserfolg behauptet. Die [Spielanleitung](docs/agent-playing-guide.md)
beginnt jetzt mit der Startfolge Wasser + Beerensammler + Holzfällerfahne als
gemeinsamer Bauauftrag, danach Wohnraum + Erfinder, anschließend Bretter.
Vorratszugriff, Personal, Flutvorsorge und konkrete Diagnosefallen sind destilliert;
fehlende Fähigkeiten stehen im [Backlog](BACKLOG.md). Reine Dokumentationspflege,
keine neue Version oder Erweiterung der technischen Liveabnahme.
Auf Nutzerhinweis ergänzt: initialer Gefährdungsscan auf Zuflussrinnen zur
Wohn-/Versorgungszone sowie präventives Terraforming und Wasserbau im Early-/Midgame,
einschließlich Rückstau, Bauphasen und Wirkungsprüfung; benötigte MCP-Fähigkeiten
im Backlog konkretisiert.

## 0.61.0 — Objektgrenzen und Planungsfehler begrenzt live abgenommen

Beide pauschalen 4.096-Kartenobjektgrenzen entfernt: Navigationsauswahl und
Baustellenisolation. Mehrfache Accessible-Komponenten werden korrekt abgefragt.
Konkrete Vorbedingungen werden bis MCP und Aktivitätszeile durchgereicht;
unbestätigte Ausgänge bleiben unconfirmed. Menschliches Gate nach Korrekturen,
installierte 0.61 und neue Sitzung bestätigt.
Live: 3.404 Zugangsvergleiche auf der großen Karte, gültige kleine Vorschau,
Höhenfehler abgelehnt, sieben verlorene Bestandsanschlüsse erkannt, gültiger
Umweg ohne Zugangsverlust. Vorschauen restauriert, Sitzung nicht gesperrt.
Pausenfehler bei Tempo 7 als project_plan_requires_pause/not_executed samt
verständlicher Ergebniszeile bestätigt. Keine Bauaufträge erteilt.
Reguläre Baufreigabe, Baustellen-/Liefernachweis und Grenzwert-Stresstest
bleiben ausgeschlossen. Anleitung und lokale Hermes-Spielskillvorlage
aktualisiert; Hermes-Integration bleibt zurückgestellt. Simulation wieder auf 7.
[Umfang, Korrekturen und Livebelege](docs/navigation-scope-0.61.md).

## 0.60.0 — Reparaturstart-Diagnose begrenzt live abgenommen

Der frühere `state_conflict` ist im neuen 0.59-Kontrollfall nicht reproduziert:
stabile Vorschau, Reparatur gestartet und abgeschlossen, Elternprojekt frisch
mit 2/2 fertig/zugänglich freigegeben, Pause bestätigt. Ursache des alten
Einzelfalls bleibt offen. Keine spekulative Lockerung der Startprüfungen.
0.60 behebt die pauschale Unbestätigt-Rückmeldung für eindeutig vor dem ersten
Bauauftrag abgewiesene Starts: konkrete Fehlercodes und lesbarer Grund auch
ingame. Unsichere Antworten bleiben unconfirmed; kein automatisches Retry.
Menschliches Gate durchlaufen, installierte 0.60 und neue Sitzung bestätigt.
Falscher Planschlüssel korrekt als not_executed abgewiesen: kein neuer Weg,
kein Projektbeleg, unveränderte Spielzeit und Pause. Frisch geplanter gültiger
Reparaturlauf nach rund 8,34 Spielstunden abgeschlossen; Weg und Elternlager
frisch fertig/zugänglich, Elternprojekt freigegeben, Pause bestätigt.
Zwölf Ergebniszeilen in beiden Dateilogs identisch, keine Schreibverluste.
Mensch bestätigt Ablehnungs- und Reparaturabschlusszeile vollständig und verständlich lesbar.
Anleitung und lokale Hermes-Spielskillvorlage aktualisiert;
Hermes-Integration bleibt uninstalliert/unabgenommen.
[Befund, Grenzen und Livepilot](docs/repair-start-0.60.md).

## 0.59.0 — Bau-Ergebniszeilen begrenzt live abgenommen

Einzelprojekte, Bauchargen, große Projekte und Räum-/Baugruppen erhalten lesbare
Ergebniszeilen, auch ingame und in beiden Dateilogs. Auftrag, Fertigbau, Zugang,
gespeicherter Beleg und frische Prüfung bleiben getrennt. Keine zusätzlichen
Spielabfragen. [Umfang und Livebelege](docs/agent-log-building-outcomes-0.59.md).
Nach menschlicher Bereitmeldung installierte Bridge 0.59 und neue Sitzung bestätigt.
Ein Lager-Einzelprojekt und eine Ein-Gebäude-Charge bis fertig/zugänglich geprüft;
Auftrag, unfertiger Zwischenstand und Abschluss korrekt getrennt. Zehn Bau-Belege
in MCP- und Bridge-Dateilogs identisch, keine Schreibverluste. Mensch bestätigt
beide Ergebniszeilen vollständig lesbar. Ergänzend am Westufer: Vier-Teile-Projekt
bis 4/4 fertig/zugänglich, Diagnose und Freigabe geprüft. Separater Stoppfall:
2/4 fertig, eine Baustelle, ein nicht erteilter Auftrag; zunächst Pause angefragt,
anschließend Pause bestätigt. Nach Fertigbau der vorhandenen Baustelle mit 3/4
und einem aufgegebenen Auftrag freigegeben. Elf weitere Logtexte in beiden Dateien
identisch, keine Schreibverluste; Mensch bestätigt Stopp-/Freigabezeilen lesbar.
Zusatzproben: Räum-/Baugruppe mit sechs Stümpfen und 2/2 fertigen Bauteilen,
Spielzeitbudgetende sowie separater Bauchargenstopp bestanden. 19 weitere
Ergebnisbelege in beiden Dateien identisch; Ingame-Lesbarkeit bestätigt.
Reparaturvorschau zulässig, Start jedoch state_conflict/unconfirmed; kein neues
Projekt/Weg lesend nachweisbar, kein Retry. Reparaturerfolg bleibt offen.
Zeitlimit-/Stopgrund im Kurztext noch allgemein; Ursache nur im Fachbericht.
[Zusatzprotokoll und Diagnosegrenzen](docs/agent-log-live-tests-0.59.md).

Versionspflege: `docs/agent-playing-guide.md` und die damalige Hermes-Skillvorlage (inzwischen entfernt)
auf Umfang und begrenzte Live-Abnahme ergänzt und bisherige Ausschlüsse präzisiert.
Die Hermes-Spielskillvorlage bleibt uninstalliert und als Integration unabgenommen.
Allgemeine Entwicklungsskills benötigen keine neuen spielbezogenen Regeln.

## 0.58.0 — Ergebniszeilen begrenzt live abgenommen

Verständliche Ergebniszeilen für einzelne Gebäudeeinstellungen, Simulationsläufe
und Pflanzchargen in MCP, Ingame-Anzeige und Dateilogs implementiert. Absicht,
Aufrufstatus und bestätigte Wirkung getrennt; kein Wachstums-/Betriebsnachweis aus
bloßen Aufträgen. Nach menschlicher Bereitmeldung Bridge 0.58 bestätigt; Pause,
Ein-Stunden-Lauf und Pflanzbeleg (1 gesetzt, 2 vorhanden, 1 ausgeschlossen) live
geprüft. Elf Ergebnisbelege in beiden Dateilogs identisch, keine Schreibverluste.
Mensch bestätigt alle Ergebniszeilen als vollständig und gut lesbar. [Umfang und Livepilot](docs/agent-log-outcomes-0.58.md).

Versionspflege vor dem Gate: `docs/agent-playing-guide.md` und
die damalige Hermes-Skillvorlage (inzwischen entfernt) für 0.55–0.58 aktualisiert. Paging/Filter, Korrelation,
Dateilogs, Autostart und Ergebnisbelege ergänzt; veraltete aktuelle Versionsangaben
und pauschale Budgetaussagen ersetzt. 0.58 ist im beschriebenen Umfang abgenommen. Nur eine
projektbezogene Spielskillvorlage vorhanden; allgemeine Entwicklungs-Skills
benötigen hierfür keine versionsspezifischen Spielanweisungen. Hermes-Vorlage
weiterhin nicht installiert oder als Integration abgenommen.

## 0.57.0 — Dateilogs und Anzeige begrenzt live abgenommen

2026-10-16: Menschliches Gate und Abschluss-Livetest bestanden. Neue Logdateien je
Laden mit Kolonie-/Spielstandname und UTC-Zeitstempel; keine Rotation/Größenlimits.
Leseaufruf, synthetische Ablehnung, Begründung und Korrelation in beiden Logs
bestätigt, keine gemeldeten Schreibverluste. Erneutes Laden erzeugt neue Session
und Dateien; bisherige Inhalte erhalten. Mensch bestätigt entfernte Ingame-
Korrelationskennungen und automatischen Spielstart am erfolgreichen Skriptende.
[Umfang und Grenzen](docs/file-diagnostics-0.57.md).
## 0.56.0 — Projekt-/Laufkorrelation begrenzt live abgenommen

2026-10-16: Explizite Projekt-, Elternprojekt- und Laufbezüge mit Herkunft
request/result, kombinierte ID-Filter und Anzeige im vorhandenen Ingame-Log
implementiert. Nach menschlicher Bereitmeldung MCP-Liveproben bestanden:
Ein-Stunden-Lauf mit Pause, Anfrage-/Ergebnisherkunft, Projektablehnung,
UND-Filter und Cursorbindung. Mensch bestätigt Ingame-Kennung und Herkunftslabels
als lesbar. Schließpfad des Vorschaltprozesses ohne unnötiges Statusschreiben
korrigiert; ursprüngliche Windows-Sperrursache bleibt unbekannt.
[Umfang, Grenzen und Pilot](docs/agent-log-correlation-0.56.md).

## 0.55.0 — B10 Agentenlog begrenzt live abgenommen

2026-10-16: Nach xUnit2013-Korrektur menschliche Gate-Freigabe durch `live bereit`;
installierte Bridge 0.55.0 und aktuelle Session bestätigt. Cursor trotz neuer
Aufrufe ohne Duplikate, Werkzeug-/Statusfilter, Nulltreffer und fremder Cursor
live geprüft. Frischer Puffer ohne Verdrängung: Lückenangaben korrekt null.
Überlauf/Löschen sind synthetische Gate-Nachweise, kein zusätzlicher Livebeleg.
Keine Freigabe zurückgestellter Experimente. Projekt-/Laufkorrelation bleibt offen.
[Vertrag und Nachweis](docs/agent-log-pagination-0.55.md).

## 0.54.0 - Pflanzen- und Flaechenjournal begrenzt live abgenommen

Menschliches Gate und Abschluss-Livetest am 12. Oktober 2026 bestanden.
Pflanz-/Faellmarkierungen, reale Pflanzenwechsel und Baum-/Stumpfunterscheidung
mit ehrlicher Herkunft; Compact/Full, Filter und Cursor live bestaetigt.
Menschliche Eingriffe bleiben unknown. Neue Sitzung startet ohne Altbestands-
ereignisse; alte Session wird abgewiesen. Produktionsregression bestanden.
Grenzen: abgetastete Zustandswechsel, keine lueckenlose Ereignis-/Erntehistorie.
[Vertrag und Livebelege](docs/spatial-journal-0.54.md).


## 0.53-Nacharbeiten abgeschlossen

Menschliches Gate und gezielter Livetest am 12. Oktober 2026 bestanden.
Native Ablehnung planting_preview_changed meldet not_executed; anschliessende
frische Vorpruefung bestaetigt unveraenderte Markierungen. Transportunsicherheit
und andere Konflikte bleiben unconfirmed (automatische Gegenproben im Gate).
Compact laesst rein passende Zellbeispiele weg: 1165 statt 2929 JSON-Zeichen,
60,2 Prozent kleiner bei gleichem Schluessel und gleichen Summen. Zusatzblockaden
bleiben sichtbar; Full vollstaendig. Testmarkierung wiederhergestellt, alle acht
Zellen frisch nachgelesen, Endpause bestaetigt. Keine Pflanzen oder Stapel entfernt.



## 0.53.0 - Sammelpflanzung begrenzt live abgenommen

Menschliches Gate bestanden; nativer Pilot am 12. Oktober 2026 bestaetigt.
Bis acht Rechtecke je 8x8, gemeinsame native Vorpruefung mit Arbeitsreichweite,
Feuchte und Belegungsart. Zwei Rechtecke mit acht Zellen live geprueft:
zwei Markierungen gesetzt, vier passende belassen, fremdes Feld und Weg ausgeschlossen.
Falsche Pflanzenart/Hoehe und veraltete Vorschau abgewiesen; Zustand frisch gelesen.
Compact/Full gleiche Summen; Testmarkierungen wiederhergestellt, Endpause bestaetigt.
Nachweis gilt fuer Markierungen, nicht Wachstum; 512-Zellen-Grenze automatisiert
getestet, nicht als grosser Liveauftrag. Kleine 0.52-Nacharbeiten im Testgate geprueft.
Nacharbeiten zu Ablehnungsstatus und Compact ebenfalls live abgenommen.
Fruehere sporadische Gatefehler bleiben ohne bewiesene Ursache.
Kein allgemeiner Aquakultur- oder anderer Fraktionsnachweis.
[Vertrag und Livepilot](docs/planting-batch-0.53.md).

## 0.52.0 begrenzt live abgenommen — Flächenräumung und Gruppenbau

Zusammenhängender Hintergrundablauf: Pflanzmarkierungen entfernen, bis 64
Vegetationsziele räumen, auf echte Fertigstellung warten, frisch gemeinsam planen
und bis acht Gebäude samt Wegen autonom bauen. Eine Fläche bis 8×8 auf einer Höhe;
Rohstoffstapel ignorieren und niemals löschen. Zeitlimits, keine automatische
Wiederholung nach Unsicherheit/Neustart. Compact/Full und lokale Statusabfrage.
Menschliches Gate bestanden; 0.52.0 am 12. Oktober 2026 begrenzt live abgenommen.
Räumbudget-Stopp, autonome Zweiergruppe, Pflanzmarkierungen und Bau auf
einer Rohstoffstapel-Zelle bestätigt; Testbauten entfernt, Endpause belegt.
[Vertrag und Abnahmepilot](docs/cleared-building-groups-0.52.md).

## 0.51.0 — Gruppen-Rückwärtssuche begrenzt live abgenommen

`plan_region_buildings` erweitert Gruppen um begrenzte Rückwärtssuche und
`reservedAreas` (bis acht Ausschlussvolumen). `searchBudget` 1–32 native Vorschauen,
Standard 32; höchstens 256 Suchzustände einschließlich Kandidatenprüfungen.
`maxCandidates` 1–4 gilt pro Erweiterungszustand. Bewertung: mehr Wunschgebäude,
danach weniger neue Wege, danach weniger Zugangshilfen. Bestätigter bester
Teilplan bleibt bei Budgetende erhalten; keine globale Optimalitätsbehauptung.
Native Prüfung hält auch neue Wege/Treppen/Stützen aus den Volumen heraus.
Bestehende Wege bleiben nutzbar; Reservierungen gelten nur für den zurückgegebenen
Plan. Keine dauerhaften Siedlungsreservierungen. Compact/Full erhalten denselben
Plan und Suchstatus. Grenzen acht Gebäude/32 Teile/32×32/acht Höhen unverändert.
Nach menschlichem Gate live bestätigt: Rücksprung senkt einen Viererplan von
13 auf zehn Hilfsteile. Ziel-/Wegeausschluss und Höhenkontrolle bestanden.
Zweiergruppe samt Treppe/zwei Wegen autonom fertig: 3,65625 Spielstunden,
10,72 reale Sekunden; Zugänge und null Bestandsverluste bestätigt. Compact
61.686 → 11.944 JSON-Zeichen (80,6 % kleiner), gleicher Plan/Schlüssel/Suchstatus.
Fünf Testobjekte entfernt, keine Rohstoffe gelöscht, Endpause bestätigt.
[Vertrag und Abnahmepilot](docs/group-search-0.51.md).

Aktueller Abschluss: **0.52.0**, begrenzt live abgenommen.
Diese Datei enthält darunter datierte Abnahmen und historische Zwischenstände.
Ältere „Gate offen“, „aktueller Auftrag“ und „nächster Schritt“-Aussagen gelten
nur für ihren damaligen Abschnitt, nicht als heutige Arbeitsanweisung.
Offene Arbeit ausschließlich im [bereinigten Backlog](BACKLOG.md); lokale
unabgenommene Änderungen siehe [Dokumentationsprüfung](docs/documentation-audit-2026-10-12.md).

## 0.50.0 — effizientere Gruppenplanung begrenzt live abgenommen

Gemeinsame Flächendaten innerhalb eines Gruppenaufrufs, frische Pause-/Sessiongrenzen
und unveränderte native Gesamtprüfung. Frühfilter anhand nativer Belegungsverträglichkeit (BlockOccupations.Intersects);
unbekannte Profile/fehlende Geometrie bleiben nativ zu prüfen.
`readMetrics` nennt tatsächliche Reads, Cachetreffer und native Vorschauen.
Antworten nennen Belegungsarten: geplantes Gebäude mit Index/Vorlage, Bestandsgebäude,
Weg, Vegetation, Rohstoffstapel oder unbekanntes Objekt. Regionsbelegung ist Kontext,
kein erfundener Ablehnungsgrund. Compact bündelt Filterbefunde und beschränkt
Bestandsbeispiele mit total/truncated; `detail=full` enthält alle Belege.
Acht Gebäude/32 Teile bleiben. Nach menschlichem Gate: gemischte Dreiergruppe
mit vier gemeinsamen Wegen live fertig und zugänglich, Endpause bestätigt.
28 Reads eingespart, drei statt sechs native Vorschauen, Compact 71 % kürzer.
Testobjekte entfernt; Bestandswege und Rohstoffstapel erhalten.
[Vertrag und Pilot](docs/efficient-group-planning.md).

## 0.49.0 — gemeinsame regionale Gebäudegruppe begrenzt live abgenommen

`plan_region_buildings` erweitert: exklusiv zu `template` eine Liste `templates`
mit 1–8 Gebäuden, Wiederholungen erlaubt. Suche einmal je unterschiedlicher Vorlage,
gemeinsame native Vorschau nach jeder Ergänzung; Zugangshilfen und Wege gemeinsam
berechnen. Maximal vier Kandidaten je Gebäude / 32 native Vorschauen insgesamt.
`complete/partial/none` bezeichnet Planung, nicht Bauabschluss. Unveränderte
`steps` und `planKey` an `run_large_project` übergeben; fehlende Gebäude stehen
in `unplaced`. Compact standardmäßig, `detail=full` mit allen Vorschau-Belegen.
Greedy in Eingabereihenfolge, keine optimale oder vollständige Standortsuche.
32 Gesamtteile, 32×32 und acht Höhen bleiben. Nach menschlicher Bereitmeldung
Bridge 0.49.0 bestätigt: drei Bänke plus vier gemeinsame Wegstücke in einem
Startauftrag fertig und zugänglich, 3,03125 Spielstunden / 8,85 reale Sekunden.
Teilplan- und belegte Einzelflächenprobe bestanden; Testbauten/Wege entfernt,
Bestandswege und drei zurückgewonnene Rohstoffstapel erhalten, Endpause bestätigt.
[Vertrag und Pilot](docs/group-building-plans.md).

## 0.48.0 — regionale native Baupläne begrenzt live abgenommen

Menschliches Gate ausdrücklich bestanden; Bridge 0.48.0 live bestätigt.
Gedrehte Tank-Geländekante in Vorprüfung, Suche und nativer Vorschau abgelehnt.
Regionale Positivprobe: vier Kandidaten, zwei nativ geprüft/bestanden, zwei
ungeprüft; unveränderter Bankplan nach 4,78125 Spielstunden fertig und zugänglich.
Zusatzwegprobe: zurückgegebene Abhängigkeit Weg → Bank unverändert ausgeführt,
zwei Teile nach 1,84375 Spielstunden fertig. Beide Projekte frisch diagnostiziert,
keine verlorenen Anschlüsse, freigegeben. Zwei Testbänke und Testweg entfernt,
Bestandswege erhalten, keine Rohstoffe gelöscht; Endpause bestätigt.
Doku und Agentenanleitung aktualisiert. [Vertrag und Grenzen](docs/regional-building-plans.md).
Keine allgemeine 3D-Standortsuche oder vollständige Bauphasenschutz-Abnahme.

## 0.47.0 — günstige Statuslesung und begrenztes Warten begrenzt live abgenommen

inspect_simulation_run und inspect_large_project: waitSeconds 0–30, optional after,
diagnose=false als Standard. Einfache Statuslesungen ohne teuren Diagnosescan;
Diagnose auf Wunsch einmal nach dem Warten. Sessiongebundene Änderungskennung,
Beobachtungszeit und Abfragezahlen, kein Schreibauftrag/Tempo-/Budgeteingriff.
Menschliches Gate nach Waldprotokoll-Korrektur ausdrücklich bestätigt. Live 0.47:
30,007 Sekunden / 30 leichte Reads ohne Diagnose; Abschluss nach weiteren
11,274 Sekunden mit Pause. Bekannte Blockade sofort; explizite Diagnose genau
einmal. Zweiteil-Projekt nach 1,024 Sekunden abgeschlossen, Full-Diagnose mit
eigener Beobachtungszeit. Testwege entfernt, Bestandswege erhalten, Endpause.
[Vertrag, Grenzen und Nachweis](docs/lightweight-status-wait.md).

Gate-Rücklauf 2026-10-12: MCP-Build bestanden, Unit-Suite bei
ForestryRangeTests.Intervening_other_marking_change_is_reported_without_undo
mit UnauthorizedAccessException beim Überschreiben des Protokolls abgebrochen.
Beide Waldaktionen verwenden jetzt wie BuildBatchStore File.Replace für bestehende
Checkpoints und File.Move nur beim ersten Anlegen. Keine Löschung des alten
Checkpoints vor Austausch, keine Aktions-/Testwiederholungsschleife. Konkreter
externer Auslöser der Windows-Zugriffsverweigerung unbekannt. Regressionen prüfen
persistierte Endzustände, leere Tempdateimenge und Replay ohne erneuten Eingriff.
Anschließend menschliches Gate vollständig bestanden und Installation vom Nutzer
bestätigt; 0.47-Statuspilot bestanden. Kein Beleg für eine generelle Beseitigung
aller extern verursachten Dateisperren.

## 0.46.0 — kompakte Bauprojektberichte begrenzt live abgenommen

Nach Größenkorrektur menschliches Gate und Zweiteil-Livepilot bestanden.
Start trennt zwei Versuche von noch null bestätigten Platzierungen; Abschluss
und frische Diagnose bestätigen zwei fertige zugängliche Teile sowie Pause.
Compact/Full: Ledger 1.384/1.856 UTF-8-JSON-Bytes (25,4 % kleiner), fertige
Diagnose 1.065/1.278 Bytes (16,7 % kleiner); gemeinsame Werte unverändert.
Unterirdischer Weg bleibt native_invalid, pilotEligible=false, restored=true.
Testwege entfernt, Fläche leer und Bestandsanschluss bestätigt; keine Rohstoffe
gelöscht. Doku/Agentenanleitung angepasst, Ausführungslogik und Limits unverändert.
[Vertrag und begrenzte Abnahme](docs/compact-project-reports.md).

## 0.45.0 — Aktionsergebnisse und Laufberichte begrenzt live abgenommen

Menschliches Gate nach korrigierter Integrationserwartung bestanden; Bridge 0.45.0
live bestätigt. Zwei ungültige Gebäudeauswahlen melden not_executed und legen
keinen Lauf an; lesendes run_not_found meldet not_applicable.
Drei Spielstunden mit zwei Gebäuden: automatische Pause, identische historische
Messwerte und Diagnosebefunde in Compact/Full; Text und StructuredContent gleich.
Compact 3.436 statt 8.206 UTF-8-JSON-Bytes (58,1 % weniger in diesem Pilot).
Erfinder zwei native Zyklen, Papierfabrik null mit mehreren Blockaden.
Vollständige Inventarbelege nur in Full; keine zusätzliche interne Lesekostenersparnis.
Transportfehler synthetisch im Gate, keine absichtlichen Live-Verbindungsabbrüche.
[Vertrag und Abnahme](docs/action-results-and-run-reports.md).

## 0.44.0 — Durchsatz und Zeitanteile begrenzt live abgenommen

Nach Korrektur der Fortschrittsbasis bei unveränderter Spielzeit menschliches
Gate und Zweierpilot bestanden. Zwei Messfenster zu je drei Spielstunden:
Erfinder vier/fünf native Zyklen; Papierfabrik nach freiem Lager null → zwei
Zyklen, Lagerstillstand 3 → 0,65625 Stunden. Automatische Pause, unveränderlicher
Altbericht und mutationsfreie Auswahlfehler bestätigt. Ursprüngliche
Lagereinstellungen wiederhergestellt; 20 transportierte Papier bleiben erhalten.
Achtfachumfang automatisch geprüft, zwei Gebäude live. Zeitanteile sind
beobachtete Schätzungen; Rezeptmengen keine Liefermengen.
[Vertrag und Abnahmegrenzen](docs/production-throughput.md).

## 0.43.0 — Engpassdiagnose begrenzt live abgenommen

Auch das abschließende menschliche Gate der Beschreibungskorrektur und die erneute Zweierdiagnose sind bestanden. Der aktualisierte Werkzeugkatalog zeigt korrekte Umlaute. 0.43.0 ist im dokumentierten Umfang abgenommen.

Menschliches Gate und begrenzter Abschluss-Livepilot bestanden. Grill,
Papierfabrik und Erfinder liefern verwertbare Diagnose; Quellenbestand und Wege
separat bestätigt. Freies Papierlager erkannt, Leermodus korrekt ausgeschlossen;
Kastanienmangel mit lokal/global null korrekt diagnostiziert. Gemeinsamer Lauf:
Erfinder ein Zyklus, Papierfabrik kein Zyklus wegen vollem Ausgang, Zeitlimit und
automatische Pause bestätigt. Testeinstellungen wiederhergestellt, keine Abrisse
oder Rohstofflöschung. Achtfach- und Teilabdeckungsgrenzen automatisch geprüft,
nicht als achtfacher Live- oder Lasttest abgenommen.
[Vertrag und Nachweisgrenzen](docs/production-bottlenecks.md).

## 0.42.0 — gemeinsame Einrichtung begrenzt live abgenommen

Nach Personal-Untergrenzenkorrektur menschliches Gate und Abschluss-Livepilot
bestanden: Dreierkonfiguration, zwei Erfinder automatisch bis Produktionsnachweis,
Rezept-/Zustandskonflikte vollständig vor Änderungen abgewiesen. Historie stabil,
Grill zurückgestellt und Simulation pausiert. Achtfachgrenze/Personaländerung
im Sammelpfad automatisch; Livepersonal blieb Soll 1.
[Vertrag und Nachweisgrenzen](docs/building-configuration-batch.md).

## 0.41.0 — Produktionsnachweis begrenzt live abgenommen

Menschliches Gate und drei Livekontrollen bestanden: Grill/Bäckerei gemeinsam
mit nativen Zyklusabschlüssen; pausiertes Sägewerk ohne falschen Erfolg;
Konfigurationsänderung an Papierfabrik beendet den Lauf mit Fehler und Pause.
Historischer Positivbericht unverändert, Papierfabrik wieder aktiviert,
Simulation pausiert. Achtfachgrenze automatisch, zwei Gebäude gemeinsam live.
[Vertrag und Nachweisgrenzen](docs/building-production-proof.md).

## 0.40.0 — Änderungsjournal begrenzt live abgenommen

Menschliches Gate und Abschluss-Livepilot bestanden: 30 Einträge mit Bau/Entfernung,
Gebäudepause, Tempo und Fertigstellung; MCP-Herkunft getrennt von unknown.
Cursor/Filter/Fehlerfälle und Sessionwechsel geprüft, keine Erfassungsfehler.
Rezept-/Lager-/Sollpersonal-/Abrissmarkierungsfelder noch nicht einzeln live belegt.
[Nachweis und Grenzen](docs/recent-actions-journal.md).

## 0.39.0 — begrenzt live abgenommen

Menschliches Gate und Such-/Sammelpilot bestanden: 15 Produktionsgebäude in einem
nativen Betriebsrequest; drei Einzelkontrollen identisch. Kompakte Batchantwort
70,6 % kleiner als Full. Höhen-/Vorlagen-/Wegefilter, Teilabdeckung und negative
ID-/Sessionkontrollen geprüft. Inaktiver Verbraucher korrekt eingeordnet;
Papierfabrik wieder aktiviert, Simulation pausiert. Liveumfang 15 statt maximal 32.
[Nachweis und Grenzen](docs/building-query-batch.md).

## 0.38.0 — begrenzt live abgenommen

Menschliches Gate und Rezeptpilot bestanden: Rezeptwahl/Rücklesung an Grill und
Bäckerei, veralteter Erwartungswert und unbekanntes Rezept abgelehnt, Pausekontrolle
geprüft. Bäckerei-Ausgangsbestand Brot 180 → 190 nach begrenztem Lauf; automatische
Pause an Tag 629, Stunde 10. Ursprüngliche Rezepte wiederhergestellt, keine Bauten
oder Rohstoffe entfernt. [Nachweis und Grenzen](docs/recipe-operation-settings.md).

## 0.37.0 abgeschlossen

Versionsabschluss am 2026-10-11 nach menschlichen Gates und begrenzten Livetests;
letzter Funktionscheckpoint `7cec7ec`. [Umfang und Grenzen](docs/releases/0.37.0.md).
Sondertests bleiben ausdrücklich zurückgestellt. Nächster beauftragter Ausbau:
Rezeptwahl und gebündelte Inbetriebnahme-Diagnose, mit eigenem menschlichem Gate.

## Live abgenommen: interne Waldabfragen gebündelt

Menschliches Gate und Vergleichspilot 2026-10-11 bestanden: Übersicht 54→32
native Reads (−40,7 %), Zweibaum-Schutz 113→40 (−64,6 %) bei identischem Bestand.
256er-Reichweiten-/Markierungsseiten, Katalogwiederverwendung und gezielte
Schlusslesung; vollständige Zählung und Kontrollen erhalten. Mengenlimit, Replay,
Entfernung beider Testmarkierungen und unveränderte Eichen bestätigt.
Spiel pausiert, Spielzeit unverändert, keine Rohstoffe gelöscht.
[Vertrag und Vergleichsnachweis](docs/forestry-range-protection.md).

## Live abgenommen: vollständige Waldreichweite und artenbezogener Fällschutz

Menschliches Gate einschließlich nativer Baumklassifikation und begrenzter Livetest
am 2026-10-11 bestanden. 446 Reichweitenzellen, 16 Teilflächen, 54 interne Reads;
18 lebende Kiefern, sieben Kiefernstümpfe und eine tote Kiefer getrennt. Löwenzahn
korrekt ausgeschlossen. Zwei räumlich getrennte Kiefermarkierungen in einem
Schutzaufruf entfernt und rückgelesen; 71 markierte lebende Eichen unverändert.
Mengenlimit vor Mutation und lesendes Replay bestätigt. Testmarkierungen entfernt,
keine Bäume/Rohstoffe gelöscht, Spielzeit unverändert und pausiert.
Keine dauerhafte Schutzregel; vollständiger Schutzaufruf benötigte 113 interne
Reads. Weitere interne Bündelung bleibt Optimierung, kein weiterer Spielausbau.
[Vertrag und Nachweis](docs/forestry-range-protection.md).

## Begrenzt live abgenommen: große Gebäude bis 512 native Blöcke

Menschliches Gate und regulärer DomedGarden-Großbau am 2026-10-11 bestanden:
zwölfteiliger Gesamtplan mit zwei Treppen und neun Wegen autonom fertig,
Bauarbeiterzugang und fertiger Eingang separat bestätigt; Ledger freigegeben.
Kataloglimit 512 und große/gedrehte Hex-Geometrie lesend nachgewiesen.
32 Teile, 32×32 und acht belegte Höhen bleiben unverändert.
Nutzerentscheidung: Großbau-Nachweis sichern, Sondertests zurückstellen.
Keine pauschale Abnahme der sechs Vorlagen: eingangslose Gebäude, native
Hex-Platzierung und besondere Standortkontrollen bleiben offen. Gemessener
Forschungsaufwand dafür etwa 12–13 reale Stunden ohne weiteren Ausbau.
Spiel pausiert, Veredelung aktiv. DomedGarden und elf Zugangsteile stehen noch;
Rückbau offen, keine Rückgewinnungsrohstoffe gelöscht.
[Umfang, Nachweis und Grenzen](docs/large-building-geometry.md).

## Live abgenommen: Produktionslauf bis Zielbestand

Explizit beauftragt: bis acht gleichzeitige Güterziele plus optional Forschung.
Bestehendes `run_simulation_for` erweitert, feste Zeitlimits/automatische Pause
beibehalten, vorhandenes Holz-Ziel kompatibel. Frische, begrenzte Manufakturdiagnose
trennt native Zutaten-/Personal-/Kapazitäts- und Kraftbefunde von unbekannter
Liefer- oder Windursache. Nach menschlichem Gate am 2026-10-11 live bestanden:
zwei Güter plus Forschung gleichzeitig, automatische Pause nach 30,20 Sekunden,
bereits erfülltes Forschungsziel ohne Zeitfortschritt, unbekanntes Gut ohne
Laufanlage sowie Zutatenmangel mit Zeitlimit statt Zielerfolg. Historischer
Abschlussbeleg bleibt bei frischer Diagnose eingefroren. Spiel pausiert an
Tag 611, Stunde 12,09375; keine offenen Testläufe oder neuen Testbauten.
[Vertrag, Nachweis und Grenzen](docs/production-target-runs.md).

## Begrenzt abgenommen: Bauarbeitszugang und automatische Ergänzung

Nach menschlichem Gate am 2026-10-11 begrenzt live geprüft: fehlender Arbeitsweg
wird abgewiesen, automatische Treppe plus zwei Wege erschließen denselben
Wetterstationsplatz; echter Bauarbeiterzugang und Fertigstellung belegt. Native Wegprüfung zu freien
Fundament-Nachbarzellen, erneute Prüfung vor Auftrag, Berücksichtigung späterer
Planobjekte und konkreter Blockgeometrie; automatische Hilfenplanung mit zwölf
Zellen Suchrand und zielbezogener Ablehnungsdiagnose, gemeinsamer 0.37.0-Build.
Native hypothetische Bauarbeiterreichweite bleibt unbekannt, Zugangshilfen behalten
ihre Laufzeitprüfung. [Abnahme und Grenzen](docs/automatic-access-planning.md).
Materialwartezeit erforderte nach dem 48-Stunden-Chargenlimit einen separaten
Produktionslauf. Alle Teile fertig, Ledger freigegeben, belegter Weg als Ziel abgewiesen.
Ergänzend bestanden: Windrad über vorhandenem Weg, Mast-auf-Weg-Gegenprobe und
kleines Lager unter dem Rotor. Bauzugänge und fertiger Lagereingang separat belegt.
Beide Zusatzpiloten autonom fertig (10,26 bzw. 11,92 Sekunden), Ledger freigegeben.
Testgebäude, beide Testwege und Treppe entfernt; alle Rückgewinnungsstapel regulär
eingesammelt. Testfläche frisch mit null Objekten bestätigt. Nachbarwerkstatt
weiterhin erreichbar. Keine aktive Agentencharge, Spiel zuletzt Tempo 7.
Spätere Überbauung bleibt ungeprüft.

## 2026-10-11: Harzveredelung und erste Folgegebäude fertig

Holzwerkstatt auf der trockenen Terrasse (49,13,6), eigene Windturbine, zwei
Kraftwellen und zwölf Wege in einer autonomen Charge fertig. Native Netzverbindung,
Besetzung, aktives TreatedPlank-Rezept und tatsächlicher Output separat belegt.
Zwei kleine Lager für veredelte Planken eingerichtet. 18 lebende Kiefern in der
vollständigen Harzsammlerreichweite vor Fällung geschützt; zehn ausgewachsen.

Wendeltreppe (47,12,6) und Wetterstation (49,23,6) fertig. Ursprüngliche leere
Wetterstationsbaustelle mangels Bauzugang entfernt und an vorhandene Straße
verlegt; keine Rückgewinnungsrohstoffe gelöscht. Alle zugehörigen Ledger frei,
Spiel an Tag 594 um 2,21875 Uhr pausiert. Zwölf veredelte Planken verbaut,
Restbestand null, Forschung 92. Versorgung bleibt eingerichtet.

Abdeckung einschließlich Biberstatue, Ressourcenzähler und Harzsammler:
**72/157 reguläre Typen, 85 noch nicht nachgewiesen**. CSV aktualisiert.
Nutzerentscheidung nach Pilot: Spielausbau stoppen und MCP-Ausbau planen.
Alle 18 TreatedPlank-Bautypen würden insgesamt 67.650 Forschungspunkte,
3.185 veredelte Planken und 2.261 Metallblöcke benötigen; sechs sind technisch
nicht unterstützt. Nächste Entwicklungsplanung: Bauzugang vor Auftrag,
zielgesteuerte Produktionsläufe und anschließend große Gebäudefälle.
Kein Implementierungsauftrag aus dieser Planungsentscheidung; Testgate bleibt.

## 2026-10-11: Automatische Zugangsplanung — begrenzt live abgenommen

Nach menschlichem Gate/Bereitmeldung und Katalogübernahme: ebener Zweiteiler
in 4,11 Sekunden, Höhen-Sechsteiler in 11,83 Sekunden fertig. Jeweils ein Run;
Wege, Treppe, Stützen und Abhängigkeiten automatisch ergänzt. Eingänge und
benachbarte Gebäude separat erreichbar, beide Ledger abgeschlossen.
Bestandsweg-Gegenprobe ohne Auftrag abgewiesen. Keine pauschale Baustellenvorschau-
oder 32-Teile-Freigabe. [Nachweise und Grenzen](docs/automatic-access-planning.md).

Sieben Testobjekte rückgebaut; letzter Rohstoffstapel (33,4,4) und sein Testzugang
(33,5,4) bewusst erhalten. Keine Rohstoffe gelöscht. Spiel pausiert; Restweg erst
nach nachgewiesener Sammlung entfernen.

## 2026-10-11: Projektabschluss und Bauzugangsreparatur — begrenzt live abgenommen

Beauftragte Erweiterung im gemeinsamen 0.37.0-Build: frische Projekt-/Material-/
Zugangsdiagnose, expliziter Ledgerabschluss ohne Replay und begrenzte Reparatur-
chargen mit eigenen Wegen, Treppen und Plattformen für bekannte Originalbaustellen.
Vier neue MCP-Werkzeuge; Bau- und Tempogates sowie Sitzung/Eltern-ID gebunden.
Alte Aufträge bleiben terminal; unbeauftragte Restteile nach Abschluss neu planen.
Fremde Baustellen und unbestätigte fehlende Wirkungen bleiben gesperrt.
Regressionsfälle, Transport-/Katalogverträge und Agentenanleitung ergänzt.
Menschliches Gate und gezielter Livetest bestanden: unerreichbare Turbine konkret
diagnostiziert, fünfteilige Zugangsreparatur samt Originalbau autonom fertig.
Abschluss wiederholbar ohne Replay; neues Lager in derselben Sitzung fertig.
Belegte Wegzelle und fremde Baustelle korrekt abgewiesen. Alle Testledger frei.
Nach Rückbau durch Agent und Spieler nur drei ursprüngliche Wege im Testbereich.
Nutzerkorrektur: Rückgewinnungsrohstoffe künftig erhalten, Stapel nicht löschen.
[Ablauf, konkrete Nachweise und Grenzen](docs/project-recovery.md).

## 2026-10-11: Autonome Bauchargen — begrenzte Liveabnahme bestanden

Nach ausdrücklichem Go: `run_large_project` implementiert. Die Mod beauftragt
den gesamten unabhängigen Satz pausiert, startet danach die Simulation und
übernimmt explizite Stütz-/Zugangsabhängigkeiten bis fertig oder blockiert.
Kein Advance je Bauteil; feste Zeitbudgets, bestätigte Abschlusspause und
kein Replay bei Verbindungsverlust. Alter manueller Ablauf bleibt kompatibel.
Nach menschlichem Gate/Bereitmeldung und einmaliger Codex-Katalogübernahme:
Fünfteiliger Höhenbau in 34,19 Sekunden, neunteilige Weg-/Turbinen-/Lagercharge
in 27,90 Sekunden vollständig fertig und jeweils automatisch pausiert.
Zwei ausführende Starts für 14 Teile, kein Advance oder separater Simulationslauf.
Beide Lagereingänge separat zugänglich; fertige Turbine separat bestätigt.
Negativkontrolle auf Hauptweg: elf gefährdete Verbindungen, Start abgewiesen.
Ein-Stunden-Kontrolle: blocked/game_time_limit mit Pause; abhängiges Lager fehlt
nachweislich. Anschließend alle 15 Testobjekte einschließlich acht Wegzellen
und sechs Abrissschuttstapel entfernt; Testflächen nachgelesen, Bestandswege
erhalten. Terminales Ledger sperrt weiterhin weitere Großprojekte dieser Sitzung.
Spiel pausiert. Keine Liveaussage zu 32 Teilen/acht
Ebenen, Kraftbetrieb oder Wiederanschluss mitten im aktiven Lauf.
[Technik, Grenzen und konkreter Pilot](docs/autonomous-construction-batches.md).

## 2026-10-11: Produktionsketten angelaufen, alle Laufräder ersetzt

Achtteilige Kraftleitung von Mühle zur Windturbine über Z6/7/8 fertig und
Projekt completed. Beide im selben Netz; später 128 Erzeugung beobachtet,
zuvor Flaute. Zehn Weizenpflanzen tatsächlich gewachsen; südliche Farm auf
Wheat priorisiert. Nutzer hat Mühle auf WheatFlour und Bäckerei auf Bread
gestellt und Bäckerei aktiviert. Brennstoff geliefert, Besetzung schwankt;
Bäckerei nun Arbeitsplatzpriorität High, separat rückgelesen. Später 35 Brote
und fünf Mehl direkt in der besetzten Bäckerei nachgewiesen. Zwei Dandelionpflanzen führten zu zwei Antidote im
Kräuterkundler. Zwei zusätzliche Erfinderplätze (44,19,6)/(47,18,6), jeweils
Drehung 2, samt zwei Wegen fertig; Forschungsprojekt completed, beide später
besetzt, Forschungsbestand bis 852 beobachtet.

Alle fünf Laufräder durch vier zusätzliche Windturbinen ersetzt: Papierfabrik
(41,22,8), unteres Sägewerk (24,8,5), oberes Sägewerk (38,2,6), Zahnradwerkstatt
(27,39,5). Je zwei senkrechte Wellen; reale Netzverbindung separat bestätigt.
Papierpilot zunächst unerreichbar, menschliche Bautreppe ermöglichte den Bau.
Folgeprojekte enthielten die Bautreppe zuerst; alle drei hohen Baustellen
buildersReachable=true. Alle Projekte completed. Vier reine Bautreppen danach
entfernt, Fabrikzugänge weiterhin unblocked/accessible. Vollständige pausierte
Inventur: 374 Objekte, null Laufräder, fünf fertige Windturbinen einschließlich
Mühlenturbine, keine offene Baustelle. Windleistung schwankt; keine Dauerleistung.
Nutzer ergänzte 24 Weizenmarkierungen (jetzt 34, Gesamtanbau 132) und entfernte
einen Beerensammler; verbleibender Sammler aktiv und besetzt. Abschluss an Tag
563, 12,96875 Uhr pausiert: alle vier Windnetze 94 Erzeugung, Bedarf Papier 80,
Sägewerke jeweils 50, Zahnrad 120 (Unterversorgung bei dieser Windstärke).
53 Biber, davon 49 Erwachsene; acht arbeitslos, keine unbesetzten Arbeitsplätze.
Abdeckung unverändert 66/157, 91 offen. Neue Befunde im Backlog; kein Code geändert.

## 2026-10-11: größere Etappe mit vier neuen Typen abgeschlossen

Kräuterkundler (43,25,6), Mühle (46,21,6), Windturbine (45,28,8) und
Bevölkerungszähler (46,19,6), Drehung 2, separat fertig nachgewiesen.
Achtteilprojekt samt fünf Wegen und folgendes Zählerprojekt completed.
Abdeckung 66/157 reguläre Typen, 91 offen. Tag 537, 14,125 Uhr pausiert.
Kräuterkundlerzugang Distanz 74, Mühlenzugang 83. Kräuterkundler benötigt
Dandelion; Mühle ohne Rezept/Kraftanschluss; Windturbine ohne Verbraucher.
Bauabdeckung ist kein Produktionsnachweis. Forschung nach 800 Punkten für
diese vier Freischaltungen weitgehend verbraucht; nächste größere Baugruppe
braucht Forschungsaufbau und teilweise neue Materialketten. Verbesserungen
zu Betriebsplanung und zentralen Kraftports dokumentiert, kein Code geändert.

## 2026-10-11: Papierlager, Zusatzkraft und Bienenstock fertig

Achtteilprojekt abgeschlossen: Papierlager (41,25,6), fünf Wegfelder, zweites
Laufrad (44,21,6), Drehung 3, und Bienenstock (27,26,3). Lager Paper/accept
separat rückgelesen, danach 30 Papier tatsächlich eingelagert. Beide Laufräder
im Fabriknetz, tagsüber 106 Erzeugung bei 80 Bedarf. Fertige Zugänge bestätigt:
Lager native Distanz 72, Zusatzlaufrad 78. Bienenstock fertig; Pflanzenwirkung
mit aktueller Schnittstelle nicht gesondert nachgewiesen.
Abdeckung 62/157 reguläre Typen, 95 offen. Tag 526, 12,46875 Uhr pausiert;
kein offenes Projekt aus diesem Ausbau. Verbesserungen nur im Backlog notiert.

## 2026-10-11: Papierproduktion und Vogelscheuche nachgewiesen

Papierfabrik (41,21,6), eigenes Laufrad (43,23,6), neun Wegfelder und anschließend
Vogelscheuche (43,20,6) fertig; beide Projekte completed. Abdeckung 61/157,
96 reguläre Typen offen. Fabrikzugang erreichbar, native Distriktdistanz 69.
Echte Kraftverbindung bestätigt: tagsüber 53 Erzeugung bei 80 Bedarf, Papier
tatsächlich produziert. Zuletzt 18 Papier im Fabrikpuffer; nachts ohne laufenden
Arbeitsauftrag 0/0 Leistung. Keine volle Kraftversorgung behauptet.
Tag 523, 18,46875 Uhr pausiert, keine offenen Projekte aus diesem Ausbau.
Nächste sinnvolle Schritte: Papierlager/Bienenstock und bessere Kraftversorgung.
Neue Befunde zu Sofortwegen und irreführendem rotationMatches im Backlog.

## 2026-10-11: 15 Tage Eichenwachstum vollständig geprüft

Vom Nutzer gewünschter Lauf exakt 360 Spielstunden, automatisch pausiert an
Tag 519 um 6,71875 Uhr. Eichenstichprobe: zwei lebende reife Bäume mit je acht
Stämmen, ein abgeernteter Rest und mehrere junge Ersatzpflanzen; andere Eichen
noch unreif. Keine flächendeckende Reife behauptet. Freie Baustoffe anschließend
144 Log, 33 Plank, 29 Gear. Oberes Sägewerk und Zahnradwerkstatt wieder aktiv;
unteres Sägewerk bleibt pausiert. Vier fehlende Fällmarkierungen ergänzt/rückgelesen.
Gebäudeabdeckung unverändert 59/157. Dusche freigeschaltet und drei ertraglose
Hindernisse bei (41..43,20,6) entfernt. Gemeinsamer Plan mit Treppe/Podest zeigte
native_invalid für Dusche: kein Projektstart, keine neue Baustelle. Platzierungsfall
offen; kein Test-Gate oder Neuladen erforderlich.

## Neue Sitzung 2026-10-11: drei weitere Typen und funktionierender Tank

VerticalPowerShaft, MediumTank und SuspensionBridge1x1 fertig; alle drei Projekte
completed. Abdeckung 59/157 reguläre Typen, 98 offen. Tank Water/accept, zuletzt
175 Wasser; Nachbarhaus weiterhin zugänglich. Zusätzlicher oberer Holzfäller
fertig/besetzt, fünf tote Vollbäume nachweislich geerntet. Zwei weitere zur Ernte
markiert, noch nicht geerntet nachgewiesen. Alte gestoppte Sitzung ist beendet.
Oberes Sägewerk wieder aktiv; unteres und Zahnradwerkstatt vorläufig pausiert.
Freie Baustoffe: 8 Stämme, 15 Bretter, 7 Zahnräder. 54 Biber/54 belegte Betten,
keine Obdachlosen. Tag 503, 14,71875 Uhr pausiert. Papierfabrik bleibt freigeschaltet,
aber ungebaut; nächster größerer Ausbau braucht Materialaufbau.
Regionalsuche lieferte erneut ungültigen gedrehten Tankplatz über Geländekante;
native Prüfung verhinderte den Auftrag. Befund im Backlog, Details im Spieltestjournal.

## Fortsetzung 2026-10-11: Pegel fertig, Projektstopp diagnostiziert

StreamGauge fertig und separat gelesen: 56/157 reguläre Typen, 101 offen.
Zweierprojekt vor Kraftwellenauftrag terminal stopped/guard_changed; Kraftwelle
nicht vorhanden. Keine offenen Baustellen oder neuen HUD-Alerts. Ursache nicht
eindeutig beobachtbar: Guard liefert keine Teilursache/betroffenen Objekte.
Weiterer Großprojektstart ist im aktuellen Ledger bis Spielstand-Neuladen gesperrt.
PaperMill und VerticalPowerShaft freigeschaltet, noch nicht gebaut. Beide Sägewerke
vorübergehend pausiert, volle Bretterpuffer; zuletzt 6 freie Stämme, 38 freie Bretter,
20 Zahnräder. 54 belegte Betten, keine freien Betten/Obdachlosen.
Tag 499, 10,84375 Uhr pausiert. Details im Spieltestjournal.

## Spieltest-Abschluss und Warnungsbereinigung (2026-10-11)

Korrektur: Terrassenabkürzung störte zwei Bestandszugänge trotz positiver Vorschau.
Treppe zurückgebaut, Eingangspfad wiederhergestellt. Haus/Tank wieder verbunden;
Terrasse bleibt erreichbar, wieder Distanz 72. Konfliktfreie Kürzung weiter offen.
Lager konfiguriert, Wasserpflanzen markiert, untätige Betriebe vorläufig pausiert.
Laufrad fertig und morgens 53 Leistung bei 50 Bedarf bestätigt; oberes Sägewerk
mit 13 Brettern im vollen Ausgangspuffer. Keine aktiven HUD-Alertgruppen mehr;
Grill, Bäckerei und Schrottfahne bleiben vorläufig pausiert. Eine kontaminierte
und eine trocknende Birke sind separate natürliche Statusmeldungen.
Spieler ergänzte 100 Eichen-Pflanzplätze: jetzt 154 Eichen- und 18 Kiefernplätze;
neue Markierungen sind noch kein vollständiger Nachweis tatsächlich gepflanzter Bäume.
53 Biber, 54 Betten, keine Obdachlosen; Tag 497 um 9,3125 Uhr pausiert.
55/157 reguläre Gebäudetypen nachgewiesen. Details im Spieltestjournal und Backlog.

## Spieltest 2026-10-11: Holzanschluss, 54 Eichen, vier weitere Typen

Westlicher Brückenkopf landseitig angebunden, Holzfäller besetzt und Erntewirkung
beobachtet. 54 neue Eichen tatsächlich gepflanzt, lebend und wachsend; noch unreif.
TripleLodge, TriplePlatform, FlowSensor und Levee fertig. Obere Siebenerfolge
completed, Wohnhauseingang frei bei Distriktdistanz 32. Abdeckung 55/157 reguläre
Typen, 102 offen. Sägewerk nach temporärer Pause wieder aktiv; Materialaufbau läuft.
Details und Grenzen: docs/settlement-relocation-playtest.md.

## Spieltest 2026-10-11: Terrasse verkürzt, 13 weitere Typen

Terrassen-Distriktdistanz durch zwei direkte Treppenflüge von 72 auf 24 reduziert;
Zugang nach Navigationsaktualisierung bestätigt. 13 neue Dach-/Dekorations- und
Automationsvorlagen fertig, Abdeckung 51/157 reguläre Typen; 106 offen.
Rundholzengpass wird über westlichen Brückenkopf bearbeitet, altes Sägewerk
vorläufig pausiert. Holzernte und weitere Baugruppe noch offen.
Details: docs/settlement-relocation-playtest.md. Frühere Stände folgen historisch.

## Spieltest 2026-10-11: dreigeschossiger Wohnabschnitt und Dachterrasse

Doppelwohnhaus über bestehender Lodge sowie drei Doppelplattformen und
Dachterrasse fertiggestellt; Eingänge und jeweilige Projektabschlüsse bestätigt.
38/157 reguläre Gebäudetypen historisch fertig nachgewiesen, 119 offen; fünf
Entwicklervorlagen separat ausgeschlossen. Drei neue Typen gebaut, zwei ältere
Nachweise in CSV nachgetragen. Tag 478, 5,21875 Uhr pausiert. Gesamtziel offen.
Details: docs/settlement-relocation-playtest.md; neue MCP-Befunde im BACKLOG.

## Live 2026-10-11: Wiederanschluss und wirkungsloser Projektstopp bestanden

Nach erneuter Bereitmeldung Backendstart erfolgreich, Bridge 0.37.0 und neue
Session bestätigt. Expliziter Stopp vor Bauversuch erzeugt kein Objekt und
blockiert neuen Auftrag in derselben Sitzung nicht mehr. Neuer Weg fertig und
zugänglich, Ledger completed; alte ID bleibt stopped. Fünfteiliger Aufbau einer
zusätzlichen Wohnetage vollständig fertig: Zuweg, Treppe, Plattform, oberer Weg,
Lodge auf Z5 über bestehendem Haus auf Z4. Plattform-/Hausbaustelle erreichbar,
fertiger Hauseingang frei, Distriktdistanz 13. Ledger completed; keine allgemeine
Acht-Ebenen-Abnahme. Tag 473, 19,59375 Uhr pausiert.
Supervisor-Liveprobe ebenfalls bestanden: Stop bestätigt Backend aus/Gate an,
Start über dieselbe Verbindung bestätigt Backend an/Gate aus, Bridge und gleiche
Spielsitzung erneut erreichbar. Kein Codex- oder Spielneustart für diese Probe.
Frühere Fehler-/Gate-Beschreibungen unten sind historische Zwischenschritte.

## Gate-Korrektur vorbereitet: Windows-Statusdateien (2026-10-11)

Zusätzlicher aktueller Befund: Alte Registrierungen verweisen auf wiederverwendete
geschützte Windows-Dienst-PIDs. Bei verweigertem Handlezugriff prüft der Starter
jetzt die CIM-Prozessstartzeit; nur nachgewiesene Wiederverwendung wird ignoriert.
Identitätsfehler bleiben sperrend. Gezielter Lifecycle-Stopp erreicht damit die
Bestätigungsprüfung. Verbliebenen alten Vorschaltprozess mit eingefrorenem
Heartbeat nach ausdrücklicher Nutzerfreigabe und erneuter Identitäts-/Kinderprüfung
gezielt beendet. Danach Lifecycle-Stopp erfolgreich: Starts gesperrt, kein
Projekt-Backend vorhanden. Menschliches Build-/Test-Gate weiter ausstehend;
Client-Verbindung muss anschließend wieder aufgebaut werden.

Erster Fehler: Statusdatei-Ersetzung verweigert, danach zweiter Schreibfehler im
Fehlerpfad. Nachfolgender Gate-Test bewies, dass FILE_SHARE_DELETE allein den
Austausch bei offenem Leser nicht ermöglicht. Diese Korrektur wurde ersetzt:
Benannter Windows-Mutex je normalisiertem Dateipfad serialisiert Lesen,
Ersetzen und Löschen über Prozesse hinweg, mit fünf Sekunden Sperrzeitlimit.
Neuer Zweiprozess-Test hält den Leser offen, belegt wartenden Schreiber und
prüft erfolgreichen Austausch nach Schließen des Lesers. Fehlerpfad stoppt Backend ohne erneutes
Statusschreiben; keine erfundene Stoppbestätigung und keine Build-Retries.
Regressionen für konkurrierenden Leser und Schreibfehler ergänzt;
Quell-/Diffprüfung erfolgt, Ausführung im menschlichen Gate ausstehend.
Bereits laufende Python-Vorschaltprozesse laden Änderungen erst bei Neustart.

## Vorbereitet, noch nicht abgenommen: wirkungsloser Höhenprojekt-Stopp

Explizit gestoppte Großprojekte ohne jeden Platzierungsversuch sollen neue
Projekte nicht mehr sperren. Alte IDs bleiben terminal; Teilbauten, automatische
Fehler und unbestätigte Wirkungen bleiben gesperrt. Code und Regressionstests
vorbereitet; Build/Tests und gezielter Stop→Neustart-Livepilot am menschlichen
Gate ausstehend. Details: docs/large-projects-0.37.0.md.

## 0.37.0 größere Bauvorhaben — Stützenpilot bestanden (2026-10-10)

Aktuell: Nach menschlichem Gate fünfteilige Höhenfolge vollständig gebaut und
als `completed` bestätigt. Trägerabhängigkeit negativ geprüft; oberer Weg und
Lager erst nach realer Trägerfertigstellung nativ validiert. Alle vier Baustellen
erreichbar, fertiger Lagereingang frei, Distriktdistanz 33. Bestandsblockade mit
sieben verlorenen Verbindungen korrekt abgewiesen und restauriert. Separaten
Plan vor Auftrag gestoppt, kein Objekt entstanden; dadurch weitere Großprojekte
in dieser Sitzung erwartungsgemäß blockiert. Tag 435, 19,40625 Uhr pausiert;
Sägewerk pausiert. Log geprüft, sieben priorisierte Verbesserungen im BACKLOG
festgehalten. Keine Rohlogs/Spiel-IDs gespeichert. Fachlicher Abschluss für 0.37.0;
keine pauschale Katalog-/Kraftdrehungsabnahme. Folgende Absätze sind Historie.

Aktuell: Kraftvorschau-Korrektur nach erneutem menschlichem Gate live bestanden.
Zweiteilige Folge Welle (26,6,3) → Lager (32,8,4) vollständig fertiggestellt;
Start erzeugt keine Baustelle, Advance wartet auf Fertigstellung, Bauarbeiterzugang
und fertiger Lagereingang separat bestätigt. Reales Dreiknotennetz mit 53 Angebot.
Noch offene Lücke: Vorschauplattformen zählen nicht als reale Träger für die native
Geometrieprüfung von oberem Weg/Lager. Korrektur `deferred_native_validation`
mit expliziter Trägerabhängigkeit, Kollisionskontrolle und zwingender frischer
Ausführungsprüfung vorbereitet; passende Policy-/Ausführungstests ergänzt.
Erneutes Gate nötig. Stützenfläche (33,8..10,4) frei, Plattform regulär freigeschaltet,
Sägewerk aktiviert. Tag 430, 15 Uhr pausiert, null offene Baustellen.
Keine Gesamt-Abnahme, kein Commit/Push. Folgende Absätze sind frühere Zwischenschritte.

Erster Livepilot nach Katalogübernahme: zwei identische Lagerpläne gültig mit
identischem Schlüssel, schwebendes Lager ohne Stütze korrekt ungültig; jeweils
Vorschau wiederhergestellt. Kraftleser bestätigt Sägewerk/Rad als verbundenes
Zweiknotennetz mit momentan null Leistung. Kraftwellen-Vorschau liefert jedoch
Backendfehler. Pilot gestoppt, null offene Baustellen rückgelesen; kein Bauauftrag.
Korrektur vorbereitet: Vorschauports als getrennte öffentliche Transput-Deskriptoren
aus TransputProviderSpec statt Runtime-Ports eines Vorschau-Knotens. Erneutes
menschliches Gate und Kraftvorschau-Livetest nötig, keine Abnahme/kein Push.

Explizite 3D-Pläne mit bis zu 32 Bauteilen, 32×32 Grundfläche und acht belegten
Höhenebenen implementiert. Abhängigkeiten, gemeinsame Vorschau, sequenzielle
Fertigstellungsprüfung und Kraftgeometrie/-netzleser. Neue Werkzeuge und
Regressionstests vorbereitet, Doku/Agentenanleitung ergänzt.
Nach menschlichem Gate/Bereitmeldung: Backendstart und Spielkontakt mit Bridge
0.37.0 bestätigt. Supervisor-Katalog enthält alle sechs neuen Werkzeuge; laufende
Codex-Sitzung bietet sie noch nicht an. Feature-Livetest wartet auf Client-Neustart,
kein weiterer Build erforderlich. Letzter vollständig live abgenommener Stand
bleibt 0.36.0. Kein Commit/Push vor fachlicher Abnahme.
[Umfang, Grenzen und Livepilot](docs/large-projects-0.37.0.md).

## Vorschaltprozess und Regionalsuche live abgenommen (2026-10-10)

Nach Katalogübernahme sind alle Spielwerkzeuge verfügbar. timberborn_status vor
und nach stop/status/start bestätigt dieselbe erreichbare Bridge 0.36.0 und Sitzung;
keine Client-Neustarts innerhalb dieses Zyklus. Menschlicher Vollgate-Lauf und acht
warnungsfreie Python-Tests liegen vor. Erstbefüllung/Toolkatalogänderungen können
weiterhin einen Client-Neustart benötigen; regulärer Backendwechsel wurde belegt.

Sägewerksfeld (31,4), 8×8, Start z=3: z=4 automatisch untersucht, auf beiden Ebenen
null unbekannte Zellen und null Legacy-Verfeinerungsreads. Rampe lokal als o;
vier Lagerkandidaten oben. 33 native Reads, vier Planungen. Lager (32,8,4)
in Spielvorschau gültig und angeschlossen, kein neuer Weg nötig.
Hauptsiedlungsfeld (30,24), 16×16, z=4: zwei getrennte Fenster, acht Planungen,
28 native Reads. Suchabdeckung unterscheidet p/c/u; elf weitere beobachtete Höhen
wegen maxHeights=1 ausdrücklich ungeprüft. Lager (35,28,4) ebenfalls gültig und
angeschlossen. Negative Lodge (33,27,4) trotz Vorschau-Anschluss geometrisch ungültig.
Alle drei Vorschauen melden Zustand wiederhergestellt, keine Sitzungssperre.
Keine Bauaufträge/Räumungen; Spiel bleibt pausiert. Bauarbeiter-/fertiger Gebäudezugang
weiterhin nicht nachgewiesen, keine neue reguläre Ausführungsfreigabe.

## Vorschaltprozess: Stop/Start live bestanden, Katalogübernahme offen (2026-10-10)

Menschlicher gezielter Test: alle acht Python-Tests ohne Warnungen bestanden.
Danach start → stop → status → start über dieselbe Codex-Verbindung erfolgreich:
Backend aktiv/ungesperrt, gestoppt/gesperrt, weiterhin gestoppt, wieder aktiv.
Kein Timeout und kein Transportverlust. Lokaler Katalog enthält 76 Backendwerkzeuge,
darunter timberborn_status und survey_region. Laufende Agentensitzung exponiert
trotz Änderungsnachricht weiterhin nur das Lifecycle-Werkzeug. Erstbefüllter Cache
braucht deshalb noch einen Client-Neustart zur tatsächlichen Werkzeugübernahme.
Backend zuletzt aktiv, Gatesperre aufgehoben. Kein neuer Build/Testlauf nötig.
Spielkontakt, Regionalsuche-Abnahme und Commit/Push bleiben offen.

## Vorschaltprozess: erster Livepilot, zwei Korrekturen offen (2026-10-10)

start meldet Backend aktiv; Verbindung bleibt auch nach stop/status/start nutzbar.
Stop meldete jedoch Timeout trotz beendetem Backend: alte Registry-PIDs waren
inzwischen RuntimeBroker/conhost/Codex-Prozessen zugeteilt. Prozessstartzeit wird
jetzt mit Registry-Heartbeat verglichen. Keine fremden Prozesse beendet.
Spielwerkzeuge wurden in dieser Agentensitzung nach tools/list_changed nicht
sichtbar; daher kein timberborn_status und keine Regionalsuche-Abnahme möglich.
Werkzeugkatalog wird jetzt nach erfolgreichem Backend-Handshake lokal gecacht und
beim nächsten Frontendstart auch während Gatesperre angeboten. Erstbefüllung braucht
einen Backendstart der neuen Fassung; tatsächliche Clientaktualisierung bleibt zu prüfen.
Zwei Regressionstests ergänzt (PID-Wiederverwendung, Katalog bei gesperrtem Start).
Menschlicher gezielter Python-Test und erneuter Livepilot offen; kein Commit/Push.

## Persistenter Codex-Vorschaltprozess vorbereitet — Gate offen (2026-10-10)

Menschlicher Gate-Lauf vollständig bestanden: sechs Python-Tests, Release-Build,
Unit-/Integrationstests, Bridge-Paketierung und Installation mit fünf geprüften
Dateien (138,9 Sekunden). Noch keine Liveabnahme.
ResourceWarning für offene stdout-Pipes erkannt: Reader schließt seinen Kanal nun
im finally; Stop wartet außerhalb des Protokolllocks auf Readerende. Stop-/Crashtests
prüfen geschlossene Kanäle zusätzlich. Diese nach dem Python-Testschritt vorgenommene
Korrektur braucht nur den gezielten menschlichen Python-Test, keinen erneuten Mod-Build;
danach Codex einmal neu starten, damit der Vorschaltprozess den neuen Code lädt.

Python-stdio-Frontend mit eigenem stop/start/status-Werkzeug, Backend-Handshake
nach Neustart und projektweiter Gatesperre implementiert. Vorbereitungsskript
stoppt über die Sperre und verwendet nur noch den exakten Releasepfad als Rückfall.
Synthetische Tests in verify.ps1 eingebunden, vom Agenten nicht ausgeführt.
Bestehender Codex-Servereintrag auf Vorschaltprozess umgestellt und zurückgeprüft.
[Einrichtung, Grenzen und Liveabnahme](docs/codex-mcp-supervisor.md).
Livebestätigung und Prüfung der Kanal-Korrektur offen; kein Commit/Push. Danach Rampenblindfleck
fachlich abnehmen; dessen offener Status bleibt bestehen.

## Rampenblindfleck: Liveabnahme an Clientverbindung blockiert (2026-10-10)

Nach menschlichem Gate erneut live bereit gemeldet. Erster timberborn_status
scheitert mit Transport closed; kein Bridgekontakt, keine neue Session und kein
fachlicher Livebeleg. Client-Reconnect-Werkzeug in dieser Sitzung nicht verfügbar.
Codex-MCP-Verbindung muss clientseitig neu aufgebaut werden; kein isolierter
stdio-Prozess als Ersatz. Keine Spieländerung, kein Commit/Push. Danach geplante
Regionalsuche-Abnahme fortsetzen. Die Bereitmeldung ersetzt kein Liveergebnis.

## Rampenblindfleck und Höhensuche vorbereitet — Gate offen (2026-10-10)

Native Überlappungszellen statt pauschaler unknown-Kachel; automatischer Scan
weiterer beobachteter Höhen, abdeckungsorientierte Fenster und explizite ungeprüfte
Bereiche implementiert. Grenzen einstellbar, Legacy-Fallback begrenzt. Tests ergänzt,
noch nicht ausgeführt; kein Build/Commit/Push. Ein Modpaket über menschliches Gate.
[Umfang und konkrete Liveabnahme](docs/regional-survey.md).


## Bauflächen oberhalb der Siedlung und beim Sägewerk geprüft (2026-10-10)

Obere Wege auf z=4 in beiden Gebieten nativ distriktverbunden. Kleines Lager
hinter Hauptsiedlung sowie kleines Lager/Wohnhaus beim Sägewerk bestehen
Spielvorschau mit Anschluss ohne neue Wege. Größerer Kandidat hinter Siedlung
abgelehnt; höherer Hang z=8 weiterhin teilweise unbekannt. Keine Bauten/Räumungen.
MCP-Suchlücke belegt: Slope/other setzt ganze 8×8-Kachel auf unknown und verhindert
survey-Planung trotz direkt gültiger Plätze. Maßnahmen im Backlog, noch kein Code.
[Prüfgrenzen und Verfahren gegen blinde Flecken](docs/regional-survey.md).
Endpause Tag 424, 21,375 Uhr; zwischenzeitlichen fremden Tempo-Wechsel beobachtet,
Urheber unbekannt. Kein neuer Nachweis von Gebäudetypen.


## Bauchargen-Frühabschluss live bestanden (2026-10-10)

Menschliches Entwickler-Gate durch anschließende Bereitmeldung bestätigt;
Bridge 0.36.0 erreichbar. Zwei kleine Lager in einer Charge ohne neue Wege:
Frühpause nach 4 und 3 statt jeweils 24 Spielstunden. Derselbe advance-Aufruf
schloss beide Bauten sequenziell ab; frische Einzelabfragen bestätigten
finished_accessible, freien Eingang, erhaltene Karotten-/accept-Konfiguration
und Pause. Laufkennungen und ursprüngliche Zeitobergrenzen blieben unverändert.
Identischer start wiederholte nur den terminalen Checkpoint, ohne Doppelbauten.

Kontrollfälle: unfertige erste Bank löste keinen Folgeauftrag aus; nach ihrer
Fertigstellung bei 6,90625 Stunden stoppte die nächste Materialprüfung im selben
Aufruf mit materials_missing (nur ein statt zwei benötigten Brettern vorhanden).
Keine zweite Bank beauftragt. Journalwechsel im Livepilot ohne Zugriffsfehler.
Die ursprüngliche externe Ursache des File.Move-Fehlers bleibt unbekannt.

Zwei Holzfällerplätze dienten als austauschbare Testfläche; Testbank wieder
entfernt, zwei Lager bleiben. Endstand Tag 424, 9,875 Uhr, Tempo 0. Keine neuen
Vorlagentypen: kumulative Abdeckung weiterhin 33/157. Keine künstlich erzeugte
verlorene Pausenbestätigung im Livetest; dafür vorbereitete Regressionen im Gate.
Kein Hintergrunddispatcher, Räumzeitfenster weiterhin unverändert.

[Details](docs/building-batch-workflow.md).

## Gebäudespieltest 0.36.0 (2026-10-10)

Aktueller Nutzerauftrag: reguläre Gebäudetypen einmal bauen, Verbesserungen
begleitend dokumentieren. Bauhütte neu finished_accessible; vorhandene fertige
Treppe bei vollständigem Bestandsabgleich nachgetragen. CSV jetzt 31/157,
126 offen; fünf Entwicklerwerkzeuge ausgeschlossen. Katalog weiterhin mit
15 nicht unterstützten regulären Vorlagen. Kein Neubau von Terrasse/Wasserfarm.
Tag 417, 17,78125 Uhr, pausiert, keine Baustellen. 49 Biber, 49 Betten,
23 freie Holzstämme. Sägewerk und Sauna für Holzansammlung pausiert.
Ein von drei geplanten Typen fertig; Charge wegen fehlendem weiterem Standort
gestoppt. Aufwandsalarm und neue Entscheidung vor Skalierung; kein MCP-Code geändert.
[Nachweise und Verbesserungsvorschläge](docs/playtest-0.36.0.md).

## 0.36.0 Screenshot abgeschlossen (2026-10-10)

Entwickler-Gate mit anschließender Bereitmeldung; native Bridge 0.36.0 bestätigt.
Zwei sichtbare Spielaufnahmen inklusive UI und Kamerametadaten live bestanden:
1280×720 / 122828 Bytes, 640×360 / 41678 Bytes. Position/Höhe, Blickrichtung,
Zielabstand, Bildwinkel und Bildweite konsistent; falsche Session ohne Bild
abgewiesen. Keine Bilddateien gespeichert. [Abnahme und Grenzen](docs/screenshot-0.36.0.md).
Einziger Release-Stand über Entwickler-Gate; keine weiteren Agent-Builds.
Vorbereitungsstände unten historisch; vorhandene Hermes-/Starteränderungen separat.

## 0.36.0 Screenshot vorbereitet — Entwickler-Gate offen (2026-10-10)

Erster Entwickler-Gatelauf: MCP-/Test-Build bestanden, Unit-Tests an veralteter
Negativerwartung für 0.36.0 gestoppt. Zwei entsprechende Teststellen gefunden
(BuildingCapabilityTests und GenericBuildingProjectTests): unbekannte Version
auf 99.0.0 geändert, positive 0.36.0-Vertrags-/Profilfälle ergänzt. Nur Quell-/
Diff-Prüfung; erneuter Entwickler-Gatelauf erforderlich, keine Agent-Testausführung.

Expliziter Unity-Spielbildabruf mit begrenztem JPEG und MCP-Bildblock implementiert;
Kameraposition/Höhe, Richtung, Ziel, Abstand, Bildwinkel und Bildweite auf Zielebene
in Metadaten. [Umfang und Prüfplan](docs/screenshot-0.36.0.md). Noch kein Livebeleg.
Finaler Stand nur quellgeprüft, vorbereitete Tests noch nicht ausgeführt.
Auf Nutzerkorrektur gilt wieder ausschließlich das Entwickler-Test-Gate:
keine Agent-Builds/Testläufe, keine Debug-/Parallelfassung. Frühere Vorprüfregel
und Zwischenstand-Builds unten sind historisch. Installation, Livetest, Commit
und Push für 0.36.0 stehen aus.

## 0.35.5 abgeschlossen (2026-10-10)

Holzdiagnose mit echter Ertragsprüfung, Sammelmarkierung/Replay, früher Holzstopp,
kompakte Chargenantworten und terminale Bauablehnung live bestanden. Gültiger
Einzelbau zusätzlich unabhängig als finished_accessible und pausiert bestätigt.
Kompakter Ablehnungsbericht 439 statt 973 Zeichen. Gezielte Vorprüfung: drei
Projekt-Builds ohne Warnungen/Fehler, 77 Unit-Tests und sechs Integrationsvarianten
bestanden; menschliches Gate durch anschließende Bereitmeldung bestätigt.
[Abschlussnachweis und Grenzen](docs/forestry-efficiency-0.35.5.md).
Screenshot-Wunsch, native Forstfilter und präzisere Eingabefehler bleiben im Backlog.
Frühere offene Abnahmestände unten sind historisch. Hermes-/Starteränderungen
bleiben separat im Arbeitsbaum.

## Baumstumpf-Korrektur und Holzstopp live bestanden (2026-10-10)

Nach erneutem menschlichem Gate native Bridge 0.35.5 erreichbar. Vollständige
Kontrollfläche: 45 Objekte, davon 34 abgeerntete Reste, acht junge Bäume ohne
Fällertrag und drei fällbare Kiefern mit je zwei Holz. Forestry und alle
Detailseiten stimmen überein. Stumpfauftrag abgewiesen; zwei echte Bäume markiert,
Replay identisch, beide anschließend als already_marked ausgeschlossen.

Begrenzter Lauf mit Holzschwelle drei: stock_target_reached nach 1,9375
Spielstunden und 5,51 realen Sekunden, vier freie Stämme statt zuvor zwei.
Beide Zielbäume anschließend als abgeerntet bestätigt. Pause separat gelesen:
Tag 411, 5,78125 Uhr, Tempo null. Kein Vollbeleg für 0.35.5: Chargenantwort und
terminale Bauablehnung noch live offen; deshalb noch kein Versionsabschluss,
Commit oder Push. [Nachweis](docs/forestry-efficiency-0.35.5.md).

## Vorprüfung vor Testübergabe korrigiert (2026-10-10)

CS8602 in ForestrySurvey behoben: nach der Nullprüfung wird durchgehend die
geprüfte lokale Vegetationsvariable verwendet. MCP-/Unit-Testprojekt,
Integrationstestprojekt und Mod gegen lokale Spielreferenzen erfolgreich
kompiliert, jeweils ohne Warnungen/Fehler. 77 gezielte Forestry-, SimulationRun-
und BuildBatch-Tests sowie alle sechs Varianten des Integrationstests
AuthenticatedBridgeThroughRealStdioHonorsIndependentActionGates bestanden.
Sandbox-Teststarts scheiterten vor der Ausführung am Testhost-Verbindungsaufbau;
die genannten erfolgreichen Testläufe erfolgten außerhalb der Sandbox.

Auf Nutzerauftrag AGENTS.md, DEVELOPMENT_WORKFLOW.md und globalen
general-mod-workflow aktualisiert: betroffene Projekte und Tests vor der Übergabe
wirklich ausführen, Compilerprüfung nicht durch Syntaxprüfung ersetzen.
Skill validiert; Diff-Prüfung sauber. Keine Installation, kein Commit/Push.
Erneutes menschliches Gate und Livevergleich Baumstumpf/erntefähiger Baum bleiben
für die korrigierten Ertragsdaten erforderlich; 0.35.5 ist noch nicht abgenommen.

## 0.35.5-Livepilot gestoppt: Baumreste nicht unterscheiden (2026-10-10)

Menschliches Gate und native Verbindung bestätigt; fünf installierte Paketdateien
mit Paket 0.35.5-20261010-180251-9ec61831 identisch. Sammelmarkierung zweier IDs,
identisches Replay, Parameterkonflikt und bereits erreichtes Holzziel geprüft.
Benutzerhinweis während des Pilots: angebotene Objekte können Baumstümpfe sein.
Lebens-/Wachstumsflags allein sind kein Nachweis eines vorhandenen Fällertrags.
Die bisherige Kandidatenprüfung ist deshalb nicht abgenommen. Lauf abgebrochen,
Pause bestätigt bei Tag 411, 3,875 Uhr; Holzziel fünf nicht erreicht.

Korrektur vorbereitet: öffentliche Cuttable.Yielder-Felder IsYieldRemoved,
IsYielding, Yield.GoodId und Yield.Amount in Vegetationsdaten aufnehmen. Forestry
schließt abgeerntete Reste, fehlende Ertragsdaten und fehlendes Holz aus.
Gezielte Tests für lebende/ausgewachsene Stümpfe, Nullbestand und unbekannte Daten
ergänzt; nur Syntax-/Diff-Prüfung. Erneutes menschliches Gate und stumpf-/baum-
spezifischer Livetest erforderlich. Versionsabschluss, Commit und Push bleiben offen.


## 0.35.5 vorbereitet – menschliches Gate offen (2026-10-10)

Auftrag: Versionsabschluss einschließlich Holz-/Effizienzfunktionen. Implementiert:
kompakte regionale Holzdiagnose mit optionalen Verbraucherinventaren, gebündelte
Fällmarkierungen mit Replay-Beleg, früher Simulationsstopp bei freiem Holzbestand,
terminale Bauablehnung und kompakte Chargenantworten mit optionalen Details.
Manifest/Bridge/Assembly und Client-Versionserkennung auf 0.35.5 vorbereitet.
[Umfang, Grenzen und Abnahme](docs/forestry-efficiency-0.35.5.md).

Menschlicher Gate-Lauf: MCP-/Test-Build und Unit-Tests bestanden. Integrationstest
stoppte an veralteter Werkzeugliste (inspect_forestry fehlte); Erwartungen für
lesende Holzdiagnose und separat freigeschaltete Markierung korrigiert. Erneuter
Gate-Lauf bestätigte Build/Unit-Tests, stoppte aber an der ebenfalls veralteten
ReadOnly-Erwartung für mark_forestry. Schreibwerkzeugliste korrigiert und explizite
Annotationstests ergänzt; entsprechende Erwartungen in den Integrationstests
zusammen geprüft. Erfolgreicher Gesamtlauf, Mod-Build, Installation und gezielter
Livetest stehen aus. Keine neue Spielbeobachtung;
letzter historischer Spielstand weiter unten. Kein Commit/Push dieser Implementierung
vor erfolgreichem Gate. Screenshot-Funktion als optionaler lesender Spielbildabruf
im Backlog vermerkt, noch nicht implementiert. Vorhandene lokale Integrationsänderungen
erhalten; sie sind kein Bestandteil des behaupteten Funktionsnachweises.


## Tagesabschluss und Übergabe (2026-10-05)

Letzter Live-Stand unverändert: Tag 409, 18,09375 Uhr, pausiert; 47 Biber,
29/157 reguläre Gebäudetypen. Holzausbau für die Fortsetzung vorbereitet,
aber auf Wunsch in diesem Nachtrag keine Spielaktion oder Codeänderung ausgeführt.
Konkrete Reihenfolge und Erfolgskriterien im
[Tagesabschluss](docs/playtest-2026-10-04.md#tagesabschluss-2026-10-05-und-geplanter-holzausbau).
Priorisierte MCP-Effizienzideen im [Backlog](BACKLOG.md): regionale Holzdiagnose,
Bestands-/Verbrauchserklärung, Sammelmarkierung, bedingtes Zeitfenster und kompakte
Antworten. Noch nicht implementiert oder abgenommen. Beim Wiedereinstieg zuerst
Live-Zustand frisch lesen; bestehende lokale Integrationsänderungen separat behandeln.


## Kraftanschluss und Sauna fertig (2026-10-05)

Weitere sieben Spieltage bis Tag 409, 18,09375 Uhr. Aktuell 47 Biber
(43 Erwachsene, vier Kinder), 49 Betten, keine Obdachlosen, keine kritischen
Bedürfnisflags und keine Stranded-Warnung. Alle Zeitfenster beendet, Pause bestätigt.

Neues Kraftlaufrad direkt neben der Zahnradwerkstatt fertig; vier Anschlusswege
fertig und Zugang frei. Kraftwarnung verschwunden und Zahnradproduktion tatsächlich
nachgewiesen: fünf Zahnräder in der neuen Sauna verbaut, vier weitere im
Werkstattausgang. Sauna finished_accessible, unpausiert, neun Holzstämme im
Brennstoffinventar, keine Betriebswarnung. Besucherwirkung nicht separat nachgewiesen.
CSV jetzt 29/157 reguläre Vorlagen fertig, 128 offen, fünf Entwickler-Vorlagen
ausgeschlossen. Das zusätzliche Rad erhöht die Typenabdeckung nicht.

Radbau zunächst nach 24 Stunden wegen fehlender Holzlieferung unfertig. Reife
Kiefern in tatsächlicher Holzfällerreichweite auf der unteren Ebene gefunden und
16 zusätzliche Fällzellen markiert; nach zwei weiteren Spieltagen Rad fertig und
Produktion angelaufen. Kein Zugangsfehler als Ursache beobachtet. Weitere elf
Karottenzellen für Rad, Wege und Sauna umgewidmet; rechnerisch 19 der ursprünglich
38 zusätzlichen Anbauzellen verbleiben. Endbestand 81 Karotten, 41 Beeren,
61 Wasser, zwei frei verfügbare Holzstämme, 31 Bretter und vier Zahnräder.

Nächster Schritt: Holzlieferung vor weiteren materialintensiven Gebäuden stärken,
dann zusätzliche Gebäudetypen. Kraftnetz-/Anschlussdiagnose im Backlog ergänzt.
Keine MCP-Codeänderung; bestehende lokale Änderungen außerhalb dieser Doku erhalten.


## Gebäudefokus: Farm, Sammelstelle, Zahnradwerkstatt (2026-10-05)

Vorläufiges Ziel 50 Biber; Gebäudeausbau hat Vorrang. Weitere acht Spieltage bis
Tag 402, 18,09375 Uhr. Aktuell 46 Biber (45 Erwachsene, ein Kind), 49 Betten,
keine Obdachlosen, keine kritischen Bedürfnisflags und keine Stranded-Warnung.
Die früher erreichten 50 sind ein historischer Zwischenstand, kein aktueller Wert.

Zweite Farm fertig/erreichbar mit einem neuen Anschlussweg; drei zugewiesene
Arbeiter. 38 neue feuchte Feldzellen in nativer Farmreichweite markiert und alle
als lebende Karotten ohne Wasserstress nachgewiesen. Später acht Zellen gezielt
für Werkstatt/Wege umgewidmet; 30 zusätzliche Anbauzellen verbleiben. Erste
Ernte angelaufen: Endbestand 48 Karotten und drei Beeren, 66 Wasser. Noch kein
langfristiger Versorgungsüberschuss nachgewiesen.

Neu für die Katalogabdeckung: ScavengerFlag.Folktails und GearWorkshop.Folktails,
beide finished_accessible samt bestätigter Endpause. CSV jetzt 28/157 reguläre
Vorlagen, 129 offen, fünf Entwickler-Vorlagen ausgeschlossen. Die Sammelstelle
hat nichts in Reichweite; Bau ist kein Altmetall-Ertragsnachweis. Zahnradwerkstatt
hat einen Arbeiter, Rezept Gear und zehn Bretter im Eingang, aber keinen Kraft-
anschluss: noch keine Zahnradproduktion. Drei neue Wege halten den südlichen
Zugang offen; keine neue Stranded-Warnung nach Fertigbau.

Nächster sinnvoller Schritt: Kraftanschluss der Werkstatt und danach weitere
Produktions-/Gebäudetypen. Freie Baustoffe zuletzt 21 Holz und acht Bretter.
Alle Zeitfenster abgeschlossen, Spiel pausiert. Verbesserungen nebenbei im
Backlog: Suchfenster samt Route erklären, wählbare Kandidaten/freie Korridore,
gebündelte Pflanz-/Umwidmungsaufträge und arbeitszeitbewusste Räumfenster.
Keine MCP-Codeänderung in diesem Spielabschnitt.


## Aktueller Spielauftrag (2026-10-05)

Vorläufiges Bevölkerungsziel auf 50 Biber geändert und bereits erreicht.
Schwerpunkt jetzt Gebäudetypen-Ausbau: jede reguläre Vorlage mindestens einmal
fertig nachweisen. Keine weitere gezielte Bevölkerungsexpansion; Nahrung und
Baustoffversorgung für den Ausbau stabilisieren. Historische Ziele unten sind
überholt. Verbesserungsvorschläge weiter nebenbei dokumentieren.


## Spielabschnitt abgeschlossen (2026-10-05): 50 Biber, 26 Gebäudetypen

Auf ausdrücklichen Spielauftrag etwa zehn Spieltage weitergespielt, Tag 384 bis
394 (18,09375 Uhr). Bevölkerung 38→50 (41 Erwachsene, neun Kinder).
Sechs Gebäude fertig/erreichbar nachgewiesen: drei Wohnhäuser, ein Miniwohnhaus,
ein Krankenbett und eine Hängematte. Ein südliches Wohnhaus anschließend zur
Öffnung eines blockierten Geländedurchgangs entfernt; fünf Neubauten verbleiben.
Krankenbett/Hängematte sind neue Typen: CSV 26 built_finished, 131 offen,
fünf Entwickler-Vorlagen ausgeschlossen; Nenner weiterhin 157 reguläre Vorlagen.

Drei Biber waren nach dem südlichen Wohnbau gestrandet, trotz nachgewiesener
Hauszugänge. Nach gezieltem Abriss und acht Spielstunden keine Stranded- oder
Exhausted-Warnung mehr, keine kritischen Bedürfnisflags. Endstand 49 Betten,
49 belegt, ein Obdachloser; weiterer Wohnraum bleibt nötig. Ein gezielter
Miniwohnhaus-Versuch auf breiterer Freifläche lieferte keinen Kandidaten und
keinen Auftrag. Kein weiteres Skalieren enger Wohnstandorte in diesem Abschnitt.

Wohncharge eins: ein Haus fertig; zweiter Kandidat nativ buildingValid=false,
kein Objekt/keine offenen Baustellen, Charge nach Diagnose beendet. Gemischte
Charge: Krankenbett plus Miniwohnhaus fertig, dann begrenzt kein Standort in
beiden Flächen. Hängematte auf zweiter Fläche fertig. Südcharge: 25 Vegetationsziele
in begrenztem Fenster geräumt, zwei Häuser fertig, eines wie oben entfernt.
Fünf Vorlagen regulär erforscht: Hecke, Laterne, Krankenbett, Doppelwohnhaus,
Hängematte. Forschung ist kein Gebäudenachweis; eingangslose/erhöhte Zugänge
passen nicht automatisch zum ebenen Projektablauf.

Abschließende Vorräte: 71 Wasser, 53 Beeren, null Karotten, 30 frei verfügbare
Holzstämme und 33 Bretter. Nahrungsproduktion vor weiterem Wachstum priorisieren.
Alle Chargen terminal, kein laufender Zeitauftrag, aktuelle Pause bestätigt.
Verbesserungen in BACKLOG.md: definitive Ablehnung statt pending_build-Endlosschleife,
Kandidatenalternativen nach sicherer Ablehnung, kompakte Fortschrittsantworten,
frühes Beenden fertiger Bauzeitfenster, ausführungsbezogene Katalogfähigkeit,
Wohnbelegung je Haus und Schutz freier Rückwege beweglicher Biber.
Nächster Spielschritt: zusätzlicher Wohnraum mit offenem südlichem Durchgang,
parallel Nahrungs-/Holzproduktion für Wachstum ausbauen. Gesamtziel bleibt offen.


## Live bestanden: eindeutige Eingabefehler (2026-10-05)

Menschliches Gate bestätigt; fünf installierte Dateien stimmen mit Paket
0.35.4-20261005-182839-57bfd38b überein. advance_building_project und
develop_building_project lehnen waitSeconds=21 mit invalid_argument, Feldname,
erlaubtem Bereich 0–20 und ausdrücklich keinem gesendeten Bau-/Simulationsauftrag ab.
Gültiger develop-Aufruf auf frisch beobachteter belegter Wegzelle: status=ok,
not_started, requestSubmitted=false, area_exhausted und object_intersection.
Spielzeit unverändert (Tag 384, 16,375 Stunden), Tempo vor/nachher 0.

Das Projektledger der früheren Sitzung ist nach Modneustart nicht verfügbar:
gültige inspect_building_completion-Abfrage des alten Auftrags liefert state_conflict.
Daher positive Kontrolle durch frische reguläre Planung ersetzt; kein neuer
Fertigbau und keine sitzungsübergreifende Wiederaufnahme behauptet. Ungültige
Geschwindigkeit synthetisch im menschlichen Testlauf abgedeckt, hier nicht
zusätzlich live erzeugt. Parameterfehler anderer Werkzeugfamilien bleiben offen.
Die folgende Gate-Vorbereitung ist historisch.


## Vor menschlichem Gate: eindeutige Eingabefehler (2026-10-05)

Live-Verbindung auf 0.35.4 bestätigt; keine Spielmutation in diesem Schritt.
Quellcodebefund: zentrale Fehlerbehandlung meldet auch reine Parameterfehler
bei Bauwerkzeugen als möglicherweise unbestätigte Aktion. Für develop_building_project,
advance_building_project und inspect_building_completion jetzt reine Vorabprüfung
von späteren Spielzugriffen getrennt. invalid_argument meldet ausdrücklich keinen
Bau-/Simulationsauftrag; bekannte Zeit-/ID-Felder erhalten konkrete Hinweise.
Echte Fehler nach Beginn der Spielzugriffe behalten ihre bisherige Behandlung.
Werkzeugbeschreibungen nennen waitSeconds 0–20.

Vier Regressionen für Wartezeit 21 und Geschwindigkeit 2 bei develop/advance
mit absichtlich nicht vorhandenem Client ergänzt (jeder Zugriff wäre ein Fehler).
Roslyn-Syntax und Diff geprüft; Tests/Build nicht ausgeführt. Änderung noch nicht
live aktiv und nicht committed. Abnahme nach menschlichem Skript: ungültige
Eingabe eindeutig ohne Auftragswirkung ablehnen, danach gültige Abschlussabfrage
eines vorhandenen Projekts. Bei falscher Fehlerklasse keine weiteren Fälle skalieren.


## Live bestanden: Mehrflächen-Charge (2026-10-05)

Menschliches Gate abgeschlossen; fünf installierte Dateien stimmen mit Paket
0.35.4-20261005-181012-9be85db6 überein. Native Vorprüfung: erste 1×1-Wegfläche
ohne Kandidat, zweite 2×2-Fläche mit zwei Kandidaten. Charge wechselte automatisch
auf regionIndex=1, dokumentierte die erste Fläche und baute genau einen Holzfäller
ohne zusätzliche Wege. Frische unabhängige Abschlussabfrage: finished_accessible,
Eingang frei/erreichbar, 24 Stunden exakt, Überschwingen 0, Pause bestätigt.
Identischer start-Aufruf liefert denselben unveränderten Abschluss/Zeitstempel.
Negativkontrolle mit zwei belegten Wegflächen: no_candidate_in_authorized_regions,
finishedCount=0, keine Auswahl, kein Bau-/Räumlauf. Spiel blieb pausiert.

Abnahme gilt für begrenzten automatischen Flächenwechsel und Abschluss/Replay.
Mehrflächen-Räumung, gemeinsame Budgets und verlorene Bestätigungen synthetisch
abgedeckt, in diesem Live-Pilot nicht zusätzlich erzeugt. Keine freie globale
Standortoptimierung. Vorbereitung und frühere offene Gate-Einträge unten historisch.


## Vor menschlichem Gate: mehrere Teilflächen (2026-10-05)

Nach ausdrücklichem Go implementiert: Basisfläche plus optional additionalRegions
mit bis zu drei weiteren Rechtecken (x/y/z/width/height, jeweils maximal 8×8).
Feste Listenreihenfolge, kein Rücksprung und keine freie Kartensuche. Pro Fläche
bisherige Planung und optionale Räumung; nur nach fehlendem Kandidaten bzw. fehlender
räumbarer Vegetation weiter. Offene/unbestätigte Eingriffe, Materialmangel,
Zugangsfehler oder ausgeschöpftes Räumbudget stoppen die gesamte Charge.

Ein gemeinsames maxClearTargets (maximal 64), keine Multiplikation je Fläche.
Worst-Case-Zeitbudget: clearanceHours × Flächenzahl + constructionHours × Gebäude
höchstens 672 Stunden. Individuelle Laufgrenzen unverändert. RegionIndex und
PreviousRegions bleiben gespeichert; Räumlauf-Kennungen je Fläche getrennt,
Baukennungen weiterhin je Gebäude. Bei Flächenwechsel keine erneute Einreichung
bereits gestarteter Aufträge. Fertignachweise enthalten die Teilfläche.

Alte Einflächenparameter serialisieren unverändert (additionalRegions bei null
weggelassen); alte Fingerprints und Räumkennungen bleiben gültig. Bestehende
terminale Chargen werden nicht nachträglich erweitert: neue Liste braucht neue ID.
Fünf Feature-Dateien syntaktisch geprüft; Regressionen ergänzt, aber noch nicht
ausgeführt. Keine Modänderung, Bridge bleibt 0.35.4. Kein Commit vor Gate/Livetest.

Abnahme nach menschlichem Skript: erste kleine Fläche ohne Kandidat, zweite
mit vorher nativ geprüftem Bauplatz. Eine Charge muss selbst wechseln und ein
Gebäude bis finished_accessible bauen; inspect/erneuter Start mit gleicher ID
darf keinen weiteren Auftrag erzeugen. Negativkontrolle: nur ungeeignete Flächen
stoppen begrenzt. Bei falscher Kontrollprobe oder unbestätigtem Eingriff stoppen.


## Ausbaupilot nach Reichweitenabnahme (2026-10-05)

13 zusätzliche Karottenzellen aus der kombinierten Suche markiert und sämtlich
als lebende wachsende Pflanzen ohne Wasserstress nachgewiesen. Insgesamt 46
Feldmarkierungen (33 vor Erweiterung). Neuer Holzfäller am natürlichen Abstieg:
Baustellenzugang und finished_accessible belegt, besetzt, unterer Wald in nativer
Reichweite. Zwölf reife Kiefern zur Ernte markiert; im Tagesfenster globales frei
verfügbares Holz 15→41. Eigener Ausgang bei Abfrage leer, deshalb keine exakte
gebäudebezogene Produktionsmenge behauptet.

Erste Wohncharge: 1/3 fertig/erreichbar, dann no_candidate_in_authorized_region.
Zweite Charge im selben Gebiet räumt 23 Vegetationsziele innerhalb 48 Stunden,
baut ein weiteres Wohnhaus mit zwei Anschlusswegen fertig/erreichbar, stoppt dann
1/2 ebenfalls mangels Kandidaten. Beide Chargen terminal; kein drittes Haus gebaut.
Alle Zeitfenster beendet und Pause bestätigt. Endstand: 38 Biber (35 Erwachsene,
3 Kinder), 42 Betten/vier frei, kein Obdachloser, keine kritischen Bedürfnisflags;
Wasser 78, Beeren 70, frei verfügbares Holz 21. Keine Gesamtzufriedenheit behauptet.
Gebäudetypenabdeckung unverändert: 24/157 reguläre Vorlagen, 133 noch offen.

Effizienzentscheidung angefragt: begrenzter automatischer Teilflächenwechsel für
Bauchargen oder ausdrücklich weitere manuelle Standortschritte. Zwei Stopps im
8×8-Gebiet zeigen die Grenze. Beispielprojektion bei 20 Aufrufen je Restvorlage:
mehr als 2600 Aufrufe, zusätzlich Bevölkerung und Versorgung; keine gemessene
Prognose. Bis neuer Nutzerentscheidung keine weitere kostspielige Bau-/Suchserie.
Reichweitenfeature bereits committed/gepusht als 4673105.


## Live bestanden: regionale Arbeitsreichweite (2026-10-05)

Menschliches Gate bestätigt, fünf installierte Dateien stimmen mit Paket
0.35.4-20261005-174150-098e2061 überein. Farm: 490 native Reichweitenzellen,
36 interne Reads, planningCalls=0. Fünf freie feuchte Rechtecke mit 13 Zellen;
alle empfohlenen Zellen zugleich frei, feucht und in Reichweite. Unabhängige
32-Zellen-Reichweitenseite stimmt vollständig mit dem Raster überein.
Wohnhauskontrolle: supported=false/source=unavailable, vollständig unbekanntes
Raster und keine Reichweitenempfehlungen; ebenfalls keine Planung (21 Reads).
Keine Spielmutation im Abnahmetest. Feature im beschriebenen Umfang bestanden.
Die folgenden offenen Gate-Einträge sind historische Vorbereitung.


## Vor Test-Gate: regionale Arbeitsreichweite (2026-10-05)

Anbaupilot auf Bridge 0.35.4 bestanden: drei freie, feuchte Zellen mit separat
belegter Farmreichweite als Karotten markiert. Nach rund 23 Spielstunden alle
DREI tatsächlich lebend/wachsend, ohne Wasserstress. Zeitlauf nach exakt 24 Stunden
abgeschlossen, Überschwingen 0, Endpause bestätigt. Danach 35 Biber (26/9),
36 Betten, Wasser 77, Beeren 70, frei verfügbares Holz 7. Keine kritischen
Bedürfnisflags; Vorlagenabdeckung weiterhin 24/157, Gesamtziel offen.

Nächste Effizienzverbesserung MCP-seitig vorbereitet, Bridge bleibt 0.35.4:
survey_region optional workBuildingId und planBuildings=false. Vollständige
native Reichweite (höchstens 2048 Zellen/64 Seiten), Quellenangabe und separates
Raster; freie feuchte Rechtecke daraus neu berechnet, nicht nur vorhandene Top-6
gefiltert. Fehlende Reichweite unbekannt, wechselnde/unvollständige Daten abweisen.
Ohne workBuildingId bleibt bisheriges Verhalten; planBuildings=false spart bis
zu zwölf Planaufrufe. template/districtId bleiben aus Kompatibilität Pflicht.
Keine automatische Markierung und keine Aussage über Personal oder passenden Rohstoff.

Regressionen ergänzt: mehrseitige Schnittmenge, trockene/gesperrte Zellen,
unbekannte Reichweite, wechselnde Zähler, Duplikate, Budget, falsche Sitzung und
Standardverhalten. Drei Feature-Dateien per Roslyn syntaktisch geprüft, kein Build
oder Testlauf. Änderungen uncommitted bis menschliches Gate und Liveabnahme.
Abnahmepilot: Farm plus 16×16-Region, planBuildings=false; planningCalls=0,
Reichweitenzellen gegen gezielte inspect_work_range-Seite prüfen und Empfehlungen
gegen Feuchte/Hindernisse. Nicht unterstütztes Wohnhaus muss unbekannte Reichweite
und keine Empfehlungen liefern. Bei falscher Kontrollprobe stoppen.


## Live bestanden: Bodenfeuchte 0.35.4 (2026-10-05)

Menschliches Gate abgeschlossen; alle fünf installierten Dateien stimmen mit
Paket 0.35.4-20261005-172618-6954fbfd überein. Native MCP-Verbindung erreichbar.
Vier Forstzellen feucht, 16 obere Vergleichszellen trocken; vier Zellen über
Boden liefern null. Drei regionale Abfragen stimmen mit den direkten Proben
überein. Trockenes freies 2×2-Rechteck ausgeschlossen, feuchtes freies 2×1-Rechteck
gefunden und separat bestätigt. Rechteckzellen sind gleichzeitig hindernisfrei
und feucht; bestehende Pflanzflächen bleiben ausgeschlossen.
Regional 12 bzw. 17 native Reads, keine zusätzlichen Reads für Bodenfeuchte.
Keine Spielmutation oder Simulation; Spiel pausiert. Momentane Bodenfeuchte
belegt keine Arbeitsreichweite, pflanzenspezifische Eignung oder Dauerbewässerung.
Versionslisten-/Syntaxkorrektur eingeschlossen; frühere offene Gate-Einträge
unten sind historisch. Feature im beschriebenen Umfang abgenommen.


## 0.35.4: Live-Einstieg blockiert, Clientkorrektur vorbereitet

Nachfolgendes menschliches Gate beim Kompilieren abgebrochen: Versionsersetzung
hatte einen einzelnen !=-Vergleich ungültig erweitert. Auf is not (Versionen)
korrigiert; alle 29 geänderten C#-Dateien mit Roslyn syntaktisch geprüft, ohne
Syntaxfehler. Das ersetzt keine Typprüfung, Tests oder den offenen Livetest.

2026-10-05: Nach menschlichem Gate ist Paket 0.35.4 vorhanden; Spielprotokoll
meldet geladene Mod und bereiten Endpoint, keine passende Start-Exception.
Status und Simulation scheitern jedoch bereits vor dem Feuchtigkeitsnachweis.
Quellcodebefund: zentrale Envelope-Versionsliste und weitere Vertragsprüfungen
akzeptierten nur bis 0.35.3. Alle bisherigen 0.35.3-Prüfungen um 0.35.4 ergänzt,
historische Versionen und Vertragsgrenzen unverändert. Regression liest die
Version direkt aus dem Mod-Manifest und prüft den NativeClient mit synthetischer
Feuchteantwort sowie Fähigkeits-/Budgetprofil. Nur statisch geprüft.
Erneutes menschliches Skript-Gate nötig; kein bestandener Livetest, kein Commit.


## Test-Gate offen: Bodenfeuchte (0.35.4, 2026-10-05)

Auf Nutzerauftrag leere grüne/bewässerte Bodenflächen direkt abfragbar machen.
Öffentliches ISoilMoistureService.SoilIsMoist(Vector3Int) lokal per Metadaten
nachgewiesen und in die Kartenabfrage integriert; nur Boden, sonst null.
Regionalsuche ergänzt separate Feuchtigkeitszeilen und freie feuchte Rechtecke
ohne zusätzliche native Reads. Alte Bridge: unbekannt statt trocken.
Feuchte bedeutet weder Pflanzbarkeit noch Arbeitsreichweite oder künftigen Ertrag.
Zwei Regressionstests ergänzt; nur Quellcode/Diff geprüft, noch kein Build,
Deploy, Livetest oder Commit dieser Erweiterung. Übergabe: docs/regional-survey.md.


## Fortsetzung 2026-10-05 — unknown-Reparatur live bestanden

Nutzer hat die Arbeit mit live bereit fortgesetzt. Neues Paket
0.35.3-20261005-165123-33f2985e: alle fünf installierten Paketdateien stimmen.
Anfangs Transport closed; nach erneuter Bereitmeldung reguläre MCP-Verbindung
wieder erreichbar. Kein zusätzlicher Server oder Spielsteuerungs-Fallback.

Gleicher 16×16-Nordpilot liefert jetzt drei Bodenrechtecke und vier Baukandidaten,
33 interne Leseaufrufe inklusive zwölf Planungen in etwa 2,65 Sekunden. Die
überlappende Teilfläche mit natürlicher Slope bleibt ohne freie Bodenrechtecke.
Eine unbeteiligte 4×1-Fläche ist separat als hindernisfrei, trocken und auf
richtiger Höhe geprüft. Reparatur im angekündigten Umfang live bestanden;
Mehrteilflächen-Überlappung synthetisch abgedeckt, nicht zusätzlich live erzeugt.
Nächster Schritt: nachweislich erreichbare Produktionsflächen vergrößern.



## Tagesabschluss 4./5. Oktober 2026 — Arbeit pausiert

Feierabend auf Nutzerwunsch. Gesamtziel offen; keine weitere Spielsteuerung,
Tests oder Installation bis zur Fortsetzung. Letzter bestätigter Spielzustand:
32 lebende Biber, 36 Betten (vier frei), 24 von 157 regulären Vorlagen gebaut.
Fünf Entwicklerwerkzeuge ausgeschlossen; 133 reguläre Vorlagen ohne Baunachweis.
Zufriedenheit insgesamt nicht nachgewiesen. Spiel zuletzt mit Tempo 0 beobachtet;
kein neuer Save oder erneuter Zustandsabruf zum Tagesabschluss behauptet.

Versorgung: zuletzt Wasser 83, Beeren 60, frei verfügbares Holz 1. Karottenbestand
zuletzt 0; 30 lebende Pflanzen vorhanden. Die 13 Kiefern der Forststichprobe
wachsen noch, ohne Wasserstress. Nächster wirtschaftlicher Schritt: größere
nachweislich erreichbare Holz- und Nahrungsproduktion, danach weitere Wohnplätze.

Abgenommen: Speichersperren im Zeitlauf, begrenzte Bauchargen, regionale Suche
und ein daraus tatsächlich fertig/erreichbar gebautes Wohnhaus. Letzter gepushter
Checkpoint: 4d117db; regionale Suche in e6006e3. Die nachfolgende unknown-Reparatur
ist nur statisch geprüft und bleibt uncommitted: RegionSurvey.cs plus zwei
Regressionsfälle in RegionSurveyTests.cs. Auch diese Abschlussdokumentation ist
lokal gespeichert. Andere bereits vorhandene Änderungen bleiben erhalten.

Wiedereinstieg: menschliches prepare-human-live-test.ps1 bei beendetem Spiel,
danach laden und live bereit. Zuerst unknown-Reparatur im bisherigen Nordbereich
gegenprüfen: nur tatsächlich betroffene Teilflächen unbekannt, keine Freigabe
unbekannter Geometrie. Erst nach bestandenem Gate/Livetest gezielt committen und
pushen, anschließend Versorgungsausbau. Ablauf: DEVELOPMENT_WORKFLOW.md.



## Vor menschlichem Gate — unbekannte Objekte regional begrenzen (2026-10-05)

Fortsetzung des Gesamtziels: Nordscan 16×16 liefert wegen unbekannter Objekte
keine Bodenrechtecke. Eine natürliche Slope auf niedrigerer Ebene wurde in einer
überlappenden Teilfläche direkt nachgewiesen. Fehler im Klassifikator: jedes
other-Objekt sperrte alle ansonsten freien Zellen der gesamten Region.

Korrektur: unbekannte Grundrisse sperren nur die nativen 8×8-Abfrageflächen,
die ihre Überlappung tatsächlich melden. Überlappt ein Objekt zwei Teilflächen,
bleiben beide gesperrt. Bekannte Gebäude, Eingänge und Pflanzmarkierungen bleiben
geschützt. Keine Annahme über den unbekannten Grundriss, keine neue Baufreigabe.
Zwei synthetische Regressionsfälle ergänzt; Quellcode/Diff geprüft. Kein Build,
keine Installation und kein Commit/Push dieser Reparatur.

Nach menschlichem Gate denselben Nordausschnitt prüfen: unbekannte Objekte dürfen
unbeteiligte Teilflächen nicht mehr sperren. Betroffene Überlappungsflächen müssen
unbekannt bleiben; freie Rechtecke stichprobenweise nativ prüfen. Bei falschem
Kontrollfall stoppen. Aktueller Ausbau unverändert: 32 Biber, 36 Betten, 24/157;
Holz frei 1. Keine neuen Spielaktionen in dieser Diagnose. Aufforstung/Nahrung
bleiben der nächste Schritt nach Reparatur. Die untersuchte Nordreihe enthält
auch einen versetzten Wohnhausgrundriss und wurde deshalb nicht bepflanzt.



## Praxistest: regionaler Kandidat bis zum fertigen Wohnhaus

2026-10-04: Ersten regional gefundenen Lodge-Kandidaten im Entwicklungspilot
frisch geplant und gebaut. Tatsächlicher Baustellenzugang buildersReachable=true
vor Zeitlauf, anschließend finished_accessible: Gebäude fertig, Eingang frei
und erreichbar. Exakt 24 Spielstunden, Überschwingen 0, Endpause bestätigt.
Bevölkerung danach 32 (25 Erwachsene, 7 Kinder), 36 Betten, vier frei;
keine kritischen Bedürfnisflags. Vorlagenabdeckung unverändert 24/157.

Versorgungsgrenze: Wasser 83, Beeren 63, frei verfügbares Holz 1. Karottenbestand
während des Laufs 0; nach Abschluss 30 lebende Karottenpflanzen, davon zwei reif,
keine mit Wasserstress. Forststichprobe: 13 lebende Kiefern, noch nicht ausgewachsen,
kein Wasserstress. Besetzte westliche Holzfällerflagge meldet laufenden Job;
das allein beweist keinen ausreichenden Holzoutput. Nächster Ausbau muss zuerst
Holz- und Nahrungsproduktion vergrößern, nicht weitere Baumaterialien verplanen.

Bedienungsfehler im Test: advance_building_project akzeptiert waitSeconds nur
0–20, Bauchargen dagegen 0–45. Versuch mit 30 vor Ausführung abgelehnt; lesend
awaiting_simulation und run=null bestätigt, dann gezielt auf 20 korrigiert.
Backlog: Parametergrenzen in kurzen Werkzeugbeschreibungen nennen und reine
Parameterfehler von möglicherweise unbestätigten Aktionen unterscheiden.



## Live bestanden — regionale Flächensuche (2026-10-04)

Menschliches Gate abgeschlossen, fünf installierte Paketdateien mit Paket
0.35.3-20261004-234748-393364c9 abgeglichen. 16×16-Pilot: 32 native Leseaufrufe,
darunter zwölf Planungen, in etwa 2,6 Sekunden. Vier Baukandidaten und vier
Bodenrechtecke in einer kompakten Antwort. Richtwert etwa 30 Aufrufe knapp
überschritten; feste Obergrenze 82 eingehalten. Kein zusätzlicher Scan nötig.

Alle 30 Feldmarkierungen als belegt, sechs unabhängig abgefragte tiefere
Wasserzellen als andere Bodenhöhe ausgeschlossen. Stichprobe von 17 vorhandenen
Gebäuden/Wegen: belegte Grundrisszellen und Eingänge nicht frei. Größtes freies
Bodenrechteck unabhängig ohne überlappende Objekte. Erster Kandidat nativ
validiert: Geometrie gültig, Eingang angeschlossen, keine verlorenen Verbindungen
in den geprüften Bestandsproben, Vorschau wiederhergestellt. Baustellenzugang
und tatsächlicher Fertigzugang bleiben unknown; keine allgemeine Baufreigabe.

Negativkontrolle bei bestätigtem Tempo 1 liefert state_conflict ohne Bericht.
Kontrolllauf danach abgebrochen, Pause separat bestätigt. Feature im begrenzten
Umfang abgenommen; keine Behauptung vollständiger Regionssuche, Bewirtschaftung
oder neuer Gebäude durch diese lesende Abfrage. Nächster Schritt: Kandidaten für
den weiteren Gesamtausbau verwenden. Letzter Ausbaucheckpoint: 31 Biber, 24/157.

## Gesamtziel — Ausbaucheckpoint 2026-10-04 nach Zeitlauf-Reparatur

Verifiziert: 31 lebende Biber (22 Erwachsene, 9 Kinder), 33 Betten, zwei frei,
keine Obdachlosen und keine kritischen Bedürfnisflags. Wasser 90, Beeren 70,
verfügbares Holz 18; Übersicht liefert keinen gesamten Nahrungsbestand.
Zwei weitere Lodge-Wohnhäuser und eine HaulingPost fertig und laut Charge
finished_accessible; Fertigstatus aller drei unabhängig zurückgelesen.
Transportstation mit fünf von fünf gewünschten Arbeitern, laufender Job.
Vorlagenabdeckung jetzt 24/157: HaulingPost neu, 133 noch ohne Baunachweis.

Erste Charge: ein von drei Häusern fertig, dann kein weiterer Kandidat im
kleinen Südostfenster. Zweite Charge: größeres 8x8-Fenster, 28 Vegetationsziele
geräumt, Wohnhaus und Transportstation nacheinander fertig; zwei neue Wege.
Keine blinde Wiederholung des gestoppten Auftrags. Vier begrenzte Zeitfenster
(48 + 24 + 24 + 24 Stunden) abgeschlossen; aktuelles Tempo separat 0 bestätigt.

Karottenfläche von 23 auf 30 Zellen erweitert; sieben neue Pflanzen unabhängig
lebend/wachsend und ohne Wasserstress beobachtet. Vier Bäume gezielt entfernt.
Farm tatsächlich mit drei von drei Arbeitern besetzt. Westliche vermeintliche
Freifläche liegt tiefer im Wasser, östliche Reserve höher: daher dort kein
Feldauftrag. Markierung und tatsächliche Pflanzung getrennt nachgewiesen.

Effizienz: rund 40 fachliche MCP-Aufrufe für drei fertige Gebäude (nur eine neue
Vorlage) und sieben Feldzellen. Bauchargen helfen, regionale Platzwahl und
Versorgungsaufbau bleiben manuell. Grobe lineare Warnprojektion bei diesem Mix:
133 übrige Vorlagen etwa 5.000 Aufrufe; keine verlässliche Gesamtprognose.
Aufwandsalarm gestellt, neue ausdrückliche Entscheidung gemäß AGENTS.md erbeten.
Nutzer entschied anschließend ausdrücklich: regionale Flächensuche entwickeln.
Gesamtziel nicht erreicht.



## Live bestanden — temporäre Spielsperren in Zeitläufen (2026-10-04)

Menschliches Test-/Installationsgate abgeschlossen; alle fünf installierten
Paketdateien stimmen mit Paket 0.35.3-20261004-231320-8e75fd0c überein.
Gezielter Vier-Tage-Lauf: beim vom Nutzer ausgelösten Speichern dreimal
running/game_speed_locked mit Tempo 0 beobachtet; danach derselbe Lauf wieder
running/advancing mit Tempo 7. Ziel und Echtzeitbudget unverändert.
Abschluss nach exakt 96 Spielstunden und 271,166 Echtzeitsekunden: completed,
target_reached, pauseConfirmed, Überschwingen 0. Aktuelles Tempo separat 0.

Negativkontrolle: ausdrückliche MCP-Pause unterbricht einen zweiten aktiven Lauf
mit explicit_speed_change; aktuelles Tempo danach separat 0 und Spielzeit stabil.
Der eingefrorene Unterbrechungsbeleg enthält noch Tempo 7 vor Befehlswirkung;
deshalb die aktuelle Pause immer gesondert lesen.

Öffentliches SpeedLockChangedEvent über EventBus angebunden; keine Entsperrung
oder Wiederbeschleunigung durch die Bridge. Sperre beim Speichern jetzt direkt
nachgewiesen; eine rückwirkende eindeutige Ursache aller früheren Abbrüche ist
nicht beweisbar. Budgetablauf/Abbruch während Sperre synthetisch geprüft, im
Livetest nicht zusätzlich provoziert. UI-Geschwindigkeitswechsel separat nicht
live getestet. Gesamtziel bleibt offen: zuletzt 27 Biber, 23/157 Vorlagen.

## Gesamtausbau: 27 Biber, 23/157 Vorlagen

Sieben weitere Gebäude fertig; Karottenanbau und Bretterlager wirken.
Spiel nach erfolgreicher Zeitlauf-Reparaturabnahme bestätigt pausiert. 27 Betten voll;
Holzversorgung und weitere Wohnplätze als nächstes. Zwölf neue Forstzellen
lebend/wachsend und ohne Wasserstress nachgewiesen. Gesamtziel weiterhin offen.
[Belege und offene Schritte](docs/playtest-2026-10-04.md),
[vollständige Vorlagenliste](docs/building-coverage.csv).


## Live bestanden — Bauchargen (2026-10-04)

Menschliches Gate mit anschließendem „live bereit“; fünf installierte Paketdateien
gegen das neue Paket geprüft. Zwei Gebäude automatisch nacheinander fertig und
zugänglich: kleines Lager mit Carrot/obtain, danach Mini-Wohnhaus. Zehn tote Bäume
automatisch geräumt, sechs lebende erhalten. Räumwartefenster tatsächlich gestartet;
bei der unabhängigen Rückfrage waren die Ziele bereits entfernt. Drei native
Zeitfenster je exakt 24 Stunden, kein Überschwingen, Pause jeweils bestätigt.
Fünf ausführende Chargenaufrufe bis Abschluss; Replay/Budgetkontrolle zusätzlich.
Identischer Start und abgeschlossene Fortsetzung unverändert, Budgetänderung
abgelehnt. Negative bebaute 1×1-Fläche stoppt vor Bau/Räumung und Folgegebäude.
Unabhängige Zugangs-/Lagerabfragen stimmen mit Abschluss überein. 21 Betten,
17 belegt; kein Nachweis für 100 zufriedene Biber oder neue Vorlagenabdeckung.

Grenzen: kein erzwungener Prozessabbruch im Livetest; Materialmangel und erschöpftes
Räumbudget synthetisch abgedeckt, hier nicht zusätzlich live provoziert.
Baustellenkonfiguration in diesem Pilot erst nach Fertigstellung zurückgelesen.
Verbesserungen: vollständiges Räumbudget läuft trotz früher geräumter Ziele aus;
Budgetkonflikt meldet zu allgemein invalid_argument; gespeicherter completion-Wert
kann während Räumung noch zum vorherigen Gebäude gehören. Keine falsche Aktualität
behaupten: checkpointOnly und Zeitstempel beachten. Breitere Vorlagen-/Höhenplanung
und kleinere gezielte Räumflächen bleiben offen.


## Vor menschlichem Gate — vollständige Bauchargen

Erster menschlicher Skriptlauf beim MCP-Build mit CS8604 abgebrochen; Installation
nicht erreicht. Nullprüfung des Chargenauftrags korrigiert (explizite Ablehnung
vor Feldzugriff). Erneuter menschlicher Skriptlauf und Livetest stehen aus.

Zweiter Skriptlauf: MCP- und Integrationstest-Projekt bauen erfolgreich;
Unit-Test-Projekt stoppt mit 22 xUnit1051-Meldungen. Alle 22 neuen asynchronen
Testaufrufe reichen jetzt TestContext.Current.CancellationToken weiter.
Tests/Installation noch nicht durchgelaufen; nächster menschlicher Lauf offen.

Dritter Skriptlauf: alle Projekte bauen; Tests scheitern am MCP-Eingabeschema
der Chargenwerkzeuge und verursachen dadurch weitere Katalog-/Startfehler.
Schemawurzel explizit auf object eingeschränkt, passend zur vorhandenen
Null-Ablehnung des Parsers. Gezielter Katalogtest mit/ohne Schreibfreigabe ergänzt.
Erneuter menschlicher Testlauf und Installation bleiben ausstehend.

Nach erneutem ausdrücklichem Go: MCP-Chargensteuerung für Standortwahl,
einmalige autorisierte Vegetationsräumung samt Wartebudget und sequenzielle
Fertigstellung implementiert. Private persistente Checkpoints vor Eingriffen,
lesende Klärung nach Abbruch, feste Budgets, kompakte Fortschrittsantworten.
Synthetische Prüfungen ergänzt; bislang nur Quellcode-/Diff-Prüfung, kein
Testlauf/Build/Installation/Livetest. Noch kein Commit oder Push dieser Änderung.
Nächster Schritt: menschliches `scripts/prepare-human-live-test.ps1`, danach
gezielter Zwei-Gebäude-Pilot mit Wiederaufnahme und negativem Kontrollfall.
[Umfang und Abnahme](docs/building-batch-workflow.md).

## Live bestanden — wiederaufnehmbare Bauabläufe

Reparatur nach erneutem menschlichem Gate abgenommen: neues Lager und zwei Wege
fertig, Eingang frei/zugänglich, Carrot/obtain erhalten. Komponentenanzahl 0
belegt fehlenden optionalen Anschlussblocker; Aggregat finished_accessible stimmt
mit Einzelabfragen überein. Exakt 24 Stunden, kein Überschwingen, aktuelle Pause.
Wiederaufnahme nach Abschluss erhält Lauf-ID/Ziel/Zeit; geändertes Budget wird
abgelehnt. Fünf Paketdateien identisch zur Installation. Vier Ablaufaufrufe bis
zum Abschluss, zusätzliche Kontrollen separat. Neue Mod-/MCP-Fähigkeit begrenzt
live bestanden; kein autonomer Gesamtausbau oder Zufriedenheitsbeleg.

Historischer Erstversuch: menschlicher Skriptlauf erfolgreich; Paketdateien verifiziert.
Live: Standortablehnung, Baustart, Bauzugang, Lagerkonfiguration, Wiederaufnahme,
Budgetkonflikt und exakt begrenzte Simulation bestanden. Lager fertig/erreichbar,
aber Aggregat fälschlich budget_exhausted wegen fehlendem optionalem Anschluss-
blocker. Reparatur ergänzt dessen Komponentenanzahl, unterscheidet nachgewiesene
Abwesenheit von unknown und meldet fehlenden Zugang separat. Reparatur inzwischen
nach erneutem menschlichem Gate live bestätigt.

Nach erneutem Nutzer-Go zur Ausbauautomatisierung: `develop_building_project`
bündelt Planung/Start und begrenzte Bauzeitbegleitung; `advance_building_project`
und `inspect_building_completion` setzen anhand vorhandener nativer Auftrags-
und Lauf-IDs fort. Ein unveränderliches Zeitfenster pro Bauprojekt; Abschluss
prüft alle Bauobjekte, Gebäudezugang, Anfangskonfiguration und aktuelle Pause.
Erfolglose Suche ergänzt eine Diagnose am Suchursprung. Keine neue Abhängigkeit
oder Bridge-Version; additiver Komponentenbeleg in der Mod, Zugangsgrenzen erhalten.
Synthetische Regressionen und Katalogintegration ergänzt; menschlicher Skriptlauf bestanden.
[Ablauf, Grenzen und Abnahme](docs/building-completion-workflow.md).

## Live bestanden — gebündelte Räumung und Baustart

Nach menschlichem Gate und Reparatur-Livetest auf Bridge 0.35.3 abgenommen.
`remove_vegetation_batch`: 1–16 konkrete Ziele seriell, Einzelbelege und Stopp
beim ersten Konflikt/unbestätigten Ergebnis. Drei Ziele in einem Aufruf entfernt,
separat bestätigt. Konfliktkontrolle nach Mod-Korrektur: state_conflict,
Folgeziel not_attempted, beide Pflanzen unverändert. Fünf Räum-Vorbedingungen
vor dem Lifecycle liefern jetzt explizite Konflikte statt HTTP-400-Unsicherheit.
Unsichere tatsächliche Änderungen bleiben unconfirmed; kein Retry/Rollback.
`start_building_project`: Planung und nativer Pilotstart in einem Aufruf,
explizit first_candidate/development_pilot, bestehende Zugangsprüfungen erhalten.
Belegtes Feld not_started/requestSubmitted=false. Positivfall: Lagerstart samt
Carrot/obtain an unfertiger Baustelle bestätigt, Bauarbeiterzugang nachgewiesen.
Nach einem Spieltag fertig und erreichbar, Einstellungen erhalten, Lauf exakt
24 Stunden mit bestätigter Pause. Fünf installierte Paketdateien verifiziert.
Drei Räumaktionen bzw. bisher Plan/Validierung/Start je in einem MCP-Aufruf;
Bauzeit und Abschlussprüfung bleiben separat. Kein allgemeiner Mehrgebäudebatch.

## Live bestanden — gebündelte Kolonieübersicht und Bauprüfdetails

Nach menschlichem Gate und gezieltem Livevergleich abgenommen. Neues lesendes `inspect_colony_overview`
bündelt Snapshot und höchstens vier Bedürfnis-Seiten in einem MCP-Aufruf.
Vollständigkeit, Fortsetzungsposition, fehlende Beobachtungen und schwankende
Bevölkerungszähler bleiben explizit; keine atomare Beobachtung, kein Gesamtfutter-
oder Zufriedenheitsnachweis. Sitzungs-/Versionswechsel und wechselnder Katalog
werden abgelehnt, ohne Wiederholung. Bridge bleibt unverändert bei 0.35.3.
Bauprüfung compact kürzt Zell-/Quellenangaben; unabhängige Diagnosezustände,
Grenzen, Verluste und Wiederherstellung bleiben. Live: Bauprüfung 3922 → 3304
Zeichen (16 %), projizierte Daten gleich außer laufendem Versuchszähler.
Kolonieübersicht 5732 statt zusammen 14363 Zeichen (60 %), ein statt drei
MCP-Aufrufen; alle 42 Bedürfnisse und Zustandswerte gegen Einzelabfragen geprüft.
Spiel pausiert, complete/countsStable=true, atomic=false. Fünf installierte
Paketdateien stimmen mit dem neuen menschlich erzeugten Paket überein.
Keine neue Baufreigabe und kein Gesamt-Zufriedenheitsnachweis.

## Live bestanden — kompakte MCP-Antworten, erster Pilot

Nutzer-Go für effizientere MCP-Abläufe nach Aufwandsalarm. MCP-Präsentationsschicht
für Status, Lagereinstellungen und generische Bauvalidierung: Standard compact,
optionales `detail=full`. Bestehende native Validierung vor der Projektion;
Fehler/unbestätigte Einstellungsänderungen vollständig, Unknown-/Zugangsbelege
bleiben sichtbar. Keine Spielmod- oder Versionsänderung (Bridge 0.35.3).
Menschliches Gate und Livevergleich bestanden: Lagerlesen 1258 → 593 Zeichen
(53 % weniger), identische Zustandswerte. Einmalige Modusänderung separat bestätigt,
veralteter Erwartungswert abgelehnt, ungültiges detail als -32602 abgelehnt.
Bauprüfung 3922 → 3647 Zeichen (7 % weniger), semantisch gleich bis auf den
Versuchszähler; valid=false, Wiederherstellung und Unknowns erhalten.
Kein breiter Effizienzgewinn für Bauprüfung behauptet; dafür gezielt weiterarbeiten.
Nachweise in docs/playtest-2026-10-04.md.

## Live bestanden — 0.35.3 Lagerkonfiguration im Bauauftrag

Auf Nutzerwunsch optionale initialStorageGood/initialStorageMode am ebenen
Bauprojekt. Direkt an der bestätigten, initialisierten Baustelle anwenden;
ausdrücklich kein Warten auf Bauabschluss. Getrennter Konfigurationsbeleg mit
Rücklesung auf späterem Frame, unveränderte Bauzugangsprüfungen. Settings-Opt-in
zusätzlich auf MCP- und Bridge-Seite. Unpassende Lagergüter vor Bau ablehnen,
Konfiguration in Replay-Identität aufnehmen, keine Wiederholung bei Unsicherheit.
Menschliches Gate abgeschlossen, fünf installierte Paketdateien stimmen per
SHA-256 überein. Feature-Livetest bestanden: Carrot/obtain an der unfertigen
Baustelle bestätigt, ungültiges Gut ohne Bau abgelehnt, Replay ohne Doppelbau,
verändertes Replay abgelehnt. Nach zwölf Spielstunden Lager fertig, Eingang frei,
Konfiguration erhalten, Simulation automatisch pausiert.
Farm/Personal/Prioritäten und große/Sondergebäude bleiben Folgearbeit.

## Abschluss am 2026-10-04 — 0.35.2 Projektbudget

Menschliches Gate und gezielter Live-Test bestanden: fünf sequenzielle ebene
Projekte derselben Sitzung fertig und erreichbar, erstes Replay ohne neue Objekte,
belegter Standort weiterhin abgelehnt, keine offenen Baustellen, Simulation pausiert.
1024 ebene und 1024 gemeinsam gezählte vertikale Projekte je Sitzung; historische
Profile, Geometrie-, Zugangs- und Isolationsgrenzen bleiben unverändert.
Grenzfall im automatischen Controllertest, kein vertikaler Massentest behauptet.
Nächster Nutzerwunsch: gewünschte Lagerkonfiguration direkt im Bauauftrag;
große und besondere Vorlagen bleiben genehmigte Folgearbeit.
[Nachweise und Playtestfortschritt](docs/playtest-2026-10-04.md).

Stand: 2026-10-03. Das Projekt entwickelt eine native MCP-Steuerung für Timberborn.
Das Spiel ist ausschließlich Testsystem; konkrete Spielstände gehören nicht zur
Projektbeschreibung.

## Phase E abgeschlossen

Mit 0.35.1 ist Phase E im vereinbarten begrenzten Umfang abgeschlossen.
[Abschluss und Nachweise](missionsplan.md#abschluss-phase-e).
Vollständiger Bauphasenschutz, Umweg-Schutzfolge, freie Höhen-/größere
Projektplanung und besondere Gebäude-/Kompatibilitätsfälle bleiben als
[F01–F05 im Backlog](BACKLOG.md#offene-folgearbeiten-nach-phase-e) offen.
Sie sind Folgearbeiten, keine ausstehende E-Abnahme; keine Schutzgrenze gelockert.

## Letzter technischer Abschluss — 0.35.1 budgetgerechte ebene Routensuche

Nach menschlichem Gate als 0.35.1 geladen. Bauablauf und isolierte
Fünf-Wege-Ablehnung mit anschließender Vier-Wege-Kandidatensuche live bestanden. Der Planer
minimiert neue Wegfelder, bei Gleichstand die Routenlänge, und verwirft Standorte
über dem unveränderten Vier-Wege-Limit vor Belegung der vier Vorschlagsplätze.
Volle Grundfläche bleibt gesperrt; native Gemeinschaftsvorschau und Zugangsnachweise
bleiben erforderlich. Bank-/LargePile-Livehistorie im MCP-Profil ergänzt.
Fünf Kandidaten geprüft, einer verworfen, vier Vorschläge mit 4/3/2/1 Wegen;
Wiederholung identisch. Zwei Banken um Hindernis fertig und erreichbar,
gefährliche Hauptwegvorschau wegen 28 Anschlussverlusten abgelehnt. Sechs Wege
für den abschließenden reinen Planungstest entfernt, Teststreifen bleibt offen.
[Nachweis und Grenzen](docs/road-budget-planner.md). Simulation pausiert.

## Vorheriger Abschluss — 0.35.0 generischer ebener Baupilot live bestanden

Gebäudenamensliste durch aktiven Katalog und vollständige gedrehte Spielgeometrie
ersetzt. Routenbaustein für mehrere Zugangskandidaten vorbereitet; öffentliche
Spiel-API liefert derzeit nur einen Wegzugang. Schutzprüfungen und feste
Vertikalpiloten unverändert. Lodge-Historie Drehung 3 im neuen Profil nachgetragen.
Nach menschlichem Gate Bank (ein Feld, zwei neue Wege) und großes Freiluftlager
(3x3 Grundfläche, ein neuer Weg), beide Drehung 1, fertig gebaut; Bau-/Fertigzugang
bestätigt. Einzelnes belegtes äußeres Eckfeld des großen Grundrisses nativ
abgelehnt. Blockierter Hauptweg hätte 24 erfasste Anschlüsse
verloren und wurde in der nativen Vorschau abgelehnt. Höhenkontrolle,
Bestandszugänge auf beiden Ebenen und Replay beider Projekte bestanden. Begrenzte Läufe exakt,
Abschluss pausiert. Direkt-fertig-Distriktzentrum und Spiel-Multi-Eingang nicht
live belegt; Bank-/LargePile-Historie seit 0.35.1 im MCP-Profil ergänzt.
[Umsetzung und Nachweis](docs/generic-building-project.md).

## Letzter Abschluss — 0.34.0 live bestanden

Ebener Projektpilot um Lodge.Folktails erweitert; vorhandene Spielgeometrie und
Zugangsprüfungen unverändert genutzt. MCP-Nachweise je Gebäudevorlage getrennt,
keine Lagerabdeckung auf Wohnhäuser übertragen. Gedrehtes Wohnhaus in Rotation 3
mit zwei neuen fertigen Anschlusswegen, tatsächlichem Bau-/Fertigzugang und
erhaltenen Vergleichsanschlüssen live bestanden. Grundriss-Negativkontrollen
und Replay ohne neue Objekte bestanden. Profil-Historie noch vor Abnahme;
neuen Wohnhausnachweis bei nächstem ohnehin nötigen MCP-Gate aufnehmen.
[Nachweis](docs/lodge-project-pilot.md).

## Letzter Abschluss — 0.33.1 live bestanden

0.33.1 ist installiert und live geprüft: bis zu vier sequenzielle Vertikalprojekte pro Sitzung,
gemeinsames Budget aller Modi, erhaltene Replay-/Baustellensperren. MCP-Profil
aktualisiert auf die vier historisch belegten Lager-Treppendrehungen.
Eine Treppe und anschließend ein siebenstufiges Lagerprojekt ohne Neuladen
fertiggebaut. Startsperre bei unfertigem Vorgänger/Schlusslager und Replay
während/nach dem Folgeprojekt bestanden; acht Objekte ohne Doppelbau.
Bau-/Fertigzugang, obere/untere Wege und Vergleichslagerzugang geprüft.
[Umfang und Nachweis](docs/vertical-sequential-pilot.md).

## Verifizierter Stand

Zusätzlicher E-Livefall auf unveränderter 0.33.0: vertikales Lagerprojekt mit
Treppendrehung 0 / Lagerdrehung 2, alle sieben Objekte fertig. Lagerbaustelle
builder-erreichbar, fertiger Eingang frei, Distanz 34; beide oberen Wege und
unterer Bestandsweg verbunden, Vergleichslager unverändert bei Distanz 25.
Replay ohne Doppelbauten. Ergänzend Treppendrehung 2 / Lagerdrehung 0 live
bestanden: sieben fertige Objekte, Bau-/Fertigzugang und Distanz 36, Bestandsweg
verbunden, Vergleichslager unverändert bei Distanz 15, Replay ohne Doppelbauten.
Treppendrehung 1 / Lagerdrehung 3 ebenfalls live bestanden: sieben fertige
Objekte, Bau-/Fertigzugang und Distanz 50; Bestandsweg verbunden,
Vergleichslager unverändert bei Distanz 15, Replay ohne Doppelbauten.
Alle vier Treppendrehungen des festen vertikalen Lagerprojekts begrenzt dokumentiert.
Der damalige MCP-Nachweiskatalog enthielt noch 3; Aktualisierung in 0.33.1 bestanden.
[Nachweis](docs/vertical-warehouse-pilot.md).

MCP-only-Abfrage `inspect_building_capabilities` nach korrigiertem menschlichem
Gate live bestanden: Profil 0.33.0/Folktails, fünf Modi mit erwarteten Grenzen,
historische Live-Abdeckung und Server-Schalter getrennt. Falsche Sitzung und
Zusatzparameter abgewiesen; Objektzahl, Pause und Spielzeit unverändert.
Keine Baufreigabe, Bridge bleibt 0.33.0. [Nachweis](docs/building-capabilities.md).

0.33.0 nach menschlichem Skript-Gate installiert und live bestanden: mittleres
ebenes Folktails-Lager im bestehenden Entwicklungspilot, zusätzliche Vorlagenbindung
der Belege; keine Lockerung der B-Sperren. Rotation 3 mit sechs Zellen (gedreht
2×3), tatsächlicher Bauarbeiterzugang und anschließend fertig mit freiem Eingang,
Distriktdistanz 25. Blockierte entfernte Grundrisszellen und zu kleine Suchregion
korrekt ausgeschlossen. Zwei Bestandswege verbunden, Vergleichslager unverändert
bei Distanz 19, Aktions-ID-Replay ohne neue Objekte.
[Nachweis und Grenzen](docs/medium-warehouse-pilot.md).

Ergänzend auf unveränderter 0.33.0 live bestanden: mittleres Lager Rotation 1
mit drei tatsächlich neuen, einzeln fertig und distriktverbunden rückgelesenen
Bodenwegen. Baustellenzugang vorhanden; fertiger Eingang frei, Distanz 36.
Ursprünglicher Anschlussweg erhalten, Vergleichslager unverändert bei Distanz
19/25; Vier-Objekt-Beleg wiederabfragbar ohne Doppelbauten.

Ebene Drehungen 0/2 des mittleren Lagers ebenfalls auf unveränderter 0.33.0 live
bestanden: jeweils sechs korrekte Grundrissfelder, tatsächlicher Bauarbeiterzugang,
fertige Lager mit freiem Eingang und Distanz 31/33. Beide Anschlusswege erhalten,
Vergleichslager unverändert bei Distanz 19. Aufträge sequenziell nach tatsächlicher
Fertigstellung. Alle vier ebenen Drehungen der zusätzlichen Vorlage damit begrenzt
belegt. Nächster Vorschlag: unterstützten Projektumfang und Nachweisniveau im MCP
strukturiert ausweisen; keine freie Höhenplanung oder allgemeine Schutzbehauptung.

0.32.1 nach menschlichem Skript-Gate installiert und live bestanden: konservative
Baustellensperre B. Neue Bauaktionen benötigen vollständige Bestandsinventur
ohne offene unabhängige Baustellen. Eigene Baustellen dürfen während Bestätigung
und Warten bestehen, aber vor jeder Folgeschritt-Platzierung müssen sie fertig
sein. Neue unabhängige Baustellen stoppen laufende Projekte. Alle Treppenmodi
verwenden jetzt die Wartefolge. Vorschau bleibt unknown, constructionCovered=false.
Lager-/Treppenprojekt und Einzelplatzierung mit unabhängiger Baustelle abgelehnt;
Objektzahl unverändert, bestehender Builderzugang erhalten. Nach deren Fertigstellung
eigener Plattformablauf mit allen fünf fertigen Objekten und beiden verbundenen
oberen Wegen bestanden. B im freigegebenen Ausschlussumfang geschlossen.

Erster E-Schritt auf unveränderter 0.32.1 live bestanden: ebene kleine Lager
mit Rotation 0 und 2 als Baustelle builder-erreichbar und anschließend fertig,
freie Eingänge und Distriktdistanz jeweils 32. Gemeinsamer Bestandsweg verbunden;
Vergleichslager unverändert bei Distanz 19. Zusammen mit früheren C-Fällen sind
alle vier ebenen Lagerdrehungen begrenzt live belegt. E nicht insgesamt beendet;
der begrenzte 0.33.0-Vorlagen-/Drehungsausbau ist ebenfalls belegt, kein freier
3-D-Ausbau. [Nachweis](docs/building-rotation-pilot.md).

0.32.0 nach menschlichem Skript-Gate installiert und live geprüft: Etappe D
mit `stair_platform_warehouse_pilot`: Treppe, drei Plattformen, zwei obere Wege
und ein kleines Lager mit rückwärtsgerichtetem Eingang. Folgeschritte warten auf
fertige Vorgänger; vor dem Lager müssen beide oberen Wege distriktverbunden sein.
Erfasste ursprüngliche Distriktverbindungen werden während des Auftrags geprüft.
Dies ersetzt keinen vollständigen Bauphasen-Vorabnachweis (B bleibt offen).
Sieben Objekte fertig rückgelesen; Lagerbaustelle builder-erreichbar, fertiger
Eingang frei und native Distriktdistanz 22. Beide oberen Wege verbunden; unterer
Bestandsweg bleibt verbunden, Vergleichslager unverändert bei Distanz 11.
Identische Aktions-ID liefert alten Beleg und weiterhin genau sieben Objekte.
Vier Drehungen synthetisch geprüft, eine davon live; keine allgemeine 3-D-Planung.

0.30.0: `inspect_selection` über den öffentlichen `EntitySelectionService`
installiert und live geprüft. Distriktzentrale und Erfinderwerkstatt korrekt
erkannt; Auswahlwechsel liefert die neue ID, Vorlage und Rasterposition, jeweils
über `inspect_building` gegengeprüft. Aufgehobene Auswahl liefert `state=none`
und `target=null`. Keine Auswahl-/Kameraänderung, Sitzungspflicht.
`unsupported` und allgemeine Entity-/Weltpositionsfälle sind durch synthetische
Tests abgedeckt, aber nicht separat live belegt. Auswahl allein ist kein
Änderungsauftrag; vor späteren Aktionen frisch lesen und das Ziel prüfen.

| Ebene | Stand |
| --- | --- |
| Bridge | Agent Bridge 0.34.0 installiert; ebenes Wohnhausprojekt mit Bau-/Fertigzugang begrenzt live bestanden; allgemeiner Vorschau-Schutz offen |
| Automatisch | Menschliche Bereitmeldung nach Skript-Gate für 0.34.0; neue Testanzahl nicht übermittelt. Letzter Zahlenstand 0.29.3: 667 erfolgreich, 0 fehlgeschlagen, 3 übersprungen |
| Mod-Build | Timberborn 1.1.2.4 / Folktails, ohne Warnungen und Fehler |
| Laufzeit | Bridge und Schreibfreigabe strukturiert erreichbar |
| Bauprojekt | 0.32.0 live: Treppe, drei Plattformen, zwei obere Wege und kleines Lager; Bauphasen, Baustellen- und fertiger Zugang separat geprüft |

Arbeitsbranch: `codex/road-protection-pilot`. Die eigene Mod nutzt keine
Fremdmod-Pflichtbasis.

## Architektur

MCP über stdio → lokales C#-Backend → authentifiziertes Loopback-HTTP →
Hauptthread-Queue → öffentliche Timberborn-Spielservices. Strukturierte Werkzeuge
ersetzen UI-Automatisierung, Screenshotauswertung und Save-Manipulation.

## Implementierter Umfang

Zustands-, Güter-, Personal-, Bau-, Forschungs-, Flächen-, Entfernungs- und
Simulationswerkzeuge sind implementiert. Aktionen verwenden technische Freigaben,
frische Sessions, fachliche Fehlercodes und Rücklesungen. Bauaufträge bleiben an
Vorschau, Wegschutzdiagnose und schrittweise Bestätigung gebunden.

## Offene Arbeit

0.31.4 nach menschlichem Skript-Gate installiert und live geprüft. Drei Lageraufträge
in derselben Sitzung, zwei neue Wege; alte Belege nach neuen Aufträgen lesbar,
identische erste Anfrage liefert nur ihren ursprünglichen Beleg. Höchstzahl vier,
Fehlersperren und Sonderfälle synthetisch geprüft im menschlichen Gate, nicht
zusätzlich live ausgereizt. Tatsächliche Zugangszellen für drei parallele Baustellen
separat gelesen (acht/sechs/sechs); an fertigen Objekten not_construction.
Ein Plateauzugang erstmals real getrennt: Builder true→false, alle sechs Zugänge
auf gleicher Geländehöhe. Freie Vorschau im getrennten Zustand bestätigt negative
Baseline und Rücknahme. Regulärer Treppenwiederaufbau; danach drei Lager, zwei Wege
und Treppe fertig, drei Eingänge und Straßenverbindungen positiv. Ein begrenzter
Lauf: 14 Spielstunden, etwa 41,4 Echtzeitsekunden, Pause bestätigt, kein Überschuss.
Kontrollierte Bestandsverbindung verbunden, Distanz 11→11. C im begrenzten Umfang
bestanden. B-Untersuchung mit belegter API-Grenze abgeschlossen, B-Schutzabnahme
nicht bestanden: Engpassvorschau geometrisch ungültig, Bezirksverluste 30, aber
lostSites=0; reale Treppentrennung ist kein identischer Vorschau-Eingriff.
Keine allgemeine Vorschau-/Bauphasensicherheit daraus ableiten.

Aktiver Auftrag seit 2026-10-03: Etappen A–E aus missionsplan.md. Nutzer priorisiert
jetzt C samt begrenztem B-Abschluss; keine weitere Blockadesuchserie.
Ein vollständiger Gebäudeablauf mit bestehendem Anschluss ist live nachgewiesen.
Beide Kernziele sind teilweise,
nicht allgemein erreicht. `RoadProtection.constructionCovered=false` verhindert
allgemeine sichere Baufreigaben; Entwicklungspiloten erlauben ausschließlich die
ausgewiesene Vorabnachweislücke. Distriktweg-Zugehörigkeit ersetzt keine
Bauarbeiter-Erreichbarkeit. C-Pilot bestätigt Vorschau, Auftrag, Baustellenzugang,
tatsächliche Fertigstellung und fertigen Eingang samt Straßenverbindung.
Keine neuen Wege erforderlich; Nachbarverbindung erhalten, Distanz jedoch von
10 auf 11 gestiegen. Kein Nachweis unveränderter Wegqualität oder vollständigen
Bestandsschutzes. Neuer Anschlussweg im selben Ablauf inzwischen live bestanden.
Details: [Phase-C-Nachweis](docs/building-completion-pilot.md).

Der ungeprüfte Vier-Wege-Entwurf (vorgesehene 0.30.1) ist zurückgestellt, lokal als
benannter Git-Stash erhalten und nicht im aktiven Quellstand. Wiederaufnahme siehe
BACKLOG.md. Etappe A ändert nur die MCP-Auswertung vorhandener
Validierungsergebnisse, keine Baufreigabe. Aktuell installierte Bridge 0.31.4.

Etappe B: Bridge 0.31.0 installiert und begrenzt live geprüft nach menschlicher
Bereitmeldung. Road-Spill-Baseline passt zur tatsächlichen Builder-Abfrage;
freie Kontrolle und Vorschau-Rücknahme bestanden. Negative Baustellenkontrolle
nicht belegt: zwei Sperrvorschauen verlieren elf Distriktverbindungen, aber
keinen diagnostizierten Baustellenzugang. Kein Abschluss von B, keine Baufreigabe.
Testserie gestoppt; Nutzer hat die begrenzte Detaildiagnose anschließend freigegeben.
Eine echte Lagerbaustelle mit Anschlussweg ist als Testfixture angelegt.
Details und Stopkriterien: [Baustellendiagnose](docs/construction-access-preview.md).

0.31.1: Detailvergleich für eine Baustelle mit maximal acht Zugängen live geprüft.
Freie Vorschau und breite Sperrkontrolle zeigen getrennte Reichweiten-,
Verbindungs- und Belegungswerte; Baseline und Rücknahme bestanden. Alle acht
Verbindungswerte vom Zentralenursprung bereits vorher false bei realem
Builderzugang true: kein geeigneter Verbindungsnachweis. Sechs Zugänge bleiben
trotz Kandidatenbelegung im Reichweitenfeld. Nächster Schritt native Start-/
Zielgültigkeit klären, keine Suchserie. Etappe B und allgemeine Baufreigabe offen.

0.31.2 ist nach menschlicher Bereitmeldung installiert. Nutzer hat eine kleine
Lagerbaustelle bereitgestellt und markiert; Auswahl und realer Builderzugang
wurden strukturiert gelesen. Zentraleingang auf tatsächlichem NavMesh und
Bezirksweg, alle acht Ziele auf tatsächlichem NavMesh. Trotzdem connected und
roadConnected an allen acht Zielen in allen fünf Zuständen false, realer
Builderzugang vor/nach Vorschau true. Vier Ziele im Reichweitenfeld; Baseline
und Rücknahme bestanden, Pause/Spielzeit unverändert. Startpunktwechsel allein
löst die Lücke nicht. Positive Kontrolle gescheitert, keine Sperrvorschau,
kein Commit. Nach erneutem go öffentliche Signaturen geprüft: AreConnected
vermutlich direkte Kante, nicht ganze Route; noch kein abschließender Beweis.
Vorhandene Accessible-Pfadsuche Zentrale zu fertigem Lager live positiv.
Öffentliche tatsächliche Road-Spill-/Pfadsuche vorhanden, keine explizite
Preview-Pfadsuche gefunden. Vorschlag: begrenzte Nachbar-/Fernkontrolle und
tatsächliche Spill-Baseline, danach erst Preview-Engpass. Keine weitere Suchserie.
Etappe B bleibt offen; Details im Fachdokument.

0.31.3 nach menschlichem Skript-Gate live geprüft: drei positive Nachbarkanten,
drei echte Straßenpfade, vier erreichbare/Spill-Ziele bei acht negativen Fern-
Kantenwerten. Freie Vorschau und Ist-Builderzugang passen; Baseline/Rücknahme
bestanden. Genau eine ungültige Sperrvorschau deckt alle vier erreichbaren
Zugangspunkte ab und verliert sieben Bezirksverbindungen, aber keinen
diagnostizierten Baustellenzugang. Ist-Abfragen bleiben unverändert; tatsächlicher
Builderzugang danach true, Pause/Spielzeit unverändert. Diagnosescope bestanden,
keine negative Baustellenkontrolle und kein Abschluss B. Nächster Schritt:
fachlich gültige Blockadekontrolle statt weiterer überlappender Bezirkszentralen;
keine Suchserie. Allgemeine Baufreigabe bleibt gesperrt.

Zusätzliche gültige Einzelpunkt-Vorschau: kleines Lager belegt einen von vier
erreichbaren Zugangspunkten, übrige drei offen; kein Reichweiten-/Bezirksverlust,
Rücknahme und echter Builderzugang unverändert. Kein Negativfall. Diese Fixture
für weitere Blockadesuche gestoppt. Nächster Vorschlag nach erneutem go: isolierter
Einzelzugang mit tatsächlicher Negativkontrolle, dann passende Vorschau.

Erhöhte Nutzer-Fixture nach begrenztem Trägerbau praktisch getrennt: eine fertige
Treppe entfernt, oberer Wegzugang ohne Reichweite, aber vier niedrigere
Geländezugänge weiterhin erreichbar. Builderzugang bleibt true; keine negative
Baustellenkontrolle. Pilot gestoppt, kein automatischer Wiederaufbau. Lager bleibt
unfertig/aktiv, Spiel pausiert. Isolation muss gegen alle tatsächlichen Zugänge
und höheres Nachbargelände geprüft werden, nicht nur gegen die Treppe.

Nach erneutem go genau ein naher Wegpunkt entfernt: Bezirksanbindung des Asts
wechselt true→false, Builderzugang und vier niedrigere Spill-/Reichweitenpunkte
bleiben true. Baseline/Rücknahme der freien Diagnose passen. Negativpilot erneut
gestoppt, keine weitere Abrissserie. Entfernt bleiben Testtreppe und ein Wegpunkt.
Nächste Entscheidung: minimaler separater Testaufbau oder Herkunftsdiagnose der
Spill-Zugänge, statt weiterer Trennversuche im vernetzten Bestand. B bleibt offen.

Zusätzlicher Nutzerauftrag: `reasoning` für sämtliche MCP-Werkzeugaufrufe
verpflichtend (auch Fake/Legacy und Leser). Zentrale Schema-/Eingangsprüfung vor
Backend und Log implementiert; Negativtests und angepasste Integrationstests
im menschlichen Skript-Gate. Live bestanden: gueltige Begruendung akzeptiert,
fehlend/blank als InvalidParams ohne Logeintrag abgewiesen. Feldname und
Ingame-Logformat bleiben unverändert; reine MCP-Anforderung, keine neue Spiel-API.

Plattformpilot 0.29.2 abgeschlossen: Der begrenzte Wartezustand behandelt
Baustellen mit unverändertem Wegschutz und Bau nur bei Pause. Nach drei getrennten
begrenzten Bauphasen sind alle fünf Objekte fertig und der Auftrag abgeschlossen.
Die vertikale Distriktanbindung beider oberer Wege ist mit 0.29.3 direkt belegt;
vollständiger generischer Wegschutz bleibt offen. Details: [Plattformpilot](docs/vertical-platform-pilot.md).

0.29.3 live: `inspect_path_district` fragt die reale Hauptwegzelle gegen ein
konkretes Distriktnetz ab, ohne den Gebäude-Eingangsfilter. Beide oberen Testwege
verbunden; Lager und unfertige Treppe korrekt als unbekannt gemeldet.
[Nachweis](docs/path-district-observation.md).

Etappe A live bestanden am 2026-10-03: zusätzliche `assessment` in
Einzel-/Projektvalidierung, aus bereits geprüften nativen Belegen abgeleitet.
Sieben getrennte Befunde, Gesamtergebnis blocked/unknown; reguläre Baufreigabe
bleibt false. Keine neue Spielabfrage oder Modänderung. Tests für fehlende Basis,
Verluste, Vorschauzugang versus tatsächlichen Zugang und Antwortprüfung ergänzt.
Menschliche Bereitmeldung nach Skript-Gate. Freie Wegkontrolle, Sperrvorschau mit
zwei verlorenen oberen Wegen und gemeinsame Lager-/Wegvorschau korrekt gemeldet;
beide Wege nach Vorschau unabhängig wieder verbunden, Spielzeit unverändert.
Baustellen- und tatsächlich fertiger Zielzugang bleiben unknown. [Nachweis](docs/build-assessment.md).
Nächster Schritt Etappe B: öffentlicher Bauphasen-/Bauarbeiter-Vorabnachweis.

Details: [Fachverträge](docs/README.md), [Backlog](BACKLOG.md) und
[Entwicklungsablauf](DEVELOPMENT_WORKFLOW.md).
