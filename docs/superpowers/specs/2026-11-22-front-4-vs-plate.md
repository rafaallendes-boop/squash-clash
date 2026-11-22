# Front 4 — Pantalla «vs» con el estilo del rival

22 nov 2026 · Base `origin/main` `518f9c3`. Spec de Claude (arquitecto, modo product lead); revisión no independiente. Dura 1,5 s o menos, se salta con un toque y se muestra encima de la espera del saque, que ya existe. No suma tiempo ni pasos antes de jugar.

## 0. Lo que ya existe (código)

- **La espera del saque.** `RallyDirector.StartMatch` (`RallyDirector.cs:124-130`) llama a `BeginPoint` en el mismo cuadro. En un partido saca primero el jugador (`NewMatch`, `:137`, `firstServer` = `PlayerId.Human`). El saque es automático y sale enseguida hacia el rival (`:230-236`), con la fase `RivalReception`. El jugador no tiene ventana de golpe hasta que el rival devuelve y empieza `HumanReception` (`LaunchToHuman`, `:293`). Ese vuelo es la espera sobre la que se monta la placa; la placa no la crea.
- **El rival.** No tiene nombre propio. Tiene «Rival» y un estilo: `RivalStyle.Name` (`Core/Rally/RivalAi.cs:60`) devuelve Fondista, Definidor o Completo. Hoy el estilo aparece en la línea de estado del primer punto («Rival: Fondista · …», `RallyHud.cs:338`) y bajo «RIVAL» en el marcador, a 9 px (`:81-82,321-322`). La revancha conserva el rival (diseño del 19 nov).
- **Toques sobre la interfaz.** `GesturePreview.OverUi` (`GesturePreview.cs:62-68`) bloquea el gesto sobre ciertos elementos con nombre y sobre los botones. `TouchInputAdapter` suprime ese toque hasta que se suelta (`TouchInputAdapter.cs:71`).

## 1. Alcance

- **Cuándo se muestra.** Solo al empezar un partido desde el menú (`FirstRunFlow.StartMatch`, `hud.MenuMatch`), en el mismo cuadro en que sale el saque. No se muestra:
  - en la revancha, porque es el mismo rival;
  - en el «Partido» DEV;
  - en la práctica.
- **Qué dice.** «TÚ», «vs», y «RIVAL» con el estilo debajo («Definidor»). No hay nombres inventados.
- **Cuándo se cierra.** Lo que ocurra primero:
  - (a) termina su animación, de 1,5 s como máximo en tiempo real sin escala;
  - (b) un toque en cualquier parte de la pantalla bajo el encabezado;
  - (c) la fase deja de ser `RivalReception`;
  - (d) el jugador pausa.

  La condición (c) garantiza que la placa nunca está visible en una ventana de golpe, aunque el vuelo del saque dure menos de 1,5 s.
- **Sin efecto en el juego.** La placa no pausa el reloj, no demora el saque, no lee ni escribe estado del juego y no cambia `RallyParams`, el preset ni los hashes.
- **El toque.**
  - La capa `vs-plate-layer` cubre la pantalla bajo el encabezado, con `PickingMode.Position`. El botón «Pausa» sigue usable.
  - Se agrega su nombre a `OverUi`, así que el toque no inicia un gesto.
  - Si el dedo sigue apoyado cuando empieza `HumanReception`, ese toque sigue suprimido hasta que se suelta. Es el comportamiento de hoy para cualquier toque sobre la interfaz.
- **Interruptor DEV.** `RallyHud.VsPlateOn`, estático, por defecto encendido y que no se recuerda (precedente: `ExtendedCardOn`). Apagado, el partido empieza como hoy.
- **Métrica.** Una línea `menu` con `screen="vs"` y `action` igual a `shown`, `tapped` o `timeout`, con el `UserEvents.Menu` existente.

## 2. Fuera de alcance

- Nombres, retratos o avatares del rival.
- Un selector de estilo y cualquier cuenta regresiva.
- Mostrar la placa en la revancha.
- **Propuesta, no se implementa sin el «sí» del PO:** una bajada por estilo con las palabras del PO del 19 nov:
  - Fondista: «Juega al fondo»;
  - Definidor: «Busca definir»;
  - Completo: «De todo un poco».

## 3. Criterio de aceptación

1. Al tocar «Jugar» con el tutorial visto, `vs-plate` es visible en el primer cuadro de `RivalReception` del punto 1, con «TÚ», «vs», «RIVAL» y `RivalStyle.Name(rally.Style)`.
2. **No suma tiempo.** Con la misma semilla y con la placa encendida o apagada, el lanzamiento del saque ocurre en el mismo paso de simulación y en la misma hora real. La secuencia de puntos de un partido con piloto automático es idéntica.
3. La placa se oculta 1,5 s después de mostrarse como máximo. Con un toque, se oculta en ese mismo cuadro. Si `HumanReception` empieza antes, se oculta en ese mismo cuadro.
4. **No hay toque fantasma.** El toque que salta la placa queda filtrado por `TouchInputAdapter.SampleFiltered` con el motivo `suppressed-ui-or-contact`, y no se abre ningún gesto ni intento.
5. La placa no aparece en la revancha, en el «Partido» DEV ni en la práctica. Pausar la oculta.
6. La placa cabe en el área segura en todos los perfiles de `ZDeviceLayoutTests` y no se superpone al encabezado.
7. EditMode y PlayMode completos en verde en el Mac del PO, y `check-release` OK. AutoQA (PASS/0/1) es obligatorio porque se toca el filtro de toques del gesto.

## 4. Tests RED primero (Codex, TDD)

- **EditMode, `Rally/RallyTextTests`:** el texto del estilo para los tres valores, y la suma `VsInSeconds + VsHoldSeconds + VsOutSeconds ≤ 1,5`.
- **PlayMode, `ZVsPlateTests`** (sobre `ZFirstRunFixture`): los criterios 1 a 5.
  - Para el criterio 2, compara dos corridas con la placa encendida y apagada, con el interruptor estático DEV `RallyHud.VsPlateOn`, que por defecto está encendido y no se recuerda.
  - Para el criterio 3, avanza el reloj real y comprueba también el corte en `HumanReception`.
  - Para el criterio 4, inyecta un toque con la fixture de `TouchInputAdapterTests`.
- **PlayMode, `ZDeviceLayoutTests`:** el criterio 6.
- **PlayMode, `ZUiLookTests`:** la placa usa Rubik Bold Italic y la línea central tiene el color `--line`.

## 5. Sin iPhone y con iPhone

- **Sin iPhone, en el Editor:** todo §3.
- **Con iPhone, en la próxima instalación autorizada:**
  - si 1,5 s se siente como presentación y no como espera;
  - si el toque con un dedo real la salta sin comerse el primer golpe;
  - si se lee bien con el notch y la isla;
  - si el saque se sigue viendo detrás.

## 6. Dirección visual

Pasada de `/frontend-design`.

- **Concepto: la línea media.** Las dos mitades de la placa entran desde los bordes opuestos de la pantalla y se juntan sobre una línea roja vertical, la línea media de la cancha de squash. Esa línea es el «vs». No se inventa ninguna superficie: es el vidrio del HUD con las camisetas del jugador y del rival.
- **Posición.** Va en la franja vacía bajo la cancha, la misma que calcula `ShotNoteView.CourtBottomEdge` (#88). Así no tapa el frontis donde pega el saque. Si la franja es más baja que la placa (64 px), la placa se apoya sobre el borde seguro inferior, igual que la nota.
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
  - Al saltarla con un toque, desaparece sin animación de salida.

```
            (cancha: el saque vuela al frontis)
  ──────────────────────────────────────────────
  ┌──────────────────┃──────────────────┐
  │                  ┃  RIVAL           │
  │  TÚ           vs ┃  Definidor       │
  └── borde azul ────┃──── borde coral ─┘
            (borde seguro inferior)
```

- **Revisión contra lo genérico.** El reflejo era la pantalla «VS» de juego de pelea: pantalla partida en diagonal, un «VS» gigante metalizado y retratos. Se descartó por tres razones. Es un cliché. Se parece al estilo de juegos existentes. Y tapa el saque. También se descartó encoger el estilo hacia el marcador al salir: es un segundo movimiento que compite con la pelota. La línea media de la cancha conecta con el squash, ya está en el lenguaje visual (las líneas rojas de `--line`) y deja la cancha libre.
