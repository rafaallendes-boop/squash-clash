# Regla experimental de retorno — entrega PR #32

2026-11-04. Rama `codex/sl56-return-rule`, apilada sobre `firstshot/s2-m24` / PR #30. **Lista para revisión de Claude/PO, sin fusionar.** Fuente de producto y pruebas finales: `a048e71`. Ninguna activación por defecto, build nativa para instalar, instalación o sesión física.

## Resultado y decisión

Decisión literal del PO **«Dale con la recomendada»** registrada en [aprobaciones](2026-10-25-first-shot-po-approvals.md) y AGENTS. Claude aprobó sin correcciones PR #31/head84ad994 y recalculó los resultados independientemente; sus lecturas de lateral, pared trasera, saltos y tin se conservan atribuidas. PR #30/#31 siguen abiertas y sin fusionar.

La regla baja el punto pedido por la misma x y con la misma P. Si la base ya retorna dentro de cancha, se conserva incluso corta. Si no encuentra solución, conserva el resultado actual del ShotSolver y registra `NoSolution`: si estaba Solved se aplica normalmente; si estaba Unsolved conserva la falla previa, sin inventar velocidad. Desactivada devuelve el resultado original y no ejecuta la clasificación adicional.

Se usa la intersección del mapa aprobado, sin regla de puntuación ni cambios de P2. La búsqueda elige el punto pedido más alto **probado** en su secuencia discreta; no se presenta como máximo continuo ni como máximo de TargetUsed cuando interviene la asistencia original.

## Comparación obligatoria antes del código

Método **f901ea7**, anterior al primer cálculo; resultado/comparación **bf89e75**, anterior al esqueleto TDD32695f7 y a la implementación90cf293. Único cambio del arnés aprobado: límite de búsqueda TinTop+0,05f en lugar deTinTop+0,01f. Mismas73posiciones/99puntos/11P y mismos clasificador/guardia/física/horizonte. Dos corridas completas con cincoTSV idénticos y base idéntica al mapa.

| Grupo | N | Y dentro, margen0,01→0,05 | Sin soluciónY | Bote traseroY | Dentro bajarP | Sin solución bajarP |
|---|---:|---:|---:|---:|---:|---:|
| SL5/SL6 | 26.136 | 26.125→26.116 | 11→20 | 6.270→6.270 | 26.099 | 37 |
| SL1–SL4 | 53.361 | 53.247→53.201 | 114→160 | 22.831→22.831 | 53.089 | 272 |

Δy de cambios exitosos conserva medianas1,25m/0,5m y máximos4,000000179m/2,200000286m focal/control. Los máximos de discontinuidad y la columnaC no cambian; se mantienen24/24botes traseros focalesY desdeP0,3 y17/24enP. Los66447retornos base dentro siguen intactos. **Juicio de Codex documentado antes del código:** conserva el panorama de la recomendación, con ventajaY sobreP y riesgos existentes; se continúa la autorización condicionada, sin criterio nuevo ni aceptación física. [Comparación completa](2026-11-04-first-shot-return-rule-margin005.md).

### Alcance real del margen

El margen limita **candidatos pedidos por la búsqueda**, no reescribe la guardia ni TargetLowered. Bases válidas cony0,50 pueden preservarse aunque TinTop+0,05≈0,53m. Además, el JSON de la evaluación previa contiene **10 cambios en controles** cuyo TargetUsed queda por debajo deese límite por TargetLowered original: peticióny≈0,65m, TargetUsed≈0,506259918m. Distancia mínima de TargetUsed al nuevo límite en cambios decontroles≈−0,023740072m; en focales es+0,020000022m. Esta lectura se explicita en la revisión final; no se cambió el arnés o el producto para ocultarla.

Por tanto, **no se garantiza un margen final de0,05m para toda trayectoria**. Imponerlo también al resultado del solver o a bases preservadas cambiaría el método/guardia y exigiría otra decisión. La vista muestra TargetUsed y el registro conserva los tres puntos. ConservarP tampoco anula SpeedRaised. Pared trasera conP alta aceptada porahora; lateral queda paraS3/FirstRally, sin modificación.

## Producto y compatibilidad

| Clave nueva | Default | Efecto |
|---|---:|---|
| `shot.returnEnabled` | 0 | Regla experimental desactivada |
| `shot.returnTinMargin` | 0,05 m | Límite inferior de las peticiones buscadas |
| `shot.returnSearchStep` | 0,05 m | Descenso desde el origen decimal de la petición |

Catálogo y JSON único de defaults ampliados a109claves, hash **af2114f4cbc7**. Los106valores previos se cotejan uno a uno contra fixture inmutable; 41heredados/proyección0d39e6016bd8 ySimulationKey166a96e319c7 intactos. Solo0,05/0,05 fue evaluado. Los rangos de edición de las claves nuevas (margen0–6m, paso0,01–6m) permiten configuración finita/acotada, no son bandas o criterios de aceptación; el paso mínimo de edición reutiliza la huella de0,01m, y6m es la envolvente existente. No se cambian rangos previos ni se recomienda otro valor.

[Preset experimental S2-return-rule](../validation/presets/S2-return-rule.json), hash **192546939375**, difiere de defaults solo en activación=1. Se valida con el codec real y el overlay compatible; no se guardó o cargó en un iPhone. P0 extendido de esta rama, solo calculado enUnity: **3c37a3959ca8**; no confundir con el P0 **dd122831f592** de la campaña instalada PR#30. La rama no se usa para las tandas2–4: siguen la dev2ef4807/prod preparadaa11fb32 del producto9131b1c.

ShotSolver, su contrato yflags, Ballistics, P2, SL, tiempos/ventanas,41valores heredados yspecv0.3 quedan intactos. D5 se añade alfinal del plan FirstShot ypropuesta r2; sus textos anteriores son prefijos byte a byte idénticos a PR#30. Sin implementación S3–S4/T8.

## Autoridad, caché, preview y registro

`ReturnShotResolver` comparte una resolución determinista para vista previa y lanzamiento; configuración/court/materiales capturados y copiados porrecepción. Caché de **una sola solicitud**, comparada porbits de posición, x/y/P; no usa la igualdad aproximada deVector. Nueva recepción crea un resolver nuevo. No interpola resultados ni aproxima claves. Una solicitud exacta repetida evita todas las llamadas alsolver.

La clasificación abre una simulaciónP2 aislada y se detiene tras completar el paso del primer bote; eso conserva exactamente la intersección aprobada hasta ese bote, sin simular el resto innecesario. El arnés externo sigue el horizonte original para comparar todos sus eventos. La predicción nunca modifica pelota/historial/autoridad.

Durante el apuntado se usa el contacto pronosticado de la orden para el instante mostrado; si el timing aún no permite contacto, se usaT* como referencia predictiva. Al soltar se resuelve el contrato original y la posición autoritativa; misma solicitud/contexto produce idéntica resolución, pero un contacto oP diferente requiere una nueva resolución. No se congela la posición predictiva como autoridad ni se rescata timing fallido.

C4 reutiliza **la mira original y el marcador factible blanco ya existente**; no se eligió un estilo/tamaño nuevo. La intención delBegan no se mueve. El punto ajustado está visible antes deEnded y el test comprueba que la pelota y la predicción siguen intactas. Se muestra TargetUsed, separándolo de la intención; el registro distingue también la petición intermedia de la regla.

| Dato | Campos de ShotRecord |
|---|---|
| Punto/P originales | `AimX`, `AimY`, `Power` |
| Petición tras regla | `ReturnRequestedX`, `ReturnRequestedY` (P original conservada) |
| Resultado real del solver | `TargetUsedX`, `TargetUsedY`, `Speed`, `VelocityX/Y/Z` |
| Flags originales | `SpeedRaised`, `TargetLowered` |
| Activación/estado | `ReturnRuleEnabled`, `ReturnRuleState`: Disabled/Preserved/Changed/NoSolution |
| Costo y llamadas | `SolverCostMs`, `ReturnSolverCalls`, `ReturnPreviewCostMs`, `ReturnPreviewSolverCalls` |

Los rechazos originales por datos/punto de frontis inválido o distancia horizontal nula no se convierten en asistencia. RED5f9eb4c poraserción→fixa048e71, incluyendo alturas6/float.MaxValue/infinito/NaN/negativas; evita búsquedas fuera del dominio sin añadir otra legalidad o mover una mira inválida.

## TDD y regresión

| Frontera | RED observado | GREEN |
|---|---|---|
| Núcleo/configuración | 7pruebas,6fallos poraserción/1Pass, sinCS; esqueleto32695f7 | 7/7 tras90cf293 |
| Preview/lanzamiento/registro | 3pruebas/3fallos poraserción, sinCS; fd7672e | 4/4 incluyendo costo diagnóstico |
| Petición inválida | 1prueba/1fallo poraserción; 5f9eb4c | Incluida en530/530 final |
| EditMode final | XMLfresco21:11:10Z | **530/530**,0omitidas |
| PlayMode final | XMLfresco21:13:18Z | **301/301**,0omitidas |

Los mínimos del encargo están cubiertos: baseválida corta preservada; x/P conservadas; límite de búsqueda tin+margen; fallback original; preview coincide con lanzamiento para igual solicitud/contacto; desactivadaidéntica. También caché exacta,compatibilidad106valores/preset separado yrechazo inválido. Una primera regresión528/529 detectó la expectativa antigua delhash extendidoP0; solo se actualizó ese metadato, manteniendo las aserciones debytes/41valores/selección. Conteos/hash dedefaults en otros tests se amplían por las tres claves; no se relajan tolerancias o aserciones de comportamiento previas.

Regresión externa de la fuente final: **73contactos×99puntos×11P=79.497solicitudes**,0diferencias en estado/petición/resultados físicos frente al arnés de margen0,05. **66.447Preserved /12.870Changed /180NoSolution**. EnNoSolution se compara explícitamente elfallback con la base, no con ceros. Desactivada: TSV de79.497filas **idéntico byte a byte** a la base aprobada. Repetición inmediata de cada solicitud:0llamadas alsolver por caché. [Datos sanitizados, hashes y verificación](../validation/evidence/2026-11-04-return-rule-product-validation.json); [reproducción](../diagnostics/2026-11-04-return-rule-product-reproduction.md).

## Costo por solicitud

Stopwatch en elMac de desarrollo. No es medición nativa, H17 ni aprobación de rendimiento. No umbral nuevo. «Con caché» significa repetición inmediata de la solicitud exacta; un cambio deP/contacto/punto puede volver a ejecutar búsqueda.

| Ruta/población | n | p50 ms | p95 ms | p99 ms | máximo ms |
|---|---:|---:|---:|---:|---:|
| Resolver, rejilla completa sin caché | 79497 | 0.0205 | 0.8033 | 2.0327 | 3.5089 |
| Resolver, repetición exacta | 79497 | 0.0001 | 0.0001 | 0.0001 | 0.0483 |
| TryPreviewReturn, SL1/5/6×11P sin caché | 33 | 0.0451 | 1.0995 | 1.1015 | 1.1015 |
| TryPreviewReturn, repetición exacta | 33 | 0.0148 | 0.0193 | 0.0928 | 0.0928 |

Llamadas alsolver en la rejilla: mediana1/p9531/p9960/máximo81; con caché0. La rutaTryPreviewReturn incluye contrato/mapeo/selección del contacto yresolver, no el dibujo/GPU delLineRenderer ni el frame completo. Las poblaciones de ambas mediciones son distintas, no se restan sus percentiles para inferir costo. El mínimo0ms del caché está por debajo de la resolución delreloj, no demuestra costo nulo. Medición inicial y corrida final preservadas enLogs; se reporta la fuente final completa.

## Release, integridad y límites de entrega

**Check-release dev/prod final desde a048e71 limpio/gitDirty=false**, exports frescos. Core+Runtime enambos; DevPanel solo dev; InputSystem.TestFramework excluido; puente térmico runtimeincluido. Define por extraScriptingDefines, Player noDevelopment, IL2CPPRelease. Se hicieron únicamente exportsUnity/Xcode para la comprobación; **sin xcodebuild/IPA/app nativa para instalar/instalación/acceso al iPhone**. Las salidas se redirigieron desde unacopia temporal delscript aLogs/margin005-20261104/release, sin cambiar scriptsversionados ni Logs/evidence.

Archivos fuente probados cotejadosSHA-256 después deexports. ProjectSettings de pruebas restaurados antes deexportar; worktree limpio alcierre. D5aditiva/specv0.3 intacta, ShotSolver/P2 intactos. Logs/evidence dePR#30 conserva **3.145archivos/875.351.187bytes**, mismasrutas/hash/tamaños/mtimeNs. Sin versionarLogs, scripts/device.env, builds, cachés, identificadores del iPhone ni claves.

**PR#30/#31/#32 sinfusionar.** Activar pordefecto y validar físicamente laregla requieren otra decisión. Tandas2–4/restauraciones dePR#30 conservan prioridad antes de **2026-11-07T17:58:46Z /7nov14:58:46America/Santiago**. Sesiónfísica pausada hasta avisoPO; no se relanzó app ni se preparó16. Cuando vuelva, revalidar P0/proceso nuevo/precheck y ventana completa; avisar antes deinstalar si no alcanza. Esta entrega se detiene paraClaude/PO.
