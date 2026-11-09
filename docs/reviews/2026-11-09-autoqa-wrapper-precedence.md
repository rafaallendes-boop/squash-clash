# AutoQA — precedencia del envoltorio ante ERROR

9 de noviembre de 2026, Chile. Rama/worktree nueva **autoqa/wrapper-precedence** desde main **e18d3e169beb01569d11b1fbbce7f26d130670a1**. Fuente corregida y probada **34b28c46c4c37a11256da902b208cc15df621fcf**. PR propia lista para revisión de Claude coordinada por el PO, sin fusionar. Esta entrada registra solo esta entrega; no duplica el cierre de #36 consignado en #37.

gate: PASS

**Dos corridas reales consecutivas PASS/0/nivel1, dirty=false y árbol limpio**, sin checkout manual entre ellas ni después; **85/85 AutoQA**, 18/18 intentos PASS y canario FAIL en ambas. Pruebas del envoltorio sin Unity: **17 métodos / 32 fixtures GREEN**, sin errores. No se cambian Assets, ProjectSettings, Packages, producción, escenarios o criterios.

## Autorización y decisión del PO

Primer commit documental **9aa0136**, antes de tocar el script, registra el encargo del PO del 2026-11-09 en [aprobaciones](2026-10-25-first-shot-po-approvals.md). Corrige el origen de la pérdida de señales y resuelve la contradicción documental: **con integridad demostrada manda la precedencia 3 > 4 > 1 > 2: salida3/nivel3, gate ERROR y executionError.kind=dirty-after-run visible; sin integridad, salida4**.

El worktree nuevo no tenía device.env y el lanzador lo requiere. Se consultó esa restricción; el PO autorizó literalmente **«Sí, copiarlo sin cambios»** desde autoqa/mvp. Confirmación registrada en caa37da. Copia ignorada byte a byte, hashes coincidentes y valores sin modificar; el original no se cambió. No se completaron valores ni se usó iPhone. La excepción de preparación fue explícita, no una solución para rodear el límite.

## Defectos reproducidos y TDD

El envoltorio anterior archivaba el resumen, pero reemplazaba señales conocidas por null y recuentos por cero, y forzaba salida4/nivel2. Incluso con nivel3 previo, la rama dirty-after-run dejaba salida4. También se perdía el error previo del propio arnés al encadenar errores.

Pruebas versionadas: [scripts/tests/test_autoqa_wrapper.py](../../scripts/tests/test_autoqa_wrapper.py). Comando:

```sh
python3 scripts/tests/test_autoqa_wrapper.py
```

Usan unittest/biblioteca estándar y ejecutan **el script real**, sin reemplazar su lógica, en repositorios Git aislados bajo Logs/autoqa/wrapper-tests. Un proceso simulado escribe resúmenes/artefactos sintéticos y provoca las rutas de error; no invoca Unity ni representa resultados del juego. Los fixtures no tocan archivos del proyecto real. FIXTURE_DIR identifica la evidencia conservada.

Orden auditado: autorización documental → pruebas comprometidas en **80b457d** → fix **34b28c4**. Antes de modificar autoqa.sh se observó RED válido: **17 métodos, 21 fallos por aserción, cero errores**, en `Logs/autoqa/wrapper-tests/run-az91_9zj`. No significa «−4 pruebas aprobadas»: unittest cuenta subcasos fallidos dentro de los métodos. Tras el fix: **17 métodos y 32 fixtures correctos, cero fallos/errores**, en `Logs/autoqa/wrapper-tests/run-a6271isq`.

Un primer armado del fixture tenía dos errores de construcción/acceso; se corrigió el fixture y se repitió el RED **antes del fix del script**. Ese primer armado no se acredita como RED válido. Ambas salidas quedan preservadas.

| Caso mínimo | Antes: gate / salida / nivel | Después: gate / salida / nivel |
| --- | --- | --- |
| Canario PASS + tests | ERROR / 4 / nivel 2 | ERROR / 3 / nivel 3 |
| presetsUnchanged=false + tests | ERROR / 4 / nivel 2 | ERROR / 3 / nivel 3 |
| FAIL/1 del arnés + environment | ERROR / 4 / nivel 2 | ERROR / 4 / nivel 2 |
| Integridad + dirty-after-run | ERROR / 4 / nivel 3 | ERROR / 3 / nivel 3 |
| PASS + dirty-after-run | ERROR / 4 / nivel 2 | ERROR / 4 / nivel 2 |
| Arnés corrupto + tests | ERROR / 4 / nivel 2 | ERROR / 4 / nivel 2 |
| tests y después dirty-after-run, conservando arnés | ERROR / 4 / nivel 2 | ERROR / 3 / nivel 3 |

También se prueban todas las rutas tests/compilation/summary-schema/attempt-schema/artifact-path/incomplete-run/missing-summary posterior, los cuatro false de integridad, campos ausentes/no booleanos, schema desconocido, declaración previa nivel3 o salida3, guardia roja y controles anteriores PASS/FAIL/INCONCLUSIVE/canario/hash/missing-xml. Estos controles conservan exactamente sus triples gate/nivel/salida. Los casos sintéticos son inyecciones del contrato del envoltorio, no cambios de escenarios o expectativas del juego.

## Arreglo aplicado

El resumen original se carga y captura una vez, incluso si el error ocurrió antes de parsearlo en el camino normal. Solo aporta clasificación si es un objeto JSON legible con schema **autoqa-summary/1**. El archivo original y su informe se archivan sin alterarlos y mantienen referencias SHA-256.

- El gate del ERROR sigue perteneciendo al envoltorio: **ERROR**, o **FAIL** por guardia roja. No copia ni inventa un resultado nuevo de juego.
- Las integridades conocidas mantienen solo true/false; lo ausente o de otro tipo permanece null. **False domina** true/null posteriores. Una guardia roja demostrada por el envoltorio también domina un true anterior.
- Se conserva canary.observed; **PASS siempre implica nivel/salida3**. No se pierde una clasificación válida nivel3/salida3 ya declarada por el arnés. El nivel final es el máximo conocido y la salida aplica **3 > 4 > 1 > 2**.
- Las razones del arnés se acumulan sin duplicados, junto con las señales de integridad y el error visible del envoltorio. `report.md` muestra gate, **nivel y salida finales**, integridad, canario y razones.
- Las llamadas sucesivas no reemplazan el original por el ERROR intermedio. `executionError.previous` retiene la cadena, incluido el error inicial del arnés cuando existe.
- Un resumen corrupto o de otro schema se archiva, pero no se usa para elevar/bajar clasificación: integridades/canario quedan desconocidos salvo otra evidencia propia del envoltorio, como guardia roja.

### Recuentos y determinismo: origen explícito

El ERROR no evalúa recepciones: sus counts quedan en cero y determinism vacío. Se añade **harnessResults** con `source=summary.harness.json`, `scope=harness-only`, gate, counts y determinism del original. Si el original no es reconocido, harnessResults es null. Esto permite consultar el resultado anterior sin presentarlo como resultado del ERROR. Los datos originales completos siguen disponibles en el archivo archivado, con SHA.

README/OPERATIONS describen esta separación y la precedencia. La frase contradictoria de dirty-after-run fue corregida. No cambian las reglas del arnés ni su oráculo.

## Dos corridas Unity reales consecutivas

Desde árbol limpio y fuente 34b28c4, sin cambios versionados entre las corridas ni checkout manual entre/después:

| Corrida | Carpeta | Gate / salida / nivel | dirty | Duración externa | Git |
| --- | --- | --- | --- | ---: | --- |
| 1 | `Logs/autoqa/20261109T195229Z-34b28c4-10968` | PASS / 0 / nivel 1 | False | 42.210 s | limpio → limpio |
| 2 | `Logs/autoqa/20261109T195311Z-34b28c4-15860` | PASS / 0 / nivel 1 | False | 14.954 s | limpio → limpio |

Cada XML fresco contiene **85/85 Pass**, cero omitidos y sin CS; cada resumen contiene **18/18 PASS y canario FAIL**, integridades y determinismo correctos. restoration.json verifica la restauración automática selectiva, SHA previos/posteriores y estado Git completo vacío. La primera corrida incluye importación del worktree nuevo, aun así menor a 5 min.

**No se repiten regresión completa EditMode/PlayMode ni check-release**, conforme al encargo: únicamente scripts y documentación cambiaron; Assets/ProjectSettings/Packages quedan idénticos a main por SHA-256 (**584 archivos**). La suite AutoQA real sí es nueva y fresca. Los valores de device.env copiado autorizado coinciden con el original, sin cambios de contenido. Sin build Xcode, instalación, iPhone, cambios de producción, defaults, spec, planes, escenarios, expectativas, márgenes, tolerancias o comparación de determinismo.

## Compatibilidad opcional con #37, solo lectura

Al consultar origin/autoqa/claude-operator, su punta había avanzado a **9a3eb3782372efd8c8561ac013b828f019145af6**, con **autoqa-check v6**; el comando de extracción v5 en esa punta produjo un archivo vacío y no se ejecutó. Se extrajo sin modificarlo el **v5 histórico de 0652660**, y adicionalmente el v6 actual. Ningún archivo/rama/PR #37 se modificó. Ambos se ejecutaron con **python3 -I**, modo read, sobre las dos corridas reales y los siete casos mínimos.

**Ambos verificadores: cero incoherencias en las dos corridas reales**; gate y salida PASS/0, nivel1. En los fixtures mínimos, sus salidas esperadas coinciden con el envoltorio. Reportan incoherencias de datos porque los intentos de fixture son deliberadamente mínimos/sintéticos (ids synthetic-N, campos de intento incompletos y runId ausente) y no un autoqa-attempt de juego completo. En el caso corrupto también señalan el JSON archivado ilegible y el canario desconocido frente al intento sintético. No se presentan esos fixtures como corridas reales compatibles ni se altera el verificador para aceptarlos.

| Caso sintético | Incoherencias v5 / v6 | Salida esperada v5 / v6 |
| --- | --- | --- |
| Canario PASS + tests | 38 / 38 | 3 / 3 |
| presetsUnchanged=false + tests | 38 / 38 | 3 / 3 |
| FAIL/1 del arnés + environment | 40 / 40 | 4 / 4 |
| Integridad + dirty-after-run | 38 / 38 | 3 / 3 |
| PASS + dirty-after-run | 38 / 38 | 4 / 4 |
| Arnés corrupto + tests | 41 / 41 | 4 / 4 |
| tests y después dirty-after-run, conservando arnés | 38 / 38 | 3 / 3 |

Las listas completas y sus razones están en compatibility-results.json y en cada salida v5/v6 preservada. Los niveles del operador pueden elevarse a3 por esas incoherencias de formato de los fixtures; no cambian el gate ni reemplazan el resultado del envoltorio. No se detectó incoherencia en las corridas reales.

## Evidencia y huellas

Manifiesto local **`Logs/autoqa/wrapper-precedence/evidence-manifest.json`**, **1231 archivos**, SHA-256 **57057179b4bdd608cbffd343f10ce0a79bccb42eaf9fb63f2d9c13c5d53a2288**. Incluye pruebas versionadas ejecutadas, resúmenes/artefactos sintéticos, ambas corridas reales, registros de restauración, verificadores extraídos y sus salidas. Logs, artefactos, cachés y configuración local no se versionan.

| Principal | SHA-256 |
| --- | --- |
| `Logs/autoqa/wrapper-precedence/red-valid.txt` | `5602e0e64eca91418689d706b523cc19266e6877e15e9ac353957a95f5922005` |
| `Logs/autoqa/wrapper-precedence/green.txt` | `4b4982ad416b09f20d5d73b0c403fa2e84021c84ef2bf1bd611ede451fd63094` |
| `Logs/autoqa/wrapper-tests/run-az91_9zj/test-result.json` | `50ebb492a907c1de6a3f0b1a5c811975ef1302124d93e472a388ee3653730f7a` |
| `Logs/autoqa/wrapper-tests/run-a6271isq/test-result.json` | `2bfc575468ac0cbdc05b3eed912571e5d1bf220d857719df2763ba03c72cd40b` |
| `Logs/autoqa/wrapper-precedence/real-runs.json` | `d35ec8de45423e3448c5f084f140ac6098b03b7b15987b29b544a6095f0b3cce` |
| `Logs/autoqa/wrapper-precedence/compatibility-results.json` | `f9c9536459728210db642c826f279861256ac69f5bad0d2038ccd63477046564` |
| `Logs/autoqa/wrapper-precedence/protected-files.json` | `5a5d06e42664698d96bc7b3de5b85674f569c2a33be56da0672889afda613c99` |
| `Logs/autoqa/wrapper-precedence/evidence-manifest.json` | `57057179b4bdd608cbffd343f10ce0a79bccb42eaf9fb63f2d9c13c5d53a2288` |
| `Logs/autoqa/20261109T195229Z-34b28c4-10968/summary.json` | `d60a64148dfb13c4dd5c9cd2ae33f4a3e185f3396a0f01d06139344c2ae88ab7` |
| `Logs/autoqa/20261109T195229Z-34b28c4-10968/tests.xml` | `ca593f4b97f4576056a6dd8616d63403994bb989d360375cc456ab9286493c0d` |
| `Logs/autoqa/20261109T195229Z-34b28c4-10968/restoration.json` | `3f01656d6c9117bda8c32bf03aab57885ec8659b45ebaf4f138de808324dc8f5` |
| `Logs/autoqa/20261109T195311Z-34b28c4-15860/summary.json` | `4d9a2ce9cbd5f9365e0028ac1d568f815a1be064dd76521045080555d80d15bf` |
| `Logs/autoqa/20261109T195311Z-34b28c4-15860/tests.xml` | `36b0c559cae9757756568d7ad90693a33eab7ff4a70311daca7d49b9568b3ba4` |
| `Logs/autoqa/20261109T195311Z-34b28c4-15860/restoration.json` | `3f01656d6c9117bda8c32bf03aab57885ec8659b45ebaf4f138de808324dc8f5` |

## Entrega y límites

Solo se añade la entrada propia de esta entrega a AGENTS.md/aprobaciones; no se duplica el registro de cierre de #36 de la rama #37. No hay conflicto de código con main; si esas entradas documentales confluyen con #37 se conservan ambos historiales, sin modificar esa rama en esta entrega.

PR nueva abierta/lista para revisión de Claude coordinada por el PO, **sin fusionar**. Ninguna parada nueva ni aceptación del PO inferida. La preparación de configuración fue autorizada explícitamente y los primeros errores de fixture no se contaron como validación. Sin intervención en #32/#37, S3-validation, S4, First Rally o First Match; ningún mensaje externo enviado. Detenerse al entregar.

AutoQA Editor no valida audibilidad, háptica percibida, lectura visual, latencia toque→imagen, sensación o diversión ni sustituye sesiones iPhone del PO.
