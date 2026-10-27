# Diseño y plan propuestos para el control §6.2 B

Fecha: 2026-10-27. Rama `firstshot/s1-control-b-design`, base main **188d59ec3bd5eb59b0340e9342e8a47afc94bdef**, tras cierre autorizado de PR #23 (head aprobado **f5b4beae3aa796c3c324efaed198ebe3f840c6b7**).

**Documento para decisión del PO, no implementación.** El PO eligió investigar el **control §6.2 B**, “apuntar, fijar y tensar”; A′ se conserva como referencia y por defecto. El **método de potencia B** por velocidad es otra cosa: sigue experimental/desactivado por defecto. Ninguna recomendación de Codex de este documento equivale a una decisión del PO. Todos los valores propuestos son **[EXPERIMENTO], no definitivos ni aplicados**. S1 sin aceptar; S2–S4/T8 no autorizados.

Fuente normativa: [spec v0.3 §6.2](../superpowers/specs/2026-10-20-squash-clash-design-v0.3.md), [First Shot r2 §4.2/C5/§11.4/M06](../superpowers/specs/2026-10-24-first-shot-design.md), [plan First Shot r2 T1.2](../superpowers/plans/2026-10-25-squash-clash-first-shot.md), [decisión del PO y propuesta de mecánica](2026-10-27-first-shot-s1-gesture-mechanics-proposal.md). Solo se citan; no se editan. Los resultados previos de PR #22 se conservan: riesgo físico del método de potencia B NotEvaluated, medios3/10, saturación30 % y juicio del PO de que A′ falla en su forma actual; no se convierte en Fail medido de calibración.

## Predeclaración del análisis offline descriptivo

**Se registra y se commitea antes de calcular resultados.** Fuentes inmutables: [138 gestos de revalidación n.º2](../validation/evidence/2026-10-25-s1-revalidation-2.json) y [49 de calibración](../validation/evidence/2026-10-27-s1-gesture-calibration-physical.json). Se incluyen todos los intentos, series, incidentales y cancelaciones: no se selecciona por potencia ni éxito. No se supone que ambos conjuntos tengan iguales configuración, mano o propósito.

Se estimará compatibilidad de pausas, no una tasa de fijación involuntaria ni rendimiento del control §6.2 B. Datos registrados con A′, escala DPI reportada, sin nueva verificación física. **No es medición nueva ni simulación del control elegido.**

Método predeclarado, sin ajuste posterior:

- Orden de llegada de rawSamples; mmPerPx original congelado por gesto. Solo Began/Moved/Stationary pueden sostener la ventana; Ended/Canceled no completan una quietud ni se cruzan límites de gesto. No se generan muestras por frame, no se interpola ni se prolonga quietud a través del silencio.
- Ventana anclada en una muestra real: duración entre sus extremos **≥120 ms**, al menos3 timestamps distintos y todos los intervalos positivos **≤51 ms**. Límites inclusivos, tolerancia aritmética1e−9 s/1e−6 mm o mm/s para representación numérica, no cambio del umbral. Registros no finitos, tiempos hacia atrás o dt=0 contradictorio rompen la ventana; un duplicado Stationary idéntico se ignora sin sumar tiempo/muestra.
- **Principal QVR:** cada velocidad entre muestras ≤15 mm/s **y** cada posición a≤1 mm del ancla. **Comparador QV, ya declarado:** velocidad≤15 mm/s con el mismo apoyo temporal, sin radio. Duración120/radio1/velocidad15/hueco51 son valores analíticos exploratorios, no parámetros aprobados. No se probarán otros tras conocer el resultado.
- Un gesto cuenta como “compatible” si existe al menos una ventana; se publica la primera pareja de índices/timestamps, duración y máximos para trazabilidad, sin sumar ventanas solapadas como múltiples personas/intentos. No compatible significa “sin ventana observada con este apoyo”, no “nunca pausó”. Se informan muestras ausentes o inválidas separadas; no se eliminan intentos del denominador.
- Dos ámbitos: **interior completo del gesto** (puede incluir potencia después de fijar en A′; no prueba que se estuviera apuntando) y **apuntado observable**. En este último, cortar antes del FreezeTimeReal nativo si se conserva para Flick; si no hubo fijación antes de soltar (None/Release), usar el interior completo. Si hubo Flick y no se conserva ese timestamp, excluir solo de este ámbito y declarar su falta de observabilidad; no reconstruirlo mediante replay de AimModel. La fecha nativa del cruce puede anteceder a la confirmación: el corte es conservador.
- Presentar por fuente y ámbito: denominador total/observable, gestos compatibles con QVR/QV y porcentaje, falta de escala/muestras/timestamp de fijación. En el resultado por intento conservar hash, protocolo y outcome; no hacer equivalencia entre fuentes. No inferir intención ni causalidad. Si no aparece ninguna ventana, se registra0 sin relajar el filtro.

La tabla agregada se incorporará aquí. Se declara un único JSON derivado nuevo, `docs/validation/evidence/2026-10-27-s1-control-b-quiet-analysis.json`, con hashes de fuentes, predeclaración, resultados por intento y agregados. Los originales quedan intactos; el script analítico transitorio no forma parte del producto ni se versiona.

Estado de esta predeclaración: **resultados todavía no calculados**. A continuación se completará el diseño documental, con opciones/recomendaciones para decisión del PO, manteniendo esta predeclaración y el resultado separados.
