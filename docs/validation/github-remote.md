# T1.4 — remoto privado

Fecha: 2026-10-21. Autorización explícita del PO registrada en [aprobaciones](../reviews/2026-10-21-first-ball-po-approvals.md).

Se ejecutó `gh repo create squash-clash --private --source . --push` desde la raíz; código 0. Verificación con `gh repo view --json visibility,name,nameWithOwner,url,defaultBranchRef`:

```json
{"defaultBranchRef":{"name":"main"},"name":"squash-clash","nameWithOwner":"rafaallendes-boop/squash-clash","url":"https://github.com/rafaallendes-boop/squash-clash","visibility":"PRIVATE"}
```

Repositorio: [rafaallendes-boop/squash-clash](https://github.com/rafaallendes-boop/squash-clash). Antes de publicar se comprobó que `device.env` está ignorado y no versionado, y que los identificadores completos del iPhone no aparecen en los blobs del historial Git. `.superpowers/`, logs, cachés y builds permanecen excluidos; `docs/` está versionado. Log local: `Logs/t14-create.log`.

**T1.4 completada.** I2 está autorizado en `firstball/i2-court`, con PR hacia `main`; revisión de Claude y validación del PO pendientes al finalizar el incremento. El worktree local `.worktrees/i2-court` se excluye de Git. T3.1 y etapas posteriores no están autorizadas.
