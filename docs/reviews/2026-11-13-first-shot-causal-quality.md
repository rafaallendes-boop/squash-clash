# First Shot T3.1 — calidad causal no direccional

## Autorización literal del PO

Decisión literal del PO del 12 de noviembre de 2026:

> D3: Aprobada la implementación de la capa causal no direccional. Prefiero completar H3b antes de cerrar First Shot, manteniéndola como una entrega independiente.

Encargo recibido el 13 de noviembre de 2026: implementar exclusivamente Tarea 3.1 del plan First Shot r2, con ruta causal compartida humano/rival, TDD, H3b según el método predeclarado de #45, regeneración de RallySimTests, suites completas, AutoQA PASS / 0 / nivel 1 y check-release dev/prod. Primer commit: este registro literal, antes de código.

Base local limpia **11ac6da**, main. Verificados como ancestros los merges de #46 (**2fbb5be72041b59a25231815270d173d2673ebc5**), #47 (**9af2b0c8bd92fd5d27f9b2486ea7b626da903874**) y T4.2/#50 (**8e53b41ce3119c03951cce3aefe5fa55a1be4f5f**); GitHub confirma las tres PR fusionadas. #51 también integrada. Rama propia `codex/firstshot-causal-quality`, worktree separado.

Sin efectos direccionales ni capa aleatoria, overlay 3.3, cambios de RallyParams/spec/criterios/umbrales/ventanas/M2-4/pullFullMm/#32, iPhone, instalación o sesión física. Mantener los 41 heredados, sus proyecciones, SimulationKey y bytes de P0-calib-1. Hashes nuevos calculados en Unity y anclajes en commit propio, después de implementar las claves. Ante incompatibilidad de SameGeometryDifferentDelta_SameShot (§17.A.12 literal), elevar sin acotar. Ante otro defecto o sorpresa, registrar y detenerse. PR para revisión independiente de Claude coordinada por el PO, sin fusionar; no acepta First Shot.

Este registro no acredita implementación, pruebas, experimentos ni aceptación.

## CQ-01 — incompatibilidad elevada antes de implementar

**Entrega detenida, no apta para fusión.** En la lectura inicial se identificó una incompatibilidad entre la exclusión del vector de timing por la categoría Perfect que expresa `Perfect_HasNoTimingVector_ButKeepsStretchAndJam`, la interfaz `Evaluate(..., bool perfect, ...)` y la invariante literal §17.A.12: **«con la misma geometría del contacto, Δ no cambia el resultado»**. La tarea exige elevar esa incompatibilidad sin acotar la prueba; se hace aquí antes de crear esqueletos o implementar comportamiento.

### Contraejemplo por lectura del contrato vigente

[ContactContract.Decide](../../Assets/SquashClash/Core/Shot/ContactContract.cs) clasifica Perfect por la ventana real ±A. Un Perfect anterior a T* se programa en T*, pero un **Perfect posterior a T*** se cuantiza al paso legal más cercano a la suelta, igual que un Valid. La rama `scheduled=perfect&&corrected<=target` lo distingue expresamente. [Las pruebas existentes](../../Assets/SquashClash/Tests/EditMode/Shot/ContactContractTests.cs) fijan ambos comportamientos en `Case1_WithinA_AfterTStar_IsPerfectAtR`, `DeltaExactlyA_IsPerfect` y `ContactStep_IsNearestStepToR_AndSignedErrorIsReported`.

Con paso de 120 Hz, tempo 1, compensación cero y A=40 ms, considérese un plan visible con T* en un paso entero y K que contiene legalmente ambos instantes y el paso cuantizado:

| Suelta corregida | Δ | Categoría | Programado en T* | Paso de contacto relativo a T* |
|---|---:|---|---|---:|
| T* + 0,039 s | +39 ms | Perfect | No | round(4,68) = +5 |
| T* + 0,041 s | +41 ms | Valid | No | round(4,92) = +5 |

Ambos pasan también V=100 ms. Usan exactamente **el mismo estado de pelota en T*+5 pasos**, cuerpo, lado, alcance e intención (P y v). No es necesario que las sueltas tengan el mismo timestamp para que el contacto cuantizado tenga la misma geometría. Se puede situar ese estado legal a una distancia geométrica no nula de la referencia ideal, dentro de R, sin E3/E5; la clasificación Perfect no impone distancia cero a esa referencia.

Si `perfect=true` elimina el vector de timing y `perfect=false` permite el vector geométrico no nulo, sus entradas al solver difieren a pesar de esa geometría idéntica. Con cualquier magnitud positiva d dentro de los topes, el segundo recibe el efecto configurado (+0,6·d de altura y −0,25·d de fracción de velocidad), mientras el primero no lo recibe. **Eso introduce una dependencia de Δ a través de la categoría**, aunque la función no acepte Δ directamente. E3/E5 iguales en ambos contactos no eliminan la diferencia. El contraejemplo no requiere azar, P3 ni un rival con reglas distintas.

Es un **contraejemplo estático a la combinación de requisitos**, no una corrida Unity, un RED observado ni una regresión de producto: CausalQuality todavía no existe. La fórmula geométrica concreta aún no se ha elegido. El problema se presenta para cualquier geometría en que dicha fórmula produzca un vector no nulo y el contrato admita esas dos categorías sobre el mismo contacto. No se declara que el contrato actual esté defectuoso ni se cambia su cuantización, asistencia o clasificación.

### Decisión necesaria del PO

Reconciliar la exigencia de «Perfect sin vector de timing» con la independencia literal de Δ en **todas** las geometrías iguales, incluida la pareja Perfect/Valid. Una implementación puramente geométrica puede conservar §17.A.12 si no suprime un vector no nulo por la categoría temporal; eso implica aclarar el significado del requisito/prueba Perfect. No se adopta esa interpretación ni se cambia spec, plan o criterio por inferencia. Tampoco se restringe `SameGeometryDifferentDelta_SameShot` a pares con el mismo bool `perfect` ni se reclasifica el contacto para hacer pasar la prueba.

### Estado de la entrega al detenerse

- Primer commit **eb895d8**: decisión literal y alcance, solo este informe.
- Producto, catálogo, DefaultPreset.json, P0-calib-1, RallyParams, spec, planes, criterios y pruebas **sin cambios**. Solo documentación de autorización y parada.
- Anclajes históricos de la base: defaults **c0e9f00176d5**, P0 efectivo **42d9efbf50fc**, proyección defaults **0d39e6016bd8**, proyección P0 **8de3f2ecb217**, SimulationKey **166a96e319c7**; fuente: [verificación T4.2/#50](2026-11-13-first-shot-s4-autoqa-continuation.md). No se presentan como cálculo Unity nuevo. No hay hash posterior ni cambio de claves; el cálculo/anclajes en commit propio quedan pendientes.
- **No ejecutados:** RED/GREEN, suites, AutoQA, check-release, H3b ni RallySimTests. H3b causal sigue **NotEvaluated**; no se cuentan fallos como vectores cero ni se inventan diferencias frente al 11 de noviembre.
- Sin Unity iniciado, ProjectSettings transitorios, exports/builds, iPhone, instalación ni sesión física.
- La entrega de implementación y la PR final lista para Claude quedan **pendientes de la decisión del PO sobre CQ-01**. No se declara completada T3.1, H3b ni First Shot; no se fusiona.

Se conserva la rama/worktree para una continuación autorizada. Se aplica la parada pedida por el PO; no se implementa una solución alternativa dentro de este encargo.

## Continuación B4 / CQ-01 — decisión literal del PO (13 nov 2026)

> Apruebo conceptualmente la opción C: zona muerta geométrica, con validación técnica previa.
> Como PO, quiero priorizar una mecánica consistente, predecible y justa. Dos golpes con idéntica geometría e intención deben producir el mismo resultado, independientemente de su clasificación temporal.
> Condiciones:
> 1. Mantener las ventanas actuales de Perfecto (±40 ms) y Valid, sin modificar el contrato de contacto.
> 2. El vector de timing dependerá exclusivamente de la geometría efectiva del contacto, sin bonificaciones ni penalizaciones adicionales por categoría.
> 3. Definir una zona muerta geométrica donde el vector sea cero, con crecimiento continuo fuera de ella.
> 4. Verificar que todos los contactos Perfecto alcanzables queden cubiertos, considerando diferentes trayectorias, velocidades y estados de contacto.
> 5. Mantener independientes los efectos de posición E3/E5.
> 6. Aceptar que un Valid cercano al límite de Perfecto pueda tener idéntica salida si su geometría es equivalente.
> 7. Actualizar explícitamente §7.1 y §17.A.12 para eliminar la contradicción, sin alterar otros criterios aprobados.
> 8. Implementar TDD para continuidad, determinismo, límites y ausencia de ventajas artificiales por anticipación.
> Antes de modificar código, presentar la definición matemática propuesta, los casos límite y una evaluación del tamaño de la zona muerta. Si resulta demasiado amplia o elimina la penalización que buscamos, detenerse y presentar alternativas.
> Una vez validada la propuesta técnica, podremos autorizar la implementación mediante el flujo habitual de Codex, r

El mensaje del PO llegó cortado al final; se registra tal cual, sin completarlo. Esta continuación autoriza **solo propuesta técnica y experimento explícito** en Tests/Experiments. La implementación sigue detenida y requiere autorización nueva del PO después de la revisión de Claude. Sin producto, claves/defaults, spec/plan/criterios/ventanas/contrato, AutoQA, check-release, iPhone, builds o instalación. Unity solo para el experimento y una instancia a la vez. Método en commit previo a ejecutar; dos corridas idénticas byte a byte con SHA-256. Entrega en esta misma rama, SIN FUSIONAR.

### Cierre verificado de PR #51 y corrección X1

PO: **«Yes, merge #51»**. Git verificado antes de este registro: merge **11ac6da80a8ac04ff727cbf55f1da47396d1c725**, padres **8e53b41ce3119c03951cce3aefe5fa55a1be4f5f** y **e4386525d13815925c94e10288595ec4749e9e6f** (segundo padre exacto). Árbol de merge y segundo padre idéntico: **2665cd7315589daa721196feaf07df4fc2811207**. No se ejecuta ninguna fusión en esta continuación. No acepta First Shot; **R3 sigue Fail 0/42 como observación abierta**.

Corrección de atribución comunicada por el PO: los tiros X1 más limpios señalados por Claude son **273 en B0 y en B3, y 87 en B1 y en B2**. La atribución anterior «273/87, respectivamente B0/B3» interpretó mal una redacción ambigua de Claude. Se corrige en los tres registros, sin recalcular ni alterar arnés, evidencia, testigos o criterios de #51.

## B4 — método predeclarado antes de ejecutar (CQ-ZONE-v1)

Esta sección fija el método sin resultados. Se compara capacidad geométrica, **no** se ejecuta H3b ni se elige una implementación. Los pesos del plan se usan solo en cálculos del arnés: 0,6 m/m, −0,25/m, topes 1,0 m y −0,4 de fracción. No hay claves nuevas.

### Referencia y dos familias propuestas

Sea b(j) la posición efectiva del cuerpo en el paso cuantizado j; q(j)=(x(j)−b_x(j), y(j)−0,9, z(j)−b_z(j)) en metros. Al espejar el lado se espeja también el eje lateral del marco. q*=q(j*) corresponde a P* en T*, **no** a `BodyFrame.IdealContactPoint`: este último es una postura preferida y puede no pertenecer a la trayectoria. Se usa la curva poligonal Γ que une q(j) en orden de pasos; s es su longitud de arco firmada desde j*. La interpolación poligonal define la geometría propuesta entre nodos; los contactos productivos siguen siendo estados P2 cuantizados, no puntos interpolados del arnés.

Se obtiene el conjunto finito J_A de pasos legales que **alguna** suelta Perfect alcanzable del plan visible puede producir con el ClockMap y la compensación vigentes. Se incluyen los vecinos legales y el empate anterior del contrato. Es una imagen de un intervalo completo, no solo soltar a −40/0/+40 ni muestrear frames. Una suelta anticipada Perfect lleva a j*, una tardía puede llevar a pasos posteriores. J_A se calcula una vez para la referencia de recepción y no se modifica según la categoría del intento evaluado.

**Familia trayectoria, Z_T:** L=max{|s(j)|:j∈J_A}; Z_T={Γ(s):−L≤s≤L}, restringida a las partes legales de K existentes. La extensión a ambos signos es deliberada: no añadir una dirección privilegiada al vector. En extremos de K no se inventan contactos al otro lado. Se aplica d_T(q)=distancia euclídea mínima de q al conjunto cerrado Z_T. La imagen incluye todos los q(j) Perfect por construcción si J_A y el cuerpo efectivo están completos. Se usa la posición geométrica, **nunca j−j*, Δ o la categoría como entrada de la magnitud**. Una intersección de la trayectoria conserva la misma distancia y efecto para el mismo q; puede crear zonas sin penalización adicionales que se informan. Usar directamente |j−j*| o |s(j)| en la magnitud sería otra propuesta y no garantiza igualdad cuando una trayectoria vuelve al mismo estado geométrico. Distancia a Z_T es continua y 1-Lipschitz, incluso al cambiar el segmento más cercano; no se necesita escoger una rama temporal.

**Familia radio, Z_R:** esfera cerrada ||q−q*||≤r; d_R=max(0,||q−q*||−r). Se comparan dos maneras de fijar r: (i) radio fijo r_m igual al máximo observado de ||q(j)−q*|| sobre todos los J_A de **esta muestra**, y (ii) radio adaptativo r_A=max_{j∈J_A}||q(j)−q*|| por recepción. El radio fijo solo cubre por medición los casos observados: no garantiza trayectorias/velocidades/cuerpos nuevos. Un radio fijo universal exigiría una cota demostrada del dominio completo; el peor caso puede abarcar prácticamente K. El radio adaptativo, igual que Z_T, sí cubre por construcción si la imagen Perfect se obtiene exhaustivamente; sacrifica sensibilidad en direcciones no recorridas. Comparar solo un radio medido con Z_T no convierte el primero en garantía universal.

Ambas familias requieren una **referencia geométrica congelada**, compartida por humano e IA. Con igual cuerpo, contacto, intención y referencia, no hay entrada de Δ/categoría a la evaluación. La interfaz original del plan no recibe la trayectoria ni J_A: sería necesario acordar un contexto de referencia explícito en la futura implementación, sin esconderlo en un bool `perfect`. Una afirmación de igualdad entre recepciones con referencias distintas requeriría también una definición universal de zona; no se la da por demostrada con una zona adaptativa.

### Crecimiento y efectos independientes

Para cualquiera de las distancias d≥0 propuestas: v_timing=(min(1,0;0,6·d), 0, max(−0,4;−0,25·d), "timing-geometría"). Las comas en 1,0/0,6/−0,4/−0,25 son decimales; min/max reciben dos escalares. En d=0 el vector es cero; crece linealmente al salir, sin salto en el borde; hay quiebre de pendiente, no discontinuidad. El tope de velocidad llega a d=1,6 m y el de altura a d=5/3 m, por componente. Con E3/E5 se conserva cada vector independiente y se aplica la suma con los topes del plan antes del solver. No se implementa ni mide E3/E5 en esta fase; Perfect no los desactiva. No se modifica la intención después del solver.

### Muestra fija, relojes y denominadores

- P2 real y configuración real DefaultPreset.json, hash/proyección/SimulationKey/RallyParams leídos y registrados; bounceThreshold del proyecto. Sin mutaciones de ParamSet ni archivos de configuración. Las seis recetas **SL1–SL6** de PracticeLaunchDefinitions; horizonte 10 s hasta segundo bote, cuerpo fijo de cada receta.
- First Rally: exactamente **8 rallies** consecutivos con BotBrain σ=30 ms/semilla 2026 y Fondo Medio/semilla 1111, primer receptor alternado Human/Rival, RallyParams sin tocar. Un decorador del brain del rival observa, sin alterar decisiones/RNG, los primeros **24 planes con Exists y TStarStep≥0** en orden de aparición (si hay menos se informa el número real, sin rellenar con otros seeds). Se inventarían todas las recepciones y omisiones de la captura, incluidas inalcanzables. Context.Predict genera cada vuelo P2 real; el decorador conserva el body efectivo predicho con `PlayerBody.PositionAfter` desde su estado real y el mismo objetivo/demora del movimiento. No se cambia RallySim ni RallyContact. Es muestra de sus recepciones y trayectorias entrantes; no una nueva campaña de 1000 rallies ni evidencia de capa causal activa.
- Para cada trayectoria: **tempo 0,3 / 0,65 / 1** y compensación **−75 / 0 / +75 ms**, producto cartesiano. ClockMap afín sin pausa/drops, origen real 1 s en el primer paso del vuelo; plan presentado en 0 s. La compensación se añade al timestamp sintético del evento y la resta el contrato una sola vez. La misma curva P2 y cinemática por paso se conserva entre relojes; estas nueve condiciones no son nueve trayectorias físicas distintas. Perfect ±40 y Valid ±100 intactos. Casos con pausa, cambio de tempo durante la recepción o ClockMap inválido se razonan aparte y no se presentan como medidos por esta muestra.
- Imagen exhaustiva del contrato en este dominio afín: particionar las sueltas corregidas por los extremos de K, cada paso/medio paso (vecino/empate), T*, ±40/±100 ms, inicio del ClockMap y eventos de contacto. Evaluar cada borde y un punto interior de cada celda abierta, con `ContactContract.Decide` real; sus salidas son constantes por celda salvo el valor informativo Δ/cuantización. No añadir epsilon que mueva ventanas. Publicar categorías/razones/pasos y las celdas de duración cero por separado. El máximo radio/L sobre todos los Perfect de esta partición define las zonas; no se ajusta después de observar resultados.
- Por zona/condición: imagen Perfect (pasos distintos); alcance mínimo/máximo de sus nodos dentro de K, número de nodos legales dentro/fuera, ancho entre extremos en pasos y ms reales (distinto del número inclusivo de nodos), longitud recorrida y diámetro/radio en metros. **K:** proporción de estados cuantizados legales con vector no cero, no fracción temporal continua. **Valid:** duración real de las celdas con contacto en ±100 ms que quedan fuera, sobre duración con contacto y sobre 200 ms; además separar las alas −100..−40 y +40..+100 y contactos Perfect/Valid/fallos. Los fallos no tienen vector y nunca entran como cero. Bordes aislados se conservan pero no añaden duración.
- Medir también d y el vector en los contactos aceptados de −100/−60/−41/−40/−39/0/+39/+40/+41/+60/+100 ms; los fallos se etiquetan sin vector. No son bots H3b ni un juicio de zona de frontis. Informar si un ala no tiene ningún contacto disponible o ninguna penalización, sin inventar criterio de porcentaje.

### Artefactos, reproducibilidad y decisión

Un único test **Explicit**, categoría Experiments, en `SquashClash.EditMode.Experiments`, invocado por nombre en dos procesos Unity secuenciales, mismo HEAD limpio. Nunca por scripts/test.sh, regresión o AutoQA. Archivos estables UTF-8 sin BOM, números invariantes con formato round-trip, filas ordenadas, sin fecha/hora/duración de cómputo en los datos. Directorios locales distintos run1/run2 bajo Logs/experiments/causal-zone; comparar **todos** los archivos de datos byte a byte y SHA-256. XML/logs propios se preservan aparte: son evidencia de ejecución y no se espera que sean idénticos. Metadata incluye fuente/hash/config, conteos de captura y límites de interpretación. Publicar los datos y manifiesto SHA-256 versionados en docs/validation/evidence/2026-11-13-causal-zone-proposal.

Comprobaciones del arnés: fuente limpia; 6 recetas; P2; defaults 108/c0e9f00176d5 y SimulationKey 166a96e319c7; 8 rallies; imagen Perfect cubierta por Z_T/Z_R adaptativa; cero topes P2 e instancias vivas al final. Su éxito solo acredita la ejecución/integridad de la muestra. Un fallo/sorpresa se documenta sin ajustar producto. Restaurar cualquier ProjectSettings transitorio al terminar y cotejar todos los archivos de producto/configuración con 245e0ac.

**Juicio previsto:** cobertura por construcción sujeta a referencia exhaustiva; cobertura medida de la muestra separada; magnitudes de altura/velocidad y asimetrías de las dos alas informadas, sin nuevo umbral de «visible». Si hay desaparición de la penalización dentro de una ventana con contactos, o tamaño preocupante, presentar alternativas y sus riesgos sin elegir ni implementar. La revisión de Claude/decisión del PO precede toda autorización nueva. El texto propuesto de adenda y el plan TDD se entregan con el análisis final; no se edita la spec.
