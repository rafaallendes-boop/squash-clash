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
- B4 añade la afirmación de que E1 no es dos paredes al escenario delantero conservado. El caso trasero adicional queda pendiente del ciclo TDD.
- B1/B1b y las guardias planned, huella y hashes no cambian de expectativa. No se modifica ningún test existente en main.
- Anexo A actualizado al texto aprobado: reflexión de LastStretchedFallback, columna fb y points.tsv (una fila por punto, incluidos los no ganados por el rival). Se conserva el contador observador separado de alineación.

No hay todavía RED válido de B3c/B3d, compilación verificada ni GREEN de esta revisión. `git diff --check` pasa; eso no es un gate de Unity.

## Mediciones pendientes, sin sustitución por cifras viejas

F1 usará `BoastExperiment.M3ConfirmRound2` sin editarlo. F2 usará el Anexo A sin cambiar mezcla, semillas ni partidos, sobre el SHA exacto del candidato y su única base incorporada `0a49b6a`. Las cifras anteriores de `230b758` siguen como antecedente; no se declaran medición del candidato ni de esta nueva base. Se deben registrar R8 total y por estilo con denominadores, las dos cuentas E1 total/estilo, nivel/zona/dos paredes y ret_fw. R7 no medido.

Si falla B5, F1 o F2, falta muestra, o alguna cuenta E1 supera el 5 %, se registra y se detiene. La PR solo sale de borrador con todas las condiciones y gates en verde. No se fusiona.

## Bloqueo de entorno antes de abrir Unity

`scripts/device.env` no existe en este worktree. `scripts/unity-env.sh` exige exactamente ese archivo y retorna 1 si falta; no admite una ruta externa. El encargo vigente dice «no toques ni copies scripts/device.env», que prevalece sobre el permiso general de copia de AGENTS §3 y la excepción del encargo anterior. No se crea, copia, enlaza ni modifica el archivo, ni se cambia el resolver para eludir esa instrucción.

Se solicitó aclarar cómo se proporcionará el archivo (SC-PM sin intervención de Codex o un mecanismo externo autorizado de solo lectura). Sin respuesta al registrar este punto. No se ha abierto Unity ni tomado/suelto el candado en esta revisión. EditMode, PlayMode, F1, F2, E1, AutoQA y check-release **no evaluados**. El bloqueo se registra sin reintentar para producir Pass; la PR permanece en borrador.
