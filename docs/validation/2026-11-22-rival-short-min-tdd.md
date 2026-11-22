# Evidencia TDD — rival ante el corto; camino A, solo I1 en #97

Base `518f9c3`, rama `feat/rival-short-min`, PR #97. Spec de #92 leída con `git show`, head `d320c13`.

## Alcance vigente — decisión de SC-Lead, 22 nov

RAL-174, camino A: «la PR #97 queda solo con I1, el intento visible». I2–I3 se conservan en `feat/rival-short-i2i3`, sin PR. En #97 se retiran mediante commits nuevos, sin reescribir historial ni forzar el push. Se restaura `RivalAi.cs` byte a byte desde `518f9c3` y se retiran únicamente el test nuevo `RivalShortBallTests.cs` y su meta; ningún test existente cambia. Los contenidos originales de I2–I3 se conservan exactamente desde `e37fbbb`, incluidos los cambios de `67d2f33` y `5d77420`.

Se corrieron de nuevo EditMode completo, PlayMode completo, AutoQA V/T3/volea y check-release, con un candado por corrida; todos quedaron verdes (tabla al final). Cualquier fallo de M3 u otro test existente habría exigido registrar y detenerse. Se autoriza retirar el borrador de #97 con estas puertas verdes. No hay autorización de fusión ni instalación. Lo que sigue hasta las nuevas corridas de I1 es el historial de I1–I3, no el alcance actual de #97.

## Autorización de la fase I1–I3 (histórica)

SC-PM, 22 nov 19:15 −03: «Mi condición anterior fue demasiado estricta y la retiro». Los errores de compilación, del parche nuevo y las suposiciones equivocadas de los tests nuevos se corrigen dentro de TDD, con expectativas derivadas de la spec. La regla de parada queda limitada a necesitar cambios en tests existentes, spec, escenarios, umbrales, tolerancias, oráculos, presets o criterios; fallos de entorno; o comportamiento inesperado del producto ya presente en main. Se conserva el candado de Unity y no se autoriza fusión ni instalación.

## Corridas previas

- RED inicial: 0 tests, CS0234 en el test nuevo (`Physics.bounceThreshold`).
- RED de I1 autorizado por SC-PM (17:58, hora del mensaje): 2 tests, 1 Passed, 1 Failed, 0 Skipped. XML fresco; esperado 103, obtenido −1. Guardia de cierre antes del bote: verde desde el esqueleto.
- Primer GREEN: 0 tests, CS1529 por `using System;` al final del parche de I1. XML anterior rechazado. Ahora se corrige el orden del import sin cambiar el test.

Logs y XML completos: `Logs/evidence/rival-short-min/` (ignorados). Cada corrida adquiere y libera su propio candado; XML viejo y cero tests nunca se cuentan como aprobado.

## I1 — función pura

`i1-green-resumed`: `scripts/test.sh editmode SquashClash.Core.Tests.Rally.RivalAttemptTests`: **2/2 Passed**, 0 Failed, 0 Skipped. XML fresco y sin errores CS. Se conserva el RED válido anterior (103 frente a −1) y la guardia que nació verde.

## I1 — director

`i1-director-red`: `scripts/test.sh playmode SquashClash.Runtime.Tests.ZRivalShortBallTests`: **2 tests, 1 Passed, 1 Failed, 0 Skipped**, XML fresco. El escenario confirmó `RivalPlan.Reason == "unreachable"`; el evento falló por aserción (esperado 1, obtenido 0). La guardia de recepción con golpe nació verde.

`i1-director-green`: mismo comando y escenario: **2/2 Passed**, 0 Failed, 0 Skipped. Un intento fallido antes de `PointEnded`, cero contactos, resultado `Unreachable` conservado y sin segundo intento. La recepción con golpe sigue emitiendo exactamente un evento. No se modificaron los tests.

## I2–I3 — RED y huella previa

`ai-red-baseline`: `scripts/test.sh editmode SquashClash.Core.Tests.Rally.RivalShortBallTests`: **15 tests, 8 Passed, 7 Failed, 0 Skipped**. XML fresco, sin errores CS.

- I2, RED válido: en 500 decisiones por estilo aparecieron **65/144/112** dos paredes (Fondista/Definidor/Completo), frente al 0 exigido. Las tres caracterizaciones defensivas desde adelante y las tres guardias `planned` con dos paredes nacieron verdes.
- I3, respaldo: RED por aserción, retraso elegido 46 pasos frente al mínimo 27.
- I3, caso mixto: la precondición del test nuevo no se satisfizo en su búsqueda a altura fija 0,8 m (cero casos). No se cuenta como RED válido del filtro. La spec exige un contacto adelante con el oponente en la T y opciones de ambas clases; se corregirá esa suposición del test buscando dentro de las alturas legales, sin cambiar esa expectativa ni parámetros.
- Hashes: guardia Passed, defaults `207da8f2c636`, RallyParams `a82c34b661bf`, SimulationKey `166a96e319c7`.
- Huella de 1.080 decisiones `planned` (3 estilos × 3 niveles × 3 contactos × 2 holguras × 20): `53870c765e9e1cc410b87b41d810afc7fbe53ac0ffa42345b9b97192786de4de`. Capturada con `RivalAi`, planificador, solver y parámetros idénticos a `518f9c3`, antes de I2/I3. Se congela como aserción adicional del test nuevo según §5.2, paso 10.

`i2-green`: filtro de I2, caracterizaciones desde adelante, guardias `planned`, huella y hashes: **11/11 Passed**, 0 Failed, 0 Skipped. Se mantiene R2 en 0; las tres guardias bajo presión siguen permitiendo dos paredes. Sin cambios en tests existentes.

## I3 — RED válido del filtro

Se corrigió la suposición de altura fija del test nuevo: la búsqueda ahora recorre la ventana legal de alturas y contactos adelante de la T, conservando al oponente en la T, el predictor como oráculo y la expectativa 0. Es el escenario de §5.2, paso 8; no cambia ningún escenario predeclarado del experimento ni test existente.

`i3-filter-red`: **4 tests, 0 Passed, 4 Failed, 0 Skipped**, XML fresco y sin errores CS. El caso encontrado es `(x=0,6; y=0,05; z=1,1)`: recta no alcanzable, cruzado alcanzable. Cada estilo eligió **62/100** opciones inalcanzables frente a 0 exigido. El respaldo volvió a fallar por 46 pasos de retraso frente al mínimo 27. Ambos son RED válidos antes de implementar I3.

`i3-green`: `scripts/test.sh editmode SquashClash.Core.Tests.Rally.RivalShortBallTests`: **15/15 Passed**, 0 Failed, 0 Skipped. R2 = 0 en los casos probados; R3 = 0/100 elecciones inalcanzables por estilo cuando existe una opción alcanzable. En el caso sin ninguna alcanzable, las 50 elecciones tienen retraso mínimo y `LastStretchedFallback == true`. La huella de las 1.080 decisiones `planned` coincide con la previa a I2/I3 y los tres hashes siguen iguales.

La protección R3 se aplica a las intenciones nominales antes del sorteo y de los ruidos de ejecución, según §5.1 I3. No garantiza que el bot humano no cometa errores (R6). El retraso del respaldo es `ArrivalStep − TStarStep` si hay contacto, o respecto del final de la trayectoria de bote si no hay contacto. No se añade ningún parámetro.

## Gate completo de EditMode — parada por M3 existente

Sobre código `5d77420`, `editmode-full`: `scripts/test.sh editmode`: **978 tests, 977 Passed, 1 Failed, 0 Skipped**, salida 1, XML fresco. Ningún error de compilación. Los 17 tests nuevos de EditMode y la huella congelada pasaron.

Único fallo: `SquashClash.Core.Tests.Rally.BoastRivalTests.On_InPlay_TheFondistaBoastsLessFromTheBackQuarterThanTheDefinidor`, línea 81 del test existente. Comparación M3 vigente, sobre seis partidos por estilo contra el bot de golpes largos:

| Estilo | Fracción de dos paredes entre contactos desde el cuarto de fondo | Porcentaje |
|---|---|---|
| Fondista | `0.039906103286384977` | 3,9906103286384977 % |
| Definidor | `0.014705882352941176` | 1,4705882352941176 % |

El criterio exige **Fondista < Definidor**; esta rama invierte ese orden. No se interpreta como fallo de entorno ni se cambia la comparación, las semillas, los partidos, pesos, tolerancias, presets o criterios. Conforme al brief (parada si cambia un test existente de calibración) y la autorización vigente, se registra y se detiene, sin reintentar para obtener Pass. No se hicieron más cambios de código para recuperar M3.

Logs y XML: `Logs/evidence/rival-short-min/editmode-full/` (ignorados). El candado adquirido fue liberado inmediatamente al terminar Unity. PlayMode completo, AutoQA V/T3/volea y check-release no se ejecutan tras esta parada; el PlayMode dirigido de I1 permanece 2/2 Passed. R7 y Anexo A no medidos: estos porcentajes M3 no son un Δ respecto de main. La PR #97 sigue en borrador.

## Gates completos del camino A — solo I1

Sobre `028718278a626f00630178d51f7a2b1a6a70830b`, árbol limpio al iniciar. Cada comando adquirió su propio candado con el bucle indicado y lo liberó al terminar, incluso entre suites. `check-release` esperó al otro worker; nunca se abrió un segundo Unity.

| Comando | Resultado | Evidencia local ignorada |
|---|---|---|
| `scripts/test.sh editmode` | **963/963 Passed**, 0 Failed, 0 Skipped; salida 0 | `Logs/evidence/rival-short-min/i1-only-editmode-full/` |
| `scripts/test.sh playmode` | **748/748 Passed**, 0 Failed, 0 Skipped; salida 0 | `Logs/evidence/rival-short-min/i1-only-playmode-full/` |
| `scripts/autoqa.sh mvp --suite v` | **102/102 tests**, 18 casos PASS; **PASS/0/nivel 1** | `Logs/autoqa/20261122T230826Z-0287182-v-27853/` |
| `scripts/autoqa.sh mvp --suite t3` | **102/102 tests**, 18 casos PASS; **PASS/0/nivel 1** | `Logs/autoqa/20261122T230913Z-0287182-t3-21029/` |
| `scripts/autoqa.sh mvp --suite volley` | **102/102 tests**, 18 casos PASS; **PASS/0/nivel 1** | `Logs/autoqa/20261122T230954Z-0287182-volley-9043/` |
| `scripts/check-release.sh` | **RELEASE-CHECK: OK**, dev y prod, build 1087, salida 0 | `Logs/evidence/rival-short-min/i1-only-release/` |

XML frescos, sin errores CS ni pruebas omitidas/canceladas. M3 (`On_InPlay_TheFondistaBoastsLessFromTheBackQuarterThanTheDefinidor`) **Passed** sin modificar el test. Las guardias existentes verifican defaults `207da8f2c636`, RallyParams `a82c34b661bf` y SimulationKey `166a96e319c7`.

En cada suite AutoQA: 0 FAIL, 0 INCONCLUSIVE, 0 NOT_EVALUATED; `dirty=false`; integridad de hashes, presets, guardia y SHA-256 en true; restauración `statusUnchanged=true`. El canario negativo falló como se esperaba. Exportaciones dev/prod frescas: solo Core/Runtime y DevPanel adicional en dev; flags iOS OK y puentes nativos verificados. El árbol volvió a quedar limpio tras las limpiezas automáticas del export.

Resumen agregado ignorado: `Logs/evidence/rival-short-min/i1-only-gates-summary.json`. Los commits posteriores a `0287182` solo documentan resultados; código y tests son idénticos. La rama diferida `feat/rival-short-i2i3` está publicada en `406f392`: sus tres archivos de I2–I3 son idénticos a `e37fbbb`, conservando `67d2f33` y `5d77420` como ancestros. Su diferencia sobre `352ebd2` es solo `RivalAi.cs`, `RivalShortBallTests.cs` y el meta. Sin PR para esa rama, sin push forzado y sin reescribir historial.
