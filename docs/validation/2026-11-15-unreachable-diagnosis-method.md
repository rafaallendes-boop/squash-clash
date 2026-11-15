# Diagnóstico de `Unreachable` con la curva de potencia — método predeclarado

Fecha: 15 de noviembre de 2026. Rama `firstshot/shot-pace-distance` (PR #56), head de partida `3fc0460`. Este documento se confirma en Git **antes** de ejecutar nada. Operador: subagente Sonnet dirigido por Claude (revisión no independiente de la dirección).

Origen: decisión literal del PO, ya registrada en `3fc0460`: «Pedir un diagnóstico específico del aumento de `Unreachable`, separando golpes, posiciones y jugador receptor.» y «Si no aparece un defecto crítico, preparar una build DEV para validar la sensación en iPhone antes de autorizar la fusión.» Solo diagnóstico; no se corrige producto, no se ajusta nada y la decisión de la build es de Claude.

## 1. Qué es `Unreachable` en la simulación

En `RallySim.BeginReception`, una recepción termina en `ReceptionResult.Unreachable` cuando `ReceptionPlanner.Plan` devuelve `Reachable=false` y el cerebro (Bot o Fondo) no golpea. Eso ocurre cuando ningún candidato de posición llega a tiempo **y** desde la posición donde el cuerpo estaría al pasar la pelota tampoco existe un contacto legal («unreachable» sin «stretched»). La pelota sigue sin ser golpeada y el punto cierra por `DoubleBounce`; `RallyOutcome.LastReception = Unreachable`. En el informe de rallies, «Unreachable 36 → 215» es el recuento de puntos cerrados así. El «jugador receptor» es quien no llega: **Human** es el `BotBrain` σ=30 ms (intención fija: paralelo, altura media, potencia media), **Rival** es `FondoAi` nivel Medio.

Modelo de alcance (código, no se modifica): cuerpo cinemático en el plano (aceleración 12, velocidad máxima 4,8, freno 16 m/s²), retardo `ReactionS` (0,18 s) más la percepción del cerebro; candidatos de posición cada `CandidateIntervalS` (0,02 s) sobre la ventana legal de la pelota, a ±`IdealLateral` del punto, con el balón dentro de `body.reach`; un candidato es alcanzable si `StepsToReach` ≤ pasos hasta T*.

## 2. Corridas (únicas; sin ajuste, sin variantes extra)

Solo cambia `shot.paceGammaMax`: **γmax = 1,0** (fórmula lineal anterior, bit a bit) y **γmax = 2,0** (valor de la rama). `shot.paceNearM = 1,5`, `shot.paceFullM = 6,0`, todo lo demás de `DefaultPreset.json` (hash `06fed4bd4ac9`, 121 claves, comprobado al empezar) y `RallyParams` por defecto, intactos.

- **Flujo A (réplica de `RallySimTests.Simulation_OfAThousandRallies_AlwaysTerminates`)**: un único `RallySim`, `BotBrain(σ=30, seed 2026)` como Human y `FondoAi` Medio seed 1111 como Rival, 1.000 rallies, `firstReceiver` alterna Human/Rival (par = Human). Comprobación de reproducción: con γ=2,0 debe dar DoubleBounce tras Air/Late/Unreachable = 406/330/215 y con γ=1,0 = 593/312/36. Si no coincide, se informa tal cual y se sigue con lo medido (los números del experimento mandan).
- **Flujo B (pareado)**: para poder comparar el mismo rally entre variantes, cada rally *i* (0–999) usa un `RallySim` y cerebros nuevos con semillas independientes del flujo: Bot `2026+i`, Fondo `1111+i`, mismo `firstReceiver`. Mismos *i* en γ=1 y γ=2. Las semillas de la primera mitad del rally coinciden mientras todos los golpes tengan D ≥ 6 m; el primer golpe con D < 6 m puede divergir y de ahí en adelante los rallies ya no son comparables golpe a golpe. Se registra el índice de la primera recepción en que difieren (posición/velocidad de la pelota al inicio de la recepción).

Dos procesos Unity separados (`SC_UNREACH_RUN=run01` y `run02`), uno a la vez; las salidas deben ser idénticas byte a byte (SHA-256). Los datos viven en `Logs/experiments/unreachable/` (ignorado); se versiona un resumen sanitizado < 1 MB y los SHA-256 de todo.

## 3. Registro por recepción y por final

Cada recepción se registra mediante un envoltorio de `IRallyBrain` que no altera ninguna decisión ni consume aleatoriedad (el ruido de postura se captura y se devuelve igual): jugador receptor, índice de recepción, pelota al inicio (posición, velocidad: es el contacto del golpeador), posiciones y velocidades de ambos cuerpos, ruido de postura, plan (`Reachable`, razón, `ArrivalStep`, holgura, `PStar`, `TStarStep`, stance), decisión (opción línea/altura/potencia, mira, P, delta). Para cada golpe (recepción k ≥ 1) se calcula con el contacto, la mira y la P realmente usados: D, γ, velocidad lineal, velocidad de la curva, velocidad real |v|, primer y segundo bote (de `context.Predict` sobre la pelota ya golpeada: solo sucesos de suelo), tiempo hasta el segundo bote, y el resultado de la recepción siguiente. Se tabulan **todas** las causas de fin de punto por causa × receptor × golpeador × `LastReception`, para las dos variantes y los dos flujos.

Para cada final `Unreachable`: golpeador y receptor; P, mira (x, y), línea/altura/potencia de la opción; D, γ, velocidad lineal y de la curva (y su cociente); contacto (x, y, z); primer bote y segundo bote (posición, tiempo); posición del receptor al empezar la recepción, la posición objetivo (`Stance`) y la distancia que tiene que recorrer; la posición del receptor cuando la pelota hace el segundo bote y su distancia a ese punto; **tiempo disponible frente a requerido** según un espejo del planificador (§4); holgura de la mejor opción en pasos y segundos; y, en el flujo B, el resultado del mismo rally i con γ=1 y el índice de divergencia.

## 4. Espejo y sondeo de alcance (solo lectura)

En cada recepción con plan no alcanzable se ejecuta, **sin tocar nada del producto**, un espejo del bucle de candidatos de `ReceptionPlanner.Plan` (ventana legal, malla de 0,02 s, dos lados, `In(k)`, `star`, `StepsToReach`) con el mismo cuerpo y el mismo ruido de postura, para contar: candidatos alcanzables, mejor candidato inalcanzable y su retraso en pasos (`arrival − available` y `arrival − untilStar`). Debe coincidir con la razón del plan: si el espejo halla candidatos alcanzables pero el plan dice no alcanzable (ruta ii, el contacto falla tras elegir), se cuenta aparte. Además, un **sondeo independiente**: para cada paso de la malla de la ventana legal, la posición más cercana al balón dentro del radio de alcance (`ClampBody`), con `StepsToReach` ≤ pasos disponibles; si existe, se evalúa `ContactPlanner.Plan` desde esa posición. Un caso del sondeo con contacto que existe y llegada ≤ T* es una *contradicción candidata* con el plan.

## 5. Clasificación (fijada antes de mirar los datos)

Cubetas por golpe (el que causó el `Unreachable`; contacto (x,y,z) con z = profundidad desde el frontis):

- P: suave ≤ 0,25; media (0,25, 0,65]; fuerte > 0,65 (los valores de las opciones son 0,12/0,27 en drop, 0,5 media, 0,85 fuerte).
- D: cercano < 3 m; medio [3, 6); lejano ≥ 6 (curva inactiva).
- z del contacto: frente < 3 m; medio [3, 6); fondo ≥ 6.
- Mira y: baja < 1,2; media [1,2; 2,4]; alta > 2,4. Línea (la de la opción: Straight/Cross/Drop).
- Cociente curva/lineal: frenado < 0,85; sin efecto ≥ 0,85.
- Muerte de la pelota: segundo bote z2 frente < 3; medio [3, 6); fondo ≥ 6.
- Receptor: distancia al segundo bote < 2 / [2, 4) / ≥ 4 m; cuadrante del receptor respecto de ese punto (delante/detrás por z, izquierda/derecha por x); retraso del mejor candidato < 0,1 / [0,1; 0,3) / ≥ 0,3 s.

Clasificación primaria excluyente, en este orden:

1. **C1 golpe corto suave desde adelante (efecto buscado)**: D < 6 ∧ P ≤ 0,25 ∧ z2 < 3.
2. **C2 frenado por la curva no suave**: D < 6 ∧ cociente < 0,85 ∧ no C1.
3. **C3 sin efecto de la curva**: D ≥ 6 ∨ cociente ≥ 0,85.

Se informa por variante, por receptor y por golpeador, y la **tasa por clase** (Unreachable sobre todos los golpes de esa clase), no solo el recuento.

## 6. Qué cuenta como defecto crítico

Cada uno se contesta sí/no con evidencia, y si hay un defecto se registra y se **detiene** (no se corrige producto):

a. **Pronóstico ≠ trayectoria real.** Para cada golpe, con la misma entrada, `Input.Speed` del registro causal debe igualar la velocidad de `ShotPace.Requested` si los vectores causales son cero; y la velocidad de `RallyContact.SolveIntention` (pronóstico) debe igualar |v| real si los vectores son cero y no hubo recorte de mira. Se informa el máximo de la diferencia y el recuento sobre 1e-3 m/s; además se comparan los errores de primer bote del pronóstico de la IA entre γ=1 y γ=2. **Defecto** = cualquier discrepancia sin vectores causales, o un empeoramiento del error del pronóstico atribuible a la curva.
b. **Inconsistencia del chequeo de alcance.** **Defecto** = una contradicción candidata del sondeo (§4) que además no esté ya presente con γ=1 en proporción similar, o una discrepancia entre espejo y plan. Las contradicciones por la conservadurización del planificador (solo prueba dos posiciones por muestra) se informan como observación si son iguales en ambas variantes.
c. **Velocidades y legalidad.** **Defecto** = velocidad de la curva fuera de [`vMin`, `vMax`], NaN o Inf, |v| real no finita o sobre `vCap`, o un final `Unreachable` cuyo golpe fuese ilegal (el punto debía cerrar por Out/Tin/NotUp): se comprueba que `Cause=DoubleBounce`, superficie terminal suelo, golpeador = oponente del receptor.
d. **Saque y doble aplicación.** Revisión estática de todos los llamadores de `ShotPace.Requested` y de la fórmula lineal, más los resultados: el saque (`ServePlanner`) usa velocidades fijas y no debe pasar por la curva; ningún camino aplica la curva dos veces. **Defecto** = cualquier camino que omita o duplique la curva.
e. **Elección degenerada de la IA.** Se comparan las distribuciones de opciones del Rival (línea × altura × potencia) y su tasa de `Unreachable` entre γ=1 y γ=2. **Defecto sospechoso** = una opción cuya cuota crece más de 15 puntos porcentuales y que además es la causa dominante de los `Unreachable`. Se informa también por código que `FondoAi.Utility` no modela el alcance del oponente (no «sabe» nada de la inalcanzabilidad).

Un hallazgo que no cumple esas condiciones se registra como observación, no como defecto. Si aparece algo inesperado (fallo de compilación en el experimento que no sea trivial, no determinismo entre las dos corridas, defecto), se registra y se detiene. No se repiten corridas para obtener otro resultado.
