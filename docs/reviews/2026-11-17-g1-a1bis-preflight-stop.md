# G1 A1-bis — detención previa por G1-A-EXPORT-01

17 nov 2026 · Codex · Base `3e1e037` de #71, aún abierta. **A1-bis no se ejecutó; A2 no comenzó.** Se registra la decisión del PO en [aprobaciones](2026-10-25-first-shot-po-approvals.md). La nueva rama `codex/g1-a1bis-timing` se apila sobre `codex/g1-diagnosis`; no presupone fusión ni aceptación de #71.

**Hallazgo:** al inspeccionar la población que A1-bis debe reproducir, apareció una columna sin nombre con valor `bodyContactZ` en el TSV de recepciones de la fase A. Es un defecto del exportador experimental que Codex añadió en esa fase, no un defecto demostrado del producto.

| Evidencia, conservada sin cambios | Hallazgo |
|---|---|
| `Logs/experiments/g1/run01/receptions.tsv` del worktree G1 | 648/648 filas con clave vacía y valor `bodyContactZ` |
| `Logs/experiments/g1/run02/receptions.tsv` del worktree G1 | 648/648, mismo defecto determinista |
| [a1-receptions.tsv](../validation/evidence/2026-11-17-g1-diagnosis/a1-receptions.tsv), versionado en #71 | Conserva esa columna adicional |
| `a1-shots.tsv` / `a2-witnesses.tsv` | Sin cabecera vacía |

**Causa estática localizada:** `G1BackCourtDiagnosisExperiment.cs:222`, inicialización de la fila en `Receive`. Faltan valores vacíos para mantener las parejas clave/valor de stance y cuerpo; acaba insertando `row[""] = "bodyContactZ"`. El segundo `Put`, líneas 234–238, escribe correctamente `stanceZ`, `bodyContactX` y `bodyContactZ`, pero no elimina la clave vacía. `WriteRows` exporta todas las claves, incluida esa. La comprobación de cantidad de campos del TSV no detectó la cabecera vacía; la repetibilidad entre procesos tampoco demuestra corrección del exportador.

**Alcance conocido:** las 108 soluciones de A1 quedaron resueltas, por lo que las 648 filas alcanzaron esa segunda escritura. Los campos nombrados de stance/cuerpo se repusieron y los agregados de #71 usan campos nombrados; la columna extra no demuestra por sí sola que sus recuentos estén mal. Esto no es una revalidación completa ni justifica borrar la evidencia original. No se cambió código de producto, defaults, hashes, umbrales ni criterios. Tampoco se cambió el exportador ni se regeneraron datos.

**Regla aplicada:** el encargo G1 fase B, §Reglas, exige: «Ante un defecto o una sorpresa: registra y detente, sin reintentar para obtener Pass.» Coincide con AGENTS §2. Se detiene antes del método/código nuevo y de cualquier invocación Unity; no hay RED, experimento nuevo, regresión, AutoQA, build ni acceso al iPhone que reportar.

**Decisión requerida para continuar:** levantar esta detención y autorizar una corrección acotada del exportador experimental y una guarda que detecte cabeceras vacías/pares incorrectos, preservando ambas corridas originales. No se propone repetir la fase A para obtener otro resultado. A1-bis mantendría su método previo y sus dos procesos propios, sin producto; su informe se entregaría para elección del PO. A2 conservaría su parada de diseño antes de implementar. No se ejecuta ninguna de estas acciones correctivas con este informe.
