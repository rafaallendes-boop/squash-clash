# I5a / T5.1–T5.2 — estado de edición, presets y diagnóstico

Estado, 2026-10-23: **entrega completa para revisión de Claude; I5a abierto**. Rama `firstball/i5a-tuning-core`, [PR #8 lista para revisión de Claude](https://github.com/rafaallendes-boop/squash-clash/pull/8); I5a abierto. Al entregar el informe se detiene, sin fusionar ni empezar I5b/T5.3.

## Cierre previo y autorización

Claude aprobó PR #7 según comunicación del PO. Fusionada con merge **50a3f4a73fde091e0a8358704d2aca4a6ca3a357**, head d6f8640, main sincronizada y cierres **I4e / Etapa 4** registrados en a0b1caf. Worktree I4e eliminado; artefactos locales preservados en `Logs/archive-i4e-20261023`, fuera de git. **P2-Analytic backend de First Ball por defecto, P1 configurable como respaldo**. Hash efectivo **f6a394a12daf**, sin cambios en este incremento. [Aprobaciones y alcance I5a](2026-10-21-first-ball-po-approvals.md#cierre-de-i4e--etapa-4-y-autorización-i5a--2026-10-23).

El PO autoriza T5.1–T5.2, incluida la pérdida de pausa manual tras «Toca para continuar». Al intervenir el host se atienden la precondición de Reset P2 y la observación de Claude de cubrir ambos backends. No se modifican motores, escenarios, medidas, criterios, umbrales, preset por defecto, ProjectSettings, plan ni spec.

## T5.1 — edición y persistencia

`TuningState` mantiene Defaults, Applied y Visible como ParamSet inmutables, y claves pendientes ordenadas. Immediate actualiza Applied y emite Changed; NextLaunch cambia Visible hasta ApplyPending. Volver al valor aplicado elimina el pendiente. Set valida tipos/opciones, rechaza valores no finitos y recorta rangos; un preset parcial se superpone sobre **Defaults**, sustituye pendientes anteriores y aplica los Immediate. Guardar FromSet(Visible) incluye los valores pendientes. No se incorpora el panel ni LaunchCurrent de T5.3.

`PresetStore`: nombres sin rutas, JSON versionado por PresetCodec, listado sin temporales y selección last-active persistida. Escribe `.tmp` en el mismo directorio, hace `Flush(true)` y cierra antes de reemplazar el destino; si ya existe usa File.Replace sin backup, si no File.Move. El `.tmp` incompleto se ignora al leer; se conservan archivo/selección anteriores si falla la escritura. TrySave/TryLoad/TrySetLastActive devuelven error sin propagar excepciones de E/S. Hay prueba real de directorio sin escritura en macOS, además de reemplazo, interrupción y nombres inválidos.

Se comprobó en el IL2CPP iOS instalado de Unity 6000.3.27f1 que `os/Posix/File.cpp`, File::ReplaceFile sin backup, llama a `rename(source,destination)`. La garantía de sustitución en el mismo sistema de archivos se apoya en [rename(2) de Apple](https://developer.apple.com/library/archive/documentation/System/Conceptual/ManPages_iPhoneOS/man2/rename.2.html); el vaciado previo usa [FileStream.Flush(Boolean)](https://learn.microsoft.com/en-us/dotnet/api/system.io.filestream.flush?view=net-7.0). No se afirma una transacción conjunta de preset y last-active ni se usa copia sobre el archivo definitivo.

Arranque real: AppBootstrap carga DefaultPreset.json, recupera la selección activa sobre esa base, inicia TuningState sin pendientes y conserva nombre/hash SHA-256 del archivo para trazabilidad. Si falta la selección o está dañada, recupera defaults con aviso, según T5.1. Hash del archivo y hash efectivo son campos separados. Sin un preset activo el backend efectivo sigue siendo P2 y el hash f6a394a12daf.

## Host — pausa y precondición de Reset

**Pausa manual corregida:** ResumeOverlay guarda el estado manual al primer Suspend; callbacks repetidos no lo sobrescriben. Al tocar la capa recupera ese estado y conserva la supresión del delta del frame de reanudación. Continue sobre una capa inactiva tampoco quita la pausa manual. Los casos existentes de interpolación, paso único y reanudación siguen pasando con P1 y P2.

**Inyección validada antes de mutar el host:** centro dentro de suelo/límites laterales desplazados por radio, posición/velocidad finitas y contador de paso representable por el host. Un rechazo no cambia la simulación viva, su generación ni su estado. No se impone techo a y: OverTop conserva su semántica. El motor P2 y su Reset no cambian; la futura entrada de golpe tendrá que respetar este contrato del host o garantizarlo si usa la fábrica directamente.

Control adicional RED válido: el límite superior calculado en una comparación podía conservar precisión intermedia en Mono y rechazar el centro float redondeado sobre el plano. Se almacenan los límites en campos float de Vector3 antes de comparar, igual que BallState, **sin ε adicional**. Se aceptan los centros frontera y se rechaza el float inmediatamente exterior. Es un defecto del nuevo guard del host, no del redondeo final del motor revisado por Claude.

Observación de cobertura de Claude resuelta: **BallSimulationHostTests parametrizado con 15 casos por backend (30)**; Launch_UsesEffectiveConfigInEveryConsumer usa el mismo cuerpo parametrizado con dos entradas P1/P2. En P1 se comprueban escenas/materiales; en P2, ausencia de escenas PhysX y predicción con la fábrica analítica/configuración efectiva. Limpieza de instancias verificada en ambos.

## T5.2 — métricas comparables

`FrameTimeStats`: histograma de **toda la sesión**, cubetas de 0,1 ms hasta 250 ms y desbordamiento, nearest rank; ventana circular de 3.600 muestras con percentiles exactos. Tiempo y frames largos/minuto usan tiempo real acumulado, no un FPS supuesto. El conteo >33 ms es estricto en ese borde del histograma; consultas no alineadas tienen resolución de 0,1 ms. Datos vacíos son NaN y un percentil en desbordamiento es +∞ en la API. El test de 54.000 frames es sintético, no una sesión de rendimiento.

`MetricsLog`: una línea JSONL por registro con ts UTC, kind, phase, build Label, presetName, presetFileHash, effectiveHash, pending, seed, metricsValid y droppedSinceRestart. **payload es una cadena JSON escapada**, decodificable con un segundo parse; conserva incluso entrada multilínea sin romper JSONL. E/S fallida queda en LastError y no detiene el juego. Un archivo propio por instancia de app evita mezclar ejecuciones.

AppBootstrap registra cada lanzamiento con su configuración efectiva y, cada **10 s**, un segmento de frames: p50/p95/p99 por histograma, frames y tiempo, >33 ms/min, CPU/GPU p95 y número de muestras, GC gen0, térmico actual/peor, pasos descartados y contadores de sesión. Al cambiar lanzamiento, fase o foco cierra el segmento para conservar su contexto. Muestrea en LateUpdate, después del avance del host. Excluye el tiempo en segundo plano y el primer delta al volver. Un segmento con descartes incluye dropped=true y metricsValid=false; debug slow motion o perfil debug tampoco son comparables. La validez expresa comparabilidad, no aprobación de objetivos de rendimiento.

Fases free/spike/benchmark/session; normal sin sorteo usa seed=0, experimento S8/spike/benchmark seed=20261020. SpikeSection marca fases y restaura fase/semilla al cancelar o terminar, sin cambiar runner, benchmark, escenarios o C17. No se ejecutó una campaña nueva. Los percentiles de segmento tienen resolución 0,1 ms; el histograma completo se conserva fuera de la ventana. En JSON, datos no disponibles/desbordados son null, con conteos y flags de desbordamiento; nunca se infiere un p95 medido de cero.

`FrameTimingSampler` usa CaptureFrameTimings/GetLatestTimings con disponibilidad y timestamp único, mantiene CPU/GPU separados y omite cuatro capturas en límites de fase/reanudación. El API devuelve la cantidad realmente disponible y entrega el frame completo más reciente primero, según [Unity 6000.3](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/FrameTimingManager.GetLatestTimings.html). Frame Timing Stats ya estaba habilitado: ProjectSettings/ProjectSetup no cambian. La latencia/disponibilidad es propia del [Frame Timing Manager](https://docs.unity.com/en-us/engine/6000.3/manual/analysis/graphics-performance-profiling/profile-rendering/frame-timing-manager/manager); esta instrumentación no certifica el hardware.

`ThermalMonitor` conserva estado actual, peor de sesión y segundos por estado; editor=unknown. `Plugins/iOS/SquashThermal.mm` lee [NSProcessInfo.thermalState](https://developer.apple.com/documentation/foundation/processinfo/thermalstate-swift.property). Este puente **runtime** es necesario en dev y prod; es independiente del puente de diagnóstico Spike/DevPanel, que sigue excluido de prod. No se modifica la política térmica del experimento.

## TDD y verificación

**EditMode 250/250, PlayMode 92/92**, cero fallos/omitidos y XML frescos, sin errores CS. [Manifiesto de pruebas y fuentes](../validation/evidence/2026-10-23-i5a-tuning-core-tests.json).

| Ciclo | RED válido | GREEN |
|---|---|---|
| Estado de edición | 6/6 fallos | 6/6 |
| Store y recuperación | 4/11 fallos | 11/11; cobertura final ampliada |
| Estadísticas | 6/6 fallos | 6/6 |
| JSONL | 3/3 fallos | 3/3 |
| Pausa/estado inyectado, P1/P2 | 4/82 fallos | 82/82 |
| Sensores | 3/3 fallos | 3/3 |
| Integración arranque/segmentos/fases | 5/5 fallos | 5/5 |
| Plano float del guard | 2/92 y enfocado 2/2 fallos | 92/92 |
| Aviso sin selección activa | 1/1 fallo | suite final 250/250 |

Un primer filtro enfocado omitió los parámetros del fixture y produjo **0 tests**: rechazado, no evidencia de aprobación; sustituido por 2/2 RED reales. El intento completo 90/92 tampoco se declaró GREEN. Fuentes probadas coinciden por SHA-256 con las del commit de implementación; motores/preset/settings/spec y escenarios sin cambios.

**Exports limpios dev/prod verificados y build dev iOS compilada y firmada:** **0.1.0+7ce86f6 dev**, gitDirty=false, Player no Development, IL2CPP/Xcode Release ARM64, firma estricta, BuildInfo y DefaultPreset exactos comprobados en el artefacto. Hash f6a394a12daf. [Evidencia saneada de build](../validation/evidence/2026-10-23-i5a-native-build.json). Ambos exports incluyen SquashThermal.mm runtime; prod excluye DevPanel y el puente Spike. Símbolo local `t _SquashThermalState` presente en Mach-O y wrapper IL2CPP con llamada directa comprobado en dev/prod. Prod exportado, sin compilación Xcode adicional.

Xcode no encontró el destino físico conectado; se compiló con **generic/platform=iOS**, sin cambiar proyecto o herramientas. El intento sin destino disponible no se cuenta como compilación exitosa. Fuentes compiladas coinciden con los hashes de las pruebas. La primera exportación marcada dirty por documentación pendiente se sustituyó por exports limpios antes de esta verificación.

**Sin instalación, campaña ni medición en el iPhone en I5a.** No se atribuyen valores de CPU/GPU/térmico o corrección física en dispositivo a esta compilación; la pausa y los sensores se verifican con regresión de editor y el enlace nativo. El perfil embebido se comprobó vigente hasta 2026-10-28T14:59:31Z. Debe renovarse **antes de cualquier instalación desde el 2026-10-27**, y comprobarse siempre antes de instalar. Herramientas sin cambios.

## Entrega y límites

**Se entrega I5a con PR e informe y se detiene para revisión de Claude.** I5a sigue abierto, sin fusionar. No se adelanta el panel T5.3, activación de edición mediante panel, calibración, ayudas ni gameplay posterior. La comprobación física/aceptación que corresponda a I5b/T7.1 sigue pendiente de su autorización. Spec intacta y entrada de backend preparada para T7.3, escrita solo con autorización PO.
