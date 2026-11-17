# Evidencia parcial de G1 A1 — release con margen

17 nov 2026 · Codex. [Criterios previos](../../2026-11-17-g1-a1-release-margin-criteria.md), [informe de estado](../../../reviews/2026-11-17-g1-a1-release-margin.md). **Criterios científicos medidos, empaquetado incompleto por G1-A1-REPORT-01.** No hay aprobación integral de entrega ni AutoQA/check-release.

## Fuente y corridas

Producto `1dba20d` y código experimental `9bf1c5aafd524e7d1a923266b4af45213d037f40`. Los datos se produjeron en dos procesos sobre ese mismo head limpio, con inventarios PID/comando/proyecto vacíos antes de cada invocación. Unity 6000.3.27f1, tempo 0,65, latencia 0, P2 y sigma 55/38/22 ms, sin variar la grilla de 108 golpes ni sus semillas.

| Corrida | Experimento UTC | XML |
|---|---|---|
| run01 | 17 nov 16:42:41–16:42:44 | 1/1 Passed, cero failed/skipped |
| run02 | 17 nov 16:43:59–16:44:02 | 1/1 Passed, cero failed/skipped |

Por corrida: 648 filas de planes, 43.080 celdas y 58.200 muestras (200 por cada uno de 291 planes utilizables del Rival). Los controles de Humano son los 99 planes repetidos bajo tres etiquetas, no 297 observaciones independientes. Se reconstruyen y comparan los campos nombrados históricos; el Humano conserva ventana, velocidad, contacto y delta=0. La columna vacía histórica se lee solo del archivo congelado, sin propagarla.

Cuatro crudos (`source.tsv`, `plans.tsv`, `intervals.tsv`, `samples.tsv`) y cuatro derivados (`probabilities.tsv`, `tables.tsv`, `checks.tsv`, `analysis.json`) coinciden byte a byte entre run01/run02. El script comprobó esa igualdad antes del fallo; una lectura forense posterior la confirmó sin escribir en los datos. Todos los archivos originales se conservan en `Logs/experiments/g1-release-margin/`, ignorados, con sus `*-runner` para XML/logs/inventarios. Los RED/GREEN y suites viven en `Logs/evidence/g1-a1/`.

## Archivos parciales versionados

- [probabilities.tsv](probabilities.tsv): 648 filas con probabilidad, media desplazada, sigma, identidad elegible/verificada y conteo de muestra. Cada fila utilizable del Rival conserva sigma; la función es la misma que usa FondoAi. Ventanas seguras: 207 planes, 41.400 muestras con rawDeltaMs=deltaMs bit a bit. La distribución conserva colas: no recorta las muestras ni garantiza éxito por definición.
- [tables.tsv](tables.tsv): mismos denominadores del método A1: 108 entradas, 103 legales y cinco out por receptor/dificultad; 29 planes medios utilizables a P=1. `contactPerLegal` y `contactPerPlan` son denominadores diferentes; C4 se aplica solo al segundo en ese estrato. Se incluyen todos los stretched. Las medianas de ventanas no incluyen planes inexistentes.
- [checks.tsv](checks.tsv), [analysis.json](analysis.json): analítica N(mu,sigma) frente a 200 muestras por plan, con las tolerancias fijadas. Orden total de dificultades 84,4593/89,9821/95,8018 %. Medio P=1 sobre planes: 93,2646 % analítico, 5.401/5.800 muestras. Proyección de RallyParams sin el campo nuevo `7d50ddaf5d4f`; hash completo `927e304bbfec`. DefaultPreset/P0, SimulationKey y anclas sin cambios; no se accedió al iPhone.
- [failure-receipt.json](failure-receipt.json): recibo de **entrega incompleta**, hashes de ambas corridas y sus XML/logs, RED/GREEN y EditMode/PlayMode. No sustituye el manifiesto que debía generar el script ni marca AutoQA/check-release como aprobados.

El generador copió los cuatro derivados antes de intentar acceder a `Logs/experiments/g1-release-margin/schema-red-runner`, inexistente: arrastre de la lógica de empaquetado de A1-bis. El fallo ocurre en líneas 196–197, después de todos los cálculos y de la producción de datos. No se crean carpetas con nombres falsos, no se copia otro XML para aparentar ese gate y no se corrige/reintenta el script bajo la autorización limitada a fallos previos a datos. Quedan pendientes corrección autorizada del empaquetado, AutoQA V/T3 y check-release.

Tests nuevos TDD: RED 2/10 controles, ocho fallos por aserción; GREEN 10/10, EditMode 764/764 y PlayMode 581/581. Tests existentes sin editar ni cambiar veredicto; ProjectSettings restaurado. No se ejecutó otro proceso Unity después del fallo de empaquetado. A2 se entregó por separado en #73 sin Unity y se detuvo para revisión. Sin fusión ni instalación.
