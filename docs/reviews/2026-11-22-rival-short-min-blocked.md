# Rival frente al golpe corto — I1, intento visible

22 nov 2026 · RAL-174 · Codex · base `518f9c3` · PR #97 · gates sobre `0287182`.

**Solo I1, listo para revisión; todas las puertas verdes.** Decisión de SC-Lead, camino A: «la PR #97 queda solo con I1, el intento visible». Sin fusión ni instalación.

## Cambio

`RivalAttempt.StepFor` encuentra el paso de máxima aproximación entre pelota y esfera de alcance sobre el camino congelado, después del primer bote y hasta el segundo. Sin intento si el punto cerró antes del primer bote. Si no hay golpe planeado, el director emite una sola vez `RivalSwing(side, false)` al llegar a ese paso, antes de terminar el punto.

Es solo vista: conserva recepción, resultado, movimiento, energía y métricas. `RivalAi` vuelve exactamente a main y `RallySim` no cambia.

## Validación completa

| Puerta | Resultado |
|---|---|
| EditMode | **963/963 Passed**, 0 fallidos, 0 omitidos |
| PlayMode | **748/748 Passed**, 0 fallidos, 0 omitidos |
| AutoQA V | **102/102 tests**, 18 casos PASS; **PASS/0/nivel 1** |
| AutoQA T3 | **102/102 tests**, 18 casos PASS; **PASS/0/nivel 1** |
| AutoQA volea | **102/102 tests**, 18 casos PASS; **PASS/0/nivel 1** |
| check-release | **OK**, dev/prod build 1087, flags iOS OK |

**M3 volvió a Passed**, sin cambiar el test existente ni su criterio. XML frescos y sin errores CS. AutoQA: 0 FAIL, 0 INCONCLUSIVE, 0 NOT_EVALUATED; integridad y restauración verificadas, árbol limpio. Hashes intactos: defaults `207da8f2c636`, RallyParams `a82c34b661bf`, SimulationKey `166a96e319c7`.

TDD de I1: RED de paso 103 frente a −1 y evento 1 frente a 0, con plan `unreachable` confirmado; GREEN dirigido 2/2 EditMode y 2/2 PlayMode. Guardias de cierre anterior al bote y recepción con golpe nacieron verdes.

## I2–I3 preservados

`feat/rival-short-i2i3`, commit `406f392`, publicado **sin PR**. `RivalAi.cs`, `RivalShortBallTests.cs` y el meta son idénticos a `e37fbbb`; conservan `67d2f33` y `5d77420`. La rama parte de I1 para retomarlos después.

En #97 se retiraron con el commit nuevo `352ebd2`, sin reescribir historial ni forzar pushes. El fallo M3 de I1–I3 queda como antecedente en la [evidencia TDD y autorización](../validation/2026-11-22-rival-short-min-tdd.md). Logs y XML completos ignorados en `Logs/evidence/rival-short-min/i1-only-*/`.

Sin cambios en tests existentes, escenarios, presets, física, solver, contrato, reloj, humano, FondoAi, `pullFullMm` o calibración. Cada corrida tomó y liberó su propio candado. El commit final solo documenta resultados; código y tests son los mismos de `0287182`. Lectura visual y p95 pendientes del PO en iPhone; no se accedió al dispositivo.
