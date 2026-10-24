# I7a — aceptación y validación integral de First Ball (T7.1–T7.2)

**En curso, entrega todavía incompleta.** Rama `firstball/i7a-validation`, PR en borrador para trazabilidad. PO autorizó T7.1–T7.2 el 2026-10-24 tras aprobar/cerrar PR #12 (merge 7d7fd3a421255ae02558840d5ff9f2a669fce0d0); main sincronizada, worktree L2 eliminado. **T7.3/gameplay no autorizados.** No se cierra I7a ni se fusiona su PR.

## Alcance y pruebas

Fuente de producto **e0f9071**: soporte dev de H17 con sesión de 900 s, 150 s por L1–L6 y repetición elegida por `launcher.repeatSeconds` (temporal 3 s para el protocolo, sin guardar). Se ejecutan lanzamientos normales, no SpikeRun ni benchmark. Inicio exige P2, repetición positiva, sin pendientes ni cámara lenta; en iPhone exige térmico nominal. Cierra/bloquea panel, deshabilita las demás acciones y permite cancelar. Foco/segundo plano interrumpen la sesión y persisten resumen incompleto, nunca Pass. Progreso visible sin panel.

Histogramas independientes abarcan **toda** la sesión, incluidos relanzamientos y cambios de L; se informan p50/p95/p99, frames >33 ms/min, CPU/GPU p95 con conteos, tiempo por térmico y peor estado, DroppedSteps, duración por caso, brillo nativo inicial, hashes y fechas. `Phase=session` desde el inicio hasta el último segmento, restaurando fase/semilla después. Sin promediar percentiles de segmentos. Percentiles por borde superior de cubeta (sesgo ≤0,1 ms ya documentado); null sin muestras=N/D, null con muestras=overflow >250 ms. Condiciones de brillo/bajo consumo también se registrarán por el PO.

Dos grupos RED→GREEN: ausencia del control delimitado (3/3 RED→GREEN) y reinicio con L1 ya seleccionado/progreso exterior (1/1 RED→GREEN); conjunto final 4/4. Batería final **270/270 EditMode y 136/136 PlayMode**, cero fallos/omitidos, XML frescos/sin errores CS. [TDD, hashes y fuente](../validation/evidence/2026-10-24-i7a-tests.json), 121 fuentes cotejadas. Los experimentos están separados de la regresión.

Sin cambios de motor, host, escenarios, preset, ProjectSettings, criterios, umbrales ni spec. `DefaultPreset.json` sigue única fuente de defaults. El encabezado nativo del spike añade SimulationKey para auditar configuración; no altera las mediciones. H17/control/progreso quedan excluidos de prod mediante el ensamblado DevPanel existente.

## T7.1 — aceptación de física

EditMode y PlayMode editor completos sobre fuente **19aae3b**: 18 filas únicas cada uno, diez criterios de gate aplicables Pass, C17 informativo con 200 muestras. p95 editor 0,0219 / 0,0207 ms; no se usa para aprobar C17 nativo ni se interpreta frente al iPhone. Física/cancha/spike idénticos byte a byte a la fuente de la build e0f9071. [Auditoría editor](../validation/evidence/2026-10-24-i7a-p2-editor.json), informes [EditMode](../validation/evidence/physics-spike-P2/editor-19aae3b-I7a-editmode.md) y [PlayMode](../validation/evidence/physics-spike-P2/editor-19aae3b-I7a-playmode.md).

Campaña nativa **pendiente**. Antes cargar Default, no P0-calib-1, y comprobar hash **0d39e6016bd8**, P2, cero pendientes, ProjectPhysics. Condiciones de campaña aprobadas: teléfono físico, No molestar, batería ≥80 %, bajo consumo apagado, sin funda, térmico nominal al inicio, sin capturas/duplicación/devicectl/herramientas durante ejecución. Recuperar Documents/spike tras aviso del PO y auditar 18 filas, variante, hash/key, 200 muestras C17, térmico de benchmark y foco. C17 p95≤1 ms; **cualquier gate antes Pass que falle obliga a detenerse como regresión**. No se declara aceptación nativa aún.

### Trazabilidad de configuración

[Comparación saneada](../validation/evidence/2026-10-24-i7a-config-traceability.json). Gate original I4d: hash **c86243b15a97**, fábrica P2 inyectada explícitamente aunque el archivo decía `physics.backend=0`; SimulationKey cruda **04372f672e9e**. Al representar la selección efectiva P2 (backend=1), key **166a96e319c7**, igual a Default actual. La key cruda histórica no se presenta como idéntica ni se reescribe el informe aprobado. Cambio de selección ya aprobado en I4e; los demás valores compartidos son idénticos. La selección real de fábrica/variante siempre fue P2.

De c86243b15a97 a **f6a394a12daf** cambió solo backend por decisión I4e. De ese preset a **0d39e6016bd8** se añadieron once claves no físicas: launcher.repeatSeconds, debug.slowMotion, camera.mode/followDamping/followMaxOffset y aids.shadow/trail/impactMarks/prediction/trailSeconds/ballVisualScale. Trece valores de physics/court, bounceThreshold=0,1 y variante analytic/r2-20261023 se conservan. La igualdad de configuración efectiva se confirma también en test C# de EffectiveConfig.

P0-calib-1 **8de3f2ecb217** es el preset aceptado de Etapa 5; difiere solo en repetición=3. No se usa en la campaña ni se promueve a defaults. Po-1 sigue prueba funcional.

## Release e iPhone

Exports limpios **dev/prod RELEASE-CHECK: OK**, fuente e0f9071 limpia. Módulos producto: Core/Runtime/DevPanel en dev, solo Core/Runtime en prod; puente nativo de spike solo dev, térmico runtime ambos. Xcode/Unity sin cambios. Compilación Release ARM64 firmada y vigencia efectiva del perfil: **pendientes de completar y enlazar**. Perfil conocido vence **2026-10-28T14:59:31Z**: comprobar antes de toda instalación y renovar antes de instalaciones desde el 27.

Instalación dev/campaña nativa, instalación prod y arranque `devpanel=absent`, ausencia de DEV y tres dedos inertes: **pendientes**. No inferir verificación funcional del export. Respaldo de presets aún bloqueado por conexión CoreDeviceError 4016; PO avisado para conectar/desbloquear por cable. Sin instalación efectuada en I7a todavía.

## T7.2 — H17 y plantilla

Sesión de rendimiento nativa **pendiente**; pruebas sintéticas de 900 s no constituyen la medición en iPhone. Build dev, estado térmico nominal al inicio, brillo/bajo consumo anotados, panel cerrado, sin cámara lenta/spike/benchmark; L1–L6, repetición temporal 3 s y Phase=session. Tras 15 min recuperar JSONL y resumen para confrontar §17.C: p95≤16,7 ms, p99≤25 ms, >33 ms≤1/min, sin serious/critical, DroppedSteps explícitos. Un resultado insuficiente se informa sin ajustar umbrales/defaults.

[Plantilla de cinco secciones para completar por PO](../validation/first-ball-validation.md): H1 ya confirmado globalmente, datos no comunicados permanecen pendientes; física, rendimiento, invariantes §17.A.1/2/3/10/14 y decisión del PO. No declara First Ball validado ni cierra Etapa 7.

Retención de logs sigue pendiente antes de T8. No se interviene el panel de importación ni la sombra experimental. No T7.3, calibración nueva ni gameplay. Entrega/revisión final **pendientes de completar las comprobaciones nativas**.
