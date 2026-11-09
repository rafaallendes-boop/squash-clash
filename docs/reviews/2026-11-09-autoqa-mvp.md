# AutoQA MVP — entrega para revisión independiente

9 de noviembre de 2026, Chile. Misma rama/worktree `autoqa/mvp` y [PR #36](https://github.com/rafaallendes-boop/squash-clash/pull/36). Base local/remota `27058e82674f633879a7a4f7cabc4f1306a6db6b`, sin cambios. Fuente de la corrida y exports: **491e10d**. Revisión independiente de Claude, coordinada por el PO, **pendiente**; ninguna aceptación del PO ni fusión inferidas.

gate: PASS

Corrida final `20261109T120909Z-491e10d-19260`: **18/18 PASS**, canario **FAIL** separado, determinismo e integridad correctos. `scripts/autoqa.sh mvp`: **RESULT=PASS, salida 0, escalationLevel=1**, executionError=null. Tiempo externo de comando completo **14.497 s**, menor a 5 min; no solo el tiempo de las recepciones. NUnit ejecutó **76/76**, cero omitidos, XML fresco y sin CS. El canario falla en el oráculo, no como error del test de ejecución.

## Alcance, decisiones y resolución de AUTOQA-MVP-01

Se conservan sin reescribir los [informes del 6](2026-11-06-autoqa-mvp.md) y [8](2026-11-08-autoqa-mvp.md). La autorización inicial, decisiones del 8 y nueva decisión del 9 están en [aprobaciones](2026-10-25-first-shot-po-approvals.md). La copia del estudio conserva SHA-256 `fac103f0bbdc5a3777103de8d59118a52707fea2eb83aabede3fb03410b386b0`.

AUTOQA-MVP-01 quedó resuelto por los literales del PO del 8: **«Sí, a SL5»** (margen sobre ±A/±V y KIn/KOut; PERFECT/canario a SL5) y **«Nivel 3»** (hash inesperado es integridad/salida 3, aunque el intento sea INCONCLUSIVE). Confirmación **«Sí, confirmo»** sobre canario: no FAIL ejecutado implica integridad; si no llegó a ejecutarse por executionError/gate ERROR, su ausencia no implica por sí sola integridad. Canario PASS siempre nivel 3. Implementado y probado en TDD; el INCONCLUSIVE forzado usa pantalla, no hash.

## Resolución de AUTOQA-MVP-02

Primer commit documental de esta continuación: **9c476bc**. Registra literalmente la decisión del PO del 2026-11-09: **«Sí, ±10 ms»**, respuesta a «¿Cambiamos PERFECT a Δ=−10 ms y añadimos otro caso PERFECT a +10 ms?», y todas sus consecuencias. Los nombres elegidos son **M24-PERFECT-EARLY10-SL5** y **M24-PERFECT-LATE10-SL5**. El canario usa EARLY10 y mantiene el esperado deliberadamente falso Failed. Los otros cuatro escenarios no cambian. Ahora son seis escenarios × tres = 18, más un canario separado.

La lectura previa de ContactContract confirma que ±10 cae dentro de ±40 y de KIn/KOut SL5; corrected<=target decide Scheduled=true/false. Se verificó en los resultados reales, sin llamar al contrato desde el oráculo. La lectura de Claude sobre origen absoluto y redondeo es coherente con la rama observada; **no se rastreó la conversión interna de tiempo en InputSystem y no se presenta como causa demostrada**, ni se declara defecto del producto. La frontera interna Δ=0 exige la holgura PO de **1 ms**, no dos pasos. Ningún redondeo se usa para decidir categorías.

### Distancias recalculadas y hashes

Paso real `1000/120/0,65 = 12,820512820513 ms`; margen exterior de dos pasos **25,641025641026 ms**. A=40, V=100, latencia compensada=0. SL1: KIn−T*=−268,143726069 y KOut−T*=+6,086025371 ms (primario de base preservado). SL5: −239,529057599 y +245,883247689 ms (primarios nuevos).

| Caso | Δ ms | Distancia −A / +A ms | Distancia −V / +V ms | Distancia KIn / KOut ms | Frontera interna | Mínimo exterior |
| --- | ---: | --- | --- | --- | ---: | ---: |
| PERFECT-EARLY10 SL5 y canario | −10 | 30 / 50 | 90 / 110 | 229,529058 / 255,883248 | 10 ≥ 1 | 30 ≥ 25,641026 |
| PERFECT-LATE10 SL5 | +10 | 50 / 30 | 110 / 90 | 249,529058 / 235,883248 | 10 ≥ 1 | 30 ≥ 25,641026 |
| VALID-EARLY SL1 | −70 | 30 / 110 | 30 / 170 | 198,143726 / 76,086025 | N/A | 30 ≥ 25,641026 |
| FAILED-LATE SL1 | +170 | 210 / 130 | 270 / 70 | 438,143726 / 163,913975 | N/A | 70 ≥ 25,641026 |

NOPULL/REJECTED no producen una suelta Swing dirigida a una ventana de contacto. NOPULL dura 300 ms, supera tapMaxMs=250 y no sale de la zona muerta. Conserva AbandonedNoPull. En SL1 +170 supera KOut por 163,913975 ms: el código devuelve Failed/Late, sin cambiar el esperado.

Hashes esperados declarados desde defaults: **SL1 c0d8ec7050e0 / SL5 55a85f500cf1**, SimulationKey **166a96e319c7**. Seleccionar `practice.launch=4` permite que la propia receta aplique human.z=(double)1.6f; no lo escribe AutoQA. Δ no cambia configuración/hash. Todos los intentos verifican hash/SimulationKey/P2/M2-4/pending=0/pantalla/ancla/UI antes de inyectar.

### POC nueva y comparación predeclarada

[Determinismo](../autoqa/determinism.md), comprometido en **9c476bc antes de la POC**: todos los campos semánticos del registro enumerados se comparan exactamente, incluidos Scheduled, geometría, hashes, saltos y conteos. Solo la lista explícita de relativos temporales admite **≤10⁻⁶ ms**. Se excluyen los GUID, los tiempos absolutos, costes de máquina y metadata de envoltura listados y justificados; originales conservados. No se añadieron exclusiones tras ver los resultados.

La aclaración de ceros centinela NoOrder se comprometió en **ddc5c9a antes de ejecutar el MVP**: sin TargetContactAtReal no se fabrica duración restando a cero un reloj absoluto. Se mantienen el registro original, los campos excluidos ya declarados y los relativos K del shot-plan. No añade tolerancias ni exclusiones.

POC nueva: **2/2 UnityTests**, tres instancias de −10 y tres de +10, Main real/GameView 1206×2622, ancla segura y bloqueador real sin reemplazos. Ambos Perfect/Applied/una presentación y predicción bit a bit; Scheduled true en las tres EARLY10 y false en las tres LATE10. Primer FrontWall de ambos a ≈2,38e−7 / 3,37e−7 m de TargetUsed, dentro de 0,05 m. Su comparación completa pasó. Artefactos de Δ=0 del 8 intactos y con SHA cotejados; no se sobrescribieron.

## TDD e integración

XML y logs locales en `Logs/autoqa/tdd/`, todos frescos y sin errores CS. Los RED acreditados fallan por aserciones con fuentes compilables, antes del GREEN:

| Bloque | RED válido | GREEN |
| --- | --- | --- |
| POC ±10 | `poc-v2-red-valid.xml`: 0/2 Pass; 2 Fail | `poc-v2-green.xml`: 2/2 Pass; 0 Fail |
| Guardia | `guard-red.xml`: 9/10 Pass; 1 Fail | `guard-green.xml`: 10/10 Pass; 0 Fail |
| Oráculo | `oracle-red.xml`: 4/20 Pass; 16 Fail | `oracle-green.xml`: 20/20 Pass; 0 Fail |
| Triaje/informe | `report-red.xml`: 0/15 Pass; 15 Fail | `report-green.xml`: 15/15 Pass; 0 Fail |
| Determinismo | `determinism-red.xml`: 2/7 Pass; 5 Fail | `determinism-green.xml`: 7/7 Pass; 0 Fail |
| Driver | `driver-red.xml`: 0/1 Pass; 1 Fail | `driver-green.xml`: 1/1 Pass; 0 Fail |
| Escritura de registros | `guard-record-red.xml`: 10/12 Pass; 2 Fail | `guard-record-green.xml`: 12/12 Pass; 0 Fail |

El primer intento de esqueleto POC (`poc-v2-red.xml`) falló por construir el prefijo de ruta con doble separador; **no cuenta como RED funcional**. Corregido ese detalle del arnés, `poc-v2-red-valid.xml` falla porque no se entrega ningún gesto y LastRecord es null. El RED de guardia contiene una llamada temporal prohibida `controller.Enqueue(default)` en GuardProbe, eliminada antes del GREEN. Los RED de registros prueban que escribir Result/Applied no puede pasar la guardia.

Driver y ScriptedPlayer están separados. Preparación en memoria; cuatro componentes desactivados; orden fijo autorizado now→toques→InputSystem.Update→Host.Advance→GesturePreview.Tick→PracticeMode.Tick→ProcessPending→FlushRecords. Se espera el cierre real sin orden en negativos y el paso del primer contacto de la predicción saliente en aplicados. No se evalúa la trayectoria posterior al primer frontis ni se corrige S2-OBS-01. Cada instancia conserva el JSONL del juego antes de destruirse; LaunchId y AttemptId salen del juego y sus correlaciones se comprueban.

## Escenarios de la corrida final

| Id | Esperado | Observado / gate del escenario | Máxima diferencia temporal entre repeticiones (ms) |
| --- | --- | --- | ---: |
| M24-FAILED-LATE-SL1 | Swing/Failed, Applied=False, Scheduled=N/A | Swing/Failed; 1 presentación(es); 3/3 PASS | 2.64321897703e-12 |
| M24-NOPULL-SL1 | AbandonedNoPull/NoOrder, Applied=False, Scheduled=N/A | AbandonedNoPull/NoOrder; 0 presentación(es); 3/3 PASS | 1.7763568394e-12 |
| M24-PERFECT-EARLY10-SL5 | Swing/Perfect, Applied=True, Scheduled=True | Swing/Perfect; 1 presentación(es); 3/3 PASS | 1.76214598469e-12 |
| M24-PERFECT-LATE10-SL5 | Swing/Perfect, Applied=True, Scheduled=False | Swing/Perfect; 1 presentación(es); 3/3 PASS | 1.76214598469e-12 |
| M24-REJECTED-SL1 | Rejected/NoOrder, Applied=False, Scheduled=N/A | Rejected/NoOrder; 0 presentación(es); 3/3 PASS | 1.7763568394e-12 |
| M24-VALID-EARLY-SL1 | Swing/Valid, Applied=True, Scheduled=N/A | Swing/Valid; 1 presentación(es); 3/3 PASS | 1.81898940355e-12 |
| CANARY-WRONG-EXPECTATION | Failed deliberado; gesto EARLY10 real | Perfect/Applied/Scheduled=true; **FAIL esperado**, fuera de recuentos | N/A |

Los dos PERFECT cumplen primer FrontWall ≤0,05 m y PredictionMatchesBitwise. VALID cumple Applied y presentación Valid. FAILED-LATE conserva Fail=Late/no Applied/presentación Failed y la predicción entrante íntegra. NOPULL/REJECTED no consumen, cierran NoOrder y no tienen presentaciones. Discrepancias cero en todos los casos. Las seis comparaciones exactas semánticas pasaron; máxima diferencia temporal global **2.64321897703e-12 ms**, inferior a la tolerancia predeclarada.

## Regresión, Players e integridad

No se repitió la base al inicio porque main no cambió. Tras implementar, regresión completa fresca: **546/546 EditMode y 354/354 PlayMode**, cero omitidos/sin CS. AutoQA tiene su propio ensamblado y runner; no modifica scripts/test.sh. Suite propia final **76/76** (54 pruebas de guardia/oráculo/informe/determinismo, 2 POC, 1 driver, 19 ejecuciones de escenario/canario).

`scripts/check-release.sh` dev/prod pasó desde árbol limpio, fuente 491e10d, `gitDirty=false` en ambos metadata. Solo exports Unity/IL2CPP/Xcode, **sin xcodebuild, build nativa, firma, instalación ni iPhone**. Módulos:

| Perfil | Fuente / limpieza | Módulos IL2CPP del producto |
| --- | --- | --- |
| dev | 491e10d / gitDirty=False | g_SquashClash_Core_CodeGenModule, g_SquashClash_DevPanel_CodeGenModule, g_SquashClash_Runtime_CodeGenModule |
| prod | 491e10d / gitDirty=False | g_SquashClash_Core_CodeGenModule, g_SquashClash_Runtime_CodeGenModule |

AutoQA y Unity.InputSystem.TestFramework no aparecen en Players. Los puentes nativos térmico/háptico y símbolos requeridos pasaron el check existente. El primer export dev se detuvo porque el device.env ignorado del worktree no tenía BUNDLE_ID. Se diagnosticó contra ProjectSettings intacto, se suministró ese mismo valor temporalmente al entorno local y se ejecutó de nuevo el check sin cambiar el script. El archivo local se restauró byte a byte, con huellas en `release-20261109-corrected-env/local-env-restoration.json`; valores personales no se exportan al informe. El primer fallo y sus logs se preservan separados. No hizo falta modificar producto ni añadir seams.

**312 archivos protegidos cotejados por SHA-256 contra main, todos idénticos.** Se restauraron GraphicsSettings/QualitySettings transitorios antes de exports; BuildInfo generado/metadata se devolvieron al estado anterior. Defaults, presets de producto, spec, planes, criterios y umbrales intactos. Editor presets antes/después tienen la misma huella (`presets-integrity.json`); no equivale a validar P0 en iPhone. Todos los JSONL copiados se verificaron por SHA-256. Búsqueda de clientes de red/IA en fuente: solo nombres de la lista de prohibidos en la guardia, sin clientes, claves ni dependencia de IA en el bucle.

## Triaje y salidas verificadas

Precedencia **3 > 4 > 1 > 2**. La corrida final real acredita 0/PASS/nivel 1. Las inyecciones siguientes son **locales, sin commit**, separadas de mediciones del juego, restauradas por SHA-256. No cambian criterios para obtener PASS ni son reintentos de fallos reales:

| Inyección | Gate | Salida | Nivel | Carpeta |
| --- | --- | ---: | ---: | --- |
| canary-pass | FAIL | 3 | 3 | `Logs/autoqa/20261109T120528Z-491e10d-22213` |
| screen-precondition | INCONCLUSIVE | 2 | 2 | `Logs/autoqa/20261109T120542Z-491e10d-9435` |
| report-fail | FAIL | 1 | 2 | `Logs/autoqa/20261109T120557Z-491e10d-4834` |
| unexpected-hash | FAIL | 3 | 3 | `Logs/autoqa/20261109T120612Z-491e10d-25404` |
| environment | ERROR | 4 | 2 | `Logs/autoqa/20261109T120626Z-491e10d-330` |

`screen-precondition` fuerza solo el DTO de precondición de un intento; no es una resolución física medida. `report-fail` fuerza un veredicto del informe después de ejecutar los escenarios, no un fallo del producto. `unexpected-hash` fuerza la expectativa de integridad de un intento. `environment` sustituye únicamente en una copia local del script el ejecutable por false y no lanza Unity. Canario ausente en ese ERROR no causa nivel 3; integridades no evaluadas quedan null. Los fuentes se restauraron byte a byte; no quedan interruptores de inyección en el código entregado.

## Alineación con Claude

| Propuesta | Estado final |
| --- | --- |
| Margen inválido NOT_EVALUATED / scenario-invalid-margin | Adoptada |
| counts / canary.observed / gameMetrics lista path+sha256 / executionError.kind | Adoptada en summary.json |
| RUN_DIR= y RESULT= | Adoptada; además ESCALATION_LEVEL= |
| report.md con gate explícito | Adoptada |
| Gate definido con canario PASS/nivel 3 | Adoptada y comprobada en corrida forzada |
| Plantilla única OPERATIONS/verificador #37 | Archivo canónico operator-summary-template.md; OPERATIONS lo referencia. #37 no se modifica; debe consumir ese mismo archivo |
| Precedencia 3 > 4 > 1 > 2 | Adoptada, TDD y salidas forzadas |
| Semántica exacta, tiempos relativos tolerados y POC ampliada | Adoptadas y predeclaradas antes de POC; listas en determinism.md |

[README](../autoqa/README.md) y [OPERATIONS](../autoqa/OPERATIONS.md) describen contrato, orden, aislamiento, política del PO/modelos según disponibilidad, niveles, códigos, prohibiciones, único reintento por executionError de entorno, limitaciones y reversión. No se construyó un orquestador ni se editó la PR #37.

## Prueba de consumibilidad

Instancia rellenada de la [única plantilla](../autoqa/operator-summary-template.md), construida **solo leyendo summary.json y attempts.jsonl de la corrida final**, sin código ni logs de Unity:

```text
RunId: 20261109T120909Z-491e10d-19260 · gitHash: 491e10d8c229edf71629894767bec21e347420a7 · dirty: False
Comando: scripts/autoqa.sh mvp
Gate: PASS · recuentos: {'PASS': 18, 'FAIL': 0, 'INCONCLUSIVE': 0, 'NOT_EVALUATED': 0} · canario esperado FAIL: True · observado: FAIL
Determinismo: {'M24-FAILED-LATE-SL1': {'identical': True}, 'M24-NOPULL-SL1': {'identical': True}, 'M24-PERFECT-EARLY10-SL5': {'identical': True}, 'M24-PERFECT-LATE10-SL5': {'identical': True}, 'M24-REJECTED-SL1': {'identical': True}, 'M24-VALID-EARLY-SL1': {'identical': True}}
Integridad: {'hashesVerified': True, 'presetsUnchanged': True, 'guardPassed': True, 'evidenceSha256Ok': True}
EscalationLevel: 1 · razones: [] · executionError: None
Artefactos: Logs/autoqa/20261109T120909Z-491e10d-19260/summary.json; attempts.jsonl SHA-256=233c738bfee4bfd361d8d8239cba190fca0048052730600765609ed8f937e2a3; report.md SHA-256=fdc79df942193fc1720536f80f4b0662982b9d97d026efa024a3886910c5a35a
JSONL del juego: game-metrics/M24-PERFECT-EARLY10-SL5-1.jsonl SHA-256=be01b75a890871a6dd924d97c831296ceb626193288ea72356792db5a5de51ee
JSONL del juego: game-metrics/M24-PERFECT-EARLY10-SL5-2.jsonl SHA-256=2f55a72f368fd9d17b4ea321e3c757dd02cb9ea54f9e4e11251e5aba57a3a308
JSONL del juego: game-metrics/M24-PERFECT-EARLY10-SL5-3.jsonl SHA-256=6d0027499c30bb3b1ae8976f0abb00902c1ec13650da08862d61e37d387d4f65
JSONL del juego: game-metrics/M24-PERFECT-LATE10-SL5-1.jsonl SHA-256=86c59a89a9472938b1f438d34082da2d629b6429c36cf3ced22d5095669bb7c9
JSONL del juego: game-metrics/M24-PERFECT-LATE10-SL5-2.jsonl SHA-256=49b61be8043d0b0f7e8d1e53e7cef76ce72c4aa0462493e1bd2d0045af6f53f6
JSONL del juego: game-metrics/M24-PERFECT-LATE10-SL5-3.jsonl SHA-256=c944f70ec22d558dec728b4e172045986b9b854872933e31119cfa1c03340331
JSONL del juego: game-metrics/M24-VALID-EARLY-SL1-1.jsonl SHA-256=662ee83babd42216ff7c843dc97bdd76b016aa05d69619ef82c4cacbfc70ac7a
JSONL del juego: game-metrics/M24-VALID-EARLY-SL1-2.jsonl SHA-256=c44dd1d42b7befaa20e93d0bfa7df6db7d283a5a82ad9bd36bf8d9acbe665be8
JSONL del juego: game-metrics/M24-VALID-EARLY-SL1-3.jsonl SHA-256=9c069262ad97cfba18460396283f76697c3d9757a1f590efd01eb6ed34b8c047
JSONL del juego: game-metrics/M24-FAILED-LATE-SL1-1.jsonl SHA-256=44b0d46b09d85f8bd9ae1f30726a1ad396a9d09880346f04090df195bc26c5d3
JSONL del juego: game-metrics/M24-FAILED-LATE-SL1-2.jsonl SHA-256=855ddc4889812509b4bff5c60a90f0e5c49f1426008fd68f44f021a605e62b34
JSONL del juego: game-metrics/M24-FAILED-LATE-SL1-3.jsonl SHA-256=e39c515472cebfb6816357135298d8e4dd6a3932f0769ac739fed287b232a8df
JSONL del juego: game-metrics/M24-NOPULL-SL1-1.jsonl SHA-256=970d6ae46e350a95ec1440f7aa79a62e18723192b9dec4eb8d1737cddb0fb445
JSONL del juego: game-metrics/M24-NOPULL-SL1-2.jsonl SHA-256=b7f856802a6ea33788b074a495893842a84708fc39eca30d26bb9cebfa233c12
JSONL del juego: game-metrics/M24-NOPULL-SL1-3.jsonl SHA-256=79696587426fc093f589a5c36e27a39b0349da44a5bd3507bcef244132dc8c72
JSONL del juego: game-metrics/M24-REJECTED-SL1-1.jsonl SHA-256=5171845c4031a3959ea008e770cb9ec7efab66882b58a0c421f076530c9b04b4
JSONL del juego: game-metrics/M24-REJECTED-SL1-2.jsonl SHA-256=be98d7b87842dccb14819dd3da32899b341dec10a1aaa3f625fdf8d9de3ac859
JSONL del juego: game-metrics/M24-REJECTED-SL1-3.jsonl SHA-256=fc8f301c57f7f79e2251a1f496b8e91384e943ec30ec25e9f273c79040ca3067
JSONL del juego: game-metrics/CANARY-WRONG-EXPECTATION-1.jsonl SHA-256=b75c1eba62dfcc2efe8d5b2384fcc1012fbde416150f0a3bb9b65c028fdac16d
CANARY-WRONG-EXPECTATION / repetición 1 (canario, fallo esperado): esperado Failed, observado Perfect, veredicto FAIL, causa oracle-mismatch; repro: scripts/autoqa.sh mvp --scenario CANARY-WRONG-EXPECTATION --repetition 1
No verifica: audibilidad, háptica percibida, lectura visual, latencia toque→imagen, sensación ni diversión; tampoco digitalizador, UIKit o IL2CPP.
```

## Huellas de evidencia

Manifiesto local **`Logs/autoqa/evidence-manifest-20261109.json`**, 115 archivos, SHA-256 **90261dfa0644e3e3367a70137ccf574b52cd749fde6a7213cd67b94e4f548352**. Contiene fuentes primarias de POC/TDD, corrida final, regresión, controles forzados, restauraciones y check-release. Los 19 hashes de game-metrics también figuran en summary.json y en el ejemplo anterior. No se versionan logs/builds/cachés ni identificadores personales.

| Principal | SHA-256 |
| --- | --- |
| `Logs/autoqa/20261109T120909Z-491e10d-19260/summary.json` | `e654079eecd75a43f31daf399d1f65a4fe0012489e880b30fbaba13a2e949720` |
| `Logs/autoqa/20261109T120909Z-491e10d-19260/attempts.jsonl` | `233c738bfee4bfd361d8d8239cba190fca0048052730600765609ed8f937e2a3` |
| `Logs/autoqa/20261109T120909Z-491e10d-19260/report.md` | `fdc79df942193fc1720536f80f4b0662982b9d97d026efa024a3886910c5a35a` |
| `Logs/autoqa/20261109T120909Z-491e10d-19260/tests.xml` | `3190bf82e78ba7d8a5114b75565bab6c93491bae0427097d1dc28db36d685d32` |
| `Logs/autoqa/20261109T120909Z-491e10d-19260/presets-integrity.json` | `477b390213545fbb1a315176a84afafe3c422cb33a5b322dd703793b8b479c99` |
| `Logs/autoqa/regression-20261109/editmode-results.xml` | `d8e98e64e2d01f60e16a593670b7e336c901971b396f75843d51c20eab117235` |
| `Logs/autoqa/regression-20261109/playmode-results.xml` | `8aee8e0f00ba60aa2628c926cc4906f42cfe0effd6abbf94456e0aa25cecf222` |
| `Logs/autoqa/evidence-manifest-20261109.json` | `90261dfa0644e3e3367a70137ccf574b52cd749fde6a7213cd67b94e4f548352` |

## Desviaciones, límites y entrega

Incremento a 18 intentos y offsets ±10 son decisiones expresas del PO. La orquestación Editor es la desviación autorizada frente al bucle de producción. Se documentan el RED inválido de ruta y la preparación local BUNDLE_ID; ninguno se cuenta como resultado del juego o check aprobado. No hay parada activa nueva; AUTOQA-MVP-01/02 quedan resueltos, con evidencia histórica intacta.

El gate PASS corresponde a este arnés en Editor/Mono. Audio audible, vibración percibida, visual, latencia toque→imagen, sensación del golpe y diversión siguen siendo del PO en iPhone; tampoco se ejercitan digitalizador, UIKit o IL2CPP durante las recepciones. Las solicitudes de audio sí se observan. S3-validation, PR #32, PR #37, S4, First Rally y First Match no se tocaron.

**PR #36 abierta/lista, sin fusionar.** Revisión técnica independiente posterior por Claude (Opus 5.5 High), coordinada por el PO. No se enviaron mensajes a Claude por herramientas. Fusión y aceptación requieren la decisión posterior del PO; detenerse al entregar.
