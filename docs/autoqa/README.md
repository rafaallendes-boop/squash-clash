# AutoQA MVP de Squash Clash

Arnés de pruebas PlayMode en Unity Editor que carga Main real, prepara defaults en memoria e introduce Began/Moved/Ended por un Touchscreen virtual. El juego conserva la autoridad de gesto, contrato, solver y pelota. AutoQA lee sus JSONL y observables, compara con un oráculo independiente y genera informes. No intervienen modelos de lenguaje, red ni claves durante la ejecución.

```sh
scripts/autoqa.sh mvp
scripts/autoqa.sh mvp --scenario M24-PERFECT-LATE10-SL5 --repetition 2
```

El primer comando exige seis escenarios × tres repeticiones = **18 PASS**, más un canario con FAIL esperado. El segundo sirve para reproducir; una selección parcial no aprueba el MVP y sale 2 aunque el intento seleccionado pase. Los artefactos se crean en una carpeta nueva `Logs/autoqa/<runId>/`. No se sobreescriben corridas. `RUN_DIR`, `RESULT` y `ESCALATION_LEVEL` se imprimen explícitamente. Los códigos y las reglas del operador están en [OPERATIONS](OPERATIONS.md).

## Escenarios y autoridad

El catálogo es `Assets/SquashClash/Tests/AutoQA/Scenarios/mvp.json`. PERFECT-EARLY10 y PERFECT-LATE10 usan SL5 con Δ=−10/+10 ms y Scheduled=true/false. VALID-EARLY (−70), FAILED-LATE (+170), NOPULL (300 ms sin tirón) y REJECTED (ancla segura fuera del frontis) conservan SL1. El canario repite EARLY10 esperando deliberadamente Failed. Se ejecuta una sola vez y se reporta separado de los 18 intentos. Defaults, escenarios de producto y parámetros permanecen intactos.

Hashes esperados declarados desde defaults: SL1 `c0d8ec7050e0`, SL5 `55a85f500cf1`, SimulationKey `166a96e319c7`. Seleccionar `practice.launch=4` aplica SL5; la propia receta cambia human.z a `(double)1.6f`, sin escritura de AutoQA en esa clave. El offset no altera el hash. Antes de cada intento se verifican hash, SimulationKey, P2, cero pendientes, M2-4, pantalla 1206×2622, ancla segura y bloqueador real de UI.

El oráculo usa ventanas de configuración y la tabla de cierres de spec/First Shot aprobada. No llama a ContactContract, ContactPlanner ni ShotSolver. KIn/KOut solo se leen del shot-plan para invalidar escenarios por margen. Dos pasos reales = 25,641026 ms frente a ±A/±V/KIn/KOut; la frontera interna Scheduled=0 exige 1 ms. Márgenes inválidos se reportan NOT_EVALUATED con `scenario-invalid-margin`.

La guardia lee las fuentes, detecta APIs de autoridad prohibidas, reflexión fuera del fixture UnityEditor, overrides de UI/área, guardado de presets, escrituras de registros y Tuning.Set fuera de practice.launch. Es una guardia estática y requiere revisión humana independiente; no es un sandbox de código arbitrario. El ensamblado usa TestAssemblies y no referencia DevPanel ni InputSystem.TestFramework. No aparece en Players según check-release.

## Driver y evidencia

ScriptedPlayer construye TouchScript; no ejecuta el juego. Convierte mm→px con ScreenMetrics del juego y usa Camera.main para la proyección. T* se lee con ClockMap.TrySimToReal(controller.Plan.TStarStep,0).

El driver carga Main, continúa el overlay real, carga defaults en memoria, selecciona línea y relanza. Desactiva Host/AppBootstrap/ShotController/PracticeMode y fija solo TouchInputAdapter.ReadRealTime. Por frame virtual: **now → QueueStateEvent(t≤now) → InputSystem.Update → Host.Advance → GesturePreview.Tick → PracticeMode.Tick → ShotController.ProcessPending → GesturePreview.FlushRecords**. No cambia pausas, geometría, filtros ni campos del registro. Esta orquestación es una desviación autorizada frente al bucle de producción.

Las instancias son frescas. Se espera al cierre de recepción sin orden en negativos y, para contactos aplicados, hasta que la pelota alcanza el paso del primer contacto de la predicción saliente, manteniendo PredictionMatchesBitwise. Para fallos se coteja además la predicción entrante sin reemplazo. No se evalúa la trayectoria después del primer frontis: S2-OBS-01 queda fuera de alcance.

`game-metrics/*.jsonl` son copias byte a byte antes de destruir cada app, con SHA-256. LaunchId y AttemptId proceden del juego y se cotejan sus correlaciones. `attempts.jsonl` usa `autoqa-attempt/1`; `summary.json`, `autoqa-summary/1`; `report.md` incluye `gate:`. Los campos de determinismo, exclusiones y tolerancias están predeclarados en [determinism.md](determinism.md); Scheduled y geometría son exactos. Los relativos admiten ≤10⁻⁶ ms. No se exige igualdad de SHA de tiempos que admiten tolerancia.

La integridad registra hashes por intento, SHA de evidencia y una huella del directorio de presets antes/después (`presets-integrity.json`). Solo se leen presets; no se guardan ni se cambia last-active. Si un resultado no canario contradice el oráculo o falla determinismo, los siguientes escenarios normales quedan sin ejecutar/NOT_EVALUATED. El canario se ejecuta de todos modos, en instancia fresca, para conservar el control de integridad (opción A aprobada en las correcciones de revisión). Con canario FAIL, una regresión conserva salida 1/nivel 2. Ante guardia roja o executionError, se omite el canario porque el estado no es fiable; Report.Triage conserva sus reglas. Errores o precondiciones no cuentan como PASS.

## Restauración automática del worktree

Antes de Unity, el script captura el estado completo de `git status --porcelain -z` y el estado/SHA-256 de GraphicsSettings.asset y QualitySettings.asset. En `finally`, también ante ERROR o timeout, hace `git checkout --` exclusivamente de cada uno de esos dos archivos que estuviera limpio respecto de HEAD (índice y worktree). Si ya tenía cambios, no lo restaura. No restaura ningún otro archivo.

`restoration.json` conserva estados previos/posteriores, hashes, operaciones, errores y comparación del estado completo; su ruta y SHA están en summary.artifacts.restoration. Si el estado final difiere del inicial o falla la restauración, el resultado es ERROR/salida 4, executionError.kind=dirty-after-run. Por eso dos corridas consecutivas desde árbol limpio mantienen dirty=false y el árbol limpio al terminar.

Ante error que sustituye un resumen ya generado, se conserva `summary.harness.json` (y el informe previo en `report.harness.md`), con referencias y SHA-256 en el resumen de ERROR. Es evidencia adicional: no convierte ERROR en PASS.

## Historial, límites y reversión

AUTOQA-MVP-01 y AUTOQA-MVP-02 se conservan como historia en los informes del 6 y 8 de noviembre. La decisión posterior del PO de ±10 ms y la predeclaración 9c476bc resuelven la frontera interna de Δ=0; los artefactos anteriores permanecen intactos.

La recepción automatizada se ejecuta en Editor/Mono. No verifica digitalizador, UIKit, IL2CPP, audibilidad, háptica percibida, lectura visual, latencia toque→imagen, sensación ni diversión. La solicitud de clip es observable; no demuestra que se oiga. No sustituye la validación física S3 ni autoriza iPhone, S4, First Rally o First Match.

Reversión: revertir los commits aditivos de esta PR, o retirar `Tests/AutoQA/` y `scripts/autoqa.sh`; no hay migraciones ni cambios de producto/presets/defaults. La PR #36 permanece abierta para revisión independiente de Claude, coordinada por el PO, sin fusión hasta autorización explícita.
