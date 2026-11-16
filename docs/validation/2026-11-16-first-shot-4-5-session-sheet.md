# First Shot 4.5: hoja de bloques de las sesiones 1 y 2

Predeclaración fijada en git **antes** de la sesión 1 (16 nov 2026). Aplica el [protocolo aprobado](../reviews/2026-11-16-first-shot-s4-4-5-protocol.md) con las recomendaciones de su §7. Plantilla de registro: [first-shot-validation.md](first-shot-validation.md). Opera Claude con el PO presente.

**Autorización del PO (16 nov), literal:** «Si, utiliza mi PC y renueva el vencimiento del 18 de nov. Autorizo las sesiones y quiero hacer las pruebas. Dime como hacerlas, la idea es que salga lo mas rapido posible».

## Build y perfil
- **Build:** `0.1.0+a1fa915 dev`, desde `main` `a1fa915`, que incluye C1 (#63).
- **Perfil:** renovarlo exige una build nueva. El perfil que vencía el 18 nov se apartó del caché de Xcode (queda en `Logs/evidence/`, ignorada) y Xcode generó uno que vence el **2026-11-23T18:03:12Z (15:03 Chile)**.
- **Instalación (16 nov, 18:04Z):**
  - respaldo nuevo antes de instalar;
  - P0-calib-1, Po-1 y last-active idénticos byte a byte después de instalar;
  - el arranque registró P0-calib-1, `effectiveHash` `0a3d3262a79d` y 0 pendientes.
- **Desviación declarada:** el protocolo recomendaba la build `22bb2d5`. **Las dos sesiones usan esta misma build** `a1fa915`. C1 solo cambia lo cosmético: la mano de la figura, la línea técnica en prod y la dificultad guardada.

## Configuración fija en las dos sesiones
- **Preset y ayuda:** P0-calib-1 en práctica (SL1, V100, cámara fija, M2-4, P2) y ayuda de timing **Completa** (`timing.aidLevel=0`). Sin cámara lenta.
- **Interruptores:** «Sonido: sí». La música queda como esté; en práctica no suena.
- **Mano:** la que el PO declare al empezar la sesión 1, igual en la sesión 2. «Mano» en el menú se ajusta a esa misma mano.
- **`repeat`:** no se toca. P0 conserva `launcher.repeatSeconds=3`, igual que en S3, que midió con el hash de P0. La práctica se repite sola con su propio ciclo.
  - **Desviación declarada:** el protocolo §1 traslada el «repeat 0 temporal» de l. 792. Viene de S1, antes de que existiera el modo práctica, y cambiaría el hash.
- **Sin reiniciar la app** durante la sesión: el offset calibrado se pierde al reiniciar. Si la app se cierra, se registra, se calibra de nuevo y se recalculan los hashes.

## Orden de la sesión 1 (sesión 2: bloques 1–4 en orden T3, V, V, T3)

| # | Bloque | Panel DEV (First Shot / timing y practice) | Recepciones | Tarea |
|---|---|---|---|---|
| 0 | Comprensión | «¿Cómo se juega?» una vez y la explicación previa de D4.7 (plan, l. 934) | — | — |
| 1 | Calibración 4.1 | «S4 · Offset perceptivo conjunto»: 3 series de 4 + 8. Dos preguntas antes de ver números, luego «Confirmar offset solo para esta sesión» | — | — |
| 2 | Calentamiento | Validity limit **V**, Early geometry **T3**, Ayuda **Completa** | 6: SL1→SL6, una cada una | sin tarea |
| 3 | B1 | **V**, T3, Completa | 12: SL1→SL6 dos veces | fija H3 |
| 4 | B2 | **T3 pure**, T3, Completa | 12: igual | fija H3 |
| 5 | B3 | **T3 pure**, T3, Completa | 12: igual | fija H3 |
| 6 | B4 | **V**, T3, Completa | 12: igual | fija H3 |
| 7 | T3-suave (H3c) | **T3 pure**, Early geometry **T3 soft**, Completa | 12: igual | fija H3 |
| 8 | H2 | **V**, T3, Completa | 12: igual | rotación H2 |
| 9 | Preguntas | §5 de la plantilla, literales, antes de ver resultados | — | — |
| 10 | Cierre | Exportar, cerrar y reabrir la app, P0 con `0a3d3262a79d` y 0 pendientes, cotejo de los tres archivos | — | — |

- **Tarea fija H3** (Tarea de observación, «Activar en la próxima recepción»):
  - Frontis X `[2,20;4,20)` y Frontis Y `[1,60;2,60)`;
  - Potencia **Media**;
  - Primer bote X `[2,20;4,20)` y Z **Medio**.
- **Rotación H2:** la recepción *i* (1–12) usa:
  - Potencia Suave, Media y Fuerte, rotando en cada recepción;
  - Frontis X `[0,20;2,20)`, `[2,20;4,20)`, `[4,20;6,20]` y otra vez `[0,20;2,20)`, cambiando cada 3 recepciones;
  - Frontis Y `[1,60;2,60)`, bote Z Medio y bote X igual que la columna del frontis.

  Cada valor se elige **antes** de su recepción.
- **Un bloque cada vez:**
  - El bloque empieza cuando el PO cierra el panel DEV (línea `dev-control` con `devOpen=false`) y abarca las N recepciones siguientes.
  - No se repiten ni se sustituyen recepciones.
  - Entre bloques, Claude exporta las métricas y coteja el hash de cada `shot-plan` con la tabla. **Si un hash difiere, la sesión se detiene**: se registra, se exporta y se restaura, sin recalcular.

## Hashes esperados
Se calculan con [Session45HashExperiment](../../Assets/SquashClash/Tests/Experiments/EditMode/Session45HashExperiment.cs), sobre el `DefaultPreset.json` real y el respaldo de P0-calib-1. Reproduce `TuningState.Set` y el reposicionamiento del cuerpo de `PracticeMode`.

**Comprobaciones previas:** defaults `06fed4bd4ac9`/121 y P0 `0a3d3262a79d`, ambos iguales a AGENTS §5.

**El hash depende del recorrido:** el cuerpo se reasigna como float al cambiar de lanzamiento. Después de una vuelta SL1→SL6, el valor es estable. El calentamiento recorre SL1→SL6, así que **todos los bloques medidos usan la columna «estable»**. El hash también incluye el offset confirmado (`timing.latencyCompMs`), así que **la tabla se recalcula con el offset de la sesión antes del bloque B1** y se agrega aquí en un commit. Referencia con offset 0:

| Modo | SL1 | SL2 | SL3 | SL4 | SL5 | SL6 |
|---|---|---|---|---|---|---|
| V, estable | `5be2362d463b` | `579e82a2c150` | `f79e5e386819` | `e8f09fa57899` | `72c1bafa0535` | `0ecd491c36e3` |
| T3, estable | `f4673af21fd3` | `351c29dae32b` | `ea4fd9ad0227` | `3537c09a4114` | `cba635c8298b` | `7b0f0efc8799` |
| T3-suave, estable | `ec2629e91c2b` | `504d13d268f2` | `cda10bd43b16` | `6e2d24ab7b3c` | `13be0e980e84` | `e59115a08347` |

En el calentamiento, SL1 y SL2 se lanzan antes de pasar por SL3, así que usan el valor «desde P0»: `0a3d3262a79d` y `6daa20fa4f6a` con V y offset 0. La tabla completa con las columnas «desde P0», «vuelta 1» y «vuelta 2» se guarda en `Logs/experiments/session45/expected-hashes.tsv`, carpeta ignorada.

## Sesión 1: offset confirmado y tabla que se aplica

**Primer intento de calibración, invalidado:** quedó registrado como «focus-lost» a las 18:18:35Z, antes de confirmar. La app perdió el foco cuando el PO salió a responder las preguntas en el chat. El offset no se aplicó.

**Recalibración:**
- **Resultado:** media **60,976 ms**, con medias por serie de 55,2, 80,4 y 47,4 ms, IC95 de 18,1 a 103,8 ms y rango de medias de 33,0 ms.
- **Confirmación:** a las 18:30:33Z, con `meanMs=60.9760989365092`.
- **Sesgo declarado:** el PO ya había visto los números del intento invalidado.
- **Comprobación de la tabla:** el primer `shot-plan` después de confirmar tiene hash `7d91265db34d`, que coincide con el valor «desde P0» calculado para V SL1. La tabla queda validada contra el teléfono.

Hashes de los bloques medidos (columna «estable», offset 60.9760989365092 ms):

| Modo | SL1 | SL2 | SL3 | SL4 | SL5 | SL6 |
|---|---|---|---|---|---|---|
| V | `21cd1bb43f9f` | `6d4c07a37147` | `8086000f6d40` | `4e7cce638f61` | `c6b8b3bf82bd` | `43c47fa596f5` |
| T3 | `f8b660c2364a` | `713778ff5c99` | `350b91119ea4` | `f2979b15af78` | `212958cdb927` | `4e6ffbdde63a` |
| T3-suave | `9a4fde02d141` | `5d5a772f1f48` | `7ea2c0c85dc1` | `677873c500e0` | `450d6d7c55a8` | `b0a997e6c3ea` |

**Calentamiento en V:** SL1 `7d91265db34d`, SL2 `3940490b7e2d`, SL3 `8086000f6d40`, SL4 `4e7cce638f61`, SL5 `c6b8b3bf82bd`, SL6 `43c47fa596f5`.
