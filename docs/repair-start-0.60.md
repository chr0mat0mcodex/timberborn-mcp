# Reparaturstart: Diagnose und Rückmeldung 0.60

## Befund am 16. Oktober 2026

Der in [0.59 protokollierte Fehler](agent-log-live-tests-0.59.md) bleibt ein
belegter Einzelfall: gültige Vorschau, danach `state_conflict`, kein neuer
Projektbeleg und kein neuer Weg nachweisbar. Die damaligen Logs enthalten keinen
Prüfzweig; eine konkrete Ursache lässt sich daraus nicht nachträglich beweisen.

Ein neuer Diagnosefall mit unveränderter installierter Bridge 0.59 wurde am
Westufer ausgeführt: Weg plus Lager, Budgetstopp nach einer Spielstunde mit
einer bestehenden Baustelle. Zwei identische Reparaturvorschauen für einen
zusätzlichen Weg lieferten denselben Planschlüssel. Ein einmaliger Start mit
diesem Schlüssel gelang. Reparaturabschluss nach rund 9,4 Spielstunden, Pause
bestätigt; anschließend Elternprojekt frisch mit 2/2 fertig/zugänglich freigegeben.
Kein Retry des alten unbestätigten Auftrags, kein Speichern des Spielstands.
Dies belegt einen funktionierenden Reparaturpfad in 0.59, nicht die Ursache oder
Behebung des früheren Einzelfalls und nicht die Wiederherstellung zerstörter Zugänge.

## Änderung für 0.60 — begrenzt live abgenommen

Vor Reservierung des Simulationslaufs, Projektregistrierung und erstem Bauauftrag
liefert der Chargenstart konkrete, fest erlaubte Fehlercodes statt des pauschalen
Konflikts. Plan-/Zustandsänderung, ungeeignete Vorschau, konkurrierende Projekte,
Simulationsbelegung, Kapazität, Kennungskollision und unvollständiger Wegvergleich
sind getrennt. Reparaturvorschauen unterscheiden außerdem Elternprojekt,
Distrikt, unzulässige Bauteile und verschachtelte Reparaturen.

Nur für einen direkten `run_large_project`/`run_large_project_repair` mit genau
einem Transportversuch und einer solchen bestätigten Vorabablehnung meldet MCP
`execution=not_executed`: **Dieser Start hat keinen neuen Bauauftrag erteilt.**
Das sagt nichts über frühere Aufträge derselben Kennung. Die Ergebniszeile nennt
den Grund verständlich, auch ingame und in beiden Dateilogs.
Generisches `state_conflict`, verlorene oder fehlerhafte Antworten und Fehler
nach dem Start bleiben `unconfirmed`. Keine automatische Wiederholung.

Bestehende Zugangs-, Identitäts-, Baustellen- und Budgetprüfungen bleiben erhalten.
Es gibt keine spekulative Lockerung als vermeintliche Reparatur des Einzelfalls.
`project_start_preflight_conflict` grenzt nur auf die native Vorprüfung ein;
deren einzelne Unterbedingung ist damit noch nicht bewiesen.

## Menschliches Gate und anschließender Livepilot

Quell- und Diffprüfung durch den Agenten; keine Builds oder automatischen Tests
vor dem menschlichen Gate. Regressionen vorbereitet: unveränderte Reparaturdaten
zwischen Vorschau/Start, geänderter Plan und fehlende Eignung, konkrete
Vorabablehnungen über den HTTP-/MCP-Pfad sowie generische, unbekannte, beschädigte
und verlorene Antworten ohne falschen Nichtausführungsnachweis.

Nach erfolgreichem Gate und `live bereit`:

1. Bridge 0.60 und neue Sitzung bestätigen. Kleines Elternprojekt am Budget stoppen.
2. Frischen Reparaturplan mit absichtlich abweichendem, formal gültigem Schlüssel
   einmal starten: `project_start_plan_changed`, `not_executed`, keine neue
   Baustelle, kein neuer Projektbeleg und weiterhin Pause erwarten.
3. Frisch planen, mit neuer Aktionskennung einmal korrekt starten. Helfer und
   ursprüngliche Baustelle fertig/zugänglich prüfen; Pause und Freigabe belegen.
4. Grundtext in beiden Dateilogs vergleichen und menschliche Ingame-Lesbarkeit
   bestätigen lassen. Bei erneutem Konflikt anhand des konkreten Codes eingrenzen,
   keine weiteren Startversuche oder automatische Vollketten-Wiederholung.

## Liveabnahme am 16. Oktober 2026

Nach menschlichem Gate installierte Bridge 0.60 und neue Sitzung bestätigt.
Vier alte fremde Treppenbaustellen blockierten zunächst die normale Vorschau;
im freigegebenen Testsystem entfernt und null verbleibende Baustellen gelesen.
Danach Lager am bestehenden Westuferweg vorgeprüft und mit einer Spielstunde
Budget gestartet: erwarteter Budgetstopp mit einer bestätigten Baustelle und Pause.

- Negativprobe mit falschem, formal gültigem Planschlüssel: exakt
  `project_start_plan_changed`, `execution=not_executed`, `retryable=false`.
  Kein Projektbeleg, kein Reparaturweg, Spielzeit unverändert und Tempo 0.
- Frische Reparaturvorschau und einmaliger gültiger Start mit eigener Kennung:
  nach rund 8,34 Spielstunden abgeschlossen und pausiert. Frische Diagnose
  bestätigt den fertigen/zugänglichen Weg, keine verlorenen Verbindungen.
  Elternlager frisch fertig/zugänglich und Elternprojekt freigegeben.
- Zwölf Ergebniszeilen zwischen MCP- und Bridge-Dateilog identisch;
  beide melden null Schreibverluste. Ablehnung als `rejected` statt `unconfirmed`.
  Mensch bestätigt Ablehnungs- und Reparaturabschlusszeile vollständig und verständlich lesbar.

Kein Speichern durch den Agenten. Abgenommen ist dieser begrenzte Start-/Ablehnungs-
und Abschlussfall, nicht jede neue Fehlerbedingung. Die ursprüngliche sporadische
Startablehnung gilt weiterhin nicht als ursächlich geklärt oder behoben.
