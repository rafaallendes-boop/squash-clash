# U3 — métricas mínimas para usuarios

16 nov 2026 · Codex · `codex/u3-user-metrics`, base `a1fa915` de `origin/main`, código `5d90951`. **Nueva PlayMode autorizada; PR en borrador y compuertas pendientes.** U1 se entrega por separado en #65; esta rama no incorpora su cambio de defaults.

## Implementación

GUID aleatorio persistente en `persistentDataPath/metrics/install-id.txt`, incluido como **installId en cada línea JSONL**: se usa el sobre existente de cada registro, sin inventar una cabecera. Sobrevive al cambio/eliminación de logs; retención intacta porque la identidad no coincide con el patrón de logs. Sin nombre de dispositivo, IDFA ni datos personales nuevos; misma implementación sin defines en dev/prod.

| Evento | Registro |
| --- | --- |
| app-session | start/background/foreground/end; duración de fase y acumulada; deduplica callbacks focus/pause del SO |
| menu | pantalla y acción, incluidas navegación y botones existentes |
| tutorial-step | paso, duración, completado/omitido; los pasos restantes al saltar quedan omitidos con duración cero |
| match-start / match-end | GUID de partido, dificultad/mano capturadas al inicio, resultado, marcador, duración, abandono; cierre único |
| pause | player/interruption, sin cambiar cómo se pausa o reanuda |

Duraciones en tiempo real desde el arranque; incluyen pausa/segundo plano, desglosados por app-session. No se añade UI. El director de rally se observa con sus eventos existentes, sin modificarlo ni duplicar `rally-point`. `session-start`/`session-summary` existentes son las sesiones DEV de PerformanceSession y se conservan. Defaults, física, solver, gesto, contrato, reloj y retención no se modifican. **AutoQA no corresponde**: solo registro, sin cambiar lógica de timing/gesto/contacto.

## Verificación y parada

- RED EditMode: **0/10 por aserción**; RED de integración PlayMode: **0/3 por aserción**, XML frescos y sin errores CS.
- GREEN EditMode completo: **747/747**, cero omitidos/errores CS, incluidos identidad dev/prod, persistencia, eventos y retención existente.
- PlayMode completo: **550/551**, XML fresco y sin errores CS. Los **tres flujos nuevos pasan**: menú → tutorial → partido → fin; omisión/abandono; interrupción y deduplicación.
- Único fallo: test existente **ReticleVisibilityTests.LocalCancelLimitAndState_AreVisibleAndNeverCoverFrontis**, línea **186**: `preview.ArmSlingBlock("B0")`, **True esperado / False observado**. El test no se modificó. No es el caso CRP-01 clasificado por el PO; **causa no demostrada**, sin atribuirlo al entorno o intermitencia.
- Se registra y se detiene según AGENTS §2 y el encargo del PO de detenerse ante otra falla. **Sin corrección ni reintento. Check-release no ejecutado.**

[Evidencia curada y hashes](../validation/evidence/2026-11-16-u3-summary.json); logs/XML ignorados en `Logs/evidence/u3/`. ProjectSettings restaurado después del cierre completo de cada PlayMode; ningún editor Unity activo al entregar. Sin build nativa, acceso al iPhone, instalación ni fusión. Revisión independiente de Claude coordinada por el PO. U2 sigue sin iniciar por esta parada.

**Continuación autorizada por el PO:** una sola PlayMode completa nueva, sin código/test modificado. Si pasa completa, se registra LocalCancelLimitAndState como intermitente **sin causa demostrada**, separado de CRP-01, y se ejecuta check-release. Si falla, detenerse. La clasificación aún está condicionada al resultado; la evidencia 550/551 se conserva. Literal en el registro de aprobaciones.
