# Front 2 — Pistas de los primeros partidos (saque, volea y dos paredes)

22 nov 2026, revisión 2 del 23 nov. Base `origin/main` `230b758`, que ya trae el saque nuevo encendido por defecto (#100). Spec de Claude como arquitecto. Roles, fusión e instalación según AGENTS §2, §7 y §8 de esa base.

Hay como máximo **4 pistas**, y cada una se ve una sola vez por instalación. Viajan en lo que ya se ve entre puntos, el aviso o la guía de saque, y no agregan pasos ni tiempo antes de jugar ni entre puntos. La prioridad es el saque nuevo: es lo que el PO no entendió al probarlo, según el encargo de SC-Lead.

**Plan de implementación.** Una rama y una PR propias, en dos tramos y en este orden:

1. **Datos.** Lo implementa Codex y lo revisa Claude. Incluye:
   - `HintPicker`;
   - la ventana y la persistencia;
   - las lecturas `LastServeChosen` y `LastStrokeBoast`, esta última si no llegó antes con la spec 3;
   - las métricas;
   - los criterios 1 a 9 y 11.
2. **Visual.** Lo implementa Claude con `/frontend-design` y Sonnet, y lo revisa Codex. Incluye la fila del aviso, la pista de la guía de saque, los estilos, los textos, el botón DEV y los criterios 10 y 12.

Se fusiona con la revisión aprobada, las puertas en verde y las reglas de AGENTS §2, sin pedir un «sí» por fusión. Se instala solo en la ventana que SC-PM coordina con Rafael (AGENTS §8).

## 0. Lo que ya existe en `main` `230b758`

- **Aviso entre puntos** (`RallyHud.cs:120-126,398-416`).
  - Se arma una vez por punto terminado y solo se ve en `PointOver`.
  - Entra en `BannerInSeconds`, 0,18 s (`:47`). `BannerOpacity` (`:58`) dice cuánto entró.
  - La línea `rally-banner-serve-hint` existe pero queda siempre oculta (`:126`).
- **Pausa antes del saque del jugador.**
  - `HumanServePause` (`RallyDirector.cs:92-93`) dura `ServeKindHumanPauseSeconds`, 3 s (`RallyParams.cs:44`). Las demás pausas de partido duran `MatchPauseSeconds`, 2,5 s (`:32`, `RallyDirector.cs:90-91`).
  - En esa pausa el aviso **completo** se oculta y se ve la guía de saque (`RallyHud.cs:414-416`): la línea que se acorta con el tiempo, «Toca arriba: globo» y «Toca abajo: drive» (`:132-152,173-224`).
  - Un toque en el frontis elige el saque y lo lanza enseguida (`ChooseHumanServe`, `RallyDirector.cs:197-204`). Sin toque, sale globo (`:255`).
  - «Seguir» salta cualquier pausa (`SkipWait`, `:147`).
- **El primer saque del partido** lo hace el jugador, con globo automático y **sin pausa previa** (`StartMatch`, `:133-138`; `ZServeTests.cs:205-212`). Para elegir un saque, el jugador primero tiene que ganar un punto.
- **Tutorial.** El sexto paso, «Saque», enseña lo mismo (`FirstRunFlow.cs:46,54`). La pista no lo reemplaza.
- **Primer partido desde el menú.** `FirstRunFlow.StartMatch` (`FirstRunFlow.cs:488-495`) deja `hud.MenuMatch` en verdadero.
- **Lecturas del golpe** (`ShotController.cs`), de solo lectura:
  - `LastOpportunity` (`:60`);
  - `VolleyShown` (`:62`);
  - `SideAimOpen` (`:84`);
  - `ShotResolved` (`:104`).
  - `LastBoast` (`:85,324`) **no sirve** para saber si un golpe fue un dos paredes aplicado. Se escribe también en resoluciones no autoritativas, no se limpia entre golpes y no distingue el drive de respaldo. Ver §1.6.
- **Métricas.** `UserMetrics` (`Core/Diagnostics/UserMetrics.cs`) escribe `match-start`, `match-end`, `menu`, `pause` y `tutorial-step`. No escribe nada de pistas ni de cómo salió el saque.
- **Dato que motiva la ventana** (RAL-193, `docs/reviews/2026-11-22-metricas-iphone.md:27,31-77`): 45 inicios de partido, 19 cierres normales, 18 abandonos y 8 sin cierre. Hay abandonos en 0–0 a los 1,75 s y a los 3,00 s, y varios en 0–1 y 0–2 sin un punto ganado.
  - Es un respaldo histórico, anterior al saque nuevo y con `metricsValid=false` en parte de los eventos.
  - Muestra una necesidad. No prueba la causa de los abandonos ni que estas pistas los reduzcan.

## 1. Alcance

### 1.1 Ventana de pistas

Reemplaza a `sc.hints.match`, que ya no se usa.

- **Partidos en la ventana.** Lo está todo partido iniciado desde el menú (`FirstRunFlow.StartMatch`) mientras `sc.hints.window` no valga 1 y quede alguna pista sin ver.
- **Quedan fuera** la revancha, el «Partido» DEV, la práctica y cualquier partido en que el piloto automático estuvo encendido.
- **Cuándo se cierra.** `sc.hints.window` se escribe en 1 cuando un partido de la ventana termina de forma normal (`MatchOver`, gane o pierda) o cuando las 4 pistas quedan vistas o retiradas.
- **Abandonar no la cierra.** Ni volver al menú desde la pausa (`StopMode`) ni cerrar la app (`UserEvents.EndSession` desde `AppBootstrap.OnDestroy`) cierran la ventana.
- **Respuesta para quien abandona pronto.** El siguiente partido desde el menú sigue en la ventana, con todas las pistas que no llegó a ver.
- **Límite.** Son 4 pistas por instalación, cada una una sola vez. La ventana solo puede cerrarse antes. No se agrega un tope de intentos; queda como decisión abierta.

### 1.2 Pistas

| Id | Disparador (hechos del juego, sin umbrales nuevos) | Dónde se ve | Texto | Clave |
|---|---|---|---|---|
| H0 Saque | Empieza una `HumanServePause` en un partido de la ventana. | Guía de saque: `serve-guide-hint` | «Sacas tú: toca el frontis antes de que se acabe la línea. Si no tocas, sale globo.» | `sc.hints.serve` |
| H1 Volea | En una recepción del jugador, `VolleyShown` fue verdadero en algún cuadro y la recepción terminó sin volea aplicada: hubo un golpe con `LastOpportunity != Volley`, un fallo o ningún golpe. | Aviso: `rally-banner-hint` | «Anillo ámbar: toca y volea antes del bote.» | `sc.hints.volley` |
| H2 Dos paredes | `ShotResolved` de un golpe aplicado del jugador (`Perfect` o `Valid`), con `SideAimOpen` verdadero y `LastStrokeBoast` falso, sin ningún dos paredes en el partido. | Aviso | «Toca la pared lateral para jugar un dos paredes.» | `sc.hints.boast` |
| H3 Cruce | `ShotResolved` del primer golpe del partido con `LastStrokeBoast` verdadero. | Aviso | «Lateral cerca de ti: la pelota cruza al otro lado.» | `sc.hints.boastCross` |

- **Textos.** Salen del tutorial aprobado (`FirstRunFlow.cs:52-54`), acortados. El PO puede cambiarlos sin tocar la lógica.
- **H0 y el tutorial.** H0 enseña lo que el sexto paso ya dice, pero en el momento en que hace falta. Haber visto el tutorial no retira H0.

### 1.3 Turnos y convivencia con la guía de saque

- **Armar.** Un disparador **arma** su pista. La pista armada vive solo en memoria, hasta su pausa elegible.
- **Pausa ordinaria.** Es `PointOver` sin `HumanServePause` y sin `MatchOver`, con el aviso visible. Muestra como máximo una pista de H1 a H3, en ese orden de prioridad. Si hay más armadas, esperan a la pausa ordinaria siguiente del mismo partido.
- **Pausa del saque del jugador** (`HumanServePause`, 3 s). Solo puede mostrar H0, dentro de la guía.
  - H1 a H3 esperan: el aviso está oculto, así que ahí no se muestran ni se marcan como vistas.
  - La pausa no se alarga para hacer caber la pista.
- **Cuándo una pista queda vista.** Se escribe su clave en 1 y se llama a `PlayerPrefs.Save()` en ese mismo cuadro.
  - **En el aviso**, cuando `BannerOpacity` llega a 1. En ese cuadro deben estar visibles tanto el aviso (`rally-banner`) como la fila de la pista, con un layout resuelto de alto mayor que 0.
  - **En la guía**, cuando `serve-guide` y `serve-guide-hint` llevan 0,18 s visibles, con un layout resuelto de alto mayor que 0. Es el mismo `BannerInSeconds`; no hay un número nuevo.
  - Ese tiempo es real y sin escala. Se mide con un reloj reemplazable, `FirstMatchHints.Now`, como `PointEndFreeze.Now`, y no se suma el `dt` recortado a 0,05 s de `RallyHud.Update` (`:290`).
  - Con el reloj del juego en pausa (`app.Clock.Paused`), el plazo no avanza.
- **«Seguir» o un toque de saque antes de 0,18 s.** La pista no queda vista y sigue armada:
  - H1 a H3 vuelven en la próxima pausa ordinaria del mismo partido;
  - H0 vuelve en la próxima `HumanServePause`, de este partido o de uno siguiente de la ventana.
- **No sale ninguna pista** en `MatchOver`, donde está la tarjeta, ni fuera de `PointOver`.
- **Al terminar o abandonar un partido**, las pistas armadas se descartan sin marcarse.
- **H2 armada y dos paredes antes de verla.** Si el jugador hace su primer dos paredes del partido antes de que H2 salga, H2 se **retira**: su clave pasa a 2, cuenta como resuelta y no vuelve. Ese mismo golpe arma H3.
- **Una pista vista o retirada** no se vuelve a armar.

### 1.4 Evidencia de que entendió el saque siguiente

- **Lectura nueva.** `RallyDirector.LastServeChosen` vale verdadero cuando el saque que empezó el punto venía de un toque (`ChooseHumanServe`) y falso cuando salió solo. Se fija en `BeginPoint`, junto a `LastServeType`. Es de solo lectura y no cambia ninguna decisión.
- **Métrica.** Cada saque del jugador que sigue a una `HumanServePause` escribe una línea `serve-choice` con estos campos:
  - `matchId`;
  - `chosen`, que es `LastServeChosen`;
  - `kind`, que es `Lob` o `Drive`;
  - `hintSeen`, si `sc.hints.serve` ya valía 1 al empezar esa pausa.
- **Evidencia en el Editor.** Después de ver H0, un toque arriba de `ServeKindLobMinAimY` da `chosen=true` y `kind=Lob`; un toque abajo da `Drive`; sin toque, `chosen=false` y `Lob`.
- **Evidencia con personas** (§5). El saque que sigue a H0, y el siguiente, salen con `chosen=true`, y la persona sabe explicar cómo eligió.

### 1.5 Métricas de pistas

- Una línea `hint` con estos campos:
  - `matchId`;
  - `id` (`serve`, `volley`, `boast` o `boastCross`);
  - `action`: `seen` o `retired`.
- Se escribe una sola vez por pista y por instalación, en el mismo cuadro en que se persiste la clave.
- Las líneas actuales (`match-start`, `match-end`, `menu` y las demás) no cambian. Lo nuevo es aditivo.

### 1.6 Contrato mínimo de `LastStrokeBoast`

La spec 3 (PR #91) define esta marca y la necesita con el mismo contrato. Si la PR de la spec 3 se fusiona antes, esta la reutiliza. Si no, esta PR la introduce con los casos de `ZBoastTests` de la spec 3, y la otra la reutiliza.

- `ShotController.LastStrokeBoast` es un `bool` que describe el golpe del `ShotRecord` que está saliendo por `ShotResolved`. Se lee dentro de ese callback, que es lo que identifica el golpe.
- Vale verdadero solo si se cumplen estas tres condiciones:
  - la orden apuntaba a la lateral (`order.Side.Exists`);
  - la resolución **autoritativa** de ese golpe usó `SolveBoast` con `boast.Solved` y `!LastCausal.FellBack` (`ShotController.cs:318-329`);
  - el golpe quedó aplicado (`Perfect` o `Valid`).
- Un dos paredes que cayó al drive de respaldo da falso y cuenta como golpe al frontis.
- Se limpia en cada lanzamiento (`:125`).
- No se deduce de `LastBoast` ni del solo hecho de apuntar a la lateral.

### 1.7 Dónde vive el código

- `HintPicker`, clase pura en `Core/Rally`.
  - Recibe los hechos del punto y los de la pausa: si es ordinaria, `HumanServePause` o `MatchOver`.
  - Recibe también el estado por pista (sin ver, armada, vista o retirada).
  - Devuelve qué pista mostrar y qué cambios de estado hacer.
- `FirstMatchHints`, adaptador en `Runtime/Menu`, junto a `PersonalRecords`.
  - Lee y escribe `PlayerPrefs` y expone `AllKeys()`.
  - Tiene `Now`, el reloj reemplazable.
  - Tiene `Enabled`, un interruptor DEV estático: encendido por defecto, no se recuerda y, apagado, no escribe ni muestra nada. Es el mismo modelo que `PersonalRecords.Enabled` (`PersonalRecords.cs:21`).
- **Botón DEV «Reiniciar pistas».** Va en el panel DEV y solo existe con `SQUASH_DEV`. Borra únicamente las claves de `FirstMatchHints.AllKeys()` y nada más de `PlayerPrefs`, presets ni métricas.

## 2. Fuera de alcance

- Cambiar el primer saque del partido (el globo automático en 0–0), las pausas, `ServeKindLobMinAimY` o cualquier parámetro del juego. No se recalibra nada.
- Pistas de cansancio, de timing u otras. Un segundo pase de pistas, o volver a verlas desde el menú.
- Cambiar el tutorial o agregarle pasos.
- **Propuesta que necesita la autorización del PO**, porque crea un umbral nuevo (AGENTS §7): que H2 salga solo cuando el jugador golpeó desde el cuarto trasero (`z ≥ X m`).

## 3. Criterio de aceptación

1. **H0.** En un partido de la ventana, en la primera `HumanServePause`, `serve-guide-hint` se ve con el texto de H0.
   - Con la guía visible durante 0,18 s del reloj de prueba, `sc.hints.serve` queda en 1 y se escribe una sola línea `hint` con `id=serve` y `action=seen`.
   - Un toque arriba y otro abajo dan `serve-choice` con `chosen=true` y `Lob` o `Drive`. Sin toque, `chosen=false` y `Lob`.
   - La pausa sigue durando `ServeKindHumanPauseSeconds`.
2. **H1.** Después de un punto con el anillo ámbar visto y sin volea aplicada, la pausa ordinaria siguiente muestra H1 en `rally-banner-hint`. `sc.hints.volley` pasa a 1 solo en el cuadro en que `BannerOpacity` llega a 1, con el aviso y la fila visibles.
3. **H2 y H3, de integración.**
   - Un golpe aplicado al frontis sin dos paredes previo arma H2.
   - El primer dos paredes aplicado arma H3.
   - Un dos paredes que cae al drive de respaldo (`FellBack`) no arma H3 y cuenta como golpe al frontis.
   - Con H2 armada y sin mostrar, un dos paredes aplicado deja `sc.hints.boast` en 2, escribe `hint` con `action=retired` y arma H3.
4. **Prioridad y arrastre.**
   - Con H1 y H2 armadas en el mismo punto, sale H1 y H2 sale en la pausa ordinaria siguiente.
   - Una pista armada antes de una `HumanServePause` no se ve ni se marca ahí, y sale en la pausa ordinaria siguiente.
   - Ninguna clave cambia mientras su contenedor está oculto.
5. **«Seguir» o un toque de saque antes de 0,18 s.** La pista no queda vista y vuelve como dice §1.3. Si el jugador toca después de 0,18 s, sí queda vista.
6. **Abandono temprano.** Se prueba este recorrido: inicio desde el menú, abandono en 0–0 sin ninguna pausa, y nuevo inicio desde el menú.
   - Tras el abandono no existe ninguna clave `sc.hints.*`, y el segundo partido está en la ventana y muestra la primera pista que corresponda.
   - Lo mismo vale cuando el partido termina con `EndSession`, como al cerrar la app.
   - Una pista armada y no vista al abandonar vuelve a armarse en el partido siguiente.
7. **Cierre de la ventana.**
   - Tras un partido de la ventana que termina normalmente, `sc.hints.window` vale 1 y el siguiente partido desde el menú no muestra pistas.
   - La revancha, el «Partido» DEV, la práctica y un partido con piloto automático nunca muestran pistas.
8. **Nunca en la ventana de golpe.** Recorriendo un partido entero cuadro a cuadro, `rally-banner-hint` y `serve-guide-hint` están ocultos en todo cuadro con `Phase != PointOver`.
9. **Sin tiempo agregado.**
   - Se fijan las mismas entradas en ambas corridas:
     - semilla, nivel, preset y `RallyParams`;
     - `RandomStyles=false` con el mismo `StyleOverride`;
     - piloto automático con el mismo `AutopilotTimingSigmaMs`;
     - la ventana forzada por el test.
   - Con las pistas encendidas o apagadas, la secuencia de `PointResult`, el paso de inicio de cada punto y cada `WaitBetweenPoints` son idénticos, con igualdad exacta.
10. **Layout.**
    - Con la pista más larga, el aviso cabe en el área segura en todos los perfiles de `ZDeviceLayoutTests`, con el texto en dos líneas como máximo y sin elipsis.
    - `serve-guide-hint` cabe en el área segura, en tres líneas como máximo y sin elipsis, y no se cruza con la línea ni con los rótulos de la guía.
11. **Persistencia.** Al reiniciar la escena con las mismas `PlayerPrefs`, ninguna pista vista o retirada vuelve, y la ventana conserva su estado. El botón DEV borra solo `FirstMatchHints.AllKeys()`.
12. **Puertas.**
    - EditMode y PlayMode completos en verde en el Mac del PO.
    - AutoQA `scripts/autoqa.sh mvp` PASS/0/nivel 1, obligatorio porque los disparadores leen la ruta de resolución del golpe. Hay que mostrar que no cambia ninguna decisión.
    - `check-release` OK.

## 4. Tests RED primero (TDD) y reglas de prueba

**Tests nuevos.**

- **EditMode, `Rally/HintPickerTests`.** Cubre §1.1 a §1.3:
  - la ventana;
  - cada disparador;
  - la prioridad;
  - una pista por pausa;
  - el arrastre;
  - la `HumanServePause` con H0 sola;
  - el retiro de H2;
  - «Seguir» antes de 0,18 s;
  - que una pista vista o retirada no vuelve;
  - que nunca hay más de 4.
- **EditMode, `Diagnostics/UserMetricsTests`.** `hint` y `serve-choice` con sus campos, emitidos una sola vez, y las líneas actuales idénticas.
- **PlayMode, `ZFirstMatchHintsTests`**, sobre `ZFirstRunFixture`.
  - Cubre los criterios 1 a 9 y 11.
  - Las escenas de volea v2 (`ZVolleyV2Tests`), dos paredes (`ZBoastTests`) y saque (`ZServeTests`) sirven de base.
  - El criterio 8 se comprueba con piloto automático y la ventana forzada por un punto de prueba interno, que solo usan los tests nuevos.
- **PlayMode, `ZBoastTests`.** Los casos de `LastStrokeBoast` de §1.6, si esta PR introduce la marca.
- **PlayMode, `ZDeviceLayoutTests`.** El criterio 10.
- **PlayMode, `ZUiLookTests`.** El aro de H1 tiene el color de `ShotController.VolleyTint`, y las pistas usan Rubik Regular.

**Reglas de prueba, fijadas antes de implementar.**

- **Los tests existentes no se editan** y tienen que pasar con las pistas **encendidas**, que es el valor por defecto.
- **Un solo cambio de fixture, declarado de antemano.** `ZFirstRunFixture.ClearPrefs` (`ZFirstRunFixture.cs:78`) también borra `FirstMatchHints.AllKeys()`, igual que ya hace con `PersonalRecords.AllKeys()`.
- **Si un test existente falla con las pistas encendidas**, es un defecto. Se registra y se detiene, según AGENTS §2. No se apaga `FirstMatchHints.Enabled`, no se cambia una aserción, una pausa ni un texto para pasar, y no se agrega `SetNewServe(false)` (AGENTS §3).
- **El comportamiento apagado** tiene un solo test, también fijado de antemano: `Off_NothingIsWrittenOrShown`. La ruta encendida queda cubierta por los tests nuevos, y los existentes conservan todas sus aserciones.

## 5. Sin iPhone y con iPhone

**Se implementa en su propia PR.**

**Sin iPhone, en el Editor:** todo §3. Eso incluye la lógica, la persistencia, la integración, el layout en los perfiles de dispositivo y las puertas.

**Con iPhone, en la ventana que SC-PM coordina con Rafael (AGENTS §8).** Dentro de esa ventana se hacen el respaldo y el cotejo de presets y métricas que pide §8, como en cualquier instalación.

1. En el panel DEV, «Reiniciar pistas». No se borra la app ni todas las `PlayerPrefs`, porque hay presets que preservar.
2. Se toca «Jugar» y se gana un punto. Resultado observable: en la pausa del saque, la pista se lee en los 3 s y no tapa el frontis.
3. Se toca arriba o abajo de la línea, según lo que diga la pista. Resultado observable: sale el saque elegido y la métrica `serve-choice` trae `chosen=true` con el `kind` tocado.
4. Se sigue jugando, para ver H1 a H3 cuando aparezcan. Resultado observable: cada pista se lee en la pausa real de 2,5 s, y el aro ámbar del aviso se reconoce como el anillo de la cancha.
5. Se abandona en 0–0 desde la pausa y se vuelve a tocar «Jugar». Resultado observable: las pistas pendientes siguen saliendo.
6. Al cerrar la ventana, las métricas copiadas traen las líneas `hint` y `serve-choice`.

**Protocolo de observación con personas reales**, en la prueba de usuarios. Habilitar TestFlight sigue siendo exclusivo del PO (AGENTS §7).

- Sin explicarle nada a la persona, se registra:
  - de las métricas: las líneas `hint`, las líneas `serve-choice` y el `match-end` (cierre o abandono, y marcador);
  - de la observación: si tocó el frontis en la pausa del saque.
- Al terminar o abandonar, una sola pregunta: «¿Cómo eliges el saque?». La respuesta cuenta como entendida si menciona tocar el frontis arriba o abajo.
- Se informan los conteos por persona: exposición a cada pista, saques elegidos frente a automáticos, respuesta a la pregunta y cierre o abandono.
- **No hay un porcentaje de aceptación.** La comparación con RAL-193 es solo descriptiva, porque aquel respaldo es anterior al saque nuevo.

## 6. Dirección visual

Pasada de `/frontend-design`.

**El aviso, para H1 a H3.** La pista es una fila más dentro del vidrio `.sc-banner`, debajo de «Saca …», separada por un filete de 1 px `--glass-edge`. Entra con el aviso, sin tarjeta, capa ni movimiento propio.

- `.sc-banner-hint`:
  - fila, con `align-items: center`;
  - `margin-top: 8px`, `padding-top: 8px` y `border-top-width: 1px` `--glass-edge`;
  - `max-width: 280px`;
  - texto en Rubik Regular 14, `--glass-text`, con `white-space: normal`.
- Token nuevo `--volley: rgb(255, 184, 41)`, igual a `ShotController.VolleyTint`.
- **La miniatura.**
  - H1 lleva un aro de 14 px con borde de 2 px `--volley`: el anillo ámbar de la cancha, en chico.
  - H2 y H3 llevan una esquina de 14 px con borde izquierdo y superior de 2 px `--glass-text`, que es la planta de la lateral y el frontis.

**La guía de saque, para H0.** El frontis queda libre, porque es donde el jugador tiene que tocar.

- `serve-guide-hint` va en la franja vacía bajo la cancha, la misma de `ShotNoteView.CourtBottomEdge` (#88). Durante la pausa del saque ahí no hay notas de golpe.
- Es una pastilla de vidrio `--glass` con borde de 1 px `--glass-edge`, Rubik Regular 14 `--glass-text` y `max-width: 280px`.
- Su miniatura es un trazo blanco de 14 × 3 px, la línea de la guía en chico, que conecta la pista con la línea que se acorta arriba.
- Entra y sale con la guía, sin movimiento propio.

```
   ┌──────── frontis ────────┐
   │   Toca arriba: globo    │
   │ ─────────────────────── │  ← línea de la guía (se acorta)
   │   Toca abajo: drive     │
   └─────────────────────────┘
          (cancha)
   ╭──────────────────────────╮
   │ ━  Sacas tú: toca el     │  ← franja bajo la cancha
   │    frontis antes de que  │
   │    se acabe la línea.    │
   │    Si no tocas, sale     │
   │    globo.                │
   ╰──────────────────────────╯
```

**Revisión contra lo genérico.** Se descartó un *coach mark* con flecha y botón «Entendido»: suma un paso, tapa la cancha y en la pausa del saque taparía justo el frontis. Se descartó también el ícono de bombilla o «i». Cada miniatura usa la misma forma que el jugador ve en la cancha.
