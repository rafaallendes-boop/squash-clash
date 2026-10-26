# Propuesta S1: preset de prueba de potencia y comprobación de fijación

**Propuesta documental para Claude/PO, 2026-10-25. No autoriza ejecutar la sesión.** Rama `firstshot/s1-power-calibration-proposal`, desde main **16b3a8e46a5f8961f48682f92516271ebbd09a02** tras fusionar PR #20. Sin código, archivo de preset, defaults, build, instalación ni medición nuevos. **S1 sigue sin aceptar; S2–S4/T8 no autorizados.**

Propongo probar una capa temporal **S1-Power-Calib-A**: full=**500 mm/s**, deadZone=**0**, minSamples=**3**, maxGap=**51 ms**, manteniendo flick=**250 mm/s**. Aumenta la lectura de los fuertes y permite observar potencia pequeña en algunos suaves, pero **no resuelve la parada del dedo antes de soltar**: con las mismas trazas aún quedarían **14/20 suaves con P=0**. Se trata de sensibilidad experimental, no de valores calibrados ni de aceptación.

## Fuente y corrección de la evidencia

Única fuente de datos: [revalidación 2, JSON](../validation/evidence/2026-10-25-s1-revalidation-2.json), **SHA-256 ab43e560f23fa9522fa4d00c0fa4ed842f26922b180d8f7a21f8aaac2aaf4b69**, y [su informe corregido](2026-10-25-first-shot-s1-revalidation-2.md). Se seleccionan exclusivamente **protocol=soft/strong: 40 gestos**, diez por serie original 30/20, trials 1–10. No se usan como observaciones nuevas ni se mezclan 311cb11, c5a7d1e, los 92 preview o los 6 intentos de distancia. Manos mezcladas, orden fijo y falta de regla física siguen limitando la generalización: las velocidades son mm/s calculados con DPI reportado, no calibrados con regla.

El encargo decía 15 conservadores/5 medidos suaves. Verificados los originales, el PO aclaró literalmente **«Usar 16/4 del JSON»**. Se usa esa cifra: **16 conservadores (12 few-samples +4 sample-gap), 4 medidos**, no se altera ninguna traza.

| Hecho observado | Distribución original |
|---|---|
| Suaves | **20/20 P=0**; 16 conservadores y 4 medidos a **9,859 / 14,386 / 20,651 / 25,411 mm/s**, todos bajo deadZone=100 |
| Fuertes | Máximo P **20,523 %**; 16 medidos con v neta **75,798–387,317 mm/s**, mediana **191,661 mm/s**; full=1500, deadZone=100. Otros 4 conservadores (1 few-samples,3 sample-gap) |
| Fijación | **14/20 fuertes; 0/20 suaves** en Frozen, flick=250, confirmación=2 intervalos; P y fijación son cálculos distintos |
| Cobertura | **40/40 ventanas de 80 ms completas**, cero short-coverage; minCoverage=20/30 nunca limitó. Experimento **NotInformative para cobertura**, se mantiene 30 ms |
| Restricciones efectivas | minSamples=4 y maxGap=34 no cambiaron; las 20 salidas conservadoras del conjunto se deben a esos filtros, no a cobertura |

El PO describe que para fuertes apunta y acelera, y a veces al levantar el dedo la potencia queda débil. Las trazas suaves muestran escaso movimiento neto al final y huecos largos; son compatibles con la parada antes de soltar. No se observa el movimiento físico directamente: **no se atribuye a todos los conservadores una causa exclusiva ni se inventa frecuencia del sensor**. Los huecos/samples son el diagnóstico registrado. La diferencia percibida 30/20 se atribuyó en la corrección de Claude/PO a fuertes más rápidos en la tanda 20; no demuestra efecto de cobertura.

## Preset propuesto, solo como tabla

Base: **P0-calib-1**, con sus valores efectivos actuales y las claves S1 resueltas desde defaults. Este documento **no crea, guarda ni carga un JSON de preset**. Si se autoriza una sesión posterior, se puede aplicar la tabla desde el panel como capa temporal sin sobrescribir P0-calib-1 ni Po-1.

| Clave | Actual | Candidata | Justificación en estos datos y riesgo |
|---|---:|---:|---|
| power.fullMmPerS | 1500 | **500** | El máximo nativo válido 387,317 solo llega a 20,523 %. Full=500 lo llevaría a 77,463 %. La fuerte 30-5, antes rechazada por hueco 50,008 ms, da **456,813 mm/s reconstruidos offline**, no velocidad nativa medida; también queda bajo 500 (91,363 %). Hay margen sin saturar ninguna de las 23 trazas elegibles. Riesgo: una velocidad moderada recibe mucha más potencia y un gesto rápido accidental puede alcanzar 100 %; estos 40 no acotan futuros máximos |
| power.deadZoneMmPerS | 100 | **0** | Los 4 suaves medidos 9,859–25,411 quedan todos anulados por 100. Quitar la zona muerta permite verlos como 1,972–5,082 %. Los 2 suaves que pasan los filtros nuevos darían 0,754/1,748 %. **Riesgo deliberado:** cualquier velocidad neta positiva elegible, incluso temblor/deriva, produce P>0. No es una recomendación de default |
| power.minSamples | 4 | **3** | Suaves: n entre 1 y 7, mediana 3; fuertes: uno con 3 y 19 con 6–7. Tres admite el suave 20-9 con n=3/gap 50,009, sin abrir hasta 2. En estas trazas, bajar a 2 **no añade ningún caso** que cumpla también gap≤51; los de n≤2 tienen huecos≥66,684 ms. Riesgo: menos soporte temporal para estimar extremos; tres puntos no garantizan un gesto fiable |
| power.maxGapMs | 34 | **51** | Permite los huecos registrados 50,008–50,009 ms (1 fuerte y 2 suaves al combinar minSamples=3), evitando que el escalón 50 siga rechazándolos por unas milésimas. Paso del catálogo 1 ms; no cambia su rango 10–60. Aún rechaza 58,355–299,990 ms. No se elige 60 por rescatar solo un suave adicional de 5,965 mm/s con hueco 58,355/80 ms. Riesgo: interpolar hasta 51 ms dentro de una ventana de 80 puede ocultar parada/aceleración y aceptar un valor alto antes protegido; la fuerte 30-5 ilustra ese riesgo |
| aim.flickMmPerS | 250 | **250, conservar** | Ya separó 0/20 suaves de 14/20 fuertes y el PO aceptó apuntado/saltos. **Bajar flick no sube P**. Seis fuertes no se fijaron; un pico aislado no cumple los 2 intervalos consecutivos. No se cambia el umbral para esconder ese resultado. Un suave tuvo pico 273,047 sin confirmación: bajar el umbral podría producir fijaciones no deseadas en otros gestos. No se estima tasa futura de falsos positivos con solo 20 suaves |

Se conservan **power.windowMs=80, power.minCoverageMs=30, power.curveExponent=1**, confirmación=2, histéresis=40, lookback=50 y ganancia=0,12. Los cuatro cambios de potencia están dentro de los rangos/pasos vigentes. Mantener la fijación evita confundir un cambio de detección con el cambio de escala de P. El preset **no arregla los 6 fuertes sin Frozen**, y no presupone que P alta signifique mira fijada ni al revés.

Hashes efectivos **solo calculados offline, no aplicados**: candidato sobre P0 con repeat=3 **aded02107500**; capa de sesión con repeat=0 **04862ced7433**. No son hashes de defaults ni archivos de preset nuevos. Se verificó la misma serialización canónica contra defaults 00a501ad7066 y P0 actual 9977d9ffd7e5 antes de calcularlos.

## Cálculo offline de los 40 gestos: no medición nueva

Se mantienen posiciones, timestamps, liberación y escala de cada traza. Se reutilizan **CoveredMs, DistinctSamples y MaxGapMs nativos** para evaluar los filtros candidatos (cobertura≥30, n≥3 y gap≤51, con las tolerancias ya presentes en el estimador). Los 40 tenían 80 ms cubiertos; la etiqueta de cohorte original 20/30 sirve para identificar el registro, **todos los cálculos candidatos usan minCoverage=30**.

En los elegibles se interpola la posición en liberación−80 ms y se toma la posición de liberación, según [PowerEstimator](../../Assets/SquashClash/Core/Shot/PowerEstimator.cs). Velocidad neta = distancia entre esos extremos × mmPerPx / duración. Con exponent 1, la candidata es **P = clamp(v/500,0,1)**; si falla un filtro, P=0 y se conserva el motivo. Los criterios/tolerancias de producto no se modifican. Para una salida conservadora original, netSpeed=0 era un valor de retorno protegido, **no velocidad medida**: se reconstruyen extremos antes de calcular su posible P; no se aplica una regla de tres a ese cero.

Comprobación aritmética: al usar los parámetros originales, se reproducen los **20 casos nativamente measured (4 suaves+16 fuertes)** con error absoluto observado **0** en v/P serializados, empleando redondeos float32 en posiciones, interpolación y fórmula. No se afirma identidad bit a bit de toda Unity ni de trazas rechazadas por el original. Las nuevas cifras para casos antes conservadores son contrafactuales de la interpolación, no evidencia de velocidad física ni de fiabilidad del gesto. No se reejecuta AimModel: la columna Fijada es el estado **original**, con sus parámetros sin cambios.

### Cohorte original 30 ms — suaves

| Intento | P observada (%) | P offline candidata (%) | v neta offline si válida (mm/s) | Motivo offline | Fijada original |
|---|---:|---:|---:|---|---|
| 1 | 0,000 | 0,000 | — | sample-gap | No |
| 2 | 0,000 | 0,000 | — | few-samples | No |
| 3 | 0,000 | 0,000 | — | sample-gap | No |
| 4 | 0,000 | 0,000 | — | sample-gap | No |
| 5 | 0,000 | 0,000 | — | sample-gap | No |
| 6 | 0,000 | 0,000 | — | sample-gap | No |
| 7 | 0,000 | 0,000 | — | sample-gap | No |
| 8 | 0,000 | 2,877 | 14,386 | measured | No |
| 9 | 0,000 | 4,130 | 20,651 | measured | No |
| 10 | 0,000 | 5,082 | 25,411 | measured | No |

### Cohorte original 30 ms — fuertes

| Intento | P observada (%) | P offline candidata (%) | v neta offline si válida (mm/s) | Motivo offline | Fijada original |
|---|---:|---:|---:|---|---|
| 1 | 5,039 | 34,111 | 170,553 | measured | No |
| 2 | 16,646 | 66,608 | 333,042 | measured | Sí |
| 3 | 7,629 | 41,360 | 206,799 | measured | No |
| 4 | 0,000 | 0,000 | — | sample-gap | No |
| 5 | 0,000 | 91,363 | 456,813 | measured | Sí |
| 6 | 20,523 | 77,463 | 387,317 | measured | Sí |
| 7 | 1,782 | 24,989 | 124,947 | measured | No |
| 8 | 0,000 | 0,000 | — | sample-gap | Sí |
| 9 | 2,171 | 26,079 | 130,396 | measured | Sí |
| 10 | 0,000 | 15,160 | 75,798 | measured | No |

### Cohorte original 20 ms — suaves

| Intento | P observada (%) | P offline candidata (%) | v neta offline si válida (mm/s) | Motivo offline | Fijada original |
|---|---:|---:|---:|---|---|
| 1 | 0,000 | 0,000 | — | few-samples | No |
| 2 | 0,000 | 0,000 | — | few-samples | No |
| 3 | 0,000 | 0,000 | — | sample-gap | No |
| 4 | 0,000 | 0,000 | — | few-samples | No |
| 5 | 0,000 | 0,000 | — | few-samples | No |
| 6 | 0,000 | 1,972 | 9,859 | measured | No |
| 7 | 0,000 | 0,000 | — | few-samples | No |
| 8 | 0,000 | 1,748 | 8,742 | measured | No |
| 9 | 0,000 | 0,754 | 3,768 | measured | No |
| 10 | 0,000 | 0,000 | — | few-samples | No |

### Cohorte original 20 ms — fuertes

| Intento | P observada (%) | P offline candidata (%) | v neta offline si válida (mm/s) | Motivo offline | Fijada original |
|---|---:|---:|---:|---|---|
| 1 | 0,000 | 0,000 | — | sample-gap | Sí |
| 2 | 2,330 | 26,525 | 132,625 | measured | No |
| 3 | 8,780 | 44,585 | 222,924 | measured | Sí |
| 4 | 18,778 | 72,577 | 362,885 | measured | Sí |
| 5 | 5,328 | 34,917 | 174,587 | measured | Sí |
| 6 | 5,466 | 35,305 | 176,523 | measured | Sí |
| 7 | 4,288 | 32,007 | 160,034 | measured | Sí |
| 8 | 13,769 | 58,552 | 292,761 | measured | Sí |
| 9 | 14,043 | 59,320 | 296,602 | measured | Sí |
| 10 | 15,267 | 62,749 | 313,745 | measured | Sí |


**Resumen contrafactual de las mismas trazas**, incluidos todos los ceros y conservadores:

| Grupo | P mínima / mediana / máxima (%) | P>0 | Conservadores con candidata |
|---|---|---:|---|
| Suaves (20 gestos) | 0 / 0 / **5,082** | **6/20** | **14: 7 few-samples +7 sample-gap** |
| Fuertes (20 gestos) | 0 / **35,111** / **91,363** | **17/20** | **3 sample-gap** |

La mediana suave sigue en 0: **un preset no reconstruye un movimiento ausente en los últimos 80 ms**. No se prometen 20 suaves con potencia. Los cuatro cambios tampoco quedan causalmente aislados por este cálculo conjunto. Mejoran legibilidad/sensibilidad en algunas trazas, con más exposición a ruido y a interpolación; falta el juicio físico. Estas 40 series eran todos Swing: no prueban inmunidad a toques accidentales, cancelaciones ni otras manos/dispositivos. La ausencia original de Frozen suave no garantiza ausencia futura de fijaciones erróneas.

## Protocolo propuesto para una sesión posterior corta

**Esta PR no autoriza realizarla ni aplicar el preset.** Si el PO la autoriza después:

1. Usar la app/producto que el PO autorice y verificar su identidad con main. Sin instalar por rutina; si una instalación futura está autorizada, comprobar vigencia efectiva del perfil y renovarlo primero desde el 27. Respaldar **P0-calib-1, Po-1 y last-active**, comprobar arranque P0 **9977d9ffd7e5/cero pendientes** y ausencia de cambios inesperados.
2. Aplicar la tabla como capa temporal sobre P0. Poner **launcher.repeatSeconds=0 sin guardar**, Aplicar y relanzar; verificar valores, cero pendientes y hash calculado 04862ced7433 (recalcular si hubiera cambiado legítimamente el producto/base). Conservar captura de configuración y cada lanzamiento/hash antes de empezar. Si hay diferencia no explicada, no medir ni dar por válida la configuración.
3. **Una sola mano declarada antes de empezar**, preferentemente izquierda dominante si el PO la elige; anotar mano/dedo, no alternar. Recordatorio: «S1 no golpea la pelota. Apunta y termina el gesto soltando el dedo; si lo paras y esperas antes de soltar, P puede quedar en cero. La mira se fija por la rapidez del gesto; P y Fijada no significan lo mismo». Es una explicación de control, no una exigencia de conseguir un número. No pedir que fuerce gestos hasta llenar la escala ni sustituya resultados débiles.
4. DEV → **10 suaves**, después DEV → **10 fuertes**; DEV se cierra solo. Registrar los diez intentos de cada serie, incluidos Tap/Canceled/SystemCanceled/P=0 y todos los motivos conservadores. Sin 2 min, regla ni comparación 20/30 en este bloque corto. Ante cambio de mano/configuración, identificar intentos afectados como no comparables; no borrarlos. Ante defecto, detener la medición y conservar evidencia, con restauración de cierre.
5. Recuperar registros. Informar min/mediana/máximo de P y v neta, motivos conservadores, n/gap, Frozen por serie, saltos y cualquier potencia inesperada durante gesto lento. Usar solo escala DPI declarada; no afirmar mm físicos sin regla. No introducir bandas ni umbrales nuevos de aprobación.
6. Preguntar **por separado y citar literalmente**: «¿Apuntas donde quieres?»; «¿Entiendes siempre dónde apuntas?»; «¿El número responde a suave/fuerte?»; «¿La mira se fija cuando esperas?»; «¿Los saltos de la mira al fijarse son aceptables?»; «¿Apareció potencia que no querías en un gesto lento o un toque?». Registrar también comodidad/observaciones espontáneas sin convertirlas en aceptación. No pedir diferencias 20/30: cobertura no cambia aquí. Cualquier comparación de memoria con la sesión anterior se marca subjetiva, no ensayo controlado.
7. Restaurar **P0-calib-1** al final (recarga/reinicio de la app autorizada), comprobar en nuevo arranque **9977d9ffd7e5/cero pendientes**, repeat=3 y potencia original 1500/100/4/34, coverage=30; cotejar **los tres archivos byte a byte** con el respaldo. No guardar la capa experimental sobre presets existentes. Informe y juicio del PO antes de decidir cualquier siguiente paso.

Este protocolo produce una observación exploratoria de un preset conjunto, no una calibración causal de cada clave. El PO conserva la decisión de repetir, ajustar o rechazarlo. Nada se promueve por las cifras offline ni por un «bien» global.

## Invariantes y límites de esta entrega

**DefaultPreset.json y hash 00a501ad7066 intactos**, 41 valores heredados/proyección 0d39e6016bd8, **SimulationKey 166a96e319c7**, spec, planes, rangos, umbrales de aceptación y criterios intactos. Los valores propuestos no son defaults ni una autorización para editarlos. Sin preset real creado/modificado, código, tests, build, instalación ni acceso al iPhone en esta tarea.

Las trazas originales y sus respuestas literales no cambian. P0 quedó restaurado en la revalidación 2 (arranque 23:24:50.6920160Z, 9977d9ffd7e5/cero pendientes); **no se presenta esa verificación anterior como comprobación nueva**. Cálculo local derivado en `Logs/evidence/s1-power-calibration-proposal/`, ignorado; la selección de fuente, fórmula, filtros y tabla completa permiten revisar el cálculo desde la evidencia versionada. No se versionan calculadoras, credenciales, logs originales ni builds.

Entrega: [PR #21 lista para Claude](https://github.com/rafaallendes-boop/squash-clash/pull/21) **sin fusionar**, fuente documental/numérica 480598400866ee36599818084b198f831dd04a70. **S1 sigue sin aceptar, S2–S4/T8 sin autorizar. Detenerse.** Comprobar siempre el perfil antes de una instalación futura y renovarlo primero desde el 27.
