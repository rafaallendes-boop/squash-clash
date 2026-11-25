# Golpe del partido: detenido por layout y por el caso «dos paredes ganador»

25 nov 2026. Rama `rafaallendes-boop/sc-golpe-partido` sobre `origin/main` `1dc9dcb`. Spec: [2026-11-25-golpe-del-partido.md](../superpowers/specs/2026-11-25-golpe-del-partido.md). La arquitectura es de Claude (Opus). Implementó un subagente Sonnet dirigido por Claude, así que esta revisión **no es independiente**. Falta la revisión cruzada de Codex. **No se abrió PR:** se registra y se detiene (AGENTS §2).

## Qué hay en la rama

- `35b3fdd` spec; `5b12bc2` tests contra stubs (RED); `99606f8` implementación.
- `StrokeOfTheMatch` es una clase pura en Core. `MatchSummary` suma `Perfects`, `BoastWinners` y `Volleys`, con parámetros opcionales, así que las llamadas existentes no cambian.
- `RallyHud` cuenta con `ShotResolved` y `PointEnded`. La etiqueta nueva es `match-result-highlight`, con la clase `.sc-result-highlight`.
- No cambian el juego, el preset, `RallyParams` ni los hashes de §5.

## Resultados

| Corrida | Resultado |
|---|---|
| RED EditMode `StrokeOfTheMatchTests` (stubs) | 13 tests: 6 pasan y 7 fallan por aserción |
| RED PlayMode, las dos clases nuevas (stubs) | 10 tests: 2 pasan y 8 fallan |
| GREEN EditMode `StrokeOfTheMatchTests` | 13/13 |
| EditMode completo | 1045/1045 |
| PlayMode `ZRecordsTests`, `ZMatchCardTests`, `ZDeviceLayoutTests`, `ZStrokeBoastTests` y `ZFirstMatchHintsTests` | 74/74, sin editar ningún test |
| PlayMode, las dos clases nuevas (corrida final de Claude) | **18 tests: 12 pasan y 6 fallan** |
| PlayMode completo, AutoQA, check-release | **no se corrieron** (detenido) |

## Hallazgo 1: con récord, la tarjeta sale del área segura

La línea sola cabe en todos los perfiles: máximo dos líneas, dentro de la tarjeta, y `AssertFits` pasa. Con la línea de récord visible, el botón inferior baja del área segura. La medición se hizo en la configuración DEV, la misma de `ZDeviceLayoutTests`, que tiene tres botones en dos filas.

En px por debajo del área segura (un número negativo significa que cabe), con la línea de récord de dos elementos visible:

| Perfil | Sin la línea nueva | Con una línea (Perfectos / voleas) | Con dos líneas (dos paredes) |
|---|---|---|---|
| SE 375×667 | **12,6** | 45,7 | 62,8 |
| Mini 375×812 | −25,6 | 5,2 | 21,5 |
| Pro 402×874 | −29,0 | −4,2 | 12,3 |
| Pro Max 430×932 | −22,7 | 9,1 | 25,7 |

- **El SE ya se salía sin la línea nueva.** Es un defecto previo que ningún test cubre, porque los partidos con piloto automático nunca escriben récords.
- La primera victoria rompe hasta tres récords a la vez, y esa línea es más larga que la medida aquí.
- Con la línea nueva, el caso más común (una línea de Perfectos con récord) también se sale en el Mini y el Pro Max.
- **Decisión pendiente (SC-Lead o PO).** Hay tres opciones:
  - (a) Compactar la tarjeta en pantallas bajas, por ejemplo con menos espacio vertical o con «Duración» y «Rally más largo» en una sola línea. Esto cambia la tarjeta existente y corrige también el SE previo. Es la recomendación de Claude.
  - (b) Mostrar el golpe del partido solo cuando no hay récord.
  - (c) Aceptar el desborde.

## Hallazgo 2: en escena, un dos paredes nunca ganó el punto

- **El caso «ganó» no se vio nunca.** En 36 intentos (nivel Fácil, estilo Fondista, varias semillas y puntos de la lateral) hubo 35 dos paredes resueltos y 0 puntos ganados, porque el rival los devolvió todos. Lo que sí se vio se cumple: un punto perdido suma 0. La rama «ganó» queda probada solo en EditMode, así que el criterio §5.3 de la spec queda a medias en PlayMode.
- **Observación de producto.** En el partido con piloto automático (semilla 9) hubo 116 Perfectos, 0 dos paredes ganadores y 0 voleas. Lo mismo dio la revancha. Con la prioridad pedida, la línea casi siempre dirá «N Perfectos», y los casos 2 y 3 se verán poco. No se cambió la regla. Queda para SC-Lead.

## Desviación menor de la spec

`StopMode` conserva `rally.Match`. El reinicio, entonces, se ata a `rally.Active ? rally.Match : null` y no a que `Match` pase a `null`. Lo cubre el test `LeavingTheMode_ClearsTheTally`.

## Textos (ancho en una línea medido con `MeasureTextSize`; contenido de la tarjeta ≈ 256 px)

| Texto | Caracteres | SE | Pro Max | Líneas |
|---|---|---|---|---|
| «Golpe del partido: 4 Perfectos» | 30 | 214 px | 208 px | 1 |
| «Golpe del partido: 5 voleas» | 27 | 189 px | 184 px | 1 |
| «Golpe del partido: 3 dos paredes ganadores» | 42 | 308 px | 300 px | 2 |
| «Golpe del partido: 99 dos paredes ganadores» | 43 | 317 px | 309 px | 2 |
