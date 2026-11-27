# El rival lee el swing del humano: experimento

27 nov 2026 · Claude (Opus: arquitectura, declaración, medición y revisión; el código lo implementa un subagente Sonnet dirigido por Claude, así que la revisión **no es independiente**) · encargo `set21-lectura-swing` de SC-PM. Es solo un experimento: rama local `rafaallendes-boop/sc-lectura-swing`, sin PR, sin merge y sin push.

**Autorización.** Rafael (PO), vía SC-Lead, 27 nov 14:10: «Sí, midan como experimento, sin merge, que el rival lea mi swing para salir antes». Es una **excepción solo para este experimento** a la regla vigente «el rival lee solo la pelota». La regla no cambia en `main`, y adoptarla le toca a Rafael.

Contexto: [2026-11-27-calidad-defensa.md](2026-11-27-calidad-defensa.md) y [2026-11-27-globo-fuerte.md](2026-11-27-globo-fuerte.md) (rama `sc-globo-fuerte`). Con b2b el rival sigue sin llegar al drive al fondo con P = 1: aun quieto a 1,6 m del T del lado del drive, llega 0,43–0,56 s tarde.

## Declaración (antes de medir)

### Base

- Rama `sc-calidad-defensa` en `1fb50c7`: valores de b2b (`RivalRecoverySideShift` = 1,6, RallyParams `5ef0c8f8b353`) y su instrumentación (`defense.tsv`, `prev-ball.tsv`). Los campos del globo fuerte de `sc-globo-fuerte` **no** van.

### Qué hace la variante (solo el rival; el humano no cambia)

Dos campos nuevos de RallyParams, **neutros (0) por defecto**:
- `RivalSwingReadLeadMs` (ms reales, ≥ 0). 0 = no hay lectura, el comportamiento de hoy.
- `RivalSwingMisreadP` (0 a 1). La probabilidad de leer mal. Solo cuenta si la lectura está encendida.

Con `RivalSwingReadLeadMs` = X > 0, en cada golpe del humano en el rally (de bote o de volea; el saque es automático y no tiene swing, así que no se lee):
1. **Cuándo sale.** X ms reales antes del paso de contacto del humano: `round(X / 1000 · tempo · stepHz)` pasos (tempo 0,65 y 120 Hz: 8 pasos con X = 100 y 16 con X = 200). Si el golpe llega antes (el humano le pega menos de X ms después del golpe del rival), sale en el mismo paso en que empieza la recepción del humano, y se registra la anticipación real.
2. **Qué lee.** La intención del golpe del humano (dirección y largo: línea, altura, potencia y P). No lee el error de timing ni la pelota que de verdad saldrá.
3. **Adónde va.** Calcula la pelota ideal de esa intención desde el punto de contacto (`SolveIntention`, o `SolveBoastIntention` en un boast, y la predicción P2) y la misma planificación de recepción del rival (`ReceptionPlanner.Plan`, con el cuerpo en ese paso, sin retardo ni ruido de postura y contando el tiempo que falta hasta el contacto). Corre hacia esa postura desde ya, sin reacción: la reacción ya la pagó leyendo el swing.
4. **El error.** Con probabilidad p lee mal, y la mitad de las veces es la dirección y la otra mitad, el largo:
   - **dirección:** corre a la postura espejada respecto del centro de la cancha (x → 6,40 − x, dentro de la cancha), o sea, hacia el lado equivocado;
   - **largo:** si el golpe es largo (recto o cruzado con altura media o alta), lo lee como el drop (bajo, medio); si es corto (drop, bajo o boast), lo lee como el drive recto medio fuerte. Planifica como en el punto 3.
   - Las dos monedas salen de un generador propio con semilla en el paso y el punto de contacto. No consume los números de ningún cerebro, así que con la lectura apagada las semillas dan lo mismo que hoy.
5. **Corrige.** Al contacto real, la recepción del rival empieza como hoy: lee la pelota real, planifica y, tras su reacción y percepción, gira hacia la postura nueva. Durante la reacción sigue corriendo hacia donde había leído. Si leyó mal, ese tramo le juega en contra.

- No toca la elección de golpe del rival, el lado humano, `FondoAi`, la banda, los umbrales, los criterios, los oráculos, las pruebas existentes, `DefaultPreset.json` ni SimulationKey.
- Se implementa solo en `RallySim` (simulación sin escena), que es lo que usan la batería y N2. El juego real (`RallyDirector`) no lo lee. Si Rafael la adopta, falta llevarla al juego.
- Los dos campos cambian el hash de RallyParams aunque estén neutros. Las 12 pruebas que ya fallan en la base `1fb50c7` (anclas de hash y defaults de b2b) siguen fallando; con los valores de las variantes como defaults puede fallar alguna más que fije el rally. No las toco y lo informo.
- Pruebas nuevas (TDD, RED válido por aserción y luego GREEN): con los campos neutros no hay ninguna lectura; con X > 0 y p = 0, cada golpe del humano da una lectura acertada, con la anticipación declarada, y el rival sale hacia la postura leída; con p = 1, todas las lecturas son erradas, de los dos tipos, y la de dirección es el espejo de la acertada; con la misma semilla, las mismas lecturas.

### Valores (2) y por qué

| Variante | X (ms reales) | p |
|---|---|---|
| **s1** | 100 | 0,25 |
| **s2** | 200 | 0,25 |

- **X.** Son los dos extremos del rango de ejemplo del encargo (100 a 200 ms). Con la misma p, la diferencia entre s1 y s2 mide solo cuánto pesa anticipar más. Como referencia, la reacción del rival es 0,18 s de simulación (≈ 277 ms reales) y su percepción en Medio, 0,04 s (≈ 62 ms). Si lee bien, sale X + ~340 ms reales antes que hoy (≈ 0,07 + 0,22 s de simulación con X = 100, y 0,13 + 0,22 s con X = 200). Es del orden del déficit que medimos con P = 1 (0,43–0,56 s quieto del lado del drive), así que las dos variantes deberían mover algo.
- **p = 0,25.** El rival acierta 3 de cada 4. Es lo bastante alto para que el engaño valga (una de cada cuatro veces lo saca de posición, como dice SC-Lead que pasa con un rival real), y lo bastante bajo para que leer el swing le convenga en promedio. No tengo un dato de squash para p: es un supuesto, no un valor calibrado. Si s2 sale de la banda, la p de un siguiente experimento se discute, no se ajusta aquí.

Cada variante es un commit con sus valores como defaults, porque `ConfirmPerStyle` usa `new RallyParams()`. Cada corrida guarda su `head.txt`. Al final, los dos campos vuelven a neutros (b2b sigue aplicado, como en la base).

### Batería

- **Métricas:** la de `sc-calidad-defensa` y `sc-globo-fuerte`. Medio, los tres estilos, `IntentBot` σ 30 ms, 100 partidos por celda, semillas `20261126`, las cuatro intenciones (`fondo-max`, `fondo-085`, `medio-suave`, `corto-drop`).
- **Instrumentación nueva, solo en el arnés:** un archivo aparte, `swing-read.tsv`, con una fila por lectura en las cuatro intenciones: el golpe del humano, el tipo de lectura (acertada, dirección o largo), la anticipación real, la postura leída, el resultado de la recepción siguiente del rival, su holgura y su motivo de plan, y el punto (ganador y si el rival lo pierde en el acto, con la misma definición de `defense.tsv`). Los archivos de hoy no cambian.
- **ANTES = b2b.** Una corrida nueva (`ls00`, el equivalente del `gf00` de `sc-globo-fuerte`) con los dos campos neutros y el arnés nuevo. **Control:** `cells.tsv` (salvo `seconds`), `rival-time.tsv`, `prev-ball.tsv` y `defense.tsv` tienen que dar igual que en `q01` de `sc-calidad-defensa`, con `scripts/compare-control.py` de `sc-globo-fuerte`, y `swing-read.tsv` tiene que estar vacío. Si hay una sola diferencia, me detengo y no informo la tabla. El hash de RallyParams sí cambia (campos nuevos) y lo anoto.
- **N2:** `RivalStylesCalibrationExperiment.ConfirmPerStyle` sin cambios: semillas de confirmación `20261219`, 200 partidos por celda, novato σ 90 ms y `bot30`, 3 estilos × 3 niveles. Se informa el Δ del novato por celda frente a dos referencias:
  - la base de `main` (`n2-before01-from-6086074.tsv`);
  - b2b (`n2-b2b.tsv`), para separar el efecto de la lectura.
- **Banda vigente, cita textual** de [2026-11-20-serve-design.md](../superpowers/specs/2026-11-20-serve-design.md) (tabla de criterios, N2): «se informa el desplazamiento; si un estilo se mueve **> 5 pp** en algún nivel (novato), **parada** y reporte, sin recalibrar». No se toca. Una variante queda en la banda solo si su máximo |Δ| es ≤ 5 pp frente a las dos referencias, como en `sc-globo-fuerte`. Si s1 la rompe, se anota y se mide s2 igual; no se recalibra.
- Orden: pruebas, `ls00`, s1 (métricas y N2), s2 (métricas y N2).
- Unity: candado `/Users/rafaallendes/sc-unity.lock`, un solo Unity, una corrida por paso, sin reintentos. Una falla de entorno no cuenta como verde: se registra y me detengo.

### Métricas por estilo (Fondista / Definidor / Completo), antes (b2b) y después

- % de puntos del humano con el drive al fondo, con P = 1 y con P = 0,85 (`cells.tsv`).
- % de recepciones del rival que no llegan (`Unreachable` o `NoSwing`), con P = 1.
- Población de defensa de `sc-calidad-defensa` (recepciones del rival a las que llega tras un drive al fondo, `fondo-max` y `fondo-085` juntas):
  - holgura mediana del rival (la de su plan) y % estirado;
  - holgura mediana del humano sobre la pelota del rival;
  - % de defensas en las que el rival pierde el punto en el acto.
- **Drop del humano** (`corto-drop`, % de puntos), destacado.
- **Lecturas** (`swing-read.tsv`, las cuatro intenciones): % de lecturas erradas (y de cada tipo) y, para acertadas frente a erradas, el % de recepciones siguientes del rival a las que no llega y el % de puntos que pierde en el acto. Por separado en el drive al fondo y en el drop.
- N2: máximo |Δ| frente a las dos referencias y si queda en la banda.

Ruido: una celda con P = 1 tiene ~740 puntos, así que los % de puntos varían ~±2 pp; con miles de defensas o lecturas por estilo, sus % varían ~±1–2 pp. Leo solo diferencias mayores.

### Salida

Una tabla con una fila por valor (antes, s1, s2), la conclusión y los riesgos. Si alguna variante queda en la banda, redacto la pregunta de sí o no para Rafael sobre adoptar la lectura del swing.
