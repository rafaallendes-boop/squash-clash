# Respuesta a la revisión técnica del plan First Ball (Claude, 2026-10-20)

**Entrada:** la revisión de Codex (`2026-10-20-first-ball-plan-review-codex.md`, veredicto **APTO CON AJUSTES**, 24 hallazgos y 6 bloqueantes) y las directrices del PO del 2026-10-20.
**Salida:** el plan corregido `docs/superpowers/plans/2026-10-20-squash-clash-first-ball.md` (r2). La versión anterior queda en `…-first-ball.r1.md`.
**Base de contraste:** la spec v0.3 con la enmienda E.0. **No se modificó la spec.** Las contradicciones encontradas están en la sección 4, para que el PO las autorice.

**Resumen:** acepto 21 hallazgos, acepto 3 parcialmente (H10, H21 y H24) y no rechazo ninguno. Las aceptaciones parciales conservan una regla de la spec o una directriz del PO que el hallazgo, aplicado al pie de la letra, habría cambiado: no inventar umbrales para §8.3, no desarrollar P2 por adelantado y no automatizar la instalación de Unity con una CLI obsoleta. **Ningún umbral aprobado se relaja.** Se elimina un umbral que no venía de la spec sino de la r1 (la tolerancia de 0,1 mm en el atravesamiento), porque mezclaba dos fenómenos distintos.

---

## 1. Matriz de respuesta

| ID | Sev. | Respuesta | Justificación técnica | Cambio concreto en el plan | ¿Decisión del PO? |
|---|---|---|---|---|---|
| **H01** | Bloq. | **Aceptado** | Es cierto: `unity-env.sh` exigía `ProjectVersion.txt` antes de crear el proyecto, y el script lo usaba la tarea anterior a la que lo creaba. | `unity-env.sh` solo resuelve el editor y comprueba la versión del proyecto **si existe** (T1.2). Las antiguas T2.1, T2.2 y T3.1 pasan a ser un solo incremento (I1: T1.2 y T1.3) que termina con la app en el iPhone. | No |
| **H02** | Bloq. | **Aceptado** | Dos errores reales. `Advance(1.0)` → 78 contradecía el tope de 8 pasos por frame. Y `+1e-6` no compensa el error de `0.65f`: `0.65f·120 ≈ 77,999997`, con un error mayor que ese margen. | τ llega como `double` desde `ParamSet`. El margen pasa a `1e-9`, que es ruido de `double` y no depende de τ. Los tests ahora reparten el segundo en 60 frames (total 78, ninguno > 8) y piden 10 minutos sin deriva (46 800 pasos) (T3.1). | No |
| **H03** | Bloq. | **Aceptado** | `Step` no permite medir 2 ms ni ordenar contactos dentro del paso. Las velocidades antes y después de `Simulate` incluyen la gravedad y otros impactos. | `ContactEvent` lleva `Normal`, `TimeInStep` y **`ContactTiming` (StepOnly, Estimated o Exact)**, que declara qué tan medido es el instante, más `ApproachVelocity` observada y `Persistent`. Se quitan `VelocityIn/Out`. Los estados por paso quedan separados en `Trajectory.States`. En P1 el instante es `Estimated` y se dice así (T3.1, T3.2). | No |
| **H04** | Bloq. | **Aceptado** | Verifiqué el cálculo de Codex: apuntar el centro a z = 0 produce un contacto ideal a 4,5479 m, 12 mm por debajo del objetivo. El escenario fallaba con una física perfecta. | Se distinguen **punto de contacto** (sobre la superficie) y **centro** (`punto + normal·r`), con `Ballistics.CenterForContact`. Cada escenario tiene una **comprobación de construcción** pura. S9 pasa a tener 9 lanzamientos por línea: 3 mm fuera, en el borde exacto y 3 mm dentro. El borde exacto mide precisión y los otros dos, clasificación sin ambigüedad. **Se conserva el umbral de 1 mm** (T4.1, C10, C11). | **Sí, D-1** (el 1 mm es un umbral del plan, no de la spec) |
| **H05** | Bloq. | **Aceptado** | Las dos partes son correctas. (1) Un `Reset` que solo copia posición y velocidad no garantiza el mismo estado interno de PhysX. (2) La r1 permitía llamar "predictor más grueso" a otro PhysX con el mismo paso. §8.2.1 define "más grueso" como **otro paso u otro código**, así que esa salida no era legítima. | El contrato de `Reset` es de comportamiento: **equivale a una instancia nueva**, y se mide (C8). Se separan el lanzamiento limpio (C6), el relanzamiento en vuelo (C7, que modela el contacto de raqueta) y la continuación sin reinicio (C9, diagnóstico). El gate prohíbe expresamente reclasificar una divergencia (T3.2, T4.2, gate). | **Sí, D-3** (C7 decide y C9 solo informa) |
| **H06** | Bloq. | **Aceptado** | La r1 pedía la decisión del PO al cerrar la Etapa 5, pero los datos del iPhone llegaban en la Etapa 6. | El editor da una **evaluación preliminar** (T4.3). La ejecución en el iPhone se adelanta a la misma etapa (T4.4), con la barra mínima de I3 y sin esperar al panel. Hay **un único gate** con los dos informes [directriz 8]. | No (lo pidió el PO) |
| **H07** | Alta | **Aceptado** | Penetrar, atravesar y salir por arriba son cosas distintas. Comparar la rapidez entre los extremos de un paso incluye el trabajo de la gravedad. Ignorar `OnCollisionStay` escondía la vibración. | Mediciones separadas (T4.2): `Crossings` (C1), `ExitedOverTop`, `MaxPenetration` (C16, informativa), `AllFinite` en **todos** los escenarios (C2), **energía mecánica** por paso de esquina (C3) y `RepeatedImpacts` / `PersistentSamples` (C4, C16). Se elimina la tolerancia de 0,1 mm de la r1, que no venía de la spec. | No. Revisar la decisión de Claude sobre C3 (sección 4) |
| **H08** | Alta | **Aceptado** | La r1 solo comparaba la predicción en S1 y S6. La spec pide S1–S8 (§8.2.2) y S1–S9 en el dispositivo (§17.A.3). | Tabla de criterios trazables: requisito, escenarios, medición, umbral y si decide el gate. C6 y C7 cubren S1–S10 y **las 10 000** de S8. Nuevo veredicto `NotEvaluated`, que **nunca cuenta como aprobado**. El informe indica cuántos casos se evaluaron. El requisito de ≥ 3 contactos solo rige para un predictor más grueso, que P1 no es (se dice explícitamente). | No |
| **H09** | Alta | **Aceptado** | `CreateScene` no mueve los objetos nuevos; `Physics.gravity` es global; `UnloadSceneAsync` no termina de inmediato. | `MoveGameObjectToScene` explícito. La fábrica fija la gravedad al crear y lanza un error si hay instancias vivas con otra. Contador de `PendingUnloads`, destrucción de materiales y tests de regresión de aislamiento y limpieza (T3.2). Como máximo tres instancias vivas en la suite (T4.3). | No |
| **H10** | Alta | **Parcial** | Es cierto que CCD no garantiza consumir el tiempo restante y que la r1 rebajaba el tope a "informativo" sin decirlo. Pero §8.3 no fija umbrales numéricos, y **inventar uno ahora sería cambiar un criterio**. | §8.3.1–8.3.3 y 8.3.7 se **miden** (C12–C15): primer impacto frente a la referencia balística, fracción de tiempo restante consumida, impactos por paso y simultaneidad. Claude los analiza con evidencia en el gate. `physics.maxImpactsPerStep` aparece en el panel como "no operativo con P1". | **Sí, D-4** (y contradicción C-2 de la sección 4) |
| **H11** | Alta | **Aceptado** | `Launch(spec, λ)` no recibía la configuración nueva, y no estaba definido reconstruir el reloj ni la cancha. | `EffectiveConfig`: una instantánea inmutable por lanzamiento con `SimulationKey`. Un orden fijo en `Launch(cfg)`: simulaciones, reloj, reinicio, predicción, interpolación y cancha. Un test integrado comprueba el **valor usado** en cada consumidor (T2.2, T3.3). | No |
| **H12** | Alta | **Aceptado** | Una corrutina que llama a un `Run()` síncrono no reparte el trabajo. | `SpikeRun.Step(budgetMs)` y `PredictionBenchmarkRun.Step(budgetMs)`, con progreso y cancelación, en el hilo principal. En el iPhone, 8 ms por frame y la pantalla siempre encendida durante la prueba (T4.3, T4.4) [directriz 7]. | No |
| **H13** | Alta | **Aceptado** | El hash del preset fijado en el constructor no describía la ejecución. | Cada línea de `MetricsLog` lleva `presetName`, `presetFileHash`, **`effectiveHash`** (del `EffectiveConfig` que se usó), `pending`, `phase` y `droppedSinceRestart`. Guardar un preset escribe los valores **visibles** (aplicados y pendientes) (T5.1, T5.2). | No |
| **H14** | Media | **Aceptado** | Faltaban casos y había dos fuentes de valores por defecto, así que promover el preset del PO rompía un test. | **Una sola fuente: `DefaultPreset.json`** [directriz 11]. El catálogo no tiene campo `Default`. `LoadDefault` es estricta y `Overlay` es parcial sobre la base, con una tabla de casos: desconocida, no finita, fuera de rango, opción inválida o duplicada. Escritura atómica con `.tmp`, error de almacenamiento sin excepción y arranque con un `last-active` corrupto. No hay migraciones entre esquemas (T2.2, T5.1). | **Sí, D-5** (contradicción C-1 con §13.4) |
| **H15** | Alta | **Aceptado** | Cambiar y restaurar defines persistentes en batch es frágil, y un grep sobre carpetas viejas puede dar un falso OK. | Defines por build con `extraScriptingDefines`. `prod` falla si los defines persistentes contienen `SQUASH_DEV`. Carpetas limpias con control de frescura, comprobación funcional en el log del arranque (`devpanel=present/absent`) y la primera pareja `dev`/`prod` en I3, no al final (T1.3, T3.3). | No |
| **H16** | Alta | **Aceptado** | `test.sh` podía dar verde sin ejecutar tests, y la regresión mezclaba los fallos experimentales de P1. | Contrato de `test.sh`: XML nuevo, `total ≥ 1`, `failed = 0` y sin errores de compilación. Un test comprueba que un filtro vacío falla. `BuildInfo.json` se importa y se verifica antes del build, y se comprueba `BuildReport`. **Ensamblado aparte de experimentos** y `spike.sh` [directriz 10] (T1.3, T4.3). | No |
| **H17** | Alta | **Aceptado** | Una ventana de 3600 frames no describe 15 minutos, y medir solo la temperatura final esconde picos. | Histograma de sesión más una ventana para mostrar. Peor estado térmico y tiempo en cada estado. CPU y GPU con `FrameTimingManager`. Recolecciones de GC. Fases (`free`, `spike`, `benchmark`, `session`). Protocolo de sesión y de benchmark (20 de calentamiento y 200 medidas). Perfil `dev` **no Development** con IL2CPP y Xcode en Release (T5.2, T7.2, tabla de perfiles). | **Sí, D-2** (estadístico de ≤ 1 ms) |
| **H18** | Media | **Aceptado** | Lo verifiqué: con una pendiente de 2,44/9,75, crecer 50 mm en vertical da 48,5 mm perpendiculares. | La franja lateral crece **50 mm en perpendicular**, con `Growth` = (0,24278; 0,97012). Hay tests por distancia normal, por tangencia exacta y en el extremo del frontis (T2.1). La fuente WSF está en la sección 5. | **Sí, D-6** (aclaración C-4 para la spec) |
| **H19** | Media | **Aceptado** | Validar la cancha sin cámara y calibrar con lanzamientos que no se comprueban era débil. | Cámara fija junto con la cancha (T2.3) [directriz 9]. L1–L6 con **condiciones numéricas** sobre la trayectoria simulada, que se ajustan cambiando el lanzamiento y nunca la condición (T6.3). | No |
| **H20** | Media | **Aceptado** | "Core puro" era impreciso, y `float` con `Options` sin valores era ambiguo. | Core se define como "sin escena ni MonoBehaviours", con tipos de Unity y `JsonUtility`. `ParamKind` (Float, Int, Bool o Choice) y `ChoiceOption` con su valor explícito. Valores en `double`. No se agrega ningún framework (T2.2). | No |
| **H21** | Media | **Parcial** | Los huecos son reales, pero el PO pidió no desarrollar P2 por adelantado. Completar ahora su contrato sería trabajo anticipado. | T4.5 lleva una **condición previa**: antes de pedir la autorización, Claude completa el asentamiento, el tiempo sobrante al llegar al tope, los límites de los planos y la equivalencia de la fricción. Sin autorización no se escribe nada de P2. | Solo si el gate lleva a P2 |
| **H22** | Media | **Aceptado** | Sin el panel, la build de producción quedaba en pausa para siempre, y faltaba reiniciar la interpolación. | En pausa, el tiempo real se descarta. `Restart()` limpia la fracción al lanzar. La interpolación arranca en el estado inicial. `ResumeOverlay` en Runtime ("Toca para continuar") funciona en producción. Los segmentos con pasos descartados quedan marcados (T3.1, T3.3, T5.2). | No |
| **H23** | Baja | **Aceptado** | Un PR por tarea numerada obligaba a hacer PR incompletos. | PR por incremento coherente, con una tabla de incrementos. Física, panel y decisiones van separados. La excepción de bootstrap queda explícita: I1 va en `main` y el remoto no bloquea la primera prueba [directriz 12]. | No (lo pidió el PO) |
| **H24** | Media | **Parcial** | Acepto: el editor arm64, las dos rutas posibles del módulo iOS, la cuenta en Xcode, los identificadores separados, el lock de paquetes, la exportación de variables y que `--console` bloquea. **Mantengo** que el PO instale Unity desde la interfaz de Hub, porque la CLI de Hub es obsoleta y la instalación es interactiva de todas formas. **Ajusto** el orden ante un fallo: primero se clasifica y no se aplican flags de linker genéricos. | T0.1 (Hub por interfaz, arm64 y verificación por archivo), T0.2 (cuenta de Xcode, `xctrace` y `devicectl`, `DEVICE_UDID` y `DEVICE_ID`), T1.2 (`set -a` y lock), T1.3 (clasificación del fallo, `--console` aparte). | No |

**Recuento:** 21 aceptados y 3 parciales (H10, H21 y H24). H04 y H05 se aceptan completos, pero cada uno deja una decisión de medición al PO (D-1 y D-3).

---

## 2. Plan corregido

Está en `docs/superpowers/plans/2026-10-20-squash-clash-first-ball.md` (r2). Al final tiene una tabla que relaciona cada tarea de la r1 con su tarea en la r2.

**Secuencia nueva, según la prioridad del PO:**

| Etapa | Contenido | Qué ve el PO en el iPhone |
|---|---|---|
| E0 | Entorno y compatibilidad | — |
| E1 | Proyecto Unity y primera instalación (`dev` y `prod`) | La pantalla de arranque |
| E2 | Cancha, configuración base y **cámara fija** | La cancha |
| E3 | **Primera pelota visible:** reloj, contrato, PhysX, host, barra mínima (Lanzar, Pausa, Paso, Repetir) y pausa del sistema | La pelota rebotando |
| E4 | Prueba de física corregida, editor e iPhone, y **gate único** | Escenarios S1–S10 y el botón de la prueba |
| E5 | Panel, presets y calibración | El panel completo |
| E6 | Seguimiento, ayudas y L1–L6 | La legibilidad |
| E7 | Validación integral | La sesión de validación |

**Paralelo y aprobaciones:** están en la sección "Modelo de trabajo" del plan. En resumen, la configuración y el diagnóstico (T5.1 y T5.2) avanzan durante la prueba de física, y la cámara de seguimiento y las ayudas (T6.1 y T6.2) durante el panel. Necesitan aprobación del PO: el remoto de GitHub, otro Xcode, la decisión del gate (incluida P2), cualquier cambio de umbral o de la spec, y promover un preset a valor por defecto.

---

## 3. Diferencias respecto del plan anterior (r1 → r2)

**Secuencia y entrega:**
1. La primera pelota llega en E3 en vez de en la Etapa 6, y la cámara fija en E2 en vez de en la Etapa 7.
2. Las antiguas Etapas 2 y 3 se unen en un incremento (I1) que ya instala `dev` y `prod`.
3. Un único gate de física, después del editor y del iPhone. La ejecución en el iPhone ya no espera al panel.
4. PR por incremento, en vez de por tarea, con la excepción de bootstrap explícita.

**Física (sin relajar umbrales):**
5. Reloj: τ en `double`, tests de tope y de deriva coherentes, tiempo descartado en pausa y `Restart` al lanzar.
6. Contactos: instante con su calidad declarada (`ContactTiming`), normal, velocidad de aproximación observada, contactos persistentes, y estados por paso separados de los eventos.
7. Escenarios: punto de contacto frente a centro, comprobación de construcción, S9 con 9 lanzamientos y S8d (36 lanzamientos dirigidos a aristas y vértices) como cobertura adicional del mismo invariante.
8. Mediciones separadas: atravesamiento, salida por arriba, penetración, energía, vibración y persistentes.
9. Criterios C1–C18 trazables, con `NotEvaluated` y ejecuciones incompletas que nunca aprueban.
10. Predicción en S1–S10 y en todo S8: lanzamiento limpio (C6), relanzamiento en vuelo (C7), reutilización (C8) y continuación como diagnóstico (C9). Se elimina la salida de "predictor más grueso" para P1.
11. §8.3 medido con evidencia (C12–C15), sin inventar umbrales.
12. Adaptador PhysX con escena propia verificable, gravedad controlada, limpieza comprobada y `sleepThreshold = 0`.
13. Suite incremental (`SpikeRun`) y experimentos de PhysX en un ensamblado aparte, con `spike.sh`. Las variantes de PhysX solo se prueban si un criterio de gate falla.
14. Se elimina la tolerancia de 0,1 mm (r1) y el máximo de 200 contactos de S10 (r1), que no venían de la spec.

**Configuración, panel y builds:**
15. `DefaultPreset.json` como única fuente. El catálogo no tiene valores por defecto, `LoadDefault` es estricta y `Overlay` es parcial con base conocida.
16. `EffectiveConfig` por lanzamiento, con el hash efectivo en cada línea del log.
17. Presets atómicos y casos robustos.
18. Tipos de parámetro explícitos y parámetros "no operativos con P1".
19. Perfiles `dev`, `prod` y `debug`. Las mediciones se hacen en `dev`, que no es Development y compila en Release.
20. Defines por build, `check-release.sh` con carpetas limpias y comprobación funcional desde I3.
21. Contrato estricto de `test.sh` y `BuildInfo` con `gitDirty` e importación verificada.
22. Métricas de sesión completas: histograma, peor estado térmico, CPU y GPU, GC y fases.
23. Pausa recuperable en producción (`ResumeOverlay`).
24. L1–L6 con condiciones numéricas, en su propia tarea (T6.3).
25. Diagonal lateral con 50 mm de ancho perpendicular.

**Entorno:**
26. Unity se instala desde la interfaz de Hub, con el editor arm64 y la ruta del módulo verificada.
27. Cuenta de Apple en Xcode y UDID para `xcodebuild`.
28. `--console` en un modo aparte.
29. Ante un fallo de build, primero se clasifica.
30. `packages-lock.json` versionado.

---

## 4. Decisiones y preparativos pendientes

### 4.1 Decisiones del PO antes de autorizar la implementación

| ID | Decisión | Propuesta de Claude | Por qué hace falta |
|---|---|---|---|
| **D-1** | Umbral de precisión del contacto en S9 (C10) | **≤ 1 mm**, como [EXPERIMENTO], y clasificación correcta a ±3 mm (C11) | La spec define S9, pero no su umbral. El 1 mm viene del plan y decide el gate. |
| **D-2** | Estadístico de "predicción ≤ 1 ms" (C17) | **p95 ≤ 1 ms** de 200 mediciones en el iPhone, tras 20 de calentamiento, con la mediana, el p99 y el máximo en el informe | La spec dice "≤ 1 ms por predicción" sin estadístico. El máximo absoluto depende del ruido del sistema operativo, y la mediana sola sería demasiado permisiva. |
| **D-3** | Forma de medir "predicción desde el estado de simulación" | **C7 decide:** en un relanzamiento en vuelo (como el contacto de raqueta), juego y predicción se reinician con el mismo estado. **C9 solo informa:** la predicción reiniciada frente a un juego que sigue sin reiniciarse. | Es como el host cumple la condición 1 de §8.2.1. Exigir C9 haría depender el gate de estado interno de PhysX que el juego nunca necesita reproducir. |
| **D-4** | §8.3 con P1 | Medir §8.3.1–8.3.3 y 8.3.7 (C12–C15) y decidir en el gate con evidencia, sin umbral numérico nuevo. `physics.maxImpactsPerStep` aparece como "no operativo con P1". | La spec dice que §8.3 vale "para cualquier alternativa" y pide un control de "impactos por paso" (§13.2), pero PhysX no permite fijar ese tope (contradicción C-2). |
| **D-5** | Fuente de valores por defecto | `DefaultPreset.json`, tal como pide la directriz 11 | Contradice §13.4 y el anexo A, que proponen ScriptableObjects (contradicción C-1). |
| **D-6** | Ancho de la diagonal lateral | 50 mm **perpendiculares**, manteniendo el borde inferior como referencia | Precisa una interpretación marcada en §8.7.2 (contradicción C-4). |

### 4.2 Contradicciones con la spec (no la modifiqué: pido autorización para registrarlas)

| ID | Dónde | Contradicción | Propuesta de registro |
|---|---|---|---|
| **C-1** | §13.4 y anexo A | Proponen ScriptableObjects para los valores por defecto; la directriz 11 del PO pide `DefaultPreset.json` como única fuente. | Cambiar §13.4 y el anexo A a "valores por defecto en `DefaultPreset.json`, con validación de esquema, tipos y rangos". Las dos fuentes están marcadas [PROPUESTA], así que no se cambia ninguna decisión aprobada. |
| **C-2** | §8.3 (encabezado y 8.3.2) y §13.2 | Los requisitos valen "para cualquier alternativa", pero P1 no puede fijar un tope de impactos por paso. | Nota en §8.3: con P1, el tope es una propiedad medida, no configurable. Se registra junto con la decisión del gate (T7.3). |
| **C-3** | §8.2.2 frente a §17.A.3 | La primera pide bit a bit en S1–S8; la segunda, en S1–S9 y en el dispositivo. | Unificar en "S1–S9 (y S10) y en el dispositivo". El plan ya cubre la unión, así que **no bloquea**. |
| **C-4** | §8.7.2 | "La franja crece 50 mm hacia arriba desde el borde inferior" admite leer el ancho en vertical (48,5 mm perpendiculares). | Cambiar a "50 mm de ancho perpendicular a la diagonal". |

Propongo registrar C-1 a C-4 en la Tarea 7.3, junto con la decisión de física, como una enmienda E.2 de la v0.3. Si el PO prefiere hacerlo antes de la implementación, también es posible. En los dos casos, el plan ya funciona con las propuestas.

### 4.3 Decisiones de Claude incluidas en el plan (para revisar al leerlo)

- C3 mide la energía mecánica del paso de esquina en vez de la rapidez entre los extremos del paso, con una tolerancia de 1e-5 por redondeo de `float`.
- S8d: 36 lanzamientos dirigidos a aristas y vértices, como cobertura adicional del invariante de atravesamiento. Decide el gate, igual que S8.
- El perfil `dev` de medición no es un Development Build (IL2CPP y Xcode en Release). El perfil `debug` queda solo para diagnosticar.
- `sleepThreshold = 0` en la pelota de PhysX, para que no se duerma durante una medición.
- `DevPanel` compila también en el editor (`SQUASH_DEV || UNITY_EDITOR`), para poder probarlo. En el Player solo entra con `SQUASH_DEV`.
- Las variantes de PhysX solo se prueban si un criterio de gate falla, y como máximo dos se llevan al iPhone.

### 4.4 Preparativos del PO (no bloquean la aprobación del plan; sí el inicio de E0 y E1)

1. Iniciar sesión en Unity Hub, activar la licencia Personal e instalar **6000.3 LTS (Apple silicon) con iOS Build Support** desde Hub (T0.1).
2. Conectar el iPhone 16 Pro, confiar en el Mac y activar el **Modo de desarrollador** (T0.2).
3. Agregar el Apple ID en **Xcode → Settings → Accounts** y entregar el **Team ID** (T0.2).
4. Confirmar el bundle id `cl.rafaallendes.squashclash` (T0.2).
5. Autorizar en su momento la creación del repo privado en GitHub (T1.4).

---

## 5. Fuente WSF (respuesta a la observación de Codex)

Codex no pudo abrir la fuente WSF (error 403) y pidió el enlace y la edición. Usé estas dos, ninguna alojada en worldsquash.org:
- **WSF Specifications for Squash Courts:** https://polskisquash.pl/wp-content/uploads/2018/04/170118-court-specifications-small.1485261194.pdf (copia en la web de la federación polaca; el nombre del archivo indica la edición del 18-01-2017). Cláusulas 4.01 y 5.01–5.05; la 5.01.04 es la del borde inferior.
- **World Squash Rules of Singles Squash 2025** (en vigor desde el 1-09-2025): https://squash.nl/media/ot0ezdzh/250901_world-squash-rules-of-singles-squash-2025.pdf (copia en la web de la federación neerlandesa). Reglas 5.7, 5.8, 5.9 y 6.2, y apéndice 1.

Codex contrastó por su lado las reglas 5.7–6.2 con World Squash Officiating y coinciden. Si el PO quiere una certificación desde el sitio oficial de la WSF, es una verificación opcional que no bloquea First Ball: la geometría del plan coincide con la v0.3 y su enmienda E.0.
