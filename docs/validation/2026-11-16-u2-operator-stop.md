# U2 — parada por secuencia del operador

16 nov 2026 · Codex · rama `codex/u2-release-lane`, base main `10bcaec`, RED sobre `d575d69`.

**Incidente:** antes del primer RED, la comprobación informó `UNITY_EDITOR_PROCESSES=2` (incluye editor o auxiliares UnityShaderCompiler/UnityPackageManager) y salió con `STOP: Unity process remains`. El comando de Codex no cortó la secuencia tras el error porque la llamada siguiente estaba en una línea independiente sin propagación obligatoria del estado. Se inició `scripts/test.sh editmode IosReleaseLaneTests`. No se guardaron PID/comandos del chequeo inicial; no se demuestra si eran editores o auxiliares ni de qué proyecto. La ausencia posterior no acredita exclusividad durante esa corrida.

**Resultado observado del RED:** XML fresco, **0/6**, seis fallos de aserción esperados y sin errores CS: número 0 frente a 1 esperado; icono sin binding; imagen de arranque sin binding; tres claves plist ausentes. No se presenta como PASS ni como acreditación de la regla de exclusividad. No se repite ni se implementa para obtener GREEN; se conserva la evidencia y se detiene conforme a AGENTS §2.

Evidencia ignorada: `Logs/evidence/u2/release-lane-red-editmode-results.xml` y `release-lane-red-editmode.log`; SHA-256 del XML en `docs/validation/evidence/2026-11-16-u2-summary.json`. El chequeo inicial consta en la salida de la herramienta, no se inventa una captura de procesos retrospectiva.

**Estado:** solo tests, referencia de assembly Editor para los tests, esqueletos compilables y documentación. PlayerSettings, importadores de arte, pipeline real de build/plist y script release-ios aún sin implementar. No se modificó gameplay, defaults, física, solver, gesto, contrato o reloj. Sin corridas completas, check-release, distribución nativa, firma, subida, iPhone, instalación o fusión. No se solicitó ni abrió ninguna clave API; solo se copió device.env sin cambios conforme a la autorización existente. `xcodebuild -help` fue una consulta local de sintaxis, sin compilación ni firma.

Para una reanudación, la comprobación y el inicio de Unity deben ser llamadas separadas; un estado no cero debe devolver el control sin lanzar el test. Debe guardarse la lista de PID/comando/proyecto para distinguir editores de auxiliares, sin terminar procesos ajenos. Esto se documenta para continuar solo cuando corresponda; no se ejecuta otra corrida aquí. #65 y #66 conservan sus resultados en sus worktrees independientes.

## Reanudación autorizada

El PO autoriza una sola repetición RED tras #66 completo, con inventario PID/comando/proyecto antes de lanzar y aborto obligatorio si existe Unity. #66 quedó publicado en `96321dd`, compuertas 748/748, 561/561 y Release OK; no queda ningún proceso de sus corridas. E1 terminó y declaró Unity libre. Esta nueva autorización no acredita retrospectivamente la exclusividad del primer intento ni borra el incidente. Se integra main `02f3c57` y se conservan intactos los seis tests y los esqueletos.

**Resultado de la repetición única autorizada:** sobre `1c1d075`, XML fresco **0/6**, seis fallos de aserción esperados y sin errores CS. Inventarios previos/posteriores guardados (cero procesos); sin incidente nuevo. Se consume la autorización y se continúa la implementación U2 original. El intento anterior conserva su limitación de exclusividad.

**Cierre de esta continuación:** implementación `66e1707`, EditMode 744/744, PlayMode 558/558, check-release DEV/PROD OK con comprobación de plist/número/icono/arranque. XML frescos, sin errores CS ni omitidos; ProjectSettings restaurado al commit intencional. Inventarios previos vacíos y cero procesos Unity al terminar. Sin nuevo incidente. Distribución nativa, firma y subida siguen NotEvaluated y no se ejecutaron; la limitación del primer intento queda conservada.

## Detención tras integrar main con U3/E1 (17 nov 2026)

Integración `e372ad9` sobre main `3780bf8` (#66 fusionada), con todas las entradas de aprobaciones conservadas y sin cambios adicionales de implementación. EditMode completo **754/754**. PlayMode completo **580/581**, cero omitidos, XML fresco y sin errores CS; falla de aserción en `ReticleVisibilityTests.DevOpen_ProtocolActionsRemainUsable_AndCloseDevBeforeArming`, línea **213**: esperado ≤ **674,663208**, observado **788,198853**. Coinciden test y cifras con la recurrencia registrada de **CRP-01**; el archivo no cambió desde main de U1. **La causa sigue sin demostrarse**; no se atribuye a U2 ni se reclasifica esta corrida como PASS.

Por el encargo del PO se detiene: **sin reintento, corrección, check-release nuevo ni fusión**. La aprobación condicional no se ejecuta porque PlayMode no quedó verde. Se guardaron inventarios PID/comando/proyecto vacíos antes de ambos lanzamientos y al terminar, además de XML/logs ignorados `Logs/evidence/u2/post-u3-*`. ProjectSettings restaurado; árbol limpio antes de documentar, ningún Unity activo. Sin build nativa, instalación, acceso al iPhone, archive, firma o subida. Las compuertas anteriores conservan sus heads y no acreditan esta integración.
