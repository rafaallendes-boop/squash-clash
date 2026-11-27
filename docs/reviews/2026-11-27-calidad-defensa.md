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

Pendiente.
