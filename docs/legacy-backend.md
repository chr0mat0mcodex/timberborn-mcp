# Legacy-Adapter und historischer POC

**Nicht der reguläre Einstieg für die eigene Agent Bridge.** Aktuelle Einrichtung:
[native Mod und MCP](native-bridge-install.md).

Der erste POC beobachtete Timberborn über More HTTP API und erprobte eine einzelne
Gebäudepause. Dieser Adapter sowie das ausdrücklich wählbare Fake-Backend bleiben
im Repository für Regression und Vergleich. Sie sind keine Laufzeitabhängigkeit
unserer nativen Mod und besitzen nicht denselben Werkzeugumfang.

Der damalige Mod-Stack nutzte More HTTP API, ModdableTimberborn, Mod Settings,
Harmony und TimberUi. Diese Fremdmods müssen für das aktuelle native Projekt nicht
installiert werden. Quelldateien werden nicht in unsere Mod kopiert oder eingebunden.

Historische Schalter: `TIMBERBORN_BACKEND=more-http-api`, `TIMBERBORN_BASE_URL`,
`TIMBERBORN_AUTHORIZATION`, `TIMBERBORN_ENABLE_WRITES` und `verify.ps1 -Live`.
Diese Auswahl benötigt einen separat eingerichteten Legacy-Mod-Stack. Die direkten
Server-Defaults wurden in der Dokumentationsbereinigung nicht geändert;
`start-native.ps1` wählt ausdrücklich das native Backend.

[Früher Live-Nachweis](testing/live-poc.md), [Phase-2A-Recherche](phase-2a-results.md),
[Referenzkatalog](references/README.md). Historische Testwerte und Beschränkungen
beschreiben ihren damaligen Zeitpunkt, nicht den aktuellen Entwicklungsstand.
