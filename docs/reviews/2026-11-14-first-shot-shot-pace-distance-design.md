# First Shot — diseño: potencia según la distancia al frontis (SL5/SL6 y R3)

Propuesta de diseño de Claude, 14 de noviembre de 2026. **Solo diseño**: no hay código, pruebas, presets, defaults ni cambios de spec. **El PO aprobó el diseño y sus tres decisiones el 14 de noviembre (sección 11).** La implementación se autoriza aparte, después de revisar esta PR de diseño, con el encargo de la sección 9.

## 1. Encargo del PO (literal)

> Claude, avancemos directamente con la propuesta de diseño y tu recomendación, sin necesidad de hacerme más preguntas por ahora.
> Para SL5/SL6 y R3, quiero que evalúes las alternativas y propongas la solución que mejor represente el squash real, manteniendo la experiencia arcade de Squash Clash.
> Considera lo siguiente:
>
> 1. Naturalidad del golpe: desde adelante debería ser posible ejecutar golpes cortos y controlados sin que una potencia media mande sistemáticamente la pelota al fondo. Desde atrás también deben existir buenos drops, pero no deberían resultar sistemáticamente más letales que desde adelante.
> 2. Modelo de potencia: evalúa si necesitamos que la velocidad inicial dependa del contexto del impacto (posición, distancia al frontis, altura, dirección), en lugar de depender únicamente de la potencia del gesto.
> 3. Física P2: prioriza conservar First Ball, cuyo gate ya cerramos. Sin embargo, si consideras que modificar la física es necesario para lograr un comportamiento natural, justifícalo y explica sus consecuencias.
> 4. Consistencia: evita soluciones artificiales, reglas especiales para determinados escenarios o ajustes que solo permitan pasar los tests del Anexo C.
> 5. Experiencia de juego: conserva M2-4 y su simplicidad. No quiero agregar gestos ni controles para resolver este problema.
>
> Quiero que entregues:
>
> * Tu diagnóstico de la causa raíz.
> * Las alternativas de diseño, con ventajas y desventajas.
> * Una recomendación concreta y fundamentada.
> * Cómo debería comportarse el golpe desde adelante, media cancha y el fondo.
> * Los criterios de aceptación para SL5/SL6 y R3.
> * Los riesgos sobre First Ball y el resto de First Shot.
>
> Importante: estamos exclusivamente en fase de diseño. No implementes código ni solicites implementación a Codex todavía.
> Tienes autonomía para proponer los parámetros y comportamientos que consideres adecuados, explicando sus fundamentos. No necesitamos seguir abriendo decisiones menores que puedas resolver técnicamente.
> Mi prioridad es que el juego se sienta natural, divertido e intuitivo, no que simplemente pase las pruebas.
> Presenta tu recomendación final y deja una propuesta suficientemente clara para que, una vez aprobada, Codex pueda implementarla sin ambigüedades.

## 2. Resumen

**Recomendación: «curva de potencia según la distancia».** La potencia del gesto M2-4 (P de 0 a 1) se sigue convirtiendo en velocidad entre `shot.vMin` (6 m/s) y `shot.vMax` (28 m/s), pero cerca del frontis la curva se vuelve cóncava hacia abajo (P elevado a un exponente γ mayor que 1).
- **Desde lejos**, a 6 m o más del punto pedido, la curva es la de hoy, byte a byte.
- **Cerca del frontis**, un gesto medio produce un golpe suave y uno fuerte sigue siendo fuerte: P=1 da la misma velocidad máxima que hoy.
- **Qué no cambia:** la física P2, First Ball, el gesto, la mira y los controles.
- **Alcance:** es una única función compartida por el humano, el rival, el pronóstico de la IA y el saque.

En lenguaje de squash: cerca de la pared no hay espacio para un swing completo y el jugador mide la fuerza. El mismo gesto «medio» desde la T es un golpe de ritmo, y pegado al frontis es un toque.

Para R3 la curva no basta por sí sola, y la propuesta lo dice con datos (sección 3.2). La recomendación es medir R3 como **tendencia**, igual que R1 y R2, y complementarla con el **tiempo hasta el segundo bote**, que es lo que de verdad hace menos letal un drop desde atrás. Esto último es una decisión del PO sobre el criterio, no un ajuste del producto.

## 3. Diagnóstico de la causa raíz

### 3.1 SL5/SL6: «desde adelante, la pelota sale lejos»

Datos de la sesión informal en iPhone del 13 de noviembre, build `0.1.0+78b2b65`:
- Contacto a unos 1,2 m del frontis, con la pelota a 0,17 m del suelo.
- Potencia típica P≈0,5, que da unos 17 m/s, y mira entre 1,2 y 1,3 m.
- Según el modelo de la sección 3.3, la pelota llega al frontis **todavía subiendo a unos 10 m/s**, rebota alta y su primer bote cae a 7–9 m. Algunos golpes tocan la pared trasera al vuelo.

La causa tiene tres piezas. Ninguna, por separado, es un defecto de la física:
1. **La velocidad depende solo de P:** `v = vMin + P·(vMax − vMin)` (`CausalQuality.Solve` y `ShotSolver.Solve`). Un gesto medio pide 17 m/s desde cualquier punto de la cancha.
2. **El solver elige el arco bajo (directo).** Desde 1,2 m, con la pelota casi en el suelo y una mira a 1,2 m o más, el arco bajo es casi una recta que sube unos 40°. P2 conserva la componente vertical tras el frontis, salvo la fricción (0,2), así que la pelota sigue subiendo y vuela al fondo. **Esto es físicamente correcto**: en squash real, un golpe a 17 m/s hacia arriba desde 1 m del frontis también se va al fondo o sale por arriba.
3. **Lo que no es natural es el input.** Ningún jugador real pega «a media fuerza» de 17 m/s pegado al frontis con la pelota casi en el suelo. Ahí se mide el golpe. El juego traduce la intención «media» igual en la T que en la pared, y eso es lo que el PO percibe como antinatural.

En S2 ya se registró el mismo síntoma (SL5/SL6 «muy arriba», [diagnóstico de #31](2026-11-04-first-shot-sl56-trajectory-diagnosis.md)): 13 de 24 golpes tocaron la lateral antes del primer bote y 9 de 24 quedaron fuera (out geométrico) antes de botar.

### 3.2 R3: «el drop desde atrás es más letal»

Según el Anexo C (spec v0.3), R3 pide que, con la misma intención y el mismo objetivo, el segundo bote del drop desde atrás quede **más lejos** del frontis que el del drop desde adelante. En #51 falló en los 42 pares: el drop desde atrás muere más corto, con diferencias de −1,65 a −0,07 m. Por ejemplo, a P≈0,15 y la misma mira, el segundo bote queda a 2,75 m adelante y a 1,83 m atrás.

La causa, confirmada con el modelo:
- **Sin resistencia del aire,** a igual P los dos drops llegan al frontis con la misma rapidez.
- **El de atrás llega más descendente,** porque recorre 6 m más con gravedad. Tras el frontis cae antes y muere más corto.
- **A potencia mínima, además,** el de atrás necesita más velocidad solo para llegar (el solver la eleva a unos 8,5–9 m/s), y eso lo hace llegar todavía más empinado.

**Hallazgo importante:** en squash real, un drop desde atrás también puede morir corto. Lo que lo hace menos letal es, sobre todo:
- **el tiempo**: tarda casi el doble en llegar al segundo bote, y el rival alcanza a llegar;
- **la precisión**: el error crece con la distancia. Eso corresponde a la capa aleatoria y causal, R6, que está diferida.

Medido como «segundo bote más lejos», R3 exige algo que la física determinista no produce sin una regla especial. Medido como «menos letal», ya se cumple: en el modelo, el drop desde atrás tarda 1,3–1,9 s en llegar al segundo bote, contra 0,8–0,9 s desde adelante, en el 100 % de los pares.

### 3.3 Cómo se obtuvieron los números

Los números son de un **modelo aproximado en 2D** (profundidad y altura) que replica las reglas de P2:
- gravedad de 9,81 m/s²;
- impulso normal con restitución de 0,55 en frontis y pared trasera, y de 0,45 en el piso;
- impulso de fricción tangencial μ·(1+e)·|vn|, con μ de 0,2 en las paredes y 0,3 en el piso;
- cancha de 9,75 m, tin a 0,48 m y out a 4,57 m;
- arco bajo idéntico al del solver;
- sin paredes laterales.

**Calibración:** el modelo reproduce lo observado en el iPhone. Con P=0,5 y mira de 1,2 m desde SL5/SL6, la pelota toca la pared trasera al vuelo.

**No es evidencia de aceptación.** Sirve para elegir el diseño. La implementación debe volver a medir todo con P2 real (sección 7). El script queda en el scratchpad de la sesión, fuera del repositorio.

## 4. Alternativas

| | Alternativa | Ventajas | Desventajas | Veredicto |
|---|---|---|---|---|
| **A** | **Curva de potencia según la distancia** (recomendada, sección 5) | No toca P2 ni First Ball. Es continua, sin zonas ni casos especiales. Conserva el golpe fuerte desde adelante (P=1 = vMax). Lejos del frontis es idéntica a hoy. Una sola función para humano y rival. | Cambia la sensación de la potencia media cerca del frontis, a propósito. Agrega 3 claves y cambia los hashes de defaults/P0. Por sí sola no da R3 con P=0. | **Recomendada** |
| B | Resistencia del aire en P2 | Es el fenómeno real; el squash tiene arrastre fuerte. | Cambia la física de First Ball: SimulationKey, gate C3/C10/C11, L1–L6 y el juicio H1. **Casi no arregla SL5/SL6** (en el modelo, P=0,5 a mira 1,2 m sigue en 9,4 m), porque el problema es la subida, no el recorrido. Acorta todavía más el drop desde atrás, así que R3 empeora. | Descartada ahora. Reservarla por si el playtest pide una pelota «menos flotante». |
| C | Escalar toda la velocidad con la raíz de la distancia | Simple y con base física (el alcance de un proyectil crece con v²). | Desde adelante, P=1 queda en unos 13 m/s: **desaparecen los golpes fuertes y los kills desde la parte delantera**. | Descartada |
| D | P como «profundidad deseada» (el solver busca la velocidad que hace caer la pelota en z*(P)) | Muy legible. | Quita la consecuencia física de la posición y obliga a buscar con P2 en cada golpe. Hace R3 igual por construcción, que es justo lo artificial. Cambia la semántica de M2-4. | Descartada |
| E | Arco según intención (a P baja, la pelota llega al frontis bajando, como un toque) | Es la técnica real del drop. | Cambia qué significa la mira, que hoy es el punto de impacto alcanzado con el arco bajo. Pide un feedback nuevo y cambios en la IA. | Candidata para una etapa futura, no ahora |
| F | Reglas especiales para SL5/SL6 o para los pares de R3 | — | Artificial; es lo que el PO pidió evitar. | Rechazada |

## 5. Recomendación: definición exacta

### 5.1 Fórmula

Para cada golpe, humano o rival, con P∈[0,1], centro de la pelota en el contacto `c` y punto pedido en el frontis `a` (mira solicitada **antes** de la capa causal):

```
D      = distancia euclídea 3D entre c y (a.x, a.y, radio)        // incluye altura y dirección
w      = clamp((D_full − D) / (D_full − D_near), 0, 1)
γ      = 1 + (γ_max − 1) · w
v_req  = vMin + P^γ · (vMax − vMin)
```

Después, en este orden, sin cambios:
1. La capa causal aplica su fracción de velocidad (`v_req · (1 + pérdida)`) y su altura (D36).
2. Se acota la mira al frente legal (`BoundToLegalFront`).
3. El solver eleva la velocidad si la requerida es mayor (`required`) y baja la mira si hace falta (`TargetLowered`).

Propiedades, que se exigen como pruebas:
- **Extremos:** P=0 da vMin y P=1 da vMax, con cualquier D.
- **Monotonía en P:** a igual D, la velocidad es estrictamente creciente en P. R1 se conserva por construcción en la velocidad pedida.
- **Monotonía en D:** a igual P∈(0,1), la velocidad no decrece al aumentar D.
- **Continuidad** en D y en P, sin saltos ni zonas.
- **Identidad lejos del frontis:** con D ≥ D_full, o con γ_max = 1, `v_req` es **bit a bit igual** a la fórmula actual. Para garantizarlo, cuando γ = 1 la implementación usa literalmente la fórmula de hoy (retorno temprano), sin pasar por la potencia.
- **Determinismo:** depende solo del contacto, la mira pedida, P y la configuración. No depende de Δ, de la categoría ni del tiempo.

### 5.2 Parámetros [EXPERIMENTO]

| Clave | Valor | Rango | Fundamento |
|---|---|---|---|
| `shot.paceGammaMax` | 2,0 | 1–3 (1 = curva actual) | Con 2,0, una potencia media pegada al frontis cae en media cancha (2.º bote a unos 5–6 m) y un toque (P≈0,25) muere a unos 2 m. Con 2,5 quedaba más corta que la intención de «media». |
| `shot.paceNearM` | 1,5 | 0,5–4,0 | Distancia típica de un contacto pegado al frontis, como SL5/SL6. Por debajo de ella, la curva es la máxima. |
| `shot.paceFullM` | 6,0 | 2,0–9,75, mayor que `paceNearM` | Un poco más allá de la short line, que queda a 5,49 m del frontis. Desde la T hacia atrás, la potencia es exactamente la de hoy. |

Las tres se cargan en el catálogo y en `DefaultPreset.json`. El validador exige `paceNearM < paceFullM`. Por defecto queda **activa con 2,0**: el objetivo es que el juego se sienta natural, y el valor 1 permite volver atrás desde el panel.

### 5.3 Dónde se aplica (una sola ruta)

1. **Función nueva compartida** en Core, por ejemplo `ShotPace.Requested(P, contact, aim, config)`. Es la única que calcula la velocidad pedida.
2. **`CausalQuality.Solve`:** reemplaza la línea `float requested=vMin+power*(vMax−vMin)`.
3. **`RallyContact.SolveIntention`:** la usan el pronóstico de la IA (`FondoAi`) y `ServePlanner`. Deja de pasar `VMin/VMax` y pasa la velocidad de la curva como extremos iguales, igual que ya hace `CausalQuality.Solve`.
4. **`ShotSolver.Solve`** no cambia su interfaz ni sus reglas (factibilidad, cap, verificador).
5. **Experimentos explícitos que llaman al solver directamente:** `SportScenarioExperiment` y `AnticipationH3bExperiment` pasan a la función compartida para medir el producto real. Ese cambio se declara antes de correrlos.
6. **Registro:** `ShotRecord` y la traza causal guardan `paceDistanceM`, `paceGamma`, `requestedSpeedLinear` (la de hoy) y `requestedSpeed` (la de la curva).

## 6. Comportamiento esperado

Valores del modelo de la sección 3.3, sin paredes laterales; la implementación los vuelve a medir con P2 real.
- **«Hoy»** es la fórmula vigente y **«propuesta»** es la curva con γ_max=2,0.
- La distancia es el segundo bote, medida desde el frontis.
- «PT» significa que la pelota toca la pared trasera antes de botar.

| Desde | Mira | P=0,25 hoy → propuesta | P=0,5 hoy → propuesta | P=1 (igual) |
|---|---|---|---|---|
| Adelante, pelota baja (SL5/SL6, 1,2 m) | drop (tin+0,10) | 3,4 → **1,9 m** | 5,4 → **3,4 m** | 9,0 m |
| | media 1,2 m | 5,8 → **2,1 m** | 8,6 PT → **5,9 m** | sale por arriba |
| | alta 2,0 m | 4,8 → **1,7 m** | sale por arriba → **5,4 m** | sale por arriba |
| Adelante, pelota cómoda (1,5 m) | drop | 3,6 → **2,1 m** | 5,6 → **3,6 m** | 9,4 m |
| | media 1,2 m | 4,9 → **2,3 m** | 9,0 → **4,9 m** | PT |
| Media cancha (5 m) | drop | 3,6 → 3,0 m | 5,5 → 5,0 m | 9,4 m |
| | media 1,2 m | 4,7 → 3,9 m | 7,6 → 6,8 m | 8,2 m |
| Fondo (8,3 m) | drop | 3,2 m (igual) | 5,5 m (igual) | 9,4 m |
| | media 1,2 m | 4,5 m (igual) | 7,6 m (igual) | 8,2 m |

En palabras:
- **Adelante:**
  - El gesto suave es un toque que muere en el tercio delantero.
  - El medio deja la pelota en media cancha, cerca de la short line, y no al fondo.
  - El fuerte sigue siendo un golpe fuerte que va al fondo.
  - Pegar fuerte, desde muy cerca y con la pelota baja, hacia una mira media o alta sigue saliendo por arriba. Es una consecuencia física que conserva la propuesta (sección 8.3).
- **Media cancha:** la curva suaviza poco, alrededor de 10 % de velocidad a potencia media. Los golpes de ritmo se mantienen.
- **Fondo:** idéntico a hoy. El drive paralelo, el cruzado, el drop defensivo (X1, X2 y X4) y las bandas C1, C2 y C4 no cambian.

## 7. Criterios de aceptación

**Cómo se mide:**
- Con P2 real, en un experimento explícito en `Tests/Experiments`, fuera de las suites de regresión.
- Método predeclarado en un commit anterior a la ejecución.
- Dos corridas en procesos separados, idénticas byte a byte (SHA-256).
- Se reportan todos los casos válidos y todos los no evaluados, con su motivo.
- No se ajustan parámetros después de ver los datos (sección 9, regla de parada).

### 7.1 SL5/SL6 (adelante)

**Muestra:**
- contactos de las recetas SL5 y SL6 en T* y en los bordes de la ventana Valid;
- miras en una grilla de x y de alturas: drop (tin+0,05/0,10/0,15), media-baja (hasta 1,6 m) y banda media (1,6 ≤ y < 2,6 m);
- P = k/20.

**Criterios:**

| # | Criterio | Umbral |
|---|---|---|
| S1 | Potencia media no manda al fondo: P∈[0,40; 0,60], miras hasta la banda media. Se cuentan los golpes resueltos y legales. | Segundo bote a 6,5 m o menos y sin tocar la pared trasera al vuelo, en ≥ 90 % de los casos |
| S2 | Golpes cortos posibles: P ≤ 0,25 con miras de drop | Segundo bote a 2,0 m o menos (banda C3) en ≥ 90 % |
| S3 | El fuerte sigue fuerte: P = 1 | Velocidad pedida bit a bit igual a la de hoy (prueba unitaria). Con miras de drop, 2.º bote a 8 m o más, o pared trasera. |
| S4 | Monotonía: igual contacto y mira, sin paredes antes del primer bote | Se reporta como tendencia la fracción de pares de potencias adyacentes con primer bote creciente. Se espera 100 % por construcción. |
| S5 | Sin cambios lejos del frontis: D ≥ 6 m | Velocidad pedida bit a bit igual a la de hoy. X1, X2 y X4 conservan sus testigos. |
| S6 | Juicio del PO en el playtest integrado | Respuesta literal a: «¿Desde adelante puedes hacer golpes cortos y controlados, y la potencia media ya no se va al fondo?» |

### 7.2 R3 (drop desde atrás)

El arnés del Anexo C y su emparejamiento no cambian (F a 1,5 m, B a 7,5 m, tres márgenes sobre el tin, P de 0 a 0,30). Solo cambia que la velocidad pase por la función compartida.

| # | Criterio | Umbral | Naturaleza |
|---|---|---|---|
| R3a | z2(B) > z2(F), la definición de la spec | Tendencia: ≥ 75 % de los pares válidos con P > 0. Los pares con P = 0 se reportan aparte y explícitamente, con su comportamiento y su causa (velocidad mínima y elevación del solver); no se ocultan ni cuentan como Pass. El modelo predice 12 de 12 pares con P > 0. | **Aprobado por el PO** como tendencia. El criterio original conserva su trazabilidad: se informa también la fracción sobre todos los pares, incluidos los de P = 0. |
| R3b | Tiempo desde el contacto hasta el segundo bote, t2(B) > t2(F) | 100 % de los pares válidos. El modelo predice 15 de 15. | **Complemento aprobado por el PO, no sustituto.** Es un **indicador físico, no una demostración completa de letalidad**: la efectividad real depende también de la posición, el desplazamiento, la anticipación y la respuesta del rival, que se evalúan en First Rally. |
| R3c | Bandas C3 y C4 | Se informan | Informativas, como hoy |
| R3d | Playtest integrado | Respuesta literal a: «¿El drop desde atrás te parece demasiado letal?» | Capa 4, palabra final del PO |

R3a y R3b no son ajustes para pasar la prueba. R3a mide lo mismo que la spec, como tendencia. R3b mide un indicador físico de por qué un drop desde atrás es menos letal; la letalidad completa se evalúa en First Rally.

### 7.3 Regresiones que deben mantenerse

- **R1 y R2:** se reportan otra vez como tendencias. La curva es monótona, así que no deberían empeorar.
- **X1–X5, C1–C4 y X6/C7 (saque):** se informan. El saque desde el cuadro queda, según mi estimación, a unos 5,6 m del punto pedido, con γ≈1,09; hay que confirmar que sigue entrando en el cuarto contrario.
- **Rally sim de 1.000 rallies:** cero fallas del sistema. Se reportan los cambios en las causas (Out, Air, Late) y en el ganador, sin tocar pesos ni cinemática.

## 8. Riesgos

### 8.1 First Ball

- **Sin cambios físicos.** No cambian P2, sus parámetros ni SimulationKey `166a96e319c7`. El lanzador (L1–L6) no usa el solver, así que el gate de First Ball, H1 y H17 no se ven afectados.
- **Defaults:** las 3 claves nuevas cambian el hash de defaults, el de P0 efectivo y los anclajes de AutoQA, que se recalculan en Unity. La proyección heredada `0d39e6016bd8` (41 claves) no cambia.

### 8.2 First Shot

- **Sensación de potencia cerca del frontis.** Un gesto medio cerca del frontis se vuelve suave, que es el objetivo. Las bandas del gesto de S1/S2 se definieron sobre P, no sobre la velocidad, así que siguen valiendo; la sensación del golpe resultante se juzga en el playtest. La saturación fuerte (P=1) sigue siendo una observación abierta y no cambia.
- **Timing y contrato:** sin cambios. El vector causal (D36) tampoco cambia; solo su velocidad base. AutoQA es obligatorio igual (el cambio toca el golpe) y debe dar PASS/0/nivel 1 con los anclajes nuevos.
- **IA y rally:** el rival también pega más suave desde adelante, porque es simétrico. Pueden cambiar los Out (hoy 59 de 1.000, casi todos por pared trasera) y el reparto de ganadores. Se mide y se reporta; no se compensa.
- **H3b y Anexo C:** sus números cambian levemente cerca del frontis; se vuelven a correr con el arnés declarado.
- **PR #32** (regla de devolución, desactivada): no interactúa ahora. Si algún día se activa, se vuelve a evaluar sobre la curva.

### 8.3 Riesgos residuales, sin regla nueva

- **Golpe fuerte desde muy cerca y con la pelota baja hacia una mira media o alta:** sale por arriba, hoy y con la propuesta. Es la física correcta. Conviene observarlo en el playtest. Si molesta, la opción natural es el arco según intención (E) en otra etapa, no una excepción.
- **El modelo es aproximado:** no tiene paredes laterales y la fricción es simplificada. Los umbrales de la sección 7 se evalúan con P2 real; si fallan, aplica la regla de parada.
- **El drop desde atrás con P = 0 seguirá muriendo corto** a menos que exista una capa de precisión (R6). Eso queda dicho y no se esconde.

## 9. Encargo de implementación para Codex (cuando el PO lo autorice, tras revisar esta PR)

1. **Rama, worktree y PR independientes** desde main, después de fusionar esta PR de diseño. Primer commit: el literal de la autorización de implementación del PO y los cierres pendientes de registrar (#54 y esta PR de diseño), en AGENTS.md y aprobaciones.
2. **TDD:** esqueleto compilable y RED por aserción antes de cada GREEN. Pruebas mínimas:
   - extremos P=0 y P=1;
   - monotonía en P y en D;
   - continuidad en D_near y D_full;
   - identidad bit a bit con D ≥ D_full y con γ_max = 1;
   - determinismo;
   - misma función para humano, rival, pronóstico y saque;
   - orden curva → causal → acotamiento → solver;
   - validador `paceNearM < paceFullM`;
   - campos nuevos del registro.
3. **Claves y hashes:** las tres claves de la sección 5.2 en el catálogo y en `DefaultPreset.json`. Hashes de defaults/P0 y anclajes de AutoQA medidos en Unity, en un commit propio, sin copiarlos.
4. **Experimento explícito, método predeclarado:**
   - SL5/SL6 (S1–S5) y el arnés del Anexo C completo (R1, R2, R3a, R3b, X1–X7 aplicables y C1–C7), con los experimentos pasando por la función compartida;
   - se evalúan **dos juegos de parámetros predeclarados en la misma corrida**: γ_max = 2,0 (principal) y 2,5 (respaldo);
   - regla: se adopta 2,0 si cumple S1–S3, R3a y R3b; si no, 2,5 si los cumple; si ninguno cumple, **se detiene, se presenta el diagnóstico y se vuelve a revisar el diseño** antes de modificar cualquier parámetro, sin buscar otros valores;
   - dos corridas idénticas byte a byte.
5. **Rally sim** de 1.000 rallies, regenerado con explicación, sin ajustar pesos ni cinemática.
6. **Verificación, un solo Unity a la vez:**
   - EditMode y PlayMode completas con XML fresco;
   - AutoQA PASS/0/nivel 1;
   - check-release dev y prod con `gitDirty=false`;
   - ProjectSettings transitorios restaurados.
7. **Fuera de alcance:**
   - resistencia del aire;
   - arco según intención;
   - cambios en P2, en M2-4, en el contrato o en las ventanas;
   - #32;
   - iPhone, build nativa e instalación.
   
   Ante un defecto o una sorpresa: registrar y detenerse.
8. **Entrega:** informe y PR lista para la revisión de Claude coordinada por el PO, **sin fusionar**. La aceptación queda para el playtest integrado (S6 y R3d).

## 10. Decisiones del PO

Las tres decisiones quedaron **aprobadas** el 14 de noviembre de 2026 (literal en la sección 11):

1. Curva de potencia según la distancia, con γ_max = 2,0 principal y 2,5 como respaldo predeclarado.
2. R3a como **tendencia** (≥ 75 % de los pares válidos con P > 0), con los pares de P = 0 reportados explícitamente.
3. **R3b** (tiempo hasta el segundo bote) como complemento, entendido como indicador físico y no como demostración completa de letalidad.

Todo lo demás (fórmula, distancias, registro, pruebas y orden de aplicación) son decisiones técnicas de Claude, propuestas como parte del diseño.

## 11. Decisión literal del PO (14 de noviembre de 2026)

> Claude, revisé la propuesta completa y apruebo avanzar con el diseño, con las siguientes decisiones como PO:
> 1. APRUEBO la curva de potencia según distancia.
>
> * γ_max = 2,0 como alternativa principal.
> * γ_max = 2,5 como respaldo predeclarado.
> * Mantener P2, First Ball y M2-4 sin modificaciones.
> * Mantener los extremos de potencia y el comportamiento actual desde 6 m.
> * Aplicar una única función compartida, sin reglas especiales por escenario.
>
> 2. APRUEBO tratar R3a como tendencia ≥75 %, considerando los pares válidos con P > 0.
> Los pares con P=0 deben seguir reportándose explícitamente, incluyendo su comportamiento y explicación. No quiero ocultar resultados desfavorables ni perder la trazabilidad del criterio original.
> 3. APRUEBO incorporar R3b, con tiempo hasta el segundo bote mayor desde atrás en el 100 % de los pares válidos.
> Sin embargo, dejemos explícito que el tiempo hasta el segundo bote es un indicador físico, no una demostración completa de letalidad. La efectividad real dependerá también de la posición, desplazamiento, anticipación y capacidad de respuesta del rival, que evaluaremos en First Rally.
> Consideraciones adicionales:
>
> * Los números del modelo aproximado son hipótesis, no resultados validados.
> * Codex debe medir nuevamente con P2 real.
> * Se mantiene la regla de parada: probar 2,0 y 2,5 sin buscar otros valores si ambos fallan.
> * Si las pruebas no cumplen, volvemos a revisar el diseño antes de modificar parámetros.
> * La aceptación final de sensaciones sigue dependiendo del playtest en iPhone.
> * No autorizo todavía el merge ni cambios adicionales de alcance.
>
> Registra estas tres decisiones literalmente en la documentación correspondiente, prepara el commit y abre la PR de documentación sin fusionarla.
> Déjame también el encargo definitivo para Codex, alineado con estas decisiones.
> Una vez revisada la PR de diseño, avanzaremos con la implementación.

Registrada también en el [registro de aprobaciones de First Shot](2026-10-25-first-shot-po-approvals.md). La implementación no está autorizada por esta decisión: se avanza después de revisar la PR de diseño. Sin merge ni cambios de alcance.
