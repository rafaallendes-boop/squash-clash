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
