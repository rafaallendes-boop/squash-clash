# U2 — parada por secuencia del operador

16 nov 2026 · Codex · rama `codex/u2-release-lane`, base main `10bcaec`, RED sobre `d575d69`.

**Incidente:** antes del primer RED, la comprobación informó `UNITY_EDITOR_PROCESSES=2` (incluye editor o auxiliares UnityShaderCompiler/UnityPackageManager) y salió con `STOP: Unity process remains`. El comando de Codex no cortó la secuencia tras el error porque la llamada siguiente estaba en una línea independiente sin propagación obligatoria del estado. Se inició `scripts/test.sh editmode IosReleaseLaneTests`. No se guardaron PID/comandos del chequeo inicial; no se demuestra si eran editores o auxiliares ni de qué proyecto. La ausencia posterior no acredita exclusividad durante esa corrida.

**Resultado observado del RED:** XML fresco, **0/6**, seis fallos de aserción esperados y sin errores CS: número 0 frente a 1 esperado; icono sin binding; imagen de arranque sin binding; tres claves plist ausentes. No se presenta como PASS ni como acreditación de la regla de exclusividad. No se repite ni se implementa para obtener GREEN; se conserva la evidencia y se detiene conforme a AGENTS §2.

Evidencia ignorada: `Logs/evidence/u2/release-lane-red-editmode-results.xml` y `release-lane-red-editmode.log`; SHA-256 del XML en `docs/validation/evidence/2026-11-16-u2-summary.json`. El chequeo inicial consta en la salida de la herramienta, no se inventa una captura de procesos retrospectiva.

**Estado:** solo tests, referencia de assembly Editor para los tests, esqueletos compilables y documentación. PlayerSettings, importadores de arte, pipeline real de build/plist y script release-ios aún sin implementar. No se modificó gameplay, defaults, física, solver, gesto, contrato o reloj. Sin corridas completas, check-release, distribución nativa, firma, subida, iPhone, instalación o fusión. No se solicitó ni abrió ninguna clave API; solo se copió device.env sin cambios conforme a la autorización existente. `xcodebuild -help` fue una consulta local de sintaxis, sin compilación ni firma.

Para una reanudación, la comprobación y el inicio de Unity deben ser llamadas separadas; un estado no cero debe devolver el control sin lanzar el test. Debe guardarse la lista de PID/comando/proyecto para distinguir editores de auxiliares, sin terminar procesos ajenos. Esto se documenta para continuar solo cuando corresponda; no se ejecuta otra corrida aquí. #65 y #66 conservan sus resultados en sus worktrees independientes.
