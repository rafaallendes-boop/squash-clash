# Anillos iguales en todo el partido: un pulso del blanco al entrar

25 nov 2026. Rama `rafaallendes-boop/sc-anillos-uniformes` desde `853c9bd`. Encargo de SC-PM (`set7-anillos.txt`). Spec: [2026-11-25-ring-entry-pulse-design.md](../superpowers/specs/2026-11-25-ring-entry-pulse-design.md). Arquitectura, spec y revisión: Claude Opus. Implementación: subagente Claude Sonnet con TDD (AGENTS §2). **La revisión de Claude no es independiente**: la revisión independiente queda para Codex.

## Qué dejó #109 y qué cambia

- **Ámbar que se cierra (#109, sin cambios).** Mientras la volea no está elegida, el radio baja de 0,45 m a 0,20 m a lo largo de la ventana de toque, del lanzamiento a S′. La condición es solo `VolleyShown && !VolleyCommitted`, y el ámbar sale del mismo `LaunchToHuman` en el saque y en el peloteo. **Ya era igual en las dos recepciones**, así que no se duplica: se agrega una prueba de partido (U1a/U1b) que lo comprueba en ambas.
- **Blanco: respiración → un pulso (cambia).** Con #109, el blanco respiraba sin parar a 2 Hz mientras esperaba, con la fase contada desde el último `Show`, de modo que un replan la reiniciaba. Ahora hace **un solo pulso al entrar**: el alfa va de 1 a 0,45 y vuelve a 1 en 0,5 s, y después queda fijo en 1.
  - La entrada se cuenta desde el primer `Show` visible del blanco en la oportunidad, y `BeginReception` y `BeginOpportunity` la reinician.
  - Un replan no repite el pulso.
  - Cuando el ámbar cede al blanco, en S′ o al rechazar la volea, hay un pulso nuevo.
  - Al converger, el alfa sigue en 1.
- **Reemplaza** el punto 2 y el criterio B1 de la spec de #109, que no se edita. El resto de esa spec sigue vigente.
- `TimingRingView.cs` cambia en 8 líneas y quita 4: el campo nuevo `whiteEnteredAt` y la condición del pulso. Nada más del runtime cambia.
- **Sin cambios:**
  - Cuándo aparece cada anillo o la pastilla, el radio que juzga el toque, la ventana de la volea, el radio de calce, las bandas, el anillo ya elegido y el cierre del ámbar.
  - Timing, contrato, reloj, solver, presets, umbrales, tolerancias, oráculos y criterios. Los hashes de §5 no cambian.
  - `TimingAidTests`, sin tocar.

## Textos

Sin cambios de texto. La regla (`ReceptionRule`), la pastilla, el tutorial y la pista H1 quedan como los dejó #109.

## Pruebas (TDD)

Archivo: `ZRingClosePulseTests.cs`.

- **RED por aserción** en `c750fdb`, filtro `ZRingClosePulse`: 13 pruebas, 8 pasaron y 5 fallaron. No hubo errores CS ni hizo falta un stub.

  | Prueba | Aserción que falló |
  |---|---|
  | P1 | «steady after the pulse: t=0.52»: esperado 1, obtuvo 0,992 |
  | P2 | Replan a los 0,3 s: esperado 1, obtuvo 0,451 (el replan reiniciaba la fase) |
  | P3 | A los 0,75 s de la entrada: esperado 1, obtuvo 0,451 |
  | U1a | Saque: esperado 1, obtuvo 0,62 |
  | U1b | Peloteo: esperado 1, obtuvo 0,992 |

  - P4 (un blanco que entra ya convergiendo no pulsa) pasa antes y después: es una guarda de lo que no cambia.
  - La primera corrida de `d2c6402` falló U1a porque la semilla 3 no ofrecía ámbar en el saque. U1a ahora prueba semillas de la 1 a la 24 y gana la **2**, con 51 muestras de ámbar y 18 de blanco fijo pasado el pulso.
  - En `14ee3fd`, U1a afirma además que es una recepción del saque del rival: `ServeReception`, `Match.Server == Rival` y `Rules.Serving`.
- **GREEN** en `b7f5449` y `14ee3fd`: 13/13.
- **Única aserción existente que cambia: B1**, que pasa a P1. B1 describía la respiración de 2 Hz («0,45 a 0,25 s, 1 a 0,5 s, siempre en [0,45; 1]»), que es justo lo que este encargo reemplaza. P1 conserva esas tres comprobaciones y agrega que el alfa queda en 1 (±1e-5) de 0,5 s a 3 s.
- A1–A5, B2, B3, C1 y C2 siguen sin cambios, y `TimingAidTests`, `ZVolleyV2Tests` y `ZReceptionRingsTests` también.

## Puertas

Sobre `14ee3fd`, con el árbol limpio, en serie y con el lock de Unity, en el Mac del PO:

| Puerta | Resultado |
|---|---|
| EditMode | **1032/1032** (0 fallidas, 0 omitidas) |
| PlayMode | **863/863** (0 fallidas, 0 omitidas) |
| AutoQA `mvp` v / t3 / volley | PASS / exit 0 / nivel 1 en las tres, `dirty: false` |
| check-release | OK (build 1209, dev y prod) |

`scripts/device.env` se copió sin cambios. Su SHA-256, `0b0c94db…`, es idéntico al de `sc-install-518f9c3`, que es el origen autorizado. El subagente lo tomó del clon principal, con el mismo contenido. No se versionó y se borró al terminar.

Evidencia local, ignorada por git: `Logs/evidence/ring-entry-pulse/` (XML de EditMode y PlayMode, salidas de AutoQA y check-release, `gates.txt`).

## Qué mirar en el iPhone

- [ ] El pulso del blanco al entrar se lee como «ahora va el blanco» y no parece un parpadeo o un fallo.
- [ ] El blanco quieto ya no distrae mientras espera el bote.
- [ ] Cuando el ámbar cede al blanco en S′, el pulso marca el cambio.
- [ ] En el saque del rival y en el peloteo, el ámbar se cierra igual hacia el punto de golpe.
- [ ] Al converger, el blanco está fijo y el calce se lee limpio.
