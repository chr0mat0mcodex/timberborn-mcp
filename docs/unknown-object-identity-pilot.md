# A12/N03: unbekannte Objektidentität

Stand 2026-10-18: begrenzter Injektionspilot nach Gate live abgenommen.
2284 Unit-, 19 Integrations-, 13 Paket- und 35 Supervisor-Tests bestanden;
drei optionale Livefälle übersprungen. Builds ohne Warnungen/Fehler und fünf
installierte Paketdateien per SHA-256 bestätigt. Nativer Wiederanschluss samt
MCP-Build-/Katalogabgleich erfolgreich. Neues Werkzeug nach vollständigem
Codex-Neustart verfügbar; dieselbe Spielsitzung blieb erhalten. Kein neues Gate
für die Katalogübernahme. N03 ist im beschriebenen Injektionsumfang abgeschlossen.

## Befund

`LargeProjectRecovery.IdentityObservation` sucht die aufgezeichnete Entity-ID
und vergleicht Initialisierung, BlockObject, Vorschauzustand, Koordinaten,
Orientierung und Template mit dem Auftrag. Fehlendes Objekt ist `missing`;
ein vorhandenes abweichendes Objekt ist `identity_mismatch`.

Ein bestätigter Abriss ist kein unbekannter Identitätsfall:
`LargeProjectRecoveryPolicy.Resolved` lässt den Abschluss bei späterem
`missing` nach bestätigter Identität zu. Automatische Beobachtungen laufen
auch bei pausiertem Spiel. Ein Rennen zwischen Auftrag und Abriss liefert
keinen belastbaren Pilot. Die vorhandenen Policytests prüfen synthetische
Unknown-Fälle, ersetzen aber keine native Liveprüfung.

Native Durchsetzung: `Diagnose` berechnet beide Freigaben aus frischen
Beobachtungen; `Recover` verweigert die Freigabe, `PlanRepair` lehnt mit
`repair_parent_ineligible` ab. `RepairIdentitiesValid` prüft erneut vor
Reparaturausführung. Diese Pfade sollen im Pilot unverändert bleiben.

## Begrenzter Mechanismus

Freigegebener Entwicklungsmechanismus, standardmäßig deaktiviert:

- Genau eine aktuelle Session, ein nicht freigegebenes terminales Projekt
  und ein vorhandenes, zuvor passend beobachtetes Teil auswählen.
- Ausschließlich das Ergebnis seiner Identitätsbeobachtung auf
  `identity_mismatch` setzen; keine Entity-ID, Auftragsdaten, Spielobjekte,
  Saves oder Journale verändern. Kein frei wählbarer Rückgabewert.
- Aktive Injektion in jeder betroffenen Antwort ausdrücklich mit Herkunft,
  Zielteil und unverändertem realem Beobachtungsbefund kennzeichnen.
- Dieselbe Beobachtungsgrenze für Diagnose, Close und Repair verwenden;
  keine nachträgliche Manipulation fertiger JSON-Antworten.
- Aktivierung nur pausiert und ohne laufendes Projekt/Simulation; genau
  eine aktive Injektion. Explizites Ausschalten und Sessionwechsel entfernen
  sie. Keine Speicherung, keine automatische Wiederaktivierung.
- Injektion darf keine positive Freigabe erzeugen. Entwicklungsoption und
  Werkzeugverfügbarkeit, Sessionbindung und Zielbindung im Gate prüfen.

### Bedienung und Transport

`set_large_project_identity_pilot` verlangt `session`, `actionId`, `partIndex`
(0..31), `pilotAction` (`enable` oder `disable`) und ausdrücklich
`mode=development_pilot`. Die vorhandenen Bauaktionsschalter schalten das
Werkzeug und seinen POST-Endpunkt frei; es gibt keinen neuen globalen
Konfigurationsschalter. Erst `enable` aktiviert die sessionlokale Injektion.
Kein frei wählbarer Fehlerwert, keine Persistenz, keine zweite aktive Auswahl.
Andere Session, anderes Projekt oder anderer Teil treffen die Injektion nicht.
Ein aktiv gebundenes Ziel kann nicht stillschweigend ersetzt werden.

`identityInjection` nennt `source=development_identity_injection`, `injected`,
`active`, Session, Projekt, Teilindex, `injectedObservation=identity_mismatch`
und `realObserved`. Die Kennzeichnung bleibt in kompakten und vollständigen
Antworten erhalten; die Aktivitätsanzeige nennt ausdrücklich INJIZIERT.
Das ist eine native Antwort mit injiziertem Einzelbefund, keine Simulation
des gesamten Spiels. Nach `disable` sind `active` und `injected` false.

Die vorhandenen Recovery-Guards entscheiden weiter. Eine native
`repair_parent_ineligible`-Ablehnung im aktiven Pilot transportiert zusätzlich
die Injektionsbelege; MCP meldet `status=error`, `execution=not_executed`.
Close meldet wie bisher `closeOutcome=rejected`, ohne Journalfreigabe.
Während aktiver Injektion bestätigt Diagnose keine Identität ins Journal
zurück und schreibt keine Fortschrittsprobe. Auftrags- und Objektbeobachtung
außerhalb der Recovery-Grenze verwenden weiterhin den echten Spielzustand.

`disable` benötigt weder eine weiterhin passende reale Identität noch Pause;
es entfernt die eigene gebundene Injektion vor der Rückbeobachtung. Bei einer
fehlgeschlagenen Antwort Zustand lesend klären, nicht blind erneut aktivieren.
Spiel-Unload verwirft den Zustand ausdrücklich; MCP-Neustart allein nicht.

## Verwendete Abnahmefolge

1. Ein kleines reguläres Projekt terminal anhalten, dessen ein reales Teil
   frisch eindeutig bestätigt ist. Für eindeutige Kontrollen ein gestopptes
   manuelles Projekt verwenden, anschließend den echten Fertigbau abwarten:
   Ausgangsdiagnose mit closeEligible=true und repairEligible=true, Journal
   noch nicht freigeben. Objekt- und Ledgerstand vor Aktivierung festhalten.
2. Injektion für genau dieses Teil aktivieren. Diagnose muss
   `identity_mismatch`, `repairEligible=false`, `closeEligible=false` melden.
3. Close tatsächlich versuchen: `closeOutcome=rejected`, `released=false`;
   Journal bleibt gesperrt. Reparaturplanung tatsächlich versuchen:
   `repair_parent_ineligible`, kein neuer Auftrag und keine Platzierung.
4. Objekt- und Ledgerstand mit dem Ausgangswert vergleichen. Injektion
   ausschalten. Frische echte Identität und beide Ausgangsfreigaben wieder sichtbar;
   Projekt entsprechend seinem realen Zustand regulär abschließen.
5. Spiel pausiert, Injektion inaktiv; keine weiteren Fehlerklassen testen.

Stop bei fehlender Kennzeichnung, falscher Zielbindung, positiver Freigabe
während Injektion oder nicht nachweisbarer Deaktivierung. Keine Wiederholung
ganzer Testketten und keine Skalierung auf weitere Identitätsfälle.

Vorbereitete Regressionen: Aktivierungsbedingungen, exakte Zielbindung,
unverändertes Ledger bei abgelehnter Freigabe, anschließender regulärer
Abschluss, strikte Parameter und POST-/Bauaktionsschalter, ein Transportaufruf
ohne Retry, Ablehnungskennzeichnung, fehlerhafte Belege und Compact-Erhalt.
Die Tests liefen ausschließlich im menschlichen Gate. Native Spielservices
und die folgenden Recovery-Aufrufe wurden anschließend live geprüft.

Erster menschlicher Gatelauf: 2284 Unit-Tests bestanden, MCP-/Testkompilierung
ohne Warnungen/Fehler. Integration stoppte an der noch fehlenden neuen
Werkzeugkennung in der festen Katalogerwartung. Katalogliste, Schreibannotation
und Detailprüfung im Integrationstest ergänzt; keine Laufzeitänderung durch
diese Korrektur. Zweiter Gatelauf und anschließende Liveabnahme bestanden.

## Livebelege

- Eine Folktails-Plattform bei (29,32,3), Rotation 0, als Identitätsträger.
  Anfangskollision durch eine Karotte konkret gelesen; Pflanzmarkierung entfernt,
  regulär räumen lassen, nach zwei Spielstunden Zelle frei. Native Vorschau
  danach gültig/restauriert, keine verlorene Bestandsverbindung.
- Kosten sechs Bretter, 33 verfügbar (550 Prozent Deckung). Vorprüfung benennt
  blockierte Sägewerke; kein Produktionsnachweis behauptet. Einmaliger Auftrag,
  reale Baustelle buildersReachable=true. Projekt vor Fertigbau regulär gestoppt;
  nach vier Spielstunden echte Plattform fertig, Spiel wieder pausiert.
- Positive Ausgangskontrolle: recorded=construction als historischer Eintrag,
  observed=finished, closeEligible=true, repairEligible=true, released=false.
  Keine fremden Baustellen, keine Bestandsverluste. Vollständige Objekt- und
  Journalantworten als Vergleichsbasis im laufenden Test erfasst.
- Genau Teil 0 aktiviert: identityInjection mit source, injected=true,
  active=true, Zielbindung und realObserved=finished. Frische kompakte Diagnose
  observed=identity_mismatch, beide Freigaben false; Recovery-Empfehlung unknown.
- Tatsächlicher Close-Aufruf: closeOutcome=rejected, released=false.
  Tatsächliche Reparaturplanung: repair_parent_ineligible, status=error,
  execution=not_executed. Beide Antworten tragen die Injektionskennzeichnung.
  Kein Reparaturauftrag; kein Test weiterer Fehlerklassen.
- Während aktiver Injektion nach beiden Negativkontrollen erneut vollständige
  Objekt- und Journalantwort gelesen: jeweiliges data-Objekt exakt gleich zur
  Ausgangsbasis, einschließlich Entity-ID, Position, Zustand und Journalbelegung.
- Explizit deaktiviert: active=false, injected=false, realObserved=finished.
  Frische Diagnose wieder observed=finished und beide Freigaben true, ohne
  identityInjection. Danach regulärer Abschluss: released=true, ein Teil fertig
  und zugänglich, lostConnections=0. Das ist die absichtliche Journalfreigabe
  nach dem unveränderten Vergleich, keine Wirkung der Injektion.
- Endzustand: Tempo 0, kein aktiver Lauf oder ausstehender Tempoauftrag.
  Injektionsaktivierung und -deaktivierung im strukturierten Aktivitätsprotokoll
  ausdrücklich als INJIZIERT beziehungsweise deaktiviert bestätigt.

Kein natürlicher Identitätsfehler provoziert; direkte Reparaturausführung,
weitere Identitätszustände und Reload während aktiver Injektion nicht live getestet.
Sessionbindung/Unload sind durch Quellprüfung und Gatefälle begrenzt abgesichert.

## Nutzen, Aufwand und Nachweisgrenze

Nutzen bestätigt: deterministischer Nachweis der nativen Recovery-Sperren mit
positiver Vor-/Nachkontrolle und unverändertem Objekt/Journal. Aufwand mittel:
Entwicklungszugang durch Mod, Vertrag und MCP samt 28 neuen Unit-Testfällen,
Integrationserwartungen und Kennzeichnung. Zusätzlich ein korrigierter Gateversuch
und Client-Neustart; Liveaufbau auf eine Plattform und eine Räumzelle begrenzt.
Der Aufwand ist für die geschlossene Nachweisgrenze vertretbar. Kein Anlass,
daraus eine allgemeine Fault-Injection-Infrastruktur oder weitere Fälle abzuleiten.

Auch ein bestandener Pilot belegt einen **injizierten** nativen Sperrfall,
keinen natürlich entstandenen Identitätsfehler und nicht alle Zweige der
Objekterkennung. Weitere A12-Pakete sind nicht Teil dieses Auftrags.
