# First Rally — informe de la entrega de gameplay (R1–R5)

Rama `firstrally/gameplay`, base `a844c76` (main con la PR #45). Implementó Claude (Fable) con la [autorización y el plan del 11 de noviembre](2026-11-11-first-rally-plan.md). PR en borrador, sin fusionar. Sin instalación ni acceso al iPhone.

**Estado:** R1–R5 implementadas y verificadas. El rally funciona de punta a punta: lanzamiento neutro, intercambio contra la IA «Fondo», cierre del punto, reinicio y HUD mínimo. **No está juzgado por jugabilidad humana**: eso queda para después de calibrar el timing (S4). La validación es headless y por tests.

## 1. Qué quedó

| Etapa | Commit | Qué hace |
|---|---|---|
| R1 | `c2a65c3` | `PointRules`: segundo bote, tin, out, no llega al frontis; cierre único; botes a 0 con cada contacto; turno de golpe. `ReturnLineJudge`: tin, out del frontis, laterales y trasero por huella, con pruebas en el borde. |
| R2 | `723e006` | `PlayerBody` (reacción, aceleración, velocidad máxima, frenado) movido por pasos de simulación. `ReceptionPlanner` elige dónde plantarse y delega K, T\* y P\* en el `ContactPlanner` existente. Recuperación a la T. |
| R3 | `da03bb9` | `FondoAi`: 18 intenciones, utilidad y softmax, defensa obligatoria, ruido de intención y de timing, tres dificultades (Medio por defecto). Pega por el mismo `ContactContract` y `ShotSolver` que el humano. |
| R4 | `df62f4e` | `RallySim` (rally completo headless) y `RallyDirector` (el mismo loop sobre el host real). Simulación de 1000 rallies dentro de la regresión. |
| R5 | `5b28e89`, `859edb6` | `RallyHud`: una línea de estado, botones Rally/Práctica, Pausa/Reanudar, Revancha y Demo. |

Cómo se juega: la app arranca en práctica, como siempre. El botón **Rally** de la cabecera inicia el modo; **Práctica** lo deja. **Demo** hace que un bot juegue por el humano, para ver el intercambio sin depender del timing propio.

Código nuevo en `Assets/SquashClash/Core/Rally/` y `Assets/SquashClash/Runtime/Rally/`. Cambios en archivos existentes: 4 líneas en `AppBootstrap` (crear director y HUD, dos propiedades) y 7 en `PlaceholderPlayers` (`Rival` y `Place`). **Nada más se tocó**: ni contrato, ni reloj, ni `ShotController`, ni anillo, ni solver, ni `DefaultPreset.json`.

## 2. Verificación

Cada commit de etapa se verificó en un worktree limpio aparte (`scripts/test.sh editmode`, `playmode` y `scripts/autoqa.sh mvp`).

| Commit | EditMode | PlayMode | AutoQA |
|---|---|---|---|
| Base `a844c76` | 562/562 | 372/372 | PASS / salida 0 / nivel 1 |
| R1 `c2a65c3` | 593/593 | 372/372 (una corrida previa 371/372) | PASS / 0 / 1 |
| R2 `723e006` | 610/610 | 372/372 (dos corridas 371/372) | PASS / 0 / 1 |
| R3 `da03bb9` | 635/635 | 372/372 | PASS / 0 / 1 |
| R4 `df62f4e` | 644/644 | 381/381 | PASS / 0 / 1 |
| R5 `5b28e89` | 655/655 | 387/388 en siete corridas | PASS / 0 / 1 |
| R5 `859edb6` | 655/655 | **388/388** | PASS / 0 / 1 |

`scripts/check-release.sh` sobre `859edb6`: OK, dev y prod. El panel sigue fuera de prod; el HUD del rally está en ambos.

Hashes protegidos, comprobados por los tests existentes y por AutoQA: defaults `c0d8ec7050e0` (107 claves), SimulationKey `166a96e319c7`, proyección `0d39e6016bd8`, P0 `6ecfdfa78cbb`. `DefaultPreset.json` no cambió.

### El único fallo visto: un test intermitente que ya existía

Todas las corridas que no dieron el total fallaron en el mismo test: `ShotRollbackTests.ApplyShot_AfterFlightStopped_ResimulatesToRealPresentViaClockMap` («esperado 6, fue 5»).

- **Causa, medida:** el test avanza el host en múltiplos exactos de un paso y espera un paso exacto. Con el reloj real de Unity entre unos 64 y 128 s desde el arranque, el redondeo deja la posición un pelo por debajo y cae un paso antes. Una sonda aritmética con `SimClock` y `ClockMap` dio 0/200 fallos con el lanzamiento a 2–45 s, 40/200 a 60 s, 200/200 entre 64,5 y 120 s, y 0/200 a 130–300 s.
- **Por qué apareció ahora:** depende de cuántos segundos lleva Unity abierto cuando le toca a ese test. Una recompilación o más tests antes lo empujan a esa franja. En R1 y R2, que solo agregan código de Core, falló justo en las corridas con recompilación. En `main` sin cambios y aislado pasó 6 de 6.
- **Qué hice:** no toqué ese test ni el reloj, que son de la zona de timing. Renombré mis dos fixtures de PlayMode (`ZRallyDirectorTests`, `ZRallyHudTests`) para que corran al final y no lo desplacen. Con eso, 388/388.
- **Queda latente:** cualquier test nuevo que corra antes, o un Mac más lento, lo vuelve a meter en la franja. La corrección de fondo es de Codex (ver §6).

## 3. Simulación headless (R4)

Fondo en dificultad Medio contra un bot que hace de humano: siempre paralelo, altura media, potencia media, con error de timing gaussiano de 30 ms. 1000 rallies, receptor inicial alternado, 15,9 s de cómputo. [Informe](../validation/evidence/2026-11-11-first-rally-sim/report.md) y [datos por rally](../validation/evidence/2026-11-11-first-rally-sim/rallies.tsv). Parámetros del rally: hash `23ddf7292b9e`.

| Medida | Resultado |
|---|---|
| Terminan en un evento terminal | **1000 de 1000** |
| Bloqueos (vuelo sin contacto ni cierre durante 20 s de simulación) | **0** |
| Cortados por tope de golpes o de tiempo | 0 |
| Fallas del sistema, contactos fuera de turno | 0 y 0 |
| Golpes por rally | media 19,3 · mediana 13 · p90 44 · máximo 137 |
| Duración real | mediana 28 s · p90 90 s |
| Ganador | bot «humano» 31 % · rival 69 % |

Causas del fin del punto:

| Causa | % |
|---|---:|
| Doble bote por golpe al aire | 61,5 |
| Doble bote por golpe tardío | 31,6 |
| Fuera (siempre del rival, en la pared trasera) | 3,6 |
| Doble bote por pelota inalcanzable | 3,3 |

Lecturas, sin convertirlas en criterio:

- **Casi todos los puntos terminan por timing, no por colocación.** Con 30 ms de error el bot casi nunca sale de la ventana de ±100 ms. Los fallos vienen de soltar cuando la pelota todavía no entró a K o ya salió: T\* queda a veces cerca de un borde de K.
- **Inalcanzable es raro** (0,6 % de las recepciones del bot, 0,06 % de las del rival). Con la cinemática de partida el sistema devuelve casi todo lo que no falla por timing.
- **Ni tin ni «no llega» aparecen.** El solver compartido no acepta apuntar al tin ni sobre la línea de out, así que nadie puede pegar ahí. Las reglas los cubren y están probados, pero el juego hoy no los produce.

## 4. Decisiones tomadas

| Decisión | Motivo |
|---|---|
| Base `a844c76` en vez de `18b796a` | Era el `main` vigente al empezar e incluye `18b796a`. Evita conflictos con S4 ya fusionado. |
| `DefaultPreset.json` intacto; parámetros del rally en `RallyParams` (código, hash propio) | AutoQA ancla `c0d8ec7050e0` en archivos protegidos. Agregar claves lo habría dejado en nivel 3. Costo: los parámetros del rally no están en el panel DEV. |
| Cada golpe del rival es un `Launch` del host | Usa el golpe del humano tal como está. El host sigue siendo la única autoridad de la pelota. |
| La posición del humano entra por `human.x`/`human.z` del lanzamiento | Es el mismo mecanismo de la práctica. El hash de configuración cambia en cada recepción del rally, como ya pasa entre SL1 y SL5. |
| K se calcula con el cuerpo en el punto de llegada | El planificador de contacto existente trabaja con un cuerpo fijo y no se toca. |
| El planificador prefiere plantarse donde T\* queda bien adentro de K | Sin eso, T\* caía en el borde de entrada y cualquier suelta apenas adelantada era «al aire»: 5 % de las recepciones del bot. Con el cambio bajó. No modifica el contrato, solo dónde se para el cuerpo. |
| IA y bot descartan la intención que su propia predicción da por fuera | Desde cerca del frontis, una altura fija sale muy empinada y la pelota se va por arriba o por la pared trasera: 54 % de los puntos terminaban así. Usan el predictor compartido sobre su propio golpe, que §11.2 permite. |
| Tope de elevación del apunte (22°) en las intenciones | Mismo problema: da opciones largas legales también desde adelante. |
| Huella de línea de 20 mm, no ~10 mm | El planificador existente ya juzga con el radio de la pelota. Con 10 mm habría pelotas buenas para la regla e ilegales para el planificador. |
| Pelota rodando tras un bote = segundo bote | P2 deja de emitir botes cuando la pelota ya no rebota; el punto no cerraría. |
| Cinemática de partida: reacción 0,18 s, 4,8 m/s, 12 m/s² | Con valores más atléticos (5,5 m/s, 14 m/s²) la mediana subía a 26 golpes y aparecían rallies de 200. |
| Sin estocada, sin W elegida por el jugador, sin volea, sin replanificación | Fuera del alcance mínimo de R1–R5. Con P2 la predicción es exacta hasta el siguiente contacto, así que replanificar no cambia nada. |
| `d_perc` como retraso extra antes de moverse; `σ_pred` como ruido en dónde se planta | La IA nunca predice mejor que el planificador del humano: solo puede salir peor. |
| Un doble bote contra el humano espera la ventana de rollback (250 ms) | Una suelta anterior al bote puede llegar un frame tarde. |
| Tope de 200 golpes por rally, sin ganador | Con timing perfecto de ambos lados el rally no termina solo. |
| Botón Demo y línea `rally-point` en el log de métricas | El PO puede ver el intercambio antes de calibrar su timing, y la sesión en el iPhone deja registro por punto. |
| Fixtures de PlayMode con prefijo `Z` | Ver §2. |

## 5. Para la sesión visual

El estado de las entidades está expuesto para leer, sin lógica que tocar:

- `AppBootstrap.Rally` → `RallyDirector`: `Active`, `Phase` (Off, HumanReception, RivalReception, PointOver), `Rules` (golpes, turno, botes), `HumanPlan` y `RivalPlan` (dónde se planta, lado del golpe, holgura), `LastPoint`, marcador.
- `RallyDirector.Human` y `.Rival` → `PlayerBody`: `Position` y `Velocity` en el suelo (x, z), `Speed`, `Phase` (Waiting, Approaching, Set, Recovering), `Target`, `Reacting`.
- Eventos: `RallyStarted`, `PointEnded`, `RivalSwing(lado, contacto)`, `ModeChanged`. El swing del humano ya tiene `ShotController.SwingAccepted`.
- Pelota: `BallSimulationHost`, igual que antes.
- Los GameObject `Human` y `Rival` de `PlaceholderPlayers` conservan nombre. En rally el director los coloca con `PlaceholderPlayers.Place`. Fuera del rally nada cambió.
- HUD: elementos `rally-hud`, `rally-status`, `rally-toggle`, `rally-pause`, `rally-rematch`, `rally-demo` dentro de `boot-header`.

## 6. Qué falló o quedó flojo

1. **Test intermitente de rollback** (§2). Es de la zona de timing. Sugerencia para Codex: que el test fije su base de tiempo, o que la normalización de `ClockMap` tolere el error según la magnitud del tiempo real y no solo de la posición.
2. **El contacto del rival se aplica al final del frame.** La física es exacta (se relanza desde el estado histórico de su paso), pero la pelota puede verse hasta un frame atrás en ese instante. Para hacerlo en el paso exacto habría que tocar el loop del host.
3. **Soltar antes de que el cuerpo llegue** puede dar contacto con el personaje todavía en camino (consecuencia de K con cuerpo fijo).
4. **Pelota fuera después del primer bote**: la regla implementada da el punto a quien pegó. Es un **supuesto**; no verifiqué el reglamento WSF para ese caso.
5. **El hash de configuración en los registros cambia en cada recepción del rally.** Las líneas `rally-point` llevan el hash de `RallyParams` para agrupar.
6. **La IA elige cruzado más que paralelo** (58 % contra 34 %). Es efecto del rasgo «lejos del humano» contra un bot que siempre juega paralelo. Pesos de partida.
7. **R6 (First Match) no está en esta entrega.**

## 7. Qué decide el PO

1. **Instalación de la build dev** para el playtest, antes del 18 de noviembre.
2. **Cinemática de los personajes**: ¿el sistema devuelve demasiado? Hoy lo inalcanzable es menos del 1 %.
3. **Dificultad Medio**: 38 ms de error de timing, 16 cm de error de apunte. ¿Punto de partida razonable?
4. **Pasar los parámetros del rally al panel.** Requiere actualizar los anclajes de hash de AutoQA; es trabajo de Codex.
5. **Supuesto de «fuera tras el bote»** y huella de 20 mm.
6. **El test intermitente**: encargar a Codex la corrección de fondo.
7. **Fusionar o no.** Nada se fusiona sin tu aprobación.

## 8. Checklist de playtest

Para después de calibrar el timing. Antes, usar **Demo** para mirar el loop.

Preparación
- [ ] La app arranca en práctica, con el preset y el hash de siempre.
- [ ] Aparece el botón **Rally** en la cabecera y no tapa el frontis.

Rally
- [ ] **Rally** lanza una pelota al frontis que vuelve hacia ti.
- [ ] Tu personaje corre solo hacia la pelota y se detiene antes de que llegue.
- [ ] El anillo y el gesto funcionan igual que en práctica.
- [ ] Si le pegas, el rival corre, devuelve, y la pelota vuelve a tu lado.
- [ ] Después de pegar, tu personaje vuelve hacia la T.
- [ ] Si no le pegas, el punto termina en el segundo bote y la línea dice «Punto Rival: doble bote (…)».
- [ ] La línea muestra marcador, golpes y turno mientras se juega.
- [ ] El siguiente punto empieza solo tras una pausa corta.

Controles
- [ ] **Pausa** congela pelota y personajes; **Reanudar** sigue donde estaba.
- [ ] **Revancha** pone el marcador en cero y lanza de inmediato.
- [ ] **Demo: sí** muestra un intercambio largo sin tocar la pantalla.
- [ ] **Práctica** vuelve al lanzador de siempre.
- [ ] Los botones del panel DEV (Lanzar, Repetir) sacan del rally sin romper nada.

Para anotar
- [ ] ¿Se entiende quién ganó el punto y por qué, sin el panel?
- [ ] ¿El rival falla de forma creíble o parece una pared?
- [ ] ¿Llegas a pelotas a las que no deberías llegar, o al revés?
- [ ] ¿Se nota un salto de la pelota cuando pega el rival?
- [ ] ¿Los dos personajes se estorban a la vista cerca de la T?
