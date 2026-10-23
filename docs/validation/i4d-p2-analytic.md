# T4.5 / I4d — validación P2

Estado, 2026-10-23: **entrega técnica completa para revisión de Claude y decisión del PO; I4d abierto**. Rama `firstball/i4d-p2-analytic`, [PR #6 borrador sin fusionar](https://github.com/rafaallendes-boop/squash-clash/pull/6). I4c cerrado con merge ac5e195 y precisión literal de paredes aprobada por PO a propuesta de Claude. Backend definitivo pendiente, P1 respaldo; I5 no autorizado.

- [Informe comparativo completo de los 18 criterios](../reviews/2026-10-23-first-ball-physics-spike-P2.md).
- [TDD: once RED válidos y suites 221/221 EditMode, 61/61 PlayMode](evidence/2026-10-23-i4d-p2-tests.json).
- [Experimentos 1/1 EditMode y 1/1 PlayMode, 18 criterios y 200 muestras cada uno](evidence/2026-10-23-i4d-p2-editor-runs.json).
- [Única campaña nativa completa, auditoría y condiciones](evidence/2026-10-23-i4d-p2-native-run.json), [informe íntegro](evidence/physics-spike-P2/iphone-557d3ff-P2-20261023-204715-409.md).
- [Preflight y resultado](2026-10-23-i4d-native-preflight.md), [build saneada](evidence/2026-10-23-i4d-native-build.json).

En editor pasan los criterios de gate aplicables. C17 allí es informativo, sin extrapolación ni comparación con nativo. Entrada experimental Editor-only llama al mismo experimento y el primer intento cero tests fue rechazado. Física/mediciones idénticas entre los experimentos y la build dev 557d3ff; después solo cambió el prefijo del archivo de DevPanel y documentación.

## Campaña nativa

Dev **0.1.0+557d3ff** instalada tras export limpio Release ARM64 firmado, metadata y perfil vigentes verificados; prod nuevo sin DevPanel/puente nativo, herramientas intactas. El intento de lanzamiento por herramientas falló por Locked; apertura manual del PO comprobada por el informe completo de la build correcta. Sin nueva instalación ni repetición de campaña.

El PO confirmó **80 % de batería inicial y todas las condiciones solicitadas**: teléfono físico, No molestar, bajo consumo apagado, sin funda y sin capturas/duplicación/herramientas durante ejecución. Sin herramientas del agente hasta aviso de término. Archivo P2-20261023-204715-409 recuperado después de resolver conexión, **18 criterios únicos, hash c86243b15a97, variante analytic/r2-20261023, 10.053 lanzamientos**. **Térmico nominal inicial/benchmark/final y cero interrupciones de foco**. ProjectPhysics imprime fastMotionThreshold=0.01, marcado solo relevante P1, y bounceThreshold efectivo P2=0.1.

**Los 11 criterios de gate nativos pasan:** C1–C8, C10, C11 y C17. **C17 Pass, p95 0,011400 ms ≤1 ms**, mediana 0,008600 ms, p99 0,127200 ms, máximo 0,158700 ms. Veinte calentamientos y 200 muestras persistidas; mediana y percentiles nearest rank recalculados y coincidentes con agregados. C3 0/1.701; C10 máximo 0,0009536743 mm; C11 0/6. C14 tope 4 configurable, cero veces alcanzado. C9/C12–C16/C18 siguen informativos. Diferencias informativas editor/nativo preservadas, sin exigir identidad entre plataformas.

Las observaciones menores de I4b (encabezado fastMotionThreshold y persistencia de las 200 muestras C17) quedan resueltas en esta campaña; I4b no se repitió. **Se detiene para Claude/PO**, sin cerrar I4d, fusionar PR #6, elegir backend, congelar variantes, cambiar escenarios/criterios/umbrales/preset/spec o iniciar I5.
