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
