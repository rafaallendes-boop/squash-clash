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

## Casos límite propuestos y resultados esperados

| Caso | Resultado esperado (ambas familias, con imagen Perfect exhaustiva) |
|---|---|
| +39/+41 ms, mismo paso +5 | El nodo es miembro de J_A por +39; ambos ven exactamente la misma referencia y distancia cero. +41 conserva la categoría Valid, sin vector de timing. E3/E5 e intención iguales ⇒ misma entrada al solver y misma salida determinista. Si el contacto no es legal no se fabrica un vector: se conserva el fallo del contrato. |
| Perfect anterior a T* | Si la suelta está dentro de K y es alcanzable, el contrato lo programa en T*; q* está en la zona. Soltar antes de K sigue siendo Air, aunque esté dentro de ±40. |
| Perfect posterior a T* | Contacto en el paso más cercano legal/vecino elegido por el contrato; su imagen está incluida. Cero timing sin consultar su categoría al evaluar el efecto. |
| Borde exacto | d=0, vector cero. Un punto que sale una distancia ε tiene altura min(1;0,6ε) y velocidad max(−0,4;−0,25ε). |
| Paso siguiente | No se exige una penalización por el mero hecho de ser el paso siguiente: solo si su geometría está fuera de la zona. Con curva que vuelve a entrar, puede seguir en cero. Si está fuera, el vector lo determina su distancia, no el paso ni Δ. |
| Anticipar/atrasar a igual distancia | Igual distancia **a la zona geométrica** ⇒ mismo vector, sin signo lateral. Igual |Δ| o igual longitud de arco desde P* no implican igual distancia a un conjunto curvo. Z_T simetriza la extensión por arco; Z_R es radial. No se promete una igualdad temporal que la geometría/contrato no dan. |
| Tempo≠1 | El ClockMap cambia qué nodos puede producir ±40; preparar J_A con el mapa vigente antes de evaluar, sin escalar el vector por tempo. Si se quiere que la referencia tampoco dependa del reloj, hay que construir una envolvente sobre el dominio de relojes aprobado y medir su tamaño. |
| Compensación≠0 | Con evento sintético r=r_corregido+c, el contrato resta c una sola vez. La geometría alcanzable debe permanecer equivalente cuando el plan visible y el mapa son iguales; los bordes IEEE se conservan, sin desplazar ventanas mediante epsilon. |
| P*/T* junto a KIn/KOut | Recortar a contactos legales reales. El vecino legal puede ser el mismo para varias sueltas. No extender K, no crear espejo fuera de K, no contar fallos como vector cero. Una referencia ideal junto a un extremo puede dejar una sola ala con contactos. |
| Rival / RallyContact.Solve | La futura llamada debe recibir el mismo contexto geométrico y el PlayerBody efectivo al contacto, mismo R y mismos parámetros que ShotController. La ruta actual no recibe esa referencia: no basta con envolver únicamente ShotController. Preparación, evaluación, Apply y registro por causa deben ser compartidos. El experimento solo observa esa ruta, no la modifica ni acredita que ya tenga causalidad. |
| Cambio de plan, tempo o mapa durante la recepción | La prueba de cobertura necesita el conjunto de todos los planes/referencias efectivamente usados. No extrapolar la prueba afín a ClockMap discontinuo. Mantener trazabilidad de versión; no cambiar la zona por la clasificación del swing. Pausa/drops conservan NotEvaluable cuando lo diga el contrato; esta fase no mide ese caso. |
| E3/E5 | Evaluarlos sobre el cuerpo efectivo sin restar la zona de timing al alcance ni a la distancia al cuerpo. Un timing cero puede coexistir con estirado/atorado. Perfect no los anula. |

### Límites de la garantía

El hecho de que una función no reciba Δ no basta: también hay que fijar su referencia antes del intento. Una zona calculada de nuevo en función de la categoría reproduciría CQ-01 de forma indirecta. Además, dos referencias adaptativas distintas pueden producir distinto vector para el mismo q y P*: su igualdad entre recepciones requiere identidad de la referencia completa, o acordar una definición universal. La invariancia literal no puede reducirse de forma tácita a «misma categoría». Se eleva explícitamente esta cuestión de contexto a la revisión.

La garantía de Z_T/r_A es una inclusión matemática J_A⊂Z para **cada referencia correctamente construida**. La medición cubre solo las trayectorias y mapas afines publicados. No demuestra cobertura exhaustiva de todos los juegos, futuras trayectorias, cambios de reloj, planes visibles o estados corporales. Para una futura implementación, esa inclusión tiene que demostrarse para el algoritmo de preparación del contexto y verificarse con tests; no se reemplaza con el máximo de seis recetas.

## Adenda aditiva propuesta — sin editar la spec

**Nota de estado — propuesta histórica SUSTITUIDA por D36 (PO, 13 nov 2026).** La decisión literal registrada [más abajo](#b4-cq01-documentary-d36) y en [aprobaciones](2026-10-25-first-shot-po-approvals.md#b4-cq01-po-decision-20261113) aprueba Z_T preparada una vez por recepción y define expresamente §17.A.12 dentro del mismo contexto congelado. El contexto incorpora trayectoria visible, `pace.tempo`, ClockMap y compensación vigentes; esta última precisión procede de Claude, separada del literal. **Queda sustituido, en particular, el párrafo de la propuesta que rechazaba «solo dentro del mismo contexto»**, así como la selección pendiente entre alternativas; no es una restricción inferida por Codex. Se conserva a continuación **todo el texto original como histórico**, incluidos su alcance intercontextos y TDD anterior, sin convertirlo en requisito vigente. D36 de la [spec](../superpowers/specs/2026-10-20-squash-clash-design-v0.3.md) y [D6 del plan](../superpowers/plans/2026-10-25-squash-clash-first-shot.md#adenda-d6-cq01) rigen esta cuestión desde la decisión documental. Los datos CQ-ZONE-v1 no cambian ni se recalculan.

Texto para añadir después del caso 1 de §7.1, sujeto a aprobación técnica y del PO:

> **Aclaración de la calidad causal del caso 1 (Perfecto).** Se conserva íntegro el contrato de contacto: ventana Perfecto de ±40 ms reales corregidos, ventana Valid y reglas de K, programación y cuantización vigentes. Perfecto clasifica la ejecución temporal y no concede una bonificación de trayectoria por categoría. El vector de timing se calcula exclusivamente a partir de la geometría efectiva del contacto cuantizado respecto del cuerpo y de una referencia geométrica preparada antes del intento. Una zona muerta geométrica cubre todos los contactos Perfecto alcanzables de esa referencia; dentro de ella el vector de timing es cero y fuera crece continuamente desde cero. Un Valid con geometría equivalente usa el mismo vector, que también puede ser cero. Los efectos de posición E3/E5 se evalúan de forma independiente y no se anulan por ser Perfecto. Esta aclaración no cambia ventanas, alcance, criterios ni efectos direccionales.

Texto para añadir a §17.A.12, sujeto a la misma aprobación:

> **Aclaración de la invariante 12.** Se conserva íntegramente la exigencia: con la misma geometría del contacto, Δ no cambia el resultado. La igualdad del golpe no se restringe a contactos con la misma clasificación temporal. Con idéntica geometría efectiva de pelota y cuerpo respecto de la referencia ideal e idéntica intención, la función causal y la entrada al solver son idénticas para cualquier Δ admitido, incluido un par Perfecto/Valid que se cuantiza al mismo contacto (+39/+41 ms a 120 Hz cuando ambos son legales). La categoría Perfecto/Valid y Δ no son entradas de la magnitud causal. La zona muerta es una construcción geométrica determinista preparada antes del intento y no se altera para favorecer su categoría. Si dos preparaciones internas representan la misma geometría e intención, deberán producir el mismo efecto: no se considera satisfecha esta invariante solo por declarar que sus identificadores o contextos son distintos. La cobertura de todos los Perfecto alcanzables se verifica sin modificar el contrato; los fallos de contacto se registran como fallos y no como vectores cero. E3/E5 mantienen su independencia. SameGeometryDifferentDelta_SameShot incluye categorías distintas y no se acota para excluir el contraejemplo.

La redacción **no selecciona una zona ni declara resuelta la invariancia entre recepciones o relojes**. La preparación debe cumplir la invariante literal; no se introduce la condición adicional «solo dentro del mismo contexto» para hacerla pasar. Las alternativas adaptativas de cobertura por recepción requieren resolver ese punto antes de poder implementarse. No se edita §7.1/§17.A.12 ni ningún otro criterio en esta fase. La forma concreta y el tamaño de la zona quedan sujetos al juicio sobre los datos.

## Plan TDD para una futura implementación autorizada

1. Esqueletos compilables y RED por aserción antes de cada GREEN. Prueba literal SameGeometryDifferentDelta_SameShot: mismo contacto, cuerpo e intención respecto de la referencia ideal; variar Δ en ambos signos y categorías, incluyendo +39/+41 y vecinos legales; comparar cada vector, entrada y salida del solver sin filtros por categoría. Probar también geometrías iguales en recepciones distintas y entre tempos sin descartarlas porque cambie el contexto interno; el algoritmo de preparación debe preservar la invariante.
2. Construcción de referencia: exhaustividad de la imagen Perfect, bordes ±40/±100 inclusivos, empates y vecino legal, ambos extremos de K, planes visibles por timestamp, tempo 0,3–1, offset aplicado una vez y mapas con cambio de tramo. Cero contactos fuera de K y fallo conservado. Ningún bool Perfect entra a la magnitud causal ni permite reconfigurar la zona por intento.
3. Continuidad: borde exacto, aproximaciones por ambos lados, cambio del segmento más próximo, reentrada/intersección, d=0 y saturación por componentes. A d=1,6 m se satura velocidad; a d=5/3 altura; no clamp conjunto que cambie dirección ni un salto por paso/categoría.
4. Determinismo: misma referencia/entrada produce vectores y solución reproducibles; registrar referencia/versión y cada causa por separado. Variar Δ sin variar geometría; ningún RNG en la capa.
5. Simetría: espejar cuerpo, contacto, ideal y zona; Δlateral=0 siempre. Igual distancia a la zona a ambos lados ⇒ igual vector. Comparar anticipar/atrasar sin usar Δ/categoría como descuento ni crear una zona favorable solo delante de T*. Informar las asimetrías que procedan de K/contrato/curva, sin introducir compensación temporal para ocultarlas.
6. E3/E5: mantenerlos con timing cero, incluidas todas las variantes de Perfect; probar mismo R y geometrías estirada/atorada en humano/rival. No modificar RallyParams, cinemática ni efectos direccionales. E7 sin vector cuando tampoco existen causas posicionales.
7. Integración antes del solver: una función compartida de preparación/evaluación/Apply por ShotController y RallyContact.Solve, con cuerpo efectivo en el contacto, mismo alcance y valores. Aserciones de igualdad humano/rival y solver llamado con P/v ya modificados; salida intacta tras resolver. Trazas de ambos con vectores por causa, incluida rally-point.
8. Tras implementar y calcular hashes reales en Unity: commit propio de anclajes; suites completas, AutoQA PASS/0/nivel1 sin cambiar expectativas y check-release dev/prod limpios, un Unity a la vez, transitorios restaurados. H3b completo con método predeclarado y RallySimTests regenerados pertenecen a la fase de implementación, no a esta propuesta. No se ejecutan en esta fase.

## Resultado B4 — propuesta medida, implementación aún detenida

Método **e8a26c1**, arnés **ad3d25977cab6c912085ca281dc47a0cd6c6f5aa**, fuente limpia en ambas corridas. El arnés no cambió entre ellas. Datos originales idénticos byte a byte en **8/8 archivos**, preservados en [paquete de evidencia](../validation/evidence/2026-11-13-causal-zone-proposal/README.md), con [SHA-256 y metadatos de ejecución](../validation/evidence/2026-11-13-causal-zone-proposal/manifest.json). [Tablas por receta/muestra](../validation/evidence/2026-11-13-causal-zone-proposal/tables.md) y [810 filas completas por reloj/forma](../validation/evidence/2026-11-13-causal-zone-proposal/zones.tsv). El script analyze.py incluido reproduce las tablas sin Unity y sin cambiar datos.

| Corrida explícita | Hora America/Santiago del 13 nov | Resultado del arnés | Código |
|---|---|---|---:|
| run1 | 09:06:31–09:06:36 | 1/1 completada; 0 failed/skipped/CS | 0 |
| run2 | 09:08:00–09:08:03 | 1/1 completada; 0 failed/skipped/CS | 0 |

Esas horas corresponden al método de experimento; la primera importación del proyecto en el worktree ocurrió antes y no es otra corrida. Una sola instancia Unity a la vez, comprobada antes de cada proceso y sin instancias restantes al terminar. Son ejecuciones **Explicit/Experiments** fuera de regresión/AutoQA: no se presentan como suites GREEN, RED→GREEN, hipótesis aprobada ni aceptación de la mecánica.

### Muestra y cobertura observada

**30 trayectorias:** 6 recetas SL y 24 recepciones del rival tomadas según el orden predeclarado en 8 rallies. El decorador vio 97 recepciones; 0 sin plan utilizable, 73 posteriores al límite de captura. Se conservan todas las entradas en capture.tsv y los 8 resultados de procedencia en rally-sample.tsv; no se regenera la campaña histórica de 1000 ni se le atribuyen resultados nuevos. Los 24 planes muestreados son alcanzables. Los nodos legales medidos tienen alturas **0,052296–0,396205 m** en SL y **0,119734–0,360200 m** en el rival; rapidez P2 **1,630361–7,764919 m/s** y **6,035832–6,775859 m/s**, respectivamente. La muestra varía velocidad/curva/cuerpo, pero **no cubre todos los estados altos/velocidades/alcances posibles**, ni una población independiente de rallies; las primeras 24 recepciones están correlacionadas. No hay volea.

Las 9 condiciones por trayectoria dieron **270 imágenes Perfect**, todas contenidas en Z_T y r_A por construcción del arnés. Son 810 evaluaciones de zona, 21.318 observaciones/celdas del contrato, 8.910 sondas con forma, 5.576 estados y 90 eventos P2. La imagen se obtiene de todos los bordes/celdas del dominio afín declarado, no de tres ejemplos temporales. Las distancias se evalúan sin rama por categoría. En las **252 condiciones** donde +39 y +41 ms tienen contacto, ambos usan el mismo paso y vector cero; en las otras 18 el par no tiene dos contactos legales, sin sustituir los fallos por cero.

Para cada recepción bien preparada, **sí** se cubren todos los Perfect alcanzables de su referencia: se incluye la imagen completa de sus pasos efectivos. Eso es una inclusión matemática de la construcción, no una extrapolación estadística. **No** está demostrada una solución global de la condición 4 combinada con la invariante literal para todo reloj, trayectoria, plan visible y estado corporal del juego: los mapas aquí son afines y la referencia adaptativa cambia con tempo. El radio fijo solo tiene cobertura medida en esta muestra. El máximo medido no es prueba de todos los casos futuros. La preparación productiva exhaustiva y la invariancia entre referencias todavía requieren validación técnica.

### Tamaño efectivo de la zona y espacio para el vector

A tempo 1 / compensación 0, Z_T y radio adaptativo tienen los mismos conteos de nodos cero en esta muestra (distintas magnitudes fuera de la zona):

| Trayectoria | Imagen Perfect | L de trayectoria / radio adaptativo | Anchura cero sobre nodos: pasos / ms / arco m | K con vector ≠0 | Sueltas en ±100 con contacto y vector ≠0 / ms con contacto |
|---|---:|---:|---|---:|---:|
| SL1/SL2 (cada una) | 1 nodo | 0 / 0 m | 0 / 0 / 0 | 20/21 | 60,000/103,956 ms |
| SL3/SL4 (cada una) | 6 nodos | 0,316161 / 0,316125 m | 5 / 41,667 / 0,316161 | 11/17 | 54,167/108,037 ms |
| SL5/SL6 (cada una) | 6 nodos | 0,068526 / 0,068358 m | 9 / 75,000 / 0,123295 | 28/38 | 114,167/200,000 ms |
| FR01–FR24 (rango, no media) | 6 nodos | 0,251658–0,268781 / 0,251612–0,268738 m | 9–10 / 75–83,333 / 0,452966–0,532570 | 67,65–74,36 % | 101,164–114,167 / 192,831–200 ms |

Ancho = último−primer nodo cero; **no** número inclusivo de nodos, ni anchura temporal de la ventana Perfect. En particular, SL1/SL2 tienen un nodo cero y ancho entre extremos cero, pero muchas sueltas Perfect se programan en ese mismo nodo. Los extremos poligonales de Z_T pueden estar entre estados; el tamaño tabulado es la huella efectiva en contactos cuantizados. El TSV también informa diámetro espacial de los nodos cero y el radio de cada esfera; no se equiparan arco y diámetro. Las fracciones de K usan nodos, las de Valid usan duración con contacto. En SL1/SL2 no hay ala tardía con contacto, y en SL3/SL4 no hay ala anticipada con contacto: **no se atribuye a la zona lo que es un fallo de K**.

Las tablas completas muestran las seis SL en los tres tempos y cada rival a tempo 1; el TSV conserva además, por cada una de las 24 muestras del rival, los otros tempos y las tres compensaciones. Las compensaciones −75/0/+75 no producen diferencias sustantivas de tamaño/conteos/duración (residuos double preservados; no se usa tolerancia en d>0). Contacto+fallo particiona los 200 ms con residuo máximo **7,39e−13 ms**, sin corregir categorías ni ventanas.

**Magnitud que queda, sin declarar visibilidad:** en Z_T, a tempo 1 los máximos de altura/pérdida de v entre contactos de ±100 son **33,86 cm/14,11 %** en SL1/SL2, **26,76 cm/11,15 %** en SL3/SL4, **6,24 cm/2,60 %** en SL5/SL6 y **21,28–22,72 cm/8,86–9,47 %** en las muestras del rival. A tempo 0,3 el máximo en SL5/SL6 baja a **2,47 cm/1,03 %**. Son magnitudes del vector propuesto, **no** impacto visual, acierto de zona ni salida del solver. La perceptibilidad queda **NotEvaluated**: no hay umbral aprobado ni prueba humana en esta fase.

Cerca del borde no puede exigirse una pérdida grande conservando continuidad y geometría equivalente. SL3 a +39/+41 usa paso 153 y cero en ambos; a +60 recibe +7,618 cm/−3,174 %; a +100, +26,764 cm/−11,152 %. SL5 a −41 tiene solo **+0,125 mm/−0,00521 %**, a +41 sigue cero y a +60, **+1,697 cm/−0,707 %**. No se convierten esos valores en un criterio nuevo de «visible». Un paso siguiente puede seguir dentro o volver a entrar en la zona; no se penaliza por índice.

En SL1, pasar de −41 a −40 ms cambia el contacto por el **snap anterior a T*** del contrato: el vector propuesto pasa de +13,919 cm a cero porque cambia la geometría efectiva. La función geométrica es continua, pero la aplicación completa en función del timestamp puede conservar un salto por el contrato vigente. Esta fase no lo corrige, no lo oculta ni promete continuidad temporal de la suelta.

### Juicio: radio común demasiado amplio y adaptación temporal pendiente

El radio fijo mínimo que cubre las imágenes Perfect de **esta muestra** es **0,31612509229966818 m**, fijado por SL3/SL4 a tempo 1. Su diámetro es 0,632250 m; su intersección con la K de SL5/SL6 ocupa **38/38 nodos**, ancho 37 pasos/308,333 ms/arco 0,567490 m a tempo 1. **Elimina toda penalización de timing en Valid para SL5/SL6 en los tres tempos.** A tempo 0,3 elimina todo vector en Valid en las **30/30 trayectorias**, aunque existan contactos Valid. En total, con compensación 0 son **34/90 condiciones** con contactos Valid pero sin vector; las otras compensaciones reproducen ese resultado. Se verifica la condición de parada pedida por el PO para esta forma: **demasiado amplia para conservar la penalización buscada en estas condiciones**. No se adopta ni se reduce su radio para ocultar el problema de cobertura.

Las dos formas adaptativas sí dejan vector en parte de Valid en **90/90 condiciones** con compensación 0 (también las otras compensaciones); ninguna ala con contactos pierde completamente su vector. Sin embargo, **no se validan por eso como solución de la invariante global**: al preparar la zona con el tempo, cambian el efecto de un mismo contacto físico. Contraejemplo observado en cells.tsv, SL3, cuerpo e intención geométrica idénticos y mismo estado P2 en el paso **151** (T*=148):

| Reloj | Suelta interior observada | Categoría | d_T | Vector propuesto |
|---|---:|---|---:|---|
| tempo 1 | +22,916667 ms | Perfect | 0 | cero |
| tempo 0,3 | +76,388889 ms | Valid | 0,126390479 m | +0,075834287 m de altura; −0,031597620 de v |

La diferencia sale de la **zona preparada**, no de una rama `if Perfect`; por tanto, excluir Δ del argumento no basta para demostrar «solo geometría». El radio adaptativo reproduce la misma limitación (d=0 frente a 0,126385001 m). No se soluciona estrechando SameGeometryDifferentDelta_SameShot a un mismo reloj o contexto interno. Las familias se presentan como construcciones de cobertura para evaluación, **no como implementaciones ya compatibles con todo el literal**. Tampoco el radio fijo es garantía universal, aunque en este ejemplo particular sea igual.

**Respuesta explícita al juicio solicitado:** cobertura de Perfect por recepción, sí por construcción; cobertura universal e invariancia global combinadas, aún no validadas. Penalización numérica fuera del borde, sí en las zonas adaptativas de la muestra; penalización perceptiblemente visible, no evaluada. El radio común medido sí elimina la penalización buscada en condiciones importantes. **Se detiene la propuesta sin elegir implementación** y se presentan alternativas para revisión/PO.

### Alternativas para revisión, sin elegir ni ejecutar

| Alternativa | Ventaja | Riesgo / validación pendiente |
|---|---|---|
| Zona geométrica por trayectoria con envolvente común de todos los relojes admitidos, preparada sin depender del reloj del intento | Puede cubrir todo Perfect sin que cambiar tempo cambie el vector para el mismo estado y referencia física; conserva forma de la curva en lugar de inflar todas las direcciones con una esfera global | Hay que demostrar la envolvente para el dominio completo de ClockMap/planes/cuerpo. Puede eliminar timing también en Valid lento: una geometría que es Perfect con otro reloj debe seguir en cero por la invariante. Requiere otra predeclaración técnica antes de medirla; no se mide aquí como si fuera Z_T adaptativa. |
| Radio común **por referencia física de trayectoria**, acotado sobre todo el dominio de relojes, sin adaptación al intento | Función radial simple, continua y fácil de espejar; evita inflación por otras trayectorias si la identidad de referencia resulta legítima bajo el literal | Igual problema de Valid lento, y puede inflar direcciones no recorridas. Hay que verificar equivalencia entre referencias que representan la misma geometría; no basta un id distinto. No se ha calculado/validado como alternativa en esta fase. |
| Zona geométrica universal/cota física común, conservando que ciertos Valid equivalentes no tengan vector de timing | Regla uniforme entre recepciones y categorías; no oculta ventaja del rival ni introduce categoría en la magnitud | El radio fijo medido ya muestra desaparición del efecto; una cota universal puede ser mayor. El PO tendría que aceptar explícitamente esa ausencia de penalización en esas condiciones. E3/E5 seguirían independientes, pero no se los usa para fingir una penalización de timing ni se cambian sus pesos. |

No se recomienda una alternativa ni se autorizan cambios de ventanas, cuantización, física, solver, RallyParams, valores causales o criterios para recuperar artificialmente una desigualdad. No se ejecuta un método adicional después de observar estos resultados. La adenda propuesta y el TDD son materiales para revisar; **no son una autorización para implementarlos**.

### Integridad y estado final de la fase

**706 archivos protegidos** antes/después idénticos por SHA-256, incluidos producto, scripts, Packages y todos los ProjectSettings. No hubo transitorios versionados que restaurar. DefaultPreset.json SHA-256 **68a17d60090583fc87ff24a79c8e8141f2131ec7c53e8ca111ce5e8497981319**, defaults **c0e9f00176d5/108**, P0 efectivo derivado **42d9efbf50fc** y SimulationKey **166a96e319c7**, intactos; valores/proyección heredados no se modifican y no se accede ni reescribe el archivo P0-calib-1 del dispositivo. RallyParams actual **7d50ddaf5d4f**, sin cambios de bytes; no se confunde con el hash histórico previo a su configuración final de First Rally. Cero topes P2 e instancias vivas al terminar. Device.env copiado byte a byte desde el checkout principal, ignorado/sin mostrar valores ni versionar, cotejado al finalizar.

Solo documentación/evidencia y el arnés explícito nuevo bajo Tests/Experiments. Spec, planes, criterios, producto, catálogo, defaults, ventanas/contrato, M2-4, pullFullMm, #32 y evidencia original de #51 intactos. No se ejecutan AutoQA, check-release, regresiones, H3b, RallySimTests, builds, iPhone o instalación; no corresponden a esta fase sin cambios productivos. No se declara RED→GREEN ni H3b completo. T3.1 no está implementada; First Shot no está aceptado.

Entrega de **propuesta técnica B4** lista para revisión independiente de Claude coordinada por el PO, en la misma rama, **SIN FUSIONAR**. La parada histórica CQ-01 se conserva; esta fase solo valida/expone opciones y riesgos. Una implementación o un nuevo experimento para resolverlos requiere autorización nueva del PO después de esa revisión. **Detenerse al entregar.**


## B4 / CQ-01 — decisión literal del PO y registro documental (13 nov 2026)

Fuente: respuesta literal del PO recibida en la continuación documental de PR #52, desde head `ac3f79b`. Sustituye la fase de selección pendiente de la propuesta; sus textos y datos se conservan como históricos. Este registro es el primer commit de la continuación, anterior a las adendas de spec/plan.

> Apruebo como decisión de PO la zona muerta geométrica por trayectoria, calculada una vez por recepción, con las siguientes precisiones:
>
> 1. La zona debe calcularse utilizando la trayectoria y el `pace.tempo` congelados de esa recepción.
> 2. §17.A.12 se interpretará como un invariante dentro del mismo contexto de recepción: misma geometría, misma intención y mismo contexto deben producir el mismo resultado, independientemente del instante de liberación.
> 3. La clasificación Perfecto/Valid no puede intervenir directamente en el cálculo del vector.
> 4. Se mantiene la continuidad de la penalización fuera de la zona muerta.
> 5. Se descarta el radio fijo y la envolvente universal entre tempos por los resultados experimentales.
> 6. Se debe medir y documentar específicamente el salto anticipado de −41 a −40 ms, sin modificar todavía el contrato de contacto.
> 7. Las magnitudes experimentales se mantienen provisionales hasta validarlas en iPhone.
> 8. Exigir pruebas automatizadas de determinismo, bordes, diferentes tempos, trayectorias y ausencia de dependencias de la categoría temporal.
>
> Solicito actualizar la especificación y su adenda para reflejar esta decisión sin contradicciones.
> Después, preparar el encargo de implementación para Codex mediante TDD y una PR independiente.
> No realizar merge automático. La implementación debe pasar revisión técnica y una prueba jugable antes del cierre de First Shot.
> El objetivo es cerrar esta mecánica con suficiente calidad para el MVP, sin iniciar otra ronda extensa de experimentación salvo que aparezca un defecto relevante.

**Precisión de Claude, separada del literal del PO:** el contexto congelado incluye el ClockMap y la compensación perceptiva vigentes; la compensación es técnicamente necesaria para obtener J_A y no cambia dentro de la recepción. No es una dimensión nueva de decisión. Esta precisión no se atribuye al texto del PO ni se inserta dentro de su cita.

**Significado operativo:** se aprueba Z_T, familia «trayectoria» de CQ-ZONE-v1, preparada una vez por recepción antes del intento con trayectoria visible, `pace.tempo`, ClockMap y compensación congelados. §17.A.12 rige dentro de ese mismo contexto; se descartan radio fijo y envolvente universal entre tempos. Categoría y Δ no entran en la magnitud. La diferencia entre tempos se documenta como efecto del ajuste de ritmo, no como incumplimiento de esa invariante contextual. E3/E5 siguen independientes y el contrato no cambia. Magnitudes [EXPERIMENTO] hasta iPhone.

**Alcance de esta continuación:** solo documentación, en esta misma rama y PR #52. Primero este registro en AGENTS.md, aprobaciones e informe; después D36 aditiva en spec, D6 aditiva en plan y nota de sustitución del texto propuesto del informe. Preparar el encargo futuro de implementación con TDD y PR independiente, sin ejecutarlo ahora. Sin código, tests, defaults, presets, Unity, AutoQA, check-release, iPhone ni builds. Revisión de Claude coordinada por el PO; SIN FUSIONAR, sin merge automático ni aceptación de First Shot. La revisión técnica y prueba jugable siguen pendientes antes del cierre.

**Ruta aclarada por el PO:** la referencia `docs/reviews/2025-10-25-first-shot-po-approvals.md` tenía un error de año; se usa el archivo existente `docs/reviews/2026-10-25-first-shot-po-approvals.md`. No se crea un registro paralelo de 2025.

<a id="b4-cq01-documentary-d36"></a>

## D36 / D6 — estado documental vigente tras la decisión del PO

Primer commit de esta continuación **809ac47**: solo el registro literal completo en aprobaciones, AGENTS.md e informe, cotejado tras quitar las marcas de cita Markdown. La precisión de Claude sobre el ClockMap y la compensación está **fuera de la cita**, no se atribuye al PO. Ruta de aprobaciones corregida a **2026-10-25** según aclaración expresa recibida; no se creó un archivo de 2025.

La [spec v0.3](../superpowers/specs/2026-10-20-squash-clash-design-v0.3.md) incorpora **D36**: fila nueva en §0.3, aclaración después de la tabla de §7.1.3, viñeta de Z_T después de «Orden en el solver» en §7.6.2, aclaración a continuación de §17.A.12 y entrada en Anexo E. Son inserciones; el texto previo se conserva. **Ninguna otra decisión aprobada cambia.** [D6 del plan First Shot r2](../superpowers/plans/2026-10-25-squash-clash-first-shot.md#adenda-d6-cq01) actualiza la interfaz de T3.1, sus pruebas y la ruta compartida; no se reutiliza D5 ni se reescribe la tarea histórica.

**Decisión vigente:** Z_T se prepara **una sola vez por recepción antes del intento**, con trayectoria visible, `pace.tempo`, ClockMap y compensación congelados. §17.A.12 es una invariante **dentro de ese mismo contexto** y para todas las categorías temporales. `Evaluate` no recibe `bool perfect`; recibe la referencia inmutable, incluidos J_A y Z_T. Se descartan el radio fijo y la envolvente universal entre tempos. La diferencia del mismo estado entre tempos registrada en la fase anterior **se conserva como dato**, pero D36 la define como efecto del ajuste de ritmo, no como violación de la invariante contextual. No se modifica una medición para hacerla pasar.

Las magnitudes siguen **[EXPERIMENTO] hasta validación en iPhone**; la continuidad es geométrica respecto de la zona. El snap anticipado −41→−40 ms del contrato se debe medir y documentar específicamente, sin cambiar ventanas, K, programación, cuantización, vecino/empate ni compensación de contacto. E3/E5 son independientes y no se anulan por Perfecto. La propuesta intercontextos y sus alternativas del informe son históricas, **sustituidas** en lo normativo por la decisión explícita del PO.

## Encargo futuro de implementación preparado — no ejecutado

El siguiente alcance queda preparado para el flujo posterior de Codex; **este registro no inicia ni autoriza por sí solo su ejecución**. La continuación actual termina para revisión de Claude coordinada por el PO, en PR #52 y sin fusión. Después de la revisión y de la autorización correspondiente del PO, la implementación tendrá **rama/worktree y PR independientes**. No hay merge automático.

1. **Base y primer registro:** partir de una base que contenga D36/D6 revisadas y aprobadas mediante el flujo del PO; verificar #46/#47/#50 y la documentación vigente antes de implementar. Registrar primero el encargo literal de implementación cuando se reciba, sin inventarlo a partir de esta plantilla. Conservar el informe y evidencia CQ-ZONE-v1 como línea de base; no repetir una ronda extensa de alternativas salvo defecto relevante.
2. **Modelo de referencia:** preparar una referencia inmutable una vez por recepción antes de cualquier intento, con trayectoria visible, tempo, ClockMap y compensación vigentes, J_A exhaustivo y Z_T de arco ±L recortada a K en el marco corporal. Los nombres exactos de los tipos quedan a criterio de implementación. `Evaluate` deja de recibir `bool perfect`; ni categoría, Δ ni paso relativo entran en la magnitud. La zona no se reconstruye según el swing. El contexto conserva la compensación de esa recepción: no releer cambios posteriores del panel para reevaluar un intento.
3. **Comportamiento:** distancia euclídea mínima a Z_T, cero dentro y crecimiento continuo fuera; pesos/topes por componente de T3.1 sin cambiar valores. Mantener E3/E5 independientes, E7 sin vector cuando no hay causas, técnica t=0 y DeltaLateral=0. Sin E8/volea, efectos direccionales, capa aleatoria ni overlay 3.3. No corregir la salida: aplicar la suma causal sobre P/v antes del solver.
4. **Dos jugadores y trazas:** misma preparación/evaluación/Apply por ShotController y RallyContact.Solve; humano con human.x/z o PlayerBody efectivo, rival con su PlayerBody en llegada, mismo alcance R. Registrar cada vector por separado con su causa en ambas trazas, incluida rally-point si aplica. No modificar RallyParams, pesos de IA ni cinemática para mejorar resultados.
5. **TDD obligatorio:** observar RED por aserción con esqueletos compilables antes de cada GREEN. Usar las sustituciones y los 14 casos de D6.2: todos los Perfect alcanzables dentro de la zona, continuidad por distancia a Z_T, lateral cero, igualdad literal dentro de la recepción/contexto **sin acotar por categoría** (Perfect/Valid +39/+41 al mismo paso), topes por componente, antes del solver, espejo, preparación única, cobertura por SL1–SL6/trayectorias de rival × tempo 0,3/0,65/1 × compensación −75/0/+75, determinismo, bordes exactos/±40/±100/KIn/KOut/vecino/empate, diferencia de tempo documentada sin exigir igualdad, snap anticipado −41→−40 medido y documentado sin corregirlo, simetría humano/rival. Los efectos posicionales siguen presentes aunque timing=0. No convertir la diferencia intercontextos en una excepción por categoría dentro de un contexto.
6. **Claves y hashes:** incorporar las diez `causal.*` autorizadas con los valores de T3.1/D6.3, todos [EXPERIMENTO] hasta iPhone. Calcular hashes de defaults/P0 efectivos **en Unity** antes de actualizar anclajes de AutoQA/verificador del operador en un **commit propio**. Mantener los 41 heredados, sus proyecciones, SimulationKey y bytes de P0-calib-1. No promover presets ni modificar los parámetros del rally.
7. **Evidencia de implementación prevista:** H3b completo con vector causal por contacto, según el mismo método predeclarado de #45 (SL1–SL6, semillas 0–7, bots 0/−60/−100/−150 ms, T3/V), separado de regresión. Informar contactos, fallos al aire/tarde y acierto de zona; los fallos no añaden vector cero. Regenerar RallySimTests y describir duración/causas/ganador frente a la evidencia del 11 nov, sin ajustar pesos/cinemática. Medir el snap −41→−40 específicamente conservando su causa contractual. Si hace falta modificar un método, predeclarar/commitear antes de ejecutar; un defecto relevante se registra y eleva al PO, no dispara ajustes ni una ronda extensa sin decisión. Estos experimentos previstos no se ejecutan en la continuación documental.
8. **Verificación/entrega futura:** suites completas frescas con un solo Unity, AutoQA **PASS / 0 / nivel 1** sin cambiar expectativas, check-release dev/prod limpio y ProjectSettings transitorios restaurados. Un escenario de AutoQA que cambie de resultado se registra y detiene. Informe propio con RED→GREEN, hashes y evidencia; PR independiente lista para Claude coordinada por el PO, **SIN FUSIONAR**. Sin cambios de spec/criterios/ventanas/contrato/M2-4/pullFullMm/#32 adicionales a D36/D6. No acceder al iPhone, compilar nativo, instalar ni iniciar sesión física sin autorización específica; la **prueba jugable y validación en iPhone quedan pendientes para el flujo autorizado posterior** y son necesarias antes del cierre, no se infieren de las suites. La entrega técnica no acepta First Shot.

### Control de esta continuación documental

El delta desde **ac3f79b** se limita a **cinco Markdown**: AGENTS.md, aprobaciones, este informe, spec v0.3 y plan First Shot r2. Primer commit solo los tres registros; segundo commit solo adendas y trazabilidad documental. La PR conserva el arnés y la evidencia de la fase anterior **sin cambios**. Comprobación documental final: cambios exclusivamente aditivos en los cinco Markdown, **1.103 archivos versionados fuera de ellos idénticos al blob Git de ac3f79b**, **14 enlaces añadidos válidos**, incluidos sus destinos/fragmentos, y literal idéntico en los tres registros del primer commit. No se ejecutan tests ni herramientas Unity. Los resultados experimentales y sus hashes siguen siendo los de la fase previa, sin corridas nuevas.

PR #52 lista para revisión de Claude coordinada por el PO, **SIN FUSIONAR**. Implementación y prueba jugable no realizadas, First Shot no aceptado. **Detenerse al entregar.**
