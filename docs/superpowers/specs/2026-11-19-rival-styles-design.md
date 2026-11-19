# Estilos del rival, volea del rival y calibración de la dificultad: diseño (fase 1)

19 nov 2026 · Subagente Opus dirigido por Claude (product lead, delegación del PO del 18 nov); **la revisión de Claude no es independiente**. Rama `claude/rival-styles`, base `f068cee` (integración de #84: G1 A2, volea v1, reajuste del cansancio, `VolleyEnergyCost`). Solo diseño y medición, sin cambios de producto. Fuentes: spec v0.3 §9.3 y §11.2–11.5, [volea v1](2026-11-18-volley-design.md), [G1 A2](../../reviews/2026-11-18-g1-a2-complete.md).

Pedido del PO, literal: «El diseño de estilos debería ser el jugador fondista, que juega mayormente al fondo. El one shoter que trata de definir todo, y un all arround. Además que el rival pueda volear.» Más el plan aprobado: calibrar Fácil/Medio/Difícil para que un usuario nuevo no se frustre y corregir la posición del rival ante golpes cortos.

**Medición:** experimento explícito `RivalStylesBaselineExperiment` (fuera de la regresión), con defaults `207da8f2c636`/132, RallyParams `5cdbb01e4621` y SimulationKey `166a96e319c7`. 200 partidos por celda, con las mismas semillas en todas; dos procesos dan las celdas comunes idénticas. Evidencia en `docs/validation/evidence/2026-11-19-rival-styles/`: `run02` en `f1a7a77`, `cover01` en `8365e6c` y `cover02` en `b838410`. Los TSV grandes van a `Logs/` (ignorado), con su SHA-256. `run01` se detuvo en su primera `SystemFault` (SF-01, §6).

## 1. Tres estilos: solo cambian cómo elige, no lo que puede hacer

Un estilo es un juego de pesos de la utilidad de §11.3 (`U = Σ peso·rasgo − aversión·riesgo`), su disposición a volear y su posición de espera W. **No cambia** física, reglas, alcance, cinemática, ventanas K/A, modelo de error, `σ` de ningún tipo ni la información (§11.2): todo rasgo nuevo sale de la observación (`AiObservation`) o del plan propio. La dificultad sigue siendo **otro eje**: los seis parámetros de ruido por nivel (`d_perc`, `σ_pred`, `σ_int` de mira y potencia, `σ_timing`, temperatura), compartidos por los tres estilos (§11.5, «difficulty changes these and nothing else»). La temperatura **no** es del estilo: el «riesgo» del estilo es la aversión al riesgo de la utilidad.

Rasgos nuevos (además de largo, lejos del rival, tiempo·necesidad y riesgo, que ya existen): `ataque` (drop o golpe bajo), `adelante` = cuánto delante de la T está el propio contacto P\* (0–1 en 2 m), y la apertura del drop por zona propia (no solo con el rival atrás).

| Peso (EXPERIMENTO) | Fondista | Definidor (one-shooter) | Completo (all-around) |
|---|---|---|---|
| Largo / lejos / tiempo | 1,2 / 0,8 / 0,8 (hoy) | 0,5 / 0,8 / 0,3 | 1,0 / 0,8 / 0,6 |
| Drop: peso; abierto si… | 0,9; rival detrás de T+0,5 y holgura ≥0,3 s (hoy) | 1,6; además desde P\*z ≤ T+1 m | 1,1; además desde P\*z ≤ T |
| `ataque` base + `adelante` | 0 + 0 | 0,8 + 0,4 | 0 + 1,0 (ataca la pelota corta, defiende la larga) |
| Aversión al riesgo | 1,6 (hoy) | 0,6 | 1,1 |
| Defensa obligatoria `s_def` | 0,12 s (hoy) | 0,06 s | 0,12 s |
| Volea: sesgo; K mínima | −1,0; 0,10 s | +0,8; 0,05 s | 0; 0,08 s, +0,6·«tiempo robado» |
| W detrás de la T (§11.4) | 0,5 m (hoy) | −0,3 m (adelante) | 0,2 m |
| Cobertura tras golpe corto propio (§4) | a la T | 1,5 m detrás del bote | a la T |

**Fondista = el Fondo de hoy** con los mismos pesos: a nivel Medio hoy juega 92 % largo (49 % cruzado, 42 % paralelo, 8 % drop; 50 % alto). Los rasgos nuevos valen 0 para él, así que su utilidad es bit a bit la actual. Identidad de estilo predeclarada, medida en cada nivel tras calibrar: Fondista ≥ 80 % de golpes largos; Definidor ≥ 35 % de drop+bajo y ≥ 2× la tasa de volea del Completo; Completo entre ambos y con más ataque desde adelante que desde atrás. Contrajuego esperado (§11.4): al Fondista se le gana llevándolo adelante; al Definidor, alargando y pegando largo cuando se adelanta.

**Dónde viven.** Campos `double` nuevos en `RallyParams` (`Shooter*`, `AllRound*`, más `RivalVolleyEnabled` y `Cover*`); los de Fondista son los `Fondo*` actuales. `RallyParams.Hash` cambia; un test exige que la proyección sin los campos nuevos sea exactamente `5cdbb01e4621`. `DefaultPreset.json` no cambia. Código: `RivalAi(context, difficulty, RivalStyle, seed)` nuevo; `FondoAi` queda **intacto** como referencia histórica (sin volea ni cobertura), de modo que las firmas congeladas y el Experimento B se reproducen sin editar ningún test.

## 2. Elección del estilo (decidida)

Aleatoria por partido con una semilla de partido (`MatchSeed`): `RivalStyle = MatchSeed % 3` sobre `RallyRng`; los tests la fijan. La revancha conserva el rival. Se muestra al empezar el partido («Rival: Fondista», en el aviso del primer punto) y en el marcador (columna «RIVAL · FONDISTA»). DEV: «Estilo: Auto/Fondista/Definidor/Completo», rige desde el partido siguiente (precedente #82). Métricas: `style` en `rally-point` y `match-start`. Sin selector en el menú (lo rediseña otro agente). **Hallazgo:** hoy `RallyDirector.Seed` vale siempre 1, así que todos los partidos usan la misma secuencia aleatoria de la IA; `MatchSeed` alimenta también la semilla de `RivalAi`.

## 3. Volea del rival

Misma oportunidad que el humano: `RallyContext.VolleyFor` sobre el camino congelado del cuerpo (M1), mismas reglas, alturas, K, separación y **sin volea del saque**. Ya se calcula en `RallySim` para el rival; falta en `RallyDirector.PlanRival` (se agrega igual que en `LaunchToHuman`, con su `CausalReference` por oportunidad, D36).

- **Decisión (§11.3 paso 3):** si hay oportunidad y el estilo acepta su K, la IA evalúa las 18 opciones también desde P\*v; utilidad de volea = U(opción en P\*v) + sesgo del estilo + peso·tiempo robado (T\*b − T\*v) − aversión·riesgo de ventana (1 − K_v/K_b). Un solo softmax sobre las 36, misma temperatura y mismo filtro de faltas pronosticadas. **Un solo intento.**
- **Soltar:** Δ ~ N(0, σ_timing) respecto de T\*v, con `RivalReleaseTiming.Adjust` sobre la K de la volea; el instante se juzga con `OpportunityContract.Select` (volea hasta S, bote después), exactamente como el del humano. Así un Δ muy tardío no puede convertirse en un golpe de bote gratis ni en una volea accidental.
- **Energía:** `VolleyEnergyCost` (2 puntos) por la misma regla que el humano; ya está en `RallySim` y `RallyDirector`.
- **Medido hoy:** el rival casi nunca tiene volea ofrecida porque espera detrás de la T: 0,9 % de sus recepciones con W 0,5 m, 3,8 % con W en la T y 13,9 % con W 0,5 m adelante (humano en la T: 3,2 %). La W del estilo es lo que vuelve visible la volea del Definidor.
- **RallySim y referencias:** las firmas congeladas (`FatigueIntegrationTests`, `FatigueArmRallyTests`) y el Experimento B usan `FondoAi`, que no cambia: se reproducen bit a bit sin tocar tests. Las referencias nuevas, con estilos, volea y cobertura, se miden con un experimento explícito y alimentan un test de identidad propio. `RivalVolleyEnabled=0` es el control sin volea.

## 4. Posición del rival ante golpes cortos desde adelante

**Medido (Medio, W 0,5 m, z de espera 5,97 m).** Cuando el humano golpea a menos de 2 m del frontis, el rival no llega en el **15,7 %** de esas recepciones con el novato (n=376) y en el **53,4 %** con `bot30` (potencia media, n=569), contra 0,0 % cuando el humano golpea desde detrás de la T. Con `bot30` esos puntos son el **16,4 %** de todos (29,5 % en Fácil). Su holgura mediana es 0,00 s. **El 97 % de esos golpes del humano vienen después de un drop del propio rival**: juega corto y se recupera 0,5 m detrás de la T.

**Por qué no basta mover W.** W fija adelante empeora todo: novato contra Medio, puntos del humano 21,6 % (W 0,5) → 32,2 % (W 0) → 40,5 % (W −0,5), y el «no llega» tras golpe desde adelante sube a 32,9 %, porque la pelota larga lo pasa.

**Contrafáctico de cobertura** (sin código de producto: el experimento reprograma la recuperación del rival justo después de su propio golpe, usando solo la pelota recién golpeada y el predictor compartido; si su bote predicho cae a menos de 3,5 m, el rival se recupera a `profundidad` detrás de ese bote):

| Medio, 200 partidos | Puntos del humano | «No llega» tras golpe del humano a <2 m |
|---|---|---|
| bot30: hoy / cobertura 1,5 m / 2,5 m / a la T / 3,5 m | 23,9 / 6,1 / 7,8 / **10,7** / 7,5 % | 53,4 / 0,0 / 0,0 / **3,4** / 0,0 % |
| novato: hoy / 1,5 m / 2,5 m / a la T / 3,5 m | 21,6 / 31,5 / 28,3 / **19,9** / 23,9 % | 15,7 / 50,0 / 46,6 / **13,5** / 39,4 % |

Cubrir adelante es una **apuesta** (§9.3): resuelve la pelota corta y abre la larga. **Recuperarse a la T tras el golpe corto propio domina a hoy contra las dos referencias** («no llega» 53,4→3,4 % y 15,7→13,5 %; desde detrás de la T sigue en 0,0 %). **Corrección mínima propuesta:** esa regla, con información observable únicamente (la pelota en el instante del contacto propio y el predictor compartido, simétrico al planificador humano), para Fondista y Completo; el Definidor cubre 1,5 m detrás del bote (apuesta del estilo). Así el contrajuego de §11.4 queda medible: al Definidor se le gana con la pelota larga. La corrección fortalece al rival contra el jugador tipo PO (bot30 23,9→10,7 %); por eso se calibra después. **Criterio predeclarado (fase 2, implementación real):** Fondista en Medio, «no llega» tras golpe del humano a <2 m ≤ 10 % con `bot30` y ≤ 16 % con el novato; desde detrás de la T, ≤ 0,5 %.

## 5. Calibración de la dificultad

**Rival de referencia («novato»), fijado antes de medir:** `NewcomerBot` (en el experimento): no mira al rival; elige paralelo o cruzado al 50 %, altura media y potencia fuerte el 60 % (mediana real de P del PO = 0,83); Δ ~ N(0, **90 ms**), la dispersión real del PO en las builds T3 (`2b82701`, `c6f9cef`: σ 98 y 84 ms, media ≈ 0). No volea. Por qué no `AnticipationBot`: es un programador de soltar para `ShotController` (offset fijo, sin intención), no un `IRallyBrain`. Se descarta `bot30` como referencia de novato porque se parece al PO: contra Medio da 23,9 %, dentro del IC del PO real antes de A2 (11/36 = 31 %, IC 18–48 %); en Fácil, 44,3 % frente a 18/32 = 56 % (IC 39–71 %).

**Medición base (Fondo de hoy, % de puntos del novato, IC 95 %):**

| | Fácil | Medio | Difícil |
|---|---|---|---|
| novato (σ 90) | **40,5** [38,5–42,6] · 45/200 partidos | **21,6** [19,8–23,6] · 4/200 | **5,3** [4,2–6,5] · 0/200 |
| bot30 (σ 30, paralelo medio) | 44,3 [42,3–46,3] · 58/200 | 23,9 [22,0–25,8] · 1/200 | 9,7 [8,4–11,3] · 0/200 |

Factibilidad: multiplicar los seis parámetros de Fácil por 1,5 da 55,9 % y por 2 da 73,5 %; los objetivos se alcanzan solo con el ruido.

**Objetivos (puntos ganados por el novato, por estilo):** Fácil **60 %**, Medio **45 %**, Difícil **30 %**. Tolerancia: el promedio de los tres estilos dentro de ±4 pp del objetivo y cada estilo dentro de ±8 pp; orden Fácil > Medio > Difícil con IC sin solaparse en cada estilo; identidad de estilo de §1 conservada. **Tamaño:** 200 partidos por celda (≈1.800–2.300 puntos, IC ±2 pp), 3 estilos × 3 niveles, semillas fijas. **Método:** solo los seis parámetros de cada nivel; un factor k por nivel aplicado a los cinco ruidos de ejecución y percepción (la temperatura queda fija para que los estilos sigan siendo legibles; pregunta 4), buscado en la malla k ∈ {1; 1,25; 1,5; …; 3} con la primera k que cumple; después **una** confirmación con semillas nuevas. Si la confirmación falla: registrar y detenerse, sin otra búsqueda. Se calibra **después** de implementar estilos, volea y cobertura, porque cambian la fuerza del rival. La validación final es en el iPhone: `rally-point` ya registra nivel y, con este trabajo, estilo.

## 6. Plan TDD (fase 2), compuertas, hashes y riesgos

0. **Inventario** de tests de PlayMode que dependen de decisiones del rival (`ZRallyDirectorTests`, `ZCharacterFeelTests`, capturas). Regla: si alguno cambia de resultado con `RivalAi`, se reporta; no se tocan valores.
1. `RallyParams`: campos y test de proyección `5cdbb01e4621`. RED por aserción.
2. `RivalAi` + `RivalStyle`: RED de identidad (Fondista con volea y cobertura apagadas = `FondoAi` decisión a decisión, 3.000 muestras; reproduce las firmas congeladas), luego rasgos por estilo (cuotas de opciones por estilo en observaciones fijas) y contrato de información (mismo test de reflexión que `FondoAi`).
3. Volea en la decisión: RED (sin oportunidad nunca volea; Definidor volea más que Fondista con la misma oportunidad; Δ juzgado con `OpportunityContract.Select`; un solo intento; costo de energía).
4. Cobertura: interfaz opcional `IRallyPositioning` (sin romper `BotBrain` ni los envoltorios de los experimentos); RED en `RallySim` (tras golpe corto propio se recupera según el estilo; sin golpe corto, W de siempre).
5. `RallyDirector`: `MatchSeed`, estilo, volea del rival con su referencia causal y pose de volea en `SwingView`; HUD «RIVAL · ESTILO»; DEV; métricas. RED en PlayMode.
6. Experimento explícito de referencias nuevas y de calibración (§5), commit limpio antes de correr.

**Compuertas:** EditMode y PlayMode completos; AutoQA `--suite v`, `--suite t3` y `--suite volley` PASS/0/1 sin dirty; `check-release.sh` dev/prod OK; informe de una página. Sin fusión ni instalación sin el «sí» del PO.

**Hashes esperados:** RallyParams cambia dos veces (campos nuevos; luego los valores calibrados de los seis parámetros por nivel), cada vez con su proyección declarada. Sin cambios: defaults `207da8f2c636`/132, P0, SimulationKey `166a96e319c7` y anclas AutoQA (no dependen de RallyParams). Si alguna ancla cambiara, solo el literal, según el procedimiento A2 v2.2.

**Archivos:** `Core/Rally/RivalAi.cs` (nuevo), `RallyParams.cs`, `RallySim.cs`, `RallyContext.cs`, `RallyText.cs`; `Runtime/Rally/RallyDirector.cs`, `RallyHud.cs`, panel DEV, `SwingView`/`CharacterView` (pose de volea del rival); tests EditMode (`RivalAiTests`, `RallySimTests`, `VolleyRallyTests`) y PlayMode (`ZRivalStyleTests`); experimento de calibración.

**Riesgos:**
- RallySim no es un humano: el PO real ganaba más en Fácil que cualquier bot. Se valida en el iPhone.
- El ruido alto puede borrar los estilos; por eso la temperatura fija y la identidad predeclarada.
- 36 opciones duplican el costo de decisión; se mide el p95 en el editor.
- Conflicto textual con el rediseño del menú; aquí solo se tocan el HUD y DEV.

**Observación SF-01:** 26 `SystemFault` «target-or-contact-outside-court» en 19.371 puntos de `run02`. Con el ruido normal hay ≈1 cada 2.000 puntos, y más con ruido alto. Aparecen en los dos jugadores; las del rival son paralelo alto medio. Se registra sin corregir.

**Preguntas para Claude:**

1. ¿W y cobertura como rasgos del estilo? §11.4 los lista y sin ellos el rival no tiene voleas, pero el encargo solo nombraba pesos, riesgo y volea.
2. «Temperatura de riesgo» interpretada como aversión al riesgo; la temperatura queda en la dificultad.
3. Ruido compartido por los tres estilos (tolerancia por estilo ±8 pp) o un juego por estilo y nivel.
4. Calibrar con la temperatura fija o con las seis.
5. σ del novato fijo en 90 ms o ajustado para igualar el 14 % de recepciones sin contacto del PO real (el novato hoy falla el 7 %).
6. ¿Los objetivos 60/45/30 son los del PO? Con el ruido actual quedan lejos (40/22/5).
7. Nombres visibles: «Fondista», «Definidor», «Completo».
8. ¿SF-01 se diagnostica aparte?
