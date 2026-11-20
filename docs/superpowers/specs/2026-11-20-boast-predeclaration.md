# Golpe de dos paredes (boast): diseño y metas fijadas antes de los datos — 20 nov 2026

Decisión del PO, literal: «El gesto es apuntar a la pared lateral. Solo 1 Gesto. autorizo calcular el golpe. Lo usen todos los jugadores.» Antecedentes: [análisis](2026-11-20-two-wall-shot-analysis.md). Diseño y metas fijados por Claude como product lead antes de implementar; implementa un subagente dirigido por Claude, así que **la revisión de Claude no es independiente**. Rama `claude/boast`, base `5088723` (integración #84).

## Diseño

1. **Gesto (M2-4, el mismo):** si el rayo del toque no corta la frontal pero sí una pared lateral visible (x=0 o x=Width, 0 < z ≤ mitad de la cancha, y entre la tin proyectada y la línea de fuera lateral), el apuntado es un **punto de la pared lateral**. Hoy esos toques se rechazan (`outside-front-wall`), así que ningún apuntado actual a la frontal cambia.
2. **Cálculo (solver, autorizado):** modo nuevo que lanza hacia el punto lateral con la potencia del gesto y **verifica con la física P2** la secuencia raqueta → lateral → frontal, sobre la tin con margen y bajo la línea de fuera, sin bote antes de la frontal. Si el punto exacto no da un boast válido, busca el válido más cercano (altura y profundidad en la lateral, dentro de la banda de potencia), igual que el apuntado a la frontal ayuda hoy. Si no existe ninguno, el gesto se trata como un apuntado inválido, con el mismo camino que ya existe para la tin.
3. **Retícula:** marca el punto lateral y, como segundo punto, dónde llegará a la frontal.
4. **Reglas:** en juego abierto ya es legal; en el saque la lateral primero es falta, así que en el saque no se admite el apuntado lateral. La volea admite el boast. La protección «Drive adelante» no se aplica al boast.
5. **Rival:** línea nueva `Boast` en sus opciones, sobre todo desde el cuarto trasero; el Definidor la usa más, el Fondista menos. Campos nuevos en RallyParams, así que cambia su hash.
6. **Interruptor DEV «Dos paredes»:** controla solo al rival. El gesto humano queda siempre activo, porque es aditivo.

## Metas (fijadas antes de los datos)

- **M1, cálculo:** en una grilla de contactos del cuarto trasero (cerca de cada lateral) y apuntados a la lateral cercana con potencia normal, ≥ 90 % produce un boast válido verificado por P2; en ≥ 80 % de esos, la frontal se toca en la mitad contraria a la del contacto.
- **M2, gesto:** en una grilla de toques sobre la frontal, el apuntado es idéntico bit a bit al de hoy; los toques sobre la lateral visible dan un apuntado lateral.
- **M3, rival:** desde el cuarto trasero, el Definidor elige el boast en 5–15 % de sus golpes y el Fondista en ≤ 5 %; el Completo queda entre ambos.
- **M4, calibración:** con el rival usando el boast, los puntos ganados por el bot principiante en cada dificultad cambian ≤ ±3 pp. Si se excede, el interruptor «Dos paredes» queda **apagado** por defecto, sin recalibrar.
- **M5, sin cambios:** `DefaultPreset.json`, defaults `207da8f2c636`/132, SimulationKey `166a96e319c7`, física P2, contrato de contacto y timing intactos; AutoQA V, T3 y volea PASS/0/1 con los mismos escenarios; EditMode y PlayMode completos en verde.

Ante un defecto o una sorpresa: registrar y detenerse. No se cambian estas metas después de ver los datos; si alguna falla, se informa como desviación.

## Adenda 1 (20 nov, tras la primera vuelta, antes de nuevos datos)

Resultado de la primera vuelta (se conserva en [el informe](../../reviews/2026-11-20-boast.md)): M1b 39,2 % y M4 +7,9/+5,7/+1,3 pp, ambas desviaciones; interruptor del rival apagado. Las dos causas están en el diseño, no en las metas:

1. **Área del gesto.** Limitar el apuntado a la mitad delantera de la lateral impide el boast real desde atrás (0/288 cruzan). **Cambio:** se admite toda la lateral visible, entre el suelo y la línea de fuera lateral, en toda la profundidad. Sigue sin admitirse en el saque.
2. **Boast sin solución.** Hoy cierra el punto como falta del sistema y la pelota no se golpea, lo que debilita al rival (de 40 a 1.152 faltas en Fácil) y frustraría al jugador. **Cambio:** el boast se resuelve en el contacto real; si no hay boast válido, el golpe sale hacia la frontal por el mismo lado con el camino de apuntado frontal que ya existe (no es falta), para el jugador y el rival. La retícula muestra el golpe que de verdad sale.

Las metas M1–M5 **no cambian**. M1b se mide en la grilla de apuntados que el gesto admite ahora; M4 se vuelve a medir con semillas nuevas. El interruptor del rival sigue la regla de M4.
