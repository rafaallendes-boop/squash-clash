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

Pendiente.
