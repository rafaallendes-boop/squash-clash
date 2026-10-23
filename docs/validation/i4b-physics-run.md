# I4b — ejecución de la prueba física P1

Estado actual, 2026-10-23: **Claude aprobó el orden canónico de c7da32e; PO autorizó build y campaña nativa**. [PR #4 borrador](https://github.com/rafaallendes-boop/squash-clash/pull/4), sin fusionar. Rama firstball/i4b-physics-run, base main 6fd04e0. I4b abierto: campaña completa/C17 nativo e informe final pendientes. I4c, P2/T4.5 e I5+ no autorizados.

[Autorización y condiciones](../reviews/2026-10-21-first-ball-po-approvals.md#revisión-claude-de-c7da32e-y-autorización-nativa--2026-10-23) · [Clasificación vigente y condiciones de C17 editor](../reviews/2026-10-22-first-ball-physics-spike-P1.md#revisión-de-claude-sobre-c7da32e-y-clasificación-vigente). La detención previa que se conserva debajo como historial quedó satisfecha por esta revisión; no autoriza cierre, merge ni I4c.

## Preparación nativa actual

Build dev **2fdede5 instalada**, export limpio, Release ARM64 firmada, Player no Development y metadata embebida verificada. Prod nuevo excluye DevPanel y el puente nativo. Perfil efectivo vigente hasta 2026-10-28T14:59:31Z, comprobado justo antes de instalar; herramientas intactas. [Comprobación corta ProjectPhysics fastMotionThreshold=0.01 y procedencia](2026-10-23-i4b-native-preflight.md), [evidencia saneada](evidence/2026-10-23-i4b-canonical-native.json).

iOS rechazó el arranque por teléfono bloqueado; pendiente de desbloqueo del PO. Campaña aún sin iniciar. El informe I4b no está terminado: faltan campaña física completa y auditoría de sus 18 filas/hash/variante/200 muestras C17/térmico benchmark/foco. C17 mantiene p95 ≤1 ms (Fail si incumple). No habrá herramientas durante la ejecución y Documents/spike se recuperará solo cuando el PO confirme término. Sin cierre ni merge.

## Entrega de orden canónico

La decisión PO 1b y la clasificación final de Claude están registradas literalmente en [aprobaciones](../reviews/2026-10-21-first-ball-po-approvals.md). Antes de cambiar comportamiento se repitieron baseline/speculative sobre 1c47621: baseline C7 3/3 solo orden; speculative C6 138 solo orden + 1 trayectoria/contactos distintos, C7 183/183 solo orden; C5 cero en ambas. Registro de cada divergencia, no una extrapolación del primer ejemplo.

RED físico: S8-109, checkpoint 50, paso 127, estados y multiconjunto completo de contactos iguales, salida LeftWall/Floor frente a Floor/LeftWall. El intento aislado de historia reducida no reprodujo el fallo; se preserva como control, no como RED. El prefijo real hasta S8-109 sí falla 1/1 antes de la corrección. Orden del adaptador al terminar Simulate: TimeInStep (NaN desconocido al final), superficie, persistencia y desempate completo por campos/bits. No ordena/agrega en el criterio. Comparador, C7, Reset, umbrales, escenarios, geometría, preset y variantes intactos.

Regresión final **EditMode 188/188, PlayMode 58/58**. Cinco experimentos nuevos **1/1 cada uno**, con 10.053 lanzamientos de robustez y 200 medidas C17 por variante. Tres editores al principio y dos después, en worktrees aislados: tiempos informativos, sin comparación aislada ni certificación nativa. Resultados 16ad653 preservados como históricos.

Baseline nueva: C1 cero cruces; C5/C6/C7 pasan (C7 18.753 medidos, 1.279 no aplicables, cero no evaluables). C3 conserva cinco fallos; C10 nueve (13,6164427 mm máximo); C11 tres/6. Speculative C6 conserva S8-6719: prefijo de estados igual pero Persistent de Floor distinto en paso 1, terminación en 225 frente a 155; Fail sin tocar Reset. C7 por variante: baseline=0, speculative=0, offset05=0, solver12=0, no-ed=3. Ninguna variante cumple todos los criterios de gate.

C16 baseline: S8-9948 robustez, paso 246, RightWall, 11,3993931 mm. **Impacto lateral, no reposo:** vx=−1,69069779 m/s; en 246 solo hay Floor y en 247 aparece RightWall no persistente y vx=+0,9298841. Centro todavía positivo en 246, por eso no es cruce C1. Diagnóstico sin criterio nuevo. C17 nativo prioritario: editor no certifica el umbral de 1 ms del iPhone.

[Informe P1 y tabla de variantes](../reviews/2026-10-22-first-ball-physics-spike-P1.md) · [Diagnóstico completo previo](evidence/2026-10-23-i4b-editor-contact-diagnostic-runs.json) · [Cinco campañas actuales](evidence/2026-10-23-i4b-editor-canonical-runs.json) · [RED→GREEN y hashes](evidence/2026-10-23-i4b-contact-order-tests.json).

Sin exportación, compilación nativa, instalación ni interacción con el iPhone en esta revisión. Solo tras la revisión de Claude: una build dev y campaña física bajo las condiciones del PO. Comprobar siempre firma; renovar antes de instalar el 2026-10-27 o después. No cambiar herramientas, elegir backend ni avanzar a I4c/P2/I5+.

## Historial de la entrega CCD 16ad653 (superado por la entrega actual)

Estado: **corrección CCD y cinco campañas nuevas del editor entregadas; detenido para revisión de Claude antes del iPhone**. [PR #4 borrador](https://github.com/rafaallendes-boop/squash-clash/pull/4), sin fusionar. Rama firstball/i4b-physics-run, base main 6fd04e0, código final de esta revisión **16ad653**. I4b no cerrado; T4.3 pendiente de clasificación final y T4.4 de campaña completa/C17 nativo. I4c, P2/T4.5 e I5+ sin autorización.

## Cambios de la revisión

- **CCD:** reproducción mínima con ContinuousDynamic en escena local y por defecto; ambos fallan sin el adaptador. A→B→A identifica el umbral global de movimiento heredado, no una incapacidad de PhysicsScene.Simulate. Corrección común a las cinco variantes: m_FastMotionThreshold=0.01 m en el asset y ProjectSetup. Sin parche de trayectoria, preset, materiales, geometría ni variantes. Diagnóstico 8a1304d; corrección separada 492685b.
- **C7:** decisión PO 5b. Checkpoint ausente por fin superior/horizonte limpio: NotApplicable fuera de población. Cruce/no finitos/pasos discontinuos/contactos inválidos u otra terminación no inferida: NotEvaluable. Measured/NotApplicable/NotEvaluable separados y 20.032 oportunidades contabilizadas por variante. Igualdad y carácter de gate conservados. 3fe6fd7 incluye la primera diferencia detallada de C5–C7.
- **Foco:** decisión PO 5a. Robustez espera sin avanzar al perder foco, registra UTC/duración/fase/progreso/contexto y recupera antes de 60 s; al alcanzar el plazo cancela, en Update o al primer retorno si no hubo frames. Segundo plano y foco durante C17 cancelan. Interrupciones contadas; un intento solo es válido si termina completo. Resume automáticamente la vista del experimento; Runtime normal y pendiente I5 de pausa manual conservados. 9764a88.
- **Térmico:** decisión PO 5c. Estado al empezar C17 antes de crear escena/calentamiento, informativo; sin nuevo umbral. Se mantienen 20 calentamientos y 200 medidas, Reset único incluido, escenas fuera del cronómetro.
- **Reset:** experimento de 25 controles con S6 (8 contactos), S10 (653), instancias nuevas y misma historia. Todos iguales en los casos seleccionados. No prueba ausencia de caché interna ni todos los checkpoints. Reset permanece intacto; no hay RED que justifique cambiarlo. 16ad653.

[Diagnóstico CCD con fuentes y trazas S1 pasos 30–36](../reviews/2026-10-22-first-ball-ccd-diagnostic.md). [Aprobaciones del PO](../reviews/2026-10-21-first-ball-po-approvals.md). Spec/plan originales y PhysicsMeasures/PhysicsScenarios/DefaultPreset/PhysXBallSimulation sin modificaciones en esta revisión.

## Pruebas y evidencia

**EditMode 180/180 y PlayMode 54/54**, cero fallos/omitidos. RED CCD tres fallos reales; C7 siete; diagnóstico de divergencia tres; foco/térmico siete en doce pruebas de sección. API NUnit incompatible en el primer intento fue rechazada por XML viejo y no cuenta como RED; corrección de expectativa de fixture C7 documentada. Los cambios de código quedaron en commits separados; no se repiten suites por documentación posterior.

[Manifiesto de regresión con XML/hash](evidence/2026-10-22-i4b-review-fixes-tests.json), [CCD](evidence/2026-10-22-i4b-ccd-diagnostic.json), [C7](evidence/2026-10-22-i4b-c7-policy.json), [foco/térmico](evidence/2026-10-22-i4b-focus-policy.json), [Reset](evidence/2026-10-22-i4b-reset-history.json). El experimento Reset 1/1 comprueba ejecución/limpieza; igualdad física se entrega como dato.

Cinco campañas frescas de editor ejecutadas sobre 16ad653: **1/1 por variante**, 10.053 lanzamientos de robustez y 200 muestras C17 cada una, con liberación de recursos/escenas y restauración de gravedad. Enhanced Determinism vuelve al valor original al salir de cada script. Dos editores parcialmente concurrentes en worktrees aislados; rendimiento informativo. Los XML y logs nuevos se preservan localmente en Logs/ccd-editor-16ad653; el worktree temporal fue retirado después de preservar esos XML/logs de ejecución y los informes. El principal se conserva para Claude.

[Informe físico actual, 18 criterios y cinco variantes](../reviews/2026-10-22-first-ball-physics-spike-P1.md), [manifiesto de campañas](evidence/2026-10-22-i4b-editor-ccd-runs.json). Históricos sin sobrescribir: [informe previo íntegro](../reviews/2026-10-22-first-ball-physics-spike-P1-pre-ccd.md), [cinco ejecuciones anteriores](evidence/2026-10-22-i4b-editor-runs.json). Los nuevos informes no usan medianas C13 derivadas de una ejecución anterior.

Baseline actual: C1=0 cruces, C5=0/117 y C6=0/10.017 divergencias; C3 cinco fallos, C7 tres/18.753 medidos (1.279 no aplicables, cero no evaluables), C10 nueve fallos, máximo 13,6164427 mm; C11 tres/6. **Ninguna variante cumple todo.** Primer desacuerdo C7: estados idénticos y contactos LeftWall/Floor intercambiados en el paso 127. Una traza detallada por criterio, no clasificación de todos los fallos; no se ordenan contactos para aprobar. C10 residual coincide estrechamente con un modelo semimplícito/TOI lineal; inferencia documentada, sin compensación de lanzamientos ni cambio de integrador. C13 aproxima cero y C16 conserva penetración de 11,3993931 mm en baseline. Pendiente de Claude, no límite final del motor atribuido por cuenta propia.

## Estado del iPhone y detención

**No hubo builds, instalaciones ni campañas nuevas del iPhone en esta revisión.** Los cuatro intentos largos/cancelados y el control corto d14df6f pertenecen a configuración anterior y siguen como históricos. La firma válida y el arranque previo no certifican los cambios nuevos. C17 nativo completo sigue pendiente; no se extrapola del editor. La causa del antiguo foco perdido a los 10 min50 s permanece sin confirmar; la nueva política es una decisión aprobada, no una causa retrospectiva demostrada.

Firma: comprobar vigencia antes de toda instalación y renovar si ocurre el **2026-10-27 o después**. Perfil actual vence 2026-10-28T14:59:31Z. Herramientas sin cambios. Solo después de revisión de Claude: teléfono físico, No molestar, batería ≥80 %, bajo consumo apagado, sin funda y térmico nominal al inicio. Sin capturas/devicectl/u otras herramientas durante la ejecución; recuperar informe al terminar.

Se entrega diagnóstico, correcciones y editor y **se detiene antes de volver al iPhone**, conforme al PO. PR #4 permanece borrador sin fusionar. No task-done de T4.3/T4.4, gate I4c, selección de backend, P2 o I5+. La revisión interna anterior cubrió código histórico; no se presenta como una nueva revisión de estos cambios de Claude/PO.

## Rulings I made

Las resoluciones históricas se conservan debajo; las sustituciones explícitas del PO están marcadas. No son decisiones nuevas de gameplay.

Resoluciones del ejecutor en orden, con coste si resultan incorrectas. No son nuevas decisiones de gameplay del PO.

1. **Histórico, sustituido por decisión PO 5b de la revisión CCD:** C7 sin checkpoint observable conserva el caso como no evaluable, incluso por fin/salida superior legítima. Coste: no aprobación conservadora pendiente de interpretación de Claude/PO; no se excluyen casos silenciosamente.
2. Reset del benchmark es el único de TrajectoryRunner; no se añade un segundo. Coste: una definición distinta exigirá volver a medir, sin cambiar umbral.
3. Dos fixtures previos de pausa comparan DroppedSteps con el contador antes de pausar, en vez de suponer cero tras cargar escena. Reloj inalterado. Coste: esos tests no evalúan drops del arranque; sí ausencia de drops nuevos durante pausa/reanudación.
4. Tiempos concurrentes del editor solo informativos, sin extrapolar a C17 nativo. Coste: pueden ser pesimistas; decide la medición del teléfono.
5. Briefs Tarea 4.3/4.4 extraídos íntegros manualmente porque task-start reconoce encabezados ingleses. Coste: trazabilidad administrativa contrastada con plan/commits/XML.
6. Única revisión fresca adelantada al final de código GREEN mientras acababan datos y se esperaba teléfono. No implica tareas terminadas. Coste: una corrección de medida exige invalidar/corregir los datos afectados, como C13.
7. Clasificación física adaptador/prueba/motor reservada a Claude/PO. Coste: puede exigir nueva corrección y repetir campañas; informe aún parcial.
8. Gate, promoción/congelación de variantes y P2 reservados al PO/I4c. Coste: espera deliberada antes de continuar esa física.
9. Escenarios, umbrales, huella y definición de energía aprobados conservados. Coste: P1 puede incumplir; no se reinterpreta para aprobarlo.
10. Igualdad de contactos conserva superficie/paso según T4.2, junto con estados bit a bit. Coste: C5–C8 no certifican igualdad de punto/normal/tiempo; C10 y C12–C15 diagnostican aparte.
11. Pérdida de intención de pausa manual diferida a I5 conforme al PO. Coste: persiste ese comportamiento conocido.
12. Tamaño/ayudas visuales de pelota diferidos a T6.2 conforme al PO. Coste: persiste legibilidad insuficiente conocida.
13. No reabrir presets corruptos/almacenamiento de I5 no modificados en I4b. Coste: no hay cobertura nueva de ese sistema.
14. No ampliar host a Step arbitrario fuera de int: controles autorizados usan escenarios Step0. Coste: un futuro llamador deberá validar o ampliar antes de usar ese rango.
15. **Histórico; la campaña actual repite las cinco variantes completas:** reutilizar baseline/speculative anteriores con procedencia explícita: baseline deriva solo mediana C13 desde dos float32 recuperables; resto de filas auditadas iguales. Coste: no equivale a nueva corrida del binario final; una discrepancia de auditoría obligaría a repetirla.
16. **Histórico; nativo ahora pausado para Claude:** preparar baseline como referencia nativa provisional porque ninguna alternativa domina los criterios. Coste: una revisión puede pedir una segunda variante (máximo dos); no congela ni elige backend.
17. Conservar ledger y no marcar tareas completas mientras faltan clasificación y teléfono, aunque la revisión de código terminó. Coste: limpieza/cierre diferidos; no hay falsa aceptación.

18. **Histórico d14df6f; la política de foco se sustituye por PO 5a:** añadir diagnóstico de ciclo de vida dentro de T4.4 tras la nueva interrupción física, sin otra revisión interna ni cambiar cancelación/mediciones. Coste: build y comprobación nativa adicionales; los valores son instantáneos y no prueban causa, como mostró el control de Inicio con UIKit aún active. Claude deberá revisar también este cambio posterior.

19. Corregir la configuración común de P1 en asset/ProjectSetup, pues la reproducción sin adaptador también fallaba. Coste: más trabajo CCD y nueva respuesta física; repetición completa de editor y después iPhone, sin elegir backend.
20. **Histórico, sustituido por PO 1b el 2026-10-23 solo para el orden de salida del adaptador; Reset sigue intacto:** no modificar Reset ni el orden de contactos para hacer pasar C7. Coste: conserva incumplimientos hasta clasificación de Claude; controles seleccionados no demuestran ausencia de caché.
21. **Histórico, sustituido por el diagnóstico completo del 2026-10-23:** diagnóstico detallado acotado al primer desacuerdo por criterio, aparte de hasta doce notas generales; contadores completos. Coste: no se atribuye automáticamente el mismo motivo a los demás fallos.
22. **Histórico, clasificación final de Claude registrada el 2026-10-23:** tratar el ajuste al modelo semimplícito y el resto temporal próximo a cero como inferencias con fuentes/limitaciones, no inspección del binario de Unity. Coste: la clasificación final puede exigir otra prueba; no se altera gravedad, integrador, escenarios o umbral.

**Deferred minors:** ninguno. Se conservan explícitamente los pendientes I5/I6 ya aprobados, sin adelantar su implementación.
