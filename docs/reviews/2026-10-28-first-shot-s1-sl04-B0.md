# S1-sling D2 — B0 nativo tras la protección SL-S1-04

Build **4cec622 dev**, PR #25, preset temporal **S1-sling / 7d1a61c39766**. Datos nuevos del 28 de octubre, separados del B0 db72a00, del incidente 42265ca y del replay diagnóstico anterior. [Corrección y preparación](2026-10-27-first-shot-s1-sl04-controls.md), [evidencia con los 31 intentos y auditoría](../validation/evidence/2026-10-28-s1-sl04-B0.json), [decisiones del PO](2026-10-25-first-shot-po-approvals.md).

## Resultado registrado

| Dato | Resultado |
|---|---|
| Inicio / fin UTC | 13:28:23.459972 / 13:30:23.462508 |
| B0 | Completo por tiempo, 120,001808 s; sin interrupción de bloque registrada |
| Intentos | 31, todos B0/sesión1, comparables al hash armado y cero pendientes |
| Fijaciones | 27/31: 23 por timer y 4 por sample |
| Outcomes | 23 Swing, 4 ZoneCanceled, 4 AbandonedBeforeFix; sin Tap/SystemCanceled |
| Cancelación guiada | Registrada por la app; la intención voluntaria requiere respuesta del PO |
| Began → fijación | Mediana 805,697 ms; descriptivo, sin umbral nuevo |
| Muestras | 3413 en los gestos y en la adquisición completa; sin duplicados ni desorden |
| Entrega / edad | 38,610 Hz según intervalos intratoque; edad p50 13,3 ms / máximo 27,773 ms |
| P2 / H17 / Escenario durante B0 | Ninguna activación registrada |

La tasa entregada no es la frecuencia del sensor; no se presupone 60/120 Hz. DPI460 reportado y mm derivados sin comprobación física con regla. Los tres archivos P0-calib-1/Po-1/last-active permanecen directamente iguales byte a byte al respaldo; selección persistidaP0. Fuente/configuración/scripts:444 archivos cotejados intactos. La configuración temporal sigue activa, sin restauración final todavía porque la continuación de la sesión está pendiente.

La auditoría registra **una** pulsación de B0 mediante ui-activation, activation y result=armed; son fases de un mismo intento de inicio. Estaba S1-sling/7d1a61c39766, pausafalse, focusedtrue, DEV cerrado antes del armado, repeat0. El PO no envió una confirmación separada de la cuenta atrás antes del primer gesto; se solicitó aclaración retrospectiva sobre verla y sobre Completado. No se sustituye esa declaración por el hecho de que el temporizador haya completado.

## Observación literal del PO y apuntado bajo

> Listo, hice la prueba. Pero apuntar debajo de la línea como para hacer un drop me costaba apuntar o que se ponga el rombo amarillo. El resto lo entendí bien

No se convierte esta frase en aceptación de S1 ni en respuestas a las 16 preguntas finales. Se preguntó qué línea señala, si vio cuenta atrás/Completado y si identificó fijación/zona y canceló voluntariamente; respuestas pendientes. No repetir todavía ni avanzar a B1.

Tres abandonos tienen lockBlocked=true, mira clasificada como válida y punto táctil final por debajo del área útil registrada (yMin=690,984 px):

| Ordinal B0 | Mira final (m) | y táctil final (px) |
|---|---|---|
| 22 | (0,442; 1,040) | 650 |
| 23 | (0,769; 1,060) | 668 |
| 28 | (0,683; 1,352) | 629 |

Es compatible con la protección de área/cabida de zona de [SlingModel.ChooseRegion](../../Assets/SquashClash/Core/Shot/SlingModel.cs). No hay timestamp explícito de cada bloqueo de fijación ni etiqueta de intención por intento de práctica: no atribuir esos tres gestos específicamente a un drop ni declarar un defecto nuevo o una causa única. Otros intentos sí fijaron con mira baja, incluido y≈0,875 m. El cuarto abandono (10) fue un gesto de688ms que terminó con mira inválida; se conserva aparte. No se cambia ganancia, área, duración, soporte ni zona para facilitar el resultado.

## O1 — quietud y menos de tres muestras reales

Auditoría descriptiva sobre las muestras nuevas, sin muestras inventadas, grid de frames ni fijaciones alternativas. Se reconstruyen únicamente el candidato y su soporte con las operaciones espaciales float32 del código y el orden de recepción. Un intervalo cuenta si el contacto sigue activo, arrastre≥3mm, el reloj supera120ms desde el último reset y aún hay menos de3 muestras reales; se distingue si la siguiente muestra mantiene el mismo radio/velocidad de1mm/15mm/s.

**2 de los4 abandonos (22,23)** contienen esos intervalos, ambos con soporte2 y confirmados por la siguiente muestra:129,59/163,32ms desde el inicio del candidato hasta la siguiente recepción. **En ambos el último punto ya está fuera del área útil; cero casos con geometría elegible en esas ventanas.** No se aísla el mínimo3 como causa de esos abandonos. El JSON conserva los intervalos y sus condiciones; no se modifica D2 ni se promueve un mínimo alternativo.

## O2 — muestras anteriores a la fijación entregadas después

Predicado directo: timeReal < lockObservedTimeReal y receivedReal > lockObservedTimeReal, para cada fijación registrada. **5 muestras Moved en5 gestos (3,9,16,17,26)**:4 asociadas a fijación timer y1 a sample. Desplazamiento respecto del punto fijado **0,0552–0,1104mm**, máximo0,1104415mm, por la escala reportada. Se conservan ambos timestamps, secuencia y componentes de desplazamiento en la evidencia.

Esto acredita la llegada posterior de muestras anteriores al instante observado; no demuestra por sí solo fijaciones involuntarias ni intención del jugador. Sin resimular, corregir el gesto a posteriori ni aplicar otros parámetros.

## Estado de la sesión

B0 completo registrado y cancelaciones por zona observadas. Aún faltan las aclaraciones del PO para el gate de comprensión/voluntariedad y la línea del drop. **B1–B5 y las16 preguntas finales no iniciados.** O1/O2 registrados sin cambios de código. No nuevas suites/check-release/builds/instalaciones: la verificación previa411/411 y194/194 corresponde a la misma fuente4cec622.

Al continuar, comprobar hash antes del siguiente bloque; si no se entiende fijación/zona o aparece un defecto, detener y restaurarP0 en nuevo arranque con cotejo de tres archivos. Sin aceptación deS1, PR sin fusionar y S2–S4/T8 sin autorizar. Perfil vigente hasta2026-11-03T18:29:44Z, sujeto a comprobación efectiva antes de toda instalación futura.


## Aclaraciones literales y continuación autorizada

- Cuenta atrás antes delprimer gesto yCompletado alfinal: **«si»**.
- Reconocer fijación/zonaDENTRO ycancelar voluntariamente al soltar dentro: **«así es»**.
- Línea señalada para eldrop: **«si, la linea horizontal del medio del frontis»**.

Estas respuestas confirman comprensión para continuar según el gate autorizado, junto con B0completo ycancelación guiada registrada. La confirmación de cuenta atrás llegó retrospectivamente: **se mantiene la ausencia de mensaje previo alprimer gesto**, no se reescribe la cronología ni se declara aceptación deS1. La dificultad corresponde a apuntar por debajo de la línea media; no a la roja inferior. No se modifica parámetro alguno.

Captura de control before-B1-gate-confirmed: ningún lanzamiento/control/gesto nuevo después deB0, último hash7d1a61c39766/cero pendientes y3archivos iguales al respaldo. No se infiere por eso el estado de foco/pausa actual: antes deB1 elPO debe comprobar enDEV presetS1-sling/hash7d1a61c39766/cero pendientes/Jugando; si veP0, otrohash o pausa, no iniciar y avisar. El armado deja trazabilidad de sus precondiciones reales. Siguiente bloque según protocolo: **soloB1,10intentos T1–T5 dos veces**, apuntar/fijar/soltar suave, respuesta por intentoOK/Se fijó antes de tiempo sin sustituir intentos. B2–B5 y16preguntas finales todavía no realizados. No hay código/suites/build/instalación nuevos.
