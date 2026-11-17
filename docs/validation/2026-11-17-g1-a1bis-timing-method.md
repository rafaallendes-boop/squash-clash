# G1 A1-bis — método predeclarado de timing

17 nov 2026 · Codex · Rama `codex/g1-a1bis-timing`, apilada sobre #71 (`3e1e037`), aún sin fusionar. La continuación y la corrección limitada de G1-A-EXPORT-01 están [autorizadas](../reviews/2026-10-25-first-shot-po-approvals.md). Se confirma este método en commit propio **antes de cualquier Unity nuevo**. No se repite la fase A ni se modifica producto.

## 1. Corrección experimental y comprobación previa

Primero cuatro pruebas de esquema, en el assembly experimental, aisladas de la regresión y sin ejecutar la grilla de fase A: inicialización de recepción con todos los campos nombrados vacíos; rechazo de lista impar de parejas; rechazo de clave vacía; rechazo de cabecera vacía al exportar. RED esperado por aserción sobre el código actual, luego corrección de exportador y GREEN del mismo filtro. Un RED esperado no es un incidente nuevo. Ante fallo distinto del RED predeclarado, detenerse. Sin cambiar datos/tablas de fase A ni sus hashes; nota de errata en su README.

## 2. Población y reconstrucción

Los mismos 108 golpes A1, leídos de `a1-shots.tsv` versionado. Se reconstruyen con `RallyContact.SolveIntention` y P2 reales; velocidad y target deben coincidir exactamente con los campos nombrados de fase A. Tres dificultades y dos receptores: 648 planes reconstruidos con las mismas esperas, reacción, percepción y ruido `FondoAi(seed=1111+índice)`. Se comparan razón, postura, ruido, T* y contacto contra los campos nombrados de `a1-receptions.tsv`. Su columna histórica vacía se admite **solo en la lectura de ese archivo congelado**, con valor obligatorio `bodyContactZ`; no se exporta a ningún resultado nuevo.

Se conserva el denominador 108 por receptor/dificultad y se separan los cinco vuelos out; principal: 103 vuelos legales. Solo planes planned/stretched con T* utilizable reciben el análisis de timing. Humano/Bot: ruido/percepción cero y misma ventana/contrato; sus tres etiquetas de dificultad son controles repetidos, no observaciones independientes. Semillas y grilla intactas. No se emula la selección de opción, el swing posterior, el vector causal ni el juego físico del usuario.

Defaults reales: 121 claves, `430da2faa818`; SimulationKey `166a96e319c7`; RallyParams `7d50ddaf5d4f`. Se registran tempo (actual 0,65), assist, validLimit, validMs, earlyPolicy y LatencyCompMs, sin sustituirlos por valores supuestos. Conversión de pasos a tiempo **real** con `ClockMap` constante (rate=StepHz×tempo), sin pausa ni slow motion. `presentedAtReal=0` y estado inicial step=0: es el mismo origen afín de la recepción, no otro reloj.

## 3. Ventana y probabilidad analítica

Por plan: KIn/KOut/T*, extremos en ms reales respecto de T*, ancho, velocidad de la pelota en T*, velocidad nominal de lanzamiento y norma real de velocidad inicial, holgura, espera, ruido, postura, contacto antes/después del vidrio. Velocidad de lanzamiento para cubetas: `ShotSolution.Speed`, evitando que el redondeo de la norma mueva un 28 nominal a otra banda.

Para incluir exactamente scheduling, cuantización, vecinos legales y recorte de release a presentación, se integra la distribución normal por celdas de decisión de `Resolve`, sin inventar una ventana simétrica. Cortes en: presentación/latencia, inicio de ClockMap, KIn/KOut, T*, ±Assist y ±Valid, pasos enteros/medios y tiempos subpaso de contactos de la trayectoria. Los cortes se expresan como DeltaMs; se incluyen ambas colas infinitas. Dentro de cada celda se clasifica con `RallyContact.Resolve` real en 1/4, 1/2 y 3/4 (colas: puntos finitos posteriores/anteriores al extremo); deben coincidir en Contact/Air/Late/NotEvaluable/SystemFault. Se guardan las celdas y el control delta=0. Los límites puntuales de una normal continua tienen masa cero; el recorte a presentación se incluye en la clasificación constante de su cola.

Python calcula la probabilidad de cada celda `[a,b]` como `Phi(b/sigma)−Phi(a/sigma)` y suma por resultado; `Phi(x)=0,5·erfc(−x/sqrt(2))`, función de la biblioteca estándar ([documentación oficial](https://docs.python.org/3/library/math.html#math.erfc)). Los sigma del Rival son los reales: Fácil 55, Media 38, Difícil 22 ms. Perfect y Valid se suman como Contact, se informan separados en muestras; NotEvaluable/SystemFault nunca son contacto. Se conserva cualquier masa fuera de Contact/Air/Late; no se renormaliza para ocultarla.

## 4. Verificación por muestreo

200 extracciones `RallyRng(seed=20261117+índice).NextGaussian(sigma)` por plan del Rival, misma secuencia normal base entre dificultades y separada del ruido de postura. Cada release pasa por `RallyContact.Resolve` real. Se guardan delta, resultado, fail, motivo y ContactStep. Humano/Bot: basta ventana y delta=0; no se supone una distribución de usuarios reales.

Comprobación estadística predeclarada: por plan y categoría Contact/Air/Late, diferencia absoluta muestreo−analítico ≤ `max(0,025; 5·sqrt(p·(1−p)/200))`. Agregado por dificultad: diferencia ≤ `0,005 + 5·sqrt(sum(200·p_i·(1−p_i)))/(200·nPlanes)`. Se informa error máximo y agregado, no solo un Pass. Un incumplimiento o una clasificación no constante dentro de celda detiene la entrega sin nuevas muestras ni ajuste de tolerancia. Cualquier SystemFault o NotEvaluable en un intervalo con masa positiva, en delta=0 o en muestras se registra y detiene; no se fuerza a Air/Late.

## 5. Salidas e interpretación

- Tablas por dificultad × velocidad de lanzamiento en bandas de 4 m/s `[0,4),…,[28,32)` y bandas residuales explícitas; se destacan 20–24, 24–28 y 28–32 como el encargo. Se incluyen velocidades **en contacto** y anchos/asimetría de ventana.
- Air/Late esperados entre planes y sobre los 103 vuelos legales; recepción total esperada = suma de probabilidad de contacto de cada plan / 103. También dividir por los 108 intentos geométricos, identificando los cinco out, para conservar trazabilidad. No sustituir promedio de productos por producto de promedios entre estratos.
- Por P=0,70/0,85/1: n legal, planned/stretched/unreachable, recepción con delta=0 y probabilidad total con ruido. Separar vidrio legal y resto.
- Hipótesis: comparar ancho/asimetría y pérdida por timing a mayor velocidad. Informar si la tendencia es consistente, parcial o no confirmada; no fijar una aceptación de producto a partir de este diagnóstico. Separar efecto de velocidad, ubicación de T* y dificultad; una ventana larga con T* en el borde también puede perder la mitad de una normal centrada.
- Métricas históricas del PO: solo referencia del encargo (3/18/24 % y 13 timing frente a 9 unreachable en 22 puntos de ≥24 m/s). No se accede al iPhone ni se reanalizan respaldos privados. Son población/builds distintos, mayoría anteriores a T3 pure y sin bandera de vidrio; no probar causalidad ni reproducir sus porcentajes por construcción.

## 6. Ejecución y parada

Experimento `[Explicit]`, assembly `SquashClash.EditMode.Experiments`, filtro propio. Dos procesos únicos `SC_G1_TIMING_RUN=run01`/`run02` sobre el mismo head limpio; TSV sin timestamps: source, plans, intervals y samples idénticos byte a byte (SHA-256). Antes de **cada** Unity (incluidos RED/GREEN): inventario PID/comando/proyecto, si hay Editor no lanzar. No se cierra otro Unity. XML fresco, total>0, cero CS; separar RED de guardas de las dos corridas científicas. No PlayMode; si otro encargo lo ejecuta, restaurar ProjectSettings conforme al PO.

Crudos/logs/inventarios bajo `Logs/experiments/g1-timing/` (ignorados); evidencia derivada sanitizada <1 MB y manifiesto de hashes. La corrección del exportador no cambia las corridas originales ni autoriza su repetición. Ante defecto/sorpresa nueva, registrar y detenerse. Sin cambios de producto, DefaultPreset, hashes, anclas ni AutoQA; no se ejecutan suites completas/AutoQA/check-release como supuesta aceptación de este diagnóstico.

Entrega: informe de una página, 2–3 opciones de timing con efecto en RallyParams/Defaults/anclas/AutoQA V/T3, sin implementar ninguna. Mantener la parada de elección de A1 y la de revisión del diseño de A2 antes de implementar. Sin fusión ni instalación.
