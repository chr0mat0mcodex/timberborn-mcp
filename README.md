# Timberborn MCP

Eine eigene Timberborn-Mod und ein lokaler C#-MCP-Server ermöglichen strukturierte
Beobachtung und kontrollierte Eingriffe über reguläre Spielservices.

## Status

Agent Bridge 0.25.0 ist gebaut, installiert und über MCP erreichbar. Die regulären
automatischen Prüfungen bestehen. Die aktuelle Bauprojektausführung benötigt noch
einen vollständigen Live-Nachweis.

## Umfang

- Zustands-, Güter-, Personal-, Bau-, Forschungs- und Flächenabfragen
- Produktions-, Zugangs- und Versorgungsdiagnosen
- Kontrollierte Aktionen für Bau, Betrieb, Forschung, Entfernung und Simulation
- Sessionbindung, Aktions-IDs, Rücklesungen und fachliche Fehlercodes

MCP nutzt stdio. Die eigene Mod kommuniziert ausschließlich über authentifiziertes
Loopback-HTTP und die Hauptthread-Queue. Es gibt keine Pflichtabhängigkeit zu
Fremdmods und keine Screenshot- oder Eingabesteuerung.

## Einstieg

```powershell
pwsh -NoProfile -File ./scripts/verify.ps1
pwsh -NoProfile -File ./scripts/build-native-bridge.ps1 -TimberbornManagedDir '<Spielverzeichnis>/Timberborn_Data/Managed'
```

Weitere Informationen: [Einstieg](START_HERE.md), [Projektstand](PROJECT_STATE.md),
[Installation](docs/native-bridge-install.md), [Werkzeuge](docs/tools.md) und
[Entwicklungsablauf](DEVELOPMENT_WORKFLOW.md).
