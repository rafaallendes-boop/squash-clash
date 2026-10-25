# First Shot S1 — revalidación física del 2026-10-25

**Revalidación detenida en la identificación de la mira (RV-S1-01), sin bloques válidos completados.** El PO no distingue sus colores/cambio de estado tras la explicación. Se conserva la evidencia incidental, se restaura P0-calib-1 y se entrega para Claude/PO. No se declara aceptación de S1 ni conclusión sobre 20 frente a 30 ms.

Base **c5a7d1e83e9f339b043c38d5436e660142973cd2**, rama `firstshot/s1-revalidation`, solo documentación. [Autorización del PO](2026-10-25-first-shot-po-approvals.md#revalidación-física-s1-autorizada-por-el-po--2026-10-25). [Evidencia nueva](../validation/evidence/2026-10-25-s1-revalidation.json). Sin cambios de código, tests, defaults, spec, plan ni criterios; S2–S4/T8 no autorizados.

## Preparación técnica

- Regresión completa fresca sobre la base: **343/343 EditMode y 164/164 PlayMode**, sin fallos ni errores CS. XML/hashes en evidencia.
- **Check-release dev/prod OK**, exports frescos desde c5a7d1e limpio: DevPanel solo dev y framework de pruebas fuera de los Players.
- Build dev limpia **c5a7d1e**, Unity 6000.3.27f1, **Release ARM64/no Development**, SQUASH_DEV por extraScriptingDefines. Firma estricta verificada; ejecutable y UnityFramework ARM64; BuildInfo y DefaultPreset embebidos cotejados. Los **617 archivos versionados** conservaban sus hashes al compilar.
- Perfil efectivo compatible, comprobado **2026-10-25T18:57:48Z**, vigente hasta **2026-10-28T14:59:31Z**; no requiere renovación hoy. Comprobar siempre vigencia y renovar antes de cualquier instalación desde el 27.
- P0-calib-1, Po-1 y last-active respaldados antes de instalar y **byte a byte iguales después**. El primer intento de abrir por devicectl fue rechazado por iPhone bloqueado; la apertura posterior muestra la nueva app. Incidencia del entorno, sin contarse como sesión física ni defecto de S1.
- Arranque nuevo observado **18:59:18.366698Z**: **P0-calib-1 / 9977d9ffd7e5 / cero pendientes**. P0-calib-1 conserva su archivo original de 41 claves; no se guarda ni promueve.
- Preparación desde el panel por duplicación: `launcher.repeatSeconds=0`, **Aplicar y relanzar**, cero pendientes; lanzamiento **19:00:36.578744Z / 00a501ad7066**, correspondiente a repetición=0 y cobertura=30 ms. DEV cerrado, presets idénticos al respaldo. Esta configuración se comprobó antes de entregar el iPhone; más tarde hay cambios de hash y otro arranque durante la familiarización, descritos abajo. No se da por mantenido el control de configuración en esos intentos.

Defaults global **00a501ad7066**, proyección de los **41 valores heredados 0d39e6016bd8** y **SimulationKey 166a96e319c7** intactos. Los cambios experimentales se hacen solo en memoria mediante panel y se restauran al finalizar.

## Explicación y protocolo

Se muestra al PO antes de comenzar:

> «En S1 la pelota no se golpea. Solo pruebas la mira, el círculo que aparece en el frontis. Arrastra despacio y la mira se mueve. Acelera el dedo y la mira se fija: pasa de círculo cian a rombo amarillo. Si queda en rojo, apuntaste al tin o fuera. El número P de la superposición es la potencia del gesto. Hay que fijarse en dónde queda la mira y en si P sube cuando haces el gesto más rápido.»

Identificación de la mira y su cambio de estado: **sin confirmar**. Respuestas literales del PO a la explicación y su aclaración:

> «no veo nada, solo veo omo va cambiando la mira»

> «no veo ningun color , la pelota eso si la veo un poco roja, pero nada mas»

Codex no liberó el inicio de los bloques antes de esta identificación. Tras la segunda respuesta se detuvo la sesión por la incidencia de visibilidad, conforme a la instrucción del PO. No se interpreta lo dicho como aceptación/rechazo de saltos, ni se identifica técnicamente qué objeto vio rojo.

Orden autorizado: **2 minutos con izquierda dominante**, regla física 50 mm, 10 suaves/10 fuertes con cobertura 30 ms, 10 suaves/10 fuertes con 20 ms, volver a 30. **Desviación expresa:** Tarea 1.4 pedía derecha; el PO autoriza izquierda como principal y derecha opcional. No se presume bloque derecho. El orden fijo **30→20** puede introducir aprendizaje o fatiga. Registrar todos los intentos de cada bloque, incluidos toques/cancelaciones y salidas conservadoras; no elegir intentos para mejorar resultados ni asumir frecuencia de sensor.

## RV-S1-01 — incidencia de visibilidad que bloquea la validación

**Alta para esta validación:** el PO no puede identificar visualmente el círculo cian y su paso a rombo amarillo. La evidencia es su devolución literal, después de dos explicaciones. **La causa técnica no está confirmada**; no se afirma que el shader falle ni que la histéresis corregida sea responsable.

El código de [AimReticleView.SetAim](../../Assets/SquashClash/Runtime/Shot/AimReticleView.cs) asigna cian/amarillo/rojo y 33/5 vértices para círculo/rombo. [ShotOverlay](../../Assets/SquashClash/DevPanel/ShotOverlay.cs) mantiene una superposición de fondo con alfa 0,86, textos y botones, incluso con DEV cerrado. Las [pruebas de la vista](../../Assets/SquashClash/Tests/PlayMode/AimReticleViewTests.cs) verifican propiedades/posición y clasificación de color, pero no que el PO distinga la figura en la pantalla real. Es evidencia de lo que hace el código, no de una causa raíz demostrada.

**Propuesta para una entrega aparte, pendiente de autorización:** diagnosticar primero la visibilidad de la mira en el encuadre y su posible oclusión por la superposición, con capturas que localicen la figura real en estados Aiming/Frozen e inválido. Si se confirma un defecto, corregirlo en una PR pequeña con RED por aserción, comprobación de la representación, regresión y revisión de Claude. No cambiar lookback/umbral/ganancia para intentar resolver a ciegas la visibilidad. No se implementa ni se construye otra versión en esta sesión. La condición de ajuste por saltos inaceptables **no se activa**: no hay ese juicio del PO ni un congelamiento registrado en estos cuatro gestos.

## Evidencia recuperada y bloques no completados

Se conservan **todos los registros S1 recuperados de c5a7d1e: 4 gestos / 134 muestras crudas y 2 ventanas incompletas**. Tres gestos son `preview` y uno está etiquetado `soft` (ordinal 1). Los cuatro acaban como `Swing`, con mira en **Aiming**, potencia 0 y cobertura mínima registrada 30 ms; uno tiene salida conservadora `few-samples`, los otros tres `measured`. No hay cancelaciones de gesto en esos cuatro registros; una ventana termina `system-cancel`. La conservación incluye los intentos incidentales aunque el protocolo formal no se haya iniciado.

Las ventanas duraron **4,716708834 s** (`restarted`, 111 muestras) y **1,835075667 s** (`system-cancel`, cero muestras). Ninguna es la adquisición de 120 s: no se usa su frecuencia como validación del muestreo ni se asume frecuencia de hardware. DPI reportado=460, escala=0,055217389 mm/px y resolución=1206×2622 en los cuatro gestos; **sin verificación física nueva con regla**. El último gesto termina con desplazamiento máximo 14,801 mm, pero no está etiquetado como regla y no se reinterpreta como medición de 50 mm.

Tras la preparación aparecen lanzamientos con hashes **4efd67eed65b**, **d5f03990efdc** y **c1f9b294dbed**, y un nuevo arranque a **19:04:11.313556Z** con P0-calib-1. No se conoce por el hash qué claves temporales se editaron ni se atribuye intención al PO. El gesto `soft` tiene hash **c1f9b294dbed**, distinto de la configuración controlada de partida; no se presenta como serie comparable. Los tres gestos preview sí registran hash **00a501ad7066**. Archivos de presets/selección intactos en todas las capturas.

| Bloque autorizado | Evidencia actual | Evaluación |
|---|---|---|
| 2 min con izquierda dominante | Dos ventanas incidentales incompletas; identificación previa no confirmada. Mano de esos intentos no confirmada por separado. | NotEvaluated |
| Regla de 50 mm | Ningún registro etiquetado de regla. | NotEvaluated |
| 30 ms: 10 suaves | Un ordinal soft incidental; sin serie completa/configuración controlada. | NotEvaluated |
| 30 ms: 10 fuertes | Sin serie registrada. | NotEvaluated |
| 20 ms: 10 suaves y 10 fuertes | Sin aplicación verificada por Codex ni gestos de 20 ms registrados. | NotEvaluated |

La comparación autorizada **no pudo realizarse por la detención previa**. Para cada una de las cuatro series previstas, mínimo/mediana/máximo de potencia, salidas conservadoras por motivo y separación suave/fuerte quedan **NotEvaluated**, con valores `null` en el JSON. No se usan el único soft o sus ceros como sustituto de diez intentos. No se propone cambiar ningún default. El orden previsto 30→20 conserva su posible sesgo de aprendizaje/fatiga como condición de una sesión futura; no se afirma que se haya completado.

**Saltos de mira:** los cuatro valores brutos son 0, pero los cuatro estados son Aiming. No se registró Frozen y no son mediciones de salto al fijarse; evaluación **NotEvaluated**, sin atribuir mejora a la corrección de histéresis.

## Cinco preguntas del PO, sin respuestas inferidas

Las preguntas de control no se formularon tras detenerse en la precondición. Se conservan literalmente y por separado; las dos respuestas de familiarización anteriores no las sustituyen.

| Pregunta literal | Respuesta literal |
|---|---|
| ¿Apuntas donde quieres? | Sin respuesta; no preguntada tras la detención. |
| ¿Entiendes siempre dónde apuntas? | Sin respuesta; no preguntada tras la detención. |
| ¿El número responde a suave/fuerte? | Sin respuesta; no preguntada tras la detención. |
| ¿Los saltos de la mira al fijarse son aceptables? | Sin respuesta; no preguntada tras la detención. |
| ¿Notaste diferencia entre 30 y 20 ms? | Sin respuesta; experimento sin realizar. |

## Comparación histórica, restauración y entrega

La sesión de **311cb11** usó **histéresis defectuosa** y tuvo repetición=3 s. Sus **102 gestos / 1475 muestras** y el replay de potencia ya documentado permanecen separados. Frente a ellos, los **4 gestos / 134 muestras** actuales son familiarización incidental sin bloques equivalentes. No se comparan potencias, Hz o saltos como mejora/regresión, ni se recalculan observaciones antiguas como nuevas. [Informe histórico](2026-10-25-first-shot-s1-gesture.md).

**Restauración verificada:** reinicio de la app mediante `devicectl --terminate-existing --no-activate`, que recarga el preset persistido sin guardar cambios temporales. Nuevo arranque **2026-10-25T19:06:44.809232Z**, app **c5a7d1e dev**, **P0-calib-1 / 9977d9ffd7e5 / cero pendientes**, configuración original con repetición=3 s y cobertura=30 ms. Se cotejan de nuevo P0-calib-1, Po-1 y last-active **byte a byte iguales al respaldo**. No queda el experimento aplicado.

| Archivo | SHA-256, idéntico antes/después |
|---|---|
| P0-calib-1.json | `2f1295f201e238d9494bee285dd3f46e5bbb0f98b09ccde90fbd268f0874e4fd` |
| Po-1.json | `9008c33ab89f82e669bc227a81b37ed641f69e4657ee70df6c89ff0c0edb1767` |
| last-active.txt | `5885ce8a7add389b2c82cb43f6944d7e2ef7ebeb0bffd77b5b635361a8a46087` |

Informe/evidencia nuevos y registro de decisiones en PR documental lista para Claude, **sin fusionar ni aceptar S1**. La rama solo modifica AGENTS.md, aprobaciones, este informe y su evidencia JSON; los otros **615 archivos versionados de la base** conservan sus bytes (auditoría en el JSON). Los logs, XML, build, exports y respaldos originales quedan en `Logs/evidence/s1-revalidation` y `Builds` del worktree, ignorados. **Detenerse: el PO decide con el informe; S2–S4/T8 siguen sin autorizar.**
