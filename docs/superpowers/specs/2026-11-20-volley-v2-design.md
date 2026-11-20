# Volea v2: diseño y medición (fase 1)

20 nov 2026 · Subagente Opus dirigido por Claude (product lead); **la revisión de Claude no es independiente**. Rama `claude/volley-v2`, base `9a23e7d` (integración #84, sin fusionar). Solo diseño y medición: ningún cambio de producto. Parte de la spec de volea v1 (`2026-11-18-volley-design.md`).

**Encargo del PO (19 nov, tras jugar `d11f3d9`):** «no podía volear»; «La idea es que sea fácil al igual que los golpes»; «cuando se me ofrecía [...] era muy corto que no me di cuenta». Ampliación del PO: «diseña que sea un golpe lograble… Quizás agregarlo al saque tb».

## 0. Predeclaración (escrita y confirmada antes de medir la regla nueva)

**Método.** Experimento explícito `VolleyV2Experiment` (EditMode, `Category("Experiments")`, fuera de la regresión). `RallySim` con el `DefaultPreset.json` real (`2ec0c953786a`) y `RallyParams` sin cambios; humano = `BotBrain` novato (σ 90 ms, nunca volea en el juego real de la simulación) esperando en la T (`HumanWaitBehindT=0`); rival = `RivalAi.For(..., AiLevel.Medium, estilo, semilla)`, los tres estilos en partes iguales (el menú los sortea). 200 puntos por estilo, semillas fijas. La medición es **contrafactual**: en cada recepción del humano se recalcula la oportunidad con cada variante sobre la misma trayectoria, el mismo camino de cuerpo y el mismo plan de bote, y se juzga una suelta muestreada con el contrato real (`RallyContact.Resolve` / `ResolveAimedVolley`, que llaman a `ContactContract.Decide` sin cambios). El rally sigue siempre con el bote, así que el rival y los puntos no cambian entre variantes. Control de coherencia: la variante «hoy» debe reproducir exactamente `plan.Volley` de la simulación (existencia, K y T\*); si no, parada.

**Variantes (solo humano), fijadas antes de medir:**

| ID | Regla |
|---|---|
| V0 | Hoy: M1, altura 0,6–1,9, ventana ≥ 0,05 s sim, separación ≥ 0,25 s real, sin saque. |
| A0 | V0 sin filtro de separación (S sigue en el punto medio entre `KOut` de la volea y `KIn` del bote). |
| A1 | A0 con ventana mínima 0,03 s sim. |
| A0s | A0 también en la devolución del saque (medida aparte). |
| B | Volea elegida: el cuerpo va a una postura de intercepción (la de K más largo entre las alcanzables antes de T\*v, con el mismo planificador de contacto y `Bounces=0`); la oportunidad de bote se pierde. Solo se considera si A no cumple. |

**Metas (predeclaradas):**

1. **Oferta:** volea ofrecida en ≥ 25 % de las recepciones del humano que no son saque (Medio, humano en la T).
2. **Devolución del saque (medida aparte):** volea ofrecida en ≥ 25 % de las devoluciones de saque del humano. Meta propia, fijada aquí.
3. **Tan fácil como el bote:** el bot novato (σ 90 ms, toma toda volea ofrecida) logra Perfect+Valid en voleas a no más de 10 pp de su tasa en golpes de bote (mismas recepciones).
4. **Aviso legible:** anticipación del anillo de volea ≥ la del anillo de bote. Se mide como `min(500 ms, T*v − lanzamiento)` frente a `min(500 ms, T*b − lanzamiento)`, en tiempo real; la meta exige la mediana de la volea ≥ la del bote y ≥ 95 % de las voleas ofrecidas con 500 ms completos o tanto como el bote.

Regla de elección: se elige la primera de A0, A1 que cumpla 1, 3 y 4; si ninguna cumple 3, se evalúa B. No se ajusta ningún valor después de ver los datos.

## 1. Diagnóstico (medido después de confirmar §0)

Corrida única de `VolleyV2Experiment.MeasureVolleyV2` (Passed): 600 puntos, 5.116 recepciones del humano (4.778 sin saque, 338 devoluciones de saque), RallyParams `0f58580f44ea`, preset `207da8f2c636`, SimulationKey `166a96e319c7`. V0 reprodujo la oportunidad de la simulación en el 100 % (0 diferencias). Una `SystemFault` del rally de base, ajena a la medición (se registra, no se investigó). Evidencia: `docs/validation/evidence/2026-11-20-volley-v2/` (`rallysim-receptions.tsv`, SHA-256 `024e230f…b2f6`; `rallysim-summary.tsv`; `summarize.py`).

| Motivo hoy (V0) | Sin saque (simulación) | Juego real del PO (96) |
|---|---|---|
| separación | **45,9 %** | 42 % |
| no pasa al alcance | **39,6 %** | 27 % |
| ventana < 0,05 s | 11,6 % | 11 % |
| saque | (aparte: 100 % de 338) | 20 % |
| ofrecida | **2,4 %** | 0 % |

- **Separación, la causa principal y estructural.** El hueco real entre `KOut` de la volea y `KIn` del bote tiene mediana **130 ms** (p10 121, p90 143, máximo 247). Es casi constante porque es el tiempo que la pelota pasa por debajo de 0,6 m alrededor del bote. Con un umbral de 250 ms, la regla oculta casi toda volea que termina cerca del bote, es decir, casi todas.
- **No pasa al alcance (M1).** El cuerpo corre hacia el bote; la pelota pasa a una mediana de 0,88 m fuera del alcance (1,3 m). Solo 388 de 1.892 casos quedan a ≤ 0,3 m. Ampliar el alcance no lo arregla.
- **Ventana.** Con el cuerpo fijo en el camino M1, K es corto: mediana 170 ms reales en las ofrecidas hoy, frente a 475 ms del bote.
- **Aviso.** Con oferta, el anillo ámbar ya sale con los 500 ms completos (100 %). «Era muy corto» se explica por la ventana corta y por la rareza (2,4 %), no por la anticipación. Las 16 sueltas `before-k-in` del PO son coherentes con intentos de volea sin oferta.

## 2. Resultado de las variantes y propuesta

Medio, humano en la T, novato σ 90 ms. Bote del mismo novato en las mismas recepciones: **93,9 % Perfect+Valid** (98,2 % en devolución de saque).

| Variante | Oferta sin saque | P+V volea | Distancia al bote | K mediana | Anticipación ≥ bote | Oferta en saque |
|---|---|---|---|---|---|---|
| V0 hoy | 2,4 % | 52,6 % | 41 pp | 170 ms | 100 % | 0 % |
| A0 sin separación | **48,3 %** | 51,4 % | **42,5 pp ✗** | 138 ms | 100 % | 100 % (P+V 39 %) |
| A1 + ventana 0,03 | 54,0 % | 48,9 % | 45 pp ✗ | 130 ms | 100 % | 100 % |
| **B volea elegida** | **67,5 %** ✓ | **84,7 %** | **9,2 pp ✓** | 313 ms | 100 % ✓ | **100 % ✓** (P+V 79,6 %) |

Por estilo (B, sin saque): Fondista 78 % de oferta y 82 % de P+V; Definidor 57 % y 83 %; Completo 71 % y 89 %.

**Aplicación de la regla de §0:** A0 y A1 cumplen la oferta y el aviso, pero no la facilidad. Mientras el cuerpo siga el camino al bote, la volea es un tercio de ventana que el bote. Por eso se elige **B**, que cumple las cuatro metas. El margen de la meta 3 es estrecho: 9,2 pp frente a 10.

**Propuesta v2 (solo humano):**

1. **Oferta.** Si existe una postura de intercepción alcanzable antes de T\*v (la de K más largo, `Bounces=0`, altura 0,6–1,9), se ofrece la volea. No hay filtro de separación. La ventana mínima sigue en 0,05 s.
2. **Elección del jugador (D24).** Mientras el anillo ámbar está visible, el toque inicial del gesto M2-4 elige la volea. El cuerpo va entonces a la postura de intercepción (V3, W elegida) y se pierde el bote. Sin toque, el cuerpo sigue M1 y nada cambia. Al tocar se presenta un plan nuevo de volea, con su `CausalReference` preparada en ese instante, antes de la suelta. La intercepción se recalcula desde el cuerpo real en el momento del toque. Si ya no llega, no se compromete y sigue el bote.
3. **Aviso.**
   - El anillo ámbar sale al lanzar, con 500 ms completos.
   - Un toque háptico ligero (`Haptics.Impact`, el más suave) cuando se abre la volea.
   - El corte S pasa a ser el **plazo de compromiso**: el último instante en que el toque todavía llega a la intercepción. Así el anillo de bote aparece antes que con el punto medio. Con el punto medio en A0, la anticipación del anillo de bote bajaba a una mediana de 255 ms (p10 231) frente a los 500 de hoy.
4. **Devolución del saque (encargo del PO).** Se aplica la misma B (oferta 100 %, P+V 79,6 %). Toca las reglas del saque:
   - `PointRules.CanHit` hoy prohíbe el golpe mientras `Serving`;
   - hace falta dar el saque por bueno si el frontis fue bueno y el receptor volea antes del bote.
   Por la condición de parada del PO, va como **opción solo humana y solo DEV**, `HumanServeVolley=0` por defecto, y se señala. Ninguna escena AutoQA usa saques (SL1–SL6 y VL1 son de práctica).
5. **Sin cambios:**
   - un intento por recepción;
   - `ContactContract.Decide`;
   - `VolleyEnergyCost`;
   - la volea del saque para el rival;
   - el rival entero.

**Predicción frente a las metas:** oferta sin saque 67,5 % (≥ 25 %); devolución de saque 100 % (≥ 25 %); P+V 84,7 % frente a 93,9 % del bote (9,2 pp, ≤ 10); anticipación 500 ms en el 100 %. **Advertencia:** B se midió con el compromiso al inicio de la recepción, así que es una cota superior. Con el toque real más tarde, la oferta efectiva y P+V bajan. En la fase 2 se predeclara un punto de control que mide B con el compromiso retrasado 150 y 300 ms reales.

## 3. Impacto

- **Rival intacto:**
  - campos nuevos `HumanVolley*` en RallyParams: `HumanVolleyMode` (1 = v2), `HumanVolleyMinWindowS = 0,05` y `HumanServeVolley = 0`;
  - `RallyContext.VolleyFor` toma `who` y solo el humano lee esos campos.
  Pruebas de la fase 2:
  - test de identidad: las oportunidades del rival son iguales campo por campo con y sin los campos;
  - repetición de `RivalStylesCalibrationExperiment` con resultado idéntico bit a bit;
  - puntos de `RallySim` con bots que no volean, idénticos a la base.
- **Hashes y contrato:**
  - la proyección de RallyParams sin los campos `HumanVolley*` debe dar exactamente `0f58580f44ea`;
  - sin cambios en `DefaultPreset.json` (`207da8f2c636`), SimulationKey `166a96e319c7` ni los bytes de `ContactContract.cs`;
  - toca `OpportunityContract` (S = plazo de compromiso), `VolleyPlanner` (intercepción), `ShotController`, `RallyDirector` (mover el cuerpo al tocar), `TimingRingView` y el háptico. Son la zona del contrato, bajo el modo product lead del 18 nov.
- **AutoQA:** la práctica sigue con las reglas v1 compartidas y el cuerpo fijo. VL1 (`24531a50618a`), su oráculo `volley-window/1` y las suites V/T3/volea no cambian. Si algún paso obligara a cambiarlos: parada.

## 4. Preguntas abiertas

1. ¿El toque inicial como elección de la volea es aceptable? Riesgo: quien toca temprano para el bote compromete una volea sin querer. Alternativa: elegir con un toque y soltar dentro del anillo ámbar.
2. ¿La devolución del saque como opción DEV basta para esta tanda, o el PO quiere el cambio de reglas en serio?
3. **Notas para el encargo del saque, sin trabajar:** todas las devoluciones de saque (338/338) pasan al alcance antes del bote y el bote del saque es fácil (P+V 98,2 %, K mediana 393 ms). El saque es blando y alto.

## 5. Fase 2: decisiones del product lead y controles predeclarados (antes de medir)

**Decisiones (20 nov, Claude como product lead, a partir de la respuesta a §4):**
- Se aprueba B.
- **Regla que aprende el jugador:** «Anillo ámbar visible: toca y voleas. Espera al anillo blanco: golpeas después del bote.» El paso 4 del tutorial y la línea de novedades dicen exactamente eso.
- **Ventana de elección:** desde que el anillo ámbar se ve, `max(lanzamiento, T*v − 500 ms)`, hasta el plazo de compromiso S.
  - Un toque dentro de esa ventana, con la intercepción todavía alcanzable desde el cuerpo real, compromete la volea.
  - Cualquier otro toque descarta la volea de esa recepción y el golpe va contra el bote.
- **Volea de la devolución del saque:** solo para el humano, activada por defecto en `RallyParams` y con un botón DEV para apagarla.
  - Requisito: ningún test, escenario AutoQA, oráculo ni ancla existente cambia de expectativa. Si alguno tuviera que cambiar, queda apagada por defecto y se señala.
  - Las reglas del saque del rival no cambian.
- **Háptico:** el impacto más suave (`Discreet`) cuando el anillo ámbar se hace visible.
- **Botón DEV:** «Volea v2 sí/no» para comparar con v1.

**Controles (`VolleyV2CommitExperiment`, mismas semillas, partidos y novato que §0):**
1. **Toque fijo:** el toque llega 150 ms y 300 ms reales después de que el anillo ámbar se hace visible.
2. **Toque del PO repetido:** el toque llega `T*b − d` antes del bote. `d` se muestrea de la distribución medida del PO (92 golpes, build `d11f3d9`).
   - Deciles: 477, 578, 684, 758, 850, 940, 1051, 1130 y 1383 ms.
   - Se interpola linealmente entre deciles; las colas (< 10 % y > 90 %) toman el decil extremo.
   - Si el toque cae antes de que el ámbar se vea o después de S, no hay volea y se juega el bote.
3. **En cada caso se informa:**
   - **oferta efectiva:** el toque compromete la volea;
   - **tasa de volea:** voleas tomadas sobre las recepciones sin saque;
   - **P+V de la volea:** suelta N(0, 90 ms) en torno a T\*v del plan comprometido, recalculado desde el cuerpo en el instante del toque;
   - **distancia frente al P+V del bote** de las mismas recepciones.
4. **Parada:** si en el toque del PO la distancia de P+V frente al bote supera **15 pp**, se registra y se detiene, sin reajustar nada.

**Corrección antes de medir los controles (20 nov, durante el GREEN):**
- **Qué estaba mal.** §5 suponía que el anillo ámbar se ve desde `T*v − 500 ms`. En el código no es así: `ImpactCue` muestra el anillo desde que se presenta, al lanzar, con su radio exterior, y los 500 ms son solo el cierre.
- **Qué fallaba.** Con la ventana mal definida, el primer GREEN dio `no-intercept` en el saque neutro y en el saque: el cuerpo ya no llega a ninguna postura si sale recién 500 ms antes.
- **Corrección.** La ventana de elección es **[lanzamiento, S]**: el ámbar se ve desde el lanzamiento, que es exactamente la regla «anillo ámbar visible».
- **Efecto en los controles.** El toque fijo cae 150 y 300 ms después del lanzamiento. En el toque del PO, un toque anterior al lanzamiento no compromete nada.
- **Lo que no cambia:** las metas, la parada de 15 pp y la distribución del PO.
