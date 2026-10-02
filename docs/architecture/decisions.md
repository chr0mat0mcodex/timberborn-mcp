# Architekturentscheidungen

## Native Bridge statt Fremdmod-Abhängigkeit

Die eigene Agent Bridge ist die Laufzeitbasis. Der MCP-Server spricht sie über
authentifiziertes Loopback-HTTP an; Spielzugriffe laufen über die Hauptthread-Queue
und öffentliche Timberborn-Services. More HTTP API bleibt ein Legacy-Adapter.

## Strukturierte Werkzeuge statt UI-Automatisierung

Zustände und Aktionen werden über feste MCP-Werkzeuge modelliert. Screenshotauswertung,
simulierte Eingaben und Save-Manipulation sind keine Ersatzschnittstellen.

## Kontrollierte Aktionen

Schreiboperationen nutzen Sessionbindung, Erwartungswerte, Aktions-IDs, technische
Freigaben und Rücklesungen. Unbestätigte Ergebnisse werden diagnostiziert, nicht
automatisch wiederholt.

## Bauprojekte

Bauprojekte bleiben mehrstufig: Suche, Vorschau, schrittweise Ausführung und
Statusabgleich. Wegschutz und Bauarbeiterzugang sind eigene Nachweispflichten.
