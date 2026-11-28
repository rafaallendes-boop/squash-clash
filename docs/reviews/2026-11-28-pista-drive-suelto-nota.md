# Pista del drive suelto · nota de código

28 nov 2026. Rama local `rafaallendes-boop/sc-pista-drive-suelto`, que sigue a `origin/sc-pista-drive-suelto`. Base de la spec: `654d243`. No hay PR, ni fusión, ni instalación. La revisión de Claude es previa al merge y no la hace este encargo.

Commits, del más viejo al más nuevo, sobre `654d243`:

- `e615d8a` docs: decisiones del PO en la spec, sin otro cambio.
- `f408bfa` test: clasificación y causa, contra un stub que responde `None` o `false`.
- `b7a8b86` feat: `LooseDrive`.
- `34b888e` test: la línea `drive-hint` contra `DriveHint` vacío.
- `36bd39a` feat: la línea aditiva.
- `1027e33` feat: `LastStrokeSideAimed` y `LastReturnedBounce`.
- `b90aa80` feat: el adaptador, el enganche del HUD y el botón DEV.
- `d095045` test: PlayMode de los criterios 3 a 6 y de las dos lecturas.
- `c4bb111` test: el criterio 7, un método nuevo en `ZDeviceLayoutTests`.

Los dos últimos se escribieron después del código. El tipo ya existía: un test de PlayMode no compila contra un adaptador que no está. El RED de PlayMode no se ejecutó. Abajo queda dicho en cada prueba.

## Pruebas

Nada de esta lista se corrió. «GREEN» es lo que la aserción pide cuando el código está, no un resultado observado.

### EditMode · `LooseDriveTests` (RED `f408bfa`, GREEN `b7a8b86`)

El stub de `f408bfa` deja `IsDrive` en `false`, `Classify` y `Judge` en `None`, y `Text` en `""`. El RED es por aserción en todo lo que espera un tipo, un drive o una frase. Los negativos que esperan `None` ya pasan con el stub; no son el RED.

| Prueba | Aserción clave | RED antes del código | GREEN después |
|---|---|---|---|
| `Classify_UsesTheCourtLines_NotHandwrittenCuts` | (3,2; 5,40) es `ShortAndWide`; (0,8; 5,40) `Short`; (3,2; 8,0) `Wide`; (0,8; 8,0) y (5,6; 8,0) `None` | falla: el stub da `None` | los cinco puntos |
| `Classify_ReadsTheShortLineAndTheBoxWidthFromTheCourt` | al mover la línea corta, z = 5,40 deja de ser corto y queda `Wide`; al poner el cajón en 3,2 m, ese x deja de ser lejos y queda `Short` | falla en la primera aserción: el stub da `None` y se espera `Wide` | lee `CourtDimensions` |
| `Classify_OnTheShortLine_IsNotShort_AndOnTheServiceBoxWidth_IsNotWide` | z igual a la línea corta no es corto; distancia igual a `ServiceBoxInner` no es lejos; bote nulo o cancha nula es `None` | el caso «exactamente el ancho y corto» espera `Short` y el stub da `None` | bordes estrictos |
| `IsDrive_PowerAndAimAreStrict_AndVolleySideAndMissAreNot` | P 0,5 no; P 0,51 sí; AimY en la línea del globo no; 0,01 por debajo sí; volea, mira lateral y no aplicado no | falla en P 0,51 y en AimY 2,99 (el stub da `false`) | cortes estrictos, `MinPower` = 0,5 |
| `Text_IsTheThreeSentences_AndNoneIsEmpty` | las tres frases de la §8, y `None` es `""` | falla: el stub devuelve `""` | las tres frases |
| `ALooseDrive_TheRivalReturnedOnce_AndTheHumanCouldNotReach_IsItsKind` | el caso positivo de `Judge` da su tipo | falla: el stub da `None` | el tipo del bote |
| `TwoWallOrder_AndAFallbackToTheDrive_AreNone_BecauseTheOrderAimedAtTheSide` | mira lateral, también si el boast cayó a drive, es `None` | ya pasa en el stub | sigue en `None` |
| `AVolley_IsNone` | volea es `None` | ya pasa en el stub | sigue en `None` |
| `PowerAtOrBelowTheCut_IsNone` | P en el corte no es drive | ya pasa en el stub | sigue en `None` |
| `AimAtOrAboveTheLobLine_IsNone` | AimY en la línea del globo no es drive | ya pasa en el stub | sigue en `None` |
| `NotApplied_IsNone` | el golpe no aplicado es `None` | ya pasa en el stub | sigue en `None` |
| `AHumanServe_HasNoPendingDrive_AndIsNone` | el saque del humano no arma la pista | ya pasa en el stub | sigue en `None` |
| `TheHumanWins_OrThePointHasNoWinner_IsNone` | ganar, o no haber ganador, es `None` | ya pasa en el stub | sigue en `None` |
| `TheHumanLosesByHisOwnFault_IsNone` | la falta propia no es esta pista | ya pasa en el stub | sigue en `None` |
| `TheHumanMissedByTimingOrDidNotSwing_IsNone` | aire, tarde o sin swing es `None` | ya pasa en el stub | sigue en `None` |
| `TheRivalDidNotReturnExactlyOnce_IsNone` | cero o dos devoluciones es `None` | ya pasa en el stub | sigue en `None` |
| `ADriveThatIsNotTheLastAppliedHumanStroke_IsNone` | no es el último golpe aplicado | ya pasa en el stub | sigue en `None` |
| `ABounceBehindTheShortLineAndInsideTheBoxWidth_IsNone` | bote largo y pegado es `None` | ya pasa en el stub | sigue en `None` |
| `NoBounce_TheRivalVolleyed_IsNone` | sin bote es `None` | ya pasa en el stub | sigue en `None` |
| `HumanServePause_MatchOver_PracticeAndRallyWithoutAMatch_AreNone` | pausa de saque, partido terminado, práctica o sin partido | ya pasa en el stub | sigue en `None` |
| `Autopilot_IsNone_UnlessTheTestPointForcesIt` | piloto automático es `None` salvo `ForceForTests` | el caso forzado espera un tipo y el stub da `None` | solo el forzado califica |
| `Disabled_IsNone` | apagada es `None` | ya pasa en el stub | sigue en `None` |

### EditMode · `UserMetricsDriveHintTests` (RED `34b888e`, GREEN `36bd39a`)

En `34b888e`, `DriveHint` existe y no escribe nada. `Only` exige exactamente una línea: el RED es por aserción, no por compilación.

| Prueba | Aserción clave | RED | GREEN |
|---|---|---|---|
| `DriveHint_WritesOneLineForTheNamedMatch_WithTheBounceAndWhetherItWasSeen` | una línea `drive-hint` con `matchId`, `kind` `Short`, bote, `seen` true, `reason` `""`, y las claves en ese orden | no hay línea | la línea y el orden |
| `DriveHint_RecordsWhyItWasNotSeen` | `limit`, `firstMatchHint` y `notSeen` quedan con `seen` false | no hay línea | la razón pedida |
| `DriveHint_UsesTheMatchIdItIsGiven_NotALaterMatch` | el id es el que se pasa, no el del partido que se abrió después | no hay línea | el id viejo |
| `CurrentLines_StayTheSame_WhenADriveHintIsWritten` | `match-start`, `hint`, `serve-choice` y `match-end` quedan byte a byte; la línea nueva es aparte | las cuatro líneas sí están; falla al exigir la quinta | las cuatro idénticas y una `drive-hint` |

### PlayMode · `ZLooseDriveHintTests` (`d095045`)

RED no ejecutado. Si estas pruebas se hubieran compilado contra el stub de las lecturas (campo ausente) no habrían compilado. Contra el adaptador de `b90aa80` deben quedar verdes. Si una aserción falla, se registra y se detiene: no se afloja.

| Prueba | Qué cubre | Aserción clave | GREEN esperado |
|---|---|---|---|
| `LastStrokeSideAimed_IsTheOrdersSideInsideShotResolved_AndClearsOnTheNextLaunch` | lectura de `ShotController` | dentro de `ShotResolved`, la orden de frontis es false y la orden con `Side` es true, también si `FellBack`; el lanzamiento siguiente la limpia; la preview no la publica | las tres |
| `LastReturnedBounce_IsTheFirstFloorAfterTheHumanStroke_AndClearsOnTheNextPoint` | lectura de `RallyDirector` | en `RivalSwing(true)`, el bote iguala el primer piso no persistente con `Step` mayor que el `ContactStep` del golpe humano; `BeginPoint` lo deja en null. Semilla 11, Medio, σ 0, `Strong`, Fondista | al menos un bote, y una limpieza |
| `Scene_BannerMatchesAnIndependentLooseDrive_AndShowsAtLeastOne` | criterio 3 | en cada pausa ordinaria el texto es `LooseDrive.Text(k)` solo si un `Judge` independiente da `k != None` y no hubo otra vista; si no, la fila está en `None`. Exige al menos un positivo | partido hasta `MatchOver`, con al menos una pausa y un positivo |
| `OncePerMatch_ASeenHintBlocksTheNext_AndARematchAllowsIt` | criterio 4, P9 | la primera inyección vista (`opacity` 1 y alto de fila > 0) escribe `reason` `""`; la segunda, `limit` y no se muestra; `Rematch` vuelve a mostrarla | las dos líneas y la frase del partido nuevo |
| `SeguirBeforeOpacityOne_DoesNotCount_AndTheNextPauseCanShowIt` | criterio 4 | `Tick(0,05)` deja `BannerOpacity` < 1; la línea es `notSeen`; la pausa siguiente sí cuenta | `notSeen` y luego `seen` |
| `FirstMatchHint_WinsThePause_AndTheDriveDoesNotCarry` | criterio 4, P10 | con `ForceShown(Volley)` sale el texto de esa pista, el icono no está en `None`, `Drive.Shown` es `None` y la razón es `firstMatchHint`; la pausa siguiente, sin repetir el punto, no muestra el drive | la pista de volea gana y no se arrastra |
| `NeverOutsideAnOrdinaryPause_AndOffShowsAndWritesNothing` | criterio 5 | el texto de drive solo con `PointOver`, sin pausa de saque humano y sin partido terminado; con `Enabled` false, `QualifyForTests` no muestra ni agrega líneas | una aparición en la primera corrida y cero líneas nuevas en la segunda |
| `NoAddedTime_OnMatchesOff` | criterio 6 | mismas entradas que el criterio 9 de front 2 (`σ` 25, Fácil, semilla 5, Fondista, `ServeKindOn` 1 en las dos corridas). `ForceForTests` en las dos. Solo cambia `Enabled`. El bucle no llama a `hud.Tick`, igual que `RunStepped` | igualdad exacta de `PointResult`, `WaitBetweenPoints` y el paso continuo de inicio |
| `DevButton_TogglesTheDriveHint` | interruptor DEV | el botón `drive-hint-toggle` dice «Pista del drive: sí/no» y alterna `Enabled`; «Reiniciar pistas» no cambia | los tres textos |

La semilla del criterio 3 y de la lectura del bote es 11, nivel Medio, σ 0, potencia `Strong`, `RandomStyles` false, estilo Fondista, sin `SetParams` y sin `FirstMatchHints.BeginMatch`. Si no aparece un positivo, el mensaje de la prueba lo dice. Ahí se detiene. No se cambia la semilla ni el criterio.

Las inyecciones usan potencia `Medium` (0,5, que no es drive) para que un punto natural no arme la pista antes de la inyección.

### PlayMode · `LooseDriveHint_Fits` (`c4bb111`)

RED no ejecutado. Método nuevo al final de `ZDeviceLayoutTests`. `Cleanup` y los métodos que ya estaban no se tocaron. El `finally` repone `LooseDriveHint`, `FirstMatchHints` y `PointEndFreeze`.

Aserción clave, en los cuatro teléfonos: el texto largo de `ShortAndWide`, la fila visible, el icono oculto, `AssertFits("rally-banner")`, como máximo dos líneas, sin elipsis, el aviso debajo de la cabecera, la tarjeta del árbitro visible y sin solaparse con la fila. GREEN: eso en SE, Mini, Pro y Pro Max.

## Qué no se pudo verificar

- No se abrió Unity ni el editor.
- No se corrió `scripts/test.sh`, ni EditMode ni PlayMode, ni un filtro.
- No se corrió `scripts/check-release.sh` ni `scripts/autoqa.sh`.
- `dotnet` no está instalado (`command not found`). No se instaló. No hubo compilación aislada en `/tmp`.
- No se compiló el proyecto de ninguna otra forma. Los nombres se cotejaron por lectura con el código de la rama (`LooseDrive.Judge`, `HintOracle.BannerSeen`, `ClosedGesture`, `ContactEvent`, `PointResult`, `RallyHud.Refresh` / `Tick`, `FirstMatchHints`, `UserMetrics.DriveHint`, `ZFirstRunFixture`, `ZDeviceFixture`).
- La semilla 11 no se buscó. No hay evidencia de que `Strong` produzca un drive suelto.
- No se midió si el alto de `rally-banner-hint` pasa de 0 tras dos `yield return null`.
- Los hashes no se recalcularon. Los archivos de defaults, `SimulationKey` y `RallyParams` no están en el diff desde `654d243`.

## Riesgos

- Compilación sin ver. Un nombre, un `using` o un `.meta` pueden fallar en el primer `scripts/test.sh`.
- `.meta`: formato `fileFormatVersion: 2` y un guid de 32 hex, igual que el resto. Guids nuevos: `LooseDrive` `771d5a277d9a420aa0abece3f05a9a12`, `LooseDriveTests` `2d0c5e8a3207447ab1dd29a2e6ce479e`, `LooseDriveHint` `69d90b0e5053441ca351be0be18433b8`, `UserMetricsDriveHintTests` `bad689d2a22f4f628fd487d1735855a0`, `ZLooseDriveHintTests` `d2e9efcec5384289bb9e0591d8a23904`. Cada uno aparece una vez en el árbol. `ZDeviceLayoutTests` ya tenía meta.
- Asmdef sin cambios. `LooseDrive` está en Core y usa `UnityEngine.Vector3`, como el resto de Core; el asmdef de Core no gana referencias. `LooseDriveHint` está en Runtime y no es un `MonoBehaviour`. Los tests nuevos caen en los asmdef de su carpeta. PlayMode ya referencia Core, Runtime y DevPanel.
- El bote en `CanonicalContacts` al compromiso del rival. `FirstReturnedBounce` camina la misma lista que `Evaluate`, antes de `RacketContact`, y `LastReturnedBounce` se asigna solo si el rival contacta, antes de `RaiseRivalSwing(true)`. La prueba independiente no corta en el paso del rival: toma el primer piso con `Step` mayor que el `ContactStep`. Si en ese instante la lista ya tiene un piso posterior, la prueba falla. Ahí se detiene.
- `flightStartStep` del director es el `ContactStep` del golpe humano (`TickHuman`). La prueba usa el `ContactStep` del `ShotRecord`. Si no son el mismo paso, la igualdad del bote falla. Ahí se detiene.
- Semilla 11 / Medio / σ 0 / `Strong` / Fondista puede no dar un caso positivo. El criterio 3 y la prueba del bote lo exigen. Si no aparece, se registra y se detiene. No se afloja.
- Alto de fila en 0. `BannerSeen` pide alto > 0. `ZFirstRunFixture` deja `hud.enabled` en false, así que la opacidad solo avanza con `hud.Tick`. Si el layout no da alto, la pista vista no cuenta y `OncePerMatch` falla. No se cambia `HintOracle`.
- Pruebas existentes. No se editó ninguna. `ZRingClosePulseTests`, `ZReceptionRingsTests`, `ZServeReceptionMarginTests` y `ZVolleyEnergyTests` siguen con `Autopilot = false` (leído en esta pasada; no están en el diff). Con la pista encendida por defecto, un partido sin piloto que pierda un punto por un drive suelto puede mostrar la fila o escribir `drive-hint`. `ZVolleyEnergyTests` filtra `kind == "fatigue"`, así que una línea de más no le cambia el último fatigue. `ZFirstMatchHintsTests` afirma en varios puntos que `rally-banner-hint` no se ve. Si uno de esos puntos califica, falla. Es un defecto: se registra y se detiene, sin editar esa prueba. El piloto automático sin `ForceForTests` no muestra ni escribe.
- Zona del contrato en `ShotController`. La lectura nueva está junto a `LastStrokeBoast`. No cambia el resultado, el solver, el timing ni la trayectoria. El detalle está en la sección de abajo. Si el verificador ve que un test de contacto o de boast cambió de resultado, se detiene.
- El botón DEV no va dentro de `#if SQUASH_DEV`. El editor que corre `scripts/test.sh` no define `SQUASH_DEV` (se agrega solo en builds que no son prod). El asmdef de DevPanel ya exige `SQUASH_DEV || UNITY_EDITOR`, igual que «Pistas» y «Reiniciar pistas». La build de producción no incluye ese ensamblado. Un `#if` extra habría dejado el botón fuera del test de PlayMode.
- `OnRivalSwing` recibe `side` y no lo usa. El proyecto no trata los warnings como errores. Si el compilador del verificador sí, se descarta el parámetro sin cambiar el comportamiento.
- `NoteMatch` borra una inyección pendiente cuando cambia el objeto `Match` (revancha). En el mismo partido no la borra. El orden en `OnRallyStarted` es cerrar la pausa, limpiar el golpe y después anotar el partido. Invertirlo perdería la cuota o la inyección.
- El id de la métrica se copia en `Arm`. `AppBootstrap` abre el partido en `RallyStarted` antes que el adaptador del HUD, porque se suscribe primero. Cerrar la pausa usa esa copia, no el id vivo.
- `Attempt` de la prueba de mira lateral es una clase. Un struct copiado dentro de `ShotResolved` no vería los campos que escribe el callback.
- El criterio 3, cuando el texto esperado está vacío, exige la fila en `None`. No llama a `BeginMatch`, así que no debería haber pista de primeros partidos. Si aparece, la prueba falla y se detiene.
- La tarjeta del árbitro en el criterio 7 se exige visible. Si en esa pausa no se dibuja, el «no se solapan» dejaría de ser vacío y la prueba falla. Ahí se detiene.
- `RallyParams`, `DefaultPreset.json` y `SimulationKey` no se editaron. No hay campo nuevo. Los hashes que la spec pide intactos siguen siendo los de `main`: defaults `207da8f2c636`, SimulationKey `166a96e319c7`, RallyParams `ebf8f717f8c5`. No se volvieron a medir.

## Textos

- Corto: «Tu drive botó corto: el rival lo aprovechó.»
- Lejos: «Tu drive quedó lejos de la pared lateral: el rival lo aprovechó.»
- Corto y lejos: «Tu drive botó corto y lejos de la pared lateral: el rival lo aprovechó.»

Están en `LooseDrive.Text` y en las pruebas. Español neutro con tú. «Drive» y «pared lateral» quedan como los aprobó el PO al dejar P12 en sí.

## Lectura de ShotController

`LastStrokeSideAimed` dice si la orden que sale por `ShotResolved` apuntaba a una pared lateral (`order.Side.Exists`), también cuando el boast cae a drive. No es `LastStrokeBoast`: esa marca exige el solve autoritativo, aplicado, sin fallback.

- En `Process`, junto a `authoritativeBoast = false`, se guarda `strokeSideAimed = order.Side.Exists`.
- En `Finish`, justo después de asignar `LastStrokeBoast` y antes de `ShotResolved`, se publica `LastStrokeSideAimed = strokeSideAimed`.
- En `OnLaunched` se limpia junto con `LastStrokeBoast`.
- `CloseReception` no la publica. Una preview o un contacto todavía en espera no la afirma: la marca pública solo se escribe en `Finish`.

No entra al contrato de contacto. No cambia `ShotResult`, el solver, el reloj ni la trayectoria.

`RallyDirector.LastReturnedBounce` (`Vector3?`) es la otra lectura, fuera de `ShotController`. Es el primer piso no persistente del vuelo humano hasta el paso del contacto del rival. Se asigna solo si el rival contacta, y `BeginPoint` lo pone en null. `Evaluate` no se modificó.

## Comandos del verificador

Cuando Unity esté libre, en este worktree, sin otro Unity abierto:

```
scripts/test.sh editmode LooseDriveTests
scripts/test.sh editmode UserMetricsDriveHintTests
scripts/test.sh playmode ZLooseDriveHintTests
scripts/test.sh playmode LooseDriveHint_Fits
scripts/test.sh editmode
scripts/test.sh playmode
scripts/check-release.sh
scripts/autoqa.sh mvp
```

AutoQA no lo pide AGENTS §3 (este cambio no toca timing, gesto ni contrato). La spec §6.9 lo pide como evidencia de que ninguna decisión cambió. Si el criterio 3 o la prueba del bote no encuentran un positivo con la semilla 11, se registra y se detiene.

Este encargo no corrió ninguno de esos comandos. No tocó `/Users/rafaallendes/sc-unity.lock`. No tocó ni copió `scripts/device.env`.
