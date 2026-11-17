# Evidencia G1 A1-bis — ventanas y ruido de timing

17 nov 2026 · Codex. [Informe](../../../reviews/2026-11-17-g1-a1bis-timing-results.md), [método predeclarado](../../2026-11-17-g1-a1bis-timing-method.md), [autorizaciones](../../../reviews/2026-10-25-first-shot-po-approvals.md). El método es byte a byte el de `73d86a2`; el producto, la grilla, las semillas y las tolerancias no se modificaron.

## Corridas completas

Código/fuente limpia `fe7633d7ad1b01630886aaaf145e69ad9ba57423` en ambos procesos; corrección previa única en esa continuación: completar el valor vacío de `kOutMs`. Revisión estática de las cuatro llamadas `Put`: 13, 30, 7 y 6 parejas, todas con claves literales no vacías y sin duplicación dentro de una llamada. Recibo íntegro en [put-parity-review.json](put-parity-review.json).

| Proceso | Compilación previa (inventario UTC) | Experimento (UTC) | XML fresco |
|---|---|---|---|
| run01 | 17 nov 16:00:25, sin Editor | 16:01:02–16:01:05 | 1/1 Passed, 0 Failed/skipped |
| run02 | 17 nov 16:02:00, sin Editor | 16:02:24–16:02:27 | 1/1 Passed, 0 Failed/skipped |

Los inventarios propios antes de los experimentos también están vacíos (16:00:55 y 16:02:16 UTC). Ambos assemblies compilados tienen SHA-256 `45ab13947b6448b0d4cd34ecc6f4419419d66325dbfe07380775662ff4682421`; fuente experimental `e010b070d6a1da3c582ad3828c7ec4fb141021614269f5a22c9255d139b361b8`. Unity 6000.3.27f1, P2, bounceThreshold 0,1, tempo 0,65, 120 Hz, latencyComp=0, validLimit=false y assist=40 ms. Defaults `430da2faa818`/121 claves, SimulationKey `166a96e319c7`, RallyParams `7d50ddaf5d4f`.

Cada proceso reconstruye los 108 golpes y 648 planes congelados de fase A, comprueba sus campos nombrados y analiza 588 planes con contacto: 291 Rival y 297 Humano (99 repetidos bajo tres etiquetas). Produce 43.080 celdas de decisión y **58.200 releases** (200 por plan del Rival). Cuatro archivos crudos (`source`, `plans`, `intervals`, `samples`) y cuatro derivados (`probabilities`, `tables`, `checks`, `analysis`) coinciden byte a byte entre procesos. [manifest.json](manifest.json) contiene tamaños y SHA-256 de cada archivo por corrida; los XML/logs contienen tiempos y no se comparan como datos deterministas.

Crudos e inventarios están ignorados en `Logs/experiments/g1-timing/run01`, `run02`, `*-runner` y `compile-*-runner`. No se versionan logs, datos privados, rutas de usuario ni device.env. El script local de comprobación de compilación y sus recibos quedan en la evidencia ignorada; sus hashes, junto con el historial de fallos, están en [provenance.json](provenance.json). No se repitió fase A. No volver a ejecutar los dos procesos completados sin encargo nuevo.

## Tablas y denominadores

- [probabilities.tsv](probabilities.tsv): 648 filas, los límites de cada plan en ms **reales** alrededor de T*, velocidad nominal de lanzamiento y velocidad en contacto, postura/ruido, razón, vidrio, probabilidad analítica y recuentos de muestras por categoría. Air/Late/Contact son exhaustivos en esta corrida: ninguna celda, control delta=0 ni muestra produjo SystemFault/NotEvaluable. Las celdas se clasificaron con Resolve real; sus masas normales se integraron sin renormalizar.
- [tables.tsv](tables.tsv): agregado por receptor/dificultad, potencia, banda de lanzamiento, vidrio y banda de velocidad en contacto. Cada receptor/dificultad: 108 intentos, 103 vuelos legales y cinco out (cuatro vidrio, uno lateral). `contactPerLegal` suma pContact / legales; `contactPerPlan`, suma pContact / planes utilizables; `contactPerAll`, suma pContact / todos los 108. Los out no son contactos ni errores de timing del receptor. `airExpected`/`lateExpected` son cantidades esperadas, no porcentajes: dividir por nPlans o legal según la comparación, indicando el denominador.
- Las tres etiquetas Humano tienen las mismas ventanas: 108 geometrías únicas, no 324 observaciones independientes. pContact=1 cuando delta=0 es contacto; cero cuando no existe plan. No es una distribución estimada de jugadores reales. No se confunde recepción potencial del contrato con swing ejecutado, trayectoria posterior ni punto ganado.
- Las bandas de velocidad son semiabiertas en m/s, usando `ShotSolution.Speed` para evitar mover un 28 nominal por redondeo de la norma. [28,32) contiene únicamente 28 nominales. La banda [16,20), ocho casos, no se oculta. Las medianas de ventana/márgenes/velocidad usan solo planes utilizables; no se divide por los casos inalcanzables para reducir artificialmente la anchura.
- [checks.tsv](checks.tsv) y [analysis.json](analysis.json): las comprobaciones de muestreo del método pasan por plan y dificultad. Máximo error individual Contact/Air/Late: **0,0650572 (6,506 puntos porcentuales)**, dentro de su tolerancia predeclarada. Máximo error agregado: **0,00313795 (0,314 puntos)**. Media: contacto analítico 87,0498 % de sus 97 planes frente a 86,7474 % observado. Son controles numéricos de la integración, no aceptación del producto.

Detalle Media P×vidrio, derivado por lectura de `probabilities.tsv` (sumar pContact de las filas seleccionadas; denominador vuelos legales de esa selección):

| P | Vidrio legal: planes / vuelos | Contactos esperados / vuelos | Sin vidrio: planes / vuelos | Contactos esperados / vuelos |
|---|---|---:|---|---:|
| 0,70 | 17/17 | 16,862789 / 17 = 99,1929 % | 17/19 | 14,745578 / 19 = 77,6083 % |
| 0,85 | 19/19 | 16,938154 / 19 = 89,1482 % | 15/16 | 12,263282 / 16 = 76,6455 % |
| 1,00 | 23/25 | 19,112177 / 25 = 76,4487 % | 6/7 | 4,516299 / 7 = 64,5186 % |

La fórmula no estima una tasa de puntos directos del PO: fixture/intención limpia, receptor desde espera quieta, distribución normal prescrita y builds/poblaciones distintos. La comparación histórica 3/18/24 % y 13 timing/9 Unreachable proviene del encargo, no de acceso al iPhone ni de reanalizar su respaldo. El sentido de la tendencia se compara; no se impone igualdad numérica.

## Intentos fallidos y correcciones conservados

| Incidente | Corrección autorizada | Evidencia preservada |
|---|---|---|
| G1-A-EXPORT-01 | Completar parejas de posición; rechazar claves/cabeceras vacías y listas impares. RED 0/4 → GREEN 4/4 | Errata en evidencia de #71; crudos y tablas originales intactos |
| G1-A1BIS-CS-01 | Dos `Is.AnyOf` sustituidos por igualdad explícita a Contact/Air/Late con Is.True | `attempt-CS-01-run01-runner`; **sin XML**, porque nunca llegó al test runner; [recibo histórico](cs-01-manifest.json) |
| G1-A1BIS-EXPORT-02 | Añadir únicamente el valor vacío de kOutMs; revisar las cuatro llamadas Put | `attempt-EXPORT-02-run01-runner`, XML 0/1 Failed(Error), log e inventario; `attempt-EXPORT-02-run01`, source y cabecera samples, cero muestras; [recibo histórico](export-02-manifest.json) |

La compilación previa a EXPORT-02 se preservó en `compile-before-EXPORT-02-run01-runner`. Las separaciones de carpetas conservaron bytes e inodos; no se reemplazó ningún XML fallido por uno aprobado ni se reconstruyeron datos parciales. Los recibos históricos conservan los nombres que tenían las carpetas antes de separarlas; la correspondencia actual está en provenance. No hubo otro defecto ni otra corrección bajo la nueva autonomía experimental antes de las corridas completas. No se modificó ningún dato científico ya producido para aprobar resultados.

Estos 1/1 son experimentos aislados, no suites de regresión. No se ejecutaron PlayMode, AutoQA, check-release, build nativa ni acceso al iPhone en este diagnóstico. No hay diff de producto, defaults, anclas, ProjectSettings ni Packages. El PO decide la corrección de A1; A2 conserva su parada de revisión de diseño antes de implementar. Sin fusión ni instalación.
