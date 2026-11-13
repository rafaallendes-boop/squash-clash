# CQ-ZONE-v1 — evidencia descriptiva de propuesta

Fuente Unity limpia: `ad3d25977cab6c912085ca281dc47a0cd6c6f5aa`; método predeclarado `e8a26c1`. [Informe y decisión del PO](../../../reviews/2026-11-13-first-shot-causal-quality.md). Solo arnés explícito; no capa causal productiva, H3b, regresión, AutoQA, check-release ni aceptación de First Shot.

Dos procesos Unity independientes, secuenciales, mismo HEAD. Cada uno completó 1/1 ejecución explícita, cero failed/skipped/CS, salida 0. Los **ocho TSV originales** aquí preservados resultaron idénticos byte a byte; [manifest.json](manifest.json) contiene sus SHA-256, bytes y hashes/horas de XML/logs locales ignorados. Esos logs/XML no se versionan. El conteo 1/1 es ejecución del arnés, no aceptación de una hipótesis o mecánica.

- 6 recetas SL1–SL6 + primeras 24 recepciones utilizables del rival observadas en 8 rallies (97 observadas, 0 sin plan utilizable, 73 posteriores al límite de captura). No se seleccionaron resultados favorables.
- 3 tempos × 3 compensaciones = 270 condiciones; 3 formas = **810 filas en zones.tsv**. 21.318 observaciones/celdas del contrato; 8.910 sondas con forma; 5.576 estados P2/cuerpo y 90 eventos de superficie.
- Configuración intacta: defaults `c0e9f00176d5` / 108, P0 derivado `42d9efbf50fc`, SimulationKey `166a96e319c7`, RallyParams actual `7d50ddaf5d4f`; bounceThreshold float `0.1`, 120 Hz, cero topes/instancias vivas.
- Radio fijo = máximo medido de las imágenes Perfect: **0.31612509229966818 m**. Es un resultado de la muestra, no un default, umbral aprobado o garantía universal.

## Archivos y lectura

| Archivo | Contenido |
|---|---|
| metadata.tsv | Configuración, fuente, conteos, radio fijo y límites. |
| geometry.tsv | Estados de pelota y cuerpo efectivo por paso, coordenadas relativas q, arco firmado desde T* y legalidad según el cuerpo del plan. |
| events.tsv | Eventos P2 entrantes por trayectoria. |
| capture.tsv | Todas las recepciones observadas; id vacío = posterior al límite (en esta muestra no hubo planes inutilizables). |
| rally-sample.tsv | Procedencia de los ocho rallies observados, sin regenerar la campaña histórica de 1000. |
| cells.tsv | Bordes, celdas abiertas y sondas; clasificación, motivo, paso, vecino y distancias de las tres formas. |
| probes.tsv | Sondas Δ predeclaradas: distancia y vector de timing **propuesto**, por forma; vacío en fallos. |
| zones.tsv | Tamaño de zona y proporciones por trayectoria, reloj y forma. |
| tables.md | [Tablas redondeadas para lectura](tables.md), reproducibles con `python3 analyze.py`. |

Los números fuente usan punto decimal, formato invariante round-trip y True/False de C#. No se usan tolerancias para decidir si d>0. `failedMs` incluye la duración sin contacto y nunca entra como cero en una media de vectores. `contactMs` y `failedMs` particionan los 200 ms de ±100; su máximo residuo numérico es 7,39e−13 ms. Las duraciones de bordes aislados son cero.

`zeroSpanSteps` es **último−primer nodo con vector cero**, no número inclusivo de nodos ni duración de sueltas Perfect. `zeroSpanRealMs=zeroSpanSteps/(120·tempo)·1000`; `zeroSpanArcM` es el arco entre esos nodos, no diámetro de una esfera. `zeroNodeDiameterM` informa el máximo alejamiento entre los nodos cero; `LarcM` y `adaptiveRadiusM` son la extensión geométrica propia de cada forma. Los conteos zeroNodes/nonzeroNodes permiten distinguir nodos de anchura. La polilínea propuesta tiene extremos interpolados que pueden quedar entre nodos; esos extremos no crean nuevos estados de contacto. Las tablas informan la anchura efectiva **sobre estados cuantizados**.

`kNonzeroFraction` usa **nodos legales** de K como denominador, no tiempo continuo ni intentos. `nonzeroOverContactFraction` usa duración de sueltas de ±100 con contacto; `nonzeroOver200msFraction` usa los 200 ms completos. `earlyContactMs`/`lateContactMs` se refieren a las alas −100..−40 y +40..+100; excluyen Perfect. Una ala con duración cero no es un fallo de la zona ni una medición de vector cero. Los demás campos separan ms Perfect y Valid no Perfect. Los máximos de altura/pérdida de velocidad se calculan en las celdas con contacto; no son salida de solver ni percepción humana.

La comprobación de cobertura es una inclusión de los contactos Perfect de cada condición en las dos zonas adaptativas, no una prueba del dominio universal de trayectorias y ClockMap. Las referencias son por recepción/tempo; su admisibilidad frente a la invariante literal requiere revisión, como explica el informe. La esfera fija solo tiene cobertura medida en la muestra. Los niveles de ayuda no se comparan: no afectan al contrato que se está midiendo aquí y este no es el experimento H3b.
