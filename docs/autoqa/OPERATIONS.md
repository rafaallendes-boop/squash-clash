# Operación de AutoQA

Unity y el oráculo determinista deciden los resultados. El operador los comunica; ningún modelo participa en las recepciones. Claude y Codex pueden ejecutar el mismo comando sin adaptadores, APIs de IA, red ni claves:

```sh
scripts/autoqa.sh mvp
```

El comando imprime `RUN_DIR=<ruta>`, `RESULT=<gate>` y `ESCALATION_LEVEL=<nivel>`. Leer `summary.json`, `attempts.jsonl` y `report.md` en esa carpeta; consultar los JSONL originales de `game-metrics/` mediante sus rutas y SHA-256. No usar el resultado de NUnit como sustituto del gate: el canario es un FAIL esperado del oráculo y el test de ejecución puede terminar bien.

## Política del PO y niveles

Preferencias sujetas a disponibilidad en cada herramienta, sin codificar modelos en el runner:

- Nivel 1, operación rutinaria: Luna 6 Low. Ejecutar, reunir artefactos y rellenar la plantilla. Gate PASS no requiere intervención del modelo principal.
- Nivel 2, investigación: Luna 6 Medium. Ante FAIL, INCONCLUSIVE, NOT_EVALUATED, diferencias de determinismo o executionError, analizar evidencia y proponer causa; sin cambios de producción, escenarios, oráculo, parámetros o criterios.
- Nivel 3, escalamiento técnico: Sol 6.1 High u Opus 5.5 High. Contratos, física, timing, arquitectura, integridad o alcance requieren revisión técnica principal.

El escalamiento inicial se calcula en código. El operador puede señalar evidencia de que corresponde una revisión técnica de nivel 3; no cambia los resultados ni la política del runner. Los modelos económicos no aprueban sus propios cambios ni sustituyen la revisión independiente. La revisión de esta PR corresponde a Claude y la coordina el PO. Toda fusión exige autorización explícita del PO después de esa revisión.

## Triaje y salidas

Primero se evalúa integridad: hash/SimulationKey inesperados, presets alterados, guardia roja o SHA inconsistente implican nivel 3 y salida 3. También un canario ejecutado que no da FAIL. Si el canario no llegó a ejecutarse por executionError/gate ERROR, su ausencia no es por sí sola integridad; **un canario PASS siempre es nivel 3**, aunque también haya error de ejecución. Hash inesperado puede dejar el intento INCONCLUSIVE, pero integridad fija nivel/salida 3.

Si un FAIL normal o una diferencia de determinismo detiene los escenarios normales, el canario se ejecuta igualmente (opción A), en una instancia fresca: con canario FAIL, la regresión sale 1/nivel 2. Se omite ante guardia roja o executionError; su ausencia en el ERROR no añade integridad. La guardia roja ya es por sí sola integridad. No se relanzan ni se sustituyen los escenarios normales detenidos.

Sin fallo de integridad: nivel 2 ante cualquier intento no PASS, determinismo diferente o executionError; si todo pasa y el canario falla, nivel 1. Los recuentos excluyen el canario. El gate siempre está definido, incluso con canario PASS; ante integridad se usa FAIL (ERROR si también hay error de ejecución), nunca se oculta la salida 3.

| Salida | Significado |
| ---: | --- |
| 0 | Corrida completa: 18/18 PASS, canario FAIL e integridad/determinismo correctos |
| 1 | FAIL o determinismo diferente, sin causa de mayor precedencia |
| 2 | INCONCLUSIVE o NOT_EVALUATED, incluida una selección parcial |
| 3 | Integridad/nivel 3 |
| 4 | Error de ejecución o entorno, sin integridad fallida |

Precedencia cuando coinciden causas: **3 > 4 > 1 > 2**. Cualquier salida distinta de cero es no aprobado. En errores que impiden evaluar una integridad, sus campos quedan `null` (desconocidos), no `true`; se conserva `executionError.kind`. Esto permite distinguir ausencia de resultado de un fallo de integridad demostrado.

## Estado del árbol y errores

El runner registra antes de Unity el estado completo de Git y si GraphicsSettings.asset/QualitySettings.asset estaban limpios respecto de HEAD, tanto en índice como en worktree. Al finalizar, incluso con ERROR o timeout, restaura solo esos dos transitorios que estuvieran limpios antes. Los previamente modificados se dejan tal cual respecto de la restauración; dirty ya era true. No restaura otros archivos.

Consultar `restoration.json` y su referencia SHA-256 en summary.json. Si el estado completo posterior difiere del previo, o la restauración falla, el control final prescrito por el PO impone **ERROR/salida 4**, `executionError.kind=dirty-after-run`; conserva un nivel 3 previo si ya existía. No aprobar esa corrida ni ejecutar git checkout sobre otros archivos para ocultar el problema. Reportar los cambios y escalar.

Antes de sustituir un resumen existente por un ERROR se conserva `summary.harness.json` y, cuando existe, `report.harness.md`, referenciados con SHA-256 en artifacts.harnessSummary/harnessReport. Esos archivos permiten consultar el resultado previo; el resultado operativo sigue siendo el ERROR del wrapper. Un archivo de restauración correcto tampoco sustituye el gate.

## Resumen del operador

La única plantilla canónica es [operator-summary-template.md](operator-summary-template.md). OPERATIONS la referencia en vez de duplicarla. El verificador de la PR #37 deberá usar ese mismo archivo; no se ha modificado su rama. Copiar valores de summary.json y de cada intento no PASS de attempts.jsonl, incluidos los del canario identificado como esperado. La prueba rellenada de consumibilidad se adjunta al informe de entrega.

## Prohibiciones y reintentos

El operador no puede inventar resultados ni valores ausentes; editar `Tests/AutoQA/**` o `scripts/autoqa.sh`; cambiar expectativas, escenarios, oráculo o parámetros para conseguir PASS; declarar éxito con INCONCLUSIVE/NOT_EVALUATED; ni reintentar buscando un resultado favorable.

Se permite **como máximo un reintento**, solo si `executionError` es de entorno (por ejemplo bloqueo del proyecto, licencia o XML ausente). Informar ambas corridas. Nunca reintentar ante FAIL, INCONCLUSIVE o falta de determinismo. El runner no reintenta automáticamente. Las inyecciones de fallo de desarrollo son comprobaciones separadas y declaradas; no son mediciones del juego ni una facultad del operador.

Audibilidad, háptica percibida, lectura visual, latencia toque→imagen, sensación y diversión siguen siendo comprobaciones del PO en una sesión iPhone autorizada aparte. Esta entrega tampoco ejercita digitalizador, UIKit ni IL2CPP durante las recepciones.
