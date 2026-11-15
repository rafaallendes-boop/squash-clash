# Segundo pase visual («sensación de juego») — plan

Rama `visual/feel-pass`, worktree nuevo desde `main` `7fdef02`. Autor: Claude (Fable). Sesión iniciada el 14 de noviembre de 2026; este plan se confirma el 15.

## Autorización literal del PO

> AUTORIZACIÓN: autorizo un segundo pase visual de «sensación de juego» y golpes más vistosos, sin mecánicas nuevas. Amplío a esta tarea la excepción de dirección de arte que concedí solo para el pase anterior. Cítala en tu informe. Mi uso de Fable es limitado: trabaja por bloques, un commit por bloque que deje la app funcionando; si te quedas sin tiempo, lo ya entregado debe servir. Puedes reordenar dentro de cada prioridad y descartar lo que en las capturas no mejore: prefiero pocas cosas buenas a muchas flojas. Objetivo: que dos jugadores peloteando, incluso sin tocar el teléfono, se vean como un partido de squash con golpes que se disfrutan.

## Límites que respeta el pase

- Solo Runtime/vista. Core, física, reglas, spec, escenarios, umbrales, criterios, `DefaultPreset.json`, SimulationKey `166a96e319c7` y los 41 valores heredados no se tocan.
- No se tocan los archivos de la PR #56 de Codex (`ShotController`, `ShotRecord`, `CausalQuality`, `ZRallyHudTests`, `ZRallyDirectorTests`, etc.) ni `TimingRingView`, `GesturePreview`, `ShotFeedback`, `PerfectFlashView`, `RallyDirector`, el shader `Legibility`, `AGENTS.md` ni el registro de aprobaciones.
- Sin cámara lenta, hit-stop ni escala de tiempo. Todo es presentación derivada de eventos existentes y se congela con la pausa.
- Techo de rendimiento: `perf-after.json` del pase anterior (batches ~58), medido con `ZVisualPerf`.
- Un solo Unity a la vez; pruebas nuevas en fixtures con prefijo `Z` y archivos nuevos.

## Bloques (un commit cada uno, la app queda funcionando)

| # | Bloque | Qué hago | Cómo lo compruebo |
|---|---|---|---|
| 0 | Temblor de cámara | Se congela en pausa y solo existe durante el render: la pose que lee la mira M2-4 no se mueve. Solo `CourtCameraRig`. | Prueba que falla primero (pausa; matriz de cámara igual a la colocada durante el temblor). |
| 1 | Golpes vistosos | Vista nueva suscrita a `PresentationDelivered`, `ShotApplied` y `RivalSwing`: onda y chispas en el contacto escaladas por potencia, color por resultado (Perfect / Valid / fallido), estela de golpe, pelota que se estira un instante, empuje breve de FOV solo en golpes fuertes (también solo en render). Sin tapar pelota, mira ni anillo. | Pruebas de congelado y de pool; tiras de capturas Perfect / Valid / fallido. |
| 2 | Jugadores | `CharacterView`: giro del cuerpo hacia la marcha, carrera hacia atrás distinta, zancada acotada, codo en el brazo de la raqueta, raqueta visible en el contacto, preparación y vuelta a la T más legibles, retroceso al golpear. | Pruebas de pose; tira de un rally en Demo. |
| 3 | Entre puntos | `RallyHud`: pulso del marcador, mensaje breve con la causa, quién saca, tarjeta final más pulida. Sin alargar la pausa de Core y con los mismos nombres de elementos. | Pruebas de nombres, pausa y contenido; capturas. |
| 4 | Pelota (si sobra tiempo) | Marcador barato de legibilidad, sin tocar el radio ni `aids.ballVisualScale`. | Capturas; se descarta si no mejora. |

Cierre: capturas antes/después, medición con `ZVisualPerf`, suites completas y `scripts/autoqa.sh mvp` por bloque, build dev lista **sin instalar**, informe de una página y PR en borrador.

## Criterio de descarte

Lo que en las capturas no mejore se quita antes del commit del bloque y se anota en el informe.
