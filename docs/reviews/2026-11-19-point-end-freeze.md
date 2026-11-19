# Congelar fin de punto (solo render) — 19 nov 2026

Hecho por un subagente Sonnet dirigido por Claude; **la revisión de Claude no es independiente**. Rama `claude/point-end-freeze`, base `7e8c64c`. Sin fusionar ni instalar.

## Encargo del PO
«Al cerrarse el punto, congela solo el render 100 ms dentro de la pausa entre puntos que ya existe, sin alargarla ni tocar la simulación ni el reloj. Con interruptor DEV.»

## Qué cambió
- `PointEndFreeze` (nuevo, `Runtime/Visual`): estado estático con `Enabled` (por defecto sí), `Active`, `Duration` y una fuente de tiempo reemplazable por tests. `Begin(pausa, sigueEnPausa)` fija `min(100 ms, pausa)` de tiempo real sin escala (`Time.unscaledTimeAsDouble`).
- `RallyDirector.Initialize` se suscribe a `PointEnded` y llama a `Begin(WaitBetweenPoints, …)`. El predicado termina el congelado en cuanto la fase deja de ser PointOver/MatchOver (p. ej. `SkipWait`), así que nunca cruza al punto siguiente.
- Las vistas retienen la última pose mientras `Active`: `BallView`, `CharacterView` (ambos), `PlaceholderPlayers.Place` (cápsulas que sostienen las figuras), `CourtCameraRig` (temblor y golpe de FOV), `ImpactFx` y `StrokeFx`. Al terminar, la vista vuelve a mostrar el estado actual en el siguiente cuadro.
- Marcador/HUD (`RallyHud`) no se toca: puntaje y barras siguen actualizándose.
- DEV: `PointFreezeSection` («Congelar fin de punto», botones «Congelar sí/no», por defecto sí, no persiste), enlazada en `DevPanelView`. «No» = comportamiento anterior exacto.

## Por qué es solo vista
- No se modifica Core, física, DefaultPreset, RallyParams, timing, reloj (ClockMap/SimClock), `Time.timeScale`, paso fijo, duración de la pausa ni el instante del siguiente saque. `waited`, `TickPointOver` y `BeginPoint` no cambian.
- Las vistas solo omiten su propio `LateUpdate`/`Place`; la simulación ya avanzó y la vista alcanza el estado vigente al reanudar.
- Sin vistas (AutoQA, sin cabeza) nada lee `Active`: no-op. La pausa por defecto es 1,6 s (Rally) o 2,5 s (Partido), mayor que 100 ms; si una pausa configurada fuera menor, el congelado se recorta a ella.

## Evidencia
- RED por aserción (commit `test: …stub (RED)`): 11 de 13 fallan por aserción, sin errores de compilación. Los 2 que pasaban eran controles: el valor por defecto y la identidad de la simulación.
- GREEN: `ZPointEndFreezeTests` 13/13. Incluye: 100 ms exactos, recorte a pausa corta, fin al empezar el punto siguiente, retención y reanudación de pelota, cuerpos, animación y temblor de cámara, botones DEV, y `SimulationPauseAndNextPoint_AreIdentical_WithFreezeOnAndOff` (dos arranques del mismo escenario; traza por paso de step, launch, fase, posiciones, marcador, `Clock.Paused`, `timeScale` y `fixedDeltaTime` idéntica con el congelado sí/no, misma cantidad de pasos de pausa, ≥ 100 ms).
- Suites completas finales: **EditMode 860/860, PlayMode 653/653**. No se corrieron AutoQA ni check-release (el cambio no toca timing, gesto ni contrato; el encargo no los pedía).

## Riesgos
- `ImpactFx` y `StrokeFx` quedan sin prueba directa (su guardia es una línea en `LateUpdate`); la pelota/cuerpos/cámara sí tienen prueba.
- Estado estático: se restablece con `Reset()` en tests; en producción vive por proceso y el toggle DEV no persiste.
- Durante los 100 ms la cámara de seguimiento (`Tick(ballRenderPos)`) no se retiene, porque sigue la pelota por su lado; solo se retiene el temblor. Si el PO quiere congelarla también, es un cambio aparte.
- No verificado en iPhone: la sensación del golpe congelado es un juicio del PO.
