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
