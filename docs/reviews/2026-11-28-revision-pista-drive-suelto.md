# Revisión cruzada · pista «tu drive quedó suelto»

28 nov 2026. Encargo `set27-revision-pista-drive-suelto` de SC-PM. Revisión de solo lectura, sin Unity. No se corrieron `scripts/test.sh`, AutoQA ni `check-release`. No se tocaron `sc-unity.lock` ni `scripts/device.env`.

- **Head revisado:** primero `30fd0c4355dc8fae0353e8fafc1b2c5a2827da24` (§1–§8). Después, el delta hasta `37f776792bc0321da4b984884958201ea77c79a7` (§9). Por último, el merge de `main` **`980cf39f0cc9db36228b685bdf01eff92716a22b`** (§10), igual a `origin/sc-pista-drive-suelto` tras el fetch. Base `origin/main` `99b4306` (merge-base). El delta de la spec contra `654d243` es solo la sección «Decisiones del PO».
- **Autor:** Grok, por la excepción de Rafael del 28 nov 11:44. Revisor: Claude (Opus), que no escribió el código.
- **Veredicto actualizado (head `980cf39`): APROBADO CON OBSERVACIONES.** El merge de `main` es limpio y no cambia la pista (§10); se suma O8. Veredicto anterior (head `37f7767`): APROBADO CON OBSERVACIONES. No hay cambios solicitados. O1 y O4 quedan cerradas. O3 queda cerrada en código, con una observación nueva, O3b (§9). Las observaciones no bloquean. La fusión sigue pendiente de las puertas en el Mac del PO (§6 punto 9 de la spec y AGENTS §2). Este informe no aprueba nada en nombre del PO.

## 1. Criterios (P2–P13)

| Criterio | Código | Prueba | Estado |
|---|---|---|---|
| P2: P > 0,5 estricto | `Core/Rally/LooseDrive.cs:12,15` (`power > MinPower`) | `LooseDriveTests.cs:54-55` (0,5 no; 0,51 sí), `:113` | OK |
| P3: mira < `ServeKindLobMinAimY` estricto | `LooseDrive.cs:15` (`aimY < lobMinAimY`). El adaptador lee `rally.Params` (`Runtime/Rally/LooseDriveHint.cs:134`) y no escribe 3 a mano | `LooseDriveTests.cs:56-57` (3,0 no; 2,99 sí), `:120-121` | OK |
| P5: corto = z < línea corta, calculada | `LooseDrive.cs:20-21` (`Length − ShortLineNearBackEdgeFromBack`); no aparece 5,49 | `:37-45` (z en la línea no es corto); `:26-34` mueve la línea y el cajón en `CourtDimensions` | OK |
| P6: lejos = distancia a la lateral más cercana > `ServiceBoxInner` | `LooseDrive.cs:22-23` | `:43-44` (1,60 exacto no es lejos, en las dos laterales) | OK |
| P7: último golpe aplicado, un solo golpe del rival, Winner Rival, DoubleBounce, Unreachable | `LooseDrive.cs:39-42`. El adaptador guarda solo el último golpe aplicado y reinicia la cuenta del rival en cada golpe nuevo (`LooseDriveHint.cs:112-129`) | `:140-175` | OK |
| P4: volea fuera | `LooseDriveHint.cs:117` (`LastOpportunity == Volley`), `LooseDrive.cs:15` | `:58`, `:107` | OK |
| Mira lateral fuera, también con respaldo | `LooseDriveHint.cs:118`; `ShotController.cs:279` (`order.Side.Exists`) | `:59,:100`; PlayMode `ZLooseDriveHintTests.cs:162-166` (`FellBack` con la mira lateral en true) | OK |
| P8: sin bote no hay pista | `RallyDirector.cs:442-451` da null si no hay piso entre el golpe y el contacto del rival; `LooseDrive.cs:19` | `:46`, `:187` | OK |
| P9: una vez por partido, solo si se vio, sin PlayerPrefs | `LooseDriveHint.cs:102,109,184,188-194`. No hay `PlayerPrefs` en el archivo | `ZLooseDriveHintTests.cs:305-366` | OK, ver O1 |
| P10: gana la pista de primeros partidos; no se arrastra | `LooseDriveHint.cs:101-102` y `RallyHud.cs:589-591,310-311` (`Hints.Observe` corre antes). `ClosePause` en `RallyStarted` desarma (`:144-149,171-187`) | `ZLooseDriveHintTests.cs:368-392` | OK |
| P11: no sale con piloto automático salvo `ForceForTests` | `LooseDrive.cs:38`; `QualifyForTests` exige `ForceForTests` (`LooseDriveHint.cs:74`) | `LooseDriveTests.cs:199-203` | OK |
| P12: textos de la §8 | `LooseDrive.cs:46-55` | `LooseDriveTests.cs:64-70` | OK (§5) |
| P13: métrica aditiva `drive-hint` | `Core/Diagnostics/UserMetrics.cs:15,65-69`. Son líneas nuevas, sin cambios en las que ya estaban | `UserMetricsDriveHintTests.cs:72-89` (las líneas actuales quedan byte a byte) | OK |
| Fuera de `PointOver`, en `MatchOver` y en la pausa de saque humano: nunca | `LooseDrive.cs:37`; `LooseDriveHint.cs:157-158`. `EndPoint` pone `MatchOver` antes de `PointEnded` (`RallyDirector.cs:620,627`) | `LooseDriveTests.cs:191-196`; `ZLooseDriveHintTests.cs:394-453` | OK |

## 2. Fuera de alcance

`git diff --numstat origin/main 30fd0c4` toca solo 18 archivos: `UserMetrics.cs` (+7/−0), `LooseDrive.cs`, `LooseDriveHint.cs`, `HintsDevSection.cs` (+6/−0), `RallyDirector.cs` (+20/−1), `RallyHud.cs` (+46/−11), `ShotController.cs` (+7/−2), las pruebas nuevas, `ZDeviceLayoutTests.cs` (+50/−0), la nota y la spec.

No aparecen `DefaultPreset.json`, `RallyParams.cs`, `PointRules.cs`, `RivalAi*`, el solver, P2, `Simulation*`, ClockMap/SimClock, los anillos, `HintPicker.cs`, `FirstMatchHints.cs` ni ningún asmdef. Los hashes defaults `207da8f2c636`, SimulationKey `166a96e319c7` y RallyParams `ebf8f717f8c5` quedan intactos por construcción. No se recalcularon. El `RallyDirector.cs` −1 es la línea `hasPending=false;var at=Evaluate(pendingStep);…`, partida en tres y con el mismo orden de llamadas.

## 3. Lecturas nuevas

- **`ShotController.LastStrokeSideAimed`** (`ShotController.cs:97-100`):
  - se fija al aceptar la orden (`:279`), se publica en `Finish` antes de `ShotResolved` (`:405`) y se limpia en cada lanzamiento junto a `LastStrokeBoast` (`:143`);
  - nadie más la lee (grep: solo `LooseDriveHint.cs:118`);
  - no está en `ShotRecord`, así que las líneas `shot` no cambian;
  - no toca `ShotResult`, el solver, el reloj ni la trayectoria. **OK.**
- **`RallyDirector.LastReturnedBounce`** (`RallyDirector.cs:91-93`):
  - `FirstReturnedBounce` (`:442-451`) recorre `CanonicalContacts` (un campo de solo lectura, `BallSimulationHost.Shot.cs:38`) con el mismo corte que `Evaluate`;
  - se asigna solo si el rival contacta, antes de `RaiseRivalSwing(true)` (`:594,598`), y se limpia en `BeginPoint` antes de `RallyStarted` (`:246,282`);
  - `Evaluate` no cambia, y en el director nada decide con ese valor. **OK.**

## 4. Pruebas existentes

- `ZDeviceLayoutTests.cs`: un solo hunk (`@@ -157,5 +157,55 @@`) con 0 líneas borradas. Solo agrega el método `LooseDriveHint_Fits`. No cambia `Cleanup` ni otro caso. **OK.**
- Ninguna otra prueba existente aparece en el diff.
- Las pruebas nuevas comprueban comportamiento real:
  - EditMode: el RED por aserción contra el stub de `f408bfa` está verificado en el historial; `34b888e` deja `DriveHint` vacío.
  - PlayMode: frecuencia, prioridad, «Seguir» antes de opacidad 1, nunca fuera de lugar y la igualdad de la corrida con la pista encendida o apagada.
  - No aflojan criterios. Las semillas sin positivo terminan en «Stop. Do not loosen».

## 5. Textos visibles

| Texto | Dónde | Revisión |
|---|---|---|
| «Tu drive botó corto: el rival lo aprovechó.» (43) | `LooseDrive.cs:50` | OK: idéntico a la §8, tú, neutro |
| «Tu drive quedó lejos de la pared lateral: el rival lo aprovechó.» (64) | `:51` | OK: idéntico, «pared lateral» completo |
| «Tu drive botó corto y lejos de la pared lateral: el rival lo aprovechó.» (71) | `:52` | OK: idéntico, es el más largo |
| «Pista del drive: sí» / «Pista del drive: no» | `DevPanel/HintsDevSection.cs:23` | OK. Solo DEV: el ensamblado exige `SQUASH_DEV \|\| UNITY_EDITOR` y no entra en la build de producción |

Se cotejaron byte a byte con la spec (grep `-F`). «Drive» está aprobado por P12. No hay chilenismos ni voseo. No cambia ningún texto que ya existía.

## 6. Compilación a ojo

- Namespaces y `using`: se cotejaron contra las definiciones reales. `Opportunity`, `ShotResult`, `StrokeSide` y `ShotRecord` están en `Core.Shot`; `PlayerId`, `PointEndCause`, `ReceptionResult`, `MatchScore` y `HintOracle` en `Core.Rally`; `RallyPhase` y `PointResult` en `Runtime.Rally`. `ShotRecord.Power` y `AimY` son `float` (`ShotRecord.cs:58`). `RallyDirector.Params` es público.
- APIs que usan las pruebas, todas presentes: `SideAim.NearWall`, `ClosedGesture(side:)`, `MetricsLog.CurrentFilePath`, `FirstMatchHints.ForceShownForTests`, `RallyHud.Tick` y `BannerInSeconds`, y los helpers de `ZFirstRunFixture` y `ZDeviceFixture`.
- `LooseDrive` está en Core y usa `UnityEngine.Vector3`, como `CourtDimensions` y el resto de Core. El asmdef de Core no tiene `noEngineReferences`. No usa `MonoBehaviour` ni escena.
- Asmdef sin cambios. Cada carpeta ya tiene las referencias necesarias: PlayMode → Core, Runtime y DevPanel; EditMode → Core; DevPanel → Runtime.
- Los 5 `.cs` nuevos tienen `.meta`. Cada GUID nuevo aparece solo en su `.meta` y en la nota del autor (`git grep` en `HEAD`): `771d5a27…`, `69d90b0e…`, `bad689d2…`, `2d0c5e8a…` y `d2e9efce…`.
- **No verificable sin Unity:**
  - que compile;
  - que la semilla 11 / Medio / `Strong` dé al menos un positivo (`ZLooseDriveHintTests.cs:218,302`);
  - que `dz = .9` caiga al drive de respaldo (`:164`);
  - que la tarjeta del árbitro esté visible en la pausa de `LooseDriveHint_Fits` (`ZDeviceLayoutTests.cs:197`);
  - que el alto de la fila sea > 0 tras dos cuadros;
  - que `FirstFloorAfter` (sin tope superior, `ZLooseDriveHintTests.cs:77-85`) coincida con el bote leído al contacto del rival.

  Todos estos riesgos fallarían en rojo, no darían un falso verde.

## 7. Observaciones (no bloquean)

- **O1. El tope de P9 depende de la métrica.** `usedThisMatch = true` solo se fija dentro de `ClosePause`, después de `if (!Enabled || app.UserEvents == null) return;` (`LooseDriveHint.cs:180-184`).
  - Hoy no tiene efecto: `AppBootstrap` siempre crea `UserEvents` (`AppBootstrap.cs:223`).
  - Si algún día la métrica no existe, la pista saldría en cada punto que califique.
  - Lo correcto es marcar la cuota usada antes de la escritura de la métrica, sin depender de ella.
- **O2. El oráculo del criterio 3 no es del todo independiente.** La prueba de escena junta los hechos por su cuenta (bote desde `CanonicalContacts` y `ShotRecord`), pero decide con el mismo `LooseDrive.Judge` (`ZLooseDriveHintTests.cs:256-260`). La lógica de `Judge` sí está cubierta caso a caso en EditMode.
- **O3. El criterio 6 puede quedar vacío.** `RunCompare` no exige que en la corrida encendida se haya armado o mostrado alguna pista (`ZLooseDriveHintTests.cs:460-507`). Por lectura, el adaptador no devuelve nada al juego, así que el riesgo es bajo.
- **O4. Casos de EditMode repetidos.**
  - `LooseDriveTests.cs:100-101` repite la misma aserción con dos mensajes.
  - `NotApplied_IsNone` recibe `result` y no lo usa (`:124-131`): los cuatro casos son el mismo.
  - El paso de `Result` a «aplicado» vive en el adaptador (`LooseDriveHint.cs:114`) y solo lo cubre PlayMode, de forma indirecta.
  - El bucle `foreach … break` de `ZLooseDriveHintTests.cs:148-160` corre una sola vez.
- **O5. Cancha fija.** El adaptador usa `CourtDimensions.Standard()` (`LooseDriveHint.cs:136`) y no la cancha efectiva. Hoy da lo mismo, porque la única variante (`WithTinTop`) no cambia el largo, el ancho, la línea corta ni el cajón.
- **O6. Sangría del texto.** Con el icono oculto, `.sc-hint-text` conserva `margin-left: 8px` (`Runtime/Resources/Visual/Hud.uss:161`): el texto queda corrido 8 px. Es cosmético; conviene mirarlo en el iPhone.
- **O7. Pruebas existentes que juegan sin piloto, con la pista encendida.** Son `ZFirstMatchHintsLayoutTests.cs:108`, `ZStrokeOfTheMatchTests.cs:91`, `ZFirstMatchHintsTests.cs:319`, `ZReceptionRingsTests`, `ZRingClosePulseTests`, `ZServeReceptionMarginTests` y `ZVolleyEnergyTests`. Si en alguna un punto califica, una aserción de «fila oculta» puede fallar. Es defecto según la spec §10: se registra y se detiene, sin editar la prueba. Solo el recuento completo en el Mac lo resuelve.

## 8. Notas de proceso (no son hallazgos)

- La spec §5 pide declarar en la PR la lectura de `ShotController`, que está junto a la zona del contrato (Codex, AGENTS §4). Por lectura, el cambio no toca el contrato.
- El interruptor DEV queda protegido por el asmdef (`SQUASH_DEV || UNITY_EDITOR`) y no por un `#if SQUASH_DEV`. Es igual que «Pistas» y «Reiniciar pistas».
- Para la prueba en el iPhone: con P7, si el jugador no hace swing ante una pelota que no alcanza, la recepción es `NoSwing` y no sale la pista. Sirve para leer la métrica `drive-hint`.
- Faltan las puertas antes de fusionar: EditMode y PlayMode completos en el Mac del PO, `check-release` y AutoQA (spec §6.9).

## 9. Delta `30fd0c4..37f7767` (28 nov, encargo `set27c`)

Se revisó sin mezclar código en esta rama: `git fetch origin sc-pista-drive-suelto`, `git diff 30fd0c4..37f7767` y `git show origin/sc-pista-drive-suelto:<archivo>`.

- **Cinco commits:** `f8cc892` (test O1), `5464559` (fix O1), `ac395a2` (test O3), `e1931e6` (O4) y `37f7767` (nota).
- **Cuatro archivos:** `LooseDriveHint.cs` (+5/−2), `LooseDriveTests.cs`, `ZLooseDriveHintTests.cs` y la nota del autor.
- **Nada más cambió:** ni producción fuera de `ClosePause`, ni física, IA, contrato, `RallyParams`, `DefaultPreset.json`, SimulationKey, asmdef, `.meta`, textos ni pruebas existentes. O2, O5, O6 y O7 quedan tal como estaban.

**O1: cerrada.**
- `LooseDriveHint.cs:178-182`: `already = usedThisMatch` se guarda primero. Después, `if (seen && !occupied) usedThisMatch = true;` corre antes del `return` por `UserEvents == null` (`:183`).
- La métrica no cambia de significado: `firstMatchHint`, después `limit` (con el valor previo, `:186`), después `""` y `notSeen`. Es la misma tabla de antes.
- La prueba `SeenOnce_BlocksTheNextPoint_WhenUserEventsIsNull` (`ZLooseDriveHintTests.cs:365-382`) cubre el caso de verdad:
  - `DropUserEvents` (`:105-122`) quita por reflexión solo los handlers de `AppBootstrap` en `PointEnded` y `ModeChanged`, que usan `UserEvents` sin null (`AppBootstrap.cs:368-374`);
  - el de `RallyStarted` sale antes de usarlo cuando el partido es el mismo (`:362`);
  - luego deja `UserEvents` en null con el setter privado (`AppBootstrap.cs:32`).
  
  Con el código de `30fd0c4`, la segunda inyección se habría mostrado y la aserción de `:382` falla. Con el arreglo pasa. RED y GREEN no se ejecutaron.
- Los otros usos de `UserEvents` en producción tienen null o un chequeo previo (`FirstMatchHints.cs:253` está dentro del `if`).

**O3: cerrada en código, con O3b.**
- `RunCompare` vuelve a contar de forma independiente con `Judge` (`:520-557`) y exige dos cosas (`:591-593`):
  - `on.LooseDrives > 0`;
  - `on.HintShown`, es decir, que una de las tres frases se vio en una pausa ordinaria (`:573`).
- Sin positivo, la prueba falla con «Stop. Do not loosen», y no se esconde.
- Las entradas no cambian: semilla 5, Fácil, σ 25, Fondista y `ServeKindOn` 1 (`:514-519`). Los cortes (`LooseDrive.cs`) son los mismos. No se fabricó un positivo.
- **La prueba no es estructuralmente roja.**
  - `RunCompare` no fija la potencia, así que queda la de fábrica, `Medium` (`RallyDirector.cs:77`). Esa opción tiene P = `PowerMedium` 0,5 (`RallyParams.cs:59`, `ShotOptions.cs:51`), que por P2 no es drive.
  - Pero `BotBrain.Decide` (`Core/Rally/RallySim.cs:70-74`) recorre las 18 opciones ordenadas por `Distance` (`:76`). Elige la primera cuyo pronóstico es bueno.
  - La segunda en ese orden es Recta/Media/**Strong** (distancia 1), con P = `PowerStrong` 0,85 y mira `AimMid` 1,7 m. Esa opción es un drive.
  - `ShotRecord.Power` es la P de la orden (`ShotController.cs:431`). Así que cada vez que la Recta/Media/Medium no tiene un pronóstico bueno, el piloto pega un drive válido.
  - Que con la semilla 5 salga un drive suelto que además califique (corto o abierto, una devolución, `Unreachable`) **no se puede saber sin correrlo**.
  - La nota del autor dice «lo esperado es el fallo», sin evidencia en ningún sentido.
- O4 y el recuento no cambian la igualdad. Los mismos handlers de prueba están en las dos corridas y solo leen.

**O3b (observación, no bloquea).** Esta puerta puede salir roja en el Mac.
- Si sale roja, AGENTS §2 manda registrar y detenerse. No se cambia semilla ni potencia, porque el criterio 6 fija las entradas del criterio 9 de front 2.
- Una salida que no toca el juego, para que decidan SC-PM y el PO:
  - mantener las entradas de front 2;
  - en las dos corridas, inyectar un punto calificado con `QualifyForTests`, que no devuelve nada al juego;
  - exigir `HintShown` solo en la corrida encendida;
  - dejar la exigencia del positivo natural en la prueba de escena (semilla 11, `Strong`, `:218,302`), que ya la tiene.
- Es un cambio de prueba. Según la regla, solo se hace tras registrar el resultado.

**O4: cerrada sin perder cobertura.**
- `TwoWallOrder_…` (`LooseDriveTests.cs:98-101`) queda con una aserción. Las dos que había eran idénticas.
- `NotApplied_IsNone` (`:123-127`) queda en un solo `[Test]`. El parámetro nunca entraba a `Judge`.
- La cobertura del respaldo a drive sigue en PlayMode (`ZLooseDriveHintTests.cs:162-166`).

**Pendiente:** las puertas de Unity. Son EditMode y PlayMode completos en el Mac del PO, `check-release` y AutoQA. Nada de esto se corrió en esta revisión.

## 10. Merge de `main` en `980cf39` (28 nov, encargo `set28`)

Se revisó sin mezclar código en esta rama: `git fetch origin sc-pista-drive-suelto main`, `git diff 37f7767...980cf39` y `git show 980cf39:Assets/SquashClash/Runtime/Rally/RallyDirector.cs` frente a `37f7767` y `967797a`. Sin Unity.

- **Commit:** `980cf39` tiene padres `37f7767` (pista) y `967797a` (`origin/main`, merge de la PR #124 de posiciones de saque). Merge-base `99b4306`, la misma base de §1–§9.
- **Sin resolución a mano.** `git merge-tree --write-tree 37f7767 967797a` da el árbol `1ec1c92829fb…`, idéntico al de `980cf39`. El merge es exactamente la fusión automática.
- **Sin cambios extra.**
  - `git diff 37f7767 980cf39` y `git diff 99b4306 967797a` tocan los mismos 62 archivos con el mismo `--stat` (+7030/−37).
  - Los 61 archivos que solo cambió `main` quedan byte a byte como en `967797a`.
  - Los 17 que solo cambió la pista quedan byte a byte como en `37f7767`: `LooseDrive.cs`, `LooseDriveHint.cs`, `RallyHud.cs`, `ShotController.cs`, `UserMetrics.cs`, `HintsDevSection.cs`, las pruebas de la pista, `ZDeviceLayoutTests.cs`, la nota y la spec.
  - El único archivo de los dos lados es `RallyDirector.cs`.
- **`RallyDirector.cs` conserva las dos aportaciones, sin mezcla.**
  - Los hunks no se solapan. La pista agrega el `using SquashClash.Core.Court`, la propiedad, el borrado en `BeginPoint`, `FirstReturnedBounce` y la asignación. `main` agrega dos líneas en el saque de `BeginPoint`.
  - `git patch-id --stable`: `967797a..980cf39` da el mismo id que `99b4306..37f7767` (`56ac73ff…`), y `37f7767..980cf39` el mismo que `99b4306..967797a` (`d1a21b0c…`).
- **El Teleport del receptor de #124 está:** `RallyDirector.cs:276-277`, `(server==PlayerId.Human?Rival:Human).Teleport(ServePlanner.ReceiverPosition(context,box));`, con `ServePlanner.ReceiverPosition` en `Core/Rally/ServePlanner.cs:27`. Va después de `body.MoveTo(...)` del sacador, como en `main`.
- **`LastReturnedBounce` sigue siendo solo lectura.**
  - En producción solo aparece en `RallyDirector.cs:93` (propiedad con setter privado), `:246` (null en `BeginPoint`), `:444` (`FirstReturnedBounce`), `:581` (lectura antes de `Evaluate`) y `:596` (asignación tras el contacto del rival). Fuera del director, solo la lee `LooseDriveHint.cs:128`.
  - El cuerpo de `Evaluate` es idéntico en `99b4306`, `37f7767` y `980cf39` (mismo SHA del bloque, `07c5a9c67e0c`). Ninguna decisión del punto lee el valor.
  - Las líneas que §1 y §3 citan del director se corren +2 desde la línea 273 (por ejemplo, `:579→:581`, `:594→:596`). El contenido es el mismo.
- **Criterios, textos, pruebas y hashes de la pista: sin cambios por esta rama.** `LooseDrive.cs`, los textos de §5 y las pruebas de la pista son los de `37f7767`. `ServeKindLobMinAimY` sigue en 3 (`RallyParams.cs:44`), que es el corte de P3.
- **Hashes que trae `main` (no son de esta rama).** #124 cambia las miras `ServeKindLob*` en `RallyParams.cs:41`. RallyParams pasa de `ebf8f717f8c5` a `249979628e7c` (`RivalAiTests.cs:63`), según el informe de #124 (`docs/reviews/2026-11-26-posiciones-saque.md:34`). Con «Saque nuevo» apagado, `5fc9f2ff92e5` → `45faf866376c`. Esta rama no toca `RallyParams.cs`, `DefaultPreset.json` ni SimulationKey. No se recalcularon.

**O8 (observación nueva, no bloquea). Las pruebas con semilla de la pista nunca corrieron con el saque de #124.**
- #124 mueve al sacador (`ServePlanner.BoxPosition`), coloca al receptor (`ReceiverPosition`) y cambia las miras del globo. Desde el primer punto, las trayectorias, los contactos y las decisiones de los bots son otras.
- Las pruebas de escena de la pista dependen de un positivo natural con entradas fijas: la semilla 11 / Medio / `Strong` (`ZLooseDriveHintTests.cs:218,302`) y `RunCompare` con la semilla 5 (O3b). Esos positivos se buscaron sobre la base `99b4306`, sin el saque nuevo. Con `980cf39` pueden aparecer o desaparecer.
- O7 sigue igual. #124 además edita `ZFirstMatchHintsTests.cs`, `ZRingClosePulseTests.cs` y `ZServeReceptionMarginTests.cs`, con la pista encendida.
- Si alguna sale roja en el Mac, rige AGENTS §2: registrar y detenerse, sin cambiar semilla, potencia ni criterio.
- Las puertas (EditMode y PlayMode completos en el Mac del PO, `check-release` y AutoQA si aplica) se corren sobre `980cf39`. No sirve ningún resultado de `37f7767`.

**Veredicto para `980cf39`: APROBADO CON OBSERVACIONES.** No hay cambios solicitados. Quedan abiertas O2, O3b, O5, O6, O7 y O8. Faltan las puertas de Unity. Este informe no aprueba nada en nombre del PO.
