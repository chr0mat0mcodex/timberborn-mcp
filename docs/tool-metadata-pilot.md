# A04-Pilot: Werkzeugmetadaten und Freigaben

Stand 2026-10-18: menschliches Gate und gezielter Livetest bestanden.
Bridge bleibt 0.74.0. Keine neue Abhängigkeit oder Spiel-API.

## Begrenzter Umfang

- `NativeToolOptions` benennt die elf bestehenden MCP-Opt-ins. Serverstart,
  Katalog und Dispatcher erhalten dieselben Optionen. Bisherige Bool-Aufrufe
  bleiben als Kompatibilitätsadapter erhalten; keine Änderung der Umgebungsvariablen.
- Nur `run_simulation_for`, `run_simulation_until`, `inspect_simulation_run` und
  `cancel_simulation_run` erhalten feste Deskriptoren für Namen, Route und Aktionsart.
  Daraus folgen Katalogsichtbarkeit, Mutationsannotation, Handlerzuordnung,
  Speed-Opt-in, Räumgruppenblockade und die bestehende Cancel-Sonderzulassung.
  Status bleibt lesend; Cancel bleibt eine Mutation mit Speed-Opt-in.
- Die übrigen Familien behalten ihre Registrierung. Ein privater Katalogindex wird
  einmal bei Typinitialisierung vor dem ersten Dispatcher angelegt. Räumgruppen-
  und Argumentfehlerpfad bauen keine Schemas mehr neu. Zurückgegebene Kataloge
  teilen keine veränderlichen Toolobjekte mit diesem Index.
- `BridgeHttpServer.MethodAllowed` bleibt unverändert und unabhängig konfiguriert.
  Ein MCP-Opt-in schaltet die Bridge nicht frei. Fehlerklassifikation, Sessionbindung,
  Betriebsdiagnose und native Parameter-/Quittungsprüfung bleiben bestehen.

## Bestandenes menschliches Gate

2174 Unit-Tests bestanden, einschließlich 28 neuer Fälle: vier feste Namens-/Routen-/Klassifikationsverträge mit HTTP-Matrix
(GET/POST/DELETE, Body, Opt-in), vier Schemafälle, drei deaktivierte Mutationen
mit null HTTP-Aufrufen, vier Transport-Routenfälle, unbekannte/fehlerhafte Aufrufe,
Katalogindex-/Räumgruppenklassifikation und elf einzelne benannte Opt-ins.
Bestehende `CancelDuringStatusWaitTests` prüfen weiterhin Konkurrenz, belegten
Dispatcher, deaktivierten Cancel und verlorene Cancelantworten; vorhandene
Räumgruppenfälle prüfen ihre eigenen Start-/Stop- und Besitzabläufe.

Quellvergleich: Schema-/Beschreibungserzeugung der Simulationsfamilie identisch;
zentraler Katalogkörper nach mechanischer Optionsumbenennung identisch.
Zusätzlich 13 Paket-, 28 Supervisor- und 13 Integrationstests bestanden; drei
optionale Livefälle übersprungen. Release-Builds ohne Warnungen/Fehler, fünf
installierte Paketdateien per SHA-256 bestätigt. Kein Agent-Build/Testlauf.

## Gezielter Livebeleg am 2026-10-18

1. Verbundener Supervisor gestartet: `running=true`, `gate=false`; native Bridge
   0.74.0 erreichbar, aktuelle Session bestätigt. Ausgangstempo 0, kein aktiver Lauf.
2. `for`-Lauf mit vier Spielstunden Obergrenze, Tempo 1 und 30 Echtzeitsekunden
   gestartet. Status zeigte `running/advancing`. Genau ein Cancel führte über
   `pausing` zu `cancelled/cancel_requested`, terminal und `pauseConfirmed=true`.
   Fortschritt 0,15625 Spielstunden; frische Tempoabfrage bestätigte 0 ohne
   aktiven Lauf oder ausstehenden Tempoauftrag.
3. `until`-Lauf bis zum frisch berechneten Ziel 0,25 Spielstunden später, Tempo 7,
   maximal 30 Echtzeitsekunden: `completed/target_reached`, kein Überschuss,
   terminal mit bestätigter Pause. Automatische Betriebsdiagnose wurde geliefert;
   ihre Befunde verhinderten den begrenzten Start nicht.
4. Frische Endabfrage: Tempo 0, kein aktiver Lauf oder ausstehender Tempoauftrag.
   Status mit absichtlich falscher Session: `stale_session`, nicht retrybar,
   `execution=not_applicable`. Keine Start- oder Cancelwiederholung.

Grenze: Opt-in-Kombinationen, HTTP-Methoden-/Bodymatrix, Räumgruppenklassifikation
und konkurrierender Status/Cancel sind synthetisch geprüft. Kein Live-Umbau der
Konfiguration oder Räumgruppenbau; keine allgemeine Bau-/Fraktionsabnahme.

## Nutzen und Aufwand

Der kleine Pilot ist sinnvoll: eine Metadatenquelle für vier Werkzeuge reduziert
getrennte Namens-, Routen- und Klassifikationspflege. Benannte Optionen machen
Freigaben am Servereinstieg prüfbar. Zwei Aufrufstellen nutzen den einmaligen Index
statt wiederholtem vollständigem Katalogaufbau; Schema- und Fehlerverträge bleiben
laut Quellvergleich, Gate und begrenztem Livetest erhalten.

Aufwand: zwei kleine Modelltypen, Anpassung von drei Produktionsdateien,
28 Regressionstestfälle und ein reguläres Gate mit begrenztem Livepilot.
Der Index kostet einmaligen Aufbau und Speicher für den vollständigen Katalog;
Bool-Kompatibilitätsadapter und übrige Familienregistrierungen bleiben als
Restarbeit bestehen. Keine Laufzeitbeschleunigung oder Startzeitverbesserung gemessen.
Nutzen/Aufwand ist für diesen abgegrenzten Schritt positiv. Gesamt-A04 bleibt offen;
weitere Werkzeugfamilien auf ausdrücklichen Nutzerwunsch vorerst zurückgestellt.
Auch A03 wird nicht erweitert.
Agenten-Spielanleitung geprüft: Werkzeuge, Parameter, Grenzen und Bedienablauf
bleiben unverändert; daher keine fachliche Anpassung. Keine projektbezogenen
Spielskills vorhanden.
