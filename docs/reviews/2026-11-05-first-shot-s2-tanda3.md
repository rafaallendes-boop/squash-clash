# First Shot S2 — 2.7 tanda 3 dev: CAM-F y CAM-S

## Alcance y preparación

El PO reanudó la tanda 3 el 5 de noviembre de 2026 (America/Santiago, UTC−03:00) en [PR #30](https://github.com/rafaallendes-boop/squash-clash/pull/30). Pidió el arranque nuevo con P0 y cero pendientes, precheck completo, preguntas literales en un modal con un campo por pregunta antes de resultados, y restauración/cotejo final. Se siguieron la [predeclaración §§2–8](../validation/2026-10-31-first-shot-s2-session27-predeclaration.md) y los [hashes externos congelados](../validation/2026-10-31-first-shot-s2-session27-hashes.md). Se usó la app instalada `0.1.0+2ef4807 dev`, fuente de producto `9131b1c`, Release ARM64. Tanda 2 conservada y aprobada por Claude en `0cc8060`.

Verificación del perfil incorporado a las 12:46:42.605151Z. Firma estricta válida, bundle/equipo/dispositivo/entitlement cotejados. Ventana del 31 de octubre 17:58:46Z al **7 de noviembre 17:58:46Z (14:58:46 Chile)**. Quedaban 53,2 h al precheck, suficientes para la reserva de 60 min de tanda 3 con restauración. El acceso y los identificadores de firma/dispositivo quedan en evidencia local ignorada.

Se exportó `before-session` antes del primer reinicio y se verificaron los tres archivos contra la referencia inicial. Se cargó B desde P0 en DEV sin guardar y se aplicó SL1 como ancla antes de cada bloque. El hash completo de 106 valores coteja el cuerpo nominal (3,2; 5,5) m con la representación exacta del manifiesto, control 2, P2, V100, T3 no suave, tempo 0,65, cámara lenta 1 y repetición práctica 1,5 s. P0 conserva su superposición de 41 claves, proyección `8de3f2ecb217` y SimulationKey `166a96e319c7`. La única diferencia entre condiciones es `camera.mode`. Se comprobó que había plan, frontis y primer bote, DEV abierto, Jugando y cero pendientes antes de la entrega de cada bloque. El pulgar izquierdo y la potencia media fueron la tarea de ambos bloques, con C como referencia verbal. Antes de CAM-S se indicó literalmente «La mira puede cambiar de píxel al moverse la cámara, aunque el objetivo del muro quede fijo».

Entre 23 y 24, durante la preparación y antes de cambiar la cámara o pedir el cierre de DEV, Duplicación mostró «iPhone en uso» (13:22:16Z en el Mac). Se conservó la preparación en `interrupted-24-preparation`, con cotejo de los tres archivos. Al recuperar Duplicación, se aplicó §3.2 por precaución y se abrió un proceso nuevo a las 13:25:00.518631Z con P0 `dd122831f592` y cero pendientes. Se repitió el precheck desde B, con el cotejo previo a aplicar SL1. Al seleccionar seguimiento, el control inmediato mostró `398bae34bb81` mientras el lanzamiento activo aún tenía el hash de B. «Aplicar y relanzar» fijó el hash de la nueva recepción antes de entregar el 24. La interrupción fue de preparación. Ambos bloques medidos conservaron su propio proceso y cuota. La evidencia no permite atribuir la causa del aviso de Duplicación.

## Preguntas y respuestas

Modal de CAM-F enviado 2026-11-05T12:53:54Z, recibido 2026-11-05T13:19:10.934286+00:00. Modal de CAM-S y cierre S1 enviado 2026-11-05T13:30:32.082761+00:00, recibido 2026-11-05T13:34:41.447690+00:00. Cada literal tuvo su campo y se recogió antes de mostrar resultados. El envío agrupado en modal se hizo por petición del PO. «Todas ok» y «Todo ok» son respuestas globales. En los 20 ordinales el feedback individual queda «sin respuesta».

| Pregunta literal | 23 CAM-F | 24 CAM-S |
|---|---|---|
| Por número de recepción, ¿qué excepciones recuerdas: Apoyo sin querer, No fijó / falló u otra dificultad? | «Todas ok» | «Todo ok» |
| ¿Seguiste la tarea y la intensidad que se pidió en este bloque? Si no, ¿qué intentabas hacer? | «si» | «si» |
| ¿la pelota va donde apunto? | «si» | «si» |
| ¿el timing es justo? | «si» | «si» |
| ¿el salto es aceptable? | «si» | «si» |

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

**Preferencia de dirección.** «Con el golpe real, ¿sigues prefiriendo tirar hacia arriba o adelante? ¿Por qué?»

> Creo que ahora me gusta como está, ya que apunto donde quiero tirar la pelota y tenso lo necesario, me gustaría probar en u npartido

Las dos duraciones declaradas son estimaciones del PO. El recorrido físico usó solo tirón descendente. El PO calificó el gesto como natural y pidió verlo en un partido. La sesión midió práctica SL1 y el rally sigue por observar.

## Resultados

**20/20 recepciones completas, 20 Swing, 14 contactos aplicados (11 Valid y 3 Perfect), 6 fallos de ventana y 4 gestos extra fuera de cuota**. Cada cámara tuvo 7 contactos sobre 10. Sin recepciones omitidas, cancelaciones o reapoyos en la cuota. El PO declaró haber seguido la intensidad. La potencia registrada clasificó 4/10 Swing en banda media en CAM-F y 2/10 en CAM-S, sin saturación. Se conservan las dos lecturas.

Cada cierre se asoció al único evento DEV de su proceso. Las horas Mac del pedido y de la confirmación de término son marcas secundarias, con latencia de escritura y desfase entre relojes desconocidos. No hubo un acuse separado de cierre de DEV. La línea de la app y su reloj monótono fijan la frontera. El vuelo en curso al cerrar y los lanzamientos después de cuota quedan en preparación.

### 23 CAM-F

Proceso nuevo `metrics-20261105T124706699-321b02b08fbd47aea5a11a7c05bb1440.jsonl`, arranque P0 a las 2026-11-05T12:47:06.7048050Z. Controles aceptados con foco, en juego y DEV abierto: `dev-toggle` L27 (2026-11-05T12:47:29.3054100Z), `dev-tab-presets-scroll` L40 (2026-11-05T12:47:39.5978170Z), `preset-load` L60 (2026-11-05T12:47:50.5187000Z), `apply-relaunch` L96 (2026-11-05T12:48:21.6625770Z), `dev-tab-parameters-scroll` L100 (2026-11-05T12:48:21.7738300Z).
Precheck con 125 líneas, último plan L125/LaunchId 26. Hash externo, efectivo, plan y gestos de cuota `dd122831f592`, cero pendientes. Mₛ Mac 2026-11-05T12:48:51.327198+00:00.
Frontera única DEV L266, 2026-11-05T12:51:11.1801740Z, `timeReal=246.57022696168482`, resultado `completed`, `accepted=true`, `devOpen=false`, `focused=true`, `paused=false`. Cuota LaunchId 61–70, diez iniciadas y completas, cero interrumpidas. El plan siguiente es L342/LaunchId 71.
Cierre del PO «listo termine 23», Mᵣ Mac 2026-11-05T12:52:50.118622+00:00. Export `after-23` terminado 2026-11-05T12:52:55.271153+00:00. SHA-256 del JSONL primario `0993dabb6c154a162325c7048bf81f9effafa47d34d92e574ce8fcbfdc9d7663`.

| R | LaunchId | Línea plan/final | Versión final | Resultado / motivo | P (%) de Swing | Aplicado | Gestos (línea, attemptId, terminal) |
|---:|---:|---|---:|---|---:|---|---|
| 1 | 61 | 272/276 | 2 | Valid / applied | 67.227930 | Sí | L273 `22e1984ea1a24c12b09dcde60febbab2` Swing |
| 2 | 62 | 279/283 | 2 | Valid / applied | 70.239806 | Sí | L280 `d4ce817847f6412c8acb2add64f6325e` Swing |
| 3 | 63 | 286/290 | 2 | Failed / outside-v | 51.499337 | No | L287 `8538945687664bdabeeb0328a9f390ac` Swing |
| 4 | 64 | 293/297 | 2 | Valid / applied | 67.060620 | Sí | L294 `8c2e3743074c4d7ca041604f07ba7b15` Swing |
| 5 | 65 | 300/304 | 2 | Failed / outside-v | 59.196329 | No | L301 `456ec64bd4c849828ad5272be98bb549` Swing |
| 6 | 66 | 307/311 | 2 | Valid / applied | 61.371517 | Sí | L308 `540e925261514dc7821eee90cd27b46a` Swing |
| 7 | 67 | 314/318 | 2 | Failed / outside-v | 58.025050 | No | L315 `6490395d53a04f2e80679d7523f4f65d` Swing |
| 8 | 68 | 321/325 | 2 | Valid / applied | 66.893280 | Sí | L322 `aca83fa2438841dc81bc0f1c1f215bf3` Swing |
| 9 | 69 | 328/332 | 2 | Valid / applied | 76.932794 | Sí | L329 `cc7ad604d3aa4015aefcc14d74e3f96a` Swing |
| 10 | 70 | 335/339 | 2 | Perfect / applied | 54.343849 | Sí | L336 `e47a7281440643b9bacf54c0bd21a825` Swing |

**10 Swing / 10 recepciones, 7 contactos**, 1 con flag Scheduled en la versión final. Resultados 3 Failed, 1 Perfect, 6 Valid. Terminales de gesto 10 Swing. Reapoyos enlazados 0. Muestras crudas 410, O2 0.
Banda solicitada media. Entre 10 Swing: 0 suaves, 4 medios, 6 fuertes y 0 saturados. Entre 7 contactos: 0 suaves, 1 medios, 6 fuertes y 0 saturados. Total de gestos de cuota 10.
Gestos extra por `launchSerial = LaunchId − 1`: 0 antes de frontera y 4 después de cuota. LaunchId 71 L343 Swing, LaunchId 72 L350 Swing, LaunchId 73 L357 Swing, LaunchId 74 L364 Swing.

| Métrica cruda | n | Mínimo | p50 | p95 | p99 | Máximo |
|---|---:|---:|---:|---:|---:|---:|
| Lanzamiento → frontis (ms) | 10 | 715.897436 | 715.897436 | 715.897436 | 715.897436 | 715.897436 |
| Lanzamiento → primer bote previsto (ms) | 10 | 1741.538813 | 1741.538813 | 1741.538813 | 1741.538813 | 1741.538813 |
| Lanzamiento → K entrada (ms) | 10 | 1757.497300 | 1757.497300 | 1757.497300 | 1757.497300 | 1757.497300 |
| Lanzamiento → K salida (ms) | 10 | 2031.727051 | 2031.727051 | 2031.727051 | 2031.727051 | 2031.727051 |
| Lanzamiento → T* (ms) | 10 | 2025.641026 | 2025.641026 | 2025.641026 | 2025.641026 | 2025.641026 |
| Began desde lanzamiento, Swing (ms) | 10 | 318.772539 | 444.374206 | 510.463705 | 510.463705 | 510.463705 |
| T* menos Began, Swing (ms) | 10 | 1515.177321 | 1573.592154 | 1706.868487 | 1706.868487 | 1706.868487 |
| Ended original desde lanzamiento, Swing (ms) | 10 | 1856.376830 | 1939.866581 | 1997.554329 | 1997.554329 | 1997.554329 |
| Ended corregido desde lanzamiento, Swing (ms) | 10 | 1856.376830 | 1939.866581 | 1997.554329 | 1997.554329 | 1997.554329 |
| Δ, Swing (ms) | 10 | -169.264196 | -85.774445 | -28.086696 | -28.086696 | -28.086696 |
| Began → Ended, Swing (ms) | 10 | 1404.327958 | 1479.355042 | 1637.757958 | 1637.757958 | 1637.757958 |
| P, Swing (%) | 10 | 51.499337 | 61.371517 | 76.932794 | 76.932794 | 76.932794 |
| Descenso al soltar, Swing (mm estimados) | 10 | 18.994781 | 22.252602 | 27.387821 | 27.387821 | 27.387821 |
| Descenso máximo, Swing (mm estimados) | 10 | 19.767826 | 22.307825 | 27.608694 | 27.608694 | 27.608694 |
| Desplazamiento lateral firmado, Swing (mm estimados) | 10 | -2.374348 | -1.104348 | -0.276087 | -0.276087 | -0.276087 |
| Aim de Began a C, referencia verbal sin marcador (m) | 10 | 0.282964 | 0.532146 | 0.889024 | 0.889024 | 0.889024 |
| Planificador (ms, uno por lanzamiento) | 10 | 0.089800 | 0.108900 | 2.417300 | 2.417300 | 2.417300 |
| Edad de orden, aplicadas (ms) | 7 | 18.107795 | 26.350046 | 38.577754 | 38.577754 | 38.577754 |
| Cuantización firmada, aplicadas (ms) | 7 | -5.986118 | 0.000000 | 3.546880 | 3.546880 | 3.546880 |
| Salto, aplicadas (m) | 7 | 0.001865 | 0.167312 | 0.331474 | 0.331474 | 0.331474 |
| Salto, aplicadas con píxeles disponibles (px) | 7 | 0.119880 | 17.876156 | 34.755219 | 34.755219 | 34.755219 |
| Salto, aplicadas (diámetros) | 7 | 0.046633 | 4.182808 | 8.286846 | 8.286846 | 8.286846 |
| Solver, aplicadas (ms) | 7 | 0.025700 | 0.033600 | 0.046000 | 0.046000 | 0.046000 |
| Reconstrucción, aplicadas (ms) | 7 | 0.003100 | 0.004800 | 0.018700 | 0.018700 | 0.018700 |
| Predicción, aplicadas (ms) | 7 | 0.044300 | 0.048500 | 0.054500 | 0.054500 | 0.054500 |
| Reconciliación, aplicadas (ms) | 7 | 0.022500 | 0.030500 | 2.497200 | 2.497200 | 2.497200 |

Fuente de DPI de Swing: `Reported` (10). Sin Aim: 0/10 gestos. Sin píxeles de salto: 0/7 contactos.

| LaunchId | Aim (m) | TargetUsed (m) | Contacto (m) | Velocidad (m/s) | SpeedRaised / TargetLowered | Paso contacto / T* | Vecino legal | Resimulados / obsoletos |
|---:|---|---|---|---:|---|---|---|---|
| 61 | (3.412767, 2.793501) | (3.412767, 2.793501) | (2.600000, 0.313514, 6.202841) | 20.790146 | False / False | 151 / 158 | False | 2 / 0 |
| 62 | (3.329407, 2.954176) | (3.329407, 2.954176) | (2.600000, 0.340546, 6.292926) | 21.452757 | False / False | 153 / 158 | False | 1 / 0 |
| 64 | (3.211716, 3.088947) | (3.211716, 3.088947) | (2.600000, 0.353040, 6.337968) | 20.753336 | False / False | 154 / 158 | False | 2 / 0 |
| 66 | (3.487062, 2.391380) | (3.487062, 2.391380) | (2.600000, 0.313514, 6.202841) | 19.501734 | False / False | 151 / 158 | False | 1 / 0 |
| 68 | (3.535030, 2.378660) | (3.535030, 2.378660) | (2.600000, 0.340546, 6.292926) | 20.716522 | False / False | 153 / 158 | False | 1 / 0 |
| 69 | (3.459193, 2.904902) | (3.459193, 2.904902) | (2.600000, 0.313514, 6.202841) | 22.925215 | False / False | 151 / 158 | False | 2 / 0 |
| 70 | (3.616928, 2.530681) | (3.616928, 2.530681) | (2.600000, 0.396205, 6.518138) | 17.955647 | False / False | 158 / 158 | False | 0 / 0 |

### 24 CAM-S

Proceso nuevo `metrics-20261105T132500504-91d8f4e3d668441da8992232bf3746d3.jsonl`, arranque P0 a las 2026-11-05T13:25:00.5186310Z. Controles aceptados con foco, en juego y DEV abierto: `dev-toggle` L24 (2026-11-05T13:25:20.5820650Z), `dev-tab-presets-scroll` L28 (2026-11-05T13:25:20.7076650Z), `preset-load` L44 (2026-11-05T13:25:28.2922920Z), `apply-relaunch` L91 (2026-11-05T13:26:08.3418210Z), `dev-tab-parameters-scroll` L95 (2026-11-05T13:26:08.4535340Z), `params-Camera` L113 (2026-11-05T13:26:20.8298890Z), `camera.mode` L144 (2026-11-05T13:26:41.3742080Z), `apply-relaunch` L151 (2026-11-05T13:26:41.5156950Z).
Precheck con 161 líneas, último plan L160/LaunchId 29. Hash externo, efectivo, plan y gestos de cuota `398bae34bb81`, cero pendientes. Mₛ Mac 2026-11-05T13:27:19.583788+00:00.
Frontera única DEV L287, 2026-11-05T13:28:59.8119060Z, `timeReal=241.44777725661905`, resultado `completed`, `accepted=true`, `devOpen=false`, `focused=true`, `paused=false`. Cuota LaunchId 60–69, diez iniciadas y completas, cero interrumpidas. El plan siguiente es L361/LaunchId 70.
Cierre del PO «termine 24!», Mᵣ Mac 2026-11-05T13:30:32.082761+00:00. Export `after-24` terminado 2026-11-05T13:30:37.696180+00:00. SHA-256 del JSONL primario `68d5cbed6c906d823a92a1086cbab937e54ce8bf41150b3a4563099ce4ed3e4d`.

| R | LaunchId | Línea plan/final | Versión final | Resultado / motivo | P (%) de Swing | Aplicado | Gestos (línea, attemptId, terminal) |
|---:|---:|---|---:|---|---:|---|---|
| 1 | 60 | 291/295 | 2 | Failed / after-k-out | 62.710160 | No | L292 `e1f520fbd1d049c1b763b9a1ec31b185` Swing |
| 2 | 61 | 298/302 | 2 | Valid / applied | 62.877446 | Sí | L299 `dde39c5dcb384b8b9978a203347cc121` Swing |
| 3 | 62 | 305/309 | 2 | Failed / outside-v | 62.542796 | No | L306 `00b07a0ac4ed40319896a306fd3699b3` Swing |
| 4 | 63 | 312/316 | 2 | Failed / after-k-out | 75.092232 | No | L313 `b7941d4287734a5ab9fd06a1d93edcb4` Swing |
| 5 | 64 | 319/323 | 2 | Valid / applied | 83.123869 | Sí | L320 `0027ad7f7bf844c58e74db8db8010d25` Swing |
| 6 | 65 | 326/330 | 2 | Perfect / applied | 48.320156 | Sí | L327 `4f8ed659851c4db4a58bd25a1d9d77ab` Swing |
| 7 | 66 | 333/337 | 2 | Valid / applied | 61.706191 | Sí | L334 `75457c597a254dce90acabe88eb36066` Swing |
| 8 | 67 | 340/344 | 2 | Valid / applied | 59.865612 | Sí | L341 `7b1fe97deed143cea8a03a11915412b1` Swing |
| 9 | 68 | 347/351 | 2 | Valid / applied | 64.216053 | Sí | L348 `8f4763a24f1c4266ac589be769fefaee` Swing |
| 10 | 69 | 354/358 | 2 | Perfect / applied | 67.562580 | Sí | L355 `78693af38dfc406daebc980733b0f1b0` Swing |

**10 Swing / 10 recepciones, 7 contactos**, 2 con flag Scheduled en la versión final. Resultados 3 Failed, 2 Perfect, 5 Valid. Terminales de gesto 10 Swing. Reapoyos enlazados 0. Muestras crudas 377, O2 0.
Banda solicitada media. Entre 10 Swing: 0 suaves, 2 medios, 8 fuertes y 0 saturados. Entre 7 contactos: 0 suaves, 2 medios, 5 fuertes y 0 saturados. Total de gestos de cuota 10.
Gestos extra por `launchSerial = LaunchId − 1`: 0 antes de frontera y 0 después de cuota. Ninguno registrado en el export de cierre.

| Métrica cruda | n | Mínimo | p50 | p95 | p99 | Máximo |
|---|---:|---:|---:|---:|---:|---:|
| Lanzamiento → frontis (ms) | 10 | 715.897436 | 715.897436 | 715.897436 | 715.897436 | 715.897436 |
| Lanzamiento → primer bote previsto (ms) | 10 | 1741.538813 | 1741.538813 | 1741.538813 | 1741.538813 | 1741.538813 |
| Lanzamiento → K entrada (ms) | 10 | 1757.497300 | 1757.497300 | 1757.497300 | 1757.497300 | 1757.497300 |
| Lanzamiento → K salida (ms) | 10 | 2031.727051 | 2031.727051 | 2031.727051 | 2031.727051 | 2031.727051 |
| Lanzamiento → T* (ms) | 10 | 2025.641026 | 2025.641026 | 2025.641026 | 2025.641026 | 2025.641026 |
| Began desde lanzamiento, Swing (ms) | 10 | 348.292202 | 431.146327 | 930.867869 | 930.867869 | 930.867869 |
| T* menos Began, Swing (ms) | 10 | 1094.773157 | 1543.729782 | 1677.348824 | 1677.348824 | 1677.348824 |
| Ended original desde lanzamiento, Swing (ms) | 10 | 1894.385119 | 1961.327452 | 2036.225160 | 2036.225160 | 2036.225160 |
| Ended corregido desde lanzamiento, Swing (ms) | 10 | 1894.385119 | 1961.327452 | 2036.225160 | 2036.225160 | 2036.225160 |
| Δ, Swing (ms) | 10 | -131.255907 | -64.313574 | 10.584135 | 10.584135 | 10.584135 |
| Began → Ended, Swing (ms) | 10 | 1104.353000 | 1512.696041 | 1629.441500 | 1629.441500 | 1629.441500 |
| P, Swing (%) | 10 | 48.320156 | 62.710160 | 83.123869 | 83.123869 | 83.123869 |
| Descenso al soltar, Swing (mm estimados) | 10 | 17.945652 | 22.694353 | 29.430876 | 29.430876 | 29.430876 |
| Descenso máximo, Swing (mm estimados) | 10 | 18.111303 | 22.970440 | 30.369564 | 30.369564 | 30.369564 |
| Desplazamiento lateral firmado, Swing (mm estimados) | 10 | -4.141304 | -2.319132 | -1.380435 | -1.380435 | -1.380435 |
| Aim de Began a C, referencia verbal sin marcador (m) | 10 | 0.380880 | 0.533271 | 0.793527 | 0.793527 | 0.793527 |
| Planificador (ms, uno por lanzamiento) | 10 | 0.101800 | 0.130000 | 0.178000 | 0.178000 | 0.178000 |
| Edad de orden, aplicadas (ms) | 7 | 13.650423 | 22.367131 | 46.233548 | 46.233548 | 46.233548 |
| Cuantización firmada, aplicadas (ms) | 7 | -0.888783 | 2.805271 | 3.687256 | 3.687256 | 3.687256 |
| Salto, aplicadas (m) | 7 | 0.011436 | 0.100701 | 0.152815 | 0.152815 | 0.152815 |
| Salto, aplicadas con píxeles disponibles (px) | 7 | 0.635986 | 10.699680 | 15.793210 | 15.793210 | 15.793210 |
| Salto, aplicadas (diámetros) | 7 | 0.285895 | 2.517517 | 3.820377 | 3.820377 | 3.820377 |
| Solver, aplicadas (ms) | 7 | 0.021000 | 0.033600 | 0.063100 | 0.063100 | 0.063100 |
| Reconstrucción, aplicadas (ms) | 7 | 0.003100 | 0.006000 | 0.010000 | 0.010000 | 0.010000 |
| Predicción, aplicadas (ms) | 7 | 0.035400 | 0.044100 | 0.067100 | 0.067100 | 0.067100 |
| Reconciliación, aplicadas (ms) | 7 | 0.022100 | 0.026700 | 0.989000 | 0.989000 | 0.989000 |

Fuente de DPI de Swing: `Reported` (10). Sin Aim: 0/10 gestos. Sin píxeles de salto: 0/7 contactos.

| LaunchId | Aim (m) | TargetUsed (m) | Contacto (m) | Velocidad (m/s) | SpeedRaised / TargetLowered | Paso contacto / T* | Vecino legal | Resimulados / obsoletos |
|---:|---|---|---|---:|---|---|---|---|
| 61 | (3.508837, 2.843102) | (3.508837, 2.843102) | (2.600000, 0.327371, 6.247883) | 19.833038 | False / False | 152 / 158 | False | 1 / 0 |
| 64 | (3.352364, 2.978762) | (3.352364, 2.978762) | (2.600000, 0.327371, 6.247883) | 24.287251 | False / False | 152 / 158 | False | 1 / 0 |
| 65 | (2.980911, 2.793501) | (2.980911, 2.793501) | (2.600000, 0.396205, 6.518138) | 16.630434 | False / False | 158 / 158 | False | 0 / 0 |
| 66 | (3.433205, 2.518065) | (3.433205, 2.518065) | (2.600000, 0.340546, 6.292926) | 19.575363 | False / False | 153 / 158 | False | 1 / 0 |
| 67 | (3.449490, 2.681362) | (3.449490, 2.681362) | (2.600000, 0.353040, 6.337968) | 19.170435 | False / False | 154 / 158 | False | 0 / 0 |
| 68 | (3.409960, 2.568477) | (3.409960, 2.568477) | (2.600000, 0.327371, 6.247883) | 20.127533 | False / False | 152 / 158 | False | 1 / 0 |
| 69 | (3.443589, 2.492804) | (3.443589, 2.492804) | (2.600000, 0.396205, 6.518138) | 20.863768 | False / False | 158 / 158 | False | 0 / 0 |

### Lectura descriptiva y límites

El salto en los contactos tuvo p50 de **4,18 diámetros en fija** y **2,52 en seguimiento**, con máximos de **8,29** y **3,82** diámetros. El PO respondió «si» a la aceptabilidad en ambas condiciones antes de conocer estas cifras. El Aim de Began quedó a una mediana de 0,532 m y 0,533 m de C, referencia verbal sin marcador. Es una distancia al punto de referencia y la intención real del PO queda sin medir. Los 14 contactos conservaron Aim como TargetUsed y los flags de asistencia se registran por golpe. Las dos condiciones son secuenciales, con un solo PO y siete saltos observados por condición. Las diferencias observadas no permiten atribuir un efecto causal a la cámara. El timing es juicio preliminar del PO. No se calibró H3.

Percentiles por rango más próximo, sin interpolación. Los costes de cada etapa usan sus muestras originales. El coste de frame completo asociado al golpe y las asignaciones por parte quedaron NotEvaluated. Térmico, batería y ruta de audio durante los bloques quedaron sin lectura específica. No se ejecutaron escenarios, benchmark ni H17. Los gestos extra se verificaron también hasta la preparación interrumpida de 24 y el export final, sin extras tardíos adicionales.

## Exportación, restauración y parada

Export final `before-final-restore` terminado 2026-11-05T13:30:52.208695+00:00, antes de abrir otro proceso. Arranque nuevo `metrics-20261105T133055456-ac71e48de4b140a7ae683fa6dada1e84.jsonl` a las **2026-11-05T13:30:55.4712260Z (10:30:55 Chile)**, `0.1.0+2ef4807 dev`, **P0-calib-1 `dd122831f592`, cero pendientes**, DEV cerrado, sin overlay y con Pausa visible. `final-restored` confirma por `cmp` los tres archivos contra el respaldo de la tanda y contra la referencia inicial de la tanda 1. Todo quedó dentro del perfil incorporado.

| Archivo | Bytes | SHA-256 tras el arranque | Cotejo con ambas referencias |
|---|---:|---|---|
| `P0-calib-1.json` | 3614 | `2f1295f201e238d9494bee285dd3f46e5bbb0f98b09ccde90fbd268f0874e4fd` | Idéntico |
| `Po-1.json` | 2843 | `9008c33ab89f82e669bc227a81b37ed641f69e4657ee70df6c89ff0c0edb1767` | Idéntico |
| `last-active.txt` | 10 | `5885ce8a7add389b2c82cb43f6944d7e2ef7ebeb0bffd77b5b635361a8a46087` | Idéntico |

Evidencia primaria, copias, SHA, controles y respuestas originales en la ruta local ignorada `Logs/evidence/s2-session27-tanda3-20261105/`. Entrega documental de tanda 3 para Claude/PO, sin build, instalación o cambios de producto. PR #30, #31 y #32 abiertas y sin fusionar. Tanda 4 PROD pendiente de autorización propia para instalar, PO presente, respaldo nuevo y frontera de §5, con perfil vigente hasta el final de la restauración. La siguiente acción sobre el dispositivo espera al PO.

## Revisión de Claude de tanda 3 — aprobación comunicada por el PO

**Aprobada sin correcciones en `a0abbe3` de PR #30.** Claude hizo un recuento independiente desde los JSONL de `after-24`. En 23 CAM-F confirmó 10 Swing (6 Valid, 3 Failed y 1 Perfect), con LaunchId 71–74 como extras. En 24 CAM-S confirmó 10 Swing (5 Valid, 3 Failed y 2 Perfect). Confirmó los hashes `dd122831f592` y `398bae34bb81` respectivamente, con cero pendientes. También coincidieron los SHA-256 de los JSONL, el texto de las preguntas predeclaradas palabra por palabra, las respuestas literales y la restauración de P0.

1. **Potencia media pedida.** Quedaron en banda media 4/10 Swing en CAM-F y 2/10 en CAM-S. Los demás quedaron en banda fuerte (6/10 y 8/10), con cero saturados. El PO declaró haber seguido la intensidad. Se conservan ambas lecturas. La observación se vincula al tema abierto de saturación. `pullFullMm` sigue en 35 mm y el PO aún no ha tomado una decisión al respecto.
2. **Salto.** En fija, p50 4,18 y máximo 8,29 diámetros. En seguimiento, p50 2,52 y máximo 3,82 diámetros. El PO juzgó aceptable el salto en ambas condiciones antes de ver las cifras. Los bloques fueron secuenciales, con siete contactos por condición, de modo que no se atribuye un efecto causal a la cámara.
3. **Conflictos documentales.** PR #31 y #32 tienen conflictos con #30 solo en `AGENTS.md` y aprobaciones. Se resolverán al fusionarlas, después de #30. La sincronización del «Estado vigente» en las tres ramas deja la resolución pendiente.

Registro documental de la revisión, con evidencia y producto intactos. La sesión física sigue detenida. Tanda 4 PROD requiere autorización aparte del PO. Las tres PR siguen abiertas.
