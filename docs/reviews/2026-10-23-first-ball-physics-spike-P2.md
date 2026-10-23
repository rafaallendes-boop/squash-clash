# T4.5 / I4d — P2 analítico y comparación con P1

Estado, 2026-10-23: **entrega técnica completa para revisión de Claude y decisión del PO; I4d abierto**. Regresión GREEN, experimentos completos EditMode/PlayMode y una campaña nativa P2 auditada, todos los criterios de gate de P2 en Pass. Rama `firstball/i4d-p2-analytic`, PR de física propia. P1 sigue como respaldo; backend definitivo pendiente del gate con datos de P2. Sin elección, congelación/promoción, spec modificada ni I5. No se fusiona esta entrega por cuenta propia.

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

## Única campaña nativa P2 — completa y auditada

Build **0.1.0+557d3ff dev**, variante **analytic/r2-20261023**, configuración **c86243b15a97**, seed **20261020**. Informe **P2-20261023-204715-409**, UTC **20:47:02.4840920Z → 20:47:15.4061240Z** del 2026-10-23: 17 fijos +10.000 S8 +36 S8d, **10.053 lanzamientos**, 18 criterios únicos con cobertura cerrada, cero no evaluables. [Informe íntegro](../validation/evidence/physics-spike-P2/iphone-557d3ff-P2-20261023-204715-409.md), [auditoría con 200 muestras y condiciones](../validation/evidence/2026-10-23-i4d-p2-native-run.json). Archivo **16.649 bytes**, SHA-256 `4a350b411f348c00d43aaf4f08704e78b0536c5269f6b11e3eb65c1b56472b99`.

El PO confirmó término («Listo. Prueba completada») y condiciones («SI, al 80% y todo según lo solicitado»): teléfono físico, **batería inicial 80 %**, No molestar, bajo consumo desactivado, sin funda, sin capturas/duplicación/devicectl/otras herramientas durante ejecución. No hubo herramientas del agente hasta el aviso de término. **Térmico nominal al inicio, al iniciar C17 y al final; cero interrupciones de foco.** Recuperación de Documents/spike posterior; hubo errores de conexión antes de recuperar el archivo, sin nueva ejecución ni instalación. El térmico de C17 sigue siendo informativo, sin atribución causal o umbral nuevo.

**C17 Pass: p95 0,011400 ms ≤1 ms**, 20 calentamientos descartados y **200 muestras individuales persistidas**. Mediana **0,008600 ms**, p99 **0,127200 ms**, máximo **0,158700 ms**. Recalculados desde las muestras finitas en orden de adquisición: mediana como promedio central, p95 rango 190 y p99 rango 198 (nearest rank); coinciden con el encabezado a seis decimales. No se compara rendimiento del editor con nativo.

**Los 11 criterios de gate nativos pasan:** C1–C8, C10, C11 y C17. Los otros siete son informativos, no aprobaciones añadidas. C3 0/1.701, C7 0/18.797 con 1.235 no aplicables y NE=0; C10 máximo **0,0009536743 mm**, C11 0/6. C14 tope 4 configurable, máximo 3 contactos no persistentes/paso y **0 topes alcanzados**. C15 Exact, S4 mismo paso 22 y S6 mismo paso 24. C16 penetración máxima **1,86264515E−05 mm**, 1.217.571 muestras persistentes. C13 40/40, mínimo 1 y mediana **1,00529957**. Se conservan las diferencias informativas frente a editor (mediana C13 1,00529981; persistentes C16 1.217.572); C5–C7 no exigen igualdad entre plataformas.

Encabezado nativo verificado: **EnhancedDeterminism=True; fastMotionThreshold=0.01; bounceThreshold=0.1; Unity=6000.3.27f1**, fastMotionThreshold marcado relevante solo para P1 y bounceThreshold efectivo P2=0.1. Fuente de fastMotionThreshold: configuración de export capturada en BuildInfo embebido, sin afirmar getter runtime. Quedan resueltas en esta campaña las dos observaciones menores diferidas de I4b; I4b no se repitió.

## Comparación de 18 criterios — completa

P1 corresponde a su campaña nativa final bajo condiciones aprobadas; P2 editor a sus dos experimentos completos y P2 nativo a la única campaña autorizada. Las columnas son resultados separados; no son comparaciones bit a bit entre backends/plataformas. El rendimiento editor sigue siendo informativo y no se compara con nativo. [P1 final](../validation/evidence/2026-10-23-i4b-native-compliant-run.json).

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

La evidencia nativa P1 conserva C3/C10/C11/C17 en Fail y C13 NotEvaluated; P2 pasa los criterios de gate evaluados en esta campaña. No se deduce identidad física con PhysX ni una elección de backend. Simplificaciones y semántica de simultáneos/tope declaradas arriba quedan para revisión de Claude con la adenda, sin ajustes posteriores a los resultados.

## Build, entrega y detención

Build **0.1.0+557d3ff dev instalada**, export limpio Release ARM64 firmado, Player no Development, metadata exacta verificada, perfil vigente comprobado inmediatamente antes de instalar; prod nuevo sin DevPanel/puente y herramientas intactas. [Comprobación previa y resultado](../validation/2026-10-23-i4d-native-preflight.md), [evidencia de build](../validation/evidence/2026-10-23-i4d-native-build.json). El lanzamiento por herramientas fue rechazado por Locked; el PO abrió manualmente y el informe completo verifica el arranque de la build correcta. Física y mediciones no cambiaron desde los experimentos del editor.

**Se detiene aquí para revisión de Claude y decisión del PO. I4d no cerrado; [PR #6 borrador](https://github.com/rafaallendes-boop/squash-clash/pull/6), sin fusionar.** Backend definitivo pendiente del gate posterior; P1 sigue de respaldo. Sin congelación/promoción de variante/preset, cambios de escenarios/umbrales/criterios/spec ni inicio de I5. No se solicita otra campaña.


## Revisión final de Claude y decisión del PO — 2026-10-23

El PO comunica **aprobación de Claude de PR #6 / T4.5–I4d**: P2 cumple la adenda r2 incluida la precisión de paredes; resultados y muestras persistidas verificados. Nativo: 11 criterios de gate Pass, C10 ≈0,00095 mm, C16 ≈0,00002 mm (informativo), C17 p95 0,0114 ms con las 200 muestras guardadas. Se conserva C17 del editor como Info según el criterio original; no se altera su clasificación.

Observaciones de Claude, registradas **sin cambiar código**:

- **(a) Precondición para I5+:** Reset de P2 supone un estado dentro de la cancha; quien inyecte estados (el golpe de raqueta) debe garantizarlo, o se agrega una validación cuando exista ese golpe. No se implementa ni autoriza I5 ahora.
- **(b) Calibración:** bajo bounceThreshold, la fricción usa μ·(1+e)·|v_n|, literal según la adenda. Queda anotada para la calibración, sin cambio ahora.
- **(c) C14:** el acoplamiento de SpikeRun con AnalyticBallSimulationFactory es aceptable en First Ball.

Claude verificó además que no existe riesgo de atravesar planos por redondeo al final del paso, porque el redondeo al float más cercano es monótono y los planos son valores float exactos. Conclusión de la revisión comunicada por el PO, sin nuevo cambio de implementación.

El PO autoriza marcar PR #6 lista y fusionarla con merge commit, sincronizar main y registrar el cierre. Decide **P2-Analytic como backend de First Ball** por la regla de §8.2.3, alternativa más simple que cumple invariantes y umbrales; P1 conserva C3/C10/C11/C17 Fail y queda como respaldo configurable. Esto sustituye la detención y el backend pendiente consignados arriba como estado de entrega anterior. Aplicación del default y decisión documentada en I4e, rama firstball/i4e-physics-backend, PR para Claude; verificación corta de arranque/rebote P2 en iPhone sin repetir campaña. Spec intacta, entrada preparada para T7.3 sujeta a autorización PO; aceptación completa P2 en T7.1. I5 no autorizado.
