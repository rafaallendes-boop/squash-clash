# Saque — diseño y metas predeclaradas

20 nov 2026 · Rama `claude/serve-improve` desde la integración `12cc652`. **Escrito por un subagente Opus dirigido por Claude; la revisión de Claude no es independiente.**

Pedido del PO: «Luego próxima mejora es mejorar el saque» (19 nov) y «Mejorar lo que hay hoy es la prioridad» (20 nov). Hallazgo previo (volea v2): las 338 devoluciones de saque pasan al alcance antes del bote y el bote es fácil (P+V 97–98 %): el saque es blando y alto.

**Revisión 0 (`fd66945`): metas y reglas de decisión fijadas ANTES de medir cualquier cambio.** Revisión 1 (este commit): diagnóstico medido (sección 2) y **parada al final de la fase 1** (sección 5). Las metas de la sección 3 no se tocan.

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

## 2. Diagnóstico

Experimento explícito `ServeDiagnosisExperiment.Diagnose` (fuera de regresión y de AutoQA; no cambia producto):

1. la forma del saque de hoy por cuadro: altura y x en el frontis, velocidad, bote, lateral, fondo y altura al pasar por la T;
2. `RallySim` en Medio contra los tres estilos, 200 partidos por estilo, novato σ 90 ms como humano: devolución de los dos restadores (P+V, alcance, desplazamiento), oferta de volea v2.2 del humano al saque (contrafactual), faltas y cuán rápido gana el servidor (sin devolución y en ≤ 2 golpes);
3. una malla contrafactual de saques candidatos (x y altura en el frontis, velocidad horizontal; 9.180 en total), juzgados por las reglas reales, con una recepción aislada desde la espera de cada restador.

Corrida única `diag01` (Passed, 371 s), defaults `207da8f2c636`, RallyParams `13f88c8d1db2`, SimulationKey `166a96e319c7`, 120 Hz, tempo 0,65. Evidencia en `Logs/experiments/serve/diag01/` (ignorada).

**2.1 Forma del saque de hoy** (la misma para los dos servidores; cuadro der./izq.):

- frontis a **3,6 m** de altura (x 2,82 / 3,58 m), velocidad horizontal **15 m/s** (16,9 m/s total; el tope de la malla de `ServePlanner`), **sin lateral**;
- bota a **z 7,43 m** (no llega a la meta de 8 m) a los 2,13 s; pega en el fondo a 0,63 m y vuelve a botar a 8,98 m;
- pasa por la profundidad de la T a **1,82 m**, dentro de la franja de volea (0,6–1,9 m);
- ventana de contacto del restador: **393–455 ms**; P+V de una recepción aislada del novato: 96,5 % / 97,5 %.

**2.2 `RallySim`, Medio, 200 partidos por estilo** (6.788 puntos):

| | Saca el rival → devuelve el novato | Saca el humano → devuelve el rival |
|---|---|---|
| Puntos | 3.869 | 2.919 |
| P+V de la devolución | **97,5 %** (Fondista 96,3; Definidor 97,5; Completo 98,9) | **100 %** |
| Perfecto | 35,2 % | 58,6 % |
| Oferta de volea v2.2 al saque | **100 %** | — (el rival no volea el saque) |
| Faltas de saque | 0 | 0 |
| El servidor gana el punto | 58,7 % | 39,1 % |
| Sin devolución (saque ganador) | 2,5 % | 0 % |
| Gana en ≤ 2 golpes | 7,9 % | 5,3 % |
| Desplazamiento del restador (mediana) | 2,09 m | 1,99 m |

**2.3 Malla de candidatos:** 2.884 de 9.180 son legales.

- **Formas de globo** (frontis ≥ 3,4 m, lateral antes del bote, bote ≥ 7,5 m): 236. P+V del novato entre **92 % y 100 %** (mediana 99 %). **Ninguna entra en la banda 80–90 %.** El globo baja lento y deja ventanas de 0,4–1,2 s.
- **Formas de drive** (frontis ≤ 2,6 m): 630, P+V del novato p10 92 %, mediana 96 %. Hay drives a 85 % solo en el borde de la malla (22 m/s horizontal).
- **Oferta de volea v2.2 al saque:** existe en **2.751 de 2.884 saques legales (95,4 %)**, en 235 de 236 globos y en 522 de 630 drives. Con el sprint de v2.2 el humano llega a interceptar casi cualquier saque antes del bote, sea cual sea su altura al pasar por la T.
- **Regla de la sección 4 aplicada** (P+V más cercano a 85 %):
  - globo der. frontis 3,6 m, 18 m/s, lateral a z 8,3 m, bote a z 9,7 m: P+V **92,0 %**, con oferta;
  - globo izq. frontis 3,45 m, 18 m/s, lateral a z 8,6 m, bote a z 9,3 m: **93,3 %**, con oferta;
  - drive der. frontis 2,55 m, 22 m/s, bote a z 8,8 m: **85,3 %**, con oferta;
  - drive izq. frontis 2,4 m, 22 m/s, bote a z 8,4 m: **85,7 %**, con oferta.
  - Estos globos ya necesitan 18 m/s para llegar al rincón, así que el drive solo es 1,22 veces más rápido. **El criterio de velocidad de M3 (≥ 1,4×) no se cumple** con ningún drive de la malla. El de tiempo, sí: el drive bota en el 72 % del tiempo del globo.
  - El rival devuelve esos saques al 99–100 % (globo) y al 94–100 % (drive).

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

## 5. Parada al final de la fase 1 (20 nov)

Con la regla predeclarada, la malla muestra que **las metas no se pueden cumplir juntas dentro del alcance permitido** (forma del saque, solver del saque, `RallyParams`, elección en `RivalAi`, vista):

- **M2 es inalcanzable por la forma:** el 95 % de los saques legales ofrece la volea v2.2, y el 99,6 % de los globos. La estimación con las formas elegidas es de alrededor del 100 % de ofertas, frente a ≤ 50 %.
- **M1 queda justo:** con la mezcla por estilo (drive 0,25 / 0,5 / 0,75), la estimación de recepciones aisladas da Fondista 90,9 %, Completo 89,1 % y Definidor 87,3 %, con un agregado ≈ 89 %. Entra solo por el drive; el Fondista queda fuera.
- **M3 falla en el criterio de velocidad** (1,22× frente a ≥ 1,4×).
- **Riesgo para N2:** el saque nuevo complica mucho más al novato que al rival (85–93 % frente a 94–100 %). Es probable que la calibración por estilo se mueva en contra del novato. No se midió.

No se implementó nada de la fase 2. Opciones para decidir:

1. **Aceptar el saque de dos tipos con metas revisadas**, decididas por Claude o el PO, no por el subagente: M1 ≈ 89 %, sin M2 y M3 por tiempo y no por velocidad. Luego medir N1–N3 en `RallySim`.
2. **Atacar la volea al saque, no la forma:** por ejemplo, sin sprint de v2.2 en la devolución de saque, o volviendo a v1 (sin volea al saque) con el interruptor ya existente. Toca la volea v2.2 recién entregada y requiere medir su efecto.
3. **Ampliar la malla** (velocidades > 22 m/s, salida de la mano más alta, globos más lentos con el frontis más arriba) y repetir la regla. Es probable que no cambie M2.
