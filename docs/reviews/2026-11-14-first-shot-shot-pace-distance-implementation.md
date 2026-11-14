# First Shot — implementación de la potencia según la distancia: detenida por la regla de adopción

Informe del 14 de noviembre de 2026. Operador: **subagente Sonnet dirigido por Claude** (autorización literal del PO registrada en AGENTS.md y en el [registro de aprobaciones](2026-10-25-first-shot-po-approvals.md)). **La revisión posterior de Claude no es independiente de la dirección.** Ejecuta el encargo de la sección 9 del [diseño aprobado](2026-11-14-first-shot-shot-pace-distance-design.md). Rama `firstshot/shot-pace-distance` desde main `7fdef02ec1682bcae5b3229b6b1a373557bdba25`.

## 1. Resultado en una línea

**La regla de adopción predeclarada falla para γ_max = 2,0 y para γ_max = 2,5 en R3a (24 de 36 pares válidos con P > 0, 66,7 %, frente al umbral de 75 %), y también en S3b.** Conforme al diseño, **se detiene aquí**: no se probaron otros valores, no se tocaron umbrales ni parámetros, y se necesita volver a revisar el diseño antes de seguir. La implementación (código, claves, registro, pruebas, hashes) queda completa y verificada, **pero no se debe fusionar ni dar por adoptada**: el valor por defecto `shot.paceGammaMax = 2,0` de la rama es el del diseño, no un resultado adoptado.

## 2. Commits y su función

| Commit | Función |
|---|---|
| `16849e0` | Solo documentación: autorización del PO y cierres de #54 y #55, verificados con Git |
| `7becfec` | **RED**: esqueleto compilable de `ShotPace`, tres claves `shot.pace*`, cuatro campos del registro; 5 de 17 pruebas fallan por aserción |
| `4f5727b` | **GREEN**: curva compartida; `CausalQuality.Solve`, `RallyContact.SolveIntention` (pronóstico de la IA) y `ShotController` (registro) |
| `7bb9e1f` | Dos pruebas existentes pasan a esperar la velocidad de la curva en lugar de la lineal (misma intención) |
| `4bf9527` | **Hashes y anclajes medidos en Unity**, en commit propio |
| `1296f60` | El lector de registro exacto de AutoQA predeclara los cuatro campos nuevos y deterministas |
| `0228f1b` | **Método predeclarado**, antes de ejecutar nada |
| `cdb3b1d` | Experimento explícito (`ShotPaceDistanceExperiment`) y refactor de `SportScenarioExperiment` para pasar por la curva |

## 3. Implementación

- `ShotPace.Requested(P, contacto, mira, radio, ajustes)`: `D` es la distancia 3D del centro de la pelota al punto `(aim.x, aim.y, radio)`; `w = clamp((D_full − D)/(D_full − D_near), 0, 1)`; `γ = 1 + (γ_max − 1)·w`; `v = vMin + P^γ·(vMax − vMin)`. Con `γ = 1` (lejos o `γ_max = 1`) devuelve **literalmente** `vMin + P·(vMax − vMin)` por retorno temprano.
- Orden: curva → composición causal → `BoundToLegalFront` → solver con extremos iguales. La distancia usa la mira pedida **antes** de la capa causal.
- Ruta única: humano (`CausalQuality.Solve`), rival (`RallyContact.Solve`) y pronóstico de la IA (`RallyContact.SolveIntention`, usado por `FondoAi` y `RallySim`). `ShotRecord` y `CausalShotTrace` guardan `paceDistanceM`, `paceGamma`, `requestedSpeedLinear` y `requestedSpeed`.
- Claves `shot.paceGammaMax` 2,0 (1–3), `shot.paceNearM` 1,5 (0,5–4,0), `shot.paceFullM` 6,0 (2,0–9,75), en catálogo y `DefaultPreset.json`; el validador exige `paceNearM < paceFullM`.
- No se tocó P2, M2-4, el contrato, las ventanas, el solver ni la física.

### Desviación del encargo: el saque

El diseño (5.3 y 7.3) supone que `ServePlanner` usa `SolveIntention`. **No es así**: el saque automático se resuelve con `Ballistics.SolveCenterTo` y una rejilla de velocidades fijas, sin potencia P. Por tanto **no pasa por la curva y no cambia**. No se tocó, porque cambiarlo sería una mecánica nueva. X6/C7 (saque) siguen fuera del alcance actual, como en #51.

## 4. TDD

- **RED** (`7becfec`, `Logs/evidence/pace/red-shotpace.xml`, local): 17 pruebas, 12 pasan y 5 fallan por aserción, sin errores de compilación. Las 5 son curva (distancia, γ), enrutado y valores de la traza. Las pruebas de identidad lejos del frontis, extremos y monotonía pasaban ya con el esqueleto lineal por construcción; es esperable.
- **GREEN** (`4f5727b`): 17/17. Las pruebas cubren: extremos P = 0 y 1; monotonía en P y en D; continuidad en `D_near` y `D_full`; identidad bit a bit lejos del frontis y con `γ_max = 1`; determinismo; distancia 3D; claves y rangos; validador; ruta compartida humano/rival/pronóstico; orden curva → causal → acotamiento → solver; distancia calculada con la mira pedida; campos nuevos en traza y registro.
- El diseño pide además la comparación de «misma función para el saque»: no aplica (sección 3).

## 5. Hashes medidos en Unity (`4bf9527`)

Medidos con las pruebas existentes de hash (nunca calculados a mano ni copiados):

| Dato | Valor |
|---|---|
| Defaults | `06fed4bd4ac9`, 121 claves (antes `9041c4aafc77`, 118) |
| P0 efectivo | `0a3d3262a79d` (antes `b6914a349068`) |
| Anclaje AutoQA SL1 | `06fed4bd4ac9` |
| Anclaje AutoQA SL5 | `8e22575422f2` (antes `e2ffa032a588`) |
| SimulationKey | `166a96e319c7`, **sin cambio** |
| Proyección heredada | `0d39e6016bd8`, **sin cambio** (P0 heredado `8de3f2ecb217`, sin cambio) |

Actualizados también `scripts/tests/test_autoqa_checker.py` (6/6 verdes) y el bloque `EXPECTED_HASH` de `.claude/agents/autoqa-operator.md` (el verificador sigue siendo `v7`). Los hashes de los informes históricos no se reescriben.

## 6. Experimento (método en `0228f1b`, fuente `cdb3b1d`)

Dos procesos Unity separados (`run01`, `run02`), un solo Unity a la vez; **los 19 archivos de cada corrida son idénticos byte a byte** (SHA-256 en `sha256-run01-run02.txt`). Tres variantes en la misma corrida: `gamma1.0` (referencia, **no elegible**), `gamma2.0` y `gamma2.5`. 38.304 tiros del arnés del Anexo C por variante y 3.402 de la muestra SL5/SL6 (6 contactos en `T*` y `T*±12` pasos, todos legales: cero casos no evaluados en la muestra SL5/SL6). Sin tope de impactos ni fugas de simulación. **Sanidad:** la referencia `gamma1.0` reproduce los números de #51 (R1 2110/2110, R2 3173/3463, R3 0/42).

### 6.1 SL5/SL6 (denominador primario: tiros resueltos y legales)

| Criterio | gamma1.0 (ref.) | gamma2.0 | gamma2.5 |
|---|---|---|---|
| S1 (≥ 90 %): P ∈ [0,40; 0,60], z2 ≤ 6,5 y sin trasera al vuelo | 482/609 (79,1 %) | **710/751 (94,5 %)** | **769/782 (98,3 %)** |
| …sobre todos los resueltos | 59,5 % | 87,7 % | 94,9 % |
| S2 (≥ 90 %): P ≤ 0,25, drops, z2 ≤ 2,0 | 212/324 (65,4 %) | **323/324 (99,7 %)** | **324/324 (100 %)** |
| S3a: P = 1, velocidad pedida = `vMax` bit a bit | 162/162 | 162/162 | 162/162 |
| S3b (≥ 90 %, umbral fijado por el operador): P = 1, drops, trasera o z2 ≥ 8 | 23/54 (42,6 %) | **23/54 (42,6 %) Fail** | **23/54 (42,6 %) Fail** |
| S4 (tendencia, fuera de la regla): primer bote creciente en potencias adyacentes | 728/728 | 1081/1081 | 1196/1196 |
| S5: tiros con contacto B0–B3 (D ≥ 6 m), 25.536 filas | idénticas | **idénticas a la referencia** | **idénticas a la referencia** |

- **S1 y S2 mejoran mucho con la curva** (de 79 % a 94,5 % y de 65 % a 99,7 % con γ_max = 2,0). Es el efecto buscado para «desde adelante».
- **S3b falla igual en la referencia lineal (42,6 %)**: P = 1 no cambia de velocidad, así que no depende de la curva. El diseño no daba porcentaje para S3b; el 90 % lo fijó el operador por analogía con S1 y S2, y la muestra incluye miras laterales (x = 1,2 y 4,0). 31 de los 54 tiros no cumplen (segundo bote por debajo de 8 m sin tocar la trasera antes); no se analizó la causa de cada uno. **Hay que revisar esa operacionalización** (y también por qué se pedía que un golpe fuerte desde 1,2 m al drop vaya al fondo con mira lateral). No cambia la conclusión, porque R3a falla de forma independiente.

### 6.2 R3 (Anexo C, arnés y emparejamiento sin cambios)

| | gamma1.0 (ref.) | gamma2.0 | gamma2.5 |
|---|---|---|---|
| **R3a** pares válidos con P > 0, z2(B) > z2(F) | 0/36 | **24/36 (66,7 %): Fail (< 75 %)** | **24/36 (66,7 %): Fail** |
| R3 original, todos los pares (incluido P = 0) | 0/42 | 24/42 (57,1 %) | 24/42 (57,1 %) |
| Pares P = 0 (aparte) | 0/6 | 0/6 | 0/6 |
| **R3b** t2(B) > t2(F), pares válidos | 42/42 | **42/42 Pass** | **42/42 Pass** |

**Diagnóstico de los 18 contraejemplos con γ_max = 2,0** (12 de R3a y 6 de P = 0; los de 2,5 son los mismos pares):
- Todos son **P ≤ 0,10** con mira de drop, en F1 y F2 y en las tres alturas. Con P ≥ 0,15 el desde-atrás supera al desde-adelante en los 24 pares.
- La causa es del lado trasero, no de la curva: desde 7,5 m del frontis el solver **eleva la velocidad al mínimo factible** (`SpeedRaised`, 8,44–8,50 m/s) cuando el gesto pide 6,0–6,2 m/s. Con esa velocidad el drop desde atrás llega al frontis y su segundo bote queda a 0,86–0,91 m, **antes** que el desde adelante a P = 0 (1,56–1,74 m, que no puede bajar de `vMin` = 6 m/s). La curva afecta solo al lado de adelante (D ≈ 1,5 m) y por eso γ_max 2,0 y 2,5 dan el mismo recuento: no puede acortar más el de adelante que el suelo de velocidad que impone `vMin`.
- A P = 0 (6 pares) el comportamiento es el mismo y se reporta aparte, tal como pidió el PO: 0 de 6.
- El modelo aproximado del diseño predecía «12 de 12 pares con P > 0»; con P2 real son 24 de 36. La diferencia son los pares de P = 0,05 y 0,10, que el modelo no contemplaba. Solo es una explicación probable basada en estos datos; el diseño debe revisarlo.
- **R3b se cumple en el 100 %, incluso en la referencia**: el tiempo hasta el segundo bote desde atrás ya era mayor en todos los pares. Es un indicador físico, no una demostración de letalidad.

### 6.3 Otros resultados del Anexo C

Todos los escenarios existen en los tres juegos (X1 B0–B3; X2 B0–B3; X3 F1 y F2; X4 B0–B3; X5 los seis contactos), con los testigos X1/X2/X4 sin cambio (mismos identificadores que en la referencia); X3 cambia de testigo porque F1/F2 caen en la zona de la curva (2933→3494 y 3034→3566 coincidencias con la curva 2,0). R1 es 2224/2224 con γ_max = 2,0 (referencia 2110/2110) y 2252/2252 con 2,5; R2 es 3149/3478 y 3109/3447, con 329 y 338 contraejemplos (referencia 290). Tendencias sin umbral nuevo. R7: `Shorter` en 6/6 recetas, igual en las tres variantes. X6, X7, R4, C5, R5, R6, R8 y C6/C7 siguen fuera del alcance actual. Las bandas C1–C4 son informativas; los testigos completos están en `witnesses.json` de cada variante.

### 6.4 Regla de adopción

γ_max = 2,0: cumple S1, S2 y S3a, falla S3b y R3a. γ_max = 2,5: igual. **Resultado: ninguna → STOP.** No se probó ningún otro valor.

## 7. Simulación de 1.000 rallies

Regenerada con la curva activa (`Logs/rally-sim/…145429Z…`, `rally-sim/report.md`): 1.000 rallies, todos terminan en punto, cero fallas del sistema, cero contactos rechazados, cero eventos tras el cierre.

| | Antes (#53, `9041c4aafc77`) | Ahora (`06fed4bd4ac9`) |
|---|---:|---:|
| Fin del punto | 1000 Point | 1000 Point |
| DoubleBounce tras Air / Late / Unreachable | 593 / 312 / 36 | 406 / 330 / 215 |
| Out (trasera del rival) | 59 | 49 |
| Ganador Humano / Rival | 330 / 670 | 356 / 644 |
| Duración media | 19,37 golpes, 40,77 s | 17,98 golpes, 37,86 s |
| Mediana / p90 / p99 / máx. (golpes) | 13 / 45 / 87 / 165 | 13 / 39 / 87 / 140 |

Hipótesis, **no demostrada**: los golpes de potencia media cerca del frontis (del bot y de la IA) son más suaves y caen más cerca, lo que reduce las pelotas «al aire» y aumenta las que «no llega» (recepciones del rival que no llega: 3 → 180). No se ajustó nada (pesos, cinemática ni `RallyParams` `7d50ddaf5d4f`).

## 8. Verificación

| Prueba | Resultado |
|---|---|
| EditMode completa, XML fresco | **732/732**, sin errores de compilación (en `1296f60` y de nuevo en `cdb3b1d`) |
| PlayMode completa, XML fresco | **457/457** en un Mac, sobre `1296f60`; los commits posteriores solo tocan `Tests/Experiments` y documentación (no se vuelve a ejecutar PlayMode) |
| AutoQA `scripts/autoqa.sh mvp` | **PASS, salida 0, nivel 1**, 85/85 pruebas, 18 PASS y el canario en FAIL esperado, `dirty=false`. Run `20261114T145004Z-cdb3b1d-18283` |
| `scripts/check-release.sh` dev y prod | **OK**, ambos `gitHash=cdb3b1d` y `gitDirty=false`; dev incluye `DevPanel`, prod no; el framework de pruebas no entra en ningún Player |
| ProjectSettings transitorios | Restaurados tras PlayMode |
| `scripts/tests/test_autoqa_checker.py` | 6/6 (1 omitida) |

AutoQA es obligatorio porque el cambio toca el golpe; pasó con los anclajes medidos. El lector `PocObservation` necesitó predeclarar los cuatro campos nuevos del registro (si no, `ExactRecord` rechaza campos desconocidos), igual que se hizo en #50; son deterministas y se comparan entre corridas.

## 9. Desviaciones y observaciones

1. **STOP por la regla de adopción** (sección 6.4). Se continuó solo con las verificaciones de estado del código (AutoQA, check-release, suites) y con la segunda corrida del experimento, para dejar la evidencia completa y reproducible. No se reintentó nada para conseguir un Pass.
2. **Saque:** no usa la curva (sección 3).
3. **Pruebas existentes cambiadas con la misma intención:** `FondoAiTests.Solve_UsesTheSharedSolverAndSpeedLimits` y `CausalQualityTests.CausalHeightWithinLegalBound_IsNotClamped…` esperaban la velocidad lineal; ahora esperan la velocidad de la curva (commit `7bb9e1f`). Cuentas de claves, hashes y la lista de claves añadidas de `S3FeedbackConfigTests` también cambian (`4bf9527`).
4. **`SportScenarioExperiment` refactorizado** (`Harness`, visibilidad interna, columnas de curva y de tiempos, R3a/R3b/P0); su comprobación de configuración protegida estaba obsoleta (`c0e9f00176d5`/108) y se actualizó a la actual. El comportamiento del arnés es el de #51 con la curva.
5. **S3b:** el umbral del 90 % lo fijó el operador (sección 6.1). Hay que revisar si esa regla de aceptación del diseño es la que se quería.
6. **Valor por defecto activo:** la rama deja `shot.paceGammaMax = 2,0`, el valor del diseño. Como la regla de adopción falló, no se ha aceptado y no debería fusionarse sin decidir; poner 1,0 devolvería exactamente la fórmula de hoy (retorno temprano, bit a bit).
7. **Directorio `Library` copiado** del worktree de B4/H3b para evitar una importación larga; no se versiona.
8. Sin iPhone, build nativa ni instalación. Sin cambios en P2, M2-4, contrato, ventanas, #32 ni First Rally.

## 10. Qué queda para el playtest y para decidir

- **S6** y **R3d** (juicio del PO en el playtest integrado) siguen pendientes.
- Decisión de diseño antes de seguir: cómo tratar los pares de P ≤ 0,10 de R3a (el suelo del solver desde atrás y `vMin` desde adelante); si S3b y su umbral son la regla correcta. Opciones fuera del alcance de este encargo: otro piso para el desde-atrás, otro `vMin`, o revisar R3a con la ayuda del PO.
- Datos derivados sanitizados: `docs/validation/evidence/2026-11-14-shot-pace-distance/` (informe, resúmenes por variante, testigos, SHA-256 de ambas corridas, AutoQA y simulación de rallies). Los `trials.tsv` y `pairs.tsv` completos (≈ 70 MB) quedan en `Logs/experiments/pace/` (ignorado), con sus SHA-256 versionados.
