# Timberborn MCP

Lokaler C#-MCP-Server für lesenden Zugriff auf Timberborn. Umsetzung nach `missionsplan.md`.
MCP läuft über stdio; der Spielzugriff wird hinter einem eigenen Backend gekapselt.

## Entwicklungsstand

Fake-Backend und MCP-Grundlage implementiert. Reale API-Anbindung in Arbeit.
SDK: .NET 10.0.303. Abhängigkeiten sind zentral gepinnt; Lockfiles gehören ins Repository.
Keine Unity-/Mod-Binärdateien, Saves oder Laufzeitcaches einchecken.

## Projektlokaler Build

In einer PowerShell im Repository:

```powershell
. ./scripts/environment.ps1
dotnet restore TimberbornMcp.slnx --locked-mode --configfile NuGet.Config
dotnet build TimberbornMcp.slnx -c Release --no-restore
dotnet test TimberbornMcp.slnx -c Release --no-build --no-restore
```

`environment.ps1` setzt nur Variablen des aufrufenden Prozesses. Für die Entwicklung eine eigene
PowerShell verwenden; deren Schließen verwirft diese Variablen. Cache- und temporäre Dateien landen
unter `.local/`, Builddateien unter `bin/obj`.
