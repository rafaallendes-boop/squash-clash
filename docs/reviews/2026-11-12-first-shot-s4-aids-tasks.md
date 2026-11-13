# First Shot T4.2 — ayudas y tareas provisionales

**ENTREGA DETENIDA: AutoQA ERROR / salida 3 / nivel 3. No apta para fusión.** Regresión completa GREEN; check-release pendiente por la parada. Sin reintentos ni correcciones de código posteriores al ERROR.

Base main/origin/main `ef9be56b382936fe948217c0efe4ee02cce28b4c`. Encargo literal guardado primero en `1f9c8fb`, en el registro de aprobaciones. D1 opción B, D2 bandas de observación y D4 correcciones autorizadas; diseño de origen: [Step 0 T4.2](2026-11-11-first-shot-s4-timing.md#t42--step-0-documental-implementación-no-autorizada). Sin aceptación de First Shot, sin sesión física/4.5 ni capa causal.

## Hashes y anclajes autorizados por D1

Cálculo Unity desde DefaultPreset.json real, prueba TimingAidConfigTests; original XML/log local en Logs/evidence/s4-aids-tasks/config-green. No hashes predichos.

| Configuración | Antes (107 claves) | Después (108 claves) |
|---|---|---|
| Defaults / SL1 | c0d8ec7050e0 | c0e9f00176d5 |
| SL5 efectivo de la preparación existente | 55a85f500cf1 | **50efa6d33c0c observado en Unity; anclaje incorrecto 1877d083c345 pendiente de corregir** |
| P0 efectivo (overlay legacy/repeat=3) | 6ecfdfa78cbb | **42d9efbf50fc** |
| Proyección defaults 41 heredados | 0d39e6016bd8 | 0d39e6016bd8 |
| SimulationKey | 166a96e319c7 | 166a96e319c7 |

Única clave nueva `timing.aidLevel=0`. Ningún valor existente cambia. P0 no se promueve ni reescribe. Para futuros prechecks, usar el nuevo P0 efectivo **42d9efbf50fc**, no el histórico.

Commit separado de anclajes `551858d`: Scenario.cs (SL1/SL5), autoqa-check v7 en .claude/agents/autoqa-operator.md y referencias de hash de regresión/experimentos. **La alineación SL5 quedó incorrecta**: se usó el hash del overlay parcial `practice.launch=4` (1877d083c345), omitiendo `human.z=(double)1.6f` aplicado por la preparación existente. El ERROR posterior dejó observado el hash efectivo correcto 50efa6d33c0c; no se modifica el anclaje después de la parada. El texto `SL5=` del diagnóstico TimingAidConfigTests representa ese overlay parcial y no acredita la configuración efectiva del escenario. El skill autoqa invoca ese mismo v7 y no contiene hashes duplicados. No cambian escenarios, expectativas, márgenes ni tolerancias. Los tres anclajes de hash de tests de rally se actualizan únicamente por la nueva clave, sin cambiar fixtures ni comportamiento de rally.

## Bordes y decisiones de representación

D4: contornos exteriores para bordes tempranos A/V, interiores para tardíos. Tras T* el anillo continúa hacia dentro; alpha constante, sin extinguirse en +V inclusivo. T3 conserva la supresión de ambos contornos V. El contrato/ImpactCue/ventanas no se modifican.

D1: nivel capturado por recepción; parcial oculta anillo/triángulo/cuatro contornos desde T*−150 ms y no reaparece por replan. Se registra target/versión que activó el corte, planes posteriores y nivel. Ninguna suprime solo timing; no toca mira/potencia/feedback.

D2: intervalos cerrados por abajo/abiertos por arriba, último extremo incluido, sin epsilon ni clamp de zonas. X del bote usa los mismos cortes exactos 0,20 / 2,20 / 4,20 / 6,20. Esta política de bordes es elección simple de implementación según el encargo, **no aprobación adicional del PO**. Las zonas/bandas son observación y no umbrales de aceptación.

## TDD y validaciones

- D4: RED 15/17 (aserciones de +V y contorno tardío), GREEN 17/17.
- D1 vista: RED 17/19 (parcial/ninguna), GREEN 19/19.
- D1 catálogo: RED 0/1 (clave ausente), GREEN 1/1.

- Tareas/bandas: RED válido 4/22 → GREEN 22/22. La primera preparación produjo dos excepciones por campos nulos del esqueleto; se completó el esqueleto y se repitió RED por aserciones, sin cambiar criterios. Se conserva como RED válido la segunda corrida.
- Conexión de tareas: RED 0/2 → GREEN 2/2 (selección por recepción, captura fallida y contactos canónicos).
- Panel/solver no aplicado: RED 0/2 → focal ampliada 45/46 → GREEN 46/46 tras excluir la mira factible en una captura inválida; sin cambiar las aserciones. La ampliación incluye los cinco nombres del plan, D4 y snapshots por recepción.

La selección de tareas queda en memoria, desactivada por defecto y capturada al relanzar; no introduce más claves ni escribe presets. El HUD muestra las bandas y marca un rectángulo del frontis. TaskSection permite combinar columna/altura/potencia/columna y profundidad de bote para la siguiente recepción. Se emite pedido aun sin gesto y se conservan todas las capturas fallidas/reapoyos, el resultado técnico y las actualizaciones de contactos observados.

`Requested` conserva las cinco bandas pedidas. Para distancias descriptivas se usa el centro del rectángulo (`RequestedAim`) y de la banda de potencia (`RequestedPower`); es una referencia de implementación documentada, no un objetivo puntual adicional aprobado ni un criterio. `AimErrorCm`, `FeasibleErrorCm` y `FrontErrorCm` son distancias separadas a ese centro; los aciertos de banda se registran aparte. Ausencias: flags `Has*`, motivo y error **−1**, nunca 0. Saturación significa **P congelada = 1**, no estima cuánto sobrepasó el tirón el clamp ni infiere voluntariedad. La potencia pedida y la observada se conservan sin declarar intención del usuario.

El primer bote es el primero posterior al contacto aplicado, no un punto de predicción: centro reconstruido desde contacto+normal×radio; contactos persistentes e incoming excluidos. `PriorSurfaces`, `LineTouches`, `OutObserved` y `TinObserved` son campos independientes, basados en geometría/clasificación ya existente, sin cambiar reglas de punto. El solver resuelto y una aplicación fallida conservan objetivo factible; ningún bote incoming se convierte en bote del golpe. La traza se actualiza por hitos y cierra como completa o recepción reemplazada; no inventa impactos fuera de la observación. Las tareas no completan H2 ni la aceptación física.

Resultados finales y parada en la sección siguiente. La implementación no se declara verificada completamente.


### Alineación adicional de metadatos D1

Primera regresión completa EditMode, fuente limpia `9a221c8`: **690/691**, sin skipped ni CS. Única falla: `S3FeedbackConfigTests.OnlyDifference_FromPreS3Fixture_IsHapticsKey` esperaba solo la clave S3 frente al fixture previo; debe enumerar también la clave nueva T4.2 autorizada. Se amplía explícitamente a feedback.haptics/timing.aidLevel; valores previos idénticos siguen exigidos. Es actualización de catálogo de D1, sin cambiar un escenario, criterio o comportamiento de producto. Evidencia original preservada en `Logs/evidence/s4-aids-tasks/regression-catalog-anchor`.

El fixture sintético del verificador (`scripts/tests/test_autoqa_checker.py`) también tenía SL1/SL5 antiguos. Su control completo dio RED por aserciones (mismatches de hash); se actualizan solo esas dos constantes a c0e9f00176d5/1877d083c345. No se reescribe evidencia histórica ni se rebaja la clasificación del v7. Un v7 con anclajes nuevos no debe aprobar una corrida histórica de hashes anteriores.

La regresión confirmó **en Unity y desde los bytes del P0 archivado**: efectivo 42d9efbf50fc, proyección P0 8de3f2ecb217, bytes intactos. Archivo local ignorado: Logs/evidence/s4-aids-tasks/P0-calib-1.json, SHA-256 2f1295f201e238d9494bee285dd3f46e5bbb0f98b09ccde90fbd268f0874e4fd. No accedido ni reescrito en iPhone.

Control sintético actualizado del verificador: **1/1 método, cinco controles (pass/fail/canario/hash/environment) GREEN**, sin skipped. No equivale a AutoQA de juego; La corrida Unity posterior está registrada abajo y no pasó.


## Corridas completas y parada S4-42-AQ-01

Fuente de producto final `9a221c8`; fuente limpia de las verificaciones completas **`6c149ad80bbc0a54b44891e17da98bab3a21f2bf`** (solo alineación de metadatos respecto de producto). Sin otro Unity en paralelo. Horas del **12 de noviembre de 2026, America/Santiago**:

| Comprobación | Resultado | Hora / evidencia local ignorada |
|---|---|---|
| EditMode completo | **691/691**, 0 failed/skipped/CS | 22:43:34–22:45:37; Logs/evidence/s4-aids-tasks/regression-editmode |
| PlayMode completo, única corrida | **450/450**, 0 failed/skipped/CS | 22:46:23–22:48:29; Logs/evidence/s4-aids-tasks/regression-playmode |
| AutoQA mvp, única corrida | **ERROR / salida 3 / nivel 3**, 83/85 tests; 2 fallos | 22:48:43–22:48:55; Logs/autoqa/20261113T014843Z-6c149ad-23689 |
| autoqa-check v7, lectura determinista | **nivel 3**, gatePass=false, salida esperada 3, sin incoherencias; no reintento elegible | Logs/evidence/s4-aids-tasks/autoqa-check-v7.md |
| check-release dev/prod | **NO EJECUTADO**, pendiente por parada | No exports/builds nativos nuevos |

AutoQA se ejecutó con `gitDirty=false`. Dos fallos: `AutoQAPocTests.Early10_ReachesRealPresentation` y `Late10_ReachesRealPresentation`. Mensaje literal: **Unknown ShotRecord field not predeclared**. `PocObservation.ExactRecord` conserva su lista estricta y rechaza seis campos añadidos por esta implementación: `AidCutoff`, `AidCutoffPlanVersion`, `AidCutoffTarget`, `AidLevel`, `CuePhase`, `Solved`. Este es un problema de compatibilidad introducido en T4.2, no se atribuye al entorno ni a la física. No se amplía el lector, no se excluyen campos ni se cambia el oráculo después del ERROR.

Además, los intentos SL5 observados por Unity registraron **50efa6d33c0c**, mientras el anclaje introducido es 1877d083c345. Error de cálculo de Codex: el hash parcial no incluía `human.z=(double)1.6f`, que ya aplica la receta/preparación (`AutoQAPocTests` lo contempla). No se cambia esa preparación ni se fuerza un hash para obtener PASS; la corrección del anclaje queda pendiente por la instrucción de parada. Defaults c0e9f00176d5 y P0 efectivo 42d9efbf50fc sí fueron calculados y cotejados correctamente en Unity.

Resumen del envoltorio: gate ERROR, counts 0/0/0/0; no usarlo como evidencia de ausencia de intentos. El resumen separado del arnés conserva **6 INCONCLUSIVE y 12 NOT_EVALUATED**, 0 PASS/FAIL; canario NOT_EVALUATED y determinismo sin evaluar. Los seis INCONCLUSIVE son EARLY10/LATE10 SL5, repeticiones 1–3; los cuatro escenarios SL1 × tres repeticiones no se evaluaron. No son resultados aprobados ni mediciones de una regresión de timing.

El v7 confirmó `operatorLevel=3`, `gatePass=false`, `expectedExitCode=3`, `incoherences=[]`, seis mismatches SL5 y `retry.eligible=false` por integridad. Se ejecutó el script y el verificador directamente en este chat; no hubo operador externo, subagente ni mensaje a Claude.

SHA-256 de `summary.json`: **23929b7c772c64664bcd1d5dfe00398bdcb99e539a295743e4a0b86311bbb9c0**. SHA-256 de `summary.harness.json`: **9de4834c613729ca93375bef6b6d25ad52968ec5bb79bd8f53c0b82e26234c0c**. Todos los originales conservados; no se reejecutó AutoQA ni se regeneró evidencia para buscar PASS.

La parada sigue el literal del encargo **«Ante otro defecto o sorpresa: registrar y detenerse»** y el cambio de resultado de AutoQA. Continuación pendiente de decisión del PO: resolver de forma explícita la compatibilidad del registro con su lector determinista (o mantener esa telemetría separada) y el anclaje SL5 efectivo, con revisión/TDD correspondiente, antes de repetir AutoQA y ejecutar check-release. Esta PR documenta una implementación con verificación incompleta, **no autoriza fusión ni acepta First Shot**.

## Integridad y límites de la entrega

- 107→108 claves; diferencia única `timing.aidLevel=0`; cero cambios de valores existentes. Los 41 heredados/proyección 0d39e6016bd8 y SimulationKey 166a96e319c7 conservados. P0 local archivado SHA-256 2f1295f201e238d9494bee285dd3f46e5bbb0f98b09ccde90fbd268f0874e4fd sin cambios; su proyección 8de3f2ecb217 y efectivo 42d9efbf50fc cotejados en Unity. No se reescribió ni promovió P0.
- Auditoría local `Logs/evidence/s4-aids-tasks/protected-audit.json`: **120 archivos protegidos idénticos** a ef9be56 (plan/spec bajo docs/superpowers, física, contrato/ImpactCue, implementaciones M2-4, fuentes rally/visual, arnés salvo Scenario.cs). La prueba M24Projection cambia únicamente hash y número de claves, verificados aparte. Ningún escenario, expectativa, margen ni tolerancia de AutoQA cambiado; Scenario.cs solo anclajes, con error SL5 expresamente pendiente.
- GraphicsSettings.asset y QualitySettings.asset restaurados tras PlayMode (diff original conservado). AutoQA restauró ambos automáticamente: statusUnchanged=true, errors=[], árbol limpio al terminar. Sin otros ProjectSettings modificados; device.env sigue idéntico a su copia de origen e ignorado.
- Sin iPhone, Duplicación, build para instalar, Xcode, instalación, firma ni sesión física; sin capa causal, cambios de ventanas/umbrales/pullFullMm/M2-4/#32, gameplay ni art-pass. El cierre técnico y físico de First Shot no se infiere.
- Después de la parada solo se leen evidencias y se escriben documentación/PR. No check-release, tests, reintentos ni cambios de producto adicionales. Revisión independiente de Claude coordinada por el PO; sin enviarle mensajes ni fusionar.
