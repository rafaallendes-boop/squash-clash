# Front 1b — Saque nuevo: parado en el cuadro, con aviso de dónde tocar (RAL-177)

22 nov 2026 · Base `origin/main` `518f9c3`. Spec de Claude (arquitecto, modo product lead); revisión no independiente. **Esta spec no enciende «Saque nuevo».** Todo lo que sigue vive detrás del interruptor, que sigue apagado por defecto. Encenderlo es otra PR y espera el «sí» del PO sobre la desviación del Definidor (−5,1/−5,5 pp, [informe del 20 nov](../../reviews/2026-11-20-serve.md)).

## 0. El saque nuevo tal como existe

Funciona solo con «Saque nuevo: sí» en el panel DEV (`RallyParams.ServeKindOn`, `RallyParams.cs:38`).

- **Elegir el saque.** En la pausa antes del saque del jugador, un toque en el frontis elige el tipo: arriba de `ServeKindLobMinAimY` (3,0 m) es globo y abajo es drive. Saca al soltar (`RallyDirector.ChooseHumanServe`, `:178-182`, que llama a `SkipWait`). Vale un toque simple, porque `ServeOrder` acepta `Tap` y `AbandonedNoPull` (`ShotController.cs:90`). Si no toca, al terminar la pausa sale el globo.
- **El primer saque del partido** no tiene pausa previa y siempre es globo.
- **Las formas del saque** son fijas por cuadro:
  - globo: frontis entre 3,45 y 3,6 m, toca la lateral y bota en el rincón;
  - drive: frontis entre 2,4 y 2,55 m, a 22 m/s.
- **El rival** elige su saque según su estilo.
- **La pausa** es `MatchPauseSeconds`, 2,5 s (`RallyParams.cs:32`).
- **El cuerpo** se teletransporta al cuadro recién en `BeginPoint` (`RallyDirector.cs:228-229`). Durante la pausa el jugador sigue donde terminó el punto.
- **El aviso de hoy** es una línea amarilla de 13 px dentro del aviso entre puntos: «Toca el frontis: arriba globo, abajo drive» (`RallyText.ServeHint`, `RallyHud.cs:119,304`).
- **Un problema de vista**, en las capturas `docs/visual/2026-11-16-device-robustness/*-06-banner.png`: el aviso entre puntos queda justo encima del frontis, que es donde hay que tocar.

## 1. Alcance

Solo con el interruptor encendido y solo en la pausa antes de un saque **del jugador**, después de un punto. El primer saque del partido y los saques del rival no cambian.

1. **Parado en el cuadro.** Cuando termina el congelado de fin de punto (`PointEndFreeze`, 0,1 s), el cuerpo del jugador aparece quieto, de cara al frontis, en `ServePlanner.BoxPosition(context, Match.Box)`, con `BodyPhase.Waiting`. `BeginPoint` lo vuelve a poner en el mismo lugar, así que el estado al empezar el punto es idéntico al de hoy.
2. **Unos 3 s para sacar.** Hay un campo nuevo, `RallyParams.ServeKindHumanPauseSeconds = 3`, que se usa en `WaitBetweenPoints` solo en esta pausa. La pausa antes del saque del rival sigue en 2,5 s.
   - El prefijo `ServeKind*` mantiene las proyecciones de hash de los tests. El hash completo de RallyParams cambia (`a82c34b661bf` → nuevo), aunque el interruptor esté apagado. Se declara en el informe.
   - Lo demás no cambia: el toque saca al instante, «Seguir» saca el globo y sin toque sale el globo.
3. **El aviso de dónde tocar**, con la dirección visual de §6, mientras dura la pausa:
   - Una línea horizontal sobre el frontis a 3,0 m de altura. Su posición en pantalla se proyecta con la cámara, igual que `ShotNoteView.CourtBottomEdge` (#88).
   - «Toca arriba: globo» encima de la línea y «Toca abajo: drive» debajo.
   - La línea hace de reloj: se acorta desde los dos extremos hacia el centro a medida que pasan los 3 s.
   - Todo el aviso tiene `PickingMode.Ignore`: el toque pasa al gesto igual que hoy.
   - El aviso reemplaza a la línea `serveHint`, que en esta pausa queda oculta pero sigue existiendo.
4. **El frontis queda libre.** En esta pausa, el aviso entre puntos se muestra en la franja bajo la cancha, la de la nota del golpe, en lugar de encima del frontis. La nota del golpe se oculta mientras tanto, porque su texto ya está en la causa del aviso. Si la franja no alcanza, se cae la línea «Saca Tú (…)», porque el cuerpo en el cuadro ya lo dice. El título y la causa no se cortan.
5. **Onboarding.** El tutorial suma un sexto paso, «Saque», con su dibujo (el frontis con la línea de 3,0 m, globo arriba y drive abajo). Texto propuesto: «Cuando sacas tú, toca el frontis: arriba de la línea sale globo, abajo sale drive. Si no tocas, sale globo.»
   - El paso solo se incluye si `rally.NewServeNextPoint` es verdadero cuando se abre el tutorial. **Con el interruptor apagado el tutorial queda igual, con 5 pasos.**
   - Hay tensión con la regla de la semana («ningún ítem suma un paso antes de jugar»). El paso aparecerá recién cuando se encienda el interruptor, así que esa decisión pertenece a la PR del encendido.

## 2. Fuera de alcance

- Encender el interruptor por defecto: es otra PR y requiere el «sí» del PO.
- Cambiar las formas del saque, `ServeKindLobMinAimY`, la calibración o las reglas.
- Una pausa antes del primer saque del partido. Ese saque sigue siendo globo automático, y sobre ese vuelo va la placa «vs» del ítem 4.
- Mover al rival a su lugar de recepción durante la pausa. Sigue moviéndose en `BeginPoint`.
- Elegir el cuadro de saque y cualquier cambio a los saques del rival.

## 3. Criterio de aceptación

Con el interruptor **encendido**:

1. En la pausa antes del saque del jugador, cuando termina `PointEndFreeze`, `Human.Position` es igual a `BoxPosition(context, Match.Box)` y el cuerpo queda quieto.
2. `WaitBetweenPoints` es 3,0 s antes del saque del jugador y 2,5 s antes del saque del rival.
3. Sin toque, el globo sale a los 3,0 s. Con un toque bajo la línea sale el drive en el mismo cuadro en que se suelta, como en `ZServeTests.On_ALowTouch…`.
4. Al empezar el punto, el estado es idéntico con colocación temprana y sin ella: pelota, posición del cuerpo, energía y paso de lanzamiento. Se verifica con un interruptor de test.
5. El aviso `serve-guide` solo es visible en esta pausa.
   - Su línea está a ±1 px de la proyección de cámara de `(x, ServeKindLobMinAimY, 0)`.
   - El largo dibujado de la línea es el tiempo restante dividido por 3,0, con un margen de un cuadro.
   - Las etiquetas quedan dentro del rectángulo proyectado del frontis y del área segura.
   - No bloquea el toque: `OverUi` da falso en el área del aviso.
6. En esta pausa, el aviso entre puntos no se superpone al rectángulo proyectado del frontis en ningún perfil de `ZDeviceLayoutTests`. El título y la causa se leen completos.
7. El tutorial tiene 6 pasos. El sexto es «Saque» y la métrica `tutorial-step` registra el paso 6.

Con el interruptor **apagado**:

8. Todo es idéntico a hoy:
   - la pausa es de 2,5 s y el cuerpo no se mueve durante ella;
   - no hay aviso de saque;
   - el aviso entre puntos sale en su lugar de siempre;
   - el tutorial tiene 5 pasos;
   - `ZServeTests` y `ZFirstRunTutorialTests` pasan sin editarse;
   - la calibración y `RallySim` no cambian, porque la pausa no existe en la simulación.

Además:

9. EditMode y PlayMode completos en verde en el Mac del PO, y `check-release` OK. AutoQA (PASS/0/1) es obligatorio: cambian la pausa y la capa sobre el frontis donde se toca.

## 4. Tests RED primero (Codex, TDD)

- **EditMode, `RallyParams`:**
  - el valor por defecto de `ServeKindHumanPauseSeconds` es 3;
  - toda proyección existente que excluye `ServeKind*` queda igual;
  - el hash completo nuevo se fija en el test y se declara.
- **PlayMode, `ZServeTests`:** los criterios 1 a 5 y 8.
  - Usar `Drive(...)` y los toques de `Touch(...)` que ya existen en ese archivo.
  - Para el 4, comparar el estado al empezar el punto con la colocación temprana encendida y apagada (interruptor de test).
- **PlayMode, `ZDeviceLayoutTests`:**
  - el criterio 6;
  - las etiquetas del aviso dentro del frontis en todos los perfiles.
- **PlayMode, `ZFirstRunTutorialTests`:** 6 pasos con el interruptor encendido y 5 con el interruptor apagado (criterios 7 y 8).
- **PlayMode, `ZUiLookTests`:** el aviso usa Rubik Bold Italic, con el mismo tratamiento sin caja que la nota del golpe.

## 5. Sin iPhone y con iPhone

- **Sin iPhone, en el Editor:** todo §3, con el interruptor encendido desde el test o desde el panel DEV.
- **Con iPhone, en la próxima instalación autorizada y con «Saque nuevo: sí» en el panel DEV:**
  - si 3 s alcanzan y no aburren;
  - si se entiende dónde tocar sin leer dos veces;
  - si la línea que se acorta se lee como tiempo;
  - si el aviso en la franja inferior se sigue leyendo;
  - cómo se ve el cuerpo apareciendo en el cuadro;
  - el paso 6 del tutorial.

  Es la primera vez que el PO juzgaría el saque nuevo en el teléfono: el informe del 20 nov lo deja como riesgo.

## 6. Dirección visual

Pasada de `/frontend-design`.

- **Concepto: la línea de decisión es la que se dibuja.** El umbral de 3,0 m que decide globo o drive ya existe en el código. Se pinta sobre el frontis como una línea más de la cancha, y esa misma línea es el reloj de la pausa. Un solo elemento dice dónde y cuánto tiempo queda.
- **Tokens:**
  - línea de 3 px, blanca al 85 %, con la sombra de `.sc-shot-note`, en el lenguaje de las líneas de la cancha pero sin el rojo `--line`, que en el frontis significa fuera o tin;
  - etiquetas en Rubik Bold Italic 20, blancas, sin caja y con `text-shadow: 0 2px 5px rgba(4,14,18,.85)`, la voz de la nota del golpe;
  - «Toca arriba: globo» arriba y a 6 px de la línea; «Toca abajo: drive» abajo y a 6 px.
- **Respuesta al toque**, opcional y declarada si se omite: mientras el dedo está apoyado, la etiqueta de la zona elegida pasa a `--dot` y la otra baja al 50 %. El amarillo es el color de quien saca, igual que el punto del marcador. Si la mira del gesto en curso no se puede leer sin tocar el gesto, esto se omite.

```
   ┌──────────── frontis ────────────┐
   │        Toca arriba: globo        │
   │   ━━━━━━━━━━━━━━━━━━━━━━━━━━━   │  ← 3,0 m; se acorta hacia el centro en 3 s
   │        Toca abajo: drive         │
   │ ─────── línea de saque ──────── │
   └─────────────────────────────────┘
        (cancha; TÚ quieto en el cuadro)
   ┌──── aviso de punto, en la franja inferior ────┐
```

- **Movimiento.** La línea entra con un fundido de 0,18 s y después solo se acorta. El cuerpo aparece de golpe en el cuadro mientras el aviso entra, de modo que el corte queda cubierto por ese movimiento.
- **Revisión contra lo genérico.** El primer impulso fue un número grande de cuenta regresiva («3, 2, 1») al centro y dos botones, «Globo» y «Drive». Se descartó:
  - el número suma un segundo foco;
  - los botones cambian el gesto, porque el saque se elige tocando el frontis y no un botón;
  - y los dos son lenguaje de *menú*, no de cancha.

  También se descartó pintar dos zonas rellenas de color sobre el frontis: tapan la pared que el jugador está apuntando.
