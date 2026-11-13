# First Shot — observaciones de #53 y H3b completo (13 de noviembre de 2026)

Rama `firstshot/b4-followups-h3b` desde main `78b2b65`. Operador: subagente Sonnet dirigido por Claude; la revisión de Claude no es independiente de la dirección del encargo.

## Registro del encargo

Instrucción literal del PO:

> «Realiza el mergeo de 53. Luego Realiza un build al iphone, para probar como está actualment (no es prueba oficial). Luego lanza un sub agente sonnet para realizar las observaciones y con h3b y luego dime cuales serían los siguientes pasos. El iphone está desbloqueado»

Hechos verificados por Claude: PR #53 fusionada (merge `78b2b656b3d3bffee9f43d3ac35c99c22e055b94`, padres `09764ab1…` y exactamente `7fb7315a8165697dccdb3bbc67effb421e1a677b`, árbol `971c70a4487b5a4b7bb118f62d34fab63d64542a` idéntico al head; no acepta First Shot). Build informal `0.1.0+78b2b65 dev` instalada el 2026-11-13 ~18:38Z, arranque P0-calib-1, hash efectivo `b6914a349068`, cero pendientes, presets idénticos byte a byte al respaldo; no es prueba oficial ni aceptación. Esta entrega no toca el iPhone ni instala nada.

Alcance: Parte A (A1/A2 con TDD, A3 solo registro) y Parte B (H3b completo como experimento, no regresión). Sin cambios en `causal.*`, defaults, presets, solver, contrato, ventanas ni física P2.

## Método predeclarado para H3b (escrito y commiteado antes de ejecutar)

Contexto: el experimento del 2026-11-11 dejó el vector causal y los niveles de ayuda parcial/ninguna como NotEvaluated. Ambos existen ahora en main (capa causal de #53, niveles 0/1/2 de #50). Plan First Shot r2, Tarea 4.3 Step 2: «con T3 puro y con V, bots a −60, −100 y −150 ms frente a uno preciso, para cada nivel de ayuda; se informan por separado vector causal promedio, tasa de al aire/tarde y acierto de zona. La hipótesis se juzga con esos datos; no se exige desigualdad estricta en cada dimensión ni se ajusta la física para que pase.»

**Se conserva del método original** (`docs/reviews/2026-11-11-first-shot-s4-timing.md`): recetas SL1–SL6, semillas 0–7, bots 0/−60/−100/−150 ms respecto del T* del plan visible, T3 puro frente a V=100 ms, mismas semillas/intenciones en todos los modos, zona del arnés H3b (frontis x∈[2,7;3,7], y∈[1,7;2,7] m; es del arnés, no H2 ni criterio), P2 hasta el primer contacto/horizonte de 10 s, 384 intentos (6×8×4×2). Fixture de intención: mira central (3,2;2,2) con variación determinista de hasta 0,2 m y potencia 0,25/0,50/0,75.

**Se añade:**

1. *Vector causal productivo por la ruta compartida.* Por receta y modo se llama una vez a `CausalQuality.Prepare` con el contexto congelado (trayectoria de llegada, plan, ClockMap, TimingSettings del modo, instante presentado, tempo, marco corporal del plan), y cada intento contactado se resuelve con `CausalQuality.Solve` (la misma función que usa `RallyContact.Solve` y por tanto el juego), con el marco corporal congelado en el paso de contacto. Se informan por separado, solo sobre contactos aplicados (un fallo nunca aporta una muestra cero): media de la altura total y de la fracción de velocidad total, media por causa (timing/stretch/jam), distancia a la zona muerta `d` (media, y recuento con d=0), recuento de objetivo acotado por el límite legal del frontis, y SpeedRaised/TargetLowered del solver. Estos números se presentan **separados** de contactos/al aire/tarde y del acierto de zona.
2. *Acierto de zona por dos rutas.* Se conservan las columnas del método original (solver sin capa causal, para comparabilidad histórica) y se añaden las de la ruta productiva con capa causal. Un intento causal no resuelto se registra como no resuelto (no se descarta ni se cuenta como acierto).
3. *Niveles de ayuda 0 (Completa), 1 (Parcial), 2 (Ninguna) como dimensión.* **Limitación declarada:** los bots no perciben la vista; leen el plan visible y sueltan en T*+Δ fijo. El nivel de ayuda solo cambia la vista (`TimingRingView`: parcial oculta desde `now ≥ target−0,15 s`, ninguna oculta siempre) y no entra en el contrato, el solver ni la capa causal. Por construcción los resultados de un bot son idénticos en los tres niveles; **no se inventa un efecto**. La dimensión se informa como (a) tablas por nivel con las mismas cifras, rotuladas como no independientes, y (b) visibilidad del anillo en el instante de soltar: completa siempre visible; parcial visible solo si el instante de soltar es estrictamente anterior a `T*−0,15 s` (con Δ=−150 exacto queda oculto, igual que la regla de la vista); ninguna nunca visible. Esa visibilidad es un dato derivado aritmético, no un efecto sobre el bot. El efecto de la ayuda sobre una persona solo puede evaluarse en el iPhone.

**Qué se reporta:** por modo × desfase (y por nivel de ayuda, con la salvedad anterior): intentos, contactos, al aire, tarde, otros; acierto de zona observado/contactos y sobre intentos para ambas rutas; el bloque causal de arriba; pares T3/V con contacto en ambos y diferencias de geometría (debe ser cero, como antes). Denominador cero se informa como NotEvaluated, no 0 %.

**Qué no se hace:** no se fuerza Pass/Fail de H3b, no se exige desigualdad estricta en ninguna dimensión, no se cambia la zona del arnés ni física, solver, valores causales o ventanas tras ver los datos. El juicio de H3b es del PO.

**Verificación de reproducibilidad:** dos ejecuciones en procesos Unity distintos y secuenciales, con el árbol limpio (`gitDirty=false`) y el mismo head; los archivos de salida deben ser idénticos byte a byte (SHA-256). Salidas crudas en `Logs/experiments/h3b-causal/` (ignorado); solo se versiona un JSON derivado en `docs/validation/evidence/2026-11-13-b4-h3b/` con los SHA-256 de los archivos crudos. Un solo Unity a la vez. Guarda de configuración: defaults `9041c4aafc77`/118 claves, P0 `b6914a349068`, SimulationKey `166a96e319c7`.

## Parte A — observaciones de la revisión de #53

### A1 — `pendingCausal` de RallySim (TDD, corregido)

- Hallazgo: el campo `pendingCausal` (`RallySim.cs`) solo se asignaba cuando había plan de contacto y nunca se reiniciaba; una recepción sin golpe podía dejar viva la referencia congelada de la anterior (RallyDirector, en cambio, limpia `pointCausal` en `BeginPoint`, `RallyDirector.cs:111`). Con el código actual no es alcanzable un uso erróneo: `Strike` solo ocurre con `hasPending`, que exige un contacto resuelto con plan; es una guarda de invariante, no un defecto observable en partidas.
- RED por aserción: `RallySimTests.CausalReference_DoesNotSurviveIntoAReceptionWithoutASwing` (humano Bot σ=0 golpea y el rival nunca golpea; la recepción del rival no hace swing). Primer fallo: «a reception without a swing kept the previous reception's causal reference». Las aserciones previas (primer contacto del humano con vectores propios, última recepción NoSwing/Unreachable) pasan. XML: `Logs/evidence/b4-followups-h3b/red-a1.xml` (local, ignorado). Sin errores CS.
- GREEN: `pendingCausal=null` al inicio de `BeginReception` (`RallySim.cs:144`). Focal 1/1; `RallySimTests` 10/10. Sin cambio de contrato, defaults, solver ni resultados de partidas (RallySimTests regenerados no cambian).
- Nota de método: la prueba lee el campo privado por reflexión (misma advertencia de mantenimiento ya registrada para las pruebas de #48); no existe otro observable porque la referencia obsoleta nunca llega a usarse.

### A2 — marco corporal vivo frente al congelado (equivalencia demostrada, sin cambio de código)

`Strike` resuelve con `context.FrameAt(Body(who).Position)` (`RallySim.cs:182`), mientras que `Prepare` recibió marcos de `positions[j-first]` (`RallySim.cs:162-163`). No hay diferencia posible por construcción:

1. `positions` se toma con `body.PositionsAfter(n)` (`RallySim.cs:162`), que copia el cuerpo con todo su estado (`PlayerBody.cs:36`) y aplica `Step()` una vez por posición (`PlayerBody.cs:38-44`). La copia se hace **después** de `body.MoveTo(plan.Stance,…)` (`RallySim.cs:149`).
2. Entre esa copia y el golpe, el cuerpo `who` solo cambia por `Step()` (`RallySim.cs:131`, una vez por `ball.Step`, `RallySim.cs:128`). Los otros `MoveTo/Teleport` de ese cuerpo ocurren en `Strike` después de leer la posición (tras `RallySim.cs:182`) o en `BeginReception` del oponente, que solo ocurre tras el golpe de `who`. El cuerpo del oponente no es el que golpea.
3. `Strike` se ejecuta cuando `ball.State.Step==pendingStep` (`RallySim.cs:125`), con el cuerpo avanzado exactamente `pendingStep−first` pasos, que es el índice que `Prepare` usa para `BodyAt`. `ball.Reset` en `Strike` conserva `Step` (`RallySim.cs`, `Strike`), sin desplazar esa cuenta.

Por tanto `FrameAt(Body(who).Position)` en el paso de contacto es idéntico a `reference.BodyAt(contactStep)`, que es lo que usan RallyDirector (`RallyDirector.cs:327`) y la ruta humana. No se demostró ni se buscó un caso en que difieran: la equivalencia es determinista. No se cambia código ni se añade prueba nueva. (No se ejecutó un experimento adicional de cotejo numérico; el argumento es estático.)

### A3 — Observaciones registradas (sin código)

- Cerca de la línea de out la altura causal se satura en 4,55 m (límite legal del frontis tras el acotado de #53) y solo queda la pérdida de velocidad: observar en el playtest.
- Simulación de rallies tras la capa causal: Rival Out 36→59 (sobre todo pared trasera) y victorias del humano 311→330. Observar junto con R3 (drop desde atrás).
- La fórmula de E3/E5 (distancia 3D, rampas lineales) es la lectura de Codex, no una decisión del PO.

## Parte B — resultado de H3b

Fuente `7f3176ef5713df956409a1780c72eb5d003ea35f`, árbol limpio, defaults `9041c4aafc77`/118, P0 `b6914a349068`, SimulationKey `166a96e319c7`. Dos procesos Unity secuenciales (`SquashClash.EditMode.Experiments`, filtro por nombre; XML de ejecución en `Logs/evidence/b4-followups-h3b/h3b-run1.xml` y `h3b-run2.xml`, 1/1 solo como ejecución del arnés). **Los tres archivos de salida son idénticos byte a byte** entre ambas corridas (`trials.json` SHA-256 `67f50b9141ba5859de149059d9150b82d9bddbe600e87edb50e2dd6a01aee1b6`, `summary.tsv` `e5f6f9f3…c`, `report.md` `e44b1dc8…a`; completos en el JSON derivado). Resumen derivado y SHA-256: `docs/validation/evidence/2026-11-13-b4-h3b/summary.json`. Crudos locales ignorados: `Logs/experiments/h3b-causal/`. 384 intentos, cero topes P2, cero instancias vivas, cero diferencias de geometría entre V y T3 en los 112 pares con contacto en ambos.

| Modo | Δ ms | Intentos | Contactos | Al aire | Tarde | Zona sin causal | Zona causal | ΔAltura media (m) | ΔVel. media (frac.) | d media (m) | d=0 | Acotado |
|---|---:|---:|---:|---:|---:|---:|---:|---:|---:|---:|---:|---:|
| T3 | 0 | 48 | 48 | 0 | 0 | 48/48 | 43/48 | 0,2368 | −0,1184 | 0 | 48 | 0 |
| T3 | −60 | 48 | 32 | 16 | 0 | 32/32 | 32/32 | 0,1148 | −0,0509 | 0,1299 | 0 | 0 |
| T3 | −100 | 48 | 32 | 16 | 0 | 32/32 | 32/32 | 0,1332 | −0,0555 | 0,2220 | 0 | 0 |
| T3 | −150 | 48 | 32 | 16 | 0 | 32/32 | 30/32 | 0,2385 | −0,1019 | 0,3478 | 0 | 0 |
| V | 0 | 48 | 48 | 0 | 0 | 48/48 | 43/48 | 0,2368 | −0,1184 | 0 | 48 | 0 |
| V | −60 | 48 | 32 | 16 | 0 | 32/32 | 32/32 | 0,1148 | −0,0509 | 0,1299 | 0 | 0 |
| V | −100 | 48 | 32 | 16 | 0 | 32/32 | 32/32 | 0,1332 | −0,0555 | 0,2220 | 0 | 0 |
| V | −150 | 48 | 0 | 48 | 0 | 0/0 | 0/0 | NotEvaluated | NotEvaluated | NotEvaluated | 0 | 0 |

Las medias son sobre contactos resueltos (denominador cero: NotEvaluated, no cero). Con la capa causal resueltos 100 % de los contactos en todas las celdas. El desglose por causa y por receta está en `summary.tsv`/summary.json.

**Niveles de ayuda.** Los bots no perciben la vista y el nivel de ayuda no entra en el contrato, el solver ni la capa causal: las tablas por nivel del informe crudo son los mismos 384 intentos y no son muestras independientes. Lo único que varía es si el anillo seguía visible en el instante de soltar: **completa 384/384, parcial 0/384, ninguna 0/384**. Parcial queda en 0 porque el anillo se oculta desde T*−0,15 s y todos los bots sueltan en o después de ese instante (con Δ=−150 exacto, oculto por la regla `now≥target−0,15` de la vista). Es un dato derivado, no un efecto medido sobre jugadores: el efecto de la ayuda sobre una persona solo puede evaluarse en el iPhone.

### Interpretación limitada (el juicio de H3b es del PO)

- Contactos y al aire/tarde reproducen el experimento del 11 de noviembre exactamente (256 contactos, 128 al aire, cero tarde): la capa causal no cambia quién conecta. V a −150 ms sigue sin contactos por la ventana; V filtra solo ahí.
- El vector causal medio crece con el adelanto de T3 hasta −150 (ΔAltura +0,115/+0,133/+0,239 m; pérdida de velocidad −0,051/−0,056/−0,102) con distancia a la zona muerta 0,13/0,22/0,35 m; con Δ=0, d=0.
- En Δ=0 el vector no es cero (+0,237 m, −0,118): proviene de la componente de estiramiento (E5, posición del cuerpo en SL1–SL4), no del tiempo; SL5/SL6 sí dan 0. Esto es coherente con E3/E5 independientes y significa que la comparación «bot preciso» no es un vector cero.
- El acierto de zona por la ruta productiva baja en 5/48 (Δ=0) y 2/32 (T3 −150) frente a la ruta sin causal: los fallos son impactos de frontis que pasan por encima del borde superior (y>2,7) de la zona del arnés por la subida causal (SL1 y SL4). Con la zona del arnés y sin cambiar nada, no hay un efecto «de desventaja por anticipar» visible en la zona: el acierto por zona en −60/−100 se mantiene 32/32 y el efecto de mayor magnitud en altura aparece en Δ=0 por posición, no por anticipación. Esto no demuestra ni refuta H3b; la zona de 1,0 m de alto del arnés es un fixture, no un criterio.
- Cero objetivos acotados por el límite legal del frontis en esta muestra; la saturación a 4,55 m no se ejercita en estos escenarios.

## Estado de verificación

Compuertas ejecutadas sobre el head `713c44c` (árbol limpio, un solo Unity a la vez, XML frescos preservados en `Logs/evidence/b4-followups-h3b/`, ignorado):

- EditMode completa: **715/715** (714 + la prueba nueva A1), cero fallos/omitidos/errores CS.
- PlayMode completa: **457/457**, cero errores CS. `ProjectSettings/GraphicsSettings.asset` y `QualitySettings.asset` quedaron modificados por la corrida y se restauraron a HEAD.
- AutoQA `scripts/autoqa.sh mvp` (no es obligatorio aquí: no se toca timing, gesto ni contrato): **PASS / salida 0 / nivel 1**, 85/85, RUN_DIR `Logs/autoqa/20261113T184825Z-713c44c-10767`.
- `scripts/check-release.sh`: **OK** para dev y prod, metadata `gitHash=713c44c`, `gitDirty=false` en ambos; dev incluye el módulo DevPanel y prod no.
- Hashes intactos: defaults `9041c4aafc77`/118, P0 `b6914a349068` y SimulationKey `166a96e319c7` verificados por la guarda del experimento; las pruebas que anclan el hash de SL5 (`e2ffa032a588`) pasan. No se tocó `causal.*`, defaults, presets, solver, contrato, ventanas ni P2.
- Los commits posteriores a `713c44c` son solo documentación (este bloque).

No se tocó el iPhone ni se usó devicectl/Duplicación/xcodebuild de dispositivo. Un `UnityAutoQuitter` residual de la primera corrida del experimento apareció en `pgrep`, pero ya había terminado la corrida (era solo el ayudante); no hubo dos editores simultáneos.
