# S1-sling D2 — sesión formal tras SL-S1-04

Build4cec622 dev, preset temporalS1-sling/**7d1a61c39766**. [B0 y gate de comprensión](2026-10-28-first-shot-s1-sl04-B0.md), [corrección/preparación](2026-10-27-first-shot-s1-sl04-controls.md). Misma PR#25 sin fusionar; S1sin aceptar, S2–S4/T8sin autorizar. Datos nuevos separados de db72a00/42265ca y del replay anterior. Ningún parámetro ni criterio modificado.


**Entrega física cerrada para revisión, sin aceptación de S1.** Se conservan65 intentos formales previstos,10 adicionales deB1,31 de prácticaB0 y5 fuera de bloque:111 gestos/10754 muestras reales, separados. Los16 juicios literales y la restauración finalP0 están al final. B2 no buscóT3 y el feedbackB1/B3 tiene significado mixto; no afirmar cumplimiento íntegro del protocolo ni tasa medida de fijación involuntaria. Potencia y cancelación recibieron respuestas favorables; apuntado bajo, interferencia de la zona y fluidez siguen señalados por elPO. Ningún ajuste ni nueva sesión tras esta entrega.

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


## B3, B4 y B5 — resultados y desviaciones conservadas

[Evidencia nueva completa](../validation/evidence/2026-10-28-s1-sl04-B3-B4-B5.json). Respuesta literal del PO:

> Listo hice B3 - B4 y B5.
> B3: A veces no aparecía el rombo amarillo para tensar, lo dejé como "Fijo antes de tiempo" por lo que ahí están los errores tanto como el que fijo antes de tiempo y el que te mencione.
>
> B4: Sin problemas
>
> B5: Pasaba algo similar a b3. Pero se logró hacer el test

El PO continuóB4/B5 antes de la instrucción separada y la revisión de registrosB3. Se preserva esa desviación de secuencia; los bloques estaban dentro de la autorización generalB0–B5. No hubo una nueva explicación específica por Codex antes deB4/B5: no se reconstruye retrospectivamente como realizada. Las secuencias de objetivos/intensidades de la app están registradas; no son prueba independiente de intención, mano usada ni de haber buscado cada objetivo a ritmo cómodo. No repetir ni sustituir.

Los25 intentos y2192 muestras están en7d1a61c39766/cero pendientes/comparableToArmedBlocktrue. Tres armados auditados aceptados con pausafalse/focusedtrue/DEVcerrado; repeat0. Sin cambios de configuración ni P2/H17/Escenario durante los bloques. Ninguna salida conservative=true ni SystemCanceled.

| Dato | B3, sesión7 | B4, sesión8 | B5, sesión9 |
|---|---|---|---|
| Inicio / fin UTC | 15:10:11.642258 / 15:10:53.753861 | 15:10:58.571584 / 15:11:10.119165 | 15:11:14.137267 / 15:11:53.710471 |
| Intentos | 10 | 5 | 10 |
| Outcome | 6Swing/3AbandonedBeforeFix/1ZoneCanceled | 5ZoneCanceled | 8Swing/2AbandonedBeforeFix |
| Fijaciones timer / sample / sin fijación | 6/1/3 | 4/1/0 | 7/1/2 |
| Banda solicitada con Swing | 0/10 medios | No aplica, cancelación | 2/10 según secuencia solicitada |
| P al soltar mín./med./máx., todos los intentos | 0/0/20,711% | 0/0/0% | 0/53,758/73,084% |
| Error a objetivo entre fijados mín./med./máx. | 0,059/0,082/2,736m (n7) | 0,206/0,342/0,742m (n5; objetivo no era criterio de cancelación) | 0,028/0,320/2,307m (n8) |
| Began→fijación mediana | 733,941ms (n7) | 383,398ms (n5) | 522,541ms (n8) |
| Último movimiento fuera de tolerancia→fijación mediana | 130,446ms | 129,713ms | 131,326ms |
| Duración Began→Ended mediana/p90/máx., todos | 1,438/4,455/4,947s | 1,538/3,255/3,255s | 1,896/4,297/7,230s |
| Margen mínimo respecto a yMin del área útil mín./med./máx., fijados | −0,220/0,056/6,020mm | 4,529/8,615/11,817mm | −17,337/−12,948/−6,680mm |

B3 alternóT1/T2 diez veces con potencia media solicitada. Tiene10 respuestas:5OK/5«Se fijó antes de tiempo». Las tres ausencias de fijación (intentos4/6/10) llevan esa segunda respuesta. **Aunque se aclaró antes el significado, el PO declara de nuevo uso mixto: no hay tasa medida de fijaciones prematuras enB3.** Se mantienen flags/respuestasraw, sin reasignar los otros dos intentos a intención confirmada. El ZoneCanceled deB3 no se declara accidental por inferencia: el PO responde después «no» a cancelaciones accidentales; conservar ambos hechos sin inventar intención por intento.

B4 registra5cancelaciones por zona y el PO declara «Sin problemas». No es aceptación deS1. B5 conservaT1–T5 dos veces y suave/media/fuerte/suave/media/fuerte/suave/media/fuerte/suave. Solo intentos8/9 combinaronSwing con banda solicitada; no convertir las ocho fijaciones en ocho aciertos conjuntos de objetivo/potencia. El PO reporta dificultad semejante aB3; se conserva sin atribuir intención a cada descenso posterior a fijar.

Los cinco abandonos deB3/B5 tienen lockBlocked=true y sus últimas muestras están por debajo de yMin=690,984px. Coinciden con la condición de área útil que impide fijar; esto **no demuestra una causa única para todo el gesto**, ni un fallo nuevo deD2, ni permite redefinir el área. Ejemplos finales: B3/intentos4/6/10 terminan eny≈630/612/619px; B5/intentos6/7 eny≈577/530px. La dificultad del drop ya fue señalada enB0. Se entrega como limitación observada de interacción paraPO/Claude; no se modifica área, modelo, mínimo de muestras ni parámetros y no se ensaya otro ajuste.

O1: soloB5/intento7 tiene ventanas de contacto quieto≥120ms con soporte1/2; punto(536,515–516px) ya fuera del área útil. **Cero casos con geometría elegible**, sin aislar el mínimo3 como causa. O2: dosMoved, B3/intento3 yB4/intento4, con timestamp anterior recibidas después de fijar, ambas origen sample; desplazamientos0,055217/0,110425mm. Auditoría descriptiva con muestras reales, sin parámetros ni outcomes alternativos.

## Inventario completo, incluidos gestos fuera de bloque

[Inventario, auditoría de los111 y restauración](../validation/evidence/2026-10-28-s1-sl04-session-closure.json). No se mezclan fuentes históricas db72a00/42265ca ni replay anterior:

- B0 práctica:31intentos/3413muestras.
- Formales originalmente previstos:65intentos. Se conservan además los10deB1 repetidos a iniciativa delPO; total formal ampliado75/6932muestras, sin sustituir la tanda original.
- Fuera de bloque, separados:5preview/409muestras. Uno trasB2 a14:22:55Z (abandono); cuatro trasB5 a15:11:57–15:13:02Z (3Swing/1ZoneCanceled). No se incorporan a denominadores formales ni se pierden por filtrar bloques.
- Total111intentos/10754muestras:88Swing/11ZoneCanceled/12AbandonedBeforeFix. Formales75:62Swing/6ZoneCanceled/7AbandonedBeforeFix. No hay criterios nuevos de éxito ni prueba de contacto real de pelota.

O1 global:5de12abandonos tienen ventanas con menos de3muestras y tiempo quieto≥120ms, **todos fuera de área;0con geometría elegible**. No afirmar que fijarían al quitar mínimo3. O2 global:11muestrasMoved anteriores a lockObservedAt recibidas después (8timer/3sample), desplazamiento máximo0,174605mm. No representan once fijaciones involuntarias. Son derivaciones descriptivas de datos nativos conservados, no nueva medición ni replay de detector.

## Las16 respuestas literales del PO

[Evidencia literal](../validation/evidence/2026-10-28-s1-sl04-po-final-answers.json). Cada pregunta se presentó por separado. Se conserva exactamente la respuesta, incluidas estimaciones de cantidad/tiempo:

| Pregunta literal | Respuesta literal |
|---|---|
| ¿Puedo colocar la mira donde quiero? | A veces si, otras veces no |
| ¿Puedo apuntar bajo sin activar accidentalmente la potencia? | A veces si otras veces no |
| ¿La mira permanece estable después de fijarse? | si |
| ¿Cuántas fijaciones involuntarias ocurren? | yo creo que un 30 - 40% |
| ¿Cuánto tiempo necesito para fijar la mira? | poco tiempo |
| ¿Entiendo claramente cuándo quedó fijada? | si |
| ¿Puedo conseguir golpes suaves, medios y fuertes voluntariamente? | si |
| ¿La potencia responde de forma consistente a la distancia? | si |
| ¿Puedo alcanzar la potencia máxima cómodamente? | si |
| ¿Existen limitaciones por el borde inferior de la pantalla? | no |
| ¿Puedo cancelar cuando realmente quiero? | si |
| ¿Ocurren cancelaciones accidentales? | no |
| ¿La zona interfiere con los movimientos normales? | si |
| ¿Cuánto demora el gesto completo, desde apoyar el dedo hasta soltar? | 1 segundo creo maximo |
| ¿Se siente natural o demasiado mecánico? | igual es mecanico, me gustaría algo que fuera natural |
| ¿Podría utilizarlo repetidamente durante un rally rápido? | si, pero se vería como pausado |

El«30 - 40%» es juicio estimado delPO, **no tasa medida ni sustituto del feedback mixto**. «1 segundo creo maximo» se conserva separado de la duración registrada (B5 mediana1,896s/máximo7,230s), sin corregir su literal ni modificar un umbral. «no» a borde inferior se conserva separado de los márgenes negativos relativos al área útil: no miden el borde físico. «no» a cancelaciones accidentales y «si» a interferencia de zona son dos respuestas distintas; no se neutralizan entre sí.

**Lectura para revisión, no aceptación:** el PO responde favorablemente a estabilidad tras fijar, comprensión, potencia por distancia/máxima y cancelación voluntaria. Declara apuntado y control bajo intermitentes, estima fijaciones involuntarias, señala interferencia de zona y desea un gesto más natural; un rally le parecería pausado. La evidencia no valida todo el protocolo debido a las desviaciones conservadas. No se elige ni implementa una mitigación, no se promueve preset/default ni se aceptaS1. Corresponde aClaude revisar y alPO decidir el siguiente paso.

## Restauración final y cierre de alcance

Primer reinicio solicitado rechazado por dispositivo bloqueado (CoreDevice10002): no cuenta como arranque. Tras elPO indicar «está listo» y observar Duplicación disponible, el segundo reinicio tuvo éxito. **Proceso nuevo2026-10-28T15:16:26.2320980Z (12:16:26Chile),4cec622 dev/P0-calib-1/e458bfd5ce9e/cero pendientes**. Cinco lanzamientos observados conP0/hash/cero pendientes, repetición persistida3; después solo launch/frames, ningún bloque/gesto/experimento. Pantalla normal/DEVcerrado/botónPausa/sin informe observados por Duplicación.

P0-calib-1,Po-1 ylast-active directamente byte a byte iguales al respaldo previo a instalar; selecciónP0. SHA-256 conservados en evidencia: P0`2f1295f201e238d9494bee285dd3f46e5bbb0f98b09ccde90fbd268f0874e4fd`,Po-1`9008c33ab89f82e669bc227a81b37ed641f69e4657ee70df6c89ff0c0edb1767`,last-active`5885ce8a7add389b2c82cb43f6944d7e2ef7ebeb0bffd77b5b635361a8a46087`.

444fuentes de producto/scripts/configuración y35archivos protegidos cotejados sin cambios. Defaults45c4e9af5c8f/preset7d1a61c39766/41heredados0d39e6016bd8/SimulationKey166a96e319c7 intactos. Sin modificación deD2, área/zona/parámetros/spec/planes/criterios/bandas/umbrales ni nuevas suites/check-release/build/instalación tras4cec622. Se mantienen los resultados técnicos frescos411/411EditMode y194/194PlayMode yexports limpiosdev/prod de esa fuente, no ejecuciones nuevas por documentación. Perfil incorporado renovado/vigente hasta2026-11-03T18:29:44Z; comprobar vigencia efectiva antes de cualquier futura instalación, ninguna autorizada ahora.

Entrega documental de sesión terminada paraClaude enPR#25, sin fusionar. **S1 sigue sin aceptar;S2–S4/T8 siguen sin autorizar. Detenerse; sin nueva práctica ni corrección sin decisión delPO.**
