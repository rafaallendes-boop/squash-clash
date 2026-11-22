# El rival ante el golpe corto: diagnóstico, incremento mínimo y propuesta de preset

22 nov 2026 · Claude define la arquitectura (modo product lead, §2 de AGENTS). La implementa Codex y la revisa Claude. **La revisión de Claude no es independiente.** Base `origin/main` `518f9c3` (verificada con `git fetch`). Rama `docs/rival-corto-arq`. Solo documentación: no se toca código de producción, preset, física, solver, `pullFullMm`, P0-calib-1 ni la calibración del jugador.

Alcance: el definitivo de SC-Lead (22 nov), que reemplaza los puntos 2, 3 y 5 del encargo original donde chocan. Son tres partes: (1) diagnóstico en simulación sin tocar parámetros; (2) un mínimo que entra sí o sí, en un incremento separado y sin valores de preset; (3) las palancas de preset quedan como propuesta para Rafael.

> **Estado de la medición.** No se pudo correr Unity. A las 10:43 la build del worker de instalación (`sc-install-518f9c3/Logs/build-dev.log`) terminó con «No valid Unity Editor license found». Por la regla de un solo Unity tampoco correspondía abrirlo mientras ese worker estaba activo. El diagnóstico de §1 sale **del código**, con archivo y línea, más un cálculo cinemático fuera de Unity (§1.6). Las tablas de cruce de §2 quedan **predeclaradas y vacías**: el experimento (Anexo A) y el comando (§2.4) están listos para correr apenas haya Unity con licencia. No se inventan cifras.

## Camino B: decisión de SC-Lead (22 nov, RAL-174). Reemplaza a I2 donde choquen

**Contexto.** La PR #97 de Codex (head `e37fbbb`) implementó I1–I3 con I2 «estirado, sin dos paredes». El gate completo de EditMode quedó en 977/978. Falló el test existente de M3, `BoastRivalTests.On_InPlay_TheFondistaBoastsLessFromTheBackQuarterThanTheDefinidor`, con Fondista 3,99 % y Definidor 1,47 %, cuando exige Fondista < Definidor.

El diagnóstico está en el [comentario de #97](https://github.com/rafaallendes-boop/squash-clash/pull/97#issuecomment-7087023653). Es una consecuencia de I2 tal como lo escribió esta spec, no un defecto de la implementación: el Definidor juega casi todas sus dos paredes cuando llega estirado al fondo, y I2 se las quitaba. SC-Lead eligió el camino B. **No cambian oráculos, umbrales, semillas, pesos ni presets.**

### Regla de producto

- **Estirado adelante**, después de una dejada o un corto: solo globo o recta alta. En el código es {recta, cruzado} × alto × media, sin dos paredes. No existe una opción «globo» propia: esa es la P5 de §6, que sigue siendo propuesta.
- **Estirado atrás**: además de lo anterior, se permite la dos paredes.
- **En los dos casos**, toda opción pasa el filtro de I3: nunca ganadora, y el humano tiene que poder alcanzarla según el predictor y el planificador compartidos.
- Las recepciones `planned`, incluidas las de holgura baja, no cambian.

**Qué es «atrás», sin parámetros nuevos:** la zona donde `ShotOptions.AddBoasts` ya ofrece la dos paredes (`ShotOptions.cs:67`).

- Contacto con z ≥ `BoastBackFraction`·L = 0,75 × 9,75 = 7,31 m, a ≤ `BoastWallMax` (2 m) de una lateral.
- Fuera de esa zona no se construyen opciones de dos paredes. Por eso «adelante», y también la media cancha, quedan sin dos paredes por construcción, no por una regla nueva de zona.

### Cambio de código en #97 (Codex)

1. **Conjunto estirado en `RivalAi.Decide`:** `allowed = (High && Medium && (Straight || Cross)) || Line == Boast` cuando `own.Reason == "stretched"`. Para el resto se mantiene la regla de `main`.
2. **`FilterStretched` con el pronóstico de la dos paredes.** Cuando la opción tiene `option.Side.Exists`:
   - la guarda de falta es `RallyContact.ForecastBoast(context, at, option.Side, option.P).Good`;
   - la velocidad sale de `RallyContact.SolveBoastIntention(context, at, option.Side, option.P, out _)`.

   Para las demás opciones queda como está (`Forecast` / `SolveIntention` con `option.Aim`). El planificador del humano, la demora `ReactionS` y el respaldo (la opción de menor retraso, con `LastStretchedFallback` marcado) son los mismos. No se añaden parámetros.
3. **No cambian:** I1, la huella de 1.080 decisiones `planned` (`53870c765e9e1cc410b87b41d810afc7fbe53ac0ffa42345b9b97192786de4de`), los hashes (defaults `207da8f2c636`, RallyParams `a82c34b661bf`, SimulationKey `166a96e319c7`), la física, el solver, el contrato, el reloj, el humano, `FondoAi`, `pullFullMm` y P0-calib-1.

### Plan TDD de B, sobre #97 (RED válido por aserción antes de cada GREEN)

| # | Test | Esperado antes del cambio |
|---|---|---|
| B1 | `RivalAi_StretchedFront_NeverBoasts`. Reemplaza a `RivalAi_Stretched_NeverBoasts`, un test nuevo de #97 que no existe en `main`, y el cambio se declara por la decisión RAL-174. Contacto estirado adelante (z ≤ 3 m) cerca de una lateral, dos paredes encendida, tres estilos, 500 decisiones: ninguna `ShotLine.Boast` | guardia: nace verde por construcción y se declara |
| B2 | `RivalAi_StretchedBack_MayBoast_WhenTheHumanReachesIt`. Mismo contacto de esquina de atrás que el test reemplazado y humano observado en la T. **Precondición afirmada en el test:** al menos una de las 4 dos paredes tiene `ForecastBoast.Good` y el plan del humano es `Reachable`. Definidor y Completo, 500 decisiones: alguna `Boast`. El Fondista queda fuera a propósito: con `BoastWeightFondo` 0 y `LongWeight` 1,2 la dos paredes es rara por diseño, y M3 le pide ≤ 5 % | RED: con I2 de #97 aparecen 0 |
| B3 | `RivalAi_StretchedBack_NeverPicksABoastTheHumanCannotReach`. Mismo contacto, con el humano observado en la esquina de atrás opuesta, lejos de donde muere la dos paredes. **Precondición afirmada:** existe al menos una opción alcanzable y al menos una dos paredes inalcanzable según `ForecastBoast` + planificador. Tres estilos, 500 decisiones: 0 elecciones inalcanzables y 0 dos paredes inalcanzables | RED: tras permitir la dos paredes (paso 1) y antes de corregir el pronóstico (paso 2), el filtro lee la dos paredes como un golpe directo al frontis. Si nace verde, se declara como guardia y se añade una aserción directa: la trayectoria que pronostica el filtro para una dos paredes toca la lateral antes que el frontis |
| B4 | Guardias, sin cambiar sus expectativas: `RivalAi_PlannedLowSlack_StillMayBoast`, `RivalAi_NotStretched_DecisionsUnchanged` (misma huella), `RivalAi_Stretched_PicksAnOptionTheOpponentCanReach`, `RivalAi_Stretched_NoReachableOption_PicksLeastLate_AndFlags`, `RivalAi_Stretched_FromFront_IsHighMediumStraightOrCross` y `Hashes_Unchanged` | verdes |
| B5 | Regresión completa `scripts/test.sh editmode`, incluido el test existente de M3 **sin tocarlo** | 978 o más, todos Passed |

Si alguna precondición de B2 o B3 no se cumple en el escenario predeclarado, se registra y se para. No se buscan parámetros para cumplirla.

### Condiciones de fusión (SC-Lead), medidas con experimentos explícitos fuera de la regresión

**F1 · M3 vuelve.** Se mide con `BoastExperiment.M3ConfirmRound2`: Medio, 100 partidos por estilo, `NewcomerBot` σ 90 y semillas `20270131`, las mismas de la confirmación del 20 nov. Así la referencia de `main` ya está publicada: Fondista 4,4 %, Completo 7,5 % y Definidor 10,3 % (`docs/reviews/2026-11-20-boast.md:85`).

- **Condición de SC-Lead:** en Medio, Fondista < Definidor y 5 % ≤ Definidor ≤ 15 %. Se lee la columna `backShare` de `confirm.tsv`.
- El resto de M3 (Fondista ≤ 5 % y Completo entre ambos) y los niveles Fácil y Difícil se reportan. Si el resto de M3 falla, también se registra y se para.
- Además debe pasar el test de regresión de M3 (B5).

```bash
pgrep -f "[U]nity.app/Contents/MacOS/Unity" && { echo 'STOP: ya hay un Editor Unity'; exit 1; }
source scripts/unity-env.sh
out="$ROOT/Logs/experiments/boast/m3-rival-short-b-runner"; [[ -e "$out" ]] && { echo "Ya existe: $out"; exit 1; }; mkdir -p "$out"
SC_BOAST_RUN=m3-rival-short-b "$UNITY" -batchmode -nographics -projectPath "$ROOT" -runTests -testPlatform EditMode \
  -assemblyNames SquashClash.EditMode.Experiments \
  -testFilter "SquashClash.Experiments.EditMode.BoastExperiment.M3ConfirmRound2" \
  -testResults "$out/results.xml" -logFile "$out/unity.log" > "$out/console.log" 2>&1
# Resultado en $ROOT/Logs/experiments/boast/m3-rival-short-b/confirm.tsv (el experimento nunca sobrescribe)
```

**F2 · No sube la tasa de ganadoras del rival cuando llega estirado (R8).** Se mide con el experimento del Anexo A. La única diferencia es una columna nueva `r_z`, la z del contacto del rival, para separar adelante de atrás. Se corre dos veces con las mismas semillas (`20261122`), 3 estilos × 3 niveles × 150 partidos:

- **base:** worktree en `518f9c3` con el archivo del experimento copiado sin cambios y sin versionar, `SC_RSB_RUN=b-base`;
- **rama:** head de #97, `SC_RSB_RUN=b-head`.

**Definición de R8:** entre las filas con `reason == stretched` y `result == Contact`, la fracción con `won == 1`. `won` incluye los errores del bot humano y se cuenta igual en las dos corridas.

- **Condición:** R8(rama) ≤ R8(base) en el total y en cada estilo, con los tres niveles juntos.
- Se reportan como datos, sin criterio: por nivel, adelante/atrás (`r_z` ≥ 7,31 m), recuentos absolutos y cuántas de las devoluciones estiradas son dos paredes.

```python
# python3 -I r8.py <rows.tsv>   (fuera de Unity; leer los dos rows.tsv y comparar)
import csv, sys, collections
n, w = collections.Counter(), collections.Counter()
for r in csv.DictReader(open(sys.argv[1], newline=''), delimiter='\t'):
    if r['reason'] != 'stretched' or r['result'] != 'Contact': continue
    for k in ('total', r['style']): n[k] += 1; w[k] += r['won'] == '1'
for k in sorted(n): print(k, w[k], n[k], f'{w[k] / n[k]:.4f}')
```

**F3 · Gates de siempre, en serie y con un solo Unity:** `scripts/test.sh playmode` con el recuento completo, `scripts/autoqa.sh mvp` (V/T3/volea PASS/0/1) y `scripts/check-release.sh`. Además, un informe de una página en `docs/reviews/` con F1, F2, los RED y las guardias. R7 se declara si no se mide.

**Orden recomendado de Unity:** B1–B4 dirigidos → B5 → F1 → F2 base → F2 rama → PlayMode → AutoQA → check-release. Cada corrida adquiere y libera su candado.

**Regla de parada.** Si falla B5, F1 o F2, se registra y se para, sin reintentar para obtener Pass y sin tocar umbrales, oráculos, semillas, pesos ni presets. **Decide Rafael.** La fusión y la instalación siguen necesitando el «sí» del PO (AGENTS §7).

**Riesgo para la calibración de B:**

- Frente a #97, B devuelve al Definidor y al Completo las dos paredes estiradas desde atrás que el humano alcanza. El efecto sobre M3 va en la dirección de `main`, pero su tamaño no se sabe hasta medir F1.
- Frente a `main`, B quita solo las dos paredes estiradas que el humano no alcanzaría. Eso debería bajar algo la cuota de dos paredes del Definidor y hacer al rival un poco más fácil (R7).

## HOY (22 nov): lo que entra en la build que Rafael prueba de 17:00 a 18:00

> **Superado en I2 por el camino B (arriba).** Lo que sigue queda como registro del alcance de la mañana.

Pedido de SC-PM (11:30): spec cerrada antes de las 12:15, Codex implementa desde ~12:30 y el merge con los gates en verde queda listo hacia las 16:00. **Hoy entra solo «nunca parado / estirado = defensivo»: I1 + I2.** Todo lo demás pasa a después (§«Después»).

**Bloqueante para los gates de hoy:** Unity no tiene licencia válida en este Mac («No valid Unity Editor license found», build del worker de instalación de las 10:43, sin log nuevo a las 11:33). Sin Unity no hay EditMode, PlayMode, AutoQA ni check-release, y por lo tanto no hay merge. **Rafael tiene que reactivar la licencia en Unity Hub antes de las ~12:30.** Además rige la regla de un solo Unity: Codex y el worker de instalación no pueden correr Unity a la vez.

### Qué hace Codex hoy

Rama `codex/rival-short-ball-min` desde `main` `518f9c3`. Hay que respetar el diseño de §5.1 I1 e I2 tal cual. No cambia:

- valores de preset ni campos de `RallyParams`;
- la física, el solver, el contrato ni el reloj;
- `pullFullMm` ni P0-calib-1;
- el humano;
- `FondoAi`.

1. **I1, nunca parado sin intento (solo vista).** Clase nueva `Core/Rally/RivalAttempt.cs` con `StepFor(context, trajectory, rulesAtStart, Vector2[] path)`, que devuelve el paso de máxima aproximación tras el primer bote (≤ segundo bote) y el lado; si el punto se cierra antes del primer bote no hay intento. `RallyDirector.PlanRival` guarda ese paso cuando `!pendingShot.Swing` y el punto sigue abierto, y `TickRival` emite **una sola vez** `RivalSwing(side, false)` al llegar a ese paso, siempre antes de `EndPoint`. `RallySim` no se toca.
2. **I2, estirado = defensivo puro.** En `RivalAi.Decide`, si `own.Reason == "stretched"`, solo se permite {recta, cruzado} × alto × media, **sin dos paredes**. La regla de holgura baja con plan `planned` queda igual.

### Plan TDD de hoy (numeración de §5.2; RED válido por aserción antes de cada GREEN)

| Orden | Paso §5.2 | Test | Esperado antes del cambio |
|---|---|---|---|
| 1 | 1 | `RivalAttempt_StepFor_IsClosestApproachAfterBounce` | RED: el esqueleto devuelve -1 |
| 2 | 2 | `RivalAttempt_StepFor_NoneWhenPointClosedBeforeBounce` | RED o guardia, según el esqueleto (declararlo) |
| 3 | 3 | `Director_UnreachableRivalReception_RaisesOneFailedSwing` (PlayMode) | RED: hoy 0 eventos |
| 4 | 4 | `Director_ReceptionWithSwing_RaisesNoExtraAttempt` (PlayMode) | guardia: nace verde, se declara |
| 5 | 5 | `RivalAi_Stretched_NeverBoasts` (tres estilos, 500 decisiones, dos paredes encendida) | RED: hoy aparecen dos paredes |
| 6 | 6 | `RivalAi_Stretched_FromFront_IsHighMediumStraightOrCross` (tres estilos) | caracterización: nace verde, se declara |
| 7 | 7 | `RivalAi_PlannedLowSlack_StillMayBoast` | guardia: nace verde |
| 8 | 12 | `Hashes_Unchanged` (defaults `207da8f2c636`, RallyParams `a82c34b661bf`, SimulationKey `166a96e319c7`) | guardia |

Escenario del paso 3: con `RallyDirector` y una dejada lanzada por el lado del humano, se coloca al rival en la esquina de atrás con `PlayerBody.Teleport` antes del lanzamiento. Ningún test de PlayMode lo hace hoy, así que Codex agrega el acceso mínimo y solo para tests. Si eso no es posible, puede extraer la decisión «emitir intento» a un método puro probado en EditMode y dejar en PlayMode solo la guardia del paso 4. Hay que comprobar que `RivalPlan.Reason == "unreachable"` antes de afirmar el evento. Si no se logra un caso inalcanzable determinista, **se registra y se para**; no se ajustan parámetros para conseguirlo.

**Gates de hoy, en el Mac del PO y en serie (un solo Unity):** `scripts/test.sh editmode` y `scripts/test.sh playmode` con el recuento completo, `scripts/autoqa.sh mvp` (V/T3/volea PASS/0/1) y `scripts/check-release.sh`. Hay que entregar un informe de una página en `docs/reviews/` con estos puntos:

- los RED y las guardias;
- los hashes sin cambio;
- el riesgo de I2: solo cambia recepciones `stretched` desde la esquina de atrás con dos paredes elegible, y puede mover la cuenta de dos paredes o M4;
- R7 no se midió hoy, y eso se declara como desviación.

La fusión e instalación requieren el «sí» de Rafael (AGENTS §7).

**Checklist en iPhone (17:00–18:00):**

- ¿la estirada del rival se lee como «lo intentó»?
- ¿queda algún momento en que mira la dejada sin moverse?
- ¿el rival estirado devuelve alto y no un winner?

### Después (no entra hoy; misma PR de docs u otra)

- Diagnóstico completo de §2: experimento del Anexo A, tablas de cruce y R4–R6. Sigue en paralelo cuando haya Unity libre.
- I3, el filtro «nunca winner»: cambia la secuencia del RNG y necesita la medición R7.
- I4, perseguir el bote: depende de que el diagnóstico encuentre `no-legal-window`.
- Los pasos 0, 8–11 y 13 de §5.2.
- La propuesta de preset P1–P6 (§6), que decide Rafael.

## 0. Resumen

- El rival **no se queda parado por falta de reacción**: en cada recepción corre hacia su plan (`RallyDirector.cs:471`, `RallySim.cs:202`). Se ve parado por dos ramas del código:
  1. Si el planificador declara la pelota **inalcanzable**, `RivalAi.Decide` devuelve «sin golpe» (`RivalAi.cs:115-117`). El director no emite `RivalSwing` (`RallyDirector.cs:481`). La figura llega al punto «menos tarde», se planta en pose de preparación (`CharacterView.cs:275-276`) y mira pasar la pelota, sin estirarse.
  2. Si no hay **ventana legal** (`no-legal-window`), el plan devuelve como destino la posición actual (`ReceptionPlanner.cs:47-48, 60`) y el cuerpo no se mueve.
- Cuando llega **estirado** (`stretched`, holgura 0), el modo defensivo **sí se activa** en los tres estilos (`ReceptionPlanner.cs:121`, `RivalAi.cs:118`). Quedan dos riesgos de «winner estirado»:
  1. La **dos paredes sigue permitida** bajo defensa (`RivalAi.cs:126`).
  2. Desde adelante, el tope de elevación (`ShotOptions.cs:54-55`, 22°) convierte el «alto» defensivo en una pelota de ~1 m en el frontis, la misma altura que un golpe bajo (`AimLow` 0,95).
- La calibración nunca lo midió. El bot de referencia `NewcomerBot` **no juega dejadas** (`RivalStylesBaselineExperiment.cs:36-41`) y `BotBrain` juega recta media por defecto.
- Incremento mínimo **I-min**, sin valores de preset:
  - (I1) estirada visible cuando no llega;
  - (I2) estirado = defensivo puro; tras el camino B, la dos paredes solo desde atrás y filtrada por I3;
  - (I3) filtro «nunca winner» cuando llega estirado;
  - (I4) perseguir el bote cuando no hay ventana, condicionado a que la medición lo encuentre.
- Las palancas de preset (espera en la T, lectura del golpe, llegada en frenada y globo desde adelante) quedan como propuesta en §6, **sin implementar**.

## 1. Diagnóstico en el código

### 1.1 Cómo decide hoy el rival una recepción

En juego, el rival es siempre `RivalAi` (`RallyDirector.cs:219`, `RivalAi.For`). `FondoAi` queda como referencia histórica: Fondista con volea y cobertura apagadas decide igual que él (`RivalAi.cs:69-70`). El flujo de una recepción, igual en `RallyDirector.PlanRival` (`RallyDirector.cs:464-483`) y en `RallySim.BeginReception` (`RallySim.cs:192-221`), es:

1. **Un solo plan por recepción, hecho al empezar.** Se hace en el paso del golpe del humano (o con `elapsed` si el director se enteró tarde, `RallyDirector.cs:467-469`). Nunca se replanifica: la «lectura» del golpe es una demora fija.
2. **Demora** = `ReactionS` 0,18 s + `PerceptionS`·k(estilo, nivel) + extra por cansancio (`RallySim.cs:200`, `RivalAi.cs:54-58`, `Fatigue.cs:51-55`). Por estilo y nivel, en pasos de 120 Hz:

   | Demora sin cansancio (s) | Fácil | Medio | Difícil |
   |---|---|---|---|
   | Fondista (k 1,5 / 1,75 / 2,5) | 0,300 | 0,250 | 0,183 |
   | Definidor (k 1,25 / 1 / 1) | 0,283 | 0,217 | 0,183 |
   | Completo (k 1,5 / 1,5 / 1,25) | 0,300 | 0,242 | 0,183 |

   Mientras dura la demora, el cuerpo sigue haciendo lo que hacía: si venía recuperando hacia la espera, sigue yendo hacia atrás (`PlayerBody.cs:62-72`).
3. **El planificador** (`ReceptionPlanner.Plan`, `ReceptionPlanner.cs:43-122`) usa una ventana legal L: tras el frontis, un bote y una altura entre 0,05 y 2 m. Elige un apoyo a ±0,6 m (`body.idealLateral`) de la pelota, con alcance esférico de 1,3 m centrado a 0,9 m de altura. Exige que el cuerpo **llegue y quede quieto (`Set`)** antes de T\* (`ReceptionPlanner.cs:83-84`, `PlayerBody.cs:85-92`); frenar cuenta como no haber llegado. Hay tres salidas:
   - `planned`: llega con holgura ≥ 0;
   - `stretched`: no llega a tiempo. Corre igual al candidato «menos tarde» y, si la pelota pasa a su alcance desde donde realmente está, conserva esa oportunidad con holgura 0 y calidad 0 (`ReceptionPlanner.cs:108-121`);
   - `unreachable`: igual que la anterior, pero la pelota no pasa a su alcance;
   - además, `no-legal-window` / `no-trajectory`: el destino es la propia posición (`ReceptionPlanner.cs:47-49, 60`).
4. **El cuerpo corre al apoyo del plan** después de la demora (`RallyDirector.cs:471`) en **todos** los casos, incluido `unreachable`. En `no-legal-window` el apoyo es su posición, así que no se mueve.
5. **La decisión** (`RivalAi.Decide`, `RivalAi.cs:113-175`):
   - Sin plan de bote alcanzable ni volea deseada, no hay golpe (`:115-117`).
   - Si hay golpe, `defensive = SlackS < DefenseSlackS` (`:118`): Fondista 0,12 s, Definidor 0,06 s, Completo 0,12 s.
   - En defensa solo se permiten {recta, cruzado} × alto × media, **y la dos paredes** (`:126`).
6. **La vista.** `RaiseRivalSwing(contact)` dibuja el golpe (`true`) o el fallo con tropiezo (`false`) (`CharacterView.cs:184-196, 292-322`). Solo se emite si hubo golpe: con contacto (`RallyDirector.cs:532`) o con un intento fallido Late/Air (`:507`). **Sin golpe no hay evento**, y la figura queda en `Set` con la pose de preparación (`CharacterView.cs:275-276`).

### 1.2 Por qué «se queda parado mirando»

| Rama | Cuerpo | Figura | Evidencia |
|---|---|---|---|
| `planned` / `stretched` con golpe que falla (Late, Air) | corre al apoyo | intento con tropiezo | `RallyDirector.cs:503-507`, `CharacterView.cs:292, 313, 322` |
| **`unreachable`** (sin golpe) | corre al apoyo «menos tarde» y frena | **se planta en pose de preparación; ningún intento** | `RivalAi.cs:115-117`, `RallyDirector.cs:481`, `CharacterView.cs:275-276` |
| **`no-legal-window`** | **no se mueve** | quieto | `ReceptionPlanner.cs:47-48, 60` |
| pelota que muere antes (tabla, fuera) | no se mueve (no hay ventana) | quieto | ídem; aquí es correcto, porque el punto ya terminó |

Hay una sutileza en `unreachable`: el apoyo «menos tarde» tiende a ser un **punto tardío** de la ventana (más tiempo disponible), así que el cuerpo corre a donde la pelota ya pasó y frena ahí. Visto desde la cámara, llega tarde y mira, que es justo lo que describe SC-Game Designer.

`no-legal-window` con el punto abierto exige que tras el primer bote la pelota no supere 0,05 m de centro (radio ~0,02 m), es decir, una dejada «muerta». No sé con qué frecuencia ocurre ante dejadas del humano: lo mide el experimento (§2). **I4 entra solo si aparece.**

### 1.3 ¿Se teletransporta?

No, en simulación. `PlayerBody.Step` es cinemático, con aceleración, frenada y velocidad máxima (`PlayerBody.cs:70-83`), y `Teleport` solo se usa al empezar el punto (`RallySim.cs:127, 136-137`). Lo que sí puede verse como un salto es la raqueta de la figura: al contactar, se coloca **en la pelota** si está a menos de 1,1 m y por debajo de 1,9 m (`CharacterView.cs:192`), mientras que el alcance del contrato es 1,3 m. Un contacto `stretched` a 1,1-1,3 m dibuja la pose estándar y no un brazo estirado. Queda como observación de la vista: no forma parte del mínimo y se comprueba en el iPhone (§4).

### 1.4 ¿Se activa el modo defensivo cuando llega estirado?

**Sí, en los tres estilos.** `stretched` lleva `SlackS = 0` (`ReceptionPlanner.cs:121`), y 0 < `DefenseSlackS` (0,12 / 0,06 / 0,12), así que `defensive = true` (`RivalAi.cs:118`). Hay dos huecos frente a «devuelve defensivo (globo o recta alta) y nunca un winner»:

1. **Dos paredes permitida en defensa** (`RivalAi.cs:126`, comentario `:133`). Solo aparece desde el cuarto de atrás (z ≥ 0,75·L) y cerca de la lateral (`ShotOptions.cs:67`), así que no afecta a las pelotas cortas recogidas adelante. Sí afecta a un estirado en la esquina de atrás, y la dos paredes no es ni globo ni recta alta.
2. **El «alto» desde adelante no es alto.** `aim.y = min(AimHigh 3 m, y_contacto + tan 22°·d)` (`ShotOptions.cs:54-55`). Con contacto a z ≈ 1,5 m, y ≈ 0,3 m y d ≈ 1,5 m, el tope es ~0,9-1,0 m, prácticamente `AimLow` 0,95. A potencia media eso es un drive bajo al fondo, que contra un humano en la T puede ser un winner. **Es una hipótesis**: la medición (§2.3, tabla «qué devuelve estirado») la confirma o la descarta.
3. Mientras el bote esté planificado (`planned`) con holgura ≥ `DefenseSlackS`, el rival puede atacar desde adelante (contradejada). El Definidor tiene la dejada abierta siempre que P\*z ≤ T+1 m (`RivalAi.cs:191`, `StyleShooterDropZoneAhead` 1). Eso es squash normal y no es «estirado». No se toca.

`FondoAiTests.MandatoryDefense_UnderPressure_IsHighAndLong` (`FondoAiTests.cs:93-98`) ya cubre la defensa con holgura 0,05 para Fondo, pero desde un contacto genérico y sin dos paredes. No hay test de `RivalAi` en `stretched` ni desde adelante.

### 1.5 Por qué la calibración no lo vio

- `NewcomerBot`, la referencia del 20 % de victorias, elige solo recta o cruzado a media altura (`RivalStylesBaselineExperiment.cs:36-41`): **nunca dejada**.
- `BotBrain`, usado por AutoQA y por `bot30`, juega por defecto `Straight/Mid/Medium` (`RallySim.cs:59`).
- `ZRallyDirectorTests` cuenta los `RivalSwing` con contacto (`ZRallyDirectorTests.cs:73, 125`), no las recepciones sin golpe.

Así que ningún experimento ni test mide la respuesta del rival a pelotas cortas. La observación abierta de AGENTS («rival ante golpes cortos», `AGENTS.md:60`) y el pedido de métricas «posición del rival ante golpes cortos desde adelante» (`docs/reviews/2026-11-16-camino-a-usuarios.md:54`) no tienen todavía un instrumento.

### 1.6 Presupuesto de tiempo para llegar adelante (cálculo cinemático, no medición de juego)

Réplica en Python de `PlayerBody.Step` (aceleración 12, velocidad máxima 4,8, frenada 16 y llegada a 0,01 m, a 120 Hz), desde la espera de cada estilo (T = (3,20; 5,465); espera = T + (0,4; W)) hasta quedar **quieto** en el apoyo más cercano (bola ±0,6 m). Sin cansancio y partiendo quieto de la espera, que es el mejor caso.

| Segundos hasta `Set` (sin demora) | z contacto 1,0 | 1,5 | 2,0 | 2,5 | 3,0 | 3,5 |
|---|---|---|---|---|---|---|
| Fondista (W 0,5), bola x 3,2 | 1,32 | 1,23 | 1,12 | 1,02 | 0,91 | 0,81 |
| Fondista, bola x 0,6 (esquina) | 1,44 | 1,35 | 1,26 | 1,18 | 1,09 | 1,01 |
| Definidor (W 0), bola x 3,2 | 1,23 | 1,12 | 1,02 | 0,91 | 0,81 | 0,71 |
| Definidor, bola x 0,6 | 1,35 | 1,26 | 1,18 | 1,09 | 1,01 | 0,94 |
| Completo (W 0,2), bola x 3,2 | 1,27 | 1,16 | 1,06 | 0,95 | 0,85 | 0,74 |
| Completo, bola x 0,6 | 1,38 | 1,29 | 1,21 | 1,12 | 1,04 | 0,97 |

Sumando la demora de §1.1, una dejada a la esquina con contacto en z = 1,5 exige al Fondista Fácil ~1,65 s entre el golpe del humano y T\*, y al Definidor Difícil ~1,44 s. Los 0,5 m extra de espera del Fondista cuestan ~0,1 s. El dato que falta, cuánto vuela de verdad una dejada del humano según la zona desde donde se pega, sale del experimento (columna `avail_ms`). Script: Anexo B.

### 1.7 Clasificación preliminar (a)/(b)/(c)

- **(c) existe con certeza en el código**: la rama `unreachable` no muestra ningún intento y la rama `no-legal-window` no se mueve (§1.2). Es lo que el mínimo corrige sí o sí.
- **(a) «no llega a dejadas normales» o (b) «llega a todo»** depende de las distribuciones de §2. No lo afirmo sin medir. La cinemática (§1.6) sugiere que desde el fondo llega a casi todo y que desde adelante, a dejadas bien ejecutadas, no llega. Es una hipótesis, no un dato.
- **«Winner estirado»**: el modo defensivo está activo, pero su contenido (dos paredes, «alto» con tope) puede producirlo. Se mide en §2.3.

## 2. Medición predeclarada (pendiente de Unity)

### 2.1 Método

Experimento explícito `RivalShortBallDiagnosisExperiment` (Anexo A), fuera de la regresión y de AutoQA (`[Explicit, Category("Experiments")]`). Configuración:

- Defaults del preset sin cambios (`207da8f2c636`/132), `new RallyParams()` (`a82c34b661bf`) y `RallySim` con P2-Analytic. Ningún parámetro se toca.
- Rival `RivalAi.For(ctx, nivel, estilo, semilla)`, el mismo constructor del juego, envuelto en un registrador que reenvía `IRallyPositioning` e `IRallyServer` para conservar la espera y la recuperación del estilo.
- Humano: `ShortMixBot`, con σ de tiempo 90 ms como `NewcomerBot`. Mezcla fija predeclarada: 40 % dejada (alto bajo/medio/alto uniforme, potencia media 70 % / fuerte 30 %), 15 % recta baja (media/fuerte 50/50) y 45 % largo como `NewcomerBot`. La mezcla busca muestras de dejadas desde todas las zonas; no representa a un usuario real.
- 3 estilos × 3 niveles × 150 partidos (`PlayMatch`), semillas base 20261122. Dos corridas con las mismas semillas deben dar `rows.tsv` idéntico byte a byte (determinismo).

Por cada recepción del rival que sigue a un golpe del humano (se excluye el resto del saque) se registra:

| Columna | Qué es |
|---|---|
| `kind` | `dejada` (línea Drop del humano), `corto` (otra línea con segundo bote z < T) o `largo` |
| `zone` | desde dónde pegó el humano: `adelante` (z contacto < T = 5,465) o `fondo` |
| `fw_tin` | altura del primer impacto en el frontis menos la tabla (0,48 m) |
| `b2z` | distancia del segundo bote al frontis (m) |
| `avail_ms` | tiempo desde el golpe del humano hasta el segundo bote |
| `apex2` | altura máxima entre botes |
| `reason` | `planned` / `stretched` / `unreachable` / `no-legal-window` / `no-trajectory` |
| `slack`, `delay_ms` | holgura del plan y demora de inicio del desplazamiento (reacción + percepción + cansancio) |
| `from_wait` | distancia del rival a su espera en el golpe del humano (¿venía recuperando?) |
| `path_m` | metros recorridos por el rival hasta su contacto o hasta el segundo bote |
| `short_m` | distancia mínima entre la pelota y su esfera de alcance en la ventana tras el bote (≤ 0: estuvo a alcance) |
| `swing`, `result` | si hubo golpe y el resultado (Contact / Late / Air / Unreachable / NoSwing / SystemFault / none) |
| `def`, `line/height/power`, `volley` | qué eligió |
| `ret_fw`, `ret_b2z` | la devolución: altura en el frontis y segundo bote |
| `next` | resultado de la recepción siguiente del humano |
| `won` | si el punto lo ganó el rival en esa recepción siguiente (winner o error forzado) |
| `r_z` | z del contacto del rival (`NaN` sin contacto); separa adelante de atrás en R8 (camino B) |

Definiciones de medición (no son umbrales):

- **Parado** = `swing == false` y `path_m < 0,30`.
- **Llega** = `result == Contact`.
- **Estirado** = `reason == stretched`.
- **Winner estirado** = estirado, `result == Contact` y `won == true`.

### 2.2 Tabla de cruce: ¿llega o no? (vacía hasta medir)

Una fila por estilo × nivel × `kind` × `zone` × franja de `b2z` × franja de `fw_tin`. Franjas predeclaradas:

- `b2z`: [0, 1,5), [1,5, 2,5), [2,5, 3,5), [3,5, T), [T, ∞).
- `fw_tin`: [0, 0,2), [0,2, 0,4), [0,4, 0,7), ≥ 0,7.

| estilo | nivel | kind | zona | b2z | fw_tin | n | llega % | estirado % | unreachable % | sin ventana % | parado % | p50 `delay_ms` | p50 `path_m` | p50 `short_m` |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| … | … | dejada | adelante | [0, 1,5) | [0, 0,2) | — | — | — | — | — | — | — | — | — |

Se publica completa en `docs/validation/2026-11-22-rival-short-ball/cross.tsv` y en un resumen de una página con las filas `dejada` y `corto`. Las distribuciones se reportan tal cual: sin número objetivo y sin «Pass».

### 2.3 Tabla «qué devuelve cuando llega estirado» (vacía hasta medir)

| estilo | nivel | zona | n estirado con contacto | def % | alto % | dos paredes % | bajo/medio % | p50 `ret_fw` | p50 `ret_b2z` | `next` ≠ Contact % | winner estirado % |
|---|---|---|---|---|---|---|---|---|---|---|---|
| … | … | adelante | — | — | — | — | — | — | — | — | — |

### 2.4 Comando preparado

No se corrió. Hace falta Unity con licencia válida y ningún otro Unity abierto. Pasos, en un worktree con `scripts/device.env` copiado sin cambios:

```bash
# 1) Copiar el Anexo A a Assets/SquashClash/Tests/Experiments/EditMode/RivalShortBallDiagnosisExperiment.cs
#    (es un archivo de experimento, no de producción; no va en la PR de docs)
# 2) Un solo Unity:
pgrep -f "[U]nity.app/Contents/MacOS/Unity" && { echo 'STOP: ya hay un Editor Unity'; exit 1; }
source scripts/unity-env.sh
out="$ROOT/Logs/experiments/rival-short-ball/run01-runner"; mkdir -p "$out"
SC_RSB_RUN=run01 "$UNITY" -batchmode -nographics -projectPath "$ROOT" -runTests -testPlatform EditMode \
  -assemblyNames SquashClash.EditMode.Experiments \
  -testFilter "SquashClash.Experiments.EditMode.RivalShortBallDiagnosisExperiment.WriteShortBallDiagnosis" \
  -testResults "$out/results.xml" -logFile "$out/unity.log" > "$out/console.log" 2>&1
# 3) Resumen (Anexo B, segunda parte):
python3 -I docs/validation/2026-11-22-rival-short-ball/summarize.py "$ROOT/Logs/experiments/rival-short-ball/run01"
# 4) Determinismo: repetir con SC_RSB_RUN=run02 y comparar rows.tsv con cmp.
```

Validez: XML fresco, 1/1 Passed y sin `error CS` en el log. Si no, se registra y se para, sin reintentar (AGENTS §2).

## 3. Criterio de aceptación verificable sin iPhone

Son métricas nuevas, salidas del mismo experimento. **No cambia ningún umbral existente** (M1/M1b/M4, B4, C3, G1).

| Id | Métrica | Tipo | Umbral |
|---|---|---|---|
| **R1** | Recepciones de pelota corta (`dejada` o `corto`) con el punto abierto en que el rival queda **parado sin intento**: ni golpe, ni estirada (I1), ni desplazamiento ≥ 0,30 m | invariante del mínimo | **0**, por requisito de SC-Lead («nunca se queda parado»), no como número de calibración |
| **R2** | Recepciones `stretched` con contacto cuya elección no es {recta, cruzado} × alto × media ni, solo desde la zona de dos paredes de atrás, `Boast` (camino B) | invariante del mínimo | **0**, por requisito de SC-Lead |
| **R3** | Recepciones `stretched` con contacto cuya devolución, según el predictor compartido (`ForecastBoast`/`SolveBoastIntention` para la dos paredes), deja al humano **sin plan alcanzable** desde su posición observada | invariante del mínimo (definición de «winner» en el planificador) | **0** cuando existe al menos una opción defensiva alcanzable; si no existe, se reporta aparte |
| **R8** | Tasa de ganadoras del rival cuando llega estirado: `stretched` y `Contact` con `won == 1`, sobre `stretched` y `Contact` (camino B, F2) | condición de fusión de SC-Lead | rama ≤ base `518f9c3`, en total y por estilo, con las mismas semillas; no es un umbral numérico |
| R4 | % «llega» por estilo × nivel × zona × franja de calidad (§2.2) | distribución | **lo fija Rafael**, propuesta para SC-Lead |
| R5 | p50/p90 de `delay_ms` y de `path_m` ante dejadas | distribución | **lo fija Rafael** |
| R6 | % winner estirado medido por `won` (incluye errores del bot humano) | distribución | **lo fija Rafael** |
| R7 | Desplazamiento de la calibración: M1 (victorias del newcomer) por estilo × nivel con las mismas semillas, antes y después de I-min | declaración de riesgo | solo se declara el Δ; no hay criterio nuevo |

R1–R3 se verifican en EditMode con `RallySim`, sin iPhone. R4–R6 son la base para que Rafael fije umbrales; quedan como propuesta para SC-Lead.

## 4. Qué se prueba sin iPhone y qué necesita iPhone

**Sin iPhone (Mac del PO):**

- [ ] EditMode: tests nuevos de I1–I4 (§5) y regresión completa `scripts/test.sh editmode`.
- [ ] PlayMode: el director emite la estirada en una recepción `unreachable` y no la emite en una con golpe; regresión completa `scripts/test.sh playmode`.
- [ ] Experimento §2 antes (diagnóstico) y después (R1–R3 en 0; R4–R7 reportadas).
- [ ] Hashes sin cambio: defaults `207da8f2c636`, RallyParams `a82c34b661bf`, SimulationKey `166a96e319c7`.
- [ ] AutoQA `scripts/autoqa.sh mvp` (V/T3/volea PASS/0/1). No toca timing, gesto ni contrato, pero I2/I3 cambian decisiones del rival y conviene la guardia.
- [ ] `scripts/check-release.sh`.

**Necesita iPhone (juicio del PO):**

- [ ] ¿La estirada del rival se lee como «lo intentó» y no como «se tropezó»?
- [ ] ¿La raqueta del contacto estirado (1,1–1,3 m) se ve como alcance y no como salto (§1.3)?
- [ ] ¿Sigue habiendo momentos en que el rival «mira» la dejada? (feel, no métrica)
- [ ] p95 de cuadro sin cambio apreciable (I3 añade predicciones solo en recepciones `stretched`).

## 5. Incremento mínimo I-min: diseño y plan TDD para Codex

Una rama y una PR: `codex/rival-short-ball-min`. No cambia valores de preset ni campos de `RallyParams`, ni la física, el solver, el contrato, el reloj, `pullFullMm`, P0-calib-1 o el humano (ni su planificador ni su cuerpo). Toca solo `RivalAi`, una clase nueva en Core, el director y `RallySim` (este último para mantener la paridad de medición).

> **Fusión.** SC-Lead pide revisión y merge autónomo. AGENTS §7 exige el «sí» explícito del PO para fusionar a `main`. Hasta que Rafael lo delegue por escrito, la PR espera su «sí».

### 5.1 Diseño

**I1. Estirada visible cuando no llega (solo vista, sin efecto en la simulación).**

- Clase nueva `Core/Rally/RivalAttempt.cs`, con una función pura `RivalAttempt.StepFor(context, trajectory, rulesAtStart, Vector2[] path)`. Devuelve el paso de máxima aproximación entre la pelota y la esfera de alcance del cuerpo sobre su camino congelado, en el tramo posterior al primer bote, acotado a ≤ el paso del segundo bote. Devuelve también el lado (`BodyFrame.SideFor`) en ese paso. Si el punto ya está cerrado antes del primer bote, no hay intento.
- `RallyDirector.PlanRival`: cuando `!pendingShot.Swing` y el punto sigue abierto, guarda `attemptStep` con el camino `past` que ya calcula (`RallyDirector.cs:474-476`). `TickRival` emite una sola vez `RivalSwing(side, false)` cuando `host.Current.Step >= attemptStep`, y siempre antes de `EndPoint`. Se reutiliza el evento existente: los suscriptores de sonido e impacto solo reaccionan a `contact == true` (`AudioScape.cs:169`, `ImpactFx.cs:43`, `StrokeFx.cs:93`), y `CharacterView` ya dibuja el fallo con estocada (`CharacterView.cs:195, 311-322`).
- No cambia `rivalReception`, ni el resultado del punto, ni las métricas, ni `RallySim`.

**I2. Estirado = defensivo puro.**

> **Corregido por el camino B (RAL-174):** desde la zona de dos paredes de atrás, la dos paredes se permite y pasa el filtro de I3. Adelante sigue excluida, por construcción. El texto original, que la excluía siempre, rompía M3 (comentario de #97).

- En `RivalAi.Decide`, si `own.Reason == "stretched"`, el conjunto permitido es {recta, cruzado} × alto × media y **se excluye la dos paredes**. Con holgura baja pero `planned` se conserva la regla de hoy (dos paredes permitida bajo presión).
- Afecta a la volea solo si la recepción de bote es `stretched`, porque el indicador `defensive` es común a las dos (`RivalAi.cs:118, 143`).
- `FondoAi` no se toca.

**I3. Filtro «nunca winner» cuando llega estirado.**

- En `RivalAi.Decide`, solo si `own.Reason == "stretched"`, cada opción permitida se pronostica con lo que el rival ya usa:
  1. `RallyContact.Forecast` para la trayectoria resultante.
  2. `ReceptionPlanner.Plan` con un cuerpo nuevo (`new PlayerBody(PlayerId.Human, observation.OpponentPosition, context.Move)`) y la demora `context.Steps(ReactionS)`.
- Las opciones cuyo plan del oponente no sea `Reachable` reciben peso 0 antes del sorteo. Si ninguna queda alcanzable, se elige la de menor retraso del oponente. Este caso se marca y se reporta (R3).
- Usa solo información observable (posición del oponente) y el predictor compartido, sin intención del oponente (`RivalAi.cs:93`), y no añade parámetros.
- Riesgo declarado: cambia el número de sorteos del RNG en esas recepciones, así que desde ahí la secuencia de la misma semilla ya no es idéntica (§5.3).

**I4. Perseguir el bote cuando no hay ventana (condicional).**

- Entra **solo si** el diagnóstico encuentra `no-legal-window` con el punto abierto (> 0 casos) tras una dejada.
- `RivalAttempt.ChaseStance(context, trajectory, rulesAtStart, body)` devuelve el apoyo a ±`IdealLateral` del primer bote, del lado del cuerpo, y `ClampBody`.
- El director y `RallySim` mueven el cuerpo del **rival** ahí con la demora normal. El planificador compartido no cambia: el humano queda igual.
- Efecto en la simulación: más metros recorridos en puntos ya perdidos, lo que da más costo de energía (`Fatigue.cs:84-88`) y puede mover el cansancio de los puntos siguientes. Se declara en R7 y en B4.

### 5.2 Plan TDD (cada paso con su RED válido por aserción antes del GREEN)

| # | Test (RED primero) | Archivo | GREEN mínimo |
|---|---|---|---|
| 0 | `ShortBallDiagnosis_BaselineRows`: el experimento del Anexo A corre y escribe `rows.tsv`. No es regresión: es la medición previa y sirve para decidir I4 | `Tests/Experiments/EditMode/RivalShortBallDiagnosisExperiment.cs` | — (solo medición) |
| 1 | `RivalAttempt_StepFor_IsClosestApproachAfterBounce`: trayectoria fija de dejada y camino de cuerpo fijo. Afirma el paso de máxima aproximación y que es ≤ el segundo bote. RED: la clase no existe; tras compilar el esqueleto que devuelve -1, falla la aserción | `Tests/EditMode/Rally/RivalAttemptTests.cs` | `RivalAttempt.StepFor` |
| 2 | `RivalAttempt_StepFor_NoneWhenPointClosedBeforeBounce` (pelota a la tabla) | ídem | guarda de reglas cerradas |
| 3 | `Director_UnreachableRivalReception_RaisesOneFailedSwing`: escenario determinista con dejada inalcanzable (posición del rival fija atrás). Cuenta `RivalSwing(_, false) == 1` antes de `PointEnded` y `RivalSwing(_, true) == 0`. RED: hoy cuenta 0 | `Tests/PlayMode/ZRivalShortBallTests.cs` | I1 en `PlanRival`/`TickRival` |
| 4 | `Director_ReceptionWithSwing_RaisesNoExtraAttempt`: con golpe, exactamente un evento. Es la guardia de I1; puede nacer verde y se declara como guardia, no como RED | ídem | — |
| 5 | `RivalAi_Stretched_NeverBoasts`: plan `stretched` en la esquina de atrás cerca de la lateral, con la dos paredes encendida, en los tres estilos y 500 decisiones. Ninguna `ShotLine.Boast`. RED: hoy aparecen | `Tests/EditMode/Rally/RivalAiTests.cs` | I2 |
| 6 | `RivalAi_Stretched_FromFront_IsHighMediumStraightOrCross`, en los tres estilos. Puede nacer verde (§1.4) y se declara como caracterización | ídem | — |
| 7 | `RivalAi_PlannedLowSlack_StillMayBoast`: holgura 0,05 y `planned` en la esquina de atrás; aparece alguna dos paredes. Es la guardia de que I2 no amplía su alcance | ídem | — |
| 8 | `RivalAi_Stretched_PicksAnOptionTheOpponentCanReach`: contacto estirado adelante y oponente en la T. Construir el caso con una opción defensiva que el planificador del oponente da por inalcanzable y otra alcanzable. Afirmar que nunca se elige la inalcanzable. RED: hoy se sortea | ídem | I3 |
| 9 | `RivalAi_Stretched_NoReachableOption_PicksLeastLate_AndFlags` | ídem | rama de respaldo de I3 |
| 10 | `RivalAi_NotStretched_DecisionsUnchanged`: 3 estilos × 3 niveles × N observaciones `planned`; las decisiones son idénticas bit a bit a las de `518f9c3`, con huella guardada en el test. Garantiza que I2/I3 solo actúan en `stretched` | ídem | — |
| 11 | *(solo si el paso 0 encuentra casos)* `RivalAttempt_ChaseStance_TowardsFirstBounce` y `RallySim_NoLegalWindow_RivalMoves`: el cuerpo recorre ≥ 0,30 m. RED: hoy 0 m | `RivalAttemptTests.cs`, `RallySimTests.cs` | I4 |
| 12 | `Hashes_Unchanged`: defaults `207da8f2c636`, RallyParams `a82c34b661bf` y SimulationKey `166a96e319c7`. Guardia | test de hashes existente o uno nuevo | — |
| 13 | Experimento «después» con las mismas semillas: R1 = R2 = R3 = 0, más R4–R7 reportadas | experimento | — |

Orden de commits: 0 → 1-2 → 3-4 → 5-7 → 8-10 → (11) → 12 → 13. Después, las regresiones completas de EditMode y PlayMode, AutoQA y check-release, y un informe de una página en `docs/reviews/`.

### 5.3 Riesgo para la calibración de los estilos

- **I1**: ninguno. Es un evento de vista y no cambia la simulación.
- **I2**: cambia decisiones solo en recepciones `stretched` desde la esquina de atrás con la dos paredes elegible. Puede mover un poco la cuenta de dos paredes y M4 (medido hoy en −0,2/+1,8/−0,0 pp). **Error de esta estimación:** en #97, la versión «sin dos paredes» bajó al Definidor a 1,47 % e invirtió el orden de M3, porque el Definidor juega casi todas sus dos paredes cuando llega estirado. El camino B la reemplaza y mide M3 como condición de fusión (F1).
- **I3**: cambia decisiones en recepciones `stretched` y la secuencia del RNG a partir de ellas. En promedio el rival devuelve pelotas más alcanzables. Eso debería **subir** un poco las victorias del humano, es decir, el rival se vuelve algo más fácil, en los nueve casilleros.
- **I4**: solo energía (más metros en puntos perdidos), con un efecto pequeño y en contra del rival.
- Desvíos abiertos que pueden moverse: Definidor Difícil 37,8 % frente al 20 % objetivo (I3 lo alejaría aún más, en la dirección de «más fácil»), M1b 39,2 % y el 6 % de B4 del cansancio.
- Mitigación: R7 mide el Δ de M1 por estilo y nivel con las mismas semillas, y el informe lo declara. Si algún Δ supera la tolerancia de la calibración del 19 nov (±4 pp), **no se recalibra en esta PR**: se registra y se para (AGENTS §2).

## 6. Propuesta de preset aparte (NO se implementa; decide Rafael)

Estas palancas son valores de `RallyParams`. Tocan la calibración de los estilos y, las que son compartidas, también G1 del humano. Se dejan con su diagnóstico y sin números objetivo; los valores concretos salen de una medición posterior con el experimento de §2.

| Id | Palanca | Diagnóstico que la motiva | Qué calibración toca |
|---|---|---|---|
| P1 | Espera más cerca de la T: `RivalWaitBehindT` 0,5 (Fondista), `StyleAllRoundWaitBehindT` 0,2 y `StyleShooterWaitBehindT` 0 | §1.6: los 0,5 m del Fondista cuestan ~0,1 s hacia adelante. La spec de estilos preveía al Definidor 0,3 m **delante** de la T, pero el código lo tiene en 0 | estilos (M1, M4, identidad de estilo); no G1 |
| P2 | Lectura del golpe: `PerceptionS` por nivel y `StyleNoiseK*` (que escala también la percepción, `RivalAi.cs:57`) | §1.1: el Fondista Fácil arranca 0,30 s después del golpe | estilos; `StyleNoiseK*` también mueve puntería, potencia y tiempo |
| P3 | `ReactionS` 0,18 | es compartido con el humano (`RallyDirector.cs:264`) | **G1** del humano y estilos: no tocar sin plan G1 |
| P4 | Llegar en frenada: que el plan del rival acepte el contacto con el cuerpo aún en movimiento, como la volea v2.3 «lunge» del humano (medida, no publicada) | §1.1, paso 3: hoy exige quedar quieto antes de T\*, lo que deja como `stretched`/`unreachable` pelotas que pasan a su alcance mientras frena | cambia el planificador compartido o crea uno del rival; estilos y, si es compartido, G1 |
| P5 | Globo defensivo desde adelante: una opción «globo» con altura y potencia propias que no pase por el tope de 22° | §1.4, punto 2: el «alto» con tope es un drive bajo | estilos; valores nuevos de geometría de opciones |
| P6 | Cobertura tras dejada propia: `Style*CoverDepth`, hoy todos −1 («a la T»); la spec del 19 nov preveía 1,5 m detrás del bote para el Definidor | la columna `from_wait` dirá si el rival suele estar fuera de su espera cuando el humano pega | estilos |

## Anexo A. Experimento `RivalShortBallDiagnosisExperiment` (listo para correr; no compilado)

Está escrito contra las API de `518f9c3` leídas para esta spec, pero **no se compiló** porque Unity no estaba disponible. Si no compila, Codex lo corrige sin cambiar el método de §2.1 y lo declara.

```csharp
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using NUnit.Framework;
using SquashClash.Core.Config;
using SquashClash.Core.Court;
using SquashClash.Core.Rally;
using SquashClash.Core.Shot;
using SquashClash.Core.Simulation;
using UnityEngine;

namespace SquashClash.Experiments.EditMode
{
    // Explicit diagnosis of the rival's answer to short balls (design docs/superpowers/specs/2026-11-22-rival-short-ball-design.md, section 2).
    // Read-only: defaults and RallyParams untouched. Not part of regression or AutoQA.
    public sealed class RivalShortBallDiagnosisExperiment
    {
        private static readonly CultureInfo C=CultureInfo.InvariantCulture;
        private static readonly UTF8Encoding Utf8=new UTF8Encoding(false);
        private const int Matches=150;private const long Seeds=20261122;

        // Human stand-in: 40 % drop, 15 % low straight, 45 % long like NewcomerBot. sigma 90 ms.
        public sealed class ShortMixBot : IRallyBrain
        {
            private readonly RallyContext ctx;private readonly RallyRng random;private readonly double sigma;
            public ShortMixBot(RallyContext ctx,double sigma,long seed){this.ctx=ctx;this.sigma=sigma;random=new RallyRng(seed);}
            public int PerceptionSteps=>0;
            public Vector2 StanceNoise()=>Vector2.zero;
            public AiShot Decide(in AiObservation o,in ReceptionPlan own)
            {
                if(!own.Reachable||!own.Contact.Exists||own.Contact.TStarStep<0)return default;
                double u=random.NextDouble();ShotLine line;ShotHeight height;ShotPower power;
                if(u<.40){line=ShotLine.Drop;height=(ShotHeight)Math.Min(2,(int)(random.NextDouble()*3));power=random.NextDouble()<.7?ShotPower.Medium:ShotPower.Strong;}
                else if(u<.55){line=ShotLine.Straight;height=ShotHeight.Low;power=random.NextDouble()<.5?ShotPower.Medium:ShotPower.Strong;}
                else{line=random.NextDouble()<.5?ShotLine.Straight:ShotLine.Cross;height=ShotHeight.Mid;power=random.NextDouble()<.6?ShotPower.Strong:ShotPower.Medium;}
                double delta=random.NextGaussian(sigma);
                var options=ShotOptions.Build(ctx,own.Contact.PStar);
                int D(ShotOption x)=>(x.Line==line?0:x.Line==ShotLine.Drop||line==ShotLine.Drop?8:4)+2*Math.Abs((int)x.Height-(int)height)+(x.Power==power?0:1);
                options.Sort((a,b)=>D(a).CompareTo(D(b)));
                foreach(var x in options)if(RallyContact.Forecast(ctx,own.Contact.PStar,x.Aim,x.P).Good)return new AiShot(true,x,x.Aim,x.P,delta,false);
                return new AiShot(true,options[0],options[0].Aim,options[0].P,delta,false);
            }
        }

        private sealed class Seen
        {
            public PlayerId Who;public BallState Ball;public ReceptionPlan Plan;public AiShot Shot;
            public double DelayMs,FromWait,PathM,ShortM;public Vector2 OpponentAtStart;
        }

        // Wraps a brain and keeps the rival's positioning and serve (RallySim reads them through the interfaces).
        private sealed class Recorder : IRallyBrain,IRallyPositioning,IRallyServer
        {
            private readonly IRallyBrain inner;private readonly PlayerId who;private readonly RallyContext ctx;public readonly List<Seen> Sink;
            public RallySim Sim;
            public Recorder(IRallyBrain inner,PlayerId who,RallyContext ctx,List<Seen> sink){this.inner=inner;this.who=who;this.ctx=ctx;Sink=sink;}
            public int PerceptionSteps=>inner.PerceptionSteps;
            public Vector2 StanceNoise()=>inner.StanceNoise();
            public Vector2 Wait=>inner is IRallyPositioning p?p.Wait:ctx.WaitPosition(who);
            public Vector2 Recovery(in AiObservation a)=>inner is IRallyPositioning p?p.Recovery(a):ctx.WaitPosition(who);
            public ServeType ChooseServe(ServiceBox box)=>inner is IRallyServer s?s.ChooseServe(box):ServeType.Lob;
            public AiShot Decide(in AiObservation o,in ReceptionPlan own)
            {
                var shot=inner.Decide(o,own);
                var s=new Seen{Who=who,Ball=o.Ball,Plan=own,Shot=shot,OpponentAtStart=o.OpponentPosition};
                if(who==PlayerId.Rival)
                {
                    var body=Sim.Rival;var energy=Sim.RivalEnergy.Energy;
                    int extra=FatigueEffects.For(ctx.Fatigue.Factor(energy),ctx.Fatigue).ExtraSteps;
                    s.DelayMs=(ctx.Steps(ctx.Params.ReactionS)+inner.PerceptionSteps+extra)*1000.0/ctx.StepHz;
                    s.FromWait=Vector2.Distance(body.Position,Wait);
                    // MoveTo already ran in BeginReception: this is the body's real path until its own stroke.
                    var t=ctx.Predict(o.Ball);var path=body.PositionsAfter(t.States.Count);long first=t.States[0].Step;
                    long b1=-1,b2=-1;foreach(var c in t.Contacts)if(c.Surface==SurfaceId.Floor){if(b1<0)b1=c.Step;else{b2=c.Step;break;}}
                    long end=shot.Swing&&own.Contact.Exists&&own.Contact.TStarStep>=0?own.Contact.TStarStep:(b2>=0?b2:first+t.States.Count-1);
                    double len=0;for(long j=first+1;j<=end&&j-first<path.Length;j++)len+=Vector2.Distance(path[j-first],path[j-first-1]);
                    s.PathM=len;
                    var frame=ctx.FrameAt(body.Position);double best=double.PositiveInfinity;
                    if(b1>=0)for(long j=b1;j<=(b2>=0?b2:end)&&j-first<path.Length;j++)
                    {var q=t.States[(int)(j-first)].Position-new Vector3(path[j-first].x,frame.ReferenceHeight,path[j-first].y);best=Math.Min(best,q.magnitude-frame.Reach);}
                    s.ShortM=best;
                }
                Sink.Add(s);return shot;
            }
        }

        private static string F(double v)=>double.IsNaN(v)||double.IsInfinity(v)?"NA":v.ToString("0.###",C);

        [Test,Explicit,Category("Experiments"),Timeout(14400000)]
        public void WriteShortBallDiagnosis()
        {
            string root=Path.GetFullPath(Path.Combine(Application.dataPath,".."));
            var preset=PresetCodec.LoadDefault(File.ReadAllText(Path.Combine(Application.dataPath,"SquashClash/Runtime/Resources/DefaultPreset.json")));
            var factory=new AnalyticBallSimulationFactory(UnityEngine.Physics.bounceThreshold);
            var ctx=new RallyContext(EffectiveConfig.From(preset),new RallyParams(),factory);
            string dir=Path.Combine(root,"Logs/experiments/rival-short-ball/"+(Environment.GetEnvironmentVariable("SC_RSB_RUN")??"run01"));
            Directory.CreateDirectory(dir);
            File.WriteAllText(Path.Combine(dir,"source.tsv"),"defaults\trallyParams\tunity\n"+preset.Hash+"\t"+ctx.Params.Hash+"\t"+Application.unityVersion+"\n",Utf8);
            float tz=ctx.T.y,tin=ctx.Court.TinTop;
            using(var w=new StreamWriter(Path.Combine(dir,"rows.tsv"),false,Utf8))
            {
                w.WriteLine("style\tlevel\tmatch\tpoint\tj\tkind\tzone\th_line\th_height\th_power\th_z\tfw_tin\tb1z\tb2z\tavail_ms\tapex2\treason\tslack\tdelay_ms\tfrom_wait\tpath_m\tshort_m\tswing\tresult\tdef\tline\theight\tpower\tvolley\tret_fw\tret_b2z\tnext\twon\tr_z");
                foreach(RivalStyleId style in new[]{RivalStyleId.Fondista,RivalStyleId.Definidor,RivalStyleId.Completo})
                foreach(AiLevel level in new[]{AiLevel.Easy,AiLevel.Medium,AiLevel.Hard})
                for(int m=0;m<Matches;m++)
                {
                    var seen=new List<Seen>();
                    var human=new Recorder(new ShortMixBot(ctx,90,Seeds+2*m),PlayerId.Human,ctx,seen);
                    var rival=new Recorder(RivalAi.For(ctx,level,style,Seeds+1+2*m),PlayerId.Rival,ctx,seen);
                    using(var sim=new RallySim(ctx,human,rival))
                    {
                        human.Sim=rival.Sim=sim;
                        // PlayMatch's own loop (RallySim.cs:145-157), point by point, so the recorder's sink is cleared per point:
                        // within a point, Decide k and log k belong to the same reception (RallySim.cs:213-248); a pending contact
                        // cut by the close leaves one Decide without a log, which only the last index can be.
                        var score=new MatchScore((int)ctx.Params.MatchTarget,(int)ctx.Params.MatchMargin,PlayerId.Human);int replays=0;
                        for(int pt=0;!score.Finished;pt++)
                        {
                            seen.Clear();var point=sim.PlayPoint(score.Server,score.Box);var logs=point.Receptions;var pointSeen=seen;
                            int used=pointSeen.Count;
                            for(int j=1;j<used;j++)
                            {
                                var s=pointSeen[j];if(s.Who!=PlayerId.Rival||j-1>=logs.Count||logs[j-1].Who!=PlayerId.Human)continue;
                                var hl=logs[j-1];var t=ctx.Predict(s.Ball);
                                float fw=float.NaN,b1z=float.NaN,b2z=float.NaN;long b1=-1,b2=-1;
                                foreach(var c in t.Contacts)
                                {
                                    if(c.Surface==SurfaceId.FrontWall&&float.IsNaN(fw))fw=c.Point.y;
                                    else if(c.Surface==SurfaceId.Floor){if(b1<0){b1=c.Step;b1z=c.Point.z;}else{b2=c.Step;b2z=c.Point.z;break;}}
                                }
                                double apex=double.NaN;if(b1>=0){apex=0;foreach(var st in t.States)if(st.Step>b1&&(b2<0||st.Step<b2))apex=Math.Max(apex,st.Position.y);}
                                long first=t.States[0].Step;double avail=b2>=0?(b2-first)*1000.0/ctx.StepHz:double.NaN;
                                string kind=hl.Line==ShotLine.Drop?"dejada":!float.IsNaN(b2z)&&b2z<tz?"corto":"largo";
                                string zone=hl.ContactPoint.z<tz?"adelante":"fondo";
                                string result=j<logs.Count?logs[j].Result.ToString():"none";
                                double retFw=double.NaN,retB2=double.NaN;string next="none";bool won=false;
                                if(j+1<pointSeen.Count&&result=="Contact")
                                {
                                    var rt=ctx.Predict(pointSeen[j+1].Ball);int floors=0;
                                    foreach(var c in rt.Contacts){if(c.Surface==SurfaceId.FrontWall&&double.IsNaN(retFw))retFw=c.Point.y;else if(c.Surface==SurfaceId.Floor&&++floors==2){retB2=c.Point.z;break;}}
                                    next=j+1<logs.Count?logs[j+1].Result.ToString():"none";
                                    won=point.Winner==PlayerId.Rival&&j+1==logs.Count-1;
                                }
                                var sh=s.Shot;
                                w.WriteLine(string.Join("\t",new[]{RivalStyle.Name(style),level.ToString(),m.ToString(C),pt.ToString(C),j.ToString(C),kind,zone,
                                    hl.Line.ToString(),hl.Height.ToString(),hl.Power.ToString(),F(hl.ContactPoint.z),F(fw-tin),F(b1z),F(b2z),F(avail),F(apex),
                                    s.Plan.Reason,F(s.Plan.SlackS),F(s.DelayMs),F(s.FromWait),F(s.PathM),F(s.ShortM),sh.Swing?"1":"0",result,
                                    sh.Defensive?"1":"0",sh.Swing?sh.Option.Line.ToString():"-",sh.Swing?sh.Option.Height.ToString():"-",sh.Swing?sh.Option.Power.ToString():"-",sh.Volley?"1":"0",
                                    F(retFw),F(retB2),next,won?"1":"0",
                                    result=="Contact"?F(logs[j].ContactPoint.z):"NaN"}));  // r_z (camino B, F2)
                            }
                            if(point.Winner.HasValue)score.PointWon(point.Winner.Value);
                            else if(++replays>50)break;
                        }
                    }
                }
            }
            Assert.Pass("rows written: "+dir);
        }
    }
}
```

Nota para Codex: el bucle replica `PlayMatch` (`RallySim.cs:145-157`) para limpiar los registros en cada punto. Dentro de un punto, el `Decide` k y el log k son la misma recepción (`RallySim.cs:213-248`). La única excepción es un contacto pendiente cortado por el cierre del punto, que deja el último `Decide` sin log y se escribe con `result = none`. Si en algún punto hay más de un `Decide` sin log, se cuenta y se reporta; no se descarta en silencio.

## Anexo B. Scripts fuera de Unity

**B.1 Cinemática de §1.6** (se corrió así; Python 3, sin dependencias):

```python
import math
Hz=120;A=12.0;V=4.8;B=16.0;eps=.01
def steps_to(p,t):
    x,z=p;vx=vz=0.0;n=0
    while n<Hz*30:
        dx,dz=t[0]-x,t[1]-z;d=math.hypot(dx,dz)
        if d<=eps: return n
        ux,uz=dx/d,dz/d;dt=1/Hz
        s=max(0,vx*ux+vz*uz);des=min(V,math.sqrt(2*B*d))
        s=min(des,s+A*dt) if s<des else max(des,s-B*dt)
        tr=s*dt
        if tr>=d: return n+1
        x+=ux*tr;z+=uz*tr;vx,vz=ux*s,uz*s;n+=1
T=(3.2,9.75-4.26-.025)
for name,wb in {'Fondista':.5,'Definidor':0,'Completo':.2}.items():
    w=(T[0]+.4,T[1]+wb)
    for bx in (0.6,1.6,3.2,4.8,5.8):
        print(name,bx,[round(min(steps_to(w,(min(max(bx+s,.35),6.05),z)) for s in (-.6,.6))/Hz,2) for z in (1.0,1.5,2.0,2.5,3.0,3.5)])
```

Diferencias con `PlayerBody`: Python usa doble precisión y Unity `float`, así que puede haber una diferencia de ±1 paso. La espera en el cálculo es la de §1.1 sin el ruido de apoyo.

**B.2 Resumen de `rows.tsv`** (lo escribe Codex junto con el experimento en `docs/validation/2026-11-22-rival-short-ball/summarize.py`). Agrupa por las claves de §2.2 y §2.3 con las franjas predeclaradas y escribe `cross.tsv` y `stretched.tsv`, con conteos y porcentajes sin redondear el n. No calcula Pass/Fail.
