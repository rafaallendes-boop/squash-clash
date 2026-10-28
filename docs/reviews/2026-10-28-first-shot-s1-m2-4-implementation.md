# M2-4 — implementación experimental y sesión

Base403d213, rama firstshot/s1-m2-4, autorización PO2026-10-28. [Diseño aprobado](2026-10-28-first-shot-s1-m2-4-design.md); [cierre PR27 y autorización](2026-10-25-first-shot-po-approvals.md). S1 sin aceptar/S2–S4/T8 no autorizados.

## Preparación 1.2c-0

Preparación declarada, no RED. Selector propuesto2 [EXPERIMENTO], sin claves nuevas ni valores de defaults nuevos; A′=0 y sling=1 conservados. Fixtures sintéticos de cámara/proyección y muestras usan parámetros heredados2/35mm y N2 3mm/250ms. Los casos negativos N01–N13/N04b se escribirán antes de cada implementación: mapeo, potencia/cierres, contexto/tiempos, métricas/cadenas/O2, vista/protocolo. Ninguna traza anterior se reproduce ni representa intención M2-4. Riesgo accidental/deslizamiento no se filtra ni se declara resuelto.

Pruebas de proyección usan cámara conocida y matriz capturada al procesar Began, sin cambiar cámara del producto. Valores de posiciones/tiempos sintéticos son fixtures, no parámetros experimentales ni mediciones. Evidencia XML/log RED/GREEN se conserva en Logs/evidence/m24-tdd, con resúmenes versionados al cerrar.

## 1.2c-1 — proyección absoluta

RED compilable 1/8→GREEN8/8; mapeo de rayo con matrices capturadas, AX-1, tin/out mapeable inválido y rechazos con motivo. Primer intento de compilación del fixture faltaba un using: no cuenta como RED, log conservado. El fixture singular usó finalmente un viewport explícito: Camera.pixelRect dependía del tamaño headless; no cambió ningún criterio de producto. Selector2 disponible, A′0/defaults85/hash45c4e9af5c8f sin cambios de valores ni claves.
