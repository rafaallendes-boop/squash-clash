# Calidad de la defensa del rival en el drive al fondo: medición

27 nov 2026 · Claude (Opus: arquitectura, declaración, medición y revisión; la instrumentación la implementa un subagente Sonnet dirigido por Claude, así que la revisión **no es independiente**) · encargo `set19-calidad-defensa` de SC-PM. Es solo una medición: rama local `rafaallendes-boop/sc-calidad-defensa`, sin PR, sin merge y sin push.

Contexto: [2026-11-27-posicion-espera.md](2026-11-27-posicion-espera.md). Con b2b (el rival se recupera 1,6 m hacia el lado de su pelota profunda) el rival llega más al drive al fondo, pero pierde el punto igual: su defensa le deja al humano ~0,9 s de holgura.

## Declaración (antes de medir)

### Base

- Rama `sc-posicion-espera` en `b7821d2` (defaults neutros), con los valores de b2b de `5b10a9f` aplicados en `34c7526` (`RivalRecoverySideShift` = 1,6). El código de producto queda idéntico al de `5b10a9f`. RallyParams esperado `5ef0c8f8b353`, el de la corrida b2b.
- Este encargo **solo mide**. No cambia la elección de golpe ni nada del rival ni del humano. El rival sigue leyendo solo la pelota.
- La instrumentación va solo en el arnés (`DeepDefenseExperiment`, ensamblado de experimentos) y en un resumidor nuevo. No se toca código de producto, banda, umbrales, tolerancias, oráculos ni pruebas existentes.

### Batería

- La misma de `sc-posicion-espera`, solo métricas: Medio, los tres estilos, `IntentBot` σ 30 ms, 100 partidos por celda, semillas `20261126`, las cuatro intenciones. Sin N2, porque el comportamiento no cambia.
- **Control de la instrumentación:** `cells.tsv` y `rival-time.tsv` de esta corrida tienen que dar igual que `metrics-b2b.tsv` y `rival-time-b2b.tsv` de `sc-posicion-espera`, en todas las columnas salvo la del tiempo de reloj (`seconds`). Si hay una sola diferencia, me detengo y no informo la tabla.
- Unity: candado `/Users/rafaallendes/sc-unity.lock`, un solo Unity, una corrida, sin reintentos. Una falla de entorno no cuenta como verde.

### (1) Calidad de la defensa

**Población.** Las recepciones del rival en las celdas `fondo-max` (P = 1) y `fondo-085` (P = 0,85) que siguen a un drive al fondo del humano (golpe del humano con contacto, Recto, Medio y Fuerte, la misma marca `drive` de `rival-time.tsv`) y en las que el rival **llega**: le pega (resultado distinto de `Unreachable` y `NoSwing`). Solo puntos bien alineados (sin mismatch), como en el arnés.

**Qué se registra en cada una:**

- **Resultado:** contacto aplicado o falla (`Air`, `Late`, `SystemFault`, `NotEvaluable`).
- **Golpe elegido**, de las clases del propio rival:
  - boast: línea `Boast`;
  - drop: línea `Drop`;
  - globo: altura `High` (recto o cruzado);
  - drive recto: `Straight` con altura `Mid` o `Low`;
  - cruzado: `Cross` con altura `Mid` o `Low`;
  - otro: lo que no entre arriba (no espero ninguno).
  - Aparte se registran la potencia, si fue de volea y si el rival lo marcó como defensivo (holgura de su plan menor que la de su estilo: 0,12 / 0,06 / 0,12 s). Cuando es defensivo, hoy el rival solo puede elegir el globo medio o el boast.
- **Bote de su pelota**, con la misma predicción P2 desde la pelota recién golpeada (lo que ve el humano):
  - primer bote tras la pared frontal: z y x; distancia a la lateral más cercana, d = mín(x, 6,40 − x);
  - si toca la pared del fondo antes del primer bote (en vuelo) o entre el primero y el segundo;
  - la distancia a la lateral en el punto donde le pega el humano, si le pega.
- **Clases del bote (fijadas ahora):**
  - largo: corto si el primer bote cae delante del T (z < 5,465); fondo en vuelo si toca el fondo antes de botar; largo si bota detrás del T y llega al fondo antes del segundo bote; medio si bota detrás del T y no llega al fondo;
  - pegado: d ≤ 0,5 m al primer bote; intermedio de 0,5 a 1,0 m; suelto > 1,0 m.
- **Holgura del humano** sobre esa pelota: la del plan del humano (la misma de `humanSlackMedianS`).
- **El punto:** % de esas defensas tras las que el rival pierde el punto, y % en las que lo pierde **en el acto** (el punto lo gana el humano y termina a más tardar en la recepción siguiente del rival).
- **Con qué margen le pegó el rival:**
  - holgura de su plan (s) y si el plan fue `stretched` (llega estirado, holgura 0);
  - margen de alcance: alcance del cuerpo menos la distancia del cuerpo (posición al contacto, altura de referencia) al punto de contacto P*, en metros;
  - ancho de la ventana de contacto (KOut − KIn) y margen de la suelta dentro de ella: mín(δ − inferior, superior − δ), en ms reales; en una volea, los del plan de volea.
- «Llegó justo»: holgura de su plan menor que la de defensa de su estilo, o plan `stretched`.

**Salida:** tabla por estilo (y por P = 1 y P = 0,85) con el golpe elegido (%), el bote (corto/medio/largo/fondo en vuelo y pegado/intermedio/suelto), la holgura mediana del humano, el % de puntos que pierde el rival (total y en el acto) y el margen con que llegó (holgura mediana, % justo, % estirado, margen de alcance y de suelta). Además, el cruce por golpe y por bote del % de puntos perdidos y de la holgura del humano, y la comparación justo frente a holgado.

**Cómo leo la hipótesis de SC-Lead** («cuando llegas justo a un drive largo, lo sano es un globo alto al rincón o un drive recto pegado a la pared; si el rival elige un golpe con una ventana chica, o le pega con poca holgura y sale corto, ahí está el problema»). La separo en tres preguntas que se contestan con la tabla, sin umbral de producto:

1. ¿Qué elige el rival cuando llega justo? ¿Globo o drive recto, o boast y drop?
2. ¿Los golpes sanos (globo que bota largo, o drive recto largo y pegado) le dejan al humano menos holgura y le hacen perder menos puntos que el resto? Si no hay diferencia, la elección de golpe no es la palanca.
3. ¿Cuando llega justo, su pelota sale más corta o más suelta que cuando llega holgado, o con menos margen de suelta?

La hipótesis se confirma si (2) muestra que los golpes sanos rinden mejor **y** (1) o (3) muestran que el rival no los consigue cuando llega justo. Ruido: con n defensas en una celda, el error estándar de un % ronda √(p(1−p)/n); con n ≈ 300 y p ≈ 0,9 da ~1,7 pp, así que solo leo diferencias de más de ~5 pp.

### (2) Diagnóstico de las recepciones que siguen sin llegar

**Población.** Las filas de `rival-time.tsv` con `drive = 1` (el rival no llega, `Unreachable` o `NoSwing`, tras un drive al fondo del humano), en `fondo-max` y `fondo-085`.

**Qué se registra de la pelota anterior del rival** (la que el rival golpeó antes de ese drive):

- origen: el saque del rival, o un golpe del rival;
- su primer bote, tal como lo lee `RivalAi.Recovery` (primer contacto con el suelo de la predicción desde la pelota recién golpeada): z y x;
- el objetivo de recuperación que devolvió `Recovery` y la espera del estilo;
- el primer bote del drive del humano (x, z), para saber de qué lado baja;
- la posición del cuerpo del rival al golpe del humano, su distancia al objetivo y su velocidad;
- el déficit con el cuerpo tal como estaba (`lateBaseS`, el mismo de `rival-time.tsv`) y dos contrafactuales con el mismo código de `Deficit`, sin tocar la simulación: el cuerpo quieto en el objetivo de recuperación, y el cuerpo quieto a 1,6 m del T **del lado del drive**, a la profundidad de su espera.

**Clases (excluyentes, en este orden):**

1. No se aplicó: la pelota anterior fue el saque (la espera del saque no cambia en b2).
2. No se aplicó: la pelota anterior botó corta (z < 3,5), así que volvió al T.
3. Se aplicó al lado contrario del drive.
4. Se aplicó del lado del drive y el rival no había llegado al objetivo: a más de 0,25 m cuando golpea el humano.
5. Se aplicó del lado del drive, el rival estaba en el objetivo (≤ 0,25 m) y no alcanza.

Por estilo: cuántas hay en cada clase y la mediana del déficit real y de los dos contrafactuales. Si el contrafactual en el objetivo sigue tarde, el desplazamiento no alcanza aunque se hubiera aplicado del todo.

### Propuestas

Si los datos sugieren una palanca, va como propuesta con valores (máximo 2), **sin implementarla**.

## Resultados

Una corrida, `q01`, el 27 nov a las 12:48–12:52 Chile, con head `3b8cbd8` (código de producto igual a `5b10a9f`). Evidencia en [`docs/validation/evidence/2026-11-27-calidad-defensa/`](../validation/evidence/2026-11-27-calidad-defensa/): `metrics-q01.tsv` con su `-source.tsv` y su `-head.txt`, `defense-q01.tsv.gz`, `prev-ball-q01.tsv`, `defense-cells-q01.tsv` y `defense-quality-summary.md` (la salida de `scripts/summarize-defense-quality.py q01`).

**Control de la instrumentación: en verde.**
- RallyParams es `5ef0c8f8b353`, el de la corrida b2b.
- `cells.tsv` coincide con `metrics-b2b.tsv` en las 12 celdas y en todas las columnas salvo `seconds`: 0 diferencias.
- `rival-time.tsv` es idéntico byte a byte a `rival-time-b2b.tsv` (SHA-256 `5f25a6f8…`).
- Hay 0 puntos desalineados en las salidas nuevas.

**Cómo leer «% que pierde».** Después de una defensa a la que llega, el rival pierde el punto el 98–100 % de las veces, sea cual sea el golpe. Es casi una tautología: el bot repite el drive hasta que el rival falla, así que una buena defensa sola no gana el punto. Lo que distingue una defensa de otra es la **holgura que le deja al humano** y el **% que pierde en el acto** (el punto termina en su recepción siguiente).

### Tabla por estilo (P = 1 / P = 0,85)

| | Fondista | Definidor | Completo |
|---|---|---|---|
| Defensas a las que llega (n) | 2 278 / 6 195 | 3 163 / 8 465 | 2 246 / 7 118 |
| Falla el golpe, % | 13,8 / 10,3 | 8,4 / 6,9 | 13,1 / 8,5 |
| **Golpe: globo, %** | **90,4 / 89,0** | **96,1 / 83,9** | **86,6 / 85,4** |
| Golpe: boast, % | 8,5 / 8,4 | 3,4 / 14,8 | 12,7 / 13,5 |
| Golpe: drive recto + cruzado + drop, % | 1,1 / 2,7 | 0,5 / 1,2 | 0,8 / 1,2 |
| De volea, % | 0,4 / 0,0 | 67,6 / 3,8 | 15,6 / 0,0 |
| **Bota corto (delante del T), %** | **57,5 / 49,8** | **38,7 / 73,6** | **60,6 / 59,5** |
| Bota largo (llega al fondo), % | 24,8 / 28,7 | 28,2 / 9,5 | 17,7 / 18,8 |
| Suelto (> 1 m de la lateral), % | 80,5 / 76,9 | 86,6 / 72,2 | 81,1 / 72,6 |
| Distancia mediana a la lateral (m) | 2,27 / 2,22 | 2,42 / 1,94 | 2,20 / 1,91 |
| **Holgura mediana del humano (s)** | **0,87 / 0,79** | **0,68 / 0,88** | **0,87 / 0,83** |
| Pierde el punto, % | 99,3 / 99,0 | 99,9 / 99,4 | 99,7 / 98,6 |
| Pierde en el acto, % | 39,7 / 20,5 | 21,4 / 14,6 | 36,1 / 17,6 |
| Llegó justo, % (estirado, %) | 97,6 (95,5) / 95,6 (18,7) | 99,3 (98,2) / 98,4 (77,5) | 98,8 (96,8) / 98,3 (51,3) |
| Margen de alcance mediano, sin voleas (m) | 0,03 / 0,12 | 0,02 / 0,04 | 0,03 / 0,03 |
| Ventana / margen de suelta medianos (ms) | 215 / 61 · 324 / 86 | 186 / 52 · 279 / 56 | 193 / 50 · 279 / 74 |

### Lo que dicen los datos

- **El rival elige el golpe sano: el globo.** Lo elige en el 84–96 % de las defensas, y casi todas las demás son boast. Llega justo en el 96–99 % y, cuando llega justo, la regla de defensa ya le deja solo el globo medio o el boast. Por eso el drive recto casi no aparece (0,3–1,1 %).
- **El problema es que el globo sale corto y suelto.** El 52–63 % de los globos bota delante del T y casi ninguno va pegado: el 6 % de todos, y el 15 % de los rectos. El 56–60 % son cruzados.
  - Un globo corto le deja al humano 0,94 s; uno largo, 0,46–0,52 s.
  - Además, el 16–20 % de los globos bota a menos de 3,5 m de la frontal, igual que el 37–50 % de los boasts.
- **Los golpes sanos rinden mejor, pero el rival casi no los consigue.** Llamo «sanos» al globo que llega al fondo y al drive recto largo y pegado.
  - Le dejan al humano 0,46–0,52 s, frente a 0,89 s el resto.
  - El rival pierde en el acto menos con ellos en el Fondista (4,9 frente a 19,8 %) y en el Completo (6,5 frente a 15,2 %); en el Definidor, igual (9,4 frente a 9,9 %).
  - Son solo el 13–24 % de sus contactos.
- **Justo frente a holgado.** Cuando llega justo, su globo sale corto más seguido que cuando llega holgado (52–63 % frente a 6–49 %) y suelta con menos margen (58–83 ms frente a 88–126 ms). El grupo holgado es chico: 36–97 globos por estilo.
- **Los pocos globos fuertes van largos.** Hay 64, todos en jugadas no defensivas: ninguno bota corto, el 55 % llega al fondo y le dejan al humano 0,41 s. De los globos de potencia media (23 301, el 99,5 % defensivos), el 58 % bota corto. La comparación está confundida, porque los fuertes salen todos de jugadas holgadas.

**La hipótesis de SC-Lead se confirma a medias.**
- Se confirma que lo sano rinde mejor y que, cuando llega justo, su pelota sale corta.
- No se confirma que elija mal: elige globo casi siempre, y la ventana o la suelta no explican el problema (falla el golpe solo en el 7–14 % de las defensas).
- **El problema es el largo del globo defensivo, no qué golpe elige.**

### Diagnóstico de las recepciones que siguen sin llegar

| | Fondista | Definidor | Completo |
|---|---|---|---|
| Recepciones que no llegan tras un drive (P = 1 / P = 0,85) | 385 / 56 | 418 / 114 | 406 / 99 |
| Su pelota anterior botó corta (< 3,5 m), así que no se aplicó el desplazamiento | 100 % / 100 % | 98,8 % / 100 % | 100 % / 100 % |
| Se aplicó al lado contrario | 0 | 5 (1,2 %, P = 1) | 0 |
| Saque / se aplicó y no llegó / se aplicó, llegó y no alcanza | 0 / 0 / 0 | 0 / 0 / 0 | 0 / 0 / 0 |
| Bote mediano de esa pelota anterior (m de la frontal) | 2,06 | 0,92 | 1,90 |
| Déficit mediano real (s) | 0,61 / 0,35 | 0,62 / 0,28 | 0,60 / 0,35 |
| Déficit quieto a 1,6 m del lado del drive (s) | 0,43 / 0,12 | 0,56 / 0,14 | 0,49 / 0,18 |

- **El desplazamiento de b2 no se aplicó.** Cuando el rival no llega, casi siempre su pelota anterior fue un boast o un globo que murió adelante. Así lo dicta la regla de b2 (solo se aplica tras una pelota profunda): el rival volvió a cubrir el T.
- **Al golpe del humano, el rival estaba quieto en ese punto** (distancia mediana al objetivo 0, velocidad 0). No es que venga en camino.
- **Aunque se aplicara, no alcanza con P = 1.** Si el rival hubiera estado quieto a 1,6 m del T del lado del drive, seguiría llegando 0,43–0,56 s tarde. Con P = 0,85 quedaría a 0,12–0,18 s.

### Propuestas (máximo 2, sin implementar)

1. **Globo defensivo con potencia fuerte.** Hoy, cuando el rival llega justo, el conjunto permitido es Alto + Medio (o boast). La propuesta es agregar Alto + Fuerte (recto y cruzado) y que la utilidad elija entre los dos, sin otro cambio. La apoyan los 64 globos fuertes: ninguno corto y 0,41 s de holgura para el humano, aunque están confundidos por la holgura. Lee solo la pelota y su propio plan. Medirlo pide N2, porque cambia la elección de golpe.
2. **Corrimiento lateral también tras su pelota corta.** Es b2b (1,6 m) aplicado a toda pelota propia, no solo a las que botan desde 3,5 m. Según el contrafactual, recorta el déficit mediano en 0,06–0,23 s, pero con P = 1 seguiría 0,43–0,56 s tarde. Tiene un riesgo: tras su pelota corta el humano juega desde adelante, y dejar el T libre puede abrir el drop. Lo dejo como cota, no como recomendación.

### Preguntas para Rafael

1. ¿Medimos la propuesta 1 (globo defensivo fuerte), con la batería de métricas y N2?
2. ¿La propuesta 2 se descarta, dado que el contrafactual ya dice que con P = 1 no alcanza?

### Riesgos y lo que elegí yo

- **Margen de alcance en las voleas.** El arnés lo calcula con la postura del plan de bote, que no sirve para una volea (da unos −1,8 m). Por eso en la tabla va sin voleas, y eso pesa sobre todo en el Definidor con P = 1, que volea el 67,6 %. La ventana y el margen de suelta sí usan el plan de volea.
- **Un cruce que descarté.** Intenté cruzar cada recepción sin llegar con la defensa anterior del rival usando el número de partido. No es único (hay varios puntos por partido), así que no lo usé. Lo que informo del golpe anterior sale del bote registrado en `prev-ball` (mediana 0,9–2,1 m), no de ese cruce.
- **Instrumentación sin pruebas unitarias.** Solo toca el arnés. La verificación es el control de 0 diferencias.
- **Revisión no independiente.** La implementó un subagente Sonnet dirigido por mí. En mi revisión encontré un `else` que quedaba colgando del bloque nuevo y habría cambiado `cells.tsv`, y se corrigió antes de medir (`3b8cbd8`). Falta la revisión cruzada de Codex.
- **Hashes.** RallyParams `5ef0c8f8b353`, el de b2b, frente a `ebf8f717f8c5` en `main`. Las 7 pruebas que anclan el hash fallan en esta rama, como en `sc-posicion-espera`; no las corrí ni las toqué. Defaults `207da8f2c636` y SimulationKey sin cambios.
- **Lo que faltó:** nada del encargo. Sin N2, como decía el encargo.
