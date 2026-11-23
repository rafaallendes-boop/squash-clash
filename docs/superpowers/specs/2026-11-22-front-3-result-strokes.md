# Front 3 — Tarjeta de resultado con vocabulario de squash

22 nov 2026, revisión 2 del 23 nov. Base `origin/main` `230b758`, con #94, #96, #97 y #100. Spec de Claude como arquitecto. Roles, fusión e instalación según AGENTS §2, §7 y §8 de esa base.

Agrega una línea a la tarjeta que ya existe, por ejemplo «5 Perfectos, 3 voleas, 1 dos paredes». No agrega pasos antes ni después del partido.

**Plan de implementación.** Una rama y una PR propias, en dos tramos y en este orden:

1. **Conteos y métricas.** Lo implementa Codex y lo revisa Claude. Incluye:
   - `MatchStrokeTally`;
   - la marca `LastStrokeBoast`, que lee la ruta de resolución del golpe;
   - la sincronización con `UserMetrics` y los campos de `match-end`;
   - los criterios 1 a 3, 5 y 6.
2. **Visual.** Lo implementa Claude con `/frontend-design` y Sonnet, y lo revisa Codex. Incluye la línea de la tarjeta, el estilo y el layout, con los criterios 4 y 7.

**Fusión e instalación.**

- Se fusiona con la revisión aprobada, las puertas en verde y las reglas de fusión de AGENTS §2. SC-PM fusiona con la autonomía de §2, sin pedir un «sí» por fusión.
- La comprobación en el iPhone de §5 va en la ventana que SC-PM coordina con Rafael (AGENTS §8). Esa ventana es la autorización de esa instalación.

## 0. Confirmación previa: ¿existen los conteos por partido en las métricas locales?

**No.** Están los ingredientes por golpe, pero falta el conteo por partido y falta marcar el dos paredes aplicado.

Qué hay hoy:

- `match-end` (`Core/Diagnostics/UserMetrics.cs:11,51-56`) guarda solo esto:
  - `matchId`, la dificultad y la mano;
  - el resultado y los puntos;
  - la duración y si se abandonó.

  No guarda ningún conteo de golpes.
- **Dónde se cierra el partido** (`AppBootstrap.cs:358-377`):
  - `OnUserPointEnded` actualiza el marcador y emite `match-end` al terminar el partido;
  - `OnUserModeChanged` lo emite como abandonado al salir del modo;
  - `OnDestroy` llama directamente a `UserEvents.EndSession()`, que también lo cierra como abandonado.
- **La tarjeta** (`Runtime/Rally/RallyHud.cs:110-117,374-396`) muestra el último punto, la duración, el rally más largo y los récords. Esos datos los calcula la vista (`TrackMatch`, `:340-345`).

Qué hay por golpe, en el log:

- **Perfecto:** la línea `shot` con `Result:"Perfect"`. No distingue al jugador del rival: con el cansancio encendido, el rival también escribe `shot` (`RallyDirector.cs:628`, `Reason:"rival-contact"`).
- **Volea:** solo en `shot-opportunity-order` (`ShotController.cs:479`, campo `opportunity`), aparte del resultado del golpe.
- **Dos paredes:** solo en `shot-boast` (`ShotController.cs:318-329`, con `solved` y `fellBack`). Se escribe en cada resolución, también en las no autoritativas, así que no equivale a «un dos paredes jugado». `ShotController.LastBoast` (`:85`) tiene el mismo problema y no se limpia entre golpes.
- **Ninguna línea de golpe lleva `matchId`.** `ShotRecord` (`Core/Shot/ShotRecord.cs:44`) no tiene campos de volea ni de dos paredes, y su esquema no se toca.

**Qué no cuenta**, en `main`:

- el intento fallido del rival de #97, que es `RivalSwing(false)` y no un `ShotResolved` del jugador;
- el saque del jugador de #100, que pasa por `ServeAim` y no genera `ShotRecord`.

**Qué falta:**

1. Un contador de partido con los tres conteos.
2. Una marca, legible al resolver el golpe, de que el golpe aplicado del jugador fue un dos paredes y no cayó al drive de respaldo.
3. Que `match-end` guarde los tres conteos, también en un partido abandonado o cerrado a mitad de un punto.

## 1. Alcance

- **`MatchStrokeTally`**, clase pura en `Core/Diagnostics`, con `Perfects`, `Volleys` y `Boasts`, y los métodos `Reset()`, `Add(...)` y `Line()`, que devuelve el texto de la tarjeta.
- **Una sola instancia.** Es de `AppBootstrap`, que ya maneja el ciclo del partido para `UserEvents`, y se expone como `AppBootstrap.MatchStrokes`. La tarjeta lee esa misma instancia.
- **Qué cuenta.** Solo los golpes del jugador resueltos por `ShotController.ShotResolved` (`ShotController.cs:104,386,403`) mientras hay un partido abierto en `UserEvents` (`userMatch != null`).
  - Los golpes del rival no pasan por ese evento y nunca cuentan.
  - Con el piloto automático se cuenta igual: la tarjeta lo muestra, y los récords ya excluyen esos partidos.
  - **Perfecto:** `Result=="Perfect"`, sea volea, dos paredes o golpe de bote.
  - **Volea:** `Result` es `Perfect` o `Valid`, y `controller.LastOpportunity==Opportunity.Volley`, leído dentro del callback (`:60,266`).
  - **Dos paredes:** `Result` es `Perfect` o `Valid`, y `controller.LastStrokeBoast` es verdadero, leído dentro del callback.
  - Un golpe fallido, «No llega» o «Sin golpe» no cuenta en nada. Un Perfecto de volea cuenta como Perfecto y como volea.
- **Contrato de `ShotController.LastStrokeBoast`.** Es el mismo que el de la spec 2 (PR #93, §1.6); la introduce la primera de las dos PRs que se fusione.
  - Es un `bool` que describe el golpe del `ShotRecord` que está saliendo por `ShotResolved`. Ese callback es lo que identifica el golpe.
  - Vale verdadero solo si la orden apuntaba a la lateral (`order.Side.Exists`), la resolución **autoritativa** usó `SolveBoast` con `boast.Solved` y `!LastCausal.FellBack` (`:318-329`), y el golpe quedó aplicado.
  - Se limpia en cada lanzamiento (`:125`).
  - Es de solo lectura: no cambia la solución ni las métricas existentes, y no se deduce de `LastBoast`.
- **Sincronización con las métricas: el acumulado nunca queda atrás.**
  - `UserMetrics` recibe un `UpdateMatchStrokes(perfects, volleys, boasts)`, análogo a `UpdateMatchScore`.
  - `AppBootstrap` lo llama **en cada golpe contado**, justo después de `MatchStrokeTally.Add`. Así `UserMetrics` siempre tiene el acumulado actual, aunque el partido se cierre a mitad de un punto.
  - Antes de cada `EndMatch` se vuelve a pasar el acumulado. Pasa en `OnUserPointEnded`, en `OnUserModeChanged` y en `OnDestroy`, antes de `EndSession`.
  - **El orden es fijo: (1) instantánea, `UpdateMatchStrokes` con los valores de la instancia; (2) emisión de `match-end`; (3) reinicio.**
  - El reinicio solo ocurre en `OnUserRallyStarted`, al abrirse un partido nuevo (`BeginMatch`). Eso incluye la revancha, que crea un `MatchScore` nuevo (`RallyDirector.cs:222`). Nunca se reinicia antes de emitir.
- **Tarjeta.** Una etiqueta nueva, `match-result-strokes`, justo debajo del marcador:
  - Orden fijo: Perfectos, voleas, dos paredes. Los conteos en 0 no se muestran; si los tres son 0, la línea se oculta.
  - Plurales: «1 Perfecto» y «5 Perfectos»; «1 volea» y «3 voleas»; «dos paredes» no cambia en plural.
  - Separador: coma y espacio, como pidió el PO.
  - Ejemplos: «5 Perfectos, 3 voleas, 1 dos paredes»; «1 Perfecto, 1 volea»; «2 dos paredes».
  - Sigue al interruptor DEV `RallyHud.ExtendedCardOn`: apagado, la tarjeta queda como antes.
- **Métricas.** `MatchLine` suma tres enteros: `perfects`, `volleys` y `boasts`.
  - El cambio es aditivo: los campos actuales quedan iguales.
  - Un partido abandonado o cerrado con la sesión guarda lo que llevaba hasta el último golpe contado.

## 2. Fuera de alcance

- Conteos del rival, porcentajes u otros golpes (drop, globo, saque).
- Récords personales con estos conteos e historial entre partidos.
- Cambiar el esquema de `ShotRecord` o las líneas `shot`, `shot-boast` y `shot-opportunity-order`.
- Cambiar el juego, el preset o `RallyParams`. Los hashes vigentes de AGENTS §5 se conservan; `RallyParams` ya es `1063d0f6cf66` por #100, y no se restauran los de la base anterior.
- Agregar un interruptor DEV nuevo.

## 3. Criterio de aceptación

1. **Recuento independiente.** Después de un partido jugado en PlayMode con piloto automático y semilla fija:
   - la línea de la tarjeta coincide exactamente con `Line()` del contador;
   - el contador coincide con el recuento de un **observador propio del test**.
     - El observador se suscribe a `ShotResolved`.
     - Dentro de su callback toma el `ShotRecord` y, en ese mismo instante, `controller.LastOpportunity` y `controller.LastStrokeBoast`, y lleva sus propios conteos con las reglas de §1.
     - No lee `MatchStrokeTally` ni la tarjeta, y no amplía `ShotRecord`.
     - Los criterios 2 y 3 completan este con escenas de resultado conocido.
2. **Volea.** En una escena de volea v2 elegida y aplicada, el contador suma 1 volea. Un golpe al bote no suma volea.
3. **Dos paredes.** En una escena de dos paredes resuelta, el contador suma 1 dos paredes. Si cae al drive de respaldo (`FellBack`), no suma. Un golpe al frontis tampoco.
4. **Ocultar y reiniciar.** Con los tres conteos en 0, la línea queda oculta. Con `ExtendedCardOn` en `false`, también. La revancha reinicia los conteos.
5. **`match-end`** lleva `perfects`, `volleys` y `boasts` con los mismos valores que muestra la tarjeta. Si el partido nunca se actualizó, lleva 0.
6. **Parciales al abandonar o cerrar la sesión.** Es una prueba de integración, aparte del test puro de `UserMetrics`.
   - Se juega hasta que haya al menos un golpe contado **después del último fin de punto**. En ese instante:
     - (a) volver al menú desde la pausa (`StopMode`);
     - (b) en otra corrida, cerrar la sesión por la ruta de `AppBootstrap.OnDestroy`.
   - En ambos casos `match-end`, con `abandoned=true`, incluye ese último golpe y coincide con el observador del criterio 1.
7. **Layout.** En el perfil más angosto de `ZDeviceLayoutTests`, la línea más larga que admite el test («12 Perfectos, 10 voleas, 10 dos paredes») cabe en la tarjeta en dos líneas como máximo, sin elipsis. La tarjeta no sale del área segura.
8. **Puertas.**
   - EditMode y PlayMode completos en verde en el Mac del PO.
   - AutoQA `scripts/autoqa.sh mvp` PASS/0/nivel 1, obligatorio porque se agrega una lectura en la ruta de resolución del golpe. Hay que mostrar que no cambia ninguna decisión.
   - `check-release` OK.

## 4. Tests RED primero (TDD) y reglas de prueba

**Tests nuevos.**

- **EditMode, `Diagnostics/MatchStrokeTallyTests`.** El formato de `Line()` en todos los casos de §1 (ceros, singulares, plurales, «dos paredes» invariable y orden) y `Reset()`.
- **EditMode, `Diagnostics/UserMetricsTests`.**
  - `match-end` lleva `perfects`, `volleys` y `boasts` en un partido terminado, en uno abandonado y con 0 si nunca se actualizó.
  - Con `UpdateMatchStrokes` después del último `UpdateMatchScore`, `EndSession` emite esos valores.
  - Los campos anteriores quedan idénticos.
- **PlayMode, `ZMatchCardTests`.**
  - La etiqueta `match-result-strokes` coincide con el observador independiente (criterio 1).
  - Se oculta con todo en 0 y con `ExtendedCardOn` en `false`.
  - La revancha la reinicia.
  - El criterio 6, en sus variantes (a) y (b).
- **PlayMode, `ZVolleyV2Tests`.** Una volea elegida y aplicada suma 1; un golpe al bote suma 0.
- **PlayMode, `ZBoastTests`.**
  - `LastStrokeBoast` es verdadero en un dos paredes resuelto y falso con `FellBack`.
  - Se limpia en el lanzamiento siguiente.
  - El contador sigue esos mismos valores.
- **PlayMode, `ZDeviceLayoutTests`.** El criterio 7.

**Reglas de prueba.**

- Los tests existentes no se editan y tienen que pasar con la línea nueva.
- Si alguno falla, es un defecto. Se registra y se detiene, según AGENTS §2. No se apaga `ExtendedCardOn` ni se cambia una aserción para pasar (AGENTS §3).

**Orden recomendado:**

1. El contador puro y las métricas en EditMode.
2. La marca `LastStrokeBoast` y la sincronización en `AppBootstrap`.
3. La tarjeta.
4. El diseño en el perfil angosto.

## 5. Sin iPhone y con iPhone

**Se implementa en su propia PR.**

**Sin iPhone, en el Editor:** todo §3, incluido el diseño en los perfiles de dispositivo de `ZDeviceLayoutTests` y los parciales al cerrar la sesión.

**Con iPhone, en la ventana que SC-PM coordina con Rafael (AGENTS §8).** Dentro de esa ventana se hacen el respaldo y el cotejo de presets y métricas.

- La línea se lee de un vistazo en la tarjeta real.
- Los conteos de un partido jugado a mano coinciden con lo que el PO recuerda haber hecho.
- `match-end` del teléfono, al copiar las métricas, trae los tres campos, también en un partido abandonado.

## 6. Dirección visual

Pasada de `/frontend-design`.

- **Plan.** La tarjeta ya es un trozo de frontis: yeso `--plaster-card`, tinta `--ink`, línea roja arriba y Rubik. No se agregan colores ni tipografías. La línea nueva va justo bajo el marcador gigante (60 px), porque responde a «cómo jugué» antes del detalle «último punto».
- **Tokens.** Clase `.sc-result-strokes`:
  - Rubik Bold 15 px, entre los 14 de las líneas de detalle y los 60 del marcador, dentro de la escala existente;
  - color `--ink`, centrada y con `white-space: normal`;
  - `margin-top: 2px` y `margin-bottom: 8px`.

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
