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
