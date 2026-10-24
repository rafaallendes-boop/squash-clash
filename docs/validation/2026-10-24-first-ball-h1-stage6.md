# H1 — punto de control de Etapa 6

Estado **pendiente del juicio del PO**, autorizado el 2026-10-24 tras aprobación/cierre de I6b. No es ejecución de T7.2 ni autorización de Etapa 7. No sesión de rendimiento, campaña física, cambios de spec/plan ni criterios nuevos.

## Base y trazabilidad

- iPhone físico, vertical; build instalada **0.1.0+2109af8 dev**, fuente de I6b aprobada. Sin nueva instalación.
- P2-Analytic por defecto, DefaultPreset **0d39e6016bd8**, 41 claves intactas. Preset aceptado de Etapa 5 **P0-calib-1**, hash **8de3f2ecb217**, difiere solo en repetición=3 s.
- Para ver cada vuelo completo, repetición temporal 0 sin guardar; selector L1–L6 configura lanzamiento/figuras. Fija y seguimiento, con ayudas/escala del preset aceptado (escala visual 1, sombra/estela/marcas activas, predicción apagada). No ajustar ayudas mientras se puntúa un caso; si se cambia algo, declararlo para registrar la configuración.
- Preparación completada por el agente mediante Duplicación/cua_repl: label 2109af8, header P0-calib-1/P2 y cero pendientes; Mode=0 fija, repetición temporal 0, DEV cerrado y L1 seleccionado. Registro de lanzamiento **2026-10-24T22:11:50.5457970Z**, hash efectivo temporal **943ff1ebe8c5**, cero pendientes. Los JSON de P0-calib-1/Po-1 y last-active coinciden con el respaldo anterior, sin guardar ajustes. [Evidencia de preparación](evidence/2026-10-24-h1-stage6-setup.json).
- Recorrido y formato solicitados al PO para observar directamente en el teléfono. Resultado perceptual todavía sin completar; Codex recuperará los datos y restaurará el preset después de su respuesta, sin operar durante su observación.

## Recorrido y resultado solicitado

La fuente del punto de control es la Etapa 6 del plan r2 y §12.3/§17.B de la spec v0.3: **el PO distingue altura y profundidad sin abrir el panel en L1–L6, con al menos una de las dos cámaras**. La spec pide iPhone real y ambas manos; registrar diferencias por mano si las hay. La valoración 1–5 organiza comentarios y no añade un umbral de aceptación.

1. Observar directamente en el teléfono, sin mirar Duplicación para juzgar legibilidad. L1–L6 desde el selector de la barra; Repetir permite ver de nuevo el caso. Mantener DEV cerrado durante cada vuelo.
2. Primera pasada fija. Después abrir DEV solo para Cámara→Mode→seguimiento, cerrar y repetir L1–L6. Con ambas manos según §12.3; anotar cualquier diferencia.
3. Por caso/cámara: altura sí/no, profundidad sí/no, si alguna ayuda tapa al rival sí/no, valoración 1–5 y comentario. Anotar también si una ayuda parece imprescindible para interpretar un rebote; sombra sigue experimental e independiente de escala visual.
4. Enviar resultados y conclusión propia (cámara preferida, ajustes que se necesitan). Codex registra, sin sustituir el juicio del PO por capturas o tests. Restaurar P0-calib-1 mediante Cargar/Aplicar al terminar, sin guardar ajustes temporales.

| Caso | Cámara | Altura sí/no | Profundidad sí/no | Ayuda tapa rival sí/no | 1–5 | Comentarios / manos |
|---|---|---|---|---|---|---|
| L1 Drive largo | fija | Pendiente | Pendiente | Pendiente | — | — |
| L2 Baja lateral | fija | Pendiente | Pendiente | Pendiente | — | — |
| L3 Drop corto | fija | Pendiente | Pendiente | Pendiente | — | — |
| L4 Globo alto | fija | Pendiente | Pendiente | Pendiente | — | — |
| L5 Detrás del humano | fija | Pendiente | Pendiente | Pendiente | — | — |
| L6 Esquina trasera | fija | Pendiente | Pendiente | Pendiente | — | — |
| L1 Drive largo | seguimiento | Pendiente | Pendiente | Pendiente | — | — |
| L2 Baja lateral | seguimiento | Pendiente | Pendiente | Pendiente | — | — |
| L3 Drop corto | seguimiento | Pendiente | Pendiente | Pendiente | — | — |
| L4 Globo alto | seguimiento | Pendiente | Pendiente | Pendiente | — | — |
| L5 Detrás del humano | seguimiento | Pendiente | Pendiente | Pendiente | — | — |
| L6 Esquina trasera | seguimiento | Pendiente | Pendiente | Pendiente | — | — |

**Resultado H1:** pendiente. **Cierre de Etapa 6:** pendiente. **Etapa 7:** no autorizada. No promediar valoraciones ni convertir respuestas ausentes en Pass.

## Observación L2 pendiente

Claude observó que el literal «y ≤ 1,0 m en el primer bote» siempre se cumple porque y=r; intención probable: altura ≤1 m desde el contacto con el frontis hasta el primer bote. PO decidirá antes de Etapa 7. Se conserva el literal aprobado, sin cambiar código ni plan, y H1 no resuelve esa decisión por sí solo.

## Referencias

[Plan r2 — Etapa 6](../superpowers/plans/2026-10-20-squash-clash-first-ball.md), [spec v0.3 — §12.3 y §17.B](../superpowers/specs/2026-10-20-squash-clash-design-v0.3.md), [informe I6b](../reviews/2026-10-24-first-ball-i6b-legibility-launches.md), [aprobaciones del PO](../reviews/2026-10-21-first-ball-po-approvals.md).
