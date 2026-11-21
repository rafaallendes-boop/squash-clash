# Nota de golpe en la franja bajo la cancha (21 nov 2026)

Rama `claude/shot-note-band`, base `main` `939d419`. Dirigido por Claude (subagente); **la revisión no es independiente**. Sin fusionar ni instalar.

## Cambio
Pedido del PO (captura de la build DEV `fceb3de`): la nota («¡Al aire!», «Perfecto», …) aparecía sobre los jugadores. Causa: `ShotNoteView` la subía `barHeight + 55` y la barra DEV, cerrada, solo está `visibility: hidden`, así que su altura aún empujaba la nota hacia la cancha.

Ahora la nota se centra, en vertical, en la franja entre el borde inferior de la cancha en pantalla y el borde inferior seguro:
- Borde de la cancha: se proyectan con `Camera.main.WorldToScreenPoint` las dos esquinas cercanas del suelo (`(0,0,Length)` y `(Width,0,Length)`, dimensiones de `Config.Court`), se toma la más baja y se pasa a unidades del panel (`y / Screen.height * root.layout.height`). Sin constantes de pantalla, así que vale para otros tamaños de teléfono.
- Piso de la franja: `SafeAreaLayout.Bottom` más la altura de `dev-bar` solo si está visible (`display` no `None` y `visibility` visible). Con el panel DEV abierto la nota queda sobre la barra, como antes.
- Si la franja no alcanza (`< alto + 2·6`), cae a «justo sobre el piso» (6 unidades), sin tocar el interruptor DEV (esquina derecha, la nota es texto centrado).
- Sin cambios en palabras, tonos, animación, momentos, tamaño de fuente, cámara, `DefaultPreset.json`, física ni timing. No se agrandó la fuente.

Archivos: `Assets/SquashClash/Runtime/Shot/ShotNoteView.cs`, `Assets/SquashClash/Tests/PlayMode/ZUiLookTests.cs`.

## RED / GREEN
Dos pruebas nuevas en `ZUiLookTests` (suben la cámara con `camera.rect = (0,.3,1,.7)` para que exista franja en cualquier pantalla de prueba):
- `ShotNote_PanelClosed_SitsBelowTheCourt_InTheBand`: panel cerrado, la nota queda bajo el borde de la cancha, sobre el piso seguro y centrada en la franja.
- `ShotNote_PanelOpen_StaysAboveTheVisibleBar`: panel abierto, la nota queda sobre la barra (control, ya pasaba).

RED (código viejo + stub `CourtBottomEdge` = NaN): 1 de 2 falla por aserción (`Expected: greater than 20`); la de panel abierto pasa. GREEN: `ZUiLookTests` 10/10.

Ninguna aserción existente cambió; `ShotNote_SitsInsideTheScreen_AboveTheSafeBottom` y las de texto, tono y pop siguen igual y pasan.

## Puertas (sobre el código `77c1864`; el commit del informe solo agrega este documento)
EditMode 961/961, PlayMode 746/746, AutoQA V, T3 y volley PASS/0/nivel 1 (102/102 cada una), check-release dev/prod OK.

## Límites
- No hay captura: el cambio solo se verificó con pruebas de geometría; falta mirarlo en el iPhone.
- El borde se calcula con la cámara base (sin sacudida de render ni empujón de FOV), igual que el resto del HUD.
- En pantallas muy anchas donde la cancha llega al borde inferior, la nota usa la alternativa (sobre el piso seguro).
