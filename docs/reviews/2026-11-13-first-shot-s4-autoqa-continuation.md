# First Shot T4.2 — continuación de S4-42-AQ-01

Decisión literal del PO: **«DECISIÓN DEL PO: opción A. Autorizo ampliar la lista de campos predeclarados del lector de AutoQA (PocObservation) solo para los seis campos nuevos de 4.2. Primer commit: registrar esta decisión literal.»** Encargo completo registrado antes de tocar código en `bc5b212`, en [aprobaciones](2026-10-25-first-shot-po-approvals.md#continuación-literal-t42--pr-50--po-13-de-noviembre-de-2026). Misma rama/worktree `firstshot/s4-aids-tasks`, continuación desde `58adfc9`.

## RED original y clasificación autorizada

El PO designó como RED el fallo existente de AutoQAPocTests. XML original `Logs/autoqa/20261113T014843Z-6c149ad-23689/tests.xml`: **85 total, 83 Pass, 2 Failed, 0 skipped**. Ambos métodos `Early10_ReachesRealPresentation` y `Late10_ReachesRealPresentation` fallaron por aserción **Unknown ShotRecord field not predeclared**, enumerando AidCutoff, AidCutoffPlanVersion, AidCutoffTarget, AidLevel, CuePhase y Solved. Se conserva sin reejecutarlo para fabricar otro RED. [Informe histórico](2026-11-12-first-shot-s4-aids-tasks.md#corridas-completas-y-parada-s4-42-aq-01).

Commit `1bfab32`: Exact añade únicamente **AidLevel, AidCutoff, AidCutoffPlanVersion, CuePhase y Solved**. Excluded añade únicamente **AidCutoffTarget**, instante absoluto de reloj real como TargetContactAtReal. Relative, TimeToleranceMs, comparación, escenarios, expectativas, márgenes y tolerancias permanecen intactos. El lector sigue rechazando campos desconocidos y campos ausentes. No cambia ShotRecord ni producto en esta continuación. El GREEN de los dos métodos se verifica en la única corrida final de AutoQA.

## SL5 calculado desde la preparación completa

Primero se corrigió el diagnóstico de TimingAidConfigTests para construir `p.With("practice.launch",4).With("human.z",(double)1.6f)` desde DefaultPreset.json real; se imprime el hash calculado antes de cotejarlo con el literal solicitado. Unity dio **1/1 Pass, 0 failed/skipped**, con salida:

```text
S4_REAL_P0 effective=42d9efbf50fc inherited=8de3f2ecb217 bytesUnchanged=true
S4_HASH defaults=c0e9f00176d5 P0=42d9efbf50fc SL5=50efa6d33c0c
```

Evidencia nueva: `Logs/evidence/s4-aids-tasks/continuation/config-complete/{editmode-results.xml,editmode.log,editmode-console.log}`. Solo tras verificar esa salida y la aserción se modificaron los cuatro anclajes solicitados: Scenario.cs, autoqa-check v7 en .claude/agents/autoqa-operator.md, .claude/skills/autoqa/SKILL.md y scripts/tests/test_autoqa_checker.py. Commit separado `ff93db6`. SL5 **1877d083c345 → 50efa6d33c0c**; el primero era el overlay parcial, nunca la preparación completa. SL1 c0e9f00176d5 y SimulationKey 166a96e319c7 intactos. La receta de juego y la preparación de AutoQA no se modifican.

Regresión Python del checker: **5 métodos ejecutados GREEN**, incluido el método de cinco controles sintéticos pass/fail/canario/hash/environment; **1 método opcional skipped** por requerir copias históricas externas (`AUTOQA_REAL_RUNS`). Esos controles sintéticos no acreditan juego real.

## Verificación del head final y publicación de resultados

Este registro se guarda **antes** de las suites finales. Para que todas se ejecuten sobre el head final exacto, los resultados, horas, hashes de evidencia y estado definitivo se publican en el cuerpo de [PR #50](https://github.com/rafaallendes-boop/squash-clash/pull/50), sin commit documental posterior a esas corridas. Este texto no anticipa un PASS. Evidencia local ignorada separada: `Logs/evidence/s4-aids-tasks/continuation/final/`; AutoQA crea su propio directorio fresco bajo `Logs/autoqa/`. El reporte de etapa identifica el HEAD real y la fuente limpia.

Secuencia predeclarada: **un solo Unity a la vez**, EditMode completo, PlayMode completo, AutoQA mvp completo (PASS/salida 0/nivel 1 y canario FAIL esperado), lectura determinista v7 y check-release dev/prod con gitDirty=false. XML nuevos, sin CS/skipped; restaurar únicamente GraphicsSettings/QualitySettings transitorios y cotejar todos los demás archivos versionados. Ante otra sorpresa o defecto: preservar evidencia, registrar y detener sin reintentar. Los exports Unity de check-release son la comprobación solicitada, sin compilar nativo ni instalar.

La PR conserva [BLOCKED] hasta verificar todas las etapas; solo entonces se quita esa marca y se entrega lista para revisión de Claude coordinada por el PO, **SIN FUSIONAR**. No se envían mensajes a Claude. Sin cambios nuevos de producto, defaults, spec/planes, escenarios, expectativas, criterios, umbrales o tolerancias, ni iPhone, sesión física, Xcode/build nativa o instalación. La entrega no acepta First Shot. Los datos de la parada original permanecen históricos, con sus resultados sin convertirlos en Pass.
