# RAL-174 — camino B, RED preparado; gates bloqueados

23 nov 2026 · Codex · `feat/rival-short-i2i3` · PR #101 en borrador.

La #92 ya está fusionada: `0a49b6a`, segundo padre igual al head aprobado `3a12e36`. Se incorporó main con merge normal `b6b0431` y se leyó íntegra la spec vigente. No se fusiona este candidato.

Preparados B3c (cuatro casos sintéticos con esqueleto sin filtro), B3d (ambos escenarios contra cálculo independiente) y la grilla B3b. El resultado histórico de la precondición B3 se conserva: las cuatro dos paredes eran Good y Reachable; no probó rechazo. El Anexo A ahora registra fb y todos los puntos en points.tsv para las dos cuentas E1. El GREEN de B no está implementado.

**Bloqueado antes de Unity:** falta `scripts/device.env`, obligatorio para el resolver. El encargo prohíbe tocarlo o copiarlo. Se solicitó aclarar su provisión; no se creó, copió, enlazó ni modificó. No se abrió Unity ni se adquirió un candado.

**Resultados de esta revisión:** `git diff --check` OK. RED B3c/B3d, compilación, EditMode, PlayMode, F1/M3, F2/R8, las dos cuentas E1, AutoQA V/T3/volea y check-release **no evaluados**. Las mediciones anteriores de main `230b758` quedan como antecedentes; no validan el candidato ni la nueva base incorporada `0a49b6a`. R7 no medido.

La PR continúa en borrador. No se cambiaron tests existentes en main, física, solver, presets, umbrales ni criterios. [Protocolo, autorización y evidencia](../validation/2026-11-23-rival-short-b-r2.md).
