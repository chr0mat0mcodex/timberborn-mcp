# Projektabschluss, Blockadediagnose und Bauzugangsreparatur

Stand 2026-10-11: menschliches Gate und begrenzter feature-spezifischer
Livetest bestanden. Gemeinsamer 0.37.0-Build.

## Werkzeugablauf

1. `diagnose_large_project(session, actionId)` liest frisch. Pro Teil werden
   erwartete Identität, Position, registrierter und beobachteter Zustand sowie
   konkrete Ursache getrennt ausgegeben. Eine unerreichbare Baustelle ist ein
   bestätigtes Objekt, kein unbestätigter Platzierungsversuch. Die normale
   Ausführung meldet nach ihrer Initialisierungsfrist `builders_unreachable`.
2. Bei beendetem Projekt und pausiertem Spiel `close_large_project` aufrufen.
   `closeOutcome=released` oder `already_released` bestätigt den Ledgerabschluss;
   `rejected` enthält den aktuellen Diagnosebericht. Bereits beauftragte Teile
   müssen fertig und zugänglich oder nach früherer bestätigter Beobachtung
   entfernt sein. Fehlend nach unbestätigtem Versuch, falsche Identität und
   unbekannter Zustand verhindern die Freigabe. Unbeauftragte Teile werden
   aufgegeben; vorhandene Objekte bleiben erhalten. Abschluss ist keine
   Fertigmeldung des ursprünglichen Gesamtplans.
3. Für fehlenden Bauzugang: laufendes Projekt zunächst stoppen und terminalen
   Zustand sowie Pause nachweisen. `plan_large_project_repair` mit `parentActionId`,
   Distrikt und zusätzlichen Wegen/Treppen/Plattformen verwenden. Bestehende
   Stützen nicht erneut beauftragen. Ein Reparaturplan enthält nur Bauhilfen,
   höchstens 32 Teile, 32×32 Grundfläche und acht belegte Höhen. Schrittindizes
   und Abhängigkeiten beziehen sich auf diesen neuen Plan.
4. `run_large_project_repair` mit gleicher Planung und `parentActionId`, frischem
   `planKey`, neuer `actionId` und festem Simulationsbudget starten. Die Mod baut
   die Hilfen autonom und wartet anschließend auf die bereits beauftragten
   Originalteile. Bestätigt entfernte Originalteile gelten als geklärt, nicht
   als fertig gebaut. Neue/unbestätigte Fehlwirkungen stoppen die Reparatur.
5. Nach Reparatur Originalprojekt erneut diagnostizieren und ausdrücklich
   schließen. Nicht erteilte Restteile frisch planen. Bauhilfen erst nach Prüfung
   der verbleibenden Zugänge gezielt abbauen; Straßen mitprüfen. Rückgewinnungs-
   stapel nicht löschen: Rohstoffe erhalten und regulär einsammeln lassen.

`inspect_large_project` bleibt Ledger-Lesung; `diagnose` ersetzt keine
Ausführung. Alte Auftrags-IDs bleiben terminal, ihre Platzierung wird niemals
wiederholt. Abgeschlossene Projekte zählen weiterhin zum Sitzungslimit 128.

## Diagnose und Grenzen

- Frische Bestandsverluste: Anzahl, bis zu 32 betroffene Objekte/Zellen und
  vorherige/aktuelle Verbindung; abgeschnittene Listen sind gekennzeichnet.
- Fremde Baustellen und aktive Ausführung getrennt ausweisen. Reparaturen dürfen
  ausschließlich die bestätigten Baustellen ihres gewählten Ursprungsprojekts
  als eigenen Bestand behandeln. Keine globale Abschaltung der Baustellenprüfung.
- Baupause, fehlende Baumaterialbereitschaft, aktuelle Baustellenbestände,
  gesamte Vorlagenkosten sowie globale freie/gesamte Güterbestände getrennt.
  Kein berechneter Restbedarf, keine erfundene Liefer- oder Personalursache.
- `lastGuardFailure` und `lastValidationFailure` bewahren die letzte beobachtete
  Prüfung; `parts`, `affected` und `foreignSites` zeigen den heutigen Zustand.
  Geänderte Umstände nicht nachträglich als ursprünglichen Verursacher ausgeben.
- Reparatur beginnt mit frischer gemeinsamer Vorschau und erhält die zu diesem
  Zeitpunkt vorhandenen Verbindungen. Frühere Verluste bleiben in der Diagnose
  des Ursprungsprojekts sichtbar; deren allgemeine Wiederherstellung wird nicht
  behauptet. Reguläre Bauprüfung und sofortige Einzelvorprüfung bleiben aktiv.
- Keine verschachtelten Reparaturen, keine fremden offenen Projekte umgehen.
  Fehlgeschlagene Reparatur erst klären/abschließen, bevor eine neue folgt.
- Keine automatische Routensuche, kein Umbau bestehender Teile, kein Abriss im
  Reparaturaufruf. Vorschau bleibt Entwicklungspilot, kein universeller
  Vorabnachweis des Bauarbeiterzugangs. Ursprungsplan wird nicht wiederaufgenommen.
- Abschluss löst die Sperre dieses Ledgers. Fremde Baustellen, andere ungeklärte
  Projekte und eine fehlerhafte Vorschau-Rücknahme können weiterhin blockieren.

## Gate und gezielter Livetest

Vorbereitete automatisierte Gegenproben: bestätigte Entfernung gegenüber
fehlender unbestätigter Mutation, Freigabe ohne Replay, unreparierbare Identität,
Schreib-/Tempo-Gates, Eltern-ID/Parameterbindung, Antwortverträge, HTTP-Methode,
Reparaturwartezeit samt ursprünglichem Zeitlimit und konkreter Zugangsblockade.

Nach menschlichem Skriptlauf und `live bereit`:

1. Kleiner begrenzter Bau mit absichtlich fehlendem Zugang: Diagnose muss das
   tatsächliche Objekt und `builders_unreachable` zeigen. Abschluss noch abweisen.
2. Kleine Treppen-/Wegreparatur planen und einmal starten. Bauhilfen und
   Originalbaustelle müssen ohne Advance pro Teil fertig werden; Pause bestätigt.
   Vorherige Nachbaranschlüsse nach echten Navigationsticks nachlesen.
3. Originalprojekt schließen, unmittelbar unabhängigen kleinen Bau beginnen,
   ohne Spielneuladen. Alte ID erneut lesen/abschließen: keine Doppelobjekte.
4. Negative Vorschau über einem belegten Weg und fremde Baustelle dürfen keine
   Reparaturfreigabe erzeugen. Bei fehlerhafter Gegenprobe nicht skalieren.
5. Testobjekte und temporäre Straßen entfernen und Abwesenheit lesen;
   Rückgewinnungsrohstoffe erhalten, keine Stapel mit `remove_debris` löschen.

## Live-Ergebnis 2026-10-11

- Dreiteiliger Höhenbau: zwei Wellen fertig, Turbinenbaustelle konkret als
  `builders_unreachable` diagnostiziert; Abschluss korrekt abgewiesen.
- Ein Reparaturlauf mit Treppe, zwei Plattformen und zwei Wegen: alle fünf
  Hilfen sowie die ursprüngliche Turbine nach 34,21 Sekunden / 11,59 Spielstunden
  fertig, Pause bestätigt. Keine erneute Platzierung der Originalteile und kein
  Advance pro Reparaturteil. Zwei benachbarte Arbeitsgebäude weiterhin zugänglich.
- Originalabschluss `released`, Wiederholung `already_released`. Neues Lager in
  derselben Sitzung beauftragt und fertig gebaut, ohne Spielneuladen.
- Gegenproben: belegte Wegzelle in der Vorschau abgewiesen; fremde Lagerbaustelle
  sperrt Reparatur eines anderen gestoppten Plans mit `state_conflict`.
  Alle vier Testledger anschließend freigegeben.
- Rückbau durch Agent und anschließend Spieler abgeschlossen. Frische vollständige
  Bereichsabfrage: nur die drei ursprünglichen Wegzellen, keine neun Testobjekte
  oder Rückgewinnungsstapel mehr vorhanden. Mindestens ein Rohstoffstapel wurde
  vor der Nutzerkorrektur vom Agenten gelöscht; daraus keinen Materialerhalt
  ableiten. Künftig Rückgewinnungsstapel beim Aufräumen ausdrücklich erhalten.

Begrenzter Funktionsnachweis, keine Liveabnahme aller 32 Teile/acht Ebenen,
unbestätigter Fehlwirkungen oder aller Reparaturgeometrien. Die Vorschau meldet
für bestehende Baustellen weiterhin `construction_access_limit`; der tatsächliche
Bauzugang wurde in diesem Pilot erst durch Fertigstellung nachgewiesen.
