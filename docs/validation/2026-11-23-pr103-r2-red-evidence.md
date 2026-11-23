# PR #103, ronda 2: evidencia del RED de las dos pruebas nuevas

23 nov 2026. Codex (CAMBIOS SOLICITADOS sobre `aae7ad1`) pidió dos pruebas que las anteriores no daban. Las dos pasan sobre el código correcto; un RED válido se obtiene **inyectando a propósito el defecto que cada una dice detectar**, sin commitear la inyección, y mostrando que las pruebas anteriores no lo ven y las nuevas fallan por aserción.

## Defectos inyectados (temporales, ya revertidos; árbol de `Runtime/` idéntico al de `aae7ad1`)

```diff
# Runtime/Rally/RallyDirector.cs, TickPointOver, tras `waited += elapsed;`
+ if (SquashClash.Runtime.Menu.FirstMatchHints.Enabled && Match != null && PointsPlayed == 3) waited -= elapsed / 4; // M1
# Runtime/Shot/ShotController.cs, Solve, antes de la línea de `authoritativeBoast`
+ if(order.Side.Exists&&boast.Solved&&!LastCausal.FellBack)LastStrokeBoast=true; // M2
```

- **M1:** con las pistas encendidas, el punto siguiente arranca más tarde, pero `WaitBetweenPoints` conserva su valor.
- **M2:** el solve de vista previa reclama la marca antes del contacto.

## Resultado bajo M1 y M2 (`scripts/test.sh playmode` con las cuatro pruebas)

| Prueba | Resultado | Mensaje |
|---|---|---|
| `NoAddedTime_OnMatchesOff` (anterior) | **Pass** | no ve M1: el paso local de la pelota en `RallyStarted` es 0 en cada saque |
| `ASolvedBoastThatIsApplied_IsTrueInsideShotResolved_AndNotBeforeTheContact` (anterior) | **Pass** | no ve M2: el contacto se aplica en el mismo `ProcessPending` |
| `NoAddedTime_EachPointStartsAtTheSameContinuousStep_OnAndOff` (nueva) | **Fail** | `the step at which each point ended and its WaitBetweenPoints`, difiere en el índice 3: esperado `8641/2.5`, real `8719/2.5` |
| `APreviewSolveBeforeTheContact_ClaimsNothing_AndTheContactClaimsItInsideShotResolved` (nueva) | **Fail** | `the preview solve claims nothing (lead=0.07, dz=-1.4, solved=True)`: esperado False, real True |

## Sobre el código correcto

Las dos nuevas pasan (2/2) y sus guardas de no-vacuidad se cumplen: la corrida con pistas encendidas mostró al menos una pista (`HintShown`), hubo órdenes aceptadas con contacto futuro (`WaitingForContact`) y al menos una fue un dos paredes resuelto y aplicado que reclamó la marca dentro de `ShotResolved`.

## Qué mide cada prueba nueva

- **Inicio de cada punto:** un contador continuo de pasos de simulación del propio arnés, desde el inicio de la corrida, registrado en cada `RallyStarted` y en cada `PointEnded`; compara con igualdad exacta pistas encendidas y apagadas con las mismas entradas (semilla, nivel, estilo fijo, sigma del piloto, ventana forzada), e incluye `PointResult.Reception`.
- **Antes del contacto:** una orden de dos paredes soltada un poco antes (dentro de la ventana válida) queda aceptada y programada; se exige `WaitingForContact`, ningún `ShotResolved` y marca falsa; después se avanza y se exige la marca verdadera dentro de `ShotResolved` cuando el solve autoritativo usó el solver de dos paredes sin caer al respaldo.
