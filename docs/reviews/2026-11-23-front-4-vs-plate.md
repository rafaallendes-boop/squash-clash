# Front 4 — placa «vs» con el estilo del rival (implementación)

23 nov 2026. Rama `feat/vs-plate`, base `origin/main` `2ccad73` (con #103 fusionada). Implementa la [spec](../superpowers/specs/2026-11-22-front-4-vs-plate.md) rev 2. **Revisión no independiente:** la escribió Claude (Sonnet 5.5) con `/frontend-design`; la revisión cruzada es de Codex, en solo lectura. **Estado: detenido por un defecto registrado (ver «Defecto»). No hay PR abierta.**

## Alcance entregado

- `VsPlateView` (`Runtime/Rally/VsPlate.cs`): capa `vs-plate-layer` (`PickingMode.Position`, bajo el encabezado) y placa `vs-plate` con dos mitades de vidrio, línea roja central y textos «TÚ», «vs», «RIVAL» y el estilo (`RallyText.VsStyle`). Reloj `VsPlate.Now` real y sin escala; la edad es `Now() − shownAt`.
- Cierre por `tapped`, `timeout`, `phase` o `pause`, con una sola línea `menu` `screen="vs"` por apertura y otra por cierre. Se muestra solo desde `FirstRunFlow.StartMatch` (`hud.ShowVsPlate()`), nunca en la revancha, el «Partido» DEV ni la práctica. Interruptor DEV `RallyHud.VsPlateOn` (encendido, no se recuerda).
- `GesturePreview.OverUi` reconoce `vs-plate-layer`. El toque se detecta por la ruta del adaptador (`SampleFiltered`) y por `PointerDownEvent`. Una mutación (quitar el nombre de `OverUi`) hace fallar los dos tests del filtro.
- Constantes y texto en `Core` (`RallyText.VsInSeconds` 0,18 · `VsHoldSeconds` 1,10 · `VsOutSeconds` 0,20; suma 1,48 s). Estilos en `Hud.uss` con la escala existente (13/20/34). Sin cambios de física, preset, SimulationKey ni RallyParams: hashes iguales.

## Tests y RED

- Nuevos: EditMode `RallyTextTests` (+4 casos), PlayMode `ZVsPlateTests` (16), `ZDeviceLayoutTests.VsPlate_…` (12 casos: 4 teléfonos × 3 estilos) y `ZUiLookTests.VsPlate_…` (1). Ningún test existente se editó.
- **RED válido por aserción** con un esqueleto de firmas sin comportamiento: EditMode 4 fallos; PlayMode 16 + 13 fallos («missing element vs-plate…», `ShownLines` 0). Una primera corrida PlayMode tuvo cinco errores de mi arnés (doble `Shutdown` entre arranques); lo corregí y repetí el RED completo antes del GREEN.
- Ajustes de mis tests nuevos tras el primer GREEN, sin tocar criterios: esperar un cuadro para que UI Toolkit resuelva el layout, y el toque de «dedo nuevo» sobre el frontis (fuera del frontis el modelo lo rechaza al instante).
- Corrección de diseño por el test de layout: «Definidor» y «Completo» a 34 px no cabían con los márgenes por defecto de `Label`; se pusieron márgenes y relleno en 0 y relleno lateral 10/4. La spec no cambia (34 px).

## Puertas (sobre `589d278`, árbol limpio)

| Puerta | Resultado |
|---|---|
| EditMode | 1004/1004 |
| PlayMode completo | **842/843, 1 fallo** (`ZPointEndFreezeSimulationTests.SimulationPauseAndNextPoint_AreIdentical_WithFreezeOnAndOff`) |
| AutoQA `mvp` | **no se corrió** (detenido) |
| `check-release` | **no se corrió** (detenido) |

## Defecto

`ZPointEndFreezeSimulationTests.SimulationPauseAndNextPoint_AreIdentical_WithFreezeOnAndOff`: «the pause lasts the same number of steps», esperado 40, fue 39 (`ZPointEndFreezeTests.cs:150`). Según AGENTS §2 y spec §4 se registra y se detiene: no se reintentó, no se apagó `VsPlateOn`, no se tocó el test.

Lo que sé, y lo que no:
- El test arranca `rally.StartMode()` directamente, con un bucle síncrono sin cuadros: no pasa por `FirstRunFlow`, `ShowVsPlate` ni la capa. Por lectura, la placa no puede intervenir. La spec también la deja fuera de ese camino.
- En `main` `c4c755d` (informe de front 2) PlayMode dio 812/812, así que el test pasaba entonces. No sé si falla de forma intermitente: el Mac tenía carga 4–5 durante la corrida. **No lo he medido**; sería una hipótesis, no un hecho.
- Falta una corrida aislada del test sobre `main` y sobre esta rama para separar «ya fallaba» de «lo causa algo mío». No la hice por la regla de detenerse.

## Textos (regla del 23 nov)

| Texto | Revisión del autor |
|---|---|
| «TÚ», «vs», «RIVAL» | OK: tuteo neutro; «vs» es el uso común en deportes |
| Estilo: «Fondista», «Definidor», «Completo» | OK: son los nombres que ya usa el HUD |

Pregunta abierta de producto (no decidida, se usa el texto de la spec): la **bajada por estilo** («Juega al fondo», «Busca definir», «De todo un poco») sigue pendiente; la placa no la lleva.

## Pendiente

Decisión sobre el defecto (¿corrida aislada de diagnóstico?). Si se resuelve: corrida completa de PlayMode, `scripts/autoqa.sh mvp` (obligatorio: se toca `OverUi`) y `check-release`, todo sobre un solo SHA, y abrir la PR. La placa no se ha visto en el iPhone ni en una captura de pantalla.
