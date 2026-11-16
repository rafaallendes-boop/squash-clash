# E1 — derecho y revés estéticos (16 nov 2026)

**Estado: autorizado; tests preparados, pendientes de RED.** Rama `codex/e1-forehand-backhand`, worktree propio, base local `main` en `10bcaec`. Unity sigue reservado a U1–U3: el PO avisará cuando quede libre. No se ha abierto Unity, implementado, fusionado ni instalado.

## Alcance autorizado

El PO resolvió el límite de trayectoria: «Se autoriza que CharacterView dibuje su propia preparación y su propio remate para el derecho y el revés, con SwingView intacto (claves, trayectoria, tiempo y fase).» Autorización y condición completas en el [registro del PO](2026-10-25-first-shot-po-approvals.md#e1-derecho-y-revés-estéticos-16-nov-2026-chile).

La mano del humano sigue su elección cosmética; el rival es diestro fijo. La clasificación visual depende de la mano y del lado físico del contacto, sin modificar `BodyFrame`, planificación, gesto, reloj, timing, física, IA ni contratos. `StrokeFx` debe diferenciar el arco por lado sin ocultar la pelota. Sin colisiones nuevas.

## Ventana y tolerancia propuestas

**Centro:** el instante en que la cabeza de `SwingView` alcanza su clave `Contact`, a `SwingView.ReachSeconds` (60 ms) desde que recibe `PresentationDelivered` y entra en `Impact`. Es una referencia de presentación, no un cambio al contacto del juego.

**Ventana: ±60 ms respecto de esa clave**, equivalente a los primeros 120 ms desde la entrega. Incluye toda la aproximación y los primeros 60 ms del remate, dando margen de varios fotogramas a 30/60/120 Hz sin cambiar las duraciones existentes. **Tolerancia: 5 mm en espacio mundo**, suficientemente menor que la cabeza de la raqueta (escala lateral actual de 22 cm) para no admitir el retraso de suavizado ni los offsets actuales en el contacto. La implementación buscará coincidencia exacta, sin interpolación residual dentro de la ventana. Estos valores son propuestas predeclaradas; todavía no han sido medidos.

Los cuatro casos de `ZE1ContactWindowTests` cubren derecho y revés visuales con mano derecha e izquierda. Usan órdenes aceptadas y entregas reales del controlador; comparan los centros de los objetos `Racket` de la figura y de `SwingView` en espacio mundo en toda la ventana, incluidos inicio, centro y final. Cada caso prueba potencia 0,1/1 y pasos de 30/60/120 Hz y 50 ms. La elección zurda solo invierte el nombre del golpe visual, nunca el lado de Core.

## Evidencia y compuertas

Tres tests adicionales en `ZE1StrokePoseTests`: mano derecha fija del rival en ambos lados; remate de revés hacia afuera; elección humana independiente del rival y ausencia de colisiones. **Ningún test ejecutado; no se declara RED ni GREEN.** No se modifica runtime antes del RED válido por aserción.

Pendientes al liberar Unity: RED, implementación, tests de preparación/arco, EditMode y PlayMode completos, `git checkout -- ProjectSettings` después de PlayMode, `scripts/check-release.sh`, capturas de derecho y revés de ambos jugadores y revisión independiente de Claude coordinada por el PO. **AutoQA no es obligatorio dentro de este alcance**, conforme a la autorización: `SwingView`, gesto, timing y contratos permanecen intactos. Sin PR de entrega todavía; sin aceptación visual ni física.
