# Método predeclarado — potencia según la distancia (SL5/SL6 y Anexo C)

Fecha: 14 de noviembre de 2026. Operador: subagente Sonnet dirigido por Claude. Este documento se confirma en Git **antes** de ejecutar el experimento. Fuente de verdad: [diseño aprobado](../reviews/2026-11-14-first-shot-shot-pace-distance-design.md) (secciones 5, 7 y 9) y decisión literal del PO (sección 11). Nada de lo que sigue modifica la spec, los escenarios, los umbrales o los criterios del diseño; solo concreta su operacionalización.

## 1. Alcance y conjuntos de parámetros

Un único experimento explícito (`ShotPaceDistanceExperiment`, `[Explicit]`, categoría `Experiments`, fuera de las suites de regresión y de AutoQA) que evalúa **en la misma corrida** las variantes siguientes con `shot.paceNearM=1,5` y `shot.paceFullM=6,0` fijos:

| Variante | `shot.paceGammaMax` | Papel |
|---|---|---|
| `gamma1.0` | 1,0 | **Referencia de comparación, no elegible.** Es la fórmula de hoy (retorno temprano) |
| `gamma2.0` | 2,0 | Principal predeclarado |
| `gamma2.5` | 2,5 | Respaldo predeclarado |

No se evalúa ningún otro valor. Los experimentos llaman a la función compartida `ShotPace.Requested` y luego al solver con extremos iguales, igual que `CausalQuality.Solve` y `RallyContact.SolveIntention`. La física P2, el contrato, las ventanas y M2-4 no se tocan. Las pruebas se hacen sobre P2 real (`AnalyticBallSimulationFactory`).

## 2. Muestra SL5/SL6 (criterios S1–S5)

- **Contactos:** para cada receta `SL5` y `SL6` de `PracticeLaunchDefinitions`, trayectoria de P2 real, plan de `ContactPlanner` con la geometría corporal de la receta. Se toman los pasos `T*−12`, `T*` y `T*+12` (el borde de la ventana Valid de 100 ms a 120 Hz). Un paso que no sea legal según `ContactPlanner.IsLegalStep` se informa como `NotEvaluated` con su motivo; no se sustituye. El centro de la pelota es la posición de la trayectoria en ese paso.
- **Miras:** x ∈ {1,2; 2,6; 4,0} m. Alturas: drop `tin+0,05`, `tin+0,10`, `tin+0,15`; media-baja 0,8, 1,2 y 1,6 m; banda media 1,8, 2,2 y 2,55 m (1,6 ≤ y < 2,6).
- **Potencias:** P = k/20, k = 0…20.
- **Resultado de cada tiro:** el solver real y el verificador P2 de diez segundos, con las mismas reglas de punto que el arnés del Anexo C (`PointRules`, `ReturnLineJudge`).

| # | Criterio | Operacionalización |
|---|---|---|
| S1 | Potencia media no manda al fondo | P ∈ {0,40; 0,45; 0,50; 0,55; 0,60}, todas las miras (drop, media-baja y banda media). **Denominador primario: tiros resueltos y legales** (`solved` y `legal`). Pasa el tiro con segundo bote a z ≤ 6,5 m y sin pared trasera antes del primer bote. Umbral ≥ 90 %. Se informa también la fracción sobre todos los tiros resueltos y sobre todos los tiros de la muestra |
| S2 | Golpes cortos posibles | P ∈ {0; 0,05; 0,10; 0,15; 0,20; 0,25}, solo miras de drop. Denominador primario: resueltos y legales. Pasa z2 ≤ 2,0 m. Umbral ≥ 90 %. Se informa también sobre resueltos y sobre todos |
| S3a | El fuerte conserva su velocidad pedida | P = 1: `requestedSpeed` bit a bit igual a `shot.vMax`. Umbral 100 % |
| S3b | El fuerte sigue fuerte | P = 1, solo miras de drop. Denominador: tiros resueltos. Pasa el tiro con pared trasera antes del primer bote, o con segundo bote observado a z ≥ 8 m. **Umbral ≥ 90 %: lo fijó el operador por analogía con S1 y S2, porque el diseño no da porcentaje.** Se informa la referencia `gamma1.0` y la identidad de resultados de P = 1 frente a ella |
| S4 | Monotonía | Fracción de pares de potencias adyacentes (misma muestra, ambos resueltos, sin pared lateral ni trasera antes del primer bote, sin ajuste del solver) con primer bote creciente. Se informa como tendencia; no entra en la regla de adopción |
| S5 | Sin cambios lejos del frontis | En el arnés del Anexo C, los tiros con contacto B0–B3 (`D ≥ 6 m`) tienen velocidad pedida bit a bit igual a la de hoy y salida idéntica a `gamma1.0`. Umbral 100 % |
| S6 | Juicio del PO en el playtest | No se evalúa aquí |

## 3. Arnés del Anexo C

Se reutiliza el arnés `SportScenarioExperiment` (misma rejilla de contactos B0–B3, F1 y F2, x de mira, alturas, P = k/20, mismo juez de línea y de punto, mismas relaciones y mismos emparejamientos), con el solo cambio de pasar la velocidad por `ShotPace.Requested`. Salidas: X1–X5 con testigos, R1, R2, R3a, R3b y los emparejamientos de tempo de R7; las métricas C1–C4 se informan descriptivamente a partir de los testigos. Queda fuera del alcance actual, como en #51: R4/C5 (capa E3), X6/X7, R5, R8, R6 y la capa 4 del playtest.

- **R1 y R2:** igual que en #51 (tendencias; se informan recuentos y contraejemplos, sin porcentaje de aceptación nuevo).
- **R3a:** emparejamientos F1/B1 y F2/B2 con P ≤ 0,30 en las mismas condiciones de #51. Tendencia ≥ 75 % de los **pares válidos con P > 0** (`z2(B) > z2(F)`). Los pares con P = 0 se informan por separado, con su comportamiento y su causa (velocidad mínima y elevación del solver). Se informa también la fracción sobre todos los pares, incluidos los de P = 0. Nunca se cuentan como Pass.
- **R3b:** `t2(B) > t2(F)` en el 100 % de los pares válidos, medido desde el contacto hasta el segundo bote. Es un **indicador físico, no una demostración de letalidad**.

## 4. Regla de adopción y parada

Se adopta `gamma2.0` si cumple S1, S2, S3a, S3b, R3a y R3b. Si no, se adopta `gamma2.5` si cumple esos seis. Si ninguno cumple, **se detiene, se presenta el diagnóstico y se vuelve a revisar el diseño**: no se prueban otros valores, ni se cambian umbrales, ni se ajustan parámetros después de ver los datos. `gamma1.0` nunca se adopta.

## 5. Reproducibilidad

Dos corridas en procesos Unity separados (`SC_PACE_RUN=run01` y `run02`), con un solo Unity a la vez. Las salidas (`trials.tsv`, `pairs.tsv`, `summary.json`, `witnesses.json`, `sl56-trials.tsv`, `sl56-summary.json` por variante) deben ser idénticas byte a byte (SHA-256). Los datos viven en `Logs/experiments/pace/` (ignorado) y un derivado sanitizado se versiona. Se reportan todos los casos válidos y todos los no evaluados, con su motivo.
