# Anillos iguales en todo el partido: un pulso del blanco al entrar

25 nov 2026. Rama `rafaallendes-boop/sc-anillos-uniformes` desde `853c9bd`. Encargo de SC-PM (`set7-anillos.txt`). Arquitectura y spec: Claude Opus. Implementación: Claude Sonnet con TDD (AGENTS §2), así que **la revisión de Claude no es independiente** y queda para Codex.

## Qué dejó #109 (merge `3e647c4`, spec `2026-11-24-ring-close-pulse-design.md`)

- **Ámbar que se cierra:** mientras la oferta de volea no está elegida, el radio baja de 0,45 m a 0,20 m a lo largo de la ventana de toque (`TryVolleyWindow`: del lanzamiento a S′). `ShotController.UpdateRingClosing()` pasa la ventana a `TimingRingView` en cada `LateUpdate`, con la única condición `VolleyShown && !VolleyCommitted`. El ámbar sale del mismo `LaunchToHuman` en el saque y en el peloteo (`RallyDirector.cs:296-320`), así que **el cierre ya es igual en las dos recepciones**. No se duplica: este encargo solo agrega una prueba de partido que lo comprueba en ambas.
- **Blanco que respira:** mientras espera en el radio exterior (`Approach` = 1), el alfa oscila sin parar entre 1 y 0,45 a 2 Hz, con la fase contada desde el último `Show` (un replan reinicia la fase). Al converger queda en 1.
- Texto único de la regla (`ReceptionRule`). No cambia aquí.

## Qué cambia

**El blanco hace un solo pulso al entrar, en lugar de respirar.** Reemplaza el punto 2 y el criterio B1 de la spec de #109; el resto de esa spec sigue vigente y no se edita.

1. El pulso es un ciclo de la misma forma: el alfa del anillo que converge va de 1 a 0,45 y vuelve a 1 en 0,5 s (coseno), y después queda fijo en 1.
2. **Entrada** = la primera vez que el anillo blanco se muestra visible en una oportunidad: al lanzamiento si no hay ámbar, o cuando el ámbar cede al blanco (paso por S′ o volea rechazada, ambos con `BeginOpportunity`). `BeginReception` y `BeginOpportunity` la reinician. Un replan del mismo anillo (otro `Show` en la misma oportunidad) **no** repite el pulso.
3. Como hoy, solo hay pulso mientras el blanco espera en el radio exterior. Si ya está convergiendo (últimos 500 ms), el alfa es 1, para que el calce se lea limpio.
4. El ámbar, el halo, el triángulo y las bandas no pulsan (sin cambios).

## Fuera de alcance

- Cuándo aparece cada anillo y la pastilla, el radio que juzga el toque, la ventana de la volea, el radio de calce, las bandas, el anillo ya elegido y el cierre del ámbar.
- Timing, contrato, reloj, solver, presets, umbrales, tolerancias, oráculos y criterios. Los hashes de §5 no cambian.
- `TimingAidTests`, sin tocar (no tiene aserciones de alfa).

## Criterios de aceptación

| # | Criterio | Prueba |
|---|---|---|
| P1 | Blanco quieto: alfa 1 al mostrarse, 0,45 (±0,01) a los 0,25 s, 1 a los 0,5 s y 1 (±1e-5) de 0,5 s a 3 s. Siempre en [0,45; 1] y habilitado. | PlayMode, vista sola (reemplaza B1) |
| P2 | Un replan del blanco en la misma oportunidad, 0,3 s después de entrar, no reinicia el pulso: 0,25 s después del replan el alfa es 1. | PlayMode, vista sola |
| P3 | Cuando el ámbar cede al blanco (`BeginOpportunity`, tinte blanco, `Show`), hay un pulso nuevo: 0,45 a los 0,25 s de la entrada y 1 a los 0,75 s. | PlayMode, vista sola |
| P4 | Un blanco que entra ya convergiendo no pulsa: alfa 1. | PlayMode, vista sola |
| U1 | En la escena Main, en una recepción del saque del rival con volea del saque y en una recepción de peloteo, ambas con ámbar: el ámbar sin elegir se cierra de forma monótona dentro de su ventana y, tras ceder al blanco, el blanco que espera tiene alfa 1 pasado su pulso. | PlayMode, escena Main |
| R | B2, B3 y A1–A5 de `ZRingClosePulseTests`, `TimingAidTests`, `ZVolleyV2Tests` y `ZReceptionRingsTests` siguen en verde sin cambiar sus aserciones. Solo cambia B1, que describe la respiración. | EditMode, PlayMode, AutoQA |

## Sin iPhone y en iPhone

- **Mac:** la tabla, con `scripts/test.sh editmode|playmode`. AutoQA `mvp` V/T3/volea, porque el cambio está junto al anillo de la volea, aunque no toque timing, gesto ni contrato.
- **iPhone:** que el pulso de entrada se note como «ahora va el blanco» sin parecer un fallo; que el blanco quieto ya no distraiga; que en saque y en peloteo el ámbar se cierre igual.

## Riesgos

- Si el blanco entra con menos de 0,5 s de espera antes de converger, el pulso se corta al converger (alfa 1). Es intencional: el calce manda.
