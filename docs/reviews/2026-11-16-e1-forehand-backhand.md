# E1 — derecho y revés estéticos (16 nov 2026)

**Estado: correcciones listas para nueva revisión independiente; compuertas completas en verde.** PR #68, rama `codex/e1-forehand-backhand`; código y tests actuales en `03bbc7e`. Sin fusión ni instalación; nueva revisión de Claude pendiente, por medio del PO.

## Corrección y justificación

La cabeza humana sigue a `SwingView` en Idle e Intention: aceptar y entregar pueden ocurrir en el mismo ciclo, por lo que conservar un reposo separado no permite asegurar continuidad y contacto simultáneamente. La mano portadora sigue siendo la elegida por el humano y el torso conserva su preparación. El PO autorizó el reposo alineado y la actualización de las expectativas de `ZHandednessTests` y de altura durante Idle en `ZCharacterFeelTests`; se conservan sus comprobaciones de mano, agarre, preparación y split step ([registro](2026-10-25-first-shot-po-approvals.md#e1-continuidad-desde-idle-y-expectativas-de-reposo-16-nov-2026-chile)).

El remate propio pasa de **0,16 a 0,28 s**, sin limitar velocidad. Para una misma distancia, el pico de la curva smoothstep escala como `1,5 × distancia / duración`; alargarla reduce la causa geométrica del pico. En la población medida, solo este alargamiento bajó la salida de **10,45 a 6,15 m/s**. Una mezcla de retorno durante el hold, entre 280 y 480 ms desde la entrega, alinea la cabeza antes del regreso a Idle de `SwingView` (540 ms), evitando trasladar el salto al final.

Se conservan las otras correcciones: rival diestro fijo con el `side` del evento, igual que su arco; ventana cerrada si falta edad válida; escala humana constante; filtro de identidad/intención aceptada y consumo único de la entrega. `SwingView` procesa el evento primero; `CharacterView` comprueba identidad pendiente y su fase posterior antes de armar la ventana.

## Compuertas y evidencia

- **Criterios intactos:** entrada, diferencia vectorial de desplazamientos ≤5 mm; salida, desplazamiento ≤`8 m/s × dt + 5 mm`; contacto, ±60 ms respecto de la clave `Contact`, tolerancia 5 mm.
- **RED previo ampliado:** 4/4 fallos por aserción; **64 casos completos, 2.704 muestras**, sin cortar en la primera infracción. **GREEN focalizado: 20/20**; 64 casos, **3.120 muestras**, incluidos Idle → Intention, Intention → Impact, salida y regreso a Idle. Ambas manos/golpes, 30/60/120 Hz y 50 ms, preparación de 25/300 ms y potencia 0,1/1. Entrada añadida máxima **0 mm**; salida máxima **7,49 m/s**. [Máximos por caso RED/GREEN](../validation/evidence/2026-11-16/e1-forehand-backhand/README.md#máximos-por-caso).
- **Contacto:** 288 comparaciones, separación máxima **0 mm**. Regresión completa: **EditMode 737/737; PlayMode 568/568**, 0 omitidos. `ProjectSettings` restaurado tras las corridas. XML/logs originales ignorados en `Logs/evidence/e1-final-*`; historial de la detención conservado en `e1-review-*`.
- **Capturas nuevas:** 1/1 Passed en fixture separada; ocho PNG regenerados e inspeccionados. [Galería y método](../validation/evidence/2026-11-16/e1-forehand-backhand/README.md).
- **Release DEV/PROD:** `scripts/check-release.sh` **OK**, exportaciones frescas y módulos esperados. Evidencia en `Logs/evidence/e1-final-release`.

**AutoQA sigue exento:** `SwingView`, Core/`BodyFrame`, planificación, gesto, reloj, timing, física, IA, defaults y contratos permanecen intactos. Las capturas de editor no constituyen aceptación física ni evaluación en el teléfono.
