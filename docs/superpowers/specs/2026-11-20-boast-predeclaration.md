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
