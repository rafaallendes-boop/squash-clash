# U3 — métricas mínimas para usuarios

17 nov 2026 · Codex · `codex/u3-user-metrics`, main integrado **`47ec6d1`** (U1 #65 y E1 #68), validación **`3597a82`**. **Lista para revisión; sin fusionar.**

GUID aleatorio persistente en `persistentDataPath/metrics/install-id.txt`, como **installId en cada línea JSONL** del sobre existente. Sobrevive a la retención de logs; retención intacta. Sin nombre del dispositivo, IDFA ni datos personales nuevos; misma implementación dev/prod.

| Evento | Datos |
| --- | --- |
| app-session | start/background/foreground/end; duración de fase/acumulada; focus/pause sin duplicación |
| menu | pantalla y acción de navegación existente |
| tutorial-step | paso, duración, completado/omitido; restantes al saltar omitidos con duración cero |
| match-start / match-end | GUID de partido, dificultad/mano al inicio, resultado, marcador, duración, abandono; cierre único |
| pause | player/interruption; pausa/reanudación intactas |

Duraciones reales incluyen pausas/segundo plano, desglosados por app-session. Sin UI nueva; se observan eventos existentes del director. `rally-point` no se duplica y las sesiones DEV existentes se conservan. U3 no cambia defaults, física, solver, gesto, contrato, reloj ni retención. **AutoQA no corresponde:** registro solamente; la integración conserva la exención visual de E1 y sus contratos.

- RED originales: EditMode **0/10**, integración PlayMode **0/3**, por aserción, XML frescos y sin errores CS.
- **Tras integrar E1:** EditMode completo **748/748**, PlayMode completo **581/581**, cero omitidos/errores CS, XML frescos. Incluye los flujos menú → tutorial → partido → fin, omisión/abandono e interrupción y la regresión visual E1.
- **check-release DEV/PROD OK** sobre `3597a82`, exportaciones frescas, `gitDirty=false`, DevPanel solo en dev. ProjectSettings restaurado y árbol limpio. Inventarios PID/comando/proyecto guardados antes de cada lanzamiento y al terminar: cero procesos; un Unity a la vez, sin incidente.

Se conservan las validaciones anteriores (747/747, 551/551; tras U1, 748/748 y 561/561) y el fallo original **550/551** de LocalCancelLimitAndState, línea 186, ArmSlingBlock(B0) False frente a True. Por decisión del PO, tras el recuento completo autorizado se registró como **intermitente sin causa demostrada, distinto de CRP-01**; no se corrige el test ni se reinterpreta esa corrida.

Integración de E1 autorizada para preparar una sola prueba física: ambos bloques de aprobaciones conservados completos y AGENTS §6 actualizado con #68 fusionada. Worktree U1 ya retirado sin --force, ignorados preservados fuera de Git. **#69 espera a que #66 esté fusionada** para integrar main y repetir sus compuertas; sus dos observaciones de release ya están documentadas sin cambio de código. Sin build nativa, instalación, iPhone, archive, firma, subida ni fusión.

[Evidencia, procedencia y hashes](../validation/evidence/2026-11-16-u3-summary.json), [historia de la parada](../validation/2026-11-16-u3-playmode-stop.md). Logs/XML/inventarios ignorados en `Logs/evidence/u3/`. Revisión independiente y fusión coordinadas por el PO.
