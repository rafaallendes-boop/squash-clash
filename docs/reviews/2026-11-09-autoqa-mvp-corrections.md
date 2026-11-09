# AutoQA MVP — correcciones de la revisión de PR #36

9 de noviembre de 2026, Chile. Misma rama/worktree `autoqa/mvp` y [PR #36](https://github.com/rafaallendes-boop/squash-clash/pull/36), abierta/lista y sin fusionar. Head revisado por Claude: **bc82c3ee570df837142f423d736f6bf893ef7e40**. Fuente corregida y verificada: **a537236cf81c5345a5d8554f93654f55eabf0ab9** (fix de flujo 15a01cc y fix de script a537236). Base main/origin-main **27058e8**, sin cambios. Revisión independiente de estas correcciones por Claude, coordinada por el PO, pendiente.

gate: PASS

Dos corridas consecutivas desde árbol limpio: **ambas RESULT=PASS/salida 0/escalationLevel=1/dirty=false**, 18/18 PASS y canario FAIL; **85/85** tests cada una, cero omitidos y sin CS. Árbol limpio tras cada una, sin checkout manual entre ellas ni después. Regresión fresca: **546/546 EditMode y 354/354 PlayMode**. No hay nueva parada ni aceptación del PO inferida.

## Autorización literal y alcance

Primer commit documental de esta continuación: **8e187de**. Registra en [aprobaciones](2026-10-25-first-shot-po-approvals.md) la decisión literal del PO **«Sí, redáctalo»**, respuesta a la pregunta de Claude **«¿Redacto el prompt para que Codex corrija estos dos puntos?»**, tras la revisión independiente **CAMBIOS SOLICITADOS** sobre bc82c3e. Autoriza estos dos fixes y no cambia otras decisiones.

Claude había confirmado una corrida limpia PASS/0/nivel1, 76/76 y18/18/canario FAIL, en `Logs/autoqa/20261109T130148Z-bc82c3e-10259`; restauró manualmente GraphicsSettings/QualitySettings, revelando el segundo defecto. Esa evidencia se mantiene intacta.

Solo cambian el flujo de pruebas, nueve pruebas unitarias nuevas, scripts/autoqa.sh y documentación. Escenarios, expectativas, offsets ±10 ms, márgenes, holgura interna, hashes y comparación/tolerancia predeclaradas quedan byte a byte intactos frente a bc82c3e. **Report.Triage no cambia.** Los informes anteriores no se reescriben.

## Defecto 1 — regresión confundida con integridad

### Opción A aplicada

Antes, el primer FAIL normal o de determinismo activaba blocked y omitía también ZCanary. Su NOT_EVALUATED sin executionError disparaba la regla de integridad: nivel/salida 3. Una regresión debía conservar salida 1/nivel 2.

Ahora los escenarios normales posteriores siguen sin ejecutarse, pero **el canario se ejecuta en una instancia fresca si la parada solo fue un FAIL normal/de determinismo**. Ante guardia roja o executionError se omite porque Unity no es fiable. La guardia roja ya constituye integridad por sí sola. No hay una prohibición expresa y específica del canario en el prompt definitivo que obligue a usar B; el encargo de correcciones prescribe A, preservando la detención de los restantes escenarios normales.

`CanExecute` concentra esa decisión; el bucle usa el mismo método que prueban los tests. Con canario FAIL, el triaje existente produce gate FAIL/salida 1/nivel 2. Se mantienen los casos anteriores: canario PASS siempre nivel3 y canario ausente en corrida completa sin error sigue nivel3. No se cambia un esperado ni se vuelve a ejecutar un escenario normal detenido.

### RED y GREEN

- **RED de ejecución:** `canary-flow-red.json` y consola en `Logs/autoqa/corrections-20261109/`. Inyección local del booleano pass=false en la primera evaluación normal, sin cambiar escenario, configuración, entrada ni esperado. La aserción que exige canario FAIL/salida1/nivel2 falla: se obtuvo canario NOT_EVALUATED y salida3/nivel3. Primario: `Logs/autoqa/20261109T133025Z-8e187de-21128`.
- **RED unitario compilable:** método esqueleto con la política anterior `!blocked`; **82/85 Pass, 3 Fail**, por aserciones de ejecución de canario. XML del run `Logs/autoqa/20261109T133200Z-8e187de-1255/tests.xml`, sin CS.
- **GREEN focal:** `Logs/autoqa/tdd/corrections-flow-green.xml`, **9/9**, que cubren estado sano, detención normal, canario tras detención, guardia roja, executionError y triaje de FAIL/determinismo.
- **GREEN de ejecución:** inyecciones locales separadas para FAIL de oráculo y determinismo. En ambas el canario realmente recorre Main y produce Perfect, que contradice su esperado deliberado Failed: canario **FAIL**. Resumen **FAIL/salida1/nivel2**, con un FAIL normal y17 NOT_EVALUATED. Las fuentes de inyección se restauran byte a byte; no se entregan interruptores permanentes.

| Comprobación GREEN | Evidencia | Gate / salida / nivel | Canario |
| --- | --- | --- | --- |
| oracle-fail | `Logs/autoqa/20261109T133622Z-8e187de-24321` | FAIL / 1 / nivel 2 | FAIL |
| determinism-fail | `Logs/autoqa/20261109T133633Z-8e187de-29193` | FAIL / 1 / nivel 2 | FAIL |
| nunit-preservation | `Logs/autoqa/20261109T133645Z-8e187de-24592` | ERROR / 4 / nivel 2 | None |

Las inyecciones son pruebas del flujo, no regresiones medidas del producto ni cambios aceptados del oráculo/determinismo. La regla nueva no relaja ningún criterio de contacto.

## Defecto 2 — transitorios y trazabilidad de corridas consecutivas

Antes de Unity, el script captura el estado completo de `git status --porcelain -z`, y para cada uno de GraphicsSettings.asset/QualitySettings.asset captura estado y SHA-256, considerando tanto índice como worktree respecto de HEAD.

En `finally`, incluso con ERROR o timeout, hace `git checkout --` **exclusivamente de cada uno de esos dos archivos que estuviera limpio antes**. Los previamente modificados no se restauran. Ningún otro archivo se restaura. `restoration.json` guarda estados/hash previos y posteriores, operaciones y errores, y el estado completo de Git en base64 para comparación exacta. summary.artifacts.restoration lo referencia con SHA-256.

Si el estado final completo difiere del inicial o la restauración falla, el control final prescrito devuelve **ERROR/salida4/executionError.kind=dirty-after-run**. No puede quedar como PASS. Conserva un nivel3 previo, si lo había, y la evidencia del resultado anterior. No hay limpieza de cambios ajenos para hacer pasar el control.

### RED y pruebas del script

- **RED válido de comportamiento:** `cleanup-red.json`. Desde árbol limpio el script anterior devolvió PASS/salida0, pero dejó ambos transitorios modificados. Falló la aserción de igualdad del estado previo/posterior. Primario: `Logs/autoqa/20261109T133037Z-8e187de-26096`.
- La prueba principal GREEN son las **dos corridas reales consecutivas** de la sección siguiente; no se hizo limpieza manual entre ellas ni después.
- Para las rutas ERROR/timeout se ejecutó además `shell-fixtures.py`, con repositorios Git **aislados bajo Logs/autoqa**, archivos sintéticos y sin Unity. No escribe los ProjectSettings reales ni representa mediciones del juego. Comprueba restauración selectiva, preservación de un cambio previo y detección de un cambio ajeno que el script deja intacto.

| Fixture aislado | Salida / executionError.kind | Estado Git sin cambio | Aserciones |
| --- | --- | --- | --- |
| environment | 4 / missing-xml | True | PASS |
| timeout | 4 / environment | True | PASS |
| preexisting-dirty | 4 / environment | True | PASS |
| dirty-after-run | 4 / dirty-after-run | False | PASS |

En `preexisting-dirty` GraphicsSettings no se restaura (su contenido previo se conserva), QualitySettings sí; dirty ya es true. En `dirty-after-run`, el archivo ajeno permanece modificado y la salida es4. La primera preparación de fixture omitía ignorar Logs y fue rechazada correctamente por cambio de estado; se corrigió el setup del fixture, sin modificar lógica del runner ni contar ese intento como GREEN.

## Observación opcional — resumen previo preservado

Se adoptó por su tamaño pequeño. Antes de sustituir un resumen existente ante un error se conserva **summary.harness.json**, y el informe previo en **report.harness.md** cuando existe. El resumen de ERROR los referencia con ruta/SHA-256. El resultado operativo sigue siendo ERROR; el resumen previo no autoriza aprobar la corrida.

RED: la corrida con los tres fallos unitarios anteriores perdió el resumen del arnés, y falló la aserción de existencia de summary.harness.json (`flow-unit-and-preservation-red.json`). GREEN: un fallo NUnit local explícito conserva el resumen original **PASS/18 intentos/canario FAIL**, mientras el wrapper devuelve **ERROR/salida4/nivel2**, kind=tests; se cotejaron los hashes del resumen, informe y registro de restauración. No es un fallo del juego. Código de prueba temporal restaurado byte a byte.

## Dos corridas consecutivas desde árbol limpio

Se ejecutaron inmediatamente una después de la otra, desde fuente a537236, sin cambios de archivos versionados ni checkout manual entre ellas. Cada carpeta es nueva y conserva su propio XML, summary, attempts, report, JSONL del juego y restoration.json.

| Corrida | Carpeta | dirty | RESULT / salida | Nivel | Duración externa | Árbol |
| --- | --- | --- | --- | ---: | ---: | --- |
| 1 | `Logs/autoqa/20261109T134237Z-a537236-28153` | False | PASS / 0 | 1 | 14.905 s | limpio → limpio |
| 2 | `Logs/autoqa/20261109T134252Z-a537236-4896` | False | PASS / 0 | 1 | 14.341 s | limpio → limpio |

Ambas: **85/85 tests**, 18/18 escenarios PASS, canario FAIL, integridad/determinismo correctos. En los dos registros, ambos archivos estaban limpios al inicio, fueron restaurados con código0, y SHA antes/después coincide; el estado Git completo sigue vacío. El SHA de restoration.json coincide entre las dos porque sus estados/operaciones son idénticos. Resúmenes y métricas son independientes y tienen sus propios hashes.

## Regresión y límites de verificación

Regresión completa nueva sobre la fuente corregida: **546/546 EditMode y354/354 PlayMode**, XML/logs en `Logs/autoqa/corrections-20261109/regression/`, cero omitidos/sin CS. Los transitorios del script test.sh (que no cambió) se restauraron una vez para preparar el árbol limpio **antes** de las dos comprobaciones de autoqa.sh. Esa preparación no se presenta como restauración del script corregido.

**No se repite check-release dev/prod** conforme al encargo: solo cambiaron tests, su script y documentos. El ensamblado sigue marcado TestAssemblies y excluido de Players; asmdef, Core, Runtime, DevPanel, Editor, Resources y Packages no cambian. Los exports previos de la entrega anterior siguen siendo históricos, no se presentan como nuevas ejecuciones.

**312 archivos protegidos cotejados por SHA-256 contra main, todos idénticos** después de las dos corridas, sin checkout adicional. Catálogo, Scenario/Expected, Oracle, Report.Triage/serialización del arnés, normalizador y predeclaración se conservaron. El hash de device.env coincide con el de la entrega anterior: **no se modificó en esta entrega** y ningún comando necesitó completarlo. Sin iPhone, devicectl, xcodebuild, build/instalación ni cambios en #32/#37/S3-validation/S4/First Rally/First Match.

README y OPERATIONS explican el canario tras un FAIL, las excepciones por estado no fiable, la restauración selectiva y dirty-after-run, así como los resúmenes previos preservados. La plantilla del operador y los criterios de juego permanecen intactos.

## Evidencia y huellas

Manifiesto local **`Logs/autoqa/corrections-20261109/evidence-manifest.json`**, **332 archivos**, SHA-256 **d08ba3df9de1476f1f0da502c66739a95f21219a255f89fd7585a02abf4d656c**. Incluye primarios de RED/GREEN, inyecciones y sus restauraciones, fixtures aislados, ambas corridas reales y regresión. No se versionan logs, cachés, builds, datos de dispositivo ni credenciales.

| Artefacto | SHA-256 |
| --- | --- |
| `Logs/autoqa/corrections-20261109/canary-flow-red.json` | `df15aafb1b64c63b16a03f715ef069f25ffe1b5c4cc7e2d79a94bd9c13b4ee52` |
| `Logs/autoqa/corrections-20261109/cleanup-red.json` | `a545dd95eac7fa64343b9c95ceaf7e12699d053fd9a4f4995e15812cf8e64f48` |
| `Logs/autoqa/corrections-20261109/flow-unit-and-preservation-red.json` | `f3dba3819bc87f1836c7c38a440de81d7a2d3643c15d64c7f1df645a362edab1` |
| `Logs/autoqa/tdd/corrections-flow-green.xml` | `0803288f71c19138cda4fa48e610ee076aa54aac8ff609359fb3210377690db5` |
| `Logs/autoqa/corrections-20261109/green-injections.json` | `9d21ba92900b70649a792f3a64ffa90b75d6390040f6af70218c41e8c9c53c8d` |
| `Logs/autoqa/corrections-20261109/shell-fixtures-results.json` | `a137a037d87d614d6cefee6bd7e5ce45cef997fbbe764bfb2b0b558db56b65bd` |
| `Logs/autoqa/corrections-20261109/consecutive-runs.json` | `af83c689ac456d0673f4a60345e3261670e9deba2998e70f7929c45d25e21e2e` |
| `Logs/autoqa/corrections-20261109/protected-final.json` | `41c4903fa7b58d7c3c0e7693a068ac7434cac53f54c004842e41024b19ab1517` |
| `Logs/autoqa/corrections-20261109/frozen-contract-and-env.json` | `215e2a8d2f5d4d1124041763404f8dbc3ce90b80dbf5730780ec55cb67efbaaa` |
| `Logs/autoqa/corrections-20261109/regression/editmode-results.xml` | `aec18baa55b256ec79c14c32e0d702b72be75ba7f29b49f2700b1cfd06e2444d` |
| `Logs/autoqa/corrections-20261109/regression/playmode-results.xml` | `66a444418a69863b97f77ead99c336c6ebf7836301538c4cd49dd745c177f0e0` |
| `Logs/autoqa/corrections-20261109/evidence-manifest.json` | `d08ba3df9de1476f1f0da502c66739a95f21219a255f89fd7585a02abf4d656c` |
| `Logs/autoqa/20261109T134237Z-a537236-28153/summary.json` | `2efd7d60d0ef7edbdcb45d77eb31098f925fdb5f45fb567dbf111cbdc8b316f5` |
| `Logs/autoqa/20261109T134237Z-a537236-28153/restoration.json` | `3f01656d6c9117bda8c32bf03aab57885ec8659b45ebaf4f138de808324dc8f5` |
| `Logs/autoqa/20261109T134252Z-a537236-4896/summary.json` | `2f3db027f3a40be15f5997c746be50ee3ac1ebe62f42b7ff9db47a3341345704` |
| `Logs/autoqa/20261109T134252Z-a537236-4896/restoration.json` | `3f01656d6c9117bda8c32bf03aab57885ec8659b45ebaf4f138de808324dc8f5` |

## Entrega

Opción **A**, dos defectos corregidos y observación opcional realizada. No se cambiaron escenarios, expectativas, márgenes, tolerancias ni criterios; ninguna parada nueva. Solo se registró el problema de setup del fixture aislado, sin atribuirlo al producto. El gate PASS es del arnés Editor/Mono, no aceptación física o del PO.

**PR #36 abierta/lista y sin fusionar**, para nueva revisión independiente de Claude (Opus 5.5 High), coordinada por el PO. Ningún mensaje externo enviado. La fusión requiere aprobación explícita posterior del PO; detenerse al entregar. Audibilidad, háptica percibida, lectura visual, latencia toque→imagen, sensación y diversión siguen fuera de AutoQA y requieren sesión física autorizada aparte.
