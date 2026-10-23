# T4.5 / I4d — P2 analítico y comparación con P1

Estado, 2026-10-23: **implementación y regresión GREEN; experimentos completos de P2 en EditMode y PlayMode; build dev 557d3ff instalada, campaña nativa y entrega final pendientes**. Rama `firstball/i4d-p2-analytic`, PR de física propia. P1 sigue como respaldo; backend definitivo pendiente del gate con datos de P2. Sin elección, congelación/promoción, spec modificada ni I5. No se fusiona esta entrega por cuenta propia.

## Autorización y contrato

I4c aprobado por Claude, comunicado por el PO, y fusionado con merge **ac5e195e6ac2c8f31a2491492a5980a3c7c88acd**; cierre en main ef25563. [Decisión B](2026-10-23-first-ball-physics-decision.md), [adenda r2 de T4.5](../superpowers/plans/2026-10-20-squash-clash-first-ball.md#adenda-r2--contrato-de-p2-aprobado-por-el-po-el-2026-10-23) y [aprobaciones](2026-10-21-first-ball-po-approvals.md#cierre-de-i4c-y-comienzo-de-t45--2026-10-23). La precisión de paredes aprobada por el PO a propuesta de Claude está incorporada: por debajo de bounceThreshold se anula la componente normal, sin reposo en paredes.

`AnalyticBallSimulation` y su fábrica en Core: **P2-Analytic**, `Timing=Exact`, `ImpactCapConfigurable=true`, `RunsInEditMode=true`. Configuración y materiales copiados por instancia; Reset deriva todo el estado físico del BallState y reinicia su contador de tope. No se usa historia oculta para apoyo, no se modifica gravedad global ni se elige P2 para el host normal. `DefaultPreset.json` sigue como única fuente de defaults; el hash de configuración es **c86243b15a97**.

- Vuelo exacto, raíz cuadrática para Floor y tiempos lineales para paredes; consumo del resto del paso tras impacto. Paredes evaluadas según altura del centro al impacto; Floor ilimitado y OverTop del runner sin cambios. No se modela borde superior.
- ε de simultaneidad **1e−6 s**, superficies en orden enum. Detalle de implementación para revisión: el conjunto simultáneo es un instante atómico; se resuelven sus normales desde la misma velocidad de llegada y la fricción actúa sobre la tangente común con la suma de μ·(1+e)·|v_n|, sin modificar las normales ya reflejadas. No se afirma igualdad numérica con PhysX.
- Apoyo inferible de y=r y v_y=0, sin aceleración vertical; contacto persistente Floor en cada paso apoyado. Deslizamiento con μ·g integrado hasta detenerse, sin inversión; barrido de pared también durante deslizamiento. Rebote lento en paredes anula solo la normal: la gravedad sigue actuando.
- Al alcanzar MaxImpactsPerStep se deja la posición del último impacto y la velocidad reflejada, se descarta el resto y avanza un paso. La unidad del tope es el instante atómico, incluidos simultáneos; C14 informa además contactos no persistentes/paso y todas las veces que se alcanzó el tope en la campaña, excluyendo el benchmark separado. Las pruebas fuerzan topes 1/2; la campaña con tope efectivo 4 informa 0 alcanzados.
- bounceThreshold se recibe de la misma fuente efectiva de P1, **UnityEngine.Physics.bounceThreshold=0.1 m/s**; sin parámetro nuevo en preset. Fricción del material por multiplicación con 1, estática igual a dinámica, y regla de impacto de la adenda. Ninguna compensación o cambio de umbral para aprobar.

## Regresión y TDD

**EditMode 221/221, PlayMode 61/61**, cero fallos/omitidos. [Once grupos RED→GREEN y hashes XML/log](../validation/evidence/2026-10-23-i4d-p2-tests.json): impacto dentro del paso, esquinas dentro de ε, reposo/deslizamiento/umbral en paredes, tope, límites superiores, fricción, copia de configuración, contadores/muestras, puente nativo/encabezado y salida cancelada/nombre de archivo. Los RED fueron aserciones con XML fresco, sin errores CS; no se cuentan como aprobación física.

Las 200 muestras individuales C17 se guardan dentro del informe, en orden de adquisición y con precisión round-trip. El cronómetro mantiene Reset+TrajectoryRunner, sin creación de escena, 20 calentamientos y 200 medidas, nearest rank; copiar/formatear muestras ocurre fuera del cronómetro. Criterios, comparador, escenarios, seed, preset y umbrales permanecen intactos; P1 no modificado.

## Experimentos completos del editor

Mismo **SpikeRun/PredictionBenchmarkRun**, 17 fijos +10.000 S8 +36 S8d = **10.053 lanzamientos** por ejecución, seed **20261020**, configuración **c86243b15a97**, variante **analytic/r2-20261023**, código **c8e5923**, Unity 6000.3.27f1. La única modificación de código posterior a estas campañas es el prefijo P2 del archivo guardado por DevPanel; los archivos físicos y de medición no cambian.

[EditMode íntegro](../validation/evidence/physics-spike-P2/editor-c8e5923-editmode.md) y [PlayMode íntegro](../validation/evidence/physics-spike-P2/editor-c8e5923-playmode.md), **1/1 cada uno**, liberación de instancias, número de escenas y gravedad/bounceThreshold global sin cambios. [Manifiesto con 18 criterios y 200 muestras por ejecución](../validation/evidence/2026-10-23-i4d-p2-editor-runs.json). Los percentiles y medianas se recalcularon desde las 200 muestras y coinciden con los agregados a seis decimales.

Todos los criterios de gate del editor pasan; C17 allí es **Info**, no certifica el iPhone. C1 cero cruces; C3 0/1.701 fallos, 8.338 no aplicables, ganancia máxima 0; C5 0/117, C6 0/10.017, C7 0/18.797 medidos y 1.235 no aplicables, cero no evaluables. C10 0/9, error máximo **0,0009536743 mm**; C11 0/6. C13 40/40 medidos, mínimo 1 y mediana 1,00529981; informativo, sin modificar su fórmula. C14 máximo 3 contactos no persistentes/paso, tope 4 configurable y 0 alcanzados. C15 S4/S6 en el mismo paso, Exact. C16 **1,86264515E−05 mm**, informativo; C18 EditMode=True. Sin igualdad exigida entre backends o plataformas.

Condiciones C17 del editor: un proceso Unity a la vez, worktree propio, actividad normal del Mac, sin aislamiento térmico/carga. EditMode p95 **0,020300 ms**, PlayMode p95 **0,018900 ms**, 200 medidas cada uno. No se extrapolan ni comparan con nativo. Las condiciones históricas de P1 (34,9 ms; antes 24,1 ms) siguen en su informe, sin reinterpretarlas.

El primer intento EditMode produjo **0 tests** y ningún informe: rechazado, no cuenta como completo. El ensamblado de experimentos original era PlayMode; se añadió una entrada exclusiva Editor que llama al mismo método. Verificado en código instalado Unity Test Framework 1.6.0 y en la [documentación oficial de Unity sobre ensamblados EditMode](https://docs.unity.com/en-us/engine/6000.7/manual/programming-environment/test-framework-introduction/getting-started/edit-mode-vs-play-mode-tests). La ejecución válida posterior 1/1 se conserva aparte.

## Campaña nativa P2 pendiente

Autorizada una campaña, después de los experimentos del editor. El botón **«Prueba física P2»** selecciona P2 solo para el experimento; host normal/escenarios visuales siguen con P1, sin cambiar catálogo o preset. Se conserva liberación/restauración del host y la política de foco/térmico aprobada. Una cancelación se reporta INCOMPLETO, incluso antes de crear runner.

La próxima build dev incorpora ProjectPhysics en BuildInfo desde el asset efectivo de exportación y lo imprime en el encabezado, incluido **fastMotionThreshold**, marcado **relevante solo para P1**. La fábrica P2 usa el bounceThreshold efectivo de runtime. No se afirma getter runtime de fastMotionThreshold. Las 200 muestras C17 se guardan en el mismo archivo `P2-…md` y permiten recalcular estadísticas tras recuperarlo.

Antes de instalar: export limpio, no Development, Release ARM64 firmado, metadata/perfil verificados y prod sin DevPanel/puente nativo. Renovar perfil si la instalación ocurre el 27 de octubre o después, sin cambiar herramientas. PO ejecuta con teléfono físico, No molestar, batería ≥80 %, bajo consumo apagado, sin funda y térmico nominal al inicio; sin capturas/duplicación/devicectl/herramientas durante ejecución. Recuperar Documents/spike solo tras aviso de término; auditar 18 filas/hash/config/variante/200 muestras/térmico/foco. C17 mantiene **p95 ≤1 ms**, Fail si incumple. No se repite I4b.

## Comparación de 18 criterios — estado parcial

P1 corresponde a su campaña nativa final bajo condiciones aprobadas; P2 editor a sus dos experimentos completos. Las columnas son resultados separados; no son comparaciones bit a bit entre backends/plataformas. **Falta P2 nativo para completar la entrega.** [P1 final](../validation/evidence/2026-10-23-i4b-native-compliant-run.json).

| Criterio | P1 nativo final | P2 editor (ambos modos) | P2 nativo |
|---|---|---|---|
| C1 | Pass, 0 cruces/10.053 | Pass, 0 cruces/10.053 | Pendiente |
| C2 | Pass, 67.737 evaluados | Pass, 67.825 evaluados | Pendiente |
| C3 | Fail, 5/3.501 | Pass, 0/1.701 | Pendiente |
| C4 | Pass, 0/4 fallos | Pass, 0/4 fallos | Pendiente |
| C5 | Pass, 0/117 | Pass, 0/117 | Pendiente |
| C6 | Pass, 0/10.017 | Pass, 0/10.017 | Pendiente |
| C7 | Pass, 0/18.753; NA 1.279 | Pass, 0/18.797; NA 1.235 | Pendiente |
| C8 | Pass, 0/2 | Pass, 0/2 | Pendiente |
| C9 | Info, 0/2 | Info, 0/2 | Pendiente |
| C10 | Fail, máx. 13,6164722 mm | Pass, máx. 0,0009536743 mm | Pendiente |
| C11 | Fail, 3/6 | Pass, 0/6 | Pendiente |
| C12 | Info, 11 medidos | Info, 11 medidos | Pendiente |
| C13 | NotEvaluated, 35/36 | Info, 40/40 | Pendiente |
| C14 | Info, máx. 2; tope no configurable | Info, máx. 3; tope 4, 0 alcanzados | Pendiente |
| C15 | Info, Estimated; pasos distintos | Info, Exact; mismo paso S4/S6 | Pendiente |
| C16 | Info, 11,3993931 mm | Info, 1,86264515E−05 mm | Pendiente |
| C17 | **Fail, p95 14,3221 ms** | Info, sin extrapolar | Pendiente |
| C18 | Info, RunsInEditMode=False | Info, RunsInEditMode=True | Pendiente |

Tras recuperar la campaña nativa se completará esta comparación y se detendrá para Claude/PO. No se elige backend, congela/promueve variante/preset, modifica spec/umbrales o inicia I5.

## Preparación nativa completada — campaña pendiente

Build **0.1.0+557d3ff dev instalada**, export limpio Release ARM64 firmado y metadata exacta verificada, perfil vigente comprobado antes de instalar, prod sin DevPanel/puente y herramientas intactas. Encabezado ProjectPhysics con fastMotionThreshold=0.01 capturado en BuildInfo embebido; P2 y persistencia de muestras comprobados en IL2CPP. [Comprobación previa](../validation/2026-10-23-i4d-native-preflight.md), [evidencia](../validation/evidence/2026-10-23-i4d-native-build.json).

iOS rechazó el lanzamiento por pantalla bloqueada; apertura manual del PO pendiente. No hubo campaña nativa P2 por herramientas. [PR #6 borrador](https://github.com/rafaallendes-boop/squash-clash/pull/6), sin fusionar; falta recuperar la única campaña bajo condiciones aprobadas y completar comparación, después detenerse para Claude/PO.
