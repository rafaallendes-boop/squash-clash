# Revisión cruzada · pista «tu drive quedó suelto»

28 nov 2026. Encargo `set27-revision-pista-drive-suelto` de SC-PM. Revisión de solo lectura, sin Unity. No se corrieron `scripts/test.sh`, AutoQA ni `check-release`. No se tocaron `sc-unity.lock` ni `scripts/device.env`.

- **Head revisado:** `30fd0c4355dc8fae0353e8fafc1b2c5a2827da24`, igual a `origin/sc-pista-drive-suelto`. Base `origin/main` `99b4306` (merge-base). El delta de la spec contra `654d243` es solo la sección «Decisiones del PO».
- **Autor:** Grok, por la excepción de Rafael del 28 nov 11:44. Revisor: Claude (Opus), que no escribió el código.
- **Veredicto preliminar: APROBADO CON OBSERVACIONES.** No hay cambios solicitados. Las observaciones no bloquean. La fusión sigue pendiente de las puertas en el Mac del PO (§6 punto 9 de la spec y AGENTS §2). Este informe no aprueba nada en nombre del PO.

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
