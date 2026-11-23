# RAL-174 — camino B: F1 cumple, F2 falla; #101 en borrador

23 nov 2026 · Codex · Código medido **ae9829c7289ac8229a996e05a5fd15f5b2ec3530**, confirmado head remoto de #101 al medir. Base única incorporada **0a49b6a3ff8ae481ad48f13ed8990faae19374e1**. Los commits posteriores solo registran el informe.

**Parada por F2.** El filtro nominal está implementado, pero R8 sube en el total y en Definidor. Se conserva el criterio: sin ajustes de código o calibración para obtener Pass y sin fusión.

| Grupo | R8 base → candidato | E1/estirados | E1/puntos del rival |
|---|---|---|---|
| total | 1436/10632 (13,5064 %) → 1437/10607 (13,5477 %) **SUBE** | 5/10607 = 0,0471 % | 2/9216 = 0,0217 % |
| Fondista | 607/3921 (15,4807 %) → 601/3886 (15,4658 %) | 3/3886 = 0,0772 % | 1/3081 = 0,0325 % |
| Definidor | 414/3527 (11,7380 %) → 422/3535 (11,9378 %) **SUBE** | 1/3535 = 0,0283 % | 0/3117 = 0,0000 % |
| Completo | 415/3184 (13,0339 %) → 414/3186 (12,9944 %) | 1/3186 = 0,0314 % | 1/3018 = 0,0331 % |

Todos los denominadores son positivos. F2 falla total **+0,0412614212 pp** y Definidor **+0,1997442241 pp**. Ambas cuentas E1 cumplen ≤5 %, total y por estilo; el denominador de puntos incluye todos los ganados por el rival en points.tsv.

**F1/M3 cumple:** experimento original sin editar, 100 partidos por estilo/nivel, NewcomerBot σ90, semillas 20270131, **1/1 Passed**. Medio: Fondista **178/3819 = 4,6609 %**, Definidor **441/4343 = 10,1543 %**, Completo **361/4156 = 8,6862 %**. Orden, bandas y resto de M3 correctos; cero SystemFault en nueve celdas. Fácil/Difícil constan en la evidencia, sin aplicarles las bandas de Medio.

**TDD y regresión:** B3c/B3d RED válido **0/6**, GREEN **6/6**; dirigidos **28/28**; EditMode completo **996/996**, incluido M3 existente sin modificar. Grilla: 21.600 decisiones, 20 evaluaciones de dos paredes Good pero inalcanzables por estilo, cero elecciones inalcanzables. El antiguo B3 conserva su precondición fallida. La casilla trasera B4 tiene 6/6 alcanzables y no ejercita E1; el caso sintético y el delantero real sí lo cubren. Huella planned y hashes intactos.

**F2:** Anexo A, 150 partidos ×3 estilos ×3 niveles, semillas 20261122, sin parámetros cambiados. Base **1/1 +1/1**, 42.986 filas y rows/points/config idénticos byte a byte. Candidato **1/1**, 42.957 filas, 13.012 puntos. Cero desalineaciones y SystemFault en las filas. No se inicia la segunda corrida del candidato tras F2: su repetición no se declara aprobada.

ret_fw del candidato: delante/media (r_z <7,31) **n6617, p50 1,045 m, p95 3,1344 m**; atrás **n3990, p50 3,344 m, p95 4,05055 m**. Dos paredes estiradas: base 728, candidato 713. El alto delantero sigue siendo la recta alta provisoria aceptada; sin globo nuevo ni preset. R7 no medido.

**PlayMode, AutoQA V/T3/volea y check-release no evaluados por la parada F2.** Se respetaron las prioridades de #102 e INSTALL-EN-CURSO; cada corrida liberó su propio candado. Copia autorizada de device.env cotejada y retirada, fuente intacta. Sin cambios a tests existentes de la base, física, solver, presets, umbrales u oráculos; sin fusión, build nativa ni instalación. [Protocolo y cifras completas](../validation/2026-11-23-rival-short-b-r2.md).
