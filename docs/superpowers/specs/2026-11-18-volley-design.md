# Volea v1: diseño (fase 1)

18 nov 2026 · Subagente Opus dirigido por Claude (product lead); **la revisión de Claude no es independiente**. Base `main` `75b9ce8`, rama `claude/volley`. Solo diseño: no se abrió Unity ni se implementó nada. Fuentes: spec v0.3 §7.1, §7.3, §9.1, §11.3 y §17.A (D23, D24, D36), y el código citado abajo.

## 1. Cómo funciona hoy y por qué no hay volea

La volea está bloqueada en tres puntos independientes. Cada uno basta para impedirla:

1. **Tramo K solo después del bote.** `Core/Shot/ContactPlanner.cs`: `Query.IsLegal` y `Query.Ranges` exigen `bounce==1` fijo en el código (además del frontis y de no haber tocado una línea). `ContactContract.Decide` (`Core/Shot/ContactContract.cs`) valida con `ContactPlanner.IsLegalAt(authoritative, …, plan.Body, plan.Legal)`, así que ningún contacto antes del bote puede salir Perfect ni Valid.
2. **Reglas sin volea.** `Core/Rally/PointRules.cs` ya tiene `volleyAllowed`: `CanHit` exige `FloorBounces<=1` con la bandera y `==1` sin ella. Solo la usan los tests (`PointRulesTests`, `MatchTests`). `RallySim` y `RallyDirector.BeginPoint` construyen `new PointRules(court, radius)` sin la bandera, de modo que `RacketContact` antes del bote se rechaza y queda como `SystemFault`.
3. **Una sola oportunidad por recepción.** `ReceptionPlanner.Plan` (`Core/Rally/ReceptionPlanner.cs`) arma la ventana con `rules.CanHit(who)`, y su comentario dice «It never picks a volley». En el humano, `ShotController.OnLaunched` (`Runtime/Shot/ShotController.cs`) calcula un único `Plan` con el cuerpo fijo `Snapshot.Body`. Ese cuerpo es el `BodyAtContact` que `RallyDirector.LaunchToHuman` le pasa por `human.x/z`. Hay además un único `PresentedPlans`, una única `CausalReference` (D36) y un único anillo (`TimingRingView`, en P\*). El rival va por `RallyDirector.PlanRival` → `ReceptionPlanner` → `FondoAi.Decide` → `RallyContact.Resolve`, con el mismo contrato, igual que `RallySim.BeginReception`.

**Lo que ya no depende del bote y se reutiliza tal cual:**
- el host, única autoridad de la pelota (`Runtime/Ball/BallSimulationHost.Shot.cs`: `ScheduleShot`/`ApplyShot` reemplazan la velocidad en el paso de contacto, sin mirar botes);
- `CausalQuality.Prepare`, que sondea el contrato con el plan que recibe;
- `ShotSolver`, `ShotPace` y el gesto M2-4 (`GesturePreview*`, que no lee el plan);
- `ClockMap`/`SimClock`;
- el brazo del cansancio, que se lee en el paso de contacto (`HumanStrikerArm`, `RallySim.Strike`).

## 2. Volea v1: propuesta decidida

| Tema | Decisión v1 | Por qué |
|---|---|---|
| **Quién** | Solo el **humano**, en Rally y Match. El rival (Fondo) y los bots (`BotBrain`, autopiloto) no volean por defecto; `BotBrain` recibe una opción `takeVolley` solo para tests y medición. | La volea del rival necesita su propia utilidad, aumenta el riesgo de equilibrio y rompería la identidad bit a bit de `RallySim`, que sostiene los experimentos de cansancio. Se mantiene la simetría de reglas y contrato (§17.A.9): el rival usa las mismas reglas, el mismo planificador y el mismo contrato; lo que falta es la opción en su política. Esa falta se registra contra §11.3 y se difiere a v2. |
| **Movimiento** | **M1** (§7.3.3, por defecto en la spec): el cuerpo siempre va hacia el plan de bote, igual que hoy. La volea aparece cuando, en ese camino, la pelota pasa al alcance antes de botar. W queda fija en la T (`HumanWaitBehindT=0`), sin V3. | Respeta D24: el planificador nunca toma una posición que favorezca una volea que el jugador no eligió. El camino del cuerpo y los planes de bote quedan idénticos a los de hoy. |
| **Oportunidad de volea** | Existe si hay un tramo continuo con: frontis tocado, **cero botes**, sin línea ni out, **sin saque en curso**, altura 0,6–1,9 m (§7.3.1), pelota dentro de `body.reach` del **cuerpo en movimiento** en cada paso, `KIn ≥ lanzamiento + ReactionS`, duración de K ≥ `VolleyMinWindowS` (0,05 s de simulación, umbral de relevancia de v1 en lugar de `q_min_rel`) y **separación** ≥ 0,25 s reales entre `KOut` de la volea y `KIn` del bote. La separación se exige solo si el plan de bote existe; si el bote es inalcanzable, la volea se ofrece igual. | Se aplica §7.3.2 al pie de la letra, salvo la relevancia: con M1 la holgura q no tiene sentido en la volea, así que la ventana mínima la reemplaza. Volear el saque es una adaptación arcade que se registra (§8.7.5). |
| **K, T\*, P\*** | `VolleyPlanner` (nuevo, Core/Shot) hace una pasada O(n) sobre la trayectoria predicha con el camino del cuerpo congelado en la recepción y elige el mejor paso del tramo, con la misma puntuación de altura y paredes. Luego congela `BodyFrame F` = cuerpo en ese paso y llama a `ContactPlanner.Plan` con `LegalSettings(Bounces=0, 0,6, 1,9)`. Si T\* cae fuera del tramo, no hay oportunidad. | Es la misma aproximación de cuerpo fijo por plan que ya usan el bote y el contrato; contrato, K y planificador quedan coherentes entre sí. El cuerpo real en movimiento solo entra por la capa causal (estirado y atorado). |
| **Contrato** | `ContactContract.Decide` **no cambia ni un byte**. Un envoltorio nuevo, `OpportunityContract`, convierte el soltar corregido a tiempo de simulación (`ClockMap.TryRealToSim`). Si cae en o antes de **S = punto medio entre `KOut` de la volea y `KIn` del bote** (o S=∞ si no hay bote), se evalúa contra la volea; si no, contra el bote. Después llama a `Decide` con el `PresentedPlans` de esa oportunidad. Sin volea, llama a `Decide` exactamente como hoy. | Es el caso 14 de §7.1.3: «se aplica la oportunidad en cuyo tramo cae r». La elección no depende de los cuadros y es determinista. |
| **Casos (T3)** | Antes de `KIn` de la volea: **¡Al aire!**. Dentro de K de la volea: contacto antes del bote, Perfect si \|Δ\|≤A y Valid si no (`validLimit 0`). Entre `KOut` de la volea y S: **¡Tarde!**. Entre S y `KIn` del bote: **¡Al aire!** (nunca una volea accidental). Bote y después: igual que hoy. **Un solo intento por recepción** (`AttemptConsumed`, sin cambios). Sin soltar no hay contacto (§17.A.7). | No hace falta ningún timing nuevo. A_volea=0,8·A **no se aplica en v1** (A=40 ms): la dificultad sale de un K más corto y de la capa causal (D23). |
| **Capa causal (D36)** | Una `CausalReference` **por oportunidad**, las dos preparadas al presentar y antes de cualquier intento, con el mismo camino de cuerpo congelado. `Solve` usa la de la oportunidad elegida. | La invariante §17.A.12 se mantiene dentro de cada contexto de oportunidad. Es una aclaración de D36, no un cambio de criterio; se registra con la PR. |
| **Cómo se ve** | **Un anillo a la vez:** primero el de la volea, en color ámbar y en P\*v, hasta S; después el anillo actual, en P\*b. La nota antepone «Volea · ». El corte de la ayuda parcial (nivel 1) se reinicia en cada oportunidad. La retícula de mira y M2-4 no cambian. Una línea en «novedades»; el tutorial no se toca. | Se cumple «las ventanas nunca se superponen» con menos trabajo y menos riesgo que dos anillos simultáneos. El jugador lo entiende así: aparece un anillo ámbar adelante → si suelto ahí, corto la pelota; si espero, aparece el anillo normal. |
| **Autoridad e aislamiento** | `VolleyPlanner` solo lee la predicción aislada (`host.Prediction` o `context.Predict`) y el camino congelado. El contacto pasa por `ScheduleShot`/`ApplyShot`. `PointRules(volleyAllowed: VolleyEnabled)` verifica el turno. `ReceptionPlanner` cambia `rules.CanHit` por un predicado solo de bote, idéntico al de hoy. | Una sola autoridad; la predicción no mueve nada; los planes de bote quedan bit a bit iguales. |
| **T3 y cansancio** | Sin claves nuevas de timing (`validLimit 0`, `assistMs 40`). Cansancio: el retardo y la velocidad ya están en el camino congelado (`MovementFactor`); f_r se toma al lanzar; el brazo f_s se lee en el paso de contacto de la volea (`AdvanceBodiesTo`); el gasto es `StrikeAt(paso, P)`. Sin claves `fatigue.*` nuevas. | La volea ahorra metros por sí sola, porque el cuerpo vuelve antes. El ajuste del cansancio sigue siendo solo de valores. |
| **Figura** | `SwingIntent` agrega `ContactHeight` y `Volley`, con valores por defecto que dejan todo como hoy. En una volea, `SwingView` usa un backswing corto y la clave de contacto a la altura de P\*v respecto del cuerpo. **Alta** (≥1,3 m): raqueta sobre el hombro y golpe corto. **Baja**: raqueta abierta a media altura. Derecha o revés salen de `plan.Side`. `CharacterView` toma el lado de la raqueta de la oportunidad visible. | Reutiliza las poses y los lados actuales sin rig nuevo. |
| **DEV y métricas** | Botón solo DEV «Volea sí/no», que rige desde el punto siguiente (precedente: #82). Línea nueva `shot-opportunity` por lanzamiento, con K, T\*, S, oportunidad elegida y motivo de ocultación (`serve`, `height`, `window`, `separation`, `disabled`). **`ShotRecord` no cambia de esquema**: sus K/T\*/Side pasan a ser los de la oportunidad usada, que en práctica y AutoQA V/T3 son idénticos. | Así se evita repetir F1-AUTOQA-01, el esquema cerrado de `PocObservation`. |

**Desviaciones de §7.3 que se registran con la PR:** sin A_volea; relevancia por ventana mínima; un anillo secuencial; sin volea del saque; sin W elegida (V3); el rival no volea; dos zonas causales por recepción.

## 3. Impacto

**Zonas protegidas (§4 de AGENTS).**
- **Toca** la de «timing, contrato de contacto y reloj» (dueño: Codex):
  - el campo `LegalSettings.Bounces` y su uso en `Query`, con valor por defecto 1 e idéntico al de hoy;
  - `OpportunityContract`, nuevo;
  - el camino del contrato en `ShotController`.
- **No toca:** `ContactContract.cs`, `ClockMap`, `SimClock`, física P2, solver, M2-4, `DefaultPreset.json` ni SimulationKey.

**Claves.**
- **Ninguna en `DefaultPreset.json`.** `RallyParams` agrega `VolleyEnabled=1`, `VolleyMinHeight=0,6`, `VolleyMaxHeight=1,9`, `VolleyMinSeparationS=0,25` (tiempo real) y `VolleyMinWindowS=0,05` (tiempo de simulación).
- Siguen el precedente de G1 A1: valores de Rally fuera del preset y con hash propio.

**Hashes.**
- **Sin cambios:** defaults `2ec0c953786a`/132 claves, P0 `f92f406f698a`, SimulationKey `166a96e319c7` y las anclas AutoQA SL5 T3 `750c407c8a2f`, V `e8df60f37d4b` y SL5 V `95a3f24313fe`.
- **Cambia:** RallyParams. El valor nuevo se mide en Unity en la fase 2, y la proyección sin los campos `Volley*` debe dar exactamente `927e304bbfec`.
- **Nueva:** una sola ancla, de la receta VL1, que solo usa la suite nueva.

**AutoQA nuevo, predeclarado.**
- La suite `--suite volley` va en `Scenarios/volley.json`, con el oráculo aditivo `volley-window/1`.
- La receta de práctica nueva **VL1** usa el índice 6: se agrega al final de `PracticeLaunchDefinitions` y el rango de `practice.launch` sube a 0–6.
- Solo VL1 activa la volea en práctica; SL1–SL6, V y T3 quedan bit a bit iguales.
- El campo nuevo `Scenario.opportunity` vale `bounce` por defecto, así que los JSON actuales no cambian.

| ID | Soltar | Esperado |
|---|---|---|
| VOL-PERFECT-VL1 | T\*v +0 ms | Perfect, aplicado, contacto antes del primer bote |
| VOL-AIR-EARLY-VL1 | `KIn` de la volea −60 ms | Failed/Air, sin contacto |
| VOL-GAP-LATE-VL1 | `KOut` de la volea + ¼ del hueco hasta `KIn` del bote | Failed/Late contra la volea |
| VOL-GAP-AIR-VL1 | `KOut` de la volea + ¾ del hueco | Failed/Air contra el bote; nunca una volea accidental |
| VOL-LETPASS-VL1 | T\*b +0 ms | Perfect después del bote; resultado de bote igual al de hoy |
| VOL-CANARY-WRONG-EXPECTATION | igual que PERFECT, con una expectativa falsa a propósito | el canario debe fallar |

**Conflictos.**
- **G1 A2** (`codex/g1-a2-flat-drive`, diferida): hoy solo agrega `ShotIntentSolver.cs` y sus tests, sin ninguna superposición. Cuando A2 se integre, tocará `CausalQuality.Solve`, `RallyContact.SolveIntention` y la vista previa y la retícula de `ShotController`, así que el único conflicto textual probable está en `ShotController`. En lo semántico, la volea no cambia la entrada del solver: A2 se aplica igual a cualquier contacto.
- **Reajuste del cansancio:** cero conflicto, porque la volea no toca `DefaultPreset.json` y los bots no volean, así que el Experimento B no cambia.

## 4. Plan TDD (fase 2, un solo Unity, `scripts/test.sh`)

Cada paso lleva primero un RED por aserción y después la suite en verde. Ante una sorpresa: registrar y detenerse.

0. **Inventario sin código.** Buscar en `ZRallyDirectorTests`, `ShotControllerTests` y similares los tests de rally que suelten lejos de T\*. Regla de parada: si con la volea activa un test existente cambia de resultado, se reporta; no se tocan ni el test ni el valor.
1. **`LegalSettings.Bounces`.** RED: con `Bounces=0` hay un tramo antes del bote y su P\* tiene y entre 0,6 y 1,9; `IsLegalAt` rechaza el paso posterior al bote. Control: los tests de `ContactPlanner`/`ContactContract` con valor por defecto 1 quedan idénticos.
2. **Reglas.** RED:
   - `volleyAllowed` con saque en curso → `CanHit` es falso;
   - la volea legal se acepta;
   - `ReceptionPlanner` da el mismo plan, campo por campo, con `volleyAllowed` activado y desactivado.
3. **`VolleyPlanner`.** RED:
   - cuerpo en movimiento frente a cuerpo fijo;
   - altura fuera de 0,6–1,9 → sin oportunidad;
   - K < 0,05 s → sin oportunidad;
   - separación < 0,25 s → sin oportunidad, salvo bote inexistente;
   - `KIn` antes de la reacción → sin oportunidad;
   - saque → sin oportunidad;
   - determinismo bit a bit.
4. **`OpportunityContract`.** RED: la tabla de casos de §2 completa, incluidos los bordes S, `KIn` y `KOut`. Sin volea, el resultado es idéntico a `Decide` en todos los casos de `ContactContractTests`. Sin soltar, nunca hay volea (§17.A.7).
5. **`RallyParams`, `RallyContext`, `RallySim` y `BotBrain(takeVolley)`.** RED:
   - el contacto antes del bote se aplica y el rival recibe un vuelo nuevo;
   - **identidad:** con `VolleyEnabled=1` y bots que no volean, los 1.927 puntos son idénticos bit a bit a `main`;
   - la proyección del hash da `927e304bbfec`.
   **Punto de control (no es compuerta):** en 200 puntos, medir la tasa de recepciones con volea ofrecida y los motivos de ocultación (R5 y R8 como observación). **Si la tasa es menor al 5 %, se detiene y se informa al PO sin tocar valores.**
6. **`ShotController` y `RallyDirector`.** RED en PlayMode:
   - primero el anillo de la volea y después el del bote, con el cambio en S;
   - soltar en K de la volea → `Applied`, `ContactStep` < primer bote y `ShotRecord` con el K de la volea;
   - soltar en el hueco → Late o Air;
   - un solo intento;
   - repetición, pausa y cambio de tempo vuelven a presentar las dos oportunidades;
   - `shot-opportunity` escrita.
7. **Vistas.** Tinte del anillo, nota «Volea · », pose alta y baja en `SwingView`, lado de la raqueta en `CharacterView` y botón DEV. Tests de PlayMode sobre el estado, no sobre píxeles.
8. **AutoQA `volley`.** La receta VL1 se fija en EditMode **antes del RED**, con una regla predeclarada: la primera de una malla fija que tenga las dos oportunidades, una separación de al menos 0,25 s y un K de la volea de al menos 0,1 s. Si ninguna la cumple, se detiene. Después: escenarios, `volley-window/1` y `--suite volley` en `scripts/autoqa.sh`.
9. **Compuertas:**
   - EditMode y PlayMode completos en verde, con el recuento informado;
   - AutoQA `--suite v`, `--suite t3` y `--suite volley` PASS/0/1, sin dirty;
   - `scripts/check-release.sh` dev y prod OK;
   - hashes de §3 verificados en Unity;
   - informe de una página.
   No incluye fusión ni instalación sin la aprobación del PO.

**Tamaño estimado (líneas agregadas y quitadas aproximadas):**

| Archivo | Cambio |
|---|---|
| `ContactPlanner.cs` | +20/−3 |
| `VolleyPlanner.cs` (nuevo) | ~110 |
| `OpportunityContract.cs` (nuevo) | ~50 |
| `PointRules.cs` | +4/−1 |
| `ReceptionPlanner.cs` | +2/−1 |
| `RallyParams.cs` | +6 |
| `RallyContext.cs` | +20 |
| `RallySim.cs` | +35 |
| `FondoAi.cs` (`AiShot`, `ReceptionPlan`, `BotBrain`) | +20 |
| `ShotController.cs` | +80/−10 |
| `RallyDirector.cs` | +25 |
| `TimingRingView.cs` | +10 |
| `ShotNoteView.cs` | +3 |
| `SwingIntent.cs` | +4 |
| `SwingView.cs` | +20 |
| `CharacterView.cs` | +3 |
| DevPanel | +15 |
| `PracticeMode.cs`, `PracticeLaunchDefinitions.cs`, `ParamCatalog.cs` | +15 |
| AutoQA (`Scenario`, `ScriptedPlayer`, `Oracle`, JSON, `autoqa.sh`) | ~+200 |
| Tests EditMode | ~+650 |
| Tests PlayMode | ~+400 |

En total, unas **500 líneas de producción**, unas **1.250 de tests y AutoQA** y ~2 PR-días.

## 5. Preguntas abiertas

1. **Bloqueante, de gobierno y no técnica:** los pasos 1, 4 y 6 tocan la zona protegida del contrato, cuyo dueño es Codex (encargo del PO del 15 nov). Decisión propuesta: esos tres pasos los implementa **Codex**, con este diseño como encargo, y el resto (Rally, vistas, DEV y AutoQA) puede ir por subagentes. La otra vía es que el PO autorice de forma explícita que Claude dirija también esos pasos. Sin una de las dos, la fase 2 no empieza.
2. No bloqueantes, ya decididas y que se registran con la PR:
   - las siete desviaciones de §7.3;
   - la aclaración de D36 sobre una zona por oportunidad;
   - el diferimiento de la volea del rival y de W (V3) a una v2, con su propio experimento (§7.3.5 y H6/H6b).

**Riesgos principales:**
- **Volea rara:** con M1, el cuerpo corre hacia el bote y la separación de 250 ms puede ocultar muchas oportunidades. Lo cubre el punto de control del paso 5.
- **Tests de rally con soltar temprano:** pueden empezar a volear. Lo cubre la parada del paso 0.
- **Aproximación de cuerpo fijo en K de la volea:** se registra en la métrica la distancia entre el cuerpo real y F en el contacto.
- **Doble `CausalQuality.Prepare` al lanzar:** se mide `PlannerCostMs` en el iPhone.
- **Poco tiempo para tensar M2-4 antes de una volea temprana:** lo mitiga `KIn ≥ ReactionS`.
- **Conflicto textual con la integración de A2 en `ShotController`.**
