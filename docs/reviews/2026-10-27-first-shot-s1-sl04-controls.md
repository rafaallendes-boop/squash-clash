# S1 — protección y trazabilidad de controles, SL-S1-04

Base aprobada técnicamente por Claude:cecd74c7ea24756528bd815065d2ab58a4cde067. [Incidente anterior y restauración](2026-10-27-first-shot-s1-sling-iteration-2.md), [autorización del PO](2026-10-25-first-shot-po-approvals.md). Misma rama/PR25, sin fusionar. La causa de las dos activacionesP2 sigue sin confirmar; no atribuirla alPO ni a autoarranque.

## Alcance autorizado

Registrar controlesDevBar/DEV y precondiciones/resultados de armarB0–B5. BloquearEscenario/P2/H17 desde selección/preparaciónS1-sling y mientras bloque o respuesta activos, con protección también en el callback. Fuera de protocolo conservar funciones. Rechazo deB0–B5 persistente y visible con causa/remedio, sin simular bloque activo; preset/hash/pausa visibles antes deB0. Solo texto opcional deP2. No cambiarD2, sus parámetros, cancelación, defaults, criterios, spec ni plan.

O1/O2 deClaude se conservarán para análisis de datos nuevos, sin alterar modelo: abandonos con candidato quieto≥120ms y<3 muestras; muestras con timeReal<lockObservedAt recibidas después de fijar y desplazamiento desde posición fijada. No recalcular las sesiones históricas como datos nuevos.

## Verificación y sesión previstas

TDD con esqueleto compilable y RED por aserciones antes de implementar; regresión completa y exports dev/prod limpios antes de builddevReleaseARM64/noDevelopment. Respaldar3archivos; comprobar perfil incorporado vigente antes de instalar (actualvence2026-11-03T18:29:44Z). PrepararS1-sling temporal sin guardar, aplicar/relanzar/verificarhash/juego sin pausa y selección persistidaP0. El PO pulsaB0 y confirma cuenta atrás antes del primer gesto. B0completo120s/identificación fijación-zona/cancelación guiada precedeB1–B5/16preguntas. Si no se entiende, detener y registrar. RestaurarP0/nuevo arranque/hash/cero pendientes/tres archivos byte a byte. S1sin aceptar/S2–S4/T8sin autorizar.


## Implementación y verificación antes de exportar

Auditoría `dev-control` con fases ui-activation/activation/result: nombre interno, reloj real, preset/hash aplicado y hash del lanzamiento, pausa/foco/DEV/pending, aceptado y motivo. La fase activation se escribe antes de ejecutar; result distingue aceptación/rechazo. Los campos de edición y despliegue registran cambios/resultado sin copiar valores del portapapeles. No interpretar cada fase como otro intento ni atribuir por estos registros una causa a las activaciones anteriores.

Escenario/physics-spike/performance-session deshabilitados desde selecciónS1-sling (incluso pendiente), bloque o respuesta activos; callback también rechaza antes de cambiar pelota/fase. Funciones fuera de protocolo conservadas yP2 rotulado «Benchmark P2 (dev)» sin cambiar nombre interno. B0–B5 identifica pausa/foco/reanudación/DEV/gesto/control/pendientes/repetición/respuesta/bloque desconocido; aviso persistente con remedio fuera deDEV, sin falso temporizador de inicio. Antes deB0, línea muestra preset/hash/Listo o motivo de indisponibilidad. El núcleoD2 y su registro de gestos no cambian.

RED compilable inicial:9/9 fallos por aserción; GREEN posterior de esos9. Hubo dos referencias de fixture aSpikeSection sobreAppBootstrap, corregidas aDevBar sin cambiar aserciones. RED adicional de geometría de aviso trasB0 interrumpido:9Pass/1Fail por desborde →10/10. Regresión fresca **411/411 EditMode y194/194 PlayMode**, sinCS. Aviso contenido y fuera del frontis en1206×2622; geometría anterior, indicador y temporizador conservados. [Evidencia automatizada](../validation/evidence/2026-10-27-s1-sl04-controls-implementation.json). XML/logs privados separados enLogs/evidence/s1-sl04-controls.

TransitoriosGraphicsSettings/QualitySettings dePlayMode preservados como patch ignorado y restaurados antes de exportar;35 archivos protegidos cotejados iguales, incluidos modeloD2/settings/indicador/defaults/preset/spec/planes. Ningún criterio, umbral, banda o valor nuevo. Pendientes exports/check-release limpios, builddev/perfil/cotejo y sesión nueva según gate. Respaldo previo deP0/Po-1/last-active ya recuperado y coincidente con el anterior; no instalación nueva todavía.
