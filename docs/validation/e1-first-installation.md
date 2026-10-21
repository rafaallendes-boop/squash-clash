# E1 — proyecto Unity y primera instalación

Fecha: 2026-10-21. Alcance autorizado: I1 / T1.1–T1.3 del plan First Ball r2. **En curso; falta compilar e instalar.**

## Implementación y comprobaciones

- T1.1: Git local en `main`, commit inicial `40b22ca`. Plantilla oficial Unity.gitignore más exclusiones del proyecto; 16 rutas verificadas, incluyendo `scripts/device.env` y `.superpowers`. No hay remoto.
- `scripts/device.env` es local, ignorado y con permisos 0600. No se publican identificadores del teléfono ni credenciales.
- Editor: Unity 6000.3.27f1, changeset `672d562a42a7`, arm64. Proyecto en la raíz con espacios, sin renombrarla.
- Paquetes resueltos por ese editor: URP 17.3.0, Input System 1.20.1, Test Framework 1.6.0. `manifest.json` y `packages-lock.json` versionables.
- Siete asmdefs del árbol del plan: Core, Runtime, DevPanel, Editor, Core.Tests, Runtime.Tests y Experiments. Editor incluye la referencia al paquete URP necesaria para crear sus assets.
- `ProjectSetup.Apply`: serialización ForceText, meta visibles, Input System nuevo, URP asignado en Graphics y Quality, iOS IL2CPP/ARM64, vertical sin autorrotación, firma automática, bundle ID confirmado y Frame Timing Stats. Minimum iOS conserva el valor del editor: 15.0.
- Se aplicaron únicamente los ajustes estáticos de Physics enumerados en T1.2; no hay simulación, cancha ni pelota.
- T1.2, primeras ejecuciones: EditMode 1/1, PlayMode 1/1 y Experiments smoke 1/1, sin fallos. Configuración aplicada dos veces; una cámara y una escena de build, sin duplicados. Evidencia local en `Logs/t12-editmode-summary.log`, `Logs/t12-playmode-summary.log` y `Logs/experiments-smoke-results.xml`.
- T1.3: `BuildInfo` lee los seis campos de metadata y compone el rótulo, con asterisco para un workspace sucio. La pantalla UI Toolkit muestra título, versión/hash/perfil y FPS actualizados cada 0,5 s. El Editor puede usar metadata de sesión local si aún no hay build; el Player exige el recurso generado.
- TDD: antes de implementar, EditMode registró dos fallos por ausencia de BuildInfo; PlayMode, uno por ausencia de UIDocument. Después: EditMode 3/3 y PlayMode 2/2. XML rojos conservados en `Logs/buildinfo-red-results.xml` y `Logs/bootscreen-red-results.xml`. Las pruebas de BuildInfo se refactorizaron a referencias tipadas después de verificar el primer verde.
- `scripts/test.sh editmode NoSuchFirstBallTest_ExpectedZero` terminó con código 1 y `RESULT: total=0 passed=0 failed=0 skipped=0`, como exige el contrato. Evidencia: `Logs/zero-tests-summary.log`.
- El runner de regresión selecciona explícitamente su ensamblado; no ejecuta `SquashClash.Experiments`. Los scripts de build y de instalación validan argumentos y respetan rutas con espacios.

## Ajustes de ejecución registrados

1. Unity rechaza `.tmp-unity` como nombre de proyecto (`Logs/create.log`). Se utilizó `TempUnityBootstrap`, ignorado, y se movieron solo `Assets`, `Packages` y `ProjectSettings`; el directorio temporal generado se retiró. Es un problema del nombre temporal del plan, no de Xcode ni de la ruta final con espacios.
2. El texto de T1.2 dice seis asmdefs, pero enumera siete. Se siguió la enumeración para conservar regresión y experimentos separados.
3. Se añadió a Editor la referencia explícita `Unity.RenderPipelines.Universal.Runtime`; el script de configuración consume esa API.
4. El helper temporal de Package Manager se eliminó después de resolver y fijar las versiones. No se conserva infraestructura de instalación de paquetes innecesaria.
5. `BuildInfo.json` y su meta se generan e importan en cada build y quedan ignorados. Se cargan por nombre desde Resources, sin referencia serializada a su GUID. Así su generación no marca falsamente el código como sucio.
6. El seguimiento de tareas usa un ledger local: los scripts de `executing-plans` buscan encabezados `Task`, pero el plan usa `Tarea`. Se mantiene el plan original intacto.
7. El bootstrap se hace en la raíz y `main`, conforme a la excepción explícita autorizada; no se crea worktree. Los scripts/configuración se verifican mediante ejecución e integración; el comportamiento de metadata y UI tiene pruebas RED→GREEN. `test.sh` se adelantó internamente para validar T1.2 con el runner real.

## Resultados pendientes

- Última ejecución completa tras refactor de tests y rótulo diagnóstico de los defines de compilación.
- Compilación `dev` y `prod`, firma automática y comprobación de sus perfiles.
- Instalación, lanzamiento, versión/FPS y orientación en el iPhone.
- Revisión de Claude sobre los commits locales y validación visual del PO. T1.4 y E2 no están autorizadas.

## Fuentes técnicas

[Plantilla oficial de GitHub para Unity](https://github.com/github/gitignore/blob/main/Unity.gitignore), [tema de runtime de UI Toolkit](https://docs.unity3d.com/6000.3/Documentation/Manual/UIE-tss.html). El diagnóstico y las fuentes de compatibilidad del entorno están en [environment.md](environment.md).
