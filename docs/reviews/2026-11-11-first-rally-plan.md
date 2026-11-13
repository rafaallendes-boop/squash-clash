# First Rally — autorización del PO y plan de R1–R5 (11 de noviembre de 2026)

Rama `firstrally/gameplay`, worktree propio. Implementa Claude (Fable). PR en borrador, sin fusionar. Este documento es el primer commit de la rama; el informe de cierre irá en otro archivo de `docs/reviews/`.

## 1. Autorización literal del PO

> AUTORIZACIÓN: autorizo First Rally, etapas R1–R5 y, si queda tiempo, R6 (First Match). Haz un commit por etapa con tests verdes, para que yo pueda cortar donde quiera. Mi uso de Fable es limitado: prioriza avance funcional y commitea seguido.

Condiciones del mismo encargo, resumidas sin cambiar su sentido:

- **Zonas protegidas:** spec, escenarios, umbrales y criterios; SimulationKey `166a96e319c7`, los 41 valores heredados (`0d39e6016bd8`), M2-4 y `power.pullFullMm=35`; el timing (TimingRingView, ImpactCue, claves `timing.*`, contrato de contacto, reloj, ventana de timing, compensación de latencia, shader Legibility), que es de Codex; `AGENTS.md` y el documento de aprobaciones; PR #32, `device.env`, visibilidad del remoto, logs, builds e identificadores del iPhone.
- **Invariantes:** una sola autoridad del estado de la pelota; la IA respeta §11.2 y usa el mismo contrato de contacto que el humano.
- **Libertad:** arquitectura, módulos, refactors necesarios y claves nuevas [EXPERIMENTO], con cada decisión registrada y sin preguntar.
- **Verificación:** TDD con `scripts/test.sh`, suites completas y `scripts/autoqa.sh mvp` en PASS/0/nivel 1 al cerrar cada etapa.
- **Sin instalar en el iPhone.** Queda lista la build dev y el PO autoriza la instalación.
- **El rally se valida headless**, no por jugabilidad humana: el timing aún no está calibrado (S4).

## 2. Base de la rama

El encargo dice «desde main `18b796a`». Al empezar, `main` y `origin/main` estaban en `a844c76` (merge de la PR #45, S4), que contiene `18b796a`. La rama parte de **`a844c76`**: es el `main` vigente, incluye el trabajo de timing ya fusionado y evita conflictos posteriores en `DevPanelView` y `ShotSection`.

## 3. Diseño

### 3.1 Principio: usar el golpe tal como está

El golpe del humano (gesto M2-4, anillo, contrato, rollback, feedback) no se toca. El rally se construye alrededor:

- **Cada golpe del rival es un `Launch` del host** desde el estado exacto de la pelota en su contacto, con la velocidad que resuelve `ShotSolver`. Para `ShotController` es una recepción más: planifica, muestra el anillo y aplica el golpe del humano por la vía de siempre.
- **El host sigue siendo la única autoridad de la pelota.** El director del rally solo le pide lanzar; no mueve la pelota.
- **La posición del cuerpo del humano entra por `human.x`/`human.z`** de la configuración del lanzamiento, igual que hace hoy el modo práctica. El planificador de movimiento decide ese punto.

### 3.2 Módulos nuevos

Núcleo puro en `Assets/SquashClash/Core/Rally/` (mismo asmdef `SquashClash.Core`, sin dependencias de escena):

| Pieza | Etapa | Función |
|---|---|---|
| `PointRules` | R1 | Eventos terminales, cierre único, botes, turno, líneas del retorno |
| `PlayerBody`, `ReceptionPlanner` | R2 | Cinemática del personaje, punto de contacto, holgura, recuperación a W |
| `FondoAi`, `ShotOptions`, `AiObservation` | R3 | Percepción, utilidad softmax, ruido de intención y de timing |
| `RallySim` | R4 | Rally completo headless con dos cerebros, para la simulación de ≥1000 rallies |
| `RallyParams` | R1–R4 | Parámetros [EXPERIMENTO] del rally, con hash propio |

Capa de escena en `Assets/SquashClash/Runtime/Rally/`: `RallyDirector` (loop sobre el host) y `RallyHud` (R5).

### 3.3 Contraste con la spec v0.3

| Spec | Qué se implementa | Desviación y motivo |
|---|---|---|
| §8.5 cierre del punto | Primer evento terminal: segundo bote, tin, out, no llega al frontis. Cierre único; lo posterior se ignora y se cuenta. Botes a 0 con cada contacto de raqueta. Turno de golpe. | Una pelota que queda rodando tras un bote cuenta como segundo bote: P2 deja de emitir botes cuando la pelota ya no rebota y el punto no cerraría nunca. |
| §8.7 líneas del retorno | Tin, out del frontis, out lateral (diagonal) y out trasero por huella, con pruebas en el borde. Línea de saque y líneas del suelo no se juzgan en el rally. | Huella de **20 mm** (radio de la pelota) en vez de ~10 mm: el planificador de contacto existente, que no se toca, ya juzga con ese radio. Con 10 mm habría pelotas buenas para la regla e ilegales para el planificador. Queda como parámetro. |
| §9.1 planificador | Candidatos sobre la trayectoria predicha (tras frontis y un bote), cada ~20 ms; llegada `t_arr` con reacción, aceleración, velocidad máxima y frenado; holgura `s = T* − t_arr`; puntuación con holgura, altura preferida y separación de paredes; calidad de llegada `q`. Asistencia extra = 0. | **Sin estocada** en esta entrega. **K se evalúa con el cuerpo en el punto de llegada**, no con su posición instante a instante: el planificador de contacto existente trabaja con un cuerpo fijo. Soltar antes de llegar puede dar contacto con el cuerpo aún en camino. **Sin replanificación ni histéresis:** con P2 la predicción coincide bit a bit con la simulación hasta el siguiente contacto de raqueta, así que el plan no cambia. |
| §9.2 recuperación | Tras el golpe, vuelta a W con la misma cinemática. W = la T para el humano. | W elegida por el jugador (§9.3) no entra en R1–R5. |
| §10 jugadores | Humano y rival con los mismos atributos: mismo alcance, misma cinemática, mismo contrato. | Sin atributos por personaje (la spec los deja para después). |
| §11.2 información | La IA recibe un `AiObservation` con solo lo observable (pelota retrasada `d_perc`, posiciones y velocidades de ambos, estado de reglas). Prueba de que no ve gesto, mira, potencia ni Δ del humano. | — |
| §11.3 estructura | Opciones {paralelo, cruzado, drop} × {baja, media, alta} × {media, fuerte}; utilidad por rasgos; softmax con temperatura; defensa obligatoria con poca holgura; `σ_int` sobre la intención; `r = T* + Δ`, `Δ ~ N(0, σ_timing)`; un intento; decide el mismo `ContactContract`. | `σ_pred` se aplica como ruido en el punto donde se planta la IA. Sin volea (§15: First Rally base sin volea). |
| §11.4 estilo Fondo | Peso alto de largos, drop solo con el humano atrás y holgura alta, aversión al riesgo alta, espera 0,5 m detrás de la T. | Pesos [EXPERIMENTO] de partida. |
| §11.5 dificultad | Fácil, Medio y Difícil; Medio por defecto. Solo cambian `d_perc`, `σ_pred`, `σ_int`, `σ_timing` y temperatura. | — |
| §15 First Rally | Lanzamiento neutro hacia el humano, intercambio hasta evento terminal, reinicio. Sin saque, sin volea, energía fija. | El experimento de volea (V1, V2+V3) y W no entran en R1–R5. |

### 3.4 Parámetros nuevos y `DefaultPreset.json`

**`DefaultPreset.json` no se modifica** (107 claves, `c0d8ec7050e0`, P0 `6ecfdfa78cbb`). AutoQA ancla ese hash en `Tests/AutoQA/Scenario.cs` y en el verificador del operador, que están protegidos: agregar claves al catálogo cambiaría el hash y dejaría AutoQA en nivel 3. Los parámetros del rally viven en `RallyParams` (Core), con valores por defecto en código, marcados [EXPERIMENTO] y con hash propio que se documenta en el informe. Consecuencia: no aparecen en el panel DEV. Pasarlos al catálogo es una decisión posterior del PO, porque obliga a actualizar los anclajes de AutoQA.

### 3.5 Riesgos conocidos

- El contacto del rival se aplica al final del frame en que la simulación alcanza su paso: se relanza desde el estado histórico exacto de ese paso, así que la física es exacta y la pelota puede mostrarse hasta un frame atrás en ese instante.
- Un doble bote contra el humano no se confirma hasta que su golpe ya no puede llegar con retraso (orden resuelta o ventana de rollback vencida).
- El modo práctica sigue siendo el arranque por defecto: los tests y AutoQA dependen de eso. El rally se activa desde el HUD.

## 4. Plan por etapa

Un commit por etapa, con RED→GREEN, suites completas y AutoQA antes de cerrarla.

1. **R1 — Reglas del punto.** `PointRules` y pruebas: cada evento terminal, cierre único, botes, turno, bordes de línea por superficie.
2. **R2 — Movimiento.** `PlayerBody` y `ReceptionPlanner`: llegada, holgura, inalcanzable, recuperación a W, simetría humano/rival.
3. **R3 — IA Fondo.** Opciones, utilidad, softmax, ruidos, contrato compartido, prueba de datos prohibidos, tres dificultades.
4. **R4 — Loop.** `RallySim` headless y `RallyDirector` en escena; simulación de ≥1000 rallies con informe (terminan, duración, causas, cero bloqueos).
5. **R5 — UX mínimo.** HUD de una línea (golpes, turno, causa), pausa/reanudar, resultado y revancha.
6. **R6 — First Match, si queda tiempo.** Saque y sus líneas, rotación, PAR a 7 con diferencia de 2, pausa entre puntos, revancha.

Al cierre: build dev lista sin instalar, informe y checklist de playtest.
