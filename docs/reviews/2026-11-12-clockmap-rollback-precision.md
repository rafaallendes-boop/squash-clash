# Precisión de ClockMap y rollback — 12 de noviembre de 2026

## Estado vigente: validación integrada completa, sin fusionar

La decisión del PO «Sí, ajeno a #48» permitió continuar. Tras integrar main por merge: **690/690 EditMode, 404/404 PlayMode (una corrida), AutoQA PASS/0/nivel 1 (85/85) y check-release dev/prod OK, gitDirty=false** sobre `c9e0f3b`. Detalle y evidencia al final; los registros de parada siguientes son históricos. La fusión requiere aprobación explícita del PO.

## Estado histórico inicial: detenido por fallo distinto en PlayMode

La corrección focal está GREEN, pero la entrega **no tiene validación completa ni AutoQA PASS**. La segunda PlayMode completa falló en ReticleVisibilityTests; se cumple la orden «Ante otro defecto o sorpresa: registrar y detenerse». No se ejecutan la tercera PlayMode, AutoQA ni check-release, no se corrige la otra prueba y no se fusiona. PR preparada para que Claude revise tanto el cambio acotado como esta parada, coordinado por el PO.

## Alcance y decisión

Encargo del PO del 12 de noviembre: «dejame los encargos a codex para ir avanzando». Base exacta `main a844c76cfadb1e1c5321ff4d185d9b57f77fb79b`; rama/worktree propios `fix/clockmap-rollback-precision`. Entrega para revisión de Claude coordinada por el PO, sin fusionar. Los hechos del informe First Rally §2 se recibieron en el encargo; no se leyó ni modificó esa rama, la visual ni #32.

**Es un defecto de producto en la normalización de ClockMap**, reproducido sin depender del tiempo de arranque de Unity. SimClock recibe un delta pequeño y avanza correctamente; ClockMap resta tiempos absolutos `double`, convierte ese delta a pasos y normaliza solo según la magnitud de la posición. El redondeo de los operandos absolutos queda fuera de esa cota. `Floor` convierte una diferencia microscópica en un paso entero de menos. Fijar solo la base del test ocultaría el defecto.

La ruta existe en producción: `BeginShotLaunch` usa `Time.realtimeSinceStartupAsDouble`; `ObserveClock` extrapola y registra el presente de un vuelo detenido; `ApplyShotCore` usa ese presente para reconstruir y restaurar la fracción del mismo SimClock. No se atribuye una medición de iPhone: el defecto es alcanzable con estos datos en el código compartido, también en un Player; la frecuencia con frames reales no se ha medido.

## Causa y fuentes oficiales

[Microsoft: System.Double, representación y precisión](https://learn.microsoft.com/en-us/dotnet/api/system.double?view=net-10.0#remarks) describe el formato binario de 64 bits compatible con IEEE 754 y el redondeo de operaciones. [Unity 6000.3: Time.realtimeSinceStartupAsDouble](https://docs.unity.com/en-us/engine/6000.3/script-reference/unityengine/time/realtimesincestartupasdouble) documenta el reloj real desde el inicio y su tipo double. Double ofrece más precisión que float; no hace exacta la aritmética decimal.

Derivación para este caso: en la binada `[64,128)` la separación entre doubles es `2^-46 s = 1,4210854715202004e-14 s`. Con `StepHz=120`, `Tempo=0,65` y `delta=3/120/0,65`, la operación `(base+delta-base)*Tempo*StepHz` produce:

| Base real (s) | Avance aritmético en pasos antes de normalizar |
| --- | --- |
| 10 | 2,9999999999999893 |
| 70 | 2,9999999999995737 |
| 100 | 2,9999999999995737 |
| 200 | 3,0000000000006821 |

En el test Core se parte del paso 200. A 70/100, el error de unos `4,26e-13` pasos supera la antigua cota `4 × 2^-52 × 203 ≈ 1,80e-13`; devuelve 202 en lugar de 203. A 200 el paso ya era correcto, pero quedaba una fracción residual `6,82e-13`. Es el mismo mecanismo en el otro sentido; el control nuevo exige además fracción cero. La banda y el signo dependen de las binadas y del redondeo de los operandos, no de un temporizador que falle al minuto. `SimClock.Alpha` tiene tipo float, pero los casos enteros entregan cero antes de ClockMap; no es la causa de este fallo.

## RED → GREEN y cambio mínimo

Commit RED `30a99aa`: solo pruebas, contra ClockMap original. Commit de producto `dd46548`: solo `Core/Shot/ClockMap.cs`.

- Core: bases 10/70/100/200, extrapolación e interpolación, comparación con SimClock. Controles adicionales conservan fracciones distinguibles a ambos lados del paso (offset de `1e-8` pasos), sin redondearlas al entero.
- PlayMode: se conserva el test original con su aserción de seis pasos y se registra su instante de entrada. Cuatro casos adicionales fijan la base antes de avanzar, reponiendo por reflexión los dos timestamps, el ClockMap y la historia inicial del host deshabilitado. No hay reloj de producción nuevo, espera real ni cambio de contrato. Verifican seis pasos, estado al presente, fracción cero y continuidad en el frame siguiente.
- RED inicial Core: 1/4 Pass, 3 Fail por aserción. RED ampliado de ClockMap: 22/28 Pass, 6 Fail por aserción; en 70/100 se pierde un paso en ambas rutas y en 200 queda fracción residual. Los controles fraccionales y los casos anteriores pasan.
- RED PlayMode focal: 2/5 Pass, 3 Fail por aserción. Bases 70 y 100: **esperado 6, fue 5**, con Unity recién en 1,380/1,383 s; 200 falla solo la nueva aserción de fracción cero. Base 10 y test original pasan. Sin errores de compilación ni dependencia del uptime real.
- GREEN focal: **28/28 ClockMap y 28/28 ShotRollbackTests**, salida 0, cero omitidos.

La normalización conserva el factor existente de cuatro epsilons (`epsilon=2^-52`, no `Double.Epsilon`). Su escala pasa de `max(1,abs(position))` a `max(1,abs(position),timeScale)`. Para extrapolación/coalescencia, `timeScale=max(t,origen) × rate`. En interpolación se usa la magnitud del extremo temporal y la pendiente local `abs(deltaPosition/deltaReal)`, incluyendo la división por la diferencia de timestamps. Así se contempla el error de representación de los operandos absolutos y su propagación a pasos. Los endpoints observados y las pausas no añaden una escala temporal. La corrección actúa simétricamente por arriba y por abajo del entero; fuera de esa envolvente conserva la fracción.

No cambia SimClock, el paso, ventanas, compensación, contrato, umbrales de gameplay, física ni defaults. Esta es una cota de error de representación derivada de la operación, no una tolerancia de juego nueva. No se intenta garantizar precisión para uptimes arbitrariamente enormes ni reemplazar el reloj existente.

## Validación final

Regresiones sobre `dd46548` (el informe en preparación era el único archivo adicional no versionado; mismo producto/tests). Unity **6000.3.27f1 en el Mac del PO**. Procesos secuenciales: antes de cada corrida completa se comprobó que no hubiera otro proceso Unity; guardas y UTC en `run.json`. XML frescos, cero omitidos y sin errores CS en todas las ejecuciones. No se repitió una corrida fallida para convertirla en Pass.

| Ejecución | Inicio → fin UTC, 12 nov | Resultado | Salida |
| --- | --- | --- | --- |
| `scripts/test.sh editmode` | 20:35:18 → 20:35:25 | **574/574 Pass** | 0 |
| `scripts/test.sh playmode`, 1 | 20:35:32 → 20:36:48 | **376/376 Pass** | 0 |
| `scripts/test.sh playmode`, 2 | 20:36:58 → 20:38:18 | **375 Pass / 1 Fail**, total 376 | 1 |
| PlayMode 3 | No ejecutada por parada | Pendiente | — |
| `scripts/autoqa.sh mvp` | No ejecutado por parada | **Pendiente; PASS/0/nivel 1 no demostrado** | — |
| `scripts/check-release.sh` | No ejecutado por parada | Pendiente, requerido por cambio de producto | — |

Instante de entrada del test original `ApplyShot_AfterFlightStopped_ResimulatesToRealPresentViaClockMap`, registrado en XML mediante TestContext (UTC / hora Chile UTC−3 / `Time.realtimeSinceStartupAsDouble`):

| PlayMode | Entrada UTC | Entrada America/Santiago | Reloj Unity (s) | Test original |
| --- | --- | --- | --- | --- |
| 1 | 20:36:33.6972660Z | 17:36:33.6972660 | 61,88758541926758 | Pass |
| 2 | 20:38:03.0700060Z | 17:38:03.0700060 | **65,744063588952372** | Pass |
| 3 | No ejecutada | — | — | Pendiente |

El test original pasó en la segunda corrida **dentro de la banda 64–128 s**. Los cuatro casos de rollback con epoch fijo también pasaron en ambas suites completas. Eso no convierte el total 375/376 en una regresión aprobada.

### Parada CRP-01 — ReticleVisibilityTests

En la segunda corrida, el **2026-11-12T20:37:37Z (17:37:37 Chile)**, falla por aserción `ReticleVisibilityTests.DevOpen_ProtocolActionsRemainUsable_AndCloseDevBeforeArming`, `ReticleVisibilityTests.cs:209`:

```text
Expected: less than or equal to 674.663208f
But was: 788.198853f
```

El output conserva `FIXTURE 1206x2622 safe=(x:0.00, y:102.00, width:1206.00, height:2334.00) root=(x:0.00, y:0.00, width:389.10, height:845.95) camera=(3.20, 8.00, 14.75) fov=90`. El archivo de esa prueba y sus vistas están intactos frente a la base. **La causa y la relación causal con este cambio no se han diagnosticado.** Es un fallo distinto observado en Mac, no una falla Linux a la que aplicar la excepción histórica de AGENTS. Se registra sin cambiar assertions, fixtures, UI ni criterios y sin reintentar. Al finalizar la suite ya en curso se detiene el coordinador, antes de iniciar la tercera corrida.

La continuación requiere decisión del PO sobre esta parada. Quedan pendientes la tercera PlayMode, AutoQA obligatorio PASS/0/nivel 1 y check-release dev/prod; no se declara la entrega apta para fusionar.

### Integridad y evidencia local

GraphicsSettings y QualitySettings cambiaron transitoriamente en ambas PlayMode completas. Se preservó cada diff en evidencia y se restauraron al terminar cada corrida; **todo ProjectSettings está byte a byte igual a la base**. Sin cambios persistentes de defines ni de herramientas. `scripts/device.env` se copió sin cambios desde main y sigue idéntico/ignorado. No se accedió al iPhone ni a sus presets; sin builds, exports, Xcode, instalación ni mediciones físicas.

`DefaultPreset.json` conserva exactamente sus bytes de `a844c76`. La suite EditMode conserva las assertions de defaults `c0d8ec7050e0`, P0 proyectado `6ecfdfa78cbb` y SimulationKey `166a96e319c7`; no se recalculan ni sustituyen hashes esperados. Sin delta en spec, planes, escenarios, criterios, umbrales, Packages, scripts, física ni vistas. El único producto modificado es ClockMap; las otras dos fuentes cambiadas son pruebas y el resto es documentación de esta entrega.

Evidencia cruda ignorada en el worktree: `Logs/evidence/clockmap-precision/` (red, red-expanded, green-focused, editmode-full, playmode-full-1, playmode-full-2, run.json, diffs de ProjectSettings y auditoría). No se versionan logs, XML, caché ni device.env. Los dos commits de código/pruebas se conservan para reproducir el RED y revisar el GREEN, sin modificaciones en las áreas excluidas.

SHA-256 de los XML preservados:

| Evidencia relativa a `Logs/evidence/clockmap-precision/` | SHA-256 |
| --- | --- |
| `red/editmode-results.xml` | `1ddc8bb644a94196af6dec4124d279c7dd8c601905f62f6d5d84a8bb7161dac0` |
| `red-expanded/editmode-results.xml` | `b5f617b3f8307e471407293c8207f05ff8f3f698ad59f88676213ca2f1b04246` |
| `red/playmode-results.xml` | `4320b0f99815c75c25c64bad3c421605625c1a9c52f162a83ef9107b9e8362de` |
| `green-focused/editmode-results.xml` | `5e81cb20fa0a8b4139c4b7acfacb5da330a62909fc082bed3c20230b23c45f34` |
| `green-focused/playmode-results.xml` | `b15b9ea4b4e1dc8d0f75da5a83c25e97a97f9f8429db7f9b4a8bb13c4590fcf9` |
| `editmode-full/editmode-results.xml` | `adb0417d578ab9c71f8f4aba9dd4028da8b02975b312d33c0f40f7b50f18dff5` |
| `playmode-full-1/playmode-results.xml` | `dec5b3f90d6c4970e68bad6f4d4b652437a0bfd5bbf60f08690a1ef8599ef8c8` |
| `playmode-full-2/playmode-results.xml` | `fdcaf57e5e9f031d3edab604a603fc9158772ed2df0697037f3624dac5d7b9f0` |


## PR #48 — decisión del PO sobre CRP-01 y revisión independiente (12 de noviembre de 2026)

**Decisión literal del PO**, 12 de noviembre de 2026, **21:52 America/Santiago**: **«Sí, ajeno a #48»**. CRP-01 se trata como intermitente ajeno al cambio; **su causa no está demostrada y no se corrige en esta PR**. Fuente: encargo del PO en este chat.

Claude revisó independientemente el head **`3615b8588a732eab174cd79cb89bb9bb9a03c464`**: **APROBADO CON OBSERVACIONES, sin bloqueantes**. Sobre un merge local con main `9af2b0c`, según la revisión comunicada por el PO:

- RED **22/28** con el ClockMap original.
- EditMode **690/690**.
- PlayMode **404/404 en tres corridas seguidas**, con `ReticleVisibilityTests` en Pass las tres veces.
- AutoQA **PASS / salida 0 / nivel 1**, **85/85**.
- check-release **dev/prod OK**.

Estos resultados pertenecen a la revisión de Claude sobre su integración local; no se presentan como nuevas corridas de Codex ni sustituyen los fallos históricos registrados.


## Continuación autorizada de #48 e integración de main

Este registro sigue a la decisión literal del PO y al resumen de revisión guardados primero en **`7821a1b`**, antes de integrar o ejecutar verificaciones nuevas. `main` remoto se comprobó en **`9af2b0c8bd92fd5d27f9b2486ea7b626da903874`**; **#49 seguía abierta, sin fusionar**, por lo que no se incorpora esa PR ni se resuelve un conflicto inexistente. Integración mediante **merge `b736832169c57d68eca5b7cc6e900b9e907ccc03`**, sin rebase; conserva el historial de #48 y añade main como segundo padre.

**Observaciones de Claude, sin cambios de código ni pruebas:**

- En la revisión independiente, el test original de rollback empezó a los **60 s**, fuera del tramo **64–128 s**. Ese tramo está cubierto por los casos con base fija; no se presenta esa corrida del test original como cobertura temporal dentro del tramo. No se sustituyen los instantes históricos de las corridas anteriores de Codex.
- Las pruebas usan **reflexión sobre setters privados del host** para preparar la base fija. Se conserva como observación de mantenimiento; no se cambian tests, host ni encapsulación en esta continuación.

Validación autorizada: sobre la integración limpia, **un solo Unity a la vez**, EditMode completo y **una sola** PlayMode completa con XML fresco, AutoQA mvp PASS/salida 0/nivel 1 y check-release dev/prod con `gitDirty=false`; restaurar los ProjectSettings transitorios. **Si vuelve a fallar ReticleVisibilityTests, registrar hora/salida y detenerse sin reintentar para obtener Pass**. Sin más cambios de producto (ClockMap conserva el fix revisado), tests, spec, planes, criterios, umbrales ni defaults. Sin iPhone, build nativa ni instalación; los exports Unity de check-release son solo la comprobación expresamente solicitada. PR lista para el PO, **sin fusionar: se requiere su aprobación explícita para la fusión**. Resultados nuevos aún pendientes al guardar esta predeclaración.


## Cierre de la validación integrada — 12 de noviembre de 2026, Chile

**Validación completa; PR #48 lista para el PO y sin fusionar.** Fuente de producto/tests verificada: **`c9e0f3bbd188736e0f79b739294bcbdc4e433ef3`**, árbol limpio antes y después de cada comando. La continuación conserva ClockMap y los dos archivos de tests exactamente como en el head revisado `3615b85`; solo añade el merge de main y documentación. No se reejecutó RED: el 22/28 de la revisión sigue atribuido a Claude.

Ejecutado en el Mac del PO, Unity **6000.3.27f1**. Antes de cada comando se comprobó que no hubiera otro proceso Unity. Corridas secuenciales; **una sola PlayMode completa**, sin reintentos. Horas **America/Santiago del 12 de noviembre** (corresponden al 13 de noviembre en UTC):

| Comando | Inicio–fin Chile | Resultado | Salida |
|---|---|---|---|
| `scripts/test.sh editmode` | 21:55:44–21:56:30 | **690/690**, cero skipped/CS, XML fresco | 0 |
| `scripts/test.sh playmode` | 21:56:35–21:58:07 | **404/404**, cero skipped/CS, XML fresco; ReticleVisibilityTests **9/9 Pass** | 0 |
| `scripts/autoqa.sh mvp` | 21:58:11–21:58:26 | **85/85; PASS / 0 / nivel 1**, dirty=false | 0 |
| `scripts/check-release.sh` | 21:58:31–21:59:37 | **dev/prod OK**, gitHash=c9e0f3b y gitDirty=false en ambos | 0 |

El test original de rollback pasó; entrada real **62,479577046151931 s**, **2026-11-13T00:57:44.0023590Z** (12 nov **21:57:44.0023590 Chile**). Está fuera del tramo 64–128 s; no se presenta como cobertura del tramo. Los cuatro casos con bases fijas **10/70/100/200 s** pasan. Las pruebas y su preparación por reflexión permanecen intactas. **CRP-01 no reapareció**; se conserva el fallo histórico y la decisión del PO de tratarlo como intermitente ajeno a #48, con causa no demostrada. No se atribuye una causa nueva ni una corrección del retículo.

AutoQA: corrida **`20261113T005811Z-c9e0f3b-26557`**, **18/18 PASS**, canario FAIL esperado, integridad reportada hashesVerified/presetsUnchanged/guardPassed/evidenceSha256Ok=true, salida 0/nivel 1 y `dirty=false`. Restauración automática sin errores; no se presenta como una revisión independiente adicional del verificador.

Check-release produjo exports Unity/IL2CPP frescos para comprobar los módulos: dev **Core/Runtime/DevPanel**, prod **Core/Runtime**, sin framework de pruebas ni ensamblado de captura en Players. Metadata de ambos **gitHash=c9e0f3b**, **gitDirty=false**. Sin compilación nativa Xcode, build firmada, acceso al iPhone ni instalación.

**Integridad:** snapshots de todos los archivos versionados antes de cada comando, diffs transitorios preservados y ProjectSettings restaurados después. Cero diferencias versionadas al finalizar cada etapa, `device.env` sin cambios e ignorado. Todo ProjectSettings/scripts/Packages coincide con main integrado. El delta propio frente a main sigue limitado a ClockMap, los dos tests ya revisados y los dos Markdown; fuera de ClockMap no hay producto distinto de main. Defaults, spec, planes, escenarios, criterios, umbrales, física y vistas sin cambio propio; hashes esperados `c0d8ec7050e0`, P0 `6ecfdfa78cbb`, SimulationKey `166a96e319c7` intactos.

Evidencia nueva separada y **sin versionar** en `Logs/evidence/clockmap-precision/integrated-c9e0f3b/`: por comando `run.json`, salida, XML/logs, manifiesto SHA-256 previo, diffs/restauración de ProjectSettings y `sha256.json`; `audit.json` recoge integración, delta y fuentes revisadas intactas. AutoQA conserva su carpeta propia en `Logs/autoqa/20261113T005811Z-c9e0f3b-26557/`. La evidencia anterior a CRP-01 se conserva.

| XML nuevo | SHA-256 |
|---|---|
| `editmode/editmode-results.xml` | `396826b5747f8f98ff378436e181d022236fa394a8cda659ba9aa3e5d6c02387` |
| `playmode/playmode-results.xml` | `65a24054b50a97167b81c035c6b38d5e8a2a99d6b877d6bdf171c8d2a76e4e08` |
| AutoQA `tests.xml` | `81222f2ed06e4ab1ea2986c46ebc25db46f57edb2395127be21d9e816a7457fa` |

Después de esta fuente verificada solo se registran los resultados en AGENTS.md y este informe; no cambian producto/tests ni se amplían las ejecuciones. Los estados iniciales de parada y pendientes quedan como históricos, actualizados por esta entrada. **Sin fusionar: la fusión requiere aprobación explícita del PO.**
