# Spielbild über MCP — 0.36.0, live abgenommen

## Liveabnahme am 2026-10-10

Entwickler-Gate durch anschließende Bereitmeldung bestätigt; native Bridge 0.36.0
im geladenen Spiel erreichbar. Zwei JPEG-Aufnahmen als echte MCP-Bildblöcke
visuell geprüft: korrektes Timberborn-Spielbild inklusive UI, kein schwarzes Bild
und keine Aufnahme anderer Desktopfenster. Quelle 1920×1080, Ausgabe 1280×720
mit 122828 Bytes sowie 640×360 mit 41678 Bytes (rund 66 % weniger Bilddaten).
Kleine Aufnahme erhält die Übersicht, UI-Schrift ist entsprechend schlechter lesbar.
Kein allgemeiner Laufzeitbenchmark. Keine Aufnahme als Datei gespeichert.

Kameradaten bei beiden Aufnahmen identisch: Position (5,3705; 55,1911; 22,6170),
Höhe 55,1911 Welteinheiten, Blickvektor (0,3694; −0,9293; −0,0038), Zielabstand
59,3924. Bildwinkel horizontal 58,544°, vertikal 35°; Bildweite auf der Zielebene
66,5826×37,4527 Welteinheiten. Richtung normiert und auf Ziel ausgerichtet;
Breite/Höhe unabhängig aus Tiefe und Bildwinkeln nachgerechnet, konsistent.
Session, Aufnahmezeit und Pixelgrößen korrekt in separaten Metadaten.
Negativkontrolle mit falscher Sitzung: stale_session, kein Bildblock.

Damit Bildausgabe, Größenoptimierung, Metadaten und Sitzungsschutz live bestanden.
Fehlende Kamera, minimiertes Fenster, Übergröße und Unload wurden in diesem Pilot
nicht künstlich live ausgelöst; keine umfassende Plattform-/Renderabnahme behauptet.
Die folgenden Gate-/Vorbereitungsabschnitte dokumentieren den früheren Verlauf.

## Umfang

capture_screenshot(session, maxWidth=1280, maxHeight=720, reasoning) liefert
explizit einen JPEG-Bildblock plus kompakte Metadaten. Maximal 1600×900,
Seitenverhältnis erhalten, kein Hochskalieren, JPEG-Qualität 75, höchstens
768 KiB Bilddaten. Keine Doppelung des Base64-Bilds in Text/structuredContent.
Bildinhalt umfasst die Unity-Spielansicht einschließlich UI; dort sichtbare Namen
können enthalten sein. Keine Desktopaufnahme, Dateiablage oder Bildhistorie.

Öffentliche Unity-API: [CaptureScreenshotAsTexture](https://docs.unity3d.com/ScriptReference/ScreenCapture.CaptureScreenshotAsTexture.html)
am Frame-Ende; [EncodeToJPG](https://docs.unity3d.com/ScriptReference/ImageConversion.EncodeToJPG.html).
GPU-Verkleinerung vor JPEG-Encoding; Texturen werden im finally freigegeben,
RenderTexture.active wiederhergestellt. Keine private Reflection, Zusatzpakete
oder Kamerabewegung. CameraService.Transform/Target dienen als öffentliche
Timberborn-Kamerareferenz. Tatsächliche Kameraauflösung muss im Livetest bestätigt werden.

## Kamerametadaten

camera.coordinateSystem=unity_world_y_up, units=world_units. Position X/Y/Z,
height=Position.Y (absolute Welthöhe, nicht Abstand über Gelände), normierte
Blickrichtung forward, up und eulerDegrees beschreiben Lage und Orientierung.
target ist der aktuelle Timberborn-Kamerazielpunkt, targetDistance der Abstand.
viewPlaneDepth ist dessen Tiefe entlang der Blickrichtung.
viewWidth/viewHeight beschreiben die sichtbare Breite/Höhe auf der Ebene durch
diesen Zielpunkt, senkrecht zur Blickrichtung. Aus tatsächlichen Frustumecken
berechnet; keine Behauptung über die sichtbare Geländegrundfläche. Horizontaler
und vertikaler Bildwinkel in Grad; bei orthografischer Kamera null, dafür bleibt
die Bildweite angegeben. viewport beschreibt den normalisierten Kameraausschnitt.
sourceWidth/sourceHeight sowie width/height sind Pixelgrößen. Aufnahmezeit und
Sitzung sind separat enthalten. Metadaten und Pixel werden am selben Frame-Ende erfasst.

## Grenzen und Fehler

Nur geladene Spielsitzung mit passender Session und aktiver Spielkamera.
Eine Aufnahme gleichzeitig, mindestens zwei Sekunden zwischen Starts.
Maximal 3 Sekunden auf Frame warten; kein automatischer Retry bei minimierter
oder nicht gerenderter Ansicht. screenshot_busy, screenshot_unavailable und
screenshot_too_large sind explizite Ablehnungen. Authentifizierung und Session-
Prüfung bleiben erhalten. Nur die Screenshot-Route erhält ein größeres begrenztes
HTTP-Antwortbudget; übrige Routen bleiben bei 128 KiB.

Asynchrone Hauptthread-Warteschlange wartet auf den Frame, ohne den Spielthread
zu blockieren. Unload beendet Antwort und Aufnahme. JPEG-Signatur, SOF-Abmessungen,
Bytezahl, Ausgabegröße und Kamerametadaten werden im nativen Client geprüft.
Dies ersetzt keinen vollständigen Bilddecoder oder sichtbaren Rendernachweis.

## Entwickler-Test-Gate und erwarteter Livetest

Erster Gatelauf am 2026-10-10: MCP-/Test-Build bestanden, Unit-Tests wegen
veralteter Einstufung von 0.36.0 als unbekannte Version abgebrochen. Entsprechende
Negativfälle in BuildingCapabilityTests und GenericBuildingProjectTests auf
99.0.0 umgestellt; positive Profil-/Bauauftragsfälle für 0.36.0 ergänzt.
Korrektur quellgeprüft, Ausführung bleibt beim erneuten Entwickler-Gate.

Quellcode und synthetische Tests vorbereitet. Vor der Rückkehr zum ausschließlichen
Entwickler-Gate wurden Zwischenstände kompiliert (Mod und isolierter Debug-Server),
aber weder gestartet noch installiert. Der finale Stand ist nicht gebaut/getestet.
Keine weiteren Agent-Builds/Testläufe; keine Debug-Fassung für die Abnahme.

Der Entwickler führt das bekannte Vorbereitungsskript aus und meldet nach Laden
live bereit. Danach: genau eine Aufnahme mit Standardgröße, Bild sichtbar prüfen;
Position/Höhe/Richtung/Bildweite und Session/Zeitpunkt vorhanden/plausibel prüfen.
Zweite kleinere Aufnahme nach Abstand: Seitenverhältnis, Pixelgrenzen und kleinere
Antwort bestätigen. Veraltete Session muss ohne Bild abgewiesen werden. Bei
fehlender Kamera oder ungeladener Sitzung klarer Fehler statt fremder Aufnahme.
Bei schwarzem/falschem Bild oder unplausiblen Kameradaten stoppen und diagnostizieren.
Erst bei bestandenem Gate und Livetest Commit und Push.

Vorbereitete Tests: Größen/Seitenverhältnis, unbekannte Parameter, ungültige Kamera-
und JPEG-Abmessungen, Bild nur einmal im MCP-Ergebnis, asynchrone Queue mit Unload,
explizite Ablehnung, realer stdio-/HTTP-Vertrag mit synthetischem Bild und falscher
Session. Integrationstests behalten den einzigen Release-Serverpfad.
