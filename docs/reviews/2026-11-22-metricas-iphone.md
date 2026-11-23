# Resumen de métricas del iPhone

Fuente: `install-518f9c3-20261122` → `backup-before-install/metrics`. 31 archivos JSONL; 26179 eventos. Lectura del respaldo existente.

Rango de eventos (UTC): 2026-11-16T14:38:35.1820960Z — 2026-11-21T15:35:31.1953000Z. La fecha del respaldo no implica que haya juego ese día.

**Este respaldo no tiene eventos del 22 de noviembre de 2026 ni de la build `518f9c3`.** Es una primera corrida histórica de RAL-193 sobre el respaldo extraído el 22; la última build registrada es `0.1.0+a7b3311 dev`.

Builds de origen (campo `build`; el nombre de la carpeta no identifica la build de los eventos):

| Build | Primer evento UTC | Último evento UTC |
|---|---|---|
| 0.1.0+22bb2d5 dev | 2026-11-16T14:38:35.1820960Z | 2026-11-16T14:50:48.1023000Z |
| 0.1.0+a1fa915 dev | 2026-11-16T18:04:30.3556570Z | 2026-11-17T00:10:07.8117170Z |
| 0.1.0+e47de98 dev | 2026-11-17T00:10:17.4499500Z | 2026-11-17T00:29:20.8576780Z |
| 0.1.0+2b82701 dev | 2026-11-17T12:20:01.7665120Z | 2026-11-17T22:48:13.0958590Z |
| 0.1.0+c6f9cef dev | 2026-11-18T00:42:33.3560790Z | 2026-11-18T01:08:40.6053680Z |
| 0.1.0+923daa3 dev | 2026-11-18T13:09:01.9755760Z | 2026-11-18T21:31:05.4028020Z |
| 0.1.0+d11f3d9 dev | 2026-11-19T23:24:51.1416600Z | 2026-11-20T01:14:34.5000960Z |
| 0.1.0+fb80015 dev | 2026-11-20T10:21:18.2380950Z | 2026-11-20T12:01:04.8382370Z |
| 0.1.0+aba4e2c dev | 2026-11-20T21:28:53.3746830Z | 2026-11-21T01:05:33.9620120Z |
| 0.1.0+fceb3de dev | 2026-11-21T10:51:55.9440810Z | 2026-11-21T12:09:21.4884600Z |
| 0.1.0+a7b3311 dev | 2026-11-21T15:35:14.2564710Z | 2026-11-21T15:35:31.1953000Z |

Cobertura: metricsValid=false: 10624 eventos. Se conservan en este resumen de uso; no se declara una validación de física o rendimiento. La retención del writer es limitada; estos recuentos corresponden solo a los archivos conservados.

Inicios de partido: 45; cierres normales: 19; abandonos: 18; sin cierre: 8. Resultados de los cierres: abandoned=18, loss=10, win=9.

Cada fila corresponde a un matchId; la duración usa exclusivamente `match-end.durationSeconds` (tiempo real transcurrido, incluidas pausas). Sin cierre, Duración: no registrado. No se reconstruye a partir de los puntos o las marcas de fecha.

| Partido | Inicio UTC | Build | Resultado | Marcador humano–rival | Duración |
|---|---|---|---|---|---|
| 1 | 2026-11-17T12:20:11.0674470Z | 0.1.0+2b82701 dev | abandoned | 0–2 | 411.66 s |
| 2 | 2026-11-17T12:27:05.4299850Z | 0.1.0+2b82701 dev | abandoned | 2–3 | 6271.60 s |
| 3 | 2026-11-17T14:11:43.1729500Z | 0.1.0+2b82701 dev | loss | 4–7 | 163.58 s |
| 4 | 2026-11-17T14:28:32.6209820Z | 0.1.0+2b82701 dev | win | 7–5 | 235.21 s |
| 5 | 2026-11-17T16:52:38.6700370Z | 0.1.0+2b82701 dev | no registrado | no registrado | no registrado |
| 6 | 2026-11-17T22:48:05.5668700Z | 0.1.0+2b82701 dev | no registrado | no registrado | no registrado |
| 7 | 2026-11-18T00:42:38.1048520Z | 0.1.0+c6f9cef dev | abandoned | 0–2 | 35.53 s |
| 8 | 2026-11-18T00:43:14.9248380Z | 0.1.0+c6f9cef dev | abandoned | 0–1 | 8.57 s |
| 9 | 2026-11-18T00:43:23.4913160Z | 0.1.0+c6f9cef dev | abandoned | 0–0 | 1.75 s |
| 10 | 2026-11-18T00:44:04.0127990Z | 0.1.0+c6f9cef dev | abandoned | 0–2 | 177.43 s |
| 11 | 2026-11-18T00:47:02.6450250Z | 0.1.0+c6f9cef dev | abandoned | 4–4 | 175.11 s |
| 12 | 2026-11-18T00:58:18.8227500Z | 0.1.0+c6f9cef dev | win | 7–5 | 302.15 s |
| 13 | 2026-11-18T01:03:30.7190320Z | 0.1.0+c6f9cef dev | loss | 4–7 | 304.69 s |
| 14 | 2026-11-18T13:24:03.8620390Z | 0.1.0+923daa3 dev | abandoned | 0–3 | 28.33 s |
| 15 | 2026-11-18T13:24:42.8720470Z | 0.1.0+923daa3 dev | no registrado | no registrado | no registrado |
| 16 | 2026-11-18T20:52:52.0736650Z | 0.1.0+923daa3 dev | abandoned | 0–5 | 2148.41 s |
| 17 | 2026-11-18T21:28:42.6520430Z | 0.1.0+923daa3 dev | abandoned | 0–2 | 12.33 s |
| 18 | 2026-11-18T21:29:29.8988330Z | 0.1.0+923daa3 dev | abandoned | 0–2 | 95.50 s |
| 19 | 2026-11-19T23:28:29.8975120Z | 0.1.0+d11f3d9 dev | loss | 5–7 | 288.29 s |
| 20 | 2026-11-19T23:33:21.6627520Z | 0.1.0+d11f3d9 dev | loss | 0–7 | 72.37 s |
| 21 | 2026-11-19T23:34:40.0484510Z | 0.1.0+d11f3d9 dev | abandoned | 1–1 | 25.94 s |
| 22 | 2026-11-20T01:02:06.5188090Z | 0.1.0+d11f3d9 dev | loss | 1–7 | 78.85 s |
| 23 | 2026-11-20T01:09:22.5160650Z | 0.1.0+d11f3d9 dev | win | 7–0 | 101.37 s |
| 24 | 2026-11-20T01:11:13.3918870Z | 0.1.0+d11f3d9 dev | abandoned | 0–2 | 33.79 s |
| 25 | 2026-11-20T01:11:49.6097170Z | 0.1.0+d11f3d9 dev | abandoned | 0–0 | 3.00 s |
| 26 | 2026-11-20T01:11:54.5268780Z | 0.1.0+d11f3d9 dev | abandoned | 0–1 | 7.99 s |
| 27 | 2026-11-20T01:14:02.7772030Z | 0.1.0+d11f3d9 dev | no registrado | no registrado | no registrado |
| 28 | 2026-11-20T10:21:29.1033360Z | 0.1.0+fb80015 dev | loss | 0–7 | 87.97 s |
| 29 | 2026-11-20T10:22:58.3033600Z | 0.1.0+fb80015 dev | loss | 4–7 | 126.71 s |
| 30 | 2026-11-20T10:25:06.4932610Z | 0.1.0+fb80015 dev | win | 7–4 | 124.38 s |
| 31 | 2026-11-20T10:27:12.4103790Z | 0.1.0+fb80015 dev | loss | 5–7 | 186.38 s |
| 32 | 2026-11-20T10:30:25.2660130Z | 0.1.0+fb80015 dev | loss | 5–7 | 193.44 s |
| 33 | 2026-11-20T10:36:57.7656310Z | 0.1.0+fb80015 dev | win | 7–5 | 272.67 s |
| 34 | 2026-11-20T12:00:12.0201290Z | 0.1.0+fb80015 dev | no registrado | no registrado | no registrado |
| 35 | 2026-11-20T21:28:59.8366980Z | 0.1.0+aba4e2c dev | abandoned | 2–5 | 104.92 s |
| 36 | 2026-11-20T21:46:18.1328230Z | 0.1.0+aba4e2c dev | win | 7–4 | 133.63 s |
| 37 | 2026-11-20T22:15:39.2004200Z | 0.1.0+aba4e2c dev | loss | 4–7 | 130.72 s |
| 38 | 2026-11-20T22:32:13.6215310Z | 0.1.0+aba4e2c dev | no registrado | no registrado | no registrado |
| 39 | 2026-11-20T22:53:54.7835390Z | 0.1.0+aba4e2c dev | win | 7–3 | 164.05 s |
| 40 | 2026-11-21T01:05:00.7770910Z | 0.1.0+aba4e2c dev | abandoned | 0–1 | 25.73 s |
| 41 | 2026-11-21T10:51:58.9997070Z | 0.1.0+fceb3de dev | abandoned | 0–1 | 1414.66 s |
| 42 | 2026-11-21T11:15:34.4390490Z | 0.1.0+fceb3de dev | no registrado | no registrado | no registrado |
| 43 | 2026-11-21T11:51:46.8425480Z | 0.1.0+fceb3de dev | win | 7–2 | 343.64 s |
| 44 | 2026-11-21T12:05:54.5365150Z | 0.1.0+fceb3de dev | win | 10–8 | 203.32 s |
| 45 | 2026-11-21T15:35:21.8669570Z | 0.1.0+a7b3311 dev | no registrado | no registrado | no registrado |

Porcentajes sobre contactos aplicados (`shot.Applied=true`); se excluyen órdenes, presentaciones, fallos y recepciones sin golpe. Incluye práctica y partidos; los eventos shot no llevan matchId.

Perfectos: 37.19 % (325/874 contactos del humano con calidad registrada); sin calidad: 0 (humano).
Perfectos: 48.40 % (318/657 contactos del rival con calidad registrada); sin calidad: 0 (rival).

Voleas humanas: 31.25 % (140/448 contactos humanos con modalidad registrada); sin modalidad registrada: 426/874. Se une shot con shot-opportunity-order por archivo, launchId y attemptId; una oferta o un toque no cuentan como volea realizada. Voleas del rival y porcentaje global: no registrado.

Ganador de cada punto: 310 eventos `rally-point`. H=humano, R=rival, N=sin ganador. Numeración local por archivo, en orden de escritura. No se asigna un matchId a los puntos porque el writer no lo guarda.

| Archivo (fecha y hora UTC de apertura) | Punto:ganador |
|---|---|
| 20261116T143835164 | 1:R, 2:R, 3:R |
| 20261116T144835068 | 1:R, 2:H, 3:R, 4:H, 5:R, 6:H, 7:R, 8:R, 9:H, 10:H, 11:H, 12:H |
| 20261116T180430333 | 1:R, 2:R, 3:R |
| 20261116T182922435 | 1:R, 2:R, 3:R |
| 20261116T212729177 | 1:R, 2:R, 3:H, 4:H, 5:H, 6:R, 7:R, 8:R, 9:R |
| 20261117T000939788 | 1:R |
| 20261117T001017433 | 1:R |
| 20261117T002522300 | 1:R, 2:H, 3:H, 4:H, 5:R, 6:H, 7:R, 8:R, 9:R, 10:R, 11:H |
| 20261117T122001761 | 1:R, 2:R, 3:R, 4:R, 5:R, 6:H, 7:H, 8:R, 9:R, 10:R, 11:R, 12:H, 13:H, 14:R, 15:H, 16:R, 17:H, 18:R, 19:H, 20:H, 21:H, 22:R, 23:H, 24:H, 25:H, 26:R, 27:R, 28:R, 29:R, 30:H |
| 20261117T165234815 | 1:R, 2:R, 3:H |
| 20261118T004233349 | 1:R, 2:R, 3:R, 4:R, 5:R, 6:H, 7:R, 8:H, 9:R, 10:R, 11:H, 12:R, 13:H, 14:H, 15:R, 16:H, 17:R, 18:R, 19:H, 20:H, 21:H, 22:R, 23:H, 24:R, 25:H, 26:H, 27:R, 28:R, 29:R, 30:H, 31:R, 32:R, 33:H, 34:R, 35:H, 36:R |
| 20261118T130901969 | 1:R, 2:R, 3:R, 4:R, 5:R, 6:R |
| 20261118T205249598 | 1:R, 2:R, 3:R, 4:R, 5:R, 6:R, 7:R, 8:R, 9:R |
| 20261119T232451136 | 1:R, 2:R, 3:R, 4:R, 5:H, 6:H, 7:H, 8:H, 9:H, 10:R, 11:R, 12:R, 13:R, 14:R, 15:R, 16:R, 17:R, 18:R, 19:R, 20:R, 21:H |
| 20261120T010202037 | 1:R, 2:R, 3:R, 4:R, 5:H, 6:R, 7:R, 8:R, 9:H, 10:H, 11:H, 12:H, 13:H, 14:H, 15:H, 16:R, 17:R, 18:R, 19:H |
| 20261120T102118232 | 1:R, 2:R, 3:R, 4:R, 5:R, 6:R, 7:R, 8:R, 9:R, 10:R, 11:H, 12:H, 13:R, 14:H, 15:R, 16:H, 17:R, 18:R, 19:R, 20:H, 21:R, 22:H, 23:R, 24:H, 25:H, 26:R, 27:H, 28:H, 29:H, 30:R, 31:H, 32:H, 33:R, 34:R, 35:R, 36:H, 37:R, 38:R, 39:H, 40:H, 41:R, 42:H, 43:H, 44:R, 45:R, 46:R, 47:R, 48:H, 49:H, 50:H, 51:R, 52:R, 53:R |
| 20261120T103647667 | 1:R, 2:H, 3:H, 4:H, 5:H, 6:R, 7:R, 8:H, 9:R, 10:H, 11:R, 12:H |
| 20261120T120010451 | 1:R, 2:H, 3:R, 4:R |
| 20261120T212853369 | 1:R, 2:R, 3:R, 4:H, 5:R, 6:H, 7:R |
| 20261120T214614325 | 1:H, 2:R, 3:R, 4:H, 5:R, 6:H, 7:H, 8:H, 9:R, 10:H, 11:H |
| 20261120T221517264 | 1:R, 2:H, 3:R, 4:H, 5:R, 6:R, 7:H, 8:H, 9:R, 10:R, 11:R |
| 20261120T223211310 | 1:R, 2:H, 3:R, 4:R |
| 20261120T225338522 | 1:H, 2:H, 3:H, 4:H, 5:R, 6:H, 7:H, 8:R, 9:R, 10:H |
| 20261121T010458957 | 1:R |
| 20261121T105155938 | 1:R, 2:R, 3:R |
| 20261121T115138165 | 1:H, 2:R, 3:H, 4:H, 5:H, 6:H, 7:R, 8:H, 9:H, 10:H, 11:H, 12:H, 13:H, 14:R, 15:H, 16:R, 17:R, 18:H, 19:R, 20:R, 21:R, 22:R, 23:H, 24:R, 25:H, 26:H, 27:H |

Totales de ganadores: Human=126, Rival=184.

Devoluciones de saque: solo puntos cerrados que registran `server`. Se toma la primera traza causal del receptor y se une con shot por archivo, launchId, attemptId y contactStep. Una traza no demuestra por sí sola que el golpe se aplicó. Sin traza, la devolución es no registrado; no se deduce del ganador del punto.

Humano ante saque rival: 151 saques; con traza de devolución: 132. Sin traza de devolución: 19. Aplicación: aplicado=132. Calidad: Perfect=58, Valid=74. Modalidad: Bounce=56, Volley=15, no registrado=61.
Rival ante saque humano: 159 saques; con traza de devolución: 159. Sin traza de devolución: 0. Aplicación: aplicado=122, no registrado=37. Calidad: Perfect=77, Valid=45, no registrado=37. Modalidad: no registrado=159.

Servidor no registrado en 0 puntos. No hay etiqueta de tipo de devolución (drive, revés, etc.) en estos eventos; no registrado.

Energía: 234 eventos `fatigue` frente a 310 puntos. Solo muestras de puntos cerrados; no se estima la energía durante intervalos sin registro. Unidades de energía 0–100; mediana calculada sobre cada campo presente.

| Jugador | Mínimo observado (Minimum) | Mediana inicial (Start) | Mediana final (End) | Mediana tras recuperar (AfterRecovery) |
|---|---|---|---|---|
| Humano | 0.00 (n=234) | 86.14 (n=234) | 80.44 (n=234) | 82.36 (n=234) |
| Rival | 38.34 (n=234) | 90.26 (n=234) | 86.21 (n=234) | 87.98 (n=234) |

Formato contrastado con `Assets/SquashClash/Core/Diagnostics/MetricsLog.cs`, `UserMetrics.cs`, `Core/Shot/ShotRecord.cs`, `Core/Rally/Fatigue.cs`, `Runtime/Rally/RallyDirector.cs` y `Runtime/Shot/ShotController.cs` (base 518f9c3). El payload es un objeto JSON escapado dentro de cada línea JSONL. Los campos ausentes no se completan con los defaults actuales.

Reproducción: `python3 -B scripts/metrics-summary.py RUTA_AL_RESPALDO` genera el cuerpo de este informe por stdout; la aclaración de alcance anterior y esta nota de validación se añaden a la entrega. Acepta la raíz de evidencia, `backup-before-install` o la carpeta `metrics`; selecciona una sola copia de los datos para no duplicar el antes y el después de una instalación.

Validación: `python3 -B -m unittest discover -s scripts/tests -p test_metrics_summary.py`: **6/6 pruebas**, Python 3.9.6. Se verifica el formato de fecha Unity de siete decimales, conservación de la fecha original, denominadores y campos ausentes, uniones limitadas al archivo, selección de la copia previa, rechazo de entradas corruptas o ambiguas y preservación de los bytes de entrada. El parser trunca a seis decimales únicamente para validar con `fromisoformat`; el informe conserva los siete originales. Respaldo completo: **43 archivos idénticos por SHA-256 antes y después**. Sin acceso al teléfono ni ejecución de Unity. Revisión propia de Codex, no independiente; sin cambios al producto, presets o criterios.
