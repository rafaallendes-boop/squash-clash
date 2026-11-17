# F2 — método predeclarado del experimento B con todo activo

17 nov 2026. Fuente: spec de cansancio v2.3 (§§4, 6, 7 y 8) y el informe de F1. Este documento se confirma en un commit propio antes de producir datos. F2 se implementa por subagentes dirigidos por Claude; la revisión de Claude no es independiente.

## Qué se mide

El brazo (vector causal «fatigue» y amplificación de los vectores de posición, con la instantánea `f_s` del golpeador) y la barra de energía. Los criterios B1–B6 de la spec §7 se repiten con todo activo, con los umbrales de la spec y sin cambios. La mecánica, la muestra y las definiciones de B1–B6 son las del [método de F1](2026-11-17-f1-fatigue-method.md); aquí solo se fijan las variantes y la regla de parada.

## Variantes y muestra

Experimento EditMode explícito, separado de la regresión (`FatigueMatchExperiment`, variables `SC_EXPERIMENT` = `f2`, `SC_F1_VARIANT` = variante). 200 partidos completos por variante, a 7 con diferencia de 2, sin tope de marcador. Bot humano: `BotBrain`, σ de timing 30 ms, paralelo/medio/potencia media; Rival: `FondoAi` Media. `RallyParams` sin cambios (`927e304bbfec`). Semillas de F1: Bot `20261117 + 2*i`, Rival `20261118 + 2*i`, i=0…199; primer servidor humano para i par y rival para i impar; cerebros nuevos en cada partido.

| Variante | Configuración | Hash de defaults esperado |
|---|---|---|
| `f2off` | defaults con `fatigue.enabled = 0` | `3262f9b89156` (132 claves; ver el erratum al final) |
| `f2all` | defaults tal cual (`enabled = 1`, `recoverBetween` 0,25, `armSpeedMax` 0,04, `positionGainMax` 0,25, resto de F1) | `2ec0c953786a` (132 claves) |

Fijos en ambas: `SimulationKey` `166a96e319c7`, `RallyParams` `927e304bbfec`, `DefaultPreset.json` sin diff respecto de `main`. No se repite `baseline`: se usa la referencia congelada `docs/validation/evidence/2026-11-17-f1-baseline/` (hash 430da2faa818 afirmado en F1, 1.927 puntos). Se agrega al `receptions.tsv` la columna `f_s` (factor del golpeador por recepción golpeada), sin tocar el resto de las columnas.

## Criterios

- **A1 (identidad):** `f2off` es idéntico bit a bit a la referencia congelada en los 200 partidos y todos sus puntos (comparación de la firma por punto, como en F1). Un solo punto distinto es parada.
- **B1–B6 en `f2all`:** con los umbrales de la spec §7, sin modificar: B1 (mediana del gasto neto ≤ 4 por jugador en puntos de ≤ 4 golpes), B2 (mediana del mayor consumo por desplazamiento ≥ 8 y de la diferencia ≥ 4 en puntos de ≥ 15 golpes), B3 (mediana de E al empezar el último punto en [45, 75] y ≥ 50% de partidos bajo `onset`), B4 (≤ 5% de partidos con inicio de punto bajo 20 y cero saques con E = 0), B5a (mediana de E desde 6-6 ≥ 35), B6 (razón de duración media en [0,85; 1,15] contra la referencia con las mismas semillas).
- **B5b, exploratorio, no compuerta:** igual que en F1, se informa con muestra, denominadores, pares ausentes y diferencia en puntos porcentuales frente a la referencia congelada; sin umbral.

## Descriptivos sin umbral

Se informan sin juzgar: puntos de `f2all` cuyo resultado difiere de la misma semilla en F1 ajustado; número de vectores «fatigue» emitidos y `f_s` medio en las recepciones golpeadas; velocidad media de golpe contra F1 ajustado; diferencia de duración media contra F1 ajustado.

## Regla de parada: sin ajuste

El único ajuste que permite la spec §7 (`recoverBetween` 0,35 → 0,25) ya se usó en F1. En F2 **no hay ajuste**: si falla B3, B4 o B5a, el resultado se conserva, se reporta al PO y no se cambian valores, rangos, umbrales ni método. Cualquier otro B fallido (B1, B2, B6) se reporta igual y tampoco se ajusta. Un partido abortado, un resultado terminal distinto de punto, un `SystemFault` o un contacto rechazado se conserva y detiene el experimento; no se completa la muestra con otras semillas. Una población vacía se informa NotEvaluated, nunca Pass. Si el resultado es adverso, el informe lo dice y la decisión es del PO.

## Evidencia y límites

Cada invocación conserva inventario de procesos Unity, XML fresco, logs, head/árbol, versión de Unity, hashes y TSV con SHA-256. Un solo Editor Unity; no se inicia si ya hay alguno. Sin fusión, instalación ni acceso al iPhone. Autonomía experimental vigente (PO, F1/F2): solo código propio de `Tests/Experiments` y scripts de diagnóstico, resumen y empaquetado, conservando cada intento y verificando SHA-256 si ya hay datos; no cubre producto, método, grilla, umbrales ni datos producidos.

## Erratum previo a datos — F2-EXP-01 (17 nov 2026)

La tabla de variantes decía que el hash de defaults de `f2off` era `5cf3885e7c02`. Es un error de transcripción: ese valor es el de la variante `off` de F1, medida antes del único ajuste (`recoverBetween` 0,35 con `enabled = 0`). Con el default ajustado de F1 (0,25) y `enabled = 0` el hash medido en Unity es **`3262f9b89156`** (132 claves), el mismo que ya afirma `ZRallyHudTests` desde F1. La primera invocación de `f2off` se detuvo por esa aserción del propio arnés **antes de crear ningún dato** (intento conservado en `Logs/experiments/f2-fatigue/f2off-runner-attempt1`, sin `points.tsv`). Se corrige solo ese literal en el método y en el arnés. No cambia ningún valor de configuración, hash de producto, muestra, umbral ni regla: `f2all` sigue en `2ec0c953786a`, `SimulationKey` en `166a96e319c7` y `RallyParams` en `927e304bbfec`.
