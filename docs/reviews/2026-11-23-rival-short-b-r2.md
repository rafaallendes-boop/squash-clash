# RAL-174 — camino B, RED preparado; gates bloqueados

23 nov 2026 · Codex · `feat/rival-short-i2i3` · PR #101 en borrador.

La #92 ya está fusionada: `0a49b6a`, segundo padre igual al head aprobado `3a12e36`. Se incorporó main con merge normal `b6b0431` y se leyó íntegra la spec vigente. No se fusiona este candidato.

Preparados B3c (cuatro casos sintéticos con esqueleto sin filtro), B3d (ambos escenarios contra cálculo independiente) y la grilla B3b. El resultado histórico de la precondición B3 se conserva: las cuatro dos paredes eran Good y Reachable; no probó rechazo. El Anexo A ahora registra fb y todos los puntos en points.tsv para las dos cuentas E1. El GREEN de B no está implementado.

**Entorno resuelto; esperando prioridad de Unity:** tras el intento fallido de la ruta anterior (código 1, fuente ausente; conservado en evidencia), el PO autorizó copiar desde `sc-install-518f9c3` (`device-env-ok.txt`, leído completo). Copia realizada sin editar, SHA-256 coincidente e ignorada por Git; se borrará al terminar. La PR #102 tiene prioridad para la build de hoy. Primera consulta: isDraft true. No se abre Unity ni se adquiere el candado hasta confirmar isDraft false.

**Resultados de esta revisión:** `git diff --check` OK. RED B3c/B3d, compilación, EditMode, PlayMode, F1/M3, F2/R8, las dos cuentas E1, AutoQA V/T3/volea y check-release **no evaluados**. Las mediciones anteriores de main `230b758` quedan como antecedentes; no validan el candidato ni la nueva base incorporada `0a49b6a`. R7 no medido.

La PR continúa en borrador. No se cambiaron tests existentes en main, física, solver, presets, umbrales ni criterios. [Protocolo, autorización y evidencia](../validation/2026-11-23-rival-short-b-r2.md).
