# I5b / T5.3 — panel autogenerado

Estado, 2026-10-24: implementación y regresión completas, pero **verificación nativa de persistencia/portapapeles no aprobada**. PO reporta que el preset no apareció tras reiniciar y que importar falló con «Invalid preset JSON: Unexpected JSON token at offset 0». Se recuperaron Po-1.json y last-active.txt, ambos coherentes, con los 32 valores por defecto (floor restitution=0,45). Falta recuperar registros de arranque y confirmar mensajes exactos de Guardar/Exportar para aislar la causa. Rama `firstball/i5b-dev-panel`, [PR #9 en borrador](https://github.com/rafaallendes-boop/squash-clash/pull/9). I5b abierto, sin fusionar ni adelantar T5.4, calibración o gameplay.

## Autorización y cierre previo

Claude aprobó PR #8 / I5a, head 98b8139, según comunicación del PO. PR ya lista y fusionada con merge commit **a13ae527d88736af0380e5dc95480043eaa3ad68**; main sincronizada. Cierre y autorización I5b registrados en 8072b88, AGENTS y [aprobaciones](2026-10-21-first-ball-po-approvals.md#cierre-de-i5a-y-autorización-i5b--2026-10-23). Worktree I5a eliminado, artefactos locales preservados en Logs/archive-i5a-20261023, ignorado. Las cinco observaciones de Claude quedaron registradas sin cambios de código en ese cierre.

## Comportamiento entregado

Panel UI Toolkit construido desde código exclusivamente en DevPanel. Botón DEV dentro de safeArea y tres dedos sostenidos 1 s para alternar; EnhancedTouch habilitado/deshabilitado de forma balanceada. Se usa el inicio del último dedo para exigir el segundo completo simultáneo y reiniciar ante sustitución. Una apertura por gesto sostenido, sin repeticiones; tras perder foco exige soltar. [API oficial EnhancedTouch](https://docs.unity.cn/Packages/com.unity.inputsystem%401.19/api/UnityEngine.InputSystem.EnhancedTouch.Touch.html); el paquete local documenta startTime en la misma escala que realtimeSinceStartup. La capa «Toca para continuar» mantiene prioridad y bloquea panel/barra; no se elude por gesto o botón. El panel ocupa la mitad inferior útil y conserva la cancha y la barra fija. El selector L1–L6 queda expresamente para T6.3.

Parámetros generados por Group/ParamDef: slider Float/Int con entrada, valor y unidad; toggle Bool; desplegable con etiquetas Choice. Se cuantizan las entradas numéricas a Step y se capturan ArgumentException de TuningState.Set sin romper la app. No finitos/opciones inválidas muestran error. El impacto máximo queda deshabilitado y rotulado **«no operativo con P1»** según el backend del vuelo activo. Encabezado con preset, backend y pendientes; **Aplicar y relanzar** y **Lanzar** llaman a LaunchCurrent: ApplyPending → EffectiveConfig.From(Applied) → Host.Launch. El evento Launched registra exactamente ese snapshot y cero pendientes.

Cambios Immediate llegan a cámara/referencias y al reloj sin alterar la física en vuelo. Repetir y la repetición automática conservan estado inicial, horizonte, condición de parada y física aplicada; incorporan los Immediate actuales, manteniendo pendientes físicos. Intervalo en tiempo real, cero deshabilita, sin ráfagas de recuperación; pausa/foco y el primer delta de reanudación se excluyen. No se cambia Reset de P2 ni sus ecuaciones. El host conserva el guard de estado válido y las suites P1/P2 de I5a.

**DefaultPreset.json sigue como única fuente de defaults.** Solo se añaden los dos valores previstos en T5.3: launcher.repeatSeconds=0 (0–10 s, Immediate) y debug.slowMotion=1 (0,1–1, Immediate). Todos los valores previos se verificaron idénticos. Hash efectivo nuevo: **164b0c21e88b**, antes f6a394a12daf. P2 sigue por defecto y P1 configurable como respaldo; SimulationKey sigue usando física/cancha. Usar cámara lenta marca las métricas como inválidas de forma persistente en el reloj, incluso al volver a 1 o relanzar.

Presets: Guardar como incluye Visible/pendientes, escritura atómica existente y selección activa; cargar lista o Default; exportar JSON visible al portapapeles; importar con TryParse y avisos de Overlay visibles. Un JSON inválido no altera el estado. Importar deja explícitamente un estado **sin guardar** y requiere Guardar como para persistirlo; no sobrescribe archivos por importar. El nombre **Default** se reserva sin distinción de mayúsculas en PresetStore (también rechaza selección/listado homónimos); cargar el builtin limpia last-active. El arranque recupera el archivo seleccionado sobre Defaults sin pendientes. Las métricas conservan nombre, hash de archivo/JSON y hash efectivo separados; una edición pendiente no se presenta como física ya aplicada.

Diagnóstico actualizado cada 0,5 s: FPS, p50/p95/p99 ventana/sesión, >33 ms/min, CPU/GPU con N/D cuando no existe muestra, térmico actual/peor, PredictionMatchesBitwise, DroppedSteps y aviso **MÉTRICAS NO VÁLIDAS** tras usar cámara lenta. Últimos ocho contactos con superficie, UV, Timing, persistencia y líneas tocadas usando huella r=0,010 m, explícita e independiente del radio físico. Los contactos conservan orden de llegada. No se añaden ayudas de T6 ni reglas de punto.

## TDD y regresión

**253/253 EditMode y 105/105 PlayMode**, cero fallos/omitidos, XML frescos y sin errores CS. [Manifiesto final y hashes de fuentes](../validation/evidence/2026-10-23-i5b-responsive-tests.json). Siete grupos de RED válidos antes del comportamiento:

| Grupo | RED válido | GREEN |
|---|---|---|
| Lanzamiento, Immediate, repetición | 3/3 fallos | 3/3 |
| Defaults/catálogo y nombre reservado | 4/10 fallos | 10/10 |
| Panel, controles, gesto, diagnóstico, presets | cuatro aserciones del grupo + preset enfocado 1/1 | 5/5 |
| Immediate al repetir | 1/1 fallo (slowMotion volvía a 1) | 9/9 integración/panel |
| Repetición tras reanudación | 1/1 fallo (lanzamiento por delta de vuelta) | 5/5 integración |
| Responsive y entrada en pestañas | 1/1 fallo, viewport comienza 42,54 puntos antes del final de pestañas | dos asignaciones 390/320 puntos; 7/7 panel |
| Escritura numérica sin sobrescritura | 1/1 fallo con teclado UI Toolkit: escribir 0. modificaba tuning a 0 | edición parcial preserva 0,45; confirmar 0,60 aplica 0,6; suite final 105/105 |

El primer intento del caso de presets acabó con una aserción NUnit inválida sobre null: **ese caso se rechazó como RED**, y se volvió a ejecutar con un stub que produjo Expected True/False real antes de implementar. El primer intento GREEN del panel fue 4/5 porque el test modificaba un control sin adjuntarlo a un panel; se corrigió el montaje para usar la UI real y pasó 5/5. No se contó ninguno de esos intentos como éxito ni se aceptaron cero tests/XML anteriores.

## Corrección solicitada por el PO

El PO precisó **«Presets guardar como no lo encontre, el resto funcionó, lo que si habría que mejorar la responsividad, ya que se sobreescribe todo»**. El agente pudo conectar con Duplicación tras autenticación manual de la Mac por el PO. En 11e505f observó contenido de parámetros sobre las pestañas; pulsar Presets accionaba un Foldout por encima. La fila encogía al competir con la altura intrínseca del ScrollView. La regresión mide tabsBottom=570,20 y viewportTop=527,66: invasión de 42,54 puntos.

Cabecera compacta y pestañas con altura conservada; viewport flexible con tamaño base cero, mínimo cero y recorte en el panel, conservando la altura del contenido. Las pestañas indican cuál está activa y reciben la entrada; campo **Nombre del preset**, botón explícito **Guardar como…**, controles en columna y mensajes que se revelan al operar. [FlexShrink de UI Toolkit](https://docs.unity3d.com/cn/6000.0/ScriptReference/UIElements.IResolvedStyle-flexShrink.html). En la prueba corregida tabsBottom=534,00, viewportTop=537,62; hay separación y Pick alcanza el botón real. Dos asignaciones de UI adjunta, ancho 390/alto panel 304 y ancho 320/alto 200; no se presentan como capturas del iPhone. El primer GREEN responsive aún invadía 1,81 puntos por márgenes y **no se aceptó**; corregido antes del GREEN final.

La inspección nativa de 5b9d564 y 1318753 confirmó pestañas separadas y accesibles. Al escribir 0.60 se detectó además la confirmación por cada carácter. Nuevo RED por teclado real de UI Toolkit; edición numérica diferida hasta Enter/perder foco y refresco que conserva el campo enfocado. La prueba inicial de propiedad isDelayed se sustituyó por la prueba de comportamiento (no se cuenta como grupo RED adicional). Fuente final **1318753**, defaults intactos y sin cambios de motor.

## Validación nativa y límites

Build final dev **0.1.0+1318753**, gitDirty=false, Player no Development, SQUASH_DEV temporal, IL2CPP/Xcode Release, ARM64 y firma estricta comprobados. Exports limpios dev/prod **RELEASE-CHECK: OK**; prod sin módulo DevPanel ni puente Spike, puente térmico runtime presente en ambos. BuildInfo y DefaultPreset embebidos exactos y fuentes cotejadas con los hashes de las suites finales 253/105. Xcode 27.1 (27A9275), Unity 6000.3.27f1; herramientas sin cambios. Xcode compiló con generic/platform=iOS. Prod exportado, sin compilación Xcode ni instalación en este incremento. [Build final](../validation/evidence/2026-10-23-i5b-responsive-native-build.json).

Perfil embebido vigente comprobado antes de instalar, compatible con dispositivo/bundle, vence **2026-10-28T14:59:31Z**. Instalación exitosa el **2026-10-23 local**, antes de la fecha de renovación obligatoria del 27. Lanzamiento automático rechazado con CoreDeviceError 10002 y causa subyacente **Locked**; no se atribuye un error de la app a ese rechazo.

Evidencia histórica de 11e505f: el PO abrió manualmente y reportó el recorrido correcto, luego aclaró que no encontró Guardar como… y pidió responsividad. Dos archivos de métricas, 15 registros/4 lanzamientos, **5.000 muestras CPU y 5.000 GPU**, térmico actual/peor nominal: sensores disponibles. Dos segmentos con descartes (195 y 185) quedaron metricsValid=false, conservados sin atribución causal ni evaluación de rendimiento. Solo había Default/config 164b0c21e88b y no carpeta presets; no se contó como guardado ni recuperación aprobados. El fallo inicial de acceso visual del agente fue superado posteriormente tras la autenticación del PO. [Auditoría histórica](../validation/evidence/2026-10-23-i5b-native-functional.json), [build inicial](../validation/evidence/2026-10-23-i5b-native-build.json).

**Comprobación parcial en 1318753, 2026-10-24:** pestañas accesibles, apertura de Presets y campo de nombre visibles; floor restitution=0,60 ajustado con el slider y aplicado, encabezado con cero pendientes. La inyección de teclado de Duplicación no permitió certificar la escritura decimal; la regresión de teclado UI Toolkit sí está en GREEN, pero no se presenta como validación de teclado nativo. Se escribió el nombre para el preset; la conexión se perdió antes de pulsar Guardar como…. El dispositivo continúa conectado según CoreDevice, pero al reintentar Duplicación macOS exige Touch ID o contraseña de la Mac; solo el PO puede completar esa autenticación. Guardado, reinicio y export/import siguen pendientes, sin declararlos aprobados. No se pidió repetir el recorrido completo ni la campaña física. PR #9 sigue en borrador.

Pausa manual y gesto se conservan como reportados por el PO en el recorrido inicial; no se atribuyen a observación directa del agente. No campaña física ni sesión de rendimiento: no se evalúan criterios ni umbrales con estos datos.

## Resultado de la comprobación del PO — 2026-10-24

El PO prefirió ejecutar la comprobación directamente en el teléfono. Reportó que los pasos 2 y 3 fallaron: el encabezado no mostraba «Po-01» (primera letra mayúscula) y apareció **«No se importo: Invalid preset JSON: Unexpected JSON token at offset 0»**. No se da por aprobado reinicio ni export/import. Se solicitaron los mensajes exactos de Guardar como… y Exportar, sin pedir repetir la prueba.

La copia de Documents/presets tuvo éxito: existe **Po-1.json**, selección **Po-1** en last-active.txt, build 0.1.0+1318753 dev, schema 1 y 32 parámetros. El archivo es JSON sintácticamente válido para un parser independiente; sus valores coinciden con Default, incluido floor restitution=0,45, hash efectivo 164b0c21e88b. Esto corrobora un guardado físico de Po-1; no prueba que sea el nombre Po-01 informado ni que el arranque lo cargara. No se reemplaza evidencia del PO con una inferencia a partir del archivo.

La recuperación de métricas falló con CoreDeviceError -1 y el listado posterior con 4016, sin conectividad/servicios necesarios. Se pidió conexión por cable y desbloqueo para recuperar registros sin Duplicación. El error de importación indica que el parser rechazó el texto en el inicio; no identifica por sí solo la causa ni demuestra un fallo de la API del portapapeles. Sin cambios de código mientras se aísla el problema. [Diagnóstico saneado](../validation/evidence/2026-10-24-i5b-native-diagnostic.json). El bloqueo de autenticación anterior queda como histórico; el próximo paso es recuperar los registros y confirmar los mensajes.

## Observaciones de Claude y próximos gates

- TuningState.Set: cubierto aquí por controles válidos y captura visible de errores.
- Nombre Default: reservado aquí; comprobación de persistencia en arranque incluida.
- Pausa manual: reportada correcta por PO en el recorrido; sensores CPU/GPU/térmico corroborados con archivos nativos de la build. Falta resolver la persistencia, como se detalla arriba.
- Logs de métricas también en prod y sin retención: **definir política antes de T8**, sin cambio en I5b.
- Percentiles de segmento por borde superior de cubeta, sesgo ≤0,1 ms; ventana exacta y sesión por histograma. No se convierten en medidas exactas ni se cambian umbrales.
- Renovar perfil antes de cualquier instalación desde **2026-10-27**, comprobar siempre vigencia; sin cambiar herramientas.

Spec, escenarios, criterios, umbrales, motores P1/P2, ProjectSettings y plan intactos. No se repite el experimento ni se declara rendimiento nativo a partir de regresiones/compilación. No se congela un preset de calibración. I5b queda abierto para revisión; T5.4 y posteriores no iniciados.
