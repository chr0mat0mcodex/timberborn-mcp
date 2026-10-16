# Verständliche Ergebniszeilen — 0.58.0

Status 2026-10-16: nach menschlicher Bereitmeldung Bridge 0.58.0 bestätigt.
Fachliche Liveproben bestanden; Mensch bestätigt alle Ergebniszeilen vollständig und gut lesbar.
0.58.0 ist damit im beschriebenen Umfang begrenzt live abgenommen.

Livebelege: Gebäudepause geändert und separat zurückgelesen; Ergebnis unterscheidet
Änderung und gelesenen Zustand. Ein Simulationslauf über genau eine Spielstunde
mit bestätigter Pause; Startzeile bleibt offen, Abschlusszeile nennt Zeit/Pause.
Pflanzkontrolle mit vier Zellen: eine Markierung gesetzt, zwei bereits vorhanden,
eine Wegzelle ausgeschlossen. Vorschau, Apply und gespeicherter Statusbeleg korrekt
getrennt. Ergebnistexte in MCP- und Bridge-Datei stimmen je Activity-Aufruf überein,
keine gemeldeten Schreibverluste. Fehlende/teilweise/unbestätigte Ergebnisse und
andere Einstellungsarten bleiben synthetische Gate-Nachweise.

## Umfang

Eigenes Feld `outcomeSummary` im Agentenlog und in beiden Dateilogs. Ingame erscheint
es als „Ergebnis:“ unter dem Aufrufstatus. Absicht und Parameter bleiben getrennt.
Der Text wird aus dem validierten Fachbericht vor Compact-Darstellung erzeugt;
keine zusätzlichen Spielabfragen, keine aus Requestparametern erfundenen Effekte.
Keine Kennungen oder ungeprüften Fehlermeldungen in den Ergebnistexten.

- Gebäudeeinstellungen: `set_building_paused`, `set_storage_good`, `set_storage_mode`,
  `set_farm_priority`, `set_farm_crop`, `set_building_recipe` unterscheiden geändert,
  bereits passend und unbestätigt anhand beobachtetem/angefragtem/vorherigem Wert.
  `inspect_building_settings` fasst den gelesenen Pausenzustand zusammen.
  Freigabe/Einstellung ist kein Betriebs- oder Produktionsnachweis.
- Simulationsläufe: `run_simulation_for`, `run_simulation_until`,
  `inspect_simulation_run`, `cancel_simulation_run` unterscheiden laufend, beendet,
  abgebrochen, unterbrochen und fehlerhaft. Verstrichene Spielstunden und bestätigte
  Pause werden getrennt berichtet. Zeitlimit, Messzeitraum und tatsächlicher
  Bestands-/Forschungs-/Produktionsnachweis bleiben verschiedene Aussagen.
- Pflanzchargen: `preview_planting_batch`, `apply_planting_batch`,
  `inspect_planting_batch` zeigen geeignete, gesetzte, bereits vorhandene,
  ausgeschlossene, unbestätigte und nicht versuchte Zellen anhand der Fachsummen.
  Vorschau setzt keine Markierungen. Statusbelege sind historisch; auch eine
  idempotente Apply-Antwort bleibt ein Beleg, keine Behauptung erneuter Ausführung.
  Markierungen belegen kein Wachstum und keinen Ertrag.
- Fehler mit `not_executed` bestätigen Nichtausführung; `unconfirmed` verlangt
  lesende Klärung ohne Wiederholung. Andere Fehler erhalten keinen Wirkungsnachweis.

Nicht enthalten: Sammelkonfiguration, beliebige Flächenaktionen oder weitere
Auftragsfamilien. Keine selbstständige Aktualisierung alter Logzeilen beim Ende
eines Hintergrundlaufs: der Startbeleg beschreibt seinen damaligen Stand, eine
spätere Statusabfrage liefert einen neuen Eintrag mit dem aktuellen Ergebnis.

## Transport und Verträglichkeit

Optionaler Base64-Header `X-Agent-Outcome`, höchstens 400 bereinigte Zeichen.
Ergebnistext ist nur für Abschlussmeldungen erlaubt, nicht für `running`.
Query-Einschleusung wird wie bei Begründungen abgewehrt. Ein Abschluss aktualisiert
nur den noch laufenden passenden Eintrag; Sequenz/Startzeit/Absicht bleiben erhalten.
Gelöschte/verdrängte Einträge werden nicht wieder eingefügt, doppelte Abschlüsse
nicht übernommen. Ältere Einträge ohne Ergebnisfeld haben eine leere Ergebniszeile.
Ältere Bridges ignorieren den neuen Header; die Anzeige benötigt 0.58.0.
Telemetrie bleibt Best Effort: ein Ausfall darf keinen Spielauftrag wiederholen.

## Gate-Nachweise und durchgeführter Livepilot

Im menschlichen Gate: Tabellenfälle für die sechs Gebäudeeinstellungen, unverändert und
unbestätigt; Simulationszustände, Pause und getrennte Ziele; Pflanzvorschau,
Teilauftrag, vollständiger Beleg und historische Statusabfrage; fehlende Daten;
Headertransport/Readback, Dateilog, Pufferabschluss, Löschen, Längenlimit und
Tokenredaktion. Versionsregressionen enthalten zusätzlich 0.58.0. Nur Quelltext-
und Diff-Prüfung durch den Agenten; keine Build-/Testläufe vor dem menschlichen Gate.

Durchgeführter Pilot nach erfolgreichem Skript und `live bereit` (Ergebnisse oben):

1. Bridge 0.58.0 und aktuelle Session bestätigen. Eine Gebäudepause ändern und
   separat lesen: Änderungsbeleg versus gelesener Zustand, auch im Dateilog.
2. Kurzen begrenzten Simulationslauf starten und Abschluss lesen: Start bleibt
   offen, Abschluss nennt tatsächlich beobachtete Zeit und Pause. Kein mehrfacher Start.
3. Kleine Pflanzvorschau mit passenden Kontrollzellen, einmalige Ausführung und
   Statusbeleg: Zählsummen stimmen überein, keine Wachstumsbehauptung. Bei fehlendem
   passenden Standort zuerst diagnostizieren statt Testkolonie umfangreich umbauen.
4. Mensch bestätigt lesbare Ergebniszeilen im Fenster. Kein Erfolg bei bloßem
   Paketbau behaupten. Beim ersten fehlerhaften Kontrollfall Diagnose statt Wiederholung.

Erst nach bestandenem Gate und diesen begrenzten Livebelegen committen/pushen.
