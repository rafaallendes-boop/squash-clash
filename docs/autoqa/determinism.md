# Predeclaración de determinismo — 9 de noviembre de 2026

Documento escrito y comprometido **antes de la POC ±10 ms**. No ampliar exclusiones ni tolerancias después de observar resultados. Mismos criterios para POC y futuro MVP; nuevas observaciones fuera del esquema fallan cerradas.

## Expectativas y frontera

`M24-PERFECT-EARLY10-SL5`: Δ=−10 ms, Perfect/Applied/Scheduled=true. `M24-PERFECT-LATE10-SL5`: Δ=+10 ms, Perfect/Applied/Scheduled=false. Tres instancias frescas por cada uno. Canario: gesto EARLY10 con esperado Failed. Otros cuatro escenarios intactos. SL1 esperado `c0d8ec7050e0`; SL5 `55a85f500cf1`; SimulationKey `166a96e319c7`.

Lectura de ContactContract previa al código: ±10 está dentro de ±40; ambos quedan dentro de KIn/KOut en SL5; corrected<=target da Scheduled true en −10 y false en +10. No se llama al contrato desde AutoQA. Si el juego lo contradice se detiene. Primer contacto de la predicción saliente debe ser FrontWall a ≤0,05 m de TargetUsed y PredictionMatchesBitwise debe ser true en ambos; sin evaluar la trayectoria posterior.

## Campos del ShotRecord observado

El JSON original se preserva completo. En la comparación se conservan **exactamente**, sin tolerancia ni redondeo, estos campos escalares (incluidos todos los booleanos, geometría y métricas de salto):

`Schema, Version, Seed, PlanVersion, Pending, O2Count, FingerId, StepsResimulated, ObsoleteEffects, LaunchId, ContactStep, TStarStep, GestureOutcome, Reason, Result, Fail, LaunchHash, GestureHash, Preset, FileHash, Build, Side, HasJumpPixels, HasRelease, Scheduled, EarlyGeometry, UsedNeighborLegalStep, QuantizationAboveThirdOfA, ValidLimitOn, EarlySoft, Applied, SpeedRaised, TargetLowered, KInSteps, KOutSteps, AssistMs, ValidMs, LatencyCompMs, Power, AimX, AimY, ContactX, ContactY, ContactZ, TargetUsedX, TargetUsedY, Speed, VelocityX, VelocityY, VelocityZ, JumpMeters, JumpPixels, JumpBallDiameters`.

Solo estos tres campos relativos admiten diferencia absoluta **≤10⁻⁶ ms** entre repeticiones: `DeltaMs, QuantizationErrorMsSigned, OrderAgeMs`. Se comprueban además finitud y misma estructura. No se redondean resultados para decidir categorías.

Se excluyen exactamente del ShotRecord:

- `AttemptId, PredecessorId`: identidad GUID del juego, diferente por instancia; se conserva y coteja por intento con el JSONL/presentación, no se inventa.
- `BeganAtReal, ReleaseEventAtReal, ReleaseCorrectedAtReal, InputReceivedAtReal, ProcessingAtReal, PlanPresentedAtReal, TargetContactAtReal, ProjectionObservedAt, LockObservedAt`: origen absoluto del reloj Editor. Para no perder el timing se derivan los relativos de la lista siguiente; PlanPresentedAtReal queda excluido porque se toma del reloj de pared real y no del virtual.
- `PlannerCostMs, SolverCostMs, ReconstructionCostMs, PredictionCostMs, ReconciliationCostMs`: costes de máquina, ajenos al resultado funcional.

Se derivan y comparan con **≤10⁻⁶ ms**, respecto de TargetContactAtReal: BeganAtReal, ReleaseEventAtReal, ReleaseCorrectedAtReal, InputReceivedAtReal, ProcessingAtReal, ProjectionObservedAt y LockObservedAt (resta ×1000). Además `(shot-plan.kInReal−targetReal)*1000` y `(shot-plan.kOutReal−targetReal)*1000`, exclusivamente para validar margen. No deciden la categoría esperada. Las magnitudes KInSteps/KOutSteps originales se comparan exactamente.

## Observables adicionales

Comparación exacta de: cierre del `m24-gesture`, identidad launchId por instancia, correlación de AttemptId (booleana), cantidad de presentaciones, categoría y paso de presentación, discrepancias, AttemptConsumed, LastClip, PerfectCount/ValidCount/SwishCount/ContactCount, PredictionMatchesBitwise, hash efectivo/gesto/SimulationKey/backend, pending, pantalla/área segura/DPI/mmPerPx/UI bloqueada, estado final Host.Current (posición/velocidad/paso), primer contacto de Host.Prediction (Surface/Point/Step) y distancia geométrica a TargetUsed. Sin tolerancia de geometría.

En el resumen normalizado estos observables exactos se serializan de forma estable y se comparan completos. Los tiempos relativos se guardan por separado con su lista ordenada y se comparan numéricamente con la tolerancia declarada. No se exige igualdad de SHA a los campos temporales tolerados; se exige la comparación numérica. La huella exacta identifica solo la parte exacta.

Envoltura/metadata excluida de la comparación funcional, conservada en evidencia: `ts` (UTC de escritura), rutas de artefactos, SHA-256 del JSONL bruto, id de corrida/repetición y marcas build de la ejecución. Los ids de intento quedan fuera de observado normalizado, pero se verifican las correlaciones dentro de la instancia. Los rawSamples y matrices de m24 se preservan en el JSONL; la comparación de determinismo aquí se aplica al observado definido arriba, no a metadata de frame como projectionFrame, al coste ni al reloj absoluto de cada muestra.

Ningún otro campo está tolerado o excluido. Cambios de esquema desconocidos deben fallar, no desaparecer. Esta predeclaración adopta las dos propuestas de Claude con la precisión de una comparación exacta semántica y numérica temporal; no es permiso para cambiar criterios de producto.
