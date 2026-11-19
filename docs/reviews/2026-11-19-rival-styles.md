# Estilos del rival, volea del rival y cobertura: informe de entrega

19 nov 2026 · Rama `claude/rival-styles`, base `f068cee` (integración de #84), head `5638c16` (compuertas); este informe va en un commit solo de documentos encima. Lo implementó un subagente Opus dirigido por Claude (product lead, delegación del PO del 18 nov); **la revisión de Claude no es independiente.** Diseño aprobado: [2026-11-19-rival-styles-design.md](../superpowers/specs/2026-11-19-rival-styles-design.md). Sin push, fusión ni instalación.

## Qué cambia

- **`RivalAi` con tres estilos:** «Fondista», «Definidor» y «Completo».
  - **Qué cambia el estilo:** pesos de la utilidad, aversión al riesgo, umbral de defensa, apertura del drop, disposición a volear, espera W y cobertura tras un golpe corto propio.
  - **Qué no cambia:** física, reglas, alcance, ventanas, error e información.
  - La dificultad sigue siendo seis parámetros por nivel, compartidos por los tres estilos.
  - `FondoAi` no se toca. El Fondista sin volea ni cobertura decide igual que `FondoAi` y reproduce las firmas congeladas de RallySim.
- **Volea del rival:**
  - Usa la misma oportunidad que el humano: un solo softmax sobre 36 opciones y un solo intento.
  - El soltar apuntado a la volea se juzga con `OpportunityContract.Select` (`RallyContact.ResolveAimedVolley`).
  - Hay una `CausalReference` por oportunidad, y la volea cuesta `VolleyEnergyCost`.
- **Cobertura:** si el bote predicho del golpe propio cae antes de 3,5 m, los tres estilos vuelven a la T. El Definidor también espera en la T desde `11a8ca9`; antes, en `search01`, esperaba 0,3 m adelante y cubría 1,5 m detrás del bote. Usa solo la pelota recién golpeada y el predictor compartido.
- **Juego real:**
  - Estilo al azar en cada partido del menú, reproducible mediante el contador `sc.firstRun.rivalMatches`.
  - La revancha conserva el rival; en Rally, o fuera del menú, juega el Fondista.
  - Se muestra debajo de «RIVAL» y en la línea del primer punto («Rival: Definidor · …»).
  - DEV «Estilo del rival»: Auto, Fondista, Definidor o Completo.
  - `style` en `rally-point`.
- **Decidido antes de calibrar:** el sesgo de volea del Completo pasa de 0 a −0,8.
- **Dificultad por estilo** (cambio de método de Claude, `b5985ff`): `RivalAi.For` multiplica los cinco ruidos del nivel por un k propio de cada estilo y nivel (`StyleNoiseK*`). La temperatura no cambia.
- **Pendiente:** la pose de volea de la figura del rival.

## Tests editados (consecuencia directa)

1. `VolleyEnergyTests` y `VolleyRallyTests`: las proyecciones excluyen también `Style*`; sus valores no cambian.
2. `ZRallyHudTests.MatchButton…`: la línea del primer punto empieza con «Rival: Fondista · ».
3. `ZImpactMarkerAlignmentTests.TimingTriangle…`: compara con `controller.ShownPlan`, porque con volea el triángulo se dibuja en P\* de la volea. Diferencia: 0,000 cm.
4. `ZFirstRunFixture`: limpia la clave del contador.

`RallyDirector` agrega `AutopilotLine` y `AutopilotPower`, que solo usa un test.

## Compuertas (`5638c16`)

- **RED por aserción:**
  - EditMode: 10 de 12 contra stubs, 1 de 12 por el Definidor en la T y 1 de 1 por k por estilo.
  - PlayMode: 3 de 3 contra stubs.
- **EditMode:** 883/883.
- **AutoQA:** v, t3 y volley PASS/0/1, sin dirty (`20261119T213459Z-5638c16-v-2326`, `20261119T213529Z-5638c16-t3-29295`, `20261119T213546Z-5638c16-volley-18218`).
- **check-release:** dev y prod OK (build 961).
- **PlayMode 665/668: parada.** No se tocaron ni los tests ni los valores. Los tres fallos vienen de rallies distintos con el rival nuevo:
  1. `ZHandednessTests.Handedness_ChangesNothingButTheDrawing`.
  2. `ZMusicTests.Music_KeepsItsSourcesApart_FromTheContactAudio`.
     - En los dos (1 y 2), la semilla 9 del menú toca Definidor en Medio, con k = 1.
     - El autopiloto (σ 30, paralelo medio) y el Definidor en la T juegan rallies de ~170 golpes, con un punto cada ~280 s. Los tests esperan 3 puntos en 400 s y 2 en 300 s.
     - No es un bloqueo; lo confirmó un diagnóstico temporal, ya borrado.
     - Fallan igual con o sin la `BuildInfo` prod que deja check-release.
  3. `ZRallyDirectorTests.Autopilot_ExchangesStrokes…`: la figura dibujada queda en (5,50; 6,28) y el cuerpo en (5,49; 6,28).
     - El test compara justo al cerrar el 4.º punto, cuando rige el congelado de 100 ms de fin de punto (`PointEndFreeze`, de la integración del 19 nov).
     - Antes pasaba porque el rival estaba quieto en ese instante.
- En `197629f` (Definidor de `search01`, ruido de hoy) PlayMode dio 668/668.

## Hashes

- **RallyParams:** `5cdbb01e4621` → `85441c9b1cd7` (estilos) → `5fdcf65f91f7` (Definidor en la T) → **`0f58580f44ea`** (k por estilo).
  - Sin `Style*`: `5cdbb01e4621`.
  - Sin `Style*` ni `Volley*`: `927e304bbfec`.
- **Sin cambios:** defaults `207da8f2c636`/132, SimulationKey `166a96e319c7`, anclas de AutoQA y `DefaultPreset.json`.

## Calibración `search01`: detenida en los tres niveles

Objetivos fijados antes de medir: novato 60/40/20 %, ±4 pp en el promedio y ±8 pp por estilo; en Difícil, bot30 ≤ 45 %. En `search01` (200 partidos por celda, k de 1 a 3 sobre los cinco ruidos, temperatura fija) **ningún k cumple**.

Puntos del novato con el ruido actual, % [IC 95 %]:

| | Fondista | Definidor | Completo |
|---|---|---|---|
| Fácil | 38,9 [36,9–40,9] | 70,1 [68,0–72,1] | 41,8 [39,8–43,9] |
| Medio | 20,1 [18,3–22,1] | 72,9 [70,9–74,9] | 29,3 [27,3–31,3] |
| Difícil | 4,7 [3,7–5,9] | 72,0 [69,9–73,9] | 18,1 [16,4–20,0] |
| Difícil, bot30 | 1,6 | 0,2 | 3,9 |

**Causa:**
- El Definidor da 70–87 % con cualquier k contra el novato, y bot30 le gana solo el 0,1–3 % de los puntos.
- Su W adelantada y su cobertura de 1,5 m pierden contra golpes largos y fuertes, y matan los cortos de potencia media.
- El Fondista y el Completo sí siguen al ruido:
  - Fácil, k = 1,5: 59,4 y 63,1 %.
  - Medio, k = 1,5: 33,0 y 42,0 %.
  - Difícil, k = 1,25: 5,7 y 20,2 %.

**Identidad de los estilos, en todas las celdas:** Fondista 82–96 % largo; Definidor 43–55 % de ataque y volea en el 9,5–12 % de sus golpes; Completo con volea en el 0,3–1,5 %.

**Decisión de Claude (19 nov):** se acepta la propuesta del agente. El Definidor pasa a W en la T y cobertura «a la T», conserva sus pesos de ataque y volea, y se repite la búsqueda con el mismo método y objetivos. Evidencia: `docs/validation/evidence/2026-11-19-rival-styles/calibration-search01/`.

## Definidor en la T y `search02` (19 nov, tarde)

**Cambio** (`11a8ca9`): el Definidor espera en la T y, tras un golpe corto propio, vuelve a la T. Conserva sus pesos de ataque y de volea.
- RED por aserción: 1 de 12 en `RivalAiTests`; después GREEN.
- RallyParams **`5fdcf65f91f7`**.
- Sin `Style*` sigue en `5cdbb01e4621`; sin `Style*` ni `Volley*`, en `927e304bbfec`.

**Calibración `search02`** (mismo método y objetivos, 200 partidos por celda): **ningún k cumple en ninguno de los tres niveles.** El ruido queda como hoy y no corresponde la confirmación.
- La distancia entre estilos bajó de ~50 pp a ~20 pp, pero sigue siendo mayor que la banda de ±8 pp.
- **Fácil:** entre k = 1,25 y 1,5 el promedio pasa de 54,2 a 64,3 %, y el Definidor da 70,4 % con k = 1,5.
- **Medio:** con k = 1,25 el Fondista da 25,8 %; con k = 1,5 el Definidor da 51,6 %.
- **Difícil:** con k = 1,5 el promedio es 22,8 %, pero el Fondista da 8,6 % y el Definidor 37,4 %.

Puntos del novato con k = 1, % [IC 95 %]:

| | Fondista | Definidor | Completo |
|---|---|---|---|
| Fácil | 38,9 | 50,2 (k 1: no medido; k 1,25: 60,5 [58,5–62,5]) | 41,8 |
| Medio | 20,1 [18,3–22,1] | 41,9 [39,8–43,9] | 29,3 [27,3–31,3] |
| Difícil | 4,7 [3,7–5,9] | 35,9 [33,9–38,0] | 18,1 [16,4–20,0] |
| Difícil, bot30 | 1,6 | 12,0 [10,5–13,7] | 3,9 |

Evidencia: `docs/validation/evidence/2026-11-19-rival-styles/calibration-search02/`.

**Compuertas sobre `81e1278`:**
- EditMode 882/882.
- AutoQA v, t3 y volley PASS/0/1, sin dirty (`20261119T205555Z-81e1278-v-14149`, `20261119T205614Z-81e1278-t3-22269`, `20261119T205633Z-81e1278-volley-13550`).
- check-release dev y prod OK (build 959).
- **PlayMode 666/668: parada.**
  - Fallan `ZHandednessTests.Handedness_ChangesNothingButTheDrawing` y `ZMusicTests.Music_KeepsItsSourcesApart_FromTheContactAudio`.
  - Los dos arrancan desde el menú con la semilla 9, que toca Definidor, y esperan 3 puntos en 400 s o 2 en 300 s. No hay bloqueo: un diagnóstico temporal, ya borrado, mostró rallies de ~170 golpes y un punto cada ~280 s entre el autopiloto (σ 30, paralelo medio) y el Definidor en la T.
  - Dieron lo mismo con y sin la `BuildInfo` prod que deja check-release.
  - No se tocaron ni los tests ni los valores.

## Calibración final: un k por estilo y nivel (cambio de método de Claude)

Los objetivos no cambian. El cambio de método es decisión de Claude, después de `search02`: los estilos tienen fuerzas intrínsecas distintas, así que un ruido compartido por nivel no puede cumplir la banda de ±8 pp por estilo.

**Elección:** el k de la malla de `search02` más cercano al objetivo, dentro de ±4 pp.
- Fondista Difícil se midió aparte con k = 2,5 (20,1 %, dentro de la banda), con las semillas de la búsqueda.
- Definidor Difícil queda en k = 1. Es una **desviación**: con k = 1 da 35,9 %, más fácil que el objetivo, y nunca se baja de 1.

**Confirmación con semillas nuevas** (`confirm-per-style01`, 200 partidos por celda): puntos del novato y de bot30, % [IC 95 %] (k):

| Nivel (objetivo) | Fondista | Definidor | Completo |
|---|---|---|---|
| Fácil (60) | 58,5 [56,4–60,5] (1,5) · bot30 46,8 | 60,0 [57,9–61,9] (1,25) · bot30 50,9 | 61,9 [59,9–63,9] (1,5) · bot30 60,0 |
| Medio (40) | 39,9 [37,9–42,0] (1,75) · bot30 30,8 | 43,6 [41,6–45,6] (1) · bot30 31,2 | 41,7 [39,7–43,8] (1,5) · bot30 27,7 |
| Difícil (20) | 19,8 [18,0–21,7] (2,5) · bot30 9,5 | **37,8** [35,8–39,9] (1, desviación) · bot30 11,3 | 18,7 [17,0–20,6] (1,25) · bot30 5,8 |

**Resultado:**
- Las 8 celdas con k elegido quedan dentro de ±4 pp de su objetivo.
- bot30 en Difícil queda ≤ 45 % en los tres estilos.
- Fácil > Medio > Difícil en cada estilo, con los IC sin solaparse.
- **Identidad de los estilos:** Fondista 82–96 % largo; Definidor 41–47 % de ataque y volea en el 5–7 % de sus golpes; Completo con volea en el 0,3–1,5 %.
- **No se hizo** el control secundario de ~86 % de contacto, por falta de tiempo.

**Observaciones de la calibración:**
- Con k = 1,5 en Fácil, `d_perc` llega a 120 ms, sobre el rango [EXPERIMENTO] de 0–80 ms de §11.2. Decide Claude si se le pone tope.
- SF-01 sube con el ruido: entre 11 y 34 por cada ~2.200 puntos en las celdas de Fácil (0,5–1,5 %).

Evidencia: `docs/validation/evidence/2026-11-19-rival-styles/confirm-per-style01/`.

## Observaciones

- **SF-01:** sin corregir.
  - En el juego real el punto termina limpio: el error se cuenta, la pelota sigue hasta el doble bote y el punto es del otro jugador.
  - Hay un caso en las métricas reales del PO: build `22bb2d5`, punto para el Rival con recepción `SystemFault`.
- **Siguiente pasada:** que los atributos físicos afecten también a las IA.

## Decisión de Claude sobre la parada de PlayMode (19 nov)

Los tres fallos venían de supuestos de los tests, no del juego.

- **`ZHandednessTests` y `ZMusicTests`:** prueban la mano y la música, no al rival. Ahora fijan al Fondista, el rival contra el que se escribieron, mediante `StyleOverride`. Así el presupuesto de puntos vuelve a valer.
- **`ZRallyDirectorTests.Autopilot_ExchangesStrokes…`:** compara el dibujo con el cuerpo justo al cerrar el punto. En ese instante el congelado de 100 ms retiene el dibujo, por diseño. Este test apaga el congelado y lo restaura en el TearDown; el congelado tiene sus propios tests (`ZPointEndFreezeTests`).

No cambia ningún umbral, valor esperado ni presupuesto. Con este cambio las tres clases pasan: PlayMode 30/30. Las suites completas se corren en la integración.

**Observación para el iPhone:** contra el autopiloto, un tiro recto con σ 30 ms, el Definidor en Medio juega rallies de unos 170 golpes. Es un artefacto del autopiloto; en el modo Demo se vería igual.
