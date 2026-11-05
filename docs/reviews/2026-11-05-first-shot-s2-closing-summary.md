# First Shot S2 — resumen de cierre de la sesión 2.7

**Para revisión de Claude y juicio global del PO, 5 de noviembre de 2026 (Chile).** Este resumen consolida las cifras publicadas y conserva las respuestas literales. La decisión sobre S2 y la fusión de la PR #30 quedan pendientes. [Predeclaración de la sesión](../validation/2026-10-31-first-shot-s2-session27-predeclaration.md).

## Alcance y configuración

| Tanda | Bloques y build | Informe | Revisión de Claude registrada |
|---|---|---|---|
| 1 | 01 FAM y 02–13, DEV `0.1.0+2ef4807 dev`. 03b y 04b son las cuotas completas. Los intentos iniciales de 03 y 04 se conservan aparte. | [Tanda 1](2026-10-31-first-shot-s2-tanda1.md) | [Preparación aprobada en `68f5383`](2026-10-25-first-shot-po-approvals.md#2026-10-31--preparación-head68f5383-aprobada-sesión-de-tanda-1-autorizada). [Revisión independiente de `66095c8`](2026-10-25-first-shot-po-approvals.md#2026-11-01--revisión-independiente-de-la-tanda-1-y-decisión-del-po-sobre-la-trayectoria), datos primarios confirmados y **cambios solicitados**. [Correcciones incorporadas al informe](2026-10-31-first-shot-s2-tanda1.md#corrección-tras-la-revisión-independiente). No consta una aprobación posterior de esta tanda. |
| 2 | 14–22, DEV `0.1.0+2ef4807 dev` | [Tanda 2](2026-11-01-first-shot-s2-tanda2.md) | [Aprobada sin correcciones en `0cc8060`](2026-10-25-first-shot-po-approvals.md#tanda-2-aprobada-por-claude-en-0cc8060--comunicado-por-el-po). |
| 3 | 23 CAM-F y 24 CAM-S, DEV `0.1.0+2ef4807 dev` | [Tanda 3](2026-11-05-first-shot-s2-tanda3.md) | [Aprobada sin correcciones en `a0abbe3`](2026-10-25-first-shot-po-approvals.md#revisión-de-claude-de-tanda-3--aprobación-comunicada-por-el-po). |
| 4 | 25 PROD, `0.1.0+8f030a8 prod` | [Tanda 4](2026-11-05-first-shot-s2-tanda4-prod.md) | [Aprobada en `e75703b`](2026-10-25-first-shot-po-approvals.md#revisión-de-claude-de-tanda-4-prod--aprobación-comunicada-por-el-po). |

Ambas builds corresponden a la fuente de producto `9131b1c`. En esa fuente, el control por defecto es **M2-4 (`gesture.control=2`)**. Defaults **106 claves, hash `915ae1e479b9`**, `power.pullFullMm=35` mm y **SimulationKey `166a96e319c7`**. El preset de control **P0-calib-1** conserva 41 claves con proyección histórica `8de3f2ecb217`. Superpuesto a los defaults S2 da **`dd122831f592`**, también H(1,V,0). La última restauración verificada en PROD observó ese hash y cero pendientes. [DefaultPreset](../../Assets/SquashClash/Runtime/Resources/DefaultPreset.json), [fase 1](2026-10-30-first-shot-s2-m24-phase1.md), [manifiesto de hashes](../validation/2026-10-31-first-shot-s2-session27-hashes.md), [restauración PROD](2026-11-05-first-shot-s2-tanda4-prod.md#restauración-final-y-entrega).

## Cuotas y totales publicados

Las sumas usan solo las filas publicadas de cuota. En tanda 1 se incluyen 03b y 04b. Los intentos iniciales de 03 y 04, la preparación y los gestos extra quedan separados. **N/Agg** indica que el desglose de los informes no permite una suma inequívoca.

| Tanda / fuente | Recepciones de cuota | Swing | Contactos | Valid | Perfect | Failed | NoOrder | C-A | AbandonedNoPull | Recepciones sin gesto | Extras fuera de cuota |
|---|---:|---:|---:|---:|---:|---:|---:|---:|---:|---:|---:|
| [1 · resultados por bloque](2026-10-31-first-shot-s2-tanda1.md#resultados-por-bloque) | 126 | 115 | 73 | 51 | 22 | N/Agg | N/Agg | 1 | N/Agg | N/Agg | 66 |
| [2 · resumen auditado](2026-11-01-first-shot-s2-tanda2.md#resumen-auditado-de-tanda21422) | 57 | 44 | 17 | 12 | 5 | 27 | 13 | 6 | 4 | 3 | 20 |
| [3 · resultados](2026-11-05-first-shot-s2-tanda3.md#resultados) | 20 | 20 | 14 | 11 | 3 | 6 | 0 | 0 | 0 | 0 | 4 |
| [4 · resultados](2026-11-05-first-shot-s2-tanda4-prod.md#resultados-observados) | 3 | 2 | 2 | 0 | 2 | 0 | 1 | 0 | 0 | 1 | 2 |
| **Total agregable** | **206** | **181** | **106** | **74** | **32** | N/Agg | N/Agg | **7** | N/Agg | N/Agg | **92** |

- **Tanda 1.** Valid y Perfect se suman de «Timing de Swing» y coinciden con el total de contactos publicado por bloque. Esa tabla usa Air/Late para timing y omisiones, InvalidAim y Rejected entre sus terminales. No publica un reparto final de Failed/NoOrder ni un desglose comparable de AbandonedNoPull o ausencia de gesto. Las omisiones publicadas son 1 en FAM y 5 en 02. El subtotal de **tandas 2–4** es 33 Failed, 14 NoOrder y 4 AbandonedNoPull. [Tanda 1](2026-10-31-first-shot-s2-tanda1.md#resultados-por-bloque), [tanda 2](2026-11-01-first-shot-s2-tanda2.md#resumen-auditado-de-tanda21422), [tanda 3](2026-11-05-first-shot-s2-tanda3.md#resultados), [tanda 4](2026-11-05-first-shot-s2-tanda4-prod.md#resultados-observados).
- **Recepciones sin gesto.** La tabla auditada de tanda 2 enumera tres (16 POT-F, 18 NEG-A y 19 NEG-B), y tanda 4 enumera una en 25. La observación de tanda 4 la llama «tercera» al citar solo NEG-A y NEG-B. Se conserva esa diferencia de alcance. No se publica un total de campaña. [Tabla de tanda 2](2026-11-01-first-shot-s2-tanda2.md#resumen-auditado-de-tanda21422), [observación de tanda 4](2026-11-05-first-shot-s2-tanda4-prod.md#resultados-observados).
- **Extras.** Los 92 son la suma de los extras vinculados a las cuotas de las cuatro tandas, incluso el vuelo previo cuando está registrado en la tabla. Quedan separados los 26 gestos antes de FAM y los 9 tardíos en el proceso de preparación de Codex, ambos publicados en tanda 1. En el 03 inicial hubo 2 Swing Valid con contacto tras el último cierre candidato. El 04 inicial conserva dos fronteras candidatas con 8 y 9 contactos, respectivamente. Esos recuentos de frontera ambigua quedan fuera de los totales. [Parciales de tanda 1](2026-10-31-first-shot-s2-tanda1.md#parciales-y-bloques-adicionales-conservados-sin-sustituir), [desviaciones](2026-10-31-first-shot-s2-tanda1.md#desviaciones-e-incidencias).

| Tanda | Interrupciones y aplicaciones de §3.2 publicadas |
|---|---|
| 1 | 03 inicial interrumpido y con cierres ambiguos. 04 inicial con dos candidatos de frontera. 03b se hizo en el mismo proceso y quedó declarada la omisión de §3.2 tras el segundo plano. Hubo suspensiones y relanzamientos sin un recuento exhaustivo comparable de episodios. [Desviaciones](2026-10-31-first-shot-s2-tanda1.md#desviaciones-e-incidencias). |
| 2 | Seis apariciones de «Toca para continuar» entre bloques, con §3.2 aplicado en cada una. Entre 15 y 16 se registró además una pausa por acceso a Duplicación, seguida de precheck en proceso nuevo al reanudar. Las cuotas se conservaron. [Límites](2026-11-01-first-shot-s2-tanda2.md#límites-y-desviaciones-conservados), [acceso entre 15 y 16](2026-11-01-first-shot-s2-tanda2.md#acceso-entre15-y16). |
| 3 | Cero recepciones interrumpidas en las dos cuotas. Una interrupción de preparación entre 23 y 24 con «iPhone en uso» y una aplicación explícita de §3.2. [Alcance y preparación](2026-11-05-first-shot-s2-tanda3.md#alcance-y-preparación), [cuotas](2026-11-05-first-shot-s2-tanda3.md#resultados). |
| 4 | La primera ventana de vigilancia de 120 s terminó sin abrir el proceso medido. El PO informó bloqueo por inactividad y desbloqueó. Se volvió a vigilar y se completó la cuota. El informe separa este incidente de preparación y el reinicio DEV por «Legibilidad». [Instalación y precheck](2026-11-05-first-shot-s2-tanda4-prod.md#instalación-y-precheck). |

**Interrupciones y aplicaciones de §3.2 en conjunto: N/Agg.** Los informes separan recepciones interrumpidas, bloques con frontera ambigua, overlays, pausas de acceso, reinicios y ventanas sin bloque iniciado. En tanda 1 falta un total exhaustivo de los episodios entre bloques. El subtotal explícito de aplicaciones por incidente enumerado es **7** entre las seis asociadas a overlays de tanda 2 y la de la preparación de 24.

## Juicios literales del PO por bloque

Las columnas corresponden a «¿la pelota va donde apunto?», «¿el timing es justo?» y «¿el salto es aceptable?» donde esas fueron las preguntas. En 09–13 se preguntó «¿La pelota fue donde apuntaste?», «¿Cómo sentiste el timing?» y «¿Cómo viste el salto?». La tabla conserva esas respuestas originales bajo su contexto. V corresponde a V100 y T a T3 puro. «—» indica que no consta una respuesta a esas preguntas para el bloque. [Tanda 1](2026-10-31-first-shot-s2-tanda1.md#respuestas-literales-por-bloque), [tanda 2](2026-11-01-first-shot-s2-tanda2.md), [tanda 3](2026-11-05-first-shot-s2-tanda3.md#preguntas-y-respuestas).

| Bloque y condición | Dirección | Timing | Salto |
|---|---|---|---|
| 01 FAM · SL1/V | — | — | — |
| 02 · SL1/V | «La pelota si fue donde apunto» | «El timing creo que está bien» | «El salto lo veo aceptable» |
| 03 inicial · SL1/T, parcial | — | — | — |
| 03b · SL1/T | «La pelota si iba donde apunte» | «El timing hay que entrenarlo pero se ve bien» | «El salto está bien» |
| 04 inicial · SL2/T, frontera ambigua | — | — | — |
| 04b · SL2/T | «la pelota si fue donde apunte» | «Buen timing» | «Buen salto» |
| 05 · SL2/V | «Pelota cuando salió, correcto» | «Timing, fue mas dificil que los anteriores.» | «Salto aceptable» |
| 06 · SL3/V | «La pelota iba donde apuntaba» | «Buen timing» | «Buen salto» |
| 07 · SL3/T | «La pelota cuando lo lograba iba donde apunte» | «Buen timing» | «Buen salto» |
| 08 · SL4/T | «La pelota fue donde apunto» | «Buen Timing.» | «Buen salto» |
| 09 · SL4/V, original reformulada | «La pelota fue donde apunte» | «Buen timing» | «Buen salto» |
| 10 · SL5/V, original reformulada | «Cuando lo logré la pelota no fue donde apunte, se iba mucho al muro de al lado. Debería haberse ido al fondo de la cancha» | «El timing mas dificl» | «Buen salto» |
| 11 · SL5/T, original reformulada | «La pelota iba muy lejos, o muy arriba, lo puedes ver, siendo que no debiese ser así» | «Senti mejor el timing» | «Buen salto» |
| 12 · SL6/T, original reformulada | «La pelota no iba donde apuntaba, que es lo mismo que vimos antes, eso se puede ver después» | «Buen timing y buen salto» (respuesta conjunta) | Véase la cita conjunta |
| 13 · SL6/V, original reformulada | «Idem de los anteriores» | «Buen timing» | «Buen salto» |
| 14 POT-S · SL1/V | «Si» | «Me costó esta vez el timing, pero se puede logar» | «si» |
| 15 POT-M · SL1/V | «Cuando logré golpear, si» | «si, me costó agarrar el timing, pero si» | «si» |
| 16 POT-F · SL1/V | «si, cuando logré que salga la pelota si» | «si» | «si» |
| 17 CAN · SL1/V | «Fueron cancelaciones, no hubo golpe» | «si» | «si» |
| 18 NEG-A · SL1/V | — | — | — |
| 19 NEG-B · SL1/V | — | — | — |
| 20 NEG-C · SL1/V | — | — | — |
| 21 NEG-D · SL1/V | — | — | — |
| 22 RIT · SL1/V | «si» | «si» | «si» |
| 23 CAM-F · SL1/V, fija | «si» | «si» | «si» |
| 24 CAM-S · SL1/V, seguimiento | «si» | «si» | «si» |
| 25 PROD · SL1/V | — | — | — |

14–16 recogen feedback complementario de potencia. 17 conserva el juicio del PO sobre timing y salto sin golpe en cuota. 18–21 recogen triada y tarea. Las cinco preguntas funcionales de PROD figuran abajo. Las respuestas globales permanecen globales. [Tanda 2](2026-11-01-first-shot-s2-tanda2.md), [tanda 4](2026-11-05-first-shot-s2-tanda4-prod.md#preguntas-y-respuestas-literales).

### Repreguntas del salto de 09–13

Se preguntó «¿el salto es aceptable?» una vez por condición, un día después, de memoria y antes del precheck de 14. Se conservan aparte de las respuestas originales. [Repreguntas retrospectivas](2026-11-01-first-shot-s2-tanda2.md#repreguntas-retrospectivas-sobre-el-salto-de-0913).

| Bloque y condición | Respuesta literal |
|---|---|
| 09 · SL4/V | «si» |
| 10 · SL5/V | «si» |
| 11 · SL5/T | «si» |
| 12 · SL6/T | «si» |
| 13 · SL6/V | «si» |

### Las 16 preguntas S1 al cierre de tanda 3 y la preferencia de dirección

[Preguntas y respuestas de tanda 3](2026-11-05-first-shot-s2-tanda3.md#preguntas-y-respuestas).

| # | Pregunta literal S1 | Respuesta literal del PO |
|---:|---|---|
| 1 | ¿Puedo colocar la mira donde quiero? | «si» |
| 2 | ¿Puedo apuntar bajo sin activar accidentalmente la potencia? | «si» |
| 3 | ¿La mira permanece estable después de fijarse? | «si» |
| 4 | ¿Cuántas fijaciones involuntarias ocurren? | «no tuve» |
| 5 | ¿Cuánto tiempo necesito para fijar la mira? | «puede ser menos de 1 segundo» |
| 6 | ¿Entiendo claramente cuándo quedó fijada? | «si» |
| 7 | ¿Puedo conseguir golpes suaves, medios y fuertes voluntariamente? | «si» |
| 8 | ¿La potencia responde de forma consistente a la distancia? | «si» |
| 9 | ¿Puedo alcanzar la potencia máxima cómodamente? | «si» |
| 10 | ¿Existen limitaciones por el borde inferior de la pantalla? | «no» |
| 11 | ¿Puedo cancelar cuando realmente quiero? | «si» |
| 12 | ¿Ocurren cancelaciones accidentales? | «no» |
| 13 | ¿La zona interfiere con los movimientos normales? | «no» |
| 14 | ¿Cuánto demora el gesto completo, desde apoyar el dedo hasta soltar? | «puede ser com o1 segundo?» |
| 15 | ¿Se siente natural o demasiado mecánico? | «Es natural, pero se debe probar realmente cuando sea un partido» |
| 16 | ¿Podría utilizarlo repetidamente durante un rally rápido? | «si, el rally puede ser rapido» |

«Con el golpe real, ¿sigues prefiriendo tirar hacia arriba o adelante? ¿Por qué?»

> Creo que ahora me gusta como está, ya que apunto donde quiero tirar la pelota y tenso lo necesario, me gustaría probar en u npartido

### Las cinco preguntas PROD

[Preguntas y respuestas de tanda 4](2026-11-05-first-shot-s2-tanda4-prod.md#preguntas-y-respuestas-literales).

| Pregunta literal | Respuesta literal del PO |
|---|---|
| Por número de recepción, ¿qué excepciones recuerdas: Apoyo sin querer, No fijó / falló u otra dificultad? | «todo ok» |
| ¿Viste el lanzamiento? | «si» |
| ¿Pudiste dar un golpe? | «si» |
| ¿Viste la nota del resultado? | «si» |
| ¿Oíste el sonido? | «tenía el cel en silencio a si que no» |

## Límites de lo evaluado

- **Sonido PROD:** NotEvaluated por iPhone en silencio, sin resultado Fail. Las preguntas de lanzamiento, golpe y nota sí tienen respuesta. [Tanda 4](2026-11-05-first-shot-s2-tanda4-prod.md#resultados-observados).
- **Timing:** los juicios son preliminares. H3 y latencia siguen sin calibrar y no se repitió la calibración física de DPI. Los tiempos observados y las estimaciones del PO se conservan separados. [Tanda 2](2026-11-01-first-shot-s2-tanda2.md#límites-y-desviaciones-conservados), [tanda 3](2026-11-05-first-shot-s2-tanda3.md#lectura-descriptiva-y-límites).
- **Rendimiento:** coste de frame completo asociado al golpe y asignaciones por parte NotEvaluated. Los costes publicados por etapa tienen sus propias muestras. Térmico disponible solo como contexto donde se registró. Tanda 3 quedó sin lectura específica de térmico, batería y ruta de audio, y la campaña no valida H17. [Tanda 2](2026-11-01-first-shot-s2-tanda2.md#límites-y-desviaciones-conservados), [tanda 3](2026-11-05-first-shot-s2-tanda3.md#lectura-descriptiva-y-límites), [tanda 4](2026-11-05-first-shot-s2-tanda4-prod.md#resultados-observados).
- **Rally y puntería:** partido y rally real sin evaluar. C fue una referencia verbal sin marcador y Aim→C no mide la intención del PO. La trayectoria tras el frontis no quedó medida en los registros de golpe de tanda 1. [Tanda 1](2026-10-31-first-shot-s2-tanda1.md#limitaciones), [tanda 3](2026-11-05-first-shot-s2-tanda3.md#lectura-descriptiva-y-límites).

## Abiertos desde la aceptación de S1

| Tema | Punto de partida en S1 | Observación en S2 |
|---|---|---|
| Saturación | B2 tuvo 30/30 en banda, con **50 % de saturación en fuertes** frente a la advertencia vigente del 20 %. `power.pullFullMm=35` mm quedó intacto. | POT-S 8/10 Swing suaves, POT-M 5/10 medios, POT-F 9/9 fuertes en 10 recepciones. Saturación 0/29 Swing POT y 0/44 en toda tanda 2. En CAM-F y CAM-S se pidió media. Quedaron en media 4/10 y 2/10, y en fuerte 6/10 y 8/10 respectivamente, con cero saturados. Se conserva junto a la declaración del PO de haber seguido la intensidad. Estas series dejan abierta la advertencia de S1. [Aceptación de S1](2026-10-25-first-shot-po-approvals.md#2026-10-30--revisión-claude-de-pr-28-y-aceptación-explícita-de-s1-por-el-po), [tanda 2](2026-11-01-first-shot-s2-tanda2.md#resumen-auditado-de-tanda21422), [revisión de tanda 3](2026-11-05-first-shot-s2-tanda3.md#revisión-de-claude-de-tanda-3--aprobación-comunicada-por-el-po). |
| N04/N04b | Riesgos abiertos en la aceptación de S1, sin evaluación física entonces. | Los resbalones simulados en 20 NEG-C y 21 NEG-D produjeron 3/3 Swing cada uno, los seis antes de K-in, con cero contactos aplicados. Es compatible con ambos riesgos. El caso límite de NEG-D apenas fuera de zona, con P≈0, quedó sin acreditar. La simulación solicitada no estima la frecuencia espontánea. [Aceptación de S1](2026-10-25-first-shot-po-approvals.md#2026-10-30--revisión-claude-de-pr-28-y-aceptación-explícita-de-s1-por-el-po), [revisión de tanda 2](2026-11-01-first-shot-s2-tanda2.md#revisión-de-claude-de-tanda-2--aprobación-comunicada-por-el-po). |
| Sensación mecánica y ritmo de rally | «mas natural que antes, pero aun sigue mecanico. Lo que si tendría que probar en partido real». B5 dio una mediana publicada de 1121,000 ms a ritmo cómodo, con la evaluación de intensidades B5 en NotEvaluated. | 22 RIT tuvo 9 Swing y 5 contactos en 10 recepciones. En tanda 3, «Es natural, pero se debe probar realmente cuando sea un partido» y «si, el rally puede ser rapido». Son juicios del PO en práctica. El rally y el partido siguen sin observación. [Informe S1 M2-4](2026-10-28-first-shot-s1-m2-4-implementation.md), [22 RIT](2026-11-01-first-shot-s2-tanda2.md#resultado-del22), [respuestas de tanda 3](2026-11-05-first-shot-s2-tanda3.md#preguntas-y-respuestas). |
| Dirección del tirón | Claude registró la preferencia por tirar hacia arriba o adelante. | Se usó el tirón descendente. La preferencia al cierre es la cita literal de arriba. No se compararon físicamente direcciones. [Informe S1 M2-4](2026-10-28-first-shot-s1-m2-4-implementation.md), [tanda 3](2026-11-05-first-shot-s2-tanda3.md#preguntas-y-respuestas). |

## Observaciones de Claude por tanda

- **Tanda 1.** La revisión independiente confirmó los datos primarios y pidió corregir el registro del protocolo. Las preguntas llegaron juntas y después de mostrar resultados. Hubo reformulaciones en 02 y 09–13, y el salto de 09–13 se repreguntó después según lo indicado arriba. Se conservaron las dos fronteras problemáticas, los extras corregidos, el inicio sin precheck previo de 03b/04b y la omisión de §3.2 antes de 03b. [Revisión independiente](2026-10-31-first-shot-s2-tanda1.md#corrección-tras-la-revisión-independiente), [repreguntas](2026-11-01-first-shot-s2-tanda2.md#repreguntas-retrospectivas-sobre-el-salto-de-0913).
- **SL5/SL6 en tanda 1.** S2-TIME-01 quedó como observación predeclarada. En 10 SL5/V hubo 7 Air en 10, 2 contactos y 1 Late. En 13 SL6/V hubo 4 Air y 5 contactos en 10, con 1 Rejected. En T3 puro hubo 10 contactos en 11 SL5/T y 7 en 12 SL6/T, sobre 10 recepciones cada uno. Se conservaron las recetas SL. Por separado, las respuestas sobre trayectoria en 10–13 y la geometría del golpe llevaron a tratar el posible defecto en otra PR. El informe de tanda 1 describe contactos alrededor de 1 m del frontis y publica, por ejemplo, elevaciones de 49–68° en 11 SL5/T. La lectura de Claude relacionó la salida alta con el contrato del primer impacto en frontis. [Fase 1 y S2-TIME-01](2026-10-30-first-shot-s2-m24-phase1.md), [resultados y trayectoria](2026-10-31-first-shot-s2-tanda1.md#observación-del-po-sobre-la-trayectoria-en-sl5sl6-para-claudepo).
- **Tanda 2.** Modal agrupado a petición del PO, con la triada de 17–21 recogida al cierre. En 18 la aclaración «A las tres recepciones que conté» llegó después de mostrar resultados. Seis overlays entre bloques con §3.2 aplicado y origen sin atribuir. R1 de 18 y 19 sin gesto registrado. Los negativos 20/21 quedan descritos arriba. Los juicios del salto abarcan el máximo publicado de 7,988459 diámetros en 16, con tres contactos aplicados. [Aprobación de tanda 2](2026-11-01-first-shot-s2-tanda2.md#revisión-de-claude-de-tanda-2--aprobación-comunicada-por-el-po).
- **Tanda 3.** Modal de campos separados antes de resultados. El PO declaró seguir la intensidad y se conserva el reparto de medios y fuertes de ambas cámaras. Salto en fija p50 4,18 y máximo 8,29 diámetros, en seguimiento p50 2,52 y máximo 3,82. El PO juzgó ambos aceptables antes de ver cifras. Bloques secuenciales y n=7 contactos por condición, sin atribuir efecto causal a la cámara. Claude dejó registrados los conflictos documentales de #31 y #32 con #30. [Aprobación de tanda 3](2026-11-05-first-shot-s2-tanda3.md#revisión-de-claude-de-tanda-3--aprobación-comunicada-por-el-po).
- **Tanda 4.** Claude confirmó la frontera, la cuota, los extras, el prefijo exacto, la fuente y la restauración. Sonido NotEvaluated. La recepción 1 quedó sin gesto, con «todo ok» global y la discrepancia de recuento indicada arriba. En DEV, a las 10:53:40 de Chile y antes del respaldo nuevo, se registró la activación de «Legibilidad», precedida de toques filtrados en la parte superior. Las herramientas de Codex no interactuaban con el iPhone ni con Duplicación en ese momento. El PO respondió «No recuerdo». El origen queda sin atribuir. [Aprobación de tanda 4](2026-11-05-first-shot-s2-tanda4-prod.md#revisión-de-claude-de-tanda-4-prod), [incidencia DEV](2026-11-05-first-shot-s2-tanda4-prod.md#instalación-y-precheck).

## Decisiones pendientes del PO

1. Aceptar o no S2 con M2-4 tras la sesión 2.7, incluido el juicio global del salto y el alcance de los juicios por condición.
2. Tratamiento de la saturación y de `pullFullMm`, actualmente 35 mm.
3. Destino de la [PR #31](https://github.com/rafaallendes-boop/squash-clash/pull/31). «Bajar el punto» figura como candidata diagnóstica, sin adopción en el producto de la campaña.
4. Destino de la [PR #32](https://github.com/rafaallendes-boop/squash-clash/pull/32). La regla de retorno está implementada allí y desactivada por defecto. Su activación y su validación física requieren autorización propia.
5. Si se autoriza una comprobación del sonido, que quedó NotEvaluated.
6. Si se autoriza fusionar, el orden registrado es #30 primero y luego resolver los conflictos documentales de #31 y #32.
7. Si se reinstala DEV mientras el perfil renovado siga vigente hasta **2026-11-12T13:55:07Z**. El estado verificado de cierre es PROD `0.1.0+8f030a8` instalada, DEV sin instalar y P0 restaurado. [Cierre de PROD](2026-11-05-first-shot-s2-tanda4-prod.md#restauración-final-y-entrega).
8. S3–S4/T8 siguen sin autorización de implementación.

Las tres PR siguen abiertas y sin fusionar. La sesión física queda detenida.
