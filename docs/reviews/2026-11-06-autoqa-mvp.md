# AutoQA MVP — parada previa a la prueba de concepto

6 de noviembre de 2026, Chile. Base `main` en `27058e82674f633879a7a4f7cabc4f1306a6db6b`. Rama `autoqa/mvp`.

## Alcance y regresión base

La decisión literal del PO y la política de operación quedaron en el [registro de aprobaciones](2026-10-25-first-shot-po-approvals.md). El [estudio aprobado](2026-11-06-autoqa-technical-study.md) se copió byte a byte y conserva SHA-256 `fac103f0bbdc5a3777103de8d59118a52707fea2eb83aabede3fb03410b386b0`.

`scripts/test.sh editmode` pasó **546/546** y `scripts/test.sh playmode` pasó **354/354** sobre la base, ambos con XML fresco, cero omitidos y sin errores CS. Después de PlayMode se restauraron los transitorios de `GraphicsSettings.asset` y `QualitySettings.asset` en el worktree de AutoQA. El diff de la entrega es documental.

## AUTOQA-MVP-01 — margen de PERFECT frente a la frontera legal

El encargo fija `Δ=0` en `M24-PERFECT-SL1` y en el canario que usa el mismo gesto. También exige que cada offset quede a dos pasos de simulación o más de cualquier frontera. El [contrato de contacto vigente](../../Assets/SquashClash/Core/Shot/ContactContract.cs) usa `KIn` y `KOut` como fronteras geométricas que pueden cambiar el resultado antes de evaluar las ventanas de asistencia y validez.

El JSONL fresco de Main contiene un `shot-plan` de SL1, LaunchId 2, con hash efectivo `c0d8ec7050e0` y cero pendientes. En el primer lanzamiento de esa instancia, Main usa el lanzamiento libre antes de cargar la receta de práctica. Los tiempos de abajo corresponden al segundo lanzamiento, ya en SL1. La prueba existente `SixPracticeLaunches_AreReachableFromTheirBodyPositions` confirma los mismos tiempos relativos al lanzamiento en el XML fresco de EditMode. [Arranque de Main](../../Assets/SquashClash/Runtime/App/AppBootstrap.cs) · [Receta SL1](../../Assets/SquashClash/Core/Shot/PracticeLaunchDefinitions.cs) · [Prueba de tiempos](../../Assets/SquashClash/Tests/EditMode/Shot/ContactPlannerTests.cs)

| Dato | Valor |
| --- | ---: |
| `KIn − T*` | −268,143726 ms |
| `KOut − T*` | +6,086025 ms |
| Un paso real, `1000 / 120 / 0,65` | 12,820513 ms |
| Margen exigido, dos pasos reales | 25,641026 ms |
| Margen de la suelta de PERFECT y del canario hasta `KOut` | 6,086025 ms |

Con `latencyCompMs=0`, la suelta en `Δ=0` queda a menos de medio paso de `KOut`. Incluso con los dos pasos expresados en tiempo simulado, la separación es de 3,955916 ms simulados frente a 16,666667 ms. La discrepancia afecta al diseño del oráculo y a la validez del escenario. El margen se aplica a la suelta, aunque la trayectoria de SL1 sea alcanzable y el juego pueda clasificar una suelta en T* como Perfect. [Defaults vigentes](../../Assets/SquashClash/Runtime/Resources/DefaultPreset.json)

Además, con la asistencia de 40 ms, aplicar dos pasos reales a la vez al borde de asistencia temprano y a `KOut` exigiría `Δ ≥ −14,358974 ms` y `Δ ≤ −19,555000 ms`, respectivamente. Un cambio de offset por sí solo no satisface ambas condiciones con SL1 y los defaults.

| Escenario | Esperado | Observado en AutoQA |
| --- | --- | --- |
| M24-PERFECT-SL1 | Swing, Perfect, Applied, una presentación Perfect, contacto en frontis y predicción bit a bit | Sin intento. Conflicto del margen previo a la POC |
| M24-VALID-EARLY-SL1 | Valid, Applied, una presentación Valid | Sin intento |
| M24-FAILED-LATE-SL1 | Failed/Late, una presentación Failed | Sin intento |
| M24-NOPULL-SL1 | AbandonedNoPull, intento sin consumir, NoOrder, cero presentaciones | Sin intento |
| M24-REJECTED-SL1 | Rejected, sin Swing, NoOrder | Sin intento |
| CANARY-WRONG-EXPECTATION | El arnés detecta que Failed difiere del resultado del gesto PERFECT | Sin intento. Comparte el conflicto del margen |

**Decisión pendiente del PO.** Confirmar si los dos pasos se exigen también respecto de `KIn/KOut` o solo respecto de A/V para el MVP. Con la lectura que incluye `KIn/KOut`, los escenarios aprobados y el margen no pueden satisfacerse simultáneamente. Cualquier revisión de esa condición requiere una decisión explícita.

## Evidencia y estado de entrega

Evidencia local ignorada en `Logs/autoqa/baseline/` del worktree de AutoQA.

| Artefacto | SHA-256 |
| --- | --- |
| `editmode-results.xml` | `eadd5d610a2efebb108a197e72e8bf63b32319bad448604e4db00a32b1374681` |
| `playmode-results.xml` | `a613c5e4ea99dbec1367ff292fae6c9f3e20d7d1d52aea51f5756ee14985f669` |
| `main-sl1-game-metrics.jsonl` | `46fdc485c8842b17a7edd8d45744f0b99d0679a69a05d16fa844566f4a3b58da` |

El `shot-plan` de SL1 se identifica por el sello `2026-11-06T22:20:09.6562710Z`. La copia procede de la regresión base. Los XML y el JSONL preservan las fuentes primarias de esta parada.

Estado de aceptación del MVP: **NOT_EVALUATED**. POC, RED/GREEN del arnés, determinismo, control SHA-256 de presets, check-release, triaje automatizado, códigos de salida y prueba de consumibilidad quedan pendientes. No existen `RESULT`, `escalationLevel` ni artefactos de una corrida de AutoQA. El conflicto corresponde a timing y contrato, materias del nivel 3 de la política del PO.

Las comprobaciones de audibilidad, vibración percibida, lectura visual, latencia toque→imagen, sensación del golpe y diversión corresponden al PO en el iPhone. La regresión base se ejecutó en Unity Editor.


## Continuación del 8 de noviembre de 2026

AUTOQA-MVP-01 resuelto por decisiones explícitas del PO. La POC posterior quedó detenida por AUTOQA-MVP-02 (Scheduled difiere entre repeticiones). Este informe conserva la parada histórica inicial; el estado vigente está en el [informe de continuación](2026-11-08-autoqa-mvp.md).
