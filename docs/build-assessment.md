# Etappe A — gemeinsamer Bauprüfbericht

`validate_building`, `validate_build_site` und `validate_building_project` liefern
zusätzlich `assessment`. Das native MCP-Backend leitet diesen Bericht erst NACH
der bestehenden Antwortprüfung aus den Spielbelegen ab. Keine neue Mod-API,
kein zusätzlicher Spielaufruf, keine gelockerte Ausführung. Bridge bleibt 0.30.0.

`decision=blocked` bei einem nachgewiesenen Fehler, sonst `unknown` wegen offener
Pflichtnachweise. `regularExecutionAllowed=false` in dieser Etappe immer. Auch
eine hypothetische `roadProtection.status=safe` beweist noch keinen neuen Zugang.
`passed` bezeichnet nur den angegebenen Prüfumfang und die Phase, keine Vollabdeckung.

Sieben Befunde mit `name/status/phase/source/reason`:

- placement: geometrische Spielprüfung, bei Projekten einschließlich aller Wege.
- existingDistrictConnections: beobachtete Verluste im fertigen Vorschauzustand
  oder einem Wegpräfix; ohne Verluste nur bei vergleichbarer verbundener Basis
  als bestanden im Stichprobenumfang ausweisen.
- existingConstructionAccess: unknown; Distriktwegzugehörigkeit beweist keinen
  Schutz der Bauarbeiter-Erreichbarkeit vorhandener Baustellen.
- candidateEntrance: nur gemeinsame Projektvorschau kann Anschluss melden;
  Einzelvorschau hat dafür keinen Beleg. Noch keine reale Zugangsbeobachtung.
- newConstructionAccess: unknown, kein Vorabnachweis des neuen Baustellenzugangs.
- newFinishedAccess: unknown, geplantes Gebäude nicht fertig beobachtet.
- previewRestoration: vorhandener Vergleich von registrierten Objekten, Bestand
  und Navigation; keine Behauptung, sämtlicher Spielzustand sei identisch.

`affected/affectedTruncated` übernehmen die vorhandenen Verlustbelege. Ein
Wegpräfixverlust kann den Gesamtentscheid blockieren, ohne im abschließenden
affected-Feld aufzutauchen: dafür gibt es bisher nur prefixLoss-Zähler, keine IDs.
Die bisherigen Rohbefunde und ihre Grenzen bleiben unverändert verfügbar.

## Gate

Menschlicher Skriptlauf durch Bereitmeldung bestätigt; gezielter MCP-Livetest
am 2026-10-03 bestanden. Freie Wegvorschau: placement/Bestandsvergleich bestanden,
decision=unknown, keine reguläre Freigabe. Geometrisch ungültige Sperrvorschau:
decision=blocked, zwei verlorene obere Wegzellen und deren IDs im Bericht.
Gemeinsame Lager-/Wegvorschau: candidateEntrance=passed, tatsächlich neuer Bau-
und Fertigzugang weiterhin unknown. Alle Vorschauen entfernt und geprüfter
Zustand wiederhergestellt; beide betroffenen Wege unabhängig wieder verbunden,
Spielzeit unverändert pausiert. Kein Bauauftrag.

Vier Vorschauen insgesamt. Ein geometrisch vorgeprüfter Lagerkandidat wurde vom
Spielvalidator abgelehnt und korrekt blocked gemeldet: Kandidatensuche bleibt
keine Platzierungsgarantie. Für die freie Einzelkontrolle wurde die bereits in
der gemeinsamen Vorschau gültige Wegzelle benutzt, keine blinde Suchschleife.
552 Distriktvergleiche, 173 verbundene Ausgangspaare, 122 Wegprüfpunkte; keine
offenen Baustellen im Test. Keine Vollabdeckung von Bauphasen oder Builderzugang.

Etappe B ergänzt eine separate Baustellen-Reichweitendiagnose. Ein beobachteter
Verlust macht existingConstructionAccess=failed und den Entscheid blocked;
kein Verlust bleibt unknown, da keine vollständige Bauphasenabdeckung besteht.
Quelle, Grenzen und offenes Gate: [Baustellendiagnose](construction-access-preview.md).
