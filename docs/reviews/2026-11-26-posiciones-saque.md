# Posiciones de saque · DETENIDO por pruebas existentes en EditMode

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
