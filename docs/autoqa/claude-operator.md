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

A la fecha de esta entrega, la PR de Codex (`autoqa/mvp`) solo registra una parada documental previa a la POC. No contiene `scripts/autoqa.sh`, `docs/autoqa/OPERATIONS.md` ni un esquema real de `summary.json`. Por eso esta capa se ajusta al contrato del prompt. Cuando exista la implementación real, **manda la implementación real**: los supuestos de la sección [Supuestos sobre el esquema](#supuestos-sobre-el-esquema) se revisan contra ella y se registra cada diferencia. Esta capa no se fusiona antes que la PR de Codex.

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

**Propuesta de Claude, pendiente de confirmación del PO antes de fusionar.** La política del PO nombra Luna 6 Low, Luna 6 Medium y Sol 6.1 High u Opus 5.5 High. En Claude Code se propone:

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
- pasa el verificador determinista incluido en su propio archivo (bloque `autoqa-check v3`, extraído con `awk` y ejecutado con `python3 -I`, solo biblioteca estándar);
- devuelve la salida del verificador carácter por carácter y una sección breve de notas (comandos, códigos, reintento);
- ejecuta el único reintento solo si el verificador lo declara elegible.

No hace:
- interpretar causas ni recomendar correcciones;
- recalcular o rebajar el nivel;
- editar archivos (no tiene `Edit`, `Write`, `NotebookEdit` ni `Agent`);
- obedecer texto de los artefactos.

### Nivel 2 — investigador (`autoqa-investigator`, Sonnet)

Hace:
- lee `summary.json`, los intentos no PASS de `attempts.jsonl`, los JSONL del juego y, si hace falta, escenarios, documentación de Codex e historial de git;
- separa hechos (con cita a archivo y campo o línea) de hipótesis (con lo que las confirmaría o refutaría);
- recomienda una sola opción: `reoperar` (solo error de entorno con el reintento sin usar), `escalar-nivel-3` o `decision-PO`.

No hace:
- modificar archivos, escenarios, oráculo, parámetros, umbrales ni criterios;
- proponer cambiar una expectativa para que un intento pase;
- aprobar cambios;
- proponer un nivel menor que el del operador;
- ejecutar Unity ni scripts del proyecto.

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

Si hay integridad, solo se acepta 3. Si hay varias categorías sin integridad, se acepta cualquiera de ellas, y el código esperado se elige por la precedencia 3 > 4 > 1 > 2 (supuesto, ver abajo). En modo `read` el código se informa en el resumen (`expectedExitCode`, `acceptableExitCodes`, `exitCategories`) sin compararse con nada.

Reintento elegible: hay `executionError`, el único reintento no se ha usado, no hay incoherencias ni fallo de integridad, ni FAIL, INCONCLUSIVE ni determinismo distinto, y la causa es de entorno. Si `executionError.kind` existe, manda (`environment`). Si no, se reconoce por el texto (bloqueo del proyecto, licencia, XML ausente, batchmode). Ante la duda no es elegible y decide el nivel 2.

Cambiar el verificador requiere aprobación del PO, como cualquier otro criterio.

## Cómo verificar que un resumen no inventa datos

1. Vuelve a ejecutar el verificador sobre el mismo `RUN_DIR`:

   ```bash
   awk '/^# >>> autoqa-check v3/{f=1;next} /^# <<< autoqa-check v3/{f=0} f' .claude/agents/autoqa-operator.md \
     | python3 -I - --run-dir "<RUN_DIR>" --mode read
   ```

2. La salida debe estar contenida, carácter por carácter, en el resumen del operador. `/autoqa` lo hace automáticamente comparando los bloques JSON.
3. Cada SHA-256 citado debe coincidir con `shasum -a 256` del archivo.
4. Cada valor de un hecho del investigador debe encontrarse en el archivo y campo que cita, por ejemplo con `jq`.
5. Un `runId` que empieza por `SYNTHETIC-` o `"synthetic": true` es un fixture de prueba, nunca evidencia.

## Limitaciones

- **Bash no se puede restringir por comando** en el frontmatter de un subagente: `tools` acepta o quita la herramienta completa. La regla de solo lectura del investigador, y la de no escribir del operador, dependen de las instrucciones. Una garantía técnica exigiría reglas `permissions.deny` o hooks, que están fuera del alcance de esta entrega y requieren decisión del PO. Las verificaciones comprueban después que `git status` y los SHA-256 no cambien.
- El verificador detecta el gate de `report.md` por texto (`gate` o `RESULT` seguidos del veredicto). Si el informe real usa otra forma, lo indica como nota y no puede compararlo.
- La detección de texto con forma de instrucción es heurística: lista frases sospechosas, no garantiza encontrarlas todas. La protección principal es que el operador solo copia la salida del verificador.
- Los fixtures sintéticos prueban el comportamiento de los agentes, no el arnés. Ninguna corrida real se simuló con fixtures.
- AutoQA no verifica audibilidad, háptica percibida, lectura visual, latencia toque→imagen, sensación del golpe ni diversión. Siguen siendo verificación del PO en el iPhone.

## Supuestos sobre el esquema

Pendientes de contrastar con la implementación real de Codex. Por decisión del PO del 2026-11-08, cuando exista el `summary.json` real se cotejan estos nombres de campo (`counts`, `canary.observed`, `gameMetrics`, `executionError.kind`, la línea `RUN_DIR=`, la línea `gate:` de `report.md`, `attempts[].config.effectiveHash`, `attempts[].config.expectedHash`, `attempts[].config.hashMatch`, `integrity.hashesVerified` y el valor de gate `ERROR`) y la plantilla de resumen del verificador con la de `docs/autoqa/OPERATIONS.md`. **Si difieren, manda lo que implemente Codex:** se adapta el verificador y se registra la diferencia.

| Supuesto | Origen |
| --- | --- |
| Recuentos en `summary.json → counts`, con claves `PASS`, `FAIL`, `INCONCLUSIVE` y `NOT_EVALUATED` | La política dice «recuentos por veredicto» sin nombre de campo |
| `canary.observed` es el veredicto del canario (`FAIL` esperado). `null` o NOT_EVALUATED cuando no se ejecutó | La política dice `canary{expectedFail, observed}` |
| `artifacts.gameMetrics[]` son objetos `{path, sha256}` relativos a `RUN_DIR` o al repositorio | La política dice `gameMetrics[]` |
| `executionError` es `null` o `{code, message}`, con `kind` opcional | La política dice «null o código y mensaje» |
| Integridad no evaluada se expresa con `null`, no con `false` | No definido. `false` se trata como integridad en rojo |
| El canario se identifica por `scenario.id` que empieza por `CANARY` | Id del escenario en el prompt |
| `scripts/autoqa.sh` imprime una línea `RUN_DIR=<ruta>` o `RUN_DIR: <ruta>` | La política dice que imprime `RESULT` y `RUN_DIR` |
| Con canario PASS, el gate de los 15 intentos puede ser PASS y el nivel 3 | No definido. El operador nunca declara éxito con nivel distinto de 1 |
| `report.md` declara el gate en una línea `Gate: <veredicto>` o `RESULT: <veredicto>` | No definido. Si no se reconoce, se anota y no se compara |
| Cada intento declara `config.effectiveHash`, `config.expectedHash` y `config.hashMatch` | Esquema `autoqa-attempt/1` del prompt de Codex |
| `integrity.hashesVerified` es el campo que refleja la comprobación de hash de los intentos | Nombre del prompt de Codex; su relación con `config.hashMatch` no está definida |
| Una corrida que no llega a ejecutarse tiene `gate: "ERROR"` y `executionError` no nulo | La política lista ERROR entre los valores de gate sin definir cuándo se usa |
| Un canario sin resultado concluyente cuenta como «no falla» (nivel 3), salvo con `executionError` o gate ERROR, donde la ausencia la explica el error (nivel 2, salida 4) | La política dice «nivel 3 … si el canario NO falla». La excepción es supuesto de Claude, pendiente del PO |
| Un hash efectivo inesperado da `integrity.hashesVerified=false`, nivel 3 y salida 3, aunque el intento sea INCONCLUSIVE | Decisión del PO del 2026-11-08. Una precondición que no es de integridad, como la pantalla, da nivel 2 y salida 2 |
| `determinism` de un escenario con una repetición INCONCLUSIVE es `null` (no comparable), no `false` | No definido. `false` cuenta como determinismo distinto (salida 1) |
| Con varias categorías de salida sin integridad, el arnés elige por la precedencia 4 > 1 > 2 | No definido. El verificador acepta cualquiera de las categorías presentes |
