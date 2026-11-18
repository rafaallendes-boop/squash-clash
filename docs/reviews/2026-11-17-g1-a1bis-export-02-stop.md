# G1 A1-bis — detención por G1-A1BIS-EXPORT-02

17 nov 2026 · Codex · #72, sobre #71. **La corrección autorizada de CS-01 compila; A1-bis sigue sin resultados científicos por un nuevo defecto del inicializador de su fila.** No se corrige ni reintenta. A2 conserva su parada de diseño, sin iniciar implementación.

| Paso sobre `5cae7bd` | Evidencia | Resultado |
|---|---|---|
| Sustitución de dos `Is.AnyOf` | Dos líneas: tres igualdades explícitas con `Is.True` | Aceptan exactamente Contact/Air/Late |
| Preservación de CS-01 | Carpeta `attempt-CS-01-run01-runner`, hashes originales | Intacta; ese intento nunca produjo XML |
| Verificación previa de compilación | Inventario vacío, log fresco y hash del DLL experimental | **Compila**, sin CS; fuente limpia |
| Primer proceso predeclarado | Inventario vacío; XML fresco, 14:29:39 UTC | **0/1**, excepción `Incomplete key/value pair` |
| Segundo proceso | No lanzado | No evaluado |

**Defecto nuevo de Codex:** la llamada `Put` del diagnóstico, `G1TimingDiagnosisExperiment.cs:84–89`, tiene cantidad impar de argumentos: falta el valor vacío de `kOutMs` antes de `widthMs`. La guarda añadida por G1-A-EXPORT-01 lanza `ArgumentException` antes de mutar la fila. No se cambió esa llamada durante esta continuación: la autorización cubría únicamente las dos aserciones.

El primer test del diagnóstico sí llegó a iniciarse, a diferencia de CS-01, pero abortó en la primera fila antes de analizar ventanas o muestrear releases. El XML registra **1 Failed (Error)**, no un RED esperado. Solo se generaron metadatos `source.tsv` y `samples.tsv` con su cabecera, **cero muestras**; no hay `plans.tsv` ni `intervals.tsv`. La hipótesis de timing sigue **NotEvaluated**: no se calculan probabilidades ni tasas parciales como si fueran resultados válidos.

**Preservación:** XML/log/inventario nuevos se separan en `Logs/experiments/g1-timing/attempt-EXPORT-02-run01-runner`; los dos archivos parciales, en `attempt-EXPORT-02-run01`. Se conservan sin cambiar bytes ni inodos. La compilación previa queda en `compile-run01-runner`. El [manifiesto](../validation/evidence/2026-11-17-g1-a1bis-timing/manifest.json) registra sus hashes y los del intento anterior; [recibo de CS-01](../validation/evidence/2026-11-17-g1-a1bis-timing/cs-01-manifest.json). Logs y crudos siguen ignorados. Los datos originales de fase A no se escribieron ni repitieron.

**Alcance cumplido:** corrección limitada a las dos aserciones; método `73d86a2` idéntico; compilación verificada antes de la primera corrida. No se ejecuta la segunda porque el PO exige detenerse ante cualquier sorpresa. No se modificaron producto, defaults, hashes, anclas, umbrales ni expectativas existentes. La reparación anterior del exportador conserva su RED/GREEN 0/4→4/4; no se vuelve a correr ese filtro.

**Continuación propuesta, pendiente de autorización:** completar únicamente el valor vacío faltante de `kOutMs`, preservando los intentos fallidos; verificar la paridad de todas las llamadas `Put` del nuevo diagnóstico antes de otra compilación/corrida, sin cambiar la grilla ni el método. Si esa inspección identifica otro defecto, registrarlo sin corregirlo. No se ejecuta esa corrección ni una nueva invocación con este informe. #72 sigue borrador; sin fusión ni instalación.
