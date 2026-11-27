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
