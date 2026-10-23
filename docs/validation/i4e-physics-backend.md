# I4e — validación del backend por defecto

Estado: **entrega completa para revisión de Claude**; código y regresión completos, build dev 8fb5d0c instalada, arranque nativo P2 verificado por consola y comprobación visual breve confirmada por el PO. **I4e abierto, [PR #7 lista para revisión](https://github.com/rafaallendes-boop/squash-clash/pull/7), sin fusionar.** [Decisión de backend y comparación de 18 criterios](../reviews/2026-10-23-first-ball-physics-backend-decision.md).

## TDD y regresión

[Dos RED válidos→GREEN y evidencia](evidence/2026-10-23-i4e-backend-tests.json):

- Configuración: RED 2/6 fallos (default aún P1 y P2 rechazado); GREEN completo **222/222 EditMode**, cero fallos/omitidos.
- Host: RED 3/3 fallos de arranque «Sequence contains no matching element» porque P2 aún no estaba registrado; no error CS ni cero tests. GREEN completo **63/63 PlayMode**, cero fallos/omitidos. Dos pruebas nuevas comprueban host normal P2 para juego/predicción, dos botes y predicción bit a bit, ausencia de escenas PhysX y cambio P2→P1→P2. La excepción secundaria de TearDown del RED procede del arranque incompleto, no se cuenta como otro comportamiento.

DefaultPreset.json es la única fuente de defaults y solo cambia physics.backend **0→1**. Hash nuevo **f6a394a12daf**, anterior **c86243b15a97**, confirmado por EffectiveConfig en el test de arranque. Fixtures específicos P1 conservan sus aserciones y eligen backend=0 explícitamente. Core/Simulation, Core/Spike, Runtime/Physics, ProjectSettings, plan y spec intactos desde 508d333.

## Verificación corta en iPhone

[Evidencia saneada de build y arranque](evidence/2026-10-23-i4e-native-smoke.json). Dev **0.1.0+8fb5d0c**, gitDirty=false, export limpio, Player no Development, SQUASH_DEV por extraScriptingDefines, IL2CPP Release, Xcode 27.1 (27A9275) Release, ARM64 y firma estricta verificados. BuildInfo y DefaultPreset exactos comprobados en el artefacto instalado. Perfil embebido vigente hasta **2026-10-28T14:59:31Z**, coincide con dispositivo/bundle y comprobado inmediatamente antes de instalar el 2026-10-23; no aplica renovación desde el 27. Herramientas sin cambios. Exports nuevos dev/prod pasaron `scripts/check-release.sh`; prod excluye DevPanel y puente térmico. Prod no instalado.

Instalación y lanzamiento aceptados. Captura corta de consola, cerrada expresamente tras observar arranque/display, no un fallo de app:

```
[SquashClash] boot 0.1.0+8fb5d0c dev devpanel=present development=False squashDev=True unity=6000.3.27f1
[SquashClash] ball ready backend=P2-Analytic visualColliders=0 localPhysicsScenes=0 config=f6a394a12daf predictedSteps=166
```

Cero marcadores de excepciones en la ventana de arranque observada. Se solicitó al PO abrir la build instalada y pulsar «Lanzar» en vista normal, confirmar arranque y rebote. Respondió **«si, funciona lanzar»**: comprobación corta confirmada en el contexto de esa solicitud. No se pulsó ni ejecutó la campaña física. No se toma esta comprobación como aceptación completa: esa prueba queda para T7.1.

Entrada de spec preparada en el documento de decisión para T7.3, sin escribirla ni modificar la spec. I5 no autorizado. **Se entrega PR #7 y se detiene para revisión de Claude**, sin cerrar I4e ni fusionar por cuenta propia. No se atribuye a esta confirmación una aprobación de Claude de I4e.
