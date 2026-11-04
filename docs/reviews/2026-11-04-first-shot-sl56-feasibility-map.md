# First Shot — mapa offline de factibilidad SL5/SL6 y controles SL1–SL4

Fecha: 2026-11-04, America/Santiago. **Diagnóstico, sin elegir regla, valores de producto ni implementación.** El PO mantiene la mira como punto del frontis y pide investigar una regla de golpe jugable con ajuste visible C4. [Decisión literal](2026-10-25-first-shot-po-approvals.md). Este mapa no implementa esa regla ni cambia el contrato vigente.

[Método](../diagnostics/2026-11-04-sl56-feasibility-method.md) confirmado en **c000f85 y 7b5d943 antes del primer cálculo**. [JSON de resumen y máscaras](../validation/evidence/2026-11-04-sl56-feasibility-summary.json), [matrices por contacto](../diagnostics/2026-11-04-sl56-feasibility-matrices.md) y [reproducción](../diagnostics/2026-11-04-sl56-feasibility-reproduction.md). Producto9131b1c/ShotSolver/P2 originales. No sesión física, iPhone, relanzamiento, preparación16, instalación, tests, builds ni ajustes de defaults/SL/spec/criterios.

## Entradas y muestreo

Se reconstruyeron las120 recepciones medidas de tanda 1:80 controles02/03b/04b/05–09 y40 focales10–13. **49 contactos deSL1–SL4 y24 de SL5/SL6**, sin parciales originales03/04, FAM, extras o fallos sin contacto. Las24 posiciones focales coinciden exactamente con el derivado revisado; las73se cotejaron con sus líneas primarias, versiones finales, hashes y pending0. Los47 casos sin contacto aplicado permanecen como exclusiones explícitas del conjunto original, no se fabrican posiciones.

Para cada posición fija: **9x×11y×11P=1089 combinaciones**. Total **79.497** solicitudes matemáticas al ShotSolver original. No se reutiliza la potencia/velocidad histórica como salida: el solver resuelve cada Aim/P de la rejilla. El casoP=0 no significa gesto Tap/C-A. La rejilla no son tareas humanas ni valores elegidos para producto.

Cada Solved se simuló con P2 original hasta el paso del segundo bote o10s. Las **dos ejecuciones completas finales son idénticas byte a byte**; fuentes/entradas protegidas intactas. Se publican máscaras y rangos de profundidad por punto; el TSV completo conserva cada solución, primer impacto, out/tin/escape, primer bote(x,z), paso a mitad trasera, pared trasera y laterales. Ruta local ignorada: **Logs/feasibility-20261104/combinations.tsv**, hash en el JSON. No se versionan logs primarios ni identificadores personales.

## Cómo leer las columnas

- **Solved:** el solver encontró una salida, que puede usar SpeedRaised y/o TargetLowered. No equivale a respetar exactamente la solicitud original.
- **Sin ajuste (U):** Solved sin esos dos cambios. **Con ajuste (A):** alguno de los ajustes existentes. Las matrices separan U/A por cada P.
- **Retorno dentro de cancha:** intersección descriptiva predeclarada: primer impacto frontis, primer bote observado dentro del rectángulo, sin out/tin ni escape muestreado antes o en ese bote. **No es aprobación de una regla de golpe jugable.**
- **Primer bote trasero:** ese retorno con z≥4,875m, mitad geométrica de 9,75 m; no short line ni nuevo criterio.
- **Paso a mitad trasera del retorno:** después del primer frontis y antes del bote. La presencia inicial del control en la mitad trasera no cuenta como retorno profundo.
- **Pared trasera antes del bote:** contacto de pared finita de ese retorno. Se informa además de la z exacta del bote; no se inventa un corte distinto para llamar «fondo».
- **Laterales:** no se descartan automáticamente. Se reportan superficies/cantidad y extensión finita en el TSV/máscaras.
- **Escape:** se observa al final del paso; puede perder una excursión muy breve entre pasos. Suelo no acotado/planos fuera del ancho real se exponen como límites. Continuidad después de out es contrafactual, no recorrido humano observado.

## Resultados de la rejilla

Conteos de **combinaciones muestreadas**, no tasas humanas ni población aleatoria. Columnas de incidencias se superponen; no se suman. Se mantienen denominadores por grupo y no se mezclan con las24salidas nativas originales.

| Grupo | Contactos | Combinaciones | Solved | Sin ajuste | Con ajuste | Retorno dentro de cancha | Ese retorno sin ajuste | Bote trasero | Pared trasera antes del bote |
|---|---:|---:|---:|---:|---:|---:|---:|---:|---:|
| SL5/SL6 | 24 | 26136 | 26096 | 23680 | 2416 | 17403 | 14989 | 1521 | 217 |
| SL1–SL4 | 49 | 53361 | 52726 | 41237 | 11489 | 49044 | 37558 | 19075 | 2799 |
| Total | 73 | 79497 | 78822 | 64917 | 13905 | 66447 | 52547 | 20596 | 3016 |

En las73 posiciones hay al menos una muestra de retorno dentro de cancha, de primer bote trasero y de pared trasera antes del bote. En los24 focales, **las1521 muestras de primer bote trasero y las217 de pared trasera son sin ajuste del solver**. En controles,18861 de 19075 botes traseros y2786 de 2799 contactos traseros son sin ajuste. Esto muestra soluciones en otras solicitudes de Aim/P, no que las solicitudes originales del PO produjeran esos resultados.

Todo Solved tuvo primer impacto frontis; **675 solicitudes quedaron Unsolved**, conservadas con motivo. Ninguna simulación de salida alcanzó el cap. No se cambió tolerancia de Verify, vMin/vMax/vCap, gravedad, materiales, radio, cuerpo, Aim/P original de la evidencia o política de fallback.

| Grupo | Out antes del bote | Escape antes del bote | Primer bote fuera | Lateral finita antes del bote | Plano fuera de extensión antes del bote |
|---|---:|---:|---:|---:|---:|
| SL5/SL6 | 2856 | 7087 | 5854 | 18573 | 553 |
| SL1–SL4 | 3482 | 377 | 239 | 22583 | 58 |

Estas incidencias no son nuevos Fail deS2 ni un detector de intención. La misma geometría de entrada puede admitir tanto retornos dentro como fuera con distintos puntos/P. La rejilla no demuestra factibilidad continua entre celdas ni garantiza que una regla simple pueda conservar siempre punto y potencia y además producir profundidad.

## Resumen por contacto

Cada fila tiene1089 muestras. «Puntos con retorno» cuenta puntos de los99 con al menos unP que cumple la columna, **incluyendo asistencia explícita**; no significa todos losP ni solicitud original intacta. Las matrices muestran la pareja punto/P exacta y U/A. «Trasero U» cuenta las muestras de bote trasero sin ajustes. Profundidad máxima esz del primer bote entre los retornos de la columna, no una cota continua.

### SL5/SL6

| Caso | Condición | Puntos con retorno /99 | Puntos con bote trasero /99 | Retornos /1089 | Botes traseros | Traseros U | Pared trasera antes del bote | Profundidad máxima z (m) |
|---|---|---:|---:|---:|---:|---:|---:|---:|
| 10-02 | SL5-V | 99 | 24 | 728 | 63 | 63 | 10 | 9,531774 |
| 10-09 | SL5-V | 99 | 24 | 723 | 60 | 60 | 9 | 9,577010 |
| 11-01 | SL5-T | 99 | 25 | 734 | 71 | 71 | 10 | 9,709097 |
| 11-02 | SL5-T | 99 | 23 | 723 | 59 | 59 | 9 | 9,647915 |
| 11-03 | SL5-T | 99 | 24 | 729 | 64 | 64 | 9 | 9,694416 |
| 11-04 | SL5-T | 99 | 24 | 729 | 64 | 64 | 9 | 9,694416 |
| 11-05 | SL5-T | 99 | 22 | 709 | 55 | 55 | 7 | 9,722714 |
| 11-06 | SL5-T | 99 | 25 | 736 | 73 | 73 | 8 | 9,584500 |
| 11-07 | SL5-T | 99 | 22 | 712 | 57 | 57 | 9 | 9,630864 |
| 11-08 | SL5-T | 99 | 24 | 723 | 60 | 60 | 9 | 9,577010 |
| 11-09 | SL5-T | 99 | 24 | 728 | 63 | 63 | 10 | 9,531774 |
| 11-10 | SL5-T | 99 | 26 | 743 | 78 | 78 | 10 | 9,586030 |
| 12-02 | SL6-T | 99 | 25 | 734 | 72 | 72 | 8 | 9,719734 |
| 12-03 | SL6-T | 99 | 24 | 725 | 63 | 63 | 10 | 9,531773 |
| 12-04 | SL6-T | 99 | 23 | 717 | 61 | 61 | 11 | 9,681354 |
| 12-05 | SL6-T | 99 | 21 | 704 | 51 | 51 | 8 | 9,474428 |
| 12-07 | SL6-T | 99 | 22 | 711 | 55 | 55 | 7 | 9,722714 |
| 12-08 | SL6-T | 99 | 23 | 721 | 61 | 61 | 9 | 9,719151 |
| 12-09 | SL6-T | 99 | 25 | 740 | 73 | 73 | 8 | 9,569754 |
| 13-02 | SL6-V | 99 | 24 | 723 | 60 | 60 | 9 | 9,577008 |
| 13-04 | SL6-V | 99 | 24 | 723 | 60 | 60 | 9 | 9,577008 |
| 13-06 | SL6-V | 99 | 24 | 726 | 64 | 64 | 9 | 9,694416 |
| 13-07 | SL6-V | 99 | 24 | 731 | 67 | 67 | 10 | 9,492521 |
| 13-10 | SL6-V | 99 | 24 | 731 | 67 | 67 | 10 | 9,492521 |

### SL1–SL4 (control)

| Caso | Condición | Puntos con retorno /99 | Puntos con bote trasero /99 | Retornos /1089 | Botes traseros | Traseros U | Pared trasera antes del bote | Profundidad máxima z (m) |
|---|---|---:|---:|---:|---:|---:|---:|---:|
| 02-03 | SL1-V | 99 | 87 | 991 | 395 | 395 | 55 | 9,724294 |
| 03b-01 | SL1-T | 99 | 86 | 986 | 384 | 384 | 52 | 9,717649 |
| 03b-02 | SL1-T | 99 | 87 | 992 | 388 | 388 | 52 | 9,712998 |
| 03b-03 | SL1-T | 99 | 87 | 1001 | 400 | 399 | 58 | 9,722301 |
| 03b-04 | SL1-T | 99 | 86 | 986 | 384 | 384 | 52 | 9,717649 |
| 03b-05 | SL1-T | 99 | 87 | 1001 | 400 | 399 | 58 | 9,722301 |
| 03b-06 | SL1-T | 99 | 87 | 992 | 388 | 388 | 52 | 9,712998 |
| 03b-07 | SL1-T | 99 | 87 | 990 | 386 | 386 | 50 | 9,722719 |
| 03b-09 | SL1-T | 99 | 87 | 1001 | 400 | 399 | 58 | 9,722301 |
| 03b-10 | SL1-T | 99 | 87 | 992 | 388 | 388 | 52 | 9,712998 |
| 04b-01 | SL2-T | 99 | 87 | 989 | 401 | 398 | 57 | 9,722301 |
| 04b-03 | SL2-T | 99 | 86 | 966 | 361 | 360 | 50 | 9,719481 |
| 04b-04 | SL2-T | 99 | 87 | 989 | 401 | 398 | 57 | 9,722301 |
| 04b-05 | SL2-T | 99 | 87 | 985 | 387 | 385 | 52 | 9,712998 |
| 04b-06 | SL2-T | 99 | 87 | 989 | 401 | 398 | 57 | 9,722301 |
| 04b-08 | SL2-T | 99 | 85 | 964 | 371 | 369 | 47 | 9,728764 |
| 04b-10 | SL2-T | 99 | 86 | 976 | 386 | 385 | 52 | 9,717649 |
| 05-01 | SL2-V | 99 | 87 | 985 | 387 | 385 | 52 | 9,712998 |
| 05-03 | SL2-V | 99 | 87 | 985 | 387 | 385 | 52 | 9,712998 |
| 05-08 | SL2-V | 99 | 87 | 989 | 401 | 398 | 57 | 9,722301 |
| 06-01 | SL3-V | 99 | 85 | 1017 | 395 | 395 | 60 | 9,706515 |
| 06-03 | SL3-V | 99 | 84 | 1020 | 386 | 384 | 60 | 9,702153 |
| 06-04 | SL3-V | 99 | 85 | 1017 | 395 | 395 | 60 | 9,706515 |
| 06-05 | SL3-V | 99 | 85 | 1017 | 399 | 397 | 61 | 9,698359 |
| 06-07 | SL3-V | 99 | 85 | 1024 | 401 | 401 | 63 | 9,706941 |
| 06-08 | SL3-V | 99 | 84 | 1020 | 386 | 384 | 60 | 9,702153 |
| 06-09 | SL3-V | 99 | 84 | 1020 | 386 | 384 | 60 | 9,702153 |
| 06-10 | SL3-V | 99 | 84 | 1020 | 386 | 384 | 60 | 9,702153 |
| 07-02 | SL3-T | 99 | 85 | 1023 | 403 | 403 | 65 | 9,725815 |
| 07-03 | SL3-T | 99 | 85 | 1017 | 399 | 397 | 61 | 9,698359 |
| 07-04 | SL3-T | 99 | 85 | 1017 | 395 | 395 | 60 | 9,706515 |
| 07-05 | SL3-T | 99 | 84 | 1020 | 386 | 384 | 60 | 9,702153 |
| 07-08 | SL3-T | 99 | 84 | 1021 | 392 | 391 | 60 | 9,658112 |
| 07-09 | SL3-T | 99 | 84 | 1021 | 392 | 391 | 60 | 9,658112 |
| 07-10 | SL3-T | 99 | 85 | 1024 | 401 | 401 | 63 | 9,706941 |
| 08-02 | SL4-T | 99 | 85 | 1011 | 393 | 377 | 60 | 9,724432 |
| 08-03 | SL4-T | 99 | 84 | 1000 | 379 | 359 | 55 | 9,710700 |
| 08-04 | SL4-T | 99 | 85 | 1000 | 381 | 368 | 57 | 9,709127 |
| 08-05 | SL4-T | 99 | 85 | 992 | 382 | 374 | 57 | 9,706515 |
| 08-06 | SL4-T | 99 | 85 | 1000 | 381 | 368 | 57 | 9,709127 |
| 08-07 | SL4-T | 99 | 84 | 1000 | 373 | 358 | 57 | 9,702152 |
| 08-08 | SL4-T | 99 | 84 | 1000 | 379 | 359 | 55 | 9,710700 |
| 08-09 | SL4-T | 99 | 85 | 1004 | 386 | 377 | 60 | 9,725110 |
| 08-10 | SL4-T | 99 | 85 | 1001 | 392 | 380 | 60 | 9,723973 |
| 09-01 | SL4-V | 99 | 85 | 1011 | 393 | 377 | 60 | 9,724432 |
| 09-05 | SL4-V | 99 | 85 | 993 | 385 | 382 | 61 | 9,698359 |
| 09-07 | SL4-V | 99 | 85 | 1011 | 393 | 377 | 60 | 9,724432 |
| 09-09 | SL4-V | 99 | 84 | 981 | 375 | 366 | 54 | 9,658112 |
| 09-10 | SL4-V | 99 | 85 | 993 | 385 | 382 | 61 | 9,698359 |


## Zonas y P sin una regla elegida

Las [matrices desplegables por contacto](../diagnostics/2026-11-04-sl56-feasibility-matrices.md) son el mapa de zonas solicitado: y por filas, x por columnas y P discretos en cada celda, distinguiendo U/A; hay una matriz de retorno dentro y otra de bote trasero para cada una de las73 posiciones. El JSON aporta máscaras independientes de paso a mitad trasera, pared trasera, laterales, out, escape y ajustes, con rangos de profundidad por punto. Los datos por combinación se conservan completos en el TSV reproducible.

No se recomienda una fila, potencia, ángulo, distancia al frontis, corte de profundidad o criterio de lateral como política. Tampoco se extrapola una máscara vacía a imposibilidad fuera de rejilla. Elegir una regla y el alcance de ajuste visible C4 necesita una decisión nueva del PO; no se incorpora a S2 ni S3 por inferencia.

## Relación con la corrección del diagnóstico anterior

La unión21/24 y los tres restantes se obtuvieron **solo leyendo el JSON anterior**, no ejecutando otra vez aquellas24salidas históricas. El primer bote de 11-08 es 4,001148 m: redondea a 4,0 m pero no ≤4,0 literal. Esos tres permanecen en la mitad delantera. El mapa cambia las solicitudes matemáticas de Aim/P como investigación, no reinterpreta lo que elPO pidió/hizo en tanda 1.

## Integridad, prioridad y entrega

Primarios de ambos procesos,15fuentesCore originales y archivos protegidos conservan hashes; las salidas nuevas están fuera de Logs/evidence. El código de arnés se documenta fuera de producto/tests y no sustituye P2. No hubo sesión física activa durante el cálculo. **Tandas 2–4 de PR #30 conservan prioridad antes de 2026-11-07T17:58:46Z**; al aviso del PO se suspende trabajo de mapa para verificar P0 en proceso nuevo y preparar 16 según protocolo. No se tocó iPhone, relanzó, preparó16, instaló o cambió ProjectSettings.

Entrega en PR #31 para Claude/PO; PR #30/#31 sin fusionar. S3 sigue solo plan; S3–S4/T8sin implementación. La mira permanece en frontis, regla y valores de producto pendientes de elección.
