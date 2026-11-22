# Front 2 — Pistas del primer partido (volea y dos paredes)

22 nov 2026 · Base `origin/main` `518f9c3`. Spec de Claude (arquitecto, modo product lead); revisión no independiente. Hay como máximo 3 pistas. Cada una sale una sola vez y solo en el primer partido. Usan el aviso entre puntos que ya existe y no agregan pasos ni tiempo antes de jugar.

## 0. Lo que ya existe (código)

- **Aviso entre puntos.** `RallyHud.cs:112-119,291-308` se construye una vez por punto terminado. Solo se ve en `RallyPhase.PointOver`, con la pelota muerta, y se oculta apenas se reanuda. Tiene título, causa, «Saca …» y, con el interruptor DEV «Saque nuevo», la línea `serveHint`.
- **Pausa entre puntos.** `MatchPauseSeconds` vale 2,5 s (`RallyParams.cs:32`), se puede saltar con «Seguir» (`RallyDirector.cs:139`) y **no cambia**.
- **Primer partido.** `FirstRunFlow.StartMatch` (`FirstRunFlow.cs:450-456`) empieza el partido desde el menú y deja `hud.MenuMatch` en verdadero.
- **Datos para los disparadores**, todos de solo lectura:
  - `ShotController.VolleyShown` (`:62`): el anillo ámbar está dibujado.
  - `LastOpportunity` (`:60`): si el golpe fue volea.
  - `SideAimOpen` (`:84`): la lateral está disponible.
  - `ShotResolved` (`:104`).
  - La marca `LastStrokeBoast` de la spec 3 ([front 3](2026-11-22-front-3-result-strokes.md)). La introduce la rama que se fusione primero y la otra la reutiliza.

## 1. Alcance

**Elegibilidad.** Es elegible el primer partido iniciado desde el menú después de instalar esta función: la clave `sc.hints.match` no existe al empezar. Al empezar, la clave se escribe enseguida, así que el partido siguiente ya no es elegible aunque este se abandone. Quedan fuera:

- la revancha;
- el «Partido» DEV;
- la práctica;
- cualquier partido con piloto automático.

**Pistas.** Cada pista tiene su clave en `PlayerPrefs` y se marca como vista cuando el aviso que la lleva llega a opacidad completa (`BannerInSeconds`, 0,18 s):

| Id | Disparador (evento del punto que acaba de terminar, sin umbrales) | Texto | Clave |
|---|---|---|---|
| H1 Volea | En una recepción del jugador se dibujó el anillo ámbar (`VolleyShown`) y el golpe de esa recepción no fue volea (al bote, fallido o sin golpe). | «Anillo ámbar: toca y volea antes del bote.» | `sc.hints.volley` |
| H2 Dos paredes | El jugador aplicó al menos un golpe con `SideAimOpen` verdadero. En el partido todavía no hizo ningún dos paredes. | «Toca la pared lateral para jugar un dos paredes.» | `sc.hints.boast` |
| H3 Cruce | En el punto, el jugador hizo su primer dos paredes del partido (`LastStrokeBoast`). | «Lateral cerca de ti: la pelota cruza al otro lado.» | `sc.hints.boastCross` |

**Reglas de turno.**

- Un disparador **arma** su pista, y la pista armada espera a la próxima pausa elegible.
- Sale **una pista por pausa**, en el orden de prioridad H1, H2, H3.
- Una pista vista no se vuelve a armar.
- No sale ninguna pista:
  - en `MatchOver`, porque ahí está la tarjeta;
  - en una pausa en que se ve `serveHint`;
  - después de que el partido elegible termina o se abandona.

**Dónde vive el código.**

- `HintPicker`, una clase pura en `Core/Rally`. Recibe los hechos del punto (cuatro booleanos) y el conjunto de pistas vistas, y devuelve la pista de esta pausa o ninguna.
- Un adaptador en `Runtime/Menu` que lee y escribe `PlayerPrefs`. Va junto a `PersonalRecords`.
- Un interruptor DEV estático `FirstMatchHints.Enabled`, por defecto encendido y que no se recuerda (precedente: `PersonalRecords.Enabled`). Apagado, no se escribe ni se muestra nada.

**Textos.** Salen del tutorial ya aprobado (`FirstRunFlow.cs:52-53`), acortados para leerse en menos de 2,5 s. El PO puede ajustarlos sin tocar la lógica.

## 2. Fuera de alcance

- Pistas de saque, de cansancio, de timing u otras.
- Un segundo partido con pistas, o la opción de volver a verlas desde el menú.
- Un tutorial interactivo o cualquier pausa extra. No se alarga `MatchPauseSeconds`.
- **Propuesta, no se implementa sin el «sí» del PO:** que H2 salga solo cuando el jugador golpeó desde el cuarto trasero de la cancha, donde el dos paredes es la salida natural del squash. Necesita un umbral nuevo de profundidad (`z ≥ X m`) que no existe en el producto.

## 3. Criterio de aceptación

1. En un partido elegible, después de un punto con el anillo ámbar visto y sin volea, el aviso de esa pausa muestra `rally-banner-hint` con el texto de H1, y `sc.hints.volley` queda en 1.
2. **Nunca en la ventana de golpe.** Recorriendo un partido completo cuadro a cuadro, `rally-banner-hint` está oculto en todo cuadro con `Phase != PointOver`.
3. Como máximo una pista por pausa y 3 por instalación. Ninguna se repite en el partido siguiente, en la revancha ni tras reiniciar la escena con las mismas `PlayerPrefs`.
4. En un partido no elegible (con `sc.hints.match` presente, en la revancha o con piloto automático) no aparece ninguna pista.
5. **Sin tiempo agregado.** Con la misma semilla y con las pistas encendidas o apagadas, la secuencia de puntos y el instante de inicio de cada punto son idénticos.
6. Con `serveHint` visible no sale ninguna pista. En `MatchOver` tampoco.
7. Con la pista, el aviso cabe en el área segura en todos los perfiles de `ZDeviceLayoutTests`, con el texto en dos líneas como máximo y sin elipsis.
8. EditMode y PlayMode completos en verde en el Mac del PO, y `check-release` OK. AutoQA (PASS/0/1) es obligatorio porque los disparadores se suscriben a la ruta de resolución del golpe. Hay que mostrar que no cambia ninguna decisión.

## 4. Tests RED primero (Codex, TDD)

- **EditMode, `Rally/HintPickerTests`:**
  - cada disparador arma su pista;
  - la prioridad es H1, H2, H3;
  - sale una sola pista por pausa;
  - la pista armada pasa a la pausa siguiente;
  - una pista vista no vuelve;
  - nunca hay más de 3;
  - no sale nada en `MatchOver`, con `serveHint` ni fuera de un partido elegible.
- **PlayMode, `ZFirstMatchHintsTests`** (sobre `ZFirstRunFixture`):
  - criterios 1 a 6, con escenas de volea v2 (`ZVolleyV2Tests`) y de dos paredes (`ZBoastTests`) como base;
  - el criterio 2 se comprueba cuadro a cuadro durante un partido entero con piloto automático y la elegibilidad forzada por el test.
- **PlayMode, `ZDeviceLayoutTests`:** el criterio 7, con la pista más larga.
- **PlayMode, `ZUiLookTests`:**
  - el aro de H1 tiene el color de `ShotController.VolleyTint`;
  - el texto usa Rubik Regular.
- **Fixture:** `ZFirstRunFixture.ClearPrefs` (`ZFirstRunFixture.cs:78`) borra `sc.hints.*`. Los tests existentes de aviso y partido siguen en verde sin editarse. Si alguno cambia por la pista, se apaga `FirstMatchHints.Enabled` en ese test y se declara.

## 5. Sin iPhone y con iPhone

- **Sin iPhone, en el Editor:** todo §3.
- **Con iPhone, en la próxima instalación autorizada:**
  - si cada pista se lee en la pausa real de 2,5 s;
  - si se nota que el aro ámbar del aviso es el mismo anillo de la cancha;
  - si el tester entiende qué hacer en el punto siguiente.
- El teléfono del PO ya jugó partidos, pero `sc.hints.match` no existe ahí, así que su próximo partido desde el menú será el elegible. Para repetir la prueba hay que borrar la app o las `PlayerPrefs`. No se agrega un botón DEV.

## 6. Dirección visual

Pasada de `/frontend-design`.

- **Plan.** La pista es una fila más dentro del aviso de vidrio `.sc-banner`, debajo de «Saca …». Va separada por un filete de 1 px `--glass-edge`. No hay tarjeta nueva, ni capa nueva, ni movimiento propio: entra con el aviso.
- **Tokens:**
  - `.sc-banner-hint`: fila, `align-items: center`, `margin-top: 8px`, `padding-top: 8px`, `border-top-width: 1px` con `--glass-edge` y `max-width: 280px`;
  - texto en Rubik Regular 14, `--glass-text`, `white-space: normal`;
  - token nuevo `--volley: rgb(255, 184, 41)`, igual a `ShotController.VolleyTint` (1; 0,72; 0,16).
- **Lo memorable.** Cada pista lleva la miniatura de la señal que enseña:
  - H1, un aro de 14 px con borde de 2 px `--volley` y centro vacío. Es el anillo ámbar del juego, en chico.
  - H2 y H3, una esquina de 14 px con borde izquierdo y borde superior de 2 px `--glass-text`. Es la planta de lateral más frontis: el camino del dos paredes visto desde arriba.

```
   ┌──────────── vidrio ────────────┐
   │        PUNTO RIVAL             │
   │  Tú: doble bote (tarde) · 4 g  │
   │        Saca Rival (izq.)       │
   │ ────────────────────────────── │
   │  ◯  Anillo ámbar: toca y volea │
   │     antes del bote.            │
   └────── borde coral ─────────────┘
```

- **Revisión contra lo genérico.** El reflejo por defecto era un *tooltip* o *coach mark* con flecha y botón «Entendido», que suma un paso y tapa la cancha. También se descartó un ícono de bombilla o de «i», que es el lenguaje de ayuda de cualquier app y no enseña nada. La miniatura del anillo o de la esquina enseña con la misma forma que el jugador va a ver en la cancha.
