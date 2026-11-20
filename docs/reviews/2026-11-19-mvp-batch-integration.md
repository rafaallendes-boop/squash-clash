# Tanda MVP del 18–19 nov: integración

Rama `claude/mvp-1118-integration`, base `main` `75b9ce8`. Modo product lead (registro del 18 nov): Claude dirigió subagentes Opus y Sonnet y tomó las decisiones técnicas por delegación del PO. **La revisión de Claude no es independiente.** Nada está fusionado en `main` ni instalado.

## Qué entra (una rama por frente, fusionadas sin aplanar)

| Frente | Rama, head | Modelo | Informe |
|---|---|---|---|
| Limpieza de documentos: AGENTS.md pasa de 26 a 8 KB y hay un registro nuevo | `claude/docs-condense` `ab81326` | Sonnet | [condense-report](../history/condense-report-2026-11-18.md) |
| Reajuste del cansancio: `costPerMeter` 0,4→0,3 y `recoverBetween` 0,25→0,08 | `claude/fatigue-retune` `c2a6a4b` | Sonnet | [fatigue-retune](2026-11-18-fatigue-retune.md) |
| «Toc» del vidrio de fondo, interruptor DEV y verificación del triángulo (0,0 cm, sin cambios) | `claude/back-glass-audio` `5fb2d4c` | Sonnet | [back-glass-audio](2026-11-18-back-glass-audio.md) |
| Inicio y retroalimentación de golpe con `/frontend-design` (Rubik OFL) | `claude/menu-visual` `be97aac` | Opus | [menu-visual](2026-11-18-menu-visual.md) |
| Revés reconocible: preparación alta tras el hombro no dominante y remate del lado dominante | `claude/backhand-pose` `3fcd245` | Sonnet | [backhand-pose](2026-11-18-backhand-pose.md) |
| G1 A2 integrada (humano, rival, RallySim y retícula) | `claude/g1-a2-complete` `2b7260b` | Opus | [g1-a2-complete](2026-11-18-g1-a2-complete.md) |
| Volea v1 (solo humano, anillo ámbar, interruptor DEV) | `claude/volley` `3083e8f` | Opus | [volley](2026-11-18-volley.md), [diseño](../superpowers/specs/2026-11-18-volley-design.md) |

## Decisiones de Claude como product lead (con la delegación del PO)

- **Cansancio, B4:** falla con 12/200 partidos (6 %) frente a ≤5 %. Se acepta como desviación sin reajustar: la simulación juega rallies de ~36 m y el juego real de ~18 m. Las compuertas B1, B2, B3, B5a y B6 pasan.
- **G1 A2:**
  - C1 24/24, C2, C4, C6 y C8 pasan (p95 0,53 ms en el editor).
  - **C3 43/48 queda como observación abierta:** 5 subidas de ápice de 0,02–0,08 m, contra +1,7–1,9 m de la primitiva.
  - **Opción B:** la capa causal (timing, estiramiento, cansancio) sigue subiendo la altura. Es una desviación del diseño archivado.
  - Las dos firmas congeladas de fatiga se re-anclan a la referencia RallySim con A2. Se demostró que la diferencia viene solo de A2.
- **Volea:** se aprueban las siete desviaciones de §7.3 del diseño. Claude dirige los pasos de contrato (zona de Codex) por la delegación del 18 nov.
- **Revés:** se invierte una expectativa de E1 (`ZE1StrokePoseTests`). Ahora el rival remata del lado del brazo que golpea.

## Integración

- **Conflictos resueltos conservando ambos lados:**
  - `FatigueArmRallyTests`: valores F1 fijados por el reajuste más la referencia A2.
  - `DevPanelView`: secciones de vidrio y de volea.
  - `ShotNoteView`: aspecto nuevo con el prefijo «Volea · ».
  - `SwingView`: arco del revés más las claves de volea.
  - `Scenario.cs`.
- **Anclas (commit `20a6d18`):** VL1 `59efe2ba2fb2`→`24531a50618a`, medido en Unity sobre `e78c55a` con los defaults reajustados. El literal SL5 T3 del test de volea pasa a `d09119d6321b`. Escenarios, oráculos y checker intactos.

## Compuertas sobre la integración

Todas pasaron por un candado global de un solo Unity.

| Compuerta | Resultado |
|---|---|
| EditMode (`e78c55a`) | 860/860 |
| PlayMode (`e78c55a`) | 640/640 |
| AutoQA V (`20a6d18`) | PASS, exit 0, nivel 1, 18/18, 102 tests, sin dirty (`20261119T044833Z-20a6d18-v-21858`) |
| AutoQA T3 (`20a6d18`) | PASS, exit 0, nivel 1, 18/18, 102 tests, sin dirty (`20261119T044848Z-20a6d18-t3-24951`) |
| AutoQA volea (`20a6d18`) | PASS, exit 0, nivel 1, 18/18, 102 tests, sin dirty (`20261119T044904Z-20a6d18-volley-10781`) |
| check-release dev/prod (`20a6d18`) | OK, build 931 |

`20a6d18` solo cambia dos literales del ensamblado AutoQA, que AutoQA corrió completo.

## Hashes en la integración

- **Cambian por el reajuste del cansancio:** defaults T3 `207da8f2c636` (132 claves; con el cansancio apagado, `ec3ff68ebdfa`), P0 `7a5972dacf1e`, SL5 T3 `d09119d6321b`, V `28e63d17dfbf` y SL5 V `8ec4deffc6c1`.
- **Cambia por la volea:** RallyParams `6a7b329a7b85`; sin los cinco campos `Volley*` da `927e304bbfec`.
- **Sin cambio:** SimulationKey `166a96e319c7`.
- **Ancla nueva:** VL1 `24531a50618a`.

## Riesgos y pendientes

- **Juicio en el iPhone:**
  - cansancio en juego real;
  - timbre del vidrio;
  - legibilidad de la cursiva y kerning apagado;
  - revés;
  - anillo ámbar y frecuencia de la volea (6 % de las recepciones en la simulación, sin ventaja táctica medida).
- **Dificultad del rival:** en RallySim, A2 favorece a la IA Media (149→160 de 200 puntos).
- **Rendimiento en el teléfono:** sin medir el p95 de cuadro con A2 y con las dos referencias causales de la volea.
- **Experimentos explícitos con hashes viejos:** `VolleyRateExperiment`, `G1A2CompleteExperiment` y `FatigueMatchExperiment` afirman hashes anteriores. Fallarían si se vuelven a correr; están fuera de la regresión.
- **CRP-01:** apareció una vez, intermitente, en la rama del cansancio.
- **Pendientes del PO:** créditos con la licencia OFL antes de publicar; resoplido del rival (después).

## Segunda vuelta (19 nov), pedido del PO

El PO pidió el top 3 del diseñador de juego y tres cambios de volea. Se hicieron en tres ramas desde `7e8c64c` y se fusionaron sin aplanar. Hubo un solo conflicto, en `DevPanelView`, y se conservaron todas las secciones.

| Frente | Rama, head | Modelo | Informe |
|---|---|---|---|
| Tarjeta de resultado (duración m:ss, rally más largo, «Revancha 5–7» solo al perder) y golpe Perfecto propio (más seco, mismo volumen que Válido) | `claude/match-card-perfect-sound` `a81015f` | Sonnet | [informe](2026-11-19-match-card-perfect-sound.md) |
| Congelado de 100 ms del render al cerrar el punto, dentro de la pausa existente | `claude/point-end-freeze` `1314bcd` | Sonnet | [informe](2026-11-19-point-end-freeze.md) |
| Volea en el tutorial (paso 4), estallido ámbar en vez de «Volea · » y costo de energía de 2 puntos por volea | `claude/volley-ux` `4ac0256` | Opus | [informe](2026-11-19-volley-ux.md) |

Cada frente tiene su interruptor DEV, encendido por defecto.

**Decisiones de Claude:**
- **Volumen del Perfecto.** Se toma literal «con el mismo volumen» que el Válido, así que el Perfecto nuevo suena más suave que el de hoy. Si hace falta, se ajusta con una constante de ganancia.
- **Costo de la volea.** Es `RallyParams.VolleyEnergyCost=2`, sin calibrar. Se aplica a cualquier jugador que volee, pero hoy solo volea el humano.
- **Seguimiento de la cámara.** No se congela durante el congelado; solo se retiene su temblor.

**Hashes:**
- RallyParams `6a7b329a7b85`→`5cdbb01e4621`. La proyección sin `VolleyEnergyCost` es exactamente `6a7b329a7b85`.
- Sin cambios: defaults, P0, SimulationKey y las anclas AutoQA.

**Compuertas sobre `8cceeb9`:**

| Compuerta | Resultado |
|---|---|
| EditMode | 870/870 |
| PlayMode | 665/665 |
| AutoQA V | PASS/0/1 (`20261119T100523Z-8cceeb9-v-28041`) |
| AutoQA T3 | PASS/0/1 (`20261119T100538Z-8cceeb9-t3-8419`) |
| AutoQA volea | PASS/0/1 (`20261119T100553Z-8cceeb9-volley-11087`) |
| check-release dev/prod | OK, build 946 |

**Para juzgar en el iPhone:**
- el timbre y el volumen del Perfecto;
- si se siente el congelado;
- si el estallido de la volea se lee bien, ya que se superpone con el destello del Perfecto;
- el costo de la volea;
- la duración real del partido, que la tarjeta muestra.

## Tercera vuelta (19 nov), pedido del PO

El PO pidió tres cosas: estilos de rival (fondista, «one shoter» y «all arround») con volea del rival, un menú principal rediseñado con `/frontend-design` y los puntos que Claude propuso:
- calibrar la dificultad;
- la posición del rival ante golpes cortos;
- el resoplido del rival;
- los créditos OFL.

Se hicieron en tres ramas desde `f068cee` y se fusionaron sin aplanar, sin conflictos.

| Frente | Rama, head | Modelo | Informe |
|---|---|---|---|
| Resoplido del rival cuando se cansa, con interruptor DEV | `claude/rival-breath` `6fa9dc0` | Sonnet | [informe](2026-11-19-rival-breath.md) |
| Menú principal rediseñado y pantalla de créditos | `claude/menu-redesign` `36e3195` | Opus | [informe](2026-11-19-menu-redesign.md) |
| Estilos Fondista, Definidor y Completo, volea y cobertura del rival, y calibración por estilo y nivel | `claude/rival-styles` `521a258` | Opus | [informe](2026-11-19-rival-styles.md), [diseño](../superpowers/specs/2026-11-19-rival-styles-design.md) |

**Decisiones de Claude:**
- **Definidor en la T:** espera en la T y vuelve a ella después de su golpe corto. Sus pesos de ataque y de volea no cambian.
- **Un factor de ruido por estilo y nivel.** Con un solo factor por nivel ninguna k cumplía la banda de ±8 pp, porque los estilos tienen fuerzas distintas. Las metas, fijadas antes de calibrar, no cambian.
  - **Confirmación:** se hizo con semillas nuevas. Ocho de las nueve celdas quedan a ±4 pp de su meta.
  - **Definidor en Difícil:** queda en k=1, la menor dificultad que se calibró. El principiante gana 37,8 % de los puntos frente a una meta de 20 %. Es una desviación registrada.
- **Tres tests de PlayMode:** su presupuesto de puntos suponía al rival antiguo, y uno de ellos chocaba con el congelado. Ahora fijan al Fondista o apagan el congelado. No cambia ningún umbral (ver el informe de estilos).
- **Pendientes de decidir:**
  - en Fácil, `d_perc` llega a 120 ms, sobre el rango de 0–80 ms de §11.2 (marcado EXPERIMENTO);
  - SF-01 aparece en el 0,5–1,5 % de los puntos en Fácil.

**Hashes:**
- RallyParams `5cdbb01e4621`→`0f58580f44ea`. La proyección sin los campos `Style*` es exactamente `5cdbb01e4621`.
- Sin cambios: defaults `207da8f2c636`/132, SimulationKey `166a96e319c7` y las anclas AutoQA.
- `FondoAi` y `DefaultPreset.json` no cambian.

**Compuertas sobre `ecff541`:**

| Compuerta | Resultado |
|---|---|
| EditMode | 883/883 |
| PlayMode | 689/689 |
| AutoQA V | PASS/0/1 (`20261119T214753Z-ecff541-v-31255`) |
| AutoQA T3 | PASS/0/1 (`20261119T214840Z-ecff541-t3-8618`) |
| AutoQA volea | PASS/0/1 (`20261119T214918Z-ecff541-volley-15597`) |
| check-release dev/prod | OK, build 975 |

**Para juzgar en el iPhone:**
- si se distinguen los tres estilos;
- si la dificultad se siente justa;
- si se nota la volea del rival, que todavía no tiene pose propia;
- el resoplido;
- el menú nuevo.

## Cuarta vuelta (20 nov): volea v2

**Pedido del PO, tras jugar `d11f3d9`:** «no podía volear», «La idea es que sea fácil al igual que los golpes», «que se pueda volear aunque te desgaste más».

**Lo que mostraron sus métricas:** recibió 96 pelotas y no se le ofreció ninguna volea. Las causas: separación 40, la pelota no pasó a su alcance 26, saque 19 y ventana corta 11.

**Rama:** `claude/volley-v2`, head `b7ce112`, hecha por un subagente Opus. Tiene su [informe](2026-11-20-volley-v2.md) y su [diseño](../superpowers/specs/2026-11-20-volley-v2-design.md). Se fusionó sobre `9a23e7d`, con un árbol idéntico al del head.

**Qué cambia:**
- El jugador elige la volea: un toque mientras se ve el anillo ámbar lo compromete. Si toca tarde, hace un sprint de hasta 2,0 que cuesta 4 puntos de energía por retraso.
- Una vibración suave avisa cuando se abre la volea.
- El jugador puede volear el saque del rival.
- Interruptores DEV: v1, v2 o v2.2, y la volea del saque.
- Todo es solo para el humano.

**Cómo se llegó a v2.2:**
- **v2:** con los tiempos reales de toque del PO, solo volearía el 6,1 % de las veces.
- **v2.1:** solo sprint, 12 %.
- **v2.2:** puede elegir otra posición más cercana, 28 %. Va por defecto según la regla fijada de antemano.
- **v2.3:** volea en movimiento, 51 %. Pero el principiante simulado acierta 25 puntos menos que en el golpe después del bote, así que no entra.

**Desviaciones:**
- Volea con los tiempos de toque del PO: 28,0 % frente a la meta de ≥40 %.
- Diferencia de aciertos con el golpe después del bote: 12,4 pp frente a la meta de ≤12.
- El sprint de 2,0 equivale a 9,6 m/s, una velocidad poco realista.

**Riesgo de energía:** el bot que volea en cada toque llega al punto 15 con una mediana de 17,6 de energía, frente a 33,3 en v1, y gana el 7 % de los partidos, frente a 29 %. Lo juzga el PO.

**Sin cambios:**
- el rival: `ConfirmPerStyle` da igual que `confirm01`;
- `ContactContract.cs`, DefaultPreset, SimulationKey y VL1.

**Hashes:** RallyParams `0fe523164363`; la proyección sin los campos nuevos da exactamente `0f58580f44ea`.

**Compuertas sobre `4593d2b`:**
- EditMode 897/897, PlayMode 695/695;
- AutoQA V, T3 y volea PASS/0/1;
- check-release dev y prod OK.

**Nota para mejorar el saque:** todas las devoluciones pasan a su alcance y la devolución después del bote sale bien en el 98 % de los casos. El saque se ve blando y alto.

## Quinta vuelta (20 nov): mejorar lo que ya existe

**Pedido del PO, después de jugar `fb80015`:**
- SL5/SL6: «adelante quería tirar una pelota al fondo y se iba por las paredes laterales».
- Energía: «llegó a 0 … el jugador se seguía moviendo igual».
- Prioridad: «Mejorar lo que hay hoy es la prioridad».

**La volea v2.2 en juego real**, en 5 partidos: el PO voleó en 72 de 152 recepciones. Logró Perfecto o Válido en el 75 % de las voleas, frente al 78 % de los golpes después del bote.

| Frente | Rama, head | Modelo | Informe |
|---|---|---|---|
| Golpe al fondo desde adelante: protección contra la pared lateral, solo del humano, con el interruptor DEV «Drive adelante» | `claude/front-deep-drive` `e027ecc` | Opus | [informe](2026-11-20-front-deep-drive.md) |
| El sprint de la volea depende de la energía; «Cansancio fuerte» como opción DEV; cansancio visible | `claude/fatigue-feel` `bab1af8` | Opus | [informe](2026-11-20-fatigue-feel.md) |
| Pose de volea del rival, solo visual | `claude/rival-volley-pose` `0ed2af2` | Sonnet | [informe](2026-11-20-rival-volley-pose.md) |
| Récords personales: línea en la tarjeta y pantalla en el menú | `claude/personal-records` `13b5ba5` | Sonnet | [informe](2026-11-20-personal-records.md) |
| Análisis del golpe a dos paredes, con la decisión del PO | `claude/two-wall-analysis` `d5df9a4` | Sonnet | [análisis](../superpowers/specs/2026-11-20-two-wall-shot-analysis.md) |

**Resultados medidos:**
- **Drive adelante,** sobre golpes reales del PO reproducidos en P2:
  - rectos desde adelante: de 11/16 a 16/16;
  - cruzados: de 3/39 a 37/39;
  - control con golpes del 19–20 nov, que no se usaron para el ajuste: 9/9 y 22/23.
  - **Advertencia:** la medición no es ciega, porque la protección usa la misma prueba de éxito que la métrica.
- **Sprint según energía:**
  - con energía bajo 10, 0,0 % de voleas con sprint, frente a 4,0 % con 70 o más;
  - los aciertos en la volea quedan en 84–86 %.
  - «Cansancio fuerte», con piernas ×3 y reacción ×2: con energía 5, el humano gana el 14 % de los puntos, frente al 29 % sin la opción. Hacerlo valor por defecto exige el «sí» del PO, cambiar el preset, rehacer las anclas y recalibrar al rival.

**Decisiones de Claude:**
- **`VolleyV21Tests`:** se fija con `HumanVolleySprintFatigue=0`, porque prueba el sprint en sí. La aserción no cambia.
- **Agentes secundarios:** los de pose y récords corrieron solo sus filtros, y las suites completas se corrieron en la integración.
- **Candado de Unity:** ahora espera con `pgrep -x Unity`. Antes, un `unity-run.sh` en espera con la ruta de Unity en su línea de comandos bloqueaba al dueño del candado.

**Sin cambios:**
- el rival: `ConfirmPerStyle` da igual que `confirm01`, tanto en la rama de cansancio como en la de SL5/SL6;
- `ContactContract.cs`, DefaultPreset, SimulationKey y las anclas AutoQA.

**Hashes:** RallyParams `13f88c8d1db2`; la proyección sin el campo nuevo da `0fe523164363`.

**Compuertas sobre `12cc652`:**

| Compuerta | Resultado |
|---|---|
| EditMode | 912/912 |
| PlayMode | 731/731 |
| AutoQA V | PASS/0/1 (`20261120T121620Z-12cc652-v-1359`) |
| AutoQA T3 | PASS/0/1 (`20261120T121637Z-12cc652-t3-25408`) |
| AutoQA volea | PASS/0/1 (`20261120T121652Z-12cc652-volley-13963`) |
| check-release dev/prod | OK, build 1029 |

**Abiertos:**
- la fila de tres enlaces del menú en un SE de 375 pt;
- el saque, en curso en `claude/serve-improve`;
- las dos paredes, después: apuntar a la lateral con un solo gesto, solver autorizado, para todos los jugadores.

## Sexta vuelta (20 nov): saque

**Pedido del PO:** «Luego próxima mejora es mejorar el saque».

**Rama:** `claude/serve-improve`, head `0eba962`, hecha por un subagente Opus. Tiene su [informe](2026-11-20-serve.md) y su [diseño](../superpowers/specs/2026-11-20-serve-design.md). Se fusionó sin conflictos; el código es idéntico al de `434da69`, que es el que pasó las compuertas.

**El saque de hoy:** uno solo, automático e igual para los dos jugadores.
- Un principiante lo devuelve con un 97,5 % de Perfecto o Válido.
- El rival lo devuelve el 100 % de las veces.

**El saque nuevo,** detrás del interruptor DEV «Saque nuevo», que queda **apagado por defecto**:
- **Dos tipos:**
  - alto, a la esquina del fondo, tocando la lateral;
  - rápido, a 22 m/s, al centro.
- **Quién elige:**
  - el humano, tocando arriba o abajo en la pausa antes de sacar;
  - el rival, según su estilo.
- **Con el interruptor encendido:**
  - el principiante devuelve el 89,7 % de los saques, dentro de la meta de 80–90 %;
  - el saque no comete faltas;
  - el rival gana el 10,3 % de los puntos directamente con el saque, frente al 2,5 % de hoy.

**Decisiones de Claude, tomadas después de ver `diag01`:**
- **M2 se retira:** contradice la volea del saque que pidió el PO.
- **M1 se mide sobre la mezcla de estilos.**
- **M3 se juzga por la zona de bote y el toque de la lateral, no por la velocidad.**
- **Por eso el interruptor queda apagado por defecto.**

**Calibración con el saque nuevo encendido:**
- el Definidor se mueve −5,1 pp en Medio y −5,5 pp en Difícil, sobre el límite de 5 pp;
- el resto de las celdas queda entre −4,5 y +1,3 pp;
- no se recalibró.

**Con el interruptor apagado,** `ConfirmPerStyle` da exactamente lo mismo que `confirm01`.

**Hashes:** RallyParams `992d0570c9de`; la proyección sin los campos nuevos da `13f88c8d1db2`.

**Compuertas sobre `434da69`:**
- EditMode 924/924 y PlayMode 735/735;
- AutoQA V, T3 y volea PASS/0/1;
- check-release OK.
