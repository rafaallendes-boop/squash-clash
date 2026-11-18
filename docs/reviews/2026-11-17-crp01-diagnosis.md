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

`baseline/run{1..10}.{xml,log,out}` y `summary.txt`; `exp-A.log` (SHA-256 `6f3027cd…`), `exp-B.log` (`286cdbd1…`), `exp-C.log` (`0ed517bf…`), `exp-D.log` (`8363fdba…`), `exp-session2-results.xml`. Experimentos en `Assets/SquashClash/Tests/PlayMode/ReticleVisibilityCrp01Experiment.cs`: `[Explicit]`, `Category("Experiments")`, en PlayMode porque el assembly de Experimentos no ve la preparación del fixture; la preparación se copió, el test original no se editó.
