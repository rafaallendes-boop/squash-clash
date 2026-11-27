# El rival lee el swing solo en los golpes largos: experimento

27 nov 2026 · Claude (Opus: arquitectura, declaración, medición y revisión; el código lo implementa un subagente Sonnet dirigido por Claude, así que la revisión **no es independiente**) · encargo `set22-lectura-golpe-largo` de SC-PM. Es solo un experimento: rama local `rafaallendes-boop/sc-lectura-golpe-largo`, sin PR, sin merge y sin push.

**Autorización.** Rafael (PO), vía SC-Lead, 27 nov 14:10: «Sí, midan como experimento, sin merge, que el rival lea mi swing para salir antes». Es una **excepción solo para este experimento** a la regla vigente «el rival lee solo la pelota». La regla no cambia en `main`, y adoptarla le toca a Rafael.

Contexto: [2026-11-27-lectura-swing.md](2026-11-27-lectura-swing.md) (set21). Con la lectura en todos los golpes (s1 y s2), el rival llega con tiempo, pero el drop del humano casi desaparece (del 23–49 % al 2–7 % de los puntos) y N2 rompe la banda por 23–26 pp. El cambio de SC-Lead: que el rival anticipe solo los golpes largos.

## Declaración (antes de medir)

### Base

- Rama `sc-lectura-swing` en `1cf258a`: b2b (`RivalRecoverySideShift` = 1,6) con la lectura del swing de set21 ya implementada en `RallySim` y sus dos campos en 0 (RallyParams `dfe84db9bae8`), y su instrumentación (`swing-read.tsv`).
- Todo lo que hace la lectura en set21 sigue igual (cuándo sale, qué lee, adónde va, los dos tipos de error y cómo corrige), salvo lo que se declara abajo.

### Qué cambia (solo el rival; el humano no cambia)

Un campo nuevo de RallyParams, **neutro (0) por defecto**: `RivalSwingReadLongOnly` (0 o 1). Con 0, la lectura es la de set21. Con 1 y `RivalSwingReadLeadMs` > 0, el rival lee el swing **solo si es de un golpe largo**. Si el swing es corto, no hay lectura: no se mueve antes del contacto, no tira la moneda, no se registra nada en `SwingReads`, y paga su reacción y su percepción completas, igual que hoy.

**La señal del swing.** Un swing es largo si se cumplen las dos condiciones:
1. va al frontis, no a la lateral (no es un boast);
2. su potencia `P` es ≥ `PowerMedium` (0,5).

`P` es el tamaño del swing. En el juego es el largo del tirón del gesto (la `P` de `SlingModel`, el tirón dividido por `pullFullMm`), o sea, cuánto lleva la raqueta atrás. El humano lo fija antes del contacto. En la simulación es la `P` del `AiShot` del humano. Que vaya a la lateral también es parte del swing: es adónde apunta el gesto. No uso la pelota que de verdad saldrá: ni su trayectoria, ni su bote, ni su largo.

Por qué esta señal. En squash el golpe al fondo se juega con un swing largo y abierto, que se ve venir. El drop se prepara igual, pero se le pega con un empujón corto y suave, así que hasta el contacto parece la misma preparación: no hay nada que leer. En la simulación, eso da:
- **largo** (lee): el drive al fondo (`P` 0,85 o 1), el drive medio suave (0,5) y el globo (alto, 0,5 o 0,85);
- **corto** (no lee): todos los drops (`P` 0,12 o 0,27);
- **corto** (no lee): el boast, aunque sea fuerte, porque se juega desde la preparación del drive y es el golpe de engaño clásico (en la batería y en N2 el humano no hace boasts).
- Un recto bajo con `P` ≥ 0,5 cuenta como largo: es un drive bajo con el swing abierto. Esto se aparta de `ShotOption.Long` (línea y altura, la regla que set21 usa para el error de largo, donde lo bajo es corto). Elijo `P` porque en el juego existe tal cual (el tirón) y no depende de una etiqueta de línea que el gesto del humano no tiene.

**El error de largo.** Con `RivalSwingReadLongOnly` = 1, el rival solo lee swings largos, así que el error de largo siempre lo lee como el drop (bajo, medio). El error de dirección es el de set21: la postura espejada. p = 0,25 y mitad y mitad, como en set21.

**La moneda del error (corrección del sesgo).** En set21 la semilla era el paso y el punto del contacto. En la devolución del saque, la misma situación se repite en muchos partidos y saca la misma moneda: ahí la tasa de error salió 31–46 % (25 % en el resto del rally).
- **Elijo una semilla distinta:** a la semilla se le suma el error de timing del humano en ese golpe (`DeltaMs`, los bits del double).
- `DeltaMs` es continuo y aleatorio (la gaussiana σ del cerebro del humano), así que cambia en cada golpe y en cada partido, y la situación repetida ya no saca la misma moneda.
- No consume los números de ningún cerebro: usa el valor que ya salió.
- El rival no lo usa para saber adónde va la pelota; solo siembra la moneda. Sigue leyendo la intención ideal, como en set21.
- No saco la devolución del saque de la lectura: el encargo pide que la tasa quede cerca de 0,25 en todos los golpes, y sacarla cambiaría lo que se mide frente a set21.
- La corrección vale para toda lectura, también con `RivalSwingReadLongOnly` = 0. Con `RivalSwingReadLeadMs` = 0 no se tira ninguna moneda, así que b2b no cambia. Las corridas s1 y s2 de set21 se reproducen solo en sus commits (`58220dc` y `0b2c72d`).

**Lo demás.**
- No toca la elección de golpe del rival, el lado humano, `FondoAi`, la banda, los umbrales, los criterios, los oráculos, las pruebas existentes, `DefaultPreset.json` ni SimulationKey.
- Se implementa solo en `RallySim`, como set21. El juego real (`RallyDirector`) no lo lee.
- El campo nuevo cambia el hash de RallyParams aunque esté neutro. Las 12 pruebas que ya fallan en la base (anclas de hash y defaults de b2b) siguen fallando; no las toco y lo informo.

**Pruebas nuevas** (TDD: RED válido por aserción y luego GREEN):
- con los defaults, `RivalSwingReadLongOnly` es 0;
- la señal: el drive, el medio y el globo con `P` ≥ 0,5 son largos; los drops y el boast son cortos;
- con el campo en 1, X = 200 ms y p = 0: cada golpe largo del humano da una lectura y ningún golpe corto da lectura;
- con el campo en 1, un humano que solo juega drops da los mismos puntos que con la lectura apagada;
- la moneda: en un mismo paso y punto de contacto, con muchos errores de timing distintos, la tasa de error queda cerca de p (0,25) y los dos tipos salen mitad y mitad.

### Valores (2) y por qué

| Variante | X (ms reales) | p | `RivalSwingReadLongOnly` |
|---|---|---|---|
| **l1** | 100 | 0,25 | 1 |
| **l2** | 200 | 0,25 | 1 |

- Los mismos X y p que s1 y s2 de set21, para que la diferencia con set21 sea solo leer únicamente los golpes largos (y la corrección de la moneda).
- p = 0,25 queda fija, como pide el encargo. Es un supuesto, no un valor calibrado.

Cada variante es un commit con sus valores como defaults, porque `ConfirmPerStyle` usa `new RallyParams()`. Cada corrida guarda su `head.txt`. Al final, los tres campos vuelven a neutros (b2b sigue aplicado, como en la base).

**Lo que espero en N2, antes de medir.** El novato de N2 (`NewcomerBot`) nunca elige el drop: juega recto o cruzado, a media altura, fuerte (60 %) o medio (`P` 0,85 o 0,5). Todos sus swings son largos, salvo los raros golpes de respaldo. Para N2, l1 y l2 son casi s1 y s2 de set21 (cambia solo la moneda), así que espero que rompan la banda otra vez, por un margen parecido (≈ 23–26 pp). Lo mido igual, como pide el encargo.

### Batería (la de set21)

- **Métricas:** Medio, los tres estilos, `IntentBot` σ 30 ms, 100 partidos por celda, semillas `20261126`, las cuatro intenciones (`fondo-max`, `fondo-085`, `medio-suave`, `corto-drop`). Arnés y archivos de set21 sin cambios (`cells.tsv`, `rival-time.tsv`, `prev-ball.tsv`, `defense.tsv`, `defense-cells.tsv`, `swing-read.tsv`).
- **ANTES = b2b.** Una corrida nueva, `lg00`, con los tres campos neutros. **Control:** `cells.tsv` (salvo `seconds`), `rival-time.tsv`, `prev-ball.tsv` y `defense.tsv` tienen que dar igual que en `q01` de `sc-calidad-defensa`, con `scripts/compare-control.py`, y `swing-read.tsv` tiene que estar vacío. Si hay una sola diferencia, me detengo y no informo la tabla. El hash de RallyParams sí cambia (campo nuevo) y lo anoto.
- **N2:** `RivalStylesCalibrationExperiment.ConfirmPerStyle` sin cambios: semillas de confirmación `20261219`, 200 partidos por celda, novato σ 90 ms y `bot30`, 3 estilos × 3 niveles. Se informa el Δ del novato por celda frente a dos referencias:
  - la base de `main` (`n2-before01-from-6086074.tsv`);
  - b2b (`n2-b2b.tsv`), para separar el efecto de la lectura.
- **Banda vigente, cita textual** de [2026-11-20-serve-design.md](../superpowers/specs/2026-11-20-serve-design.md) (tabla de criterios, N2): «se informa el desplazamiento; si un estilo se mueve **> 5 pp** en algún nivel (novato), **parada** y reporte, sin recalibrar». No se toca. Una variante queda en la banda solo si su máximo |Δ| es ≤ 5 pp frente a las dos referencias, como en set21. Si l1 la rompe, se anota y se mide l2 igual; no se recalibra.
- Orden: pruebas, `lg00`, l1 (métricas, tasa de error y N2), l2 (métricas, tasa de error y N2).
- Unity: candado `/Users/rafaallendes/sc-unity.lock`, un solo Unity, una corrida por paso, sin reintentos. Una falla de entorno no cuenta como verde: se registra y me detengo.

**Verificación de la moneda.** De `swing-read.tsv`, la tasa de lecturas erradas por tipo de golpe del humano (línea, altura y potencia) y por posición en el punto:
- k = 1: la devolución del saque (el rival sacó);
- k = 2: el primer golpe del humano cuando sacó él;
- k > 2: el resto del rally.

Criterio declarado: cada grupo con n ≥ 500 queda dentro de 0,25 ± 3·√(0,25·0,75/n) (± 5,8 pp con n = 500; ± 4,1 pp con n = 1000). Los grupos más chicos se informan sin criterio. Si un grupo con n ≥ 500 queda fuera, es un defecto de la moneda: lo registro y me detengo antes del N2 de esa variante.

### Métricas por estilo (Fondista / Definidor / Completo): la tabla de set21, una fila por valor (antes, l1, l2)

- % de puntos del humano con el drive al fondo, con P = 1 y con P = 0,85 (`cells.tsv`).
- % de recepciones del rival que no llegan (`Unreachable` o `NoSwing`), con P = 1.
- Defensa (recepciones del rival a las que llega tras un drive al fondo, `fondo-max` y `fondo-085` juntas): holgura mediana del rival y % estirado; holgura mediana del humano; % de defensas en las que el rival pierde el punto en el acto.
- **Drop del humano** (`corto-drop`, % de puntos), destacado.
- **Lecturas:** % de lecturas erradas (en el drive al fondo y en el drop) y, para acertadas frente a erradas, el % de recepciones siguientes del rival a las que no llega y el % de puntos que pierde en el acto, en el drive al fondo.
- **Tasa de error por tipo de golpe** (lo nuevo): la de la verificación de la moneda.
- **N2**, destacado: máximo |Δ| frente a las dos referencias y si queda en la banda.

Ruido: una celda con P = 1 tiene ~740 puntos, así que los % de puntos varían ~±2 pp; con miles de defensas o lecturas por estilo, sus % varían ~±1–2 pp. Leo solo diferencias mayores.

### Salida

Una tabla con una fila por valor (antes, l1, l2), la conclusión y los riesgos. Si alguna variante queda en la banda, redacto la pregunta de sí o no para Rafael sobre adoptarla.

## Resultados

Medido el 27 nov de 15:40 a 16:22 Chile, en el orden declarado, una corrida por paso y sin reintentos de medición. **Detenido antes del N2 de l2:** la verificación de la moneda de l2 salió fuera del criterio declarado en la devolución del saque (ver abajo). El N2 de l2 no se corrió. Evidencia en [`docs/validation/evidence/2026-11-27-lectura-golpe-largo/`](../validation/evidence/2026-11-27-lectura-golpe-largo/):
- `metrics-<corrida>.tsv` con su `-source.tsv` y su `-head.txt`;
- `defense-<corrida>.tsv.gz` (l1 y l2), `defense-cells-<corrida>.tsv` y `swing-read-<corrida>.tsv.gz`. El `defense.tsv` de `lg00` es idéntico byte a byte al de `q01`, así que no se duplica;
- `n2-l1.tsv` con su `-source.tsv` y su `-head.txt`, y `n2-delta.txt`;
- `swing-read-summary.md` (`summarize-swing-read.py lg00 l1 l2`), `misread-by-shot.txt` (`misread-by-shot.py`), `coin-k1-diagnostic.txt`, `control.txt` y `tests.txt`.

**Control (ANTES = b2b): en verde.** `lg00` (head `35a9f05`, campos nuevos en 0, RallyParams `c932fc94ec91`) reproduce `q01` con 0 diferencias (`control.txt`):
- `cells.tsv`, en todas las columnas salvo `seconds`;
- `rival-time.tsv`, `prev-ball.tsv`, `defense.tsv` y `defense-cells.tsv`, byte a byte;
- `swing-read.tsv` vacío.

**Pruebas** (`tests.txt`):
- RED en `449aaf5`: fallan 4 de 5, por aserción y sin errores CS. La que pasa es la del default en 0.
- GREEN en `c56773f`: la suite `SquashClash.Core.Tests.Rally` da 305/317. Las 5 nuevas y las 7 de set21 pasan, y las 12 que fallan son las mismas 12 de la base. No las toqué.

### Tabla (Fondista / Definidor / Completo)

Defensa: las recepciones del rival a las que llega tras un drive al fondo, con P = 1 y P = 0,85 juntas. Lecturas: la recepción del rival que sigue a cada golpe leído; «no llega / pierde en el acto», en %.

| Valor (head, RallyParams) | Drive al fondo P = 1, % | P = 0,85, % | Rival no llega (P = 1), % | Holgura del rival (s) · estirado, % | Holgura del humano (s) | **Drop del humano, %** | Pierde en el acto, % | Lecturas erradas, % (drive / drop) | Lee bien → no llega / pierde en el acto (drive al fondo) | **Lee mal** → no llega / pierde en el acto (drive al fondo) | **N2 máx \|Δ\| frente a main / b2b (pp)** | ¿En la banda? |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| **Antes: b2b** (`lg00`, `35a9f05`, `c932fc94ec91`) | 90,4 / 96,4 / 93,8 | 92,1 / 96,7 / 94,6 | 11,6 / 9,9 / 12,3 | 0,01 / 0,00 / 0,00 · 39 / 83 / 62 | 0,82 / 0,85 / 0,84 | **48,6 / 22,8 / 34,8** | 25,7 / 16,4 / 22,1 | — | — | — | **4,60 / —** (`sc-posicion-espera`) | Sí |
| **l1:** X = 100 ms, p = 0,25, solo largos (`4384296`, `0cbb811942de`) | 80,7 / 90,4 / 91,6 | 63,1 / 53,5 / 58,4 | 4,9 / 6,3 / 6,1 | 0,17 / 0,06 / 0,10 · 15 / 17 / 16 | 0,58 / 0,54 / 0,63 | **48,6 / 22,8 / 34,8** | 10,5 / 6,8 / 9,1 | 25 / 25 / 25 · no lee | 1,2/10,1 · 1,3/7,1 · 1,1/8,6 | 4,9/19,2 · 5,0/14,4 · 4,6/17,6 | **23,46 / 26,25** (Definidor, Fácil) | **No** |
| **l2:** X = 200 ms, p = 0,25, solo largos (`df70e80`, `46308cf9a0b9`) | 75,4 / 80,3 / 84,1 | 59,0 / 60,5 / 63,8 | 4,3 / 6,0 / 4,8 | 0,23 / 0,11 / 0,16 · 14 / 14 / 15 | 0,53 / 0,44 / 0,54 | **48,6 / 22,8 / 34,8** | 10,0 / 7,5 / 9,1 | 25 / 25 / 25 · no lee | 0,9/8,9 · 1,4/8,2 · 1,0/8,3 | 5,9/20,9 · 9,0/17,6 · 6,2/19,8 | **no medido** (detenido) | — |

Como referencia, set21 con lectura en todos los golpes: drop del humano 6,0 / 2,2 / 4,4 (s1) y 6,5 / 2,8 / 4,5 (s2); N2 24,28 / 26,34 (s1) y 23,35 / 26,14 (s2).

El drive medio suave (`medio-suave`) sí se lee: el humano gana 28,5 / 32,7 / 32,3 % con b2b, 28,5 / 7,7 / 20,7 % con l1 y 27,6 / 10,5 / 18,8 % con l2. El 28,5 del Fondista en l1 es una coincidencia: las filas cambian (964 puntos frente a 969).

### Tasa de error por tipo de golpe (`misread-by-shot.txt`)

Declarado: p = 0,25. Criterio: cada grupo con n ≥ 500 dentro de 0,25 ± 3·√(0,25·0,75/n).

| Grupo | l1: n · % erradas | l2: n · % erradas |
|---|---|---|
| Recto, medio, medio (`medio-suave`) | 29 278 · 24,9 | 28 978 · 24,8 |
| Recto, medio, fuerte (drive al fondo) | 71 531 · 25,0 | 68 737 · 25,1 |
| Drop (cualquiera) | 0 · no lee | 0 · no lee |
| k = 1, devolución del saque | 3 877 · 23,7 (banda ± 2,1) | 4 042 · **22,5 (banda ± 2,0): FUERA** |
| k = 2, primer golpe tras el saque propio | 4 291 · 25,1 | 4 330 · 25,1 |
| k > 2, resto del rally | 92 641 · 25,0 | 89 343 · 25,1 |
| Veredicto | **MONEDA OK** | **MONEDA FUERA** (k = 1) |

- En set21 (moneda vieja) la devolución del saque daba 50,2 % (s1, mismo script). La corrección baja el sesgo de 25 pp a 1–2,5 pp, y ahora el error va del lado contrario (el rival lee mal un poco menos).
- Por intención, cada subgrupo de k = 1 de l2 (22,2–22,7 %) queda dentro de su banda; solo el grupo k = 1 completo queda fuera.
- **Hipótesis, no probada** (`coin-k1-diagnostic.txt`, de solo lectura): las lecturas k = 1 no son independientes entre celdas. Las nueve celdas (3 estilos × 3 intenciones que leen) comparten las semillas del humano y la situación del saque. En ~50 % de los partidos, la primera lectura k = 1 da el mismo resultado en todas las celdas, cuando con monedas independientes pasaría en ~8 %. Así, el n efectivo de k = 1 es varias veces menor que el nominal, y la banda declarada, que supone independencia, es más estrecha de lo que corresponde. Pasa igual en l1, que sí quedó dentro. No lo corregí: el criterio estaba declarado y me detuve, como dice.

### Lectura

- **El drop del humano vuelve entero.** Con l1 y l2 el drop gana lo mismo que con b2b (48,6 / 22,8 / 34,8 %), punto por punto: el rival no lee ningún drop, así que esas celdas reproducen el control exacto. En set21 el drop había caído al 2–7 %.
- **En los golpes largos, el rival gana lo mismo que en set21.** Llega con holgura (0,06–0,23 s), solo llega estirado el 14–17 % de las veces (39–83 % en b2b), no llega con P = 1 el 4–6 % (10–12 %) y pierde en el acto el 7–11 % (16–26 %). El drive al fondo con P = 1 sigue ganando el 75–92 %, pero con P = 0,85 cae al 54–64 % (92–97 % en b2b). El drive medio suave también pierde fuerza en el Definidor y el Completo.
- **Leer mal cuesta como en set21.** Con una lectura errada en el drive al fondo, el rival pierde en el acto el 14–21 %, frente al 7–10 % cuando acierta. El error de largo, que ahora siempre es «leer drop», es el más caro.
- **X pesa poco**, como en set21: de 100 a 200 ms, el drive con P = 1 baja 1–10 pp más; con P = 0,85 no hay una diferencia clara.
- **N2 (l1): rompe la banda, por lo mismo que set21.** El novato pierde entre 6,0 y 23,5 pp frente a `main`, y entre 9,4 y 26,3 pp frente a b2b, en las 9 celdas (`n2-delta.txt`). Es lo que se esperaba antes de medir: el novato nunca hace drops, así que todos sus golpes se leen. Por la banda vigente es **parada y reporte, sin recalibrar**.

**Conclusión.**
- Leer solo los golpes largos hace lo que pedía SC-Lead en la batería: el rival anticipa el drive y el globo, y el drop del humano queda como hoy.
- Para el novato de N2 no cambia nada frente a set21: l1 endurece al rival 23–26 pp, cinco veces lo que permite la banda (≤ 5). La lectura de golpes largos castiga justo al jugador que solo pega largo, que es el novato.
- Ninguna variante medida queda en la banda (l1 fuera; l2 sin N2, detenida), así que no redacto la pregunta de sí o no para Rafael.
- Si se quisiera seguir, las palancas no son X: que la lectura le ahorre al rival solo parte de su reacción, o que dependa del nivel (por ejemplo, apagada en Fácil). Serían otro experimento, con otros valores, y no los hago aquí.

### Riesgos y lo que elegí yo

- **Detención.** Me detuve por el criterio de la moneda que yo mismo declaré. La causa probable es que el criterio supone muestras independientes (ver la hipótesis), no un defecto de la moneda, pero no está probado. Si SC-PM o Rafael dan por buena la explicación, el N2 de l2 se corre desde `df70e80` con el árbol limpio (`scripts/measure-deep-defense.sh l2 n2`). Por cómo salió l1, lo esperable es que l2 también rompa la banda.
- **Señal del swing elegida por mí:** `P` ≥ `PowerMedium` y que no sea un boast. En la batería y en N2 da lo mismo que clasificar por la línea (el humano no hace boasts ni rectos bajos fuertes). En el juego real, un drop con el tirón largo contaría como largo, y un drive con el tirón corto, como corto. Eso es parte del juego («disimular»), pero no está medido.
- **Moneda nueva:** el error de timing del humano siembra la moneda. Con un humano sin ruido de timing (`DeltaMs` = 0) vuelve el sesgo de set21. En el juego real el timing siempre varía.
- **Lo demás de set21 sigue igual:** el rival sale sin reacción, lee la intención ideal (sin el error de timing), y el plan vacío lo deja quieto. Todo está solo en `RallySim`: el juego real (`RallyDirector`) no lo lee. El bot solo juega recto.
- **Hashes.**
  - RallyParams: `c932fc94ec91` con los campos neutros (en la cabeza final, `1c72619`, el código es idéntico a `35a9f05`), `0cbb811942de` en l1 y `46308cf9a0b9` en l2. En `main` es `ebf8f717f8c5`.
  - Defaults `207da8f2c636` y SimulationKey sin cambios.
- **Revisión.** El código lo implementó un subagente Sonnet dirigido por mí, así que la revisión no es independiente. Falta la revisión cruzada de Codex.
- **Excepción de la regla.** La lectura del swing rompe «el rival lee solo la pelota» por la excepción de Rafael para este experimento. La rama no se fusiona y `main` no cambia.
- **Lo que faltó del encargo:** el N2 de l2 (detenido). La pregunta para Rafael no va porque ninguna variante medida quedó en la banda.
