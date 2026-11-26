# Árbitro simple

26 nov 2026. Base `origin/main` `8a32d42` (#120). Encargo `set11-arbitro`. Claude define la arquitectura y un subagente Sonnet dirigido por Claude implementa (AGENTS §2). Por eso la revisión de Claude no es independiente y se declara así. Queda pendiente la revisión cruzada de Codex.

## 1. Contexto

Durante el partido nadie canta el marcador. El jugador ve los dígitos «Tú – Rival» arriba, pero no oye lo que diría un árbitro de squash: el puntaje con el del sacador primero, el aviso cuando la pelota da en el tin y el «match ball». Este cambio agrega esas tres frases como texto. No cambia el juego, el preset, `RallyParams` ni los hashes de AGENTS §5.

## 2. Alcance

- **(a) Marcador cantado.** En cada pausa entre puntos de un partido sale el puntaje con el del **sacador del punto siguiente primero**, como canta el árbitro real (WSF, *Rules of Singles Squash*, apéndice de llamadas del árbitro: «el marcador del sacador se canta primero»). Por ejemplo, si Tú tienes 1, el Rival tiene 3 y saca el Rival, sale «3–1». Si saca Tú, sale «1–3». El sacador es `MatchScore.Server` después de `PointWon`, es decir, el ganador del punto. Un punto sin ganador repite el mismo marcador.
- **(b) Aviso de tin.** El juego ya usa «tin» (`RallyText.Cause`, el tutorial y la cancha), así que se mantiene ese término. El texto es «¡Tin!» y sale cuando la pelota da en el tin y eso decide el punto:
  - en el peloteo, con la causa `PointEndCause.Tin`;
  - en el saque, cuando la falta de saque fue un contacto en el frontis que el juez de líneas marca como tin (`ServiceFault` con contacto terminal en `FrontWall` y veredicto `LineVerdict.Tin`).

  El encargo lo llama «tic». Aquí se entiende como el aviso de tin, con el término que ya usa el juego.
- **(c) «Match ball».** Sale cuando el próximo punto puede cerrar el partido para cualquiera de los dos jugadores. Se calcula con el objetivo (`MatchTarget`) y el margen (`MatchMargin`) del propio `MatchScore`, sin números fijos. La condición es `!Finished` y que exista un jugador con puntos `p`, frente a un rival con puntos `q`, que cumpla `p+1 ≥ objetivo` y `(p+1) − q ≥ margen`.

## 3. Fuera de alcance

Audio y voz; cambiar reglas, puntajes, umbrales, oráculos o pruebas existentes; el árbitro de frontis (en reserva); «let», «stroke», «cambio de saque» y la llamada inicial «cero a cero»; el modo Rally sin partido. En el último punto del partido no sale la frase: la tarjeta de resultado ya dice «Último punto: …».

## 4. Diseño

- **Core.**
  - `MatchScore` suma `Target`, `Margin` y `MatchBall`, todas de solo lectura.
  - `PointRules` suma `TinTouched`, de solo lectura y derivada del cierre ya registrado (`Cause`, `TerminalSurface` y `TerminalPoint`). No cambia ninguna decisión.
  - Clase pura nueva `RefereeCall`, con:
    - `Tin = "¡Tin!"` y `MatchBall = "Match ball"`;
    - `Score(int servidor, int receptor)` y `Score(MatchScore)`;
    - `Call(MatchScore, bool tin)`, que arma el texto completo para el oráculo de las pruebas.
- **Runtime.**
  - `PointResult` suma `TinTouched`, con un parámetro opcional que vale `false` por defecto. `RallyDirector.EndPoint` lo toma de `rules.TinTouched`.
  - `RallyHud` agrega una capa propia, `rally-referee`, con una pastilla `rally-referee-card` y tres etiquetas: `rally-referee-tin`, `rally-referee-matchball` y `rally-referee-score`.
  - La pastilla se ve en toda la pausa entre puntos de un partido que no terminó. Eso incluye la pausa del saque del jugador, donde el cartel del punto no se muestra. Se oculta al reanudar el juego o al terminar el partido.
  - Va centrada en el hueco de 28 px que ya existe entre la cabecera y el cartel del punto (`top = header.yMax + 3`, como máximo 22 px de alto). No mueve el cartel ni la guía de saque.
  - **Revisión 1, 26 nov.** En la pausa del saque del jugador, el texto «Toca arriba: globo» de la guía queda pegado bajo la cabecera en Mini, Pro y Pro Max. La primera corrida de layout lo mostró: la pastilla se cruzaba con `serve-guide-lob` en 3 de los 4 perfiles. Por eso, en esa pausa la pastilla va 6 px bajo `serve-guide-drive`, centrada, sin salir del área segura. En las demás pausas sigue bajo la cabecera. La guía no se mueve.
  - Hay un interruptor DEV `RallyHud.RefereeOn`, encendido por defecto, y la propiedad de lectura `RefereeShown`, que es el texto que hay en pantalla.
- **Estilo.**
  - La pastilla usa el vidrio de la cancha, como `.sc-ring-caption`.
  - El marcador va en Rubik Bold Italic de 16 px y en blanco: es lo que se mueve, como los dígitos del marcador.
  - «¡Tin!» va en Rubik Bold de 13 px, en el coral de golpe fallido `rgb(255,128,112)`.
  - «Match ball» va en Rubik Bold de 13 px, en `--dot`, el amarillo que el juego ya usa sobre vidrio para el saque y el partido ganado.
  - Las tres partes se separan con un espacio de 8 px y no con puntos medios.
  - No agrega fuentes ni colores nuevos.

## 5. Textos

| Caso | Pastilla | `RefereeShown` |
|---|---|---|
| Punto normal, saca quien va 3 contra 1 | «3–1» | `3–1` |
| Tin, sin match ball | «¡Tin!  2–4» | `¡Tin! · 2–4` |
| Match ball | «Match ball  6–4» | `Match ball · 6–4` |
| Tin y match ball | «¡Tin!  Match ball  6–5» | `¡Tin! · Match ball · 6–5` |

El guion es el de rango (–), sin espacios. «Match ball» es el término que usan los árbitros en las competencias en español. Se escribe con mayúscula inicial y sin comillas.

## 6. Criterio verificable

1. **EditMode, `Rally/RefereeCallTests`.**
   - **Orden del sacador.** Desde 3–1 con saque del jugador sale «3–1». Con Tú 1, Rival 3 y saque del Rival sale «3–1». Con Tú 1, Rival 3 y saque del jugador sale «1–3».
   - **Match ball con 7 y 2.**
     - 6–0 a 6–5 y 0–6 a 5–6 son match ball, sin importar quién saque.
     - 6–6 no lo es.
     - Con ventaja y margen de 2, 7–6 y 6–7 sí lo son, y 7–7 no.
     - 8–7 sí lo es.
     - 5–5 no lo es.
     - Un partido terminado (7–5) no lo es.
   - **Sin números fijos.** Con objetivo 11 y margen 2, 10–8 sí es match ball y 6–4 no. Con objetivo 7 y margen 1, 6–6 sí lo es.
   - **`Call`.** Arma los cuatro textos de §5.
2. **EditMode, `PointRulesTests` (pruebas nuevas, las existentes no se tocan).**
   - Un golpe del peloteo al frontis bajo el tin da `TinTouched` verdadero y la causa `Tin`.
   - Un saque al tin da `TinTouched` verdadero y la causa `ServiceFault`.
   - Un saque bajo la línea de saque pero sobre el tin, un golpe afuera y un doble bote dan `TinTouched` falso.
3. **PlayMode, en escena.** Se juega un partido con piloto automático y semilla fija.
   - En cada pausa entre puntos de un partido que no terminó, `rally-referee-card` se ve y `hud.RefereeShown == RefereeCall.Call(rally.Match, rally.LastPoint.TinTouched)`.
   - La parte del tin se ve solo si `TinTouched`. La de match ball, solo si `Match.MatchBall`.
   - El test exige que en la corrida haya al menos un match ball.
   - **Revisión 2, 26 nov.** Hubo tres búsquedas deterministas con más de 50 partidos con piloto automático. Se variaron el nivel, la sigma de timing, la semilla, la línea y la potencia del piloto, el saque nuevo y el estilo del rival. Ninguna dio un punto con tin, porque el piloto y el rival nunca apuntan bajo el tin (`ShotOptions` recorta la intención al dominio entre el tin y la línea de out). Por eso el test no exige un tin en escena. Verifica la regla en cada pausa: la parte del tin se ve si y solo si `LastPoint.TinTouched`.
   - La rama del tin queda probada así:
     - en EditMode, por `PointRules.TinTouched` (§6.2) y `RefereeCall.Call(…, true)` (§6.1);
     - en PlayMode, por el layout (§6.4), que muestra `rally-referee-tin` en pantalla;
     - `RallyDirector.EndPoint` pasa `rules.TinTouched` a `PointResult` en una sola línea, revisada a mano.

     Que salga en el juego real queda como observación para el iPhone.
   - Fuera de las pausas y en el fin del partido, la pastilla está oculta. Con `RefereeOn` en `false`, también.
4. **PlayMode, layout en todos los perfiles de `ZDeviceFixture`, desde el SE 16:9 hasta el Pro Max.**
   - Se usa el texto más largo, «¡Tin!  Match ball  99–98».
   - Con el cartel del punto visible y con la guía de saque visible, la capa `rally-referee` cumple `AssertFits` (dentro del área segura, sin aplastarse ni cortarse).
   - La pastilla no se cruza con la cabecera, con `rally-banner-card` ni con las etiquetas de la guía de saque.
   - El test registra el ancho medido.
5. **Pruebas existentes sin editar.** EditMode y PlayMode completos en verde en el Mac del PO.
6. **AutoQA.** El cambio no toca timing, gesto ni contrato. Igual se corre `scripts/autoqa.sh mvp` como evidencia de que ninguna decisión cambia.
