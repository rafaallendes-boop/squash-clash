# Dos paredes: vista previa en el frontis y pista de altura

24 nov 2026. Ítem 2 del set de recepción, opciones 1 y 2 del [diagnóstico](../../reviews/2026-11-24-dos-paredes-alto.md) (#110) con las condiciones de su revisión cruzada (comentario en #110). Lo implementa Claude (Opus), así que la revisión de Claude no es independiente. Rutas relativas a `Assets/SquashClash/`.

## Contexto

Al tocar la lateral, el jugador elige un punto (z, y) de esa pared. Desde un contacto bajo y con un golpe rápido, la pelota llega a la lateral todavía subiendo, y cada metro de altura en la lateral puede ser hasta ≈ 2,8 m en el frontis. Mientras el dedo está apoyado solo se ve el anillo en la lateral. La cruz del frontis aparece recién al resolver el golpe, cuando ya no se puede corregir. El tutorial y la pista no dicen nada de la altura.

## Alcance

1. **Vista previa (opción 1).** Mientras el dedo está apoyado en la lateral y ya se tira (P > 0), se muestra en el frontis una **zona punteada** (un óvalo de 12 trazos celestes de 0,30 × 0,36 m de semiejes) centrada en el punto donde pegaría el dos paredes con el timing ideal. La zona está a 0,03 m delante del frontis (z = 0,03), y la cruz, a z = 0,035.
   - **Discreta (SC-Lead, 24 nov).** Es una pista, no un blanco que juegue por el jugador. Por eso el trazo (0,022 m) es más fino que el de la cruz (0,035 m) y el color es translúcido (alfa 0,45). El shader `Legibility` mezcla el alfa.
   - **Vista aparte.** La vista es un componente nuevo, `Runtime/Shot/BoastPreviewView.cs`. Solo **lee** `GesturePreview.M24` (`Active`, `HasAim`, `Side`, `P`) y, del `ShotController`, `Plan`/`Volley`/`VolleyCommitted`/`AttemptConsumed`/`Available`. No escribe en ellos, no escribe métricas y no se suscribe a la entrada. `GesturePreview.M24.cs`, `M24Model`, `FrontisProjection` y la admisión del gesto no cambian. `AppBootstrap` solo suma una línea para crear el componente.
   - **Camino de pronóstico.** El cálculo va en `Core/Shot/BoastPreview.cs`: `ShotPace.Boast` y luego `BoastSolver.Solve` desde P\* (el contacto planeado; el de la volea si se eligió), sin la capa causal. Es el mismo cálculo que el pronóstico del rival (`RallyContact.SolveBoastIntention`). No toca `ShotController.Solve`, `ShowShot` ni la resolución autoritativa. El solver, la física, el contrato y el timing se usan sin cambios.
   - **Costo acotado.** `BoastPreviewSchedule` decide cuándo recalcular. Recalcula una vez al empezar cada toque (o si cambia el contacto o el punto lateral). Después, como mucho una vez cada 0,10 s de tiempo real y solo si P cambió. Con el dedo quieto no recalcula. Cada cálculo tiene el tope fijo del solver (`BoastSolver.Budget` = 25 trayectorias). El 0,10 s es un parámetro de la vista, no de juego: no está en `DefaultPreset` ni en ningún hash.
   - **Se distingue de la cruz final.** La cruz blanca (`AimReticleView.ShowFeasible`) es la que marca el golpe que de verdad sale y aparece al resolver. La zona es otra forma (óvalo de trazos, no cruz), de otro color (celeste translúcido, no blanco), con un trazo más fino y solo existe con el dedo apoyado: se oculta al soltar, al cancelar y cuando se consume el intento. Nunca se ven las dos a la vez.
   - **No promete el punto exacto.** La vista previa no incluye la capa causal (timing, estiramiento, cansancio), que en los casos del diagnóstico sumó +0,55 y +0,60 m. Por eso es una zona y no un punto, no lleva número ni texto, y el tutorial dice «más o menos». La adenda 2 de la predeclaración («la retícula muestra el golpe que de verdad sale») sigue valiendo para la retícula sin cambios: la zona no es la retícula.
   - Si con P\* el dos paredes no tiene solución (iría al frontis por el respaldo), no se muestra ninguna zona.
2. **Pista (opción 2).** En el paso «Dos paredes» del tutorial y en la pista «dos paredes» del primer partido, el aviso de que tocar alto en la lateral puede dejarla larga, y en el tutorial además el consejo de tocar bajo. Términos de squash, español neutro y tú. En los textos que se tocan se usa «frontis», y el tutorial lo explica una vez como «la pared del frente», que es el término del paso 1. Los textos finales están en la PR (sección «Textos»).

## Fuera de alcance

Las opciones 3 a 6 del diagnóstico (tope de altura, cambiar el significado de la altura lateral, acotar la capa causal, bajar la velocidad) son de Rafael. Tampoco cambian umbrales, tolerancias, oráculos, criterios, timing, física, contrato, `DefaultPreset.json`, SimulationKey, RallyParams ni el código del gesto M2-4. La vista previa no se agrega al rival ni al saque, donde la lateral no está abierta.

## Criterio verificable

- **EditMode:**
  - `BoastPreview.Estimate` da, bit a bit, lo mismo que `BoastSolver.Solve` con la velocidad de `ShotPace.Boast`.
  - Con el contacto del partido 2 del diagnóstico, un toque más alto en la lateral da una zona más alta en el frontis.
  - `BoastPreviewSchedule` cumple las reglas de costo de arriba.
- **PlayMode, con un toque real en la lateral y la lateral abierta:**
  - Con el dedo apoyado y tirando, la zona se ve delante del frontis (z = 0,03) en el punto de `BoastPreview.Estimate` desde `Plan.PStar` y `M24.P`, y la cruz no se ve.
  - Al soltar, la zona se oculta.
  - Un toque al frontis nunca la muestra.
  - Con el dedo quieto no recalcula.
  - La vista no cambia el estado del gesto ni del controlador.
  - La zona es discreta: el trazo es más fino que el de la cruz (< 0,035 m) y el alfa es ≤ 0,5.
- **Textos:** la constante `BoastText` de `ZFirstMatchHintsTests` pasa al texto nuevo. Ninguna otra aserción se debilita: el tutorial sigue conteniendo «pared lateral», «frente» y «saque», y cabe en la pantalla.
- **Puertas:** EditMode, PlayMode, AutoQA (`scripts/autoqa.sh mvp`, PASS/0/1, porque el cambio está cerca del gesto) y check-release. Los hashes de §5 no cambian.

## Qué se prueba sin iPhone y qué en el iPhone

- **Sin iPhone:** todo lo anterior, y el costo de un cálculo medido en el Mac con el peor caso (25 trayectorias).
- **En el iPhone:**
  - Que la zona se lea bien sobre el frontis y no se confunda con la cruz ni con el anillo.
  - Que no se note un tirón de cuadro al tirar del dedo (el p95 de cuadro es una observación abierta).
  - Que tocar alto en la lateral muestre la zona alta antes de soltar.
  - Que los textos quepan y se entiendan.
