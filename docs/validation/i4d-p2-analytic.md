# T4.5 / I4d — validación P2

Estado: implementación completada, regresión GREEN, dos experimentos completos; build dev 557d3ff instalada y verificada; campaña nativa pendiente. Rama `firstball/i4d-p2-analytic`, PR de física propia; sin merge ni selección de backend. I4c cerrado con merge ac5e195 y precisión literal de paredes aprobada por PO a propuesta de Claude. I5 no autorizado.

- [Informe comparativo parcial](../reviews/2026-10-23-first-ball-physics-spike-P2.md).
- [TDD: once RED válidos y suites 221/221 EditMode, 61/61 PlayMode](evidence/2026-10-23-i4d-p2-tests.json).
- [Experimentos 1/1 EditMode y 1/1 PlayMode, 18 criterios y 200 muestras cada uno](evidence/2026-10-23-i4d-p2-editor-runs.json).

En editor pasan los criterios de gate aplicables. C17 informativo; no se extrapola al iPhone. Entrada experimental Editor-only llama al mismo experimento y el primer intento cero tests fue rechazado. Física/mediciones idénticas entre las dos ejecuciones; después solo cambió el prefijo del archivo de DevPanel.

Siguiente: build dev actual, export limpio Release ARM64 firmado/metadata, comprobación prod y perfil vigente; una campaña física bajo condiciones aprobadas con encabezado fastMotionThreshold (solo P1) y 200 muestras C17 persistidas. Auditar archivo completo y completar comparación P1/P2; detenerse para Claude/PO. Mantener escenarios/criterios/umbrales/preset/spec y backend definitivo pendiente.

## Instalación nativa

[Preflight](2026-10-23-i4d-native-preflight.md) y [build saneada](evidence/2026-10-23-i4d-native-build.json). Dev 557d3ff instalada, Release ARM64 firmada, metadata y perfil vigentes verificados; prod limpio sin puente nativo. Lanzamiento rechazado por teléfono bloqueado, apertura manual PO pendiente. Encabezado y 200 muestras implementados; auditoría de su salida nativa pendiente de la campaña, sin herramientas durante ejecución. PR #6 borrador, sin fusionar.
