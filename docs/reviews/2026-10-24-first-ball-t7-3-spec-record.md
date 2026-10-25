# T7.3 — registro autorizado en la spec v0.3

**T7.3 cerrado** tras aprobación de Claude y condición PO cumplida: PR #15 fusionada con merge commit **e192b36e4d60029f7ebb71507485a266d69bfd25**, head **b124cdfaa1147ba2e54c4b67f72e17807b99fa41**. Main sincronizada; worktree eliminado y artefactos ignorados preservados en Logs/archive-t7-3-20261024. La entrega inicial descrita abajo se conserva como histórico. [Autorización PO del 2026-10-24](2026-10-21-first-ball-po-approvals.md#autorización-r1-de-retención-y-secuencia-posterior-t73--2026-10-24), habilitada tras [cerrar R1](2026-10-21-first-ball-po-approvals.md#cierre-r1--2026-10-24): PR #14 fusionada con merge commit **bb0f939947991d801afb45b0c0c32005c677b9ea**, head **3ccae954d291c287b98ac68a25133c819777e320**. Main de partida **20ed51b** incorpora cierre, preservación de artefactos ignorados y eliminación del worktree R1.

## Registro y decisiones de origen

Se modifica únicamente documentación. La [spec v0.3](../superpowers/specs/2026-10-20-squash-clash-design-v0.3.md) incorpora el registro posterior a R3 en §0.3 (D30–D35); D15/D27 conservan el proceso original y D30 registra su resultado para First Ball. Cada bloque modificado cita la decisión de origen y la autorización para registrarla.

| Registro | Sección | Decisión de origen |
|---|---|---|
| P2-Analytic backend de First Ball por §8.2.3; P1 falla C3/C10/C11/C17, respaldo configurable sin default | §0.3 D30 y §8.2 | [Gate PO 2026-10-23 y 18 criterios](2026-10-23-first-ball-physics-backend-decision.md) |
| §8.3.1–3/8.3.7 medidos C12–C15 sin umbral nuevo; tope no operativo P1 y operativo P2 | §0.3 D31 y nota §8.3 | [D-4](2026-10-20-first-ball-plan-review-response-claude.md#41-decisiones-del-po-antes-de-autorizar-la-implementación), adenda r2 y evidencia P2 |
| DefaultPreset.json única fuente de defaults, esquema/tipos/rangos | §0.3 D32, §13.4 y anexo A | [D-5](2026-10-20-first-ball-plan-review-response-claude.md#41-decisiones-del-po-antes-de-autorizar-la-implementación), directriz 11 y autorización T7.3 |
| Diagonal lateral 50 mm perpendiculares, borde inferior de referencia | §0.3 D33 y §8.7.2 | [D-6](2026-10-20-first-ball-plan-review-response-claude.md#41-decisiones-del-po-antes-de-autorizar-la-implementación), autorización T7.3 |
| Aceptación First Ball con desviación H17 p95 17,2/16,8 ms frente a ≤16,7; ambos Fail, umbral [EXPERIMENTO], sin restar sesgo ni repetir | **Solo §0.3 D34** | [Decisión PO I7a](2026-10-21-first-ball-po-approvals.md#revisión-final-claude-y-aceptación-po-de-first-ball--2026-10-24) |
| No promover P0-calib-1; única diferencia repeatSeconds=3 no física, defaults 0d39e6016bd8 intactos | §0.3 D35 y aclaración de promoción autorizada en §13.4 | [Decisión PO paso 2 T7.3](2026-10-21-first-ball-po-approvals.md#autorización-r1-de-retención-y-secuencia-posterior-t73--2026-10-24) |

La aceptación H17 conserva el cumplimiento de p99/frames largos/térmico/DroppedSteps y el fundamento corregido tras revisión de Claude: distribución estrecha en torno a 16,7 ms; frames >33 ms: 1 en H17-1 (0,067/min) y 0 en H17-2, ambos dentro de ≤1/min. Interpretación de Claude, no demostrada: oscilación de la cadencia de presentación más que carga; CPU p95 = cpuFrameTime incluye la espera de presentación. **No convierte el Fail en Pass**. §17.C y §17.D quedan byte a byte intactos. No se incorpora la contradicción C-3 de la respuesta a Claude ni otra ampliación fuera de D-4/D-5/D-6 autorizadas.

## Verificación del tope P2 (D-4)

Verificación de lectura del código y la evidencia existente, sin cambiar código ni ejecutar nueva campaña:

- [EffectiveConfig.Physics](../../Assets/SquashClash/Core/Config/EffectiveConfig.cs) transmite `physics.maxImpactsPerStep` a PhysicsParams; la [fábrica P2](../../Assets/SquashClash/Core/Simulation/AnalyticBallSimulationFactory.cs) valida que sea positivo y lo pasa a la simulación.
- [AnalyticBallSimulation](../../Assets/SquashClash/Core/Simulation/AnalyticBallSimulation.cs) declara ImpactCapConfigurable=true, copia el valor y corta el bucle al alcanzar el tope. Queda en el último impacto con velocidad reflejada, descarta el remanente, avanza el reloj un paso y cuenta cada tope; los simultáneos son un instante atómico. Cumple la [adenda r2](2026-10-23-first-ball-physics-decision.md).
- [Pruebas existentes](../../Assets/SquashClash/Tests/EditMode/Simulation/AnalyticBallSimulationTests.cs): ImpactCap_DiscardsRemainderWithReflectedVelocityAndCountsEveryReachedCap, ImpactCap_SimultaneousGroupIsAtomicAndNeverIntegratesThroughWall e ImpactCap_CountsRepeatedCappedStepsAndStopsAtLastImpact fuerzan topes **1/2**, comprueban posición/velocidad/paso y contador (incluido Reset). La suite completa fresca de R1, misma física, pasó 287/287 EditMode.
- [SpikeRun C14](../../Assets/SquashClash/Core/Spike/SpikeRun.cs) informa el valor y contador de topes alcanzados de toda la campaña, excluyendo el benchmark. [Campaña P2 revisada](2026-10-23-first-ball-physics-spike-P2.md): **tope efectivo 4, 0 alcanzados, máximo 3 contactos no persistentes/paso**. Ese cero no sustituye las pruebas que sí fuerzan el límite. P1 conserva su tope no configurable; C12–C15 siguen informativos, sin umbrales nuevos ni reinterpretación del C13 NotEvaluated de P1.

## Defaults y comprobación documental

El paso 2 queda resuelto por decisión explícita del PO: **no se promueve P0-calib-1**. Punto de control aceptado de Etapa 5, hash efectivo **8de3f2ecb217**; solo `launcher.repeatSeconds=3` frente a Default (0), sin diferencia física. [Registro y evidencia del punto de control](2026-10-21-first-ball-po-approvals.md#cierre-i6a-punto-de-control-etapa-5-y-autorización-i6b--2026-10-24). Default efectivo **0d39e6016bd8**, P2 por defecto, archivo DefaultPreset.json SHA-256 **40fae0dc80c77753e0980da5fdf5396ac9f7f5c5bee2d098ffa1169a4810f12f**, sin modificación.

Comprobación del diff contra main: solo AGENTS.md y docs/**; cero cambios en Assets/**, ProjectSettings/**, scripts/** ni plan r2. §8.2.1/§8.2.2/§8.2.3, los siete requisitos numerados de §8.3 y toda §17 se conservan exactamente. Verificación documental completada: **26 enlaces locales nuevos** (archivos y anclas) válidos; secciones protegidas comparadas byte a byte y git diff --check limpio. Auditoría local en Logs/t7-3-document-audit.json, no versionada. **Sin ejecutar tests, builds, instalación o mediciones nuevas:** no hay comportamiento de producto modificado; la regresión/exports frescos corresponden al cierre R1. No se actualizan herramientas ni el perfil; antes de una instalación desde el 27 debe renovarse y siempre comprobarse vigencia.

## Entrega inicial (histórico)

T7.3 documental preparado en [PR #15 lista para Claude](https://github.com/rafaallendes-boop/squash-clash/pull/15), abierta y **sin fusionar**. Fuente documental 5507c8e; este agregado solo enlaza la entrega. No se cambian escenarios, criterios, umbrales ni defaults. **Gameplay y T8 siguen sin autorizar. Detenerse al entregar este informe y la PR lista.**

## Corrección factual tras revisión de Claude

Claude aprobó head **6bc5084b0a9140aba381ee4f8dae46fbde2da476** con la precisión factual H17 indicada arriba; error de origen en el mensaje de Claude, no de Codex. [Corrección y autorización condicionada del PO](2026-10-21-first-ball-po-approvals.md#revisión-claude-de-t73-y-fusión-condicionada-por-el-po). La spec cambia únicamente D34; las tablas históricas §18.2 B5/resumen v0.3 quedan fuera de alcance y sin cambios. Se revisan todos los documentos de esta PR para conservar los datos de frames largos de H17-1/H17-2 y distinguirlos de la interpretación no demostrada de Claude. Sin código, tests, build, instalación ni mediciones nuevas; Fail/umbrales/defaults conservados. Condiciones del PO verificadas: cuatro archivos documentales, en spec únicamente D34, §17 byte a byte intacta; ningún cambio en plan ni producto. Auditoría local Logs/t7-3-final-review-audit.json. Fusión autorizada con merge commit; tras cierre detenerse sin iniciar gameplay/T8.

## Cierre autorizado por el PO

**PR #15 ya lista y fusionada**, merge commit **e192b36e4d60029f7ebb71507485a266d69bfd25**, head **b124cdfaa1147ba2e54c4b67f72e17807b99fa41**. Auditoría desde 6bc5084: cuatro archivos documentales, spec únicamente D34, §17 intacta y sin plan/producto; diff SHA-256 **54cb9e595d703e4bcb626abc140c2180e30882c31b779a17ef659eb84c62bf34**. Main sincronizada y cierre registrado en AGENTS/aprobaciones. Worktree eliminado sin force, Logs/ preservado en **Logs/archive-t7-3-20261024**; ramas remotas intactas. Sin tests/build/instalación/medición nuevos.

**T7.3 cerrado; detenerse. Gameplay/T8 siguen sin autorizar.** Pendientes: comprobar P0-calib-1 en próxima apertura y renovar perfil antes de cualquier instalación desde el 27, verificando vigencia. No se actúa sobre el iPhone en este cierre.
