# Revisión técnica del plan First Ball (Codex, 2026-10-20)

> Copia literal del informe de Codex, tomada de su sesión (2026-10-20T14:33Z) para que quede trazabilidad. La respuesta está en `2026-10-20-first-ball-plan-review-response-claude.md`.

## A. Veredicto ejecutivo

**APTO CON AJUSTES. Recomiendo corregir los bloqueantes antes de autorizar la ejecución del plan.**

Leí completos el [plan de First Ball](</Users/rafaallendes/Projects/Squash Clash/docs/superpowers/plans/2026-10-20-squash-clash-first-ball.md>) y la [spec v0.3, incluida E.0](</Users/rafaallendes/Projects/Squash Clash/docs/superpowers/specs/2026-10-20-squash-clash-design-v0.3.md>).

La dirección general es adecuada:

- First Ball tiene un alcance explícito y excluye sistemas de gameplay posteriores.
- Probar primero PhysX y desarrollar P2 solo si existe una razón medida evita trabajo innecesario.
- La autoridad única sobre la pelota y la separación entre simulación y representación son buenas decisiones.
- El panel, los presets y la validación en iPhone aportan valor inmediato.
- No se propone infraestructura de red ni arquitectura empresarial.

**El mayor riesgo es elegir la física usando pruebas que todavía no miden correctamente sus criterios.** Hay contradicciones ejecutables en el reloj, un error geométrico en S9, información insuficiente en los contactos y diferencias entre la cobertura declarada y la implementada.

No encontré evidencia suficiente para declarar incompatible la combinación Unity 6.3–Xcode 27.1. Tampoco puede darse por validada todavía.

### Entorno: qué pude comprobar

| Elemento | Resultado de esta revisión |
|---|---|
| Mac | macOS **26.6.2**, arquitectura **arm64**. |
| Xcode seleccionado | **27.1, build 27A9275**, con SDK iOS **27.1**. |
| Unity instalado | Encontré **6000.6.6f1**; no encontré una instalación 6000.3 ni iOS Build Support en la instalación inspeccionada. |
| Unity Hub | **3.22.3**. |
| iPhone físico | `devicectl` solo mostró un simulador apagado. No pude verificar conexión, emparejamiento ni iOS del iPhone 16 Pro. Esto no contradice que sea el dispositivo elegido. |
| Repositorio local | No existe `.git` en el workspace. No asumí configuración de GitHub. |
| Compilación, firma e instalación | **No probadas**, conforme a tus restricciones. |

Unity publica soporte de 6.3 LTS hasta diciembre de 2027 y requisitos de iOS 15+ y Xcode 16 o posterior. Esos mínimos no certifican una combinación concreta de parche de Unity y SDK nuevo. Apple sí incluye iOS 26 dentro del rango de dispositivos compatible con Xcode 27.1. [Soporte de Unity](https://unity.com/releases/unity-6/support), [requisitos de Unity 6.3](https://docs.unity3d.com/6000.3/Documentation/Manual/system-requirements.html), [matriz de Apple](https://developer.apple.com/xcode/system-requirements).

## B. Matriz de hallazgos

**Bloqueante** significa que debe resolverse antes de implementar o aprobar la parte afectada. Una omisión documental no demuestra que una futura implementación vaya a fallar; sí impide ejecutarla literalmente con un criterio inequívoco.

### Contratos y pruebas de física

| ID | Severidad | Etapa / tarea | Problema concreto | Evidencia técnica | Consecuencia | Recomendación |
|---|---|---|---|---|---|---|
| **H01** | **Bloqueante** | 2 · T2.1–T2.2 | El arranque tiene dependencias circulares. | T2.1 utiliza `unity-env.sh`, creado en T2.2. Ese script exige `ProjectVersion.txt`, que todavía no existe. T2.2 también exige compilar antes de que T3.1 complete el arranque. | La ejecución estricta por tarea y PR queda bloqueada al comenzar. | Separar resolución inicial del editor de validación de proyecto existente. Preparar helpers mínimos antes de T2.1 y trasladar la primera compilación al corte conjunto de arranque. |
| **H02** | **Bloqueante** | 5 · T5.1 | El contrato y los tests del reloj son incompatibles. | `MaxStepsPerFrame = 8`, pero `Advance(1.0)` con τ=0,65 exige **78**. Además, el ajuste `+1e-6` no compensa el error de `0.65f`: el cálculo indicado produce aproximadamente **77,99999814**, cuyo `floor` es 77. | No existe una implementación que satisfaga literalmente ambos contratos. | Conservar el límite de trabajo por frame y probar el segundo repartido entre frames. Definir precisión y acumulación sin un epsilon elegido para un único valor. |
| **H03** | **Bloqueante** | 5 · T5.1–T5.3 | `ContactEvent` no representa los datos que sus verificadores suponen. | Solo contiene `Step`; no instante dentro del paso. `VelocityIn/Out` se toman antes y después de todo `Simulate`, incluyendo gravedad y otros impactos. | No permite medir 2 ms, ordenar contactos cercanos ni atribuir ganancia de rapidez a cada impacto. | Separar muestras por paso de eventos de impacto. Precisar qué datos son observados, estimados o no disponibles en P1. No presentar velocidades del paso como velocidades instantáneas del contacto. |
| **H04** | **Bloqueante** | 5 · T5.2, S9 | Los objetivos de S9 confunden centro de esfera con punto de contacto. | `SolveToTarget` apunta a z=0, pero una esfera de radio 0,020 toca el frontis cuando su centro llega a z=0,020. El ejemplo numérico se detalla debajo. | Una solución correcta puede fallar precisión y clasificación de línea por culpa del escenario. | Corregir la construcción del lanzamiento y la definición de `TargetContact`. Mantener inicialmente el umbral de 1 mm para evaluar la medición corregida. |
| **H05** | **Bloqueante** | 5 · T5.2–T5.4 | El estado público no garantiza un reinicio físico equivalente; además, el gate permite una excepción incompatible con la spec. | `BallState` contiene posición, velocidad y paso. `Reset` no especifica estado de reposo, contactos previos ni otros estados internos. T5.3 permite llamar «predictor más grueso» a otro PhysX con el mismo paso si falla bit a bit. | Se puede rechazar P1 por un reinicio incompleto o aprobarlo mediante una excepción no prevista. | Distinguir repetición desde lanzamiento limpio de continuación desde un estado en vuelo. Investigar divergencias antes de atribuirlas al motor. Un predictor idéntico que diverge no pasa a ser «más grueso» por cambiarle el nombre. |
| **H06** | **Bloqueante** | Cierre de 5 · T5.3, T5.5, T6.4 | El gate tiene dos momentos de decisión contradictorios. | La salida de etapa 5 exige que el PO decida la física para avanzar. T5.3 y T6.4 dicen que el resultado completo requiere el iPhone de etapa 6. | La política de aprobar una etapa antes de empezar la siguiente genera un bloqueo de secuencia. | Declarar etapa 5 como evaluación preliminar y adelantar un ejecutor mínimo en iPhone. Cerrar una sola decisión definitiva con resultados del editor y dispositivo. |
| **H07** | **Alta** | 5 · T5.2 | Los verificadores mezclan fenómenos diferentes. | «Estar fuera del volumen interior», penetrar ligeramente y atravesar una pared no son equivalentes. Comparar rapidez entre extremos de un paso incorpora trabajo de gravedad. `OnCollisionEnter` no describe contactos persistentes. | Falsos positivos y falsos negativos pueden decidir el backend. | Separar cruce de superficie, penetración, balance energético, rebote y contacto persistente. Verificar estados finitos en **todos** los escenarios, no solo S10. |
| **H08** | **Alta** | 5 · T5.2; 8 · T8.2 | La cobertura efectiva de predicción no cumple la cobertura declarada. | §17.A.3 exige S1–S9 y dispositivo. La suite compara predicción principalmente en **S1 y S6**. Tampoco asegura explícitamente ≥3 contactos antes de medir el segundo bote. | El informe podría declarar cumplida una invariante que no ejecutó. | Crear trazabilidad requisito→escenario→medición. Registrar escenarios no evaluables; no contarlos como aprobados. |
| **H09** | **Alta** | 5 · T5.3–T5.4 | La independencia y el ciclo de vida de las escenas están incompletos. | Crear una escena no mueve automáticamente los objetos nuevos a ella. Se utiliza `Physics.gravity`, una configuración global. `IDisposable` no especifica cómo termina la descarga de escenas. | Objetos en la escena equivocada, interferencia entre pruebas y vuelo visible, o acumulación de escenas/materiales. | Explicitar pertenencia de objetos, aislamiento de configuración y limpieza verificable. Mantener un número acotado de escenas y restaurar ajustes globales usados por tests. |
| **H10** | **Alta** | 5 · T5.3; restricciones globales | El requisito de consumir el tiempo restante y limitar impactos no está demostrado para P1. | El máximo de 4 impactos es obligatorio al principio, pero informativo en T5.2 porque P1 no lo impone. Activar CCD no garantiza por sí solo procesar todos los impactos restantes. | Una etiqueta «CCD» podría sustituir la comprobación del comportamiento requerido. | Resolver la discrepancia con la spec. Medir avance después del primer impacto, contactos dobles y tiempo perdido; indicar qué controles son realmente aplicables a P1. |
| **H11** | **Alta** | 5–6 · T5.4, T6.1, T6.3 | No está cerrado el recorrido de parámetros aplicados hacia los consumidores. | `LaunchCurrent()` aplica pendientes y llama a `Ball.Launch`, cuya firma no recibe parámetros nuevos. No se especifica reconstruir `SimClock` al cambiar Hz ni actualizar τ, cancha y backend conjuntamente. | El panel puede mostrar cambios aplicados mientras la simulación conserva valores anteriores. | Definir una configuración efectiva por lanzamiento y el orden de actualización de reloj, simulaciones, geometría y predicción. Añadir una prueba integrada del valor **usado**, no solo del registro. |
| **H12** | **Alta** | 5–6 · T5.2, T6.4 | La suite síncrona no puede repartirse en frames con su contrato actual. | `Run()` devuelve `SpikeResult` sin puntos de suspensión. S8 requiere hasta **3,6 millones de pasos** a 120 Hz. Una corrutina exterior no interrumpe esa llamada. | Bloqueo de interfaz, memoria temporal elevada y mediciones contaminadas. | Permitir ejecución incremental con presupuesto de trabajo, progreso y cancelación. PhysX continúa en el hilo apropiado; no trasladarlo arbitrariamente a un worker. |

### Panel, compilación y validación

| ID | Severidad | Etapa / tarea | Problema concreto | Evidencia técnica | Consecuencia | Recomendación |
|---|---|---|---|---|---|---|
| **H13** | **Alta** | 6 · T6.1–T6.3 | El hash del preset puede dejar de describir la ejecución. | `MetricsLog` recibe nombre/hash al construirse, pero los parámetros pueden cambiar después. `Get` devuelve valores vigentes; no se define si guardar incluye pendientes. | Resultados atribuidos a configuraciones equivocadas. | Registrar una instantánea efectiva y su hash por lanzamiento o segmento. Distinguir preset guardado, edición pendiente y configuración vigente. |
| **H14** | **Media** | 6 · T6.1; 8 · T8.2 | La recuperación ante presets problemáticos está cubierta solo parcialmente. | Faltan casos explícitos de esquema antiguo, valores no finitos, claves duplicadas, opciones discretas inválidas, escritura interrumpida y error de almacenamiento. También hay defaults en el registro y en JSON. | Arranque inconsistente o pérdida de calibraciones. Copiar un preset a `DefaultPreset.json` puede romper el test que exige igualdad con defaults codificados. | Elegir una fuente de defaults; definir validación estructural y semántica, guardado atómico y recuperación. Un preset parcial debe cargarse sobre una base conocida. |
| **H15** | **Alta** | 2 · T2.2; 8 · T8.1 | El mecanismo de defines y la prueba de exclusión necesitan más precisión. | Se cambian/restauran símbolos durante un build batch sin especificar compilación. El `grep` depende de rutas, generación y limpieza del proyecto IL2CPP exportado. | Panel ausente en desarrollo, presente en producción o verificación basada en artefactos viejos. | Usar defines de compilación del Player por build, sin depender de una recarga implícita. Comprobar artefactos frescos y verificar tipos del Player, además de inspección funcional. |
| **H16** | **Alta** | 2–3 · T2.2, T3.1; 8 · T8.2 | Los scripts no tienen un contrato suficiente de éxito/error. | `test.sh` solo especifica fallar si `failed > 0`; no cubre XML ausente, viejo, ejecución abortada o cero tests. Tampoco se fija importación de `BuildInfo.json` antes del build. | Falsos «verde» y builds con identificación incorrecta. | Exigir proceso correcto, resultado nuevo, tests ejecutados y ausencia de errores de compilación. Comprobar `BuildReport`, importación de recursos y producto generado. Separar regresiones de resultados experimentales de P1. |
| **H17** | **Alta** | 6 · T6.2–T6.4; 8 · T8.2 | El protocolo de rendimiento no garantiza métricas de sesión comparables. | La capacidad de 3600 frames cubre aproximadamente un minuto, pero se valida durante 15. Solo se pide temperatura final. No se separan carga de suite, benchmark y vuelo normal. | Se pueden omitir tirones o estados térmicos previos y rechazar P1 por una build/configuración inadecuada. | Separar estadísticas acumuladas y ventana reciente; registrar peor estado térmico, duración y fases. Fijar configuración nativa, estimador del benchmark, calentamiento y costo de diagnóstico. |
| **H18** | **Media** | 4 · T4.1 | La franja diagonal no conserva 50 mm de ancho perpendicular. | La pendiente es 2,44/9,75. Crecer 50 mm verticalmente produce aproximadamente **48,50 mm** en dirección normal a la línea. | La geometría dibujada y la región de contacto no cumplen el ancho declarado por el propio plan. | Confirmar qué representa «ancho» en esa franja y resolver la interpretación de E.0. Probar tangencias según distancia normal y extremos, no solo alturas verticales. |
| **H19** | **Media** | 4, 6–7 · T4.2, T6.1, T7.1 | La primera cámara útil llega tarde y L1–L6 están definidos principalmente por intención. | T4.2 requiere revisión visual, pero el rig llega en etapa 7. No hay condiciones numéricas de aceptación para todos los lanzamientos de legibilidad. | El PO puede estar calibrando física con un encuadre deficiente o con lanzamientos que no representan lo que indican sus nombres. | Colocar cámara fija mínima en T4.2. Verificar que L1 llega a la trasera, L3 tiene el segundo bote previsto y L5 produce la oclusión buscada. |
| **H20** | **Media** | 2, 6 · Arquitectura y stack | «Core puro» no describe exactamente la propuesta; el registro genérico necesita límites. | Core utiliza `Vector2/3` y `JsonUtility`. Esto es válido en EditMode, pero sigue dependiendo de UnityEngine. `ParamDef` representa todo mediante floats y `Options` sin correspondencia explícita entre etiquetas y valores. | Purismo innecesario o conversiones ambiguas para Hz, backend y tin. | Definir Core como **sin dependencias de escena**. Conservar tipos Unity útiles y definir valores discretos explícitos. No introducir un framework general para resolverlo. |
| **H21** | **Media** | 5 · T5.5, solo si se autoriza | P2 todavía tiene huecos que impedirían conectarlo «sin cambios». | No se define asentamiento, tratamiento del tiempo sobrante al alcanzar 4 impactos ni límites superiores de los planos. Su fricción multiplicativa no equivale a fricción Coulomb de PhysX. | P2 podría pasar a tener otra semántica de parámetros o fallar S10 indefinidamente. | Completar esos contratos **antes de autorizar P2**, usando la misma suite corregida. No desarrollar P2 ahora. |
| **H22** | **Media** | 5–6 · T5.1, T6.3 | Pausa, relanzamiento y descarte de pasos no tienen un cierre completo. | No se especifica limpiar fracciones al relanzar, evitar salto de interpolación ni invalidar mediciones temporales tras descartes. «Continuar» existe en DevPanel, ausente en producción. | Primer frame extraño, tiempo perdido oculto o build de producción permanentemente pausada después de volver del fondo. | Definir reinicio de reloj/interpolación y recuperación independiente del panel. Registrar descartes y excluirlos o tratarlos explícitamente según la medición. |
| **H23** | **Baja** | 1 · T1.1 y flujo general | Una rama y PR por tarea es demasiado rígido para algunos cortes. | Hay tareas que producen infraestructura necesaria para la anterior y una excepción inevitable para crear el primer repositorio. | Más espera y PRs incompletos sin mejorar revisión. | PR por incremento coherente y verificable; mantener separadas física, panel y cambios de decisión. Explicitar la excepción de bootstrap. |
| **H24** | **Media** | 0–3 · T0.1, T0.2, T2.2, T3.1 | Hay detalles de herramientas y firma que deben corregirse o confirmarse. | Falta arquitectura ARM del editor; la comprobación de módulo omite el interior de `Unity.app`; un identificador CoreDevice no debe asumirse idéntico al UDID de Xcode; Team ID solo no configura credenciales. | Fallos de instalación, destino o firma que parecen incompatibilidad Unity–Xcode. | Verificar cada dato con la herramienta correspondiente y separar esos fallos de errores del motor. Fijar versiones y prerequisitos antes del primer build. |

### Fundamentos de los hallazgos más importantes

**1. S9 fallaría incluso con una trayectoria balística ideal.**

Para el lanzamiento hacia el borde de out:

- Inicio: `(3,2; 1; 5)`.
- Objetivo declarado: `(3,2; 4,560; 0)`.
- Velocidad horizontal: 15 m/s.
- Tiempo hasta z=0: 0,333333 s.
- Velocidad vertical calculada: 12,315 m/s.
- Tiempo hasta que la esfera toca la pared: **0,332 s**.
- Altura ideal del contacto: **4,547931 m**.

El desvío es **12,07 mm** respecto del objetivo declarado. La huella de 10 mm ni siquiera alcanza el borde inferior del out en ese caso ideal. Esto es una deducción geométrica del plan, **no una medición de PhysX**.

La corrección debe afectar al significado del objetivo y al lanzamiento. Aumentar la tolerancia escondería el error.

**2. Los eventos actuales no permiten comprobar todas las invariantes.**

Un paso de 120 Hz dura **8,333 ms**. Guardar únicamente su número no permite verificar diferencias de 2 ms entre contactos.

Además, NVIDIA distingue el estado al finalizar el paso del estado cuando se detectó el contacto. Esa diferencia importa especialmente con varios impactos dentro del mismo paso. El plan no puede tratarlos como equivalentes. [Documentación de simulación de PhysX](https://nvidia-omniverse.github.io/PhysX/physx/5.7.0/docs/Simulation.html).

Para las esquinas, hay que separar el efecto del contacto del trabajo de gravedad. Una comprobación energética del intervalo puede ser útil, pero debe declararse como tal; no sustituye automáticamente datos por impacto.

**3. Igualdad bit a bit merece una prueba válida, no una promesa ni una excepción nominal.**

Mantendría la exigencia aprobada mientras se investiga:

- repetición desde un lanzamiento limpio;
- predicción calculada desde el lanzamiento;
- predicción desde un estado intermedio;
- reutilización después de contactos y reposo.

No asumiría que copiar posición y velocidad reproduce todo el estado relevante de PhysX. Su documentación condiciona la repetibilidad a escena, secuencia y configuración equivalentes. [Buenas prácticas de PhysX](https://nvidia-omniverse.github.io/PhysX/physx/5.1.1/docs/BestPractices.html).

Si el PO decide aceptar predicción aproximada con el mismo backend y paso, eso requiere **una modificación explícita del criterio**, no describir retrospectivamente la divergencia como un predictor grueso.

**4. CCD requiere comprobar el comportamiento concreto.**

La documentación nativa de PhysX explica que su CCD puede efectuar un número limitado de pasadas y descartar tiempo restante. Esto no demuestra qué hará exactamente el parche de Unity elegido, pero sí invalida asumir que «ContinuousDynamic» garantiza consumir todo el intervalo. [Advanced Collision Detection](https://nvidia-omniverse.github.io/PhysX/physx/5.1.3/docs/AdvancedCollisionDetection.html).

También debe distinguirse generación anticipada de contactos de penetración real: Unity genera contactos según la **suma** de los `contactOffset` de ambos colliders. Reducir ese parámetro no equivale a obtener un punto de impacto exacto. [Collider.contactOffset](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/Collider-contactOffset.html).

### Validez de S1–S10

| Escenario | Evaluación |
|---|---|
| **S1 — Drive rápido** | Útil. Debe registrar que efectivamente hubo el número de contactos requerido y definir su horizonte. |
| **S2 — Drop bajo** | Útil para rebote bajo y legibilidad. Su precisión depende de corregir la distinción entre objetivo de contacto y centro de esfera. |
| **S3 — Rasante** | Bien elegido. Añadir comprobación del ángulo real de incidencia para evitar una prueba mal construida. |
| **S4 — Lateral/suelo** | Necesario para estabilidad. No debe interpretarse como validación de un nick auténtico. |
| **S5 — Frontis/lateral** | Útil para contactos próximos. Hace falta observar orden, multiplicidad y desplazamiento posterior. |
| **S6 — Dos contactos por paso** | Esencial. Verificar que ambos ocurren realmente en el mismo paso; el nombre del escenario no basta. |
| **S7 — Pared trasera** | Útil para física y lectura de profundidad. Precisar contactos y duración esperados. |
| **S8 — Aleatorios** | Buena prueba de robustez, pero no exhaustiva. Distinguir salida legítima por arriba de las paredes de atravesarlas. Añadir casos dirigidos cerca de bordes y comprobar estados finitos. |
| **S9 — Líneas** | **Inválido tal como se construye.** Requiere la corrección geométrica indicada. |
| **S10 — Asentamiento** | Aporta valor. Ignorar `OnCollisionStay` no debe permitir que vibración o contactos persistentes desaparezcan del diagnóstico. Añadir reposo seguido de nuevo lanzamiento. |

No cambiaría automáticamente los umbrales:

| Criterio | Tratamiento recomendado |
|---|---|
| **0/10 000 atravesamientos** | Conservar. Separar atravesamiento, penetración numérica y escape por arriba. |
| **Sin aumento de rapidez en esquina** | Conservar su intención física; corregir la medición para aislar el contacto. |
| **Bit a bit** | Conservar según la spec; corregir cobertura y reinicio antes de concluir. |
| **≤1 cm / ≤2 ms** | Conservar para la alternativa prevista; los 2 ms requieren instrumentación con resolución suficiente. |
| **≤1 mm en S9** | Primero corregir escenario. Después evaluar si contacto del motor y referencia permiten una comparación válida. |
| **Penetración/tolerancia de 0,1 mm** | No usarla como sinónimo de atravesamiento. Es un criterio adicional muy exigente que necesita definición y justificación. |
| **Predicción ≤1 ms** | Mantener como objetivo experimental; fijar qué incluye y qué estadístico decide el gate. No aceptar solo la mediana sin acordarlo. |
| **FPS y percentiles** | Mantener objetivos; corregir protocolo, configuración y agregación antes de juzgarlos. |

## C. Revisión de dependencias

### Ensamblados

**No hay un ciclo de referencias necesario en el diseño propuesto.**

- Runtime depende de Core.
- DevPanel depende de Runtime y Core.
- Editor depende de Runtime y Core.
- Runtime no debe depender de DevPanel.

Los cuatro ensamblados son proporcionales. DevPanel justifica una frontera de compilación; Editor mantiene fuera del Player las APIs de edición. La afirmación de que Unity «exige» un asmdef Editor separado es demasiado absoluta, pero crearlo aquí es razonable.

No convertiría Core en una biblioteca .NET independiente: **usar vectores de Unity y pruebas EditMode sin escena es suficiente** para este prototipo.

URP, UI Toolkit e Input System son opciones coherentes. No añaden valor inmediato:

- otro sistema de UI;
- generación por reflexión;
- un framework universal de parámetros;
- una capa de servicios para cada clase;
- interfaces adicionales para futuros golpes, IA o red.

`IBallSimulation` y su fábrica sí están justificados por la comparación autorizada P1/P2. Necesitan un contrato más preciso sobre reinicio, propiedad de recursos, configuración y eventos; no más capas.

### Dependencias entre entregables

| Dependencia | Inconsistencia | Ajuste |
|---|---|---|
| Herramientas → proyecto | El helper necesita el proyecto que debería crear. | Resolver editor primero; validar versión del proyecto después. |
| Configuración física → suite | Los defaults llegan en T6.1, después de la prueba de T5.3. | Adelantar datos mínimos de configuración, sin adelantar todo el panel. |
| Suite → iPhone | La API es síncrona, pero se promete progreso por frames. | Un único ejecutor incremental reutilizable por test y panel. |
| Parámetros → host | La aplicación no entrega explícitamente la configuración nueva a todos los consumidores. | Configuración efectiva por lanzamiento, con instantánea identificable. |
| Física → aprobación de etapa | La etapa 5 necesita una decisión que depende de etapa 6. | Evaluación preliminar y gate final único en dispositivo. |
| Geometría → validación visual | Se pide reconocer la cancha antes del rig de cámara. | Cámara fija básica junto con la cancha. |
| Build → exclusión del panel | La comprobación de producción llega al final. | Probar una pareja dev/prod cuando aparece el primer DevPanel. |
| Calibración → defaults | Dos fuentes de defaults dificultan promover el preset del PO. | Una fuente de valores; registro separado de límites y etiquetas. |
| Suite experimental → regresión | T8.2 tolera fallos históricos de P1 dentro de la batería general. | Separar resultados del experimento de tests obligatorios del backend seleccionado. |

### Compilación y herramientas iOS

Los comandos de instalación y lanzamiento son válidos en el Xcode inspeccionado. **`--console` existe**, pero la ayuda local confirma que espera hasta que la aplicación termine. Por tanto, conviene separar «instalar y lanzar» de «mantener una sesión de consola».

También corregiría:

- **Editor ARM64:** especificarlo expresamente. La referencia del Hub CLI documenta selección de arquitectura y marca ese CLI como obsoleto desde Hub 3.18; no hace falta instalar otra herramienta si la UI existente resuelve esta preparación. [Hub CLI reference](https://docs.unity.com/en-us/hub/hub-cli-reference).
- **Ruta del módulo:** comprobar el soporte iOS dentro de `Unity.app/Contents/PlaybackEngines`, según la instalación efectiva.
- **Firma:** confirmar cuenta de desarrollo configurada en Xcode, certificado/perfil y registro del dispositivo. `-allowProvisioningUpdates` no sustituye las credenciales; `xcodebuild -help` lo indica expresamente.
- **Identificador del dispositivo:** usar para Xcode un destino reconocido por Xcode. No asumir que cualquier identificador mostrado por CoreDevice sirve indistintamente.
- **Versiones:** fijar parche de Unity, manifest y `packages-lock.json`. «La versión que propone por defecto» debe convertirse en una selección concreta y reproducible.
- **Entorno:** cargar explícitamente Team ID y bundle ID donde corresponda; tenerlos en una variable del shell no garantiza que el proceso Unity los reciba.
- **Defines:** `BuildPlayerOptions.extraScriptingDefines` permite símbolos específicos para compilar el Player. Para producción hay que garantizar también que `SQUASH_DEV` no venga heredado de ajustes persistentes. [API oficial](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/BuildPlayerOptions-extraScriptingDefines.html).
- **BuildInfo:** importar el JSON generado antes de compilar y registrar también configuración efectiva; un hash Git con cambios locales no identifica por sí solo todo el contenido construido.
- **Producción:** una build no Development instalada con firma de desarrollo sirve para comprobar exclusión del panel. No necesita convertirse ahora en distribución App Store.

No aplicaría ajustes de linker genéricos ni instalaría Xcode 26 por anticipado. Si aparece un fallo, primero identificaría si pertenece a credenciales, destino, exportación Unity, compilación C++ o enlace.

## D. Propuesta de ajustes

### Correcciones obligatorias antes de implementar las partes afectadas

1. **Reordenar el bootstrap y unificar el gate de física.**
2. **Resolver reloj, eventos, reinicio y configuración efectiva.**
3. **Corregir S9 y completar la cobertura exigida por la spec.**
4. **Hacer incremental la suite y definir limpieza de escenas.**
5. **Cerrar la compilación dev/prod y los criterios de éxito de los scripts.**
6. **Definir la procedencia real de cada medición: configuración, build, dispositivo y fase.**

Estas correcciones no modifican decisiones deportivas aprobadas.

Respecto de E.0, mantengo como base el origen, ejes, alturas de referencia, short line y tin seleccionados. La observación sobre la diagonal es una inconsistencia geométrica de su representación. Conviene que Claude adjunte enlace y edición exacta de la fuente WSF: la consulta directa a la página/PDF de WSF devolvió 403 en esta revisión, por lo que **no doy por recertificadas aquí todas las cláusulas constructivas**.

Sí pude contrastar las reglas de saque directo al frontis y contactos de línea con la fuente de arbitraje. La separación entre «qué franja toca» y «qué consecuencia tiene por fase» es correcta; no incorporaría puntuación o arbitraje completo en First Ball. [World Squash Officiating — reglas 5.7–6.2](https://worldsquashofficiating.com/rules-of-squash/).

### Mejoras recomendadas

- Adelantar cámara fija, lanzamiento único y controles mínimos en iPhone.
- Aplicar cambios físicos como una operación completa al relanzar.
- Ocultar o marcar como no operativo cualquier parámetro que P1 no implemente, como un límite de impactos no controlable.
- Guardar archivos de presets de forma atómica y conservar un fallback conocido.
- Registrar configuración efectiva al cambiar de lanzamiento o preset.
- Medir CPU/GPU, asignaciones temporales y estado térmico máximo, además de tiempos de frame.
- Ejecutar el benchmark fuera de la sesión de legibilidad y de la prueba sostenida.
- Fijar una configuración nativa optimizada para comparar física. **Development Build no obliga a usar compilación C++ Debug.**

### Elementos que pueden esperar

- P2 y sus detalles de implementación, hasta que exista un fallo válido de P1.
- Un sistema general de migración de múltiples versiones de presets.
- Automatización extensa de GitHub.
- Autoría mediante ScriptableObjects si se adopta JSON como fuente única.
- Optimización avanzada del renderer, servicios genéricos y cualquier preparación para multijugador.

La política de revisión con Claude merece conservarse. La unidad más útil es **un incremento revisable y comprobable**, no necesariamente cada tarea numerada. Mantendría la validación del PO en los puntos acordados, evitando añadir aprobaciones para ajustes internos rutinarios.

## E. Recomendación de ejecución

Después de corregir el plan y recibir autorización, comenzaría así:

1. **Compatibilidad mínima primero.**  
   Unity 6000.3 con iOS, dispositivo físico identificado y credenciales confirmadas. Proyecto mínimo vertical, UI Toolkit sencilla, IL2CPP/ARM64. Exportar, compilar, firmar, instalar y lanzar. Confirmar también una build no Development. El remoto GitHub no necesita bloquear esta prueba.

2. **Primera pelota visible cuanto antes.**  
   Cámara fija, cancha básica, una esfera PhysX y botones mínimos para lanzar, pausar y repetir. Sin esperar al registro completo de parámetros ni al panel autogenerado.

3. **Prueba técnica corregida de P1.**  
   Configuración congelada, eventos con semántica definida, escenarios construidos correctamente y comparación desde lanzamiento y estado intermedio. Repetir en editor y iPhone dentro de cada binario, sin exigir igualdad entre ambos.

4. **Gate definitivo de física.**  
   Cerrarlo con resultados válidos del dispositivo, cobertura completa y benchmark con configuración identificada. Si falla P1, distinguir defecto del adaptador, defecto de la prueba y limitación real. Solo esta última, o una complejidad desproporcionada para corregirla, justificaría proponer P2.

5. **Completar calibración y First Ball.**  
   Presets, segunda cámara, ayudas y L1–L6. Finalmente, sesión sostenida de 15 minutos y validación deportiva del PO. Esta última confirma el sistema integrado; no debe ser la primera medición útil en el teléfono.

No creé archivos, repositorios ni código, no instalé herramientas y no inicié Unity. Las huellas SHA-256 del plan y la spec permanecen iguales a las tomadas al empezar.

**Decisiones del PO antes de autorizar la implementación:**

- Aceptar la corrección de secuencia y el único gate definitivo de física en iPhone.
- Confirmar que se conserva la exigencia bit a bit según la spec; cualquier excepción debe aprobarse explícitamente.
- Ratificar objetivos, magnitudes y protocolo de los criterios de contacto y rendimiento, sin relajar aún sus valores.
- Elegir una única fuente de defaults y la unidad de revisión por PR.
- Confirmar Team ID, bundle ID y disponibilidad del iPhone físico para la primera prueba.
