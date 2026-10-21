# Squash Clash: First Ball Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.
>
> **En este proyecto:** Codex implementa las tareas una a una en el Mac del PO. Claude revisa cada PR antes de la siguiente tarea. El PO valida en el iPhone al cierre de cada etapa. **No se empieza ninguna etapa sin el visto bueno del PO sobre la anterior.**

**Goal:** dejar instalada en el iPhone 16 Pro del PO una app vertical que lanza una pelota de squash en una cancha WSF, la muestra de forma legible con dos cámaras y un panel de desarrollador, y produce la evidencia para elegir la física (§8.2 de la spec).

**Architecture:** un proyecto Unity en la raíz del repositorio con cuatro ensamblados:
- **Core:** lógica pura, sin escena: geometría de la cancha, contrato de simulación, reloj, parámetros, presets y suite de la prueba de física. Se prueba en EditMode.
- **Runtime:** integración: PhysX, vistas, cámara y arranque.
- **DevPanel:** excluido de producción con un define.
- **Editor:** build y configuración.

La simulación de la pelota está detrás de `IBallSimulation`. La prueba de física la ejecuta primero con PhysX (P1). La simulación propia (P2) es una tarea **condicional**, que solo se hace si el PO la decide tras el informe.

**Tech Stack:**
- Unity 6.3 LTS (6000.3.x, última versión de mantenimiento) con C#, URP, Input System (Enhanced Touch), UI Toolkit (runtime) y Unity Test Framework (NUnit);
- IL2CPP/ARM64;
- Xcode 27.1, `xcodebuild` y `xcrun devicectl`;
- git y GitHub (`gh`).

**Spec:** `docs/superpowers/specs/2026-10-20-squash-clash-design-v0.3.md` (v0.3 aprobada, con la enmienda E.0 tras la verificación WSF).

## Global Constraints

- iPhone primero, **solo vertical**, Unity + C#, macOS [D01]. Dispositivo de validación: **iPhone 16 Pro con iOS 26.6.2**.
- **Solo First Ball.** No se implementan golpes, gestos de golpe, planificador, IA, reglas de punto, marcador, saque, cansancio ni error. Tampoco se crean interfaces "para después" (spec §14, §15).
- **Una sola autoridad sobre el estado de la pelota.** Nunca hay dos sistemas que corrijan alternadamente la misma pelota (§8.2).
- La predicción parte **del estado de simulación, nunca de una posición interpolada para el render**. Los parámetros físicos se aplican solo al iniciar un lanzamiento (§8.2.1, condiciones 1 y 3).
- Pelota con **radio físico de 20 mm** (§8.1). **Paso fijo de 120 Hz** para la pelota, independiente del render [EXPERIMENTO] (§8.3.6). Tope de **4 impactos por paso** (§8.3.2).
- **Ritmo global τ = 0,65** de partida y **escala de lanzamiento λ = 1,0**. Se empieza ajustando solo τ (§8.4).
- **Cámara lenta solo en el panel.** Al usarla, las métricas de la sesión quedan marcadas como **no válidas** (§8.4).
- **Cancha WSF verificada** (§8.1, §8.7.2):
  - 9,75 × 6,40 m;
  - líneas de 50 mm, con las alturas medidas **hasta el borde inferior**: out del frontis a 4,570 m, línea de saque a 1,780 m y out trasero a 2,130 m;
  - **borde superior del tin a 0,480 m** (opción PSA: 0,430 m);
  - short line entre 5,440 y 5,490 m del frontis;
  - cuadros de saque de 1,600 m de lado interior.
- **Huella de contacto de 10 mm** para juzgar las líneas [EXPERIMENTO] (§8.7.3).
- **Sin backend.** El **panel queda excluido de las builds de producción** y está presente en las de desarrollo [D29].
- **Presets locales en JSON versionado.** Al arrancar se carga el último preset activo. Cada registro de métricas guarda el preset (nombre y hash), el build y la semilla (§13.4).
- **No se usan:** ECS/DOTS, contenedores de inyección, buses globales de eventos, frameworks genéricos, motores de red ni aritmética determinista (anexo A).
- **Metas de rendimiento** (§17.C):
  - 60 FPS, con p95 ≤ 16,7 ms, p99 ≤ 25 ms y como máximo 1 frame por minuto por encima de 33 ms;
  - predicción ≤ 1 ms.
- **Umbrales de la prueba de física** (§8.2.2):
  - 0 pelotas que atraviesan superficies en 10 000 lanzamientos;
  - la rapidez nunca aumenta tras un contacto en una esquina;
  - predicción **bit a bit**, o ≤ 1 cm y ≤ 2 ms si el predictor es más grueso.
- **Idiomas:** la documentación va en español. Los identificadores, el código y los mensajes de commit van en inglés.
- **Codex no modifica la spec.** Cualquier discrepancia entre el plan y la spec se reporta a Claude.

## Review Focus

Son los cinco fallos más probables que ningún test de las tareas cubre por sí solo. Cada uno tiene ahora un test o una verificación asignada:

1. **Ruta con espacios** (`/Users/rafaallendes/Projects/Squash Clash`) en Unity CLI, IL2CPP y `xcodebuild`. Se espera que todo compile igual. → Tarea 2.2, paso de verificación con la ruta real. Si falla, un enlace simbólico sin espacios.
2. **La app pasa a segundo plano o se bloquea la pantalla en pleno vuelo.** Al volver, la pelota no salta ni se pone a simular segundos de golpe: la simulación queda en pausa. → Tarea 5.1 (`Advance_HugeDelta_CapsStepsAndCountsDropped`) y Tarea 6.3 (`OnApplicationPause` → `SimClock.Paused`).
3. **Un preset corrupto, de un esquema antiguo, con claves desconocidas o con valores fuera de rango.** Se espera que la app arranque igual, con los valores por defecto y un aviso. → Tarea 6.1 (tests de `PresetCodec`).
4. **Un cambio de física en el panel con la pelota en el aire.** Se espera que no afecte al vuelo actual y que se aplique en el siguiente lanzamiento. → Tarea 6.1 (`NextLaunchParam_StaysPendingUntilApply`) y Tarea 6.3 (`Launch` aplica lo pendiente).
5. **La pelota rodando o quieta en el suelo al final de un vuelo.** Velocidades bajas y `bounceThreshold` de PhysX: se espera que no haya NaN, que el número de eventos de contacto sea acotado y que no haya vibración. → Tarea 5.3 (`S10_Settle_NoNaNAndBoundedContacts`).

---

## Estructura de archivos

```
/  (raíz del repo = proyecto Unity)
  AGENTS.md  README.md  .gitignore  .gitattributes
  docs/  (existente)   docs/validation/first-ball-validation.md
  scripts/  unity-env.sh  test.sh  build-ios.sh  install-ios.sh  check-release.sh  device.env.example
  Assets/SquashClash/
    Core/                    SquashClash.Core.asmdef (sin MonoBehaviours, sin escena)
      Court/                 SurfaceId, CourtDimensions, LineId, LineBand, CourtLines, LineContactClassifier
      Simulation/            BallState, ContactEvent, SurfaceMaterial, PhysicsParams, IBallSimulation,
                             Trajectory, TrajectoryRunner, SimClock, LaunchSpec, Launcher
      Spike/                 PhysicsScenario, PhysicsScenarios, PhysicsInvariants, SpikeResult,
                             PhysicsSpikeSuite, SpikeReportWriter, PredictionBenchmark
      Tuning/                ApplyClass, ParamDef, ParamRegistry, FirstBallParams, PresetFile,
                             PresetCodec, PresetStore, LaunchPresets
      Diagnostics/           FrameTimeStats, MetricsLog, BuildInfo
    Runtime/                 SquashClash.Runtime.asmdef
      Physics/               SurfaceTag, PhysXBallSimulation, PhysXBallSimulationFactory
      Court/                 CourtView
      Ball/                  BallSimulationHost, BallView, LegibilityAids
      Camera/                CourtCameraRig
      App/                   AppBootstrap, PlaceholderPlayers, ThermalState, AppPanelSettings.asset
      Resources/             DefaultPreset.json, BuildInfo.json (lo genera el build)
    Plugins/iOS/             SquashThermal.mm
    DevPanel/                SquashClash.DevPanel.asmdef (defineConstraints: SQUASH_DEV)
                             DevPanelBootstrap, ThreeFingerHold, DevPanelView, ParamSection,
                             DiagnosticsSection, PresetsSection, SpikeSection
    Editor/                  SquashClash.Editor.asmdef: BuildScript, ProjectSetup
    Scenes/Main.unity
    Tests/EditMode/          SquashClash.Core.Tests.asmdef
    Tests/PlayMode/          SquashClash.Runtime.Tests.asmdef
```

**Decisiones de estructura (de Claude):**
- **Proyecto Unity en la raíz**, para que los scripts y las rutas sean cortas.
- **Ensamblados:**
  - El anexo A pide dos (lógica e integración).
  - Se agrega **DevPanel**, porque es la forma más simple de excluirlo de producción con `defineConstraints`.
  - Se agrega **Editor**, que Unity exige para el código de build.
- **Valores por defecto en `DefaultPreset.json`,** con el mismo formato que los presets del PO, en lugar de ScriptableObjects (anexo A, [PROPUESTA]). Así, "el preset del PO pasa a ser el nuevo valor por defecto" es copiar un archivo, y hay un solo formato.
- **El panel no se referencia desde la escena.** Lo crea `DevPanelBootstrap` con `[RuntimeInitializeOnLoadMethod]`, así que en producción no quedan scripts faltantes.

**Convención de coordenadas** (la usan todas las tareas):
- origen en la esquina del suelo con el frontis y la pared izquierda, vista desde atrás;
- `x` va de 0 a 6,40 hacia la derecha, `y` es la altura y `z` va de 0 en el frontis a 9,75 en la pared trasera.

**Coordenadas 2D (uv) de cada superficie:**

| Superficie | u | v |
|---|---|---|
| Frontis (z = 0) | x | y |
| Pared trasera (z = 9,75) | x | y |
| Laterales (x = 0 y x = 6,40) | z | y |
| Suelo (y = 0) | x | z |

---

## Etapa 0 — Preparación y validación del entorno

**Qué hace Codex:** inventario, instalación de Unity LTS y verificación del iPhone. No crea código.
**Cómo se verifica:** salida de los comandos registrada en `docs/validation/environment.md`.
**En el iPhone:** nada todavía. Solo debe aparecer en `devicectl list devices` como disponible.
**Para avanzar:** se cumplen todas las casillas de la Tarea 0.2. Cualquier punto que no se pueda verificar queda **explícito como pendiente**, nunca se da por confirmado.

### Tarea 0.1: Inventario del entorno y Unity 6.3 LTS con iOS

**Archivos:**
- Crear: `docs/validation/environment.md`

**Estado verificado por Claude el 2026-10-20:**
- macOS 26.6.2.
- Xcode 27.1 (27A9275), con la licencia aceptada.
- git y gh, con sesión iniciada como `rafaallendes-boop`.
- Unity Hub instalado.
- Editor **6000.6.6f1** instalado, **sin módulo iOS**. *No es la línea LTS y no se usa.*

**Compatibilidad, según las fuentes públicas a esa fecha:**
- Unity 6.3 LTS soporta iOS 15+ y recomienda Xcode 16 o posterior. No menciona Xcode 27.
- Unity 6.0 LTS termina su soporte en octubre de 2026, y la 6.3 LTS lo mantiene hasta diciembre de 2027.
- **La compatibilidad Unity 6.3 ↔ Xcode 27.1 ↔ iOS 26.6.2 está sin verificar** y se verifica en la Etapa 3.

- [ ] **Paso 1:** confirmar en https://unity.com/releases/unity-6/support cuál es la línea LTS con soporte vigente y su último parche. Por defecto, 6000.3.x. Si existe una LTS más nueva con soporte, se informa al PO antes de elegirla. Anotar la versión exacta (`6000.3.NNf1`) y su changeset.
- [ ] **Paso 2:** el **PO** inicia sesión en Unity Hub y activa la licencia Personal. Es interactivo, así que Codex no lo puede hacer.
- [ ] **Paso 3:** instalar el editor con el módulo iOS desde la CLI de Hub:

```bash
HUB="/Applications/Unity Hub.app/Contents/MacOS/Unity Hub"
"$HUB" -- --headless install --version 6000.3.NNf1 --changeset <changeset> --module ios
"$HUB" -- --headless editors --installed
```
Esperado: la lista incluye `6000.3.NNf1`, y existe `/Applications/Unity/Hub/Editor/6000.3.NNf1/PlaybackEngines/iOSSupport`.

- [ ] **Paso 4:** escribir `docs/validation/environment.md` con:
  - las versiones de macOS, Xcode, Unity (con su changeset), gh y git;
  - la tabla de compatibilidad, con las fuentes y la marca "VERIFICADO" o "PENDIENTE, se verifica en la Tarea 3.1";
  - el editor 6000.6.6f1, presente pero no usado.

### Tarea 0.2: iPhone listo para desarrollo

**Archivos:**
- Crear: `scripts/device.env.example`. El `scripts/device.env` real queda en `.gitignore`.

- [ ] **Paso 1:** el **PO** conecta el iPhone 16 Pro por cable, confía en el Mac y activa el **Modo de desarrollador** (Ajustes → Privacidad y seguridad). Esto requiere reiniciar el iPhone.
- [ ] **Paso 2:** ejecutar `xcrun devicectl list devices`. Esperado: el iPhone aparece con estado `available (paired)`. Anotar su identificador.
- [ ] **Paso 3:** el **PO** entrega el **Team ID** de Apple Developer y confirma el **bundle id**. Propuesta de Claude: `cl.rafaallendes.squashclash`.
- [ ] **Paso 4:** crear `scripts/device.env.example`:

```bash
DEVICE_ID=00000000-0000000000000000   # xcrun devicectl list devices
TEAM_ID=XXXXXXXXXX                     # Apple Developer Team ID
BUNDLE_ID=cl.rafaallendes.squashclash
UNITY_VERSION=6000.3.NNf1
```

---

## Etapa 1 — Repositorio Git/GitHub y estructura

**Qué hace Codex:** repositorio local, archivos base y repositorio remoto privado.
**Cómo se verifica:** `git status` limpio, y `gh repo view` muestra el repo privado con la documentación.
**En el iPhone:** nada.
**Para avanzar:** el PO ve el repo en GitHub, con `docs/` y sin `.superpowers/`.

### Tarea 1.1: Inicializar el repositorio

**Archivos:**
- Crear: `.gitignore`, `.gitattributes`, `README.md` y `AGENTS.md`

- [ ] **Paso 1:** `git init -b main` en la raíz.
- [ ] **Paso 2:** crear `.gitignore` a partir de la plantilla oficial de Unity de GitHub (`Library/`, `Temp/`, `Obj/`, `Build/`, `Builds/`, `Logs/`, `UserSettings/`, `MemoryCaptures/`, `*.csproj`, `*.sln`, `.vs/`, `.idea/`), más `.DS_Store`, `.superpowers/` y `scripts/device.env`.
- [ ] **Paso 3:** crear `.gitattributes` con `* text=auto` y `*.cs`, `*.md`, `*.json`, `*.unity`, `*.asset`, `*.meta`, `*.prefab` y `*.mat` como `text eol=lf`. **Sin Git LFS:** en First Ball no hay recursos binarios grandes.
- [ ] **Paso 4:** crear `README.md`: una línea del proyecto, enlaces a la spec y al plan, y cómo ejecutar `scripts/test.sh` y `scripts/build-ios.sh`.
- [ ] **Paso 5:** crear `AGENTS.md` con las reglas para Codex:
  - una tarea del plan por rama `firstball/<n.m>-<slug>` y por PR;
  - TDD con `scripts/test.sh`;
  - nada fuera del alcance de la tarea;
  - no modificar `docs/superpowers/specs/`;
  - no tocar umbrales de pruebas para que pasen;
  - documentación en español y código en inglés.
- [ ] **Paso 6:** comprobar que no se versiona `.superpowers/`. `git status --porcelain | grep -c superpowers/brainstorm` debe imprimir `0`.
- [ ] **Paso 7:** hacer commit:

```bash
git add .gitignore .gitattributes README.md AGENTS.md docs
git commit -m "chore: initialize repository with specs, reviews and plan"
```

- [ ] **Paso 8:** crear el remoto. **Es una acción hacia fuera: Codex la confirma con el PO justo antes de ejecutarla.**

```bash
gh repo create squash-clash --private --source . --push
gh repo view --json visibility,name
```
Esperado: `"visibility":"PRIVATE"`.

---

## Etapa 2 — Proyecto Unity, configuración iOS y pipeline

**Qué hace Codex:** el proyecto Unity, los ensamblados, la configuración vertical para iOS y los scripts de prueba y build.
**Cómo se verifica:**
- `scripts/test.sh editmode` y `scripts/test.sh playmode` pasan con un test de humo cada uno;
- `scripts/build-ios.sh dev` genera el proyecto Xcode y el `.app`.

**En el iPhone:** nada todavía. La instalación es la Etapa 3.
**Para avanzar:** los dos scripts terminan con código 0 desde la ruta real con espacios.

### Tarea 2.1: Crear el proyecto y configurarlo

**Archivos:**
- Crear: el proyecto Unity en la raíz (`Assets/`, `Packages/`, `ProjectSettings/`), los cuatro `.asmdef` del árbol de arriba más los dos de tests, `Assets/SquashClash/Editor/ProjectSetup.cs`, `Assets/SquashClash/Scenes/Main.unity`, `Assets/SquashClash/Tests/EditMode/SmokeTests.cs` y `Assets/SquashClash/Tests/PlayMode/SmokePlayTests.cs`.

**Interfaces:**
- Produce: `ProjectSetup.Apply()`, invocable con `-executeMethod SquashClash.EditorTools.ProjectSetup.Apply`. Es idempotente.

- [ ] **Paso 1:** crear el proyecto vacío. Unity no admite crearlo sobre una carpeta que no está vacía, así que se crea en una carpeta temporal y se mueven `Assets/`, `Packages/` y `ProjectSettings/` a la raíz:

```bash
source scripts/unity-env.sh   # ver Tarea 2.2: define $UNITY a partir de UNITY_VERSION
"$UNITY" -batchmode -quit -createProject "$PWD/.tmp-unity" -logFile Logs/create.log
mv .tmp-unity/Assets .tmp-unity/Packages .tmp-unity/ProjectSettings . && rm -rf .tmp-unity
```

- [ ] **Paso 2:** agregar a `Packages/manifest.json` los paquetes `com.unity.render-pipelines.universal`, `com.unity.inputsystem` y `com.unity.test-framework`, con **la versión que el editor 6000.3 propone por defecto**. Esa versión queda fijada en el manifest.
- [ ] **Paso 3:** implementar `ProjectSetup.Apply()`. Deja el proyecto en este estado:
  - Asset Serialization en `ForceText` y Visible Meta Files;
  - Active Input Handling en `Input System Package (New)`;
  - URP con un Pipeline Asset y un Renderer asignados en Graphics y en Quality;
  - iOS:
    - bundle id tomado de la variable de entorno `BUNDLE_ID`;
    - IL2CPP y ARM64;
    - orientación por defecto `Portrait`, sin autorrotación a horizontal;
    - Target minimum iOS: el valor por defecto del editor;
  - `Physics.simulationMode = SimulationMode.Script`;
  - `Physics.bounceThreshold = 0.1f`, porque el valor por defecto de 2 m/s anula los botes lentos;
  - `Physics.defaultContactOffset = 0.001f`, porque el de 1 cm adelanta el contacto;
  - Enhanced Determinism activado;
  - `Main.unity` como única escena del build.
- [ ] **Paso 4:** crear los asmdef:
  - `SquashClash.Core` sin referencias.
  - `SquashClash.Runtime` → Core, Unity.InputSystem.
  - `SquashClash.DevPanel` → Core, Runtime, Unity.InputSystem, con `defineConstraints: ["SQUASH_DEV"]`.
  - `SquashClash.Editor` → Core, Runtime, con plataforma Editor.
  - `SquashClash.Core.Tests` → Core, solo Editor, con TestAssemblies.
  - `SquashClash.Runtime.Tests` → Core, Runtime, con TestAssemblies.
- [ ] **Paso 5:** escribir los tests de humo:

```csharp
public class SmokeTests { [Test] public void CoreAssemblyLoads() => Assert.Pass(); }
public class SmokePlayTests { [UnityTest] public IEnumerator PlayModeRuns() { yield return null; Assert.Pass(); } }
```

- [ ] **Paso 6:** aplicar y probar con `"$UNITY" -batchmode -quit -projectPath "$PWD" -executeMethod SquashClash.EditorTools.ProjectSetup.Apply`, y después `scripts/test.sh editmode` y `scripts/test.sh playmode`. Esperado: `RESULT: passed=1 failed=0` en cada uno.
- [ ] **Paso 7:** hacer commit con `chore: create Unity 6.3 LTS project with portrait iOS settings`.

### Tarea 2.2: Scripts de entorno, pruebas y build

**Archivos:**
- Crear: `scripts/unity-env.sh`, `scripts/test.sh`, `scripts/build-ios.sh` y `Assets/SquashClash/Editor/BuildScript.cs`
- Crear: `Assets/SquashClash/Core/Diagnostics/BuildInfo.cs`

**Interfaces:**
- Produce: `BuildScript.BuildIosDevelopment()` y `BuildScript.BuildIosProduction()`, invocables con `-executeMethod`.
- Produce: `[Serializable] public sealed class BuildInfo { public string version; public string gitHash; public bool development; public string builtAtUtc; public static BuildInfo FromJson(string json); }`.
- Produce: `scripts/build-ios.sh dev|prod`, que deja el `.app` en `Builds/DerivedData/Build/Products/{Debug|Release}-iphoneos/`.

- [ ] **Paso 1:** `scripts/unity-env.sh`:
  - carga `scripts/device.env`;
  - define `UNITY="/Applications/Unity/Hub/Editor/$UNITY_VERSION/Unity.app/Contents/MacOS/Unity"`;
  - define `ROOT` como la raíz del repo;
  - si `UNITY_VERSION` no coincide con `ProjectSettings/ProjectVersion.txt`, falla con un mensaje claro.

  **Todas las rutas van entre comillas.**
- [ ] **Paso 2:** `scripts/test.sh editmode|playmode [filtro]`:
  - ejecuta `"$UNITY" -batchmode -projectPath "$ROOT" -runTests -testPlatform EditMode|PlayMode -testResults "$ROOT/Logs/<plataforma>-results.xml" -logFile "$ROOT/Logs/<plataforma>.log" [-testFilter <filtro>]`;
  - EditMode lleva además `-nographics`;
  - imprime `RESULT: passed=N failed=M` a partir del XML y sale con código 1 si M > 0.
- [ ] **Paso 3:** `BuildScript`:
  - escribe `Assets/SquashClash/Runtime/Resources/BuildInfo.json` (versión, `git rev-parse --short HEAD`, flag de desarrollo y hora UTC);
  - define `SQUASH_DEV` solo en desarrollo;
  - compila iOS con `BuildOptions.Development` (desarrollo) o `None` (producción), con salida en `Builds/iOS-dev` o `Builds/iOS-prod`;
  - al terminar, restaura los defines originales.
- [ ] **Paso 4:** `scripts/build-ios.sh dev|prod`:
  - Unity en modo batch con `-quit -executeMethod`;
  - después, `xcodebuild -project "<salida>/Unity-iPhone.xcodeproj" -scheme Unity-iPhone -configuration Debug|Release -destination "id=$DEVICE_ID" -derivedDataPath "$ROOT/Builds/DerivedData" -allowProvisioningUpdates DEVELOPMENT_TEAM="$TEAM_ID" CODE_SIGN_STYLE=Automatic build`;
  - se detiene en el primer error y muestra la ruta del log.
- [ ] **Paso 5:** verificar desde la ruta real (Review Focus 1) con `cd "/Users/rafaallendes/Projects/Squash Clash" && scripts/build-ios.sh dev`. Esperado: `** BUILD SUCCEEDED **` y un `.app` en `Builds/DerivedData/Build/Products/Debug-iphoneos/`.
  - **Si falla por la ruta con espacios,** se documenta el error en `docs/validation/environment.md`. Se crea el enlace `~/Projects/squash-clash` hacia la carpeta y los scripts se usan desde ahí. No se renombra la carpeta sin el PO.
  - **Si falla por la compatibilidad entre Xcode 27.1 y Unity,** se para y se pasa a la Tarea 3.1, paso 4.
- [ ] **Paso 6:** hacer commit con `build: add test and iOS build scripts with build info`.

---

## Etapa 3 — Primera compilación e instalación en el iPhone 16 Pro

**Qué hace Codex:** una escena mínima que muestra la información del build y los FPS, más el script de instalación. Resuelve el bloqueante B4 (compatibilidad Unity–Xcode).
**Cómo se verifica:** `scripts/install-ios.sh` instala y lanza la app, y la consola muestra la línea de arranque.
**En el iPhone:** fondo de color y, centrados, "Squash Clash — First Ball", la versión, el hash, "DEV" y los FPS. La app se queda en vertical aunque gires el teléfono.
**Para avanzar:** el PO confirma lo anterior. Se anotan en `environment.md` la combinación exacta que funcionó y cualquier ajuste necesario.

### Tarea 3.1: Escena de arranque e instalación

**Archivos:**
- Crear: `Assets/SquashClash/Runtime/App/AppBootstrap.cs` y `scripts/install-ios.sh`
- Modificar: `Assets/SquashClash/Scenes/Main.unity`, con la cámara y `AppBootstrap`

**Interfaces:**
- Produce: `public sealed class AppBootstrap : MonoBehaviour { public static AppBootstrap Instance { get; } public BuildInfo Build { get; } }`. En `Awake` fija `Application.targetFrameRate = 60` y registra `[SquashClash] boot <version> <gitHash> dev=<bool>`.

- [ ] **Paso 1:** implementar `AppBootstrap`. En esta tarea dibuja la etiqueta de arranque con un `UIDocument` que usa `Runtime/App/AppPanelSettings.asset`, creado aquí con el tema de runtime por defecto. El panel de la Etapa 6 reutiliza ese mismo PanelSettings.
- [ ] **Paso 2:** `scripts/install-ios.sh`:

```bash
xcrun devicectl device install app --device "$DEVICE_ID" "$APP_PATH"
xcrun devicectl device process launch --device "$DEVICE_ID" --console "$BUNDLE_ID"
```
Si `--console` no existe en Xcode 27.1, se registra y se usa `--terminate-existing` sin consola. Los logs se leen desde Console.app.
- [ ] **Paso 3:** `scripts/build-ios.sh dev && scripts/install-ios.sh`. Esperado: la consola muestra `[SquashClash] boot`.
- [ ] **Paso 4 (solo si el build o la instalación fallan por compatibilidad):** se prueban estas alternativas en orden, registrando cada una y **sin dar ninguna por buena sin instalarla**:
  1. el último parche de 6000.3;
  2. los ajustes de linker que Unity documente para Xcode 26 o posterior;
  3. instalar una versión de Xcode 26.x en paralelo, solo para compilar.

  Para elegir la tercera hace falta el OK del PO.
- [ ] **Paso 5:** el PO confirma lo que ve. Codex anota el resultado en `docs/validation/environment.md`, con la compatibilidad Unity–Xcode–iOS como "VERIFICADO" o con lo que falte.
- [ ] **Paso 6:** hacer commit con `feat: boot scene with build info and device install script`.

---

## Etapa 4 — Cancha y superficies

**Qué hace Codex:** la geometría WSF en Core (medidas, franjas de línea y clasificador de huella) y su vista.
**Cómo se verifica:** los tests de geometría y de bordes de línea pasan. La vista se revisa en el iPhone.
**En el iPhone:** una cancha completa en vertical:
- suelo de madera clara y paredes blancas;
- líneas rojas de 50 mm: out, saque, tin y las líneas del suelo;
- pared trasera de vidrio translúcido;
- dos cápsulas de referencia (azul para el humano y roja para el rival).

**Para avanzar:** el PO reconoce la cancha y la proporción de las líneas.

### Tarea 4.1: Geometría de la cancha y clasificador de líneas

**Archivos:**
- Crear: `Core/Court/SurfaceId.cs`, `CourtDimensions.cs`, `LineId.cs`, `LineBand.cs`, `CourtLines.cs` y `LineContactClassifier.cs`
- Test: `Tests/EditMode/Court/CourtGeometryTests.cs` y `LineContactClassifierTests.cs`

**Interfaces:**
- Produce:

```csharp
public enum SurfaceId { Floor, FrontWall, LeftWall, RightWall, BackWall }
public sealed class CourtDimensions {
  public float Length = 9.75f, Width = 6.40f, WallHeight = 6.0f, LineWidth = 0.05f;
  public float FrontOutLower = 4.57f, ServiceLower = 1.78f, BackOutLower = 2.13f, TinTop = 0.48f;
  public float ShortLineNearBackEdgeFromBack = 4.26f, ServiceBoxInner = 1.60f;
  public static CourtDimensions Standard();   // TinTop 0.48
  public static CourtDimensions Psa();        // TinTop 0.43
  public Vector2 ToSurfaceUV(SurfaceId s, Vector3 p);   // convención uv de este plan
}
public enum LineId { FrontOut, Service, Tin, BackOut, LeftSideOut, RightSideOut,
                     ShortLine, HalfCourt, LeftBoxSide, LeftBoxBack, RightBoxSide, RightBoxBack }
public readonly struct LineBand {             // franja = {a + t·(b−a) + s·growth | t∈[0,1], s∈[0,Width]}
  public readonly LineId Id; public readonly SurfaceId Surface;
  public readonly Vector2 A, B, Growth; public readonly float Width;
}
public static class CourtLines { public static IReadOnlyList<LineBand> Build(CourtDimensions d); }
public static class LineContactClassifier {
  public const float EdgeEpsilon = 1e-6f;     // 0,001 mm
  public static bool Touches(in LineBand band, Vector2 uv, float footprintRadius);
  public static void Overlapping(IReadOnlyList<LineBand> bands, SurfaceId s, Vector2 uv, float r, List<LineId> result);
}
```
Una huella "toca" una franja si la distancia del punto a la franja es ≤ r + `EdgeEpsilon` (regla de superposición cerrada, §8.7.6). `WallHeight = 6,0 m` es un supuesto de modelado: las paredes del juego, sin techo.

**Franjas que produce `CourtLines.Build(Standard())`** (verificadas o interpretadas según §8.7.2):

| LineId | Superficie | A → B (uv) | Growth | Width |
|---|---|---|---|---|
| FrontOut | FrontWall | (0, 4.57) → (6.40, 4.57) | (0, 1) | 0.05 |
| Service | FrontWall | (0, 1.78) → (6.40, 1.78) | (0, 1) | 0.05 |
| Tin | FrontWall | (0, 0.43) → (6.40, 0.43) | (0, 1) | 0.05 |
| BackOut | BackWall | (0, 2.13) → (6.40, 2.13) | (0, 1) | 0.05 |
| LeftSideOut / RightSideOut | Left/RightWall | (0, 4.57) → (9.75, 2.13) | (0, 1) | 0.05 |
| ShortLine | Floor | (0, 5.44) → (6.40, 5.44) | (0, 1) | 0.05 |
| HalfCourt | Floor | (3.175, 5.49) → (3.175, 9.75) | (1, 0) | 0.05 |
| LeftBoxSide | Floor | (1.60, 5.49) → (1.60, 7.14) | (1, 0) | 0.05 |
| LeftBoxBack | Floor | (0, 7.09) → (1.65, 7.09) | (0, 1) | 0.05 |
| RightBoxSide | Floor | (4.75, 5.49) → (4.75, 7.14) | (1, 0) | 0.05 |
| RightBoxBack | Floor | (4.75, 7.09) → (6.40, 7.09) | (0, 1) | 0.05 |

- [ ] **Paso 1: tests de geometría que fallan**

```csharp
[Test] public void Standard_UsesVerifiedWsfValues() {
  var d = CourtDimensions.Standard();
  Assert.AreEqual(9.75f, d.Length); Assert.AreEqual(6.40f, d.Width); Assert.AreEqual(0.05f, d.LineWidth);
  Assert.AreEqual(4.57f, d.FrontOutLower); Assert.AreEqual(1.78f, d.ServiceLower);
  Assert.AreEqual(2.13f, d.BackOutLower);  Assert.AreEqual(0.48f, d.TinTop);
  Assert.AreEqual(0.43f, CourtDimensions.Psa().TinTop);
}
[Test] public void ShortLine_SpansFrom5_44To5_49FromFrontWall() {
  var b = CourtLines.Build(CourtDimensions.Standard()).Single(l => l.Id == LineId.ShortLine);
  Assert.AreEqual(5.44f, b.A.y, 1e-5f); Assert.AreEqual(5.49f, b.A.y + b.Width, 1e-5f);
}
[Test] public void Build_Returns12Bands() => Assert.AreEqual(12, CourtLines.Build(CourtDimensions.Standard()).Count);
[Test] public void ToSurfaceUV_FollowsPlanConvention() {
  var d = CourtDimensions.Standard(); var p = new Vector3(1f, 2f, 3f);
  Assert.AreEqual(new Vector2(1f, 2f), d.ToSurfaceUV(SurfaceId.FrontWall, p));
  Assert.AreEqual(new Vector2(3f, 2f), d.ToSurfaceUV(SurfaceId.LeftWall, p));
  Assert.AreEqual(new Vector2(1f, 3f), d.ToSurfaceUV(SurfaceId.Floor, p));
}
```

- [ ] **Paso 2:** `scripts/test.sh editmode CourtGeometryTests`. Esperado: FAIL, porque los tipos no existen.
- [ ] **Paso 3:** implementar `CourtDimensions`, `LineBand` y `CourtLines` con los valores de la tabla.
- [ ] **Paso 4: tests de bordes que fallan** (§8.7.6). La huella es de 10 mm. Cada caso se escribe con `[TestCase]`:

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
| Floor | (1.0, 5.430) | 0.010 | sí | ShortLine |
| Floor | (1.0, 5.4289) | 0.010 | no | ShortLine |
| Floor | (1.0, 5.5011) | 0.010 | no | ShortLine |
| LeftWall | (4.875, 3.340) | 0.010 | sí | LeftSideOut |
| LeftWall | (4.875, 3.329) | 0.010 | no | LeftSideOut |
| FrontWall | (3.2, 4.560) | 0.010 | no | Service (otra franja, sin falsos positivos) |

```csharp
[TestCase(SurfaceId.FrontWall, 3.2f, 4.559f, 0.010f, LineId.FrontOut, false)]  // ... una fila por caso de la tabla
public void Touches_RespectsClosedOverlapRule(SurfaceId s, float u, float v, float r, LineId id, bool expected) {
  var bands = CourtLines.Build(CourtDimensions.Standard()); var hits = new List<LineId>();
  LineContactClassifier.Overlapping(bands, s, new Vector2(u, v), r, hits);
  Assert.AreEqual(expected, hits.Contains(id));
}
```
La fila de la diagonal funciona así: en la mitad (z = 4,875), el borde inferior está en y = 3,35. Un punto a 0,010 por debajo, en y = 3,340, toca. Uno en y = 3,329 no toca.

- [ ] **Paso 5:** ejecutar, ver que falla, implementar `LineContactClassifier` (distancia de un punto a un paralelogramo) y ejecutar hasta que pase. Esperado: `RESULT: passed=… failed=0`.
- [ ] **Paso 6:** hacer commit con `feat(core): WSF court geometry and line footprint classifier`.

### Tarea 4.2: Vista de la cancha y figuras de referencia

**Archivos:**
- Crear: `Runtime/Court/CourtView.cs`, `Runtime/Physics/SurfaceTag.cs` y `Runtime/App/PlaceholderPlayers.cs`
- Test: `Tests/PlayMode/CourtViewTests.cs`

**Interfaces:**
- Consume: `CourtDimensions` y `CourtLines.Build`.
- Produce: `public sealed class SurfaceTag : MonoBehaviour { public SurfaceId Surface; }`.
- Produce: `CourtView.BuildVisuals(CourtDimensions d)`, que genera el suelo, cuatro paredes y 12 franjas como quads desplazados 1 mm hacia dentro de la cancha. **Solo crea elementos visuales, sin colisionadores:** los colisionadores son de la simulación (Tarea 5.3).
- Produce: `PlaceholderPlayers.SetPositions(Vector2 humanXZ, Vector2 rivalXZ, bool visible)`, con dos cápsulas de 1,80 m sin colisión.

- [ ] **Paso 1: test que falla**

```csharp
[UnityTest] public IEnumerator BuildVisuals_CreatesFiveSurfacesAndTwelveLines() {
  var go = new GameObject("court"); var view = go.AddComponent<CourtView>();
  view.BuildVisuals(CourtDimensions.Standard()); yield return null;
  Assert.AreEqual(5, go.GetComponentsInChildren<SurfaceTag>().Length);
  Assert.AreEqual(12, go.transform.Find("Lines").childCount);
  Assert.AreEqual(0, go.GetComponentsInChildren<Collider>().Length);
}
```

- [ ] **Paso 2:** ejecutarlo y ver que falla. Implementar con materiales URP Unlit:
  - suelo de madera clara y paredes blancas;
  - **todas las líneas de pared del mismo rojo y todas las del suelo del mismo rojo**, según la cláusula WSF 5.01.01;
  - pared trasera translúcida, con alfa 0,25 [EXPERIMENTO].

  Ejecutar hasta que pase.
- [ ] **Paso 3:** colocar `CourtView` y `PlaceholderPlayers` en `Main.unity`. Humano en (3,2; 5,5) y rival en (4,5; 6,5) por defecto. Compilar e instalar.
- [ ] **Paso 4:** hacer commit con `feat(runtime): court visuals with WSF lines and placeholder players`.

---

## Etapa 5 — Pelota y prueba técnica de física (secuencial, empezando por PhysX)

**Qué hace Codex:**
- el contrato de simulación, el reloj y el lanzador, en Core;
- la suite de escenarios S1–S10 con verificadores de invariantes, en Core;
- la implementación PhysX (P1), en Runtime;
- un informe automático.

**Cómo se verifica:** EditMode para el contrato y los verificadores. PlayMode con PhysX para la suite. El informe queda en `docs/reviews/`.
**En el iPhone:** nada nuevo hasta la Etapa 6, que conecta la pelota a la escena.
**Para avanzar:** Claude revisa el informe y **el PO decide la física** (§8.2.3): P1 se acepta, o se autoriza la Tarea 5.5 (P2). Ningún umbral se cambia para que la suite pase.

### Tarea 5.1: Contrato de simulación, reloj y lanzador

**Archivos:**
- Crear: `Core/Simulation/BallState.cs`, `ContactEvent.cs`, `SurfaceMaterial.cs`, `PhysicsParams.cs`, `IBallSimulation.cs`, `Trajectory.cs`, `TrajectoryRunner.cs`, `SimClock.cs`, `LaunchSpec.cs` y `Launcher.cs`
- Test: `Tests/EditMode/Simulation/SimClockTests.cs`, `LauncherTests.cs` y `TrajectoryRunnerTests.cs`

**Interfaces:**
- Produce:

```csharp
public struct BallState { public Vector3 Position; public Vector3 Velocity; public long Step; }
public readonly struct ContactEvent {
  public readonly SurfaceId Surface; public readonly Vector3 Point; public readonly Vector2 UV;
  public readonly long Step; public readonly Vector3 VelocityIn, VelocityOut;
}
public sealed class SurfaceMaterial { public float Restitution; public float TangentialFriction; }
public sealed class PhysicsParams {
  public float Gravity = 9.81f, BallRadius = 0.020f, BallMass = 0.024f;
  public int StepHz = 120, MaxImpactsPerStep = 4;
  public SurfaceMaterial Floor, FrontWall, SideWalls, BackWall;   // valores de FirstBallParams (Tarea 6.1)
  public float StepSeconds => 1f / StepHz;
  public PhysicsParams Clone();
}
public interface IBallSimulation : IDisposable {
  void Reset(in BallState initial);                 // pone estado; no cambia parámetros
  BallState State { get; }
  void Step(List<ContactEvent> contactsThisStep);  // avanza exactamente un paso fijo
}
public interface IBallSimulationFactory {
  string Name { get; }
  IBallSimulation Create(PhysicsParams p, CourtDimensions d);   // los parámetros se fijan al crear
}
public enum StopCondition { SecondFloorBounce, MaxSteps }
public sealed class Trajectory { public readonly List<BallState> States = new(); public readonly List<ContactEvent> Contacts = new(); public bool LeftCourt; }
public static class TrajectoryRunner {
  public static Trajectory Run(IBallSimulation sim, in BallState from, int maxSteps, StopCondition stop, CourtDimensions d);
}
public sealed class SimClock {
  public const int MaxStepsPerFrame = 8;
  public SimClock(int stepHz, float tempo);
  public float Tempo { get; set; }             // τ
  public float DebugSlowMotion { get; set; }   // 1 = apagada; <1 marca MetricsInvalid
  public bool Paused { get; set; }
  public bool MetricsInvalid { get; }
  public long DroppedSteps { get; }
  public float Alpha { get; }                  // interpolación de render en [0,1)
  public void RequestSingleStep();
  public int Advance(double realDeltaSeconds);  // pasos a ejecutar en este frame
}
public readonly struct LaunchSpec { public readonly Vector3 Position, Direction; public readonly float SpeedRealMps; }
public static class Launcher {
  public static BallState ToState(in LaunchSpec spec, float launchScale);   // v = dir.normalized·speed·λ
  public static Vector3 SolveToTarget(Vector3 from, Vector3 target, float horizontalSpeed, float gravity); // balística sin colisiones
}
```

`TrajectoryRunner` termina en el segundo bote en el suelo (`SecondFloorBounce`), en `maxSteps`, o si la pelota supera `WallHeight` (en ese caso `LeftCourt = true`).

- [ ] **Paso 1: tests que fallan**

```csharp
[Test] public void Advance_Tempo1_SixtiethSecond_TwoSteps() => Assert.AreEqual(2, new SimClock(120, 1f).Advance(1.0 / 60));
[Test] public void Advance_Tempo065_OneSecond_78Steps() => Assert.AreEqual(78, new SimClock(120, 0.65f).Advance(1.0));
[Test] public void Advance_AccumulatesFractions() {
  var c = new SimClock(120, 1f); Assert.AreEqual(0, c.Advance(0.004)); Assert.AreEqual(0, c.Advance(0.004)); Assert.AreEqual(1, c.Advance(0.004));
}
[Test] public void Advance_HugeDelta_CapsStepsAndCountsDropped() {   // Review Focus 2
  var c = new SimClock(120, 1f); Assert.AreEqual(SimClock.MaxStepsPerFrame, c.Advance(5.0));
  Assert.AreEqual(600 - SimClock.MaxStepsPerFrame, c.DroppedSteps); Assert.AreEqual(0, c.Advance(0.0));
}
[Test] public void Paused_SingleStep_RunsExactlyOne() {
  var c = new SimClock(120, 1f) { Paused = true }; Assert.AreEqual(0, c.Advance(1.0));
  c.RequestSingleStep(); Assert.AreEqual(1, c.Advance(0.0)); Assert.AreEqual(0, c.Advance(1.0));
}
[Test] public void SlowMotion_MarksMetricsInvalid() {
  var c = new SimClock(120, 1f); Assert.IsFalse(c.MetricsInvalid); c.DebugSlowMotion = 0.5f; Assert.IsTrue(c.MetricsInvalid);
}
[Test] public void ToState_AppliesLaunchScale() {
  var s = Launcher.ToState(new LaunchSpec(Vector3.one, new Vector3(0, 0, -2), 10f), 1.2f);
  Assert.AreEqual(new Vector3(0, 0, -12f), s.Velocity); Assert.AreEqual(0, s.Step);
}
[Test] public void SolveToTarget_ReachesTargetAnalytically() {
  Vector3 from = new(3.2f, 1f, 7f), to = new(1f, 0.53f, 0f); var v = Launcher.SolveToTarget(from, to, 8f, 9.81f);
  float t = Vector2.Distance(new Vector2(from.x, from.z), new Vector2(to.x, to.z)) / 8f;
  var p = from + v * t + 0.5f * new Vector3(0, -9.81f, 0) * t * t; Assert.Less(Vector3.Distance(p, to), 1e-4f);
}
```
Además, `TrajectoryRunnerTests` usa una simulación falsa (`FakeFloorBounceSimulation`, definida en el propio archivo de test). Esa simulación rebota en el suelo cada 10 pasos. El test comprueba que `Run(..., SecondFloorBounce)` se detiene en el paso 20 con dos contactos.

- [ ] **Paso 2:** ejecutar. Esperado: FAIL.
- [ ] **Paso 3:** implementar.
  - `Advance` acumula `realDelta · Tempo · DebugSlowMotion · StepHz` en un `double` y obtiene los pasos con `floor(acumulado + 1e-6)`, para que 0,65 × 120 dé 78 aunque `0.65f` no sea exacto. Si en pausa hay un paso pendiente, devuelve 1. Si el resultado supera `MaxStepsPerFrame`, descarta el resto, lo suma a `DroppedSteps` y deja el acumulador en 0.
  - `MetricsInvalid` es *pegajoso*: una vez activado, no vuelve a falso en toda la sesión.
- [ ] **Paso 4:** ejecutar. Esperado: PASS.
- [ ] **Paso 5:** hacer commit con `feat(core): simulation contract, sim clock and launcher`.

### Tarea 5.2: Escenarios S1–S10, invariantes y suite

**Archivos:**
- Crear: `Core/Spike/PhysicsScenario.cs`, `PhysicsScenarios.cs`, `PhysicsInvariants.cs`, `SpikeResult.cs`, `PhysicsSpikeSuite.cs`, `SpikeReportWriter.cs` y `PredictionBenchmark.cs`
- Test: `Tests/EditMode/Spike/PhysicsInvariantsTests.cs` y `SpikeReportWriterTests.cs`

**Interfaces:**
- Consume: lo producido por la Tarea 5.1, `CourtDimensions` y `LineContactClassifier`.
- Produce:

```csharp
public sealed class PhysicsScenario { public string Id; public string Description; public BallState Start; public int MaxSteps; public StopCondition Stop; public Vector3? TargetContact; }
public static class PhysicsScenarios {
  public static IReadOnlyList<PhysicsScenario> Fixed(CourtDimensions d, PhysicsParams p);   // S1–S7, S9, S10
  public static IEnumerable<PhysicsScenario> Random(CourtDimensions d, int count, int seed); // S8
}
public static class PhysicsInvariants {
  public static bool NoTunnelling(Trajectory t, CourtDimensions d, float radius, float tol, out string detail);
  public static bool SpeedNeverIncreasesOnContact(Trajectory t, float tol, out string detail);
  public static bool NoJitter(Trajectory t, float minNormalSpeed, out string detail);
  public static bool BitwiseEqual(IReadOnlyList<BallState> a, IReadOnlyList<BallState> b, out int firstDiffIndex);
  public static (float cm, float ms) DeviationAtSecondBounce(Trajectory reference, Trajectory other, float stepSeconds);
}
public enum Verdict { Pass, Fail, Info }
public sealed class CriterionResult { public string Criterion; public Verdict Verdict; public string Measured; public string Threshold; }
public sealed class SpikeResult { public string Backend; public string BuildInfo; public string Device; public List<CriterionResult> Criteria = new(); }
public static class PhysicsSpikeSuite {
  public static SpikeResult Run(IBallSimulationFactory f, PhysicsParams p, CourtDimensions d, int randomCount = 10000, int seed = 20261020);
}
public static class SpikeReportWriter { public static string ToMarkdown(SpikeResult r); }
public static class PredictionBenchmark { public static (double medianMs, double p95Ms) Measure(IBallSimulationFactory f, PhysicsParams p, CourtDimensions d, int runs = 200); }
```

**Escenarios fijos.** Las velocidades son "reales", con λ = 1. El arranque se calcula con `Launcher.SolveToTarget` cuando hay un objetivo.

| Id | Descripción (§8.2.2) | Inicio | Objetivo o dirección | Velocidad |
|---|---|---|---|---|
| S1 | Drive paralelo rápido, 3 rebotes | (1.0, 0.9, 7.5) | frontis (0.6, 2.2, 0) | 28 m/s horizontal |
| S2 | Drop bajo, 5 cm sobre el tin | (4.5, 0.6, 3.0) | frontis (5.5, 0.53, 0) | 6 m/s horizontal |
| S3 | Rasante en la lateral (< 10°) | (0.8, 1.0, 8.0) | dirección (−sin 8°, 0, −cos 8°) | 20 m/s |
| S4 | Unión entre la lateral y el suelo (nick) | (1.5, 1.0, 4.0) | centro (r, r, 2.5) | 12 m/s horizontal |
| S5 | Frontis y lateral cerca de la esquina frontal | (2.0, 1.5, 3.0) | centro (r + 0.005, 1.5, r) | 15 m/s horizontal |
| S6 | Dos contactos en un paso (esquina trasera) | (3.2, 1.0, 6.0) | centro (6.40 − r, 1.0, 9.75 − r) | 25 m/s horizontal |
| S7 | Sale de la pared trasera | (3.2, 1.2, 6.0) | dirección (0, 0.2, 1) | 15 m/s |
| S9 | Bordes de línea | (3.2, 1.0, 5.0) | frontis en y = 4.560, 0.490 y 1.770 (tres lanzamientos) | 15 m/s horizontal |
| S10 | La pelota se asienta (Review Focus 5) | (3.2, 0.3, 5.0) | dirección (0, −1, 0) | 1 m/s, 6 s simulados |

**S8:** `count` lanzamientos con semilla 20261020:
- posición uniforme en [0,3; 6,1] × [0,3; 2,5] × [0,3; 9,45];
- dirección uniforme en la esfera;
- rapidez uniforme en [2; 35] m/s;
- 3 s simulados cada uno.

**Lo que `Run` evalúa** (una fila por criterio en `SpikeResult`):
1. **Robustez:** `NoTunnelling` en S1–S10 y en todo S8, con tolerancia de 0,1 mm. Se pasa con **0 fallos**.
2. **Esquinas:** `SpeedNeverIncreasesOnContact` (tolerancia de 1e-5 m/s) y `NoJitter` en S4, S5 y S6. NoJitter falla si la misma superficie recibe contactos en pasos consecutivos con una rapidez normal mayor que 0,5 m/s.
3. **Consistencia:** cada escenario fijo, ejecutado dos veces con instancias nuevas, da resultados `BitwiseEqual`.
4. **Fidelidad de la predicción:**
   - se crea una instancia "juego" que corre S1 y S6;
   - en el paso k = 30 se crea una instancia "predicción", se hace `Reset(estado del juego en k)` y se corre hasta el segundo bote;
   - se registran `BitwiseEqual` a partir de k (criterio **invariante**) y `DeviationAtSecondBounce` (umbral ≤ 1 cm y ≤ 2 ms, informativo si ya hay igualdad bit a bit);
   - además, la misma instancia de predicción se reutiliza tres veces seguidas con `Reset`, para detectar estado interno residual.
5. **Precisión del contacto (S9):** el UV del contacto con el frontis queda a ≤ 1 mm del objetivo [EXPERIMENTO]. `LineContactClassifier` con r = 0,010 da FrontOut, Tin y Service, respectivamente.
6. **Asentamiento (S10):** no hay NaN, hay como máximo 200 contactos y la pelota termina con rapidez < 0,05 m/s o sigue rodando sin contactos repetidos más de una vez por paso.
7. **Impactos por paso:** máximo observado frente a `MaxImpactsPerStep`. Es informativo para P1, porque PhysX no permite imponerlo.
8. **Rendimiento en el editor:** `PredictionBenchmark`. Es informativo. El decisivo es el del iPhone (Tarea 6.4).
9. **Facilidad de prueba:** "PlayMode requerido = sí o no". Es informativo, preferencia de §8.2.2.

- [ ] **Paso 1: tests de los verificadores que fallan.** Usan simulaciones falsas definidas en el test:

```csharp
[Test] public void NoTunnelling_DetectsBallBeyondFrontWall() {
  var t = new Trajectory(); t.States.Add(new BallState { Position = new Vector3(3.2f, 1f, 0.5f) });
  t.States.Add(new BallState { Position = new Vector3(3.2f, 1f, -0.3f) });
  Assert.IsFalse(PhysicsInvariants.NoTunnelling(t, CourtDimensions.Standard(), 0.02f, 1e-4f, out _));
}
[Test] public void SpeedNeverIncreases_DetectsGain() { /* contacto con |VelocityOut| = |VelocityIn| + 0.01 → false */ }
[Test] public void NoJitter_DetectsRepeatedContactsOnConsecutiveSteps() { /* misma superficie en los pasos 10 y 11 con vn = 1 m/s → false */ }
[Test] public void BitwiseEqual_FindsFirstDifferingIndex() { /* listas iguales salvo un float en el índice 7 → false, índice 7 */ }
[Test] public void DeviationAtSecondBounce_ReportsCmAndMs() { /* segundo bote a 2 cm y 3 pasos de diferencia a 120 Hz → (2.0, 25.0) */ }
[Test] public void Random_IsReproducibleForSeed() => CollectionAssert.AreEqual(
  PhysicsScenarios.Random(CourtDimensions.Standard(), 50, 7).Select(s => s.Start.Velocity).ToList(),
  PhysicsScenarios.Random(CourtDimensions.Standard(), 50, 7).Select(s => s.Start.Velocity).ToList());
[Test] public void ToMarkdown_ListsEveryCriterionWithVerdict() { /* 2 criterios → tabla con 2 filas y PASS/FAIL */ }
```

- [ ] **Paso 2:** ejecutar. Esperado: FAIL.
- [ ] **Paso 3:** implementar los verificadores, los escenarios, `Run`, `ToMarkdown` y `PredictionBenchmark`. El benchmark usa `System.Diagnostics.Stopwatch`: cada medición es un `TrajectoryRunner.Run` hasta el segundo bote de S1, con una instancia reutilizada.
- [ ] **Paso 4:** ejecutar. Esperado: PASS.
- [ ] **Paso 5:** hacer commit con `feat(core): physics spike scenarios, invariants and report`.

### Tarea 5.3: P1, simulación con PhysX

**Archivos:**
- Crear: `Runtime/Physics/PhysXBallSimulation.cs` y `PhysXBallSimulationFactory.cs`
- Test: `Tests/PlayMode/PhysXSpikeTests.cs`

**Interfaces:**
- Consume: `IBallSimulation`, `IBallSimulationFactory`, `PhysicsParams`, `CourtDimensions`, `SurfaceTag` y `PhysicsSpikeSuite`.
- Produce: `public sealed class PhysXBallSimulationFactory : IBallSimulationFactory { public string Name => "P1-PhysX"; }`.

**Diseño:**
- **Cada instancia crea su propia escena** con `SceneManager.CreateScene(nombre, new CreateSceneParameters(LocalPhysicsMode.Physics3D))` y la avanza con `scene.GetPhysicsScene().Simulate(p.StepSeconds)`. La predicción es simplemente **otra instancia**: es la "escena física separada" de §8.2.
- **Colisionadores:** cinco `BoxCollider` de 1 m de espesor fuera de los planos, de modo que las caras interiores quedan en x = 0, x = 6,40, y = 0, z = 0 y z = 9,75, con altura `WallHeight`. Cada uno lleva un `SurfaceTag` y un `PhysicsMaterial` con `bounciness = Restitution`, `dynamic/staticFriction = TangentialFriction` y combinación `Multiply`. La pelota tiene un material con bounciness 1 y friction 1.
- **Pelota:**
  - `SphereCollider` con radio 0,020 y `Rigidbody` con masa 0,024;
  - `linearDamping = 0`, `FreezeRotation` (sin spin, §8.3.4);
  - `CollisionDetectionMode.ContinuousDynamic`, la gravedad de `Physics.gravity = (0, −Gravity, 0)` y sin interpolación.
- **Contactos:** un `MonoBehaviour` en la pelota registra `OnCollisionEnter`, con superficie, punto y UV. `VelocityIn` es la velocidad antes de `Simulate` y `VelocityOut` la de después. `OnCollisionStay` se ignora.
- **Reset:** fija la posición y la velocidad del Rigidbody y anula la velocidad angular.

- [ ] **Paso 1:** crear los tests de PlayMode. Cada criterio de `PhysicsSpikeSuite.Run(new PhysXBallSimulationFactory(), …)` es un test con su `Assert`: `S8_NoTunnelling`, `Corners_SpeedNeverIncreases`, `Corners_NoJitter`, `Consistency_Bitwise`, `Prediction_Bitwise`, `Prediction_WithinThreshold`, `S9_ContactPrecision`, `S10_Settle_NoNaNAndBoundedContacts`. Un test más, `WriteReport`, siempre pasa y escribe `Logs/physics-spike-P1.md` con `SpikeReportWriter`.
- [ ] **Paso 2:** ejecutar `scripts/test.sh playmode PhysXSpikeTests`. Esperado: FAIL, porque la fábrica no existe.
- [ ] **Paso 3:** implementar P1 según el diseño.
- [ ] **Paso 4:** ejecutar. **El resultado es información, no un objetivo.** Si un criterio falla, **solo se pueden probar los ajustes documentados de PhysX**:
  - `ContinuousDynamic` o `ContinuousSpeculative`;
  - `defaultContactOffset` en {0,001; 0,0005};
  - solver iterations;
  - Enhanced Determinism activado o no.

  Cada combinación probada queda en el informe. **No se cambian umbrales ni escenarios.**
- [ ] **Paso 5:** copiar el informe a `docs/reviews/2026-MM-DD-first-ball-physics-spike-P1.md`, con una sección de resumen: qué pasa, qué falla y qué ajustes se probaron.
- [ ] **Paso 6:** hacer commit con `feat(runtime): PhysX ball simulation (P1) and spike report`.

> **Gate de física (el resultado completo se cierra en la Tarea 6.4, con la medición en el iPhone).** Claude revisa el informe. Si P1 cumple todos los invariantes y umbrales, la regla del §8.2.3 lo elige. Si falla la igualdad bit a bit pero cumple ≤ 1 cm / ≤ 2 ms, **el PO decide** si acepta P1 como "predictor más grueso" o si autoriza P2. Claude registra la decisión en la spec (nueva entrada del registro).

### Tarea 5.4: Pelota en la escena (host de simulación y vista)

**Archivos:**
- Crear: `Runtime/Ball/BallSimulationHost.cs` y `BallView.cs`
- Modificar: `Main.unity`
- Test: `Tests/PlayMode/BallSimulationHostTests.cs`

**Interfaces:**
- Consume: `IBallSimulationFactory`, `SimClock`, `TrajectoryRunner`, `Launcher` y `PhysicsInvariants.BitwiseEqual`.
- Produce:

```csharp
public sealed class BallSimulationHost : MonoBehaviour {
  public void Configure(IBallSimulationFactory factory, PhysicsParams p, CourtDimensions d, SimClock clock);
  public void Launch(in LaunchSpec spec, float launchScale);   // recrea sims si cambió PhysicsParams; predice; arranca
  public void ResetBall();                                     // pelota quieta en el punto del lanzador
  public BallState Current { get; }        // estado de simulación (autoridad)
  public Vector3 RenderPosition { get; }   // interpolado: SOLO para dibujar
  public Trajectory Prediction { get; }    // calculada desde el estado de simulación al lanzar
  public bool PredictionMatchesBitwise { get; }   // comparación paso a paso con Prediction
  public float MaxPredictionDeviationCm { get; }
  public event Action<ContactEvent> ContactRaised;
}
```

- [ ] **Paso 1: tests que fallan**

```csharp
[UnityTest] public IEnumerator Launch_PredictionMatchesSimulationStepByStep() {
  // host con PhysX, clock tempo 1; Launch(S1); avanzar 240 pasos con Paused + RequestSingleStep
  Assert.IsTrue(host.PredictionMatchesBitwise);   // si falla en P1, el test lo informa: no se toca el umbral
}
[UnityTest] public IEnumerator Prediction_StartsFromSimulationState_NotRenderPosition() {
  // tras Launch: Prediction.States[0] es bit a bit igual a Current; RenderPosition puede diferir
}
[UnityTest] public IEnumerator Launch_RecreatesSimulationsOnlyWhenParamsChanged() { /* mismos params → misma instancia; Restitution cambiada → instancias nuevas */ }
```

- [ ] **Paso 2:** ejecutar y ver que falla. Implementar:
  - `Update` llama a `clock.Advance(Time.unscaledDeltaTime)` y ejecuta N pasos;
  - `RenderPosition = Lerp(previo, actual, clock.Alpha)`;
  - `BallView` dibuja una esfera del radio físico por `aids.ballVisualScale` en `RenderPosition`. Hasta la Tarea 6.1 la escala vale 1,0.

  Ejecutar hasta que pase.
- [ ] **Paso 3:** hacer commit con `feat(runtime): ball simulation host with prediction check and view`.

### Tarea 5.5 [CONDICIONAL: solo con la autorización del PO tras el gate de física]: P2, simulación propia analítica

**Archivos:**
- Crear: `Core/Simulation/AnalyticBallSimulation.cs`
- Test: `Tests/EditMode/Spike/AnalyticSpikeTests.cs`, la misma batería que `PhysXSpikeTests`, pero en EditMode.

**Interfaces:**
- Produce: `public sealed class AnalyticBallSimulationFactory : IBallSimulationFactory { public string Name => "P2-Analytic"; }`. Se conecta sin cambios al host, a la suite y al panel.

**Algoritmo (§8.3):**
- dentro de cada paso se calcula el **primer impacto** de la esfera contra los cinco planos, desplazados por el radio: cuadrático en `y` por la gravedad y lineal en `x` y `z`;
- se integra exactamente hasta ese instante;
- la componente normal se refleja multiplicada por `Restitution` y la tangencial se multiplica por `(1 − TangentialFriction)`;
- se consume el tiempo restante, hasta `MaxImpactsPerStep`;
- **dos impactos separados por menos de ε = 1e-6 s** se resuelven juntos, reflejando cada componente normal;
- la posición se integra de forma exacta (`p += v·dt + ½·g·dt²`), sin semi-implícito.

- [ ] **Pasos:** test que falla → implementación → `scripts/test.sh editmode AnalyticSpikeTests` → informe `…-physics-spike-P2.md` → commit `feat(core): analytic ball simulation (P2)`.

---

## Etapa 6 — Panel de desarrollador, presets y diagnóstico

*Se adelanta a la cámara (punto 8 antes que el 7 del pedido del PO) porque el lanzador, la calibración y la medición en el iPhone dependen del panel.*

**Qué hace Codex:**
- el registro de parámetros con sus clases de aplicación;
- los presets JSON;
- el panel autogenerado a partir del registro;
- el diagnóstico de FPS, predicción, contactos y temperatura;
- el log de métricas;
- la ejecución de la prueba de física y del benchmark **en el iPhone**.

**Cómo se verifica:** EditMode para el registro, los presets y las estadísticas. En el iPhone, el panel se abre y un preset sobrevive al reinicio de la app.
**En el iPhone:**
- se mantienen tres dedos 1 s, o se toca el botón "DEV", y aparece el panel;
- el PO elige un lanzamiento, toca "Lanzar" y ve la pelota rebotar;
- cambia la restitución y ve "1 cambio pendiente" hasta el siguiente lanzamiento;
- guarda un preset, cierra la app, la vuelve a abrir y el preset sigue activo.

**Para avanzar:**
- el PO calibra al menos un preset propio sin tocar código;
- la prueba de física en el iPhone produce su informe;
- se cumple el gate de física.

### Tarea 6.1: Registro de parámetros y presets

**Archivos:**
- Crear: `Core/Tuning/ApplyClass.cs`, `ParamDef.cs`, `ParamRegistry.cs`, `FirstBallParams.cs`, `PresetFile.cs`, `PresetCodec.cs`, `PresetStore.cs` y `LaunchPresets.cs`, y `Runtime/Resources/DefaultPreset.json`
- Test: `Tests/EditMode/Tuning/ParamRegistryTests.cs`, `PresetCodecTests.cs`, `PresetStoreTests.cs` y `LaunchPresetsTests.cs`

**Interfaces:**
- Produce:

```csharp
public enum ApplyClass { Immediate, NextLaunch }   // "próximo rally" = próximo lanzamiento en First Ball (§13.4)
public sealed class ParamDef { public string Key, Label, Unit, Group; public float Min, Max, Default, Step; public ApplyClass Apply; public string[] Options; }
public sealed class ParamRegistry {
  public void Register(ParamDef def);
  public IReadOnlyList<ParamDef> Defs { get; }
  public float Get(string key);                        // valor vigente
  public void Set(string key, float value);            // clamp; NextLaunch → queda pendiente
  public IReadOnlyCollection<string> PendingKeys { get; }
  public void ApplyPending();
  public event Action<string> Changed;                 // se dispara al aplicar
  public PresetFile ToPreset(string name, string build);
  public IReadOnlyList<string> LoadPreset(PresetFile f); // devuelve avisos; los valores NextLaunch quedan pendientes
}
public static class FirstBallParams {
  public static ParamRegistry CreateRegistry();
  public static PhysicsParams ToPhysics(ParamRegistry r);
  public static CourtDimensions ToCourt(ParamRegistry r);
  public static LaunchSpec ToLaunch(ParamRegistry r);
}
[Serializable] public sealed class PresetEntry { public string key; public float value; }
[Serializable] public sealed class PresetFile { public int schemaVersion = 1; public string name; public string createdBuild; public List<PresetEntry> values = new(); }
public static class PresetCodec {
  public const int SupportedSchema = 1;
  public static string ToJson(PresetFile f);
  public static bool TryFromJson(string json, out PresetFile f, out string error);  // nunca lanza
  public static string Hash(PresetFile f);   // SHA-256 de "key=value\n" ordenado por key, InvariantCulture "R"; primeros 12 hex
}
public sealed class PresetStore {
  public PresetStore(string directory);      // Application.persistentDataPath/presets en runtime
  public void Save(PresetFile f); public bool TryLoad(string name, out PresetFile f, out string error);
  public IReadOnlyList<string> List(); public string LastActiveName { get; set; }   // last-active.txt
}
public static class LaunchPresets {
  public sealed class Entry { public string Id, Label; public LaunchSpec Launch; public Vector2 HumanXZ, RivalXZ; }
  public static IReadOnlyList<Entry> All(CourtDimensions d);   // L1–L6 (§12.3) y S1–S7, S9
}
```

**Catálogo de `FirstBallParams`:** todos los valores son [EXPERIMENTO], para calibrar con el PO. La restitución y la fricción son puntos de partida de Claude, no datos medidos.

| Grupo | Clave | Defecto | Rango | Clase |
|---|---|---|---|---|
| Física | `physics.tempo` (τ) | 0.65 | 0.3–1.0 | NextLaunch |
| Física | `physics.launchScale` (λ) | 1.0 | 0.5–1.5 | NextLaunch |
| Física | `physics.gravity` | 9.81 | 5–15 | NextLaunch |
| Física | `physics.stepHz` | 120 | opciones 60/120/240 | NextLaunch |
| Física | `physics.maxImpactsPerStep` | 4 | 1–8 | NextLaunch |
| Física | `physics.floor.restitution` / `.friction` | 0.45 / 0.30 | 0–1 | NextLaunch |
| Física | `physics.front.restitution` / `.friction` | 0.55 / 0.20 | 0–1 | NextLaunch |
| Física | `physics.side.restitution` / `.friction` | 0.55 / 0.20 | 0–1 | NextLaunch |
| Física | `physics.back.restitution` / `.friction` | 0.55 / 0.20 | 0–1 | NextLaunch |
| Física | `physics.backend` | 0 (P1) | opciones según las fábricas disponibles | NextLaunch |
| Cancha | `court.tinTop` | 0.48 | opciones 0.48/0.43 | NextLaunch |
| Lanzador | `launcher.x` / `.y` / `.z` | 3.2 / 1.0 / 7.0 | dentro de la cancha | NextLaunch |
| Lanzador | `launcher.yawDeg` / `.pitchDeg` | 0 / 10 | −80–80 / −30–80 | NextLaunch |
| Lanzador | `launcher.speed` | 15 | 2–40 m/s | NextLaunch |
| Lanzador | `launcher.repeatSeconds` | 0 (apagado) | 0–10 | Immediate |
| Cámara | `camera.mode` | 0 (fija) | opciones fija/seguimiento | Immediate |
| Cámara | `camera.height` / `.backOffset` / `.lookAtHeight` | 4.5 / 2.0 / 1.6 | 2–8 / 0–5 / 0–3 | Immediate |
| Cámara | `camera.fovDeg` | 62 | 40–90 | Immediate |
| Cámara | `camera.followDamping` / `.followMaxOffset` | 0.3 / 1.5 | 0–1 / 0–3 | Immediate |
| Ayudas | `aids.shadow` / `.trail` / `.impactMarks` / `.prediction` | 1 / 1 / 1 / 0 | toggle | Immediate |
| Ayudas | `aids.trailSeconds` / `aids.ballVisualScale` | 0.15 / 1.0 | 0–0.5 / 1–2.5 | Immediate |
| Depuración | `debug.slowMotion` | 1.0 | 0.1–1.0 | Immediate |
| Referencias | `placeholders.visible`, `human.x/z`, `rival.x/z` | 1, 3.2/5.5, 4.5/6.5 | dentro de la cancha | Immediate |

`DefaultPreset.json` contiene exactamente estos valores. Al arrancar se carga `LastActiveName` si existe y es válido. Si no, se usa `DefaultPreset`.

- [ ] **Paso 1: tests que fallan**

```csharp
[Test] public void NextLaunchParam_StaysPendingUntilApply() {   // Review Focus 4
  var r = FirstBallParams.CreateRegistry(); r.Set("physics.floor.restitution", 0.6f);
  Assert.AreEqual(0.45f, r.Get("physics.floor.restitution")); CollectionAssert.Contains(r.PendingKeys, "physics.floor.restitution");
  r.ApplyPending(); Assert.AreEqual(0.6f, r.Get("physics.floor.restitution")); Assert.IsEmpty(r.PendingKeys);
}
[Test] public void ImmediateParam_AppliesAndNotifies() { /* camera.fovDeg = 70 → Get 70 y Changed recibió "camera.fovDeg" */ }
[Test] public void Set_ClampsToRange() { /* launcher.speed = 99 → 40 tras aplicar */ }
[Test] public void Codec_RoundTrip_PreservesValues() { /* ToJson → TryFromJson → mismos pares */ }
[Test] public void Hash_IndependentOfEntryOrder() { /* mismos pares en distinto orden → mismo Hash, de 12 caracteres */ }
[Test] public void TryFromJson_Malformed_ReturnsFalseWithoutThrowing() {   // Review Focus 3
  Assert.IsFalse(PresetCodec.TryFromJson("{not json", out _, out var err)); Assert.IsNotEmpty(err);
}
[Test] public void TryFromJson_NewerSchema_IsRejected() { /* schemaVersion 2 → false, el error menciona "schema" */ }
[Test] public void LoadPreset_UnknownKeyIgnored_OutOfRangeClamped_MissingKeepsDefault() {
  // valores: {"bogus.key":1, "launcher.speed":99}; avisos.Count == 2; launcher.speed → 40; physics.tempo sigue en 0.65
}
[Test] public void Store_SaveThenReload_RestoresLastActive() { /* directorio temporal; Save, LastActiveName; instancia nueva → mismo nombre y valores */ }
[Test] public void DefaultPresetJson_MatchesCatalogDefaults() { /* lee Assets/.../DefaultPreset.json; cada valor == Default */ }
[Test] public void LaunchPresets_AllStartInsideCourt() { /* cada Entry.Launch.Position dentro de la cancha, a radio de distancia de las paredes */ }
```

- [ ] **Paso 2:** ejecutar. Esperado: FAIL.
- [ ] **Paso 3:** implementar. `PresetCodec` usa `JsonUtility`, y para el hash `System.Security.Cryptography.SHA256`. Los L1–L6 de `LaunchPresets` corresponden a §12.3:
  - **L1:** drive largo hacia la pared trasera;
  - **L2:** pelota baja pegada a la lateral izquierda;
  - **L3:** drop corto, con segundo bote a menos de 2 m;
  - **L4:** globo alto, que sube por encima de 4 m;
  - **L5:** pelota que pasa detrás del humano, con el humano en (3,2; 6,0) y la trayectoria por z > 6,5;
  - **L6:** esquina trasera con el rival a 1 m.

  Cada uno se resuelve con `Launcher.SolveToTarget` y un objetivo en el frontis.
- [ ] **Paso 4:** ejecutar. Esperado: PASS.
- [ ] **Paso 5:** hacer commit con `feat(core): parameter registry, versioned presets and launch presets`.

### Tarea 6.2: Estadísticas de frame, log de métricas y temperatura

**Archivos:**
- Crear: `Core/Diagnostics/FrameTimeStats.cs` y `MetricsLog.cs`, `Runtime/App/ThermalState.cs` y `Plugins/iOS/SquashThermal.mm`
- Test: `Tests/EditMode/Diagnostics/FrameTimeStatsTests.cs` y `MetricsLogTests.cs`

**Interfaces:**
- Produce:

```csharp
public sealed class FrameTimeStats {
  public FrameTimeStats(int capacity = 3600);     // ~60 s a 60 FPS
  public void Add(float ms); public float Percentile(float p);   // nearest-rank
  public int CountOver(float ms); public double ElapsedMinutes { get; }
}
public sealed class MetricsLog {
  public MetricsLog(string directory, string presetName, string presetHash, string build, int seed);
  public void Write(string kind, string jsonPayload);   // una línea JSONL con ts, kind, preset, hash, build, seed, metricsValid
  public bool MetricsValid { get; set; }
  public string CurrentFilePath { get; }
}
public static class ThermalState { public static string Current(); }   // "nominal|fair|serious|critical|unknown"
```
`SquashThermal.mm` expone `extern "C" int SquashThermalState()`, que devuelve `[[NSProcessInfo processInfo] thermalState]`. En el editor, `Current()` devuelve `"unknown"`.

- [ ] **Paso 1: tests que fallan**

```csharp
[Test] public void Percentiles_NearestRank() {
  var s = new FrameTimeStats(); for (int i = 1; i <= 100; i++) s.Add(i);
  Assert.AreEqual(50f, s.Percentile(50)); Assert.AreEqual(95f, s.Percentile(95)); Assert.AreEqual(99f, s.Percentile(99));
  Assert.AreEqual(67, s.CountOver(33.3f));
}
[Test] public void MetricsLog_EveryLineCarriesPresetHashBuildSeedAndValidity() {
  // directorio temporal; Write("launch","{}"); MetricsValid=false; Write("contact","{}")
  // dos líneas; ambas con preset, hash, build y seed; la segunda con "metricsValid":false
}
```

- [ ] **Paso 2:** ejecutar, ver que falla, implementar y ejecutar hasta que pase.
- [ ] **Paso 3:** hacer commit con `feat(core): frame stats, metrics log and iOS thermal state`.

### Tarea 6.3: Arranque integrado y panel autogenerado

**Archivos:**
- Modificar: `Runtime/App/AppBootstrap.cs`
- Crear: `DevPanel/DevPanelBootstrap.cs`, `ThreeFingerHold.cs`, `DevPanelView.cs`, `ParamSection.cs` y `PresetsSection.cs`. Reutilizan `AppPanelSettings.asset` de la Tarea 3.1.
- Test: `Tests/PlayMode/AppBootstrapTests.cs`

**Interfaces:**
- Consume: `FirstBallParams`, `ParamRegistry`, `PresetStore`, `MetricsLog`, `BallSimulationHost`, `CourtView` y `PlaceholderPlayers`.
- Produce, en `AppBootstrap`: `public ParamRegistry Params { get; } public PresetStore Presets { get; } public MetricsLog Metrics { get; } public BallSimulationHost Ball { get; } public SimClock Clock { get; } public IReadOnlyList<IBallSimulationFactory> Backends { get; } public void LaunchCurrent(); public string ActivePresetName { get; }`.
- `LaunchCurrent()` sigue este orden: `Params.ApplyPending()`, después `Ball.Launch(FirstBallParams.ToLaunch(Params), λ)`, y por último escribe `"launch"` en `Metrics`.

- [ ] **Paso 1: tests que fallan**

```csharp
[UnityTest] public IEnumerator Boot_LoadsLastActivePreset_OrDefault() { /* sin last-active → "default"; con un preset guardado → ese nombre */ }
[UnityTest] public IEnumerator LaunchCurrent_AppliesPendingBeforeLaunching() { /* Set restitución → LaunchCurrent → PendingKeys vacío y Ball usó 0.6 */ }
[UnityTest] public IEnumerator FocusLoss_PausesClock() {   // Review Focus 2
  boot.SendMessage("OnApplicationPause", true); Assert.IsTrue(boot.Clock.Paused);
}
[UnityTest] public IEnumerator SlowMotion_InvalidatesMetrics() { /* debug.slowMotion = 0.5 → Metrics.MetricsValid == false */ }
```

- [ ] **Paso 2:** ejecutar, ver que falla, implementar `AppBootstrap` y ejecutar hasta que pase.
  - Al volver del segundo plano, el reloj **sigue en pausa** hasta que el PO toque "Continuar".
  - Los cambios Immediate de cámara y ayudas se propagan con `Params.Changed`.
- [ ] **Paso 3:** implementar el panel, solo en DevPanel, con UI Toolkit construida desde el código.
  - `DevPanelBootstrap` se crea con `[RuntimeInitializeOnLoadMethod(AfterSceneLoad)]` y agrega un botón "DEV" en la esquina superior derecha del `Screen.safeArea`.
  - `ThreeFingerHold` usa EnhancedTouch y abre o cierra el panel con tres dedos mantenidos 1 s (§13.1).
  - `ParamSection` genera un control por cada `ParamDef`, agrupado por `Group`: slider con valor y unidad, toggle o desplegable según `Options`.
  - El encabezado muestra "N cambios pendientes" y el botón **"Aplicar y relanzar"**.
  - **Barra fija** con "Lanzar", "Reiniciar", "Pausa/Continuar", "Paso" y el preset de lanzamiento (lista de `LaunchPresets`).
  - `PresetsSection` permite "Guardar como…", cargar desde la lista, "Exportar" (JSON al portapapeles con `GUIUtility.systemCopyBuffer`) e "Importar del portapapeles" con validación de `TryFromJson`.
  - El panel ocupa la mitad inferior y deja ver la cancha.
- [ ] **Paso 4:** compilar e instalar. Comprobar en el iPhone, en este orden: abrir con tres dedos, lanzar L1, cambiar la restitución del suelo (aparece "1 cambio pendiente"), relanzar, guardar "po-1", cerrar y abrir la app (el panel indica "po-1"), y exportar.
- [ ] **Paso 5:** hacer commit con `feat(devpanel): auto-generated tuning panel with presets and launcher`.

### Tarea 6.4: Diagnóstico y prueba de física en el iPhone

**Archivos:**
- Crear: `DevPanel/DiagnosticsSection.cs` y `SpikeSection.cs`

**Interfaces:**
- Consume: `FrameTimeStats`, `ThermalState`, `BallSimulationHost`, `PhysicsSpikeSuite`, `PredictionBenchmark` y `SpikeReportWriter`.

- [ ] **Paso 1:** `DiagnosticsSection` se actualiza cada 0,5 s y muestra:
  - FPS, p50, p95 y p99 del tiempo de frame, y frames > 33 ms por minuto;
  - temperatura del dispositivo;
  - `PredictionMatchesBitwise` y `MaxPredictionDeviationCm`;
  - los últimos 8 `ContactEvent` (superficie, UV y líneas tocadas con r = 0,010);
  - `DroppedSteps` y la marca "MÉTRICAS NO VÁLIDAS" si se usó la cámara lenta.

  Cada 10 s escribe un resumen `"frames"` en `MetricsLog`.
- [ ] **Paso 2:** `SpikeSection`:
  - el botón **"Ejecutar prueba de física"** corre `PhysicsSpikeSuite.Run` con el backend activo y 10 000 lanzamientos aleatorios, repartida en frames con una corrutina y con progreso visible;
  - el botón **"Medir predicción"** ejecuta `PredictionBenchmark.Measure` con 200 mediciones;
  - el resultado se muestra, se copia al portapapeles con `SpikeReportWriter` (incluidos el build y el dispositivo) y se escribe en `MetricsLog`.
- [ ] **Paso 3:** compilar e instalar. El PO o Codex ejecutan las dos acciones en el iPhone, y el informe del dispositivo se agrega a `docs/reviews/…-physics-spike-P1.md` como "Resultados en el iPhone 16 Pro".
- [ ] **Paso 4:** hacer commit con `feat(devpanel): diagnostics and on-device physics spike`.

> **Cierre del gate de física.** Con los resultados del editor y del iPhone, Claude propone la elección según §8.2.3 (la alternativa más simple que cumple todo) y el **PO decide**. Si se autoriza P2, se ejecuta la Tarea 5.5 y se repiten las Tareas 6.4 (paso 3) y 5.3 (paso 5) con P2.

---

## Etapa 7 — Cámara y legibilidad de la trayectoria

**Qué hace Codex:**
- una cámara fija de referencia y una variante de seguimiento amortiguado;
- ayudas de legibilidad: sombra siempre visible, estela corta, marcas de impacto en las paredes y, opcionalmente, la trayectoria predicha.

**Cómo se verifica:** con tests de PlayMode de posición y encuadre. La legibilidad la juzga el PO con L1–L6.
**En el iPhone:** desde detrás y por encima de la pared trasera se ven el frontis, las laterales y las dos figuras. La pelota tiene sombra en el suelo, una estela breve y deja una marca donde pega. En modo seguimiento, la cámara acompaña lateralmente con suavidad.
**Para avanzar:** H1. El PO distingue altura y profundidad **sin abrir el panel** en L1–L6, con al menos una de las dos cámaras (§12.3, §17.B).

### Tarea 7.1: Rig de cámara

**Archivos:**
- Crear: `Runtime/Camera/CourtCameraRig.cs`
- Test: `Tests/PlayMode/CourtCameraRigTests.cs`

**Interfaces:**
- Produce: `public sealed class CourtCameraRig : MonoBehaviour { public void Apply(ParamRegistry p, CourtDimensions d); public void Tick(Vector3 ballRenderPos, float dt); }`.
  - **Fija:** posición (W/2, `camera.height`, L + `camera.backOffset`), mirando hacia (W/2, `camera.lookAtHeight`, 0), con un FOV vertical de `camera.fovDeg`.
  - **Seguimiento:** desplaza la `x` de la cámara hacia la `x` de la pelota con un amortiguamiento crítico de `camera.followDamping`, limitado a ±`followMaxOffset`, y mantiene el mismo punto de mira desplazado.

- [ ] **Paso 1: tests que fallan**

```csharp
[UnityTest] public IEnumerator Fixed_DefaultFraming_ShowsFrontWallCornersAndBackFloorCorners() {
  // aspect 1206/2622 (iPhone 16 Pro en vertical); para (0,0,0), (6.4,0,0), (0,4.57,0), (6.4,4.57,0), (0,0,9.75), (6.4,0,9.75):
  // WorldToViewportPoint ∈ [0,1]²
}
[UnityTest] public IEnumerator Follow_StaysWithinMaxOffset() { /* pelota en x = 0 durante 5 s → |cam.x − 3.2| ≤ 1.5 */ }
[UnityTest] public IEnumerator Follow_DoesNotChangeFixedWhenModeIsFixed() { /* modo 0 y la pelota se mueve → la cámara no cambia */ }
```
Si el encuadre por defecto no cumple el primer test, se ajustan **solo** los valores por defecto de `camera.*` en `FirstBallParams` y en `DefaultPreset.json`, y se registra el cambio.

- [ ] **Paso 2:** ejecutar, ver que falla, implementar y ejecutar hasta que pase.
- [ ] **Paso 3:** hacer commit con `feat(runtime): fixed and damped-follow court camera`.

### Tarea 7.2: Ayudas de legibilidad

**Archivos:**
- Crear: `Runtime/Ball/LegibilityAids.cs`
- Test: `Tests/PlayMode/LegibilityAidsTests.cs`

**Interfaces:**
- Consume: `BallSimulationHost.RenderPosition`, `ContactRaised` y `Prediction`, y `ParamRegistry`.
- Produce: `LegibilityAids`.
  - **Sombra:** un disco oscuro en (x, 0,001, z) bajo la pelota, que se ve siempre, a cualquier altura. Su tamaño y su opacidad bajan con la altura [EXPERIMENTO].
  - **Estela:** un `TrailRenderer` de `aids.trailSeconds` en tiempo real.
  - **Marcas de impacto:** un disco plano en cada contacto con una pared, que se desvanece en 1,5 s, con un máximo de 6 a la vez.
  - **Predicción:** un `LineRenderer` con `Prediction.States`.

  Cada ayuda se enciende y se apaga con su toggle.

- [ ] **Paso 1: tests que fallan**

```csharp
[UnityTest] public IEnumerator Shadow_FollowsBallXZ_OnFloor() { /* pelota en (2, 3, 5) → sombra en (2, 0.001, 5) */ }
[UnityTest] public IEnumerator ImpactMark_SpawnsOnWallContact_NotOnFloor_AndCapsAtSix() { /* 8 contactos con paredes y 1 con el suelo → 6 marcas */ }
[UnityTest] public IEnumerator Toggles_DisableAids() { /* aids.shadow = 0 → sombra inactiva */ }
```
El §8.6 pide que las ayudas no sean imprescindibles para entender un rebote ni tapen al rival. Eso lo juzga el PO en la Etapa 8, no un test.

- [ ] **Paso 2:** ejecutar, ver que falla, implementar y ejecutar hasta que pase.
- [ ] **Paso 3:** compilar e instalar. Recorrer L1–L6 con la cámara fija y con la de seguimiento.
- [ ] **Paso 4:** hacer commit con `feat(runtime): shadow, trail, impact marks and prediction line`.

---

## Etapa 8 — Pruebas automatizadas y validación de First Ball

**Qué hace Codex:** la comprobación de que el panel no está en producción, una ejecución completa de las pruebas, la plantilla de validación del PO y una sesión de rendimiento.
**Cómo se verifica:** con los scripts y la plantilla completada por el PO.
**En el iPhone:** la build de desarrollo para la sesión de validación, y una build de producción sin panel (no aparece el botón "DEV" y tres dedos no hacen nada).
**Para avanzar:** el PO declara First Ball validado [D10], con la evidencia de §15:
- distingue altura y profundidad;
- se cumplen los invariantes de física;
- hay una decisión de física tomada;
- hay un rendimiento base medido.

### Tarea 8.1: El panel no está en producción

**Archivos:**
- Crear: `scripts/check-release.sh`

- [ ] **Paso 1:** `scripts/check-release.sh` genera **los dos** proyectos Xcode (desarrollo y producción) con `BuildScript`, sin `xcodebuild`, y después:

```bash
! grep -rq "SquashClash_DevPanel\|SquashClash.DevPanel" "Builds/iOS-prod/Il2CppOutputProject" \
  && grep -rq "SquashClash_DevPanel\|SquashClash.DevPanel" "Builds/iOS-dev/Il2CppOutputProject" \
  && echo "RELEASE-CHECK: OK"
```
Esperado: `RELEASE-CHECK: OK`. El panel está en desarrollo y no en producción (§17.A.10).
- [ ] **Paso 2:** instalar la build de producción y comprobar que no hay botón "DEV" ni panel con tres dedos.
- [ ] **Paso 3:** hacer commit con `test: verify dev panel is excluded from production builds`.

### Tarea 8.2: Batería completa y plantilla de validación

**Archivos:**
- Crear: `docs/validation/first-ball-validation.md`

- [ ] **Paso 1:** `scripts/test.sh editmode && scripts/test.sh playmode`. Esperado: `failed=0` en EditMode. En PlayMode, los fallos de `PhysXSpikeTests` solo se aceptan si ya están documentados en el informe y el PO ya decidió sobre ellos.
- [ ] **Paso 2:** crear la plantilla de validación con estas secciones:
  1. **Legibilidad (H1).** Una tabla de L1–L6 × {fija, seguimiento} con: "¿distingo la altura? sí/no", "¿distingo la profundidad? sí/no", "¿alguna ayuda tapa al rival? sí/no", una valoración de 1 a 5 y comentarios.
  2. **Física.** Enlace a los informes P1/P2, la decisión del PO y el preset de calibración elegido (nombre y hash).
  3. **Rendimiento.** Una sesión de 15 minutos con la build de desarrollo, sin cámara lenta. Se registran el p50, p95 y p99 del tiempo de frame, los frames > 33 ms por minuto, la temperatura al final y el `DroppedSteps`, frente a las metas de §17.C.
  4. **Invariantes de First Ball** (§17.A.1, 2, 3, 10 y 14): estado y evidencia.
  5. **Decisión del PO:** continuar a First Shot, ajustar o volver.
- [ ] **Paso 3:** el PO completa la plantilla en el iPhone. Codex adjunta los JSONL de `MetricsLog`, exportados al portapapeles o copiados con `devicectl device copy from`.
- [ ] **Paso 4:** hacer commit con `docs: First Ball validation record`.
- [ ] **Paso 5:** Claude actualiza la spec. Registra la decisión de física en §0.3 y §8.2, y el preset de calibración elegido pasa a ser `DefaultPreset.json` en un PR aparte.

---

## Cobertura de la spec (autorrevisión)

| Requisito de la spec | Tarea |
|---|---|
| Preparación: Unity, Xcode, iPhone y compatibilidad (§15, B2–B4, riesgo 16.12) | 0.1, 0.2, 2.2, 3.1 |
| Repositorio, `.superpowers/` ignorado (anexo A) | 1.1 |
| Vertical, IL2CPP, compilación e instalación por CLI (anexo A, D18) | 2.1, 2.2, 3.1 |
| Cancha WSF, líneas de 50 mm, tin de 0,48 m, huella (§8.1, §8.7.2, §8.7.3, §8.7.6, S9) | 4.1, 4.2, 5.2 |
| Una autoridad, primer impacto, impactos por paso, esquinas, paso de 120 Hz (§8.2, §8.3) | 5.1–5.3, 5.5 |
| Condiciones de predicción e invariante bit a bit (§8.2.1, §17.A.3) | 5.2, 5.3, 5.4 |
| Evaluación secuencial P1 → P2 y P3 solo si hace falta (§8.2.3) | 5.3, 6.4, 5.5 (P3 no se planifica: §8.2.3 no lo prevé) |
| Un reloj, τ, λ y cámara lenta que invalida métricas (§8.4) | 5.1, 6.1, 6.3 |
| Panel desde First Ball: física, lanzador, cámara, trayectorias, cámara lenta, paso a paso, FPS (§13.2), presets locales, export y exclusión (§13.1, §13.4, D29) | 6.1–6.4, 8.1 |
| Clases de cambio y "Aplicar y relanzar" (§13.4) | 6.1, 6.3 |
| Trazabilidad de preset, build y semilla (§13.4, §17.A.14) | 6.2, 6.3 |
| Cámara fija y variante de seguimiento (§12.1) | 7.1 |
| Sombra, estela, marcas y ayudas no imprescindibles (§8.6) | 7.2, 8.2 |
| Escenarios de legibilidad 1–6 (§12.3) y H1 | 6.1 (L1–L6), 7.2, 8.2 |
| Rendimiento base: FPS, frames largos, temperatura y predicción ≤ 1 ms (§17.C) | 6.2, 6.4, 8.2 |
| Pérdida de foco: pausa (§6.5, regla 7) | 6.3 |

**Fuera de este plan, a propósito:** gestos y golpes, contrato T3, error, planificador, IA, reglas de punto y saque, consecuencias por fase de las líneas, sonido y vibración (desde First Shot), dirección de arte y Android. El clasificador de líneas solo informa qué franjas toca la huella. Las consecuencias (out, down, fault) llegan con First Rally y First Match.
