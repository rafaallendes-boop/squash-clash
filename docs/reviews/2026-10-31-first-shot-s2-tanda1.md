# First Shot S2 — tanda 1 de 2.7

## Estado de la sesión

Autorización del PO del 2026-10-31 tras revisión de Claude del head `68f5383`. Misma rama `firstshot/s2-m24`, PR #30 abierta y sin fusionar. Solo FAM (01) y 02–13, en el orden y condiciones de la [predeclaración aprobada](../validation/2026-10-31-first-shot-s2-session27-predeclaration.md); [literales esperados](../validation/2026-10-31-first-shot-s2-session27-hashes.md) sin cambios.

**Tanda 1 terminada y cerrada el 2026-11-01 (America/Santiago), antes de las 22:00.** Hay un bloque medido para FAM y para cada uno de los 12 bloques 02–13. Se conservan además dos parciales (03 intento 1 y 04 intento 1) junto con sus repeticiones 03b/04b, decididas por el PO. P0 se restauró en un proceso nuevo y los tres archivos son idénticos byte a byte al respaldo.

Este informe es descriptivo: **no infiere aceptación, no fija umbrales y no declara defectos**. El juicio queda para Claude/PO.

**Cambio de operador:** Codex preparó la sesión e inició la comprensión, hasta el 2026-10-31 ~22:19Z. Al quedarse Codex sin créditos, el PO pidió a Claude que operara el resto de la tanda desde la PC del PO, con Duplicación del iPhone, `devicectl` y extracción de registros. Consecuencia declarada: **Claude fue operador y no es revisor independiente de esta tanda** (ver §Limitaciones).

## Comprobaciones previas

Perfil incorporado comprobado de nuevo: inicio `2026-10-31T17:58:46Z`, vencimiento `2026-11-07T17:58:46Z`. Firma estricta, equipo, dispositivo y bundle coinciden. El perfil cubre toda la ventana autorizada, incluida la restauración. No hubo build ni instalación nuevas.

Respaldo nuevo de Documents antes de abrir (`before-session`). El primer intento se interrumpió por un fallo de transporte; la repetición terminó correctamente. Los tres archivos coinciden byte a byte con la referencia aprobada:

| Archivo | Bytes | SHA-256 |
|---|---:|---|
| P0-calib-1.json | 3614 | `2f1295f201e238d9494bee285dd3f46e5bbb0f98b09ccde90fbd268f0874e4fd` |
| Po-1.json | 2843 | `9008c33ab89f82e669bc227a81b37ed641f69e4657ee70df6c89ff0c0edb1767` |
| last-active.txt | 10 | `5885ce8a7add389b2c82cb43f6944d7e2ef7ebeb0bffd77b5b635361a8a46087` |

Codex arrancó la app en el proceso `metrics-20261031T202013094-…jsonl` (línea 1, **2026-10-31T20:20:13.1061270Z**): build **0.1.0+2ef4807 dev**, P0 **dd122831f592**, pending=0. Ese proceso terminó a las 20:32:30Z sin gestos. Claude lanzó después el proceso nuevo `metrics-20261031T222118277-…jsonl` (22:21:18.2913560Z): misma build, P0 dd122831f592, pending 0 y los tres archivos iguales al respaldo.

Evidencia local ignorada en `Logs/evidence/s2-session27-tanda1-20261031/`:
- un export antes y otro después de cada bloque;
- las extracciones JSON por bloque;
- las notas de sesión `session-notes-claude.md`, con las horas Mac M_s/M_r;
- el export `final-restore`.

Los identificadores privados no se versionan.

## Comprensión previa, respuestas literales

| Pregunta literal | Respuesta literal |
|---|---|
| ¿Puedes señalar la mira que fija dónde apuntas? | «Al hacer clic en el centro puedo ver el rombo amarillo que es la mira que fija supongo.» |
| ¿Puedes señalar el anillo que indica cuándo soltar? | «El anillo que indica cuando soltar, no se a cual se refiere» |
| ¿Puedes explicar cómo cargas y cómo cancelas? | «Para cargar debo ir hacia abajo y soltar, para cancelar es volver a donde realicé la mira» |

El PO no identificó el anillo, lo que activó la parada de §4/§7. El PO decidió literalmente **«Aclarar el anillo»** (22:26:31Z). Se describió de palabra el anillo existente, sin ayudas nuevas, y se repitió la pregunta. Respuesta: **«Ah perfecto, veo el circulo celeste y el circulo que se va cerrando.»** (22:27:48Z). Comprensión confirmada **con desviación**.

Durante la comprensión el PO hizo 15 gestos de práctica no solicitados entre las 22:23:38 y las 22:27:00Z: 5 Swing, 6 Tap y 4 Rejected outside-front-wall. Quedan registrados como preparación, fuera de los bloques.

## Frontera y método

- **Inicio del bloque:** un único cierre de DEV hecho por el PO dentro del proceso (`dev-control`, `dev-toggle`, `stage=result`, `devOpen=false`).
- **Recepciones contadas:** la recepción 1 es el primer `LaunchId` posterior al cierre; se cuentan 10 recepciones (6 en FAM). El vuelo de preparación que estaba en curso al cerrar DEV queda excluido.
- **Comprobaciones de cada ventana:**
  - el hash efectivo de todas las líneas coincide con el literal;
  - también coinciden los hashes de lanzamiento, gesto, orden y shot;
  - pending=0 en todos los registros;
  - no hay ningún `dev-control` dentro de la ventana.
- **Horas Mac:** M_s/M_r son secundarias.
- **Extracción:** script local de solo lectura, fuera del repositorio, que vincula los intentos por `attemptId`.

## Resultados por bloque

Métricas descriptivas, sin umbrales:
- **Contactos:** shots finales con `Applied=true`.
- **P:** potencia por Swing (percentil nearest-rank).
- **Aim→C:** distancia sobre el frontis entre el punto de Began y la referencia verbal C (3,2; 2,2) m. C no tiene marcador en pantalla.

| Bloque | Condición | H esperado = observado | Frontera (archivo/línea, UTC) | Terminales | Timing de Swing | Contactos | P p50 | Aim→C p50 | Extras fuera de bloque |
|---|---|---|---|---|---|---:|---:|---:|---|
| 01 FAM | SL1/V | `dd122831f592` | 222118277 l.1146, 22:30:08.807Z | 1 omisión, 4 Swing, 1 C-A | 2 Late, 2 Air | 0 | — | — | 1 post |
| 02 | SL1/V | `dd122831f592` | 143555909 l.142, 14:38:14.160Z | 5 Swing, 5 omisiones | 1 Valid, 4 Air | 1 | 0,808 | 2,100 m | 30 post |
| 03b | SL1/T | `2505640b1d88` | 143555909 l.1656, 15:00:29.569Z | 10 Swing | 3 Perfect, 6 Valid, 1 Late | 9 | 0,413 | 0,410 m | 3 post |
| 04b | SL2/T | `b4fc81c8d4d5` | 143555909 l.2484, 15:21:06.145Z | 9 Swing, 1 InvalidAim | 3 Perfect, 4 Valid, 1 Air, 1 Late | 7 | 0,415 | 0,521 m | 2 post |
| 05 | SL2/V | `3e555176e725` | 204549929 l.329, 20:49:56.679Z | 10 Swing | 1 Perfect, 2 Valid, 5 Air, 2 Late | 3 | 0,428 | 0,576 m | 6 post |
| 06 | SL3/V | `1d47540a023e` | 204549929 l.563, 20:55:11.311Z | 10 Swing | 4 Perfect, 4 Valid, 2 Late | 8 | 0,644 | 0,349 m | 4 post |
| 07 | SL3/T | `76e313deebb2` | 204549929 l.1655, 21:11:50.349Z | 10 Swing | 3 Perfect, 4 Valid, 3 Air | 7 | 0,590 | 0,332 m | 1 post |
| 08 | SL4/T | `122f7a4b16b4` | 204549929 l.1961, 21:16:41.357Z | 10 Swing | 5 Perfect, 4 Valid, 1 Air | 9 | 0,574 | 0,296 m | 0 |
| 09 | SL4/V | `d765ec727b4b` | 204549929 l.2243, 21:20:50.436Z | 10 Swing | 1 Perfect, 4 Valid, 2 Air, 3 Late | 5 | 0,595 | 0,764 m | 1 pre, 3 post |
| 10 | SL5/V | `85f1c31ed871` | 204549929 l.2720, 21:28:25.014Z | 10 Swing | 2 Valid, 7 Air, 1 Late | 2 | 0,257 | 0,533 m | 8 post |
| 11 | SL5/T | `5fb30723ab9b` | 204549929 l.3192, 21:37:49.496Z | 10 Swing | 1 Perfect, 9 Valid | 10 | 0,304 | 0,480 m | 14 post |
| 12 | SL6/T | `3b1ed25fbab0` | 204549929 l.3450, 21:48:39.507Z | 8 Swing, 1 Rejected, 1 InvalidAim | 1 Perfect, 6 Valid, 1 Air | 7 | 0,364 | 0,494 m | 13 post |
| 13 | SL6/V | `18bbcbe64304` | 204549929 l.3917, 21:53:35.020Z | 9 Swing, 1 Rejected | 5 Valid, 4 Air | 5 | 0,510 | 0,874 m | 12 post |

El detalle por recepción (horas, deltas de timing, P individuales y hashes por registro) está en las extracciones `b<NN>.json`/`.txt` y en las notas de sesión de la evidencia local. En la columna de frontera, el número de proceso abrevia el nombre de archivo `metrics-<prefijo>-…jsonl`.

### Parciales y bloques adicionales (conservados, sin sustituir)

**03, intento 1 (interrumpido).**
- La app pasó a segundo plano entre la preparación y el bloque: iPhone bloqueado de 14:51:56 a 14:54:13Z.
- Al desbloquear, el PO alternó DEV varias veces: tres cierres (l.1109, 1117 y 1317) con reaperturas entre ellos.
- Hubo 2 Swing (L149 y L150). Luego el PO se detuvo para preguntar, literal: «revisa de nuevo, no veo el anillo al centro o bien yo tengo que apuntar donde quiera?».
- La frontera es ambigua y DEV estuvo abierto dentro del tramo.
- Decisión literal del PO: **«Sí, repetir 03»**, que dio lugar a 03b. El PO empezó 03b (frontera l.1656) antes de enviar esa decisión. La frontera de 03b es única e inequívoca.

**04, intento 1 (frontera ambigua).**
- Hubo dos cierres completados (l.2351 y l.2362) con un lanzamiento entre ellos, por un triple toque sobre DEV.
- Se registraron ambos candidatos sin elegir ninguno:

  | Candidato | Contactos | Nota |
  |---|---:|---|
  | l.2351 | 8 | DEV abierto dentro de R1 |
  | l.2362 | 9 | — |

- El PO repitió por iniciativa propia, literal: **«Hice la prueba de nuevo, para que revises»**. Esa repetición es 04b y su frontera es única.

### Respuestas literales por bloque

Las cinco preguntas se hicieron por separado al cerrar cada bloque. En 02–13 el PO no identificó por número de recepción ninguna excepción de la triada, así que no se reetiquetó ninguna recepción. Las respuestas globales no se convierten en OK individuales: las recepciones sin respuesta propia quedan como «sin respuesta».

**01 FAM.** No hubo triada por intento: R1 es una omisión y R2–R6 quedan «sin respuesta».
- Literal al terminar el bloque: «Creo que termine, intente hacer lo que me dijiste. Y creo que salió ok, revisa los logs y vamos avanzando».
- Literal sobre la tarea: «cuando salia la pelota, presionaba el muro para que aparezca el rombo y le metía fuerza dependiendo si era golpe fuerte medio o sualve, y soltaba antes que caiga la pelota o bien intentando en el momento. Eso fueron mis intentos».
- La secuencia pedida no se siguió en orden: sin carga, cargar/cancelar, suave/medio/fuerte, corregir. Desviación conservada, sin repetir.

| Bloque | 1. Dificultad/excepciones | 2. Tarea | 3. Dirección | 4. Timing | 5. Salto |
|---|---|---|---|---|---|
| 02 | «Las 10 recepciones fueron OK cuando salieron, había algunas que era no fijo. pero no recuerdo exactamente cual.» | «Si, intenté seguir la intensidad, aunque a veces salia mas fuerte.» | «La pelota si fue donde apunto» | «El timing creo que está bien» | «El salto lo veo aceptable» |
| 03b | «No hubo dificultades la verdad.» | «Segui la tarea, osea apuntaba al centro y ponia potencia media.» | «La pelota si iba donde apunte» | «El timing hay que entrenarlo pero se ve bien» | «El salto está bien» |
| 04b | «No hubo dificultad, fue buena prueba para juego» | «Segui la tarea e intensidad, se veía bien» | «la pelota si fue donde apunte» | «Buen timing» | «Buen salto» |
| 05 | «Me costó mas ahora hacer los golpes, varios salieron al aire. Me cosó un poco mas el timing.» | «Trate de seguir la tarea, pero como te decía me costo un poco mas.» | «Pelota cuando salió, correcto» | «Timing, fue mas dificil que los anteriores.» | «Salto aceptable» |
| 06 | «No hubo ninguna dificultad, ya agarré mas el golpe por lo que sale correcto.» | «Seguí la tarea» | «La pelota iba donde apuntaba» | «Buen timing» | «Buen salto» |
| 07 | «Todas fueron correctas. Solo que yo falle algunas.» | «Segui la tarea» | «La pelota cuando lo lograba iba donde apunte» | «Buen timing» | «Buen salto» |
| 08 | «Salieron bien dentro de lo que intente.» | «Segui la tarea.» | «La pelota fue donde apunto» | «Buen Timing.» | «Buen salto» |
| 09 | «Estsa ves fue mas dificil. Pero lo logré.» | «Segui la tarea» | «La pelota fue donde apunte» | «Buen timing» | «Buen salto» |
| 10 | «Esta vez me costo mas realizar la tarea, ya que me salieron varias pelotas al aire.» | «Segui la tarea» | «Cuando lo logré la pelota no fue donde apunte, se iba mucho al muro de al lado. Debería haberse ido al fondo de la cancha» | «El timing mas dificl» | «Buen salto» |
| 11 | «No, fue mas facil.» | «Apunte con potencia media» | «La pelota iba muy lejos, o muy arriba, lo puedes ver, siendo que no debiese ser así» | «Senti mejor el timing» | «Buen salto» |
| 12 | «Estar adelante tiene mas dificultad . Pero lo logré» | «Segui la tarea apuntando a C» | «La pelota no iba donde apuntaba, que es lo mismo que vimos antes, eso se puede ver después» | «Buen timing y buen salto» (respuesta conjunta a 4 y 5) | (ver 4) |
| 13 | «Algunas mas dificiles que otras, pero se logró.» | «Segui la tarea apuntando a C a con potencia media» | «Idem de los anteriores» | «Buen timing» | «Buen salto» |

Comentarios espontáneos conservados:
- al terminar 02: «Me costo entenderlo para que salga la pelota, pero lo logré. Ahí reevisa porque hubo varios intentos»;
- al terminar 03b: «No conté la verdad, por eso cuando pensé quie llevaba 10 paré, a si los 13 fueron intentos»;
- en el bloque 10: «Hice varios golpes intentando lograrlo, fueron mas de 10».

**Juicio del PO sobre el salto (requisito de cierre de la predeclaración):** en los 12 bloques medidos, la respuesta 5 es «aceptable», «está bien» o «Buen salto». Es una respuesta literal por bloque, no un Pass.

## Observación del PO sobre la trayectoria en SL5/SL6 (para Claude/PO)

En los cuatro bloques con el cuerpo adelante (SL5/SL6, human x=3,2, z=1,6), la respuesta 3 pasa de positiva a negativa (bloques 10–13; literales en la tabla).
- Tras el bloque 11 se escaló al PO por la regla de §8 (posible defecto distinto de las observaciones conocidas). Decisión literal del PO: **«Seguir con 12»**.
- En el bloque 12 el PO dejó explícitamente la revisión para después.

Cotejo descriptivo con los registros `shot`. No es una medición de la trayectoria completa.

| Bloque | Contactos | Distancia de contacto al frontis (z) | Elevación inicial | Velocidad |
|---|---:|---|---|---|
| 10 | 2 | 1,10–1,13 m | 50–59° | 10,3–12,1 m/s; vx +4,1…+5,2 m/s, hacia LeftWall |
| 11 | 10 | 1,02–1,31 m | 49–68° | 9,9–17,2 m/s |
| 12 | 7 | 0,98–1,27 m | 51–67° | vx −1,9…−8,3 m/s, hacia RightWall |
| 13 | 5 | 1,10–1,16 m | 59–65° | 15,0–19,1 m/s |

Según el registro, la pelota llega a la zona apuntada del frontis todavía subiendo y con poca componente de velocidad hacia el frontis. El registro no contiene el recorrido posterior al frontis. Una salida alta, o una llegada al muro lateral antes del fondo, es compatible con esa geometría, pero no está medida.

**No se declara defecto ni se ajusta nada.** Queda para Claude/PO decidir qué se espera de un golpe dado a 1 m del frontis. Las respuestas 3 de los bloques 02–09 (cuerpo atrás o al centro, SL1–SL4) fueron positivas.

## Desviaciones e incidencias

1. **Comprensión.** El anillo no se identificó de entrada; hubo una aclaración verbal aprobada por el PO («Aclarar el anillo»). Antes de FAM hubo 15 gestos de práctica no solicitados.
2. **Orden de FAM.** La tarea por intento no se siguió en el orden pedido, y no hubo triada por intento.
3. **Extras fuera de bloque.**
   - En todos los bloques salvo el 08, el PO siguió golpeando después de la 10.ª recepción (columna «Extras»).
   - En el 09 hubo además un Swing sobre el vuelo de preparación que estaba en curso al cerrar DEV (L392).
   - Todos se conservan y ninguno sustituye recepciones.
   - Los 30 extras tras el bloque 02 pueden haber influido en sus respuestas 3–5, sin que se puedan separar.
4. **Parciales.** 03 intento 1 interrumpido y 04 intento 1 con frontera ambigua y ambos candidatos registrados (ver «Parciales y bloques adicionales»).
5. **Incidente de preparación del bloque 03 (Claude).** Un clic con inercia de desplazamiento cayó en el deslizador `timing.assistMs`: pasó de 40 a 50, con hash `5e19cffcbf7c`, y se produjo un lanzamiento de preparación. Se revirtió a 40 y se verificó `dd122831f592` antes de continuar. Desde entonces solo se usaron las flechas y la pista de la barra de desplazamiento.
6. **Segundo plano y procesos.**
   - **Suspensiones entre bloques:** el iPhone quedó bloqueado entre bloques. Claude tocó el overlay «Toca para continuar», lo que no genera gesto.
   - **Pausa larga entre 04b y 05:** el proceso 143555909 terminó a las 15:23:57Z. El sistema o el PO reabrió la app en el proceso 204549929, con P0/dd122831f592/pending 0 y sin gestos.
   - **Aperturas incidentales del PO** fuera de bloque: procesos 230256199 y 143423637.
   - **Relanzamientos CLI de Claude:** 222118277 el día 31 y 143555909 el día 1. Este último llegó tras un intento rechazado por bloqueo, que no cuenta.
   - En cada segmento nuevo se repitió el precheck completo.
7. **Vía de preparación.**
   - En los bloques 04–09 se preparó por la vía directa desde el estado verificado anterior, en lugar de B→SL1→SL. La equivalencia se comprobó por igualdad literal del hash.
   - **En el bloque 10, la vía directa SL4→SL5 dio `85d4e7a26d1d`, distinto del esperado `85f1c31ed871`.** La causa es que `PracticeMode` compara en float y, desde SL4, deja human.x=3.2000000476837158, tal como describe el anexo.
   - No se midió nada ni hubo gestos, y el esperado no se cambió.
   - Se rehízo la preparación por la vía prescrita: Cargar P0 sin guardar → `dd122831f592` → SL1 → SL5 → `85f1c31ed871`. El `preset-load` reescribió last-active.txt con bytes idénticos.
   - Los bloques 11–13 parten de ese estado preparado por la vía prescrita.
8. **Conectividad.** Hubo varios `copy`/`launch` fallidos por la conexión CoreDevice (4016/4000) o por el bloqueo del iPhone; no cuentan y se repitieron. Tras el bloque 10, Duplicación mostró «Conexión interrumpida», que se resolvió cuando el PO bloqueó el iPhone.
9. **S2-TIME-01.** En SL5/SL6 con V predominan los Air tempranos (bloque 10: 7 de 10). Se registra como observación conocida y no motivó ajustes.

## Cierre y restauración

- **Export final del proceso de bloques:** `after-13`, hecho antes de reabrir la app. El proceso 204549929 terminó con 8 líneas `frames` (última a las 21:56:00.585Z, hash `18bbcbe64304`) y sin gestos después del bloque.
- **Proceso nuevo:** el relanzamiento CLI con `--terminate-existing` fue aceptado. Archivo `metrics-20261101T215611331-…jsonl`, primera línea a las **2026-11-01T21:56:11.3538050Z**:
  - build 0.1.0+2ef4807 dev;
  - **P0-calib-1**, presetFileHash `2f1295f2…`;
  - **`dd122831f592`**, **pending 0**.

  Sus 20 líneas son todas P0/dd122831f592/pending 0, sin gestos.
- **Archivos tras restaurar:** el export `final-restore` muestra P0-calib-1.json, Po-1.json y last-active.txt **idénticos byte a byte (`cmp`) al respaldo `before-session`**, con los tamaños y SHA-256 de la tabla inicial.
- **Sin cambios de producto:** ningún preset guardado. No hubo código, tests, builds, instalaciones, defaults, SL, spec, plan, criterios, bandas ni umbrales nuevos.

## Limitaciones

- **Independencia.** Claude operó la tanda: preparaciones, prechecks, extracciones y este informe. Por eso su revisión posterior no es independiente. Se recomienda que la revisión de cierre la haga otro revisor, o que el PO la contraste con la evidencia local.
- **Script de extracción.** Es una herramienta local, no versionada ni probada por TDD. La ruta de contacto (`Applied=true`) se ejerció con contactos reales por primera vez en 02/03b. Sus conteos se cotejaron a mano en varios bloques, no en todos.
- **Lo que no se mide.**
  - La trayectoria después del frontis no está en `shot`.
  - Aim→C es una referencia verbal sin marcador en pantalla.
  - Los extras posteriores a cada bloque impiden separar aprendizaje y respuestas en algunos bloques.
  - Las respuestas globales no permiten asignar excepciones a recepciones concretas.
- **Pendiente de juicio.**
  - La observación de trayectoria en SL5/SL6.
  - La potencia baja en SL5 (bloque 10, p50 0,257) frente a la potencia media pedida.
  - Los efectos de orden y de aprendizaje.

PR #30 sigue abierta y sin fusionar hasta el juicio del PO. Siguen sin autorizar las tandas 2–4, PROD, S3–S4 y T8.
