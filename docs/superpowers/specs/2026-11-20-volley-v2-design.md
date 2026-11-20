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

## 1. Diagnóstico

_(Se completa con la medición.)_

## 2. Propuesta

_(Se completa con la medición.)_

## 3. Impacto

_(Se completa con la medición.)_
