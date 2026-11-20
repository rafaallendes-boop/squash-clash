# Cansancio que se siente: informe

20 nov 2026 · Subagente Opus dirigido por Claude (product lead); **la revisión de Claude no es independiente**. Rama `claude/fatigue-feel`, base `9e4da8a`, sin push, fusión ni instalación. Diseño predeclarado: [spec](../superpowers/specs/2026-11-20-fatigue-feel-design.md) (`1ddf3e3`, antes de medir). Evidencia: `docs/validation/evidence/2026-11-20-fatigue-feel/` (SHA-256 en `SHA256SUMS`, `summarize.py`, `summary.tsv`).

**Informe del PO:** «llegó a 0 en un momento) el jugador se seguía moviendo igual». **Diagnóstico confirmado:** en E = 0 el cuerpo pierde solo 10 % de velocidad (4,8 → 4,35 m/s) y suma 37–40 ms de reacción, y el sprint de volea seguía en 2 × (8,7 m/s) sin importar la energía.

## A. La energía limita el sprint (humano, activado por defecto)

Sprint = `1 + (2 − 1)(1 − f)` con f al empezar la recepción; S′ y el ámbar se acortan, S no cambia. Campo `HumanVolleySprintFatigue = 1`, botón DEV «Sprint según energía sí/no».

Una corrida (`FatigueFeelExperiment`, RallySim, Medio, 3 estilos, bot de v2.2 que toca con la distribución del PO). Recepciones del humano sin devolución de saque, energía fijada al inicio del punto (600 puntos por brazo):

| Banda | A-off: volea / sprint | **A-on: volea / sprint** | factor medio A-on |
|---|---|---|---|
| ≥ 70 (n 902) | 27,6 % / 4,0 % | 27,6 % / **4,0 %** | 2,00 |
| 40–70 | 26,2 / 4,5 | 25,4 / 2,8 | 1,77 |
| 10–40 | 25,6 / 3,3 | 22,9 / 1,9 | 1,36 |
| < 10 (n 1.195) | 24,8 / 4,7 | 19,7 / **0,0** | — |

**Meta (≤ 1/3 de la tasa en ≥ 70): PASS**, 0,0 % frente a 4,0 %. El P+V de la volea se mantiene (84–86 %). En partidos naturales no cambia el marcador: 26,6 % de los puntos para el humano en A-off y A-on. **Lectura:** el sprint solo explica el 14 % de las voleas, así que A arregla la incoherencia, pero por sí solo no hace que el jugador se sienta lento.

## B. «Cansancio fuerte», solo DEV (× 3 piernas, × 2 reacción, ambos jugadores)

Valores fijados antes de medir: `legsMax` 0,30 y `reactionMaxMs` 80 solo en el contexto del rally. B43 (0,40 y 120 ms) es informativo.

| Energía inicial del humano (rival en 100) | A-on: puntos del humano | **B32** | B43 | velocidad máx. en E = 5 (A-on / B32 / B43) |
|---|---|---|---|---|
| 100 | 32,0 % | 32,7 % | 32,7 % | — |
| 55 | 25,3 | 24,0 | 26,0 | — |
| 25 | 30,7 | 22,7 | 12,0 | — |
| 5 | 28,7 | **14,0** | 9,3 | 4,35 / **3,46** / 3,02 m/s (+37 / +74 / +111 ms) |

Con B32 en E < 10 la holgura media baja de 0,48 a 0,40 s y la alcanzabilidad de 98,4 a 95,4 %. Partidos naturales (90 por brazo): humano 26,6 → 26,1 % de puntos, energía mediana del humano en el punto 10: 37,6 → 40,4. El rival casi nunca baja de 40 (menos del 1 % de sus recepciones), así que en la práctica B castiga sobre todo al jugador que corre y volea más.

**Si B fuera el default**, el costo sería:
1. el «sí» del PO;
2. `DefaultPreset.json` pasa de `207da8f2c636` a `16a18ced73b3` (medido), con las cuatro anclas AutoQA y su proyección, como en F1;
3. recalibrar el rival (`RivalStylesCalibrationExperiment`: búsqueda de unas 2–4 h más una confirmación de 18 min);
4. repetir el Experimento B del cansancio.

## C. Cansancio visible (solo vista, DEV)

Con E < 25 la cadera baja hasta 7 cm, el tronco se inclina 8° más y la respiración en reposo se hace más honda. Botón «Cansancio visible sí/no», apagado por defecto; no toca la simulación (test).

## Integridad

- **TDD, RED por aserción:** EditMode 4 de 6 (2 controles), PlayMode 3 de 3 y vista 2 de 2. Después GREEN en las suites dirigidas: EditMode 100/100 y PlayMode 59/59 + 26/26.
- **Corrección durante el GREEN:** en f = 1 la primera versión volvía al S de v2 (28 pasos frente a 75); se corrigió el código y queda anotado en la spec.
- **Hashes:** RallyParams nuevo `13f88c8d1db2`; proyección sin el campo nuevo `0fe523164363` y sin `HumanVolley*` `0f58580f44ea`. DefaultPreset `207da8f2c636` y SimulationKey `166a96e319c7` sin cambios.
- **Tests existentes modificados (commit `843772c`):**
  - dos literales derivados: el hash y el número de campos `HumanVolley*` (5 → 6);
  - **uno de comportamiento:** `VolleyV21Tests.RallySim_ATouchingHuman…` falló con la regla nueva (cero voleas con sprint en sus 3 partidos). Se fijó a `HumanVolleySprintFatigue=0` (v2.2 tal como está), sin cambiar su aserción. **Lo decide Claude:** aceptarlo o dejar A apagado por defecto.
- **Rival:** el test de control da partidos idénticos con la regla prendida y apagada. `ConfirmPerStyle` repetido (`confirm-tired`, RallyParams `13f88c8d1db2`) es **idéntico** a `confirm01`: `cells.tsv` sin la columna de segundos (19 celdas) y `fondista-hard.tsv`.
- **Sin cambios en:** `ContactContract.cs`, ClockMap/SimClock, P2 y AutoQA.
- **No corridos** (por encargo): suites completas, AutoQA y check-release.
