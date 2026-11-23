# RAL-174 — camino B, revisión 2 del encargo

## Fuente y base

Encargo completo leído: `/Users/rafaallendes/sc-specs/impl-101-r2.txt`, seguido de la spec completa y `/Users/rafaallendes/sc-specs/impl-i2i3-b.md`. Sigue vigente la autorización de SC-PM del 22 nov, 19:15 −03 (`auth-green-1122.txt`): los errores propios de compilación, parche o supuestos en tests nuevos se pueden corregir; las expectativas salen de la spec. No se cambian tests existentes, escenarios, umbrales, tolerancias, oráculos, presets ni criterios para pasar.

- Primer `gh pr view 92 --json state,mergeCommit`: OPEN, mergeCommit null (23 nov, 10:33 Chile).
- Consulta a los cinco minutos (10:38): MERGED, `0a49b6a3ff8ae481ad48f13ed8990faae19374e1`.
- Segundo padre del merge: `3a12e3669eed853b87c9b148e9895836e599bd30`, exactamente el head aprobado.
- Merge normal de `origin/main` en `feat/rival-short-i2i3`: `b6b04314a3f0941a37c14f1b73a99c8cf6566734`. No se reescribe historial ni se fuerza push. La base incorporada pasa a ser `0a49b6a3ff8ae481ad48f13ed8990faae19374e1`.

La evidencia anterior permanece en [el registro B](2026-11-23-rival-short-b.md). La precondición B3 del 22 nov falló: sus cuatro dos paredes eran Good y Reachable. No se convierte ese resultado en aprobación del rechazo.

## Preparación de RED, todavía sin ejecutar

- B3c: cuatro casos sintéticos fijados por la spec, con `StretchedFilter.Apply` como esqueleto que devuelve −1 sin modificar pesos.
- B3d: ambos escenarios predeclarados, contacto `(.9,.8,8.6)` con humano en la T o `ClampBody(W,L)`. Test independiente por opción para Good, Reachable, Delay e IsBoast. La evaluación de producción conserva intencionalmente el pronóstico directo anterior hasta registrar el RED.
- B3a conserva la prueba lateral antes de frontis; B2 conserva sus precondiciones y 500 elecciones por estilo indicado.
- B3b sustituye solo el test nuevo de #101, según el cambio explícito de spec: grilla x `{.5,.9,1.5}`, y `{.3,.8}`, z `{7.6,8.6,9.3}`, cuatro posiciones humanas limitadas por ClampBody, tres estilos y 100 decisiones por casilla. No se acredita rechazo sin muestra negativa.
- B4 añade la afirmación de que E1 no es dos paredes al escenario delantero conservado. Se añade una guardia trasera con dos paredes candidatas: primera casilla de la grilla B3b `(.5,.3,7.6)`, humano en `ClampBody(W,L)`. Un cálculo independiente decide si existe opción segura; si no, afirma el menor retraso entre las que no son dos paredes y la marca E1. Se declarará si este caso no ejercita E1; no se supone de antemano que carezca de opciones alcanzables.
- B1/B1b y las guardias planned, huella y hashes no cambian de expectativa. No se modifica ningún test existente en main.
- Anexo A actualizado al texto aprobado: reflexión de LastStretchedFallback, columna fb y points.tsv (una fila por punto, incluidos los no ganados por el rival). Se conserva el contador observador separado de alineación.

No hay todavía RED válido de B3c/B3d, compilación verificada ni GREEN de esta revisión. `git diff --check` pasa; eso no es un gate de Unity.

## Mediciones pendientes, sin sustitución por cifras viejas

F1 usará `BoastExperiment.M3ConfirmRound2` sin editarlo. F2 usará el Anexo A sin cambiar mezcla, semillas ni partidos, sobre el SHA exacto del candidato y su única base incorporada `0a49b6a`. Las cifras anteriores de `230b758` siguen como antecedente; no se declaran medición del candidato ni de esta nueva base. Se deben registrar R8 total y por estilo con denominadores, las dos cuentas E1 total/estilo, nivel/zona/dos paredes y ret_fw. R7 no medido.

Si falla B5, F1 o F2, falta muestra, o alguna cuenta E1 supera el 5 %, se registra y se detiene. La PR solo sale de borrador con todas las condiciones y gates en verde. No se fusiona.

## Bloqueo de entorno antes de abrir Unity

`scripts/device.env` no existe en este worktree. `scripts/unity-env.sh` exige exactamente ese archivo y retorna 1 si falta; no admite una ruta externa. El encargo vigente dice «no toques ni copies scripts/device.env», que prevalece sobre el permiso general de copia de AGENTS §3 y la excepción del encargo anterior. No se crea, copia, enlaza ni modifica el archivo, ni se cambia el resolver para eludir esa instrucción.

Se solicitó aclarar cómo se proporcionará el archivo (SC-PM sin intervención de Codex o un mecanismo externo autorizado de solo lectura). Sin respuesta al registrar este punto. No se ha abierto Unity ni tomado/suelto el candado en esta revisión. EditMode, PlayMode, F1, F2, E1, AutoQA y check-release **no evaluados**. El bloqueo se registra sin reintentar para producir Pass; la PR permanece en borrador.

## Corrección de entorno de SC-PM e intento de continuación

Leído completo `/Users/rafaallendes/sc-specs/impl-101-env.txt`. SC-PM retira la prohibición de copiar y autoriza copiar sin modificar desde `/Users/rafaallendes/orca/squash-clash/scripts/device.env`, sin versionar el archivo y borrándolo al terminar. Se mantienen las condiciones de parada de r2.

El primer intento de copia retorna código **1**, antes de cualquier Unity:

```text
cp: /Users/rafaallendes/orca/squash-clash/scripts/device.env: No such file or directory
```

La fuente indicada no existe; no se creó el destino. No se buscó otra fuente ni se cambió el resolver para eludir la falla. **Parada por entorno**, según el encargo y la autorización de iteración: no es RED por aserción ni GREEN. No hubo apertura de Unity, adquisición/liberación de candado ni nuevos resultados de tests o mediciones. La PR #101 continúa en borrador y no se fusiona. Para continuar falta que se proporcione la fuente autorizada (o se indique su ubicación correcta).

## Entorno autorizado y prioridad de #102

Leído completo `/Users/rafaallendes/sc-specs/device-env-ok.txt`. Autorización literal del PO, 23 nov 13:36, recibida vía SC-Lead: «Sí, autorizo que los trabajadores copien sin cambios scripts/device.env desde sc-install-518f9c3 y lo borren al terminar.» Se copió desde `/Users/rafaallendes/orca/workspaces/squash-clash/sc-install-518f9c3/scripts/device.env`; SHA-256 origen/destino coincidentes. Archivo ignorado por Git, sin editar ni versionar; se retirará al terminar.

El encargo posterior da prioridad de Unity a la PR #102 para la build de hoy: no se adquiere el candado hasta observar `gh pr view 102 --json isDraft` con false. Primera consulta de esta reanudación: true. Durante la espera solo se prepara trabajo que no usa Unity; los esqueletos B3c/B3d permanecen sin GREEN hasta registrar su RED válido.

Resúmenes reproducibles preparados en `docs/validation/2026-11-22-rival-short-ball/`: `summarize.py` genera las tablas descriptivas de §2.2/§2.3; `evaluate.py` calcula F1, F2 y ambas cuentas E1 con comparaciones de fracciones enteras, sin tolerancias nuevas. Verificación descriptiva sobre la evidencia antigua: 42.986 filas, 10.632 contactos estirados y F1 de main `230b758` cumple, como ya estaba registrado. No son resultados del candidato.

## B3c/B3d: RED válido después de la prioridad

Primera observación de #102 con `isDraft: false`: **2026-11-23T16:48:49Z**. Solo después se adquirió el candado para `scripts/test.sh editmode 'StretchedFilterTests;RivalAi_EvaluateStretched_MatchesThePlannerPerOption'`. Corrida de 16:48:49Z a 16:49:01Z, head `1d06fe7`, árbol limpio al inicio.

`b-r2-required-red`: **6 tests, 0 Passed, 6 Failed, 0 Skipped**, XML fresco, sin errores CS. B3c falla sus cuatro casos porque los pesos rechazados quedan intactos (100, 1, 100 y 100, respectivamente), frente a −∞. B3d falla ambos escenarios en la primera dos paredes (Low/Medium): `Good` false frente a true en el cálculo independiente. Son fallos de aserción esperados, no errores de compilación ni de precondición.

Evidencia ignorada: `Logs/evidence/rival-short-min/b-r2-priority/observations.txt` y `b-r2-required-red/` (XML, log, fuente, estado y horarios). El candado propio se liberó al terminar; no se libera el de otro worker.

Después del RED se implementan `StretchedFilter.Apply` y la evaluación por trayectoria correcta. `FilterStretched` compone exactamente ambas piezas; las opciones estiradas se guardan en el mismo orden compacto en Decide y EvaluateStretched. E1 elige el menor retraso no dos paredes con empate por orden; planned conserva su recorrido y sus sorteos. GREEN dirigido en espera del candado normal de Unity, sin declarar resultado todavía.

## GREEN y guardias dirigidas

Tras esperar el candado, `b-r2-required-green`: **6/6 Passed, 0 Failed, 0 Skipped, salida 0**, XML fresco y sin errores CS. Corrida de 2026-11-23T17:06:54Z a 17:07:05Z; árbol de trabajo con el cambio de implementación posterior al RED. No se cambió ninguna aserción de B3c/B3d entre ambas corridas.

`b-r2-directed-green`: **28/28 Passed, 0 Failed, 0 Skipped, salida 0**, XML fresco. Duración de tests 207,1583186 s. B1/B1b/B2/B3a/B3b/B3c/B3d y las guardias B4/B4b pasan.

- B3b: 72 casillas y 7.200 decisiones por estilo, **21.600 decisiones en total**. **20 evaluaciones de dos paredes Good pero inalcanzables por estilo**, cero elecciones inalcanzables cuando hay alternativa segura. Esta vez la grilla sí ejercita el rechazo; no se buscaron posiciones fuera de la grilla predeclarada ni se alteraron parámetros. El resultado anterior de la esquina `(.9,.8,8.6)` sigue siendo el mismo y no se borra.
- Cero casillas traseras sin ninguna opción alcanzable. La casilla B4 añadida `(.5,.3,7.6)` da **6/6 alcanzables**, E1 false; es guardia trasera, **no un caso real de E1 trasero**. E1 con dos paredes de menor retraso queda probado por B3c, caso 3; el escenario delantero conservado prueba E1 real y la marca. No se inventa un escenario trasero inalcanzable.
- B4b conserva el negativo real de frontis `(.60,.05,1.10)`: recta inalcanzable, cruzado alcanzable, tres estilos, ninguna elección inalcanzable.
- Huella de las 1.080 decisiones planned: `53870c765e9e1cc410b87b41d810afc7fbe53ac0ffa42345b9b97192786de4de`, idéntica.
- Hashes comprobados: defaults `207da8f2c636`, RallyParams `1063d0f6cf66`, SimulationKey `166a96e319c7`, sin cambios.

XML, log, fuente y estado en `Logs/evidence/rival-short-min/b-r2-required-green/` y `b-r2-directed-green/`. El candado propio se libera al acabar cada corrida. B5 y las mediciones/gates completos todavía pendientes; estos dirigidos no los sustituyen.


## Candidato exacto medido y B5

Código medido **`ae9829c7289ac8229a996e05a5fd15f5b2ec3530`**, confirmado igual al head remoto de #101 y con árbol limpio antes de cada gate/medición. Base única incorporada para F2: **`0a49b6a3ff8ae481ad48f13ed8990faae19374e1`**. Los commits posteriores solo documentan estos resultados; no se presentan como otra medición.

`b-r2-ae9829c-editmode`: **996/996 Passed, 0 Failed, 0 Skipped, salida 0**, XML fresco, duración 326,6683828 s. El test existente `BoastRivalTests.On_InPlay_TheFondistaBoastsLessFromTheBackQuarterThanTheDefinidor` pasa sin modificarlo. Corrida 2026-11-23T17:19:43Z–17:25:19Z.

## Prioridad de instalación respetada

Al recibir la prioridad de `/Users/rafaallendes/sc-specs/INSTALL-EN-CURSO`, F1 aún esperaba el candado: no había adquirido ni abierto Unity. Se cancela solo ese waiter (salida 143 por cancelación solicitada), sin retirar el candado ajeno. No es una corrida F1 ni un fallo de M3. Registro: `b-r2-ae9829c-f1/deferred.txt`.

El wrapper comprueba el marcador antes de cada intento. Si aparece justo después de adquirir, libera solo su propio candado antes de abrir Unity. Una vez adquirido también comprueba que no haya otro Unity activo. Durante la pausa solo se auditan fuentes y se preparan informes. Marcador observado ausente a las **17:39:31Z**; F1 empieza a las **17:39:32Z**, con head local/remoto `ae9829c` y árbol limpio.

## F1: M3 vuelve

Experimento original `BoastExperiment.M3ConfirmRound2` **sin editar**, 100 partidos por estilo/nivel, `NewcomerBot` σ90, semillas `20270131`. `b-r2-ae9829c-f1-after-install`: **1/1 Passed**, XML fresco, duración 318,4724328 s. Cero SystemFault en nueve celdas.

| Nivel | Fondista | Definidor | Completo |
|---|---|---|---|
| Medio | 178/3819 = 4,6609059963 % | 441/4343 = 10,1542712411 % | 361/4156 = 8,6862367661 % |
| Fácil | 447/3187 = 14,0257295262 % | 554/3023 = 18,3261660602 % | 477/2781 = 17,1521035599 % |
| Difícil | 12/3671 = 0,3268864070 % | 79/5151 = 1,5336827800 % | 48/4208 = 1,1406844106 % |

**F1 cumple:** en Medio, Fondista < Definidor, Fondista ≤5 %, Definidor entre 5 y 15 % y Completo entre ambos. No se aplican las bandas de Medio a Fácil/Difícil. Confirmación en `Logs/experiments/boast/b-r2-ae9829c-f1-after-install/confirm.tsv`; evaluación en la evidencia del mismo label, runner en `Logs/experiments/rival-short-ball/`.

## F2: base única repetida y candidato

Se mide otra vez `0a49b6a`; no se sustituye por las cifras antiguas de `230b758`. Checkout detached, archivos seguidos intactos, añadiendo solo el mismo observador del Anexo A y su .meta, idénticos a los del candidato. Después se retiran solo esos dos archivos propios y se vuelve a `feat/rival-short-i2i3`, limpio en `ae9829c`, confirmado igual al remoto antes de medirlo.

- Base `b-r2-main-0a49b6a-run01`: **1/1 Passed**, XML fresco, 176,5216603 s.
- Base `b-r2-main-0a49b6a-run02`: **1/1 Passed**, XML fresco, 157,9930825 s. **rows.tsv, points.tsv y source.tsv idénticos byte a byte**. **42.986 filas**, fb NA en todas (la base no tiene la propiedad).
- Candidato `b-r2-ae9829c-run01`: **1/1 Passed**, XML fresco, 217,5168565 s. Corrida 17:54:44Z–17:58:36Z. **42.957 filas**, **13.012 puntos**, **9.216 ganados por el rival**.
- Método fijo: 3 estilos ×3 niveles ×150 partidos, semillas `20261122`, mezcla y parámetros sin cambios. Configuración idéntica base/candidato: defaults `207da8f2c636`, RallyParams `1063d0f6cf66`, Unity `6000.3.27f1`.
- Cero extraMissingLogs, roleMismatches y SystemFault en las filas de las tres corridas; sin descartar filas para alinear índices.

### R8: **NO cumple**

Comparación de fracciones enteras, sin tolerancias ni redondeo para el veredicto. Los cuatro grupos tienen denominadores positivos en ambas corridas.

| Grupo | Base 0a49b6a | Candidato ae9829c | F2 |
|---|---|---|---|
| total | 1436/10632 = 13,5063957863 % | 1437/10607 = 13,5476572075 % | **SUBE** |
| Fondista | 607/3921 = 15,4807447080 % | 601/3886 = 15,4657745754 % | No sube |
| Definidor | 414/3527 = 11,7380209810 % | 422/3535 = 11,9377652051 % | **SUBE** |
| Completo | 415/3184 = 13,0339195980 % | 414/3186 = 12,9943502825 % | No sube |

Sube el total **+0,0412614212 pp** y Definidor **+0,1997442241 pp**. La condición requiere no subir en los cuatro grupos; falla en dos.

### R3b: ambas cuentas E1 cumplen ≤5 %

El denominador de E1/puntos incluye **todos** los puntos ganados por el rival en points.tsv, no solo las recepciones estiradas.

| Grupo | E1/contactos estirados | E1 ganadora/puntos del rival |
|---|---|---|
| total | 5/10607 = 0,0471386820 % | 2/9216 = 0,0217013889 % |
| Fondista | 3/3886 = 0,0772002059 % | 1/3081 = 0,0324569945 % |
| Definidor | 1/3535 = 0,0282885431 % | 0/3117 = 0,0000000000 % |
| Completo | 1/3186 = 0,0313873195 % | 1/3018 = 0,0331345262 % |

### Datos descriptivos sin criterio adicional

| R8 por nivel | Base | Candidato |
|---|---|---|
| Fácil | 544/4195 = 12,9678188319 % | 543/4173 = 13,0122214234 % |
| Medio | 541/3988 = 13,5656970913 % | 542/3985 = 13,6010037641 % |
| Difícil | 351/2449 = 14,3323805635 % | 352/2449 = 14,3732135566 % |

| R8 por contacto rival | Base | Candidato |
|---|---|---|
| Delante/media: r_z <7,31 m | 1264/6588 = 19,1863995143 % | 1264/6617 = 19,1023122261 % |
| Atrás: r_z ≥7,31 m | 172/4044 = 4,2532146390 % | 173/3990 = 4,3358395990 % |

Dos paredes entre contactos estirados, según la columna de intención del Anexo A: **728/10632** base, **713/10607** candidato.

Distribución ret_fw, metros, cuantiles lineales; ninguna muestra faltante en estos grupos:

| Fuente/zona | n | mín | p25 | p50 | p75 | p95 | máx |
|---|---|---|---|---|---|---|---|
| Base delante/media | 6588 | 0,504 | 0,768 | 1,0425 | 1,461 | 3,12565 | 4,352 |
| Base atrás | 4044 | 0,602 | 3,05375 | 3,342 | 3,563 | 4,0607 | 4,55 |
| Candidato delante/media | 6617 | 0,504 | 0,768 | 1,045 | 1,465 | 3,1344 | 4,352 |
| Candidato atrás | 3990 | 0,59 | 3,052 | 3,344 | 3,557 | 4,05055 | 4,55 |

Tablas descriptivas completas generadas sin Unity en los directorios de las corridas: cross.tsv y stretched.tsv. JSON con R8/E1/distribuciones en `Logs/evidence/rival-short-min/b-r2-ae9829c-run01/r8-e1-evaluation.json`. No se reclasifica won ni se excluyen errores del bot humano.

## Parada obligatoria y entrega

El evaluador devuelve **F2 false, E1 true** y salida 1; no inicia la siguiente corrida. **Se detiene por F2**, sin modificar código, tests, escenarios, semillas, oráculos, tolerancias, umbrales, pesos ni presets para obtener Pass. El determinismo de la base sí está demostrado. **La segunda corrida del candidato no se inicia por la parada**, por lo que su repetición queda no evaluada. No se reintenta para obtener otro R8.

**PlayMode, AutoQA V/T3/volea y check-release no evaluados en esta tarea por la parada F2.** No se reutilizan gates de otra rama/SHA. R7 no medido. La PR #101 permanece en borrador. No se fusiona, construye nativamente, instala ni accede al iPhone desde este trabajo.

Cada corrida liberó su propio candado; no se retiró ninguno ajeno. La copia local de device.env se cotejó con el SHA-256 guardado y **se borró al terminar**, sin editarla ni versionarla; la fuente queda intacta. Constancia ignorada: `b-r2-install-priority/device-env-copy-proof.json`. Los commits del informe posteriores a `ae9829c` no cambian el código medido ni se presentan como gates de un SHA nuevo.
