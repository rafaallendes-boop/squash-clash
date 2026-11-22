# Front 3 — Tarjeta de resultado con vocabulario de squash

22 nov 2026 · Base `origin/main` `518f9c3`. Spec de Claude (arquitecto, modo product lead); revisión no independiente. Agrega una línea a la tarjeta que ya existe, por ejemplo «5 Perfectos, 3 voleas, 1 dos paredes». No agrega pasos antes ni después del partido.

## 0. Confirmación previa: ¿existen los conteos por partido en las métricas locales?

**Resultado: no existen. Están los ingredientes por golpe, pero falta el conteo por partido y falta marcar el dos paredes aplicado.**

Qué hay hoy:

- La línea de partido (`match-end`, `Core/Diagnostics/UserMetrics.cs:11,52-56`) guarda solo `matchId`, la dificultad, la mano, el resultado, los puntos, la duración y si se abandonó. No guarda ningún conteo de golpes.
- La tarjeta (`Runtime/Rally/RallyHud.cs:280-287`) muestra el último punto, la duración, el rally más largo y los récords. Todo lo calcula la vista (`TrackMatch`, `:234-239`).

Qué hay por golpe, en el log:

- **Perfecto:** línea `shot` con `Result:"Perfect"`. Esa línea no distingue al jugador del rival: con el cansancio encendido, el rival también escribe `shot` (`RallyDirector.cs:586-592`, `Reason:"rival-contact"`).
- **Volea:** solo en `shot-opportunity-order` (`ShotController.cs:477`, campo `opportunity`), separada del resultado del golpe.
- **Dos paredes:** solo en `shot-boast` (`ShotController.cs:318-329`, con `solved` y `fellBack`). Se escribe en cada resolución, incluidas las no autoritativas, así que no equivale a «un dos paredes jugado».
- Ninguna línea de golpe lleva `matchId`. `ShotRecord` (`Core/Shot/ShotRecord.cs:44`) no tiene campos de volea ni de dos paredes, y su esquema no se toca.

Qué falta:

1. Un contador de partido con los tres conteos, en memoria, que sirva a la tarjeta.
2. Una marca, legible al resolver el golpe, de que el golpe aplicado del jugador fue un dos paredes y no cayó al drive de respaldo.
3. Que `match-end` guarde los tres conteos, para que existan en las métricas locales.

## 1. Alcance

- **`MatchStrokeTally`, clase pura en `Core/Diagnostics`.** Guarda `Perfects`, `Volleys` y `Boasts`, y tiene `Reset()`, `Add(...)` y `Line()`, que devuelve el texto de la tarjeta.
- **Qué cuenta.** Solo los golpes del jugador resueltos por `ShotController.ShotResolved` (`ShotController.cs:104,386,403`) con un partido activo. Los golpes del rival no pasan por ese evento y nunca cuentan. Con el piloto automático se cuenta igual: la tarjeta lo muestra y los récords ya excluyen esos partidos.
  - **Perfecto:** `Result=="Perfect"`, sea volea, dos paredes o golpe de bote.
  - **Volea:** `Result` es `Perfect` o `Valid`, y `controller.LastOpportunity==Opportunity.Volley` en ese instante (`:60,266`).
  - **Dos paredes:** `Result` es `Perfect` o `Valid`, y la marca nueva `ShotController.LastStrokeBoast` es verdadera. Vale `true` cuando la resolución autoritativa del golpe aplicado usó `SolveBoast` y `!LastCausal.FellBack` (`:311-321`). Se limpia en cada lanzamiento (`:125`). Es de solo lectura: no cambia la solución ni las métricas existentes.
  - Un golpe fallido, «No llega» o «Sin golpe» no cuenta en nada. Un Perfecto de volea cuenta como Perfecto y como volea.
- **Reinicio.** Al empezar un partido, en la revancha y al salir, con la misma regla que `TrackMatch` usa para el rally más largo.
- **Tarjeta.** Una etiqueta nueva, `match-result-strokes`, justo debajo del marcador:
  - Orden fijo: Perfectos, voleas, dos paredes. Los conteos en 0 no se muestran; si los tres son 0, la línea se oculta.
  - Plurales: «1 Perfecto» y «5 Perfectos»; «1 volea» y «3 voleas»; «dos paredes» no cambia en plural. Separador: coma y espacio, como pidió el PO.
  - Ejemplos: «5 Perfectos, 3 voleas, 1 dos paredes»; «1 Perfecto, 1 volea»; «2 dos paredes».
  - Sigue al interruptor DEV `RallyHud.ExtendedCardOn`: apagado, la tarjeta queda como antes.
- **Métricas.** `MatchLine` suma tres enteros: `perfects`, `volleys` y `boasts`. `UserMetrics` recibe un `UpdateMatchStrokes(p,v,b)` análogo a `UpdateMatchScore`, que se llama donde ya se pasa el marcador (`AppBootstrap.cs:366,372`). Un partido abandonado guarda los conteos que llevaba hasta ese momento. El cambio es aditivo: los campos actuales quedan iguales.

## 2. Fuera de alcance

- No hay conteos del rival, porcentajes ni otros golpes (drop, globo, saque).
- Los récords personales con estos conteos y el historial entre partidos quedan fuera.
- No cambian el esquema de `ShotRecord` ni las líneas `shot`, `shot-boast` y `shot-opportunity-order`.
- No cambian el juego, el preset, `RallyParams` ni los hashes de §5. No se agrega un nuevo interruptor DEV.

## 3. Criterio de aceptación

1. Después de un partido jugado en PlayMode (con piloto automático y semilla fija), la línea de la tarjeta coincide exactamente con `Line()` del contador. El contador coincide con un recuento independiente del test sobre los `ShotRecord` del jugador emitidos por `ShotResolved`.
2. En una escena de volea v2 elegida y aplicada, el contador suma 1 volea. Un golpe al bote no suma volea.
3. En una escena de dos paredes resuelta, el contador suma 1 dos paredes. Si cae al drive de respaldo (`FellBack`), no suma. Un golpe al frontis tampoco suma.
4. Con los tres conteos en 0, la línea queda oculta. Con `ExtendedCardOn` en `false`, también. La revancha reinicia los conteos.
5. `match-end` lleva `perfects`, `volleys` y `boasts` con los mismos valores que muestra la tarjeta. Si el partido se abandona, lleva los parciales. Si nunca se actualizó, lleva 0.
6. En el perfil de dispositivo más angosto de `ZDeviceLayoutTests`, la línea más larga que admite el test («12 Perfectos, 10 voleas, 10 dos paredes») cabe en la tarjeta en dos líneas como máximo, sin elipsis. La tarjeta no sale del área segura.
7. EditMode y PlayMode completos en verde en el Mac del PO, y `check-release` OK. AutoQA (`scripts/autoqa.sh mvp`, PASS/0/1) es obligatorio porque se agrega una lectura en la ruta de resolución del golpe. Hay que mostrar que no cambia ninguna decisión.

## 4. Tests RED primero (Codex, TDD)

- **EditMode, `Diagnostics/MatchStrokeTallyTests`:** el formato de `Line()` en todos los casos de §1 (ceros, singulares, plurales, «dos paredes» invariable, orden) y `Reset()`.
- **EditMode, `Diagnostics/UserMetricsTests`:**
  - `match-end` con `perfects`, `volleys` y `boasts` en un partido terminado y en uno abandonado, y con 0 si no se actualizó;
  - los campos anteriores quedan idénticos.
- **PlayMode, `ZMatchCardTests`:**
  - la etiqueta `match-result-strokes` coincide con el recuento independiente (criterio 1);
  - se oculta con todo en 0 y con `ExtendedCardOn` en `false`;
  - la revancha la reinicia.
- **PlayMode, `ZVolleyV2Tests`:** volea elegida y aplicada suma 1; un golpe al bote suma 0.
- **PlayMode, `ZBoastTests`:**
  - `LastStrokeBoast` es verdadero en un dos paredes resuelto y falso con `FellBack`;
  - se limpia en el lanzamiento siguiente;
  - el contador sigue esos mismos valores.
- **PlayMode, `ZDeviceLayoutTests`:** el criterio 6.

Orden recomendado:

1. El contador puro y las métricas en EditMode.
2. La marca `LastStrokeBoast`.
3. La tarjeta.
4. El diseño en el perfil angosto.

## 5. Sin iPhone y con iPhone

- **Sin iPhone, en el Editor:** todo §3, incluido el diseño en los perfiles de dispositivo de `ZDeviceLayoutTests`.
- **Con iPhone, en la próxima instalación autorizada:**
  - que la línea se lea de un vistazo en la tarjeta real;
  - que los conteos de un partido jugado a mano coincidan con lo que el PO recuerda haber hecho;
  - que `match-end` del teléfono, al copiar las métricas, traiga los tres campos.

## 6. Dirección visual

Pasada de `/frontend-design`.

- **Plan.** La tarjeta ya es un trozo de frontis: yeso `--plaster-card`, tinta `--ink`, línea roja arriba y Rubik. No se agregan colores ni tipografías. La línea nueva va justo bajo el marcador gigante (60 px), porque responde a «cómo jugué» antes del detalle «último punto».
- **Tokens.** Clase `.sc-result-strokes`: Rubik Bold 15 px (entre los 14 de las líneas de detalle y los 60 del marcador; en la escala existente), color `--ink`, centrada, `white-space: normal`, `margin-top: 2px` y `margin-bottom: 8px`.

```
  ┌───────── línea roja ─────────┐
  │           VICTORIA            │
  │     ¡Ganaste el partido!      │
  │            7 – 4              │
  │ 5 Perfectos, 3 voleas, 1 dos paredes │  ← nueva, Rubik Bold 15, tinta
  │   Último punto: Rival: tin    │
  │   Duración: 6:12              │
  │   ...                         │
```

- **Revisión contra lo genérico.** El primer impulso fue una fila de tres «stat tiles» con número grande y etiqueta chica, que es el recurso por defecto de cualquier tarjeta de resultados. Se descartó: rompe la regla de «una línea más», compite con el marcador y suena a panel. El acento está en las palabras de squash, que son el pedido. Por eso no se pinta de amarillo «Perfectos»: el amarillo `--dot` no tiene contraste suficiente sobre yeso, y está reservado para la pelota en la cancha.
- **Movimiento.** Ninguno propio. La línea entra con la tarjeta (0,28 s, `ResultInSeconds`).
