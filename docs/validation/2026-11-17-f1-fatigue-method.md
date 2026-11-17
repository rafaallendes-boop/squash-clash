# F1 — método predeclarado del experimento B

17 nov 2026. Fuente: spec de cansancio v2.2, §§3–5 y 7–8; encargo completo `Logs/handoff/2026-11-16/codex-F1-F2-cansancio.md`. Línea base: main `766dedd`, con A1 de G1; A2 de G1 no se incorpora. Este documento se confirma en un commit propio antes de ejecutar el experimento.

## Muestra y ejecución

Experimento EditMode explícito, separado de las suites de regresión: 200 partidos completos por variante, a 7 con diferencia de 2, sin tope de marcador. Bot humano: `BotBrain`, σ de timing 30 ms, paralelo/medio/potencia media; Rival: `FondoAi` Media. `RallyParams` de main, sin cambios. Cada partido usa cerebros nuevos: semilla Bot `20261117 + 2*i`, Rival `20261118 + 2*i`, i=0…199. Primer servidor humano para i par y rival para i impar. No se reutiliza el estado aleatorio entre partidos. Energía 100 al iniciar cada partido.

Variantes: (1) main antes de implementar, referencia sin cansancio; (2) F1 con `fatigue.enabled=0`, identidad bit a bit frente a (1); (3) F1 con todos los valores suaves de la spec. La referencia de main usa el preset intacto de 121 claves. Se conservan datos por partido, por punto y por recepción, con ganador, marcador previo, servidor, pasos, golpes y duración. La identidad compara los campos preexistentes del resultado, los registros de recepción y posiciones/velocidades finales de ambos cuerpos mediante una representación binaria canónica y SHA-256 por punto. No incluye los campos nuevos de cansancio. Se conserva el código de captura desde antes de implementar; la comparación se convierte en test de regresión con la referencia congelada.

Con F1 se registran por jugador E inicial, E al cierre antes de recuperación entre puntos y E después, metros acumulados, consumo atribuible al desplazamiento sin truncarlo al llegar a cero, gasto por golpes fuertes y recuperación efectiva quieto/entre puntos; f_r por recepción. El saque automático cuenta como golpe fuerte solo si su intención supera strongFrom; se usa la P real de la intención del saque existente, sin cambiarla. Los golpes de B1/B2 son `PointRules.Shots` (contactos de raqueta, como en main); no se añade el saque al contador existente.

## Cálculo y decisiones

Mediana convencional: valor central, o promedio de los dos centrales. No se excluyen partidos, puntos ni recepciones. Un partido abortado, un resultado distinto de punto terminal, un SystemFault o un contacto rechazado se conserva y detiene el experimento; no se completa la muestra seleccionando otras semillas. Una población vacía se informa NotEvaluated, nunca Pass.

- B1: en puntos de ≤4 golpes, mediana del gasto neto `E inicial − E al cierre antes de recoverBetween`, por jugador por separado, ≤4.
- B2: en puntos de ≥15 golpes, por punto se ordenan los dos consumos `costPerMeter × metros`. Mediana del mayor ≥8 y mediana de la diferencia ≥4. No se cuentan golpes ni recuperación.
- B3: mediana conjunta de ambas E iniciales del último punto de cada partido en [45,75]; ≥50% de partidos con alguna E bajo onset en cualquier paso (se conserva mínimo por punto).
- B4: ≤5% de partidos con alguna E inicial de punto <20; cero saques con E=0.
- B5a: puntos cuyo marcador previo tiene ambos jugadores ≥6 (incluidas extensiones): mediana conjunta de ambas E iniciales ≥35.
- B5b: solo indicador. En puntos ajustados de F1 con diferencia inicial ≥20, se identifica al jugador más cansado. Se compara su porcentaje de victorias con el del mismo jugador en el mismo índice de punto y partido de la referencia sin cansancio, solo si ese punto existe y también es ajustado. Se informan ambos denominadores, pares ausentes, victorias y diferencia en puntos porcentuales. Sin umbral de aprobación.
- B6: promedio de duración real de todos los puntos F1 respecto de todos los puntos de referencia (mismas 200 semillas), razón en [0,85;1,15]. Se incluyen los puntos que difieren entre variantes.

Único ajuste permitido por §7: si falla B3 por energía demasiado alta, y B4/B5a no fallan por energía baja, bajar recoverBetween de 0,35 a 0,25; si falla B3 por energía baja o B4/B5a por energía baja, subir recoverBetween a 0,45. Si las señales son contradictorias, no ajustar y reportar. No se cambia costPerMeter ni otra clave. Se registra el motivo antes de ejecutar la única variante ajustada con las mismas semillas. Si persiste la falla, se conserva el resultado y se reporta al PO; no hay búsqueda ni más calibración. B1/B2/B6 fallidos se reportan sin ajustar por ellos.

## Evidencia y límites

Cada invocación conserva inventario de Unity (PID, comando, proyecto), XML fresco, logs, head/árbol, versión Unity, hashes de defaults/SimulationKey/RallyParams, CSV/TSV y SHA-256. Un solo Editor Unity; no se inicia si ya hay alguno. Sin fusión, instalación ni acceso al iPhone. Tras PlayMode se restaura ProjectSettings.

El PO confirmó el 17 nov la autonomía experimental del encargo: se permite corregir únicamente código propio de Tests/Experiments o scripts si aborta antes de datos, conservando intentos, y resumen/empaquetado después de datos con SHA-256 idénticos. No cubre producto, método, grilla, umbrales ni datos producidos; cualquier otra sorpresa se registra y detiene. No se alteran escenarios ni oráculos de AutoQA. Solo sus cuatro anclas, en commit propio y después de probar la proyección exacta sobre las 121 claves anteriores.
