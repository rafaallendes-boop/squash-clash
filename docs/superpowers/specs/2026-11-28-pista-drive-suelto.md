# Pista «tu drive quedó suelto»

28 nov 2026. Base `origin/main` `99b4306` (#123). Encargo `set24-pista-drive-suelto` de SC-PM. La idea es del Game Designer y la eligió SC-Lead. **Esta fase es solo spec**: no hay implementación ni PR, y no se corrieron Unity ni pruebas. Adoptar la mejora es decisión de Rafael (pregunta 1). Claude escribe la spec como arquitecto (AGENTS §2).

Cita de SC-Lead: «Cuando pierdes un punto por un drive tuyo que botó corto o lejos de la lateral, sale una línea breve entre puntos que lo explica, en español neutro, con tú y terminología real de squash. No cambia la física ni la IA.»

Las marcas **PARÁMETRO A DECIDIR POR RAFAEL (Pn)** son propuestas, no hechos aprobados. Todas vuelven en la §11, cada una como pregunta.

## 1. Contexto: lo que ya existe en `99b4306`

**El aviso entre puntos** (`Runtime/Rally/RallyHud.cs`):
- `rally-banner` / `rally-banner-card` se arma una vez por punto terminado y se ve solo en `PointOver`, salvo en la pausa del saque del jugador (`HumanServePause`). En esa pausa se muestra la guía de saque (`:538-555`).
- Título, `rally-banner-cause` con `CauseLine` (`:405-409`, texto de `RallyText.Cause`, `Core/Rally/RallyText.cs:20-32`) y «Saca …».
- Entra en `BannerInSeconds` 0,18 s (`:58`). `BannerOpacity` (`:74`) dice cuánto entró.
- **Fila de pistas** `rally-banner-hint` (icono `rally-banner-hint-icon` y texto `rally-banner-hint-text`, `:145-147`). Hoy solo la usan las pistas de primeros partidos: `RefreshHints` y `SetHintRow` (`:290-308`) la llenan con `FirstMatchHints.Text(HintId, HintWhere)`.

**Pistas de primeros partidos** (spec `2026-11-22-front-2-first-match-hints.md`):
- `Core/Rally/HintPicker.cs`: clase pura con 4 pistas (`HintId` Serve, Volley, Boast, BoastCross), estados persistidos y una pista por pausa. Prioridad H0 > H1 > H2 > H3 (`:13`, `Choose` `:96-112`).
- `Runtime/Menu/FirstMatchHints.cs`: el adaptador. `Enabled` (`:20`), `ForceWindowForTests` (`:24`), textos (`:39-51`). Excluye el piloto automático (`BeginMatch`, `:108`). Elige la pista **una vez al empezar la pausa**.
- `Core/Rally/HintOracle.cs:9-10`: `BannerSeen(opacity, bannerVisible, rowVisible, rowHeight)` es el oráculo de «vista» en el aviso.
- Pruebas: `Tests/EditMode/Rally/HintPickerTests.cs`, `Tests/PlayMode/ZFirstMatchHintsTests.cs` y `ZFirstMatchHintsLayoutTests.cs`.

**El árbitro simple** (spec `2026-11-26-arbitro-simple.md`) **no comparte el aviso**. Tiene su propia capa `rally-referee` sobre el aviso, en el hueco bajo la cabecera, controlada por `RefereeOn`, `RefreshReferee` y `RefereeCall` (`RallyHud.cs:21,454-460`; `Core/Rally/RefereeCall.cs`).

**Cómo se atribuye el punto** (`Core/Rally/PointRules.cs`):
- `PointEndCause` (`:9`): None, DoubleBounce, Tin, Out, NotUp, LaunchFault, ServiceFault. El perdedor del `DoubleBounce` es `ToHit`. El de Tin, Out y NotUp es el `Striker` (`:144-182`).
- `RallyDirector.PointResult` (`Runtime/Rally/RallyDirector.cs:14-22`) lleva `Winner`, `Cause`, `Shots`, `Reception` (solo con DoubleBounce) y `TinTouched`.
- `Reception` es `ReceptionResult` (`Core/Rally/RallySim.cs:11`): Contact, Air, Late, Unreachable, NoSwing, SystemFault o NotEvaluable. La del humano sale de `HumanReceptionResult` (`RallyDirector.cs:494-503`).
- Quien gana el punto saca el siguiente (`MatchScore.Server` después de `PointWon`). **Si el humano pierde, la pausa siguiente es ordinaria y no hay `HumanServePause`**: el aviso está visible.

**Golpe del humano** (`Runtime/Shot/ShotController.cs`):
- `ShotResolved(ShotRecord)` (`:115`), emitido en `Finish` (`:397-403`).
- El `ShotRecord` trae `Applied`, `Result` (Perfect/Valid/…), `Power` = P pedida, `AimX` y `AimY` = mira pedida (`:426`), y `ContactX/Y/Z`.
- `LastOpportunity` Bounce/Volley (`:67`). `LastStrokeBoast` (`:95`): dos paredes aplicado y sin respaldo.
- Que la orden apuntó a la lateral (`order.Side.Exists`, `:333-344`) **no se expone**. Además `ShotRecord.Side` es el lado del golpe, drive o revés (`:285`), y no la mira lateral.
- El saque del humano no pasa por `ShotResolved` como golpe. Sale por `ServeAim` y `ServePlanner`, y no se registra como `shot` (informe `2026-11-26-fondo-gana.md` §3b).

**Rival y botes** (`RallyDirector.cs`):
- El golpe del humano se compromete en `:452-466` (`flightStartStep = contactStep`).
- El del rival, en `:573-579`: `at = Evaluate(pendingStep)` antes de `at.RacketContact(Rival)`.
- `Evaluate` (`:436-447`) recorre `host.CanonicalContacts` con `Step > flightStartStep`. El primer contacto `Floor` no persistente de ese tramo es el primer bote del golpe del humano. **Hoy nadie lo expone.**
- `RivalSwing(StrokeSide, bool contact)` (`:114`) avisa cuando el rival le da a la pelota.

**Líneas y constantes de cancha que ya existen** (`Core/Court/CourtDimensions.cs:8-10`):
- `Length` 9,75, `Width` 6,40.
- Línea corta: `Length − ShortLineNearBackEdgeFromBack` = 5,49 m desde el frontis. La usan `FrontDeepDrive.Active` y `ReturnLineJudge.ServeLandsInQuarter`.
- `ServiceBoxInner` 1,60: ancho del cuadro de saque medido desde la pared lateral.
- AX-1: la pared derecha está en x = 0 y la izquierda en x = 6,40. El frontis está en z = 0.

**Límites de potencia y altura que ya existen:**
- P > 0,5 («por encima de potencia media») en `FrontDeepDrive.Active` (`Core/Shot/FrontDeepDrive.cs:43-44`) y en `ShotIntentSolver.Active` (`Core/Shot/ShotIntentSolver.cs:70-80`).
- `RallyParams.ServeKindLobMinAimY` = 3 m (`Core/Rally/RallyParams.cs:44`): en el saque del humano, una mira a esa altura o más arriba es globo, y por debajo es saque rápido. Es igual a `AimHigh` 3 m, el globo de las opciones del rival (`:59`).
- `ShotOption.Long` (`Core/Rally/ShotOptions.cs:22`) es solo del rival.

**Lo que no existe:**
- **No hay un clasificador del golpe del humano en el peloteo** (drive, globo, dejada). El humano pide un punto del frontis y una potencia. La spec v0.3 lista drive, cruzado y drop como intenciones y el globo como [FUTURO] (`2026-10-20-squash-clash-design-v0.3.md:386-387`). También dice que la «pelota suelta» es un **resultado emergente**, no un golpe (`:391`).
- **No hay un rasgo «despegada de la pared» ni «al medio»** (informe `2026-11-26-fondo-gana.md` §4.2; `RivalAi.cs:185-200` solo lee si la pelota es corta).
- «Drive paralelo: largo, pegado a la pared lateral» (`2026-10-19-squash-clash-design.md:166`) es una descripción de la spec v0.1 archivada. **No es un umbral vigente.**
- El informe de #123 agrupa los botes en < 5,49 m, 5,49–7 m y ≥ 7 m. El corte de 7 m es de ese diagnóstico y **no es una constante del código**.

## 2. Alcance y fuera de alcance

**Dentro:**
- Una línea de texto en la fila `rally-banner-hint` del aviso entre puntos. Sale cuando el humano pierde un punto por un drive suyo suelto (§4).
- Una clase pura de clasificación (`Core/Rally/LooseDrive`).
- Un adaptador (`Runtime/Rally/LooseDriveHint`).
- Dos lecturas nuevas, de solo lectura (§5).
- Un interruptor DEV.
- Una métrica aditiva (P13).

**Fuera:**
- Física, P2, solver, trayectorias, la guardia «Drive adelante», `ShotIntentSolver`, timing, anillos, contrato de contacto, reloj y gesto.
- La IA del rival (`RivalAi`, estilos, opciones).
- `PointRules` y sus decisiones.
- Umbrales, tolerancias, oráculos y criterios existentes: no se cambia ninguno.
- `RallyParams`: no se agregan campos, porque cambiaría el hash `ebf8f717f8c5` (AGENTS §5). Los valores nuevos son `const` de `LooseDrive`.
- `DefaultPreset.json`, SimulationKey y las pistas de primeros partidos: su lógica, sus textos y `HintPicker`.
- El árbitro (`rally-referee`) y la nota del golpe (`shot-note`).
- Audio y voz.
- UI nueva: ni capa, ni tarjeta, ni token de color, ni fuente, ni icono nuevo.
- El modo Rally sin partido y la práctica.
- Recalibrar.

## 3. Enganche

- **Canal.** La fila que ya existe, `rally-banner-hint`, dentro de `rally-banner-card`, sin canal nuevo. **Decisión conservadora de la spec:** la miniatura `rally-banner-hint-icon` queda oculta para esta pista. Así no hay icono nuevo, y el aro y la esquina siguen siendo propios de H1 a H3.
- **Cuándo.**
  - En la pausa ordinaria (`PointOver`, sin `HumanServePause` y sin `MatchOver`) que sigue a un punto que cumple §4, dentro de un partido (`rally.Match != null`) que no terminó.
  - Si ese punto cierra el partido, sale la tarjeta de resultado y **no** sale la pista.
  - Nunca fuera de `PointOver`.
- **Qué partidos** — **P11**. Propuesta: todo partido sin piloto automático, incluidos la revancha y el «Partido» DEV. Con piloto automático no sale, salvo con el punto de prueba `ForceForTests`, que solo usan los tests nuevos. Razón: la pista no es de primeros partidos y el piloto no es una persona. Excluir el piloto además deja intactas las pruebas existentes, que juegan con piloto. Alternativa: solo partidos desde el menú, como `FirstMatchHints`.
- **Frecuencia** — **P9**. Propuesta: como máximo **una vez por partido**, sin guardar nada en `PlayerPrefs`. Cuenta como usada cuando `HintOracle.BannerSeen` da verdadero, igual que el oráculo de H1 a H3. Si se toca «Seguir» antes de opacidad 1, no cuenta y puede salir en el próximo punto que cumpla. Razón: no ser insistente, y no persistir nada evita tocar `ZFirstRunFixture`. Alternativas: cada punto que cumpla; como máximo 2 por partido; una vez por instalación.
- **Prioridad y convivencia** — **P10**. Propuesta:
  - (a) Si `FirstMatchHints` eligió una pista de aviso para esa pausa, sale esa y la del drive **no** sale ni cuenta.
  - (b) La del drive **no se arrastra** a la pausa siguiente, porque habla de ese punto.
  - (c) El árbitro y la nota del golpe no cambian: van en otra capa y en otra franja.

  Razón: el encargo pide no pisar las pistas de primeros partidos. Además, H0 en el aviso sale justo en la pausa después de que el humano pierde el primer punto. Alternativa: que gane la del drive.
- **Contenido de la fila:** solo el texto de la §8, en Rubik Regular 14 `--glass-text`, con `.sc-banner-hint` y `.sc-hint-text` tal como están.

## 4. Definición verificable de «drive suelto»

Todo se calcula con datos que ya existen (§1). Los valores son `const` de `LooseDrive`, salvo `ServeKindLobMinAimY`, que se lee de `rally.Params`.

**4.1 Drive del humano.** Es un golpe que sale por `ShotResolved` en una recepción del humano de un partido y cumple todo esto:
1. Aplicado: `Applied` y `Result` Perfect o Valid.
2. Al frontis: la orden no apuntaba a la lateral. Eso excluye el dos paredes y también el que cayó al drive de respaldo. Hace falta la lectura nueva `ShotController.LastStrokeSideAimed` (§5).
3. Desde el bote: `LastOpportunity == Bounce`. Excluye la volea (**P4**).
4. `Power > 0,5` (**P2**). Es la frontera «por encima de potencia media» que ya usan `FrontDeepDrive.Active` y `ShotIntentSolver.Active`. Deja fuera la dejada, que en el juego es suave (`PowerDrop` 0,12 del rival). Alternativa: sin filtro de potencia.
5. `AimY < ServeKindLobMinAimY` (3 m), con la mira pedida del `ShotRecord` (**P3**). Es la frontera globo / saque rápido que ya rige el saque del humano, igual a `AimHigh`. Deja fuera el globo. Alternativa: sin filtro de altura.

El saque nunca cuenta: no es un golpe de `ShotResolved`.

**4.2 Primer bote.** Es el primer contacto `Floor` no persistente de `host.CanonicalContacts` con `flightStartStep < Step ≤ pendingStep`, leído en el compromiso del golpe del rival (`RallyDirector.cs:573-579`), antes de `RacketContact`. Se usa `ContactEvent.Point`, sin corregir por el radio.
- Si el rival voleó y no hubo bote, no hay pista (**P8**).

**4.3 Botó corto** (**P5**). `bote.z < court.Length − court.ShortLineNearBackEdgeFromBack`: el primer bote cae delante de la línea corta (5,49 m). Es la línea real de la cancha.
- El informe de #123 muestra que el humano gana el 36 % de los puntos con botes < 5,49 m, frente al 97 % con botes de 7 a 8,5 m. Eso respalda que la línea separa algo, pero ese dato no es un criterio.
- Alternativas: el tercio de fondo `2/3 · Length` (6,5 m), que usa `FrontDeepDrive.Deep` para otra cosa. El corte de 7 m del informe se descarta porque no es una constante del código.

**4.4 Lejos de la lateral** (**P6**). `min(bote.x, court.Width − bote.x) > court.ServiceBoxInner`: el primer bote cae a más de 1,60 m de la pared lateral más cercana, es decir, fuera del ancho del cuadro de saque. Vale para la recta y para la cruzada.
- Alternativas: `BoastWallMax` 2 m, que es del rival, o `ServeLandingFromWall` 1,4 m, que es del saque. Las dos son de otros sistemas.

**4.5 El drive es la causa.** Se cumple todo esto:
1. El drive (4.1) es el **último golpe aplicado del humano** en el punto.
2. Después hubo **exactamente un** contacto de raqueta del rival (`RivalSwing(_, true)`).
3. El punto cierra con `Winner == Rival`, `Cause == DoubleBounce` y `Reception == Unreachable`: el humano no llegó a la devolución (**P7**). Alternativas: sumar `NoSwing`, o cualquier `DoubleBounce`.

El resultado es `LooseDriveKind`: `Short`, `Wide` o `ShortAndWide`. Si no hay drive suelto que explique el punto, es `None`.

**4.6 Casos negativos (todos dan `None`):**
- El golpe no es drive: dos paredes, dos paredes con respaldo, volea, P ≤ 0,5, mira ≥ 3 m, no aplicado (Failed, Unreachable, NoOrder, SystemFault).
- Saque del humano: el punto empieza con el saque y el rival devuelve el saque. No hay drive pendiente.
- El humano gana el punto, o el punto no tiene ganador (`LaunchFault`, tope de golpes).
- El humano pierde por una falta propia: Tin, Out o NotUp, sea con el drive o con el golpe siguiente.
- El humano pierde por `Air` o `Late` (P7).
- El rival devolvió más de una vez después del drive. Pasa, por ejemplo, si el humano volvió a golpear: entonces hay un golpe nuevo y el drive deja de ser el último.
- Bote detrás de la línea corta **y** a 1,60 m o menos de la lateral.
- El rival voleó el drive (P8).
- `HumanServePause`: no puede pasar, porque el humano perdió y saca el rival. Igual se prueba.
- `MatchOver`, práctica, Rally sin partido, piloto automático (P11) o `LooseDriveHint.Enabled = false`.

## 5. Dónde vive el código (para la implementación)

- **`Core/Rally/LooseDrive.cs`**, pura y sin Unity:
  - `IsDrive(applied, volley, sideAimed, power, aimY, lobMinAimY)`;
  - `Classify(Vector3? bounce, CourtDimensions)`;
  - `Judge(...)`, con los hechos de 4.5;
  - `Text(LooseDriveKind)`;
  - `const float MinPower = .5f`.
- **`Runtime/Rally/LooseDriveHint.cs`**, el adaptador:
  - se suscribe a `ShotResolved`, `RivalSwing`, `PointEnded` y `RallyStarted`;
  - guarda el drive pendiente y la cuenta por partido;
  - tiene `Enabled` (estático, encendido por defecto, no se recuerda) y `ForceForTests`;
  - tiene `Shown` (`LooseDriveKind`) y `ReportBanner(...)` con `HintOracle.BannerSeen`.
- **Lecturas nuevas, solo lectura y sin cambiar decisiones:**
  - `ShotController.LastStrokeSideAimed`, que vale `order.Side.Exists` del golpe y se limpia en cada lanzamiento como `LastStrokeBoast` (`:139`);
  - `RallyDirector.LastReturnedBounce` (`Vector3?`), que es 4.2 fijado en el compromiso del rival y se limpia en `BeginPoint`.

  `ShotController` está junto al contrato de contacto (zona de Codex, AGENTS §4). La lectura nueva no toca el contrato, pero se declara en la PR y la implementa o revisa Codex.
- **`RallyHud`:** en `RefreshHints`, si `FirstMatchHints` no muestra una pista en el aviso y `LooseDriveHint.Shown != None`, la fila muestra `LooseDrive.Text(kind)` con el icono oculto. `ReportHints` informa al adaptador que corresponda.
- **DEV:** un interruptor «Pista del drive» en `DevPanel/HintsDevSection.cs`, solo con `SQUASH_DEV`.

## 6. Criterio de aceptación

1. **Clasificación** (EditMode). Con `CourtDimensions.Standard()`, la línea corta se calcula y no se escribe a mano:
   - bote en (3,2; 5,40) da `ShortAndWide`; en (0,8; 5,40), `Short`; en (3,2; 8,0), `Wide`; en (0,8; 8,0) y en (5,6; 8,0), `None`;
   - en el borde, `z` igual a la línea corta no es corto, y una distancia igual a 1,60 m no es lejos;
   - `IsDrive`: P 0,5 no es drive y 0,51 sí; `AimY` 3,0 no es drive y 2,99 sí; la volea, la mira lateral y el golpe no aplicado no son drive.
2. **Causa** (EditMode, `Judge`): cada caso de 4.6 da `None` y el caso positivo da su tipo.
3. **En escena** (PlayMode). Partido con piloto automático, `ForceForTests`, `AutopilotPower = Strong` (P 0,85), semilla y nivel fijos:
   - en cada pausa entre puntos, `rally-banner-hint-text` muestra `LooseDrive.Text(k)` si y solo si el último punto da `k != None`, según un recálculo independiente del test desde `host.CanonicalContacts` y los `ShotRecord`, y no hubo antes otra vista en el partido;
   - el test exige al menos un caso positivo en la corrida. Si con la semilla no aparece ninguno, se registra y se detiene. No se afloja el criterio (AGENTS §2). Queda pendiente verificarlo en la implementación.
4. **Frecuencia y prioridad** (PlayMode, con un punto de prueba que inyecta un punto calificado):
   - con P9, la segunda vez en el mismo partido no sale;
   - «Seguir» con `BannerOpacity < 1` no cuenta;
   - un partido nuevo vuelve a habilitarla;
   - con una pista de primeros partidos elegida en la pausa, sale esa, y en la pausa siguiente no aparece la del drive si su punto no se repitió.
5. **Nunca fuera de lugar.** Recorriendo el partido cuadro a cuadro, la fila con texto de drive solo se ve con `Phase == PointOver`, sin `HumanServePause` y sin `MatchOver`. Con `Enabled = false`, nunca.
6. **Sin tiempo agregado ni cambio de juego.** Con las mismas entradas que el criterio 9 de front 2, con la pista encendida o apagada, la secuencia de `PointResult`, el paso de inicio de cada punto y cada `WaitBetweenPoints` son idénticos, con igualdad exacta. Los hashes no cambian: defaults `207da8f2c636`, SimulationKey `166a96e319c7`, RallyParams `ebf8f717f8c5`.
7. **Layout** (`ZDeviceLayoutTests`, todos los perfiles):
   - con el texto más largo de la §8, el aviso cabe en el área segura, en dos líneas como máximo y sin elipsis;
   - no se cruza con `rally-referee-card` ni con la cabecera.
8. **Las pruebas existentes no se editan** y pasan con la pista encendida.
9. **Puertas:**
   - EditMode y PlayMode completos en verde en el Mac del PO;
   - `check-release` OK;
   - AutoQA `scripts/autoqa.sh mvp` PASS/0/nivel 1. No es obligatorio por AGENTS §3, porque no toca timing, gesto ni contrato, pero se pide como evidencia de que no cambia ninguna decisión, como en el árbitro.
10. **iPhone** (§7): en la pausa después de perder un punto por un drive corto o abierto, la línea se lee en los 2,5 s reales; sale como máximo una vez por partido (P9), y la métrica (P13) trae el bote.

## 7. Qué se prueba sin iPhone y qué con iPhone

**Sin iPhone (Editor, TDD):** todo lo de la §6 salvo el punto 10:
- EditMode: `Rally/LooseDriveTests` y, si se adopta P13, `Diagnostics/UserMetricsTests`;
- PlayMode: `ZLooseDriveHintTests` y `ZDeviceLayoutTests`.

**Con iPhone**, solo en la ventana que SC-PM coordina con Rafael, con respaldo y cotejo de presets y métricas (AGENTS §8):
1. En Medio, pegar a propósito unos drives suaves al medio (P un poco sobre la media y mira baja).
   - Resultado observable: tras perder un punto así, sale la línea y se entiende.
   - En el resto del partido no vuelve a salir.
2. Ganar puntos con drives largos y pegados. Resultado observable: no sale nada.
3. Métricas: las líneas `drive-hint` coinciden con lo visto.

## 8. Textos

**Regla de textos** (pedido de Rafael del 23 nov, spec front 2 §1.8):
- términos reales de squash: frontis, pared lateral, dos paredes, globo, recta, volea, dejada, saque;
- español neutro con tú, sin chilenismos ni voseo;
- si hay dudas de vocabulario, se preguntan y no se inventa un término;
- la revisión de textos es parte obligatoria del review de la PR de implementación.

| Caso | Texto propuesto | Largo |
|---|---|---|
| `Short` | «Tu drive botó corto: el rival lo aprovechó.» | 43 |
| `Wide` | «Tu drive quedó lejos de la pared lateral: el rival lo aprovechó.» | 64 |
| `ShortAndWide` | «Tu drive botó corto y lejos de la pared lateral: el rival lo aprovechó.» | 71 |

- La línea explica el punto y no da una orden. La causa visible («Tú: doble bote (no llega)») sigue en `rally-banner-cause`.
- El texto más largo es más corto que H1 (`ReceptionRule.Full`, 78 caracteres), que ya cabe en dos líneas en esa fila.
- Se usan «bote», que ya está en el juego («doble bote»), y «pared lateral», el término completo.
- Rafael los ajusta después del playtest sin tocar la lógica.

**Preguntas de vocabulario** (P12, también para SC-Lead):
1. «Drive» no está en la lista de la regla, que nombra «recta». Pero la pista cubre la recta y la cruzada, y el encargo dice «drive». La guía de saque ya cambió «drive» por «saque rápido» (`RallyHud.cs:174`). ¿Va «drive»?
2. «Suelto» (el nombre del encargo) no aparece en el texto. ¿Se prefiere «tu drive quedó suelto»?

## 9. Reglas de AGENTS.md que aplican

- **§2:**
  - Claude define la arquitectura; implementa Codex y revisa Claude. Si implementa Claude o un subagente, se usa Sonnet, la revisión no es independiente y revisa Codex.
  - Con el límite de créditos de 5 horas agotado, se detiene todo.
  - Un incremento es una rama y una PR, con commits pequeños.
  - Ante un defecto, se registra y se detiene.
  - La fusión sigue las reglas de §2, con la revisión aprobada y las puertas en verde. **Esta fase no abre PR ni fusiona.**
- **§3:**
  - No se modifican spec, escenarios, umbrales ni criterios para pasar.
  - Una sola autoridad de la pelota: la pista solo lee.
  - TDD con `scripts/test.sh editmode|playmode [filtro]`, con RED válido por aserción.
  - Gate oficial de PlayMode: el Mac del PO.
  - Un solo Unity a la vez, con `sc-unity.lock`.
  - No se versiona `scripts/device.env`.
  - Código, identificadores y commits en inglés; documentación en español.
  - Sin abstracciones fuera de la necesidad.
- **§4:** no se tocan física, solver, `DefaultPreset.json` ni SimulationKey. La lectura en `ShotController` está junto al contrato (zona de Codex) y se declara.
- **§5:** no cambia ningún hash. No hay campos nuevos en `RallyParams`.
- **§7:** umbrales y criterios nuevos son del PO; por eso van P2 a P8. La sesión en el iPhone sigue §8.
- **§8:** instalación solo en una ventana coordinada, con respaldo y cotejo.
- **Esta fase:** sin Unity, sin `scripts/test.sh`, sin AutoQA, sin `check-release`, sin el lock y sin tocar `device.env` (encargo de SC-PM).

## 10. Plan TDD para el implementador (no se ejecuta en esta fase)

1. **RED 1 (EditMode, `Tests/EditMode/Rally/LooseDriveTests.cs`).** Primero se crea `LooseDrive` con métodos que devuelven `None` o `false`, para que el RED sea por aserción y no por error de compilación. Las pruebas son los casos del criterio 1. Después viene GREEN con `Classify` e `IsDrive`.
2. **RED 2 (EditMode).** `Judge` con la tabla de 4.5 y 4.6, sin escena. Luego GREEN.
3. **RED 3 (EditMode, si se adopta P13).** Una línea `drive-hint` por punto calificado, con `matchId`, `kind`, `bounceX`, `bounceZ`, `seen` y `reason` (`""`, `limit`, `firstMatchHint` o `notSeen`), escrita al terminar la pausa. Las líneas actuales quedan idénticas.
4. **RED 4 (PlayMode, `ZLooseDriveHintTests`).**
   - Se escribe sobre la base de `ZRallyDirectorTests` o `ZFirstRunFixture`, con `ForceForTests` y un punto de prueba que inyecta un punto calificado.
   - Cubre los criterios 3 a 6.
   - Las pruebas de `LastStrokeSideAimed` y `LastReturnedBounce` van en el mismo archivo.
5. **RED 5 (PlayMode, `ZDeviceLayoutTests`, casos nuevos).** El criterio 7.

**Reglas de prueba, fijadas antes de implementar:**
- **No se tocan:**
  - pruebas existentes, aserciones, textos de otras pistas y `HintPicker`;
  - `ZFirstRunFixture`, porque no se persiste nada;
  - `RallyParams`, `DefaultPreset.json`, física, IA, timing, contrato y oráculos existentes.
- La pista **apagada** se usa solo en dos lugares, ambos en tests nuevos: la prueba «apagada no muestra ni escribe nada» y la corrida de referencia del criterio 6.
- Si una prueba existente que juega sin piloto falla con la pista encendida, es un defecto: se registra y se detiene. Pendiente de verificación en la implementación: `ZRingClosePulseTests`, `ZReceptionRingsTests`, `ZServeReceptionMarginTests` y `ZVolleyEnergyTests` ponen `Autopilot = false`.
- Pendiente de verificación en la implementación (no se pudo correr en esta fase):
  - que `host.CanonicalContacts` todavía tenga el bote del humano en el compromiso del rival, como ya supone `Evaluate(pendingStep)`;
  - que el piloto `Strong` dé al menos un caso positivo con una semilla fija;
  - cuántas veces por partido calificaría hoy un punto. Eso se mide con un experimento aparte de la regresión (AGENTS §3), y sirve para P9.

## 11. Preguntas para Rafael

Cada una se responde con sí o no. Va la recomendación de Claude con su razón.

1. **¿Se adopta la mejora y se encarga la implementación según esta spec?** Recomiendo **sí**: explica el punto con datos que el juego ya tiene, sin tocar el juego.
2. **P2. ¿Un golpe cuenta como drive solo si P > 0,5?** Recomiendo **sí**: es la frontera de potencia que ya usan dos sistemas del humano y deja fuera la dejada.
3. **P3. ¿Se excluye como globo la mira pedida a 3 m o más (`ServeKindLobMinAimY`)?** Recomiendo **sí**: es la frontera globo / saque rápido que el jugador ya conoce.
4. **P4. ¿Se excluye la volea, aunque sea fuerte y baja?** Recomiendo **sí**: el texto dice «tu drive» y la volea tiene su propio nombre y su propia pista.
5. **P5. ¿«Botó corto» es un primer bote delante de la línea corta (5,49 m)?** Recomiendo **sí**: es una línea real de la cancha, que el jugador ve.
6. **P6. ¿«Lejos de la pared lateral» es un primer bote a más de 1,60 m (`ServiceBoxInner`) de la lateral más cercana?** Recomiendo **sí**: es el ancho del cuadro de saque, visible en la cancha.
7. **P7. ¿Solo cuenta si el rival devuelve ese drive con un solo golpe y el humano no llega (`Unreachable`)?** Recomiendo **sí**: con `Air` o `Late`, el error fue de timing y la pista explicaría mal el punto.
8. **P8. ¿Sin bote observado (el rival voleó) no hay pista?** Recomiendo **sí**: sin bote no hay «botó corto» medido, y un pronóstico sería una regla nueva.
9. **P9. ¿Como máximo una vez por partido, sin guardar nada, contando solo cuando se vio?** Recomiendo **sí**: no es insistente y no toca `PlayerPrefs`.
10. **P10. ¿Las pistas de primeros partidos tienen prioridad y la del drive no pasa a la pausa siguiente?** Recomiendo **sí**: así no se pisan y la línea siempre habla del punto recién jugado.
11. **P11. ¿Sale en todo partido sin piloto automático, revancha y «Partido» DEV incluidos?** Recomiendo **sí**: es una ayuda de juego, no de bienvenida.
12. **P12. ¿El texto usa «drive» (y no «recta» ni «suelto»), con los textos de la §8?** Recomiendo **sí**: «drive» cubre la recta y la cruzada, y es el término del encargo. SC-Lead puede corregirlo.
13. **P13. ¿Se agrega la métrica aditiva `drive-hint` por punto calificado?** Recomiendo **sí**: sin ella, en el iPhone no se puede medir cuántas veces sale ni si se vio.
