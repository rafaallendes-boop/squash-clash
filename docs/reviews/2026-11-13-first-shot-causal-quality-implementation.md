# B4 — implementación de calidad causal (T3.1, D6/D36)

**Estado: DETENIDA por B4-CQ-IMPL-01, defecto del arnés de cobertura; implementación parcial, no apta para fusión.** Base main `09764ab`; rama independiente `firstshot/causal-quality-impl`. [Encargo literal y cierre de #52](2026-10-25-first-shot-po-approvals.md#b4--encargo-literal-de-implementación-y-cierre-de-pr-52-13-nov-2026), primer commit `1772bfc`. La revisión se coordina con el PO; sin fusión automática. First Shot sigue pendiente de prueba jugable integrada.

## Método de implementación y TDD

La referencia se calcula antes del intento y no conserva referencias a la trayectoria mutable ni al reloj vivo. Se congela una copia de ClockMap para calcular J_A con ContactContract.Decide real. Se particiona por los extremos de Perfect/Valid, T*, K, pasos y medios pasos, eventos, observaciones del reloj y presentación corregida. Se consulta cada borde y cada celda abierta; no se desplazan ventanas con epsilon. Las posiciones corporales efectivas por paso se congelan al preparar q. La curva poligonal, el arco firmado desde T* y el intervalo ±L se recortan a K, incluidos sus extremos fraccionarios.

Evaluate recibe referencia, cuerpo y punto efectivo; carece de Δ, categoría o bool perfect. El vector timing usa exclusivamente distancia euclídea mínima a Z_T. Los nodos aislados se conservan, incluido L=0. No se usa la distancia de arco como magnitud del efecto. La igualdad exigida pertenece a una misma recepción/contexto; entre tempos se documenta la diferencia.

E3/E5 usan distancia tridimensional al centro de referencia corporal y rampas lineales: E3 desde stretchStart·R hasta R; E5 desde jamDistance hasta distancia cero. Se conserva cada causa y t=0. Composición por componente antes del solver; los pesos y topes serán los diez valores autorizados [EXPERIMENTO]. Esta elección implementa el inicio de estiramiento/atoramiento sin introducir un cambio de ventanas ni una penalización por categoría.

Primer grupo: API neutra compilable y diez pruebas geométricas. Una corrida inicial sin XML por error CS1503 en una aserción nueva no cuenta como RED. Corregida la preparación de pruebas, XML fresco: **10 ejecutadas, 0 Pass, 10 fallos por aserción**, sin CS. Evidencia local ignorada `Logs/evidence/b4-causal/01-red.xml` y `01-red.log`. GREEN observado después: 10/10.

## Alcance conservado

No se modifica ContactContract, ventanas, P2, M2-4, SimulationKey, valores heredados ni rangos existentes. Sin H3b completo, medición nueva de R3, iPhone, build nativa, instalación, #32, nuevas mecánicas First Rally ni arte. Se conserva evidencia histórica CQ-ZONE-v1; no se repite su experimento. R3 conserva Fail 0/42 como observación abierta. Cualquier defecto o sorpresa se registra y detiene según el encargo.

Hashes/anclajes medidos en Unity y commit propio, integración humana/rival, pruebas D6 restantes, regresiones completas, AutoQA, check-release y restauración de transitorios: pendientes. No se anticipan resultados.


## Parada B4-CQ-IMPL-01 — no corregida ni reintentada

Fuente limpia **4d08f35**, corrida 2026-11-13 17:53:49Z–2026-11-13 17:53:50Z según XML (las horas exactas quedan en el JSON de evidencia). Resultado fresco **17 ejecutadas / 16 Pass / 1 Fail / 0 skipped**, sin CS. Se detiene ante la primera sorpresa, antes de GREEN runtime, regresiones completas, AutoQA y check-release. No se ajusta ninguna tolerancia, contrato, evidencia ni aserción para obtener Pass.

El fallo aparece en `ZoneCovers_AllReachablePerfect_AcrossTemposAndTrajectories_AndMeasuresEarlySnap`, SL3 / tempo 0,65 / compensación −75 ms. El producto obtiene **L=0,18950885632079206 m**. Mi prueba usa por error `zone[14]`, columna **adaptiveRadiusM**, como esperado: **0,18950155531216195 m** (tolerancia de la aserción 1e−9 m). La columna correcta es `zone[13]`, **LarcM**, y contiene **0,18950885632079206 m**, idéntico al valor productivo. Se identifica un **defecto del arnés nuevo**, no una discrepancia demostrada del producto ni de la evidencia de #52. La corrección necesaria es leer LarcM por su columna correcta, preferiblemente por nombre; queda pendiente, **sin aplicarla ni repetir la corrida tras la parada**.

La comparación de J_A de ese contexto pasó antes del fallo de arco; no se infiere cobertura de las 270 condiciones. Se completaron **21 contextos** y el 22.º se interrumpió por la aserción. No hay cobertura de todos los rivales ni SL4–SL6 en esta corrida. Las 15 pruebas de CausalQuality y la prueba de hashes pasaron en el mismo XML; la matriz incompleta sigue **Fail**.

### TDD observado y límites

| Grupo | RED válido | GREEN / estado |
|---|---|---|
| Geometría, referencia, composición, determinismo | 0/10, diez fallos por aserción | 10/10 |
| Claves experimentales y entrada previa al solver | 10/12, dos fallos por aserción | 12/12 |
| Cuerpo efectivo y ruta compartida Core | 12/15, tres fallos por aserción | 15/15 en la última corrida |
| Integración ShotController en PlayMode | 0/1, referencia todavía nula | GREEN pendiente; runtime implementado después, no validado |
| Matriz conservada de #52 y hashes | Cobertura añadida como regresión sobre el núcleo implementado | 16/17 global; parada por error de columna |

Los XML/logs locales de cada frontera quedan en `Logs/evidence/b4-causal/`. Una corrida inicial con CS1503 y total=0 no cuenta como RED. Las correcciones previas del montaje no se confunden con el defecto del arnés que dispara esta parada. [Resumen verificable y SHA-256 de cada XML](../validation/evidence/2026-11-13-b4-causal-implementation-stop.json).

### Medición parcial del snap −41→−40 ms

En SL1 y SL2, cada una con compensaciones −75/0/+75 ms, las tres compensaciones dan los mismos valores siguientes. Magnitudes de timing; las causas posicionales permanecen independientes. Son datos del contrato vigente en esta prueba, no aceptación perceptiva.

| Tempo | Paso a −41 ms → −40 ms | d a −41 ms → −40 ms (m) | Δaltura (m) | Δfracción de velocidad |
|---|---|---|---|---|
| 0,3 | 157 Valid → 158 Perfect | 0,046089741468661884 → 0 | 0,0276538469 → 0 | −0,0115224356 → 0 |
| 0,65 | 155 Valid → 158 Perfect | 0,13871678991808684 → 0 | 0,08323008 → 0 | −0,0346791968 → 0 |
| 1 | 153 Valid → 158 Perfect | 0,23198823479252398 → 0 | 0,139192954 → 0 | −0,05799706 → 0 |

SL3 / tempo 0,3 / las tres compensaciones: −41 y −40 son **Failed/Air**, sin contacto ni vector; no se registran como cero. Son **21 pares medidos**, 18 con contacto y salto / 3 Air–Air; las demás recetas/contextos siguen pendientes. No se modifica el snap contractual. En el caso sintético de diferencia entre tempos: L rápido=0,0399999618530273 m, L lento=0,0100002288818359 m; se conserva y documenta la diferencia, sin exigir igualdad intercontextos.

### Hashes efectivamente calculados en Unity

DefaultPreset **9041c4aafc77 / 118 claves**, P0 efectivo **b6914a349068**, escenario SL5 completo (practice.launch=4, human.z=(double)1.6f) **e2ffa032a588**. Proyección de 41 heredados **0d39e6016bd8**, P0 heredado **8de3f2ecb217**, SimulationKey **166a96e319c7**. Se leyó el respaldo local real de P0-calib-1, se comprobó que su overlay coincide con el derivado y que sus bytes no cambian; SHA-256 **2f1295f201e238d9494bee285dd3f46e5bbb0f98b09ccde90fbd268f0874e4fd**. Sin acceso al iPhone.

**El commit propio de actualización de hashes/anclajes sigue pendiente**: no se aplican después de la parada. Los anclajes antiguos y los recuentos 108 de pruebas anteriores aún deben actualizarse con estas mediciones en una continuación autorizada; no constituyen regresiones corridas ni resultados nuevos.

### Implementación parcial y pendientes observados

El código incorpora referencia geométrica, claves, composición y ruta Core compartida; integra ShotController, RallyContact, RallySim y la ruta runtime del rival. Los solves hipotéticos de intención se llaman explícitamente `SolveIntention`; los contactos usan la ruta causal. ShotRecord conserva su esquema y los vectores usan trazas `shot-causal`/`rally-causal`, con resumen en rally-point. Cambios en el bucle existente solo para pasar cuerpo/contexto y registrar causas, sin nuevas mecánicas First Rally.

**Sin GREEN runtime ni regresión completa, no se da por terminada esa integración.** Revisión estática tras detenerse: `pointCausal` todavía no se vacía en `BeginPoint`, y la preparación de recepción con ClockMap pausado requiere comprobarse antes de declarar completa la ruta. Se registran como pendientes adicionales, sin corregirlos después de la parada. No se afirma simetría runtime validada, aunque la igualdad Core pase.

RallySimTests no se han regenerado ni ejecutado en esta entrega. Tampoco se han ejecutado suites completas, AutoQA ni check-release. Contrato, solver, RallyParams, spec, plan, evidencia CQ-ZONE-v1 y los **108 valores anteriores** de defaults intactos; solo se añaden diez claves. **ProjectSettings idénticos a la base**, sin transitorios pendientes. Un solo Unity por vez; ninguno ejecutándose al documentar la parada. No hay iPhone, build nativa, instalación, H3b ni medición R3 nuevas. R3 sigue Fail 0/42. Revisión de Claude coordinada por el PO, PR independiente **SIN FUSIONAR**, marcada bloqueada para revisar esta parada; First Shot no aceptado.
