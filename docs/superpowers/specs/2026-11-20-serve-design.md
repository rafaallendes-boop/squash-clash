# Saque — diseño y metas predeclaradas

20 nov 2026 · Rama `claude/serve-improve` desde la integración `12cc652`. **Escrito por un subagente Opus dirigido por Claude; la revisión de Claude no es independiente.**

Pedido del PO: «Luego próxima mejora es mejorar el saque» (19 nov) y «Mejorar lo que hay hoy es la prioridad» (20 nov). Hallazgo previo (volea v2): las 338 devoluciones de saque pasan al alcance antes del bote y el bote es fácil (P+V 97–98 %): el saque es blando y alto.

**Revisión 0 (este commit): metas y reglas de decisión fijadas ANTES de medir cualquier cambio.** El diagnóstico medido y la elección de las formas se agregan en la revisión 1, sin tocar las metas de la sección 3.

## 1. Cómo funciona hoy (código)

- **Un solo saque automático, igual para los dos.** `RallyContext.Serve(box)` (`Core/Rally/RallyContext.cs:89-93`) guarda un único `BallState` por cuadro, buscado una vez con `ServePlanner.Find` (`Core/Rally/ServePlanner.cs:17-37`).
  - Elige el que bota más cerca de un punto fijo: `ServeLandingDepth` 8 m y `ServeLandingFromWall` 1,4 m (`Core/Rally/RallyParams.cs:34`).
  - Malla fija: 4 profundidades, alturas en el frontis {2,4; 2,8; 3,2; 2,1; 3,6} m y velocidades horizontales {11…15, 10, 9} m/s (`ServePlanner.cs:23-25`). La x del frontis sale del reflejo del punto de bote.
  - Valida cada candidato con las reglas reales sobre una predicción aislada (`ServePlanner.Forecast`, `ServePlanner.cs:39-59`).
- **Nadie elige.** Ni el humano (no hay gesto: `RallyDirector.BeginPoint`, `Runtime/Rally/RallyDirector.cs:198-205`) ni el rival (`RivalAi` no tiene saque, `Core/Rally/RivalAi.cs`). El servidor solo se teletransporta al cuadro y vuelve a esperar (`RallySim.PlayPoint`, `Core/Rally/RallySim.cs:126-135`).
- **Reglas** (`Core/Rally/PointRules.cs`):
  - frontis primero; cualquier otra pared antes del frontis es falta (`PointRules.cs:122-126`);
  - en el frontis, por encima de la línea de saque (tocarla es falta), sin tin ni fuera (`ReturnLineJudge.ServeFrontWallGood`, `PointRules.cs:41-45`);
  - primer bote en el cuarto trasero opuesto, sin tocar la línea corta ni la media (`ServeLandsInQuarter`, `PointRules.cs:47-53`);
  - tras el frontis y antes del bote, las laterales y el fondo valen si no son fuera (`PointRules.cs:128`);
  - un solo intento: la falta da el punto al restador (`PointRules.cs:112-113`).
- **Cansancio:** el saque automático cobra la intención P = 0 al servidor (`Energy(server).Strike(0)`, `RallySim.cs:128` y `RallyDirector.cs:202`; spec de cansancio v2.3).
- **Volea al saque:** solo el humano puede volear la devolución (`RallyContext.NewRules`, `RallyContext.cs:61`; `PointRules.CanHit`, `PointRules.cs:132`).

## 2. Diagnóstico (se mide en la revisión 1)

Experimento explícito `ServeDiagnosisExperiment.Diagnose` (fuera de regresión y de AutoQA; no cambia producto):

1. la forma del saque de hoy por cuadro: altura y x en el frontis, velocidad, bote, lateral, fondo y altura al pasar por la T;
2. `RallySim` en Medio contra los tres estilos, 200 partidos por estilo, novato σ 90 ms como humano: devolución de los dos restadores (P+V, alcance, desplazamiento), oferta de volea v2.2 del humano al saque (contrafactual), faltas y cuán rápido gana el servidor (sin devolución y en ≤ 2 golpes);
3. una malla contrafactual de saques candidatos (x y altura en el frontis, velocidad horizontal; 9.180 en total), juzgados por las reglas reales, con una recepción aislada desde la espera de cada restador.

## 3. Metas predeclaradas (no se cambian para pasar)

Medidas en `RallySim`, Medio, los tres estilos, novato σ 90 ms como humano, semillas nuevas, 200 partidos por estilo.

| Id | Meta | Umbral |
|---|---|---|
| M1 | Dificultad de la devolución: P+V del novato al devolver el saque del rival (agregado de los tres estilos) | **80–90 %** (hoy ≈ 98 %) |
| M2 | Menos voleas regaladas: ofertas de volea v2.2 del humano en la devolución de saque | **≤ 50 %** de las devoluciones (hoy ≈ 100 %); se informa el número |
| M3 | Variedad: dos tipos distinguibles, globo y drive, disponibles para quien saque | globo: frontis ≥ 3,4 m, toca la lateral entre el frontis y el bote, bota a z ≥ 7,5 m; drive: frontis ≤ 2,6 m, velocidad horizontal ≥ 1,4 × la del globo y llega al bote en ≤ 75 % del tiempo del globo |
| N1 | Faltas de saque de la IA | **≤ 3 %** de sus saques |
| N2 | Calibración por estilo: `ConfirmPerStyle` con el saque nuevo frente a `confirm01` | se informa el desplazamiento; si un estilo se mueve **> 5 pp** en algún nivel (novato), **parada** y reporte, sin recalibrar |
| N3 | Sin cambio fuera del saque | con «Saque nuevo» apagado, `ConfirmPerStyle` idéntico a `confirm01` (`cells.tsv` sin segundos y `fondista-hard.tsv`); el juego del rival fuera del saque usa el mismo código y el mismo generador aleatorio |

Además, en todas las corridas: cero fallas de sistema nuevas atribuibles al saque y AutoQA `v`, `t3` y `volley` PASS/0/1.

## 4. Propuesta (a confirmar con el diagnóstico)

- **Dos tipos, globo y drive,** cada uno un saque fijo por cuadro elegido por `ServePlanner` con las reglas reales (uso del solver propio del saque). Sin ruido de ejecución: el saque nunca es falta, como hoy (adaptación arcade de 8.7.5).
- **Regla de elección de las formas (predeclarada):** para cada tipo y cuadro, entre los candidatos de la malla que cumplen la forma de M3, el de P+V del novato (recepción aislada desde su espera) más cercano a **85 %**; desempate: sin oferta de volea, luego mayor margen a las líneas. Si un tipo no tiene candidato en la banda 80–90 %, se informa y se toma el más cercano.
- **Humano (gesto M2-4):** en la pausa antes de su saque, el jugador toca el frontis como en un golpe. La altura de la mira elige el tipo: por encima de un umbral (propuesto 3,0 m), globo; por debajo, drive. Al soltar, saca en ese momento. Sin toque, al terminar la pausa sale el globo. El primer saque del partido, sin pausa previa, es el globo.
- **Rival:** el estilo elige con un generador propio, separado del de sus golpes: probabilidad de drive Fondista 0,25, Completo 0,5 y Definidor 0,75.
- **`RallySim`:** un cerebro sin elección de saque (los bots de referencia) saca globo.
- **Interruptor DEV «Saque nuevo: sí/no»** para comparar con el saque de hoy. Encendido por defecto; apagado reproduce hoy bit a bit.
- **Permitido:** objetivo y potencia del saque, solver del saque, campos de `RallyParams`, elección del saque en `RivalAi` y la vista. **Parada** si hace falta física P2, `DefaultPreset.json`, SimulationKey, `ContactContract.cs`, ClockMap/SimClock, las reglas de saque de `PointRules` o cualquier escenario, oráculo o ancla de AutoQA.
