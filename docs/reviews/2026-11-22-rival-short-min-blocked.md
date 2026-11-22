# Rival frente al golpe corto — I1, intento visible

22 nov 2026 · RAL-174 · Codex · base `518f9c3` · PR #97.

**Alcance vigente: solo I1; gates completos en curso.** Decisión de SC-Lead, camino A: «la PR #97 queda solo con I1, el intento visible». La PR continúa en borrador hasta completar todas las puertas. Sin fusión ni instalación.

## Cambio

`RivalAttempt.StepFor` encuentra el paso de máxima aproximación entre pelota y esfera de alcance sobre el camino congelado del rival, después del primer bote y hasta el segundo. Si el punto cerró antes del primer bote, no hay intento. Cuando no hay golpe planeado, el director emite una sola vez `RivalSwing(side, false)` al llegar a ese paso, antes de terminar el punto.

Es un evento de vista: no cambia el resultado de la recepción, el punto, movimiento, energía ni métricas. `RallySim` y `RivalAi` conservan su código de main.

## Separación de I2–I3

I2, I3 y `RivalShortBallTests.cs` (con su meta) están preservados en `feat/rival-short-i2i3`, commit `406f392`, sin PR. Los tres archivos son idénticos byte a byte a `e37fbbb`, incluidos los cambios de `67d2f33` y `5d77420`. La rama se apoya en I1 para poder retomarlos después.

En #97 se retiraron mediante `352ebd2`, un commit nuevo: `RivalAi.cs` vuelve exactamente a `518f9c3` y solo se retiraron los tests nuevos de I2–I3. Sin reescribir historial ni forzar pushes; ningún test existente, escenario, preset o criterio cambia.

## Validación

TDD previo de I1: RED de paso 103 frente a −1 y evento 1 frente a 0, con plan `unreachable` confirmado. GREEN dirigido **2/2 EditMode y 2/2 PlayMode**. Guardias de cierre anterior al bote y recepción con golpe nacieron verdes.

Pendientes para el alcance I1: EditMode completo (incluido M3), PlayMode completo, AutoQA V/T3/volea y check-release. Si falla cualquier test existente, se registra y se detiene.

El fallo M3 anterior (Fondista 3,9906 % frente a Definidor 1,4706 %, criterio Fondista < Definidor) corresponde a I1–I3 en `5d77420`, no a este alcance. Se conserva como historial en la [evidencia TDD](../validation/2026-11-22-rival-short-min-tdd.md).

## Límites

Sin cambios en física, solver, contrato, reloj, humano, FondoAi, `pullFullMm`, calibración ni parámetros. Hashes esperados intactos: defaults `207da8f2c636`, RallyParams `a82c34b661bf`, SimulationKey `166a96e319c7`. Cada corrida usa y libera su propio candado. Lectura visual de la estirada y p95 pendientes del PO en iPhone; no se accede al dispositivo.
