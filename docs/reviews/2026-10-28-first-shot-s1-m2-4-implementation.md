# M2-4 — implementación experimental y sesión

Base403d213, rama firstshot/s1-m2-4, autorización PO2026-10-28. [Diseño aprobado](2026-10-28-first-shot-s1-m2-4-design.md); [cierre PR27 y autorización](2026-10-25-first-shot-po-approvals.md). S1 sin aceptar/S2–S4/T8 no autorizados.

## Preparación 1.2c-0

Preparación declarada, no RED. Selector propuesto2 [EXPERIMENTO], sin claves nuevas ni valores de defaults nuevos; A′=0 y sling=1 conservados. Fixtures sintéticos de cámara/proyección y muestras usan parámetros heredados2/35mm y N2 3mm/250ms. Los casos negativos N01–N13/N04b se escribirán antes de cada implementación: mapeo, potencia/cierres, contexto/tiempos, métricas/cadenas/O2, vista/protocolo. Ninguna traza anterior se reproduce ni representa intención M2-4. Riesgo accidental/deslizamiento no se filtra ni se declara resuelto.

Pruebas de proyección usan cámara conocida y matriz capturada al procesar Began, sin cambiar cámara del producto. Valores de posiciones/tiempos sintéticos son fixtures, no parámetros experimentales ni mediciones. Evidencia XML/log RED/GREEN se conserva en Logs/evidence/m24-tdd, con resúmenes versionados al cerrar.

## 1.2c-1 — proyección absoluta

RED compilable 1/8→GREEN8/8; mapeo de rayo con matrices capturadas, AX-1, tin/out mapeable inválido y rechazos con motivo. Primer intento de compilación del fixture faltaba un using: no cuenta como RED, log conservado. El fixture singular usó finalmente un viewport explícito: Camera.pixelRect dependía del tamaño headless; no cambió ningún criterio de producto. Selector2 disponible, A′0/defaults85/hash45c4e9af5c8f sin cambios de valores ni claves.

## 1.2c-2 — potencia y terminales

RED0/15 por aserción→GREEN15/15. Dos correcciones del fixture antes del GREEN: invocar explícitamente el constructor opcional de SlingSettings (un struct sin argumentos queda a cero), y matriz ortográfica con eje correcto hacia el frontis. Sin cambios de parámetros de producto. Distancia actual2/35/N2estricto/C-A/invalid prioritario; N04 y N04b conservan posibilidad de Swing no buscado, sin filtro ficticio. No hay impulso de pelota.

## 1.2c-3 — tiempo y contacto

RED16/21→GREEN21/21: Ended idéntico conserva P/r y cierra una vez; tiempo contradictorio/no finito conservador sin Swing, cancelación de contexto anterior a invalidez y sin r inventado, secundarios/duplicados sin reanclar. Integración con contexto real se verificará también en PlayMode de la siguiente etapa.

## 1.2c-4 — integración, cadenas y O2

RED PlayMode0/5→GREEN5/5. Snapshot al Began/instante observado/matrices crudas/O2, resultado y error separados, nuevo id por reapoyo/contexto rompe cadena; pausa/foco/DEV/relanzamiento/config invalidan sin r. Host.Current y Prediction idénticos en el test de integración. Compilación inicial del fixture necesitó calificar UnityEngine.Camera por choque con namespace Runtime.Camera: no cuenta como RED. Muestras originales y recepción conservadas, sin muestras fabricadas. No se reproduce evidencia histórica.

## 1.2c-5 — vista/protocolo

RED6/8 más geometría0/1→GREEN9/9. B0/reloj/guía C-A, B1 sin carga con Tap/abandono esperados, tres respuestas con discriminador y premature=null en fallo, terminal visible, tin/out rojo sin flash, protección SL04 para control2. Geometría nativa1206×2622 confirma botones/guía/status fuera del frontis; vista previa sin ayuda adicional. S1-sling conserva texto/instrumento bool históricos.

## Fronteras adicionales de 1.2c-3/4

RED7/11→GREEN11/11: foco sin contacto rompe vínculo, viewport invalida también durante despacho (antes de Tick), eventos UI filtrados y secundarios auditados sin crear intentos. Se conserva un único input. Timestamp recibido capturado al entrar; proyección y fijación tienen sus instantes observados separados, O2 usa ambos tiempos de cada muestra real. La comparación del timestamp sintético del fixture usa tolerancia1e-9 por representación binaria del Input System; ningún umbral temporal de producto cambia. Se conserva el XML RED, que también mostraba ese redondeo.

## Comparabilidad del contacto invalidado

RED0/1→GREEN1/1: un cambio de configuración durante un intento armado conserva el registro y marca comparableToArmedBlock=false; no reinterpreta con parámetros nuevos ni sustituye el intento. Los cierres por sistema se conservan sin Swing.

## Regresión y preparación para release

Fuente **ae1b423**: regresión fresca **441/441 EditMode**, **207/207 PlayMode**, sin errores CS ni tests omitidos. [Resumen de ejecuciones y hashes de XML](../validation/evidence/2026-10-28-m24-tdd.json); XML/log originales ignorados en Logs/evidence/m24-tdd. Los dos cambios transitorios de PlayMode (temperatura de luz y antialiasing) se restauraron byte a byte antes de exportar. Spec, propuesta r2, plan, EffectiveConfig.cs, SlingModel.cs y defaults se cotejaron contra403d213; 183 archivos de evidencia histórica intactos.

85 claves/defaults **45c4e9af5c8f**, sin claves nuevas; selector experimental2. Preset separado **S1-M2-4 / 1c8e598b099b**; P0 extendido esperado **e458bfd5ce9e**, pendiente observar arranque de la build. 41 heredados/proyección0d39e6016bd8 y SimulationKey166a96e319c7 pasan sus pruebas originales. Parámetros2/35mm y N2 3mm/250ms inalterados; A′=0 sigue por defecto, S1-sling sin promover. N04/N04b no se declaran resueltos.

Respaldo nuevo antes de instalación: P0 SHA2562f1295f201e238d9494bee285dd3f46e5bbb0f98b09ccde90fbd268f0874e4fd; Po-1 9008c33ab89f82e669bc227a81b37ed641f69e4657ee70df6c89ff0c0edb1767; last-active5885ce8a7add389b2c82cb43f6944d7e2ef7ebeb0bffd77b5b635361a8a46087. Archivos coinciden con el registro anterior, selección P0; copia local ignorada en Logs/evidence/m24-session/backup-before-install. Instalación y sesión pendientes, no hay mediciones nuevas aún.
