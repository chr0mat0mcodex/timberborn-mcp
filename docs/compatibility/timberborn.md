# Kompatibilität und Nachweise

Aktuelle Bridge-/Manifestversion: **0.74.0**. Jüngere MCP-Erweiterungen verwenden
weiterhin diese Bridge-API. Maßgeblicher Featurestand, letzte Gatezahlen und
Abnahmegrenzen: [Projektstand](../../PROJECT_STATE.md).
Die folgenden historischen Einzelpiloten sind keine pauschale Freigabe weiterer
Fraktionen, Karten oder Spielversionen. Offene Nachweise stehen im [Backlog](../../BACKLOG.md).

## A03-Pilot — zentrale Wire-Vertragsprofile

Stand 2026-10-18: menschliches Gate und gezielter Livetest bestanden. Bridge bleibt 0.74.0. Betroffen sind ausschließlich
[Envelope-Zulassung](../../src/Timberborn.Backend.Native/NativeClient.cs) und
[Validierungsbudget](../../src/Timberborn.Backend.Native/ValidationBudgetContract.cs).

[BridgeVersionProfiles](../../src/Timberborn.Backend.Native/BridgeVersionProfiles.cs)
ordnet jede der bisher zugelassenen 102 Versionszeichenketten genau einem
unveränderlichen Profil zu: 58 Legacyversionen, 44 mit erweitertem Budget ab
dem expliziten Eintrag 0.35.1. Keine numerische Bereichsfreigabe: unbekannte
Patch-/Zukunftsversionen liefern kein Profil und scheitern im Envelope.
Schema, Session, Zeitstempel und Nutzdaten bleiben unverändert verpflichtend.
Der eigenständige Budgethelfer behält für unbekannte Versionen seinen bisherigen
Legacy-Fallback; dieser ist keine Zulassung zum nativen Client.
Ein Profil beschreibt erwartete Transportverträge, keine Fraktions-, Geometrie-,
Ausführungs- oder Livefreigabe. Die getrennten spielseitigen Fähigkeitsberichte,
Straßen-/Bauzugangs- und Laufzeitverträge werden im Pilot nicht migriert.

Bestandener Gate-Nachweis: 102 unabhängig festgehaltene historische Versionen
über einen synthetischen HTTP-Reader und Budgetgrenzen 7/15/255, dazu unbekannte
Versionen (einschließlich 99.0.0) und fehlende/ungültige Envelopefelder: insgesamt
121 neue Fälle. Bestehende Validierungstests prüfen weiterhin, dass ein passender
Budgetzähler keine reguläre Baufreigabe erzeugt. Kein Agent-Test-/Buildlauf.

Gate insgesamt: 13 Paket-, 28 Supervisor-, 2146 Unit- und 13 Integrationstests
bestanden; drei optionale Livefälle übersprungen. Release-Builds ohne Fehler;
fünf installierte Paketdateien per SHA-256 bestätigt.

Gezielter Livepilot am 2026-10-18 nach menschlichem Gate und `live bereit` bestanden:

1. Verbundener Supervisor: `running=true`, `gate=false`; native Bridge 0.74.0
   erreichbar, aktuelle Session bestätigt. Karten- und Simulationsreader erfolgreich.
2. Frisch vorgeprüfte Wegvorschau: `gameValidated=true`, `valid=true`,
   `attemptsRemaining=999999`, `noPersistentChangeObserved=true`, Restaurierung
   bestätigt, keine verlorene geprüfte Verbindung. Zugangsschutz bleibt `unknown`
   und `regularExecutionAllowed=false`: Budgetannahme erweitert keine Baufreigabe.
   Kein Bauauftrag erzeugt.
3. Einmaliger Lauf über 0,25 Spielstunden bei Tempo 7, maximal 30 Echtzeitsekunden:
   `completed/target_reached`, kein Zeitüberschuss, `pauseConfirmed=true`.
   Frische Rücklesung: Tempo 0, kein aktiver Lauf und kein ausstehendes Tempo.
4. Lesende Laufabfrage mit absichtlich falscher Session: `stale_session`, nicht
   wiederholbar. Keine Wiederholung des Starts.

Grenze: Live ist der aktuelle Stand 0.74.0 geprüft. Historische Versionen und
fehlende/ungültige Envelopefelder sind synthetisch geprüft; keine zusätzliche
Geometrie-, Zugangs-, Fraktions- oder allgemeine Bauabnahme.

**Nutzen/Aufwand:** Der kleine Pilot ist sinnvoll. Zwei getrennte Produktionslisten
werden durch eine explizite Zuordnung ersetzt; 102 bisherige Versionsentscheidungen
und beide Budgetprofile bleiben erhalten. Produktionsumfang: eine kleine Profildatei
und zwei ersetzte Entscheidungsstellen, ohne neue Abhängigkeit oder Bridge-API.
Der wesentliche Zusatzaufwand liegt in der unabhängigen historischen Testmatrix
(121 Fälle) sowie einem regulären Gate und dem begrenzten Livepilot. Kein belegter
Laufzeitgewinn behauptet; der Nutzen ist weniger doppelte Versionspflege und
bessere Absicherung gegen versehentliche Vertragsänderungen.
Gesamt-A03 bleibt offen. Weitere Vertragsfamilien auf ausdrücklichen Nutzerwunsch
vorerst zurückgestellt; dieser Abschluss löst keine weitere Migration aus.

## Historischer Journalabschluss 0.74.0

Historischer Journalabschluss: **Bridge 0.74.0**, zusätzlich MCP-/Supervisor-Journalkorrekturen in `c09750f`; keine neue Bridge-API. Gate: 23 Supervisor-, 1931 Unit-, 13 Integrationstests bestanden, drei optionale Live-Integrationstests übersprungen.

Live zusätzlich bestätigt: unerreichbare Plattformbaustelle ohne Bestandsverluste, Forstbelege ohne erneute Mutation nach Backend-Neustart und Client-Neuverbindung, Supervisor-Lifecycle mit neu geladenem Skript. Unbekannte Objektidentität bleibt live offen; Windows-Dateifehler und unterbrochene Schreibaufträge nur synthetisch geprüft. [Vollständige Nachweise](../releases/0.74.0.md).

## Historische Versionsnachweise

0.74.0: Menschliches Gate und gezielter Livetest bestanden: Materialwarten, echter Bestandszugangsverlust und fertige Plattform bei historischem Zeitlimit korrekt unterschieden; kein allgemeiner Zugangs-/Betriebsnachweis.

0.73.0: Menschliches Gate und gezielter Livetest für Parameterdiagnose und frischen Fertigbaustand bestanden; Einzelplattform nach historischem Zeitlimit, keine allgemeine Betriebs-/Zugangsgarantie.

**0.72.0 abgeschlossen:** Gate, Laufzeitdiagnosen und vollständiger Plattform-Umwegpilot bestanden: 15/15 Teile fertig und zugänglich, Bestandswege erhalten, Journal freigegeben. [Livebelege](../releases/0.72.0.md).

**0.71.0: Gate und Umwegplatzierung bestanden, Fertigbau offen.** Oberer Treppenanschluss und seitliche Negativkontrolle bestätigt; automatischer Plattformumweg nativ gültig, 17/17 Teile platziert, keine verlorene Bestandsverbindung. [Belege und Learnings](../releases/0.71.0.md).

**0.70.0: Höhenkernpilot bestanden, Umweg offen:** native Vorschau registrierter Treppen-Zielwege, getrennte Anschlussbelege. Gate und bekannter Höhenbau bis Fertigzugang bestanden; erweiterter Umweg nativ abgelehnt. [Nachweisplan](../releases/0.70.0.md).

**0.69.0: Gate bestanden, Höhenpilot fehlgeschlagen:** native Weggraph-Prüfung für Bestandstreppen und automatisch geöffnetes MCP-Log. Beide Graphabfragen bleiben negativ, manuelle Gegenprobe gültig. Logfenster sichtbar und angehängt. [Nachweisplan](../releases/0.69.0.md).

0.68.0: Gate bestanden, Höhenpilot fehlgeschlagen, Diagnose live sichtbar:
[Suchpriorisierung und Anschlussdiagnose](../releases/0.68.0.md).

## Historische Einordnung 0.63 bis 0.67

0.67.0: Gate bestanden, Höhenpilot an Suchbudget gescheitert; keine Liveabnahme:
[Eigenkollisionen und vorhandene Treppen](../releases/0.67.0.md).

0.66.0: menschliches Gate und begrenzter Kernpilot bestanden; Uferpumpe bis
Fertigzugang sowie Tempoabweichung/konkurrierender Start mit gültigem Folgelauf:
[Uferpumpen und Startablehnungen](../releases/0.66.0.md).

0.65.0: Nach menschlicher Bereitmeldung begrenzter Kernpilot bestanden:
Wasserwarnungen, native Überflutung auch bei Pause und produktive Pumpenkontrolle.
Blockiertes Ansaugrohr und höherer Umweg bleiben synthetische Kontrollfälle.
[API, Regressionen und Livebelege](../releases/0.65.0.md).

0.64.0: menschliches Gate und begrenzter Livepilot am 17. Oktober 2026 bestanden.
Höhenbau bis Fertigzugang, Pause und Recovery mit Folgeauftrag ohne Reload.
[Konkrete Belege und Grenzen](../releases/0.64.0.md).

Vorheriger Bridge-Stand: 0.63.0. Menschliches Gate und begrenzte Liveabnahme
am 17. Oktober 2026 bestanden; [Nachweisgrenzen](../releases/0.63.0.md). Feature-spezifische Nachweise
stehen im [Projektstand](../../PROJECT_STATE.md) und den verlinkten Fachverträgen.
Die historischen Nachweise unten sind keine pauschale Abnahme neuer Fraktionen,
Karten oder Spielversionen.

## Versionsabschluss 0.37.0

Am 2026-10-11 abgeschlossen: [Releaseumfang und Nachweisgrenzen](../releases/0.37.0.md).
Die folgenden Einzelpiloten sind begrenzte Nachweise, keine pauschale Abnahme
aller Gebäudetypen oder Sonderstandorte.

## 0.37.0-Ergänzung autonome Charge — begrenzt live abgenommen

`run_large_project`: nach menschlichem Gate zwei vollständige Chargen mit fünf
und neun Teilen, automatische Folgeaufträge und Abschlusspause, negative Weg-
und Zeitlimitkontrolle bestanden. [Livetest](../autonomous-construction-batches.md).
Die Versionsnummer allein beweist diese Fähigkeit nicht. Frühere manuelle
Stützen-/Kraftpiloten gelten weiterhin nur für den damaligen Einzelschrittmodus.

## 0.37.0 — begrenzte Live-Nachweise (2026-10-10)

Nach menschlichen Gates und Bereitmeldungen: reale Wellen-/Lagerfolge und
fünfteiliger Höhenbau mit Treppe, zwei Plattformen, Weg und erhöhtem Lager fertig.
Aufgeschobene Stützenprüfung nur in der Planung, vollständige native Prüfung
vor jedem realen Auftrag. Träger-Negativkontrolle, Bestandsblockade, identischer
Start und Stopp vor Ausführung geprüft. Bau-/Fertigzugang sowie reale Kraftnetzdaten
separat beobachtet. [Belege und Grenzen](../large-projects-0.37.0.md).
Kein Livebau mit 32 Teilen/acht Ebenen und keine Abnahme sämtlicher Sondergebäude,
Fraktionen oder Kraftdrehungen. Der Release-Nachweis bleibt auf diese Fälle begrenzt.

## Screenshot-Zusatznachweis 0.36.0 (2026-10-10)

Entwickler-Gate und begrenzte Liveabnahme bestanden: zwei Spielbildauflösungen,
Kamerametadaten einschließlich Bildweite, falsche Session ohne Bild. Keine
zusätzliche Fraktions-/Plattformabnahme. [Details](../screenshot-0.36.0.md).

## Aktueller Zusatznachweis 0.35.5 (2026-10-10)

Menschliches Gate mit anschließender Bereitmeldung; native Bridge 0.35.5 live
bestätigt. Baumstumpf-/Ertragsunterscheidung, Sammelmarkierung/Replay, früher
Holzstopp und kompakte/terminale Chargenberichte bestanden. Gültige Flagge
unabhängig finished_accessible bestätigt. 77 gezielte Unit-Tests und sechs
Integrationsvarianten bestanden; MCP-/Testprojekte und Mod ohne Compilerwarnung.
Keine erneute allgemeine Katalog-, Fraktions- oder Spielversionsabnahme.
[Nachweis](../forestry-efficiency-0.35.5.md).

## Frühere Basis

Installiert und begrenzt live bestanden: 0.35.1 mit budgetgerechter ebener
Routensuche, generischem Gebäudeprojekt und dynamischem MCP-Katalogprofil.
[Nachweis](../road-budget-planner.md).

Vorheriger Schritt live bestanden: Bridge 0.33.1 mit sequenziellen
Vertikalprojekten. Versionszuordnungen im nativen Backend erweitert, übrige
Spiel-/Protokollbasis unverändert. [Nachweis](../vertical-sequential-pilot.md).
Die folgende Tabelle beschreibt den zuletzt verifizierten Stand.

| Bereich | Stand |
| --- | --- |
| Timberborn | 1.1.2.4 / Folktails |
| Bridge | Agent Bridge 0.35.1; Wegbudget-Grenzkontrolle, generischer ebener Bauablauf samt Bau-/Fertigzugang, Bestandskontrolle und Replay begrenzt live belegt; allgemeiner Vorschau-Bauphasenschutz nicht belegt |
| Transport | stdio-MCP und authentifiziertes Loopback-HTTP |
| Abhängigkeiten | keine Fremdmod-Pflichtbasis |
| Automatische Prüfung | Letzter übermittelter Zahlenstand 0.29.3: 667 erfolgreich, 3 übersprungen; Bereitmeldung für 0.35.1 ohne neue Zählung |

## Nachweisniveau

Die Bridge, die Lesewerkzeuge, strukturierte Aktionen und ausgewählte Interaktionen
sind jeweils für konkrete technische Fälle geprüft. Das ist keine Zusage für jede
Spielversion, Fraktion, Vorlage, Kartenform oder Modkombination.

Für neue Funktionen gilt: Build, synthetische Tests und ein begrenzter Livefall mit
strukturierter Rücklesung. Bauprojekte brauchen zusätzlich Vorschau, Sessionbindung,
Aktions-ID und Nachprüfung der tatsächlich erzeugten Objekte.

Historische Spielweltdaten und Testprotokolle werden nicht mehr geführt.
