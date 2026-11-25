# Golpe del partido en la tarjeta de resultado

25 nov 2026. Base `origin/main` `1dc9dcb` (#117). Encargo `set8-golpe-partido`. Claude define la arquitectura; un subagente Sonnet dirigido por Claude implementa (AGENTS §2), así que la revisión de Claude no es independiente y se declara. Queda pendiente la revisión cruzada de Codex.

Agrega **una** línea a la tarjeta de fin de partido con el dato de mayor mérito del jugador, si llega a 3. No cambia el juego, el preset, `RallyParams` ni los hashes de AGENTS §5.

## 1. Regla

Prioridad fija; se muestra el **primero** que llegue a **3 o más**:

1. Perfectos
2. Dos paredes ganadores
3. Voleas

Si ninguno llega a 3, la línea no sale. El umbral de 3 y el orden vienen del encargo; no se inventa ningún otro umbral.

## 2. Definiciones (todas existentes)

Solo cuentan los golpes del **jugador** resueltos por `ShotController.ShotResolved` durante un partido (`rally.Active`, `rally.Match != null`, fase `HumanReception`, la misma puerta de `RallyDirector.OnResolved`). El rival no pasa por ese evento y nunca cuenta. El saque del jugador no genera `ShotRecord` y no cuenta.

| Dato | Definición | Origen |
|---|---|---|
| Perfecto | `record.Applied` y `Result == "Perfect"` | `ShotResult.Perfect`; spec Front 3 §1 |
| Volea | `Applied`, `Result` es `Perfect` o `Valid`, y `controller.LastOpportunity == Opportunity.Volley` leído dentro del callback | spec Front 3 §1; `FirstMatchHints.cs:210` |
| Dos paredes | `controller.LastStrokeBoast` leído dentro del callback (golpe aplicado, resolución autoritativa con `SolveBoast`, sin caer al drive de respaldo) | contrato de la spec Front 2 §1.6, `ShotController.cs:93-95,400` |
| Ganador | El punto lo gana quien pegó y ese golpe fue **su último golpe del punto** («winner o error forzado») | `won` de la spec `2026-11-22-rival-short-ball-design.md` (tabla del Anexo y `won=point.Winner==… && j+1==logs.Count-1`) |

Por lo tanto, **dos paredes ganador** quiere decir esto: `RallyDirector.PointEnded` con `Winner == Human` y el último golpe del jugador que salió por `ShotResolved` en ese punto fue un dos paredes. Un Perfecto de volea cuenta como Perfecto y como volea. Un Perfecto a dos paredes ganador cuenta en los dos. Un golpe fallido, «No llega» o «Sin golpe» no cuenta. Un punto sin ganador no suma dos paredes ganadores.

Con el piloto automático se cuenta igual, como en Front 3. Los récords personales ya excluyen esos partidos y no cambian.

## 3. Diseño

- **`SquashClash.Core.Rally.StrokeOfTheMatch`**: clase pura con `Perfects`, `BoastWinners` y `Volleys`, y los métodos `Reset()`, `OnStroke(applied, perfect, volley, boast)` y `OnPointEnded(humanWon)`. Tiene además la función estática `Line(perfects, boastWinners, volleys)`, que devuelve el texto o `""`, con `MinCount = 3`.
- **`MatchSummary`** (`Runtime/Menu/PersonalRecords.cs`) suma los campos `Perfects`, `BoastWinners` y `Volleys`, con parámetros opcionales en 0 para que las llamadas existentes no cambien, y la propiedad `StrokeOfTheMatchLine`. `PersonalRecords.Apply` no los lee.
- **`RallyHud`** es dueño de la instancia (`hud.Strokes`), igual que ya lo es de `LongestRally` y `MatchSeconds`. Se suscribe a `ShotResolved` y a `PointEnded`. La instancia se reinicia cuando cambia la referencia de `rally.Match`, es decir en un partido nuevo o en la revancha (`RallyDirector.Rematch` crea un `MatchScore` nuevo), y al salir del modo. La vista nunca escribe nada en Core.
- **Tarjeta.** Arma el `MatchSummary` siempre. Lo aplica a los récords solo en las mismas condiciones de hoy. Una etiqueta nueva, `match-result-highlight`, va después de «Rally más largo» y antes de la línea de récord. Sigue al interruptor DEV `RallyHud.ExtendedCardOn`: apagado, la tarjeta queda como antes.
- **Estilo** `.sc-result-highlight`: Rubik Bold, 14 px, `--ink`, centrada, `white-space: normal` y márgenes de 4 px. No agrega colores ni tipografías. El rojo de línea queda para el récord, que sigue siendo lo más fuerte. El amarillo `--dot` no tiene contraste sobre el yeso y es de la pelota.

## 4. Textos

| Caso | Texto | Caracteres |
|---|---|---|
| Perfectos | «Golpe del partido: 4 Perfectos» | 30 (31 con dos dígitos) |
| Dos paredes ganadores | «Golpe del partido: 3 dos paredes ganadores» | 42 (43) |
| Voleas | «Golpe del partido: 5 voleas» | 27 (28) |

Siempre en plural, porque nunca baja de 3. «Perfecto» va con mayúscula, como en el resto del juego. «Dos paredes» no cambia en plural.

## 5. Criterio de aceptación

1. **EditMode, `Rally/StrokeOfTheMatchTests`.**
   - Con exactamente 3 de cada tipo y lo demás en 0 sale la línea de ese tipo.
   - Con 2 no sale nada, sea en uno o en los tres a la vez.
   - En los empates de cantidad manda la prioridad: 3/3/3 da Perfectos, 0/3/3 da dos paredes, y 2/5/5 da dos paredes.
   - Si uno de mayor prioridad llega a 3, gana aunque otro tenga más: 3/9/9 da Perfectos.
   - Un partido sin golpes no muestra línea.
   - Las reglas de `OnStroke` y `OnPointEnded` se cumplen:
     - un dos paredes seguido de un punto ganado suma 1;
     - un dos paredes seguido de un punto perdido o sin ganador suma 0;
     - un dos paredes seguido de un golpe de frontis y luego un punto ganado suma 0;
     - un golpe no aplicado no suma nada;
     - un Perfecto de volea suma en los dos;
     - `Reset()` deja todo en 0.
2. **PlayMode, observador independiente.** Se juega un partido con piloto automático y semilla fija. El test se suscribe a `ShotResolved` y `PointEnded` y lleva sus propios conteos con las reglas de §2, sin leer `hud.Strokes`. Al final:
   - `hud.Strokes` coincide con esos conteos;
   - el texto de `match-result-highlight` es `StrokeOfTheMatch.Line(...)` de esos conteos;
   - la línea se ve solo si ese texto no está vacío.
3. **PlayMode, dos paredes ganador en escena.** En un partido, un dos paredes resuelto (`LastStrokeBoast`) seguido de un punto ganado por el jugador suma 1 en `hud.Strokes.BoastWinners`. Si el punto lo gana el rival, suma 0.
   - **Revisión 2, 25 nov.** En escena no se reprodujo ningún dos paredes ganador: 0 de 35, porque el rival los devolvió todos. Por decisión de producto, la prioridad se mantiene y esto queda como observación.
   - El test exige al menos un dos paredes resuelto en la escena y verifica la regla en cada punto: suma 1 si el punto lo ganó el jugador y 0 si no. No exige que el caso ganado aparezca.
   - La rama «ganó» queda probada en EditMode.
4. **PlayMode, ocultar y reiniciar.** Con los conteos por debajo de 3 al cerrar el partido, la línea queda oculta. Con `ExtendedCardOn` en `false`, también. La revancha reinicia los conteos.
5. **Layout.** La línea más larga posible, «Golpe del partido: 99 dos paredes ganadores», cabe en la tarjeta en dos líneas como máximo, sin salirse de la tarjeta.
   - Con esa línea y con la línea de récord más larga que puede salir, la tarjeta queda dentro del área segura (`AssertFits`). La línea de récord más larga es la de tres récords: «¡Nuevo récord! Rally más largo: 23 golpes · Mayor diferencia: 7 puntos · Victoria más rápida: 4:12».
   - Se prueba en todos los perfiles de `ZDeviceFixture`, desde el SE 16:9 hasta el Pro Max. El test registra el ancho medido de los tres textos.
   - **Revisión 2, 25 nov.** La primera versión no cabía en el área segura cuando había récord. El SE ya se salía antes de este cambio. Decidieron SC-PM y SC-Lead, con consentimiento del PO, compactar la tarjeta. Solo se tocan el espaciado y los tamaños de las clases `.sc-result-*`. No cambian los textos de las otras líneas, los umbrales, las fuentes ni los colores, y los botones conservan los 46 px de alto.
6. **Tests existentes sin editar.** `ZRecordsTests` y `ZMatchCardTests` pasan igual.
7. **Puertas.** EditMode y PlayMode completos en verde en el Mac del PO. La AutoQA se corre como evidencia de que no cambia ninguna decisión, aunque el cambio no toca timing, gesto ni contrato. Lo mismo vale para `check-release`.

## 6. Fuera de alcance

Conteos del rival, la línea completa de Front 3 («5 Perfectos, 3 voleas, …»), métricas `match-end`, récords con estos conteos y un interruptor DEV nuevo.

## 7. En el iPhone

- La línea se lee de un vistazo bajo «Rally más largo».
- En el SE o en el teléfono más chico, «dos paredes ganadores» corta en dos líneas limpias, sin elipsis.
- Un partido con pocos golpes buenos no muestra la línea.
- El conteo coincide con lo que recuerdas haber hecho.
