# Cansancio que se siente: diseño y medición predeclarada

20 nov 2026 · Subagente Opus dirigido por Claude (product lead, delegación del PO del 18 nov); **la revisión de Claude no es independiente**. Rama `claude/fatigue-feel`, base `9e4da8a` (integración con volea v2.2, sin fusionar). Parte de la spec de cansancio v2.3 (`2026-11-16-fatigue-design.md`) y de la volea v2.2 (`2026-11-20-volley-v2-design.md` §7).

**Informe del PO (20 nov, un partido en `fb80015`), literal:** «traté de volear todo y lo logré. Pero al ir bajando mi barra (llegó a 0 en un momento) el jugador se seguía moviendo igual. Entonces la barra de energía no está haciendo su función.» Antes: «Lo de que se gasta más el jugador mantengamoslo» y «Mejorar lo que hay hoy es la prioridad».

## 0. Diagnóstico (del código, antes de medir)

1. **Los efectos son suaves por diseño.** Con energía E y `f = clamp((onset − E)/onset, 0, 1)` (onset 70): velocidad y aceleración × (1 − `legsMax`·f), con `legsMax` 0,1, y reacción + `reactionMaxMs`·f, con 40 ms. En E = 0: 10 % más lento y 40 ms más de reacción.
2. **El sprint de la volea no mira la energía.** `PlayerBody` multiplica la velocidad por `MovementFactor × Sprint`. Con `HumanVolleySprintMax` = 2, un humano en E = 0 corre a 0,9 × 2 = **1,8 ×** hacia cada volea tardía. El costo (`HumanVolleySprintCost` × tardanza) se paga, pero lo que compra no empeora. Como el PO voleaba todo, casi todo su movimiento visible era sprint.

## 1. A: la energía limita el sprint (solo humano, activado por defecto)

**Regla.** Sprint máximo efectivo de la recepción = `1 + (HumanVolleySprintMax − 1)·(1 − f)`, con f el factor del humano **al empezar la recepción** (el mismo f congelado que ya usan piernas y reacción). Sprint completo en E ≥ 70; nada en E = 0 (en E < 10, menos de 1,15 ×). En f = 0 el código devuelve `HumanVolleySprintMax` sin operar.

- S′ (plazo de compromiso con sprint) y la ventana ámbar se calculan con ese sprint, así que se acortan con el cansancio. En f = 1, S′ = S, con S el de v2.2 (la última postura alcanzable sin sprint), que no cambia.
- El costo del sprint (4 × tardanza) no cambia.
- **Campo nuevo de `RallyParams`, solo humano:** `HumanVolleySprintFatigue = 1` (1: la energía limita el sprint; 0: v2.2 tal como está). Botón DEV «Sprint según energía sí/no», desde el próximo punto.
- Con el cansancio apagado, o con E ≥ 70, el sprint es bit a bit el de hoy (f = 0 da exactamente `HumanVolleySprintMax`).
- **Hash:** la proyección de `RallyParams` sin el campo nuevo debe ser exactamente `0fe523164363`. Las proyecciones sin `HumanVolley*` siguen en `0f58580f44ea`.

**Corrección antes de medir (20 nov, en el GREEN):** la primera versión, en f = 1, volvía al S de v2 (solo la postura de la oferta), que es más corto: en el saque neutro del test, 28 pasos frente a 75. El test RED ya pedía que S no cambiara; se corrige el código, no la meta.

## 2. B: «Cansancio fuerte», opción solo DEV (los defaults no cambian)

**Regla.** Con el botón DEV «Cansancio fuerte sí», desde el próximo punto, el contexto del rally de **ambos jugadores** usa `fatigue.legsMax × 3` y `fatigue.reactionMaxMs × 2`: 0,30 y 80 ms con el preset (E = 0: 30 % más lento y 80 ms más de reacción). Se aplica sobre la configuración efectiva del rally, no sobre `DefaultPreset.json` ni sobre la práctica. Valores **fijados aquí, antes de medir**; no se ajustan después de ver los datos.

- Variante informativa medida al lado, sin elegirla: × 4 y × 3 (`legsMax` 0,40, el tope del catálogo, y 120 ms).
- Apagado por defecto: rival, calibración y anclas AutoQA intactos.

## 3. C: cansancio visible (opcional, solo vista)

Con E < 25 (zona roja de la barra) la figura baja la cadera y se inclina hacia adelante, más cuanto menor la energía, y respira más hondo cuando está quieta. Solo `CharacterView`, interruptor DEV «Cansancio visible sí/no», apagado por defecto. Ningún cambio en la simulación.

## 4. Medición predeclarada (`FatigueFeelExperiment`, EditMode, `Category("Experiments")`, una sola corrida)

**Común.** `RallySim` con el `DefaultPreset.json` real (cansancio encendido), rival `RivalAi` Medio, los tres estilos en partes iguales. Humano: el bot del experimento de energía de v2.2 (`VolleyEnergyExperiment.Bot`): novato σ 90 ms que toca en cada recepción según la distribución del PO (`T*b − d`, deciles 477…1383 ms) y por eso volea siempre que el toque compromete; 60 % golpes fuertes. Volea v2.2 (`HumanVolleyMode` 3, sprint 2). Semillas fijas.

**Brazos:**

| ID | `HumanVolleySprintFatigue` | Cansancio fuerte |
|---|---|---|
| A-off | 0 | no (hoy) |
| A-on | 1 | no |
| B32 | 1 | × 3 / × 2 |
| B43 | 1 | × 4 / × 3 (informativo) |

**Conjunto 1, energía fijada (como el botón DEV «Energía»).** Antes de cada punto se fija la energía del humano en 100, 55, 25 o 5 y la del rival en 100. 50 puntos por (energía inicial, estilo): 600 puntos por brazo; saca el humano en los puntos pares y el rival en los impares, cuadros alternos.

**Conjunto 2, partidos naturales.** `PlayMatch`, 30 por estilo y brazo, saca primero el humano en los pares. Solo datos.

**Bandas** por la energía del humano **al empezar cada recepción**: ≥ 70, 40–70, 10–40, < 10. Base: recepciones del humano que no son devolución de saque.

**Se informa por banda y brazo:** tasa de volea (recepciones con volea comprometida), tasa de volea con sprint, P+V de la volea, recepciones alcanzables y holgura media, velocidad máxima teórica del cuerpo (4,8 m/s × (1 − legsMax·f) en el centro de la banda, y con sprint). Puntos ganados por el humano según su energía inicial (conjunto 1) y, en el conjunto 2, puntos ganados, partidos ganados y energía al empezar los puntos 5, 10 y 15 de ambos.

**Meta de A (sobre A-on, conjunto 1):** tasa de volea con sprint en E < 10 ≤ **1/3** de la tasa en E ≥ 70. Si hay menos de 100 recepciones en la banda < 10, no es evaluable. Si no se cumple o no es evaluable: se registra y se detiene, sin ajustar nada.

**B no tiene meta:** sus valores ya están fijados (§2). Se informa para que el PO lo pruebe en el iPhone y decida.

## 5. Invariantes y controles

- **Rival bit a bit:** A solo entra en el camino de un cerebro humano que toca (`IRallyVolleyToucher`); B está apagado por defecto. Test: con defaults, partidos de `RallySim` con bots que no tocan dan resultados idénticos con `HumanVolleySprintFatigue` 0 y 1. Se repite `ConfirmPerStyle` y se compara con `confirm01` (sin la columna de segundos).
- Sin cambios en `ContactContract.cs`, ClockMap/SimClock, física P2, `DefaultPreset.json`, SimulationKey `166a96e319c7`, escenarios, oráculos ni anclas de AutoQA.
- Si un control falla o algo sorprende: se registra y se detiene.

## 6. Qué costaría que «Cansancio fuerte» sea el default

Requiere el «sí» del PO y:
1. cambiar `fatigue.legsMax` y `fatigue.reactionMaxMs` en `DefaultPreset.json`: cambia el hash de defaults y las cuatro anclas de AutoQA (como en F1, con proyección exacta);
2. recalibrar el rival: los k de ruido por estilo y nivel se eligieron con el cansancio de hoy (`RivalStylesCalibrationExperiment`, unas 2–4 h de Unity con búsqueda y confirmación);
3. repetir el Experimento B del cansancio (B1–B6) con los valores nuevos.
