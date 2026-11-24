# Dos paredes «a veces sale un poco alto»: diagnóstico

24 nov 2026. Ítem 2 del set (prioridad de SC-Lead, vamos de Rafael 15:07). **Solo diagnóstico:** no se cambió código de producto, umbrales, criterios ni física. Lo hizo Claude (Opus) y no hay revisión independiente. Rama `docs/two-wall-high-diagnosis` desde `3e647c4`. Rutas relativas a `Assets/SquashClash/`.

## Fuentes

- Métricas del iPhone, build 1174 (`0.1.0+f306d70 dev`), en `metrics-20261124T175940193-….jsonl` (SHA-256 `b6bef4208566…`; copia de SC-PM fuera del repo, en `sc-specs/out/metrics-1174/`). Como contexto: el playtest del 23 nov (`metrics-20261123T174154689-…`, 1 dos paredes) y los archivos del 20–21 nov del respaldo `install-518f9c3` (13 dos paredes, builds `aba4e2c` y `fceb3de`).
- Diseño y metas: [predeclaración](../superpowers/specs/2026-11-20-boast-predeclaration.md) y [análisis](../superpowers/specs/2026-11-20-two-wall-shot-analysis.md). Informe: [boast](2026-11-20-boast.md).
- Código: `Core/Shot/BoastSolver.cs`, `Core/Shot/CausalQuality.cs:110-124`, `Core/Shot/ShotPace.cs:41-53`, `Core/Shot/FrontisProjection.cs:45-63` y `Runtime/Shot/ShotController.cs:331-374`.
- Simulación determinista con la P2 y el `BoastSolver` de producción, sin tocar parámetros. `DefaultPreset` `207da8f2c636`/132; P0-calib-1 hereda todos los `physics.*`, `shot.*` y `causal.*`. Se corrió una vez en Unity con el candado: Passed 1/1. La [evidencia](../validation/evidence/2026-11-24-dos-paredes-alto/) incluye el código del experimento (`.cs.txt`, no versionado como código), las entradas, las salidas y los SHA-256.
- **Comprobación del replay:** al lanzar desde el contacto y la velocidad registrados, la P2 reproduce exactamente el punto del frontis registrado en los 12 dos paredes resueltos.

## 1. Los dos paredes de los 2 partidos

Hubo **2**, los dos del humano, ambos de revés, con la pared izquierda (x = 6,40) y desde el bote. Ninguno cayó en el respaldo al frontis. Del rival, **no registrado**: `rally-causal` no marca si el apuntado fue lateral.

| | Partido 1, punto 1 (18:02:59Z) | Partido 2, último punto (18:06:43Z) |
|---|---|---|
| Contacto (x, y, z) | 6,19 / 0,26 / 9,03 (rincón trasero) | 3,21 / 0,30 / 4,36 (centro) |
| Potencia, toque | P 1,00; +70 ms (tarde) | P 0,73; −160 ms (temprano, fuera del ámbar) |
| Altura tocada en la lateral → la usada | 1,71 → **2,24 m** (z 1,07). Capa causal: timing +0,13, estiramiento +0,40 | 2,06 → **2,25 m** (z 2,73). Timing +0,19 |
| **Frontis** | **2,39 m**, x 6,37: la misma esquina, no cruza | **3,49 m**, x 1,47: cruza a la otra mitad |
| Bote (simulado) | z **7,49 m** (fondo), a 1,8 s reales | z 1,28 m (adelante), a 2,6 s reales |
| Altura del rebote tras el bote (simulada) | 0,50 m; pega en la pared del fondo a 0,50 m | 0,67 m |
| Resultado | Válido. El rival la devolvió desde el fondo, y el punto siguió hasta 9 golpes; lo ganó el rival (doble bote, el humano no golpeó) | Válido. El rival llegó 2,8 s después y jugó una dejada; el humano no alcanzó. Fue el 3–7 final |

- **Qué es lo esperado según la spec.** La predeclaración solo fija la validez: lateral → frontis, entre la tin + 0,10 (0,58 m) y la línea de fuera − 0,10 (**4,47 m**), sin bote antes y legal hasta el primer bote. Además, el ≥ 80 % de los válidos debe cruzar a la mitad contraria (M1b), medido en la grilla y no por golpe. **No fija** una altura del frontis, una altura de bote ni una profundidad. Los dos son válidos por la spec. El primero no cruza. Lo que se espera del golpe (la pelota «muere» adelante) solo está implícito en el análisis §1 y en las opciones del rival (`ShotOptions.cs:69`, puntos laterales a 0,9 y 1,5 m, `RallyParams.cs:101`).
- **Contexto.** En esos partidos, los 34 golpes del humano al frontis van a una mediana de 1,30 m y un máximo de 3,39 m. El segundo dos paredes (3,49 m) quedó por encima de todos. El 20 nov ya hubo uno a **4,46 m** (toque a 2,26 m, desde el bote, P 0,67), casi en el límite.
- **No registrado:** la altura y el lugar del bote (`launch` va vacío). Por eso se reconstruyen con el replay determinista.

## 2. Por qué sube

La causa es una cadena de cuatro eslabones, con evidencia en el código y en la simulación.

1. **La altura que se toca en la lateral se conserva.** El apuntado lateral es un punto (z, y) de esa pared (`FrontisProjection.TryMapSide`). Se admite desde el suelo hasta la línea de fuera lateral (4,57 → 2,13 m), igual que una altura del frontis. Rafael tocó a 1,71 y 2,06 m. Son alturas normales para un apuntado al frontis: en esos partidos, sus toques al frontis tienen una mediana de 1,22 m y un cuartil superior de 2,45 m.
2. **El solver la amplifica.** `BoastSolver.Launch` toma la raíz de arco bajo a la velocidad del gesto, que suele ser rápida (20–21 m/s en los dos). Desde un contacto bajo (0,26–0,30 m, tras el bote), la pelota **llega a la lateral todavía subiendo** y sigue subiendo hasta el frontis. La velocidad vertical al llegar es ≈ h/t − g·t/2, que es mayor cuanto más rápido sea el golpe y más suba. La subida extra crece con la distancia que queda entre la lateral y el frontis (la z del punto) y con la potencia.
   - Barrido en el contacto del partido 2: lateral 1,8 → frontis 2,16; 2,0 → 2,76; 2,2 → 3,34; 2,4 → 3,90 m. Son **≈ 2,8 m de frontis por cada metro de lateral**.
   - En el del partido 1, con el punto lateral a 1,07 m del frontis, la ganancia es ≈ 1,1.
   - Grilla de 827 dos paredes resueltos con el timing ideal: el frontis tiene una mediana de 1,00 m si se toca a 0,9 m, de 2,00 m a 1,5 m y de 2,90 m a 2,1 m. Con un toque a 2,1 m y P 0,75, el 90 % pasa de 2,5 m.
   - Los dos paredes de volea, con contacto alto, no suben. En los 6 de los datos del 20 nov con contacto a 0,9 m o más, el frontis quedó a la altura de la lateral o por debajo. La subida aparece desde el bote.
3. **La capa causal sube el punto lateral, y esa subida también se amplifica.** `CausalQuality.SolveBoast` compone los vectores de timing, estiramiento y cansancio sobre la altura lateral, sin ningún tope en el frontis.
   - Sin la capa, el frontis habría sido de 1,79 m (en lugar de 2,39) en el partido 1 y de 2,94 m (en lugar de 3,49) en el partido 2.
   - Es decir, +0,53 m en la lateral dieron +0,60 m en el frontis, y +0,19 m dieron **+0,55 m**.
4. **Nada lo corrige ni lo anticipa.** La verificación solo rechaza por encima de 4,47 m. La búsqueda del «más cercano» solo baja la altura si el golpe es inválido. Mientras el dedo está apoyado, la retícula solo muestra el anillo en la lateral; la cruz del frontis aparece al resolver (`ShotController.cs:364-374`). El tutorial no dice nada de la altura (`FirstRunFlow.cs:53`).
   - Además, una potencia alta lo vuelve largo. En el partido 1 (P 1,0) el bote cayó en el fondo aunque el frontis fuera de 2,39 m. En la grilla, con P 1,0 el 75 % bota pasada la media cancha, y con P 0,3 ninguno.

La precisión en pantalla no explica la diferencia: un metro de altura ocupa ≈ 4,3–6,2 mm en la lateral y 4,1 mm en el frontis. Lo que cambia es la amplificación, no la puntería.

## 3. Opciones de arreglo, en orden

| # | Opción | Qué resuelve | ¿Toca algo reservado? |
|---|---|---|---|
| 1 | **Vista previa del frontis durante el toque.** Resolver el dos paredes desde P\* al apoyar el dedo, como el pronóstico del rival (`RallyContact.ForecastBoast`), y mostrar la cruz con su altura antes de soltar. | El jugador ve que tocar alto en la lateral es pegar alto en el frontis. No cambia ningún golpe. | **No.** Es feedback e implementación; el solver se usa sin cambios. Riesgo: la previa sale sin la capa causal, igual que en el frontis. |
| 2 | **Texto del tutorial y de la pista** (`FirstRunFlow.cs:53`, `FirstMatchHints.cs:47`): «toca bajo en la lateral; si tocas alto, sube y queda larga». | Educa sobre la causa 1. Es barato. Combina con la 1. | **No.** Solo es texto. |
| 3 | **Tope de altura en el frontis para el dos paredes,** dentro de la verificación (p. ej. la línea de saque, 1,78 m). La búsqueda que ya existe bajaría el punto lateral. | Corta las causas 2 a 4 para el jugador y el rival. | **Sí: es un umbral o criterio nuevo en el solver** (§4). Además mueve el respaldo, M3 y M4 del rival. Decide Rafael. |
| 4 | **Cambiar el significado de la altura tocada:** que el toque elija la pared y la profundidad, y que el solver busque la altura lateral que da un frontis bajo. | Elimina la amplificación como entrada del jugador. | **Sí: cambia el diseño fijado** («el apuntado es un punto de la lateral», predeclaración §1–2) y el solver. Decide Rafael. |
| 5 | **Capa causal en el dos paredes:** no componer la subida sobre la lateral, o acotarla. | Quita los +0,55–0,60 m del mal toque. | **Depende.** No cambia los valores `causal.*`, pero sí el cálculo del golpe y cómo se castiga el error. Decide Rafael. |
| 6 | **Bajar la velocidad del dos paredes** (`ShotPace.Boast`). | Menos subida y botes más cortos. | **Sí: cambia el ritmo del golpe**, y los criterios M1 y M4 se vuelven a medir. Decide Rafael. |

**Recomendación:** hacer 1 + 2 primero, porque no tocan nada reservado. Después, con una prueba en el iPhone, Rafael decide si hace falta la 3 o la 4.

**Límites:** n = 2 en los partidos de hoy (16 con el contexto). Un solo jugador y un solo estilo de rival (Completo, Medio). Los botes son simulados, no medidos. Los dos paredes del rival no se pueden identificar en las métricas.
