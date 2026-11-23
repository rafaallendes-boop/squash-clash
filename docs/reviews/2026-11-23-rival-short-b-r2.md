# RAL-174 — camino B, dirigidos GREEN; gates pendientes

23 nov 2026 · Codex · `feat/rival-short-i2i3` · PR #101 en borrador.

La #92 ya está fusionada: `0a49b6a`, segundo padre igual al head aprobado `3a12e36`. Se incorporó main con merge normal `b6b0431` y se leyó íntegra la spec vigente. No se fusiona este candidato.

Implementado B: recta/cruzado alto medio y dos paredes donde ya existe, todo filtrado por el planificador nominal del humano. La dos paredes usa ForecastBoast/SolveBoastIntention. E1 escoge el menor retraso no dos paredes y se marca; planned no cambia. El Anexo A registra fb y todos los puntos en points.tsv para ambas cuentas E1.

**TDD:** B3c/B3d RED válido, **6 tests, 0 Passed, 6 Failed**, sin errores CS. GREEN **6/6**. Dirigidos completos **28/28**. Grilla B3b: 21.600 decisiones, 20 evaluaciones de dos paredes Good pero inalcanzables por estilo y cero elecciones inalcanzables. El antiguo B3 sigue registrado como precondición fallida en su única escena; la nueva grilla sí ejercita el rechazo. La casilla trasera B4 ofrece 6/6 alcanzables y no ejercita E1; E1 con dos paredes se verifica sintéticamente en B3c y el delantero real conserva su guardia. Huella planned y hashes intactos.

**Pendientes:** regresión EditMode, PlayMode, F1/M3, F2/R8, ambas cuentas E1, AutoQA V/T3/volea y check-release. Las mediciones anteriores de main `230b758` son antecedentes; no validan el candidato ni la nueva base incorporada `0a49b6a`. R7 no medido. El entorno se resolvió con la copia autorizada desde `sc-install-518f9c3`, SHA-256 coincidente e ignorada; se borrará al terminar. #102 salió de borrador a las 16:48:49Z y solo después se tomó el primer candado de esta reanudación. Cada corrida libera el suyo al terminar.

La PR continúa en borrador. No se cambiaron tests existentes en main, física, solver, presets, umbrales ni criterios. [Protocolo, autorización y evidencia](../validation/2026-11-23-rival-short-b-r2.md).
