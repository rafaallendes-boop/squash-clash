# Front 1 — «¡Al aire!» pasa a «¡Temprano!»

22 nov 2026 · Base `origin/main` `518f9c3`. Spec de Claude (arquitecto, modo product lead); revisión no independiente. Meta de la semana: el primer tester de TestFlight juega un partido completo y siente squash real. Este ítem no agrega ningún paso antes de jugar.

## 0. Confirmación previa: «Al aire» cubre solo el golpe adelantado

**Resultado: sí, solo el golpe adelantado. Se puede cambiar.**

- El texto sale de un único lugar: `Assets/SquashClash/Runtime/Shot/ShotNoteView.cs:29`, `r.Result=="Failed"?(r.Fail=="Air"?"¡Al aire!":"¡Tarde!")`.
- `FailKind.Air` (`Core/Shot/ContactContract.cs:8`) se asigna en dos líneas, y las dos miden el **tiempo** de la suelta:
  - `ContactContract.cs:78`: `before-k-in`, cuando se suelta antes de que abra la ventana de contacto (con `EarlySoft` apagado; `timing.earlyPolicy` vale 0 en `DefaultPreset.json`).
  - `ContactContract.cs:89`: `outside-v` con `corrected<target`, cuando se suelta antes de la ventana V. Hoy no ocurre porque `timing.validLimit` vale 0 (U1).
- Los fallos de posición tienen su propio resultado y su propio texto. Una pelota fuera de alcance sin plan da `Unreachable` (`ContactContract.cs:74`) y muestra «No llega». Una geometría que diverge da `SystemFault` (`:84`). Los fallos tardíos (`:79`, `:83` y `:89` con `corrected>target`) dan `Late` y muestran «¡Tarde!».
- Todas las rutas pasan por `ContactContract.Decide`: bote, volea (`OpportunityContract.cs:20`), rival (`RallyContact.cs:28,46`) y capa causal (`CausalQuality.cs:161`).
- Matiz: el instante en que abre la ventana (`KIn`) depende del alcance del cuerpo. Aun así, el veredicto compara la hora de la suelta con esa ventana. «Temprano» describe bien el error.

El mismo error aparece con otra palabra en el aviso entre puntos, «Tú: doble bote (al aire)», generado en `Core/Rally/RallyText.cs:37` (`ReceptionResult.Air`, que viene de `FailKind.Air` en `RallyDirector.cs:458,505`). Entra en el alcance para que el juego diga lo mismo en los dos lugares.

## 1. Alcance

- `ShotNoteView.cs:29`: «¡Al aire!» pasa a «¡Temprano!».
- `RallyText.cs:37`: «al aire» pasa a «temprano», así que el aviso dice «doble bote (temprano)».

## 2. Fuera de alcance

- Los identificadores no cambian: `FailKind.Air`, `ShotRecord.Fail == "Air"`, `ReceptionResult.Air`, el esquema de métricas y los informes de AutoQA. Tampoco cambian los hashes, la configuración ni los umbrales.
- Las tablas de experimentos y de informes DEV (`RallySimTests.cs:182`, `AnticipationH3bExperiment.cs`) y los documentos históricos.
- El tutorial («voleas en el aire», `FirstRunFlow.cs:52`) usa «en el aire» en su sentido literal y no nombra el error.
- «Adelantado»/«Atrasado» (golpe válido) no cambian.

## 3. Criterio de aceptación

1. Un golpe fallido por adelantado (`Fail=="Air"`, por `before-k-in` o por `outside-v`) muestra exactamente «¡Temprano!» con el tono `failed`. Un fallo tardío sigue mostrando «¡Tarde!» y uno fuera de alcance, «No llega».
2. El aviso de un punto perdido por doble bote tras un golpe adelantado dice «doble bote (temprano)».
3. Ningún `.cs` de `Runtime/` o `Core/` contiene el literal «Al aire» ni «al aire».
4. EditMode y PlayMode completos en verde en el Mac del PO, y `check-release` OK. AutoQA no es obligatorio porque no se toca timing, gesto ni contrato. Se corre igual si el implementador toca algo más.

## 4. Tests RED primero (Codex, TDD)

Se cambian tres aserciones existentes a pedido del PO, con el precedente de #86. Cada una debe fallar por aserción antes de tocar el código:

- `Tests/EditMode/Rally/RallyTextTests.cs:13`: espera `"doble bote (temprano)"`.
- `Tests/PlayMode/ZUiLookTests.cs:50`: espera `"¡Temprano!"` con tono `failed`.
- `Tests/PlayMode/SwingViewTests.cs:191`: espera `note.Text == "¡Temprano!"`.

Tests nuevos:

- PlayMode, `ShotControllerTests`, junto a los casos `early` y `outside-v` (`:359-361`): en los dos casos de `Fail=="Air"` (el segundo con `timing.validLimit` 1) el texto es «¡Temprano!». Un caso `Unreachable` sigue diciendo «No llega».
- EditMode, `NoAirWordingTests`: recorre los `.cs` de `Assets/SquashClash/Runtime` y `Assets/SquashClash/Core` y falla si encuentra «Al aire» o «al aire» en un literal.

Comandos: `scripts/test.sh editmode RallyText`, `scripts/test.sh playmode ZUiLook`, `scripts/test.sh playmode SwingView`, `scripts/test.sh playmode ShotController`, y después las dos suites completas.

## 5. Sin iPhone y con iPhone

- **Sin iPhone, en el Editor:** todo lo de §3.
- **Con iPhone, en la próxima instalación autorizada:** que «¡Temprano!» se lea completa bajo la cancha durante el pop (escala 1,3) sin cortarse en el teléfono del PO. Es una palabra un carácter más larga que antes: unos 190 px escalados frente a unos 300 px útiles. No requiere una instalación propia.

## 6. Dirección visual

- No hay cambio visual. Es la misma nota, `.sc-shot-note--failed`: coral `rgb(255,128,112)`, Rubik Bold Italic 24, sin caja y con sombra sobre el piso, con el mismo pop de 0,22 s.
- El par «¡Temprano!»/«¡Tarde!» queda simétrico: dos palabras de tiempo, las dos con exclamación y en el mismo color. El jugador lee primero el eje del error (antes o después) y no un estado de la pelota.
- Opción descartada: diferenciar temprano y tarde con dirección, por ejemplo con la nota desplazándose a la izquierda o a la derecha. El PO pidió solo texto, y una segunda señal visual compite con el anillo. Queda como idea para después, sin compromiso.
