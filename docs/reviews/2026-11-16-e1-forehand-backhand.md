# E1 — derecho y revés estéticos (16 nov 2026)

**Estado: detenido antes de implementación; requiere resolver el límite de trayectoria.** Rama `codex/e1-forehand-backhand`, worktree propio, base local `main` en `10bcaec`. No se ha abierto Unity, fusionado ni instalado.

## Encargo y alcance

El PO autorizó el 16 nov: «Avanzaría en las mejoras como por ejemplo diferenciar el golpe de derecho y de reves (estetico)» y «Los encargos esteticos puedo lanzarlos a codex en paralelo.» El encargo E1 exige conservar trayectoria y tiempo de `SwingView` y ordena: «Si para el revés hiciera falta tocarlo, detente y reporta». Unity continúa reservado a U1–U3, según confirmación del PO en este chat.

## Hallazgo que exige decisión

`Runtime/Shot/SwingView.cs` define `Back.x = sign * .55` y `Follow.x = -sign * .3`; `SetSide` obtiene `sign` del lado del contacto. Tanto derecho como revés usan esas mismas claves (`OnAccepted` y `OnDelivered`). `Runtime/Visual/CharacterView.cs` sigue esa posición, suavizada y con pequeños offsets de preparación/remate. El humano mantiene la mano elegida; el rival usa la mano del lado del contacto.

Fijar la mano del rival y diferenciar hombros y efectos cabe en la capa visual. Sin embargo, obtener el recorrido humano solicitado de adentro hacia afuera exige cambiar las claves de `SwingView` o permitir que la raqueta dibujada siga una preparación y un remate distintos en `CharacterView`. Cambiar solo el torso o dibujar un arco distinto no acredita ese recorrido de la raqueta. Se detiene antes de realizar cualquiera de esas desviaciones.

**Propuesta mínima para decisión del PO:** permitir preparación y remate propios en `CharacterView`, conservando `SwingView` intacto y la posición y fase de contacto que entrega. La pose se clasificaría por lado físico del contacto y mano cosmética, sin modificar `BodyFrame`, planificación, gesto, reloj, timing, física, IA ni contratos. El rival sería diestro fijo. Esto es una propuesta, no una autorización recibida ni una solución validada.

## Trabajo preparado y compuertas pendientes

Se añadieron tres tests PlayMode en `ZE1StrokePoseTests`: mano derecha fija del rival en ambos lados; remate de revés hacia afuera; elección de mano humana independiente del rival y ausencia de colisiones. Usan la figura aislada sin avanzar el juego. **No ejecutados: no se declara RED ni GREEN.** La implementación espera un RED válido por aserción y la decisión anterior.

Pendientes: tests de preparación humana y arco, implementación, EditMode y PlayMode completos, restaurar `ProjectSettings` después de PlayMode, `scripts/check-release.sh`, capturas de derecho y revés de ambos jugadores y revisión independiente de Claude coordinada por el PO. AutoQA queda por determinar según el alcance finalmente autorizado; no se ha ejecutado. Sin PR de entrega todavía. No hay aceptación visual ni física.
