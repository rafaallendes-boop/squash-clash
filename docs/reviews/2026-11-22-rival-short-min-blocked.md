# Rival frente al golpe corto — I-min detenido

22 nov 2026 · RAL-174 · Codex · base `origin/main` `518f9c3` · rama `feat/rival-short-min` · PR #97.

**Estado: incompleto, no compila y no es apto para la build.** Hay un RED válido de la función pura de I1. Su primera implementación falló al compilar; se detuvo sin corregir ni reintentar, conforme a la condición expresa de SC-PM. I1 en el director e I2–I3 siguen pendientes.

## Alcance y autorización

Spec leída con `git show origin/docs/rival-corto-arq:docs/superpowers/specs/2026-11-22-rival-short-ball-design.md`, head `d320c13` de #92. El pedido de esta sesión incluye «incremento mínimo I-min (I1 a I3)», aunque la adenda HOY difiere I3. I4 y P1–P6 quedan fuera.

SC-PM, 22 nov 17:58 −03 (hora indicada en su mensaje), autorizó: «Corrige solo las referencias que impiden compilar» y «Repite el RED una sola vez». Condición literal: «Si vuelve a fallar algo que no sea una aserción esperada, regístralo y detente». Se calificó únicamente `UnityEngine.Physics.bounceThreshold` en el test nuevo de PlayMode; ninguna aserción, escenario, umbral ni test existente fue modificado.

## Resultados exactos

| Corrida | Total / Passed / Failed / Skipped | Resultado |
|---|---|---|
| RED inicial, 15:39 | 0 / 0 / 0 / 0 | CS0234 en el test nuevo; sin XML |
| Único RED autorizado, 17:56 | 2 / 1 / 1 / 0 | XML fresco y sin errores CS; aserción esperada: paso 103 frente a −1 |
| Primer GREEN de I1, 17:57 | 0 / 0 / 0 / 0 | CS1529 en código nuevo; XML anterior rechazado |

Comando en las tres: `scripts/test.sh editmode SquashClash.Core.Tests.Rally.RivalAttemptTests`. Salida 1 en las tres, con motivos distintos. La guardia `NoneWhenPointClosedBeforeBounce` nació verde en el RED autorizado.

Bloqueo actual:

```text
Assets/SquashClash/Core/Rally/RivalAttempt.cs(34,1): error CS1529: A using clause must precede all other elements defined in the namespace except extern alias declarations
```

El parche de Codex dejó `using System;` al final del archivo. Se conserva sin corregir. El XML copiado en la carpeta de este GREEN es el XML anterior, **no evidencia de una ejecución nueva**. Evidencia local ignorada: `Logs/evidence/rival-short-min/compile-blocker/`, `i1-red-authorized-console.txt`, `editmode-results.xml`, `editmode.log`, `i1-green-console.txt` e `i1-green/`.

Cada corrida adquirió el candado y lo liberó mediante `trap` al terminar. `device.env` fue copiado sin cambios y no se versionó; la ruta original faltaba y se cotejaron byte a byte las copias de los worktrees de instalación y del otro incremento.

## Puertas y límites

R1/R2/R3 globales **no evaluados**; no se afirman valores 0. EditMode completo, PlayMode, AutoQA V/T3/volea y check-release pendientes. Ningún test existente de calibración se cambió ni ejecutó; no hay cifras nuevas ni ajustes. Huella `planned`, R7 y Anexo A en main/rama no medidos.

Sin cambios en preset, RallyParams, física, solver, contrato, reloj, humano, FondoAi, `pullFullMm` o calibración. Hashes esperados, aún sin verificación ejecutada: defaults `207da8f2c636`, RallyParams `a82c34b661bf`, SimulationKey `166a96e319c7`. No hubo fusión, build nativa, instalación ni acceso al iPhone. La PR permanece en borrador.
