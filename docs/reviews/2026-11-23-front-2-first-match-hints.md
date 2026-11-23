# Front 2 — pistas del primer partido (implementación)

23 nov 2026. Rama `feat/first-match-hints`, base `origin/main` `4639df0` (merge de #93, con la spec aprobada en `5a3b15b`). Implementa [la spec](../superpowers/specs/2026-11-22-front-2-first-match-hints.md) en una sola PR, con los dos tramos de la spec (datos y visual). **Revisión no independiente:** la escribió Claude (Sonnet 5.5); la revisión cruzada es de Codex, en solo lectura.

## Alcance entregado

- **Todo, incluido el bloque separable H0 en el aviso [H0-aviso], completo**, con sus tests. No se difiere nada.
- `HintPicker` y los oráculos (`Core/Rally`), el adaptador `FirstMatchHints` (`Runtime/Menu`), la fila del aviso, la pastilla de la guía, las líneas `hint` y `serve-choice`, el interruptor DEV y «Reiniciar pistas».
- Marcas de solo lectura: `ShotController.LastStrokeBoast` (§1.6, se introduce aquí), `RallyDirector.LastServeChosen` y `LastServeFromPause` (§1.4). Ninguna cambia una decisión: el AutoQA lo confirma.
- **Textos de H0 de SC-Lead (23 nov, 11:17, punto 1)**, no los de la spec en `main`: guía «Sacas tú: toca el frontis antes de que se acabe la barra. Si no tocas, sale un globo.»; aviso «Cuando saques tú, toca el frontis durante la pausa: arriba sale un globo; abajo, un saque rápido.» (tres líneas como máximo). La spec sigue diciendo «la línea»: queda desactualizada en esas dos cadenas.
- **Fuera de esta PR, por indicación de SC-Lead (punto 2):** «Toca abajo: drive» no se toca; va en otra PR.

## Cumplimiento de las reglas de prueba (spec §4)

- Los tests existentes **no se editaron**. El único cambio de fixture es el declarado: `ZFirstRunFixture.ClearPrefs` borra también `FirstMatchHints.AllKeys()`.
- Las pistas están **encendidas** en toda la suite. Se apagan solo en `Off_NothingIsWrittenOrShown`, en `NoAddedTime_OnMatchesOff` y en la referencia de la nota del criterio 10.
- Precondiciones de H1/H2 frente a H0 (observación de Codex r4): los tests aislados de H1 y H2 parten con H0 ya vista (`OnlyVolleyLeft`, `Preset`). El caso simultáneo, H0 antes que H1, está en `H0InTheBanner_GoesBeforeAnArmedVolley…` y en `HintPickerTests`. No se cambió ninguna aserción ni criterio de la spec.
- Los RED fueron válidos por aserción (esqueleto con firmas, sin comportamiento). Dos correcciones de **mis propios tests** antes del GREEN, no de criterios: el arnés de `ZStrokeBoastTests` no llamaba a `Enqueue`, y el esqueleto devolvía `AllKeys()` vacío, lo que filtraba claves entre tests. Tras el primer GREEN ajusté mis tests nuevos: tolerancia de píxeles en los tests de aspecto (la capa de UI escala 2 px a 1,94), la sonda «antes del contacto» (leía tras una aplicación instantánea) y mi expectativa de que sin golpear no se arma H1 (sí se arma, es el disparador de la spec).

## Resultados (sobre `c4c755d`, árbol limpio)

| Puerta | Resultado |
|---|---|
| EditMode | 1000/1000 (`scripts/test.sh editmode`) |
| PlayMode | 812/812 (`scripts/test.sh playmode`), con las pistas encendidas |
| AutoQA `mvp` V, T3 y volea | PASS / 0 / nivel 1 en las tres (102/102 cada una, `dirty=false`, hash `c4c755d806ef`) |
| `check-release` | OK (dev y prod, build 1144, flags OK) |

Hashes: sin cambios (no se tocó `DefaultPreset.json`, SimulationKey, física, solver, timing ni RallyParams).

## Textos (regla del 23 nov)

| Texto | Revisión del autor |
|---|---|
| H0 guía | «frontis», «globo»; «barra» por decisión de SC-Lead; tuteo neutro |
| H0 aviso | «frontis», «globo», «saque rápido»; tuteo neutro; pensado para tres líneas |
| H1 «Anillo ámbar: toca y volea antes del bote.» | OK |
| H2 «Toca la pared lateral para jugar un dos paredes.» | OK |
| H3 «Pared lateral cerca de ti: la pelota cruza al otro lado.» | OK |
| Botones DEV «Pistas: sí/no», «Reiniciar pistas» | solo DEV, el jugador no los ve |

Pregunta para SC-Lead que queda abierta: ¿«saque rápido» (aviso) y «Toca abajo: drive» (guía, sin tocar) deben unificarse en la PR aparte del punto 2?
