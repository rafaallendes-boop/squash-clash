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
