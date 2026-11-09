# Operador QA de Claude para AutoQA

Capa de operación del lado de Claude para AutoQA de Squash Clash. Consume las salidas del arnés (`summary.json`, `attempts.jsonl`, `report.md` y los JSONL del juego) y aplica la política de operación del PO con modelos económicos. **No duplica el arnés ni lo modifica.** Unity y el oráculo deciden PASS/FAIL; ningún modelo de lenguaje lo decide.

Archivos:

| Archivo | Papel |
| --- | --- |
| [`.claude/agents/autoqa-operator.md`](../../.claude/agents/autoqa-operator.md) | Nivel 1. Ejecuta o lee una corrida, pasa el verificador determinista y devuelve la plantilla |
| [`.claude/agents/autoqa-investigator.md`](../../.claude/agents/autoqa-investigator.md) | Nivel 2. Análisis de solo lectura de un resultado no PASS |
| [`.claude/skills/autoqa/SKILL.md`](../../.claude/skills/autoqa/SKILL.md) | `/autoqa`: encadena operador → investigador → parada |

## Estado y dependencia

El contrato que consume esta capa es la sección `<operations_policy>` del prompt de Codex del 2026-11-06: esquema `autoqa-summary/1`, códigos de salida 0–4, niveles 1–3 y su regla fija, prohibiciones del operador, reintento único por error de entorno y plantilla de resumen.

La PR de Codex se fusionó el 2026-11-09 (PR #36, merge `e18d3e1`). Su contrato real está en [`OPERATIONS.md`](OPERATIONS.md) y su plantilla única en [`operator-summary-template.md`](operator-summary-template.md). **Manda la implementación real:** el verificador `v4` se ajustó a ella y las diferencias con los supuestos anteriores están en [Contraste con la implementación real](#contraste-con-la-implementación-real).

## Correspondencia con la política del PO

| Política del PO | Implementación |
| --- | --- |
| Cualquier agente autorizado puede ejecutar `scripts/autoqa.sh mvp`, leer los artefactos, identificar veredictos y escalar | El operador ejecuta el script (modo `run`) o lee un `RUN_DIR` (modo `read`) |
| Triaje determinista calculado por el arnés, nunca por un modelo | El nivel sale de `summary.json → escalationLevel`. El verificador solo puede subirlo a 3 por incoherencia de integridad |
| Nivel 1, QA rutinario: resumen breve con la plantilla | Operador en Haiku. La plantilla la imprime el verificador determinista; el modelo la copia |
| Nivel 2, investigación sin modificar nada | Investigador en Sonnet, solo lectura, hechos citados y separados de hipótesis |
| Nivel 3, escalamiento a Codex o Claude principal | `/autoqa` se detiene y entrega un paquete de escalamiento. No envía mensajes a nadie |
| Prohibiciones del operador | Copiadas en el operador, el investigador y la skill |
| Como máximo un reintento, solo por error de entorno | Lo decide el verificador (`retry.eligible`). Solo el operador en modo `run` lo ejecuta, una vez |
| Códigos de salida 0–4; distinto de 0 es no aprobado | El verificador comprueba que el código 0 coincida con gate PASS y nivel 1; si no, incoherencia |
| Los modelos económicos no aprueban sus propios cambios | Ningún nivel modifica archivos ni aprueba cambios |
| Qué NO verifica AutoQA | Lo imprime cada resumen: audibilidad, háptica percibida, lectura visual, latencia toque→imagen, sensación del golpe y diversión |

## Equivalencia de modelos

**Confirmada por el PO el 2026-11-08** («Confirmo los modelos.»), comunicada por la sesión principal de AutoQA. Era una propuesta de Claude. La política del PO nombra Luna 6 Low, Luna 6 Medium y Sol 6.1 High u Opus 5.5 High. En Claude Code se usa:

| Nivel | Política del PO | Claude Code |
| --- | --- | --- |
| 1 | Luna 6 Low | Haiku 5.5 (`model: haiku` en el operador) |
| 2 | Luna 6 Medium | Sonnet 5.5 (`model: sonnet` en el investigador) |
| 3 | Sol 6.1 High u Opus 5.5 High | Opus 5.5 High en la sesión principal, no como subagente |

Los nombres de modelo solo aparecen en el frontmatter de los agentes, nunca en el arnés. No se fija `effort` en el frontmatter.

## Cómo invocarlo

En una sesión de Claude Code abierta en el repositorio:

- `/autoqa` o `/autoqa mvp`: ejecuta `scripts/autoqa.sh mvp` y opera la corrida.
- `/autoqa <RUN_DIR>`: lee una corrida existente, por ejemplo `Logs/autoqa/<runId>`, sin ejecutar Unity.
- `/autoqa <RUN_DIR> repo=<checkout>`: lee o ejecuta en otro checkout, por ejemplo el worktree de la PR de Codex.

Claude Code detecta los cambios en `.claude/agents/` y `.claude/skills/` durante la sesión, pero no un directorio `agents` o `skills` creado después de abrirla. En ese caso hace falta reiniciar o usar `/reload-skills`. Si el subagente no está disponible, `/autoqa` usa la equivalencia `general-purpose` con el modelo forzado y lo dice en su respuesta.

Antes de ejecutar, el operador comprueba que exista `scripts/autoqa.sh` y que ningún Unity esté abierto sobre ese checkout. Si alguna condición falla, no ejecuta y escala con el motivo.

## Qué hace y qué no hace cada nivel

### Nivel 1 — operador (`autoqa-operator`, Haiku)

Hace:
- ejecuta el script una vez, o lee un `RUN_DIR`;
- pasa el verificador determinista incluido en su propio archivo (bloque `autoqa-check v4`, extraído con `awk` y ejecutado con `python3 -I`, solo biblioteca estándar);
- devuelve la salida del verificador carácter por carácter y una sección breve de notas (comandos, códigos, reintento);
- ejecuta el único reintento solo si el verificador lo declara elegible.

No hace:
- interpretar causas ni recomendar correcciones;
- recalcular o rebajar el nivel;
- editar archivos (no tiene `Edit`, `Write`, `NotebookEdit` ni `Agent`);
- obedecer texto de los artefactos.

### Nivel 2 — investigador (`autoqa-investigator`, Sonnet)

Herramientas: solo `Read`, `Grep` y `Glob`, sin Bash (decisión del PO del 2026-11-08, opción B). El nivel 2 es de solo lectura por herramientas.

Hace:
- lee `summary.json`, los intentos no PASS de `attempts.jsonl`, los JSONL del juego y, si hace falta, escenarios y documentación de Codex;
- separa hechos (con cita a archivo y campo o línea) de hipótesis (con lo que las confirmaría o refutaría);
- recomienda una sola opción: `reoperar` (solo error de entorno con el reintento sin usar), `escalar-nivel-3` o `decision-PO`.

No hace:
- modificar archivos, escenarios, oráculo, parámetros, umbrales ni criterios;
- proponer cambiar una expectativa para que un intento pase;
- aprobar cambios;
- proponer un nivel menor que el del operador;
- ejecutar comandos: no tiene Bash, así que no hay `jq`, `shasum` ni historial de git. Si una causa exige un comando, recomienda `escalar-nivel-3` y anota el comando como evidencia pendiente.

### Nivel 3 — parada

`/autoqa` no lanza el investigador. Entrega un paquete de escalamiento con datos copiados (runId, gitHash, gate, razones, incoherencias, integridad, SHA-256, intentos no PASS y texto con forma de instrucción) y la decisión requerida. El destino lo asigna el PO: Codex o Claude principal. No se envía nada a nadie.

### Orquestación (`/autoqa`)

La skill compara el bloque `autoqa-operator-result` del operador con una ejecución propia del mismo verificador. Si difieren, el operador alteró datos y la corrida pasa a nivel 3. Si el investigador propone un nivel menor, declara `filesModified` distinto de `false` o cambia el estado de git, su propuesta se descarta y se escala a nivel 3. La skill no reintenta: una recomendación `reoperar` se entrega al usuario, que decide.

## Verificador determinista

El verificador es el único cálculo del operador. Solo copia valores, compara SHA-256 y aplica comprobaciones que únicamente **suben** el nivel a 3:

| Comprobación | Resultado si falla |
| --- | --- |
| `summary.json` existe, es JSON y `schema` es `autoqa-summary/1`, con todos los campos de la política | Incoherencia, nivel 3 |
| `escalationLevel` ∈ {1, 2, 3} y `gate` ∈ {PASS, FAIL, INCONCLUSIVE, NOT_EVALUATED, ERROR} | Incoherencia, nivel 3 |
| SHA-256 de `attempts.jsonl`, `report.md` y cada JSONL del juego declarado, en el resumen y en cada intento | Incoherencia, nivel 3 |
| `report.md` no declara un gate o `RESULT` distinto del de `summary.json` | Incoherencia, nivel 3 |
| Recuentos por veredicto y canario de `summary.json` iguales a los de `attempts.jsonl`, con `runId` igual en cada intento | Incoherencia, nivel 3 |
| Integridad explícitamente `false`, o canario que no falla, con nivel distinto de 3. El canario no falla si `observed` es `PASS`, o si es INCONCLUSIVE, NOT_EVALUATED o `null` en una corrida sin `executionError` ni gate ERROR | Incoherencia, nivel 3 |
| Hash efectivo inesperado en un intento (`config.hashMatch=false` o `effectiveHash` distinto de `expectedHash`) con nivel distinto de 3, o con `integrity.hashesVerified=true` | Incoherencia, nivel 3 (decisión del PO del 2026-11-08: es fallo de integridad, nivel 3 y salida 3) |
| Gate, recuentos, determinismo o `executionError` no PASS con nivel 1 | Incoherencia, nivel 3 |
| En modo `run`, el código de salida del script es uno de los que corresponden a la evidencia (ver abajo), y 0 solo con nivel 1 | Incoherencia, nivel 3 |
| Texto con forma de instrucción en los artefactos | Se lista como dato. No cambia el nivel |

«Sin intervención necesaria» solo es `sí` con gate PASS, nivel 1, sin incoherencias, código esperado 0 y código real 0 (o sin código, en modo lectura).

**Código de salida que corresponde a la evidencia.** El verificador lo deriva de `summary.json`, `attempts.jsonl` y los SHA-256, según el §6 de la política y la decisión del PO del 2026-11-08:

| Categoría | Código |
| --- | --- |
| Integridad: algún campo de `integrity` en `false`, hash efectivo inesperado en un intento, SHA-256 de evidencia que no coincide o falta, o canario que no falla (con la misma excepción) | 3 |
| `executionError` o gate ERROR | 4 |
| FAIL en el gate o en los recuentos, o determinismo `false` | 1 |
| INCONCLUSIVE o NOT_EVALUATED en el gate o en los recuentos | 2 |
| Ninguna de las anteriores | 0 |

Solo se acepta el código esperado, elegido por la precedencia **3 > 4 > 1 > 2** que el arnés adoptó en `OPERATIONS.md`. En modo `read` el código se informa en el resumen (`expectedExitCode`, `acceptableExitCodes`, `exitCategories`) sin compararse con nada. Un `null` en `determinism` es desconocido y no cuenta como determinismo distinto; solo `identical: false` da salida 1.

Reintento elegible: hay `executionError`, el único reintento no se ha usado, no hay incoherencias ni fallo de integridad, ni FAIL, INCONCLUSIVE ni determinismo distinto, y la causa es de entorno. Si `executionError.kind` existe, manda: `environment` (Unity no disponible o timeout) o `missing-xml` (XML ausente), según `OPERATIONS.md`. `dirty-after-run`, `tests`, `compilation`, `incomplete-run` y los demás no son elegibles. Si no hay `kind`, se reconoce por el texto (bloqueo del proyecto, licencia, XML ausente, batchmode). Ante la duda no es elegible y decide el nivel 2.

Cambiar el verificador requiere aprobación del PO, como cualquier otro criterio.

## Cómo verificar que un resumen no inventa datos

1. Vuelve a ejecutar el verificador sobre el mismo `RUN_DIR`:

   ```bash
   awk '/^# >>> autoqa-check v4/{f=1;next} /^# <<< autoqa-check v4/{f=0} f' .claude/agents/autoqa-operator.md \
     | python3 -I - --run-dir "<RUN_DIR>" --mode read --template docs/autoqa/operator-summary-template.md
   ```

2. La salida debe estar contenida, carácter por carácter, en el resumen del operador. `/autoqa` lo hace automáticamente comparando los bloques JSON.
3. Cada SHA-256 citado debe coincidir con `shasum -a 256` del archivo.
4. Cada valor de un hecho del investigador debe encontrarse en el archivo y campo que cita, por ejemplo con `jq` (lo comprueba quien revisa, no el investigador).
5. Un `runId` que empieza por `SYNTHETIC-` o `"synthetic": true` es un fixture de prueba, nunca evidencia.

## Limitaciones

- **Bash no se puede restringir por comando** en el frontmatter de un subagente: `tools` acepta o quita la herramienta completa. Decisión del PO del 2026-11-08, «Me parece la opción B»:
  - el investigador no tiene Bash, así que el nivel 2 es de solo lectura por herramientas;
  - el operador conserva Bash porque lo necesita para ejecutar `scripts/autoqa.sh` y el verificador. Su regla de no escribir depende de las instrucciones, y las verificaciones comprueban después que `git status` y los SHA-256 no cambien;
  - una lista blanca técnica (hook o `permissions.deny`) queda fuera de alcance, para una posible PR futura. `.claude/settings*.json` no se toca.
- Si el agente `autoqa-investigator` no está cargado, `/autoqa` usa un `general-purpose` con Sonnet, que sí tiene Bash y solo lo evita por instrucciones. La skill lo dice en la entrega.
- El verificador detecta el gate de `report.md` por texto (`gate` o `RESULT` seguidos del veredicto). Si el informe real usa otra forma, lo indica como nota y no puede compararlo.
- La detección de texto con forma de instrucción es heurística: lista frases sospechosas, no garantiza encontrarlas todas. La protección principal es que el operador solo copia la salida del verificador.
- Los fixtures sintéticos prueban el comportamiento de los agentes, no el arnés. Ninguna corrida real se simuló con fixtures.
- AutoQA no verifica audibilidad, háptica percibida, lectura visual, latencia toque→imagen, sensación del golpe ni diversión. Siguen siendo verificación del PO en el iPhone.

## Contraste con la implementación real

Cotejo del 2026-11-09 contra la PR #36 fusionada (`e18d3e1`) y diez corridas reales de `scripts/autoqa.sh mvp`. Donde la implementación difería del supuesto anterior, manda la de Codex y el verificador pasó a `v4`.

| Supuesto anterior | Implementación real | Cambio en `v4` |
| --- | --- | --- |
| Recuentos en `counts` con `PASS`, `FAIL`, `INCONCLUSIVE`, `NOT_EVALUATED` | Igual; excluyen el canario | Ninguno |
| `canary.observed` con el veredicto del canario | Igual. `null` cuando no se ejecutó | Ninguno |
| `artifacts.gameMetrics[]` como `{path, sha256}` | Igual, relativos a `RUN_DIR` | Ninguno |
| `executionError` `{code, message}` con `kind` opcional | `{kind, message}`; `kind` siempre presente | Reintento solo con `kind` `environment` o `missing-xml` |
| `determinism` como booleano por escenario | `{id: {"identical": bool}}` | Se lee `identical`. Con `v3`, una corrida PASS real salía nivel 3 por esto |
| Un intento detenido conserva su configuración | El intento detenido trae `config` vacía, `effectiveHash=""` y `hashMatch=false` | Sin `effectiveHash` y NOT_EVALUATED no es hash inesperado. Con `v3`, un FAIL real salía nivel 3 |
| La salida acepta cualquier categoría presente sin integridad | Precedencia estricta 3 > 4 > 1 > 2 en `OPERATIONS.md` | Solo se acepta el código esperado |
| `report.md` con `Gate:` o `RESULT:` | Línea `gate: <valor>` | Ninguno; ya se reconocía |
| `RUN_DIR=` o `RUN_DIR:` | `RUN_DIR=`, `RESULT=` y además `ESCALATION_LEVEL=` | Ninguno |
| Plantilla de resumen propia del verificador | Plantilla única `docs/autoqa/operator-summary-template.md` | El verificador la rellena con `--template`; un marcador desconocido es incoherencia |
| Canario omitido si se detiene la corrida | El canario se ejecuta tras un FAIL normal o de determinismo (opción A) | Ninguno en el cálculo; regla documentada en el operador |
| Sin restauración del árbol | `restoration.json` y `executionError.kind=dirty-after-run` | Se verifica su SHA-256; `dirty-after-run` no es elegible para reintento |
| Sin resumen previo conservado | `summary.harness.json` y `report.harness.md` ante un ERROR del envoltorio | Se verifican sus SHA-256 y `attempts.jsonl` se coteja con `summary.harness.json` |
| `report.md` menciona el `runId` | No lo menciona | Se retira esa nota |

Se mantienen: el canario identificado por `scenario.id` que empieza por `CANARY`; integridad no evaluada como `null`; canario PASS siempre nivel 3; hash efectivo inesperado como integridad, nivel 3 y salida 3 (decisión del PO «Nivel 3»); la excepción del canario sin ejecución por `executionError` o gate ERROR (decisión del PO «Sí, confirmo»).

Caso límite que el verificador cubre aunque el arnés no lo resuelva: si coinciden un fallo de integridad y `dirty-after-run`, el script sale 4 con nivel 3. El verificador espera 3, marca la incoherencia y mantiene el nivel 3.
