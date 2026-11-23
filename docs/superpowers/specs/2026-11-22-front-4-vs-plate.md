# Front 4 — Pantalla «vs» con el estilo del rival

22 nov 2026, revisión 2 del 23 nov. Base `origin/main` `230b758`, que ya trae el saque nuevo encendido (#100). Spec de Claude como arquitecto. Roles, fusión e instalación según AGENTS §2, §7 y §8 de esa base.

Dura 1,5 s o menos, se salta con un toque y se muestra encima del vuelo del primer saque, que ya existe. No suma tiempo ni pasos antes de jugar.

**Plan de implementación.** Una rama y una PR propias.

- Lo implementa Claude con `/frontend-design` y Sonnet, y lo revisa Codex.
- Codex revisa con atención el nombre nuevo en `GesturePreview.OverUi`, porque toca el filtro de toques del gesto.
- Se fusiona con la revisión aprobada, las puertas en verde y las reglas de AGENTS §2, sin pedir un «sí» por fusión.
- Se instala solo en la ventana que SC-PM coordina con Rafael (AGENTS §8).

## 0. Lo que ya existe en `main` `230b758`

- **El vuelo del primer saque.**
  - `RallyDirector.StartMatch` (`RallyDirector.cs:133-138`) llama a `BeginPoint` (`:223`) en el mismo cuadro.
  - En un partido saca primero el jugador (`NewMatch`, `:145`).
  - Con el saque nuevo, ese primer saque es un globo automático, sin pausa previa (`:255-256`; `ZServeTests.cs:205-212`). Va al rival en la fase `RivalReception` (`LaunchToRival`, `:325-334`).
  - El jugador no tiene ventana de golpe hasta que el rival devuelve y empieza `HumanReception` (`:313`).
  - Ese vuelo es la espera sobre la que se monta la placa. La placa no la crea.
- **El rival** no tiene nombre propio, solo «Rival» y un estilo.
  - `RivalStyle.Name` devuelve Fondista, Definidor o Completo.
  - El estilo ya aparece en la línea de estado del primer punto (`RallyText.RivalIntro`, `RallyHud.cs:446`) y bajo «RIVAL» en el marcador (`rally-rival-style`, `:90,430`).
  - La revancha conserva el rival.
- **Toques sobre la interfaz.**
  - `GesturePreview.OverUi` (`GesturePreview.cs:62-69`) bloquea el gesto sobre elementos con ciertos nombres y sobre cualquier `Button`.
  - `TouchInputAdapter` lo consulta en `Began`, y un toque suprimido sigue suprimido hasta que se suelta (`TouchInputAdapter.cs:68-78`). Lo filtrado sale por `SampleFiltered` con el motivo `suppressed-ui-or-contact`.
- **Reloj de la vista.** `RallyHud.Update` recorta el `dt` a 0,05 s (`RallyHud.cs:290`), así que sumar ese `dt` no mide tiempo real. `PointEndFreeze.Now` (`PointEndFreeze.cs:14`) es el precedente de un reloj real reemplazable por los tests.

## 1. Alcance

- **Cuándo se muestra.** Solo al empezar un partido desde el menú (`FirstRunFlow.StartMatch`, `hud.MenuMatch`), en el mismo cuadro en que sale el saque. No se muestra:
  - en la revancha, porque es el mismo rival;
  - en el «Partido» DEV;
  - en la práctica.
- **Qué dice.** «TÚ», «vs», y «RIVAL» con el estilo debajo («Definidor»). No hay nombres inventados.
- **Reloj.** `VsPlate.Now` es real y sin escala (`Time.unscaledTimeAsDouble`) y los tests lo pueden reemplazar.
  - La edad de la placa es `Now() − shownAt`. No se suma el `dt` de `Tick`.
  - Mientras el reloj del juego está en pausa, la placa ya está cerrada por la causa (d).
- **Cuándo se cierra.** Lo que ocurra primero:
  - (a) su edad llega a la duración total, 1,5 s como máximo;
  - (b) un toque en cualquier parte de la capa, bajo el encabezado;
  - (c) la fase deja de ser `RivalReception`, como al empezar `HumanReception`, terminar el punto o salir del partido;
  - (d) el jugador pausa.

  La causa (c) garantiza que la placa nunca está visible en una ventana de golpe, aunque el vuelo del saque dure menos de 1,5 s.
- **Sin efecto en el juego.** La placa no pausa el reloj, no demora el saque, no lee ni escribe estado del juego y no cambia `RallyParams`, el preset ni los hashes.
- **El toque.**
  - La capa `vs-plate-layer` cubre la pantalla bajo el encabezado, con `PickingMode.Position`. El botón «Pausa» y los demás botones del encabezado siguen usables.
  - El nombre de la capa se agrega a `OverUi`, así que un toque sobre ella no abre un gesto.
  - Si el dedo sigue apoyado cuando empieza `HumanReception`, ese toque sigue suprimido hasta que se suelta. Es lo que ya pasa hoy con cualquier toque sobre la interfaz.
- **Interruptor DEV.** `RallyHud.VsPlateOn`, estático, encendido por defecto y que no se recuerda, como `ExtendedCardOn`. Apagado, el partido empieza como hoy.
- **Métricas.** Una línea `menu` con `screen="vs"`, a través del `UserEvents.Menu` existente (`UserMetrics.cs:38`).
  - `action=shown` al mostrarse.
  - Al cerrarse, **una sola** línea con el motivo:
    - `tapped` (b);
    - `timeout` (a);
    - `phase` (c);
    - `pause` (d).
  - Un cierre por fase o por pausa nunca se registra como `timeout`.

## 2. Fuera de alcance

- Nombres, retratos o avatares del rival.
- Un selector de estilo y cualquier cuenta regresiva.
- Mostrar la placa en la revancha.
- **Propuesta pendiente de decisión de producto:** una bajada por estilo con las palabras del PO del 19 nov:
  - Fondista: «Juega al fondo»;
  - Definidor: «Busca definir»;
  - Completo: «De todo un poco».

## 3. Criterio de aceptación

1. Al tocar «Jugar» con el tutorial visto, `vs-plate` es visible en el primer cuadro de `RivalReception` del punto 1, con «TÚ», «vs», «RIVAL» y `RivalStyle.Name(rally.Style)`.
2. **No suma tiempo.**
   - Las dos corridas usan las mismas entradas:
     - nivel, preset, `RallyParams` y `Seed`;
     - `RandomStyles=false` con el mismo `StyleOverride`, o `RandomStyles=true` con el mismo `StyleCounter` inicial;
     - piloto automático con el mismo `AutopilotTimingSigmaMs`;
     - el mismo reloj de prueba para `VsPlate.Now` y para `TouchInputAdapter.ReadRealTime`.
   - Con la placa encendida y con ella apagada:
     - el saque sale en el **mismo paso de simulación**;
     - el tiempo transcurrido desde el toque de «Jugar» hasta el lanzamiento, medido con ese reloj, es **el mismo**;
     - la secuencia de `PointResult` del partido es idéntica.
   - Todo con igualdad exacta, sin tolerancias.
3. **Cierre por tiempo y por fase.**
   - Con el reloj de prueba adelantado 1,5 s en **un solo cuadro**, la placa queda oculta en ese cuadro. Esto descarta que la edad se calcule sumando el `dt` recortado.
   - Si `HumanReception` empieza antes, la placa se oculta en ese mismo cuadro.
4. **No hay toque fantasma: el filtro integrado.** Se prueba con el `Touchscreen` y el reloj de entrada de las fixtures existentes (`ShotControllerTests.cs:49`), sin reemplazar `UiBlocker` por un stub.
   - El `Began` cae sobre la capa real. El *picking* del panel lo reconoce y `OverUi` da verdadero.
   - La placa se oculta en ese cuadro.
   - El mismo dedo sigue con `Moved` durante `RivalReception` y con `Moved` y `Ended` ya en `HumanReception`. Todas esas muestras salen por `SampleFiltered` con `suppressed-ui-or-contact`, y no se abre ningún gesto ni intento.
   - Después, un `Began` nuevo en `HumanReception` llega por `SampleReceived` y abre un gesto normal.
5. **Pausa.**
   - Tocar «Pausa» con la placa visible pausa el juego, oculta la placa y escribe `pause`.
   - Los botones de la pantalla de pausa siguen funcionando.
6. **Dónde no aparece.** La placa no aparece en la revancha, en el «Partido» DEV ni en la práctica.
7. **Métricas.** En cada uno de los cuatro motivos de cierre hay exactamente una línea `shown` y una sola de cierre, con el motivo correcto.
8. **Layout.** La placa cabe en el área segura en todos los perfiles de `ZDeviceLayoutTests` y no se superpone al encabezado.
9. **Puertas.**
   - EditMode y PlayMode completos en verde en el Mac del PO.
   - AutoQA `scripts/autoqa.sh mvp` PASS/0/nivel 1, obligatorio porque se toca el filtro de toques del gesto.
   - `check-release` OK.

## 4. Tests RED primero (TDD) y reglas de prueba

**Tests nuevos.**

- **EditMode, `Rally/RallyTextTests`.** El texto del estilo para los tres valores, y que `VsInSeconds + VsHoldSeconds + VsOutSeconds ≤ 1,5`.
- **PlayMode, `ZVsPlateTests`**, sobre `ZFirstRunFixture`. Cubre los criterios 1 a 7.
  - El criterio 2 compara dos corridas con `RallyHud.VsPlateOn` encendido y apagado.
  - Los criterios 3 y 4 reemplazan `VsPlate.Now` y `ReadRealTime` por el reloj de prueba.
- **PlayMode, `ZDeviceLayoutTests`.** El criterio 8.
- **PlayMode, `ZUiLookTests`.** La placa usa Rubik Bold Italic y la línea central tiene el color `--line`.

**Reglas de prueba, fijadas antes de implementar.**

- **Los tests existentes no se editan** y tienen que pasar con la placa **encendida**, que es el valor por defecto.
- **Si un test existente falla**, es un defecto. Se registra y se detiene, según AGENTS §2. No se apaga `VsPlateOn`, no se cambia una aserción y no se cambia una duración para pasar (AGENTS §3).
- **El apagado** solo se usa en la corrida de comparación del criterio 2, que está fijada de antemano.

## 5. Sin iPhone y con iPhone

**Se implementa en su propia PR.**

**Sin iPhone, en el Editor:** todo §3. Eso incluye el tiempo, el filtro de toques integrado, las métricas, el layout en los perfiles de dispositivo y las puertas.

**Con iPhone, en la ventana que SC-PM coordina con Rafael (AGENTS §8).** Dentro de esa ventana se hacen el respaldo y el cotejo de presets y métricas, como en cualquier instalación.

- Resultados observables:
  - 1,5 s se sienten como presentación y no como espera;
  - un toque con un dedo real salta la placa sin comerse el primer golpe;
  - la placa se lee bien con el notch y la isla;
  - el saque se sigue viendo detrás.
- Las métricas copiadas traen `shown` y una sola línea de cierre por partido.

## 6. Dirección visual

Pasada de `/frontend-design`.

- **Concepto: la línea media.** Las dos mitades de la placa entran desde los bordes opuestos de la pantalla y se juntan sobre una línea roja vertical, la línea media de la cancha de squash. Esa línea es el «vs». No se inventa ninguna superficie: es el vidrio del HUD con las camisetas del jugador y del rival.
- **Posición.** Va en la franja vacía bajo la cancha, la misma que calcula `ShotNoteView.CourtBottomEdge` (#88), para no tapar el frontis donde pega el saque. Si la franja es más baja que la placa (64 px), la placa se apoya sobre el borde seguro inferior, igual que la nota.
- **Tokens.**
  - Mitad izquierda: `--glass` con borde inferior de 4 px `--human`. «TÚ» en Rubik Bold Italic 34, color `rgb(128,194,255)`.
  - Mitad derecha: `--glass` con borde inferior de 4 px `--rival`. «RIVAL» en Rubik Bold 13, `--glass-soft`, sobre el estilo en Rubik Bold Italic 34, color `rgb(255,150,130)`.
  - Centro: línea de 3 px `--line` y «vs» en Rubik Bold Italic 20 blanco.
  - Todo está en la escala tipográfica existente de `Hud.uss`.
- **Movimiento.** Es el único momento orquestado del ítem.
  - Entrada: 0,18 s (`VsInSeconds`, igual que `BannerInSeconds`), con *smoothstep*.
  - Pausa: 1,10 s (`VsHoldSeconds`).
  - Salida: 0,20 s (`VsOutSeconds`) con fundido.
  - Total: 1,48 s.
  - Al saltarla con un toque, o al cerrarse por fase o por pausa, desaparece sin animación de salida.

```
            (cancha: el saque vuela al frontis)
  ──────────────────────────────────────────────
  ┌──────────────────┃──────────────────┐
  │                  ┃  RIVAL           │
  │  TÚ           vs ┃  Definidor       │
  └── borde azul ────┃──── borde coral ─┘
            (borde seguro inferior)
```

- **Revisión contra lo genérico.** El reflejo era la pantalla «VS» de juego de pelea: pantalla partida en diagonal, un «VS» gigante metalizado y retratos. Se descartó porque es un cliché, se parece al estilo de juegos existentes y tapa el saque. También se descartó encoger el estilo hacia el marcador al salir: es un segundo movimiento que compite con la pelota. La línea media de la cancha conecta con el squash, ya está en el lenguaje visual (las líneas rojas de `--line`) y deja la cancha libre.
