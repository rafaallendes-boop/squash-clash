# Front 2 — Pistas de los primeros partidos (saque, volea y dos paredes)

22 nov 2026, revisión 4 del 23 nov, con las decisiones de producto de SC-Lead (§7). Base `origin/main` `230b758`, que ya trae el saque nuevo encendido por defecto (#100). Spec de Claude como arquitecto. Roles, fusión e instalación según AGENTS §2, §7 y §8 de esa base.

Hay como máximo **4 pistas**, y cada una se ve una sola vez por instalación. Viajan en lo que ya se ve entre puntos, el aviso o la guía de saque, y no agregan pasos ni tiempo antes de jugar ni entre puntos. La prioridad es el saque nuevo: es lo que el PO no entendió al probarlo, según el encargo de SC-Lead.

**Plan de implementación.** Una rama y una PR propias, en dos tramos y en este orden:

1. **Datos.** Lo implementa Codex y lo revisa Claude. Incluye:
   - `HintPicker`;
   - la ventana y la persistencia;
   - las lecturas `LastServeChosen` y `LastStrokeBoast`, esta última si no llegó antes con la spec 3;
   - las métricas;
   - los criterios 1 a 9, 11 y 13.
2. **Visual.** Lo implementa Claude con `/frontend-design` y Sonnet, y lo revisa Codex. Incluye la fila del aviso, la pista de la guía de saque, los estilos, los textos, el botón DEV y los criterios 10 y 12.

**Bloque separable: H0 en el aviso** (decisión 3 de SC-Lead, §7). Comprende las filas y los criterios marcados **[H0-aviso]**, y entra **completo** en esta PR. Si meterlo pone en riesgo la build de las 17, esta PR sale con el resto del alcance y el bloque va completo en la PR siguiente, con sus tests RED, **nunca a la mitad**. Sin el bloque, H0 sale solo en la guía y todo lo demás de esta spec vale igual.

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
- **Respuesta para quien abandona pronto.** El siguiente partido desde el menú sigue en la ventana. Las pistas que no llegó a ver siguen sin ver y se arman si su disparador vuelve a ocurrir (§1.3).
- **Quien abandona sin ganar un punto.** No llega a ninguna `HumanServePause`. Por la decisión 3 de SC-Lead, H0 sale también en el aviso de la pausa ordinaria antes del primer saque del rival **[H0-aviso]**, así que la ve quien pierde al menos un punto.
  - Sigue sin verla quien abandona en 0–0 antes de cualquier pausa: es una limitación conocida.
- **Límite.** Son 4 pistas por instalación, cada una una sola vez. La ventana solo puede cerrarse antes.
- **Sin tope de intentos abandonados**, por la decisión 2 de SC-Lead. Queda como limitación conocida y se revisa con las métricas de las pistas (§1.5).

### 1.2 Pistas

| Id | Disparador (hechos del juego, sin umbrales nuevos) | Dónde se ve | Texto | Clave |
|---|---|---|---|---|
| H0 Saque, en la guía | Empieza una `HumanServePause` en un partido de la ventana. | Guía de saque: `serve-guide-hint` | «Sacas tú: toca el frontis antes de que se acabe la línea. Si no tocas, sale globo.» | `sc.hints.serve` |
| H0 Saque, en el aviso **[H0-aviso]** | Empieza la pausa ordinaria antes del **primer saque del rival** en un partido de la ventana: `PointOver`, partido sin terminar, `Match.Server == Rival` y el rival todavía no sacó en este partido. Lo lleva el adaptador desde `RallyStarted`, sin estado nuevo en el juego. | Aviso: `rally-banner-hint` | «Cuando saques tú: toca el frontis antes de que se acabe la línea. Si no tocas, sale globo.» | `sc.hints.serve`, la misma |
| H1 Volea | En una recepción del jugador, `VolleyShown` fue verdadero en algún cuadro y la recepción terminó sin volea aplicada: hubo un golpe con `LastOpportunity != Volley`, un fallo o ningún golpe. | Aviso: `rally-banner-hint` | «Anillo ámbar: toca y volea antes del bote.» | `sc.hints.volley` |
| H2 Dos paredes | `ShotResolved` de un golpe aplicado del jugador (`Perfect` o `Valid`), con `SideAimOpen` verdadero y `LastStrokeBoast` falso, sin ningún dos paredes en el partido. | Aviso | «Toca la pared lateral para jugar un dos paredes.» | `sc.hints.boast` |
| H3 Cruce | `ShotResolved` del primer golpe del partido con `LastStrokeBoast` verdadero. | Aviso | «Lateral cerca de ti: la pelota cruza al otro lado.» | `sc.hints.boastCross` |

- **H0 es una sola pista con dos formas**, con la misma clave. Cuenta como vista una sola vez: la primera forma que llega a su oráculo deja `sc.hints.serve` en 1, y después no sale ninguna de las dos.
- **Textos.** Van los de esta spec (decisión 4 de SC-Lead). Salen del tutorial aprobado (`FirstRunFlow.cs:52-54`), acortados. Rafael los ajusta después del playtest, sin tocar la lógica.
- **H2 va sin condición de profundidad** (decisión 5, §7).
- **H0 y el tutorial.** H0 enseña lo que el sexto paso ya dice, pero en el momento en que hace falta. Haber visto el tutorial no retira H0.

### 1.3 Estados, turnos y convivencia con la guía de saque

**Estados de cada pista.** Son cuatro. Dos se guardan y dos viven solo en memoria:

| Estado | Dónde vive | Cómo se llega | Depende de que se vea |
|---|---|---|---|
| **Sin ver** | ausencia de la clave | estado inicial | — |
| **Armada** | solo en memoria, dentro del partido | su disparador ocurre en un partido de la ventana | no |
| **Vista** | clave = 1 | la pista estuvo visible según la regla de abajo | **sí, y es la única transición que depende** |
| **Retirada** | clave = 2 | solo H2, ver abajo | no |

- **Armar.** Un disparador arma su pista solo si está sin ver. Las vistas y las retiradas no se vuelven a armar.
- **Lo armado no se guarda.** Al terminar, abandonar o cerrar la app, toda pista armada vuelve a sin ver.
  - **No se restaura** en el partido siguiente ni tras un reinicio de la escena o de la app.
  - Solo vuelve a armarse si **su disparador ocurre de nuevo** en un partido de la ventana.
  - Conservar la ventana abierta (§1.1) no es conservar una pista armada.
- **Retiro de H2.** Si el jugador hace su primer dos paredes del partido mientras H2 está sin ver o armada, H2 pasa a retirada: clave = 2, línea `hint` con `action=retired` y `PlayerPrefs.Save()`.
  - Ocurre en el cuadro del `ShotResolved` de ese golpe, en plena fase de juego.
  - **No requiere que nada sea visible.** Ese mismo golpe arma H3.
- **Cierre de la ventana** (`sc.hints.window = 1`, §1.1). Tampoco depende de la visibilidad: se escribe al llegar a `MatchOver` o cuando las 4 pistas quedan vistas o retiradas.

**Pausa ordinaria.** Es `PointOver` sin `HumanServePause` y sin `MatchOver`, con el aviso visible.

- Muestra como máximo una pista armada, en este orden de prioridad: H0 en el aviso **[H0-aviso]**, H1, H2 y H3.
- Las demás siguen armadas hasta la pausa ordinaria siguiente del mismo partido.

**Pausa del saque del jugador** (`HumanServePause`, 3 s).

- Solo puede mostrar H0, dentro de la guía.
- H1 a H3 siguen armadas: el aviso está oculto, así que ahí no se muestran ni pasan a vistas.
- La pausa no se alarga para hacer caber la pista.

**Paso a vista.** Es la única transición que exige visibilidad. Se escribe la clave en 1, la línea `hint` con `action=seen` y `PlayerPrefs.Save()` en ese mismo cuadro. Hay dos oráculos distintos, uno por contenedor.

- **En el aviso (H1 a H3, y H0 en el aviso), el oráculo es `BannerOpacity == 1`.**
  - En ese cuadro tienen que estar visibles el aviso (`rally-banner`) y la fila de la pista, con un layout resuelto de alto mayor que 0.
  - La opacidad la sigue acumulando `RallyHud.Tick` con el `dt` recortado a 0,05 s (`RallyHud.cs:290,316-323`), sin cambios en la animación ni en sus pruebas.
  - **No se usa tiempo real para el aviso.** Tras un cuadro largo pueden haber pasado 0,18 s reales con la opacidad todavía por debajo de 1, y entonces la pista **no** está vista.
- **En la guía (H0), el oráculo es el tiempo real visible.** La guía no tiene animación de entrada.
  - Se suman las diferencias de `FirstMatchHints.Now` entre cuadros consecutivos en los que `serve-guide` y `serve-guide-hint` están visibles, con un layout de alto mayor que 0, y el reloj del juego no está en pausa.
  - Se marca vista al llegar a 0,18 s. Es el mismo `BannerInSeconds`; no hay un número nuevo.
  - Un cuadro largo cuenta entero, porque es tiempo real, y no se suma el `dt` recortado.
  - `FirstMatchHints.Now` es real, sin escala y reemplazable, como `PointEndFreeze.Now`.

**Si la pausa se termina antes de su oráculo**, sea por «Seguir», por fin del plazo o por un toque de saque, la pista no queda vista y sigue armada:

- H1 a H3, hasta la próxima pausa ordinaria **del mismo partido**. Si el partido termina antes, vuelven a sin ver.
- H0 en la guía, hasta el fin de la pausa. Como su disparador es la propia `HumanServePause`, en la próxima vuelve a armarse, en este partido o en uno siguiente de la ventana.
- H0 en el aviso **[H0-aviso]**, hasta el fin de esa pausa. Su disparador ocurre una sola vez por partido, así que después solo puede salir en la guía, en la próxima `HumanServePause`, o en el aviso de un partido siguiente de la ventana.

**Fuera de `PointOver`** no se muestra ninguna pista, ni en `MatchOver`, donde está la tarjeta.

**Convivencia de H0 con la nota del golpe.** La nota `shot-note` (`ShotNoteView`) conserva el texto del último golpe, por ejemplo «¡Temprano!», y sigue visible durante la pausa del saque si `timing.showNotes` está encendido (`ShotController.cs:441`). No hay exclusión por fase.

- La nota no cambia: ni su texto, ni su visibilidad, ni su pop, ni su posición.
- **H0 se acomoda a la nota.**
  - Con la nota visible, `serve-guide-hint` va justo encima de ella, con 8 px de separación, sin cruzarse con ella ni con la guía.
  - Con la nota oculta o vacía, H0 se centra en la franja bajo la cancha, igual que la nota (`ShotNoteView.BandBottom`).
  - Si la franja no alcanza, H0 puede subir sobre el piso cercano de la cancha, pero nunca sobre el frontis ni fuera del área segura.

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
  - `action`: `seen` o `retired`;
  - `where`: `banner` o `guide`, en qué contenedor se vio. Va vacío con `retired`.
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

- Cambiar el primer saque del partido: el 0–0 sigue con el globo automático (decisión 1 de SC-Lead). Tampoco se cambian las pausas, `ServeKindLobMinAimY` o cualquier parámetro del juego. No se recalibra nada.
- Pistas de cansancio, de timing u otras. Un segundo pase de pistas, o volver a verlas desde el menú.
- Cambiar el tutorial o agregarle pasos.
- **Condición de profundidad para H2.** Sería que H2 salga solo cuando el jugador golpeó desde el cuarto trasero (`z ≥ X m`).
  - Crea un umbral nuevo, así que es decisión de Rafael (AGENTS §7) y **queda pendiente**; SC-Lead se la planteará con datos.
  - Mientras tanto H2 va **sin** condición de profundidad y no se agrega ningún umbral.

## 3. Criterio de aceptación

1. **H0 en la guía.** En un partido de la ventana en que el jugador gana el primer punto, con H0 sin ver, `serve-guide-hint` se ve con el texto de H0 en la primera `HumanServePause`.
   - Cuando la guía suma 0,18 s visibles en el reloj de prueba, `sc.hints.serve` queda en 1 y se escribe una sola línea `hint` con `id=serve`, `action=seen` y `where=guide`. Esto incluye el caso de un solo cuadro de 0,18 s.
   - Un toque arriba y otro abajo dan `serve-choice` con `chosen=true` y `Lob` o `Drive`. Sin toque, `chosen=false` y `Lob`.
   - La pausa sigue durando `ServeKindHumanPauseSeconds`.
2. **H1 en el aviso.** Después de un punto con el anillo ámbar visto y sin volea aplicada, la pausa ordinaria siguiente muestra H1 en `rally-banner-hint`.
   - `sc.hints.volley` pasa a 1 en el cuadro en que `BannerOpacity` llega a 1, con el aviso y la fila visibles, y no antes.
3. **H2 y H3: transiciones sin mostrar nada en juego.**
   - Un golpe aplicado al frontis, sin dos paredes antes, deja H2 armada. Se verifica en memoria (`HintPicker`) sin escribir ninguna clave.
   - El primer dos paredes aplicado deja H3 armada.
   - Un dos paredes que cae al drive de respaldo (`FellBack`) no arma H3 y cuenta como golpe al frontis.
   - **Retiro.** Con H2 sin ver o armada, un dos paredes aplicado deja `sc.hints.boast` en 2, escribe `hint` con `action=retired` y deja H3 armada. Todo ocurre en el cuadro del `ShotResolved`, con `Phase != PointOver`, y en ese cuadro `rally-banner-hint` y `serve-guide-hint` siguen ocultos, como pide el criterio 8.
4. **Prioridad, arrastre y visibilidad.**
   - Con H1 y H2 armadas en el mismo punto, sale H1, y H2 sale en la pausa ordinaria siguiente del mismo partido.
   - **[H0-aviso]** El orden de prioridad completo en una pausa ordinaria es H0 en el aviso, H1, H2 y H3.
   - Una pista armada antes de una `HumanServePause` no se ve ni pasa a vista ahí, y sale en la pausa ordinaria siguiente.
   - **Ninguna clave de pista pasa a 1 (vista) mientras su contenedor está oculto.** El retiro (clave 2) y el cierre de la ventana (`sc.hints.window`) no dependen de la visibilidad (§1.3).
5. **La pausa se acaba antes del oráculo.**
   - **Aviso, con un cuadro largo.** El reloj de prueba avanza 0,18 s reales en un cuadro, y `Update` recorta el `dt` a 0,05 s, así que `BannerOpacity` queda por debajo de 1. Si en ese momento se toca «Seguir», `sc.hints.volley` no cambia y no se escribe ninguna línea `hint`. H1 sigue armada y sale en la pausa ordinaria siguiente del mismo partido.
   - **Aviso, con la opacidad completa.** Con `BannerOpacity == 1` antes de «Seguir», la pista queda vista.
   - **Guía.** Un toque de saque o «Seguir» con menos de 0,18 s visibles no marca H0, y H0 vuelve en la `HumanServePause` siguiente. Con 0,18 s o más, sí la marca.
   - La animación del aviso y sus pruebas existentes no cambian.
6. **Abandono temprano, con y sin repetición del disparador.**
   - (a) Se inicia desde el menú y se abandona en 0–0 sin ninguna pausa, primero con `StopMode` y en otra corrida con `EndSession`. No existe ninguna clave `sc.hints.*`, y el partido siguiente desde el menú está en la ventana.
   - (b) Se abandona con H1 armada y sin ver. En el partido siguiente, H1 **no** sale en la primera pausa ordinaria si su disparador no se repitió. Cuando se repite, H1 se arma y sale en la pausa ordinaria siguiente.
   - (c) Lo mismo que en (b), pero con un reinicio de la escena entre los dos partidos y las mismas `PlayerPrefs`. Lo armado no se restaura.
   - (d) H0 vuelve a armarse en la primera `HumanServePause` del partido siguiente, porque su disparador es esa pausa.
   - (e) **[H0-aviso]** Se inicia, el rival gana el primer punto, H0 se ve en el aviso y se abandona en 0–1. `sc.hints.serve` vale 1, y en el partido siguiente H0 no sale en ninguna de sus formas.
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
10. **Layout y convivencia con la nota.**
    - Con la pista más larga de H1 a H3, el aviso cabe en el área segura en todos los perfiles de `ZDeviceLayoutTests`, con el texto en dos líneas como máximo y sin elipsis.
    - **[H0-aviso]** Con H0 en el aviso, el aviso cabe en el área segura en todos los perfiles, con el texto en tres líneas como máximo, igual que su forma en la guía, y sin elipsis.
    - `serve-guide-hint` cabe en el área segura, en tres líneas como máximo y sin elipsis, y no se cruza con la línea ni con los rótulos de la guía.
    - **Con una nota previa visible.** Por ejemplo «¡Temprano!», con `timing.showNotes` encendido. `serve-guide-hint` y `shot-note` no se cruzan en ningún perfil. La nota conserva su texto, su `display` y su posición iguales a los de la corrida con las pistas apagadas.
    - Con `timing.showNotes` apagado, H0 ocupa la franja bajo la cancha.
11. **Persistencia.** Al reiniciar la escena con las mismas `PlayerPrefs`:
    - ninguna pista vista o retirada vuelve;
    - la ventana conserva su estado;
    - lo armado no se restaura, como en el criterio 6c.

    El botón DEV borra solo `FirstMatchHints.AllKeys()`.
12. **Puertas.**
    - EditMode y PlayMode completos en verde en el Mac del PO.
    - AutoQA `scripts/autoqa.sh mvp` PASS/0/nivel 1, obligatorio porque los disparadores leen la ruta de resolución del golpe. Hay que mostrar que no cambia ninguna decisión.
    - `check-release` OK.
13. **[H0-aviso] H0 en el aviso.** En un partido de la ventana en que el rival gana el primer punto, con H0 sin ver:
    - la pausa ordinaria antes del primer saque del rival muestra en `rally-banner-hint` el texto «Cuando saques tú: …» de §1.2;
    - con `BannerOpacity == 1`, `sc.hints.serve` queda en 1 y se escribe una sola línea `hint` con `id=serve`, `action=seen` y `where=banner`;
    - en la `HumanServePause` siguiente, `serve-guide-hint` **no** aparece, porque la clave es compartida, y la guía de saque sigue igual que hoy;
    - si se toca «Seguir» antes de opacidad 1, H0 no queda vista y sale en la guía en la próxima `HumanServePause`. En este partido no vuelve a salir en el aviso, porque el rival ya sacó;
    - si ya estaba vista en la guía, la pausa antes del primer saque del rival no la muestra;
    - con H1 también armada en esa pausa, sale H0 y H1 espera a la pausa ordinaria siguiente.

## 4. Tests RED primero (TDD) y reglas de prueba

**Tests nuevos.**

- **EditMode, `Rally/HintPickerTests`.** Cubre la máquina de estados de §1.3 sin Unity:
  - la ventana;
  - cada disparador;
  - la prioridad y una pista por pausa;
  - el arrastre dentro del partido y el descarte al terminarlo;
  - que no se rearma sin repetir el disparador;
  - la `HumanServePause` con H0 sola;
  - **[H0-aviso]** H0 con dos formas y una sola clave, su disparador una vez por partido y su prioridad en la pausa ordinaria;
  - el retiro de H2 sin visibilidad;
  - los dos oráculos de vista;
  - que una pista vista o retirada no vuelve;
  - que nunca hay más de 4.
- **EditMode, `Diagnostics/UserMetricsTests`.** `hint` (con `seen` y `retired`, y `where`) y `serve-choice` con sus campos, emitidos una sola vez, y las líneas actuales idénticas.
- **PlayMode, `ZFirstMatchHintsTests`**, sobre `ZFirstRunFixture`.
  - Cubre los criterios 1 a 9, 11 y 13, y la parte de la nota del criterio 10. Los casos marcados **[H0-aviso]** van con su bloque separable (plan, al inicio).
  - Las escenas de volea v2 (`ZVolleyV2Tests`), dos paredes (`ZBoastTests`) y saque (`ZServeTests`) sirven de base.
  - El criterio 8 se comprueba con piloto automático y la ventana forzada por un punto de prueba interno, que solo usan los tests nuevos.
  - El criterio 5 reemplaza `FirstMatchHints.Now` y avanza el tiempo en un solo cuadro largo.
- **PlayMode, `ZBoastTests`.** Los casos de `LastStrokeBoast` de §1.6, si esta PR introduce la marca.
- **PlayMode, `ZDeviceLayoutTests`.** El criterio 10.
- **PlayMode, `ZUiLookTests`.** El aro de H1 tiene el color de `ShotController.VolleyTint`, y las pistas usan Rubik Regular.

**Reglas de prueba, fijadas antes de implementar.**

- **Los tests existentes no se editan** y tienen que pasar con las pistas **encendidas**, que es el valor por defecto.
- **Un solo cambio de fixture, declarado de antemano.** `ZFirstRunFixture.ClearPrefs` (`ZFirstRunFixture.cs:78`) también borra `FirstMatchHints.AllKeys()`, igual que ya hace con `PersonalRecords.AllKeys()`.
- **Si un test existente falla con las pistas encendidas**, es un defecto. Se registra y se detiene, según AGENTS §2. No se apaga `FirstMatchHints.Enabled`, no se cambia una aserción, una pausa ni un texto para pasar, y no se agrega `SetNewServe(false)` (AGENTS §3).
- **Las pistas apagadas solo se usan en dos lugares**, los dos declarados de antemano y ambos en tests nuevos:
  1. `Off_NothingIsWrittenOrShown`: apagadas, no se escribe ni se muestra nada;
  2. la corrida de referencia del criterio 9, `NoAddedTime_OnMatchesOff`, que compara encendidas con apagadas, y la referencia de la nota del criterio 10.

  Fuera de esos dos, ningún test apaga las pistas.

## 5. Sin iPhone y con iPhone

**Se implementa en su propia PR.**

**Sin iPhone, en el Editor:** todo §3. Eso incluye la máquina de estados, la persistencia, la integración, el layout en los perfiles de dispositivo y las puertas.

**Con iPhone, en la ventana que SC-PM coordina con Rafael (AGENTS §8).** Dentro de esa ventana se hacen el respaldo y el cotejo de presets y métricas que pide §8, como en cualquier instalación. Son dos recorridos separados, en este orden.

**Recorrido A: abandono temprano**, en un intento nuevo en 0–0.

1. En el panel DEV, «Reiniciar pistas». No se borra la app ni todas las `PlayerPrefs`, porque hay presets que preservar.
2. Se toca «Jugar» y se abandona desde la pausa en 0–0, antes de cualquier pausa entre puntos.
3. Resultado observable: no apareció ninguna pista y las métricas no traen líneas `hint`. El partido siguiente sigue en la ventana.

**Recorrido B: pistas en juego.** Es un partido nuevo desde el menú, sin volver a reiniciar las pistas.

1. Se juega. H0 sale una sola vez, en la forma que llegue primero.
   - **[H0-aviso]** Si el rival gana el primer punto, H0 sale en el aviso de esa pausa. Resultado observable: se lee en la pausa real de 2,5 s y la persona entiende dónde va a tocar cuando saque. Es el problema del playtest de Rafael.
   - Si el jugador gana el primer punto, H0 sale en la guía. Resultado observable: se lee en los 3 s, no tapa el frontis y no se cruza con la nota del golpe anterior.
   - Las métricas muestran una sola línea `hint` con `id=serve`, con `where=banner` o `where=guide`.
2. En la primera pausa del saque propio, se toca arriba o abajo de la línea. Resultado observable: sale el saque elegido y `serve-choice` trae `chosen=true` con el `kind` tocado.
3. Se sigue jugando para ver H1 a H3 cuando aparezcan. Resultado observable: cada pista se lee en la pausa real de 2,5 s, y el aro ámbar del aviso se reconoce como el anillo de la cancha.

**Al cerrar la ventana**, las métricas copiadas traen las líneas `hint` y `serve-choice` del recorrido B y ninguna del recorrido A.

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
  - **[H0-aviso]** H0 en el aviso lleva el mismo trazo blanco de 14 × 3 px que su forma en la guía, la línea de la guía en chico. Así, cuando la guía aparezca, se reconoce la misma línea.

**La guía de saque, para H0.** El frontis queda libre, porque es donde el jugador tiene que tocar.

- `serve-guide-hint` va en la franja bajo la cancha, la de `ShotNoteView.CourtBottomEdge` (#88). Esa franja puede tener todavía la nota del golpe anterior, así que H0 se apila sobre ella con 8 px de separación y no la mueve ni la oculta (§1.3).
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
            ¡Temprano!            ← nota del golpe anterior, sin cambios
```

**Revisión contra lo genérico.** Se descartó un *coach mark* con flecha y botón «Entendido»: suma un paso, tapa la cancha y en la pausa del saque taparía justo el frontis. Se descartó también el ícono de bombilla o «i». Cada miniatura usa la misma forma que el jugador ve en la cancha.

## 7. Decisiones de producto

Las respondió SC-Lead (product lead) el 23 nov, con los criterios de producto que Rafael le delegó. Texto literal:

1. **Primer saque del partido.** «No. El 0–0 sigue con el globo automático. Hoy no cambiamos el juego.» Efecto: queda fuera de alcance (§2).
2. **Tope de intentos abandonados.** «Sin tope por ahora. Queda como limitación conocida y lo vemos con las métricas de las pistas.» Efecto: §1.1. Las líneas `hint` y `serve-choice` (§1.4, §1.5) son las que se revisan.
3. **Adelantar H0.** «Sí, se adelanta. H0 sale también en la pausa ordinaria antes del primer saque del rival, en modo aviso («Cuando saques tú: toca el frontis antes de que se acabe la línea…»). Así la ve quien abandona sin ganar un punto. Viene directo del playtest de Rafael: no entendió dónde tocar. Cuenta como vista una sola vez, la primera que salga. Si meterla pone en riesgo la build de las 17, entra el alcance actual y esto va en la PR siguiente, nunca a la mitad.»
   - Efecto: H0 en el aviso, en §1.2, §1.3, el criterio 13 y lo marcado **[H0-aviso]**.
   - Es un bloque separable que entra completo o va completo en la PR siguiente (plan, al inicio).
   - El texto sigue el de la forma en la guía.
4. **Textos.** «Van los textos de la spec. Rafael los ajusta después del playtest.» Efecto: §1.2.
5. **Profundidad para H2.** «Es de Rafael. H2 va sin condición de profundidad y se lo planteo después con datos.»
   - Efecto: H2 va **sin** condición de profundidad y no se agrega ningún umbral (§2).
   - La condición sigue siendo una **decisión pendiente del PO**, porque crea un umbral nuevo (AGENTS §7).

**Limitaciones conocidas que quedan:**

- quien abandona en 0–0 antes de cualquier pausa no ve ninguna pista;
- la ventana no tiene tope de intentos.
