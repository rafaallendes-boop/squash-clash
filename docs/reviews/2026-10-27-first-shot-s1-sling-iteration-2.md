# S1-sling, iteración 2 — adenda de diseño y predeclaración del replay

PO2026-10-27, misma rama y PR #25, base revisada **508f625982c7998f13f315cc8f7ea993f40c5c13**. Claude confirma entrega técnica y parada SL-S1-01 correctas, restauración P0 verificada y SL-S1-02 registrada. **SL-S1-03 es un defecto del diseño aprobado, responsabilidad reconocida por Claude**: el detector exigía eventos durante la quietud. No se reescribe el diseño anterior ni se presenta como error de cumplimiento de db72a00. [Diseño aprobado](2026-10-27-first-shot-s1-sling-prototype-interaction.md), [informe histórico](2026-10-27-first-shot-s1-sling-prototype.md), [registro PO](2026-10-25-first-shot-po-approvals.md).

## D2 — decisión del PO

Con contacto primario activo y sin Ended/Canceled, el reloj real cuenta la quietud desde el candidato anclado en la última posición válida. Tick por frame evalúa fijación sin añadir TouchSample. Se conservan120 ms/1 mm/15 mm/s/arrastre≥3 mm/área útil/mínimo3 muestras reales. Se conserva el mínimo por candidato de quietud del código anterior: Tick no incrementa ese contador. El hueco51 ms deja de reiniciar el candidato con contacto activo; la clave anterior permanece para compatibilidad de presets/hash, sin servir de veto al nuevo detector. Nueva muestra fuera de radio o velocidad reinicia; pausa/foco/DEV invalidan antes del Tick. Ended/Canceled procesados no pueden fijar por reloj. A′ intacto.

Registrar origen sample/timer, instante teórico de elegibilidad, instante del frame/observación y última muestra real usada. El instante teórico es el mayor entre quietAt+120 ms y el timestamp de la muestra real que completa el mínimo requerido. No se inventan muestras ni se afirma que el frame ocurrió en el instante teórico. Fijación única por gesto; potencia sigue desde el punto fijado.

Indicación local de cancelación reconocible respecto del dedo: dirección, límite y estado dentro/fuera; nunca intersectar el frontis. Dimensiones12/20/8 intactas. Guía en pantalla de una cancelación enB0 antes de finalizar. Temporizador visible, cuenta atrás B0 y fin/interrupción explícitos. Son cambios autorizados, sin promover defaults ni modificar umbrales/criterios.

## Replay diagnóstico predeclarado, antes de implementar

Fuente única [33 intentos originales de B0 db72a00](../validation/evidence/2026-10-27-s1-sling-B0.json), inmutable. Reproducir los silencios/velocidades citados por Claude y aplicar D2 con mínimo3 real por candidato. Todos los intentos, incluido Tap y preview, permanecen; nada se reemplaza.

Dos lecturas temporales, declaradas **antes de calcular**: (1) elegibilidad teórica ideal usando timestamps originales, procesando una muestra terminal antes de un temporizador coincidente; (2) grid sintético60 Hz con fase en Began y eventos procesados antes de Tick al coincidir. Este grid es una hipótesis de replay, **no frecuencia real del iPhone ni sus frames observados**. No se interpola movimiento: entre eventos se conserva la última posición reportada mientras el contacto siga activo. Tiempo de recepción se conserva en las fuentes, pero el replay principal usa tiempo del evento y no reproduce la cola del player.

Comparar instante/posición/fijación con el registro original. Señalar fijaciones nuevas/anticipadas y movimiento posterior que podría mostrar fijación involuntaria: distancia hasta el objetivo final libre calculado con la misma ganancia, descenso posterior y proximidad de nueva muestra que rompe la quietud. **Sin umbral de intención ni clasificar como involuntaria sin juicio del PO.** La cifra aproximada de Claude (~5/13, mediana~0,6s) es orientativa, no un objetivo que el auditor deba alcanzar ni medición.

Después: TDD con RED por aserciones, regresión completa, check-release dev/prod y build dev Release ARM64/noDevelopment. **No instalar hasta renovación previa del perfil por el PO y comprobación efectiva del incorporado**, además de respaldo/cotejo de los tres archivos. Sesión nueva separada: B0 completo120s/con guía; identificación antes de B1–B5/16preguntas. Restaurar P0 y cotejar al cerrar. S1 sin aceptar, S2–S4/T8 sin autorizar; PR sin fusionar.
