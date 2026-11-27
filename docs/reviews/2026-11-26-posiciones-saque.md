# Posiciones de saque · DETENIDO por pruebas existentes en PlayMode

**Actualización 27 nov, 13:10 (`set16d`, arreglo clase A de `VisualKit.Joint`).** Causa en `set16c`, sección 7: cuando la mano cae sobre el hombro (`d < 1e-4`), la guarda dejaba `dir = to/d` con largo 10 000 y la muñeca quedaba 110 m abajo.

- **RED** (`8843fdf`): `VisualKitJointTests.TargetOnTheRoot_KeepsTheChainWithinItsLength` (EditMode, objetivo en la raíz y a 5e-5 m, largos del brazo 0,27/0,26) exige que |fin − raíz| y |codo − raíz| sean ≤ l1 + l2. Falló 2/2 por aserción: «end», 110,000206. Para alcanzar `VisualKit`, el asmdef de EditMode ahora referencia `SquashClash.Runtime`; no tenía esa referencia.
- **GREEN** (`5bbc73f`): `var dir=d<1e-4f?Vector3.down:to/d;d=Mathf.Max(d,1e-4f);`. Con `d ≥ 1e-4` la función no cambia. Filtro 2/2.
- **Suites** sobre `5bbc73f`, con el candado y sin reintentos: EditMode **1082/1082** (1080 + 2 nuevas; XML SHA-256 `72a9b0d9…1529`). PlayMode **total=893 passed=887 failed=6** (XML SHA-256 `2a203c90…c5b2`). `ZHandednessTests` pasa 6/6. Los 6 fallos son exactamente los esperados: `ZFirstMatchHintsTests` ×4, U1a y `ZServeReceptionMarginTests`. Siguen esperando la decisión de Rafael. [Fallos textuales](../validation/evidence/2026-11-26-posiciones-saque/playmode-failures-after-joint-fix.txt).
- **Observación:** en `ZServeReceptionMarginTests` la aserción es la misma, pero los valores cambiaron: ahora se esperaba < 316,41 y salió 318,07; en `94071d6` fueron < 343,88 y 345,53. No lo investigué.
- Sin cambios en otras pruebas, umbrales, tolerancias, semillas, preset, hashes ni spec. Es solo de vista: no toca timing, contrato ni física. AutoQA y check-release no se corrieron. `scripts/device.env` se copió con el SHA-256 cotejado y se borró al terminar. No hay PR.

**Actualización 26 nov, 18:30 (camino 2 de SC-Lead: otras miras).** `ServeAimsSearch.WriteAimSearchWithA6` busca el juego de miras más cercano al de `main` que cumple los criterios de `ServeTests` y `ServePositionsTests` sin cambiarlos, con el drive legal en los dos cuadros. También exige al menos un vector de cansancio en el partido exacto de A6, sin cambiar semilla, N ni prueba. Cumplió la primera combinación evaluada: globo derecho (3,6; 3,6) → **(3,55; 3,5)** y globo izquierdo (2,8; 3,45) → **(2,8; 3,4)**. Los dos drives quedan como en `main`. A6 da 96 vectores; como control, las miras de `main` dan 33 y las de `5143310` dan 0. [Búsqueda](../validation/evidence/2026-11-26-posiciones-saque/aims-a6/).

- **RallyParams** `ebf8f717f8c5` → `249979628e7c`; con «Saque nuevo» apagado, `5fc9f2ff92e5` → `45faf866376c`. Sin `HumanServeEarlyMs`, `1063d0f6cf66` → `d39cb1b9f9c9`. La proyección histórica apagada pasa de `992d0570c9de` a `c1c3beb5e744`. Las firmas re-congeladas usan el saque automático y no dependen de las miras: punto 0/0 de `936646e5ba2a…` a `bb6fb1093201…`.
- **Recepción** sobre `94071d6`, con las mismas semillas y N: humano **99,02 % (1917/1936)**, a +0,09 pp del 98,93 % aceptado; rival **100 % (1457/1457)**. Pasa la regla. [Tabla](../validation/evidence/2026-11-26-posiciones-saque/after-aims2-reception.tsv).
- **EditMode** sobre `94071d6`: **1080/1080** (XML SHA-256 `18eba1fd…`).
- **PlayMode** sobre `94071d6`: **total=893 passed=886 failed=7 skipped=0** (XML SHA-256 `7183374d…`). No reintenté. AutoQA y check-release no se corrieron. No hay PR. [Fallos textuales](../validation/evidence/2026-11-26-posiciones-saque/playmode-failures-after-aims2.txt):
  - `ZFirstMatchHintsTests`: `Abandon_AfterH0WasSeenInTheBanner_NeverShowsItAgain`, `Abandon_AndASceneRestart_KeepTheStoredKeysAndRestoreNothingArmed`, `Abandon_WithAnArmedHint_DoesNotBringItBack_UnlessTheTriggerRepeats` y `H0InTheBanner_IfItWasSeenInTheGuide_TheRivalsFirstServePauseShowsNothing`. En los cuatro se esperaba `null` y salió `Volley`: aparece la pista de volea.
  - `ZHandednessTests.LeftHanded_StrokesOnBothSidesStayInTheLeftHand_OnTheSwingViewPath`, aserción «the left hand stays on the grip, across the body too»: se esperaba menos de 0,12 y salió 110,000229.
  - `ZRingClosePulseMatchTests.U1a_RivalServeReceptionWithTheServeVolley_AmberClosesAndTheWhiteIsSteadyAfterItsPulse`, aserción «waiting-white samples past the pulse, seed=1»: se esperaba 3 o más y salió 0.
  - `ZServeReceptionMarginTests.RivalServe_AfterFirstBounce_KeepsAllowanceUntilTheOrder_ThenResetsOnTheNextFlight`, aserción «the tested release is after the bounce but before K-in»: se esperaba menos de 343,8783 y salió 345,5322.
- **Lectura sin verificar:** PlayMode no se corrió en `ae97194`, así que no sé si estos fallos vienen de las posiciones o de las miras. Cinco tocan la recepción del saque del rival: la volea del saque y su pista, el anillo y el margen de 25 ms. Con el restador cerca del bote, la volea del saque se ofrece donde antes no. Ninguno está en la lista de fallos de entorno de AGENTS §3.
- **Decisión pendiente** (Rafael o SC-Lead): estas pruebas dependen de dónde está el restador al recibir el saque. Para seguir hay que cambiar la posición del restador o esas pruebas, y las dos cosas quedan fuera de lo autorizado.


**Actualización 26 nov, 18:15 (`set12b`, opción 1).** Rafael eligió (26 nov 17:44, vía SC-Lead), literal: «Opción 1: congela de nuevo las 3 firmas y ajusta las miras del saque en RallyParams para cumplir los criterios actuales, sin aflojarlos; luego mide de nuevo la recepción».

- **Miras** (`5143310`, `ServeAimsSearch`): globo derecho (3,6; 3,6) → (3,5; 3,55); globo izquierdo (2,8; 3,45) → (2,8; 3,4); drive derecho (2,8; 2,55) → (2,75; 2,55); drive izquierdo (3,6; 2,4) sin cambio. Velocidades sin cambio. RallyParams `ebf8f717f8c5` → `3833bb374542`. Con «Saque nuevo» apagado, `5fc9f2ff92e5` → `263dfbb4bd5d`. Las proyecciones que dependen de las miras se actualizaron: sin `HumanServeEarlyMs`, `1063d0f6cf66` → `1dc6c2b3c360`; la histórica apagada, `992d0570c9de` → `ccea7e5eef72`. [Elección](../validation/evidence/2026-11-26-posiciones-saque/aims/chosen.txt).
- **Firmas re-congeladas:** referencias nuevas en `fatigue-replay/{off,arm0}/points.tsv` de esta evidencia (SHA-256 `e07a01f2…` y `c79eb136…`; antes `2ed055c7…` y `e8459633…`). El punto 0/0 pasa de `936646e5ba2a…` a `bb6fb1093201…`, igual al valor que dio la prueba en rojo.
- **Recepción** (mismas semillas, 100 partidos por estilo): humano **99,28 % (1942/1956)**, a +0,36 pp del 98,93 % aceptado; rival **100 % (1503/1503)**. Pasa la regla de ±1 pp y ≥ 99 %. [Tabla](../validation/evidence/2026-11-26-posiciones-saque/after-aims-reception.tsv).
- **EditMode sobre `5143310`: total=1080 passed=1079 failed=1 skipped=0** (XML SHA-256 `eb1ba80d…5315`). Falla `FatigueArmRallyTests.A6_AllActive_TwoRunsAreIdentical_AndTheArmIsExercised`: «at least one fatigue vector in a whole match», se esperaba más de 0 y dio 0. Con las miras anteriores pasaba (no estaba entre los 6 fallos de `ae97194`). El partido de esa prueba (semillas 20261117/20261118, defaults, saque nuevo) ya no produce ningún vector de cansancio. No reintenté y no toqué la prueba. PlayMode, AutoQA y check-release no se corrieron. No hay PR. [Fallo textual](../validation/evidence/2026-11-26-posiciones-saque/editmode-failures-after-aims.txt).
- **Decisión pendiente** (Rafael o SC-Lead): esa prueba depende de un solo partido con semillas fijas. Se puede aceptar cambiarla (otra semilla o más partidos, sin cambiar el criterio «al menos un vector»), probar otras miras que también cumplan, o cambiar de camino.


**Actualización 26 nov, tarde (`set12b`).** Rafael aceptó el cambio de recepción (26 nov 16:54, vía SC-Lead), literal: «Sí, acepto que la devolución suba al 98,9 % con las nuevas posiciones de saque». Seguí desde el paso 4 sobre `ae97194`, con el worktree limpio y `device.env` copiado sin cambios (SHA-256 `0b0c94db…cec2f`, igual al origen, sin agregarlo a git y borrado al terminar). Tomé el lock de Unity a las 17:10:51.

`scripts/test.sh editmode`: **total=1080 passed=1074 failed=6 skipped=0** (XML SHA-256 `cf6f17bf…9072`). Por la regla, no reintenté. PlayMode, AutoQA y check-release **no se corrieron**. No hay PR. [Fallos textuales](../validation/evidence/2026-11-26-posiciones-saque/editmode-failures.txt).

Las seis pruebas son existentes y fallan por la nueva posición de partida del saque (`BoxPosition`):

- **Firma congelada de RallySim:** se esperaba `936646e5…0527` y salió `bb6fb109…4199`. Fallan `FatigueArmRallyTests.A1_EnabledWithZeroArm_ReplaysTheFrozenF1AdjustedSignatures`, `FatigueIntegrationTests.Off_ReplaysTheFrozenMainReference_BitForBit` y `RivalAiTests.Fondista_WithVolleyAndCoverOff_ReplaysTheFrozenRallySimSignatures`.
- **`ServeTests.Lob_IsHighOnTheFrontWall_TouchesTheSideWall_AndDiesInTheBackCorner`**, aserción «next to the receiver's side wall» (< 0,5): da 0,7135 en Right y 0,5442 en Left.
- **`ServeTests.Drive_IsLowAndFast_WithoutTheSideWall_AndLandsShorterThanTheLob(Right)`**, aserción «two landing zones»: se esperaba < 9,0521 y salió 9,1408.

Para seguir, las posiciones, las pruebas existentes o la referencia congelada tienen que cambiar, y el encargo prohíbe las tres cosas. La decisión es de Rafael o de SC-Lead. Algunas opciones: re-congelar la firma de RallySim y aceptar el bote nuevo del globo y del drive (cambiar pruebas existentes); ajustar las miras `ServeKind*` a la posición nueva (cambia RallyParams); o volver a otra posición del sacador.

## Primera parada (mañana): recepción

26 nov 2026. Encargo `set12-posiciones-saque`, rama `rafaallendes-boop/sc-posiciones-saque` rebasada sobre `origin/main` `253ea44`. [Spec](../superpowers/specs/2026-11-26-posiciones-saque.md). Implementación de Claude (Opus): la revisión de Claude no es independiente. Sin PR, sin fusión y sin instalación.

## Motivo de la parada: la recepción del humano sube más de 5 pp

Regla del brief, fijada antes de medir: si la tasa cambia más de 5 pp en cualquiera de los dos, se detiene el trabajo. Aceptar esa dificultad le corresponde a Rafael.

Se jugaron 100 partidos por estilo (Fondista, Definidor y Completo) en nivel Medio, con las semillas fijas `20261126+100000·estilo`, el preset y RallyParams por defecto. El humano es el novato de σ 90 ms y saca globo. Tasa = primer golpe del restador al saque en `Contact`.

| Restador | ANTES `32f400e` (= `main` + experimento) | DESPUÉS `8245996` | Cambio |
|---|---|---|---|
| Humano (recibe al rival) | 91,60 % (1722/1880) | **98,93 % (1936/1957)** | **+7,33 pp** |
| Rival (recibe al humano) | 99,93 % (1379/1380) | 100,00 % (1538/1538) | +0,07 pp |

Por estilo, el humano pasa de 92,72/88,96/93,04 % a 99,40/98,74/98,62 % ([antes](../validation/evidence/2026-11-26-posiciones-saque/before-reception.tsv), [después](../validation/evidence/2026-11-26-posiciones-saque/after-reception.tsv)). Casi toda la diferencia está en los golpes «al aire»: bajan de 156 a 7. Los tardíos suben de 2 a 14. El restador ahora parte cerca de donde bota el saque y no desde la T. Con la base anterior `8a32d42`, la medición ANTES dio las mismas cifras exactas, así que el árbitro de #121 no cambia la simulación.

## Lo hecho (camino 3 de SC-PM)

- **Sacador** a 0,70 m de la línea interior del cuadro y 0,30 m detrás de la línea corta: (0,90; 5,79) en el cuadro derecho y (5,50; 5,79) en el izquierdo. `ServePositionsSearch` lo eligió en una rejilla de 5 cm como la posición más cercana a la esquina (a 0,40 m de ella) en la que, en los dos cuadros, drive, globo y Auto son legales y botan al menos 0,3 m dentro de lo exigido. Lo limita el drive del cuadro izquierdo (0,39 m). [Rejilla](../validation/evidence/2026-11-26-posiciones-saque/search-grid.tsv). RallyParams, su hash, defaults y SimulationKey no cambian. `Find` y `Typed` ganan una sobrecarga con el origen explícito, sin cambio de comportamiento.
- **Restador** en (4,80; 7,65) y (1,60; 7,65), como posición real del cuerpo en `RallyDirector.BeginPoint` y `RallySim.PlayPoint`.
- **Primera parada (esquina a 0,3 m):** el drive era falta en los dos cuadros ([contactos](../validation/evidence/2026-11-26-posiciones-saque/stop1-serve-contacts.tsv)).
- **TDD** ([detalle](../validation/evidence/2026-11-26-posiciones-saque/tdd.tsv)): primer RED con 12 fallos por aserción. RED de la posición elegida, con el valor anterior: 6 fallos, 2 por aserción y 4 por la excepción de drive ilegal. GREEN con 20/20 en EditMode (`ServePositionsTests`) y 2/2 en PlayMode (`ZServePositionsTests`). El saque no atraviesa al restador: el paso más cercano es 0,67 m (Auto), y la cápsula mide 0,30 m ([geometría](../validation/evidence/2026-11-26-posiciones-saque/after-geometry.tsv)).
- **No corrido** por la parada: la suite completa de EditMode y PlayMode (incluido `TimingAidTests`), AutoQA y check-release.

## Decisión que necesito de Rafael

1. Aceptar una recepción del humano de 98,93 % en el saque (+7,33 pp) y seguir con las suites y el PR.
2. Otra posición del restador (más lejos del bote, por ejemplo) y volver a medir.
3. Dejar el restador en la T y mover solo al sacador.
