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
