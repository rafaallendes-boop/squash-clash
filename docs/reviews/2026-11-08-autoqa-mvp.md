# AutoQA MVP — continuación y parada de determinismo

8 de noviembre de 2026, Chile. Rama `autoqa/mvp`, mismo worktree y [PR #36](https://github.com/rafaallendes-boop/squash-clash/pull/36), abierta y lista para revisión, **sin fusionar**. Base local y remota comprobada tras `git fetch`: `27058e82674f633879a7a4f7cabc4f1306a6db6b`, sin cambios desde la regresión base. Continúa el [informe inicial](2026-11-06-autoqa-mvp.md).

gate: NOT_EVALUATED

**Entrega parcial detenida por AUTOQA-MVP-02. El MVP no está aprobado.** No existe todavía `scripts/autoqa.sh`: no hay `RESULT` ni código de salida de ese comando, ni `autoqa-summary/1` o `autoqa-attempt/1`. El nivel **3** se registra como escalamiento técnico por timing/determinismo, no como salida de una regla de triaje implementada. El JSON local de auditoría usa otro esquema, `autoqa-poc-audit/1`, y no sustituye el resumen del MVP.

## Resolución de AUTOQA-MVP-01

Primer commit de esta continuación: `e0854a2`, exclusivamente documental, registra íntegramente las decisiones en [aprobaciones](2026-10-25-first-shot-po-approvals.md#continuación-del-mvp-de-autoqa--decisiones-del-po-2026-11-08-chile), con fecha 2026-11-08 y atribución al PO:

1. **«Sí, a SL5».** Los dos pasos reales se exigen respecto de todas las fronteras A/V y KIn/KOut. PERFECT y el canario pasan a SL5, Δ=0; los otros cuatro escenarios conservan SL1 y sus entradas. KIn/KOut del JSONL solo pueden invalidar por margen, no decidir la expectativa.
2. **«Nivel 3».** Hash efectivo inesperado es integridad: `hashesVerified=false`, nivel 3 y salida 3; un intento puede conservar INCONCLUSIVE. La futura prueba de salida 2 usará una precondición ajena a integridad, como pantalla.

Confirmación adicional literal **«Sí, confirmo»**: canario ejecutado que no da FAIL implica integridad/nivel 3; si no llegó a ejecutarse por `executionError` o gate ERROR, su ausencia no implica integridad. Canario PASS siempre implica nivel 3. Estas reglas quedan registradas; su TDD e implementación siguen pendientes de superar la POC.

### Valores reales, ventanas y márgenes

Defaults: `physics.stepHz=120`, `pace.tempo=0,65`, `assistMs=40`, `validMs=100`, `latencyCompMs=0`. Un paso real es `1000 / 120 / 0,65 = 12,820512820513 ms`; dos pasos son **25,641025641026 ms**. Las distancias se calculan a cada frontera, incluida la más cercana aunque no delimite la categoría esperada.

SL1 procede del `shot-plan` de LaunchId 2 de la regresión base, preservado y con hash en el informe inicial; no se ha repetido esa medición. SL5 procede de las tres instancias frescas de la POC; las variaciones de resta son del orden de 10⁻¹² ms y se preservan completas en el JSONL/auditoría.

| Línea | KIn − T* (ms reales) | KOut − T* (ms reales) | Hash efectivo esperado |
| --- | ---: | ---: | --- |
| SL1 | −268,143726069 | +6,086025371 | `c0d8ec7050e0` |
| SL5 | −239,529057599 | +245,883247689 | `55a85f500cf1` |

Los hashes se derivaron de `Tuning.Defaults`, antes de lanzar, no del hash observado. SL1 coincide con defaults; SL5 usa `practice.launch=4` (índice cero) y el `human.z` que la receta real aplica automáticamente: `(double)1.6f = 1,600000023841858`. `human.x` no cambia porque la comparación de la receta se hace como float. No se escribe `human.z` desde AutoQA. [Preparación real de práctica](../../Assets/SquashClash/Runtime/Shot/PracticeMode.cs) · [Recetas](../../Assets/SquashClash/Core/Shot/PracticeLaunchDefinitions.cs) · [Hash canónico](../../Assets/SquashClash/Core/Config/ParamSet.cs). La POC compara el hash SL5 y cero pendientes en las tres instancias; no hay todavía verificación de los 15 intentos del MVP ni de SimulationKey por intento.

| Escenario | Δ (ms) | Distancias a −A / +A (ms) | Distancias a −V / +V (ms) | Distancias a KIn / KOut (ms) | Mínimo / margen |
| --- | ---: | --- | --- | --- | --- |
| M24-PERFECT-SL5 | 0 | 40 / 40 | 100 / 100 | 239,529058 / 245,883248 | 40 ≥ 25,641026 |
| CANARY-WRONG-EXPECTATION (SL5) | 0 | 40 / 40 | 100 / 100 | 239,529058 / 245,883248 | 40 ≥ 25,641026 |
| M24-VALID-EARLY-SL1 | −70 | 30 / 110 | 30 / 170 | 198,143726 / 76,086025 | 30 ≥ 25,641026 |
| M24-FAILED-LATE-SL1 | +170 | 210 / 130 | 270 / 70 | 438,143726 / 163,913975 | 70 ≥ 25,641026 |

NOPULL y REJECTED no tienen suelta dirigida a una ventana de contacto; conservan su entrada y línea originales. El antiguo PERFECT en SL1 queda histórico y no se ejecuta: sus 6,086025 ms hasta KOut no cumplen el margen.

Se verificó [ContactContract.Decide](../../Assets/SquashClash/Core/Shot/ContactContract.cs) antes de codificar: una posición posterior a KOut devuelve `Failed`, `Fail=Late`, razón `after-k-out`, antes de comprobar A/V. En SL1, +170 ms supera KOut por 163,913975 ms. No contradice el esperado aprobado y no se cambiaron escenario ni expectativa. La POC no llama a ese contrato ni a ContactPlanner/ShotSolver; esta comprobación fue de lectura del código.

## POC: RED válido, presentación real y parada

Fuente reproducible mínima: [AutoQAPocTests.cs](../../Assets/SquashClash/Tests/AutoQA/AutoQAPocTests.cs), [NativeGameView.cs](../../Assets/SquashClash/Tests/AutoQA/NativeGameView.cs) y ensamblado aislado `SquashClash.AutoQA`. Commit `bfe1474`.

- **RED por aserción:** `Logs/autoqa/tdd/poc-red.xml`, 1 ejecutado, 0 Pass / 1 Fail, sin CS. El esqueleto carga Main y falla porque `PresentationCount` es 0 frente a 1.
- **GREEN acotado:** `Logs/autoqa/tdd/poc-green.xml`, 1 UnityTest Pass / 0 Fail, sin CS; contiene tres instancias frescas. GameView 1206×2622 verificado; ancla proyectada dentro de `Screen.safeArea`; consulta del bloqueador real no nulo y resultado false. Sin sustituir UI, área de entrada ni componentes del juego. Las tres reciben Began/Moved/Ended y producen Swing → Perfect, Applied, paso 113 y una `shot-presentation` real.
- Ese test solo comparaba entre repeticiones la firma Result/Applied/ContactStep/PresentationCount. **Su GREEN no demuestra determinismo completo ni supera la puerta POC completa.** La auditoría del JSONL encontró la diferencia semántica siguiente antes de avanzar al MVP.

La preparación carga defaults en memoria, selecciona SL5 por `Tuning.Set("practice.launch",4)` y llama `LaunchCurrent()`. Desactiva solo Host/AppBootstrap/ShotController/PracticeMode, fija el reloj de lectura y usa un Touchscreen virtual. Cada frame virtual hace, en este orden: establecer now; encolar muestras con t≤now; InputSystem.Update; Host.Advance; GesturePreview.Tick; PracticeMode.Tick; ShotController.ProcessPending; GesturePreview.FlushRecords. Las copias JSONL se hacen antes de destruir cada app. Se restaura GameView y se retira el dispositivo virtual. La reflexión se limita a tipos de UnityEditor para GameView, nunca a SquashClash.*.

### AUTOQA-MVP-02 — las repeticiones difieren en Scheduled

| Repetición POC SL5 | Result / Applied | ContactStep / presentaciones | Scheduled | DeltaMs del juego | QuantizationErrorMsSigned |
| --- | --- | --- | --- | ---: | ---: |
| 1 | Perfect / true | 113 / 1 | true | −8,881784197001252e−13 | 0 |
| 2 | Perfect / true | 113 / 1 | false | +8,881784197001252e−13 | −6,217248937900877e−12 |
| 3 | Perfect / true | 113 / 1 | true | −8,881784197001252e−13 | 0 |

La intención es idéntica, Ended en T* (Δ=0). Los registros originales contienen estas diferencias entre `ReleaseEventAtReal` y `TargetContactAtReal`. El contrato de producción decide Scheduled con `perfect && corrected <= target`. La diferencia de signo coincide con el cambio de esa rama. No se ha demostrado aquí el origen exacto de la diferencia de coma flotante ni un defecto perceptible del juego; no se atribuye a la espera de presentación ni a S2-OBS-01.

**Scheduled es estado semántico, no un coste ni una marca de reloj de pared.** No se ha excluido ni redondeado para hacer coincidir las repeticiones. Tampoco se movió Δ=0, se cambió el reloj de producción, se añadió un seam o se reintentó Unity para buscar un resultado favorable.

`Logs/autoqa/tdd/poc-determinism-audit.json` deriva exclusivamente de las tres copias originales y muestra `scheduledIdentical=false`; `poc-determinism-assertion.txt` conserva la aserción offline fallida sobre `[true,false,true]`. Es análisis de la misma corrida, **no una cuarta medición**. No se comparó ni declaró completo el determinismo de todos los campos observados del MVP.

El encargo definitivo exige detenerse si «las repeticiones no son deterministas». El encargo de continuación exige registrar, hacer commit/push documental y detenerse sin improvisar. Se aplica esa parada antes de guardia, oráculo, reportes, normalizador y runner completos. **Decisión pendiente del PO:** cómo proceder con esta diferencia de timing en la POC conservando Δ=0 y el contrato; cualquier posible tratamiento requiere autorización y revisión, sin solución aplicada en esta entrega.

## Escenarios y aceptación pendiente

| Escenario vigente | Esperado | Observado / estado |
| --- | --- | --- |
| M24-PERFECT-SL5 | Swing/Perfect/Applied, 1 presentación Perfect, 0 discrepancias, primer FrontWall ≤0,05 m de TargetUsed, predicción bit a bit | POC: Swing/Perfect/Applied/paso 113/una presentación, tres veces; Scheduled distinto. FrontWall y predicción no evaluados por el oráculo del MVP. NOT_EVALUATED |
| M24-VALID-EARLY-SL1 | Valid/Applied, una presentación Valid | Sin intento MVP. NOT_EVALUATED |
| M24-FAILED-LATE-SL1 | Failed/Late, no Applied, una presentación Failed | Sin intento MVP. NOT_EVALUATED |
| M24-NOPULL-SL1 | AbandonedNoPull, no consumo, NoOrder, cero presentaciones | Sin intento MVP. NOT_EVALUATED |
| M24-REJECTED-SL1 | Rejected, sin Swing, NoOrder | Sin intento MVP. NOT_EVALUATED |
| CANARY-WRONG-EXPECTATION | Mismo gesto SL5, esperado deliberado Failed; debe dar FAIL | No ejecutado. No es un fallo de integridad por ausencia: la corrida MVP no llegó a ejecutarse |

Regresión base vigente: **546/546 EditMode y 354/354 PlayMode**, XML frescos del 6 de noviembre, ya registrados con SHA-256 en el informe inicial. Main no cambió y el PO pidió no repetirla. No se presentan como regresión nueva del 8. Únicas ejecuciones Unity nuevas: RED 0/1 y GREEN acotado 1/1 de POC, con tres instancias en este último. No se ejecutaron suites completas posteriores ni check-release dev/prod: pendientes por la parada. No hubo builds nativas, instalación, iPhone ni cambios en PR #32, #37 o S3-validation.

## Integridad y evidencia

Los archivos de producción, defaults, ProjectSettings y Packages terminan byte a byte iguales a main. Tras PlayMode se restauraron exclusivamente los transitorios `GraphicsSettings.asset` y `QualitySettings.asset`. No se invocan APIs de guardado de presets, no se cambian parámetros salvo la línea autorizada ni se guarda un preset. **No se instrumentó SHA-256 antes/después del directorio de presets**: ese criterio sigue pendiente y no se declara aprobado. La guardia automática y su RED, SimulationKey por intento, integridad global y verificación del SHA por el futuro runner tampoco están implementados. Los hashes siguientes identifican las evidencias locales preservadas; no son artefactos versionados.

| Artefacto local, relativo al worktree | SHA-256 |
| --- | --- |
| `Logs/autoqa/tdd/poc-red.xml` | `f2ccae1a0019247581448de8cad5cd01b43d99e7c5d7d4ae776ff594d658a255` |
| `Logs/autoqa/tdd/poc-red.log` | `fba12e0050e90cadf5899015e3381e245035f01075e31dfe1540ce72c88164ff` |
| `Logs/autoqa/tdd/poc-green.xml` | `fb7cba56e1574a9d0a97235b5558f87274ab21ecd85871b454ecc93042ce08ed` |
| `Logs/autoqa/tdd/poc-green.log` | `5bce31b4973684457bbfa4efd360ae75630800eb23b121077fdc64af9a6ef656` |
| `Logs/autoqa/tdd/poc-game-1.jsonl` | `ee8f12bbd589f901d99315d48f3f478abd11a97e396265a4a4b9b43362459179` |
| `Logs/autoqa/tdd/poc-game-2.jsonl` | `8066d65617ff5ea61ac60bcda0629583e49a2a7bd77d0b7d35ef0c5d9c416430` |
| `Logs/autoqa/tdd/poc-game-3.jsonl` | `e0ba1ccf1ac29fe3e4ad1c6948a42bf61973cc054697f1c68a6acb6595be1029` |
| `Logs/autoqa/tdd/poc-determinism-audit.json` | `66da144f8740c89c7e25f839057a30822e4484fe832bc0be0f83a0690ab99c8c` |
| `Logs/autoqa/tdd/poc-determinism-assertion.txt` | `93e72dca1346033004c0389da27a163a9be7b56ed8cf1680fe8f13210c0e1701` |

## Alineación con las propuestas de Claude

Son propuestas, no nuevas decisiones del PO. Ninguna incompatibilidad conceptual detectada. Su estado exacto es el siguiente; no se declara integración operativa con #37.

| Propuesta | Decisión de esta entrega |
| --- | --- |
| Margen inválido → NOT_EVALUATED / scenario-invalid-margin | Adoptada documentalmente para el futuro oráculo; implementación/TDD pendientes por parada |
| `counts`, `canary.observed`, `gameMetrics` como lista `{path,sha256}`, `executionError.kind` | Adoptados como nombres del futuro resumen; no se generó `summary.json` de MVP |
| Líneas `RUN_DIR=<ruta>` y `RESULT=<gate>` | Adoptadas para el futuro script; script aún no creado |
| Línea explícita `gate: <valor>` en report.md | Adoptada en este informe (`gate: NOT_EVALUATED`); futuro report.md automático pendiente |
| Gate definido incluso cuando canario pasa/nivel 3 | Adoptada documentalmente: integridad decide nivel/salida; el gate sigue definido. TDD pendiente |
| Una sola plantilla del operador compartida por OPERATIONS.md/verificador #37 | Pendiente: parada previa a OPERATIONS/plantilla; no se creó otra plantilla ni se tocó #37 |
| Precedencia de salidas `3 > 4 > 1 > 2` | Adoptada documentalmente para causas concurrentes, respetando la excepción de canario no ejecutado; implementación/TDD pendientes |

El [README](../autoqa/README.md) identifica esta entrega incompleta y enlaza este contrato, sin publicar un comando inexistente.

## Desviaciones, límites y siguiente paso

La selección SL5 es un cambio autorizado por el PO. El bucle de Editor orquestado es la desviación ya autorizada respecto del bucle de producción. La primera comprobación entre repeticiones fue demasiado estrecha para descubrir Scheduled en el XML; se registra expresamente y la evidencia primaria impide declarar la POC completa. La entrega conserva el fixture que produjo esos datos, no una prueba de aceptación completa.

Criterios 2–14 del MVP quedan pendientes salvo las comprobaciones parciales detalladas; la consumibilidad a partir de `summary.json`/`attempts.jsonl` no se puede demostrar porque no existen. No se fabricaron resumen, intentos, salida 0–4 ni una plantilla rellenada. S2-OBS-01 sigue abierta y fuera de alcance: la POC no evalúa trayectoria posterior al primer frontis ni corrige SL5.

AutoQA Editor no verifica digitalizador, UIKit, IL2CPP, audibilidad, háptica percibida, lectura visual, latencia toque→imagen, sensación ni diversión. Esas verificaciones siguen correspondiendo al PO en una sesión iPhone autorizada aparte.

**Detenerse.** La revisión independiente de esta entrega corresponde a Claude, coordinada por el PO. Ningún mensaje externo enviado. PR #36 abierta/lista, sin fusión; resolver AUTOQA-MVP-02 requiere decisión del PO antes de retomar el punto 2. S4/First Rally/First Match siguen fuera de alcance.
