# Squash Clash: First Ball Implementation Plan (r2)

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.
>
> **En este proyecto:** Codex implementa en el Mac del PO, Claude revisa cada PR y el PO valida en el iPhone en los puntos de control. Las reglas de trabajo están en "Modelo de trabajo".
>
> **Revisión r2 (2026-10-20):** incorpora la revisión técnica de Codex (`docs/reviews/2026-10-20-first-ball-plan-review-codex.md`) y las directrices del PO. La respuesta a cada hallazgo está en `docs/reviews/2026-10-20-first-ball-plan-review-response-claude.md`. La versión anterior queda en `2026-10-20-squash-clash-first-ball.r1.md`. Las referencias `[Hnn]` remiten a los hallazgos de Codex.

**Goal:** dejar instalada en el iPhone 16 Pro del PO una app vertical que lanza una pelota de squash en una cancha WSF, la muestra de forma legible, permite calibrarla con un panel de desarrollador y produce la evidencia para elegir la física (§8.2 de la spec).

**Architecture:** un proyecto Unity en la raíz del repositorio, con cuatro ensamblados de producto y tres de pruebas:
- **Core:** sin escena ni MonoBehaviours. Usa los tipos matemáticos de Unity y `JsonUtility` [H20]. Contiene la geometría de la cancha, el contrato de simulación, el reloj, la configuración, los presets y las mediciones de la prueba de física. Se prueba en EditMode.
- **Runtime:** integración: PhysX, vistas, cámara y arranque.
- **DevPanel:** solo en builds con el define `SQUASH_DEV`.
- **Editor:** build y configuración del proyecto.
- **Pruebas:** dos ensamblados de **regresión** (EditMode y PlayMode), que siempre deben pasar, y uno de **experimentos** de física, cuyos resultados son datos y no un criterio de CI [directriz 10].

La pelota está detrás de `IBallSimulation`. La prueba de física evalúa primero PhysX (P1). La simulación propia (P2) es **condicional**: solo se hace si el PO la autoriza tras el gate único de física.

**Tech Stack:**
- Unity 6.3 LTS (6000.3.x, el parche que se fija en la Tarea 0.1), editor Apple silicon, con C#, URP, Input System (Enhanced Touch), UI Toolkit en runtime y Unity Test Framework;
- IL2CPP/ARM64;
- Xcode 27.1, `xcodebuild` y `xcrun devicectl`;
- git y GitHub (`gh`).

**Spec:** `docs/superpowers/specs/2026-10-20-squash-clash-design-v0.3.md` (v0.3 aprobada, con la enmienda E.0).

## Global Constraints

- iPhone primero, **solo vertical**, Unity + C#, macOS [D01]. Dispositivo de validación: **iPhone 16 Pro con iOS 26.6.2**.
- **Solo First Ball.** No se implementan golpes, gestos, planificador, IA, reglas de punto, marcador, saque, cansancio ni error. No se crean interfaces "para después" (§14, §15).
- **Una sola autoridad sobre el estado de la pelota** (§8.2).
- La predicción parte **del estado de simulación**, nunca de la posición interpolada para el render. Los parámetros físicos se aplican solo al iniciar un lanzamiento (§8.2.1, condiciones 1 y 3).
- Pelota con **radio físico de 20 mm** (§8.1). **Paso fijo de 120 Hz** [EXPERIMENTO] (§8.3.6). Tope de **4 impactos por paso** (§8.3.2; su aplicación a P1 está en la Tarea 4.2).
- **Ritmo global τ = 0,65** y **escala de lanzamiento λ = 1,0**. Se empieza ajustando solo τ (§8.4).
- **Cámara lenta solo en el panel.** Al usarla, las métricas de la sesión quedan marcadas como **no válidas** (§8.4).
- **Cancha WSF verificada** (§8.1, §8.7.2):
  - 9,75 × 6,40 m;
  - líneas de 50 mm, con las alturas medidas **hasta el borde inferior**: out del frontis a 4,570 m, línea de saque a 1,780 m y out trasero a 2,130 m;
  - **borde superior del tin a 0,480 m** (opción PSA: 0,430 m);
  - short line entre 5,440 y 5,490 m del frontis;
  - cuadros de saque de 1,600 m de lado interior.
- **Huella de contacto de 10 mm** para juzgar las líneas [EXPERIMENTO] (§8.7.3).
- **Sin backend.** El **panel queda excluido de las builds de producción** y está presente en las de desarrollo [D29].
- **Una sola fuente de valores por defecto: `DefaultPreset.json`** [directriz 11]. El catálogo de parámetros define tipos, rangos, opciones y clase de aplicación, pero **no** valores por defecto.
- **Presets locales en JSON versionado.** Al arrancar se carga el último preset activo. Cada registro de métricas guarda el preset, el hash de la **configuración efectiva**, el build y la semilla (§13.4) [H13].
- **No se usan:** ECS/DOTS, contenedores de inyección, buses globales de eventos, frameworks genéricos, motores de red ni aritmética determinista (anexo A).
- **Metas de rendimiento** (§17.C): 60 FPS, p95 ≤ 16,7 ms, p99 ≤ 25 ms, como máximo 1 frame por minuto por encima de 33 ms, 15 minutos sin estado térmico "serious" y predicción ≤ 1 ms.
- **Criterios de la prueba de física (§8.2.2, §17.A). No se relaja ninguno** sin una decisión explícita del PO:
  - 0 atravesamientos en 10 000 lanzamientos aleatorios;
  - la rapidez nunca aumenta tras un contacto en esquina, y no hay vibración;
  - consistencia y predicción **bit a bit** en el mismo binario y el mismo dispositivo;
  - predicción ≤ 1 ms en el iPhone.

  Los umbrales ≤ 1 cm / ≤ 2 ms rigen **solo para un predictor más grueso** (otro paso u otro código, §8.2.1 condición 2). Una divergencia de un predictor idéntico **no** lo convierte en "más grueso" [H05].
- **Idiomas:** la documentación va en español. Los identificadores, el código y los mensajes de commit van en inglés.
- **Codex no modifica la spec.** Cualquier discrepancia se reporta a Claude, que la presenta al PO.

## Review Focus

Son los cinco fallos más probables que ningún test de tarea cubre por sí solo. Cada uno tiene un test o una verificación asignada:

1. **La app pasa a segundo plano o se bloquea la pantalla en pleno vuelo.** Al volver, la pelota no salta ni se simulan segundos de golpe. La app queda en pausa y se puede continuar **también en producción**, sin el panel [H22]. → Tarea 3.1 (`Advance_WhilePaused_DiscardsTime`), Tarea 3.3 (`SystemPause_ShowsResumeOverlay_WithoutDevPanel`) y la comprobación en la build `prod` del Paso 4 de esa tarea.
2. **Un preset corrupto, de otro esquema, con claves duplicadas, valores no finitos u opciones inválidas, o una escritura interrumpida.** La app arranca igual, sobre una base conocida y con un aviso, y no se pierde el preset anterior [H14]. → Tareas 2.2 y 5.1.
3. **Un cambio de física en el panel con la pelota en el aire.** No afecta al vuelo actual. En el siguiente lanzamiento lo usan **todos** los consumidores: reloj, simulaciones, predicción y cancha [H11]. → Tarea 3.3 (`Launch_UsesEffectiveConfigInEveryConsumer`) y Tarea 5.1.
4. **La pelota rodando o quieta en el suelo al final de un vuelo, y luego relanzada.** No hay NaN ni vibración, y el relanzamiento es idéntico al de una instancia nueva [H05, H07]. → Tarea 4.1 (S10 y S10b) y criterios C2, C4 y C8 de la Tarea 4.2.
5. **Resultados de verificación falsos:** un `test.sh` "verde" sin tests ejecutados, un informe que da por cumplido un criterio no evaluado o una build de producción comprobada con artefactos viejos [H08, H15, H16]. → Tarea 1.3 (contrato de `test.sh`), Tarea 4.2 (`NotEvaluated` nunca cuenta como aprobado) y Tarea 3.3 (`check-release.sh` con carpetas limpias).

---

## Modelo de trabajo

### Incrementos y PR [directriz 12, H23]

Cada PR es **un incremento coherente y verificable**, no necesariamente una tarea. Se mantienen en PR separados: física, panel y cualquier cambio de decisión (gate, spec, valores por defecto).

| Incremento | Tareas | Rama | Qué ve el PO al cerrarlo |
|---|---|---|---|
| I1 Proyecto y primera instalación | 1.1–1.3 | `main` (excepción de bootstrap) | La app de arranque en el iPhone |
| I1r Remoto GitHub | 1.4 | — | El repo privado en GitHub |
| I2 Cancha y cámara fija | 2.1–2.3 | `firstball/i2-court` | La cancha |
| I3 Primera pelota | 3.1–3.3 | `firstball/i3-ball` | La pelota rebotando, con Lanzar, Pausa, Paso y Repetir |
| I4a Mediciones de física | 4.1–4.2 | `firstball/i4a-physics-measures` | Nada nuevo |
| I4b Ejecución de la prueba | 4.3–4.4 | `firstball/i4b-physics-run` | El botón "Prueba física" y los escenarios S1–S10 |
| I4c Decisión de física | Gate | `firstball/i4c-physics-decision` | El informe y la decisión |
| I5a Configuración y diagnóstico | 5.1–5.2 | `firstball/i5a-tuning-core` | Nada nuevo |
| I5b Panel | 5.3 | `firstball/i5b-panel` | El panel completo y sus presets |
| I6 Legibilidad | 6.1–6.3 | `firstball/i6-legibility` | Cámara de seguimiento, ayudas y L1–L6 |
| I7 Validación | 7.1–7.3 | `firstball/i7-validation` | La sesión de validación |

**Excepción de bootstrap:** el incremento I1 se hace directamente en `main`, porque el repo no existe antes y la primera prueba de compatibilidad no debe esperar al remoto. Claude lo revisa sobre el commit local. Desde I2, todo pasa por PR.

### Puntos de control y aprobaciones

- **Puntos de control del PO** (validación en el iPhone): al cerrar I1, I2, I3, I5b, I6 e I7. Mientras el PO valida, Codex puede seguir con el trabajo marcado como paralelo en la tabla siguiente, pero **no fusiona** un incremento cuyo punto de control previo no se haya cerrado.
- **Aprobaciones bloqueantes del PO** (Codex se detiene y pregunta):
  1. crear el repositorio remoto en GitHub (Tarea 1.4);
  2. instalar otra versión de Xcode (Tarea 1.3, solo si hace falta);
  3. la **decisión del gate único de física**, incluida cualquier autorización de P2;
  4. cualquier cambio de umbral, de criterio o de la spec;
  5. promover un preset calibrado a `DefaultPreset.json` (Tarea 7.3).
- **No requieren aprobación:** los ajustes internos de implementación dentro de una tarea, ni los cambios de valores por defecto de cámara o de lanzamientos de prueba que pida una tarea, siempre que se registren en el PR.

### Qué se puede hacer en paralelo

| Trabajo | Puede empezar cuando… | Bloqueado por |
|---|---|---|
| 2.1 Geometría y 2.2 Configuración | I1 fusionado | — |
| 3.1 Reloj y contrato | I1 fusionado (es Core puro) | — |
| 4.1–4.2 Mediciones de física | 3.1 fusionado | — |
| 5.1–5.2 Configuración y diagnóstico | 2.2 fusionado | — (no depende del gate) |
| 6.1–6.2 Cámara de seguimiento y ayudas | I3 fusionado | — |
| 4.4 Ejecución en el iPhone | 4.3 terminado | iPhone disponible |
| Gate de física | 4.3 y 4.4 con resultados válidos | Decisión del PO |
| 5.3 Panel | 5.1, 5.2 e I3 | — (la opción de backend muestra solo los autorizados) |
| 4.5 P2 | — | Autorización explícita del PO |
| 7.2 Validación integral | Gate decidido e I6 cerrado | — |

---

## Estructura de archivos

```
/  (raíz del repo = proyecto Unity)
  AGENTS.md  README.md  .gitignore  .gitattributes
  docs/  (existente)   docs/validation/environment.md   docs/validation/first-ball-validation.md
  scripts/  unity-env.sh  test.sh  spike.sh  build-ios.sh  install-ios.sh  check-release.sh  device.env.example
  Packages/manifest.json  Packages/packages-lock.json   (ambos versionados)
  Assets/SquashClash/
    Core/                    SquashClash.Core.asmdef (sin escena ni MonoBehaviours)
      Court/                 SurfaceId, CourtDimensions, LineId, LineBand, CourtLines, LineContactClassifier
      Config/                ApplyClass, ParamKind, ParamDef, ParamCatalog, ParamSet, PresetFile,
                             PresetCodec, EffectiveConfig, TuningState, PresetStore, LaunchPresets
      Simulation/            BallState, ContactEvent, SurfaceMaterial, PhysicsParams, IBallSimulation,
                             Trajectory, TrajectoryRunner, SimClock, LaunchSpec, Launcher, Ballistics
      Spike/                 PhysicsScenario, PhysicsScenarios, PhysicsMeasures, SpikeCriteria,
                             SpikeResult, SpikeRun, PredictionBenchmarkRun, SpikeReportWriter
      Diagnostics/           BuildInfo, FrameTimeStats, MetricsLog
    Runtime/                 SquashClash.Runtime.asmdef
      Physics/               SurfaceTag, PhysXVariant, PhysXBallSimulation, PhysXBallSimulationFactory
      Court/                 CourtView
      Ball/                  BallSimulationHost, BallView, LegibilityAids
      Camera/                CourtCameraRig
      App/                   AppBootstrap, ResumeOverlay, PlaceholderPlayers, ThermalMonitor,
                             FrameTimingSampler, AppPanelSettings.asset
      Resources/             DefaultPreset.json, BuildInfo.json (lo genera el build)
    Plugins/iOS/             SquashThermal.mm
    DevPanel/                SquashClash.DevPanel.asmdef (defineConstraints: SQUASH_DEV || UNITY_EDITOR)
                             DevPanelBootstrap, DevBar, ThreeFingerHold, DevPanelView, ParamSection,
                             PresetsSection, DiagnosticsSection, SpikeSection
    Editor/                  SquashClash.Editor.asmdef: ProjectSetup, BuildScript
    Scenes/Main.unity
    Tests/EditMode/          SquashClash.Core.Tests.asmdef        (regresión)
    Tests/PlayMode/          SquashClash.Runtime.Tests.asmdef     (regresión)
    Tests/Experiments/       SquashClash.Experiments.asmdef       (PlayMode; experimentos de física)
```

**Decisiones de estructura (de Claude):**
- **Ensamblados:** el anexo A pide dos (lógica e integración). Se agrega **DevPanel**, porque `defineConstraints` es la forma más simple de excluirlo de producción, y **Editor**, para que el código de build no entre al Player.
- **Experimentos en su propio ensamblado de tests,** para que `test.sh` ejecute solo regresión y `spike.sh` solo experimentos, sin depender de filtros por categoría [H16, directriz 10].
- **El panel no se referencia desde la escena.** Lo crea `DevPanelBootstrap` con `[RuntimeInitializeOnLoadMethod]`, así que en producción no quedan scripts faltantes.

**Convención de coordenadas** (la usan todas las tareas): origen en la esquina del suelo con el frontis y la pared izquierda, vista desde atrás. `x` va de 0 a 6,40 hacia la derecha, `y` es la altura y `z` va de 0 en el frontis a 9,75 en la pared trasera.

| Superficie | u | v | Normal interior |
|---|---|---|---|
| Frontis (z = 0) | x | y | (0, 0, 1) |
| Pared trasera (z = 9,75) | x | y | (0, 0, −1) |
| Lateral izquierda (x = 0) | z | y | (1, 0, 0) |
| Lateral derecha (x = 6,40) | z | y | (−1, 0, 0) |
| Suelo (y = 0) | x | z | (0, 1, 0) |

**Centro y punto de contacto [H04]:** un **punto de contacto** está sobre la superficie. El **centro** de la pelota en ese contacto es `punto + normal interior · r`. Los escenarios se construyen con puntos de contacto y se resuelven hacia el centro.

**Perfiles de build [H15, H17]:**

| Perfil | Define | Player | IL2CPP | Xcode | Uso |
|---|---|---|---|---|---|
| `dev` | `SQUASH_DEV` | no Development | Release | Release | Sesiones del PO y **todas las mediciones** |
| `prod` | ninguno | no Development | Release | Release | Comprobar la exclusión del panel |
| `debug` | `SQUASH_DEV` | Development | Debug | Debug | Solo para diagnosticar. Sus métricas no se comparan. |

Los defines se pasan con `BuildPlayerOptions.extraScriptingDefines`. No se modifican ni se restauran los defines persistentes del proyecto.

---

## Etapa 0 — Entorno y compatibilidad

**Qué hace Codex:** inventario, verificación de Unity, del iPhone y de la firma. No crea código.
**Cómo se verifica:** la salida de cada comando queda en `docs/validation/environment.md`.
**En el iPhone:** nada todavía.
**Para avanzar:** se cumplen las casillas de las Tareas 0.1 y 0.2. Lo que no se pueda verificar queda **explícito como pendiente**.

### Tarea 0.1: Unity 6.3 LTS (Apple silicon) con iOS

**Archivos:**
- Crear: `docs/validation/environment.md`

**Estado verificado el 2026-10-20 (Claude y Codex):** macOS 26.6.2 en arm64; Xcode 27.1 (27A9275) con SDK iOS 27.1; Unity Hub 3.22.3; git y gh con sesión de `rafaallendes-boop`; editor **6000.6.6f1** instalado, **sin iOS y fuera de la línea LTS**, que no se usa. Unity 6.3 LTS documenta iOS 15+ y Xcode 16 o posterior, pero no menciona Xcode 27. **La compatibilidad Unity 6.3 ↔ Xcode 27.1 ↔ iOS 26.6.2 está sin verificar** y se verifica en la Tarea 1.3.

- [ ] **Paso 1:** confirmar en https://unity.com/releases/unity-6/support la línea LTS vigente y su último parche. Si hay una LTS más nueva con soporte, se informa al PO antes de elegirla. Anotar la versión exacta (`6000.3.NNfN`) y su changeset.
- [ ] **Paso 2:** el **PO** inicia sesión en Unity Hub, activa la licencia Personal e instala desde la interfaz de Hub esa versión, **Apple silicon**, con el módulo **iOS Build Support** [H24]. La CLI de Hub no se usa: es obsoleta desde Hub 3.18 y la instalación es interactiva de todas formas.
- [ ] **Paso 3:** Codex verifica la instalación:

```bash
V=6000.3.NNfN; E="/Applications/Unity/Hub/Editor/$V"
file "$E/Unity.app/Contents/MacOS/Unity"            # esperado: arm64
ls -d "$E/PlaybackEngines/iOSSupport" "$E/Unity.app/Contents/PlaybackEngines/iOSSupport" 2>/dev/null
```
Esperado: binario `arm64` y **una** de las dos rutas de `iOSSupport`. Se anota la que existe.
- [ ] **Paso 4:** escribir `docs/validation/environment.md` con las versiones (Unity con su changeset y arquitectura), la tabla de compatibilidad con fuentes y la marca "VERIFICADO" o "PENDIENTE — Tarea 1.3", y el editor 6000.6.6f1 presente pero no usado.

### Tarea 0.2: iPhone y firma listos

**Archivos:**
- Crear: `scripts/device.env.example`. El `scripts/device.env` real queda en `.gitignore`.

- [ ] **Paso 1:** el **PO** conecta el iPhone 16 Pro por cable, confía en el Mac y activa el **Modo de desarrollador** (Ajustes → Privacidad y seguridad), lo que reinicia el teléfono.
- [ ] **Paso 2:** el **PO** agrega su Apple ID en **Xcode → Settings → Accounts** y comprueba que aparece su equipo. `-allowProvisioningUpdates` no sustituye esa cuenta [H24]. El PO entrega el **Team ID** y confirma el **bundle id** (propuesta: `cl.rafaallendes.squashclash`).
- [ ] **Paso 3:** obtener los identificadores **con la herramienta que los usa** [H24]:

```bash
xcrun devicectl list devices                                  # identificador de CoreDevice y estado
xcrun xctrace list devices                                    # UDID que acepta xcodebuild -destination id=
```
Esperado: el iPhone aparece `available (paired)` en `devicectl` y con iOS 26.6.2. Si `devicectl --device` acepta el UDID, se usa uno solo; si no, se guardan los dos.
- [ ] **Paso 4:** crear `scripts/device.env.example`:

```bash
UNITY_VERSION=6000.3.NNfN
DEVICE_UDID=00000000-0000000000000000   # destino de xcodebuild
DEVICE_ID=00000000-0000000000000000     # --device de devicectl (puede ser el mismo UDID)
TEAM_ID=XXXXXXXXXX
BUNDLE_ID=cl.rafaallendes.squashclash
```

---

## Etapa 1 — Proyecto Unity y primera instalación

**Qué hace Codex:** el repo local, el proyecto Unity, los scripts de pruebas y build, una escena de arranque y la instalación en el iPhone. También verifica una build de producción.
**Cómo se verifica:** `test.sh` cumple su contrato, y `build-ios.sh dev|prod` más `install-ios.sh` instalan y lanzan la app.
**En el iPhone:** fondo de color y, centrados, "Squash Clash — First Ball", la versión, el hash, el perfil (`dev` o `prod`) y los FPS. La app se queda en vertical aunque se gire el teléfono.
**Para avanzar (punto de control):** el PO confirma lo anterior en las dos builds. La combinación exacta que funcionó queda "VERIFICADO" en `environment.md`.

### Tarea 1.1: Repositorio local

**Archivos:**
- Crear: `.gitignore`, `.gitattributes`, `README.md` y `AGENTS.md`

- [ ] **Paso 1:** `git init -b main` en la raíz.
- [ ] **Paso 2:** `.gitignore` a partir de la plantilla oficial de Unity de GitHub (`Library/`, `Temp/`, `Obj/`, `Build/`, `Builds/`, `Logs/`, `UserSettings/`, `MemoryCaptures/`, `*.csproj`, `*.sln`, `.vs/`, `.idea/`), más `.DS_Store`, `.superpowers/` y `scripts/device.env`.
- [ ] **Paso 3:** `.gitattributes` con `* text=auto` y `*.cs`, `*.md`, `*.json`, `*.unity`, `*.asset`, `*.meta`, `*.prefab`, `*.mat` y `*.sh` como `text eol=lf`. Sin Git LFS.
- [ ] **Paso 4:** `README.md` con una línea del proyecto, los enlaces a la spec y al plan, y el uso de `scripts/test.sh`, `spike.sh`, `build-ios.sh` e `install-ios.sh`.
- [ ] **Paso 5:** `AGENTS.md` con las reglas para Codex: el "Modelo de trabajo" de este plan, TDD con `scripts/test.sh`, nada fuera del alcance de la tarea, no modificar `docs/superpowers/specs/`, no tocar umbrales ni escenarios para que algo pase, y documentación en español con código en inglés.
- [ ] **Paso 6:** `git status --porcelain | grep -c superpowers/brainstorm` imprime `0`.
- [ ] **Paso 7:** commit `chore: initialize repository with specs, reviews and plan`.

### Tarea 1.2: Resolución del editor y creación del proyecto [H01]

**Archivos:**
- Crear: `scripts/unity-env.sh`, el proyecto Unity (`Assets/`, `Packages/`, `ProjectSettings/`), los seis `.asmdef` del árbol, `Assets/SquashClash/Editor/ProjectSetup.cs`, `Assets/SquashClash/Scenes/Main.unity` y un test de humo por ensamblado de tests.

**Interfaces:**
- Produce: `scripts/unity-env.sh`, que **solo resuelve el editor**. Exporta (`set -a`) las variables de `device.env`, define `UNITY` y `ROOT`, y comprueba que el binario existe. **Solo si existe** `ProjectSettings/ProjectVersion.txt`, verifica que coincide con `UNITY_VERSION`.
- Produce: `ProjectSetup.Apply()`, idempotente e invocable con `-executeMethod SquashClash.EditorTools.ProjectSetup.Apply`.

- [ ] **Paso 1:** `unity-env.sh` según la interfaz. Todas las rutas van entre comillas.
- [ ] **Paso 2:** crear el proyecto en una carpeta temporal y mover `Assets/`, `Packages/` y `ProjectSettings/` a la raíz (Unity no crea proyectos sobre carpetas con contenido):

```bash
source scripts/unity-env.sh
"$UNITY" -batchmode -quit -createProject "$ROOT/.tmp-unity" -logFile "$ROOT/Logs/create.log"
mv "$ROOT/.tmp-unity/Assets" "$ROOT/.tmp-unity/Packages" "$ROOT/.tmp-unity/ProjectSettings" "$ROOT/" && rm -rf "$ROOT/.tmp-unity"
```
- [ ] **Paso 3:** agregar `com.unity.render-pipelines.universal`, `com.unity.inputsystem` y `com.unity.test-framework` con la versión que resuelve el editor fijado, y **versionar** `manifest.json` y `packages-lock.json` [H24].
- [ ] **Paso 4:** `ProjectSetup.Apply()` deja:
  - Asset Serialization `ForceText` y Visible Meta Files;
  - Active Input Handling `Input System Package (New)`;
  - URP con Pipeline Asset y Renderer asignados en Graphics y en Quality;
  - iOS: bundle id de la variable de entorno `BUNDLE_ID` (falla con un mensaje claro si no está), IL2CPP, ARM64, orientación `Portrait` sin autorrotación, Target minimum iOS por defecto del editor, **Frame Timing Stats activado** (para CPU y GPU, Tarea 5.2);
  - `Physics.simulationMode = SimulationMode.Script`;
  - `Physics.bounceThreshold = 0.1f` (el valor por defecto de 2 m/s anula los botes lentos);
  - `Physics.defaultContactOffset = 0.001f` (se mide su efecto en la Tarea 4.3; no equivale a un punto de impacto exacto);
  - Enhanced Determinism activado (variante base; la Tarea 4.3 lo compara);
  - `Main.unity` como única escena del build.
- [ ] **Paso 5:** asmdefs:
  - `SquashClash.Core`, sin referencias;
  - `SquashClash.Runtime` → Core, Unity.InputSystem;
  - `SquashClash.DevPanel` → Core, Runtime, Unity.InputSystem, con `defineConstraints: ["SQUASH_DEV || UNITY_EDITOR"]`. Así compila y se prueba en el editor, y en el Player solo entra si el build pasa `SQUASH_DEV`;
  - `SquashClash.Editor` → Core, Runtime, solo Editor;
  - `SquashClash.Core.Tests` → Core, solo Editor, TestAssemblies;
  - `SquashClash.Runtime.Tests` → Core, Runtime, TestAssemblies;
  - `SquashClash.Experiments` → Core, Runtime, TestAssemblies.
- [ ] **Paso 6:** aplicar con `"$UNITY" -batchmode -quit -projectPath "$ROOT" -executeMethod SquashClash.EditorTools.ProjectSetup.Apply -logFile "$ROOT/Logs/setup.log"` y comprobar que el log no contiene `error CS`.
- [ ] **Paso 7:** commit `chore: create Unity 6.3 LTS project with portrait iOS settings`.

### Tarea 1.3: Pruebas, build, arranque e instalación

**Archivos:**
- Crear: `scripts/test.sh`, `scripts/build-ios.sh`, `scripts/install-ios.sh`, `Assets/SquashClash/Editor/BuildScript.cs`, `Assets/SquashClash/Core/Diagnostics/BuildInfo.cs`, `Assets/SquashClash/Runtime/App/AppBootstrap.cs` y `Runtime/App/AppPanelSettings.asset`
- Modificar: `Main.unity`, con cámara y `AppBootstrap`
- Test: `Tests/EditMode/Diagnostics/BuildInfoTests.cs`

**Interfaces:**
- Produce: `[Serializable] public sealed class BuildInfo { public string version, gitHash, profile, builtAtUtc, unityVersion; public bool gitDirty; public static BuildInfo FromJson(string json); public string Label { get; } }`. `Label` da, por ejemplo, `0.1.0+a1b2c3d* dev` (el asterisco indica cambios sin commit).
- Produce: `BuildScript.BuildIos()`, invocable con `-executeMethod`, que lee el perfil de la variable `BUILD_PROFILE` (`dev|prod|debug`).
- Produce: `public sealed class AppBootstrap : MonoBehaviour { public static AppBootstrap Instance { get; } public BuildInfo Build { get; } }`. En `Awake` fija `Application.targetFrameRate = 60` y registra `[SquashClash] boot <Label> devpanel=<present|absent>`. La presencia del panel se detecta con `Type.GetType("SquashClash.DevPanel.DevPanelBootstrap, SquashClash.DevPanel") != null`.

- [ ] **Paso 1: contrato de `scripts/test.sh editmode|playmode [filtro]`** [H16]:
  - ejecuta Unity con `-runTests -testPlatform EditMode|PlayMode -assemblyNames <ensamblado de regresión> -testResults "$ROOT/Logs/<plataforma>-results.xml" -logFile "$ROOT/Logs/<plataforma>.log"` y `-testFilter` si se pasa (EditMode con `-nographics`);
  - **éxito** solo si se cumplen todas: el XML existe y es más nuevo que el inicio del script, `total ≥ 1`, `failed = 0`, y el log no contiene `error CS` ni `Scripts have compiler errors`;
  - si no, sale con código 1 e imprime el motivo;
  - siempre imprime `RESULT: total=N passed=P failed=F skipped=S`.
- [ ] **Paso 2:** test de humo y de `BuildInfo`:

```csharp
[Test] public void FromJson_ReadsAllFields_AndLabelMarksDirty() {
  var b = BuildInfo.FromJson("{\"version\":\"0.1.0\",\"gitHash\":\"a1b2c3d\",\"gitDirty\":true,\"profile\":\"dev\",\"builtAtUtc\":\"2026-10-20T00:00:00Z\",\"unityVersion\":\"6000.3.0f1\"}");
  Assert.AreEqual("0.1.0+a1b2c3d* dev", b.Label);
}
```
Además, `scripts/test.sh editmode` con un filtro que no coincide con nada **debe fallar** (`total=0`).
- [ ] **Paso 3: `BuildScript.BuildIos()`** [H15, H16]:
  1. borra la carpeta de salida `Builds/iOS-<perfil>`;
  2. escribe `Runtime/Resources/BuildInfo.json` (versión, `git rev-parse --short HEAD`, `gitDirty` según `git status --porcelain`, perfil, hora UTC y versión de Unity), lo importa con `AssetDatabase.ImportAsset(…, ForceSynchronousImport)` y comprueba que el `TextAsset` cargado tiene ese contenido;
  3. en `prod`, **falla** si los defines persistentes de iOS contienen `SQUASH_DEV`;
  4. compila con `BuildPlayerOptions` según la tabla de perfiles (`extraScriptingDefines`, `BuildOptions`) y la configuración IL2CPP correspondiente;
  5. si `report.summary.result != BuildResult.Succeeded`, sale con `EditorApplication.Exit(1)`.
- [ ] **Paso 4: `scripts/build-ios.sh dev|prod|debug`:** Unity en batch con `BUILD_PROFILE` exportado y `-executeMethod`; después `xcodebuild -project "<salida>/Unity-iPhone.xcodeproj" -scheme Unity-iPhone -configuration <Release|Debug> -destination "id=$DEVICE_UDID" -derivedDataPath "$ROOT/Builds/DerivedData-<perfil>" -allowProvisioningUpdates DEVELOPMENT_TEAM="$TEAM_ID" CODE_SIGN_STYLE=Automatic build`. Se detiene en el primer error, muestra la ruta del log e imprime la ruta del `.app`.
- [ ] **Paso 5: `scripts/install-ios.sh <perfil> [--console]`:** instalar y lanzar **sin esperar a que la app termine**. `--console` existe, pero mantiene la sesión hasta que la app se cierra [H24]. Por eso va en un modo aparte:

```bash
xcrun devicectl device install app --device "$DEVICE_ID" "$APP_PATH"
xcrun devicectl device process launch --device "$DEVICE_ID" --terminate-existing "$BUNDLE_ID"
# con --console: ... process launch --device "$DEVICE_ID" --console "$BUNDLE_ID"   (bloquea; se corta con Ctrl-C)
```
- [ ] **Paso 6:** `AppBootstrap` dibuja la etiqueta de arranque con un `UIDocument` que usa `AppPanelSettings.asset` (tema de runtime por defecto). El panel lo reutiliza.
- [ ] **Paso 7:** primera compilación desde la ruta real con espacios: `cd "/Users/rafaallendes/Projects/Squash Clash" && scripts/build-ios.sh dev && scripts/install-ios.sh dev --console`. Esperado: `** BUILD SUCCEEDED **` y en la consola `[SquashClash] boot … dev devpanel=absent` (el panel todavía no existe).
  - **Si falla,** primero se clasifica el fallo y se registra en `environment.md` [H24]: credenciales o firma, destino, exportación de Unity, compilación C++, enlace, o instalación y lanzamiento. Los fallos de firma o destino se corrigen en la Tarea 0.2 y **no** se tratan como incompatibilidad.
  - **Si es un fallo de Unity con Xcode 27.1,** se prueba en orden, sin dar nada por bueno sin instalarlo: (1) el último parche de 6000.3; (2) la solución que Unity documente para ese error concreto; (3) Xcode 26.x en paralelo, solo para compilar, **con la aprobación del PO**. No se aplican ajustes de linker genéricos.
  - **Si falla por la ruta con espacios,** se documenta y se usa el enlace `~/Projects/squash-clash`. No se renombra la carpeta sin el PO.
- [ ] **Paso 8:** `scripts/build-ios.sh prod && scripts/install-ios.sh prod`. Esperado: arranca y muestra el perfil `prod`.
- [ ] **Paso 9:** el PO confirma lo que ve en las dos builds. Codex marca la compatibilidad Unity–Xcode–iOS como "VERIFICADO" en `environment.md`, o deja lo que falte.
- [ ] **Paso 10:** commit `build: test/build/install scripts and boot scene verified on device`.

### Tarea 1.4: Repositorio remoto (requiere aprobación del PO)

- [ ] **Paso 1:** **Codex confirma con el PO justo antes de ejecutar**, porque es una acción hacia fuera:

```bash
gh repo create squash-clash --private --source . --push
gh repo view --json visibility,name
```
Esperado: `"visibility":"PRIVATE"`. El PO ve en GitHub `docs/` y no ve `.superpowers/`.

---

## Etapa 2 — Cancha y cámara fija básica

**Qué hace Codex:** la geometría WSF y el clasificador de líneas (Core), la base de configuración con `DefaultPreset.json` (Core) y la vista de la cancha con una cámara fija.
**Cómo se verifica:** tests de geometría, de bordes de línea y de configuración; un test de encuadre de la cámara.
**En el iPhone:** la cancha completa en vertical, vista desde detrás y por encima de la pared trasera: suelo de madera clara, paredes blancas, líneas rojas de 50 mm, pared trasera translúcida y dos cápsulas de referencia (azul para el humano y roja para el rival).
**Para avanzar (punto de control):** el PO reconoce la cancha y la proporción de las líneas.

### Tarea 2.1: Geometría de la cancha y clasificador de líneas

**Archivos:**
- Crear: `Core/Court/SurfaceId.cs`, `CourtDimensions.cs`, `LineId.cs`, `LineBand.cs`, `CourtLines.cs` y `LineContactClassifier.cs`
- Test: `Tests/EditMode/Court/CourtGeometryTests.cs` y `LineContactClassifierTests.cs`

**Interfaces:**
- Produce:

```csharp
public enum SurfaceId { Floor, FrontWall, LeftWall, RightWall, BackWall }
public sealed class CourtDimensions : IEquatable<CourtDimensions> {
  public float Length = 9.75f, Width = 6.40f, WallHeight = 6.0f, LineWidth = 0.05f;
  public float FrontOutLower = 4.57f, ServiceLower = 1.78f, BackOutLower = 2.13f, TinTop = 0.48f;
  public float ShortLineNearBackEdgeFromBack = 4.26f, ServiceBoxInner = 1.60f;
  public static CourtDimensions Standard();                 // TinTop 0.48
  public static CourtDimensions WithTinTop(float tinTop);   // 0.48 (WSF) o 0.43 (PSA)
  public Vector2 ToSurfaceUV(SurfaceId s, Vector3 p);
  public static Vector3 InwardNormal(SurfaceId s);
}
public enum LineId { FrontOut, Service, Tin, BackOut, LeftSideOut, RightSideOut,
                     ShortLine, HalfCourt, LeftBoxSide, LeftBoxBack, RightBoxSide, RightBoxBack }
public readonly struct LineBand {   // franja = {A + t·(B−A) + s·Growth | t∈[0,1], s∈[0,Width]}, Growth unitario y perpendicular a B−A
  public readonly LineId Id; public readonly SurfaceId Surface;
  public readonly Vector2 A, B, Growth; public readonly float Width;
}
public static class CourtLines { public static IReadOnlyList<LineBand> Build(CourtDimensions d); }
public static class LineContactClassifier {
  public const float EdgeEpsilon = 1e-6f;
  public static bool Touches(in LineBand band, Vector2 uv, float footprintRadius);   // distancia al paralelogramo ≤ r + EdgeEpsilon
  public static void Overlapping(IReadOnlyList<LineBand> bands, SurfaceId s, Vector2 uv, float r, List<LineId> result);
}
```
`WallHeight = 6,0 m` es un supuesto de modelado: paredes del juego sin techo.

**Franjas de `CourtLines.Build(Standard())`:**

| LineId | Superficie | A → B (uv) | Growth | Width |
|---|---|---|---|---|
| FrontOut | FrontWall | (0, 4.57) → (6.40, 4.57) | (0, 1) | 0.05 |
| Service | FrontWall | (0, 1.78) → (6.40, 1.78) | (0, 1) | 0.05 |
| Tin | FrontWall | (0, TinTop − 0.05) → (6.40, TinTop − 0.05) | (0, 1) | 0.05 |
| BackOut | BackWall | (0, 2.13) → (6.40, 2.13) | (0, 1) | 0.05 |
| LeftSideOut / RightSideOut | Left/RightWall | (0, 4.57) → (9.75, 2.13) | normal unitaria hacia arriba: (0.24278, 0.97012) | 0.05 |
| ShortLine | Floor | (0, 5.44) → (6.40, 5.44) | (0, 1) | 0.05 |
| HalfCourt | Floor | (3.175, 5.49) → (3.175, 9.75) | (1, 0) | 0.05 |
| LeftBoxSide | Floor | (1.60, 5.49) → (1.60, 7.14) | (1, 0) | 0.05 |
| LeftBoxBack | Floor | (0, 7.09) → (1.65, 7.09) | (0, 1) | 0.05 |
| RightBoxSide | Floor | (4.75, 5.49) → (4.75, 7.14) | (1, 0) | 0.05 |
| RightBoxBack | Floor | (4.75, 7.09) → (6.40, 7.09) | (0, 1) | 0.05 |

**Diagonal lateral [H18]:** el borde inferior une (0; 4,57) y (9,75; 2,13), y la franja tiene **50 mm de ancho perpendicular**. Crecer 50 mm en vertical daría unos 48,5 mm perpendiculares. La spec (§8.7.2) marca esta franja como interpretación de Claude ("crece 50 mm hacia arriba desde el borde inferior"). El plan la precisa como ancho perpendicular, sin cambiar la referencia del borde inferior, y la aclaración queda pendiente de registrar en la spec (ver la respuesta a la revisión, decisión D-6).

- [ ] **Paso 1: tests de geometría que fallan**

```csharp
[Test] public void Standard_UsesVerifiedWsfValues() {
  var d = CourtDimensions.Standard();
  Assert.AreEqual(9.75f, d.Length); Assert.AreEqual(6.40f, d.Width); Assert.AreEqual(0.05f, d.LineWidth);
  Assert.AreEqual(4.57f, d.FrontOutLower); Assert.AreEqual(1.78f, d.ServiceLower);
  Assert.AreEqual(2.13f, d.BackOutLower);  Assert.AreEqual(0.48f, d.TinTop);
  Assert.AreEqual(0.43f, CourtDimensions.WithTinTop(0.43f).TinTop);
}
[Test] public void ShortLine_SpansFrom5_44To5_49FromFrontWall() { /* A.y = 5.44 y A.y + Width = 5.49, tolerancia 1e-5 */ }
[Test] public void Build_Returns12Bands_WithUnitPerpendicularGrowth() { /* 12 franjas; |Growth| = 1 y Growth·(B−A) = 0, tolerancia 1e-5 */ }
[Test] public void SideOut_PerpendicularWidthIs50mm() { /* punto del borde inferior en z = 4.875 + Growth·0.05 → distancia normal al borde = 0.050 */ }
[Test] public void ToSurfaceUV_FollowsPlanConvention() { /* (1,2,3): FrontWall → (1,2); LeftWall → (3,2); Floor → (1,3) */ }
```
- [ ] **Paso 2:** `scripts/test.sh editmode CourtGeometryTests`. Esperado: FAIL.
- [ ] **Paso 3:** implementar `CourtDimensions`, `LineBand` y `CourtLines`.
- [ ] **Paso 4: tests de bordes que fallan** (§8.7.6), huella de 10 mm, un `[TestCase]` por fila:

| Superficie | uv | r | ¿Toca? | Línea |
|---|---|---|---|---|
| FrontWall | (3.2, 4.559) | 0.010 | no | FrontOut |
| FrontWall | (3.2, 4.560) | 0.010 | sí (borde exacto) | FrontOut |
| FrontWall | (3.2, 4.600) | 0.000 | sí | FrontOut |
| FrontWall | (3.2, 4.630) | 0.010 | sí (borde superior) | FrontOut |
| FrontWall | (3.2, 0.490) | 0.010 | sí | Tin |
| FrontWall | (3.2, 0.4911) | 0.010 | no | Tin |
| FrontWall | (3.2, 1.770) | 0.010 | sí | Service |
| FrontWall | (3.2, 1.8405) | 0.010 | no | Service |
| FrontWall | (3.2, 4.560) | 0.010 | no | Service (sin falsos positivos) |
| Floor | (1.0, 5.430) | 0.010 | sí | ShortLine |
| Floor | (1.0, 5.4289) | 0.010 | no | ShortLine |
| Floor | (1.0, 5.5011) | 0.010 | no | ShortLine |
| LeftWall | borde(4.875) − Growth·0.0100 | 0.010 | sí (tangente exacta) | LeftSideOut |
| LeftWall | borde(4.875) − Growth·0.0101 | 0.010 | no | LeftSideOut |
| LeftWall | borde(0.0) − Growth·0.0100 | 0.010 | sí (extremo del frontis) | LeftSideOut |

`borde(z)` es el punto del borde inferior de la diagonal en `u = z`: (z; 4,57 − 2,44·z/9,75). Las filas de la diagonal se generan en el test con esa expresión.
- [ ] **Paso 5:** ejecutar, ver que falla, implementar `LineContactClassifier` (distancia de un punto a un paralelogramo) y ejecutar hasta que pase.
- [ ] **Paso 6:** commit `feat(core): WSF court geometry and line footprint classifier`.

### Tarea 2.2: Base de configuración con una sola fuente de valores por defecto [H14, H20, directriz 11]

**Archivos:**
- Crear: `Core/Config/ApplyClass.cs`, `ParamKind.cs`, `ParamDef.cs`, `ParamCatalog.cs`, `ParamSet.cs`, `PresetFile.cs`, `PresetCodec.cs` y `EffectiveConfig.cs`, y `Runtime/Resources/DefaultPreset.json`
- Test: `Tests/EditMode/Config/ParamCatalogTests.cs`, `PresetCodecTests.cs` y `EffectiveConfigTests.cs`

**Interfaces:**
- Produce:

```csharp
public enum ApplyClass { Immediate, NextLaunch }   // "próximo rally" = próximo lanzamiento en First Ball (§13.4)
public enum ParamKind { Float, Int, Bool, Choice }
public readonly struct ChoiceOption { public readonly double Value; public readonly string Label; }
public sealed class ParamDef {          // sin valor por defecto: los valores viven solo en DefaultPreset.json
  public string Key, Label, Unit, Group; public ParamKind Kind;
  public double Min, Max, Step; public ChoiceOption[] Choices; public ApplyClass Apply;
  public bool OperativeFor(string backend);   // false si el backend no implementa el parámetro (ver Tarea 4.2)
}
public static class ParamCatalog { public static IReadOnlyList<ParamDef> All { get; } public static bool TryGet(string key, out ParamDef def); }
public sealed class ParamSet {          // inmutable; valores double
  public double Get(string key); public bool GetBool(string key); public int GetInt(string key);
  public ParamSet With(string key, double value);
  public IReadOnlyList<KeyValuePair<string, double>> Ordered { get; }   // por clave, orden ordinal
  public string Hash { get; }           // SHA-256 de "key=value\n" con value.ToString("R", InvariantCulture); primeros 12 hex
  public string HashOf(params string[] prefixes);
}
[Serializable] public sealed class PresetEntry { public string key; public double value; }
[Serializable] public sealed class PresetFile { public int schemaVersion = 1; public string name; public string createdBuild; public List<PresetEntry> values = new(); }
public static class PresetCodec {
  public const int SupportedSchema = 1;
  public static string ToJson(PresetFile f);
  public static bool TryParse(string json, out PresetFile f, out string error);   // nunca lanza
  public static ParamSet LoadDefault(string json);                                // estricto; lanza InvalidDataException con todos los problemas
  public static ParamSet Overlay(ParamSet baseSet, PresetFile f, List<string> warnings);   // parcial, sobre una base conocida
  public static PresetFile FromSet(ParamSet s, string name, string build);
}
public sealed class EffectiveConfig {   // instantánea inmutable de todo lo que usa un lanzamiento
  public ParamSet Source { get; } public string Hash { get; }              // = Source.Hash
  public string SimulationKey { get; }                                     // = Source.HashOf("physics.", "court.")
  public CourtDimensions Court { get; } public double Tempo { get; } public double LaunchScale { get; }
  public PhysicsParams Physics { get; } public string Backend { get; } public LaunchSpec Launch { get; }   // desde la Tarea 3.1 y 3.3
  public static EffectiveConfig From(ParamSet s);
}
```

**Reglas de `TryParse`:** JSON inválido, `schemaVersion ≠ 1` (no hay migraciones en First Ball) o **claves duplicadas** → `false` con un error que lo dice.

**Reglas de `LoadDefault` (estricta):** el archivo tiene **todas** las claves de `ParamCatalog`, ninguna desconocida, todos los valores finitos y en rango, los `Choice` con valores de su lista, y los `Int` y `Bool` enteros (los `Bool` en 0 o 1).

**Reglas de `Overlay` (parcial, sobre la base):**

| Entrada | Resultado | Aviso |
|---|---|---|
| Clave desconocida | se ignora | sí |
| Valor no finito | se ignora; queda el de la base | sí |
| Fuera de rango | se recorta al rango | sí |
| `Choice` fuera de la lista, `Int` o `Bool` no enteros | se ignora | sí |
| Clave ausente | queda el de la base | no |

**Claves de esta tarea** (cada tarea agrega las suyas al catálogo y a `DefaultPreset.json` en el mismo PR; el catálogo completo está en la Tarea 5.3):

| Clave | Tipo | Defecto | Rango u opciones | Clase |
|---|---|---|---|---|
| `court.tinTop` | Choice | 0.48 | 0.48 "WSF" / 0.43 "PSA" | NextLaunch |
| `camera.height` / `camera.backOffset` / `camera.lookAtHeight` | Float | 4.5 / 2.0 / 1.6 | 2–8 / 0–5 / 0–3 | Immediate |
| `camera.fovDeg` | Float | 62 | 40–90 | Immediate |
| `placeholders.visible` | Bool | 1 | — | Immediate |
| `human.x` / `human.z` / `rival.x` / `rival.z` | Float | 3.2 / 5.5 / 4.5 / 6.5 | 0.3–6.1 / 0.3–9.45 | Immediate |

- [ ] **Paso 1: tests que fallan**

```csharp
[Test] public void DefaultPreset_IsCompleteAndValid() { /* lee Assets/SquashClash/Runtime/Resources/DefaultPreset.json; LoadDefault no lanza */ }
[Test] public void LoadDefault_MissingKey_Throws_ListingKey() { /* quitar court.tinTop → InvalidDataException que menciona "court.tinTop" */ }
[Test] public void LoadDefault_UnknownKeyOrOutOfRange_Throws() { /* … */ }
[Test] public void TryParse_Malformed_ReturnsFalseWithoutThrowing() { Assert.IsFalse(PresetCodec.TryParse("{not json", out _, out var e)); Assert.IsNotEmpty(e); }
[Test] public void TryParse_OtherSchema_IsRejected() { /* schemaVersion 0 y 2 → false; el error menciona "schema" */ }
[Test] public void TryParse_DuplicateKeys_IsRejected() { /* camera.fovDeg dos veces → false; el error menciona la clave */ }
[Test] public void Overlay_FollowsTable() {
  // base = default; entradas: bogus.key=1, camera.fovDeg=1e999 (no finito), camera.height=99, court.tinTop=0.5
  // → avisos.Count == 4; fovDeg = 62; height = 8; tinTop = 0.48; el resto igual a la base
}
[Test] public void Hash_IndependentOfEntryOrder_And12Hex() { /* … */ }
[Test] public void Catalog_ChoiceAndBoolHaveExplicitValues() { /* todo Choice tiene ≥ 2 opciones con Value distinto; Bool sin Choices */ }
[Test] public void EffectiveConfig_SimulationKeyIgnoresCameraButNotCourt() {
  var d = Default(); var a = EffectiveConfig.From(d);
  Assert.AreEqual(a.SimulationKey, EffectiveConfig.From(d.With("camera.fovDeg", 70)).SimulationKey);
  Assert.AreNotEqual(a.SimulationKey, EffectiveConfig.From(d.With("court.tinTop", 0.43)).SimulationKey);
}
```
- [ ] **Paso 2:** ejecutar. Esperado: FAIL.
- [ ] **Paso 3:** implementar. `JsonUtility` para el JSON. La detección de duplicados se hace sobre `values` después de parsear. SHA-256 con `System.Security.Cryptography`.
- [ ] **Paso 4:** ejecutar. Esperado: PASS.
- [ ] **Paso 5:** commit `feat(core): parameter catalog, presets codec and DefaultPreset as single source`.

### Tarea 2.3: Vista de la cancha, cámara fija y figuras de referencia [H19, directriz 9]

**Archivos:**
- Crear: `Runtime/Court/CourtView.cs`, `Runtime/Physics/SurfaceTag.cs`, `Runtime/App/PlaceholderPlayers.cs` y `Runtime/Camera/CourtCameraRig.cs`
- Modificar: `AppBootstrap.cs` (carga `DefaultPreset.json` → `ParamSet` → `EffectiveConfig`) y `Main.unity`
- Test: `Tests/PlayMode/CourtViewTests.cs` y `CourtCameraRigTests.cs`

**Interfaces:**
- Consume: `CourtDimensions`, `CourtLines.Build`, `ParamSet` y `EffectiveConfig`.
- Produce: `public sealed class SurfaceTag : MonoBehaviour { public SurfaceId Surface; }`.
- Produce: `CourtView.Build(CourtDimensions d)`, que genera el suelo, cuatro paredes y 12 franjas como mallas desplazadas 1 mm hacia dentro. Llamarlo de nuevo reemplaza lo anterior. **Solo elementos visuales, sin colisionadores.**
- Produce: `PlaceholderPlayers.Apply(ParamSet p)`, con dos cápsulas de 1,80 m sin colisión.
- Produce: `public sealed class CourtCameraRig : MonoBehaviour { public void Apply(ParamSet p, CourtDimensions d); public void Tick(Vector3 ballRenderPos, float dt); }`. En esta tarea solo existe el modo **fijo**: posición (W/2, `camera.height`, L + `camera.backOffset`), mirando a (W/2, `camera.lookAtHeight`, 0), con FOV vertical `camera.fovDeg`. `Tick` no hace nada en modo fijo.
- Produce: `AppBootstrap.Params` (el `ParamSet` vigente) y `AppBootstrap.Config` (`EffectiveConfig`).

- [ ] **Paso 1: tests que fallan**

```csharp
[UnityTest] public IEnumerator Build_CreatesFiveSurfacesAndTwelveLines_NoColliders() { /* 5 SurfaceTag, 12 hijos en "Lines", 0 Collider */ }
[UnityTest] public IEnumerator Build_Twice_ReplacesPrevious() { /* sigue habiendo 12 líneas */ }
[UnityTest] public IEnumerator Fixed_DefaultFraming_ShowsFrontWallAndBackFloorCorners() {
  // aspecto 1206/2622 (iPhone 16 Pro en vertical); (0,0,0), (6.4,0,0), (0,4.57,0), (6.4,4.57,0), (0,0,9.75), (6.4,0,9.75):
  // WorldToViewportPoint ∈ [0,1]² y z > 0
}
```
Si el encuadre por defecto no cumple, se ajustan **solo** los valores `camera.*` de `DefaultPreset.json`, y el cambio se registra en el PR.
- [ ] **Paso 2:** ejecutar, ver que falla e implementar con materiales URP Unlit: suelo de madera clara, paredes blancas, **todas las líneas de pared de un mismo rojo y todas las del suelo de un mismo rojo** (cláusula WSF 5.01.01) y pared trasera translúcida con alfa 0,25 [EXPERIMENTO]. Ejecutar hasta que pase.
- [ ] **Paso 3:** colocar en `Main.unity`. Compilar e instalar `dev`.
- [ ] **Paso 4:** commit `feat(runtime): court view, fixed camera and placeholder players`.

---

## Etapa 3 — Primera pelota visible

**Qué hace Codex:** el reloj y el contrato de simulación (Core), el adaptador PhysX (P1) con su ciclo de vida, el host de la pelota con configuración efectiva por lanzamiento, una barra mínima de desarrollo y la pausa del sistema. Además, la primera comprobación de que el panel no está en producción.
**Cómo se verifica:** tests de regresión del reloj, del contrato, del ciclo de vida de PhysX y del host; `check-release.sh`.
**En el iPhone:** en la build `dev`, una barra con **Lanzar**, **Pausa/Continuar**, **Paso** y **Repetir**. La pelota sale del lanzador por defecto, rebota en el frontis y el suelo y se detiene en el segundo bote. Al bloquear y desbloquear el teléfono, la app aparece en pausa con "Toca para continuar". En la build `prod` no hay barra ni panel.
**Para avanzar (punto de control):** el PO ve la pelota rebotar y puede pausar, avanzar y repetir. `RELEASE-CHECK: OK`.

### Tarea 3.1: Reloj, contrato de simulación y balística [H02, H03, H22]

**Archivos:**
- Crear: `Core/Simulation/BallState.cs`, `ContactEvent.cs`, `SurfaceMaterial.cs`, `PhysicsParams.cs`, `IBallSimulation.cs`, `Trajectory.cs`, `TrajectoryRunner.cs`, `SimClock.cs`, `LaunchSpec.cs`, `Launcher.cs` y `Ballistics.cs`
- Modificar: `ParamCatalog`, `DefaultPreset.json` y `EffectiveConfig` (claves `pace.*` y `physics.gravity`, `physics.stepHz`, `physics.maxImpactsPerStep`)
- Test: `Tests/EditMode/Simulation/SimClockTests.cs`, `BallisticsTests.cs` y `TrajectoryRunnerTests.cs`

**Interfaces:**
- Produce:

```csharp
public struct BallState { public Vector3 Position; public Vector3 Velocity; public long Step; }
public enum ContactTiming { StepOnly, Estimated, Exact }
public readonly struct ContactEvent {
  public readonly SurfaceId Surface; public readonly Vector3 Point; public readonly Vector2 UV; public readonly Vector3 Normal;
  public readonly long Step;
  public readonly float TimeInStep;          // segundos desde el inicio del paso; NaN si Timing == StepOnly
  public readonly ContactTiming Timing;      // qué tan medido es el instante
  public readonly Vector3 ApproachVelocity;  // velocidad de aproximación observada en el contacto
  public readonly bool Persistent;           // true = muestra de contacto persistente, no un impacto nuevo
}
public sealed class SurfaceMaterial : IEquatable<SurfaceMaterial> { public float Restitution; public float TangentialFriction; }
public sealed class PhysicsParams : IEquatable<PhysicsParams> {
  public float Gravity, BallRadius = 0.020f, BallMass = 0.024f; public int StepHz, MaxImpactsPerStep;
  public SurfaceMaterial Floor, FrontWall, SideWalls, BackWall;
  public double StepSeconds => 1.0 / StepHz;
}
public sealed class BackendCapabilities { public ContactTiming Timing; public bool ImpactCapConfigurable; public bool RunsInEditMode; }
public interface IBallSimulation : IDisposable {
  BackendCapabilities Capabilities { get; }
  void Reset(in BallState initial);   // contrato: equivale a una instancia nueva con Reset(initial) (se mide, Tarea 4.2 C8)
  BallState State { get; }
  void Step(List<ContactEvent> contactsThisStep);   // avanza exactamente un paso fijo
}
public interface IBallSimulationFactory {
  string Name { get; }
  IBallSimulation Create(PhysicsParams p, CourtDimensions d);   // parámetros fijos durante la vida de la instancia
  int LiveInstances { get; }
}
public enum StopCondition { SecondFloorBounce, MaxSteps }
public enum TrajectoryExit { None, OverTop }
public sealed class Trajectory {
  public readonly List<BallState> States = new();      // States[0] = inicio; States[i] = fin del paso i
  public readonly List<ContactEvent> Contacts = new();
  public TrajectoryExit Exit;
}
public static class TrajectoryRunner {
  public static Trajectory Run(IBallSimulation sim, in BallState from, int maxSteps, StopCondition stop, CourtDimensions d);
}
public sealed class SimClock {
  public const int MaxStepsPerFrame = 8;
  public SimClock(int stepHz, double tempo);
  public int StepHz { get; } public double Tempo { get; }
  public void Configure(int stepHz, double tempo);   // solo al lanzar; reinicia la fracción acumulada
  public void Restart();                             // fracción = 0 y DroppedSinceRestart = 0
  public double DebugSlowMotion { get; set; }        // 1 = apagada; < 1 activa MetricsInvalid
  public bool Paused { get; set; }
  public bool MetricsInvalid { get; }                // pegajoso durante la sesión
  public long DroppedSteps { get; } public long DroppedSinceRestart { get; }
  public float Alpha { get; }                        // fracción pendiente en [0,1), para interpolar el render
  public void RequestSingleStep();
  public int Advance(double realDeltaSeconds);       // pasos que se ejecutan en este frame
}
public readonly struct LaunchSpec { public readonly Vector3 Position, Direction; public readonly float SpeedRealMps; }
public static class Launcher { public static BallState ToState(in LaunchSpec spec, double launchScale); }   // v = dir.normalized·speed·λ
public static class Ballistics {   // tiro parabólico sin colisiones, en double
  public static Vector3 PositionAt(in BallState s, float gravity, double t);
  public static Vector3 VelocityAt(in BallState s, float gravity, double t);
  public static Vector3 SolveCenterTo(Vector3 from, Vector3 center, float horizontalSpeed, float gravity);
  public static Vector3 CenterForContact(SurfaceId s, Vector3 contactPoint, float radius);   // punto + normal interior · r
  public static bool FirstPlaneHit(in BallState s, float gravity, float radius, CourtDimensions d, double maxT,
                                   out SurfaceId surface, out double t);   // primer instante en que la esfera toca un plano
}
```

**Algoritmo de `Advance`:** `acc += realDelta · Tempo · DebugSlowMotion · StepHz`, todo en `double`. `steps = floor(acc + 1e-9)` y `acc −= steps`. Si `steps > MaxStepsPerFrame`, el exceso se suma a `DroppedSteps` y `DroppedSinceRestart` y se ejecutan `MaxStepsPerFrame`. **En pausa, el tiempo real se descarta** (no se acumula ni cuenta como descartado). Si hay un paso pedido, devuelve 1. El margen de `1e-9` es ruido numérico de `double` y no depende de ningún valor concreto de τ: τ llega como `double` desde `ParamSet`.

**Claves de esta tarea:**

| Clave | Tipo | Defecto | Rango u opciones | Clase |
|---|---|---|---|---|
| `pace.tempo` (τ) | Float | 0.65 | 0.3–1.0 | NextLaunch |
| `pace.launchScale` (λ) | Float | 1.0 | 0.5–1.5 | NextLaunch |
| `physics.gravity` | Float | 9.81 | 5–15 | NextLaunch |
| `physics.stepHz` | Choice | 120 | 60 / 120 / 240 | NextLaunch |
| `physics.maxImpactsPerStep` | Int | 4 | 1–8 | NextLaunch |

- [ ] **Paso 1: tests que fallan**

```csharp
[Test] public void Advance_Tempo1_SixtiethSecond_TwoSteps() => Assert.AreEqual(2, new SimClock(120, 1.0).Advance(1.0 / 60));
[Test] public void Advance_Tempo065_OneSecondOver60Frames_Totals78_NeverOverCap() {
  var c = new SimClock(120, 0.65); int total = 0;
  for (int i = 0; i < 60; i++) { int n = c.Advance(1.0 / 60); Assert.LessOrEqual(n, SimClock.MaxStepsPerFrame); total += n; }
  Assert.AreEqual(78, total); Assert.AreEqual(0, c.DroppedSteps);
}
[Test] public void Advance_Tempo065_TenMinutesAt60fps_NoDrift() { /* 36 000 frames de 1/60 → 46 800 pasos exactos */ }
[Test] public void Advance_AccumulatesFractions() { /* tempo 1: 0.004 → 0, 0.004 → 0, 0.004 → 1 */ }
[Test] public void Advance_HugeDelta_CapsAndCountsDropped() {
  var c = new SimClock(120, 1.0); Assert.AreEqual(8, c.Advance(5.0)); Assert.AreEqual(592, c.DroppedSteps); Assert.AreEqual(592, c.DroppedSinceRestart);
}
[Test] public void Advance_WhilePaused_DiscardsTime() {   // Review Focus 1
  var c = new SimClock(120, 1.0) { Paused = true }; Assert.AreEqual(0, c.Advance(30.0)); Assert.AreEqual(0, c.DroppedSteps);
  c.Paused = false; Assert.AreEqual(2, c.Advance(1.0 / 60));
}
[Test] public void Paused_SingleStep_RunsExactlyOne() { /* RequestSingleStep → 1, después 0 */ }
[Test] public void Configure_ChangesHz_AndRestartClearsFraction() { /* Configure(240, 1) → Advance(1/60) = 4; Restart() → Alpha = 0 y DroppedSinceRestart = 0 */ }
[Test] public void SlowMotion_MarksMetricsInvalid_Sticky() { /* 0.5 → true; volver a 1 → sigue true */ }
[Test] public void ToState_AppliesLaunchScale() { /* dir (0,0,−2), 10 m/s, λ 1.2 → v (0,0,−12), Step 0 */ }
[Test] public void SolveCenterTo_ReachesCenterAnalytically() { /* error < 1e-4 m en el instante horizontal previsto */ }
[Test] public void CenterForContact_OffsetsAlongInwardNormal() { /* FrontWall (3.2, 4.56, 0) → (3.2, 4.56, 0.02) */ }
[Test] public void FirstPlaneHit_FrontWallContactMatchesTarget() {
  // H04: desde (3.2, 1, 5) a 15 m/s horizontales hacia el centro CenterForContact(FrontWall, (3.2, 4.560, 0), 0.02)
  // → surface FrontWall y PositionAt(t) − n·r a menos de 1e-5 m de (3.2, 4.560, 0)
}
```
`TrajectoryRunnerTests` usa una simulación falsa definida en el test, que rebota en el suelo cada 10 pasos: `Run(..., SecondFloorBounce)` se detiene en el paso 20 con dos contactos, `States.Count == 21`, y los contactos `Persistent` no cuentan como botes.
- [ ] **Paso 2:** ejecutar. Esperado: FAIL.
- [ ] **Paso 3:** implementar.
- [ ] **Paso 4:** ejecutar. Esperado: PASS.
- [ ] **Paso 5:** commit `feat(core): sim clock, simulation contract and ballistics`.

### Tarea 3.2: Adaptador PhysX (P1) con ciclo de vida verificable [H03, H05, H09]

**Archivos:**
- Crear: `Runtime/Physics/PhysXVariant.cs`, `PhysXBallSimulation.cs` y `PhysXBallSimulationFactory.cs`
- Modificar: `ParamCatalog`, `DefaultPreset.json` y `EffectiveConfig` (materiales y backend)
- Test: `Tests/PlayMode/PhysXLifecycleTests.cs` (regresión)

**Interfaces:**
- Consume: lo producido por la Tarea 3.1, `CourtDimensions` y `SurfaceTag`.
- Produce:

```csharp
public sealed class PhysXVariant {   // ajustes documentados de PhysX que la Tarea 4.3 compara
  public CollisionDetectionMode Ccd = CollisionDetectionMode.ContinuousDynamic;
  public float ContactOffset = 0.001f; public int SolverIterations = 6; public int SolverVelocityIterations = 1;
  public string Name { get; }        // p. ej. "ccdDyn-co1mm-it6-1"; Enhanced Determinism se registra aparte (es del proyecto)
  public static PhysXVariant Baseline { get; }
}
public sealed class PhysXBallSimulationFactory : IBallSimulationFactory {
  public PhysXBallSimulationFactory(PhysXVariant variant = null);
  public string Name => "P1-PhysX";
  public int LiveInstances { get; } public int PendingUnloads { get; }
}
```

**Diseño:**
- **Escena propia por instancia:** `SceneManager.CreateScene(nombre, new CreateSceneParameters(LocalPhysicsMode.Physics3D))`. Cada objeto creado se mueve explícitamente con `SceneManager.MoveGameObjectToScene` [H09]. La instancia avanza con `scene.GetPhysicsScene().Simulate((float)p.StepSeconds)`.
- **Gravedad:** `Physics.gravity` es global en Unity. La fábrica la fija en `Create` y **lanza `InvalidOperationException`** si ya hay instancias vivas con otra gravedad. El host destruye las instancias antes de crear otras con otros parámetros. Los tests guardan y restauran `Physics.gravity`.
- **Colisionadores:** cinco `BoxCollider` de 1 m de espesor fuera de los planos (caras interiores en x = 0, x = 6,40, y = 0, z = 0 y z = 9,75), con altura `WallHeight`, `SurfaceTag` y `PhysicsMaterial` (`bounciness = Restitution`, `dynamic/staticFriction = TangentialFriction`, combinación `Multiply`). La pelota tiene `bounciness 1` y `friction 1`. `contactOffset` según la variante.
- **Pelota:** `SphereCollider` de radio 0,020 y `Rigidbody` de masa 0,024, `linearDamping = 0`, `angularDamping = 0`, `FreezeRotation` (sin spin, §8.3.4), detección según la variante, sin interpolación y `sleepThreshold = 0` (que no se duerma a mitad de una medición).
- **Contactos:** un `MonoBehaviour` en la pelota registra:
  - `OnCollisionEnter` → impacto: superficie, `GetContact(0).point`, normal, UV y `ApproachVelocity = −relativeVelocity` (las paredes son estáticas). El instante es `Estimated`: se calcula con `Ballistics.FirstPlaneHit` desde el estado al inicio del paso; si no cae dentro del paso (contacto anticipado por `contactOffset`), queda `StepOnly` con `NaN`;
  - `OnCollisionStay` → `Persistent = true`, para que la vibración y los contactos persistentes **no desaparezcan del diagnóstico** [H07].
- **Capacidades:** `Timing = Estimated`, `ImpactCapConfigurable = false`, `RunsInEditMode = false`.
- **Reset:** fija posición y velocidad, anula la velocidad angular y despierta el cuerpo. Si la Tarea 4.3 muestra que esto no equivale a una instancia nueva, la corrección del adaptador (por ejemplo, recrear el Rigidbody) va en un PR de física y se vuelve a medir.
- **Dispose:** destruye los materiales creados y descarga la escena con `UnloadSceneAsync`. `PendingUnloads` llega a 0 cuando terminan las descargas.

**Claves de esta tarea:**

| Clave | Tipo | Defecto | Rango u opciones | Clase |
|---|---|---|---|---|
| `physics.floor.restitution` / `.friction` | Float | 0.45 / 0.30 | 0–1 | NextLaunch |
| `physics.front.restitution` / `.friction` | Float | 0.55 / 0.20 | 0–1 | NextLaunch |
| `physics.side.restitution` / `.friction` | Float | 0.55 / 0.20 | 0–1 | NextLaunch |
| `physics.back.restitution` / `.friction` | Float | 0.55 / 0.20 | 0–1 | NextLaunch |
| `physics.backend` | Choice | 0 | 0 "P1-PhysX" (P2 se agrega solo si se autoriza) | NextLaunch |

La restitución y la fricción son puntos de partida de Claude, no datos medidos [EXPERIMENTO].

- [ ] **Paso 1: tests de regresión que fallan**

```csharp
[UnityTest] public IEnumerator Drop_HitsFloor_WithSurfaceUVAndEstimatedTime() { /* desde y = 1 → un ContactEvent Floor no persistente, UV = (x, z) ± 1 mm */ }
[UnityTest] public IEnumerator Create_PutsAllObjectsInOwnScene() { /* todos los GameObjects de la instancia están en su escena; ninguno en la activa */ }
[UnityTest] public IEnumerator Instances_DoNotInterfere() { /* 100 pasos en A no cambian el State de B, bit a bit */ }
[UnityTest] public IEnumerator Create_WithOtherGravityWhileAlive_Throws() { /* … */ }
[UnityTest] public IEnumerator Dispose_UnloadsScenesAndMaterials() {
  // crear y destruir 20 instancias; esperar PendingUnloads == 0 → sceneCount igual al inicial y LiveInstances == 0
}
[UnityTest] public IEnumerator Materials_FollowParams() { /* front.restitution 0.3 → bounciness 0.3 en el collider del frontis */ }
```
- [ ] **Paso 2:** ejecutar `scripts/test.sh playmode PhysXLifecycleTests`. Esperado: FAIL.
- [ ] **Paso 3:** implementar según el diseño.
- [ ] **Paso 4:** ejecutar. Esperado: PASS.
- [ ] **Paso 5:** commit `feat(runtime): PhysX ball simulation (P1) with isolated scenes`.

### Tarea 3.3: Host de la pelota, barra mínima, pausa del sistema y exclusión del panel [H11, H15, H22]

**Archivos:**
- Crear: `Runtime/Ball/BallSimulationHost.cs`, `BallView.cs`, `Runtime/App/ResumeOverlay.cs`, `DevPanel/DevPanelBootstrap.cs`, `DevPanel/DevBar.cs` y `scripts/check-release.sh`
- Modificar: `AppBootstrap.cs`, `ParamCatalog`, `DefaultPreset.json`, `EffectiveConfig` (lanzador) y `Main.unity`
- Test: `Tests/PlayMode/BallSimulationHostTests.cs` y `AppBootstrapTests.cs`

**Interfaces:**
- Consume: `IBallSimulationFactory`, `SimClock`, `TrajectoryRunner`, `Launcher`, `EffectiveConfig`, `CourtView` y `CourtCameraRig`.
- Produce:

```csharp
public sealed class BallSimulationHost : MonoBehaviour {
  public void Initialize(IReadOnlyList<IBallSimulationFactory> backends, SimClock clock);
  public void Launch(EffectiveConfig cfg);   // ver el orden abajo
  public void Replay();                      // Launch(ActiveConfig)
  public EffectiveConfig ActiveConfig { get; }
  public int SimulationGeneration { get; }   // sube cada vez que se recrean las simulaciones
  public BallState Current { get; }          // estado de simulación (autoridad)
  public Vector3 RenderPosition { get; }     // interpolado: SOLO para dibujar
  public Trajectory Prediction { get; }      // calculada al lanzar, desde el estado de simulación
  public bool PredictionMatchesBitwise { get; }   // comparación paso a paso con Prediction
  public event Action<ContactEvent> ContactRaised;
  public event Action<EffectiveConfig> Launched;
}
```

**Orden de `Launch(cfg)`** (configuración efectiva por lanzamiento):
1. Si `cfg.SimulationKey` cambió: `Dispose` de las simulaciones de juego y de predicción, y `Create` de las dos con `cfg.Physics` y `cfg.Court` en la fábrica `cfg.Backend`. Sube `SimulationGeneration`.
2. `clock.Configure(cfg.Physics.StepHz, cfg.Tempo)` y `clock.Restart()`.
3. `s = Launcher.ToState(cfg.Launch, cfg.LaunchScale)`; `game.Reset(s)` y `predictor.Reset(s)`.
4. `Prediction = TrajectoryRunner.Run(predictor, s, …, SecondFloorBounce, cfg.Court)`.
5. Estado previo y actual = `s`, para que el primer frame no interpole desde el vuelo anterior.
6. `ActiveConfig = cfg` y se dispara `Launched(cfg)`. `AppBootstrap` reconstruye `CourtView` si cambió `cfg.Court`.

`Update` llama a `clock.Advance(Time.unscaledDeltaTime)`, ejecuta N pasos en el juego, compara cada estado con `Prediction.States[step]` y calcula `RenderPosition = Lerp(previo, actual, clock.Alpha)`. `BallView` dibuja una esfera del radio físico en `RenderPosition`.

**Pausa del sistema (también en producción):** `OnApplicationPause(true)` o `OnApplicationFocus(false)` → `Clock.Paused = true`. Al volver, `ResumeOverlay` (Runtime, no DevPanel) muestra "Toca para continuar" y un toque quita la pausa. La pausa manual de la barra no muestra la capa.

**Barra mínima (DevPanel):** `DevPanelBootstrap` (`[RuntimeInitializeOnLoadMethod(AfterSceneLoad)]`) crea `DevBar` en la parte inferior del `Screen.safeArea`, con **Lanzar** (`Launch(EffectiveConfig.From(Params))`), **Pausa/Continuar**, **Paso** (`RequestSingleStep`) y **Repetir** (`Replay`).

**Claves de esta tarea:**

| Clave | Tipo | Defecto | Rango | Clase |
|---|---|---|---|---|
| `launcher.x` / `.y` / `.z` | Float | 3.2 / 1.0 / 7.0 | 0.3–6.1 / 0.1–5.0 / 0.3–9.45 | NextLaunch |
| `launcher.yawDeg` / `.pitchDeg` | Float | 0 / 10 | −80–80 / −30–80 | NextLaunch |
| `launcher.speed` | Float | 15 | 2–40 m/s | NextLaunch |

- [ ] **Paso 1: tests que fallan**

```csharp
[UnityTest] public IEnumerator Launch_PredictionStartsFromSimulationState() { /* Prediction.States[0] bit a bit igual a Current tras Launch */ }
[UnityTest] public IEnumerator Launch_UsesEffectiveConfigInEveryConsumer() {   // Review Focus 3
  // cfg2 = cfg1 con physics.floor.restitution 0.6, physics.stepHz 240, pace.tempo 0.8 y court.tinTop 0.43; Launch(cfg2) →
  // SimulationGeneration sube; clock.StepHz == 240; clock.Tempo == 0.8; ActiveConfig.Physics.Floor.Restitution == 0.6;
  // el collider del suelo de la simulación de juego Y de la de predicción tiene bounciness 0.6; CourtView usa TinTop 0.43
}
[UnityTest] public IEnumerator Launch_SameSimulationKey_KeepsInstances() { /* cambiar solo camera.fovDeg → SimulationGeneration igual */ }
[UnityTest] public IEnumerator Relaunch_ClearsInterpolationAndFraction() { /* tras Launch, RenderPosition == posición inicial y clock.Alpha == 0 */ }
[UnityTest] public IEnumerator SystemPause_ShowsResumeOverlay_WithoutDevPanel() {   // Review Focus 1
  // typeof(ResumeOverlay) está en SquashClash.Runtime, no en DevPanel;
  // SendMessage("OnApplicationPause", true) → Clock.Paused y la capa visible; toque simulado → Paused == false
}
```
El test que compara la pelota con su predicción durante el vuelo **no** es de regresión: es el criterio C6 de la prueba de física (Tarea 4.2), cuyo resultado es un dato.
- [ ] **Paso 2:** ejecutar, ver que falla, implementar y ejecutar hasta que pase.
- [ ] **Paso 3: `scripts/check-release.sh`** [H15]: compila `dev` y `prod` en carpetas vacías (sin `xcodebuild`) y comprueba:

```bash
start=$(date +%s)
# … BUILD_PROFILE=dev y prod con BuildScript …
for p in dev prod; do test "$(stat -f %m "Builds/iOS-$p/Il2CppOutputProject")" -ge "$start" || { echo "STALE $p"; exit 1; }; done
grep -rqE "SquashClash[._]DevPanel" "Builds/iOS-dev/Il2CppOutputProject"  || { echo "DEV WITHOUT PANEL"; exit 1; }
! grep -rqE "SquashClash[._]DevPanel" "Builds/iOS-prod/Il2CppOutputProject" || { echo "PROD HAS PANEL"; exit 1; }
echo "RELEASE-CHECK: OK"
```
Además, la comprobación funcional: en el iPhone, `prod` registra `devpanel=absent` y `dev` registra `devpanel=present`.
- [ ] **Paso 4:** compilar e instalar `dev` y `prod`. En `dev`: Lanzar, Pausa, Paso y Repetir; bloquear y desbloquear el teléfono en pleno vuelo y ver "Toca para continuar". En `prod`: sin barra.
- [ ] **Paso 5:** commit `feat(runtime): ball host with effective config, dev bar and system pause`.

---

## Etapa 4 — Prueba técnica de física corregida y gate único

**Qué hace Codex:** escenarios bien construidos, mediciones separadas por fenómeno, criterios trazables a la spec, ejecución incremental en el editor y en el iPhone, y los informes.
**Cómo se verifica:** las mediciones se prueban en EditMode con trayectorias sintéticas (regresión). La prueba sobre PhysX es un **experimento**: `scripts/spike.sh` y el botón del iPhone producen informes, no aprobaciones de CI.
**En el iPhone:** en la barra, **Escenario** (S1–S10, para ver cada uno) y **Prueba física**, con progreso, cancelación y el informe copiado al portapapeles.
**Para avanzar:** el **gate único de física**, al final de la etapa.

### Tarea 4.1: Escenarios S1–S10 con comprobación de construcción [H04, H08]

**Archivos:**
- Crear: `Core/Spike/PhysicsScenario.cs` y `PhysicsScenarios.cs`
- Test: `Tests/EditMode/Spike/PhysicsScenariosTests.cs` (regresión)

**Interfaces:**
- Consume: `Ballistics`, `CourtDimensions`, `PhysicsParams` y `LineContactClassifier`.
- Produce:

```csharp
public sealed class PhysicsScenario {
  public string Id, Description; public BallState Start; public int MaxSteps; public StopCondition Stop;
  public Vector3? ContactPoint; public SurfaceId? ContactSurface;    // punto de contacto previsto, sobre la superficie
  public LineId? Line; public bool? ExpectTouch;                       // solo S9
  public int MinContactsBeforeStop;                                    // lo que el escenario debe producir para ser evaluable
}
public static class PhysicsScenarios {
  public static IReadOnlyList<PhysicsScenario> Fixed(CourtDimensions d, PhysicsParams p);           // S1–S7, S9a–S9i, S10
  public static IEnumerable<PhysicsScenario> Random(CourtDimensions d, PhysicsParams p, int count, int seed);   // S8
  public static IReadOnlyList<PhysicsScenario> EdgeDirected(CourtDimensions d, PhysicsParams p);    // S8d
}
```

**Escenarios fijos.** Velocidades reales, con λ = 1. Cuando hay punto de contacto, el inicio se resuelve con `Ballistics.SolveCenterTo(from, CenterForContact(superficie, punto, r), …)`.

| Id | Descripción (§8.2.2) | Inicio | Punto de contacto o dirección | Velocidad | Comprobación de construcción (pura, sin motor) |
|---|---|---|---|---|---|
| S1 | Drive paralelo rápido, 3 rebotes | (1.0, 0.9, 7.5) | frontis (0.6, 2.2) | 28 m/s horiz. | primer plano = frontis en el punto previsto ± 1e-5 m. `MinContactsBeforeStop = 3` |
| S2 | Drop bajo, 5 cm sobre el tin | (4.5, 0.6, 3.0) | frontis (5.5, TinTop + 0.05) | 6 m/s horiz. | ídem |
| S3 | Rasante en la lateral (< 10°) | (0.8, 1.0, 8.0) | dirección (−sin 8°, 0, −cos 8°) | 20 m/s | primer plano = lateral izquierda, incidencia real (con gravedad) < 10° |
| S4 | Unión entre la lateral y el suelo (nick) | (1.5, 1.0, 4.0) | centro (r, r, 2.5) | 12 m/s horiz. | lateral y suelo alcanzados en el **mismo paso** |
| S5 | Frontis y lateral cerca de la esquina frontal | (2.0, 1.5, 3.0) | frontis (r + 0.005, 1.5) | 15 m/s horiz. | primer plano = frontis; la lateral está a ≤ 5 mm del punto |
| S6 | Dos contactos en un paso (esquina trasera) | (3.2, 1.0, 6.0) | centro (6.40 − r, 1.0, 9.75 − r) | 25 m/s horiz. | los dos planos en el **mismo paso**, a ≥ 1 ms de sus bordes |
| S7 | Sale de la pared trasera | (3.2, 1.2, 6.0) | dirección (0, 0.2, 1) | 15 m/s | primer plano = pared trasera |
| S9a–i | Bordes de línea (ver abajo) | (3.2, 1.0, 5.0) | frontis (3.2, y) | 15 m/s horiz. | primer plano = frontis en (3.2, y) ± 1e-5 m |
| S10 | Se asienta en el suelo | (3.2, 0.3, 5.0) | dirección (0, −1, 0) | 1 m/s | 720 pasos (6 s simulados) |

**S9 [H04]:** tres líneas, con la huella de 10 mm. Para cada línea, un lanzamiento **3 mm por fuera**, otro **en el borde exacto** y otro **3 mm por dentro**. El margen de 3 mm supera la tolerancia de precisión de 1 mm, así que la clasificación es un resultado inequívoco. El borde exacto solo mide la precisión.

| Línea | Fuera (no toca) | Borde exacto | Dentro (toca) |
|---|---|---|---|
| FrontOut (borde inferior 4,570) | y = 4.557 | 4.560 | 4.563 |
| Tin (borde superior 0,480) | y = 0.493 | 0.490 | 0.487 |
| Service (borde inferior 1,780) | y = 1.767 | 1.770 | 1.773 |

**S8:** `count` lanzamientos con la semilla 20261020: posición uniforme en [0,3; 6,1] × [0,3; 2,5] × [0,3; 9,45], dirección uniforme en la esfera, rapidez uniforme en [2; 35] m/s y 360 pasos (3 s simulados) cada uno.
**S8d** (cobertura dirigida, mismo invariante de robustez): desde (3,2; 1,0; 4,875), hacia las 4 aristas verticales a y = 1,0, los 4 vértices del suelo y los puntos medios de las 4 aristas suelo–pared, con el centro a r de cada plano, a 10, 25 y 40 m/s: 36 lanzamientos.

- [ ] **Paso 1: tests que fallan.** Un test por fila de la columna "Comprobación de construcción", más:

```csharp
[Test] public void S9_ReferenceClassificationMatchesDesign() {
  // para S9a–i: LineContactClassifier sobre el punto de contacto PREVISTO (ideal) da ExpectTouch en los casos fuera/dentro
}
[Test] public void Random_IsReproducibleForSeed() { /* dos generaciones con semilla 7 → mismas posiciones y velocidades */ }
[Test] public void EdgeDirected_Has36LaunchesInsideCourt() { /* … */ }
```
- [ ] **Paso 2:** ejecutar. Esperado: FAIL.
- [ ] **Paso 3:** implementar. Si una comprobación de S4 o S6 no se cumple con los valores de la tabla, se ajusta **solo** la coordenada `z` del inicio hasta que se cumpla, y se registra el valor en el PR.
- [ ] **Paso 4:** ejecutar. Esperado: PASS.
- [ ] **Paso 5:** commit `feat(core): physics scenarios with construction checks`.

### Tarea 4.2: Mediciones, criterios trazables e informe [H03, H05, H07, H08, H10]

**Archivos:**
- Crear: `Core/Spike/PhysicsMeasures.cs`, `SpikeCriteria.cs`, `SpikeResult.cs` y `SpikeReportWriter.cs`
- Test: `Tests/EditMode/Spike/PhysicsMeasuresTests.cs` y `SpikeReportWriterTests.cs` (regresión)

**Interfaces:**
- Produce:

```csharp
public static class PhysicsMeasures {
  // Atravesamiento: al final de un paso, el centro queda al otro lado del plano de una superficie (por debajo de WallHeight),
  // o el segmento entre dos estados cruza un plano sin un ContactEvent de esa superficie en ese paso.
  public static int Crossings(Trajectory t, CourtDimensions d, out string detail);
  public static bool ExitedOverTop(Trajectory t, CourtDimensions d);    // salida legítima por arriba: no es atravesamiento
  public static float MaxPenetration(Trajectory t, CourtDimensions d, float radius);   // r − distancia del centro al plano; informativa
  public static bool AllFinite(Trajectory t, out int firstBadIndex);
  // Energía mecánica E = ½·m·|v|² + m·g·y entre el inicio y el fin de cada paso con contacto; aísla el trabajo de la gravedad.
  public static float MaxRelativeEnergyGain(Trajectory t, PhysicsParams p, bool cornerStepsOnly, out long step);
  public static bool IsCornerStep(Trajectory t, long step);   // ≥ 2 superficies en el paso, o superficies distintas en pasos consecutivos
  public static int RepeatedImpacts(Trajectory t, float minNormalSpeed);   // impactos no persistentes en la misma superficie en pasos consecutivos con |vn| > 0,5
  public static int PersistentSamples(Trajectory t);
  public static bool TrajectoriesBitwiseEqual(Trajectory a, Trajectory b, out int firstDiffIndex);   // estados (bits de float) y contactos (superficie y paso)
  public static int MaxImpactsInOneStep(Trajectory t);
  public static float ContactPrecisionMm(Trajectory t, PhysicsScenario s);   // primer contacto observado frente a ContactPoint
  public static float RemainingTimeConsumed(Trajectory t, PhysicsParams p, int contactIndex);   // ver §8.3.2 abajo
}
public enum Verdict { Pass, Fail, Info, NotEvaluated }
public sealed class CriterionResult {
  public string Id, Requirement, Scenarios, Measured, Threshold; public Verdict Verdict; public bool Gating;
  public int Evaluated, NotEvaluable;   // cuántos casos se pudieron evaluar
}
public sealed class SpikeResult {
  public string Backend, Variant, ProjectPhysics, Build, Device, ConfigHash; public bool Complete;
  public List<CriterionResult> Criteria = new();
  public bool AllGatingPass { get; }   // false si algún criterio de gate es Fail o NotEvaluated, o si Complete == false
}
public static class SpikeReportWriter { public static string ToMarkdown(SpikeResult r); }
```

**Criterios** (una fila por criterio en `SpikeResult` y en el informe). "Gate" = decide el gate de física. **`NotEvaluated` nunca cuenta como aprobado** [H08].

| Id | Requisito (spec) | Escenarios | Medición | Umbral | Gate |
|---|---|---|---|---|---|
| C1 | Robustez (§8.2.2, §17.A.1) | S1–S10, S8 (10 000), S8d | `Crossings` | 0 | sí |
| C2 | Estados finitos (robustez) | todos | `AllFinite` | todos finitos | sí |
| C3 | Esquinas: sin ganancia de rapidez (§8.2.2) | S4, S5, S6, S8, S8d (pasos de esquina) | `MaxRelativeEnergyGain(cornerStepsOnly)` | ≤ 1e-5 (ruido de float) | sí |
| C4 | Esquinas: sin vibración (§8.2.2) | S4, S5, S6, S10 | `RepeatedImpacts` | 0 | sí |
| C5 | Consistencia (§17.A.2) | S1–S7, S9, S10 con dos instancias nuevas; S8 con instancias reutilizadas frente a nuevas en 1 de cada 100 | `TrajectoriesBitwiseEqual` | igualdad | sí |
| C6 | Predicción desde el lanzamiento (§8.2.1, §17.A.3) | S1–S10 y S8 completo | juego y predicción con `Reset(s)` | igualdad | sí |
| C7 | Predicción tras un relanzamiento en vuelo (§17.A.3, contacto de raqueta) | S1–S7, S9 y S8 completo, en k = 30 y en el paso siguiente al primer contacto | juego y predicción con `Reset(estado del juego en k)` | igualdad | sí |
| C8 | Reutilización tras contactos y reposo (§8.2.1 cond. 1) | S10 → S10b (S1 en la misma instancia) y S6 → S1 | instancia reutilizada frente a una nueva | igualdad | sí |
| C9 | Continuación sin reinicio del juego | S1, S6 en k = 30 | predicción `Reset(estado en k)` frente al juego **sin** reiniciar | — | no (diagnóstico) |
| C10 | Precisión del contacto en líneas (S9) | S9a–i | `ContactPrecisionMm` | ≤ 1 mm [EXPERIMENTO, umbral del plan] | sí |
| C11 | Clasificación de líneas (S9) | S9 fuera y dentro | `LineContactClassifier` sobre el punto **observado** | igual a `ExpectTouch` | sí |
| C12 | §8.3.1 Primer impacto en el paso | S5, S6, S9 | superficie y punto del primer contacto frente a `Ballistics` | — | informe al gate |
| C13 | §8.3.2 Tiempo restante tras el impacto | impactos únicos de S1, S2, S7 y S9 | `RemainingTimeConsumed` (mediana y mínimo) | — | informe al gate |
| C14 | §8.3.2 Impactos por paso | todos | `MaxImpactsInOneStep`; tope configurable: sí o no | — | informe al gate |
| C15 | §8.3.3 y §8.3.7 Simultáneos e instante del evento | S4, S6 | ambos contactos en el mismo paso; `ContactTiming` entregado | — | informe al gate |
| C16 | Penetración y contactos persistentes | todos | `MaxPenetration`, `PersistentSamples` | — | no |
| C17 | Rendimiento de la predicción (§8.2.2, §17.C) | S1 hasta el segundo bote | `PredictionBenchmarkRun` (Tarea 4.3) | **p95 ≤ 1 ms en el iPhone** (estadístico pendiente de ratificar, D-2) | sí, solo en el iPhone |
| C18 | Facilidad de prueba (§8.2.2) | — | `Capabilities.RunsInEditMode` | — | no (preferencia) |

**Precisiones de medición:**
- **C1** reemplaza la tolerancia de 0,1 mm de la r1: esa tolerancia **mezclaba penetración con atravesamiento** y no estaba en la spec. La penetración se informa en C16 [H07].
- **C3** mide la energía mecánica entre el inicio y el fin del paso del contacto. Así, el trabajo de la gravedad no se confunde con una ganancia del contacto. Es la forma medible de "la rapidez después de un contacto doble nunca supera a la de antes". El umbral 1e-5 es la tolerancia de redondeo de `float`, no un margen físico [H07].
- **C5–C8:** bit a bit **en el mismo binario y el mismo dispositivo** (§8.2.1, condición 5). No se compara el editor con el iPhone. La comparación llega hasta el segundo bote o `MaxSteps`, porque en First Ball no hay contacto de raqueta.
- **C7** modela el contacto de raqueta: en ese momento, juego y predicción se reinician con el mismo estado. Es la forma en que el host cumple la condición 1 de §8.2.1. **C9** mide si PhysX conserva estado oculto cuando el juego no se reinicia. Es un diagnóstico y no decide el gate (decisión D-3).
- **Predictor más grueso:** P1 usa el mismo paso y el mismo código para la predicción, así que rigen C6–C8. Los umbrales de ≤ 1 cm y ≤ 2 ms y la exigencia de ≥ 3 contactos solo se aplicarían a otro predictor (§8.2.2), que este plan no construye [H05].
- **C12–C15 (§8.3):** se miden e informan con evidencia. No hay umbral numérico en la spec y el plan no inventa uno. Claude los analiza en el gate. `RemainingTimeConsumed` compara la distancia normal al plano al final del paso del impacto con `r + vn_salida · (dt − t_impacto)` (1,0 = consumió todo el tiempo; ~0 = se detuvo en el contacto). El tope de 4 impactos no es configurable en PhysX. El parámetro `physics.maxImpactsPerStep` queda como "no operativo con P1" en el panel (decisión D-4) [H10].

- [ ] **Paso 1: tests de las mediciones que fallan**, con trayectorias sintéticas construidas en el test:

```csharp
[Test] public void Crossings_DetectsCenterBeyondFrontWall() { /* estados z = 0.5 → −0.3 → 1 cruce */ }
[Test] public void Crossings_DetectsSegmentCrossingWithoutContact() { /* z = 0.05 → z = 0.05 al otro lado por un lateral sin evento → 1 */ }
[Test] public void Crossings_IgnoresExitOverTop() { /* y sube por encima de WallHeight → 0 cruces y ExitedOverTop */ }
[Test] public void Penetration_IsNotCrossing() { /* centro a 0.015 del frontis → Crossings 0 y MaxPenetration ≈ 0.005 */ }
[Test] public void EnergyGain_IgnoresGravityWork() { /* paso de caída libre con contacto ficticio: rapidez sube por la gravedad → ganancia ≤ 0 */ }
[Test] public void EnergyGain_DetectsContactGain() { /* paso de esquina con |v| de salida 1 % mayor a la misma altura → ganancia ≈ 0.02 */ }
[Test] public void RepeatedImpacts_IgnoresPersistentSamples() { /* Enter en 10 y Stay en 11 → 0; Enter en 10 y 11 con vn 1 m/s → 1 */ }
[Test] public void Bitwise_FindsFirstDifferingIndex() { /* un bit distinto en el índice 7 → false, 7 */ }
[Test] public void Bitwise_ComparesContactsToo() { /* mismos estados y un contacto con otra superficie → false */ }
[Test] public void RemainingTime_FullAndStalled() { /* salida ideal → 1.0 ± 0.01; centro quieto en r → 0 ± 0.01 */ }
[Test] public void Report_NotEvaluatedIsNeverPass_AndListsTraceability() {
  // un criterio de gate NotEvaluated → AllGatingPass == false; el Markdown tiene una fila por criterio con
  // Requisito, Escenarios, Medición, Umbral, Veredicto y "evaluados/no evaluables"
}
[Test] public void Report_IncompleteRunIsNeverPass() { /* Complete = false → AllGatingPass == false y el informe dice "INCOMPLETO" */ }
```
- [ ] **Paso 2:** ejecutar. Esperado: FAIL.
- [ ] **Paso 3:** implementar.
- [ ] **Paso 4:** ejecutar. Esperado: PASS.
- [ ] **Paso 5:** commit `feat(core): separated physics measures and traceable spike criteria`.

### Tarea 4.3: Ejecución incremental y experimento P1 en el editor [H06, H09, H12, directriz 7, directriz 10]

**Archivos:**
- Crear: `Core/Spike/SpikeRun.cs`, `PredictionBenchmarkRun.cs`, `Tests/Experiments/PhysXSpikeExperiment.cs` y `scripts/spike.sh`
- Test: `Tests/EditMode/Spike/SpikeRunTests.cs` (regresión, con una simulación falsa)

**Interfaces:**
- Produce:

```csharp
public sealed class SpikeOptions { public int RandomCount = 10000; public int Seed = 20261020; public string Device = "editor"; public string Build; public string ConfigHash; public string Variant; }
public sealed class SpikeRun {   // lanzamientos completos dentro de un presupuesto de tiempo por llamada; en el hilo principal
  public SpikeRun(IBallSimulationFactory f, PhysicsParams p, CourtDimensions d, SpikeOptions o);
  public bool Step(double budgetMs);   // true cuando terminó o se canceló
  public float Progress { get; } public string Phase { get; }
  public void Cancel();
  public SpikeResult Result { get; }   // Complete == false si se canceló
}
public sealed class PredictionBenchmarkRun {   // 20 mediciones de calentamiento descartadas y 200 medidas
  public PredictionBenchmarkRun(IBallSimulationFactory f, PhysicsParams p, CourtDimensions d, int warmup = 20, int runs = 200);
  public bool Step(double budgetMs);
  public (double median, double p95, double p99, double max) ResultMs { get; }
}
```
**Recursos acotados:** `SpikeRun` mantiene como máximo tres instancias vivas (juego, predicción y la "nueva" de C5 y C8). La "nueva" se destruye y se recrea por escenario fijo. Al terminar o cancelar, todo se destruye. Cada medición del benchmark es `Reset` + `TrajectoryRunner.Run` hasta el segundo bote de S1 con una instancia reutilizada, cronometrada con `Stopwatch`. **Incluye** el `Reset`, los pasos y la recolección de contactos. **No incluye** crear la escena.

- [ ] **Paso 1: tests que fallan** (simulación falsa en EditMode):

```csharp
[Test] public void Step_RespectsBudget_AndCompletesOverManyCalls() { /* falsa que tarda ~0,1 ms por lanzamiento; Step(2) procesa ≤ 25 lanzamientos; termina en N llamadas */ }
[Test] public void Cancel_ProducesIncompleteResult() { /* Cancel a mitad → Result.Complete == false */ }
[Test] public void Run_KeepsAtMostThreeLiveInstances_AndReleasesAll() { /* LiveInstances ≤ 3 durante; 0 al final */ }
[Test] public void Benchmark_DiscardsWarmup() { /* falsa con 20 primeras de 5 ms y el resto de 0,1 ms → max < 1 ms */ }
```
- [ ] **Paso 2:** ejecutar, ver que falla, implementar y ejecutar hasta que pase.
- [ ] **Paso 3: experimento** `PhysXSpikeExperiment` (ensamblado de experimentos): corre `SpikeRun` con `PhysXBallSimulationFactory(variant)` hasta terminar, escribe `Logs/physics-spike-P1-<variante>.md` y **solo falla si la ejecución no se completa** (excepción o `Complete == false`). Los veredictos son datos. Al final comprueba que `LiveInstances == 0`, que el número de escenas volvió al inicial y que `Physics.gravity` está restaurada.
- [ ] **Paso 4: `scripts/spike.sh [variante]`:** ejecuta el ensamblado de experimentos con `-assemblyNames SquashClash.Experiments`, pasa la variante por variable de entorno y aplica Enhanced Determinism activado o no con `ProjectSetup` antes de la ejecución. Sale con 0 si el informe se generó completo, aunque haya criterios en `Fail`.
- [ ] **Paso 5:** ejecutar la línea base y, **solo si un criterio de gate falla**, las variantes de ajustes documentados de PhysX, una por ejecución:
  - `Ccd`: `ContinuousDynamic` o `ContinuousSpeculative`;
  - `ContactOffset`: 0,001 o 0,0005;
  - iteraciones del solver: por defecto o 12/4;
  - Enhanced Determinism: activado o no.

  **No se cambian umbrales, escenarios ni mediciones.** Si un fallo parece un defecto del adaptador o de la prueba, se corrige en un PR de física, se explica en el informe y se vuelve a ejecutar.
- [ ] **Paso 6:** informe preliminar `docs/reviews/2026-MM-DD-first-ball-physics-spike-P1.md`, con la tabla de criterios de la línea base, las variantes probadas y sus diferencias, el benchmark del editor (informativo) y, para cada fallo, la clasificación de Claude: defecto del adaptador, defecto de la prueba o limitación real del motor.
- [ ] **Paso 7:** commit `feat(spike): incremental spike run, PhysX experiment and preliminary report`.

### Tarea 4.4: Ejecución en el iPhone [H06, H12, H17]

**Archivos:**
- Crear: `DevPanel/SpikeSection.cs`
- Modificar: `DevPanel/DevBar.cs` (botones **Escenario** y **Prueba física**)

**Interfaces:**
- Consume: `SpikeRun`, `PredictionBenchmarkRun`, `SpikeReportWriter`, `PhysicsScenarios`, `BallSimulationHost` y `AppBootstrap.Build`.

- [ ] **Paso 1:** **Escenario** lanza en la pelota visible el escenario elegido (S1–S10), para que el PO vea cada caso.
- [ ] **Paso 2:** **Prueba física** pausa la pelota, destruye sus simulaciones, ejecuta `SpikeRun` con un presupuesto de **8 ms por frame** y muestra el progreso, la fase y un botón **Cancelar**. Después ejecuta `PredictionBenchmarkRun`. La pantalla se mantiene encendida durante la ejecución (`Screen.sleepTimeout = NeverSleep`, restaurado al terminar). Si la app pasa a segundo plano, la ejecución se cancela y queda `INCOMPLETO`.
- [ ] **Paso 3:** al terminar, el informe completo (con build, perfil, dispositivo, iOS, variante, hash de configuración y estado térmico al inicio y al final) se muestra, se copia al portapapeles y se guarda en `Application.persistentDataPath/spike/`.
- [ ] **Paso 4:** compilar `dev` con la variante candidata del editor (la línea base, salvo que la Tarea 4.3 haya mostrado otra mejor; como máximo dos variantes). El PO o Codex ejecuta la prueba **con el teléfono en reposo térmico "nominal" al empezar**, y el informe se agrega a `docs/reviews/…-physics-spike-P1.md` como "Resultados en el iPhone 16 Pro". Para recuperar el archivo: `xcrun devicectl device copy from --device "$DEVICE_ID" --domain-type appDataContainer --domain-identifier "$BUNDLE_ID" --source Documents/spike --destination Logs/device-spike` (se verifica la sintaxis la primera vez; si no funciona, se usa el portapapeles).
- [ ] **Paso 5:** commit `feat(devpanel): on-device scenario launcher and physics spike runner`.

> ### Gate único de física [H06, directriz 8]
>
> **Entradas:** el informe del editor (Tarea 4.3) y el del iPhone (Tarea 4.4), **completos**, con la misma variante y el mismo hash de configuración física.
>
> 1. **Si todos los criterios de gate pasan en los dos,** Claude propone P1 con su variante congelada, según §8.2.3 (la alternativa más simple que cumple todo). Los criterios C12–C15 se presentan con su análisis.
> 2. **Si algún criterio de gate falla,** Claude lo clasifica con evidencia:
>    - **defecto del adaptador o de la prueba:** se corrige en un PR de física y se repiten las dos ejecuciones;
>    - **limitación real del motor:** Claude presenta al PO la evidencia y las alternativas, por ejemplo P2 con su contrato completado (Tarea 4.5). **No se propone ningún cambio de umbral** y una divergencia del predictor idéntico no se reclasifica como "predictor más grueso".
> 3. **El PO decide.** Claude registra la decisión en `docs/reviews/…-physics-decision.md` (PR I4c) y prepara la entrada para la spec, que solo se escribe con la autorización del PO (Tarea 7.3).
> 4. **Después del gate,** la variante elegida queda fija en `ProjectSetup` y en `PhysXVariant.Baseline`, y el experimento completo con esa variante se repite en la Tarea 7.1 como **prueba de aceptación**.

### Tarea 4.5 [CONDICIONAL: solo con la autorización explícita del PO tras el gate]: P2, simulación propia analítica [H21]

**Condición previa (Claude, antes de pedir la autorización):** completar en este plan el contrato que P2 todavía no tiene, para que se conecte sin cambiar la semántica de los parámetros:
- asentamiento y reposo (cuándo una pelota en el suelo deja de botar y rueda);
- qué pasa con el tiempo sobrante al llegar a `MaxImpactsPerStep`;
- los límites superiores de los planos (paredes de `WallHeight`, salida por arriba);
- la fricción: equivalencia del modelo de P2 con la fricción de Coulomb que usa PhysX con los mismos parámetros, o una regla explícita de traducción.

**Archivos (cuando se autorice):** `Core/Simulation/AnalyticBallSimulation.cs` y `AnalyticBallSimulationFactory.cs` (`Name => "P2-Analytic"`, `Timing = Exact`, `ImpactCapConfigurable = true`, `RunsInEditMode = true`). Test: el mismo experimento de la Tarea 4.3, también en EditMode.

**Algoritmo (§8.3), a completar con la condición previa:** primer impacto de la esfera contra los planos desplazados por el radio, en cada paso (cuadrático en `y`, lineal en `x` y `z`); integración exacta hasta ese instante; reflexión de la componente normal por `Restitution`; consumo del tiempo restante hasta `MaxImpactsPerStep`; impactos separados por menos de ε = 1e-6 s resueltos juntos; posición exacta `p += v·dt + ½·g·dt²`.

---

## Etapa 5 — Panel, presets y calibración

**Qué hace Codex:** el estado de edición con clases de aplicación, el almacenamiento atómico de presets, el diagnóstico con métricas de sesión y el panel autogenerado.
**Cómo se verifica:** EditMode para el estado de edición, el almacenamiento, las estadísticas y el log. En el iPhone, el recorrido del Paso 5 de la Tarea 5.3.
**En el iPhone:**
- con tres dedos durante 1 s, o con el botón "DEV", se abre el panel en la mitad inferior y se sigue viendo la cancha;
- el PO cambia la restitución y ve "1 cambio pendiente" hasta "Aplicar y relanzar";
- guarda un preset, cierra la app, la vuelve a abrir y el preset sigue activo;
- el diagnóstico muestra FPS, percentiles, CPU y GPU, temperatura, contactos y la comparación con la predicción.

**Para avanzar (punto de control):** el PO calibra al menos un preset propio sin tocar código.

*Las Tareas 5.1 y 5.2 no dependen del gate y pueden hacerse en paralelo con la Etapa 4.*

### Tarea 5.1: Estado de edición y almacenamiento de presets [H13, H14]

**Archivos:**
- Crear: `Core/Config/TuningState.cs` y `PresetStore.cs`
- Test: `Tests/EditMode/Config/TuningStateTests.cs` y `PresetStoreTests.cs`

**Interfaces:**
- Produce:

```csharp
public sealed class TuningState {
  public TuningState(ParamSet defaults, ParamSet applied);
  public ParamSet Defaults { get; }   // DefaultPreset.json: la base conocida
  public ParamSet Applied { get; }    // en uso
  public ParamSet Visible { get; }    // aplicado + pendientes: lo que muestra el panel y lo que se guarda
  public IReadOnlyCollection<string> PendingKeys { get; }
  public void Set(string key, double value);   // valida y recorta; Immediate → Applied y Changed; NextLaunch → pendiente
  public ParamSet ApplyPending();              // devuelve el nuevo Applied
  public void LoadPreset(PresetFile f, List<string> warnings);   // Overlay sobre Defaults; Immediate al instante, NextLaunch pendiente
  public event Action<string> Changed;
}
public sealed class PresetStore {
  public PresetStore(string directory);   // Application.persistentDataPath/presets en runtime
  public bool TrySave(PresetFile f, out string error);   // escribe <nombre>.json.tmp y lo reemplaza de forma atómica; nunca lanza
  public bool TryLoad(string name, out PresetFile f, out string error);
  public IReadOnlyList<string> List();
  public string LastActiveName { get; }   // last-active.txt, escrito también de forma atómica
  public bool TrySetLastActive(string name, out string error);
}
```
**Arranque:** `defaults = LoadDefault(DefaultPreset.json)`. Si `LastActiveName` existe y `TryLoad` funciona, `applied = Overlay(defaults, preset, avisos)`; si no, `applied = defaults` con un aviso. Al arrancar no hay pendientes.

- [ ] **Paso 1: tests que fallan**

```csharp
[Test] public void NextLaunch_StaysPendingUntilApply() {
  var s = NewState(); s.Set("physics.floor.restitution", 0.6);
  Assert.AreEqual(0.45, s.Applied.Get("physics.floor.restitution")); Assert.AreEqual(0.6, s.Visible.Get("physics.floor.restitution"));
  CollectionAssert.Contains(s.PendingKeys, "physics.floor.restitution");
  Assert.AreEqual(0.6, s.ApplyPending().Get("physics.floor.restitution")); Assert.IsEmpty(s.PendingKeys);
}
[Test] public void Immediate_AppliesAndNotifies() { /* camera.fovDeg 70 → Applied 70 y Changed("camera.fovDeg") */ }
[Test] public void Set_ClampsToRange_AndRejectsInvalidChoice() { /* launcher.speed 99 → 40; physics.stepHz 100 → sin cambio */ }
[Test] public void LoadPreset_PartialUsesDefaultsAsBase() { /* preset con solo launcher.speed → el resto igual a Defaults, aunque Applied tuviera otro valor */ }
[Test] public void SavedPreset_ContainsVisibleValues() { /* con un pendiente, FromSet(Visible) lo incluye */ }
[Test] public void Store_SaveThenReload_RestoresLastActive() { /* directorio temporal; instancia nueva → mismo nombre y valores */ }
[Test] public void Store_InterruptedWrite_KeepsPreviousFile() { /* dejar un .tmp a medias → TryLoad devuelve la versión anterior y se ignora el .tmp */ }
[Test] public void Store_ReadOnlyDirectory_ReturnsErrorWithoutThrowing() { /* … */ }
[Test] public void Boot_CorruptLastActive_FallsBackToDefaultsWithWarning() {   // Review Focus 2
  /* last-active apunta a un archivo con "{not json" → applied == defaults y 1 aviso */
}
```
- [ ] **Paso 2:** ejecutar, ver que falla, implementar y ejecutar hasta que pase.
- [ ] **Paso 3:** commit `feat(core): tuning state with apply classes and atomic preset store`.

### Tarea 5.2: Diagnóstico y métricas comparables [H13, H17, H22]

**Archivos:**
- Crear: `Core/Diagnostics/FrameTimeStats.cs` y `MetricsLog.cs`, `Runtime/App/ThermalMonitor.cs`, `Runtime/App/FrameTimingSampler.cs` y `Plugins/iOS/SquashThermal.mm`
- Test: `Tests/EditMode/Diagnostics/FrameTimeStatsTests.cs` y `MetricsLogTests.cs`

**Interfaces:**
- Produce:

```csharp
public sealed class FrameTimeStats {
  public FrameTimeStats(int windowCapacity = 3600);
  public void Add(double ms);
  public void ResetSession();
  public double SessionPercentile(double p);   // histograma de la sesión completa: cubetas de 0,1 ms hasta 250 ms + desbordamiento
  public double WindowPercentile(double p);    // últimos windowCapacity frames, exacto (para mostrar)
  public long SessionFrames { get; } public double SessionMinutes { get; }
  public double SessionLongFramesPerMinute(double overMs);
}
public sealed class MetricsLog {
  public MetricsLog(string directory, BuildInfo build);
  public string Phase { get; set; }   // "free" | "spike" | "benchmark" | "session"
  public void Write(string kind, EffectiveConfig cfg, string presetName, string presetFileHash, int pendingCount,
                    int seed, bool metricsValid, long droppedSinceRestart, string jsonPayload);   // una línea JSONL
  public string CurrentFilePath { get; }
}
public sealed class ThermalMonitor : MonoBehaviour { public string Current { get; } public string WorstThisSession { get; } public IReadOnlyDictionary<string, double> SecondsByState { get; } }
public sealed class FrameTimingSampler : MonoBehaviour { public double CpuMs { get; } public double GpuMs { get; } }   // FrameTimingManager
```
Cada línea JSONL lleva `ts`, `kind`, `phase`, `build` (`Label`), `presetName`, `presetFileHash`, `effectiveHash` (`cfg.Hash`), `pending`, `seed`, `metricsValid` y `droppedSinceRestart` [H13]. `SquashThermal.mm` expone `extern "C" int SquashThermalState()` con `[[NSProcessInfo processInfo] thermalState]`. En el editor, el estado es `"unknown"`.

- [ ] **Paso 1: tests que fallan**

```csharp
[Test] public void SessionPercentiles_NearestRankOverHistogram() { /* 1..100 ms → p50 50, p95 95, p99 99 (±0,1) */ }
[Test] public void Session_CoversMoreThanWindow() {
  var s = new FrameTimeStats(3600); for (int i = 0; i < 54000; i++) s.Add(i % 10800 == 0 ? 40 : 16.6);   // 15 min a 60 FPS, 5 frames largos
  Assert.AreEqual(15.0, s.SessionMinutes, 0.01); Assert.AreEqual(5.0 / 15.0, s.SessionLongFramesPerMinute(33), 1e-6);
}
[Test] public void MetricsLog_EveryLineCarriesTraceability() { /* dos líneas con cfg distintos → effectiveHash distinto; ambas con build, preset, pending, seed, phase y metricsValid */ }
```
- [ ] **Paso 2:** ejecutar, ver que falla, implementar y ejecutar hasta que pase.
- [ ] **Paso 3:** `AppBootstrap` escribe `"launch"` en cada `Launched`, y cada 10 s un `"frames"` con p50, p95 y p99 del segmento, frames > 33 ms, CPU y GPU p95, recolecciones de GC del segmento (`GC.CollectionCount(0)`), estado térmico actual y peor, y `DroppedSteps`. Un segmento con pasos descartados queda marcado `"dropped": true` [H22].
- [ ] **Paso 4:** commit `feat(diagnostics): session frame stats, thermal worst state and traceable metrics log`.

### Tarea 5.3: Panel autogenerado

**Archivos:**
- Crear: `DevPanel/ThreeFingerHold.cs`, `DevPanelView.cs`, `ParamSection.cs`, `PresetsSection.cs` y `DiagnosticsSection.cs`
- Modificar: `AppBootstrap.cs` (`TuningState`, `PresetStore`, `MetricsLog`), `DevBar.cs`, `ParamCatalog` y `DefaultPreset.json` (`launcher.repeatSeconds`, `debug.slowMotion`)
- Test: `Tests/PlayMode/AppBootstrapTests.cs` (ampliado)

**Interfaces:**
- Produce, en `AppBootstrap`: `TuningState Tuning`, `PresetStore Presets`, `MetricsLog Metrics`, `BallSimulationHost Ball`, `SimClock Clock`, `string ActivePresetName`, `void LaunchCurrent()`.
- `LaunchCurrent()`: `Tuning.ApplyPending()` → `EffectiveConfig.From(Tuning.Applied)` → `Ball.Launch(cfg)`. La línea `"launch"` la escribe el evento `Launched` con ese `cfg`. Desde esta tarea, **Lanzar** de la barra llama a `LaunchCurrent()`.

**Catálogo completo de First Ball** (los valores viven en `DefaultPreset.json`; aquí se listan para revisión):

| Grupo | Claves | Tarea |
|---|---|---|
| Ritmo | `pace.tempo`, `pace.launchScale` | 3.1 |
| Física | `physics.gravity`, `physics.stepHz`, `physics.maxImpactsPerStep` (no operativo con P1), materiales por superficie, `physics.backend` | 3.1, 3.2 |
| Cancha | `court.tinTop` | 2.2 |
| Lanzador | `launcher.x/y/z`, `launcher.yawDeg/pitchDeg`, `launcher.speed`, `launcher.repeatSeconds` (Float, 0, 0–10, Immediate) | 3.3, 5.3 |
| Cámara | `camera.height/backOffset/lookAtHeight/fovDeg`; `camera.mode`, `camera.followDamping/followMaxOffset` | 2.2, 6.1 |
| Ayudas | `aids.shadow/trail/impactMarks/prediction`, `aids.trailSeconds`, `aids.ballVisualScale` | 6.2 |
| Depuración | `debug.slowMotion` (Float, 1.0, 0.1–1.0, Immediate) | 5.3 |
| Referencias | `placeholders.visible`, `human.x/z`, `rival.x/z` | 2.2 |

- [ ] **Paso 1: tests que fallan**

```csharp
[UnityTest] public IEnumerator Boot_LoadsLastActivePreset_OrDefault() { /* sin last-active → "default"; con un preset guardado → ese nombre */ }
[UnityTest] public IEnumerator LaunchCurrent_AppliesPendingBeforeLaunching() { /* Set restitución 0.6 → LaunchCurrent → PendingKeys vacío y Ball.ActiveConfig.Physics.Floor.Restitution == 0.6 */ }
[UnityTest] public IEnumerator SlowMotion_InvalidatesMetrics() { /* debug.slowMotion 0.5 → la siguiente línea del log tiene metricsValid false */ }
[UnityTest] public IEnumerator NonOperativeParam_IsFlagged() { /* con P1, ParamDef("physics.maxImpactsPerStep").OperativeFor("P1-PhysX") == false */ }
```
- [ ] **Paso 2:** ejecutar, ver que falla, implementar `AppBootstrap` y ejecutar hasta que pase.
- [ ] **Paso 3:** el panel, solo en DevPanel, con UI Toolkit construida desde código:
  - `ThreeFingerHold` (EnhancedTouch) abre o cierra el panel con tres dedos mantenidos 1 s (§13.1); el botón "DEV" en la esquina superior derecha del `Screen.safeArea` hace lo mismo;
  - `ParamSection` genera un control por `ParamDef`, agrupado por `Group`: slider con valor y unidad (`Float`, `Int`), toggle (`Bool`) o desplegable con las etiquetas de `Choices`. Un parámetro no operativo para el backend activo aparece deshabilitado con el texto "no operativo con P1";
  - el encabezado muestra "N cambios pendientes" y el botón **"Aplicar y relanzar"** (`LaunchCurrent`);
  - la barra de la Etapa 3 queda fija, y se le agrega el selector de lanzamientos (Tarea 6.3);
  - `PresetsSection`: "Guardar como…", cargar desde la lista, "Exportar" (JSON al portapapeles con `GUIUtility.systemCopyBuffer`) e "Importar del portapapeles" con `TryParse` y los avisos de `Overlay` visibles;
  - `DiagnosticsSection`, actualizada cada 0,5 s: FPS; p50, p95 y p99 de la ventana y de la sesión; frames > 33 ms por minuto; CPU y GPU; temperatura actual y peor; `PredictionMatchesBitwise`; los últimos 8 `ContactEvent` (superficie, UV, líneas tocadas con r = 0,010 y `Timing`); `DroppedSteps`; y "MÉTRICAS NO VÁLIDAS" si se usó la cámara lenta.
- [ ] **Paso 4:** `check-release.sh` vuelve a dar `RELEASE-CHECK: OK`.
- [ ] **Paso 5:** compilar e instalar `dev`. Comprobar en el iPhone, en orden: abrir con tres dedos, lanzar, cambiar la restitución del suelo ("1 cambio pendiente"), aplicar y relanzar, guardar "po-1", cerrar y abrir la app (el panel indica "po-1"), exportar e importar.
- [ ] **Paso 6:** commit `feat(devpanel): auto-generated tuning panel with presets and diagnostics`.

---

## Etapa 6 — Legibilidad: cámara de seguimiento, ayudas y L1–L6

**Qué hace Codex:** la variante de cámara con seguimiento amortiguado, las ayudas de legibilidad y los seis lanzamientos de §12.3, con condiciones numéricas que prueban que cada uno hace lo que dice su nombre [H19].
**Cómo se verifica:** tests de PlayMode de cámara, ayudas y lanzamientos. La legibilidad la juzga el PO.
**En el iPhone:** la pelota tiene sombra en el suelo, una estela breve y deja una marca donde pega. En modo seguimiento, la cámara acompaña lateralmente con suavidad. El selector de la barra ofrece L1–L6.
**Para avanzar (punto de control):** H1, primera pasada: el PO distingue altura y profundidad **sin abrir el panel** en L1–L6, con al menos una de las dos cámaras (§12.3, §17.B).

*Las Tareas 6.1 y 6.2 pueden hacerse en paralelo con la Etapa 5.*

### Tarea 6.1: Cámara de seguimiento

**Archivos:**
- Modificar: `Runtime/Camera/CourtCameraRig.cs`, `ParamCatalog` y `DefaultPreset.json` (`camera.mode` Choice 0 "fija" / 1 "seguimiento", Immediate; `camera.followDamping` 0.3, 0–1; `camera.followMaxOffset` 1.5, 0–3)
- Test: `Tests/PlayMode/CourtCameraRigTests.cs` (ampliado)

**Interfaces:**
- Produce: en modo **seguimiento**, `Tick` desplaza la `x` de la cámara hacia la `x` de la pelota con amortiguamiento crítico de constante `camera.followDamping`, limitado a ±`followMaxOffset`, y desplaza igual el punto de mira.

- [ ] **Paso 1: tests que fallan**

```csharp
[UnityTest] public IEnumerator Follow_StaysWithinMaxOffset() { /* pelota en x = 0 durante 5 s → |cam.x − 3.2| ≤ 1.5 */ }
[UnityTest] public IEnumerator Fixed_IgnoresBall() { /* modo 0 y la pelota se mueve → la cámara no cambia */ }
[UnityTest] public IEnumerator Follow_DefaultFraming_StillShowsFrontWallCorners() { /* pelota en x = 0 y x = 6.4: las esquinas del frontis siguen en el viewport */ }
```
- [ ] **Paso 2:** ejecutar, ver que falla, implementar y ejecutar hasta que pase.
- [ ] **Paso 3:** commit `feat(runtime): damped-follow court camera`.

### Tarea 6.2: Ayudas de legibilidad

**Archivos:**
- Crear: `Runtime/Ball/LegibilityAids.cs`
- Modificar: `BallView.cs` (escala visual), `ParamCatalog` y `DefaultPreset.json` (`aids.shadow`, `aids.trail`, `aids.impactMarks` y `aids.prediction` Bool 1/1/1/0; `aids.trailSeconds` 0.15, 0–0.5; `aids.ballVisualScale` 1.0, 1–2.5; todas Immediate)
- Test: `Tests/PlayMode/LegibilityAidsTests.cs`

**Interfaces:**
- Consume: `BallSimulationHost.RenderPosition`, `ContactRaised`, `Prediction` y `TuningState.Changed`.
- Produce: `LegibilityAids`:
  - **sombra:** disco oscuro en (x; 0,001; z) bajo la pelota, siempre visible; tamaño y opacidad bajan con la altura [EXPERIMENTO];
  - **estela:** `TrailRenderer` de `aids.trailSeconds` en tiempo real;
  - **marcas de impacto:** disco plano en cada contacto **no persistente** con una pared, que se desvanece en 1,5 s, con un máximo de 6;
  - **predicción:** `LineRenderer` con `Prediction.States`.

- [ ] **Paso 1: tests que fallan**

```csharp
[UnityTest] public IEnumerator Shadow_FollowsBallXZ_OnFloor() { /* pelota en (2, 3, 5) → sombra en (2, 0.001, 5) */ }
[UnityTest] public IEnumerator ImpactMark_OnWallImpactsOnly_CapsAtSix() { /* 8 impactos de pared, 1 de suelo y 3 persistentes → 6 marcas */ }
[UnityTest] public IEnumerator Toggles_DisableAids() { /* aids.shadow = 0 → sombra inactiva */ }
[UnityTest] public IEnumerator BallVisualScale_DoesNotChangePhysicsRadius() { /* escala 2 → la esfera dibujada mide 2r; ActiveConfig.Physics.BallRadius sigue en 0.020 */ }
```
El §8.6 pide que las ayudas no sean imprescindibles para entender un rebote ni tapen al rival. Eso lo juzga el PO, no un test.
- [ ] **Paso 2:** ejecutar, ver que falla, implementar y ejecutar hasta que pase.
- [ ] **Paso 3:** commit `feat(runtime): shadow, trail, impact marks and prediction line`.

### Tarea 6.3: Lanzamientos de legibilidad L1–L6 con condiciones numéricas [H19]

**Archivos:**
- Crear: `Core/Config/LaunchPresets.cs`
- Modificar: `DevBar.cs` (selector L1–L6)
- Test: `Tests/EditMode/Config/LaunchPresetsTests.cs` y `Tests/PlayMode/LaunchPresetsPhysicsTests.cs` (regresión con el backend elegido en el gate y `DefaultPreset`)

**Interfaces:**
- Produce:

```csharp
public static class LaunchPresets {
  public sealed class Entry { public string Id, Label; public LaunchSpec Launch; public Vector2 HumanXZ, RivalXZ; }
  public static IReadOnlyList<Entry> All(CourtDimensions d, float gravity);   // L1–L6 (§12.3), resueltos con Ballistics hacia un punto del frontis
}
```

| Id | §12.3 | Condición numérica sobre la trayectoria simulada (hasta el segundo bote) |
|---|---|---|
| L1 | Drive largo que rebota en la pared trasera | hay un contacto con `BackWall` antes del segundo bote |
| L2 | Pelota baja pegada a la lateral | después del contacto con el frontis, `x ≤ 0,30 m` durante ≥ 50 % de los pasos, y `y ≤ 1,0 m` en el primer bote |
| L3 | Drop corto adelante | segundo bote a `z ≤ 2,0 m` del frontis |
| L4 | Globo alto | altura máxima ≥ 4,0 m y primer bote a `z ≥ 7,0 m` |
| L5 | Pasa detrás del humano (oclusión) | durante ≥ 12 pasos, la recta cámara fija → pelota pasa a ≤ 0,25 m del eje de la cápsula del humano |
| L6 | Esquina trasera con el rival cerca | contactos con `BackWall` y una lateral a ≤ 1,0 m de la esquina, y el rival a ≤ 1,0 m de ese contacto |

- [ ] **Paso 1: tests que fallan.** EditMode: todos los inicios dentro de la cancha, a más de r de las paredes. PlayMode: un test por fila de la tabla, con el backend elegido.
- [ ] **Paso 2:** ejecutar, ver que falla e implementar. Si un lanzamiento no cumple su condición, se ajusta **solo su punto objetivo o su velocidad**, nunca la condición. Si una calibración posterior del PO rompe una condición, se reajusta el lanzamiento en un PR que lo diga.
- [ ] **Paso 3:** compilar e instalar `dev`. Recorrer L1–L6 con las dos cámaras.
- [ ] **Paso 4:** commit `feat(core): legibility launches L1–L6 with numeric checks`.

---

## Etapa 7 — Validación integral de First Ball

**Qué hace Codex:** la batería final, la sesión de rendimiento y la plantilla de validación.
**Cómo se verifica:** con los scripts y la plantilla completada por el PO.
**En el iPhone:** la build `dev` para la sesión, y la `prod` sin panel.
**Para avanzar:** el PO declara First Ball validado [D10], con la evidencia de §15: distingue altura y profundidad, se cumplen los invariantes de física, hay una decisión de física tomada y hay un rendimiento base medido.

### Tarea 7.1: Batería final y prueba de aceptación de física

- [ ] **Paso 1:** `scripts/test.sh editmode && scripts/test.sh playmode`. Esperado: los dos con `failed=0` y `total` mayor que en la Tarea 6.3. Los fallos de física **no** se mezclan con la regresión [H16].
- [ ] **Paso 2:** `scripts/spike.sh <variante elegida>`. Esperado: `AllGatingPass` igual que en el gate. Si un criterio que pasaba ahora falla, es una regresión y se trata antes de seguir.
- [ ] **Paso 3:** `scripts/check-release.sh` → `RELEASE-CHECK: OK`, e instalación de `prod`: no hay botón "DEV", tres dedos no hacen nada y el log dice `devpanel=absent`.

### Tarea 7.2: Sesión de rendimiento y registro de validación

**Archivos:**
- Crear: `docs/validation/first-ball-validation.md`

- [ ] **Paso 1: protocolo de rendimiento** [H17]: build `dev` con la variante elegida, el teléfono en estado térmico "nominal" al empezar, brillo y modo de bajo consumo anotados, sin cámara lenta ni la prueba de física ni el benchmark durante la sesión. `MetricsLog.Phase = "session"` durante **15 minutos** de lanzamientos repetidos (L1–L6 con `launcher.repeatSeconds`). Se registran los percentiles de la **sesión completa**, los frames > 33 ms por minuto, CPU y GPU p95, el **peor** estado térmico y el tiempo en cada estado, y `DroppedSteps`, frente a §17.C.
- [ ] **Paso 2:** plantilla con estas secciones:
  1. **Legibilidad (H1):** L1–L6 × {fija, seguimiento}, con "¿distingo la altura? sí/no", "¿distingo la profundidad? sí/no", "¿alguna ayuda tapa al rival? sí/no", una valoración de 1 a 5 y comentarios.
  2. **Física:** enlaces al informe y a la decisión del gate, la variante elegida y el preset de calibración (nombre y hash efectivo).
  3. **Rendimiento:** los resultados del Paso 1.
  4. **Invariantes de First Ball** (§17.A.1, 2, 3, 10 y 14): estado y evidencia.
  5. **Decisión del PO:** continuar a First Shot, ajustar o volver.
- [ ] **Paso 3:** el PO completa la plantilla. Codex adjunta los JSONL de `MetricsLog` (con `devicectl device copy from` o el portapapeles).
- [ ] **Paso 4:** commit `docs: First Ball validation record`.

### Tarea 7.3: Registro en la spec y nuevos valores por defecto (requiere aprobación del PO)

- [ ] **Paso 1:** con la autorización del PO, Claude registra en la spec la decisión de física (§0.3 y §8.2) y las aclaraciones pendientes de la respuesta a la revisión (decisiones D-4, D-5 y D-6).
- [ ] **Paso 2:** con la aprobación del PO, el preset calibrado pasa a ser `DefaultPreset.json` en un PR aparte. `DefaultPreset_IsCompleteAndValid` y las pruebas de L1–L6 deben seguir pasando.

---

## Cobertura de la spec (autorrevisión)

| Requisito de la spec | Tarea |
|---|---|
| Preparación: Unity, Xcode, iPhone y compatibilidad (§15, B2–B4, riesgo 16.12) | 0.1, 0.2, 1.3 |
| Repositorio, `.superpowers/` ignorado (anexo A) | 1.1, 1.4 |
| Vertical, IL2CPP, compilación e instalación por CLI (anexo A, D18) | 1.2, 1.3 |
| Cancha WSF, líneas de 50 mm, tin de 0,48 m, huella (§8.1, §8.7.2, §8.7.3, §8.7.6) | 2.1, 2.3 |
| Una autoridad, eventos de contacto, paso de 120 Hz (§8.2, §8.3) | 3.1–3.3 |
| Requisitos de colisión §8.3.1–8.3.3 y 8.3.7 (medidos en P1) | 4.2 (C12–C15) |
| Escenarios S1–S9 (§8.2.2) más S10 y S8d | 4.1 |
| Condiciones de predicción e invariante bit a bit en S1–S9 y en el dispositivo (§8.2.1, §17.A.3) | 4.2 (C6–C8), 4.4 |
| Robustez, esquinas y consistencia (§8.2.2, §17.A.1, §17.A.2) | 4.2 (C1–C5) |
| Rendimiento de la predicción en el iPhone (§8.2.2, §17.C) | 4.3, 4.4 (C17) |
| Evaluación secuencial P1 → P2 y regla de decisión (§8.2.3) | Gate, 4.5 |
| Un reloj, τ, λ y cámara lenta que invalida métricas (§8.4) | 3.1, 5.3 |
| Panel desde First Ball (§13.1, §13.2, D29) y exclusión de producción (§17.A.10) | 3.3, 4.4, 5.3, 7.1 |
| Clases de cambio y "Aplicar y relanzar" (§13.4) | 5.1, 5.3 |
| Presets JSON versionados, recuperación tras reiniciar y trazabilidad (§13.4, §17.A.14) | 2.2, 5.1, 5.2 |
| Cámara fija de referencia y variante de seguimiento (§12.1) | 2.3, 6.1 |
| Sombra, estela, marcas y ayudas no imprescindibles (§8.6) | 6.2, 7.2 |
| Escenarios de legibilidad 1–6 (§12.3) y H1 | 6.3, 7.2 |
| Métricas de FPS, CPU/GPU, térmica y duración (§17.C, §17.E) | 5.2, 7.2 |
| Pérdida de foco: pausa (§6.5, regla 7) | 3.3 |

**Fuera de este plan, a propósito:** gestos y golpes, contrato T3, error, planificador, IA, reglas de punto y saque, consecuencias por fase de las líneas, sonido y vibración (desde First Shot), dirección de arte, Android, un sistema de migración de presets entre esquemas, automatización de GitHub más allá de crear el repo, y P2 mientras no se autorice. El clasificador de líneas solo informa qué franjas toca la huella; las consecuencias (out, down, fault) llegan con First Rally y First Match.

## Correspondencia con la r1

| r1 | r2 |
|---|---|
| T0.1, T0.2 | T0.1, T0.2 (Hub por interfaz, arm64, identificadores y cuenta de Xcode) |
| T1.1 | T1.1 (local) y T1.4 (remoto) |
| T2.1, T2.2, T3.1 | T1.2 (editor y proyecto), T1.3 (scripts, build, arranque e instalación de `dev` y `prod`) |
| T4.1 | T2.1 (diagonal con ancho perpendicular) |
| T6.1 (catálogo y presets) | T2.2 (base de configuración) y T5.1 (estado de edición y almacenamiento) |
| T4.2 + cámara fija de T7.1 | T2.3 |
| T5.1 | T3.1 (reloj y contrato corregidos, `Ballistics`) |
| T5.3 (adaptador) | T3.2 (adaptador y ciclo de vida) y T4.3 (experimento) |
| T5.4 + arranque de T6.3 | T3.3 (host con configuración efectiva, barra mínima, pausa y exclusión del panel) |
| T5.2 | T4.1 (escenarios) y T4.2 (mediciones y criterios) |
| Prueba en el iPhone de T6.4 | T4.4 |
| Gates de T5.3 y T6.4 | Gate único de física |
| T5.5 | T4.5 (con condición previa) |
| T6.2 | T5.2 |
| Panel de T6.3 + diagnóstico de T6.4 | T5.3 |
| Seguimiento de T7.1 | T6.1 |
| T7.2 | T6.2 |
| L1–L6 de T6.1 | T6.3 (con condiciones numéricas) |
| T8.1 | T3.3 (primer `check-release.sh`) y T7.1 |
| T8.2 | T7.1, T7.2 y T7.3 |
