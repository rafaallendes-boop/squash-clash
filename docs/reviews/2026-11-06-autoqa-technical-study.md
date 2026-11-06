# Squash Clash — Estudio técnico de AutoQA

Fecha: 6 de noviembre de 2026. Base inspeccionada: `main` en `27058e8` (merge de PR #35). Autor: Claude (Opus 5.5), solo lectura.
Alcance: estudio. No hubo implementación, commits, ramas, PR, cambios de configuración ni acceso al iPhone. PR #32 y los worktrees de Codex no se tocaron.

Convención: **[R]** = confirmado en el repositorio (con ruta), **[POC]** = requiere prueba de concepto, **[J]** = juicio técnico mío.

---

## Resumen ejecutivo

- **Sí se puede** construir un AutoQA que juegue recepciones reales con gestos M2-4 reales, sin tocar producción. El MVP no necesita ningún cambio en `Core/`, `Runtime/`, `DevPanel/`, `Editor/`, `ProjectSettings/`, `Packages/` ni `DefaultPreset.json`.
- Motivo [R]: la entrada del juego ya es el Input System nuevo y `TouchInputAdapter` acepta eventos de *cualquier* `Touchscreen`. Las pruebas PlayMode actuales ya inyectan toques virtuales con marca de tiempo exacta, que recorren el camino completo adaptador → `GesturePreview` → `M24Model` → `ShotController` → `ContactContract` → host P2.
- Recomendación: **GO CON CONDICIONES**. MVP = opción B (arnés PlayMode determinista en Editor), con el “jugador” separado del motor de ejecución para poder llevarlo después al iPhone (opción A en dispositivo, fase 2). La opción C (XCUITest) **no** se recomienda: no puede acertar la ventana Perfect de ±40 ms y su mantenimiento es alto.
- Lo que seguirá siendo tuyo: oír el sonido, sentir la háptica, juzgar visualmente, latencia toque→imagen, sensación, “¿se siente bien?” y “¿es entretenido?”.

---

## A. Diagnóstico técnico

### A.1 Inventario confirmado

| Área | Hallazgo | Fuente |
|---|---|---|
| Motor | Unity 6000.3.27f1; Input System 1.20.1; Test Framework 1.6.0; URP 17.3 | `ProjectSettings/ProjectVersion.txt`, `Packages/manifest.json` [R] |
| Ensamblados | Core (sin dependencias), Runtime (Core + InputSystem), DevPanel (`SQUASH_DEV \|\| UNITY_EDITOR`), Editor, tests EditMode/PlayMode, Experiments | `*.asmdef` [R] |
| Entrada | `TouchInputAdapter` se suscribe a `InputSystem.onEvent`, lee `TouchState` de cualquier `Touchscreen`, conserva `e.time` en double, filtra por UI, foco, pausa, área segura y `CancelSystem` | `Runtime/Input/TouchInputAdapter.cs:49-79` [R] |
| Inyección existente | `InputSystem.AddDevice<Touchscreen>()` + `QueueStateEvent(screen, TouchState, time)` ya usados en `ShotControllerTests.Send`, `M24PreviewTests.Send`, `TouchInputAdapterTests`, `InputFixtureSmokeTests` | `Tests/PlayMode/*.cs` [R] |
| M2-4 | `M24Model`: Began fija la mira mediante `FrontisProjection.TryMap` (fallo → `Rejected`); P = clamp((tirón hacia abajo − 2 mm)/(35 − 2 mm)); cierres `InvalidAim`, `Tap`, `CanceledReturnToDeadZone`, `AbandonedNoPull`, `Swing`, `SystemCanceled`; tiempo repetido con posición distinta → cancelación | `Core/Shot/M24Model.cs` [R] |
| Entrega al controlador | `GesturePreview.EmitShotClosure` encola un `ClosedGesture` inmutable en `ShotController` | `Runtime/Shot/GesturePreview.Shot.cs:15-25` [R] |
| Contrato | `ContactContract.Decide`: latencia compensada, plan presentado en el instante del evento, KIn/KOut, legalidad geométrica, V (±100 ms) y asistencia Perfect (±40 ms) | `Core/Shot/ContactContract.cs:61-91`, `DefaultPreset.json` [R] |
| Un intento por recepción | `AttemptConsumed` se reserva antes del solver; segundo Swing → `attempt-consumed`; LaunchId distinto → `wrong-launch` | `Runtime/Shot/ShotController.cs` [R] |
| Entrega única de feedback | `ShotPresentationGate.TryDeliver` + `PresentationCount` + `PresentationDiscrepancyCount` | `Core/Shot/ShotPresentation.cs`, `ShotController.Present` [R] |
| Host | Autoridad única; `Update()` usa `Time.unscaledDeltaTime` y `realtimeSinceStartupAsDouble`; `Advance(double)` público permite avanzar sin reloj de máquina; predicción aislada y comparación bit a bit | `Runtime/Ball/BallSimulationHost.cs` [R] |
| Práctica | `PracticeMode`: Reception → Resolved → Observing → Waiting → relanzamiento; recetas SL1–SL6 | `Runtime/Shot/PracticeMode.cs`, `Core/Shot/PracticeLaunchDefinitions.cs` [R] |
| Configuración | `EffectiveConfig.Hash` (Source) y `SimulationKey = HashOf("physics.","court.")`; 107 claves por defecto, `gesture.control=2`, `physics.backend=1` (P2) | `Core/Config/EffectiveConfig.cs:11,46`, `DefaultPreset.json` [R] |
| Persistencia | Presets en `persistentDataPath/presets`, métricas JSONL en `persistentDataPath/metrics`, con retención (30 archivos / 50 MB) al construir `MetricsLog` | `AppBootstrap.Awake`, `Core/Diagnostics/MetricsLog.cs` [R] |
| Evidencia del juego | JSONL con `launch`, `shot-plan`, `shot-gesture`, `shot-order`, `shot`, `shot-presentation`, `shot-presentation-discrepancy`, `m24-gesture` (con muestras crudas), `m24-input-filtered`, `dev-control` | `ShotController.Write*`, `GesturePreview.M24.cs` [R] |
| Audio | `ShotFeedback.Play` registra `LastClip` y contadores, pero **no llama a `PlayOneShot` en batchmode** | `Runtime/Feedback/ShotFeedback.cs:29` [R] |
| Háptica | `Haptics.Backend` es un seam de prueba documentado (doble de grabación) | `Runtime/Feedback/Haptics.cs:21-23` [R] |
| Scripts | `test.sh` rechaza XML viejo, total=0 y errores CS; `spike-p2.sh` ejecuta el experimento P2 y valida el informe; `check-release.sh` exige la lista exacta de módulos IL2CPP y prohíbe `InputSystem.TestFramework` en Players | `scripts/*.sh` [R] |
| Dispositivo | `build-ios.sh` (Unity → Xcode, firma automática), `install-ios.sh` (`devicectl install` + `process launch`); el historial usa `devicectl device copy from` y `capture screenshot`; iPhone bloqueado → CoreDevice 10002 | `scripts/`, `AGENTS.md` [R] |
| XCTest | No hay target de UI tests; el proyecto Xcode se regenera en cada build | `Editor/BuildScript.cs`, `Builds/` [R] |
| Arranque por argumentos | El runtime no lee argumentos de lanzamiento ni variables de entorno | grep en `Runtime/`, `Core/`, `DevPanel/` [R] |
| First Ball | Ya existe un arnés autónomo: `SpikeRun`, `PhysicsScenarios` (fijos, aleatorios, de borde), criterios C1–C17, `scripts/spike-p2.sh` en Editor y `SpikeSection` nativa | `Core/Spike/`, `DevPanel/SpikeSection.cs` [R] |
| Coste de la regresión | PlayMode completa: 354 pruebas en unos 71 s (revisión de PR #35) | `Logs/archive-review-pr35-20261106/.../playmode-results.xml` [R] |
| S4 | La tarea 4.3 ya prevé bots de anticipación que sueltan con un offset fijo desde el T* visible | `docs/superpowers/plans/2026-10-25-squash-clash-first-shot.md:697` [R] |

### A.2 Qué se reutiliza tal cual

1. **Inyección de toques**: dispositivo `Touchscreen` virtual + `QueueStateEvent` con tiempo explícito. No requiere cambios.
2. **Reloj virtual**: `TouchInputAdapter.ReadRealTime` (Func asignable), `BallSimulationHost.Advance`, `GesturePreview.Tick/FlushRecords`, `PracticeMode.Tick`, `ShotController.ProcessPending/CloseReception`. Es el mismo patrón de `ShotControllerTests.At()`.
3. **Observables del jugador**: `ShotController.Plan`, `Presented`, `Host.ClockMap.TrySimToReal(Plan.TStarStep, 0)`. Es el mismo T* que dibuja el anillo de timing, lo que ve el jugador humano. La tarea 4.3 de S4 ya lo usa como referencia.
4. **Geometría**: `Camera.main.WorldToScreenPoint(x, y, 0)` sobre el plano del frontis y `ScreenMetrics.MmPerPx(Screen.dpi, input.fallbackDpi)` para convertir milímetros a píxeles igual que el juego.
5. **Resolución nativa en Editor**: el ajuste del GameView a 1206×2622 de `ReticleVisibilityTests` funciona en batchmode.
6. **Evidencia**: el JSONL de métricas del propio juego es la fuente autoritativa del resultado. AutoQA lo copia, no lo reescribe.
7. **First Ball**: `SpikeRun`/`PhysicsScenarios`/`spike-p2.sh`. Solo hace falta un adaptador de informe.

### A.3 Qué falta

| Falta | Gravedad | Comentario |
|---|---|---|
| Catálogo de escenarios declarativo (intención → entrada) | MVP | Nuevo, solo en el ensamblado de AutoQA |
| Oráculo independiente (reglas de la spec, no `ContactContract`) | MVP | Evita que AutoQA valide el código con el propio código |
| Normalizador y comparador de determinismo | MVP | Debe excluir campos de reloj de pared y costes (`*CostMs`, `ts`) |
| Informe JSON/Markdown + script `scripts/autoqa.sh` | MVP | Copia el patrón de `test.sh`/`spike-p2.sh` (artefacto fresco, total>0, sin CS) |
| Guardia de API prohibidas | MVP | Prueba estática que lee las fuentes de AutoQA |
| Modo tiempo real en Editor | Fase 2 [POC] | Dejar correr el bucle de Unity y soltar en el frame que corresponda |
| Arranque y exportación en iPhone | Fase 2 | Ensamblado solo dev + disparador + `devicectl copy from`; requiere cambiar `check-release.sh` y autorización propia |
| Pausa vía botón de UI | Fase 2 [POC] | El MVP no la usa; las pruebas actuales simulan `OnApplicationPause`/`Clock.Paused` |
| Corpus de trazas humanas reales | Fase 2 | Reproducir las muestras crudas `m24-gesture` (en mm respecto del ancla) de las sesiones S1/S2 |

### A.4 Riesgos técnicos encontrados en el código

1. **Seams públicos peligrosos** [R]. `ShotController.Enqueue(ClosedGesture)`, `Host.ApplyShot`, `Host.ScheduleShot`, `Clock.Paused`, `Tuning.Set` y los overrides `InputAreaOverride` y `UiBlocker` son públicos. Las pruebas actuales usan `Enqueue` (el helper `Order/Submit`) para saltarse el gesto. **AutoQA nunca debe usarlos**: se necesita una guardia automática.
2. **Reloj de pared mezclado con el virtual** [R]. `ShotController.PresentCurrentPlan` marca la presentación con `Time.realtimeSinceStartupAsDouble`, no con `ReadRealTime`. `ring.Tick` también. `ContactContract` exige que el plan esté presentado antes del evento. En modo determinista esto funciona porque la marca de pared es anterior a los tiempos virtuales, pero es una fuente de no determinismo que el MVP debe medir. [POC: confirmar que no cambia ningún resultado entre repeticiones.]
3. **El bucle de frames lo orquesta el arnés** [R]. En modo determinista se desactivan `Host`, `AppBootstrap`, `ShotController` y `PracticeMode` (igual que en las pruebas) y el arnés llama a sus métodos en orden. Es una desviación documentada respecto de producción. Mitigación: fijar el orden, documentarlo y contrastarlo en fase 2 con el modo tiempo real y el dispositivo.
4. **DPI del Editor** [R]. En Editor `Screen.dpi` suele caer en el valor nominal (`DpiSource.NominalFallback`). Los milímetros por píxel no son los del iPhone. Mitigación: expresar todo en mm y registrar `dpiSource`/`mmPerPx` en cada intento.
5. **Persistencia del Editor** [R]. Al cargar la escena se leen presets de `persistentDataPath` del Editor (no los del iPhone; P0-calib-1 vive en el dispositivo). Mitigación: cargar los defaults en memoria como hacen los fixtures, comparar por SHA-256 el directorio de presets antes y después y no guardar nunca.
6. **Retención de métricas** [R]. Cada instancia crea un JSONL y la retención poda. Mitigación: copiar el JSONL a `Logs/autoqa/<run>/` antes de desmontar la app (`PlayModeTestWorld.ClearApp` lo borra).
7. **Audio en batchmode** [R]. No se reproduce, solo se registra `LastClip`. AutoQA verifica la *solicitud* de sonido, no que suene.

### A.5 Respuesta a la pregunta central

¿Puede AutoQA jugar automáticamente, con recepciones y gestos M2-4 reproducibles, sin modificar producción ni fabricar resultados? **Sí en Editor [R + POC menor]; parcialmente en iPhone [POC].**

- En Editor, el pipeline completo es alcanzable con toques sintéticos sin cambiar producción. Las pruebas actuales `SameTimestampIdenticalEnded_OneShotWithReliableP`, `FocusLoss_DuringGesture_NoSwingOnResume` y `SecondaryFinger_NoSecondOrder` lo demuestran.
- En iPhone se puede inyectar con la misma API de runtime (`QueueStateEvent` no forma parte del framework de pruebas), pero necesita código nuevo solo dev, un disparador, la exportación y una modificación de `check-release.sh`. El iPhone debe estar desbloqueado.
- **Ninguna opción** reproduce la capa digitalizador → UIKit → Unity iOS. Solo la opción C la ejercita, y lo hace sin precisión temporal.

---

## B. Comparación de arquitecturas

| Criterio | A. Jugador virtual dentro del juego | B. Arnés PlayMode (Test Framework) | C. XCUITest / automatización iOS |
|---|---|---|---|
| Viabilidad | Alta en Editor; media en iPhone [POC] | **Alta, confirmada** [R] | Baja para timing; media para un smoke test |
| Herramientas | Unity, Input System (runtime); en iPhone: build dev, devicectl | Unity batchmode + Test Framework (ya en uso) | Xcode, target UI Test añadido por post-proceso, firma del runner, `xcodebuild test` |
| Gestos M2-4 | Completos: trazas mm/ms arbitrarias | Completos, con tiempo exacto | `press(forDuration:thenDragTo:withVelocity:)`: forma limitada, sin muestras controladas |
| Acertar ±40 ms | Editor: sí. iPhone: en el frame (≈8–17 ms de error) [POC] | Sí, exacto | No: XCTest no ve el estado del juego y la latencia de síntesis es variable |
| Fidelidad | Bucle real de Unity; en iPhone, render, CPU y reloj reales; entrada sintética | Código de producción real; bucle orquestado; Editor/Mono, no IL2CPP | La mayor en la capa táctil del SO; nada más |
| Adaptaciones de producción | Editor: ninguna. iPhone: ensamblado solo dev, disparador, exportación, `check-release` | **Ninguna** | Post-proceso del proyecto Xcode, canal de estado (archivo o accesibilidad) |
| Riesgos | Código QA filtrado a prod; oculta defectos del digitalizador | Pasar solo en Editor; diferencias Mono/IL2CPP; orden de frames | Frágil, lento, flaky; dependencia de Xcode; bloqueo del iPhone |
| Esfuerzo | Fase 2: 3–5 días de agente | **MVP: 2–4 días de agente** | 6–10+ días; mantenimiento alto |
| Claude Code / Codex | Ambos: Bash + Unity + devicectl | Ambos (ya lo hacen con `test.sh`) | Ambos, con más fricción de firma y Xcode |
| Necesita | Mac + Unity; iPhone en fase 2 | Mac + Unity; nada más | Mac + Xcode + iPhone (o simulador de baja fidelidad) |

**Simulador iOS** [J]: requiere un perfil de build adicional (SDK de simulador), no tiene háptica y su rendimiento y su reloj no son representativos. No aporta frente a B en lógica ni frente a A en dispositivo en fidelidad.

### Recomendación de arquitectura: híbrido por fases

```
Escenario (JSON declarativo: receta SL, mira en metros, tirón en mm, offset Δ respecto de T*)
      │
      ▼
Jugador (lee observables permitidos → produce TouchScript: fase, t, x_px, y_px)
      │
      ▼
Driver ──► Fase 1: Editor determinista  (bombea Advance/Tick, reloj virtual)
       └─► Fase 2: tiempo real           (Editor y luego iPhone dev: suelta en el frame)
      │
      ▼
Toques sintéticos → Touchscreen virtual → InputSystem.onEvent → TouchInputAdapter → … juego real
      │
      ▼
Colector: copia el JSONL del juego + estado del host → Oráculo independiente → Informe PASS/FAIL
```

El jugador y el oráculo no conocen al driver. Así el mismo escenario sirve en Editor y en iPhone. Esto también resuelve la tarea 4.3 de S4 (los bots de anticipación son jugadores con Δ fijo).

Esfuerzo total estimado: fase 1 (MVP) 2–4 días de agente más una revisión; fase 2 3–5 días más una sesión con iPhone desbloqueado. Opción C: no recomendada.

---

## C. MVP propuesto

### C.1 Alcance

Demostrar, en Editor y sin intervención del PO, que AutoQA:

1. arranca la escena `Main` real con defaults en memoria (P2, M2-4, práctica activa) a 1206×2622;
2. espera una recepción real (SL1) y lee su T* presentado;
3. ejecuta gestos M2-4 sintéticos (Began sobre el frontis → Moved → Ended) con timing respecto de T*;
4. deja que el juego decida y registre el resultado;
5. recoge la evidencia del JSONL del juego y la compara con un oráculo independiente;
6. repite cada escenario en instancias frescas y comprueba el determinismo;
7. sale con código distinto de cero ante FAIL, INCONCLUSIVE o NOT_EVALUATED.

Fuera del MVP: dispositivo, modo tiempo real, pausa por UI, SL2–SL6, trazas humanas, First Rally/Match, capturas.

### C.2 Escenarios del MVP

Defaults vigentes: `timing.assistMs=40`, `validMs=100`, `validLimit=1`, `latencyCompMs=0`, `physics.stepHz=120` (paso de 8,33 ms). Todos los offsets dejan al menos dos pasos de margen respecto de cualquier frontera.

| Id | Entrada | Esperado (oráculo) |
|---|---|---|
| `M24-PERFECT-SL1` | Began en (3,2; 2,2) m del frontis, tirón de 18,5 mm, Ended con Δ = 0 ms | Cierre `Swing`; resultado `Perfect`; `Applied=true`; exactamente una presentación `Perfect`; cero discrepancias; primer contacto `FrontWall` a ≤ 0,05 m de `TargetUsed`; `PredictionMatchesBitwise` |
| `M24-VALID-EARLY-SL1` | Igual, Δ = −70 ms | `Valid`, `Applied=true`, una presentación `Valid` |
| `M24-FAILED-LATE-SL1` | Igual, Δ = +170 ms | `Failed` con `Fail=Late`; `Applied=false`; una presentación `Failed`; la pelota sigue la predicción original |
| `M24-NOPULL-SL1` | Began sobre el frontis, Ended en el mismo punto a los 300 ms | Cierre `AbandonedNoPull` (300 ms > `gesture.tapMaxMs`=250, desplazamiento 0 < `tapMaxMm`=3: no es tap; el oráculo lo deriva de la configuración); `AttemptConsumed=false`; al terminar la recepción, `NoOrder`; cero presentaciones de contacto |
| `M24-REJECTED-SL1` | Began dentro del área segura pero fuera del frontis | Cierre `Rejected`; sin Swing; `NoOrder` al final |
| `CANARY-WRONG-EXPECTATION` | Igual que PERFECT, pero el escenario espera `Failed` | El informe **debe** dar FAIL. Prueba que el arnés detecta discrepancias; no cuenta para el gate |

Cada escenario se repite 3 veces en instancias frescas de la app.

### C.3 Componentes (todos nuevos y aislados)

```
Assets/SquashClash/Tests/AutoQA/
  SquashClash.AutoQA.asmdef      (Core, Runtime, Unity.InputSystem; TestAssemblies; sin DevPanel)
  Scenario.cs                    (modelo + carga JSON)
  Scenarios/mvp.json             (catálogo)
  TouchScript.cs                 (intención → muestras en px vía mm/px del juego)
  ScriptedPlayer.cs              (lee observables permitidos, produce TouchScript)
  DeterministicDriver.cs         (bombea el bucle en orden documentado)
  Oracle.cs                      (reglas de la spec → esperado; nunca llama a ContactContract)
  EvidenceCollector.cs           (copia el JSONL, SHA-256, estado del host)
  Report.cs                      (JSON + Markdown; NotEvaluated ≠ PASS)
  AutoQAGuardTests.cs            (API prohibidas, aislamiento de presets)
  AutoQAHarnessTests.cs          (TDD de oráculo, normalizador e informe)
  AutoQAScenarioRuns.cs          ([UnityTest] por escenario)
scripts/autoqa.sh                (ejecuta, valida artefactos frescos, imprime RESULT)
docs/autoqa/README.md            (contrato, orden de bucle, uso, reversión)
```

### C.4 Contrato de observables y escrituras (para aprobar)

**Lectura permitida** (equivale a lo que ve o configura un jugador o el PO):
`ShotController.Plan/Presented/LastRecord/LastPresentation/PresentationCount/PresentationDiscrepancyCount/AttemptConsumed`, `Host.ClockMap` (solo `TrySimToReal`), `Host.LaunchId`, `Host.ObservedRealTime`, `Host.Current`, `Host.Prediction`, `Host.ActiveConfig.Hash/SimulationKey/Backend`, `Camera.main` (proyección), `Screen.*`, `app.Metrics.CurrentFilePath`, `ShotFeedback.LastClip` y contadores.

**Escritura permitida (solo preparación y driver)**:
- `Tuning.LoadPreset(PresetCodec.FromSet(Defaults, …))` en memoria;
- `Tuning.Set("practice.launch", n)` (lo mismo que hace el PO desde DEV);
- `ResumeOverlay.Continue()` (equivale a tocar «Toca para continuar»);
- `TouchInputAdapter.ReadRealTime` (reloj virtual);
- `enabled=false` de Host/App/Controller/Practice y llamadas a `Advance`, `Tick`, `ProcessPending`, `CloseReception` y `FlushRecords`, solo dentro del driver;
- `InputSystem.AddDevice/QueueStateEvent/Update/RemoveDevice`.

**Prohibido** (guardia automática): `ShotController.Enqueue`, `ClosedGesture`, `ApplyShot`, `ScheduleShot`, `Clock.Paused`, `Clock.Configure`, `InputAreaOverride`, `UiBlocker`, `CancelSystem`, `Presets.TrySave`, `TrySetLastActive`, `TryClearLastActive`, cualquier `Tuning.Set` fuera de la lista blanca, reflexión sobre tipos de `SquashClash.Core/Runtime`, escribir campos de `ShotRecord` y referencias a `DevPanel`.

### C.5 Formato de informe

`Logs/autoqa/<runId>/attempts.jsonl` (una línea por intento), `summary.json`, `report.md` y `game-metrics/<n>.jsonl` (copias del juego con SHA-256).

```json
{
  "schema": "autoqa-attempt/1",
  "runId": "20261106T150000Z-27058e8",
  "scenario": {"id": "M24-PERFECT-SL1", "sha256": "…", "repetition": 2},
  "build": {"gitHash": "27058e8", "dirty": false, "profile": "editor", "unity": "6000.3.27f1", "driver": "editor-deterministic"},
  "config": {"effectiveHash": "…", "simulationKey": "166a96e319c7", "gestureHash": "…", "preset": "AutoQA defaults", "pending": 0, "expectedHash": "…", "hashMatch": true},
  "environment": {"screen": [1206, 2622], "safeArea": [0, 0, 1206, 2622], "dpiSource": "NominalFallback", "mmPerPx": 0.0552, "uiBlockedAtAnchor": false},
  "input": {"intent": {"recipe": "SL1", "aimWorld": [3.2, 2.2], "pullMm": 18.5, "releaseOffsetMs": 0},
            "samples": [{"phase": "Began", "t": 12.3456, "x": 603.1, "y": 1410.2}]},
  "ids": {"launchId": 3, "attemptId": "<de shot-gesture>"},
  "expected": {"oracle": "timing-window/1", "closure": "Swing", "result": "Perfect", "fail": "None", "applied": true, "presentations": 1, "rationale": "|Δ|=0 ≤ assistMs−2 pasos"},
  "observed": {"closure": "Swing", "result": "Perfect", "fail": "None", "applied": true, "deltaMs": -0.4, "contactStep": 412,
               "quantizationErrorMs": 3.1, "presentations": 1, "presentationCategory": "Perfect", "discrepancies": 0,
               "clip": "contact-perfect", "ballAtContact": [3.1, 0.9, 6.8], "frontContact": [3.19, 2.21], "targetUsed": [3.2, 2.2]},
  "invariants": [{"id": "single-presentation", "pass": true}, {"id": "prediction-bitwise", "pass": true}, {"id": "presets-unchanged", "pass": true}],
  "metrics": {"solverCostMs": 0.2, "jumpBallDiameters": 0.0},
  "artifacts": {"gameMetrics": "game-metrics/7.jsonl", "sha256": "…"},
  "verdict": "PASS",
  "failure": null,
  "repro": "scripts/autoqa.sh mvp --scenario M24-PERFECT-SL1 --repetition 2"
}
```

Reglas: el bloque `observed` se rellena **solo** a partir del JSONL del juego y de propiedades de lectura; los ids no se inventan. Valores de `verdict`: `PASS`, `FAIL`, `INCONCLUSIVE` (precondición no cumplida: hash, pending, área de UI, resolución) o `NOT_EVALUATED`. Solo `PASS` cuenta. `summary.json` agrega por escenario, determinismo (hash de `observed` normalizado por repetición) y el gate global.

### C.6 Qué se verifica automáticamente y qué sigue siendo tuyo

| Verificable por AutoQA | Sigue siendo del PO en iPhone |
|---|---|
| Clasificación Perfect/Valid/Failed, Air/Late, KIn/KOut | Si el timing *se siente* justo y legible |
| Cierres M2-4, cancelaciones, rechazos, un intento por recepción | Si el gesto es cómodo, si apuntar es natural |
| Contacto aplicado/rechazado, `Applied`, `Reason`, paso de contacto | Si el golpe *parece* responder a lo que hiciste |
| Entrega única de feedback y cero discrepancias | — |
| Que se **solicita** el clip correcto (`LastClip`, contadores) y que se llama al backend háptico (doble de grabación) | Que el sonido **se oye** (silencio del iPhone, volumen, sesión de audio) y que la vibración **se siente** |
| Integridad de hash/SimulationKey/pending y presets sin cambios | P0-calib-1 en el iPhone (fase 2 puede cotejar bytes por devicectl) |
| Determinismo y regresiones de física (C1–C17 ya existentes) | Rendimiento percibido, térmico y frames largos reales (las métricas nativas sí son automatizables en fase 2) |
| Animaciones: que el estado de swing/destello existe y se congela en pausa (como las pruebas actuales) | Si la animación se ve bien |
| — | Latencia toque→imagen, oclusión por el dedo, legibilidad, diversión |

**Solicitar un sonido frente a oírlo**: AutoQA puede demostrar que el juego decidió reproducir `contact-perfect` una sola vez para el intento X. En iPhone (fase 2) puede además registrar que `AudioSource.isPlaying` pasó a verdadero y que `AudioListener` está activo (el defecto de PR #35). Ninguna de las dos cosas demuestra audibilidad: el interruptor de silencio, el volumen, la ruta de audio y tu oído quedan fuera. Eso seguirá siendo una verificación tuya, cada vez más corta.

### C.7 Criterios de aceptación objetivos

1. `git diff --stat main...HEAD` solo contiene `Assets/SquashClash/Tests/AutoQA/**`, `scripts/autoqa.sh`, `docs/autoqa/**`, `.gitignore` (si hace falta) y los registros de `AGENTS.md`/aprobaciones. Cero cambios en `Core/`, `Runtime/`, `DevPanel/`, `Editor/`, `Resources/`, `ProjectSettings/`, `Packages/`.
2. `AutoQAGuardTests` en verde y con RED válido demostrado: una línea prohibida temporal la hace fallar por aserción.
3. `scripts/autoqa.sh mvp`: 5 escenarios × 3 repeticiones = 15 intentos PASS; el canario da FAIL y se reporta aparte; exit 0 solo si los 15 pasan y el canario falla.
4. Determinismo: el `observed` normalizado es idéntico en las 3 repeticiones de cada escenario. Los campos excluidos están listados y justificados.
5. Hash efectivo, SimulationKey y pending=0 verificados antes de cada intento. Si no coinciden → INCONCLUSIVE, nunca PASS.
6. SHA-256 del directorio de presets del Editor idéntico antes y después. JSONL del juego copiado con su hash.
7. Regresión fresca intacta: `scripts/test.sh editmode` y `playmode` con los totales vigentes (546 y 354 a la fecha; verificar al empezar) y `scripts/check-release.sh` dev/prod OK con `gitDirty=false`; AutoQA no aparece en los Players.
8. Ejecución completa ≤ 5 min en el Mac, con un solo comando, sin el PO.
9. Revisión independiente por el otro agente aprobada.

### C.8 Mecanismo de reversión

Todo es aditivo y está aislado: un ensamblado de pruebas, un script y documentación. Revertir = `git revert -m 1 <merge>` o borrar `Tests/AutoQA/` y `scripts/autoqa.sh`. No hay migración de datos, presets, defaults ni hashes que restaurar. Defaults `c0d8ec7050e0`, SimulationKey `166a96e319c7` y P0 del iPhone no se ven afectados.

---

## D. Impacto en el roadmap

- **Estado** [R]: S3 está fusionado (PR #34/#35). La validación física S3-A–E está en curso o en pausa en el worktree `s3-validation` de Codex. PR #32 está abierta en espera. S4 no está autorizado.
- **S4 depende de capacidades de AutoQA**: la tarea 4.3 (bots de anticipación desde el T* visible, deterministas por semilla) *es* un jugador de AutoQA. La tarea 4.4 (escenarios deportivos X1–X5) necesita ejecutar golpes reales con solver y física. La tarea 4.5 (sesiones con el PO) se acorta si las comprobaciones funcionales ya llegan hechas.
- **Recomendación**: implementar el MVP **ahora, antes de S4**, en paralelo con el cierre de S3 o justo después. No toca producto, dispositivo ni la sesión física. Solo hace falta un worktree propio para no chocar con el bloqueo del proyecto Unity.
- **Costo de oportunidad**: un incremento pequeño (2–4 días de agente, dos aprobaciones tuyas de unos 15 minutos cada una) que retrasa el arranque de S4 en ese margen.
- **Beneficio esperado** [J]: hoy cada tanda física cuesta horas de tu tiempo; en S2 hubo 206 recepciones manuales para medir cosas que en buena parte son funcionales. Con AutoQA, las comprobaciones funcionales (clasificación, cierres, entrega única, integridad) se hacen en minutos y en cada PR. Tus sesiones se reducen a percepción: sonido, háptica, sensación, diversión. Además, cada regresión de los contratos se detecta antes de llegar al iPhone.
- **Fase 2** (después del MVP y antes de S4.5 si lo autorizas): modo tiempo real y ejecución en iPhone dev, que resuelve “pasa solo en Editor” y aporta métricas nativas por escenario.
- **First Rally/Match**: requisitos de extensibilidad ya cubiertos por el diseño. El `ScriptedPlayer` pasa a ser una interfaz `IAutoPlayer`. Las secuencias de recepciones usan LaunchId crecientes, que ya existen. El rival IA será código de producto, no del arnés. Las corridas largas (cientos de intercambios) son viables en modo determinista porque P2 es analítico y barato [POC: medir recepciones/minuto]. La detección de bloqueos se resuelve con un watchdog de pasos sin progreso de fase.

---

## E. Riesgos y protecciones

| Riesgo | Control |
|---|---|
| Validar mocks en lugar del juego | Escena `Main` real y `AppBootstrap` real; ningún doble salvo el backend háptico documentado; la guardia prohíbe tipos de reemplazo y reflexión |
| Resultados prefabricados (Perfect porque sí) | El arnés solo produce toques; `observed` sale del JSONL del juego; prohibidos `Enqueue/ApplyShot/ScheduleShot`; canario obligatorio que debe fallar |
| Oráculo que copia al código | Oráculo escrito desde la spec (ventanas de timing y tabla de cierres) con su propio TDD; prohibido llamar a `ContactContract`/`ContactPlanner`; márgenes ≥ 2 pasos en las fronteras |
| Dependencia de tiempos de máquina | Driver determinista con reloj virtual; campos de pared excluidos y listados; determinismo exigido en 3 repeticiones; el modo tiempo real queda como fase aparte con tolerancias predeclaradas |
| Pasa solo en Editor | Declarado en cada informe (`driver: editor-deterministic`); fase 2 en iPhone dev con los mismos escenarios; las validaciones físicas del PO no se sustituyen sin tu decisión |
| Diferencias DEV/PROD | MVP sin DevPanel y con AutoQA excluido de los Players (`check-release`); en fase 2 el ensamblado solo dev se verifica ausente en prod |
| Modificar P0 accidentalmente | En Editor P0 no existe (vive en el iPhone); presets por SHA-256 antes y después; prohibidas las APIs de guardado. En fase 2: respaldo y cotejo byte a byte con el protocolo actual |
| Duplicar lógica de producción | El arnés no recalcula P ni la proyección: convierte mm→px con `ScreenMetrics` y proyecta con `Camera.main`, igual que el juego; el oráculo es una tabla de reglas, no una reimplementación del contrato |
| Fragilidad ante cambios visuales | Nada de comparación de píxeles; posiciones derivadas de la cámara y de coordenadas del frontis; precondición de UI no bloqueante en el ancla |
| Instrumentación que afecta al rendimiento | MVP en Editor (no mide rendimiento); en fase 2 el registro diferido existente y la medición con y sin AutoQA para cuantificar el overhead |
| Ocultar defectos reales del dispositivo | Cada informe declara qué capa no ejercita (digitalizador, UIKit, audio audible); se mantiene una lista de “comprobaciones que siguen siendo del PO” |
| Bloqueo del proyecto Unity con el Editor abierto | Worktree propio, como en los incrementos anteriores |

---

## F. Recomendación final

**GO CON CONDICIONES.**

Evidencia: la entrada es Input System con un adaptador que acepta cualquier `Touchscreen` (`TouchInputAdapter.cs:49`). Ya existe inyección con tiempo exacto en pruebas (`ShotControllerTests.Send`, `M24PreviewTests.Send`). El host acepta avance explícito (`BallSimulationHost.Advance`). El juego ya escribe evidencia autoritativa por intento (JSONL `shot-*`, `m24-gesture`). La regresión completa dura unos 71 s. S4.3 ya pide bots de esta clase. Todo esto permite un MVP sin cambios de producción.

Condiciones:

1. **Apruebas el contrato de observables y escrituras** (C.4), incluida la lista blanca de preparación y las prohibiciones.
2. **Aceptas que el MVP es Editor determinista** con el bucle orquestado por el arnés (desviación documentada). Su resultado no sustituye validaciones físicas hasta la fase 2 y tu decisión.
3. **Cero cambios de producción.** Si el agente descubre que necesita un seam nuevo, se detiene y te consulta.
4. **Puerta POC dentro del MVP**: resolución 1206×2622 en batchmode, ancla del frontis dentro del área segura y sin bloqueo de UI, y resultados idénticos entre repeticiones pese al reloj de pared de `PresentCurrentPlan`. Si algo falla: registrar y detenerse.
5. Un solo agente ejecutor y el otro revisor independiente; puerta del PO antes de implementar y antes de fusionar.

Prompts: `autoqa-prompt-claude.md` y `autoqa-prompt-codex.md`, en la misma carpeta.
