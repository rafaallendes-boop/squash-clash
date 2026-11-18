# CRP-01 — diagnóstico (fase A)

17 nov 2026 · rama `claude/crp01-diagnosis` sobre `main` `bddcef0` · Claude (Opus) dirigiendo un subagente; método declarado antes de ejecutar en `docs/validation/2026-11-17-crp01-diagnosis-method.md`. No se tocó producto, test existente, `DefaultPreset.json`, AutoQA ni ProjectSettings. Un solo Unity a la vez; ProjectSettings restaurado tras cada corrida.

## Resultado

**Causa identificada y reproducida de forma determinista, con las mismas cifras de CRP-01** (788,198900 frente a 674,663200). Es una carrera de layout en el propio test, no un defecto de física ni de timing.

## Hechos

1. **Tasa natural.** `scripts/test.sh playmode ReticleVisibilityTests`, N = 10: **0/10** reproducciones (9/9 en cada una). El test dura ≈0,7 s. Experimento A (cadencia original repetida, 100 comprobaciones): 0 fallos; B (espera 0,35 s antes de `ScrollTo`, 100): 0 fallos. La falla natural es rara y depende del reloj/carga, como en las tres ocurrencias previas.
2. **Mecanismo medido.** `ShotOverlay` refresca cada 0,1 s los cuatro labels `shot-preview-state/power/sampling/status` (multilínea) **solo si el panel DEV está abierto**. Esos labels están *encima* de los botones `s1-*` dentro de `parameters-scroll`. Con el panel recién abierto (primer botón de un fixture nuevo) están vacíos: `contentContainer` 1365,07 px, botón `s1-sampling` en y [598,81; 643,01], ya visible, `scrollOffset` 0, así que `ScrollTo` no desplaza nada. Cuando el temporizador de 0,1 s llena los labels, el contenido crece a 1510,26 px (+145,19) y el botón baja a [744,00; **788,20**], fuera del viewport (yMax 673,66). 788,20 es exactamente la cifra de CRP-01.
3. **Ventana.** En marcha estable el llenado cae 4 frames después de `Toggle`, justo después de la comprobación (frame 2 tras `ScrollTo`), por eso pasa. Si el llenado cae entre `ScrollTo` y la aserción, falla.
4. **Reproducción determinista.** C: tras `ScrollTo` y dos frames, esperar al llenado de labels → `WOULD_FAIL`, yMax 788,1989. D (barrido de fase): fixture nuevo, k frames de retardo antes de `Toggle`, k = 0..11: falla en **k = 3 y k = 9** (misma fase módulo 6 frames ≈ 0,1 s), k = 4 y 10 quedan a 1 px del límite (673,66, pasan); el resto pasan. Es decir, 1 fase de ≈6 falla.
5. Solo el **primer** botón (`s1-sampling`) de un fixture fresco puede fallar: después los labels conservan texto y la altura ya es estable (694 de 700 lecturas con 1510,26).

## Hipótesis (no demostradas)

- Que lo que desplaza la fase en las corridas completas sea un hitch de frame (carga de la suite, compilación de shaders, GC). Es coherente con que desaparezca al repetir, pero no se midió en una PlayMode completa. No se ejecutó la PlayMode completa (no hizo falta para la causa).
- H3 (ScrollTo antes de layout resuelto) y H4 (estado de tests previos, resolución): no necesarias; el layout ya estaba resuelto y el fixture se recrea.

## Opciones de corrección (decide el PO)

1. **Solo test, esperar estabilidad del layout.** Antes de `ScrollTo` (y/o antes de comparar) esperar a que `contentContainer.layout.height` y `scrollOffset` no cambien durante 2–3 frames consecutivos (o a que `shot-preview-power` tenga texto). No cambia aserciones ni expectativas; sí cambia la mecánica de espera (requiere su aprobación). Experimento B respalda que con el estado ya estable no falla.
2. **Solo test, reaplicar `ScrollTo` justo antes de medir** (tras `yield` y en el mismo frame de la aserción). Cambio mínimo, aserciones intactas; sigue teniendo una ventana mínima si el llenado cae entre ambos.
3. **Producto (solo DEV, `ShotOverlay`)**: llenar los labels al abrir el panel (llamar a `Refresh` desde `Toggle`) o reservar altura fija para ellos, de modo que el contenido no cambie de tamaño después de abrir. El test queda intacto; toca código DEV (no prod, no zona protegida de §4) y evita además el salto visual del panel al abrirlo.

Recomendación de Claude (no vinculante): 1, por ser la más acotada y no tocar producto.

## Evidencia (ignorada, no versionada) en `Logs/evidence/crp01/`

`baseline/run{1..10}.{xml,log,out}` y `summary.txt`; `exp-A.log` (SHA-256 `6f3027cd…`), `exp-B.log` (`286cdbd1…`), `exp-C.log` (`0ed517bf…`), `exp-D.log` (`8363fdba…`), `exp-session2-results.xml`. Experimentos (movidos después, ver «Corrección»; ahora en `docs/validation/evidence/2026-11-17-crp01/ReticleVisibilityCrp01Experiment.cs.txt`, con instrumentación `DIAG` añadida): `[Explicit]`, `Category("Experiments")`, en PlayMode porque el assembly de Experimentos no ve la preparación del fixture; la preparación se copió, el test original no se editó.

## Corrección (opción 3)

18 nov 2026 · decisión del PO, literal: «Sí, opción 3». Implementada por un subagente Sonnet dirigido por Claude; **la revisión de Claude sobre este trabajo no es independiente**.

**Cambio (solo DEV, assembly DevPanel, fuera de prod; no toca el test existente, `DefaultPreset.json`, AutoQA, física, timing ni gesto):** `ShotOverlay.RefreshNow()` (`elapsed=0; Refresh();`), llamado desde `DevPanelView.Toggle` solo cuando el panel queda abierto. Los cuatro labels multilínea quedan llenos en el mismo frame de la apertura, antes del primer layout, y la altura del contenido de `parameters-scroll` ya no crece a los 0,1 s. Commit `9305a75`.

**TDD:** nuevo `Tests/PlayMode/ShotOverlayOpenTests.cs` (mismo fixture 1206x2622 que el experimento, 13 casos): tras abrir y esperar un frame los labels tienen texto y la altura del contenido no cambia en 12 frames ni tras el tick de 0,1 s; y el barrido de fase k=0..11 mantiene `s1-sampling` dentro del viewport. RED por aserción (sin errores CS): 10/13 (fallan «shot-preview-state filled on open» y las fases k=2 y k=3 con yMax 788,198853 frente a 674,663208). GREEN tras el cambio: 13/13.

**Experimento D repetido sin cambiar su método** (12 fases): 0 fallidas; todas con yMax 673,663200 frente al límite 674,663200. Antes fallaban k=3 y k=9. Log `Logs/evidence/crp01/exp-D.log` (SHA-256 `43cd9ac1…1137`); el anterior queda como `exp-D-before.log`.

**Experimentos fuera de la regresión.** `scripts/test.sh` ejecuta la assembly completa con `-assemblyNames` y el runner de línea de comandos ejecuta también los tests `[Explicit]`; por eso los experimentos de esta PR entraron en el PlayMode completo (621 casos). AGENTS exige separar los experimentos de la regresión, así que `ReticleVisibilityCrp01Experiment.cs` se movió (junto con la eliminación de su `.meta`) a `docs/validation/evidence/2026-11-17-crp01/ReticleVisibilityCrp01Experiment.cs.txt`, sin compilar. `ShotOverlayOpenTests` sí queda en la regresión.

**Falla de B en el PlayMode completo (no concluyente).** Una corrida completa con el fix (621 casos) falló solo en `B_WaitBeforeScroll_Repeated` (línea 130: el panel DEV seguía abierto tras activar el botón). Sin el fix (`7d3ddeb`) la suite completa dio 608/608. B aislado pasa 3/3 con fix y 3/3 sin él. Lectura del código: el fix no reconstruye elementos (`Refresh()` solo asigna texto/estilos) ni toca la ruta de cierre (el handler de botón llama a `Toggle` en estado abierto y `RefreshNow` solo corre al abrir); `Toggle` deja el panel abierto únicamente si `CanToggle()` es falso. Se instrumentó B para registrar `CanToggle()` y sus componentes (reanudar, calibración, spike, rendimiento), el estado de sampling/protocolo y el test anterior; con la instrumentación se corrió el PlayMode completo con el fix **2 veces: 621/621 y 621/621, sin reproducir**, con `canToggle=True` en todas las lecturas (0 con falso). Se trata como interacción de orden/tiempo entre experimentos no reproducible; la falla no se atribuye ni se descarta como del fix con certeza. Al salir los experimentos de la regresión, la condición deja de existir en la suite.

**Puertas sobre el head de código `3a1953f` (árbol limpio):** EditMode 803/803; PlayMode 606/606 (`git checkout -- ProjectSettings` después); AutoQA `--suite v` PASS, exit 0, nivel 1, 18 PASS/0 FAIL/0 INCONCLUSIVE/0 NOT_EVALUATED, runId `20261118T111504Z-3a1953f-v-18582`, dirty false; AutoQA `--suite t3` PASS, exit 0, nivel 1, 18/0/0/0, runId `20261118T111521Z-3a1953f-t3-2595`, dirty false; `scripts/check-release.sh` OK (dev y prod).
