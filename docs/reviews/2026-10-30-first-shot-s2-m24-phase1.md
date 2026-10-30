# First Shot S2 fase1 — M2-4, tareas2.0–2.6

Base8dd142dff2fcdc5d52871ac3c89733b4aff88fd6, rama firstshot/s2-m24. AutorizaciónPO2026-10-30 en [registro](2026-10-25-first-shot-po-approvals.md); [plan/D4](../superpowers/plans/2026-10-25-squash-clash-first-shot.md). Trabajo en curso; no se declara completada ninguna tarea sin evidencia.

## Alcance y línea base

2.0primero/hashreales, luego2.1–2.6enorden/TDD. No Xcodebuild,instalación,acceso al iPhone,sesión2.7 ni prod físico. Bloques experimentales2.7pendientes dePO. TablaD4.2decisión aprobada, no propuesta;se conserva referencia histórica del plan y literal deaprobaciones. N04bcorregido solo como texto. CierrePR29 registrado aquí porremisión alregistro, sincommitaparte enmain.

Regresión inicial fresca:441/441EditMode y207/207PlayMode, sinCS/ceroomitidos. TransitoriosGraphicsSettings/QualitySettings restaurados byte a byte antes de implementar. Artefactos ignorados enLogs/evidence/s2-tdd, archivos y comienzo de ejecución verificados por scripts/test.sh. EntornoUnity preparado con caché local previa, pero XML/log de cada ejecución nuevos;sinreutilizar resultados anteriores.

## Progreso

2.0: RED observado antes de cambiar defaults; restante resultado por registrar.2.1–2.6pendientes. Ante defecto o incompatibilidad de vuelo se detiene,sinajustarventanas/parámetros/SL.


## 2.0 — promoción de M2-4

RED válido:6testsEditMode/3fallos por aserción (valor por defecto, delta de valores, overlayP0) y1PlayMode/1fallo por ausencia deM24 enMain. GREEN6/6y1/1 tras cambiar exclusivamente gesture.control0→2 enDefaultPreset. FixturesA′seleccionan0explícitamente yS1-sling1, sin debilitar aserciones; expectativashash revisadas por promoción autorizada. Suites completas frescas447/447EditMode/208/208PlayMode,sinCS;transitoriosGraphics/Quality restaurados byte a byte. [Resultados y SHA de XML](../validation/evidence/2026-10-30-s2-task20.json).

85claves; defaults **1c8e598b099b**, P0legacy41claves **944668168805**, calculados porUnityenlos tests, no observados en iPhone ni instalados. P0/Po-1/last-active de un almacén temporal de prueba permanecen byte a byte iguales; no se accede al dispositivo ni se reescriben archivos del PO.41heredados/proyección0d39e6016bd8, P0histórico8de3f2ecb217 ySimulationKey166a96e319c7 intactos.2/35mm/N2estricto intactos. La app instalada anterior no cambia por esta promoción en la rama.


## 2.1 — mapa del reloj por lanzamiento

Esqueleto compilable/RED9/9 fallidos por aserción, luego RED12/12 al cubrir observaciones de frames, descarte explícito y paso manual pausado. GREEN12/12; suite completa EditMode459/459 sinCS. Primera implementación11/12 por un ulp bajo el entero24 después de pausa: se conservó la aserción, normalizando solo error de representación double, no ms de juego. [Evidencia](../validation/evidence/2026-10-30-s2-task21.json).

ClockMap observa el SimClock existente, no lo avanza. Segmentos reales/simulados en double; pendientes futuros extrapolados con tempo vigente, muestras anteriores al lanzamiento rechazadas. Pausa conserva posición; avance manual no se interpola como continuo. Como decisión técnica de interfaz, Mark admite un indicador opcional de pasos descartados del SimClock (llamadas originales de seis argumentos conservadas); el intervalo descartado es desconocido/no mapeable, no se deduce de diferencias menores entre reloj real y observación de frame. Captura de τ/cámara lenta/pausa invalida el tramo de gesto correspondiente. Ninguna integración de host2.5 aún.

## 2.2 — predeclaración de recetas iniciales, sin resultados calculados

El plan/spec dan las seis categorías SL pero no sus coordenadas numéricas. Se fijan estas recetas iniciales **[EXPERIMENTO]** para implementar esos casos, antes de ejecutar su comprobación; no se derivan de L1–L6 ni se reajustarán para hacer pasar el alcance o el tiempo. Velocidad de la tabla = componente horizontal de la entrada, a convertir con Ballistics.SolveCenterTo existente, no velocidad total de la salida2.4. Radio físico0,02m/gravedad/tempo permanecen los de configuración. Objetivo del frontis usa su centro físico desplazado por radio como ya hace Ballistics.CenterForContact.

| SL / categoría | Apoyo de pelota inicial (x,y,z) m | Objetivo frontis (x,y) m | Velocidad horizontal m/s | Cuerpo fijo (x,z) m |
|---|---|---|---|---|
| SL1 media altura drive | (2,6;1,2;7) | (2,6;2,2) | 15 | (3,2;5,5) |
| SL2 media altura revés | (3,8;1,2;7) | (3,8;2,2) | 15 | (3,2;5,5) |
| SL3 esquina trasera drive | (0,35;1,3;7) | (0,35;1,5) | 18 | (1,3;8,3) |
| SL4 esquina trasera revés | (6,05;1,3;7) | (6,05;1,5) | 18 | (5,1;8,3) |
| SL5 adelante drive | (2,6;1;3) | (2,6;0,7) | 6 | (3,2;1,6) |
| SL6 adelante revés | (3,8;1;3) | (3,8;0,7) | 6 | (3,2;1,6) |

Son valores iniciales de experimentación, no criterios de aceptación. Posición/punto de alcance se capturan al lanzar. Interpretación de implementación de M04: alcance euclídeo3D desde el centro del cuerpo de la cápsula existente (y0,9m de PlaceholderPlayers), independiente de la banda de altura preferida. Mano del personaje diestra; con AX-1 drive al lado de x menor que el cuerpo, empate exactamente sobre eje→drive, sin nueva franja de umbral. U suma altura preferida y separación de paredes con pesos iniciales iguales **[EXPERIMENTO]**, desempate por paso menor; no bonifica alcance ni añade estocada. Si estas concreciones o los vuelos revelan una incompatibilidad, registrar/detener y elevar al PO, sin ajustar las recetas congeladas.
