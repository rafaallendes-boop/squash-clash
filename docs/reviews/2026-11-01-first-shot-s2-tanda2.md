# First Shot S2 — tanda 2 de 2.7

## Estado

Retomada por Codex desde `e160f98`, rama `firstshot/s2-m24`, PR #30 sin fusionar. Decisión de repreguntar registrada en `c336c51`; procedimiento confirmado y precisado literalmente por el PO en [aprobaciones](2026-10-25-first-shot-po-approvals.md). Rigen la [predeclaración](../validation/2026-10-31-first-shot-s2-session27-predeclaration.md) y el estado vigente de AGENTS.md. App prevista: **0.1.0+2ef4807 dev ya instalada**, sin build/instalación nuevos.

**Cinco repreguntas recogidas y precheck de 14 completado; bloque 14 aún no iniciado.** Tanda 2 prevista: 14–22, 57 recepciones SL1. Perfil y P0 de esta tanda comprobados abajo; la restauración final de tanda 2 sigue pendiente. No se atribuyen aquí las comprobaciones de tanda 1. Al cierre se requiere exportación/restauración en proceso nuevo/dd122831f592/pending 0 y cotejo de los tres archivos; entrega para Claude/PO y parada. S3–S4/T8 no autorizados.

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
