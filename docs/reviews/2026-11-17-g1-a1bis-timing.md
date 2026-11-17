# G1 A1-bis — exportador corregido; diagnóstico NotEvaluated por CS-01

17 nov 2026 · Codex · #72 apilada sobre #71. **G1-A-EXPORT-01 queda corregido en código experimental; A1-bis no tiene resultados científicos.** La primera invocación del diagnóstico falló al compilar. Se registra `G1-A1BIS-CS-01` y se detiene sin corrección ni reintento. A2 no comenzó; mantiene su parada de revisión de diseño antes de implementar.

| Trabajo | Evidencia | Estado |
|---|---|---|
| Autorización y método previo | `73d86a2` | Registrados antes de Unity |
| Guardas de esquema RED | `e498feb`, cuatro tests | **0/4**, cuatro aserciones esperadas, XML fresco, sin CS |
| Corrección y GREEN | `0161ae6`, mismos tests | **4/4**, XML fresco, sin CS |
| Nota de errata #71 | README de su evidencia | Añadida, sin sustituir tablas ni datos |
| Primera invocación A1-bis | `e184ce9`, inventario previo vacío | **NotEvaluated**: errores CS, ningún XML nuevo |

**Corrección acotada:** se completaron las parejas de `stanceZ`/`bodyContactZ` en el inicializador del exportador; `Put` valida cantidad par y nombres no vacíos antes de mutar la fila, y `WriteRows` rechaza cabeceras vacías antes de crear archivo. Las pruebas verifican esas tres condiciones y los campos iniciales nombrados. Son pruebas del assembly experimental: no ejecutan la grilla de fase A ni representan una suite completa de producto.

**Evidencia preservada:** se cotejó cada archivo crudo de `run01`/`run02` de fase A con su SHA-256 original: todos idénticos. El diff de su evidencia solo añade la errata al README. Según la verificación de Claude comunicada por el PO, los recuentos no cambian; las parejas se realinean, los campos nombrados se reescriben en las 648 filas y el resumen lee por nombre. No se repitió fase A.

**Defecto nuevo, atribuible al código experimental de Codex:** `G1TimingDiagnosisExperiment.cs:110` y `:151` usan `Is.AnyOf`, que no existe en la versión de NUnit del proyecto. Unity devuelve dos `CS0117` y `Scripts have compiler errors`; no se genera `results.xml`. El invocador termina con `STOP: falta XML fresco`. **Cero tests ejecutados, sin probabilidad analítica ni muestreo evaluados**; no es RED válido, ni Pass, ni evidencia de que la hipótesis sea cierta o falsa. El head actual del diagnóstico no compila y #72 permanece borrador.

**Evidencia local:** `Logs/experiments/g1-timing/schema-red-runner`, `schema-green-runner` y `run01-runner`. Inventario PID/comando/proyecto vacío antes de cada lanzamiento; Unity terminó en los tres casos. Logs/inventarios ignorados, [recibo y hashes](../validation/evidence/2026-11-17-g1-a1bis-timing/manifest.json) sanitizados. No hay corrida run02.

**Continuación propuesta, pendiente del PO:** sustituir solo esas dos restricciones de aserción por la API compatible de igualdad con alternativas, sin tocar la lógica medida ni los criterios; conservar el intento fallido y autorizar una nueva invocación del primer proceso de A1-bis. Solo tras completarlo y verificar sus datos se ejecutaría el segundo proceso predeclarado. No se corrigen las aserciones ni se lanza Unity con este informe. No hace falta cambiar Packages/NUnit ni repetir fase A.

Parada exigida por el encargo G1 fase B y AGENTS §2: «Ante un defecto o una sorpresa: registrar y detenerse, sin corregir ni reintentar para obtener Pass». Sin cambios de producto, defaults, hashes, anclas, fusión, build nativa ni instalación.
