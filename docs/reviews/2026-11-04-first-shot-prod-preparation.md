# First Shot — preparación PROD offline, 2026-11-04

## Alcance y estado inicial

Autorización del PO en [aprobaciones](2026-10-25-first-shot-po-approvals.md): check-release dev/prod y buildprodRelease desde el producto de devinstalada **9131b1c**, sin acceso al iPhone ni instalación. PR#30 sigue abierta/sinfusionar; sesiónfísica enpausa tras14/15,16sinpreparar. FuentesAssets/ProjectSettings/Packages/scripts comparadas con9131b1c sin diferencias al iniciar desde **dfffd58**. Evidencia previa íntegra se protege con inventariohash/tamaño/mtime fuera de Logs/evidence.

El script check-release versionado escribe resultados en Logs/evidence. Se ejecutará su mismo contenido con **solo redirección de los destinos de sus archivos** a `Logs/prod-preparation-20261104`, sin modificar el archivo ni sus comprobaciones. Exports/dev-prod y buildprod deben registrar gitDirty=false. Xcode usará destino **generic/platform=iOS**, sin consulta/conexión al iPhone, y firma local con perfil previamente incorporado/vigente, sin -allowProvisioningUpdates. Sin nuevas suites ni tests.

## Ventana de campaña pendiente

Vencimiento de referencia incorporado **2026-11-07T17:58:46Z**, sábado7nov**14:58:46America/Santiago**. Tanda2restante:16POT-F/17CAN/18–21NEG/22RIT, **37recepciones**; tanda3:23–24/**20recepciones**,16preguntasS1+preferencia; tanda4PROD:25/**3recepciones**,autorizaciónpropia/respaldo nuevo. Cerrar/restaurar/exportar/cotejar cada tanda y entregar paraClaude/PO antes de la siguiente, segúnordenPO.

Propuesta operativa de agenda, sin declarar disponibilidad/velocidad humana: reservar una ventana de **90min para restante tanda2**, otra de **60min para tanda3** y **45min paraPRODincluidosrespaldo/instalación/restauración**, más coordinación/revisión entre ellas. Son reservas de planificación, **no umbrales ni duración obligatoria de bloques**. Objetivo terminar como máximo el6nov para dejar el7nov de margen; disponibilidadPOporconfirmaralvolver. No comprometer instalación7novsi no queda todoeltramo+restauración cubierto. Si no se pueden fijar ventanas suficientes, avisar alPO **antesdeinstalar** y no avanzar instalación con perfil insuficiente. Revalidar perfil incorporado antesdecadatanda/instalación; no renovar por inferencia ni reanudar ahora.

## Resultados

**Preparación completada sin acceso al iPhone ni instalación.** Fuentes de producto, presets, spec, plan y criterios intactos.

| Comprobación fresca | Resultado |
|---|---|
| check-release dev | OK, export fresco, `gitHash=a11fb32`, `gitDirty=false`; Core/Runtime/DevPanel incluidos, InputSystem.TestFramework excluido |
| check-release prod | OK, export fresco, `gitHash=a11fb32`, `gitDirty=false`; Core/Runtime incluidos, DevPanel y su puente térmico dev excluidos, puente diagnóstico runtime incluido |
| Perfil del export | `prod`, `development=False`, `il2cpp=Release`, `extraDefines=` vacío |
| Xcode | **BUILD SUCCEEDED**, Release, destino `generic/platform=iOS`, sin `-allowProvisioningUpdates` ni destino/id de iPhone |
| Binarios firmados | app y UnityFramework **arm64**; firma estricta `codesign --verify --deep --strict` OK |
| Perfil incorporado PROD | mismo tramo conocido **2026-10-31T17:58:46Z → 2026-11-07T17:58:46Z**; no renovación ni instalación |
| Fuentes protegidas | **514 archivos Assets/ProjectSettings/Packages/scripts** SHA-256 iguales antes/después y diff Git vacío frente a9131b1c |
| Evidencia previa | **3145 archivos/875.351.187 bytes**; inventario de rutas/SHA-256/tamaño/**mtimeNs** exactamente igual antes/después |
| Estado final | worktree limpio, ProjectSettings sin diferencias; PR#30 sin fusionar |

Build preparada **0.1.0+a11fb32 prod**, fuente de producto idéntica a devinstalada0.1.0+2ef4807; la diferencia de gitHash es documental. App en `Builds/DerivedData-prod/Build/Products/Release-iphoneos/SquashClash.app` de este worktree. SHA-256 del ejecutable app `e5e3c5cf927adebb93fa73f5a86f1ca902052335b91408aec0bcf7c484d88c8c`; UnityFramework `59daf8374f032e64a8a0bfec0fa23543ae669a62b4e5060a29dade7b5d7a2460`. La firma abarca el bundle; no se exportan claves ni identificadores del equipo/dispositivo.

Los dos checks ejecutaron el script original con solo tres sustituciones de destino (`mkdir` y dos archivos JSON), preservadas con hashes en `Logs/prod-preparation-20261104/check-release-provenance.json`. No se cambió `scripts/check-release.sh`; ningún resultado se escribió en Logs/evidence. Unity generó transitoriamente `Assets/Resources/PerformanceTestRun*` y sus metas; se preservaron fuera de Assets en `Logs/prod-preparation-20261104/generated-performance-resources` antes del build nativo, sin versionarlos ni ampliar el alcance a tests. No se ejecutaron suites ni se alteraron aserciones. Ambos exports registraron gitDirty=false; no se reconstruyó uno con estado sucio ni se cambió su metadata a mano.

Logs y comprobaciones privadas nuevos en `Logs/prod-preparation-20261104`, ignorados. La compilación nativa usó directamente el exportprod fresco ya comprobado, con el Xcode instalado y firma automática local sin habilitar actualizaciones de provisioning. No se invocó `devicectl`, Duplicación, instalación, relanzamiento ni preparación física de16 en esta entrega offline.

**Límite para uso futuro:** la build queda disponible; instalar requiere PO presente/autorización propia, nuevo respaldo y cotejo§2/§8, export/restauración dev previa y ventana completa antesdelvencimiento. No se considera evidencia de recorridoPROD ni cierra S2/tanda4.
