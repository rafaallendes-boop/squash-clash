# Front 4 — placa «vs» con el estilo del rival (implementación)

23 nov 2026. Rama `feat/vs-plate`, base `origin/main` `2ccad73` (con #103 fusionada). Implementa la [spec](../superpowers/specs/2026-11-22-front-4-vs-plate.md) rev 2. **Revisión no independiente:** la escribió Claude (Sonnet 5.5) con `/frontend-design`; la revisión cruzada es de Codex, en solo lectura. **Estado: PR abierta tras el diagnóstico del defecto (ver «Defecto registrado»).**

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

## Puertas

Código de la placa idéntico en todas las corridas (`589d278`; `583f964` solo añade el informe). Las puertas de AutoQA y `check-release` y la tabla final están en la descripción de la PR.

| Puerta | SHA | Resultado |
|---|---|---|
| EditMode | `589d278` | 1004/1004 |
| PlayMode completo, corrida 1 | `589d278` | **842/843, 1 fallo** (ver «Defecto») |
| PlayMode completo, corrida 2 | `583f964` | 843/843 |

## Defecto registrado (no es verde limpio)

En la corrida 1 falló `ZPointEndFreezeSimulationTests.SimulationPauseAndNextPoint_AreIdentical_WithFreezeOnAndOff`: «the pause lasts the same number of steps», esperado 40, fue 39 (`ZPointEndFreezeTests.cs:150`). Se registró y se detuvo (AGENTS §2, spec §4). Con la autorización posterior del PO se diagnosticó así:

- Test aislado en `main` `2ccad73`: **pasa 3 de 3**. Aislado en esta rama: **pasa 3 de 3**.
- Suite completa de la rama otra vez: 843/843, sin ese fallo.
- Por lectura, el test no pasa por la placa (`rally.StartMode()` directo, bucle síncrono sin cuadros, sin `FirstRunFlow`).

**Conclusión honesta:** el fallo apareció una vez en la suite completa y no se reproduce ni aislado ni en la repetición. No falla igual en `main`, y tampoco quedó demostrado que lo cause la placa. La causa no está identificada (el Mac tenía carga 4–5 en esa corrida; es una hipótesis). No se tocó el test, los umbrales ni los criterios. Queda como observación abierta para la revisión de Codex y de SC-Lead.

## Textos (regla del 23 nov)

| Texto | Revisión del autor |
|---|---|
| «TÚ», «vs», «RIVAL» | OK: tuteo neutro; «vs» es el uso común en deportes |
| Estilo: «Fondista», «Definidor», «Completo» | OK: son los nombres que ya usa el HUD |

Pregunta abierta de producto (no decidida, se usa el texto de la spec): la **bajada por estilo** («Juega al fondo», «Busca definir», «De todo un poco») sigue pendiente; la placa no la lleva.

## Pendiente

La placa no se ha visto en una captura ni en el iPhone: la comprobación en dispositivo va en la ventana que SC-PM coordine con Rafael (AGENTS §8). La bajada por estilo queda fuera, como dice la spec.
