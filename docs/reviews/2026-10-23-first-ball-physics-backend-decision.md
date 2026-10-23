# I4e — decisión del backend de física de First Ball

Decisión del PO del **2026-10-23**, tras aprobación de Claude de PR #6 / T4.5–I4d: **P2-Analytic es el backend de First Ball**, según §8.2.3 de la spec v0.3, la alternativa más simple que cumple todos los invariantes y umbrales. P1 falla C3, C10, C11 y C17 y se conserva en código como respaldo configurable, sin usarse por defecto. No se cambian umbrales, escenarios, criterios, geometría, fricción ni semántica de Reset.

I4d cerrado: PR #6 marcada lista y fusionada con merge commit **9d508595c51bbf07b381640f4ba4c65df2c07786**, head d7687d5, main sincronizada y cierre registrado en AGENTS/aprobaciones (508d333). [Revisión final y observaciones Claude](2026-10-23-first-ball-physics-spike-P2.md#revisión-final-de-claude-y-decisión-del-po--2026-10-23), [decisión previa B / I4c](2026-10-23-first-ball-physics-decision.md), [aprobaciones PO](2026-10-21-first-ball-po-approvals.md#cierre-de-i4d-y-gate-de-backend--autorización-i4e--2026-10-23).

## Evidencia y comparación de los 18 criterios

Se conservan las campañas ya revisadas, todas con configuración **c86243b15a97**, seed **20261020**, 10.053 lanzamientos y los mismos escenarios/criterios/umbrales. P1 nativo: dev 2fdede5, baseline/ccdDyn-co1mm-it6-1; P2 editor: c8e5923 en EditMode/PlayMode; P2 nativo: dev 557d3ff, analytic/r2-20261023. Fuentes: [P1 nativo final](../validation/evidence/2026-10-23-i4b-native-compliant-run.json), [P2 editor](../validation/evidence/2026-10-23-i4d-p2-editor-runs.json), [P2 nativo y 200 muestras](../validation/evidence/2026-10-23-i4d-p2-native-run.json), [informe comparativo P2](2026-10-23-first-ball-physics-spike-P2.md). No se repite campaña en I4e.

| Criterio | P1 nativo final | P2 editor (ambos modos) | P2 nativo |
|---|---|---|---|
| C1 | Pass, 0 cruces/10.053 | Pass, 0 cruces/10.053 | Pass, 0 cruces/10.053 |
| C2 | Pass, 67.737 evaluados | Pass, 67.825 evaluados | Pass, 67.825 evaluados |
| C3 | Fail, 5/3.501 | Pass, 0/1.701 | Pass, 0/1.701 |
| C4 | Pass, 0/4 fallos | Pass, 0/4 fallos | Pass, 0/4 fallos |
| C5 | Pass, 0/117 | Pass, 0/117 | Pass, 0/117 |
| C6 | Pass, 0/10.017 | Pass, 0/10.017 | Pass, 0/10.017 |
| C7 | Pass, 0/18.753; NA 1.279 | Pass, 0/18.797; NA 1.235 | Pass, 0/18.797; NA 1.235 |
| C8 | Pass, 0/2 | Pass, 0/2 | Pass, 0/2 |
| C9 | Info, 0/2 | Info, 0/2 | Info, 0/2 |
| C10 | Fail, máx. 13,6164722 mm | Pass, máx. 0,0009536743 mm | Pass, máx. 0,0009536743 mm |
| C11 | Fail, 3/6 | Pass, 0/6 | Pass, 0/6 |
| C12 | Info, 11 medidos | Info, 11 medidos | Info, 11 medidos |
| C13 | NotEvaluated, 35/36 | Info, 40/40 | Info, 40/40; mín. 1, mediana 1,00529957 |
| C14 | Info, máx. 2; tope no configurable | Info, máx. 3; tope 4, 0 alcanzados | Info, máx. 3; tope 4, 0 alcanzados |
| C15 | Info, Estimated; pasos distintos | Info, Exact; mismo paso S4/S6 | Info, Exact; mismo paso S4/S6 |
| C16 | Info, 11,3993931 mm | Info, 1,86264515E−05 mm | Info, 1,86264515E−05 mm; 1.217.571 persistentes |
| C17 | **Fail, p95 14,3221 ms** | Info, sin extrapolar | **Pass, p95 0,0114 ms** |
| C18 | Info, RunsInEditMode=False | Info, RunsInEditMode=True | Info, RunsInEditMode=True |

P2 pasa los **11 criterios de gate nativos** (C1–C8, C10, C11, C17); los siete restantes son informativos. En editor pasan los criterios de gate aplicables y C17 sigue **Info**, sin extrapolarlo ni compararlo con nativo. C17 nativo: mediana 0,0086 ms, p95 0,0114 ms, p99 0,1272 ms, máximo 0,1587 ms; 20 calentamientos y 200 muestras guardadas, percentiles nearest rank recalculados. PO confirmó batería inicial 80 % y condiciones aprobadas; térmico nominal inicial/benchmark/final y cero interrupciones de foco. P1 conserva sus fallos y C13 NotEvaluated; no se reinterpretan.

## Aplicación del default y respaldo

I4e en `firstball/i4e-physics-backend`, con PR para revisión de Claude. DefaultPreset.json sigue como **única fuente de defaults**: se cambia únicamente physics.backend de **0 (P1-PhysX)** a **1 (P2-Analytic)**. El catálogo admite las dos opciones y EffectiveConfig resuelve sus nombres; el bootstrap registra ambas fábricas y el host elige por la configuración efectiva del lanzamiento. Sin fallback silencioso a P1. La configuración del experimento histórico P2 conservaba backend=0 porque la fábrica P2 se inyectaba explícitamente; su hash/evidencia no se reescriben al cambiar el default del host.

Hash nuevo del preset efectivo: **f6a394a12daf**, confirmado por PresetCodec/EffectiveConfig en el test de arranque del host; anterior **c86243b15a97**. Los demás valores permanecen idénticos; no se promueve un preset calibrado ni se cambia variante P1. TDD de configuración y host por defecto/P1 configurable completado: **dos RED válidos→GREEN; suites completas EditMode 222/222 y PlayMode 63/63**, cero fallos/omitidos. [XML/log hashes y conteos](../validation/evidence/2026-10-23-i4e-backend-tests.json). El host normal crea juego/predictor P2, llega a dos botes y mantiene predicción bit a bit; el cambio P2→P1→P2 crea las instancias correctas. Se conservan pruebas específicas P1 con selección explícita. Build dev 8fb5d0c instalada y arranque corto nativo verificado: backend=P2-Analytic, config=f6a394a12daf, cero escenas PhysX y cero excepciones observadas. [Build y arranque](../validation/i4e-physics-backend.md), [evidencia](../validation/evidence/2026-10-23-i4e-native-smoke.json). Observación visual breve del rebote por PO pendiente. Es una comprobación de integración, **no aceptación completa ni campaña física nueva**.

## Observaciones conservadas y siguiente gate

Observaciones de Claude aceptadas sin cambio de código físico: (a) Reset de P2 presupone un estado dentro de cancha; el futuro golpe debe garantizarlo o se validará al existir ese inyector, precondición I5+; (b) bajo bounceThreshold se mantiene fricción μ·(1+e)·|v_n| literal según adenda, anotada para calibración; (c) acoplamiento SpikeRun/AnalyticBallSimulationFactory para C14 aceptable en First Ball. Claude verificó que el redondeo al float más cercano es monótono y los planos son floats exactos, sin riesgo de atravesarlos al redondear al final del paso.

**Entrada preparada para T7.3, sin escribir en la spec:** «Decisión PO 2026-10-23: First Ball usa P2-Analytic según §8.2.3 por cumplir invariantes y umbrales con la alternativa más simple evaluada. P1-PhysX permanece como respaldo configurable; no es el default. Rige la adenda r2 de T4.5, incluida precisión de impactos lentos en paredes.» La incorporación a spec requiere autorización PO en T7.3; este PR no la modifica.

La prueba de aceptación completa de P2 queda para **T7.1**. **I5 no autorizado.** Detenerse cuando PR I4e esté lista para Claude; no fusionarla por cuenta propia. No se adelantan panel completo, calibración, golpe o gameplay posterior.
