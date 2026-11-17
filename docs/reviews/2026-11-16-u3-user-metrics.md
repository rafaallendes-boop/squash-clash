# U3 — métricas mínimas para usuarios

16 nov 2026 · Codex · `codex/u3-user-metrics`, main integrado `02f3c57`, código U3 `5d90951`, integración validada `aa6fb08`. **Integración E1 en validación; sin fusionar.** U1 #65 y E1 #68 ya están en main.

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
- Tras integrar U1/T3: EditMode completo **748/748**, PlayMode **561/561**, cero omitidos/errores CS y XML frescos. Los tres flujos nuevos pasan: menú → tutorial → partido → fin, omisión/abandono e interrupción. Antes de la integración: 747/747 y única PlayMode nueva autorizada 551/551.
- `check-release.sh` **OK dev/prod**, exportaciones frescas sobre `aa6fb08`, `gitDirty=false`; DevPanel solo en dev. Validación anterior sobre `bda8bb5` conservada en la evidencia.

Se conserva el fallo previo **550/551**: LocalCancelLimitAndState, línea 186, ArmSlingBlock(B0) False frente a True. Tras el recuento completo autorizado, **por decisión del PO se trata como intermitente sin causa demostrada**, separado de CRP-01. No hubo corrección del test o producto ni otro reintento.

[Evidencia y hashes](../validation/evidence/2026-11-16-u3-summary.json), [historia de la parada/decisión](../validation/2026-11-16-u3-playmode-stop.md). Logs/XML ignorados en `Logs/evidence/u3/`. ProjectSettings restaurado tras PlayMode; ningún Unity activo al entregar. Sin build nativa, iPhone, instalación ni fusión. Revisión independiente de Claude y fusión coordinadas por el PO; U2 sigue después.

**Continuación tras fusionar #65:** main `02f3c57` integrado; ambos bloques de aprobaciones íntegros, hashes T3 ahora efectivos bajo U3. AGENTS §5/§6 actualizados y worktree U1 archivado sin --force, con todos sus ignorados preservados fuera de Git (recibo local en `Logs/evidence/u3/u1-retirement.json`). Compuertas completas nuevas aprobadas con T3. E1 terminó y declaró Unity libre antes de PlayMode; un chequeo previo había detectado su editor y devolvió el control sin iniciar tests. Inventarios guardados antes de cada lanzamiento, sin procesos al lanzar y al terminar. #69 sigue después de esta entrega; ninguna fusión autorizada aquí.

**Continuación del 17 nov tras E1:** se integra main `47ec6d1` conservando completos ambos bloques del registro (E1 y U3). AGENTS §6 actualizado. EditMode/PlayMode/check-release completos nuevos pendientes; las validaciones anteriores conservan su procedencia. Inventario PID/comando/proyecto guardado antes de cada lanzamiento, sin iniciar si hay Unity activo. #69 espera a que #66 esté fusionada. Sin build nativa, instalación, iPhone, archive, firma ni subida.
