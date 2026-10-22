# I3 — primera pelota visible

Fecha: 2026-10-22. Rama `firstball/i3-ball`, base `6f8da64`. Autorización del PO: T3.1–T3.3 con PR hacia main y revisión de Claude; validación en iPhone pendiente hasta que el PO lo conecte al finalizar. **Entrega técnica preparada: EditMode 109/109, PlayMode 36/36, release-check y builds nativas dev/prod verificados con código `a538bc5`. I3 no cerrado ni fusionado. P1/PhysX es un candidato; la elección de backend sigue reservada al gate I4c. I4 y posteriores no están autorizados.**

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
4. Arranque con lanzamiento automático del preset para que prod muestre pelota sin barra. Yaw positivo se mide hacia +X (izquierda de pantalla tras AX-1); el defecto sigue en cero. Coste: ajustar experiencia de inicio/signo si el PO lo pide en su checkpoint, sin cambiar trayectorias por defecto.
5. Choice puede contener el único candidato autorizado P1; el test anterior que exigía al menos dos opciones era una suposición de I2, no un criterio. El tope de impactos queda marcado no operativo para P1 desde ahora. Coste: UI futura debe admitir una sola opción; no se promete un control inexistente.
6. Exclusión de producción comprobada en Il2CppCodeRegistration, no mediante una búsqueda genérica que también encontraría la cadena de reflexión usada por Runtime. Coste: adaptar el verificador si Unity cambia el formato de módulos; evita atribuir panel compilado a una mera cadena.
7. Las comprobaciones físicas del iPhone se difieren por instrucción explícita del PO. Se preparan artefactos y PR, sin cerrar I3 ni elegir backend. Coste: cualquier diferencia nativa sigue pendiente del punto de control.

## Observación PhysX que debe medirse en I4

La caída desde un metro produjo OnCollisionEnter de suelo desde un estado previo y=−0,01165615 m en el paso 54, con punto de contacto sobre y≈0. No existe entonces una raíz analítica futura dentro del paso: **StepOnly + NaN** es el respaldo explícito del diseño. El título abreviado del test del plan decía Estimated incondicionalmente; el test sigue el contrato completo, conserva UV/plano ≤1 mm y añade un caso cercano al suelo que sí reporta Estimated. No se cambió el adaptador para ocultar la limitación ni se relajó un umbral. La precisión del centro/contacto y la penetración deberán medirse en I4; no se presenta esta regresión como aprobación de física.

La documentación de Unity confirma que la simulación local afecta solo a su escena y que los callbacks estándar no se reportan en EditMode: [PhysicsScene.Simulate, Unity 6000.3](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/PhysicsScene.Simulate.html). La convención de materiales usa Multiply, conforme al plan: [PhysicsMaterialCombine](https://docs.unity3d.com/cn/6000.0/ScriptReference/PhysicsMaterialCombine.html).

## Pendientes antes de cerrar I3

- Revisión interna final completada; I1 de interpolación corregido con RED→GREEN.
- Revisión de Claude de la PR hacia main pendiente; no fusionar automáticamente.
- iPhone del PO: ambas builds, exclusión de barra en prod, cuatro acciones en dev, bloquear/desbloquear en pleno vuelo y continuar sin salto. El PO lo conectará al terminar. No se ha instalado ni validado I3 en el dispositivo.


## Revisión interna y pase único de corrección

La [revisión independiente](../reviews/2026-10-22-first-ball-i3-internal-review.md) no encontró Critical y detectó un Important de presentación al continuar: pausa mostraba Current, pero continuar podía volver al estado visual anterior. Se reprodujo con **5/5 fallos RED** (manual/sistema, Alpha=0/0,5, congelación en pausa), y se corrigió solo el historial visual: pausa sin paso congela la imagen, paso solicitado rebasa el historial para evitar retroceso. **PlayMode completo 36/36 GREEN** tras el cambio; Current, parámetros, paso físico y fracciones del reloj no se alteran. El informe recoge cada conducta que el revisor dejó para I4, I5/I6 o el iPhone; no quedan silenciosamente aprobadas.


## Artefactos finales y evidencia

Código compilado: **`a538bc5`**, metadata `gitDirty=false` en ambas builds. Los commits posteriores de documentación no cambian la app. [Manifiesto de tests y hashes XML](evidence/2026-10-22-i3-tests.json): **EditMode 109/109 y PlayMode 36/36**, cero fallos/omitidos, incluida la corrección de la revisión interna.

- `scripts/check-release.sh`: código 0, **RELEASE-CHECK: OK**. Exports de dev/prod recién generados por BuildScript en directorios vacíos, archivos posteriores al inicio y metadata de su perfil. Dev contiene Core/Runtime/DevPanel; prod contiene Core/Runtime. [Dev](evidence/2026-10-22-i3-release-dev.json), [prod](evidence/2026-10-22-i3-release-prod.json).
- `xcodebuild` en Release para **generic/platform=iOS**, con firma automática y el equipo local, terminó con código 0 / BUILD SUCCEEDED en ambos perfiles. No requiere un iPhone conectado para compilar. [Resumen de builds y advertencias](evidence/2026-10-22-i3-builds.json).
- Unity **6000.3.27f1**, Xcode **27.1 (27A9275)**, IL2CPP/ARM64. Inspección estricta de firma, bundle `cl.rafaallendes.squashclash`, Portrait, mínimo iOS 15.0, ejecutable/UnityFramework ARM64 y módulos compilados: [dev](evidence/2026-10-22-i3-dev-inspection.json), [prod](evidence/2026-10-22-i3-prod-inspection.json).
- Ambas apps usan el perfil de desarrollo local ya conocido; esto no es una distribución App Store. El perfil contiene el dispositivo registrado y vence el **28 de octubre de 2026**, igual que en I1/I2. Los UDID quedan fuera de la evidencia versionada.
- Sin errores nativos; cada perfil conserva 14 mensajes únicos de advertencia, enumerados en el resumen. Incluyen libc++ por flags internos de IL2CPP, APIs de Unity obsoletas en el SDK y objetos sin símbolos. La advertencia libc++ ya estaba documentada en I1; el proyecto/app sigue con mínimo iOS 15.0 y ambas compilaciones finalizaron. No se cambió Xcode ni se ocultaron avisos.

Los comandos nativos usan el proyecto `Builds/iOS-<perfil>/Unity-iPhone.xcodeproj`, esquema Unity-iPhone, configuración Release, `-destination generic/platform=iOS`, `-derivedDataPath Builds/DerivedData-<perfil>`, `-allowProvisioningUpdates`, `DEVELOPMENT_TEAM` del entorno local y `CODE_SIGN_STYLE=Automatic`. Artefactos preparados para instalar:

- `Builds/DerivedData-dev/Build/Products/Release-iphoneos/SquashClash.app`
- `Builds/DerivedData-prod/Build/Products/Release-iphoneos/SquashClash.app`

**I3 todavía no se ha instalado ni ejecutado en el iPhone.** Cuando lo conecte el PO se usarán `scripts/install-ios.sh dev` y `scripts/install-ios.sh prod`, sin atribuir éxito antes de comprobarlo. En dev: cuatro acciones, dos botes, pausa y paso sin retroceso; en ambos perfiles: bloqueo/desbloqueo en vuelo, «Toca para continuar» sin salto, versión/perfil y orientación. En prod además ausencia funcional de barra y log `devpanel=absent`; en dev `devpanel=present`. Se dejará dev al terminar el checkpoint si el PO no indica otra cosa.

No se han ejecutado criterios C1–C9, escenarios S1–S10, benchmark de 200 predicciones ni prueba térmica sostenida. I4 y posteriores siguen sin autorización y la decisión de backend sigue reservada al PO en I4c. Las suites de regresión y la compilación nativa no sustituyen esas mediciones.
