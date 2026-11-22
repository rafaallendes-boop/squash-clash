# Rival frente al golpe corto — I-min detenido

22 nov 2026 · RAL-174 · Codex · base `origin/main` `518f9c3` · rama `feat/rival-short-min`.

**Estado: incompleto; no apto para fusionar ni para la build.** Se detuvo la primera corrida por un error de compilación en un test nuevo. No hay RED válido ni implementación del comportamiento. No se corrigió ni se reintentó, conforme a AGENTS §2 («Ante un defecto o sorpresa: registrar y detenerse, sin corregir ni reintentar para obtener Pass»).

## Alcance y preparación

Spec leída con `git show origin/docs/rival-corto-arq:docs/superpowers/specs/2026-11-22-rival-short-ball-design.md`, head `d320c13` de la PR #92. El pedido explícito de esta sesión incluye «incremento mínimo I-min (I1 a I3)», aunque la adenda HOY de la spec difiere I3. I4 y P1–P6 quedan fuera.

Se agregaron tests preparatorios de aproximación y cierre anterior al bote (I1), evento único antes de terminar el punto y guardia de recepción con golpe (PlayMode), defensa sin dos paredes y guardia `planned` (I2), alcanzabilidad y respaldo de menor retraso (I3), captura de huella de decisiones `planned` y hashes. `RivalAttempt.StepFor` es únicamente el esqueleto que devuelve −1 previsto para el primer RED. Las guardias aún no se ejecutaron; la huella aún no se capturó ni congeló.

## Evidencia y bloqueo

Comando: `scripts/test.sh editmode SquashClash.Core.Tests.Rally.RivalAttemptTests`.

Resultado exacto: **total=0 passed=0 failed=0 skipped=0**, salida 1; Unity no generó XML. Log:

```text
Assets/SquashClash/Tests/PlayMode/ZRivalShortBallTests.cs(40,105): error CS0234: The type or namespace name 'bounceThreshold' does not exist in the namespace 'SquashClash.Runtime.Physics' (are you missing an assembly reference?)
```

El identificador `Physics` del test nuevo se resolvió al namespace del producto. El fallo es del test escrito por Codex. No es una falla por aserción y no demuestra RED. Evidencia local ignorada: `Logs/editmode.log`, `Logs/editmode-console.log` y `Logs/evidence/rival-short-min/i1-red-console.txt`.

El candado se adquirió antes de lanzar Unity y se liberó mediante `trap` al terminar. La ruta de origen indicada para `device.env` no existía: se usó una copia del worktree de instalación, cotejada byte a byte con la del otro incremento y con la copia de destino; no se versionó.

## Puertas pendientes

R1/R2/R3 **no evaluados**; no se afirman valores 0. EditMode completo, PlayMode, AutoQA V/T3/volea y check-release no ejecutados después del bloqueo. Ningún test existente de calibración se cambió ni ejecutó; no hay cifras nuevas ni ajustes. R7 y Anexo A en main/rama no medidos; no se publicaron tablas vacías como resultados.

No hay cambios en valores de preset, RallyParams, física, solver, contrato, reloj, humano, FondoAi, `pullFullMm` ni calibración. Los hashes esperados en las guardias siguen siendo defaults `207da8f2c636`, RallyParams `a82c34b661bf` y SimulationKey `166a96e319c7`, **sin verificación de ejecución**. No hubo fusión, build nativa, instalación ni acceso al iPhone. El reloj ya marcaba 15:28 de Chile al inicio; la revisión de las ~15:00 no pudo cumplirse.
