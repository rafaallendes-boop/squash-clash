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

## Resultados

Medido el 27 nov de 14:17 a 15:26 Chile, en el orden declarado, una corrida por paso y sin reintentos de medición. Evidencia en [`docs/validation/evidence/2026-11-27-lectura-swing/`](../validation/evidence/2026-11-27-lectura-swing/):
- `metrics-<corrida>.tsv` con su `-source.tsv` y su `-head.txt`;
- `defense-<corrida>.tsv.gz`, `defense-cells-<corrida>.tsv` y `swing-read-<corrida>.tsv.gz`. El `defense.tsv` de `ls00` es idéntico byte a byte al de `q01`, así que no se duplica;
- `n2-s1.tsv` y `n2-s2.tsv` con su `-source.tsv` y su `-head.txt`, y `n2-delta.txt`;
- `swing-read-summary.md` (la salida de `summarize-swing-read.py ls00 s1 s2`), `misread-by-k.txt`, `control.txt` y `tests.txt`.

**Control (ANTES = b2b): en verde.** `ls00` (head `6da123e`, campos nuevos en 0, RallyParams `dfe84db9bae8`) reproduce `q01` con 0 diferencias (`control.txt`):
- `cells.tsv`, en todas las columnas salvo `seconds`;
- `rival-time.tsv`, `prev-ball.tsv`, `defense.tsv` y `defense-cells.tsv`, byte a byte;
- `swing-read.tsv` vacío.

**Pruebas** (`tests.txt`):
- RED en `b77cce7`: fallan 6 de 7, por aserción y sin errores CS.
- GREEN en `6da123e`: la suite `SquashClash.Core.Tests.Rally` da 300/312. Las 7 nuevas pasan, y las 12 que fallan son las mismas 12 que ya fallaban en la base `1fb50c7` (293/305): las anclas de hash y las pruebas que dependen de los defaults de b2b. No las toqué.

### Tabla (Fondista / Definidor / Completo)

Defensa: las recepciones del rival a las que llega tras un drive al fondo, con P = 1 y P = 0,85 juntas. Lecturas: la recepción del rival que sigue a cada golpe leído; «no llega / pierde en el acto», en %.

| Valor (head, RallyParams) | Drive al fondo P = 1, % | P = 0,85, % | Rival no llega (P = 1), % | Holgura del rival (s) · estirado, % | Holgura del humano (s) | **Drop del humano, %** | Pierde en el acto, % | Lecturas erradas, % (drive / drop) | Lee bien → no llega / pierde en el acto (drive al fondo) | **Lee mal** → no llega / pierde en el acto (drive al fondo) | N2 máx \|Δ\| frente a main / b2b (pp) | ¿En la banda? |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| **Antes: b2b** (`ls00`, `6da123e`, `dfe84db9bae8`) | 90,4 / 96,4 / 93,8 | 92,1 / 96,7 / 94,6 | 11,6 / 9,9 / 12,3 | 0,01 / 0,00 / 0,00 · 39 / 83 / 62 | 0,82 / 0,85 / 0,84 | **48,6 / 22,8 / 34,8** | 25,7 / 16,4 / 22,1 | — | — | — | 4,60 / — (`sc-posicion-espera`) | Sí |
| **s1:** X = 100 ms, p = 0,25 (`58220dc`, `341f61147b6d`) | 86,1 / 91,3 / 91,4 | 61,4 / 56,4 / 61,3 | 5,1 / 6,4 / 6,2 | 0,16 / 0,06 / 0,10 · 16 / 17 / 17 | 0,58 / 0,55 / 0,63 | **6,0 / 2,2 / 4,4** | 10,6 / 7,1 / 9,3 | 26 / 25 / 26 · 35 / 35 / 35 | 1,2/10,3 · 1,3/7,4 · 1,2/8,9 | 3,9/18,0 · 4,6/14,1 · 4,2/17,2 | **24,28 / 26,34** (Definidor) | **No** |
| **s2:** X = 200 ms, p = 0,25 (`0b2c72d`, `6092ce43db1a`) | 79,2 / 80,2 / 84,4 | 57,9 / 61,5 / 60,4 | 4,6 / 6,4 / 5,1 | 0,23 / 0,11 / 0,16 · 15 / 15 / 15 | 0,53 / 0,45 / 0,53 | **6,5 / 2,8 / 4,5** | 9,2 / 7,3 / 8,6 | 26 / 25 / 26 · 36 / 35 / 36 | 0,9/8,2 · 1,4/7,7 · 1,0/7,9 | 4,9/18,9 · 9,1/18,3 · 6,2/18,9 | **23,35 / 26,14** (Definidor) | **No** |

Detalle por intención, por tipo de error (dirección o largo) y del drop en `swing-read-summary.md`; Δ por celda de N2 en `n2-delta.txt`.

### Lectura

- **Leer el swing le da al rival casi todo el tiempo que le faltaba.**
  - Su holgura al llegar al drive al fondo pasa de ~0 a 0,06–0,23 s, y llegar estirado baja del 39–83 % al 15–17 %.
  - No llega con P = 1 el 5–6 %, frente al 10–12 %.
  - La holgura del humano baja de 0,82–0,85 s a 0,45–0,63 s. El rival pierde en el acto la mitad o menos (7–11 % frente a 16–26 %).
- **El drive al fondo todavía gana con P = 1, pero ya no con P = 0,85.** Con P = 1 el humano sigue ganando el 79–91 % (−3 a −16 pp). Con P = 0,85 cae a 56–62 % (−31 a −40 pp).
- **El drop del humano casi desaparece:** del 23–49 % al 2–7 % de los puntos. Si el rival lee bien el drop, sale hacia adelante antes del contacto y no llega solo el 0–0,3 % de las veces.
- **Leer mal sí cuesta, pero poco.**
  - Con una lectura errada, en el drive al fondo el rival no llega el 4–9 % de las veces y pierde en el acto el 14–19 %. Con una acertada, el 1 % y el 7–10 %.
  - El error de largo cuesta más que el de dirección: pierde en el acto el 17–25 %, frente al 11–15 %.
  - Aun leyendo mal, el rival queda mejor que hoy sin lectura (pierde en el acto el 16–26 %), porque después corrige sobre la pelota real.
  - El engaño vale algo, pero con p = 0,25 no alcanza para equilibrar.
- **X casi no importa: s1 ≈ s2.** La mayor parte de la ventaja no sale de los 100 ms extra, sino de que el rival sale antes del contacto en vez de 0,22 s de simulación (≈ 340 ms reales de reacción y percepción) después. Con X = 100 ms el rival ya gana ≈ 440 ms reales frente a hoy; con 200 ms, ≈ 540 ms. Pasar de 100 a 200 solo agrega algo en el drive con P = 1 (−7 a −11 pp en el Fondista y el Definidor).
- **N2: las dos variantes rompen la banda, y por mucho.** El novato pierde entre 6,6 y 24,3 pp frente a `main` y entre 10 y 26,3 pp frente a b2b, en las 9 celdas. Por la banda vigente es **parada y reporte, sin recalibrar**.

**Conclusión.**
- La lectura del swing hace lo que pedía Rafael: el rival sale antes y llega con tiempo. Contra el bot, el drive al fondo con P = 0,85 deja de ganar casi siempre y el drop del humano se anula.
- Pero endurece al rival para el novato 4 a 5 veces más de lo que permite la banda (23–26 pp frente a ≤ 5).
- Ninguna variante queda en la banda, así que, como dice el encargo, no redacto la pregunta de sí o no para Rafael.
- Si se quisiera seguir, la palanca no es X sino cuánto de la reacción se ahorra. Por ejemplo, que el rival lea el swing pero siga pagando su reacción desde el inicio del swing, o una p mucho más alta para el novato. Eso sería otro experimento con otros valores, y no lo hago aquí.

### Riesgos y lo que elegí yo

- **La moneda del error no es pareja.**
  - La sembré con el paso y el punto del contacto para no tocar los números de ningún cerebro. Pero la devolución del saque repite casi la misma situación en muchos partidos y saca la misma moneda.
  - Ahí la tasa de error es 31–46 %; en el resto del rally, 24–26 %, como se declaró (`misread-by-k.txt`). En total sale 25–27 %, y en el drop 35 %.
  - No lo corregí: el sesgo hace que el rival lea mal más de lo declarado, así que favorece al humano, y aun así la banda se rompe por 18–21 pp de más.
- **Diseño mío:** al leer, el rival sale sin reacción (la reacción ya la pagó leyendo el swing). Es lo que pide «sale X ms antes del contacto», y es lo que explica el tamaño del efecto (ver arriba).
- **Lectura sin pelota de verdad.** El rival lee la intención ideal (sin el error de timing del humano). Con el bot de σ 30 ms la diferencia es chica; con un humano real que ejecuta peor, el rival leería mejor de lo que el golpe sale.
- **Plan vacío.** Si la pelota leída no tiene ventana legal, el planificador devuelve la posición actual y el rival se queda quieto hasta el contacto real. Es raro (golpes que serán falta) y no lo medí aparte.
- **Solo en `RallySim`.** El juego real (`RallyDirector`) no lo lee: si se adoptara, faltaría llevarlo al juego y verlo en el iPhone.
- **Mismatch.** Hay puntos desalineados en 4 celdas de `fondo-085` (1–6 por celda, en s1 y s2). Quedan fuera solo de las estadísticas de contacto y de lectura.
- **El bot solo juega recto**, como en b2b: no se prueba el cruzado.
- **Hashes.**
  - RallyParams: `dfe84db9bae8` con los campos neutros (en la cabeza final, `d131ac2`), `341f61147b6d` en s1 y `6092ce43db1a` en s2. En `main` es `ebf8f717f8c5`.
  - Defaults `207da8f2c636` y SimulationKey sin cambios.
- **Revisión.** El código lo implementó un subagente Sonnet dirigido por mí, así que la revisión no es independiente. Falta la revisión cruzada de Codex.
- **Excepción de la regla.** La lectura del swing rompe «el rival lee solo la pelota» por la excepción de Rafael para este experimento. La rama no se fusiona y `main` no cambia.
- **Lo que faltó:** nada del encargo. La pregunta para Rafael no va porque ninguna variante quedó en la banda.
