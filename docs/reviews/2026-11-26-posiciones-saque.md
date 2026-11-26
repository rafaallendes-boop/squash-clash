# Posiciones de saque · DETENIDO por pruebas existentes en EditMode

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
