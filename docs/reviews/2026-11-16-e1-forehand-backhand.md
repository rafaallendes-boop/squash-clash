# E1 — derecho y revés estéticos (16 nov 2026)

**Estado: listo para revisión independiente; compuertas completas.** Rama `codex/e1-forehand-backhand`, base `main` en `10bcaec`; código visual en `8d0e1bb`, fixture de capturas en `decdc47`. Sin fusión ni instalación. Revisión independiente de Claude pendiente, coordinada por el PO.

## Cambio y límites

Humano y rival usan preparación cruzada y hombro girado para el revés; el remate abre hacia el lado del contacto. El derecho prepara detrás del lado de la mano y remata hacia el centro. Se respeta la mano elegida por el humano y el rival queda diestro fijo. `StrokeFx` añade dos arcos reutilizables, con sentidos de barrido distintos y separados del centro del contacto. La cara de las raquetas de las figuras queda abierta y su estela se oculta durante el contacto para despejar la pelota. Sin colliders nuevos.

La autorización del PO permite preparación y remate propios en `CharacterView`, con `SwingView` intacto. El PO aprobó la ventana y liberó Unity: «OK con ±60 ms y 5 mm; Unity Libre». Detalle en el [registro de aprobaciones](2026-10-25-first-shot-po-approvals.md#e1-derecho-y-revés-estéticos-16-nov-2026-chile).

**Ventana:** ±60 ms respecto de la llegada de `SwingView` a su clave `Contact` (60 ms después de la entrega), equivalente a los primeros 120 ms desde `PresentationDelivered`. Cubre toda la aproximación y los primeros 60 ms del remate. **Tolerancia:** 5 mm en espacio mundo, muy inferior al ancho de la cabeza; se copia la posición de referencia sin suavizado ni offsets en ese intervalo. Una edad local de presentación conserva el bloqueo un fotograma adicional si la entrega sucede después del Update de `SwingView`; no modifica el reloj del juego.

## Validación

- **RED por aserción:** 7/7 fallos previstos de contacto/mano/remate; después 4/4 por ausencia de los arcos. Sin errores CS. XML y logs preservados en `Logs/evidence/e1-red` y `e1-arc-red` (ignorados).
- **GREEN focalizado:** 11/11. Derecho y revés humanos, diestro y zurdo, potencias 0,1/1, pasos de 30/60/120 Hz y 50 ms: **288 muestras, separación máxima 0 m**, incluidos inicio, centro y final de ventana. Arcos: sentido, espacio libre, pausa, caducidad y reutilización. Rival: mano fija y remate hacia afuera.
- **Regresión completa:** EditMode **737/737**, PlayMode **559/559**, 0 omitidos. `git checkout -- ProjectSettings` ejecutado después de cada PlayMode y de las capturas. Evidencia en `Logs/evidence/e1-full`.
- **Capturas:** fixture independiente **1/1 Passed**; ocho PNG de cuatro golpes reales, inspeccionados. [Contacto/remate del humano: derecho](../validation/evidence/2026-11-16/e1-forehand-backhand/human-forehand-contact-follow.png), [revés](../validation/evidence/2026-11-16/e1-forehand-backhand/human-backhand-contact-follow.png); [del rival: derecho](../validation/evidence/2026-11-16/e1-forehand-backhand/rival-forehand-contact-follow.png), [revés](../validation/evidence/2026-11-16/e1-forehand-backhand/rival-backhand-contact-follow.png). [Pantallas completas y método](../validation/evidence/2026-11-16/e1-forehand-backhand/README.md).
- **Release:** `scripts/check-release.sh` **OK** en DEV y PROD, con exportaciones frescas y módulos esperados. Evidencia local en `Logs/evidence/release-{dev,prod}.json`.

**AutoQA exento** conforme a la autorización: `SwingView` (claves, trayectoria, tiempo y fase), Core/`BodyFrame`, planificación, gesto, reloj, timing, física, IA, defaults y contratos permanecen intactos. Las capturas de editor no acreditan aceptación visual en un teléfono ni aceptación física de ninguna etapa.
