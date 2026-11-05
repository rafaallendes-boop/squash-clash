# First Shot S2 — tanda 2 de 2.7

## Estado

Retomada por Codex desde `e160f98`, rama `firstshot/s2-m24`, PR #30 sin fusionar. Decisión de repreguntar registrada en `c336c51`; procedimiento confirmado y precisado literalmente por el PO en [aprobaciones](2026-10-25-first-shot-po-approvals.md). Rigen la [predeclaración](../validation/2026-10-31-first-shot-s2-session27-predeclaration.md) y el estado vigente de AGENTS.md. App prevista: **0.1.0+2ef4807 dev ya instalada**, sin build/instalación nuevos.

**14 y15 completos, conservados; reanudación del4nov preparada para16, aún sin iniciar.** Tanda 2 prevista: 14–22, 57 recepciones SL1. Perfil y P0 de esta tanda comprobados abajo; la restauración final de tanda 2 sigue pendiente. No se atribuyen aquí las comprobaciones de tanda 1. Al cierre se requiere exportación/restauración en proceso nuevo/dd122831f592/pending 0 y cotejo de los tres archivos; entrega para Claude/PO y parada. S3–S4/T8 no autorizados.

## Repreguntas retrospectivas sobre el salto de 09–13

**De memoria, un día después**, conforme a la precisión del PO; no son observaciones ni mediciones nuevas. Se hacen las cinco antes del precheck de 14, sin repetir bloques y sin mostrar resultados, contactos, timing ni trayectoria. Solo contexto de número de bloque, SL, ventana y cuerpo nominal; las coordenadas nominales son las de la tabla de recetas aprobada y no una nueva medición del cuerpo. Una pregunta y una respuesta cada vez. Las preguntas 1–4 de 09–13 siguen declaradas no literales y no se repreguntan.

Se conservan intactas las respuestas originales y sus limitaciones en el [informe de tanda 1](2026-10-31-first-shot-s2-tanda1.md). Cada respuesta nueva se identifica como repregunta, sin sustituir o reinterpretar la anterior. «No recuerdo» queda literal y pendiente; resolverlo exigiría otro bloque autorizado aparte, pues tanda 2 es solo SL1. Ninguna respuesta infiere por sí sola Pass, aceptación de S2 ni autorización de fusión.

Las horas son **UTC del Mac secundarias**, con equivalente America/Santiago: hora de formulación o recepción/registro en este chat, no un tiempo de evento del dispositivo ni una sincronización de relojes. Registro inicial: **2026-11-02T00:33:42.012499+00:00 / 2026-11-01T21:33:42.012499-03:00**.

| Repregunta | Condición | Cuerpo nominal (x,z) m | Pregunta literal | Hora de formulación/registro Mac | Respuesta nueva literal / hora de recepción-registro |
|---|---|---|---|---|---|
| R09 | Bloque 09, SL4, V100 | (5,1; 8,3) | «¿el salto es aceptable?» | 2026-11-02T00:33:50.736854+00:00 / 2026-11-01T21:33:50.736854-03:00 (formulación previa al envío) | «si» / 2026-11-02T00:41:25.214304+00:00 / 2026-11-01T21:41:25.214304-03:00 (recepción-registro; hora exacta del envío del PO no disponible) |
| R10 | Bloque 10, SL5, V100 | (3,2; 1,6) | «¿el salto es aceptable?» | 2026-11-02T00:41:25.214304+00:00 / 2026-11-01T21:41:25.214304-03:00 (formulación previa al envío) | «si» / 2026-11-02T00:41:35.433067+00:00 / 2026-11-01T21:41:35.433067-03:00 (recepción-registro; hora exacta del envío del PO no disponible) |
| R11 | Bloque 11, SL5, T3 puro | (3,2; 1,6) | «¿el salto es aceptable?» | 2026-11-02T00:41:35.433067+00:00 / 2026-11-01T21:41:35.433067-03:00 (formulación previa al envío) | «si» / 2026-11-02T00:41:46.812081+00:00 / 2026-11-01T21:41:46.812081-03:00 (recepción-registro; hora exacta del envío del PO no disponible) |
| R12 | Bloque 12, SL6, T3 puro | (3,2; 1,6) | «¿el salto es aceptable?» | 2026-11-02T00:41:46.812081+00:00 / 2026-11-01T21:41:46.812081-03:00 (formulación previa al envío) | «si» / 2026-11-02T00:41:57.469307+00:00 / 2026-11-01T21:41:57.469307-03:00 (recepción-registro; hora exacta del envío del PO no disponible) |
| R13 | Bloque 13, SL6, V100 | (3,2; 1,6) | «¿el salto es aceptable?» | 2026-11-02T00:41:57.469307+00:00 / 2026-11-01T21:41:57.469307-03:00 (formulación previa al envío) | «si» / 2026-11-02T00:45:24.099170+00:00 / 2026-11-01T21:45:24.099170-03:00 (recepción-registro; hora exacta del envío del PO no disponible) |

Las cinco repreguntas se recogieron una a una **antes del precheck de 14**: R09/R10/R11/R12/R13 = «si» en cada una. Juicios retrospectivos explícitos sobre aceptabilidad del salto, con las limitaciones de recuerdo declaradas; no son una observación nueva ni eliminan el sesgo/desviaciones de la tanda 1. No se repreguntaron las preguntas 1–4. PR #30 sigue sin fusionar; no se infiere aceptación global de S2 ni autorización de merge.

## Precheck de tanda 2 y preparación de 14 POT-S

Evidencia local privada ignorada: `Logs/evidence/s2-session27-tanda2-20261101/`. Sin nuevas suites/build/instalación. Diff de Assets/ProjectSettings/Packages/scripts frente a `9131b1c` vacío.

- Perfil de la app dev firmada preservada: creación **2026-10-31T17:58:46Z**, vencimiento **2026-11-07T17:58:46Z**, comprobado **2026-11-02T00:45:35.930652Z**; vigente para la tanda prevista y restauración. Firma estricta verificada. UI y log nativos confirman **0.1.0+2ef4807 dev**; no se sustituye la identidad por el CFBundleVersion 0 del plist. Batería/térmico actuales no disponibles en esta captura.
- Export previo: **31 metrics / 22.023.502 bytes**, **11 spike / 136.868 bytes**. Primer copy falló CoreDevice7000/remoteservices11001 (canal cerrado); preservado como fallido. Consulta details y reintento completo aceptados. `before-session-retry/Documents` es el respaldo válido; el intento fallido no se considera export.
- `cmp` de **P0-calib-1.json, Po-1.json y last-active.txt** contra el respaldo inicial de tanda 1: los tres idénticos. P0 3614 bytes/SHA-256 `2f1295f201e238d9494bee285dd3f46e5bbb0f98b09ccde90fbd268f0874e4fd`; Po-1 2843/`9008c33ab89f82e669bc227a81b37ed641f69e4657ee70df6c89ff0c0edb1767`; last-active 10/`5885ce8a7add389b2c82cb43f6944d7e2ef7ebeb0bffd77b5b635361a8a46087`.
- Arranque nuevo por CLI aceptado: proceso con prefijo **metrics-20261102T004628684-…**, línea 1 **2026-11-02T00:46:28.7025900Z**, build **0.1.0+2ef4807 dev**, **P0-calib-1/dd122831f592/pending 0**. Tres archivos cotejados de nuevo después del arranque, iguales. No hubo bloqueo/segundo plano observado durante esta preparación.
- Preparación B→SL1→SL1: DEV abierto por Codex (no frontera de bloque), pestaña Presets y **Cargar P0** sin guardar, aceptado en líneas **123/125**, **00:47:22.068403Z/00:47:22.098045Z**; tres archivos recuperados y `cmp` iguales **antes de aplicar**. Aplicar y relanzar ancla SL1 aceptado en líneas **144/148**, **00:47:38.391300Z/00:47:38.397032Z**. La SL solicitada también es SL1; no cambio de receta intermedio. Última pestaña Parámetros, DEV abierto.
- Precheck **aceptado** para **14 POT-S, 10 recepciones, SL1/V100/cámara fija, pulgar izquierdo dominante, potencia suave voluntaria**. H externo **dd122831f592** = hash efectivo/recepción observado **dd122831f592**, pending **0**; correspondencia del conjunto completo de 106 claves con manifiesto B por igualdad del hash aprobado, sin nueva lectura individual de cada slider. Cuerpo B human.x=3.2/human.z=5.5; control2/P2, tempo0.65/slowMotion1, practice.launch0/enabled1, A40/V100/comp0/T3 no suave y cámara0 según manifiesto que se acaba de recargar/aplicar. UI P0/P2/Jugando/0 pendientes/Pausa disponible; registro de apply-relaunch **paused=false/focused=true/devOpen=true**. Comprensión previa de mira/anillo conservada de tanda 1; no se declara nueva respuesta de comprensión en esta captura.
- Export `pre-14/Documents`: **205 líneas**, todas dd122831f592/pending0; **0 m24-gesture**, 32 launch/32 shot-plan, 30 shot, 31 frames, 70 m24-input-filtered y 10 dev-control. No hay cierre DEV de Codex: el bloque aún no tiene frontera. Último plan capturado línea **205**, **00:48:32.876889Z**, LaunchId32, H=dd122831f592, hasPlan/front/bounce=true. Todos los lanzamientos hasta que el PO cierre DEV son preparación, fuera de cuotas. Tres archivos nuevamente iguales al respaldo.

**Solicitud de cierre único de DEV para 14 (M_s, UTC Mac secundaria): 2026-11-02T00:49:33.517489+00:00 / 2026-11-01T21:49:33.517489-03:00.** Falta la confirmación M_r y el cierre único dev-toggle/result/completed/accepted/devOpen=false del PO; recepción 1 será primer LaunchId posterior. Este timestamp es formulación previa al envío, no evento del dispositivo. Sin relación calibrada Mac/dispositivo.

## Recepción del cierre de 14 y preguntas de cierre

PO: **«Listo, termine 14»**. Hora Mac secundaria de recepción-registro **2026-11-02T00:53:36.072337+00:00 / 2026-11-01T21:53:36.072337-03:00**; no se dispone de hora exacta de envío del PO. La frase no confirma por sí sola el cierre DEV, pero el export `after-14/Documents` conserva un cierre completado único del PO después de M_s; la frontera se determina por esa línea de la app. Este mensaje posterior sirve de M_r aproximado del intercambio, con la incertidumbre declarada en §3.2, no de reloj del evento. Export completo y tres archivos `cmp` idénticos al respaldo. Frontera/cuota/hash recuperados para revisión; resultados reservados hasta recoger las preguntas literales de cierre. No se prepara ni solicita 15 todavía.

1. **«Por número de recepción, ¿qué excepciones recuerdas: Apoyo sin querer, No fijó / falló u otra dificultad?»** Formulación previa al envío, Mac **2026-11-02T00:53:36.072337+00:00 / 2026-11-01T21:53:36.072337-03:00**. Respuesta pendiente. Las demás preguntas de cierre no se envían juntas con esta.

Respuesta literal a la pregunta 1 de 14: **«Todos fueron intencionados, a si que los que falle fui yo»**. Recepción-registro Mac **2026-11-02T00:54:32.413678+00:00 / 2026-11-01T21:54:32.413678-03:00** (hora exacta del envío del PO no disponible). Declaración global de intención; no identifica excepciones por número ni se convierte en diez OK individuales. Se conserva separada de los resultados técnicos.

2. **«¿Seguiste la tarea y la intensidad que se pidió en este bloque? Si no, ¿qué intentabas hacer?»** Formulación previa al envío, Mac **2026-11-02T00:54:32.413678+00:00 / 2026-11-01T21:54:32.413678-03:00**. Respuesta pendiente.

Respuesta literal a la pregunta 2 de 14: **«Si, lo seguí.»** Recepción-registro Mac **2026-11-02T00:55:27.212861+00:00 / 2026-11-01T21:55:27.212861-03:00** (hora exacta del envío del PO no disponible).

### Cambio de modalidad solicitado por el PO durante el cierre de 14

El PO pide literalmente: **«Para el resto de las preguntas dejamelas de una para ir respondiendo en un pequeño modal.»** Recepción-registro Mac **2026-11-02T00:55:27.212861+00:00 / 2026-11-01T21:55:27.212861-03:00**. Se aplica a las preguntas restantes desde este punto: agrupadas en un modal, cada literal con su respuesta propia, sin mostrar resultados antes de recogerlas. **Desviación de la entrega una a una autorizada por esta petición expresa del PO**, sin modificar predeclaración, criterios ni preguntas; las preguntas 1 y 2 de 14 ya se hicieron por separado y no se vuelven a preguntar. Se mantiene separada la recogida por intento donde §6.1 exige triada.

Para cerrar 14 se ofrecen las tres originales de 2.7, como feedback complementario de POT-S (el mínimo de §6.1 enumera esas tres para principales/CAN/RIT/CAM, no exige extenderlas a potencia). No se añade criterio de aceptación ni se mezcla con los principales. Modal preparado, Mac **2026-11-02T00:55:27.212861+00:00 / 2026-11-01T21:55:27.212861-03:00**:

- «¿la pelota va donde apunto?» — **«Si»**. Recepción-registro Mac **2026-11-02T00:57:31.866543+00:00 / 2026-11-01T21:57:31.866543-03:00**; hora exacta de envío del PO no disponible.
- «¿el timing es justo?» — **«Me costó esta vez el timing, pero se puede logar»**. Recepción-registro Mac **2026-11-02T00:57:31.866543+00:00 / 2026-11-01T21:57:31.866543-03:00**; hora exacta de envío del PO no disponible.
- «¿el salto es aceptable?» — **«si»**. Recepción-registro Mac **2026-11-02T00:57:31.866543+00:00 / 2026-11-01T21:57:31.866543-03:00**; hora exacta de envío del PO no disponible.

### Resultado de 14 POT-S (publicado después de recoger las respuestas)

Frontera única del PO: proceso **metrics-20261102T004628684-…**, línea **363**, **2026-11-02T00:51:14.001249Z**, timeReal **287,43715154223355**, dev-toggle/result/completed/accepted=true/devOpen=false, paused=false/focused=true. Primera recepción posterior: **LaunchId71**, línea **368** del archivo, **00:51:17.537050Z**; cuota **71–80**, diez sucesivas/completas. El vuelo ya iniciado al cerrar queda preparación. Último cierre de cuota: línea **435**, **00:52:07.430335Z**. No hay reapertura DEV en el tramo; no se seleccionaron recepciones por éxito.

H esperado externo **dd122831f592** = observado de lanzamiento/gesto en los diez; pending0 en todos, sin ajustes durante el bloque. Cierres originales y versiones posteriores preservados; aquí una versión final por recepción. El campo nativo `comparableToArmedBlock=false` es del protocolo S1 no armado, no un fallo de H: la comparabilidad de esta cuota S2 se comprueba externamente por la frontera y hashes.

| R | LaunchId | Línea de cierre final | Resultado / motivo | P (%) | Contacto aplicado |
|---|---:|---:|---|---:|---|
| 1 | 71 | 372 | Failed / outside-v | 31,587636 | No |
| 2 | 72 | 379 | Valid / applied | 17,364973 | Sí |
| 3 | 73 | 386 | Perfect / applied | 20,042159 | Sí |
| 4 | 74 | 393 | Failed / after-k-out | 20,376809 | No |
| 5 | 75 | 400 | Failed / after-k-out | 25,396574 | No |
| 6 | 76 | 407 | Failed / after-k-out | 21,548109 | No |
| 7 | 77 | 414 | Valid / applied | 24,727292 | Sí |
| 8 | 78 | 421 | Failed / outside-v | 9,835288 | No |
| 9 | 79 | 428 | Valid / applied | 17,364973 | Sí |
| 10 | 80 | 435 | Perfect / applied | 15,859045 | Sí |

**10 Swing/10 recepciones, 5 contactos (2 Perfect/3 Valid), 5 Failed** (2 outside-v/3 after-k-out). Todos conservados. Potencia: **8/10 suaves, 2/10 medios, 0 fuertes** con bandas vigentes suave [0;0,25), media [0,25;0,60), fuerte [0,60;1]; no son bandas nuevas ni H2. Entre contactos: **5/5 suaves**. P mín/p50/p95/máx **9,835288/20,042159/31,587636/31,587636 %**, n10; saturación0. Distancia descendente estimada por DPI, n10: mín/p50/p95/máx **5,245645/8,613913/12,423920/12,423920 mm**; no verificación física con regla.

Muestras **291**; O2 **0**. Duración Began→Ended, n10: mín/p50/p95/máx **1304,327167/1454,395875/1696,061042/1696,061042 ms**. Aim(Began)→C, **referencia verbal sin marcador**, n10: mín/p50/p95/máx **0,108291/0,229207/0,431284/0,431284 m**, sin umbral ni equivalencia con intención real.

Saltos de órdenes aplicadas, n5, p50/p95/máximo: **0,085290/0,184661/0,184661 m**; **9,404952/19,456272/19,456272 px**, HasJumpPixels=true en5; **2,132239/4,616518/4,616518 diámetros**. Sin suavizado ni criterio nuevo. PO acepta el salto en el modal con «si», no se deduce de estos valores.

| Coste | n | p50 ms | p95 ms | p99 ms | Máximo ms |
|---|---:|---:|---:|---:|---:|
| Planificador (una por recepción) | 10 | 0,104300 | 0,158200 | 0,158200 | 0,158200 |
| SolverCostMs | 5 | 0,041200 | 0,085200 | 0,085200 | 0,085200 |
| ReconstructionCostMs | 5 | 0,007800 | 0,022900 | 0,022900 | 0,022900 |
| PredictionCostMs | 5 | 0,058500 | 0,064500 | 0,064500 | 0,064500 |
| ReconciliationCostMs | 5 | 0,034000 | 2,676500 | 2,676500 | 2,676500 |

Percentiles crudos por rango más próximo, ceil(p·n), según §6.2. Partes de órdenes no aplicadas no se presentan como ceros medidos; costes anteriores pertenecen a las cinco aplicadas y planificación a diez lanzamientos únicos. Frame completo del golpe/asignaciones por parte **NotEvaluated**; no se suman percentiles. Datos originales completos y extracción en Logs ignorados; no nueva instrumentación.

**Extras:** cero gestos previos a la cuota; **4 posteriores**, con `launchSerial=80,81,82,83` → **LaunchId81,82,83,84** (LaunchId=launchSerial+1). No se suman a las diez recepciones. No se pidió repetir; se conservan como desviación. Tres archivos byte a byte iguales al respaldo tras export. Las respuestas globales de intención no se convierten en diez OK individuales: sin feedback individual asociado quedan «sin respuesta». Juicios de dirección/timing/salto conservados arriba sin reinterpretar.

### Transición a 15: segundo plano entre bloques

Al inspeccionar la UI después de recoger las respuestas de 14 se observa **«Toca para continuar»/Pausa**. No se presupone el instante exacto ni se atribuye al interior de 14: el tramo completo 71–80 estaba exportado y cerrado antes. Export adicional `between-14-15/Documents` preservado **489 líneas**, última **00:56:25.222598Z**; tres archivos iguales al respaldo. Aplicado §3.2: no continuar ese proceso; reinicio CLI nuevo aceptado (sin build/instalación), P0 persistido intacto. Falta verificar ese nuevo arranque, aplicar otra vez B→SL1 y completar nuevo precheck de15; todavía no solicitar gestos.

### Nuevo precheck de 15 POT-M aceptado

Proceso nuevo **metrics-20261102T00564278…**, arranque **2026-11-02T00:56:42.8062740Z**, misma build **0.1.0+2ef4807 dev/P0-calib-1/dd122831f592/pending0**. Tres archivos iguales tras el arranque; §3.2 satisfecho sin continuar el proceso con overlay. Preparación B→SL1→SL1 por nueva carga P0 sin guardar, selección/tres archivos recuperados y cotejados **antes de aplicar** (`after-load15-presets`). Aplicar y relanzar aceptado línea **180**, **2026-11-02T00:58:25.8809060Z**, paused=false/focused=true/devOpen=true. UI Jugando/P2/0pendientes/DEV abierto/sin overlay.

Precheck de **15 POT-M, 10 recepciones, SL1/V100/cámara fija/pulgar izquierdo dominante/potencia media voluntaria**, esperado externo **dd122831f592** = efectivo/recepciones, pending0. El conjunto B de106claves y cuerpo3.2/5.5 se coteja por hash completo tras recarga/aplicación; mismos control2/P2/tempo0.65/slowMotion1/A40/V100/comp0/T3nosuave. Export `pre-15/Documents`: **210 líneas**, todosH/pending iguales, **cero gestos** y ningún cierreDEV del agente. Tres archivos siguen iguales. Perfil vigente comprobado para esta tanda, vence7nov17:58:46Z; sin nueva instalación. **14 terminado no se repite; solo15 solicitado.**

**M_s de solicitud de cierre único DEV para15, formulación Mac previa al envío: 2026-11-02T00:59:00.564431+00:00 / 2026-11-01T21:59:00.564431-03:00.** Falta frontera única delPO/M_r; primera recepción será primerLaunchId posterior al cierre. No hay bloque15 iniciado por el agente.

## Recepción del cierre de 15 y preguntas en modal

PO: **«Terminé 15»**. Hora Mac secundaria de recepción-registro **2026-11-02T01:02:12.900626+00:00 / 2026-11-01T22:02:12.900626-03:00**; hora exacta del envío del PO no disponible. Export `after-15/Documents` completo; cotejo de los tres archivos byte a byte idénticos al respaldo. Frontera única, cuota y hashes recuperados en `block15-primary-private.json` ignorado; resultados reservados hasta recoger las respuestas. M_r aproximado del intercambio, sin sincronización calibrada con el dispositivo. No se prepara ni solicita16 todavía.

Preguntas agrupadas en un modal según petición literal del PO, cada una con campo propio; desviación de modalidad ya registrada, sin resultados previos. Preguntas1/2de§6.1 y tres originales como feedback complementario de potencia (mismo alcance aclarado en14, no criterio nuevo). Formulación previa al envío Mac **2026-11-02T01:02:12.900626+00:00 / 2026-11-01T22:02:12.900626-03:00**:

| Pregunta literal | Respuesta nueva literal | Hora recepción-registro Mac |
|---|---|---|
| «Por número de recepción, ¿qué excepciones recuerdas: Apoyo sin querer, No fijó / falló u otra dificultad?» | **«Esta vez iba a distintas velocidades por lo que me costó agarrar los timing, pero todas fueron intencionadas»** | 2026-11-02T01:05:57.464054+00:00 / 2026-11-01T22:05:57.464054-03:00 (recepción-registro; hora exacta de envío no disponible) |
| «¿Seguiste la tarea y la intensidad que se pidió en este bloque? Si no, ¿qué intentabas hacer?» | **«Si»** | 2026-11-02T01:05:57.464054+00:00 / 2026-11-01T22:05:57.464054-03:00 (recepción-registro; hora exacta de envío no disponible) |
| «¿la pelota va donde apunto?» | **«Cuando logré golpear, si»** | 2026-11-02T01:05:57.464054+00:00 / 2026-11-01T22:05:57.464054-03:00 (recepción-registro; hora exacta de envío no disponible) |
| «¿el timing es justo?» | **«si, me costó agarrar el timing, pero si»** | 2026-11-02T01:05:57.464054+00:00 / 2026-11-01T22:05:57.464054-03:00 (recepción-registro; hora exacta de envío no disponible) |
| «¿el salto es aceptable?» | **«si»** | 2026-11-02T01:05:57.464054+00:00 / 2026-11-01T22:05:57.464054-03:00 (recepción-registro; hora exacta de envío no disponible) |

### Resultado de 15 POT-M (después de recoger el modal)

Proceso **metrics-20261102T00564278…**. Frontera única del PO línea **287**, **2026-11-02T01:00:15.1467090Z**, timeReal **214.46274866875058**, dev-toggle/result/completed/accepted=true/devOpen=false/paused=false/focused=true. Primera recepción posterior línea **292**, **2026-11-02T01:00:16.2440040Z**, LaunchId53; cuota **L53–L62**, diez sucesivas completas. Último cierre final línea **359**, **2026-11-02T01:01:02.8650430Z**. Sin reapertura DEV en el tramo; hash externo/lanzamiento/gesto **dd122831f592**, pending0. Todas las versiones se conservan; resultados usan una final por recepción. Campo `comparableToArmedBlock=false` del protocolo S1 no se confunde con la comprobación externa S2.

| R | LaunchId | Línea final | Resultado / motivo | P (%) | Contacto aplicado |
|---|---:|---:|---|---:|---|
| 1 | 53 | 296 | Failed / outside-v | 52,837944 | No |
| 2 | 54 | 303 | Failed / before-k-in | 67,897230 | No |
| 3 | 55 | 310 | Failed / after-k-out | 72,080356 | No |
| 4 | 56 | 317 | Failed / after-k-out | 56,519103 | No |
| 5 | 57 | 324 | Failed / outside-v | 63,881439 | No |
| 6 | 58 | 331 | Perfect / applied | 60,200262 | Sí |
| 7 | 59 | 338 | Perfect / applied | 59,363633 | Sí |
| 8 | 60 | 345 | Perfect / applied | 58,527005 | Sí |
| 9 | 61 | 352 | Failed / after-k-out | 61,204195 | No |
| 10 | 62 | 359 | Valid / applied | 50,328058 | Sí |

**10 Swing/10 recepciones, 4 contactos**; resultados **{'Failed': 6, 'Perfect': 3, 'Valid': 1}**. Bandas vigentes: **0 suaves/5 medios/5 fuertes** sobre 10 Swing, sin sustituir fallos. Entre contactos: **3/4** en media. Saturación **0**. Muestras **336**, O2 **0**. La declaración global de intención no se convierte en OK por recepción; no se identificaron excepciones individuales por número y se mantienen sin respuesta propia.

| Métrica cruda (unidad en nombre) | n | Mínimo | p50 | p95 | p99 | Máximo |
|---|---:|---:|---:|---:|---:|---:|
| P (%) | 10 | 50,328058 | 59,363633 | 72,080356 | 72,080356 | 72,080356 |
| Duración Began→Ended (ms) | 10 | 1212,705292 | 1529,412084 | 1854,537667 | 1854,537667 | 1854,537667 |
| Distancia descendente estimada DPI (mm) | 10 | 18,608259 | 21,589998 | 25,786518 | 25,786518 | 25,786518 |
| Aim(Began)→C, referencia verbal sin marcador (m) | 10 | 0,292350 | 0,951901 | 2,263438 | 2,263438 | 2,263438 |
| DeltaMs | 10 | -277,891985 | -35,742652 | 189,492680 | 189,492680 | 189,492680 |
| JumpMeters, aplicadas | 4 | 0,016483 | 0,018264 | 0,030472 | 0,030472 | 0,030472 |
| JumpPixels, aplicadas/HasJumpPixels | 4 | 1,057217 | 1,171442 | 3,081582 | 3,081582 | 3,081582 |
| JumpBallDiameters, aplicadas | 4 | 0,412064 | 0,456611 | 0,761788 | 0,761788 | 0,761788 |
| PlannerCostMs, uno por lanzamiento | 10 | 0,090800 | 0,136100 | 0,171000 | 0,171000 | 0,171000 |
| SolverCostMs, aplicadas | 4 | 0,026000 | 0,030400 | 0,122500 | 0,122500 | 0,122500 |
| ReconstructionCostMs, aplicadas | 4 | 0,003700 | 0,003900 | 0,018200 | 0,018200 | 0,018200 |
| PredictionCostMs, aplicadas | 4 | 0,034300 | 0,053000 | 0,059300 | 0,059300 | 0,059300 |
| ReconciliationCostMs, aplicadas | 4 | 0,025300 | 0,027700 | 1,487400 | 1,487400 | 1,487400 |

Rango más próximo ceil(p·n), sin interpolación; muestras de coste por parte solo donde se ejecutó y una planificación por recepción. No sumar percentiles ni ceros no medidos; frame completo/asignaciones por parte **NotEvaluated**. No se infieren Pass, precisión humana o calibración. **Extras:** 0 gestos previos, **3 posteriores**, **LaunchId 63,64,65**, contados por launchSerial=LaunchId−1 y fuera de cuotas. Presets/selección idénticos al respaldo tras el export.

### Aclaración de «distintas velocidades»

Antes de preparar16 se pidió aclaración sin cambiar parámetros. Literal del PO: **«Ritmo entre lanzamientos»** (recepción-registro Mac **2026-11-02T01:05:57.464054+00:00 / 2026-11-01T22:05:57.464054-03:00**; hora exacta de envío no disponible). No se refiere a velocidad de entrada ni salida. Contraste de registros: los diez planes de SL1 tienen idénticos intervalos relativos lanzamiento→frontis **0,715897s**, primer bote **1,741539s**, Kout **2,031727s**, dentro de la precisión registrada. Entre inicios consecutivos de cuota: **4,167034–6,668706s**; los tramos más largos siguen a contactos aplicados. Es compatible con `PracticeMode` aprobado (Resolved→Observing hasta terminar vuelo/límite observeMaxSeconds, después Waiting/repeatSeconds), no un temporizador metronómico constante. Esta correspondencia es interpretación de código/registro, **no prueba del origen de toda sensación del PO**. Se conserva la dificultad literal del timing, sin nuevo criterio ni ajuste. Con la aclaración y este contraste no se identifica un defecto distinto que requiera la parada de§7.

### Acceso entre15 y16

Duplicación muestra **«iPhone en uso»** después del cierre de15. Al intentar Conectar pide autenticación local del Mac; se solicitó al PO completar el aviso directamente, sin compartir contraseña. Export `between-15-16/Documents` recuperado antes de otro arranque; precheck16 aún no realizado, ningún gesto16 solicitado. Aplicar§3.2/proceso nuevo/P0/cotejo completo al recuperar acceso, sin prolongar un bloque de15 ya cerrado. Restauración final de tanda2 aún pendiente; la selección/archivos persistidos siguenP0 y no se afirma un arranque nuevo todavía.


## Reanudación autorizada por el PO — 2026-11-04, segmento nuevo antes de16

El PO vuelve y pide literalmente: «Volví. Retomemos la tanda 2 de PR #30 desde el bloque 16, tal como estaba predeclarado». Solicita ventana/perfil, proceso nuevo/P0/cero pendientes y precheck. **Nueva instrucción vigente:** «Las preguntas van literales, una a la vez y antes de mostrar resultados. Avísame qué necesitas de mí en cada paso». Sustituye desde esta reanudación el modal agrupado solicitado el1nov; no modifica ni borra respuestas/desviación de14/15. No repetir14/15 ni simular continuidad de día/proceso.

Evidencia local ignorada nueva: Logs/evidence/s2-session27-tanda2-resume-20261104/. Trabajo solo en la campaña originalPR#30/producto9131b1c/dev2ef4807; no usar PR#32, sus defaults o su regla. Diff deproducto frente a9131b1c vacío. Sinbuild/instalación/tests nuevos.

### Ventana y respaldo antes del proceso nuevo

Comprobación delperfil incorporado a la appdev firmada preservada **2026-11-04T23:46:12.497332Z**: creación31oct17:58:46Z/vencimiento **7nov17:58:46Z (14:58:46Chile)**; firma estricta,bundle/equipo/dispositivo/entitlement coincidentes. Restan≈66,21h; cubre la reserva técnica propuesta de90min para37recepciones restantes16–22 yrestauración. La reserva no es un criterio ni duración mínima. Hora límite personal solicitada porchat, aún sin respuesta al preparar16; no se presume disponibilidad confirmada. Tandas3/PROD/restauraciones y revisiones aún pendientes conservan prioridad dentro delperfil; revalidar ventana antes de cada tramo yavisar antes deinstalar si noalcanza.

Export before-new-process **23:46:12.533215Z→23:46:19.137811Z**,31metrics/22.589.613bytes, documentos completos preservados antes de abrir otro proceso/retención. P0-calib-1/Po-1/last-active idénticos byte a byte alrespaldo válido inicial detanda2 (before-session-retry), que ya estaba cotejado con referencia inicial detanda1. Mismos bytes/SHA de3614–2843–10bytes registrados arriba. No se sobrescribe evidencia previa.

### Proceso nuevo y P0 verificados

Reinicio CLI sin instalar aceptado **23:46:46.815873Z** (horaMac secundaria de finalización). Nuevo proceso prefijo **metrics-20261104T23464928…**: línea1 **2026-11-04T23:46:49.3045200Z**, build **0.1.0+2ef4807dev**, preset **P0-calib-1**, hash **dd122831f592**, **pending0**. Exportstartup23:47:38→23:47:43, tres archivos de nuevo byte a byte iguales. UI confirma P0/P2/Jugando/0pendientes, sin overlay; no segundo plano/bloqueo observado durante esta preparación. No se infiere horaUTC sincronizadaMac-dispositivo.

### Precheck de16 POT-F aceptado, bloque sin iniciar

Codex abreDEV (no frontera debloque), cargaP0 sin guardar en líneas178/180 (resultado23:49:34.082407Z) yrecupera/coteja los3archivos **antes deaplicar** (after-load-before-apply,23:49:41→23:49:46, todosiguales). Aplicar/relanzar anclaSL1 aceptado línea232 **23:50:21.6751200Z**, paused=false/focused=true/devOpen=true. La condición solicitada es tambiénSL1: B→SL1→SL1, sin vía directa desde otra receta ni otrohash. Parámetros abierto poroperador línea25623:50:43.004283Z; DEV sigueabierto.

**16POT-F:10recepciones, SL1/V100/T3nosuave/cámarafija/pulgarizquierdodominante; buscarC y anillo, potencia fuerte voluntaria, sin exigir máximo.** Hexterno=efectivo=recepción **dd122831f592**, pending0. Igualdad delhash completo de106claves respecto almanifiestoB tras cargar/aplicar; cuerpo3,2/5,5/control2/P2/tempo0,65/slowMotion1/A40/V100/comp0/2–35mm intactos. UIJugando/sinpausa/DEVabierto; contexto nativo aplicado sin pausa/con foco.

Exportpre16 **23:50:49.602400Z→23:50:55.302701Z**:265líneas delproceso, todosHdd/pending0, **0gestosM24 y0cierresDEV**; último plan línea265/LaunchId60 hasPlan/front/bounce=true. Tres archivos byte a byte iguales después deaplicar. Todos estos lanzamientos son preparación, fuera de cuota. No se armaS1B0–B5 ni se usa un contador inexistente.

Solo elPO cerraráDEV una vez para empezar16; frontera nativa yprimerLaunchId posterior aún pendientes. Eloperador no accede al teléfono durante los gestos. Tras las10recepciones completas y avisoPO, exportar y preguntar literal/una auna antes de mostrar resultados; no solicitar17 hasta cerrar16. Restauración final detanda2 pendiente.


**Disponibilidad confirmada posteriormente porPO:** a la pregunta de reserva hasta22:15Chile, respuesta literal **«Hasta las 22:15 o más»**; recepción-registroMacUTC **2026-11-04T23:57:39.477815+00:00**. Se dispone almenos hasta2026-11-04T22:15:00−03:00, incluida restauración. No se infiere disponibilidad ilimitada ni cierre de16. Bloque16 solicitado solo alPO; no acceder al dispositivo durante sus gestos.


### Cierre de16: evidencia reservada y nueva modalidad pedida porPO

POavisa literalmente **«Termine 16»**; export after-16 conservado y cotejo3archivos idénticos. No se prepara17 ni se muestran resultados antes del cierre de preguntas. Primera pregunta literal enviada: «Por número de recepción, ¿qué excepciones recuerdas: Apoyo sin querer, No fijó / falló u otra dificultad?». Respuesta completa literal **«Fueron todas bien. Solo que el timing me costó un poco mas. Deja el modal completo para ir 1 a 1 las preguntas , así vamos mas rápido»**; recepción-registroMacUTC **2026-11-05T00:00:28.437746+00:00**. La primera parte se conserva como respuesta global, sin convertirla enOKindividuales ni confirmación de tarea/intensidad.

La última frase es la nueva instrucción de modalidad: **modal completo con campos literales separados**, para responderlos uno auno; sustituye desdeahora la entrega de una sola pregunta por mensaje solicitada alretomar. Desviación deenvío agrupado autorizada explícitamente porPO; ninguna pregunta ocriterio cambia. Se ofrecen las cuatro pendientes (tarea y tres originales como feedback complementarioPOT-F, mismo alcance de14/15), sin repetir la deexcepciones ni mostrar resultados. Respuestas pendientes.


### Respuestas restantes de16 recibidas antes de mostrar resultados

Recepción-registro MacUTC **2026-11-05T00:04:59.036946+00:00**; hora exacta de envío no disponible. Modal agrupado por petición literal delPO, cada pregunta/campo por separado.

| Pregunta literal | Respuesta literal |
|---|---|
| «¿Seguiste la tarea y la intensidad que se pidió en este bloque? Si no, ¿qué intentabas hacer?» | **«si, segui la intensidad correctamente»** |
| «¿la pelota va donde apunto?» | **«si, cuando logré que salga la pelota si»** |
| «¿el timing es justo?» | **«si»** |
| «¿el salto es aceptable?» | **«si»** |

### Resultado de16 POT-F

Proceso **metrics-20261104T23464928…**, frontera única línea **652**, **2026-11-04T23:57:33.6333430Z**, timeReal **646.4526684223357**; dev-toggle/result/completed/accepted=true/devOpen=false/paused=false/focused=true. Cuota **L156–L165**, diez sucesivas completas; primera planificación posterior al cierre, sin escoger por éxito. Último cierre final línea **733 / 2026-11-04T23:58:17.2553060Z**. Hash externo/recepción/gesto **dd122831f592**, pending0. Todas las comprobaciones de integridad guardadas pasan; todas las versiones primarias se conservan.

| R | LaunchId | Línea final | Resultado / motivo | P (%) | Contacto aplicado |
|---|---:|---:|---|---:|---|
| 1 | 156 | 657 | NoOrder / reception-ended-without-order | 0.000000 | No |
| 2 | 157 | 664 | Failed / after-k-out | 67.395258 | No |
| 3 | 158 | 671 | Valid / applied | 69.570512 | Sí |
| 4 | 159 | 691 | Failed / outside-v | 80.111980 | No |
| 5 | 160 | 698 | Failed / outside-v | 79.944670 | No |
| 6 | 161 | 705 | Valid / applied | 74.757582 | Sí |
| 7 | 162 | 712 | Failed / after-k-out | 79.610032 | No |
| 8 | 163 | 719 | Failed / outside-v | 74.590236 | No |
| 9 | 164 | 726 | Failed / after-k-out | 87.474304 | No |
| 10 | 165 | 733 | Valid / applied | 80.948645 | Sí |

**9 Swing/10 recepciones, 3 contactos**; resultados {'NoOrder': 1, 'Failed': 6, 'Valid': 3}. Primera recepción NoOrder conservada, sin sustitución. **9/9 Swing fuertes**,0 suaves/0 medios según bandas vigentes;3/3 contactos fuertes. Saturación0/9. Respuesta global no se convierte enOK individuales; no hay excepciones numeradas propias. Muestras 366, O2 0.

| Métrica cruda (unidad en nombre) | n | Mínimo | p50 | p95 | p99 | Máximo |
|---|---:|---:|---:|---:|---:|---:|
| P (%) | 9 | 67.395258 | 79.610032 | 87.474304 | 87.474304 | 87.474304 |
| Duración Began→Ended (ms) | 9 | 1129.413917 | 1529.424041 | 1629.310000 | 1629.310000 | 1629.310000 |
| Distancia descendente estimada DPI (mm) | 9 | 24.240435 | 28.271311 | 30.866520 | 30.866520 | 30.866520 |
| Aim(Began)→C, referencia verbal sin marcador (m) | 9 | 0.273857 | 0.538937 | 1.019733 | 1.019733 | 1.019733 |
| DeltaMs | 9 | -160.162239 | -52.691240 | 30.665386 | 30.665386 | 30.665386 |
| JumpMeters, aplicadas | 3 | 0.023938 | 0.245992 | 0.319538 | 0.319538 | 0.319538 |
| JumpPixels, aplicadas | 3 | 2.491828 | 25.551849 | 33.794376 | 33.794376 | 33.794376 |
| JumpBallDiameters, aplicadas | 3 | 0.598440 | 6.149812 | 7.988459 | 7.988459 | 7.988459 |
| SolverCostMs, aplicadas | 3 | 0.030200 | 0.039600 | 0.046500 | 0.046500 | 0.046500 |
| ReconstructionCostMs, aplicadas | 3 | 0.003700 | 0.006300 | 0.010100 | 0.010100 | 0.010100 |
| PredictionCostMs, aplicadas | 3 | 0.033900 | 0.041200 | 0.058200 | 0.058200 | 0.058200 |
| ReconciliationCostMs, aplicadas | 3 | 0.033600 | 0.051500 | 2.372300 | 2.372300 | 2.372300 |
| PlannerCostMs, uno por lanzamiento | 10 | 0.058400 | 0.124200 | 0.191200 | 0.191200 | 0.191200 |

Rango más próximo ceil(p·n), sin interpolación; costes por parte solo donde se ejecutó, sin sumar percentiles/ceros no medidos. Frame completo/asignaciones por parte NotEvaluated. Sin nuevos criterios, calibración o aceptaciónS2 inferidos.

Extras fuera de cuota hasta exportafter16: **2** gestos, LaunchId **[166, 167]**, por launchSerial=LaunchId−1; no suman recepciones medidas. Tres archivos byte a byte iguales alrespaldo en ese export.

### Incidencia entre16 y17

Al observar Duplicación después de recoger respuestas y extraer16, aparece **«Toca para continuar»/Pausa**. Se conserva captura y evidencia previa; no se atribuye causa concreta. Aplicar§3.2: exportantes deotroarranque, proceso nuevo/P0dd122831f592/pending0/cotejo3/precheck17 completo. Ningún17 solicitado ni preparación realizada todavía.


### Proceso nuevo y precheck17 CAN aceptado

Exportbetween16-17 preservado antes de reiniciar;3archivos iguales. Proceso nuevo **2026-11-05T00:05:16.6685720Z / 0.1.0+2ef4807dev / P0-calib-1 / dd122831f592 / pending0**. Exportstartup-before17 coteja de nuevo los3archivos. UI normal/sinoverlay/Pausa disponible, luegoDEVabierto/Jugando/P2. CargaB desdeP0 sin guardar: resultado línea56/00:05:55.563299Z. Cotejobefore17-load-before-apply completo antes deaplicar,3iguales. Aplicar/relanzar anclaSL1 resultado línea79/00:06:13.889473Z, acceptedtrue/pausedfalse/focusedtrue/devOpentrue. Receta solicitadaSL1: **B→SL1→SL1**; sin vía directa desde otra receta. Parámetros resultado87/00:06:20.194054Z.

Precheck **17 CAN:5recepciones/SL1/V100/T3no suave/cámarafija/pulgarizq/control2/P2**, cuerpo(3,2;5,5), mismos106valores deB porigualdad literal delhashcompleto contra manifiesto independiente **dd122831f592**. Todos los registros/exportpre17 mismohash/pending0;100líneas/0gestos/0cierresDEV. UIJugando/sinpausa/DEVabierto; foco verdadero en auditoría. Tresarchivos byte a byte iguales despuésdeaplicar. Últimaobservación sinbloqueo/segundoplano enesta preparación. **SoloPOcierraDEV para iniciar17; frontera aúnpendiente**, M_sMacUTC **2026-11-05T00:07:14.883733+00:00**. Vuelo ya iniciado alcerrar ytodoslanzamientos previos quedanpreparación. NoS1B0–B5.

Se solicita solo17: cinco recepciones consecutivas completas, sin ampliar/repetir por fallos/omisiones; cancelar porretorno y reapoyar para medio siqueda tiempo. Triada por intento trasterminal, sinintervenir contacto ni ampliarrecepción; conservar ordinal deprimerapoyo/reapoyo o«sinrespuesta» sininferirOK. Modal de cierre porinstrucciónPO, antes deresultados. No18hasta recuperar/cerrar17. Restauraciónfinaltanda2pendiente; perfil7nov17:58:46Z yventanapersonal hasta22:15Chile aún cubrenresto/restauración.

### Aviso de cierre17 y preguntas pendientes

LiteralPO **«Terminé 017»**, recepción-registroMacUTC **2026-11-05T00:09:59.770732+00:00**. Exportafter17 preservado; los3archivos siguenbyte a byte iguales alrespaldo. Integridad/frontera/cuota/hash verificados internamente; resultados reservados hasta recoger feedback. Sin preparar18 ni más accesoUI.

Modal completo porpetición vigentePO, formulación-registroMacUTC **2026-11-05T00:11:00.189121+00:00**: triada literal porcada intento identificado (contexto recepción1–5/primerapoyo), tarea y trespreguntas originales, campos separados. Triada después delterminal, aquí retrospectiva al cierre sininterrumpir elcontacto activo ni ampliarrecepciones; se declara esta modalidad temporal, sininferir respuesta ni intención desdecierres técnicos. SiPOnoasocia respuesta, «sinrespuesta». Sinresultados previos.

### Respuestas de17, recibidas antes de mostrar resultados

Recepción-registroMacUTC **2026-11-05T00:13:17.270424+00:00**; horaexacta deenvío nodisponible. Triada retrospectiva de cierre, campos literales separados enmodal porpeticiónPO.

| Contexto | Pregunta literal | Respuesta literal |
|---|---|---|
| recepción 1, primer apoyo | «¿Fue querido el apoyo de este intento: OK, Apoyo sin querer o No fijó / falló?» | **«Ok»** |
| recepción 2, primer apoyo | «¿Fue querido el apoyo de este intento: OK, Apoyo sin querer o No fijó / falló?» | **«Ok»** |
| recepción 3, primer apoyo | «¿Fue querido el apoyo de este intento: OK, Apoyo sin querer o No fijó / falló?» | **«OK»** |
| recepción 4, primer apoyo | «¿Fue querido el apoyo de este intento: OK, Apoyo sin querer o No fijó / falló?» | **«OK»** |
| recepción 5, primer apoyo | «¿Fue querido el apoyo de este intento: OK, Apoyo sin querer o No fijó / falló?» | **«creo que no miré y miré las reglas»** |
| cierre | «¿Seguiste la tarea y la intensidad que se pidió en este bloque? Si no, ¿qué intentabas hacer?» | **«Si»** |
| cierre | «¿la pelota va donde apunto?» | **«Fueron cancelaciones, no hubo golpe»** |
| cierre | «¿el timing es justo?» | **«si»** |
| cierre | «¿el salto es aceptable?» | **«si»** |

Recepciones1–4: cuatroOK declarados, sin convertirlos en acierto de timing/puntería. Recepción5: **«creo que no miré y miré las reglas»** no corresponde inequívocamente a una categoría: **sin respuesta clasificable**, originalintacto; noinferir apoyoaccidental/fallo niomisión desdeesta frase. Tarea«Si» conservado sininferir que se hicieron reapoyos. Dirección«Fueron cancelaciones, no hubo golpe» conservado; timing/salto«si» son juicios literales, sin salto físico medido en cuota.

### Resultado de17 CAN

Proceso **metrics-20261105T000516652-e9cdebfabcb1499385f9e52e5e4c48c6.jsonl**, fronteraúnica línea **247 / 2026-11-05T00:09:05.6631910Z**, timeReal **231.1290708798186**; dev-toggle/result/completed/acceptedtrue/devOpenfalse/pausedfalse/focusedtrue. Cuota **L56–L60**, cinco sucesivas completas, sin escoger por éxito. Hash externo/recepción/gestos **dd122831f592**, pending0; sin otroDEV en ventana. Primerplan posterior al cierre y todasversiones conservadas.

| R | LaunchId | Línea gesto | Cierre gesto | Línea final recepción | Resultado recepción | Respuesta triada |
|---|---:|---:|---|---:|---|---|
| 1 | 56 | 252 | CanceledReturnToDeadZone | 254 | NoOrder / reception-ended-without-order | «Ok» |
| 2 | 57 | 258 | CanceledReturnToDeadZone | 260 | NoOrder / reception-ended-without-order | «Ok» |
| 3 | 58 | 264 | CanceledReturnToDeadZone | 266 | NoOrder / reception-ended-without-order | «OK» |
| 4 | 59 | 271 | CanceledReturnToDeadZone | 270 | NoOrder / reception-ended-without-order | «OK» |
| 5 | 60 | 276 | CanceledReturnToDeadZone | 278 | NoOrder / reception-ended-without-order | «creo que no miré y miré las reglas» |

**5cancelaciones C-A por retorno/5recepciones,0Swing/0contactos/0reapoyos registrados en cuota**. Todas lasrecepciones cierranNoOrder al no haber ordenSwing: no es otra cancelación ni cincofallos deSwing. Cancelación deR4/gesto línea271 termina después delcierreNoOrder línea270; se conserva poridentidad launchSerial58→LaunchId59 y no se cambia la cuota pororden de escritura. La tarea permitía reapoyo solo si quedaba tiempo; no se prolonga ni repite para obtenerlo. Voluntariedad individual solo donde elPO diorespuesta; la cancelación técnica no demuestra por sí sola voluntad.

| Métrica cruda | n | Mínimo | p50 | p95 | p99 | Máximo |
|---|---:|---:|---:|---:|---:|---:|
| Duración Began→terminal(ms) | 5 | 1095.944083 | 1379.339917 | 2712.870042 | 2712.870042 | 2712.870042 |
| Aim(Began)→C,referenciaverbalsinmarcador(m) | 5 | 0.295969 | 0.605916 | 0.780181 | 0.780181 | 0.780181 |
| Máxima distancia descendente estimadaDPI(mm) | 5 | 18.663490 | 23.853918 | 28.492172 | 28.492172 | 28.492172 |
| PlannerCostMs,unoporlanzamiento | 5 | 0.077900 | 0.125700 | 0.149800 | 0.149800 | 0.149800 |

Muestras **277**, O2 **0**. Potencia deSwing/bandas/saturación/DeltaMs/salto/costes decontacto: **NotEvaluated (0Swing/0contactos)**, sin introducir ceros como mediciones. Rango más próximo ceil(p·n),sininterpolación. Extraspostcuota **3gestos/LaunchId61,62,63** porlaunchSerial=LaunchId−1; separados de los5apoyos/cuota, no amplían muestra. Tresarchivos byte a byte iguales enexportafter17. Sinajustes/criterios nuevos/aceptaciónS2inferida.

### Preparación18 iniciada tras cerrar17

UIentrebloquesnormal/sinoverlay/Pausa disponible/build2ef4807/P0dd122831f592. No se observa segundo plano/bloqueo aquí; no se reinicia porinferencia. Se preparaB→SL1→SL1 conprecheckcompleto para18NEG-A; todavía no solicitado.


### Precheck18 NEG-A aceptado

Mismo proceso **metrics-20261105T000516652-e9cdebfabcb1499385f9e52e5e4c48c6.jsonl** verificado nuevo/P0 alpreparar17; sinreinicio/segundoplano observado entre17y18. DEVabierto poroperador; cargaB/P0 sin guardar resultado línea **423 / 2026-11-05T00:14:30.4670430Z**. Exportbefore18-load-before-apply coteja3archivos iguales **antes** de aplicar. Aplicar/relanzar anclaSL1 resultado **446 / 2026-11-05T00:14:48.3029900Z**, acceptedtrue/pausedfalse/focusedtrue/devOpentrue; condiciónSL1→vía **B→SL1→SL1**. Parámetros resultado **454 / 2026-11-05T00:14:53.2765900Z**.

**18NEG-A:3recepciones/SL1/V100/T3no suave/cámarafija/pulgarizq/control2/P2/cuerpo3,2;5,5.** Referencia externa **dd122831f592**=efectivo=recepción; pending0. Igualdad delhashcompleto106valores contra manifiestoB. UIJugando/sinpausa/sinoverlay/DEVabierto; auditoría con foco. Exportpre18 **467líneas**,0gestos/0cierresDEV desdeaplicación. Tresarchivos byte a byte iguales. RegistroM_sMacUTC **2026-11-05T00:15:35.877662+00:00** alformular solicitud de cierreDEV; **soloPOcierrauna vez**, frontera aúnpendiente/recepción1primer lanzamiento posterior; vueloencurso/preparación no cuentan.

Solicitar solo18/tresrecepciones consecutivas completas: apoyo sin tirón yretirada/incluirduda breve sinmedirla/sinpedirSwing oP0 contirónintencional. Fallos/omisiones no sesustituyen. Triada porapoyo/cierre literal, modal separadoporpedidoPO yantesderesultados; no19hasta revisión. Restauraciónfinaltanda2pendiente.

### Aviso de cierre18 y feedback pendiente

LiteralPO **«temine 18»**, recepción-registroMacUTC **2026-11-05T00:17:29.321985+00:00**. Exportafter18 preservado/3archivos byte a byte iguales alrespaldo. Integridad ycuota verificadasinternamente; resultados reservados. Modal formulación-registroMacUTC **2026-11-05T00:18:29.667603+00:00**: triada literal porrecepción1–3 ytarea, camposseparados/antes deresultados, porpeticiónPO. Feedback retrospectivo trasterminal al cierre; nointerrumpe contacto ni ampliarecepción, modalidadtemporal declarada. Contexto ordinal nofabrica un gesto ni unattemptId enrecepción sinregistro; asociar soloidentidades existentes. No19preparado ni otroaccesoUI.

### Respuestas de18 recibidas antes de mostrar resultados

Recepción-registroMacUTC **2026-11-05T00:18:51.661719+00:00**; horaexacta deenvío nodisponible.

| Contexto | Pregunta literal | Respuesta literal |
|---|---|---|
| Recepción1 | «¿Fue querido el apoyo de este intento: OK, Apoyo sin querer o No fijó / falló?» | **«OK»** |
| Recepción2 | «¿Fue querido el apoyo de este intento: OK, Apoyo sin querer o No fijó / falló?» | **«OK»** |
| Recepción3 | «¿Fue querido el apoyo de este intento: OK, Apoyo sin querer o No fijó / falló?» | **«OK»** |
| Cierre | «¿Seguiste la tarea y la intensidad que se pidió en este bloque? Si no, ¿qué intentabas hacer?» | **«si»** |

Tras recoger las cuatrorespuestas ymostrar la discrepancia instrumental, se pidió aclaración deasociación: «Bloque18: ¿tus tres«OK» corresponden,enorden,a los tresapoyos quehiciste,o alasrecepciones1–3 quecontaste?». Respuesta literal **«A las tres recepciones que conté»**, recepción-registroMacUTC **2026-11-05T00:20:07.956407+00:00**. Se conservan tresOK declarados porordinal de recepción yla tarea«si». **R1 no tiene gesto/attemptId registrado**: no se crea uno ni se convierte la declaración en una acción instrumentada. R2/R3 tienenidentidades. **El apoyoextraL122 no recibeOK porinferencia**; tampoco se mueve lafrontera/cuota o se sustituye R1. No repetición ni aceptaciónS2inferida.

### Resultado de18 NEG-A

Proceso **metrics-20261105T000516652-e9cdebfabcb1499385f9e52e5e4c48c6.jsonl**, fronteraúnica línea **578 / 2026-11-05T00:16:54.7675640Z**, timeReal **700.2326008378187**; dev-toggle/result/completed/acceptedtrue/devOpenfalse/pausedfalse/focusedtrue. Cuota **L119–L121**, tres sucesivas completas desdeprimerplan posterior; todasversiones conservadas. Hexterno=recepción=gestos **dd122831f592**, pending0; sinotroDEV enventana.

| R | LaunchId | Gesto | Línea gesto | Línea final recepción | Resultado recepción | FeedbackPO porordinal |
|---|---:|---|---:|---:|---|---|
| 1 | 119 | Sin gesto registrado | — | 583 | NoOrder / reception-ended-without-order | «OK» |
| 2 | 120 | AbandonedNoPull | 587 | 589 | NoOrder / reception-ended-without-order | «OK» |
| 3 | 121 | AbandonedNoPull | 593 | 595 | NoOrder / reception-ended-without-order | «OK» |

**3recepcionescompletas/2AbandonedNoPull/0Swing/0contactos**, sinreapoyos encuota. TresNoOrder soncierres de recepción sinordenSwing, no otrogesto ni tresfallos deSwing. **1gestoextra posterior**, LaunchId122/launchSerial121, AbandonedNoPull, separado de las3recepciones; no feedback individual asociado. ToquesDEVfiltrados no son gestos.

| Métrica cruda | n | Mínimo | p50 | p95 | p99 | Máximo |
|---|---:|---:|---:|---:|---:|---:|
| Duración Began→terminal(ms) | 2 | 354.215001 | 354.215001 | 1571.097833 | 1571.097833 | 1571.097833 |
| Aim(Began)→C,referenciaverbalsinmarcador(m) | 2 | 0.757178 | 0.757178 | 0.891793 | 0.891793 | 0.891793 |
| Máximo desplazamiento estimadoDPI(mm) | 2 | 0.000000 | 0.000000 | 0.000000 | 0.000000 | 0.000000 |
| PlannerCostMs,unoporlanzamiento | 3 | 0.103900 | 0.119300 | 0.139600 | 0.139600 | 0.139600 |

Muestras **4**,O2 **0**. PotenciaSwing/bandas/saturación/DeltaMs/salto/costescontacto **NotEvaluated(0Swing/0contactos)**, sin convertir ceros delregistro enmediciones. Rangomáspróximo ceil(p·n),sininterpolación. Tresarchivos byte a byte iguales enexportafter18. Sin parámetros/criterios/umbrales nuevos.


### Incidencia entre18/19 y proceso nuevo

Tras cerrar feedback/aclaración18, Duplicación muestra **«Toca para continuar»/Pausa**. Causa/hora de entrada noatribuidas; no se afirma foco continuo porausencia de controlesDEV. Se conserva exportbetween18-19 antes dearranque/3archivosiguales. Aplicar§3.2 antesde19: proceso nuevo observado **2026-11-05T00:20:37.0783100Z / 0.1.0+2ef4807dev / P0-calib-1 / dd122831f592 / pending0**; UI normal/sinoverlay/Pausa disponible. Exportstartup-before19 coteja3archivos iguales.18conservado sin repetición/ampliación;19aún sin gestos.

### Precheck19 NEG-B aceptado

Proceso **metrics-20261105T002037061-db8f69d7529b450ca51573540c8e9df4.jsonl**. CargaB/P0 sin guardar resultado **80 / 2026-11-05T00:21:41.4128190Z**; exportbefore19-load-before-apply coteja3archivos **antes** de aplicar. Aplicar/relanzar anclaSL1 resultado **107 / 2026-11-05T00:22:02.0024580Z**, acceptedtrue/pausedfalse/focusedtrue/devOpentrue; vía **B→SL1→SL1**, mismaSL1solicitada. Parámetros resultado **115 / 2026-11-05T00:22:07.3994680Z**.

**19NEG-B:3recepciones/SL1/V100/T3no suave/cámarafija/pulgarizq/control2/P2/cuerpo3,2;5,5.** Hexterno **dd122831f592**=efectivo=recepción/pending0, igualdad delhashcompleto106valores frentealmanifiesto independienteB. UIJugando/sinpausa/sinoverlay/DEVabierto/foco en auditoría; mira/anillo conservados. Exportpre19 **128líneas/0gestos/0cierresDEV**,3archivos byte a byte iguales. No segundo plano observado duranteesta preparación. **SoloPOcierraDEV una vez para empezar19**, M_sMacUTC **2026-11-05T00:22:57.422030+00:00**; frontera/primerLaunchIdposterior pendientes. Vueloencurso yprevios preparación/fuera cuota.

Solicitar solo19: apoyo ypequeñotemblor/rozamiento, sinintención decargar; registrar desplazamientoreal ycierre, sinforzar zona muerta ni milímetrosexactos. Tresrecepciones consecutivas completas/fallos-omisionesincluidos/sinrepetir. Triada porapoyo ytarealiteral enmodal decierre antes deresultados. No20hasta recuperar/cerrar19. Restauraciónfinaltanda2pendiente; perfil vigente7nov17:58:46Z yventanahasta22:15Chile aúncubrenresto19–22/restauración.

### Aviso de cierre19 y preguntas pendientes

LiteralPO **«termine 19 »** (espacio final conservado enestado local), recepción-registroMacUTC **2026-11-05T00:24:16.666306+00:00**. Exportafter19 preservado;3archivos byte a byte iguales alrespaldo. Frontera/cuota/hash verificadosinternamente; resultados reservados. Modal formulación-registroMacUTC **2026-11-05T00:25:25.585930+00:00**: triada literal porcada intento identificado deR2/R3 (primerapoyo) ytarea, campos separados porpeticiónPO. No se crea un intento para preguntarR1 sinregistro de gesto; quedará sinfeedback asociado aattemptId. Preguntas después delterminal, retrospectivas decierre/sininterrumpir contacto ni ampliarcuota. No20preparado ni accesoUIposterior.

### Respuestas de19 recibidas antes de mostrar resultados

Recepción-registroMacUTC **2026-11-05T00:29:21.232703+00:00**: ambas respuestas delmodal yaclaración ya recibidas alregistrar, horas exactas deenvío no disponibles.

| Contexto | Pregunta literal | Respuesta literal |
|---|---|---|
| Recepción2,primerapoyo | «¿Fue querido el apoyo de este intento: OK, Apoyo sin querer o No fijó / falló?» | **«ok»** |
| Recepción3,primerapoyo | «¿Fue querido el apoyo de este intento: OK, Apoyo sin querer o No fijó / falló?» | **«ok»** |
| Cierre | «¿Seguiste la tarea y la intensidad que se pidió en este bloque? Si no, ¿qué intentabas hacer?» | **«is»** |

Se pidió aclarar«is», sincorregirlo porinferencia. LiteralPO **«Sí, seguí la tarea»**. Originalconservado, adhesión declarada trasaclaración. R1singestoregistrado no recibeOK pordefecto; apoyoextra sinrespuesta propia.

### Resultado de19 NEG-B

Proceso **metrics-20261105T002037061-db8f69d7529b450ca51573540c8e9df4.jsonl**, fronteraúnica **213 / 2026-11-05T00:23:42.4049180Z**, timeReal **187.45480333560093**; dev-toggle/result/completed/acceptedtrue/devOpenfalse/pausedfalse/focusedtrue. Cuota **L47–L49**, tres sucesivas completas desdeprimerplan posterior al cierre, todasversiones conservadas. Hexterno=recepción=gesto **dd122831f592**, pending0; sinotroDEV enventana.

| R | LaunchId | Gesto | Línea gesto | Línea final recepción | Resultado recepción | FeedbackPO |
|---|---:|---|---:|---:|---|---|
| 1 | 47 | Sin gesto registrado | — | 218 | NoOrder / reception-ended-without-order | Sin respuesta/ningúnattemptId |
| 2 | 48 | AbandonedNoPull | 222 | 224 | NoOrder / reception-ended-without-order | «ok» |
| 3 | 49 | AbandonedNoPull | 228 | 230 | NoOrder / reception-ended-without-order | «ok» |

**3recepciones/2AbandonedNoPull/0Swing/0contactos**, sinreapoyos. TresNoOrder soncierres sinordenSwing, no tresfallos deSwing. Dosrespuestasok asociadas aidentidades R2/R3.

| Métrica cruda | n | Mínimo | p50 | p95 | p99 | Máximo |
|---|---:|---:|---:|---:|---:|---:|
| Duración Began→terminal(ms) | 2 | 762.594917 | 762.594917 | 1429.393876 | 1429.393876 | 1429.393876 |
| Aim(Began)→C,referenciaverbalsinmarcador(m) | 2 | 0.282185 | 0.282185 | 0.328110 | 0.328110 | 0.328110 |
| Máximo desplazamiento estimadoDPI(mm) | 2 | 2.037069 | 2.037069 | 2.430193 | 2.430193 | 2.430193 |
| Máxima distancia descendente estimadaDPI(mm) | 2 | 1.049130 | 1.049130 | 1.104355 | 1.104355 | 1.104355 |
| PlannerCostMs,unoporlanzamiento | 3 | 0.076400 | 0.088000 | 0.137800 | 0.137800 | 0.137800 |

Muestras **86**,O2 **0**. PotenciaSwing/bandas/saturación/DeltaMs/salto/costescontacto NotEvaluated(0Swing/0contactos); no cerosmedidos. Rango máspróximo ceil(p·n),sininterpolación.

**Extra posterior1/LaunchId50 (launchSerial49)**, línea234: Swing/P2,138319%; descensoalsoltar2,705645mm/máximodescenso3,754776mm/desplazamientomáx4,356223mm/lateral−1,656522mm. Fuera de cuota/feedback individual sinrespuesta, intención no inferida. Es compatible geométricamente conelriesgoN04descendente yaabierto/aceptado, no un detector de voluntad; sin filtro nuevo ni defecto distinto demostrado que active§7. No se suma a las2acciones dentro ni se usa parasustituirR1. Tresarchivos byte a byte iguales enexportafter19.

### Incidencia entre19/20

Duplicación muestra«Toca para continuar»/Pausa entrebloques; causa/hora deentrada noatribuidas. Exportbetween19-20 preservado/3cmpiguales. Aplicar§3.2/proceso nuevo/P0/hash0pendientes/cotejo antes delprecheck20;19conservado sinrepetir. Ningún20solicitado todavía.


### Proceso nuevo y precheck20 NEG-C aceptado

Nuevo arranque **2026-11-05T00:30:18.5199710Z / 0.1.0+2ef4807dev / P0-calib-1 / dd122831f592 / pending0**, según§3.2 trasoverlayentre19/20. Exportstartup-before20 yUI normal/sinoverlay/Pausa disponible comprobados;3archivos byte a byte iguales. Proceso **metrics-20261105T003018504-ba510538120642f9b7d9523514c86ceb.jsonl**. CargaB/P0 sin guardar resultado **96 / 2026-11-05T00:31:39.2301350Z**, exportbefore20-load-before-apply coteja3antesdeaplicar. Aplicar/relanzar anclaSL1 resultado **127 / 2026-11-05T00:32:03.1862550Z**, acceptedtrue/pausedfalse/focusedtrue/devOpentrue. Vía **B→SL1→SL1**, mismaSL1solicitada; Parámetros resultado **135 / 2026-11-05T00:32:09.4444560Z**.

**20NEG-C:3recepciones/SL1/V100/T3no suave/cámarafija/pulgarizq/control2/P2/cuerpo3,2;5,5.** Hexterno **dd122831f592**=efectivo=recepción/pending0; igualdad delhashcompleto106valores contra manifiesto independienteB. UIJugando/sinpausa/sinoverlay/DEVabierto ycon foco en auditoría; mira/anillo conservados. Exportpre20 **145líneas/0gestos/0cierresDEV**,3archivos byte a byte iguales despuésdeaplicar. Sinsegundoplano observado duranteesta preparación. **SoloPOcierraDEV una vez para empezar20**, M_sMacUTC **2026-11-05T00:33:00.022598+00:00**; frontera/primerLaunchIdposterior aúnpendientes. Vueloencurso/previos quedanpreparación.

Se solicita solo20: tresrecepciones consecutivas completas, imitarresbalón descendente trasapoyar sinbuscar golpe; puedeproducirSwing/contacto porN04conocido. **Resbalónsolicitado parasimular riesgo**, nofrecuencia espontánea ni detección devoluntad. NoexigirP0/milímetrosexactos/ajustar parámetros/repetir omisiones-fallos. Triada porapoyo ytarealiteral al cierre/modal porpeticiónPO/antes deresultados; no21hasta recuperar/cerrar20. Perfil7nov17:58:46Z/reserva hasta22:15Chile aún cubrenresto20–22/restauraciónfinalpendiente.
