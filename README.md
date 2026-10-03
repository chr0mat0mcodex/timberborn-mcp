# Timberborn MCP

Eine eigene Timberborn-Mod und ein lokaler C#-MCP-Server ermöglichen strukturierte
Beobachtung und kontrollierte Eingriffe über reguläre Spielservices.

## Status

0.35.1: budgetgerechte Routensuche live bestanden. Fünf neue Wege werden bereits
bei der Kandidatensuche verworfen, vier akzeptiert; passende spätere Vorschläge
bleiben erhalten. Bau um ein Hindernis, Bau-/Fertigzugang und Bestandskontrolle
geprüft. [Nachweis](docs/road-budget-planner.md).

0.35.0 macht den ebenen Bauprojektpiloten generisch über den aktiven Spielkatalog,
volle Geometrie und anschließbaren Eingang. Bank sowie großes 3x3-Freiluftlager
mit Anschlusswegen, Bau-/Fertigzugang, vollständiger Grundrisskontrolle,
gefährdeter Bestandsverbindung, Höhenkontrolle und Replay nach menschlichem
Gate live geprüft. Derzeit ein öffentlich gemeldeter Wegzugang;
kein vollständiger Bauphasen-Vorabnachweis oder pauschaler Katalognachweis.
[Umfang und Nachweis](docs/generic-building-project.md).

Agent Bridge 0.34.0 ergänzt das ebene Folktails-Wohnhausprojekt. Gedrehter
Vier-Zellen-Grundriss, zwei neue Anschlusswege, tatsächlicher Bau-/Fertigzugang,
Grundriss-Ablehnung, Bestandsanschlüsse und Replay live geprüft.
[Nachweis und Grenzen](docs/lodge-project-pilot.md).

Agent Bridge 0.33.1 ermöglicht bis zu vier Vertikalprojekte nacheinander pro
Sitzung. Zwei Projekte ohne Neuladen fertiggebaut, Startsperren und Replay
live geprüft; Bau-/Fertigzugang und ausgewählte Bestandsanschlüsse erhalten.
Kein paralleler Bau oder allgemeiner Vorschau-Schutz.
[Nachweis](docs/vertical-sequential-pilot.md).

`inspect_building_capabilities` weist den versions-/fraktionsgebundenen
Projektumfang, technische Grenzen und historische Live-Nachweise strukturiert aus.
Lesend live geprüft, keine allgemeine Baufreigabe.
[Nachweis](docs/building-capabilities.md).

Agent Bridge 0.33.0 erweitert den ebenen Entwicklungspilot um das mittlere
Folktails-Lager. Gedrehter Sechs-Felder-Grundriss, erreichbare Baustelle, fertiger
freier Eingang und ausgewählte Bestandswege live geprüft; zusätzlich ein fertiger
Lagerablauf mit drei tatsächlich neuen Bodenwegen. Alle vier ebenen Drehungen
dieser Vorlage sind in begrenzten Livefällen belegt. Kein allgemeiner
Vorlagen-/Wegschutz. [Nachweis und Grenzen](docs/medium-warehouse-pilot.md).

Agent Bridge 0.32.1 schließt B im konservativen Minimalumfang: Bauanfragen bei
unabhängigen offenen Baustellen werden abgelehnt; eigene Folgeplatzierungen
benötigen fertige Vorgänger. Ablehnung ohne neue Objekte und erlaubter eigener
Ablauf live geprüft. Kein allgemeiner Builder-Vorabnachweis.
[Nachweis und Grenzen](docs/construction-isolation.md).

Erster E-Schritt auf 0.32.1: fehlende Drehungen des kleinen ebenen Lagers live
geprüft, mit erreichbarer Baustelle, freiem fertigem Eingang und erhaltenem
Bestandsweg. Weitere Vorlagen und Projektgrößen bleiben offen.
[Nachweis und Grenzen](docs/building-rotation-pilot.md).

Agent Bridge 0.32.0 ergänzt einen live belegten vertikalen Lagerablauf: neue
Treppe, drei Plattformen, zwei obere Wege und kleines Lager. Bauarbeiterzugang
und fertiger Lageranschluss wurden getrennt geprüft; ausgewählte Bestands-
anschlüsse blieben erhalten. Begrenzter Entwicklungspilot, kein vollständiger
Bauphasen-Vorabnachweis. [Nachweis und Grenzen](docs/vertical-warehouse-pilot.md).

Agent Bridge 0.29.3 ergänzt einen live belegten Plattformpilot: eine Treppe,
zwei Plattformen und zwei obere Wege. Der Ablauf wartet auf fertige Träger und
setzt erst bei Pause weiter. Alle fünf Objekte wurden fertig zurückgelesen;
die vertikale Distriktanbindung beider oberer Wege ist mit der direkten
Wegzellenabfrage ebenfalls live belegt.

Agent Bridge 0.30.0 ergänzt `inspect_selection` für die aktuelle UI-Objektauswahl.
Zwei Gebäude, der Auswahlwechsel und keine Auswahl sind live verifiziert.
Damit lässt sich „das markierte Objekt“ strukturiert zuordnen; Auswahl allein
erteilt keinen Änderungsauftrag. Details und Grenzen im [Projektstand](PROJECT_STATE.md).

## Umfang

Aktueller Schwerpunkt: Gebäude bauen, ohne bestehende Wege/Zugänge zu verlieren,
und den neuen Zugang während Bau und nach Fertigstellung nachweisen. Beides ist
teilweise, noch nicht allgemein gelöst. Der freigegebene [Etappenplan](missionsplan.md)
beginnt mit einheitlicher Bauprüfung und Baustellenzugang, nicht größerem Bauumfang.

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
