# Anillo ámbar que se cierra, anillo blanco que pulsa y un texto único de la regla

24 nov 2026. Rama `feat/ring-close-pulse-rule-text` desde `f306d70`. Encargo de SC-PM, ítem 1 del set de recepción (prioridad de SC-Lead, visto bueno de Rafael el 24 nov a las 15:07). Arquitectura y spec: Claude Opus. Implementación: Claude Sonnet con TDD (AGENTS §2), así que **la revisión de Claude no es independiente** y queda para Codex.

## Contexto

Hoy, en la recepción, el anillo ámbar (oferta de volea) aparece en el lanzamiento con su radio exterior (0,45 m) y se queda quieto hasta los últimos 500 ms antes del contacto de la volea (`ImpactCue.ApproachMs`). Su ventana de toque, en cambio, termina en S′ (`ShotController.TryVolleyWindow`), y entonces aparece el anillo blanco. El jugador no ve que esa ventana se está acabando. El anillo blanco también espera quieto en el radio exterior hasta sus últimos 500 ms, y no se distingue «espera» de «prepárate».

La regla está escrita de tres maneras: el paso 4 del tutorial («…toca y voleas en el aire. Espera al anillo blanco: golpeas…»), la pista H1 («Anillo ámbar: toca y volea antes del bote.») y la pastilla del anillo blanco («Espera el bote», #107).

## Alcance (solo cómo se ve y el texto)

1. **El anillo ámbar se cierra.** Mientras la oferta de volea no está elegida (`VolleyShown` y sin `VolleyCommitted`), el radio que se dibuja baja linealmente desde el radio exterior (0,45 m) en el inicio de la ventana (`from`) hasta el círculo del triángulo fijo (0,20 m, el doble del radio de calce) en S′ (`until`). Se contrae hacia el punto donde se le pega a la pelota. Nunca baja de 0,20 m, para no confundirse con el calce dentro del triángulo (0,10 m), que es la señal para soltar. Si el radio de `ImpactCue` ya es menor, se dibuja ese, así que el anillo nunca se ve más grande que hoy. El halo sigue al anillo. Sin S′ (ventana infinita, por ejemplo con la volea ya elegida) no hay cierre: se dibuja igual que hoy.
2. **El anillo blanco pulsa mientras espera.** Mientras está quieto en el radio exterior (`Approach` = 1, antes de sus últimos 500 ms), el alfa del anillo que converge oscila suavemente entre 1 y 0,45, con un periodo de 0,5 s (2 Hz). Empieza en 1 cuando se muestra. Cuando empieza a converger se queda fijo en 1, para que el calce se lea limpio. El trazo, el radio, el triángulo y las bandas no cambian. 2 Hz está por debajo del límite de tres destellos por segundo, y la variación es de alfa en una línea fina, no un destello de pantalla.
3. **Un texto único de la regla**, en una sola fuente (`ReceptionRule`), en dos mitades, una por anillo:
   - Ámbar: «Anillo ámbar: toca y volea en el aire.»
   - Blanco: «Anillo blanco: espera el bote y golpea.»
   - Regla completa = las dos mitades separadas por un espacio. Va en el paso 4 del tutorial y en la pista H1. La pastilla del anillo blanco muestra la mitad blanca, letra por letra la misma.

### Por qué se ajustó el texto propuesto

La propuesta de la revisión de #107 era «Anillo ámbar visible: toca y volea en el aire. Espera el anillo blanco: golpea después del bote.» Se ajustó por tres razones:

- La pastilla aparece sobre el anillo blanco cuando ya está visible, así que ahí «Espera el anillo blanco» no tendría sentido. Con una mitad por anillo, la pastilla puede repetir la regla al pie de la letra.
- Se conserva «espera el bote», aprobado por el PO el 23 nov.
- Las dos mitades usan imperativo con tú, como pedía la revisión.

Mostrar la regla completa en la pastilla (dos frases, unos 78 caracteres) taparía cancha y no se alcanza a leer en la espera. Queda como pregunta para SC-Lead.

## Fuera de alcance

- No cambia **cuándo** aparece cada anillo ni cuándo aparece la pastilla: sigue saliendo solo en la recepción del saque del rival y solo con el anillo blanco.
- No cambia el radio de calce (0,10 m), ni las bandas Perfecto/Válido, ni el radio del anillo ya elegido (volea comprometida) ni el del anillo blanco.
- No cambia el radio de toque del gesto ni la ventana de la volea. No toca timing, contrato, reloj (ClockMap/SimClock), solver, presets, umbrales, tolerancias, oráculos ni criterios. Los hashes de §5 no cambian.
- `TimingAidTests` no se modifica.
- No se reescriben las specs anteriores (`2026-11-20-volley-v2-design.md`, `2026-11-22-front-2-first-match-hints.md`): esta spec reemplaza sus textos de la regla.

## Criterios de aceptación verificables

| # | Criterio | Prueba |
|---|---|---|
| A1 | Con la ventana del ámbar fijada [from, until], el radio del anillo que converge en `from` es 0,45 m, en el punto medio es 0,325 m (±1e-4) y en `until` es 0,20 m (±1e-4). | PlayMode, vista sola |
| A2 | El halo tiene en todo momento el mismo radio que el anillo. | PlayMode, vista sola |
| A3 | Sin ventana (o con `until` infinito), el radio del ámbar es el mismo que hoy, el de `ImpactCue`. | PlayMode, vista sola |
| A4 | El radio que se dibuja nunca es mayor que el de `ImpactCue` ni menor que 0,20 m antes de T*. | PlayMode, vista sola |
| A5 | En la partida, mientras el ámbar no está elegido, el radio baja de forma monótona dentro de la ventana; después de elegir la volea, el anillo sigue el radio de `ImpactCue`. | PlayMode, escena Main |
| B1 | Anillo blanco quieto: el alfa del anillo que converge vale 1 al mostrarse, 0,45 (±0,01) a los 0,25 s y 1 a los 0,5 s; siempre está en [0,45; 1] y el anillo sigue habilitado. | PlayMode, vista sola |
| B2 | Anillo blanco convergiendo (últimos 500 ms): alfa 1 en todo momento. | PlayMode, vista sola |
| B3 | El ámbar no pulsa: alfa 1. El triángulo y las bandas no pulsan. | PlayMode, vista sola |
| C1 | El paso 4 del tutorial, la pista H1 y `ReceptionRule.Full` son exactamente el mismo texto; la pastilla muestra exactamente `ReceptionRule.White`, y `Full` empieza con `Amber` y termina con `White`. | PlayMode |
| C2 | Texto literal: «Anillo ámbar: toca y volea en el aire. Anillo blanco: espera el bote y golpea.» | PlayMode |
| R | `TimingAidTests`, `ZVolleyV2Tests` y la visibilidad de los anillos y de la pastilla de `ZReceptionRingsTests` siguen en verde sin cambiar sus aserciones. Solo cambian las cadenas esperadas de los textos que este encargo reemplaza (H1 en `ZFirstMatchHintsTests` y la pastilla en `ZReceptionRingsTests`). | EditMode, PlayMode, AutoQA |

## Qué se prueba sin iPhone y qué en iPhone

- **Sin iPhone (Mac):** todo lo de la tabla, con `scripts/test.sh editmode|playmode`. Además AutoQA `mvp` V/T3/volea, porque el cambio está junto a la oferta de volea y el gesto, aunque no toque timing ni contrato, y `check-release`.
- **Solo en iPhone:**
  - Que se vea que el ámbar «se cierra» y que se entienda como «se acaba la ventana para tocar».
  - Que el pulso del blanco se note sin molestar y que se detenga claramente al converger.
  - Que el salto del ámbar ya elegido de vuelta al radio exterior no confunda.
  - Que la pastilla «Anillo blanco: espera el bote y golpea.» se lea en una línea y no tape la pelota.
  - Que la pista H1 entre en el banner sin cortarse.
  - El contraste del ámbar y del blanco a pleno sol.

## Riesgos

- Al elegir la volea, el anillo ámbar vuelve al radio exterior (el anillo elegido es otra oportunidad, con su propio `ImpactCue`). Es intencional, porque marca que la elección quedó tomada, pero hay que verlo en el iPhone.
- `until` sale de `ClockMap.TrySimToReal` y puede moverse en su último bit mientras el mapa del reloj crece. Como solo se usa para dibujar, no hay efecto en el juego.
