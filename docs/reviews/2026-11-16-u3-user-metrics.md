# U3 — métricas mínimas para usuarios

16 nov 2026 · Codex · `codex/u3-user-metrics`, base main `a1fa915`, código `5d90951`. **Lista para revisión independiente; sin fusionar.** Rama independiente de U1 (#65).

GUID aleatorio persistente en `persistentDataPath/metrics/install-id.txt`, como **installId en cada línea JSONL**. Se usa el sobre existente por registro, sin nueva cabecera. La identidad sobrevive al borrado/retención de logs porque no coincide con su patrón; retención intacta. Sin nombre de dispositivo, IDFA ni datos personales nuevos; misma implementación sin defines en dev/prod.

| Evento | Datos |
| --- | --- |
| app-session | start/background/foreground/end; duración de fase/acumulada; combina focus/pause para no duplicar transiciones |
| menu | pantalla y acción de botones/navegación existentes |
| tutorial-step | paso, duración, completado/omitido; pasos restantes al saltar quedan omitidos con duración cero |
| match-start / match-end | GUID de partido, dificultad/mano capturadas al inicio, resultado, marcador, duración, abandono; cierre único |
| pause | player/interruption; comportamiento de pausa/reanudación intacto |

Duraciones reales incluyen pausas/segundo plano, desglosados por app-session. Sin UI nueva. Se observan eventos existentes del director, que no cambia; `rally-point` no se duplica. `session-start`/`session-summary` de PerformanceSession DEV se conservan. Defaults, física, solver, gesto, contrato, reloj y retención intactos. **AutoQA no corresponde**: solo registro, sin cambios de lógica de timing/gesto/contacto.

- RED EditMode **0/10** y RED de integración PlayMode **0/3**, por aserción, XML frescos y sin errores CS.
- EditMode completo **747/747**; PlayMode nueva autorizada **551/551**, cero omitidos/errores CS. Los tres flujos nuevos pasan: menú → tutorial → partido → fin, omisión/abandono e interrupción.
- `check-release.sh` **OK dev/prod**, exportaciones frescas sobre `bda8bb5`, `gitDirty=false`; DevPanel solo en dev.

Se conserva el fallo previo **550/551**: LocalCancelLimitAndState, línea 186, ArmSlingBlock(B0) False frente a True. Tras el recuento completo autorizado, **por decisión del PO se trata como intermitente sin causa demostrada**, separado de CRP-01. No hubo corrección del test o producto ni otro reintento.

[Evidencia y hashes](../validation/evidence/2026-11-16-u3-summary.json), [historia de la parada/decisión](../validation/2026-11-16-u3-playmode-stop.md). Logs/XML ignorados en `Logs/evidence/u3/`. ProjectSettings restaurado tras PlayMode; ningún Unity activo al entregar. Sin build nativa, iPhone, instalación ni fusión. Revisión independiente de Claude y fusión coordinadas por el PO; U2 sigue después.
