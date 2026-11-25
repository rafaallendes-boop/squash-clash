# Golpe del partido en la tarjeta de resultado

25 nov 2026. Rama `rafaallendes-boop/sc-golpe-partido` sobre `origin/main` `1dc9dcb`. Spec en su revisión 2: [2026-11-25-golpe-del-partido.md](../superpowers/specs/2026-11-25-golpe-del-partido.md). La arquitectura es de Claude (Opus). Implementó un subagente Sonnet dirigido por Claude, así que esta revisión **no es independiente**. Falta la revisión cruzada de Codex.

## Qué cambia

- La tarjeta muestra una línea, «Golpe del partido: N …», con el primer dato del jugador que llega a 3. El orden es Perfectos, después dos paredes ganadores y después voleas. Si ninguno llega a 3, no sale nada.
- `StrokeOfTheMatch` es una clase pura en Core. `MatchSummary` suma los campos `Perfects`, `BoastWinners` y `Volleys`, con parámetros opcionales, así que las llamadas existentes no cambian. `RallyHud` cuenta con `ShotResolved` y `PointEnded`.
- **Tarjeta compacta** (revisión 2, por decisión de SC-PM y SC-Lead con consentimiento del PO). Solo cambian el espaciado y los tamaños de `.sc-result-*`:
  - ancho de 300 a 340 px, con `max-width: 92%`;
  - padding de 22/18 a 16/12;
  - título de 28 a 24 px;
  - marcador de 60 a 52 px;
  - márgenes de las líneas de detalle de 4 a 2 px;
  - margen inferior de la nota de 16 a 8 px;
  - márgenes de los botones de 4 a 2 px, con los botones todavía de 46 px de alto.

  No cambian los textos, las fuentes, los colores ni los umbrales. Con el contenido más ancho, los tres botones DEV entran en una fila.
- No cambian el juego, el preset, `RallyParams` ni los hashes de AGENTS §5.

## Puertas, sobre `5a326ec`

El head final solo agrega este informe.

| Puerta | Resultado |
|---|---|
| EditMode completo | 1045/1045 |
| PlayMode completo | 881/881 |
| AutoQA `mvp` V / T3 / volea | PASS / 0 / nivel 1, 102/102 cada una, árbol limpio |
| check-release | OK (dev y prod, build 1217) |

- **TDD**:
  - EditMode `StrokeOfTheMatchTests` con stubs: 7 de 13 en RED por aserción, y GREEN 13/13.
  - PlayMode de las clases nuevas con stubs: 8 de 10 en RED.
  - Layout antes de compactar: 5 de 18 en RED, y GREEN 18/18 después.
- No se editó ningún test existente.

## Layout: holgura bajo el área segura, peor caso

El peor caso es la línea de récord más larga (98 caracteres, tres récords) junto con «Golpe del partido: 99 dos paredes ganadores». La medición se hizo en la configuración DEV, la misma de `ZDeviceLayoutTests`.

| Perfil | Antes | Después |
|---|---|---|
| SE 375×667 | −62,8 px (se salía; ya se salía con el récord solo, −12,6) | +76,5 px |
| Mini 375×812 | −38,1 | +81,8 |
| Pro 402×874 | −28,7 | +85,2 |
| Pro Max 430×932 | −42,0 | +87,0 |

La línea nueva ocupa como máximo dos líneas. La tarjeta no tapa el encabezado y deja al menos 8 px a cada lado.

## Observaciones

- **Dos paredes ganador: 0 de 35 en escena.** Se probó en nivel Fácil, con estilo Fondista y varias semillas, y el rival devolvió todos los dos paredes. Por decisión de producto, la prioridad se mantiene.
  - El test de escena exige un dos paredes resuelto y verifica la regla en cada punto.
  - La rama «ganó» queda probada en EditMode.
- **Perfectos.** En el partido con piloto automático (semilla 9) hubo 116 Perfectos, 0 dos paredes ganadores y 0 voleas. Con esta prioridad, la línea casi siempre dirá «N Perfectos».
- **Desviación menor.** `StopMode` conserva `rally.Match`, así que el conteo se reinicia cuando cambia `rally.Active ? rally.Match : null`. Lo cubre el test `LeavingTheMode_ClearsTheTally`.
