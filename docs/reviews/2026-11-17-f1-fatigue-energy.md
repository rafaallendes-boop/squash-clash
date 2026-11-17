# F1 — energía, piernas y reacción: entrega con parada de AutoQA

17 nov 2026 · base `766dedd` (#75) · rama `codex/f1-fatigue-energy` · producto/literales finales `5b1329d`. Implementa energía por jugador, consumo por metros y P de intención, recuperación por paso/una vez al cierre, instantánea f_r, función compartida de piernas/reacción y métricas `fatigue`/`shot`. Humano, rival y RallySim usan el mismo factor y delay en planificación/ejecución. Saque automático P=0 para gasto fuerte. Claves DEV: once, 132 totales. Revisión independiente y aprobación del PO pendientes; sin fusión ni instalación.

| Comprobación | Resultado conservado |
|---|---|
| TDD | RED inicial: 19 aserciones válidas y 1 error de clave ausente (no contado como RED); GREEN 20/20. Integración: RED 2 aserciones + 2 controles, GREEN 4/4. Director: RED 2 aserciones, GREEN 2/2. Ajuste: RED 1 aserción, GREEN 24/24. |
| Suites finales | **EditMode 788/788, PlayMode 583/583**, cero omitidos; XML frescos, ProjectSettings restaurado. |
| A1 / A6 | Off: **200 partidos / 1.927 puntos idénticos bit a bit** a main; determinismo activo, reinicio y persistencia comprobados. |
| Experimento B | Soft: B3 Fail (80,29). Único ajuste predeclarado recoverBetween **0,35→0,25**; 200 partidos / 1.844 puntos: **todas las compuertas B pasan**. B3 70,68; 194/200 bajo onset; B4 0/200; B5a 63,84 (10 puntos); B6 −3,20 %. B5b: 1 elegible, 0 pares, sin estimación; exploratorio. |
| Proyección / hashes | Cinco proyecciones históricas exactas, incluida P0 `dcc7c8cc0688`; SimulationKey `166a96e319c7`. Defaults nuevos `2ec0c953786a`, P0 `f92f406f698a`; AGENTS §5. P0-calib-1 intacto. |
| Puertas (tras F1-AUTOQA-01) | Ver sección «F1-AUTOQA-01 resuelto»: EditMode 790/790, PlayMode 583/583, AutoQA V y T3 PASS/0/1, check-release OK. A2 pendiente de revisión. |

**F1-AUTOQA-01 (inspección estática):** `Tests/AutoQA/PocObservation.cs:26–39` enumera el esquema exacto de ShotRecord y rechaza campos desconocidos. El nuevo `ShotRecord.f_r` se serializa también en práctica, por lo que sería rechazado aunque valga cero. Solo están autorizadas las cuatro anclas de Scenario; no se toca la lista, el oráculo ni el checker y no se ejecuta para fabricar un fallo conocido. Se aplica la parada del encargo/spec §8.

**Propuesta (ya autorizada, ver abajo):** emitir f_r solo en los eventos shot de Rally/Partido, conservando el esquema exacto anterior en práctica/SL/AutoQA. Requiere un cambio acotado de serialización de producto, con TDD; no cambiaría PocObservation, escenarios, oráculos, checker ni contactos/reloj. No implementada ni autorizada como corrección del incidente. Después corresponderían regresiones y puertas restantes. F2 espera F1 fusionada.

**Valores fijos heredados actualizados** (commits propios `f2b840f` y `5b1329d`, segundo por el único ajuste autorizado): DefaultPresetHashTests, M24ProjectionTests, TimingAidConfigTests, S3FeedbackConfigTests (solo añade fatigue.* a su lista), ContactContractTests, ShotSolverTests, CausalQualityTests, CausalReferenceCoverageTests, S2DefaultPromotionTests (P0), RivalReleaseTimingTests, AutoQAT3OracleTests, ZRallyHudTests, ZFirstRunMenuTests, ZRallyDirectorTests y las cuatro anclas de Scenario. Fixtures de rally/partido fijan fatigue.enabled=0. [Lista exacta y comprobación de que todo lo demás es idéntico](../validation/evidence/2026-11-17-f1-fatigue/fixture-values.json).

[Evidencia B e integridad](../validation/evidence/2026-11-17-f1-fatigue/manifest.json), [recibos TDD/suites/parada](../validation/evidence/2026-11-17-f1-fatigue/gates.json), [método](../validation/2026-11-17-f1-fatigue-method.md), [ajuste único](../validation/2026-11-17-f1-fatigue-adjustment.md). Datos iniciales/ajustados intactos por SHA-256; logs/XML/recepciones crudas conservados en Logs ignorados. F1-PACK-01/02: falsos positivos de normalización/inclusión de una prueba deliberada de la guarda corregidos bajo autonomía de empaquetado; intentos preservados y SHA-256 intactos, sin repetir experimentos. No hay aceptación física ni global de F1.

## F1-AUTOQA-01 resuelto (17 nov 2026)

Autorizado por el PO (17 nov, por intermedio de Claude); implementado por un subagente Sonnet dirigido por Claude, **revisión no independiente** (declarada en el registro de aprobaciones). Commit `c87cd89`: `ShotRecord.ToJson(bool)` quita el miembro `f_r` y `ShotController` solo lo emite si Rally/Partido lo fijó para ese lanzamiento (`SetNextReceptionFatigue`); el `shot` de Rally-contact del rival conserva `f_r`. No se tocó `PocObservation`, escenarios, oráculos, checker, `DefaultPreset.json`, física, timing ni reloj. Spec §8 aclarada.

| Puerta | Resultado |
|---|---|
| TDD | RED por aserción: `ShotRecordSchemaTests` 1 fallo de 2 (el de Rally ya cumplía); GREEN 2/2; PlayMode `ZFatigueFlowTests` 2/2 (añade que todo `shot` de Rally lleva `f_r`). |
| EditMode / PlayMode | **790/790** y **583/583**, XML frescos; `git checkout -- ProjectSettings` hecho. |
| AutoQA V | PASS, exit 0, nivel 1, 88/88 (`20261117T201842Z-c87cd89-v-14560`). |
| AutoQA T3 | PASS, exit 0, nivel 1, 88/88 (`20261117T201902Z-c87cd89-t3-20321`). |
| check-release | dev y prod OK. |

Experimento B no repetido: el cambio no altera sus datos (Rally conserva `f_r`). Sin fusión ni instalación.
