# B4 — continuación de PR #53 y parada B4-CQ-IMPL-02

**DETENIDA, PR #53 mantiene [BLOCKED] y no es apta para fusión.** La primera parada B4-CQ-IMPL-01 queda resuelta por la lectura correcta del arnés. Esta continuación termina ante un fallo diferente en PlayMode completa: `FeasibleAdjustment_ShowsIntentionAndFeasibleMarks`, **TargetLowered esperado True, observado False**. No se corrige ni reintenta tras el fallo; AutoQA y check-release no se ejecutan. No acepta First Shot.

## Autorización y trazabilidad

Misma rama/worktree `firstshot/causal-quality-impl`, misma [PR #53](https://github.com/rafaallendes-boop/squash-clash/pull/53), head inicial exacto **92a6696f721240e8fb23005238e1c63177ba3c6b**, árbol limpio y PR abierta/sin fusionar. Primer commit **218204c**, únicamente registro literal en AGENTS.md y aprobaciones, antes de cualquier cambio/corrida. El encargo recibido contiene el marcador «[pegar la respuesta literal del PO]»: se preserva literalmente y no se inventa otra respuesta. Ese mismo encargo constituye la autorización expresa y comunica la revisión de Claude: columna 13 `LarcM` coincide con producto, columna 14 es `adaptiveRadiusM`; defecto del arnés, no del producto.

## 1. Arnés y cobertura completa

**e0ca095:** todos los campos de zones.tsv y los otros TSV usados para reconstruir/comparar la recepción se leen **por nombre de encabezado**, incluido `LarcM` para `ArcRadius`. Sin índices de columna fijos ni cambios de tolerancia, corpus histórico o producto. La aserción de arco conserva **1e−9 m**; la cobertura Perfect conserva distancia geométrica cero.

Corrida limpia: **2/2**, cero failed/skipped/CS. Matriz completa **270 condiciones = (SL1–SL6 + 24 trayectorias de rival) × 3 tempos (0,3/0,65/1) × 3 compensaciones (−75/0/+75 ms)**. **1.026 contactos Perfect distintos por contexto** verificados en total: J_A coincide con la imagen histórica y cada contacto queda en Z_T. Son las trayectorias congeladas de CQ-ZONE-v1, sin nueva campaña ni alternativas; no constituye prueba universal de cualquier trayectoria ni validación perceptiva.

### Salto −41→−40 ms: 270 pares conservados

[Datos por contexto](../validation/evidence/2026-11-13-b4-causal-continuation/matrix-and-hashes.json), con resultado, paso, distancia y vector de timing de ambos lados. Ningún fallo se convierte en vector cero.

- **252 pares Valid→Perfect cambian de paso físico**. En **180** el vector de timing pasa de no cero a cero; en **72** ya era cero a −41 porque la geometría está en Z_T. Cambiar de paso no implica cambiar de magnitud.
- **18 pares Failed/Air→Failed/Air**, todos SL3/SL4 en los nueve contextos de cada receta: sin contacto ni vector.
- SL1/SL2: por tempo 0,3/0,65/1, pasos **157/155/153 → 158**, d anterior **0,046089741468661884 / 0,13871678991808684 / 0,23198823479252398 m → 0**. Las tres compensaciones coinciden.
- SL5/SL6: pasos **112/110/108 → 113**, d anterior **0,0000084691048205405579 / 0,000075785055682147322 / 0,00020847235766066182 m → 0**. Se conservan las magnitudes pequeñas sin introducir umbral nuevo.
- Las **216 condiciones de rival** también tienen su par completo registrado. El contrato, las ventanas y el snap se conservan.

## 2. TDD de limpieza y pausa

**81111ec RED válido:** cuatro tests nuevos ejecutados y fallidos por aserción, sin CS: referencia nula tras reanudar con cola vacía, resultado Failed donde corresponde NotEvaluable sin target observable, contacto rival reanudado sin referencia y trazas del punto anterior retenidas.

**bad5df2 GREEN 4/4**, mismas aserciones:

- `BeginPoint` vacía `pointCausal` antes de iniciar el punto nuevo. El test inspecciona dos registros reales `rally-point` y comprueba que el segundo no conserva launchId anteriores.
- ShotController actualiza la presentación por observaciones del reloj incluso con cola vacía, antes de procesar órdenes. Una referencia ya preparada se conserva; no depende de datos/categoría del intento.
- Mientras no existe target mapeado, no se publica un target ficticio 0. Sin plan presentado evaluable, el **ContactContract existente** devuelve `NotEvaluable / no-presented-plan-at-event`, sin vector ni contacto.
- El rival congela su camino corporal al planificar; la preparación geométrica espera a que el mapa permita observar T*, antes de resolver el intento. La referencia se prepara una sola vez. El test reanuda una recepción real del rival y observa el contacto sin SystemFault.

No cambian ContactContract, ClockMap, CausalQuality, solver, parámetros/ventanas ni configuración matemática en esta continuación.

## 3. GREEN focal runtime y hashes medidos

**3ef1a3c, 3/3 PlayMode:** integración original de ShotController GREEN y dos comparaciones de la entrada/salida humana runtime con `RallyContact.Solve`, a Δ=−60 y 0 ms, usando misma referencia/cuerpo/contacto/intención. Vectores, objetivo/velocidad compuestos y solución iguales; cero instancias de predictor vivas al terminar. No se infiere aceptación jugable de estas pruebas.

Unity midió **9041c4aafc77 / 118 claves** en defaults, **b6914a349068** en P0 efectivo y **e2ffa032a588** en SL5 preparado completo (`practice.launch=4`, `human.z=(double)1.6f`). La lectura del respaldo P0 local confirma overlay y bytes, sin acceder al iPhone. Proyecciones **0d39e6016bd8 / 8de3f2ecb217** y SimulationKey **166a96e319c7**, intactas. La re-medición en EditMode completa coincide exactamente.

Commits propios **330d190** (anclajes/recuentos a partir del XML, incluidas las diez claves autorizadas en la comparación de S3) y **0ed17c5** (constantes del verificador de operador). Sin modificar las fixtures históricas ni los arneses de experimentos congelados. Controles Python del verificador: **5 ejecutados Pass, 1 control opcional de copias reales skipped, salida 0**; no equivalen a ejecutar AutoQA MVP.

## 4. RallySimTests regenerado, sin ajustes de IA o cinemática

La regresión EditMode ejecutó el método existente de **1.000 rallies**, Fondo Medio contra bot σ=30 ms, semillas y receptor alternado originales. [Informe regenerado](../validation/evidence/2026-11-13-b4-causal-continuation/rally-sim/report.md), [rallies individuales](../validation/evidence/2026-11-13-b4-causal-continuation/rally-sim/rallies.tsv) y [comparación derivada](../validation/evidence/2026-11-13-b4-causal-continuation/rally-comparison.json).

| Medida | Evidencia 11 nov | Capa causal actual |
|---|---:|---:|
| Rallies que terminan en punto | 1.000 | 1.000 |
| DoubleBounce / Air | 615 | 593 |
| DoubleBounce / Late | 316 | 312 |
| DoubleBounce / Unreachable | 33 | 36 |
| Out | 36 | 59 |
| Ganador Human / Rival | 311 / 689 | 330 / 670 |
| Media de golpes | 19,265 | 19,367 |
| Media de segundos reales de juego | 40,2739102564 | 40,7684230769 |
| p90 / p99 / máximo de duración (s) | 90,49 / 171,82 / 272,60 | 91,69 / 175,51 / 333,28 |
| Fallas de sistema / contactos rechazados por turno | 0 / 0 | 0 / 0 |

Hay 58 Out del rival por BackWall y 1 por LeftWall; antes los 36 eran BackWall. Cero Stalled, LaunchFault, StepCap o ShotCap. Cómputo de esta corrida **51,1 s** frente a **15,9 s** del registro histórico; no es una medición de rendimiento en iPhone ni una aceptación de FPS.

**Por qué se regenera:** la capa causal modifica P/v antes de resolver. Cambiar una salida cambia la siguiente trayectoria, oportunidades y decisiones del mismo rally; con los mismos seeds la secuencia puede divergir. No se retocan expectativas de RallySimTests, pesos, dificultad ni cinemática para recuperar la tabla antigua. RallyParams actual **7d50ddaf5d4f**, intacto en B4. El hash histórico **23ddf7292b9e** cambió en R6 al añadir parámetros de partido; la regresión posterior `c9e2488` ya reprodujo la tabla antigua byte a byte con 7d50ddaf5d4f, según el [registro anterior](2026-11-11-first-rally-gameplay.md). Se informa esa procedencia en vez de atribuir al hash una modificación de pesos. Esta comparación no ejecuta H3b, R3 ni juicio deportivo nuevo.

## 5. Regresión completa y nueva parada

| Comando | Fuente | Resultado |
|---|---|---|
| `scripts/test.sh editmode` | 330d190; durante la corrida solo cambió el Markdown del operador, ningún Assets/test | **708/708**, XML fresco, cero failed/skipped/CS |
| `scripts/test.sh playmode` | 0ed17c5, limpio | **456/457**, XML fresco, un fallo, cero skipped/CS |
| `scripts/autoqa.sh mvp` | — | **No ejecutado**, parada previa |
| `scripts/check-release.sh` | — | **No ejecutado**, parada previa |

PlayMode fue **una sola corrida completa**, 2026-11-13 **15:10:19–15:12:43 America/Santiago** (18:10:19Z–18:12:43Z). Fallo **B4-CQ-IMPL-02** en `ShotControllerTests.FeasibleAdjustment_ShowsIntentionAndFeasibleMarks`, aserción `Assert.That(r.TargetLowered, Is.True)`, línea 162 de la fuente probada. Fixture: SL3 (`practice.launch=2`), vMax=vCap=10 m/s, P=0, objetivo (3,2;4,4), suelta en T*. Observado **False**.

El cuerpo del test es **idéntico al de 92a6696**. No se altera el test ni se atribuye el fallo al entorno. El XML no guarda los campos completos de ShotRecord de ese caso y el teardown elimina su archivo de métricas; **la causa del False no queda demostrada por la evidencia retenida**. No se ejecuta una reproducción adicional ni se cambia producto, preparación, criterio o aserción después del fallo. La investigación de la interacción entre la composición causal, el solver y las marcas factibles queda pendiente de decisión del PO.

## Integridad y entrega de la parada

XML/logs y diff de ProjectSettings preservados localmente en `Logs/evidence/b4-causal-continuation/`; [SHA-256, resultados y pendientes](../validation/evidence/2026-11-13-b4-causal-continuation/continuation-stop.json). GraphicsSettings/QualitySettings transitorios restaurados desde su estado limpio; todo ProjectSettings idéntico al head inicial. Corpus #52 íntegro por SHA-256. Spec/plan, Core (incluidos contrato, reloj, solver y capa causal), DefaultPreset, rangos, RallyParams, pesos y cinemática sin cambios durante esta continuación. Solo los dos archivos runtime autorizados, tests, anclajes y documentación cambian.

Un solo Unity por vez; **ningún Unity ni corrida nueva después de la parada**. Sin build nativa, instalación, iPhone, H3b o R3 nuevos. R3 conserva Fail 0/42 como observación abierta. Falta el gate completo PlayMode/AutoQA/check-release para quitar [BLOCKED]. PR #53 para revisión de Claude coordinada por el PO, **SIN FUSIONAR**, sin mensajes externos a Claude. First Shot sigue sin aceptar y pendiente del playtest integrado.
