# M2-4 — implementación experimental y sesión

Base403d213, rama firstshot/s1-m2-4, autorización PO2026-10-28. [Diseño aprobado](2026-10-28-first-shot-s1-m2-4-design.md); [cierre PR27 y autorización](2026-10-25-first-shot-po-approvals.md). S1 sin aceptar/S2–S4/T8 no autorizados.

## Preparación 1.2c-0

Preparación declarada, no RED. Selector propuesto2 [EXPERIMENTO], sin claves nuevas ni valores de defaults nuevos; A′=0 y sling=1 conservados. Fixtures sintéticos de cámara/proyección y muestras usan parámetros heredados2/35mm y N2 3mm/250ms. Los casos negativos N01–N13/N04b se escribirán antes de cada implementación: mapeo, potencia/cierres, contexto/tiempos, métricas/cadenas/O2, vista/protocolo. Ninguna traza anterior se reproduce ni representa intención M2-4. Riesgo accidental/deslizamiento no se filtra ni se declara resuelto.

Pruebas de proyección usan cámara conocida y matriz capturada al procesar Began, sin cambiar cámara del producto. Valores de posiciones/tiempos sintéticos son fixtures, no parámetros experimentales ni mediciones. Evidencia XML/log RED/GREEN se conserva en Logs/evidence/m24-tdd, con resúmenes versionados al cerrar.

## 1.2c-1 — proyección absoluta

RED compilable 1/8→GREEN8/8; mapeo de rayo con matrices capturadas, AX-1, tin/out mapeable inválido y rechazos con motivo. Primer intento de compilación del fixture faltaba un using: no cuenta como RED, log conservado. El fixture singular usó finalmente un viewport explícito: Camera.pixelRect dependía del tamaño headless; no cambió ningún criterio de producto. Selector2 disponible, A′0/defaults85/hash45c4e9af5c8f sin cambios de valores ni claves.

## 1.2c-2 — potencia y terminales

RED0/15 por aserción→GREEN15/15. Dos correcciones del fixture antes del GREEN: invocar explícitamente el constructor opcional de SlingSettings (un struct sin argumentos queda a cero), y matriz ortográfica con eje correcto hacia el frontis. Sin cambios de parámetros de producto. Distancia actual2/35/N2estricto/C-A/invalid prioritario; N04 y N04b conservan posibilidad de Swing no buscado, sin filtro ficticio. No hay impulso de pelota.

## 1.2c-3 — tiempo y contacto

RED16/21→GREEN21/21: Ended idéntico conserva P/r y cierra una vez; tiempo contradictorio/no finito conservador sin Swing, cancelación de contexto anterior a invalidez y sin r inventado, secundarios/duplicados sin reanclar. Integración con contexto real se verificará también en PlayMode de la siguiente etapa.

## 1.2c-4 — integración, cadenas y O2

RED PlayMode0/5→GREEN5/5. Snapshot al Began/instante observado/matrices crudas/O2, resultado y error separados, nuevo id por reapoyo/contexto rompe cadena; pausa/foco/DEV/relanzamiento/config invalidan sin r. Host.Current y Prediction idénticos en el test de integración. Compilación inicial del fixture necesitó calificar UnityEngine.Camera por choque con namespace Runtime.Camera: no cuenta como RED. Muestras originales y recepción conservadas, sin muestras fabricadas. No se reproduce evidencia histórica.

## 1.2c-5 — vista/protocolo

RED6/8 más geometría0/1→GREEN9/9. B0/reloj/guía C-A, B1 sin carga con Tap/abandono esperados, tres respuestas con discriminador y premature=null en fallo, terminal visible, tin/out rojo sin flash, protección SL04 para control2. Geometría nativa1206×2622 confirma botones/guía/status fuera del frontis; vista previa sin ayuda adicional. S1-sling conserva texto/instrumento bool históricos.

## Fronteras adicionales de 1.2c-3/4

RED7/11→GREEN11/11: foco sin contacto rompe vínculo, viewport invalida también durante despacho (antes de Tick), eventos UI filtrados y secundarios auditados sin crear intentos. Se conserva un único input. Timestamp recibido capturado al entrar; proyección y fijación tienen sus instantes observados separados, O2 usa ambos tiempos de cada muestra real. La comparación del timestamp sintético del fixture usa tolerancia1e-9 por representación binaria del Input System; ningún umbral temporal de producto cambia. Se conserva el XML RED, que también mostraba ese redondeo.

## Comparabilidad del contacto invalidado

RED0/1→GREEN1/1: un cambio de configuración durante un intento armado conserva el registro y marca comparableToArmedBlock=false; no reinterpreta con parámetros nuevos ni sustituye el intento. Los cierres por sistema se conservan sin Swing.

## Regresión y preparación para release

Fuente **ae1b423**: regresión fresca **441/441 EditMode**, **207/207 PlayMode**, sin errores CS ni tests omitidos. [Resumen de ejecuciones y hashes de XML](../validation/evidence/2026-10-28-m24-tdd.json); XML/log originales ignorados en Logs/evidence/m24-tdd. Los dos cambios transitorios de PlayMode (temperatura de luz y antialiasing) se restauraron byte a byte antes de exportar. Spec, propuesta r2, plan, EffectiveConfig.cs, SlingModel.cs y defaults se cotejaron contra403d213; 183 archivos de evidencia histórica intactos.

85 claves/defaults **45c4e9af5c8f**, sin claves nuevas; selector experimental2. Preset separado **S1-M2-4 / 1c8e598b099b**; P0 extendido esperado **e458bfd5ce9e**, pendiente observar arranque de la build. 41 heredados/proyección0d39e6016bd8 y SimulationKey166a96e319c7 pasan sus pruebas originales. Parámetros2/35mm y N2 3mm/250ms inalterados; A′=0 sigue por defecto, S1-sling sin promover. N04/N04b no se declaran resueltos.

Respaldo nuevo antes de instalación: P0 SHA2562f1295f201e238d9494bee285dd3f46e5bbb0f98b09ccde90fbd268f0874e4fd; Po-1 9008c33ab89f82e669bc227a81b37ed641f69e4657ee70df6c89ff0c0edb1767; last-active5885ce8a7add389b2c82cb43f6944d7e2ef7ebeb0bffd77b5b635361a8a46087. Archivos coinciden con el registro anterior, selección P0; copia local ignorada en Logs/evidence/m24-session/backup-before-install. Instalación y sesión pendientes, no hay mediciones nuevas aún.

## Release, firma e instalación

**0f8002d dev**: check-release dev/prod completos con exports limpios y `gitDirty=false`; dev Core/Runtime/DevPanel, prod solo Core/Runtime. Xcode Release ARM64 sobre el export dev fresco, `BUILD SUCCEEDED`, Player no Development. Firma estricta verificada; bundle/dispositivo corresponden y perfil efectivamente incorporado vigente hasta **2026-11-03T18:29:44Z**, cotejado antes de instalar. [Evidencia sanitizada](../validation/evidence/2026-10-28-m24-build-install.json). No se publican identificadores ni perfil/credenciales.

Instalación completada y **P0-calib-1, Po-1 y last-active byte a byte iguales al respaldo nuevo**. El primer launch fue rechazado por iOS con el dispositivo bloqueado; no cuenta como arranque observado. Falta reautenticar Duplicación y comprobar P0/e458bfd5ce9e/cero pendientes antes de cargar el candidato temporal. No empezó B0 ni hay datos físicos M2-4. La preparación y el cierre se registrarán a continuación, sin mezclar con las111trazas de4cec622.

Contrato del registro: `releaseTimeReal` solo es r de golpe si outcome=Swing; en otros cierres es el Ended real observado, no un golpe válido. Cuando `hasRelease=false`, el cero serializado es un marcador explícito, no un Ended inventado. Error objetivo=-1 con hasAim=false o targetId=0 indica que no es evaluable, no un error medido. Matrices en orden column-major de Unity; admisión y margen inferior se refieren al área útil, no al borde físico.

## Preparación verificada y entrega de B0 al PO

Tras reanudar Duplicación, proceso nuevo0f8002d arrancó con P0/e458bfd5ce9e/cero pendientes; pantalla DEV confirma Jugando. La falta de acceso inicial quedó resuelta, sin atribuirla a un defecto del control. Se copió S1-M2-4 separado y se cargó desde Presets sin guardar. **last-active se repuso a P0 y se leyó/cotejó antes de pulsar Aplicar y relanzar**. Lanzamiento preparado **2026-10-28T23:31:48.3537350Z**, S1-M2-4/**1c8e598b099b**, cero pendientes, repeat0 por configuración exacta. P0/Po-1/last-active byte a byte con el respaldo después de preparar; Escenario/P2/H17 deshabilitados observados. DEV cerrado/Listo y B0 accesible al reabrir; ningún gesto M2-4 ni bloque antes del traspaso. Las muestras de interacción de preparación por Duplicación no son mediciones físicas del PO.

Explicación previa: apoyar directamente en el frontis fija; bajar solo carga y subir reduce P; para cancelar después de cargar, volver a la altura del apoyo o más arriba y soltar; corregir implica cancelar y apoyar de nuevo. Pelota sin golpe. Se solicita **solo B0**, PO pulsa el botón y confirma desde el Mac la cuenta atrás **antes del primer gesto**, izquierda/un pulgar/120s completos sin DEV/salir; cancelar guiado, avisar al terminar, no B1. Si no comprende C-A, detener/restaurar sin ajustar nada. No se infiere la confirmación ni el resultado hasta su respuesta y los registros. Durante el bloque no se controla ni captura el teléfono.

**Estado al enviar la instrucción:** B0 pendiente; comprensión de C-A pendiente; B1–B5 y 16 preguntas pendientes. Configuración temporal M2-4 activa, selección persistida P0. Restauración final en proceso nuevo/cotejo aún pendiente porque la sesión no ha terminado. PR28 en borrador, sin fusionar; no aceptación de S1.

## B0 recuperado — práctica completa, aclaraciones pendientes antes de B1

[Datos nuevos sanitizados, auditoría y literal completo del PO](../validation/evidence/2026-10-28-m24-B0.json). Un solo B0, armado **2026-10-28T23:39:54.6567690Z**, fin **23:41:54.6690640Z**, **120,01248675 s**, complete=true/guidedCancellationRecorded=true. Configuración **1c8e598b099b**, cero pendientes y comparable=true en los104; sin relanzamientos ni cambios de configuración durante el bloque. Ninguna activación Escenario/P2/H17 ni bloques formales posteriores.

| Cierre de práctica | Intentos conservados |
|---|---:|
| Swing preview, sin golpear pelota | 69 |
| Rejected fuera del frontis | 12 |
| AbandonedNoPull | 10 |
| CanceledReturnToDeadZone | 5 |
| InvalidAim tin/out | 5 |
| Tap | 3 |
| Total | 104 |

2853 muestras primarias reales en intentos; todos los rechazos/toques se conservan, sin sustitución. En Rejected el cierre es la resolución de admisión del Began, no la duración física completa del dedo; las muestras posteriores filtradas/ignoradas se preservan por separado. B0 no pide un objetivo T: el error del Began frente a objetivo solicitado es **NotEvaluated**, no cero ni precisión demostrada. O2: **3 muestras** anteriores a lockObservedAt entregadas después, desplazamiento máximo respecto al ancla **1,654680 mm reportados**; no se movió el objetivo ni se fabricaron muestras. **15 enlaces** después de cierre C-A; no equivalen a15 correcciones humanas declaradas. Ninguna violación de la prioridad de cierres en el auditor de datos nuevos.

Literal del PO:

> listo, apuntaba, cargaba y también cancelaba. Por 2 minutos, me pareció bien, pero para cargar había que ir con el pulgar hacia abajo o atras, me gusta mas que sea hacia arriba o adelante, ese fue mi primer movimiento natural, luego me acostumbre y fui con el pulgar hacia abajo. Pero se ve bien

Se registra como sensación/preferencia por un tensado hacia arriba/adelante; **no cambia la dirección descendente aprobada ni autoriza ajustar parámetros en esta sesión**, y no acepta S1. No inferir intención de cada intento a partir de P o del cierre. Los cinco retornos C-A y el flag guiado acreditan el comportamiento registrado, no por sí solos la comprensión/voluntariedad del PO. Se pide aclaración explícita sin repetir.

**Desviación de procedimiento:** no llegó una confirmación por chat de cuenta atrás antes del primer gesto. Se pregunta retrospectivamente si la vio antes de tocar y si apareció Completado; la respuesta posterior no se convertirá en una confirmación previa. Práctica completa en log, sin repetir ni sustituir. Pendiente la aclaración y la comprensión explícita de C-A; todavía no se solicita B1 ni se dan por explicadas sus tres respuestas. P0/Po-1/last-active iguales al respaldo;456fuentes de producto intactas. M2-4 temporal sigue activo/last-active persistidoP0; restauración de cierre aún pendiente.

### Aclaración posterior del PO y gate de B0

A la pregunta sobre cuenta atrás antes del primer gesto y Completado al final respondió literalmente: **«Si, lo vi, preferí empezar de una después de leer el mensaje»**. A la pregunta sobre cancelaciones voluntarias y comprensión del retorno C-A: **«fueron todas voluntarias»**. Se conserva la desviación de no haber confirmado antes por chat; no se convierte esta aclaración en confirmación previa ni se repite B0.

El gate de B0 queda cubierto por práctica completa/guiada registrada y comprensión/voluntariedad declaradas por el PO. No se reetiquetan los cierres individuales ni se interpreta que todos los abandonos fueron correcciones. Sigue intacta la preferencia arriba/adelante como sensación, sin cambio de dirección. **Antes de solicitar B1 se presentan tres ejemplos de respuesta**, pendientes de contestar: fijación vista aunque lejos del objetivo→OK; apoyo accidental no querido→Apoyo sin querer; no identificar/obtener la fijación esperada o fallo operativo→No fijó / falló. No se solicita todavía ningún bloque nuevo, ni se presume comprensión de la triada.

### Triada comprobada y B1 solicitado por separado

Antes de B1, el PO respondió a los tres ejemplos, cada uno por separado:

| Ejemplo presentado | Respuesta literal |
|---|---|
| Apoyo querido y rombo visto, aunque lejos del cuadrado; error de puntería separado | «OK» |
| Apoyo accidental que fijó sin querer elegir punto todavía | «Apoyo sin querer» |
| No apareció/no se identificó el rombo esperado o hubo fallo operativo | «No fijó / falló» |

Las tres coinciden con los significados predeclarados. Esto comprueba la comprensión del ejemplo, no garantiza el significado de cada respuesta futura ni reinterpreta los datos de S1-sling. Se solicita **solo B1**, con precheck del PO en DEV: S1-M2-4/1c8e598b099b/cero pendientes/Jugando; si difiere, no empezar y avisar. Diez apoyos directos al cuadrado T1–T5 dos veces con un solo pulgar izquierdo, retirar sin cargar ni arrastrar para corregir; una respuesta explícita por intento, todos los cierres conservados, sin repetir/sustituir. No B2 hasta recuperar/verificar B1. Resultado y confirmación del precheck pendientes; sin acceso al teléfono durante los gestos. Dirección descendente/parámetros/código intactos, M2-4 temporal activo/selección P0/restauración final pendiente.

## B1 — diez apoyos y diez respuestas; tarea sin carga no aislada

[Datos nuevos B1 y aclaraciones literales](../validation/evidence/2026-10-28-m24-B1.json). Armado **2026-10-29T00:34:06.8443420Z** (21:34 del28 en Santiago), fin **00:35:01.0598170Z**,54,213393417s incluidos feedbacks; una sola tanda de10, sin sustitución/extra. Hash1c8e598b099b/cero pendientes, armado aceptado **sin pausa/con foco/DEVcerrado**, todos comparables. Secuencia T1–T5 dos veces;597muestras primarias reales,10/10 con objetivo fijado y10 respuestas «OK». Esto significa fijación declarada, no precisión conseguida ni éxito sin carga.

Cierres: **2 AbandonedNoPull,3 Swing,5 CanceledReturnToDeadZone**. Ocho contactos salieron de zona muerta en algún momento; los3Swing finalizaron con P39,95–51,50%. El instrumento pide requestedPower=none/taskKind=aim-began-no-pull, por lo que **B1 no entra en bandas de golpes suaves ni en B2**. Se conservan los resultados, sin corregir potencia ni repetir hasta lograr abandono.

El PO aclara literalmente: **«intente acertar, en algunos bajaba el pulgar para cargar, retiraba sin cargar y tambien retiraba para destapar la mira y comprobarla»**. Por ello, el bloque mezcla tareas: **sin carga no quedó aislado**. La intención de cada3Swing no se deduce de la frase global; no se cuentan automáticamente como golpes no buscados. El PO sí declara buscar los objetivos, así que se informa error continuo del Began frente a T: **mín0,055710m / mediana0,296372m / máx1,027877m**, sin umbral nuevo ni Pass de precisión. Duración Began→cierre mín320,843ms/mediana1796,037ms/máx3404,718ms; distinta de duración del bloque con respuestas, no tiempo de rally.

O2=0 enB1,6enlaces temporales de reapoyo y0al mismo objetivo: el protocolo cambió deT, por lo que **no son seis correcciones acertadas ni intención inferida**. No violaciones de precedencia en auditoría de datos nuevos. Ningún experimento ni configuración cambiada duranteB1.3archivos byte iguales al respaldo y456fuentes de producto intactas.

Después del último feedback/fin de B1, **2026-10-29T00:35:03.1154830Z**, se activó `step`/Paso; el registro result muestra paused=true. No afectó los10intentos ya cerrados. PO: **«probablemente fue accidental»**, conservar como juicio probabilístico, no causa confirmada ni atribución de autoarranque. Antes del siguiente bloque se indica **Continuar**, no Paso, y verificar Jugando/hash/pendientes.

Se solicita **solo B2-soft/10suaves**, con precheckS1-M2-4/1c8e598b099b/0pendientes/Jugando tras reanudar; pulgar izquierdo, buscar realmente el cuadrado centralT3 al apoyar, tirón pequeño descendente/suelta, no arrastrar para corregir. Avance automático y todos los cierres cuentan; sin sustituir ni repetir. No medios/fuertes/B3 hasta recuperar la tanda suave. No se modifican dirección, parámetros, bandas ni criterios para corregir la desviación de B1. M2-4 temporal activo/selecciónP0 y restauración final pendientes;PR28borrador/sin fusionar/S1sin aceptar.

## 2026-10-29 — continuación tras reinicio del PO, antes de B2

PO: **«Prueba conectarlo de nuevo, para empezar la prueba. Tuve que reiniciar la app»**. Recuperación previa confirma **B2 no armado y cero intentos B2**; solo B0/104 y B1/10. P0-calib-1/e458bfd5ce9e/cero pendientes tras el reinicio es el arranque previsto por selección persistida, no prueba de pérdida del candidato ni defecto nuevo. Los tres archivos continúan byte a byte iguales al respaldo y456fuentes de producto sin cambios.

Se reconecta la misma app **0f8002d dev**, sin suites/check-release/build/instalación nuevos. Se carga S1-M2-4 existente sin guardar; last-activeP0 repuesto y leído **antes** de aplicar/relanzar. Lanzamiento preparado **2026-10-29T10:45:43.4368230Z**, hash**1c8e598b099b**, cero pendientes/repeat0/Jugando/DEVcerrado/Listo, Escenario/P2/H17 deshabilitados observados. Preset experimental coincide byte a byte con el de la rama; originalesP0/Po-1/last-active iguales. Ningún gesto ni bloque nuevo al traspasar. [Comprobación de reconexión](../validation/evidence/2026-10-29-m24-reconnect.json).

**Tramo nuevo 2026-10-29-reconnect-1:** otro día y proceso, no sesión continua con la práctica nocturna. Session/launchSerial nativos pueden reiniciarse; se distinguen por archivo/época/attemptId y segmento derivado, sin modificar crudos ni enlazar correcciones a través de reinicio. No se reemplazan B0/B1 ni se repiten. Configuración igual no acredita iguales condiciones humanas/aprendizaje/fatiga ni comparación causal entre tramos.

Se vuelve a solicitar **solo B2-soft/10suaves**, que aún no había empezado: precheck0f8002d/S1-M2-4/1c8e598b099b/0pendientes/Jugando, pulgarizquierdo, buscarT3 al apoyar/tirónpequeño descendente/suelta, todoscierres/sin sustitución ni repetir. No B0/B1/medios/fuertes/B3. Resultado pendiente, sin acceso al teléfono durante los gestos. M2-4 temporal activo/selecciónP0/restauración final pendiente; no cambios de dirección/parámetros/umbrales ni aceptaciónS1.
