# Lectura de golpes largos solo en Medio y Difícil: experimento

28 nov 2026 · Claude (Opus: arquitectura, declaración, medición y revisión; el código lo implementa un subagente Sonnet dirigido por Claude, así que la revisión **no es independiente**) · encargo `set25-lectura-largo-medio-dificil` de SC-PM. Solo experimento: rama `rafaallendes-boop/sc-lectura-largo-md`, push sin force al final, sin PR y sin merge. Sin Codex: el encargo es solo de Claude, así que no hay revisión cruzada.

**Autorización.** Rafael (PO), vía SC-Lead, 28 nov: «Midan la lectura de golpes largos solo en Medio y Difícil, con Fácil igual que hoy, y apliquen N2 a todo salvo a las celdas del drive al fondo». Es una **excepción solo para este experimento** a la regla «el rival lee solo la pelota». La regla no cambia en `main`. Adoptarlo le toca a Rafael: aquí se mide, no se recalibra ni se decide.

Parte de set22: [2026-11-27-lectura-golpe-largo.md](2026-11-27-lectura-golpe-largo.md). Allí, l1 (X = 100 ms, p = 0,25, solo golpes largos) devolvió entero el drop del humano, pero N2 rompió la banda en las 9 celdas (máx. 23,5 pp frente a `main`), también en Fácil.

## Declaración (antes de medir)

### Base

- Rama `sc-lectura-golpe-largo` en `c2bbcc2`: b2b (`RivalRecoverySideShift` = 1,6) con la lectura del swing de set21 y set22 en `RallySim`, sus tres campos en 0 (RallyParams `c932fc94ec91`), la corrección de la moneda de set22 (la semilla suma `DeltaMs`) y la instrumentación (`swing-read.tsv`).
- **La señal del swing es la de set22, ya declarada y sin cambios:** un swing es largo si no es un boast (no apunta a la lateral) y su `P` es ≥ `PowerMedium` (0,5) (`RivalSwingRead.LongSwing`). Nunca se usa la pelota futura.

### Qué cambia: el campo de nivel

Un campo nuevo de RallyParams, **neutro (0) por defecto**: `RivalSwingReadMinLevel` (0 o 1).
- **0:** como set22; la lectura no distingue el nivel.
- **1:** el rival lee solo si su nivel es Medio o Difícil. En Fácil no hay lectura: no se programa, no se tira la moneda, no hay nada en `SwingReads` y el rival paga su reacción y su percepción completas, como hoy.

**Cómo sabe `RallySim` el nivel (el diseño más pequeño que encontré).**
- `RallySim` solo recibe cerebros (`IRallyBrain`). `RivalAi` tampoco guarda el nivel: recibe una `AiDifficulty` ya construida (`RivalAi.cs:82-84`).
- N2 crea el rival con `new RivalAi(ctx, Scaled(level, k), style, seed)` (`RivalStylesCalibrationExperiment.cs:61`, con la dificultad de `Scaled`, `:161`), y ese arnés no se puede cambiar. Así que el nivel no se puede pasar por un parámetro nuevo.
- **Lo que hago:** `RivalAi` obtiene su nivel una vez, en el constructor, de la **temperatura** de la dificultad configurada. Es la única parte de `AiDifficulty` que fija solo el nivel (`EasyTemperature` 1,2, `MediumTemperature` 0,6 y `HardTemperature` 0,3, `RallyParams.cs:50-52`), y el factor k del estilo nunca la escala (`RivalAi.cs:44` «never the temperature» y `:54-58`; `Scaled` de N2, `RivalStylesCalibrationExperiment.cs:161`). Si la temperatura no coincide con exactamente un nivel, el nivel queda desconocido.
- Una interfaz nueva de una sola propiedad, `IRallyLevel { AiLevel? Level }`. La implementa `RivalAi`. `RallySim` la lee de su cerebro rival una sola vez, al construirse.
- La regla queda en `RivalSwingRead`: lee si `RivalSwingReadMinLevel` ≤ 0, o si el nivel es Medio o Difícil. **Con el campo en 1 y el nivel desconocido** (un cerebro rival sin la interfaz), **no lee**.
- La decisión depende solo de la dificultad configurada del rival, fija durante toda la simulación. Nunca de la pelota, del punto ni del resultado.
- El arnés de métricas envuelve al rival en `RivalTap` (`DeepDefenseExperiment.cs:72`); `RivalTap` pasa a implementar `IRallyLevel` y devuelve el nivel del `RivalAi` que envuelve. Con el campo en 0 nadie lo lee, así que el control no cambia.
- El juego real (`RallyDirector`) no lo lee. No toca la elección de golpe, el lado humano, `FondoAi`, `DefaultPreset.json` ni SimulationKey. El campo nuevo cambia el hash de RallyParams aunque esté en 0.

**Pruebas nuevas** (TDD: RED válido por aserción y luego GREEN; ninguna prueba existente cambia):
1. Con los defaults, `RivalSwingReadMinLevel` es 0.
2. `RivalAi.For(ctx, nivel, estilo, …)` da ese nivel en los 3 niveles × 3 estilos. Con la dificultad del nivel escalada por k = 2,5 (como `Scaled` de N2) da el mismo nivel. Con una temperatura que no es de ningún nivel, da desconocido.
3. Campo en 1, X = 100 ms, p = 0,25, solo largos, **Fácil**: ningún punto tiene lecturas, y los puntos son idénticos a los de la misma partida con la lectura apagada.
4. Campo en 1 frente a campo en 0, **Medio y Difícil**: las mismas lecturas y los mismos puntos.
5. Campo en 0, Fácil: sí hay lecturas (set22 sin cambios).
6. Campo en 1, un rival sin `IRallyLevel`: no hay lecturas.

### Valores

| Variante | X (ms reales) | p | `RivalSwingReadLongOnly` | `RivalSwingReadMinLevel` |
|---|---|---|---|---|
| **m1** (obligatoria) | 100 | 0,25 | 1 | 1 |
| **m2** (si la moneda de m1 pasa) | 200 | 0,25 | 1 | 1 |

- m1 es l1 de set22 con el campo de nivel en 1; m2 es l2 con el campo en 1. p = 0,25 queda fija (supuesto, no calibrado).
- Cada variante es un commit con sus valores como defaults, porque `ConfirmPerStyle` usa `new RallyParams()`. Cada corrida guarda su `head.txt`. Al final, un commit vuelve a neutros los cuatro campos de set21/set22/set25 (b2b sigue aplicado, como en la base).
- m2 se mide solo si la moneda de m1 pasa, como dice el encargo.

### Batería: la de set22, ahora por nivel

- **Métricas:** `DeepDefenseExperiment.WriteDeepDefense` sin cambios en lo que mide: los tres estilos, `IntentBot` σ 30 ms, 100 partidos por celda, semillas `20261126`, las cuatro intenciones (`fondo-max`, `fondo-085`, `medio-suave`, `corto-drop`).
  - El arnés ya lee los niveles de `SC_DD_LEVELS` (por defecto `Medium`, `DeepDefenseExperiment.cs:267`). Las semillas de cada partido no dependen del nivel (`seeds+2*m`, `seeds+1+2*m`), y cada celda crea sus cerebros y su simulación de cero. Por eso, correr los tres niveles no cambia lo que mide cada nivel.
  - Extensiones: `scripts/measure-deep-defense.sh` guarda los niveles de la corrida en `levels.txt`. `scripts/slice-level.py`, nuevo y de solo lectura, separa por nivel los archivos de una corrida (cabecera más las filas de ese nivel, byte a byte) para compararlos y resumirlos con los scripts de set22 sin cambiarlos.
- **Corridas, en este orden, una por paso, sin reintentos:**
  1. `mc00`, control: campos neutros, Medio.
  2. `mr00`, referencia: campos neutros, Fácil y Difícil. Su Fácil es la referencia de «Fácil igual que hoy». Su Difícil y el Medio de `mc00` son las filas «Antes: b2b».
  3. `m1`: métricas en Fácil, Medio y Difícil; la moneda; y N2 (`m1-n2`) solo si la moneda pasa.
  4. `m2`, igual que m1, si aplica.
- **Control b2b:** `mc00` tiene que reproducir `q01` de `sc-calidad-defensa` con `scripts/compare-control.py`: 0 diferencias y `swing-read.tsv` vacío, como `lg00` de set22. Si falla: **DETENIDO**, sin tabla.
- **Fácil idéntico a hoy, por variante.** El Fácil de la variante frente al Fácil de `mr00`, los dos separados con `slice-level.py`:
  - `cells.tsv` (salvo `seconds`), `rival-time.tsv`, `prev-ball.tsv` y `defense.tsv`, con `compare-control.py`; además, `rival-time.tsv`, `prev-ball.tsv`, `defense.tsv` y `defense-cells.tsv` byte a byte (`cmp`);
  - `swing-read.tsv` de Fácil vacío.
  - Resultado en la fila «Fácil: idéntico a hoy sí/no».
- **«Igual que hoy» quiere decir igual que b2b en Fácil, no igual que `main`.** Lo que ya está medido, antes de esta corrida: b2b no es igual a `main` en Fácil. En N2, b2b se mueve en Fácil +0,42 / +2,79 / +1,33 pp (Fondista / Definidor / Completo) frente a `main` (`n2-delta.py n2-b2b.tsv` frente a `n2-before01-from-6086074.tsv`). Además, `RivalRecoverySideShift` (b2b) se aplica en todos los niveles (`RivalAi.Recovery`, `RivalAi.cs:89-105`). No afirmaré que Fácil es idéntico a `main`.

### La moneda (antes de N2)

- Con `scripts/misread-by-shot.py`, por tipo de golpe y por posición (k = 1, k = 2, k > 2), sobre Medio y Difícil juntos y sobre cada nivel por separado. Fácil debe quedar vacío.
- **Criterio del encargo (reemplaza aquí al de set22):** la tasa de lecturas erradas en la devolución del saque (k = 1) queda dentro de 25 ± 2,0 puntos, es decir entre 23,0 y 27,0 %. Tiene que cumplirse en las tres agrupaciones: Medio y Difícil juntos, solo Medio y solo Difícil.
- Si alguna queda fuera: **DETENIDO** y reporte, sin el N2 de esa variante y sin la variante siguiente. No corrijo la moneda a medio experimento.
- El veredicto «MONEDA OK/FUERA» que imprime el script es el criterio de set22: se informa, pero no decide.

### N2

- `RivalStylesCalibrationExperiment.ConfirmPerStyle` sin cambios: semillas `20261219`, 200 partidos por celda, novato σ 90 ms y `bot30`, 3 estilos × 3 niveles.
- `scripts/n2-delta.py`: el Δ del novato por celda frente a `main` (`n2-before01-from-6086074.tsv`) y frente a b2b (`n2-b2b.tsv`).
- **Banda vigente, cita textual** de [2026-11-20-serve-design.md](../superpowers/specs/2026-11-20-serve-design.md) (tabla de criterios, N2): «se informa el desplazamiento; si un estilo se mueve **> 5 pp** en algún nivel (novato), **parada** y reporte, sin recalibrar». No se cambia. Una lectura queda en la banda solo si su máximo |Δ| es ≤ 5 pp frente a las dos referencias, como en set21 y set22. Se informa también por nivel.

**«N2 a todo salvo a las celdas del drive al fondo»: qué hay en el código.** El pedido es ambiguo, y no lo decido yo:
- Las celdas de N2 son (fase, nivel, estilo, referencia), y su resultado es el % de puntos del novato en 200 partidos (`RivalStylesCalibrationExperiment.cs:55-81` y `:150-182`). No tienen una dimensión de golpe o intención.
- El novato (`NewcomerBot.Decide`, `RivalStylesBaselineExperiment.cs:33-45`) elige en cada golpe recto o cruzado (50/50), fuerte o medio (60/40), apuntando a media altura; el drop solo puede salir como respaldo, si ninguna opción más cercana pasa `Forecast`. El «drive al fondo» de la batería es recto, medio y fuerte (`fondo-max` y `fondo-085`, `DeepDefenseExperiment.cs:248-254`, y la condición `drive` de `defense.tsv`). Así, en cada celda de N2, cerca del 30 % de los golpes del novato (0,5 × 0,6) son drives al fondo con P = 0,85, mezclados con los demás en los mismos puntos.
- **Conclusión, antes de medir:** ninguna celda de N2 es una «celda del drive al fondo». Para quitar los drives al fondo habría que separar los golpes dentro de cada partido, y eso cambia lo que N2 mide. Las celdas que sí son «del drive al fondo» (`fondo-max` y `fondo-085`) están en la batería de métricas, que no es N2.
- **Lo que reporto siempre:**
  - **(a)** el N2 de las 9 celdas, completo, como en set22;
  - **(b)** el N2 sin las celdas del drive al fondo. Por lo anterior, el arnés no permite definirlas sin cambiar lo que mide, así que **(b) queda vacío**. Si Rafael quisiera decir «no hay celdas que excluir», (b) sería igual a (a). Eso lo confirma él.
- **(b) es una interpretación pendiente de confirmación de Rafael, y la mía no cambia el criterio:** la banda se aplica tal cual.

### Tabla

Por nivel (Fácil, Medio, Difícil) y por valor, en el formato de set22: una fila por (nivel, valor), con «Antes: b2b» arriba de cada nivel. Fondista / Definidor / Completo en cada celda:
- drive al fondo con P = 1 y con P = 0,85 (% de puntos del humano);
- **% de puntos del humano con el drop** (destacado);
- holgura del rival (s) y % estirado; holgura del humano (s);
- rival no llega (P = 1); pierde en el acto;
- lecturas erradas (%);
- N2 máx. |Δ| frente a `main` / b2b, en las lecturas (a) y (b), y «en la banda».

Más la tasa de error por tipo de golpe (`misread-by-shot.py`), la fila «Fácil: idéntico a hoy sí/no» y la moneda (k = 1) por variante, con «dentro/fuera de 25 ± 2,0».

Ruido: una celda con P = 1 tiene ~740 puntos, así que los % de puntos varían ~±2 pp. Con miles de defensas o lecturas por estilo, sus % varían ~±1–2 pp.

**Lo que espero, antes de medir.**
- En Fácil, m1 y m2 son idénticos a b2b, en las métricas y en las celdas de N2 de Fácil (Δ frente a b2b = 0,00).
- En Medio y Difícil, N2 se moverá como en l1 de set22 (−6 a −26 pp), así que la lectura (a) romperá la banda.

### Salida

La tabla, la conclusión y los riesgos. Si alguna variante queda en la banda (en la lectura que sea), redacto la pregunta de sí o no para Rafael sobre adoptarla, citando qué lectura de N2 aplica. Si no, lo digo y no la redacto. Evidencia en [`docs/validation/evidence/2026-11-28-lectura-largo-medio-dificil/`](../validation/evidence/2026-11-28-lectura-largo-medio-dificil/).

Unity: candado `/Users/rafaallendes/sc-unity.lock` (lo toman `measure-deep-defense.sh` y `rival-time-tests.sh`), un solo Unity, una corrida por paso. Una falla de entorno no es verde: la registro y me detengo. `scripts/device.env` no hace falta (los dos scripts usan el editor de `ProjectVersion.txt`), así que no lo copio.

## Resultados

Medido el 28 nov, en el orden declarado: pruebas, `mc00`, `mr00` y m1. Una corrida por paso, sin reintentos de medición y sin fallas de entorno.

**DETENIDO por la moneda de m1.** En Difícil, la tasa de error en la devolución del saque (k = 1) dio 22,51 %, fuera de 23,0–27,0. Por la regla del encargo, no se corrió el N2 de m1 y no se midió m2. No corregí la moneda.

Evidencia en [`docs/validation/evidence/2026-11-28-lectura-largo-medio-dificil/`](../validation/evidence/2026-11-28-lectura-largo-medio-dificil/):
- `metrics-<corrida>.tsv`, con su `-source.tsv`, su `-head.txt` y su `-levels.txt`, y `defense-cells-<corrida>.tsv`;
- `defense-mr00.tsv.gz`, `defense-m1-Hard.tsv.gz` y `swing-read-m1-Hard.tsv.gz`. Lo demás no se duplica: Fácil de m1 es idéntico a `mr00`, Medio de m1 es idéntico byte a byte a l1 de set22, y `mc00` es idéntico a `q01` (ver `easy-identical.txt` y `control.txt`);
- `coin-k1.txt`, `misread-by-shot.txt`, `swing-read-summary.md` (`summarize-swing-read.py` sobre cada nivel separado con `slice-level.py`), `n2-delta.txt` y `tests.txt`.

**Pruebas** (`tests.txt`):
- RED en `81a09d2`: fallan 3 de 6, por aserción y sin errores CS (el nivel desde la dificultad, Fácil sin lecturas y el rival sin nivel). Las otras tres pasan en RED porque protegen lo que no debe cambiar: el default en 0, Medio y Difícil como con el campo en 0, y Fácil con el campo en 0.
- GREEN en `59295a4`: la suite `SquashClash.Core.Tests.Rally` da 311/323. Pasan las 6 nuevas; fallan las mismas 12 de la base (anclas de hash y defaults de b2b), que no toqué.

**Control b2b: en verde.** `mc00` (head `e40b502`, campos en 0, RallyParams `39bc135edc46`) reproduce `q01` con 0 diferencias (`control.txt`): `cells.tsv` salvo `seconds`, y `rival-time.tsv`, `prev-ball.tsv`, `defense.tsv` y `defense-cells.tsv` byte a byte. `swing-read.tsv` queda vacío.

**Fácil idéntico a hoy, m1: sí** (`easy-identical.txt`). El Fácil de m1 es idéntico al de `mr00`:
- `cells.tsv` salvo `seconds`, con 0 diferencias;
- `rival-time.tsv` (1314 filas), `prev-ball.tsv` (1314), `defense.tsv` (15 510) y `defense-cells.tsv`, byte a byte;
- `swing-read.tsv` de Fácil, con 0 filas.

**Fácil frente a `main`: no es igual, y no lo afirmo.** b2b ya difiere de `main` en Fácil. En N2, el novato se mueve +0,42 / +2,79 / +1,33 pp en Fácil frente a `main` (medición anterior, `n2-delta.txt`), y `RivalRecoverySideShift` actúa en todos los niveles. «Igual que hoy» aquí es igual que b2b.

Además, sin ser criterio: el Medio de m1 es idéntico byte a byte a l1 de set22 (`cells.tsv` salvo `seconds`, `defense`, `prev-ball`, `rival-time`, `defense-cells` y `swing-read`). En Medio y Difícil, el campo de nivel no cambia nada.

### La moneda, k = 1 (`coin-k1.txt`)

| Variante | Medio + Difícil | Medio | Difícil | ¿Dentro de 25 ± 2,0? |
|---|---|---|---|---|
| **m1** (`75241e1`) | 8 195 · 23,06 % | 3 877 · 23,68 % | 4 318 · **22,51 %** | **No (Difícil fuera)** |
| **m2** | no medida | — | — | — |

Fácil: 0 lecturas k = 1, como se esperaba.

### Tasa de error por tipo de golpe y posición, m1 (`misread-by-shot.txt`)

| Grupo | Medio + Difícil | Medio | Difícil |
|---|---|---|---|
| Recto, medio, medio (`medio-suave`) | 58 318 · 24,9 | 29 278 · 24,9 | 29 040 · 25,0 |
| Recto, medio, fuerte (drive al fondo) | 181 046 · 25,0 | 71 531 · 25,0 | 109 515 · 24,9 |
| Drop | 0 · no lee | 0 | 0 |
| k = 1, devolución del saque | 8 195 · 23,1 | 3 877 · 23,7 | 4 318 · 22,5 |
| k = 2, primer golpe tras el saque propio | 7 839 · 25,1 | 4 291 · 25,1 | 3 548 · 25,2 |
| k > 2, resto del rally | 223 330 · 25,0 | 92 641 · 25,0 | 130 689 · 25,0 |

- En Difícil, k = 1 por intención: `fondo-085` 23,1 % (1 895), `fondo-max` 24,5 % (473), `medio-suave` 21,4 % (1 950).
- Fuera de la devolución del saque, la moneda da 25,0 % en todos los grupos. El sesgo está solo en k = 1, del mismo lado que en set22 (l1 23,7 % y l2 22,5 % en Medio): el rival lee mal un poco menos.
- **Hipótesis de set22, no probada aquí:** las lecturas k = 1 no son independientes entre celdas, porque comparten las semillas del humano y la situación del saque. Así, el n efectivo es menor que el nominal. No lo investigué más: el criterio estaba fijado y me detuve.

### Tabla por nivel (Fondista / Definidor / Completo)

Defensa: las recepciones del rival a las que llega tras un drive al fondo, con P = 1 y P = 0,85 juntas. La holgura es la mediana. «Rival no llega» y «pierde en el acto» van en % de esas recepciones. N2 (a): las 9 celdas, completo; (b): sin las celdas del drive al fondo, vacía (ver la declaración). **(b) es una interpretación pendiente de confirmación de Rafael.**

| Nivel · valor (head, RallyParams) | Drive al fondo P = 1, % | P = 0,85, % | **Drop del humano, %** | Holgura del rival (s) · estirado, % | Holgura del humano (s) | Rival no llega (P = 1), % | Pierde en el acto, % | Lecturas erradas, % | **N2 (a) máx. \|Δ\| frente a main / b2b, pp** | N2 (b) | ¿En la banda? |
|---|---|---|---|---|---|---|---|---|---|---|---|
| **Fácil · Antes: b2b** (`mr00`, `e40b502`, `39bc135edc46`) | 95,8 / 96,8 / 96,2 | 92,5 / 94,0 / 94,5 | **58,7 / 40,6 / 47,6** | 0,000 / 0,000 / 0,000 · 57,6 / 87,7 / 78,9 | 0,700 / 0,733 / 0,717 | 12,9 / 11,6 / 11,8 | 36,2 / 36,2 / 39,4 | — | **2,79 / —** (Definidor; `n2-b2b`) | vacía | Sí |
| Fácil · **m1** (`75241e1`, `e9f7680daa7d`) | 95,8 / 96,8 / 96,2 | 92,5 / 94,0 / 94,5 | **58,7 / 40,6 / 47,6** | 0,000 / 0,000 / 0,000 · 57,6 / 87,7 / 78,9 | 0,700 / 0,733 / 0,717 | 12,9 / 11,6 / 11,8 | 36,2 / 36,2 / 39,4 | no lee (0 lecturas) | **no medido** (detenido) | vacía | — |
| **Medio · Antes: b2b** (`mc00`, `e40b502`, `39bc135edc46`) | 90,4 / 96,4 / 93,8 | 92,1 / 96,7 / 94,6 | **48,6 / 22,8 / 34,8** | 0,008 / 0,000 / 0,000 · 39,3 / 83,1 / 62,2 | 0,817 / 0,850 / 0,842 | 11,6 / 9,9 / 12,3 | 25,7 / 16,4 / 22,1 | — | **4,40 / —** (Completo; `n2-b2b`) | vacía | Sí |
| Medio · **m1** | 80,7 / 90,4 / 91,6 | 63,1 / 53,5 / 58,4 | **48,6 / 22,8 / 34,8** | 0,167 / 0,058 / 0,100 · 15,1 / 16,6 / 15,7 | 0,575 / 0,542 / 0,625 | 4,9 / 6,3 / 6,1 | 10,5 / 6,8 / 9,1 | 25,2 / 24,7 / 25,1 · el drop no se lee | **no medido** (detenido) | vacía | — |
| **Difícil · Antes: b2b** (`mr00`, `e40b502`, `39bc135edc46`) | 93,0 / 97,4 / 93,7 | 90,8 / 95,9 / 93,5 | **41,0 / 9,4 / 8,0** | 0,017 / 0,000 / 0,000 · 36,7 / 64,6 / 45,9 | 0,825 / 0,900 / 0,967 | 4,8 / 5,9 / 4,8 | 14,4 / 13,6 / 13,4 | — | **4,60 / —** (Completo; `n2-b2b`) | vacía | Sí |
| Difícil · **m1** | 78,7 / 93,0 / 89,5 | 46,8 / 45,4 / 38,0 | **41,0 / 9,4 / 8,0** | 0,167 / 0,042 / 0,075 · 8,3 / 15,3 / 13,1 | 0,650 / 0,625 / 0,733 | 3,2 / 3,0 / 2,4 | 6,7 / 4,5 / 4,5 | 25,0 / 24,9 / 24,9 · el drop no se lee | **no medido** (detenido) | vacía | — |
| Fácil: idéntico a hoy | **m1: sí** | | | | | | | | | | |
| Moneda k = 1 (25 ± 2,0) | **m1: fuera** (Difícil 22,51 %; Medio 23,68 %; los dos niveles juntos, 23,06 %) | | | | | | | | | | |

- m2 no se midió (la moneda de m1 cayó fuera).
- En las filas «Antes», el N2 es b2b frente a `main`, medido antes de este encargo; frente a b2b no aplica.

El drive medio suave (`medio-suave`) sí se lee. El humano gana:
- en Medio, 28,5 / 32,7 / 32,3 % con b2b y 28,5 / 7,7 / 20,7 % con m1;
- en Difícil, 10,1 / 13,7 / 5,1 % con b2b y 10,7 / 2,4 / 3,4 % con m1;
- en Fácil, lo mismo que con b2b.

### Lectura

- **Fácil queda exactamente como b2b.** Con el campo en 1, el rival Fácil no lee: no hay lecturas ni monedas, y todas las salidas son iguales byte a byte. El campo de nivel hace lo que se pidió.
- **Medio es l1 de set22, idéntico byte a byte.** El rival llega con holgura al drive al fondo; el drive con P = 0,85 cae del 92–97 % al 54–63 %; el drop del humano no cambia.
- **Difícil va en la misma dirección, algo más fuerte.** El drive con P = 0,85 cae del 91–96 % al 38–47 %, y el rival pierde en el acto el 4,5–6,7 % (13–14 % con b2b). El drop del humano no cambia (41,0 / 9,4 / 8,0), porque el rival no lee ningún drop.
- **Detenido antes de N2.** No sé cómo se mueve el novato en Medio y Difícil con m1. En set22, l1 movió esas celdas entre −6 y −25 pp frente a `main`, y m1 en Medio es l1 exacto. Es una referencia, no una medición de m1.

**Conclusión.**
- El diseño funciona: Fácil queda igual que hoy (b2b), y Medio y Difícil leen como l1.
- La variante obligatoria se detuvo por la regla de la moneda: k = 1 de Difícil dio 22,51 %, fuera de 23,0–27,0. Por eso no hay N2 en ninguna de las dos lecturas, (a) o (b), ni m2.
- Ninguna variante tiene N2 medido, así que ninguna queda en la banda, y no redacto la pregunta de sí o no para Rafael.

### Riesgos y lo que elegí yo

- **Detención.** La causa probable es la de set22: las lecturas k = 1 no son independientes entre celdas, así que la banda fija de ± 2,0 pp, con n ≈ 4 300 por nivel, es estrecha para ese n efectivo. No está probada. Medio y Difícil juntos dieron 23,06 %, dentro por 0,06 pp. Si SC-PM o Rafael dan por buena esa explicación, o cambian la regla, el N2 de m1 se corre desde `75241e1` con el árbol limpio (`scripts/measure-deep-defense.sh m1 n2`), y después m2. Esa decisión no es mía.
- **Cómo se sabe el nivel.** `RivalAi` deduce el nivel de la temperatura de la dificultad configurada, porque N2 crea el rival sin pasar el nivel. Si un día dos niveles tuvieran la misma temperatura, el nivel quedaría desconocido y, con el campo en 1, el rival no leería en ninguno de ellos. Con los valores actuales (1,2 / 0,6 / 0,3), la prueba 2 cubre los 3 niveles × 3 estilos y el escalado por k.
- **Lectura (b) de N2:** queda vacía por cómo está hecho N2 (ver la declaración). Es una interpretación pendiente de Rafael y no cambia el criterio.
- **Lo demás de set21 y set22 sigue igual:** la señal del swing, la moneda con `DeltaMs` y la lectura de la intención ideal. Todo está solo en `RallySim`; el juego real (`RallyDirector`) no lo lee.
- **Hashes.**
  - RallyParams: `39bc135edc46` con los cuatro campos neutros (la cabeza final, `19f6856`, tiene el mismo código que `59295a4`), y `e9f7680daa7d` en m1. En set22 era `c932fc94ec91` y en `main`, `ebf8f717f8c5`.
  - Defaults `207da8f2c636` y SimulationKey sin cambios.
- **Revisión.** El código lo implementó un subagente Sonnet dirigido por mí, así que la revisión no es independiente. Sin Codex, por el encargo: no hay revisión cruzada.
- **Excepción a la regla.** La lectura del swing rompe «el rival lee solo la pelota» por la excepción de Rafael para este experimento. La rama no se fusiona y `main` no cambia.
- `scripts/device.env` no se copió: no hizo falta.
