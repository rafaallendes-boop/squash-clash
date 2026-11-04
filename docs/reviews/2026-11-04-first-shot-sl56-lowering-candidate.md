# Evaluación offline de la candidata «bajar el punto»

2026-11-04. Entrega documental adicional en PR #31 para revisión de Claude/PO; PR #30 y #31 siguen sin fusionar.

## Lectura de los resultados

La búsqueda Y encuentra retorno dentro de cancha en **26.125/26.136 solicitudes focales** y **53.247/53.361 controles**. Preserva exactamente todas las bases que ya daban retorno dentro: 17.403 focales y 49.044 controles. La guardia conserva también los retornos cortos. Bajar potencia, con la misma guardia, obtiene 26.099/26.136 y 53.089/53.361, respectivamente.

La candidata no resuelve toda la rejilla y no garantiza continuidad del punto final. En focales, el descenso aplicado tiene mediana 1,25 m y máximo 4,000000179 m. Aparecen retornos con punto pedido casi sobre tin+huella en P baja y variaciones de TargetUsed entre vecinos de hasta 2,95 m en el eje P. En controles hay variaciones mayores, algunas ya presentes en las asistencias originales. Estos datos no eligen regla, parámetros ni implementación y no miden percepción humana.

En C=(3,20; 2,20), Y da bote en mitad trasera en 24/24 focales para P=0,3–1,0; P conserva 17/24. Esto describe solo esa columna. En toda la rejilla Y cambia 4.203 controles: **«los 49 controles no cambian» se confirma para C, no para todos los puntos**. El subconjunto bajo/suave conserva todos sus retornos previos, pero modifica solicitudes que antes no tenían solución; no se afirma que todas las dejadas posibles sean inalteradas.

## Decisión, predeclaración y condiciones

El PO comunica aprobación de Claude sin correcciones de PR #31/head546b184 y declara literalmente:

> Sí perfecto. Me parece esa regla candidata para evaluarla offline

La autorización se limita al diagnóstico de bajar y manteniendo x/P pedidas. [Aprobaciones](2026-10-25-first-shot-po-approvals.md). Método **140d0ca confirmado antes del primer cálculo**, sin refinamientos posteriores: [predeclaración](../diagnostics/2026-11-04-sl56-lowering-rule-method.md). Paso Y=0,05 m, contraste P=0,05; extremo inferior abierto tin+huella existente, con primer float32 superior como último candidato. Primer candidato que pasa, sin optimizar profundidad. Si no se encuentra, no hay trayectoria final ni otra asistencia inventada.

Las mismas 73 posiciones (24 focales, 49 controles), 99 puntos y 11 potencias P=0…1 producen **79.497 solicitudes por brazo / 238.491 filas**. Posiciones de los contactos Applied, no son 73 jugadores ni nuevas mediciones. No se añaden muestras para los que fallaron. Se usan ShotSolver/P2 originales, física y horizonte del mapa aprobado; no Editor, suites, build de producto ni acceso al iPhone.

«Retorno dentro de cancha» conserva exactamente la intersección descriptiva del mapa: Solved, primer impacto frontis, primer bote dentro del rectángulo y sin out/tin/escape observado antes o al mismo tiempo que ese bote. Mitad trasera: primer bote z≥4,875 m. Pared trasera: impacto finito antes del primer bote, con retorno dentro. No equivalen a una regla de golpe jugable aceptada.

## Frecuencia y resultados por grupo

Todos los contadores de resultado usan N original, incluso si falta salida final. «Busca» incluye cambios exitosos y búsquedas sin solución. Los eventos/zonas se superponen; no sumar columnas.

| Grupo | Brazo | N | Preserva | Busca | Cambia | Sin solución | Dentro | Bote trasero | Pared trasera | Lateral antes de bote (dentro) |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| focal | base | 26.136 | 0 | 0 | 0 | 0 | 17.403 | 1.521 | 217 | 13.956 |
| focal | Y | 26.136 | 17.403 | 8.733 | 8.722 | 11 | 26.125 | 6.270 | 2.501 | 21.246 |
| focal | P | 26.136 | 17.403 | 8.733 | 8.696 | 37 | 26.099 | 3.389 | 684 | 20.263 |
| control | base | 53.361 | 0 | 0 | 0 | 0 | 49.044 | 19.075 | 2.799 | 20.254 |
| control | Y | 53.361 | 49.044 | 4.317 | 4.203 | 114 | 53.247 | 22.831 | 5.952 | 22.795 |
| control | P | 53.361 | 49.044 | 4.317 | 4.045 | 272 | 53.089 | 22.278 | 5.078 | 22.803 |


La base tiene 40 focales y 635 controles Unsolved; los demás fallos base son trayectorias resueltas fuera de la intersección. Y deja 11/114 sin solución encontrada y P deja 37/272, respectivamente. «Sin solución» significa que esta búsqueda discreta no la encontró, no imposibilidad continua. Sus datos físicos finales son ausentes, nunca ceros. Las 66.447 bases dentro se conservan sin cambio de solicitud ni resultado en ambos brazos.


### Eventos y salidas disponibles


| Grupo | Brazo | Con trayectoria | Out≤bote | Tin≤bote | Escape≤bote | Bote fuera | Plano fuera extensión | Lateral finito≤bote | Lateral cualquier momento | Paso trasero de retorno dentro | Cap | Censura |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| focal | base | 26.096 | 2.856 | 0 | 7.087 | 5.854 | 553 | 18.573 | 18.979 | 1.521 | 0 | 0 |
| focal | Y | 26.125 | 0 | 0 | 0 | 0 | 0 | 21.246 | 21.652 | 6.270 | 0 | 0 |
| focal | P | 26.099 | 0 | 0 | 0 | 0 | 0 | 20.263 | 20.669 | 3.389 | 0 | 0 |
| control | base | 52.726 | 3.482 | 0 | 377 | 239 | 58 | 22.583 | 24.456 | 19.075 | 0 | 0 |
| control | Y | 53.247 | 0 | 0 | 0 | 0 | 0 | 22.795 | 24.671 | 22.831 | 0 | 0 |
| control | P | 53.089 | 0 | 0 | 0 | 0 | 0 | 22.803 | 24.742 | 22.278 | 0 | 0 |


Todos los 237.382 resultados disponibles entre los tres brazos tienen primer impacto en frontis; los 1.109 restantes no tienen trayectoria final. No se cuentan las 195.936 evaluaciones únicas como solicitudes humanas adicionales.

Los laterales «cualquier momento» se cuentan hasta completar el paso del segundo bote o el horizonte. La base puede seguir contrafactualmente tras un out; no se interpreta como jugada válida. Out/escape se evalúan como en el mapa, con envolvente muestreada al final de paso y sus limitaciones. En los resultados finales disponibles de Y/P no quedan out/tin/escape hasta el bote por construcción de la intersección. El dato no convierte los casos sin solución en éxito.

### Por P original

Cada fila focal tiene N=2.376 solicitudes; cada control N=4.851. Dentro/bote trasero/pared trasera siempre sobre ese N, no solo los cambiados.


**focal**


| P | Brazo | Preserva | Busca | Cambia | Sin solución | Dentro | Bote trasero | Pared trasera | Lateral dentro |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 0.0 | base | 0 | 0 | 0 | 0 | 2.340 | 0 | 0 | 1.140 |
| 0.0 | Y | 2.340 | 36 | 26 | 10 | 2.366 | 0 | 0 | 1.166 |
| 0.0 | P | 2.340 | 36 | 0 | 36 | 2.340 | 0 | 0 | 1.140 |
| 0.1 | base | 0 | 0 | 0 | 0 | 2.372 | 0 | 0 | 1.556 |
| 0.1 | Y | 2.372 | 4 | 3 | 1 | 2.375 | 0 | 0 | 1.559 |
| 0.1 | P | 2.372 | 4 | 3 | 1 | 2.375 | 0 | 0 | 1.559 |
| 0.2 | base | 0 | 0 | 0 | 0 | 2.214 | 0 | 0 | 1.640 |
| 0.2 | Y | 2.214 | 162 | 162 | 0 | 2.376 | 0 | 0 | 1.802 |
| 0.2 | P | 2.214 | 162 | 162 | 0 | 2.376 | 0 | 0 | 1.781 |
| 0.3 | base | 0 | 0 | 0 | 0 | 1.866 | 202 | 0 | 1.621 |
| 0.3 | Y | 1.866 | 510 | 510 | 0 | 2.376 | 217 | 0 | 2.042 |
| 0.3 | P | 1.866 | 510 | 510 | 0 | 2.376 | 202 | 0 | 1.925 |
| 0.4 | base | 0 | 0 | 0 | 0 | 1.689 | 165 | 0 | 1.504 |
| 0.4 | Y | 1.689 | 687 | 687 | 0 | 2.376 | 457 | 0 | 2.067 |
| 0.4 | P | 1.689 | 687 | 687 | 0 | 2.376 | 260 | 0 | 1.950 |
| 0.5 | base | 0 | 0 | 0 | 0 | 1.483 | 200 | 23 | 1.363 |
| 0.5 | Y | 1.483 | 893 | 893 | 0 | 2.376 | 806 | 193 | 2.086 |
| 0.5 | P | 1.483 | 893 | 893 | 0 | 2.376 | 362 | 53 | 1.969 |
| 0.6 | base | 0 | 0 | 0 | 0 | 1.284 | 237 | 20 | 1.194 |
| 0.6 | Y | 1.284 | 1.092 | 1.092 | 0 | 2.376 | 898 | 373 | 2.093 |
| 0.6 | P | 1.284 | 1.092 | 1.092 | 0 | 2.376 | 454 | 73 | 1.976 |
| 0.7 | base | 0 | 0 | 0 | 0 | 1.146 | 200 | 42 | 1.075 |
| 0.7 | Y | 1.146 | 1.230 | 1.230 | 0 | 2.376 | 926 | 442 | 2.102 |
| 0.7 | P | 1.146 | 1.230 | 1.230 | 0 | 2.376 | 482 | 107 | 1.985 |
| 0.8 | base | 0 | 0 | 0 | 0 | 1.087 | 185 | 40 | 1.032 |
| 0.8 | Y | 1.087 | 1.289 | 1.289 | 0 | 2.376 | 966 | 477 | 2.107 |
| 0.8 | P | 1.087 | 1.289 | 1.289 | 0 | 2.376 | 522 | 135 | 1.990 |
| 0.9 | base | 0 | 0 | 0 | 0 | 986 | 158 | 43 | 939 |
| 0.9 | Y | 986 | 1.390 | 1.390 | 0 | 2.376 | 981 | 498 | 2.110 |
| 0.9 | P | 986 | 1.390 | 1.390 | 0 | 2.376 | 537 | 148 | 1.993 |
| 1.0 | base | 0 | 0 | 0 | 0 | 936 | 174 | 49 | 892 |
| 1.0 | Y | 936 | 1.440 | 1.440 | 0 | 2.376 | 1.019 | 518 | 2.112 |
| 1.0 | P | 936 | 1.440 | 1.440 | 0 | 2.376 | 570 | 168 | 1.995 |


**control**


| P | Brazo | Preserva | Busca | Cambia | Sin solución | Dentro | Bote trasero | Pared trasera | Lateral dentro |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 0.0 | base | 0 | 0 | 0 | 0 | 4.730 | 0 | 0 | 1.656 |
| 0.0 | Y | 4.730 | 121 | 92 | 29 | 4.822 | 0 | 0 | 1.748 |
| 0.0 | P | 4.730 | 121 | 0 | 121 | 4.730 | 0 | 0 | 1.656 |
| 0.1 | base | 0 | 0 | 0 | 0 | 4.730 | 0 | 0 | 1.656 |
| 0.1 | Y | 4.730 | 121 | 95 | 26 | 4.825 | 0 | 0 | 1.751 |
| 0.1 | P | 4.730 | 121 | 1 | 120 | 4.731 | 0 | 0 | 1.657 |
| 0.2 | base | 0 | 0 | 0 | 0 | 4.759 | 0 | 0 | 1.787 |
| 0.2 | Y | 4.759 | 92 | 83 | 9 | 4.842 | 0 | 0 | 1.870 |
| 0.2 | P | 4.759 | 92 | 62 | 30 | 4.821 | 0 | 0 | 1.849 |
| 0.3 | base | 0 | 0 | 0 | 0 | 4.755 | 0 | 0 | 1.750 |
| 0.3 | Y | 4.755 | 96 | 90 | 6 | 4.845 | 0 | 0 | 1.782 |
| 0.3 | P | 4.755 | 96 | 95 | 1 | 4.850 | 0 | 0 | 1.821 |
| 0.4 | base | 0 | 0 | 0 | 0 | 4.728 | 1.503 | 0 | 1.846 |
| 0.4 | Y | 4.728 | 123 | 116 | 7 | 4.844 | 1.568 | 0 | 1.935 |
| 0.4 | P | 4.728 | 123 | 123 | 0 | 4.851 | 1.508 | 0 | 1.952 |
| 0.5 | base | 0 | 0 | 0 | 0 | 4.672 | 2.380 | 0 | 1.901 |
| 0.5 | Y | 4.672 | 179 | 170 | 9 | 4.842 | 2.526 | 0 | 2.050 |
| 0.5 | P | 4.672 | 179 | 179 | 0 | 4.851 | 2.443 | 0 | 2.061 |
| 0.6 | base | 0 | 0 | 0 | 0 | 4.569 | 2.818 | 208 | 1.948 |
| 0.6 | Y | 4.569 | 282 | 274 | 8 | 4.843 | 3.083 | 243 | 2.192 |
| 0.6 | P | 4.569 | 282 | 282 | 0 | 4.851 | 2.998 | 228 | 2.202 |
| 0.7 | base | 0 | 0 | 0 | 0 | 4.381 | 3.006 | 474 | 1.999 |
| 0.7 | Y | 4.381 | 470 | 466 | 4 | 4.847 | 3.471 | 842 | 2.328 |
| 0.7 | P | 4.381 | 470 | 470 | 0 | 4.851 | 3.391 | 690 | 2.325 |
| 0.8 | base | 0 | 0 | 0 | 0 | 4.103 | 3.049 | 540 | 1.968 |
| 0.8 | Y | 4.103 | 748 | 743 | 5 | 4.846 | 3.791 | 1.242 | 2.383 |
| 0.8 | P | 4.103 | 748 | 748 | 0 | 4.851 | 3.708 | 1.027 | 2.397 |
| 0.9 | base | 0 | 0 | 0 | 0 | 3.893 | 3.168 | 665 | 1.915 |
| 0.9 | Y | 3.893 | 958 | 955 | 3 | 4.848 | 4.122 | 1.609 | 2.361 |
| 0.9 | P | 3.893 | 958 | 958 | 0 | 4.851 | 4.042 | 1.366 | 2.419 |
| 1.0 | base | 0 | 0 | 0 | 0 | 3.724 | 3.151 | 912 | 1.828 |
| 1.0 | Y | 3.724 | 1.127 | 1.119 | 8 | 4.843 | 4.270 | 2.016 | 2.395 |
| 1.0 | P | 3.724 | 1.127 | 1.127 | 0 | 4.851 | 4.188 | 1.767 | 2.464 |


## Descenso del punto y contraste de potencia

Distribuciones nearest-rank, n de cambios exitosos. Δy es y original menos y solicitada tras regla; Δy total incluye TargetLowered original; ΔP es P original menos P pedida tras regla. Diferencias calculadas sobre float32 reales enviados al solver. Las tablas de [resumen](../validation/evidence/2026-11-04-sl56-lowering-summary.json) separan además preservadas/sin solución y cada P/contacto. No hay Δy/ΔP final para «sin solución».

| Grupo | Brazo | Magnitud | n | min | p5 | p50 | p95 | p99 | max |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| focal | Y | deltaY | 8722 | 0.00999999046 | 0.100000024 | 1.25 | 3.30000019 | 3.80000019 | 4.00000018 |
| focal | Y | totalDeltaY | 8722 | 0.00999999046 | 0.100000024 | 1.25 | 3.30000019 | 3.80000019 | 4.00000018 |
| focal | P | deltaP | 8696 | 0.0499999523 | 0.0500000119 | 0.349999994 | 0.75 | 0.849999994 | 0.849999994 |
| focal | P | totalDeltaY | 8696 | 0 | 0 | 0 | 0 | 0 | 0.379872322 |
| control | Y | deltaY | 4203 | 0.00999999046 | 0.0499999523 | 0.5 | 1.50000024 | 1.80000019 | 2.20000029 |
| control | Y | totalDeltaY | 4203 | 0.00999999046 | 0.0499999523 | 0.5 | 1.55000019 | 1.80000019 | 3.91384524 |
| control | P | deltaP | 4045 | 0.0499999523 | 0.0500000119 | 0.199999988 | 0.550000012 | 0.699999988 | 0.75 |
| control | P | totalDeltaY | 4045 | 0 | 0 | 0 | 0 | 1.29874849 | 3.44987893 |


Y mantiene exactamente x/P pedidos en sus cambios; P mantiene x/y pedidos. Eso no obliga al solver a conservar velocidad o TargetUsed. Ningún paso ni valor de estas búsquedas se propone como parámetro de producto.

## Tin y potencia alta

Distancia continua respecto a **TinTop+0,010f ≈ 0,489999989 m**, sin umbral añadido de «cerca». Se distinguen TargetUsed y primer impacto real; pueden diferir por las aproximaciones originales. El clasificador incluye EdgeEpsilon=1e-6f. El extremo abierto y≈0,4900000095 m puede ser aceptado si el impacto calculado queda por encima del borde expandido: no es un margen seguro demostrado. No se corrige ni desplaza el extremo después de observarlo.

Entre cambios Y, el margen mínimo de TargetUsed es **2,048909664e−8 m** en ambos grupos. Ejemplos: 11-03/x6,38/y0,50/P0 acaba pidiendo y0,4900000095, impacto y0,4900037348; 03b-01/x0,02/y0,50/P0,1 usa el mismo punto, impacto y0,4900049567. El menor margen del impacto real entre todos los cambios es 1,003965735e−6 m focal y 1,093372703e−6 m control. Son resultados numéricos próximos a la frontera, no tolerancia de producto elegida.

Con P originales 0,8–1,0, los puntos Y **cambiados** tienen márgenes mínimos mayores que esos extremos de P baja; las solicitudes **preservadas** incluyen y0,50 con margen nominal≈0,01 m. No atribuir este mínimo preservado a un descenso de la candidata.

| Grupo | Brazo | P | Estado | n | mín margen m | p5 | p50 | p95 | p99 | máx |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| focal | Y | 0.8 | preserved | 1087 | 0.010000011 | 0.010000011 | 0.409999987 | 1.71000006 | 2.30999996 | 2.30999996 |
| focal | Y | 0.8 | changed | 1289 | 0.159999987 | 0.260000011 | 0.710000059 | 2.35999992 | 2.46000006 | 2.46000006 |
| focal | Y | 0.9 | preserved | 986 | 0.010000011 | 0.010000011 | 0.409999987 | 1.71000006 | 2.30999996 | 2.30999996 |
| focal | Y | 0.9 | changed | 1390 | 0.110000035 | 0.159999987 | 0.610000035 | 2.26000001 | 2.30999996 | 2.30999996 |
| focal | Y | 1.0 | preserved | 936 | 0.010000011 | 0.010000011 | 0.409999987 | 1.71000006 | 1.71000006 | 1.71000006 |
| focal | Y | 1.0 | changed | 1440 | 0.0600000229 | 0.159999987 | 0.510000011 | 2.16000011 | 2.21000006 | 2.21000006 |
| focal | P | 0.8 | preserved | 1087 | 0.010000011 | 0.010000011 | 0.409999987 | 1.71000006 | 2.30999996 | 2.30999996 |
| focal | P | 0.8 | changed | 1289 | 0.209999999 | 0.710000059 | 3.01000001 | 4.0600002 | 4.0600002 | 4.0600002 |
| focal | P | 0.9 | preserved | 986 | 0.010000011 | 0.010000011 | 0.409999987 | 1.71000006 | 2.30999996 | 2.30999996 |
| focal | P | 0.9 | changed | 1390 | 0.209999999 | 0.710000059 | 2.30999996 | 4.0600002 | 4.0600002 | 4.0600002 |
| focal | P | 1.0 | preserved | 936 | 0.010000011 | 0.010000011 | 0.409999987 | 1.71000006 | 1.71000006 | 1.71000006 |
| focal | P | 1.0 | changed | 1440 | 0.209999999 | 0.409999987 | 2.30999996 | 4.0600002 | 4.0600002 | 4.0600002 |
| control | Y | 0.8 | preserved | 4103 | 0.010000011 | 0.010000011 | 1.01000001 | 3.51000001 | 4.0600002 | 4.0600002 |
| control | Y | 0.8 | changed | 743 | 0.110000035 | 2.55999996 | 3.05999996 | 3.76000001 | 3.91000011 | 4.01000001 |
| control | Y | 0.9 | preserved | 3893 | 0.010000011 | 0.010000011 | 1.01000001 | 3.01000001 | 4.0600002 | 4.0600002 |
| control | Y | 0.9 | changed | 955 | 0.110000035 | 2.35999992 | 2.80999996 | 3.5600002 | 3.76000001 | 3.91000011 |
| control | Y | 1.0 | preserved | 3724 | 0.010000011 | 0.010000011 | 1.01000001 | 3.01000001 | 3.51000001 | 4.0600002 |
| control | Y | 1.0 | changed | 1119 | 0.110000035 | 2.05999996 | 2.55999996 | 3.41000011 | 3.91000011 | 4.01000001 |
| control | P | 0.8 | preserved | 4103 | 0.010000011 | 0.010000011 | 1.01000001 | 3.51000001 | 4.0600002 | 4.0600002 |
| control | P | 0.8 | changed | 748 | 0.010000011 | 3.01000001 | 3.54454209 | 4.0600002 | 4.0600002 | 4.0600002 |
| control | P | 0.9 | preserved | 3893 | 0.010000011 | 0.010000011 | 1.01000001 | 3.01000001 | 4.0600002 | 4.0600002 |
| control | P | 0.9 | changed | 958 | 0.010000011 | 3.01000001 | 3.51000001 | 4.0600002 | 4.0600002 | 4.0600002 |
| control | P | 1.0 | preserved | 3724 | 0.010000011 | 0.010000011 | 1.01000001 | 3.01000001 | 3.51000001 | 4.0600002 |
| control | P | 1.0 | changed | 1127 | 0.010000011 | 2.30999996 | 3.51000001 | 4.0600002 | 4.0600002 | 4.0600002 |


[Detalle tin](../validation/evidence/2026-11-04-sl56-lowering-tin.json): distribuciones del margen de TargetUsed y primer impacto, grupos completos, P alta conjunta y P0,8/0,9/1,0 separadas; **20 más próximos** por grupo/brazo/sección, con identificadores de solicitud, flags y primer bote. Son rangos descriptivos y paginación, sin corte de aceptación.

## Continuidad entre entradas vecinas

Se incluyen todos los pares vecinos predeclarados por contacto: 990 ejeP, 968 eje x y 990 eje y, **215.204 pares por brazo / 645.612 en total**. Se informa n con ambas salidas y ausencias aparte. En base, «con resultado» significa Solved incluso si el retorno queda fuera; en Y/P solo existe salida si se preservó o encontró retorno dentro. No usar estas poblaciones distintas como demostración de continuidad mejorada.

La tabla muestra |ΔTargetUsed.y| entre vecinos, en metros. Para eje y, una parte sigue el cambio solicitado; el JSON incluye diferencia con signo, pendiente por separación real y exceso respecto al Δy de entrada, además de solicitud final/ΔreglaY/ΔreglaP. No se fija un umbral para llamar errático/brusco ni se afirma cómo lo sentiría el PO.

| Grupo | Brazo | Eje | Pares | Ambas salidas | Falta≥1 | p50 m | p95 m | p99 m | máx m |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| focal | base | P | 23.760 | 23.718 | 42 | 0 | 0 | 5.79953194e-05 | 2.25004578 |
| focal | base | x | 23.232 | 23.192 | 40 | 0 | 0 | 0.00186920166 | 2.25004578 |
| focal | base | y | 23.760 | 23.707 | 53 | 0.335845947 | 0.700000048 | 0.700000048 | 2.75004578 |
| focal | Y | P | 23.760 | 23.749 | 11 | 0 | 0.549999952 | 1.4000001 | 2.95000017 |
| focal | Y | x | 23.232 | 23.221 | 11 | 0 | 0.799999952 | 1.39999986 | 2.25004578 |
| focal | Y | y | 23.760 | 23.749 | 11 | 0.199999988 | 0.599999905 | 0.700000048 | 2.75004578 |
| focal | P | P | 23.760 | 23.723 | 37 | 0 | 0 | 1.52587891e-05 | 2.25004578 |
| focal | P | x | 23.232 | 23.195 | 37 | 0 | 0 | 0.0125061274 | 2.25004578 |
| focal | P | y | 23.760 | 23.712 | 48 | 0.301740885 | 0.700000048 | 0.700000048 | 2.75004578 |
| control | base | P | 48.510 | 47.620 | 890 | 0 | 0 | 0.557864189 | 3.76597619 |
| control | base | x | 47.432 | 46.797 | 635 | 0 | 0 | 0.857081652 | 3.76597619 |
| control | base | y | 48.510 | 47.601 | 909 | 0.400000095 | 0.700000048 | 1.08724666 | 3.89784718 |
| control | Y | P | 48.510 | 48.362 | 148 | 0 | 0.250000238 | 0.704943895 | 3.86384505 |
| control | Y | x | 47.432 | 47.318 | 114 | 0 | 0.300000191 | 1.10000014 | 3.91384524 |
| control | Y | y | 48.510 | 48.396 | 114 | 0.300000072 | 0.700000048 | 1.12754822 | 3.89784718 |
| control | P | P | 48.510 | 48.238 | 272 | 0 | 0 | 0.664698601 | 3.76597619 |
| control | P | x | 47.432 | 47.160 | 272 | 0 | 0 | 0.986762941 | 3.76597619 |
| control | P | y | 48.510 | 48.075 | 435 | 0.398832798 | 0.700000048 | 1.21484852 | 3.89784718 |


Ejemplos máximos del brazo Y, seleccionados por el rango predeclarado:

- **Focal 12-05, x3,20/y4,55, P0,2→0,3:** pasa de preservado con TargetUsed4,550000191 a cambiado con TargetUsed1,600000024; diferencia−2,950000167 m. Es transición de la guardia a la búsqueda, sin asistencia previa en los dos resultados.
- **Control 08-10, x6,38/y4,55, P0,2→0,3:** y pedida tras regla4,449999809→4,5 (solo+0,050000191 m), pero TargetUsed0,636154950→4,5 (+3,863845050 m). SpeedRaised/TargetLowered están activos en el primer resultado y desactivados en el segundo. La regla externa elige petición más alta probada, no TargetUsed máximo continuo.
- **Control 08-10, y4,55/P0, x6,05→6,38:** TargetUsed4,550000191→0,636154950 (−3,913845241 m); en la segunda salida actúa TargetLowered, además de la búsqueda externa. En base ese segundo extremo no tenía solución; no se puede rellenar con altura cero.
- **Control 09-09, x6,38/P0,7, y4,0→4,55:** ambos preservados, pero TargetUsed0,652153015→4,550000191 (+3,897847176 m) por asistencia original. La candidata no introduce esa transición; tampoco la elimina con su guardia.
- **Focal 12-08**, x6,05→6,38, y3,5/P0: ambos preservados, TargetUsed3,5→1,249954224. Los cambios entre vecinos pueden existir ya en el solver original.

Como contraste, máximos de |ΔP final| de bajarP: focal0,150000006 entre P vecinas,0,549999982 entre x vecinas y0,399999976 entre y vecinas; control0,300000012/0,699999988/0,649999976. No son pasos elegidos, son diferencias entre dos resultados finales de la rejilla inicial.

[Continuidad completa resumida](../validation/evidence/2026-11-04-sl56-lowering-continuity.json) conserva distribuciones y **top20 por eje/grupo/brazo** de |ΔTargetUsed.y|, |Δy solicitada final| y |ΔP final|. Incluye transiciones preservada↔cambiada y resultado↔sin solución, sin mezclar ausencia con cero.

| Grupo | Brazo | Eje | Preserva→cambia | Cambia→preserva | Resultado→ausencia | Ausencia→resultado | Ambas ausentes |
| --- | --- | --- | --- | --- | --- | --- | --- |
| focal | Y | P | 1445 | 31 | 0 | 10 | 1 |
| focal | Y | x | 1191 | 1164 | 11 | 0 | 0 |
| focal | Y | y | 1764 | 28 | 0 | 11 | 0 |
| focal | P | P | 1445 | 6 | 0 | 36 | 1 |
| focal | P | x | 1167 | 1162 | 35 | 2 | 0 |
| focal | P | y | 1753 | 8 | 11 | 30 | 7 |
| control | Y | P | 1347 | 319 | 42 | 63 | 43 |
| control | Y | x | 1254 | 883 | 86 | 28 | 0 |
| control | Y | y | 2133 | 344 | 0 | 113 | 1 |
| control | P | P | 1385 | 268 | 0 | 121 | 151 |
| control | P | x | 1148 | 831 | 192 | 80 | 0 |
| control | P | y | 1963 | 193 | 176 | 239 | 20 |


## Dejadas y controles

Subconjunto predeclarado de y={0,50;0,70;0,90}, P={0;0,1;0,2}, todas x: 1.944 focales y 3.969 controles. Es una sección de rejilla baja/suave, no una medición de intención de dejada.

| Grupo | Brazo | N | Preserva | Busca | Cambia | Sin solución | Dentro | Bote trasero | Pared trasera | Lateral antes de bote (dentro) |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| focal | base | 1.944 | 0 | 0 | 0 | 0 | 1.916 | 0 | 0 | 1.174 |
| focal | Y | 1.944 | 1.916 | 28 | 17 | 11 | 1.933 | 0 | 0 | 1.191 |
| focal | P | 1.944 | 1.916 | 28 | 1 | 27 | 1.917 | 0 | 0 | 1.175 |
| control | base | 3.969 | 0 | 0 | 0 | 0 | 3.815 | 0 | 0 | 1.056 |
| control | Y | 3.969 | 3.815 | 154 | 90 | 64 | 3.905 | 0 | 0 | 1.146 |
| control | P | 3.969 | 3.815 | 154 | 40 | 114 | 3.855 | 0 | 0 | 1.096 |


Los 1.916 retornos bajos/suaves focales y 3.815 controles que ya estaban dentro se conservan exactamente, con su profundidad y asistencias. Y rescata 17 focales y90 controles previamente Unsolved; P rescata1 y40. Ninguno de los resultados finales de esta sección bota en mitad trasera ni toca pared trasera antes del bote. No equivale a conservar todas las solicitudes: existen cambios y búsquedas sin solución. No se probó intención humana, percepción ni valores intermedios fuera de la rejilla.

En C=(3,20;2,20), los49controles×11P=539solicitudes quedan preservados en ambos brazos, mientras que en toda la rejilla de controles Y activa4.317búsquedas/cambia4.203 yP activa4.317/cambia4.045. La preservación exacta se garantiza por la guardia para las bases dentro de esta muestra, no para cualquier combinación futura.

### Columna C, focales, N=24 por P

| P | Brazo | Preserva | Cambia | Sin solución | Dentro | Bote trasero | Pared trasera |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 0.0 | base | 0 | 0 | 0 | 24 | 0 | 0 |
| 0.0 | Y | 24 | 0 | 0 | 24 | 0 | 0 |
| 0.0 | P | 24 | 0 | 0 | 24 | 0 | 0 |
| 0.1 | base | 0 | 0 | 0 | 24 | 0 | 0 |
| 0.1 | Y | 24 | 0 | 0 | 24 | 0 | 0 |
| 0.1 | P | 24 | 0 | 0 | 24 | 0 | 0 |
| 0.2 | base | 0 | 0 | 0 | 24 | 0 | 0 |
| 0.2 | Y | 24 | 0 | 0 | 24 | 0 | 0 |
| 0.2 | P | 24 | 0 | 0 | 24 | 0 | 0 |
| 0.3 | base | 0 | 0 | 0 | 17 | 17 | 0 |
| 0.3 | Y | 17 | 7 | 0 | 24 | 24 | 0 |
| 0.3 | P | 17 | 7 | 0 | 24 | 17 | 0 |
| 0.4 | base | 0 | 0 | 0 | 0 | 0 | 0 |
| 0.4 | Y | 0 | 24 | 0 | 24 | 24 | 0 |
| 0.4 | P | 0 | 24 | 0 | 24 | 17 | 0 |
| 0.5 | base | 0 | 0 | 0 | 0 | 0 | 0 |
| 0.5 | Y | 0 | 24 | 0 | 24 | 24 | 0 |
| 0.5 | P | 0 | 24 | 0 | 24 | 17 | 0 |
| 0.6 | base | 0 | 0 | 0 | 0 | 0 | 0 |
| 0.6 | Y | 0 | 24 | 0 | 24 | 24 | 20 |
| 0.6 | P | 0 | 24 | 0 | 24 | 17 | 0 |
| 0.7 | base | 0 | 0 | 0 | 0 | 0 | 0 |
| 0.7 | Y | 0 | 24 | 0 | 24 | 24 | 24 |
| 0.7 | P | 0 | 24 | 0 | 24 | 17 | 0 |
| 0.8 | base | 0 | 0 | 0 | 0 | 0 | 0 |
| 0.8 | Y | 0 | 24 | 0 | 24 | 24 | 24 |
| 0.8 | P | 0 | 24 | 0 | 24 | 17 | 0 |
| 0.9 | base | 0 | 0 | 0 | 0 | 0 | 0 |
| 0.9 | Y | 0 | 24 | 0 | 24 | 24 | 24 |
| 0.9 | P | 0 | 24 | 0 | 24 | 17 | 0 |
| 1.0 | base | 0 | 0 | 0 | 0 | 0 | 0 |
| 1.0 | Y | 0 | 24 | 0 | 24 | 24 | 24 |
| 1.0 | P | 0 | 24 | 0 | 24 | 17 | 0 |


La lectura exploratoria atribuida a Claude sobre P0,3–0,7 en C queda contextualizada por esta evaluación formal; no se usó como meta ni se refinó el método para lograr24/24. ParaP0–0,2 la candidata conserva retornos cortos. ParaP0,8–1,0 en C baja el punto, y todos los focales de esa columna llegan a la pared trasera antes del bote. La rejilla completa tiene muchos otros retornos cortos; C no representa todos los tiros.

## SpeedRaised y TargetLowered originales

Flags de la salida final, sobre N original y con n disponible explícito. La razón/velocidad/punto real se conserva por evaluación; las cifras de transición incluyen solo resultados finales, no se interpreta un caso sin solución como False→False. El JSON separa las transiciones y el cambio de velocidad respecto de bases resueltas.

| Grupo | Brazo | N | Con resultado | SpeedRaised | TargetLowered | Cambios con SpeedRaised | Cambios con TargetLowered |
| --- | --- | --- | --- | --- | --- | --- | --- |
| focal | base | 26136 | 26096 | 2332 | 256 | — | — |
| focal | Y | 26136 | 26125 | 2347 | 262 | 15 | 8 |
| focal | P | 26136 | 26099 | 3147 | 294 | 815 | 40 |
| control | base | 53361 | 52726 | 10772 | 1521 | — | — |
| control | Y | 53361 | 53247 | 10979 | 1599 | 207 | 81 |
| control | P | 53361 | 53089 | 10820 | 1649 | 48 | 131 |


En cambios Y cuyas bases ya eran Solved, la velocidad final coincide con la base (n=8.693 focales y3.682 controles); algunas bases Unsolved rescatadas activan SpeedRaised. No se generaliza que conservarP prohíba al solver elevar velocidad. En bajarP, las reducciones de velocidad frente a bases Solved son −18,69999981…−0,399926186 m/s focales y −16,5…−1,099998474 m/s controles. TargetLowered puede bajar y real aunque el brazo P conserve y pedida; máximo descenso total de esos cambios≈0,379872322 m focal y3,449878931 m control. La interacción explica por qué «solo bajar un valor pedido» no significa que solo cambie una magnitud final.

Nota C4, sin diseño ni implementación autorizados: una representación futura tendría que distinguir punto original, punto pedido tras regla y TargetUsed final. Este informe no elige cómo visualizarlo.

## Integridad, reproducción y limitaciones

- Método140d0ca enGit antes del primer cálculo; arnés original del mapa reutilizado sin cambiar función física ni clasificador. Base completa idéntica byte a byte a79.497filas aprobadas, SHA-256 `913551248ac43ad9e029dfbcd5d6551f9cf19cf9775faeb0e7b3a2aaaba69b43`.
- Dos ejecuciones completas con los cincoTSV deterministas idénticos. **195.936evaluaciones únicas /264.286referencias de caché**, sin fusionar contactos repetidos. **380.725referencias de candidatos** auditadas: secuencia/guardia/límite/paso/x-P o x-y constantes/primera solución/ausencia sin ceros.
- 521 archivos protegidos del diagnóstico conservanSHA-256; incluye producto/tests/defaults/SL/spec/plan, además de documentos de origen. Los primarios y derivados previos quedan intactos. Logs/evidence conserva sus3.145 archivos / 875.351.187 bytes/rutas+hash+tamaño+mtimeNs idénticos al inventario previo; solo lectura. Salidas nuevas bajo Logs/lowering-evaluation-20261104 ignorado.
- [Reproducción y arnés exacto](../diagnostics/2026-11-04-sl56-lowering-rule-reproduction.md), [resumen por grupo/P/contacto y casos sin solución](../validation/evidence/2026-11-04-sl56-lowering-summary.json), [continuidad](../validation/evidence/2026-11-04-sl56-lowering-continuity.json), [tin](../validation/evidence/2026-11-04-sl56-lowering-tin.json).

La búsqueda discreta no demuestra máximos continuos ni imposibilidad entre pasos; no se afina después de resultados. El primer punto pedido válido puede dar un TargetUsed muy distinto por la asistencia previa. El límite exterior usa huella visual/clasificadora0,010m; radio físico P2 sigue0,020m. El mismo modelo finito de paredes/horizonte y la misma limitación de muestreo del mapa se mantienen. EjecuciónMono offline no se declara bit a bit equivalente a IL2CPP/iPhone ni reemplaza juicio delPO.

No se elige una regla o parámetros de producto, no se declara aprobada la candidata, y no se implementanC4/S3–S4/T8. PR#30/#31 siguen abiertas/sin fusionar. Sesión física permanece pausada hasta aviso delPO, sin tocar iPhone/relanzar/preparar16. Tandas2–4/restauraciones antes del **2026-11-07T17:58:46Z (7nov14:58:46Chile)** conservan prioridad; comprobar ventana al volver y avisar antes de instalar si no alcanza. Esta entrega se detiene para revisiónClaude/PO.

## Resumen por posición de contacto

Cada contacto tiene N=1.089 solicitudes por brazo. Cifras Dentro/Bote trasero se dan sobre ese N. Posiciones exactas y desglose porP en elJSON, sin identificadores del dispositivo.

| Contacto | Grupo | Base dentro | Y cambia | Y sin sol. | Y dentro | Y trasero | P cambia | P sin sol. | P dentro | P trasero |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 02-03 | control | 991 | 94 | 4 | 1085 | 483 | 96 | 2 | 1087 | 469 |
| 03b-01 | control | 986 | 100 | 3 | 1086 | 480 | 97 | 6 | 1083 | 466 |
| 03b-02 | control | 992 | 97 | 0 | 1089 | 482 | 97 | 0 | 1089 | 468 |
| 03b-03 | control | 1001 | 86 | 2 | 1087 | 483 | 84 | 4 | 1085 | 469 |
| 03b-04 | control | 986 | 100 | 3 | 1086 | 480 | 97 | 6 | 1083 | 466 |
| 03b-05 | control | 1001 | 86 | 2 | 1087 | 483 | 84 | 4 | 1085 | 469 |
| 03b-06 | control | 992 | 97 | 0 | 1089 | 482 | 97 | 0 | 1089 | 468 |
| 03b-07 | control | 990 | 99 | 0 | 1089 | 482 | 97 | 2 | 1087 | 468 |
| 03b-09 | control | 1001 | 86 | 2 | 1087 | 483 | 84 | 4 | 1085 | 469 |
| 03b-10 | control | 992 | 97 | 0 | 1089 | 482 | 97 | 0 | 1089 | 468 |
| 04b-01 | control | 989 | 99 | 1 | 1088 | 485 | 91 | 9 | 1080 | 471 |
| 04b-03 | control | 966 | 121 | 2 | 1087 | 476 | 117 | 6 | 1083 | 462 |
| 04b-04 | control | 989 | 99 | 1 | 1088 | 485 | 91 | 9 | 1080 | 471 |
| 04b-05 | control | 985 | 101 | 3 | 1086 | 482 | 98 | 6 | 1083 | 468 |
| 04b-06 | control | 989 | 99 | 1 | 1088 | 485 | 91 | 9 | 1080 | 471 |
| 04b-08 | control | 964 | 122 | 3 | 1086 | 479 | 118 | 7 | 1082 | 463 |
| 04b-10 | control | 976 | 112 | 1 | 1088 | 482 | 105 | 8 | 1081 | 468 |
| 05-01 | control | 985 | 101 | 3 | 1086 | 482 | 98 | 6 | 1083 | 468 |
| 05-03 | control | 985 | 101 | 3 | 1086 | 482 | 98 | 6 | 1083 | 468 |
| 05-08 | control | 989 | 99 | 1 | 1088 | 485 | 91 | 9 | 1080 | 471 |
| 06-01 | control | 1017 | 72 | 0 | 1089 | 458 | 66 | 6 | 1083 | 452 |
| 06-03 | control | 1020 | 67 | 2 | 1087 | 452 | 67 | 2 | 1087 | 439 |
| 06-04 | control | 1017 | 72 | 0 | 1089 | 458 | 66 | 6 | 1083 | 452 |
| 06-05 | control | 1017 | 70 | 2 | 1087 | 460 | 65 | 7 | 1082 | 454 |
| 06-07 | control | 1024 | 65 | 0 | 1089 | 460 | 60 | 5 | 1084 | 454 |
| 06-08 | control | 1020 | 67 | 2 | 1087 | 452 | 67 | 2 | 1087 | 439 |
| 06-09 | control | 1020 | 67 | 2 | 1087 | 452 | 67 | 2 | 1087 | 439 |
| 06-10 | control | 1020 | 67 | 2 | 1087 | 452 | 67 | 2 | 1087 | 439 |
| 07-02 | control | 1023 | 64 | 2 | 1087 | 461 | 59 | 7 | 1082 | 455 |
| 07-03 | control | 1017 | 70 | 2 | 1087 | 460 | 65 | 7 | 1082 | 454 |
| 07-04 | control | 1017 | 72 | 0 | 1089 | 458 | 66 | 6 | 1083 | 452 |
| 07-05 | control | 1020 | 67 | 2 | 1087 | 452 | 67 | 2 | 1087 | 439 |
| 07-08 | control | 1021 | 68 | 0 | 1089 | 457 | 66 | 2 | 1087 | 444 |
| 07-09 | control | 1021 | 68 | 0 | 1089 | 457 | 66 | 2 | 1087 | 444 |
| 07-10 | control | 1024 | 65 | 0 | 1089 | 460 | 60 | 5 | 1084 | 454 |
| 08-02 | control | 1011 | 73 | 5 | 1084 | 453 | 73 | 5 | 1084 | 447 |
| 08-03 | control | 1000 | 87 | 2 | 1087 | 447 | 77 | 12 | 1077 | 434 |
| 08-04 | control | 1000 | 85 | 4 | 1085 | 456 | 87 | 2 | 1087 | 447 |
| 08-05 | control | 992 | 87 | 10 | 1079 | 452 | 82 | 15 | 1074 | 446 |
| 08-06 | control | 1000 | 85 | 4 | 1085 | 456 | 87 | 2 | 1087 | 447 |
| 08-07 | control | 1000 | 84 | 5 | 1084 | 447 | 79 | 10 | 1079 | 432 |
| 08-08 | control | 1000 | 87 | 2 | 1087 | 447 | 77 | 12 | 1077 | 434 |
| 08-09 | control | 1004 | 82 | 3 | 1086 | 454 | 78 | 7 | 1082 | 447 |
| 08-10 | control | 1001 | 84 | 4 | 1085 | 455 | 80 | 8 | 1081 | 447 |
| 09-01 | control | 1011 | 73 | 5 | 1084 | 453 | 73 | 5 | 1084 | 447 |
| 09-05 | control | 993 | 92 | 4 | 1085 | 456 | 91 | 5 | 1084 | 448 |
| 09-07 | control | 1011 | 73 | 5 | 1084 | 453 | 73 | 5 | 1084 | 447 |
| 09-09 | control | 981 | 102 | 6 | 1083 | 454 | 95 | 13 | 1076 | 436 |
| 09-10 | control | 993 | 92 | 4 | 1085 | 456 | 91 | 5 | 1084 | 448 |
| 10-02 | focal | 728 | 360 | 1 | 1088 | 258 | 360 | 1 | 1088 | 142 |
| 10-09 | focal | 723 | 365 | 1 | 1088 | 258 | 364 | 2 | 1087 | 142 |
| 11-01 | focal | 734 | 355 | 0 | 1089 | 263 | 354 | 1 | 1088 | 154 |
| 11-02 | focal | 723 | 366 | 0 | 1089 | 259 | 366 | 0 | 1089 | 129 |
| 11-03 | focal | 729 | 360 | 0 | 1089 | 259 | 359 | 1 | 1088 | 143 |
| 11-04 | focal | 729 | 360 | 0 | 1089 | 259 | 359 | 1 | 1088 | 143 |
| 11-05 | focal | 709 | 380 | 0 | 1089 | 264 | 378 | 2 | 1087 | 126 |
| 11-06 | focal | 736 | 352 | 1 | 1088 | 263 | 350 | 3 | 1086 | 154 |
| 11-07 | focal | 712 | 375 | 2 | 1087 | 263 | 376 | 1 | 1088 | 125 |
| 11-08 | focal | 723 | 365 | 1 | 1088 | 258 | 364 | 2 | 1087 | 142 |
| 11-09 | focal | 728 | 360 | 1 | 1088 | 258 | 360 | 1 | 1088 | 142 |
| 11-10 | focal | 743 | 346 | 0 | 1089 | 271 | 346 | 0 | 1089 | 164 |
| 12-02 | focal | 734 | 355 | 0 | 1089 | 264 | 354 | 1 | 1088 | 155 |
| 12-03 | focal | 725 | 363 | 1 | 1088 | 258 | 360 | 4 | 1085 | 142 |
| 12-04 | focal | 717 | 372 | 0 | 1089 | 262 | 369 | 3 | 1086 | 132 |
| 12-05 | focal | 704 | 385 | 0 | 1089 | 268 | 383 | 2 | 1087 | 126 |
| 12-07 | focal | 711 | 378 | 0 | 1089 | 264 | 377 | 1 | 1088 | 126 |
| 12-08 | focal | 721 | 368 | 0 | 1089 | 261 | 367 | 1 | 1088 | 131 |
| 12-09 | focal | 740 | 349 | 0 | 1089 | 263 | 349 | 0 | 1089 | 154 |
| 13-02 | focal | 723 | 366 | 0 | 1089 | 258 | 364 | 2 | 1087 | 142 |
| 13-04 | focal | 723 | 366 | 0 | 1089 | 258 | 364 | 2 | 1087 | 142 |
| 13-06 | focal | 726 | 362 | 1 | 1088 | 259 | 359 | 4 | 1085 | 143 |
| 13-07 | focal | 731 | 357 | 1 | 1088 | 261 | 357 | 1 | 1088 | 145 |
| 13-10 | focal | 731 | 357 | 1 | 1088 | 261 | 357 | 1 | 1088 | 145 |
