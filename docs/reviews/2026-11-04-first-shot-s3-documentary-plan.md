# First Shot — plan documental de S3 con S2/M2-4

Fecha: 2026-11-04, America/Santiago. **Propuesta de secuencia documental, sin autorización de implementación.** No modifica ni sustituye el [plan First Shot r2, tareas 3.1–3.4](../superpowers/plans/2026-10-25-squash-clash-first-shot.md), la [propuesta r2/D3/D4](../superpowers/specs/2026-10-24-first-shot-design.md) o la [spec v0.3](../superpowers/specs/2026-10-20-squash-clash-design-v0.3.md). Concreta sus dependencias sobre el producto 9131b1c de PR #30, aún sin fusionar. El PO solo autorizó redactar este plan; S3–S4/T8 siguen sin implementación.

## Punto de partida y fronteras

S1 aceptado con M2-4. S2 tiene planificador, contrato, solver, rollback y práctica; control2 por defecto, A′ y S1-sling configurables. Las suites 522/522 EditMode y 297/297 PlayMode son evidencia anterior aprobada, no ejecuciones de este trabajo. Tanda 1 terminó con desviaciones declaradas; las cinco repreguntas retrospectivas del salto 09–13 fueron respondidas «si», sin borrar las originales. Tanda 2 tiene 14/15 completos, 16–22 pendientes; tanda 3 y PROD aún pendientes. No inferir aceptación global de S2.

Ahora solo existen notas y dos sonidos provisionales runtime, contacto/swish; [ShotFeedback](../../Assets/SquashClash/Runtime/Feedback/ShotFeedback.cs) no distingue todavía Perfect/Valid ni implementa háptica nativa, destello de impacto, vectores causales o swing visible. [PROVENANCE](../../Assets/SquashClash/Runtime/Resources/Audio/PROVENANCE.md) documenta esos clips provisionales. No presentar los tests pendientes del plan S3 como existentes o verdes.

El [diagnóstico SL5/SL6](2026-11-04-first-shot-sl56-trajectory-diagnosis.md) y sus opciones quedan separados: S3 no debe cambiar la semántica de Aim, el solver, SL, la dirección del tirón o las ayudas para resolver silenciosamente esa observación. Las modificaciones de calidad previstas por S3 son explícitas y anteriores al solver; no autorizan una trayectoria posterior garantizada.

Mantener una sola autoridad de pelota, input, reloj, planificador y predictor. En M2-4, Aim queda fijado en Began y P final en Ended; r original, plan visible, cuantización R2-1/R2-4, N2 estricto, C-A, intento único y procedencia de hashes se conservan. El cuerpo del lanzamiento no se reinterpreta a partir de la mano de interfaz; AX-1 permanece.

## Secuencia para una futura autorización propia

| Incremento futuro | Entrega revisable | Dependencias |
|---|---|---|
| 3.1 Calidad causal | Vectores no direccionales registrados y aplicados antes del solver | Orden explícita del PO para implementar; fuente S2 fijada y observaciones abiertas conservadas |
| 3.2 Feedback | Háptica, niveles de audio y destello runtime, procedencia de medios y validación prod | Identidad del resultado/contacto; causalidad de 3.1 si se quiere evaluar la diferencia de calidad completa |
| 3.3 Vista e instrumentación | Swing procedural, explicación visual causal y latencias/costes observables | Identidades de orden/contacto/efecto; 3.1/3.2 integrados sin duplicar eventos |
| 3.4 Validación | Campañas dev/prod y dos sesiones audio de 15 minutos, juicio H4 | Regresión/release/build y autorización física propias; perfil vigente para cada tramo y restauración |

Cada incremento futuro usa rama/worktree/PR y revisión Claude antes de fusión según reglas vigentes, con TDD por aserciones: esqueleto compilable → RED válido observado → implementación → GREEN focal y regresión fresca. Ninguna de esas ejecuciones se realiza ahora. Aprobación de este documento no constituye autorización para código, build S3, instalación o campaña.

## 3.1 — Calidad causal no direccional

**Integración prevista:** CausalQuality/CausalVector del plan, alimentados por BodyFrame del lanzamiento y contacto físico reconstruido, lado Drive/Backhand y estado Perfect/Valid. Calcular una sola vez al resolver la orden, antes de ShotSolver; conservar Aim/P solicitados, componentes causales, objetivo/velocidad tras la capa, TargetUsed y salida física. No corregir velocidad/dirección después de resolver para forzar un acierto.

- Timing: solo por geometría relativa del contacto, no por distancia a V, magnitud de Δ o categoría. Perfect elimina el vector de timing, pero no los de estirado E3/atorado E5.
- E3/E5 y timing simétrico solo afectan altura/velocidad/soltura autorizadas por C2. Componente lateral siempre cero. E1/E2/E4/E6 y aleatorio continúan fuera de alcance.
- Conservar los nombres y valores experimentales ya propuestos en T3.1: timingHeightPerM0.6, timingSpeedPerM−0.25, stretchStart0.85R, jamDistance0.35m, stretchHeight0.4m, stretchSpeed−0.2, jamHeight0.3m, jamSpeed−0.15, capHeight1.0m, capSpeed−0.4. **Son referencias del plan, no parámetros aplicados, nuevos defaults aprobados o resultados de calibración.**
- La aplicación exacta de límites por componente, acumulación y signo debe quedar en el encargo revisado antes de implementar. No elegir aquí una política que cambie el significado del plan. Claves nuevas futuras según R2-3, registrando hash extendido y compatibilidad; no cambiar las 106 claves previas, los 41 valores heredados, SimulationKey, SL o P0 por inferencia.

**Cobertura ya exigida por el plan, todavía futura:** Perfect conserva E3/E5; crecimiento de timing por distancia geométrica; lateral cero; límites por componente; simetría; aplicación antes del solver; y **SameGeometryDifferentDelta_SameShot literal**. La tensión entre categoría Perfect y geometría idéntica con distinto Δ ya está registrada en r2 §11.3: si aparece incompatibilidad, elevar al PO sin acotar pruebas, criterios o invariante para pasar. No resolverla silenciosamente en este plan.

**Límite de interpretación:** subir altura o reducir velocidad por causalidad puede cambiar la salida cercana al frontis. Se registra como efecto causal solicitado y ajuste del solver, no como corrección de SL5/SL6. No extrapolar este diagnóstico sin capa causal a los futuros resultados S3.

## 3.2 — Háptica, audio y destello

**Ruta prevista:** extender ShotFeedback sobre la identidad de orden/contacto aplicada y los contactos canónicos de superficies. Háptica UIKit con puente mínimo Objective-C++ y wrapper runtime, sin dependencia de DevPanel. Preparar/reutilizar el generador al armar el gesto según el plan; emitir el impacto al resolver el contacto físico, no en cada muestra ni al fijar Aim. Cancelación, omisión, orden descartada y fallo del sistema no se convierten en un golpe válido.

Perfect: sonido más fuerte, háptica firme y destello; Valid: sonido/háptica más discretos; Failed: swish sin háptica. Frontis, laterales y tin tienen sonidos propios y alternativas visuales. Respetar identidades y reconciliación: no repetir efectos por versiones append-only o rollback; un sonido/háptico ya emitido es irreversible y, si queda obsoleto, se cuenta sin afirmar que fue borrado.

Precargar clips y generators; no cargar recursos en el contacto. Cada medio final necesita entrada de procedencia/licencia/origen en PROVENANCE antes de incluirlo; los clips provisionales de S2 no acreditan las diferencias finales. Conservar alternativa visual con silencio o háptica desactivada. No cámara lenta ni sacudida de cámara por feedback. Toggle feedback.haptics runtime según T3.2; no exponer controles DEV en prod.

**Cobertura futura de T3.2:** tres niveles, fallo sin háptica, superficies/tin, toggle sin perder alternativa visual, precarga, procedencia y ausencia de cámara lenta/sacudida. Extender check-release con símbolo nativo SquashHaptics en prod, sin confundirlo con el puente térmico dev que debe seguir excluido. Verificar también que la llamada de feedback no duplique el contacto ni cambie clasificación/tiempo r.

Fuentes oficiales para la implementación futura: [UIImpactFeedbackGenerator](https://developer.apple.com/documentation/uikit/uiimpactfeedbackgenerator), [prepare()](https://developer.apple.com/documentation/uikit/uifeedbackgenerator/prepare()). El plan existente prescribe hilo principal/preparación; la disponibilidad y semántica exactas del SDK instalado se cotejarán antes de escribir el puente. Una solicitud de API no acredita percepción ni latencia física.

## 3.3 — Swing, explicación e instrumentación

Swing procedural mínimo sobre placeholders, brazo/raqueta simples, sin rig ni personajes nuevos. Drive, revés y golpe al aire, con poses de estirado/atorado/adelantado que expliquen la geometría sin cambiarla. Debe obedecer el contacto confirmado/programado y sus identidades; la animación no es una segunda simulación ni autoridad de tiempo. El Aim fijado y la marca factible siguen explicando lo pedido/usado.

Superposición causal runtime para entender el fallo sin abrir DEV: componentes y causa por separado, sin tapar rival/frontis ni filtrar la interpretación de las preguntas del PO. No halo, lupa, zoom, cámara nueva, dominio B ni efectos direccionales por esta tarea. Una propuesta que los necesite requiere decisión propia.

**Instrumentación futura sobre el reloj real ya existente:** evento original Ended, recepción/procesamiento, contacto físico cuantizado/programado, frame de aplicación/reconciliación, llamada de feedback y presentación disponible. Conservar campos y versiones actuales, identificar cuándo no hay dato. El coste Stopwatch de una parte es una duración de ejecución, no timestamp de input ni calibración de latencia. Latencia de llamada no equivale a emisión acústica/vibración/percepción.

Ampliar costes por parte según A09: estimador, planificador, solver, reconstrucción, predicción, reconciliación y feedback; una muestra por ejecución, nunca repetir coste de un plan por cada cierre. No sumar percentiles de partes para crear el del frame. Informar asignaciones y frame completo solo con evidencia nativa correlacionable; estado estable sin asignaciones según test del plan. No reinterpretar CPU p95 de FrameTiming como carga pura ni restar vsync/esperas. No introducir instrumentación opcional de First Ball por inferencia.

**Cobertura futura de T3.3:** swing Drive/Backhand/Air y poses; orden evento→contacto→llamada; costes por parte; estado estable sin asignaciones; y regresión de plan visible, frontera A/V/K, R2-1/R2-4, caducidad/identidad de lanzamiento, descarte/obsolescencia y hash por lanzamiento/gesto. Son dependencias de S2 que deben permanecer; no se ejecutan ahora ni se sustituyen por ceros serializados.

## 3.4 — Validación física futura de S3

No es continuación automática de 2.7 ni usa los datos de tanda1/14/15 como sesiones S3. Antes de medir, preparar protocolo y autorización propios que fijen fuente, build, perfil, escenarios, orden, preguntas y métricas vigentes. Regresión fresca sin errores CS/cero tests, restaurar transitorios, exports limpios/check-release dev/prod, builds Release ARM64/no Development, firma/perfil efectivamente incorporados.

Respaldar/exportar y cotejar P0-calib-1/Po-1/last-active antes y después de instalar. Configuración temporal sin guardar, hash literal externo/pending0/Jugando/foco, DEV cerrado. Nuevo precheck por bloque/segmento y restauración de P0 en proceso nuevo con tres archivos idénticos al cerrar cada tanda o interrumpir. Versiones append-only, todos los intentos, parciales/extras y respuestas literales; desconocido/NotEvaluated nunca aprobado. Ante defecto aplicar la parada vigente, sin ajustar.

Mantener **dos sesiones completas de 15 minutos**, una audio Default y otra Best Latency, panel cerrado/sin cámara lenta. El [manual oficial Unity 6.3](https://docs.unity3d.com/6000.3/Documentation/Manual/class-AudioManager.html) documenta que Best Latency reduce latencia a costa de recursos; esa es la comparación ya prevista, no una elección anticipada ni promesa de milisegundos. Predeclarar orden y contexto/ruta de audio, reportar ambos resultados y sesgo de orden sin modificar umbrales H17 de spec §17.C. La desviación de First Ball no se hereda a First Shot. Sesión incompleta no se suma a otra como 15 minutos continuos.

Juicios originales del plan: «¿un perfecto se siente claramente mejor?» y «¿entiendo por qué un golpe salió peor sin el panel?» (H4). Probar silencio, háptica del sistema apagada y toggle de la app, manteniendo alternativa visual y sin resultados previos que sugieran respuestas. Conservar la preferencia de tirón, saturación, N04/N04b y sensación mecánica como observaciones abiertas, no declarar rally validado por práctica. El PO pidió modales agrupados durante 2.7; para S3 documentar la modalidad autorizada al predeclarar su campaña, sin copiar respuestas anteriores.

PROD S3 requiere verificación funcional real de sonido/háptica/notas/swing con autorización propia de instalación y respaldo nuevo; el binario PROD de S2 preparado offline el4nov no acredita esos módulos todavía inexistentes. Perfil nuevo/renovación cuando sea necesario, sin asumir que la vigencia7nov servirá para una campaña S3 posterior. Entrega de informe y PR para Claude/PO, sin fusión por inferencia.

## Decisiones pendientes del PO

- Autorizar o no la implementación futura de S3 y su división en incrementos, una vez fijada la fuente de S2.
- La decisión posterior del PO mantiene la mira como punto del frontis y pide un mapa offline antes de elegir una regla de golpe jugable con ajuste visible C4. El [mapa documental](2026-11-04-first-shot-sl56-feasibility-map.md) no elige esa regla ni autoriza incorporarla a S3.
- Resolver cualquier incompatibilidad real del invariante §17.A.12 que aparezca al implementar T3.1; no anticipar una excepción.
- Autorizar protocolo/instalaciones/campañas S3 y disponibilidad, con fuente de audio/háptica y condiciones verificadas.

Este plan termina en documentación revisable. No solicita ejecutar esas decisiones ahora ni modifica spec, plan, defaults, bandas o criterios.
