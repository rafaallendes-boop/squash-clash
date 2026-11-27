# Dónde espera y se recupera el rival: experimento

27 nov 2026 · Claude (Opus: arquitectura, declaración, medición y revisión; el código lo implementa un subagente Sonnet dirigido por Claude, así que la revisión **no es independiente**) · encargo `set17-posicion-espera` de SC-PM. Es solo un experimento: rama local `rafaallendes-boop/sc-posicion-espera`, sin PR, sin merge y sin push. Rafael decide con los números.

Contexto: `docs/reviews/2026-11-27-tiempo-rival.md`, en la rama local `rafaallendes-boop/sc-tiempo-rival` (`a684463`). Ni la reacción, ni la anticipación, ni el primer paso hacen que el rival llegue al drive al fondo con P = 1. Llega 0,45–0,54 s tarde: el drive pide ~0,8 s de desplazamiento en un presupuesto de ~0,83 s. SC-PM y SC-Lead deciden medir si cambiar dónde espera o se recupera el rival lo deja llegar.

## Declaración (antes de medir)

### Base y arnés

- Base: `origin/main` `99b4306` (RallyParams `ebf8f717f8c5`, defaults `207da8f2c636`).
- De `sc-tiempo-rival` (`a684463`) se traen solo la medición: `DeepDefenseExperiment`, `measure-deep-defense.sh`, `rival-time-tests.sh` y los resumidores `summarize-deep-defense.py`, `summarize-rival-reach.py` y `summarize-rival-time.py`. Las palancas de esa rama (reacción, percepción, postura, paso dividido y aceleración del rival) **no** se traen. Donde el arnés las leía se usan los valores comunes (`ReactionS` y `Move`), que es lo mismo que daban con las palancas neutras.
- **ANTES:** se mide el arnés en esta rama con los defaults neutros. Se coteja con `metrics-base01.tsv` de `sc-tiempo-rival` en todas las columnas salvo la del tiempo de reloj. Si coincide, N2 ANTES es `n2-before01-from-6086074.tsv` (mismo código de producto), como en `sc-tiempo-rival`. Si no coincide, se mide también N2 ANTES.

### Variantes (solo el rival; el humano no cambia)

El rival lee **solo la pelota**. No lee la raqueta, el swing, patrones ni el conteo de golpes. La espera es fija por variante (b1) o depende solo de la pelota que el propio rival acaba de golpear (b2).

Geometría de hoy (Medio). El T está en z = 5,465 m (la pared del fondo está en 9,75). El rival espera a x = T.x + 0,4 y, detrás del T, a 0,5 m (Fondista), 0 m (Definidor) o 0,2 m (Completo). Tras su golpe vuelve a esa espera; si su pelota pica corta, vuelve al T. En las recepciones que no llegan, la postura está a ~3,7–3,9 m. El drive recto del humano baja por la pared del lado donde el humano golpea, así que el camino del rival es en diagonal hacia la esquina del fondo, con ~2,7 m de lateral y ~2,6 m de fondo (estimación mía). A 4,8 m/s cada metro menos ahorra ~0,21 s. Para cerrar 0,45–0,54 s harían falta ~2,2–2,6 m menos.

- **b1: espera más atrás del T.** Se cambian solo los valores de campos que ya existen (`RivalWaitBehindT`, `StyleShooterWaitBehindT`, `StyleAllRoundWaitBehindT`). Cada estilo conserva su diferencia con los otros. La espera se usa al empezar el punto y en cada recuperación tras una pelota profunda. Tras una pelota corta del rival sigue volviendo al T, como hoy.
  - **b1a: +0,75 m.** Fondista a 1,25 m, Definidor a 0,75 m, Completo a 0,95 m detrás del T. Es una espera defensiva habitual, «un paso detrás del T». Estimación mía: ahorra ~0,5 m de camino al fondo (~0,1 s).
  - **b1b: +1,5 m.** Fondista a 2,0 m (z = 7,47, a 2,3 m de la pared del fondo), Definidor a 1,5 m, Completo a 1,7 m. Es una cota: el camino ya es casi todo lateral, así que esperar más atrás casi no ahorra más en el drive (~0,17 s de un techo de ~0,22 s). En cambio, sí alarga el camino al drop.
- **b2: se recupera cargado hacia el lado del drive del humano.** Campo nuevo `RivalRecoverySideShift` (metros, neutro 0). Tras su propio golpe, el rival predice su pelota con el mismo P2 del juego (lo que ya hace hoy para la cobertura). Si el primer pique es profundo (z ≥ `StyleCoverBounceZ` 3,5), se recupera a x = T.x ∓ desplazamiento hacia el lado de ese pique, a la profundidad de su espera. Es el lado donde el humano va a golpear y, por lo tanto, por donde baja su drive recto. Las pelotas cortas siguen como hoy. La espera del saque no cambia.
  - **b2a: 0,8 m.** El rival queda a ~2,4 m de la pared de ese lado, frente a los 3,6 o 2,8 m de hoy (según el lado). Estimación mía: ahorra ~0,6–0,8 m de camino (~0,15 s).
  - **b2b: 1,6 m.** Queda a ~1,6 m de la pared, casi sobre la línea de la postura. Es una cota (~0,2 s): deja abierta la mitad contraria a un cruzado, que la batería no prueba porque el bot solo juega recto.
- Ninguna variante toca la banda, los umbrales, los criterios, los oráculos, las pruebas existentes, el lado humano, `DefaultPreset.json` ni SimulationKey.
- `RivalRecoverySideShift` cambia el hash de RallyParams aunque esté neutro. Las pruebas existentes que anclan proyecciones del hash van a fallar en esta rama (en `sc-tiempo-rival` eran 7). No las toco y lo informo.
- Cada variante es un commit con sus valores como defaults, porque `ConfirmPerStyle` usa `new RallyParams()`. Cada corrida guarda su `head.txt`. Al final, los defaults vuelven a neutros.
- Orden de medición: b1a, b1b, b2a, b2b.

### Batería (la misma de `sc-tiempo-rival`)

- **Métricas:** Medio, los tres estilos, `IntentBot` σ 30 ms, 100 partidos por celda, semillas `20261126`. Intenciones: drive al fondo con P = 1 (`fondo-max`) y con P = 0,85 (`fondo-085`), medio suave y drop.
- **Se informan por estilo** (Fondista / Definidor / Completo):
  - % de puntos del humano con el drive al fondo con P = 1 y con P = 0,85.
  - % de recepciones del rival que no llegan (`Unreachable` o `NoSwing`).
  - Holgura mediana del humano tras una defensa del rival.
  - **Drop del humano** (% de puntos), destacado: esperar más atrás o de lado puede dispararlo.
  - Medio suave y la descomposición del déficit de las recepciones que siguen sin llegar (`summarize-rival-time.py`), como apoyo.
- **N2:** `RivalStylesCalibrationExperiment.ConfirmPerStyle` sin cambios: chequeo previo del Fondista en Difícil, semillas de confirmación `20261219`, 200 partidos por celda, novato σ 90 ms y `bot30`, 3 estilos × 3 niveles. Se informan el Δ por celda del novato frente a la base y el máximo |Δ|.
- **Banda vigente, cita textual** de [2026-11-20-serve-design.md](../superpowers/specs/2026-11-20-serve-design.md) (tabla de criterios, N2): «se informa el desplazamiento; si un estilo se mueve **> 5 pp** en algún nivel (novato), **parada** y reporte, sin recalibrar». No se toca la banda. Si una variante la rompe, se anota y se sigue con la siguiente, igual que en `sc-tiempo-rival`; no se recalibra.
- Unity: candado `/Users/rafaallendes/sc-unity.lock`, un solo Unity, sin reintentos. Una falla de entorno no cuenta como verde: se registra y me detengo.

## Resultados

Medido el 27 nov hasta las 12:35 Chile, en el orden declarado. Evidencia en [`docs/validation/evidence/2026-11-27-posicion-espera/`](../validation/evidence/2026-11-27-posicion-espera/):
- `metrics-<corrida>.tsv`, con su `-source.tsv` y su `-head.txt`;
- `rival-time-<corrida>.tsv`;
- `n2-<variante>.tsv`, con su `-source.tsv` y su `-head.txt`;
- `reach-summary.md` y `rival-time-summary.md`, que salen de `summarize-rival-reach.py` y `summarize-rival-time.py`;
- `tests.txt`.

**ANTES.** El arnés corrió en esta rama con los defaults neutros (`base01`, head `13307c1`, RallyParams `1c0f918ad1a0` con el campo nuevo en 0). Reproduce `metrics-base01.tsv` de `sc-tiempo-rival` en las 12 celdas, en todas las columnas salvo la del tiempo de reloj. `rival-time.tsv` también sale idéntico byte a byte. Por eso N2 ANTES es `n2-before01-from-6086074.tsv`, como estaba declarado.

**Ruido.** Una celda con P = 1 tiene ~740 puntos, así que el error estándar ronda 0,8 pp y las diferencias de hasta ±2 pp son ruido.

### Tabla

Antes (Fondista / Definidor / Completo):
- Fondo con P = 1: 94,0 / 97,0 / 94,7.
- Fondo con P = 0,85: 94,0 / 96,2 / 92,8.
- El rival no llega (P = 1): 12,9 / 21,2 / 18,6 %.
- Holgura del humano: 0,89 / 0,85 / 0,89 s.
- **Drop del humano: 53,1 / 25,8 / 43,6 %.**

| Variante (head) | Drive al fondo P = 1, después | P = 0,85, después | Rival no llega (P = 1), % | Holgura del humano (s) | **Drop del humano %** | N2 máx \|Δ\| (pp) | ¿Dentro de la banda? |
|---|---|---|---|---|---|---|---|
| **b1a** +0,75 m atrás (`ac7bb6a`) | 94,6 / 99,2 / 93,1 | 77,1 / 82,4 / 91,3 | 10,8 / 21,7 / 12,6 | 0,85 / 0,88 / 0,86 | **65,9 / 58,8 / 71,3** | 14,54 (Definidor Difícil) | No |
| **b1b** +1,5 m atrás (`ebba25e`) | 86,1 / 96,6 / 91,7 | 60,6 / 62,4 / 62,2 | 8,3 / 16,4 / 11,9 | 0,77 / 0,81 / 0,76 | **82,1 / 83,5 / 77,5** | 16,44 (Definidor Difícil) | No |
| **b2a** 0,8 m hacia el lado (`2af18ae`) | 94,0 / 96,0 / 94,2 | 93,7 / 95,9 / 92,5 | 11,8 / 11,0 / 12,6 | 0,90 / 0,78 / 0,88 | 55,0 / 25,5 / 38,6 | 2,13 (Fondista Fácil) | Sí |
| **b2b** 1,6 m hacia el lado (`5b10a9f`) | 90,4 / 96,4 / 93,8 | 92,1 / 96,7 / 94,6 | 11,6 / 9,9 / 12,3 | 0,88 / 0,68 / 0,88 | 48,6 / 22,8 / 34,8 | 4,60 (Completo Difícil) | Sí |

### Lectura

- **Ninguna variante baja de forma clara el drive al fondo con P = 1.** Los únicos cambios fuera del ruido son el del Fondista en b1b (−7,9 pp) y en b2b (−3,6 pp). El Definidor sigue en 96–99 % con las cuatro variantes.
- **b1, esperar más atrás, dispara el drop del humano.** El Definidor pasa de 25,8 a 58,8 % con b1a y a 83,5 % con b1b. El Completo pasa de 43,6 a 71,3 % y a 77,5 %. Además rompe N2: el novato pierde hasta 14,5 y 16,4 pp. Lo único que baja con b1 es el drive con P = 0,85, a ~61 % con b1b. Esa baja no viene de que el rival llegue más (con P = 0,85 el rival ya casi siempre llegaba, ~1 % no llega), sino de que devuelve mejor desde atrás. Hay un estilo que no cumple el patrón: el Completo en b1a, que se queda en 91,3 %.
- **b2, recuperarse hacia el lado, es la única dentro de la banda, y no dispara el drop** (incluso lo baja algo en el Completo, de 43,6 a 34,8 % con b2b). El rival llega bastante más (el Definidor no llega en un 9,9 %, frente al 21,2 % de antes), pero el humano gana el punto igual. La holgura del humano tras la defensa del rival casi no baja (0,85→0,68 s en el Definidor con b2b; en el resto, ±0,03 s). **El rival llega, pero su defensa no complica al humano.**
- **Las recepciones que siguen sin llegar no se acercan.** Con b2b, la distancia mediana sigue en 3,8–3,9 m y el déficit en ~0,6 s, igual que antes (`rival-time-summary.md`). Mi hipótesis, no verificada: en esas recepciones el desplazamiento no se aplica o no alcanza (por ejemplo, si la pelota anterior del rival fue corta, si picó del otro lado o si venía del saque). El arnés no registra la pelota anterior, así que no lo puedo confirmar.
- **El novato en N2 gana un poco con b2** (hasta +4,6 pp en el Completo en Difícil). Es coherente con que el rival deje abierta la otra mitad: el novato no juega solo recto como el bot.

**Conclusión.** Cambiar dónde espera o se recupera el rival no basta para que llegue al drive al fondo con P = 1, al menos en estos valores. b1 no sirve: abre el drop y rompe N2. b2 está en la banda y el rival llega más, pero el humano sigue ganando esos puntos, porque la defensa a la que llega es pobre. Solo en el Fondista con b2b el drive baja algo (−3,6 pp).

### Preguntas para Rafael (no implementadas)

1. **Calidad de la defensa.** Con b2 el rival llega, pero pierde el punto igual. ¿Quieres medir por qué la defensa a la que llega le deja al humano ~0,9 s de holgura? Por ejemplo, el golpe que elige o la calidad del contacto cuando llega justo. Se mediría sobre b2, sin leer al humano.
2. **Las recepciones que no se acercan.** ¿Instrumento la pelota anterior del rival en las recepciones que siguen sin llegar? Serviría para saber si el desplazamiento de b2 no se aplicó o si no alcanza.

### Riesgos y lo que elegí yo

- **b2 abre el cruzado.** La batería no lo prueba porque el bot solo juega recto. En N2 el novato gana hasta +4,6 pp (dentro de la banda). Con un jugador real que cruce, el efecto puede ser mayor.
- **b1 también mueve la espera del saque.** `RivalWaitBehindT` es además la espera común del rival para los cerebros sin estilo. No afecta a la batería, pero sí cambiaría el juego si se adoptara.
- **Hashes.** `RivalRecoverySideShift` cambia el hash de RallyParams aunque esté en 0 (`1c0f918ad1a0` frente a `ebf8f717f8c5` en `main`). En esta rama fallan las 7 pruebas existentes que anclan proyecciones del hash. No las toqué. Defaults `207da8f2c636` y SimulationKey sin cambios.
- **Pruebas** (`tests.txt`):
  - RED en `4ac9983`: 2 de 5 fallan por aserción. GREEN: 5/5.
  - El GREEN corrió con la implementación todavía sin commitear sobre `4ac9983`. Se commiteó igual en `c65edd7`.
  - La suite `SquashClash.Core.Tests.Rally` en `c65edd7` da 298/305: fallan solo las 7 anclas de hash.
  - El commit final `12e55d7` vuelve a los defaults neutros y tiene el mismo código que `c65edd7`.
- **Mismatch.** En b1b, una celda (Fondista, `fondo-085`) tiene 2 puntos con mismatch de alineación, de 1 131. Quedan fuera solo de las estadísticas de contacto; el % de puntos no cambia.
- **Valores que elegí yo** (justificados en la declaración):
  - b1a +0,75 m, una espera habitual.
  - b1b +1,5 m, una cota.
  - b2a 0,8 m.
  - b2b 1,6 m, una cota.
  - En b2 el desplazamiento se mide desde T.x, sin el desfase de 0,4 m que solo sirve para que no se superpongan los cuerpos, para que sea simétrico en los dos lados.
- **Revisión.** El código lo implementó un subagente Sonnet dirigido por mí, así que la revisión no es independiente. Falta la revisión cruzada de Codex.
- **Un fallo mío, sin efecto.** El primer lanzamiento de mi script de variantes falló antes de aplicar ni medir nada, porque el bash de macOS no tiene arreglos asociativos. Lo corregí y lo relancé. No es un reintento de una medición.
- **Lo que faltó:** nada del encargo.
