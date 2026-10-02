# Bauplatzsuche und Anschlussprüfung

Die Suche ermittelt innerhalb eines begrenzten Bereichs Kandidaten für ein
Bauvorhaben und mögliche Anschlüsse. Sie meldet Abdeckung, Ablehnungsgründe und
Prüfgrenzen; ein fehlender Kandidat ist keine Aussage über die gesamte Karte.

Ein Kandidat wird vor Ausführung erneut gegen den aktuellen Spielzustand geprüft.
Geometrie, Freischaltung, Eingang, Wegverbindung, Bauarbeiterzugang und möglicher
Verlust bestehender Zugänge bleiben getrennte Befunde. Unbekannte Pflichtprüfungen
blockieren die Ausführung.

Die Suche erzeugt keine Spieländerung. Bauprojekte verwenden anschließend den
[Ausführungsvertrag](building-project-execution-proposal.md).
