# A07-Pilot: MCP-Laufzeitkennung

Stand 2026-10-18: menschliches Gate und gezielter Livetest bestanden.
Bridge bleibt 0.74.0. Keine neue Abhängigkeit; keine weitere A03/A04/A05-Migration.

## Vertrag und begrenzter Umfang

Erfolgreiches natives `timberborn_status` ergänzt `data.mcp` in compact und full:

| Feld | Bedeutung |
| --- | --- |
| buildId | SHA-256 über die sortierten Namen/MVIDs der geladenen Module McpServer, Backend.Native und Bridge.Core. |
| catalogId | SHA-256 über den wirksamen nativen Backend-Katalog nach ResponsePresentation, einschließlich der aktuellen Opt-ins. |
| buildScope | `loaded-mvid-v1:McpServer,Backend.Native,Bridge.Core` |
| catalogScope | `native-backend-tools-v1:name,description,inputSchema,outputSchema,annotations` |
| clientCatalogState | `not_observed`: Der Clientcache ist dem Backend nicht bekannt. |

Build-/Katalogkennungen verwenden `sha256:` und 64 kleine Hexzeichen.
Die Bridge-Version, Session und Liveabnahme bleiben getrennte Angaben.
Identischer deterministischer Modulstand behält dieselbe Kennung; nicht jeder
erneute Buildvorgang erhält eine neue ID. Geänderte Modul-IDs unterscheiden sich
auch bei unveränderter Bridge-Version. Die Buildkennung ist keine Prüfsumme aller
Installationsdateien: Fremdbibliotheken, Runtime, Supervisor und Spielmod sind
nicht Teil dieses begrenzten MCP-Modulprofils. Die A06-Paketprüfung bleibt separat.

Katalogeinträge werden nach Namen und JSON-Objektfelder ordinal sortiert;
Arrayreihenfolge bleibt erhalten. Kennung umfasst Namen, Beschreibungen, Ein-/
Ausgabeschemas und Annotationen. Keine Argumentwerte, Tokens, lokalen Pfade,
Git-/Benutzerdaten, Zeiten oder Sessiondaten werden aufgenommen. Die einmalig
pro NativeTools-Instanz ermittelte Kennung beschreibt deren feste Optionen.

Der Supervisor verändert Beschreibungen zum Statuswarten und fügt sein eigenes
Lifecycle-Werkzeug hinzu. Diese Zusätze und sein konstanter Handshake bleiben
unverändert und sind nicht im Backend-Kataloghash enthalten. Ein anderer
Kataloghash als im Gate kann andere Opt-ins oder einen anderen Backendstand
anzeigen; er beweist keinen veralteten Clientcache. Ein fehlendes `mcp`-Feld
kennzeichnet einen alten oder nicht unterstützten Statusvertrag, keine sichere
Versionszuordnung. Bei fehlgeschlagener Bridgeabfrage bleibt der bisherige
Fehlervertrag bestehen; Offline-Statusidentität ist außerhalb dieses Piloten.

## Menschliches Gate und Zuordnung

`verify.ps1` ruft nach dem Release-Build denselben MCP-Binärstand mit
`--print-native-identity` auf. Dieser explizite Offline-Bericht gibt eine JSON-Zeile
aus und beendet sich ohne Hoststart, Konfigurationslesen, Spielaufruf oder Logdatei.
Er verwendet alle nativen Opt-ins wie der vorhandene Codex-Supervisor.
Das Gate prüft die Kennungsform und protokolliert `[MCP] Build ...; Katalog ...;
Profil native/all-enabled` im bestehenden lokalen Gate-Log. Kein zweiter Build,
keine eigene stdio-Verbindung und keine neue Installationskomponente.
Im normalen MCP-Betrieb bleibt stdout ausschließlich Protokollausgabe.

## Gate-Korrektur 2026-10-18

Erster menschlicher Lauf: Release-Build ohne Warnungen/Fehler und Offline-
Kennungsbericht erfolgreich. Unit-Lauf beim ersten Fehler abgebrochen:
765 bestanden, ein Fehler, übrige Fälle und Integration nicht abgeschlossen.
Der Schemaänderungstest verwendete unzulässig `type: string` als Eingabeschema;
das MCP-SDK wies es vor dem Hashvergleich ab. Fixture auf gültiges Objektschema
mit zusätzlichem Integerfeld korrigiert. Hashlogik unverändert. Nachbarfälle
quellseitig geprüft; erneuter menschlicher Gate-Nachweis und Livetest anschließend bestanden.

## Bestandene Nachweise

11 Unit-Fälle: Änderungen je geladenem Modul, fünf Arten öffentlicher
Katalogänderung, Reihenfolgeunabhängigkeit von Tools/Objektfeldern, Opt-ins und
Ausgabeaufbereitung sowie Statuszuordnung/-stabilität in compact/full.
Ein Integrationstest prüft den realen Offline-Bericht gegen die geladenen
Projektmodule und den vollständigen Supervisor-Optionskatalog; absichtlich
ungültige Backendumgebung ohne Konfiguration darf dabei nicht gelesen werden.
Moduländerungen bei gleicher Bridge sind synthetische Kontrollfälle; für diesen
Pilot werden keine zwei zusätzlichen Release-Builds erzeugt. Keine Tests/Builds
durch den Agenten ausgeführt.

Menschliches Gate: 13 Paket-, 28 Supervisor-, 2208 Unit- und 14 Integrationstests
bestanden; drei optionale Livefälle übersprungen. Release-Builds ohne Warnungen/
Fehler, fünf installierte Paketdateien per SHA-256 bestätigt. Alle elf neuen
Unit-Fälle und der neue Offline-Integrationstest bestanden.

Liveabgleich am 2026-10-18 bestanden:

1. Backend über verbundenen Supervisor gestartet: `running=true`, `gate=false`;
   native Bridge 0.74.0 erreichbar und aktuelle Session bestätigt.
2. `timberborn_status` in compact und full liefert exakt die Build- und
   Katalogkennung des erfolgreichen Gate-Berichts (Profil native/all-enabled).
   Beide Ausgaben stimmen überein; `clientCatalogState=not_observed`.
3. Nach lesender Simulationsabfrage bleiben beide Kennungen und Session gleich.
   Spiel pausiert, kein aktiver Lauf; keine Spielmutation für diesen Nachweis.

Vergleich erfolgte gegen die vollständigen SHA-256-Werte, nicht nur Kurzpräfixe.
Der reale Nachweis umfasst diesen Gate-/Backendstand; geänderte Modulstände und
abweichende Kataloge bei gleicher Bridge sind synthetisch geprüft. Keine Prüfung
oder automatische Aktualisierung des Clientcaches, kein Supervisor-Codewechsel.

## Nutzen und Aufwand

Der begrenzte Pilot ist sinnvoll: MCP-only-Änderungen lassen sich trotz gleicher
Bridge-Version mit dem Gatebeleg abgleichen. Geladene Module und wirksamer
Backend-Katalog sind getrennt erkennbar; Opt-in-Unterschiede ändern den Kataloghash.
Aufwand: ein Kennungshelfer, optionaler Statusdatensatz, Offline-Gatebericht sowie
elf Unit-Fälle und ein Integrationstest. Die erste Statusabfrage berechnet einmalig
den Kataloghash. Zusätzlich war ein Gate nach Korrektur einer ungültigen Testfixture
nötig; die Produktivlogik blieb dabei unverändert. Kein Laufzeitgewinn gemessen.

Grenzen bleiben bewusst sichtbar: keine vollständige Installationsattestierung,
keine automatische Clientcacheprüfung und keine Liveabnahme aus einem Hash.
Gesamt-A07 bleibt offen; weitere Erweiterungen auf ausdrücklichen Nutzerwunsch
vorerst zurückgestellt. Spielanleitung mit tatsächlichem Kennungsabgleich und
Nachweisgrenzen aktualisiert; keine projektbezogenen Spielskills vorhanden.
