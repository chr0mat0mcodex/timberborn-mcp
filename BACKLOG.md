# Backlog

## Priorität

1. Bauen kontrolliert auf weitere Vorlagen, längere Wege und größere Vorhaben
   erweitern.
2. Wegkonflikte, Produktionsblockaden sowie Hunger-/Durst- und Versorgungsrisiken
   als nachvollziehbare Diagnosebefunde zusammenführen.

## Technische Lücken

- **Aktuelle Ingame-Auswahl abfragen:** Die vom Nutzer angeklickte/ausgewählte
  Objektmarkierung strukturiert über MCP lesen, damit Aussagen wie „das markierte
  Objekt meine ich“ eindeutig einem Spielobjekt zugeordnet werden können.
  Objekt-ID, Vorlage/Objekttyp, Position und aktuelle Session liefern; keine oder
  nicht unterstützte Auswahl ausdrücklich melden. Gemeint ist die UI-Auswahl,
  nicht eine Abriss-, Pflanz- oder Flächenmarkierung. Über reguläre Spielservices
  auslesen, ohne Screenshotauswertung oder simulierte Eingaben. Vor einer späteren
  Aktion die Auswahl frisch lesen und das Ziel prüfen; Auswahl allein erteilt
  keinen Änderungsauftrag. Zukünftiges Feature, noch nicht implementiert.

- Vollständiger Wegschutz für Bauvorhaben.
- Diagnose der tatsächlichen Ursachen von Produktionsstillstand.
- Belastbarkeit auf unterschiedlichen Fraktionen, Karten und Spielversionen.
- Persistenz oder Wiederaufnahme von Bauprojekten nach Sessionwechseln.

Historische Baupläne, Kolonieziele und Spielstände sind bewusst kein Backlog mehr.
