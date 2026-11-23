# RAL-174 — protocolo y evidencia del camino B

Encargo completo leído en `~/sc-specs/impl-i2i3-b.md`; iteración propia RED→GREEN autorizada por `~/sc-specs/auth-green-1122.txt`. Sin cambio de umbrales, oráculos, escenarios, semillas, pesos o presets para pasar. Sin fusión ni instalación.

## Base y criterio, fijados antes de implementar B

- Rama de partida `feat/rival-short-i2i3` en `406f392`. Se incorpora `origin/main` `230b758` con el merge `2bf90f6`; no se reescribe historial ni se fuerza el push.
- Spec de #92 leída con `git show origin/docs/rival-corto-arq:docs/superpowers/specs/2026-11-22-rival-short-ball-design.md`, head `77544cf`, incluida su adenda de camino B. La adenda precisa que globo/recta alta es `{Straight,Cross} × High × Medium`. Se usa ese mapeo, sin añadir globo ni P5.
- La dos paredes existe solo donde ya la ofrece `ShotOptions.AddBoasts`; no se añade un corte de zona. En `stretched`, también puede elegirse allí, pasando `ForecastBoast` / `SolveBoastIntention` y el planificador del humano con `ReactionS`.
- B1: contacto adelante `(.9,.8,1.5)`, tres estilos y 500 decisiones, ninguna dos paredes. Reemplaza solo el test nuevo de I2 que no existe en main; no cambia tests existentes.
- B2: esquina de atrás `(.9,.8,8.6)` y humano en la T; precondición: alguna dos paredes Good y Reachable. Definidor y Completo, 500 decisiones, alguna dos paredes. Si falta la precondición se registra y se para, sin buscar parámetros.
- B3: mismo contacto, humano en la esquina de atrás opuesta, limitada por `ClampBody`; alguna opción alcanzable y alguna dos paredes inalcanzable como precondiciones. Tres estilos y 500 decisiones: cero opciones inalcanzables elegidas. Si nace verde, guardia y prueba directa de lateral antes de frontis en la trayectoria usada por I3.
- B4: se conservan las guardias de `planned`, huella `53870c765e9e1cc410b87b41d810afc7fbe53ac0ffa42345b9b97192786de4de`, alcanzabilidad y respaldo. El test nuevo de hashes se actualiza al valor **ya aprobado en main**: RallyParams `1063d0f6cf66` (AGENTS §5 y #100), no al resultado de B. Defaults `207da8f2c636` y SimulationKey `166a96e319c7` siguen iguales.

## Mediciones predeclaradas

- F1: experimento existente `BoastExperiment.M3ConfirmRound2`, sin editarlo: 100 partidos por estilo y nivel, NewcomerBot σ 90, semillas `20270131`. En Medio: Fondista < Definidor, 5 % ≤ Definidor ≤ 15 %, Fondista ≤ 5 % y Completo entre ambos. Fácil y Difícil se reportan.
- F2/R8: Anexo A de la spec, incluida `r_z`, sin cambiar mezcla, 150 partidos × 3 estilos × 3 niveles ni semillas `20261122`. El encargo nuevo exige comparar con **main `230b758`**, que sustituye la base antigua `518f9c3` del texto histórico de la spec. No se apaga el saque nuevo para la medición.
- R8 es `won == 1` entre `reason == stretched && result == Contact`. La rama no debe superar main en total ni por estilo sumando niveles. Se comparan fracciones enteras sin tolerancias; nivel y zona se reportan sin criterio nuevo.
- Se repite cada `rows.tsv` para comprobar determinismo byte a byte. Se añade solo un contador observador de alineación Decide/log en archivo aparte; no cambia las filas ni la simulación. Si hay más de un Decide sin log por punto se cuenta, no se oculta.
- Se corren EditMode, PlayMode, AutoQA V/T3/volea y check-release con el candado indicado, uno por corrida. Cualquier fallo de B5, F1 o F2 exige registrar y detenerse; PR draft con cifras si no vuelve M3. Una falla de entorno nunca se aprueba.

## Baseline main

`b-main-230b758-run01`: experimento del Anexo A y diagnóstico del escenario unitario de M3, **2/2 Passed**, XML fresco. El diagnóstico de seis partidos (Fondista 25/387 = 6,4599 %, Definidor 36/319 = 11,2853 %) **no es F1** y no se usa como condición del producto.

`b-main-230b758-run02`: Anexo A con contador de alineación, **1/1 Passed**. `rows.tsv` idéntico a run01: **42.986 filas**. Alineación: 0 Decide extra sin log y 0 cambios de rol. Sin SystemFault en las filas. Baseline y rama emplearán el mismo archivo de experimento con el contador.

Evidencia completa ignorada en `Logs/experiments/rival-short-ball/` y `Logs/evidence/rival-short-min/b-*/`. `scripts/device.env` se lee sin editar; la copia local se cotejará y retirará al terminar según el encargo.

## F1 de main, confirmación predeclarada

`b-main-f1`: `BoastExperiment.M3ConfirmRound2`, **1/1 Passed**, 100 partidos por estilo/nivel, XML fresco, sin errores CS. Archivo original del experimento sin cambios. Datos en `Logs/experiments/boast/b-main-230b758-f1/confirm.tsv`.

| Nivel | Fondista | Definidor | Completo |
|---|---|---|---|
| Medio | 180/3814 = 4,7194546408 % | 454/4388 = 10,3463992707 % | 370/4169 = 8,8750299832 % |
| Fácil | 457/3254 = 14,0442532268 % | 561/3079 = 18,2202013641 % | 484/2810 = 17,2241992883 % |
| Difícil | 12/3671 = 0,3268864070 % | 82/5189 = 1,5802659472 % | 48/4208 = 1,1406844106 % |

Medio cumple F1 en main. Cero fallos de sistema en los nueve casilleros. Fácil/Difícil son datos, no se aplican a ellos las bandas de Medio.

## F2 de main

Ambas corridas del Anexo A dieron estas mismas fracciones:

| Grupo | Ganadoras / contactos estirados | R8 |
|---|---|---|
| Total | 1436/10632 | 13,5063957863 % |
| Fondista | 607/3921 | 15,4807447080 % |
| Definidor | 414/3527 | 11,7380209810 % |
| Completo | 415/3184 | 13,0339195980 % |

## B1–B4: RED y parada por precondición B3

Se extrajo mecánicamente `StretchedIntention` del filtro existente, manteniendo la intención directa anterior, para que la prueba directa B3 examine la solución que realmente usa I3. No se aplicó aún el cambio de política ni el solver de dos paredes del camino B.

- `b-directed-red`: **21 tests, 15 Passed, 6 Failed, 0 Skipped**, XML fresco. B1 y B4 verdes. B2 (Definidor/Completo): las precondiciones Good/Reachable pasan; RED válido por **0 dos paredes** frente a alguna exigida. Prueba directa B3: RED válido, `Solved` false al usar la intención directa con el aim lateral.
- La precondición de B3 falló en los tres estilos: ninguna de las opciones consideradas era inalcanzable desde la esquina opuesta. El helper inicial de mi test descartaba las faltas antes de comprobar la precondición. Eso era una suposición propia adicional, no un requisito de B3; se corrigió para que las cuatro opciones se clasificaran con la guarda `ForecastBoast.Good && plan.Reachable`, sin cambiar posición, contacto ni parámetros.
- `b-directed-red-forecast-guard`: **21 tests, 15 Passed, 6 Failed, 0 Skipped**, XML fresco, sin errores CS. Confirma la misma precondición incumplida incluso contando todas las opciones. Datos de B3, contacto `(.9,.8,8.6)`, humano `ClampBody((Width,Length))` (esquina opuesta):

| Dos paredes | ForecastBoast.Good | Plan del humano Reachable |
|---|---|---|
| Low / Medium | true | true |
| Low / Strong | true | true |
| Mid / Medium | true | true |
| Mid / Strong | true | true |

La adenda B, inmediatamente bajo B1–B5, establece: «Si alguna precondición de B2 o B3 no se cumple en el escenario predeclarado, se registra y se para». **Se detiene aquí**. No se buscan otros contactos, posiciones, parámetros, semillas ni criterios para hacer pasar la precondición. No es un RED válido de la propiedad de rechazo B3; no se declara como tal.

Por esta parada, B no está implementado, no hay GREEN dirigido ni B5 de esta tarea, y F1/F2 de la rama, PlayMode, AutoQA y check-release quedan **no evaluados**. No se extrapolan los resultados viejos de I2–I3 a la base actual `230b758`. La PR se entrega como borrador para resolver la precondición, no como candidata de fusión. Ningún test existente en main, preset, solver, física o contrato se modifica. Los candados adquiridos se liberaron al terminar cada corrida.
