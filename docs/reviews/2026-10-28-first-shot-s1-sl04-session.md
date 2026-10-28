# S1-sling D2 — sesión formal tras SL-S1-04

Build4cec622 dev, preset temporalS1-sling/**7d1a61c39766**. [B0 y gate de comprensión](2026-10-28-first-shot-s1-sl04-B0.md), [corrección/preparación](2026-10-27-first-shot-s1-sl04-controls.md). Misma PR#25 sin fusionar; S1sin aceptar, S2–S4/T8sin autorizar. Datos nuevos separados de db72a00/42265ca y del replay anterior. Ningún parámetro ni criterio modificado.

## B1 — tanda solicitada y repetición adicional

[Todos los registros B1, respuestas y auditoría](../validation/evidence/2026-10-28-s1-sl04-B1.json). El PO declaró literalmente:

> Listo. Hice 2 intentos porque en el 2do intento si intente de apuntar al cuadrado

Aclaración de cuándo siguió el objetivo:

> Desde la 2da tanda, en la primera fue desde el gesto 5

Se conservan las dos tandas completas de10 y sus20 respuestas: **sesión2 es la tanda solicitada originalmente; sesión3 es repetición adicional y no la reemplaza**. Según el PO, los intentos1–4 de sesión2 no buscaban el objetivo;5–10 sí. Toda la sesión3 lo buscaba. Esa adhesión a la tarea se declara aparte del campo técnico comparableToArmedBlock, que estrue en20/20 por configuración.

| Dato | Sesión2, original solicitada | Sesión3, adicional |
|---|---|---|
| Inicio / fin UTC | 13:55:28.064579 / 13:56:03.421768 | 13:56:09.267487 / 13:56:35.059404 |
| Intentos / respuestas | 10/10 | 10/10 |
| Secuencia de objetivos | T1–T5 dos veces | T1–T5 dos veces |
| Seguimiento del objetivo declarado | Intentos5–10 | Intentos1–10 |
| Outcomes | 10Swing | 9Swing/1AbandonedBeforeFix |
| Fijaciones | 10timer | 5timer/4sample;1sin fijación |
| Error de mira de todos los fijados, mín./med./máx. | 0,025/1,921/4,772m (n10; primeros4sin tarea seguida) | 0,044/0,223/1,319m (n9; abandono excluido, no error0) |
| Error solo donde POdeclara seguir objetivo | 0,025/0,340/2,832m (n6) | Igual a fila anterior (n9) |
| Began→fijación mediana | 746,244ms (n10) | 691,843ms (n9) |
| Último movimiento fuera de tolerancia→fijación mediana | 129,543ms | 131,018ms |
| P al soltar mín./med./máx. | 0/0/19,373% | 0/0/7,325% |
| d descendente máxima por gesto mín./med./máx. | 0/0,138/8,780mm | 0/0,828/5,190mm |
| «OK» / «Se fijó antes de tiempo» | 9/1 | 5/5 |

Son indicadores continuos descriptivos, sin umbral nuevo de error ni comparación causal entre tandas: cambian seguimiento de tarea y familiaridad. Swing es outcome del gesto; no hubo contacto real de pelota. El abandono adicional sesión3/intento6 tiene lockBlocked=true y errornull; no se elimina ni sustituye. Los963 samples de los20 gestos y los vínculos attemptId/sesión/intento con sus respuestas se preservan.

Objetivos registrados del modelo (m), ya aprobados y sin reubicarlos: T1(4,77;1,1355),T2(1,63;1,1355),T3(3,20;2,5250),T4(4,77;3,7160),T5(1,63;3,7160). Las coordenadas se interpretan con la proyección AX-1; los rótulos izquierda/derecha corresponden a pantalla. No son nuevas zonas H2 ni umbrales de aceptación.

## Significado de las respuestas y límite de interpretación

Sesión3/intento6 tiene AbandonedBeforeFix y respuesta «Se fijó antes de tiempo». Se pidió aclarar el significado sin pedir que recordara/reetiquetara ese gesto. Respuesta literal:

> También intentos fallidos

Por tanto, **las6 pulsaciones no se convierten en una tasa de6 fijaciones prematuras**. Se conserva el flagraw premature y la respuesta original, con interpretación de intención **NotEvaluated por significado mixto**. No se corrigen retrospectivamente etiquetas ni se infiere que los5 gestos fijados con esa respuesta fueron necesariamente anticipados. El registro objetivo de fijación/error/abandono permanece independiente.

Para B3 se aclarará antes de empezar: «Se fijó antes de tiempo» solo indica fijación observada que se adelantó a donde quería apuntar; los demás casos se respondenOK para avanzar, sin convertir un fallo o abandono en acierto. No se añade botón ni se modifica código. B2 no usa ese feedback.

## O1/O2 en B1

Mismo auditor descriptivo de muestras/soporte y geometría documentado enB0, sin fijaciones alternativas ni grid de frames. O1 aparece en **un abandono, sesión3/intento6**: soporte2 y267,211ms hasta siguiente recepción, punto ya fuera del área útil; **cero ventanas O1 con geometría elegible**. No se aísla el mínimo3 como causa.

O2: **3 muestrasMoved**, sesiones/intentoss2/1,s2/8,s3/10, todas tras fijacióntimer;desplazamiento máximo **0,174605mm** desde el punto fijado. Son timestamps anteriores a lockObservedAt recibidos después, no prueba de intención o defecto. B0+B1 suman8 muestras de ese tipo (7timer/1sample), sin mezclar con las fuentes históricas.

## Configuración y siguiente bloque

Todos los armados/gestos/respuestas de B1 están en7d1a61c39766/cero pendientes. Los dos armados fueron aceptados con pausafalse/focusedtrue yDEVcerrado antes dearmar. No hubo cambio deconfig ni P2/H17/Escenario durante esas tandas; archivosP0/Po-1/last-active directamente iguales al respaldo.444fuentes sin cambios; sin código/suites/build/instalación nuevos.

Siguiente B2 según autorización: **T3,10 suaves→10 medios→10 fuertes**, mano izquierda, sin ajustes ni sustitución. Antes de cada tanda elPO verificaDEV S1-sling/7d1a61c39766/cero pendientes/Jugando; si no coincide, no iniciar yavisar. Los tres botones B2 son series separadas; mantener todos los intentos aunque fallen o cancelen. NoB3 hasta recuperar/verificar los30 y dar su instrucción. Configuración temporalS1activa yrestauraciónP0pendiente al terminar/detener, con nuevo arranque/hash/cero pendientes/cotejo3archivos. PRsin fusionar/S1sin aceptar/S2–S4/T8sin autorizar.

## B2 — tres intensidades, todos los intentos conservados

Respuesta literal del PO:

> Listo, hice las 3 tandas

[Evidencia B2 completa, controles y auditoría](../validation/evidence/2026-10-28-s1-sl04-B2.json). Sesiones4/5/6, diez intentos cada una, T3 registrado en todos. Orden suave→medio→fuerte, sin repetición ni sustitución. Izquierda solicitada y previamente declarada dominante; no observación independiente de la mano. Tres bloques completos,3777 muestras reales,29Swing/1AbandonedBeforeFix. Armados aceptados en7d1a61c39766, repeat0/cero pendientes, pausafalse/focusedtrue/DEVcerrado. Todos los gestos comparables por configuración; no cambios ni P2/H17/Escenario durante las series.

| Dato | Suaves, sesión4 | Medios, sesión5 | Fuertes, sesión6 |
|---|---|---|---|
| Inicio / fin UTC | 14:20:35.506481 / 14:21:10.865623 | 14:21:15.967744 / 14:22:03.771534 | 14:22:10.307531 / 14:22:54.160753 |
| Swing / abandono | 10/0 | 9/1 | 10/0 |
| Fijaciones timer / sample | 9/1 | 7/2 | 7/3 |
| Banda solicitada con Swing / todos los intentos | 5/10 | 9/10 | 9/10 |
| Bandas registradas, sin reinterpretarlas | 5soft/5medium | 1soft/9medium | 1soft/9strong |
| P al soltar mín./med./máx. | 0,800/25,480/41,125% | 0/50,244/59,364% | 0/82,957/100% |
| Dispersión P, desviación estándar poblacional | 10,787 puntos porcentuales | 16,272 puntos porcentuales | 27,162 puntos porcentuales |
| d al soltar mín./med./máx. | 2,264/10,408/15,571mm | 0/18,581/21,590mm | 0,607/29,376/37,382mm |
| Dispersión d, desviación estándar poblacional | 3,560mm | 5,946mm | 9,579mm |
| Saturados P=100% al soltar | 0/10 (0%) | 0/10 (0%) | 1/10 (10%) |
| Margen mínimo registrado respecto a yMin del área útil, mín./med./máx. entre fijados | −11,705/−1,159/4,584mm (n10) | −17,834/−14,411/−7,233mm (n9) | −29,430/−24,102/17,118mm (n10) |
| Error al objetivo entre fijados, mín./med./máx. | 1,416/2,407/2,743m (n10) | 1,234/2,565/3,189m (n9) | 0,921/2,354/3,289m (n10) |

P y d incluyen los diez intentos de cada serie, también los ceros; los errores y márgenesnull del abandono no se convierten en0. Las bandas son las existentes del registro nativo (soft P<0,25;medium0,25≤P<0,60;strongP≥0,60), citadas desde [GesturePreview.Sling](../../Assets/SquashClash/Runtime/Shot/GesturePreview.Sling.cs); no se cambian. Saturación al soltar se distingue de alcanzar el tope antes: ninguna serie supera la advertencia existente>20%. Los mm usan460dpi reportados, **physicallyVerified=false**: no nueva comprobación con regla ni medida física calibrada.

**El margen es respecto al límite inferior del área útil**, calculado como(y−slingArea.yMin)·mmPerPx, no distancia al borde físico del iPhone ni a su gesto de inicio. Valores negativos indican que, después de fijar, el dedo descendió más allá de ese límite; no prueban por sí mismos un Canceled del sistema. No hubo SystemCanceled enB2. La fijación exige área útil; el tensado posterior puede continuar fuera de ella según el comportamiento existente, sin alterarlo.

Medios/intento8 se conserva como AbandonedBeforeFix/P0/lockBlocked=true. O1: candidato con2muestras,680,030ms hasta siguiente recepción, punto(722,591) ya fuera del área útil; cero casosO1 con geometría elegible. No se atribuye el abandono únicamente al mínimo3. Fuertes/intento3 fijó por timer tras6831,346ms y terminó con d=0,607mm, por debajo de la zona muerta2mm: Swing/P0/reason=distance/conservative=false. No es un fallo de repetición de timestamp ni una salida conservadora; intención del PO no inferida. Ninguna salida conservative=true en las tres series.

O2: una muestraMoved en sesión6/intento8 con timeReal anterior a lockObservedAt recibida después, fijacióntimer; desplazamiento0,110435mm desde el punto fijado. Mismo auditor descriptivo deB0/B1, sin eventos inventados ni replay con parámetros alternativos.

Aclaración literal del PO sobre el objetivo deB2:

> no, no intente aputnar al cuadrado cenral, apuntabaa simulando golpes

**Desviación declarada en las tres tandas:** el PO simulaba golpes sin buscarT3. Los errores respecto aT3 se conservan como distancias registradas, **no como precisión al intentar acertar el cuadrado**. El campo raw comparableToArmedBlock sigue siendo true por configuración; la adhesión a la tarea se declara false por separado. Ninguna serie se elimina, sustituye ni repite. Las potencias observadas y su separación se conservan, pero no acreditan el cumplimiento conjunto de apuntado aT3 y potencia ni aceptación voluntaria de las tres intensidades. Sin ajuste de escala, umbrales o criterios.

Tres archivosP0/Po-1/last-active directamente byte a byte iguales al respaldo y444fuentes cotejadas sin cambios. Sin código/suites/build/instalación nuevos. S1temporal activo/restauraciónP0pendiente al cerrar. SiguienteB3: diez medios aT1/T2alternados, izquierda, hash/pendientes/Jugando antes dearmar; aclarar antes que«Se fijó antes de tiempo» solo indica fijación anticipada observada, no fallo genérico. NoB4 todavía;PRsin fusionar/S1sin aceptar/S2–S4/T8sin autorizar.


## Instrucción B3 tras aclaración de B2

Se solicitó soloB3: izquierda, comprobar antes DEV S1-sling/7d1a61c39766/cero pendientes/Jugando; si no coincide, no iniciar yavisar. Diez medios buscando esta vez el cuadrado bajo T1/T2alternados, apuntar→dejar fijar→tensar. Se aclaró explícitamente antes del bloque: «Se fijó antes de tiempo» exclusivamente si observó fijación antes de llegar donde quería; cualquier otro resultado, también un fallo sin fijación, se respondeOK para avanzar, **sin convertirlo en acierto**. Conservar diez respuestas y todos los intentos, sin sustitución ni repetición; sin parámetros nuevos ni B4todavía. B3pendiente de devolución/captura; no tocar teléfono durante el bloque.
