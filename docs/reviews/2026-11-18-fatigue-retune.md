# Reajuste del cansancio con datos de juego real

18 nov 2026 · rama `claude/fatigue-retune` (base `main` `75b9ce8`) · **Implementado por subagente Sonnet dirigido por Claude; la revisión de Claude no es independiente.** Sin push, PR, fusión ni instalación.

**Problema (PO):** «La energía no baja, aunque haya partido en 40, después iba subiendo.» Los datos del teléfono (7 partidos, 36 puntos humanos, 18 nov 00:42–01:08Z) dan una mediana de 18 m por punto (el Experimento B supone unos 36 m). Con `costPerMeter` 0,4 y `recoverBetween` 0,25 el equilibrio queda cerca de 80 y nunca se cruza `onset` (70).

**Cambio (solo valores de `DefaultPreset.json`, sin tocar `Fatigue.cs`):** `fatigue.costPerMeter` 0,4 → **0,3**; `fatigue.recoverBetween` 0,25 → **0,08**. `recoverPerS` y el resto, iguales. Elegidos con un modelo Python fiel a `FatigueState`, reproducido luego en Unity con las mismas cifras (80,62 antes y 67,5 después en el punto 11).

| Objetivo (modelo = Unity, mediana de 400 partidos de 20 puntos remuestreados de los 36 reales) | Antes | Después |
|---|---|---|
| O1 energía al empezar el punto 11 (debe ser <70) | 80,6 | **67,5** |
| O1 energía al empezar el punto 20 (45–60) | 78,8 | **53,2** |
| O2 tras «Energía 40», inicio de los puntos 2/3/4 (≤55) | 50,3 / 58,1 / 63,9 | **40,8 / 41,4 / 42,1** |
| O3 rallies largos de B (≈36 m): puntos con E<10 | 0 % | 0 % (modelo) |
| O4 | Partido nuevo parte en 100 (telemetría `Start=100,0` en cada primer punto). Ningún preset del teléfono (P0-calib-1, Po-1, S1-*) contiene claves `fatigue.*`, así que valen los defaults; P0-calib-1 intacto (SHA `2f1295f2…`). |

Rallies largos se notan más: a 12 m por punto la energía cae a 81 tras 20 puntos, a 30 m a 36, a 48 m queda en 8 (agotado, es lo esperado de rallies de 48 m sostenidos).

**TDD:** `FatigueRetuneTests` (5): RED por aserción con los valores anteriores (O1 y O2 fallan con las cifras del modelo; los 3 restantes ya cumplían), GREEN tras el cambio.

**Tests ajustados (valores o literales derivados del preset, sin cambiar criterios):** `FatigueTests` (cuatro aserciones fijaban 0,4/0,25 y sus derivados), `FatigueArmRallyTests.A1` (repite la evidencia F1 congelada: fija 0,4/0,25 explícitamente), `ZFatigueFlowTests` (0,25→0,08 y tolerancia 1e-9 en una igualdad exacta con ida y vuelta por JSON que pasó a diferir en el último bit). Hashes en commits propios (medidos en Unity): defaults T3 `2ec0c953786a`→**`207da8f2c636`** (132 claves), defaults con cansancio apagado `3262f9b89156`→**`ec3ff68ebdfa`**, P0 `f92f406f698a`→**`7a5972dacf1e`**, SL5 T3 `750c407c8a2f`→**`d09119d6321b`**, V `e8df60f37d4b`→**`28e63d17dfbf`**, SL5 V `95a3f24313fe`→**`8ec4deffc6c1`**. Las cuatro anclas de `Scenario.ExpectedHash` actualizadas en commit propio. La proyección sobre las 121 claves previas sigue exacta (T3 `430da2faa818`, P0 `dcc7c8cc0688`…), SimulationKey `166a96e319c7` y RallyParams `927e304bbfec` intactos.

**Experimento B (todo activo, 200 partidos / 1.882 puntos, sin reajuste):** B1 Pass (humano 1,61; rival 1,09); B2 Pass (19,11 y 5,39); B3 Pass (62,12; 196/200 bajo onset); **B4 Fail: 12/200 partidos (6,0 %) con alguna energía inicial <20, umbral ≤5 %**; cero saques con E=0; B5a Pass (37,80, 14 puntos); B5b exploratorio (3 pares, sin conclusión); B6 Pass (razón 0,926). No se reajustó; decide Claude/PO. El modelo estimaba 4 %; B4 solo mide partidos simulados de rallies muy largos (mediana B ≈36 m frente a 18 m reales).

**Puertas:** EditMode **808/808**, PlayMode **611/611** (una corrida previa falló `ReticleVisibilityTests` de forma intermitente, CRP-01, y pasó en la siguiente sin cambios), AutoQA V y T3 PASS/0/nivel 1 (88/88, sin dirty: `20261119T000319Z-089907b-v-5236`, `20261119T000336Z-089907b-t3-26984`), check-release dev/prod OK. ProjectSettings/Packages sin cambios. Head `089907b` + este informe.
