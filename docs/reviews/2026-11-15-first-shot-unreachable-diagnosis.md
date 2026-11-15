# Diagnóstico del aumento de `Unreachable` con la curva de potencia (PR #56)

15 de noviembre de 2026. Operador: subagente Sonnet dirigido por Claude (revisión no independiente de la dirección). Solo diagnóstico: no se tocó producto, defaults, física, solver, contrato, IA ni pruebas existentes. Pedido del PO (literal, registrado en `3fc0460`): «Pedir un diagnóstico específico del aumento de `Unreachable`, separando golpes, posiciones y jugador receptor.» Método predeclarado y confirmado en Git antes de ejecutar: [`docs/validation/2026-11-15-unreachable-diagnosis-method.md`](../validation/2026-11-15-unreachable-diagnosis-method.md) (commit `48b6942`; la adenda §7 del contrafáctico se confirmó junto con su código en `92a96bc`, antes de ejecutarlo). Experimento: `Tests/Experiments/EditMode/UnreachableDiagnosisExperiment.cs`. Datos derivados y SHA-256: `docs/validation/evidence/2026-11-15-unreachable-diagnosis/`.

## 1. Resultado en una frase

**No hay defecto crítico.** Los 180 `Unreachable` nuevos del Rival son **un solo tipo de golpe**: el Bot (σ=30 ms, intención fija «paralelo, altura media, potencia media») pega a menos de 1,5 m del frontis (mediana 0,9 m), y la curva convierte su P=0,5 en una velocidad de 11,5 m/s en lugar de 17 m/s. La pelota muere a unos 4 m, el Rival espera en z=5,97 m y llega 0,2–0,4 s tarde. Con **el mismo golpe y velocidad lineal, 178 de esos 180 son alcanzables** (aunque «estirados», holgura 0). Es el efecto de la curva, no un fallo de cálculo; si es demasiado letal o no es una decisión de sensación para el iPhone.

## 2. Qué es `Unreachable` y si se reprodujo

Una recepción termina en `Unreachable` si el planificador no halla posición ni contacto legal; el punto cierra por doble bote. El «36 → 215» del informe de rallies es `DoubleBounce` con `LastReception = Unreachable`. Se reprodujo **exactamente** (flujo A, mismas semillas que `RallySimTests`): γ=1,0 → Air 593 / Late 312 / Unreachable 36 / Out 59, ganador 330/670, 19,367 golpes; γ=2,0 → 406 / 330 / 215 / 49, 356/644, 17,975. Aparte, el Humano tiene 92 (γ=1) y 84 (γ=2) *recepciones* `Unreachable`, pero 59 y 49 de ellas son pelotas que se van `Out` (cierra por Out, no por doble bote): no cuentan en 215.

## 3. Resultados (flujo A: 1.000 rallies, γ=1,0 frente a γ=2,0)

| Final `DoubleBounce|Unreachable` | γ=1,0 | γ=2,0 |
|---|---:|---:|
| Receptor Rival (golpeó el Bot) | **3** | **180** |
| Receptor Humano/Bot (golpeó el Rival) | 33 | 35 |
| Total | 36 | 215 |

Causas de fin de punto × receptor (DoubleBounce tras Air / Late / Unreachable): γ=1,0: Humano 367 / 270 / 33, Rival 226 / 42 / 3 (cada cifra incluye los pocos finales sin golpeador, del lanzamiento neutro); γ=2,0: Humano 320 / 289 / 35, Rival 86 / 41 / 180. `Out`: 59 → 49. Tablas completas en `tables.tsv`. El Rival pierde `Air` (226 → 86) porque los golpes frenados del Bot ya no le llegan por el aire; ese cambio es el mismo fenómeno.

## 4. Clasificación de los 215 (cubetas predeclaradas, §5 del método)

| Receptor | Clase | γ=1,0 | γ=2,0 |
|---|---|---:|---:|
| Humano/Bot (golpea el Rival) | C3 sin efecto de la curva | 33 | 35 |
| Rival (golpea el Bot) | C1 golpe corto suave (P ≤ 0,25, z2 < 3) | 0 | **0** |
| Rival (golpea el Bot) | C2 frenado por la curva, no suave | 0 | **180** |
| Rival (golpea el Bot) | C3 | 3 | 0 |

- **C1, el efecto «buscado» (drop suave desde adelante), no explica ninguno**: los 69 golpes C1 del Rival (γ=2,0) no produjeron un solo `Unreachable` del Bot (0/69; 0/64 con γ=1,0).
- Los 180 nuevos: Bot como golpeador, `Straight|Mid|Medium`, P=0,5, contacto z p50 0,88 m (p10–p90 0,70–1,20), D p50 0,93 m (el 100 % con D < 3 m), mira y p50 0,68 m (coherente con el recorte de `ShotOptions` por `MaxAimElevationDeg` = 22° cerca del frontis), cociente curva/lineal 0,676 (curva 11,5 m/s frente a 17,0 m/s lineal; P efectiva (v − vMin)/(vMax − vMin) = 0,25, justo el borde de «suave»). Primer bote z p50 2,57 m y t 0,53 s; segundo bote z p50 3,97 m (todos en la banda media, 3–6 m) y t 0,87 s. Con la misma P y γ=1,0 caen a z2 ≈ 7,0 m.
- **Tasa**: de 461 golpes del Bot con D < 3 m, 180 (39 %) acaban en `Unreachable`; por D: [0, 1) 120/139 (86 %), [1, 1,5) 54/131 (41 %), [1,5, 2) 6/47, [2, 3) 0/144. Con D ≥ 3 m (8.430 golpes) cero. Con γ=1,0, 3/515. Los golpes con D < 3 m son el 5,2 % de los golpes del Bot.
- **Receptor Rival**: siempre parado en su espera (z = 5,965 m, velocidad 0), «detrás» del punto de muerte; hasta el segundo bote queda a 0,9–1,6 m de ese punto (<2 m en los 180), pero necesita recorrer 2,6–3,2 m hasta la posición objetivo. Retraso del mejor candidato (llegada − pasos hasta la pelota): 0,22–0,38 s (p10/p50/p90); 106 con 0,1–0,3 s y 74 con ≥ 0,3 s. Tiempo disponible 0,77–0,92 s frente a 1,06–1,19 s requeridos. Cuadrante: 112 detrás-derecha y 68 detrás-izquierda. El contacto y el primer bote quedan pegados a una pared lateral (la intención «paralela»): 112 con x < 1,5 m y 68 con x > 4,9 m.
- **Receptor Humano/Bot** (35): golpes fuertes del Rival (P > 0,65) al fondo (z2 > 8,6 m, 100 % «fondo»), 20 con D ≥ 6 m y 15 con D medio; cociente ≈ 1 (curva sin efecto): es el mismo fenómeno que con γ=1,0 (33), no cambia.
- **Rutas del planificador**: 208 de 215 «ninguna posición llega a tiempo» (ruta i), 7 «sin candidatos»; **0 de la ruta ii** (candidatos alcanzables pero el contacto falló). La mejor holgura negativa mínima es 20 pasos (0,167 s): según los números del propio planificador, todo `Unreachable` llega tarde con margen.

## 5. Contrafáctico del mismo golpe (adenda §7; dos corridas idénticas)

Para cada final `Unreachable` del flujo A con γ=2,0 se rehizo **el mismo golpe** (contacto, mira, P, sin capa causal) con γ=1,0 y con γ=2,0 contra el mismo receptor en reposo:

| Receptor | Golpe real sin vectores causales | γ=2,0 inalcanzable → γ=1,0 alcanzable | γ=2,0 inalcanzable → γ=1,0 inalcanzable |
|---|---:|---:|---:|
| Rival | 158 de 180 | 156 | 2 |
| Rival | 22 con vectores causales | 22 | 0 |
| Humano/Bot | 24 sin vectores | 1 | 23 |
| Humano/Bot | 11 con vectores | 2 (+1 alcanzable ya con γ=2,0) | 8 |

Es decir **178 de 180 de los del Rival son efecto de la curva sobre ese mismo golpe**; los del Humano/Bot (3 de 35) no lo son. «Alcanzable» con γ=1,0 es en 178 de 178 casos *estirado* (`stretched`, holgura 0): el Rival llega a tocarla justo, no cómodo; su segundo bote sería z ≈ 7,0 m (más profundo, no mueren a 4 m). La reproducción con γ=2,0 coincide con el registro en 214 de 215 (la excepción es un caso del Humano/Bot con vectores causales).

## 6. Flujo B (semillas independientes por rally, pareado por índice)

`Unreachable` DoubleBounce: γ=1,0 → 60 (todos receptor Humano); γ=2,0 → 199 (Rival 142, Humano 57); duración media 19,50 → 17,01 golpes; ganador 263/737 → 330/670. Misma estructura (Rival ← Bot a corta distancia). El emparejamiento rally a rally **solo sirve para agregados**: en 950 de 1.000 rallies los dos flujos divergen y la mediana del índice de divergencia es 1 (el primer golpe del Bot ya cambia); 192 de los 199 `Unreachable` ocurren tras la divergencia. Por eso el contrafáctico de golpe único (§5) es la comparación válida.

## 7. Las cinco comprobaciones de defecto crítico

| | ¿Defecto? | Evidencia |
|---|---|---|
| a. Pronóstico de la IA ≠ trayectoria real | **No** | 13.172 golpes sin vectores causales (A, γ=2,0): `Input.Speed` − curva = 0 y |v_intención − v_real| = 0 (máx. 0; 0 por encima de 1e-3, también γ=1,0 y flujo B); error del primer bote del pronóstico de la IA (opción limpia en P*) p50/p95/máx. 0,553/1,753/4,319 m (γ=1,0) frente a 0,563/1,806/4,748 m (γ=2,0): sin empeoramiento atribuible a la curva (diferencia por ruido de mira/potencia y vectores causales). Con la entrada idéntica, 0 diferencias > 1e-3 m sin vectores. |
| b. Chequeo de alcance inconsistente | **No** | Espejo del planificador: 0 de 215 en la ruta ii; sondeo independiente (punto más cercano dentro del radio de alcance para cada paso de la malla): 0 de 215 con alcance a tiempo y 0 contradicciones candidatas; holgura negativa mínima 20 pasos. Nota: el planificador solo prueba dos posiciones por muestra, pero el sondeo más permisivo tampoco halla ninguna. |
| c. Velocidades/legalidad | **No** | 0 valores no finitos y 0 fuera de [vMin, vMax] (curva) o sobre vCap (real) en 19.367 + 17.975 + 19.504 + 17.014 golpes; los 215/199 finales son DoubleBounce, superficie terminal suelo, golpeador = oponente del receptor. Los «Unreachable» del Humano que son `Out` (59 y 49) cierran por Out, no se cuentan como `Unreachable`. |
| d. Saque y doble aplicación | **No** | Los dos únicos llamadores de `ShotPace.Requested` son `CausalQuality.Solve` y `RallyContact.SolveIntention` (que `Forecast` reutiliza); todos los `ShotSolver.Solve` del producto pasan por ellos con extremos iguales (la curva no se reaplica); `ServePlanner` usa velocidades fijas y no llama a la curva; `ShotController`/`RallyDirector` usan `CausalQuality.Solve`/`RallyContact.Solve`. |
| e. Elección degenerada de la IA | **No** | Cuota de las 18 opciones del Rival sin cambio relevante (máximo ±0,6 puntos entre γ=1,0 y γ=2,0, p. ej. Cross|High|Medium 15,1 → 15,4 %; Straight|High|Medium 11,8 → 11,3 %); los `Unreachable` nuevos no vienen de la IA sino de la intención fija del Bot; `FondoAi.Utility` no modela el alcance del oponente (no «sabe» nada). |

## 8. Hechos frente a hipótesis

**Hechos** (medidos): lo de §2–§7. **Hipótesis, no demostradas**: (1) lo «letal» o no de ese golpe para un jugador real depende de la anticipación del rival, que en esta simulación es una espera quieta en z = 5,97 m y un retardo de reacción de 0,18 s + 0,04 s; (2) el Bot es un sustituto escrito del humano con potencia fija 0,5, no un jugador: un humano elige su P y los golpes desde el frontis no son un caso frecuente (5,2 % de los golpes del Bot); (3) que el efecto sea «el buscado» es una interpretación del diseño (golpe suave cerca del frontis muere pronto): el golpe sí cae pronto (z2 ≈ 4 m) pero no es un drop (P=0,5, no suave); los drops reales del Rival (C1) no producen ningún `Unreachable`.

## 9. Qué mirar en el iPhone

1. Pegar **desde menos de 1,5 m del frontis** con un gesto de potencia media y ver si la pelota se siente demasiado lenta y muere a media pista; el golpe se frena a ≈ 11,5 m/s en lugar de ≈ 17 m/s y el primer bote queda a ≈ 2,5 m.
2. Si el rival, parado en su posición de espera, deja pasar esos golpes sin intentar adelantarse (sensación de «el rival no se mueve»).
3. Que los golpes desde el fondo y los fuertes no cambian (los 35 `Unreachable` del Humano son iguales a los de antes).
4. Un golpe suave a propósito desde adelante (drop): cae cerca del frontis y el rival lo alcanza, como antes.

Sin iPhone, build, instalación, `check-release` ni cambios de producto en este diagnóstico. La decisión de preparar la build DEV es de Claude y del PO. PR #56 sin fusionar.
