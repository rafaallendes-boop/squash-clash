# First Shot S2 — observaciones abiertas tras la aceptación

**5 de noviembre de 2026 (Chile).** El PO aprobó S2 con condiciones y mantuvo M2-4 como control vigente. Las ocho observaciones de su decisión siguen abiertas. La preferencia de dirección se registra por separado. Los momentos de reevaluación son propuestas pendientes del PO. [Decisión literal del PO](2026-10-25-first-shot-po-approvals.md#decisión-del-po-y-cierre-documental-de-s2--5-de-noviembre-de-2026-chile) · [Resumen de la evidencia 2.7](2026-11-05-first-shot-s2-closing-summary.md).

## S2-OBS-01 — Trayectorias SL5/SL6

- **Estado:** abierta.
- **Evidencia:** en los bloques 10–13 el PO describió golpes que iban hacia el muro lateral o demasiado arriba. El informe documenta contactos cerca del frontis y la lectura de Claude sobre el contrato de primer impacto. La PR #31 conserva el diagnóstico offline y la evaluación de «bajar el punto». [Tanda 1, trayectoria](2026-10-31-first-shot-s2-tanda1.md#observación-del-po-sobre-la-trayectoria-en-sl5sl6-para-claudepo) · [Evaluación diagnóstica aprobada](https://github.com/rafaallendes-boop/squash-clash/blob/84ad994/docs/reviews/2026-11-04-first-shot-sl56-lowering-candidate.md).
- **Impacto conocido:** en esas condiciones, el recorrido percibido por el PO difirió de lo que esperaba al apuntar. Los registros de golpe de tanda 1 cubren el contacto y la salida. La evaluación de retornos de #31 es offline. La regla experimental de #32 sigue desactivada.
- **Momento propuesto, pendiente del PO:** después de integrar el diagnóstico documental de #31 y antes de considerar validada la experiencia de rally, delimitar qué retorno se espera en SL5/SL6 y qué evidencia faltaría para evaluarlo.

## S2-OBS-02 — Potencia media que cae en banda fuerte

- **Estado:** abierta.
- **Evidencia:** POT-M registró 5/10 Swing en banda media. En CAM-F y CAM-S, con media pedida, se registraron 4/10 y 2/10 en media y 6/10 y 8/10 en fuerte, respectivamente. El PO declaró que siguió la intensidad pedida. [Tanda 2, resumen](2026-11-01-first-shot-s2-tanda2.md#resumen-auditado-de-tanda21422) · [Tanda 3, revisión](2026-11-05-first-shot-s2-tanda3.md#revisión-de-claude-de-tanda-3--aprobación-comunicada-por-el-po).
- **Impacto conocido:** en esas series, la intensidad media declarada por el PO no coincidió siempre con la banda registrada. La potencia y su curva siguen vigentes.
- **Momento propuesto, pendiente del PO:** en una evaluación autorizada de la experiencia del jugador y, más adelante, al observar el control de potencia durante intercambios.

## S2-OBS-03 — Diferencia de saturación entre S1 y S2

- **Estado:** abierta.
- **Evidencia:** S1 B2 registró 50 % de saturación en fuertes, por encima de la advertencia vigente del 20 %. En S2, las series POT registraron 0/29 Swing saturados y la tanda 2 registró 0/44. CAM-F y CAM-S registraron cero saturados. `pullFullMm=35` mm sigue intacto. [Informe S1 M2-4](2026-10-28-first-shot-s1-m2-4-implementation.md) · [Tanda 2, resumen](2026-11-01-first-shot-s2-tanda2.md#resumen-auditado-de-tanda21422) · [Tanda 3, revisión](2026-11-05-first-shot-s2-tanda3.md#revisión-de-claude-de-tanda-3--aprobación-comunicada-por-el-po).
- **Impacto conocido:** la saturación de S1 limitó la diferencia registrada entre fuerte y máximo. Las series citadas de S2 no reprodujeron esa observación y la advertencia sigue abierta.
- **Momento propuesto, pendiente del PO:** junto con una revisión autorizada de potencia basada en la experiencia del jugador y el uso durante intercambios.

## S2-OBS-04 — Resbalones simulados con Swing (N04/N04b)

- **Estado:** abierta.
- **Evidencia:** 20 NEG-C y 21 NEG-D registraron 3/3 Swing cada uno, los seis antes de K-in y con cero contactos aplicados. El caso límite de NEG-D apenas fuera de zona, con P≈0, quedó sin acreditar. [Tanda 2, revisión](2026-11-01-first-shot-s2-tanda2.md#revisión-de-claude-de-tanda-2--aprobación-comunicada-por-el-po) · [Límites de la tanda](2026-11-01-first-shot-s2-tanda2.md#límites-y-desviaciones-conservados).
- **Impacto conocido:** el gesto simulado puede producir una orden Swing. Estos intentos registraron cero impactos y dejan sin estimar la frecuencia de un resbalón espontáneo.
- **Momento propuesto, pendiente del PO:** en la próxima evaluación autorizada de gestos involuntarios con golpe real, con N04 y N04b identificados por separado.

## S2-OBS-05 — Sonido en iPhone

- **Estado:** abierta.
- **Evidencia:** en PROD el PO respondió «tenía el cel en silencio a si que no». El resultado de sonido quedó **NotEvaluated**. [Tanda 4, preguntas](2026-11-05-first-shot-s2-tanda4-prod.md#preguntas-y-respuestas-literales) · [Resultados](2026-11-05-first-shot-s2-tanda4-prod.md#resultados-observados).
- **Impacto conocido:** lanzamiento, golpe y nota tienen confirmación del PO. La audibilidad del sonido en el iPhone quedó sin comprobar.
- **Momento propuesto, pendiente del PO:** en una comprobación de audio con autorización propia, tras identificar en el diseño de S3 qué se verificará automáticamente y qué requiere el iPhone.

## S2-OBS-06 — Timing pendiente de calibración

- **Estado:** abierta.
- **Evidencia:** el cierre recoge juicios preliminares por condición. H3 y latencia quedaron sin calibrar. S2-TIME-01 conservó las observaciones de SL5/SL6 sin ajustar las recetas. [Resumen 2.7, límites](2026-11-05-first-shot-s2-closing-summary.md#límites-de-lo-evaluado) · [Tanda 1, resultados](2026-10-31-first-shot-s2-tanda1.md#resultados-por-bloque) · [Tanda 2, límites](2026-11-01-first-shot-s2-tanda2.md#límites-y-desviaciones-conservados).
- **Impacto conocido:** el PO consideró satisfactorio el funcionamiento inicial del timing. Los juicios y los tiempos de la sesión dejan pendiente la calibración H3.
- **Momento propuesto, pendiente del PO:** al definir una evaluación autorizada de timing que distinga la calibración de la experiencia de juego, antes de darla por validada.

## S2-OBS-07 — Gesto durante rallies reales

- **Estado:** abierta.
- **Evidencia:** 22 RIT registró 9 Swing y 5 contactos en 10 recepciones de práctica. En tanda 3 el PO dijo «Es natural, pero se debe probar realmente cuando sea un partido» y «si, el rally puede ser rapido». [Tanda 2, RIT](2026-11-01-first-shot-s2-tanda2.md#resultado-del22) · [Tanda 3, preguntas](2026-11-05-first-shot-s2-tanda3.md#preguntas-y-respuestas).
- **Impacto conocido:** hay juicio de naturalidad en práctica. El ritmo y la repetición del gesto durante intercambios reales siguen sin observación.
- **Momento propuesto, pendiente del PO:** en una futura validación de rally, si el PO autoriza esa etapa y después de delimitar el impacto de SL5/SL6.

## S2-OBS-08 — Evento «Legibilidad» sin atribuir

- **Estado:** abierta.
- **Evidencia:** el 5 de noviembre a las 13:53:40 UTC, después de restaurar tanda 3 y antes del respaldo de PROD, se registró `legibility-launch` en DEV, precedido de toques filtrados. Las herramientas de Codex no interactuaban con el iPhone ni con Duplicación en ese momento. El PO respondió «No recuerdo». [Tanda 4, instalación y precheck](2026-11-05-first-shot-s2-tanda4-prod.md#instalación-y-precheck).
- **Impacto conocido:** el hash temporal pasó a `141920afa6af`. El reinicio repuso P0 `dd122831f592` y los tres archivos conservaron sus bytes. El origen sigue sin atribuir.
- **Momento propuesto, pendiente del PO:** si el evento vuelve a aparecer en una sesión DEV autorizada, revisar la secuencia de interfaz y los registros de esa sesión.

## S2-PREF-01 — Preferencia de dirección del tirón

**Estado:** abierta desde la aceptación de S1. En S2 se usó el tirón descendente y no se compararon físicamente direcciones. Última respuesta literal del PO a «Con el golpe real, ¿sigues prefiriendo tirar hacia arriba o adelante? ¿Por qué?».

> Creo que ahora me gusta como está, ya que apunto donde quiero tirar la pelota y tenso lo necesario, me gustaría probar en u npartido

El cierre de esta preferencia requiere una decisión expresa del PO. **Momento propuesto, pendiente del PO:** cuando revise la experiencia del gesto en intercambios, si autoriza esa evaluación. [Aceptación de S1](2026-10-25-first-shot-po-approvals.md#2026-10-30--revisión-claude-de-pr-28-y-aceptación-explícita-de-s1-por-el-po) · [Tanda 3, respuesta literal](2026-11-05-first-shot-s2-tanda3.md#preguntas-y-respuestas).
