# Squash Clash: First Shot Implementation Plan (r2)

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.
>
> **En este proyecto:** Codex implementa en el Mac del PO, Claude revisa cada PR y el PO prueba cada incremento en el iPhone y aprueba antes de fusionar. **Este plan no autoriza implementación:** el PO lo revisa y autoriza S1 de forma explícita. Cada incremento siguiente requiere su propia autorización.

>
> **Revisión r2 (2026-10-25):** incorpora la [revisión del plan de Codex](../../reviews/2026-10-25-first-shot-r2-plan-review-codex.md) (F01–F10) y las decisiones R2-4 y R2-5 del PO (propuesta §11.1).

**Adenda D1 — PO 2026-10-27, tarea 1.2b en S1:** S1-sling del control §6.2 B se desarrolla bajo la secuencia del [registro literal](../../reviews/2026-10-25-first-shot-po-approvals.md). **Primera parada: un commit solo Markdown y PR en borrador de interacción; detenerse. No código antes de aprobación de Claude.** El [diseño de interacción y protocolo B0–B5](../../reviews/2026-10-27-first-shot-s1-sling-prototype-interaction.md) concreta esta parada. Las tareas históricas de A′ siguen intactas; S2–S4/T8 no autorizados.

**Goal:** que el PO pueda, en su iPhone 16 Pro y con una mano, apuntar al frontis, pegarle a una pelota del lanzador con la fuerza y el momento que elija, y sentir con claridad la diferencia entre un golpe perfecto, uno válido y uno fallido.

**Architecture:** el mismo patrón que First Ball. La lógica nueva vive en **Core/Shot**, en C# puro con tipos matemáticos de Unity y probada en EditMode: gesto, mira, potencia, mapa de relojes, planificador estático, contrato de contacto, solver y calidad causal. **Runtime** añade el adaptador táctil del Input System, el golpe retroactivo dentro de `BallSimulationHost` (única autoridad), el modo práctica, las vistas y la retroalimentación (audio y háptica nativa). **DevPanel** añade parámetros, superposición y registro. No se modifica P2: el golpe se aplica en un **paso completo** (R2-1) y se resimula con el mismo camino de pasos.

**Tech Stack:** Unity 6000.3.27f1, C#, URP, Input System 1.20.1 (flujo de eventos táctiles), UI Toolkit, Unity Test Framework (EditMode/PlayMode, `InputTestFixture`), plug-in nativo iOS en Objective-C++ (`UIImpactFeedbackGenerator`), IL2CPP/ARM64, Xcode 27.1, `xcrun devicectl`.

**Spec:** `docs/superpowers/specs/2026-10-24-first-shot-design.md` **r2** (normativa su §11), sobre la spec v0.3 (`docs/superpowers/specs/2026-10-20-squash-clash-design-v0.3.md`, con D30–D35). Revisión técnica: `docs/reviews/2026-10-24-first-shot-design-review-codex.md` (los códigos `[A01]…[B01]` remiten a ella).

## Global Constraints

- iPhone 16 Pro, **solo vertical**, una mano. Objetivo de **60 FPS** sin cambios (`Application.targetFrameRate = 60`).
- **Una sola autoridad sobre la pelota:** el golpe es una operación del host; nadie más escribe el estado (§8.2).
- **P2-Analytic** es el backend de First Shot. Con P1 el modo práctica se declara «no disponible con P1» y no golpea [A01, §11.2].
- P2 **no se modifica.** El golpe físico se aplica en el **paso más cercano a r**; error máximo de medio paso, registrado (R2-1).
- **Predicción bit a bit** con el juego después del golpe, por el mismo camino de pasos (§8.2.1, §17.A.3).
- **A = 40 ms** reales para Perfecto. **V experimental 80/100/120 ms** (inicial 100) con **T3 puro** siempre disponible (C1, condición 2 del PO). A, V y compensación en **ms reales**, no de simulación [A02].
- **Un intento por pelota** (D19). **N1:** cancelar exige salir del círculo de ~6 mm y volver (regreso a distancia ≤ 6 mm); no consume el intento (R2-2). **N2:** desplazamiento máximo **estrictamente** < 3 mm en **estrictamente** < 250 ms es un toque: sin swing ni intento (F10).
- **Legalidad de la recepción:** frontis previo y **exactamente un bote no persistente** antes del contacto (sin volea), dentro del alcance y entre `legal.minHeight` y `legal.maxHeight` (F01). La legalidad se decide en el **instante exacto r** con los tiempos de evento de P2 (`TimeInStep`); el paso físico usa R2-1 y, en los bordes, **R2-4**: paso legal más cercano del mismo lado, error ≤ 1 paso, nunca contacto ilegal.
- **Calibración de latencia = offset perceptivo conjunto** (R2-5), sin atribuirlo al dispositivo.
- Penalización **solo por geometría** y **sin desvío lateral** (C2, §17.A.12). Capa aleatoria **apagada**.
- **Sin suavizado** del salto visual del contacto retroactivo; se mide (condición 1 del PO).
- `DefaultPreset.json` sigue siendo la **única fuente de defaults**. Las claves nuevas cambian el hash global; los **41 valores heredados y la SimulationKey física no cambian** (R2-3). P0-calib-1 no se promueve (D35).
- L1–L6, sus pruebas y `Ballistics.SolveCenterTo` **no se modifican.** Única excepción (Tarea 2.6): una prueba First Ball que dependa del lanzamiento al arrancar puede fijar `practice.enabled = 0` en su preparación, **conservando su aserción y el escenario que verificaba**, y se declara en la PR.
- **Invariantes de configuración** (F08): `power.deadZoneMmPerS < power.fullMmPerS`, `shot.vMin ≤ shot.vMax ≤ shot.vCap`, `body.preferredMinHeight < body.preferredMaxHeight`, `legal.minHeight < legal.maxHeight`, `timing.assistMs < timing.validMs` en modo V. Se validan **atómicamente** al aplicar, importar y capturar; si fallan, se muestra el error y se conserva el último `ShotConfig` válido, sin corregir valores en silencio.
- Panel excluido de prod (§17.A.10). Gesto, mira, anillo, notas, sonido, háptica y modo práctica son **runtime** y existen en prod.
- **No se usan:** frameworks genéricos de input, audio o habilidades; ECS; inyección de dependencias; buses globales.
- La desviación H17 de First Ball **no se extiende** a First Shot; las metas de §17.C siguen vigentes.
- Documentación en español; identificadores, código y commits en inglés. Codex no modifica la spec: las discrepancias van a Claude, que las presenta al PO.
- **Perfil de firma (dato vigente, 11 nov 2026):** el PO renovó el perfil el 2026-11-11T13:50:05Z; vence el **2026-11-18T13:50:05Z (10:50:05 America/Santiago del 18 de noviembre de 2026)**, con el mismo certificado, equipo, dispositivo y entitlements. La app instalada sigue firmada con el perfil anterior (tanda 4 PROD, creado el 2026-11-05T13:55:07Z; [informe](../../reviews/2026-11-05-first-shot-s2-tanda4-prod.md)), que vence el **2026-11-12T13:55:07Z (10:55:07 America/Santiago)**; después de esa hora hay que reinstalar con el perfil nuevo. Los vencimientos 2026-10-28T14:59:31Z y 2026-11-12T13:55:07Z quedan como datos históricos. Comprobar vigencia efectiva antes de toda instalación; no instalar si el perfil no está vigente.

## Review Focus

1. **La liberación llega después de que la pelota ya botó dos veces o salió de K** (entrega tardía). Se resuelve por el **tiempo del evento**, no por el estado al procesarlo. → Tarea 2.5, `ApplyShot_LateDeliveryAfterSecondBounce_ResolvesByEventTime`.
2. **La app pasa a segundo plano, llega una notificación o se abre el panel en pleno gesto.** El gesto se cancela, no se consume el intento y al volver no se ejecuta un swing. → Tarea 1.1, `SystemCancel_MidDrag_DoesNotConsumeAttempt`; Tarea 2.6, `FocusLoss_DuringGesture_NoSwingOnResume`.
3. **`Screen.dpi` devuelve 0 o un valor incoherente.** Se usa la calibración declarada, se marca en el registro y no hay división por cero. → Tarea 1.3, `ScreenMetrics_ZeroDpi_UsesDeclaredFallbackAndFlagsIt`.
4. **Una liberación del lanzamiento anterior llega después de un relanzamiento automático.** Se descarta y no toca la pelota nueva. → Tarea 2.5, `ApplyShot_FromPreviousLaunchId_IsRejected`.
5. **El PO cambia parámetros en el panel con la pelota en el aire.** La recepción en curso usa la configuración capturada; el rollback nunca lee valores nuevos. → Tarea 2.6, `PanelChange_MidReception_DoesNotAffectCurrentShot`.

---

## Modelo de trabajo

| Incremento | Tareas | Rama | Qué prueba el PO en el iPhone |
|---|---|---|---|
| **S1** Gesto y mira | 1.0–1.4, incluida adenda1.2b | `firstshot/s1-gesture` | Apuntar, entender la mira, potencia suave/fuerte en la superposición; medición del muestreo táctil y de los mm |
| **S2** Golpe funcional | 2.1–2.7 | `firstshot/s2-contact` | **Primer golpe jugable**: la pelota va donde apunta, el timing es justo, salto visual medido |
| **S3** Sensación | 3.1–3.4 | `firstshot/s3-feel` | Perfecto frente a normal, sonido, vibración, swing visible, entender el fallo |
| **S4** Calibración y validación | 4.1–4.5 | `firstshot/s4-validation` | Protocolo completo y decisión: continuar a First Rally, ajustar o volver |

Reglas, iguales a First Ball:
- Una rama y una PR por incremento, commits pequeños, TDD con `scripts/test.sh editmode|playmode [filtro]`: **RED válido observado antes de implementar** y suites completas GREEN después. No vale un verde con cero tests, XML viejo, errores CS o builds anteriores.
- **RED válido = aserciones ejecutadas que fallan** (F06). Cada tarea crea primero los **esqueletos compilables** de su API (firmas del bloque Interfaces que devuelven valores neutros o lanzan `NotImplementedException` capturada por la aserción), y el RED se observa con los tests compilando y descubiertos. Un error CS o un cambio de montaje de pruebas nunca cuenta como RED.
- Al cerrar cada incremento: regresión completa (EditMode y PlayMode), `scripts/check-release.sh` dev/prod, build dev instalada en el iPhone tras comprobar el perfil, informe en `docs/reviews/` y PR lista para Claude **sin fusionar**. Fusiona solo con la revisión de Claude y la aprobación del PO.
- **Nota vigente (10 nov 2026, 13:39 America/Santiago, autorización del PO vía SC-Lead):** `scripts/autoqa.sh mvp` es obligatorio cuando el cambio toca timing, gesto o contrato, cualquiera de los tres. El gate oficial de PlayMode es 354/354 en el Mac del PO; detalle en `AGENTS.md`.
- Los parámetros `[EXPERIMENTO]` de este plan son valores de partida; cambiarlos con el panel no requiere aprobación. Cambiar criterios, contratos o decisiones del PO sí.

## Estructura de archivos

**Core** (`Assets/SquashClash/Core/Shot/`, nuevo, namespace `SquashClash.Core.Shot`):

| Archivo | Responsabilidad |
|---|---|
| `TouchSample.cs` | Muestra táctil inmutable: dedo, fase, posición en px, timestamp `double`, secuencia |
| `GestureTracker.cs` | Máquina de gesto: toque (N2), arrastre, cancelación armada/regreso (N1), cancelación del sistema, primer dedo |
| `AimModel.cs` | Mira en el frontis (m), ganancia mm→m, posición inicial, límites, congelamiento con histéresis |
| `PowerEstimator.cs` | Potencia por desplazamiento neto en ventana, cobertura mínima, salida conservadora |
| `ClockMap.cs` | Segmentos `(tReal, paso, τ, cámara lenta, pausado)` por lanzamiento; conversión real↔simulación |
| `BodyFrame.cs` | Cuerpo fijo por lanzamiento: posición, mano, dirección frontal, alcance, punto ideal, lado drive/revés |
| `ContactPlanner.cs` | Planificador estático: K, P\*, T\*, legalidad mínima de la recepción |
| `ContactContract.cs` | Clasificación de la liberación: Perfecto/Válido/Fallido y casos §7.1.3 |
| `ShotSolver.cs` | Velocidad total, arco bajo, infactible (subir v, bajar y) |
| `CausalQuality.cs` | (S3) Vectores no direccionales: timing por geometría, E3, E5 |
| `ShotRecord.cs` | Registro por golpe serializable a `MetricsLog` |

**Runtime**:

| Archivo | Responsabilidad |
|---|---|
| `Runtime/Input/TouchInputAdapter.cs` | Flujo de eventos del Input System → `TouchSample`; área segura; gestos de borde diferidos |
| `Runtime/Input/ScreenMetrics.cs` | DPI, escala px↔mm, calibración declarada y bandera de procedencia |
| `Runtime/Ball/BallSimulationHost.cs` (modificar) | Historial por paso, `LaunchId`, `ApplyShot`, `ScheduleShot`, reconciliación, `PredictionReplaced`, medición del salto |
| `Runtime/Shot/ShotController.cs` | Orquesta gesto → r → contrato → solver → host; captura de configuración por recepción |
| `Runtime/Shot/PracticeMode.cs` | Seis lanzamientos §6.3, cuerpo por lanzamiento, repetición propia |
| `Runtime/Shot/AimReticleView.cs`, `TimingRingView.cs`, `ShotNoteView.cs` | Mira (estados apuntando/fijada/intención/factible), anillo, notas |
| `Runtime/Feedback/ShotFeedback.cs` | (S2 provisional, S3 completo) Audio precargado, háptica, destello |
| `Runtime/Feedback/Haptics.cs` + `Assets/Plugins/iOS/SquashHaptics.mm` | (S3) Puente nativo; no-op fuera de iOS |
| `Runtime/Shot/SwingView.cs` | (S3) Brazo y raqueta simples con swing procedural |
| `Runtime/Resources/Audio/` + `PROVENANCE.md` | (S2/S3) Clips con procedencia |

**DevPanel**: `ShotSection.cs` (interruptores y lectura), `ShotOverlay.cs` (P\*, T\*, K, Δ, categoría, potencia, muestreo, salto, vectores).

**Config** (modificar): `ParamCatalog.cs` y `Runtime/Resources/DefaultPreset.json` con las claves de la tabla siguiente.

### Claves nuevas (todas `[EXPERIMENTO]` salvo las marcadas)

| Clave | Tipo | Rango | Default | Aplicación | Tarea |
|---|---|---|---|---|---|
| `aim.gainMPerMm` | Float | 0,03–0,30 | 0,12 | Inmediata, congelada al apoyar | 1.2 |
| `aim.startHeight` | Float | 0,5–4,5 | 2,2 | Inmediata, al apoyar | 1.2 |
| `aim.flickMmPerS` | Float | 50–1000 | 250 | Inmediata, al apoyar | 1.2 |
| `aim.flickHysteresisMmPerS` | Float | 0–200 | 40 | Inmediata, al apoyar | 1.2 |
| `aim.freezeLookbackMs` | Float | 0–100 | 50 | Inmediata, al apoyar | 1.2 |
| `power.windowMs` | Float | 60–100 | 80 | Inmediata, al apoyar | 1.2 |
| `power.minCoverageMs` | Float | 10–60 | 30 | Inmediata, al apoyar | 1.2 |
| `power.minSamples` | Int | 2–8 | 4 | Inmediata, al apoyar | 1.2 |
| `power.deadZoneMmPerS` | Float | 0–400 | 100 | Inmediata, al apoyar | 1.2 |
| `power.fullMmPerS` | Float | 300–4000 | 1500 | Inmediata, al apoyar | 1.2 |
| `power.curveExponent` | Float | 0,5–3 | 1 | Inmediata, al apoyar | 1.2 |
| `gesture.tapMaxMm` | Float | 1–6 | 3 (spec §6.5) | Inmediata, al apoyar | 1.1 |
| `gesture.tapMaxMs` | Float | 100–400 | 250 (spec §6.5) | Inmediata, al apoyar | 1.1 |
| `gesture.cancelRadiusMm` | Float | 3–12 | 6 (spec §6.5) | Inmediata, al apoyar | 1.1 |
| `input.fallbackDpi` | Float | 100–600 | 460 (ficha iPhone 16 Pro, **nominal**, no verificada) | Inmediata | 1.3 |
| `power.maxGapMs` | Float | 10–60 | 34 (≈ 2 intervalos a 60 Hz) | Inmediata, al apoyar | 1.2 |
| `aim.flickConfirmSamples` | Int | 1–4 | 2 | Inmediata, al apoyar | 1.2 |
| `legal.minHeight` | Float | 0,02–0,3 | 0,05 [EXPERIMENTO; spec `h_min`] | Por lanzamiento | 2.2 |
| `legal.maxHeight` | Float | 1,2–3,0 | 2,0 [EXPERIMENTO; spec `h_max`] | Por lanzamiento | 2.2 |
| `practice.observeMaxSeconds` | Float | 1–8 | 4 | Por lanzamiento | 2.6 |
| `timing.assistMs` | Float | 0–80 | 40 (D19) | Por recepción | 2.3 |
| `timing.validLimit` | Choice | 0 = T3 puro, 1 = V | 1 | Por recepción | 2.3 |
| `timing.validMs` | Choice | 80 / 100 / 120 | 100 | Por recepción | 2.3 |
| `timing.earlyPolicy` | Choice | 0 = T3, 1 = T3-suave | 0 | Por recepción | 2.3 |
| `timing.latencyCompMs` | Float | −100–100 | 0 | Por recepción | 2.3 |
| `timing.showNotes` | Bool | | 1 | Inmediata | 2.6 |
| `body.reach` | Float | 0,8–1,8 | 1,3 (§9.1) | Por lanzamiento | 2.2 |
| `body.idealLateral` | Float | 0,3–1,0 | 0,6 | Por lanzamiento | 2.2 |
| `body.preferredMinHeight` | Float | 0,2–0,8 | 0,4 (§9.1) | Por lanzamiento | 2.2 |
| `body.preferredMaxHeight` | Float | 0,8–1,6 | 1,1 (§9.1) | Por lanzamiento | 2.2 |
| `body.wallClearance` | Float | 0–0,6 | 0,3 (§9.1) | Por lanzamiento | 2.2 |
| `shot.vMin` | Float | 2–15 | 6 | Por recepción | 2.4 |
| `shot.vMax` | Float | 10–45 | 28 | Por recepción | 2.4 |
| `shot.vCap` | Float | 10–50 | 32 | Por recepción | 2.4 |
| `practice.enabled` | Bool | | 1 | Al lanzar | 2.6 |
| `practice.launch` | Choice | SL1…SL6 | SL1 | Al lanzar | 2.6 |
| `practice.repeatSeconds` | Float | 0,5–5 | 1,5 | Inmediata | 2.6 |
| `rollback.historyMs` | Float | 100–500 | 250 | Por lanzamiento | 2.5 |
| `causal.*` | ver Tarea 3.1 | | | Por recepción | 3.1 |
| `feedback.haptics` | Bool | | 1 | Inmediata, **runtime** | 3.2 |
| `timing.aidLevel` | Choice | 0 completa, 1 parcial, 2 sin anillo | 0 | Por recepción | 4.2 |

«Al apoyar» y «por recepción» se implementan capturando un `ShotConfig` inmutable (Tarea 2.6). `ApplyClass` no cambia: estas claves usan `Immediate` en el catálogo y la captura hace el resto.

---

## Etapa S1 — Gesto táctil y mira

### Tarea 1.0: Montaje de pruebas del Input System [F06]

**Files:**
- Modify: `Assets/SquashClash/Tests/PlayMode/SquashClash.Runtime.Tests.asmdef` (referencias `Unity.InputSystem` y `Unity.InputSystem.TestFramework`), `Packages/manifest.json` (`"testables": ["com.unity.inputsystem"]`)
- Test: `Assets/SquashClash/Tests/PlayMode/InputFixtureSmokeTests.cs`

- [ ] **Step 1:** añadir las referencias y `testables` según [Set up test assemblies](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.20/manual/set-up-test-assemblies.html). No es un ciclo RED→GREEN: es preparación declarada en la PR.
- [ ] **Step 2:** `InputFixtureSmokeTests.VirtualTouchscreen_DeliversEventWithTimestamp` (con `InputTestFixture`) descubre y pasa; `scripts/test.sh playmode` completo sigue GREEN con el mismo número de tests previos + 1.
- [ ] **Step 3:** `scripts/check-release.sh` confirma que el framework de pruebas no entra en dev ni prod.
- [ ] **Step 4: Commit** `test: enable Input System test fixture for runtime tests`

### Tarea 1.1: Máquina de gesto (N1, N2, cancelación del sistema) [A08]

**Files:**
- Create: `Assets/SquashClash/Core/Shot/TouchSample.cs`, `Assets/SquashClash/Core/Shot/GestureTracker.cs`
- Test: `Assets/SquashClash/Tests/EditMode/Shot/GestureTrackerTests.cs`

**Interfaces:**
- Produces:
  - `public enum TouchPhaseKind { Began, Moved, Stationary, Ended, Canceled }`
  - `public readonly struct TouchSample { public readonly int FingerId; public readonly TouchPhaseKind Phase; public readonly Vector2 PositionPx; public readonly double TimeReal; public readonly long Sequence; }` con constructor completo.
  - `public enum GestureOutcome { None, Tap, Canceled, SystemCanceled, Swing }`
  - `public sealed class GestureTracker` con `GestureTracker(float tapMaxMm, float tapMaxMs, float cancelRadiusMm, float mmPerPx)`, `void Push(in TouchSample s)`, `void CancelBySystem()`, `bool Active`, `GestureOutcome Outcome`, `double ReleaseTimeReal`, `IReadOnlyList<TouchSample> Samples` (del dedo activo), `float MaxDisplacementMm`, `bool CancelArmed`, `void Reset()`.

- [ ] **Step 1: Write the failing tests**

```csharp
[Test] public void Tap_MaxDisplacementBelow3mmUnder250ms_IsTap()            // 2,9 mm, 200 ms → Tap
[Test] public void Tap_DisplacementExactly3mm_IsNotTap()                      // N2 estricto: 3,0 mm → no Tap
[Test] public void Tap_DurationExactly250ms_IsNotTap()                        // N2 estricto: 250 ms → no Tap
[Test] public void Tap_JustBelowBothLimits_IsTap()                            // 2,999 mm y 249,9 ms → Tap
[Test] public void ShortDrag_EndsInsideCancelCircleWithoutLeaving_IsSwing()  // máx. 4 mm, termina a 4 mm → Swing (R2-2)
[Test] public void DragOutAndBack_EndsInsideCircle_IsCanceled()              // sale a 9 mm, vuelve a 5 mm → Canceled
[Test] public void DragOutAndBack_ExactlyOnRadius_IsCanceled()               // termina a 6,0 mm tras salir → Canceled (radio inclusivo)
[Test] public void DragOut_EndsOutsideCircle_IsSwingWithEventTime()          // ReleaseTimeReal == timestamp del Ended, no del procesamiento
[Test] public void TapMeasuredByMaxNotFinalDistance()                         // va a 5 mm y vuelve a 0 en 150 ms → no es Tap (es Canceled solo si salió de 6 mm; aquí Swing)
[Test] public void SecondFinger_IsIgnored_AndNotPromotedWhenFirstLifts()
[Test] public void SystemCancel_MidDrag_DoesNotConsumeAttempt()              // Outcome == SystemCanceled; requiere Began nuevo
[Test] public void CanceledPhaseFromOs_IsSystemCanceled()
[Test] public void SamplesAreCopied_NotReferenced()                          // mutar el array fuente no cambia Samples
```

- [ ] **Step 2: Run** `scripts/test.sh editmode GestureTrackerTests` tras crear los esqueletos compilables. Expected: FAIL por aserciones (Outcome siempre `None`).

- [ ] **Step 3: Implement** `TouchSample` y `GestureTracker`. Estados internos: `Idle → TapCandidate → Dragging → (CancelArmed) → Done`. `MaxDisplacementMm` se mide desde el `Began` con `mmPerPx` congelado al apoyar. Prioridad al terminar: SystemCanceled → Tap (N2) → Canceled (salió y volvió, N1) → Swing.

- [ ] **Step 4: Run** `scripts/test.sh editmode GestureTrackerTests`. Expected: PASS.

- [ ] **Step 5: Commit** `feat: add one-finger gesture tracker with tap and return-to-cancel rules`

### Tarea 1.2: Mira y potencia [A06, §11.4]

**Files:**
- Create: `Assets/SquashClash/Core/Shot/AimModel.cs`, `Assets/SquashClash/Core/Shot/PowerEstimator.cs`
- Modify: `Assets/SquashClash/Core/Config/ParamCatalog.cs`, `Assets/SquashClash/Runtime/Resources/DefaultPreset.json` (claves `aim.*`, `power.*`, `gesture.*` de la tabla)
- Create: `Assets/SquashClash/Core/Shot/ShotConfigValidator.cs` (`public static bool TryValidate(ParamSet candidate, out string error)`, usado por `TuningState` al aplicar e importar)
- Test: `Assets/SquashClash/Tests/EditMode/Shot/AimModelTests.cs`, `Assets/SquashClash/Tests/EditMode/Shot/PowerEstimatorTests.cs`, `Assets/SquashClash/Tests/EditMode/Shot/ShotConfigValidatorTests.cs`, `Assets/SquashClash/Tests/EditMode/Config/DefaultPresetHashTests.cs`

**Interfaces:**
- Consumes: `TouchSample` (1.1).
- Produces:
  - `public enum AimState { Aiming, Frozen }`
  - `public sealed class AimModel` con `AimModel(AimSettings s, CourtDimensions court, bool ballOnLeftSide)`, `void Update(in TouchSample s, float mmPerPx)`, `Vector2 Aim` (x ∈ [0; 6,40], y ∈ [0; 5,0] en m del frontis), `AimState State`, `double FreezeTimeReal`, `bool FreezeUsedFirstValidPoint`.
  - `public readonly struct AimSettings { GainMPerMm, StartHeight, FlickMmPerS, FlickHysteresisMmPerS, FlickConfirmSamples, FreezeLookbackMs }`.
  - `public readonly struct PowerResult { public readonly float P; public readonly float NetSpeedMmPerS; public readonly double CoveredMs; public readonly int DistinctSamples; public readonly double MaxGapMs; public readonly bool Conservative; public readonly string Reason; }`
  - `public static class PowerEstimator { public static PowerResult Estimate(IReadOnlyList<TouchSample> samples, double releaseTimeReal, float mmPerPx, PowerSettings s); }`
  - `public readonly struct PowerSettings { WindowMs, MinCoverageMs, MinSamples, MaxGapMs, DeadZoneMmPerS, FullMmPerS, CurveExponent }`.

- [ ] **Step 1: Write the failing tests**

```csharp
// AimModelTests
[Test] public void StartPosition_IsNeutral2_2mOnBallSideDriveLine()          // y == 2,2; x en el lado de la pelota
[Test] public void Drag10mm_MovesAimByGainTimes10()                           // 0,12 m/mm → 1,2 m
[Test] public void Aim_ClampsAtFrontWallEdges()                               // nunca fuera de [0;6,40] × [0;5,0]
[Test] public void Flick_FreezesAtPositionFromLookbackBeforeCrossing()        // valor a r_cruce − 50 ms
[Test] public void Flick_WithoutHistory_UsesFirstValidPointAndFlagsIt()
[Test] public void Freeze_HappensOncePerGesture_WithHysteresis()             // ruido alrededor de v_flick no re-congela ni descongela
[Test] public void AfterFreeze_FurtherMovementDoesNotMoveAim()
[Test] public void NoiseBetweenTwoCloseSamples_DoesNotTriggerFreeze()          // F07: requiere aim.flickConfirmSamples intervalos sobre v_flick

// PowerEstimatorTests
[Test] public void NetDisplacement_OverWindow_GivesSpeed()                    // 80 mm en 80 ms → 1000 mm/s
[Test] public void ReverseInWindow_GivesLowPower()                            // ida y vuelta → p cercana a 0 (desplazamiento neto, documentado)
[Test] public void BelowDeadZone_IsZeroPower_AboveFull_IsOne()
[Test] public void CurveExponent_IsApplied()                                   // p = clamp01((v−dead)/(full−dead))^exp
[Test] public void ShortGesture_WithEnoughSamples_UsesAvailableDuration()     // 40 ms, 5 muestras → no conservadora, p alta para flick rápido
[Test] public void ShortGesture_TooFewSamples_IsConservativeMinimum()        // 3 muestras → P = 0, Conservative, Reason
[Test] public void RepeatedOrNonMonotonicTimestamps_AreConservative()
[Test] public void WindowEdges_InterpolateOnlyBetweenRealSamples()
[Test] public void DuplicateStationarySamples_DoNotCountAsDistinct()
[Test] public void EnoughSamplesButGapAboveMax_IsConservative()               // F07: hueco > power.maxGapMs dentro de la ventana
[Test] public void CoverageExactlyMin_IsValid_JustBelow_IsConservative()

// ShotConfigValidatorTests (F08)
[Test] public void DeadZoneNotBelowFull_IsRejectedAndPreviousConfigKept()
[Test] public void SpeedOrderViolated_IsRejected()                              // vMin ≤ vMax ≤ vCap; igualdad válida
[Test] public void InvalidImport_IsRejectedAtomically_NoPartialApply()

// DefaultPresetHashTests (R2-3)
[Test] public void InheritedValues_AreUnchanged()                              // los 41 valores heredados idénticos a la baseline 0d39e6016bd8
[Test] public void SimulationKey_IsUnchanged()                                 // 166a96e319c7
[Test] public void NewHash_IsRecorded()                                        // el hash global nuevo coincide con el anotado en el informe S1
```

- [ ] **Step 2: Run** `scripts/test.sh editmode "AimModelTests|PowerEstimatorTests|ShotConfigValidatorTests|DefaultPresetHashTests"` con esqueletos compilables. Expected: FAIL por aserciones.

- [ ] **Step 3: Implement.** Potencia: `v = |x(r) − x(r−W')| / W'` con `W' = min(W, duración desde el arrastre reconocido)`; válida si `W' ≥ MinCoverageMs`, hay `≥ MinSamples` tiempos distintos y ningún hueco entre muestras consecutivas de la ventana supera `MaxGapMs`; extremos interpolados entre muestras reales; en otro caso `P = 0`, `Conservative = true`. Mira: el cruce exige `FlickConfirmSamples` intervalos consecutivos sobre `FlickMmPerS` (el primero de ellos fecha el cruce) y no se reevalúa por debajo de `FlickMmPerS − Hysteresis`. Añadir las claves al catálogo y a `DefaultPreset.json` sin tocar las existentes.

- [ ] **Step 4: Run** las mismas pruebas y la suite completa `scripts/test.sh editmode`. Expected: PASS; ninguna prueba previa cambia.

- [ ] **Step 5: Commit** `feat: add front-wall aim model and windowed net-displacement power`

### Tarea 1.2b: Prototipo S1-sling del control §6.2 B [D1, C5, PO 2026-10-27]

**No renumera1.2 ni1.3.** A′, sus pruebas/reglas y comportamiento por defecto permanecen. El método de potencia B por velocidad no es este control. No promover08b6fe90889b ni aceptar S1 por esta adenda.

**Paso1 — parada vigente, solo documentación:** registro literal de las decisiones en aprobaciones/AGENTS, adendas D1, tabla de cambios respecto a PR #24, diseño crítico/final propuesto de cancelación y protocolo B0–B5 con criterios verificables. Un único commit Markdown, PR en borrador y detenerse para revisión de Claude; no pruebas, builds, instalaciones o sesiones.

**Paso2 — solo después de aprobación de Claude de la interacción:** implementación en la arquitectura TouchSample/GesturePreview/Core Shot/ShotOverlay/DevPanel, sin duplicar el sistema táctil ni abstracciones generales. Selector gesture.control capturado al apoyar, A′ por defecto; claves K2 en unidades explícitas y preset experimental separado S1-sling. Iniciales [EXPERIMENTO]: pausa120 ms (120–150 permitidos), velocidad15 mm/s/radio1 mm, huecos≤51 ms/mínimo 3 muestras, guardia previa≥3 mm, zona muerta2 mm/tope35 mm (30–40), curva lineal saturada, destello120 ms y zona según interacción revisada del paso 1. Ajustables en DEV pero estables durante gesto y sesión. Cambio120→150 requiere decisión posterior del PO con evidencia, no ajuste en campaña.

**Files futuros (ninguno editado en paso 1):** Core/Shot para el estado específico mínimo si hace falta; integrar el selector en GesturePreview y vistas/ShotOverlay/DevPanel existentes; catálogo/DefaultPreset solo claves nuevas autorizadas y tests EditMode/PlayMode del control. No otra fuente de TouchSample, scheduler, reloj, pelota o predicción. A′ conserva N1 por regreso al origen/Flick/150 ms-P<0,60 y la ruta temporal existente.

**TDD previsto:** esqueletos compilables, RED por aserciones observadas y GREEN tras implementar; preparación se declara sin fingir RED. Casos futuros:

| Grupo | Expectativa de la decisión del PO |
|---|---|
| Quietud | Muestras reales/duración/tolerancia/huecos/guardia; silencio no fija; fijación única con destello/cambio visible. |
| Potencia | Cero antes de fijar incluso apuntando abajo; origen en fijación; proyección descendente actual, lateral sin potencia, regreso reduce, zona muerta/tope/saturación lineal. |
| Cancelación | Ended dentro cancela sin consumo; pasar y salir no; tirón válido no se cancela por volver a Began. Región superior/fallback y falta de margen según interacción revisada, sin latch N1 de A′. |
| Fases/contexto | Tap N2, abandono antes de fijar, r=Ended original, primer dedo/lanzamiento/control/configuración capturados; pausa/foco/DEV/lanzamiento invalidan sin swing al volver. |
| Terminal dt=0 | Ended idéntico no anula potencia fiable del nuevo control, sin dt artificial; contradictorios/no monótonos conservadores; **sin modificar A′**. |
| Vista/registros | Estados Apuntando/Fijada/Tensando/Zona de cancelación/Inválida con forma/texto, mínimos PR #19/frontis despejado; marcas objetivo solo DEV/protocolo, no prod. Todas las transiciones/outcomes/hash/bordes registradas. |
| RB1–RB6 | Expectativas del diseño de interacción: separación antes de fijar, distancia actual y zona; mantener tirón deliberado no hereda decaimiento temporal. **RB6 se registra/mide, no se oculta ni se fuerza a P=0 después de fijar para prometer riesgo resuelto.** |

Registro por gesto: control; Began/fijación/inicio de tensado/entrada-salida de zona/Ended con timestamp; objetivo pedido/posición de fijación; d actual/máxima/lateral/P/banda; Swing/Cancelado por zona/Abandonado antes de fijar/SystemCanceled/Tap; duración total/tiempo hasta fijar; distancia mínima al borde inferior/área segura; hash/preset/escenario/intento. Describir fijación involuntaria según señalamiento literal del PO y error continuo, sin umbral nuevo.

Compatibilidad R2-3: nuevo hash global solo por claves autorizadas; registrar el nuevo y conservar41 valores heredados y SimulationKey byte a byte. Default A′ sin cambio de comportamiento. Regresión completa **fresca** EditMode/PlayMode sin CS, check-release dev/prod, build dev Release ARM64/no Development. Perfil efectivo y renovación a cargo del PO según su decisión, respaldo/cotejo deP0-calib-1/Po-1/last-active antes de instalar. No ejecutar nada de esto en la primera parada.

**Paso3 — sesión exploratoria posterior condicionada, mano izquierda declarada:** protocolo breve B0 práctica 2 min aparte; B1 diez a cinco objetivos (dos bajos), soltar suave; B2 diez suaves/diez medios/diez fuertes al centro; B3 diez medios a objetivo bajo/RB6; B4 cinco cancelaciones voluntarias; B5 diez de ritmo cómodo con objetivos/potencias alternados. Antes de CADA bloque hash/preset/cero pendientes/repeat=0 preparados sin guardar, DEV cerrado. Conservar todo; ningún parámetro se cambia durante la sesión. Métricas descriptivas sin umbrales de aceptación; preguntas del punto 6 literal aclarado del PO y la pregunta de rally del adjunto. RestaurarP0 y comprobar en arranque nuevo hash/cero pendientes/los tres archivos byte a byte.

Fuera de esta tarea: contacto real, S2–S4, timing ±40 ms, V/T3, sonido/háptica/calidad, jugadores/IA/rallies/marcador. Defecto: registrar y detenerse. La primera entrega de esta tarea es **borrador de interacción**, no PR de código lista para fusionar. S1 sigue sin aceptar.

### Tarea 1.3: Adaptador táctil, mm físicos, vista de la mira y superposición [A05, M07]

**Files:**
- Create: `Assets/SquashClash/Runtime/Input/TouchInputAdapter.cs`, `Assets/SquashClash/Runtime/Input/ScreenMetrics.cs`, `Assets/SquashClash/Runtime/Shot/AimReticleView.cs`, `Assets/SquashClash/DevPanel/ShotOverlay.cs`
- Modify: `Assets/SquashClash/Runtime/App/AppBootstrap.cs` (crear adaptador y vista), `Assets/SquashClash/Core/Config/ParamCatalog.cs` + `DefaultPreset.json` (`input.fallbackDpi`)
- Test: `Assets/SquashClash/Tests/PlayMode/TouchInputAdapterTests.cs`, `Assets/SquashClash/Tests/PlayMode/AimReticleViewTests.cs`

**Interfaces:**
- Consumes: `GestureTracker`, `AimModel`, `PowerEstimator`.
- Produces:
  - `public sealed class TouchInputAdapter : MonoBehaviour` con `event Action<TouchSample> SampleReceived`, `TouchSamplingStats Stats` (muestras, tiempos distintos, intervalos p50/p95, máximo hueco, muestras por frame, antigüedad al procesar p50/p95/máx., duplicados, fuera de orden) y `bool BlockedByUi`.
  - `public enum DpiSource { Reported, NominalFallback, PhysicallyVerified }` y `public static class ScreenMetrics { public static float MmPerPx(out DpiSource source); }`: `Screen.dpi` si es finito y plausible (150–700, constante documentada), o `input.fallbackDpi` marcado `NominalFallback`. `PhysicallyVerified` solo tras la comprobación con regla de la Tarea 1.4, registrada; nunca se infiere de un DPI no nulo.
  - `public sealed class AimReticleView : MonoBehaviour` con estados visuales `Aiming`, `Frozen` y, desde S2, `Intention` y `Feasible`.

- [ ] **Step 1: Write the failing tests** (con `InputTestFixture` y `Touchscreen` virtual)

```csharp
[Test] public void EventTimestamps_ArePreservedAsDouble()                     // TimeReal == InputEvent.time del evento enviado, no Time.time
[Test] public void MultipleEventsInOneFrame_YieldDistinctSamplesInOrder()
[Test] public void NoEventsInFrame_DoesNotSynthesizeSamples()
[Test] public void TouchStartedOverUi_DoesNotStartGesture()                   // panel, pausa, ResumeOverlay
[Test] public void ScreenMetrics_ZeroDpi_UsesDeclaredFallbackAndFlagsIt()     // Review Focus 3
[Test] public void ScreenMetrics_ImplausiblePositiveDpi_UsesFallbackAndFlagsIt() // F07: p. ej. 30 o 5000
[Test] public void OnEvent_ReadsTouchStatePayload_NotStaleControls()             // F06: fase, id y posición del evento
[Test] public void NonMonotonicEventTimes_AreKeptInSequenceAndCounted()
[Test] public void Reticle_ShowsFrozenState_AndStaysAtFrozenPoint()
[Test] public void SafeArea_ExcludesSystemEdges()
```

- [ ] **Step 2: Run** `scripts/test.sh playmode "TouchInputAdapterTests|AimReticleViewTests"` con esqueletos compilables. Expected: FAIL por aserciones.

- [ ] **Step 3: Implement.** El adaptador lee **solo** `InputSystem.onEvent` para el `Touchscreen` (sin mezclar acciones ni EnhancedTouch). Como `onEvent` se llama **antes** de procesar el evento y no ordena por timestamp, lee el **payload** del evento (`TouchState` individual: id, fase, posición) y no el estado de los controles; copia los datos a `TouchSample`, asigna `Sequence` por orden de llegada y usa `InputEvent.time` (línea `realtimeSinceStartup`). `deferSystemGesturesMode` de bordes inferiores en tiempo de ejecución. La superposición (solo dev) muestra mira, estado, potencia `PowerResult` completo y `Stats`.

- [ ] **Step 4: Run** las pruebas y ambas suites completas. Expected: PASS.

- [ ] **Step 5: Commit** `feat: add timestamped touch adapter, physical mm metrics and aim reticle`

### Tarea 1.4: Validación S1 en el iPhone

**Files:**
- Create: `docs/reviews/2026-10-XX-first-shot-s1-gesture.md`, `docs/validation/evidence/<fecha>-s1-touch-sampling.json`

- [ ] **Step 1:** regresión completa, `check-release.sh` dev/prod, build dev. **Comprobar la vigencia del perfil; si es el 27 o después, renovarlo antes.** Instalar.
- [ ] **Step 2:** el PO apunta durante 2 minutos con mano diestra y, si quiere, zurda. Se registran `Stats` y se anota la frecuencia táctil efectiva **medida** (no se asume 120 Hz).
- [ ] **Step 3:** comprobación de mm con regla: el PO arrastra 50 mm medidos; registrar los mm calculados, `Screen.dpi`, resolución y si se usó el fallback.
- [ ] **Step 4:** el PO ejecuta 10 flicks «suaves» y 10 «fuertes»; registrar `PowerResult` de cada uno (incluidos `Conservative` y `Reason`) y el salto de la mira al congelarse.
- [ ] **Step 5:** informe con los datos y las preguntas del PO de la §5 de la propuesta. Si la frecuencia efectiva no deja `power.minSamples` en la ventana, proponer ajuste a Claude/PO; no cambiar defaults sin decirlo. PR lista para Claude, **sin fusionar**.

---

## Etapa S2 — Timing, contacto y golpe funcional

### Tarea 2.1: Mapa de relojes [A02]

**Files:**
- Create: `Assets/SquashClash/Core/Shot/ClockMap.cs`
- Test: `Assets/SquashClash/Tests/EditMode/Shot/ClockMapTests.cs`

**Interfaces:**
- Produces: `public sealed class ClockMap` con `void Begin(long launchId, double tReal, long step, double tempo, double slowMotion, int stepHz)`, `void Mark(double tReal, long step, double fraction, double tempo, double slowMotion, bool paused)`, `bool TryRealToSim(double tReal, out long step, out double fraction)`, `bool TrySimToReal(long step, double fraction, out double tReal)`, `bool InvalidSince(double tReal)` (cámara lenta, pausa o pasos descartados en el tramo).

- [ ] **Step 1: Write the failing tests**

```csharp
[Test] public void ContinuousSegment_MapsWithTempo()           // τ = 0,65, 120 Hz: 40 ms reales → 3,12 pasos
[Test] public void PausedSegment_DoesNotAdvanceSimTime()
[Test] public void SlowMotionChange_IsSegmented_AndMarksInvalid()
[Test] public void DroppedSteps_MarkInvalid_DoNotFabricateTime()
[Test] public void OtherLaunchId_IsRejected()
[Test] public void RoundTrip_RealSimReal_IsStableWithinOneMicrosecond()
```

- [ ] **Step 2–4:** RED con `scripts/test.sh editmode ClockMapTests`, implementar con segmentos en `double`, GREEN.
- [ ] **Step 5: Commit** `feat: add per-launch real-to-simulation clock map`

### Tarea 2.2: Cuerpo fijo y planificador estático [M04, §11.3]

**Files:**
- Create: `Assets/SquashClash/Core/Shot/BodyFrame.cs`, `Assets/SquashClash/Core/Shot/ContactPlanner.cs`
- Modify: catálogo + `DefaultPreset.json` (`body.*`)
- Test: `Assets/SquashClash/Tests/EditMode/Shot/ContactPlannerTests.cs`

**Interfaces:**
- Consumes: `Trajectory` (existente, con estados y contactos), `CourtDimensions`.
- Produces:
  - `public enum StrokeSide { Drive, Backhand }`
  - `public readonly struct BodyFrame { Vector2 PositionXZ; bool RightHanded; float Reach, IdealLateral, PreferredMinHeight, PreferredMaxHeight, WallClearance; StrokeSide SideFor(Vector3 ball); Vector3 IdealContactPoint(StrokeSide side); }`
  - `public readonly struct SimTime { public readonly long Step; public readonly double Fraction; }` (instante exacto: paso + fracción de `TimeInStep`)
  - `public readonly struct ContactPlan { public readonly bool Exists; public readonly SimTime KIn, KOut; public readonly long TStarStep; public readonly Vector3 PStar; public readonly StrokeSide Side; public readonly int Version; }`
  - `public static class ContactPlanner { public static ContactPlan Plan(Trajectory t, BodyFrame body, CourtDimensions court, LegalSettings legal, int version); public static bool IsLegalAt(Trajectory t, SimTime when, BodyFrame body, LegalSettings legal); public static bool IsLegalStep(Trajectory t, long step, BodyFrame body, LegalSettings legal); }`
  - `public readonly struct LegalSettings { MinHeight, MaxHeight }`

- [ ] **Step 1: Write the failing tests**

```csharp
[Test] public void NoK_BeforeFirstBounce_EvenIfReachable()                      // F01: sin volea
[Test] public void Legality_RequiresFrontWallThenExactlyOneBounce()
[Test] public void K_BoundariesUseExactEventTimes_NotWholeSteps()                // F02: TimeInStep del primer y segundo bote
[Test] public void LegalHeights_AreSeparateFromPreferredBand()
[Test] public void K_IsContiguousLegalStepsWithinReach_ResolvedPerStep()       // no por la rejilla de 20 ms
[Test] public void K_EndsAtExactSecondBounceTime()                               // antes del impacto, legal; en y después, ilegal
[Test] public void TStar_MaximizesPreferredHeightAndWallClearance()
[Test] public void NoReachableOpportunity_PlanDoesNotExist()                    // caso 7
[Test] public void SideSelection_NearAxis_IsDeterministic()
[Test] public void SixPracticeLaunches_AreReachableFromTheirBodyPositions()     // SL1–SL6 (Tarea 2.6) con su BodyFrame
[Test] public void LegibilityLaunchesL1toL6_AreUnchanged()                       // no se reajustan
```

- [ ] **Step 2–4:** RED, implementar (barrido O(N) sobre los estados de la predicción tras el primer frontis; puntuación `U = w_h·altura_preferida + w_w·separación` con desempate por paso menor), GREEN.
- [ ] **Step 5: Commit** `feat: add fixed-body frame and static contact planner`

### Tarea 2.3: Contrato de contacto [A04, M05, C1]

**Files:**
- Create: `Assets/SquashClash/Core/Shot/ContactContract.cs`
- Modify: catálogo + `DefaultPreset.json` (`timing.*` salvo `aidLevel`)
- Test: `Assets/SquashClash/Tests/EditMode/Shot/ContactContractTests.cs`

**Interfaces:**
- Consumes: `ContactPlan`, `ClockMap`.
- Produces:
  - `public enum ShotResult { Perfect, Valid, Failed, NoOrder, Unreachable, SystemFault, AttemptConsumed, NotEvaluable }`
  - `public enum FailKind { None, Air, Late }`
  - `public readonly struct TimingSettings { AssistMs, ValidLimitOn, ValidMs, EarlySoft, LatencyCompMs }`
  - `public readonly struct ContactDecision { ShotResult Result; FailKind Fail; double DeltaMs; long ContactStep; bool Scheduled; bool EarlyGeometry; int PlanVersionUsed; double PlanPresentedAtReal; double TargetContactAtReal; double ReleaseEventAtReal; double ReleaseCorrectedAtReal; double QuantizationErrorMsSigned; bool UsedNeighborLegalStep; bool QuantizationAboveThirdOfA; }`
  - `public static class ContactContract { public static ContactDecision Decide(double releaseTimeReal, PresentedPlans plans, ClockMap clock, Trajectory authoritative, TimingSettings s, bool attemptConsumed); }`
  - `public sealed class PresentedPlans { void Present(double planPresentedAtReal, ContactPlan plan, double targetContactAtReal); bool TryVisibleAt(double releaseEventAtReal, out ContactPlan plan, out double targetContactAtReal); }`

**Orden de decisión (F02, F03, R2-4):** (1) plan visible en `ReleaseEventAtReal` (timestamp original, no el corregido); (2) `rCorregido = rEvento − compensación`; (3) `Δ = rCorregido − TargetContactAtReal`, en ms reales; (4) legalidad e inclusión en K en el instante **exacto** `rCorregido` mapeado a `SimTime`; si es ilegal, no hay contacto; (5) Perfecto con `rCorregido ≤ T*`: se programa en `TStarStep` del plan visible (sin cuantizar r); (6) en los demás contactos, paso más cercano a `rCorregido` (empate: el anterior); si ese paso es ilegal, paso legal más cercano del mismo lado de la frontera (R2-4); (7) se registran el error firmado y las banderas.

- [ ] **Step 1: Write the failing tests** (cada caso de §7.1.3 aplicable y los bordes de §11.3)

```csharp
[Test] public void Case1_WithinA_BeforeTStar_IsPerfectScheduledAtTStar()
[Test] public void Case1_WithinA_AfterTStar_IsPerfectAtR()
[Test] public void DeltaExactlyA_IsPerfect()
[Test] public void Case2_EarlyBeyondA_InK_IsValidAtR()
[Test] public void Case3_LateBeyondA_InK_IsValidAtR()
[Test] public void DeltaExactlyV_IsValid_BeyondV_IsFailedOnlyInVMode()
[Test] public void PureT3_BeyondV_InK_IsValid()                                   // condición 2 del PO
[Test] public void Case4_BeforeKIn_IsAir_EvenWithinA()
[Test] public void T3Soft_BeforeKIn_ContactsAtKInWithEarlyGeometry()               // solo caso 4 cambia
[Test] public void Case5_AfterKOut_IsLate()
[Test] public void Case6_NoRelease_IsNoOrder()
[Test] public void Case7_NoPlan_IsUnreachable()
[Test] public void Case8_PlanChangedBeforeR_UsesPlanVisibleAtR()
[Test] public void Delta_UsesTargetContactTime_NotPresentationTime()                // F03: plan mostrado a 10,0 s con T* a 10,5 s, soltar a 10,5 s → Δ = 0
[Test] public void PlanChangeBetweenCorrectedAndEventTime_UsesPlanAtEventTime()
[Test] public void Case9_AuthoritativeLeavesK_IsSystemFault()
[Test] public void Case11_AttemptAlreadyConsumed_NoSwing()
[Test] public void Case13_AtSecondBounce_NoContact()
[Test] public void DeltaIsInRealMs_NotSimulationMs()                               // τ = 0,65
[Test] public void LatencyCompensation_IsAppliedOnce_BeforeMapping()
[Test] public void ContactStep_IsNearestStepToR_AndSignedErrorIsReported()         // R2-1: |error| ≤ 6,41 ms reales a 120 Hz, τ 0,65
[Test] public void ExactTieBetweenSteps_UsesEarlierStep()
[Test] public void LegalRNearSecondBounce_NearestStepIllegal_UsesLegalNeighbor()    // R2-4: |error| ≤ 1 paso, UsedNeighborLegalStep
[Test] public void LegalRNearKOut_NearestStepOutOfReach_UsesLegalNeighbor()
[Test] public void IllegalRAfterSecondBounce_NoContactEvenIfNeighborLegal()
[Test] public void PerfectEarly_SchedulesAtTStarOfVisiblePlan_NotNearestToR()
[Test] public void T3Soft_KInContact_RespectsSameBoundaryRules()
[Test] public void HalfStepAboveThirdOfA_IsFlagged()                               // 60 Hz, τ 0,3
[Test] public void ReleaseOlderThanClockMap_IsNotEvaluable()
```

- [ ] **Step 2–4:** RED, implementar, GREEN.
- [ ] **Step 5: Commit** `feat: add T3 contact contract with experimental valid window`

### Tarea 2.4: Solver de velocidad total [A07, §11.5]

**Files:**
- Create: `Assets/SquashClash/Core/Shot/ShotSolver.cs`
- Modify: catálogo + `DefaultPreset.json` (`shot.*`)
- Test: `Assets/SquashClash/Tests/EditMode/Shot/ShotSolverTests.cs`

**Interfaces:**
- Produces:
  - `public readonly struct ShotRequest { Vector3 ContactCenter; Vector2 AimOnFrontWall; float P; }`
  - `public readonly struct ShotSolution { bool Solved; Vector3 Velocity; float Speed; Vector2 TargetUsed; bool SpeedRaised; bool TargetLowered; string Reason; }`
  - `public static class ShotSolver { public static ShotSolution Solve(in ShotRequest r, float vMin, float vMax, float vCap, float gravity, float radius, CourtDimensions court, IBallSimulationFactory verifier, PhysicsParams physics); }`

- [ ] **Step 1: Write the failing tests**

```csharp
[Test] public void Speed_IsTotalSpeed_vMinPlusPTimesRange()                        // |Velocity| == vMin + p(vMax − vMin)
[Test] public void LowArc_IsChosen()
[Test] public void ReachesAimCenter_WithPhysicalRadius20mm_NotVisualScale()
[Test] public void Infeasible_RaisesMinimumSpeedUpToCap()                          // registra SpeedRaised
[Test] public void InfeasibleAtCap_LowersTargetKeepingX()                          // TargetLowered
[Test] public void NearDoubleRoot_IsStable()
[Test] public void VerticalOrZeroHorizontalDistance_IsHandled()
[Test] public void VerifiedWithP2_NoFloorOrSideBeforeFrontWall()                    // si toca antes, se marca y se ajusta el objetivo
[Test] public void SolveCenterTo_AndL1toL6_AreUnchanged()
[Test] public void NoSolutionEvenLoweringY_ReturnsUnsolvedWithReason()             // el controlador lo registra como SystemFault, swing sin contacto, nunca como fallo del PO
[Test] public void SameRequest_SameSolutionBits()                                  // replay determinista, iteraciones acotadas
```

- [ ] **Step 2–4:** RED, implementar con `v⁴ − g(g·d² + 2h·v²) ≥ 0`, `v²_min = g(h + √(d² + h²))` y la raíz de arco bajo en forma estable; iteraciones acotadas (máximo 16) para bajar y. GREEN.
- [ ] **Step 5: Commit** `feat: add total-speed low-arc shot solver`

### Tarea 2.5: Golpe retroactivo en el host [A01, A03, M01, §11.2]

**Files:**
- Modify: `Assets/SquashClash/Runtime/Ball/BallSimulationHost.cs`
- Modify: `Assets/SquashClash/Runtime/Ball/LegibilityAids.cs` (retirar marcas/estela invalidadas)
- Test: `Assets/SquashClash/Tests/PlayMode/ShotRollbackTests.cs`

**Interfaces:**
- Consumes: `ClockMap` (2.1).
- Produces (en `BallSimulationHost`):
  - `public long LaunchId { get; }` — incrementa en **cada** `Launch`/`Replay`.
  - `public ClockMap ClockMap { get; }`
  - `public bool TryGetHistoricalState(long step, out BallState s)`
  - `public ShotApplyResult ApplyShot(long launchId, long contactStep, Vector3 outgoingVelocity)` — operación atómica.
  - `public bool ScheduleShot(long launchId, long contactStep, Func<BallState, Vector3> velocityAtContact)` — contacto futuro.
  - `public event Action<Trajectory> PredictionReplaced`
  - `public event Action<ShotApplyResult> ShotApplied`
  - `public readonly struct ShotApplyResult { bool Applied; string Reason; long ContactStep; int StepsResimulated; float JumpMeters; float JumpPixels; float JumpBallDiameters; double OrderAgeMs; int ObsoleteEffects; }`

- [ ] **Step 1: Write the failing tests**

```csharp
[Test] public void ApplyShot_PunctualVsDelayed_SameBitsAndSameCanonicalEvents()   // misma orden entregada a tiempo y 2 frames tarde → mismos bits
[Test] public void ApplyShot_PredictionMatchesGameBitwiseAfterShot()
[Test] public void ApplyShot_DoesNotAdvanceClockAgain()
[Test] public void ApplyShot_ResetsOutgoingBounces_KeepsReceptionLegality()
[Test] public void ApplyShot_FromPreviousLaunchId_IsRejected()                    // Review Focus 4
[Test] public void ApplyShot_OlderThanHistory_IsNotEvaluable()
[Test] public void ApplyShot_LateDeliveryAfterSecondBounce_ResolvesByEventTime()  // Review Focus 1
[Test] public void ApplyShot_DoesNotReemitCommonPrefixEvents_AndCountsObsolete()
[Test] public void ApplyShot_InvalidatesStaleTrailAndMarks()
[Test] public void ApplyShot_MeasuresJumpAtSameVisualInstantAndCamera()          // sin suavizado: RenderPosition salta
[Test] public void ScheduleShot_AppliesAtTStarStep_AndFiresFeedbackAtContact()
[Test] public void PauseStep_AndSlowMotion_DoNotBreakRollback()
[Test] public void ApplyShot_AfterFlightStopped_ResimulatesToRealPresentViaClockMap() // F02: el presente es el paso que corresponde al reloj real, no el último Current.Step
[Test] public void ApplyShot_RecordsPerPartCosts()                                  // F09: reconstrucción, predicción y reconciliación (p50/p95/p99/máx. en el registro)
[Test] public void NoShot_FirstBallBehaviourUnchanged()                           // suites First Ball intactas
```

- [ ] **Step 2: Run** `scripts/test.sh playmode ShotRollbackTests`. Expected: FAIL.
- [ ] **Step 3: Implement.** Anillo preasignado de `BallState` por paso (capacidad = `rollback.historyMs` convertido con el reloj vigente) más contador de botes de la recepción. `ApplyShot`: validar `launchId`, `predictor.Reset(estado histórico con la velocidad nueva)` → `TrajectoryRunner.Run` como nueva predicción; `game.Reset` con el mismo estado y avanzar los mismos pasos que el presente, sin `Clock.Advance` y sin callbacks públicos; reconciliar eventos por `(LaunchId, Step, Surface, ordinal)`; publicar `PredictionReplaced` y `ShotApplied`. El salto se mide antes del cambio con la misma `Alpha` y la cámara activa. P2 sin cambios.
- [ ] **Step 4: Run** las pruebas y **ambas suites completas**. Expected: PASS; ninguna prueba de First Ball modificada.
- [ ] **Step 5: Commit** `feat: add atomic retroactive shot application to the ball host`

### Tarea 2.6: Controlador del golpe, modo práctica, vistas y sonido provisional

**Files:**
- Create: `Assets/SquashClash/Runtime/Shot/ShotController.cs`, `PracticeMode.cs`, `TimingRingView.cs`, `ShotNoteView.cs`, `Assets/SquashClash/Runtime/Feedback/ShotFeedback.cs`, `Assets/SquashClash/Core/Shot/ShotRecord.cs`, `Assets/SquashClash/Runtime/Resources/Audio/PROVENANCE.md`, `Assets/SquashClash/DevPanel/ShotSection.cs`
- Modify: `AppBootstrap.cs`, `AimReticleView.cs` (marcas de intención y factible), catálogo + `DefaultPreset.json` (`practice.*`, `rollback.historyMs`, `timing.showNotes`)
- Test: `Assets/SquashClash/Tests/PlayMode/ShotControllerTests.cs`, `Assets/SquashClash/Tests/EditMode/Shot/ShotRecordTests.cs`

**Interfaces:**
- Consumes: todo lo anterior.
- Produces:
  - `public sealed class ShotConfig` inmutable, capturado al lanzar (cuerpo, timing, shot) y al apoyar (aim, power, gesture).
  - `public sealed class ShotController : MonoBehaviour` con `event Action<ShotRecord> ShotResolved`.
  - `PracticeMode` con los seis lanzamientos `SL1`–`SL6` de §6.3 (media altura de drive, media altura de revés, esquina trasera de drive, esquina trasera de revés, adelante de drive, adelante de revés), cada uno con su `BodyFrame` alcanzable.
  - **Ciclo de práctica (F04):** `Reception → Resolved → Observing → Waiting → Reception`. `Observing` termina con el **segundo bote de la salida**, la salida de la cancha o `practice.observeMaxSeconds` de simulación (lo primero); sin golpe (sin orden, inalcanzable, al aire, tarde), termina con el fin del vuelo de recepción. `Waiting` dura `practice.repeatSeconds` reales y no corre en pausa ni sin foco. **Único dueño de la repetición:** con `practice.enabled = 1`, el temporizador `launcher.repeatSeconds` de `AppBootstrap.TickRepeat` queda **suspendido sin modificar ni persistir su valor** (P0-calib-1 conserva 3 s); con `practice.enabled = 0` vuelve su comportamiento First Ball.

- [ ] **Step 1: Write the failing tests**

```csharp
[Test] public void FirstPlayableShot_BallReachesAimWithinSolverTolerance()        // golpe perfecto simulado: impacto en frontis a ≤ 5 cm del objetivo usado
[Test] public void FeasibleAdjustment_ShowsIntentionAndFeasibleMarks()            // C4
[Test] public void PanelChange_MidReception_DoesNotAffectCurrentShot()            // Review Focus 5
[Test] public void FocusLoss_DuringGesture_NoSwingOnResume()                      // Review Focus 2
[Test] public void PauseTempoOrSlowMotionChange_DuringGesture_CancelsGesture()
[Test] public void OneAttemptPerReception_SecondReleaseIgnored()
[Test] public void VModeAndPureT3_AreSwitchableAndRecordedPerShot()               // condición 2
[Test] public void T3AndT3Soft_AreSeparateSwitch()
[Test] public void P1Backend_PracticeShowsNotAvailable_AndNeverShoots()
[Test] public void PracticeCycle_DoesNotRelaunchBeforeOutgoingObservationEnds()  // vuelo lento: espera al segundo bote de la salida
[Test] public void PracticeCycle_NoOrderOrUnreachable_RelaunchesAfterReceptionEnds()
[Test] public void PracticeCycle_PausedOrUnfocused_DoesNotAdvanceWaiting()
[Test] public void PracticeEnabled_SuspendsLauncherRepeat_WithoutChangingIt()      // P0-calib-1 con repeat = 3 intacto byte a byte
[Test] public void PracticeDisabled_RestoresFirstBallRepeat()
[Test] public void SolverUnsolved_IsSystemFaultSwingWithoutContact()
[Test] public void ProdConfig_HasPracticeAndFeedback_WithoutDevPanelTypes()      // verificación estática; el recorrido funcional de prod va en 2.7 (F09)
[Test] public void ShotRecord_ContainsFieldsOfSection11()                         // ShotRecordTests: LaunchId, plan visible, timestamps, Δ, categoría, potencia, solver, salto, efectos obsoletos, preset/hash/build/semilla
```

- [ ] **Step 2–4:** RED, implementar, GREEN en ambas suites. Si alguna prueba First Ball depende del lanzamiento al arrancar, fijar `practice.enabled = 0` en su preparación, **conservando su aserción y el escenario que verificaba**, y declararlo en la PR (excepción de Global Constraints). Sonido provisional: un único clip de contacto y un «swish» con procedencia en `PROVENANCE.md`. Notas: Perfecto, Adelantado, Atrasado, ¡Al aire!, ¡Tarde!, No llega.
- [ ] **Step 5: Commit** `feat: add practice mode with first playable shot`

### Tarea 2.7: Validación S2 en el iPhone

- [ ] **Step 1:** regresión completa, `check-release.sh` (verificación **estática**), perfil vigente (renovado si es el 27 o después), build dev instalada.
- [ ] **Step 1b: recorrido funcional de prod** (F09): instalar prod y que el PO confirme lanzamiento, golpe, nota y sonido sin panel; se registra como recorrido observado, no se infiere de símbolos.
- [ ] **Step 2:** el PO juega SL1–SL6 en bloques con V=100 y con T3 puro, sin cámara lenta. Se registran todos los golpes.
- [ ] **Step 3:** informe: distribución del salto (p50/p95/máximo en m, px y diámetros), efectos obsoletos, error de medio paso, edad de las órdenes, frame del golpe (costo por parte, A09, con la instrumentación mínima de 2.5) y juicio del PO: ¿la pelota va donde apunto?, ¿el timing es justo? (preliminar: sin calibrar latencia), ¿el salto es aceptable?
- [ ] **Step 4:** PR lista para Claude, sin fusionar. El PO decide si el salto es aceptable antes de fusionar (condición 1).

---

## Etapa S3 — Sonido, háptica y calidad del impacto

### Tarea 3.1: Calidad causal no direccional [C2, §7.6, §17.A.12]

**Files:**
- Create: `Assets/SquashClash/Core/Shot/CausalQuality.cs`
- Modify: `ShotController.cs` (aplicar antes del solver), catálogo + `DefaultPreset.json`: `causal.timingHeightPerM` (0,6), `causal.timingSpeedPerM` (−0,25 como fracción de v), `causal.stretchStart` (0,85 de R), `causal.jamDistance` (0,35 m), `causal.stretchHeight` (0,4 m), `causal.stretchSpeed` (−0,2), `causal.jamHeight` (0,3 m), `causal.jamSpeed` (−0,15), `causal.capHeight` (1,0 m), `causal.capSpeed` (−0,4) [EXPERIMENTO]
- Test: `Assets/SquashClash/Tests/EditMode/Shot/CausalQualityTests.cs`

**Interfaces:**
- Produces: `public readonly struct CausalVector { float DeltaHeight; float DeltaLateral; float DeltaSpeedFraction; string Cause; }` y `public static class CausalQuality { public static IReadOnlyList<CausalVector> Evaluate(BodyFrame body, Vector3 contactPoint, StrokeSide side, bool perfect, CausalSettings s); public static (Vector2 aim, float speed) Apply(Vector2 aim, float speed, IReadOnlyList<CausalVector> v, CausalSettings s); }`

- [ ] **Step 1: Write the failing tests**

```csharp
[Test] public void Perfect_HasNoTimingVector_ButKeepsStretchAndJam()
[Test] public void Valid_TimingVector_GrowsWithGeometricDistance()
[Test] public void NoVector_HasLateralComponent()                                  // C2: DeltaLateral == 0 siempre
[Test] public void SameGeometryDifferentDelta_SameShot()                           // §17.A.12 literal; si aparece incompatibilidad, se eleva, no se acota
[Test] public void Caps_ArePerComponent()
[Test] public void AppliedBeforeSolver_NeverCorrectedAfter()
[Test] public void MirroredSide_IsSymmetric()
```

- [ ] **Step 2–4:** RED, implementar, GREEN.
- [ ] **Step 5: Commit** `feat: add non-directional causal shot quality`

### Tarea 3.2: Háptica nativa, audio y destello [M02, M03, B01]

**Files:**
- Create: `Assets/Plugins/iOS/SquashHaptics.mm`, `Assets/SquashClash/Runtime/Feedback/Haptics.cs`
- Modify: `ShotFeedback.cs`, `Runtime/Resources/Audio/` + `PROVENANCE.md`, catálogo + `DefaultPreset.json` (`feedback.haptics`), `scripts/check-release.sh` (símbolo háptico presente en prod)
- Test: `Assets/SquashClash/Tests/PlayMode/ShotFeedbackTests.cs`

**Interfaces:**
- Produces: `public static class Haptics { public static void Prepare(); public static void Impact(float intensity01); public static bool Enabled { get; set; } }` (no-op fuera de iOS); `ShotFeedback.Play(ShotResult, FailKind, SurfaceId?)`.

- [ ] **Step 1: Write the failing tests**

```csharp
[Test] public void Perfect_StrongSoundFirmHapticAndFlash()
[Test] public void Valid_SofterSoundDiscreteHaptic()
[Test] public void Failed_SwishNoHaptic()
[Test] public void Walls_AndTin_HaveOwnSounds()
[Test] public void HapticsToggle_RuntimeOff_KeepsVisualAlternative()
[Test] public void ClipsArePreloaded_NoLoadOnContact()
[Test] public void EveryClip_HasProvenanceEntry()
[Test] public void NoCameraShakeOrSlowMotionOnFeedback()
```

- [ ] **Step 2–4:** RED, implementar (`UIImpactFeedbackGenerator` en el hilo principal, `prepare()` al armarse el gesto, reutilizar generadores), GREEN. `check-release.sh` verifica el símbolo `SquashHaptics` en prod.
- [ ] **Step 5: Commit** `feat: add tiered sound, native haptics and contact flash`

### Tarea 3.3: Swing visible, superposición causal y mediciones de latencia y rendimiento [A09, M03]

**Files:**
- Create: `Assets/SquashClash/Runtime/Shot/SwingView.cs`
- Modify: `PlaceholderPlayers.cs` (brazo y raqueta), `ShotOverlay.cs` (vectores), `ShotRecord.cs` (latencias y costos)
- Test: `Assets/SquashClash/Tests/PlayMode/SwingViewTests.cs`, `Assets/SquashClash/Tests/PlayMode/ShotTimingInstrumentationTests.cs`

- [ ] **Step 1: Write the failing tests**

```csharp
[Test] public void Swing_DriveBackhandAndAir_ArePlayedProcedurally()
[Test] public void Swing_ShowsStretchedJammedEarlyPoses()                          // apoyo a H4
[Test] public void Latencies_EventToContactToFeedbackCall_AreRecorded()
[Test] public void PerPartCosts_ExtendS2InstrumentationToEstimatorPlannerSolverAndFeedback()
[Test] public void NoAllocationsPerFrameInSteadyState()
```

- [ ] **Step 2–4:** RED, implementar, GREEN.
- [ ] **Step 5: Commit** `feat: add procedural swing and shot latency instrumentation`

### Tarea 3.4: Validación S3 en el iPhone

- [ ] **Step 1:** regresión, `check-release.sh`, perfil vigente, builds dev y prod; prod verificada con háptica y sonido.
- [ ] **Step 2:** comparar audio Default frente a Best Latency con **dos sesiones de 15 minutos**, una por configuración, (panel cerrado, sin cámara lenta), informando las métricas H17 de §17.C **sin aplicar la desviación de First Ball**.
- [ ] **Step 3:** el PO juega y responde: ¿un perfecto se siente claramente mejor?, ¿entiendo por qué un golpe salió peor sin el panel? (H4). Probar silencio, háptica del sistema apagada e interruptor de la app.
- [ ] **Step 4:** informe y PR lista para Claude, sin fusionar.

---

## Etapa S4 — Calibración y validación

### Tarea 4.1: Calibración de latencia [§7.1.5, A02]

**Files:**
- Create: `Assets/SquashClash/DevPanel/LatencyCalibration.cs`
- Test: `Assets/SquashClash/Tests/PlayMode/LatencyCalibrationTests.cs`

- [ ] **Step 1:** pruebas: `Calibration_ComputesJointOffsetWithUncertaintyAndRepeatability`, `Calibration_ResultIsWrittenToLatencyCompOnlyOnConfirm`, `Calibration_InvalidWithSlowMotionOrFocusLoss`, `Calibration_RecordStatesJointOffsetNotDeviceLatency`. **Método (R2-5):** offset perceptivo conjunto, tocando al ritmo de un estímulo regular (destello y clic) en varias series; se informan media, dispersión e intervalo entre series. La evidencia declara que el resultado mezcla presentación, sonido, captura y respuesta humana y **no** se presenta como latencia del iPhone. Las pruebas con retardos sintéticos validan el cálculo, no el protocolo físico.
- [ ] **Step 2–4:** RED, implementar, GREEN.
- [ ] **Step 5: Commit** `feat: add touch latency calibration routine`

### Tarea 4.2: Tareas en pantalla y niveles de ayuda [§6.3, §7.1.5]

**Files:**
- Create: `Assets/SquashClash/Runtime/Shot/PracticeTask.cs`, `Assets/SquashClash/DevPanel/TaskSection.cs`
- Modify: `TimingRingView.cs` (parcial: desaparece en los últimos 150 ms; sin anillo), catálogo + `DefaultPreset.json` (`timing.aidLevel`)

- [ ] **Step 0:** la infraestructura de tareas se implementa con valores provisionales. Claude propone al PO las zonas del frontis, las bandas de potencia suave/media/fuerte y las zonas del primer bote, y **el PO las aprueba antes de medir H2** (decisión 11 de la propuesta). No bloquea S1–S3.
- [ ] **Step 1:** pruebas: `Task_RecordsRequestedVsFinalAimInCm`, `Task_RecordsInvoluntaryPower`, `Task_RecordsFirstBounceZoneHit`, `AidPartial_HidesRingLast150ms`, `AidNone_ShowsOnlyBallAndShadow`.
- [ ] **Step 2–4:** RED, implementar, GREEN.
- [ ] **Step 5: Commit** `feat: add practice tasks and timing aid levels`

### Tarea 4.3: Bots de H3b y protocolo de comparación [M05, §17.B]

**Files:**
- Create: `Assets/SquashClash/Tests/EditMode/Shot/AnticipationBotTests.cs` (regresión) y `Assets/SquashClash/Core/Shot/AnticipationBot.cs`

- [ ] **Step 1:** pruebas **del bot y del protocolo** (regresión): `Bot_ReleasesAtFixedOffset_FromVisibleTStar`, `Bot_IsDeterministicPerSeed`, `Report_SeparatesContactAndNoContactPopulations` (el vector causal solo se promedia sobre golpes con contacto; los fallos se cuentan aparte, nunca como vector cero), `Report_SeparatesVEffectFromT3Geometry`.
- [ ] **Step 2:** experimento H3b (datos, no regresión, como en First Ball): con T3 puro y con V, bots a −60, −100 y −150 ms frente a uno preciso, para cada nivel de ayuda; se informan **por separado** vector causal promedio, tasa de al aire/tarde y acierto de zona. La hipótesis se juzga con esos datos; no se exige desigualdad estricta en cada dimensión ni se ajusta la física para que pase.
- [ ] **Step 3–4:** RED de las pruebas del bot, implementar, GREEN; ejecutar el experimento y adjuntar el informe.
- [ ] **Step 5: Commit** `test: add anticipation bots for H3b in T3 and V modes`

### Tarea 4.4: Pruebas deportivas del anexo C aplicables [§17.F, D25, F09]

**Files:**
- Create: `Assets/SquashClash/Tests/EditMode/Shot/SportScenarioTests.cs` (experimento, no regresión)

- [ ] **Step 0:** Claude presenta al PO las filas del anexo C (contenido **PENDIENTE PO N6**) que aplican a First Shot, con sus condiciones de validez: X1 drive paralelo pegado, X2 cruzado ancho, X3 drop que muere adelante, X4 drop defensivo y X5 golpe seguro con buena posición, y las relaciones R aplicables (R1: más potencia, primer bote más profundo). Se registran como **no aplicables** las que exigen saque, volea, IA o capa aleatoria (R6). El PO valida las filas antes de ejecutarlas.
- [ ] **Step 1:** implementar cada escenario dentro de sus condiciones de validez con el solver y la física reales, sin umbrales nuevos.
- [ ] **Step 2:** ejecutar y adjuntar el informe. **Si una prueba falla, decide el PO** (código, calibración o relación) y se registra (§17.F). Los tests de solver y contacto no sustituyen esta capacidad expresiva.
- [ ] **Step 3: Commit** `test: add First Shot sport scenarios from annex C`

### Tarea 4.5: Sesiones de validación y registro para el PO

**Files:**
- Create: `docs/validation/first-shot-validation.md` (plantilla), `docs/reviews/<fecha>-first-shot-s4-validation.md`

- [ ] **Step 1:** calibrar la latencia (4.1) **antes** de cualquier medición de H3.
- [ ] **Step 2:** protocolo propuesto por Claude y aprobado por el PO: **al menos dos sesiones en días distintos** para H3 entre sesiones (§17.B), además de los primeros 10–15 intentos de cada sesión analizados aparte (C6, ampliación, no reemplazo); bloques alternos V/T3 puro con los mismos seis lanzamientos, tareas y nivel de ayuda; T3 frente a T3-suave aparte; calentamiento separado.
- [ ] **Step 3:** plantilla con las preguntas del PO de la §6 de la propuesta, H2, H3, H3b, H3c y H4, invariantes §17.A aplicables (1, 2, 3, 5, 10, 12, 14) y la decisión del PO: continuar a First Rally, ajustar (incluido añadir B o C) o volver.
- [ ] **Step 4:** PR lista para Claude, sin fusionar. Los resultados no se convierten en umbrales definitivos sin decisión del PO.

---

## Cobertura de la propuesta (autorrevisión)

| Requisito | Tarea |
|---|---|
| Gesto A′, N1/N2 estrictos y prioridad (C5, R2-2, A08, F10) | 1.1 |
| D1/C5: prototipo control §6.2 B/S1-sling, zona explícita y protocolo exploratorio; A′ conservado | 1.2b, con parada documental y aprobación de interacción previas; no ejecuta H2/S4 |
| Mira, congelamiento visible, dos marcas (C4, §11.4) | 1.2, 1.3, 2.6 |
| Potencia filtrada y flick corto (A06) | 1.2, 1.4 |
| Timestamps, muestreo y mm medidos (A05, M07) | 1.3, 1.4 |
| Reloj real↔simulación (A02) | 2.1 |
| Cuerpo fijo, K, P\*, T\*, legalidad (M04) | 2.2 |
| Tres resultados, V/T3 puro, T3-suave, casos §7.1.3 (C1, A04, M05) | 2.3, 2.6 |
| Solver de velocidad total (A07) | 2.4 |
| Golpe retroactivo, efectos, salto (R2-1, A01, A03, M01) | 2.5, 2.7 |
| Defaults y hash (R2-3, M06) | 1.2 y cada tarea que añade claves |
| Capa causal no direccional (C2) | 3.1 |
| Sonido, háptica, destello (§4.6, M02, M03, B01) | 2.6, 3.2 |
| Swing visible (decisión 0) | 3.3 |
| Rendimiento y latencias (A09) | 2.7, 3.3, 3.4 |
| Calibración, ayudas, tareas, H3b, validación (§6, C6, R2-5) | 4.1–4.3, 4.5 |
| Pruebas deportivas del anexo C (§17.F) | 4.4 |
| Legalidad exacta y bordes cuantizados (F01, F02, R2-4) | 2.2, 2.3 |
| Ciclo de práctica y único repetidor (F04) | 2.6 |
| Invariantes de configuración (F08) | 1.2 y Global Constraints |
| Montaje de pruebas del Input System (F06) | 1.0 |
| Prod jugable sin panel (§17.A.10, M06) | 2.6, 2.7, 3.2, 3.4 |


## Nueva adenda D3 — diseño M2-4 del PO, 2026-10-28 (solo documentación)

**Aditiva: se conserva byte a byte todo el texto previo. No renumera ni ejecuta tareas existentes.** El [diseño M2-4](../../reviews/2026-10-28-first-shot-s1-m2-4-design.md) y la [nueva adenda D3 de propuesta](../specs/2026-10-24-first-shot-design.md) documentan la desviación §6.4 aceptada por el PO solo para M2-4; spec v0.3 intacta. A′ por defecto; S1-sling/D2 conservado. Dominio A referencia principal/B respaldo; diseño a revisión, ubicación final después de prueba física autorizada.

### Tarea 1.2c: diseño previsto de «apuntar al apoyar» [D3, implementación no autorizada]

**Estado de esta entrega:** documento de diseño, cinco decisiones del PO y cierre de PR #26, adendas para revisión. Sin código/tests/defaults/presets ni suites/check-release/build/instalación/sesión/acceso al iPhone. Sin replay/cálculos sobre111trazas. Que1.2c figure aquí no ejecuta ni autoriza su TDD.

Contratos candidatos: Began primario captura proyección/objetivo absoluto/ancla; distancia actual descendente produce P; Ended del mismo contacto es r. C-A sin Swing en zona muerta, N2 vigente y prioridad de terminales explícita; tin/out rojo inválido sin recorte; ayuda (iii), sin halo/lupa nuevo. Riesgo150ms por fases y resbalón accidental expuesto, sin detector de voluntad. Mano activa/contexto/snapshot fijos, UI/P2/H17 protegidos durante preparación/protocolo; sin otra autoridad/input/reloj.

**Subtareas y RED/GREEN futuros, solo tras autorización separada:**

| Paso | Contrato verificable previsto; sin escribir ni ejecutar tests ahora |
|---|---|
| 1.2c-0 Preparación declarada | Fixtures compilables/snapshots/configuración; preparación no es RED. Selector/hash/preset experimental solo si se autorizan, sin valor numérico o JSON creado ahora. |
| 1.2c-1 Mapeo | Began absoluto con proyección capturada/AX-1; Moved no cambia objetivo; tin/out rojo inválido; rechazo fuera de dominio; sin mover cámara/zoom. |
| 1.2c-2 Potencia/cancelación | N01–N06 del diseño: apoyo/pausa150ms/noSwing, fronteras N2, C-A retorno, salir de nuevo antes de soltar, distancia mantenida con P≥0,60 permitida; no confundir resbalón con voluntad. |
| 1.2c-3 Tiempo/contexto | N07/N10/N11/N12: Ended idéntico cierra una vez, datos contradictorios conservadores, contexto invalida, sin dedo promovido ni Ended ficticio. |
| 1.2c-4 Evidencia | N09/N13: O2 tiempos/desplazamiento/matriz realmente usada, cadena de reapoyos sin sustitución y error continuo del primer Began conservado. |
| 1.2c-5 Vista/protocolo | Mira actual/rojo/motivos/frontis despejado; tercera respuesta «No fijó / falló» a aprobar; temporizador/fin/rechazo visible y protección SL-S1-04 para el modo futuro. |

Cada comportamiento futuro requiere esqueleto compilable y RED observado por aserción antes de GREEN; no errores CS ni suite antigua como éxito. Una implementación autorizada requiere regresión fresca/check-release adecuados bajo su propio alcance; build/instalación/sesión requieren autorización propia. Defaults45c4e9af5c8f/S1-sling7d1a61c39766/41heredados/SimulationKey intactos ahora; R2-3 rige cualquier clave futura, con hash real registrado y sin promover preset.

**B0–B5 futuros, adaptación para revisar:**

- B0: cuenta atrás/comprensión/cancelación antes de medir.
- **B1: diez apoyos T1–T5 dos veces y retirada sin carga. «Soltar suave» ya no exige Swing; Tap/abandono C-A esperados**, error del Began y feedback por separado.
- B2: diez suaves/medios/fuertes al T3 realmente buscado; Tap/C-A se clasifican y conservan.
- B3: diez medios a T1/T2 bajos; errores de apoyo/resbalón/C-A separados.
- B4: cinco cancelaciones por retorno; reapoyo con id nuevo y enlace, sin sustitución.
- B5: ritmo cómodo incluyendo el costo de corregir. Secuencias/objetivos/bandas previos conservados; no repetir para sustituir fallos.

Cada Began tiene id propio. Reapoyos extra/cadenas registrados sin borrar el primero; cierres neutros planeados de B1 no son cancelación accidental. Tercera respuesta y semántica del diseño necesitan aprobación; respuestas antiguas mixtas intactas.

Antes de cada bloque futuro: control/preset/hash/pendientes0/Jugando/mano/condición sin ayuda declarados, repeat0 temporal sin guardar; sin cambios durante gesto. Restauración P0 con arranque nuevo/hash extendido de la build autorizada y tres archivos byte a byte al respaldo. Perfil incorporado efectivamente válido antes de instalar; ninguna instalación autorizada ahora.16preguntas originales literales/por separado, métricas descriptivas sin nuevos umbrales; no inferir aceptación S1. **Entregar PR documental lista para Claude, sin fusionar y detenerse. S2–S4/T8 no autorizados.**


**Precisiones documentales de revisión de Claude sobre e415bcb (sin ejecutar1.2c):**

- En1.2c-5, proponer texto visible **«Apoyo sin querer»**, conservando la correspondencia interna con «Se fijó antes de tiempo» (`premature=true`); texto pendiente del PO al autorizar implementación. B1/B3 de S1-sling mezclaron esa respuesta con fallos/no fijación: no reetiquetar ni atribuir intención medida.
- En feedback/B0 y explicación previa: **«Para cancelar, vuelve a la altura donde apoyaste, o más arriba, y suelta»**; d≤zona muerta descendente. No exigir círculo ni región superior/lateral.
- En1.2c-2, incluir variante **N04b**: resbalón mayormente lateral, componente descendente apenas fuera de zona muerta, desplazamiento máximo≥3mm y Swing con P≈0 positivo no buscado. Sin valor/umbral/filtro nuevo ni aserción ficticia de voluntad.
- En1.2c-1, política de rechazo con motivo fuera del muro frontal (laterales/suelo/rayo inválido) marcada **propuesta de Codex**, distinta del tin/out rojo aprobado por el PO.

Implementación/TDD requieren autorización separada; también build, instalación y sesión. S1 no aceptado; S2–S4/T8 no autorizados.

## Nueva adenda D4 — preparación de S2 con M2-4 (PO, 2026-10-30)

**Adenda aditiva, solo documental:** todo el texto anterior, incluidas D1–D3 y las tareas 2.1–2.7, se conserva byte a byte. D4 actualiza su aplicación a M2-4 sin renumerarlas. No modifica spec v0.3 ni escenarios, criterios, bandas o umbrales. **No autoriza implementar S2, escribir tests, cambiar presets/defaults, ejecutar suites/check-release, build, instalación, sesión ni acceder al iPhone.** El código de S2–S4/T8 necesita otra autorización. [Decisiones y cierre de PR #28](../../reviews/2026-10-25-first-shot-po-approvals.md), [propuesta D4](../specs/2026-10-24-first-shot-design.md).

### D4.1 Base, decisión de control y alcance

S1 fue aceptado explícitamente por el PO con M2-4 para avanzar hacia S2; no se cambian sus resultados anteriores ni sus límites. **M2-4 será el control por defecto al preparar la implementación de S2**, mediante la nueva tarea previa 2.0. A′ y S1-sling se conservan configurables. Hoy, en la base `1b0ba27`, `gesture.control=0` sigue en defaults: esta adenda no lo cambia ni afirma que la app instalada haya sido promovida.

La decisión de saturación es **observarla con golpe real en S2**, sin cambiar `power.pullFullMm=35`. Se conservan `power.pullDeadZoneMm=2`, N2 estricto de desplazamiento máximo <3 mm y duración <250 ms, curva/bandas vigentes, dirección descendente, dominio A y referencia sin ayuda adicional. No halo, lupa, cámara/zoom nuevos, dominio B, decaimiento, nuevo detector de intención ni filtro de N04/N04b. La desviación de §6.4 aceptada para M2-4 sigue explícita; spec intacta. El rechazo del apoyo fuera del muro frontal y la invalidez tin/out siguen separados.

**Fuentes de implementación actuales, sin ejecutarlas:** [M24Model](../../../Assets/SquashClash/Core/Shot/M24Model.cs), [FrontisProjection](../../../Assets/SquashClash/Core/Shot/FrontisProjection.cs), [GesturePreview.M24](../../../Assets/SquashClash/Runtime/Shot/GesturePreview.M24.cs), [GesturePreview](../../../Assets/SquashClash/Runtime/Shot/GesturePreview.cs), [host](../../../Assets/SquashClash/Runtime/Ball/BallSimulationHost.cs), [SimClock](../../../Assets/SquashClash/Core/Simulation/SimClock.cs). M2-4 actualmente registra preview y **no produce contacto de pelota**; `ShotRequest`, `ClockMap`, `ContactContract` y el rollback de S2 son interfaces previstas, no capacidades ya presentes. S2 integrará el flujo táctil existente con la única autoridad del host/P2; no creará otro input, reloj o simulación autoritativa.

### D4.2 Orden de golpe e intento por recepción (2.3–2.6)

Contrato propuesto para la integración S2, sometido a revisión con D4:

- Cada Began admitido durante Recepción captura dedo primario, `LaunchId`/recepción, configuración del lanzamiento y del gesto, escala mm/px y proyección realmente usada. El propósito de un contacto iniciado fuera de Recepción no cambia al entrar en ella (§6.5.1); requiere nuevo Began. El `launchSerial` del preview se correlaciona con `LaunchId` del host, no se trata como identificador global independiente.
- `ShotRequest.AimOnFrontWall` es **el objetivo en metros fijado por ese Began**. No se toma la posición del Ended, la posición actual de la pelota ni el centro del anillo. `ShotRequest.P` es la potencia final del **Ended del mismo contacto**, por distancia descendente actual, no máximo retenido, velocidad o valor leído después desde el panel. `ShotRequest.ContactCenter` procede del estado autoritativo en el paso de contacto decidido, no de la pelota en Began ni en el frame de recepción del input.
- `rEvento` es el timestamp original de ese Ended, en el dominio real del adaptador existente. Began, fijación, `lockObservedAt`, recepción del evento, fin de animación y frame no son r. `rCorregido = rEvento − compensación` se calcula una sola vez en 2.3. `ShotRequest` de 2.4 sigue siendo el payload geométrico/P; una orden inmutable asociada conserva r original/corregido, dedo, `attemptId`, `LaunchId`, hashes y versión de plan. No se añade a escondidas una dependencia temporal al solver.
- **Solo `GestureOutcome.Swing` entrega una orden al contrato de contacto.** Primero se aplican los guardas de identidad/contexto e intento ya consumido; dentro del gesto se conserva sistema → inválido → N2 → C-A → Swing. El gate de intento único no convierte otros cierres en Swing ni altera su registro.

| Cierre del gesto M2-4 | Orden/contacto y estado del intento de juego |
|---|---|
| `Swing` válido del contacto/recepción vigentes | Reserva/consume atómicamente el único intento al aceptar la suelta, aunque el contacto se programe para T*. Después no hay corrección/cancelación ni segundo Swing. Resolver Perfecto/Válido/Fallido o las categorías propias del contrato. El éxito de pelota no se infiere del cierre del gesto. |
| `InvalidAim` (incluidas tin/out/líneas) | Mira roja y cierre sin orden, sin solver, sin contacto y **sin consumir**. Puede reapoyar si aún está en Recepción y queda oportunidad temporal; no se recorta a un objetivo legal. Es una concreción operativa de D4, no un Swing fallido de timing. |
| `Tap` | Sin Swing ni consumo, N2 estricto con prioridad sobre C-A. No cuenta como golpe suave; conserva el intento y el registro del toque. |
| `AbandonedNoPull` | C-A sin haber cargado: sin orden ni consumo. No fabricar golpe P≈0. Reapoyo nuevo si la recepción sigue disponible. |
| `CanceledReturnToDeadZone` | C-A después de cargar: sin Swing ni consumo; soltar a la altura del apoyo o por encima. Reapoyo con id nuevo/vínculo, sin borrar el intento táctil anterior. |
| `Rejected` (área/rayo/fuera del muro) | Sin orden ni consumo; motivo visible y registrado. Lo filtrado antes de crear gesto no inventa un `attemptId` de juego. Un apoyo posterior válido debe ser nuevo Began, sin promover otro dedo. |
| `SystemCanceled` | Sin orden ni consumo (UI previa sin gesto se audita filtrada, no como gesto nuevo): pausa/foco/DEV/cambio de contexto/datos contradictorios o relanzamiento invalidan el gesto, no inventan Ended. Al volver no hay Swing; exige nuevo Began. |
| `None`/contacto sin Ended al terminar la recepción | No constituye orden ni reserva el intento por estar fijado/cargado. Al cerrar la oportunidad, registrar `NoOrder` si corresponde al caso 6; si nunca hubo plan alcanzable, preservar la categoría de caso 7. No disparar automáticamente al expirar K. |
| Orden de otro lanzamiento, duplicada o con intento ya consumido | Se descarta con motivo/id; no produce segundo Swing ni consume el intento de la pelota nueva. |

El «intento de juego» es por recepción; cada intento táctil rechazado/cancelado conserva su propio id. No consumir no significa congelar o ampliar K: cancelar/reapoyar cuesta tiempo. Si al terminar el vuelo solo hubo cierres sin orden, el ciclo registra la omisión/no oportunidad correspondiente y pasa por las fases de 2.6, sin repetir hasta lograr éxito.

Un Swing aceptado antes del fin, aunque no tenga contacto por ser temprano/tarde/fuera de V o inalcanzable, **ya consumió** el intento. Solver sin solución, divergencia autoritativa o historia insuficiente se registran como `SystemFault`/`NotEvaluable` según 2.3–2.5, nunca como fallo del PO ni como contacto inventado. La reserva no se reabre para dar un segundo golpe en esa recepción; una orden obsoleta se descarta sin afectar la nueva. No confundir `SystemCanceled` antes de la orden con un fallo de infraestructura posterior a un Swing aceptado.

El cierre de recepción por frame no puede borrar una suelta anterior que llegó tarde: dentro del mismo `LaunchId`, con r verificable e historial disponible, se resuelve por su timestamp original incluso si la pelota ya terminó al procesarla. Una omisión emitida antes de recibirla se reconcilia como nueva versión append-only, no como dos intentos. Tras un relanzamiento real, la orden antigua no toca la pelota nueva; no retener ni trasladar el contacto entre recepciones. Conserva Review Focus 1/4 y el ciclo de observación/repetición de 2.6, sin otra autoridad de repetición.

### D4.3 Vuelo, duración del gesto y reloj (2.1/2.2/2.3)

**Evidencia existente, no nueva medición ni duración obligatoria:** las medianas Began→cierre de las tres series B2 fueron aproximadamente 0,9–1,7 s; [informe S1](../../reviews/2026-10-28-first-shot-s1-m2-4-implementation.md) y [resumen](../../validation/evidence/2026-10-30-m24-session-summary.json). No representan todas las tareas, un límite por gesto ni una anticipación garantizada en partido. Fijar al apoyar no elimina el tiempo de tensar y elegir la suelta. Antes de medir S2 habrá que comprobar si cada vuelo deja margen para hacerlo, sin alargar A/V ni ralentizar la pelota para hacer pasar el resultado.

Para cada SL1–SL6 de 2.6, la implementación deberá registrar, **antes de evaluar al PO**, lanzamiento real, primer frontis/bote, K exacto y T* del plan, convertidos con `ClockMap`; estos lanzamientos todavía no existen en main y no se deducen de L1–L6. El horizonte de 10 s de `BallSimulationHost.Launch` es un límite de predicción actual, no una medición del tiempo disponible. L1–L6 no se retocan. El tiempo de simulación depende de τ y de los segmentos del reloj: no comparar segundos de trayectoria directamente con segundos de dedo, ni escalar A/V por τ.

Análisis que deberá contener el registro por recepción, sin calcular resultados ahora:

- `b = Began.timeReal`, `e = Ended.timeReal`, `d = e−b`, `lead = T*_real−b`. Entonces `Δ = (b+d−compensación)−T*_real`. Para acabar cerca de T*, el apoyo normalmente debe antecederlo en aproximadamente la duración del gesto; **no se fija un lead mínimo** a partir de las medianas S1.
- Comparar tiempo lanzamiento→T*, tiempo disponible desde la presentación de la recepción/anillo y duración real de cada gesto, además de las fronteras exactas de K. El arranque antes del primer bote puede preparar el objetivo, pero no autoriza volea; decide el Ended.
- Si una recepción deja poco tiempo para la duración observada, registrar esa incompatibilidad de ritmo **[EXPERIMENTO]**, con todas las omisiones/cancelaciones/tempranos/tardíos. Elevarla al PO; no cambiar SL, velocidad, tempo, tamaño de ventanas ni programar contacto al Began sin autorización.

| Situación temporal | Comportamiento previsto, sin umbral nuevo |
|---|---|
| Began temprano durante la misma Recepción, incluso antes de K | Fija el objetivo y permite tensar/esperar. Sin golpe al apoyar; r sigue siendo Ended. La retención intencional conserva P por distancia, sin decaimiento o timeout. |
| Began llega tarde pero su Ended aún satisface el contrato | No invalidar por duración corta ni por una hora límite de apoyo nueva. N2/C-A y el contrato en r deciden; no hay requisito de 0,9 s o 1,7 s. |
| Ended antes de k_in | Swing consume intento: T3 aplica caso 4/al aire; T3-suave conserva su excepción existente al primer punto alcanzable. No se corrige por haber apuntado bien o empezado antes. |
| Ended en K | Plan visible en rEvento, Δ en ms reales y reglas exactas de 2.3: bordes A/V intactos; V experimental frente a T3 puro. La geometría autoritativa puede impedir contacto aunque la predicción anunciara K. |
| Ended fuera de A pero dentro de V/K, o fuera de V en modo T3 puro | Aplicar los mismos casos Válido de 2.3; no degradar adicionalmente por durar más ni por haber empezado tarde. En modo V, fuera de V es Fallido aunque esté en K; la comparación con T3 puro se conserva. |
| Ended después de K o en el segundo bote | Nunca contacto ilegal; tarde/caso13 según frontera exacta. Consumo si es Swing vigente; no reasignar a T* anterior ni recortar r a la ventana. |
| Ended capturado a tiempo y entregado tras K/segundo bote | La entrega tardía no equivale a soltar tarde. Resolver r original/compensación una vez con historial y plan visible, manteniendo identidad y métricas de edad de orden. Fuera del historial, falla del sistema/no evaluable. |
| El plan cambia entre Began y Ended | La mira de intención permanece en el objetivo del Began; el anillo usa el plan presentado vigente en rEvento, no el plan de Began ni el más reciente al procesar. T* no queda congelado por apoyar. |

Se conservan en 2.1 los segmentos en double/pausa/τ/cámara lenta/DroppedSteps y los casos de 2.3/R2-1/R2-4: r exacto clasifica, solo el paso físico se cuantiza, empate anterior/vecino legal y error firmado registrados. Pausa, foco, DEV, cambio de tempo/cámara lenta o relanzamiento durante el gesto invalidan sin consumo; cámara lenta invalida métricas, no es un recurso para la campaña comparativa. No agrandar el historial porque el gesto dure un segundo: hace falta historia desde el **contacto** a su procesamiento; conservar la configuración de historial ya prevista y medir edad/cobertura real.

### D4.4 Seguimiento de cámara con objetivo fijo

[FrontisProjection](../../../Assets/SquashClash/Core/Shot/FrontisProjection.cs) captura matrices/viewport/área al **procesar** Began; no hay historia de cámara que demuestre una proyección retrospectiva en b. Registrar `b`, recepción, snapshot/`ObservedAt`, `lockObservedAt` y O2 por separado, sin convertir el snapshot en imagen exacta vista por el PO en el timestamp original. D4 conserva este contrato y no introduce compensación espacial de la cámara.

El objetivo queda fijo en coordenadas del frontis (x,y; z del muro); el ancla de tensado queda en coordenadas de pantalla/mm del Began. [CourtCameraRig](../../../Assets/SquashClash/Runtime/Camera/CourtCameraRig.cs) traslada lateralmente la cámara y su punto de mira en seguimiento; la [mira](../../../Assets/SquashClash/Runtime/Shot/AimReticleView.cs) se sitúa en el mundo. Por tanto, puede cambiar su píxel al moverse la cámara **sin cambiar AimOnFrontWall ni P**. No mantener el píxel inicial reproyectándolo a otro punto, no usar Moved para retocar el objetivo y no exigir que quede debajo del dedo. Cambio de viewport/área segura conserva la invalidación de contexto existente, distinta del movimiento normal de seguimiento.

El punto factible del solver se muestra aparte de la intención, como ya pide 2.6; nunca retargetea silenciosamente el Began. Comparar fija/seguimiento con las mismas condiciones declaradas **[EXPERIMENTO de observación, sin parámetros nuevos]**, sin modificar cámara ni zoom. Conservar el cálculo de salto de rollback al mismo instante visual/Alpha y **misma cámara** antes/después, sin suavizarlo; separar movimiento ordinario de seguimiento de salto por cambio de trayectoria. Si el tamaño/oclusiones impiden entender la mira o el anillo, registrar y detener para decisión, sin añadir halo/lupa sobre la marcha.

### D4.5 Tarea previa 2.0 — promoción del control, solo prevista

Se ejecutará **solo con autorización de implementación S2** y antes de 2.1–2.6. La decisión del PO autoriza planificar `gesture.control=2` en la fuente única `DefaultPreset.json`; no promover íntegramente S1-M2-4 ni P0. El selector0/1/2 ya existe en [ParamCatalog](../../../Assets/SquashClash/Core/Config/ParamCatalog.cs), así que no se necesita una clave nueva para esta promoción. En2.0, el delta de valores existentes será únicamente `gesture.control`; las claves S2 que añadan2.2–2.6 siguenR2-3, no alteran los41 heredados ni SimulationKey. `pullFullMm=35` permanece.

TDD futuro de 2.0, con esqueleto compilable si hace falta y RED **por aserción** → cambio mínimo → GREEN:

- `Defaults_SelectM24_Control2_DevAndProd` falla con el valor actual0; tras el cambio exige 2. Una prueba Main verifica la ruta real por defecto, no solo la existencia de la opción en catálogo. A′/sling se seleccionan explícitamente en pruebas que verifican sus comportamientos, sin borrarlas.
- `DefaultChange_OnlyGestureControl_AmongExistingValues`, `Inherited41Values_AndSimulationKeyRemainUnchanged` y `M24DistanceSettings_Stay2And35_N2Strict` comprueban los conjuntos/valores reales. No devolver un hash antiguo ni limitar la comprobación a un nombre de preset.
- Revisar [DefaultPresetHashTests](../../../Assets/SquashClash/Tests/EditMode/Shot/DefaultPresetHashTests.cs) (`NewHash_IsRecorded`) y [GesturePreviewTests](../../../Assets/SquashClash/Tests/PlayMode/GesturePreviewTests.cs) (`Main_WiresAimAndPowerWithoutChangingBallAuthority`, fixtures que asumen A′). Actualizar expectativas de hash justificadas por el cambio autorizado; conservar aserciones del comportamiento A′ con setup explícito0. Añadir prueba de integración M2-4 por defecto. Las aserciones de First Ball/L1–L6 se mantienen; solo su preparación dependiente del arranque puede fijar `practice.enabled=0` conforme a la excepción existente, declarada en PR.
- `LegacyP0Overlay_UsesM24ButPreservesFileAndInherited41`, `ExplicitApostropheOrSlingPreset_RemainsSelectable`, `P0Selection_RoundTripIsByteIdentical`: [PresetCodec.Overlay](../../../Assets/SquashClash/Core/Config/PresetCodec.cs) hereda defaults para claves ausentes. P0 antiguo con41claves podrá heredar2; un preset que contenga explícitamente control0/1 seguirá expresándolo, no se migra a escondidas. Mostrar/hashar la configuración efectiva y detectar qué caso se cargó. No reescribir los archivos P0/Po-1/last-active para simular la promoción.

| Registro de hashes | Base de esta entrega | Resultado futuro que debe registrar la implementación |
|---|---|---|
| Defaults | `45c4e9af5c8f`,85 claves,control0 | Hash real después de 2.0 y después de añadir claves S2; número de claves y delta por commit. **Pendiente, no calculado ni fijado aquí.** |
| P0-calib-1 efectivo extendido | `e458bfd5ce9e` en build0f8002d | Hash real del overlay con los defaults de cada build S2, comprobado en arranque nuevo; pendiente. No usar el hash anterior como expectativa final. |
| Proyección de los41 heredados | defaults `0d39e6016bd8`; P0 histórico `8de3f2ecb217` | Idénticas; P0 conserva repeat3, sin promoverlo. |
| SimulationKey | `166a96e319c7` | Idéntica. |

Los cambios futuros de hash son consecuencia de control/claves S2 autorizados, no de física heredada. Escribir hashes reales en informe/aprobaciones y metadatos de build antes de instalar, sin valores predichos. Baselines S1 permanecen como históricos; los datos de S1 no se reetiquetan con el nuevo hash.

### D4.6 TDD adicional por tarea y negativos predeclarados

**Solo planificación:** estos nombres describen pruebas futuras, no archivos de test creados ni resultados RED/GREEN observados ahora. Se mantienen todos los tests/casos de 2.1–2.6 y los de1.2c/S1; los siguientes los complementan. Preparar fixtures compilables no cuenta como RED; erroresCS/cero tests/XMLantiguo nunca son un RED válido. Después de cada implementación autorizada, pruebas focales y regresión adecuada según el plan; al cerrarS2, ambas suites completas frescas y check-release dev/prod limpios, solo si se autoriza esa fase.

| Tarea futura | Casos adicionales a predeclarar y orden RED→GREEN |
|---|---|
| 2.0 | Promoción, hashes y fixtures explícitos deD4.5. No cambiar a la vez valores de potencia ni promover un preset completo. |
| 2.1 | `M24LongGesture_OriginalEndedMapsAcrossFrames`, `GestureDuration_IsNotRollbackAge`, `LateDelivery_UsesEventTime_NotCurrentFrame`, `PauseTempoFocusDroppedSteps_InvalidatesWithoutSyntheticRelease`. Datos sintéticos alrededor de fronteras existentes; no obligar duración humana fija. |
| 2.2 | `EachSL_ReportsLaunchKAndTStarInRealClock`, `BeganBeforeFirstBounce_DoesNotAuthorizeVolley`, `NoK_PreservesUnreachable`, `ShortLead_IsReportedWithoutChangingFlightOrWindows`. Factibilidad/anticipación descriptivas, sin aserción que convierta medianas humanas en criterio de vuelo. |
| 2.3 | `LateBegan_StillJudgedByEnded`, `PlanChangedSinceBegan_UsesVisibleAtOriginalEnded`, `OriginalEndedVersusCorrected_SelectPlanOnce`, `EndedExactlyAAndV_PreservesBoundaries`, `EndedBeforeKIn_OrAtSecondBounce_PreservesT3AndSoftCases`. Conservar también V/T3 puro, caso8, Kexacto y vecino legal de la lista original. |
| 2.4 | `RequestUsesBeganAimAndEndedP_ContactCenterFromDecidedStep`, `HeldPull_UsesCurrentDistance_NoDecay`, `ReturnBeforeEnded_ReducesP`, `FailedSolve_DoesNotRewriteIntention`. Probar N04/N04b con salida potencial no querida, sin aserción falsa de que el sistema reconoce intención. |
| 2.5 | `SameM24Ended_PunctualAndDelayed_SameBits`, `DelayedEndedAfterSecondBounce_PreservesOriginalR`, `DuplicateEnded_AndReentrantCallback_ConsumeOnce`, `OldLaunchEnded_DoesNotTouchNewBall`, `FuturePerfect_ReservesOnce_NoSecondOrder`, `NoSwingClosures_DoNotInvokeApplyOrSchedule`. Conservar eventos/efectos/costos y salto sin suavizado; pausa después de una orden no permite duplicarla. |
| 2.6 | `M24ClosureTable_PreservesAttemptAndReason`, `NoOrderWithOnlyCanceledContacts_DoesNotShoot`, `BeganOutsideReception_IsNotPromoted`, `CancelThenNewBegan_CorrelatesWithoutReplacement`, `SameTimestampIdenticalEnded_OneShotWithReliableP`, `ContradictoryEnded_NoShot`, `SecondaryFinger_NoSecondOrder`, `FollowCamera_MovesScreenProjectionNotWorldAimOrPower`, `FeasibleMark_DoesNotMoveIntention`, `PendingOrWrongHash_BlockArmRejectedWithRemedy`, `ControlProtection_NoAccidentalP2H17Scenario`. Conservar guardas/input único, reloj, contexto y registro de orden/cierre. |
| 2.7 | No RED de «test humano»: requiere instrumentación probada en 2.6, preparación verificada y datos nuevos. Smoke de prod real separado de check estático; protocolo no empieza por tener símbolos correctos. |

Negativos **M2S2-N01–N12**, además de las fronteras exactas del plan original, con fixtures y expectativas declarados antes de implementar:

1. **M2S2-N01:** apoyo sin tirón, duda/pausa 150 ms, tap/temblor dentro de zona muerta → P=0/sin Swing/sin consumo; si termina como Tap, no reclasificarlo C-A. No inventar Ended si se mantiene el dedo.
2. **M2S2-N02:** tirón deliberado retenido y suelta → P por distancia conservada incluso≥0,60; sin decaimiento ni restricción temporal nueva. La regla original 150 ms/P<0,60 del método de potencia B de A′ queda intacta para esa alternativa.
3. **M2S2-N03:** N04/apoyo accidental+resbalón descendente → puede producir Swing y consumir el intento; registrar intención declarada aparte, **no afirmar que la regla por fases lo protege**. N04b lateral con descenso apenas fuera de 2 mm y desplazamiento máximo≥3 mm puede producir P positivo pequeño; no añadir filtro/umbral o forzar P=0.
4. **M2S2-N04:** retorno a zona muerta tras carga y suelta, frente a retorno y nueva salida antes de Ended → C-A sin consumo frente a Swing que consume; no cancelar por cruzar la zona sin soltar.
5. **M2S2-N05:** invalidez tin/out y Began fuera del muro/área/rayo → sin contacto/consumo, motivos distintos; no recorte legal ni Swing con P=0. Prioridad invalidez sobre Tap cuando ambas condiciones se cumplen.
6. **M2S2-N06:** Ended idéntico con el mismo timestamp que la muestra previa → cierre único con P fiable; Ended duplicado posterior ignorado. Datos contradictorios/no finitos/no monótonos → cierre conservador sin orden ni r ficticio.
7. **M2S2-N07:** segundo dedo, dedo sostenido a través de pausa/foco/DEV/cambio de configuración/tempo/cámara lenta/viewport → sin promoción/Swing al reanudar; nuevo Began requerido. Distinguir cancelación previa de una orden ya aceptada.
8. **M2S2-N08:** cancelar→reapoyar, objetivo errado→C-A→nuevo apoyo, varios fallos antes del fin deK → todos con id/enlace/tarea/contexto; sin sustituir el primer error ni prolongar la recepción.
9. **M2S2-N09:** Began tardío, Ended temprano/tardío/fueraV o sin plan → categorías/consumo deD4.2–D4.3, sin salto al T*para rescatar el gesto. Casos exactosA/V y segundo bote intactos.
10. **M2S2-N10:** Ended anterior a Kout, entregado después de segundo bote; viejoLaunchId, historia insuficiente y emisión de omisión previa → resolución/reconciliación por evento o motivo de sistema, sin tocar pelota nueva ni fabricar tiempo.
11. **M2S2-N11:** cámara fija/seguimiento, plan cambia después de Began, input en tránsito O2 → objetivo mundo inmutable/proyección usada registrada; plan visible en r y distinción de cámara deD4.4.
12. **M2S2-N12:** dos callbacks/órdenes, nuevo contacto después de Swing programado, preset explícitoA′/sling y P0 legacy tras promoción → un intento/recepción, alternativas conservadas y hashes reales, sin reinterpretar eventos de un control con reglas de otro.

### D4.7 Adaptación del protocolo 2.7 y abiertos de S1

**Sesión futura, aún no autorizada.** No se reutilizan S1/B0–B5 como si hubieran tenido golpe real. Los169intentos programados más1extra se conservan como referencia con sus desviaciones; tampoco se simulan resultados de S2 con sus trazas. La aceptaciónS1 no demuestra latencia, seguridadN04/N04b o fluidez de rally.

**Antes de entregar el teléfono:** implementación aprobada/regresión/check-release dev/prod/exportlimpio, build devReleaseARM64/noDevelopment e identidad trazable; perfil efectivamente incorporado vigente (actual vence2026-11-03T18:29:44Z), renovar si la fecha efectiva lo requiere; respaldo nuevoP0-calib-1/Po-1/last-active y cotejo byte a byte después de instalar. Arranque nuevoP0 con **hash extendidoS2 realmente registrado**, cero pendientes. Preparar modo/práctica/selección temporal sin guardar los presets; reponer last-activeP0 **antes** de aplicar/relanzar, comprobar control2/hash/receptionConfig/gestureConfig/0 pendientes/Jugando y cerrarDEV. Sin campaña hasta autorizarla y verificar esos datos.

`practice.enabled=1` conserva el único temporizador de 2.6: suspende `launcher.repeatSeconds` sin persistir cambios al 3 deP0. Para preparación/ejercicio manual sin práctica, repeat0temporal sin guardar; no confundir esa pausa de repetición con el ciclo automático de observar/esperar. Nadie inicia Escenario/P2/H17 durante preparación/protocolo; mantener protección y auditoría de controles, sin atribuir la causa de activaciones antiguas.

**Explicación previa propuesta:** «Ahora sí hay golpe de pelota. Apoya en el frontis para elegir dónde; bajar el pulgar carga potencia. Debes soltar cuando toca golpear: el apoyo no golpea. Para cancelar, vuelve a la altura del apoyo o más arriba y suelta. Si debes corregir el punto, cancela y apoya otra vez; ese tiempo cuenta». El PO identifica mira/anillo/carga/cancelación antes de medir; falta de comprensión o defecto obliga a registrar/restaurarP0/detener, sin ajustar nada sobre la marcha.

**Orden de bloques para predeclarar al autorizar2.7:**

1. Familiarización con contacto real y una cancelación voluntaria; si usa bloque con cuenta atrás, el PO confirma verla **antes del primer gesto**, no retrospectivamente. Duración/tamaño/objetivos de este bloque **[EXPERIMENTO]** deben quedar escritos antes de iniciar, sin inferirlos de B0 ni añadir una barrera de aceptación numérica aquí.
2. SL1–SL6 con V=100 y T3 puro, manteniendo T3 (no T3-suave) en ambas ramas para aislar la condición del PO. Orden y número de recepciones por bloque **[EXPERIMENTO]**, predeclarados con misma mano/control y registro de posibles aprendizaje/fatiga. Cámara lenta apagada; sin calibraciónH3 ni compararS4 ahora. T3-suave, si se autoriza observarlo, es bloque separado, no mezcla en la comparaciónV/T3.
3. Ejercicios explícitos de potencia/cancelación/riesgo y ritmo deD4.7 abajo, con SL/tarea/potencia solicitada visibles. Un bloque no puede pedir «ritmo cómodo» y asumir después que se siguió una secuencia de intensidades: aclarar la tarea **antes**, con ejemplos, y registrar después si se siguió. Si se autorizan fija/seguimiento, cada condición se etiqueta y se mantiene estable dentro del bloque; no cambia la cámara para ganar tiempo.
4. Recorrido funcionalprod de 2.7, Step 1b separado, misma fuente de producto con control2: PO observa lanzamiento/golpe/nota/sonido sin panel. No inferirlo de build/check estático ni mezclar sus datos condev. Requiere respaldo/cotejo/perfil efectivos para cada instalación autorizada.

**Un bloque cada vez:** Codex pide únicamente el siguiente y espera final/recuperación antes de pedir otro. Justo antes de **cada** bloque verificar build/control/preset/hash efectivo y de recepción, cero pendientes/Jugando/foco, mano activa y tarea/condición/cámara/ventana; registrar armadoaceptado/rechazado con motivo/remedio. Un reinicio vuelve aP0 por diseño: comprobar/repreparar antes de continuar y abrir segmento nuevo, sin reutilizar ids como globales ni afirmar continuidad. Si el PO se adelanta, conservar todos los intentos y declarar desviación, sin repetir ni sustituir hasta éxito. No tocar/acceder al teléfono durante gestos del PO. Cambios de configuración/contexto invalidan comparabilidad de afectados y nunca reevalúan la suelta con valores del panel posteriores.

| Abierto de S1 que se incorpora a 2.7 | Cómo observarlo sin resolverlo ni cambiar parámetros |
|---|---|
| Saturación fuerte50% sobre advertencia20% | Pedir intensidades voluntarias y realmente declaradas, con objetivo/timing; informarP/distancia/banda/saturación sobre **todos** los intentos y cuántos tuvieron contacto. Comparar salida solicitada y salida del solver (`SpeedRaised`/`TargetLowered`) separadas de P. `pullFullMm35` no cambia por el resultado; cualquier ajuste necesita decisiónPO. |
| N04/N04b sin evaluación física | Bloque negativo predeclarado de apoyo sin tirón/tap/temblor, y apoyo no querido/resbalón descendente o lateral. Conservar posiblesSwing/contacto/P y respuesta de intención por intento; un resbalón solicitado para probar el caso no estima su frecuencia espontánea. Respetar N2/C-A; no forzar un «pasa» ni atribuir voluntad al algoritmo. |
| «Aún mecánica»/ritmo de rally | Bloque con golpe real a ritmo cómodo y tareas explícitas; medirBegan→Ended, anticipación, cancelación→reapoyo/transiciones y duración de recepción/ciclo por separado. La espera del launcher no es duración del gesto ni prueba de rally. Preguntar fluidez literal; no empezar rival/rally/S3–S4/T8 ni extrapolar de una práctica a partido real. |
| Preferencia arriba/adelante | Registrar juicio tras usar la dirección descendente aprobada. No invertirla en esta sesión ni asignarla a una mano sin autorización; comparar direcciones requeriría otra decisión/diseño/orden predeclarado. |

Antes del feedback, repetir ejemplos de la triada para separar **OK** (orden/fijación querida observada, aunque falle puntería/timing), **Apoyo sin querer** (correspondencia `premature=true`) y **No fijó / falló** (fijación no obtenida/identificada o fallo operativo; motivo aparte). Una respuesta humana no cambia el cierre técnico ni la categoría de timing. Conservar intento/recepción sin feedback como tal; no rellenarlo por P ni usar un botón ambiguo para dos causas.

**Registro/informe 2.7:** por lanzamiento/contacto: build/control/preset/hashes/semilla, b/rEvento/rCorregido/recepción/lockObservedAt/snapshot/O2 y desplazamiento, plan presentado y T*, K exacto, Δ/categoría/paso/contacto, cierre/consumo/motivo/id/reapoyo/configuración, P/distancia/banda/saturación, objetivoBegan/error al objetivo pedido/objetivo factible/error del solver, cámara, edad de orden/historia/pasos resimulados/costos por parte/efectos obsoletos. Error de apuntado, tolerancia del solver y calidad del timing se informan separados, sin umbral de puntería nuevo. Saltos p50/p95/máximo en m/px/diámetros, mismo instante/cámara y sin suavizado; el PO debe juzgarlos antes de fusionarS2 (condición1). No borrar el último intento si el evento de fin de bloque se emitió antes que su registro: correlacionar por id/trial y conservar los datos originales.

**Juicios por separado, respuestas literales:** conservar las preguntas de 2.7 «¿la pelota va donde apunto?», «¿el timing es justo?» (preliminar, sin afirmar calibración de latencia) y «¿el salto es aceptable?», más las16 preguntas originales de S1 que figuran en [respuestasM2-4](../../validation/evidence/2026-10-30-m24-po-answers.json). Relacionar sus respuestas con saturación/riesgos/fluidez/dirección sin inferir porcentajes de «no recuerdo» ni aprobación de otra fase. No confundir la mecánica referida por el PO con la medición técnica.

**Cierre futuro:** recuperar evidencia antes de que opere la retención, restaurar P0 en **proceso nuevo**, observar el hash extendido propio de la build S2 y cero pendientes/selecciónP0, cotejar los tres archivos byte a byte con el respaldo de esa instalación. Preset/hash/número de claves real y proyecciones heredadas registrados. Informe nuevo separado de S1, limitaciones/desviaciones/NotEvaluated intactos; PR para Claude sin fusionar y parada para PO. Esta D4 se entrega ahora como documentación, no ejecuta ninguno de esos pasos.


<a id="adenda-d6-cq01"></a>

## Adenda D6 — calidad causal CQ-01 (PO, 2026-11-13)

[Decisión literal del PO](../../reviews/2026-10-25-first-shot-po-approvals.md#b4-cq01-po-decision-20261113), precisión técnica de Claude separada y **D36** de la [spec v0.3](../specs/2026-10-20-squash-clash-design-v0.3.md). Se incorpora de forma **aditiva**: todo texto previo se conserva como antecedente; esta adenda ajusta la Tarea 3.1 en los puntos siguientes. Se usa **D6**, sin reutilizar D5, reservada a la entrega de PR #32 según el encargo del PO.

### D6.1 Contexto de referencia e interfaz de T3.1

`Evaluate` **deja de recibir `bool perfect`**. Recibe un contexto de referencia de timing **inmutable**, preparado una vez por recepción antes de cualquier intento: trayectoria visible, `pace.tempo`, ClockMap, compensación vigentes, imagen J_A de todos los contactos Perfecto alcanzables y zona geométrica Z_T. Los nombres exactos de los tipos y parámetros quedan a criterio de la implementación. Se mantienen CausalVector, la aplicación sobre P/v antes del solver, la composición por componente y el registro separado por causa. La categoría y Δ no son argumentos de la magnitud; no se recalcula la zona a partir del intento.

J_A incluye programación, cuantización, vecino legal y empate del contrato. Z_T es la imagen del tramo de arco ±L alrededor del contacto ideal en el marco corporal, L=max|s| de J_A, recortada a K; d es la distancia euclídea mínima del contacto a Z_T. Timing es cero dentro y crece continuamente fuera. Se descartan el radio fijo y la envolvente universal entre tempos. La invariante de §17.A.12 se verifica **dentro del mismo contexto de recepción**, sin acotarla por categoría: mismo contacto/cuerpo, intención y contexto producen mismo vector/entrada al solver/golpe para cualquier Δ admitido, incluido +39/+41 ms al mismo paso. Otro tempo puede producir otra zona; la diferencia se documenta como ajuste de ritmo, no se exige igualdad entre contextos.

### D6.2 Pruebas de T3.1 y TDD

RED por aserción con esqueletos compilables antes de cada GREEN. Los dos nombres anteriores se conservan en el texto histórico de T3.1; sus sustituciones vigentes son:

- `Perfect_HasNoTimingVector_ButKeepsStretchAndJam` → **`AllReachablePerfectContacts_InsideZone_ButKeepStretchAndJam`**: el cero de timing surge de la inclusión geométrica de todos los Perfect alcanzables, sin anular E3/E5.
- `Valid_TimingVector_GrowsWithGeometricDistance` → **`TimingVector_GrowsContinuouslyWithDistanceToZone`**: crecimiento desde cero en el borde, sin salto geométrico por clasificación.

Se mantienen:

- `NoVector_HasLateralComponent` (DeltaLateral=0 siempre).
- `SameGeometryDifferentDelta_SameShot` (dentro de la misma recepción/contexto congelado, **sin acotar por categoría**, incluido Perfect/Valid +39/+41 al mismo paso).
- `Caps_ArePerComponent`.
- `AppliedBeforeSolver_NeverCorrectedAfter`.
- `MirroredSide_IsSymmetric`.

Se agregan:

- `ZonePreparedOncePerReception_IndependentOfAttemptCategory`.
- `ZoneCovers_AllReachablePerfect_AcrossTemposAndTrajectories`: tempo **0,3/0,65/1**, compensación **−75/0/+75 ms**, SL1–SL6 y trayectorias de rival; cada contexto cubre su imagen Perfect completa.
- `Determinism_SameInputsSameVectorAndShot`.
- `Edges`: borde geométrico exacto, ventanas **±40/±100 inclusivas**, KIn/KOut, vecino legal y empate anterior.
- `DifferentTempo_DifferenceDocumented_NotRequiredEqual`.
- `EarlySnap_Minus41ToMinus40_MeasuredAndDocumented`: medir/documentar el salto del contrato anticipado, sin modificarlo.
- `HumanAndRival_SharedPath_SameResult`.

No hay bonificación o penalización adicional por categoría, término directo de Δ ni azar. Anticipar y atrasar a igual distancia geométrica a Z_T usan la misma función; las diferencias por geometría o snap del contrato se miden y documentan sin alterar ventanas. El cambio de contexto entre tempos no es una excepción por categoría dentro de una recepción.

### D6.3 Ruta compartida y valores experimentales

**ShotController y RallyContact.Solve** usan la misma preparación de contexto, evaluación y aplicación causal. El humano usa human.x/z o PlayerBody efectivo; el rival usa su PlayerBody en el punto de llegada, con **el mismo alcance R**. La prueba de simetría compara entradas equivalentes a ambas rutas, sin dejar al rival sin capa causal. P/v se modifican antes del solver y nunca se corrige la salida. Cada vector con su causa se conserva en las trazas humana/rival, incluida rally-point si aplica.

Las diez claves `causal.*` de T3.1 mantienen sus valores: timingHeightPerM **0,6**; timingSpeedPerM **−0,25**; stretchStart **0,85**; jamDistance **0,35**; stretchHeight **0,4**; stretchSpeed **−0,2**; jamHeight **0,3**; jamSpeed **−0,15**; capHeight **1,0**; capSpeed **−0,4**. Todas son **[EXPERIMENTO] hasta validarlas en iPhone**; este registro no crea las claves ni modifica DefaultPreset.json. E3/E5 son independientes del cero de timing; E7 sin vector cuando no hay causas posicionales; técnica t=0. Sin E8/volea, efectos direccionales E1/E2/E4/E6 ni capa aleatoria.

### D6.4 Alcance documental y entrega futura

Esta adenda no ejecuta implementación, tests, experimentos, Unity, AutoQA/check-release, builds, iPhone ni instalación. Se mantiene la evidencia CQ-ZONE-v1 intacta. La propuesta histórica que rechazaba la invariancia «solo dentro del mismo contexto» queda sustituida por D36: la precisión es una decisión explícita del PO, no un filtro añadido a una prueba para hacerla pasar.

El [informe de PR #52](../../reviews/2026-11-13-first-shot-causal-quality.md) deja preparado el encargo futuro de implementación con TDD y **PR independiente**. La continuación actual es documental, para revisión de Claude coordinada por el PO, SIN FUSIONAR y sin merge automático. La implementación requiere el encargo autorizado posterior; antes del cierre de First Shot debe pasar revisión técnica y una prueba jugable. Las magnitudes siguen provisionales hasta iPhone; esta adenda no autoriza acceder al dispositivo ni declara First Shot aceptado. No se inicia otra ronda extensa de experimentación salvo defecto relevante.
