# Precisión del criterio de altura de L2

Estado 2026-10-24: corrección implementada y verificada en `firstball/l2-height-criterion`, [PR #12 lista para revisión de Claude](https://github.com/rafaallendes-boop/squash-clash/pull/12), abierta y sin fusionar. No inicia Etapa 7. H1/Etapa 6 conservan su cierre.

## Decisión aprobada

El PO respondió **«Apruebo correguir L2»** a la observación de Claude tras I6b. El literal anterior `y ≤ 1,0 m en el primer bote` era siempre cierto porque en el contacto con el suelo y=r. Se sustituye en T6.3 del plan r2 por **`y ≤ 1,0 m entre el contacto con el frontis y el primer bote`**.

Se mantiene el umbral **1,0 m** y la condición lateral: x≤0,30 m durante ≥50 % de los pasos después del frontis, hasta el segundo bote. No se cambia spec, lanzamiento L2, física/backend, DefaultPreset ni otros criterios. Esta decisión específica sustituye las anotaciones históricas de L2 pendiente; no autoriza Etapa 7 ni gameplay.

## Medición y TDD

Solo cambia `LaunchPresetsPhysicsTests.cs` en Assets. La prueba mide la altura máxima del centro en el intervalo con extremos incluidos. Usa los instantes de contacto exactos de P2 (Step/TimeInStep), los estados y los contactos intermedios. Entre nodos consecutivos sin colisión, comprueba también el vértice balístico desde la velocidad vertical entrante del extremo final; incluye el vértice únicamente si pertenece al segmento. Así se detecta un máximo entre ticks aunque todos los estados muestreados estén bajo 1 m. Contactos simultáneos preceden al estado posterior de igual instante; no se extrapola a antes del frontis ni a después del primer bote. No epsilon ni tolerancia añadida al límite.

RED válido antes de reemplazar la medición: **8 tests, 6 Pass y 2 Fail**, XML fresco y sin CS. Se extrajo el literal anterior y fallaron dos contraejemplos:

- Trayectoria P2 real con objetivo de frontis a 1,2 m: el estado del primer bote estaba a 0,03086284 m; la comprobación anterior la aceptaba aunque el tramo alcanzaba más de 1 m.
- Intervalo vertical de referencia con vértice entre muestras: todos los estados/endpoints están bajo 1 m, pero el máximo interior supera el límite. La medición anterior devolvía 0,03380758 m, la altura después del primer bote.

GREEN del grupo **8/8** al implementar la medición. Regresión final **269/269 EditMode y 132/132 PlayMode**, cero fallos/omitidos, XML frescos y compilación sin errores. Dos tests nuevos protegen esos contraejemplos. La geometría horizontal de la referencia se completó antes de las suites finales, conservando el mismo intervalo vertical y la prueba de máximo entre ticks. Sin ejecuciones rechazadas ni XML viejo aceptado.

Fuente probada **ddf73c82855afd2fa7b12f0719eaa10bb180a986**, 119 hashes cotejados con la última ejecución. [Evidencia con RED/GREEN y resultados](../validation/evidence/2026-10-24-l2-height-criterion.json).

## Resultado de L2 existente

P2 y DefaultPreset **0d39e6016bd8**:

| Medida | Resultado | Criterio |
|---|---:|---:|
| Máxima altura frontis→primer bote | **0,35000011797003433 m** | ≤1,0 m |
| Pasos postfrontis con x≤0,30 m | **100 %** | ≥50 % |

**Pass sin reajustar el lanzamiento.** Inicio (0,3; 0,35; 4), objetivo frontis (0,3; 0,35) y velocidad horizontal 18 m/s idénticos a I6b. L1 y L3–L6 también pasan sus condiciones originales.

Solo plan, prueba y documentación cambian. Runtime/Core/DevPanel, scripts, ProjectSettings y spec se cotejan sin cambios respecto a main de partida `cf28daa`. No build ni instalación: la app instalada 2109af8 y sus parámetros permanecen iguales. Perfil vence 2026-10-28T14:59:31Z; renovación antes de instalaciones desde el 27 y vigencia siempre comprobada.

Se entrega para Claude y se detiene **sin fusionar**, sin campaña, repetición de H1, cambios de presets ni ejecución de Etapa 7/gameplay. El drive/lets/strokes siguen como comentarios futuros del PO.
