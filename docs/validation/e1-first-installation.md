# E1 — proyecto Unity y primera instalación

Fecha: 2026-10-21. Alcance autorizado: I1 / T1.1–T1.3 del plan First Ball r2. **Implementación y comprobación técnica completadas: dev y prod compiladas, firmadas, instaladas y ejecutadas en el iPhone 16 Pro con iOS 26.6.2.** El cierre de I1 sigue pendiente de revisión de Claude y validación del PO. Se deja `prod` instalada y abierta. No se ha comenzado E2 ni creado un remoto.

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
- Comprobación final documental: los SHA-256 de spec, plan y dos revisiones originales coinciden con los de E0; enlaces locales válidos, sintaxis de los cuatro scripts correcta y diff de documentación sin errores de espacios. `device.env` sigue ignorado, no versionado y con permisos 0600; no hay identificadores completos del teléfono en los archivos versionables ni remotos Git configurados.

## Ajustes de ejecución registrados

1. Unity rechaza `.tmp-unity` como nombre de proyecto (`Logs/create.log`). Se utilizó `TempUnityBootstrap`, ignorado, y se movieron solo `Assets`, `Packages` y `ProjectSettings`; el directorio temporal generado se retiró. Es un problema del nombre temporal del plan, no de Xcode ni de la ruta final con espacios.
2. El texto de T1.2 dice seis asmdefs, pero enumera siete. Se siguió la enumeración para conservar regresión y experimentos separados.
3. Se añadió a Editor la referencia explícita `Unity.RenderPipelines.Universal.Runtime`; el script de configuración consume esa API.
4. El helper temporal de Package Manager se eliminó después de resolver y fijar las versiones. No se conserva infraestructura de instalación de paquetes innecesaria.
5. `BuildInfo.json` y su meta se generan e importan en cada build y quedan ignorados. Se cargan por nombre desde Resources, sin referencia serializada a su GUID. Así su generación no marca falsamente el código como sucio.
6. El seguimiento de tareas usa un ledger local: los scripts de `executing-plans` buscan encabezados `Task`, pero el plan usa `Tarea`. Se mantiene el plan original intacto.
7. El bootstrap se hace en la raíz y `main`, conforme a la excepción explícita autorizada; no se crea worktree. Los scripts/configuración se verifican mediante ejecución e integración; el comportamiento de metadata y UI tiene pruebas RED→GREEN. `test.sh` se adelantó internamente para validar T1.2 con el runner real.
8. El primer `xcodebuild dev` terminó con `BUILD SUCCEEDED`, pero la comprobación final del wrapper falló porque asumía `Squash Clash.app`. Unity genera el producto `SquashClash.app`; se corrigieron ambos scripts según el artefacto real. Es resolución de ruta de salida, no incompatibilidad del toolchain. El log inicial se conserva en `Logs/dev-build-initial-summary.log`.
9. La primera exportación iOS completó datos serializados de URP y la entrada de batching iOS. Se inspeccionaron y guardaron en `62ffdf3`. Después de corregir las rutas (`ea0a1e1`), se repitió `build-ios.sh dev` completo: código 0 y workspace limpio. Las builds finales usan ese mismo commit de código, con `gitDirty=false`.
10. I1 ya se encuentra en `main`, sin rama de implementación ni remoto. La autorización y la excepción de bootstrap del plan resuelven la integración: se conserva el estado local y se entrega para revisión, sin menú de merge/PR ni push. Los commits posteriores a `ea0a1e1` registran documentación y evidencia; el hash mostrado por las apps identifica el código que se compiló.

## Pruebas y evidencia de dispositivo

| Comprobación | dev | prod |
|---|---|---|
| Versión / commit | `0.1.0+ea0a1e1` | `0.1.0+ea0a1e1` |
| Player / IL2CPP / Xcode | No Development / Release / Release | No Development / Release / Release |
| Define de build | `SQUASH_DEV` | Ninguno |
| `build-ios.sh` | Código 0, `BUILD SUCCEEDED` | Código 0, `BUILD SUCCEEDED` |
| Firma y ARM64 | Verificadas | Verificadas |
| Instalación y lanzamiento | Verificados con `install-ios.sh dev --console` | `install-ios.sh prod`: código 0; arranque con y sin consola verificado |
| Rótulo y FPS | Verificados visualmente, 60 FPS | Verificados visualmente, 60 FPS |
| Orientación | Portrait en plist, log y captura | Portrait en plist, log y captura |

Log real de `dev`:

```text
[SquashClash] boot 0.1.0+ea0a1e1 dev devpanel=absent development=False squashDev=True unity=6000.3.27f1
[SquashClash] display profile=dev fps=60,0 orientation=Portrait resolution=1206x2622
```

Log real de `prod`:

```text
[SquashClash] boot 0.1.0+ea0a1e1 prod devpanel=absent development=False squashDev=False unity=6000.3.27f1
[SquashClash] display profile=prod fps=60,0 orientation=Portrait resolution=1206x2622
```

Las dos apps comparten bundle ID; instalar `prod` sustituye `dev`. La consola adjunta de dev terminó al sustituirse esa app; la sesión de diagnóstico de prod se cortó con Ctrl-C y se relanzó sin consola, con código 0, para dejarla independiente de la terminal. La salida de una consola adjunta interrumpida no se presenta como un fallo de arranque ni como código 0 del comando completo.

`codesign --verify --deep --strict` pasó en ambos artefactos. El ejecutable y UnityFramework son ARM64. La lista de módulos IL2CPP del producto incluye solo Core y Runtime de Squash Clash: no Editor, tests, Experiments ni DevPanel. DevPanel aún no tiene implementación; esto no sustituye la comprobación de exclusión del panel real en T3.3.

Evidencia versionable, sin identificadores personales del iPhone:

- [Captura dev](evidence/2026-10-21-dev.png).
- [Inspección de app y firma dev](evidence/2026-10-21-dev-inspection.json).
- [Log de arranque dev](evidence/2026-10-21-dev-boot.txt).
- [Captura prod](evidence/2026-10-21-prod.png).
- [Inspección de app y firma prod](evidence/2026-10-21-prod-inspection.json).
- [Log de arranque prod](evidence/2026-10-21-prod-boot.txt).
- [Resultados de tests, RED→GREEN y filtro vacío](evidence/2026-10-21-tests.json), extraídos de los XML y logs locales con sus hashes SHA-256.
- [Revisión interna de código](../reviews/2026-10-21-first-ball-e1-internal-review.md).

Logs completos locales, ignorados por Git: `Logs/dev-build-summary.log`, `Logs/prod-build-summary.log`, `Logs/build-dev.log`, `Logs/build-prod.log`, `Logs/device-dev-console.log`, `Logs/device-prod-install.log` y `Logs/device-prod-console.log`. Los artefactos están en `Builds/DerivedData-<perfil>/Build/Products/Release-iphoneos/SquashClash.app`.

La captura corresponde al display Wireless de Duplicación del iPhone, seleccionado con `devicectl device info displays` y `device capture screenshot --display-unique-id <id>`. Se verificó también visualmente la ventana de Duplicación del iPhone mediante control de aplicaciones. Durante la duplicación la pantalla física puede estar bloqueada: el display LCD no representa la pantalla de la app remota y no se usa como evidencia de arranque.

Esto prueba arranque y actualización del contador, **no** los objetivos de rendimiento de 15 minutos, percentiles, temperatura ni predicción de First Ball. La duplicación estaba activa; no es una sesión de rendimiento válida.

### Firma y confianza

El primer lanzamiento fue rechazado por iOS con CoreDevice 10002 / FBS Security. `codesign --verify --deep --strict` pasó; el perfil nuevo tiene bundle correcto, equipo `PYXNMVF47Z` y dispositivo registrado. El PO confió en su desarrollador en Ajustes del iPhone y confirmó que pudo abrir la app. Después, el lanzamiento desde CLI también funcionó. **Incidencia resuelta por confianza del desarrollador**, sin cambiar Xcode, backend o parámetros del Player.

El perfil es `iOS Team Provisioning Profile: cl.rafaallendes.squashclash`, con vencimiento **2026-10-28T14:59:31Z**. La firma local usa Apple Development/Personal Team (`get-task-allow=true`); esto es distinto de Unity Development Player, que está desactivado en `dev` y `prod`. `prod` es el perfil de Player del plan, firmado para instalación local; no es una distribución App Store. Tras vencer el perfil será necesario volver a compilar y reinstalar con un perfil renovado para continuar las pruebas locales.

### Advertencia del compilador observada

El SDK iOS 27.1 emite `The selected platform is no longer supported by libc++` al compilar parte de IL2CPP con `-mios-version-min=11.0`. Su propio header `__configuration/availability.h` advierte para targets inferiores a iOS 15. El proyecto, el enlace UnityFramework y el plist final tienen mínimo iOS **15.0**. Ambas builds compilaron, se firmaron y arrancaron en iOS **26.6.2**. Se registra esta diferencia de flags internos de IL2CPP; no se considera un fallo de compatibilidad en el dispositivo probado ni se extrapola soporte a otros sistemas sin probarlos. No se cambiaron herramientas ni se añadieron flags de linker para ocultar el aviso.

## Resultados pendientes

- Giro físico del teléfono y aceptación visual final del PO en ambas builds. La configuración, los logs y las capturas verticales ya están verificados para dev y prod; no se ha comprobado todavía el giro físico.
- Revisión de Claude sobre los commits locales y validación visual del PO. T1.4 y E2 no están autorizadas.

No quedan bloqueos técnicos observados para esta primera instalación. No se probaron el perfil `debug`, App Store/TestFlight ni dispositivos o versiones de sistema adicionales, porque no son objetivos de E1.

## Fuentes técnicas

[Plantilla oficial de GitHub para Unity](https://github.com/github/gitignore/blob/main/Unity.gitignore), [tema de runtime de UI Toolkit](https://docs.unity3d.com/6000.3/Documentation/Manual/UIE-tss.html). El diagnóstico y las fuentes de compatibilidad del entorno están en [environment.md](environment.md).
