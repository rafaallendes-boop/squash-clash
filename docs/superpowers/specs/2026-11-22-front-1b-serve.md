# Front 1b — Saque nuevo: parado en el cuadro, con aviso de dónde tocar (RAL-177)

22 nov 2026 · Base `origin/main` `518f9c3`. Spec de Claude (arquitecto, modo product lead); revisión no independiente. **El incremento 1 no enciende «Saque nuevo».** Todo vive detrás del interruptor, que sigue apagado por defecto. El encendido es el último incremento (§7), en una PR aparte.

Pregunta resuelta el 22 nov: el PO aceptó sin recalibrar la desviación del Definidor (−5,1/−5,5 pp) y la baja del novato en la devolución (P+V de 97,5 % a 89,7 %). Datos en el [informe del 20 nov](../../reviews/2026-11-20-serve.md). Cita literal en §7.

## Plan de hoy (22 nov, build para el PO entre 17:00 y 18:00)

Codex implementa desde las ~12:30, con fusión y compuertas en verde a las ~16:00. El orden: tres PRs, una por incremento, una tras otra (un solo Unity a la vez).

1. Front 1, «¡Temprano!» ([spec](2026-11-22-front-1-temprano.md)). Es lo más corto y va primero.
2. Front 1b, incremento 1: §1 a §6 de esta spec, con el interruptor apagado.
3. Front 1b, incremento 2: el encendido por defecto (§7).

Lo que queda para después está en §8. Ninguna de esas piezas bloquea la build de hoy.

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
4. **El frontis queda libre.** En esta pausa, el aviso entre puntos (`rally-banner`) no se muestra.
   - Esta pausa solo ocurre después de un punto que ganó el jugador, porque saca quien gana el punto (`MatchScore.cs:29-31`). El resultado sigue a la vista en el pulso del marcador y en la línea de estado del encabezado.
   - Bajar el aviso a la franja inferior queda para después (§8).
5. **Onboarding.** El tutorial suma un sexto paso, «Saque», con su dibujo (el frontis con la línea de 3,0 m, globo arriba y drive abajo). Texto propuesto: «Cuando sacas tú, toca el frontis: arriba de la línea sale globo, abajo sale drive. Si no tocas, sale globo.»
   - El paso solo se incluye si `rally.NewServeNextPoint` es verdadero cuando se abre el tutorial. **Con el interruptor apagado el tutorial queda igual, con 5 pasos.**
   - Hay tensión con la regla de la semana («ningún ítem suma un paso antes de jugar»). El paso aparecerá recién con el incremento 2. Se acepta porque es parte del alcance pedido por el PO para el saque («el onboarding suma un paso de saque», SC-PM, 22 nov).

## 2. Fuera de alcance

- Encender el interruptor por defecto: no va en este incremento, sino en el incremento 2 (§7).
- Recalibrar los estilos o tocar umbrales, en cualquiera de los dos incrementos.
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
6. En esta pausa, `rally-banner` está oculto. La línea de estado del encabezado muestra el resultado del punto, como hoy. En las demás pausas, el aviso sale igual que hoy.
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
- **PlayMode, `ZServeTests`:** los criterios 1 a 6 y 8.
  - Usar `Drive(...)` y los toques de `Touch(...)` que ya existen en ese archivo.
  - Para el 4, comparar el estado al empezar el punto con la colocación temprana encendida y apagada (interruptor de test).
- **PlayMode, `ZDeviceLayoutTests`:** las etiquetas del aviso quedan dentro del rectángulo del frontis y del área segura en todos los perfiles.
- **PlayMode, `ZFirstRunTutorialTests`:** 6 pasos con el interruptor encendido y 5 con el interruptor apagado (criterios 7 y 8).
- **PlayMode, `ZUiLookTests`:** el aviso usa Rubik Bold Italic, con el mismo tratamiento sin caja que la nota del golpe.

## 5. Sin iPhone y con iPhone

- **Sin iPhone, en el Editor:** todo §3, con el interruptor encendido desde el test o desde el panel DEV.
- **Con iPhone, en la próxima instalación autorizada y con «Saque nuevo: sí» en el panel DEV:**
  - si 3 s alcanzan y no aburren;
  - si se entiende dónde tocar sin leer dos veces;
  - si la línea que se acorta se lee como tiempo;
  - si se echa de menos el aviso «PUNTO PARA TI» en esa pausa;
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
- **Respuesta al toque:** queda para después (§8).

```
   ┌──────────── frontis ────────────┐
   │        Toca arriba: globo        │
   │   ━━━━━━━━━━━━━━━━━━━━━━━━━━━   │  ← 3,0 m; se acorta hacia el centro en 3 s
   │        Toca abajo: drive         │
   │ ─────── línea de saque ──────── │
   └─────────────────────────────────┘
        (cancha; TÚ quieto en el cuadro)
```

- **Movimiento.** La línea entra con un fundido de 0,18 s y después solo se acorta. El cuerpo aparece de golpe en el cuadro mientras el aviso entra, de modo que el corte queda cubierto por ese movimiento.
- **Revisión contra lo genérico.** El primer impulso fue un número grande de cuenta regresiva («3, 2, 1») al centro y dos botones, «Globo» y «Drive». Se descartó:
  - el número suma un segundo foco;
  - los botones cambian el gesto, porque el saque se elige tocando el frontis y no un botón;
  - y los dos son lenguaje de *menú*, no de cancha.

  También se descartó pintar dos zonas rellenas de color sobre el frontis: tapan la pared que el jugador está apuntando.

## 7. Incrementos

Hay dos PRs, en este orden. Cada una tiene su rama y necesita el «sí» del PO para fusionarse.

**Incremento 1: el cuadro de saque, el aviso y el onboarding.** Es todo lo de §1 a §6, con el interruptor apagado por defecto.

**Incremento 2 (el último): encender el saque nuevo por defecto.** Empieza después de fusionar el incremento 1.

- **Cambio.** `RallyParams.ServeKindOn` pasa de 0 a 1. Es un valor por defecto de `RallyParams`; `DefaultPreset.json` no cambia (informe del 20 nov). El interruptor DEV «Saque nuevo» sigue existiendo y arranca en «sí».
- **Sin recalibración.** No se tocan pesos, umbrales, `ServeKindLobMinAimY`, las formas del saque ni la calibración de los estilos. La desviación ya está medida y aceptada.
- **Registro.** En [docs/reviews/2026-11-18-po-approvals.md](../../reviews/2026-11-18-po-approvals.md) va una entrada del 22 nov con la cita literal del PO, tomada del chat de SC-Lead: «Está bien la decisión. Igual hay que probar después con los usuarios reales. A si que todo bien.» La entrada dice qué cubre: el saque nuevo encendido por defecto sin recalibrar, con el Definidor en −5,1/−5,5 pp en Medio/Difícil y el novato devolviendo el saque del rival con P+V de 97,5 % a 89,7 %. Anota también que **se revisa con los primeros usuarios reales de TestFlight**, como observación abierta. Lo mismo se resume en AGENTS §6 y §7.
- **Hashes.** El hash completo de RallyParams cambia y se declara en el informe. Toda proyección que excluye `ServeKind*` queda igual.
- **Tests.**
  - Las aserciones de «apagado por defecto» pasan a «encendido por defecto» por esta aprobación. Se declaran una por una, como en #86: por ejemplo `ZServeTests.cs:43` (`Off_TheMatchServeIsTodaysAutomaticServe`) y `:82`.
  - Las pruebas del saque de hoy siguen existiendo con el interruptor apagado en el test.
  - Primero va el RED por aserción: una prueba nueva verifica que `new RallyParams().ServeKindOn == 1`.
- **Compuertas.**
  - EditMode y PlayMode completos.
  - AutoQA V/T3/volea PASS/0/1. AutoQA usa la práctica y no los partidos, así que debe salir idéntico.
  - `check-release` OK.
  - Una corrida de confirmación por estilo con el saque encendido, para mostrar que reproduce `confirm-new-serve01`: es la misma medición, no una recalibración. **No bloquea la build de hoy.** Si no alcanza antes de las 16:00, se corre después de la fusión y se informa. El encendido no cambia ningún parámetro respecto de la corrida que ya se midió.
- **iPhone.** Lo que dice §5, pero sin tocar el panel DEV. Se instala solo con una autorización propia.

## 8. Después (no entra hoy)

- **El aviso entre puntos en la franja inferior.** En la pausa del saque del jugador, el aviso se mostraría bajo la cancha en lugar de ocultarse. Hay que verificar que quepa en cada perfil de `ZDeviceLayoutTests`; en el SE la franja mide unos 112 pt y el aviso unos 117 pt.
- **Respuesta al toque.** Mientras el dedo está apoyado, la etiqueta de la zona elegida pasa a `--dot` y la otra baja al 50 %. El amarillo es el color de quien saca, igual que el punto del marcador.
- **Front 3 (tarjeta), front 2 (pistas) y front 4 (pantalla «vs»):** van en #91 y #93.
