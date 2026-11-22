# Evidencia TDD — rival ante el corto, I1–I3

Base `518f9c3`, rama `feat/rival-short-min`, PR #97. Spec de #92 leída con `git show`, head `d320c13`.

## Autorización vigente

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
