# I3 — primera pelota visible

Fecha: 2026-10-22. Rama `firstball/i3-ball`, base `6f8da64`. [PR #2 hacia main](https://github.com/rafaallendes-boop/squash-clash/pull/2), abierta. Autorización del PO: T3.1–T3.3 con PR hacia main. El PO comunica revisión de Claude sin bugs bloqueantes y autoriza corregir yaw e instalar dev/prod. **Código actual `f075217`: EditMode 109/109, PlayMode 37/37, release-check y builds nativas dev/prod firmadas ARM64 correctos. Checkpoint físico y aceptación del PO pendientes. I3 no cerrado ni fusionado. P1/PhysX es un candidato; la elección de backend sigue reservada al gate I4c. I4 y posteriores no están autorizados.**

## Implementación y regresión

- T3.1: reloj en double a 120 Hz por defecto, τ=0,65 y λ=1 independientes, máximo ocho pasos/frame y contadores de descarte, pausa sin acumulación, paso único y marca pegajosa de cámara lenta. Contratos de estado/contacto/capacidades, balística analítica y runner hasta segundo bote no persistente o límite. AX-1 preservada en normales y planos analíticos. RED 29 fallos de 107; GREEN EditMode 107/107 y PlayMode 11/11.
- T3.2: una escena física local por instancia y todos sus objetos movidos explícitamente, cinco cajas externas y esfera sin spin/interpolación nativa, CCD/materiales/solver de la variante baseline. Gravedad global protegida incluso entre fábricas; snapshots físicos independientes de DTOs mutables; contactos persistentes retenidos; Dispose desactiva objetos, libera materiales y descarga escenas asíncronamente. RED inicial 7/7; GREEN 8/8 de ciclo de vida, EditMode 108/108 y PlayMode completo 19/19.
- T3.3: host con simulación autoritativa y predictor separados, configuración efectiva por lanzamiento, recreación de ambas instancias cuando cambia SimulationKey, interpolación solo visual y parada al segundo bote. Esfera visual amarilla de radio físico 20 mm, sin collider. Barra mínima DevPanel con Lanzar, Pausa/Continuar, Paso y Repetir; pausa del sistema y «Toca para continuar» en Runtime, también en prod. El toque usa la capa real de UI y las acciones de la barra se ejercitan mediante eventos reales de los botones.
- Regresión inicial antes de revisión: **EditMode 109/109 y PlayMode 31/31**, cero fallos/omitidos. RED T3.3: 11 fallos del host/app, dos de configuración y uno adicional de reanudación del sistema. El test adicional demostró ocho pasos en el frame de reanudación; se descarta solo el delta de ese frame, conservando las fracciones del reloj. No se afirma igualdad de vuelo bit a bit como regresión: C6 pertenece a I4.
- Scene setup ejecutado con Unity 6000.3.27f1, código 0; Main referencia host, vista y overlay, sin referencias a DevPanel. `check-release.sh` exporta dev/prod nuevos y verifica los módulos IL2CPP compilados; release-check inicial completado en a2c8946, `RELEASE-CHECK: OK`; builds nativas iniciales dev/prod también código 0. Se regeneran después de la corrección de revisión, y se registra abajo el resultado final.

## Resoluciones trazables

1. El helper de tareas del skill reconoce encabezados ingleses; este plan usa Tarea. Se extraen briefs exactos y se mantiene ledger local. Coste si fuera incorrecto: seguimiento manual, respaldado por commits y evidencia.
2. OverTop marca el primer estado cuyo centro supera WallHeight, sin inventar un techo, conforme al ejemplo de T4.2. Coste: su interpretación se revisará si los datos del gate exigen mayor precisión; ninguna regla deportiva se cambia.
3. La vista previa se limita a diez segundos simulados. El host para al segundo bote real, salida superior o ese límite. Es un límite operativo, no criterio de aceptación de I4; cada escenario de I4 conserva su propio MaxSteps. Coste: un vuelo legal excepcionalmente largo puede quedar truncado hasta revisar datos.
4. Arranque con lanzamiento automático del preset para que prod muestre pelota sin barra. La resolución inicial de yaw positivo hacia +X fue **sustituida por la aclaración del PO tras revisar Claude PR #2**: positivo hacia la derecha de pantalla (−X). El defecto sigue en cero, sin cambiar la trayectoria por defecto. Coste del auto-lanzamiento: ajustar la experiencia de inicio si el PO lo pide en su checkpoint.
5. Choice puede contener el único candidato autorizado P1; el test anterior que exigía al menos dos opciones era una suposición de I2, no un criterio. El tope de impactos queda marcado no operativo para P1 desde ahora. Coste: UI futura debe admitir una sola opción; no se promete un control inexistente.
6. Exclusión de producción comprobada en Il2CppCodeRegistration, no mediante una búsqueda genérica que también encontraría la cadena de reflexión usada por Runtime. Coste: adaptar el verificador si Unity cambia el formato de módulos; evita atribuir panel compilado a una mera cadena.
7. Las comprobaciones físicas del iPhone se difieren por instrucción explícita del PO. Se preparan artefactos y PR, sin cerrar I3 ni elegir backend. Coste: cualquier diferencia nativa sigue pendiente del punto de control.

## Observación PhysX que debe medirse en I4

La caída desde un metro produjo OnCollisionEnter de suelo desde un estado previo y=−0,01165615 m en el paso 54, con punto de contacto sobre y≈0. No existe entonces una raíz analítica futura dentro del paso: **StepOnly + NaN** es el respaldo explícito del diseño. El título abreviado del test del plan decía Estimated incondicionalmente; el test sigue el contrato completo, conserva UV/plano ≤1 mm y añade un caso cercano al suelo que sí reporta Estimated. No se cambió el adaptador para ocultar la limitación ni se relajó un umbral. La precisión del centro/contacto y la penetración deberán medirse en I4; no se presenta esta regresión como aprobación de física.

La documentación de Unity confirma que la simulación local afecta solo a su escena y que los callbacks estándar no se reportan en EditMode: [PhysicsScene.Simulate, Unity 6000.3](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/PhysicsScene.Simulate.html). La convención de materiales usa Multiply, conforme al plan: [PhysicsMaterialCombine](https://docs.unity3d.com/cn/6000.0/ScriptReference/PhysicsMaterialCombine.html).

## Pendientes antes de cerrar I3

- Revisión interna final completada; I1 de interpolación corregido con RED→GREEN.
- Revisión de Claude comunicada por el PO: sin bugs bloqueantes; corrección de yaw solicitada ya verificada RED→GREEN. No fusionar automáticamente.
- iPhone del PO: verificar ambas builds, exclusión de barra en prod, cuatro acciones en dev, bloquear/desbloquear en pleno vuelo y continuar sin salto. La aceptación del PO se registrará expresamente tras el checkpoint; la pérdida de pausa manual queda para I5 por instrucción suya.


## Revisión interna y pase único de corrección

La [revisión independiente](../reviews/2026-10-22-first-ball-i3-internal-review.md) no encontró Critical y detectó un Important de presentación al continuar: pausa mostraba Current, pero continuar podía volver al estado visual anterior. Se reprodujo con **5/5 fallos RED** (manual/sistema, Alpha=0/0,5, congelación en pausa), y se corrigió solo el historial visual: pausa sin paso congela la imagen, paso solicitado rebasa el historial para evitar retroceso. **PlayMode completo 36/36 GREEN** tras el cambio; Current, parámetros, paso físico y fracciones del reloj no se alteran. El informe recoge cada conducta que el revisor dejó para I4, I5/I6 o el iPhone; no quedan silenciosamente aprobadas.


## Artefactos iniciales y evidencia — anteriores a la corrección de yaw

Código compilado: **`a538bc5`**, metadata `gitDirty=false` en ambas builds. Los commits posteriores de documentación no cambian la app. [Manifiesto de tests y hashes XML](evidence/2026-10-22-i3-tests.json): **EditMode 109/109 y PlayMode 36/36**, cero fallos/omitidos, incluida la corrección de la revisión interna.

- `scripts/check-release.sh`: código 0, **RELEASE-CHECK: OK**. Exports de dev/prod recién generados por BuildScript en directorios vacíos, archivos posteriores al inicio y metadata de su perfil. Dev contiene Core/Runtime/DevPanel; prod contiene Core/Runtime. [Dev](evidence/2026-10-22-i3-release-dev.json), [prod](evidence/2026-10-22-i3-release-prod.json).
- `xcodebuild` en Release para **generic/platform=iOS**, con firma automática y el equipo local, terminó con código 0 / BUILD SUCCEEDED en ambos perfiles. No requiere un iPhone conectado para compilar. [Resumen de builds y advertencias](evidence/2026-10-22-i3-builds.json).
- Unity **6000.3.27f1**, Xcode **27.1 (27A9275)**, IL2CPP/ARM64. Inspección estricta de firma, bundle `cl.rafaallendes.squashclash`, Portrait, mínimo iOS 15.0, ejecutable/UnityFramework ARM64 y módulos compilados: [dev](evidence/2026-10-22-i3-dev-inspection.json), [prod](evidence/2026-10-22-i3-prod-inspection.json).
- Ambas apps usan el perfil de desarrollo local ya conocido; esto no es una distribución App Store. El perfil contiene el dispositivo registrado y vence el **28 de octubre de 2026**, igual que en I1/I2. Los UDID quedan fuera de la evidencia versionada.
- Sin errores nativos; cada perfil conserva 14 mensajes únicos de advertencia, enumerados en el resumen. Incluyen libc++ por flags internos de IL2CPP, APIs de Unity obsoletas en el SDK y objetos sin símbolos. La advertencia libc++ ya estaba documentada en I1; el proyecto/app sigue con mínimo iOS 15.0 y ambas compilaciones finalizaron. No se cambió Xcode ni se ocultaron avisos.

Los comandos nativos usan el proyecto `Builds/iOS-<perfil>/Unity-iPhone.xcodeproj`, esquema Unity-iPhone, configuración Release, `-destination generic/platform=iOS`, `-derivedDataPath Builds/DerivedData-<perfil>`, `-allowProvisioningUpdates`, `DEVELOPMENT_TEAM` del entorno local y `CODE_SIGN_STYLE=Automatic`. Artefactos preparados para instalar:

- `Builds/DerivedData-dev/Build/Products/Release-iphoneos/SquashClash.app`
- `Builds/DerivedData-prod/Build/Products/Release-iphoneos/SquashClash.app`

**Al preparar esos artefactos iniciales aún no se había instalado I3 en el iPhone.** El resultado actual de las nuevas builds se registra debajo. Checklist: en dev, cuatro acciones, dos botes, pausa y paso sin retroceso; en ambos perfiles, bloqueo/desbloqueo en vuelo, «Toca para continuar» sin salto, versión/perfil y orientación. En prod además ausencia funcional de barra y log `devpanel=absent`; en dev `devpanel=present`. Se dejará dev al terminar el checkpoint si el PO no indica otra cosa.

No se han ejecutado criterios C1–C9, escenarios S1–S10, benchmark de 200 predicciones ni prueba térmica sostenida. I4 y posteriores siguen sin autorización y la decisión de backend sigue reservada al PO en I4c. Las suites de regresión y la compilación nativa no sustituyen esas mediciones.

## Revisión de Claude y corrección de yaw — 2026-10-22

El PO comunica la revisión de PR #2 por Claude, **sin bugs bloqueantes**, y autoriza corregir yaw antes de instalar. Se invierte únicamente la componente X de `EffectiveConfig.Launch`: yaw positivo hacia −X/derecha de pantalla y negativo hacia +X/izquierda, con coordenadas y cámara AX-1 intactas. No se modifican preset, yaw cero, pitch, velocidad, escenarios, umbrales, spec ni plan.

Prueba de proyección `Launcher_PositiveYawProjectsRightAndNegativeYawProjectsLeft`, con cámara por defecto y aspecto Portrait del iPhone: **RED 1/1**, yaw +15° proyectaba un desplazamiento X de −0,0369616449; después del cambio **PlayMode completo 37/37 y EditMode 109/109 GREEN**, cero fallos/omitidos. Se verifican ambos signos y yaw cero a través de EffectiveConfig → Launcher → WorldToViewportPoint. [XML, hashes y resultado RED→GREEN](evidence/2026-10-22-i3-yaw-tests.json).

La pérdida de una pausa manual previa al tocar «Toca para continuar» queda **aceptada como pendiente de I5**, según instrucción del PO. No está corregida en I3 ni autoriza trabajo en I5. Las builds `a538bc5` anteriores quedan como evidencia histórica; se regeneran con esta corrección antes de instalar. El iPhone está conectado por cable, emparejado y con Modo de desarrollador activo. El perfil comprobado sigue vigente hasta `2026-10-28T14:59:31Z`; deberá renovarse antes de cualquier instalación cuando haya vencido. El checkpoint físico y la aceptación del PO se registrarán con sus resultados reales, sin cerrar ni fusionar I3 automáticamente.

### Builds posteriores a la corrección

`scripts/check-release.sh` terminó con código 0 / **RELEASE-CHECK: OK** para ambos exports nuevos: dev Core/Runtime/DevPanel y prod Core/Runtime. Metadata **`0.1.0+f075217 dev|prod`, `gitDirty=false`**: [dev](evidence/2026-10-22-i3-yaw-release-dev.json), [prod](evidence/2026-10-22-i3-yaw-release-prod.json). Xcode Release `generic/platform=iOS` terminó con código 0 / BUILD SUCCEEDED en ambos perfiles, usando la misma combinación Unity 6000.3.27f1 / Xcode 27.1.

La inspección posterior verifica firma estricta, ejecutable y UnityFramework ARM64, bundle, Portrait, registro local del dispositivo y perfil vigente: [dev](evidence/2026-10-22-i3-yaw-dev-inspection.json), [prod](evidence/2026-10-22-i3-yaw-prod-inspection.json). Se conservan las 14 advertencias nativas únicas por perfil enumeradas en esos JSON, sin errores ni cambio de herramientas. Antes de reinstalar se vuelve a comprobar la vigencia; no se usa un perfil vencido.

### Instalación completada; checkpoint bloqueado por el bloqueo del iPhone

**Prod y dev se instalaron correctamente**, en ese orden, mediante `scripts/install-ios.sh prod|dev`: ambos `install-<perfil>.json` tienen outcome success. **Dev `0.1.0+f075217` quedó instalada al terminar.** iOS rechazó ambos lanzamientos con **Locked**, por lo que cada script terminó con código 1 en la fase de lanzamiento; la instalación no se confunde con un arranque correcto. El PO ha recibido el aviso para desbloquear de nuevo. El arranque de ambas builds, logs devpanel, cuatro controles, bloqueo/reanudación real y aceptación del PO siguen pendientes. [Evidencia de instalación y bloqueo](evidence/2026-10-22-i3-yaw-device-install.json).

La captura tomada durante el bloqueo queda solo en Logs local y no se publica como evidencia de la app. No existe aún captura ni log de arranque verificado de I3 en dispositivo. Cuando se desbloquee, comprobar primero dev; la comprobación pendiente de prod requerirá volver a instalarla y restaurar dev, verificando siempre el vencimiento antes de reinstalar. **No se cierra ni fusiona I3 ni se avanza a I4.**
