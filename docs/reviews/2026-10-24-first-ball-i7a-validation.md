# I7a — aceptación y validación integral de First Ball (T7.1–T7.2)

**Claude aprobó con ajustes solo documentales y el PO acepta First Ball con desviación H17 registrada.** [PR #13](https://github.com/rafaallendes-boop/squash-clash/pull/13), rama `firstball/i7a-validation`, sin fusionar. **T7.1 Pass; H17 p95 Fail en ambas sesiones.** PO autorizó T7.1–T7.2 el 2026-10-24 tras aprobar/cerrar PR #12 (merge 7d7fd3a421255ae02558840d5ff9f2a669fce0d0); main sincronizada, worktree L2 eliminado. **T7.3/gameplay no autorizados.** Fusión/cierre autorizados por el PO si el diff desde el head revisado `51014a08fb49db46b2d1440fb3305b85a5f523ce` solo toca docs/** y AGENTS.md, excluyendo spec/plan/criterios/umbrales/defaults. Esta precisión no cambia datos ni resultados medidos; el cierre con merge/head se registra en AGENTS.md y aprobaciones.

## Alcance y pruebas

Fuente de producto **e0f9071**: soporte dev de H17 con sesión de 900 s, 150 s por L1–L6 y repetición elegida por `launcher.repeatSeconds` (temporal 3 s para el protocolo, sin guardar). Se ejecutan lanzamientos normales, no SpikeRun ni benchmark. Inicio exige P2, repetición positiva, sin pendientes ni cámara lenta; en iPhone exige térmico nominal. Cierra/bloquea panel, deshabilita las demás acciones y permite cancelar. Foco/segundo plano interrumpen la sesión y persisten resumen incompleto, nunca Pass. Progreso visible sin panel.

Histogramas independientes abarcan **toda** la sesión, incluidos relanzamientos y cambios de L; se informan p50/p95/p99, frames >33 ms/min, CPU/GPU p95 con conteos, tiempo por térmico y peor estado, DroppedSteps, duración por caso, brillo nativo inicial, hashes y fechas. `Phase=session` desde el inicio hasta el último segmento, restaurando fase/semilla después. Sin promediar percentiles de segmentos. Percentiles por borde superior de cubeta (sesgo ≤0,1 ms ya documentado); null sin muestras=N/D, null con muestras=overflow >250 ms. Condiciones de brillo/bajo consumo también se registrarán por el PO.

Dos grupos RED→GREEN: ausencia del control delimitado (3/3 RED→GREEN) y reinicio con L1 ya seleccionado/progreso exterior (1/1 RED→GREEN); conjunto final 4/4. Batería final **270/270 EditMode y 136/136 PlayMode**, cero fallos/omitidos, XML frescos/sin errores CS. [TDD, hashes y fuente](../validation/evidence/2026-10-24-i7a-tests.json), 121 fuentes cotejadas. Los experimentos están separados de la regresión.

Sin cambios de motor, host, escenarios, preset, ProjectSettings, criterios, umbrales ni spec. `DefaultPreset.json` sigue única fuente de defaults. El encabezado nativo del spike añade SimulationKey para auditar configuración; no altera las mediciones. H17/control/progreso quedan excluidos de prod mediante el ensamblado DevPanel existente.

## T7.1 — aceptación de física

EditMode y PlayMode editor completos sobre fuente **19aae3b**: 18 filas únicas cada uno, diez criterios de gate aplicables Pass, C17 informativo con 200 muestras. p95 editor 0,0219 / 0,0207 ms; no se usa para aprobar C17 nativo ni se interpreta frente al iPhone. Física/cancha/spike idénticos byte a byte a la fuente de la build e0f9071. [Auditoría editor](../validation/evidence/2026-10-24-i7a-p2-editor.json), informes [EditMode](../validation/evidence/physics-spike-P2/editor-19aae3b-I7a-editmode.md) y [PlayMode](../validation/evidence/physics-spike-P2/editor-19aae3b-I7a-playmode.md).

**Campaña nativa P2 completa y aceptación de física Pass**: dev e0f9071, UTC **23:33:03→23:33:21**, Default **0d39e6016bd8**, SimulationKey **166a96e319c7**, variante analytic/r2-20261023. **18 filas únicas, los 11 criterios de gate Pass**, coberturas cerradas/sin no evaluables y ninguna regresión frente a I4d. C17 **p95 0,0188 ms ≤1 ms**: 200 muestras persistidas, 20 warmup, mediana 0,0133, p99 0,0215, máximo 0,2688 ms; agregados recalculados por nearest rank/mediana de centrales. C10 **0,0009536743 mm**, C16 **0,0000186264515 mm**, informativo. Sin cambio de umbral ni interpretación entre editor/nativo.

PO: «Prueba completa. El iphone sigue conectado. Todo según lo solicitado». Confirmación global de teléfono físico/Default/cero pendientes/No molestar/batería≥80 %/bajo consumo apagado/sin funda/sin capturas, Duplicación ni herramientas durante ejecución. **Porcentaje exacto de batería no comunicado**. El archivo confirma térmico **nominal inicial/benchmark/final**, **cero interrupciones de foco** y ProjectPhysics fastMotionThreshold=0,01/bounceThreshold=0,1. Sin herramientas del agente entre solicitud y aviso de término. Informe recuperado solo después.

[Informe nativo íntegro](../validation/evidence/physics-spike-P2/iphone-e0f9071-I7a-P2-20261024-233321-241.md), [auditoría/200 muestras](../validation/evidence/2026-10-24-i7a-p2-native.json), [estado/presets](../validation/evidence/2026-10-24-i7a-post-campaign-state.json). Métricas corroboran Default aplicado **23:32:33Z**, antes de iniciar campaña a las 23:33:03Z, cero pendientes. P0-calib-1/Po-1 idénticos byte a byte; selección last-active correctamente borrada al cargar Default, sin sobrescribir presets. **Prueba de aceptación de física T7.1 satisfecha; prod y H17 se registran aparte.**

### Trazabilidad de configuración

[Comparación saneada](../validation/evidence/2026-10-24-i7a-config-traceability.json). Gate original I4d: hash **c86243b15a97**, fábrica P2 inyectada explícitamente aunque el archivo decía `physics.backend=0`; SimulationKey cruda **04372f672e9e**. Al representar la selección efectiva P2 (backend=1), key **166a96e319c7**, igual a Default actual. La key cruda histórica no se presenta como idéntica ni se reescribe el informe aprobado. Cambio de selección ya aprobado en I4e; los demás valores compartidos son idénticos. La selección real de fábrica/variante siempre fue P2.

De c86243b15a97 a **f6a394a12daf** cambió solo backend por decisión I4e. De ese preset a **0d39e6016bd8** se añadieron once claves no físicas: launcher.repeatSeconds, debug.slowMotion, camera.mode/followDamping/followMaxOffset y aids.shadow/trail/impactMarks/prediction/trailSeconds/ballVisualScale. Trece valores de physics/court, bounceThreshold=0,1 y variante analytic/r2-20261023 se conservan. La igualdad de configuración efectiva se confirma también en test C# de EffectiveConfig.

P0-calib-1 **8de3f2ecb217** es el preset aceptado de Etapa 5; difiere solo en repetición=3. No se usa en la campaña ni se promueve a defaults. Po-1 sigue prueba funcional.

## Release e iPhone

Exports limpios **dev/prod RELEASE-CHECK: OK**, fuente e0f9071 limpia. Módulos producto: Core/Runtime/DevPanel en dev, solo Core/Runtime en prod; puente nativo de spike solo dev, térmico runtime ambos. Xcode/Unity sin cambios. Dev **0.1.0+e0f9071** compilada Release ARM64 y firmada, verificación codesign estricta, metadata/preset embebidos y 121 fuentes coincidentes con pruebas. Perfil compatible con dispositivo/bundle, vigente hasta 2026-10-28T14:59:31Z, comprobado el 24, sin renovación requerida todavía. [Auditoría dev](../validation/evidence/2026-10-24-i7a-native-dev-build.json). Prod **0.1.0+e0f9071** también compilada Release ARM64, firmada y verificada con las mismas garantías; ensamblado DevPanel y símbolo del puente de spike ausentes, puente térmico runtime presente. [Auditoría prod](../validation/evidence/2026-10-24-i7a-native-prod-build.json). Las dos builds están preparadas; **dev instalada** tras comprobar de nuevo perfil/firma/metadata/fuentes. P0-calib-1, Po-1 y last-active recuperados antes/después: idénticos byte a byte. **Prod instalada** tras revalidar vigencia/firma/metadata/fuentes, exit=0. Perfil conocido vence **2026-10-28T14:59:31Z**: comprobar antes de toda instalación y renovar antes de instalaciones desde el 27.

PO conectó/desbloqueó por cable y se resolvió 4016. Dev instalada; lanzamiento automático rechazado por bloqueo posterior del teléfono. Duplicación exige autenticación macOS, por lo que se usa recorrido directo del PO: abrir build e0f9071, cargar/aplicar Default, comprobar P2/cero pendientes y ejecutar campaña bajo condiciones aprobadas. Arranque manual/campaña completados por PO y auditados en la sección anterior; sin herramientas del agente durante ejecución. ProjectPhysics de metadata embebida comprobado antes de campaña: fastMotionThreshold=0,01 y bounceThreshold=0,1 (el primero solo relevante P1). Prod arrancó con `devpanel=absent development=False squashDev=False`, P2/Default/cero escenas PhysX, sin excepciones observadas. Log de display 60 FPS solo como arranque, **no medición de rendimiento H17**. Consola detenida de forma controlada después de observar. [Evidencia funcional prod](../validation/evidence/2026-10-24-i7a-prod-functional.json). **Comprobación física prod confirmada por PO**: «Listo. Funciona bien las 3 cosas», en respuesta a verificar etiqueta e0f9071 prod, sin botón DEV y mantener tres dedos ≥1 s sin abrir panel. Se usa su comprobación directa porque la herramienta visual solo admite un puntero y Duplicación pide autenticación Mac. **T7.1 completo técnicamente**: batería, aceptación física y release/prod; el PO acepta después la desviación H17 y autoriza el cierre sujeto al diff documental.

## T7.2 — H17 y plantilla

Dev e0f9071 reinstalada tras revalidar perfil/firma/metadata/fuentes, lanzamiento sin consola exit=0. El PO cargó **P0-calib-1**, aceptado en Etapa 5, igual a Default salvo repetición=3. Se conservaron los JSON guardados; L1–L6 solo cambian sus claves de lanzamiento/referencias, con SimulationKey **166a96e319c7**. La campaña física de T7.1 se realizó antes con **Default**, tal como se exigió.

PO: «Listo H17 completa. Informe guardado. El brillo lo tenía al 60% y el resto de las condiciones si estaban como solicitabas. Creo que fueron 2 informes porque pensé que me equivoque y lo ejecuté de nuevo». Recuperados **dos inicios y dos resúmenes completos**, sin intentos incompletos en la instantánea. Se conservan **ambas sesiones en orden**, sin elegir la mejor, combinarlas ni solicitar otra ejecución. Sin herramientas del agente entre solicitud y respuesta.

| Medición de sesión completa | H17-1 | H17-2 | Objetivo / resultado |
|---|---:|---:|---|
| Hora local America/Santiago, 24 de octubre | 20:41:23→20:56:23 | 20:58:05→21:13:05 | 15 min cada una |
| Duración medida | 900,000054 s | 900,002363 s | ≥900 s: ambas completas |
| Frames / FPS medio calculado | 53.985 / 59,9833 | 53.991 / 59,9898 | 60 FPS, meta |
| Frame p50 | 16,7 ms | 16,7 ms | Informativo |
| **Frame p95** | **17,2 ms** | **16,8 ms** | **≤16,7 ms: Fail ambas** |
| Frame p99 | 17,3 ms | 17,1 ms | ≤25 ms: Pass ambas |
| Frames >33 ms / minuto | 1 / 0,066667 | 0 / 0 | ≤1/min: Pass ambas |
| CPU p95 / muestras | 17,2 ms / 52.491 | 16,8 ms / 52.497 | Registrado, sin umbral nuevo |
| GPU p95 / muestras | 2,5 ms / 52.491 | 2,5 ms / 52.497 | Registrado, sin umbral nuevo |
| Térmico inicial / peor | nominal / nominal | nominal / nominal | Sin serious/critical: Pass |
| Segundos nominal | 900,000054 | 900,002363 | Toda la sesión |
| Segundos unknown / fair / serious / critical | 0 / 0 / 0 / 0 | 0 / 0 / 0 / 0 | Sin cobertura térmica desconocida |
| DroppedSteps | 0 | 0 | Registrado, comparable |
| Lanzamientos / L1–L6 | 300 / 50 por caso | 300 / 50 por caso | ≈150 s por caso, repeat=3 |
| Phase / métricas comparables | session / true | session / true | Ambas completas |

**Objetivos de rendimiento H17: Fail en ambas sesiones por p95.** Se conserva el borde superior de cubeta de 0,1 ms (sesgo ≤0,1 ms ya documentado), sin restar ese sesgo para declarar Pass, sin redondear 16,8 a 16,7 ni modificar el objetivo. No se optimiza durante esta entrega. El fundamento de aceptación del PO y la precisión de Claude se registran abajo. La prueba de física P2/C17 mantiene su resultado propio; no se mezclan con los frames H17.

Condiciones: teléfono físico, térmico nominal inicial en ambos registros, panel cerrado, sin cámara lenta/spike/benchmark/capturas/Duplicación/herramientas durante ejecución; confirmación global del PO y control H17. **Bajo consumo apagado**, por esa confirmación. Brillo **≈60 % declarado por PO** y **Screen.brightness=0,5500000119 (≈55 %) nativo inicial en ambas**: se registran los dos valores, sin inferir la causa de la diferencia. El PO confirmó el resto de condiciones solicitadas, incluida no modificación durante medición. No cancelación de foco registrada; el control habría persistido incompleto al perder foco.

[Evidencia/auditoría H17](../validation/evidence/2026-10-24-i7a-h17.json), extractos JSONL de datos de validación [H17-1](../validation/evidence/h17/h17-e0f9071-session-1.jsonl) y [H17-2](../validation/evidence/h17/h17-e0f9071-session-2.jsonl): **602 registros cada uno**, 300 launch/300 frames/start/summary, líneas originales Phase=session separadas por intervalo. La instantánea de origen completa (1.275 registros) queda en Logs ignorado; SHA-256 y selección documentados. No se versionan logs de herramientas ni identificadores personales del dispositivo.

Límites de auditoría: los percentiles completos provienen del histograma acumulado nativo probado con TDD; los segmentos no permiten recalcular independientemente ese histograma y no se promedian sus percentiles. Los segmentos tienen **un frame inicial adicional** frente al resumen H17, que descarta explícitamente el delta anterior al inicio; CPU/GPU coinciden exactamente en conteos, los frames largos reconstruyen 1/0 y todos los DroppedSteps son cero. En cada sesión dos flushes al cambiar automáticamente L4→L5/L5→L6 capturan 4/6 claves de lanzador pendientes, seguidos inmediatamente por launch con cero; se preservan esas filas y su explicación por el código, sin atribuir ediciones manuales ni ocultarlas.

Tras recuperar datos: P0-calib-1, Po-1 y last-active **idénticos byte a byte** al respaldo original; selección guardada P0-calib-1. Intento de reinicio controlado para volver a ese preset rechazado por iPhone bloqueado: no se afirma una restauración observada. Sin nueva instalación ni cambios en archivos de presets; la siguiente apertura normal usa la selección guardada. No se solicita repetir nada.

[Plantilla de cinco secciones para completar por PO](../validation/first-ball-validation.md): H1 ya confirmado globalmente, datos no comunicados permanecen pendientes; física, rendimiento, invariantes §17.A.1/2/3/10/14 y decisión del PO. La sección 5 recoge la aceptación expresa de First Ball con desviación; los campos perceptuales no comunicados no se inventan.

Retención de logs sigue pendiente antes de T8. No se interviene el panel de importación ni la sombra experimental. No T7.3, calibración nueva ni gameplay. La revisión condicionada de Claude y decisión del PO constan en la sección siguiente. Tras cumplir la condición documental y registrar la fusión/cierre, detenerse; T7.3 y gameplay siguen sin autorizar.


## Revisión de Claude y decisión del PO — 2026-10-24

**Precisión de medida aprobada:** con resolución de **0,1 ms** y borde superior de cubeta, **16,8 ms no se distingue de ≤16,7 ms**, según la revisión de Claude comunicada por el PO. Se mantiene **Fail tal como se midió**, sin restar el sesgo, recalcular el resultado ni cambiar el umbral. H17-1 conserva 17,2 ms y H17-2 16,8 ms; las dos sesiones originales y sus hashes permanecen intactos.

**Qué significa CPU p95:** proviene de **FrameTiming.cpuFrameTime**, que incluye la espera de presentación/vsync y el resto de esperas/overheads entre frames. Por eso coincide aquí con el p95 de frame (17,2/16,8 ms); **no mide la carga real de CPU**. Fuente de la métrica en `FrameTimingSampler.AcceptTiming`, sin modificarla. Referencias oficiales: [Unity 6.3, cpuFrameTime](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/FrameTiming-cpuFrameTime.html) y [contadores de FrameTiming](https://docs.unity3d.com/6000.3/Documentation/Manual/frame-timing-manager-counter-reference.html).

**Decisión PO:** se acepta First Ball con desviación registrada en H17 p95 (H17-1 17,2 ms y H17-2 16,8 ms frente a ≤16,7 ms). El resultado se conserva como Fail; no se cambia el umbral, no se resta el sesgo de cubeta ni se repite la sesión. Fundamento del PO: p99, frames >33 ms/min, térmico y DroppedSteps cumplen; distribución estrecha en torno a 16,7 ms (oscilación de cadencia, sin frames perdidos); el umbral está marcado [EXPERIMENTO hasta conocer el iPhone] en §17.C/§17.D del spec v0.3. No se solicita otra medición. Spec, criterios, umbrales y defaults intactos.

**Observaciones de Claude, DevPanel solo dev; sin cambios de código:**

- PerformanceSession.Finish restaura fase/semilla, pero no launcher/human/rival. Después de H17 los registros continúan etiquetados **P0-calib-1 con L6 aplicado**: deuda de trazabilidad para el invariante 14; preset guardado intacto no equivale a valores activos restaurados.
- En el cambio automático de L, los Set Immediate disparan FlushFrames con **4/6 claves pendientes** antes de aplicar el lanzador. Se conservan las filas ya auditadas.
- Trabajo por frame en Tick/DevBar.Update (root.Q, GetComponent, texto/tooltip del botón) que conviene cachear en una intervención futura autorizada.
- El bloqueo de Begin no identifica la condición incumplida; **MetricsInvalid persiste hasta reiniciar**.
- Acumuladores frame/CPU/GPU duplicados respecto de RecordDiagnosticsFrame y cambio temporal de Metrics.Phase en Finish.
- **Pendiente opcional, no autorizado:** medir cpuMainThreadFrameTime y cpuMainThreadPresentWaitTime para aclarar H17 en una revisión futura de la spec. No se implementa, mide ni modifica la spec ahora.

**Próxima apertura:** comprobar que P0-calib-1 queda activo, hash efectivo **8de3f2ecb217**, cero pendientes y estado inicial del preset; la restauración anterior no se observó. Se intentó abrir nuevamente la build ya instalada durante este ajuste documental, pero el iPhone sigue bloqueado y el lanzamiento fue rechazado. Queda como seguimiento, sin afirmar que la etiqueta P0-calib-1 durante L6 demuestra restauración. No requiere build ni instalación.

La aprobación de Claude se satisface con estos ajustes documentales, comunicados por el PO. Fusión autorizada únicamente si el diff desde el head revisado hasta el nuevo respeta el perímetro documental; cualquier otro archivo obliga a detenerse sin fusionar. No se ejecutan código nuevo, build, instalación ni medición de rendimiento.
